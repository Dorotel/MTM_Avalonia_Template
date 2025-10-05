using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for diagnostic checks and system health
/// </summary>
public class DiagnosticsTests
{
    private static DiagnosticsService CreateDiagnosticsService()
    {
        var logger = Substitute.For<ILogger<DiagnosticsService>>();
        var hardwareDetection = Substitute.For<HardwareDetection>();

        // Configure mock to return hardware capabilities
        hardwareDetection.DetectCapabilities().Returns(new HardwareCapabilities
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
    public async Task DiagnosticsChecks_ShouldDetectStorageAvailability()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var results = await diagnosticsService.RunAllChecksAsync();

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.CheckName == "StorageDiagnostic");
    }

    [Fact]
    public async Task DiagnosticsChecks_ShouldDetectNetworkConnectivity()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var result = await diagnosticsService.RunCheckAsync("NetworkDiagnostic");

        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("NetworkDiagnostic");
        result.DurationMs.Should().BeLessThan(5000, "network check should complete within 5s timeout");
    }

    [Fact]
    public void HardwareCapabilities_ShouldDetectSystemResources()
    {
        // Arrange
        var diagnosticsService = CreateDiagnosticsService();

        // Act
        var capabilities = diagnosticsService.GetHardwareCapabilities();

        // Assert
        capabilities.Should().NotBeNull();
        capabilities.TotalMemoryMB.Should().BeGreaterThan(0);
        capabilities.ProcessorCount.Should().BeGreaterThan(0);
        capabilities.Platform.Should().NotBeNullOrEmpty();
    }
}
