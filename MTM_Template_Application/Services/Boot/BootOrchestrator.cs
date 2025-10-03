using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Services.Boot.Stages;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Orchestrates the three-stage boot sequence: Stage 0 (Splash) → Stage 1 (Services) → Stage 2 (Application).
/// Manages progress tracking, timeout enforcement, and error recovery.
/// </summary>
public class BootOrchestrator : IBootOrchestrator
{
    private readonly ILogger<BootOrchestrator> _logger;
    private readonly Stage0Bootstrap _stage0;
    private readonly Stage1ServicesInitialization _stage1;
    private readonly Stage2ApplicationReady _stage2;
    private readonly BootWatchdog _watchdog;
    private readonly BootProgressCalculator _progressCalculator;

    private BootMetrics? _currentMetrics;
    private readonly object _metricsLock = new();

    public event EventHandler<BootProgressEventArgs>? OnProgressChanged;

    public BootOrchestrator(
        ILogger<BootOrchestrator> logger,
        Stage0Bootstrap stage0,
        Stage1ServicesInitialization stage1,
        Stage2ApplicationReady stage2,
        BootWatchdog watchdog,
        BootProgressCalculator progressCalculator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(stage0);
        ArgumentNullException.ThrowIfNull(stage1);
        ArgumentNullException.ThrowIfNull(stage2);
        ArgumentNullException.ThrowIfNull(watchdog);
        ArgumentNullException.ThrowIfNull(progressCalculator);

        _logger = logger;
        _stage0 = stage0;
        _stage1 = stage1;
        _stage2 = stage2;
        _watchdog = watchdog;
        _progressCalculator = progressCalculator;
    }

    public async Task<BootMetrics> ExecuteBootSequenceAsync(CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Boot sequence started. SessionId: {SessionId}", sessionId);

        try
        {
            InitializeMetrics(sessionId, startTime);

            // Execute stages in sequence
            await ExecuteStage0Async(cancellationToken);
            await ExecuteStage1Async(cancellationToken);
            await ExecuteStage2Async(cancellationToken);

            stopwatch.Stop();
            CompleteMetrics(stopwatch.ElapsedMilliseconds, BootStatus.Success);

            _logger.LogInformation(
                "Boot sequence completed successfully. Duration: {DurationMs}ms, Memory: {MemoryMB}MB",
                stopwatch.ElapsedMilliseconds,
                _currentMetrics?.MemoryUsageMB ?? 0
            );

            return GetBootMetrics();
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Boot sequence cancelled. SessionId: {SessionId}", sessionId);
            CompleteMetrics(stopwatch.ElapsedMilliseconds, BootStatus.Cancelled, "User cancelled boot sequence");
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Boot sequence failed. SessionId: {SessionId}", sessionId);
            CompleteMetrics(stopwatch.ElapsedMilliseconds, BootStatus.Failed, ex.Message, ex.ToString());
            throw;
        }
    }

    public async Task ExecuteStage0Async(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Stage 0: Splash");
        var stageStopwatch = Stopwatch.StartNew();

        try
        {
            // Start watchdog for Stage 0 (10s timeout)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

            await _stage0.ExecuteAsync(timeoutCts.Token);
            stageStopwatch.Stop();

            UpdateStageMetrics(0, "Splash", stageStopwatch.ElapsedMilliseconds, 100, "Stage 0 complete");

            _logger.LogInformation("Stage 0 completed in {DurationMs}ms", stageStopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Stage 0 timed out after {stageStopwatch.ElapsedMilliseconds}ms");
        }
    }

    public async Task ExecuteStage1Async(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Stage 1: Services Initialization");
        var stageStopwatch = Stopwatch.StartNew();

        try
        {
            // Start watchdog for Stage 1 (60s timeout)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

            // Subscribe to progress updates
            _stage1.OnProgressChanged += (sender, args) =>
            {
                var overallProgress = _progressCalculator.CalculateProgress(1, args.Progress);
                UpdateStageMetrics(1, "Services", null, args.Progress, args.StatusMessage);
                OnProgressChanged?.Invoke(this, new BootProgressEventArgs
                {
                    StageNumber = 1,
                    StageName = "Services",
                    ProgressPercentage = overallProgress,
                    StatusMessage = args.StatusMessage
                });
            };

            await _stage1.ExecuteAsync(timeoutCts.Token);
            stageStopwatch.Stop();

            UpdateStageMetrics(1, "Services", stageStopwatch.ElapsedMilliseconds, 100, "Stage 1 complete");

            _logger.LogInformation("Stage 1 completed in {DurationMs}ms", stageStopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Stage 1 timed out after {stageStopwatch.ElapsedMilliseconds}ms");
        }
    }

    public async Task ExecuteStage2Async(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Stage 2: Application Ready");
        var stageStopwatch = Stopwatch.StartNew();

        try
        {
            // Start watchdog for Stage 2 (15s timeout)
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            // Subscribe to progress updates
            _stage2.OnProgressChanged += (sender, args) =>
            {
                var overallProgress = _progressCalculator.CalculateProgress(2, args.Progress);
                UpdateStageMetrics(2, "Application", null, args.Progress, args.StatusMessage);
                OnProgressChanged?.Invoke(this, new BootProgressEventArgs
                {
                    StageNumber = 2,
                    StageName = "Application",
                    ProgressPercentage = overallProgress,
                    StatusMessage = args.StatusMessage
                });
            };

            await _stage2.ExecuteAsync(timeoutCts.Token);
            stageStopwatch.Stop();

            UpdateStageMetrics(2, "Application", stageStopwatch.ElapsedMilliseconds, 100, "Stage 2 complete");

            _logger.LogInformation("Stage 2 completed in {DurationMs}ms", stageStopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Stage 2 timed out after {stageStopwatch.ElapsedMilliseconds}ms");
        }
    }

    public BootMetrics GetBootMetrics()
    {
        lock (_metricsLock)
        {
            if (_currentMetrics == null)
            {
                throw new InvalidOperationException("Boot sequence has not been started yet");
            }

            return _currentMetrics;
        }
    }

    private void InitializeMetrics(Guid sessionId, DateTimeOffset startTime)
    {
        lock (_metricsLock)
        {
            _currentMetrics = new BootMetrics
            {
                SessionId = sessionId,
                StartTimestamp = startTime,
                PlatformInfo = Environment.OSVersion.Platform.ToString(),
                AppVersion = typeof(BootOrchestrator).Assembly.GetName().Version?.ToString() ?? "Unknown",
                SuccessStatus = BootStatus.InProgress
            };
        }
    }

    private void UpdateStageMetrics(int stageNumber, string stageName, long? durationMs, int progress, string statusMessage)
    {
        lock (_metricsLock)
        {
            if (_currentMetrics == null)
                return;

            // Update memory usage
            var process = Process.GetCurrentProcess();
            _currentMetrics.MemoryUsageMB = (int)(process.WorkingSet64 / 1024 / 1024);

            // Update stage-specific duration
            switch (stageNumber)
            {
                case 0:
                    if (durationMs.HasValue)
                        _currentMetrics.Stage0DurationMs = durationMs.Value;
                    break;
                case 1:
                    if (durationMs.HasValue)
                        _currentMetrics.Stage1DurationMs = durationMs.Value;
                    break;
                case 2:
                    if (durationMs.HasValue)
                        _currentMetrics.Stage2DurationMs = durationMs.Value;
                    break;
            }
        }
    }

    private void CompleteMetrics(long totalDurationMs, BootStatus status, string? errorMessage = null, string? errorDetails = null)
    {
        lock (_metricsLock)
        {
            if (_currentMetrics == null)
                return;

            _currentMetrics.EndTimestamp = DateTimeOffset.UtcNow;
            _currentMetrics.TotalDurationMs = totalDurationMs;
            _currentMetrics.SuccessStatus = status;
            _currentMetrics.ErrorMessage = errorMessage;
            _currentMetrics.ErrorDetails = errorDetails;

            // Final memory measurement
            var process = Process.GetCurrentProcess();
            _currentMetrics.MemoryUsageMB = (int)(process.WorkingSet64 / 1024 / 1024);
        }
    }
}
