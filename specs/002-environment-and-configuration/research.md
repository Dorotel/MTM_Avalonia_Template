# Research: Environment and Configuration Management System

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-05
**Status**: Complete

---

## Research Summary

This feature extends the partially implemented configuration system from Boot Feature 001. Core infrastructure already exists (ConfigurationService, SecretsService, FeatureFlagEvaluator), but several key capabilities need enhancement based on clarifications from stakeholders.

---

## Technology Decisions

### 1. Configuration Persistence Strategy

**Decision**: Dual-storage approach using JSON configuration file + MySQL database

**Rationale**:
- **JSON config for paths**: Admin-configured central server path stored in `config/user-folders.json` with placeholder values (defaults to MyDocuments until finalized)
- **MySQL for preferences**: User preferences (display settings, sort orders, filters) stored in database using schema defined in `config/database-schema.json`
- Separation of concerns: File-based for admin infrastructure settings, database for dynamic user data
- Aligns with existing MySQL architecture (MAMP 5.7)

**Alternatives Considered**:
- Single JSON file storage: Rejected due to poor multi-user concurrency and no server-side management
- appsettings.json integration: Rejected because .NET Configuration API doesn't support runtime writes well
- SQLite local storage: Rejected to maintain consistency with MySQL stack

**Implementation Notes**:
- Create `config/` folder in repository root
- `user-folders.json`: Single JSON file with `{"centralServerPath": "PLACEHOLDER_REPLACE_ME", "defaultPath": "%MyDocuments%/MTM_App"}`
- `database-schema.json`: Full schema definition with tables, columns, types, constraints
- Developer replaces placeholders before first run

---

### 2. Credential Dialog for Recovery

**Decision**: Implement Avalonia dialog with Material Design styling

**Rationale**:
- When OS-native storage fails (corrupted keychain, revoked permissions), user needs recovery path
- Material Design provides familiar, professional UI patterns
- Avalonia's Window/Dialog system supports both platforms (Windows Desktop + Android)
- Consistent with Theme V2 semantic tokens

**Alternatives Considered**:
- Console prompt: Rejected (not suitable for GUI application)
- Platform-native dialogs: Rejected (breaks cross-platform consistency)
- Error message only: Rejected (no recovery path for user)

**Implementation Requirements**:
- `CredentialDialogView` (AXAML) with username/password fields
- `CredentialDialogViewModel` with `[ObservableProperty]` and `[RelayCommand]`
- Show dialog when `RetrieveSecretAsync` throws `CryptographicException` or `UnauthorizedAccessException`
- Re-attempt storage after user provides credentials
- Support keyboard navigation and screen readers (accessibility)

---

### 3. Configuration Error Notification System

**Decision**: Severity-based notification approach (status bar vs modal dialog)

**Rationale**:
- Non-critical errors (display preferences, timeouts) shouldn't block workflow
- Critical errors (credentials, database connection) must be resolved before continuing
- Status bar indicator is non-intrusive for minor issues
- Modal dialog ensures critical issues are addressed

**Per Clarification Q3 and Q9**:
- **Non-critical settings**: Show warning icon in status bar, click for details (toast notification style)
- **Critical settings**: Show modal dialog blocking UI until corrected

**Implementation Components**:
- `ErrorNotificationService` singleton with severity enum (Info, Warning, Critical)
- Status bar integration in `MainView.axaml` (existing UI element)
- `ConfigurationErrorDialog` for critical issues
- Structured logging for all config errors (Serilog)

---

### 4. Visual API Toolkit Command Whitelist

**Decision**: Store whitelist in `appsettings.json` under `Visual:AllowedCommands` array

**Rationale**:
- Configuration file is appropriate for deploy-time settings
- JSON array format is easy to edit and validate
- Existing .NET Configuration API reads it automatically
- Whitelist approach (explicit allow-list) is safer than blocklist

**Whitelist Strategy** (per Clarification Q8):
- Include **only read-only commands** (any command that does NOT write data)
- Block all write operations explicitly
- Required citation format: "Reference-{File Name} - {Chapter/Section/Page}"

**Example appsettings.json**:

```json
{
  "Visual": {
    "AllowedCommands": [
      "GET_PART_DETAILS",
      "LIST_INVENTORY",
      "QUERY_WORK_ORDERS",
      "READ_BOM_STRUCTURE"
    ],
    "RequireCitation": true
  }
}
```

---

### 5. Feature Flag Synchronization Strategy

**Decision**: Tie feature flag updates to MTM_Application_Launcher version checks (next launch only)

**Rationale** (per Clarification Q10):
- Feature flags are tied to application version deployments for predictability
- Launcher already checks for version mismatches and prompts for updates
- No real-time synchronization prevents mid-session behavior changes
- Stable, consistent user experience within a session

**Implementation Notes**:
- Feature flags stored in database (tied to version)
- Launcher checks server version, downloads updated config if mismatch detected
- User prompted to update; on acceptance, new flags take effect on next launch
- Running applications do NOT poll for flag changes

---

### 6. Android Platform Database Connection

**Decision**: Direct MySQL connection (same as Windows Desktop)

**Rationale** (per Clarification Q7):
- Application is not public-facing (internal manufacturing use)
- No need for connection pool limits or API server intermediary
- Simplifies architecture (single code path for both platforms)
- Leverages existing MySQL infrastructure

**Security Considerations**:
- Use OS-native credential storage (Android KeyStore)
- Connection string includes encryption settings
- Network-level security (VPN, firewall rules) managed by IT department

**Implementation Notes**:
- Shared `DatabaseService` in `.Core` project
- Platform-specific connection string resolution (via ConfigurationService)
- Same MySQL.Data NuGet package for both platforms

---

## Best Practices Research

### Configuration Precedence Patterns

**Standard Precedence Order** (from .NET Core configuration best practices):
1. Environment Variables (highest priority)
2. User Configuration (runtime-set values)
3. Application Defaults (code-defined fallback)

**Thread Safety**:
- Use `lock` statement for configuration dictionary access
- Event dispatching on background thread (avoid blocking UI)
- ConfigurationChangedEventArgs immutable record

**Error Handling**:
- Never throw on missing keys; return default value
- Log all configuration errors with structured data
- Redact sensitive keys (password, token, secret, credential)

---

### OS-Native Secure Storage Patterns

**Windows DPAPI** (Data Protection API):
- Use `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- Store in Windows Credential Manager via `CredentialManager` NuGet package
- Hardware-backed encryption when TPM available

**Android KeyStore**:
- Use `KeyStore` class with `AndroidKeyStore` provider
- Hardware-backed encryption on modern devices (API 23+)
- Fallback to software-backed on older devices

**Common Error Scenarios**:
- `CryptographicException`: Storage corrupted → prompt for re-entry
- `UnauthorizedAccessException`: Permissions revoked → show explanation + prompt
- `PlatformNotSupportedException`: Unsupported OS → clear error message

---

### Feature Flag Evaluation Patterns

**Deterministic Rollout** (per FR-023):
- Current implementation uses `Random.Next()` (non-deterministic)
- **Need to change**: Use hash of user ID + flag name for consistent results
- Formula: `hash(userId + flagName) % 100 < rolloutPercentage`
- Same user always sees same flag state for given percentage

**Environment-Specific Flags**:
- Check current environment (MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → build config)
- If flag has environment constraint and doesn't match, return false
- Log environment mismatches for troubleshooting

---

## Database Schema Design

**UserPreferences Table**:

```sql
CREATE TABLE UserPreferences (
    PreferenceId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    PreferenceKey VARCHAR(255) NOT NULL,
    PreferenceValue TEXT,
    Category VARCHAR(100), -- e.g., "Display", "Filters", "Sort"
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY UK_UserPreferences (UserId, PreferenceKey),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
```

**Users Table** (if not exists):

```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(100) NOT NULL UNIQUE,
    DisplayName VARCHAR(255),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME
);
```

**FeatureFlags Table** (for launcher sync):

```sql
CREATE TABLE FeatureFlags (
    FlagId INT PRIMARY KEY AUTO_INCREMENT,
    FlagName VARCHAR(255) NOT NULL UNIQUE,
    IsEnabled BOOLEAN NOT NULL DEFAULT FALSE,
    Environment VARCHAR(50), -- NULL for all environments
    RolloutPercentage INT NOT NULL DEFAULT 0,
    AppVersion VARCHAR(50), -- Tied to app version
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

**Schema stored in**: `config/database-schema.json` (editable by developer before setup)

---

## Performance Considerations

### Configuration Caching
- In-memory dictionary for fast lookup (<10ms target per NFR-001)
- Thread-safe read/write with `ReaderWriterLockSlim` (upgrade from `lock`)
- Lazy loading for environment variables (parse once, cache forever)

### Credential Retrieval Optimization
- OS-native storage is already fast (<100ms per NFR-002)
- Cache credentials in memory after first retrieval (with TTL of 1 hour)
- Clear cache on logout or credential rotation

### Feature Flag Evaluation
- In-memory dictionary lookup (<5ms per NFR-003)
- Pre-compute hash-based rollout during registration (deterministic approach)
- No database calls for flag evaluation (load once at startup)

---

## Testing Strategy

### Unit Tests (xUnit + NSubstitute)
- `ConfigurationServiceTests`: Precedence, thread safety, event notification
- `FeatureFlagEvaluatorTests`: Deterministic rollout, environment filtering
- `WindowsSecretsServiceTests`: Mock DPAPI, test error scenarios
- `AndroidSecretsServiceTests`: Mock KeyStore, test hardware fallback

### Integration Tests
- `ConfigurationPersistenceTests`: Round-trip to MySQL database
- `CredentialRecoveryTests`: Dialog flow, re-storage after failure
- `FeatureFlagSyncTests`: Launcher version mismatch, flag update on launch

### Contract Tests
- `ConfigurationServiceContract`: GetValue/SetValue schema validation
- `SecretsServiceContract`: StoreSecret/RetrieveSecret encryption verification
- `FeatureFlagContract`: IsEnabledAsync deterministic behavior

---

## Unresolved Questions

All clarification questions from spec.md have been resolved. No remaining unknowns.

---

## Next Steps

1. ✅ Research complete
2. → Phase 1: Create data-model.md, contracts/, quickstart.md
3. → Phase 2: Task generation strategy (describe in plan.md, execute in /tasks command)

---

**Last Updated**: 2025-10-05
