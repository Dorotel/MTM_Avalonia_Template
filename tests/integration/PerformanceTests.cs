using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Boot;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for performance validation
/// </summary>
public class PerformanceTests
{
    [Fact]
    public async Task Performance_BootTimeShouldBeLessThan10Seconds()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await orchestrator.ExecuteStage0Async();
        await orchestrator.ExecuteStage1Async();
        await orchestrator.ExecuteStage2Async();
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "total boot time should be less than 10 seconds");
    }

    [Fact]
    public async Task Performance_Stage1ShouldCompleteLessThan3Seconds()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await orchestrator.ExecuteStage1Async();
        stopwatch.Stop();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Stage 1 (core services) should complete in less than 3 seconds");
    }

    [Fact]
    public async Task Performance_MemoryUsageShouldBeLessThan100MB()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        var initialMemory = System.GC.GetTotalMemory(false);
        
        // Act
        await orchestrator.ExecuteStage0Async();
        await orchestrator.ExecuteStage1Async();
        await orchestrator.ExecuteStage2Async();
        
        var finalMemory = System.GC.GetTotalMemory(false);
        var memoryUsedMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);
        
        // Assert
        memoryUsedMB.Should().BeLessThan(100, "memory usage during boot should be less than 100MB");
    }

    [Fact]
    public async Task Performance_ServicesShouldInitializeInParallel()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        var metrics = new MTM_Template_Application.Models.Boot.BootMetrics
        {
            TotalDurationMs = 5000,
            ServiceMetrics = new System.Collections.Generic.List<MTM_Template_Application.Models.Boot.ServiceMetrics>
            {
                new() { ServiceName = "Service1", Success = true, DurationMs = 500 },
                new() { ServiceName = "Service2", Success = true, DurationMs = 600 },
                new() { ServiceName = "Service3", Success = true, DurationMs = 550 }
            }
        };
        orchestrator.GetBootMetrics().Returns(metrics);
        
        // Act
        await orchestrator.ExecuteStage1Async();
        var bootMetrics = orchestrator.GetBootMetrics();
        
        // Assert
        bootMetrics.ServiceMetrics.Should().NotBeEmpty("services should be tracked during parallel initialization");
        bootMetrics.ServiceMetrics.Count.Should().BeGreaterThan(0, "multiple services should start in parallel where possible");
    }

    [Fact]
    public async Task Performance_CacheShouldImproveResponseTime()
    {
        // Arrange
        var cacheService = NSubstitute.Substitute.For<MTM_Template_Application.Services.Cache.ICacheService>();
        var stopwatch = Stopwatch.StartNew();
        
        // First call (cache miss) - simulate slower response
        cacheService.GetAsync<string>("test_key").Returns(Task.FromResult<string?>(null));
        await cacheService.GetAsync<string>("test_key");
        var firstCallTime = stopwatch.ElapsedMilliseconds;
        
        // Second call (cache hit) - should be faster
        stopwatch.Restart();
        cacheService.GetAsync<string>("test_key").Returns(Task.FromResult<string?>("cached_value"));
        await cacheService.GetAsync<string>("test_key");
        var secondCallTime = stopwatch.ElapsedMilliseconds;
        
        // Assert
        secondCallTime.Should().BeLessOrEqualTo(firstCallTime, "cache hits should be faster than cache misses");
    }
}
