using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// System diagnostics and health checks
/// </summary>
public interface IDiagnosticsService
{
    /// <summary>
    /// Run all diagnostic checks
    /// </summary>
    Task<List<DiagnosticResult>> RunAllChecksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Run a specific diagnostic check
    /// </summary>
    Task<DiagnosticResult> RunCheckAsync(string checkName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hardware capabilities
    /// </summary>
    HardwareCapabilities GetHardwareCapabilities();
}
