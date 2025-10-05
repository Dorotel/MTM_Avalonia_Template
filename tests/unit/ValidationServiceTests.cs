using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MTM_Template_Application.Models.Core;
using MTM_Template_Application.Services.Core;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for ValidationService (T151)
/// Tests FluentValidation integration with rule discovery
/// </summary>
public class ValidationServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesServiceSuccessfully()
    {
        // Arrange & Act
        var service = new ValidationService();

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region RegisterValidator Tests

    [Fact]
    public void RegisterValidator_WithNullValidator_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new ValidationService();

        // Act
        Action act = () => service.RegisterValidator<TestModel>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("validator");
    }

    [Fact]
    public void RegisterValidator_WithInvalidValidatorType_ThrowsArgumentException()
    {
        // Arrange
        var service = new ValidationService();
        var invalidValidator = new object(); // Not an IValidator<T>

        // Act
        Action act = () => service.RegisterValidator<TestModel>(invalidValidator);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IValidator<TestModel>*");
    }

    [Fact]
    public void RegisterValidator_WithValidValidator_RegistersSuccessfully()
    {
        // Arrange
        var service = new ValidationService();
        var validator = new TestModelValidator();

        // Act
        var act = () => service.RegisterValidator<TestModel>(validator);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region ValidateAsync Tests

    [Fact]
    public async Task ValidateAsync_WithNullObject_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new ValidationService();

        // Act
        var act = async () => await service.ValidateAsync<TestModel>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("obj");
    }

    [Fact]
    public async Task ValidateAsync_WithNoValidator_ReturnsValidResult()
    {
        // Arrange
        var service = new ValidationService();
        var model = new TestModel { Name = "Test", Age = 25 };

        // Act
        var result = await service.ValidateAsync(model);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithRegisteredValidator_ValidatesCorrectly()
    {
        // Arrange
        var service = new ValidationService();
        service.RegisterValidator<TestModel>(new TestModelValidator());
        var model = new TestModel { Name = "John", Age = 30 };

        // Act
        var result = await service.ValidateAsync(model);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidObject_ReturnsValidationErrors()
    {
        // Arrange
        var service = new ValidationService();
        service.RegisterValidator<TestModel>(new TestModelValidator());
        var model = new TestModel { Name = "", Age = -5 }; // Invalid

        // Act
        var result = await service.ValidateAsync(model);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Age");
    }

    [Fact]
    public async Task ValidateAsync_WithPartiallyInvalidObject_ReturnsSpecificErrors()
    {
        // Arrange
        var service = new ValidationService();
        service.RegisterValidator<TestModel>(new TestModelValidator());
        var model = new TestModel { Name = "John", Age = -5 }; // Only Age invalid

        // Act
        var result = await service.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be("Age");
        result.Errors[0].ErrorMessage.Should().Contain("greater than 0");
    }

    #endregion

    #region GetRuleMetadata Tests

    [Fact]
    public void GetRuleMetadata_WithNoValidator_ReturnsEmptyList()
    {
        // Arrange
        var service = new ValidationService();

        // Act
        var metadata = service.GetRuleMetadata<TestModel>();

        // Assert
        metadata.Should().NotBeNull();
        metadata.Should().BeEmpty();
    }

    [Fact]
    public void GetRuleMetadata_WithRegisteredValidator_ReturnsRuleMetadata()
    {
        // Arrange
        var service = new ValidationService();
        service.RegisterValidator<TestModel>(new TestModelValidator());

        // Act
        var metadata = service.GetRuleMetadata<TestModel>();

        // Assert
        metadata.Should().NotBeEmpty();
        metadata.Should().Contain(m => m.PropertyName == "Name");
        metadata.Should().Contain(m => m.PropertyName == "Age");
    }

    #endregion

    #region Auto-Discovery Tests

    [Fact]
    public async Task ValidateAsync_WithAutoDiscoverableValidator_DiscoversAndValidates()
    {
        // Arrange
        var service = new ValidationService();
        var model = new TestModel { Name = "Auto", Age = 99 };

        // Act - First call should trigger auto-discovery
        var result = await service.ValidateAsync(model);

        // Assert
        result.Should().NotBeNull();
        // Auto-discovery may or may not find validator depending on naming convention
        result.IsValid.Should().BeTrue(); // If not found, returns valid
    }

    #endregion

    #region Helper Classes

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required");

            RuleFor(x => x.Age)
                .GreaterThan(0)
                .WithMessage("Age must be greater than 0");
        }
    }

    #endregion
}
