# API Contracts: MAMP MySQL 5.7

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Data Source**: MAMP MySQL 5.7 (Secondary, Backup Data)  
**Access Pattern**: Desktop direct MySQL connection, Android via API proxy  
**Date**: 2025-10-02

## Overview

This contract defines the data access patterns for MAMP MySQL 5.7 server, which serves as a secondary backup data source between Visual ERP (primary) and local cache (tertiary). MAMP contains master data synced from Visual but may be less current. Desktop platforms connect via MySQL client; Android platforms connect via HTTP API proxy.

---

## MAMP Architecture

**Purpose**: Intermediate backup data source when Visual ERP is unavailable  
**Data Freshness**: Near-live (periodic sync from Visual, may lag by hours)  
**Availability**: Depends on MAMP server uptime + network connectivity  
**Performance**: 500ms-2s latency (LAN or local network)

**Sync Mechanism**: External process synchronizes Visual ERP → MAMP MySQL periodically (not managed by this application)

---

## Desktop Access Pattern (Direct MySQL Connection)

### Connection Configuration

**Service**: `IMampDataService` implemented by `MampDataService` (Desktop platform)  
**Technology**: MySQL.Data NuGet package  
**Connection String**: Loaded from `ServiceConfiguration.MampConnectionString`

Example connection string:
```
Server=192.168.1.50;Port=3306;Database=mamp_manufacturing;Uid=readonly_user;Pwd=***REDACTED***;
```

**Security**:
- ✅ Credentials stored in secure local storage (Stage 2: Secrets) - SR-005
- ✅ Read-only database user (no write/admin privileges) - SR-005
- ✅ Connection uses TLS if MAMP server supports it - SR-004

### 1. Reachability Check Query

**Purpose**: Health check during boot Stage 5 (Reachability)

```sql
SELECT 1 AS health_check;
```

**Expected Result**:
```
+---------------+
| health_check  |
+---------------+
|             1 |
+---------------+
1 row in set (0.05 sec)
```

**Contract Assertions**:
- ✅ Query completes within 10-second timeout
- ✅ Returns single row with value `1`
- ✅ Connection failure triggers cascade to local cache
- ✅ Timeout triggers cascade to local cache

### 2. Get Last Sync Timestamp Query

**Purpose**: Retrieve when MAMP data was last synchronized from Visual ERP

```sql
SELECT sync_timestamp, sync_status
FROM mamp_sync_log
ORDER BY sync_timestamp DESC
LIMIT 1;
```

**Expected Result**:
```
+---------------------+-------------+
| sync_timestamp      | sync_status |
+---------------------+-------------+
| 2025-10-02 14:30:00 | SUCCESS     |
+---------------------+-------------+
1 row in set (0.08 sec)
```

**Contract Assertions**:
- ✅ Returns most recent sync timestamp
- ✅ `sync_status` is either 'SUCCESS' or 'FAILURE'
- ✅ If sync older than 24 hours, display warning to operator (FR-022)
- ✅ If no rows returned, treat MAMP as unavailable

**Data Model**:
```csharp
public record MampSyncLog
{
    public DateTime SyncTimestamp { get; init; }
    public string SyncStatus { get; init; } // "SUCCESS" or "FAILURE"
}
```

### 3. Get Items Master Data Query

**Purpose**: Prefetch Items master data during boot Stage 8 (Caching)

```sql
SELECT 
    item_id AS ItemId,
    part_number AS PartNumber,
    description AS Description,
    category AS Category,
    unit_of_measure AS UnitOfMeasure,
    is_active AS IsActive,
    last_updated AS LastUpdated
FROM mamp_items
WHERE is_active = 1
ORDER BY item_id
LIMIT 5000;
```

**Expected Result**:
```
+---------+-------------+------------------+------------------+-----------------+-----------+---------------------+
| ItemId  | PartNumber  | Description      | Category         | UnitOfMeasure   | IsActive  | LastUpdated         |
+---------+-------------+------------------+------------------+-----------------+-----------+---------------------+
| 1001    | ABC-12345   | Widget Assembly  | Finished Goods   | EA              | 1         | 2025-10-02 14:30:00 |
+---------+-------------+------------------+------------------+-----------------+-----------+---------------------+
```

**Contract Assertions**:
- ✅ Query returns all active items (is_active = 1)
- ✅ Returns maximum 5000 rows (prevent memory overflow)
- ✅ All columns mapped to `ItemData` C# model
- ✅ Query completes within 30-second cache prefetch timeout
- ✅ Empty result set (0 rows) triggers cascade to local cache

**Data Model**:
```csharp
public record ItemData
{
    public required int ItemId { get; init; }
    public required string PartNumber { get; init; }
    public required string Description { get; init; }
    public string? Category { get; init; }
    public required string UnitOfMeasure { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime LastUpdated { get; init; }
}
```

### 4. Get Locations Master Data Query

**Purpose**: Prefetch Locations master data during boot Stage 8 (Caching)

```sql
SELECT 
    location_id AS LocationId,
    location_code AS LocationCode,
    description AS Description,
    warehouse AS Warehouse,
    is_active AS IsActive,
    last_updated AS LastUpdated
FROM mamp_locations
WHERE is_active = 1
ORDER BY location_id
LIMIT 5000;
```

**Data Model**:
```csharp
public record LocationData
{
    public required int LocationId { get; init; }
    public required string LocationCode { get; init; }
    public required string Description { get; init; }
    public string? Warehouse { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime LastUpdated { get; init; }
}
```

### 5. Get WorkCenters Master Data Query

**Purpose**: Prefetch WorkCenters master data during boot Stage 8 (Caching)

```sql
SELECT 
    workcenter_id AS WorkCenterId,
    workcenter_code AS WorkCenterCode,
    description AS Description,
    department AS Department,
    is_active AS IsActive,
    last_updated AS LastUpdated
FROM mamp_workcenters
WHERE is_active = 1
ORDER BY workcenter_id
LIMIT 5000;
```

**Data Model**:
```csharp
public record WorkCenterData
{
    public required int WorkCenterId { get; init; }
    public required string WorkCenterCode { get; init; }
    public required string Description { get; init; }
    public string? Department { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime LastUpdated { get; init; }
}
```

---

## Android Access Pattern (API Proxy)

### API Proxy Configuration

**Service**: `IMampDataService` implemented by `MampApiProxyService` (Android platform)  
**Technology**: HttpClient (REST API)  
**Base URL**: Loaded from `ServiceConfiguration.MampApiProxyEndpoint`

Example endpoint:
```
https://api.manufacturing.com/mamp-proxy/
```

**Security**:
- ✅ HTTPS required (TLS 1.2+) - SR-004
- ✅ MAMP credentials stay server-side (not in Android app) - SR-005
- ✅ API uses same authentication as Visual API (Bearer token)

### 1. Proxy Health Check Endpoint

**Purpose**: Reachability check during boot Stage 5 (Reachability)

```http
HEAD /mamp-proxy/health
Host: {MampApiProxyEndpoint}
Authorization: Bearer {AccessToken}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Length: 0
```

**Contract Assertions**:
- ✅ Returns 200 OK if MAMP server reachable from proxy
- ✅ Returns 503 Service Unavailable if MAMP server unreachable
- ✅ Completes within 10-second timeout

### 2. Get Last Sync Timestamp Endpoint

```http
GET /mamp-proxy/sync-status
Host: {MampApiProxyEndpoint}
Authorization: Bearer {AccessToken}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "syncTimestamp": "2025-10-02T14:30:00Z",
  "syncStatus": "SUCCESS"
}
```

**Response Schema**:
```json
{
  "syncTimestamp": "datetime (required, ISO 8601)",
  "syncStatus": "string (required, values: 'SUCCESS' or 'FAILURE')"
}
```

### 3. Get Items Endpoint

```http
GET /mamp-proxy/items?activeOnly=true&maxResults=5000
Host: {MampApiProxyEndpoint}
Authorization: Bearer {AccessToken}
```

**Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "itemId": 1001,
      "partNumber": "ABC-12345",
      "description": "Widget Assembly",
      "category": "Finished Goods",
      "unitOfMeasure": "EA",
      "isActive": true,
      "lastUpdated": "2025-10-02T14:30:00Z"
    }
  ],
  "totalCount": 1
}
```

**Response Schema**:
```json
{
  "items": [
    {
      "itemId": "integer (required)",
      "partNumber": "string (required)",
      "description": "string (required)",
      "category": "string (optional)",
      "unitOfMeasure": "string (required)",
      "isActive": "boolean (required)",
      "lastUpdated": "datetime (required, ISO 8601)"
    }
  ],
  "totalCount": "integer (required)"
}
```

### 4. Get Locations Endpoint

```http
GET /mamp-proxy/locations?activeOnly=true&maxResults=5000
Host: {MampApiProxyEndpoint}
Authorization: Bearer {AccessToken}
```

**Response Schema**: Same structure as Items, with `locations` array

### 5. Get WorkCenters Endpoint

```http
GET /mamp-proxy/workcenters?activeOnly=true&maxResults=5000
Host: {MampApiProxyEndpoint}
Authorization: Bearer {AccessToken}
```

**Response Schema**: Same structure as Items, with `workCenters` array

---

## Error Handling

### Desktop MySQL Errors

**Connection Timeout**:
```csharp
catch (MySqlException ex) when (ex.Number == 0) // Connection timeout
{
    // Cascade to local cache
    _logger.LogWarning("MAMP connection timeout after 10 seconds");
    return DataSourceResult.Unavailable("Connection timeout");
}
```

**Authentication Failure**:
```csharp
catch (MySqlException ex) when (ex.Number == 1045) // Access denied
{
    // Fail-fast - invalid MAMP credentials
    _logger.LogError("MAMP authentication failed: invalid credentials");
    return DataSourceResult.Failed("Invalid MAMP credentials");
}
```

**Empty Result Set**:
```csharp
if (items.Count == 0)
{
    _logger.LogWarning("MAMP returned empty result set for Items");
    // Cascade to local cache
    return DataSourceResult.Unavailable("Empty result set");
}
```

### Android API Proxy Errors

**503 Service Unavailable** (MAMP server unreachable):
```json
{
  "error": "service_unavailable",
  "errorDescription": "MAMP server is currently unreachable from proxy."
}
```

**500 Internal Server Error** (Proxy error):
```json
{
  "error": "internal_error",
  "errorDescription": "An unexpected error occurred in the MAMP proxy."
}
```

**Timeout**:
```csharp
using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
cts.CancelAfter(TimeSpan.FromSeconds(10));

try
{
    var response = await _httpClient.GetAsync("/mamp-proxy/health", cts.Token);
}
catch (TaskCanceledException)
{
    // Cascade to local cache
    return DataSourceResult.Unavailable("Timeout after 10 seconds");
}
```

---

## Fallback Cascade Strategy

```
1. Visual API Toolkit unavailable
   ↓
2. Check MAMP reachability (10s timeout)
   ↓
   IF MAMP reachable AND returns data
     → Use MAMP as active data source
     → Display warning banner: "Backup data (last synced: {timestamp})"
     → If MAMP data > 24 hours old, additional warning (FR-022)
   ↓
   IF MAMP unreachable OR returns empty result set
     → Cascade to local cache
```

---

## Data Validation

**Schema Mismatch**:
```csharp
// Validate required fields exist in query result
if (string.IsNullOrEmpty(reader.GetString("PartNumber")))
{
    _logger.LogError("MAMP schema mismatch: PartNumber column missing or null");
    return DataSourceResult.Unavailable("Schema validation failed");
}
```

**Corrupt Records**:
```csharp
// Validate data integrity
if (reader.GetInt32("ItemId") <= 0)
{
    _logger.LogWarning("MAMP corrupt record: ItemId <= 0, skipping record");
    continue; // Skip invalid record, continue with valid ones
}
```

---

## Performance Requirements

- ✅ Reachability check completes within 10 seconds - PR-003
- ✅ Master data queries complete within 30 seconds - PR-004
- ✅ LIMIT 5000 rows per query to prevent memory overflow
- ✅ Desktop MySQL direct connection: 500ms-2s latency
- ✅ Android API proxy: Add 200-500ms overhead for HTTP roundtrip

---

## Contract Test Implementation

Contract tests will be created in `tests/MTM_Template_Application.Tests.Contract/MampApiContractTests.cs` using xUnit and FluentAssertions. Tests will validate:

1. **Desktop MySQL Access**:
   - Connection string format validation
   - Health check query execution
   - Master data query result mapping
   - Timeout enforcement
   - Empty result set handling

2. **Android API Proxy**:
   - HTTP endpoint availability
   - Request/response schema validation
   - Error response structure
   - Timeout enforcement

**Test Execution**: Tests will fail initially (no implementation yet) following TDD workflow.

---

**Next Contract**: Local cache access patterns (LiteDB document storage) for tertiary fallback.
