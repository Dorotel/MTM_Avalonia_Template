using System;

namespace MTM_Template_Application.Models.DataLayer;

/// <summary>
/// State of circuit breaker for a service
/// </summary>
public class CircuitBreakerState
{
    /// <summary>
    /// Name of the service
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Circuit state: Closed, Open, HalfOpen
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Number of consecutive failures
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// When the last failure occurred
    /// </summary>
    public DateTimeOffset? LastFailureUtc { get; set; }

    /// <summary>
    /// When to attempt next retry
    /// </summary>
    public DateTimeOffset? NextRetryUtc { get; set; }

    /// <summary>
    /// When the circuit was opened
    /// </summary>
    public DateTimeOffset? OpenedAt { get; set; }
}
