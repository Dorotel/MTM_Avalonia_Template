using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Orchestrate all diagnostic checks
/// </summary>
public class DiagnosticsService : IDiagnosticsService
{
    private readonly IEnumerable<IDiagnosticCheck> _diagnosticChecks;
    private readonly HardwareDetection _hardwareDetection;

    public DiagnosticsService(
        IEnumerable<IDiagnosticCheck> diagnosticChecks,
        HardwareDetection hardwareDetection)
    {
        ArgumentNullException.ThrowIfNull(diagnosticChecks);
        ArgumentNullException.ThrowIfNull(hardwareDetection);

        _diagnosticChecks = diagnosticChecks;
        _hardwareDetection = hardwareDetection;
    }

    /// <summary>
    /// Run all diagnostic checks
    /// </summary>
    public async Task<List<DiagnosticResult>> RunAllChecksAsync()
    {
        var results = new List<DiagnosticResult>();

        // Run all checks in parallel
        var checkTasks = _diagnosticChecks.Select(async check =>
        {
            try
            {
                return await check.RunAsync();
            }
            catch (Exception ex)
            {
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

        return results;
    }

    /// <summary>
    /// Run a specific diagnostic check
    /// </summary>
    public async Task<DiagnosticResult> RunCheckAsync(string checkName)
    {
        ArgumentNullException.ThrowIfNull(checkName);

        var check = _diagnosticChecks.FirstOrDefault(c => 
            c.GetType().Name.Equals(checkName, StringComparison.OrdinalIgnoreCase));

        if (check == null)
        {
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
            return await check.RunAsync();
        }
        catch (Exception ex)
        {
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
        return _hardwareDetection.DetectCapabilities();
    }
}

/// <summary>
/// Base interface for diagnostic checks
/// </summary>
public interface IDiagnosticCheck
{
    /// <summary>
    /// Run the diagnostic check
    /// </summary>
    Task<DiagnosticResult> RunAsync();
}
