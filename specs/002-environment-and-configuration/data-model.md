# Data Model: Environment and Configuration Management

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-05
**Status**: Design Complete

## Entity Relationship Diagram

```
[Users] 1---* [UserPreferences]
        |
        └---* [FeatureFlags] (no direct FK, flags are global)

[ConfigurationService] ---uses---> [UserPreferences]
[ConfigurationService] ---uses---> [EnvironmentVariables] (OS-level)
[FeatureFlagEvaluator] ---uses---> [FeatureFlags]
[SecretsServiceFactory] ---creates---> [ISecretsService]
                                        ├── [WindowsSecretsService]
                                        └── [AndroidSecretsService]
```

## Core Entities

### 1. Configuration Management

#### ConfigurationService
Platform-agnostic configuration manager with layered precedence and change events.

**Properties**:
- `_environmentVariables: Dictionary<string, string?>` - Cached env vars (read at startup)
- `_userConfiguration: Dictionary<string, object?>` - Runtime user settings (in-memory + DB)
- `_applicationDefaults: Dictionary<string, object?>` - Hard-coded fallback values
- `_lockObject: object` - Thread-safety lock

**Methods**:
- `T? GetValue<T>(string key, T? defaultValue = default)` - Layered retrieval with type safety
- `void SetValue(string key, object? value)` - Update user config + trigger event if changed
- `Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken)` - Load from MySQL
- `Task SaveUserPreferenceAsync(int userId, string key, object? value, CancellationToken cancellationToken)` - Persist to MySQL

**Events**:
- `event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged` - Fires when value changes

**Validation Rules**:
- Configuration keys MUST follow hierarchical namespace (e.g., `API:BaseUrl`, `Visual:Username`)
- Type safety enforced via generics + runtime `Convert.ChangeType`
- Null values supported (nullable reference types)

**Layered Precedence** (highest to lowest):
1. Environment Variables (MTM_ENVIRONMENT, MTM_API_TIMEOUT, etc.)
2. User Configuration (runtime-set via SetValue, persisted to MySQL)
3. Application Defaults (hard-coded fallback)

#### EnvironmentType (Enum)

```csharp
public enum EnvironmentType
{
    Development,
    Staging,
    Production
}
```

**Detection Logic** (startup-only, precedence order):
1. `MTM_ENVIRONMENT` environment variable
2. `ASPNETCORE_ENVIRONMENT` environment variable
3. `DOTNET_ENVIRONMENT` environment variable
4. Build configuration (DEBUG → Development, RELEASE → Production)

#### ConfigurationChangedEventArgs

```csharp
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
```

### 2. User Configuration Models

#### UserConfiguration
User-scoped configuration values persisted to MySQL.

**Properties**:
- `UserId: int` - Foreign key to Users table
- `Preferences: Dictionary<string, object?>` - Key-value pairs (serialized to/from PreferenceValue column)

**Persistence**:
- Table: `UserPreferences` (MySQL)
- Columns: `PreferenceId`, `UserId`, `PreferenceKey`, `PreferenceValue` (TEXT, JSON serialized), `LastUpdated`

#### UserFolderConfiguration
Dynamic network/local path resolution with fallback strategy.

**Properties**:
- `NetworkDrivePath: string?` - Admin-configured central server path (e.g., `\\\\server\\share\\MTM_Apps\\users`)
- `LocalFallbackPath: string` - Local directory (default: `{MyDocuments}\\MTM_Apps\\users`)
- `HomeDevelopmentIPAddress: string` - IP for home dev detection (default: `73.94.78.172`)
- `NetworkAccessTimeoutSeconds: int` - Network check timeout (default: 2)
- `EnableDualWrite: bool` - Write to both network and local (default: false)
- `LocationCacheDurationMinutes: int` - Cache location decision (default: 5)

**Resolution Logic**:
1. Check public IP (via external service) → if matches HomeDevelopmentIPAddress, use LocalFallbackPath only
2. Attempt NetworkDrivePath access with timeout
3. If network unavailable → use LocalFallbackPath
4. If EnableDualWrite → write to both locations
5. Cache location decision for LocationCacheDurationMinutes

**Source**: `config/user-folders.json` (JSON configuration file)

#### DatabaseSchemaConfiguration
JSON schema loader and validator for MySQL database structure.

**Properties**:
- `Tables: List<TableSchema>` - Table definitions
- `Version: string` - Schema version (semantic versioning)
- `LastUpdated: DateTime` - Schema file modification timestamp

**TableSchema**:
- `Name: string` - Table name (case-sensitive: `Users`, `UserPreferences`, `FeatureFlags`)
- `Columns: List<ColumnSchema>` - Column definitions
- `Indexes: List<IndexSchema>` - Index definitions
- `ForeignKeys: List<ForeignKeySchema>` - Relationships

**ColumnSchema**:
- `Name: string` - Column name (case-sensitive: `UserId`, `PreferenceKey`)
- `Type: string` - MySQL type (VARCHAR(100), INT, DATETIME, BOOLEAN, TEXT)
- `Nullable: bool` - NULL constraint
- `Default: string?` - Default value
- `Description: string` - Column purpose

**Source**: `config/database-schema.json` (developer edits placeholders before setup)

### 3. Secrets Management

#### ISecretsService (Interface)
Platform-agnostic credential storage contract.

**Methods**:
- `Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default)` - Store credential
- `Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)` - Retrieve credential (null if not exists, throws if storage unavailable)
- `Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)` - Remove credential

**Implementations**:
- `WindowsSecretsService` - Windows DPAPI (Data Protection API) via `CredentialManager`
- `AndroidSecretsService` - Android KeyStore API (hardware-backed when available)

**Factory**:
- `SecretsServiceFactory.Create(ILoggerFactory)` - Platform detection + appropriate implementation

**Platform Support**:
- ✅ Windows Desktop: `WindowsSecretsService`
- ✅ Android: `AndroidSecretsService`
- ❌ macOS, Linux, iOS: Throw `PlatformNotSupportedException`

#### WindowsSecretsService
Windows-specific credential storage using DPAPI.

**Implementation Details**:
- Uses `System.Security.Cryptography.DataProtectionScope.CurrentUser`
- Credentials stored in Windows Credential Manager
- Hardware-backed encryption via TPM when available
- Per-user isolation (credentials not shared across Windows accounts)

**Error Handling**:
- Storage unavailable → Throw `InvalidOperationException` with user-friendly message
- User cancels credential dialog → Application closes (FR-013)

#### AndroidSecretsService
Android-specific credential storage using KeyStore API.

**Implementation Details**:
- Uses `Android.Security.Keystore.KeyStore`
- Hardware-backed encryption when device supports it (Trusted Execution Environment)
- Per-application isolation (credentials not shared across apps)
- Requires device unlock (biometric or PIN)

**Error Handling**:
- KeyStore unavailable → Throw `InvalidOperationException` with user-friendly message
- Device lock screen required → Prompt user to enable device security

### 4. Feature Flags

#### FeatureFlagEvaluator
Feature flag registration, evaluation, and runtime updates with rollout percentages.

**Properties**:
- `_flags: Dictionary<string, FeatureFlag>` - Registered flags
- `_currentEnvironment: EnvironmentType` - Detected environment
- `_lockObject: object` - Thread-safety lock

**Methods**:
- `void RegisterFlag(FeatureFlag flag)` - Register flag with validation
- `Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default)` - Check if enabled (deterministic per user)
- `Task SetEnabledAsync(string flagName, bool enabled, CancellationToken cancellationToken = default)` - Update flag state
- `Task LoadFlagsFromDatabaseAsync(CancellationToken cancellationToken = default)` - Load from MySQL

**Rollout Percentage Logic**:

```csharp
// Deterministic per user (same user always sees same result)
int userHash = Math.Abs(userId.GetHashCode()) % 100;
bool enabled = userHash < flag.RolloutPercentage;
```

**Validation Rules**:
- Flag names MUST match regex: `^[a-zA-Z0-9._-]+$` (letters, numbers, dots, underscores, hyphens)
- Invalid names → Throw `ArgumentException` with clear error message
- Unregistered flags → Treat as disabled, log warning (NFR-013)

#### FeatureFlag
Feature flag data structure.

**Properties**:
- `Name: string` - Flag identifier (e.g., `Visual.UseForItems`)
- `IsEnabled: bool` - Global enable/disable state
- `Environment: EnvironmentType` - Target environment (Development, Staging, Production)
- `RolloutPercentage: int` - Percentage 0-100 (0 = disabled for all, 100 = enabled for all)
- `Description: string?` - Human-readable purpose

**Core Feature Flags** (from FR-018):
- `Visual.UseForItems` - Use Visual ERP for item master data
- `Visual.UseForLocations` - Use Visual ERP for location data
- `Visual.UseForWorkCenters` - Use Visual ERP for work center data
- `OfflineModeAllowed` - Enable offline operation mode
- `Printing.Enabled` - Enable printing functionality

**Persistence**:
- Table: `FeatureFlags` (MySQL)
- Columns: `FlagId`, `FlagName`, `IsEnabled`, `Environment`, `RolloutPercentage`, `Description`, `CreatedDate`, `LastModified`
- Indexes: INDEX on `FlagName`, `Environment`, `IsEnabled`

**Synchronization** (CL-010):
- Changes on server applied only at application launch
- MTM_Application_Launcher detects version mismatch → updates local copy
- Running applications do NOT sync flags in real-time

## Database Schema (MAMP MySQL 5.7)

### Users Table (if not exists)

```sql
CREATE TABLE IF NOT EXISTS Users (
    UserId INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate DATETIME NOT NULL,
    LastLoginDate DATETIME,
    INDEX idx_username (Username),
    INDEX idx_email (Email),
    INDEX idx_isactive (IsActive)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Purpose**: Store user account information for application access

### UserPreferences Table

```sql
CREATE TABLE IF NOT EXISTS UserPreferences (
    PreferenceId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    PreferenceKey VARCHAR(100) NOT NULL,
    PreferenceValue TEXT,
    LastUpdated DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    UNIQUE INDEX idx_user_key (UserId, PreferenceKey),
    INDEX idx_preferencekey (PreferenceKey)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Purpose**: Store user-specific application settings (display preferences, filters, UI customization)

### FeatureFlags Table

```sql
CREATE TABLE IF NOT EXISTS FeatureFlags (
    FlagId INT PRIMARY KEY AUTO_INCREMENT,
    FlagName VARCHAR(100) NOT NULL UNIQUE,
    IsEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    Environment VARCHAR(50) NOT NULL,
    RolloutPercentage INT NOT NULL DEFAULT 0,
    Description TEXT,
    CreatedDate DATETIME NOT NULL,
    LastModified DATETIME NOT NULL,
    INDEX idx_flagname (FlagName),
    INDEX idx_environment (Environment),
    INDEX idx_isenabled (IsEnabled)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Purpose**: Store feature flag configurations with environment-specific settings

**Important**: All schema changes MUST update `.github/mamp-database/schema-tables.json` and increment version in `migrations-history.json`.

## Validation Rules Summary

### Configuration Keys
- Format: Hierarchical namespace with colon separator (e.g., `API:BaseUrl`, `Visual:Username`)
- Case-sensitive
- No validation on key existence (return default value if not found)

### Feature Flag Names
- Regex: `^[a-zA-Z0-9._-]+$`
- Supports hierarchical organization: `Visual.UseForItems`
- Supports naming conventions: kebab-case, snake_case, dot.notation

### Environment Variables
- Format: Underscore separator (e.g., `MTM_ENVIRONMENT`, `MTM_API_TIMEOUT`)
- OS compatibility (Windows, Android)
- Read once at startup only

### Rollout Percentages
- Integer range: 0-100
- 0 = disabled for all users
- 100 = enabled for all users
- Deterministic per user (hash-based)

## State Transitions

### Configuration Value Lifecycle

```
[Not Set] → GetValue (returns default)
          ↓
[Set via SetValue] → Triggers OnConfigurationChanged event (if value differs)
                   → Persists to MySQL (UserPreferences table)
          ↓
[Persisted] → Available across application restarts
```

### Feature Flag Lifecycle

```
[Unregistered] → IsEnabledAsync returns false + logs warning
               ↓
[Registered] → Evaluation based on IsEnabled + Environment + RolloutPercentage
             ↓
[Enabled/Disabled] → SetEnabledAsync updates state → Persists to MySQL
```

### Credential Lifecycle

```
[Not Stored] → GetSecretAsync returns null
             ↓
[Stored via SetSecretAsync] → Encrypted in OS-native storage (DPAPI/KeyStore)
                             ↓
[Retrieved via GetSecretAsync] → Decrypted and returned
                                ↓
[Deleted via DeleteSecretAsync] → Removed from OS-native storage
```

## Error Scenarios

### Configuration Errors
- **Missing key**: Return default value (no exception)
- **Invalid type**: Throw `InvalidCastException` with clear message
- **Database unavailable**: In-memory operation only, log warning

### Secrets Errors
- **Storage unavailable**: Show user-friendly dialog prompting credential re-entry
- **Dialog cancellation**: Application closes with clear warning (FR-013)
- **Unsupported platform**: Throw `PlatformNotSupportedException`

### Feature Flag Errors
- **Unregistered flag**: Treat as disabled, log warning (FR-021)
- **Invalid flag name**: Throw `ArgumentException` during RegisterFlag
- **Database unavailable**: Use in-memory flag state only, log warning

## Thread Safety

All services implement thread-safe operations:
- **ConfigurationService**: Uses `lock` statement around dictionary operations
- **FeatureFlagEvaluator**: Uses `lock` statement around flag registry
- **SecretsService implementations**: OS-native storage APIs are inherently thread-safe

## Performance Considerations

- **Configuration lookups**: O(1) dictionary access, <10ms target
- **Feature flag evaluation**: O(1) dictionary lookup + hash calculation, <5ms target
- **Credential retrieval**: OS-native API call, <100ms target
- **Change events**: Event dispatch, <50ms target
- **Database queries**: Indexed lookups on `UserId`, `PreferenceKey`, `FlagName`

## References

- Feature Specification: `spec.md`
- Research Findings: `research.md`
- Constitution: `.specify/memory/constitution.md` v1.3.0
- Database Documentation: `.github/mamp-database/schema-tables.json`
