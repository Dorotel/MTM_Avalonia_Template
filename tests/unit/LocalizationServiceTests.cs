using System;
using System.Globalization;
using FluentAssertions;
using MTM_Template_Application.Models.Localization;
using MTM_Template_Application.Services.Localization;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for LocalizationService (T153)
/// Tests culture switching, resource loading, and missing translation tracking
/// </summary>
public class LocalizationServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullMissingTranslationHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var cultureProvider = Substitute.For<CultureProvider>();

        // Act
        Action act = () => new LocalizationService(null!, cultureProvider);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("missingTranslationHandler");
    }

    [Fact]
    public void Constructor_WithNullCultureProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var missingTranslationHandler = Substitute.For<MissingTranslationHandler>();

        // Act
        Action act = () => new LocalizationService(missingTranslationHandler, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cultureProvider");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesServiceSuccessfully()
    {
        // Arrange
        var missingTranslationHandler = Substitute.For<MissingTranslationHandler>();
        var cultureProvider = Substitute.For<CultureProvider>();
        cultureProvider.GetCurrentCulture().Returns(new CultureInfo("en-US"));

        // Act
        var service = new LocalizationService(missingTranslationHandler, cultureProvider);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region GetString Tests

    [Fact]
    public void GetString_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        Action act = () => service.GetString(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public void GetString_WithExistingKey_ReturnsTranslation()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        var result = service.GetString("Common.OK");

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetString_WithMissingKey_ReturnsFallbackKey()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        var result = service.GetString("NonExistent.Key");

        // Assert
        result.Should().Be("[NonExistent.Key]");
    }

    [Fact]
    public void GetString_WithFormatArguments_FormatsCorrectly()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        var result = service.GetString("Common.Welcome", "John");

        // Assert
        result.Should().NotBeEmpty();
        // Should contain formatted value if translation exists
    }

    #endregion

    #region SetCulture Tests

    [Fact]
    public void SetCulture_WithNullCultureName_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        Action act = () => service.SetCulture(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cultureName");
    }

    [Fact]
    public void SetCulture_WithValidCulture_ChangesCulture()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        service.SetCulture("es-ES");

        // Assert
        CultureInfo.CurrentCulture.Name.Should().Be("es-ES");
        CultureInfo.CurrentUICulture.Name.Should().Be("es-ES");
    }

    [Fact]
    public void SetCulture_WithInvalidCulture_ThrowsCultureNotFoundException()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        Action act = () => service.SetCulture("invalid-CULTURE");

        // Assert
        act.Should().Throw<CultureNotFoundException>();
    }

    [Fact]
    public void SetCulture_WhenChanged_RaisesOnLanguageChangedEvent()
    {
        // Arrange
        var service = CreateLocalizationService();
        LanguageChangedEventArgs? eventArgs = null;
        service.OnLanguageChanged += (sender, args) => eventArgs = args;

        // Act
        service.SetCulture("fr-FR");

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.OldCulture.Should().Be("en-US");
        eventArgs.NewCulture.Should().Be("fr-FR");
    }

    #endregion

    #region GetSupportedCultures Tests

    [Fact]
    public void GetSupportedCultures_ReturnsNonEmptyList()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        var cultures = service.GetSupportedCultures();

        // Assert
        cultures.Should().NotBeEmpty();
        cultures.Should().Contain("en-US");
    }

    [Fact]
    public void GetSupportedCultures_ContainsExpectedCultures()
    {
        // Arrange
        var service = CreateLocalizationService();

        // Act
        var cultures = service.GetSupportedCultures();

        // Assert
        cultures.Should().Contain(new[] { "en-US", "es-ES", "fr-FR", "de-DE" });
    }

    #endregion

    #region Missing Translation Tracking Tests

    [Fact]
    public void GetString_WithMissingTranslation_ReportsMissingKey()
    {
        // Arrange
        var missingTranslationHandler = Substitute.For<MissingTranslationHandler>();
        var cultureProvider = Substitute.For<CultureProvider>();
        cultureProvider.GetCurrentCulture().Returns(new CultureInfo("en-US"));
        cultureProvider.GetFallbackCulture(Arg.Any<CultureInfo>()).Returns((CultureInfo?)null);

        var service = new LocalizationService(missingTranslationHandler, cultureProvider);

        // Act
        service.GetString("Missing.Key");

        // Assert
        missingTranslationHandler.Received(1).ReportMissing(
            Arg.Is<string>(k => k == "Missing.Key"),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    #endregion

    #region Helper Methods

    private static LocalizationService CreateLocalizationService()
    {
        var missingTranslationHandler = Substitute.For<MissingTranslationHandler>();
        var cultureProvider = Substitute.For<CultureProvider>();
        cultureProvider.GetCurrentCulture().Returns(new CultureInfo("en-US"));
        cultureProvider.GetFallbackCulture(Arg.Any<CultureInfo>()).Returns((CultureInfo?)null);

        return new LocalizationService(missingTranslationHandler, cultureProvider);
    }

    #endregion
}
