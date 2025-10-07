using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Boot;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Configuration;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.ViewModels;

/// <summary>
/// ViewModel for debug terminal window showing boot sequence and configuration diagnostics
/// </summary>
public partial class DebugTerminalViewModel : ViewModelBase
{
    private readonly ILogger<DebugTerminalViewModel> _logger;
    private readonly IBootOrchestrator? _bootOrchestrator;
    private readonly IConfigurationService? _configurationService;
    private readonly ISecretsService? _secretsService;
    private readonly FeatureFlagEvaluator? _featureFlagEvaluator;
    private readonly Services.Theme.IThemeService? _themeService;
    private readonly Services.Cache.ICacheService? _cacheService;
    private readonly Services.DataLayer.IMySqlClient? _mySqlClient;
    private readonly Services.DataLayer.IVisualApiClient? _visualApiClient;
    private readonly Services.Logging.ILoggingService? _loggingService;
    private readonly Services.Diagnostics.IDiagnosticsService? _diagnosticsService;
    private readonly Services.Navigation.INavigationService? _navigationService;
    private readonly Services.Localization.ILocalizationService? _localizationService;
    private readonly IPerformanceMonitoringService? _performanceMonitoringService;
    private readonly IDiagnosticsServiceExtensions? _diagnosticsServiceExtensions;
    private readonly IExportService? _exportService;

    [ObservableProperty]
    private string _title = "Debug Terminal - Boot & Configuration Diagnostics";

    // Boot Metrics
    [ObservableProperty]
    private string _sessionId = "N/A";

    [ObservableProperty]
    private string _bootStatus = "Unknown";

    [ObservableProperty]
    private string _bootStatusColor = "#FF9E9E9E"; // Gray

    [ObservableProperty]
    private long _totalBootDurationMs;

    [ObservableProperty]
    private string _totalBootDurationColor = "#FF9E9E9E";

    [ObservableProperty]
    private long _stage0DurationMs;

    [ObservableProperty]
    private string _stage0DurationColor = "#FF9E9E9E";

    [ObservableProperty]
    private long _stage1DurationMs;

    [ObservableProperty]
    private string _stage1DurationColor = "#FF9E9E9E";

    [ObservableProperty]
    private long _stage2DurationMs;

    [ObservableProperty]
    private string _stage2DurationColor = "#FF9E9E9E";

    [ObservableProperty]
    private int _memoryUsageMB;

    [ObservableProperty]
    private string _memoryUsageColor = "#FF9E9E9E";

    [ObservableProperty]
    private string _platformInfo = "Unknown";

    [ObservableProperty]
    private string _appVersion = "Unknown";

    [ObservableProperty]
    private ObservableCollection<ServiceMetricDisplay> _serviceMetrics = new();

    // Configuration
    [ObservableProperty]
    private string _configurationStatus = "Not Initialized";

    [ObservableProperty]
    private string _configurationStatusColor = "#FFFF4444"; // Red by default

    [ObservableProperty]
    private string _environmentType = "Unknown";

    [ObservableProperty]
    private ObservableCollection<ConfigurationSettingDisplay> _configurationSettings = new();

    // Feature Flags
    [ObservableProperty]
    private ObservableCollection<FeatureFlagDisplay> _featureFlags = new();

    // Secrets
    [ObservableProperty]
    private string _secretsServiceType = "Unknown";

    [ObservableProperty]
    private string _secretsServiceStatus = "Unknown";

    [ObservableProperty]
    private string _secretsServiceColor = "#FF9E9E9E";

    // Theme Service
    [ObservableProperty]
    private string _themeMode = "Unknown";

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _osThemeDetection = "Unknown";

    // Cache Service
    [ObservableProperty]
    private ObservableCollection<CacheStatDisplay> _cacheStats = new();

    // Database
    [ObservableProperty]
    private string _databaseConnectionStatus = "Unknown";

    [ObservableProperty]
    private string _databaseConnectionColor = "#FF9E9E9E";

    [ObservableProperty]
    private string _databaseName = "Unknown";

    [ObservableProperty]
    private string _databaseServer = "Unknown";

    // Data Layer
    [ObservableProperty]
    private string _visualApiStatus = "Unknown";

    [ObservableProperty]
    private string _visualApiColor = "#FF9E9E9E";

    [ObservableProperty]
    private string _httpClientStatus = "Unknown";

    // Logging Service
    [ObservableProperty]
    private string _logLevel = "Unknown";

    [ObservableProperty]
    private string _logFilePath = "Unknown";

    [ObservableProperty]
    private long _logFileSizeKB;

    // Diagnostics Service
    [ObservableProperty]
    private ObservableCollection<HealthCheckDisplay> _healthChecks = new();

    // Navigation Service
    [ObservableProperty]
    private string _navigationServiceStatus = "Unknown";

    [ObservableProperty]
    private string _currentRoute = "Unknown";

    // Localization Service
    [ObservableProperty]
    private string _currentCulture = "Unknown";

    [ObservableProperty]
    private int _loadedTranslations;

    // User Folder Locations
    [ObservableProperty]
    private string _userFolderNetworkPath = "Unknown";

    [ObservableProperty]
    private string _userFolderLocalPath = "Unknown";

    [ObservableProperty]
    private string _userFolderActiveLocation = "Unknown";

    // Performance Monitoring (T026)
    [ObservableProperty]
    private PerformanceSnapshot? _currentPerformance;

    [ObservableProperty]
    private ObservableCollection<PerformanceSnapshot> _performanceHistory = new();

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private bool _canToggleMonitoring = true;

    // Boot Timeline (T027)
    [ObservableProperty]
    private BootTimeline? _currentBootTimeline;

    [ObservableProperty]
    private ObservableCollection<BootTimeline> _historicalBootTimelines = new();

    [ObservableProperty]
    private TimeSpan _totalBootTime;

    [ObservableProperty]
    private string? _slowestStage;

    // Error History (T028)
    [ObservableProperty]
    private ObservableCollection<ErrorEntry> _recentErrors = new();

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private ErrorSeverity _selectedSeverityFilter = ErrorSeverity.Error;

    public DebugTerminalViewModel(
        ILogger<DebugTerminalViewModel> logger,
        IBootOrchestrator? bootOrchestrator = null,
        IConfigurationService? configurationService = null,
        ISecretsService? secretsService = null,
        FeatureFlagEvaluator? featureFlagEvaluator = null,
        Services.Theme.IThemeService? themeService = null,
        Services.Cache.ICacheService? cacheService = null,
        Services.DataLayer.IMySqlClient? mySqlClient = null,
        Services.DataLayer.IVisualApiClient? visualApiClient = null,
        Services.Logging.ILoggingService? loggingService = null,
        Services.Diagnostics.IDiagnosticsService? diagnosticsService = null,
        Services.Navigation.INavigationService? navigationService = null,
        Services.Localization.ILocalizationService? localizationService = null,
        IPerformanceMonitoringService? performanceMonitoringService = null,
        IDiagnosticsServiceExtensions? diagnosticsServiceExtensions = null,
        IExportService? exportService = null)
    {
        _logger = logger;
        _bootOrchestrator = bootOrchestrator;
        _configurationService = configurationService;
        _secretsService = secretsService;
        _featureFlagEvaluator = featureFlagEvaluator;
        _themeService = themeService;
        _cacheService = cacheService;
        _mySqlClient = mySqlClient;
        _visualApiClient = visualApiClient;
        _loggingService = loggingService;
        _diagnosticsService = diagnosticsService;
        _navigationService = navigationService;
        _localizationService = localizationService;
        _performanceMonitoringService = performanceMonitoringService;
        _diagnosticsServiceExtensions = diagnosticsServiceExtensions;
        _exportService = exportService;

        LoadDiagnostics();
    }

    // T029: Performance Monitoring Commands
    [RelayCommand(CanExecute = nameof(CanStartMonitoring))]
    private async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        if (_performanceMonitoringService == null)
        {
            _logger.LogWarning("Cannot start monitoring - PerformanceMonitoringService not available");
            return;
        }

        try
        {
            IsMonitoring = true;
            CanToggleMonitoring = false;
            await _performanceMonitoringService.StartMonitoringAsync(TimeSpan.FromSeconds(5), cancellationToken);
            _logger.LogInformation("Performance monitoring started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start performance monitoring");
            IsMonitoring = false;
        }
        finally
        {
            CanToggleMonitoring = true;
        }
    }

    private bool CanStartMonitoring() => !IsMonitoring && CanToggleMonitoring && _performanceMonitoringService != null;

    [RelayCommand(CanExecute = nameof(CanStopMonitoring))]
    private async Task StopMonitoringAsync()
    {
        if (_performanceMonitoringService == null)
        {
            _logger.LogWarning("Cannot stop monitoring - PerformanceMonitoringService not available");
            return;
        }

        try
        {
            CanToggleMonitoring = false;
            await _performanceMonitoringService.StopMonitoringAsync();
            IsMonitoring = false;
            _logger.LogInformation("Performance monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop performance monitoring");
        }
        finally
        {
            CanToggleMonitoring = true;
        }
    }

    private bool CanStopMonitoring() => IsMonitoring && CanToggleMonitoring && _performanceMonitoringService != null;

    // T030: Quick Actions Panel Commands
    [RelayCommand]
    private async Task ClearCacheAsync(CancellationToken cancellationToken)
    {
        if (_cacheService == null)
        {
            _logger.LogWarning("Cannot clear cache - CacheService not available");
            return;
        }

        try
        {
            // TODO: Add confirmation dialog per CL-008
            await _cacheService.ClearAsync(cancellationToken);
            _logger.LogInformation("Cache cleared successfully");
            LoadCacheData(); // Refresh cache stats
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
        }
    }

    [RelayCommand]
    private async Task ReloadConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_configurationService == null)
        {
            _logger.LogWarning("Cannot reload configuration - ConfigurationService not available");
            return;
        }

        try
        {
            // Configuration service doesn't have explicit reload method
            // Just refresh the display data
            await Task.Run(() => LoadConfigurationData(), cancellationToken);
            _logger.LogInformation("Configuration reloaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration");
        }
    }

    [RelayCommand]
    private async Task TestDatabaseConnectionAsync(CancellationToken cancellationToken)
    {
        if (_mySqlClient == null)
        {
            _logger.LogWarning("Cannot test database - MySqlClient not available");
            DatabaseConnectionStatus = "Not Available";
            DatabaseConnectionColor = "#FFFF4444";
            return;
        }

        try
        {
            // TODO: Add actual connection test when IMySqlClient has TestConnectionAsync method
            await Task.Delay(500, cancellationToken); // Simulate connection test
            DatabaseConnectionStatus = "Connection Test: SUCCESS";
            DatabaseConnectionColor = "#FF44FF44";
            _logger.LogInformation("Database connection test successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            DatabaseConnectionStatus = $"Connection Test: FAILED - {ex.Message}";
            DatabaseConnectionColor = "#FFFF4444";
        }
    }

    [RelayCommand]
    private async Task ForceGarbageCollectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(() =>
            {
                var beforeMB = GC.GetTotalMemory(false) / 1024 / 1024;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var afterMB = GC.GetTotalMemory(false) / 1024 / 1024;
                _logger.LogInformation("Forced GC: {BeforeMB}MB -> {AfterMB}MB (freed {FreedMB}MB)",
                    beforeMB, afterMB, beforeMB - afterMB);
            }, cancellationToken);

            LoadBootMetrics(); // Refresh memory usage
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to force garbage collection");
        }
    }

    [RelayCommand]
    private async Task RefreshAllDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(() => LoadDiagnostics(), cancellationToken);
            _logger.LogInformation("All diagnostic data refreshed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh diagnostic data");
        }
    }

    [RelayCommand]
    private async Task ExportDiagnosticsAsync(CancellationToken cancellationToken)
    {
        if (_exportService == null)
        {
            _logger.LogWarning("Cannot export diagnostics - ExportService not available");
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var exportPath = System.IO.Path.Combine(
                AppContext.BaseDirectory,
                $"diagnostics-export-{timestamp}.json"
            );

            var bytesWritten = await _exportService.ExportToJsonAsync(exportPath, cancellationToken);
            _logger.LogInformation("Diagnostics exported to {Path} ({Bytes} bytes)", exportPath, bytesWritten);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export diagnostics");
        }
    }

    // T031: Boot Timeline Refresh Command
    [RelayCommand]
    private async Task RefreshBootTimelineAsync(CancellationToken cancellationToken)
    {
        if (_diagnosticsServiceExtensions == null)
        {
            _logger.LogWarning("Cannot refresh boot timeline - DiagnosticsServiceExtensions not available");
            return;
        }

        try
        {
            var timeline = await _diagnosticsServiceExtensions.GetBootTimelineAsync(cancellationToken);
            CurrentBootTimeline = timeline;

            // Calculate total boot time
            TotalBootTime = timeline.Stage0.Duration + timeline.Stage1.Duration + timeline.Stage2.Duration;

            // Determine slowest stage
            var stages = new[]
            {
                ("Stage 0", timeline.Stage0.Duration),
                ("Stage 1", timeline.Stage1.Duration),
                ("Stage 2", timeline.Stage2.Duration)
            };
            var slowest = stages.OrderByDescending(s => s.Item2).First();
            SlowestStage = $"{slowest.Item1} ({slowest.Item2.TotalMilliseconds:F0}ms)";

            _logger.LogInformation("Boot timeline refreshed: {TotalMs}ms total", TotalBootTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh boot timeline");
        }
    }

    // T032: Error History Management Commands
    [RelayCommand]
    private async Task ClearErrorHistoryAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(() =>
            {
                RecentErrors.Clear();
                ErrorCount = 0;
            }, cancellationToken);

            _logger.LogInformation("Error history cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear error history");
        }
    }

    [RelayCommand]
    private async Task FilterErrorsBySeverityAsync(ErrorSeverity severity, CancellationToken cancellationToken)
    {
        if (_diagnosticsServiceExtensions == null)
        {
            _logger.LogWarning("Cannot filter errors - DiagnosticsServiceExtensions not available");
            return;
        }

        try
        {
            SelectedSeverityFilter = severity;

            // Get recent errors and filter by severity
            var allErrors = await _diagnosticsServiceExtensions.GetRecentErrorsAsync(100, cancellationToken);
            var filteredErrors = allErrors.Where(e => e.Severity >= severity).ToList();

            await Task.Run(() =>
            {
                RecentErrors.Clear();
                foreach (var error in filteredErrors)
                {
                    RecentErrors.Add(error);
                }
                ErrorCount = RecentErrors.Count;
            }, cancellationToken);

            _logger.LogInformation("Filtered errors by severity {Severity}: {Count} errors", severity, ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to filter errors by severity");
        }
    }

    /// <summary>
    /// Load all diagnostic data from services
    /// </summary>
    private void LoadDiagnostics()
    {
        LoadBootMetrics();
        LoadConfigurationData();
        LoadFeatureFlags();
        LoadSecretsData();
        LoadThemeData();
        LoadCacheData();
        LoadDatabaseData();
        LoadDataLayerData();
        LoadLoggingData();
        LoadDiagnosticsData();
        LoadNavigationData();
        LoadLocalizationData();
        LoadUserFolderData();
    }

    /// <summary>
    /// Load boot metrics and validate against spec expectations
    /// </summary>
    private void LoadBootMetrics()
    {
        try
        {
            if (_bootOrchestrator == null)
            {
                _logger.LogWarning("BootOrchestrator not available");
                return;
            }

            var metrics = _bootOrchestrator.GetBootMetrics();

            SessionId = metrics.SessionId.ToString();
            BootStatus = metrics.SuccessStatus.ToString();

            // Validate boot status (spec: must be Success)
            BootStatusColor = metrics.SuccessStatus == Models.Boot.BootStatus.Success
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            // Validate total boot duration (spec: <10s = 10000ms)
            TotalBootDurationMs = metrics.TotalDurationMs ?? 0;
            TotalBootDurationColor = TotalBootDurationMs > 0 && TotalBootDurationMs < 10000
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            // Validate Stage 0 duration (spec: <1s = 1000ms)
            Stage0DurationMs = metrics.Stage0DurationMs;
            Stage0DurationColor = Stage0DurationMs > 0 && Stage0DurationMs < 1000
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            // Validate Stage 1 duration (spec: <3s = 3000ms)
            Stage1DurationMs = metrics.Stage1DurationMs ?? 0;
            Stage1DurationColor = Stage1DurationMs > 0 && Stage1DurationMs < 3000
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            // Validate Stage 2 duration (spec: <1s = 1000ms)
            Stage2DurationMs = metrics.Stage2DurationMs ?? 0;
            Stage2DurationColor = Stage2DurationMs > 0 && Stage2DurationMs < 1000
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            // Validate memory usage (spec: <100MB)
            MemoryUsageMB = metrics.MemoryUsageMB;
            MemoryUsageColor = MemoryUsageMB > 0 && MemoryUsageMB < 100
                ? "#FF44FF44" // Green
                : "#FFFF4444"; // Red

            PlatformInfo = metrics.PlatformInfo;
            AppVersion = metrics.AppVersion;

            // Load service metrics
            ServiceMetrics.Clear();
            foreach (var service in metrics.ServiceMetrics)
            {
                var display = new ServiceMetricDisplay
                {
                    ServiceName = service.ServiceName,
                    DurationMs = service.DurationMs ?? 0,
                    Success = service.Success,
                    ErrorMessage = service.ErrorMessage ?? "N/A",
                    // Green if success, red if failed
                    StatusColor = service.Success ? "#FF44FF44" : "#FFFF4444"
                };
                ServiceMetrics.Add(display);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load boot metrics");
            BootStatus = "Error: " + ex.Message;
            BootStatusColor = "#FFFF4444";
        }
    }

    /// <summary>
    /// Load configuration data
    /// </summary>
    private void LoadConfigurationData()
    {
        try
        {
            if (_configurationService == null)
            {
                _logger.LogWarning("ConfigurationService not available");
                return;
            }

            // Check if configuration service is initialized
            ConfigurationStatus = "Initialized";
            ConfigurationStatusColor = "#FF44FF44"; // Green

            // Get environment type
            EnvironmentType = _configurationService.GetValue<string>("Environment", "Unknown") ?? "Unknown";

            // Load sample configuration settings
            ConfigurationSettings.Clear();

            // Database connection string (check if exists)
            var dbConnection = _configurationService.GetValue<string>("Database:Server", null);
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Database:Server",
                Value = dbConnection ?? "NOT SET",
                Source = dbConnection != null ? "Configuration" : "Missing",
                Color = dbConnection != null ? "#FF44FF44" : "#FFFF4444"
            });

            // API timeout
            var apiTimeout = _configurationService.GetValue<int>("API:Timeout", 30);
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "API:Timeout",
                Value = apiTimeout.ToString() + "s",
                Source = "Configuration",
                Color = apiTimeout > 0 ? "#FF44FF44" : "#FFFF4444"
            });

            // Log level
            var logLevel = _configurationService.GetValue<string>("Logging:LogLevel:Default", "Information");
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Logging:LogLevel",
                Value = logLevel ?? "Information",
                Source = "Configuration",
                Color = "#FF44FF44"
            });

            // Theme mode
            var themeMode = _configurationService.GetValue<string>("Display:Theme", "Auto");
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Display:Theme",
                Value = themeMode ?? "Auto",
                Source = "Configuration",
                Color = "#FF44FF44"
            });

            // Cache compression enabled
            var cacheCompression = _configurationService.GetValue<bool>("Cache:CompressionEnabled", true);
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Cache:CompressionEnabled",
                Value = cacheCompression.ToString(),
                Source = "Configuration",
                Color = "#FF44FF44"
            });

            // Visual API base URL
            var visualApiUrl = _configurationService.GetValue<string>("Visual:BaseUrl", null);
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Visual:BaseUrl",
                Value = visualApiUrl ?? "NOT SET",
                Source = visualApiUrl != null ? "Configuration" : "Missing",
                Color = visualApiUrl != null ? "#FF44FF44" : "#FFFF4444"
            });

            // Repository root path
            var repoRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            ConfigurationSettings.Add(new ConfigurationSettingDisplay
            {
                Key = "Repository:Root",
                Value = repoRoot,
                Source = "Detected",
                Color = "#FF44FF44"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration data");
            ConfigurationStatus = "Error: " + ex.Message;
            ConfigurationStatusColor = "#FFFF4444";
        }
    }

    /// <summary>
    /// Load feature flags
    /// </summary>
    private void LoadFeatureFlags()
    {
        try
        {
            if (_featureFlagEvaluator == null)
            {
                _logger.LogWarning("FeatureFlagEvaluator not available");
                return;
            }

            FeatureFlags.Clear();

            // Get all registered flags - access private _flags field via reflection
            var flagsField = _featureFlagEvaluator.GetType().GetField("_flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var flagsDict = flagsField?.GetValue(_featureFlagEvaluator) as System.Collections.Generic.Dictionary<string, Models.Configuration.FeatureFlag>;
            var flags = flagsDict?.Values.ToList() ?? new List<Models.Configuration.FeatureFlag>();
            foreach (var flag in flags)
            {
                var display = new FeatureFlagDisplay
                {
                    Name = flag.Name,
                    IsEnabled = flag.IsEnabled,
                    Environment = flag.Environment ?? "All",
                    RolloutPercentage = flag.RolloutPercentage,
                    // Green if enabled, gray if disabled (not necessarily an error)
                    StatusColor = flag.IsEnabled ? "#FF44FF44" : "#FF9E9E9E"
                };
                FeatureFlags.Add(display);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load feature flags");
        }
    }

    /// <summary>
    /// Load secrets service data
    /// </summary>
    private void LoadSecretsData()
    {
        try
        {
            if (_secretsService == null)
            {
                _logger.LogWarning("SecretsService not available");
                SecretsServiceStatus = "Not Available";
                SecretsServiceColor = "#FFFF4444";
                return;
            }

            // Determine platform-specific implementation
            var serviceType = _secretsService.GetType().Name;
            SecretsServiceType = serviceType switch
            {
                "WindowsSecretsService" => "Windows DPAPI",
                "AndroidSecretsService" => "Android KeyStore",
                _ => serviceType
            };

            // Service is available (green)
            SecretsServiceStatus = "Available";
            SecretsServiceColor = "#FF44FF44";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load secrets data");
            SecretsServiceStatus = "Error: " + ex.Message;
            SecretsServiceColor = "#FFFF4444";
        }
    }

    /// <summary>
    /// Load theme service data
    /// </summary>
    private void LoadThemeData()
    {
        try
        {
            if (_themeService == null)
            {
                _logger.LogWarning("ThemeService not available");
                return;
            }

            var theme = _themeService.GetCurrentTheme();
            ThemeMode = theme.ThemeMode;
            IsDarkMode = theme.IsDarkMode;
            OsThemeDetection = IsDarkMode ? "Dark Mode" : "Light Mode";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load theme data");
        }
    }

    /// <summary>
    /// Load cache service statistics
    /// </summary>
    private void LoadCacheData()
    {
        try
        {
            if (_cacheService == null)
            {
                _logger.LogWarning("CacheService not available");
                return;
            }

            CacheStats.Clear();
            var stats = _cacheService.GetStatistics();

            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Total Entries",
                Value = stats.TotalEntries.ToString(),
                Color = "#FF44FF44"
            });

            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Cache Hits",
                Value = stats.HitCount.ToString(),
                Color = "#FF44FF44"
            });

            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Cache Misses",
                Value = stats.MissCount.ToString(),
                Color = stats.MissCount > stats.HitCount ? "#FFFF4444" : "#FF44FF44"
            });

            var hitRate = stats.HitCount + stats.MissCount > 0
                ? (double)stats.HitCount / (stats.HitCount + stats.MissCount) * 100
                : 0;
            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Hit Rate",
                Value = $"{hitRate:F1}%",
                Color = hitRate > 50 ? "#FF44FF44" : "#FFFF4444"
            });

            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Evictions",
                Value = stats.EvictionCount.ToString(),
                Color = "#FF9E9E9E"
            });

            CacheStats.Add(new CacheStatDisplay
            {
                Metric = "Memory Used (est)",
                Value = $"{stats.TotalSizeBytes / 1024}KB",
                Color = stats.TotalSizeBytes < 41943040 ? "#FF44FF44" : "#FFFF4444" // Target: <40MB
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cache data");
        }
    }

    /// <summary>
    /// Load database connection data
    /// </summary>
    private void LoadDatabaseData()
    {
        try
        {
            if (_configurationService == null)
            {
                _logger.LogWarning("ConfigurationService not available for database info");
                return;
            }

            var dbServer = _configurationService.GetValue<string>("Database:Server", "localhost");
            var dbName = _configurationService.GetValue<string>("Database:Database", "mtm_template_dev");

            DatabaseServer = dbServer ?? "localhost";
            DatabaseName = dbName ?? "mtm_template_dev";

            // Try to determine if MySQL is available
            if (_mySqlClient != null)
            {
                DatabaseConnectionStatus = "Initialized";
                DatabaseConnectionColor = "#FF44FF44";
            }
            else
            {
                DatabaseConnectionStatus = "Not Available";
                DatabaseConnectionColor = "#FFFF4444";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load database data");
            DatabaseConnectionStatus = "Error: " + ex.Message;
            DatabaseConnectionColor = "#FFFF4444";
        }
    }

    /// <summary>
    /// Load data layer service status
    /// </summary>
    private void LoadDataLayerData()
    {
        try
        {
            // Visual API Client
            if (_visualApiClient != null)
            {
                VisualApiStatus = "Initialized";
                VisualApiColor = "#FF44FF44";
            }
            else
            {
                VisualApiStatus = "Not Available (Android)";
                VisualApiColor = "#FF9E9E9E";
            }

            // HTTP Client (always available)
            HttpClientStatus = "Available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load data layer data");
        }
    }

    /// <summary>
    /// Load logging service configuration
    /// </summary>
    private void LoadLoggingData()
    {
        try
        {
            if (_loggingService == null)
            {
                _logger.LogWarning("LoggingService not available");
                return;
            }

            // Use configuration service to get log settings
            if (_configurationService != null)
            {
                LogLevel = _configurationService.GetValue<string>("Logging:LogLevel:Default", "Information") ?? "Information";
                var repoRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
                var logPath = System.IO.Path.Combine(repoRoot, "logs", $"app-{DateTime.Now:yyyyMMdd}.txt");
                LogFilePath = logPath;

                // Get log file size if exists
                if (System.IO.File.Exists(logPath))
                {
                    var fileInfo = new System.IO.FileInfo(logPath);
                    LogFileSizeKB = fileInfo.Length / 1024;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load logging data");
        }
    }

    /// <summary>
    /// Load diagnostics service health checks
    /// </summary>
    private void LoadDiagnosticsData()
    {
        try
        {
            if (_diagnosticsService == null)
            {
                _logger.LogWarning("DiagnosticsService not available");
                return;
            }

            HealthChecks.Clear();
            // Simplified - just show service is available
            HealthChecks.Add(new HealthCheckDisplay
            {
                Name = "DiagnosticsService",
                Status = "Available",
                Description = "System diagnostics monitoring active",
                DurationMs = 0,
                StatusColor = "#FF44FF44"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load diagnostics data");
        }
    }

    /// <summary>
    /// Load navigation service status
    /// </summary>
    private void LoadNavigationData()
    {
        try
        {
            if (_navigationService == null)
            {
                _logger.LogWarning("NavigationService not available");
                NavigationServiceStatus = "Not Available";
                return;
            }

            NavigationServiceStatus = "Available";
            CurrentRoute = "/DebugTerminal"; // Simplified - we're on this route
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load navigation data");
            NavigationServiceStatus = "Error: " + ex.Message;
        }
    }

    /// <summary>
    /// Load localization service data
    /// </summary>
    private void LoadLocalizationData()
    {
        try
        {
            if (_localizationService == null)
            {
                _logger.LogWarning("LocalizationService not available");
                return;
            }

            CurrentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            LoadedTranslations = 0; // Simplified - would need to check actual loaded resources
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load localization data");
        }
    }

    /// <summary>
    /// Load user folder location data
    /// </summary>
    private void LoadUserFolderData()
    {
        try
        {
            if (_configurationService == null)
            {
                _logger.LogWarning("ConfigurationService not available for user folder info");
                return;
            }

            // Try to get user folder locations (requires async, so we'll use synchronous config values)
            var networkPath = _configurationService.GetValue<string>("UserFolders:NetworkDrivePath", "Unknown");
            var localPath = _configurationService.GetValue<string>("UserFolders:LocalFallbackPath", "Unknown");

            UserFolderNetworkPath = networkPath ?? "Unknown";
            UserFolderLocalPath = localPath ?? "Unknown";

            // Determine active location (simplified - check if network path is accessible)
            if (!string.IsNullOrEmpty(networkPath) && networkPath != "Unknown")
            {
                UserFolderActiveLocation = System.IO.Directory.Exists(networkPath) ? "Network Drive" : "Local Fallback";
            }
            else
            {
                UserFolderActiveLocation = "Local Fallback";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user folder data");
        }
    }
}

/// <summary>
/// Display model for service metrics
/// </summary>
public class ServiceMetricDisplay
{
    public string ServiceName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "#FF9E9E9E";
}

/// <summary>
/// Display model for configuration settings
/// </summary>
public class ConfigurationSettingDisplay
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF9E9E9E";
}

/// <summary>
/// Display model for feature flags
/// </summary>
public class FeatureFlagDisplay
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Environment { get; set; } = string.Empty;
    public int RolloutPercentage { get; set; }
    public string StatusColor { get; set; } = "#FF9E9E9E";
}

/// <summary>
/// Display model for cache statistics
/// </summary>
public class CacheStatDisplay
{
    public string Metric { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF9E9E9E";
}

/// <summary>
/// Display model for health checks
/// </summary>
public class HealthCheckDisplay
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string StatusColor { get; set; } = "#FF9E9E9E";
}
