using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Boot;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for boot sequence execution
/// </summary>
public class BootSequenceTests
{
    [Fact]
    public async Task NormalBootSequence_ShouldExecuteAllStagesSuccessfully()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await orchestrator.ExecuteStage0Async();
        await orchestrator.ExecuteStage1Async();
        await orchestrator.ExecuteStage2Async();
        stopwatch.Stop();
        
        var metrics = orchestrator.GetBootMetrics();
        
        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "total boot should complete in <10s");
        
        // This test will fail until IBootOrchestrator is implemented
        metrics.Should().NotBeNull();
        metrics.SuccessStatus.Should().Be(MTM_Template_Application.Models.Boot.BootStatus.Success);
        metrics.Stage0DurationMs.Should().BeGreaterThan(0);
        metrics.Stage1DurationMs.Should().BeGreaterThan(0);
        metrics.Stage2DurationMs.Should().BeGreaterThan(0);
        metrics.MemoryUsageMB.Should().BeLessThan(100, "memory usage should be <100MB");
    }

    [Fact]
    public async Task BootSequence_ShouldReportProgressDuringExecution()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        int progressCallbacks = 0;
        
        orchestrator.OnProgressChanged += (sender, args) =>
        {
            progressCallbacks++;
            args.ProgressPercentage.Should().BeInRange(0, 100);
            args.StatusMessage.Should().NotBeNullOrEmpty();
        };
        
        // Act
        await orchestrator.ExecuteStage0Async();
        await orchestrator.ExecuteStage1Async();
        await orchestrator.ExecuteStage2Async();
        
        // Assert
        progressCallbacks.Should().BeGreaterThan(0, "progress events should be raised during boot");
    }
}
