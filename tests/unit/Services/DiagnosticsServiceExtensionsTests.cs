using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.unit.Services;

/// <summary>
/// Unit tests for DiagnosticsServiceExtensions implementation.
/// Tests boot timeline retrieval, error history, connection stats, and network diagnostics.
/// </summary>
public class DiagnosticsServiceExtensionsTests
{
    private readonly ILogger<DiagnosticsServiceExtensions> _logger;
    private readonly IBootOrchestrator _bootOrchestrator;
    private readonly DiagnosticsServiceExtensions _service;

    public DiagnosticsServiceExtensionsTests()
    {
        _logger = Substitute.For<ILogger<DiagnosticsServiceExtensions>>();
        _bootOrchestrator = Substitute.For<IBootOrchestrator>();
        _service = new DiagnosticsServiceExtensions(_logger, _bootOrchestrator);
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Return_Valid_Timeline()
    {
        // Arrange
        var bootMetrics = new BootMetrics
        {
            SessionId = Guid.NewGuid(),
            StartTimestamp = DateTimeOffset.UtcNow.AddSeconds(-4),
            EndTimestamp = DateTimeOffset.UtcNow,
            Stage0DurationMs = 1000,
            Stage1DurationMs = 2000,
            Stage2DurationMs = 1000,
            TotalDurationMs = 4000,
            SuccessStatus = BootStatus.Success,
            ServiceMetrics = new List<ServiceMetrics>
            {
                new() { ServiceName = "TestService", DurationMs = 500, Success = true }
            }
        };
        _bootOrchestrator.GetBootMetrics().Returns(bootMetrics);

        // Act
        var timeline = await _service.GetBootTimelineAsync();

        // Assert
        timeline.Should().NotBeNull();
        timeline.TotalBootTime.Should().Be(TimeSpan.FromSeconds(4));
        timeline.Stage0.Duration.Should().Be(TimeSpan.FromSeconds(1));
        timeline.Stage1.Duration.Should().Be(TimeSpan.FromSeconds(2));
        timeline.Stage2.Duration.Should().Be(TimeSpan.FromSeconds(1));
        timeline.Stage1.ServiceTimings.Should().HaveCount(1);
        timeline.Stage1.ServiceTimings[0].ServiceName.Should().Be("TestService");
        timeline.Stage1.ServiceTimings[0].Duration.Should().Be(TimeSpan.FromMilliseconds(500));
        timeline.Stage1.ServiceTimings[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Handle_Null_DurationMs()
    {
        // Arrange
        var bootMetrics = new BootMetrics
        {
            SessionId = Guid.NewGuid(),
            StartTimestamp = DateTimeOffset.UtcNow.AddSeconds(-3),
            EndTimestamp = DateTimeOffset.UtcNow,
            Stage0DurationMs = 1000,
            Stage1DurationMs = 1000,
            Stage2DurationMs = 1000,
            TotalDurationMs = 3000,
            SuccessStatus = BootStatus.Success,
            ServiceMetrics = new List<ServiceMetrics>
            {
                new() { ServiceName = "TestService", DurationMs = null, Success = true }
            }
        };
        _bootOrchestrator.GetBootMetrics().Returns(bootMetrics);

        // Act
        var timeline = await _service.GetBootTimelineAsync();

        // Assert
        timeline.Should().NotBeNull();
        timeline.Stage1.ServiceTimings[0].Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task GetRecentErrorsAsync_Should_Return_Empty_Initially()
    {
        // Act
        var errors = await _service.GetRecentErrorsAsync(10);

        // Assert
        errors.Should().NotBeNull();
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentErrorsAsync_Should_Limit_Count()
    {
        // Arrange - Add multiple errors
        for (int i = 0; i < 150; i++)
        {
            _service.AddError(new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Error,
                Category = "Test",
                Message = $"Error {i}",
                StackTrace = null,
                RecoverySuggestion = null,
                ContextData = new Dictionary<string, string>()
            });
        }

        // Act - Request more than buffer size
        var errors = await _service.GetRecentErrorsAsync(150);

        // Assert - Should be limited to 100 (buffer size per CL-007)
        errors.Count.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public async Task GetRecentErrorsAsync_Should_Return_Most_Recent_Errors()
    {
        // Arrange - Add errors with different timestamps
        var oldError = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            Severity = ErrorSeverity.Error,
            Category = "Old",
            Message = "Old error",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        };

        var newError = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "New",
            Message = "New error",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        };

        _service.AddError(oldError);
        _service.AddError(newError);

        // Act
        var errors = await _service.GetRecentErrorsAsync(2);

        // Assert
        errors.Should().HaveCount(2);
        errors.First().Message.Should().Be("Old error"); // FIFO order
        errors.Last().Message.Should().Be("New error");
    }

    [Fact]
    public async Task GetConnectionPoolStatsAsync_Should_Return_Valid_Stats_On_Windows()
    {
        // Act
        var stats = await _service.GetConnectionPoolStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        stats.MySqlPool.Should().NotBeNull();
        stats.HttpPool.Should().NotBeNull();
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_Should_Return_Results()
    {
        // Arrange
        var endpoints = new List<string> { "https://www.google.com" };

        // Act
        var results = await _service.RunNetworkDiagnosticsAsync(endpoints, timeoutSeconds: 5);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Endpoint.Should().Be("https://www.google.com");
        results[0].ResponseTime.Should().NotBeNull();
        results[0].ResponseTime?.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_Should_Reject_Invalid_Endpoint_Count()
    {
        // Act & Assert - Empty list
        await FluentActions.Invoking(() => _service.RunNetworkDiagnosticsAsync(new List<string>(), timeoutSeconds: 5))
            .Should().ThrowAsync<ArgumentException>()
            .WithParameterName("endpoints");

        // Act & Assert - Too many endpoints (>5)
        var tooMany = Enumerable.Range(1, 6).Select(i => $"https://endpoint{i}.com").ToList();
        await FluentActions.Invoking(() => _service.RunNetworkDiagnosticsAsync(tooMany, timeoutSeconds: 5))
            .Should().ThrowAsync<ArgumentException>()
            .WithParameterName("endpoints");
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_Should_Reject_Invalid_Timeout()
    {
        // Arrange
        var endpoints = new List<string> { "https://www.google.com" };

        // Act & Assert - Too short (<1s)
        await FluentActions.Invoking(() => _service.RunNetworkDiagnosticsAsync(endpoints, timeoutSeconds: 0))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("timeoutSeconds");

        // Act & Assert - Too long (>30s)
        await FluentActions.Invoking(() => _service.RunNetworkDiagnosticsAsync(endpoints, timeoutSeconds: 31))
            .Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("timeoutSeconds");
    }

    [Fact]
    public async Task RunNetworkDiagnosticsAsync_Should_Handle_Unreachable_Endpoint()
    {
        // Arrange - Use an invalid endpoint
        var endpoints = new List<string> { "https://this-domain-does-not-exist-12345.com" };

        // Act
        var results = await _service.RunNetworkDiagnosticsAsync(endpoints, timeoutSeconds: 5);

        // Assert
        results.Should().HaveCount(1);
        results[0].IsReachable.Should().BeFalse();
        results[0].ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ForceGarbageCollectionAsync_Should_Complete_Without_Error()
    {
        // Act
        var memoryFreed = await _service.ForceGarbageCollectionAsync();

        // Assert
        memoryFreed.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ClearCacheAsync_Should_Complete_Without_Error()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.ClearCacheAsync())
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Handle_Empty_ServiceMetrics()
    {
        // Arrange
        var bootMetrics = new BootMetrics
        {
            SessionId = Guid.NewGuid(),
            StartTimestamp = DateTimeOffset.UtcNow.AddSeconds(-3),
            EndTimestamp = DateTimeOffset.UtcNow,
            Stage0DurationMs = 1000,
            Stage1DurationMs = 1000,
            Stage2DurationMs = 1000,
            TotalDurationMs = 3000,
            SuccessStatus = BootStatus.Success,
            ServiceMetrics = new List<ServiceMetrics>()
        };
        _bootOrchestrator.GetBootMetrics().Returns(bootMetrics);

        // Act
        var timeline = await _service.GetBootTimelineAsync();

        // Assert
        timeline.Should().NotBeNull();
        timeline.Stage1.ServiceTimings.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentErrorsAsync_Should_Return_Chronological_Order()
    {
        // Arrange
        var errors = new List<ErrorEntry>();
        for (int i = 0; i < 5; i++)
        {
            var error = new ErrorEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow.AddSeconds(i),
                Severity = ErrorSeverity.Error,
                Category = "Test",
                Message = $"Error {i}",
                StackTrace = null,
                RecoverySuggestion = null,
                ContextData = new Dictionary<string, string>()
            };
            _service.AddError(error);
            errors.Add(error);
        }

        // Act
        var recentErrors = await _service.GetRecentErrorsAsync(5);

        // Assert
        recentErrors.Should().HaveCount(5);
        var timestamps = recentErrors.Select(e => e.Timestamp).ToList();
        timestamps.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task AddError_Should_Accept_All_Severity_Levels()
    {
        // Arrange & Act
        _service.AddError(new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Info,
            Category = "Test",
            Message = "Info message",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        });

        _service.AddError(new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Warning,
            Category = "Test",
            Message = "Warning message",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        });

        _service.AddError(new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Test",
            Message = "Error message",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        });

        _service.AddError(new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Critical,
            Category = "Test",
            Message = "Critical message",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        });

        // Assert
        var errors = await _service.GetRecentErrorsAsync(10);
        errors.Should().HaveCount(4);
        errors.Should().Contain(e => e.Severity == ErrorSeverity.Info);
        errors.Should().Contain(e => e.Severity == ErrorSeverity.Warning);
        errors.Should().Contain(e => e.Severity == ErrorSeverity.Error);
        errors.Should().Contain(e => e.Severity == ErrorSeverity.Critical);
    }
}
