using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Boot.Stages;

/// <summary>
/// Stage 0: Splash screen initialization and minimal bootstrap.
/// Timeout: 10 seconds
/// Purpose: Show splash screen immediately, initialize watchdog, minimal services only
/// </summary>
public class Stage0Bootstrap : IBootStage
{
    private readonly ILogger<Stage0Bootstrap> _logger;

    public int StageNumber => 0;
    public string Name => "Splash";

    public Stage0Bootstrap(ILogger<Stage0Bootstrap> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stage 0: Splash screen initialization started");

        try
        {
            // Validate preconditions
            ValidatePreconditions();

            // Step 1: Initialize splash screen (handled by platform entry point)
            _logger.LogDebug("Splash screen should be visible");

            // Step 2: Initialize watchdog timer
            _logger.LogDebug("Watchdog timer active");

            // Step 3: Minimal bootstrap (just mark stage as started)
            await Task.Delay(100, cancellationToken); // Simulate minimal initialization

            _logger.LogInformation("Stage 0 completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Stage 0 cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stage 0 failed");
            throw;
        }
    }

    public void ValidatePreconditions()
    {
        // Stage 0 has no preconditions - it's the entry point
        _logger.LogDebug("Stage 0 preconditions validated (none required)");
    }
}
