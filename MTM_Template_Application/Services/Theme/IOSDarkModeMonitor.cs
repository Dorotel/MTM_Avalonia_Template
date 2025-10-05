using System;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Interface for monitoring OS dark mode changes
/// </summary>
public interface IOSDarkModeMonitor : IDisposable
{
    /// <summary>
    /// Event raised when OS dark mode changes
    /// </summary>
    event EventHandler<DarkModeChangedEventArgs>? OnDarkModeChanged;

    /// <summary>
    /// Check if OS is currently in dark mode
    /// </summary>
    /// <returns>True if dark mode is enabled</returns>
    bool IsOSDarkMode();
}
