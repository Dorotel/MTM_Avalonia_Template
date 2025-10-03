using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Logging;

namespace MTM_Template_Application.Services.Logging;

/// <summary>
/// Batch telemetry for efficient transmission
/// </summary>
public class TelemetryBatchProcessor : IDisposable
{
    private const int DefaultBatchSize = 100;
    private const int DefaultFlushIntervalMs = 5000; // 5 seconds

    private readonly List<LogEntry> _batchBuffer;
    private readonly int _batchSize;
    private readonly Timer _flushTimer;
    private readonly SemaphoreSlim _bufferLock;
    private readonly Func<IEnumerable<LogEntry>, Task> _batchHandler;
    private bool _disposed;

    public TelemetryBatchProcessor(
        Func<IEnumerable<LogEntry>, Task> batchHandler,
        int batchSize = DefaultBatchSize,
        int flushIntervalMs = DefaultFlushIntervalMs)
    {
        ArgumentNullException.ThrowIfNull(batchHandler);

        if (batchSize <= 0)
        {
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));
        }

        if (flushIntervalMs <= 0)
        {
            throw new ArgumentException("Flush interval must be positive", nameof(flushIntervalMs));
        }

        _batchHandler = batchHandler;
        _batchSize = batchSize;
        _batchBuffer = new List<LogEntry>(batchSize);
        _bufferLock = new SemaphoreSlim(1, 1);
        _flushTimer = new Timer(OnFlushTimerElapsed, null, flushIntervalMs, flushIntervalMs);
    }

    /// <summary>
    /// Add a log entry to the batch
    /// </summary>
    public async Task AddAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(entry);

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TelemetryBatchProcessor));
        }

        await _bufferLock.WaitAsync();
        try
        {
            _batchBuffer.Add(entry);

            // Flush if batch is full
            if (_batchBuffer.Count >= _batchSize)
            {
                await FlushInternalAsync();
            }
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    /// <summary>
    /// Flush all pending entries
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TelemetryBatchProcessor));
        }

        await _bufferLock.WaitAsync();
        try
        {
            await FlushInternalAsync();
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    /// <summary>
    /// Get the current buffer count
    /// </summary>
    public int GetBufferCount()
    {
        return _batchBuffer.Count;
    }

    /// <summary>
    /// Internal flush without locking (assumes lock is already held)
    /// </summary>
    private async Task FlushInternalAsync()
    {
        if (_batchBuffer.Count == 0)
        {
            return;
        }

        // Create a copy of the batch to process
        var batch = _batchBuffer.ToList();
        _batchBuffer.Clear();

        // Process the batch (release lock first to avoid blocking)
        try
        {
            await _batchHandler(batch);
        }
        catch (Exception)
        {
            // Silently ignore batch processing errors to avoid losing logs
            // In production, you might want to retry or save to disk
        }
    }

    /// <summary>
    /// Timer callback to flush periodically
    /// </summary>
    private async void OnFlushTimerElapsed(object? state)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await FlushAsync();
        }
        catch (Exception)
        {
            // Silently ignore flush errors
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Stop the timer
        _flushTimer?.Dispose();

        // Flush remaining entries
        try
        {
            FlushAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // Silently ignore flush errors during disposal
        }

        _bufferLock?.Dispose();
    }
}

/// <summary>
/// Factory for creating telemetry batch processors
/// </summary>
public static class TelemetryBatchProcessorFactory
{
    /// <summary>
    /// Create a batch processor with default settings
    /// </summary>
    public static TelemetryBatchProcessor Create(Func<IEnumerable<LogEntry>, Task> batchHandler)
    {
        ArgumentNullException.ThrowIfNull(batchHandler);
        return new TelemetryBatchProcessor(batchHandler);
    }

    /// <summary>
    /// Create a batch processor with custom settings
    /// </summary>
    public static TelemetryBatchProcessor Create(
        Func<IEnumerable<LogEntry>, Task> batchHandler,
        int batchSize,
        int flushIntervalMs)
    {
        ArgumentNullException.ThrowIfNull(batchHandler);
        return new TelemetryBatchProcessor(batchHandler, batchSize, flushIntervalMs);
    }
}
