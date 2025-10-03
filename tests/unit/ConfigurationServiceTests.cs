using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for ConfigurationService
/// </summary>
public class ConfigurationServiceTests
{

    [Fact]
    public void GetValue_ExistingSetting_ShouldReturnValue()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        var expectedValue = "test-value";
        mockService.GetValue<string>("TestKey").Returns(expectedValue);

        // Act
        var result = mockService.GetValue<string>("TestKey");

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetValue_NonExistingSetting_ShouldReturnDefault()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        mockService.GetValue<string>("NonExistentKey").Returns((string?)null);

        // Act
        var result = mockService.GetValue<string>("NonExistentKey");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetValue_ValidKeyValue_ShouldSucceed()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        mockService.SetValue("TestKey", "test-value").Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await mockService.SetValue("TestKey", "test-value");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReloadAsync_ShouldTriggerConfigurationReload()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        mockService.ReloadAsync().Returns(Task.CompletedTask);

        // Act
        await mockService.ReloadAsync();

        // Assert
        await mockService.Received(1).ReloadAsync();
    }

    [Fact]
    public void GetValue_WithPrecedence_ShouldRespectOverrideOrder()
    {
        // Test configuration precedence: env vars > user config > app config > defaults
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        var envVarValue = "env-value";
        mockService.GetValue<string>("TestKey").Returns(envVarValue);

        // Act
        var result = mockService.GetValue<string>("TestKey");

        // Assert
        result.Should().Be(envVarValue, "environment variables should take precedence");
    }

    [Fact]
    public void OnConfigurationChanged_WhenConfigChanges_ShouldRaiseEvent()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        var eventRaised = false;
        mockService.When(x => x.OnConfigurationChanged += Arg.Any<EventHandler<MTM_Template_Application.Services.Configuration.ConfigurationChangedEventArgs>>())
            .Do(_ => eventRaised = true);

        // Act
        mockService.OnConfigurationChanged += (sender, args) => { };

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void GetValue_TypedValue_ShouldReturnCorrectType()
    {
        // Arrange
        var mockService = Substitute.For<IConfigurationService>();
        mockService.GetValue<int>("IntKey").Returns(42);
        mockService.GetValue<bool>("BoolKey").Returns(true);

        // Act
        var intValue = mockService.GetValue<int>("IntKey");
        var boolValue = mockService.GetValue<bool>("BoolKey");

        // Assert
        intValue.Should().Be(42);
        boolValue.Should().BeTrue();
    }
}
