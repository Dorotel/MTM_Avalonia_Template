using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Interface for error notification service with severity-based routing
/// </summary>
public interface IErrorNotificationService
{
    /// <summary>
    /// Active configuration errors that have not been resolved
    /// </summary>
    ReadOnlyObservableCollection<ConfigurationError> ActiveErrors { get; }

    /// <summary>
    /// Event raised when a new error occurs
    /// </summary>
    event EventHandler<ConfigurationError>? OnErrorOccurred;

    /// <summary>
    /// Notify of a configuration error with automatic severity-based routing
    /// </summary>
    /// <param name="error">The configuration error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task NotifyAsync(ConfigurationError error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Show status bar warning for non-critical errors
    /// </summary>
    /// <param name="error">The configuration error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ShowStatusBarWarningAsync(ConfigurationError error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Show modal dialog for critical errors (blocks UI until dismissed)
    /// </summary>
    /// <param name="error">The configuration error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user took action, false if cancelled</returns>
    Task<bool> ShowModalDialogAsync(ConfigurationError error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark an error as resolved
    /// </summary>
    /// <param name="key">Configuration key of the error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ResolveErrorAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all active errors
    /// </summary>
    Task ClearAllErrorsAsync(CancellationToken cancellationToken = default);
}
