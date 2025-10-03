using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Secrets;

/// <summary>
/// Factory for creating platform-specific secrets services
/// </summary>
public class SecretsServiceFactory
{
    public static ISecretsService Create(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsSecretsService(loggerFactory.CreateLogger<WindowsSecretsService>());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSSecretsService(loggerFactory.CreateLogger<MacOSSecretsService>());
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Check if this is Android
            var isAndroid = RuntimeInformation.RuntimeIdentifier.Contains("android", StringComparison.OrdinalIgnoreCase);
            if (isAndroid)
            {
                return new AndroidSecretsService(loggerFactory.CreateLogger<AndroidSecretsService>());
            }
        }

        throw new PlatformNotSupportedException($"Secrets service not available for platform: {RuntimeInformation.OSDescription}");
    }
}
