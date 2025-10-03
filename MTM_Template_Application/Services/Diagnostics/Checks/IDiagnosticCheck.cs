using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics.Checks;

/// <summary>
/// Interface for diagnostic health checks
/// </summary>
public interface IDiagnosticCheck
{
    /// <summary>
    /// Run the diagnostic check
    /// </summary>
    Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default);
}
