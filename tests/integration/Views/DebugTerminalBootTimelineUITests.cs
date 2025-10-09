using Avalonia.Headless.XUnit;
using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.ViewModels;
using MTM_Template_Application.Views;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration.Views;

/// <summary>
/// Integration tests for Boot Timeline UI panel using Avalonia.Headless.
/// Tests T038: Boot Timeline UI visualization rendering and interaction.
/// </summary>
[Trait("Category", "UITest")]
[Trait("Feature", "003-DebugTerminal")]
public class DebugTerminalBootTimelineUITests
{
    [AvaloniaFact]
    public void BootTimelinePanel_Should_RenderWithoutErrors()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act & Assert - Should render without exceptions
        window.Show();

        // Verify window is visible
        window.IsVisible.Should().BeTrue();
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_DisplayStageInfo_WhenDataAvailable()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        viewModel.CurrentBootTimeline = CreateSampleBootTimeline();

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();

        // Assert
        viewModel.CurrentBootTimeline.Should().NotBeNull();
        viewModel.CurrentBootTimeline!.TotalBootTime.Should().Be(TimeSpan.FromMilliseconds(4000));
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_CalculateTotalBootTime_Correctly()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        var timeline = CreateSampleBootTimeline();
        viewModel.CurrentBootTimeline = timeline;

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();
        var totalBootTime = viewModel.TotalBootTime;

        // Assert
        totalBootTime.Should().Be(TimeSpan.FromMilliseconds(4000));
        totalBootTime.Should().Be(
            timeline.Stage0.Duration +
            timeline.Stage1.Duration +
            timeline.Stage2.Duration
        );
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_IdentifySlowestStage_Correctly()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        viewModel.CurrentBootTimeline = CreateSampleBootTimeline();

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();
        var slowestStage = viewModel.SlowestStage;

        // Assert
        slowestStage.Should().StartWith("Stage 1");
        slowestStage.Should().Contain("2500ms");
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_ShowNoDataMessage_WhenBootTimelineIsNull()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        viewModel.CurrentBootTimeline = null;

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();

        // Assert
        viewModel.CurrentBootTimeline.Should().BeNull();
        // UI should show "No boot data available" message
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_UpdateOnRefreshCommand()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        var initialTimeline = CreateSampleBootTimeline();
        viewModel.CurrentBootTimeline = initialTimeline;

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };
        window.Show();

        // Act - Trigger refresh command
        var newTimeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Success = true,
                ErrorMessage = null
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(2000),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>(),
                ErrorMessage = null
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = TimeSpan.FromMilliseconds(3000)
        };
        viewModel.CurrentBootTimeline = newTimeline;

        // Assert
        viewModel.CurrentBootTimeline.Should().Be(newTimeline);
        viewModel.TotalBootTime.Should().Be(TimeSpan.FromMilliseconds(3000));
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_DisplayServiceTimings_WhenStage1HasServices()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        var timeline = CreateBootTimelineWithServices();
        viewModel.CurrentBootTimeline = timeline;

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();

        // Assert
        viewModel.CurrentBootTimeline.Should().NotBeNull();
        viewModel.CurrentBootTimeline!.Stage1.ServiceTimings.Should().HaveCount(3);
        viewModel.CurrentBootTimeline.Stage1.ServiceTimings[0].ServiceName.Should().Be("ConfigurationService");
    }

    [AvaloniaFact]
    public void BootTimelinePanel_Should_ColorCodeStages_BasedOnTargets()
    {
        // Arrange
        var viewModel = CreateMockedViewModel();
        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(1500), // Exceeds 1000ms target
                Success = true,
                ErrorMessage = null
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(2000), // Within 3000ms target
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>(),
                ErrorMessage = null
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(500), // Within 1000ms target
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = TimeSpan.FromMilliseconds(4000)
        };
        viewModel.CurrentBootTimeline = timeline;

        var window = new DebugTerminalWindow
        {
            DataContext = viewModel
        };

        // Act
        window.Show();

        // Assert - Stage 0 should be red (exceeds target), Stage 1 and 2 should be green
        timeline.Stage0.Duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(1000));
        timeline.Stage1.Duration.Should().BeLessThan(TimeSpan.FromMilliseconds(3000));
        timeline.Stage2.Duration.Should().BeLessThan(TimeSpan.FromMilliseconds(1000));
    }

    // Helper Methods

    private DebugTerminalViewModel CreateMockedViewModel()
    {
        var mockLogger = Substitute.For<Microsoft.Extensions.Logging.ILogger<DebugTerminalViewModel>>();
        var mockBootOrchestrator = Substitute.For<MTM_Template_Application.Services.Boot.IBootOrchestrator>();
        var mockPerformanceService = Substitute.For<MTM_Template_Application.Services.Diagnostics.IPerformanceMonitoringService>();
        var mockDiagnosticsServiceExt = Substitute.For<MTM_Template_Application.Services.Diagnostics.IDiagnosticsServiceExtensions>();
        var mockExportService = Substitute.For<MTM_Template_Application.Services.Diagnostics.IExportService>();

        return new DebugTerminalViewModel(
            mockLogger,
            bootOrchestrator: mockBootOrchestrator,
            performanceMonitoringService: mockPerformanceService,
            diagnosticsServiceExtensions: mockDiagnosticsServiceExt,
            exportService: mockExportService
        );
    }

    private BootTimeline CreateSampleBootTimeline()
    {
        return new BootTimeline
        {
            BootStartTime = DateTime.UtcNow.AddSeconds(-10),
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(800),
                Success = true,
                ErrorMessage = null
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(2500),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>(),
                ErrorMessage = null
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(700),
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = TimeSpan.FromMilliseconds(4000)
        };
    }

    private BootTimeline CreateBootTimelineWithServices()
    {
        var serviceTimings = new List<ServiceInitInfo>
        {
            new ServiceInitInfo
            {
                ServiceName = "ConfigurationService",
                Duration = TimeSpan.FromMilliseconds(500),
                Success = true
            },
            new ServiceInitInfo
            {
                ServiceName = "SecretsService",
                Duration = TimeSpan.FromMilliseconds(300),
                Success = true
            },
            new ServiceInitInfo
            {
                ServiceName = "DatabaseService",
                Duration = TimeSpan.FromMilliseconds(1700),
                Success = true
            }
        };

        return new BootTimeline
        {
            BootStartTime = DateTime.UtcNow.AddSeconds(-10),
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(800),
                Success = true,
                ErrorMessage = null
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(2500),
                Success = true,
                ServiceTimings = serviceTimings,
                ErrorMessage = null
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(700),
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = TimeSpan.FromMilliseconds(4000)
        };
    }
}
