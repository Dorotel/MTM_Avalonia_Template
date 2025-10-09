# Data Model & Entity Design

**Feature**: 005 - Manufacturing Application Modernization (All-in-One Mega-Feature)
**Date**: 2025-10-08
**Status**: In Progress - Phases 1-4 Design Complete

## Entity Definitions

### Section 1: Custom Control Metadata (Phase 1)

#### 1.1 ControlDefinition

**Description**: Metadata describing a reusable Avalonia custom control for documentation and discovery purposes.

**Source**: In-memory catalog (loaded from UI-CUSTOM-CONTROLS-CATALOG.md documentation)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| ControlName | string | Yes | Unique, max 100 chars | Control class name (e.g., "StatusCard") |
| DisplayName | string | Yes | Max 200 chars | Human-readable name |
| Purpose | string | Yes | Max 500 chars | Description of control's purpose |
| Category | string | Yes | Max 50 chars | Category (Navigation, Display, Input, Diagnostic, Dialog) |
| Properties | List<PropertyDefinition> | Yes | N/A | List of StyledProperty definitions |
| UsageExample | string | Yes | Max 2000 chars | XAML usage example |
| ScreenshotPath | string | No | Max 500 chars | Path to screenshot image |
| DocumentationUrl | string | No | Max 500 chars | Link to detailed documentation |

**Relationships**:
- **ControlDefinition → PropertyDefinition**: One-to-many

**Validation Rules**:
- ControlName must be unique within catalog
- Category must be one of: Navigation, Display, Input, Diagnostic, Dialog
- UsageExample must be valid XAML

**Example**:
```json
{
  "controlName": "StatusCard",
  "displayName": "Status Card",
  "purpose": "Displays status information with icon, title, and status indicator",
  "category": "Display",
  "properties": [
    {"name": "Title", "type": "string", "defaultValue": "\"\"", "isRequired": false},
    {"name": "Status", "type": "string", "defaultValue": "\"\"", "isRequired": false},
    {"name": "IconSource", "type": "string", "defaultValue": "null", "isRequired": false}
  ],
  "usageExample": "<controls:StatusCard Title=\"Boot Sequence\" Status=\"Complete\" IconSource=\"CheckCircle\" />",
  "screenshotPath": "/docs/images/controls/status-card.png"
}
```

---

### Section 2: Settings Management (Phase 2)

#### 2.1 SettingDefinition

**Description**: Represents a configurable application setting exposed through Settings UI.

**Source**: IConfigurationService (persisted to UserPreferences table in MySQL)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| SettingKey | string | Yes | Unique, max 200 chars | Configuration key (e.g., "UI:Theme") |
| SettingValue | string | Yes | N/A | Current value (serialized) |
| SettingType | SettingValueType | Yes | Enum | String, Int, Decimal, Boolean, Enum, FilePath, ConnectionString, URL |
| Category | SettingCategory | Yes | Enum | General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer |
| DisplayName | string | Yes | Max 200 chars | Human-readable label |
| Description | string | No | Max 500 chars | Help text for user |
| ValidationRules | List<ValidationRule> | No | N/A | FluentValidation rules |
| DefaultValue | string | No | N/A | Fallback when user value not set |
| IsReadOnly | bool | Yes | N/A | If true, displayed but not editable |
| IsSensitive | bool | Yes | N/A | If true, masked in UI and exports |
| LastModified | DateTime | Yes | N/A | Timestamp of last change |
| ModifiedBy | string | No | Max 100 chars | User who made last change |

**Relationships**:
- **SettingDefinition → SettingCategory**: Many-to-one
- **SettingDefinition → ValidationRule**: One-to-many

**Validation Rules**:
- SettingKey must be unique
- ConnectionString type must pass connection string format validation
- URL type must be valid URI format
- FilePath type must be valid absolute or relative path
- Numeric types (Int, Decimal) must parse correctly

**State Transitions**: Staged → Saved → Persisted

**Example**:
```json
{
  "settingKey": "Database:ConnectionString",
  "settingValue": "Server=localhost;Port=3306;Database=mtm_template_dev;User=root;Password=***FILTERED***",
  "settingType": "ConnectionString",
  "category": "Database",
  "displayName": "MySQL Connection String",
  "description": "Connection string for MAMP MySQL database",
  "validationRules": ["NotEmpty", "ValidConnectionStringFormat"],
  "defaultValue": "Server=localhost;Port=3306;Database=mtm_template_dev;User=root",
  "isReadOnly": false,
  "isSensitive": true,
  "lastModified": "2025-10-08T14:30:00Z",
  "modifiedBy": "admin"
}
```

#### 2.2 SettingCategory

**Description**: Groups related settings for organized navigation in Settings UI.

**Source**: Hard-coded enum in application

**Enum Values**:
- General - Theme, language, startup behavior
- Database - Connection strings, query timeouts
- VISUAL ERP - API endpoints, credentials, cache policies
- Logging - Log levels, file paths, structured logging
- UI - Themes, fonts, layout preferences
- Cache - TTL policies, size limits, eviction strategies
- Performance - Boot time budgets, query thresholds
- Developer - Debug mode, trace levels, diagnostic options

---

### Section 3: Debug Terminal Diagnostics (Phase 3)

#### 3.1 DiagnosticSection

**Description**: Represents a feature-specific section in the modernized Debug Terminal with SplitView navigation.

**Source**: In-memory (constructed from diagnostic data collectors)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| SectionId | string | Yes | Unique, max 50 chars | Section identifier (e.g., "feature-001-boot") |
| SectionName | string | Yes | Max 200 chars | Display name (e.g., "Feature 001: Boot") |
| Icon | string | No | Max 100 chars | Material icon name |
| Order | int | Yes | >= 0 | Display order in navigation menu |
| IsEnabled | bool | Yes | N/A | If false, section hidden |
| ContentType | DiagnosticContentType | Yes | Enum | Timeline, Metrics, Logs, Configuration, Performance |
| DataSource | string | Yes | Max 200 chars | Data provider service name |

**Relationships**:
- **DiagnosticSection → PerformanceSnapshot**: One-to-many (for Performance type)
- **DiagnosticSection → BootTimelineEntry**: One-to-many (for Timeline type)
- **DiagnosticSection → ErrorLogEntry**: One-to-many (for Logs type)

**Validation Rules**:
- SectionId must be unique
- Order must be non-negative
- DataSource must reference valid service

**Example**:
```json
{
  "sectionId": "feature-001-boot",
  "sectionName": "Feature 001: Boot",
  "icon": "RocketLaunch",
  "order": 1,
  "isEnabled": true,
  "contentType": "Timeline",
  "dataSource": "BootOrchestratorService"
}
```

---

### Section 4: VISUAL ERP Integration (Phase 5) - Read-Only Entities

#### 4.1 Part (VISUAL ERP Read-Only Entity)

**Description**: Represents a manufactured or purchased component in VISUAL ERP system. Read from VISUAL API Toolkit, never written directly.

**Source**: Infor VISUAL ERP via API Toolkit (read-only)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| PartId | string | Yes | Unique, max 50 chars | Part number/identifier (e.g., "12345-ABC") |
| Description | string | Yes | Max 500 chars | Human-readable part description |
| UnitOfMeasure | string | Yes | Max 10 chars | Unit (EA, LB, KG, FT, etc.) |
| OnHandQuantity | decimal | Yes | >= 0 | Available inventory quantity |
| AllocatedQuantity | decimal | Yes | >= 0 | Quantity allocated to work orders |
| Location | string | No | Max 100 chars | Warehouse location code |
| ProductClass | string | No | Max 50 chars | Product classification (Raw Material, Finished Good, etc.) |
| Specifications | Dictionary<string, string> | No | N/A | Key-value pairs for dimensions, weight, material, etc. |
| LastUpdated | DateTime | Yes | N/A | Cache timestamp (when fetched from VISUAL) |

**Relationships**:
- **Part → WorkOrder**: Many-to-many (via material requirements)
- **Part → InventoryTransaction**: One-to-many
- **Part → LocalTransactionRecord**: One-to-many (via visual_reference_data JSON)

**Validation Rules**:
- PartId must be unique (enforced by VISUAL)
- OnHandQuantity and AllocatedQuantity cannot be negative
- UnitOfMeasure must be valid VISUAL UoM code

**State Transitions**: N/A (read-only entity, state managed by VISUAL)

**Cache Behavior**:
- **TTL**: 24 hours (from CacheStalenessDetector)
- **Cache Key**: `visual:part:{PartId}`
- **Eviction**: LRU when cache exceeds 40MB compressed

**Example**:
```json
{
  "partId": "12345-ABC",
  "description": "Steel Bracket Assembly",
  "unitOfMeasure": "EA",
  "onHandQuantity": 150.00,
  "allocatedQuantity": 50.00,
  "location": "WH-01-A-05",
  "productClass": "Finished Good",
  "specifications": {
    "length": "12 inches",
    "weight": "2.5 lbs",
    "material": "Steel ASTM A36"
  },
  "lastUpdated": "2025-10-08T14:30:00Z"
}
```

---

#### 4.2 WorkOrder (VISUAL ERP Read-Only Entity)

**Description**: Represents a manufacturing job scheduled for production. Read from VISUAL API Toolkit for display and decision-making. Status updates persist to local MySQL only (VISUAL remains unchanged).

**Source**: Infor VISUAL ERP via API Toolkit (read-only)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| WorkOrderId | string | Yes | Unique, max 50 chars | Work order number (e.g., "WO-2024-001") |
| PartId | string | Yes | FK to Part | Output part number |
| QuantityToProduce | decimal | Yes | > 0 | Target production quantity |
| DueDate | DateTime | Yes | N/A | Scheduled completion date |
| CurrentStatus | WorkOrderStatus | Yes | Enum | Open, InProgress, Closed |
| RequiredMaterials | List<MaterialRequirement> | No | N/A | List of input parts with quantities |
| OperationSteps | List<OperationStep> | No | N/A | Production steps with estimated hours |
| LaborHours | decimal | No | >= 0 | Actual labor hours recorded |
| LastUpdated | DateTime | Yes | N/A | Cache timestamp (when fetched from VISUAL) |

**Nested Types**:

**MaterialRequirement**:
- PartId (string, FK to Part)
- RequiredQuantity (decimal)
- AllocatedQuantity (decimal)
- IssuedQuantity (decimal)

**OperationStep**:
- OperationNumber (int)
- OperationDescription (string)
- EstimatedHours (decimal)
- ActualHours (decimal)

**WorkOrderStatus Enum**:
```csharp
public enum WorkOrderStatus
{
    Open,
    InProgress,
    Closed
}
```

**Relationships**:
- **WorkOrder → Part**: Many-to-one (output part)
- **WorkOrder → Part**: Many-to-many (via RequiredMaterials)
- **WorkOrder → InventoryTransaction**: One-to-many (material issues)
- **WorkOrder → LocalTransactionRecord**: One-to-many (status updates saved locally)

**Validation Rules**:
- WorkOrderId must be unique (enforced by VISUAL)
- QuantityToProduce must be positive
- DueDate cannot be in the past (VISUAL may allow, but warn user)
- CurrentStatus transitions: Open → InProgress → Closed (one-way, no reverting)

**State Transitions**:
```
[Open] --user updates--> [InProgress] --user updates--> [Closed]
```

*Note*: State transitions are saved to LocalTransactionRecords table, NOT sent to VISUAL.

**Cache Behavior**:
- **TTL**: 7 days (from CacheStalenessDetector)
- **Cache Key**: `visual:workorder:{WorkOrderId}` (single work order)
- **Cache Key**: `visual:workorders:list:{filterHash}` (work order list query)
- **Eviction**: LRU when cache exceeds 40MB compressed

**Example**:
```json
{
  "workOrderId": "WO-2024-001",
  "partId": "12345-ABC",
  "quantityToProduce": 100.00,
  "dueDate": "2025-10-15T00:00:00Z",
  "currentStatus": "InProgress",
  "requiredMaterials": [
    {
      "partId": "RM-001",
      "requiredQuantity": 200.00,
      "allocatedQuantity": 200.00,
      "issuedQuantity": 150.00
    }
  ],
  "operationSteps": [
    {
      "operationNumber": 10,
      "operationDescription": "Cut Steel",
      "estimatedHours": 2.0,
      "actualHours": 1.8
    }
  ],
  "laborHours": 12.5,
  "lastUpdated": "2025-10-08T14:30:00Z"
}
```

---

#### 4.3 InventoryTransaction (VISUAL ERP Read-Only Entity)

**Description**: Represents historical inventory movements in VISUAL ERP. Read-only data for reference. New transactions are saved to LocalTransactionRecords table only (VISUAL unchanged).

**Source**: Infor VISUAL ERP via API Toolkit (read-only, historical data)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| TransactionId | string | Yes | Unique | VISUAL transaction ID |
| TransactionType | TransactionType | Yes | Enum | Receipt, Issue, Adjustment, Transfer |
| PartId | string | Yes | FK to Part | Part number |
| Quantity | decimal | Yes | != 0 | Transaction quantity (positive or negative) |
| UnitOfMeasure | string | Yes | Max 10 chars | Unit |
| ReferenceDocument | string | No | Max 50 chars | PO number, WO number, etc. |
| TransactionDate | DateTime | Yes | N/A | When transaction occurred |
| CreatedBy | string | No | Max 100 chars | Username who recorded transaction |
| LastUpdated | DateTime | Yes | N/A | Cache timestamp |

**TransactionType Enum**:
```csharp
public enum TransactionType
{
    Receipt,         // Receive into inventory (PO receipt)
    Issue,           // Issue to work order
    Adjustment,      // Physical count adjustment
    Transfer         // Move between locations
}
```

**Relationships**:
- **InventoryTransaction → Part**: Many-to-one
- **InventoryTransaction → WorkOrder**: Many-to-one (optional, for Issue transactions)

**Validation Rules**:
- TransactionId must be unique (VISUAL-generated)
- Quantity cannot be zero
- Receipt/Issue transactions require ReferenceDocument (PO/WO number)

**State Transitions**: N/A (immutable historical records)

**Cache Behavior**:
- **TTL**: 7 days (from CacheStalenessDetector)
- **Cache Key**: `visual:inventory:transactions:{partId}` (transaction history per part)
- **Eviction**: LRU when cache exceeds 40MB compressed

---

#### 4.4 CustomerOrder / Shipment (VISUAL ERP Read-Only Entity)

**Description**: Represents customer orders and their fulfillment status. Read from VISUAL for order lookup. Shipment confirmations are saved to LocalTransactionRecords table only (VISUAL unchanged).

**Source**: Infor VISUAL ERP via API Toolkit (read-only)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| OrderId | string | Yes | Unique | Customer order number (e.g., "SO-2024-100") |
| CustomerName | string | Yes | Max 200 chars | Customer name |
| OrderDate | DateTime | Yes | N/A | Date order was placed |
| ShipDate | DateTime | No | N/A | Scheduled/actual ship date |
| TrackingNumber | string | No | Max 100 chars | Carrier tracking number |
| Carrier | string | No | Max 100 chars | Shipping carrier name |
| OrderLines | List<OrderLine> | Yes | Min 1 line | Line items (part, quantity, prices) |
| ShipmentStatus | ShipmentStatus | Yes | Enum | Open, PartiallyShipped, Shipped, Invoiced |
| LastUpdated | DateTime | Yes | N/A | Cache timestamp |

**Nested Types**:

**OrderLine**:
- LineNumber (int)
- PartId (string, FK to Part)
- QuantityOrdered (decimal)
- QuantityShipped (decimal)
- UnitPrice (decimal)

**ShipmentStatus Enum**:
```csharp
public enum ShipmentStatus
{
    Open,
    PartiallyShipped,
    Shipped,
    Invoiced
}
```

**Relationships**:
- **CustomerOrder → Part**: Many-to-many (via OrderLines)
- **CustomerOrder → LocalTransactionRecord**: One-to-many (shipment confirmations saved locally)

**Validation Rules**:
- OrderId must be unique (VISUAL-generated)
- OrderLines must have at least 1 line
- QuantityShipped cannot exceed QuantityOrdered per line
- ShipmentStatus = Shipped requires TrackingNumber and Carrier

**State Transitions**:
```
[Open] --partial ship--> [PartiallyShipped] --complete ship--> [Shipped] --invoice--> [Invoiced]
```

*Note*: Shipment confirmations are saved to LocalTransactionRecords, NOT sent to VISUAL.

**Cache Behavior**:
- **TTL**: 7 days (from CacheStalenessDetector)
- **Cache Key**: `visual:order:{OrderId}`
- **Eviction**: LRU when cache exceeds 40MB compressed

---

#### 4.5 LocalTransactionRecord (Local MAMP MySQL Entity - System of Record)

**Description**: Represents user-entered transactions persisted exclusively to local MAMP MySQL database. This is the **system of record** for all write operations from this application. No syncing to VISUAL server.

**Source**: User input via application (Work Order status updates, Inventory transactions, Shipment confirmations)

**Storage**: MAMP MySQL 5.7 table `LocalTransactionRecords` (see `.github/mamp-database/schema-tables.json`)

**Attributes**:

| Attribute Name | Type | Required | Constraints | Description |
|---------------|------|----------|-------------|-------------|
| TransactionId | int | Yes | PK, Auto-increment | Unique transaction ID (MySQL-generated) |
| TransactionType | LocalTransactionType | Yes | Enum | WorkOrderStatusUpdate, InventoryReceipt, etc. |
| EntityData | JSON | Yes | Valid JSON | Complete transaction payload (part-specific structure per type) |
| CreatedAt | DateTime | Yes | Default CURRENT_TIMESTAMP | When transaction was recorded |
| CreatedBy | string | Yes | Max 100 chars | Username (from WindowsSecretsService) |
| VisualReferenceData | JSON | No | Valid JSON | Cached VISUAL data at time of transaction (for audit/validation) |
| AuditMetadata | JSON | No | Valid JSON | Additional audit info (IP, app version, etc.) |

**LocalTransactionType Enum**:
```csharp
public enum LocalTransactionType
{
    WorkOrderStatusUpdate,
    InventoryReceipt,
    InventoryIssue,
    InventoryAdjustment,
    InventoryTransfer,
    ShipmentConfirmation
}
```

**EntityData Structure** (varies by TransactionType):

**WorkOrderStatusUpdate**:
```json
{
  "workOrderId": "WO-2024-001",
  "previousStatus": "Open",
  "newStatus": "InProgress",
  "notes": "Started production today"
}
```

**InventoryReceipt**:
```json
{
  "partId": "12345-ABC",
  "quantity": 100.00,
  "unitOfMeasure": "EA",
  "referenceDocument": "PO-2024-050",
  "location": "WH-01-A-05"
}
```

**InventoryIssue**:
```json
{
  "partId": "RM-001",
  "quantity": 50.00,
  "unitOfMeasure": "EA",
  "workOrderId": "WO-2024-001",
  "location": "WH-01-B-03"
}
```

**ShipmentConfirmation**:
```json
{
  "orderId": "SO-2024-100",
  "trackingNumber": "1Z999AA10123456784",
  "carrier": "UPS",
  "shipDate": "2025-10-08",
  "orderLines": [
    {
      "lineNumber": 1,
      "partId": "12345-ABC",
      "quantityShipped": 50.00
    }
  ]
}
```

**Relationships**:
- **LocalTransactionRecord → Part**: Many-to-one (via EntityData.partId)
- **LocalTransactionRecord → WorkOrder**: Many-to-one (via EntityData.workOrderId)
- **LocalTransactionRecord → CustomerOrder**: Many-to-one (via EntityData.orderId)

**Validation Rules**:
- EntityData must be valid JSON
- EntityData structure must match TransactionType requirements
- CreatedBy must be valid Windows username
- VisualReferenceData should contain cached VISUAL entity data at time of transaction (for validation warnings)

**State Transitions**: N/A (immutable audit records)

**MySQL Schema** (from `.github/mamp-database/schema-tables.json`):
```sql
CREATE TABLE LocalTransactionRecords (
    transaction_id          INT AUTO_INCREMENT PRIMARY KEY,
    transaction_type        ENUM('WorkOrderStatusUpdate', 'InventoryReceipt', 'InventoryIssue',
                                 'InventoryAdjustment', 'InventoryTransfer', 'ShipmentConfirmation') NOT NULL,
    entity_data             JSON NOT NULL COMMENT 'Complete transaction payload',
    created_at              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              VARCHAR(100) NOT NULL COMMENT 'Username',
    visual_reference_data   JSON NULL COMMENT 'Cached VISUAL data at transaction time',
    audit_metadata          JSON NULL COMMENT 'Additional audit info',

    INDEX idx_transaction_type (transaction_type),
    INDEX idx_created_at (created_at),
    INDEX idx_created_by (created_by)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

### 6. VisualPerformanceMetrics (Local Application Entity)

**Description**: Real-time performance metrics for VISUAL API Toolkit operations. Used for performance monitoring (FR-019) and automatic degradation detection (FR-020).

**Source**: Application-generated (VisualPerformanceMonitor service)

**Storage**: In-memory (ObservableCollection for UI binding) + structured logs (Serilog)

**Attributes**:

| Attribute Name | Type | Required | Description |
|---------------|------|----------|-------------|
| Timestamp | DateTime | Yes | When API call was made |
| Operation | string | Yes | API operation name (e.g., "GetPart", "GetWorkOrders") |
| DurationMs | long | Yes | Response time in milliseconds |
| Status | PerformanceStatus | Yes | Success, Timeout, Error |
| ErrorMessage | string | No | Error details if Status = Error |
| WasCached | bool | Yes | True if served from cache, false if live API call |

**PerformanceStatus Enum**:
```csharp
public enum PerformanceStatus
{
    Success,
    Timeout,
    Error
}
```

**Usage**:
- Collected by `VisualPerformanceMonitor` service wrapping all VISUAL API calls
- Exposed via `ObservableCollection<VisualPerformanceMetrics>` for DebugTerminalWindow UI binding
- Logged to Serilog structured logs: `{ "timestamp": "...", "operation": "GetPart", "duration_ms": 1234, "status": "success" }`
- Used by `VisualDegradationDetector` to trigger degradation mode (5 consecutive failures)

---

## Entity Relationship Diagram (ERD)

### Phase 1-4: Custom Controls, Settings, Debug Terminal

```
┌─────────────────────────────────┐
│     ControlDefinition           │ (In-Memory Catalog)
│  - ControlName (PK)             │
│  - DisplayName                  │
│  - Purpose                      │
│  - Category                     │
│  - Properties (List)            │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│     SettingCategory             │ (Enum)
│  - General                      │
│  - Database                     │
│  - VISUAL ERP                   │
│  - Logging / UI / Cache         │
└─────┬───────────────────────────┘
      │
      │ 1:N
      │
┌─────▼───────────────────────────┐
│     SettingDefinition           │ (IConfigurationService + MySQL)
│  - SettingKey (PK)              │
│  - SettingValue                 │
│  - SettingType                  │
│  - Category (FK)                │
│  - ValidationRules (List)       │
│  - IsSensitive                  │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│     DiagnosticSection           │ (In-Memory)
│  - SectionId (PK)               │
│  - SectionName                  │
│  - ContentType                  │
│  - DataSource                   │
└─────┬───────────────────────────┘
      │
      │ 1:N (for Performance type)
      │
┌─────▼───────────────────────────┐
│   PerformanceSnapshot           │ (From DiagnosticsService)
│  - Timestamp                    │
│  - TotalMemoryMB                │
│  - Stage0/1/2DurationMs         │
│  - ErrorCount                   │
└─────────────────────────────────┘
```

### Phase 5: VISUAL ERP Integration

```
┌─────────────────────┐
│       Part          │ (VISUAL Read-Only)
│  - PartId (PK)      │
│  - Description      │
│  - OnHandQuantity   │
└──────┬──────────────┘
       │
       │ 1:N
       │
┌──────▼──────────────┐
│   WorkOrder         │ (VISUAL Read-Only)
│  - WorkOrderId (PK) │
│  - PartId (FK)      │
│  - QuantityToProduce│
│  - CurrentStatus    │
└──────┬──────────────┘
       │
       │ 1:N
       │
┌──────▼──────────────────────┐
│ InventoryTransaction        │ (VISUAL Read-Only)
│  - TransactionId (PK)       │
│  - PartId (FK)              │
│  - WorkOrderId (FK, opt)    │
│  - Quantity                 │
└─────────────────────────────┘

┌────────────────────────────┐
│     CustomerOrder          │ (VISUAL Read-Only)
│  - OrderId (PK)            │
│  - CustomerName            │
│  - ShipmentStatus          │
│  - OrderLines              │
└────────────────────────────┘

┌──────────────────────────────────┐
│   LocalTransactionRecord         │ (Local MySQL - System of Record)
│  - TransactionId (PK)            │
│  - TransactionType               │
│  - EntityData (JSON)             │
│  - CreatedBy                     │
│  - VisualReferenceData (JSON)   │
└──────────────────────────────────┘

┌──────────────────────────────┐
│  VisualPerformanceMetrics    │ (In-Memory + Logs)
│  - Timestamp                 │
│  - Operation                 │
│  - DurationMs                │
│  - Status                    │
└──────────────────────────────┘
```

**Key Relationships**:
- **Phase 1-4 Entities**: Custom controls (catalog), settings (MySQL UserPreferences), diagnostics (in-memory)
- **VISUAL Entities** (Part, WorkOrder, InventoryTransaction, CustomerOrder): Read-only, cached with TTL
- **LocalTransactionRecord**: Permanent storage for user-entered transactions (MAMP MySQL)
- **VisualPerformanceMetrics**: Ephemeral monitoring data (in-memory + logs)

**Data Flow**:
1. **Read**: VISUAL API → Cache → Application Display
2. **Write**: User Input → LocalTransactionRecord (MAMP MySQL) → No VISUAL update
3. **Validation**: LocalTransactionRecord.VisualReferenceData stores cached VISUAL state at transaction time for warnings (FR-013)

---

## Export Formats (FR-012)

FR-012 requires transaction export functionality for LocalTransactionRecord data. This section defines CSV column order and JSON schema for exported data.

### CSV Export Format

**Filename Pattern**: `visual-transactions-export-{yyyyMMdd-HHmmss}.csv`

**Column Order** (all transaction types):

| Position | Column Name | Data Type | Description | Example |
|----------|-------------|-----------|-------------|---------|
| 1 | TransactionId | GUID | Unique transaction identifier | `a7f3c9d2-8b4e-4a5f-9c1b-3e8d7f2a6b4c` |
| 2 | UserId | string | User who created transaction | `john.doe` |
| 3 | TransactionType | string | Transaction type enum value | `WorkOrderUpdate`, `InventoryReceipt`, `ShipmentConfirmation` |
| 4 | Timestamp | ISO8601 | UTC timestamp of transaction | `2025-10-08T14:35:22Z` |
| 5 | EntityType | string | VISUAL entity type affected | `WorkOrder`, `InventoryTransaction`, `Shipment` |
| 6 | EntityId | string | VISUAL entity identifier | `WO-2025-001`, `INV-2025-123` |
| 7 | EntityData | JSON string | Entity-specific data (escaped JSON) | `"{\"PartId\":\"12345-ABC\",\"Quantity\":100}"` |
| 8 | VisualReferenceData | JSON string | Cached VISUAL data at transaction time (escaped JSON) | `"{\"OnHandQuantity\":150,\"AllocatedQuantity\":50}"` |

**Header Row**: First row contains column names (required)

**Encoding**: UTF-8 with BOM

**Line Endings**: Windows CRLF (`\r\n`)

**String Escaping**: Double-quote enclosed values, escape internal quotes with `""`

**Example CSV**:
```csv
TransactionId,UserId,TransactionType,Timestamp,EntityType,EntityId,EntityData,VisualReferenceData
"a7f3c9d2-8b4e-4a5f-9c1b-3e8d7f2a6b4c","john.doe","WorkOrderUpdate","2025-10-08T14:35:22Z","WorkOrder","WO-2025-001","{\"WorkOrderId\":\"WO-2025-001\",\"Status\":\"InProgress\",\"CompletionPercentage\":45}","{\"PartDescription\":\"Steel Bracket Assembly\",\"DueDate\":\"2025-10-15\"}"
"b8g4d0e3-9c5f-5b6g-0d2c-4f9e8g3b7c5d","jane.smith","InventoryReceipt","2025-10-08T15:20:10Z","InventoryTransaction","INV-2025-123","{\"PartId\":\"12345-ABC\",\"Quantity\":100,\"TransactionType\":\"Receipt\",\"ReferenceDocument\":\"PO-2025-456\"}","{\"OnHandQuantity\":150,\"UnitOfMeasure\":\"EA\"}"
```

### JSON Export Format

**Filename Pattern**: `visual-transactions-export-{yyyyMMdd-HHmmss}.json`

**Root Schema**:
```json
{
  "exportMetadata": {
    "timestamp": "2025-10-08T14:35:22Z",         // ISO8601 UTC export timestamp
    "exportedBy": "john.doe",                     // User who triggered export
    "transactionCount": 245,                      // Total records in export
    "dateRangeStart": "2025-10-01T00:00:00Z",   // Earliest transaction timestamp
    "dateRangeEnd": "2025-10-08T23:59:59Z",     // Latest transaction timestamp
    "filterCriteria": {                           // Applied filters (if any)
      "transactionTypes": ["WorkOrderUpdate", "InventoryReceipt"],
      "userIds": ["john.doe", "jane.smith"],
      "entityTypes": ["WorkOrder", "InventoryTransaction"]
    }
  },
  "transactions": [
    {
      "transactionId": "a7f3c9d2-8b4e-4a5f-9c1b-3e8d7f2a6b4c",
      "userId": "john.doe",
      "transactionType": "WorkOrderUpdate",
      "timestamp": "2025-10-08T14:35:22Z",
      "entityType": "WorkOrder",
      "entityId": "WO-2025-001",
      "entityData": {                              // Parsed JSON object (not escaped string)
        "workOrderId": "WO-2025-001",
        "status": "InProgress",
        "completionPercentage": 45,
        "notes": "Machining stage completed"
      },
      "visualReferenceData": {                     // Parsed JSON object (not escaped string)
        "partDescription": "Steel Bracket Assembly",
        "dueDate": "2025-10-15",
        "originalStatus": "Open"
      }
    },
    {
      "transactionId": "b8g4d0e3-9c5f-5b6g-0d2c-4f9e8g3b7c5d",
      "userId": "jane.smith",
      "transactionType": "InventoryReceipt",
      "timestamp": "2025-10-08T15:20:10Z",
      "entityType": "InventoryTransaction",
      "entityId": "INV-2025-123",
      "entityData": {
        "partId": "12345-ABC",
        "quantity": 100,
        "transactionType": "Receipt",
        "referenceDocument": "PO-2025-456",
        "unitOfMeasure": "EA"
      },
      "visualReferenceData": {
        "onHandQuantity": 150,
        "unitOfMeasure": "EA",
        "location": "WH-A-01"
      }
    }
  ]
}
```

**JSON Schema Validation Rules**:
- `exportMetadata.timestamp`: Must be ISO8601 UTC format
- `exportMetadata.transactionCount`: Must match `transactions` array length
- `transactions[].transactionId`: Must be valid GUID format
- `transactions[].transactionType`: Must be valid LocalTransactionType enum value
- `transactions[].entityData`: Schema varies by EntityType (see LocalTransactionRecord.EntityData specification)
- All date/time fields: ISO8601 UTC format (`yyyy-MM-ddTHH:mm:ssZ`)

**Export Task References**:
- **T037**: Implement CSV export for work order updates
- **T043**: Implement CSV export for inventory transactions
- **T045**: Implement CSV export for shipment confirmations

---

## Data Model Completion Checklist

- [x] **Part entity**: Complete with attributes, relationships, validation, cache behavior
- [x] **WorkOrder entity**: Complete with nested types (MaterialRequirement, OperationStep), state transitions
- [x] **InventoryTransaction entity**: Complete with TransactionType enum, validation rules
- [x] **CustomerOrder/Shipment entity**: Complete with OrderLine nested type, ShipmentStatus enum
- [x] **LocalTransactionRecord entity**: Complete with MySQL schema, EntityData structure per type
- [x] **VisualPerformanceMetrics entity**: Complete with performance monitoring attributes
- [x] **Export Formats**: CSV column order and JSON schema for FR-012 transaction exports
- [x] **ERD**: Entity relationships documented with cardinality
- [x] **Read-Only Architecture**: All VISUAL entities marked as read-only, local writes to MAMP MySQL
- [x] **Cache Strategy**: TTL policies (Parts: 24h, Others: 7d), cache keys, eviction strategy
- [x] **Validation Rules**: Specified for all entities where applicable

**Ready for Phase 1 Contracts**: ✅ YES
