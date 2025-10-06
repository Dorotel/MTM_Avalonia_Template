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

        // Load environment variables at startup (clarification 2025-10-05: read once at startup)
        LoadEnvironmentVariables();
        _userFoldersConfigPath = Path.Combine(_configDirectory, "user-folders.json");
        _databaseSchemaConfigPath = Path.Combine(_configDirectory, "database-schema.json");

        // NOTE: Do NOT call async methods from constructor (causes deadlock)
        // Initialize runtime environment synchronously (directories only - no I/O blocking)
        InitializeRuntimeEnvironmentSync();
    }

    /// <summary>
    /// Initialize runtime environment synchronously: create directories only (no I/O blocking)
    /// Config files will be created lazily on first access
    /// </summary>
    private void InitializeRuntimeEnvironmentSync()
    {
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

            _logger.LogInformation("Runtime environment directories created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize runtime environment directories");
            throw; // Fail fast - can't proceed without basic directories
        }
    }

    /// <summary>
    /// Initialize runtime environment asynchronously: load/create configuration files
    /// Call this explicitly after DI construction to complete initialization
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Loading configuration files...");

            // Create/validate config files
            await EnsureConfigFileExistsAsync(_userFoldersConfigPath, GetDefaultUserFoldersConfig(), cancellationToken);
            await EnsureConfigFileExistsAsync(_databaseSchemaConfigPath, GetDefaultDatabaseSchemaConfig(), cancellationToken);

            // Load configuration files
            _userFoldersConfig = await LoadJsonConfigAsync<UserFoldersConfig>(_userFoldersConfigPath, cancellationToken);
            _databaseSchemaConfig = await LoadJsonConfigAsync<DatabaseSchemaConfig>(_databaseSchemaConfigPath, cancellationToken);

            _isInitialized = true;
            _logger.LogInformation("Configuration service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize configuration service - continuing with degraded functionality");
            // Don't throw - graceful degradation
        }
    }

    /// <summary>
    /// Initialize runtime environment: create directories and configuration files if missing
    /// DEPRECATED: Use InitializeAsync() instead - this is now just a stub that does nothing
    /// </summary>
    [Obsolete("Use InitializeAsync() instead - this method is deprecated and does nothing")]
    public void InitializeRuntimeEnvironment()
    {
        // This method is deprecated and does nothing
        // Use InitializeAsync() instead for proper async initialization
        _logger.LogWarning("InitializeRuntimeEnvironment() called - this method is deprecated. Use InitializeAsync() instead.");
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

    /// <summary>
    /// Load environment variables at startup (clarification 2025-10-05: read once)
    /// Precedence: MTM_ENVIRONMENT > ASPNETCORE_ENVIRONMENT > DOTNET_ENVIRONMENT > build config
    /// </summary>
    private void LoadEnvironmentVariables()
    {
        // Check environment variables in precedence order
        var mtmEnv = Environment.GetEnvironmentVariable("MTM_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        // Determine effective environment (highest precedence wins)
        string? effectiveEnvironment = null;
        string source = "Default";

        if (!string.IsNullOrWhiteSpace(mtmEnv))
        {
            effectiveEnvironment = mtmEnv;
            source = "MTM_ENVIRONMENT";
        }
        else if (!string.IsNullOrWhiteSpace(aspnetEnv))
        {
            effectiveEnvironment = aspnetEnv;
            source = "ASPNETCORE_ENVIRONMENT";
        }
        else if (!string.IsNullOrWhiteSpace(dotnetEnv))
        {
            effectiveEnvironment = dotnetEnv;
            source = "DOTNET_ENVIRONMENT";
        }

        if (effectiveEnvironment != null)
        {
            _lock.EnterWriteLock();
            try
            {
                _settings["Environment"] = new ConfigurationSetting
                {
                    Key = "Environment",
                    Value = effectiveEnvironment,
                    Source = source,
                    Precedence = 10, // Highest precedence (environment variables)
                    IsEncrypted = false
                };

                _logger.LogInformation("Environment set to {Environment} from {Source}", effectiveEnvironment, source);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        else
        {
            _logger.LogDebug("No environment variables found (MTM_ENVIRONMENT, ASPNETCORE_ENVIRONMENT, DOTNET_ENVIRONMENT), using build configuration");
        }
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

        // FR-009: Validate environment variable key format
        // Environment variables (MTM_*, DOTNET_*, ASPNETCORE_*) must use underscores only
        // Hierarchical config keys (Display.Theme, API:Timeout) can use dots/colons
        if (key.Contains(' '))
        {
            throw new ArgumentException(
                $"Invalid configuration key format: '{key}'. Keys cannot contain spaces.",
                nameof(key));
        }

        // Check if this looks like an environment variable key (all uppercase with underscores or dots)
        var isEnvVarStyle = key.All(c => char.IsUpper(c) || c == '_' || c == '.');
        if (isEnvVarStyle && (key.Contains('.') || key.Contains('-')))
        {
            throw new ArgumentException(
                $"Invalid configuration key format: '{key}'. Environment variable keys must use underscores only (e.g., MTM_ENVIRONMENT, not MTM.ENVIRONMENT or MTM-ENVIRONMENT).",
                nameof(key));
        }

        // Reject hyphens in any key
        if (key.Contains('-'))
        {
            throw new ArgumentException(
                $"Invalid configuration key format: '{key}'. Keys cannot contain hyphens. Use underscores for environment variables (MTM_ENVIRONMENT) or dots/colons for hierarchical keys (Display.Theme, API:Timeout).",
                nameof(key));
        }

        _lock.EnterReadLock();
        try
        {
            // Special handling for ENVIRONMENT key with precedence chain
            if (key.Equals("ENVIRONMENT", StringComparison.OrdinalIgnoreCase))
            {
                var mtmEnv = Environment.GetEnvironmentVariable("MTM_ENVIRONMENT");
                if (!string.IsNullOrWhiteSpace(mtmEnv))
                {
                    _logger.LogDebug("Configuration key {Key} resolved from MTM_ENVIRONMENT", key);
                    return ConvertValue<T>(mtmEnv);
                }

                var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (!string.IsNullOrWhiteSpace(aspnetEnv))
                {
                    _logger.LogDebug("Configuration key {Key} resolved from ASPNETCORE_ENVIRONMENT", key);
                    return ConvertValue<T>(aspnetEnv);
                }

                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                if (!string.IsNullOrWhiteSpace(dotnetEnv))
                {
                    _logger.LogDebug("Configuration key {Key} resolved from DOTNET_ENVIRONMENT", key);
                    return ConvertValue<T>(dotnetEnv);
                }
            }

            // Check environment variables first (highest precedence)
            // Try with MTM_ prefix
            var mtmKey = $"MTM_{ConvertToEnvironmentVariableKey(key)}";
            var envValue = Environment.GetEnvironmentVariable(mtmKey);
            if (!string.IsNullOrEmpty(envValue))
            {
                _logger.LogDebug("Configuration key {Key} resolved from environment variable {EnvKey}", key, mtmKey);
                return ConvertValue<T>(envValue);
            }

            // Try exact key if it's already a valid env var key
            if (IsValidEnvironmentVariableKey(key))
            {
                envValue = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrEmpty(envValue))
                {
                    _logger.LogDebug("Configuration key {Key} resolved from environment variable {EnvKey}", key, key);
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
            var newValueString = value.ToString() ?? string.Empty;

            var setting = new ConfigurationSetting
            {
                Key = key,
                Value = newValueString,
                Source = "UserConfig",
                Precedence = 50, // UserConfig precedence
                IsEncrypted = IsSensitiveKey(key)
            };

            _settings[key] = setting;

            _logger.LogInformation("Configuration setting {Key} updated", key);

            // Only fire event if value actually changed (clarification 2025-10-05)
            if (oldValue?.ToString() != newValueString)
            {
                var eventArgs = new ConfigurationChangedEventArgs
                {
                    Key = key,
                    OldValue = oldValue,
                    NewValue = value
                };

                // Redact sensitive values in logs
                var oldValueForLog = IsSensitiveKey(key) ? "[REDACTED]" : oldValue?.ToString() ?? "null";
                var newValueForLog = IsSensitiveKey(key) ? "[REDACTED]" : newValueString;

                _logger.LogDebug("Configuration change event fired for {Key} (old: {OldValue}, new: {NewValue})", key, oldValueForLog, newValueForLog);

                // Raise event immediately after releasing lock
                OnConfigurationChanged?.Invoke(this, eventArgs);
            }
            else
            {
                _logger.LogDebug("Configuration value unchanged for {Key}, event not fired", key);
            }
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
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
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

            // Reload config files asynchronously
            _userFoldersConfig = await LoadJsonConfigAsync<UserFoldersConfig>(_userFoldersConfigPath, cancellationToken);
            _databaseSchemaConfig = await LoadJsonConfigAsync<DatabaseSchemaConfig>(_databaseSchemaConfigPath, cancellationToken);

            _logger.LogInformation("Configuration reloaded successfully, {Count} settings active", _settings.Count);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
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

            var query = "SELECT PreferenceKey, PreferenceValue FROM UserPreferences WHERE UserId = @UserId";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var loadedCount = 0;
            _lock.EnterWriteLock();
            try
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var key = reader.GetString(reader.GetOrdinal("PreferenceKey"));
                    var value = reader.IsDBNull(reader.GetOrdinal("PreferenceValue"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("PreferenceValue"));

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
                INSERT INTO UserPreferences (UserId, PreferenceKey, PreferenceValue, LastUpdated)
                VALUES (@UserId, @Key, @Value, @LastUpdated)
                ON DUPLICATE KEY UPDATE PreferenceValue = @Value, LastUpdated = @LastUpdated";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value.ToString());
            command.Parameters.AddWithValue("@LastUpdated", DateTime.UtcNow);

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
                UseCompression = false // Disabled for MAMP MySQL 5.7 compatibility (clarification 2025-10-05)
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
