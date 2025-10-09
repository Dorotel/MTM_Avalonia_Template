# Developer Quickstart Guide

**Feature**: 005 - Manufacturing Application Modernization (All-in-One Mega-Feature)
**Date**: 2025-10-08
**Audience**: New developers setting up local development environment for Phases 1-5

## Prerequisites

### Required Software

| Software | Version | Purpose | Download Link |
|----------|---------|---------|---------------|
| Visual Studio 2022 | 17.8+ | IDE | https://visualstudio.microsoft.com/downloads/ |
| .NET SDK | 9.0 | Runtime/compiler | https://dotnet.microsoft.com/download |
| MAMP | 5.7+ (MySQL 5.7) | Local database | https://www.mamp.info/en/downloads/ |
| Git | 2.40+ | Version control | https://git-scm.com/downloads |
| Avalonia Templates | 11.3.6+ | Custom control development | `dotnet new install Avalonia.Templates` |
| Infor VISUAL API Toolkit | ✅ **AVAILABLE LOCALLY** | ERP integration (Phase 5) | `docs\Visual Files\ReferenceFiles\` (9 DLLs included in repo) |

### Optional Software

| Software | Version | Purpose |
|----------|---------|---------|
| HeidiSQL / MySQL Workbench | Latest | Database management |
| Postman / Insomnia | Latest | API testing |
| Avalonia XAML Previewer | Latest | XAML design-time preview |

### System Requirements

- **OS**: Windows 10/11 (64-bit) - **Required for VISUAL API Toolkit**
- **RAM**: 16GB minimum (8GB for development + 4GB for MAMP + 4GB for VISUAL/OS)
- **Disk**: 10GB free space (5GB for Visual Studio + 2GB for MAMP + 3GB for project)
- **Architecture**: x86 architecture required for VISUAL API Toolkit (.NET 4.x x86 library)

---

## Step 1: Custom Control Development Setup (Phase 1)

### 1.1 Verify Avalonia Templates

```powershell
# Check if Avalonia templates are installed
dotnet new list avalonia

# Expected output should include:
# Avalonia AvaloniaUI Application
# Avalonia AvaloniaUI Cross Platform Application
# Avalonia Custom Control
# Avalonia Resource Dictionary
# Avalonia Styles
# Avalonia TemplatedControl
# Avalonia UserControl
# Avalonia Window

# If not installed, install Avalonia templates
dotnet new install Avalonia.Templates
```

### 1.2 Configure XAML Previewer

**Visual Studio 2022**:
1. Install **Avalonia for Visual Studio 2022** extension from Extensions → Manage Extensions
2. Restart Visual Studio
3. Open any `.axaml` file to verify previewer appears in right panel
4. If previewer doesn't work, check Tools → Options → Avalonia → "Enable Previewer"

**VS Code (Alternative)**:
1. Install **Avalonia for VSCode** extension
2. Install **C# Dev Kit** extension
3. Open `.axaml` file and press `Ctrl+Shift+P` → "Avalonia: Open Previewer"

### 1.3 Verify StyledProperty Syntax

Create a test file to verify StyledProperty compilation:

```csharp
// File: TestControl.cs (delete after verification)
using Avalonia;
using Avalonia.Controls;

namespace MTM_Template_Application.Controls;

public class TestControl : TemplatedControl
{
    // This should compile without errors
    public static readonly StyledProperty<string?> TestProperty =
        AvaloniaProperty.Register<TestControl, string?>(
            nameof(Test),
            defaultValue: null);

    public string? Test
    {
        get => GetValue(TestProperty);
        set => SetValue(TestProperty, value);
    }
}
```

**Validation**:
- Build project: `dotnet build`
- If no errors, StyledProperty pattern works ✅
- Delete `TestControl.cs` after verification

---

## Step 2: Settings Management Setup (Phase 2)

### 2.1 Verify IConfigurationService API

```csharp
// File: Services/Configuration/IConfigurationService.cs (should already exist from Feature 002)
public interface IConfigurationService
{
    T GetValue<T>(string key, T defaultValue);
    void SetValue<T>(string key, T value);
    event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;
}
```

**Validation**:
```powershell
# Search for IConfigurationService usage
Select-String -Path "MTM_Template_Application\Services\**\*.cs" -Pattern "IConfigurationService"

# Expected: Should find ConfigurationService implementation from Feature 002
```

### 2.2 Verify UserPreferences Database Table

```sql
-- Connect to MAMP MySQL (from MAMP control panel: Tools → phpMyAdmin)
-- Or use HeidiSQL with: Host=localhost, Port=3306, User=root, Password=root

USE mtm_template_dev;

-- Verify UserPreferences table exists (created in Feature 002)
SHOW TABLES LIKE 'UserPreferences';

-- Expected: UserPreferences table should exist
-- If not, run migration: config\migrations\001_initial_schema.sql

-- Verify schema
DESCRIBE UserPreferences;

-- Expected columns:
-- PreferenceId (int, PK, AUTO_INCREMENT)
-- UserId (int, FK to Users)
-- PreferenceKey (varchar(200), UNIQUE)
-- PreferenceValue (text)
-- LastUpdated (datetime)
```

### 2.3 Test Settings Validation Patterns

Create a test validator to verify FluentValidation integration:

```csharp
// File: TestSettingValidator.cs (delete after verification)
using FluentValidation;

namespace MTM_Template_Application.Models.Configuration;

public class TestSettingValidator : AbstractValidator<SettingDefinition>
{
    public TestSettingValidator()
    {
        RuleFor(x => x.SettingKey)
            .NotEmpty().WithMessage("Setting key is required")
            .MaximumLength(200).WithMessage("Setting key too long");

        When(x => x.SettingType == SettingValueType.ConnectionString, () =>
        {
            RuleFor(x => x.SettingValue)
                .Must(BeValidConnectionString)
                .WithMessage("Invalid connection string format");
        });
    }

    private bool BeValidConnectionString(string value)
    {
        // Simple validation: must contain Server= and Database=
        return value.Contains("Server=") && value.Contains("Database=");
    }
}
```

**Validation**:
```powershell
# Build project
dotnet build

# Run validator test
dotnet test --filter "FullyQualifiedName~SettingValidator"

# If tests pass, validation works ✅
# Delete TestSettingValidator.cs after verification
```

---

## Step 3: Debug Terminal Navigation Setup (Phase 3)

### 3.1 Verify SplitView Layout Pattern

Open `Views/DebugTerminalView.axaml` (should exist from Feature 003):

```xml
<!-- Expected structure: -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MTM_Template_Application.Views.DebugTerminalView">

    <SplitView IsPaneOpen="True" DisplayMode="Inline" PaneBackground="...">
        <!-- Left navigation pane (feature sections) -->
        <SplitView.Pane>
            <ListBox SelectionMode="Single">
                <!-- Feature 001: Boot -->
                <!-- Feature 002: Config -->
                <!-- Feature 003: Debug Terminal -->
                <!-- Feature 005: VISUAL ERP -->
            </ListBox>
        </SplitView.Pane>

        <!-- Right content area (diagnostic data) -->
        <SplitView.Content>
            <ContentControl Content="{Binding SelectedSection}" />
        </SplitView.Content>
    </SplitView>
</UserControl>
```

**Validation**:
- Open file in Visual Studio
- XAML previewer should show split view layout
- If errors, check x:DataType is set to DebugTerminalViewModel

### 3.2 Test Feature Routing

```csharp
// File: ViewModels/DebugTerminalViewModel.cs (partial snippet for verification)
public partial class DebugTerminalViewModel : ObservableObject
{
    [ObservableProperty]
    private DiagnosticSection? _selectedSection;

    partial void OnSelectedSectionChanged(DiagnosticSection? value)
    {
        // Route to feature-specific view
        if (value?.SectionId == "feature-001-boot")
        {
            // Load Feature 001 diagnostics
        }
        else if (value?.SectionId == "feature-002-config")
        {
            // Load Feature 002 diagnostics
        }
        // ... etc
    }
}
```

**Validation**:
```powershell
# Search for SplitView usage
Select-String -Path "MTM_Template_Application\Views\**\*.axaml" -Pattern "SplitView"

# Expected: Should find DebugTerminalView.axaml with SplitView
```

---

## Step 4: Configuration Error Dialog Setup (Phase 4)

### 4.1 Verify Dialog Service Integration

```csharp
// File: Services/Core/IDialogService.cs (should exist)
public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task<string?> ShowInputAsync(string title, string prompt, string? defaultValue = null);
}
```

**Validation**:
```powershell
# Search for IDialogService
Select-String -Path "MTM_Template_Application\Services\**\*.cs" -Pattern "IDialogService"

# Expected: Should find DialogService implementation
```

### 4.2 Test Error Dialog Patterns

Create test to verify error dialog can show configuration errors:

```csharp
// File: Tests/unit/ConfigurationErrorDialogTests.cs
[Fact]
public async Task ShowConfigurationError_WithInvalidConnectionString_ShowsDialog()
{
    // Arrange
    var dialogService = Substitute.For<IDialogService>();
    var viewModel = new ConfigurationErrorDialogViewModel(dialogService);
    var error = new ConfigurationError
    {
        SettingKey = "Database:ConnectionString",
        ErrorMessage = "Invalid connection string format",
        Severity = ErrorSeverity.Critical
    };

    // Act
    await viewModel.ShowErrorAsync(error);

    // Assert
    await dialogService.Received(1).ShowErrorAsync(
        Arg.Is<string>(s => s.Contains("Configuration Error")),
        Arg.Is<string>(s => s.Contains("Database:ConnectionString"))
    );
}
```

---

## Step 5: Verify Infor VISUAL API Toolkit Files (Phase 5)

**EXCELLENT NEWS**: The VISUAL API Toolkit files are already available in this repository at `docs\Visual Files\`!

### 5.1 Verify Toolkit Files

```powershell
# Verify DLL assemblies (9 files)
Get-ChildItem "docs\Visual Files\ReferenceFiles\*.dll" | Select-Object Name

# Expected output:
# LsaCore.dll
# LsaShared.dll
# VmfgFinancials.dll
# VmfgInventory.dll
# VmfgPurchasing.dll
# VmfgSales.dll
# VmfgShared.dll
# VmfgShopFloor.dll
# VmfgTrace.dll

# Verify reference guides (7 files)
Get-ChildItem "docs\Visual Files\Guides\*.txt" | Select-Object Name

# Expected output:
# Reference - Development Guide.txt
# Reference - Core.txt
# Reference - Inventory.txt
# Reference - Shared Library.txt
# Reference - Shop Floor.txt
# Reference - VMFG Shared Library.txt
# User Manual.txt

# Verify database schema files (4 files)
Get-ChildItem "docs\Visual Files\Database Files\*.csv" | Select-Object Name

# Expected output:
# MTMFG Tables.csv
# MTMFG Procedures.csv
# MTMFG Relationships.csv
# Visual Data Table.csv
```

### 5.2 Review Reference Documentation (RECOMMENDED)

Before implementation, review these guides to understand API patterns:

1. **Start Here**: `docs\Visual Files\Guides\Reference - Development Guide.txt`
   - API authentication patterns
   - Connection management
   - Error handling conventions
   - Best practices

2. **Inventory Operations** (FR-004): `docs\Visual Files\Guides\Reference - Inventory.txt`
   - GetInventoryBalance methods
   - InventoryTransaction structures
   - Inventory adjustment operations

3. **Shop Floor Operations** (FR-002): `docs\Visual Files\Guides\Reference - Shop Floor.txt`
   - GetWorkOrders methods
   - WorkOrder entity structures
   - Material requirements and operation steps

4. **Database Schema Validation**: `docs\Visual Files\Database Files\MTMFG Tables.csv`
   - Cross-reference entity structures in data-model.md
   - Validate field names and types match VISUAL database

**Time Savings**: Having local documentation eliminates 4-8 hours of API discovery research originally planned!

---

## Step 6: Configure MAMP MySQL Database

### 6.1 Start MAMP MySQL

```powershell
# Start MAMP application
Start-Process "C:\MAMP\MAMP.exe"

# Wait for MySQL to start (green indicator in MAMP UI)
# Default port: 3306
```

### 6.2 Create Development Database

```powershell
# Connect to MySQL using MAMP's bundled client
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -e "CREATE DATABASE IF NOT EXISTS mtm_template_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Verify database creation
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -e "SHOW DATABASES LIKE 'mtm_template_dev';"

# Expected output:
# +-------------------------+
# | Database (mtm_template_dev) |
# +-------------------------+
# | mtm_template_dev        |
# +-------------------------+
```

### 6.3 Run Initial Schema Migration

```powershell
# Navigate to project root
cd C:\Users\johnk\source\repos\MTM_Avalonia_Template

# Run migration script
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -D mtm_template_dev < config\migrations\001_initial_schema.sql

# Verify tables created
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -D mtm_template_dev -e "SHOW TABLES;"

# Expected output:
# +----------------------------+
# | Tables_in_mtm_template_dev |
# +----------------------------+
# | Users                      |
# | UserPreferences            |
# | FeatureFlags               |
# | LocalTransactionRecords    | (new in feature 005)
# +----------------------------+
```

### 6.4 Load Sample Data (Optional)

```powershell
# Load sample transaction records for testing
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -D mtm_template_dev -e "
INSERT INTO LocalTransactionRecords (transaction_type, entity_data, created_by) VALUES
('WorkOrderStatusUpdate', '{\"workOrderId\": \"WO-2024-001\", \"previousStatus\": \"Open\", \"newStatus\": \"InProgress\", \"notes\": \"Test\"}', 'developer'),
('InventoryReceipt', '{\"partId\": \"12345-ABC\", \"quantity\": 100.00, \"unitOfMeasure\": \"EA\", \"referenceDocument\": \"PO-TEST-001\"}', 'developer');
"

# Verify data loaded
& "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -D mtm_template_dev -e "SELECT * FROM LocalTransactionRecords;"
```

---

## Step 7: Configure VISUAL API Toolkit Credentials

### 7.1 Store Credentials Using WindowsSecretsService

The application uses Windows Credential Manager (DPAPI) for secure credential storage. You need to store VISUAL database connection details:

```powershell
# Option 1: Use application UI (recommended)
# 1. Run application (see Step 5)
# 2. Navigate to Settings > VISUAL Connection
# 3. Enter credentials and click "Save"

# Option 2: Use PowerShell script (for automation)
$credentialScript = @'
Add-Type -AssemblyName System.Security
$server = "visual-server.example.com"
$database = "VISUAL_DB"
$username = "visual_user"
$password = "your_visual_password" | ConvertTo-SecureString -AsPlainText -Force

# Store credentials in Windows Credential Manager
cmdkey /generic:"MTM_VISUAL_Server" /user:"$server" /pass
cmdkey /generic:"MTM_VISUAL_Database" /user:"$database" /pass
cmdkey /generic:"MTM_VISUAL_Username" /user:"$username" /pass
# Password stored separately via SecureString (handled by WindowsSecretsService)
'@

# Save script and run as administrator
$credentialScript | Out-File -FilePath ".\setup-visual-credentials.ps1"
```

### 7.2 Test Credential Retrieval

```powershell
# List stored credentials
cmdkey /list | Select-String "MTM_VISUAL"

# Expected output:
# Target: MTM_VISUAL_Server
# Target: MTM_VISUAL_Database
# Target: MTM_VISUAL_Username
```

---

## Step 8: Clone and Build Project

### 8.1 Clone Repository

```powershell
# Clone repository
git clone https://github.com/Dorotel/MTM_Avalonia_Template.git C:\Users\johnk\source\repos\MTM_Avalonia_Template

# Navigate to project directory
cd C:\Users\johnk\source\repos\MTM_Avalonia_Template

# Checkout feature branch (if not already on it)
git checkout 005-migrate-infor-visual
```

### 8.2 Restore NuGet Packages

```powershell
# Restore all project dependencies
dotnet restore MTM_Template_Application.sln

# Expected output:
# Determining projects to restore...
# Restored C:\Users\johnk\source\repos\MTM_Avalonia_Template\MTM_Template_Application\MTM_Template_Application.csproj (in X ms).
# Restored C:\Users\johnk\source\repos\MTM_Avalonia_Template\tests\MTM_Template_Tests.csproj (in X ms).
```

### 8.3 Build Solution

```powershell
# Build entire solution (all projects)
dotnet build MTM_Template_Application.sln --configuration Debug

# Expected output:
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)
# Time Elapsed 00:00:XX.XX
```

**Common Build Errors**:

| Error | Cause | Solution |
|-------|-------|----------|
| `CS0246: The type or namespace 'Visual' could not be found` | VISUAL API Toolkit not referenced | Add project reference to Visual.dll (see Step 4.4) |
| `error MSB4018: The "ResolveComReference" task failed` | COM registration missing | Run `regsvr32 Visual.dll` as admin |
| `NETSDK1045: The current .NET SDK does not support 'newer version' as a target` | .NET SDK version mismatch | Install .NET 9.0 SDK |

### 8.4 Add VISUAL API Toolkit References

The VISUAL API Toolkit DLLs are located at `docs\Visual Files\ReferenceFiles\`. Add references to all 9 assemblies:

**Manual .csproj edit** (recommended):

```xml
<!-- MTM_Template_Application/MTM_Template_Application.csproj -->
<ItemGroup>
  <!-- VISUAL API Toolkit Core -->
  <Reference Include="LsaCore">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\LsaCore.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="LsaShared">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\LsaShared.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>

  <!-- VISUAL Manufacturing (Vmfg) Modules -->
  <Reference Include="VmfgFinancials">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgFinancials.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgInventory">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgInventory.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgPurchasing">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgPurchasing.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgSales">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgSales.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgShared">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgShared.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgShopFloor">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgShopFloor.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
  <Reference Include="VmfgTrace">
    <HintPath>..\..\docs\Visual Files\ReferenceFiles\VmfgTrace.dll</HintPath>
    <Private>True</Private>
    <CopyLocal>True</CopyLocal>
  </Reference>
</ItemGroup>
```

**Verify References**:

```powershell
# Check that all DLLs are copied to output directory
Get-ChildItem ".\MTM_Template_Application\bin\Debug\net9.0\*.dll" | Where-Object { $_.Name -like "Lsa*" -or $_.Name -like "Vmfg*" } | Select-Object Name

# Expected output (9 DLLs):
# LsaCore.dll
# LsaShared.dll
# VmfgFinancials.dll
# VmfgInventory.dll
# VmfgPurchasing.dll
# VmfgSales.dll
# VmfgShared.dll
# VmfgShopFloor.dll
# VmfgTrace.dll
```

---

## Step 8.5: Configure VISUAL Integration Options

### 8.5.1 Configuration Schema

The `VisualIntegrationOptions` configuration section controls VISUAL API Toolkit behavior, performance monitoring, and degradation mode settings. Add this to `appsettings.json`:

```json
{
  "VisualIntegration": {
    "Performance": {
      "MaxResponseTimeMs": 5000,              // FR-020: Degradation trigger threshold
      "MaxConsecutiveFailures": 5,            // FR-020: Consecutive failures before degradation
      "DegradationRetryIntervalMinutes": 5,   // FR-020: Auto-retry interval in degradation mode
      "PartLookupTargetMs": 3000,             // FR-001: 99.9% part lookups under 3s
      "WorkOrderQueryTargetMs": 2000,         // FR-002: Work orders under 2s
      "InventoryQueryTargetMs": 5000,         // FR-004: Inventory transactions under 5s
      "CachedDataTargetMs": 1000              // FR-006: Cached reads under 1s
    },
    "Cache": {
      "PartTtlHours": 24,                     // FR-015: Parts cached 24 hours
      "WorkOrderTtlDays": 7,                  // FR-015: Work orders cached 7 days
      "InventoryTtlDays": 7,                  // FR-015: Inventory cached 7 days
      "ShipmentTtlDays": 7,                   // FR-015: Shipments cached 7 days
      "MaxCacheSizeMB": 40,                   // Design: LZ4 compressed cache budget
      "EvictionPolicy": "LRU"                 // Design: Least Recently Used eviction
    },
    "DegradationMode": {
      "Enabled": true,                        // FR-020: Enable automatic degradation
      "DisplayNotification": true,            // FR-020: Show "VISUAL API unavailable" banner
      "AllowManualRetry": true,               // FR-020: "Retry Connection" button enabled
      "AutoRetryEnabled": true,               // FR-020: Auto-retry every 5 minutes
      "LogDegradationEvents": true            // FR-019: Log degradation triggers
    },
    "PerformanceMonitoring": {
      "SilentLoggingEnabled": true,           // FR-019: Log all VISUAL API calls
      "DebugTerminalEnabled": true,           // FR-021: Show performance panel in DebugTerminal
      "MaxHistoryEntries": 10,                // FR-021: Last 10 API calls in UI
      "TrendWindowHours": 1,                  // FR-021: Performance trend chart (1 hour)
      "ConnectionPoolAlertThresholdMs": 1000  // FR-021: Alert if average wait >1s
    },
    "Connection": {
      "Server": "VISUAL\\PLAY",               // Database.config: VISUAL server instance
      "Database": "MTMFGPLAY",                // Database.config: VISUAL database name
      "TimeoutSeconds": 30,                   // Default connection timeout
      "RetryPolicy": {
        "MaxRetries": 3,                      // Polly: Max retry attempts
        "BackoffMultiplier": 2,               // Polly: Exponential backoff (1s, 2s, 4s)
        "InitialDelayMs": 1000                // Polly: First retry after 1s
      }
    },
    "Logging": {
      "LogRequestTimestamps": true,           // FR-011, FR-019: Log request timestamps
      "LogResponseTimes": true,               // FR-011, FR-019: Log response duration
      "LogErrorDetails": true,                // FR-011, FR-019: Log error codes/messages
      "LogCacheIndicators": true,             // FR-019: Log cached vs live data source
      "ExcludeSensitiveData": true            // FR-011: Never log credentials/PII
    },
    "Validation": {
      "ValidateTransactions": true,           // FR-013: Validate against cached VISUAL data
      "ShowWarnings": true,                   // FR-013: Display warnings (not blocking)
      "AllowExceedingCachedValues": true      // FR-013: Allow save even with warnings
    },
    "PlatformConstraints": {
      "WindowsOnly": true,                    // FR-014: VISUAL API Toolkit Windows-only
      "UnsupportedPlatformMessage": "VISUAL integration requires Windows. You are running in view-only mode with cached data."
    }
  }
}
```

### 8.5.2 Environment Variable Overrides

Configuration values can be overridden using environment variables with double-underscore format:

```powershell
# Override performance thresholds
$env:VisualIntegration__Performance__MaxResponseTimeMs = "3000"
$env:VisualIntegration__Performance__MaxConsecutiveFailures = "3"

# Override cache TTL
$env:VisualIntegration__Cache__PartTtlHours = "48"

# Override degradation settings
$env:VisualIntegration__DegradationMode__AutoRetryEnabled = "false"

# Override connection settings
$env:VisualIntegration__Connection__Server = "CUSTOM_SERVER"
$env:VisualIntegration__Connection__Database = "CUSTOM_DB"
```

### 8.5.3 Configuration Validation

After adding configuration, validate settings using the Debug Terminal:

1. Launch application (`dotnet run`)
2. Open Debug Terminal (Window → Debug Terminal or F12)
3. Navigate to "VISUAL API Performance" panel (FR-021)
4. Check connection status indicator (green = healthy, yellow = degradation, red = offline)
5. Click "Test Connection" to validate VISUAL API Toolkit connectivity

**Expected Debug Terminal Output**:
```
[VISUAL] Configuration loaded successfully
[VISUAL] Connection: VISUAL\PLAY → MTMFGPLAY
[VISUAL] Performance thresholds: MaxResponseTime=5000ms, MaxFailures=5
[VISUAL] Cache TTL: Parts=24h, WorkOrders=7d, Inventory=7d
[VISUAL] Degradation mode: ENABLED, auto-retry every 5 minutes
[VISUAL] Test Connection: SUCCESS (response=234ms)
```

---

## Step 9: Run Application

### 9.1 Run Desktop Application

```powershell
# Run from command line
dotnet run --project MTM_Template_Application.Desktop\MTM_Template_Application.Desktop.csproj

# Or use VS Code task (Ctrl+Shift+P → "Tasks: Run Task" → "Run Main App")
```

**Expected Behavior**:
1. **Splash screen** appears (Stage 0: <1s)
2. **Service initialization** (Stage 1: <3s)
   - ConfigurationService loads settings
   - WindowsSecretsService retrieves VISUAL credentials
   - MySQL connection pool initialized
   - VISUAL API Toolkit connection established
3. **Main window** appears (Stage 2: <1s)
4. **Debug Terminal** accessible (View → Debug Terminal or F12)

### 9.2 Verify VISUAL Connection

1. Open Debug Terminal (F12 or View → Debug Terminal)
2. Navigate to "VISUAL API Performance" tab
3. Check "Connection Status" indicator:
   - **Green**: Connected to VISUAL server
   - **Yellow**: Using cached data only (degraded mode)
   - **Red**: Connection failed (check credentials/network)

### 9.3 Test Part Lookup

1. Navigate to "Parts" view
2. Enter part number: "12345-ABC" (replace with actual part in your VISUAL database)
3. Click "Search"
4. Verify:
   - Part details display within <3s (FR-011 performance target)
   - Debug Terminal shows API call metrics (duration, cache hit/miss)
   - No errors in application logs

---

## Step 10: Run Tests

### 10.1 Run All Tests

```powershell
# Run all tests (unit + integration + contract)
dotnet test MTM_Template_Application.sln --configuration Debug

# Expected output:
# Passed!  - Failed:     0, Passed:    XX, Skipped:     0, Total:    XX, Duration: X s
```

### 10.2 Run Contract Tests (VISUAL API Toolkit)

Contract tests validate that VISUAL API Toolkit endpoints match expected behavior:

```powershell
# Run contract tests only
dotnet test --filter "Category=Contract"

# Expected output:
# Passed!  - Failed:     0, Passed:    10, Skipped:     0, Total:    10, Duration: X s
# (10 contract tests: Parts, WorkOrders, Inventory, Shipments, Authentication, etc.)
```

**Common Test Failures**:

| Test Failure | Cause | Solution |
|-------------|-------|----------|
| `VisualConnectionTests.Should_Connect_To_Visual_Server` | Credentials not configured | Complete Step 3 (configure credentials) |
| `PartLookupTests.Should_Retrieve_Part_Details` | VISUAL server unavailable | Check network, VPN, firewall |
| `LocalTransactionTests.Should_Persist_To_MySQL` | MAMP MySQL not running | Start MAMP (Step 2.1) |

### 10.3 Run Performance Tests

```powershell
# Run performance tests (validates <3s targets)
dotnet test --filter "Category=Performance"

# Expected output:
# Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: X s
# (5 performance tests: Part lookup, Work order query, Inventory transaction, Cache compression, Boot time)
```

---

## Step 11: Development Workflow

### 11.1 Hot Reload (Watch Mode)

```powershell
# Enable hot reload during development
dotnet watch --project MTM_Template_Application.Desktop\MTM_Template_Application.Desktop.csproj

# Now edit .cs or .axaml files and see changes immediately (no rebuild required)
```

### 11.2 Debug with Visual Studio

1. Open `MTM_Template_Application.sln` in Visual Studio 2022
2. Set `MTM_Template_Application.Desktop` as startup project (right-click → Set as Startup Project)
3. Press **F5** to start debugging
4. Set breakpoints in VISUAL integration code (Services/Visual/VisualApiService.cs)

### 11.3 View Logs

Logs are written to `logs/app-YYYYMMDD.txt` using Serilog:

```powershell
# Tail logs in real-time
Get-Content logs\app-$(Get-Date -Format yyyyMMdd).txt -Tail 50 -Wait

# Filter for VISUAL API calls
Select-String -Path "logs\*.txt" -Pattern "VISUAL API"

# Filter for errors
Select-String -Path "logs\*.txt" -Pattern "ERROR|FATAL"
```

---

## Step 12: Troubleshooting

### Issue: "VISUAL API Toolkit not found"

**Symptoms**: Build error `CS0246: The type or namespace 'Visual' could not be found`

**Solution**:
1. Verify installation: `Test-Path "C:\Program Files (x86)\Infor\VISUAL API Toolkit\Visual.dll"`
2. Add project reference (Step 4.4)
3. Rebuild solution

### Issue: "MySQL connection failed"

**Symptoms**: Application crashes on startup with `MySqlException: Unable to connect to any of the specified MySQL hosts`

**Solution**:
1. Check MAMP is running: Open MAMP UI, verify green "MySQL" indicator
2. Test connection manually:
   ```powershell
   & "C:\MAMP\bin\mysql\bin\mysql.exe" -u root -p"root" -h 127.0.0.1 -P 3306 -e "SELECT 1;"
   ```
3. Verify connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "MySQL": "Server=127.0.0.1;Port=3306;Database=mtm_template_dev;Uid=root;Pwd=root;"
     }
   }
   ```

### Issue: "VISUAL credentials not found"

**Symptoms**: Application shows "VISUAL Connection Failed" dialog on startup

**Solution**:
1. Store credentials using Step 3.1
2. Verify credentials stored:
   ```powershell
   cmdkey /list | Select-String "MTM_VISUAL"
   ```
3. Restart application

### Issue: "Performance tests failing (timeout)"

**Symptoms**: `PartLookupTests.Should_Complete_Within_3_Seconds` fails with "Expected duration < 3000ms, but was 5234ms"

**Solution**:
1. Check VISUAL server latency (network/VPN)
2. Verify cache is enabled (check `appsettings.json` → `CacheEnabled: true`)
3. Run test again (first run is always slower due to cold cache)

---

## Step 13: Next Steps

### For Feature Development

1. **Review specification**: Read `specs/005-migrate-infor-visual/spec.md`
2. **Review implementation plan**: Read `specs/005-migrate-infor-visual/plan.md`
3. **Check task list**: Read `specs/005-migrate-infor-visual/tasks.md` (after generation)
4. **Join team standup**: Daily at 9:00 AM (check team calendar)

### For Testing

1. **Write contract tests**: Add tests to `tests/contract/VisualApiContractTests.cs`
2. **Write integration tests**: Add tests to `tests/integration/VisualIntegrationTests.cs`
3. **Run full test suite**: `dotnet test MTM_Template_Application.sln`

### For Documentation

1. **Update this guide**: If you encounter issues not listed here, add troubleshooting steps
2. **Update data model**: If entity structures change, update `specs/005-migrate-infor-visual/data-model.md`

---

## Quick Reference

### Useful Commands

```powershell
# Start MAMP MySQL
Start-Process "C:\MAMP\MAMP.exe"

# Run application
dotnet run --project MTM_Template_Application.Desktop\MTM_Template_Application.Desktop.csproj

# Run tests
dotnet test

# View logs
Get-Content logs\app-$(Get-Date -Format yyyyMMdd).txt -Tail 50 -Wait

# Build solution
dotnet build MTM_Template_Application.sln

# Clean build artifacts
dotnet clean MTM_Template_Application.sln
```

### Key File Locations

| File/Directory | Purpose |
|---------------|---------|
| `MTM_Template_Application/Services/Visual/` | VISUAL API integration services |
| `MTM_Template_Application/ViewModels/Visual/` | VISUAL-related ViewModels |
| `MTM_Template_Application/Views/Visual/` | VISUAL UI views (.axaml) |
| `tests/contract/` | VISUAL API contract tests |
| `logs/` | Application logs (Serilog output) |
| `config/migrations/` | MySQL schema migrations |
| `.github/mamp-database/schema-tables.json` | Database schema documentation |

### Support Contacts

| Issue Type | Contact |
|-----------|---------|
| VISUAL API Toolkit questions | Review `docs\Visual Files\Guides\Reference - Development Guide.txt` |
| Application bugs | GitHub issues: <https://github.com/Dorotel/MTM_Avalonia_Template/issues> |
| Development questions | Team lead: <john.koll@example.com> |

---

**Quickstart Completion Checklist**:

- [ ] Installed all prerequisites (Visual Studio, .NET 9.0, MAMP, VISUAL API Toolkit)
- [ ] Started MAMP MySQL and created `mtm_template_dev` database
- [ ] Ran initial schema migration (`001_initial_schema.sql`)
- [ ] Configured VISUAL credentials using WindowsSecretsService
- [ ] Cloned repository and checked out feature branch
- [ ] Built solution successfully (`dotnet build`)
- [ ] Added VISUAL API Toolkit reference to project
- [ ] Ran application and verified splash screen → main window flow
- [ ] Verified VISUAL connection in Debug Terminal (green status)
- [ ] Ran all tests successfully (`dotnet test`)
- [ ] Reviewed specification and implementation plan

**Ready for Development**: ✅ YES (if all checklist items complete)
