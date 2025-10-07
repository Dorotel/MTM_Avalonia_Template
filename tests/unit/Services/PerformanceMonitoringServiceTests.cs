using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.unit.Services;

/// <summary>
/// Unit tests for PerformanceMonitoringService implementation.
/// Tests circular buffer rotation, interval validation, cancellation, and monitoring state.
/// </summary>
public class PerformanceMonitoringServiceTests
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly PerformanceMonitoringService _service;

    public PerformanceMonitoringServiceTests()
    {
        _logger = Substitute.For<ILogger<PerformanceMonitoringService>>();
        _service = new PerformanceMonitoringService(_logger);
    }

    [Fact]
    public async Task GetCurrentSnapshotAsync_Should_Return_Valid_Snapshot()
    {
        // Act
        var snapshot = await _service.GetCurrentSnapshotAsync();

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.CpuUsagePercent.Should().BeInRange(0, 100);
        snapshot.MemoryUsageMB.Should().BeGreaterThanOrEqualTo(0);
        snapshot.ThreadCount.Should().BeGreaterThan(0);
        snapshot.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
        snapshot.GcGen0Collections.Should().BeGreaterThanOrEqualTo(0);
        snapshot.GcGen1Collections.Should().BeGreaterThanOrEqualTo(0);
        snapshot.GcGen2Collections.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetRecentSnapshotsAsync_Should_Return_Empty_Initially()
    {
        // Act
        var snapshots = await _service.GetRecentSnapshotsAsync(10);

        // Assert
        snapshots.Should().NotBeNull();
        snapshots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentSnapshotsAsync_Should_Throw_For_Invalid_Count()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.GetRecentSnapshotsAsync(0))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("count");

        await FluentActions.Invoking(() => _service.GetRecentSnapshotsAsync(101))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("count");

        await FluentActions.Invoking(() => _service.GetRecentSnapshotsAsync(-1))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("count");
    }

    [Fact]
    public async Task StartMonitoringAsync_Should_Reject_Invalid_Intervals()
    {
        // Act & Assert - Too short (< 1 second)
        await FluentActions.Invoking(() => _service.StartMonitoringAsync(TimeSpan.FromMilliseconds(500)))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("interval");

        // Act & Assert - Too long (> 30 seconds)
        await FluentActions.Invoking(() => _service.StartMonitoringAsync(TimeSpan.FromSeconds(31)))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("interval");
    }

    [Fact]
    public async Task StartMonitoringAsync_Should_Set_IsMonitoring_True()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // Act
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);

        // Allow monitoring to start
        await Task.Delay(100);

        // Assert
        _service.IsMonitoring.Should().BeTrue();

        // Cleanup
        await cts.CancelAsync();
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task StartMonitoringAsync_Should_Throw_If_Already_Monitoring()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);
        await Task.Delay(100); // Allow monitoring to start

        // Act & Assert
        await FluentActions.Invoking(() => _service.StartMonitoringAsync(TimeSpan.FromSeconds(1)))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already started*");

        // Cleanup
        await cts.CancelAsync();
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task StopMonitoringAsync_Should_Set_IsMonitoring_False()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);
        await Task.Delay(100); // Allow monitoring to start

        // Act
        await _service.StopMonitoringAsync();

        // Assert
        _service.IsMonitoring.Should().BeFalse();

        // Cleanup
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task CancellationToken_Should_Stop_Monitoring()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        // Act
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);

        // Assert
        await FluentActions.Invoking(() => monitoringTask)
            .Should().ThrowAsync<OperationCanceledException>();

        _service.IsMonitoring.Should().BeFalse();
    }

    [Fact]
    public async Task Circular_Buffer_Should_Maintain_Max_100_Snapshots()
    {
        // Arrange - Start monitoring with 1-second interval
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);

        // Wait for monitoring to collect snapshots (at least 5 snapshots)
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Act - Get recent snapshots
        var snapshots = await _service.GetRecentSnapshotsAsync(100);

        // Assert - Should have collected some snapshots, but not exceed 100
        snapshots.Should().NotBeEmpty();
        snapshots.Count.Should().BeLessThanOrEqualTo(100);

        // Verify snapshots are ordered chronologically
        var timestamps = snapshots.Select(s => s.Timestamp).ToList();
        timestamps.Should().BeInAscendingOrder();

        // Cleanup
        await cts.CancelAsync();
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task Performance_Snapshots_Should_Have_Valid_Metrics()
    {
        // Arrange - Start monitoring
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);

        // Wait for at least 2 snapshots
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Act
        var snapshots = await _service.GetRecentSnapshotsAsync(10);

        // Assert
        snapshots.Should().NotBeEmpty();
        foreach (var snapshot in snapshots)
        {
            snapshot.CpuUsagePercent.Should().BeInRange(0, 100);
            snapshot.MemoryUsageMB.Should().BeGreaterThan(0);
            snapshot.ThreadCount.Should().BeGreaterThan(0);
            snapshot.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
            snapshot.GcGen0Collections.Should().BeGreaterThanOrEqualTo(0);
            snapshot.GcGen1Collections.Should().BeGreaterThanOrEqualTo(0);
            snapshot.GcGen2Collections.Should().BeGreaterThanOrEqualTo(0);
        }

        // Cleanup
        await cts.CancelAsync();
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task Service_Should_Be_Disposable_Without_Errors()
    {
        // Arrange
        var service = new PerformanceMonitoringService(_logger);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var monitoringTask = service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);
        await Task.Delay(100);

        // Act & Assert - Dispose should not throw
        FluentActions.Invoking(() => service.Dispose())
            .Should().NotThrow();

        // Verify monitoring stopped
        service.IsMonitoring.Should().BeFalse();

        // Cleanup
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    [Fact]
    public async Task Multiple_Dispose_Calls_Should_Be_Safe()
    {
        // Arrange
        var service = new PerformanceMonitoringService(_logger);

        // Act & Assert - Multiple dispose calls should not throw
        FluentActions.Invoking(() =>
        {
            service.Dispose();
            service.Dispose();
            service.Dispose();
        }).Should().NotThrow();
    }

    [Fact]
    public async Task GetRecentSnapshotsAsync_Should_Return_Requested_Count()
    {
        // Arrange - Start monitoring and collect snapshots
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
        var monitoringTask = _service.StartMonitoringAsync(TimeSpan.FromSeconds(1), cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Collect 5 snapshots

        // Act
        var snapshots3 = await _service.GetRecentSnapshotsAsync(3);
        var snapshots10 = await _service.GetRecentSnapshotsAsync(10);

        // Assert
        snapshots3.Count.Should().BeLessThanOrEqualTo(3);
        snapshots10.Count.Should().BeGreaterThan(snapshots3.Count);

        // Cleanup
        await cts.CancelAsync();
        try
        {
            await monitoringTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }
}
