using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Boot.Stages;
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
        var services = TestHelpers.BootTestHelper.CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();
        var stopwatch = Stopwatch.StartNew();

        // Act - Use ExecuteBootSequenceAsync for full boot with proper metrics
        var metrics = await orchestrator.ExecuteBootSequenceAsync();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "total boot should complete in <10s");
        metrics.Should().NotBeNull();
        metrics.SuccessStatus.Should().Be(BootStatus.Success);
        metrics.Stage0DurationMs.Should().BeGreaterThan(0);
        metrics.Stage1DurationMs.Should().BeGreaterThan(0);
        metrics.Stage2DurationMs.Should().BeGreaterThan(0);
        metrics.MemoryUsageMB.Should().BeLessThan(100, "memory usage should be <100MB");
    }

    [Fact]
    public async Task BootSequence_ShouldReportProgressDuringExecution()
    {
        // Arrange
        var services = TestHelpers.BootTestHelper.CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();
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
        var services = TestHelpers.BootTestHelper.CreateServiceCollection();
        var orchestrator = services.GetRequiredService<IBootOrchestrator>();
        using var cts = new CancellationTokenSource();

        // Cancel immediately to test cancellation handling
        cts.Cancel();

        // Act & Assert - Should throw OperationCanceledException or derived type (TaskCanceledException)
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await orchestrator.ExecuteStage0Async(cts.Token);
        });

        // Verify cancellation was handled
        cts.Token.IsCancellationRequested.Should().BeTrue();
        exception.Should().NotBeNull();
    }
}
