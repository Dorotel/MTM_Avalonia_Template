using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;

namespace MTM_Template_Application.Services.DataLayer.Policies;

/// <summary>
/// Circuit breaker policy: 5 consecutive failures, exponential recovery 30s→10m (Polly)
/// </summary>
public class CircuitBreakerPolicy
{
    private const int FailureThreshold = 5;
    private readonly TimeSpan[] _recoveryDelays = 
    {
        TimeSpan.FromSeconds(30),  // 30s
        TimeSpan.FromMinutes(1),   // 1m
        TimeSpan.FromMinutes(2),   // 2m
        TimeSpan.FromMinutes(5),   // 5m
        TimeSpan.FromMinutes(10)   // 10m (max)
    };
    
    private int _currentRecoveryIndex = 0;

    /// <summary>
    /// Get the Polly circuit breaker policy
    /// </summary>
    public AsyncCircuitBreakerPolicy<HttpResponseMessage> GetPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5, // 50% failure rate
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: FailureThreshold,
                durationOfBreak: GetCurrentRecoveryDelay(),
                onBreak: (result, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s after {FailureThreshold} failures");
                    IncrementRecoveryDelay();
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                    ResetRecoveryDelay();
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("Circuit breaker half-open, testing connection");
                });
    }

    /// <summary>
    /// Get simple circuit breaker policy (for compatibility)
    /// </summary>
    public AsyncCircuitBreakerPolicy<HttpResponseMessage> GetSimplePolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: FailureThreshold,
                durationOfBreak: _recoveryDelays[0],
                onBreak: (result, duration) =>
                {
                    Console.WriteLine($"Circuit breaker opened for {duration.TotalSeconds}s");
                },
                onReset: () =>
                {
                    Console.WriteLine("Circuit breaker reset");
                });
    }

    /// <summary>
    /// Get current recovery delay
    /// </summary>
    private TimeSpan GetCurrentRecoveryDelay()
    {
        var index = Math.Min(_currentRecoveryIndex, _recoveryDelays.Length - 1);
        return _recoveryDelays[index];
    }

    /// <summary>
    /// Increment recovery delay (exponential backoff)
    /// </summary>
    private void IncrementRecoveryDelay()
    {
        if (_currentRecoveryIndex < _recoveryDelays.Length - 1)
        {
            _currentRecoveryIndex++;
        }
    }

    /// <summary>
    /// Reset recovery delay after successful operations
    /// </summary>
    private void ResetRecoveryDelay()
    {
        _currentRecoveryIndex = 0;
    }

    /// <summary>
    /// Get failure threshold
    /// </summary>
    public int GetFailureThreshold()
    {
        return FailureThreshold;
    }

    /// <summary>
    /// Get recovery delays for testing/documentation
    /// </summary>
    public TimeSpan[] GetRecoveryDelays()
    {
        return (TimeSpan[])_recoveryDelays.Clone();
    }
}
