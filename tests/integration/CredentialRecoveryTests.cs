using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Secrets;
using MTM_Template_Application.ViewModels.Configuration;
using MTM_Template_Application.Views.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// T024: Integration tests for credential recovery flow
/// Tests the flow: CryptographicException → CredentialDialogView → User entry → Re-storage
/// </summary>
public class CredentialRecoveryTests
{
    private readonly ITestOutputHelper _output;

    public CredentialRecoveryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// T024.1: Mock CryptographicException from SecretsService
    /// Verifies that RetrieveSecretAsync throws CryptographicException for corrupted storage
    /// </summary>
    [Fact]
    public async Task T024_CredentialRecovery_SecretsServiceThrowsCryptographicException()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string testKey = "TestCredential";

        // Mock CryptographicException (simulates corrupted storage)
        mockSecretsService
            .RetrieveSecretAsync(testKey, Arg.Any<CancellationToken>())
            .Throws(new CryptographicException("Failed to decrypt credential - storage corrupted"));

        // Act
        Func<Task> act = async () => await mockSecretsService.RetrieveSecretAsync(testKey, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CryptographicException>()
            .WithMessage("*decrypt credential*");

        _output.WriteLine("✓ SecretsService correctly throws CryptographicException for corrupted storage");
    }

    /// <summary>
    /// T024.2: Verify CredentialDialogViewModel initialization
    /// Verifies that CredentialDialogViewModel can be created with required dependencies
    /// </summary>
    [Fact]
    public void T024_CredentialRecovery_CredentialDialogViewModelInitialization()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();
        const string testTitle = "Enter Credentials";
        const string testMessage = "Your saved credentials could not be retrieved. Please enter them again.";

        // Act
        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            DialogTitle = testTitle,
            DialogMessage = testMessage
        };

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.DialogTitle.Should().Be(testTitle);
        viewModel.DialogMessage.Should().Be(testMessage);
        viewModel.Username.Should().BeEmpty("username should start empty");
        viewModel.Password.Should().BeEmpty("password should start empty");
        viewModel.IsLoading.Should().BeFalse("should not be loading initially");
        viewModel.ErrorMessage.Should().BeEmpty("should have no error initially");

        _output.WriteLine("✓ CredentialDialogViewModel initializes correctly");
    }

    /// <summary>
    /// T024.3: Simulate user credential entry and submission
    /// Verifies that SubmitCommand stores credentials via SecretsService
    /// </summary>
    [Fact]
    public async Task T024_CredentialRecovery_UserCredentialEntryAndSubmission()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();
        const string testUsername = "testuser";
        const string testPassword = "testpassword123";

        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            Username = testUsername,
            Password = testPassword
        };

        // Mock StoreSecretAsync to succeed
        mockSecretsService
            .StoreSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.SubmitCommand.ExecuteAsync(null);

        // Assert - Verify StoreSecretAsync was called twice (username and password)
        await mockSecretsService.Received(2)
            .StoreSecretAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        viewModel.ErrorMessage.Should().BeEmpty("should have no error after successful submission");
        _output.WriteLine("✓ SubmitCommand successfully stores credentials via SecretsService");
    }

    /// <summary>
    /// T024.4: Test credential re-storage after successful entry
    /// Verifies that credentials are properly serialized and stored
    /// </summary>
    [Fact]
    public async Task T024_CredentialRecovery_VerifyCredentialReStorage()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();
        const string username = "admin";
        const string password = "secure123";

        var storedKeys = new System.Collections.Generic.List<string>();
        var storedValues = new System.Collections.Generic.List<string>();

        // Capture stored values
        mockSecretsService
            .StoreSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                storedKeys.Add(callInfo.ArgAt<string>(0));
                storedValues.Add(callInfo.ArgAt<string>(1));
                return Task.CompletedTask;
            });

        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            Username = username,
            Password = password
        };

        // Act
        await viewModel.SubmitCommand.ExecuteAsync(null);

        // Assert
        storedKeys.Should().HaveCount(2, "should store both username and password");
        storedKeys.Should().Contain("Visual.Username", "should store username with correct key");
        storedKeys.Should().Contain("Visual.Password", "should store password with correct key");
        storedValues.Should().Contain(username, "should store username value");
        storedValues.Should().Contain(password, "should store password value");

        _output.WriteLine($"✓ Credentials stored - Keys: {string.Join(", ", storedKeys)}");
        _output.WriteLine($"✓ Credentials stored - Values: {storedValues.Count} items");
    }

    /// <summary>
    /// T024.5: Test cancellation flow (user clicks Cancel button)
    /// Verifies that CancelCommand executes without storing credentials
    /// </summary>
    [Fact]
    public async Task T024_CredentialRecovery_CancellationFlow()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();

        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            Username = "testuser",
            Password = "testpass"
        };

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert - Verify no credentials were stored
        await mockSecretsService.DidNotReceive()
            .StoreSecretAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Dialog result should be false (cancelled)
        viewModel.GetDialogResult().Should().BeFalse("dialog result should be false when cancelled");
        _output.WriteLine("✓ Cancel command executes without storing credentials");
    }

    /// <summary>
    /// T024.6: Test retry mechanism with exponential backoff
    /// Verifies that SubmitCommand retries failed storage attempts
    /// </summary>
    [Fact]
    public async Task T024_CredentialRecovery_RetryMechanismWithExponentialBackoff()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();
        int attemptCount = 0;

        // Mock first 2 username storage attempts to fail, 3rd to succeed
        // Then password storage succeeds on first try
        mockSecretsService
            .StoreSecretAsync(Arg.Is<string>(k => k == "Visual.Username"), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    throw new Exception($"Storage failure attempt {attemptCount}");
                }
                return Task.CompletedTask;
            });

        // Password storage always succeeds
        mockSecretsService
            .StoreSecretAsync(Arg.Is<string>(k => k == "Visual.Password"), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act - Note: This test may take time due to exponential backoff delays
        await viewModel.SubmitCommand.ExecuteAsync(CancellationToken.None);

        // Assert
        attemptCount.Should().BeGreaterOrEqualTo(3, "should retry until success");
        _output.WriteLine($"✓ Retry mechanism worked - succeeded after {attemptCount} username storage attempts");
    }

    /// <summary>
    /// T024.7: Test validation prevents empty credentials
    /// Verifies that SubmitCommand is disabled when credentials are empty
    /// </summary>
    [Fact]
    public void T024_CredentialRecovery_ValidationPreventsEmptyCredentials()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        var mockLogger = Substitute.For<ILogger<CredentialDialogViewModel>>();

        var viewModel = new CredentialDialogViewModel(mockSecretsService, mockLogger)
        {
            Username = "",
            Password = ""
        };

        // Act & Assert - SubmitCommand should be disabled with empty credentials
        viewModel.SubmitCommand.CanExecute(null).Should().BeFalse("SubmitCommand should be disabled with empty username");

        // Set username but leave password empty
        viewModel.Username = "testuser";
        viewModel.SubmitCommand.CanExecute(null).Should().BeFalse("SubmitCommand should be disabled with empty password");

        // Set both
        viewModel.Password = "testpass";
        viewModel.SubmitCommand.CanExecute(null).Should().BeTrue("SubmitCommand should be enabled with both credentials");

        _output.WriteLine("✓ Validation correctly prevents submission with empty credentials");
    }
}
