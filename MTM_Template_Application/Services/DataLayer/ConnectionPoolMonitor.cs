using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.DataLayer;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// Track connection pool metrics and emit telemetry
/// </summary>
public class ConnectionPoolMonitor : IDisposable
{
    private readonly List<IConnectionMetricsProvider> _providers;
    private readonly Timer _monitorTimer;
    private readonly TimeSpan _monitorInterval;
    private readonly Action<Dictionary<string, ConnectionPoolMetrics>>? _metricsCallback;
    private bool _disposed;

    public ConnectionPoolMonitor(
        IEnumerable<IConnectionMetricsProvider> providers,
        TimeSpan? monitorInterval = null,
        Action<Dictionary<string, ConnectionPoolMetrics>>? metricsCallback = null)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _providers = providers.ToList();
        _monitorInterval = monitorInterval ?? TimeSpan.FromSeconds(30);
        _metricsCallback = metricsCallback;

        _monitorTimer = new Timer(OnMonitorTick, null, _monitorInterval, _monitorInterval);
    }

    /// <summary>
    /// Get current metrics from all providers
    /// </summary>
    public Dictionary<string, ConnectionPoolMetrics> GetCurrentMetrics()
    {
        var metrics = new Dictionary<string, ConnectionPoolMetrics>();

        foreach (var provider in _providers)
        {
            try
            {
                var providerMetrics = provider.GetConnectionMetrics();
                metrics[providerMetrics.PoolName] = providerMetrics;
            }
            catch (Exception ex)
            {
                // Log error but continue with other providers
                Console.WriteLine($"Error getting metrics from provider: {ex.Message}");
            }
        }

        return metrics;
    }

    /// <summary>
    /// Get aggregated metrics across all pools
    /// </summary>
    public AggregatedConnectionMetrics GetAggregatedMetrics()
    {
        var allMetrics = GetCurrentMetrics();
        var avgAcquireTime = allMetrics.Count > 0 
            ? (long)allMetrics.Average(m => m.Value.AverageAcquireTimeMs) 
            : 0;
        
        return new AggregatedConnectionMetrics
        {
            TotalPools = allMetrics.Count,
            TotalActiveConnections = allMetrics.Sum(m => m.Value.ActiveConnections),
            TotalIdleConnections = allMetrics.Sum(m => m.Value.IdleConnections),
            TotalMaxPoolSize = allMetrics.Sum(m => m.Value.MaxPoolSize),
            AverageAcquireTimeMs = avgAcquireTime,
            TotalWaitingRequests = allMetrics.Sum(m => m.Value.WaitingRequests),
            PoolMetrics = allMetrics
        };
    }

    /// <summary>
    /// Monitor timer callback
    /// </summary>
    private void OnMonitorTick(object? state)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            var metrics = GetCurrentMetrics();
            _metricsCallback?.Invoke(metrics);

            // Log metrics for telemetry
            LogMetrics(metrics);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in connection pool monitor: {ex.Message}");
        }
    }

    /// <summary>
    /// Log metrics to telemetry
    /// </summary>
    private void LogMetrics(Dictionary<string, ConnectionPoolMetrics> metrics)
    {
        foreach (var kvp in metrics)
        {
            var poolName = kvp.Key;
            var poolMetrics = kvp.Value;

            Console.WriteLine($"[ConnectionPool:{poolName}] " +
                $"Active={poolMetrics.ActiveConnections}, " +
                $"Idle={poolMetrics.IdleConnections}, " +
                $"Max={poolMetrics.MaxPoolSize}, " +
                $"AvgAcquireMs={poolMetrics.AverageAcquireTimeMs}, " +
                $"Waiting={poolMetrics.WaitingRequests}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _monitorTimer?.Dispose();
    }
}

/// <summary>
/// Interface for connection metrics providers
/// </summary>
public interface IConnectionMetricsProvider
{
    /// <summary>
    /// Get connection pool metrics
    /// </summary>
    ConnectionPoolMetrics GetConnectionMetrics();
}

/// <summary>
/// Aggregated metrics across all connection pools
/// </summary>
public class AggregatedConnectionMetrics
{
    public int TotalPools { get; set; }
    public int TotalActiveConnections { get; set; }
    public int TotalIdleConnections { get; set; }
    public int TotalMaxPoolSize { get; set; }
    public long AverageAcquireTimeMs { get; set; }
    public int TotalWaitingRequests { get; set; }
    public Dictionary<string, ConnectionPoolMetrics> PoolMetrics { get; set; } = new();
}
