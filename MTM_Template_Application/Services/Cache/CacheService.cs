using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Cache;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// In-memory cache service with LZ4 compression and TTL enforcement
/// </summary>
public class CacheService : ICacheService
{
    private readonly ILogger<CacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly LZ4CompressionHandler _compressionHandler;
    private readonly object _statsLock = new object();
    private long _hitCount;
    private long _missCount;
    private long _evictionCount;

    public CacheService(
        ILogger<CacheService> logger,
        LZ4CompressionHandler compressionHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(compressionHandler);

        _logger = logger;
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _compressionHandler = compressionHandler;

        _logger.LogInformation("CacheService initialized with LZ4 compression");
    }

    /// <summary>
    /// Get cached item
    /// </summary>
    public Task<T?> GetAsync<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAtUtc.HasValue && entry.ExpiresAtUtc.Value < DateTimeOffset.UtcNow)
            {
                _logger.LogDebug("Cache key {Key} expired, removing", key);
                // Remove expired entry
                _cache.TryRemove(key, out _);
                IncrementMiss();
                IncrementEviction();
                return Task.FromResult<T?>(default);
            }

            // Update access metrics
            entry.LastAccessedUtc = DateTimeOffset.UtcNow;
            entry.AccessCount++;

            IncrementHit();
            _logger.LogDebug("Cache hit for key: {Key}", key);

            // Decompress and deserialize
            var json = _compressionHandler.Decompress(entry.CompressedValue);
            return Task.FromResult(JsonSerializer.Deserialize<T>(json));
        }

        IncrementMiss();
        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    /// <summary>
    /// Set cached item
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        _logger.LogDebug("Setting cache key: {Key}, Type: {Type}, Expiration: {Expiration}",
            key, typeof(T).Name, expiration?.TotalSeconds.ToString() ?? "none");

        // Serialize and compress
        var json = JsonSerializer.Serialize(value);
        var compressed = _compressionHandler.Compress(json);

        var uncompressedSize = System.Text.Encoding.UTF8.GetByteCount(json);
        var compressionRatio = (double)compressed.Length / uncompressedSize;

        _logger.LogDebug("Compressed {Original}B to {Compressed}B (ratio: {Ratio:F2})",
            uncompressedSize, compressed.Length, compressionRatio);

        var entry = new CacheEntry
        {
            Key = key,
            CompressedValue = compressed,
            UncompressedSizeBytes = uncompressedSize,
            CreatedUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : null,
            LastAccessedUtc = DateTimeOffset.UtcNow,
            AccessCount = 0,
            EntityType = typeof(T).Name
        };

        _cache[key] = entry;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove cached item
    /// </summary>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);

        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug("Cache key removed: {Key}", key);
            IncrementEviction();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clear all cached items
    /// </summary>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var count = _cache.Count;
        _cache.Clear();

        lock (_statsLock)
        {
            _evictionCount += count;
        }

        _logger.LogInformation("Cache cleared. {Count} entries removed", count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        lock (_statsLock)
        {
            var totalSize = 0L;
            var totalUncompressed = 0L;

            foreach (var entry in _cache.Values)
            {
                totalSize += entry.CompressedValue.Length;
                totalUncompressed += entry.UncompressedSizeBytes;
            }

            var compressionRatio = totalUncompressed > 0
                ? (double)totalSize / totalUncompressed
                : 0;

            var totalRequests = _hitCount + _missCount;
            var hitRate = totalRequests > 0
                ? (double)_hitCount / totalRequests
                : 0;

            return new CacheStatistics
            {
                TotalEntries = _cache.Count,
                HitCount = _hitCount,
                MissCount = _missCount,
                HitRate = hitRate,
                TotalSizeBytes = totalSize,
                CompressionRatio = compressionRatio,
                EvictionCount = _evictionCount
            };
        }
    }

    /// <summary>
    /// Refresh stale cache entries
    /// </summary>
    public async Task RefreshAsync()
    {
        _logger.LogDebug("Refreshing stale cache entries");

        var expiredKeys = new System.Collections.Generic.List<string>();
        var now = DateTimeOffset.UtcNow;

        foreach (var kvp in _cache)
        {
            if (kvp.Value.ExpiresAtUtc.HasValue && kvp.Value.ExpiresAtUtc.Value < now)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            await RemoveAsync(key);
        }

        _logger.LogInformation("Cache refresh complete. {Count} expired entries removed", expiredKeys.Count);
    }

    private void IncrementHit()
    {
        lock (_statsLock)
        {
            _hitCount++;
        }
    }

    private void IncrementMiss()
    {
        lock (_statsLock)
        {
            _missCount++;
        }
    }

    private void IncrementEviction()
    {
        lock (_statsLock)
        {
            _evictionCount++;
        }
    }
}
