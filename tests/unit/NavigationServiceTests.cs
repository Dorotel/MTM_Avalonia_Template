using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Navigation;
using MTM_Template_Application.Services.Navigation;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for NavigationService (T155)
/// Tests stack-based navigation, history tracking, and deep linking
/// </summary>
public class NavigationServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUnsavedChangesGuard_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new NavigationService(Substitute.For<ILogger<NavigationService>>(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unsavedChangesGuard");
    }

    [Fact]
    public void Constructor_WithValidGuard_CreatesServiceSuccessfully()
    {
        // Arrange
        var guard = Substitute.For<UnsavedChangesGuard>();

        // Act
        var service = new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region NavigateToAsync Tests

    [Fact]
    public async Task NavigateToAsync_WithNullViewName_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateNavigationService();

        // Act
        var act = async () => await service.NavigateToAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("viewName");
    }

    [Fact]
    public async Task NavigateToAsync_WithValidViewName_NavigatesSuccessfully()
    {
        // Arrange
        var guard = Substitute.For<UnsavedChangesGuard>();
        guard.CanNavigateAsync().Returns(true);
        var service = new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);

        // Act
        await service.NavigateToAsync("HomeView");

        // Assert - Should complete without throwing
        var history = service.GetHistory();
        history.Should().ContainSingle();
        history[0].ViewName.Should().Be("HomeView");
    }

    [Fact]
    public async Task NavigateToAsync_WithParameters_StoresParameters()
    {
        // Arrange
        var service = CreateNavigationService();
        var parameters = new Dictionary<string, object>
        {
            { "UserId", 123 },
            { "Mode", "Edit" }
        };

        // Act
        await service.NavigateToAsync("UserDetailsView", parameters);

        // Assert
        var history = service.GetHistory();
        history[0].Parameters.Should().ContainKey("UserId");
        history[0].Parameters["UserId"].Should().Be(123);
    }

    [Fact]
    public async Task NavigateToAsync_WhenUnsavedChanges_CancelsNavigation()
    {
        // Arrange
        var guard = Substitute.For<UnsavedChangesGuard>();
        guard.CanNavigateAsync().Returns(false); // User cancelled
        var service = new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);

        // Navigate to initial view
        guard.CanNavigateAsync().Returns(true);
        await service.NavigateToAsync("InitialView");

        // Act - Try to navigate with unsaved changes
        guard.CanNavigateAsync().Returns(false);
        await service.NavigateToAsync("SecondView");

        // Assert - Should still be on initial view
        var history = service.GetHistory();
        history.Should().ContainSingle();
        history[0].ViewName.Should().Be("InitialView");
    }

    [Fact]
    public async Task NavigateToAsync_ClearsForwardStack()
    {
        // Arrange
        var service = CreateNavigationService();

        // Build history: View1 -> View2 -> View3
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");
        await service.NavigateToAsync("View3");

        // Go back twice: View3 -> View2 -> View1
        await service.GoBackAsync();
        await service.GoBackAsync();

        // Act - Navigate to new view from View1
        await service.NavigateToAsync("View4");

        // Assert - Forward stack should be cleared
        var canGoForward = service.GetHistory().Any(h => h.CanGoForward);
        canGoForward.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var service = CreateNavigationService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await service.NavigateToAsync("TestView", null, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GoBackAsync Tests

    [Fact]
    public async Task GoBackAsync_WithEmptyBackStack_ReturnsFalse()
    {
        // Arrange
        var service = CreateNavigationService();

        // Act
        var result = await service.GoBackAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GoBackAsync_WithHistory_NavigatesBackAndReturnsTrue()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");

        // Act
        var result = await service.GoBackAsync();

        // Assert
        result.Should().BeTrue();
        var history = service.GetHistory();
        history.Last().ViewName.Should().Be("View1");
    }

    [Fact]
    public async Task GoBackAsync_WhenUnsavedChanges_ReturnsFalse()
    {
        // Arrange
        var guard = Substitute.For<UnsavedChangesGuard>();
        guard.CanNavigateAsync().Returns(true);
        var service = new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);

        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");

        // Act - Block navigation back
        guard.CanNavigateAsync().Returns(false);
        var result = await service.GoBackAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GoBackAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await service.GoBackAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GoForwardAsync Tests

    [Fact]
    public async Task GoForwardAsync_WithEmptyForwardStack_ReturnsFalse()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");

        // Act
        var result = await service.GoForwardAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GoForwardAsync_AfterGoingBack_NavigatesForwardAndReturnsTrue()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");
        await service.GoBackAsync(); // Go back to View1

        // Act
        var result = await service.GoForwardAsync();

        // Assert
        result.Should().BeTrue();
        var history = service.GetHistory();
        history.Last().ViewName.Should().Be("View2");
    }

    [Fact]
    public async Task GoForwardAsync_WhenUnsavedChanges_ReturnsFalse()
    {
        // Arrange
        var guard = Substitute.For<UnsavedChangesGuard>();
        guard.CanNavigateAsync().Returns(true);
        var service = new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);

        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");
        await service.GoBackAsync();

        // Act - Block forward navigation
        guard.CanNavigateAsync().Returns(false);
        var result = await service.GoForwardAsync();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetHistory Tests

    [Fact]
    public void GetHistory_InitiallyEmpty_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateNavigationService();

        // Act
        var history = service.GetHistory();

        // Assert
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_AfterNavigation_ReturnsFullHistory()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");
        await service.NavigateToAsync("View3");

        // Act
        var history = service.GetHistory();

        // Assert
        history.Should().HaveCount(3);
        history[0].ViewName.Should().Be("View1");
        history[1].ViewName.Should().Be("View2");
        history[2].ViewName.Should().Be("View3");
    }

    [Fact]
    public async Task GetHistory_IncludesTimestamps()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");

        // Act
        var history = service.GetHistory();

        // Assert
        history[0].NavigatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region CanGoBack/CanGoForward Tests

    [Fact]
    public async Task NavigationHistoryEntry_CanGoBack_UpdatesCorrectly()
    {
        // Arrange
        var service = CreateNavigationService();

        // Act
        await service.NavigateToAsync("View1");
        var history1 = service.GetHistory();
        var canGoBack1 = history1.Last().CanGoBack;

        await service.NavigateToAsync("View2");
        var history2 = service.GetHistory();
        var canGoBack2 = history2.Last().CanGoBack;

        // Assert
        canGoBack1.Should().BeFalse(); // First view, can't go back
        canGoBack2.Should().BeTrue();  // Second view, can go back
    }

    [Fact]
    public async Task NavigationHistoryEntry_CanGoForward_UpdatesAfterGoBack()
    {
        // Arrange
        var service = CreateNavigationService();
        await service.NavigateToAsync("View1");
        await service.NavigateToAsync("View2");

        // Act
        await service.GoBackAsync();
        var history = service.GetHistory();

        // Assert
        var currentEntry = history.Last();
        currentEntry.CanGoForward.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static NavigationService CreateNavigationService()
    {
        var guard = Substitute.For<UnsavedChangesGuard>();
        guard.CanNavigateAsync().Returns(true);
        return new NavigationService(Substitute.For<ILogger<NavigationService>>(), guard);
    }

    #endregion
}
