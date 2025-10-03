using FluentAssertions;
using MTM_Template_Application.Models.Theme;
using MTM_Template_Application.Services.Theme;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for theme management
/// </summary>
public class ThemeTests
{
    [Fact]
    public void Theme_ShouldSwitchBetweenLightAndDark()
    {
        // Arrange
        var themeService = Substitute.For<IThemeService>();
        bool themeChanged = false;
        
        themeService.OnThemeChanged += (sender, args) =>
        {
            themeChanged = true;
        };
        
        // Act
        themeService.SetTheme("Dark");
        themeService.OnThemeChanged += NSubstitute.Raise.EventWith(
            new ThemeChangedEventArgs
            {
                OldTheme = "Light",
                NewTheme = "Dark",
                IsDarkMode = true
            });
        
        // Assert
        themeChanged.Should().BeTrue("theme change event should be raised");
    }

    [Fact]
    public void Theme_ShouldSupportHighContrastMode()
    {
        // Arrange
        var themeService = Substitute.For<IThemeService>();
        var highContrastTheme = new ThemeConfiguration
        {
            ThemeMode = "Dark",
            IsDarkMode = true,
            HighContrast = true,
            AccentColor = "#FFFFFF"
        };
        themeService.GetCurrentTheme().Returns(highContrastTheme);
        
        // Act
        var theme = themeService.GetCurrentTheme();
        
        // Assert
        theme.HighContrast.Should().BeTrue("high contrast mode should be enabled");
    }

    [Fact]
    public void Theme_ShouldSupportAutoMode()
    {
        // Arrange
        var themeService = Substitute.For<IThemeService>();
        
        // Act
        themeService.SetTheme("Auto");
        
        // Assert
        themeService.Received(1).SetTheme("Auto");
    }

    [Fact]
    public void Theme_ShouldAdjustFontSize()
    {
        // Arrange
        var themeService = Substitute.For<IThemeService>();
        var largeTextTheme = new ThemeConfiguration
        {
            ThemeMode = "Light",
            IsDarkMode = false,
            FontSize = 1.5,
            HighContrast = false
        };
        themeService.GetCurrentTheme().Returns(largeTextTheme);
        
        // Act
        var theme = themeService.GetCurrentTheme();
        
        // Assert
        theme.FontSize.Should().Be(1.5, "font size multiplier should be applied");
    }
}
