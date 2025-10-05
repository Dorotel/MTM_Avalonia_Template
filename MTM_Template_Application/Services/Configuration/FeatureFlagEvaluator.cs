using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;

namespace MTM_Template_Application.Services.Configuration;

/// <summary>
/// Evaluates feature flags based on environment and rollout percentage
/// </summary>
public class FeatureFlagEvaluator
{
    private readonly ILogger<FeatureFlagEvaluator> _logger;
    private readonly Dictionary<string, FeatureFlag> _flags = new();
    private readonly Random _random = new(); // Fallback for non-deterministic scenarios

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

        // Validate RolloutPercentage
        if (flag.RolloutPercentage < 0 || flag.RolloutPercentage > 100)
        {
            throw new ArgumentException($"RolloutPercentage must be between 0 and 100, got {flag.RolloutPercentage}", nameof(flag));
        }

        lock (_flags)
        {
            // Handle duplicate flag registration: Update existing flag properties
            if (_flags.ContainsKey(flag.Name))
            {
                _logger.LogInformation("Feature flag {FlagName} already registered, updating properties", flag.Name);
            }

            _flags[flag.Name] = flag;
            _logger.LogDebug("Feature flag {FlagName} registered with rollout {Rollout}%", flag.Name, flag.RolloutPercentage);
        }
    }

    /// <summary>
    /// Evaluate if a feature flag is enabled (non-deterministic - uses random)
    /// </summary>
    public Task<bool> IsEnabledAsync(string flagName)
    {
        return IsEnabledAsync(flagName, userId: null);
    }

    /// <summary>
    /// Evaluate if a feature flag is enabled with deterministic user-based rollout
    /// </summary>
    /// <param name="flagName">The name of the feature flag</param>
    /// <param name="userId">Optional user ID for deterministic rollout. If null, uses random.</param>
    public Task<bool> IsEnabledAsync(string flagName, int? userId)
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
                int rolloutValue;

                if (userId.HasValue)
                {
                    // Deterministic hash-based evaluation
                    rolloutValue = ComputeDeterministicRollout(userId.Value, flagName);
                    _logger.LogDebug("Feature flag {FlagName} deterministic rollout for user {UserId}: {RolloutValue} < {Rollout}",
                        flagName, userId.Value, rolloutValue, flag.RolloutPercentage);
                }
                else
                {
                    // Fallback to random evaluation
                    rolloutValue = _random.Next(0, 100);
                    _logger.LogDebug("Feature flag {FlagName} random rollout evaluation: {RandomValue} < {Rollout}",
                        flagName, rolloutValue, flag.RolloutPercentage);
                }

                var isEnabled = rolloutValue < flag.RolloutPercentage;

                // Update evaluation timestamp
                flag.EvaluatedAt = DateTimeOffset.UtcNow;

                return Task.FromResult(flag.IsEnabled && isEnabled);
            }

            // Update evaluation timestamp
            flag.EvaluatedAt = DateTimeOffset.UtcNow;

            _logger.LogDebug("Feature flag {FlagName} evaluated as {Enabled}", flagName, flag.IsEnabled);
            return Task.FromResult(flag.IsEnabled);
        }
    }

    /// <summary>
    /// Compute deterministic rollout value based on user ID and flag name
    /// Uses SHA256 hash to ensure consistent results for same user + flag combination
    /// </summary>
    /// <param name="userId">User ID for deterministic hash</param>
    /// <param name="flagName">Flag name for hash input</param>
    /// <returns>Value between 0-99 (inclusive)</returns>
    private static int ComputeDeterministicRollout(int userId, string flagName)
    {
        // Combine userId and flagName for hash input
        var input = $"{userId}:{flagName}";
        var inputBytes = Encoding.UTF8.GetBytes(input);

        // Compute SHA256 hash
        var hashBytes = SHA256.HashData(inputBytes);

        // Use first 4 bytes as integer
        var hashInt = BitConverter.ToInt32(hashBytes, 0);

        // Convert to absolute value and mod 100 to get 0-99 range
        return Math.Abs(hashInt) % 100;
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

    /// <summary>
    /// Get hash of all registered feature flags for version check integration
    /// Used by launcher to detect if flags have changed and need sync
    /// </summary>
    /// <returns>SHA256 hash of all flag names and their properties</returns>
    public Task<string> GetFlagHashAsync()
    {
        lock (_flags)
        {
            if (_flags.Count == 0)
            {
                return Task.FromResult(string.Empty);
            }

            // Sort flags by name for consistent hashing
            var sortedFlags = _flags.Values.OrderBy(f => f.Name).ToList();

            // Build hash input from all flag properties
            var hashInput = new StringBuilder();
            foreach (var flag in sortedFlags)
            {
                hashInput.Append($"{flag.Name}|{flag.IsEnabled}|{flag.Environment}|{flag.RolloutPercentage}|{flag.AppVersion}|");
            }

            // Compute SHA256 hash
            var inputBytes = Encoding.UTF8.GetBytes(hashInput.ToString());
            var hashBytes = SHA256.HashData(inputBytes);

            // Convert to hex string
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            _logger.LogDebug("Feature flags hash computed: {Hash} for {Count} flags", hashString, sortedFlags.Count);

            return Task.FromResult(hashString);
        }
    }

    /// <summary>
    /// Synchronize feature flags from server (called by launcher on version mismatch)
    /// This method would typically fetch flags from database and update local cache
    /// </summary>
    /// <param name="appVersion">Application version to sync flags for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of flags synchronized</returns>
    public Task<int> SyncFlagsFromServerAsync(string appVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(appVersion);

        _logger.LogInformation("Syncing feature flags from server for app version {AppVersion}", appVersion);

        // NOTE: This is a placeholder implementation
        // In a full implementation, this would:
        // 1. Query FeatureFlags database table WHERE AppVersion = appVersion
        // 2. Register/update each flag using RegisterFlag()
        // 3. Remove flags that no longer exist in the database
        // 4. Return count of synchronized flags
        //
        // Example SQL query:
        // SELECT FlagName, IsEnabled, Environment, RolloutPercentage, AppVersion
        // FROM FeatureFlags
        // WHERE AppVersion = @appVersion OR AppVersion IS NULL

        // For now, just log and return 0
        _logger.LogWarning("SyncFlagsFromServerAsync not fully implemented - database integration pending");

        return Task.FromResult(0);
    }

    private static string GetCurrentEnvironment()
    {
        // Environment variable precedence: MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → DOTNET_ENVIRONMENT → build config
        var env = Environment.GetEnvironmentVariable("MTM_ENVIRONMENT");

        if (string.IsNullOrEmpty(env))
        {
            env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        }

        if (string.IsNullOrEmpty(env))
        {
            env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        if (!string.IsNullOrEmpty(env))
        {
            return env;
        }

        // Default to Development in debug builds, Production in release builds
#if DEBUG
        return "Development";
#else
        return "Production";
#endif
    }
}
