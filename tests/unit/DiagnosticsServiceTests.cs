using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using MTM_Template_Application.Services.Diagnostics.Checks;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for DiagnosticsService
/// Tests cover T143: Test diagnostic checks, issue detection, hardware capabilities
/// </summary>
public class DiagnosticsServiceTests
{
    private readonly List<IDiagnosticCheck> _mockChecks;
    private readonly HardwareDetection _mockHardwareDetection;
    private readonly DiagnosticsService _service;

    public DiagnosticsServiceTests()
    {
        _mockChecks = new List<IDiagnosticCheck>();
        _mockHardwareDetection = Substitute.For<HardwareDetection>();
        var mockLogger = Substitute.For<ILogger<DiagnosticsService>>();

        _service = new DiagnosticsService(mockLogger, _mockChecks, _mockHardwareDetection);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public void Constructor_WithNullChecks_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<IDiagnosticCheck> nullChecks = null!;
        var mockLogger = Substitute.For<ILogger<DiagnosticsService>>();

        // Act
        Action act = () => new DiagnosticsService(mockLogger, nullChecks, _mockHardwareDetection);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("diagnosticChecks");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public void Constructor_WithNullHardwareDetection_ThrowsArgumentNullException()
    {
        // Arrange
        HardwareDetection nullHardware = null!;
        var mockLogger = Substitute.For<ILogger<DiagnosticsService>>();

        // Act
        Action act = () => new DiagnosticsService(mockLogger, _mockChecks, nullHardware);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("hardwareDetection");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_WithNoChecks_ReturnsEmptyList()
    {
        // Act
        var results = await _service.RunAllChecksAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_WithMultipleChecks_RunsAllChecks()
    {
        // Arrange
        var check1 = Substitute.For<IDiagnosticCheck>();
        var check2 = Substitute.For<IDiagnosticCheck>();

        check1.RunAsync(Arg.Any<CancellationToken>()).Returns(new DiagnosticResult
        {
            CheckName = "Check1",
            Status = DiagnosticStatus.Passed,
            Message = "Check 1 passed",
            Details = new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            DurationMs = 100
        });

        check2.RunAsync(Arg.Any<CancellationToken>()).Returns(new DiagnosticResult
        {
            CheckName = "Check2",
            Status = DiagnosticStatus.Passed,
            Message = "Check 2 passed",
            Details = new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            DurationMs = 150
        });

        _mockChecks.Add(check1);
        _mockChecks.Add(check2);

        // Act
        var results = await _service.RunAllChecksAsync();

        // Assert
        results.Should().HaveCount(2);
        await check1.Received(1).RunAsync(Arg.Any<CancellationToken>());
        await check2.Received(1).RunAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_WithFailingCheck_ReturnsFailedResult()
    {
        // Arrange
        var check = Substitute.For<IDiagnosticCheck>();
        check.RunAsync(Arg.Any<CancellationToken>()).Returns<DiagnosticResult>(x =>
            throw new InvalidOperationException("Check failed"));

        _mockChecks.Add(check);

        // Act
        var results = await _service.RunAllChecksAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Status.Should().Be(DiagnosticStatus.Failed);
        results[0].Message.Should().Contain("Check failed with exception");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.RunAllChecksAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunCheckAsync_WithNullCheckName_ThrowsArgumentNullException()
    {
        // Arrange
        string nullCheckName = null!;

        // Act
        Func<Task> act = async () => await _service.RunCheckAsync(nullCheckName);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("checkName");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunCheckAsync_WithNonExistentCheck_ReturnsFailedResult()
    {
        // Act
        var result = await _service.RunCheckAsync("NonExistentCheck");

        // Assert
        result.Status.Should().Be(DiagnosticStatus.Failed);
        result.Message.Should().Contain("not found");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunCheckAsync_WithExistingCheck_RunsCheckSuccessfully()
    {
        // Arrange
        var check = Substitute.For<IDiagnosticCheck>();
        var checkType = check.GetType();

        check.RunAsync().Returns(new DiagnosticResult
        {
            CheckName = "TestCheck",
            Status = DiagnosticStatus.Passed,
            Message = "Check passed",
            Details = new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            DurationMs = 100
        });

        _mockChecks.Add(check);

        // Act
        var result = await _service.RunCheckAsync(checkType.Name);

        // Assert
        result.Status.Should().Be(DiagnosticStatus.Passed);
        await check.Received(1).RunAsync();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunCheckAsync_WithFailingCheck_ReturnsFailedResult()
    {
        // Arrange
        var check = Substitute.For<IDiagnosticCheck>();
        var checkType = check.GetType();

        check.RunAsync().Returns<DiagnosticResult>(x =>
            throw new InvalidOperationException("Check failed"));

        _mockChecks.Add(check);

        // Act
        var result = await _service.RunCheckAsync(checkType.Name);

        // Assert
        result.Status.Should().Be(DiagnosticStatus.Failed);
        result.Message.Should().Contain("Check failed with exception");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunCheckAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.RunCheckAsync("SomeCheck", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public void GetHardwareCapabilities_WhenCalled_ReturnsCapabilities()
    {
        // Arrange
        var expectedCapabilities = new HardwareCapabilities
        {
            ProcessorCount = 8,
            TotalMemoryMB = 16384,
            AvailableMemoryMB = 8192,
            ScreenResolution = "1920x1080",
            Platform = "Windows",
            HasCamera = false,
            HasBarcodeScanner = true
        };

        _mockHardwareDetection.DetectCapabilities().Returns(expectedCapabilities);

        // Act
        var capabilities = _service.GetHardwareCapabilities();

        // Assert
        capabilities.Should().BeEquivalentTo(expectedCapabilities);
        _mockHardwareDetection.Received(1).DetectCapabilities();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_WithMixedResults_ReturnsAllResults()
    {
        // Arrange
        var passCheck = Substitute.For<IDiagnosticCheck>();
        var failCheck = Substitute.For<IDiagnosticCheck>();

        passCheck.RunAsync(Arg.Any<CancellationToken>()).Returns(new DiagnosticResult
        {
            CheckName = "PassCheck",
            Status = DiagnosticStatus.Passed,
            Message = "Passed",
            Details = new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            DurationMs = 100
        });

        failCheck.RunAsync(Arg.Any<CancellationToken>()).Returns(new DiagnosticResult
        {
            CheckName = "FailCheck",
            Status = DiagnosticStatus.Failed,
            Message = "Failed",
            Details = new Dictionary<string, object>(),
            Timestamp = DateTimeOffset.UtcNow,
            DurationMs = 150
        });

        _mockChecks.Add(passCheck);
        _mockChecks.Add(failCheck);

        // Act
        var results = await _service.RunAllChecksAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.Status == DiagnosticStatus.Passed);
        results.Should().Contain(r => r.Status == DiagnosticStatus.Failed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "Diagnostics")]
    public async Task RunAllChecksAsync_ChecksRunInParallel_CompletesEfficiently()
    {
        // Arrange
        var slowCheck1 = Substitute.For<IDiagnosticCheck>();
        var slowCheck2 = Substitute.For<IDiagnosticCheck>();

        slowCheck1.RunAsync(Arg.Any<CancellationToken>()).Returns(async x =>
        {
            await Task.Delay(100);
            return new DiagnosticResult
            {
                CheckName = "SlowCheck1",
                Status = DiagnosticStatus.Passed,
                Message = "Passed",
                Details = new Dictionary<string, object>(),
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = 100
            };
        });

        slowCheck2.RunAsync(Arg.Any<CancellationToken>()).Returns(async x =>
        {
            await Task.Delay(100);
            return new DiagnosticResult
            {
                CheckName = "SlowCheck2",
                Status = DiagnosticStatus.Passed,
                Message = "Passed",
                Details = new Dictionary<string, object>(),
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = 100
            };
        });

        _mockChecks.Add(slowCheck1);
        _mockChecks.Add(slowCheck2);

        // Act
        var startTime = DateTimeOffset.UtcNow;
        var results = await _service.RunAllChecksAsync();
        var duration = DateTimeOffset.UtcNow - startTime;

        // Assert
        results.Should().HaveCount(2);
        duration.TotalMilliseconds.Should().BeLessThan(200, "checks should run in parallel");
    }
}
