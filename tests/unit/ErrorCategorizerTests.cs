using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security;
using FluentAssertions;
using MTM_Template_Application.Services.ErrorHandling;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for ErrorCategorizer service
/// </summary>
public class ErrorCategorizerTests
{
    private readonly ErrorCategorizer _errorCategorizer;

    public ErrorCategorizerTests()
    {
        _errorCategorizer = new ErrorCategorizer();
    }

    [Fact]
    public void Categorize_HttpRequestException_ShouldReturnTransientMedium()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Transient");
        severity.Should().Be("Medium");
    }

    [Fact]
    public void Categorize_SocketException_ShouldReturnTransientMedium()
    {
        // Arrange
        var exception = new SocketException();

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Transient");
        severity.Should().Be("Medium");
    }

    [Fact]
    public void Categorize_UnauthorizedAccessException_ShouldReturnPermissionHigh()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Permission");
        severity.Should().Be("High");
    }

    [Fact]
    public void Categorize_SecurityException_ShouldReturnPermissionHigh()
    {
        // Arrange
        var exception = new SecurityException("Security violation");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Permission");
        severity.Should().Be("High");
    }

    [Fact]
    public void Categorize_IOException_ShouldReturnStorageHigh()
    {
        // Arrange
        var exception = new IOException("File not found");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Storage");
        severity.Should().Be("High");
    }

    [Fact]
    public void Categorize_NullReferenceException_ShouldReturnProgrammingCritical()
    {
        // Arrange
        var exception = new NullReferenceException("Object reference not set");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Programming");
        severity.Should().Be("Critical");
    }

    [Fact]
    public void Categorize_OutOfMemoryException_ShouldReturnResourceCritical()
    {
        // Arrange
        var exception = new OutOfMemoryException("Insufficient memory");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Resource");
        severity.Should().Be("Critical");
    }

    [Fact]
    public void Categorize_ArgumentNullException_ShouldReturnValidationLow()
    {
        // Arrange
        var exception = new ArgumentNullException("param");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Validation");
        severity.Should().Be("Low");
    }

    [Fact]
    public void Categorize_ConfigurationInvalidOperationException_ShouldReturnConfigurationHigh()
    {
        // Arrange
        var exception = new InvalidOperationException("Configuration error: missing required setting");

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Configuration");
        severity.Should().Be("High");
    }

    [Fact]
    public void Categorize_AggregateException_ShouldUnwrapAndCategorizeInner()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");
        var exception = new AggregateException(innerException);

        // Act
        var (category, severity) = _errorCategorizer.Categorize(exception);

        // Assert
        category.Should().Be("Transient");
        severity.Should().Be("Medium");
    }

    [Fact]
    public void IsTransient_TransientException_ShouldReturnTrue()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        // Act
        var result = _errorCategorizer.IsTransient(exception);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTransient_NonTransientException_ShouldReturnFalse()
    {
        // Arrange
        var exception = new NullReferenceException();

        // Act
        var result = _errorCategorizer.IsTransient(exception);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCritical_CriticalException_ShouldReturnTrue()
    {
        // Arrange
        var exception = new OutOfMemoryException();

        // Act
        var result = _errorCategorizer.IsCritical(exception);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCritical_NonCriticalException_ShouldReturnFalse()
    {
        // Arrange
        var exception = new HttpRequestException();

        // Act
        var result = _errorCategorizer.IsCritical(exception);

        // Assert
        result.Should().BeFalse();
    }
}
