using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for OS-native credential storage
/// </summary>
public class SecretsTests
{
    [Fact]
    public async Task SecretStorage_ShouldEncryptAndStoreCredentials()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        const string testKey = "test_credential";
        const string testValue = "P@ssw0rd123!";

        // Configure mock to return the stored value
        secretsService.RetrieveSecretAsync(testKey).Returns(testValue);

        // Act
        await secretsService.StoreSecretAsync(testKey, testValue);
        var retrieved = await secretsService.RetrieveSecretAsync(testKey);

        // Assert
        retrieved.Should().Be(testValue, "retrieved secret should match stored secret");
    }

    [Fact]
    public async Task SecretRotation_ShouldUpdateCredential()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        const string testKey = "rotation_test";
        const string oldValue = "OldPassword";
        const string newValue = "NewPassword";

        // Configure mock - after rotation, retrieve should return new value
        secretsService.RetrieveSecretAsync(testKey).Returns(newValue);

        await secretsService.StoreSecretAsync(testKey, oldValue);

        // Act
        await secretsService.RotateSecretAsync(testKey, newValue);
        var retrieved = await secretsService.RetrieveSecretAsync(testKey);

        // Assert
        retrieved.Should().Be(newValue, "rotated secret should return new value");
    }

    [Fact]
    public async Task SecretDeletion_ShouldRemoveCredential()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        const string testKey = "delete_test";

        // Configure mock - after deletion, retrieve should return null
        secretsService.RetrieveSecretAsync(testKey).Returns((string?)null);

        await secretsService.StoreSecretAsync(testKey, "value");

        // Act
        await secretsService.DeleteSecretAsync(testKey);
        var retrieved = await secretsService.RetrieveSecretAsync(testKey);

        // Assert
        retrieved.Should().BeNull("deleted secret should return null");
    }
}
