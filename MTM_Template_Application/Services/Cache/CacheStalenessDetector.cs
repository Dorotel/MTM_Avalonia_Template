using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Cache;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// Detect expired entries (Parts 24h, Others 7d) and trigger refresh
/// </summary>
public class CacheStalenessDetector
{
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _partsTtl = TimeSpan.FromHours(24);
    private readonly TimeSpan _othersTtl = TimeSpan.FromDays(7);
    private readonly TimeSpan _nearExpirationThreshold = TimeSpan.FromMinutes(30);

    public CacheStalenessDetector(ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        _cacheService = cacheService;
    }

    /// <summary>
    /// Detect all stale cache entries
    /// </summary>
    public async Task<List<StaleEntry>> DetectStaleEntriesAsync()
    {
        var staleEntries = new List<StaleEntry>();
        var now = DateTimeOffset.UtcNow;

        // Get all cache entries (in a real implementation, this would use internal cache access)
        // For now, this is a placeholder that would need access to the cache internals
        
        return staleEntries;
    }

    /// <summary>
    /// Get entries nearing expiration
    /// </summary>
    public async Task<List<StaleEntry>> GetEntriesNearExpirationAsync()
    {
        var nearExpiration = new List<StaleEntry>();
        var now = DateTimeOffset.UtcNow;
        var threshold = now.Add(_nearExpirationThreshold);

        // Get all cache entries nearing expiration
        // This would need access to cache internals
        
        return nearExpiration;
    }

    /// <summary>
    /// Check if an entry is stale based on entity type
    /// </summary>
    public bool IsStale(CacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!entry.ExpiresAtUtc.HasValue)
        {
            return false;
        }

        return entry.ExpiresAtUtc.Value < DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Get TTL for entity type
    /// </summary>
    public TimeSpan GetTtlForEntityType(string entityType)
    {
        return entityType switch
        {
            "Part" => _partsTtl,
            _ => _othersTtl
        };
    }

    /// <summary>
    /// Check if entry is near expiration
    /// </summary>
    public bool IsNearExpiration(CacheEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!entry.ExpiresAtUtc.HasValue)
        {
            return false;
        }

        var timeUntilExpiration = entry.ExpiresAtUtc.Value - DateTimeOffset.UtcNow;
        return timeUntilExpiration <= _nearExpirationThreshold && timeUntilExpiration > TimeSpan.Zero;
    }
}

/// <summary>
/// Represents a stale cache entry
/// </summary>
public class StaleEntry
{
    public string Key { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public DateTimeOffset LastAccessedUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
}
