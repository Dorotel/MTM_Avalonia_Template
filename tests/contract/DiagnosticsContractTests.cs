using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for diagnostic check APIs
/// </summary>
public class DiagnosticsContractTests
{
    [Fact]
    public async Task DiagnosticsApi_ShouldProvideStorageCheck()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();

        // Act
        var result = await diagnosticsService.RunCheckAsync("Storage");

        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("Storage");
        result.Status.Should().BeDefined();
    }

    [Fact]
    public async Task DiagnosticsApi_ShouldProvidePermissionsCheck()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();

        // Act
        var result = await diagnosticsService.RunCheckAsync("Permissions");

        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("Permissions");
    }

    [Fact]
    public async Task DiagnosticsApi_ShouldProvideNetworkCheck()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();

        // Act
        var result = await diagnosticsService.RunCheckAsync("Network");

        // Assert
        result.Should().NotBeNull();
        result.DurationMs.Should().BeLessThan(5000, "network check should have 5s timeout");
    }

    [Fact]
    public void DiagnosticsApi_ShouldProvideHardwareCapabilities()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();

        // Act
        var capabilities = diagnosticsService.GetHardwareCapabilities();

        // Assert
        capabilities.Should().NotBeNull();
    }
}
