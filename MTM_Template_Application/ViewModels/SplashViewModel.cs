using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Boot;

namespace MTM_Template_Application.ViewModels;

/// <summary>
/// ViewModel for the splash screen during application boot.
/// Displays boot progress, stage information, and status messages.
/// </summary>
public partial class SplashViewModel : ObservableObject
{
    private readonly ILogger<SplashViewModel> _logger;
    private readonly IBootOrchestrator _bootOrchestrator;
    private CancellationTokenSource? _bootCancellationTokenSource;
    private readonly List<string> _bootLogs = new();
    private long _bootStartTicks;
    private long _bootDurationMs;
    private double _bootMemoryMB;

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private string _statusMessage = "Starting application...";

    [ObservableProperty]
    private int _currentStage;

    [ObservableProperty]
    private string _stageName = "Initializing";

    [ObservableProperty]
    private string _timeRemaining = string.Empty;

    [ObservableProperty]
    private bool _isBootInProgress = true;

    [ObservableProperty]
    private bool _canCancel = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public SplashViewModel(
        ILogger<SplashViewModel> logger,
        IBootOrchestrator bootOrchestrator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bootOrchestrator);

        _logger = logger;
        _bootOrchestrator = bootOrchestrator;

        // Subscribe to boot progress updates
        _bootOrchestrator.OnProgressChanged += OnBootProgressChanged;
    }

    /// <summary>
    /// Start the boot sequence.
    /// </summary>
    public async Task StartBootSequenceAsync()
    {
        _logger.LogInformation("Starting boot sequence from SplashViewModel");
        _bootLogs.Clear();
        _bootStartTicks = DateTime.UtcNow.Ticks;
        AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Boot sequence started");

        _bootCancellationTokenSource = new CancellationTokenSource();
        IsBootInProgress = true;
        CanCancel = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var bootMetrics = await _bootOrchestrator.ExecuteBootSequenceAsync(_bootCancellationTokenSource.Token);

            _bootDurationMs = bootMetrics.TotalDurationMs ?? 0;
            _bootMemoryMB = bootMetrics.MemoryUsageMB;

            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Boot completed successfully");
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Duration: {_bootDurationMs}ms");
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Memory: {_bootMemoryMB:F1}MB");
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Status: {bootMetrics.SuccessStatus}");

            _logger.LogInformation(
                "Boot sequence completed. Duration: {DurationMs}ms, Status: {Status}",
                bootMetrics.TotalDurationMs,
                bootMetrics.SuccessStatus
            );

            // Boot completed successfully
            IsBootInProgress = false;
            ProgressPercentage = 100;
            StatusMessage = "Application ready!";
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Boot sequence cancelled by user");
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: Boot cancelled by user");
            IsBootInProgress = false;
            HasError = true;
            ErrorMessage = "Boot sequence cancelled by user";
            StatusMessage = "Cancelled";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Boot sequence failed");
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {ex.Message}");
            IsBootInProgress = false;
            HasError = true;
            ErrorMessage = ex.Message;
            StatusMessage = "Boot failed - please restart the application";
        }
    }

    /// <summary>
    /// Add a log entry to the boot log collection.
    /// </summary>
    private void AddBootLog(string message)
    {
        _bootLogs.Add(message);
    }

    /// <summary>
    /// Get all collected boot logs.
    /// </summary>
    public IReadOnlyList<string> GetBootLogs() => _bootLogs.AsReadOnly();

    /// <summary>
    /// Get boot duration in milliseconds.
    /// </summary>
    public long GetBootDurationMs() => _bootDurationMs;

    /// <summary>
    /// Get boot memory usage in MB.
    /// </summary>
    public double GetBootMemoryMB() => _bootMemoryMB;

    /// <summary>
    /// Cancel the boot sequence (if user requests).
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelBoot))]
    private void CancelBoot()
    {
        _logger.LogWarning("User requested boot cancellation");

        if (_bootCancellationTokenSource != null && !_bootCancellationTokenSource.IsCancellationRequested)
        {
            _bootCancellationTokenSource.Cancel();
            CanCancel = false;
            StatusMessage = "Cancelling...";
        }
    }

    private bool CanCancelBoot()
    {
        return IsBootInProgress && CanCancel;
    }

    /// <summary>
    /// Handle boot progress updates from orchestrator.
    /// </summary>
    private void OnBootProgressChanged(object? sender, BootProgressEventArgs e)
    {
        ProgressPercentage = e.ProgressPercentage;
        StatusMessage = e.StatusMessage;

        // Update stage information based on progress
        if (e.ProgressPercentage <= 10)
        {
            CurrentStage = 0;
            StageName = "Stage 0: Splash";
            if (e.ProgressPercentage == 0 || string.IsNullOrEmpty(e.StatusMessage) == false)
            {
                AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Stage 0: {e.StatusMessage}");
            }
        }
        else if (e.ProgressPercentage <= 75)
        {
            CurrentStage = 1;
            StageName = "Stage 1: Services";
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Stage 1: {e.StatusMessage}");
        }
        else
        {
            CurrentStage = 2;
            StageName = "Stage 2: Application";
            AddBootLog($"[{DateTime.Now:HH:mm:ss.fff}] Stage 2: {e.StatusMessage}");
        }

        _logger.LogDebug(
            "Boot progress: {Progress}% - {Status} (Stage {Stage})",
            ProgressPercentage,
            StatusMessage,
            CurrentStage
        );

        // Notify command executability changed (disable cancel if near completion)
        if (e.ProgressPercentage > 90)
        {
            CanCancel = false;
            CancelBootCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// Cleanup when ViewModel is disposed.
    /// </summary>
    public void Dispose()
    {
        _bootOrchestrator.OnProgressChanged -= OnBootProgressChanged;
        _bootCancellationTokenSource?.Dispose();
    }
}
