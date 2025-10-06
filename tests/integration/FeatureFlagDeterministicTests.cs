using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// T025: Integration tests for feature flag deterministic rollout
/// Tests that same user always gets same result (hash-based evaluation)
/// Maps to Scenario 4 from quickstart.md
/// </summary>
public class FeatureFlagDeterministicTests
{
    private readonly ITestOutputHelper _output;

    public FeatureFlagDeterministicTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region T025: Feature Flag Deterministic Rollout Tests

    /// <summary>
    /// T025.1: Test same user gets consistent result across multiple evaluations
    /// Verifies deterministic behavior (FR-023)
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_SameUserGetsConsistentResult()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.Rollout50",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag);

        const int testUserId = 42;
        const int evaluationCount = 10;
        var results = new List<bool>();

        // Act - Evaluate same flag for same user 10 times
        for (int i = 0; i < evaluationCount; i++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", testUserId);
            results.Add(enabled);
        }

        // Assert - All results should be identical (deterministic)
        results.Should().HaveCount(evaluationCount);
        results.Distinct().Should().HaveCount(1, "same user should always get same result (deterministic behavior)");

        var finalResult = results.First();
        _output.WriteLine($"✓ User {testUserId} consistently got: {(finalResult ? "ENABLED" : "DISABLED")} across {evaluationCount} evaluations");
    }

    /// <summary>
    /// T025.2: Test distribution approximates rollout percentage with 100 users
    /// Verifies that hash-based rollout achieves target distribution (\u00b110% variance allowed)
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_DistributionApproximatesRolloutPercentage()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.Rollout50",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag);

        const int userCount = 100;
        const int targetRolloutPercentage = 50;
        var enabledCount = 0;

        // Act - Evaluate flag for 100 different users
        for (int userId = 1; userId <= userCount; userId++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", userId);
            if (enabled)
            {
                enabledCount++;
            }
        }

        // Assert - Distribution should approximate 50% (\u00b110% variance allowed)
        var actualPercentage = (enabledCount / (double)userCount) * 100;
        actualPercentage.Should().BeInRange(targetRolloutPercentage - 10, targetRolloutPercentage + 10,
            "distribution should approximate target rollout percentage within \u00b110%");

        _output.WriteLine($"✓ Target rollout: {targetRolloutPercentage}%");
        _output.WriteLine($"✓ Actual distribution: {actualPercentage:F1}% ({enabledCount}/{userCount} users)");
        _output.WriteLine($"✓ Variance: {Math.Abs(actualPercentage - targetRolloutPercentage):F1}% (within acceptable range)");
    }

    /// <summary>
    /// T025.3: Test 0% rollout disables feature for all users
    /// Verifies edge case: 0% rollout
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_ZeroPercentRolloutDisablesAll()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.Rollout0",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 0, // 0% rollout
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag);

        const int userCount = 50;
        var enabledCount = 0;

        // Act
        for (int userId = 1; userId <= userCount; userId++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout0", userId);
            if (enabled)
            {
                enabledCount++;
            }
        }

        // Assert - No users should have feature enabled
        enabledCount.Should().Be(0, "0% rollout should disable feature for all users");
        _output.WriteLine($"✓ 0% rollout correctly disabled feature for all {userCount} users");
    }

    /// <summary>
    /// T025.4: Test 100% rollout enables feature for all users
    /// Verifies edge case: 100% rollout
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_HundredPercentRolloutEnablesAll()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.Rollout100",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 100, // 100% rollout
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag);

        const int userCount = 50;
        var enabledCount = 0;

        // Act
        for (int userId = 1; userId <= userCount; userId++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout100", userId);
            if (enabled)
            {
                enabledCount++;
            }
        }

        // Assert - All users should have feature enabled
        enabledCount.Should().Be(userCount, "100% rollout should enable feature for all users");
        _output.WriteLine($"✓ 100% rollout correctly enabled feature for all {userCount} users");
    }

    /// <summary>
    /// T025.5: Test deterministic behavior persists across application restart
    /// Simulates restart by creating new evaluator instance
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_BehaviorPersistsAcrossRestart()
    {
        // Arrange - First evaluator instance
        var logger1 = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator1 = new FeatureFlagEvaluator(logger1);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.Rollout50",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator1.RegisterFlag(flag);

        const int testUserId = 123;

        // Act - First evaluation (before "restart")
        var result1 = await flagEvaluator1.IsEnabledAsync("TestFeature.Rollout50", testUserId);

        // Simulate restart by creating new evaluator instance
        var logger2 = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator2 = new FeatureFlagEvaluator(logger2);
        flagEvaluator2.RegisterFlag(flag);

        // Act - Second evaluation (after "restart")
        var result2 = await flagEvaluator2.IsEnabledAsync("TestFeature.Rollout50", testUserId);

        // Assert - Results should be identical
        result2.Should().Be(result1, "same user should get same result after restart (deterministic hash-based evaluation)");
        _output.WriteLine($"✓ User {testUserId} got consistent result before and after restart: {(result1 ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// T025.6: Test different flags with same rollout percentage have different distributions
    /// Verifies that hash includes flag name (prevents all 50% flags having identical distribution)
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_DifferentFlagsHaveDifferentDistributions()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag1 = new FeatureFlag
        {
            Name = "TestFeature.Flag1",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        var flag2 = new FeatureFlag
        {
            Name = "TestFeature.Flag2",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        flagEvaluator.RegisterFlag(flag1);
        flagEvaluator.RegisterFlag(flag2);

        const int userCount = 100;
        var flag1EnabledUsers = new List<int>();
        var flag2EnabledUsers = new List<int>();

        // Act - Evaluate both flags for same set of users
        for (int userId = 1; userId <= userCount; userId++)
        {
            var flag1Enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Flag1", userId);
            var flag2Enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Flag2", userId);

            if (flag1Enabled)
            {
                flag1EnabledUsers.Add(userId);
            }
            if (flag2Enabled)
            {
                flag2EnabledUsers.Add(userId);
            }
        }

        // Assert - Distributions should be different (not 100% overlap)
        var overlapCount = flag1EnabledUsers.Intersect(flag2EnabledUsers).Count();
        var overlapPercentage = (overlapCount / (double)flag1EnabledUsers.Count) * 100;

        overlapPercentage.Should().BeLessThan(90,
            "different flags should have different distributions (hash includes flag name)");

        _output.WriteLine($"✓ Flag1 enabled for {flag1EnabledUsers.Count} users");
        _output.WriteLine($"✓ Flag2 enabled for {flag2EnabledUsers.Count} users");
        _output.WriteLine($"✓ Overlap: {overlapCount} users ({overlapPercentage:F1}%)");
        _output.WriteLine($"✓ Distributions are sufficiently different (not identical)");
    }

    /// <summary>
    /// T025.7: Test invalid rollout percentage validation
    /// Verifies that RegisterFlag rejects rollout percentage outside 0-100 range
    /// </summary>
    [Fact]
    public void T025_FeatureFlagDeterministic_InvalidRolloutPercentageRejected()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var invalidFlag = new FeatureFlag
        {
            Name = "TestFeature.InvalidRollout",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 150, // Invalid: >100
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        // Act & Assert - Should throw ArgumentException
        Action act = () => flagEvaluator.RegisterFlag(invalidFlag);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*rollout*percentage*0*100*", "should validate rollout percentage is between 0 and 100");

        _output.WriteLine("✓ Invalid rollout percentage (150) correctly rejected");
    }

    /// <summary>
    /// T025.8: Test gradual rollout scenario (10% -> 50% -> 100%)
    /// Verifies that users enabled at 10% stay enabled when rollout increases
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_GradualRolloutMaintainsConsistency()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        const int userCount = 100;

        // Phase 1: 10% rollout
        var flag10Percent = new FeatureFlag
        {
            Name = "TestFeature.GradualRollout",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 10,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag10Percent);

        var users10Percent = new List<int>();
        for (int userId = 1; userId <= userCount; userId++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.GradualRollout", userId);
            if (enabled)
            {
                users10Percent.Add(userId);
            }
        }

        // Phase 2: 50% rollout
        var flag50Percent = new FeatureFlag
        {
            Name = "TestFeature.GradualRollout",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag50Percent); // Update existing flag

        var users50Percent = new List<int>();
        for (int userId = 1; userId <= userCount; userId++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.GradualRollout", userId);
            if (enabled)
            {
                users50Percent.Add(userId);
            }
        }

        // Assert - All users from 10% should still be enabled at 50%
        var maintained10PercentUsers = users10Percent.All(u => users50Percent.Contains(u));
        maintained10PercentUsers.Should().BeTrue(
            "users enabled at 10% rollout should remain enabled when rollout increases to 50%");

        _output.WriteLine($"✓ 10% rollout: {users10Percent.Count} users enabled");
        _output.WriteLine($"✓ 50% rollout: {users50Percent.Count} users enabled");
        _output.WriteLine($"✓ All {users10Percent.Count} users from 10% maintained in 50% rollout");
    }

    /// <summary>
    /// T025.9: Test hash-based evaluation vs random evaluation
    /// Demonstrates difference between deterministic (hash-based) and non-deterministic (random) approaches
    /// </summary>
    [Fact]
    public async Task T025_FeatureFlagDeterministic_HashBasedVsRandomEvaluation()
    {
        // Arrange
        var logger = Substitute.For<ILogger<FeatureFlagEvaluator>>();
        var flagEvaluator = new FeatureFlagEvaluator(logger);

        var flag = new FeatureFlag
        {
            Name = "TestFeature.HashBased",
            IsEnabled = true,
            Environment = "",
            RolloutPercentage = 50,
            EvaluatedAt = DateTimeOffset.UtcNow
        };
        flagEvaluator.RegisterFlag(flag);

        const int testUserId = 456;
        const int evaluationCount = 100;

        // Act - Hash-based evaluation (should be consistent)
        var hashBasedResults = new List<bool>();
        for (int i = 0; i < evaluationCount; i++)
        {
            var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.HashBased", testUserId);
            hashBasedResults.Add(enabled);
        }

        // Act - Random evaluation (for comparison)
        var random = new Random(42); // Seeded for reproducibility
        var randomResults = new List<bool>();
        for (int i = 0; i < evaluationCount; i++)
        {
            var enabled = random.Next(100) < 50; // Random 50% chance
            randomResults.Add(enabled);
        }

        // Assert - Hash-based should be 100% consistent
        var hashBasedDistinctCount = hashBasedResults.Distinct().Count();
        hashBasedDistinctCount.Should().Be(1, "hash-based evaluation should always return same result");

        // Assert - Random should have variations
        var randomDistinctCount = randomResults.Distinct().Count();
        randomDistinctCount.Should().Be(2, "random evaluation should have both true and false results");

        var hashBasedResult = hashBasedResults.First();
        var randomTrueCount = randomResults.Count(r => r);

        _output.WriteLine($"✓ Hash-based: {(hashBasedResult ? "ENABLED" : "DISABLED")} in 100/100 evaluations (100% consistent)");
        _output.WriteLine($"✓ Random: ENABLED in {randomTrueCount}/100 evaluations (~50% average, but inconsistent)");
        _output.WriteLine($"✓ Hash-based approach provides deterministic user experience");
    }

    #endregion T025: Feature Flag Deterministic Rollout Tests
}
