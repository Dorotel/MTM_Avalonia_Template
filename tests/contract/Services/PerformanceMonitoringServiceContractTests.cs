using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.contract.Services;

/// <summary>
/// Contract tests for IPerformanceMonitoringService.
/// Validates that implementations conform to the expected behavior.
/// </summary>
public class PerformanceMonitoringServiceContractTests
{
    private readonly IPerformanceMonitoringService _service;

    public PerformanceMonitoringServiceContractTests()
    {
        _service = Substitute.For<IPerformanceMonitoringService>();
    }

    [Fact]
    public async Task GetCurrentSnapshotAsync_ShouldReturnValidSnapshot()
    {
        // Arrange
        var expectedSnapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };
        _service.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>()).Returns(expectedSnapshot);

        // Act
        var result = await _service.GetCurrentSnapshotAsync();

        // Assert
        result.Should().NotBeNull();
        result.CpuUsagePercent.Should().BeInRange(0, 100);
        result.MemoryUsageMB.Should().BeGreaterThanOrEqualTo(0);
        result.ThreadCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCurrentSnapshotAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        _service.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromCanceled<PerformanceSnapshot>(cts.Token));

        // Act & Assert
        await _service.Invoking(s => s.GetCurrentSnapshotAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetRecentSnapshotsAsync_ShouldRespectCountParameter(int count)
    {
        // Arrange
        var snapshots = Enumerable.Range(0, count).Select(i => new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow.AddSeconds(-i),
            CpuUsagePercent = 50 + i,
            MemoryUsageMB = 100 + i,
            GcGen0Collections = i,
            GcGen1Collections = i / 2,
            GcGen2Collections = i / 5,
            ThreadCount = 10 + i,
            Uptime = TimeSpan.FromMinutes(i)
        }).ToList().AsReadOnly();

        _service.GetRecentSnapshotsAsync(count, Arg.Any<CancellationToken>()).Returns(snapshots);

        // Act
        var result = await _service.GetRecentSnapshotsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeLessOrEqualTo(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetRecentSnapshotsAsync_WithInvalidCount_ShouldThrowArgumentOutOfRangeException(int count)
    {
        // Arrange
        _service.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<PerformanceSnapshot>>(
                new ArgumentOutOfRangeException(nameof(count))));

        // Act & Assert
        await _service.Invoking(s => s.GetRecentSnapshotsAsync(count))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(30)]
    public async Task StartMonitoringAsync_WithValidInterval_ShouldStart(int seconds)
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(seconds);
        _service.StartMonitoringAsync(interval, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _service.IsMonitoring.Returns(false, true);

        // Act
        await _service.StartMonitoringAsync(interval);

        // Assert
        await _service.Received(1).StartMonitoringAsync(interval, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    [InlineData(60)]
    public async Task StartMonitoringAsync_WithInvalidInterval_ShouldThrowArgumentOutOfRangeException(int seconds)
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(seconds);
        _service.StartMonitoringAsync(interval, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ArgumentOutOfRangeException(nameof(interval))));

        // Act & Assert
        await _service.Invoking(s => s.StartMonitoringAsync(interval))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task StartMonitoringAsync_WhenAlreadyMonitoring_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _service.IsMonitoring.Returns(true);
        _service.StartMonitoringAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Monitoring already started")));

        // Act & Assert
        await _service.Invoking(s => s.StartMonitoringAsync(TimeSpan.FromSeconds(5)))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StopMonitoringAsync_ShouldStopMonitoring()
    {
        // Arrange
        _service.StopMonitoringAsync().Returns(Task.CompletedTask);
        _service.IsMonitoring.Returns(true, false);

        // Act
        await _service.StopMonitoringAsync();

        // Assert
        await _service.Received(1).StopMonitoringAsync();
    }

    [Fact]
    public void IsMonitoring_ShouldReflectMonitoringState()
    {
        // Arrange
        _service.IsMonitoring.Returns(false);

        // Act
        var isMonitoring = _service.IsMonitoring;

        // Assert
        isMonitoring.Should().BeFalse();
    }

    [Fact]
    public async Task MonitoringWorkflow_StartStopCycle_ShouldWorkCorrectly()
    {
        // Arrange
        var interval = TimeSpan.FromSeconds(5);
        _service.IsMonitoring.Returns(false, true, false);
        _service.StartMonitoringAsync(interval, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _service.StopMonitoringAsync().Returns(Task.CompletedTask);

        // Act - Start monitoring
        await _service.StartMonitoringAsync(interval);

        // Assert - Should be monitoring
        _service.IsMonitoring.Returns(true);

        // Act - Stop monitoring
        await _service.StopMonitoringAsync();

        // Assert - Should not be monitoring
        _service.IsMonitoring.Returns(false);
    }
}
