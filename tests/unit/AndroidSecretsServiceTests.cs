using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Secrets;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for AndroidSecretsService (T141)
/// Tests Android KeyStore integration for credential storage
/// </summary>
[SupportedOSPlatform("android")]
public class AndroidSecretsServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new AndroidSecretsService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region StoreSecretAsync Tests

    [Fact]
    public async Task StoreSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.StoreSecretAsync(null!, "value");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task StoreSecretAsync_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.StoreSecretAsync("test-key", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Fact]
    public async Task StoreSecretAsync_WithValidCredentials_StoresSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var key = "android-test-key";
        var value = "test-password-123";

        // Act
        await service.StoreSecretAsync(key, value);
        var retrieved = await service.RetrieveSecretAsync(key);

        // Assert
        retrieved.Should().Be(value);
    }

    [Fact]
    public async Task StoreSecretAsync_OverwritingExistingKey_UpdatesValue()
    {
        // Arrange
        var service = CreateService();
        var key = "android-overwrite-key";
        await service.StoreSecretAsync(key, "old-value");

        // Act
        await service.StoreSecretAsync(key, "new-value");
        var retrieved = await service.RetrieveSecretAsync(key);

        // Assert
        retrieved.Should().Be("new-value");
    }

    [Fact]
    public async Task StoreSecretAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = CreateService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await service.StoreSecretAsync("test-key", "value", cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region RetrieveSecretAsync Tests

    [Fact]
    public async Task RetrieveSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.RetrieveSecretAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task RetrieveSecretAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RetrieveSecretAsync("non-existent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RetrieveSecretAsync_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var service = CreateService();
        var key = "android-retrieve-key";
        var value = "retrieve-test-value";
        await service.StoreSecretAsync(key, value);

        // Act
        var retrieved = await service.RetrieveSecretAsync(key);

        // Assert
        retrieved.Should().Be(value);
    }

    #endregion

    #region DeleteSecretAsync Tests

    [Fact]
    public async Task DeleteSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.DeleteSecretAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task DeleteSecretAsync_WithExistingKey_RemovesSecret()
    {
        // Arrange
        var service = CreateService();
        var key = "android-delete-key";
        await service.StoreSecretAsync(key, "value-to-delete");

        // Act
        await service.DeleteSecretAsync(key);
        var retrieved = await service.RetrieveSecretAsync(key);

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSecretAsync_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.DeleteSecretAsync("non-existent-key");
        // Should complete without throwing

        // Assert - No exception means success
    }

    #endregion

    #region RotateSecretAsync Tests

    [Fact]
    public async Task RotateSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.RotateSecretAsync(null!, "new-value");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    public async Task RotateSecretAsync_WithNullNewValue_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.RotateSecretAsync("test-key", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("newValue");
    }

    [Fact]
    public async Task RotateSecretAsync_WithExistingKey_UpdatesValue()
    {
        // Arrange
        var service = CreateService();
        var key = "android-rotate-key";
        await service.StoreSecretAsync(key, "old-value");

        // Act
        await service.RotateSecretAsync(key, "rotated-value");
        var retrieved = await service.RetrieveSecretAsync(key);

        // Assert
        retrieved.Should().Be("rotated-value");
    }

    #endregion

    #region Helper Methods

    private static AndroidSecretsService CreateService()
    {
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AndroidSecretsService>.Instance;
        return new AndroidSecretsService(logger);
    }

    #endregion
}
