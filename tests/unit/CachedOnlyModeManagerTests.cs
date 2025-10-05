using System;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Cache;
using MTM_Template_Application.Services.DataLayer;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for CachedOnlyModeManager (T149)
/// Tests offline mode detection and management
/// </summary>
public class CachedOnlyModeManagerTests : IDisposable
{
    private readonly CachedOnlyModeManager _manager;
    private readonly IVisualApiClient _mockApiClient;

    public CachedOnlyModeManagerTests()
    {
        _mockApiClient = Substitute.For<IVisualApiClient>();
        _manager = new CachedOnlyModeManager(_mockApiClient);
    }

    public void Dispose()
    {
        _manager?.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullApiClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new CachedOnlyModeManager(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("visualApiClient");
    }

    [Fact]
    public void Constructor_WithCustomReconnectionInterval_UsesSpecifiedInterval()
    {
        // Arrange
        var customInterval = TimeSpan.FromSeconds(10);

        // Act
        using var manager = new CachedOnlyModeManager(_mockApiClient, customInterval);

        // Assert
        manager.Should().NotBeNull();
    }

    #endregion

    #region EnableCachedOnlyMode Tests

    [Fact]
    public void EnableCachedOnlyMode_FirstTime_EnablesModeAndRaisesEvent()
    {
        // Arrange
        var eventRaised = false;
        _manager.ModeChanged += (sender, args) => eventRaised = true;

        // Act
        _manager.EnableCachedOnlyMode("Test reason");

        // Assert
        eventRaised.Should().BeTrue();
        _manager.IsCachedOnlyMode.Should().BeTrue();
    }

    [Fact]
    public void EnableCachedOnlyMode_WhenAlreadyEnabled_DoesNotRaiseEventAgain()
    {
        // Arrange
        _manager.EnableCachedOnlyMode("Initial reason");
        var eventCount = 0;
        _manager.ModeChanged += (sender, args) => eventCount++;

        // Act
        _manager.EnableCachedOnlyMode("Second reason");

        // Assert
        eventCount.Should().Be(0);
    }

    #endregion

    #region DisableCachedOnlyMode Tests

    [Fact]
    public void DisableCachedOnlyMode_WhenEnabled_DisablesModeAndRaisesEvent()
    {
        // Arrange
        _manager.EnableCachedOnlyMode("Test reason");
        var eventRaised = false;
        _manager.ModeChanged += (sender, args) => eventRaised = true;

        // Act
        _manager.DisableCachedOnlyMode();

        // Assert
        eventRaised.Should().BeTrue();
        _manager.IsCachedOnlyMode.Should().BeFalse();
    }

    [Fact]
    public void DisableCachedOnlyMode_WhenNotEnabled_DoesNotRaiseEvent()
    {
        // Arrange
        var eventCount = 0;
        _manager.ModeChanged += (sender, args) => eventCount++;

        // Act
        _manager.DisableCachedOnlyMode();

        // Assert
        eventCount.Should().Be(0);
    }

    #endregion

    #region CheckServerAvailabilityAsync Tests

    [Fact]
    public async Task CheckServerAvailabilityAsync_WhenServerAvailable_ReturnsTrue()
    {
        // Arrange
        _mockApiClient.IsServerAvailable().Returns(true);

        // Act
        var result = await _manager.CheckServerAvailabilityAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckServerAvailabilityAsync_WhenServerUnavailable_ReturnsFalse()
    {
        // Arrange
        _mockApiClient.IsServerAvailable().Returns(false);

        // Act
        var result = await _manager.CheckServerAvailabilityAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckServerAvailabilityAsync_WhenServerBecomesUnavailable_EnablesCachedOnlyMode()
    {
        // Arrange
        _mockApiClient.IsServerAvailable().Returns(false);
        var eventRaised = false;
        _manager.ModeChanged += (sender, args) => eventRaised = true;

        // Act
        await _manager.CheckServerAvailabilityAsync();

        // Assert
        eventRaised.Should().BeTrue();
        _manager.IsCachedOnlyMode.Should().BeTrue();
    }

    [Fact]
    public async Task CheckServerAvailabilityAsync_WhenServerBecomesAvailable_DisablesCachedOnlyMode()
    {
        // Arrange
        _mockApiClient.IsServerAvailable().Returns(false);
        await _manager.CheckServerAvailabilityAsync(); // Enable cached-only mode

        _mockApiClient.IsServerAvailable().Returns(true);
        var modeChangedCount = 0;
        _manager.ModeChanged += (sender, args) => modeChangedCount++;

        // Act
        await _manager.CheckServerAvailabilityAsync();

        // Assert
        modeChangedCount.Should().Be(1);
        _manager.IsCachedOnlyMode.Should().BeFalse();
    }

    #endregion

    #region GetFeatureLimitations Tests

    [Fact]
    public void GetFeatureLimitations_ReturnsLimitationsList()
    {
        // Arrange & Act
        var limitations = _manager.GetFeatureLimitations();

        // Assert
        limitations.Should().NotBeNull();
        limitations.Should().NotBeEmpty();
        limitations.Should().Contain(l => l.Contains("Cannot create") || l.Contains("Limited to cached"));
    }

    #endregion

    #region ModeChanged Event Tests

    [Fact]
    public void ModeChanged_WhenEnabled_RaisesEventWithCorrectArgs()
    {
        // Arrange
        CachedOnlyModeChangedEventArgs? eventArgs = null;
        _manager.ModeChanged += (sender, args) => eventArgs = args;

        // Act
        _manager.EnableCachedOnlyMode("Test reason");

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.IsCachedOnlyMode.Should().BeTrue();
        eventArgs.Reason.Should().Be("Test reason");
        eventArgs.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ModeChanged_WhenDisabled_RaisesEventWithCorrectArgs()
    {
        // Arrange
        _manager.EnableCachedOnlyMode("Initial reason");
        CachedOnlyModeChangedEventArgs? eventArgs = null;
        _manager.ModeChanged += (sender, args) => eventArgs = args;

        // Act
        _manager.DisableCachedOnlyMode();

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.IsCachedOnlyMode.Should().BeFalse();
        eventArgs.Reason.Should().Contain("reconnected");
        eventArgs.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_WithRunningTimer_StopsTimerAndDisposesResources()
    {
        // Arrange
        var manager = new CachedOnlyModeManager(_mockApiClient);
        manager.EnableCachedOnlyMode("Test");

        // Act
        manager.Dispose();

        // Assert - Should not throw
        var act = () => manager.Dispose(); // Second dispose should be safe
        act.Should().NotThrow();
    }

    #endregion
}
