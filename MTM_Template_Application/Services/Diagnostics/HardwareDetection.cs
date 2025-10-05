using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics;

/// <summary>
/// Detect hardware capabilities (memory, CPU, screen resolution, peripherals)
/// </summary>
public class HardwareDetection
{
    /// <summary>
    /// Detect all hardware capabilities
    /// </summary>
    public virtual HardwareCapabilities DetectCapabilities()
    {
        var capabilities = new HardwareCapabilities
        {
            TotalMemoryMB = (int)GetTotalMemoryMB(),
            AvailableMemoryMB = (int)GetAvailableMemoryMB(),
            ProcessorCount = Environment.ProcessorCount,
            Platform = GetPlatformName(),
            ScreenResolution = GetScreenResolution(),
            HasCamera = DetectCamera(),
            HasBarcodeScanner = DetectBarcodeScanner()
        };

        return capabilities;
    }

    /// <summary>
    /// Get total physical memory in MB
    /// </summary>
    private long GetTotalMemoryMB()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsTotalMemory();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxTotalMemory();
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get available memory in MB
    /// </summary>
    private long GetAvailableMemoryMB()
    {
        try
        {
            // Use GC to get approximate available memory
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            var totalMemory = GetTotalMemoryMB() * 1024 * 1024;
            var available = totalMemory - workingSet;

            return available / (1024 * 1024);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get platform name
    /// </summary>
    private string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "Windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "Linux";
        }
        else
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Get screen resolution
    /// </summary>
    private string GetScreenResolution()
    {
        // Platform-specific screen detection would go here
        // For Avalonia, this would need to be called from UI context
        return "Not Detected (requires UI context)";
    }

    /// <summary>
    /// Detect camera availability
    /// </summary>
    private bool DetectCamera()
    {
        // Platform-specific camera detection would go here
        // This is a placeholder for future implementation
        return false;
    }

    /// <summary>
    /// Detect barcode scanner availability
    /// </summary>
    private bool DetectBarcodeScanner()
    {
        // Platform-specific scanner detection would go here
        // This is a placeholder for future implementation
        return false;
    }

    #region Platform-Specific Memory Detection

    private long GetWindowsTotalMemory()
    {
        try
        {
            // Use GC memory info as fallback (System.Management requires additional package)
            var gcInfo = GC.GetGCMemoryInfo();
            return gcInfo.TotalAvailableMemoryBytes / (1024 * 1024);
        }
        catch
        {
            // Fallback
        }

        return 0;
    }

    private long GetLinuxTotalMemory()
    {
        try
        {
            var lines = System.IO.File.ReadAllLines("/proc/meminfo");
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                    {
                        return kb / 1024;
                    }
                }
            }
        }
        catch
        {
            // Fallback
        }

        return 0;
    }

    #endregion
}
