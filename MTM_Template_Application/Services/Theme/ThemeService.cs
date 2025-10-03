using System;
using MTM_Template_Application.Models.Theme;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Theme service - Theme switching (Light/Dark/Auto), OS dark mode detection
/// </summary>
public class ThemeService : IThemeService
{
    private ThemeConfiguration _currentTheme;
    private readonly OSDarkModeMonitor _darkModeMonitor;

    public event EventHandler<ThemeChangedEventArgs>? OnThemeChanged;

    public ThemeService(OSDarkModeMonitor darkModeMonitor)
    {
        ArgumentNullException.ThrowIfNull(darkModeMonitor);

        _darkModeMonitor = darkModeMonitor;
        _currentTheme = new ThemeConfiguration
        {
            ThemeMode = "Auto",
            IsDarkMode = _darkModeMonitor.IsOSDarkMode(),
            AccentColor = "#0078D4",
            FontSize = 14,
            HighContrast = false,
            LastChangedUtc = DateTimeOffset.UtcNow
        };

        // Subscribe to OS dark mode changes
        _darkModeMonitor.OnDarkModeChanged += OnOSDarkModeChanged;
    }

    public void SetTheme(string themeMode)
    {
        ArgumentNullException.ThrowIfNull(themeMode);

        if (!IsValidThemeMode(themeMode))
        {
            throw new ArgumentException($"Invalid theme mode: {themeMode}. Valid values are: Light, Dark, Auto");
        }

        var oldTheme = _currentTheme;
        _currentTheme = new ThemeConfiguration
        {
            ThemeMode = themeMode,
            IsDarkMode = DetermineIsDarkMode(themeMode),
            AccentColor = oldTheme.AccentColor,
            FontSize = oldTheme.FontSize,
            HighContrast = oldTheme.HighContrast,
            LastChangedUtc = DateTimeOffset.UtcNow
        };

        OnThemeChanged?.Invoke(this, new ThemeChangedEventArgs
        {
            OldTheme = oldTheme,
            NewTheme = _currentTheme
        });
    }

    public ThemeConfiguration GetCurrentTheme()
    {
        return _currentTheme;
    }

    private bool DetermineIsDarkMode(string themeMode)
    {
        return themeMode switch
        {
            "Dark" => true,
            "Light" => false,
            "Auto" => _darkModeMonitor.IsOSDarkMode(),
            _ => false
        };
    }

    private bool IsValidThemeMode(string themeMode)
    {
        return themeMode is "Light" or "Dark" or "Auto";
    }

    private void OnOSDarkModeChanged(object? sender, DarkModeChangedEventArgs e)
    {
        // Only update if theme is in Auto mode
        if (_currentTheme.ThemeMode == "Auto")
        {
            var oldTheme = _currentTheme;
            _currentTheme = new ThemeConfiguration
            {
                ThemeMode = "Auto",
                IsDarkMode = e.IsDarkMode,
                AccentColor = oldTheme.AccentColor,
                FontSize = oldTheme.FontSize,
                HighContrast = oldTheme.HighContrast,
                LastChangedUtc = DateTimeOffset.UtcNow
            };

            OnThemeChanged?.Invoke(this, new ThemeChangedEventArgs
            {
                OldTheme = oldTheme,
                NewTheme = _currentTheme
            });
        }
    }
}
