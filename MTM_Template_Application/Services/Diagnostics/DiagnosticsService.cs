using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Orchestrate all diagnostic checks
/// </summary>
public class DiagnosticsService : IDiagnosticsService
{
    private readonly ILogger<DiagnosticsService> _logger;
    private readonly IEnumerable<IDiagnosticCheck> _diagnosticChecks;
    private readonly HardwareDetection _hardwareDetection;

    public DiagnosticsService(
        ILogger<DiagnosticsService> logger,
        IEnumerable<IDiagnosticCheck> diagnosticChecks,
        HardwareDetection hardwareDetection)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(diagnosticChecks);
        ArgumentNullException.ThrowIfNull(hardwareDetection);

        _logger = logger;
        _diagnosticChecks = diagnosticChecks;
        _hardwareDetection = hardwareDetection;

        _logger.LogInformation("DiagnosticsService initialized with {CheckCount} diagnostic checks",
            _diagnosticChecks.Count());
    }

    /// <summary>
    /// Run all diagnostic checks
    /// </summary>
    public async Task<List<DiagnosticResult>> RunAllChecksAsync(CancellationToken cancellationToken = default)
    {
        // Check cancellation before starting any work
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Running all diagnostic checks ({Count} checks)", _diagnosticChecks.Count());

        var results = new List<DiagnosticResult>();

        // Run all checks in parallel
        var checkTasks = _diagnosticChecks.Select(async check =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var checkName = check.GetType().Name;
            _logger.LogDebug("Starting diagnostic check: {CheckName}", checkName);

            try
            {
                var result = await check.RunAsync(cancellationToken);
                _logger.LogInformation("Check {CheckName} completed with status: {Status}",
                    checkName, result.Status);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check {CheckName} failed with exception", checkName);
                return new DiagnosticResult
                {
                    CheckName = check.GetType().Name,
                    Status = DiagnosticStatus.Failed,
                    Message = $"Check failed with exception: {ex.Message}",
                    Details = new Dictionary<string, object>
                    {
                        ["Exception"] = ex.ToString()
                    },
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = 0
                };
            }
        });

        var checkResults = await Task.WhenAll(checkTasks);
        results.AddRange(checkResults);

        var failedCount = results.Count(r => r.Status == DiagnosticStatus.Failed);
        _logger.LogInformation("All diagnostic checks completed. Total: {Total}, Failed: {Failed}",
            results.Count, failedCount);

        return results;
    }

    /// <summary>
    /// Run a specific diagnostic check
    /// </summary>
    public async Task<DiagnosticResult> RunCheckAsync(string checkName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(checkName);

        _logger.LogInformation("Running specific diagnostic check: {CheckName}", checkName);

        var check = _diagnosticChecks.FirstOrDefault(c =>
            c.GetType().Name.Equals(checkName, StringComparison.OrdinalIgnoreCase));

        if (check == null)
        {
            _logger.LogWarning("Diagnostic check not found: {CheckName}", checkName);
            return new DiagnosticResult
            {
                CheckName = checkName,
                Status = DiagnosticStatus.Failed,
                Message = $"Check '{checkName}' not found",
                Details = new Dictionary<string, object>(),
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = 0
            };
        }

        try
        {
            var result = await check.RunAsync();
            _logger.LogInformation("Check {CheckName} completed with status: {Status}",
                checkName, result.Status);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Check {CheckName} failed with exception", checkName);
            return new DiagnosticResult
            {
                CheckName = checkName,
                Status = DiagnosticStatus.Failed,
                Message = $"Check failed with exception: {ex.Message}",
                Details = new Dictionary<string, object>
                {
                    ["Exception"] = ex.ToString()
                },
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = 0
            };
        }
    }

    /// <summary>
    /// Get hardware capabilities
    /// </summary>
    public HardwareCapabilities GetHardwareCapabilities()
    {
        _logger.LogDebug("Detecting hardware capabilities");
        var capabilities = _hardwareDetection.DetectCapabilities();
        _logger.LogInformation("Hardware detected: Platform={Platform}, Cores={Cores}, Memory={MemoryMB}MB",
            capabilities.Platform, capabilities.ProcessorCount, capabilities.TotalMemoryMB);
        return capabilities;
    }
}
