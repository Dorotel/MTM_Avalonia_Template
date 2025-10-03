namespace MTM_Template_Application.Models.Cache;

/// <summary>
/// Statistics about cache performance
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// Total number of entries in cache
    /// </summary>
    public int TotalEntries { get; set; }

    /// <summary>
    /// Number of cache hits
    /// </summary>
    public long HitCount { get; set; }

    /// <summary>
    /// Number of cache misses
    /// </summary>
    public long MissCount { get; set; }

    /// <summary>
    /// Cache hit rate (0.0 to 1.0)
    /// </summary>
    public double HitRate { get; set; }

    /// <summary>
    /// Total size of cached data in bytes
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Average compression ratio achieved
    /// </summary>
    public double CompressionRatio { get; set; }

    /// <summary>
    /// Number of entries evicted from cache
    /// </summary>
    public long EvictionCount { get; set; }
}
