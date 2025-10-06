using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Service for exporting diagnostic data to JSON or Markdown formats.
/// Handles PII sanitization and large log file handling.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Creates a diagnostic export package with all available data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Diagnostic export object ready for serialization</returns>
    /// <remarks>
    /// Sanitizes PII (passwords, tokens, connection strings).
    /// Includes current performance, boot timeline, recent errors, connection stats, env vars, and logs.
    /// </remarks>
    Task<DiagnosticExport> CreateExportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports diagnostics to JSON format.
    /// </summary>
    /// <param name="filePath">Output file path (must be writable)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="progress">Progress reporter for large exports (optional)</param>
    /// <returns>Number of bytes written</returns>
    /// <exception cref="ArgumentException">Thrown if filePath is invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if filePath is not writable</exception>
    /// <exception cref="OperationCanceledException">Thrown if operation is cancelled</exception>
    Task<long> ExportToJsonAsync(
        string filePath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null);

    /// <summary>
    /// Exports diagnostics to Markdown format.
    /// </summary>
    /// <param name="filePath">Output file path (must be writable)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="progress">Progress reporter for large exports (optional)</param>
    /// <returns>Number of bytes written</returns>
    /// <exception cref="ArgumentException">Thrown if filePath is invalid</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if filePath is not writable</exception>
    /// <exception cref="OperationCanceledException">Thrown if operation is cancelled</exception>
    /// <remarks>
    /// Markdown format is optional per CL-003 (JSON is priority).
    /// </remarks>
    Task<long> ExportToMarkdownAsync(
        string filePath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null);

    /// <summary>
    /// Validates that the export file path is writable.
    /// </summary>
    /// <param name="filePath">File path to validate</param>
    /// <returns>True if path is valid and writable, false otherwise</returns>
    bool ValidateExportPath(string filePath);

    /// <summary>
    /// Sanitizes PII from a string value.
    /// </summary>
    /// <param name="value">String to sanitize</param>
    /// <returns>Sanitized string with PII masked</returns>
    /// <remarks>
    /// Masks: passwords, tokens, connection strings, API keys, email addresses.
    /// Uses patterns like "password=***", "token=***", etc.
    /// </remarks>
    string SanitizePii(string value);
}
