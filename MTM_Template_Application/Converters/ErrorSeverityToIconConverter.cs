using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Converters;

/// <summary>
/// Converts ErrorSeverity enum to a user-friendly icon string.
/// Critical: 🔴 (red circle)
/// Error: 🔴 (red circle)
/// Warning: 🟡 (yellow circle)
/// Info: ℹ️ (information symbol)
/// </summary>
public class ErrorSeverityToIconConverter : IValueConverter
{
    /// <summary>
    /// Critical/Error icon (red circle emoji)
    /// </summary>
    public const string CriticalIcon = "🔴";

    /// <summary>
    /// Warning icon (yellow circle emoji)
    /// </summary>
    public const string WarningIcon = "🟡";

    /// <summary>
    /// Info icon (information emoji)
    /// </summary>
    public const string InfoIcon = "ℹ️";

    /// <summary>
    /// Fallback icon for unknown severity
    /// </summary>
    public const string UnknownIcon = "❔";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ErrorSeverity severity)
            return UnknownIcon;

        return severity switch
        {
            ErrorSeverity.Critical => CriticalIcon,
            ErrorSeverity.Error => CriticalIcon,
            ErrorSeverity.Warning => WarningIcon,
            ErrorSeverity.Info => InfoIcon,
            _ => UnknownIcon
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ErrorSeverityToIconConverter only supports one-way binding.");
    }
}
