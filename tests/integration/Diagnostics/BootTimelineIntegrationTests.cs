using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration.Diagnostics;

/// <summary>
/// Integration tests for boot timeline retrieval from DiagnosticsServiceExtensions.
/// Tests T024: Boot timeline query with real BootOrchestrator integration.
/// </summary>
[Trait("Category", "Integration")]
public class BootTimelineIntegrationTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IDiagnosticsServiceExtensions _diagnosticsService;
    private readonly IBootOrchestrator _bootOrchestrator;

    public BootTimelineIntegrationTests()
    {
        // Setup DI container
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Mock IBootOrchestrator with test data
        var mockBootOrchestrator = Substitute.For<IBootOrchestrator>();
        var testBootMetrics = new BootMetrics
        {
            SessionId = Guid.NewGuid(),
            StartTimestamp = DateTimeOffset.UtcNow.AddSeconds(-4),
            EndTimestamp = DateTimeOffset.UtcNow,
            TotalDurationMs = 4000, // 4 seconds
            Stage0DurationMs = 1000, // 1 second
            Stage1DurationMs = 2500, // 2.5 seconds
            Stage2DurationMs = 500,  // 0.5 seconds
            SuccessStatus = BootStatus.Success,
            MemoryUsageMB = 85,
            PlatformInfo = "Windows",
            AppVersion = "1.0.0",
            ServiceMetrics = new List<ServiceMetrics>
            {
                new()
                {
                    SessionId = Guid.NewGuid(),
                    ServiceName = "ConfigurationService",
                    DurationMs = 150,
                    Success = true,
                    ErrorMessage = null
                },
                new()
                {
                    SessionId = Guid.NewGuid(),
                    ServiceName = "LoggingService",
                    DurationMs = 200,
                    Success = true,
                    ErrorMessage = null
                },
                new()
                {
                    SessionId = Guid.NewGuid(),
                    ServiceName = "CacheService",
                    DurationMs = 400,
                    Success = true,
                    ErrorMessage = null
                }
            }
        };

        mockBootOrchestrator.GetBootMetrics().Returns(testBootMetrics);
        services.AddSingleton(mockBootOrchestrator);

        // Add Debug Terminal services
        services.AddDebugTerminalServices();

        _serviceProvider = services.BuildServiceProvider();
        _diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsServiceExtensions>();
        _bootOrchestrator = mockBootOrchestrator;
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Return_Valid_Timeline()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var timeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        timeline.Should().NotBeNull();
        timeline.Stage0.Should().NotBeNull();
        timeline.Stage1.Should().NotBeNull();
        timeline.Stage2.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Have_Correct_Stage_Durations()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var timeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        timeline.Stage0.Duration.Should().Be(TimeSpan.FromMilliseconds(1000), "Stage 0 should be 1000ms per test data");
        timeline.Stage1.Duration.Should().Be(TimeSpan.FromMilliseconds(2500), "Stage 1 should be 2500ms per test data");
        timeline.Stage2.Duration.Should().Be(TimeSpan.FromMilliseconds(500), "Stage 2 should be 500ms per test data");
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Calculate_Total_Boot_Time()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var timeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        var expectedTotal = TimeSpan.FromMilliseconds(4000); // 4000ms = 4s
        timeline.TotalBootTime.Should().Be(expectedTotal, "TotalBootTime should match sum of stage durations");
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Include_Service_Initialization_Timings()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var timeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        timeline.Stage1.ServiceTimings.Should().NotBeNull();
        timeline.Stage1.ServiceTimings.Should().HaveCount(3, "3 services in test data");

        var configService = timeline.Stage1.ServiceTimings.FirstOrDefault(s => s.ServiceName == "ConfigurationService");
        configService.Should().NotBeNull();
        configService!.Duration.Should().Be(TimeSpan.FromMilliseconds(150));
        configService.Success.Should().BeTrue();

        var loggingService = timeline.Stage1.ServiceTimings.FirstOrDefault(s => s.ServiceName == "LoggingService");
        loggingService.Should().NotBeNull();
        loggingService!.Duration.Should().Be(TimeSpan.FromMilliseconds(200));
        loggingService.Success.Should().BeTrue();

        var cacheService = timeline.Stage1.ServiceTimings.FirstOrDefault(s => s.ServiceName == "CacheService");
        cacheService.Should().NotBeNull();
        cacheService!.Duration.Should().Be(TimeSpan.FromMilliseconds(400));
        cacheService.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Validate_Timeline_Structure()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var timeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert - Verify IsValid() method works
        timeline.IsValid().Should().BeTrue("timeline should be valid (sum of stages equals total)");
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Return_Empty_ServiceTimings_When_No_Services()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var emptyBootMetrics = new BootMetrics
        {
            SessionId = Guid.NewGuid(),
            StartTimestamp = DateTimeOffset.UtcNow.AddSeconds(-3.5),
            EndTimestamp = DateTimeOffset.UtcNow,
            TotalDurationMs = 3500,
            Stage0DurationMs = 1000,
            Stage1DurationMs = 2000,
            Stage2DurationMs = 500,
            SuccessStatus = BootStatus.Success,
            ServiceMetrics = new List<ServiceMetrics>() // Empty list
        };

        var mockBootOrchestrator = Substitute.For<IBootOrchestrator>();
        mockBootOrchestrator.GetBootMetrics().Returns(emptyBootMetrics);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
        services.AddSingleton(mockBootOrchestrator);
        services.AddDebugTerminalServices();

        var serviceProvider = services.BuildServiceProvider();
        var diagnosticsService = serviceProvider.GetRequiredService<IDiagnosticsServiceExtensions>();

        // Act
        var timeline = await diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        timeline.Stage1.ServiceTimings.Should().NotBeNull();
        timeline.Stage1.ServiceTimings.Should().BeEmpty("no services initialized");
    }

    [Fact]
    public async Task GetBootTimelineAsync_Should_Handle_Null_Boot_Metrics()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var mockBootOrchestrator = Substitute.For<IBootOrchestrator>();
        mockBootOrchestrator.GetBootMetrics().Returns((BootMetrics?)null);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
        services.AddSingleton(mockBootOrchestrator);
        services.AddDebugTerminalServices();

        var serviceProvider = services.BuildServiceProvider();
        var diagnosticsService = serviceProvider.GetRequiredService<IDiagnosticsServiceExtensions>();

        // Act
        Func<Task> act = async () => await diagnosticsService.GetBootTimelineAsync(cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*timeline is not available*");
    }
}
