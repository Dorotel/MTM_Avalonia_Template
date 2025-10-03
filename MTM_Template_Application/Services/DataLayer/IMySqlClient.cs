using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Models.DataLayer;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// MySQL database client with connection pooling
/// </summary>
public interface IMySqlClient
{
    /// <summary>
    /// Execute a query and return results
    /// </summary>
    Task<List<T>> ExecuteQueryAsync<T>(string query, object? parameters = null);

    /// <summary>
    /// Execute a non-query command (INSERT, UPDATE, DELETE)
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string command, object? parameters = null);

    /// <summary>
    /// Execute a scalar query (COUNT, SUM, etc.)
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string query, object? parameters = null);

    /// <summary>
    /// Get connection pool metrics
    /// </summary>
    ConnectionPoolMetrics GetConnectionMetrics();
}
