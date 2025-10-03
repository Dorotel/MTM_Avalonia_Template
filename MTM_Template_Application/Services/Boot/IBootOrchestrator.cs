using System;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Boot;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Orchestrates the three-stage boot sequence
/// </summary>
public interface IBootOrchestrator
{
    /// <summary>
    /// Execute the complete boot sequence (all stages)
    /// </summary>
    Task<BootMetrics> ExecuteBootSequenceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Stage 0: Splash screen and minimal initialization
    /// </summary>
    Task ExecuteStage0Async(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Stage 1: Core services initialization
    /// </summary>
    Task ExecuteStage1Async(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Stage 2: Application ready
    /// </summary>
    Task ExecuteStage2Async(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get boot metrics for the current or last session
    /// </summary>
    BootMetrics GetBootMetrics();

    /// <summary>
    /// Event raised when boot progress changes
    /// </summary>
    event EventHandler<BootProgressEventArgs>? OnProgressChanged;
}

/// <summary>
/// Boot progress event arguments
/// </summary>
public class BootProgressEventArgs : EventArgs
{
    public int StageNumber { get; init; }
    public string StageName { get; init; } = string.Empty;
    public int ProgressPercentage { get; init; }
    public string StatusMessage { get; init; } = string.Empty;
}
