using System;
using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Manages application configuration with layered precedence
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get a configuration value
    /// </summary>
    T? GetValue<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Set a configuration value
    /// </summary>
    Task SetValue(string key, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reload configuration from all sources
    /// </summary>
    Task ReloadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when configuration changes
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;
}

/// <summary>
/// Configuration changed event arguments
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
