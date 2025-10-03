using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Secrets;

/// <summary>
/// Manages encrypted credentials using OS-native secure storage
/// </summary>
public interface ISecretsService
{
    /// <summary>
    /// Store a secret securely
    /// </summary>
    Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a secret
    /// </summary>
    Task<string?> RetrieveSecretAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a secret
    /// </summary>
    Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotate a secret (store new value, delete old)
    /// </summary>
    Task RotateSecretAsync(string key, string newValue, CancellationToken cancellationToken = default);
}
