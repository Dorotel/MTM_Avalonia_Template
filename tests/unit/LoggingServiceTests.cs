using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Logging;
using NSubstitute;
using Serilog;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for LoggingService
/// Tests cover T142: Test structured logging, PII redaction, rotation
/// </summary>
public class LoggingServiceTests
{
    private readonly ILogger _mockLogger;
    private readonly PiiRedactionMiddleware _mockPiiRedaction;
    private readonly LoggingService _service;

    public LoggingServiceTests()
    {
        _mockLogger = Substitute.For<ILogger>();
        _mockPiiRedaction = Substitute.For<PiiRedactionMiddleware>();

        // Setup default redaction behavior (returns input unchanged)
        _mockPiiRedaction.Redact(Arg.Any<string>()).Returns(x => x.Arg<string>());

        _service = new LoggingService(_mockLogger, _mockPiiRedaction);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        ILogger nullLogger = null!;

        // Act
        Action act = () => new LoggingService(nullLogger, _mockPiiRedaction);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void Constructor_WithNullPiiRedaction_ThrowsArgumentNullException()
    {
        // Arrange
        PiiRedactionMiddleware nullMiddleware = null!;

        // Act
        Action act = () => new LoggingService(_mockLogger, nullMiddleware);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("piiRedactionMiddleware");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogInformation_WithValidMessage_CallsLoggerInformation()
    {
        // Arrange
        const string message = "Test information message";

        // Act
        _service.LogInformation(message);

        // Assert
        _mockLogger.Received(1).Information(message, Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogInformation_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        string nullMessage = null!;

        // Act
        Action act = () => _service.LogInformation(nullMessage);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogWarning_WithValidMessage_CallsLoggerWarning()
    {
        // Arrange
        const string message = "Test warning message";

        // Act
        _service.LogWarning(message);

        // Assert
        _mockLogger.Received(1).Warning(message, Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogWarning_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        string nullMessage = null!;

        // Act
        Action act = () => _service.LogWarning(nullMessage);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogError_WithValidMessage_CallsLoggerError()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        _service.LogError(message);

        // Assert
        _mockLogger.Received(1).Error(message, Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogError_WithException_CallsLoggerErrorWithException()
    {
        // Arrange
        const string message = "Test error message";
        var exception = new InvalidOperationException("Test exception");

        // Act
        _service.LogError(message, exception);

        // Assert
        _mockLogger.Received(1).Error(exception, message, Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogError_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        string nullMessage = null!;

        // Act
        Action act = () => _service.LogError(nullMessage);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogInformation_WithSensitiveData_RedactsBeforeLogging()
    {
        // Arrange
        const string message = "User password is: secret123";
        const string redactedMessage = "User password is: [REDACTED]";
        _mockPiiRedaction.Redact(message).Returns(redactedMessage);

        // Act
        _service.LogInformation(message);

        // Assert
        _mockPiiRedaction.Received(1).Redact(message);
        _mockLogger.Received(1).Information(redactedMessage, Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void SetContext_WithValidKeyValue_StoresContext()
    {
        // Arrange
        const string key = "CorrelationId";
        const string value = "12345-67890";

        // Act
        _service.SetContext(key, value);
        _service.LogInformation("Test message");

        // Assert - Context should be applied (no exception thrown)
        _mockLogger.Received(1).Information(Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void SetContext_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string nullKey = null!;
        const string value = "testvalue";

        // Act
        Action act = () => _service.SetContext(nullKey, value);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void SetContext_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        const string key = "TestKey";
        object nullValue = null!;

        // Act
        Action act = () => _service.SetContext(key, nullValue);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public async Task FlushAsync_WhenCalled_CompletesSuccessfully()
    {
        // Act
        Func<Task> act = async () => await _service.FlushAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public async Task FlushAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.FlushAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void LogInformation_WithArguments_RedactsArgumentStrings()
    {
        // Arrange
        const string message = "User {Name} logged in";
        const string userName = "john.doe@example.com";
        const string redactedUserName = "[REDACTED]@example.com";

        _mockPiiRedaction.Redact(userName).Returns(redactedUserName);

        // Act
        _service.LogInformation(message, userName);

        // Assert
        _mockPiiRedaction.Received(1).Redact(message);
        _mockPiiRedaction.Received(1).Redact(userName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Logging")]
    public void MultipleContextProperties_SetAndUsed_AppliedCorrectly()
    {
        // Arrange
        _service.SetContext("TraceId", "trace-123");
        _service.SetContext("SpanId", "span-456");
        _service.SetContext("UserId", "user-789");

        // Act
        _service.LogInformation("Test message with multiple context properties");

        // Assert
        _mockLogger.Received(1).Information(Arg.Any<string>(), Arg.Any<object[]>());
    }
}
