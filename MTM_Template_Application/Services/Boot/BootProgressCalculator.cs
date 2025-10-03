using System;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Calculates overall boot progress based on stage number and stage progress.
/// Each stage has a weighted contribution to the total progress:
/// - Stage 0: 0-10% (splash)
/// - Stage 1: 10-75% (services - largest weight due to service initialization)
/// - Stage 2: 75-100% (application ready)
/// </summary>
public class BootProgressCalculator
{
    private readonly ILogger<BootProgressCalculator> _logger;

    // Stage weights (must sum to 100)
    private const int Stage0Weight = 10;  // Splash: 10%
    private const int Stage1Weight = 65;  // Services: 65% (most time spent here)
    private const int Stage2Weight = 25;  // Application: 25%

    public BootProgressCalculator(ILogger<BootProgressCalculator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Calculate overall boot progress percentage (0-100).
    /// </summary>
    /// <param name="stageNumber">Current stage (0, 1, or 2)</param>
    /// <param name="stageProgress">Progress within the stage (0-100)</param>
    /// <returns>Overall boot progress (0-100)</returns>
    public int CalculateProgress(int stageNumber, int stageProgress)
    {
        if (stageNumber < 0 || stageNumber > 2)
        {
            _logger.LogWarning("Invalid stage number: {StageNumber}. Defaulting to 0", stageNumber);
            stageNumber = 0;
        }

        if (stageProgress < 0 || stageProgress > 100)
        {
            _logger.LogWarning("Invalid stage progress: {StageProgress}. Clamping to 0-100", stageProgress);
            stageProgress = Math.Clamp(stageProgress, 0, 100);
        }

        int overallProgress = stageNumber switch
        {
            0 => CalculateStage0Progress(stageProgress),
            1 => CalculateStage1Progress(stageProgress),
            2 => CalculateStage2Progress(stageProgress),
            _ => 0
        };

        return Math.Clamp(overallProgress, 0, 100);
    }

    /// <summary>
    /// Get the progress range for a specific stage.
    /// </summary>
    public (int Start, int End) GetStageRange(int stageNumber)
    {
        return stageNumber switch
        {
            0 => (0, Stage0Weight),
            1 => (Stage0Weight, Stage0Weight + Stage1Weight),
            2 => (Stage0Weight + Stage1Weight, 100),
            _ => (0, 0)
        };
    }

    private int CalculateStage0Progress(int stageProgress)
    {
        // Stage 0: 0-10%
        // Progress within stage maps to 0-10% of total
        return (int)(stageProgress * (Stage0Weight / 100.0));
    }

    private int CalculateStage1Progress(int stageProgress)
    {
        // Stage 1: 10-75%
        // Progress within stage maps to 10-75% of total
        var stageContribution = (int)(stageProgress * (Stage1Weight / 100.0));
        return Stage0Weight + stageContribution;
    }

    private int CalculateStage2Progress(int stageProgress)
    {
        // Stage 2: 75-100%
        // Progress within stage maps to 75-100% of total
        var stageContribution = (int)(stageProgress * (Stage2Weight / 100.0));
        return Stage0Weight + Stage1Weight + stageContribution;
    }

    /// <summary>
    /// Calculate estimated time remaining based on current progress and elapsed time.
    /// </summary>
    /// <param name="currentProgress">Current overall progress (0-100)</param>
    /// <param name="elapsedMs">Time elapsed so far in milliseconds</param>
    /// <returns>Estimated time remaining in milliseconds, or null if insufficient data</returns>
    public long? EstimateTimeRemaining(int currentProgress, long elapsedMs)
    {
        if (currentProgress <= 0 || elapsedMs <= 0)
        {
            return null; // Insufficient data
        }

        if (currentProgress >= 100)
        {
            return 0; // Complete
        }

        // Linear projection: if X% took Y ms, 100% will take (100/X) * Y ms
        var totalEstimatedMs = (long)((100.0 / currentProgress) * elapsedMs);
        var remainingMs = totalEstimatedMs - elapsedMs;

        return remainingMs > 0 ? remainingMs : 0;
    }

    /// <summary>
    /// Format time remaining as a human-readable string.
    /// </summary>
    public string FormatTimeRemaining(long? remainingMs)
    {
        if (!remainingMs.HasValue || remainingMs.Value <= 0)
        {
            return string.Empty;
        }

        var totalSeconds = remainingMs.Value / 1000;

        if (totalSeconds < 5)
        {
            return "A few seconds...";
        }
        else if (totalSeconds < 60)
        {
            return $"About {totalSeconds} seconds...";
        }
        else
        {
            var minutes = totalSeconds / 60;
            return $"About {minutes} minute{(minutes > 1 ? "s" : "")}...";
        }
    }
}
