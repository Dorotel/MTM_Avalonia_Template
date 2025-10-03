namespace MTM_Template_Application.Models.DataLayer;

/// <summary>
/// Metrics for database connection pool
/// </summary>
public class ConnectionPoolMetrics
{
    /// <summary>
    /// Name of the connection pool
    /// </summary>
    public string PoolName { get; set; } = string.Empty;

    /// <summary>
    /// Number of active connections
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Number of idle connections
    /// </summary>
    public int IdleConnections { get; set; }

    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; }

    /// <summary>
    /// Average time to acquire a connection in milliseconds
    /// </summary>
    public double AverageAcquireTimeMs { get; set; }

    /// <summary>
    /// Number of requests waiting for a connection
    /// </summary>
    public int WaitingRequests { get; set; }
}
