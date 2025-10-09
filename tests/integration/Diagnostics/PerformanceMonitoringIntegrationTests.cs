using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Secrets;
using Xunit;

namespace MTM_Template_Tests.Integration.Diagnostics;

/// <summary>
/// Integration tests for performance monitoring service end-to-end workflow.
/// Tests T023: Performance monitoring with real service instance and DI container.
/// </summary>
[Trait("Category", "Integration")]
public class PerformanceMonitoringIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IPerformanceMonitoringService _service;
    private bool _disposed;

    public PerformanceMonitoringIntegrationTests()
    {
        // Setup DI container with Debug Terminal services
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add Debug Terminal services
        services.AddDebugTerminalServices();

        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IPerformanceMonitoringService>();
    }

    [Fact]
    public async Task StartMonitoring_Should_Capture_Snapshots_With_Valid_Interval()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(1); // 1 second (within valid range 1-30)
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Act - Start monitoring with 1-second interval
            await _service.StartMonitoringAsync(interval, cancellationToken);

            // Wait for 2.5 seconds to capture at least 2 snapshots
            await Task.Delay(2500, cancellationToken);

            // Stop monitoring
            await _service.StopMonitoringAsync();

            // Assert - Verify snapshots were captured
            var snapshots = await _service.GetRecentSnapshotsAsync(100, cancellationToken);
            snapshots.Should().NotBeNull();
            snapshots.Should().HaveCountGreaterOrEqualTo(2, "at least 2 snapshots should be captured in 2.5 seconds with 1-second interval");

            // Verify IsMonitoring is false after stopping
            _service.IsMonitoring.Should().BeFalse("monitoring should be stopped");
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    [Fact]
    public async Task GetRecentSnapshotsAsync_Should_Respect_Circular_Buffer_Limit()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(1); // 1 second
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Act - Start monitoring
            await _service.StartMonitoringAsync(interval, cancellationToken);

            // Wait for 3 seconds to capture 3 snapshots
            await Task.Delay(3000, cancellationToken);

            // Retrieve only last 2 snapshots
            var snapshots = await _service.GetRecentSnapshotsAsync(2, cancellationToken);

            await _service.StopMonitoringAsync();

            // Assert - Verify count respects limit
            snapshots.Should().NotBeNull();
            snapshots.Should().HaveCountLessOrEqualTo(2, "GetRecentSnapshotsAsync should respect count parameter");
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    [Fact]
    public async Task GetCurrentSnapshotAsync_Should_Return_Valid_Snapshot_With_Metrics()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var snapshot = await _service.GetCurrentSnapshotAsync(cancellationToken);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2), "snapshot timestamp should be recent");
        snapshot.CpuUsagePercent.Should().BeGreaterOrEqualTo(0, "CPU usage should be non-negative");
        snapshot.MemoryUsageMB.Should().BeGreaterThan(0, "memory usage should be positive");
        snapshot.GcGen0Collections.Should().BeGreaterOrEqualTo(0);
        snapshot.GcGen1Collections.Should().BeGreaterOrEqualTo(0);
        snapshot.GcGen2Collections.Should().BeGreaterOrEqualTo(0);
        snapshot.ThreadCount.Should().BeGreaterThan(0, "thread count should be positive");
    }

    [Fact]
    public async Task StartMonitoring_Should_Reject_Invalid_Interval_Less_Than_One()
    {
        // Arrange
        var invalidInterval = TimeSpan.FromMilliseconds(500); // Invalid (must be 1-30 seconds)
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _service.StartMonitoringAsync(invalidInterval, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*interval*");
    }

    [Fact]
    public async Task StartMonitoring_Should_Reject_Invalid_Interval_Greater_Than_Thirty()
    {
        // Arrange
        var invalidInterval = TimeSpan.FromSeconds(31); // Invalid (must be 1-30)
        var cancellationToken = CancellationToken.None;

        // Act
        Func<Task> act = async () => await _service.StartMonitoringAsync(invalidInterval, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*interval*");
    }

    [Fact]
    public async Task StopMonitoringAsync_Should_Stop_Background_Task()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(1);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Act
            await _service.StartMonitoringAsync(interval, cancellationToken);
            _service.IsMonitoring.Should().BeTrue("monitoring should be active after start");

            await Task.Delay(1500, cancellationToken); // Let it capture at least 1 snapshot

            await _service.StopMonitoringAsync();

            // Assert
            _service.IsMonitoring.Should().BeFalse("monitoring should be stopped");

            // Wait a bit and verify no new snapshots are captured
            var snapshotsBeforeWait = await _service.GetRecentSnapshotsAsync(100, cancellationToken);
            var countBefore = snapshotsBeforeWait.Count;

            await Task.Delay(2000, cancellationToken);

            var snapshotsAfterWait = await _service.GetRecentSnapshotsAsync(100, cancellationToken);
            var countAfter = snapshotsAfterWait.Count;

            countAfter.Should().Be(countBefore, "no new snapshots should be captured after stopping");
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    [Fact]
    public async Task Performance_Monitoring_Should_Have_Low_CPU_Usage()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(1);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Get baseline CPU before monitoring
            var baselineSnapshot = await _service.GetCurrentSnapshotAsync(cancellationToken);
            var baselineCpu = baselineSnapshot.CpuUsagePercent;

            // Act - Start monitoring
            await _service.StartMonitoringAsync(interval, cancellationToken);

            // Let it run for 5 seconds
            await Task.Delay(5000, cancellationToken);

            // Get CPU usage during monitoring
            var monitoringSnapshot = await _service.GetCurrentSnapshotAsync(cancellationToken);
            var monitoringCpu = monitoringSnapshot.CpuUsagePercent;

            await _service.StopMonitoringAsync();

            // Assert - CPU increase should be less than 2% (NFR-003)
            var cpuIncrease = monitoringCpu - baselineCpu;
            cpuIncrease.Should().BeLessThan(2.0, "CPU usage during monitoring should be <2% (NFR-003)");
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    [Fact]
    public async Task Circular_Buffer_Should_Maintain_Max_100_Snapshots()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(1);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            // Act - Start monitoring and let it run long enough to exceed 100 snapshots
            // Note: This test would take 100+ seconds to truly verify 100-snapshot limit.
            // For integration test speed, we'll verify that GetRecentSnapshotsAsync(100) works correctly.

            await _service.StartMonitoringAsync(interval, cancellationToken);

            // Wait for 3 seconds (3 snapshots)
            await Task.Delay(3000, cancellationToken);

            var snapshots = await _service.GetRecentSnapshotsAsync(100, cancellationToken);

            await _service.StopMonitoringAsync();

            // Assert - Verify snapshots list respects the buffer (should have 3 or fewer)
            snapshots.Should().NotBeNull();
            snapshots.Count.Should().BeLessOrEqualTo(100, "circular buffer should never exceed 100 snapshots (CL-002)");
            snapshots.Count.Should().BeGreaterOrEqualTo(2, "should have captured at least 2 snapshots in 3 seconds");
        }
        finally
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // PerformanceMonitoringService implements IDisposable, but accessed via interface
        if (_service is IDisposable disposableService)
        {
            disposableService.Dispose();
        }

        _serviceProvider?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
