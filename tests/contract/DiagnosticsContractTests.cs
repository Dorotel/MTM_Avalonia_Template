using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for diagnostic check APIs
/// </summary>
public class DiagnosticsContractTests
{
    private static DiagnosticsService CreateDiagnosticsService()
    {
        var logger = Substitute.For<ILogger<DiagnosticsService>>();
        var hardwareDetection = Substitute.For<HardwareDetection>();

        // Configure mock to return hardware capabilities
        hardwareDetection.DetectCapabilities().Returns(new MTM_Template_Application.Models.Diagnostics.HardwareCapabilities
        {
            Platform = "TestPlatform",
            ProcessorCount = 4,
            TotalMemoryMB = 8192,
            AvailableMemoryMB = 4096
        });

        // Create real diagnostic checks
        var checks = new List<IDiagnosticCheck>
        {
            new StorageDiagnostic(),
            new PermissionsDiagnostic(),
            new NetworkDiagnostic()
        };

        return new DiagnosticsService(logger, checks, hardwareDetection);
    }

    [Fact]
    public async Task DiagnosticsApi_ShouldProvideStorageCheck()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var result = await diagnosticsService.RunCheckAsync("StorageDiagnostic");

        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("StorageDiagnostic");
        result.Status.Should().BeDefined();
    }

    [Fact]
    public async Task DiagnosticsApi_ShouldProvidePermissionsCheck()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var result = await diagnosticsService.RunCheckAsync("PermissionsDiagnostic");

        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("PermissionsDiagnostic");
    }

    [Fact]
    public async Task DiagnosticsApi_ShouldProvideNetworkCheck()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var result = await diagnosticsService.RunCheckAsync("NetworkDiagnostic");

        // Assert
        result.Should().NotBeNull();
        result.DurationMs.Should().BeLessThan(5000, "network check should have 5s timeout");
    }

    [Fact]
    public void DiagnosticsApi_ShouldProvideHardwareCapabilities()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var capabilities = diagnosticsService.GetHardwareCapabilities();

        // Assert
        capabilities.Should().NotBeNull();
    }
}
