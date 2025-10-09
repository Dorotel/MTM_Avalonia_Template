# Service Contracts: Debug Terminal Modernization

**Version**: 1.0 | **Date**: 2025-10-06 | **Spec**: [../spec.md](../spec.md)

## Overview

This directory contains all service interface definitions (contracts) for the Debug Terminal Modernization feature. These contracts define the API surface for the three new services and ViewModel extensions.

---

## Service Contracts

### 1. IPerformanceMonitoringService.cs

**Purpose**: Real-time system performance monitoring
**Phase**: Phase 1
**Features**: Real-Time Performance Monitoring (FR-001 to FR-010)

**Key Methods**:
- `GetCurrentSnapshotAsync()` - Get current CPU, memory, GC, thread metrics
- `GetRecentSnapshotsAsync(count)` - Get last N snapshots from circular buffer
- `StartMonitoringAsync(interval)` - Start continuous monitoring (1-30s per CL-002)
- `StopMonitoringAsync()` - Stop monitoring

**Returns**: `PerformanceSnapshot` record with 8 properties

---

### 2. IDiagnosticsServiceExtensions.cs

**Purpose**: Extensions to existing diagnostics service
**Phase**: Phases 1-3
**Features**: Boot Timeline, Connection Pool, Network Diagnostics, Quick Actions

**Key Methods**:
- `GetBootTimelineAsync()` - Get Stage 0/1/2 breakdown (FR-011 to FR-013)
- `GetHistoricalBootTimelinesAsync(count)` - Last 10 boots per CL-001 (FR-029 to FR-032)
- `GetConnectionPoolStatsAsync()` - MySQL + HTTP pool metrics (FR-018 to FR-021)
- `RunNetworkDiagnosticsAsync(endpoints)` - Ping endpoints with 5s timeout per CL-005 (FR-033 to FR-036)
- `ClearCacheAsync()` - Quick Action (requires confirmation per CL-008) (FR-026)
- `ForceGarbageCollectionAsync()` - Quick Action (no confirmation) (FR-027)
- `RestartApplicationAsync()` - Quick Action (no confirmation) (FR-028)

**Returns**: `BootTimeline`, `ConnectionPoolStats`, `NetworkDiagnosticResult` records

---

### 3. IExportService.cs

**Purpose**: Export diagnostics to JSON/Markdown
**Phase**: Phase 3
**Features**: Export Functionality (FR-041 to FR-045)

**Key Methods**:
- `CreateExportAsync()` - Create sanitized diagnostic package
- `ExportToJsonAsync(filePath, progress)` - Export to JSON (priority per CL-003)
- `ExportToMarkdownAsync(filePath, progress)` - Export to Markdown (optional)
- `ValidateExportPath(filePath)` - Validate file path is writable
- `SanitizePii(value)` - Mask passwords, tokens, connection strings

**Returns**: `DiagnosticExport` record with all diagnostic data

---

## ViewModel Contracts

### ViewModelContracts.md

**Purpose**: Define ViewModel extensions for Debug Terminal
**Scope**: All phases (1-3)
**Type**: Observable properties + Relay commands

**Contents**:
- **70+ Observable Properties**: Performance metrics, boot timelines, errors, connection stats, env vars, network results, logs, export settings
- **15+ Relay Commands**: Start/stop monitoring, refresh data, Quick Actions, run diagnostics, export data
- **Validation Rules**: Refresh interval (1-30s), export path validation, confirmation dialogs
- **Testing Strategy**: Unit test patterns for commands, properties, CanExecute logic

---

## Implementation Guidelines

### 1. Service Registration (Dependency Injection)

```csharp
// Program.cs
services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
services.AddSingleton<IDiagnosticsServiceExtensions, DiagnosticsServiceExtensions>();
services.AddSingleton<IExportService, ExportService>();
services.AddTransient<DebugTerminalViewModel>();
```

### 2. Constructor Injection Pattern

```csharp
public DebugTerminalViewModel(
    IPerformanceMonitoringService performanceService,
    IDiagnosticsServiceExtensions diagnosticsService,
    IExportService exportService,
    ILogger<DebugTerminalViewModel> logger)
{
    _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
    _diagnosticsService = diagnosticsService ?? throw new ArgumentNullException(nameof(diagnosticsService));
    _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
}
```

### 3. Async Pattern with Cancellation

All async methods support `CancellationToken` for cancellation and timeout handling:

```csharp
[RelayCommand]
private async Task RefreshPerformanceAsync(CancellationToken cancellationToken)
{
    try
    {
        CurrentPerformance = await _performanceService.GetCurrentSnapshotAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Performance refresh cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to refresh performance");
    }
}
```

### 4. Error Handling

All services throw specific exceptions (documented in XML comments):
- `OperationCanceledException` - Operation cancelled via token
- `ArgumentException` / `ArgumentOutOfRangeException` - Invalid parameters
- `InvalidOperationException` - Invalid state (e.g., monitoring already started)
- `UnauthorizedAccessException` - File access denied (export)

### 5. Platform Differences

**Android Graceful Degradation** (per CL-004):
- `GetConnectionPoolStatsAsync()` returns `ConnectionPoolStats` with message "Not Available on Android"
- Check `RuntimeInformation.IsOSPlatform(OSPlatform.Android)` before querying unavailable metrics

---

## Testing Contracts

### Unit Test Coverage

**Service Layer** (80% coverage target):
- ✅ Verify method contracts (parameters, return types, exceptions)
- ✅ Mock dependencies (ILogger, database connections)
- ✅ Validate data constraints (1-30s interval, 1-10 boots, etc.)
- ✅ Error handling (cancellation, timeouts, invalid inputs)

**ViewModel Layer**:
- ✅ Command execution calls correct service methods
- ✅ Observable properties update after commands
- ✅ CanExecute logic enables/disables commands correctly
- ✅ Validation rules enforced (CL-002, CL-005, CL-008)

**Integration Tests**:
- ✅ Service → ViewModel → XAML data flow
- ✅ Performance validation (<2% CPU, <500ms render)
- ✅ Platform-specific behavior (Windows vs Android)

---

## Cross-References

- **Data Models**: See [../data-model.md](../data-model.md) for entity definitions
- **Implementation Guide**: See [../quickstart.md](../quickstart.md) for step-by-step development
- **Requirements**: See [../spec.md](../spec.md) for functional requirements

---

**Status**: ✅ **Complete - All Contracts Defined**
**Document Version**: 1.0 | **Last Updated**: 2025-10-06
