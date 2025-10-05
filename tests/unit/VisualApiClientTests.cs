using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.DataLayer;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for VisualApiClient (T145)
/// Tests whitelist enforcement, authentication, and API communication
/// </summary>
public class VisualApiClientTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new VisualApiClient(null!, "http://localhost", new[] { "TestCommand" });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_WithNullBaseUrl_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        Action act = () => new VisualApiClient(httpClient, null!, new[] { "TestCommand" });

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("baseUrl");
    }

    [Fact]
    public void Constructor_WithNullWhitelist_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        Action act = () => new VisualApiClient(httpClient, "http://localhost", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("whitelistedCommands");
    }

    #endregion

    #region ExecuteCommandAsync Tests

    [Fact]
    public async Task ExecuteCommandAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.ExecuteCommandAsync<object>(null!, new Dictionary<string, object>());

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("command");
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.ExecuteCommandAsync<object>("TestCommand", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("parameters");
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithNonWhitelistedCommand_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var whitelist = new[] { "AllowedCommand" };
        var client = CreateClient(whitelist);

        // Act
        var act = async () => await client.ExecuteCommandAsync<object>("BlockedCommand", new Dictionary<string, object>());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not whitelisted*");
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithWhitelistedCommand_ExecutesSuccessfully()
    {
        // Arrange
        var whitelist = new[] { "GetCustomer" };
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{\"CustomerId\": 123}");
        var client = CreateClient(whitelist, mockHandler);

        // Act
        var result = await client.ExecuteCommandAsync<TestResponse>("GetCustomer", new Dictionary<string, object> { ["Id"] = 123 });

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(123);
    }

    #endregion

    #region IsServerAvailable Tests

    [Fact]
    public async Task IsServerAvailable_WhenServerResponds_ReturnsTrue()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        var client = CreateClient(mockHandler: mockHandler);

        // Act
        var result = await client.IsServerAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsServerAvailable_WhenServerUnreachable_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "");
        var client = CreateClient(mockHandler: mockHandler);

        // Act
        var result = await client.IsServerAvailable();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetWhitelistedCommands Tests

    [Fact]
    public void GetWhitelistedCommands_ReturnsWhitelistCopy()
    {
        // Arrange
        var whitelist = new[] { "Command1", "Command2", "Command3" };
        var client = CreateClient(whitelist);

        // Act
        var result = client.GetWhitelistedCommands();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Command1");
        result.Should().Contain("Command2");
        result.Should().Contain("Command3");
    }

    #endregion

    #region DeviceCertificateAuthenticationProvider Tests

    [Fact]
    public void DeviceCertificateAuthProvider_Constructor_WithNullSecretsService_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new DeviceCertificateAuthenticationProvider(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("secretsService");
    }

    [Fact]
    public async Task DeviceCertificateAuthProvider_GetAuthenticationTokenAsync_WithMissingCertificate_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var mockSecretsService = Substitute.For<ISecretsService>();
        mockSecretsService.RetrieveSecretAsync("VisualDeviceCertificate", Arg.Any<System.Threading.CancellationToken>())
            .Returns(Task.FromResult<string?>(null));

        var provider = new DeviceCertificateAuthenticationProvider(mockSecretsService);

        // Act
        var act = async () => await provider.GetAuthenticationTokenAsync();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Device certificate not found*");
    }

    #endregion

    #region Helper Methods & Classes

    private static VisualApiClient CreateClient(
        IEnumerable<string>? whitelist = null,
        MockHttpMessageHandler? mockHandler = null)
    {
        var handler = mockHandler ?? new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var wl = whitelist ?? new[] { "TestCommand" };

        return new VisualApiClient(httpClient, "http://localhost", wl);
    }

    private class TestResponse
    {
        public int CustomerId { get; set; }
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };

            return Task.FromResult(response);
        }
    }

    #endregion
}
