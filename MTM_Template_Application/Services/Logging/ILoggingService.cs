using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Logging;

/// <summary>
/// Structured logging service with OpenTelemetry format
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Log an informational message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Log a warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log an error message
    /// </summary>
    void LogError(string message, System.Exception? exception = null, params object[] args);

    /// <summary>
    /// Set logging context (correlation ID, trace ID, etc.)
    /// </summary>
    void SetContext(string key, object value);

    /// <summary>
    /// Flush pending log entries
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
