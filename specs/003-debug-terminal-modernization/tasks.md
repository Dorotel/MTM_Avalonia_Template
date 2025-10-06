# Tasks: Debug Terminal Modernization

**Feature Branch**: `003-debug-terminal-modernization`
**Input**: Design documents from `/specs/003-debug-terminal-modernization/`
**Prerequisites**: plan.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

---

## Execution Summary

This task list implements Debug Terminal Modernization in **3 phases** across **60 tasks**. Phase 1 delivers real-time monitoring, boot timeline, quick actions, and error history. Phase 2 adds connection pool stats, environment variables, assembly info, and auto-refresh. Phase 3 provides network diagnostics, historical trends, live log viewer, and export functionality.

**Total Estimated Time**:
- **Copilot Agent**: 12-15 hours
- **Human Developer**: 4-5 weeks (120-160 hours)

**Dependencies**: Tests before implementation (TDD), models before services, services before ViewModels, ViewModels before XAML.

---

## Phase 1: Setup & Foundation (T001-T010)

### T001: Create Models Directory Structure
**Path**: `MTM_Template_Application/Models/Diagnostics/`
**Description**: Create directory structure for diagnostic models
**Dependencies**: None
**Parallel**: Yes [P]
**Estimated Time**: 5 minutes

```bash
mkdir -p MTM_Template_Application/Models/Diagnostics
```

---

### T002 [P]: Create PerformanceSnapshot Model
**Path**: `MTM_Template_Application/Models/Diagnostics/PerformanceSnapshot.cs`
**Description**: Create PerformanceSnapshot record with validation rules per data-model.md
**Dependencies**: T001
**Parallel**: Yes [P]
**Estimated Time**: 15 minutes

**Acceptance Criteria**:
- Record type with required properties (Timestamp, CpuUsagePercent, MemoryUsageMB, GcGen0/1/2Collections, ThreadCount, Uptime)
- CpuUsagePercent: 0.0 ≤ value ≤ 100.0
- All counts ≥ 0
- Uptime ≥ TimeSpan.Zero

---

### T003 [P]: Create BootTimeline Models
**Path**: `MTM_Template_Application/Models/Diagnostics/BootTimeline.cs`
**Description**: Create BootTimeline, Stage0Info, Stage1Info, Stage2Info, ServiceInitInfo records per data-model.md
**Dependencies**: T001
**Parallel**: Yes [P]
**Estimated Time**: 20 minutes

**Acceptance Criteria**:
- All Duration values ≥ TimeSpan.Zero
- TotalBootTime = Stage0.Duration + Stage1.Duration + Stage2.Duration
- ServiceTimings list not null (can be empty)

---

### T004 [P]: Create ErrorEntry Model
**Path**: `MTM_Template_Application/Models/Diagnostics/ErrorEntry.cs`
**Description**: Create ErrorEntry record and ErrorSeverity enum per data-model.md
**Dependencies**: T001
**Parallel**: Yes [P]
**Estimated Time**: 15 minutes

**Acceptance Criteria**:
- ErrorSeverity enum: Info, Warning, Error, Critical
- Required properties: Id, Timestamp, Severity, Category, Message, StackTrace?, RecoverySuggestion?, ContextData
- ContextData not null (can be empty dictionary)

---

### T005 [P]: Create ConnectionPoolStats Models
**Path**: `MTM_Template_Application/Models/Diagnostics/ConnectionPoolStats.cs`
**Description**: Create ConnectionPoolStats, MySqlPoolStats, HttpPoolStats records per data-model.md
**Dependencies**: T001
**Parallel**: Yes [P]
**Estimated Time**: 15 minutes

**Acceptance Criteria**:
- TotalConnections = ActiveConnections + IdleConnections
- All counts ≥ 0
- AverageWaitTime/AverageResponseTime ≥ TimeSpan.Zero

---

### T006 [P]: Create DiagnosticExport Model
**Path**: `MTM_Template_Application/Models/Diagnostics/DiagnosticExport.cs`
**Description**: Create DiagnosticExport record per data-model.md (aggregates all diagnostic data)
**Dependencies**: T002, T003, T004, T005
**Parallel**: Yes [P]
**Estimated Time**: 20 minutes

**Acceptance Criteria**:
- Platform: One of "Windows", "Android"
- All collections not null (can be empty)
- References PerformanceSnapshot, BootTimeline, ErrorEntry, ConnectionPoolStats

---

### T007 [P]: Unit Tests for PerformanceSnapshot
**Path**: `tests/unit/Models/Diagnostics/PerformanceSnapshotTests.cs`
**Description**: Write unit tests for PerformanceSnapshot validation rules
**Dependencies**: T002
**Parallel**: Yes [P]
**Estimated Time**: 20 minutes

**Test Cases**:
- Valid snapshot creation
- CpuUsagePercent validation (0-100 range)
- Negative value rejection
- Required property enforcement

---

### T008 [P]: Unit Tests for BootTimeline
**Path**: `tests/unit/Models/Diagnostics/BootTimelineTests.cs`
**Description**: Write unit tests for BootTimeline and nested stage records
**Dependencies**: T003
**Parallel**: Yes [P]
**Estimated Time**: 25 minutes

**Test Cases**:
- Valid boot timeline creation
- TotalBootTime calculation correctness
- Empty ServiceTimings list handling
- Negative duration rejection

---

### T009 [P]: Unit Tests for ErrorEntry
**Path**: `tests/unit/Models/Diagnostics/ErrorEntryTests.cs`
**Description**: Write unit tests for ErrorEntry validation
**Dependencies**: T004
**Parallel**: Yes [P]
**Estimated Time**: 20 minutes

**Test Cases**:
- Valid error entry creation
- All severity levels work
- ContextData dictionary not null
- Optional properties (StackTrace, RecoverySuggestion) nullable

---

### T010 [P]: Unit Tests for ConnectionPoolStats
**Path**: `tests/unit/Models/Diagnostics/ConnectionPoolStatsTests.cs`
**Description**: Write unit tests for ConnectionPoolStats validation
**Dependencies**: T005
**Parallel**: Yes [P]
**Estimated Time**: 20 minutes

**Test Cases**:
- TotalConnections = ActiveConnections + IdleConnections
- All counts non-negative
- TimeSpan properties non-negative

---

## Phase 2: Service Interfaces & Contracts (T011-T015)

### T011 [P]: Create IPerformanceMonitoringService Interface
**Path**: `MTM_Template_Application/Services/Diagnostics/IPerformanceMonitoringService.cs`
**Description**: Copy contract from specs/003-debug-terminal-modernization/contracts/IPerformanceMonitoringService.cs
**Dependencies**: T002
**Parallel**: Yes [P]
**Estimated Time**: 10 minutes

**Acceptance Criteria**:
- GetCurrentSnapshotAsync() method
- GetRecentSnapshotsAsync(int count) method
- StartMonitoringAsync(TimeSpan interval) method
- StopMonitoringAsync() method
- IsMonitoring property

---

### T012 [P]: Create IDiagnosticsServiceExtensions Interface
**Path**: `MTM_Template_Application/Services/Diagnostics/IDiagnosticsServiceExtensions.cs`
**Description**: Copy contract from specs/003-debug-terminal-modernization/contracts/IDiagnosticsServiceExtensions.cs
**Dependencies**: T003, T004, T005
**Parallel**: Yes [P]
**Estimated Time**: 10 minutes

**Acceptance Criteria**:
- GetBootTimelineAsync() method
- GetRecentErrorsAsync(int count) method
- GetConnectionPoolStatsAsync() method

---

### T013 [P]: Create IExportService Interface
**Path**: `MTM_Template_Application/Services/Diagnostics/IExportService.cs`
**Description**: Copy contract from specs/003-debug-terminal-modernization/contracts/IExportService.cs
**Dependencies**: T006
**Parallel**: Yes [P]
**Estimated Time**: 10 minutes

**Acceptance Criteria**:
- CreateExportAsync() method
- ExportToJsonAsync(DiagnosticExport, string filePath) method
- ExportToMarkdownAsync(DiagnosticExport, string filePath) method (if time permits)

---

### T014 [P]: Service Contract Tests for IPerformanceMonitoringService
**Path**: `tests/contract/Services/PerformanceMonitoringServiceContractTests.cs`
**Description**: Write contract tests validating IPerformanceMonitoringService behavior
**Dependencies**: T011
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Cases**:
- GetCurrentSnapshotAsync returns valid snapshot
- GetRecentSnapshotsAsync respects count parameter (max 100)
- StartMonitoringAsync validates interval (1-30s per CL-002)
- StartMonitoringAsync throws if already monitoring
- StopMonitoringAsync stops monitoring

---

### T015 [P]: Service Contract Tests for IDiagnosticsServiceExtensions
**Path**: `tests/contract/Services/DiagnosticsServiceExtensionsContractTests.cs`
**Description**: Write contract tests validating IDiagnosticsServiceExtensions behavior
**Dependencies**: T012
**Parallel**: Yes [P]
**Estimated Time**: 25 minutes

**Test Cases**:
- GetBootTimelineAsync returns current boot timeline
- GetRecentErrorsAsync respects count parameter
- GetConnectionPoolStatsAsync returns current stats

---

## Phase 3: Service Implementation (T016-T025)

### T016: Implement PerformanceMonitoringService
**Path**: `MTM_Template_Application/Services/Diagnostics/PerformanceMonitoringService.cs`
**Description**: Implement IPerformanceMonitoringService with circular buffer (100 snapshots)
**Dependencies**: T011, T014
**Parallel**: No (sequential after tests)
**Estimated Time**: 90 minutes

**Implementation Details**:
- Use `System.Diagnostics.Process.GetCurrentProcess()` for CPU/memory
- Use `GC.CollectionCount(generation)` for GC metrics
- Use `Process.Threads.Count` for thread count
- Circular buffer using `Queue<PerformanceSnapshot>` with max 100 entries
- Background timer for StartMonitoringAsync (validate 1-30s interval per CL-002)
- CancellationToken support for StopMonitoringAsync

**Acceptance Criteria**:
- All contract tests pass (T014)
- CPU usage <2% during monitoring (NFR-003)
- Memory usage <100KB for 100 snapshots (NFR-001)
- No UI blocking (async operations)

---

### T017: Implement DiagnosticsServiceExtensions
**Path**: `MTM_Template_Application/Services/Diagnostics/DiagnosticsServiceExtensions.cs`
**Description**: Implement IDiagnosticsServiceExtensions (boot timeline, errors, connection stats)
**Dependencies**: T012, T015
**Parallel**: No (sequential after tests)
**Estimated Time**: 60 minutes

**Implementation Details**:
- GetBootTimelineAsync: Query IBootOrchestrator for last 10 boot sessions (per CL-001)
- GetRecentErrorsAsync: Query global error buffer (session-only per CL-007)
- GetConnectionPoolStatsAsync: Query MySql.Data connection pool + HttpClient metrics

**Acceptance Criteria**:
- All contract tests pass (T015)
- Boot timeline includes Stage 0/1/2 breakdown
- Error history limited to last 100 entries
- Connection stats return 0 if unavailable (graceful degradation)

---

### T018: Implement ExportService
**Path**: `MTM_Template_Application/Services/Diagnostics/ExportService.cs`
**Description**: Implement IExportService with PII sanitization
**Dependencies**: T013
**Parallel**: No (sequential after tests)
**Estimated Time**: 60 minutes

**Implementation Details**:
- CreateExportAsync: Aggregate all diagnostic data into DiagnosticExport
- ExportToJsonAsync: Use System.Text.Json with indentation
- Sanitize PII: Redact passwords, tokens, email addresses from EnvironmentVariables and StackTrace
  - Password regex: `password[:\s]*[^\s]+` → `password: [REDACTED]`
  - Token regex: `(token|key|secret)[:\s]*[^\s]+` → `$1: [REDACTED]`
  - Email regex: `[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}` → `[EMAIL_REDACTED]`
- Background thread for large exports (>10MB)

**Acceptance Criteria**:
- JSON export completes <2s for typical diagnostic data
- PII sanitization regex patterns tested
- File I/O uses async methods (no UI blocking)

---

### T019 [P]: Unit Tests for PerformanceMonitoringService
**Path**: `tests/unit/Services/PerformanceMonitoringServiceTests.cs`
**Description**: Write unit tests for PerformanceMonitoringService implementation
**Dependencies**: T016
**Parallel**: Yes [P]
**Estimated Time**: 45 minutes

**Test Cases**:
- Circular buffer rotation at 100 snapshots
- Monitoring interval validation (reject <1s or >30s)
- CancellationToken stops monitoring
- IsMonitoring property reflects state
- No memory leaks after 1000 snapshots

---

### T020 [P]: Unit Tests for DiagnosticsServiceExtensions
**Path**: `tests/unit/Services/DiagnosticsServiceExtensionsTests.cs`
**Description**: Write unit tests for DiagnosticsServiceExtensions implementation
**Dependencies**: T017
**Parallel**: Yes [P]
**Estimated Time**: 40 minutes

**Test Cases**:
- Boot timeline retrieval (last 10 boots)
- Error history filtering by severity
- Connection pool stats graceful degradation (return 0 if unavailable)
- Null handling for optional properties

---

### T021 [P]: Unit Tests for ExportService
**Path**: `tests/unit/Services/ExportServiceTests.cs`
**Description**: Write unit tests for ExportService implementation
**Dependencies**: T018
**Parallel**: Yes [P]
**Estimated Time**: 40 minutes

**Test Cases**:
- PII sanitization (passwords, tokens, emails redacted) with specific regex validation:
  - `password: secret123` → `password: [REDACTED]`
  - `api_token: abc123xyz` → `api_token: [REDACTED]`
  - `user@example.com` → `[EMAIL_REDACTED]`
- JSON export format validation
- File path validation
- Large export handling (>10K log entries)

---

### T022: Register Services in DI Container
**Path**: `MTM_Template_Application/Extensions/ServiceCollectionExtensions.cs`
**Description**: Register IPerformanceMonitoringService, IDiagnosticsServiceExtensions, IExportService in DI container
**Dependencies**: T016, T017, T018
**Parallel**: No (requires all service implementations)
**Estimated Time**: 15 minutes

**Implementation**:


```csharp
public static IServiceCollection AddDiagnosticServices(this IServiceCollection services)
{
    services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
    services.AddSingleton<IDiagnosticsServiceExtensions, DiagnosticsServiceExtensions>();
    services.AddTransient<IExportService, ExportService>();
    return services;
}
```

**Acceptance Criteria**:
- Services resolve via DI container
- Singleton lifetime for performance/diagnostics services
- Transient lifetime for export service (one per export operation)

---

### T023 [P]: Integration Test: Performance Monitoring End-to-End
**Path**: `tests/integration/Diagnostics/PerformanceMonitoringIntegrationTests.cs`
**Description**: Test complete performance monitoring workflow
**Dependencies**: T016, T022
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Scenario**:
1. Start monitoring with 1-second interval
2. Capture 10 snapshots
3. Verify circular buffer contains 10 entries
4. Stop monitoring
5. Verify IsMonitoring = false

**Acceptance Criteria**:
- CPU usage <2% during monitoring
- Memory usage <100KB
- No UI thread blocking

---

### T024 [P]: Integration Test: Boot Timeline Retrieval
**Path**: `tests/integration/Diagnostics/BootTimelineIntegrationTests.cs`
**Description**: Test boot timeline retrieval from IBootOrchestrator
**Dependencies**: T017, T022
**Parallel**: Yes [P]
**Estimated Time**: 25 minutes

**Test Scenario**:
1. Query boot timeline
2. Verify Stage 0/1/2 durations
3. Verify service initialization timings (Stage 1)
4. Verify TotalBootTime calculation

**Acceptance Criteria**:
- Boot timeline includes all stages
- Service timings list populated (if boot completed)
- TotalBootTime matches sum of stage durations

---

### T025 [P]: Integration Test: Diagnostic Export
**Path**: `tests/integration/Diagnostics/ExportIntegrationTests.cs`
**Description**: Test complete diagnostic export workflow with PII sanitization
**Dependencies**: T018, T022
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Scenario**:
1. Create diagnostic export with sample data
2. Export to JSON file
3. Verify file exists and is valid JSON
4. Verify PII sanitization (passwords redacted)
5. Clean up test file

**Acceptance Criteria**:
- JSON export completes <2s
- PII sanitization effective (no plaintext passwords)
- File I/O uses async methods

---

## Phase 4: ViewModel Extensions (T026-T035)

### T026: Extend DebugTerminalViewModel with Performance Properties
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add observable properties for performance monitoring per ViewModelContracts.md
**Dependencies**: T016
**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 30 minutes


**Properties to Add**:

```csharp
[ObservableProperty] private PerformanceSnapshot? _currentPerformance;
[ObservableProperty] private ObservableCollection<PerformanceSnapshot> _performanceHistory = new();
[ObservableProperty] private bool _isMonitoring;
[ObservableProperty] private bool _canToggleMonitoring = true;
```

**Acceptance Criteria**:
- Properties use `[ObservableProperty]` attribute
- Collections initialized to empty (not null)

---

### T027: Extend DebugTerminalViewModel with Boot Timeline Properties
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add observable properties for boot timeline per ViewModelContracts.md
**Dependencies**: T017
**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 20 minutes


**Properties to Add**:

```csharp
[ObservableProperty] private BootTimeline? _currentBootTimeline;
[ObservableProperty] private ObservableCollection<BootTimeline> _historicalBootTimelines = new();
[ObservableProperty] private TimeSpan _totalBootTime;
[ObservableProperty] private string? _slowestStage;
```

---

### T028: Extend DebugTerminalViewModel with Error History Properties
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add observable properties for error history per ViewModelContracts.md
**Dependencies**: T017
**Parallel**: No (modifies existing ViewModel)

**Estimated Time**: 20 minutes

**Properties to Add**:

```csharp
[ObservableProperty] private ObservableCollection<ErrorEntry> _recentErrors = new();
[ObservableProperty] private int _errorCount;
[ObservableProperty] private ErrorSeverity _selectedSeverityFilter = ErrorSeverity.Error;
```

---

### T029: Add Performance Monitoring Commands to DebugTerminalViewModel
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add relay commands for starting/stopping performance monitoring
**Dependencies**: T026

**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 40 minutes

**Commands to Add**:

```csharp
[RelayCommand(CanExecute = nameof(CanStartMonitoring))]
private async Task StartMonitoringAsync(CancellationToken cancellationToken)
{
    IsMonitoring = true;
    await _performanceService.StartMonitoringAsync(TimeSpan.FromSeconds(5), cancellationToken);
}

[RelayCommand(CanExecute = nameof(CanStopMonitoring))]
private async Task StopMonitoringAsync()
{
    await _performanceService.StopMonitoringAsync();
    IsMonitoring = false;
}

private bool CanStartMonitoring() => !IsMonitoring;
private bool CanStopMonitoring() => IsMonitoring;
```

**Acceptance Criteria**:
- Commands support async operations
- CanExecute logic prevents double-start/double-stop
- NotifyCanExecuteChangedFor attribute updates button states

---

### T030: Add Quick Actions Panel Commands to DebugTerminalViewModel
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add relay commands for Quick Actions Panel (Clear Cache, Reload Config, Test DB, Force GC, Refresh, Export)
**Dependencies**: T017, T018
**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 60 minutes

**Commands to Add** (per FR-026):
- `[RelayCommand] ClearCacheAsync()` - Requires confirmation dialog (per CL-008)
- `[RelayCommand] ReloadConfigurationAsync()`
- `[RelayCommand] TestDatabaseConnectionAsync()`
- `[RelayCommand] ForceGcCollectionAsync()`
- `[RelayCommand] RefreshAllMetricsAsync()`
- `[RelayCommand] ExportDiagnosticReportAsync()`
- `[RelayCommand] CopyDiagnosticsToClipboardAsync()` - Copy JSON to clipboard (per FR-044)

**Acceptance Criteria**:
- All commands async with cancellation support
- ClearCache shows confirmation dialog (per FR-029)
- Loading state (IsExecutingQuickAction property)
- Error handling with user-friendly messages

---

### T031: Add Load Boot Timeline Command to DebugTerminalViewModel
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add command to load current boot timeline and historical timelines

**Dependencies**: T027
**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 30 minutes

**Command to Add**:

```csharp
[RelayCommand]
private async Task LoadBootTimelineAsync(CancellationToken cancellationToken)
{
    CurrentBootTimeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);
    // Calculate TotalBootTime and SlowestStage
}
```

**Acceptance Criteria**:
- Loads current boot timeline from diagnostics service
- Calculates slowest stage (Stage 0, 1, or 2)
- Populates HistoricalBootTimelines with last 10 boots (per CL-001)

---

### T032: Add Load Error History Command to DebugTerminalViewModel
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`

**Description**: Add command to load recent errors from diagnostics service
**Dependencies**: T028
**Parallel**: No (modifies existing ViewModel)
**Estimated Time**: 30 minutes

**Command to Add**:

```csharp
[RelayCommand]
private async Task LoadRecentErrorsAsync(CancellationToken cancellationToken)
{
    var errors = await _diagnosticsService.GetRecentErrorsAsync(100, cancellationToken); // Max 100 buffer
    RecentErrors.Clear();
    // Display last 10 in UI
    foreach (var error in errors.Take(10)) RecentErrors.Add(error);
    ErrorCount = errors.Count; // Show total count
}
```

**Acceptance Criteria**:
- Loads last 10 errors (per FR-024)
- Filters by SelectedSeverityFilter
- Updates ErrorCount property

---

### T033 [P]: Unit Tests for DebugTerminalViewModel Performance Monitoring
**Path**: `tests/unit/ViewModels/DebugTerminalViewModelPerformanceTests.cs`
**Description**: Test ViewModel performance monitoring commands and properties
**Dependencies**: T026, T029
**Parallel**: Yes [P]
**Estimated Time**: 40 minutes

**Test Cases**:
- StartMonitoringCommand updates IsMonitoring
- StopMonitoringCommand resets IsMonitoring
- CanExecute logic prevents double-start
- CurrentPerformance updates on monitoring tick
- PerformanceHistory circular buffer (max 100)

---

### T034 [P]: Unit Tests for DebugTerminalViewModel Quick Actions Panel
**Path**: `tests/unit/ViewModels/DebugTerminalViewModelQuickActionsTests.cs`
**Description**: Test ViewModel Quick Actions Panel commands
**Dependencies**: T030
**Parallel**: Yes [P]
**Estimated Time**: 45 minutes

**Test Cases**:
- ClearCacheCommand confirmation dialog
- TestDatabaseConnectionCommand success/failure paths
- ForceGcCollectionCommand executes GC.Collect()
- ExportDiagnosticReportCommand opens save dialog
- CopyDiagnosticsToClipboardCommand copies JSON to clipboard (per FR-044)
- Loading state during command execution

---

### T035 [P]: Unit Tests for DebugTerminalViewModel Boot Timeline
**Path**: `tests/unit/ViewModels/DebugTerminalViewModelBootTimelineTests.cs`
**Description**: Test ViewModel boot timeline loading and calculations
**Dependencies**: T027, T031
**Parallel**: Yes [P]
**Estimated Time**: 35 minutes

**Test Cases**:
- LoadBootTimelineCommand populates CurrentBootTimeline
- SlowestStage calculation correct (Stage 0/1/2)
- HistoricalBootTimelines limited to 10 entries
- TotalBootTime calculation matches boot data

---

## Phase 5: XAML UI Implementation (T036-T045)

### T036: Create Performance Monitoring Panel XAML
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add performance monitoring panel to DebugTerminalWindow (FR-001 to FR-006)
**Dependencies**: T026, T029
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 60 minutes

**UI Components** (per spec FR-001 to FR-006):
- CPU usage % (real-time, 1-second updates)
- Memory usage MB (color-coded: green <70MB, yellow 70-90MB, red >90MB)
- GC collection counts (Gen 0, 1, 2)
- Thread count
- Handle count
- Start/Stop monitoring buttons

**Acceptance Criteria**:
- Uses `{CompiledBinding}` with `x:DataType="vm:DebugTerminalViewModel"`
- Color-coding implemented via value converters
- No UI blocking during updates (async binding)

---

### T037: Create Boot Timeline Visualization XAML
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add boot timeline horizontal bar chart to DebugTerminalWindow (FR-007 to FR-009)
**Dependencies**: T027, T031
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 90 minutes

**UI Components** (per spec FR-007 to FR-009):
- Horizontal bar chart for Stage 0/1/2
- Color-coding: green (meets target), red (exceeds target), gray (no data)
- Proportional bar widths
- Expandable Stage 1 details (service initialization timings)

**Implementation Details**:
- Use Avalonia `Canvas` or custom control for bar chart
- Data binding to CurrentBootTimeline.Stage0/Stage1/Stage2.Duration
- Color converter: green if Duration < target, red if Duration >= target

**Acceptance Criteria**:
- Bar widths proportional to total boot time
- Color-coding matches requirements
- Expandable Stage 1 service details

---

### T038: Create Quick Actions Panel XAML
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add Quick Actions panel to DebugTerminalWindow (FR-026 to FR-029)
**Dependencies**: T030
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 45 minutes

**UI Components** (per spec FR-026):
- "Clear Cache" button (with confirmation dialog per FR-029)
- "Reload Configuration" button
- "Test Database Connection" button
- "Force GC Collection" button
- "Refresh All Metrics" button
- "Export Diagnostic Report" button
- "Copy to Clipboard" button (per FR-044)
- Loading spinner (visible during command execution)

**Acceptance Criteria**:
- Buttons bound to ViewModel commands using `{CompiledBinding}`
- Loading state shows spinner
- ClearCache button triggers confirmation dialog

---

### T039: Create Error History Panel XAML
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add error history panel to DebugTerminalWindow (FR-024 to FR-026)
**Dependencies**: T028, T032
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 60 minutes

**UI Components** (per spec FR-024 to FR-026):
- ListBox showing last 10 errors
- Error entry format:
  - Timestamp (HH:mm:ss.fff)
  - Category icon/text (Database, Network, Cache, Configuration)
  - Error message (first 100 chars, expandable)
  - Severity icon (🔴 Error, 🟡 Warning, ℹ️ Info)
  - Expandable stack trace

**Acceptance Criteria**:
- ListBox bound to RecentErrors collection
- Expandable error details (click to show stack trace)
- Severity filter dropdown (Info, Warning, Error, Critical)

---

### T040: Create Connection Pool Stats Panel XAML (Phase 2)
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add connection pool statistics panel (FR-010, FR-011)
**Dependencies**: T017
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 45 minutes

**UI Components** (per spec FR-010, FR-011):
- MySQL connection pool stats:
  - Active connections count
  - Pool size (max connections)
  - Average wait time (ms)
  - Connection failures (session)
- HTTP client connection stats:
  - Reused connections count
  - DNS lookup average time (ms)
  - Active requests count

**Acceptance Criteria**:
- Desktop-first implementation (per CL-004)
- Graceful degradation on Android (show "Not Available")

---

### T041: Create Environment Variables Display XAML (Phase 2)
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add environment variables display panel (FR-012)
**Dependencies**: None (existing data)
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 30 minutes

**UI Components**:
- ListBox showing all `MTM_` prefixed environment variables
- Copy to clipboard button

**Acceptance Criteria**:
- Displays only `MTM_` prefixed variables
- Copy button uses `Clipboard.SetTextAsync()`

---

### T042: Add Auto-Refresh Toggle XAML (Phase 2)
**Path**: `MTM_Template_Application/Views/DebugTerminalWindow.axaml`
**Description**: Add auto-refresh toggle and interval selector (FR-037 to FR-039)
**Dependencies**: T029
**Parallel**: No (modifies existing XAML)
**Estimated Time**: 30 minutes

**UI Components**:
- ToggleButton for auto-refresh (On/Off)
- ComboBox for refresh interval (1-30 seconds, default 5s per CL-002)

**Acceptance Criteria**:
- Toggle button bound to IsMonitoring property
- Interval selector updates monitoring interval
- Interval validation (1-30s range)

---

### T043 [P]: Integration Test: Performance Monitoring UI
**Path**: `tests/integration/Views/DebugTerminalPerformanceUITests.cs`
**Description**: Test performance monitoring UI workflow end-to-end
**Dependencies**: T036, T042
**Parallel**: Yes [P]
**Estimated Time**: 40 minutes

**Test Scenario**:
1. Open Debug Terminal window
2. Click "Start Monitoring" button
3. Verify performance metrics update every 1 second
4. Verify memory color-coding (green/yellow/red)
5. Click "Stop Monitoring" button
6. Verify monitoring stops

**Acceptance Criteria**:
- UI updates without blocking
- Color-coding correct
- Start/Stop buttons enable/disable correctly

---

### T044 [P]: Integration Test: Boot Timeline UI
**Path**: `tests/integration/Views/DebugTerminalBootTimelineUITests.cs`
**Description**: Test boot timeline visualization UI
**Dependencies**: T037
**Parallel**: Yes [P]
**Estimated Time**: 35 minutes

**Test Scenario**:
1. Open Debug Terminal window
2. Verify boot timeline bar chart displayed
3. Verify Stage 0/1/2 color-coding (green/red/gray)
4. Click on Stage 1 to expand service timings
5. Verify service initialization details shown

**Acceptance Criteria**:
- Bar chart renders correctly
- Color-coding matches boot performance
- Stage 1 expansion works

---

### T045 [P]: Integration Test: Quick Actions Panel UI
**Path**: `tests/integration/Views/DebugTerminalQuickActionsPanelUITests.cs`
**Description**: Test Quick Actions Panel UI workflow
**Dependencies**: T038
**Parallel**: Yes [P]
**Estimated Time**: 45 minutes

**Test Scenario**:
1. Open Debug Terminal window
2. Click "Clear Cache" button → Verify confirmation dialog appears
3. Click "Test Database Connection" → Verify loading state → Verify result message
4. Click "Force GC Collection" → Verify GC metrics update
5. Click "Export Diagnostic Report" → Verify save dialog opens

**Acceptance Criteria**:
- All buttons functional
- Loading state shows spinner
- Confirmation dialog for ClearCache only (per CL-008)

---

## Phase 6: Polish & Documentation (T046-T060)

### T046: Performance Optimization - Circular Buffer Efficiency
**Path**: `MTM_Template_Application/Services/Diagnostics/PerformanceMonitoringService.cs`
**Description**: Optimize circular buffer to ensure <100KB memory usage and <2% CPU
**Dependencies**: T016
**Parallel**: No
**Estimated Time**: 30 minutes

**Optimizations**:
- Use `ArrayPool<T>` for buffer allocation
- Reduce allocation frequency (reuse snapshots if possible)
- Profile with dotMemory to verify <100KB usage

**Acceptance Criteria**:
- Memory usage <100KB (verified with profiler)
- CPU usage <2% during monitoring (verified with Performance Monitor)

---

### T047: Add Value Converters for Color-Coding
**Path**: `MTM_Template_Application/Converters/MemoryUsageToColorConverter.cs`
**Description**: Create Avalonia value converters for memory color-coding (green/yellow/red)
**Dependencies**: T036
**Parallel**: No
**Estimated Time**: 30 minutes

**Converters to Create**:
- `MemoryUsageToColorConverter` (green <70MB, yellow 70-90MB, red >90MB)
- `BootStageToColorConverter` (green meets target, red exceeds, gray no data)
- `ErrorSeverityToIconConverter` (🔴 Error, 🟡 Warning, ℹ️ Info)

**Acceptance Criteria**:
- Converters implement `IValueConverter`
- Color thresholds match spec requirements
- Registered in XAML resources

---

### T048: Add Error Handling for Service Failures
**Path**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
**Description**: Add comprehensive error handling with user-friendly messages
**Dependencies**: T029, T030, T031, T032
**Parallel**: No
**Estimated Time**: 45 minutes

**Error Scenarios**:
- Performance monitoring service unavailable → Show "Performance monitoring unavailable"
- Database connection test fails → Show connection error details
- Export service fails → Show "Export failed, check disk space"
- Boot timeline unavailable → Show "No boot data available"

**Acceptance Criteria**:
- All commands wrapped in try-catch
- User-friendly error messages (not stack traces)
- Errors logged to Serilog

---

### T049: Add Logging for Diagnostic Operations
**Path**: `MTM_Template_Application/Services/Diagnostics/*Service.cs`
**Description**: Add structured logging to all diagnostic services
**Dependencies**: T016, T017, T018
**Parallel**: No
**Estimated Time**: 30 minutes

**Logging Points**:
- Performance monitoring start/stop
- Boot timeline retrieval
- Error history retrieval
- Export operations
- Quick Actions execution

**Acceptance Criteria**:
- All log statements use structured logging (Serilog)
- Log levels appropriate (Debug/Info/Warning/Error)
- No sensitive data logged (PII redacted)

---

### T050 [P]: Unit Tests for Value Converters
**Path**: `tests/unit/Converters/DiagnosticConvertersTests.cs`
**Description**: Test all diagnostic value converters
**Dependencies**: T047
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Cases**:
- MemoryUsageToColorConverter thresholds (70MB, 90MB)
- BootStageToColorConverter logic (green/red/gray)
- ErrorSeverityToIconConverter mapping (🔴/🟡/ℹ️)

---

### T051 [P]: Performance Test - Circular Buffer Under Load
**Path**: `tests/integration/Performance/CircularBufferPerformanceTests.cs`
**Description**: Validate circular buffer performance under heavy load (1000+ snapshots)
**Dependencies**: T016, T046
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Scenario**:
- Create 1000 performance snapshots
- Verify memory usage <100KB
- Verify CPU usage <2%
- Verify no memory leaks (GC.GetTotalMemory stable)

**Acceptance Criteria**:
- Memory usage <100KB (verified)
- CPU usage <2% (verified)
- No memory leaks (verified with dotMemory)

---

### T052 [P]: Performance Test - UI Render Time
**Path**: `tests/integration/Performance/DebugTerminalRenderPerformanceTests.cs`
**Description**: Validate Debug Terminal render time <500ms (NFR-002)
**Dependencies**: T036, T037, T038, T039
**Parallel**: Yes [P]
**Estimated Time**: 30 minutes

**Test Scenario**:
- Open Debug Terminal window
- Measure render time (window open to fully rendered)
- Verify <500ms (NFR-002)

**Acceptance Criteria**:
- Render time <500ms
- No frame drops during rendering

---

### T053: Update Debug Terminal Documentation
**Path**: `docs/DEBUG-TERMINAL-IMPLEMENTATION.md`
**Description**: Update documentation to reflect new Debug Terminal features
**Dependencies**: T036, T037, T038, T039, T040, T041, T042
**Parallel**: No
**Estimated Time**: 60 minutes

**Documentation Updates**:
- Add "Real-Time Performance Monitoring" section with screenshots
- Add "Boot Timeline Visualization" section
- Add "Quick Actions" section
- Add "Error History" section
- Add "Connection Pool Statistics" section (Phase 2)
- Add "Environment Variables Display" section (Phase 2)
- Add "Auto-Refresh Toggle" section (Phase 2)

**Acceptance Criteria**:
- All features documented with complete descriptions
- 1 screenshot per feature (Performance Monitoring, Boot Timeline, Quick Actions Panel, Error History, Connection Pool Stats, Environment Variables, Auto-Refresh)
- Usage examples provided for each Quick Action button
- Performance targets referenced (NFR-001 to NFR-005)
- Maximum 3000 words total (technical audience)

---

### T054: Create User Guide for Debug Terminal
**Path**: `docs/USER-GUIDE-DEBUG-TERMINAL.md`
**Description**: Create user guide for non-technical users
**Dependencies**: T053
**Parallel**: No
**Estimated Time**: 90 minutes

**User Guide Contents**:
- How to open Debug Terminal
- How to read performance metrics
- How to interpret boot timeline
- How to use Quick Actions
- How to export diagnostic report
- Troubleshooting common issues

**Acceptance Criteria**:
- Non-technical language (no code samples, no technical jargon)
- Step-by-step instructions with numbered lists
- 1 annotated screenshot per section (callouts for UI elements)
- Troubleshooting section with 5+ common issues and solutions
- Maximum 2000 words (non-technical audience)
- Validated by product stakeholder for clarity

---

### T055: Update AGENTS.md with Debug Terminal Patterns
**Path**: `AGENTS.md`
**Description**: Update AGENTS.md to include Debug Terminal development patterns
**Dependencies**: T016, T017, T018, T026, T027, T028, T029, T030
**Parallel**: No
**Estimated Time**: 30 minutes

**Pattern Updates**:
- Circular buffer implementation pattern
- Performance monitoring service pattern
- Diagnostic export pattern
- ViewModel command pattern for Quick Actions

**Acceptance Criteria**:
- Patterns documented with code examples
- References to Debug Terminal feature files

---

### T056: Constitutional Compliance Audit
**Path**: `specs/003-debug-terminal-modernization/CONSTITUTIONAL-AUDIT.md`
**Description**: Run constitutional audit to verify feature compliance with project principles
**Dependencies**: All implementation tasks (T001-T055)
**Parallel**: No
**Estimated Time**: 45 minutes

**Audit Checklist**:
- ✅ MVVM Community Toolkit patterns used (`[ObservableProperty]`, `[RelayCommand]`)
- ✅ CompiledBinding with `x:DataType` in all XAML
- ✅ Nullable reference types enforced
- ✅ Async with CancellationToken support
- ✅ DI via AppBuilder
- ✅ No database changes (in-memory only)
- ✅ Cross-platform support (Windows + Android graceful degradation)
- ✅ Test-first development (>80% coverage)

**Acceptance Criteria**:
- All 9 constitutional principles verified
- Audit results documented

---

### T057 [P]: Integration Test - Export Workflow End-to-End
**Path**: `tests/integration/Workflows/ExportWorkflowTests.cs`
**Description**: Test complete diagnostic export workflow from UI to file
**Dependencies**: T018, T030, T038
**Parallel**: Yes [P]
**Estimated Time**: 40 minutes

**Test Scenario**:
1. Open Debug Terminal
2. Click "Export Diagnostic Report" button
3. Select JSON format and save location
4. Verify file created and valid JSON
5. Verify PII sanitization (no passwords in export)
6. Clean up test file

**Acceptance Criteria**:
- Export completes <2s
- JSON valid
- PII sanitization effective

---

### T058 [P]: Integration Test - Auto-Refresh Workflow
**Path**: `tests/integration/Workflows/AutoRefreshWorkflowTests.cs`
**Description**: Test auto-refresh toggle and interval selection workflow
**Dependencies**: T029, T042
**Parallel**: Yes [P]
**Estimated Time**: 35 minutes

**Test Scenario**:
1. Open Debug Terminal
2. Toggle auto-refresh ON
3. Set interval to 1 second
4. Verify metrics update every 1 second
5. Change interval to 10 seconds
6. Verify metrics update every 10 seconds
7. Toggle auto-refresh OFF
8. Verify monitoring stops

**Acceptance Criteria**:
- Auto-refresh respects interval setting
- Interval validation (1-30s)
- No UI blocking during updates

---

### T059: Run Validation Script
**Path**: `.specify/scripts/powershell/validate-implementation.ps1`
**Description**: Run validation script to ensure 100% task completion and spec compliance
**Dependencies**: All implementation tasks (T001-T058)
**Parallel**: No
**Estimated Time**: 15 minutes

**Validation Checks**:
- ✅ All 60 tasks marked complete
- ✅ All acceptance criteria met
- ✅ All tests passing (>80% coverage)
- ✅ Zero build errors
- ✅ Constitutional compliance verified
- ✅ Documentation complete

**Acceptance Criteria**:
- Validation script passes with zero warnings
- Ready for PR submission

---

### T060: Create Pull Request
**Path**: N/A (GitHub PR)
**Description**: Create pull request for `003-debug-terminal-modernization` branch
**Dependencies**: T059
**Parallel**: No
**Estimated Time**: 30 minutes

**PR Contents**:
- Title: `[003] Debug Terminal Modernization - Phase 1 Complete`
- Description: Feature summary, implemented FRs, test results
- Link to spec: `specs/003-debug-terminal-modernization/spec.md`
- Link to tasks: `specs/003-debug-terminal-modernization/tasks.md`
- Screenshots: Performance monitoring, boot timeline, Quick Actions, error history

**Acceptance Criteria**:
- PR template completed
- All CI checks passing
- Ready for code review

---

### T061 [P]: Integration Test - Android Platform Graceful Degradation
**Path**: `tests/integration/Platform/AndroidGracefulDegradationTests.cs`
**Description**: Validate Android platform shows "Not Available" for unsupported features (NFR-014)
**Dependencies**: T040 (Connection Pool Stats Panel)
**Parallel**: Yes [P]
**Estimated Time**: 35 minutes

**Test Scenario** (Android device or emulator):
1. Open Debug Terminal on Android platform
2. Verify Connection Pool Statistics panel shows "Not Available" for MySQL pool metrics
3. Verify HTTP client stats display correctly (Android supports HTTP metrics)
4. Verify no crashes or exceptions on missing features
5. Verify UI layout remains consistent (no missing panels)

**Acceptance Criteria**:
- Android platform detected via `RuntimeInformation.IsOSPlatform(OSPlatform.Linux)` (Android is Linux-based)
- "Not Available" text displayed for unsupported metrics (MySQL connection pool)
- HTTP client stats work correctly on Android
- No crashes or null reference exceptions
- UI gracefully degrades per NFR-014

---

## Dependencies Summary

```
Setup (T001-T010)
  ├── Models (T002-T006) [P] → Model Tests (T007-T010) [P]
  └── Service Interfaces (T011-T013) [P] → Contract Tests (T014-T015) [P]

Service Implementation (T016-T025)
  ├── PerformanceMonitoringService (T016) → Unit Tests (T019) [P]
  ├── DiagnosticsServiceExtensions (T017) → Unit Tests (T020) [P]
  ├── ExportService (T018) → Unit Tests (T021) [P]
  ├── DI Registration (T022)
  └── Integration Tests (T023-T025) [P]

ViewModel Extensions (T026-T035)
  ├── Performance Properties (T026) → Commands (T029) → Tests (T033) [P]
  ├── Boot Timeline Properties (T027) → Commands (T031) → Tests (T035) [P]
  ├── Error History Properties (T028) → Commands (T032)
  ├── Quick Actions Commands (T030) → Tests (T034) [P]

XAML UI (T036-T045)
  ├── Performance Panel (T036) → UI Tests (T043) [P]
  ├── Boot Timeline Panel (T037) → UI Tests (T044) [P]
  ├── Quick Actions Panel (T038) → UI Tests (T045) [P]
  ├── Error History Panel (T039)
  ├── Connection Pool Panel (T040) [Phase 2]
  ├── Environment Variables Panel (T041) [Phase 2]
  └── Auto-Refresh Toggle (T042)

Polish & Documentation (T046-T060)
  ├── Performance Optimization (T046)
  ├── Value Converters (T047) → Tests (T050) [P]
  ├── Error Handling (T048)
  ├── Logging (T049)
  ├── Performance Tests (T051-T052) [P]
  ├── Documentation (T053-T055)
  ├── Constitutional Audit (T056)
  ├── Workflow Tests (T057-T058) [P]
  ├── Validation (T059)
  └── Pull Request (T060)

```

---

## Parallel Execution Examples

### Example 1: Create All Models in Parallel (T002-T006)

```bash
# Launch 5 model creation tasks simultaneously (different files)

Task T002: "Create PerformanceSnapshot model in MTM_Template_Application/Models/Diagnostics/PerformanceSnapshot.cs"
Task T003: "Create BootTimeline models in MTM_Template_Application/Models/Diagnostics/BootTimeline.cs"
Task T004: "Create ErrorEntry model in MTM_Template_Application/Models/Diagnostics/ErrorEntry.cs"
Task T005: "Create ConnectionPoolStats models in MTM_Template_Application/Models/Diagnostics/ConnectionPoolStats.cs"
Task T006: "Create DiagnosticExport model in MTM_Template_Application/Models/Diagnostics/DiagnosticExport.cs"
```

### Example 2: Write All Model Tests in Parallel (T007-T010)


```bash
# Launch 4 test tasks simultaneously (different files)
Task T007: "Unit tests for PerformanceSnapshot in tests/unit/Models/Diagnostics/PerformanceSnapshotTests.cs"
Task T008: "Unit tests for BootTimeline in tests/unit/Models/Diagnostics/BootTimelineTests.cs"
Task T009: "Unit tests for ErrorEntry in tests/unit/Models/Diagnostics/ErrorEntryTests.cs"
Task T010: "Unit tests for ConnectionPoolStats in tests/unit/Models/Diagnostics/ConnectionPoolStatsTests.cs"
```


### Example 3: Service Contract Tests in Parallel (T014-T015)

```bash
# Launch 2 contract test tasks simultaneously (different services)
Task T014: "Contract tests for IPerformanceMonitoringService in tests/contract/Services/PerformanceMonitoringServiceContractTests.cs"
Task T015: "Contract tests for IDiagnosticsServiceExtensions in tests/contract/Services/DiagnosticsServiceExtensionsContractTests.cs"
```

### Example 4: Integration Tests in Parallel (T023-T025)

```bash
# Launch 3 integration tests simultaneously (different workflows)
Task T023: "Performance monitoring end-to-end test in tests/integration/Diagnostics/PerformanceMonitoringIntegrationTests.cs"
Task T024: "Boot timeline retrieval test in tests/integration/Diagnostics/BootTimelineIntegrationTests.cs"
Task T025: "Diagnostic export test in tests/integration/Diagnostics/ExportIntegrationTests.cs"
```

---

## Task Completion Tracking

### Phase 1: Setup & Foundation (0/10 complete)
- [ ] T001, T002 [P], T003 [P], T004 [P], T005 [P], T006 [P], T007 [P], T008 [P], T009 [P], T010 [P]

### Phase 2: Service Interfaces & Contracts (0/5 complete)
- [ ] T011 [P], T012 [P], T013 [P], T014 [P], T015 [P]

### Phase 3: Service Implementation (0/10 complete)
- [ ] T016, T017, T018, T019 [P], T020 [P], T021 [P], T022, T023 [P], T024 [P], T025 [P]

### Phase 4: ViewModel Extensions (0/10 complete)
- [ ] T026, T027, T028, T029, T030, T031, T032, T033 [P], T034 [P], T035 [P]

### Phase 5: XAML UI Implementation (0/10 complete)
- [ ] T036, T037, T038, T039, T040, T041, T042, T043 [P], T044 [P], T045 [P]

### Phase 6: Polish & Documentation (0/16 complete)
- [ ] T046, T047, T048, T049, T050 [P], T051 [P], T052 [P], T053, T054, T055, T056, T057 [P], T058 [P], T059, T060, T061 [P]

**Overall Progress**: 0/61 tasks complete (0%)

---

## Notes

- **[P] Marker**: Tasks marked [P] can run in parallel (different files, no dependencies)
- **TDD Approach**: Tests before implementation (contract tests → implementation → unit tests)
- **Cancellation Support**: All async methods include CancellationToken parameters
- **Performance Budgets**: <2% CPU, <100KB memory, <500ms render (validated in T051-T052)
- **Constitutional Compliance**: Verified in T056 audit
- **Phase 2 Deferred**: T040 (Connection Pool Stats), T041 (Environment Variables) deferred to Phase 2
- **Phase 3 Deferred**: Network diagnostics, historical trends, live log viewer, additional export formats (per plan.md)
- **Android Platform Testing**: T061 validates graceful degradation on Android (per NFR-014)

---

**Status**: ✅ **Ready for Execution**
**Document Version**: 1.1
**Last Updated**: 2025-10-06
**Total Tasks**: 61 (Phase 1: 45 tasks, Phase 2: 9 tasks, Phase 3: 7 tasks deferred)
**Changes**: Added T061 (Android Platform Graceful Degradation Test), FR-044 (Copy to Clipboard)
