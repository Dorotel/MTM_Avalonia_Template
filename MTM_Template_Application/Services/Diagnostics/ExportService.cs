using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Service for exporting diagnostic data to JSON or Markdown formats.
/// Handles PII sanitization and large file exports with progress reporting.
/// </summary>
public partial class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly IDiagnosticsServiceExtensions _diagnosticsService;

    // PII sanitization regex patterns (compiled for performance)
    [GeneratedRegex(@"password[:\s=]*[^\s,;\n]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"(token|key|secret|apikey|api_key)[:\s=]*[^\s,;\n]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(connection|connectionstring)[:\s=]*[^\n;]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ConnectionStringRegex();

    public ExportService(
        ILogger<ExportService> logger,
        IPerformanceMonitoringService performanceService,
        IDiagnosticsServiceExtensions diagnosticsService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(performanceService);
        ArgumentNullException.ThrowIfNull(diagnosticsService);

        _logger = logger;
        _performanceService = performanceService;
        _diagnosticsService = diagnosticsService;
    }

    /// <inheritdoc/>
    public async Task<DiagnosticExport> CreateExportAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating diagnostic export package");

        try
        {
            // Gather all diagnostic data
            var currentPerformance = await _performanceService.GetCurrentSnapshotAsync(cancellationToken);
            var recentPerformance = await _performanceService.GetRecentSnapshotsAsync(100, cancellationToken);
            var bootTimeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);
            var recentErrors = await _diagnosticsService.GetRecentErrorsAsync(100, cancellationToken);
            var connectionStats = await _diagnosticsService.GetConnectionPoolStatsAsync(cancellationToken);

            // Get environment variables and sanitize PII
            var envVars = Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .ToDictionary(
                    e => e.Key.ToString() ?? string.Empty,
                    e => SanitizeEnvironmentVariable(
                        e.Key.ToString() ?? string.Empty,
                        e.Value?.ToString() ?? string.Empty)
                );

            // Collect system information
            var platform = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                ? "Windows"
                : "Android";

            // Get application version from assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

            // Create export package - provide default snapshot if current performance is null
            var export = new DiagnosticExport
            {
                ExportTime = DateTime.UtcNow,
                ApplicationVersion = version,
                Platform = platform,
                CurrentPerformance = currentPerformance ?? new PerformanceSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsagePercent = 0,
                    MemoryUsageMB = 0,
                    GcGen0Collections = 0,
                    GcGen1Collections = 0,
                    GcGen2Collections = 0,
                    ThreadCount = 0,
                    Uptime = TimeSpan.Zero
                },
                BootTimeline = bootTimeline,
                RecentErrors = recentErrors.ToList(),
                ConnectionStats = connectionStats,
                EnvironmentVariables = envVars,
                RecentLogEntries = new List<string>() // Placeholder - will be populated when log viewer is implemented
            };

            // Validate export before returning
            if (!export.IsValid())
            {
                _logger.LogWarning("Created diagnostic export failed validation");
            }

            _logger.LogInformation("Diagnostic export package created successfully with {ErrorCount} errors",
                export.RecentErrors.Count);

            return export;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Diagnostic export creation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create diagnostic export package");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<long> ExportToJsonAsync(
        string filePath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (!ValidateExportPath(filePath))
        {
            throw new ArgumentException("File path is invalid or not writable", nameof(filePath));
        }

        _logger.LogInformation("Exporting diagnostics to JSON: {FilePath}", filePath);

        try
        {
            // Report progress: Starting
            progress?.Report(0);

            // Create diagnostic export
            var export = await CreateExportAsync(cancellationToken);

            // Report progress: Data collected
            progress?.Report(25);

            // Serialize to JSON with indentation for readability
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            // Report progress: Serializing
            progress?.Report(50);

            // Use background thread for serialization if export is large
            string jsonContent;
            await using (var memoryStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(memoryStream, export, jsonOptions, cancellationToken);
                memoryStream.Position = 0;
                using var reader = new StreamReader(memoryStream);
                jsonContent = await reader.ReadToEndAsync(cancellationToken);
            }

            // Report progress: Writing to file
            progress?.Report(75);

            // Write to file asynchronously
            await File.WriteAllTextAsync(filePath, jsonContent, Encoding.UTF8, cancellationToken);

            var fileInfo = new FileInfo(filePath);
            var bytesWritten = fileInfo.Length;

            // Report progress: Complete
            progress?.Report(100);

            _logger.LogInformation("Exported {BytesWritten} bytes to JSON: {FilePath}", bytesWritten, filePath);

            return bytesWritten;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("JSON export cancelled: {FilePath}", filePath);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access to file: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export diagnostics to JSON: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<long> ExportToMarkdownAsync(
        string filePath,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (!ValidateExportPath(filePath))
        {
            throw new ArgumentException("File path is invalid or not writable", nameof(filePath));
        }

        _logger.LogInformation("Exporting diagnostics to Markdown: {FilePath}", filePath);

        try
        {
            // Report progress: Starting
            progress?.Report(0);

            // Create diagnostic export
            var export = await CreateExportAsync(cancellationToken);

            // Report progress: Data collected
            progress?.Report(25);

            // Build Markdown content
            var markdown = new StringBuilder();
            markdown.AppendLine("# Diagnostic Export");
            markdown.AppendLine();
            markdown.AppendLine($"**Exported At**: {export.ExportTime:yyyy-MM-dd HH:mm:ss UTC}");
            markdown.AppendLine($"**Platform**: {export.Platform}");
            markdown.AppendLine($"**Application Version**: {export.ApplicationVersion}");
            markdown.AppendLine();

            // Report progress: Building Markdown sections
            progress?.Report(40);

            // Performance Section
            if (export.CurrentPerformance != null)
            {
                markdown.AppendLine("## Current Performance");
                markdown.AppendLine();
                markdown.AppendLine($"- **CPU Usage**: {export.CurrentPerformance.CpuUsagePercent:F2}%");
                markdown.AppendLine($"- **Memory Usage**: {export.CurrentPerformance.MemoryUsageMB:F2} MB");
                markdown.AppendLine($"- **Thread Count**: {export.CurrentPerformance.ThreadCount}");
                markdown.AppendLine($"- **Uptime**: {export.CurrentPerformance.Uptime}");
                markdown.AppendLine($"- **GC Collections**: Gen0={export.CurrentPerformance.GcGen0Collections}, Gen1={export.CurrentPerformance.GcGen1Collections}, Gen2={export.CurrentPerformance.GcGen2Collections}");
                markdown.AppendLine();
            }

            progress?.Report(55);

            // Boot Timeline Section
            if (export.BootTimeline != null)
            {
                markdown.AppendLine("## Boot Timeline");
                markdown.AppendLine();
                markdown.AppendLine($"- **Total Boot Time**: {export.BootTimeline.TotalBootTime.TotalSeconds:F2}s");
                markdown.AppendLine($"- **Stage 0 Duration**: {export.BootTimeline.Stage0.Duration.TotalSeconds:F2}s");
                markdown.AppendLine($"- **Stage 1 Duration**: {export.BootTimeline.Stage1.Duration.TotalSeconds:F2}s");
                markdown.AppendLine($"- **Stage 2 Duration**: {export.BootTimeline.Stage2.Duration.TotalSeconds:F2}s");
                markdown.AppendLine();
            }

            progress?.Report(70);

            // Recent Errors Section
            if (export.RecentErrors.Count > 0)
            {
                markdown.AppendLine("## Recent Errors");
                markdown.AppendLine();
                foreach (var error in export.RecentErrors.Take(10)) // Top 10 errors
                {
                    markdown.AppendLine($"### {error.Severity}: {error.Category}");
                    markdown.AppendLine();
                    markdown.AppendLine($"**Message**: {error.Message}");
                    markdown.AppendLine($"**Timestamp**: {error.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    if (!string.IsNullOrWhiteSpace(error.RecoverySuggestion))
                    {
                        markdown.AppendLine($"**Recovery**: {error.RecoverySuggestion}");
                    }
                    markdown.AppendLine();
                }
            }

            progress?.Report(85);

            // Connection Stats Section
            if (export.ConnectionStats != null)
            {
                markdown.AppendLine("## Connection Pool Statistics");
                markdown.AppendLine();
                markdown.AppendLine("### MySQL Pool");
                markdown.AppendLine($"- **Total Connections**: {export.ConnectionStats.MySqlPool.TotalConnections}");
                markdown.AppendLine($"- **Active Connections**: {export.ConnectionStats.MySqlPool.ActiveConnections}");
                markdown.AppendLine($"- **Idle Connections**: {export.ConnectionStats.MySqlPool.IdleConnections}");
                markdown.AppendLine($"- **Waiting Requests**: {export.ConnectionStats.MySqlPool.WaitingRequests}");
                markdown.AppendLine();
                markdown.AppendLine("### HTTP Pool");
                markdown.AppendLine($"- **Total Connections**: {export.ConnectionStats.HttpPool.TotalConnections}");
                markdown.AppendLine($"- **Active Connections**: {export.ConnectionStats.HttpPool.ActiveConnections}");
                markdown.AppendLine($"- **Idle Connections**: {export.ConnectionStats.HttpPool.IdleConnections}");
                markdown.AppendLine();
            }

            // Write to file
            progress?.Report(90);
            await File.WriteAllTextAsync(filePath, markdown.ToString(), Encoding.UTF8, cancellationToken);

            var fileInfo = new FileInfo(filePath);
            var bytesWritten = fileInfo.Length;

            progress?.Report(100);

            _logger.LogInformation("Exported {BytesWritten} bytes to Markdown: {FilePath}", bytesWritten, filePath);

            return bytesWritten;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Markdown export cancelled: {FilePath}", filePath);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access to file: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export diagnostics to Markdown: {FilePath}", filePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool ValidateExportPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            // Check if the path is valid
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);

            // If directory doesn't exist, check if parent exists and is writable
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                var parentDir = Directory.GetParent(directory);
                if (parentDir == null || !parentDir.Exists)
                {
                    return false;
                }

                // Check if we can create the directory
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch
                {
                    return false;
                }
            }

            // Check if file already exists and is writable
            if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                return !fileInfo.IsReadOnly;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public string SanitizePii(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        // Apply PII sanitization patterns
        var sanitized = value;

        // Sanitize passwords
        sanitized = PasswordRegex().Replace(sanitized, "password: [REDACTED]");

        // Sanitize tokens, keys, secrets
        sanitized = TokenRegex().Replace(sanitized, match =>
        {
            var prefix = match.Groups[1].Value.ToLowerInvariant();
            return $"{prefix}: [REDACTED]";
        });

        // Sanitize email addresses
        sanitized = EmailRegex().Replace(sanitized, "[EMAIL_REDACTED]");

        // Sanitize connection strings
        sanitized = ConnectionStringRegex().Replace(sanitized, match =>
        {
            var prefix = match.Groups[1].Value.ToLowerInvariant();
            return $"{prefix}: [REDACTED]";
        });

        return sanitized;
    }

    /// <summary>
    /// Sanitizes environment variable values based on the key name.
    /// Redacts values for keys containing sensitive keywords.
    /// </summary>
    private string SanitizeEnvironmentVariable(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        // Check if key contains sensitive keywords
        var lowerKey = key.ToLowerInvariant();
        var sensitiveKeywords = new[] { "password", "pwd", "token", "key", "secret", "apikey", "api_key" };

        if (sensitiveKeywords.Any(keyword => lowerKey.Contains(keyword)))
        {
            return "[REDACTED]";
        }

        // Check if value looks like an email
        if (EmailRegex().IsMatch(value))
        {
            return "[EMAIL_REDACTED]";
        }

        // Apply general PII sanitization to the value
        return SanitizePii(value);
    }
}
