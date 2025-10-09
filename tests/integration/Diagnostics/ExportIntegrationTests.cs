using System.IO;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Extensions;
using MTM_Template_Application.Models.Diagnostics;
using MTM_Template_Application.Services.Diagnostics;
using NSubstitute;
using Xunit;

namespace MTM_Template_Tests.Integration.Diagnostics;

/// <summary>
/// Integration tests for diagnostic export service with PII sanitization.
/// Tests T025: Complete export workflow with JSON serialization and file I/O.
/// </summary>
[Trait("Category", "Integration")]
public class ExportIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IExportService _exportService;
    private readonly string _testOutputDir;
    private readonly List<string> _testFilesToCleanup = new();

    public ExportIntegrationTests()
    {
        // Setup DI container
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Mock dependencies
        var mockPerformanceService = Substitute.For<IPerformanceMonitoringService>();
        mockPerformanceService.GetCurrentSnapshotAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 8.5,
                MemoryUsageMB = 92,
                GcGen0Collections = 15,
                GcGen1Collections = 3,
                GcGen2Collections = 1,
                ThreadCount = 28,
                Uptime = TimeSpan.FromMinutes(12)
            }));

        mockPerformanceService.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<PerformanceSnapshot>)new List<PerformanceSnapshot>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow.AddMinutes(-2),
                    CpuUsagePercent = 5.2,
                    MemoryUsageMB = 85,
                    GcGen0Collections = 10,
                    GcGen1Collections = 2,
                    GcGen2Collections = 1,
                    ThreadCount = 25,
                    Uptime = TimeSpan.FromMinutes(10)
                }
            }));

        var mockDiagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();
        mockDiagnosticsService.GetBootTimelineAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BootTimeline
            {
                BootStartTime = DateTime.UtcNow.AddMinutes(-10),
                Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
                Stage1 = new Stage1Info { Duration = TimeSpan.FromMilliseconds(2500), Success = true, ServiceTimings = new List<ServiceInitInfo>() },
                Stage2 = new Stage2Info { Duration = TimeSpan.FromMilliseconds(500), Success = true },
                TotalBootTime = TimeSpan.FromSeconds(4)
            }));

        mockDiagnosticsService.GetRecentErrorsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<ErrorEntry>)new List<ErrorEntry>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Category = "Database",
                    Message = "Connection timeout after 30s",
                    Severity = ErrorSeverity.Error,
                    StackTrace = "   at MySql.Data.MySqlClient.Open()\n   at MyApp.DatabaseService.ConnectAsync()",
                    ContextData = new Dictionary<string, string>
                    {
                        { "Server", "localhost" },
                        { "Database", "test_db" }
                    }
                }
            }));

        mockDiagnosticsService.GetConnectionPoolStatsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ConnectionPoolStats
            {
                Timestamp = DateTime.UtcNow,
                MySqlPool = new MySqlPoolStats
                {
                    TotalConnections = 10,
                    ActiveConnections = 5,
                    IdleConnections = 5,
                    WaitingRequests = 0,
                    AverageWaitTime = TimeSpan.FromMilliseconds(15)
                },
                HttpPool = new HttpPoolStats
                {
                    TotalConnections = 20,
                    ActiveConnections = 3,
                    IdleConnections = 17,
                    AverageResponseTime = TimeSpan.FromMilliseconds(10)
                }
            }));

        services.AddSingleton(mockPerformanceService);
        services.AddSingleton(mockDiagnosticsService);

        // Register ExportService (directly, not via AddDebugTerminalServices which requires IBootOrchestrator)
        services.AddTransient<IExportService, ExportService>();

        _serviceProvider = services.BuildServiceProvider();
        _exportService = _serviceProvider.GetRequiredService<IExportService>();

        // Create temp output directory for test files
        _testOutputDir = Path.Combine(Path.GetTempPath(), "MTM_ExportTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public async Task CreateExportAsync_Should_Aggregate_All_Diagnostic_Data()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var export = await _exportService.CreateExportAsync(cancellationToken);

        // Assert
        export.Should().NotBeNull();
        export.ExportTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        export.CurrentPerformance.Should().NotBeNull("should include current performance snapshot");
        export.BootTimeline.Should().NotBeNull("should include boot timeline");
        export.RecentErrors.Should().NotBeNull().And.NotBeEmpty("should include recent errors");
        export.ConnectionStats.Should().NotBeNull("should include connection pool stats");
        export.EnvironmentVariables.Should().NotBeNull("should include environment variables");
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Create_Valid_JSON_File()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var fileName = $"diagnostic_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_testOutputDir, fileName);
        _testFilesToCleanup.Add(filePath);

        // Act
        var bytesWritten = await _exportService.ExportToJsonAsync(filePath, cancellationToken);

        // Assert
        File.Exists(filePath).Should().BeTrue("JSON file should be created");
        bytesWritten.Should().BeGreaterThan(0, "should write bytes to file");

        // Verify valid JSON
        var jsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        jsonContent.Should().NotBeNullOrWhiteSpace();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var deserializedExport = JsonSerializer.Deserialize<DiagnosticExport>(jsonContent, jsonOptions);
        deserializedExport.Should().NotBeNull("JSON should deserialize to DiagnosticExport");
        deserializedExport!.CurrentPerformance.Should().NotBeNull();
        deserializedExport.RecentErrors.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Complete_Within_2_Seconds()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var fileName = $"diagnostic_export_perf_test_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_testOutputDir, fileName);
        _testFilesToCleanup.Add(filePath);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _exportService.ExportToJsonAsync(filePath, cancellationToken);
        stopwatch.Stop();

        // Assert
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2), "export should complete <2s (acceptance criteria)");
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task CreateExportAsync_Should_Sanitize_Environment_Variables()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Set environment variables with sensitive data
        Environment.SetEnvironmentVariable("TEST_PASSWORD", "secret123");
        Environment.SetEnvironmentVariable("TEST_API_KEY", "abc123xyz");
        Environment.SetEnvironmentVariable("TEST_USER_EMAIL", "user@example.com");

        try
        {
            // Act
            var export = await _exportService.CreateExportAsync(cancellationToken);

            // Assert - Verify PII sanitization
            var passwordVar = export.EnvironmentVariables.FirstOrDefault(kv => kv.Key == "TEST_PASSWORD");
            passwordVar.Should().NotBeNull("TEST_PASSWORD should be included");
            passwordVar.Value.Should().Contain("[REDACTED]", "password should be redacted");
            passwordVar.Value.Should().NotContain("secret123", "plaintext password should be removed");

            var apiKeyVar = export.EnvironmentVariables.FirstOrDefault(kv => kv.Key == "TEST_API_KEY");
            apiKeyVar.Should().NotBeNull("TEST_API_KEY should be included");
            apiKeyVar.Value.Should().Contain("[REDACTED]", "API key should be redacted");
            apiKeyVar.Value.Should().NotContain("abc123xyz", "plaintext API key should be removed");

            var emailVar = export.EnvironmentVariables.FirstOrDefault(kv => kv.Key == "TEST_USER_EMAIL");
            emailVar.Should().NotBeNull("TEST_USER_EMAIL should be included");
            emailVar.Value.Should().Contain("[EMAIL_REDACTED]", "email should be redacted");
            emailVar.Value.Should().NotContain("user@example.com", "plaintext email should be removed");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("TEST_PASSWORD", null);
            Environment.SetEnvironmentVariable("TEST_API_KEY", null);
            Environment.SetEnvironmentVariable("TEST_USER_EMAIL", null);
        }
    }

    [Fact]
    public async Task CreateExportAsync_Should_Sanitize_Stack_Traces()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var export = await _exportService.CreateExportAsync(cancellationToken);

        // Assert - Verify stack trace sanitization (if error contains sensitive data)
        var errorWithStackTrace = export.RecentErrors.FirstOrDefault();
        if (errorWithStackTrace != null && !string.IsNullOrWhiteSpace(errorWithStackTrace.StackTrace))
        {
            // Stack trace should not contain email addresses
            errorWithStackTrace.StackTrace.Should().NotMatchRegex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
                "stack trace should not contain email addresses");
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Create_Directory_If_Not_Exists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var newDirPath = Path.Combine(_testOutputDir, "NewSubDir");
        var fileName = "diagnostic_export.json";
        var filePath = Path.Combine(newDirPath, fileName);
        _testFilesToCleanup.Add(filePath);

        // Act
        await _exportService.ExportToJsonAsync(filePath, cancellationToken);

        // Assert
        Directory.Exists(newDirPath).Should().BeTrue("directory should be created");
        File.Exists(filePath).Should().BeTrue("file should be created in new directory");
    }

    [Fact]
    public async Task ExportToJsonAsync_Should_Use_Async_File_IO()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var export = await _exportService.CreateExportAsync(cancellationToken);

        var fileName = $"diagnostic_export_async_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_testOutputDir, fileName);
        _testFilesToCleanup.Add(filePath);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _exportService.ExportToJsonAsync(filePath, cancellationToken);
        stopwatch.Stop();

        // Assert - Async I/O should not block (no UI thread blocking)
        // This test verifies the method completes and returns successfully
        File.Exists(filePath).Should().BeTrue();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3), "async I/O should not cause excessive delays");
    }

    [Fact]
    public async Task CreateExportAsync_Should_Handle_Missing_Optional_Data_Gracefully()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Create service with empty dependencies
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));

        var mockPerformanceService = Substitute.For<IPerformanceMonitoringService>();
        mockPerformanceService.GetRecentSnapshotsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<PerformanceSnapshot>)new List<PerformanceSnapshot>()));

        var mockDiagnosticsService = Substitute.For<IDiagnosticsServiceExtensions>();
        mockDiagnosticsService.GetRecentErrorsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((IReadOnlyList<ErrorEntry>)new List<ErrorEntry>()));

        services.AddSingleton(mockPerformanceService);
        services.AddSingleton(mockDiagnosticsService);

        // Register ExportService directly (not via AddDebugTerminalServices)
        services.AddTransient<IExportService, ExportService>();

        var serviceProvider = services.BuildServiceProvider();
        var exportService = serviceProvider.GetRequiredService<IExportService>();

        // Act
        var export = await exportService.CreateExportAsync(cancellationToken);

        // Assert - Should not throw, should handle empty data
        export.Should().NotBeNull();
        export.CurrentPerformance.Should().NotBeNull();
        export.RecentErrors.Should().NotBeNull();
    }

    public void Dispose()
    {
        // Cleanup test files
        foreach (var filePath in _testFilesToCleanup)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // Cleanup test directory
        try
        {
            if (Directory.Exists(_testOutputDir))
            {
                Directory.Delete(_testOutputDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
