using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.ErrorHandling;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for RecoveryStrategy service
/// </summary>
public class RecoveryStrategyTests
{
    private readonly RecoveryStrategy _recoveryStrategy;
    private readonly ILogger<RecoveryStrategy> _mockLogger;
    private readonly ErrorCategorizer _errorCategorizer;

    public RecoveryStrategyTests()
    {
        _mockLogger = Substitute.For<ILogger<RecoveryStrategy>>();
        _errorCategorizer = new ErrorCategorizer();
        _recoveryStrategy = new RecoveryStrategy(_mockLogger, _errorCategorizer);
    }

    [Fact]
    public void DetermineRecoveryAction_TransientException_ShouldReturnRetry()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        // Act
        var action = _recoveryStrategy.DetermineRecoveryAction(exception);

        // Assert
        action.Should().Be(RecoveryAction.Retry);
    }

    [Fact]
    public void DetermineRecoveryAction_ConfigurationException_ShouldReturnUseDefaults()
    {
        // Arrange
        var exception = new InvalidOperationException("Configuration error");

        // Act
        var action = _recoveryStrategy.DetermineRecoveryAction(exception);

        // Assert
        action.Should().Be(RecoveryAction.UseDefaults);
    }

    [Fact]
    public void DetermineRecoveryAction_PermissionException_ShouldReturnPromptUser()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var action = _recoveryStrategy.DetermineRecoveryAction(exception);

        // Assert
        action.Should().Be(RecoveryAction.PromptUser);
    }

    [Fact]
    public void DetermineRecoveryAction_StorageException_ShouldReturnCheckStorage()
    {
        // Arrange
        var exception = new IOException("Disk error");

        // Act
        var action = _recoveryStrategy.DetermineRecoveryAction(exception);

        // Assert
        action.Should().Be(RecoveryAction.CheckStorage);
    }

    [Fact]
    public void DetermineRecoveryAction_CriticalException_ShouldReturnRestartApplication()
    {
        // Arrange
        var exception = new OutOfMemoryException();

        // Act
        var action = _recoveryStrategy.DetermineRecoveryAction(exception);

        // Assert
        action.Should().Be(RecoveryAction.RestartApplication);
    }

    [Fact]
    public async Task ExecuteRecoveryAsync_RetryAction_ShouldReturnTrue()
    {
        // Arrange
        var exception = new HttpRequestException();
        var action = RecoveryAction.Retry;

        // Act
        var result = await _recoveryStrategy.ExecuteRecoveryAsync(exception, action);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteRecoveryAsync_UseDefaultsAction_ShouldReturnTrue()
    {
        // Arrange
        var exception = new InvalidOperationException();
        var action = RecoveryAction.UseDefaults;

        // Act
        var result = await _recoveryStrategy.ExecuteRecoveryAsync(exception, action);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteRecoveryAsync_PromptUserAction_ShouldReturnFalse()
    {
        // Arrange
        var exception = new UnauthorizedAccessException();
        var action = RecoveryAction.PromptUser;

        // Act
        var result = await _recoveryStrategy.ExecuteRecoveryAsync(exception, action);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteRecoveryAsync_RestartApplicationAction_ShouldReturnFalse()
    {
        // Arrange
        var exception = new OutOfMemoryException();
        var action = RecoveryAction.RestartApplication;

        // Act
        var result = await _recoveryStrategy.ExecuteRecoveryAsync(exception, action);

        // Assert
        result.Should().BeFalse();
    }
}
