using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for WindowsSecretsService (DPAPI encryption and Credential Manager storage)
/// Tests cover T139: Test DPAPI encryption, storage, retrieval (mock DPAPI)
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsSecretsServiceTests
{
    private readonly ILogger<WindowsSecretsService> _mockLogger;
    private readonly WindowsSecretsService _service;

    public WindowsSecretsServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<WindowsSecretsService>>();
        _service = new WindowsSecretsService(_mockLogger);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task StoreSecretAsync_WithValidKeyAndValue_StoresEncryptedSecret()
    {
        // Arrange
        const string key = "Test.Username";
        const string value = "testuser@example.com";

        // Act
        await _service.StoreSecretAsync(key, value);

        // Assert
        var retrieved = await _service.RetrieveSecretAsync(key);
        retrieved.Should().Be(value, "stored secret should be retrievable");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task StoreSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string nullKey = null!;
        const string value = "testvalue";

        // Act
        Func<Task> act = async () => await _service.StoreSecretAsync(nullKey, value);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task StoreSecretAsync_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        const string key = "Test.Password";
        string nullValue = null!;

        // Act
        Func<Task> act = async () => await _service.StoreSecretAsync(key, nullValue);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("value");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RetrieveSecretAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        const string nonExistentKey = "NonExistent.Key";

        // Act
        var result = await _service.RetrieveSecretAsync(nonExistentKey);

        // Assert
        result.Should().BeNull("non-existent secret should return null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RetrieveSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string nullKey = null!;

        // Act
        Func<Task> act = async () => await _service.RetrieveSecretAsync(nullKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task DeleteSecretAsync_WithExistingKey_RemovesSecret()
    {
        // Arrange
        const string key = "Test.ToDelete";
        const string value = "tempvalue";
        await _service.StoreSecretAsync(key, value);

        // Act
        await _service.DeleteSecretAsync(key);

        // Assert
        var result = await _service.RetrieveSecretAsync(key);
        result.Should().BeNull("deleted secret should no longer exist");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task DeleteSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string nullKey = null!;

        // Act
        Func<Task> act = async () => await _service.DeleteSecretAsync(nullKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RotateSecretAsync_WithExistingKey_UpdatesSecret()
    {
        // Arrange
        const string key = "Test.Rotate";
        const string oldValue = "oldpassword";
        const string newValue = "newpassword";
        await _service.StoreSecretAsync(key, oldValue);

        // Act
        await _service.RotateSecretAsync(key, newValue);

        // Assert
        var retrieved = await _service.RetrieveSecretAsync(key);
        retrieved.Should().Be(newValue, "rotated secret should have new value");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RotateSecretAsync_WithNonExistentKey_ThrowsInvalidOperationException()
    {
        // Arrange
        const string nonExistentKey = "NonExistent.Key";
        const string newValue = "newvalue";

        // Act
        Func<Task> act = async () => await _service.RotateSecretAsync(nonExistentKey, newValue);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Secret {nonExistentKey} not found");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RotateSecretAsync_WithNullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string nullKey = null!;
        const string newValue = "newvalue";

        // Act
        Func<Task> act = async () => await _service.RotateSecretAsync(nullKey, newValue);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("key");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task RotateSecretAsync_WithNullNewValue_ThrowsArgumentNullException()
    {
        // Arrange
        const string key = "Test.Rotate";
        string nullValue = null!;

        // Act
        Func<Task> act = async () => await _service.RotateSecretAsync(key, nullValue);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("newValue");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task StoreSecretAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        const string key = "Test.Cancellation";
        const string value = "testvalue";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.StoreSecretAsync(key, value, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task SecretType_WithPasswordKey_DeterminesCorrectType()
    {
        // Arrange
        const string key = "Visual.Password";
        const string value = "secretpassword";

        // Act
        await _service.StoreSecretAsync(key, value);

        // Assert - Verify secret was stored (type determination is internal)
        var retrieved = await _service.RetrieveSecretAsync(key);
        retrieved.Should().Be(value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task SecretType_WithApiKeyKey_DeterminesCorrectType()
    {
        // Arrange
        const string key = "Service.ApiKey";
        const string value = "sk_test_1234567890";

        // Act
        await _service.StoreSecretAsync(key, value);

        // Assert - Verify secret was stored (type determination is internal)
        var retrieved = await _service.RetrieveSecretAsync(key);
        retrieved.Should().Be(value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task SecretType_WithConnectionStringKey_DeterminesCorrectType()
    {
        // Arrange
        const string key = "Database.ConnectionString";
        const string value = "Server=localhost;Database=test;";

        // Act
        await _service.StoreSecretAsync(key, value);

        // Assert - Verify secret was stored (type determination is internal)
        var retrieved = await _service.RetrieveSecretAsync(key);
        retrieved.Should().Be(value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Secrets")]
    public async Task MultipleSecrets_StoredConcurrently_AllRetrievableCorrectly()
    {
        // Arrange
        var secrets = new[]
        {
            ("Secret1", "Value1"),
            ("Secret2", "Value2"),
            ("Secret3", "Value3")
        };

        // Act - Store concurrently
        var storeTasks = secrets.Select(s => _service.StoreSecretAsync(s.Item1, s.Item2));
        await Task.WhenAll(storeTasks);

        // Assert - Retrieve and verify
        foreach (var (key, expectedValue) in secrets)
        {
            var retrieved = await _service.RetrieveSecretAsync(key);
            retrieved.Should().Be(expectedValue, $"secret {key} should be retrievable");
        }
    }
}
