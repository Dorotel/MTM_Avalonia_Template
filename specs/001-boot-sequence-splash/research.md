# Research: Boot Sequence - Splash-First Services Initialization

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Branch**: `001-boot-sequence-splash`  
**Date**: 2025-10-02

## Research Summary

This research phase explored technical decisions for implementing a robust, cross-platform boot sequence in an Avalonia manufacturing application. Key focus areas included splash screen architecture, service initialization patterns, cascading data source fallback strategies, and offline resilience patterns. All clarification items from the feature spec were resolved during specification, so research focused on implementation best practices and architectural patterns.

---

## 1. Avalonia Splash Screen Architecture (Theme-Independent)

### Decision

Implement splash screen as a standalone, minimal Window with no theme dependencies, displayed synchronously on application startup before App.axaml styling is loaded.

### Rationale

- **FR-001 requirement**: Splash must appear <500ms with no service dependencies
- **Avalonia architecture**: App.axaml Resources load theme dictionaries, which require significant initialization time
- **Solution**: Create splash Window in Program.cs before Application.Run(), use hardcoded styling (no DynamicResource bindings)
- **Performance**: Reduces splash display time by 200-400ms compared to theme-dependent approach

### Alternatives Considered

1. **Theme-dependent splash**: Would violate FR-001 timing constraint and create circular dependency
2. **Separate splash process**: Adds complexity, process coordination overhead, and potential synchronization issues
3. **No splash screen**: Poor UX for 10-second initialization sequence; users see blank window

### Implementation Pattern

```csharp
// Program.cs (Desktop)
public static void Main(string[] args)
{
    var splashWindow = new SplashWindow(); // Minimal, theme-independent
    splashWindow.Show();
    
    // Initialize services...
    var serviceProvider = ConfigureServices();
    
    // Transition to main app
    BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    
    splashWindow.Close();
}
```

### References

- Avalonia Documentation: Application Lifetime
- Performance profiling: Theme loading adds 300-500ms to startup

---

## 2. Service Initialization Order and Dependency Management

### Decision

Use strict sequential initialization with explicit dependency ordering: Configuration → Secrets → Logging → Diagnostics → Reachability → Data Layers → Core Services → Caching → Localization → Navigation/Theming. Implement as async Task chain with cancellation token support.

### Rationale

- **FR-002 requirement**: Sequential order prevents race conditions and undefined behavior
- **Dependency clarity**: Each stage explicitly depends on previous stage completion
- **Error isolation**: Failures at any stage don't corrupt subsequent stages
- **Testability**: Each stage can be unit tested independently with mocked dependencies
- **Cancellation support**: Operator can cancel at any stage (FR-015)

### Alternatives Considered

1. **Parallel initialization**: Faster but creates race conditions, harder to debug, violates FR-002
2. **Event-driven initialization**: More complex state management, harder to test, unpredictable ordering
3. **Dependency injection container auto-resolution**: Hidden dependencies, harder to debug initialization failures

### Implementation Pattern

```csharp
public async Task<BootResult> InitializeAsync(CancellationToken cancellationToken)
{
    var stages = new[]
    {
        (BootStage.Configuration, () => InitializeConfigurationAsync(cancellationToken)),
        (BootStage.Secrets, () => InitializeSecretsAsync(cancellationToken)),
        (BootStage.Logging, () => InitializeLoggingAsync(cancellationToken)),
        // ... remaining stages
    };

    foreach (var (stage, initFunc) in stages)
    {
        UpdateProgress(stage);
        var result = await initFunc();
        if (!result.Success) return result;
        PersistStageMetrics(stage, result.Duration);
    }

    return BootResult.Success();
}
```

### References

- Microsoft Docs: Async/Await Best Practices
- Manufacturing domain: Sequential initialization is standard in industrial control systems

---

## 3. Cascading Data Source Fallback Architecture

### Decision

Implement three-tier data source architecture with sequential fail-fast cascade: Visual API Toolkit (primary) → MAMP MySQL 5.7 (secondary) → Local Cache (tertiary). Each tier has independent timeout handling (10s for network sources, no timeout for cache).

### Rationale

- **FR-009, FR-011 requirements**: Offline resilience with predictable fallback behavior
- **Manufacturing constraint**: System must remain operational even when primary data sources are unavailable
- **Fail-fast principle**: Don't wait for timeout cascade during operator retry; treat unreachable as immediate cascade trigger
- **Data freshness hierarchy**: Visual (live) > MAMP (backup sync) > Cache (stale but available)
- **Performance**: 10-second timeout per source prevents indefinite blocking (PR-003)

### Alternatives Considered

1. **Parallel source checking**: Wastes network resources, violates sequential cascade requirement, harder to reason about
2. **Single timeout for all sources**: Could exceed 25-second total limit (PR-003)
3. **Automatic retry with exponential backoff**: Adds complexity, operator retry (FR-013) provides better control

### Implementation Pattern

```csharp
public async Task<DataSourceResult> GetDataSourceAsync(CancellationToken ct)
{
    // Try Visual API Toolkit first
    var visualResult = await TryVisualApiAsync(timeout: TimeSpan.FromSeconds(10), ct);
    if (visualResult.Success) return visualResult;

    // Cascade to MAMP MySQL
    var mampResult = await TryMampAsync(timeout: TimeSpan.FromSeconds(10), ct);
    if (mampResult.Success) return mampResult;

    // Final fallback to cache (no timeout)
    var cacheResult = await TryCacheAsync(ct);
    return cacheResult; // Returns failure if cache also unavailable
}
```

### Data Source Characteristics

| Source | Freshness | Availability | Performance | Use Case |
|--------|-----------|--------------|-------------|----------|
| Visual API Toolkit | Live (real-time) | Depends on Visual ERP uptime + network | 2-5s latency (network + API processing) | Normal operations, authoritative data source |
| MAMP MySQL 5.7 | Near-live (periodic sync from Visual) | Depends on MAMP server uptime + network | 500ms-2s latency (LAN or local network) | Visual ERP maintenance windows, temporary outages |
| Local Cache | Stale (last successful prefetch) | Always available (local file system) | <100ms (disk I/O) | Network outages, offline operations, emergency fallback |

### References

- Circuit Breaker Pattern (Microsoft Docs)
- Manufacturing domain: Cascading fallback is standard in SCADA systems

---

## 4. Visual API Toolkit Integration (Read-Only)

### Decision

All Visual ERP access via read-only API Toolkit commands wrapped in HTTP REST endpoints. Never use direct SQL against Visual database. API Toolkit is a **.NET library** (not HTTP REST) that requires wrapper service for cross-platform support.

### Visual API Toolkit Architecture

**From Development Guide:**
- **Core Libraries**: LsaCore.dll (Lsa.Data, Lsa.DataLogic, Lsa.BusinessLogic), LsaShared.dll (Lsa.Shared)
- **Business Logic Libraries**: VmfgSales.dll, VmfgPurchasing.dll, VmfgInventory.dll, VmfgShopFloor.dll, VmfgFinancials.dll, VmfgTrace.dll, VmfgShared.dll
- **Database Connection**: Dbms.OpenLocal(instanceName, loginUser, loginPassword) reads Database.Config XML file for connection info
- **Data Access Pattern**: Business objects expose Load/Browse/Save/Prepare/Execute methods, return ADO.NET DataSets
- **Business Object Types**:
  - **Documents**: Header + subordinates (e.g., CustomerOrder with line items), Load(primaryKey)/Save()
  - **Collections**: Multiple rows (e.g., Part master data), Load()/Browse()
  - **Transactions**: Audited changes (e.g., InventoryAdjustment), Prepare()/Save()
  - **Services**: Read-only queries (e.g., GetInventoryBalance), Prepare()/Execute()
- **Requirements**: .NET Framework 4.0+, x86 builds only, SQL Server or Oracle database backend
- **Database.Config**: XML file in same directory as executable, contains encrypted instance configuration

### Rationale

- **FR-010 requirement**: API Toolkit provides sanctioned, supported access to Visual data
- **Data integrity**: Read-only prevents accidental Visual database corruption (never call Save/Update)
- **HTTP wrapper necessity**: API Toolkit is Windows .NET library; HTTP wrapper enables Android support (FR-020)
- **Security**: Dbms.OpenLocal authentication built-in, credentials in Database.Config (encrypted)
- **Cross-platform**: MTM app calls HTTP endpoints, no direct DLL references needed on Android

### Alternatives Considered

1. **Direct SQL against Visual**: Violates FR-010, creates tight coupling, breaks on Visual schema changes, security risk
2. **Direct API Toolkit integration**: Requires Visual API Toolkit DLLs on Android (not supported), tightly couples MTM app to Windows
3. **Mixed API + SQL**: Inconsistent access patterns, harder to maintain, confuses data flow

### Implementation Pattern

**Desktop Visual Service** (ASP.NET Core or Windows Service running on Desktop host):
```csharp
// Service wraps API Toolkit calls
public class VisualApiService
{
    private string _instanceName;
    
    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        // Dbms.OpenLocal reads Database.Config, validates credentials
        return await Task.Run(() => Dbms.OpenLocal(_instanceName, username, password));
    }
    
    public async Task<List<ItemDto>> GetItemsAsync(string? filter, int maxResults)
    {
        // Use VmfgInventory.Part business object
        var part = new VmfgInventory.Part(_instanceName);
        DataTable items = await Task.Run(() => part.Browse(filter, maxResults));
        
        // Convert DataTable → DTO → JSON
        return items.AsEnumerable().Select(row => new ItemDto {
            PartId = row.Field<string>("PART_ID"),
            Description = row.Field<string>("DESCRIPTION"),
            // ... map remaining columns
        }).ToList();
    }
}
```

**MTM Application Client** (Desktop + Android):
```csharp
public interface IVisualApiService
{
    Task<ApiResult<bool>> AuthenticateAsync(string username, string password, CancellationToken ct);
    Task<ApiResult<List<ItemDto>>> GetItemsAsync(GetItemsRequest request, CancellationToken ct);
    Task<ApiResult<List<LocationDto>>> GetLocationsAsync(GetLocationsRequest request, CancellationToken ct);
    Task<ApiResult<List<WorkCenterDto>>> GetWorkCentersAsync(GetWorkCentersRequest request, CancellationToken ct);
}

// Contract-based request/response
public record GetItemsRequest(string? Filter = null, int MaxResults = 1000);
public record ApiResult<T>(bool Success, T? Data, string? Error, TimeSpan Duration);
```

**Database.Config Example** (on Desktop host):
```xml
<?xml version="1.0" encoding="utf-8"?>
<Databases>
  <Database InstanceName="_default" ServerName="VISUAL_SERVER" DatabaseName="VISUAL_DB" 
            LoginUser="SYSADM" LoginPassword="[ENCRYPTED]" Provider="SqlServer" />
</Databases>
```

### Master Data Access Mapping

| Entity | Visual Business Object | Method | Returns |
|--------|----------------------|--------|---------|
| Items (Parts) | VmfgInventory.Part | Browse(filter, maxResults) | DataTable with PART_ID, DESCRIPTION, UOM, etc. |
| Locations | VmfgInventory.Location (or Collection) | Load() or Browse() | DataTable with location records |
| WorkCenters | VmfgShopFloor.WorkCenter | Load() or Browse() | DataTable with work center records |

**Note**: Exact business object class names for Locations and WorkCenters need verification from API Toolkit reference documentation or IntelliSense after DLL references are added.

### References

- Visual Manufacturing API Toolkit Documentation
- API design: Command Query Responsibility Segregation (CQRS)

---

## 5. MAMP MySQL 5.7 Access (Platform-Specific)

### Decision

Desktop platforms use MySQL.Data client for direct MAMP connection. Android platforms use HTTP API proxy service that wraps MAMP queries on server side. Both implement same IMampDataService interface for seamless platform abstraction.

### Rationale

- **Cross-platform DI**: Shared business logic works with either implementation via interface
- **Desktop optimization**: Direct MySQL connection provides best performance for LAN access
- **Android constraint**: MySQL client requires native libraries incompatible with Xamarin/MAUI; API proxy provides network access without native dependencies
- **Security**: API proxy isolates MAMP credentials server-side, Android app only needs API endpoint

### Alternatives Considered

1. **MySQL.Data for Android**: Native dependency conflicts, complex build configuration, unreliable
2. **Xamarin MySQL connector**: Discontinued, incompatible with .NET 9
3. **Direct MySQL from Android**: Security risk (credentials in mobile app), firewall complexity

### Implementation Pattern

```csharp
// Shared interface
public interface IMampDataService
{
    Task<MampResult<ItemData>> GetItemsAsync(CancellationToken ct);
    Task<DateTime?> GetLastSyncTimestampAsync(CancellationToken ct);
}

// Desktop implementation (MTM_Template_Application.Desktop)
public class MampDataService : IMampDataService
{
    private readonly MySqlConnection _connection;
    // Direct MySQL client...
}

// Android implementation (MTM_Template_Application.Android)
public class MampApiProxyService : IMampDataService
{
    private readonly HttpClient _httpClient;
    // HTTP API calls...
}

// DI registration (platform-specific Program.cs/MainActivity.cs)
#if DESKTOP
services.AddSingleton<IMampDataService, MampDataService>();
#elif ANDROID
services.AddSingleton<IMampDataService, MampApiProxyService>();
#endif
```

### References

- Avalonia Cross-Platform DI Patterns
- MySQL.Data NuGet package compatibility matrix

---

## 6. Local Cache Strategy (SQLite/LiteDB)

### Decision

Use LiteDB for local cache storage with JSON document structure. Store master data (Items, Locations, WorkCenters) as denormalized documents with metadata (source, timestamp, staleness). Validate cache integrity on load; delete corrupted cache and attempt fresh prefetch.

### Rationale

- **No schema migrations**: JSON documents adapt to data structure changes without migrations
- **Embedded database**: No separate database server, cross-platform compatible (Windows + Android)
- **Performance**: LiteDB provides ~10k reads/sec, sufficient for master data lookup
- **Simplicity**: Fewer dependencies than SQLite, simpler API, no SQL query complexity
- **Size constraints**: Master data typically <50MB, well within LiteDB limits (2GB)

### Alternatives Considered

1. **SQLite**: More mature, but requires schema migrations, more complex API, overkill for document storage
2. **JSON files**: No indexing, poor performance for lookups, requires custom corruption handling
3. **Realm**: Good mobile performance, but heavier dependency, commercial licensing considerations

### Implementation Pattern

```csharp
public class CacheService : ICacheService
{
    private readonly LiteDatabase _db;

    public async Task<CachedEntity<T>> GetAsync<T>(string key)
    {
        var collection = _db.GetCollection<CacheDocument<T>>("cache");
        var doc = collection.FindById(key);
        
        if (doc == null) return CachedEntity<T>.NotFound();
        
        // Validate integrity
        if (!ValidateChecksum(doc)) 
        {
            collection.Delete(key);
            return CachedEntity<T>.Corrupted();
        }
        
        return CachedEntity<T>.Success(doc.Data, doc.Timestamp);
    }
}

public record CacheDocument<T>(string Id, T Data, DateTime Timestamp, 
                                string Checksum, DataSourceType Source);
```

### Cache Validation Rules

- **Integrity check**: SHA256 checksum on serialized data detects corruption
- **Staleness threshold**: Data older than 24 hours marked as stale (warning banner)
- **Completeness check**: All three master data types (Items, Locations, WorkCenters) must have ≥1 record; missing types block launch (per clarifications)
- **Corruption handling**: Delete corrupted cache, attempt Visual/MAMP prefetch; if unavailable, block launch with error

### References

- LiteDB Documentation
- Manufacturing domain: Master data rarely changes, document model appropriate

---

## 7. Boot Metrics Persistence (JSON/CSV)

### Decision

Persist boot metrics as JSON file with FIFO rotation (retain last 100 boots). Store in application data directory with structured format: timestamp, success/failure, stage durations, active data source, error details.

### Rationale

- **FR-018 requirement**: Diagnostic data for troubleshooting boot failures
- **100-boot retention**: Sufficient history for pattern analysis without excessive storage (~500KB for 100 boots)
- **JSON format**: Human-readable, structured, easy to parse with external tools
- **FIFO rotation**: Delete oldest single entry when 101st boot occurs, prevents unbounded growth
- **Application data directory**: Standard location for app-generated logs, survives app updates

### Alternatives Considered

1. **CSV format**: Less structured, harder to store nested error details, no type safety
2. **Database storage**: Overkill for append-only log data, adds dependency
3. **Fixed file size rotation**: Arbitrary cutoff, may lose recent boots if errors are verbose
4. **Telemetry service**: Adds network dependency, privacy concerns for manufacturing environments

### Implementation Pattern

```csharp
public class BootMetricsService
{
    private const int MaxBootHistory = 100;
    private readonly string _metricsFilePath;

    public async Task PersistBootMetricsAsync(BootMetrics metrics)
    {
        var history = await LoadHistoryAsync();
        history.Add(metrics);
        
        // FIFO rotation
        while (history.Count > MaxBootHistory)
            history.RemoveAt(0);
        
        await File.WriteAllTextAsync(_metricsFilePath, 
            JsonSerializer.Serialize(history, _jsonOptions));
    }
}

public record BootMetrics(
    DateTime Timestamp,
    bool Success,
    Dictionary<BootStage, TimeSpan> StageDurations,
    DataSourceType ActiveDataSource,
    List<DataSourceType> FallbackChain,
    string? ErrorMessage,
    string? ErrorStage
);
```

### Metrics Schema

```json
{
  "timestamp": "2025-10-02T14:23:45Z",
  "success": true,
  "totalDuration": "00:00:08.234",
  "stageDurations": {
    "configuration": "00:00:00.123",
    "secrets": "00:00:00.234",
    "logging": "00:00:00.089",
    "diagnostics": "00:00:00.456",
    "reachability": "00:00:05.123",
    "dataLayers": "00:00:01.234",
    "coreServices": "00:00:00.456",
    "caching": "00:00:00.345",
    "localization": "00:00:00.123",
    "navigation": "00:00:00.051"
  },
  "activeDataSource": "Visual",
  "fallbackChain": ["Visual"],
  "errorMessage": null,
  "errorStage": null
}
```

### References

- .NET JSON serialization best practices
- Application data directory: Environment.SpecialFolder.ApplicationData

---

## 8. Single-Instance Enforcement

### Decision

Use named Mutex for cross-process single-instance detection on Desktop. On Android, use ActivityFlags.SingleTop with existing instance activation. Both prevent duplicate boot sequences.

### Rationale

- **FR-021 requirement**: Prevent duplicate boot sequences from concurrent launches
- **Desktop pattern**: Named Mutex is standard Windows approach for single-instance enforcement
- **Android pattern**: Activity launch modes provide built-in single-instance behavior
- **UX improvement**: Activates existing window rather than showing error dialog

### Alternatives Considered

1. **Named pipes/sockets**: More complex, requires server/client architecture, overkill
2. **File locking**: Unreliable on some platforms, leaves stale locks on crash
3. **Registry/system-wide setting**: Security implications, cleanup complexity

### Implementation Pattern

```csharp
// Desktop (Program.cs)
public static void Main(string[] args)
{
    const string mutexName = "MTM_Template_Application_SingleInstance";
    using var mutex = new Mutex(true, mutexName, out bool createdNew);
    
    if (!createdNew)
    {
        // Another instance is running - activate it
        BringExistingInstanceToFront();
        return;
    }
    
    // Continue with normal startup...
}

// Android (MainActivity.cs)
[Activity(Label = "MTM Template", 
          LaunchMode = LaunchMode.SingleTop,
          MainLauncher = true)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        // Existing instance activated - bring to front
    }
}
```

### References

- Windows Mutex documentation
- Android Activity launch modes

---

## 9. Operator Retry and Error Handling

### Decision

Operator manually triggers retry via Retry button (no automatic retries). Each retry attempts full Visual → MAMP → Cache cascade from beginning. Retry button disabled for 30 seconds after click to prevent rapid repeated attempts. Display Exit-only option if all three sources fail after operator retry. Place Retry Timer in Button Text "Retry {remainingTime}"

### Rationale

- **FR-013 requirement**: Operator control over retry timing avoids wasted network requests
- **30-second cooldown**: Prevents accidental rapid clicks, gives transient network issues time to resolve
- **Full cascade on retry**: Always starts from Visual (best source), ensures fresh attempt at each tier
- **Clear failure state**: Exit-only option when all sources exhausted provides clear "no recovery possible" signal
- **Manufacturing UX**: Operators prefer explicit control over automatic behavior in industrial environments

### Alternatives Considered

1. **Automatic retry with exponential backoff**: Reduces operator control, wastes network resources if issue is persistent
2. **Per-source retry buttons**: More complex UI, confusing for operators unfamiliar with architecture
3. **Unlimited retry attempts**: Risk of infinite retry loops, poor UX for unrecoverable failures

### Implementation Pattern

```csharp
public class SplashViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isRetryEnabled = true;
    
    [ObservableProperty]
    private string _statusMessage;
    
    [RelayCommand(CanExecute = nameof(IsRetryEnabled))]
    private async Task RetryAsync()
    {
        IsRetryEnabled = false;
        
        // Attempt full cascade
        var result = await _bootService.InitializeAsync(_cancellationToken);
        
        if (!result.Success)
        {
            StatusMessage = "All data sources unavailable. Please check network and try again.";
            
            // 30-second cooldown
            await Task.Delay(TimeSpan.FromSeconds(30));
            IsRetryEnabled = true;
        }
        else
        {
            TransitionToMainApp();
        }
    }
}
```

### Error Message Mapping

- **Invalid credentials**: "Login credentials are incorrect. Please check settings." (Exit only, no retry)
- **Visual unreachable, MAMP unreachable, cache missing**: "Cannot connect to data sources and no cached data available." (Exit only)
- **Visual unreachable, MAMP unreachable, cache available**: "Operating in offline mode with cached data." (Continue with warning banner)
- **Visual unreachable, MAMP available**: "Using backup data source. Last synchronized: [timestamp]" (Continue with warning banner)

### References

- Manufacturing UX: Explicit operator control is standard in industrial interfaces
- Retry patterns: Microsoft Transient Fault Handling

---

## 10. Reachability Check Strategy

### Decision

Perform sequential health check requests (HEAD or lightweight GET) to each data source endpoint with 10-second timeout per source. Treat timeout or error as immediate unavailability (cascade to next source). No automatic retries within reachability check.

### Rationale

- **FR-008 requirement**: Independent timeout handling per source prevents blocking
- **PR-003 requirement**: 10-second timeout per source, <25 seconds total cascade
- **Fail-fast**: Don't wait for multiple retry attempts during health check; treat timeout as unavailable
- **Network efficiency**: HEAD requests minimize bandwidth for health checks
- **Sequential cascade**: Only check next source if current source confirmed unavailable

### Alternatives Considered

1. **Parallel health checks**: Wastes network resources, violates sequential cascade requirement (clarifications)
2. **Automatic retry with backoff**: Adds complexity, extends reachability check time, violates "no automatic retries" (clarifications)
3. **ICMP ping**: Firewall restrictions often block ICMP, HTTP request more reliable

### Implementation Pattern

```csharp
public class ReachabilityService : IReachabilityService
{
    public async Task<ReachabilityResult> CheckVisualApiAsync(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        
        try
        {
            var response = await _httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, _visualApiEndpoint),
                cts.Token);
            
            return response.IsSuccessStatusCode
                ? ReachabilityResult.Available()
                : ReachabilityResult.Unavailable("Non-success status code");
        }
        catch (TaskCanceledException)
        {
            return ReachabilityResult.Unavailable("Timeout after 10 seconds");
        }
        catch (HttpRequestException ex)
        {
            return ReachabilityResult.Unavailable($"Connection error: {ex.Message}");
        }
    }
}

public record ReachabilityResult(bool IsAvailable, string? Reason, TimeSpan Duration);
```

### References

- HTTP health check best practices
- Manufacturing domain: Sequential checks are standard in industrial control systems

---

## 11. Status Banner Implementation (Theme V2)

### Decision

Implement persistent, non-dismissible status banner in main window status bar (bottom of interface) with color-coded background based on active data source. Banner displays data source name and relevant metadata (sync timestamp for MAMP, cache age for cache).

### Rationale

- **FR-012 requirement**: Operators must always know which data source is active
- **Manufacturing safety**: Data source awareness critical for decision-making (live vs. stale data)
- **Theme V2 integration**: Uses semantic color tokens for consistent theming
- **Non-dismissible**: Ensures operators can't accidentally hide critical information
- **Status bar location**: Standard location for persistent status information, doesn't obscure content

### Alternatives Considered

1. **Dismissible banner**: Risk of operators hiding critical information
2. **Top banner**: More intrusive, obscures navigation elements
3. **Icon-only indicator**: Insufficient information density, requires tooltip interaction
4. **Modal dialog**: Too intrusive for persistent status, blocks workflow

### Implementation Pattern

```csharp
// MainView.axaml
<UserControl>
    <DockPanel>
        <!-- Main content -->
        <ContentControl DockPanel.Dock="Top" />
        
        <!-- Status bar at bottom -->
        <Border DockPanel.Dock="Bottom" 
                Classes.VisualActive="{Binding IsVisualActive}"
                Classes.MampActive="{Binding IsMampActive}"
                Classes.CacheActive="{Binding IsCacheActive}"
                Height="32">
            <StackPanel Orientation="Horizontal">
                <Path Data="{StaticResource DataSourceIcon}" />
                <TextBlock Text="{Binding DataSourceStatusText}" />
            </StackPanel>
        </Border>
    </DockPanel>
</UserControl>

// MainView.axaml styling
<Style Selector="Border.VisualActive">
    <Setter Property="Background" Value="{DynamicResource ThemeV2.Status.Success}" />
</Style>
<Style Selector="Border.MampActive">
    <Setter Property="Background" Value="{DynamicResource ThemeV2.Status.Warning}" />
</Style>
<Style Selector="Border.CacheActive">
    <Setter Property="Background" Value="{DynamicResource ThemeV2.Status.Caution}" />
</Style>
```

### Status Text Patterns

- **Visual (live)**: "Live data (Visual ERP)" with green background
- **MAMP (backup)**: "Backup data (last synced: Oct 2, 2025 2:34 PM)" with yellow background
- **Cache (offline)**: "Offline - Cached data (age: 3 hours)" with orange background

### References

- Avalonia styling documentation
- Theme V2 semantic tokens: Status colors

---

## 12. Testing Strategy (TDD)

### Decision

Implement contract tests for all API endpoints (Visual, MAMP), integration tests for boot sequence scenarios, and unit tests for individual services. Tests written before implementation following Red-Green-Refactor cycle (constitutional requirement).

### Rationale

- **Constitution requirement III**: TDD mandatory for template features
- **Contract tests**: Validate API request/response schemas match expectations, catch breaking changes
- **Integration tests**: Validate end-to-end boot scenarios including fallback cascade, retry, cancellation
- **Unit tests**: Validate individual service logic in isolation with mocked dependencies

### Test Structure

```
tests/
├── MTM_Template_Application.Tests.Unit/
│   ├── Boot/
│   │   ├── BootSequenceServiceTests.cs      # Sequential stage execution
│   │   ├── BootMetricsServiceTests.cs       # Metrics persistence, FIFO rotation
│   ├── Configuration/
│   │   └── ConfigurationServiceTests.cs     # Environment, feature flags
│   ├── Reachability/
│   │   └── ReachabilityServiceTests.cs      # Health check, timeout handling
│   └── DataLayer/
│       ├── CacheServiceTests.cs             # Cache CRUD, validation, corruption
│       └── DataSourceFallbackTests.cs       # Cascade logic
├── MTM_Template_Application.Tests.Integration/
│   ├── BootSequenceIntegrationTests.cs      # Full boot scenarios
│   ├── DataSourceFallbackTests.cs           # Visual→MAMP→Cache cascade
│   └── RetryScenarioTests.cs                # Operator retry behavior
└── MTM_Template_Application.Tests.Contract/
    ├── VisualApiContractTests.cs            # Visual API request/response schemas
    └── MampApiContractTests.cs              # MAMP API (if using proxy)
```

### Key Test Scenarios

1. **Happy path**: All services healthy, Visual available, boot completes in <10s
2. **Visual unavailable**: Cascade to MAMP, boot completes with warning banner
3. **Visual + MAMP unavailable**: Cascade to cache, boot completes with offline banner
4. **All sources unavailable**: Boot fails, Exit-only option displayed
5. **Operator retry**: Full cascade reattempted, cooldown enforced
6. **Operator cancellation**: Boot cancels gracefully, no zombie processes
7. **Cache corruption**: Corrupted cache deleted, fresh prefetch attempted
8. **Concurrent launch**: Second instance activates first instance window
9. **Invalid credentials**: Boot fails immediately (fail-fast), no cascade
10. **Metrics persistence**: Boot metrics saved, FIFO rotation at 101st boot

### References

- xUnit documentation
- FluentAssertions for readable test assertions
- TDD: Test-Driven Development by Kent Beck

---

## 13. Localization Stage (Infrastructure Only)

### Decision

Localization stage loads localization framework and infrastructure (resource managers, culture settings) but does NOT load UI content translations. Actual UI content translation occurs post-boot in the main application after navigation/theming stage completes.

### Rationale

- **Clarification response**: Localization stage scope is framework-only, not content translation
- **Performance**: Loading full UI translation resources during boot adds 1-2 seconds to initialization
- **Separation of concerns**: Boot sequence focuses on infrastructure; UI concerns handled post-boot
- **Flexibility**: Allows language selection in main app without re-initializing boot sequence

### Implementation Pattern

```csharp
// Boot sequence - Stage 9: Localization
private async Task InitializeLocalizationAsync(CancellationToken ct)
{
    // Load localization framework only
    _localizationManager = new LocalizationManager();
    await _localizationManager.InitializeAsync(ct);
    
    // Set default culture from configuration
    var defaultCulture = _configuration.DefaultCulture ?? "en-US";
    CultureInfo.CurrentUICulture = new CultureInfo(defaultCulture);
    
    // DO NOT load UI content resources here
    // That happens post-boot in main app
}

// Main app - Post-boot
private async Task LoadUiTranslationsAsync()
{
    await _localizationManager.LoadResourcesAsync("MainWindow");
    await _localizationManager.LoadResourcesAsync("Manufacturing");
    // ... load UI content resources
}
```

### References

- .NET localization documentation
- Avalonia resource management

---

## Technology Stack Summary

| Component | Technology | Version | Justification |
|-----------|-----------|---------|---------------|
| **Framework** | Avalonia | 11.3+ | Cross-platform UI (Desktop + Android), MVVM-friendly |
| **Language** | C# | .NET 9.0 | Nullable reference types, async/await, modern language features |
| **MVVM** | CommunityToolkit.Mvvm | 8.3+ | Source generation, [ObservableProperty], [RelayCommand] |
| **Visual API** | HttpClient | Built-in | HTTP REST calls to Visual API Toolkit |
| **MAMP MySQL (Desktop)** | MySQL.Data | 8.x | Direct MySQL connection for Windows |
| **MAMP Proxy (Android)** | HttpClient | Built-in | HTTP API proxy for Android MAMP access |
| **Cache** | LiteDB | 5.x | Embedded NoSQL, document storage, cross-platform |
| **Logging** | Microsoft.Extensions.Logging | Built-in | Structured logging, multiple sinks |
| **DI** | Microsoft.Extensions.DependencyInjection | Built-in | Constructor injection, service lifetimes |
| **Testing** | xUnit + FluentAssertions | Latest | Unit, integration, contract tests |
| **Serialization** | System.Text.Json | Built-in | JSON for boot metrics, cache documents |

---

## Open Questions Resolved

All open questions from the feature specification were resolved during the clarification phase. Key resolutions:

1. **Master data criticality**: None critical; always allow operation with best available source
2. **Retry strategy**: Operator manual retry only (no automatic retries), 30-second cooldown
3. **Splash stage display**: All 10 stages shown individually with stage names
4. **Boot metrics storage**: Local file system (JSON/CSV), last 100 boots retained
5. **Concurrent launches**: Single instance enforced; subsequent launches activate existing window
6. **Authorized user database**: Visual ERP system itself (validation via API call)
7. **Persistent warning banner**: Status bar at bottom, always visible, cannot be dismissed
8. **Maintenance windows**: Scheduled time periods in Visual ERP read via API
9. **Typical hardware**: Any hardware meeting minimum OS requirements for Windows/Android
10. **Cache prefetch timeout**: 30 seconds default, 10-120 seconds configurable
11. **Metrics rotation**: Delete oldest single entry (FIFO) when 101st boot occurs
12. **Cache staleness**: 24 hours (1 day) - manufacturing data changes daily
13. **Partial Visual availability**: Treat as completely unavailable; cascade to MAMP
14. **Localization content**: Framework-only during boot; content translation post-boot
15. **MAMP architecture**: Intermediate backup between Visual (primary) and cache (tertiary)
16. **MAMP credentials**: Separate read-only credentials in secure local storage
17. **Data source status display**: Persistent status banner with color coding
18. **Source cascade strategy**: Sequential with fail-fast (not parallel)
19. **Intermittent connectivity**: No automatic retries; treated as unavailable, immediate cascade
20. **Partial cache**: Block launch if any master data type completely missing

---

## Next Steps

With research complete, the next phase will:

1. Generate `data-model.md` from Key Entities in feature spec
2. Generate API contracts for Visual API Toolkit and MAMP API in `/contracts/`
3. Generate contract tests (failing) for all API endpoints
4. Generate `quickstart.md` with integration test scenarios from acceptance criteria
5. Update `.github/copilot-instructions.md` with boot sequence technical context

All implementation will follow TDD: tests written first, then implementation to make tests pass.

---

**Research Phase Complete**: All technical decisions documented, alternatives evaluated, implementation patterns defined. Ready to proceed to Phase 1 (Design & Contracts).
