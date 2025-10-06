using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Service for notifying users of configuration errors with severity-based routing
/// </summary>
public class ErrorNotificationService : IErrorNotificationService
{
    private readonly ILogger<ErrorNotificationService> _logger;
    private readonly ObservableCollection<ConfigurationError> _activeErrors;
    private readonly ReadOnlyObservableCollection<ConfigurationError> _readOnlyActiveErrors;

    /// <summary>
    /// Event raised when a new error occurs
    /// </summary>
    public event EventHandler<ConfigurationError>? OnErrorOccurred;

    /// <summary>
    /// Active configuration errors that have not been resolved
    /// </summary>
    public ReadOnlyObservableCollection<ConfigurationError> ActiveErrors => _readOnlyActiveErrors;

    public ErrorNotificationService(ILogger<ErrorNotificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _activeErrors = new ObservableCollection<ConfigurationError>();
        _readOnlyActiveErrors = new ReadOnlyObservableCollection<ConfigurationError>(_activeErrors);
    }

    /// <summary>
    /// Notify of a configuration error with automatic severity-based routing
    /// </summary>
    public async Task NotifyAsync(ConfigurationError error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);

        // Log the error with structured logging (no sensitive data)
        _logger.LogError(
            "Configuration error occurred: Key={Key}, Severity={Severity}, Message={Message}",
            error.Key,
            error.Severity,
            error.Message
        );

        // Add to active errors collection
        _activeErrors.Add(error);

        // Raise event
        OnErrorOccurred?.Invoke(this, error);

        // Route based on severity
        switch (error.Severity)
        {
            case ErrorSeverity.Info:
            case ErrorSeverity.Warning:
                await ShowStatusBarWarningAsync(error, cancellationToken);
                break;

            case ErrorSeverity.Critical:
                await ShowModalDialogAsync(error, cancellationToken);
                break;

            default:
                _logger.LogWarning("Unknown error severity {Severity} for key {Key}", error.Severity, error.Key);
                break;
        }
    }

    /// <summary>
    /// Show status bar warning for non-critical errors
    /// </summary>
    public Task ShowStatusBarWarningAsync(ConfigurationError error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);

        _logger.LogInformation(
            "Showing status bar warning for {Severity} error: {Message}",
            error.Severity,
            error.Message
        );

        // NOTE: Actual UI integration will be implemented in Phase 4 (T020-T021)
        // This method will integrate with MainWindow.axaml status bar
        // For now, just log the notification

        return Task.CompletedTask;
    }

    /// <summary>
    /// Show modal dialog for critical errors (blocks UI until dismissed)
    /// </summary>
    public Task<bool> ShowModalDialogAsync(ConfigurationError error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);

        _logger.LogInformation(
            "Showing modal dialog for critical error: {Message}",
            error.Message
        );

        // NOTE: Actual dialog implementation will be created in Phase 4 (T021)
        // This method will show ConfigurationErrorDialog.axaml
        // For now, just log and return true (user acknowledged)

        return Task.FromResult(true);
    }

    /// <summary>
    /// Mark an error as resolved
    /// </summary>
    public Task ResolveErrorAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var error = _activeErrors.FirstOrDefault(e => e.Key == key);
        if (error != null)
        {
            error.IsResolved = true;
            _activeErrors.Remove(error);

            _logger.LogInformation("Configuration error resolved: Key={Key}", key);
        }
        else
        {
            _logger.LogWarning("Attempted to resolve non-existent error: Key={Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clear all active errors
    /// </summary>
    public Task ClearAllErrorsAsync(CancellationToken cancellationToken = default)
    {
        _activeErrors.Clear();
        _logger.LogInformation("All configuration errors cleared");

        return Task.CompletedTask;
    }
}
