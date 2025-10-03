using System;
using MTM_Template_Application.Models.Theme;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Theme management service
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Set theme (Light, Dark, Auto)
    /// </summary>
    void SetTheme(string themeMode);

    /// <summary>
    /// Get current theme configuration
    /// </summary>
    ThemeConfiguration GetCurrentTheme();

    /// <summary>
    /// Event raised when theme changes
    /// </summary>
    event EventHandler<ThemeChangedEventArgs>? OnThemeChanged;
}

/// <summary>
/// Theme changed event arguments
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public string OldTheme { get; init; } = string.Empty;
    public string NewTheme { get; init; } = string.Empty;
    public bool IsDarkMode { get; init; }
}
