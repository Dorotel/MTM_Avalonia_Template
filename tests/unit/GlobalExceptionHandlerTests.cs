using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.ErrorHandling;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for GlobalExceptionHandler service
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly ILogger<GlobalExceptionHandler> _mockLogger;
    private readonly ErrorCategorizer _errorCategorizer;
    private readonly DiagnosticBundleGenerator _bundleGenerator;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _errorCategorizer = new ErrorCategorizer();

        var bundleLogger = Substitute.For<ILogger<DiagnosticBundleGenerator>>();
        _bundleGenerator = new DiagnosticBundleGenerator(bundleLogger);

        _handler = new GlobalExceptionHandler(_mockLogger, _errorCategorizer, _bundleGenerator);
    }

    [Fact]
    public async Task HandleExceptionAsync_ValidException_ShouldReturnErrorReport()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var errorReport = await _handler.HandleExceptionAsync(exception);

        // Assert
        errorReport.Should().NotBeNull();
        errorReport.ErrorId.Should().NotBe(Guid.Empty);
        errorReport.Message.Should().Be("Test error");
        errorReport.Category.Should().NotBeNullOrEmpty();
        errorReport.Severity.Should().NotBeNullOrEmpty();
        errorReport.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleExceptionAsync_ShouldGenerateDiagnosticBundle()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var errorReport = await _handler.HandleExceptionAsync(exception);

        // Assert
        errorReport.DiagnosticBundle.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleExceptionAsync_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        await _handler.HandleExceptionAsync(exception);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>()
        );
    }

    [Fact]
    public async Task HandleExceptionAsync_NetworkException_ShouldCategorizeCorrectly()
    {
        // Arrange
        var exception = new System.Net.Http.HttpRequestException("Network error");

        // Act
        var errorReport = await _handler.HandleExceptionAsync(exception);

        // Assert
        errorReport.Category.Should().Be("Transient");
        errorReport.Severity.Should().Be("Medium");
    }

    [Fact]
    public async Task HandleExceptionAsync_CriticalException_ShouldMarkAsCritical()
    {
        // Arrange
        var exception = new OutOfMemoryException("Out of memory");

        // Act
        var errorReport = await _handler.HandleExceptionAsync(exception);

        // Assert
        errorReport.Severity.Should().Be("Critical");
    }

    [Fact]
    public async Task HandleExceptionAsync_NullException_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _handler.HandleExceptionAsync(null!);
        });
    }

    [Fact]
    public void RegisterGlobalHandlers_ShouldNotThrow()
    {
        // Act
        Action act = () => _handler.RegisterGlobalHandlers();

        // Assert
        act.Should().NotThrow();
    }
}
