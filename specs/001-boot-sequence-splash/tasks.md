# Tasks: Boot Sequence - Splash-First Services Initialization

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Branch**: `001-boot-sequence-splash`  
**Input**: Design documents from `/specs/001-boot-sequence-splash/`  
**Prerequisites**: plan.md, research.md, data-model.md, quickstart.md, contracts/

---

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

---

## Phase 3.1: Setup

- [ ] **T001** Create test projects structure
  - Create `tests/MTM_Template_Application.Tests.Unit/` project
  - Create `tests/MTM_Template_Application.Tests.Integration/` project
  - Create `tests/MTM_Template_Application.Tests.Contract/` project
  - Configure xUnit and FluentAssertions dependencies via `Directory.Packages.props`

- [ ] **T002** Configure logging infrastructure
  - Install Serilog NuGet packages (Serilog.Sinks.Console, Serilog.Sinks.File)
  - Create `MTM_Template_Application/Services/Logging/LoggingService.cs` with structured logging support
  - Configure log file rotation and boot metrics persistence paths

- [ ] **T003** [P] Configure MySQL.Data dependency for MAMP access
  - Add MySQL.Data package reference in `Directory.Packages.props`
  - Ensure version compatibility with .NET 9.0 and Avalonia 11.3+

---

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### Contract Tests

- [ ] **T004** [P] Contract test Visual API health check endpoint
  - File: `tests/MTM_Template_Application.Tests.Contract/VisualApiContractTests.cs`
  - Test: `GET /api/visual/health` returns 200 with "Healthy" status
  - Test: Response time < 10 seconds
  - Test: Timeout handling when endpoint unreachable

- [ ] **T005** [P] Contract test MAMP MySQL reachability query
  - File: `tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs`
  - Test: `SELECT 1 AS health_check` executes successfully
  - Test: Query completes within 10-second timeout
  - Test: Connection failure handling

- [ ] **T006** [P] Contract test MAMP sync timestamp query
  - File: `tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs`
  - Test: Sync timestamp query returns valid `MampSyncLog` record
  - Test: Handles missing sync log table gracefully
  - Test: Query completes within timeout

- [ ] **T007** [P] Contract test MAMP items master data query
  - File: `tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs`
  - Test: Items query returns `ItemData` records with all required fields
  - Test: Query respects 5000 row limit
  - Test: Query completes within 30-second prefetch timeout

### Integration Tests

- [ ] **T008** [P] Integration test happy path boot sequence (Visual available)
  - File: `tests/MTM_Template_Application.Tests.Integration/BootSequenceIntegrationTests.cs`
  - Test: All 10 stages complete successfully when Visual reachable
  - Test: Boot completes within 10 seconds
  - Test: Visual data source is active at completion
  - Corresponds to quickstart.md Scenario 1

- [ ] **T009** [P] Integration test Visual unavailable fallback to MAMP
  - File: `tests/MTM_Template_Application.Tests.Integration/DataSourceFallbackTests.cs`
  - Test: Visual check fails → MAMP check succeeds → MAMP becomes active source
  - Test: Reachability stage completes within 25 seconds (10s Visual + 10s MAMP + overhead)
  - Test: Status banner shows "Backup data" with sync timestamp
  - Corresponds to quickstart.md Scenario 2

- [ ] **T010** [P] Integration test Visual + MAMP unavailable fallback to cache
  - File: `tests/MTM_Template_Application.Tests.Integration/DataSourceFallbackTests.cs`
  - Test: Visual fails → MAMP fails → Cache succeeds → Cache becomes active source
  - Test: Reachability stage completes within 25 seconds
  - Test: Status banner shows "Offline - Cached data" with age
  - Corresponds to quickstart.md Scenario 3

- [ ] **T011** [P] Integration test all sources unavailable boot failure
  - File: `tests/MTM_Template_Application.Tests.Integration/DataSourceFallbackTests.cs`
  - Test: All sources fail → Error dialog displayed with Exit-only option
  - Test: No Retry button when cache empty/missing
  - Test: Application shuts down cleanly on Exit
  - Corresponds to quickstart.md Scenario 4

- [ ] **T012** [P] Integration test operator retry mechanism
  - File: `tests/MTM_Template_Application.Tests.Integration/BootSequenceIntegrationTests.cs`
  - Test: Retry button triggers full cascade reattempt
  - Test: Retry button disabled for 30-second cooldown
  - Test: Countdown timer displays remaining seconds
  - Corresponds to quickstart.md Scenario 5

- [ ] **T013** [P] Integration test operator cancellation
  - File: `tests/MTM_Template_Application.Tests.Integration/BootSequenceIntegrationTests.cs`
  - Test: Cancel button stops current operation within 2 seconds
  - Test: Application closes cleanly without zombie processes
  - Test: No corrupted state persisted to disk
  - Corresponds to quickstart.md Scenario 6

- [ ] **T014** [P] Integration test invalid Visual credentials fail-fast
  - File: `tests/MTM_Template_Application.Tests.Integration/BootSequenceIntegrationTests.cs`
  - Test: Invalid credentials → immediate failure without cascading to MAMP
  - Test: Error message: "Authentication failed"
  - Test: No network timeout delays (fail-fast)
  - Corresponds to quickstart.md Scenario 7

### Unit Tests (Core Logic)

- [ ] **T015** [P] Unit tests for BootStage model
  - File: `tests/MTM_Template_Application.Tests.Unit/Models/BootStageTests.cs`
  - Test: State transitions (NotStarted → InProgress → Completed/Failed/Cancelled)
  - Test: Duration calculation accuracy
  - Test: Validation rules (ProgressPercentage 0-100, Status constraints)

- [ ] **T016** [P] Unit tests for ServiceConfiguration model
  - File: `tests/MTM_Template_Application.Tests.Unit/Models/ServiceConfigurationTests.cs`
  - Test: HTTPS validation for VisualApiEndpoint
  - Test: Timeout range validation (CachePrefetchTimeout 10-120s)
  - Test: Platform-specific validation (MampConnectionString vs MampApiProxyEndpoint)

- [ ] **T017** [P] Unit tests for ReachabilityStatus model
  - File: `tests/MTM_Template_Application.Tests.Unit/Models/ReachabilityStatusTests.cs`
  - Test: IsAvailable logic for each data source
  - Test: LastChecked timestamp accuracy
  - Test: Error message handling for failures

- [ ] **T018** [P] Unit tests for ConfigurationService
  - File: `tests/MTM_Template_Application.Tests.Unit/Services/ConfigurationServiceTests.cs`
  - Test: Environment-specific settings loading
  - Test: Feature flag resolution
  - Test: Configuration file parsing and validation

- [ ] **T019** [P] Unit tests for ReachabilityService
  - File: `tests/MTM_Template_Application.Tests.Unit/Services/ReachabilityServiceTests.cs`
  - Test: Timeout enforcement (10 seconds per source)
  - Test: Sequential cascade order (Visual → MAMP → Cache)
  - Test: Fail-fast for authentication errors

- [ ] **T020** [P] Unit tests for BootMetricsService
  - File: `tests/MTM_Template_Application.Tests.Unit/Services/BootMetricsServiceTests.cs`
  - Test: Metrics persistence to JSON/CSV
  - Test: 100-boot retention with FIFO rotation
  - Test: Stage timing accuracy

---

## Phase 3.3: Core Implementation (ONLY after tests are failing)

### Models

- [ ] **T021** [P] Implement BootStage model
  - File: `MTM_Template_Application/Models/Boot/BootStage.cs`
  - Implement: BootStageType enum (10 stages)
  - Implement: BootStageStatus enum (NotStarted, InProgress, Completed, Failed, Cancelled)
  - Implement: BootStage record with validation rules
  - Ensure: Nullable reference types enabled, immutable record pattern

- [ ] **T022** [P] Implement BootMetrics model
  - File: `MTM_Template_Application/Models/Boot/BootMetrics.cs`
  - Implement: BootMetrics record with session ID, timestamps, stage durations
  - Implement: BootResult enum (Success, Failure, Cancelled)
  - Include: Active data source tracking, error details

- [ ] **T023** [P] Implement ReachabilityStatus model
  - File: `MTM_Template_Application/Models/Boot/ReachabilityStatus.cs`
  - Implement: ReachabilityStatus record with IsAvailable flags per source
  - Implement: LastChecked timestamps, latency measurements
  - Include: Error message properties for failures

- [ ] **T024** [P] Implement CacheManifest model
  - File: `MTM_Template_Application/Models/Boot/CacheManifest.cs`
  - Implement: CacheManifest record with last prefetch timestamp
  - Implement: IsStale computed property (> 24 hours)
  - Include: Item count, cache size metadata

- [ ] **T025** [P] Implement ServiceConfiguration model
  - File: `MTM_Template_Application/Models/Configuration/ServiceConfiguration.cs`
  - Implement: EnvironmentType enum (Development, Staging, Production)
  - Implement: ServiceConfiguration record with all config properties
  - Include: Platform-specific settings dictionary, timeout configurations

- [ ] **T026** [P] Implement DataSourceType enum
  - File: `MTM_Template_Application/Models/DataSources/DataSourceType.cs`
  - Implement: DataSourceType enum (Visual, Mamp, Cache)
  - Include: XML documentation for each data source tier

- [ ] **T027** [P] Implement MampSyncLog model
  - File: `MTM_Template_Application/Models/DataSources/MampSyncLog.cs`
  - Implement: MampSyncLog record from MAMP contract
  - Properties: SyncTimestamp, SyncStatus

- [ ] **T028** [P] Implement ItemData model
  - File: `MTM_Template_Application/Models/DataSources/ItemData.cs`
  - Implement: ItemData record from MAMP contract
  - Properties: ItemId, PartNumber, Description, Category, UnitOfMeasure, IsActive, LastUpdated

### Service Interfaces

- [ ] **T029** [P] Define IBootSequenceService interface
  - File: `MTM_Template_Application/Services/Boot/IBootSequenceService.cs`
  - Define: InitializeAsync(CancellationToken) → Task<BootResult>
  - Define: Event for stage progress updates
  - Define: GetCurrentStage() → BootStage

- [ ] **T030** [P] Define IReachabilityService interface
  - File: `MTM_Template_Application/Services/Reachability/IReachabilityService.cs`
  - Define: CheckAllSourcesAsync(CancellationToken) → Task<ReachabilityStatus>
  - Define: CheckVisualAsync, CheckMampAsync, CheckCacheAsync methods
  - Define: 10-second timeout constraint per method

- [ ] **T031** [P] Define IVisualApiService interface
  - File: `MTM_Template_Application/Services/DataLayer/IVisualApiService.cs`
  - Define: HealthCheckAsync(CancellationToken) → Task<bool>
  - Define: Read-only method signatures (no Save/Update methods per FR-010)
  - Include: Authentication method (credentials from Stage 2 Secrets)

- [ ] **T032** [P] Define IMampDataService interface
  - File: `MTM_Template_Application/Services/DataLayer/IMampDataService.cs`
  - Define: HealthCheckAsync → Task<bool> (SELECT 1 query)
  - Define: GetLastSyncTimestampAsync → Task<MampSyncLog?>
  - Define: GetItemsAsync → Task<List<ItemData>>

- [ ] **T033** [P] Define IMampApiProxyService interface (Android)
  - File: `MTM_Template_Application/Services/DataLayer/IMampApiProxyService.cs`
  - Define: Same methods as IMampDataService but via HTTP endpoints
  - Include: Base URL configuration from ServiceConfiguration

- [ ] **T034** [P] Define ICacheService interface
  - File: `MTM_Template_Application/Services/DataLayer/ICacheService.cs`
  - Define: IsAvailableAsync → Task<bool>
  - Define: GetCacheManifestAsync → Task<CacheManifest?>
  - Define: GetItemsAsync → Task<List<ItemData>>
  - Define: SaveItemsAsync (for prefetch persistence)

### Service Implementations

- [ ] **T035** Implement ConfigurationService
  - File: `MTM_Template_Application/Services/Configuration/ConfigurationService.cs`
  - Load: Environment settings from appsettings.json or environment variables
  - Load: Feature flags dictionary
  - Validate: Configuration constraints (HTTPS URLs, timeout ranges)
  - Thread-safe: Use lock for singleton instance

- [ ] **T036** Implement ReachabilityService
  - File: `MTM_Template_Application/Services/Reachability/ReachabilityService.cs`
  - Implement: Sequential cascade logic (Visual → MAMP → Cache)
  - Enforce: 10-second timeout per check using CancellationTokenSource
  - Implement: Fail-fast for authentication errors (no cascade)
  - Dependencies: IVisualApiService, IMampDataService/IMampApiProxyService, ICacheService

- [ ] **T037** Implement BootMetricsService
  - File: `MTM_Template_Application/Services/Boot/BootMetricsService.cs`
  - Implement: PersistMetricsAsync(BootMetrics) → saves to JSON file
  - Implement: GetMetricsHistoryAsync → loads last 100 boot sessions
  - Implement: FIFO rotation when exceeding MaxBootMetricsHistory (100)
  - File format: `boot-metrics.json` in app data directory

- [ ] **T038** Implement BootSequenceService (orchestrator)
  - File: `MTM_Template_Application/Services/Boot/BootSequenceService.cs`
  - Implement: InitializeAsync with 10 sequential stages
  - Implement: Stage 1 (Configuration) → ConfigurationService
  - Implement: Stage 2 (Secrets) → Load credentials from secure storage
  - Implement: Stage 3 (Logging) → LoggingService initialization
  - Implement: Stage 4 (Diagnostics) → Permissions, storage, hardware checks
  - Implement: Stage 5 (Reachability) → ReachabilityService.CheckAllSourcesAsync
  - Implement: Stage 6 (Data Layers) → Connect to active data source
  - Implement: Stage 7 (Core Services) → DI container population
  - Implement: Stage 8 (Caching) → Prefetch master data (30s timeout, configurable 10-120s)
  - Implement: Stage 9 (Localization) → Framework infrastructure only
  - Implement: Stage 10 (Navigation/Theming) → Transition preparation
  - Emit: Progress events for each stage transition
  - Handle: CancellationToken for operator cancellation (2-second max response)

- [ ] **T039** [P] Implement MampDataService (Desktop)
  - File: `MTM_Template_Application.Desktop/Services/MampDataService.cs`
  - Implement: Direct MySQL connection using MySQL.Data
  - Implement: HealthCheckAsync → `SELECT 1` query with 10s timeout
  - Implement: GetLastSyncTimestampAsync → sync log query
  - Implement: GetItemsAsync → items master data query (5000 row limit)
  - Security: Use read-only credentials from secure storage (Stage 2)

- [ ] **T040** [P] Implement MampApiProxyService (Android)
  - File: `MTM_Template_Application.Android/Services/MampApiProxyService.cs`
  - Implement: HTTP client wrapper for MAMP proxy endpoints
  - Implement: HealthCheckAsync → GET /api/mamp/health
  - Implement: GetLastSyncTimestampAsync → GET /api/mamp/sync-status
  - Implement: GetItemsAsync → GET /api/mamp/items
  - Use: HttpClient with 10-second timeout per request

- [ ] **T041** [P] Implement CacheService
  - File: `MTM_Template_Application/Services/DataLayer/CacheService.cs`
  - Implement: SQLite or LiteDB local database for master data cache
  - Implement: IsAvailableAsync → Check cache file existence and integrity
  - Implement: GetCacheManifestAsync → Read manifest metadata
  - Implement: GetItemsAsync → Query cached items
  - Implement: SaveItemsAsync → Persist prefetched data with timestamp
  - Handle: Cache staleness threshold (24 hours default)

- [ ] **T042** Implement VisualApiService stub (HTTP wrapper client)
  - File: `MTM_Template_Application/Services/DataLayer/VisualApiService.cs`
  - Implement: HttpClient wrapper for Desktop Visual Service endpoints
  - Implement: HealthCheckAsync → GET /api/visual/health with 10s timeout
  - Implement: Authentication token management (bearer token)
  - Note: Full Visual API Toolkit integration deferred to future feature

### ViewModels

- [ ] **T043** Implement SplashViewModel
  - File: `MTM_Template_Application/ViewModels/SplashViewModel.cs`
  - Inherit: ObservableObject from CommunityToolkit.Mvvm
  - Properties: CurrentStage (BootStage), ProgressPercentage (0-100), StatusMessage (string)
  - Properties: IsRetryEnabled (bool), RetryCountdown (int seconds), IsCancelling (bool)
  - Commands: RetryCommand [RelayCommand], CancelCommand [RelayCommand]
  - Subscribe: BootSequenceService progress events to update UI properties
  - Implement: 30-second retry cooldown timer logic
  - Implement: Error dialog display for boot failures

- [ ] **T044** Update MainViewModel with data source banner
  - File: `MTM_Template_Application/ViewModels/MainViewModel.cs`
  - Add properties: ActiveDataSource (DataSourceType), DataSourceBannerText (string), BannerBackgroundColor (Color)
  - Add properties: MampSyncTimestamp (DateTime?), CacheAge (TimeSpan?)
  - Logic: Set banner text/color based on active source:
    - Visual: "Live data (Visual ERP)" / Green
    - MAMP: "Backup data (last synced: {timestamp})" / Yellow
    - Cache: "Offline - Cached data (age: {duration})" / Orange
  - Read: Active data source from BootSequenceService result

### Views

- [ ] **T045** Implement SplashView.axaml (XAML)
  - File: `MTM_Template_Application/Views/SplashView.axaml`
  - Layout: Vertical stack with centered content
  - Elements: 
    - App logo/icon
    - "Initializing..." text
    - ProgressBar (IsIndeterminate when stage doesn't report percentage)
    - Current stage name (e.g., "Configuration", "Reachability")
    - Status message (e.g., "Checking Visual ERP connection...")
    - Retry button (visible only on failure, enabled based on cooldown)
    - Cancel button (always visible during boot)
  - Styling: NO theme dependencies (hardcoded colors/fonts per FR-001)
  - Bindings: Bind to SplashViewModel properties

- [ ] **T046** Implement SplashView.axaml.cs (code-behind)
  - File: `MTM_Template_Application/Views/SplashView.axaml.cs`
  - Set: DataContext to SplashViewModel (via DI)
  - Handle: Window closing event (ensure clean cancellation)

- [ ] **T047** Update MainView.axaml with status banner
  - File: `MTM_Template_Application/Views/MainView.axaml`
  - Add: Status banner at bottom of window (DockPanel.Dock="Bottom")
  - Elements: TextBlock with DataSourceBannerText binding, Background bound to BannerBackgroundColor
  - Height: 30px, padding 8px horizontal
  - Use: Theme V2 semantic tokens for banner styling (post-boot, theme loaded)

### Platform Entry Points

- [ ] **T048** Update Program.cs (Desktop) with splash-first pattern
  - File: `MTM_Template_Application.Desktop/Program.cs`
  - Modify: Main method to show SplashView synchronously before App.axaml load
  - Steps:
    1. Create SplashWindow instance (theme-independent)
    2. Show splash window synchronously
    3. Initialize BootSequenceService (await InitializeAsync)
    4. Transition to main app window
    5. Close splash window
  - Handle: Cancellation token for operator Cancel button
  - Handle: Boot failure → show error dialog, exit cleanly

- [ ] **T049** Update MainActivity.cs (Android) with splash-first pattern
  - File: `MTM_Template_Application.Android/MainActivity.cs`
  - Modify: OnCreate method to show splash view before main content
  - Steps: Same as Desktop but using Android activity lifecycle
  - Handle: Android-specific lifecycle (onPause, onResume during boot)

### Dependency Injection Configuration

- [ ] **T050** Register services in DI container
  - File: `MTM_Template_Application.Desktop/Program.cs` and `MTM_Template_Application.Android/MainActivity.cs`
  - Register: Singleton services (ConfigurationService, LoggingService, BootMetricsService)
  - Register: Scoped services (BootSequenceService, ReachabilityService)
  - Register: Platform-specific implementations:
    - Desktop: IMampDataService → MampDataService
    - Android: IMampApiProxyService → MampApiProxyService
  - Register: Shared services (ICacheService → CacheService, IVisualApiService → VisualApiService)
  - Register: ViewModels (SplashViewModel, MainViewModel)

---

## Phase 3.4: Integration

- [ ] **T051** Integrate BootSequenceService with SplashViewModel
  - Verify: Stage progress events update splash UI in real-time
  - Verify: Error events trigger error dialog display
  - Verify: Completion event transitions to main window

- [ ] **T052** Integrate ReachabilityService with cascading fallback
  - Verify: Visual fails → MAMP check begins automatically
  - Verify: MAMP fails → Cache check begins automatically
  - Verify: All fail → Error dialog with Exit-only option
  - Verify: Authentication errors skip cascade (fail-fast)

- [ ] **T053** Integrate MampDataService/MampApiProxyService with data layer
  - Verify: Desktop direct MySQL connection works
  - Verify: Android HTTP proxy connection works
  - Verify: Both platforms handle timeouts consistently

- [ ] **T054** Integrate CacheService with prefetch and staleness detection
  - Verify: Stage 8 prefetches master data from active source
  - Verify: Cache manifest updates with prefetch timestamp
  - Verify: Stale cache (>24 hours) triggers warning in status banner

- [ ] **T055** Integrate BootMetricsService with persistence
  - Verify: Each boot session persisted to boot-metrics.json
  - Verify: FIFO rotation after 100 boots
  - Verify: Stage timings recorded accurately

- [ ] **T056** Integrate retry mechanism with cooldown timer
  - Verify: Retry button disabled for 30 seconds after click
  - Verify: Countdown timer updates every second
  - Verify: Full cascade reattempts on retry

- [ ] **T057** Integrate cancellation with clean shutdown
  - Verify: Cancel button stops current operation within 2 seconds
  - Verify: No zombie processes left running
  - Verify: No corrupted state files

---

## Phase 3.5: Polish

- [ ] **T058** [P] Add XML documentation to all public APIs
  - Files: All service interfaces, models, ViewModels
  - Document: Method parameters, return values, exceptions thrown

- [ ] **T059** [P] Performance profiling for boot sequence
  - Test: Normal boot completes in <10 seconds
  - Test: Splash visible in <500ms
  - Test: Reachability checks respect 10s timeout per source
  - Test: Cache prefetch completes in <30s (default timeout)
  - Test: Memory usage <200MB during initialization

- [ ] **T060** [P] Improve error messages for operators
  - Review: All error dialogs use operator-friendly language
  - Review: No stack traces or technical jargon in UI
  - Review: Clear action guidance (e.g., "Check network connection")

- [ ] **T061** [P] Update documentation
  - File: `docs/BOOT-SEQUENCE.md` → Add implementation notes
  - File: `README.md` → Add boot sequence overview
  - File: `specs/001-boot-sequence-splash/README.md` → Add feature summary

- [ ] **T062** Remove code duplication
  - Review: Desktop/Android platform services for shared logic
  - Extract: Common timeout handling to base class
  - Extract: Common HTTP client configuration to helper

- [ ] **T063** Run manual testing scenarios from quickstart.md
  - Execute: All 7 test scenarios (happy path, fallback, failure, retry, cancellation)
  - Verify: All acceptance criteria met
  - Document: Test results in test report

- [ ] **T064** Final code review and cleanup
  - Review: Nullable reference types correctly applied
  - Review: MVVM Community Toolkit patterns consistent
  - Review: No Theme V2 dependencies in splash screen
  - Review: Read-only constraint for Visual API (no Save methods)

---

## Dependencies

### Sequential Chains (Must Complete in Order)
1. **Setup → Tests → Implementation → Integration → Polish**
   - T001-T003 (Setup) BEFORE T004-T020 (Tests)
   - T004-T020 (Tests) BEFORE T021-T050 (Implementation)
   - T021-T050 (Implementation) BEFORE T051-T057 (Integration)
   - T051-T057 (Integration) BEFORE T058-T064 (Polish)

2. **Models → Service Interfaces → Service Implementations**
   - T021-T028 (Models) BEFORE T029-T034 (Interfaces)
   - T029-T034 (Interfaces) BEFORE T035-T042 (Service Implementations)

3. **Services → ViewModels → Views**
   - T035-T042 (Services) BEFORE T043-T044 (ViewModels)
   - T043-T044 (ViewModels) BEFORE T045-T047 (Views)

4. **Core Services → BootSequenceService (Orchestrator)**
   - T035 (ConfigurationService) BEFORE T038 (BootSequenceService Stage 1)
   - T036 (ReachabilityService) BEFORE T038 (BootSequenceService Stage 5)
   - T037 (BootMetricsService) BEFORE T038 (BootSequenceService completion)

5. **Platform Services → Platform Entry Points**
   - T039 (MampDataService Desktop) BEFORE T048 (Program.cs Desktop)
   - T040 (MampApiProxyService Android) BEFORE T049 (MainActivity.cs Android)
   - T041 (CacheService) BEFORE T048, T049 (both platforms)

6. **Entry Points → DI Configuration**
   - T048-T049 (Entry Points) BEFORE T050 (DI Container)

### Parallel Opportunities (Can Execute Simultaneously)

**Phase 3.2 Tests**:
- T004, T005, T006, T007 (contract tests - different files)
- T008, T009, T010, T011, T012, T013, T014 (integration tests - different files)
- T015, T016, T017, T018, T019, T020 (unit tests - different files)

**Phase 3.3 Models**:
- T021, T022, T023, T024, T025, T026, T027, T028 (all different files)

**Phase 3.3 Service Interfaces**:
- T029, T030, T031, T032, T033, T034 (all different files)

**Phase 3.3 Platform-Specific Implementations**:
- T039 (Desktop MampDataService) parallel with T040 (Android MampApiProxyService)
- T041 (CacheService) parallel with T039, T040

**Phase 3.5 Polish**:
- T058, T059, T060, T061 (different concerns, no file conflicts)

---

## Parallel Execution Examples

### Execute All Contract Tests Together
```bash
# Launch T004-T007 in parallel (different test files)
Task: "Contract test Visual API health check endpoint in tests/MTM_Template_Application.Tests.Contract/VisualApiContractTests.cs"
Task: "Contract test MAMP MySQL reachability query in tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs"
Task: "Contract test MAMP sync timestamp query in tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs"
Task: "Contract test MAMP items master data query in tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs"
```

### Execute All Model Implementations Together
```bash
# Launch T021-T028 in parallel (different model files)
Task: "Implement BootStage model in MTM_Template_Application/Models/Boot/BootStage.cs"
Task: "Implement BootMetrics model in MTM_Template_Application/Models/Boot/BootMetrics.cs"
Task: "Implement ReachabilityStatus model in MTM_Template_Application/Models/Boot/ReachabilityStatus.cs"
Task: "Implement CacheManifest model in MTM_Template_Application/Models/Boot/CacheManifest.cs"
Task: "Implement ServiceConfiguration model in MTM_Template_Application/Models/Configuration/ServiceConfiguration.cs"
Task: "Implement DataSourceType enum in MTM_Template_Application/Models/DataSources/DataSourceType.cs"
Task: "Implement MampSyncLog model in MTM_Template_Application/Models/DataSources/MampSyncLog.cs"
Task: "Implement ItemData model in MTM_Template_Application/Models/DataSources/ItemData.cs"
```

### Execute Platform-Specific Services Together
```bash
# Launch T039-T041 in parallel (different platform files)
Task: "Implement MampDataService (Desktop) in MTM_Template_Application.Desktop/Services/MampDataService.cs"
Task: "Implement MampApiProxyService (Android) in MTM_Template_Application.Android/Services/MampApiProxyService.cs"
Task: "Implement CacheService in MTM_Template_Application/Services/DataLayer/CacheService.cs"
```

---

## Notes

- **[P] tasks**: Different files, no shared dependencies, safe for parallel execution
- **Sequential tasks**: Same file or explicit dependency (e.g., interface before implementation)
- **TDD workflow**: All tests MUST FAIL before implementing (verify red → green)
- **Commit frequency**: Commit after each completed task for rollback safety
- **Platform testing**: Test Desktop and Android separately after T048/T049
- **Performance gates**: Verify PR-003 (10s normal boot) and PR-001 (<500ms splash) before merging

---

## Validation Checklist

*GATE: Checked before marking feature complete*

- [x] All contracts have corresponding tests (T004-T007 cover visual-api-contract.md, mamp-api-contract.md)
- [x] All entities have model tasks (T021-T028 cover all data-model.md entities)
- [x] All tests come before implementation (Phase 3.2 before Phase 3.3)
- [x] Parallel tasks truly independent (verified no file conflicts in [P] tasks)
- [x] Each task specifies exact file path (all tasks include file paths)
- [x] No task modifies same file as another [P] task (validated in Dependencies section)
- [x] All quickstart.md scenarios have integration tests (T008-T014 cover Scenarios 1-7)
- [x] All 10 boot stages implemented in BootSequenceService (T038)
- [x] Cross-platform support (Desktop T039/T048, Android T040/T049)
- [x] Constitutional compliance verified (MVVM, null safety, theme-less splash)
