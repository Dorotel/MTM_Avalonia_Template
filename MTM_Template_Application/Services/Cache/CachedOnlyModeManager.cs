using System;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Services.DataLayer;

namespace MTM_Template_Application.Services.Cache;

/// <summary>
/// Detect Visual unavailability, enable cached-only mode, reconnection detection
/// </summary>
public class CachedOnlyModeManager : IDisposable
{
    private readonly IVisualApiClient _visualApiClient;
    private readonly Timer _reconnectionTimer;
    private readonly TimeSpan _reconnectionCheckInterval;
    private bool _isCachedOnlyMode;
    private bool _disposed;

    public event EventHandler<CachedOnlyModeChangedEventArgs>? ModeChanged;

    public CachedOnlyModeManager(
        IVisualApiClient visualApiClient,
        TimeSpan? reconnectionCheckInterval = null)
    {
        ArgumentNullException.ThrowIfNull(visualApiClient);

        _visualApiClient = visualApiClient;
        _reconnectionCheckInterval = reconnectionCheckInterval ?? TimeSpan.FromSeconds(30);
        _isCachedOnlyMode = false;

        _reconnectionTimer = new Timer(OnReconnectionCheck, null, _reconnectionCheckInterval, _reconnectionCheckInterval);
    }

    /// <summary>
    /// Check if currently in cached-only mode
    /// </summary>
    public bool IsCachedOnlyMode => _isCachedOnlyMode;

    /// <summary>
    /// Enable cached-only mode
    /// </summary>
    public void EnableCachedOnlyMode(string reason)
    {
        if (_isCachedOnlyMode)
        {
            return; // Already in cached-only mode
        }

        _isCachedOnlyMode = true;
        OnModeChanged(true, reason);
    }

    /// <summary>
    /// Disable cached-only mode (reconnected)
    /// </summary>
    public void DisableCachedOnlyMode()
    {
        if (!_isCachedOnlyMode)
        {
            return; // Already in online mode
        }

        _isCachedOnlyMode = false;
        OnModeChanged(false, "Visual server reconnected");
    }

    /// <summary>
    /// Check Visual server availability and update mode
    /// </summary>
    public async Task<bool> CheckServerAvailabilityAsync()
    {
        try
        {
            var isAvailable = await _visualApiClient.IsServerAvailable();
            
            if (!isAvailable && !_isCachedOnlyMode)
            {
                EnableCachedOnlyMode("Visual server unavailable");
            }
            else if (isAvailable && _isCachedOnlyMode)
            {
                DisableCachedOnlyMode();
            }

            return isAvailable;
        }
        catch (Exception ex)
        {
            if (!_isCachedOnlyMode)
            {
                EnableCachedOnlyMode($"Error checking Visual server: {ex.Message}");
            }
            return false;
        }
    }

    /// <summary>
    /// Get feature limitations in cached-only mode
    /// </summary>
    public string[] GetFeatureLimitations()
    {
        return new[]
        {
            "Cannot create new orders",
            "Cannot update inventory",
            "Cannot sync with Visual ERP",
            "Cannot access real-time data",
            "Limited to cached data only"
        };
    }

    private async void OnReconnectionCheck(object? state)
    {
        if (_disposed)
        {
            return;
        }

        await CheckServerAvailabilityAsync();
    }

    private void OnModeChanged(bool isCachedOnly, string reason)
    {
        ModeChanged?.Invoke(this, new CachedOnlyModeChangedEventArgs
        {
            IsCachedOnlyMode = isCachedOnly,
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _reconnectionTimer?.Dispose();
    }
}

/// <summary>
/// Event args for cached-only mode changes
/// </summary>
public class CachedOnlyModeChangedEventArgs : EventArgs
{
    public bool IsCachedOnlyMode { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
