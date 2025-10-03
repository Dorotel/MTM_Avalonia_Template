using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Monitor OS dark mode changes and auto-switch when Theme=Auto
/// </summary>
public class OSDarkModeMonitor : IDisposable
{
    private readonly Timer? _pollingTimer;
    private bool _lastKnownDarkMode;
    private bool _disposed;

    public event EventHandler<DarkModeChangedEventArgs>? OnDarkModeChanged;

    public OSDarkModeMonitor(TimeSpan? pollingInterval = null)
    {
        _lastKnownDarkMode = IsOSDarkMode();

        // Poll for changes (OS doesn't always provide real-time notifications)
        var interval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _pollingTimer = new Timer(CheckForChanges, null, interval, interval);
    }

    public bool IsOSDarkMode()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsWindowsDarkMode();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return IsMacOSDarkMode();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return IsLinuxDarkMode();
        }

        return false;
    }

    private void CheckForChanges(object? state)
    {
        if (_disposed)
        {
            return;
        }

        var currentDarkMode = IsOSDarkMode();
        if (currentDarkMode != _lastKnownDarkMode)
        {
            _lastKnownDarkMode = currentDarkMode;
            OnDarkModeChanged?.Invoke(this, new DarkModeChangedEventArgs
            {
                IsDarkMode = currentDarkMode,
                ChangedAt = DateTimeOffset.UtcNow
            });
        }
    }

    [SupportedOSPlatform("windows")]
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Method is only called when running on Windows platform")]
    private bool IsWindowsDarkMode()
    {
#pragma warning disable CA1416 // Validate platform compatibility
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            var value = key?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return false;
        }
#pragma warning restore CA1416
    }

    private bool IsMacOSDarkMode()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "defaults",
                    Arguments = "read -g AppleInterfaceStyle",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Trim().Equals("Dark", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private bool IsLinuxDarkMode()
    {
        // Linux dark mode detection varies by desktop environment
        // This is a simplified check for GNOME
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "gsettings",
                    Arguments = "get org.gnome.desktop.interface gtk-theme",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("dark", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pollingTimer?.Dispose();
    }
}

public class DarkModeChangedEventArgs : EventArgs
{
    public bool IsDarkMode { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
