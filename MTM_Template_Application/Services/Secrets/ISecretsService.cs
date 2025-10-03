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
    Task StoreSecretAsync(string key, string value);

    /// <summary>
    /// Retrieve a secret
    /// </summary>
    Task<string?> RetrieveSecretAsync(string key);

    /// <summary>
    /// Delete a secret
    /// </summary>
    Task DeleteSecretAsync(string key);

    /// <summary>
    /// Rotate a secret (store new value, delete old)
    /// </summary>
    Task RotateSecretAsync(string key, string newValue);
}
