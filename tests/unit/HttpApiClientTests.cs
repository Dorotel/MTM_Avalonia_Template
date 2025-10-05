using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.DataLayer;
using MTM_Template_Application.Services.DataLayer.Policies;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for HttpApiClient (T146)
/// Tests HTTP operations with Polly resilience policies
/// </summary>
public class HttpApiClientTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new HttpApiClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.GetAsync<object>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task GetAsync_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{\"Id\": 123, \"Name\": \"Test\"}");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.GetAsync<TestModel>("/api/test");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_WithTransientFailure_RetriesAndSucceeds()
    {
        // Arrange
        var mockHandler = new RetryMockHttpMessageHandler(
            new[] { HttpStatusCode.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK },
            "{\"Id\": 456, \"Name\": \"Retry\"}");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.GetAsync<TestModel>("/api/test");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(456);
        mockHandler.RequestCount.Should().Be(3);
    }

    #endregion

    #region PostAsync Tests

    [Fact]
    public async Task PostAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.PostAsync<object, object>(null!, new { });

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task PostAsync_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.PostAsync<object, object>("/api/test", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("data");
    }

    [Fact]
    public async Task PostAsync_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.Created, "{\"Id\": 789, \"Name\": \"Posted\"}");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.PostAsync<TestModel, TestModel>("/api/test", new TestModel { Id = 789, Name = "Posted" });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(789);
        result.Name.Should().Be("Posted");
    }

    #endregion

    #region PutAsync Tests

    [Fact]
    public async Task PutAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.PutAsync<object, object>(null!, new { });

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task PutAsync_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.PutAsync<object, object>("/api/test", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("data");
    }

    [Fact]
    public async Task PutAsync_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, "{\"Id\": 999, \"Name\": \"Updated\"}");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.PutAsync<TestModel, TestModel>("/api/test", new TestModel { Id = 999, Name = "Updated" });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(999);
        result.Name.Should().Be("Updated");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithNullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var act = async () => await client.DeleteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task DeleteAsync_WithSuccessfulResponse_ReturnsTrue()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.DeleteAsync("/api/test/123");

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAsync_WithNotFoundResponse_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var client = CreateClient(mockHandler);

        // Act
        var result = await client.DeleteAsync("/api/test/999");

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods & Classes

    private static HttpApiClient CreateClient(HttpMessageHandler? mockHandler = null)
    {
        var handler = mockHandler ?? new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        return new HttpApiClient(httpClient);
    }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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
                Content = new StringContent(_content, System.Text.Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }

    private class RetryMockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode[] _statusCodes;
        private readonly string _finalContent;
        private int _requestIndex;

        public int RequestCount => _requestIndex;

        public RetryMockHttpMessageHandler(HttpStatusCode[] statusCodes, string finalContent)
        {
            _statusCodes = statusCodes;
            _finalContent = finalContent;
            _requestIndex = 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var statusCode = _statusCodes[Math.Min(_requestIndex, _statusCodes.Length - 1)];
            _requestIndex++;

            var content = statusCode == HttpStatusCode.OK ? _finalContent : "";
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }

    #endregion
}
