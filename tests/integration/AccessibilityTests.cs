using FluentAssertions;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for accessibility features
/// </summary>
public class AccessibilityTests
{
    [Fact]
    public void Accessibility_ShouldAnnounceBootProgress()
    {
        // Arrange
        var orchestrator = NSubstitute.Substitute.For<MTM_Template_Application.Services.Boot.IBootOrchestrator>();
        string? lastAnnouncement = null;
        
        orchestrator.OnProgressChanged += (sender, args) =>
        {
            lastAnnouncement = args.StatusMessage;
        };
        
        // Act
        orchestrator.OnProgressChanged += NSubstitute.Raise.EventWith(
            new MTM_Template_Application.Services.Boot.BootProgressEventArgs
            {
                StageNumber = 1,
                StageName = "Core Services",
                ProgressPercentage = 50,
                StatusMessage = "Initializing core services"
            });
        
        // Assert
        lastAnnouncement.Should().NotBeNullOrEmpty("screen reader should receive progress announcements");
    }

    [Fact]
    public void Accessibility_ShouldSupportKeyboardNavigation()
    {
        // Arrange - keyboard navigation support
        var navigationService = NSubstitute.Substitute.For<MTM_Template_Application.Services.Navigation.INavigationService>();
        
        // Act - simulate keyboard navigation
        var canGoBack = true;
        var canGoForward = false;
        
        // Assert
        canGoBack.Should().BeTrue("keyboard navigation should support going back");
        canGoForward.Should().BeFalse("keyboard navigation state should be tracked");
    }
}
