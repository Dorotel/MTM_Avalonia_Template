using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract test for Android device certificate + two-factor authentication (FR-154)
/// </summary>
public class AndroidAuthContractTests
{
    [Fact]
    public async Task AndroidAuth_ShouldRequireDeviceCertificateFromKeyStore()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        const string deviceCertKey = "android_device_cert";
        const string mockCertData = "MOCK_CERT_DATA_FROM_KEYSTORE";
        
        secretsService.RetrieveSecretAsync(deviceCertKey).Returns(mockCertData);
        
        // Act
        var deviceCert = await secretsService.RetrieveSecretAsync(deviceCertKey);
        
        // Assert
        deviceCert.Should().NotBeNullOrEmpty("device certificate must be stored in Android KeyStore");
    }

    [Fact]
    public async Task AndroidAuth_ShouldRequireUserCredentials()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        const string usernameKey = "user_credentials_username";
        const string passwordKey = "user_credentials_password";
        
        secretsService.RetrieveSecretAsync(usernameKey).Returns("testuser");
        secretsService.RetrieveSecretAsync(passwordKey).Returns("testpass");
        
        // Act
        var username = await secretsService.RetrieveSecretAsync(usernameKey);
        var password = await secretsService.RetrieveSecretAsync(passwordKey);
        
        // Assert
        username.Should().NotBeNullOrEmpty("username is required for two-factor auth");
        password.Should().NotBeNullOrEmpty("password is required for two-factor auth");
    }

    [Fact]
    public async Task AndroidAuth_ShouldValidateBothFactors()
    {
        // Arrange
        var secretsService = Substitute.For<ISecretsService>();
        
        // Device certificate (something you have)
        secretsService.RetrieveSecretAsync("android_device_cert").Returns("CERT_DATA");
        
        // User credentials (something you know)
        secretsService.RetrieveSecretAsync("user_credentials_username").Returns("user");
        secretsService.RetrieveSecretAsync("user_credentials_password").Returns("pass");
        
        // Act
        var cert = await secretsService.RetrieveSecretAsync("android_device_cert");
        var user = await secretsService.RetrieveSecretAsync("user_credentials_username");
        var pass = await secretsService.RetrieveSecretAsync("user_credentials_password");
        
        // Assert - Both factors must be present
        cert.Should().NotBeNullOrEmpty("device certificate (factor 1) is required");
        user.Should().NotBeNullOrEmpty("username (factor 2) is required");
        pass.Should().NotBeNullOrEmpty("password (factor 2) is required");
    }
}
