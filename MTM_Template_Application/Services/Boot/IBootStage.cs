using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Represents a single boot stage
/// </summary>
public interface IBootStage
{
    /// <summary>
    /// Stage number (0, 1, or 2)
    /// </summary>
    int StageNumber { get; }

    /// <summary>
    /// Human-readable stage name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Execute the stage
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate preconditions before executing the stage
    /// </summary>
    void ValidatePreconditions();
}
