using System;

namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Individual service initialization timing
/// </summary>
public class ServiceMetrics
{
    /// <summary>
    /// Foreign key to BootMetrics
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Name of service (e.g., "ConfigurationService", "LoggingService")
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// When service initialization began
    /// </summary>
    public DateTimeOffset StartTimestamp { get; set; }

    /// <summary>
    /// When service initialization completed
    /// </summary>
    public DateTimeOffset? EndTimestamp { get; set; }

    /// <summary>
    /// Initialization duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Whether initialization succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time spent waiting for dependencies in milliseconds
    /// </summary>
    public long DependenciesWaitMs { get; set; }
}
