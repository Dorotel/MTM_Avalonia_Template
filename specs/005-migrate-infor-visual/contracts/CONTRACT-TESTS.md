# VISUAL API Contract Tests

**Feature**: 005 - Migrate Infor VISUAL ERP Integration
**Date**: 2025-10-08
**Purpose**: Define expected behavior of Infor VISUAL API Toolkit endpoints

## Overview

Contract tests validate that the VISUAL API Toolkit endpoints match expected request/response formats, error handling, and business rules. These tests ensure:

1. **Interface Stability**: API contracts remain consistent across VISUAL versions
2. **Error Handling**: Proper handling of network failures, authentication errors, and data validation errors
3. **Data Format**: Response data matches expected JSON structure and data types
4. **Performance**: API calls meet performance targets (<3s for parts, <2s for work orders, <5s for inventory)

## Test Categories

### 1. Authentication Contracts
### 2. Part Lookup Contracts
### 3. Work Order Query Contracts
### 4. Inventory Transaction Contracts
### 5. Customer Order / Shipment Contracts
### 6. Error Handling Contracts
### 7. Performance Contracts

---

## 1. Authentication Contracts

### CT-AUTH-001: Successful Windows Integrated Authentication

**Endpoint**: `Visual.Connect()` (VISUAL API Toolkit method)

**Preconditions**:
- Windows domain credentials stored in WindowsSecretsService
- VISUAL server accessible on network
- User has read permissions in VISUAL database

**Request**:
```csharp
var credentials = new VisualCredentials
{
    Server = "visual-server.example.com",
    Database = "VISUAL_DB",
    AuthenticationMode = VisualAuthenticationMode.WindowsIntegrated
};

var connection = await _visualApiService.ConnectAsync(credentials, cancellationToken);
```

**Expected Response**:
```csharp
// Connection object with:
connection.IsConnected == true
connection.ServerVersion >= "8.0" (minimum supported VISUAL version)
connection.ConnectionId != null (unique connection identifier)
```

**Expected Duration**: <500ms

**Failure Scenarios**:
- **Network Unreachable**: Throws `VisualConnectionException` with message "Server unreachable"
- **Invalid Credentials**: Throws `VisualAuthenticationException` with message "Authentication failed"
- **Database Not Found**: Throws `VisualDatabaseException` with message "Database 'VISUAL_DB' does not exist"

---

### CT-AUTH-002: Successful Username/Password Authentication

**Endpoint**: `Visual.Connect()` with credentials

**Preconditions**:
- Username/password stored in WindowsSecretsService
- VISUAL server configured for SQL authentication
- User has read permissions in VISUAL database

**Request**:
```csharp
var credentials = new VisualCredentials
{
    Server = "visual-server.example.com",
    Database = "VISUAL_DB",
    AuthenticationMode = VisualAuthenticationMode.UsernamePassword,
    Username = await _secretsService.RetrieveSecretAsync("Visual.Username", cancellationToken),
    Password = await _secretsService.RetrieveSecretAsync("Visual.Password", cancellationToken)
};

var connection = await _visualApiService.ConnectAsync(credentials, cancellationToken);
```

**Expected Response**:
```csharp
connection.IsConnected == true
connection.ServerVersion >= "8.0"
connection.ConnectionId != null
```

**Expected Duration**: <500ms

---

### CT-AUTH-003: Connection Timeout Handling

**Endpoint**: `Visual.Connect()` with unreachable server

**Request**:
```csharp
var credentials = new VisualCredentials
{
    Server = "unreachable-server.invalid",
    Database = "VISUAL_DB",
    Timeout = TimeSpan.FromSeconds(5)
};

var connection = await _visualApiService.ConnectAsync(credentials, cancellationToken);
```

**Expected Behavior**:
- Throws `VisualConnectionException` after 5 seconds
- Exception message: "Connection timeout after 5 seconds"
- Circuit breaker does NOT open (only opens after 5 consecutive failures)

---

## 2. Part Lookup Contracts

### CT-PART-001: Get Part by ID (Successful)

**Endpoint**: `Visual.GetPart(partId)` (VISUAL API Toolkit method)

**Preconditions**:
- Authenticated connection to VISUAL
- Part "12345-ABC" exists in VISUAL database

**Request**:
```csharp
var part = await _visualApiService.GetPartAsync("12345-ABC", cancellationToken);
```

**Expected Response**:
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

**Expected Duration**: <3s (FR-011 performance target)

**Validation Rules**:
- `partId` must match request exactly (case-sensitive)
- `onHandQuantity` and `allocatedQuantity` must be >= 0
- `unitOfMeasure` must be valid VISUAL UoM code
- `specifications` may be empty dictionary if no custom attributes

---

### CT-PART-002: Get Part by ID (Not Found)

**Endpoint**: `Visual.GetPart(partId)` with nonexistent part

**Request**:
```csharp
var part = await _visualApiService.GetPartAsync("NONEXISTENT-PART", cancellationToken);
```

**Expected Behavior**:
- Returns `null` (NOT exception)
- Logs warning: "Part 'NONEXISTENT-PART' not found in VISUAL"
- Does NOT cache null result (avoid caching 404s)

---

### CT-PART-003: Search Parts by Description

**Endpoint**: `Visual.SearchParts(query)` (VISUAL API Toolkit method)

**Request**:
```csharp
var parts = await _visualApiService.SearchPartsAsync("Steel Bracket", cancellationToken);
```

**Expected Response**:
```json
[
  {
    "partId": "12345-ABC",
    "description": "Steel Bracket Assembly",
    "unitOfMeasure": "EA",
    "onHandQuantity": 150.00
  },
  {
    "partId": "12345-DEF",
    "description": "Steel Bracket - Large",
    "unitOfMeasure": "EA",
    "onHandQuantity": 80.00
  }
]
```

**Expected Duration**: <3s for up to 100 results

**Validation Rules**:
- Results ordered by relevance (exact matches first, then partial matches)
- Maximum 100 results returned (pagination required for more)
- Empty array returned if no matches (NOT null)

---

## 3. Work Order Query Contracts

### CT-WO-001: Get Work Order by ID (Successful)

**Endpoint**: `Visual.GetWorkOrder(workOrderId)` (VISUAL API Toolkit method)

**Preconditions**:
- Authenticated connection to VISUAL
- Work order "WO-2024-001" exists with status "InProgress"

**Request**:
```csharp
var workOrder = await _visualApiService.GetWorkOrderAsync("WO-2024-001", cancellationToken);
```

**Expected Response**:
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

**Expected Duration**: <2s (FR-012 performance target)

**Validation Rules**:
- `currentStatus` must be one of: Open, InProgress, Closed
- `requiredMaterials` may be empty array if no materials defined
- `operationSteps` may be empty array if no operations defined
- `issuedQuantity` <= `allocatedQuantity` <= `requiredQuantity`

---

### CT-WO-002: Get Open Work Orders (List)

**Endpoint**: `Visual.GetWorkOrders(filter)` (VISUAL API Toolkit method)

**Request**:
```csharp
var filter = new WorkOrderFilter
{
    Status = WorkOrderStatus.Open,
    DueDateStart = DateTime.Now,
    DueDateEnd = DateTime.Now.AddDays(30)
};

var workOrders = await _visualApiService.GetWorkOrdersAsync(filter, cancellationToken);
```

**Expected Response**:
```json
[
  {
    "workOrderId": "WO-2024-002",
    "partId": "12345-DEF",
    "quantityToProduce": 50.00,
    "dueDate": "2025-10-20T00:00:00Z",
    "currentStatus": "Open"
  },
  {
    "workOrderId": "WO-2024-003",
    "partId": "12345-GHI",
    "quantityToProduce": 75.00,
    "dueDate": "2025-10-25T00:00:00Z",
    "currentStatus": "Open"
  }
]
```

**Expected Duration**: <2s for up to 500 work orders

**Validation Rules**:
- Results filtered by status and date range (server-side filtering)
- Results ordered by due date ascending
- Maximum 500 results returned (pagination required for more)

---

## 4. Inventory Transaction Contracts

### CT-INV-001: Get Inventory Transaction History

**Endpoint**: `Visual.GetInventoryTransactions(partId)` (VISUAL API Toolkit method)

**Request**:
```csharp
var transactions = await _visualApiService.GetInventoryTransactionsAsync("12345-ABC", cancellationToken);
```

**Expected Response**:
```json
[
  {
    "transactionId": "TXN-001",
    "transactionType": "Receipt",
    "partId": "12345-ABC",
    "quantity": 100.00,
    "unitOfMeasure": "EA",
    "referenceDocument": "PO-2024-050",
    "transactionDate": "2025-10-01T08:00:00Z",
    "createdBy": "receiving_user"
  },
  {
    "transactionId": "TXN-002",
    "transactionType": "Issue",
    "partId": "12345-ABC",
    "quantity": -50.00,
    "unitOfMeasure": "EA",
    "referenceDocument": "WO-2024-001",
    "transactionDate": "2025-10-05T14:30:00Z",
    "createdBy": "production_user"
  }
]
```

**Expected Duration**: <5s for up to 1000 transactions (FR-013 performance target)

**Validation Rules**:
- Transactions ordered by transaction date descending (most recent first)
- `quantity` is positive for receipts, negative for issues
- `referenceDocument` is required for Receipt and Issue types
- Maximum 1000 transactions returned (pagination required for more)

---

## 5. Customer Order / Shipment Contracts

### CT-ORDER-001: Get Customer Order by ID

**Endpoint**: `Visual.GetCustomerOrder(orderId)` (VISUAL API Toolkit method)

**Request**:
```csharp
var order = await _visualApiService.GetCustomerOrderAsync("SO-2024-100", cancellationToken);
```

**Expected Response**:
```json
{
  "orderId": "SO-2024-100",
  "customerName": "ABC Manufacturing Inc.",
  "orderDate": "2025-09-15T00:00:00Z",
  "shipDate": "2025-10-08T00:00:00Z",
  "trackingNumber": "1Z999AA10123456784",
  "carrier": "UPS",
  "orderLines": [
    {
      "lineNumber": 1,
      "partId": "12345-ABC",
      "quantityOrdered": 100.00,
      "quantityShipped": 50.00,
      "unitPrice": 25.50
    }
  ],
  "shipmentStatus": "PartiallyShipped",
  "lastUpdated": "2025-10-08T14:30:00Z"
}
```

**Expected Duration**: <3s

**Validation Rules**:
- `shipmentStatus` must be one of: Open, PartiallyShipped, Shipped, Invoiced
- `quantityShipped` <= `quantityOrdered` for each line
- `trackingNumber` and `carrier` required if status = Shipped or PartiallyShipped

---

## 6. Error Handling Contracts

### CT-ERROR-001: Network Timeout

**Scenario**: VISUAL API call exceeds timeout

**Request**:
```csharp
// Configure short timeout for testing
_visualApiService.Timeout = TimeSpan.FromSeconds(1);

var part = await _visualApiService.GetPartAsync("12345-ABC", cancellationToken);
```

**Expected Behavior**:
- Throws `TimeoutException` after 1 second
- Circuit breaker increments failure count (5 consecutive = open circuit)
- Falls back to cached data if available
- Logs error: "VISUAL API timeout after 1000ms for operation GetPart(12345-ABC)"

---

### CT-ERROR-002: Authentication Failure

**Scenario**: Invalid credentials provided

**Request**:
```csharp
var credentials = new VisualCredentials
{
    Server = "visual-server.example.com",
    Database = "VISUAL_DB",
    AuthenticationMode = VisualAuthenticationMode.UsernamePassword,
    Username = "invalid_user",
    Password = "wrong_password"
};

var connection = await _visualApiService.ConnectAsync(credentials, cancellationToken);
```

**Expected Behavior**:
- Throws `VisualAuthenticationException` with message "Authentication failed for user 'invalid_user'"
- Does NOT increment circuit breaker failure count (authentication errors are not transient)
- Logs error with security event level
- Shows user-friendly dialog: "Unable to connect to VISUAL. Please check your credentials."

---

### CT-ERROR-003: Circuit Breaker Opens After 5 Failures

**Scenario**: 5 consecutive VISUAL API failures

**Request**:
```csharp
// Simulate 5 consecutive failures (network timeouts)
for (int i = 1; i <= 5; i++)
{
    try
    {
        await _visualApiService.GetPartAsync("12345-ABC", cancellationToken);
    }
    catch (TimeoutException)
    {
        // Expected
    }
}

// Circuit breaker should now be OPEN
var circuitState = _visualApiService.GetCircuitBreakerState();
```

**Expected Behavior**:
- After 5th failure: Circuit breaker transitions to OPEN state
- Subsequent API calls fail immediately with `CircuitBreakerOpenException` (no network call)
- Application displays status bar warning: "VISUAL server unavailable. Using cached data only."
- DebugTerminalWindow shows "Connection Status: Yellow (Degraded Mode)"
- Circuit breaker automatically retries after 30 seconds (FR-020)

---

## 7. Performance Contracts

### CT-PERF-001: Part Lookup Performance Target

**Endpoint**: `Visual.GetPart(partId)` under load

**Test Scenario**:
- 100 concurrent part lookups
- Mix of cached and uncached parts (50/50)
- Network latency: 50ms (simulated)

**Expected Results**:
- **P50 (median)**: <1s
- **P95**: <3s (FR-011 compliance)
- **P99**: <5s
- **Cache hit rate**: >80% (after warm-up)

---

### CT-PERF-002: Work Order Query Performance Target

**Endpoint**: `Visual.GetWorkOrders(filter)` with 500 results

**Test Scenario**:
- Query for 500 open work orders
- Network latency: 50ms (simulated)

**Expected Results**:
- **Median query time**: <2s (FR-012 compliance)
- **95th percentile**: <3s
- **Cache TTL**: 7 days (work orders change infrequently)

---

### CT-PERF-003: Inventory Transaction History Performance Target

**Endpoint**: `Visual.GetInventoryTransactions(partId)` with 1000 results

**Test Scenario**:
- Query for 1000 historical transactions
- Network latency: 50ms (simulated)

**Expected Results**:
- **Median query time**: <5s (FR-013 compliance)
- **95th percentile**: <7s
- **Result pagination**: 1000 transactions per page (server-side limit)

---

## Test Implementation Plan

### Phase 1: Core Contract Tests (P1)

- [ ] CT-AUTH-001: Windows Integrated Authentication
- [ ] CT-AUTH-002: Username/Password Authentication
- [ ] CT-PART-001: Get Part by ID (Successful)
- [ ] CT-PART-002: Get Part by ID (Not Found)
- [ ] CT-WO-001: Get Work Order by ID (Successful)
- [ ] CT-ERROR-001: Network Timeout
- [ ] CT-ERROR-003: Circuit Breaker Opens After 5 Failures

### Phase 2: Advanced Contract Tests (P2)

- [ ] CT-AUTH-003: Connection Timeout Handling
- [ ] CT-PART-003: Search Parts by Description
- [ ] CT-WO-002: Get Open Work Orders (List)
- [ ] CT-INV-001: Get Inventory Transaction History
- [ ] CT-ORDER-001: Get Customer Order by ID
- [ ] CT-ERROR-002: Authentication Failure

### Phase 3: Performance Contract Tests (P3)

- [ ] CT-PERF-001: Part Lookup Performance Target
- [ ] CT-PERF-002: Work Order Query Performance Target
- [ ] CT-PERF-003: Inventory Transaction History Performance Target

---

## Contract Test Execution

### Prerequisites

1. VISUAL API Toolkit installed (`C:\Program Files (x86)\Infor\VISUAL API Toolkit\Visual.dll`)
2. VISUAL server accessible on network (or VPN)
3. Test credentials configured in WindowsSecretsService
4. MAMP MySQL running with `mtm_template_dev` database

### Run Contract Tests

```powershell
# Run all contract tests
dotnet test --filter "Category=Contract"

# Run specific contract test
dotnet test --filter "FullyQualifiedName~CT_AUTH_001"

# Run with detailed output
dotnet test --filter "Category=Contract" --logger "console;verbosity=detailed"
```

### Contract Test Output Format

```
Passed CT-AUTH-001: Successful Windows Integrated Authentication (Duration: 324ms)
Passed CT-PART-001: Get Part by ID (Duration: 1,234ms, Cache: MISS)
Failed CT-PERF-001: Part Lookup Performance Target
  Expected: P95 < 3000ms
  Actual: P95 = 4,567ms
  Recommendation: Increase cache TTL or optimize VISUAL query
```

---

**Contract Test Completion Checklist**:

- [ ] All Phase 1 contract tests passing (7 tests)
- [ ] All Phase 2 contract tests passing (6 tests)
- [ ] All Phase 3 performance tests passing (3 tests)
- [ ] Contract tests integrated into CI/CD pipeline
- [ ] Contract test documentation updated with actual VISUAL API behavior

**Ready for Implementation**: ✅ YES (once contracts defined - implementation validates against these contracts)
