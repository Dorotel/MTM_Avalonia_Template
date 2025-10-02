# Data Model: Boot Sequence - Splash-First Services Initialization

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Branch**: `001-boot-sequence-splash`  
**Date**: 2025-10-02

## Model Overview

This document defines the data entities required for the boot sequence feature, extracted from the Key Entities section of the feature specification. All models use nullable reference types enabled (.NET 9.0) and follow MVVM Community Toolkit patterns where applicable.

---

## Core Entities

### 1. BootStage

Represents a discrete initialization phase with status, progress, and timing information.

```csharp
namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Represents the ten sequential boot initialization stages.
/// </summary>
public enum BootStageType
{
    /// <summary>Stage 0: Pre-initialization (splash display)</summary>
    PreInit = 0,
    
    /// <summary>Stage 1.1: Configuration loading (environment, feature flags)</summary>
    Configuration = 1,
    
    /// <summary>Stage 1.2: Secrets loading (credentials from secure storage)</summary>
    Secrets = 2,
    
    /// <summary>Stage 1.3: Logging initialization (console, file sinks)</summary>
    Logging = 3,
    
    /// <summary>Stage 1.4: System diagnostics (permissions, storage, hardware)</summary>
    Diagnostics = 4,
    
    /// <summary>Stage 1.5: Reachability checks (Visual, MAMP, cache)</summary>
    Reachability = 5,
    
    /// <summary>Stage 1.6: Data layer connections (Visual API, MAMP MySQL, cache)</summary>
    DataLayers = 6,
    
    /// <summary>Stage 1.7: Core services initialization (DI container population)</summary>
    CoreServices = 7,
    
    /// <summary>Stage 1.8: Caching (master data prefetch)</summary>
    Caching = 8,
    
    /// <summary>Stage 1.9: Localization (framework infrastructure only)</summary>
    Localization = 9,
    
    /// <summary>Stage 1.10: Navigation and theming (transition to main app)</summary>
    NavigationTheming = 10
}

/// <summary>
/// Represents the execution status of a boot stage.
/// </summary>
public enum BootStageStatus
{
    /// <summary>Stage has not started yet</summary>
    NotStarted,
    
    /// <summary>Stage is currently executing</summary>
    InProgress,
    
    /// <summary>Stage completed successfully</summary>
    Completed,
    
    /// <summary>Stage failed with error</summary>
    Failed,
    
    /// <summary>Stage was cancelled by operator</summary>
    Cancelled
}

/// <summary>
/// Represents a single boot initialization stage with execution details.
/// </summary>
public record BootStage
{
    /// <summary>The type of boot stage</summary>
    public required BootStageType Type { get; init; }
    
    /// <summary>Human-readable stage name (e.g., "Configuration", "Reachability")</summary>
    public required string Name { get; init; }
    
    /// <summary>Current execution status</summary>
    public required BootStageStatus Status { get; init; }
    
    /// <summary>Progress percentage (0-100) for stages with measurable progress</summary>
    public int ProgressPercentage { get; init; }
    
    /// <summary>Time when stage started execution (null if not started)</summary>
    public DateTime? StartTime { get; init; }
    
    /// <summary>Time when stage completed (null if not completed)</summary>
    public DateTime? EndTime { get; init; }
    
    /// <summary>Total duration of stage execution (null if not completed)</summary>
    public TimeSpan? Duration => EndTime - StartTime;
    
    /// <summary>Error message if stage failed (null if successful)</summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>Detailed error information for diagnostics (null if successful)</summary>
    public string? ErrorDetails { get; init; }
}
```

**Validation Rules**:
- `ProgressPercentage` must be 0-100
- `Status` = `Completed` requires `EndTime` != null
- `Status` = `Failed` requires `ErrorMessage` != null
- `Duration` is computed property (immutable)

**State Transitions**:
```
NotStarted → InProgress → {Completed | Failed | Cancelled}
```

---

### 2. ServiceConfiguration

Environment settings, feature flags, endpoint URLs, timeout values, and platform-specific parameters.

```csharp
namespace MTM_Template_Application.Models.Configuration;

/// <summary>
/// Defines the runtime environment for the application.
/// </summary>
public enum EnvironmentType
{
    Development,
    Staging,
    Production
}

/// <summary>
/// Represents application configuration loaded during boot Stage 1 (Configuration).
/// </summary>
public record ServiceConfiguration
{
    /// <summary>Current runtime environment</summary>
    public required EnvironmentType Environment { get; init; }
    
    /// <summary>Feature flags controlling optional functionality</summary>
    public required Dictionary<string, bool> FeatureFlags { get; init; }
    
    /// <summary>Visual API Toolkit base endpoint URL</summary>
    public required Uri VisualApiEndpoint { get; init; }
    
    /// <summary>MAMP MySQL server connection string (Desktop only)</summary>
    public string? MampConnectionString { get; init; }
    
    /// <summary>MAMP API proxy endpoint URL (Android only)</summary>
    public Uri? MampApiProxyEndpoint { get; init; }
    
    /// <summary>Timeout for reachability checks per endpoint (default: 10 seconds)</summary>
    public TimeSpan ReachabilityTimeout { get; init; } = TimeSpan.FromSeconds(10);
    
    /// <summary>Timeout for cache prefetch operations (default: 30 seconds, range: 10-120)</summary>
    public TimeSpan CachePrefetchTimeout { get; init; } = TimeSpan.FromSeconds(30);
    
    /// <summary>Retry button cooldown period (default: 30 seconds)</summary>
    public TimeSpan RetryButtonCooldown { get; init; } = TimeSpan.FromSeconds(30);
    
    /// <summary>Cache staleness threshold (default: 24 hours)</summary>
    public TimeSpan CacheStalenessThreshold { get; init; } = TimeSpan.FromHours(24);
    
    /// <summary>Maximum boot metrics to retain (default: 100)</summary>
    public int MaxBootMetricsHistory { get; init; } = 100;
    
    /// <summary>Default localization culture (default: en-US)</summary>
    public string DefaultCulture { get; init; } = "en-US";
    
    /// <summary>Platform-specific settings (e.g., Android camera permissions)</summary>
    public Dictionary<string, object>? PlatformSettings { get; init; }
}
```

**Validation Rules**:
- `VisualApiEndpoint` must use HTTPS scheme
- `CachePrefetchTimeout` must be 10-120 seconds
- `ReachabilityTimeout` must be 1-30 seconds
- `MaxBootMetricsHistory` must be 1-1000
- Either `MampConnectionString` (Desktop) OR `MampApiProxyEndpoint` (Android) must be set

---

### 3. ReachabilityStatus

Network connectivity state for all three data sources with timing and availability details.

```csharp
namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Defines the three-tier data source architecture.
/// </summary>
public enum DataSourceType
{
    /// <summary>Visual ERP via API Toolkit (primary, live data)</summary>
    Visual,
    
    /// <summary>MAMP MySQL 5.7 backup server (secondary, near-live data)</summary>
    Mamp,
    
    /// <summary>Local cache (tertiary, stale but always available)</summary>
    Cache
}

/// <summary>
/// Represents the reachability check result for a single data source.
/// </summary>
public record DataSourceReachability
{
    /// <summary>Data source being checked</summary>
    public required DataSourceType Source { get; init; }
    
    /// <summary>Whether the data source is reachable and responsive</summary>
    public required bool IsAvailable { get; init; }
    
    /// <summary>Response time for health check (null if unavailable)</summary>
    public TimeSpan? ResponseTime { get; init; }
    
    /// <summary>Timestamp of this reachability check</summary>
    public required DateTime CheckTimestamp { get; init; }
    
    /// <summary>Reason for unavailability (null if available)</summary>
    public string? UnavailabilityReason { get; init; }
    
    /// <summary>HTTP status code from health check (null if connection failed)</summary>
    public int? HttpStatusCode { get; init; }
}

/// <summary>
/// Represents the overall reachability status across all data sources.
/// </summary>
public record ReachabilityStatus
{
    /// <summary>Reachability check results for all three data sources</summary>
    public required Dictionary<DataSourceType, DataSourceReachability> DataSources { get; init; }
    
    /// <summary>Active data source selected by cascading fallback logic</summary>
    public required DataSourceType ActiveDataSource { get; init; }
    
    /// <summary>Timestamp when reachability checks started</summary>
    public required DateTime CheckStartTime { get; init; }
    
    /// <summary>Timestamp when reachability checks completed</summary>
    public required DateTime CheckEndTime { get; init; }
    
    /// <summary>Total duration of cascading reachability checks</summary>
    public TimeSpan TotalDuration => CheckEndTime - CheckStartTime;
    
    /// <summary>Cascade history showing which sources were attempted</summary>
    public required List<DataSourceType> CascadeHistory { get; init; }
}
```

**Validation Rules**:
- `DataSources` dictionary must contain all three `DataSourceType` values
- `ActiveDataSource` must be in `CascadeHistory`
- If `IsAvailable` = true, `ResponseTime` must not be null
- If `IsAvailable` = false, `UnavailabilityReason` must not be null
- `CascadeHistory` must be ordered by attempt sequence (Visual first, then MAMP, then Cache)

**Cascade Logic**:
1. Check Visual API Toolkit (10s timeout)
2. If Visual unavailable → Check MAMP MySQL (10s timeout)
3. If MAMP unavailable → Use Local Cache (always available)
4. Set `ActiveDataSource` to first available source in cascade

---

### 4. BootMetrics

Startup telemetry including stage-by-stage durations, failure points, retry counts, and data source selection.

```csharp
namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Represents comprehensive metrics for a single boot attempt.
/// </summary>
public record BootMetrics
{
    /// <summary>Timestamp when boot sequence started</summary>
    public required DateTime Timestamp { get; init; }
    
    /// <summary>Overall boot success or failure</summary>
    public required bool Success { get; init; }
    
    /// <summary>Total duration of boot sequence from start to completion/failure</summary>
    public required TimeSpan TotalDuration { get; init; }
    
    /// <summary>Duration of each boot stage (key = stage type, value = duration)</summary>
    public required Dictionary<BootStageType, TimeSpan> StageDurations { get; init; }
    
    /// <summary>Active data source selected during this boot</summary>
    public required DataSourceType ActiveDataSource { get; init; }
    
    /// <summary>Cascade history showing all data sources attempted</summary>
    public required List<DataSourceType> FallbackChain { get; init; }
    
    /// <summary>Stage where failure occurred (null if successful)</summary>
    public BootStageType? FailureStage { get; init; }
    
    /// <summary>Error message if boot failed (null if successful)</summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>Detailed error information for diagnostics (null if successful)</summary>
    public string? ErrorDetails { get; init; }
    
    /// <summary>Number of operator retry attempts during this boot (0 = no retries)</summary>
    public int RetryCount { get; init; }
    
    /// <summary>Whether operator cancelled the boot sequence</summary>
    public bool WasCancelled { get; init; }
    
    /// <summary>.NET runtime version</summary>
    public required string RuntimeVersion { get; init; }
    
    /// <summary>Application version</summary>
    public required string ApplicationVersion { get; init; }
    
    /// <summary>Platform (Desktop or Android)</summary>
    public required string Platform { get; init; }
}
```

**Validation Rules**:
- If `Success` = true, `FailureStage` and `ErrorMessage` must be null
- If `Success` = false, `ErrorMessage` must not be null
- `StageDurations` sum should approximately equal `TotalDuration` (within margin for overhead)
- `RetryCount` must be ≥ 0
- `FallbackChain` must be non-empty and contain valid `DataSourceType` values

**Persistence Format (JSON)**:
```json
{
  "timestamp": "2025-10-02T14:23:45Z",
  "success": true,
  "totalDuration": "00:00:08.234",
  "stageDurations": {
    "0": "00:00:00.050",
    "1": "00:00:00.123",
    "2": "00:00:00.234",
    "3": "00:00:00.089",
    "4": "00:00:00.456",
    "5": "00:00:05.123",
    "6": "00:00:01.234",
    "7": "00:00:00.456",
    "8": "00:00:00.345",
    "9": "00:00:00.123",
    "10": "00:00:00.051"
  },
  "activeDataSource": "Visual",
  "fallbackChain": ["Visual"],
  "failureStage": null,
  "errorMessage": null,
  "errorDetails": null,
  "retryCount": 0,
  "wasCancelled": false,
  "runtimeVersion": "9.0.0",
  "applicationVersion": "1.0.0",
  "platform": "Desktop"
}
```

**FIFO Rotation**:
- Maximum 100 boot metrics retained in `boot-metrics.json`
- When 101st boot occurs, delete oldest entry (index 0)
- File stored in application data directory

---

### 5. CacheManifest

Inventory of prefetched master data with metadata about source, freshness, and completeness.

```csharp
namespace MTM_Template_Application.Models.Boot;

/// <summary>
/// Defines the master data entity types that must be cached.
/// </summary>
public enum MasterDataType
{
    /// <summary>Manufacturing items/parts</summary>
    Items,
    
    /// <summary>Warehouse locations</summary>
    Locations,
    
    /// <summary>Manufacturing work centers</summary>
    WorkCenters
}

/// <summary>
/// Represents cached data for a single master data entity type.
/// </summary>
public record CachedEntityManifest
{
    /// <summary>Master data entity type</summary>
    public required MasterDataType EntityType { get; init; }
    
    /// <summary>Number of cached records</summary>
    public required int RecordCount { get; init; }
    
    /// <summary>Timestamp when this entity was last synchronized</summary>
    public required DateTime LastSyncTimestamp { get; init; }
    
    /// <summary>Data source that provided this cached data</summary>
    public required DataSourceType SourceOrigin { get; init; }
    
    /// <summary>Whether the last sync was successful</summary>
    public required bool SyncSuccess { get; init; }
    
    /// <summary>Error message if sync failed (null if successful)</summary>
    public string? SyncError { get; init; }
    
    /// <summary>Whether cached data is considered stale (older than 24 hours)</summary>
    public bool IsStale => DateTime.UtcNow - LastSyncTimestamp > TimeSpan.FromHours(24);
    
    /// <summary>Age of cached data</summary>
    public TimeSpan Age => DateTime.UtcNow - LastSyncTimestamp;
}

/// <summary>
/// Represents the overall cache manifest for all master data.
/// </summary>
public record CacheManifest
{
    /// <summary>Manifests for each master data entity type</summary>
    public required Dictionary<MasterDataType, CachedEntityManifest> Entities { get; init; }
    
    /// <summary>Timestamp when cache manifest was last updated</summary>
    public required DateTime ManifestLastUpdated { get; init; }
    
    /// <summary>Total number of cached records across all entities</summary>
    public int TotalRecordCount => Entities.Values.Sum(e => e.RecordCount);
    
    /// <summary>Whether cache is complete (all three entity types have at least 1 record)</summary>
    public bool IsComplete => 
        Entities.ContainsKey(MasterDataType.Items) && Entities[MasterDataType.Items].RecordCount > 0 &&
        Entities.ContainsKey(MasterDataType.Locations) && Entities[MasterDataType.Locations].RecordCount > 0 &&
        Entities.ContainsKey(MasterDataType.WorkCenters) && Entities[MasterDataType.WorkCenters].RecordCount > 0;
    
    /// <summary>Whether any cached entity is stale (older than 24 hours)</summary>
    public bool HasStaleData => Entities.Values.Any(e => e.IsStale);
    
    /// <summary>Oldest cache entry age across all entities</summary>
    public TimeSpan? OldestCacheAge => 
        Entities.Values.Any() 
            ? Entities.Values.Max(e => e.Age) 
            : null;
}
```

**Validation Rules**:
- `Entities` dictionary must contain all three `MasterDataType` values for complete cache
- If `SyncSuccess` = false, `SyncError` must not be null
- `RecordCount` must be ≥ 0
- `IsComplete` = true requires all three entity types with `RecordCount` ≥ 1
- If cache is incomplete (`IsComplete` = false), boot must block with Exit-only option

**Cache Completeness Check**:
```
IF Items.RecordCount = 0 OR Locations.RecordCount = 0 OR WorkCenters.RecordCount = 0
THEN Block launch with error: "Cache is incomplete. Missing master data: [list missing types]"
```

**Staleness Warning**:
```
IF HasStaleData = true
THEN Display warning banner: "Cached data is [OldestCacheAge] old. Last updated: [LastSyncTimestamp]"
```

---

## Visual-Specific Data Transfer Objects (DTOs)

These DTOs represent master data retrieved from InforVisual Manufacturing ERP via the Visual API Toolkit. The Desktop Visual Service converts Visual ADO.NET DataTable objects into these DTOs for JSON serialization.

### 6. ItemDto

Manufacturing items/parts master data from Visual PART table.

```csharp
namespace MTM_Template_Application.Models.Visual;

/// <summary>
/// Represents a manufacturing item/part from Visual ERP.
/// Mapped from Visual PART table via VmfgInventory.Part business object.
/// </summary>
public record ItemDto
{
    /// <summary>Part ID - primary key (Visual: PART_ID, max 30 chars)</summary>
    public required string PartId { get; init; }
    
    /// <summary>Part description (Visual: DESCRIPTION, max 80 chars)</summary>
    public required string Description { get; init; }
    
    /// <summary>Unit of measure (Visual: U_OF_M, e.g., EA, LB, FT)</summary>
    public required string Uom { get; init; }
    
    /// <summary>Part type: M=Manufactured, P=Purchased, S=Subcontract (Visual: PART_TYPE)</summary>
    public required string PartType { get; init; }
    
    /// <summary>Product line code (Visual: PRODUCT_LINE)</summary>
    public string? ProductLine { get; init; }
    
    /// <summary>Standard cost (Visual: STD_COST)</summary>
    public decimal? StdCost { get; init; }
    
    /// <summary>Average cost (Visual: AVG_COST)</summary>
    public decimal? AvgCost { get; init; }
    
    /// <summary>Last purchase cost (Visual: LAST_COST)</summary>
    public decimal? LastCost { get; init; }
    
    /// <summary>Current on-hand quantity (Visual: ON_HAND_QTY)</summary>
    public decimal OnHand { get; init; }
    
    /// <summary>Available quantity = on-hand - allocated (Visual: AVAILABLE_QTY)</summary>
    public decimal Available { get; init; }
    
    /// <summary>Quantity allocated to orders (Visual: ALLOC_QTY)</summary>
    public decimal Allocated { get; init; }
    
    /// <summary>Quantity on purchase orders (Visual: ON_ORDER_QTY)</summary>
    public decimal OnOrder { get; init; }
    
    /// <summary>Lead time in days (Visual: LEAD_TIME_DAYS)</summary>
    public int LeadTimeDays { get; init; }
    
    /// <summary>Safety stock level (Visual: SAFETY_STOCK)</summary>
    public decimal? SafetyStock { get; init; }
    
    /// <summary>Reorder point (Visual: REORDER_POINT)</summary>
    public decimal? ReorderPoint { get; init; }
    
    /// <summary>Reorder quantity (Visual: REORDER_QTY)</summary>
    public decimal? ReorderQty { get; init; }
    
    /// <summary>ABC inventory classification (Visual: ABC_CODE, values: A/B/C)</summary>
    public string? AbcCode { get; init; }
    
    /// <summary>Make or buy flag: M=Make, B=Buy (Visual: MAKE_OR_BUY)</summary>
    public string? MakeOrBuy { get; init; }
    
    /// <summary>Phantom BOM component flag (Visual: PHANTOM)</summary>
    public bool Phantom { get; init; }
    
    /// <summary>Inactive flag (Visual: INACTIVE)</summary>
    public bool Inactive { get; init; }
    
    /// <summary>Image filename (Visual: PICTURE_FILE)</summary>
    public string? PictureFile { get; init; }
    
    /// <summary>Commodity classification code (Visual: COMMODITY_CODE)</summary>
    public string? CommodityCode { get; init; }
    
    /// <summary>Engineering drawing number (Visual: DRAWING_NUMBER)</summary>
    public string? DrawingNumber { get; init; }
    
    /// <summary>Revision level (Visual: REVISION)</summary>
    public string? Revision { get; init; }
}
```

### 7. LocationDto

Warehouse location master data from Visual (table name TBD - schema incomplete).

```csharp
namespace MTM_Template_Application.Models.Visual;

/// <summary>
/// Represents a warehouse location from Visual ERP.
/// Mapped from Visual location table (exact table name TBD) via VmfgInventory business object.
/// </summary>
/// <remarks>
/// Visual Data Table.csv schema read incomplete (2,000 of 14,776 rows read).
/// Location table not yet found. Likely table names: LOCATION, PART_LOCATION, WAREHOUSE_LOCATION.
/// Field mappings are estimated based on typical ERP schema and may need adjustment after full schema review.
/// </remarks>
public record LocationDto
{
    /// <summary>Location ID - primary key (estimated: LOCATION_ID)</summary>
    public required string LocationId { get; init; }
    
    /// <summary>Location description (estimated: DESCRIPTION)</summary>
    public required string Description { get; init; }
    
    /// <summary>Site ID (estimated: SITE_ID)</summary>
    public required string SiteId { get; init; }
    
    /// <summary>Location type: STORAGE, WIP, SCRAP, etc. (estimated: LOCATION_TYPE)</summary>
    public string? LocationType { get; init; }
    
    /// <summary>Warehouse ID (estimated: WAREHOUSE_ID)</summary>
    public string? WarehouseId { get; init; }
    
    /// <summary>Aisle (estimated: AISLE)</summary>
    public string? Aisle { get; init; }
    
    /// <summary>Bin (estimated: BIN)</summary>
    public string? Bin { get; init; }
    
    /// <summary>Pick sequence for warehouse operations (estimated: PICK_SEQUENCE)</summary>
    public int? PickSequence { get; init; }
    
    /// <summary>Storage capacity (estimated: CAPACITY)</summary>
    public decimal? Capacity { get; init; }
    
    /// <summary>Capacity unit of measure (estimated: CAPACITY_UOM, e.g., CF, CBM)</summary>
    public string? CapacityUom { get; init; }
    
    /// <summary>Allow negative inventory flag (estimated: ALLOW_NEGATIVE)</summary>
    public bool AllowNegative { get; init; }
    
    /// <summary>Inactive flag (estimated: INACTIVE)</summary>
    public bool Inactive { get; init; }
}
```

### 8. WorkCenterDto

Manufacturing work center master data from Visual (table name TBD - schema incomplete).

```csharp
namespace MTM_Template_Application.Models.Visual;

/// <summary>
/// Represents a manufacturing work center from Visual ERP.
/// Mapped from Visual work center table (exact table name TBD) via VmfgShopFloor business object.
/// </summary>
/// <remarks>
/// Visual Data Table.csv schema read incomplete (2,000 of 14,776 rows read).
/// Work center table not yet found. Likely table names: WORK_CENTER, RESOURCE, WORKCENTER.
/// Field mappings are estimated based on typical ERP schema and may need adjustment after full schema review.
/// </remarks>
public record WorkCenterDto
{
    /// <summary>Work center ID - primary key (estimated: WORK_CENTER_ID)</summary>
    public required string WorkCenterId { get; init; }
    
    /// <summary>Work center description (estimated: DESCRIPTION)</summary>
    public required string Description { get; init; }
    
    /// <summary>Site ID (estimated: SITE_ID)</summary>
    public required string SiteId { get; init; }
    
    /// <summary>Department code (estimated: DEPARTMENT)</summary>
    public string? Department { get; init; }
    
    /// <summary>Labor rate per hour (estimated: LABOR_RATE)</summary>
    public decimal? LaborRate { get; init; }
    
    /// <summary>Overhead rate per hour (estimated: OVERHEAD_RATE)</summary>
    public decimal? OverheadRate { get; init; }
    
    /// <summary>Machine rate per hour (estimated: MACHINE_RATE)</summary>
    public decimal? MachineRate { get; init; }
    
    /// <summary>Capacity (estimated: CAPACITY)</summary>
    public decimal? Capacity { get; init; }
    
    /// <summary>Capacity unit of measure (estimated: CAPACITY_UOM, e.g., HR, MIN)</summary>
    public string? CapacityUom { get; init; }
    
    /// <summary>Efficiency percentage (estimated: EFFICIENCY, e.g., 95.0 = 95%)</summary>
    public decimal? Efficiency { get; init; }
    
    /// <summary>Utilization percentage (estimated: UTILIZATION, e.g., 85.0 = 85%)</summary>
    public decimal? Utilization { get; init; }
    
    /// <summary>Queue time in hours (estimated: QUEUE_TIME)</summary>
    public decimal? QueueTime { get; init; }
    
    /// <summary>Setup time in hours (estimated: SETUP_TIME)</summary>
    public decimal? SetupTime { get; init; }
    
    /// <summary>Inactive flag (estimated: INACTIVE)</summary>
    public bool Inactive { get; init; }
}
```

### Visual API Response Wrappers

```csharp
namespace MTM_Template_Application.Models.Visual;

/// <summary>
/// Response wrapper for Items endpoint (GET /api/visual/master-data/items).
/// </summary>
public record ItemsResponse
{
    public required List<ItemDto> Items { get; init; }
    public required int TotalCount { get; init; }
    public required bool HasMore { get; init; }
    public required int Offset { get; init; }
    public required int MaxResults { get; init; }
}

/// <summary>
/// Response wrapper for Locations endpoint (GET /api/visual/master-data/locations).
/// </summary>
public record LocationsResponse
{
    public required List<LocationDto> Locations { get; init; }
    public required int TotalCount { get; init; }
    public required bool HasMore { get; init; }
    public required int Offset { get; init; }
    public required int MaxResults { get; init; }
}

/// <summary>
/// Response wrapper for WorkCenters endpoint (GET /api/visual/master-data/workcenters).
/// </summary>
public record WorkCentersResponse
{
    public required List<WorkCenterDto> WorkCenters { get; init; }
    public required int TotalCount { get; init; }
    public required bool HasMore { get; init; }
    public required int Offset { get; init; }
    public required int MaxResults { get; init; }
}

/// <summary>
/// Authentication request for Visual API.
/// </summary>
public record AuthRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
}

/// <summary>
/// Authentication response from Visual API (POST /api/visual/auth/token).
/// </summary>
public record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserInfo User { get; init; }
}

/// <summary>
/// User information from Visual authentication.
/// </summary>
public record UserInfo
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string Site { get; init; }
    public string? Department { get; init; }
}
```

**Note**: LocationDto and WorkCenterDto field mappings are estimated based on typical manufacturing ERP schema. The Visual Data Table.csv file is only 13.5% read (2,000 of 14,776 rows). Remaining schema read required to confirm exact table names (LOCATION, WORK_CENTER, or variants) and column names. ItemDto mappings are based on documented Visual PART table structure from CSV first 2,000 rows.

---

## Entity Relationships

```
BootMetrics
  ├── References: ActiveDataSource (DataSourceType)
  ├── References: FailureStage (BootStageType)
  └── Contains: StageDurations (Dictionary<BootStageType, TimeSpan>)

ReachabilityStatus
  ├── References: ActiveDataSource (DataSourceType)
  └── Contains: DataSources (Dictionary<DataSourceType, DataSourceReachability>)

CacheManifest
  └── Contains: Entities (Dictionary<MasterDataType, CachedEntityManifest>)

ServiceConfiguration
  ├── References: VisualApiEndpoint (Uri)
  └── References: MampApiProxyEndpoint (Uri, optional)

Visual DTOs (from Desktop Visual Service)
  ItemDto → Cached as MasterDataType.Items
  LocationDto → Cached as MasterDataType.Locations
  WorkCenterDto → Cached as MasterDataType.WorkCenters
```

---

## Data Flow Summary

1. **Boot Stage 1 (Configuration)**: Load `ServiceConfiguration` from environment-specific config file
2. **Boot Stage 5 (Reachability)**: Perform health checks → Populate `ReachabilityStatus`
3. **Boot Stage 6 (Data Layers)**: Establish connections to active data source from `ReachabilityStatus`
4. **Boot Stage 7 (Authentication)**: POST /api/visual/auth/token → Receive `AuthResponse` with JWT token
5. **Boot Stage 8 (Caching)**: Prefetch master data via GET /api/visual/master-data/{items,locations,workcenters} → Convert JSON responses to DTOs → Update `CacheManifest`
6. **Boot Complete**: Persist `BootMetrics` to JSON file with FIFO rotation

---

## Nullability Annotations

All models use C# nullable reference types (.NET 9.0):
- `required` properties: Must be initialized during construction
- `?` nullable properties: Optional or conditional (e.g., error messages only when failed)
- Non-nullable properties: Always have values (validated at construction)

---


**Next Steps**: Generate API contracts in `contracts/` directory based on data source interactions.
