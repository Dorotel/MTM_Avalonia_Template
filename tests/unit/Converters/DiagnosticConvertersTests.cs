using System.Globalization;
using Avalonia.Media;
using FluentAssertions;
using MTM_Template_Application.Converters;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.unit.Converters;

/// <summary>
/// Unit tests for diagnostic value converters
/// Tests: T050 - Value converter unit tests
/// </summary>
public class DiagnosticConvertersTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    #region MemoryUsageToColorConverter Tests

    [Fact]
    public void MemoryUsageToColorConverter_Should_Return_Green_When_Below_70MB()
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();
        var memoryUsage = 50.0; // Below yellow threshold

        // Act
        var result = converter.Convert(memoryUsage, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(76, 175, 80)); // Green #4CAF50
    }

    [Fact]
    public void MemoryUsageToColorConverter_Should_Return_Yellow_When_Between_70_And_90MB()
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();
        var memoryUsage = 80.0; // Between thresholds

        // Act
        var result = converter.Convert(memoryUsage, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(255, 235, 59)); // Yellow #FFEB3B
    }

    [Fact]
    public void MemoryUsageToColorConverter_Should_Return_Red_When_Above_90MB()
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();
        var memoryUsage = 100.0; // Above red threshold

        // Act
        var result = converter.Convert(memoryUsage, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(244, 67, 54)); // Red #F44336
    }

    [Theory]
    [InlineData(69.9)] // Just below yellow threshold
    [InlineData(70.0)] // Exactly at yellow threshold (should be yellow)
    [InlineData(89.9)] // Just below red threshold
    [InlineData(90.0)] // Exactly at red threshold (should be red)
    public void MemoryUsageToColorConverter_Should_Handle_Boundary_Values(double memoryUsage)
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();

        // Act
        var result = converter.Convert(memoryUsage, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();

        var brush = (SolidColorBrush)result!;
        if (memoryUsage < 70.0)
            brush.Color.Should().Be(Color.FromRgb(76, 175, 80)); // Green
        else if (memoryUsage < 90.0)
            brush.Color.Should().Be(Color.FromRgb(255, 235, 59)); // Yellow
        else
            brush.Color.Should().Be(Color.FromRgb(244, 67, 54)); // Red
    }

    [Fact]
    public void MemoryUsageToColorConverter_Should_Return_Gray_When_Value_Is_Null()
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();

        // Act
        var result = converter.Convert(null, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(158, 158, 158)); // Gray #9E9E9E
    }

    [Fact]
    public void MemoryUsageToColorConverter_ConvertBack_Should_Throw_NotSupportedException()
    {
        // Arrange
        var converter = new MemoryUsageToColorConverter();
        var brush = new SolidColorBrush(Colors.Green);

        // Act & Assert
        var act = () => converter.ConvertBack(brush, typeof(double), null, _culture);
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*one-way binding*");
    }

    #endregion

    #region BootStageToColorConverter Tests

    [Theory]
    [InlineData(500.0, "Stage0", true)]   // Stage 0: 500ms < 1000ms (green)
    [InlineData(1500.0, "Stage0", false)] // Stage 0: 1500ms > 1000ms (red)
    [InlineData(2000.0, "Stage1", true)]  // Stage 1: 2000ms < 3000ms (green)
    [InlineData(3500.0, "Stage1", false)] // Stage 1: 3500ms > 3000ms (red)
    [InlineData(800.0, "Stage2", true)]   // Stage 2: 800ms < 1000ms (green)
    [InlineData(1200.0, "Stage2", false)] // Stage 2: 1200ms > 1000ms (red)
    public void BootStageToColorConverter_Should_Return_Correct_Color_For_Stage(
        double durationMs, string stageId, bool shouldBeGreen)
    {
        // Arrange
        var converter = new BootStageToColorConverter();

        // Act
        var result = converter.Convert(durationMs, typeof(SolidColorBrush), stageId, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;

        if (shouldBeGreen)
            brush.Color.Should().Be(Color.FromRgb(76, 175, 80)); // Green #4CAF50
        else
            brush.Color.Should().Be(Color.FromRgb(244, 67, 54)); // Red #F44336
    }

    [Theory]
    [InlineData(1000.0, "Stage0")] // Exactly at target
    [InlineData(3000.0, "Stage1")] // Exactly at target
    [InlineData(1000.0, "Stage2")] // Exactly at target
    public void BootStageToColorConverter_Should_Return_Green_When_At_Target(double durationMs, string stageId)
    {
        // Arrange
        var converter = new BootStageToColorConverter();

        // Act
        var result = converter.Convert(durationMs, typeof(SolidColorBrush), stageId, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(76, 175, 80)); // Green (at target is considered good)
    }

    [Fact]
    public void BootStageToColorConverter_Should_Return_Gray_When_Value_Is_Null()
    {
        // Arrange
        var converter = new BootStageToColorConverter();

        // Act
        var result = converter.Convert(null, typeof(SolidColorBrush), "Stage0", _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(158, 158, 158)); // Gray #9E9E9E
    }

    [Fact]
    public void BootStageToColorConverter_Should_Return_Gray_When_Parameter_Is_Null()
    {
        // Arrange
        var converter = new BootStageToColorConverter();

        // Act
        var result = converter.Convert(1000.0, typeof(SolidColorBrush), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(158, 158, 158)); // Gray #9E9E9E
    }

    [Fact]
    public void BootStageToColorConverter_Should_Return_Green_For_Unknown_Stage()
    {
        // Arrange
        var converter = new BootStageToColorConverter();

        // Act
        var result = converter.Convert(5000.0, typeof(SolidColorBrush), "UnknownStage", _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Color.FromRgb(76, 175, 80)); // Green (unknown stages always pass)
    }

    [Fact]
    public void BootStageToColorConverter_ConvertBack_Should_Throw_NotSupportedException()
    {
        // Arrange
        var converter = new BootStageToColorConverter();
        var brush = new SolidColorBrush(Colors.Green);

        // Act & Assert
        var act = () => converter.ConvertBack(brush, typeof(double), "Stage0", _culture);
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*one-way binding*");
    }

    #endregion

    #region ErrorSeverityToIconConverter Tests

    [Theory]
    [InlineData(ErrorSeverity.Critical, "🔴")]
    [InlineData(ErrorSeverity.Error, "🔴")]
    [InlineData(ErrorSeverity.Warning, "🟡")]
    [InlineData(ErrorSeverity.Info, "ℹ️")]
    public void ErrorSeverityToIconConverter_Should_Return_Correct_Icon_For_Severity(
        ErrorSeverity severity, string expectedIcon)
    {
        // Arrange
        var converter = new ErrorSeverityToIconConverter();

        // Act
        var result = converter.Convert(severity, typeof(string), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().Be(expectedIcon);
    }

    [Fact]
    public void ErrorSeverityToIconConverter_Should_Return_Unknown_Icon_When_Value_Is_Null()
    {
        // Arrange
        var converter = new ErrorSeverityToIconConverter();

        // Act
        var result = converter.Convert(null, typeof(string), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("❔"); // Unknown icon
    }

    [Fact]
    public void ErrorSeverityToIconConverter_Should_Return_Unknown_Icon_For_Invalid_Severity()
    {
        // Arrange
        var converter = new ErrorSeverityToIconConverter();
        var invalidSeverity = (ErrorSeverity)999; // Invalid enum value

        // Act
        var result = converter.Convert(invalidSeverity, typeof(string), null, _culture);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be("❔"); // Unknown icon
    }

    [Fact]
    public void ErrorSeverityToIconConverter_ConvertBack_Should_Throw_NotSupportedException()
    {
        // Arrange
        var converter = new ErrorSeverityToIconConverter();

        // Act & Assert
        var act = () => converter.ConvertBack("🔴", typeof(ErrorSeverity), null, _culture);
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*one-way binding*");
    }

    #endregion
}
