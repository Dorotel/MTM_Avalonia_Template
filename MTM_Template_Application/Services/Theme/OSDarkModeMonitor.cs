using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Monitor OS dark mode changes and auto-switch when Theme=Auto
/// </summary>
public class OSDarkModeMonitor : IDisposable
{
    private readonly Thread? _pollingThread;
    private readonly CancellationTokenSource _cts;
    private readonly TimeSpan _pollingInterval;
    private bool _lastKnownDarkMode;
    private bool _disposed;

    public event EventHandler<DarkModeChangedEventArgs>? OnDarkModeChanged;

    public OSDarkModeMonitor(TimeSpan? pollingInterval = null)
    {
        _lastKnownDarkMode = IsOSDarkMode();
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _cts = new CancellationTokenSource();

        // Create explicit thread for dark mode monitoring
        _pollingThread = new Thread(MonitorDarkModeLoop)
        {
            Name = "OSDarkModeMonitor.MonitorDarkModeLoop",
            IsBackground = true
        };
        _pollingThread.Start();
    }

    private void MonitorDarkModeLoop()
    {

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                CheckForChanges();
                Task.Delay(_pollingInterval, _cts.Token).Wait();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
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

    private void CheckForChanges()
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
        _cts.Cancel();

        if (_pollingThread != null && _pollingThread.IsAlive)
        {
            if (!_pollingThread.Join(TimeSpan.FromSeconds(2)))
            {
                // Thread didn't stop gracefully
            }
        }

        _cts.Dispose();
    }
}

public class DarkModeChangedEventArgs : EventArgs
{
    public bool IsDarkMode { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
