using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Cache;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// Interface for detecting expired cache entries (Parts 24h, Others 7d) and triggering refresh
/// </summary>
public interface ICacheStalenessDetector
{
    /// <summary>
    /// Detect all stale cache entries
    /// </summary>
    Task<List<StaleEntry>> DetectStaleEntriesAsync();

    /// <summary>
    /// Get entries nearing expiration
    /// </summary>
    Task<List<StaleEntry>> GetEntriesNearExpirationAsync();

    /// <summary>
    /// Check if an entry is stale based on entity type
    /// </summary>
    bool IsStale(CacheEntry entry);

    /// <summary>
    /// Get TTL for entity type
    /// </summary>
    TimeSpan GetTtlForEntityType(string entityType);

    /// <summary>
    /// Check if an entry is near expiration
    /// </summary>
    bool IsNearExpiration(CacheEntry entry);
}
