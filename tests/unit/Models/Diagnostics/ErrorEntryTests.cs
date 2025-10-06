using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.unit.Models.Diagnostics;

public class ErrorEntryTests
{
    [Fact]
    public void ValidErrorEntry_ShouldPassValidation()
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Database",
            Message = "Connection failed",
            StackTrace = "at System.Data...",
            RecoverySuggestion = "Check database connection string",
            ContextData = new Dictionary<string, string>
            {
                { "ConnectionString", "Server=localhost" },
                { "Operation", "Connect" }
            }
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyGuid_ShouldFailValidation()
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.Empty,
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Database",
            Message = "Connection failed",
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InvalidCategory_ShouldFailValidation(string? category)
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = category!,
            Message = "Connection failed",
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InvalidMessage_ShouldFailValidation(string? message)
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Database",
            Message = message!,
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void NullContextData_ShouldFailValidation()
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Database",
            Message = "Connection failed",
            ContextData = null!
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyContextData_ShouldPassValidation()
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Database",
            Message = "Connection failed",
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeTrue();
        entry.ContextData.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ErrorSeverity.Info)]
    [InlineData(ErrorSeverity.Warning)]
    [InlineData(ErrorSeverity.Error)]
    [InlineData(ErrorSeverity.Critical)]
    public void AllSeverityLevels_ShouldBeSupported(ErrorSeverity severity)
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            Category = "Test",
            Message = "Test message",
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeTrue();
        entry.Severity.Should().Be(severity);
    }

    [Fact]
    public void OptionalFields_CanBeNull()
    {
        // Arrange
        var entry = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Warning,
            Category = "Validation",
            Message = "Input validation failed",
            StackTrace = null,
            RecoverySuggestion = null,
            ContextData = new Dictionary<string, string>()
        };

        // Act
        var isValid = entry.IsValid();

        // Assert
        isValid.Should().BeTrue();
        entry.StackTrace.Should().BeNull();
        entry.RecoverySuggestion.Should().BeNull();
    }

    [Fact]
    public void UniqueIds_ShouldBeDifferent()
    {
        // Arrange
        var entry1 = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Test",
            Message = "Test 1",
            ContextData = new Dictionary<string, string>()
        };

        var entry2 = new ErrorEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Severity = ErrorSeverity.Error,
            Category = "Test",
            Message = "Test 2",
            ContextData = new Dictionary<string, string>()
        };

        // Assert
        entry1.Id.Should().NotBe(entry2.Id);
    }
}
