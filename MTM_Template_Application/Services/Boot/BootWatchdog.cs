using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Monitors boot sequence for timeouts and hangs.
/// Enforces stage-specific timeout limits:
/// - Stage 0: 10 seconds
/// - Stage 1: 60 seconds
/// - Stage 2: 15 seconds
/// Collects diagnostic information if timeout occurs.
/// </summary>
public class BootWatchdog
{
    private readonly ILogger<BootWatchdog> _logger;
    private CancellationTokenSource? _watchdogCts;
    private Task? _watchdogTask;
    private readonly Stopwatch _stopwatch = new();

    // Timeout configurations (in seconds)
    private static readonly TimeSpan Stage0Timeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Stage1Timeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan Stage2Timeout = TimeSpan.FromSeconds(15);

    public BootWatchdog(ILogger<BootWatchdog> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Start watchdog for a specific stage.
    /// </summary>
    public void StartWatchdog(int stageNumber, Action onTimeout)
    {
        ArgumentNullException.ThrowIfNull(onTimeout);

        if (_watchdogTask != null && !_watchdogTask.IsCompleted)
        {
            _logger.LogWarning("Watchdog already running. Stopping previous watchdog.");
            StopWatchdog();
        }

        var timeout = GetTimeoutForStage(stageNumber);

        _logger.LogInformation(
            "Starting watchdog for Stage {StageNumber} with timeout {TimeoutSeconds}s",
            stageNumber,
            timeout.TotalSeconds
        );

        _watchdogCts = new CancellationTokenSource();
        _stopwatch.Restart();

        _watchdogTask = Task.Factory.StartNew(async () =>
        {
            try
            {
                await Task.Delay(timeout, _watchdogCts.Token);

                // If we reach here, timeout occurred
                _logger.LogError(
                    "Stage {StageNumber} timeout after {TimeoutSeconds}s",
                    stageNumber,
                    timeout.TotalSeconds
                );

                onTimeout();
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation - stage completed before timeout
                _logger.LogDebug(
                    "Watchdog for Stage {StageNumber} cancelled (stage completed)",
                    stageNumber
                );
            }
        }, _watchdogCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    /// <summary>
    /// Stop the watchdog (stage completed successfully).
    /// </summary>
    public void StopWatchdog()
    {
        _stopwatch.Stop();

        if (_watchdogCts != null)
        {
            _watchdogCts.Cancel();
            _watchdogCts.Dispose();
            _watchdogCts = null;
        }

        if (_watchdogTask != null)
        {
            try
            {
                _watchdogTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Expected - task was cancelled
            }

            _watchdogTask = null;
        }

        _logger.LogDebug("Watchdog stopped. Elapsed: {ElapsedMs}ms", _stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Get the current elapsed time since watchdog started.
    /// </summary>
    public long GetElapsedMilliseconds()
    {
        return _stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Check if watchdog is currently running.
    /// </summary>
    public bool IsRunning()
    {
        return _watchdogTask != null && !_watchdogTask.IsCompleted;
    }

    /// <summary>
    /// Get timeout duration for a specific stage.
    /// </summary>
    public TimeSpan GetTimeoutForStage(int stageNumber)
    {
        return stageNumber switch
        {
            0 => Stage0Timeout,
            1 => Stage1Timeout,
            2 => Stage2Timeout,
            _ => throw new ArgumentOutOfRangeException(nameof(stageNumber), $"Invalid stage number: {stageNumber}")
        };
    }

    /// <summary>
    /// Collect diagnostic information for timeout scenarios.
    /// </summary>
    public BootTimeoutDiagnostics CollectDiagnostics(int stageNumber)
    {
        _logger.LogInformation("Collecting timeout diagnostics for Stage {StageNumber}", stageNumber);

        var diagnostics = new BootTimeoutDiagnostics
        {
            StageNumber = stageNumber,
            ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
            TimeoutMilliseconds = (long)GetTimeoutForStage(stageNumber).TotalMilliseconds,
            TimestampUtc = DateTimeOffset.UtcNow,
            ThreadCount = Process.GetCurrentProcess().Threads.Count,
            MemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024
        };

        _logger.LogInformation(
            "Timeout diagnostics: Stage={Stage}, Elapsed={ElapsedMs}ms, Threads={ThreadCount}, Memory={MemoryMB}MB",
            diagnostics.StageNumber,
            diagnostics.ElapsedMilliseconds,
            diagnostics.ThreadCount,
            diagnostics.MemoryUsageMB
        );

        return diagnostics;
    }
}

/// <summary>
/// Diagnostic information collected during a boot timeout.
/// </summary>
public class BootTimeoutDiagnostics
{
    public int StageNumber { get; init; }
    public long ElapsedMilliseconds { get; init; }
    public long TimeoutMilliseconds { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }
    public int ThreadCount { get; init; }
    public long MemoryUsageMB { get; init; }
}
