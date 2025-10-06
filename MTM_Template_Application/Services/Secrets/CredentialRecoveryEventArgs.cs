using System;

namespace MTM_Template_Application.Services.Secrets;

/// <summary>
/// Event arguments for credential recovery events
/// </summary>
public class CredentialRecoveryEventArgs : EventArgs
{
    /// <summary>
    /// The key of the secret that failed to retrieve
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The exception that triggered the recovery need
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// The type of failure that occurred
    /// </summary>
    public CredentialRecoveryFailureType FailureType { get; init; }

    /// <summary>
    /// User-friendly message describing the issue
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Types of credential recovery failures
/// </summary>
public enum CredentialRecoveryFailureType
{
    /// <summary>
    /// Credentials are corrupted or encryption failed
    /// </summary>
    CorruptedCredentials,

    /// <summary>
    /// Access to OS-native storage was denied (permissions issue)
    /// </summary>
    AccessDenied,

    /// <summary>
    /// Credentials were not found in storage
    /// </summary>
    NotFound,

    /// <summary>
    /// Unknown error occurred
    /// </summary>
    Unknown
}
