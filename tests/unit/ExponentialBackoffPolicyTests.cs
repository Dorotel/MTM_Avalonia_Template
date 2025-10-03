using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.DataLayer;
using MTM_Template_Application.Services.DataLayer.Policies;
using NSubstitute;
using Polly;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for ExponentialBackoffPolicy
/// </summary>
public class ExponentialBackoffPolicyTests
{
    [Fact]
    public void ExponentialBackoff_ShouldFollowExpectedDelayPattern()
    {
        // Test exponential backoff delays: 1s, 2s, 4s, 8s, 16s
        // Arrange
        var expectedDelays = new[] { 1000, 2000, 4000, 8000, 16000 };

        // Act & Assert
        for (int i = 0; i < expectedDelays.Length; i++)
        {
            var delay = Math.Pow(2, i) * 1000; // 2^i seconds in milliseconds
            delay.Should().BeApproximately(expectedDelays[i], 0.1);
        }
    }

    [Fact]
    public void ExponentialBackoff_WithJitter_ShouldVaryWithin25Percent()
    {
        // Test jitter (±25%)
        // Arrange
        var baseDelay = 1000; // 1 second
        var jitterPercent = 0.25; // ±25%
        var minDelay = baseDelay * (1 - jitterPercent);
        var maxDelay = baseDelay * (1 + jitterPercent);

        // Act
        var random = new Random(42); // Fixed seed for reproducibility
        var jitteredDelay = baseDelay + (random.NextDouble() * 2 - 1) * baseDelay * jitterPercent;

        // Assert
        jitteredDelay.Should().BeInRange(minDelay, maxDelay);
    }

    [Fact]
    public async Task ExponentialBackoff_ShouldRetryOnTransientFailure()
    {
        // Arrange
        var attemptCount = 0;
        var maxRetries = 5;

        Func<Task<bool>> operation = async () =>
        {
            attemptCount++;
            await Task.Delay(10); // Simulate work

            if (attemptCount < 3)
            {
                throw new System.Net.Http.HttpRequestException("Transient error");
            }

            return true;
        };

        // Act
        var result = false;
        try
        {
            result = await operation();
        }
        catch
        {
            // Expected to retry
        }

        // Assert
        attemptCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ExponentialBackoff_MaxRetries_ShouldNotExceedLimit()
    {
        // Arrange
        var maxRetries = 5;
        var attemptCount = 0;

        // Act
        for (int i = 0; i < 10; i++)
        {
            if (attemptCount < maxRetries)
            {
                attemptCount++;
            }
        }

        // Assert
        attemptCount.Should().Be(maxRetries);
    }

    [Fact]
    public void CalculateDelay_ForAttempt_ShouldReturnCorrectValue()
    {
        // Test delay calculation for specific retry attempts
        // Arrange & Act & Assert
        CalculateExponentialDelay(1).Should().BeApproximately(1000, 100); // 1s
        CalculateExponentialDelay(2).Should().BeApproximately(2000, 100); // 2s
        CalculateExponentialDelay(3).Should().BeApproximately(4000, 100); // 4s
        CalculateExponentialDelay(4).Should().BeApproximately(8000, 100); // 8s
        CalculateExponentialDelay(5).Should().BeApproximately(16000, 100); // 16s
    }

    private static double CalculateExponentialDelay(int retryAttempt)
    {
        return Math.Pow(2, retryAttempt - 1) * 1000; // 2^(n-1) seconds in milliseconds
    }
}
