using System;
using System.Net.Http;
using Polly;
using Polly.Retry;

namespace MTM_Template_Application.Services.DataLayer.Policies;

/// <summary>
/// Exponential backoff policy: 1s, 2s, 4s, 8s, 16s with ±25% jitter (Polly)
/// </summary>
public class ExponentialBackoffPolicy
{
    private readonly int[] _delays = { 1000, 2000, 4000, 8000, 16000 }; // milliseconds
    private readonly double _jitterFactor = 0.25; // ±25%
    private readonly Random _random = new Random();

    /// <summary>
    /// Get the Polly retry policy
    /// </summary>
    public AsyncRetryPolicy<HttpResponseMessage> GetPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: _delays.Length,
                sleepDurationProvider: retryAttempt => GetDelayWithJitter(retryAttempt - 1),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                    Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds}s delay");
                });
    }

    /// <summary>
    /// Get delay with jitter for a specific retry attempt
    /// </summary>
    private TimeSpan GetDelayWithJitter(int attemptIndex)
    {
        if (attemptIndex >= _delays.Length)
        {
            attemptIndex = _delays.Length - 1;
        }

        var baseDelay = _delays[attemptIndex];
        var jitter = baseDelay * _jitterFactor * (2 * _random.NextDouble() - 1); // ±25%
        var actualDelay = baseDelay + (int)jitter;

        return TimeSpan.FromMilliseconds(Math.Max(0, actualDelay));
    }

    /// <summary>
    /// Get the maximum retry count
    /// </summary>
    public int GetMaxRetries()
    {
        return _delays.Length;
    }

    /// <summary>
    /// Get the delays (without jitter) for testing/documentation
    /// </summary>
    public int[] GetBaseDelays()
    {
        return (int[])_delays.Clone();
    }
}
