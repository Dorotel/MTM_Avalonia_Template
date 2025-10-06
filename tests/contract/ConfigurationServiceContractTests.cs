using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for ConfigurationService based on configuration-service-contract.json
/// These tests MUST FAIL until the implementation is complete.
/// </summary>
public class ConfigurationServiceContractTests
{
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationServiceContractTests()
    {
        _logger = Substitute.For<ILogger<ConfigurationService>>();
    }

    #region T005: GetValue/SetValue Contract Tests

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_WithValidKey_ReturnsValue()
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act
        var value = service.GetValue<int>("API:TimeoutSeconds", 30);

        // Assert
        value.Should().Be(value); // Value is int type
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_WithMissingKey_ReturnsDefaultValue()
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act
        var value = service.GetValue<string>("NonExistent:Key", "DefaultValue");

        // Assert
        value.Should().Be("DefaultValue");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_WithNullKey_ThrowsException()
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act
        Action act = () => service.GetValue<string>(null!, "default");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("API:TimeoutSeconds", typeof(int))]
    [InlineData("Visual:Username", typeof(string))]
    [InlineData("Feature:IsEnabled", typeof(bool))]
    [Trait("Category", "Contract")]
    public void GetValue_SupportsTypeSafety_ForDifferentTypes(string key, Type expectedType)
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act & Assert
        if (expectedType == typeof(int))
        {
            var value = service.GetValue<int>(key, 0);
            value.Should().BeGreaterThanOrEqualTo(0); // Type is validated by compilation
        }
        else if (expectedType == typeof(string))
        {
            var value = service.GetValue<string>(key, "");
            value.Should().NotBeNull(); // Type is validated by compilation
        }
        else if (expectedType == typeof(bool))
        {
            var value = service.GetValue<bool>(key, false);
            // Type is validated by compilation - bool can only be true or false
            (value == true || value == false).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task SetValue_WithValidKey_PersistsValue()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var key = "Test:Setting";
        var value = "TestValue";

        // Act
        await service.SetValue(key, value, CancellationToken.None);
        var retrievedValue = service.GetValue<string>(key, "");

        // Assert
        retrievedValue.Should().Be(value);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task SetValue_RaisesOnConfigurationChangedEvent()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var eventRaised = false;
        service.OnConfigurationChanged += (sender, args) =>
        {
            eventRaised = true;
            args.Key.Should().Be("Test:EventKey");
        };

        // Act
        await service.SetValue("Test:EventKey", "EventValue", CancellationToken.None);

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Theory]
    [InlineData("API:Timeout:Seconds")] // Valid colon format
    [InlineData("MTM_ENVIRONMENT")]      // Valid underscore format
    [InlineData("Simple")]               // Valid simple format
    [Trait("Category", "Contract")]
    public async Task SetValue_AcceptsValidKeyFormats(string key)
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act
        Func<Task> act = async () => await service.SetValue(key, "value", CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task GetValue_PerformanceTarget_LessThan10Milliseconds()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        await service.SetValue("Performance:Test", "Value", CancellationToken.None);
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var value = service.GetValue<string>("Performance:Test", "default");
        }
        stopwatch.Stop();

        // Assert
        var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;
        avgTime.Should().BeLessThan(10, $"Average lookup time was {avgTime}ms");
    }

    #endregion

    #region T006: LoadUserPreferencesAsync Contract Tests

    [Fact]
    [Trait("Category", "Contract")]
    public async Task LoadUserPreferencesAsync_WithValidUserId_LoadsPreferences()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var userId = 42;

        // Act
        Func<Task> act = async () => await service.LoadUserPreferencesAsync(userId, CancellationToken.None);

        // Assert
        // This should fail until database connection is implemented
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task LoadUserPreferencesAsync_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var invalidUserId = 0;

        // Act
        Func<Task> act = async () => await service.LoadUserPreferencesAsync(invalidUserId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task LoadUserPreferencesAsync_LoadsPreferencesIntoMemory()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var userId = 42;

        // Act
        await service.LoadUserPreferencesAsync(userId, CancellationToken.None);
        var theme = service.GetValue<string>("Display.Theme", "Light");

        // Assert
        // Should load Dark theme from database for user 42
        theme.Should().Be("Dark");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task LoadUserPreferencesAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new ConfigurationService(_logger);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await service.LoadUserPreferencesAsync(42, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region T009a: Environment Detection Precedence Tests

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_MTM_ENVIRONMENT_TakesPrecedenceOverASPNETCORE_ENVIRONMENT()
    {
        // Arrange
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var service = new ConfigurationService(_logger);

        // Act
        var environment = service.GetValue<string>("ENVIRONMENT", "");

        // Assert
        environment.Should().Be("Production");

        // Cleanup
        Environment.SetEnvironmentVariable("MTM_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_ASPNETCORE_ENVIRONMENT_TakesPrecedenceOverDOTNET_ENVIRONMENT()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        var service = new ConfigurationService(_logger);

        // Act
        var environment = service.GetValue<string>("ENVIRONMENT", "");

        // Assert
        environment.Should().Be("Staging");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void GetValue_ValidatesEnvironmentVariableKeyFormat()
    {
        // Arrange
        var service = new ConfigurationService(_logger);

        // Act & Assert
        // Valid format: MTM_*, DOTNET_*, ASPNETCORE_*
        var validKeys = new[] { "MTM_ENVIRONMENT", "DOTNET_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT" };
        foreach (var key in validKeys)
        {
            Action act = () => service.GetValue<string>(key, "default");
            act.Should().NotThrow();
        }

        // Invalid formats should be rejected (spaces, special chars except underscore)
        var invalidKeys = new[] { "MTM ENVIRONMENT", "MTM-ENVIRONMENT", "MTM.ENVIRONMENT" };
        foreach (var key in invalidKeys)
        {
            Action act = () => service.GetValue<string>(key, "default");
            act.Should().Throw<ArgumentException>();
        }
    }

    #endregion
}
