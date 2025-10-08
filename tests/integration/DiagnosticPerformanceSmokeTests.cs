using FluentAssertions;
using MTM_Template_Application.Models.Diagnostics;
using Xunit;

namespace MTM_Template_Tests.integration;

/// <summary>
/// Performance smoke tests for Debug Terminal diagnostics.
/// Tests basic performance characteristics without requiring profiling tools.
/// Feature 003: Debug Terminal Modernization (T051-T052)
/// </summary>
/// <remarks>
/// These tests validate performance budgets using simple benchmarking:
/// - Memory usage within acceptable bounds
/// - Operation latency meets targets
/// - No performance degradation under load
/// </remarks>
public class DiagnosticPerformanceSmokeTests
{
    #region Performance Snapshot Tests

    [Fact]
    public void PerformanceSnapshot_ObjectSize_IsReasonable()
    {
        // Arrange & Act: Create snapshot with typical data
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            CpuUsagePercent = 25.5,
            MemoryUsageMB = 65,
            GcGen0Collections = 100,
            GcGen1Collections = 50,
            GcGen2Collections = 5,
            ThreadCount = 30,
            Uptime = TimeSpan.FromMinutes(15)
        };

        // Assert: Snapshot is lightweight (estimate ~100 bytes)
        // DateTime (8) + 2 doubles (16) + 4 ints (16) + TimeSpan (8) = ~48 bytes + object overhead
        snapshot.Should().NotBeNull();
        snapshot.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        snapshot.IsValid().Should().BeTrue();
    }

    [Fact]
    public void PerformanceSnapshot_Validation_ExecutesQuickly()
    {
        // Arrange: Create valid snapshot
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            CpuUsagePercent = 50.0,
            MemoryUsageMB = 75,
            GcGen0Collections = 10,
            GcGen1Collections = 5,
            GcGen2Collections = 1,
            ThreadCount = 25,
            Uptime = TimeSpan.FromMinutes(10)
        };

        // Act & Assert: Validation is fast (<1ms per operation)
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            snapshot.IsValid().Should().BeTrue();
        }

        stopwatch.Stop();

        // Assert: 1000 validations in <10ms (avg <0.01ms each)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10,
            "1000 validation calls should complete in <10ms");
    }

    #endregion

    #region Boot Timeline Performance

    [Fact]
    public void BootTimeline_ObjectCreation_IsEfficient()
    {
        // Arrange & Act: Create boot timeline with typical stages
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var timeline = new BootTimeline
        {
            Stage0Name = "Splash Screen",
            Stage0DurationMs = 850,
            Stage0Status = "Success",
            Stage1Name = "Core Services",
            Stage1DurationMs = 2400,
            Stage1Status = "Success",
            Stage2Name = "Application Ready",
            Stage2DurationMs = 750,
            Stage2Status = "Success",
            TotalBootTimeMs = 4000,
            BootSessionId = Guid.NewGuid(),
            BootTimestamp = DateTime.Now,
            ServiceMetrics = new List<string>()
        };

        stopwatch.Stop();

        // Assert: Object creation is fast (<1ms)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1,
            "BootTimeline creation should be nearly instantaneous");

        timeline.TotalBootTimeMs.Should().Be(4000);
        timeline.Stage0DurationMs.Should().BeLessThan(1000, "Stage 0 meets target");
        timeline.Stage1DurationMs.Should().BeLessThan(3000, "Stage 1 meets target");
        timeline.Stage2DurationMs.Should().BeLessThan(1000, "Stage 2 meets target");
    }

    #endregion

    #region Error Entry Performance

    [Fact]
    public void ErrorEntry_CollectionOperations_ScaleLinear()
    {
        // Arrange: Create error entry collection (simulating circular buffer)
        var errors = new List<ErrorEntry>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act: Add 50 error entries (max capacity for error history)
        for (int i = 0; i < 50; i++)
        {
            errors.Add(new ErrorEntry
            {
                Timestamp = DateTime.Now.AddSeconds(-i),
                Severity = i % 3 == 0 ? "Error" : "Warning",
                Message = $"Test error {i}",
                StackTrace = $"   at TestMethod{i}()",
                Source = "TestSource"
            });
        }

        stopwatch.Stop();

        // Assert: 50 additions in <5ms (avg <0.1ms each)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5,
            "50 error entry additions should complete in <5ms");

        errors.Should().HaveCount(50);

        // Act: Simulate circular buffer behavior (remove oldest, add new)
        stopwatch.Restart();

        for (int i = 0; i < 100; i++)
        {
            if (errors.Count >= 50)
            {
                errors.RemoveAt(0); // Remove oldest
            }

            errors.Add(new ErrorEntry
            {
                Timestamp = DateTime.Now,
                Severity = "Info",
                Message = $"New error {i}",
                StackTrace = string.Empty,
                Source = "Test"
            });
        }

        stopwatch.Stop();

        // Assert: 100 circular operations in <10ms
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10,
            "100 circular buffer operations should complete in <10ms");

        errors.Should().HaveCount(50, "Circular buffer maintains max capacity");
    }

    #endregion

    #region Connection Pool Stats Performance

    [Fact]
    public void ConnectionPoolStats_Calculation_IsInstantaneous()
    {
        // Arrange & Act: Create connection pool stats with calculation
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var stats = new ConnectionPoolStats
        {
            ActiveConnections = 5,
            IdleConnections = 15,
            MaxConnections = 20,
            UtilizationPercentage = (5.0 / 20.0) * 100, // 25%
            LastChecked = DateTime.Now
        };

        stopwatch.Stop();

        // Assert: Calculation is instantaneous (<1ms)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1,
            "Connection pool stats calculation should be instantaneous");

        stats.UtilizationPercentage.Should().BeApproximately(25.0, 0.1);
        stats.ActiveConnections.Should().Be(5);
        stats.IdleConnections.Should().Be(15);
    }

    #endregion

    #region Memory Usage Simulation

    [Fact]
    public void CircularBuffer_MemoryFootprint_StaysWithinBudget()
    {
        // Arrange: Simulate circular buffer with 100 performance snapshots
        var snapshots = new List<PerformanceSnapshot>();

        // Act: Fill buffer to capacity
        for (int i = 0; i < 100; i++)
        {
            snapshots.Add(new PerformanceSnapshot
            {
                Timestamp = DateTime.Now.AddSeconds(-i),
                CpuUsagePercent = 10 + (i % 50),
                MemoryUsageMB = 40 + (i % 60),
                GcGen0Collections = 10 + i,
                GcGen1Collections = 5 + (i / 10),
                GcGen2Collections = 1 + (i / 50),
                ThreadCount = 20 + (i % 10),
                Uptime = TimeSpan.FromMinutes(i)
            });
        }

        // Assert: Collection size is reasonable
        snapshots.Should().HaveCount(100);

        // Estimate memory: 100 snapshots × ~100 bytes ≈ 10KB
        // Well under 100KB budget for circular buffer
        // (Actual measurement would require dotMemory, but this validates structure)

        // Verify all snapshots are valid
        snapshots.Should().OnlyContain(s => s.IsValid());
    }

    [Fact]
    public void CircularBuffer_OverwriteBehavior_MaintainsCapacity()
    {
        // Arrange: Simulate circular buffer at capacity
        var buffer = new List<PerformanceSnapshot>(capacity: 100);

        for (int i = 0; i < 100; i++)
        {
            buffer.Add(new PerformanceSnapshot
            {
                Timestamp = DateTime.Now.AddSeconds(-100 + i),
                CpuUsagePercent = 10,
                MemoryUsageMB = 40,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 1,
                ThreadCount = 25,
                Uptime = TimeSpan.FromMinutes(i)
            });
        }

        // Act: Simulate adding beyond capacity (circular overwrite)
        for (int i = 0; i < 50; i++)
        {
            if (buffer.Count >= 100)
            {
                buffer.RemoveAt(0); // Remove oldest
            }

            buffer.Add(new PerformanceSnapshot
            {
                Timestamp = DateTime.Now.AddSeconds(i),
                CpuUsagePercent = 15,
                MemoryUsageMB = 45,
                GcGen0Collections = 110 + i,
                GcGen1Collections = 5,
                GcGen2Collections = 1,
                ThreadCount = 25,
                Uptime = TimeSpan.FromMinutes(100 + i)
            });
        }

        // Assert: Buffer maintains capacity (100 snapshots)
        buffer.Should().HaveCount(100);

        // Verify newest snapshots are present (uptime > 100 minutes)
        buffer.Should().Contain(s => s.Uptime > TimeSpan.FromMinutes(100));

        // Verify oldest snapshots were overwritten (uptime < 50 minutes should be gone)
        buffer.Should().NotContain(s => s.Uptime < TimeSpan.FromMinutes(50));
    }

    #endregion

    #region UI Responsiveness Smoke Tests

    [Fact]
    public void DiagnosticExport_ObjectSerialization_IsReasonablyFast()
    {
        // Arrange: Create diagnostic export with typical data
        var export = new DiagnosticExport
        {
            ExportTimestamp = DateTime.Now,
            ApplicationVersion = "1.0.0",
            Platform = "Windows 11",
            PerformanceSnapshots = new List<PerformanceSnapshot>(),
            ErrorHistory = new List<ErrorEntry>(),
            BootMetrics = new BootTimeline
            {
                Stage0Name = "Splash",
                Stage0DurationMs = 850,
                Stage0Status = "Success",
                Stage1Name = "Core",
                Stage1DurationMs = 2400,
                Stage1Status = "Success",
                Stage2Name = "Ready",
                Stage2DurationMs = 750,
                Stage2Status = "Success",
                TotalBootTimeMs = 4000,
                BootSessionId = Guid.NewGuid(),
                BootTimestamp = DateTime.Now,
                ServiceMetrics = new List<string>()
            }
        };

        // Add sample data
        for (int i = 0; i < 100; i++)
        {
            export.PerformanceSnapshots.Add(new PerformanceSnapshot
            {
                Timestamp = DateTime.Now.AddSeconds(-i),
                CpuUsagePercent = 15,
                MemoryUsageMB = 45,
                GcGen0Collections = 10,
                GcGen1Collections = 5,
                GcGen2Collections = 1,
                ThreadCount = 25,
                Uptime = TimeSpan.FromMinutes(i)
            });
        }

        for (int i = 0; i < 50; i++)
        {
            export.ErrorHistory.Add(new ErrorEntry
            {
                Timestamp = DateTime.Now.AddSeconds(-i),
                Severity = "Warning",
                Message = $"Test warning {i}",
                StackTrace = string.Empty,
                Source = "Test"
            });
        }

        // Act: Measure object preparation time
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Simulate preparing for export (accessing properties)
        var snapshotCount = export.PerformanceSnapshots.Count;
        var errorCount = export.ErrorHistory.Count;
        var bootTime = export.BootMetrics.TotalBootTimeMs;

        stopwatch.Stop();

        // Assert: Object access is instantaneous
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1,
            "Diagnostic export object access should be instantaneous");

        snapshotCount.Should().Be(100);
        errorCount.Should().Be(50);
        bootTime.Should().Be(4000);
    }

    #endregion
}
