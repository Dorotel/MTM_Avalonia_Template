namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// MySQL and HTTP connection pool health metrics.
/// </summary>
public sealed record ConnectionPoolStats
{
    /// <summary>
    /// Gets the timestamp when these statistics were captured.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets MySQL connection pool statistics.
    /// </summary>
    public required MySqlPoolStats MySqlPool { get; init; }

    /// <summary>
    /// Gets HTTP client connection pool statistics.
    /// </summary>
    public required HttpPoolStats HttpPool { get; init; }
}

/// <summary>
/// MySQL connection pool statistics.
/// </summary>
public sealed record MySqlPoolStats
{
    /// <summary>
    /// Gets the total number of connections (active + idle).
    /// </summary>
    public required int TotalConnections { get; init; }

    /// <summary>
    /// Gets the number of active connections currently in use.
    /// </summary>
    public required int ActiveConnections { get; init; }

    /// <summary>
    /// Gets the number of idle connections in the pool.
    /// </summary>
    public required int IdleConnections { get; init; }

    /// <summary>
    /// Gets the number of requests waiting for a connection.
    /// </summary>
    public required int WaitingRequests { get; init; }

    /// <summary>
    /// Gets the average time requests wait for a connection.
    /// </summary>
    public required TimeSpan AverageWaitTime { get; init; }

    /// <summary>
    /// Validates that TotalConnections = ActiveConnections + IdleConnections.
    /// </summary>
    public bool IsValid()
    {
        return TotalConnections == ActiveConnections + IdleConnections
            && TotalConnections >= 0
            && ActiveConnections >= 0
            && IdleConnections >= 0
            && WaitingRequests >= 0
            && AverageWaitTime >= TimeSpan.Zero;
    }
}

/// <summary>
/// HTTP client connection pool statistics.
/// </summary>
public sealed record HttpPoolStats
{
    /// <summary>
    /// Gets the total number of HTTP connections (active + idle).
    /// </summary>
    public required int TotalConnections { get; init; }

    /// <summary>
    /// Gets the number of active HTTP connections currently in use.
    /// </summary>
    public required int ActiveConnections { get; init; }

    /// <summary>
    /// Gets the number of idle HTTP connections in the pool.
    /// </summary>
    public required int IdleConnections { get; init; }

    /// <summary>
    /// Gets the average response time for HTTP requests.
    /// </summary>
    public required TimeSpan AverageResponseTime { get; init; }

    /// <summary>
    /// Validates that TotalConnections = ActiveConnections + IdleConnections.
    /// </summary>
    public bool IsValid()
    {
        return TotalConnections == ActiveConnections + IdleConnections
            && TotalConnections >= 0
            && ActiveConnections >= 0
            && IdleConnections >= 0
            && AverageResponseTime >= TimeSpan.Zero;
    }
}
