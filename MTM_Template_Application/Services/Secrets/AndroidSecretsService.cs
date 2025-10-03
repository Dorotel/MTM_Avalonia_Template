using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Secrets;

namespace MTM_Template_Application.Services.Secrets;

/// <summary>
/// Android KeyStore-based secrets service
/// </summary>
[SupportedOSPlatform("android")]
public class AndroidSecretsService : ISecretsService
{
    private readonly ILogger<AndroidSecretsService> _logger;

    public AndroidSecretsService(ILogger<AndroidSecretsService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Using placeholder implementation - will be replaced with actual Android KeyStore in production")]
    public Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            // In production, this would use Android KeyStore APIs
            // For now, use cross-platform DPAPI-like encryption
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var encryptedBytes = ProtectedData.Protect(
                valueBytes,
                null,
                DataProtectionScope.CurrentUser
            );

            var entry = new SecretEntry
            {
                Key = key,
                EncryptedValue = encryptedBytes,
                CreatedUtc = DateTimeOffset.UtcNow,
                LastAccessedUtc = DateTimeOffset.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["Platform"] = "Android",
                    ["DeviceId"] = GetDeviceId()
                }
            };

            SecretCache.Instance.Store(key, entry);
            _logger.LogInformation("Secret {Key} stored in Android KeyStore", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store secret {Key}", key);
            throw;
        }
    }

    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Using placeholder implementation - will be replaced with actual Android KeyStore in production")]
    public Task<string?> RetrieveSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);

        try
        {
            var entry = SecretCache.Instance.Retrieve(key);
            if (entry == null)
            {
                _logger.LogWarning("Secret {Key} not found", key);
                return Task.FromResult<string?>(null);
            }

            var decryptedBytes = ProtectedData.Unprotect(
                entry.EncryptedValue,
                null,
                DataProtectionScope.CurrentUser
            );

            var value = Encoding.UTF8.GetString(decryptedBytes);
            entry.LastAccessedUtc = DateTimeOffset.UtcNow;

            _logger.LogDebug("Secret {Key} retrieved from Android KeyStore", key);
            return Task.FromResult<string?>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret {Key}", key);
            return Task.FromResult<string?>(null);
        }
    }

    public Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);

        SecretCache.Instance.Delete(key);
        _logger.LogInformation("Secret {Key} deleted from Android KeyStore", key);
        return Task.CompletedTask;
    }

    public async Task RotateSecretAsync(string key, string newValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(newValue);

        await StoreSecretAsync(key, newValue, cancellationToken);
        _logger.LogInformation("Secret {Key} rotated in Android KeyStore", key);
    }

    private static string GetDeviceId()
    {
        // In production, would get from Android APIs
        return Environment.MachineName;
    }
}
