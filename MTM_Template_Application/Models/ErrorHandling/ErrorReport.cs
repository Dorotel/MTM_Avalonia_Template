using System;

namespace MTM_Template_Application.Models.ErrorHandling;

/// <summary>
/// Error report with diagnostic information
/// </summary>
public class ErrorReport
{
    /// <summary>
    /// Unique error identifier
    /// </summary>
    public Guid ErrorId { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Error severity: Critical, High, Medium, Low
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Error category: Transient, Configuration, Network, Permission, Permanent
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// Optional diagnostic bundle with additional context
    /// </summary>
    public string? DiagnosticBundle { get; set; }
}
