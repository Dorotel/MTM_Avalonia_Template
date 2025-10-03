using System;
using System.Collections.Generic;

namespace MTM_Template_Application.Models.Logging;

/// <summary>
/// Structured log entry in OpenTelemetry format
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Timestamp when the log was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Log level: Debug, Information, Warning, Error, Critical
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Log message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Distributed trace ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Span ID within the trace
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Additional structured attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// Resource information (application, environment, etc.)
    /// </summary>
    public Dictionary<string, string> Resource { get; set; } = new();

    /// <summary>
    /// Scope information (logger name, version, etc.)
    /// </summary>
    public Dictionary<string, string> Scope { get; set; } = new();
}
