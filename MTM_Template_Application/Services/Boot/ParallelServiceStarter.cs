using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Starts independent services in parallel for improved boot performance.
/// Coordinates parallel execution while respecting dependencies.
/// </summary>
public class ParallelServiceStarter
{
    private readonly ILogger<ParallelServiceStarter> _logger;
    private readonly ServiceDependencyResolver _dependencyResolver;

    public ParallelServiceStarter(
        ILogger<ParallelServiceStarter> logger,
        ServiceDependencyResolver dependencyResolver)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dependencyResolver);

        _logger = logger;
        _dependencyResolver = dependencyResolver;
    }

    /// <summary>
    /// Start services in parallel groups, respecting dependencies.
    /// </summary>
    /// <param name="serviceInitializers">Dictionary mapping service names to initialization functions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Results of service initialization</returns>
    public async Task<ParallelStartResult> StartServicesAsync(
        Dictionary<string, Func<CancellationToken, Task>> serviceInitializers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceInitializers);

        _logger.LogInformation("Starting parallel service initialization for {ServiceCount} services", serviceInitializers.Count);

        var result = new ParallelStartResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get parallel groups from dependency resolver
            var parallelGroups = _dependencyResolver.GetParallelGroups();

            _logger.LogInformation("Executing {GroupCount} parallel service groups", parallelGroups.Count);

            foreach (var group in parallelGroups)
            {
                await StartGroupAsync(group, serviceInitializers, result, cancellationToken);
            }

            stopwatch.Stop();
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Parallel service initialization completed. Total: {TotalMs}ms, Success: {SuccessCount}, Failed: {FailedCount}",
                result.TotalDurationMs,
                result.SuccessfulServices.Count,
                result.FailedServices.Count
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            _logger.LogError(ex, "Parallel service initialization failed");
            throw;
        }
    }

    /// <summary>
    /// Start a single group of services in parallel.
    /// </summary>
    private async Task StartGroupAsync(
        List<string> group,
        Dictionary<string, Func<CancellationToken, Task>> serviceInitializers,
        ParallelStartResult result,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting parallel group: [{Services}]", string.Join(", ", group));

        var groupStopwatch = Stopwatch.StartNew();

        // Create tasks for all services in this group
        var tasks = group
            .Where(serviceName => serviceInitializers.ContainsKey(serviceName))
            .Select(serviceName => StartServiceAsync(serviceName, serviceInitializers[serviceName], result, cancellationToken))
            .ToList();

        if (tasks.Count == 0)
        {
            _logger.LogWarning("No initializers found for group: [{Services}]", string.Join(", ", group));
            return;
        }

        // Wait for all services in group to complete
        await Task.WhenAll(tasks);

        groupStopwatch.Stop();

        _logger.LogDebug(
            "Parallel group completed in {DurationMs}ms. Services: [{Services}]",
            groupStopwatch.ElapsedMilliseconds,
            string.Join(", ", group)
        );
    }

    /// <summary>
    /// Start a single service and track its result.
    /// </summary>
    private async Task StartServiceAsync(
        string serviceName,
        Func<CancellationToken, Task> initializer,
        ParallelStartResult result,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Starting service: {ServiceName}", serviceName);

            await initializer(cancellationToken);

            stopwatch.Stop();

            result.SuccessfulServices.Add(serviceName);
            result.ServiceDurations[serviceName] = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Service initialized successfully: {ServiceName} ({DurationMs}ms)",
                serviceName,
                stopwatch.ElapsedMilliseconds
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            result.FailedServices.Add(serviceName);
            result.ServiceErrors[serviceName] = ex.Message;
            result.ServiceDurations[serviceName] = stopwatch.ElapsedMilliseconds;

            _logger.LogError(
                ex,
                "Service initialization failed: {ServiceName} ({DurationMs}ms)",
                serviceName,
                stopwatch.ElapsedMilliseconds
            );

            // Re-throw to fail the group (all services in group must succeed)
            throw;
        }
    }

    /// <summary>
    /// Start services sequentially (fallback when parallelization is not desired).
    /// </summary>
    public async Task<ParallelStartResult> StartServicesSequentiallyAsync(
        Dictionary<string, Func<CancellationToken, Task>> serviceInitializers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceInitializers);

        _logger.LogInformation("Starting sequential service initialization for {ServiceCount} services", serviceInitializers.Count);

        var result = new ParallelStartResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var initOrder = _dependencyResolver.GetInitializationOrder();

            foreach (var serviceName in initOrder)
            {
                if (serviceInitializers.ContainsKey(serviceName))
                {
                    await StartServiceAsync(serviceName, serviceInitializers[serviceName], result, cancellationToken);
                }
            }

            stopwatch.Stop();
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Sequential service initialization completed. Total: {TotalMs}ms, Success: {SuccessCount}, Failed: {FailedCount}",
                result.TotalDurationMs,
                result.SuccessfulServices.Count,
                result.FailedServices.Count
            );

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            _logger.LogError(ex, "Sequential service initialization failed");
            throw;
        }
    }
}

/// <summary>
/// Results of parallel service initialization.
/// </summary>
public class ParallelStartResult
{
    public List<string> SuccessfulServices { get; } = new();
    public List<string> FailedServices { get; } = new();
    public Dictionary<string, long> ServiceDurations { get; } = new();
    public Dictionary<string, string> ServiceErrors { get; } = new();
    public long TotalDurationMs { get; set; }

    public bool IsSuccess => FailedServices.Count == 0;

    public string GetSummary()
    {
        return $"Success: {SuccessfulServices.Count}, Failed: {FailedServices.Count}, Total: {TotalDurationMs}ms";
    }
}
