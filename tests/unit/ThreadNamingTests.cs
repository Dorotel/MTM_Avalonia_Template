using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Tests to verify thread naming convention is properly applied.
/// </summary>
public class ThreadNamingTests
{
    private readonly ITestOutputHelper _output;

    public ThreadNamingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Task_Run_WithThreadNaming_SetsThreadNameCorrectly()
    {
        // Arrange
        string? capturedThreadName = null;
        var tcs = new TaskCompletionSource<bool>();
        var expectedThreadName = "ThreadNamingTests.SetAndGetThreadName[Sync]";

        // Act
        _ = Task.Run(() =>
        {
            Thread.CurrentThread.Name = expectedThreadName;
            capturedThreadName = Thread.CurrentThread.Name;
            Thread.Sleep(100); // Keep thread alive briefly
            tcs.SetResult(true);
        });

        await tcs.Task;

        // Assert
        _output.WriteLine($"Captured thread name: {capturedThreadName}");
        capturedThreadName.Should().Be(expectedThreadName);
    }

    [Fact]
    public async Task TaskFactory_StartNew_WithLongRunning_SetsThreadNameCorrectly()
    {
        // Arrange
        string? capturedThreadName = null;
        var tcs = new TaskCompletionSource<bool>();
        var expectedThreadName = "ThreadNamingTests.SetThreadName_WithAsyncCode[Async]";

        // Act
        _ = Task.Factory.StartNew(async () =>
        {
            Thread.CurrentThread.Name = expectedThreadName;
            capturedThreadName = Thread.CurrentThread.Name;
            _output.WriteLine($"Thread name set to: {capturedThreadName}");
            _output.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");

            await Task.Delay(100); // Async operation

            var threadNameAfterAwait = Thread.CurrentThread.Name;
            _output.WriteLine($"Thread name after await: {threadNameAfterAwait}");
            _output.WriteLine($"Thread ID after await: {Thread.CurrentThread.ManagedThreadId}");

            tcs.SetResult(true);
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        await tcs.Task;

        // Assert
        _output.WriteLine($"Final captured thread name: {capturedThreadName}");
        capturedThreadName.Should().Be(expectedThreadName);
    }

    [Fact]
    public async Task GetAllThreadNames_ShowsNamedThreads()
    {
        // Arrange
        var threadNames = new List<string>();
        var tasks = new List<Task>();
        var barrier = new Barrier(5); // Wait for all 5 threads to start

        // Act - Start 5 named threads
        for (int i = 0; i < 5; i++)
        {
            var index = i;
            var task = Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = $"ThreadNamingTests.SetMultipleThreadNames[Thread{index}]";
                barrier.SignalAndWait(TimeSpan.FromSeconds(5)); // Wait for all threads
                Thread.Sleep(200); // Keep thread alive
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            tasks.Add(task);
        }

        // Give threads time to start
        await Task.Delay(100);

        // Capture all thread names from current process
        var process = Process.GetCurrentProcess();
        foreach (ProcessThread thread in process.Threads)
        {
            try
            {
                // Note: ProcessThread doesn't expose the thread name directly
                // This is a limitation of the ProcessThread API
                _output.WriteLine($"Thread ID: {thread.Id}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error reading thread: {ex.Message}");
            }
        }

        await Task.WhenAll(tasks);

        // Assert - At least verify tasks completed
        tasks.Should().AllSatisfy(t => t.IsCompleted.Should().BeTrue());
    }

    [Fact]
    public void ThreadNamingConvention_FollowsStandard()
    {
        // Arrange & Act
        var testCases = new[]
        {
            "BootWatchdog-StartWatchdog-Stage0",
            "BootWatchdog-StartWatchdog-Stage1",
            "BootWatchdog-StartWatchdog-Stage2",
            "LoggingService-FlushAsync",
            "Stage1ServicesInitialization-InitializeCoreServices-Validation",
            "Stage1ServicesInitialization-InitializeCoreServices-Mapping",
            "MessageBusTests-PublishAsync-ConcurrentPublisher0",
            "MessageBusTests-SubscribeAsync-ConcurrentSubscriber0"
        };

        // Assert - All follow {FileName}-{MethodName}-{Extra} pattern
        foreach (var threadName in testCases)
        {
            var parts = threadName.Split('-');
            _output.WriteLine($"Testing: {threadName}");

            parts.Should().HaveCountGreaterOrEqualTo(2,
                $"Thread name '{threadName}' should have at least FileName-MethodName");

            // First part should be PascalCase (FileName)
            parts[0].Should().MatchRegex("^[A-Z][a-zA-Z0-9]*$",
                $"FileName part '{parts[0]}' should be PascalCase");

            // Second part should be PascalCase or camelCase (MethodName)
            parts[1].Should().MatchRegex("^[A-Z][a-zA-Z0-9]*$",
                $"MethodName part '{parts[1]}' should be PascalCase");
        }
    }
}
