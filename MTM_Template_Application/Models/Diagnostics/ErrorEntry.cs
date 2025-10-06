namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Structured error history entry with categorization and recovery suggestions.
/// </summary>
public sealed record ErrorEntry
{
    /// <summary>
    /// Gets the unique identifier for this error entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the severity level of the error.
    /// </summary>
    public required ErrorSeverity Severity { get; init; }

    /// <summary>
    /// Gets the error category (e.g., "Database", "Network", "Validation").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the stack trace if available.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets the recovery suggestion for this error.
    /// </summary>
    public string? RecoverySuggestion { get; init; }

    /// <summary>
    /// Gets additional context data as key-value pairs.
    /// </summary>
    public required Dictionary<string, string> ContextData { get; init; }

    /// <summary>
    /// Validates the error entry data.
    /// </summary>
    public bool IsValid()
    {
        return Id != Guid.Empty
            && !string.IsNullOrWhiteSpace(Category)
            && !string.IsNullOrWhiteSpace(Message)
            && ContextData != null;
    }
}

/// <summary>
/// Error severity levels.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message (not an error).
    /// </summary>
    Info,

    /// <summary>
    /// Warning - potential issue but operation continues.
    /// </summary>
    Warning,

    /// <summary>
    /// Error - operation failed but application continues.
    /// </summary>
    Error,

    /// <summary>
    /// Critical - severe error requiring immediate attention.
    /// </summary>
    Critical
}
