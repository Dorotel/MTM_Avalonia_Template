using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MTM_Template_Application.Converters;

/// <summary>
/// Converts memory usage (in MB) to a color brush for visual feedback.
/// Green: &lt;70MB (good performance)
/// Yellow: 70-90MB (moderate performance)
/// Red: &gt;90MB (high memory usage)
/// </summary>
public class MemoryUsageToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(76, 175, 80));   // #4CAF50
    private static readonly SolidColorBrush YellowBrush = new(Color.FromRgb(255, 235, 59)); // #FFEB3B
    private static readonly SolidColorBrush RedBrush = new(Color.FromRgb(244, 67, 54));     // #F44336
    private static readonly SolidColorBrush GrayBrush = new(Color.FromRgb(158, 158, 158));  // #9E9E9E (fallback)

    /// <summary>
    /// Threshold for yellow warning (MB)
    /// </summary>
    public const double YellowThreshold = 70.0;

    /// <summary>
    /// Threshold for red critical (MB)
    /// </summary>
    public const double RedThreshold = 90.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double memoryUsageMB)
            return GrayBrush; // Fallback for invalid input

        return memoryUsageMB switch
        {
            < YellowThreshold => GreenBrush,           // Good: <70MB
            >= YellowThreshold and < RedThreshold => YellowBrush, // Moderate: 70-90MB
            >= RedThreshold => RedBrush,               // Critical: >90MB
            _ => GrayBrush                             // Fallback
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("MemoryUsageToColorConverter only supports one-way binding.");
    }
}
