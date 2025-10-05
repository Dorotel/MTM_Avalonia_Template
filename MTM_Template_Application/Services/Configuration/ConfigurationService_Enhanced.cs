using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;
using MySql.Data.MySqlClient;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Enhanced configuration service with database persistence and dynamic location detection
/// </summary>
public class ConfigurationServiceEnhanced : IConfigurationService
{
    private readonly ILogger<ConfigurationServiceEnhanced> _logger;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, ConfigurationSetting> _settings = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly HttpClient _httpClient;

    private string? _connectionString;
    private UserFoldersConfig? _userFoldersConfig;
    private DatabaseSchemaConfig? _databaseConfig;

    public event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;

    public ConfigurationServiceEnhanced(
        ILogger<ConfigurationServiceEnhanced> logger,
        IMemoryCache cache,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(httpClient);

        _logger = logger;
        _cache = cache;
        _httpClient = httpClient;

        // Initialize runtime environment
        InitializeRuntimeEnvironment();
    }

    /// <summary>
    /// Initialize runtime environment - create directories and config files if missing
    /// </summary>
    private void InitializeRuntimeEnvironment()
    {
        try
        {
            var repoRoot = GetRepositoryRoot();

            // Ensure config directory exists
            var configDir = Path.Combine(repoRoot, "config");
            EnsureDirectoryExists(configDir, "Repository config");

            // Ensure logs directory exists
            var logsDir = Path.Combine(repoRoot, "logs");
            EnsureDirectoryExists(logsDir, "Logs");

            // Ensure cache directory exists
            var cacheDir = Path.Combine(repoRoot, "cache");
            EnsureDirectoryExists(cacheDir, "Cache");

            // Ensure local user directories base path exists
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var mtmAppsBase = Path.Combine(myDocs, "MTM_Apps");
            EnsureDirectoryExists(mtmAppsBase, "MTM Apps base");

            var usersBase = Path.Combine(mtmAppsBase, "users");
            EnsureDirectoryExists(usersBase, "Users base");

            // Create config files with defaults if missing
            EnsureConfigFileExists(configDir, "user-folders.json", GetDefaultUserFoldersJson());
            EnsureConfigFileExists(configDir, "database-schema.json", GetDefaultDatabaseSchemaJson());

            _logger.LogInformation("Runtime environment initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fully initialize runtime environment - some features may not work");
        }
    }

    private void EnsureDirectoryExists(string path, string description)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogDebug("Created {Description} directory: {Path}", description, path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create {Description} directory: {Path}", description, path);
        }
    }

    private void EnsureConfigFileExists(string configDir, string fileName, string defaultContent)
    {
        var filePath = Path.Combine(configDir, fileName);
        try
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, defaultContent);
                _logger.LogInformation("Created default config file: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create config file: {FileName}", fileName);
        }
    }

    private static string GetRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Look for solution file or .git directory
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "*.sln").Any() ||
                Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }

            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName ?? string.Empty;
        }

        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }

    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        _lock.EnterReadLock();
        try
        {
            // Check environment variables first (highest precedence)
            var envKey = ConvertToEnvironmentVariableKey(key);
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogDebug("Configuration key {Key} resolved from environment variable {EnvKey}", key, envKey);
                return ConvertValue<T>(envValue);
            }

            // Check settings dictionary
            if (_settings.TryGetValue(key, out var setting))
            {
                _logger.LogDebug("Configuration key {Key} resolved from {Source}", key, setting.Source);
                return ConvertValue<T>(setting.Value);
            }

            _logger.LogDebug("Configuration key {Key} not found, using default", key);
            return defaultValue;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task SetValue(string key, object value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        _lock.EnterWriteLock();
        try
        {
            var oldValue = _settings.TryGetValue(key, out var existing) ? existing.Value : null;

            var setting = new ConfigurationSetting
            {
                Key = key,
                Value = value.ToString() ?? string.Empty,
                Source = "UserConfig",
                Precedence = 50,
                IsEncrypted = IsSensitiveKey(key)
            };

            _settings[key] = setting;

            var logValue = setting.IsEncrypted ? "[REDACTED]" : value.ToString();
            _logger.LogInformation("Configuration setting {Key} updated to {Value}", key, logValue);

            OnConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value
            });
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterWriteLock();
        try
        {
            _logger.LogInformation("Reloading configuration from all sources");

            // Clear caches
            _userFoldersConfig = null;
            _databaseConfig = null;
            _connectionString = null;

            _logger.LogInformation("Configuration reloaded successfully, {Count} settings active", _settings.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    public async Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive", nameof(userId));
        }

        try
        {
            var connectionString = await GetDatabaseConnectionStringAsync(cancellationToken);
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = new MySqlCommand(
                "SELECT ConfigKey, ConfigValue FROM UserPreferences WHERE UserId = @userId",
                connection
            );
            command.Parameters.AddWithValue("@userId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var loadedCount = 0;
            _lock.EnterWriteLock();
            try
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var keyOrdinal = reader.GetOrdinal("ConfigKey");
                    var valueOrdinal = reader.GetOrdinal("ConfigValue");

                    var key = reader.GetString(keyOrdinal);
                    var value = reader.IsDBNull(valueOrdinal)
                        ? string.Empty
                        : reader.GetString(valueOrdinal);

                    _settings[key] = new ConfigurationSetting
                    {
                        Key = key,
                        Value = value,
                        Source = "Database",
                        Precedence = 40,
                        IsEncrypted = IsSensitiveKey(key)
                    };
                    loadedCount++;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            _logger.LogInformation("Loaded {Count} user preferences for user {UserId}", loadedCount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user preferences for user {UserId}", userId);
            throw;
        }
    }

    public async Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive", nameof(userId));
        }
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var connectionString = await GetDatabaseConnectionStringAsync(cancellationToken);
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = new MySqlCommand(
                @"INSERT INTO UserPreferences (UserId, ConfigKey, ConfigValue, LastModified)
                  VALUES (@userId, @key, @value, @now)
                  ON DUPLICATE KEY UPDATE ConfigValue = @value, LastModified = @now",
                connection
            );
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", value.ToString());
            command.Parameters.AddWithValue("@now", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(cancellationToken);

            var logValue = IsSensitiveKey(key) ? "[REDACTED]" : value.ToString();
            _logger.LogInformation("Saved user preference {Key}={Value} for user {UserId}", key, logValue, userId);

            // Update in-memory cache
            await SetValue(key, value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user preference {Key} for user {UserId}", key, userId);
            throw;
        }
    }

    public async Task<string> GetUserFolderPathAsync(int userId, CancellationToken cancellationToken = default)
    {
        var locations = await GetUserFolderLocationsAsync(userId, cancellationToken);
        return locations.Primary;
    }

    public async Task<UserFolderLocations> GetUserFolderLocationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId <= 0)
        {
            throw new ArgumentException("User ID must be positive", nameof(userId));
        }

        // Check cache first
        var cacheKey = $"UserFolderLocations_{userId}";
        if (_cache.TryGetValue<UserFolderLocations>(cacheKey, out var cached))
        {
            _logger.LogDebug("Retrieved user folder locations from cache for user {UserId}", userId);
            return cached!;
        }

        try
        {
            var config = await GetUserFoldersConfigAsync(cancellationToken);
            var isHome = await IsHomeDevelopmentEnvironmentAsync(config, cancellationToken);

            string primary;
            string? backup = null;
            bool isNetworkAvailable = false;

            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var localPath = config.LocalFallbackPath.Replace("{MyDocuments}", myDocs);
            var userLocalPath = Path.Combine(localPath, userId.ToString());

            if (isHome)
            {
                // Home development: local only
                primary = userLocalPath;
                _logger.LogDebug("Home development environment detected, using local path only");
            }
            else
            {
                // On-premises: try network first
                var networkPath = Path.Combine(config.NetworkDrivePath, userId.ToString());
                isNetworkAvailable = await IsNetworkPathAccessibleAsync(
                    config.NetworkDrivePath,
                    config.NetworkAccessTimeoutSeconds,
                    cancellationToken
                );

                if (isNetworkAvailable)
                {
                    primary = networkPath;
                    backup = config.EnableDualWrite ? userLocalPath : null;
                    _logger.LogDebug("Network drive accessible, using network as primary");
                }
                else
                {
                    primary = userLocalPath;
                    _logger.LogDebug("Network drive not accessible, using local as primary");
                }
            }

            // Ensure primary directory exists
            EnsureDirectoryExists(primary, $"User {userId} primary folder");

            // Ensure backup directory exists if dual write enabled
            if (backup != null)
            {
                EnsureDirectoryExists(backup, $"User {userId} backup folder");
            }

            var result = new UserFolderLocations
            {
                Primary = primary,
                Backup = backup,
                IsNetworkAvailable = isNetworkAvailable
            };

            // Cache for configured duration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(config.LocationCacheDurationMinutes));
            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine user folder locations for user {UserId}", userId);

            // Fallback to local path
            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fallbackPath = Path.Combine(myDocs, "MTM_Apps", "users", userId.ToString());
            EnsureDirectoryExists(fallbackPath, $"User {userId} fallback folder");

            return new UserFolderLocations
            {
                Primary = fallbackPath,
                Backup = null,
                IsNetworkAvailable = false
            };
        }
    }

    public async Task<string> GetDatabaseConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Check cache
        if (_connectionString != null)
        {
            return _connectionString;
        }

        try
        {
            var config = await GetDatabaseSchemaConfigAsync(cancellationToken);
            var isHome = await IsHomeDevelopmentEnvironmentAsync(config.ConnectionSettings.HomeDevelopmentIPAddress, cancellationToken);

            var dbConfig = isHome
                ? config.ConnectionSettings.HomeDatabase
                : config.ConnectionSettings.ProductionDatabase;

            var builder = new MySqlConnectionStringBuilder
            {
                Server = dbConfig.Host,
                Port = (uint)dbConfig.Port,
                Database = dbConfig.Database,
                UserID = "root", // TODO: Get from secure storage
                Password = "root", // TODO: Get from secure storage
                ConnectionTimeout = (uint)config.ConnectionSettings.ConnectionTimeoutSeconds,
                Pooling = config.ConnectionSettings.EnableConnectionPooling,
                MaximumPoolSize = (uint)config.ConnectionSettings.MaxPoolSize,
                MinimumPoolSize = (uint)config.ConnectionSettings.MinPoolSize,
                CharacterSet = "utf8mb4"
            };

            _connectionString = builder.ConnectionString;

            _logger.LogInformation("Database connection string built for {Environment} environment: {Database}",
                isHome ? "Home" : "Production", dbConfig.Database);

            return _connectionString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build database connection string");

            // Fallback to default local
            var fallback = "Server=localhost;Port=3306;Database=mtm_template_dev;Uid=root;Pwd=root;Charset=utf8mb4;";
            _logger.LogWarning("Using fallback connection string");
            return fallback;
        }
    }

    private async Task<UserFoldersConfig> GetUserFoldersConfigAsync(CancellationToken cancellationToken)
    {
        if (_userFoldersConfig != null)
        {
            return _userFoldersConfig;
        }

        var repoRoot = GetRepositoryRoot();
        var configPath = Path.Combine(repoRoot, "config", "user-folders.json");

        var json = await File.ReadAllTextAsync(configPath, cancellationToken);
        _userFoldersConfig = JsonSerializer.Deserialize<UserFoldersConfig>(json)
            ?? throw new InvalidOperationException("Failed to parse user-folders.json");

        return _userFoldersConfig;
    }

    private async Task<DatabaseSchemaConfig> GetDatabaseSchemaConfigAsync(CancellationToken cancellationToken)
    {
        if (_databaseConfig != null)
        {
            return _databaseConfig;
        }

        var repoRoot = GetRepositoryRoot();
        var configPath = Path.Combine(repoRoot, "config", "database-schema.json");

        var json = await File.ReadAllTextAsync(configPath, cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _databaseConfig = JsonSerializer.Deserialize<DatabaseSchemaConfig>(json, options)
            ?? throw new InvalidOperationException("Failed to parse database-schema.json");

        return _databaseConfig;
    }

    private async Task<bool> IsHomeDevelopmentEnvironmentAsync(string homeDevelopmentIp, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://api.ipify.org", cancellationToken);
            var publicIp = response.Trim();

            var isHome = publicIp.Equals(homeDevelopmentIp, StringComparison.OrdinalIgnoreCase);
            _logger.LogDebug("Public IP: {PublicIp}, Home IP: {HomeIp}, IsHome: {IsHome}",
                publicIp, homeDevelopmentIp, isHome);

            return isHome;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect public IP, defaulting to on-premises mode");
            return false;
        }
    }

    private async Task<bool> IsHomeDevelopmentEnvironmentAsync(UserFoldersConfig config, CancellationToken cancellationToken)
    {
        return await IsHomeDevelopmentEnvironmentAsync(config.HomeDevelopmentIPAddress, cancellationToken);
    }

    private async Task<bool> IsNetworkPathAccessibleAsync(string networkPath, int timeoutSeconds, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            await Task.Run(() =>
            {
                return Directory.Exists(networkPath);
            }, cts.Token);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ConvertToEnvironmentVariableKey(string key)
    {
        // Convert "API:TimeoutSeconds" to "MTM_API_TIMEOUTSECONDS"
        return "MTM_" + key.Replace(":", "_").Replace(".", "_").ToUpperInvariant();
    }

    private static T? ConvertValue<T>(string value)
    {
        var targetType = typeof(T);
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

        return (T?)Convert.ChangeType(value, underlyingType);
    }

    private static bool IsSensitiveKey(string key)
    {
        var sensitiveKeywords = new[] { "password", "secret", "key", "token", "credential", "apikey" };
        return sensitiveKeywords.Any(keyword => key.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetDefaultUserFoldersJson()
    {
        return """
        {
          "HomeDevelopmentIPAddress": "73.94.78.172",
          "NetworkDrivePath": "\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users",
          "LocalFallbackPath": "{MyDocuments}\\MTM_Apps\\users",
          "NetworkAccessTimeoutSeconds": 2,
          "LocationCacheDurationMinutes": 5,
          "EnableDualWrite": true
        }
        """;
    }

    private static string GetDefaultDatabaseSchemaJson()
    {
        return """
        {
          "ConnectionSettings": {
            "HomeDevelopmentIPAddress": "73.94.78.172",
            "HomeDatabase": {
              "Host": "localhost",
              "Port": 3306,
              "Database": "mtm_template_dev",
              "Description": "MAMP MySQL 5.7 local development"
            },
            "ProductionDatabase": {
              "Host": "mtmanu-sql01",
              "Port": 3306,
              "Database": "mtm_template_prod",
              "Description": "Production MySQL server"
            },
            "ConnectionTimeoutSeconds": 5,
            "EnableConnectionPooling": true,
            "MaxPoolSize": 100,
            "MinPoolSize": 5
          }
        }
        """;
    }
}

// Configuration models
internal class UserFoldersConfig
{
    public string HomeDevelopmentIPAddress { get; set; } = string.Empty;
    public string NetworkDrivePath { get; set; } = string.Empty;
    public string LocalFallbackPath { get; set; } = string.Empty;
    public int NetworkAccessTimeoutSeconds { get; set; }
    public int LocationCacheDurationMinutes { get; set; }
    public bool EnableDualWrite { get; set; }
}

internal class DatabaseSchemaConfig
{
    public ConnectionSettingsConfig ConnectionSettings { get; set; } = new();
}

internal class ConnectionSettingsConfig
{
    public string HomeDevelopmentIPAddress { get; set; } = string.Empty;
    public DatabaseConfig HomeDatabase { get; set; } = new();
    public DatabaseConfig ProductionDatabase { get; set; } = new();
    public int ConnectionTimeoutSeconds { get; set; }
    public bool EnableConnectionPooling { get; set; }
    public int MaxPoolSize { get; set; }
    public int MinPoolSize { get; set; }
}

internal class DatabaseConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
