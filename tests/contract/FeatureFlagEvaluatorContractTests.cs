using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for FeatureFlagEvaluator based on feature-flag-evaluator-contract.json
/// These tests validate deterministic rollout behavior and environment filtering.
/// These tests MUST FAIL until the implementation is complete.
/// </summary>
public class FeatureFlagEvaluatorContractTests
{
    private readonly ILogger<FeatureFlagEvaluator> _logger;

    public FeatureFlagEvaluatorContractTests()
    {
        _logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
    }

    #region RegisterFlag Validation Tests

    [Fact]
    [Trait("Category", "Contract")]
    public void RegisterFlag_WithValidFlag_AddsToRegistry()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.Feature",
            IsEnabled = true,
            RolloutPercentage = 100,
            Environment = ""
        };

        // Act
        evaluator.RegisterFlag(flag);

        // Assert
        var result = evaluator.IsEnabledAsync("Test.Feature").Result;
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void RegisterFlag_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Invalid Name With Spaces", // Invalid pattern
            IsEnabled = true,
            RolloutPercentage = 100
        };

        // Act
        Action act = () => evaluator.RegisterFlag(flag);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(150)]
    [Trait("Category", "Contract")]
    public void RegisterFlag_WithInvalidRolloutPercentage_ThrowsArgumentException(int rollout)
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.Feature",
            IsEnabled = true,
            RolloutPercentage = rollout
        };

        // Act
        Action act = () => evaluator.RegisterFlag(flag);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void RegisterFlag_WithDuplicateName_UpdatesExistingFlag()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag1 = new FeatureFlag
        {
            Name = "Test.Feature",
            IsEnabled = false,
            RolloutPercentage = 0
        };
        var flag2 = new FeatureFlag
        {
            Name = "Test.Feature",
            IsEnabled = true,
            RolloutPercentage = 100
        };

        // Act
        evaluator.RegisterFlag(flag1);
        evaluator.RegisterFlag(flag2); // Should update, not throw

        // Assert
        var result = evaluator.IsEnabledAsync("Test.Feature").Result;
        result.Should().BeTrue(); // Second registration should win
    }

    #endregion

    #region Deterministic Rollout Tests

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithSameUser_ReturnsSameResult()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.Rollout50",
            IsEnabled = true,
            RolloutPercentage = 50,
            Environment = ""
        };
        evaluator.RegisterFlag(flag);
        int userId = 42;

        // Act
        var result1 = await evaluator.IsEnabledAsync("Test.Rollout50", userId);
        var result2 = await evaluator.IsEnabledAsync("Test.Rollout50", userId);
        var result3 = await evaluator.IsEnabledAsync("Test.Rollout50", userId);

        // Assert
        result1.Should().Be(result2);
        result2.Should().Be(result3);
        // Deterministic: same user always gets same result
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithDifferentUsers_ApproximatesRolloutPercentage()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.Rollout50",
            IsEnabled = true,
            RolloutPercentage = 50,
            Environment = ""
        };
        evaluator.RegisterFlag(flag);
        var enabledCount = 0;

        // Act
        for (int userId = 1; userId <= 100; userId++)
        {
            var enabled = await evaluator.IsEnabledAsync("Test.Rollout50", userId);
            if (enabled)
                enabledCount++;
        }

        // Assert
        // Should be approximately 50% (allow ±10% variance)
        enabledCount.Should().BeInRange(40, 60, "rollout percentage should approximate 50%");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_With100PercentRollout_AlwaysReturnsTrue()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.FullRollout",
            IsEnabled = true,
            RolloutPercentage = 100,
            Environment = ""
        };
        evaluator.RegisterFlag(flag);

        // Act & Assert
        for (int userId = 1; userId <= 20; userId++)
        {
            var result = await evaluator.IsEnabledAsync("Test.FullRollout", userId);
            result.Should().BeTrue();
        }
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_With0PercentRollout_AlwaysReturnsFalse()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Test.NoRollout",
            IsEnabled = true,
            RolloutPercentage = 0,
            Environment = ""
        };
        evaluator.RegisterFlag(flag);

        // Act & Assert
        for (int userId = 1; userId <= 20; userId++)
        {
            var result = await evaluator.IsEnabledAsync("Test.NoRollout", userId);
            result.Should().BeFalse();
        }
    }

    #endregion

    #region Environment Filtering Tests

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithMatchingEnvironment_ReturnsExpectedValue()
    {
        // Arrange
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", "Development");
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Debug.ShowSqlQueries",
            IsEnabled = true,
            RolloutPercentage = 100,
            Environment = "Development"
        };
        evaluator.RegisterFlag(flag);

        // Act
        var result = await evaluator.IsEnabledAsync("Debug.ShowSqlQueries");

        // Assert
        result.Should().BeTrue();

        // Cleanup
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", null);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithNonMatchingEnvironment_ReturnsFalse()
    {
        // Arrange
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", "Development");
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Production.AdvancedFeature",
            IsEnabled = true,
            RolloutPercentage = 100,
            Environment = "Production"
        };
        evaluator.RegisterFlag(flag);

        // Act
        var result = await evaluator.IsEnabledAsync("Production.AdvancedFeature");

        // Assert
        result.Should().BeFalse(); // Development environment doesn't match Production flag

        // Cleanup
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", null);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithEmptyEnvironment_AllowsAllEnvironments()
    {
        // Arrange
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", "Development");
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "OfflineModeAllowed",
            IsEnabled = true,
            RolloutPercentage = 100,
            Environment = "" // Empty = all environments
        };
        evaluator.RegisterFlag(flag);

        // Act
        var result = await evaluator.IsEnabledAsync("OfflineModeAllowed");

        // Assert
        result.Should().BeTrue();

        // Cleanup
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", null);
    }

    #endregion

    #region Performance Tests

    [Fact]
    [Trait("Category", "Contract")]
    public void IsEnabledAsync_PerformanceTarget_LessThan5Milliseconds()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);
        var flag = new FeatureFlag
        {
            Name = "Performance.Test",
            IsEnabled = true,
            RolloutPercentage = 50,
            Environment = ""
        };
        evaluator.RegisterFlag(flag);
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var result = evaluator.IsEnabledAsync("Performance.Test", userId: i).Result;
        }
        stopwatch.Stop();

        // Assert
        var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;
        avgTime.Should().BeLessThan(5, $"Average flag evaluation was {avgTime}ms");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithNonExistentFlag_ReturnsFalse()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);

        // Act
        var result = await evaluator.IsEnabledAsync("NonExistent.Flag");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task IsEnabledAsync_WithNullFlagName_ThrowsArgumentNullException()
    {
        // Arrange
        var evaluator = new FeatureFlagEvaluator(_logger);

        // Act
        Func<Task> act = async () => await evaluator.IsEnabledAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
