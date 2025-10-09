using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MTM_Template_Application.Converters;

/// <summary>
/// Converts boot stage duration (in milliseconds) to a color brush based on performance targets.
/// Green: Meets performance target
/// Red: Exceeds performance target (slow)
/// Gray: No data available
///
/// Performance Targets (from spec):
/// - Stage 0 (Splash): &lt;1000ms
/// - Stage 1 (Services): &lt;3000ms
/// - Stage 2 (Shell): &lt;1000ms
/// </summary>
public class BootStageToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(76, 175, 80));   // #4CAF50 (meets target)
    private static readonly SolidColorBrush RedBrush = new(Color.FromRgb(244, 67, 54));     // #F44336 (exceeds target)
    private static readonly SolidColorBrush GrayBrush = new(Color.FromRgb(158, 158, 158));  // #9E9E9E (no data)

    /// <summary>
    /// Stage 0 performance target (milliseconds)
    /// </summary>
    public const double Stage0TargetMs = 1000.0;

    /// <summary>
    /// Stage 1 performance target (milliseconds)
    /// </summary>
    public const double Stage1TargetMs = 3000.0;

    /// <summary>
    /// Stage 2 performance target (milliseconds)
    /// </summary>
    public const double Stage2TargetMs = 1000.0;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Value: boot stage duration in milliseconds (double)
        // Parameter: stage identifier ("Stage0", "Stage1", "Stage2")

        if (value is not double durationMs)
            return GrayBrush; // No data

        if (parameter is not string stageId)
            return GrayBrush; // Invalid parameter

        var targetMs = stageId switch
        {
            "Stage0" => Stage0TargetMs,
            "Stage1" => Stage1TargetMs,
            "Stage2" => Stage2TargetMs,
            _ => double.MaxValue // Unknown stage, always green
        };

        return durationMs <= targetMs ? GreenBrush : RedBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("BootStageToColorConverter only supports one-way binding.");
    }
}
