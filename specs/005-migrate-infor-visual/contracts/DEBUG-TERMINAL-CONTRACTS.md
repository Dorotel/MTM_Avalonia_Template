# Debug Terminal Contract Tests

**Feature**: 005 - Manufacturing Application Modernization (Phase 3: Debug Terminal Modernization)
**Date**: 2025-10-09
**Purpose**: Define contract tests for Debug Terminal diagnostic data retrieval

---

## Overview

These contract tests validate the Debug Terminal's ability to retrieve diagnostic data from various application subsystems. They ensure that:
- Performance snapshots can be retrieved with proper filtering
- Boot timeline data accurately reflects startup stages
- Environment variables are correctly filtered for sensitive values

**Related Files**:
- Implementation: `MTM_Template_Application/Services/Diagnostics/DiagnosticsService.cs`
- ViewModels: `MTM_Template_Application/ViewModels/DebugTerminalViewModel.cs`
- Views: `MTM_Template_Application/Views/DebugTerminalView.axaml`
- Test Implementation: `tests/contract/DebugTerminalContractTests.cs`

---

## CT-DEBUG-001: Get Performance Snapshots

**Purpose**: Verify that performance snapshots can be retrieved with filtering by feature and count limits.

**Test Scenario**: Get Last 10 Performance Snapshots for All Features

**Request**:
```csharp
// Request parameters
GetPerformanceSnapshotsRequest {
    FeatureFilter = null, // null = all features
    Count = 10, // Last 10 snapshots
    IncludeMemoryMetrics = true,
    IncludeBootMetrics = true,
    IncludeErrorCount = true
}
```

**Expected Response**:
```csharp
// Response structure (array of DiagnosticSnapshot)
DiagnosticSnapshot[] {
    {
        Timestamp = "2025-10-09T10:35:00Z",
        TotalMemoryMB = 95.3,
        PrivateMemoryMB = 68.7,
        Stage0DurationMs = 850,
        Stage1DurationMs = 2400,
        Stage2DurationMs = 750,
        ErrorCount = 2,
        ErrorSummary = "2 warnings in Feature 003",
        ConnectionPoolActive = 3,
        ConnectionPoolIdle = 7,
        EnvironmentVariables = {
            "DOTNET_ENVIRONMENT" = "Development",
            "MTM_DATABASE_SERVER" = "localhost",
            "MTM_VISUAL_API_PASSWORD" = "***FILTERED***"
        }
    },
    // ... 9 more snapshots
}
```

**Validation Rules**:
- Response array MUST contain at most `Count` items (10 in this case)
- Snapshots MUST be ordered by `Timestamp` descending (newest first)
- `TotalMemoryMB` MUST equal `PrivateMemoryMB` + shared memory
- `Stage0DurationMs + Stage1DurationMs + Stage2DurationMs` MUST equal total boot time
- `ErrorCount` MUST match number of logged errors since startup
- `EnvironmentVariables` with sensitive keywords (PASSWORD, TOKEN, SECRET, KEY) MUST show "***FILTERED***"
- All timestamps MUST be valid ISO 8601 format

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetPerformanceSnapshots_WithCount10_ReturnsUpTo10Snapshots()
{
    // Arrange
    var diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsService>();
    var request = new GetPerformanceSnapshotsRequest
    {
        FeatureFilter = null,
        Count = 10,
        IncludeMemoryMetrics = true,
        IncludeBootMetrics = true,
        IncludeErrorCount = true
    };

    // Act
    var snapshots = await diagnosticsService.GetPerformanceSnapshotsAsync(request, CancellationToken.None);

    // Assert
    snapshots.Should().NotBeNull();
    snapshots.Length.Should().BeLessThanOrEqualTo(10);
    snapshots.Should().BeInDescendingOrder(s => s.Timestamp);

    foreach (var snapshot in snapshots)
    {
        snapshot.TotalMemoryMB.Should().BeGreaterThan(0);
        snapshot.PrivateMemoryMB.Should().BeLessThanOrEqualTo(snapshot.TotalMemoryMB);
        snapshot.Timestamp.Should().BeBefore(DateTimeOffset.Now);

        // Validate environment variables filtering
        foreach (var envVar in snapshot.EnvironmentVariables)
        {
            if (envVar.Key.Contains("PASSWORD") ||
                envVar.Key.Contains("TOKEN") ||
                envVar.Key.Contains("SECRET") ||
                envVar.Key.Contains("KEY"))
            {
                envVar.Value.Should().Be("***FILTERED***");
            }
        }
    }
}
```

**Test Scenario**: Get Performance Snapshots for Specific Feature

**Request**:
```csharp
// Request parameters
GetPerformanceSnapshotsRequest {
    FeatureFilter = "feature-001-boot", // Only boot-related metrics
    Count = 5,
    IncludeMemoryMetrics = true,
    IncludeBootMetrics = true,
    IncludeErrorCount = false
}
```

**Expected Response**:
```csharp
// Response structure
DiagnosticSnapshot[] {
    {
        Timestamp = "2025-10-09T10:35:00Z",
        TotalMemoryMB = 95.3,
        PrivateMemoryMB = 68.7,
        Stage0DurationMs = 850,
        Stage1DurationMs = 2400,
        Stage2DurationMs = 750,
        ErrorCount = 0, // Excluded when IncludeErrorCount = false
        ErrorSummary = null,
        ConnectionPoolActive = 0,
        ConnectionPoolIdle = 0,
        EnvironmentVariables = {}
    },
    // ... up to 4 more snapshots
}
```

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetPerformanceSnapshots_WithFeatureFilter_ReturnsOnlyBootMetrics()
{
    // Arrange
    var diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsService>();
    var request = new GetPerformanceSnapshotsRequest
    {
        FeatureFilter = "feature-001-boot",
        Count = 5,
        IncludeMemoryMetrics = true,
        IncludeBootMetrics = true,
        IncludeErrorCount = false
    };

    // Act
    var snapshots = await diagnosticsService.GetPerformanceSnapshotsAsync(request, CancellationToken.None);

    // Assert
    snapshots.Should().NotBeNull();
    snapshots.Length.Should().BeLessThanOrEqualTo(5);

    foreach (var snapshot in snapshots)
    {
        snapshot.Stage0DurationMs.Should().BeGreaterThan(0);
        snapshot.Stage1DurationMs.Should().BeGreaterThan(0);
        snapshot.Stage2DurationMs.Should().BeGreaterThan(0);
        snapshot.ErrorCount.Should().Be(0); // Excluded
        snapshot.ErrorSummary.Should().BeNull();
    }
}
```

---

## CT-DEBUG-002: Get Boot Timeline

**Purpose**: Verify that boot timeline data can be retrieved with stage durations, targets, and pass/fail indicators.

**Test Scenario**: Get Boot Timeline for Last Application Startup

**Request**:
```csharp
// Request parameters (no parameters - always returns last boot)
GetBootTimelineRequest { }
```

**Expected Response**:
```csharp
// Response structure
BootTimelineEntry[] {
    {
        StageNumber = 0,
        StageName = "Splash",
        DurationMs = 850,
        TargetMs = 1000,
        MeetsTarget = true // 850 <= 1000
    },
    {
        StageNumber = 1,
        StageName = "Core Services",
        DurationMs = 2400,
        TargetMs = 3000,
        MeetsTarget = true // 2400 <= 3000
    },
    {
        StageNumber = 2,
        StageName = "Application Ready",
        DurationMs = 750,
        TargetMs = 1000,
        MeetsTarget = true // 750 <= 1000
    }
}
```

**Validation Rules**:
- Response array MUST contain exactly 3 entries (Stage 0, 1, 2)
- `StageNumber` MUST be 0, 1, or 2
- `DurationMs` MUST be greater than 0 (stages can't complete instantly)
- `TargetMs` MUST match performance budget from spec (Stage 0: 1s, Stage 1: 3s, Stage 2: 1s)
- `MeetsTarget` MUST be true if `DurationMs <= TargetMs`
- Sum of all `DurationMs` MUST equal total boot time

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetBootTimeline_Always_ReturnsThreeStages()
{
    // Arrange
    var diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsService>();
    var request = new GetBootTimelineRequest();

    // Act
    var timeline = await diagnosticsService.GetBootTimelineAsync(request, CancellationToken.None);

    // Assert
    timeline.Should().NotBeNull();
    timeline.Length.Should().Be(3);

    timeline[0].StageNumber.Should().Be(0);
    timeline[0].StageName.Should().Be("Splash");
    timeline[0].TargetMs.Should().Be(1000);
    timeline[0].DurationMs.Should().BeGreaterThan(0);

    timeline[1].StageNumber.Should().Be(1);
    timeline[1].StageName.Should().Be("Core Services");
    timeline[1].TargetMs.Should().Be(3000);
    timeline[1].DurationMs.Should().BeGreaterThan(0);

    timeline[2].StageNumber.Should().Be(2);
    timeline[2].StageName.Should().Be("Application Ready");
    timeline[2].TargetMs.Should().Be(1000);
    timeline[2].DurationMs.Should().BeGreaterThan(0);

    // Verify MeetsTarget calculation
    foreach (var stage in timeline)
    {
        var expectedMeetsTarget = stage.DurationMs <= stage.TargetMs;
        stage.MeetsTarget.Should().Be(expectedMeetsTarget);
    }

    // Verify total boot time
    var totalBootTime = timeline.Sum(s => s.DurationMs);
    totalBootTime.Should().BeLessThan(10000); // <10s target
}
```

---

## CT-DEBUG-003: Get Environment Variables with Filtering

**Purpose**: Verify that environment variables can be retrieved with proper filtering of sensitive values.

**Test Scenario**: Get All Environment Variables with Sensitive Filtering

**Request**:
```csharp
// Request parameters
GetEnvironmentVariablesRequest {
    FilterPattern = null, // null = all variables
    ExcludeSensitive = true // Filter sensitive variables
}
```

**Expected Response**:
```csharp
// Response structure (Dictionary<string, string>)
Dictionary<string, string> {
    { "DOTNET_ENVIRONMENT", "Development" },
    { "ASPNETCORE_ENVIRONMENT", "Development" },
    { "MTM_DATABASE_SERVER", "localhost" },
    { "MTM_DATABASE_PORT", "3306" },
    { "MTM_DATABASE_PASSWORD", "***FILTERED***" }, // Sensitive
    { "MTM_VISUAL_API_ENDPOINT", "https://visual.example.com" },
    { "MTM_VISUAL_API_TOKEN", "***FILTERED***" }, // Sensitive
    { "MTM_ENCRYPTION_KEY", "***FILTERED***" }, // Sensitive
    { "PATH", "C:\\Windows\\System32;..." },
    { "TEMP", "C:\\Users\\johnk\\AppData\\Local\\Temp" }
}
```

**Validation Rules**:
- All environment variables MUST be returned (no filtering by name when `FilterPattern = null`)
- Variables containing "PASSWORD", "TOKEN", "SECRET", "KEY", "CONNECTIONSTRING" MUST show "***FILTERED***" when `ExcludeSensitive = true`
- Filtering MUST be case-insensitive (e.g., "password", "Password", "PASSWORD" all filtered)
- Non-sensitive variables MUST show actual values
- Dictionary keys MUST be environment variable names (exact casing)

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetEnvironmentVariables_WithSensitiveFiltering_FiltersSensitiveValues()
{
    // Arrange
    var diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsService>();
    var request = new GetEnvironmentVariablesRequest
    {
        FilterPattern = null,
        ExcludeSensitive = true
    };

    // Act
    var envVars = await diagnosticsService.GetEnvironmentVariablesAsync(request, CancellationToken.None);

    // Assert
    envVars.Should().NotBeNull();
    envVars.Should().NotBeEmpty();

    // Check that sensitive keywords are filtered
    var sensitiveKeywords = new[] { "PASSWORD", "TOKEN", "SECRET", "KEY", "CONNECTIONSTRING" };

    foreach (var envVar in envVars)
    {
        var isSensitive = sensitiveKeywords.Any(keyword =>
            envVar.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (isSensitive)
        {
            envVar.Value.Should().Be("***FILTERED***");
        }
        else
        {
            envVar.Value.Should().NotBe("***FILTERED***");
            envVar.Value.Should().NotBeNullOrEmpty();
        }
    }

    // Verify common environment variables exist
    envVars.Should().ContainKey("DOTNET_ENVIRONMENT");
    envVars.Should().ContainKey("PATH");
}
```

**Test Scenario**: Get Environment Variables with Pattern Filter

**Request**:
```csharp
// Request parameters
GetEnvironmentVariablesRequest {
    FilterPattern = "MTM_*", // Only MTM-prefixed variables
    ExcludeSensitive = true
}
```

**Expected Response**:
```csharp
// Response structure
Dictionary<string, string> {
    { "MTM_DATABASE_SERVER", "localhost" },
    { "MTM_DATABASE_PORT", "3306" },
    { "MTM_DATABASE_PASSWORD", "***FILTERED***" },
    { "MTM_VISUAL_API_ENDPOINT", "https://visual.example.com" },
    { "MTM_VISUAL_API_TOKEN", "***FILTERED***" },
    { "MTM_ENCRYPTION_KEY", "***FILTERED***" }
}
```

**Validation Rules**:
- Only variables matching `FilterPattern` glob pattern MUST be returned
- Pattern matching MUST support wildcards: `*` (any characters), `?` (single character)
- Sensitive filtering MUST still apply even with pattern filter
- Empty pattern MUST return empty dictionary

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetEnvironmentVariables_WithPatternFilter_ReturnsOnlyMatchingVariables()
{
    // Arrange
    var diagnosticsService = _serviceProvider.GetRequiredService<IDiagnosticsService>();
    var request = new GetEnvironmentVariablesRequest
    {
        FilterPattern = "MTM_*",
        ExcludeSensitive = true
    };

    // Act
    var envVars = await diagnosticsService.GetEnvironmentVariablesAsync(request, CancellationToken.None);

    // Assert
    envVars.Should().NotBeNull();

    // All returned keys should start with "MTM_"
    foreach (var envVar in envVars)
    {
        envVar.Key.Should().StartWith("MTM_");
    }

    // Verify sensitive filtering still applies
    if (envVars.ContainsKey("MTM_DATABASE_PASSWORD"))
    {
        envVars["MTM_DATABASE_PASSWORD"].Should().Be("***FILTERED***");
    }
}
```

---

## Summary

**Total Contract Tests**: 3
- CT-DEBUG-001: Get performance snapshots (2 test scenarios: all features + specific feature)
- CT-DEBUG-002: Get boot timeline (1 test scenario)
- CT-DEBUG-003: Get environment variables (2 test scenarios: all variables + pattern filter)

**Coverage**:
- ✅ Performance snapshot retrieval with filtering
- ✅ Memory metrics validation
- ✅ Boot timeline with target comparison
- ✅ Environment variable filtering for sensitive values
- ✅ Pattern-based environment variable filtering
- ✅ Error count and summary tracking

**Implementation Location**: `tests/contract/DebugTerminalContractTests.cs`

**Related Functional Requirements**:
- FR-045: Debug Terminal with SplitView navigation
- FR-046: Organize content by feature
- FR-047: CopyToClipboardCommand implementation
- FR-048: IsMonitoring toggle with snapshot collection
- FR-049: Environment Variables display with filtering
- FR-050: Use custom controls in Debug Terminal sections
- FR-051: Debug Terminal loads within 500ms
