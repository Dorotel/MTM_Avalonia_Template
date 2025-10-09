# Reference: Visual ERP API Integration Scope

**Date**: October 8, 2025

**Purpose**: Define Visual ERP integration requirements, API scope, and data contracts

---

## Integration Overview

**Visual ERP**: Manufacturing ERP system by Infor

**Integration Type**: Read-only API integration for parts, work orders, and inventory data

**Primary Use Case**: Mobile-first inventory management with offline-first operation

**User Persona**: Shop floor workers, inventory clerks, production managers

---

## API Architecture

### Base Configuration

- **Base URL**: Configurable via `Visual:BaseUrl` setting
- **Authentication**: API Key stored in OS-native secrets service
- **Transport**: HTTPS only (no HTTP fallback)
- **Timeout**: Configurable via `Visual:Timeout` setting (default: 30s)
- **Retry Policy**: 3 retries with exponential backoff
- **Connection Pool**: Max 5 concurrent connections

### Client Structure

```
MTM_Template_Application/Services/DataLayer/Visual/
├── IVisualApiClient.cs              # Main client interface
├── VisualApiClient.cs               # Implementation
├── Models/                          # DTOs for API responses
│   ├── VisualItem.cs
│   ├── VisualWorkOrder.cs
│   └── VisualInventoryEntry.cs
├── Contracts/                       # Request/response contracts
└── Exceptions/                      # Custom exceptions
```

---

## Data Entities

### 1. Items (Parts/Products)

**Purpose**: Product catalog data (part numbers, descriptions, locations)

**API Endpoint**: `GET /api/v1/items`

**Data Contract**:

```csharp
public record VisualItem
{
    public string ItemId { get; init; }              // Part number (primary key)
    public string Description { get; init; }         // Part description
    public string? LongDescription { get; init; }    // Extended description
    public string? Location { get; init; }           // Storage location
    public string UnitOfMeasure { get; init; }       // EA, FT, LB, etc.
    public decimal UnitCost { get; init; }           // Standard unit cost
    public decimal QuantityOnHand { get; init; }     // Current inventory
    public decimal QuantityAvailable { get; init; }  // Available for allocation
    public decimal QuantityAllocated { get; init; }  // Allocated to orders
    public decimal ReorderPoint { get; init; }       // Reorder threshold
    public decimal ReorderQuantity { get; init; }    // Standard reorder qty
    public string? VendorPartNumber { get; init; }   // Vendor's part number
    public string? Barcode { get; init; }            // Barcode/UPC
    public DateTime? LastReceived { get; init; }     // Last receipt date
    public DateTime? LastIssued { get; init; }       // Last issue date
    public bool IsActive { get; init; }              // Active flag
    public DateTime LastUpdated { get; init; }       // Last update timestamp
}
```

**Query Parameters**:

- `search` (string): Search by part number or description
- `location` (string): Filter by storage location
- `isActive` (bool): Filter by active status
- `page` (int): Page number for pagination
- `pageSize` (int): Items per page (default: 100)

**Caching Strategy**:

- Cache duration: 24 hours (configurable via `Visual:CacheExpiryHours`)
- Cache key: `visual:items:{itemId}` or `visual:items:search:{query}`
- Invalidation: Manual refresh or TTL expiry

---

### 2. Work Orders

**Purpose**: Production work order data (jobs, operations, status)

**API Endpoint**: `GET /api/v1/workorders`

**Data Contract**:

```csharp
public record VisualWorkOrder
{
    public string WorkOrderId { get; init; }         // WO number (primary key)
    public string ItemId { get; init; }              // Part being produced
    public string? ItemDescription { get; init; }    // Part description
    public decimal QuantityOrdered { get; init; }    // Total quantity ordered
    public decimal QuantityCompleted { get; init; }  // Quantity completed
    public decimal QuantityRemaining { get; init; }  // Quantity remaining
    public string Status { get; init; }              // Open, Released, Complete, Closed
    public DateTime? StartDate { get; init; }        // Scheduled start date
    public DateTime? DueDate { get; init; }          // Due date
    public DateTime? CompletionDate { get; init; }   // Actual completion date
    public string? Location { get; init; }           // Production location
    public string? Notes { get; init; }              // Work order notes
    public List<WorkOrderOperation> Operations { get; init; } // Operations list
    public DateTime LastUpdated { get; init; }       // Last update timestamp
}

public record WorkOrderOperation
{
    public int SequenceNumber { get; init; }         // Operation sequence
    public string OperationCode { get; init; }       // Operation ID
    public string Description { get; init; }         // Operation description
    public string Status { get; init; }              // Pending, InProgress, Complete
    public decimal StandardHours { get; init; }      // Standard labor hours
    public decimal ActualHours { get; init; }        // Actual labor hours
    public DateTime? StartedAt { get; init; }        // Operation start time
    public DateTime? CompletedAt { get; init; }      // Operation completion time
}
```

**Query Parameters**:

- `status` (string): Filter by status (Open, Released, Complete, Closed)
- `itemId` (string): Filter by part number
- `startDate` (DateTime): Filter by start date range
- `dueDate` (DateTime): Filter by due date range
- `page` (int): Page number for pagination
- `pageSize` (int): Items per page (default: 50)

**Caching Strategy**:

- Cache duration: 6 hours (work orders change frequently)
- Cache key: `visual:workorders:{workOrderId}`
- Invalidation: Manual refresh or TTL expiry

---

### 3. Inventory Transactions

**Purpose**: Inventory movement history (receipts, issues, adjustments)

**API Endpoint**: `GET /api/v1/inventory/transactions`

**Data Contract**:

```csharp
public record VisualInventoryTransaction
{
    public int TransactionId { get; init; }          // Transaction ID (primary key)
    public string ItemId { get; init; }              // Part number
    public string TransactionType { get; init; }     // Receipt, Issue, Adjustment, Transfer
    public decimal Quantity { get; init; }           // Transaction quantity (+ or -)
    public string? FromLocation { get; init; }       // Source location (for transfers)
    public string? ToLocation { get; init; }         // Destination location
    public string? ReferenceNumber { get; init; }    // PO, WO, or adjustment number
    public string? Notes { get; init; }              // Transaction notes
    public string? UserId { get; init; }             // User who created transaction
    public DateTime TransactionDate { get; init; }   // Transaction timestamp
    public decimal UnitCost { get; init; }           // Unit cost at time of transaction
    public decimal ExtendedCost { get; init; }       // Total cost (qty * unit cost)
}
```

**Query Parameters**:

- `itemId` (string): Filter by part number
- `transactionType` (string): Filter by type
- `startDate` (DateTime): Start of date range
- `endDate` (DateTime): End of date range
- `location` (string): Filter by location
- `page` (int): Page number for pagination
- `pageSize` (int): Items per page (default: 100)

**Caching Strategy**:

- Cache duration: 1 hour (history data, less volatile)
- Cache key: `visual:inventory:transactions:{query}`
- Invalidation: Manual refresh or TTL expiry

---

## Barcode Scanning Integration

### Barcode Scanner Configuration

- **Supported Formats**: Code 39, Code 128, UPC-A, UPC-E, EAN-13, EAN-8, QR Code
- **Scan Timeout**: Configurable via `Visual:BarcodeTimeout` (default: 10s)
- **Continuous Scan**: Disabled (single scan per action)
- **Audio Feedback**: Beep on successful scan

### Scan Workflows

#### 1. Item Lookup by Barcode

```
User scans barcode
→ Extract item ID from barcode
→ Query Visual API: GET /api/v1/items/{itemId}
→ Display item details
→ Show quantity on hand, location, etc.
```

#### 2. Work Order Lookup by Barcode

```
User scans work order barcode
→ Extract work order ID
→ Query Visual API: GET /api/v1/workorders/{workOrderId}
→ Display work order details
→ Show status, operations, quantities
```

#### 3. Inventory Transaction Creation

```
User scans item barcode
→ Extract item ID
→ User enters quantity and transaction type
→ Create transaction in Visual
→ POST /api/v1/inventory/transactions
→ Update local cache
```

---

## Offline Mode Operation

### Offline Data Requirements

- **Items**: Last 7 days of accessed items cached
- **Work Orders**: Last 30 days of accessed work orders cached
- **Inventory Transactions**: Last 90 days of history cached

### Offline Capabilities

| Feature                  | Offline Support | Notes                                |
| ------------------------ | --------------- | ------------------------------------ |
| Item Lookup              | Yes             | From local cache                     |
| Work Order Lookup        | Yes             | From local cache                     |
| Inventory History        | Yes             | From local cache (90 days)           |
| Create Transaction       | Queue           | Queued for sync when online          |
| Update Work Order Status | Queue           | Queued for sync when online          |
| Barcode Scanning         | Yes             | Works offline with cached data       |

### Offline Sync Queue

```csharp
public record OfflineSyncQueueItem
{
    public Guid ItemId { get; init; }                // Queue item ID
    public string ApiEndpoint { get; init; }         // Target API endpoint
    public string HttpMethod { get; init; }          // POST, PUT, PATCH
    public string RequestBody { get; init; }         // JSON request body
    public DateTime QueuedAt { get; init; }          // Timestamp queued
    public int RetryCount { get; init; }             // Retry attempts
    public string Status { get; init; }              // Pending, Syncing, Failed, Completed
}
```

**Sync Strategy**:

- Auto-sync when network available
- Manual sync button in UI
- Show sync status indicator (pending count)
- Conflict resolution: Last-write-wins (user confirmation)

---

## Error Handling

### API Error Codes

| HTTP Status | Error Type          | Handling Strategy                      |
| ----------- | ------------------- | -------------------------------------- |
| 200         | Success             | Parse response, cache data             |
| 400         | Bad Request         | Log error, show user-friendly message  |
| 401         | Unauthorized        | Prompt credential re-entry             |
| 403         | Forbidden           | Log error, show permission denied      |
| 404         | Not Found           | Show "Item not found" message          |
| 429         | Rate Limit Exceeded | Backoff and retry after delay          |
| 500         | Server Error        | Retry with exponential backoff         |
| 503         | Service Unavailable | Activate offline mode, use cache       |

### Retry Policy (Polly)

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                "Retry {RetryCount} after {Delay}s due to {Exception}",
                retryCount, timespan.TotalSeconds, exception.Message
            );
        }
    );
```

---

## Visual API Mock Service

**Purpose**: Allow development/testing without Visual ERP access

**Configuration**: `Visual:UseMockData = true`

**Mock Data Sources**:

- JSON files in `MTM_Template_Application/MockData/Visual/`
- Generates realistic test data with randomization
- Simulates API latency (configurable delay)

**Mock Endpoints**:

```
MTM_Template_Application/Services/DataLayer/Visual/Mock/
├── MockVisualApiClient.cs           # Mock client implementation
├── MockData/
│   ├── items.json                   # Sample items
│   ├── workorders.json              # Sample work orders
│   └── transactions.json            # Sample transactions
```

---

## Performance Requirements

| Operation                | Target  | Measurement                      |
| ------------------------ | ------- | -------------------------------- |
| Item lookup (single)     | <500ms  | API call + cache write           |
| Item search (paginated)  | <1s     | API call + cache write           |
| Work order lookup        | <750ms  | API call + cache write           |
| Inventory transaction    | <1s     | Create transaction + cache write |
| Barcode scan → display   | <2s     | Scan + API call + UI render      |
| Offline mode activation  | <100ms  | Switch to cache-only mode        |
| Sync queue processing    | <5s     | Process 10 queued items          |

---

## Testing Requirements

### Unit Tests

- [ ] VisualApiClient: Test all methods with mocked HTTP responses
- [ ] Data contract serialization/deserialization
- [ ] Retry policy behavior (success after 2 retries)
- [ ] Error handling for all HTTP status codes
- [ ] Offline sync queue operations

### Integration Tests

- [ ] End-to-end API calls against mock service
- [ ] Cache persistence (LZ4 compression)
- [ ] Offline mode activation/deactivation
- [ ] Sync queue processing

### Contract Tests

- [ ] Validate API response schemas match contracts
- [ ] Test API versioning compatibility
- [ ] Test pagination behavior
- [ ] Test query parameter filtering

**Coverage Target**: 80%+ per Constitution Principle IV

---

## Security & Compliance

### Data Protection

- **Credentials**: Stored in OS-native secrets service (ISecretsService)
- **API Keys**: Never logged or cached
- **Connection Strings**: HTTPS only, no HTTP fallback
- **User Data**: PII filtered from logs

### Compliance

- **GDPR**: User can request data deletion (clear cache)
- **CCPA**: User can request data export (export diagnostics)
- **SOC 2**: Audit logs for all API calls

---

## Visual Integration Roadmap

### Phase 1: Read-Only Item Lookup (Feature 005)

- [ ] Implement `IVisualApiClient` interface
- [ ] Implement item lookup by ID
- [ ] Implement item search with pagination
- [ ] Implement barcode scanning for items
- [ ] Implement offline caching with LZ4 compression
- [ ] Implement mock service for testing

### Phase 2: Work Order Integration (Future)

- [ ] Implement work order lookup by ID
- [ ] Implement work order search with filters
- [ ] Implement work order status updates
- [ ] Implement offline sync queue

### Phase 3: Inventory Transactions (Future)

- [ ] Implement inventory transaction history
- [ ] Implement transaction creation (receipts, issues)
- [ ] Implement transaction queuing for offline
- [ ] Implement conflict resolution

### Phase 4: Advanced Features (Future)

- [ ] Real-time sync with WebSockets
- [ ] Push notifications for work order changes
- [ ] Advanced barcode scanning (batch scan)
- [ ] Inventory cycle counting workflows

---

## Visual API Documentation References

- **Internal Docs**: `docs/InforVisualToolkitIntegration-SpecReady.md`
- **Data Contracts**: `docs/DATA-CONTRACTS.md`
- **Visual Whitelist**: `docs/VISUAL-WHITELIST.md` (allowed API operations)
- **Visual Credentials Flow**: `docs/VISUAL-CREDENTIALS-FLOW.md`

---

## Implementation Notes

- Use `HttpClientFactory` for HTTP client management
- Use Polly for retry policies and circuit breakers
- Use LZ4 compression for cached API responses
- Use structured logging for all API calls
- Use feature flags to control Visual integration features
