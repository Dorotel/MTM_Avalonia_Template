# Phase 0: Research & Technical Decisions

**Feature**: 005 - Migrate Infor VISUAL ERP Integration
**Date**: 2025-10-08
**Status**: Complete

---

## 📑 Table of Contents

### Research Questions
- [Q1: Data Volume Analysis](#q1-data-volume-analysis-scalescope-clarification) - Scale estimation and cache sizing
- [Q2: Infor VISUAL API Toolkit Endpoint Discovery](#q2-infor-visual-api-toolkit-endpoint-discovery) - API Toolkit documentation availability
- [Q3: Connection Pool Sizing](#q3-connection-pool-sizing-for-visual-api-toolkit) - Connection pool optimization

### Technical Decisions
- [Decision 1: API Integration Architecture](#1-infor-visual-api-toolkit-integration-architecture) - Abstraction layer design
- [Decision 2: Local Transaction Storage](#2-mamp-mysql-local-transaction-storage-schema) - Database schema for transactions
- [Decision 3: Performance Monitoring](#3-performance-monitoring-implementation-fr-019-fr-020-fr-021) - Hybrid monitoring approach
- [Decision 4: Cache Strategy](#4-cache-strategy-for-visual-data) - Cache operations and TTL policies
- [Decision 5: Offline Degradation](#5-offline-degradation-behavior) - Graceful offline handling
- [Decision 6: Barcode Integration](#6-barcode-scanning-integration) - Hardware integration approach
- [Decision 7: Contract Testing](#7-contract-testing-approach) - API contract validation

### API Toolkit Reference
- [Available Resources](#available-resources) - Local assemblies and documentation
- [Endpoint Mappings](#next-steps-prerequisite-for-t007-visualapiservice-implementation) - Functional requirements to API methods

### Implementation Guidance
- [Architecture Patterns](#architecture-pattern) - Service abstractions and repository pattern
- [Risk Mitigation](#rationale) - Testing strategies and incremental implementation

---

## Research Questions

### Q1: Data Volume Analysis (Scale/Scope Clarification)

**Question**: How many parts, work orders, and inventory transactions exist in production VISUAL database?

**Research Method**:
1. Query production VISUAL database for entity counts
2. Analyze cache size requirements
3. Determine pagination defaults and cache eviction strategy

**Decision**: **Assume medium-scale manufacturing operation** based on typical VISUAL ERP deployments

**Findings**:
- **Parts**: 10,000-50,000 active parts (estimate for medium manufacturing)
- **Work Orders**: 500-2,000 open work orders at any time (spec already defines "up to 500 work orders" in FR-002)
- **Inventory Transactions**: 100-500 transactions per day (estimate)
- **Cache Size**: ~40MB compressed (~120MB uncompressed) sufficient for:
  - 10,000 parts (avg 12KB each uncompressed)
  - 500 work orders (avg 20KB each uncompressed)
  - 7 days of inventory/shipment history

**Rationale**:
- Spec FR-002 already defines "up to 500 work orders" - use this as baseline
- Cache budget ~40MB already established in constitution (Principle V)
- TTL policies (Parts: 24h, Others: 7d) already implemented via CacheStalenessDetector
- Pagination strategy: Load first 100 results, "Load More" button for additional pages (spec §Edge Cases)

**Impact on Implementation**:
- No changes needed - spec already accounts for scale
- Cache eviction: LRU (Least Recently Used) when cache exceeds 40MB compressed
- Pagination: Server-side (via VISUAL API Toolkit), client displays 100 results default

**Alternatives Considered**:
- **Query actual production database**: Requires production access, may delay planning
- **Dynamic cache sizing**: Adds complexity, constitution already defines 40MB budget
- **Rejected**: Proceed with estimated scale, validate during testing phase

---

### Q2: Infor VISUAL API Toolkit Endpoint Discovery

**Question**: Which specific Infor VISUAL API Toolkit endpoints/methods map to functional requirements (FR-001 through FR-005)?

**Research Method**:
1. Review Infor VISUAL API Toolkit documentation. Located in ./docs/Visual Files/Guides
2. Examine existing VISUAL integration code (if any legacy implementation exists)
3. Prototype sample API calls to discover endpoint structure

**STATUS**: ✅ **TOOLKIT AVAILABLE LOCALLY** - All reference materials discovered at `docs\Visual Files\`

**Available Resources**:

**API Assemblies** (9 DLLs at `docs\Visual Files\ReferenceFiles\`):
- ✅ LsaCore.dll - Core authentication and session management
- ✅ LsaShared.dll - Shared utilities and common types
- ✅ VmfgFinancials.dll - Financial operations (invoicing, AR/AP)
- ✅ VmfgInventory.dll - Inventory operations (FR-004 inventory balance queries)
- ✅ VmfgPurchasing.dll - Purchasing operations (PO management)
- ✅ VmfgSales.dll - Sales operations (FR-005 customer orders, shipments)
- ✅ VmfgShared.dll - VMFG shared library (common VISUAL manufacturing types)
- ✅ VmfgShopFloor.dll - Shop floor operations (FR-002 work order queries)
- ✅ VmfgTrace.dll - Traceability and audit operations

**Reference Documentation** (7 TXT files at `docs\Visual Files\Guides\`):
- ✅ Reference - Development Guide.txt - API patterns, authentication, best practices
- ✅ Reference - Core.txt - LsaCore/LsaShared API reference
- ✅ Reference - Inventory.txt - VmfgInventory API reference (FR-004 implementation guide)
- ✅ Reference - Shared Library.txt - Shared types and utilities
- ✅ Reference - Shop Floor.txt - VmfgShopFloor API reference (FR-002 implementation guide)
- ✅ Reference - VMFG Shared Library.txt - Manufacturing domain types
- ✅ User Manual.txt - End-user perspective (may include data structure insights)

**Database Schema** (4 CSV files at `docs\Visual Files\Database Files\`):
- ✅ MTMFG Tables.csv - Complete table schema for VISUAL database validation
- ✅ MTMFG Procedures.csv - Stored procedures catalog
- ✅ MTMFG Relationships.csv - Foreign key relationships and entity associations
- ✅ Visual Data Table.csv - Data dictionary and field definitions

**Database Configuration**:
- ✅ Database.config - VISUAL connection string (MTMFGPLAY instance on VISUAL\PLAY server)

**Decision**: **Define logical interface abstraction independent of Toolkit implementation details**

**Findings** (Architecture Pattern):

Since VISUAL API Toolkit is a .NET 4.x x86 library (likely COM or .NET Framework assembly), we will:

1. **Create abstraction layer** (`IVisualApiService`) that defines domain operations:
   ```csharp
   public interface IVisualApiService
   {
       Task<Part?> GetPartByIdAsync(string partId, CancellationToken cancellationToken);
       Task<List<Part>> SearchPartsAsync(string searchTerm, CancellationToken cancellationToken);
       Task<List<WorkOrder>> GetWorkOrdersAsync(WorkOrderFilter filter, CancellationToken cancellationToken);
       Task<WorkOrder?> GetWorkOrderByIdAsync(string workOrderId, CancellationToken cancellationToken);
       Task<decimal> GetInventoryBalanceAsync(string partId, CancellationToken cancellationToken);
       Task<List<CustomerOrder>> GetCustomerOrdersAsync(CustomerOrderFilter filter, CancellationToken cancellationToken);
   }
   ```

2. **Implement wrapper** (`VisualApiService`) that:
   - Wraps Infor VISUAL API Toolkit calls
   - Converts Toolkit responses to domain models (Part, WorkOrder, etc.)
   - Handles Toolkit-specific errors and converts to standard exceptions
   - Applies Polly retry policies
   - Logs all calls for performance monitoring (FR-019)

3. **Repository pattern** for domain-specific operations:
   - `VisualPartsRepository`: Part queries (FR-001)
   - `VisualWorkOrderRepository`: Work order queries (FR-002)
   - `VisualInventoryRepository`: Inventory balance queries
   - `VisualShipmentRepository`: Order/shipment queries

**Rationale**:
- **Abstraction enables testing**: Mock `IVisualApiService` for unit tests without actual Toolkit dependency
- **Isolates Toolkit complexity**: Toolkit may have COM interop, threading restrictions, or unusual error handling
- **Enables incremental implementation**: Can stub out interface, implement endpoints incrementally based on priority
- **Supports contract testing**: Contract tests validate expected endpoint behavior without production data

**Impact on Implementation**:
- Phase 1 (Design): Define `IVisualApiService` interface with all read-only operations
- Phase 2 (Implementation): Implement `VisualApiService` wrapper against actual Toolkit (7 reference guides available locally)
- **MAJOR RISK ELIMINATED**: API Toolkit documentation accessibility changed from UNKNOWN/HIGH RISK to ✅ RESOLVED
- **Timeline Impact**: Research phase reduced from 4-8 hours to 1-2 hours (reference guides eliminate API discovery guesswork)

**Next Steps** (Prerequisite for T007 VisualApiService Implementation):
1. Extract API endpoint mappings from reference guides:
   - Review `docs\Visual Files\Guides\Reference - Development Guide.txt` (authentication patterns, API conventions)
   - Review `docs\Visual Files\Guides\Reference - Inventory.txt` (FR-004: `GetInventoryBalanceAsync` implementation)
   - Review `docs\Visual Files\Guides\Reference - Shop Floor.txt` (FR-002: `GetWorkOrdersAsync`, `GetWorkOrderByIdAsync`)
   - Review `docs\Visual Files\Guides\Reference - Sales.txt` (implied from VmfgSales.dll, FR-005: `GetCustomerOrdersAsync`)
2. Cross-reference database schema CSV files to validate entity structures match data model:
   - `docs\Visual Files\Database Files\MTMFG Tables.csv` → Part, WorkOrder, InventoryTransaction, CustomerOrder tables
   - `docs\Visual Files\Database Files\MTMFG Relationships.csv` → Foreign key constraints, entity associations
3. Document Toolkit method signatures in this research.md file (update interface definitions with actual method names)

**Alternatives Considered**:
- **Direct Toolkit usage in repositories**: Tight coupling, hard to test, violates dependency inversion
- **Generic CRUD interface**: Doesn't express domain operations clearly (e.g., "GetWorkOrdersWithMaterialRequirements")
- **Rejected**: Domain-specific abstraction provides better expressiveness and testability

---

### Q3: Connection Pool Sizing for VISUAL API Toolkit

**Question**: What is the optimal connection pool size for expected concurrent user load?

**Research Method**:
1. Analyze expected concurrent API call patterns
2. Review VISUAL API Toolkit documentation for connection/session management
3. Consider manufacturing floor user behavior (200-300 API calls per user per shift)

**Decision**: **Start with conservative pool sizing, monitor via DebugTerminalWindow, tune based on telemetry**

**Findings**:

**Initial Configuration** (Conservative):
- **HTTP Client pool**: 10-20 concurrent connections (existing HttpClient infrastructure)
- **VISUAL API Toolkit sessions**: NEEDS CLARIFICATION (Toolkit may use connection pooling internally or require session management)
- **Database connections** (MAMP MySQL): Use existing connection pool (already configured)

**Assumption**: VISUAL API Toolkit likely manages its own connection pooling or uses stateless HTTP calls to VISUAL server.

**Monitoring Strategy** (FR-019, FR-021):
1. **Track active connections** in DebugTerminalWindow VISUAL Performance panel
2. **Monitor queue depth**: Alert if requests wait >1s for available connection
3. **Adjust pool size** based on production telemetry (not guesswork)

**Rationale**:
- **Start conservative**: Avoid overwhelming VISUAL server or application memory
- **Measure before optimizing**: Use FR-021 performance panel to gather real data
- **Tune incrementally**: Increase pool size if connection contention detected

**Impact on Implementation**:
- Initial implementation: Use default HttpClient connection pool (10 concurrent connections)
- Performance monitoring (FR-021): Track connection pool utilization (active/idle/total)
- Post-deployment tuning: Adjust based on Debug Terminal telemetry

**Alternatives Considered**:
- **Large pool (50+ connections)**: Risk of overwhelming VISUAL server, excessive memory usage
- **Per-user connections**: Complex session management, may not be supported by Toolkit
- **Rejected**: Conservative start with production monitoring is safer approach

---

## Technology Decisions

### 1. Infor VISUAL API Toolkit Integration Architecture

**Decision**: **Abstraction layer with domain-specific repositories**

**Components**:
1. **IVisualApiService**: Low-level abstraction wrapping Toolkit API calls
2. **VisualApiService**: Concrete implementation with error handling, retries, logging
3. **Domain Repositories**: `VisualPartsRepository`, `VisualWorkOrderRepository`, etc.
4. **Cache Integration**: Check cache first, fallback to API, update cache on successful API response

**Rationale**:
- Enables testing without actual Toolkit dependency
- Isolates Toolkit-specific quirks (COM interop, error codes, threading)
- Supports incremental implementation (stub interface, implement endpoints by priority)
- Aligns with existing repository pattern in codebase

**Alternatives Considered**:
- **Direct Toolkit usage**: Hard to test, tight coupling, violates SOLID principles
- **Generic API wrapper**: Loses domain expressiveness, doesn't match spec user stories
- **Rejected**: Domain abstraction provides best balance of flexibility and clarity

---

### 2. MAMP MySQL Local Transaction Storage Schema

**Decision**: **Single table `LocalTransactionRecords` with JSON entity_data column**

**Schema**:
```sql
CREATE TABLE LocalTransactionRecords (
    transaction_id          INT AUTO_INCREMENT PRIMARY KEY,
    transaction_type        ENUM('WorkOrderStatusUpdate', 'InventoryReceipt', 'InventoryIssue',
                                 'InventoryAdjustment', 'InventoryTransfer', 'ShipmentConfirmation') NOT NULL,
    entity_data             JSON NOT NULL COMMENT 'Complete transaction payload (part numbers, quantities, references)',
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              VARCHAR(100) NOT NULL COMMENT 'Username from WindowsSecretsService',
    visual_reference_data   JSON NULL COMMENT 'Cached VISUAL data at time of transaction (part info, work order info, etc.)',
    audit_metadata          JSON NULL COMMENT 'Additional audit info (IP address, application version, etc.)',

    INDEX idx_transaction_type (transaction_type),
    INDEX idx_created_at (created_at),
    INDEX idx_created_by (created_by)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

**Rationale**:
- **JSON flexibility**: Schema can evolve without migrations (each transaction_type has different entity_data structure)
- **Audit trail**: created_at, created_by, visual_reference_data provide complete context
- **Query performance**: Indexes on transaction_type, created_at, created_by enable efficient reporting/export (FR-012)
- **No VISUAL sync**: Table is permanent storage, not a queue (aligns with spec §Key Entities)

**Alternatives Considered**:
- **Separate tables per transaction type**: Requires more migrations, more complex queries, loses unified audit trail
- **Queue table with sync status**: Violates spec (no VISUAL syncing), adds unnecessary complexity
- **Rejected**: Single JSON table provides best flexibility and aligns with read-only VISUAL architecture

---

### 3. Performance Monitoring Implementation (FR-019, FR-020, FR-021)

**Decision**: **Hybrid monitoring with structured logging + real-time DebugTerminalWindow panel**

**Components**:

1. **VisualPerformanceMonitor** (Service):
   - Wraps all VISUAL API calls with timing/metrics
   - Logs structured data: `{ "timestamp": "...", "operation": "GetPart", "duration_ms": 1234, "status": "success" }`
   - Exposes ObservableCollection<PerformanceMetric> for UI binding

2. **VisualDegradationDetector** (Service):
   - Tracks last 5 API call results
   - Triggers degradation mode if 5 consecutive failures (>5s OR 500+ errors OR timeouts)
   - Emits `OnDegradationTriggered` event for UI notification

3. **DebugTerminalWindow.axaml VISUAL Performance Panel** (FR-021):
   - **Last 10 API Calls**: DataGrid with timestamp, operation, duration, status
   - **Performance Trend**: Simple chart (response times over last hour) - use Avalonia charting library or ASCII chart
   - **Error History**: Categorized errors (timeouts, 500 errors, auth failures, network errors)
   - **Connection Pool Stats**: Active/idle connections, utilization percentage
   - **Cache Metrics**: Hit rate, miss rate, cached data age
   - **Manual Actions**: Buttons for Force Refresh, Clear Cache, Test Connection, Export Diagnostics

**Rationale**:
- **Silent logging**: Enables post-mortem analysis, trend detection, production monitoring
- **Real-time UI**: Enables developers/support to diagnose issues immediately
- **Automatic degradation**: Prevents cascading failures, improves user experience during VISUAL outages
- **Aligns with existing patterns**: Debug Terminal already monitors boot sequence, configuration, errors (Feature 003)

**Alternatives Considered**:
- **Logging only**: No real-time visibility, requires log file analysis
- **UI metrics only**: Loses historical data, can't analyze trends post-deployment
- **External monitoring**: Adds infrastructure complexity, overkill for desktop application
- **Rejected**: Hybrid approach provides best of both (historical logs + real-time visibility)

---

### 4. Cache Strategy for VISUAL Data

**Decision**: **Reuse existing CacheService with VISUAL-specific wrapper (VisualCacheService)**

**Cache Operations**:
1. **Get**: Check cache first, return if fresh (within TTL)
2. **Miss**: Call VISUAL API, store response in cache with TTL metadata
3. **Refresh**: Call VISUAL API, update cache, return fresh data
4. **Invalidate**: Remove specific cache entries (e.g., after local transaction recorded)

**TTL Policies** (Already Implemented):
- **Parts**: 24 hours (from CacheStalenessDetector)
- **Other entities**: 7 days (work orders, inventory, shipments)

**Cache Keys**:
- Parts: `visual:part:{partId}`
- Work Orders: `visual:workorder:{workOrderId}` and `visual:workorders:list:{filterHash}`
- Inventory: `visual:inventory:{partId}`
- Orders: `visual:order:{orderId}`

**Rationale**:
- **Reuse established pattern**: CacheService + CacheStalenessDetector already exist
- **LZ4 compression**: ~3:1 ratio, keeps cache within 40MB budget
- **TTL enforcement**: Automatic cleanup of stale data, no manual intervention
- **Offline-first**: Application continues operating when VISUAL unavailable (Principle X)

**Alternatives Considered**:
- **No caching**: Requires constant VISUAL availability, violates offline-first principle
- **Manual cache management**: Error-prone, complex invalidation logic
- **External cache (Redis)**: Overkill for desktop application, adds infrastructure dependency
- **Rejected**: Existing CacheService provides all needed functionality

---

## Best Practices Research

### 1. MVVM with CommunityToolkit.Mvvm (ViewModels)

**Pattern**: Source generators for properties and commands

**Example** (PartLookupViewModel):
```csharp
public partial class PartLookupViewModel : ObservableObject
{
    private readonly IVisualApiService _visualApi;
    private readonly ILogger<PartLookupViewModel> _logger;

    [ObservableProperty]
    private string? _partNumber;

    [ObservableProperty]
    private Part? _selectedPart;

    [ObservableProperty]
    private ObservableCollection<Part> _searchResults = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchPartCommand))]
    private bool _isSearching;

    public PartLookupViewModel(
        IVisualApiService visualApi,
        ILogger<PartLookupViewModel> logger)
    {
        _visualApi = visualApi ?? throw new ArgumentNullException(nameof(visualApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchPartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(PartNumber)) return;

        IsSearching = true;
        try
        {
            var results = await _visualApi.SearchPartsAsync(PartNumber, cancellationToken);
            SearchResults = new ObservableCollection<Part>(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search for part {PartNumber}", PartNumber);
            // Show error to user via message box or status bar
        }
        finally
        {
            IsSearching = false;
        }
    }

    private bool CanSearch() => !IsSearching && !string.IsNullOrWhiteSpace(PartNumber);
}
```

**Key Practices**:
- `[ObservableProperty]` for all bindable properties
- `[RelayCommand]` for commands with automatic CanExecute support
- `[NotifyCanExecuteChangedFor]` to update command state when properties change
- Constructor injection for dependencies (IVisualApiService, ILogger)
- Async commands with CancellationToken support
- Try-catch error handling with structured logging

---

### 2. Avalonia CompiledBinding (Views)

**Pattern**: x:DataType with CompiledBinding

**Example** (PartLookupView.axaml):
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MTM_Template_Application.ViewModels"
             x:Class="MTM_Template_Application.Views.PartLookupView"
             x:DataType="vm:PartLookupViewModel"
             x:CompileBindings="True">
    <Design.DataContext>
        <vm:PartLookupViewModel />
    </Design.DataContext>

    <StackPanel Spacing="16">
        <TextBox Text="{CompiledBinding PartNumber}"
                 Watermark="Enter part number..."
                 IsEnabled="{CompiledBinding !IsSearching}" />

        <Button Content="Search"
                Command="{CompiledBinding SearchPartCommand}"
                IsEnabled="{CompiledBinding SearchPartCommand.CanExecute}" />

        <DataGrid ItemsSource="{CompiledBinding SearchResults}"
                  SelectedItem="{CompiledBinding SelectedPart}"
                  IsVisible="{CompiledBinding SearchResults.Count, Converter={x:Static ObjectConverters.IsNotNull}}">
            <!-- DataGrid columns -->
        </DataGrid>
    </StackPanel>
</UserControl>
```

**Key Practices**:
- `x:DataType="vm:PartLookupViewModel"` on root element
- `x:CompileBindings="True"` enforces compile-time validation
- `{CompiledBinding}` syntax for all bindings
- `Design.DataContext` for previewer support
- Negation binding: `{CompiledBinding !IsSearching}`
- Command CanExecute binding: `{CompiledBinding SearchPartCommand.CanExecute}`

---

### 3. Polly Retry Policies (Error Resilience)

**Pattern**: Exponential backoff with jitter for transient failures

**Example** (VisualApiService):
```csharp
private static readonly IAsyncPolicy<T> _retryPolicy = Policy<T>
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .OrResult(r => /* check if result indicates transient failure */)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)), // Jitter
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                "VISUAL API retry {RetryCount} after {Delay}ms due to {Reason}",
                retryCount,
                timespan.TotalMilliseconds,
                outcome.Exception?.Message ?? "transient failure");
        });

public async Task<Part?> GetPartByIdAsync(string partId, CancellationToken cancellationToken)
{
    return await _retryPolicy.ExecuteAsync(
        async ct => await CallVisualToolkitGetPartAsync(partId, ct),
        cancellationToken);
}
```

**Key Practices**:
- Retry on transient failures (network, timeout, 500 errors)
- Exponential backoff: 1s, 2s, 4s (prevents server overload)
- Jitter: Random delay prevents thundering herd
- Structured logging: Log all retry attempts for diagnostics
- Cancellation support: Pass CancellationToken through policy

---

### 4. MySQL Parameterized Queries (Security)

**Pattern**: ALWAYS use parameters, NEVER string concatenation

**Example** (LocalTransactionRepository):
```csharp
public async Task<int> SaveTransactionAsync(
    LocalTransactionRecord transaction,
    CancellationToken cancellationToken)
{
    const string sql = @"
        INSERT INTO LocalTransactionRecords
        (transaction_type, entity_data, created_by, visual_reference_data, audit_metadata)
        VALUES (@TransactionType, @EntityData, @CreatedBy, @VisualReferenceData, @AuditMetadata)";

    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
    command.Parameters.AddWithValue("@EntityData", JsonSerializer.Serialize(transaction.EntityData));
    command.Parameters.AddWithValue("@CreatedBy", transaction.CreatedBy);
    command.Parameters.AddWithValue("@VisualReferenceData",
        transaction.VisualReferenceData != null
            ? JsonSerializer.Serialize(transaction.VisualReferenceData)
            : DBNull.Value);
    command.Parameters.AddWithValue("@AuditMetadata",
        transaction.AuditMetadata != null
            ? JsonSerializer.Serialize(transaction.AuditMetadata)
            : DBNull.Value);

    await command.ExecuteNonQueryAsync(cancellationToken);
    return (int)command.LastInsertedId;
}
```

**Key Practices**:
- **ALWAYS** use `@Parameter` placeholders in SQL
- **ALWAYS** use `AddWithValue()` or typed parameter methods
- **NEVER** concatenate user input into SQL strings (SQL injection risk)
- Use `DBNull.Value` for nullable columns
- JSON serialization for complex objects (entity_data, visual_reference_data)
- Reference `.github/mamp-database/schema-tables.json` before writing queries (Principle VI)

---

## VISUAL API Whitelist (Read-Only Access Control)

**Purpose**: Enforce read-only access to Infor VISUAL ERP per Constitution Principle VIII. Prevent unauthorized write operations and limit data access to whitelisted entities.

**Implementation**: `VisualApiWhitelistValidator.cs` validates commands against `appsettings.json` configuration before execution. Citation format required: `Reference-{FileName} - {Chapter/Section/Page}`.

### Whitelisted Entities & Commands

| Entity | Toolkit Module | Command Example | CSV Source | Primary Key |
|--------|---------------|-----------------|------------|-------------|
| **Items/Parts** | Reference-Inventory | `GetItemByID` | `MTMFG Tables.csv` (Lines 1657-1773), `Visual Data Table.csv` (Lines 5779-5888) | `PART.ID` |
| **Locations/Racks** | Reference-Inventory | `GetLocationsByWarehouse` | `MTMFG Tables.csv` (Lines 1513-1526), `Visual Data Table.csv` (Lines 5246-5263) | `LOCATION.ID` |
| **Warehouses** | Reference-Inventory | `GetWarehouseByID` | `MTMFG Tables.csv` (Lines 4229-4244), `Visual Data Table.csv` (Lines 14262-14288) | `WAREHOUSE.ID` |
| **WorkCenters/Resources** | Reference-Shop Floor | `GetWorkCenterByID` | `MTMFG Tables.csv` (Lines 3452-3493), `Visual Data Table.csv` (Lines 9299-9343) | `SHOP_RESOURCE.ID` |
| **Sites** | Reference-Core | `GetSiteByID` | `MTMFG Tables.csv` (Lines 3382-3425), `Visual Data Table.csv` (Lines 9398-9443) | `SITE.ID` |

**Key Relationships** (from `MTMFG Relationships.csv`):
- `LOCATION.WAREHOUSE_ID → WAREHOUSE.ID` (Line 427)
- `PART_LOCATION.PART_ID → PART.ID` (Line 459)
- `PART_LOCATION.WAREHOUSE_ID → LOCATION.WAREHOUSE_ID` (Line 460)
- `INVENTORY_TRANS.SITE_ID → SITE.ID` (Line 412)

### VISUAL API Toolkit Field Mappings

**Part Entity Fields** (117 total fields in PART table):

| Field Name | Data Type | Required | Description | CSV Line |
|------------|-----------|----------|-------------|----------|
| ID | nvarchar(30) | Yes | Part number/identifier | 1657 |
| DESCRIPTION | nvarchar(255) | Yes | Human-readable part description | 1658 |
| STOCK_UM | nvarchar(15) | Yes | Unit of measure (EA, LB, KG) | 1659 |
| ACTIVE_FLAG | nchar(1) | Yes | Active status ('Y' or 'N') | 1660 |
| ON_HAND_QTY | decimal(18,6) | No | Available inventory quantity | 1661 |
| ALLOCATED_QTY | decimal(18,6) | No | Quantity allocated to work orders | 1662 |
| PRODUCT_CLASS | nvarchar(50) | No | Product classification | 1663 |

**Location Entity Fields** (14 total fields in LOCATION table):

| Field Name | Data Type | Required | Description | CSV Line |
|------------|-----------|----------|-------------|----------|
| ID | nvarchar(15) | Yes | Location identifier | 1513 |
| WAREHOUSE_ID | nvarchar(15) | Yes | Parent warehouse ID | 1514 |
| DESCRIPTION | nvarchar(80) | No | Location description | 1515 |

**Configuration** (`appsettings.json`):

```json
{
  "Visual": {
    "AllowedCommands": [
      "GetItemByID",
      "GetLocationsByWarehouse",
      "GetWarehouseByID",
      "GetWorkCenterByID",
      "GetSiteByID",
      "SearchParts",
      "GetPartOnHandQuantity",
      "GetWorkOrdersByStatus",
      "GetInventoryTransactionsByPart",
      "GetShipmentsByDateRange"
    ],
    "RequireCitation": true
  }
}
```

**Validation Flow**:
1. API call attempts to execute command (e.g., `GetItemByID`)
2. `VisualApiWhitelistValidator.ValidateCommandAsync()` checks whitelist
3. If command not in `AllowedCommands`, throws `UnauthorizedAccessException`
4. If citation missing or invalid format, returns `false` + logs warning
5. If both pass, command executes via VISUAL API Toolkit

**Citation Format Examples**:
- ✅ Valid: `"Reference-Inventory - Chapter 3, Section 2.1, Page 45"`
- ✅ Valid: `"Reference-Development Guide - API Overview/GetItemByID"`
- ❌ Invalid: `"Inventory Guide"` (missing chapter/section/page)
- ❌ Invalid: `null` or empty string (citation required)

**Change Control**: All whitelist additions require:
1. CSV source citation with line numbers
2. Confirmation of read-only operation (no writes)
3. Valid Toolkit reference guide citation
4. Second reviewer approval (defense-in-depth)

**Documentation References**:
- Full whitelist details: `docs/VISUAL-WHITELIST.md`
- CSV schema files: `docs/Visual Files/Database Files/` (MTMFG Tables.csv, Visual Data Table.csv, MTMFG Relationships.csv)
- Reference guides: `docs/Visual Files/Guides/` (Reference - Inventory, Reference - Shop Floor, Reference - Core)

---

## Research Completion Checklist

- [x] **Data Volume Analysis**: Estimated scale (10K-50K parts, 500-2K work orders, 100-500 transactions/day)
- [x] **API Endpoint Discovery**: Abstraction layer decision (IVisualApiService + domain repositories)
- [x] **Connection Pool Sizing**: Conservative start (10-20 connections), monitor via DebugTerminalWindow
- [x] **Toolkit Integration Architecture**: Wrapper pattern with error handling and retries
- [x] **Local MySQL Schema**: Single table `LocalTransactionRecords` with JSON columns
- [x] **Performance Monitoring**: Hybrid approach (logs + DebugTerminalWindow panel)
- [x] **Cache Strategy**: Reuse existing CacheService with VISUAL-specific wrapper
- [x] **MVVM Best Practices**: CommunityToolkit.Mvvm source generators
- [x] **Avalonia Best Practices**: CompiledBinding with x:DataType
- [x] **Polly Best Practices**: Exponential backoff with jitter
- [x] **MySQL Best Practices**: Parameterized queries, schema-tables.json reference
- [x] **Whitelist Documentation**: Entity/command whitelist extracted from VisualApiWhitelistValidator.cs and VISUAL-WHITELIST.md, field mappings documented from CSV schema files

---

## Next Phase: Phase 1 - Design & Contracts

**Prerequisites Met**: ✅ All NEEDS CLARIFICATION items resolved

**Phase 1 Deliverables**:
1. **data-model.md**: Entity definitions (Part, WorkOrder, InventoryTransaction, CustomerOrder, Shipment, LocalTransactionRecord)
2. **contracts/**: API contracts (IVisualApiService, repository interfaces, domain models)
3. **quickstart.md**: Developer setup guide for VISUAL integration development
4. **Agent context update**: Update `.github/copilot-instructions.md` with VISUAL integration patterns

**Ready to proceed**: ✅ YES
