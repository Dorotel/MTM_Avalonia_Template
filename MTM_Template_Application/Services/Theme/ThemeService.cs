using System;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Theme;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Theme service - Theme switching (Light/Dark/Auto), OS dark mode detection
/// </summary>
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    private ThemeConfiguration _currentTheme;
    private readonly IOSDarkModeMonitor _darkModeMonitor;

    public event EventHandler<ThemeChangedEventArgs>? OnThemeChanged;

    public ThemeService(
        ILogger<ThemeService> logger,
        IOSDarkModeMonitor darkModeMonitor)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(darkModeMonitor);

        _logger = logger;

        _logger = logger;
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

        _logger.LogInformation("ThemeService initialized. Mode: {Mode}, IsDark: {IsDark}",
            _currentTheme.ThemeMode, _currentTheme.IsDarkMode);

        // Subscribe to OS dark mode changes
        _darkModeMonitor.OnDarkModeChanged += OnOSDarkModeChanged;
    }

    public void SetTheme(string themeMode)
    {
        ArgumentNullException.ThrowIfNull(themeMode);

        if (!IsValidThemeMode(themeMode))
        {
            _logger.LogError("Invalid theme mode requested: {ThemeMode}", themeMode);
            throw new ArgumentException($"Invalid theme mode: {themeMode}. Valid values are: Light, Dark, Auto");
        }

        _logger.LogInformation("Setting theme mode to: {ThemeMode}", themeMode);

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

        _logger.LogInformation("Theme changed from {OldMode} to {NewMode}, IsDark: {IsDark}",
            oldTheme.ThemeMode, _currentTheme.ThemeMode, _currentTheme.IsDarkMode);

        OnThemeChanged?.Invoke(this, new ThemeChangedEventArgs
        {
            OldTheme = oldTheme.ThemeMode,
            NewTheme = _currentTheme.ThemeMode,
            IsDarkMode = _currentTheme.IsDarkMode
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
        _logger.LogInformation("OS dark mode changed to: {IsDark}", e.IsDarkMode);

        // Only update if theme is in Auto mode
        if (_currentTheme.ThemeMode == "Auto")
        {
            _logger.LogDebug("Updating theme due to OS change (Auto mode active)");

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
                OldTheme = oldTheme.ThemeMode,
                NewTheme = _currentTheme.ThemeMode,
                IsDarkMode = _currentTheme.IsDarkMode
            });
        }
        else
        {
            _logger.LogDebug("OS dark mode changed but theme is in {Mode} mode - no action taken",
                _currentTheme.ThemeMode);
        }
    }
}
