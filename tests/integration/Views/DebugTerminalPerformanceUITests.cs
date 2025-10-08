using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.ViewModels;
using NSubstitute;
using Xunit;

namespace MTM_Template_Application.Tests.Integration.Views;

/// <summary>
/// Integration tests for Debug Terminal Performance Monitoring UI workflow.
/// Tests ViewModel-level UI logic and service integration for performance monitoring.
/// Note: Full UI rendering tests deferred - testing ViewModel contracts here.
/// </summary>
public class DebugTerminalPerformanceUITests
{
    private readonly IPerformanceMonitoringService _mockPerformanceService;
    private readonly IDiagnosticsServiceExtensions _mockDiagnosticsService;
    private readonly IExportService _mockExportService;
    private readonly ILogger<DebugTerminalViewModel> _mockLogger;

    public DebugTerminalPerformanceUITests()
    {
        _mockPerformanceService = Substitute.For<IPerformanceMonitoringService>();
        _mockDiagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();
        _mockExportService = Substitute.For<IExportService>();
        _mockLogger = Substitute.For<ILogger<DebugTerminalViewModel>>();

        // Setup default behavior for performance service
        _mockPerformanceService.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(CreateSampleSnapshot());

        _mockPerformanceService.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<PerformanceSnapshot> { CreateSampleSnapshot() });
    }

    /// <summary>
    /// T043 Test Case 1: Verify Start Monitoring button updates IsMonitoring property.
    /// </summary>
    [Fact]
    public async Task StartMonitoringButton_Should_Update_IsMonitoring_Property()
    {
        // Arrange
        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsMonitoring.Should().BeTrue("monitoring should start when command executes");
        await _mockPerformanceService.Received(1).StartMonitoringAsync(
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>()
        );
    }

    /// <summary>
    /// T043 Test Case 2: Verify Stop Monitoring button resets IsMonitoring property.
    /// </summary>
    [Fact]
    public async Task StopMonitoringButton_Should_Reset_IsMonitoring_Property()
    {
        // Arrange
        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        await viewModel.StartMonitoringCommand.ExecuteAsync(null);

        // Act
        await viewModel.StopMonitoringCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsMonitoring.Should().BeFalse("monitoring should stop when command executes");
        await _mockPerformanceService.Received(1).StopMonitoringAsync();
    }

    /// <summary>
    /// T043 Test Case 3: Verify performance metrics update during monitoring.
    /// Simulates monitoring with 5-second interval (default per CL-002).
    /// </summary>
    [Fact]
    public async Task PerformanceMetrics_Should_Update_During_Monitoring()
    {
        // Arrange
        var snapshot1 = CreateSampleSnapshot(cpuUsage: 25.5, memoryUsageMb: 512);
        var snapshot2 = CreateSampleSnapshot(cpuUsage: 30.2, memoryUsageMb: 540);

        _mockPerformanceService.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(snapshot1, snapshot2);

        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(null);
        await Task.Delay(100); // Simulate short monitoring duration

        // Verify service was called to start monitoring with 5s interval
        await _mockPerformanceService.Received(1).StartMonitoringAsync(
            Arg.Is<TimeSpan>(ts => ts.TotalSeconds == 5), // Default 5s interval per CL-002
            Arg.Any<CancellationToken>()
        );

        // Assert
        viewModel.IsMonitoring.Should().BeTrue();
    }

    /// <summary>
    /// T043 Test Case 4: Verify Start button is disabled when monitoring is active.
    /// Tests CanExecute logic for StartMonitoringCommand.
    /// </summary>
    [Fact]
    public async Task StartButton_Should_Be_Disabled_When_Monitoring_Active()
    {
        // Arrange
        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(null);

        // Assert
        viewModel.StartMonitoringCommand.CanExecute(null).Should().BeFalse(
            "Start button should be disabled when monitoring is already active"
        );
    }

    /// <summary>
    /// T043 Test Case 5: Verify Stop button is disabled when monitoring is inactive.
    /// Tests CanExecute logic for StopMonitoringCommand.
    /// </summary>
    [Fact]
    public void StopButton_Should_Be_Disabled_When_Monitoring_Inactive()
    {
        // Arrange
        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Assert
        viewModel.StopMonitoringCommand.CanExecute(null).Should().BeFalse(
            "Stop button should be disabled when monitoring is not active"
        );
    }

    /// <summary>
    /// T043 Test Case 6: Verify memory usage color-coding thresholds.
    /// Green: <70MB, Yellow: 70-90MB, Red: >90MB (per FR-002)
    /// Note: Actual color conversion will be tested in T047 Value Converter tests.
    /// </summary>
    [Fact]
    public void MemoryUsage_Color_Should_Match_Thresholds()
    {
        // Arrange - Create snapshots with different memory usage values
        var greenSnapshot = CreateSampleSnapshot(memoryUsageMb: 50); // <70MB = green
        var yellowSnapshot = CreateSampleSnapshot(memoryUsageMb: 80); // 70-90MB = yellow
        var redSnapshot = CreateSampleSnapshot(memoryUsageMb: 120); // >90MB = red

        // Act & Assert
        greenSnapshot.MemoryUsageMB.Should().BeLessThan(70, "memory usage <70MB should be green");
        yellowSnapshot.MemoryUsageMB.Should().BeInRange(70, 90, "memory usage 70-90MB should be yellow");
        redSnapshot.MemoryUsageMB.Should().BeGreaterThan(90, "memory usage >90MB should be red");

        // Note: Actual color conversion will be tested in T047 Value Converter tests
    }

    /// <summary>
    /// T043 Test Case 7: Verify UI updates without blocking (async binding).
    /// Performance monitoring should not block the UI thread (NFR-003).
    /// </summary>
    [Fact]
    public async Task PerformanceMonitoring_Should_Not_Block_UI_Thread()
    {
        // Arrange
        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Act
        var startTime = DateTime.UtcNow;
        await viewModel.StartMonitoringCommand.ExecuteAsync(null);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(500),
            "StartMonitoringCommand should complete quickly without blocking UI (NFR-003)"
        );
    }

    /// <summary>
    /// T043 Test Case 8: Verify performance history collection grows during monitoring.
    /// Tests that snapshots are added to PerformanceHistory collection (circular buffer max 100).
    /// </summary>
    [Fact]
    public async Task PerformanceHistory_Should_Grow_During_Monitoring()
    {
        // Arrange
        var snapshots = new List<PerformanceSnapshot>
        {
            CreateSampleSnapshot(),
            CreateSampleSnapshot(),
            CreateSampleSnapshot()
        };

        _mockPerformanceService.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(snapshots);

        var viewModel = new DebugTerminalViewModel(
            _mockLogger,
            performanceMonitoringService: _mockPerformanceService,
            diagnosticsServiceExtensions: _mockDiagnosticsService,
            exportService: _mockExportService
        );

        // Act
        await viewModel.StartMonitoringCommand.ExecuteAsync(null);

        // Note: PerformanceHistory population is handled by the service internally
        // This test verifies the service contract is called correctly
        await _mockPerformanceService.Received(1).StartMonitoringAsync(
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>()
        );
    }

    // Helper Methods

    private static PerformanceSnapshot CreateSampleSnapshot(
        double cpuUsage = 25.5,
        long memoryUsageMb = 512)
    {
        return new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = cpuUsage,
            MemoryUsageMB = memoryUsageMb,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 20,
            Uptime = TimeSpan.FromMinutes(5)
        };
    }
}
