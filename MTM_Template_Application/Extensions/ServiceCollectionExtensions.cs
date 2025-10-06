using System;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Boot.Stages;
using MTM_Template_Application.Services.Cache;
using MTM_Template_Application.Services.Configuration;
using MTM_Template_Application.Services.Core;
using MTM_Template_Application.Services.DataLayer;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;
using MTM_Template_Application.Services.Localization;
using MTM_Template_Application.Services.Logging;
using MTM_Template_Application.Services.Navigation;
using MTM_Template_Application.Services.Secrets;
using MTM_Template_Application.Services.Theme;
using MTM_Template_Application.Services.Visual;
using MTM_Template_Application.ViewModels;

namespace MTM_Template_Application.Extensions;

/// <summary>
/// Extension methods for registering application services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all boot-related services.
    /// </summary>
    public static IServiceCollection AddBootServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddBootServices() - Registering boot orchestration services");

        // Boot orchestration
        Serilog.Log.Verbose("[DI] Registering IBootOrchestrator -> BootOrchestrator");
        services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
        Serilog.Log.Verbose("[DI] Registering Stage0Bootstrap");
        services.AddSingleton<Stage0Bootstrap>();
        Serilog.Log.Verbose("[DI] Registering Stage1ServicesInitialization");
        services.AddSingleton<Stage1ServicesInitialization>();
        Serilog.Log.Verbose("[DI] Registering Stage2ApplicationReady");
        services.AddSingleton<Stage2ApplicationReady>();

        // Boot utilities
        Serilog.Log.Verbose("[DI] Registering boot utilities (Calculator, Watchdog, Resolver, Starter)");
        services.AddSingleton<BootProgressCalculator>();
        services.AddSingleton<BootWatchdog>();
        services.AddSingleton<ServiceDependencyResolver>();
        services.AddSingleton<ParallelServiceStarter>();

        Serilog.Log.Information("[DI] AddBootServices() - Registered 8 boot services");
        return services;
    }

    /// <summary>
    /// Add configuration management services.
    /// </summary>
    public static IServiceCollection AddConfigurationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddConfigurationServices() - Entry");

        Serilog.Log.Verbose("[DI] Registering IConfigurationService -> ConfigurationService");
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        Serilog.Log.Verbose("[DI] Registering FeatureFlagEvaluator");
        services.AddSingleton<FeatureFlagEvaluator>();

        Serilog.Log.Information("[DI] AddConfigurationServices() - Registered 2 configuration services");
        return services;
    }

    /// <summary>
    /// Add secrets management services (platform-specific, registered separately).
    /// </summary>
    public static IServiceCollection AddSecretsServices(this IServiceCollection services, ISecretsService secretsService)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(secretsService);

        services.AddSingleton(secretsService);

        return services;
    }

    /// <summary>
    /// Add logging and telemetry services.
    /// </summary>
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddLoggingServices() - Entry");

        // PII redaction middleware
        Serilog.Log.Verbose("[DI] Registering PiiRedactionMiddleware");
        services.AddSingleton<PiiRedactionMiddleware>();

        // Telemetry batch processor
        Serilog.Log.Verbose("[DI] Registering TelemetryBatchProcessor");
        services.AddSingleton<TelemetryBatchProcessor>();

        // Logging service
        Serilog.Log.Verbose("[DI] Registering ILoggingService -> LoggingService");
        services.AddSingleton<ILoggingService, LoggingService>();

        Serilog.Log.Information("[DI] AddLoggingServices() - Registered 3 logging services");
        return services;
    }

    /// <summary>
    /// Add diagnostics and health check services.
    /// </summary>
    public static IServiceCollection AddDiagnosticsServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddDiagnosticsServices() - Entry");

        // Hardware detection
        Serilog.Log.Verbose("[DI] Registering HardwareDetection");
        services.AddSingleton<HardwareDetection>();

        // Diagnostic checks
        Serilog.Log.Verbose("[DI] Registering diagnostic checks");
        services.AddSingleton<IDiagnosticCheck, StorageDiagnostic>();
        services.AddSingleton<IDiagnosticCheck, PermissionsDiagnostic>();
        services.AddSingleton<IDiagnosticCheck, NetworkDiagnostic>();

        // Diagnostics service
        Serilog.Log.Verbose("[DI] Registering IDiagnosticsService -> DiagnosticsService");
        services.AddSingleton<IDiagnosticsService, DiagnosticsService>();

        Serilog.Log.Information("[DI] AddDiagnosticsServices() - Registered 5 diagnostic services");
        return services;
    }

    /// <summary>
    /// Add data layer services (MySQL, Visual API, HTTP client).
    /// </summary>
    public static IServiceCollection AddDataLayerServices(
        this IServiceCollection services,
        string? mySqlConnectionString = null,
        bool includeVisualApi = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddDataLayerServices() - Entry (includeVisualApi={IncludeVisual})", includeVisualApi);

        // HttpClient for API communication
        Serilog.Log.Verbose("[DI] Registering HttpClient");
        services.AddSingleton<HttpClient>(sp =>
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            client.DefaultRequestHeaders.Add("User-Agent", "MTM_Template_Application");
            return client;
        });

        Serilog.Log.Verbose("[DI] HttpClient registered");

        // MySQL client (connection string provided by caller or will be set later)
        Serilog.Log.Verbose("[DI] Registering IMySqlClient");
        if (!string.IsNullOrWhiteSpace(mySqlConnectionString))
        {
            services.AddSingleton<IMySqlClient>(sp => new MySqlClient(mySqlConnectionString));
        }
        else
        {
            // Register as transient - will need connection string from configuration service
            services.AddSingleton<IMySqlClient, MySqlClient>(sp =>
            {
                // Default connection string - should be overridden by configuration
                var defaultConnectionString = "Server=localhost;Port=3306;Database=mtm_template;Uid=root;Pwd=root;";
                return new MySqlClient(defaultConnectionString);
            });
        }
        Serilog.Log.Verbose("[DI] IMySqlClient registered");

        // Visual API Whitelist Validator
        if (includeVisualApi)
        {
            Serilog.Log.Verbose("[DI] Registering IVisualApiWhitelistValidator");
            services.AddSingleton<IVisualApiWhitelistValidator, VisualApiWhitelistValidator>();
            Serilog.Log.Verbose("[DI] IVisualApiWhitelistValidator registered");
        }

        // Visual API client (not available on Android)
        if (includeVisualApi)
        {
            Serilog.Log.Verbose("[DI] Registering IVisualApiClient");
            services.AddSingleton<IVisualApiClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();

                // TODO: Load from configuration service
                var baseUrl = "https://visualapi.example.com/api"; // Placeholder

                // Whitelisted commands from VISUAL-WHITELIST.md
                var whitelistedCommands = new[]
                {
                    "GetItems",
                    "GetParts",
                    "GetLocations",
                    "GetWarehouses",
                    "GetInventory",
                    "GetOrders",
                    "GetCustomers",
                    "GetVendors"
                };

                var logger = sp.GetRequiredService<ILogger<VisualApiClient>>();
                return new VisualApiClient(logger, httpClient, baseUrl, whitelistedCommands);
            });
            Serilog.Log.Verbose("[DI] IVisualApiClient registered");
        }
        else
        {
            // Register null instance for platforms without Visual API (Android)
            Serilog.Log.Verbose("[DI] Registering IVisualApiClient as null (Visual API not available on this platform)");
            services.AddSingleton<IVisualApiClient>(_ => null!);
            Serilog.Log.Verbose("[DI] IVisualApiClient registered as null");
        }

        // Connection pool monitor (needs IConnectionMetricsProvider implementations)
        Serilog.Log.Verbose("[DI] Registering ConnectionPoolMonitor");
        services.AddSingleton<ConnectionPoolMonitor>(sp =>
        {
            // Get all connection metrics providers
            var providers = sp.GetServices<IConnectionMetricsProvider>();
            return new ConnectionPoolMonitor(providers);
        });
        Serilog.Log.Verbose("[DI] ConnectionPoolMonitor registered");

        Serilog.Log.Information("[DI] AddDataLayerServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add caching services.
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddCachingServices() - Entry");

        // Compression handler
        Serilog.Log.Verbose("[DI] Registering LZ4CompressionHandler");
        services.AddSingleton<LZ4CompressionHandler>();

        Serilog.Log.Verbose("[DI] LZ4CompressionHandler registered");

        // Cache mode manager (needs IVisualApiClient, registered conditionally)
        Serilog.Log.Verbose("[DI] Registering CachedOnlyModeManager");
        services.AddSingleton<CachedOnlyModeManager>(sp =>
        {
            // Try to get Visual API client (may not be registered on Android)
            var visualApiClient = sp.GetService<IVisualApiClient>();
            if (visualApiClient == null)
            {
                throw new InvalidOperationException("CachedOnlyModeManager requires IVisualApiClient, which is not available on this platform.");
            }
            return new CachedOnlyModeManager(visualApiClient);
        });
        Serilog.Log.Verbose("[DI] CachedOnlyModeManager registered");

        // Cache service
        Serilog.Log.Verbose("[DI] Registering ICacheService");
        services.AddSingleton<ICacheService, CacheService>();
        Serilog.Log.Verbose("[DI] ICacheService registered");

        // Cache staleness detector
        Serilog.Log.Verbose("[DI] Registering ICacheStalenessDetector");
        services.AddSingleton<ICacheStalenessDetector, CacheStalenessDetector>();
        Serilog.Log.Verbose("[DI] ICacheStalenessDetector registered");

        Serilog.Log.Information("[DI] AddCachingServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add core application services (message bus, validation, mapping).
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddCoreServices() - Entry");

        // Message bus
        Serilog.Log.Verbose("[DI] Registering IMessageBus");
        services.AddSingleton<IMessageBus, MessageBus>();

        Serilog.Log.Verbose("[DI] IMessageBus registered");

        // Validation service
        Serilog.Log.Verbose("[DI] Registering IValidationService");
        services.AddSingleton<IValidationService, ValidationService>();
        Serilog.Log.Verbose("[DI] IValidationService registered");

        // AutoMapper - configure with auto-discovered profiles
        Serilog.Log.Verbose("[DI] Registering AutoMapper");
        services.AddSingleton<IMapper>(sp =>
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(ServiceCollectionExtensions).Assembly);
            });
            return config.CreateMapper();
        });
        Serilog.Log.Verbose("[DI] AutoMapper registered");

        // Mapping service
        Serilog.Log.Verbose("[DI] Registering IMappingService");
        services.AddSingleton<IMappingService, MappingService>();
        Serilog.Log.Verbose("[DI] IMappingService registered");

        // Health check service (no health checks registered by default)
        Serilog.Log.Verbose("[DI] Registering HealthCheckService");
        services.AddSingleton<HealthCheckService>(sp =>
        {
            return new HealthCheckService(Enumerable.Empty<IHealthCheck>());
        });
        Serilog.Log.Verbose("[DI] HealthCheckService registered");

        Serilog.Log.Information("[DI] AddCoreServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add localization services.
    /// </summary>
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddLocalizationServices() - Entry");

        // Missing translation handler
        Serilog.Log.Verbose("[DI] Registering MissingTranslationHandler");
        services.AddSingleton<MissingTranslationHandler>();

        // Culture provider
        Serilog.Log.Verbose("[DI] Registering CultureProvider");
        services.AddSingleton<CultureProvider>();

        // Localization service
        Serilog.Log.Verbose("[DI] Registering ILocalizationService");
        services.AddSingleton<ILocalizationService, LocalizationService>();

        Serilog.Log.Information("[DI] AddLocalizationServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add theme management services.
    /// </summary>
    public static IServiceCollection AddThemeServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddThemeServices() - Entry");

        // OS dark mode monitor
        Serilog.Log.Verbose("[DI] Registering IOSDarkModeMonitor");
        services.AddSingleton<IOSDarkModeMonitor, OSDarkModeMonitor>();

        // Theme service
        Serilog.Log.Verbose("[DI] Registering IThemeService");
        services.AddSingleton<IThemeService, ThemeService>();

        Serilog.Log.Information("[DI] AddThemeServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add navigation services.
    /// </summary>
    public static IServiceCollection AddNavigationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddNavigationServices() - Entry");

        // Unsaved changes guard
        Serilog.Log.Verbose("[DI] Registering UnsavedChangesGuard");
        services.AddSingleton<UnsavedChangesGuard>();

        // Navigation service
        Serilog.Log.Verbose("[DI] Registering INavigationService");
        services.AddSingleton<INavigationService, NavigationService>();

        Serilog.Log.Information("[DI] AddNavigationServices() - Completed successfully");
        return services;
    }

    /// <summary>
    /// Add error handling and recovery services.
    /// </summary>
    public static IServiceCollection AddErrorHandlingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddErrorHandlingServices() - Entry (stub, no services registered)");

        // TODO: Implement error handling services (T134-T137)
        // services.AddSingleton<GlobalExceptionHandler>();
        // services.AddSingleton<ErrorCategorizer>();
        // services.AddSingleton<RecoveryStrategy>();
        // services.AddSingleton<DiagnosticBundleGenerator>();

        Serilog.Log.Information("[DI] AddErrorHandlingServices() - Completed (no services registered yet)");
        return services;
    }

    /// <summary>
    /// Add all ViewModels.
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Serilog.Log.Debug("[DI] AddViewModels() - Entry");

        Serilog.Log.Verbose("[DI] Registering SplashViewModel");
        services.AddTransient<SplashViewModel>();

        Serilog.Log.Verbose("[DI] Registering MainViewModel");
        services.AddTransient<MainViewModel>();

        Serilog.Log.Verbose("[DI] Registering DebugTerminalViewModel");
        services.AddTransient<DebugTerminalViewModel>();

        Serilog.Log.Information("[DI] AddViewModels() - Registered 3 ViewModels");
        return services;
    }

    /// <summary>
    /// Add all application services (convenience method).
    /// </summary>
    public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        ISecretsService secretsService,
        bool includeVisualApi = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(secretsService);

        return services
            .AddBootServices()
            .AddConfigurationServices()
            .AddSecretsServices(secretsService)
            .AddLoggingServices()
            .AddDiagnosticsServices()
            .AddDataLayerServices(mySqlConnectionString: null, includeVisualApi: includeVisualApi)
            .AddCachingServices()
            .AddCoreServices()
            .AddLocalizationServices()
            .AddThemeServices()
            .AddNavigationServices()
            .AddErrorHandlingServices()
            .AddViewModels();
    }
}
