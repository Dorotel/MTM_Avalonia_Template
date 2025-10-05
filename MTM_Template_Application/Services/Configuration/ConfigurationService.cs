using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Configuration service with layered precedence: env vars > user config > app config > defaults
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly Dictionary<string, ConfigurationSetting> _settings = new();
    private readonly object _lock = new();

    public event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Get a configuration value following precedence order
    /// </summary>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_lock)
        {
            // Check environment variables first (highest precedence)
            var envValue = Environment.GetEnvironmentVariable(key.Replace(":", "_"));
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogDebug("Configuration key {Key} resolved from environment variable", key);
                return ConvertValue<T>(envValue);
            }

            // Check settings dictionary (loaded from user/app config)
            if (_settings.TryGetValue(key, out var setting))
            {
                _logger.LogDebug("Configuration key {Key} resolved from {Source}", key, setting.Source);
                return ConvertValue<T>(setting.Value);
            }

            _logger.LogDebug("Configuration key {Key} not found, using default", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Set a configuration value
    /// </summary>
    public Task SetValue(string key, object value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        lock (_lock)
        {
            var oldValue = _settings.TryGetValue(key, out var existing) ? existing.Value : null;

            var setting = new ConfigurationSetting
            {
                Key = key,
                Value = value.ToString() ?? string.Empty,
                Source = "UserConfig",
                Precedence = 50, // UserConfig precedence
                IsEncrypted = IsSensitiveKey(key)
            };

            _settings[key] = setting;

            _logger.LogInformation("Configuration setting {Key} updated", key);

            // Raise change event
            OnConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value
            });
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reload configuration from all sources
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            _logger.LogInformation("Reloading configuration from all sources");

            // In a full implementation, this would:
            // 1. Load from base config file
            // 2. Apply overlay config files
            // 3. Check environment variables
            // 4. Validate all settings

            // For now, this is a placeholder that preserves existing settings
            _logger.LogInformation("Configuration reloaded successfully, {Count} settings active", _settings.Count);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Load user preferences from database (not implemented in basic service)
    /// </summary>
    public Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ConfigurationServiceEnhanced for database persistence features");
    }

    /// <summary>
    /// Save user preference to database (not implemented in basic service)
    /// </summary>
    public Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ConfigurationServiceEnhanced for database persistence features");
    }

    /// <summary>
    /// Get user folder path (not implemented in basic service)
    /// </summary>
    public Task<string> GetUserFolderPathAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ConfigurationServiceEnhanced for dynamic location detection features");
    }

    /// <summary>
    /// Get user folder locations (not implemented in basic service)
    /// </summary>
    public Task<UserFolderLocations> GetUserFolderLocationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ConfigurationServiceEnhanced for dynamic location detection features");
    }

    /// <summary>
    /// Get database connection string (not implemented in basic service)
    /// </summary>
    public Task<string> GetDatabaseConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Use ConfigurationServiceEnhanced for database connection features");
    }

    private static T? ConvertValue<T>(string value)
    {
        var targetType = typeof(T);

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
        {
            return (T)(object)value;
        }

        if (underlyingType == typeof(int))
        {
            return (T)(object)int.Parse(value);
        }

        if (underlyingType == typeof(bool))
        {
            return (T)(object)bool.Parse(value);
        }

        if (underlyingType == typeof(double))
        {
            return (T)(object)double.Parse(value);
        }

        // Default: try to convert via Convert.ChangeType
        return (T?)Convert.ChangeType(value, underlyingType);
    }

    private static bool IsSensitiveKey(string key)
    {
        var sensitiveKeywords = new[] { "password", "secret", "key", "token", "credential" };
        return sensitiveKeywords.Any(keyword => key.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
