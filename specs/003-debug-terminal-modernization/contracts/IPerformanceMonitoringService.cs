// Service Contract: Real-Time Performance Monitoring
// Spec: specs/003-debug-terminal-modernization/spec.md
// Feature: Real-Time Performance Monitoring (Phase 1)

namespace MTM_Template_Application.Services;

/// <summary>
/// Real-time system performance monitoring service.
/// Provides CPU, memory, GC, and thread metrics for the Debug Terminal.
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Gets the current performance snapshot with all metrics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current performance snapshot</returns>
    /// <exception cref="OperationCanceledException">Thrown if operation is cancelled</exception>
    Task<PerformanceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last N performance snapshots from the circular buffer.
    /// </summary>
    /// <param name="count">Number of snapshots to retrieve (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Read-only list of recent snapshots, ordered chronologically</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if count &lt; 1 or count &gt; 100</exception>
    Task<IReadOnlyList<PerformanceSnapshot>> GetRecentSnapshotsAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts continuous performance monitoring at the specified interval.
    /// </summary>
    /// <param name="interval">Refresh interval (1-30 seconds per CL-002)</param>
    /// <param name="cancellationToken">Cancellation token to stop monitoring</param>
    /// <returns>Task that runs until cancelled</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if interval &lt; 1s or interval &gt; 30s</exception>
    /// <exception cref="InvalidOperationException">Thrown if monitoring is already started</exception>
    Task StartMonitoringAsync(TimeSpan interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops continuous performance monitoring.
    /// </summary>
    /// <returns>Task that completes when monitoring is stopped</returns>
    Task StopMonitoringAsync();

    /// <summary>
    /// Gets whether monitoring is currently active.
    /// </summary>
    bool IsMonitoring { get; }
}
