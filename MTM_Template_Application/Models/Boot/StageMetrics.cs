using System;

namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Detailed timing for each boot stage
/// </summary>
public class StageMetrics
{
    /// <summary>
    /// Foreign key to BootMetrics
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Stage number: 0, 1, or 2
    /// </summary>
    public int StageNumber { get; set; }

    /// <summary>
    /// Stage name: Splash, Services, Application
    /// </summary>
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Stage start time
    /// </summary>
    public DateTimeOffset StartTimestamp { get; set; }

    /// <summary>
    /// Stage end time
    /// </summary>
    public DateTimeOffset? EndTimestamp { get; set; }

    /// <summary>
    /// Stage duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Progress through stage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Current status message shown to user
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Number of operations finished
    /// </summary>
    public int OperationsCompleted { get; set; }

    /// <summary>
    /// Total operations in stage
    /// </summary>
    public int OperationsTotal { get; set; }
}
