using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Real-time system performance monitoring service.
/// Provides CPU, memory, GC, and thread metrics for the Debug Terminal.
/// </summary>
public sealed class PerformanceMonitoringService : IPerformanceMonitoringService, IDisposable
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Queue<PerformanceSnapshot> _snapshotBuffer = new();
    private readonly object _bufferLock = new();
    private readonly int _maxBufferSize = 100;

    private Process? _currentProcess;
    private DateTime? _lastCpuTime;
    private TimeSpan? _lastTotalProcessorTime;
    private bool _isMonitoring;
    private bool _disposed;

    public bool IsMonitoring
    {
        get
        {
            lock (_bufferLock)
            {
                return _isMonitoring;
            }
        }
    }

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _currentProcess = Process.GetCurrentProcess();
    }

    /// <inheritdoc/>
    public async Task<PerformanceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var process = _currentProcess ?? Process.GetCurrentProcess();

            // Calculate CPU usage
            var cpuUsage = await CalculateCpuUsageAsync(process, cancellationToken);

            // Get memory usage in MB
            var memoryUsageMB = process.WorkingSet64 / 1024 / 1024;

            // Get GC collection counts
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            // Get thread count
            var threadCount = process.Threads.Count;

            // Calculate uptime
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

            var snapshot = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = Math.Clamp(cpuUsage, 0.0, 100.0),
                MemoryUsageMB = memoryUsageMB,
                GcGen0Collections = gen0,
                GcGen1Collections = gen1,
                GcGen2Collections = gen2,
                ThreadCount = threadCount,
                Uptime = uptime
            };

            return snapshot;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to capture performance snapshot");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PerformanceSnapshot>> GetRecentSnapshotsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count < 1 || count > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be between 1 and 100");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await Task.CompletedTask; // Async consistency

        lock (_bufferLock)
        {
            return _snapshotBuffer
                .TakeLast(count)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public async Task StartMonitoringAsync(
        TimeSpan interval,
        CancellationToken cancellationToken = default)
    {
        // Validate interval per CL-002 (1-30 seconds)
        if (interval < TimeSpan.FromSeconds(1) || interval > TimeSpan.FromSeconds(30))
        {
            throw new ArgumentOutOfRangeException(nameof(interval),
                "Monitoring interval must be between 1 and 30 seconds (per CL-002)");
        }

        lock (_bufferLock)
        {
            if (_isMonitoring)
            {
                throw new InvalidOperationException(
                    "Monitoring is already started. Call StopMonitoringAsync first.");
            }

            _isMonitoring = true;
        }

        _logger.LogInformation("Starting performance monitoring with {Interval}s interval",
            interval.TotalSeconds);

        try
        {
            // Monitor until cancellation
            while (!cancellationToken.IsCancellationRequested)
            {
                await CaptureSnapshotAsync(cancellationToken);
                await Task.Delay(interval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Performance monitoring cancelled");
            throw; // Re-throw to satisfy test expectations
        }
        finally
        {
            lock (_bufferLock)
            {
                _isMonitoring = false;
            }
        }
    }

    /// <inheritdoc/>
    public async Task StopMonitoringAsync()
    {
        lock (_bufferLock)
        {
            if (!_isMonitoring)
            {
                return; // Already stopped
            }

            _isMonitoring = false;
        }

        _logger.LogInformation("Stopping performance monitoring");

        await Task.CompletedTask;
    }

    private async Task CaptureSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await GetCurrentSnapshotAsync(cancellationToken);

            lock (_bufferLock)
            {
                _snapshotBuffer.Enqueue(snapshot);

                // Maintain circular buffer (max 100 entries)
                while (_snapshotBuffer.Count > _maxBufferSize)
                {
                    _snapshotBuffer.Dequeue();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Performance snapshot capture cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing performance snapshot");
        }
    }

    private async Task<double> CalculateCpuUsageAsync(Process process, CancellationToken cancellationToken)
    {
        try
        {
            var currentTime = DateTime.UtcNow;
            var currentTotalProcessorTime = process.TotalProcessorTime;

            // Need at least two samples to calculate CPU usage
            if (_lastCpuTime.HasValue && _lastTotalProcessorTime.HasValue)
            {
                var timeDiff = (currentTime - _lastCpuTime.Value).TotalMilliseconds;
                var processorTimeDiff = (currentTotalProcessorTime - _lastTotalProcessorTime.Value).TotalMilliseconds;

                if (timeDiff > 0)
                {
                    var cpuUsage = (processorTimeDiff / (timeDiff * Environment.ProcessorCount)) * 100.0;

                    _lastCpuTime = currentTime;
                    _lastTotalProcessorTime = currentTotalProcessorTime;

                    return Math.Clamp(cpuUsage, 0.0, 100.0);
                }
            }

            // First sample - store for next calculation
            _lastCpuTime = currentTime;
            _lastTotalProcessorTime = currentTotalProcessorTime;

            await Task.CompletedTask; // Async consistency
            return 0.0; // Return 0 for first sample
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate CPU usage");
            return 0.0;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Stop monitoring if active
        lock (_bufferLock)
        {
            _isMonitoring = false;
        }

        _currentProcess?.Dispose();

        _disposed = true;
    }
}
