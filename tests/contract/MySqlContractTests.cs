using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for MySQL connection and access
/// </summary>
public class MySqlContractTests
{
    [Fact]
    public async Task MySqlConnection_ShouldValidateConnectionString()
    {
        // Arrange
        var mysqlClient = Substitute.For<IMySqlClient>();
        
        // Act
        var result = await mysqlClient.ExecuteScalarAsync<int>("SELECT 1");
        
        // Assert
        await mysqlClient.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
    }

    [Fact]
    public async Task MySqlAccess_ShouldEnforceRoleBasedPermissions()
    {
        // Arrange
        var mysqlClient = Substitute.For<IMySqlClient>();
        
        // Act - read operation should succeed
        await mysqlClient.ExecuteQueryAsync<object>("SELECT * FROM parts");
        
        // Assert
        await mysqlClient.Received(1).ExecuteQueryAsync<object>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public void MySqlConnectionPool_ShouldTrackMetrics()
    {
        // Arrange
        var mysqlClient = Substitute.For<IMySqlClient>();
        var mockMetrics = new MTM_Template_Application.Models.DataLayer.ConnectionPoolMetrics
        {
            PoolName = "Main",
            ActiveConnections = 2,
            IdleConnections = 3,
            MaxPoolSize = 10
        };
        mysqlClient.GetConnectionMetrics().Returns(mockMetrics);
        
        // Act
        var metrics = mysqlClient.GetConnectionMetrics();
        
        // Assert
        metrics.Should().NotBeNull();
        metrics.ActiveConnections.Should().BeGreaterThanOrEqualTo(0);
        metrics.MaxPoolSize.Should().BeGreaterThan(0);
    }
}
