# API Contracts: Environment and Configuration Management

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-05
**Status**: Complete - All contracts implemented

## Overview

This directory documents the public API contracts for the Environment and Configuration Management System. These contracts define the interfaces that consumers of this feature can depend on.

## Core Service Contracts

### 1. IConfigurationService

**Location**: `MTM_Template_Application/Services/Configuration/IConfigurationService.cs`

**Purpose**: Platform-agnostic configuration management with layered precedence

**Public Methods**:

```csharp
T? GetValue<T>(string key, T? defaultValue = default)
void SetValue(string key, object? value)
IConfigurationSection? GetSection(string sectionKey)
Task LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
Task SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken cancellationToken = default)
```

**Events**:

```csharp
event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged
```

**Configuration Precedence** (highest to lowest):
1. Environment Variables (MTM_*, ASPNETCORE_*, DOTNET_*)
2. User Configuration (runtime SetValue, persisted to MySQL)
3. Application Defaults (hard-coded fallbacks)

**Contract Tests**: `tests/contract/ConfigurationServiceContractTests.cs`

---

### 2. ISecretsService

**Location**: `MTM_Template_Application/Services/Secrets/ISecretsService.cs`

**Purpose**: Platform-agnostic credential storage using OS-native secure storage

**Public Methods**:

```csharp
Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default)
Task<string?> RetrieveSecretAsync(string key, CancellationToken cancellationToken = default)
Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
```

**Platform Implementations**:
- **Windows**: WindowsSecretsService (DPAPI via Credential Manager)
- **Android**: AndroidSecretsService (KeyStore with hardware-backed encryption)
- **Unsupported**: Throws `PlatformNotSupportedException`

**Factory**: `SecretsServiceFactory.Create(ILoggerFactory)` - Returns platform-appropriate implementation

**Contract Tests**: `tests/contract/SecretsServiceContractTests.cs`

---

### 3. FeatureFlagEvaluator (Concrete Class)

**Location**: `MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs`

**Purpose**: Feature flag evaluation with deterministic rollout percentages

**Public Methods**:

```csharp
void RegisterFlag(FeatureFlag flag)
Task<bool> IsEnabledAsync(string flagName, string? userId = null, CancellationToken cancellationToken = default)
Task SetEnabledAsync(string flagName, bool isEnabled, CancellationToken cancellationToken = default)
Task RefreshFlagsAsync(CancellationToken cancellationToken = default)
List<FeatureFlag> GetAllFlags()
```

**Behavior**:
- Unregistered flags return `false` (disabled)
- Rollout percentage uses deterministic hash-based evaluation (userId dependent)
- Flags loaded from MySQL at startup, cached in-memory
- Environment-specific flags filter by current environment

**Design Decision**: No separate interface - using concrete class directly (simpler, still DI-compatible)

**Contract Tests**: `tests/contract/FeatureFlagEvaluatorContractTests.cs`

---

## Data Models

### FeatureFlag

```csharp
public class FeatureFlag
{
    public int FlagId { get; set; }
    public string FlagName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = false;
    public string? Environment { get; set; } // null = all environments
    public int RolloutPercentage { get; set; } = 0; // 0-100
    public string? AppVersion { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### ConfigurationSetting

```csharp
public class ConfigurationSetting
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Source { get; set; } = string.Empty; // "Environment", "User", "Default", "Database"
    public int Precedence { get; set; } // Higher = wins
    public bool IsEncrypted { get; set; } = false;
}
```

### ConfigurationChangedEventArgs

```csharp
public class ConfigurationChangedEventArgs : EventArgs
{
    public string Key { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}
```

---

## Database Contracts

### Users Table

**Schema**: See `.github/mamp-database/schema-tables.json`

```sql
CREATE TABLE Users (
    UserId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    DisplayName VARCHAR(255),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME
);
```

### UserPreferences Table

```sql
CREATE TABLE UserPreferences (
    PreferenceId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    PreferenceKey VARCHAR(255) NOT NULL,
    PreferenceValue TEXT,
    Category VARCHAR(100),
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY (UserId, PreferenceKey),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
```

### FeatureFlags Table

```sql
CREATE TABLE FeatureFlags (
    FlagId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    FlagName VARCHAR(255) NOT NULL UNIQUE,
    IsEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    Environment VARCHAR(50),
    RolloutPercentage INT NOT NULL DEFAULT 0,
    AppVersion VARCHAR(50),
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CHECK (RolloutPercentage BETWEEN 0 AND 100) -- MySQL 5.7: not enforced, app-level validation required
);
```

---

## Integration Points

### Dependency Injection Registration

```csharp
// Configuration
services.AddConfigurationServices();
// Registers: IConfigurationService -> ConfigurationService

// Secrets (platform-specific)
services.AddSecretsServices(SecretsServiceFactory.Create(loggerFactory));
// Registers: ISecretsService -> WindowsSecretsService or AndroidSecretsService

// Feature Flags
services.AddSingleton<FeatureFlagEvaluator>();
// Registered in AddConfigurationServices()
```

### Environment Variable Naming Convention

**App-Specific Prefix**: `MTM_`

Examples:
- `MTM_ENVIRONMENT` = Development | Staging | Production
- `MTM_DATABASE_SERVER` = MySQL connection string override
- `MTM_VISUAL_API_BASE_URL` = Visual ERP API endpoint
- `MTM_FEATURE_FLAG_OFFLINE_MODE` = true | false

**Precedence**: `MTM_*` > `ASPNETCORE_*` > `DOTNET_*` > Build Config > Defaults

---

## Performance Targets

**Measured in contract tests**:

- **Configuration Retrieval**: <100ms for GetValue<T>() with 50+ keys
- **Credential Retrieval**: <200ms for RetrieveSecretAsync() (Windows/Android)
- **Feature Flag Evaluation**: <5ms for IsEnabledAsync() (in-memory cache)

---

## Error Handling Contracts

### Severity-Based Error Notifications

```csharp
public enum ErrorSeverity
{
    Info,       // Status bar notification
    Warning,    // Status bar with click-for-details
    Critical    // Modal dialog blocking interaction
}
```

### Graceful Degradation

**Database Unavailable**:
- Configuration: Falls back to environment variables → defaults
- User Preferences: Uses last-known cached values
- Feature Flags: Uses last-known cached values (persisted to local JSON)

**Credential Storage Unavailable**:
- Prompts user for credential re-entry with clear explanation
- Attempts to re-save to OS storage
- If user cancels: Application exits with warning message

---

## Breaking Changes Policy

**Version**: 1.0.0 (Feature 002)

**Semantic Versioning**:
- **MAJOR**: Breaking contract changes (method removal, signature changes)
- **MINOR**: New methods, optional parameters (backward compatible)
- **PATCH**: Bug fixes, internal refactoring (no contract changes)

**Current Stability**: ✅ **STABLE** - All contracts finalized for Feature 002

---

## Testing Requirements

All contract implementations MUST:
1. Pass contract tests in `tests/contract/`
2. Pass integration tests in `tests/integration/`
3. Meet performance targets (measured in PerformanceTests.cs)
4. Handle graceful degradation scenarios
5. Support cancellation tokens on all async methods
6. Use nullable reference types correctly (no null warnings)

---

## Related Documentation

- **Technical Specification**: [../spec.md](../spec.md)
- **Implementation Plan**: [../plan.md](../plan.md)
- **Task Breakdown**: [../tasks.md](../tasks.md)
- **Data Model**: [../data-model.md](../data-model.md)
- **Database Schema**: `.github/mamp-database/schema-tables.json`
- **Contract Tests**: `tests/contract/` (ConfigurationService, SecretsService, FeatureFlagEvaluator)

---

**Last Updated**: 2025-10-05
**Status**: ✅ All contracts implemented and tested
**Next Review**: After Feature 003 completion
