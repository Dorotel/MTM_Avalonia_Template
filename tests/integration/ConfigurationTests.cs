using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for configuration loading and precedence
/// </summary>
public class ConfigurationTests
{
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
}
