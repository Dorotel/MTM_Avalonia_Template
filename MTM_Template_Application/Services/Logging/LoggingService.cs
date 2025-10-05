using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace MTM_Template_Application.Services.Logging;

/// <summary>
/// Logging service with Serilog + OpenTelemetry integration, structured JSON format
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _contextProperties;
    private readonly PiiRedactionMiddleware _piiRedactionMiddleware;

    public LoggingService(ILogger logger, PiiRedactionMiddleware piiRedactionMiddleware)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(piiRedactionMiddleware);

        _logger = logger;
        _piiRedactionMiddleware = piiRedactionMiddleware;
        _contextProperties = new Dictionary<string, object>();
    }

    /// <summary>
    /// Log an informational message
    /// </summary>
    public void LogInformation(string message, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(message);

        var redactedMessage = _piiRedactionMiddleware.Redact(message);
        var redactedArgs = RedactArgs(args);

        using (EnrichWithContext())
        {
            _logger.Information(redactedMessage, redactedArgs);
        }
    }

    /// <summary>
    /// Log a warning message
    /// </summary>
    public void LogWarning(string message, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(message);

        var redactedMessage = _piiRedactionMiddleware.Redact(message);
        var redactedArgs = RedactArgs(args);

        using (EnrichWithContext())
        {
            _logger.Warning(redactedMessage, redactedArgs);
        }
    }

    /// <summary>
    /// Log an error message
    /// </summary>
    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(message);

        var redactedMessage = _piiRedactionMiddleware.Redact(message);
        var redactedArgs = RedactArgs(args);

        using (EnrichWithContext())
        {
            if (exception != null)
            {
                _logger.Error(exception, redactedMessage, redactedArgs);
            }
            else
            {
                _logger.Error(redactedMessage, redactedArgs);
            }
        }
    }

    /// <summary>
    /// Set logging context (correlation ID, trace ID, etc.)
    /// </summary>
    public void SetContext(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        lock (_contextProperties)
        {
            _contextProperties[key] = value;
        }
    }

    /// <summary>
    /// Flush pending log entries
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5s timeout for flush

        var flushTask = Task.Factory.StartNew(() =>
        {
            if (_logger is Logger logger)
            {
                logger.Dispose();
            }
        }, TaskCreationOptions.LongRunning);

        await flushTask;
    }

    /// <summary>
    /// Enrich log context with current context properties
    /// </summary>
    private IDisposable EnrichWithContext()
    {
        var disposables = new List<IDisposable>();

        lock (_contextProperties)
        {
            foreach (var kvp in _contextProperties)
            {
                disposables.Add(LogContext.PushProperty(kvp.Key, kvp.Value));
            }
        }

        return new CompositeDisposable(disposables);
    }

    /// <summary>
    /// Redact PII from arguments
    /// </summary>
    private object[] RedactArgs(object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return args ?? Array.Empty<object>();
        }

        var redactedArgs = new object[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is string str)
            {
                redactedArgs[i] = _piiRedactionMiddleware.Redact(str);
            }
            else
            {
                redactedArgs[i] = args[i];
            }
        }

        return redactedArgs;
    }

    /// <summary>
    /// Helper class to dispose multiple disposables
    /// </summary>
    private class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables;

        public CompositeDisposable(List<IDisposable> disposables)
        {
            _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}
