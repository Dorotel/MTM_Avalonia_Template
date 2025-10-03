using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Secrets;

namespace MTM_Template_Application.Services.Secrets;

/// <summary>
/// Windows DPAPI-based secrets service using Credential Manager
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsSecretsService : ISecretsService
{
    private readonly ILogger<WindowsSecretsService> _logger;

    public WindowsSecretsService(ILogger<WindowsSecretsService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            // Encrypt using DPAPI (Data Protection API)
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var encryptedBytes = ProtectedData.Protect(
                valueBytes,
                null, // No additional entropy
                DataProtectionScope.CurrentUser // User-specific encryption
            );

            // Store in Credential Manager (simplified - would use Windows Credential Manager API in production)
            var entry = new SecretEntry
            {
                Key = key,
                EncryptedValue = encryptedBytes,
                CreatedUtc = DateTimeOffset.UtcNow,
                LastAccessedUtc = DateTimeOffset.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["SecretType"] = DetermineSecretType(key),
                    ["UserId"] = Environment.UserName,
                    ["MachineId"] = Environment.MachineName
                }
            };

            // For now, store in a simple in-memory cache (production would use Windows Credential Manager)
            SecretCache.Instance.Store(key, entry);

            _logger.LogInformation("Secret {Key} stored successfully using Windows DPAPI", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store secret {Key}", key);
            throw;
        }
    }

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

            // Decrypt using DPAPI
            var decryptedBytes = ProtectedData.Unprotect(
                entry.EncryptedValue,
                null,
                DataProtectionScope.CurrentUser
            );

            var value = Encoding.UTF8.GetString(decryptedBytes);

            // Update last accessed timestamp
            entry.LastAccessedUtc = DateTimeOffset.UtcNow;

            _logger.LogDebug("Secret {Key} retrieved successfully", key);
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

        try
        {
            SecretCache.Instance.Delete(key);
            _logger.LogInformation("Secret {Key} deleted successfully", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret {Key}", key);
            throw;
        }
    }

    public Task RotateSecretAsync(string key, string newValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(newValue);

        try
        {
            var entry = SecretCache.Instance.Retrieve(key);
            if (entry == null)
            {
                _logger.LogWarning("Secret {Key} not found for rotation", key);
                throw new InvalidOperationException($"Secret {key} not found");
            }

            // Encrypt new value
            var valueBytes = Encoding.UTF8.GetBytes(newValue);
            var encryptedBytes = ProtectedData.Protect(
                valueBytes,
                null,
                DataProtectionScope.CurrentUser
            );

            entry.EncryptedValue = encryptedBytes;
            entry.Metadata["LastRotated"] = DateTimeOffset.UtcNow.ToString("O");

            SecretCache.Instance.Store(key, entry);

            _logger.LogInformation("Secret {Key} rotated successfully", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate secret {Key}", key);
            throw;
        }
    }

    private static string DetermineSecretType(string key)
    {
        var lowerKey = key.ToLowerInvariant();

        if (lowerKey.Contains("password"))
            return "Password";
        if (lowerKey.Contains("apikey") || lowerKey.Contains("api_key"))
            return "ApiKey";
        if (lowerKey.Contains("connection") || lowerKey.Contains("connectionstring"))
            return "ConnectionString";
        if (lowerKey.Contains("certificate") || lowerKey.Contains("cert"))
            return "Certificate";

        return "Password"; // Default
    }
}

/// <summary>
/// Simple in-memory cache for secrets (production would use Windows Credential Manager)
/// </summary>
internal class SecretCache
{
    private static readonly Lazy<SecretCache> _instance = new(() => new SecretCache());
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SecretEntry> _cache = new();

    public static SecretCache Instance => _instance.Value;

    public void Store(string key, SecretEntry entry) => _cache[key] = entry;

    public SecretEntry? Retrieve(string key) => _cache.TryGetValue(key, out var entry) ? entry : null;

    public void Delete(string key) => _cache.TryRemove(key, out _);
}
