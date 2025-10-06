using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.unit.Models.Diagnostics;

public class ConnectionPoolStatsTests
{
    [Fact]
    public void ValidMySqlPoolStats_ShouldPassValidation()
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = 10,
            ActiveConnections = 6,
            IdleConnections = 4,
            WaitingRequests = 2,
            AverageWaitTime = TimeSpan.FromMilliseconds(50)
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void MySqlPoolStats_TotalEqualsSumOfActiveAndIdle()
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = 10,
            ActiveConnections = 6,
            IdleConnections = 4,
            WaitingRequests = 0,
            AverageWaitTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeTrue();
        stats.TotalConnections.Should().Be(stats.ActiveConnections + stats.IdleConnections);
    }

    [Fact]
    public void MySqlPoolStats_IncorrectTotal_ShouldFailValidation()
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = 15, // Incorrect
            ActiveConnections = 6,
            IdleConnections = 4,
            WaitingRequests = 0,
            AverageWaitTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(10, -1, 0)]
    [InlineData(10, 0, -1)]
    [InlineData(10, 6, 4, -1)]
    public void MySqlPoolStats_NegativeValues_ShouldFailValidation(
        int total,
        int active,
        int idle,
        int waiting = 0)
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = total,
            ActiveConnections = active,
            IdleConnections = idle,
            WaitingRequests = waiting,
            AverageWaitTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void MySqlPoolStats_NegativeAverageWaitTime_ShouldFailValidation()
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = 10,
            ActiveConnections = 6,
            IdleConnections = 4,
            WaitingRequests = 0,
            AverageWaitTime = TimeSpan.FromMilliseconds(-10)
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ValidHttpPoolStats_ShouldPassValidation()
    {
        // Arrange
        var stats = new HttpPoolStats
        {
            TotalConnections = 20,
            ActiveConnections = 12,
            IdleConnections = 8,
            AverageResponseTime = TimeSpan.FromMilliseconds(150)
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HttpPoolStats_TotalEqualsSumOfActiveAndIdle()
    {
        // Arrange
        var stats = new HttpPoolStats
        {
            TotalConnections = 20,
            ActiveConnections = 12,
            IdleConnections = 8,
            AverageResponseTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeTrue();
        stats.TotalConnections.Should().Be(stats.ActiveConnections + stats.IdleConnections);
    }

    [Fact]
    public void HttpPoolStats_IncorrectTotal_ShouldFailValidation()
    {
        // Arrange
        var stats = new HttpPoolStats
        {
            TotalConnections = 25, // Incorrect
            ActiveConnections = 12,
            IdleConnections = 8,
            AverageResponseTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(20, -1, 0)]
    [InlineData(20, 0, -1)]
    public void HttpPoolStats_NegativeValues_ShouldFailValidation(int total, int active, int idle)
    {
        // Arrange
        var stats = new HttpPoolStats
        {
            TotalConnections = total,
            ActiveConnections = active,
            IdleConnections = idle,
            AverageResponseTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void HttpPoolStats_NegativeAverageResponseTime_ShouldFailValidation()
    {
        // Arrange
        var stats = new HttpPoolStats
        {
            TotalConnections = 20,
            ActiveConnections = 12,
            IdleConnections = 8,
            AverageResponseTime = TimeSpan.FromMilliseconds(-50)
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ConnectionPoolStats_ShouldContainBothPoolTypes()
    {
        // Arrange
        var mySqlStats = new MySqlPoolStats
        {
            TotalConnections = 10,
            ActiveConnections = 6,
            IdleConnections = 4,
            WaitingRequests = 0,
            AverageWaitTime = TimeSpan.FromMilliseconds(50)
        };

        var httpStats = new HttpPoolStats
        {
            TotalConnections = 20,
            ActiveConnections = 12,
            IdleConnections = 8,
            AverageResponseTime = TimeSpan.FromMilliseconds(150)
        };

        var poolStats = new ConnectionPoolStats
        {
            Timestamp = DateTime.UtcNow,
            MySqlPool = mySqlStats,
            HttpPool = httpStats
        };

        // Assert
        poolStats.MySqlPool.Should().NotBeNull();
        poolStats.HttpPool.Should().NotBeNull();
        poolStats.MySqlPool.Should().Be(mySqlStats);
        poolStats.HttpPool.Should().Be(httpStats);
    }

    [Fact]
    public void BoundaryValues_ZeroConnections_ShouldBeValid()
    {
        // Arrange
        var stats = new MySqlPoolStats
        {
            TotalConnections = 0,
            ActiveConnections = 0,
            IdleConnections = 0,
            WaitingRequests = 0,
            AverageWaitTime = TimeSpan.Zero
        };

        // Act
        var isValid = stats.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }
}
