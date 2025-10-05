using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.Android.Services;

/// <summary>
/// Registers Android-specific services.
/// </summary>
public static class AndroidServiceRegistration
{
    /// <summary>
    /// Register all Android platform services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="loggerFactory">Logger factory for creating platform-specific loggers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAndroidServices(
        this IServiceCollection services,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        // Android-specific secrets service (Android KeyStore)
        ISecretsService secretsService = new AndroidSecretsService(
            loggerFactory.CreateLogger<AndroidSecretsService>()
        );

        // Add all common services with Android-specific secrets
        // Note: Visual API not available on Android (use HTTP API only)
        services.AddAllServices(secretsService, includeVisualApi: false);

        // TODO: Register Android-specific services here:
        // - Device certificate service (Android KeyStore integration)
        // - Android permissions service
        // - Camera/barcode scanner services
        // - Android-specific diagnostics

        return services;
    }
}
