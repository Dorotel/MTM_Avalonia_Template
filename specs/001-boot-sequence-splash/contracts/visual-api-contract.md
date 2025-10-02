# Visual API Toolkit Integration Contract

## Overview

This contract defines the integration between MTM Template Application and InforVisual Manufacturing ERP via the **Visual API Toolkit**. The API Toolkit is a **.NET library** (not HTTP REST) that provides programmatic access to Visual database through business objects and data access classes.

**Critical Note**: Since the API Toolkit is a Windows .NET Framework library (x86 only), the MTM application uses an **HTTP wrapper service** running on a Desktop host. The wrapper service references the API Toolkit DLLs and exposes HTTP REST endpoints that the MTM application (Desktop + Android) can call.

## Architecture

```
MTM Application (Desktop/Android)
    ↓ HTTP REST calls
Desktop Visual Service (ASP.NET Core on Desktop host)
    ↓ API Toolkit .NET method calls
Visual API Toolkit DLLs (LsaCore.dll, VmfgInventory.dll, etc.)
    ↓ ADO.NET database connection
Visual ERP Database (SQL Server or Oracle)
```

### Visual API Toolkit Components

| Component | DLL | Purpose |
|-----------|-----|---------|
| Core Data Access | LsaCore.dll | Lsa.Data.Dbms class, database connection management |
| Shared Utilities | LsaShared.dll | Lsa.Shared namespace, GetSingleSignOnData for SSO |
| Inventory Module | VmfgInventory.dll | Part, Location business objects (Items master data) |
| Shop Floor Module | VmfgShopFloor.dll | WorkCenter business objects |
| Sales Module | VmfgSales.dll | Customer, Order business objects |
| Purchasing Module | VmfgPurchasing.dll | Vendor, PO business objects |
| Financials Module | VmfgFinancials.dll | AR, AP business objects |

### Database.Config File

The API Toolkit requires a `Database.Config` XML file in the same directory as the executable (Desktop Visual Service). This file contains Visual database connection information:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Databases>
  <Database InstanceName="_default" 
            ServerName="VISUAL_SERVER" 
            DatabaseName="VISUAL_DB" 
            LoginUser="SYSADM" 
            LoginPassword="[ENCRYPTED_BY_API_TOOLKIT]" 
            Provider="SqlServer" />
</Databases>
```

**Fields**:
- `InstanceName`: Logical name for this connection (typically "_default")
- `ServerName`: SQL Server or Oracle server hostname/IP
- `DatabaseName`: Visual database name
- `LoginUser`: Visual user account (typically "SYSADM" for service accounts)
- `LoginPassword`: Encrypted password (encrypted by API Toolkit, not plaintext)
- `Provider`: "SqlServer" or "Oracle"

## API Toolkit Access Patterns

### Connection Management

**Dbms.OpenLocal Method**:
```csharp
bool success = Lsa.Data.Dbms.OpenLocal(
    instanceName: "_default",
    loginUser: credentials.Username,
    loginPassword: credentials.Password
);
```

- Returns `true` if credentials valid and connection successful
- Returns `false` if credentials invalid or Visual database unreachable
- Reads `Database.Config` file for server/database connection info
- Validates user credentials against Visual USER table
- Maintains single connection per instance name

**Connection Lifecycle**:
1. Desktop Visual Service starts → loads Database.Config
2. MTM app sends authentication request → Desktop Service calls Dbms.OpenLocal
3. API Toolkit opens ADO.NET connection to Visual database
4. Connection remains open for subsequent data requests
5. Desktop Service shutdown → Dbms.CloseAll()

### Business Object Patterns

**Documents** (header + subordinates):
```csharp
var customerOrder = new VmfgSales.CustomerOrder("_default");
bool loaded = customerOrder.Load(orderId); // Load by primary key
DataTable header = customerOrder.Tables["CUSTOMER_ORDER"];
DataTable lines = customerOrder.Tables["CUST_ORDER_LINE"];
// Read-only: never call customerOrder.Save()
```

**Collections** (multiple rows):
```csharp
var part = new VmfgInventory.Part("_default");
DataTable items = part.Browse(filter: "PART_ID LIKE 'A%'", maxResults: 1000);
foreach (DataRow row in items.Rows)
{
    string partId = row.Field<string>("PART_ID");
    string description = row.Field<string>("DESCRIPTION");
}
```

**Services** (read-only queries):
```csharp
var inventoryService = new VmfgInventory.InventoryBalanceService("_default");
inventoryService.Prepare(); // Returns empty DataSet
DataRow paramRow = inventoryService.NewRow();
paramRow["PART_ID"] = "12345";
paramRow["LOCATION_ID"] = "WH1";
inventoryService.Execute(); // Runs query, populates result DataSet
DataTable balance = inventoryService.Tables["RESULT"];
```

### Read-Only Constraint

**FR-010 Requirement**: All Visual access must be read-only. Never call mutation methods:
- ❌ Document.Save()
- ❌ Transaction.Save()
- ❌ Dbms.GetNextNumberAndAdvance()
- ✅ Document.Load()
- ✅ Collection.Browse()
- ✅ Service.Execute()

## HTTP Wrapper Service Contract

The Desktop Visual Service exposes HTTP REST endpoints that wrap API Toolkit calls. This section defines the HTTP contract between MTM application and Desktop Visual Service.

### Base URL

```
http://localhost:5050/api/visual
```

**Production Configuration**:
- Desktop: `http://localhost:5050` (loopback only for security)
- Android: `http://{DESKTOP_HOST_IP}:5050` (replace with Desktop machine's LAN IP)

### Authentication

All endpoints except `/health` require authentication token in header:
```
Authorization: Bearer {JWT_TOKEN}
```

Token obtained via `/auth/token` endpoint (see below).

---

## Endpoint 1: Health Check

**Purpose**: Verify Desktop Visual Service is reachable and Visual database connection can be established.

### Request

```http
HEAD /api/visual/health HTTP/1.1
Host: localhost:5050
```

**Method**: HEAD (no response body)  
**Timeout**: 10 seconds  
**No authentication required** (public health check)

### Response

**Success** (Visual database reachable):
```http
HTTP/1.1 204 No Content
X-Visual-Instance: _default
X-Visual-Server: VISUAL_SERVER
X-Visual-Database: VISUAL_DB
```

**Failure** (Visual database unreachable):
```http
HTTP/1.1 503 Service Unavailable
X-Visual-Error: Database connection failed
```

### Boot Sequence Behavior

- **204**: Visual available, proceed with authentication (Stage 7)
- **503**: Visual unavailable, **cascade to MAMP** immediately (no retry)
- **Timeout**: Visual unreachable, **cascade to MAMP** immediately

---

## Endpoint 2: Authentication

**Purpose**: Validate operator credentials against Visual USER table and obtain JWT access token.

### Request

```http
POST /api/visual/auth/token HTTP/1.1
Host: localhost:5050
Content-Type: application/json

{
  "username": "OPERATOR1",
  "password": "password123"
}
```

**Timeout**: 10 seconds

### Response

**Success** (credentials valid):
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "userId": "OPERATOR1",
    "userName": "John Smith",
    "site": "MAIN",
    "department": "ASSEMBLY"
  }
}
```

**Failure** (credentials invalid):
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "error": "Invalid credentials",
  "errorCode": "INVALID_CREDENTIALS"
}
```

**Failure** (Visual database unreachable):
```http
HTTP/1.1 503 Service Unavailable
Content-Type: application/json

{
  "error": "Visual database connection failed",
  "errorCode": "DATABASE_UNAVAILABLE"
}
```

### Implementation (Desktop Visual Service)

```csharp
[HttpPost("auth/token")]
public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
{
    try
    {
        // Call API Toolkit authentication
        bool success = await Task.Run(() => 
            Lsa.Data.Dbms.OpenLocal("_default", request.Username, request.Password)
        );
        
        if (!success)
        {
            return Unauthorized(new { error = "Invalid credentials", errorCode = "INVALID_CREDENTIALS" });
        }
        
        // Generate JWT token
        string token = GenerateJwtToken(request.Username);
        
        // Load user details from Visual USER table
        var userData = await GetUserDetailsAsync(request.Username);
        
        return Ok(new AuthResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = userData
        });
    }
    catch (Exception ex) when (ex.Message.Contains("database"))
    {
        return StatusCode(503, new { error = "Visual database connection failed", errorCode = "DATABASE_UNAVAILABLE" });
    }
}
```

### Boot Sequence Behavior

- **200**: Authentication successful, proceed with master data prefetch (Stage 8)
- **401**: Invalid credentials, **fail-fast** (show credential error, no cascade)
- **503**: Visual database unavailable, **cascade to MAMP**
- **Timeout**: Visual unreachable, **cascade to MAMP**

---

## Endpoint 3: Get Items (Parts Master Data)

**Purpose**: Prefetch Items master data for offline operations.

### Request

```http
GET /api/visual/master-data/items?filter=PART_ID+LIKE+%27A%25%27&maxResults=1000&offset=0 HTTP/1.1
Host: localhost:5050
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters**:
- `filter` (optional): SQL WHERE clause (e.g., "PART_ID LIKE 'A%'")
- `maxResults` (optional): Max records to return (default: 1000, max: 5000)
- `offset` (optional): Pagination offset (default: 0)

**Timeout**: 30 seconds

### Response

**Success**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "partId": "ABC-123",
      "description": "Widget Assembly",
      "uom": "EA",
      "partType": "M",
      "productLine": "WIDGETS",
      "stdCost": 125.50,
      "avgCost": 128.00,
      "lastCost": 130.00,
      "onHand": 150.0,
      "available": 120.0,
      "allocated": 30.0,
      "onOrder": 200.0,
      "leadTimeDays": 5,
      "safetyStock": 50.0,
      "reorderPoint": 75.0,
      "reorderQty": 100.0,
      "abcCode": "A",
      "makeOrBuy": "M",
      "phantom": false,
      "inactive": false,
      "pictureFile": "ABC-123.jpg",
      "commodityCode": "WIDGET",
      "drawingNumber": "DWG-123",
      "revision": "C"
    }
  ],
  "totalCount": 1543,
  "hasMore": true,
  "offset": 0,
  "maxResults": 1000
}
```

### Implementation (Desktop Visual Service)

```csharp
[HttpGet("master-data/items")]
[Authorize]
public async Task<IActionResult> GetItems(
    [FromQuery] string? filter = null,
    [FromQuery] int maxResults = 1000,
    [FromQuery] int offset = 0)
{
    maxResults = Math.Min(maxResults, 5000); // Cap at 5000
    
    var part = new VmfgInventory.Part("_default");
    DataTable items = await Task.Run(() => part.Browse(filter, maxResults + offset));
    
    // Convert DataTable → DTO
    var itemDtos = items.AsEnumerable()
        .Skip(offset)
        .Take(maxResults)
        .Select(row => new ItemDto
        {
            PartId = row.Field<string>("PART_ID"),
            Description = row.Field<string>("DESCRIPTION"),
            Uom = row.Field<string>("U_OF_M"),
            PartType = row.Field<string>("PART_TYPE"),
            ProductLine = row.Field<string>("PRODUCT_LINE"),
            // ... map remaining fields
        })
        .ToList();
    
    return Ok(new
    {
        items = itemDtos,
        totalCount = items.Rows.Count,
        hasMore = items.Rows.Count > (offset + maxResults),
        offset,
        maxResults
    });
}
```

### Boot Sequence Behavior

- **200**: Store items in cache, proceed to Stage 9 (Locations prefetch)
- **401/403**: Authentication expired, **fail-fast**
- **404/500**: Visual error, **cascade to MAMP**
- **Timeout**: Visual slow/unreachable, **cascade to MAMP**

---

## Endpoint 4: Get Locations

**Purpose**: Prefetch Locations master data for offline operations.

### Request

```http
GET /api/visual/master-data/locations?filter=SITE_ID+%3D+%27MAIN%27&maxResults=1000&offset=0 HTTP/1.1
Host: localhost:5050
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters**: Same as Endpoint 3

**Timeout**: 30 seconds

### Response

**Success**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "locations": [
    {
      "locationId": "WH1-A-01",
      "description": "Warehouse 1 - Aisle A - Bin 01",
      "siteId": "MAIN",
      "locationType": "STORAGE",
      "warehouseId": "WH1",
      "aisle": "A",
      "bin": "01",
      "pickSequence": 1,
      "capacity": 1000.0,
      "capacityUom": "CF",
      "allowNegative": false,
      "inactive": false
    }
  ],
  "totalCount": 245,
  "hasMore": false,
  "offset": 0,
  "maxResults": 1000
}
```

### Implementation

```csharp
[HttpGet("master-data/locations")]
[Authorize]
public async Task<IActionResult> GetLocations(
    [FromQuery] string? filter = null,
    [FromQuery] int maxResults = 1000,
    [FromQuery] int offset = 0)
{
    // Use appropriate VmfgInventory business object
    // Exact class name TBD (Location, LocationCollection, or GeneralQuery)
    var location = new VmfgInventory.Location("_default"); // Or equivalent
    DataTable locations = await Task.Run(() => location.Browse(filter, maxResults + offset));
    
    // Convert DataTable → DTO
    // ... similar to GetItems
}
```

**Note**: Exact Visual API Toolkit class for Locations needs verification. May be `VmfgInventory.Location` or accessed via `GeneralQuery` if no dedicated business object exists.

### Boot Sequence Behavior

Same as Endpoint 3.

---

## Endpoint 5: Get Work Centers

**Purpose**: Prefetch WorkCenters master data for offline operations.

### Request

```http
GET /api/visual/master-data/workcenters?filter=SITE_ID+%3D+%27MAIN%27&maxResults=1000&offset=0 HTTP/1.1
Host: localhost:5050
Authorization: Bearer {JWT_TOKEN}
```

**Query Parameters**: Same as Endpoint 3

**Timeout**: 30 seconds

### Response

**Success**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "workCenters": [
    {
      "workCenterId": "WC-ASSY-01",
      "description": "Assembly Line 01",
      "siteId": "MAIN",
      "department": "ASSEMBLY",
      "laborRate": 45.00,
      "overheadRate": 35.00,
      "machineRate": 0.00,
      "capacity": 160.0,
      "capacityUom": "HR",
      "efficiency": 95.0,
      "utilization": 85.0,
      "queueTime": 2.0,
      "setupTime": 0.5,
      "inactive": false
    }
  ],
  "totalCount": 32,
  "hasMore": false,
  "offset": 0,
  "maxResults": 1000
}
```

### Implementation

```csharp
[HttpGet("master-data/workcenters")]
[Authorize]
public async Task<IActionResult> GetWorkCenters(
    [FromQuery] string? filter = null,
    [FromQuery] int maxResults = 1000,
    [FromQuery] int offset = 0)
{
    // Use VmfgShopFloor business object
    var workCenter = new VmfgShopFloor.WorkCenter("_default"); // Or equivalent
    DataTable workCenters = await Task.Run(() => workCenter.Browse(filter, maxResults + offset));
    
    // Convert DataTable → DTO
    // ... similar to GetItems
}
```

**Note**: Exact Visual API Toolkit class for WorkCenters needs verification from VmfgShopFloor.dll.

### Boot Sequence Behavior

Same as Endpoint 3.

---

## Endpoint 6: Get Maintenance Windows

**Purpose**: Check if Visual is currently in maintenance mode.

### Request

```http
GET /api/visual/system/maintenance-windows HTTP/1.1
Host: localhost:5050
Authorization: Bearer {JWT_TOKEN}
```

**Timeout**: 5 seconds

### Response

**Success**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "currentlyInMaintenance": false,
  "nextMaintenanceWindow": {
    "startTime": "2024-01-15T02:00:00Z",
    "endTime": "2024-01-15T04:00:00Z",
    "reason": "Monthly database backup"
  },
  "scheduledWindows": [
    {
      "startTime": "2024-01-15T02:00:00Z",
      "endTime": "2024-01-15T04:00:00Z",
      "reason": "Monthly database backup"
    }
  ]
}
```

### Implementation

```csharp
[HttpGet("system/maintenance-windows")]
[Authorize]
public async Task<IActionResult> GetMaintenanceWindows()
{
    // Query Visual MESSAGE_MAINTENANCE or custom maintenance schedule table
    // Use GeneralQuery if no dedicated business object exists
    
    var query = new Lsa.Data.GeneralQuery("_default");
    query.Query = @"
        SELECT TOP 1 
            CASE WHEN GETDATE() BETWEEN START_TIME AND END_TIME THEN 1 ELSE 0 END AS CURRENTLY_IN_MAINTENANCE,
            START_TIME,
            END_TIME,
            REASON
        FROM MAINTENANCE_SCHEDULE
        WHERE END_TIME > GETDATE()
        ORDER BY START_TIME";
    
    DataTable result = await Task.Run(() => query.Execute());
    
    // Parse result and return DTO
    // ...
}
```

**Note**: Visual may not have a dedicated maintenance schedule table. This endpoint may need to be implemented differently based on actual Visual schema.

### Boot Sequence Behavior

- **200 with currentlyInMaintenance=true**: Show maintenance banner, cascade to MAMP
- **200 with currentlyInMaintenance=false**: Proceed normally
- **404**: Visual doesn't track maintenance windows, ignore
- **Timeout**: Visual slow, proceed normally (not critical)

---

## Error Handling

### HTTP Status Code Behavior

| Status | Meaning | Boot Behavior |
|--------|---------|---------------|
| 200 | Success | Proceed to next stage |
| 401 | Invalid credentials | **Fail-fast** (show credential error) |
| 403 | Authorization error | **Fail-fast** (insufficient permissions) |
| 404 | Resource not found | **Cascade to MAMP** (Visual error) |
| 429 | Rate limited | **Cascade to MAMP** (Visual overloaded) |
| 500 | Internal server error | **Cascade to MAMP** (Visual error) |
| 503 | Service unavailable | **Cascade to MAMP** (Visual down) |
| Timeout | No response within timeout | **Cascade to MAMP** (Visual unreachable) |

### Logging

**Never log**:
- Passwords
- JWT tokens
- Visual database connection strings

**Always log**:
- Request durations
- HTTP status codes
- Error messages (sanitized)
- Cascade events

---

## Performance Requirements

| Metric | Target | Source |
|--------|--------|--------|
| Health check | < 2 seconds | PR-003 |
| Authentication | < 3 seconds | PR-003 |
| Master data prefetch (single entity) | < 10 seconds | PR-003 |
| Total Visual initialization | < 25 seconds (all 3 entities) | PR-003 |
| Timeout per endpoint | 10s (health/auth), 30s (prefetch) | Research Decision 3 |

---

## Security Requirements

### Transport Security

- **Desktop**: HTTP over loopback (localhost) acceptable for development
- **Production**: HTTPS required with valid certificate
- **Android**: HTTPS required when calling Desktop host over network

### Credential Security

- **Storage**: Never store Visual credentials in MTM application
- **Transmission**: HTTPS only for credential payloads
- **Logging**: Never log passwords or tokens
- **Token lifetime**: JWT tokens expire after 1 hour, require re-authentication

### Network Security

- **Desktop Visual Service**: Bind to `localhost:5050` for Desktop-only access
- **Android access**: Configure firewall to allow Android device IP addresses on port 5050
- **Production**: Use reverse proxy (nginx, IIS) with HTTPS termination

---

## Testing Strategy

### Contract Tests (VisualApiContractTests.cs)

**Mocking Strategy**: Mock API Toolkit calls (Dbms.OpenLocal, Part.Browse) with test data.

```csharp
[Fact]
public async Task HealthCheck_WhenVisualAvailable_Returns204()
{
    // Arrange: Mock Dbms.InstanceIsOpen to return true
    // Act: HEAD /api/visual/health
    // Assert: Status 204, headers present
}

[Fact]
public async Task Authenticate_WithValidCredentials_Returns200WithToken()
{
    // Arrange: Mock Dbms.OpenLocal to return true
    // Act: POST /api/visual/auth/token with test credentials
    // Assert: Status 200, token present, user data populated
}

[Fact]
public async Task Authenticate_WithInvalidCredentials_Returns401()
{
    // Arrange: Mock Dbms.OpenLocal to return false
    // Act: POST /api/visual/auth/token with invalid credentials
    // Assert: Status 401, error code INVALID_CREDENTIALS
}

[Fact]
public async Task GetItems_WithValidToken_ReturnsItemsArray()
{
    // Arrange: Mock VmfgInventory.Part.Browse to return test DataTable
    // Act: GET /api/visual/master-data/items
    // Assert: Status 200, items array populated, pagination fields correct
}

[Fact]
public async Task GetItems_WithFilterAndPagination_RespectsParameters()
{
    // Arrange: Mock Part.Browse with filter
    // Act: GET /api/visual/master-data/items?filter=PART_ID LIKE 'A%'&maxResults=100
    // Assert: Status 200, maxResults=100, filter applied
}

[Fact]
public async Task GetItems_WhenVisualUnavailable_Returns503()
{
    // Arrange: Mock Part.Browse to throw database exception
    // Act: GET /api/visual/master-data/items
    // Assert: Status 503, error code DATABASE_UNAVAILABLE
}
```

### Integration Tests (VisualApiIntegrationTests.cs)

**Environment**: Requires Visual API Toolkit DLLs and test Database.Config pointing to test Visual database.

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task RealVisualConnection_HealthCheck_Succeeds()
{
    // Arrange: Real Desktop Visual Service with test Database.Config
    // Act: HEAD /api/visual/health
    // Assert: Status 204 (or 503 if test Visual unavailable)
}

[Fact]
[Trait("Category", "Integration")]
public async Task RealVisualConnection_Authenticate_WithTestCredentials_Succeeds()
{
    // Arrange: Real Visual Service, test credentials from appsettings.Test.json
    // Act: POST /api/visual/auth/token
    // Assert: Status 200, token parseable as JWT
}
```

---

## Visual Database Schema Mapping

### Items (Parts) - PART Table

| Visual Column | DTO Property | Type | Notes |
|---------------|--------------|------|-------|
| PART_ID | partId | string | Primary key, max 30 chars |
| DESCRIPTION | description | string | Max 80 chars |
| U_OF_M | uom | string | Unit of measure (EA, LB, FT, etc.) |
| PART_TYPE | partType | string | M=Manufactured, P=Purchased, S=Subcontract |
| PRODUCT_LINE | productLine | string | Product line code |
| STD_COST | stdCost | decimal? | Standard cost (nullable) |
| AVG_COST | avgCost | decimal? | Average cost (nullable) |
| LAST_COST | lastCost | decimal? | Last purchase cost (nullable) |
| ON_HAND_QTY | onHand | decimal | Current on-hand quantity |
| AVAILABLE_QTY | available | decimal | Available quantity (on-hand - allocated) |
| ALLOC_QTY | allocated | decimal | Allocated to orders |
| ON_ORDER_QTY | onOrder | decimal | On purchase orders |
| LEAD_TIME_DAYS | leadTimeDays | int | Lead time in days |
| SAFETY_STOCK | safetyStock | decimal? | Safety stock level |
| REORDER_POINT | reorderPoint | decimal? | Reorder point |
| REORDER_QTY | reorderQty | decimal? | Reorder quantity |
| ABC_CODE | abcCode | string | A/B/C inventory classification |
| MAKE_OR_BUY | makeOrBuy | string | M=Make, B=Buy |
| PHANTOM | phantom | bool | Phantom BOM component |
| INACTIVE | inactive | bool | Inactive flag |
| PICTURE_FILE | pictureFile | string? | Image filename |
| COMMODITY_CODE | commodityCode | string? | Commodity classification |
| DRAWING_NUMBER | drawingNumber | string? | Engineering drawing number |
| REVISION | revision | string? | Revision level |

### Locations - Table Name TBD

**Schema Incomplete**: Visual Data Table.csv read incomplete (2,000 of 14,776 rows). Location table not yet found. Likely table names: `LOCATION`, `PART_LOCATION`, `WAREHOUSE_LOCATION`.

**Estimated Fields** (based on typical ERP schema):
- LOCATION_ID (PK)
- DESCRIPTION
- SITE_ID
- LOCATION_TYPE (STORAGE, WIP, SCRAP, etc.)
- WAREHOUSE_ID
- AISLE
- BIN
- CAPACITY
- CAPACITY_UOM
- ALLOW_NEGATIVE
- INACTIVE

### WorkCenters - Table Name TBD

**Schema Incomplete**: Work center table not yet found in CSV. Likely table names: `WORK_CENTER`, `RESOURCE`, `WORKCENTER`.

**Estimated Fields**:
- WORK_CENTER_ID (PK)
- DESCRIPTION
- SITE_ID
- DEPARTMENT
- LABOR_RATE
- OVERHEAD_RATE
- MACHINE_RATE
- CAPACITY
- CAPACITY_UOM (HR, MIN, etc.)
- EFFICIENCY (%)
- UTILIZATION (%)
- QUEUE_TIME
- SETUP_TIME
- INACTIVE

**TODO**: Complete Visual Data Table.csv read (remaining 12,776 rows) to find exact table/column names for Locations and WorkCenters.

---

## Dependencies

### Desktop Visual Service Requirements

- **.NET Framework 4.6.2+** or **.NET 6.0+** (for ASP.NET Core)
- **Visual API Toolkit DLLs**: LsaCore.dll, LsaShared.dll, VmfgInventory.dll, VmfgShopFloor.dll (x86 builds)
- **Database.Config**: XML file with Visual connection info
- **SQL Server drivers** or **Oracle.ManagedDataAccess** (depending on Visual database backend)
- **Network access**: Desktop host must reach Visual database server

### MTM Application Requirements

- **HttpClient**: For calling Desktop Visual Service endpoints
- **System.Text.Json** or **Newtonsoft.Json**: For JSON deserialization
- **Network access**: Desktop/Android must reach Desktop Visual Service (http://localhost:5050 or http://{DESKTOP_IP}:5050)

---

## Appendix: API Toolkit Version

**Visual API Toolkit Core Library Version**: 8.1.0.0  
**Visual ERP Version**: 9.0.8 (July 2022)  
**Supported .NET Framework**: 4.0, 4.5, 4.6, 4.7, 4.8  
**Supported Databases**: SQL Server, Oracle  
**Platform**: Windows x86 (32-bit) only

---

## Appendix: Example Database.Config

```xml
<?xml version="1.0" encoding="utf-8"?>
<Databases>
  <!-- Production Visual -->
  <Database InstanceName="_default" 
            ServerName="VISPROD01" 
            DatabaseName="VISUAL" 
            LoginUser="SYSADM" 
            LoginPassword="JDFjc2RhZGZhc2Rm" 
            Provider="SqlServer" />
  
  <!-- Test Visual -->
  <Database InstanceName="TEST" 
            ServerName="VISTEST01" 
            DatabaseName="VISUAL_TEST" 
            LoginUser="TESTUSER" 
            LoginPassword="YXNkZmFzZGZhc2Rm" 
            Provider="SqlServer" />
</Databases>
```

**Note**: LoginPassword is encrypted by Visual API Toolkit. Do not store plaintext passwords. Use Visual Configuration Manager tool to encrypt passwords.

**Feature**: Boot Sequence - Splash-First Services Initialization  
**Data Source**: Visual ERP via API Toolkit (Primary, Live Data)  
**Access Pattern**: Read-only API commands, never direct SQL  
**Date**: 2025-10-02

## Overview

This contract defines the HTTP REST API interactions with the Visual API Toolkit for master data prefetch operations during boot Stage 8 (Caching). All Visual ERP access must go through these API endpoints; direct SQL access is prohibited (FR-010).

---

## Base Configuration

**Base URL**: Configured in `ServiceConfiguration.VisualApiEndpoint`  
**Protocol**: HTTPS (TLS required for security - SR-004)  
**Authentication**: Credentials loaded from secure local storage (Stage 2: Secrets)  
**Timeout**: 10 seconds per request (configurable via `ServiceConfiguration.ReachabilityTimeout`)

---

## 1. Health Check Endpoint

### Request

```http
HEAD /api/health
Host: {VisualApiEndpoint}
Authorization: Bearer {AccessToken}
```

**Purpose**: Reachability check during boot Stage 5 (Reachability)  
**Method**: `HEAD` (minimal bandwidth)  
**Headers**:
- `Authorization`: Bearer token from credential validation

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Length: 0
```

**Unauthorized (401)**:
```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer error="invalid_token"
```

**Service Unavailable (503)**:
```http
HTTP/1.1 503 Service Unavailable
Retry-After: 300
```

### Contract Test Assertions

- ✅ `HEAD /api/health` with valid credentials returns 200 OK
- ✅ `HEAD /api/health` with invalid credentials returns 401 Unauthorized
- ✅ `HEAD /api/health` completes within 10-second timeout
- ✅ Health check endpoint does not require body payload

---

## 2. Authenticate Endpoint

### Request

```http
POST /api/auth/token
Host: {VisualApiEndpoint}
Content-Type: application/json

{
  "username": "operator",
  "password": "***REDACTED***",
  "clientId": "MTM_Template_Application"
}
```

**Purpose**: Validate Visual credentials during boot Stage 2 (Secrets)  
**Method**: `POST`  
**Headers**:
- `Content-Type`: `application/json`

**Request Schema**:
```json
{
  "username": "string (required, email format)",
  "password": "string (required, min 8 chars)",
  "clientId": "string (required)"
}
```

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "refreshToken": "refresh_token_value",
  "user": {
    "userId": "12345",
    "username": "operator@manufacturing.com",
    "displayName": "John Operator",
    "roles": ["Operator", "Viewer"]
  }
}
```

**Response Schema**:
```json
{
  "accessToken": "string (required, JWT format)",
  "tokenType": "string (required, value: 'Bearer')",
  "expiresIn": "integer (required, seconds)",
  "refreshToken": "string (optional)",
  "user": {
    "userId": "string (required)",
    "username": "string (required)",
    "displayName": "string (required)",
    "roles": "array of strings (required)"
  }
}
```

**Invalid Credentials (401)**:
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "error": "invalid_credentials",
  "errorDescription": "The username or password is incorrect."
}
```

**Error Schema**:
```json
{
  "error": "string (required, error code)",
  "errorDescription": "string (required, human-readable message)"
}
```

### Contract Test Assertions

- ✅ `POST /api/auth/token` with valid credentials returns 200 OK with access token
- ✅ Response contains all required fields (`accessToken`, `tokenType`, `expiresIn`, `user`)
- ✅ `accessToken` is valid JWT format
- ✅ `tokenType` is "Bearer"
- ✅ `POST /api/auth/token` with invalid credentials returns 401 Unauthorized
- ✅ 401 response contains `error` and `errorDescription` fields
- ✅ Request fails fast if credentials are invalid (no retry cascade - SR-002)

---

## 3. Get Items Endpoint

### Request

```http
GET /api/master-data/items?filter=Active&maxResults=1000
Host: {VisualApiEndpoint}
Authorization: Bearer {AccessToken}
```

**Purpose**: Prefetch Items master data during boot Stage 8 (Caching)  
**Method**: `GET`  
**Headers**:
- `Authorization`: Bearer token from authentication

**Query Parameters**:
- `filter` (optional): Filter expression (e.g., "Active", "Category=Raw", "PartID like 'ABC%'")
- `maxResults` (optional): Maximum records to return (default: 1000, max: 5000)
- `offset` (optional): Pagination offset (default: 0)

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [
    {
      "itemId": "ITEM-001",
      "partNumber": "ABC-12345",
      "description": "Widget Assembly",
      "category": "Finished Goods",
      "unitOfMeasure": "EA",
      "isActive": true,
      "lastUpdated": "2025-10-02T14:30:00Z"
    }
  ],
  "totalCount": 1,
  "offset": 0,
  "hasMore": false
}
```

**Response Schema**:
```json
{
  "items": [
    {
      "itemId": "string (required)",
      "partNumber": "string (required)",
      "description": "string (required)",
      "category": "string (optional)",
      "unitOfMeasure": "string (required)",
      "isActive": "boolean (required)",
      "lastUpdated": "datetime (required, ISO 8601)"
    }
  ],
  "totalCount": "integer (required)",
  "offset": "integer (required)",
  "hasMore": "boolean (required)"
}
```

**No Data (200 OK with empty array)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "items": [],
  "totalCount": 0,
  "offset": 0,
  "hasMore": false
}
```

**Unauthorized (401)**:
```http
HTTP/1.1 401 Unauthorized
Content-Type: application/json

{
  "error": "unauthorized",
  "errorDescription": "Access token is invalid or expired."
}
```

### Contract Test Assertions

- ✅ `GET /api/master-data/items` with valid token returns 200 OK
- ✅ Response `items` array contains objects with all required fields
- ✅ `totalCount` matches number of items in array when not paginated
- ✅ `hasMore` is false when all items returned
- ✅ `GET /api/master-data/items` with invalid token returns 401 Unauthorized
- ✅ Empty result set returns empty array (not null)
- ✅ Request completes within 30-second cache prefetch timeout

---

## 4. Get Locations Endpoint

### Request

```http
GET /api/master-data/locations?filter=Active&maxResults=1000
Host: {VisualApiEndpoint}
Authorization: Bearer {AccessToken}
```

**Purpose**: Prefetch Locations master data during boot Stage 8 (Caching)  
**Method**: `GET`  
**Headers**:
- `Authorization`: Bearer token from authentication

**Query Parameters**:
- `filter` (optional): Filter expression (e.g., "Active", "Warehouse='Main'")
- `maxResults` (optional): Maximum records to return (default: 1000, max: 5000)
- `offset` (optional): Pagination offset (default: 0)

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "locations": [
    {
      "locationId": "LOC-001",
      "locationCode": "WH-A-01-A",
      "description": "Warehouse A, Aisle 1, Bin A",
      "warehouse": "Main",
      "isActive": true,
      "lastUpdated": "2025-10-02T14:30:00Z"
    }
  ],
  "totalCount": 1,
  "offset": 0,
  "hasMore": false
}
```

**Response Schema**:
```json
{
  "locations": [
    {
      "locationId": "string (required)",
      "locationCode": "string (required)",
      "description": "string (required)",
      "warehouse": "string (optional)",
      "isActive": "boolean (required)",
      "lastUpdated": "datetime (required, ISO 8601)"
    }
  ],
  "totalCount": "integer (required)",
  "offset": "integer (required)",
  "hasMore": "boolean (required)"
}
```

### Contract Test Assertions

- ✅ `GET /api/master-data/locations` with valid token returns 200 OK
- ✅ Response `locations` array contains objects with all required fields
- ✅ `totalCount` matches number of locations in array when not paginated
- ✅ Request completes within 30-second cache prefetch timeout

---

## 5. Get WorkCenters Endpoint

### Request

```http
GET /api/master-data/workcenters?filter=Active&maxResults=1000
Host: {VisualApiEndpoint}
Authorization: Bearer {AccessToken}
```

**Purpose**: Prefetch WorkCenters master data during boot Stage 8 (Caching)  
**Method**: `GET`  
**Headers**:
- `Authorization`: Bearer token from authentication

**Query Parameters**:
- `filter` (optional): Filter expression (e.g., "Active", "Department='Assembly'")
- `maxResults` (optional): Maximum records to return (default: 1000, max: 5000)
- `offset` (optional): Pagination offset (default: 0)

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "workCenters": [
    {
      "workCenterId": "WC-001",
      "workCenterCode": "ASSY-01",
      "description": "Assembly Line 1",
      "department": "Assembly",
      "isActive": true,
      "lastUpdated": "2025-10-02T14:30:00Z"
    }
  ],
  "totalCount": 1,
  "offset": 0,
  "hasMore": false
}
```

**Response Schema**:
```json
{
  "workCenters": [
    {
      "workCenterId": "string (required)",
      "workCenterCode": "string (required)",
      "description": "string (required)",
      "department": "string (optional)",
      "isActive": "boolean (required)",
      "lastUpdated": "datetime (required, ISO 8601)"
    }
  ],
  "totalCount": "integer (required)",
  "offset": "integer (required)",
  "hasMore": "boolean (required)"
}
```

### Contract Test Assertions

- ✅ `GET /api/master-data/workcenters` with valid token returns 200 OK
- ✅ Response `workCenters` array contains objects with all required fields
- ✅ Request completes within 30-second cache prefetch timeout

---

## 6. Get Maintenance Windows Endpoint

### Request

```http
GET /api/system/maintenance-windows
Host: {VisualApiEndpoint}
Authorization: Bearer {AccessToken}
```

**Purpose**: Check if Visual ERP is in maintenance mode before attempting data operations  
**Method**: `GET`  
**Headers**:
- `Authorization`: Bearer token from authentication

### Response

**Success (200 OK)**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "maintenanceWindows": [
    {
      "startTime": "2025-10-03T02:00:00Z",
      "endTime": "2025-10-03T04:00:00Z",
      "reason": "Database backup and optimization"
    }
  ],
  "currentlyInMaintenance": false
}
```

**Response Schema**:
```json
{
  "maintenanceWindows": [
    {
      "startTime": "datetime (required, ISO 8601)",
      "endTime": "datetime (required, ISO 8601)",
      "reason": "string (optional)"
    }
  ],
  "currentlyInMaintenance": "boolean (required)"
}
```

### Contract Test Assertions

- ✅ `GET /api/system/maintenance-windows` returns 200 OK
- ✅ Response contains `currentlyInMaintenance` boolean
- ✅ If `currentlyInMaintenance` = true, `maintenanceWindows` array contains at least one active window

---

## Error Handling

### Common Error Responses

**400 Bad Request** (Invalid query parameters):
```json
{
  "error": "bad_request",
  "errorDescription": "Invalid filter expression: 'Category=='",
  "details": {
    "parameter": "filter",
    "value": "Category=="
  }
}
```

**401 Unauthorized** (Invalid or expired token):
```json
{
  "error": "unauthorized",
  "errorDescription": "Access token is invalid or expired."
}
```

**403 Forbidden** (Insufficient permissions):
```json
{
  "error": "forbidden",
  "errorDescription": "User does not have permission to access master data."
}
```

**404 Not Found** (Endpoint doesn't exist):
```json
{
  "error": "not_found",
  "errorDescription": "The requested resource was not found."
}
```

**429 Too Many Requests** (Rate limiting):
```json
{
  "error": "rate_limit_exceeded",
  "errorDescription": "Too many requests. Please retry after 60 seconds.",
  "retryAfter": 60
}
```

**500 Internal Server Error** (Visual ERP error):
```json
{
  "error": "internal_error",
  "errorDescription": "An unexpected error occurred. Please contact support.",
  "correlationId": "abc-123-def-456"
}
```

**503 Service Unavailable** (Visual ERP down or in maintenance):
```json
{
  "error": "service_unavailable",
  "errorDescription": "Visual ERP is currently unavailable. Please try again later.",
  "retryAfter": 300
}
```

### Error Handling Strategy

1. **401 Unauthorized**: Fail-fast, display error to operator (no cascade to MAMP)
2. **403 Forbidden**: Fail-fast, display permission error (no cascade to MAMP)
3. **404 Not Found**: Log error, cascade to MAMP MySQL
4. **429 Rate Limit**: Wait for `retryAfter` seconds, then cascade to MAMP
5. **500/503 Errors**: Log error, cascade to MAMP MySQL immediately
6. **Timeout**: After 10 seconds, cascade to MAMP MySQL

---

## Security Requirements

- ✅ All requests use HTTPS (TLS 1.2+) - SR-004
- ✅ Credentials never logged in plaintext - SR-003
- ✅ Access tokens stored securely in memory only (not persisted)
- ✅ Invalid credentials cause immediate boot failure (fail-fast) - FR-016, SR-002
- ✅ Credentials loaded from secure local storage (Stage 2: Secrets) - SR-001

---

## Performance Requirements

- ✅ Health check completes within 10 seconds - PR-003
- ✅ Master data prefetch completes within 30 seconds (configurable 10-120s) - PR-004
- ✅ Pagination used for large datasets (maxResults: 1000-5000)
- ✅ HEAD requests used for health checks (minimal bandwidth)

---

## Contract Test Implementation

Contract tests will be created in `tests/MTM_Template_Application.Tests.Contract/VisualApiContractTests.cs` using xUnit and FluentAssertions. Tests will validate:

1. Request/response schemas match contracts
2. HTTP status codes are correct
3. Error responses contain required fields
4. Timeouts are enforced
5. Authentication flow works end-to-end
6. Master data endpoints return valid data structures

**Test Execution**: Tests will fail initially (no implementation yet) following TDD workflow.

---

**Next Contract**: [mamp-api-contract.md](./mamp-api-contract.md) for MAMP MySQL 5.7 access patterns.
