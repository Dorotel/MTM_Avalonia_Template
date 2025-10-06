// Service Contract: Diagnostics Service Extensions
// Spec: specs/003-debug-terminal-modernization/spec.md
// Features: Boot Timeline, Connection Pool Stats, Network Diagnostics (Phases 1-3)

namespace MTM_Template_Application.Services;

/// <summary>
/// Extensions to the existing IDiagnosticsService for Debug Terminal Modernization.
/// Adds boot timeline, connection pool stats, and network diagnostics.
/// </summary>
public interface IDiagnosticsServiceExtensions
{
    /// <summary>
    /// Gets the boot timeline for the current session.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boot timeline with Stage 0/1/2 breakdown</returns>
    /// <exception cref="InvalidOperationException">Thrown if boot timeline is not available</exception>
    Task<BootTimeline> GetBootTimelineAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets boot timelines for the last N boots (per CL-001, max 10).
    /// </summary>
    /// <param name="count">Number of historical boots to retrieve (1-10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Read-only list of boot timelines, most recent first</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if count &lt; 1 or count &gt; 10</exception>
    Task<IReadOnlyList<BootTimeline>> GetHistoricalBootTimelinesAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current connection pool statistics for MySQL and HTTP.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection pool stats snapshot</returns>
    /// <remarks>
    /// On Android: Returns graceful degradation message "Not Available on Android" per CL-004.
    /// </remarks>
    Task<ConnectionPoolStats> GetConnectionPoolStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs network diagnostics against specified endpoints.
    /// </summary>
    /// <param name="endpoints">List of endpoints to test (max 5)</param>
    /// <param name="timeoutSeconds">Timeout per endpoint (per CL-005, default 5s)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Network diagnostic results for each endpoint</returns>
    /// <exception cref="ArgumentException">Thrown if endpoints list is empty or has more than 5 entries</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if timeoutSeconds &lt; 1 or &gt; 30</exception>
    Task<IReadOnlyList<NetworkDiagnosticResult>> RunNetworkDiagnosticsAsync(
        IReadOnlyList<string> endpoints,
        int timeoutSeconds = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the diagnostic cache (Quick Action).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of cache entries cleared</returns>
    /// <remarks>
    /// Requires user confirmation per CL-008 (only Quick Action requiring confirmation).
    /// </remarks>
    Task<int> ClearCacheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces garbage collection (Quick Action).
    /// </summary>
    /// <returns>Memory freed in MB</returns>
    /// <remarks>
    /// No confirmation required per CL-008.
    /// </remarks>
    Task<long> ForceGarbageCollectionAsync();

    /// <summary>
    /// Restarts the application (Quick Action).
    /// </summary>
    /// <returns>Task that completes when restart is initiated</returns>
    /// <remarks>
    /// No confirmation required per CL-008.
    /// </remarks>
    Task RestartApplicationAsync();
}

/// <summary>
/// Network diagnostic result for a single endpoint.
/// </summary>
public sealed record NetworkDiagnosticResult
{
    public required string Endpoint { get; init; }
    public required bool IsReachable { get; init; }
    public required TimeSpan? ResponseTime { get; init; }
    public required int StatusCode { get; init; }
    public required string? ErrorMessage { get; init; }
}
