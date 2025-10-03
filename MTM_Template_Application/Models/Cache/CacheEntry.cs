using System;

namespace MTM_Template_Application.Models.Cache;

/// <summary>
/// Cached entry with compression and metadata
/// </summary>
public class CacheEntry
{
    /// <summary>
    /// Cache key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Compressed data using LZ4
    /// </summary>
    public byte[] CompressedValue { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Original size before compression in bytes
    /// </summary>
    public long UncompressedSizeBytes { get; set; }

    /// <summary>
    /// When this entry was created
    /// </summary>
    public DateTimeOffset CreatedUtc { get; set; }

    /// <summary>
    /// When this entry expires
    /// </summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    /// <summary>
    /// When this entry was last accessed
    /// </summary>
    public DateTimeOffset LastAccessedUtc { get; set; }

    /// <summary>
    /// Number of times this entry was accessed
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Type of entity cached (e.g., "Part", "Location")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
}
