using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Configuration;

/// <summary>
/// Represents a complete configuration set for an environment
/// </summary>
public class ConfigurationProfile
{
    /// <summary>
    /// Profile name: Development, Staging, Production, Custom
    /// </summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this profile is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Configuration settings in this profile
    /// </summary>
    public List<ConfigurationSetting> Settings { get; set; } = new();

    /// <summary>
    /// Feature flags in this profile
    /// </summary>
    public List<FeatureFlag> FeatureFlags { get; set; } = new();

    /// <summary>
    /// When this profile was last modified
    /// </summary>
    public DateTimeOffset LastModifiedUtc { get; set; }
}
