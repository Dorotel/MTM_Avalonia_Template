using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Boot;

/// <summary>
/// Resolves service initialization order based on dependencies.
/// Ensures services are initialized in the correct sequence.
/// </summary>
public class ServiceDependencyResolver
{
    private readonly ILogger<ServiceDependencyResolver> _logger;
    private readonly Dictionary<string, ServiceNode> _services = new();

    public ServiceDependencyResolver(ILogger<ServiceDependencyResolver> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Register a service with its dependencies.
    /// </summary>
    public void RegisterService(string serviceName, params string[] dependencies)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        if (!_services.ContainsKey(serviceName))
        {
            _services[serviceName] = new ServiceNode(serviceName);
        }

        var node = _services[serviceName];
        foreach (var dependency in dependencies ?? Array.Empty<string>())
        {
            node.Dependencies.Add(dependency);

            // Ensure dependency exists
            if (!_services.ContainsKey(dependency))
            {
                _services[dependency] = new ServiceNode(dependency);
            }
        }

        _logger.LogDebug(
            "Registered service: {ServiceName}, Dependencies: [{Dependencies}]",
            serviceName,
            string.Join(", ", dependencies ?? Array.Empty<string>())
        );
    }

    /// <summary>
    /// Get services in initialization order (topological sort).
    /// Services with no dependencies come first.
    /// </summary>
    public List<string> GetInitializationOrder()
    {
        _logger.LogInformation("Resolving service initialization order");

        var result = new List<string>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var serviceName in _services.Keys)
        {
            if (!visited.Contains(serviceName))
            {
                TopologicalSort(serviceName, visited, recursionStack, result);
            }
        }

        // Reverse because we added in reverse order (dependencies first)
        result.Reverse();

        _logger.LogInformation(
            "Service initialization order: [{Order}]",
            string.Join(" → ", result)
        );

        return result;
    }

    /// <summary>
    /// Get services that can be initialized in parallel (no dependencies on each other).
    /// </summary>
    public List<List<string>> GetParallelGroups()
    {
        _logger.LogInformation("Resolving parallel service groups");

        var initOrder = GetInitializationOrder();
        var groups = new List<List<string>>();
        var initialized = new HashSet<string>();

        while (initialized.Count < initOrder.Count)
        {
            var parallelGroup = new List<string>();

            foreach (var service in initOrder)
            {
                if (initialized.Contains(service))
                {
                    continue;
                }

                // Check if all dependencies are initialized
                var node = _services[service];
                if (node.Dependencies.All(dep => initialized.Contains(dep)))
                {
                    parallelGroup.Add(service);
                }
            }

            if (parallelGroup.Count == 0)
            {
                // Circular dependency or error
                var remaining = initOrder.Where(s => !initialized.Contains(s)).ToList();
                _logger.LogError(
                    "Circular dependency detected. Remaining services: [{Services}]",
                    string.Join(", ", remaining)
                );
                throw new InvalidOperationException("Circular service dependency detected");
            }

            groups.Add(parallelGroup);
            foreach (var service in parallelGroup)
            {
                initialized.Add(service);
            }

            _logger.LogDebug(
                "Parallel group {GroupNumber}: [{Services}]",
                groups.Count,
                string.Join(", ", parallelGroup)
            );
        }

        _logger.LogInformation("Resolved {GroupCount} parallel service groups", groups.Count);
        return groups;
    }

    /// <summary>
    /// Validate that there are no circular dependencies.
    /// </summary>
    public bool ValidateDependencies(out List<string> circularDependencies)
    {
        circularDependencies = new List<string>();

        try
        {
            GetInitializationOrder();
            _logger.LogInformation("Service dependencies validated - no circular dependencies found");
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circular"))
        {
            _logger.LogError("Circular dependency validation failed");
            circularDependencies.AddRange(_services.Keys);
            return false;
        }
    }

    private void TopologicalSort(
        string serviceName,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string> result)
    {
        if (recursionStack.Contains(serviceName))
        {
            _logger.LogError("Circular dependency detected involving service: {ServiceName}", serviceName);
            throw new InvalidOperationException($"Circular dependency detected: {serviceName}");
        }

        if (visited.Contains(serviceName))
        {
            return;
        }

        recursionStack.Add(serviceName);
        visited.Add(serviceName);

        var node = _services[serviceName];
        foreach (var dependency in node.Dependencies)
        {
            if (_services.ContainsKey(dependency))
            {
                TopologicalSort(dependency, visited, recursionStack, result);
            }
            else
            {
                _logger.LogWarning(
                    "Service {ServiceName} depends on {Dependency} which is not registered",
                    serviceName,
                    dependency
                );
            }
        }

        recursionStack.Remove(serviceName);
        result.Add(serviceName);
    }

    private class ServiceNode
    {
        public string Name { get; }
        public List<string> Dependencies { get; } = new();

        public ServiceNode(string name)
        {
            Name = name;
        }
    }
}
