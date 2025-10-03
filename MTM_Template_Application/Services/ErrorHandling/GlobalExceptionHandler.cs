using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.ErrorHandling;

namespace MTM_Template_Application.Services.ErrorHandling;

/// <summary>
/// Global exception handler for unhandled exceptions
/// </summary>
public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ErrorCategorizer _errorCategorizer;
    private readonly DiagnosticBundleGenerator _diagnosticBundleGenerator;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ErrorCategorizer errorCategorizer,
        DiagnosticBundleGenerator diagnosticBundleGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(errorCategorizer);
        ArgumentNullException.ThrowIfNull(diagnosticBundleGenerator);

        _logger = logger;
        _errorCategorizer = errorCategorizer;
        _diagnosticBundleGenerator = diagnosticBundleGenerator;
    }

    /// <summary>
    /// Handle an unhandled exception
    /// </summary>
    public async Task<ErrorReport> HandleExceptionAsync(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var errorReport = new ErrorReport
        {
            ErrorId = Guid.NewGuid(),
            Message = exception.Message,
            StackTrace = exception.StackTrace ?? string.Empty,
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Categorize the error
        var (category, severity) = _errorCategorizer.Categorize(exception);
        errorReport.Category = category;
        errorReport.Severity = severity;

        // Generate diagnostic bundle
        errorReport.DiagnosticBundle = await _diagnosticBundleGenerator.GenerateAsync(exception, errorReport);

        // Log the error
        _logger.LogError(
            exception,
            "Unhandled exception: {ErrorId}, Category: {Category}, Severity: {Severity}",
            errorReport.ErrorId,
            errorReport.Category,
            errorReport.Severity
        );

        return errorReport;
    }

    /// <summary>
    /// Register the global exception handler with the current AppDomain
    /// </summary>
    public void RegisterGlobalHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                _ = HandleExceptionAsync(ex);
            }
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            _ = HandleExceptionAsync(args.Exception);
            args.SetObserved();
        };
    }
}
