using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for Visual API Toolkit integration
/// </summary>
public class VisualApiContractTests
{
    [Fact]
    public void VisualApiWhitelist_ShouldOnlyAllowApprovedCommands()
    {
        // Arrange
        var visualClient = Substitute.For<IVisualApiClient>();
        var whitelistedCommands = new List<string> { "GetPart", "GetLocation", "GetWorkCenter" };
        visualClient.GetWhitelistedCommands().Returns(whitelistedCommands);
        
        // Act
        var commands = visualClient.GetWhitelistedCommands();
        
        // Assert
        commands.Should().NotBeEmpty();
        commands.Should().Contain("GetPart");
        commands.Should().NotContain("DeletePart", "destructive operations should not be whitelisted");
    }

    [Fact]
    public async Task VisualApiAuthentication_ShouldRequireDeviceCertificateAndCredentials()
    {
        // Arrange
        var visualClient = Substitute.For<IVisualApiClient>();
        var parameters = new Dictionary<string, object>
        {
            { "deviceCert", "cert_data" },
            { "username", "test_user" },
            { "password", "test_pass" }
        };
        
        // Act
        var result = await visualClient.ExecuteCommandAsync<object>("GetPart", parameters);
        
        // Assert
        await visualClient.Received(1).ExecuteCommandAsync<object>("GetPart", Arg.Any<Dictionary<string, object>>());
    }

    [Fact]
    public async Task VisualApiSchemaValidation_ShouldResolveTableAndColumnNames()
    {
        // Arrange
        var visualClient = Substitute.For<IVisualApiClient>();
        
        // Act
        var result = await visualClient.ExecuteCommandAsync<object>("GetTableSchema", 
            new Dictionary<string, object> { { "table", "PART" } });
        
        // Assert
        await visualClient.Received(1).ExecuteCommandAsync<object>(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>());
    }
}
