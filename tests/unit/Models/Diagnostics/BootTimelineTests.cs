using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.unit.Models.Diagnostics;

public class BootTimelineTests
{
    [Fact]
    public void ValidBootTimeline_ShouldPassValidation()
    {
        // Arrange
        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromSeconds(1),
                Success = true,
                ErrorMessage = null
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>(),
                ErrorMessage = null
            },
            Stage2 = new Stage2Info
            {
                Duration = TimeSpan.FromSeconds(1),
                Success = true,
                ErrorMessage = null
            },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };

        // Act
        var isValid = timeline.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void TotalBootTime_ShouldEqualSumOfStages()
    {
        // Arrange
        var stage0Duration = TimeSpan.FromSeconds(1.5);
        var stage1Duration = TimeSpan.FromSeconds(2.3);
        var stage2Duration = TimeSpan.FromSeconds(0.8);
        var expectedTotal = stage0Duration + stage1Duration + stage2Duration;

        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info { Duration = stage0Duration, Success = true },
            Stage1 = new Stage1Info
            {
                Duration = stage1Duration,
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info { Duration = stage2Duration, Success = true },
            TotalBootTime = expectedTotal
        };

        // Act
        var isValid = timeline.IsValid();

        // Assert
        isValid.Should().BeTrue();
        timeline.TotalBootTime.Should().Be(expectedTotal);
    }

    [Fact]
    public void IncorrectTotalBootTime_ShouldFailValidation()
    {
        // Arrange
        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(10) // Incorrect total
        };

        // Act
        var isValid = timeline.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EmptyServiceTimings_ShouldBeValid()
    {
        // Arrange
        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>() // Empty list
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };

        // Act
        var isValid = timeline.IsValid();

        // Assert
        isValid.Should().BeTrue();
        timeline.Stage1.ServiceTimings.Should().BeEmpty();
    }

    [Fact]
    public void PopulatedServiceTimings_ShouldBeValid()
    {
        // Arrange
        var serviceTimings = new List<ServiceInitInfo>
        {
            new() { ServiceName = "Database", Duration = TimeSpan.FromMilliseconds(500), Success = true },
            new() { ServiceName = "Cache", Duration = TimeSpan.FromMilliseconds(300), Success = true },
            new() { ServiceName = "Config", Duration = TimeSpan.FromMilliseconds(200), Success = true }
        };

        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = serviceTimings
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };

        // Act
        var isValid = timeline.IsValid();

        // Assert
        isValid.Should().BeTrue();
        timeline.Stage1.ServiceTimings.Should().HaveCount(3);
    }

    [Fact]
    public void FailedStage_ShouldHaveErrorMessage()
    {
        // Arrange
        var timeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow,
            Stage0 = new Stage0Info
            {
                Duration = TimeSpan.FromSeconds(1),
                Success = false,
                ErrorMessage = "Splash screen failed to load"
            },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>()
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };

        // Act & Assert
        timeline.Stage0.Success.Should().BeFalse();
        timeline.Stage0.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ServiceInitInfo_ShouldContainRequiredFields()
    {
        // Arrange
        var service = new ServiceInitInfo
        {
            ServiceName = "TestService",
            Duration = TimeSpan.FromMilliseconds(123),
            Success = true
        };

        // Assert
        service.ServiceName.Should().Be("TestService");
        service.Duration.Should().Be(TimeSpan.FromMilliseconds(123));
        service.Success.Should().BeTrue();
    }
}
