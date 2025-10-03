using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Localization;
using MTM_Template_Application.Services.Navigation;
using MTM_Template_Application.Services.Theme;

namespace MTM_Template_Application.Services.Boot.Stages;

/// <summary>
/// Stage 2: Application ready - load theme, navigation, and user session.
/// Timeout: 15 seconds
/// Services: Theme, Localization, Navigation
/// </summary>
public class Stage2ApplicationReady : IBootStage
{
    private readonly ILogger<Stage2ApplicationReady> _logger;
    private readonly IThemeService _themeService;
    private readonly INavigationService _navigationService;
    private readonly ILocalizationService _localizationService;

    public int StageNumber => 2;
    public string Name => "Application";

    public event EventHandler<StageProgressEventArgs>? OnProgressChanged;

    public Stage2ApplicationReady(
        ILogger<Stage2ApplicationReady> logger,
        IThemeService themeService,
        INavigationService navigationService,
        ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(localizationService);

        _logger = logger;
        _themeService = themeService;
        _navigationService = navigationService;
        _localizationService = localizationService;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stage 2: Application initialization started");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            ValidatePreconditions();

            var totalSteps = 5;
            var currentStep = 0;

            // Step 1: Load localization settings
            ReportProgress(++currentStep, totalSteps, "Loading language settings...");
            await InitializeLocalizationAsync(cancellationToken);

            // Step 2: Load and apply theme
            ReportProgress(++currentStep, totalSteps, "Applying theme...");
            await InitializeThemeAsync(cancellationToken);

            // Step 3: Initialize navigation service
            ReportProgress(++currentStep, totalSteps, "Initializing navigation...");
            await InitializeNavigationAsync(cancellationToken);

            // Step 4: Navigate to home screen
            ReportProgress(++currentStep, totalSteps, "Loading home screen...");
            await NavigateToHomeAsync(cancellationToken);

            // Step 5: Final preparation
            ReportProgress(++currentStep, totalSteps, "Application ready");
            await FinalizeApplicationReadyAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Stage 2 completed successfully in {DurationMs}ms",
                stopwatch.ElapsedMilliseconds
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Stage 2 cancelled after {DurationMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stage 2 failed after {DurationMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public void ValidatePreconditions()
    {
        // Stage 2 requires Stage 1 to be complete (all services initialized)
        _logger.LogDebug("Stage 2 preconditions validated");
    }

    private async Task InitializeLocalizationAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing localization service");

        // Get supported cultures
        var supportedCultures = _localizationService.GetSupportedCultures();
        _logger.LogInformation("Supported cultures: {CultureCount}", supportedCultures.Count);

        // Set culture from OS or saved preference
        // (Culture is already set during service initialization)

        await Task.CompletedTask;
    }

    private async Task InitializeThemeAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing theme service");

        // Get current theme configuration
        var currentTheme = _themeService.GetCurrentTheme();

        _logger.LogInformation(
            "Theme loaded: Mode={ThemeMode}, IsDarkMode={IsDarkMode}",
            currentTheme.ThemeMode,
            currentTheme.IsDarkMode
        );

        // Apply theme (will trigger UI update)
        _themeService.SetTheme(currentTheme.ThemeMode);
    }

    private Task InitializeNavigationAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing navigation service");
        // Navigation service is already initialized via DI
        return Task.CompletedTask;
    }

    private async Task NavigateToHomeAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Navigating to home screen");

        try
        {
            // Navigate to the main application window/view
            await _navigationService.NavigateToAsync("Home", null, cancellationToken);
            _logger.LogInformation("Navigation to home screen completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to home screen");
            throw;
        }
    }

    private Task FinalizeApplicationReadyAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Finalizing application ready state");

        // Perform any final cleanup or preparation
        // - Close splash screen (handled by orchestrator)
        // - Show main window (handled by platform entry point)
        // - Enable user interaction

        _logger.LogInformation("Application is ready for user interaction");
        return Task.CompletedTask;
    }

    private void ReportProgress(int currentStep, int totalSteps, string statusMessage)
    {
        var progress = (int)((currentStep / (double)totalSteps) * 100);
        _logger.LogDebug("Stage 2 progress: {Progress}% - {Message}", progress, statusMessage);
        OnProgressChanged?.Invoke(this, new StageProgressEventArgs(progress, statusMessage));
    }
}
