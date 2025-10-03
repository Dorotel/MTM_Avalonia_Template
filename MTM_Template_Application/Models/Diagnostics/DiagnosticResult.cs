using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Result of a diagnostic check
/// </summary>
public class DiagnosticResult
{
    /// <summary>
    /// Name of the diagnostic check
    /// </summary>
    public string CheckName { get; set; } = string.Empty;

    /// <summary>
    /// Check status: Pass, Warning, Fail
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the check
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// When the check was performed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// How long the check took in milliseconds
    /// </summary>
    public long DurationMs { get; set; }
}
