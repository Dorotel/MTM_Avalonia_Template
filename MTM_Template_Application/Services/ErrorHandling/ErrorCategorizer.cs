using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.ErrorHandling;

/// <summary>
/// Categorizes errors based on exception type and characteristics
/// </summary>
public class ErrorCategorizer
{
    /// <summary>
    /// Categorize an exception and determine its severity
    /// </summary>
    /// <returns>Tuple of (Category, Severity)</returns>
    public (string Category, string Severity) Categorize(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Network-related exceptions
        if (exception is HttpRequestException or SocketException or TaskCanceledException)
        {
            return ("Transient", "Medium");
        }

        // Configuration-related exceptions
        if (exception is InvalidOperationException &&
            (exception.Message.Contains("configuration", StringComparison.OrdinalIgnoreCase) ||
             exception.Message.Contains("setting", StringComparison.OrdinalIgnoreCase)))
        {
            return ("Configuration", "High");
        }

        // Permission-related exceptions
        if (exception is UnauthorizedAccessException or SecurityException)
        {
            return ("Permission", "High");
        }

        // Storage-related exceptions
        if (exception is IOException or DriveNotFoundException)
        {
            return ("Storage", "High");
        }

        // Argument/validation exceptions
        if (exception is ArgumentException or ArgumentNullException or ArgumentOutOfRangeException)
        {
            return ("Validation", "Low");
        }

        // NotImplementedException
        if (exception is NotImplementedException or NotSupportedException)
        {
            return ("Implementation", "High");
        }

        // Null reference exceptions
        if (exception is NullReferenceException)
        {
            return ("Programming", "Critical");
        }

        // OutOfMemoryException
        if (exception is OutOfMemoryException)
        {
            return ("Resource", "Critical");
        }

        // Aggregate exceptions (unwrap and categorize the inner exception)
        if (exception is AggregateException aggregateException &&
            aggregateException.InnerExceptions.Count > 0)
        {
            return Categorize(aggregateException.InnerExceptions[0]);
        }

        // Default: permanent error with high severity
        return ("Permanent", "High");
    }

    /// <summary>
    /// Determine if an error is transient (can be retried)
    /// </summary>
    public bool IsTransient(Exception exception)
    {
        var (category, _) = Categorize(exception);
        return category == "Transient";
    }

    /// <summary>
    /// Determine if an error is critical (requires immediate attention)
    /// </summary>
    public bool IsCritical(Exception exception)
    {
        var (_, severity) = Categorize(exception);
        return severity == "Critical";
    }
}
