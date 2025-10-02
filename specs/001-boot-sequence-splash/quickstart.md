# Quickstart Guide: Boot Sequence - Splash-First Services Initialization

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Branch**: `001-boot-sequence-splash`  
**Date**: 2025-10-02

## Purpose

This quickstart guide provides step-by-step validation scenarios for manually testing the boot sequence feature. Each scenario corresponds to an acceptance criteria from the feature specification and validates end-to-end behavior.

---

## Prerequisites

Before running these tests, ensure:

1. ✅ Development environment configured (Visual Studio 2022+ or Rider)
2. ✅ .NET 9.0 SDK installed
3. ✅ Visual API Toolkit test endpoint available OR mock service configured
4. ✅ MAMP MySQL 5.7 server available OR mock service configured (Desktop)
5. ✅ Android emulator or device available (Android tests)
6. ✅ Test credentials configured in secure local storage

---

## Test Scenarios

### Scenario 1: Happy Path - All Services Healthy

**Given**: Visual API Toolkit is reachable and responding  
**When**: Operator launches the application  
**Then**: Boot completes successfully with Visual as active data source

**Steps**:
1. Launch application (Desktop: `MTM_Template_Application.Desktop.exe` or Android: tap app icon)
2. **Verify**: Splash screen appears within 500ms
3. **Verify**: Splash displays "Initializing..." with progress indicator
4. **Verify**: Splash shows all 10 stages sequentially:
   - Configuration
   - Secrets
   - Logging
   - Diagnostics
   - Reachability
   - Data Layers
   - Core Services
   - Caching
   - Localization
   - Navigation/Theming
5. **Verify**: Each stage transitions from "In Progress" → "Completed" within reasonable time
6. **Verify**: Total boot time < 10 seconds
7. **Verify**: Splash transitions to main application interface
8. **Verify**: Main window displays with full theme applied
9. **Verify**: Status banner at bottom shows "Live data (Visual ERP)" with green background
10. **Verify**: No error messages displayed

**Success Criteria**: Boot completes in < 10 seconds, Visual data source active, no errors

---

### Scenario 2: Visual Unavailable - MAMP Fallback

**Given**: Visual API Toolkit is unreachable (e.g., endpoint down, network issue)  
**When**: Operator launches the application  
**Then**: Boot completes using MAMP as active data source with warning banner

**Steps**:
1. **Precondition**: Stop Visual API Toolkit service OR disconnect from Visual network
2. Launch application
3. **Verify**: Splash screen appears within 500ms
4. **Verify**: During "Reachability" stage:
   - Visual check attempts for 10 seconds
   - Visual check fails
   - MAMP check begins automatically (cascade)
   - MAMP check succeeds
5. **Verify**: "Data Layers" stage connects to MAMP
6. **Verify**: "Caching" stage prefetches data from MAMP
7. **Verify**: Boot completes successfully
8. **Verify**: Main window displays
9. **Verify**: Status banner shows "Backup data (last synced: [timestamp])" with yellow background
10. **Verify**: MAMP sync timestamp is displayed (e.g., "Oct 2, 2025 2:34 PM")

**Success Criteria**: Boot completes using MAMP, warning banner visible, sync timestamp shown

---

### Scenario 3: Visual + MAMP Unavailable - Cache Fallback

**Given**: Both Visual and MAMP are unreachable  
**When**: Operator launches the application  
**Then**: Boot completes using local cache with offline banner

**Steps**:
1. **Precondition**: 
   - Stop Visual API Toolkit service
   - Stop MAMP MySQL server OR disconnect from MAMP network
   - Ensure local cache has master data (run successful boot first to populate cache)
2. Launch application
3. **Verify**: Splash screen appears within 500ms
4. **Verify**: During "Reachability" stage:
   - Visual check attempts for 10 seconds → fails
   - MAMP check attempts for 10 seconds → fails
   - Cache check succeeds (always available)
5. **Verify**: Total reachability stage time ≤ 25 seconds (two 10s checks + overhead)
6. **Verify**: "Data Layers" stage uses local cache
7. **Verify**: "Caching" stage skips prefetch (using existing cache)
8. **Verify**: Boot completes successfully
9. **Verify**: Main window displays
10. **Verify**: Status banner shows "Offline - Cached data (age: [duration])" with orange background
11. **Verify**: Cache age is displayed (e.g., "3 hours")

**Success Criteria**: Boot completes using cache, offline banner visible, cache age shown

---

### Scenario 4: All Sources Unavailable - Boot Failure

**Given**: Visual, MAMP, and cache are all unavailable (cache empty or missing)  
**When**: Operator launches the application  
**Then**: Boot fails with Exit-only option

**Steps**:
1. **Precondition**: 
   - Stop Visual API Toolkit service
   - Stop MAMP MySQL server
   - Delete local cache files (e.g., `cache.db` in app data directory)
2. Launch application
3. **Verify**: Splash screen appears within 500ms
4. **Verify**: During "Reachability" stage:
   - Visual check attempts for 10 seconds → fails
   - MAMP check attempts for 10 seconds → fails
   - Cache check → reports missing/empty
5. **Verify**: During "Data Layers" stage, initialization fails
6. **Verify**: Error dialog appears on splash screen:
   - Title: "Initialization Failed"
   - Message: "Cannot connect to data sources and no cached data available."
   - Buttons: "Exit" only (no Retry button)
7. **Verify**: Clicking "Exit" closes application cleanly
8. **Verify**: No zombie processes left running

**Success Criteria**: Boot blocks with clear error message, Exit-only option, clean shutdown

---

### Scenario 5: Critical Service Failure with Retry

**Given**: Visual API Toolkit is temporarily unreachable  
**When**: Operator clicks Retry after initial failure  
**Then**: Full Visual → MAMP → Cache cascade reattempts, retry cooldown enforced

**Steps**:
1. **Precondition**: Visual API Toolkit temporarily unreachable (e.g., firewall blocking)
2. Launch application
3. **Verify**: Reachability check fails, error dialog appears with "Retry" and "Exit" buttons
4. **Verify**: Error message: "Cannot connect to Visual ERP system. Check network connection."
5. Click "Retry" button
6. **Verify**: Retry button immediately disabled (grayed out)
7. **Verify**: Full cascade reattempts:
   - Visual check (10s timeout)
   - If Visual fails → MAMP check (10s timeout)
   - If MAMP fails → Cache check
8. **Verify**: If cascade fails again, Retry button remains disabled for 30 seconds
9. **Verify**: After 30 seconds, Retry button re-enables
10. **Verify**: Counter displays "Retry available in 30s... 29s... 28s..."

**Success Criteria**: Retry performs full cascade, 30-second cooldown enforced, clear operator feedback

---

### Scenario 6: Operator Cancellation

**Given**: Boot sequence is in progress  
**When**: Operator clicks Cancel button during initialization  
**Then**: Application shuts down gracefully without zombie processes

**Steps**:
1. Launch application
2. **Verify**: Splash screen appears with boot stages
3. During "Reachability" or "Caching" stage (slow stages), click "Cancel" button
4. **Verify**: Cancellation request acknowledged immediately
5. **Verify**: Current operation stops within 2 seconds
6. **Verify**: Splash screen shows "Cancelling..." message
7. **Verify**: Application closes cleanly
8. **Verify**: No error dialogs appear
9. **Verify**: Check Task Manager (Windows) or Process Manager (Android):
   - No MTM_Template_Application processes running
   - No orphaned background processes

**Success Criteria**: Clean cancellation, no zombie processes, no corrupted state

---

### Scenario 7: Invalid Visual Credentials - Fail-Fast

**Given**: Visual credentials in secure storage are invalid  
**When**: Operator launches the application  
**Then**: Boot fails immediately without cascading to MAMP

**Steps**:
1. **Precondition**: Configure invalid Visual credentials in secure local storage
2. Launch application
3. **Verify**: Splash screen appears within 500ms
4. **Verify**: "Secrets" stage loads credentials
5. **Verify**: "Reachability" stage attempts Visual authentication
6. **Verify**: Visual auth returns 401 Unauthorized
7. **Verify**: Boot fails immediately (no MAMP/Cache cascade)
8. **Verify**: Error dialog appears:
   - Title: "Authentication Failed"
   - Message: "Visual ERP credentials are incorrect. Please update settings."
   - Button: "Exit" only
9. **Verify**: No attempt to connect to MAMP or cache
10. **Verify**: Boot metrics show failure at "Reachability" stage

**Success Criteria**: Fail-fast on invalid credentials, no cascade, clear error message

---

### Scenario 8: Boot Metrics Persistence

**Given**: Application has been launched multiple times  
**When**: Checking boot metrics file  
**Then**: Last 100 boot attempts are retained with FIFO rotation

**Steps**:
1. Launch application successfully (creates boot metric entry)
2. Close application
3. Repeat steps 1-2 multiple times (at least 5 times)
4. Navigate to application data directory (Windows: `%APPDATA%\MTM_Template_Application\boot-metrics.json`)
5. **Verify**: `boot-metrics.json` file exists
6. Open file and verify JSON structure:
   ```json
   [
     {
       "timestamp": "2025-10-02T14:23:45Z",
       "success": true,
       "totalDuration": "00:00:08.234",
       "stageDurations": { ... },
       "activeDataSource": "Visual",
       "fallbackChain": ["Visual"],
       ...
     }
   ]
   ```
7. **Verify**: Each boot attempt has entry with:
   - Timestamp
   - Success status
   - Stage durations
   - Active data source
   - Fallback chain
8. Launch application 100+ times
9. **Verify**: File contains maximum 100 entries
10. **Verify**: Oldest entries deleted (FIFO rotation)

**Success Criteria**: Boot metrics persisted correctly, FIFO rotation at 100 entries, valid JSON format

---

### Scenario 9: Stale MAMP Data Warning

**Given**: MAMP data was last synced from Visual > 24 hours ago  
**When**: Operator launches app using MAMP fallback  
**Then**: Additional warning about stale data displayed

**Steps**:
1. **Precondition**: 
   - MAMP last sync timestamp > 24 hours old (query `mamp_sync_log` table)
   - Visual unavailable (to trigger MAMP fallback)
2. Launch application
3. **Verify**: Boot uses MAMP fallback
4. **Verify**: Status banner shows "Backup data (last synced: [timestamp] - WARNING: Data is 25 hours old)"
5. **Verify**: Banner color is yellow/orange (warning state)
6. **Verify**: Tooltip or info icon explains: "MAMP data has not synced with Visual in over 24 hours. Data may be outdated."

**Success Criteria**: Stale data warning displayed when MAMP data > 24 hours old (FR-022)

---

### Scenario 10: Cache Corruption Handling

**Given**: Local cache exists but is corrupted (invalid checksums)  
**When**: Operator launches application with Visual unavailable  
**Then**: Corrupted cache deleted, fresh prefetch attempted or boot blocks

**Steps**:
1. **Precondition**: 
   - Manually corrupt cache file (e.g., modify `cache.db` bytes)
   - Stop Visual API Toolkit (trigger cache usage)
2. Launch application
3. **Verify**: During "Caching" stage, cache validation fails
4. **Verify**: Log message: "Cache corruption detected, deleting corrupted cache files"
5. **Verify**: Cache files deleted from disk
6. **Verify**: If Visual becomes reachable, fresh prefetch attempted
7. **Verify**: If Visual unavailable, boot blocks with error:
   - Message: "Cached data is corrupted and data sources are unavailable."
   - Button: "Exit" only

**Success Criteria**: Corrupted cache detected and deleted, fresh prefetch attempted, clear error if no sources available

---

### Scenario 11: Partial Cache (Missing Master Data Type)

**Given**: Cache contains Items and Locations but missing WorkCenters  
**When**: Operator launches application with Visual and MAMP unavailable  
**Then**: Boot blocks with error about incomplete cache

**Steps**:
1. **Precondition**: 
   - Populate cache with Items (100 records) and Locations (50 records)
   - Delete WorkCenters from cache (0 records)
   - Stop Visual and MAMP servers
2. Launch application
3. **Verify**: Reachability cascade fails (Visual → MAMP both unavailable)
4. **Verify**: "Caching" stage checks cache completeness
5. **Verify**: Cache validation detects missing WorkCenters
6. **Verify**: Error dialog appears:
   - Message: "Cache is incomplete. Missing master data: WorkCenters. Cannot launch in offline mode."
   - Button: "Exit" only
7. **Verify**: Application does not launch (boot blocked)

**Success Criteria**: Boot blocks if any master data type completely missing, clear error message

---

### Scenario 12: Concurrent Launch (Single Instance Enforcement)

**Given**: Application is already running  
**When**: Operator double-clicks application icon again  
**Then**: Existing window activated, no second instance starts

**Steps**:
1. Launch application (first instance)
2. **Verify**: Application starts normally
3. While first instance is running, double-click application icon again
4. **Verify**: Existing window brought to foreground
5. **Verify**: No splash screen appears for second launch
6. **Verify**: No error message displayed
7. **Verify**: Check Task Manager/Process Manager:
   - Only ONE MTM_Template_Application process running
8. **Verify**: First instance remains functional (no disruption)

**Success Criteria**: Single instance enforced, existing window activated, no duplicate processes

---

## Automated Test Mapping

Each scenario above maps to automated integration tests:

| Scenario | Test Class | Test Method |
|----------|------------|-------------|
| 1. Happy Path | `BootSequenceIntegrationTests` | `Boot_WithAllServicesHealthy_CompletesSuccessfully` |
| 2. MAMP Fallback | `DataSourceFallbackTests` | `Boot_WithVisualUnavailable_FallsBackToMamp` |
| 3. Cache Fallback | `DataSourceFallbackTests` | `Boot_WithVisualAndMampUnavailable_FallsBackToCache` |
| 4. All Unavailable | `DataSourceFallbackTests` | `Boot_WithAllSourcesUnavailable_FailsWithExitOnly` |
| 5. Retry | `RetryScenarioTests` | `Boot_WithRetry_ReattemptsCascadeAfterCooldown` |
| 6. Cancellation | `BootSequenceIntegrationTests` | `Boot_WithOperatorCancellation_ShutsDownGracefully` |
| 7. Invalid Credentials | `BootSequenceIntegrationTests` | `Boot_WithInvalidCredentials_FailsFast` |
| 8. Metrics Persistence | `BootMetricsServiceTests` | `PersistBootMetrics_WithMultipleBoots_EnforcesFifoRotation` |
| 9. Stale MAMP Warning | `DataSourceFallbackTests` | `Boot_WithStaleMampData_DisplaysWarning` |
| 10. Cache Corruption | `CacheServiceTests` | `ValidateCache_WithCorruption_DeletesAndRefreshes` |
| 11. Partial Cache | `CacheServiceTests` | `ValidateCache_WithMissingEntityType_BlocksBoot` |
| 12. Single Instance | `BootSequenceIntegrationTests` | `Boot_WithConcurrentLaunch_ActivatesExistingInstance` |

---

## Performance Benchmarks

Expected performance targets for manual validation:

| Metric | Target | Measurement |
|--------|--------|-------------|
| Splash display time | < 500ms | Stopwatch from app launch to splash visible |
| Normal boot (all healthy) | < 10s | Total boot sequence duration |
| Reachability check (single source) | ≤ 10s | Visual or MAMP health check timeout |
| Reachability cascade (all sources) | ≤ 25s | Visual (10s) + MAMP (10s) + Cache (instant) + overhead |
| Cache prefetch | ≤ 30s | Default timeout (configurable 10-120s) |
| Retry button cooldown | 30s | Time from Retry click to re-enable |
| Memory during init | < 200MB | Task Manager/Process Monitor |

---

## Troubleshooting

### Common Issues

**Splash doesn't appear within 500ms**:
- Check if theme resources loading before splash (violates FR-001)
- Verify splash window creation happens in Program.cs before App initialization

**Reachability checks timeout**:
- Verify network connectivity to Visual/MAMP endpoints
- Check firewall rules blocking HTTP/MySQL traffic
- Validate timeout configuration in `ServiceConfiguration`

**Cache always reports missing data**:
- Verify cache directory permissions (read/write access)
- Check cache file path in application data directory
- Validate LiteDB initialization code

**Single instance not working**:
- Verify Mutex name uniqueness (Desktop)
- Check Activity launch mode on Android (should be SingleTop)
- Ensure named Mutex logic in Program.cs

---

## Next Steps

After completing these manual quickstart scenarios:

1. ✅ Run automated integration tests: `dotnet test`
2. ✅ Review boot metrics JSON for anomalies
3. ✅ Performance profiling with Avalonia DevTools
4. ✅ Cross-platform testing (Windows + Android)
5. ✅ Stress testing (rapid launches, network instability)

---

**Completion**: All 12 scenarios passing = Boot sequence feature ready for production
