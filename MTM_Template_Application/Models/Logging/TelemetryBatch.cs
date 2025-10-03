using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Logging;

/// <summary>
/// Batch of telemetry entries for efficient transmission
/// </summary>
public class TelemetryBatch
{
    /// <summary>
    /// Unique batch identifier
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Log entries in this batch
    /// </summary>
    public List<LogEntry> Entries { get; set; } = new();

    /// <summary>
    /// When this batch was created
    /// </summary>
    public DateTimeOffset CreatedUtc { get; set; }

    /// <summary>
    /// Batch status: Pending, Sent, Failed
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
