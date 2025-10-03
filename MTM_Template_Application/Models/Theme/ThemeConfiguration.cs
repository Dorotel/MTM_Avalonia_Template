using System;

namespace MTM_Template_Application.Models.Theme;

/// <summary>
/// Theme configuration settings
/// </summary>
public class ThemeConfiguration
{
    /// <summary>
    /// Theme mode: Light, Dark, Auto
    /// </summary>
    public string ThemeMode { get; set; } = string.Empty;

    /// <summary>
    /// Whether dark mode is currently active
    /// </summary>
    public bool IsDarkMode { get; set; }

    /// <summary>
    /// Accent color (hex format, e.g., "#FF6B35")
    /// </summary>
    public string AccentColor { get; set; } = string.Empty;

    /// <summary>
    /// Font size multiplier
    /// </summary>
    public double FontSize { get; set; } = 1.0;

    /// <summary>
    /// Whether high contrast mode is enabled
    /// </summary>
    public bool HighContrast { get; set; }

    /// <summary>
    /// When theme was last changed
    /// </summary>
    public DateTimeOffset LastChangedUtc { get; set; }
}
