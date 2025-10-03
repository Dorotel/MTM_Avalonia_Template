using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Secrets;

/// <summary>
/// Encrypted credential stored in OS-native secure storage
/// </summary>
public class SecretEntry
{
    /// <summary>
    /// Unique key for the secret
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted value
    /// </summary>
    public byte[] EncryptedValue { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// When this secret was created
    /// </summary>
    public DateTimeOffset CreatedUtc { get; set; }

    /// <summary>
    /// When this secret was last accessed
    /// </summary>
    public DateTimeOffset LastAccessedUtc { get; set; }

    /// <summary>
    /// Optional expiration time
    /// </summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Additional metadata about the secret
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
