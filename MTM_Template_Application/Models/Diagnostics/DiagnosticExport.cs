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
    /// Throws ArgumentException if validation fails.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void IsValid()
    {
        if (string.IsNullOrWhiteSpace(ApplicationVersion))
        {
            throw new ArgumentException("Application version cannot be empty", nameof(ApplicationVersion));
        }

        if (string.IsNullOrWhiteSpace(Platform))
        {
            throw new ArgumentException("Platform cannot be empty", nameof(Platform));
        }

        if (Platform != "Windows" && Platform != "Android")
        {
            throw new ArgumentException(
                $"Platform must be 'Windows' or 'Android', got '{Platform}'",
                nameof(Platform)
            );
        }

        if (RecentErrors == null)
        {
            throw new ArgumentException("RecentErrors cannot be null", nameof(RecentErrors));
        }

        if (EnvironmentVariables == null)
        {
            throw new ArgumentException("EnvironmentVariables cannot be null", nameof(EnvironmentVariables));
        }

        if (RecentLogEntries == null)
        {
            throw new ArgumentException("RecentLogEntries cannot be null", nameof(RecentLogEntries));
        }
    }
}
