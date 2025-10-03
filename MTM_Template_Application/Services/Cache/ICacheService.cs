using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Cache;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// LZ4-compressed cache service for Visual master data
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached item
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Set cached item
    /// </summary>
    Task SetAsync<T>(string key, T value, System.TimeSpan? expiration = null);

    /// <summary>
    /// Remove cached item
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear all cached items
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    CacheStatistics GetStatistics();

    /// <summary>
    /// Refresh stale cache entries
    /// </summary>
    Task RefreshAsync();
}
