using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.ViewModels;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.unit.ViewModels;

/// <summary>
/// Unit tests for DebugTerminalViewModel diagnostic extensions (T033-T035).
/// Tests performance monitoring, boot timeline, error history commands and properties.
/// </summary>
[Trait("Category", "Unit")]
public class DebugTerminalViewModelTests
{
    private readonly ILogger<DebugTerminalViewModel> _logger;
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly IDiagnosticsServiceExtensions _diagnosticsExtensions;
    private readonly IExportService _exportService;

    public DebugTerminalViewModelTests()
    {
        _logger = Substitute.For<ILogger<DebugTerminalViewModel>>();
        _performanceService = Substitute.For<IPerformanceMonitoringService>();
        _diagnosticsExtensions = Substitute.For<IDiagnosticsServiceExtensions>();
        _exportService = Substitute.For<IExportService>();
    }

    private DebugTerminalViewModel CreateViewModel()
    {
        return new DebugTerminalViewModel(
            _logger,
            performanceMonitoringService: _performanceService,
            diagnosticsServiceExtensions: _diagnosticsExtensions,
            exportService: _exportService
        );
    }

    #region T033: Performance Monitoring Tests

    [Fact]
    public void Constructor_Should_Initialize_Performance_Properties()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.CurrentPerformance.Should().BeNull("no monitoring started yet");
        viewModel.PerformanceHistory.Should().NotBeNull().And.BeEmpty();
        viewModel.IsMonitoring.Should().BeFalse();
        viewModel.CanToggleMonitoring.Should().BeTrue();
    }

    [Fact]
    public async Task StartMonitoringCommand_Should_Update_IsMonitoring_Property()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        // Setup mock to not throw
        _performanceService.StartMonitoringAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(cts.Token);

        // Assert
        viewModel.IsMonitoring.Should().BeTrue();
        await _performanceService.Received(1).StartMonitoringAsync(
            Arg.Is<TimeSpan>(t => t.TotalSeconds == 5),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task StopMonitoringCommand_Should_Reset_IsMonitoring_Property()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        _performanceService.StartMonitoringAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _performanceService.StopMonitoringAsync().Returns(Task.CompletedTask);

        // Start monitoring first
        await viewModel.StartMonitoringCommand.ExecuteAsync(cts.Token);
        viewModel.IsMonitoring.Should().BeTrue("monitoring was started");

        // Act
        await viewModel.StopMonitoringCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsMonitoring.Should().BeFalse();
        await _performanceService.Received(1).StopMonitoringAsync();
    }

    [Fact]
    public void StartMonitoringCommand_CanExecute_Should_Be_False_When_Already_Monitoring()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act - simulate IsMonitoring = true
        viewModel.IsMonitoring = true;

        // Assert
        viewModel.StartMonitoringCommand.CanExecute(null).Should().BeFalse(
            "cannot start monitoring when already monitoring"
        );
    }

    [Fact]
    public void StopMonitoringCommand_CanExecute_Should_Be_False_When_Not_Monitoring()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act - IsMonitoring is false by default
        // Assert
        viewModel.StopMonitoringCommand.CanExecute(null).Should().BeFalse(
            "cannot stop monitoring when not monitoring"
        );
    }

    [Fact]
    public async Task StartMonitoringCommand_Should_Handle_Service_Exceptions()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        _performanceService.StartMonitoringAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Service error")));

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(cts.Token);

        // Assert - should not throw, IsMonitoring should be false
        viewModel.IsMonitoring.Should().BeFalse("error occurred during start");
    }

    #endregion

    #region T034: Boot Timeline Tests

    [Fact]
    public void Constructor_Should_Initialize_BootTimeline_Properties()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.CurrentBootTimeline.Should().BeNull("no timeline loaded yet");
        viewModel.HistoricalBootTimelines.Should().NotBeNull().And.BeEmpty();
        viewModel.TotalBootTime.Should().Be(TimeSpan.Zero);
        viewModel.SlowestStage.Should().BeNull();
    }

    [Fact]
    public async Task RefreshBootTimelineCommand_Should_Load_Timeline_And_Calculate_Totals()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        var mockTimeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Success = true
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(2500),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(800),
                Success = true
            },
            TotalBootTime = TimeSpan.FromMilliseconds(3800)
        };

        _diagnosticsExtensions.GetBootTimelineAsync(Arg.Any<CancellationToken>())
            .Returns(mockTimeline);

        // Act
        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(cts.Token);

        // Assert
        viewModel.CurrentBootTimeline.Should().NotBeNull().And.Be(mockTimeline);
        viewModel.TotalBootTime.TotalMilliseconds.Should().Be(3800);
        viewModel.SlowestStage.Should().Contain("Stage 1").And.Contain("2500ms");
    }

    [Fact]
    public async Task RefreshBootTimelineCommand_Should_Handle_Service_Exceptions()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        _diagnosticsExtensions.GetBootTimelineAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BootTimeline>(new InvalidOperationException("Timeline unavailable")));

        // Act
        await viewModel.RefreshBootTimelineCommand.ExecuteAsync(cts.Token);

        // Assert - should not throw
        viewModel.CurrentBootTimeline.Should().BeNull("error occurred during refresh");
    }

    #endregion

    #region T035: Error History Tests

    [Fact]
    public void Constructor_Should_Initialize_ErrorHistory_Properties()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.RecentErrors.Should().NotBeNull().And.BeEmpty();
        viewModel.ErrorCount.Should().Be(0);
        viewModel.SelectedSeverityFilter.Should().Be(ErrorSeverity.Error);
    }

    [Fact]
    public async Task ClearErrorHistoryCommand_Should_Clear_Errors()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        // Add some errors manually
        viewModel.RecentErrors.Add(new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Test",
            Message = "Test error",
            ContextData = new Dictionary<string, string>()
        });
        viewModel.ErrorCount = 1;

        // Act
        await viewModel.ClearErrorHistoryCommand.ExecuteAsync(cts.Token);

        // Assert
        viewModel.RecentErrors.Should().BeEmpty();
        viewModel.ErrorCount.Should().Be(0);
    }

    [Fact]
    public async Task FilterErrorsBySeverityCommand_Should_Filter_Errors_By_Severity()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        var allErrors = new List<ErrorEntry>
        {
            new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Info,
                Category = "Info",
                Message = "Info message",
                ContextData = new Dictionary<string, string>()
            },
            new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Warning,
                Category = "Warning",
                Message = "Warning message",
                ContextData = new Dictionary<string, string>()
            },
            new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Error,
                Category = "Error",
                Message = "Error message",
                ContextData = new Dictionary<string, string>()
            },
            new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Critical,
                Category = "Critical",
                Message = "Critical error",
                ContextData = new Dictionary<string, string>()
            }
        };

        _diagnosticsExtensions.GetRecentErrorsAsync(100, Arg.Any<CancellationToken>())
            .Returns(allErrors);

        // Act - Filter by Warning severity (should get Warning, Error, Critical)
        await viewModel.FilterErrorsBySeverityCommand.ExecuteAsync(ErrorSeverity.Warning);

        // Assert
        viewModel.SelectedSeverityFilter.Should().Be(ErrorSeverity.Warning);
        viewModel.RecentErrors.Should().HaveCount(3, "Warning, Error, and Critical match the filter");
        viewModel.RecentErrors.Should().NotContain(e => e.Severity == ErrorSeverity.Info);
        viewModel.ErrorCount.Should().Be(3);
    }

    [Fact]
    public async Task FilterErrorsBySeverityCommand_Should_Handle_Service_Exceptions()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        _diagnosticsExtensions.GetRecentErrorsAsync(100, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<ErrorEntry>>(
                new InvalidOperationException("Error service unavailable")));

        // Act
        await viewModel.FilterErrorsBySeverityCommand.ExecuteAsync(ErrorSeverity.Error);

        // Assert - should not throw
        viewModel.RecentErrors.Should().BeEmpty("error occurred during filter");
    }

    #endregion

    #region Quick Actions Tests

    [Fact]
    public async Task ExportDiagnosticsCommand_Should_Call_ExportService()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        _exportService.ExportToJsonAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<IProgress<int>?>())
            .Returns(1024L); // 1KB written

        // Act
        await viewModel.ExportDiagnosticsCommand.ExecuteAsync(cts.Token);

        // Assert
        await _exportService.Received(1).ExportToJsonAsync(
            Arg.Is<string>(path => path.Contains("diagnostics-export-") && path.EndsWith(".json")),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<int>?>()
        );
    }

    [Fact]
    public async Task RefreshAllDataCommand_Should_Complete_Successfully()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var cts = new CancellationTokenSource();

        // Act
        await viewModel.RefreshAllDataCommand.ExecuteAsync(cts.Token);

        // Assert - should not throw
        // Command calls LoadDiagnostics() which queries all injected services
        // Since all services are mocked with default behavior, this should complete
    }

    #endregion
}
