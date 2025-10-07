using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.unit.Services;

/// <summary>
/// Unit tests for ExportService implementation.
/// Tests PII sanitization, JSON/Markdown export, file path validation, and large file handling.
/// </summary>
public class ExportServiceTests
{
    private readonly ILogger<ExportService> _logger;
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly IDiagnosticsServiceExtensions _diagnosticsService;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _logger = Substitute.For<ILogger<ExportService>>();
        _performanceService = Substitute.For<IPerformanceMonitoringService>();
        _diagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();
        _service = new ExportService(_logger, _performanceService, _diagnosticsService);

        // Setup default mocks
        _performanceService.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 150,
                ThreadCount = 20,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                Uptime = TimeSpan.FromMinutes(30)
            });

        _performanceService.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<PerformanceSnapshot>());

        _diagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())
            .Returns(new BootTimeline
            {
                BootStartTime = DateTime.UtcNow,
                Stage0 = new Stage0Info
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Success = true
                },
                Stage1 = new Stage1Info
                {
                    Duration = TimeSpan.FromSeconds(2),
                    Success = true,
                    ServiceTimings = new List<ServiceInitInfo>()
                },
                Stage2 = new Stage2Info
                {
                    Duration = TimeSpan.FromSeconds(1),
                    Success = true
                },
                TotalBootTime = TimeSpan.FromSeconds(4)
            });

        _diagnosticsService.GetRecentErrorsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<ErrorEntry>());

        _diagnosticsService.GetConnectionPoolStatsAsync(Arg.Any<CancellationToken>())
            .Returns(new ConnectionPoolStats
            {
                Timestamp = DateTime.UtcNow,
                MySqlPool = new MySqlPoolStats
                {
                    TotalConnections = 10,
                    ActiveConnections = 3,
                    IdleConnections = 7,
                    WaitingRequests = 0,
                    AverageWaitTime = TimeSpan.Zero
                },
                HttpPool = new HttpPoolStats
                {
                    TotalConnections = 5,
                    ActiveConnections = 2,
                    IdleConnections = 3,
                    AverageResponseTime = TimeSpan.FromMilliseconds(200)
                }
            });
    }

    [Theory]
    [InlineData("password: secret123")]
    [InlineData("Password=MyP@ssw0rd")]
    [InlineData("PASSWORD: TopSecret!")]
    public void SanitizePii_Should_Redact_Passwords(string input)
    {
        // Act
        var result = _service.SanitizePii(input);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("secret");
        result.Should().NotContain("MyP@ssw0rd");
        result.Should().NotContain("TopSecret");
    }

    [Theory]
    [InlineData("api_token: abc123xyz")]
    [InlineData("apikey=xyzABC789")]
    [InlineData("secret: my-secret-value")]
    [InlineData("TOKEN=Bearer xyz123")]
    public void SanitizePii_Should_Redact_Tokens_And_Secrets(string input)
    {
        // Act
        var result = _service.SanitizePii(input);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("abc123xyz");
        result.Should().NotContain("xyzABC789");
        result.Should().NotContain("my-secret-value");
        result.Should().NotContain("Bearer xyz123");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("john.doe@company.co.uk")]
    [InlineData("Contact: admin@test.org for help")]
    public void SanitizePii_Should_Redact_Email_Addresses(string input)
    {
        // Act
        var result = _service.SanitizePii(input);

        // Assert
        result.Should().Contain("[EMAIL_REDACTED]");
        result.Should().NotContain("@example.com");
        result.Should().NotContain("@company.co.uk");
        result.Should().NotContain("@test.org");
    }

    [Theory]
    [InlineData("connectionstring: Server=localhost;Database=test;")]
    [InlineData("Connection=Data Source=myserver;User=admin;")]
    public void SanitizePii_Should_Redact_Connection_Strings(string input)
    {
        // Act
        var result = _service.SanitizePii(input);

        // Assert
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("Server=localhost");
        result.Should().NotContain("Data Source=myserver");
    }

    [Fact]
    public void SanitizePii_Should_Handle_Null_Or_Empty_Input()
    {
        // Act & Assert
        _service.SanitizePii(null!).Should().BeNull();
        _service.SanitizePii("").Should().Be("");
        _service.SanitizePii("   ").Should().Be("   ");
    }

    [Fact]
    public void SanitizePii_Should_Handle_Multiple_PII_Types_In_One_String()
    {
        // Arrange
        var input = "User: admin@example.com, Password: secret123, Token: abc-xyz-789";

        // Act
        var result = _service.SanitizePii(input);

        // Assert
        result.Should().Contain("[EMAIL_REDACTED]");
        result.Should().Contain("[REDACTED]");
        result.Should().NotContain("admin@example.com");
        result.Should().NotContain("secret123");
        result.Should().NotContain("abc-xyz-789");
    }

    [Fact]
    public async Task CreateExportAsync_Should_Return_Valid_Export()
    {
        // Act
        var export = await _service.CreateExportAsync();

        // Assert
        export.Should().NotBeNull();
        export.ExportTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        export.Platform.Should().BeOneOf("Windows", "Android");
        export.ApplicationVersion.Should().NotBeNullOrWhiteSpace();
        export.CurrentPerformance.Should().NotBeNull();
        export.RecentErrors.Should().NotBeNull();
        export.EnvironmentVariables.Should().NotBeNull();
        export.RecentLogEntries.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateExportAsync_Should_Sanitize_Environment_Variables()
    {
        // Arrange
        Environment.SetEnvironmentVariable("TEST_PASSWORD", "secret123");
        Environment.SetEnvironmentVariable("TEST_API_KEY", "abc-xyz-789");

        try
        {
            // Act
            var export = await _service.CreateExportAsync();

            // Assert
            export.EnvironmentVariables.Should().ContainKey("TEST_PASSWORD");
            export.EnvironmentVariables["TEST_PASSWORD"].Should().Contain("[REDACTED]");

            export.EnvironmentVariables.Should().ContainKey("TEST_API_KEY");
            export.EnvironmentVariables["TEST_API_KEY"].Should().Contain("[REDACTED]");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("TEST_PASSWORD", null);
            Environment.SetEnvironmentVariable("TEST_API_KEY", null);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Create_Valid_Json_File()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.json");

        try
        {
            // Act
            var bytesWritten = await _service.ExportToJsonAsync(tempFile);

            // Assert
            bytesWritten.Should().BeGreaterThan(0);
            File.Exists(tempFile).Should().BeTrue();

            var jsonContent = await File.ReadAllTextAsync(tempFile);
            jsonContent.Should().NotBeNullOrWhiteSpace();

            // Verify it's valid JSON
            var export = JsonSerializer.Deserialize<DiagnosticExport>(jsonContent);
            export.Should().NotBeNull();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Throw_For_Invalid_Path()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.ExportToJsonAsync(""))
            .Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");

        await FluentActions.Invoking(() => _service.ExportToJsonAsync(null!))
            .Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Report_Progress()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.json");
        var progressReports = new List<int>();
        var progress = new Progress<int>(p => progressReports.Add(p));

        try
        {
            // Act
            await _service.ExportToJsonAsync(tempFile, CancellationToken.None, progress);

            // Assert
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(0);   // Start
            progressReports.Should().Contain(100); // Complete
            progressReports.Should().BeInAscendingOrder();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToMarkdownAsync_Should_Create_Valid_Markdown_File()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.md");

        try
        {
            // Act
            var bytesWritten = await _service.ExportToMarkdownAsync(tempFile);

            // Assert
            bytesWritten.Should().BeGreaterThan(0);
            File.Exists(tempFile).Should().BeTrue();

            var markdownContent = await File.ReadAllTextAsync(tempFile);
            markdownContent.Should().Contain("# Diagnostic Export");
            markdownContent.Should().Contain("## Current Performance");
            markdownContent.Should().Contain("**Exported At**:");
            markdownContent.Should().Contain("**Platform**:");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToMarkdownAsync_Should_Report_Progress()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.md");
        var progressReports = new List<int>();
        var progress = new Progress<int>(p => progressReports.Add(p));

        try
        {
            // Act
            await _service.ExportToMarkdownAsync(tempFile, CancellationToken.None, progress);

            // Assert
            progressReports.Should().NotBeEmpty();
            progressReports.Should().Contain(0);   // Start
            progressReports.Should().Contain(100); // Complete
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void ValidateExportPath_Should_Return_True_For_Valid_Paths()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.json");

        // Act
        var isValid = _service.ValidateExportPath(tempPath);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateExportPath_Should_Return_False_For_Invalid_Paths()
    {
        // Act & Assert
        _service.ValidateExportPath("").Should().BeFalse();
        _service.ValidateExportPath(null!).Should().BeFalse();
        _service.ValidateExportPath("   ").Should().BeFalse();
    }

    [Fact]
    public void ValidateExportPath_Should_Return_False_For_Read_Only_Files()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"readonly-{Guid.NewGuid()}.json");
        File.WriteAllText(tempFile, "test");
        var fileInfo = new FileInfo(tempFile);
        fileInfo.IsReadOnly = true;

        try
        {
            // Act
            var isValid = _service.ValidateExportPath(tempFile);

            // Assert
            isValid.Should().BeFalse();
        }
        finally
        {
            // Cleanup
            fileInfo.IsReadOnly = false;
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Handle_Cancellation()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test-export-{Guid.NewGuid()}.json");
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await FluentActions.Invoking(() => _service.ExportToJsonAsync(tempFile, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();

        // Cleanup
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CreateExportAsync_Should_Handle_Cancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateExportAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void ValidateExportPath_Should_Create_Directory_If_Needed()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), $"test-dir-{Guid.NewGuid()}");
        var testFile = Path.Combine(testDir, "export.json");

        try
        {
            // Act
            var isValid = _service.ValidateExportPath(testFile);

            // Assert
            isValid.Should().BeTrue();
            Directory.Exists(testDir).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
