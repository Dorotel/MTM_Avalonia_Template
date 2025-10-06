using System;

namespace MTM_Template_Application.Models.Configuration;

/// <summary>
/// Represents a configuration error with severity classification and user action guidance.
/// Used by ErrorNotificationService to route errors to appropriate UI (status bar vs modal dialog).
/// </summary>
public class ConfigurationError
{
    /// <summary>
    /// Configuration key that caused the error
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly error message (no technical jargon)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error severity level (Info, Warning, Critical)
    /// </summary>
    public ErrorSeverity Severity { get; set; }

    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Whether the error has been resolved
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// What the user should do to resolve the error (required for Critical severity)
    /// </summary>
    public string? UserAction { get; set; }

    /// <summary>
    /// Creates a new ConfigurationError with the specified severity
    /// </summary>
    public static ConfigurationError Create(
        string key,
        string message,
        ErrorSeverity severity,
        string? userAction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (severity == ErrorSeverity.Critical && string.IsNullOrWhiteSpace(userAction))
        {
            throw new ArgumentException("UserAction is required for Critical severity errors", nameof(userAction));
        }

        return new ConfigurationError
        {
            Key = key,
            Message = message,
            Severity = severity,
            UserAction = userAction,
            Timestamp = DateTimeOffset.UtcNow,
            IsResolved = false
        };
    }
}

/// <summary>
/// Severity classification for configuration errors
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational (FYI only, no action needed)
    /// Example: Configuration changes successfully persisted
    /// Routing: Status bar (non-intrusive)
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning (non-critical issue, application can continue)
    /// Example: Invalid filter setting, using default instead
    /// Routing: Status bar with warning icon (user can click for details)
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Critical (must be resolved for application to function correctly)
    /// Example: Database connection unavailable, required credentials missing
    /// Routing: Modal dialog (blocks UI until addressed)
    /// </summary>
    Critical = 2
}
