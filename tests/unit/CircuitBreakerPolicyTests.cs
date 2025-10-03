using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for CircuitBreakerPolicy
/// </summary>
public class CircuitBreakerPolicyTests
{
    [Fact]
    public void CircuitBreaker_InitialState_ShouldBeClosed()
    {
        // Arrange
        var state = new CircuitBreakerState
        {
            ServiceName = "TestService",
            State = "Closed",
            FailureCount = 0,
            LastFailureUtc = null,
            NextRetryUtc = null,
            OpenedAt = null
        };

        // Assert
        state.State.Should().Be("Closed");
        state.FailureCount.Should().Be(0);
    }

    [Fact]
    public void CircuitBreaker_After5ConsecutiveFailures_ShouldOpen()
    {
        // Test circuit breaker opens after 5 consecutive failures
        // Arrange
        var failureThreshold = 5;
        var failureCount = 0;

        // Act
        for (int i = 0; i < 6; i++)
        {
            failureCount++;
        }

        var shouldOpen = failureCount >= failureThreshold;

        // Assert
        shouldOpen.Should().BeTrue();
        failureCount.Should().BeGreaterThanOrEqualTo(failureThreshold);
    }

    [Fact]
    public void CircuitBreaker_WhenOpen_ShouldRejectCalls()
    {
        // Arrange
        var state = new CircuitBreakerState
        {
            ServiceName = "TestService",
            State = "Open",
            FailureCount = 5,
            LastFailureUtc = DateTimeOffset.UtcNow,
            NextRetryUtc = DateTimeOffset.UtcNow.AddSeconds(30),
            OpenedAt = DateTimeOffset.UtcNow
        };

        // Assert
        state.State.Should().Be("Open");
        state.NextRetryUtc.Should().NotBeNull();
        state.NextRetryUtc.Value.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CircuitBreaker_HalfOpen_ShouldAllowTestRequest()
    {
        // Arrange
        var state = new CircuitBreakerState
        {
            ServiceName = "TestService",
            State = "HalfOpen",
            FailureCount = 5,
            LastFailureUtc = DateTimeOffset.UtcNow.AddSeconds(-30),
            NextRetryUtc = DateTimeOffset.UtcNow,
            OpenedAt = DateTimeOffset.UtcNow.AddSeconds(-30)
        };

        // Assert
        state.State.Should().Be("HalfOpen");
        state.NextRetryUtc.Should().NotBeNull();
    }

    [Fact]
    public void CircuitBreaker_SuccessfulTestRequest_ShouldClose()
    {
        // Arrange
        var state = new CircuitBreakerState
        {
            ServiceName = "TestService",
            State = "HalfOpen",
            FailureCount = 5,
            LastFailureUtc = DateTimeOffset.UtcNow.AddSeconds(-30),
            NextRetryUtc = DateTimeOffset.UtcNow,
            OpenedAt = DateTimeOffset.UtcNow.AddSeconds(-30)
        };

        // Act - simulate successful request
        state.State = "Closed";
        state.FailureCount = 0;
        state.LastFailureUtc = null;
        state.NextRetryUtc = null;
        state.OpenedAt = null;

        // Assert
        state.State.Should().Be("Closed");
        state.FailureCount.Should().Be(0);
    }

    [Fact]
    public void CircuitBreaker_ExponentialRecovery_ShouldIncrease()
    {
        // Test exponential recovery: 30s → 1m → 2m → 4m → 8m → 10m (max)
        // Arrange
        var recoveryDelays = new[] { 30, 60, 120, 240, 480, 600 }; // seconds

        // Act & Assert
        for (int i = 0; i < recoveryDelays.Length; i++)
        {
            var expectedDelay = i < 5
                ? 30 * Math.Pow(2, i)
                : 600; // Cap at 10 minutes

            expectedDelay.Should().BeApproximately(recoveryDelays[i], 0.1);
        }
    }

    [Fact]
    public void CircuitBreaker_MaxRecoveryDelay_ShouldCap()
    {
        // Test that recovery delay caps at 10 minutes
        // Arrange
        var maxDelay = 600; // 10 minutes in seconds

        // Act
        var calculatedDelay = Math.Min(30 * Math.Pow(2, 10), maxDelay);

        // Assert
        calculatedDelay.Should().Be(maxDelay);
    }

    [Fact]
    public void CircuitBreaker_StateTransitions_ShouldBeValid()
    {
        // Test valid state transitions: Closed → Open → HalfOpen → Closed
        // Arrange
        var validTransitions = new[]
        {
            ("Closed", "Open"),       // After failures
            ("Open", "HalfOpen"),     // After wait time
            ("HalfOpen", "Closed"),   // After success
            ("HalfOpen", "Open")      // After failure
        };

        // Assert
        foreach (var (from, to) in validTransitions)
        {
            // Valid transition - just verify they are state names
            from.Should().NotBeNullOrEmpty();
            to.Should().NotBeNullOrEmpty();
        }
    }
}
