using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Tests.unit.Models.Diagnostics;

public class DiagnosticExportTests
{
    [Fact]
    public void Should_Create_Valid_DiagnosticExport()
    {
        // Arrange & Act
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "1.0.0",
            Platform = "Windows",
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            BootTimeline = null,
            RecentErrors = new List<ErrorEntry>(),
            ConnectionStats = null,
            EnvironmentVariables = new Dictionary<string, string>(),
            RecentLogEntries = new List<string>()
        };

        // Assert
        export.Should().NotBeNull();
        export.ApplicationVersion.Should().Be("1.0.0");
        export.Platform.Should().Be("Windows");
        export.CurrentPerformance.Should().NotBeNull();
        export.RecentErrors.Should().NotBeNull().And.BeEmpty();
        export.EnvironmentVariables.Should().NotBeNull().And.BeEmpty();
        export.RecentLogEntries.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Validate_Should_Throw_When_ApplicationVersion_Empty()
    {
        // Arrange
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "",
            Platform = "Windows",
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            RecentErrors = new List<ErrorEntry>(),
            EnvironmentVariables = new Dictionary<string, string>(),
            RecentLogEntries = new List<string>()
        };

        // Act & Assert
        export.Invoking(e => e.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Application version*");
    }

    [Fact]
    public void Validate_Should_Throw_When_Platform_Empty()
    {
        // Arrange
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "1.0.0",
            Platform = "",
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            RecentErrors = new List<ErrorEntry>(),
            EnvironmentVariables = new Dictionary<string, string>(),
            RecentLogEntries = new List<string>()
        };

        // Act & Assert
        export.Invoking(e => e.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Platform*");
    }

    [Theory]
    [InlineData("Linux")]
    [InlineData("MacOS")]
    [InlineData("iOS")]
    [InlineData("Unknown")]
    public void Validate_Should_Throw_When_Platform_Invalid(string platform)
    {
        // Arrange
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "1.0.0",
            Platform = platform,
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            RecentErrors = new List<ErrorEntry>(),
            EnvironmentVariables = new Dictionary<string, string>(),
            RecentLogEntries = new List<string>()
        };

        // Act & Assert
        export.Invoking(e => e.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*must be 'Windows' or 'Android'*");
    }

    [Theory]
    [InlineData("Windows")]
    [InlineData("Android")]
    public void Validate_Should_Pass_For_Valid_Platforms(string platform)
    {
        // Arrange
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "1.0.0",
            Platform = platform,
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            RecentErrors = new List<ErrorEntry>(),
            EnvironmentVariables = new Dictionary<string, string>(),
            RecentLogEntries = new List<string>()
        };

        // Act & Assert
        export.Invoking(e => e.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Should_Support_Full_Diagnostic_Data()
    {
        // Arrange
        var bootTimeline = new BootTimeline
        {
            BootStartTime = DateTime.UtcNow.AddMinutes(-5),
            Stage0 = new Stage0Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            Stage1 = new Stage1Info
            {
                Duration = TimeSpan.FromSeconds(2),
                Success = true,
                ServiceTimings = new List<ServiceInitInfo>
                {
                    new() { ServiceName = "ConfigService", Duration = TimeSpan.FromMilliseconds(500), Success = true }
                }
            },
            Stage2 = new Stage2Info { Duration = TimeSpan.FromSeconds(1), Success = true },
            TotalBootTime = TimeSpan.FromSeconds(4)
        };

        var errors = new List<ErrorEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Severity = ErrorSeverity.Warning,
                Category = "Network",
                Message = "Slow response time detected",
                ContextData = new Dictionary<string, string> { { "ResponseTime", "5000ms" } }
            }
        };

        var connectionStats = new ConnectionPoolStats
        {
            Timestamp = DateTime.UtcNow,
            MySqlPool = new MySqlPoolStats
            {
                TotalConnections = 10,
                ActiveConnections = 3,
                IdleConnections = 7,
                WaitingRequests = 0,
                AverageWaitTime = TimeSpan.FromMilliseconds(50)
            },
            HttpPool = new HttpPoolStats
            {
                TotalConnections = 5,
                ActiveConnections = 2,
                IdleConnections = 3,
                AverageResponseTime = TimeSpan.FromMilliseconds(200)
            }
        };

        // Act
        var export = new DiagnosticExport
        {
            ExportTime = DateTime.UtcNow,
            ApplicationVersion = "1.0.0",
            Platform = "Windows",
            CurrentPerformance = new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercent = 25.5,
                MemoryUsageMB = 512,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 2,
                ThreadCount = 20,
                Uptime = TimeSpan.FromMinutes(5)
            },
            BootTimeline = bootTimeline,
            RecentErrors = errors,
            ConnectionStats = connectionStats,
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "MTM_ENVIRONMENT", "Development" },
                { "MTM_DATABASE_SERVER", "localhost" }
            },
            RecentLogEntries = new List<string>
            {
                "[INFO] Application started",
                "[DEBUG] Configuration loaded"
            }
        };

        // Assert
        export.BootTimeline.Should().NotBeNull();
        export.RecentErrors.Should().HaveCount(1);
        export.ConnectionStats.Should().NotBeNull();
        export.EnvironmentVariables.Should().HaveCount(2);
        export.RecentLogEntries.Should().HaveCount(2);
        export.Invoking(e => e.Validate()).Should().NotThrow();
    }
}
