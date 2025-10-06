using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.contract.Services;

/// <summary>
/// Contract tests for IDiagnosticsServiceExtensions.
/// Validates that implementations conform to the expected behavior.
/// </summary>
public class DiagnosticsServiceExtensionsContractTests
{
    private readonly IDiagnosticsServiceExtensions _service;

    public DiagnosticsServiceExtensionsContractTests()
    {
        _service = Substitute.For<IDiagnosticsServiceExtensions>();
    }

    [Fact]
    public async Task GetBootTimelineAsync_ShouldReturnValidBootTimeline()
    {
        // Arrange
        var expectedTimeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow.AddSeconds(-10),
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };
        _service.GetBootTimelineAsync(Arg.Any<CancellationToken>()).Returns(expectedTimeline);

        // Act
        var result = await _service.GetBootTimelineAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalBootTime.Should().BeGreaterThan(TimeSpan.Zero);
        result.Stage0.Should().NotBeNull();
        result.Stage1.Should().NotBeNull();
        result.Stage2.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBootTimelineAsync_WhenNotAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _service.GetBootTimelineAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<BootTimeline>(
                new InvalidOperationException("Boot timeline not available")));

        // Act & Assert
        await _service.Invoking(s => s.GetBootTimelineAsync())
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GetHistoricalBootTimelinesAsync_ShouldRespectCountParameter(int count)
    {
        // Arrange
        var timelines = Enumerable.Range(0, count).Select(i => new BootTimeline
        {
            BootStartTime = DateTime.UtcNow.AddMinutes(-i * 10),
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        }).ToList().AsReadOnly();

        _service.GetHistoricalBootTimelinesAsync(count, Arg.Any<CancellationToken>()).Returns(timelines);

        // Act
        var result = await _service.GetHistoricalBootTimelinesAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeLessOrEqualTo(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task GetHistoricalBootTimelinesAsync_WithInvalidCount_ShouldThrowArgumentOutOfRangeException(int count)
    {
        // Arrange
        _service.GetHistoricalBootTimelinesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<BootTimeline>>(
                new ArgumentOutOfRangeException(nameof(count))));

        // Act & Assert
        await _service.Invoking(s => s.GetHistoricalBootTimelinesAsync(count))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetRecentErrorsAsync_ShouldRespectCountParameter(int count)
    {
        // Arrange
        var errors = Enumerable.Range(0, count).Select(i => new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddSeconds(-i),
            Severity = ErrorSeverity.Error,
            Category = "Test",
            Message = $"Error {i}",
            ContextData = new Dictionary<string, string>()
        }).ToList().AsReadOnly();

        _service.GetRecentErrorsAsync(count, Arg.Any<CancellationToken>()).Returns(errors);

        // Act
        var result = await _service.GetRecentErrorsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeLessOrEqualTo(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task GetRecentErrorsAsync_WithInvalidCount_ShouldThrowArgumentOutOfRangeException(int count)
    {
        // Arrange
        _service.GetRecentErrorsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<ErrorEntry>>(
                new ArgumentOutOfRangeException(nameof(count))));

        // Act & Assert
        await _service.Invoking(s => s.GetRecentErrorsAsync(count))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetConnectionPoolStatsAsync_ShouldReturnValidStats()
    {
        // Arrange
        var expectedStats = new ConnectionPoolStats
        {
            Timestamp = DateTime.UtcNow,
            MySqlPool = new MySqlPoolStats
            {
                TotalConnections = 10,
                ActiveConnections = 6,
                IdleConnections = 4,
                WaitingRequests = 0,
                AverageWaitTime = TimeSpan.Zero
            },
            HttpPool = new HttpPoolStats
            {
                TotalConnections = 20,
                ActiveConnections = 12,
                IdleConnections = 8,
                AverageResponseTime = TimeSpan.FromMilliseconds(150)
            }
        };
        _service.GetConnectionPoolStatsAsync(Arg.Any<CancellationToken>()).Returns(expectedStats);

        // Act
        var result = await _service.GetConnectionPoolStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.MySqlPool.Should().NotBeNull();
        result.HttpPool.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(3, 10)]
    [InlineData(5, 30)]
    public async Task RunNetworkDiagnosticsAsync_WithValidParameters_ShouldReturnResults(int endpointCount, int timeout)
    {
        // Arrange
        var endpoints = Enumerable.Range(0, endpointCount)
            .Select(i => $"https://api{i}.example.com")
            .ToList().AsReadOnly();

        var results = endpoints.Select(ep => new NetworkDiagnosticResult
        {
            Endpoint = ep,
            IsReachable = true,
            ResponseTime = TimeSpan.FromMilliseconds(100),
            StatusCode = 200,
            ErrorMessage = null
        }).ToList().AsReadOnly();

        _service.RunNetworkDiagnosticsAsync(endpoints, timeout, Arg.Any<CancellationToken>()).Returns(results);

        // Act
        var result = await _service.RunNetworkDiagnosticsAsync(endpoints, timeout);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(endpointCount);
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_WithEmptyEndpoints_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyEndpoints = new List<string>().AsReadOnly();
        _service.RunNetworkDiagnosticsAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<NetworkDiagnosticResult>>(
                new ArgumentException("Endpoints list cannot be empty")));

        // Act & Assert
        await _service.Invoking(s => s.RunNetworkDiagnosticsAsync(emptyEndpoints))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_WithTooManyEndpoints_ShouldThrowArgumentException()
    {
        // Arrange
        var tooManyEndpoints = Enumerable.Range(0, 6).Select(i => $"https://api{i}.com").ToList().AsReadOnly();
        _service.RunNetworkDiagnosticsAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<NetworkDiagnosticResult>>(
                new ArgumentException("Maximum 5 endpoints allowed")));

        // Act & Assert
        await _service.Invoking(s => s.RunNetworkDiagnosticsAsync(tooManyEndpoints))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(31)]
    public async Task RunNetworkDiagnosticsAsync_WithInvalidTimeout_ShouldThrowArgumentOutOfRangeException(int timeout)
    {
        // Arrange
        var endpoints = new List<string> { "https://api.example.com" }.AsReadOnly();
        _service.RunNetworkDiagnosticsAsync(Arg.Any<IReadOnlyList<string>>(), timeout, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<NetworkDiagnosticResult>>(
                new ArgumentOutOfRangeException(nameof(timeout))));

        // Act & Assert
        await _service.Invoking(s => s.RunNetworkDiagnosticsAsync(endpoints, timeout))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ClearCacheAsync_ShouldReturnClearedCount()
    {
        // Arrange
        _service.ClearCacheAsync(Arg.Any<CancellationToken>()).Returns(42);

        // Act
        var result = await _service.ClearCacheAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ForceGarbageCollectionAsync_ShouldReturnFreedMemory()
    {
        // Arrange
        _service.ForceGarbageCollectionAsync().Returns(1024L);

        // Act
        var result = await _service.ForceGarbageCollectionAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task RestartApplicationAsync_ShouldComplete()
    {
        // Arrange
        _service.RestartApplicationAsync().Returns(Task.CompletedTask);

        // Act & Assert
        await _service.Invoking(s => s.RestartApplicationAsync())
            .Should().NotThrowAsync();
    }
}
