namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Real-time system performance metrics captured at regular intervals.
/// </summary>
public sealed record PerformanceSnapshot
{
    /// <summary>
    /// Gets the timestamp when this snapshot was captured.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the CPU usage percentage (0.0 - 100.0).
    /// </summary>
    public required double CpuUsagePercent { get; init; }

    /// <summary>
    /// Gets the memory usage in megabytes (current working set).
    /// </summary>
    public required long MemoryUsageMB { get; init; }

    /// <summary>
    /// Gets the number of Generation 0 garbage collections.
    /// </summary>
    public required int GcGen0Collections { get; init; }

    /// <summary>
    /// Gets the number of Generation 1 garbage collections.
    /// </summary>
    public required int GcGen1Collections { get; init; }

    /// <summary>
    /// Gets the number of Generation 2 garbage collections.
    /// </summary>
    public required int GcGen2Collections { get; init; }

    /// <summary>
    /// Gets the number of active threads.
    /// </summary>
    public required int ThreadCount { get; init; }

    /// <summary>
    /// Gets the application uptime since boot.
    /// </summary>
    public required TimeSpan Uptime { get; init; }

    /// <summary>
    /// Validates the snapshot data against business rules.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public bool IsValid()
    {
        return CpuUsagePercent >= 0.0 && CpuUsagePercent <= 100.0
            && MemoryUsageMB >= 0
            && GcGen0Collections >= 0
            && GcGen1Collections >= 0
            && GcGen2Collections >= 0
            && ThreadCount > 0
            && Uptime >= TimeSpan.Zero;
    }
}
