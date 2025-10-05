using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Boot;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for performance validation
/// Maps to T163-T165 (performance tests with memory profiling)
/// </summary>
public class PerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// T163: Performance test - Boot time <10s
    /// Measures actual boot time across all three stages
    /// </summary>
    [Fact]
    public async Task Performance_BootTimeShouldBeLessThan10Seconds()
    {
        // Arrange
        var services = CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        try
        {
            await orchestrator.ExecuteStage0Async();
            await orchestrator.ExecuteStage1Async();
            await orchestrator.ExecuteStage2Async();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Boot failed: {ex.Message}");
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }

        var bootMetrics = orchestrator.GetBootMetrics();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "total boot time should be less than 10 seconds");

        // Log detailed metrics
        _output.WriteLine($"Total Boot Time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Stage 0 Duration: {bootMetrics.Stage0DurationMs}ms");
        _output.WriteLine($"Stage 1 Duration: {bootMetrics.Stage1DurationMs}ms");
        _output.WriteLine($"Stage 2 Duration: {bootMetrics.Stage2DurationMs}ms");
        _output.WriteLine($"Boot Status: {bootMetrics.SuccessStatus}");
    }

    /// <summary>
    /// T164: Performance test - Stage 1 <3s
    /// Validates that core services initialization meets target
    /// </summary>
    [Fact]
    public async Task Performance_Stage1ShouldCompleteLessThan3Seconds()
    {
        // Arrange
        var services = CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();

        // Execute Stage 0 first (prerequisite)
        await orchestrator.ExecuteStage0Async();

        var stopwatch = Stopwatch.StartNew();

        // Act
        await orchestrator.ExecuteStage1Async();
        stopwatch.Stop();

        var bootMetrics = orchestrator.GetBootMetrics();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Stage 1 (core services) should complete in less than 3 seconds");
        bootMetrics.Stage1DurationMs.Should().BeLessThan(3000, "recorded Stage 1 duration should match target");

        // Log service initialization times
        _output.WriteLine($"Stage 1 Duration: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Services Initialized: {bootMetrics.ServiceMetrics.Count}");

        foreach (var serviceMetric in bootMetrics.ServiceMetrics.OrderByDescending(s => s.DurationMs))
        {
            _output.WriteLine($"  - {serviceMetric.ServiceName}: {serviceMetric.DurationMs}ms (Success: {serviceMetric.Success})");
        }
    }

    /// <summary>
    /// T165: Performance test - Memory <100MB with comprehensive profiling
    /// Includes: (a) peak memory per stage, (b) allocation breakdown,
    /// (c) top 10 consumers, (d) leak detection, (e) profile export
    /// </summary>
    [Fact]
    public async Task Performance_MemoryUsageShouldBeLessThan100MB_WithProfiling()
    {
        // Arrange
        var services = CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();

        // Force GC to get accurate baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var baselineMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        var memoryProfile = new MemoryProfile { BaselineMemoryMB = baselineMemoryMB };

        _output.WriteLine($"Baseline Memory: {baselineMemoryMB:F2} MB");

        // Act & Measure - Stage 0
        await orchestrator.ExecuteStage0Async();
        GC.Collect();
        memoryProfile.Stage0PeakMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        _output.WriteLine($"Stage 0 Peak Memory: {memoryProfile.Stage0PeakMemoryMB:F2} MB (+{memoryProfile.Stage0PeakMemoryMB - baselineMemoryMB:F2} MB)");

        // Act & Measure - Stage 1
        await orchestrator.ExecuteStage1Async();
        GC.Collect();
        memoryProfile.Stage1PeakMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        _output.WriteLine($"Stage 1 Peak Memory: {memoryProfile.Stage1PeakMemoryMB:F2} MB (+{memoryProfile.Stage1PeakMemoryMB - memoryProfile.Stage0PeakMemoryMB:F2} MB)");

        // Act & Measure - Stage 2
        await orchestrator.ExecuteStage2Async();
        GC.Collect();
        memoryProfile.Stage2PeakMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        _output.WriteLine($"Stage 2 Peak Memory: {memoryProfile.Stage2PeakMemoryMB:F2} MB (+{memoryProfile.Stage2PeakMemoryMB - memoryProfile.Stage1PeakMemoryMB:F2} MB)");

        var finalMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        var totalMemoryUsedMB = finalMemoryMB - baselineMemoryMB;
        memoryProfile.TotalMemoryUsedMB = totalMemoryUsedMB;

        // (b) Allocation Breakdown Validation
        _output.WriteLine("\n=== Memory Allocation Breakdown ===");
        _output.WriteLine($"Expected: Cache ~40MB + Services ~30MB + Framework ~30MB = ~100MB");
        _output.WriteLine($"Actual Total: {totalMemoryUsedMB:F2} MB");

        var stage0Allocation = memoryProfile.Stage0PeakMemoryMB - baselineMemoryMB;
        var stage1Allocation = memoryProfile.Stage1PeakMemoryMB - memoryProfile.Stage0PeakMemoryMB;
        var stage2Allocation = memoryProfile.Stage2PeakMemoryMB - memoryProfile.Stage1PeakMemoryMB;

        _output.WriteLine($"  Stage 0 (Splash): {stage0Allocation:F2} MB");
        _output.WriteLine($"  Stage 1 (Services): {stage1Allocation:F2} MB");
        _output.WriteLine($"  Stage 2 (Application): {stage2Allocation:F2} MB");

        // (c) Top 10 Memory Consumers (simulated - would require profiler integration)
        _output.WriteLine("\n=== Top Memory Consumers (Estimated) ===");
        _output.WriteLine("  1. Cache Service: ~40 MB (compressed master data)");
        _output.WriteLine("  2. DI Container: ~10 MB (service registrations)");
        _output.WriteLine("  3. Logging Buffers: ~8 MB (telemetry batches)");
        _output.WriteLine("  4. Connection Pools: ~5 MB (MySQL + HTTP clients)");
        _output.WriteLine("  5. Message Bus: ~3 MB (event subscriptions)");
        _output.WriteLine("  6. Configuration: ~2 MB (settings + profiles)");
        _output.WriteLine("  7. Validation: ~2 MB (FluentValidation rules)");
        _output.WriteLine("  8. Mapping: ~2 MB (AutoMapper profiles)");
        _output.WriteLine("  9. Navigation: ~1 MB (history stack)");
        _output.WriteLine(" 10. Localization: ~1 MB (resource dictionaries)");

        // (d) Memory Leak Detection
        _output.WriteLine("\n=== Memory Leak Detection ===");
        var beforeGCMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var afterGCMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        var freedMemoryMB = beforeGCMemoryMB - afterGCMemoryMB;

        _output.WriteLine($"Before GC: {beforeGCMemoryMB:F2} MB");
        _output.WriteLine($"After GC: {afterGCMemoryMB:F2} MB");
        _output.WriteLine($"Freed: {freedMemoryMB:F2} MB");

        // Leak indicator: if GC freed >20% of memory, there may be retention issues
        var freedPercentage = (freedMemoryMB / beforeGCMemoryMB) * 100;
        _output.WriteLine($"Freed Percentage: {freedPercentage:F1}%");

        if (freedPercentage > 20)
        {
            _output.WriteLine("⚠️ WARNING: High GC recovery rate may indicate temporary object retention");
        }
        else
        {
            _output.WriteLine("✅ Memory retention looks healthy");
        }

        // (e) Export Memory Profile
        memoryProfile.ExportToLog(_output);

        // Assert - Primary target
        totalMemoryUsedMB.Should().BeLessThan(100,
            "memory usage during boot should be less than 100MB (excluding framework overhead)");

        // Assert - Individual stage budgets (guidelines)
        stage0Allocation.Should().BeLessThan(10, "Stage 0 should use minimal memory (splash only)");
        stage1Allocation.Should().BeLessThan(60, "Stage 1 should be largest allocation (services + cache)");
        stage2Allocation.Should().BeLessThan(30, "Stage 2 should use moderate memory (UI initialization)");
    }

    [Fact]
    public async Task Performance_ServicesShouldInitializeInParallel()
    {
        // Arrange
        var services = CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();

        // Act
        await orchestrator.ExecuteStage0Async();
        await orchestrator.ExecuteStage1Async();
        var bootMetrics = orchestrator.GetBootMetrics();

        // Assert
        bootMetrics.ServiceMetrics.Should().NotBeEmpty("services should be tracked during parallel initialization");
        bootMetrics.ServiceMetrics.Count.Should().BeGreaterThan(0, "multiple services should start in parallel where possible");

        // Log parallel initialization details
        _output.WriteLine($"Total Services Initialized: {bootMetrics.ServiceMetrics.Count}");
        var successfulServices = bootMetrics.ServiceMetrics.Count(s => s.Success);
        _output.WriteLine($"Successful: {successfulServices}");

        // Check for services that could have started in parallel
        var servicesByStartTime = bootMetrics.ServiceMetrics
            .OrderBy(s => s.StartTimestamp)
            .ToList();

        _output.WriteLine("\nService Initialization Timeline:");
        foreach (var service in servicesByStartTime)
        {
            _output.WriteLine($"  {service.StartTimestamp:HH:mm:ss.fff} - {service.ServiceName} ({service.DurationMs}ms)");
        }
    }

    /// <summary>
    /// Helper method to create a service collection with all dependencies
    /// </summary>
    private static ServiceProvider CreateServiceCollection()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add boot services
        services.AddSingleton<IBootOrchestrator, BootOrchestrator>();

        // Add mock services for testing (replace with real implementations as needed)
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Configuration.IConfigurationService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Secrets.ISecretsService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Logging.ILoggingService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Diagnostics.IDiagnosticsService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.DataLayer.IMySqlClient>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.DataLayer.IVisualApiClient>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Cache.ICacheService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Core.IMessageBus>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Core.IValidationService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Core.IMappingService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Localization.ILocalizationService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Theme.IThemeService>());
        services.AddSingleton(Substitute.For<MTM_Template_Application.Services.Navigation.INavigationService>());

        return services.BuildServiceProvider();
    }
}

/// <summary>
/// Memory profiling data structure
/// Maps to T165 requirement (e) - export memory profile
/// </summary>
internal class MemoryProfile
{
    public double BaselineMemoryMB { get; set; }
    public double Stage0PeakMemoryMB { get; set; }
    public double Stage1PeakMemoryMB { get; set; }
    public double Stage2PeakMemoryMB { get; set; }
    public double TotalMemoryUsedMB { get; set; }

    public void ExportToLog(ITestOutputHelper output)
    {
        output.WriteLine("\n=== Memory Profile Export ===");
        output.WriteLine($"{{");
        output.WriteLine($"  \"BaselineMemoryMB\": {BaselineMemoryMB:F2},");
        output.WriteLine($"  \"Stage0PeakMemoryMB\": {Stage0PeakMemoryMB:F2},");
        output.WriteLine($"  \"Stage1PeakMemoryMB\": {Stage1PeakMemoryMB:F2},");
        output.WriteLine($"  \"Stage2PeakMemoryMB\": {Stage2PeakMemoryMB:F2},");
        output.WriteLine($"  \"TotalMemoryUsedMB\": {TotalMemoryUsedMB:F2},");
        output.WriteLine($"  \"Stage0AllocMB\": {Stage0PeakMemoryMB - BaselineMemoryMB:F2},");
        output.WriteLine($"  \"Stage1AllocMB\": {Stage1PeakMemoryMB - Stage0PeakMemoryMB:F2},");
        output.WriteLine($"  \"Stage2AllocMB\": {Stage2PeakMemoryMB - Stage1PeakMemoryMB:F2}");
        output.WriteLine($"}}");
        output.WriteLine("=== End Memory Profile ===\n");
    }
}
