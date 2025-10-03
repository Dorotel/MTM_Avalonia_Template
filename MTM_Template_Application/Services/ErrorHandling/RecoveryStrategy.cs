using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.ErrorHandling;

/// <summary>
/// Determines and executes recovery strategies based on error category
/// </summary>
public class RecoveryStrategy
{
    private readonly ILogger<RecoveryStrategy> _logger;
    private readonly ErrorCategorizer _errorCategorizer;

    public RecoveryStrategy(
        ILogger<RecoveryStrategy> logger,
        ErrorCategorizer errorCategorizer)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(errorCategorizer);

        _logger = logger;
        _errorCategorizer = errorCategorizer;
    }

    /// <summary>
    /// Determine the appropriate recovery action for an exception
    /// </summary>
    public RecoveryAction DetermineRecoveryAction(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var (category, severity) = _errorCategorizer.Categorize(exception);

        _logger.LogInformation(
            "Determining recovery action for error: Category={Category}, Severity={Severity}",
            category,
            severity
        );

        return category switch
        {
            "Transient" => RecoveryAction.Retry,
            "Configuration" => RecoveryAction.UseDefaults,
            "Permission" => RecoveryAction.PromptUser,
            "Storage" => RecoveryAction.CheckStorage,
            "Network" => RecoveryAction.Retry,
            "Resource" => RecoveryAction.RestartApplication,
            "Programming" => RecoveryAction.ReportAndContinue,
            "Critical" => RecoveryAction.RestartApplication,
            _ => RecoveryAction.ReportAndContinue
        };
    }

    /// <summary>
    /// Execute the recovery action
    /// </summary>
    public async Task<bool> ExecuteRecoveryAsync(Exception exception, RecoveryAction action)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogInformation(
            "Executing recovery action: {Action} for exception: {ExceptionType}",
            action,
            exception.GetType().Name
        );

        switch (action)
        {
            case RecoveryAction.Retry:
                _logger.LogInformation("Recovery: Will retry operation with exponential backoff");
                return true;

            case RecoveryAction.UseDefaults:
                _logger.LogInformation("Recovery: Falling back to default configuration");
                return true;

            case RecoveryAction.PromptUser:
                _logger.LogInformation("Recovery: User intervention required");
                // Note: Actual UI prompting would be done by the caller
                return false;

            case RecoveryAction.CheckStorage:
                _logger.LogInformation("Recovery: Checking storage availability");
                // Note: Actual storage check would be done by DiagnosticsService
                return true;

            case RecoveryAction.RestartApplication:
                _logger.LogCritical("Recovery: Application restart required");
                return false;

            case RecoveryAction.ReportAndContinue:
                _logger.LogWarning("Recovery: Reporting error and continuing");
                return true;

            default:
                _logger.LogWarning("Recovery: Unknown action, defaulting to report and continue");
                return true;
        }
    }
}

/// <summary>
/// Available recovery actions
/// </summary>
public enum RecoveryAction
{
    /// <summary>
    /// Retry the operation with exponential backoff
    /// </summary>
    Retry,

    /// <summary>
    /// Use default configuration values
    /// </summary>
    UseDefaults,

    /// <summary>
    /// Prompt the user for intervention
    /// </summary>
    PromptUser,

    /// <summary>
    /// Check storage availability and free space
    /// </summary>
    CheckStorage,

    /// <summary>
    /// Restart the application
    /// </summary>
    RestartApplication,

    /// <summary>
    /// Report the error and continue execution
    /// </summary>
    ReportAndContinue
}
