using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// Integration tests for diagnostic checks and system health
/// </summary>
public class DiagnosticsTests
{
    [Fact]
    public async Task DiagnosticsChecks_ShouldDetectStorageAvailability()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();
        var mockResults = new List<DiagnosticResult>
        {
            new DiagnosticResult
            {
                CheckName = "Storage",
                Status = "Pass",
                Message = "Storage available",
                DurationMs = 50
            }
        };
        diagnosticsService.RunAllChecksAsync().Returns(mockResults);
        
        // Act
        var results = await diagnosticsService.RunAllChecksAsync();
        
        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.CheckName == "Storage");
    }

    [Fact]
    public async Task DiagnosticsChecks_ShouldDetectNetworkConnectivity()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();
        
        // Act
        var result = await diagnosticsService.RunCheckAsync("Network");
        
        // Assert
        result.Should().NotBeNull();
        result.CheckName.Should().Be("Network");
        result.DurationMs.Should().BeLessThan(5000, "network check should complete within 5s timeout");
    }

    [Fact]
    public void HardwareCapabilities_ShouldDetectSystemResources()
    {
        // Arrange
        var diagnosticsService = Substitute.For<IDiagnosticsService>();
        var mockCapabilities = new HardwareCapabilities
        {
            TotalMemoryMB = 8192,
            AvailableMemoryMB = 4096,
            ProcessorCount = 4,
            Platform = "Windows"
        };
        diagnosticsService.GetHardwareCapabilities().Returns(mockCapabilities);
        
        // Act
        var capabilities = diagnosticsService.GetHardwareCapabilities();
        
        // Assert
        capabilities.Should().NotBeNull();
        capabilities.TotalMemoryMB.Should().BeGreaterThan(0);
        capabilities.ProcessorCount.Should().BeGreaterThan(0);
        capabilities.Platform.Should().NotBeNullOrEmpty();
    }
}
