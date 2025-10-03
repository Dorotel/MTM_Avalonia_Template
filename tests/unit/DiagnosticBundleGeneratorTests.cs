using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.ErrorHandling;
using MTM_Template_Application.Services.ErrorHandling;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for DiagnosticBundleGenerator service
/// </summary>
public class DiagnosticBundleGeneratorTests
{
    private readonly DiagnosticBundleGenerator _generator;
    private readonly ILogger<DiagnosticBundleGenerator> _mockLogger;

    public DiagnosticBundleGeneratorTests()
    {
        _mockLogger = Substitute.For<ILogger<DiagnosticBundleGenerator>>();
        _generator = new DiagnosticBundleGenerator(_mockLogger);
    }

    [Fact]
    public async Task GenerateAsync_ValidException_ShouldReturnBase64String()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var errorReport = new ErrorReport
        {
            ErrorId = Guid.NewGuid(),
            Message = "Test error",
            Category = "Test",
            Severity = "Medium",
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Act
        var bundle = await _generator.GenerateAsync(exception, errorReport);

        // Assert
        bundle.Should().NotBeNullOrEmpty();
        bundle.Should().MatchRegex("^[A-Za-z0-9+/=]+$", "bundle should be valid base64");
    }

    [Fact]
    public async Task GenerateAsync_BundleCanBeDecompressed()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var errorReport = new ErrorReport
        {
            ErrorId = Guid.NewGuid(),
            Message = "Test error",
            Category = "Test",
            Severity = "Medium",
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Act
        var bundle = await _generator.GenerateAsync(exception, errorReport);
        var decompressed = await DiagnosticBundleGenerator.DecompressBundleAsync(bundle);

        // Assert
        decompressed.Should().NotBeNullOrEmpty();
        decompressed.Should().Contain("Test error");
        decompressed.Should().Contain("ErrorReport");
        decompressed.Should().Contain("Environment");
    }

    [Fact]
    public async Task GenerateAsync_ShouldIncludeSystemInformation()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var errorReport = new ErrorReport
        {
            ErrorId = Guid.NewGuid(),
            Message = "Test error",
            Category = "Test",
            Severity = "Medium",
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Act
        var bundle = await _generator.GenerateAsync(exception, errorReport);
        var decompressed = await DiagnosticBundleGenerator.DecompressBundleAsync(bundle);

        // Assert
        decompressed.Should().Contain("OSVersion");
        decompressed.Should().Contain("Architecture");
        decompressed.Should().Contain("FrameworkDescription");
        decompressed.Should().Contain("MachineName");
        decompressed.Should().Contain("ProcessorCount");
    }

    [Fact]
    public async Task GenerateAsync_ShouldIncludeExceptionDetails()
    {
        // Arrange
        var innerException = new ArgumentNullException("param");
        var exception = new InvalidOperationException("Test error", innerException);
        var errorReport = new ErrorReport
        {
            ErrorId = Guid.NewGuid(),
            Message = "Test error",
            Category = "Test",
            Severity = "Medium",
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Act
        var bundle = await _generator.GenerateAsync(exception, errorReport);
        var decompressed = await DiagnosticBundleGenerator.DecompressBundleAsync(bundle);

        // Assert
        decompressed.Should().Contain("Test error");
        decompressed.Should().Contain("InvalidOperationException");
        decompressed.Should().Contain("param");
    }

    [Fact]
    public async Task DecompressBundleAsync_InvalidBase64_ShouldThrow()
    {
        // Arrange
        var invalidBundle = "not-valid-base64!!!";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(async () =>
        {
            await DiagnosticBundleGenerator.DecompressBundleAsync(invalidBundle);
        });
    }
}
