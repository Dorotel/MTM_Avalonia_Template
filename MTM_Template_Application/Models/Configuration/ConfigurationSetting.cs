namespace MTM_Template_Application.Models.Configuration;

/// <summary>
/// Individual configuration setting with source and precedence information
/// </summary>
public class ConfigurationSetting
{
    /// <summary>
    /// Configuration key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Configuration value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Source of the setting: Environment, User, Application, Default
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Precedence level (higher number = higher priority)
    /// </summary>
    public int Precedence { get; set; }

    /// <summary>
    /// Whether this setting contains encrypted data
    /// </summary>
    public bool IsEncrypted { get; set; }
}
