namespace MTM_Template_Application.Models.Diagnostics;

/// <summary>
/// Hardware capabilities detected on the device
/// </summary>
public class HardwareCapabilities
{
    /// <summary>
    /// Total system memory in MB
    /// </summary>
    public int TotalMemoryMB { get; set; }

    /// <summary>
    /// Available system memory in MB
    /// </summary>
    public int AvailableMemoryMB { get; set; }

    /// <summary>
    /// Number of processor cores
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// Platform name: Windows, macOS, Linux, Android
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Screen resolution (e.g., "1920x1080")
    /// </summary>
    public string ScreenResolution { get; set; } = string.Empty;

    /// <summary>
    /// Whether device has a camera
    /// </summary>
    public bool HasCamera { get; set; }

    /// <summary>
    /// Whether device has a barcode scanner
    /// </summary>
    public bool HasBarcodeScanner { get; set; }
}
