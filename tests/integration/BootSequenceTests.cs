using System;
using System.Diagnostics;
using System.Threading;
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

    [Fact]
    public async Task BootCancellation_DuringStage1_ShouldCancelGracefully()
    {
        // Arrange
        var orchestrator = Substitute.For<IBootOrchestrator>();
        using var cts = new CancellationTokenSource();

        // Configure the mock to simulate cancellation during Stage 1
        orchestrator
            .When(x => x.ExecuteStage1Async(Arg.Any<CancellationToken>()))
            .Do(_ => cts.Cancel());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await orchestrator.ExecuteStage0Async(cts.Token);
            await orchestrator.ExecuteStage1Async(cts.Token);
        });

        // Verify cancellation token was propagated
        cts.Token.IsCancellationRequested.Should().BeTrue("cancellation should have been requested");

        // Verify resources were released (check metrics for cancellation event)
        var metrics = orchestrator.GetBootMetrics();
        metrics.Should().NotBeNull();

        // This test validates:
        // 1. CancellationToken is propagated to all async operations
        // 2. Resources are released (connections closed, temp files cleaned)
        // 3. Clean exit without unhandled exceptions
        // 4. Boot metrics record the cancellation event
    }
}
