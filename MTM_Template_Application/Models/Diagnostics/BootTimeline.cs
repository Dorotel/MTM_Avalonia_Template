namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Boot sequence breakdown showing Stage 0 (splash), Stage 1 (services), Stage 2 (app ready) timings.
/// </summary>
public sealed record BootTimeline
{
    /// <summary>
    /// Gets the timestamp when boot started.
    /// </summary>
    public required DateTime BootStartTime { get; init; }

    /// <summary>
    /// Gets Stage 0 (splash screen) information.
    /// </summary>
    public required Stage0Info Stage0 { get; init; }

    /// <summary>
    /// Gets Stage 1 (service initialization) information.
    /// </summary>
    public required Stage1Info Stage1 { get; init; }

    /// <summary>
    /// Gets Stage 2 (application ready) information.
    /// </summary>
    public required Stage2Info Stage2 { get; init; }

    /// <summary>
    /// Gets the total boot time (sum of all stages).
    /// </summary>
    public required TimeSpan TotalBootTime { get; init; }

    /// <summary>
    /// Validates that TotalBootTime equals the sum of all stage durations.
    /// </summary>
    public bool IsValid()
    {
        var calculatedTotal = Stage0.Duration + Stage1.Duration + Stage2.Duration;
        return Math.Abs((TotalBootTime - calculatedTotal).TotalMilliseconds) < 1.0; // Allow 1ms tolerance for rounding
    }
}

/// <summary>
/// Stage 0 (splash screen) boot information.
/// </summary>
public sealed record Stage0Info
{
    /// <summary>
    /// Gets the duration of Stage 0.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets a value indicating whether Stage 0 completed successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the error message if Stage 0 failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Stage 1 (service initialization) boot information.
/// </summary>
public sealed record Stage1Info
{
    /// <summary>
    /// Gets the duration of Stage 1.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets a value indicating whether Stage 1 completed successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the service initialization timings.
    /// </summary>
    public required List<ServiceInitInfo> ServiceTimings { get; init; }

    /// <summary>
    /// Gets the error message if Stage 1 failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Stage 2 (application ready) boot information.
/// </summary>
public sealed record Stage2Info
{
    /// <summary>
    /// Gets the duration of Stage 2.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets a value indicating whether Stage 2 completed successfully.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the error message if Stage 2 failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Individual service initialization timing information.
/// </summary>
public sealed record ServiceInitInfo
{
    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Gets the duration of service initialization.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets a value indicating whether the service initialized successfully.
    /// </summary>
    public required bool Success { get; init; }
}
