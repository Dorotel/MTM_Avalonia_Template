using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.unit.Models.Diagnostics;

public class PerformanceSnapshotTests
{
    [Fact]
    public void ValidSnapshot_ShouldPassValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(100.1)]
    [InlineData(150.0)]
    public void InvalidCpuUsagePercent_ShouldFailValidation(double cpuUsage)
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = cpuUsage,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void NegativeMemoryUsage_ShouldFailValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = -1,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, 5, 2)]
    [InlineData(10, -1, 2)]
    [InlineData(10, 5, -1)]
    public void NegativeGcCollections_ShouldFailValidation(int gen0, int gen1, int gen2)
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = 128,
            GcGen0Collections = gen0,
            GcGen1Collections = gen1,
            GcGen2Collections = gen2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void ZeroOrNegativeThreadCount_ShouldFailValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 0,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void NegativeUptime_ShouldFailValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 45.5,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(-1)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void BoundaryValues_ShouldPassValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 0.0,
            MemoryUsageMB = 0,
            GcGen0Collections = 0,
            GcGen1Collections = 0,
            GcGen2Collections = 0,
            ThreadCount = 1,
            Uptime = TimeSpan.Zero
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void MaxCpuUsage_ShouldPassValidation()
    {
        // Arrange
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = 100.0,
            MemoryUsageMB = 128,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 2,
            ThreadCount = 15,
            Uptime = TimeSpan.FromMinutes(30)
        };

        // Act
        var isValid = snapshot.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }
}
