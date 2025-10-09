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
    /// Initialize the configuration service asynchronously
    /// Must be called after DI construction before using other methods
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

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
    /// Load user preferences from database
    /// </summary>
    Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save a user preference to database
    /// </summary>
    Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user folder path with dynamic location detection
    /// </summary>
    Task<string> GetUserFolderPathAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user folder locations (primary, backup, network availability)
    /// </summary>
    Task<UserFolderLocations> GetUserFolderLocationsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get database connection string with dynamic selection
    /// </summary>
    Task<string> GetDatabaseConnectionStringAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when configuration changes
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;
}

/// <summary>
/// User folder locations result
/// </summary>
public record UserFolderLocations
{
    public string Primary { get; init; } = string.Empty;
    public string? Backup { get; init; }
    public bool IsNetworkAvailable { get; init; }
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
