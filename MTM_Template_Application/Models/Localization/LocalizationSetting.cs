using System.Collections.Generic;

namespace MTM_Template_Application.Models.Localization;

/// <summary>
/// Localization settings for culture/language
/// </summary>
public class LocalizationSetting
{
    /// <summary>
    /// Culture code (e.g., "en-US", "es-MX")
    /// </summary>
    public string Culture { get; set; } = string.Empty;

    /// <summary>
    /// Whether this culture is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fallback culture if translation is missing
    /// </summary>
    public string FallbackCulture { get; set; } = string.Empty;

    /// <summary>
    /// List of supported languages
    /// </summary>
    public List<string> SupportedLanguages { get; set; } = new();

    /// <summary>
    /// Date format pattern
    /// </summary>
    public string DateFormat { get; set; } = string.Empty;

    /// <summary>
    /// Number format pattern
    /// </summary>
    public string NumberFormat { get; set; } = string.Empty;
}
