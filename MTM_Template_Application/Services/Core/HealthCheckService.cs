using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// Aggregate health checks from all services
/// </summary>
public class HealthCheckService
{
    private readonly List<IHealthCheck> _healthChecks;

    public HealthCheckService(IEnumerable<IHealthCheck> healthChecks)
    {
        ArgumentNullException.ThrowIfNull(healthChecks);
        _healthChecks = healthChecks.ToList();
    }

    /// <summary>
    /// Run all health checks
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = new List<IndividualHealthCheckResult>();

        foreach (var check in _healthChecks)
        {
            try
            {
                var result = await check.CheckHealthAsync();
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new IndividualHealthCheckResult
                {
                    Name = check.GetType().Name,
                    IsHealthy = false,
                    Message = $"Health check failed: {ex.Message}",
                    Data = new Dictionary<string, object>
                    {
                        ["Exception"] = ex.ToString()
                    }
                });
            }
        }

        var overallHealth = results.All(r => r.IsHealthy);

        return new HealthCheckResult
        {
            IsHealthy = overallHealth,
            CheckedAt = DateTimeOffset.UtcNow,
            Checks = results
        };
    }

    /// <summary>
    /// Get health status for a specific service
    /// </summary>
    public async Task<IndividualHealthCheckResult?> CheckServiceHealthAsync(string serviceName)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        var check = _healthChecks.FirstOrDefault(c =>
            c.GetType().Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (check == null)
        {
            return null;
        }

        try
        {
            return await check.CheckHealthAsync();
        }
        catch (Exception ex)
        {
            return new IndividualHealthCheckResult
            {
                Name = serviceName,
                IsHealthy = false,
                Message = $"Health check failed: {ex.Message}",
                Data = new Dictionary<string, object>
                {
                    ["Exception"] = ex.ToString()
                }
            };
        }
    }
}

/// <summary>
/// Interface for service health checks
/// </summary>
public interface IHealthCheck
{
    Task<IndividualHealthCheckResult> CheckHealthAsync();
}

/// <summary>
/// Overall health check result
/// </summary>
public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
    public List<IndividualHealthCheckResult> Checks { get; set; } = new();
}

/// <summary>
/// Individual service health check result
/// </summary>
public class IndividualHealthCheckResult
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}
