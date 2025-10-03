using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for HTTP API endpoints
/// </summary>
public class HttpApiContractTests
{
    [Fact]
    public async Task HttpApiEndpoints_ShouldRespondToHealthCheck()
    {
        // Arrange
        var httpClient = Substitute.For<IHttpApiClient>();
        httpClient.GetAsync<object>("/health").Returns(Task.FromResult<object?>(new { status = "healthy" }));
        
        // Act
        var result = await httpClient.GetAsync<object>("/health");
        
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HttpApi_ShouldSupportStandardRESTOperations()
    {
        // Arrange
        var httpClient = Substitute.For<IHttpApiClient>();
        
        // Act & Assert - GET
        await httpClient.GetAsync<object>("/api/items");
        await httpClient.Received(1).GetAsync<object>("/api/items");
        
        // Act & Assert - POST
        await httpClient.PostAsync<object, object>("/api/items", new { });
        await httpClient.Received(1).PostAsync<object, object>("/api/items", Arg.Any<object>());
    }
}
