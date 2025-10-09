using System.Collections.Concurrent;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Boot;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Extensions to the existing IDiagnosticsService for Debug Terminal Modernization.
/// Adds boot timeline, connection pool stats, error history, and network diagnostics.
/// </summary>
public sealed class DiagnosticsServiceExtensions : IDiagnosticsServiceExtensions
{
    private readonly ILogger<DiagnosticsServiceExtensions> _logger;
    private readonly IBootOrchestrator _bootOrchestrator;
    private readonly ConcurrentQueue<ErrorEntry> _errorBuffer = new();
    private readonly int _maxErrorBufferSize = 100;
    private readonly List<Models.Boot.BootMetrics> _bootHistory = new();
    private readonly int _maxBootHistorySize = 10;

    public DiagnosticsServiceExtensions(
        ILogger<DiagnosticsServiceExtensions> logger,
        IBootOrchestrator bootOrchestrator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(bootOrchestrator);

        _logger = logger;
        _bootOrchestrator = bootOrchestrator;
    }

    /// <inheritdoc/>
    public async Task<BootTimeline> GetBootTimelineAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.CompletedTask; // Async consistency

        var bootMetrics = _bootOrchestrator.GetBootMetrics();

        if (bootMetrics == null)
        {
            throw new InvalidOperationException("Boot timeline is not available");
        }

        return MapBootMetricsToTimeline(bootMetrics);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BootTimeline>> GetHistoricalBootTimelinesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count < 1 || count > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be between 1 and 10 (per CL-001)");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await Task.CompletedTask; // Async consistency

        lock (_bootHistory)
        {
            return _bootHistory
                .TakeLast(count)
                .Select(MapBootMetricsToTimeline)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ErrorEntry>> GetRecentErrorsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count must be at least 1");
        }

        cancellationToken.ThrowIfCancellationRequested();

        await Task.CompletedTask; // Async consistency

        // Clamp count to buffer size (100) per CL-007
        var effectiveCount = Math.Min(count, 100);

        return _errorBuffer
            .TakeLast(effectiveCount)
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<ConnectionPoolStats> GetConnectionPoolStatsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.CompletedTask; // Async consistency

        try
        {
            // Check if running on Android (graceful degradation per CL-004)
            if (OperatingSystem.IsAndroid())
            {
                _logger.LogInformation("Connection pool stats not available on Android (per CL-004)");
                return CreateEmptyConnectionPoolStats("Not Available on Android");
            }

            // TODO: Query actual MySql.Data connection pool metrics
            // For now, return empty stats with graceful degradation
            return CreateEmptyConnectionPoolStats();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve connection pool stats - returning zeros");
            return CreateEmptyConnectionPoolStats("Error retrieving stats");
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<NetworkDiagnosticResult>> RunNetworkDiagnosticsAsync(
        IReadOnlyList<string> endpoints,
        int timeoutSeconds = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        if (endpoints.Count == 0 || endpoints.Count > 5)
        {
            throw new ArgumentException("Endpoints list must have between 1 and 5 entries", nameof(endpoints));
        }

        if (timeoutSeconds < 1 || timeoutSeconds > 30)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds),
                "Timeout must be between 1 and 30 seconds");
        }

        var results = new List<NetworkDiagnosticResult>();

        foreach (var endpoint in endpoints)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                var startTime = DateTime.UtcNow;
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                var response = await httpClient.GetAsync(endpoint, cts.Token);
                var responseTime = DateTime.UtcNow - startTime;

                results.Add(new NetworkDiagnosticResult
                {
                    Endpoint = endpoint,
                    IsReachable = response.IsSuccessStatusCode,
                    ResponseTime = responseTime,
                    StatusCode = (int)response.StatusCode,
                    ErrorMessage = null
                });
            }
            catch (OperationCanceledException)
            {
                results.Add(new NetworkDiagnosticResult
                {
                    Endpoint = endpoint,
                    IsReachable = false,
                    ResponseTime = null,
                    StatusCode = 0,
                    ErrorMessage = "Request timed out"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Network diagnostic failed for endpoint {Endpoint}", endpoint);
                results.Add(new NetworkDiagnosticResult
                {
                    Endpoint = endpoint,
                    IsReachable = false,
                    ResponseTime = null,
                    StatusCode = 0,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results.AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<int> ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: Implement cache clearing logic
        // This requires integration with the cache service
        _logger.LogInformation("Cache clear requested (placeholder implementation)");

        await Task.CompletedTask;
        return 0; // Return count of cleared entries
    }

    /// <inheritdoc/>
    public async Task<long> ForceGarbageCollectionAsync()
    {
        var beforeMemory = GC.GetTotalMemory(false) / 1024 / 1024; // MB

        _logger.LogInformation("Forcing garbage collection (all generations)");

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var afterMemory = GC.GetTotalMemory(false) / 1024 / 1024; // MB
        var freedMemory = beforeMemory - afterMemory;

        _logger.LogInformation("Garbage collection freed {FreedMemory}MB (before: {Before}MB, after: {After}MB)",
            freedMemory, beforeMemory, afterMemory);

        await Task.CompletedTask; // Async consistency
        return freedMemory;
    }

    /// <inheritdoc/>
    public async Task RestartApplicationAsync()
    {
        _logger.LogWarning("Application restart requested");

        // TODO: Implement application restart logic
        // This requires platform-specific restart mechanisms

        await Task.CompletedTask;
        throw new NotImplementedException("Application restart not yet implemented");
    }

    /// <summary>
    /// Adds an error to the internal error buffer.
    /// </summary>
    public void AddError(ErrorEntry error)
    {
        ArgumentNullException.ThrowIfNull(error);

        _errorBuffer.Enqueue(error);

        // Maintain buffer size limit (per CL-007: session-only, last 100 entries)
        while (_errorBuffer.Count > _maxErrorBufferSize)
        {
            _errorBuffer.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Adds boot metrics to history.
    /// </summary>
    public void AddBootMetricsToHistory(Models.Boot.BootMetrics bootMetrics)
    {
        ArgumentNullException.ThrowIfNull(bootMetrics);

        lock (_bootHistory)
        {
            _bootHistory.Add(bootMetrics);

            // Maintain history size limit (per CL-001: last 10 boots)
            while (_bootHistory.Count > _maxBootHistorySize)
            {
                _bootHistory.RemoveAt(0);
            }
        }
    }

    private static BootTimeline MapBootMetricsToTimeline(Models.Boot.BootMetrics bootMetrics)
    {
        var serviceTimings = bootMetrics.ServiceMetrics
            .Select(s => new ServiceInitInfo
            {
                ServiceName = s.ServiceName,
                Duration = TimeSpan.FromMilliseconds(s.DurationMs ?? 0),
                Success = s.Success
            })
            .ToList();

        return new BootTimeline
        {
            BootStartTime = bootMetrics.StartTimestamp.UtcDateTime,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromMilliseconds(bootMetrics.Stage0DurationMs),
                Success = bootMetrics.SuccessStatus == Models.Boot.BootStatus.Success
                    || bootMetrics.Stage1DurationMs.HasValue,
                ErrorMessage = bootMetrics.ErrorMessage
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromMilliseconds(bootMetrics.Stage1DurationMs ?? 0),
                Success = bootMetrics.SuccessStatus == Models.Boot.BootStatus.Success
                    || bootMetrics.Stage2DurationMs.HasValue,
                ServiceTimings = serviceTimings,
                ErrorMessage = bootMetrics.ErrorMessage
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromMilliseconds(bootMetrics.Stage2DurationMs ?? 0),
                Success = bootMetrics.SuccessStatus == Models.Boot.BootStatus.Success,
                ErrorMessage = bootMetrics.ErrorMessage
            },
            TotalBootTime = TimeSpan.FromMilliseconds(bootMetrics.TotalDurationMs ?? 0)
        };
    }

    private static ConnectionPoolStats CreateEmptyConnectionPoolStats(string? message = null)
    {
        return new ConnectionPoolStats
        {
            Timestamp = DateTime.UtcNow,
            MySqlPool = new MySqlPoolStats
            {
                TotalConnections = 0,
                ActiveConnections = 0,
                IdleConnections = 0,
                WaitingRequests = 0,
                AverageWaitTime = TimeSpan.Zero
            },
            HttpPool = new HttpPoolStats
            {
                TotalConnections = 0,
                ActiveConnections = 0,
                IdleConnections = 0,
                AverageResponseTime = TimeSpan.Zero
            }
        };
    }
}
