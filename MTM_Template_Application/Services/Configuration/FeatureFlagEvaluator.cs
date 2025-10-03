using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Evaluates feature flags based on environment and rollout percentage
/// </summary>
public class FeatureFlagEvaluator
{
    private readonly ILogger<FeatureFlagEvaluator> _logger;
    private readonly Dictionary<string, FeatureFlag> _flags = new();
    private readonly Random _random = new();

    public FeatureFlagEvaluator(ILogger<FeatureFlagEvaluator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Register a feature flag
    /// </summary>
    public void RegisterFlag(FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        lock (_flags)
        {
            _flags[flag.Name] = flag;
            _logger.LogDebug("Feature flag {FlagName} registered", flag.Name);
        }
    }

    /// <summary>
    /// Evaluate if a feature flag is enabled
    /// </summary>
    public Task<bool> IsEnabledAsync(string flagName)
    {
        ArgumentNullException.ThrowIfNull(flagName);

        lock (_flags)
        {
            if (!_flags.TryGetValue(flagName, out var flag))
            {
                _logger.LogWarning("Feature flag {FlagName} not found, defaulting to disabled", flagName);
                return Task.FromResult(false);
            }

            // Check environment match
            var currentEnvironment = GetCurrentEnvironment();
            if (!string.IsNullOrEmpty(flag.Environment) && 
                !flag.Environment.Equals(currentEnvironment, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Feature flag {FlagName} disabled for environment {Environment}", 
                    flagName, currentEnvironment);
                return Task.FromResult(false);
            }

            // Check rollout percentage
            if (flag.RolloutPercentage < 100)
            {
                var randomValue = _random.Next(0, 100);
                var isEnabled = randomValue < flag.RolloutPercentage;
                
                _logger.LogDebug("Feature flag {FlagName} rollout evaluation: {RandomValue} < {Rollout} = {Enabled}",
                    flagName, randomValue, flag.RolloutPercentage, isEnabled);
                
                return Task.FromResult(isEnabled);
            }

            // Update evaluation timestamp
            flag.EvaluatedAt = DateTimeOffset.UtcNow;

            _logger.LogDebug("Feature flag {FlagName} evaluated as {Enabled}", flagName, flag.IsEnabled);
            return Task.FromResult(flag.IsEnabled);
        }
    }

    /// <summary>
    /// Get all registered feature flags
    /// </summary>
    public Task<IEnumerable<FeatureFlag>> GetAllFlagsAsync()
    {
        lock (_flags)
        {
            return Task.FromResult<IEnumerable<FeatureFlag>>(_flags.Values);
        }
    }

    /// <summary>
    /// Update a feature flag's enabled state
    /// </summary>
    public Task SetEnabledAsync(string flagName, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(flagName);

        lock (_flags)
        {
            if (_flags.TryGetValue(flagName, out var flag))
            {
                flag.IsEnabled = enabled;
                flag.EvaluatedAt = DateTimeOffset.UtcNow;
                
                _logger.LogInformation("Feature flag {FlagName} set to {Enabled}", flagName, enabled);
            }
            else
            {
                _logger.LogWarning("Feature flag {FlagName} not found, cannot update", flagName);
            }
        }

        return Task.CompletedTask;
    }

    private static string GetCurrentEnvironment()
    {
        // Check environment variable first
        var env = Environment.GetEnvironmentVariable("MTM_ENVIRONMENT") 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        if (!string.IsNullOrEmpty(env))
        {
            return env;
        }

        // Default to Development in debug builds
        #if DEBUG
        return "Development";
        #else
        return "Production";
        #endif
    }
}
