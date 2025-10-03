using System;

namespace MTM_Template_Application.Models.Localization;

/// <summary>
/// Tracking for missing translation strings
/// </summary>
public class MissingTranslation
{
    /// <summary>
    /// Translation key that was missing
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Culture where translation was missing
    /// </summary>
    public string Culture { get; set; } = string.Empty;

    /// <summary>
    /// Fallback value that was used
    /// </summary>
    public string FallbackValue { get; set; } = string.Empty;

    /// <summary>
    /// When this was first reported
    /// </summary>
    public DateTimeOffset ReportedAt { get; set; }

    /// <summary>
    /// Number of times this key was requested
    /// </summary>
    public int Frequency { get; set; }
}
