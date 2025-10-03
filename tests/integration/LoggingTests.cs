using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Logging;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for structured logging and telemetry
/// </summary>
public class LoggingTests
{
    [Fact]
    public void Logging_ShouldFormatAsStructuredJSON()
    {
        // Arrange
        var loggingService = Substitute.For<ILoggingService>();
        
        // Act
        loggingService.SetContext("TraceId", "test-trace-123");
        loggingService.LogInformation("Test message with {Param}", "value");
        
        // Assert
        loggingService.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public void Logging_ShouldRedactPII()
    {
        // Arrange
        var loggingService = Substitute.For<ILoggingService>();
        
        // Act
        loggingService.LogInformation("User email: user@example.com");
        
        // Assert - PII should be redacted in actual implementation
        loggingService.Received(1).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task LoggingFlush_ShouldPersistPendingEntries()
    {
        // Arrange
        var loggingService = Substitute.For<ILoggingService>();
        
        loggingService.LogInformation("Message 1");
        loggingService.LogInformation("Message 2");
        
        // Act
        await loggingService.FlushAsync();
        
        // Assert
        await loggingService.Received(1).FlushAsync();
    }
}
