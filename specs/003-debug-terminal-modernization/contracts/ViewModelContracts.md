# ViewModel Contracts: Debug Terminal Modernization

**Version**: 1.0 | **Date**: 2025-10-06 | **Spec**: [spec.md](../spec.md)

## Overview

This document defines the ViewModel extensions required for Debug Terminal Modernization. The existing `DebugTerminalViewModel` will be extended with new observable properties and relay commands.

---

## DebugTerminalViewModel Extensions

**Location**: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`

### Dependencies (Constructor Injection)

```csharp
public partial class DebugTerminalViewModel : ObservableObject
{
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly IDiagnosticsServiceExtensions _diagnosticsService;
    private readonly IExportService _exportService;
    private readonly ILogger<DebugTerminalViewModel> _logger;

    public DebugTerminalViewModel(
        IPerformanceMonitoringService performanceService,
        IDiagnosticsServiceExtensions diagnosticsService,
        IExportService exportService,
        ILogger<DebugTerminalViewModel> logger)
    {
        // Constructor validation omitted for brevity
    }
}
```

---

## Observable Properties (CommunityToolkit.Mvvm)

### Phase 1: Real-Time Performance Monitoring

```csharp
[ObservableProperty]
private PerformanceSnapshot? _currentPerformance;

[ObservableProperty]
private ObservableCollection<PerformanceSnapshot> _performanceHistory = new();

[ObservableProperty]
private bool _isMonitoring;

[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(StartMonitoringCommand))]
[NotifyCanExecuteChangedFor(nameof(StopMonitoringCommand))]
private bool _canToggleMonitoring = true;
```

### Phase 1: Boot Timeline Visualization

```csharp
[ObservableProperty]
private BootTimeline? _currentBootTimeline;

[ObservableProperty]
private ObservableCollection<BootTimeline> _historicalBootTimelines = new();

[ObservableProperty]
private TimeSpan _totalBootTime;

[ObservableProperty]
private string? _slowestStage; // "Stage 1" if Stage 1 took longest
```

### Phase 1: Error History

```csharp
[ObservableProperty]
private ObservableCollection<ErrorEntry> _recentErrors = new();

[ObservableProperty]
private int _errorCount;

[ObservableProperty]
private ErrorSeverity _selectedSeverityFilter = ErrorSeverity.Error;
```

### Phase 2: Connection Pool Statistics

```csharp
[ObservableProperty]
private ConnectionPoolStats? _connectionPoolStats;

[ObservableProperty]
private int _mySqlIdleConnections;

[ObservableProperty]
private int _httpIdleConnections;

[ObservableProperty]
private TimeSpan _averageMySqlWaitTime;
```

### Phase 2: Environment Variables

```csharp
[ObservableProperty]
private ObservableCollection<KeyValuePair<string, string>> _environmentVariables = new();

[ObservableProperty]
private string _searchFilter = string.Empty;
```

### Phase 2: Auto-Refresh Toggle

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(UpdateRefreshIntervalCommand))]
private bool _isAutoRefreshEnabled;

[ObservableProperty]
private int _refreshIntervalSeconds = 5; // Default per CL-002
```

### Phase 3: Network Diagnostics

```csharp
[ObservableProperty]
private ObservableCollection<NetworkDiagnosticResult> _networkResults = new();

[ObservableProperty]
private bool _isRunningDiagnostics;

[ObservableProperty]
private string _customEndpoint = string.Empty;
```

### Phase 3: Historical Trends

```csharp
[ObservableProperty]
private ObservableCollection<PerformanceSnapshot> _last10Snapshots = new();

[ObservableProperty]
private double _averageCpuUsage;

[ObservableProperty]
private long _averageMemoryUsage;
```

### Phase 3: Live Log Viewer

```csharp
[ObservableProperty]
private ObservableCollection<string> _liveLogEntries = new();

[ObservableProperty]
private string _logLevelFilter = "All"; // "All", "Debug", "Info", "Warning", "Error"

[ObservableProperty]
private bool _isAutoScrollEnabled = true;
```

### Phase 3: Export Functionality

```csharp
[ObservableProperty]
private string _exportFilePath = string.Empty;

[ObservableProperty]
private string _selectedExportFormat = "JSON"; // "JSON" or "Markdown"

[ObservableProperty]
private bool _isExporting;

[ObservableProperty]
private int _exportProgress; // 0-100
```

---

## Relay Commands (CommunityToolkit.Mvvm)

### Phase 1: Real-Time Performance

```csharp
[RelayCommand(CanExecute = nameof(CanStartMonitoring))]
private async Task StartMonitoringAsync(CancellationToken cancellationToken)
{
    IsMonitoring = true;
    await _performanceService.StartMonitoringAsync(
        TimeSpan.FromSeconds(RefreshIntervalSeconds),
        cancellationToken);
}

private bool CanStartMonitoring() => CanToggleMonitoring && !IsMonitoring;

[RelayCommand(CanExecute = nameof(CanStopMonitoring))]
private async Task StopMonitoringAsync()
{
    IsMonitoring = false;
    await _performanceService.StopMonitoringAsync();
}

private bool CanStopMonitoring() => CanToggleMonitoring && IsMonitoring;

[RelayCommand]
private async Task RefreshPerformanceAsync(CancellationToken cancellationToken)
{
    CurrentPerformance = await _performanceService.GetCurrentSnapshotAsync(cancellationToken);
}
```

### Phase 1: Boot Timeline

```csharp
[RelayCommand]
private async Task LoadBootTimelineAsync(CancellationToken cancellationToken)
{
    CurrentBootTimeline = await _diagnosticsService.GetBootTimelineAsync(cancellationToken);
    TotalBootTime = CurrentBootTimeline.TotalBootTime;

    // Determine slowest stage
    var stages = new[] { CurrentBootTimeline.Stage0, CurrentBootTimeline.Stage1, CurrentBootTimeline.Stage2 };
    SlowestStage = stages.OrderByDescending(s => s.Duration).First().ToString();
}

[RelayCommand]
private async Task LoadHistoricalBootsAsync(CancellationToken cancellationToken)
{
    var timelines = await _diagnosticsService.GetHistoricalBootTimelinesAsync(10, cancellationToken);
    HistoricalBootTimelines = new ObservableCollection<BootTimeline>(timelines);
}
```

### Phase 1: Quick Actions

```csharp
[RelayCommand]
private async Task ClearCacheAsync(CancellationToken cancellationToken)
{
    // Confirmation dialog required per CL-008
    var confirmed = await ShowConfirmationDialogAsync("Clear cache? This action cannot be undone.");
    if (!confirmed) return;

    var clearedCount = await _diagnosticsService.ClearCacheAsync(cancellationToken);
    _logger.LogInformation("Cache cleared: {Count} entries", clearedCount);
}

[RelayCommand]
private async Task ForceGarbageCollectionAsync()
{
    var freedMB = await _diagnosticsService.ForceGarbageCollectionAsync();
    _logger.LogInformation("GC completed: {FreedMB} MB freed", freedMB);
    await RefreshPerformanceAsync(default);
}

[RelayCommand]
private async Task RestartApplicationAsync()
{
    await _diagnosticsService.RestartApplicationAsync();
}
```

### Phase 2: Connection Pool

```csharp
[RelayCommand]
private async Task RefreshConnectionStatsAsync(CancellationToken cancellationToken)
{
    ConnectionPoolStats = await _diagnosticsService.GetConnectionPoolStatsAsync(cancellationToken);
    MySqlIdleConnections = ConnectionPoolStats.MySqlPool.IdleConnections;
    HttpIdleConnections = ConnectionPoolStats.HttpPool.IdleConnections;
}
```

### Phase 2: Auto-Refresh

```csharp
[RelayCommand(CanExecute = nameof(IsAutoRefreshEnabled))]
private async Task UpdateRefreshIntervalAsync(CancellationToken cancellationToken)
{
    // Validate range per CL-002
    if (RefreshIntervalSeconds < 1 || RefreshIntervalSeconds > 30)
    {
        _logger.LogWarning("Invalid refresh interval: {Interval}s. Must be 1-30s.", RefreshIntervalSeconds);
        RefreshIntervalSeconds = 5; // Reset to default
        return;
    }

    await StopMonitoringAsync();
    await StartMonitoringAsync(cancellationToken);
}
```

### Phase 3: Network Diagnostics

```csharp
[RelayCommand]
private async Task RunNetworkDiagnosticsAsync(CancellationToken cancellationToken)
{
    IsRunningDiagnostics = true;
    try
    {
        var endpoints = new[] { "https://api.example.com", CustomEndpoint }
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        var results = await _diagnosticsService.RunNetworkDiagnosticsAsync(
            endpoints,
            timeoutSeconds: 5,
            cancellationToken);

        NetworkResults = new ObservableCollection<NetworkDiagnosticResult>(results);
    }
    finally
    {
        IsRunningDiagnostics = false;
    }
}
```

### Phase 3: Export

```csharp
[RelayCommand]
private async Task ExportDiagnosticsAsync(CancellationToken cancellationToken)
{
    if (!_exportService.ValidateExportPath(ExportFilePath))
    {
        _logger.LogError("Invalid export path: {Path}", ExportFilePath);
        return;
    }

    IsExporting = true;
    var progress = new Progress<int>(p => ExportProgress = p);

    try
    {
        if (SelectedExportFormat == "JSON")
        {
            await _exportService.ExportToJsonAsync(ExportFilePath, cancellationToken, progress);
        }
        else if (SelectedExportFormat == "Markdown")
        {
            await _exportService.ExportToMarkdownAsync(ExportFilePath, cancellationToken, progress);
        }

        _logger.LogInformation("Diagnostics exported to {Path}", ExportFilePath);
    }
    finally
    {
        IsExporting = false;
        ExportProgress = 0;
    }
}
```

---

## Property Change Notifications

Key property dependencies for `[NotifyCanExecuteChangedFor]`:

- `CanToggleMonitoring` → `StartMonitoringCommand`, `StopMonitoringCommand`
- `IsAutoRefreshEnabled` → `UpdateRefreshIntervalCommand`
- `IsMonitoring` → UI visibility bindings
- `IsExporting` → UI disabled state bindings

---

## Testing Strategy

### Unit Tests (ViewModel Layer)

1. **Command Execution**: Verify each command calls the correct service method
2. **Property Updates**: Verify observable properties update correctly after commands
3. **CanExecute Logic**: Verify commands enable/disable based on state
4. **Validation**: Verify input validation (refresh interval 1-30s, export path validation)
5. **Error Handling**: Verify exception handling with try-finally patterns

Example:

```csharp
[Fact]
public async Task StartMonitoringCommand_Should_Call_Service()
{
    var mockService = Substitute.For<IPerformanceMonitoringService>();
    var viewModel = new DebugTerminalViewModel(mockService, ...);

    await viewModel.StartMonitoringCommand.ExecuteAsync(default);

    await mockService.Received(1).StartMonitoringAsync(
        Arg.Any<TimeSpan>(),
        Arg.Any<CancellationToken>());
    viewModel.IsMonitoring.Should().BeTrue();
}
```

---

**Status**: ✅ **Complete**
**Document Version**: 1.0 | **Last Updated**: 2025-10-06
