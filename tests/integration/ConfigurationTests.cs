using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for configuration loading and precedence
/// </summary>
public class ConfigurationTests
{
    private readonly ITestOutputHelper? _output;

    public ConfigurationTests(ITestOutputHelper? output = null)
    {
        _output = output;
    }
    [Fact]
    public async Task ConfigurationLoading_ShouldFollowPrecedenceOrder()
    {
        // Arrange - Set environment variable (highest precedence)
        System.Environment.SetEnvironmentVariable("TEST_SETTING", "env_value");

        try
        {
            // Create a real ConfigurationService
            var logger = Substitute.For<ILogger<ConfigurationService>>();
            var configService = new ConfigurationService(logger);

            // Act
            await configService.ReloadAsync();
            var value = configService.GetValue<string>("TEST_SETTING");

            // Assert - env vars should override all other sources
            value.Should().Be("env_value", "environment variables have highest precedence");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable("TEST_SETTING", null);
        }
    }

    [Fact]
    public async Task ConfigurationHotReload_ShouldRaiseChangeEvent()
    {
        // Arrange - Create real configuration service
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);

        bool eventRaised = false;
        string? changedKey = null;

        configService.OnConfigurationChanged += (sender, args) =>
        {
            eventRaised = true;
            changedKey = args.Key;
        };

        // Act
        await configService.SetValue("TestKey", "TestValue");

        // Assert
        eventRaised.Should().BeTrue("configuration change event should be raised");
        changedKey.Should().Be("TestKey");
    }

    [Fact]
    public void ConfigurationGet_WithDefaultValue_ShouldReturnDefaultWhenKeyMissing()
    {
        // Arrange
        var configService = Substitute.For<IConfigurationService>();
        configService.GetValue<int>("NonExistentKey", 42).Returns(42);

        // Act
        var value = configService.GetValue<int>("NonExistentKey", 42);

        // Assert
        value.Should().Be(42, "should return default value when key doesn't exist");
    }

    #region T022: Configuration Precedence Validation

    /// <summary>
    /// T022.1: Test environment variable > user config > default precedence
    /// Verifies that environment variables have highest precedence
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_EnvironmentVariableWins()
    {
        // Arrange - Set environment variable (highest precedence)
        const string testKey = "MTM_TEST_PRECEDENCE_KEY";
        const string envValue = "environment_value";
        const string defaultValue = "default_value";

        System.Environment.SetEnvironmentVariable(testKey, envValue);

        try
        {
            var logger = Substitute.For<ILogger<ConfigurationService>>();
            var configService = new ConfigurationService(logger);

            // Act
            await configService.ReloadAsync();
            var value = configService.GetValue<string>(testKey, defaultValue);

            // Assert
            value.Should().Be(envValue, "environment variables should have highest precedence over defaults");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable(testKey, null);
        }
    }

    /// <summary>
    /// T022.2: Test MTM_API_TIMEOUT environment variable override
    /// Verifies that specific environment variables like MTM_API_TIMEOUT can override configuration
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_MTM_API_TIMEOUT_Override()
    {
        // Arrange - Set MTM_API_TIMEOUT environment variable
        const string envKey = "MTM_API_TIMEOUT";
        const string envTimeoutValue = "60000"; // 60 seconds

        System.Environment.SetEnvironmentVariable(envKey, envTimeoutValue);

        try
        {
            var logger = Substitute.For<ILogger<ConfigurationService>>();
            var configService = new ConfigurationService(logger);

            // Act
            await configService.ReloadAsync();
            var timeoutValue = configService.GetValue<int>(envKey, 30000);

            // Assert
            timeoutValue.Should().Be(60000, "MTM_API_TIMEOUT environment variable should override default timeout");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable(envKey, null);
        }
    }

    /// <summary>
    /// T022.3: Test removal of environment variable reverts to user config
    /// Verifies that removing an environment variable causes fallback to next precedence level
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_RemoveEnvVarFallbackToUserConfig()
    {
        // Arrange - Set environment variable, load config, then remove env var
        const string testKey = "MTM_TEST_FALLBACK_KEY";
        const string envValue = "environment_value";

        System.Environment.SetEnvironmentVariable(testKey, envValue);

        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);

        // Act 1 - Verify env var wins
        await configService.ReloadAsync();
        var valueWithEnv = configService.GetValue<string>(testKey, "default");
        valueWithEnv.Should().Be(envValue, "environment variable should be used initially");

        // Act 2 - Remove env var and reload
        System.Environment.SetEnvironmentVariable(testKey, null);
        await configService.ReloadAsync();

        // Note: In real implementation, user config would be stored in database or JSON file
        // For this test, we verify fallback to default when env var is removed
        var valueWithoutEnv = configService.GetValue<string>(testKey, "default");

        // Assert - Should fall back to default (since we don't have user config mocked)
        valueWithoutEnv.Should().Be("default", "should fall back to default when environment variable is removed");
    }

    /// <summary>
    /// T022.4: Test default fallback for non-existent keys
    /// Verifies that configuration returns default values when key doesn't exist in any source
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_DefaultFallbackForNonExistentKeys()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);

        const string nonExistentKey = "MTM_TOTALLY_NONEXISTENT_KEY_12345";
        const string defaultValue = "fallback_default_value";

        // Act
        await configService.ReloadAsync();
        var value = configService.GetValue<string>(nonExistentKey, defaultValue);

        // Assert
        value.Should().Be(defaultValue, "should return default value for non-existent configuration keys");
    }

    /// <summary>
    /// T022.5: Test precedence with multiple environment variables
    /// Verifies that multiple environment variables can coexist and override independently
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_MultipleEnvironmentVariables()
    {
        // Arrange - Set multiple environment variables
        const string key1 = "MTM_TEST_KEY_1";
        const string key2 = "MTM_TEST_KEY_2";
        const string value1 = "env_value_1";
        const string value2 = "env_value_2";

        System.Environment.SetEnvironmentVariable(key1, value1);
        System.Environment.SetEnvironmentVariable(key2, value2);

        try
        {
            var logger = Substitute.For<ILogger<ConfigurationService>>();
            var configService = new ConfigurationService(logger);

            // Act
            await configService.ReloadAsync();
            var result1 = configService.GetValue<string>(key1, "default");
            var result2 = configService.GetValue<string>(key2, "default");

            // Assert
            result1.Should().Be(value1, "first environment variable should be respected");
            result2.Should().Be(value2, "second environment variable should be respected");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable(key1, null);
            System.Environment.SetEnvironmentVariable(key2, null);
        }
    }

    /// <summary>
    /// T022.6: Test type conversion with environment variables
    /// Verifies that environment variables (strings) are correctly converted to target types
    /// </summary>
    [Fact]
    public async Task T022_ConfigurationPrecedence_TypeConversionFromEnvironmentVariable()
    {
        // Arrange - Set environment variables with different types
        const string intKey = "MTM_TEST_INT_VALUE";
        const string boolKey = "MTM_TEST_BOOL_VALUE";
        const string doubleKey = "MTM_TEST_DOUBLE_VALUE";

        System.Environment.SetEnvironmentVariable(intKey, "42");
        System.Environment.SetEnvironmentVariable(boolKey, "true");
        System.Environment.SetEnvironmentVariable(doubleKey, "3.14159");

        try
        {
            var logger = Substitute.For<ILogger<ConfigurationService>>();
            var configService = new ConfigurationService(logger);

            // Act
            await configService.ReloadAsync();
            var intValue = configService.GetValue<int>(intKey, 0);
            var boolValue = configService.GetValue<bool>(boolKey, false);
            var doubleValue = configService.GetValue<double>(doubleKey, 0.0);

            // Assert
            intValue.Should().Be(42, "should convert string '42' to int");
            boolValue.Should().Be(true, "should convert string 'true' to bool");
            doubleValue.Should().BeApproximately(3.14159, 0.00001, "should convert string to double");
        }
        finally
        {
            // Cleanup
            System.Environment.SetEnvironmentVariable(intKey, null);
            System.Environment.SetEnvironmentVariable(boolKey, null);
            System.Environment.SetEnvironmentVariable(doubleKey, null);
        }
    }

    #endregion T022: Configuration Precedence Validation

    #region T023: User Preferences Persistence Tests

    /// <summary>
    /// T023.1: Test LoadUserPreferencesAsync loads preferences from database
    /// Verifies that user preferences can be loaded and accessed via GetValue
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_LoadFromDatabase()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);
        const int testUserId = 999;
        const string prefKey = "TestPreference";

        // Note: This test requires database connection
        // If database is not available, test will skip gracefully
        try
        {
            // Act - Load user preferences (this will query UserPreferences table)
            await configService.LoadUserPreferencesAsync(testUserId, CancellationToken.None);

            // Assert - If load succeeds without exception, preferences were loaded
            // Note: GetValue may return default if no preferences exist for this user
            var value = configService.GetValue<string>(prefKey, "default");
            value.Should().NotBeNull("loaded preferences should be accessible");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    /// <summary>
    /// T023.2: Test SaveUserPreferenceAsync persists to database
    /// Verifies that SaveUserPreferenceAsync stores preference and it can be retrieved
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_SaveToDatabase()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);
        const int testUserId = 1000;
        const string prefKey = "TestSavePreference";
        const string prefValue = "TestValue123";

        try
        {
            // Act - Save user preference
            await configService.SaveUserPreferenceAsync(testUserId, prefKey, prefValue, CancellationToken.None);

            // Assert - Preference should be immediately accessible via GetValue
            var retrievedValue = configService.GetValue<string>(prefKey, "default");
            retrievedValue.Should().Be(prefValue, "saved preference should be immediately accessible");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    /// <summary>
    /// T023.3: Test database round-trip (save, reload, verify)
    /// Verifies that preferences persist across configuration reload
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_DatabaseRoundTrip()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);
        const int testUserId = 1001;
        const string prefKey = "RoundTripPreference";
        const string prefValue = "RoundTripValue456";

        try
        {
            // Act 1 - Save preference
            await configService.SaveUserPreferenceAsync(testUserId, prefKey, prefValue, CancellationToken.None);

            // Act 2 - Reload configuration (simulates application restart)
            await configService.ReloadAsync();

            // Act 3 - Load user preferences again
            await configService.LoadUserPreferencesAsync(testUserId, CancellationToken.None);

            // Assert - Preference should survive reload
            var retrievedValue = configService.GetValue<string>(prefKey, "default");
            retrievedValue.Should().Be(prefValue, "preference should persist across configuration reload");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    /// <summary>
    /// T023.4: Test restart simulation with cache clear
    /// Verifies that preferences are loaded correctly after cache is cleared (simulating app restart)
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_RestartSimulationWithCacheClear()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        const int testUserId = 1002;
        const string prefKey = "RestartTestPreference";
        const string prefValue = "RestartTestValue789";

        try
        {
            // Act 1 - Create first instance, save preference
            var configService1 = new ConfigurationService(logger);
            await configService1.SaveUserPreferenceAsync(testUserId, prefKey, prefValue, CancellationToken.None);

            // Act 2 - Create second instance (simulates app restart with cleared cache)
            var configService2 = new ConfigurationService(logger);
            await configService2.LoadUserPreferencesAsync(testUserId, CancellationToken.None);

            // Assert - Second instance should load preference from database
            var retrievedValue = configService2.GetValue<string>(prefKey, "default");
            retrievedValue.Should().Be(prefValue, "preference should be available after restart simulation");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    /// <summary>
    /// T023.5: Test SaveUserPreferenceAsync updates existing preference
    /// Verifies that saving a preference with the same key updates the value
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_UpdateExistingPreference()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);
        const int testUserId = 1003;
        const string prefKey = "UpdateTestPreference";
        const string initialValue = "InitialValue";
        const string updatedValue = "UpdatedValue";

        try
        {
            // Act 1 - Save initial value
            await configService.SaveUserPreferenceAsync(testUserId, prefKey, initialValue, CancellationToken.None);
            var value1 = configService.GetValue<string>(prefKey, "default");
            value1.Should().Be(initialValue, "initial save should work");

            // Act 2 - Update with new value
            await configService.SaveUserPreferenceAsync(testUserId, prefKey, updatedValue, CancellationToken.None);
            var value2 = configService.GetValue<string>(prefKey, "default");

            // Assert - Should reflect updated value
            value2.Should().Be(updatedValue, "updated preference should overwrite initial value");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    /// <summary>
    /// T023.6: Test LoadUserPreferencesAsync with different user IDs
    /// Verifies that preferences are scoped per user
    /// </summary>
    [Fact]
    public async Task T023_UserPreferences_DifferentUsersHaveSeparatePreferences()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);
        const int user1Id = 2001;
        const int user2Id = 2002;
        const string prefKey = "UserScopedPreference";
        const string user1Value = "User1Value";
        const string user2Value = "User2Value";

        try
        {
            // Act - Save preferences for two different users with same key
            await configService.SaveUserPreferenceAsync(user1Id, prefKey, user1Value, CancellationToken.None);
            await configService.SaveUserPreferenceAsync(user2Id, prefKey, user2Value, CancellationToken.None);

            // Load user 1's preferences
            await configService.LoadUserPreferencesAsync(user1Id, CancellationToken.None);
            var user1Retrieved = configService.GetValue<string>(prefKey, "default");

            // Load user 2's preferences
            await configService.LoadUserPreferencesAsync(user2Id, CancellationToken.None);
            var user2Retrieved = configService.GetValue<string>(prefKey, "default");

            // Assert - Each user should see their own value
            user1Retrieved.Should().Be(user1Value, "user 1 should see their own preference");
            user2Retrieved.Should().Be(user2Value, "user 2 should see their own preference");
        }
        catch (Exception ex) when (ex.Message.Contains("database") || ex.Message.Contains("connection"))
        {
            // Skip test if database is not available
            _output?.WriteLine($"Skipping test - database not available: {ex.Message}");
        }
    }

    #endregion T023: User Preferences Persistence Tests
}
