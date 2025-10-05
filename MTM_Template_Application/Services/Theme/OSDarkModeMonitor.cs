using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MTM_Template_Application.Services.Theme;

/// <summary>
/// Monitor OS dark mode changes and auto-switch when Theme=Auto
/// </summary>
public class OSDarkModeMonitor : IOSDarkModeMonitor
{
    private readonly ILogger<OSDarkModeMonitor> _logger;
    private readonly CancellationTokenSource _cts;
    private readonly TimeSpan _pollingInterval;
    private bool _lastKnownDarkMode;
    private bool _disposed;

    public event EventHandler<DarkModeChangedEventArgs>? OnDarkModeChanged;

    public OSDarkModeMonitor(
        ILogger<OSDarkModeMonitor> logger,
        TimeSpan? pollingInterval = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _lastKnownDarkMode = IsOSDarkMode();
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(5);
        _cts = new CancellationTokenSource();

        _logger.LogInformation("OSDarkModeMonitor initialized. Initial dark mode: {IsDark}, Polling interval: {Interval}s",
            _lastKnownDarkMode, _pollingInterval.TotalSeconds);

        // Start monitoring as a background task (non-blocking)
        _ = Task.Run(async () => await MonitorDarkModeLoopAsync().ConfigureAwait(false), _cts.Token);
    }

    private async Task MonitorDarkModeLoopAsync()
    {
        _logger.LogDebug("Dark mode monitoring loop started");

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                CheckForChanges();
                await Task.Delay(_pollingInterval, _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Dark mode monitoring loop cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in dark mode monitoring loop");
            }
        }

        _logger.LogDebug("Dark mode monitoring loop ended");
    }

    public bool IsOSDarkMode()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsWindowsDarkMode();
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
            _logger.LogInformation("OS dark mode changed from {OldMode} to {NewMode}",
                _lastKnownDarkMode ? "dark" : "light",
                currentDarkMode ? "dark" : "light");

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
            var isDark = value is int intValue && intValue == 0;
            _logger.LogDebug("Windows dark mode detected: {IsDark}", isDark);
            return isDark;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect Windows dark mode");
            return false;
        }
#pragma warning restore CA1416
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

            var isDark = output.Contains("dark", StringComparison.OrdinalIgnoreCase);
            _logger.LogDebug("Linux dark mode detected: {IsDark}", isDark);
            return isDark;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect Linux dark mode");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation("Disposing OSDarkModeMonitor");

        _disposed = true;
        _cts.Cancel();

        // CancellationToken will stop the background task

        _cts.Dispose();

        _logger.LogDebug("OSDarkModeMonitor disposed");
    }
}

public class DarkModeChangedEventArgs : EventArgs
{
    public bool IsDarkMode { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}
