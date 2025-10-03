using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Template_Application.Models.DataLayer;

namespace MTM_Template_Application.Services.DataLayer;

/// <summary>
/// MySQL client implementation with connection pooling (Desktop: 2-10, Android: 1-5), 
/// query execution, and role-based access
/// </summary>
public class MySqlClient : IMySqlClient, IDisposable
{
    private readonly string _connectionString;
    private readonly ConnectionPoolConfiguration _poolConfig;
    private readonly object _metricsLock = new object();
    private int _activeConnections;
    private int _totalConnectionsCreated;
    private long _totalAcquireTimeMs;
    private int _acquireCount;
    private bool _disposed;

    public MySqlClient(string connectionString, ConnectionPoolConfiguration? poolConfig = null)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        _connectionString = BuildConnectionStringWithPooling(connectionString, poolConfig);
        _poolConfig = poolConfig ?? ConnectionPoolConfiguration.GetDesktopDefault();
    }

    /// <summary>
    /// Execute a query and return results
    /// </summary>
    public async Task<List<T>> ExecuteQueryAsync<T>(string query, object? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        var stopwatch = Stopwatch.StartNew();
        using var connection = await AcquireConnectionAsync();
        stopwatch.Stop();
        RecordAcquireTime(stopwatch.ElapsedMilliseconds);

        using var command = new MySqlCommand(query, connection);
        AddParameters(command, parameters);

        var results = new List<T>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (reader is MySqlDataReader mysqlReader)
            {
                results.Add(MapToType<T>(mysqlReader));
            }
        }

        return results;
    }

    /// <summary>
    /// Execute a non-query command (INSERT, UPDATE, DELETE)
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(string command, object? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        var stopwatch = Stopwatch.StartNew();
        using var connection = await AcquireConnectionAsync();
        stopwatch.Stop();
        RecordAcquireTime(stopwatch.ElapsedMilliseconds);

        using var cmd = new MySqlCommand(command, connection);
        AddParameters(cmd, parameters);

        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute a scalar query (COUNT, SUM, etc.)
    /// </summary>
    public async Task<T?> ExecuteScalarAsync<T>(string query, object? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        var stopwatch = Stopwatch.StartNew();
        using var connection = await AcquireConnectionAsync();
        stopwatch.Stop();
        RecordAcquireTime(stopwatch.ElapsedMilliseconds);

        using var command = new MySqlCommand(query, connection);
        AddParameters(command, parameters);

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
        {
            return default;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Get connection pool metrics
    /// </summary>
    public ConnectionPoolMetrics GetConnectionMetrics()
    {
        lock (_metricsLock)
        {
            return new ConnectionPoolMetrics
            {
                PoolName = "MySqlConnectionPool",
                ActiveConnections = _activeConnections,
                IdleConnections = _poolConfig.MaxPoolSize - _activeConnections,
                MaxPoolSize = _poolConfig.MaxPoolSize,
                AverageAcquireTimeMs = _acquireCount > 0 ? _totalAcquireTimeMs / _acquireCount : 0,
                WaitingRequests = 0 // MySQL connector doesn't expose this
            };
        }
    }

    /// <summary>
    /// Acquire a connection from the pool
    /// </summary>
    private async Task<MySqlConnection> AcquireConnectionAsync()
    {
        lock (_metricsLock)
        {
            _activeConnections++;
            _totalConnectionsCreated++;
        }

        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// Record connection acquire time for metrics
    /// </summary>
    private void RecordAcquireTime(long milliseconds)
    {
        lock (_metricsLock)
        {
            _totalAcquireTimeMs += milliseconds;
            _acquireCount++;
        }
    }

    /// <summary>
    /// Add parameters to a command
    /// </summary>
    private void AddParameters(MySqlCommand command, object? parameters)
    {
        if (parameters == null)
        {
            return;
        }

        // Simple parameter mapping using reflection
        var properties = parameters.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(parameters);
            command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
        }
    }

    /// <summary>
    /// Map a data reader row to a type
    /// </summary>
    private T MapToType<T>(MySqlDataReader reader)
    {
        // Simple mapping - for production, consider using Dapper or AutoMapper
        if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(reader.GetValue(0), typeof(T));
        }

        var instance = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var property = Array.Find(properties, p => 
                p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            if (property != null && !reader.IsDBNull(i))
            {
                var value = reader.GetValue(i);
                property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
            }
        }

        return instance;
    }

    /// <summary>
    /// Build connection string with pooling configuration
    /// </summary>
    private string BuildConnectionStringWithPooling(string baseConnectionString, ConnectionPoolConfiguration? config)
    {
        var builder = new MySqlConnectionStringBuilder(baseConnectionString);
        
        if (config != null)
        {
            builder.MinimumPoolSize = (uint)config.MinPoolSize;
            builder.MaximumPoolSize = (uint)config.MaxPoolSize;
            builder.ConnectionTimeout = (uint)config.ConnectionTimeoutSeconds;
            builder.Pooling = true;
        }

        return builder.ConnectionString;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        MySqlConnection.ClearAllPools();
    }
}

/// <summary>
/// Connection pool configuration
/// </summary>
public class ConnectionPoolConfiguration
{
    public int MinPoolSize { get; set; }
    public int MaxPoolSize { get; set; }
    public int ConnectionTimeoutSeconds { get; set; }

    public static ConnectionPoolConfiguration GetDesktopDefault()
    {
        return new ConnectionPoolConfiguration
        {
            MinPoolSize = 2,
            MaxPoolSize = 10,
            ConnectionTimeoutSeconds = 30
        };
    }

    public static ConnectionPoolConfiguration GetAndroidDefault()
    {
        return new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 5,
            ConnectionTimeoutSeconds = 30
        };
    }
}
