using System;
using Microsoft.Extensions.DependencyInjection;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Boot.Stages;
using MTM_Template_Application.Services.Cache;
using MTM_Template_Application.Services.Configuration;
using MTM_Template_Application.Services.Core;
using MTM_Template_Application.Services.DataLayer;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Localization;
using MTM_Template_Application.Services.Logging;
using MTM_Template_Application.Services.Navigation;
using MTM_Template_Application.Services.Secrets;
using MTM_Template_Application.Services.Theme;
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

        // Boot orchestration
        services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
        services.AddSingleton<Stage0Bootstrap>();
        services.AddSingleton<Stage1ServicesInitialization>();
        services.AddSingleton<Stage2ApplicationReady>();

        // Boot utilities
        services.AddSingleton<BootProgressCalculator>();
        services.AddSingleton<BootWatchdog>();
        services.AddSingleton<ServiceDependencyResolver>();
        services.AddSingleton<ParallelServiceStarter>();

        return services;
    }

    /// <summary>
    /// Add configuration management services.
    /// </summary>
    public static IServiceCollection AddConfigurationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IConfigurationService, ConfigurationService>();

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

        services.AddSingleton<ILoggingService, LoggingService>();

        return services;
    }

    /// <summary>
    /// Add diagnostics and health check services.
    /// </summary>
    public static IServiceCollection AddDiagnosticsServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IDiagnosticsService, DiagnosticsService>();

        return services;
    }

    /// <summary>
    /// Add data layer services (MySQL, Visual API, HTTP client).
    /// </summary>
    public static IServiceCollection AddDataLayerServices(this IServiceCollection services, bool includeVisualApi = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        // MySQL client
        services.AddSingleton<IMySqlClient, MySqlClient>();

        // Visual API client (not available on Android)
        if (includeVisualApi)
        {
            services.AddSingleton<IVisualApiClient, VisualApiClient>();
        }

        // HTTP API client
        services.AddSingleton<IHttpApiClient, HttpApiClient>();

        return services;
    }

    /// <summary>
    /// Add caching services.
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// Add core application services (message bus, validation, mapping).
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMessageBus, MessageBus>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IMappingService, MappingService>();

        return services;
    }

    /// <summary>
    /// Add localization services.
    /// </summary>
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILocalizationService, LocalizationService>();

        return services;
    }

    /// <summary>
    /// Add theme management services.
    /// </summary>
    public static IServiceCollection AddThemeServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IThemeService, ThemeService>();

        return services;
    }

    /// <summary>
    /// Add navigation services.
    /// </summary>
    public static IServiceCollection AddNavigationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<INavigationService, NavigationService>();

        return services;
    }

    /// <summary>
    /// Add error handling and recovery services.
    /// </summary>
    public static IServiceCollection AddErrorHandlingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // TODO: Implement error handling services (T134-T137)
        // services.AddSingleton<GlobalExceptionHandler>();
        // services.AddSingleton<ErrorCategorizer>();
        // services.AddSingleton<RecoveryStrategy>();
        // services.AddSingleton<DiagnosticBundleGenerator>();

        return services;
    }

    /// <summary>
    /// Add all ViewModels.
    /// </summary>
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<SplashViewModel>();
        // Add other ViewModels as needed

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
            .AddDataLayerServices(includeVisualApi)
            .AddCachingServices()
            .AddCoreServices()
            .AddLocalizationServices()
            .AddThemeServices()
            .AddNavigationServices()
            .AddErrorHandlingServices()
            .AddViewModels();
    }
}
