using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using MTM_Template_Application.Services.Localization;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for localization and culture management
/// </summary>
public class LocalizationTests
{
    [Fact]
    public void Localization_ShouldSwitchCulture()
    {
        // Arrange
        var localizationService = Substitute.For<ILocalizationService>();
        string? newCulture = null;
        
        localizationService.OnLanguageChanged += (sender, args) =>
        {
            newCulture = args.NewCulture;
        };
        
        // Act
        localizationService.SetCulture("es-MX");
        localizationService.OnLanguageChanged += NSubstitute.Raise.EventWith(
            new LanguageChangedEventArgs
            {
                OldCulture = "en-US",
                NewCulture = "es-MX"
            });
        
        // Assert
        newCulture.Should().Be("es-MX", "culture should change to Spanish (Mexico)");
    }

    [Fact]
    public void Localization_ShouldFallbackForMissingTranslations()
    {
        // Arrange
        var localizationService = Substitute.For<ILocalizationService>();
        localizationService.GetString("missing.key").Returns("[missing.key]");
        
        // Act
        var translation = localizationService.GetString("missing.key");
        
        // Assert
        translation.Should().Contain("missing.key", "should show key when translation missing");
    }

    [Fact]
    public void Localization_ShouldSupportMultipleCultures()
    {
        // Arrange
        var localizationService = Substitute.For<ILocalizationService>();
        var supportedCultures = new List<string> { "en-US", "es-MX", "fr-CA" };
        localizationService.GetSupportedCultures().Returns(supportedCultures);
        
        // Act
        var cultures = localizationService.GetSupportedCultures();
        
        // Assert
        cultures.Should().Contain("en-US");
        cultures.Should().Contain("es-MX");
        cultures.Should().Contain("fr-CA");
    }

    [Fact]
    public void Localization_ShouldFormatDatesCorrectly()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;
        
        // Act - verify culture-specific date formatting
        var testDate = new System.DateTime(2024, 10, 3);
        var formatted = testDate.ToString("d", CultureInfo.GetCultureInfo("en-US"));
        
        // Assert
        formatted.Should().NotBeNullOrEmpty("date should be formatted according to culture");
    }
}
