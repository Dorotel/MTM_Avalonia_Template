using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.Desktop.Services;

/// <summary>
/// Registers Desktop-specific services (Windows, Linux).
/// Note: macOS support removed - project targets Windows desktop and Android only.
/// </summary>
public static class DesktopServiceRegistration
{
    /// <summary>
    /// Register all Desktop platform services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="loggerFactory">Logger factory for creating platform-specific loggers.</param>
    /// <param name="includeVisualApi">Whether to include Visual API client (desktop has direct access).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDesktopServices(
        this IServiceCollection services,
        ILoggerFactory loggerFactory,
        bool includeVisualApi = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        // Platform-specific secrets service
        ISecretsService secretsService = GetPlatformSecretsService(loggerFactory);

        // Add all common services with platform-specific secrets
        services.AddAllServices(secretsService, includeVisualApi);

        return services;
    }

    /// <summary>
    /// Get the appropriate secrets service for the current desktop platform.
    /// </summary>
    private static ISecretsService GetPlatformSecretsService(ILoggerFactory loggerFactory)
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSecretsService(loggerFactory.CreateLogger<WindowsSecretsService>());
        }

        if (OperatingSystem.IsLinux())
        {
            // Linux doesn't have platform-specific secrets service yet
            // For now, throw NotSupportedException to make it clear
            // TODO: Implement LinuxSecretsService using Secret Service API (libsecret)
            throw new PlatformNotSupportedException(
                "Linux secrets service not yet implemented. Please implement LinuxSecretsService using libsecret.");
        }

        throw new PlatformNotSupportedException(
            $"Desktop platform not supported: {Environment.OSVersion.Platform}. Only Windows is currently supported.");
    }
}
