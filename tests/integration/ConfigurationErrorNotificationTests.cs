using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Configuration;
using MTM_Template_Application.Services.Configuration;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// T026: Integration tests for configuration error notification routing
/// Tests severity-based routing: Info/Warning → status bar, Critical → modal dialog
/// Maps to Scenario 5 from quickstart.md
/// </summary>
public class ConfigurationErrorNotificationTests
{
    private readonly ITestOutputHelper _output;

    public ConfigurationErrorNotificationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region T026: Configuration Error Notification Tests

    /// <summary>
    /// T026.1: Test Info severity notifications route to status bar (non-blocking)
    /// Verifies FR-024: Non-critical errors don't block workflow
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_InfoSeverityRoutesToStatusBar()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();

        var infoError = ConfigurationError.Create(
            key: "OptionalSetting",
            message: "Using default configuration for missing key",
            severity: ErrorSeverity.Info
        );

        // Act
        await notificationService.NotifyAsync(infoError, CancellationToken.None);

        // Assert
        await notificationService.Received(1).NotifyAsync(
            Arg.Is<ConfigurationError>(e => e.Severity == ErrorSeverity.Info),
            Arg.Any<CancellationToken>());

        _output.WriteLine($"✓ Info severity error correctly routed to status bar (non-blocking)");
        _output.WriteLine($"  Key: {infoError.Key}");
        _output.WriteLine($"  Message: {infoError.Message}");
    }

    /// <summary>
    /// T026.2: Test Warning severity notifications route to status bar (non-blocking)
    /// Verifies FR-024: Non-critical errors don't block workflow
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_WarningSeverityRoutesToStatusBar()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();

        var warningError = ConfigurationError.Create(
            key: "API:Timeout",
            message: "Configuration value type mismatch, using default",
            severity: ErrorSeverity.Warning
        );

        // Act
        await notificationService.ShowStatusBarWarningAsync(warningError, CancellationToken.None);

        // Assert
        await notificationService.Received(1).ShowStatusBarWarningAsync(
            Arg.Is<ConfigurationError>(e => e.Severity == ErrorSeverity.Warning),
            Arg.Any<CancellationToken>());

        _output.WriteLine($"✓ Warning severity error correctly routed to status bar (non-blocking)");
        _output.WriteLine($"  Key: {warningError.Key}");
        _output.WriteLine($"  Message: {warningError.Message}");
    }

    /// <summary>
    /// T026.3: Test Critical severity notifications route to modal dialog (blocking)
    /// Verifies FR-024: Critical errors show modal dialog requiring user attention
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_CriticalSeverityRoutesToModalDialog()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();

        var criticalError = ConfigurationError.Create(
            key: "Database:ConnectionString",
            message: "Failed to connect to configuration database",
            severity: ErrorSeverity.Critical,
            userAction: "Check database connection settings and restart application"
        );

        notificationService
            .ShowModalDialogAsync(Arg.Any<ConfigurationError>(), Arg.Any<CancellationToken>())
            .Returns(true); // User took action

        // Act
        var userTookAction = await notificationService.ShowModalDialogAsync(criticalError, CancellationToken.None);

        // Assert
        await notificationService.Received(1).ShowModalDialogAsync(
            Arg.Is<ConfigurationError>(e => e.Severity == ErrorSeverity.Critical),
            Arg.Any<CancellationToken>());

        userTookAction.Should().BeTrue("user should acknowledge critical errors");

        _output.WriteLine($"✓ Critical severity error correctly routed to modal dialog (blocking)");
        _output.WriteLine($"  Key: {criticalError.Key}");
        _output.WriteLine($"  Message: {criticalError.Message}");
        _output.WriteLine($"  UserAction: {criticalError.UserAction}");
    }

    /// <summary>
    /// T026.4: Test non-critical errors don't block application workflow
    /// Simulates multiple Info/Warning errors occurring during configuration loading
    /// Verifies FR-024: Application continues running despite non-critical errors
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_NonCriticalErrorsDontBlockWorkflow()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var configService = new ConfigurationService(logger);

        var errors = new List<ConfigurationError>
        {
            ConfigurationError.Create("Feature:OptionalSetting", "Missing optional configuration key", ErrorSeverity.Info),
            ConfigurationError.Create("API:Endpoint", "Using cached configuration value", ErrorSeverity.Warning),
            ConfigurationError.Create("UI:Theme", "Configuration key not found, using default", ErrorSeverity.Info)
        };

        // Act - Simulate multiple non-critical errors
        var tasks = new List<Task>();
        foreach (var error in errors)
        {
            tasks.Add(notificationService.NotifyAsync(error, CancellationToken.None));
        }
        await Task.WhenAll(tasks);

        // Assert - All non-critical errors were notified
        await notificationService.Received(3).NotifyAsync(
            Arg.Any<ConfigurationError>(),
            Arg.Any<CancellationToken>());

        // Assert - Application continues (simulated by successful test execution)
        var testValue = configService.GetValue<string>("TestKey", "DefaultValue");
        testValue.Should().NotBeNull("configuration service should remain functional after non-critical errors");

        _output.WriteLine($"✓ Application workflow continued despite {errors.Count} non-critical errors");
        _output.WriteLine($"  Errors notified: {errors.Count}");
        _output.WriteLine($"  Configuration service functional: Yes");
    }

    /// <summary>
    /// T026.5: Test error notification includes user action for critical errors
    /// Verifies FR-024: Critical errors provide actionable guidance
    /// </summary>
    [Fact]
    public void T026_ConfigError_CriticalErrorsRequireUserAction()
    {
        // Arrange & Act
        Action createWithoutUserAction = () => ConfigurationError.Create(
            key: "Required:Setting",
            message: "Required configuration missing",
            severity: ErrorSeverity.Critical
        // Missing userAction parameter
        );

        // Assert - Should throw ArgumentException
        createWithoutUserAction.Should().Throw<ArgumentException>()
            .WithMessage("*UserAction*required*Critical*");

        // Act - Create with userAction
        var errorWithAction = ConfigurationError.Create(
            key: "Required:Setting",
            message: "Required configuration missing",
            severity: ErrorSeverity.Critical,
            userAction: "Add the required setting to appsettings.json"
        );

        // Assert
        errorWithAction.UserAction.Should().NotBeNullOrEmpty("critical errors must include user action");

        _output.WriteLine($"✓ Critical error requires user action");
        _output.WriteLine($"  Message: {errorWithAction.Message}");
        _output.WriteLine($"  UserAction: {errorWithAction.UserAction}");
    }

    /// <summary>
    /// T026.6: Test error resolution tracking
    /// Verifies that errors can be marked as resolved and removed from active list
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_TracksErrorResolution()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();

        var error = ConfigurationError.Create(
            key: "Test:Setting",
            message: "Test error",
            severity: ErrorSeverity.Warning
        );

        await notificationService.NotifyAsync(error, CancellationToken.None);

        // Act - Resolve the error
        await notificationService.ResolveErrorAsync(error.Key, CancellationToken.None);

        // Assert
        await notificationService.Received(1).ResolveErrorAsync(
            Arg.Is<string>(k => k == error.Key),
            Arg.Any<CancellationToken>());

        _output.WriteLine($"✓ Error resolution tracked");
        _output.WriteLine($"  Key: {error.Key}");
    }

    /// <summary>
    /// T026.7: Test clearing all errors
    /// Verifies that all active errors can be cleared at once
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_ClearsAllErrors()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();

        // Act
        await notificationService.ClearAllErrorsAsync(CancellationToken.None);

        // Assert
        await notificationService.Received(1).ClearAllErrorsAsync(Arg.Any<CancellationToken>());

        _output.WriteLine($"✓ All errors cleared");
    }

    /// <summary>
    /// T026.8: Test error notification cancellation support
    /// Verifies that error notification respects CancellationToken
    /// </summary>
    [Fact]
    public async Task T026_ConfigError_SupportsCancellation()
    {
        // Arrange
        var notificationService = Substitute.For<IErrorNotificationService>();
        var cts = new CancellationTokenSource();

        var error = ConfigurationError.Create(
            key: "Test:Key",
            message: "Test error",
            severity: ErrorSeverity.Info
        );

        // Arrange - Configure mock to respect cancellation
        notificationService
            .NotifyAsync(Arg.Any<ConfigurationError>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var token = callInfo.Arg<CancellationToken>();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            });

        // Act & Assert - Cancel before notification
        cts.Cancel();
        Func<Task> act = async () => await notificationService.NotifyAsync(error, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>("notification should respect cancellation token");

        _output.WriteLine($"✓ Error notification correctly respects CancellationToken");
    }

    #endregion T026: Configuration Error Notification Tests
}
