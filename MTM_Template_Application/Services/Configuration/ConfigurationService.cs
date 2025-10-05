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
using MySql.Data.MySqlClient;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Configuration service with layered precedence: env vars > user config > app config > defaults
/// Enhanced with MySQL persistence, dynamic location detection, and runtime initialization
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly Dictionary<string, ConfigurationSetting> _settings = new();
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly string _repositoryRoot;
    private readonly string _configDirectory;
    private readonly string _userFoldersConfigPath;
    private readonly string _databaseSchemaConfigPath;

    private UserFoldersConfig? _userFoldersConfig;
    private DatabaseSchemaConfig? _databaseSchemaConfig;
    private bool _isInitialized;

    public event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;

    public ConfigurationService(
        ILogger<ConfigurationService> logger,
        IMemoryCache? cache = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        // Determine repository root (go up from executing assembly location)
        var assemblyLocation = AppContext.BaseDirectory;
        _repositoryRoot = GetRepositoryRoot(assemblyLocation);
        _configDirectory = Path.Combine(_repositoryRoot, "config");
        _userFoldersConfigPath = Path.Combine(_configDirectory, "user-folders.json");
        _databaseSchemaConfigPath = Path.Combine(_configDirectory, "database-schema.json");

        // Initialize runtime environment (create directories and files)
        InitializeRuntimeEnvironmentAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initialize runtime environment: create directories and configuration files if missing
    /// </summary>
    private async Task InitializeRuntimeEnvironmentAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Initializing runtime environment...");

            // Create config directory
            EnsureDirectoryExists(_configDirectory, "Config");

            // Create logs directory
            var logsDirectory = Path.Combine(_repositoryRoot, "logs");
            EnsureDirectoryExists(logsDirectory, "Logs");

            // Create cache directory
            var cacheDirectory = Path.Combine(_repositoryRoot, "cache");
            EnsureDirectoryExists(cacheDirectory, "Cache");

            // Create local user base directories
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var mtmAppsBase = Path.Combine(myDocuments, "MTM_Apps");
            var mtmUsersBase = Path.Combine(mtmAppsBase, "users");
            EnsureDirectoryExists(mtmAppsBase, "MTM_Apps (MyDocuments)");
            EnsureDirectoryExists(mtmUsersBase, "MTM_Apps\\users (MyDocuments)");

            // Create/validate config files
            await EnsureConfigFileExistsAsync(_userFoldersConfigPath, GetDefaultUserFoldersConfig(), cancellationToken);
            await EnsureConfigFileExistsAsync(_databaseSchemaConfigPath, GetDefaultDatabaseSchemaConfig(), cancellationToken);

            // Load configuration files
            _userFoldersConfig = await LoadJsonConfigAsync<UserFoldersConfig>(_userFoldersConfigPath, cancellationToken);
            _databaseSchemaConfig = await LoadJsonConfigAsync<DatabaseSchemaConfig>(_databaseSchemaConfigPath, cancellationToken);

            _isInitialized = true;
            _logger.LogInformation("Runtime environment initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize runtime environment - continuing with degraded functionality");
        }
    }

    private void EnsureDirectoryExists(string path, string description)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created directory: {Description} at {Path}", description, path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create directory: {Description} at {Path}", description, path);
        }
    }

    private async Task EnsureConfigFileExistsAsync(string filePath, string defaultContent, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, defaultContent, cancellationToken);
                _logger.LogInformation("Created configuration file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create configuration file: {FilePath}", filePath);
        }
    }

    private async Task<T?> LoadJsonConfigAsync<T>(string filePath, CancellationToken cancellationToken) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Configuration file not found: {FilePath}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var config = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            _logger.LogDebug("Loaded configuration from {FilePath}", filePath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration from {FilePath}", filePath);
            return null;
        }
    }

    private static string GetRepositoryRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current != null)
        {
            // Look for .git directory or solution file as markers
            if (Directory.Exists(Path.Combine(current.FullName, ".git")) ||
                Directory.GetFiles(current.FullName, "*.sln").Any())
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }

    private static string GetDefaultUserFoldersConfig()
    {
        return JsonSerializer.Serialize(new
        {
            HomeDevelopmentIPAddress = "73.94.78.172",
            NetworkDrivePath = "\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users",
            LocalFallbackPath = "{MyDocuments}\\MTM_Apps\\users",
            NetworkAccessTimeoutSeconds = 2,
            LocationCacheDurationMinutes = 5,
            EnableDualWrite = true
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string GetDefaultDatabaseSchemaConfig()
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

    /// <summary>
    /// Get a configuration value following precedence order
    /// </summary>
    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        _lock.EnterReadLock();
        try
        {
            // Check environment variables first (highest precedence)
            var envKey = ConvertToEnvironmentVariableKey(key);
            if (IsValidEnvironmentVariableKey(envKey))
            {
                var envValue = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrEmpty(envValue))
                {
                    _logger.LogDebug("Configuration key {Key} resolved from environment variable {EnvKey}", key, envKey);
                    return ConvertValue<T>(envValue);
                }
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
        finally
        {
            _lock.ExitReadLock();
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

        _lock.EnterWriteLock();
        try
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

            // Raise change event (outside lock to avoid deadlock)
            Task.Run(() => OnConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                Key = key,
                OldValue = oldValue,
                NewValue = value
            }), cancellationToken);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reload configuration from all sources
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterWriteLock();
        try
        {
            _logger.LogInformation("Reloading configuration from all sources");

            // Clear cache
            _cache.Remove("public_ip");
            _cache.Remove($"user_folder_location_*");
            _cache.Remove("database_connection_string");

            // Reload config files
            _userFoldersConfig = LoadJsonConfigAsync<UserFoldersConfig>(_userFoldersConfigPath, cancellationToken).GetAwaiter().GetResult();
            _databaseSchemaConfig = LoadJsonConfigAsync<DatabaseSchemaConfig>(_databaseSchemaConfigPath, cancellationToken).GetAwaiter().GetResult();

            _logger.LogInformation("Configuration reloaded successfully, {Count} settings active", _settings.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Load user preferences from database
    /// </summary>
    public async Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero", nameof(userId));
        }

        try
        {
            var connectionString = await GetDatabaseConnectionStringAsync(cancellationToken);
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = "SELECT ConfigKey, ConfigValue FROM UserPreferences WHERE UserId = @UserId";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var loadedCount = 0;
            _lock.EnterWriteLock();
            try
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var key = reader.GetString(reader.GetOrdinal("ConfigKey"));
                    var value = reader.IsDBNull(reader.GetOrdinal("ConfigValue"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("ConfigValue"));

                    var setting = new ConfigurationSetting
                    {
                        Key = key,
                        Value = value,
                        Source = "Database",
                        Precedence = 40, // Database precedence (below user config, above defaults)
                        IsEncrypted = false
                    };

                    _settings[key] = setting;
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

    /// <summary>
    /// Save user preference to database
    /// </summary>
    public async Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var connectionString = await GetDatabaseConnectionStringAsync(cancellationToken);
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var query = @"
                INSERT INTO UserPreferences (UserId, ConfigKey, ConfigValue, LastModified)
                VALUES (@UserId, @Key, @Value, @LastModified)
                ON DUPLICATE KEY UPDATE ConfigValue = @Value, LastModified = @LastModified";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value.ToString());
            command.Parameters.AddWithValue("@LastModified", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Saved user preference {Key} for user {UserId}", key, userId);

            // Update in-memory cache
            await SetValue(key, value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user preference {Key} for user {UserId}", key, userId);
            throw;
        }
    }

    /// <summary>
    /// Get user folder path with dynamic location detection
    /// </summary>
    public async Task<string> GetUserFolderPathAsync(int userId, CancellationToken cancellationToken = default)
    {
        var locations = await GetUserFolderLocationsAsync(userId, cancellationToken);
        return locations.Primary;
    }

    /// <summary>
    /// Get user folder locations (primary, backup, network availability)
    /// </summary>
    public async Task<UserFolderLocations> GetUserFolderLocationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId must be greater than zero", nameof(userId));
        }

        var cacheKey = $"user_folder_location_{userId}";

        // Check cache first
        if (_cache.TryGetValue<UserFolderLocations>(cacheKey, out var cachedLocations) && cachedLocations != null)
        {
            _logger.LogDebug("User folder location for user {UserId} retrieved from cache", userId);
            return cachedLocations;
        }

        try
        {
            // Get public IP address
            var publicIp = await GetPublicIpAddressAsync(cancellationToken);
            var homeIp = _userFoldersConfig?.HomeDevelopmentIPAddress ?? "73.94.78.172";
            var isHomeEnvironment = publicIp == homeIp;

            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var localPath = _userFoldersConfig?.LocalFallbackPath?.Replace("{MyDocuments}", myDocuments)
                ?? Path.Combine(myDocuments, "MTM_Apps", "users");
            var localUserPath = Path.Combine(localPath, userId.ToString());

            string primaryPath;
            string? backupPath = null;
            var isNetworkAvailable = false;

            if (isHomeEnvironment)
            {
                // Home development: use local path only
                primaryPath = localUserPath;
                _logger.LogDebug("Home environment detected - using local path for user {UserId}", userId);
            }
            else
            {
                // On-premises: try network, fallback to local
                var networkPath = _userFoldersConfig?.NetworkDrivePath;
                if (!string.IsNullOrEmpty(networkPath))
                {
                    var networkUserPath = Path.Combine(networkPath, userId.ToString());
                    var timeout = TimeSpan.FromSeconds(_userFoldersConfig?.NetworkAccessTimeoutSeconds ?? 2);

                    isNetworkAvailable = await IsNetworkPathAccessibleAsync(networkPath, timeout, cancellationToken);

                    if (isNetworkAvailable)
                    {
                        primaryPath = networkUserPath;
                        backupPath = _userFoldersConfig?.EnableDualWrite == true ? localUserPath : null;
                        _logger.LogDebug("Network path accessible - using network for user {UserId}", userId);
                    }
                    else
                    {
                        primaryPath = localUserPath;
                        _logger.LogWarning("Network path not accessible - using local fallback for user {UserId}", userId);
                    }
                }
                else
                {
                    primaryPath = localUserPath;
                    _logger.LogWarning("Network path not configured - using local fallback for user {UserId}", userId);
                }
            }

            // Ensure directories exist
            EnsureDirectoryExists(primaryPath, $"Primary user folder for user {userId}");
            if (!string.IsNullOrEmpty(backupPath))
            {
                EnsureDirectoryExists(backupPath, $"Backup user folder for user {userId}");
            }

            var result = new UserFolderLocations
            {
                Primary = primaryPath,
                Backup = backupPath,
                IsNetworkAvailable = isNetworkAvailable
            };

            // Cache the result
            var cacheDuration = TimeSpan.FromMinutes(_userFoldersConfig?.LocationCacheDurationMinutes ?? 5);
            _cache.Set(cacheKey, result, cacheDuration);

            _logger.LogInformation("User folder location determined for user {UserId}: Primary={Primary}, Backup={Backup}, NetworkAvailable={NetworkAvailable}",
                userId, result.Primary, result.Backup ?? "None", result.IsNetworkAvailable);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine user folder location for user {UserId} - using local fallback", userId);

            // Fallback to local path
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fallbackPath = Path.Combine(myDocuments, "MTM_Apps", "users", userId.ToString());
            EnsureDirectoryExists(fallbackPath, $"Fallback user folder for user {userId}");

            return new UserFolderLocations
            {
                Primary = fallbackPath,
                Backup = null,
                IsNetworkAvailable = false
            };
        }
    }

    /// <summary>
    /// Get database connection string with dynamic selection
    /// </summary>
    public async Task<string> GetDatabaseConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "database_connection_string";

        // Check cache first
        if (_cache.TryGetValue<string>(cacheKey, out var cachedConnectionString) && !string.IsNullOrEmpty(cachedConnectionString))
        {
            _logger.LogDebug("Database connection string retrieved from cache");
            return cachedConnectionString;
        }

        try
        {
            // Get public IP address
            var publicIp = await GetPublicIpAddressAsync(cancellationToken);
            var homeIp = _databaseSchemaConfig?.ConnectionSettings?.HomeDevelopmentIPAddress ?? "73.94.78.172";
            var isHomeEnvironment = publicIp == homeIp;

            var dbConfig = isHomeEnvironment
                ? _databaseSchemaConfig?.ConnectionSettings?.HomeDatabase
                : _databaseSchemaConfig?.ConnectionSettings?.ProductionDatabase;

            if (dbConfig == null)
            {
                throw new InvalidOperationException("Database configuration not found");
            }

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = dbConfig.Host ?? "localhost",
                Port = (uint)(dbConfig.Port),
                Database = dbConfig.Database ?? "mtm_template_dev",
                ConnectionTimeout = (uint)(_databaseSchemaConfig?.ConnectionSettings?.ConnectionTimeoutSeconds ?? 5),
                Pooling = _databaseSchemaConfig?.ConnectionSettings?.EnableConnectionPooling ?? true,
                MaximumPoolSize = (uint)(_databaseSchemaConfig?.ConnectionSettings?.MaxPoolSize ?? 100),
                MinimumPoolSize = (uint)(_databaseSchemaConfig?.ConnectionSettings?.MinPoolSize ?? 5),
                AllowUserVariables = true,
                UseCompression = true
            };

            // Credentials should be retrieved from SecretsService (not implemented here - would require ISecretsService injection)
            // For now, using default development credentials (should be replaced with proper secret management)
            if (isHomeEnvironment)
            {
                connectionStringBuilder.UserID = "root";
                connectionStringBuilder.Password = "root"; // MAMP default
            }

            var connectionString = connectionStringBuilder.ConnectionString;

            // Cache the result
            _cache.Set(cacheKey, connectionString, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Database connection string determined: Environment={Environment}, Host={Host}, Database={Database}",
                isHomeEnvironment ? "Home" : "Production", dbConfig.Host, dbConfig.Database);

            return connectionString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine database connection string - using localhost fallback");

            // Fallback to localhost
            return "Server=localhost;Port=3306;Database=mtm_template_dev;User=root;Password=root;ConnectionTimeout=5;Pooling=true;";
        }
    }

    private async Task<string> GetPublicIpAddressAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "public_ip";

        // Check cache first (5 minute TTL)
        if (_cache.TryGetValue<string>(cacheKey, out var cachedIp) && !string.IsNullOrEmpty(cachedIp))
        {
            return cachedIp;
        }

        try
        {
            var response = await _httpClient.GetStringAsync("https://api.ipify.org", cancellationToken);
            var ip = response.Trim();

            _cache.Set(cacheKey, ip, TimeSpan.FromMinutes(5));
            _logger.LogDebug("Public IP address detected: {IpAddress}", ip);

            return ip;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect public IP address - assuming production environment");
            return "0.0.0.0"; // Fallback: assume production
        }
    }

    private async Task<bool> IsNetworkPathAccessibleAsync(string networkPath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            return await Task.Run(() => Directory.Exists(networkPath), cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Network path {NetworkPath} not accessible", networkPath);
            return false;
        }
    }

    private static string ConvertToEnvironmentVariableKey(string configKey)
    {
        // Convert colon-separated config key to underscore-separated environment variable key
        // Example: "API:TimeoutSeconds" -> "MTM_API_TIMEOUT_SECONDS" or "API_TIMEOUT_SECONDS"
        return configKey.Replace(":", "_").ToUpperInvariant();
    }

    private static bool IsValidEnvironmentVariableKey(string envKey)
    {
        // Valid patterns: MTM_*, DOTNET_*, ASPNETCORE_*
        if (string.IsNullOrWhiteSpace(envKey))
        {
            return false;
        }

        var validPrefixes = new[] { "MTM_", "DOTNET_", "ASPNETCORE_" };
        if (!validPrefixes.Any(prefix => envKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Must contain only alphanumeric and underscores
        return envKey.All(c => char.IsLetterOrDigit(c) || c == '_');
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

/// <summary>
/// User folders configuration model (loaded from config/user-folders.json)
/// </summary>
internal class UserFoldersConfig
{
    public string? HomeDevelopmentIPAddress { get; set; }
    public string? NetworkDrivePath { get; set; }
    public string? LocalFallbackPath { get; set; }
    public int NetworkAccessTimeoutSeconds { get; set; } = 2;
    public int LocationCacheDurationMinutes { get; set; } = 5;
    public bool EnableDualWrite { get; set; } = true;
}

/// <summary>
/// Database schema configuration model (loaded from config/database-schema.json)
/// </summary>
internal class DatabaseSchemaConfig
{
    public ConnectionSettings? ConnectionSettings { get; set; }
}

internal class ConnectionSettings
{
    public string? HomeDevelopmentIPAddress { get; set; }
    public DatabaseConfig? HomeDatabase { get; set; }
    public DatabaseConfig? ProductionDatabase { get; set; }
    public int ConnectionTimeoutSeconds { get; set; } = 5;
    public bool EnableConnectionPooling { get; set; } = true;
    public int MaxPoolSize { get; set; } = 100;
    public int MinPoolSize { get; set; } = 5;
}

internal class DatabaseConfig
{
    public string? Host { get; set; }
    public int Port { get; set; } = 3306;
    public string? Database { get; set; }
    public string? Description { get; set; }
}
