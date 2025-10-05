# Data Model: Environment and Configuration Management

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-05
**Status**: Design Phase

---

## Overview

This document defines the data entities, relationships, and validation rules for the Environment and Configuration Management system. The data model extends the existing Boot Feature 001 implementation with enhanced capabilities for persistence, credential recovery, and error notification.

---

## Entity Diagram

```
┌──────────────────────┐
│ ConfigurationService │ (Singleton Service)
├──────────────────────┤
│ - _envVars: Dict     │
│ - _userConfig: Dict  │
│ - _defaults: Dict    │
│ - _lock: RWLockSlim  │
└──────────────────────┘
        │
        │ uses
        ↓
┌──────────────────────┐
│ UserPreferences      │ (Database Table)
├──────────────────────┤
│ PreferenceId: int PK │
│ UserId: int FK       │
│ PreferenceKey: str   │
│ PreferenceValue: str │
│ Category: str        │
│ LastUpdated: datetime│
└──────────────────────┘

┌──────────────────────┐
│ ISecretsService      │ (Interface)
├──────────────────────┤
│ + StoreSecretAsync   │
│ + RetrieveSecretAsync│
│ + DeleteSecretAsync  │
│ + RotateSecretAsync  │
└──────────────────────┘
        △
        │ implements
        ├────────────────────────┐
        │                        │
┌───────────────────┐  ┌─────────────────────┐
│ WindowsSecrets    │  │ AndroidSecrets      │
│ Service           │  │ Service             │
├───────────────────┤  ├─────────────────────┤
│ - uses DPAPI      │  │ - uses KeyStore     │
│ - CredentialMgr   │  │ - hardware-backed   │
└───────────────────┘  └─────────────────────┘

┌──────────────────────┐
│ FeatureFlagEvaluator │ (Singleton Service)
├──────────────────────┤
│ - _flags: Dict       │
│ - _userIdHasher      │
└──────────────────────┘
        │
        │ manages
        ↓
┌──────────────────────┐
│ FeatureFlag          │ (Model)
├──────────────────────┤
│ Name: string         │
│ IsEnabled: bool      │
│ Environment: string  │
│ RolloutPercentage:int│
│ EvaluatedAt: datetime│
└──────────────────────┘
        │
        │ syncs from
        ↓
┌──────────────────────┐
│ FeatureFlags         │ (Database Table)
├──────────────────────┤
│ FlagId: int PK       │
│ FlagName: string UQ  │
│ IsEnabled: bool      │
│ Environment: string? │
│ RolloutPercentage:int│
│ AppVersion: string   │
│ UpdatedAt: datetime  │
└──────────────────────┘

┌──────────────────────┐
│ ErrorNotification    │ (Service)
│ Service              │
├──────────────────────┤
│ + NotifyAsync        │
│ + ShowStatusBar      │
│ + ShowModalDialog    │
└──────────────────────┘
        │
        │ uses
        ↓
┌──────────────────────┐
│ ConfigurationError   │ (Model)
├──────────────────────┤
│ Key: string          │
│ Message: string      │
│ Severity: enum       │
│ Timestamp: datetime  │
│ IsResolved: bool     │
└──────────────────────┘

┌──────────────────────┐
│ CredentialDialog     │ (ViewModel + View)
├──────────────────────┤
│ Username: string     │
│ Password: string     │
│ ErrorMessage: string │
│ + SubmitCommand      │
│ + CancelCommand      │
└──────────────────────┘
```

---

## Core Entities

### 1. ConfigurationService (Enhanced)

**Type**: Service (Singleton)
**Namespace**: `MTM_Template_Application.Services.Configuration`

**Properties**:
```csharp
// Internal storage (existing)
private readonly Dictionary<string, object> _environmentVariables;
private readonly Dictionary<string, object> _userConfiguration;
private readonly Dictionary<string, object> _defaults;
private readonly ReaderWriterLockSlim _lock; // Upgrade from lock

// NEW: Persistence integration
private readonly IDbConnectionFactory _dbFactory;
private readonly string _userFoldersConfigPath;
```

**Methods** (existing + new):
```csharp
// Existing methods (from Boot Feature 001)
T? GetValue<T>(string key, T? defaultValue = default);
Task SetValue(string key, object value, CancellationToken ct = default);
Task ReloadAsync(CancellationToken ct = default);
event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;

// NEW: Persistence methods
Task LoadUserPreferencesAsync(int userId, CancellationToken ct = default);
Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken ct);
Task<string> GetUserFolderPathAsync(int userId, CancellationToken ct = default);
```

**Validation Rules**:
- `key`: Cannot be null or whitespace
- `value`: Type must match expected type for key
- Environment variable keys must use underscore format (e.g., `MTM_API_TIMEOUT`)
- Configuration keys use colon format (e.g., `API:TimeoutSeconds`)

**State Transitions**:
1. **Initialization**: Load env vars → Load user config from DB → Apply defaults
2. **Runtime Update**: Validate value → Update dictionary → Dispatch event → Persist to DB
3. **Reload**: Clear cache → Re-execute initialization

---

### 2. UserPreferences (Database Table)

**Table Name**: `UserPreferences`
**Database**: MAMP MySQL 5.7

**Schema**:
```sql
CREATE TABLE UserPreferences (
    PreferenceId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    PreferenceKey VARCHAR(255) NOT NULL,
    PreferenceValue TEXT,
    Category VARCHAR(100) COMMENT 'e.g., Display, Filters, Sort',
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY UK_UserPreferences (UserId, PreferenceKey),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    INDEX IDX_UserId (UserId),
    INDEX IDX_Category (Category)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Validation Rules**:
- `UserId`: Must exist in Users table
- `PreferenceKey`: Max 255 characters, no null/empty
- `PreferenceValue`: TEXT field (up to 65,535 characters for complex JSON values)
- `Category`: Optional, max 100 characters

**Example Data**:
```json
{
  "PreferenceId": 1,
  "UserId": 42,
  "PreferenceKey": "Display.Theme",
  "PreferenceValue": "Dark",
  "Category": "Display",
  "LastUpdated": "2025-10-05T14:30:00Z"
}
```

---

### 3. FeatureFlag (Enhanced Model)

**Type**: Model
**Namespace**: `MTM_Template_Application.Models.Configuration`

**Properties** (existing + new):
```csharp
public class FeatureFlag
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Environment { get; set; } = string.Empty; // Empty = all environments
    public int RolloutPercentage { get; set; } // 0-100
    public DateTimeOffset EvaluatedAt { get; set; }

    // NEW: For deterministic rollout
    public string? TargetUserIdHash { get; set; } // Hash for user-specific rollout
    public string? AppVersion { get; set; } // Tied to version
}
```

**Validation Rules**:
- `Name`: Required, max 255 characters, alphanumeric + underscore/dot only
- `RolloutPercentage`: Must be 0-100 (inclusive)
- `Environment`: Must be "Development", "Staging", "Production", or empty string
- If `RolloutPercentage` < 100, `TargetUserIdHash` should be computed

**State Transitions**:
1. **Registered**: Flag added to evaluator dictionary
2. **Evaluated**: `IsEnabledAsync` called, `EvaluatedAt` timestamp updated
3. **Updated**: `SetEnabledAsync` called, `IsEnabled` toggled, `EvaluatedAt` updated
4. **Synced**: Launcher loads from server, updates local flags

---

### 4. FeatureFlags (Database Table)

**Table Name**: `FeatureFlags`
**Database**: MAMP MySQL 5.7

**Schema**:
```sql
CREATE TABLE FeatureFlags (
    FlagId INT PRIMARY KEY AUTO_INCREMENT,
    FlagName VARCHAR(255) NOT NULL UNIQUE,
    IsEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    Environment VARCHAR(50) COMMENT 'NULL for all environments',
    RolloutPercentage INT NOT NULL DEFAULT 0 CHECK (RolloutPercentage BETWEEN 0 AND 100),
    AppVersion VARCHAR(50) COMMENT 'Tied to application version',
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX IDX_FlagName (FlagName),
    INDEX IDX_AppVersion (AppVersion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Validation Rules**:
- `FlagName`: Unique, max 255 characters
- `RolloutPercentage`: 0-100 with CHECK constraint
- `AppVersion`: Semantic versioning format (e.g., "1.0.0")

**Example Data**:
```json
{
  "FlagId": 1,
  "FlagName": "Visual.UseForItems",
  "IsEnabled": true,
  "Environment": "Development",
  "RolloutPercentage": 100,
  "AppVersion": "1.0.0",
  "UpdatedAt": "2025-10-05T14:30:00Z"
}
```

---

### 5. ConfigurationError (New Model)

**Type**: Model
**Namespace**: `MTM_Template_Application.Models.Configuration`

**Properties**:
```csharp
public class ConfigurationError
{
    public string Key { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ErrorSeverity Severity { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool IsResolved { get; set; }
    public string? UserAction { get; set; } // What user should do
}

public enum ErrorSeverity
{
    Info,       // FYI only (e.g., using default value)
    Warning,    // Non-critical (e.g., invalid filter setting)
    Critical    // Must resolve (e.g., database connection)
}
```

**Validation Rules**:
- `Key`: Required, matches configuration key format
- `Message`: Required, user-friendly language (no technical jargon)
- `Severity`: Must be valid enum value
- `UserAction`: Required for Critical severity, optional for others

**State Transitions**:
1. **Created**: Error detected, model instantiated
2. **Notified**: Error shown to user (status bar or dialog)
3. **Resolved**: User corrects issue, `IsResolved` set to true

---

### 6. CredentialDialogViewModel (New)

**Type**: ViewModel
**Namespace**: `MTM_Template_Application.ViewModels.Configuration`

**Properties**:
```csharp
public partial class CredentialDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty; // Sensitive - never log!

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _dialogTitle = "Enter Credentials";

    [ObservableProperty]
    private string _dialogMessage = "Your saved credentials could not be retrieved. Please enter them again.";
}
```

**Commands**:
```csharp
[RelayCommand(CanExecute = nameof(CanSubmit))]
private async Task SubmitAsync(CancellationToken cancellationToken)
{
    IsLoading = true;
    ErrorMessage = string.Empty;

    try
    {
        // Re-attempt storing credentials
        await _secretsService.StoreSecretAsync("Visual.Username", Username, cancellationToken);
        await _secretsService.StoreSecretAsync("Visual.Password", Password, cancellationToken);

        // Close dialog with success
        _dialogResult = true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to store credentials");
        ErrorMessage = "Could not save credentials. Please check permissions.";
    }
    finally
    {
        IsLoading = false;
    }
}

private bool CanSubmit() => !string.IsNullOrWhiteSpace(Username)
                             && !string.IsNullOrWhiteSpace(Password)
                             && !IsLoading;

[RelayCommand]
private void Cancel()
{
    _dialogResult = false;
    // Close dialog
}
```

**Validation Rules**:
- `Username`: Required, min 3 characters, max 100 characters
- `Password`: Required, min 8 characters (Visual ERP policy)
- Both fields trimmed before validation

---

### 7. ErrorNotificationService (New)

**Type**: Service (Singleton)
**Namespace**: `MTM_Template_Application.Services.Configuration`

**Properties**:
```csharp
public class ErrorNotificationService
{
    private readonly ILogger<ErrorNotificationService> _logger;
    private readonly ObservableCollection<ConfigurationError> _activeErrors;

    public IReadOnlyObservableCollection<ConfigurationError> ActiveErrors { get; }
    public event EventHandler<ConfigurationError>? OnErrorOccurred;
}
```

**Methods**:
```csharp
Task NotifyAsync(ConfigurationError error, CancellationToken ct = default);
Task ShowStatusBarWarningAsync(ConfigurationError error, CancellationToken ct = default);
Task<bool> ShowModalDialogAsync(ConfigurationError error, CancellationToken ct = default);
Task ResolveErrorAsync(string key, CancellationToken ct = default);
```

**Behavior**:
- `NotifyAsync`: Route to status bar (Warning/Info) or modal dialog (Critical)
- Status bar: Non-blocking, click to expand details
- Modal dialog: Blocks UI until user addresses issue
- Log all errors with Serilog (structured logging)

#### Severity Classification Criteria

**Critical Severity** (Modal Dialog - Blocks UI):
- Database connection failures preventing data persistence
- Required Visual ERP credentials missing or invalid
- Configuration keys marked as `IsCritical=true` in ConfigurationService
- OS-native secure storage completely unavailable (not just corrupted)
- Invalid critical settings that prevent core functionality

**Warning Severity** (Status Bar - Non-intrusive):
- Invalid data types in environment variables (non-critical keys)
- Missing optional configuration keys (fallback to defaults used)
- User preference load failures (can continue with application defaults)
- Feature flag registration errors (flag treated as disabled)
- Performance threshold warnings (slow database queries)

**Info Severity** (Status Bar - FYI only):
- Configuration changes successfully persisted to database
- Feature flag updates applied successfully
- Credentials re-saved successfully after recovery
- Environment variable overrides applied
- Cache refresh completed

#### Sample User-Friendly Error Messages

**Critical Errors** (Modal Dialog):
```
Title: "Database Connection Required"
Message: "We couldn't connect to the application database. Please check that MySQL is running on port 3306."
UserAction: "Click 'Retry' to attempt reconnection, or contact your system administrator if the problem persists."

Title: "Visual ERP Login Required"
Message: "We couldn't access your saved Visual ERP login information. Please enter your username and password to continue."
UserAction: "Enter your credentials below and click 'Save' to continue."

Title: "Configuration Error"
Message: "The database connection string is missing. This setting is required for the application to work."
UserAction: "Please set the MTM_DB_CONNECTION environment variable and restart the application."
```

**Warning Errors** (Status Bar):
```
Message: "Invalid filter setting 'ABC' - using default filter instead"
Details: "The 'Display.DefaultFilter' setting should be a number. Using default value '0'."

Message: "Feature flag 'NewReportUI' not found - feature disabled"
Details: "The requested feature flag has not been registered. Contact support if you need this feature enabled."

Message: "Unable to load user preferences from database - using defaults"
Details: "Could not retrieve saved preferences for user 'jsmith'. All settings will use application defaults."
```

---

## Configuration Files

### 1. user-folders.json

**Location**: `{repository_root}/config/user-folders.json`

**Schema**:
```json
{
  "centralServerPath": "PLACEHOLDER_REPLACE_ME",
  "defaultPath": "%MyDocuments%/MTM_App",
  "description": "Replace PLACEHOLDER_REPLACE_ME with admin-configured network path (e.g., \\\\server\\share\\users). %MyDocuments% resolves to user's Documents folder."
}
```

**Validation**:
- `centralServerPath`: Must be valid UNC path or "PLACEHOLDER_REPLACE_ME"
- `defaultPath`: Must be valid local path or environment variable reference

---

### 2. database-schema.json

**Location**: `{repository_root}/config/database-schema.json`

**Schema**:
```json
{
  "version": "1.0",
  "tables": [
    {
      "name": "UserPreferences",
      "columns": [
        { "name": "PreferenceId", "type": "INT", "primaryKey": true, "autoIncrement": true },
        { "name": "UserId", "type": "INT", "nullable": false },
        { "name": "PreferenceKey", "type": "VARCHAR(255)", "nullable": false },
        { "name": "PreferenceValue", "type": "TEXT", "nullable": true },
        { "name": "Category", "type": "VARCHAR(100)", "nullable": true },
        { "name": "LastUpdated", "type": "DATETIME", "nullable": false, "default": "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP" }
      ],
      "indexes": [
        { "name": "UK_UserPreferences", "columns": ["UserId", "PreferenceKey"], "unique": true },
        { "name": "IDX_UserId", "columns": ["UserId"] },
        { "name": "IDX_Category", "columns": ["Category"] }
      ],
      "foreignKeys": [
        { "column": "UserId", "references": "Users(UserId)", "onDelete": "CASCADE" }
      ]
    },
    {
      "name": "FeatureFlags",
      "columns": [
        { "name": "FlagId", "type": "INT", "primaryKey": true, "autoIncrement": true },
        { "name": "FlagName", "type": "VARCHAR(255)", "nullable": false, "unique": true },
        { "name": "IsEnabled", "type": "BOOLEAN", "nullable": false, "default": "FALSE" },
        { "name": "Environment", "type": "VARCHAR(50)", "nullable": true },
        { "name": "RolloutPercentage", "type": "INT", "nullable": false, "default": "0", "check": "BETWEEN 0 AND 100" },
        { "name": "AppVersion", "type": "VARCHAR(50)", "nullable": true },
        { "name": "UpdatedAt", "type": "DATETIME", "nullable": false, "default": "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP" }
      ],
      "indexes": [
        { "name": "IDX_FlagName", "columns": ["FlagName"] },
        { "name": "IDX_AppVersion", "columns": ["AppVersion"] }
      ]
    }
  ]
}
```

**Usage**:
- Developer reads this file to generate SQL scripts
- Can be modified before initial database setup
- Version field tracks schema evolution

---

## Relationships

### ConfigurationService ↔ UserPreferences
- **Type**: One-to-Many (User → Preferences)
- **Cardinality**: 1 User : N Preferences
- **Interaction**: ConfigurationService reads/writes to `UserPreferences` table via `IDbConnectionFactory`

### SecretsService → CredentialDialogViewModel
- **Type**: Service → ViewModel (dependency)
- **Interaction**: When `RetrieveSecretAsync` fails, service triggers credential dialog
- **Recovery Flow**: Dialog captures input → Submit → Re-attempt storage

### FeatureFlagEvaluator ↔ FeatureFlags
- **Type**: In-Memory Cache ↔ Database (sync on launch)
- **Interaction**: Launcher loads from DB → Populates evaluator → App uses in-memory cache

### ErrorNotificationService → ConfigurationError
- **Type**: Service manages Model collection
- **Interaction**: Errors created → Added to `ActiveErrors` → Dispatched to UI

---

## Validation Summary

| Entity                    | Key Validations                                     |
| ------------------------- | --------------------------------------------------- |
| ConfigurationService      | Key format, type matching, thread safety            |
| UserPreferences           | UserId FK, unique key per user, TEXT size limits    |
| FeatureFlag               | Name alphanumeric, rollout 0-100, valid environment |
| ConfigurationError        | Severity enum, required user action for Critical    |
| CredentialDialogViewModel | Username min 3, password min 8 chars                |

---

## Performance Constraints

- **Configuration lookup**: <10ms (in-memory dictionary with ReaderWriterLockSlim)
- **Database write**: <100ms (async, non-blocking UI)
- **Credential retrieval**: <100ms (OS-native storage)
- **Feature flag evaluation**: <5ms (in-memory, deterministic hash)

---

## Next Steps

1. ✅ Data model complete
2. → Generate contracts (OpenAPI-style JSON schemas)
3. → Generate contract tests (xUnit tests for data validation)
4. → Create quickstart.md (integration test scenario)

---

**Last Updated**: 2025-10-05
