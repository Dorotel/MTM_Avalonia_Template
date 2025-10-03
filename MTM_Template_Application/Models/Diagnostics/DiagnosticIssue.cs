using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Diagnostic issue requiring attention
/// </summary>
public class DiagnosticIssue
{
    /// <summary>
    /// Issue severity: Critical, High, Medium, Low
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Issue category: Storage, Network, Permission, Hardware, Configuration
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Steps to resolve the issue
    /// </summary>
    public List<string> ResolutionSteps { get; set; } = new();

    /// <summary>
    /// When the issue was detected
    /// </summary>
    public DateTimeOffset DetectedAt { get; set; }
}
