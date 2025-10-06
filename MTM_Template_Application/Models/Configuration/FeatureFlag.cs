using System;

namespace MTM_Template_Application.Models.Configuration;

/// <summary>
/// Feature flag for controlling feature availability
/// </summary>
public class FeatureFlag
{
    /// <summary>
    /// Feature flag name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the feature is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Environment where this flag applies
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Percentage of users who see this feature (0-100)
    /// </summary>
    public int RolloutPercentage { get; set; }

    /// <summary>
    /// When this flag was last evaluated
    /// </summary>
    public DateTimeOffset EvaluatedAt { get; set; }

    /// <summary>
    /// Hash value for deterministic user-specific rollout
    /// Computed from userId + flagName for consistent results across sessions
    /// </summary>
    public string? TargetUserIdHash { get; set; }

    /// <summary>
    /// Application version this flag is tied to (for launcher sync)
    /// Example: "1.0.0", "1.2.3-beta"
    /// </summary>
    public string? AppVersion { get; set; }
}
