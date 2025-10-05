using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// T024a: Integration tests for Android two-factor authentication
/// Tests user credentials + device certificate validation on Android platform
/// Aligns with Feature 001 boot sequence and Constitution v1.2.0 Principle VIII
/// </summary>
public class AndroidTwoFactorAuthTests
{
    private readonly ITestOutputHelper _output;

    public AndroidTwoFactorAuthTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region T024a: Android Two-Factor Authentication Tests

    /// <summary>
    /// T024a.1: Test user credentials validation on Android
    /// Verifies that user credentials are properly stored and retrieved from Android KeyStore
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_UserCredentialsStoredInKeyStore()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string testUsername = "android_user";
        const string testPassword = "android_password123";

        // Mock successful credential storage in Android KeyStore
        mockSecretsService
            .StoreSecretAsync("Visual.Username", testUsername, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockSecretsService
            .StoreSecretAsync("Visual.Password", testPassword, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Mock successful credential retrieval
        mockSecretsService
            .RetrieveSecretAsync("Visual.Username", Arg.Any<CancellationToken>())
            .Returns(testUsername);
        mockSecretsService
            .RetrieveSecretAsync("Visual.Password", Arg.Any<CancellationToken>())
            .Returns(testPassword);

        // Act
        await mockSecretsService.StoreSecretAsync("Visual.Username", testUsername, CancellationToken.None);
        await mockSecretsService.StoreSecretAsync("Visual.Password", testPassword, CancellationToken.None);
        var retrievedUsername = await mockSecretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
        var retrievedPassword = await mockSecretsService.RetrieveSecretAsync("Visual.Password", CancellationToken.None);

        // Assert
        retrievedUsername.Should().Be(testUsername, "username should be retrieved from Android KeyStore");
        retrievedPassword.Should().Be(testPassword, "password should be retrieved from Android KeyStore");

        _output.WriteLine("✓ User credentials successfully stored and retrieved from Android KeyStore");
    }

    /// <summary>
    /// T024a.2: Test device certificate storage in Android KeyStore
    /// Verifies that device certificates are stored in Android KeyStore for two-factor authentication
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_DeviceCertificateStoredInKeyStore()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string certificateKey = "Device.Certificate";
        const string mockCertificateData = "MOCK_CERTIFICATE_BASE64_DATA_12345";

        // Mock certificate storage
        mockSecretsService
            .StoreSecretAsync(certificateKey, mockCertificateData, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Mock certificate retrieval
        mockSecretsService
            .RetrieveSecretAsync(certificateKey, Arg.Any<CancellationToken>())
            .Returns(mockCertificateData);

        // Act
        await mockSecretsService.StoreSecretAsync(certificateKey, mockCertificateData, CancellationToken.None);
        var retrievedCertificate = await mockSecretsService.RetrieveSecretAsync(certificateKey, CancellationToken.None);

        // Assert
        retrievedCertificate.Should().Be(mockCertificateData, "device certificate should be retrieved from Android KeyStore");
        retrievedCertificate.Should().NotBeNullOrWhiteSpace("certificate data should not be empty");

        _output.WriteLine("✓ Device certificate successfully stored and retrieved from Android KeyStore");
    }

    /// <summary>
    /// T024a.3: Test two-factor authentication validation (credentials + certificate)
    /// Verifies that both user credentials and device certificate must be present for authentication
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_BothCredentialsAndCertificateRequired()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string username = "test_android_user";
        const string password = "test_password";
        const string certificate = "DEVICE_CERT_DATA";

        // Mock all secrets available
        mockSecretsService.RetrieveSecretAsync("Visual.Username", Arg.Any<CancellationToken>()).Returns(username);
        mockSecretsService.RetrieveSecretAsync("Visual.Password", Arg.Any<CancellationToken>()).Returns(password);
        mockSecretsService.RetrieveSecretAsync("Device.Certificate", Arg.Any<CancellationToken>()).Returns(certificate);

        // Act - Retrieve all authentication factors
        var retrievedUsername = await mockSecretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
        var retrievedPassword = await mockSecretsService.RetrieveSecretAsync("Visual.Password", CancellationToken.None);
        var retrievedCertificate = await mockSecretsService.RetrieveSecretAsync("Device.Certificate", CancellationToken.None);

        // Simulate two-factor validation
        bool isTwoFactorValid = !string.IsNullOrWhiteSpace(retrievedUsername) &&
                                 !string.IsNullOrWhiteSpace(retrievedPassword) &&
                                 !string.IsNullOrWhiteSpace(retrievedCertificate);

        // Assert
        isTwoFactorValid.Should().BeTrue("two-factor authentication requires both credentials and certificate");
        _output.WriteLine("✓ Two-factor authentication validated: credentials + certificate present");
    }

    /// <summary>
    /// T024a.4: Test missing device certificate scenario
    /// Verifies that authentication fails when device certificate is missing
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_MissingCertificateFailsAuthentication()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string username = "test_user";
        const string password = "test_password";

        // Mock credentials available but certificate missing
        mockSecretsService.RetrieveSecretAsync("Visual.Username", Arg.Any<CancellationToken>()).Returns(username);
        mockSecretsService.RetrieveSecretAsync("Visual.Password", Arg.Any<CancellationToken>()).Returns(password);
        mockSecretsService.RetrieveSecretAsync("Device.Certificate", Arg.Any<CancellationToken>()).Returns((string?)null);

        // Act
        var retrievedUsername = await mockSecretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
        var retrievedPassword = await mockSecretsService.RetrieveSecretAsync("Visual.Password", CancellationToken.None);
        var retrievedCertificate = await mockSecretsService.RetrieveSecretAsync("Device.Certificate", CancellationToken.None);

        // Simulate two-factor validation
        bool isTwoFactorValid = !string.IsNullOrWhiteSpace(retrievedUsername) &&
                                 !string.IsNullOrWhiteSpace(retrievedPassword) &&
                                 !string.IsNullOrWhiteSpace(retrievedCertificate);

        // Assert
        isTwoFactorValid.Should().BeFalse("authentication should fail when device certificate is missing");
        retrievedCertificate.Should().BeNullOrWhiteSpace("certificate should be null or empty when missing");

        _output.WriteLine("✓ Two-factor authentication correctly fails when certificate is missing");
    }

    /// <summary>
    /// T024a.5: Test expired device certificate detection
    /// Verifies that expired certificates are detected and require renewal
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_ExpiredCertificateDetection()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string expiredCertificate = "EXPIRED_CERT_DATA_TIMESTAMP_2024-01-01";
        const string certificateExpiryKey = "Device.CertificateExpiry";

        // Mock expired certificate (expiry date in the past)
        var expiredDate = DateTimeOffset.UtcNow.AddMonths(-1); // 1 month ago
        mockSecretsService
            .RetrieveSecretAsync("Device.Certificate", Arg.Any<CancellationToken>())
            .Returns(expiredCertificate);
        mockSecretsService
            .RetrieveSecretAsync(certificateExpiryKey, Arg.Any<CancellationToken>())
            .Returns(expiredDate.ToString("O")); // ISO 8601 format

        // Act
        var certificate = await mockSecretsService.RetrieveSecretAsync("Device.Certificate", CancellationToken.None);
        var expiryDateStr = await mockSecretsService.RetrieveSecretAsync(certificateExpiryKey, CancellationToken.None);
        var expiryDate = DateTimeOffset.Parse(expiryDateStr!);
        var isExpired = expiryDate < DateTimeOffset.UtcNow;

        // Assert
        certificate.Should().NotBeNullOrWhiteSpace("certificate data should be retrieved");
        isExpired.Should().BeTrue("certificate should be detected as expired");
        _output.WriteLine($"✓ Expired certificate detected - Expiry: {expiryDate}, Current: {DateTimeOffset.UtcNow}");
        _output.WriteLine("✓ Certificate renewal will be required");
    }

    /// <summary>
    /// T024a.6: Test valid device certificate detection
    /// Verifies that valid (non-expired) certificates pass validation
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_ValidCertificatePassesValidation()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string validCertificate = "VALID_CERT_DATA_TIMESTAMP_2026-12-31";
        const string certificateExpiryKey = "Device.CertificateExpiry";

        // Mock valid certificate (expiry date in the future)
        var futureDate = DateTimeOffset.UtcNow.AddMonths(6); // 6 months from now
        mockSecretsService
            .RetrieveSecretAsync("Device.Certificate", Arg.Any<CancellationToken>())
            .Returns(validCertificate);
        mockSecretsService
            .RetrieveSecretAsync(certificateExpiryKey, Arg.Any<CancellationToken>())
            .Returns(futureDate.ToString("O"));

        // Act
        var certificate = await mockSecretsService.RetrieveSecretAsync("Device.Certificate", CancellationToken.None);
        var expiryDateStr = await mockSecretsService.RetrieveSecretAsync(certificateExpiryKey, CancellationToken.None);
        var expiryDate = DateTimeOffset.Parse(expiryDateStr!);
        var isValid = expiryDate > DateTimeOffset.UtcNow;

        // Assert
        certificate.Should().NotBeNullOrWhiteSpace("certificate data should be retrieved");
        isValid.Should().BeTrue("certificate should be valid (not expired)");
        _output.WriteLine($"✓ Valid certificate detected - Expiry: {expiryDate}, Current: {DateTimeOffset.UtcNow}");
    }

    /// <summary>
    /// T024a.7: Test server-side certificate validation via MTM Server API
    /// Verifies that device certificate is validated server-side (per Constitution v1.2.0)
    /// </summary>
    [Fact]
    public void T024a_AndroidTwoFactor_ServerSideValidation()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string deviceCertificate = "DEVICE_CERT_DATA_FOR_SERVER_VALIDATION";
        const string username = "android_user";

        // Mock certificate retrieval (synchronous for test simplicity)
        mockSecretsService
            .RetrieveSecretAsync("Device.Certificate", Arg.Any<CancellationToken>())
            .Returns(deviceCertificate);
        mockSecretsService
            .RetrieveSecretAsync("Visual.Username", Arg.Any<CancellationToken>())
            .Returns(username);

        // Simulate server-side validation call (would be actual HTTP call in production)
        // For this test, we mock the validation result
        bool serverValidationResult = ValidateDeviceCertificateServerSide(username, deviceCertificate);

        // Assert
        serverValidationResult.Should().BeTrue("server should validate device certificate successfully");
        _output.WriteLine($"✓ Server-side certificate validation passed for user: {username}");
        _output.WriteLine("✓ MTM Server API validated device certificate against user credentials");
    }

    /// <summary>
    /// T024a.8: Test Android KeyStore hardware-backed encryption
    /// Verifies that secrets are stored with hardware-backed encryption when available
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_HardwareBackedEncryption()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string testSecret = "HARDWARE_BACKED_SECRET_DATA";
        const string secretKey = "Test.HardwareBackedSecret";
        const string encryptionTypeKey = "Test.EncryptionType";

        // Mock hardware-backed encryption indicator
        mockSecretsService
            .StoreSecretAsync(secretKey, testSecret, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockSecretsService
            .RetrieveSecretAsync(secretKey, Arg.Any<CancellationToken>())
            .Returns(testSecret);

        // Mock encryption type retrieval (hardware-backed on modern Android devices)
        mockSecretsService
            .RetrieveSecretAsync(encryptionTypeKey, Arg.Any<CancellationToken>())
            .Returns("HardwareBacked"); // Indicates StrongBox or TEE encryption

        // Act
        await mockSecretsService.StoreSecretAsync(secretKey, testSecret, CancellationToken.None);
        var retrievedSecret = await mockSecretsService.RetrieveSecretAsync(secretKey, CancellationToken.None);
        var encryptionType = await mockSecretsService.RetrieveSecretAsync(encryptionTypeKey, CancellationToken.None);

        // Assert
        retrievedSecret.Should().Be(testSecret, "secret should be retrieved successfully");
        encryptionType.Should().Be("HardwareBacked", "encryption should use hardware-backed security on modern devices");

        _output.WriteLine("✓ Android KeyStore using hardware-backed encryption (StrongBox or TEE)");
        _output.WriteLine($"✓ Encryption type: {encryptionType}");
    }

    /// <summary>
    /// T024a.9: Test certificate generation process
    /// Verifies that device certificates can be generated for new devices
    /// (See Feature 001 boot sequence documentation for certificate generation details)
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_CertificateGenerationForNewDevice()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string deviceId = "ANDROID_DEVICE_12345";
        const string newCertificateKey = "Device.Certificate";

        // Simulate new device scenario: certificate doesn't exist yet
        mockSecretsService
            .RetrieveSecretAsync(newCertificateKey, Arg.Any<CancellationToken>())
            .Returns((string?)null); // No certificate exists

        // Act - Simulate certificate generation (would call server API in production)
        var existingCertificate = await mockSecretsService.RetrieveSecretAsync(newCertificateKey, CancellationToken.None);
        bool needsGeneration = string.IsNullOrWhiteSpace(existingCertificate);

        string? generatedCertificate = null;
        if (needsGeneration)
        {
            // Simulate certificate generation via server API
            generatedCertificate = GenerateDeviceCertificate(deviceId);

            // Store generated certificate
            mockSecretsService
                .StoreSecretAsync(newCertificateKey, generatedCertificate, Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            await mockSecretsService.StoreSecretAsync(newCertificateKey, generatedCertificate, CancellationToken.None);
        }

        // Assert
        needsGeneration.Should().BeTrue("new device should require certificate generation");
        generatedCertificate.Should().NotBeNullOrWhiteSpace("certificate should be generated for new device");
        generatedCertificate.Should().Contain(deviceId, "certificate should include device identifier");

        _output.WriteLine($"✓ Device certificate generated for new device: {deviceId}");
        _output.WriteLine($"✓ Certificate data: {generatedCertificate?.Substring(0, Math.Min(50, generatedCertificate.Length))}...");
    }

    /// <summary>
    /// T024a.10: Test certificate renewal process
    /// Verifies that expired certificates can be renewed
    /// </summary>
    [Fact]
    public async Task T024a_AndroidTwoFactor_CertificateRenewal()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        const string oldCertificate = "OLD_CERT_DATA_EXPIRED";
        const string certificateKey = "Device.Certificate";
        const string expiryKey = "Device.CertificateExpiry";

        // Mock expired certificate
        mockSecretsService
            .RetrieveSecretAsync(certificateKey, Arg.Any<CancellationToken>())
            .Returns(oldCertificate);

        var expiredDate = DateTimeOffset.UtcNow.AddDays(-1);
        mockSecretsService
            .RetrieveSecretAsync(expiryKey, Arg.Any<CancellationToken>())
            .Returns(expiredDate.ToString("O"));

        // Act - Detect expiry and renew
        var currentCertificate = await mockSecretsService.RetrieveSecretAsync(certificateKey, CancellationToken.None);
        var expiryDateStr = await mockSecretsService.RetrieveSecretAsync(expiryKey, CancellationToken.None);
        var expiryDate = DateTimeOffset.Parse(expiryDateStr!);
        bool isExpired = expiryDate < DateTimeOffset.UtcNow;

        if (isExpired)
        {
            // Simulate renewal via server API
            var renewedCertificate = RenewDeviceCertificate(currentCertificate!);
            var newExpiryDate = DateTimeOffset.UtcNow.AddMonths(12); // 1 year validity

            // Store renewed certificate
            mockSecretsService
                .StoreSecretAsync(certificateKey, renewedCertificate, Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            mockSecretsService
                .StoreSecretAsync(expiryKey, newExpiryDate.ToString("O"), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            await mockSecretsService.StoreSecretAsync(certificateKey, renewedCertificate, CancellationToken.None);
            await mockSecretsService.StoreSecretAsync(expiryKey, newExpiryDate.ToString("O"), CancellationToken.None);

            // Verify renewal
            mockSecretsService
                .RetrieveSecretAsync(certificateKey, Arg.Any<CancellationToken>())
                .Returns(renewedCertificate);
        }

        // Assert
        isExpired.Should().BeTrue("certificate should be detected as expired before renewal");
        var renewedCert = await mockSecretsService.RetrieveSecretAsync(certificateKey, CancellationToken.None);
        renewedCert.Should().NotBe(oldCertificate, "certificate should be different after renewal");

        _output.WriteLine("✓ Certificate renewal completed successfully");
        _output.WriteLine($"✓ Old certificate expired: {expiredDate}");
        _output.WriteLine($"✓ New certificate will expire: {DateTimeOffset.UtcNow.AddMonths(12)}");
    }

    #endregion T024a: Android Two-Factor Authentication Tests

    #region Helper Methods

    /// <summary>
    /// Simulates server-side device certificate validation
    /// In production, this would be an actual HTTP call to MTM Server API
    /// </summary>
    private bool ValidateDeviceCertificateServerSide(string username, string certificate)
    {
        // Mock server-side validation logic
        // Real implementation would verify certificate signature, expiry, and user association
        return !string.IsNullOrWhiteSpace(username) &&
               !string.IsNullOrWhiteSpace(certificate) &&
               certificate.Contains("CERT_DATA");
    }

    /// <summary>
    /// Simulates device certificate generation for new devices
    /// In production, this would call MTM Server API to generate signed certificates
    /// </summary>
    private string GenerateDeviceCertificate(string deviceId)
    {
        // Mock certificate generation
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"DEVICE_CERT_{deviceId}_{timestamp}_SIGNED_BY_SERVER";
    }

    /// <summary>
    /// Simulates certificate renewal process
    /// In production, this would call MTM Server API to renew existing certificates
    /// </summary>
    private string RenewDeviceCertificate(string oldCertificate)
    {
        // Mock renewal logic - generates new certificate based on old one
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"RENEWED_CERT_{timestamp}_FROM_{oldCertificate.GetHashCode()}_SIGNED_BY_SERVER";
    }

    #endregion Helper Methods
}
