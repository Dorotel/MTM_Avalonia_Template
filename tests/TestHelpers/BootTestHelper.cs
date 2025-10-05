using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Boot;
using MTM_Template_Application.Services.Boot.Stages;
using NSubstitute;

namespace MTM_Template_Tests.TestHelpers;

/// <summary>
/// Helper class for creating boot-related test infrastructure
/// </summary>
public static class BootTestHelper
{
    /// <summary>
    /// Creates a service collection with all dependencies for boot testing
    /// </summary>
    public static ServiceProvider CreateServiceCollection()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging();

        // Add mock services for testing (must be registered BEFORE boot stages to avoid null dependencies)
        var mockConfigService = Substitute.For<MTM_Template_Application.Services.Configuration.IConfigurationService>();
        var mockSecretsService = Substitute.For<MTM_Template_Application.Services.Secrets.ISecretsService>();
        var mockLoggingService = Substitute.For<MTM_Template_Application.Services.Logging.ILoggingService>();
        var mockDiagnosticsService = Substitute.For<MTM_Template_Application.Services.Diagnostics.IDiagnosticsService>();
        var mockMySqlClient = Substitute.For<MTM_Template_Application.Services.DataLayer.IMySqlClient>();
        var mockVisualApiClient = Substitute.For<MTM_Template_Application.Services.DataLayer.IVisualApiClient>();
        var mockCacheService = Substitute.For<MTM_Template_Application.Services.Cache.ICacheService>();
        var mockMessageBus = Substitute.For<MTM_Template_Application.Services.Core.IMessageBus>();
        var mockValidationService = Substitute.For<MTM_Template_Application.Services.Core.IValidationService>();
        var mockMappingService = Substitute.For<MTM_Template_Application.Services.Core.IMappingService>();
        var mockLocalizationService = Substitute.For<MTM_Template_Application.Services.Localization.ILocalizationService>();
        var mockThemeService = Substitute.For<MTM_Template_Application.Services.Theme.IThemeService>();
        var mockNavigationService = Substitute.For<MTM_Template_Application.Services.Navigation.INavigationService>();

        // Configure diagnostic service to return empty results
        var emptyDiagnosticResults = new List<MTM_Template_Application.Models.Diagnostics.DiagnosticResult>();
        mockDiagnosticsService.RunAllChecksAsync(Arg.Any<CancellationToken>())
            .Returns(emptyDiagnosticResults);

        // Configure MySQL client to return connection metrics
        var mockConnectionMetrics = new MTM_Template_Application.Models.DataLayer.ConnectionPoolMetrics
        {
            PoolName = "TestPool",
            ActiveConnections = 1,
            IdleConnections = 9,
            MaxPoolSize = 10,
            AverageAcquireTimeMs = 5.0,
            WaitingRequests = 0
        };
        mockMySqlClient.GetConnectionMetrics().Returns(mockConnectionMetrics);

        // Configure Visual API client
        mockVisualApiClient.IsServerAvailable().Returns(true);

        // Configure cache service
        var mockCacheStats = new MTM_Template_Application.Models.Cache.CacheStatistics
        {
            TotalEntries = 0,
            HitCount = 0,
            MissCount = 0,
            HitRate = 0.0,
            TotalSizeBytes = 0,
            CompressionRatio = 1.0,
            EvictionCount = 0
        };
        mockCacheService.GetStatistics().Returns(mockCacheStats);
        mockCacheService.RefreshAsync().Returns(System.Threading.Tasks.Task.CompletedTask);

        // Configure localization service
        var supportedCultures = new List<string> { "en-US", "es-MX", "fr-FR", "de-DE", "zh-CN" };
        mockLocalizationService.GetSupportedCultures().Returns(supportedCultures);

        // Configure theme service
        var mockThemeConfig = new MTM_Template_Application.Models.Theme.ThemeConfiguration
        {
            ThemeMode = "Auto",
            IsDarkMode = false,
            AccentColor = "#FF6B35",
            FontSize = 1.0,
            HighContrast = false,
            LastChangedUtc = DateTimeOffset.UtcNow
        };
        mockThemeService.GetCurrentTheme().Returns(mockThemeConfig);

        // Configure navigation service
        mockNavigationService.NavigateToAsync(
            Arg.Any<string>(),
            Arg.Any<Dictionary<string, object>?>(),
            Arg.Any<CancellationToken>())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Configure other service returns as needed
        mockLoggingService.FlushAsync(Arg.Any<CancellationToken>()).Returns(System.Threading.Tasks.Task.CompletedTask);

        services.AddSingleton(mockConfigService);
        services.AddSingleton(mockSecretsService);
        services.AddSingleton(mockLoggingService);
        services.AddSingleton(mockDiagnosticsService);
        services.AddSingleton(mockMySqlClient);
        services.AddSingleton(mockVisualApiClient);
        services.AddSingleton(mockCacheService);
        services.AddSingleton(mockMessageBus);
        services.AddSingleton(mockValidationService);
        services.AddSingleton(mockMappingService);
        services.AddSingleton(mockLocalizationService);
        services.AddSingleton(mockThemeService);
        services.AddSingleton(mockNavigationService);

        // Add boot utilities first (needed by BootOrchestrator)
        services.AddSingleton<BootProgressCalculator>();
        services.AddSingleton<BootWatchdog>();
        services.AddSingleton<ServiceDependencyResolver>();
        services.AddSingleton<ParallelServiceStarter>();

        // Add boot stages (after mocks so dependencies are available)
        services.AddSingleton<Stage0Bootstrap>();
        services.AddSingleton<Stage1ServicesInitialization>();

        // Register Stage2ApplicationReady explicitly with factory to ensure dependencies resolve
        services.AddSingleton<Stage2ApplicationReady>(sp =>
            new Stage2ApplicationReady(
                sp.GetRequiredService<ILogger<Stage2ApplicationReady>>(),
                sp.GetRequiredService<MTM_Template_Application.Services.Theme.IThemeService>(),
                sp.GetRequiredService<MTM_Template_Application.Services.Navigation.INavigationService>(),
                sp.GetRequiredService<MTM_Template_Application.Services.Localization.ILocalizationService>()
            )
        );

        // Register BootOrchestrator last (after all dependencies)
        services.AddSingleton<IBootOrchestrator, BootOrchestrator>();

        return services.BuildServiceProvider();
    }
}
