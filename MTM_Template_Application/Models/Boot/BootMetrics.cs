using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Captures telemetry data during startup for monitoring and performance optimization
/// </summary>
public class BootMetrics
{
    /// <summary>
    /// Unique identifier for boot session
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// When boot sequence began
    /// </summary>
    public DateTimeOffset StartTimestamp { get; set; }

    /// <summary>
    /// When boot sequence completed (null if in progress/failed)
    /// </summary>
    public DateTimeOffset? EndTimestamp { get; set; }

    /// <summary>
    /// Total boot time in milliseconds
    /// </summary>
    public long? TotalDurationMs { get; set; }

    /// <summary>
    /// Splash screen initialization duration
    /// </summary>
    public long Stage0DurationMs { get; set; }

    /// <summary>
    /// Services initialization duration
    /// </summary>
    public long? Stage1DurationMs { get; set; }

    /// <summary>
    /// Application initialization duration
    /// </summary>
    public long? Stage2DurationMs { get; set; }

    /// <summary>
    /// Boot status: Success, Failed, Cancelled, Timeout
    /// </summary>
    public BootStatus SuccessStatus { get; set; }

    /// <summary>
    /// Error category if failed
    /// </summary>
    public ErrorCategory? ErrorCategory { get; set; }

    /// <summary>
    /// User-friendly error description
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Technical error details
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Peak memory usage during boot in MB
    /// </summary>
    public int MemoryUsageMB { get; set; }

    /// <summary>
    /// Platform information: Windows, macOS, Linux, Android
    /// </summary>
    public string PlatformInfo { get; set; } = string.Empty;

    /// <summary>
    /// Application version
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// Detailed metrics for each boot stage
    /// </summary>
    public List<StageMetrics> StageMetrics { get; set; } = new();

    /// <summary>
    /// Metrics for each initialized service
    /// </summary>
    public List<ServiceMetrics> ServiceMetrics { get; set; } = new();
}

/// <summary>
/// Boot status enumeration
/// </summary>
public enum BootStatus
{
    InProgress,
    Success,
    Failed,
    Cancelled,
    Timeout
}

/// <summary>
/// Error category enumeration
/// </summary>
public enum ErrorCategory
{
    Transient,
    Configuration,
    Network,
    Permission,
    Permanent
}
