namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Complete diagnostic package for export (JSON or Markdown).
/// </summary>
public sealed record DiagnosticExport
{
    /// <summary>
    /// Gets the timestamp when this export was created.
    /// </summary>
    public required DateTime ExportTime { get; init; }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    public required string ApplicationVersion { get; init; }

    /// <summary>
    /// Gets the platform ("Windows" or "Android").
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>
    /// Gets the current performance snapshot.
    /// </summary>
    public required PerformanceSnapshot CurrentPerformance { get; init; }

    /// <summary>
    /// Gets the boot timeline (may be null if boot not yet completed).
    /// </summary>
    public BootTimeline? BootTimeline { get; init; }

    /// <summary>
    /// Gets recent error entries.
    /// </summary>
    public required List<ErrorEntry> RecentErrors { get; init; }

    /// <summary>
    /// Gets connection pool statistics (may be null if unavailable).
    /// </summary>
    public ConnectionPoolStats? ConnectionStats { get; init; }

    /// <summary>
    /// Gets environment variables (with PII redacted).
    /// </summary>
    public required Dictionary<string, string> EnvironmentVariables { get; init; }

    /// <summary>
    /// Gets recent log entries (with PII redacted).
    /// </summary>
    public required List<string> RecentLogEntries { get; init; }

    /// <summary>
    /// Validates the diagnostic export data.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApplicationVersion)
            && (Platform == "Windows" || Platform == "Android")
            && RecentErrors != null
            && EnvironmentVariables != null
            && RecentLogEntries != null;
    }
}
