# Data Model: Debug Terminal Modernization

**Version**: 1.0 | **Date**: 2025-10-06 | **Plan**: [plan.md](./plan.md)

## Overview

Five key entities support the Debug Terminal Modernization feature. All entities use **in-memory storage only** (no database persistence).

## Entity Definitions

### 1. PerformanceSnapshot

Real-time system performance metrics captured at regular intervals.

```csharp
public sealed record PerformanceSnapshot
{
    public required DateTime Timestamp { get; init; }
    public required double CpuUsagePercent { get; init; }      // 0.0 - 100.0
    public required long MemoryUsageMB { get; init; }          // Current working set
    public required int GcGen0Collections { get; init; }       // Gen 0 GC count
    public required int GcGen1Collections { get; init; }       // Gen 1 GC count
    public required int GcGen2Collections { get; init; }       // Gen 2 GC count
    public required int ThreadCount { get; init; }             // Active threads
    public required TimeSpan Uptime { get; init; }             // Time since boot
}
```

**Validation Rules**:
- CpuUsagePercent: 0.0 ≤ value ≤ 100.0
- MemoryUsageMB: value ≥ 0
- GcGen*Collections: value ≥ 0
- ThreadCount: value > 0
- Uptime: value ≥ TimeSpan.Zero

**Lifecycle**: Created by `IPerformanceMonitoringService`, stored in circular buffer (last 100 snapshots), garbage collected when buffer rotates.

**Relationships**: None (standalone snapshots)

---

### 2. BootTimeline

Boot sequence breakdown showing Stage 0 (splash), Stage 1 (services), Stage 2 (app ready) timings.

```csharp
public sealed record BootTimeline
{
    public required DateTime BootStartTime { get; init; }
    public required Stage0Info Stage0 { get; init; }
    public required Stage1Info Stage1 { get; init; }
    public required Stage2Info Stage2 { get; init; }
    public required TimeSpan TotalBootTime { get; init; }
}

public sealed record Stage0Info
{
    public required TimeSpan Duration { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record Stage1Info
{
    public required TimeSpan Duration { get; init; }
    public required bool Success { get; init; }
    public required List<ServiceInitInfo> ServiceTimings { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record Stage2Info
{
    public required TimeSpan Duration { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record ServiceInitInfo
{
    public required string ServiceName { get; init; }
    public required TimeSpan Duration { get; init; }
    public required bool Success { get; init; }
}
```

**Validation Rules**:
- All Duration values: value ≥ TimeSpan.Zero
- ServiceTimings: Not null, can be empty list
- TotalBootTime = Stage0.Duration + Stage1.Duration + Stage2.Duration

**Lifecycle**: Created by `IBootOrchestrator` during boot, stored for current session + last 10 boots (per CL-001).

**Relationships**: Contains nested Stage*Info records

---

### 3. ErrorEntry

Structured error history with categorization and recovery suggestions.

```csharp
public sealed record ErrorEntry
{
    public required Guid Id { get; init; }
    public required DateTime Timestamp { get; init; }
    public required ErrorSeverity Severity { get; init; }
    public required string Category { get; init; }            // "Database", "Network", "Validation", etc.
    public required string Message { get; init; }
    public required string? StackTrace { get; init; }
    public required string? RecoverySuggestion { get; init; }
    public required Dictionary<string, string> ContextData { get; init; }
}

public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
```

**Validation Rules**:
- Id: Must be unique
- Category: Non-empty string
- Message: Non-empty string
- ContextData: Not null (can be empty dictionary)

**Lifecycle**: Created by global exception handler, stored in session-only circular buffer (last 100 errors per CL-007), cleared on app restart.

**Relationships**: None (standalone entries)

---

### 4. ConnectionPoolStats

MySQL and HTTP connection pool health metrics.

```csharp
public sealed record ConnectionPoolStats
{
    public required DateTime Timestamp { get; init; }
    public required MySqlPoolStats MySqlPool { get; init; }
    public required HttpPoolStats HttpPool { get; init; }
}

public sealed record MySqlPoolStats
{
    public required int TotalConnections { get; init; }
    public required int ActiveConnections { get; init; }
    public required int IdleConnections { get; init; }
    public required int WaitingRequests { get; init; }
    public required TimeSpan AverageWaitTime { get; init; }
}

public sealed record HttpPoolStats
{
    public required int TotalConnections { get; init; }
    public required int ActiveConnections { get; init; }
    public required int IdleConnections { get; init; }
    public required TimeSpan AverageResponseTime { get; init; }
}
```

**Validation Rules**:
- TotalConnections = ActiveConnections + IdleConnections
- All counts: value ≥ 0
- AverageWaitTime/AverageResponseTime: value ≥ TimeSpan.Zero

**Lifecycle**: Created by `IDiagnosticsService.GetConnectionPoolStatsAsync()`, not persisted (query on-demand).

**Relationships**: None (snapshot entity)

---

### 5. DiagnosticExport

Complete diagnostic package for export (JSON or Markdown).

```csharp
public sealed record DiagnosticExport
{
    public required DateTime ExportTime { get; init; }
    public required string ApplicationVersion { get; init; }
    public required string Platform { get; init; }             // "Windows", "Android"
    public required PerformanceSnapshot CurrentPerformance { get; init; }
    public required BootTimeline? BootTimeline { get; init; }
    public required List<ErrorEntry> RecentErrors { get; init; }
    public required ConnectionPoolStats? ConnectionStats { get; init; }
    public required Dictionary<string, string> EnvironmentVariables { get; init; }
    public required List<string> RecentLogEntries { get; init; }
}
```

**Validation Rules**:
- ApplicationVersion: Non-empty string
- Platform: One of "Windows", "Android"
- RecentErrors: Not null (can be empty list)
- EnvironmentVariables: Not null (can be empty dictionary)
- RecentLogEntries: Not null (can be empty list)

**Lifecycle**: Created on-demand by `IExportService.CreateExportAsync()`, sanitized (PII removed), written to file, then disposed.

**Relationships**: Aggregates PerformanceSnapshot, BootTimeline, ErrorEntry, ConnectionPoolStats

---

## Memory Management

| Entity | Storage | Max Size | Retention |
|--------|---------|----------|-----------|
| PerformanceSnapshot | Circular buffer | 100 snapshots (~10KB) | Session |
| BootTimeline | List | 10 boots (~5KB) | Session |
| ErrorEntry | Circular buffer | 100 errors (~30KB) | Session |
| ConnectionPoolStats | None (query on-demand) | N/A | N/A |
| DiagnosticExport | Temporary | ~50KB | Until file written |

**Total Memory Budget**: ~50KB (well under 100KB limit per NFR-001)

---

## Entity Relationships Diagram

```
DiagnosticExport (aggregator)
  ├── PerformanceSnapshot (current)
  ├── BootTimeline (latest)
  │   ├── Stage0Info
  │   ├── Stage1Info
  │   │   └── List<ServiceInitInfo>
  │   └── Stage2Info
  ├── List<ErrorEntry> (recent)
  ├── ConnectionPoolStats
  │   ├── MySqlPoolStats
  │   └── HttpPoolStats
  └── Environment Variables + Log Entries
```

---

**Status**: ✅ **Complete**
**Document Version**: 1.0 | **Last Updated**: 2025-10-06
