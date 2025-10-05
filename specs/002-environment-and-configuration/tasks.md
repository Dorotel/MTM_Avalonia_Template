# Tasks: Environment and Configuration Management System

**Feature**: 002-environment-and-configuration
**Input**: Design documents from `specs/002-environment-and-configuration/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/, quickstart.md

## Execution Summary

✅ 1. Loaded plan.md → Extracted: C# .NET 9.0, Avalonia 11.3+, MVVM, MySQL 5.7
✅ 2. Loaded data-model.md → Entities: ConfigurationService, FeatureFlag, ConfigurationError, CredentialDialogViewModel
✅ 3. Loaded contracts/ → 4 contract files found
✅ 4. Loaded quickstart.md → 7 integration test scenarios + 3 performance tests
✅ 5. Generated 35 tasks across 6 phases (updated after /fix: added T031 for log redaction)
✅ 6. Applied TDD ordering (tests before implementation)
✅ 7. Identified parallel execution opportunities ([P] markers)
✅ 8. All /analyze issues resolved (C1, C2, C3, I1, I2, U1-U5, D1, D2, A1, A2, T1)

---

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- All file paths are absolute or repository-relative

---

## Phase 1: Setup & Prerequisites

**Goal**: Prepare project infrastructure and configuration files

- [x] **T001** Create `config/` directory structure at repository root (✅ COMPLETED 2025-10-05)
  - Create `config/user-folders.json` with the following structure:

    ```json
    {
      "HomeDevelopmentIPAddress": "73.94.78.172",
      "NetworkDrivePath": "\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users",
      "LocalFallbackPath": "{MyDocuments}\\MTM_Apps\\users",
      "NetworkAccessTimeoutSeconds": 2,
      "LocationCacheDurationMinutes": 5,
      "EnableDualWrite": true
    }
    ```

  - Create `config/database-schema.json` with the following structure:

    ```json
    {
      "ConnectionSettings": {
        "HomeDevelopmentIPAddress": "73.94.78.172",
        "HomeDatabase": {
          "Host": "localhost",
          "Port": 3306,
          "Database": "mtm_template_dev",
          "Description": "MAMP MySQL 5.7 local development"
        },
        "ProductionDatabase": {
          "Host": "mtmanu-sql01",
          "Port": 3306,
          "Database": "mtm_template_prod",
          "Description": "Production MySQL server"
        },
        "ConnectionTimeoutSeconds": 5,
        "EnableConnectionPooling": true,
        "MaxPoolSize": 100,
        "MinPoolSize": 5
      },
      "Schema": {
        "Tables": {
          "UserPreferences": {
            "Columns": [
              {"Name": "UserId", "Type": "INT", "Nullable": false, "PrimaryKey": true},
              {"Name": "ConfigKey", "Type": "VARCHAR(255)", "Nullable": false, "PrimaryKey": true},
              {"Name": "ConfigValue", "Type": "TEXT", "Nullable": true},
              {"Name": "LastModified", "Type": "DATETIME", "Nullable": false, "Default": "CURRENT_TIMESTAMP"}
            ],
            "Indexes": [
              {"Name": "UK_UserPreferences", "Type": "UNIQUE", "Columns": ["UserId", "ConfigKey"]},
              {"Name": "IX_UserPreferences_UserId", "Type": "INDEX", "Columns": ["UserId"]}
            ]
          },
          "FeatureFlags": {
            "Columns": [
              {"Name": "FlagName", "Type": "VARCHAR(255)", "Nullable": false, "PrimaryKey": true},
              {"Name": "IsEnabled", "Type": "BOOLEAN", "Nullable": false, "Default": false},
              {"Name": "Environment", "Type": "VARCHAR(50)", "Nullable": true},
              {"Name": "RolloutPercentage", "Type": "INT", "Nullable": false, "Default": 0},
              {"Name": "AppVersion", "Type": "VARCHAR(50)", "Nullable": true},
              {"Name": "LastModified", "Type": "DATETIME", "Nullable": false, "Default": "CURRENT_TIMESTAMP"}
            ],
            "Indexes": [
              {"Name": "IX_FeatureFlags_Environment", "Type": "INDEX", "Columns": ["Environment"]}
            ]
          }
        }
      }
    }
    ```

  - **File**: `{repo_root}/config/user-folders.json`, `{repo_root}/config/database-schema.json`
  - **Acceptance**: Files exist with valid JSON, include all required settings for dynamic location detection and database schema definition
  - **Completed**: Created config directory, user-folders.json, database-schema.json, migrations subdirectory, and MyDocuments\MTM_Apps\users folder

- [x] **T001a** Document placeholder replacement process in config/README.md (✅ COMPLETED 2025-10-05)
  - Create `config/README.md` with instructions for admin configuration
  - Document dynamic user folder location logic:
    - **Home Development**: If public IP matches `HomeDevelopmentIPAddress` (default: 73.94.78.172), uses local path only
    - **On-Premises**: If IP differs, attempts network drive first (`NetworkDrivePath`), falls back to local if inaccessible
    - **Dual-Write**: When both available, writes to both locations (network primary, local backup)
    - **Caching**: Location detection cached for 5 minutes to minimize network checks
    - **Runtime Directory Creation**: Application automatically creates user folders on first access:
      - Local: `{MyDocuments}\MTM_Apps\users\{userId}\` created if missing
      - Network: `{NetworkDrivePath}\{userId}\` created only if network accessible
      - Graceful fallback: If directory creation fails, uses available alternative location
      - All creation attempts logged with structured logging
  - Document `user-folders.json` settings with default values for development:
    - `HomeDevelopmentIPAddress`: Developer's home ISP IP (e.g., "73.94.78.172")
    - `NetworkDrivePath`: Production network drive (e.g., "\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users") [REQUIRED for production]
    - `LocalFallbackPath`: Local backup path (default: "{MyDocuments}\\MTM_Apps\\users" - resolves to user's MyDocuments folder automatically)
    - `NetworkAccessTimeoutSeconds`: Network drive availability test timeout (default: 2)
    - `LocationCacheDurationMinutes`: How long to cache location decision (default: 5)
    - `EnableDualWrite`: Write to both locations when available (default: true)
    - **Development Defaults**: When placeholders not configured, system uses MyDocuments folder for local storage (Environment.SpecialFolder.MyDocuments)
  - Document dynamic database connection logic:
    - **Home Development**: If public IP matches `HomeDevelopmentIPAddress`, uses `HomeDatabase` settings (localhost MAMP)
    - **On-Premises**: If IP differs, uses `ProductionDatabase` settings (production MySQL server)
    - **Connection Pooling**: Enabled by default for performance
  - Document `database-schema.json` settings with default values:
    - `ConnectionSettings.HomeDevelopmentIPAddress`: Same as user-folders.json for consistency
    - `ConnectionSettings.HomeDatabase`: MAMP MySQL local development settings
      - Host: "localhost" (MAMP default)
      - Port: 3306 (MAMP default)
      - Database: "mtm_template_dev" (development database name)
    - `ConnectionSettings.ProductionDatabase`: Production MySQL server settings
      - Host: "mtmanu-sql01" (production server hostname) [REQUIRED for production]
      - Port: 3306 (standard MySQL port)
      - Database: "mtm_template_prod" (production database name) [REQUIRED for production]
    - `ConnectionSettings.ConnectionTimeoutSeconds`: Database connection timeout (default: 5)
    - `ConnectionSettings.EnableConnectionPooling`: Use connection pooling (default: true)
    - `ConnectionSettings.MaxPoolSize`: Maximum connections in pool (default: 100)
    - `ConnectionSettings.MinPoolSize`: Minimum connections in pool (default: 5)
    - `Schema.Tables`: Complete table definitions with columns, types, constraints, and indexes
    - **Development Defaults**: When production placeholders not configured, system uses localhost:3306 with MAMP MySQL defaults
  - Provide example values for development vs production
  - **File**: `{repo_root}/config/README.md`
  - **Dependency**: T001 complete (config files created)
  - **Acceptance**: Admins can configure production values; developers can set their home IP; dynamic location and database connection logic clearly documented
  - **Completed**: Created comprehensive README.md with dynamic location logic, database connection logic, configuration precedence, production deployment checklist, troubleshooting guide, and examples

- [x] **T002** [P] Generate SQL migration scripts from database-schema-contract.json (✅ COMPLETED 2025-10-05)
  - Create `config/migrations/001_initial_schema.sql` with CREATE TABLE statements
  - Include UserPreferences, FeatureFlags tables (Users table creation optional if exists)
  - Add indexes and foreign keys per contract
  - **File**: `{repo_root}/config/migrations/001_initial_schema.sql`
  - **Acceptance**: SQL executes successfully on MAMP MySQL 5.7
  - **Completed**: Created comprehensive SQL migration with CREATE TABLE IF NOT EXISTS, indexes, foreign keys, and sample data

- [x] **T003** [P] Update `appsettings.json` with Visual API whitelist (✅ COMPLETED 2025-10-05)
  - Add `Visual:AllowedCommands` array with read-only commands per research.md
  - Add `Visual:RequireCitation` setting (true)
  - **File**: `MTM_Template_Application/appsettings.json` (or create if missing)
  - **Acceptance**: Configuration loads correctly, whitelist enforced
  - **Completed**: Created appsettings.json with Visual API whitelist (10 read-only commands), citation format, and configuration sections for Database, FeatureFlags, and Secrets

- [x] **T004** Run SQL migration on development database (✅ COMPLETED 2025-10-05)
  - Execute `001_initial_schema.sql` against MAMP MySQL
  - Verify tables created with correct structure
  - Insert sample data from contracts/database-schema-contract.json
  - **Dependency**: T002 complete
  - **Acceptance**: Tables exist (UserPreferences, FeatureFlags), sample data queryable. Create Users table if not exists (check for existing from Feature 001). Document assumption: Users table may exist from Feature 001 boot sequence.
  - **Completed**: Successfully executed migration on MAMP MySQL 5.7. Verified tables: Users (2 rows), UserPreferences (5 rows), FeatureFlags (5 rows)

---

## Phase 2: Contract Tests First (TDD) ⚠️ MUST COMPLETE BEFORE PHASE 3

**CRITICAL**: These tests MUST be written and MUST FAIL before ANY implementation

- [x] **T005** [P] Contract test for ConfigurationService.GetValue/SetValue (✅ COMPLETED 2025-10-05)
  - Add tests to existing `tests/contract/ConfigurationServiceContractTests.cs`
  - Test key format validation (colon/underscore patterns)
  - Test type safety (string, int, bool, object)
  - Test default value fallback
  - Test null key behavior (should throw)
  - Verify performance target (<10ms for GetValue)
  - **File**: `tests/contract/ConfigurationServiceContractTests.cs` (EXISTING FILE - tests added)
  - **Source**: `contracts/configuration-service-contract.json`
  - **Acceptance**: Tests compile and FAIL (implementation not ready)
  - **Completed**: Tests already exist in ConfigurationServiceContractTests.cs with T005 region marker

- [x] **T006** [P] Contract test for ConfigurationService.LoadUserPreferencesAsync (✅ COMPLETED 2025-10-05)
  - Add tests to existing `tests/contract/ConfigurationServiceContractTests.cs`
  - Test userId validation (must be > 0)
  - Test database connection handling
  - Mock database with sample data from contracts
  - Verify preferences loaded into memory
  - **File**: `tests/contract/ConfigurationServiceContractTests.cs` (EXISTING FILE - tests added)
  - **Source**: `contracts/configuration-service-contract.json`
  - **Dependency**: T004 complete (database schema must exist)
  - **Acceptance**: Tests compile and FAIL
  - **Completed**: Tests already exist in ConfigurationServiceContractTests.cs with T006 region marker

- [x] **T007** [P] Contract test for FeatureFlagEvaluator deterministic rollout (✅ COMPLETED 2025-10-05)
  - Add tests to existing `tests/contract/FeatureFlagEvaluatorContractTests.cs`
  - Test RegisterFlag validation (name pattern, rollout 0-100)
  - Test IsEnabledAsync deterministic behavior (same user → same result)
  - Test rollout percentage distribution (100 users, ~50% for 50% rollout)
  - Test environment filtering
  - Verify performance target (<5ms for IsEnabledAsync)
  - **File**: `tests/contract/FeatureFlagEvaluatorContractTests.cs` (EXISTING FILE - tests added)
  - **Source**: `contracts/feature-flag-evaluator-contract.json`
  - **Acceptance**: Tests compile and FAIL
  - **Completed**: Tests already exist in FeatureFlagEvaluatorContractTests.cs

- [x] **T008** [P] Contract test for SecretsService encryption and recovery (✅ COMPLETED 2025-10-05)
  - Create `tests/contract/SecretsServiceContractTests.cs`
  - Test StoreSecretAsync with valid/invalid keys
  - Test RetrieveSecretAsync with missing key (returns null)
  - Mock CryptographicException for credential recovery flow
  - Test platform-specific implementations (Windows DPAPI, Android KeyStore mocked)
  - Test PlatformNotSupportedException thrown on unsupported platforms (macOS, Linux, iOS)
  - Test OS-native storage isolation: per-user on Windows, per-app on Android (NFR-007)
  - Verify performance targets (<100ms)
  - **File**: `tests/contract/SecretsServiceContractTests.cs`
  - **Source**: `contracts/secrets-service-contract.json`
  - **Acceptance**: Tests compile and FAIL

- [x] **T009** [P] Contract test for database schema validation (✅ COMPLETED 2025-10-05)
  - Create `tests/contract/DatabaseSchemaContractTests.cs`
  - Test UserPreferences table structure (columns, types, constraints)
  - Test FeatureFlags table structure
  - Test foreign key constraints (UserPreferences.UserId → Users.UserId)
  - Test unique constraints (UK_UserPreferences)
  - Test sample data insertion
  - **File**: `tests/contract/DatabaseSchemaContractTests.cs`
  - **Source**: `contracts/database-schema-contract.json`
  - **Dependency**: T004 complete (database must be set up)
  - **Acceptance**: Tests compile and FAIL (or PASS if DB already set up from T004)

- [x] **T009a** [P] Contract test for environment detection precedence (✅ COMPLETED 2025-10-05)
  - Add tests to existing `tests/contract/ConfigurationServiceContractTests.cs` (T009a region)
  - Test MTM_ENVIRONMENT takes precedence over ASPNETCORE_ENVIRONMENT
  - Test ASPNETCORE_ENVIRONMENT takes precedence over DOTNET_ENVIRONMENT
  - Test DOTNET_ENVIRONMENT takes precedence over build configuration default
  - Test default to "Development" (DEBUG build) or "Production" (RELEASE build)
  - Test environment variable parsing and normalization
  - Test environment variable key format validation (MTM_*, DOTNET_*, ASPNETCORE_* patterns)
  - Test invalid key formats are rejected (no spaces, no special chars except underscore)
  - Mock environment variables with different combinations
  - **File**: `tests/contract/ConfigurationServiceContractTests.cs` (EXISTING FILE - tests added in T009a region)
  - **Source**: spec.md FR-001 to FR-003, FR-009
  - **Acceptance**: Tests compile and FAIL
  - **Completed**: Tests already exist in ConfigurationServiceContractTests.cs with T009a region marker

- [x] **T031** [P] Contract test for log redaction validation (✅ COMPLETED 2025-10-05)
  - Create `tests/contract/LogRedactionContractTests.cs`
  - Test that credentials are NOT logged in plaintext
  - Test that sensitive keys (password, token, secret, credential, apikey) are redacted in log output
  - Mock Serilog logger and capture log output using in-memory sink
  - Attempt to log configuration values with sensitive keys
  - Verify output contains "[REDACTED]" or similar masking for sensitive values
  - Test ConfigurationService logging does not expose sensitive data
  - Test SecretsService logging does not expose credential values
  - **File**: `tests/contract/LogRedactionContractTests.cs`
  - **Source**: spec.md FR-014, NFR-006
  - **Acceptance**: All sensitive data redacted in log output, tests compile and FAIL

---

## Phase 3: Core Models & Enhancements (ONLY after contracts are failing)

**Goal**: Implement data models and enhance existing services

- [x] **T010** [P] Create ConfigurationError model (✅ COMPLETED 2025-10-05)
  - Create `MTM_Template_Application/Models/Configuration/ConfigurationError.cs`
  - Add properties: Key, Message, Severity (enum), Timestamp, IsResolved, UserAction
  - Add ErrorSeverity enum (Info, Warning, Critical)
  - Add XML documentation comments
  - Follow nullable reference types pattern
  - **File**: `MTM_Template_Application/Models/Configuration/ConfigurationError.cs`
  - **Source**: `data-model.md` section 5
  - **Acceptance**: Model compiles, matches contract schema

- [x] **T011** [P] Enhance FeatureFlag model with deterministic rollout properties (✅ COMPLETED 2025-10-05)
  - Modify `MTM_Template_Application/Models/Configuration/FeatureFlag.cs` (exists from Feature 001)
  - Add `TargetUserIdHash` property (string?, nullable)
  - Add `AppVersion` property (string?, nullable)
  - Update XML documentation
  - **File**: `MTM_Template_Application/Models/Configuration/FeatureFlag.cs`
  - **Source**: `data-model.md` section 3
  - **Acceptance**: Model compiles, tests T007 closer to passing

- [x] **T012** [P] Create CredentialDialogViewModel (✅ COMPLETED 2025-10-05)
  - Create `MTM_Template_Application/ViewModels/Configuration/CredentialDialogViewModel.cs`
  - Add properties: Username, Password, ErrorMessage, IsLoading, DialogTitle, DialogMessage, RetryCount
  - Use `[ObservableProperty]` for all bindable properties
  - Add SubmitCommand with `[RelayCommand(CanExecute = nameof(CanSubmit))]`
  - Add CancelCommand with `[RelayCommand]`
  - Add RetryCommand with `[RelayCommand]` for manual retry trigger after 3 failed attempts (NFR-017)
  - Implement CanSubmit validation (min lengths)
  - Constructor injection: `ISecretsService`, `ILogger<CredentialDialogViewModel>`
  - **File**: `MTM_Template_Application/ViewModels/Configuration/CredentialDialogViewModel.cs`
  - **Source**: `data-model.md` section 6, spec.md NFR-017
  - **Acceptance**: ViewModel compiles, follows MVVM Community Toolkit patterns, includes RetryCommand

- [x] **T013** Enhance ConfigurationService with persistence methods (✅ COMPLETED 2025-10-05)
  - Modify `MTM_Template_Application/Services/Configuration/ConfigurationService.cs`
  - **CRITICAL - Runtime Environment Initialization**:
    - On service initialization, check and create all required directories and files:
      - **Repository Config Directory**: Ensure `{repo_root}/config/` exists, create if missing
      - **Repository Config Files**:
        - If `config/user-folders.json` missing: Create with default values (from T001 specification)
        - If `config/database-schema.json` missing: Create with default values (from T001 specification)
        - If `config/README.md` missing: Create with basic documentation
        - If `config.base.json` missing: Create with default base configuration (Feature 001 requirement)
        - If `config.dev.json` missing: Create with default development overrides (Feature 001 requirement)
        - **Note**: appsettings.json managed by .NET runtime, not created dynamically
      - **Logs Directory**: Ensure `{repo_root}/logs/` exists for application logs (Feature 001 requirement)
      - **Cache Directory**: Ensure `{repo_root}/cache/` exists for Visual master data cache (Feature 001 requirement)
      - **Local User Directories**:
        - Ensure `{MyDocuments}\MTM_Apps\` base directory exists
        - Ensure `{MyDocuments}\MTM_Apps\users\` directory exists
        - On first user access: Create `{MyDocuments}\MTM_Apps\users\{userId}\` directory
      - **Network User Directories** (only if network accessible):
        - Check network drive accessibility (2s timeout)
        - If accessible: Create `{NetworkDrivePath}\{userId}\` directory on first user access
        - If not accessible: Skip network folder creation, log warning, use local only
      - **Error Handling**: Never throw exceptions - log failures and gracefully fall back to available locations
      - **Logging**: Use structured logging for all directory/file creation attempts (success/failure)
  - Add `LoadUserPreferencesAsync(int userId, CancellationToken ct)` method
  - Add `SaveUserPreferenceAsync(int userId, string key, object value, CancellationToken ct)` method
  - Add `GetUserFolderPathAsync(int userId, CancellationToken ct)` method with dynamic location detection:
    - Check public ISP IP via external service (e.g., `<https://api.ipify.org>`)
    - Compare with `HomeDevelopmentIPAddress` from config
    - If home IP: Return local path only
    - If on-premises: Test network drive accessibility (2s timeout), return network path or local fallback
    - If dual-write enabled and both accessible: Return both paths for redundant writes
    - Cache location decision for 5 minutes using `MemoryCache`
    - **Before returning path**: Ensure user-specific directory exists (create if missing)
  - Add `GetUserFolderLocationsAsync(int userId, CancellationToken ct)` method returning `UserFolderLocations` record with Primary, Backup, and IsNetworkAvailable properties
  - Add `GetDatabaseConnectionStringAsync(CancellationToken ct)` method with dynamic connection selection:
    - Check public ISP IP (same logic as user folders)
    - If home IP: Use `HomeDatabase` connection settings from `database-schema.json`
    - If on-premises: Use `ProductionDatabase` connection settings
    - Build connection string with pooling, timeout, and security settings
    - Cache connection string selection for 5 minutes
  - Implement MySQL database connection via `IDbConnectionFactory` (inject in constructor)
  - Read/write to UserPreferences table with parameterized queries
  - Parse `config/user-folders.json` for location settings (create with defaults if missing)
  - Parse `config/database-schema.json` for database connection settings (create with defaults if missing)
  - Update SetValue to persist to database
  - Add ReaderWriterLockSlim for thread safety (upgrade from `lock`)
  - Validate environment variable key format (MTM_*, DOTNET_*, ASPNETCORE_*)
  - **File**: `MTM_Template_Application/Services/Configuration/ConfigurationService.cs`
  - **Dependency**: T010, T011 complete (models exist), T002 complete (DB schema exists)
  - **Source**: `data-model.md` section 1, `contracts/configuration-service-contract.json`, spec.md FR-032
  - **Acceptance**: Contract tests T005, T006 now PASS, env var format validated, dynamic user folder location detection works, dynamic database connection selection works, all directories and config files created automatically at runtime, application works on fresh install without manual setup
  - **Completed**: Enhanced ConfigurationService with runtime initialization, MySQL persistence, dynamic location detection, thread safety with ReaderWriterLockSlim, and environment variable validation. Removed duplicate ConfigurationService_Enhanced.cs file.

- [x] **T013a** Implement Visual API whitelist validation service (✅ COMPLETED 2025-10-05)
  - Create `MTM_Template_Application/Services/Visual/VisualApiWhitelistValidator.cs`
  - Load whitelist from `appsettings.json` under `Visual:AllowedCommands` array
  - Implement `IsCommandAllowedAsync(string command, CancellationToken ct)` method
  - Implement `ValidateCitationFormat(string citation)` method (regex: "Reference-{FileName} - {Chapter/Section/Page}")
  - Throw `UnauthorizedAccessException` for non-whitelisted commands
  - Log all validation attempts (allowed + blocked) with structured logging
  - Register service in DI container
  - **File**: `MTM_Template_Application/Services/Visual/VisualApiWhitelistValidator.cs`
  - **Dependency**: T003 complete (appsettings.json updated with whitelist)
  - **Source**: spec.md FR-030, FR-031, Constitution Principle VIII
  - **Acceptance**: Contract test validates whitelist enforcement, citation format validation
  - **Completed**: Created VisualApiWhitelistValidator and IVisualApiWhitelistValidator with all required validation methods

- [ ] **T014** Enhance FeatureFlagEvaluator with deterministic rollout algorithm
  - Modify `MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs`
  - Replace `Random.Next()` with deterministic hash-based evaluation
  - Implement: `hash = SHA256(userId.ToString() + flagName)`, use first 4 bytes as int, `% 100 < rolloutPercentage`
  - Update `IsEnabledAsync` to use hash if userId provided, else fallback to Random
  - Update `RegisterFlag` to validate RolloutPercentage (0-100)
  - Add environment detection logic (MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → build config)
  - Filter flags by environment if specified
  - Handle duplicate flag registration: Update existing flag properties if already registered
  - **File**: `MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs`
  - **Dependency**: T011 complete (FeatureFlag model enhanced)
  - **Source**: `data-model.md` section 3, `contracts/feature-flag-evaluator-contract.json`
  - **Acceptance**: Contract tests T007 now PASS, same user always gets same result, duplicate flags update existing

- [ ] **T014a** Implement launcher version check integration for feature flag sync
  - Modify `FeatureFlagEvaluator` to expose `GetFlagHashAsync()` method
  - Add `SyncFlagsFromServerAsync()` method (called by launcher on version mismatch)
  - Store flag hash in `FeatureFlags` database table with `AppVersion` column
  - Test: Integration test validates flags only change after launcher update
  - **File**: `MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs`
  - **Dependency**: T014 complete (FeatureFlagEvaluator enhanced)
  - **Source**: spec.md FR-022a
  - **Acceptance**: Flags sync only when launcher detects version mismatch

- [ ] **T015** Create ErrorNotificationService
  - Create `MTM_Template_Application/Services/Configuration/ErrorNotificationService.cs`
  - Implement `IErrorNotificationService` interface
  - Add `ObservableCollection<ConfigurationError> _activeErrors` (backing field)
  - Expose `IReadOnlyObservableCollection<ConfigurationError> ActiveErrors` property
  - Add `event EventHandler<ConfigurationError>? OnErrorOccurred`
  - Implement `NotifyAsync(ConfigurationError error, CancellationToken ct)` method
  - Route to `ShowStatusBarWarningAsync` for Info/Warning severity
  - Route to `ShowModalDialogAsync` for Critical severity
  - Add structured logging with Serilog
  - Constructor injection: `ILogger<ErrorNotificationService>`
  - **File**: `MTM_Template_Application/Services/Configuration/ErrorNotificationService.cs`
  - **Dependency**: T010 complete (ConfigurationError model exists)
  - **Source**: `data-model.md` section 7
  - **Acceptance**: Service compiles, methods route correctly by severity

- [ ] **T016** [P] Add credential recovery trigger to WindowsSecretsService
  - Modify `MTM_Template_Application.Desktop/Services/WindowsSecretsService.cs`
  - Wrap `RetrieveSecretAsync` in try-catch for `CryptographicException`, `UnauthorizedAccessException`
  - On exception, raise event or callback to trigger CredentialDialogView
  - Log exception with Serilog (no sensitive data)
  - **File**: `MTM_Template_Application.Desktop/Services/WindowsSecretsService.cs`
  - **Source**: `data-model.md` section 2, `contracts/secrets-service-contract.json`
  - **Acceptance**: Contract tests T008 closer to passing, exceptions handled gracefully

- [ ] **T017** [P] Add credential recovery trigger to AndroidSecretsService
  - Modify `MTM_Template_Application.Android/Services/AndroidSecretsService.cs`
  - Wrap `RetrieveSecretAsync` in try-catch for `CryptographicException`, `UnauthorizedAccessException`
  - On exception, raise event or callback to trigger CredentialDialogView
  - Log exception with Serilog (no sensitive data)
  - **File**: `MTM_Template_Application.Android/Services/AndroidSecretsService.cs`
  - **Source**: `data-model.md` section 2, `contracts/secrets-service-contract.json`
  - **Acceptance**: Contract tests T008 now PASS, Android-specific handling works

---

## Phase 4: UI Components & Integration

**Goal**: Build user-facing dialogs and integrate with existing UI

- [ ] **T018** Create CredentialDialogView (AXAML)
  - Create `MTM_Template_Application/Views/Configuration/CredentialDialogView.axaml`
  - Add `x:DataType="vm:CredentialDialogViewModel"` and `x:CompileBindings="True"`
  - Add `Design.DataContext` for previewer support
  - Use Material Design layout with TextBox for Username, PasswordBox for Password
  - Use `{CompiledBinding Username}`, `{CompiledBinding Password}` syntax
  - Add Button for Submit (Command="{CompiledBinding SubmitCommand}")
  - Add Button for Cancel (Command="{CompiledBinding CancelCommand}")
  - Add Button for Retry (Command="{CompiledBinding RetryCommand}") for manual retry trigger after 3 failed credential storage attempts (NFR-017)
  - Add TextBlock for ErrorMessage with IsVisible binding
  - Use Theme V2 semantic tokens (`{DynamicResource ThemeV2.Input.Background}`, etc.)
  - Add loading spinner (ProgressBar with IsIndeterminate) when IsLoading=true
  - **File**: `MTM_Template_Application/Views/Configuration/CredentialDialogView.axaml`
  - **Dependency**: T012 complete (ViewModel exists)
  - **Source**: `research.md` section 2, Theme V2 guidelines, spec.md NFR-017
  - **Acceptance**: View compiles, previewer shows correct layout, uses CompiledBinding, includes Retry button

- [ ] **T019** Create CredentialDialogView code-behind
  - Create `MTM_Template_Application/Views/Configuration/CredentialDialogView.axaml.cs`
  - Set up DataContext binding to CredentialDialogViewModel
  - Add ShowDialog method to display as modal dialog
  - Handle dialog result (true on Submit, false on Cancel)
  - **File**: `MTM_Template_Application/Views/Configuration/CredentialDialogView.axaml.cs`
  - **Dependency**: T018 complete (AXAML exists)
  - **Acceptance**: Dialog can be shown and dismissed, returns correct result

- [ ] **T020** Integrate ErrorNotificationService with MainWindow status bar
  - Modify `MTM_Template_Application/Views/MainWindow.axaml`
  - Add status bar section for error notifications (if not exists)
  - Bind to `ErrorNotificationService.ActiveErrors` collection
  - Show warning icon + count when errors present
  - Add click handler to show error details (toast notification)
  - Use Theme V2 semantic tokens for styling
  - **File**: `MTM_Template_Application/Views/MainWindow.axaml`
  - **Dependency**: T015 complete (ErrorNotificationService exists)
  - **Source**: `research.md` section 3
  - **Acceptance**: Status bar shows errors, click displays details

- [ ] **T021** Create ConfigurationErrorDialog for critical errors
  - Create `MTM_Template_Application/Views/Configuration/ConfigurationErrorDialog.axaml`
  - Modal dialog with error message, user action guidance, OK/Retry buttons
  - Use Material Design patterns, Theme V2 tokens
  - Block UI until user dismisses (modal behavior)
  - **File**: `MTM_Template_Application/Views/Configuration/ConfigurationErrorDialog.axaml` + `.axaml.cs`
  - **Dependency**: T015 complete (ErrorNotificationService exists)
  - **Source**: `research.md` section 3
  - **Acceptance**: Dialog blocks UI, displays clear error message with action guidance

---

## Phase 5: Integration Tests & Validation

**Goal**: Validate end-to-end functionality with quickstart scenarios

- [ ] **T022** [P] Integration test: Configuration precedence validation (Scenario 1)
  - Add tests to existing `tests/integration/ConfigurationTests.cs`
  - Test environment variable > user config > default
  - Set `MTM_API_TIMEOUT` env var, verify override
  - Remove env var, reload, verify user config wins
  - Test default fallback for non-existent keys
  - **File**: `tests/integration/ConfigurationTests.cs` (EXISTING FILE - add new tests)
  - **Source**: `quickstart.md` Scenario 1
  - **Acceptance**: All assertions pass, precedence rules enforced
  - **Note**: File already has ConfigurationLoading_ShouldFollowPrecedenceOrder test; enhance with additional scenarios

- [ ] **T023** [P] Integration test: User preferences persistence (Scenario 2)
  - Add tests to existing `tests/integration/ConfigurationTests.cs`
  - Load preferences for test user (userId 99)
  - Update preference at runtime (`Display.Theme`)
  - Verify database persistence (query UserPreferences table)
  - Simulate restart (clear cache, reload)
  - Verify persisted value loaded correctly
  - **File**: `tests/integration/ConfigurationTests.cs` (EXISTING FILE - add new tests)
  - **Source**: `quickstart.md` Scenario 2
  - **Dependency**: T004 complete (database must exist for persistence tests)
  - **Acceptance**: Preferences persist across restart, OnConfigurationChanged event fires
  - **Note**: File already has ConfigurationHotReload_ShouldRaiseChangeEvent test; extend for database persistence

- [ ] **T024** [P] Integration test: Credential recovery flow (Scenario 3)
  - Create `tests/integration/CredentialRecoveryTests.cs`
  - Mock SecretsService to throw `CryptographicException`
  - Verify CredentialDialogView is triggered
  - Simulate user entering credentials
  - Verify credentials re-stored successfully
  - Test cancellation scenario
  - **File**: `tests/integration/CredentialRecoveryTests.cs`
  - **Source**: `quickstart.md` Scenario 3
  - **Acceptance**: Dialog shown on error, credentials recoverable, application continues

- [ ] **T024a** [P] Integration test: Android two-factor authentication
  - Create `tests/integration/AndroidTwoFactorAuthTests.cs`
  - Test user credentials + device certificate validation
  - Mock Android KeyStore for device certificate storage
  - Verify certificate retrieval from KeyStore
  - Test authentication failure scenarios (missing certificate, expired certificate)
  - Validate server-side certificate validation (MTM Server API)
  - **File**: `tests/integration/AndroidTwoFactorAuthTests.cs`
  - **Source**: spec.md FR-025a, Feature 001 boot sequence documentation
  - **Acceptance**: Two-factor auth validated on Android platform

- [ ] **T025** [P] Integration test: Feature flag deterministic rollout (Scenario 4)
  - Create `tests/integration/FeatureFlagDeterministicTests.cs`
  - Register flag with 50% rollout
  - Call `IsEnabledAsync` 10 times for same user, verify consistent result
  - Test 100 different users, verify ~50% enabled (allow ±10% variance)
  - Verify same user always gets same result across restarts
  - **File**: `tests/integration/FeatureFlagDeterministicTests.cs`
  - **Source**: `quickstart.md` Scenario 4
  - **Acceptance**: Deterministic behavior validated, distribution approximates rollout %

- [ ] **T026** [P] Integration test: Configuration error notification routing (Scenario 5)
  - Create `tests/integration/ConfigurationErrorNotificationTests.cs`
  - Trigger non-critical error (invalid type in env var)
  - Verify status bar warning shown, OnErrorOccurred event fires
  - Simulate critical error (database unavailable)
  - Verify modal dialog shown, blocks UI until dismissed
  - Test error resolution flow (IsResolved flag)
  - **File**: `tests/integration/ConfigurationErrorNotificationTests.cs`
  - **Source**: `quickstart.md` Scenario 5
  - **Acceptance**: Errors route to correct UI (status bar vs dialog), user-friendly messages

- [ ] **T027** [P] Integration test: Feature flag environment filtering (Scenario 6)
  - Create `tests/integration/FeatureFlagEnvironmentTests.cs`
  - Set `MTM_ENVIRONMENT=Development`
  - Register Development-only and Production-only flags
  - Verify Development flag enabled, Production flag disabled
  - Change environment to Production, reload
  - Verify opposite behavior
  - Test empty environment (all environments allowed)
  - **File**: `tests/integration/FeatureFlagEnvironmentTests.cs`
  - **Source**: `quickstart.md` Scenario 6
  - **Acceptance**: Environment filtering works, respects precedence order

- [ ] **T028** [P] Integration test: Visual API whitelist enforcement (Scenario 7)
  - Create `tests/integration/VisualApiWhitelistTests.cs`
  - Load whitelist from `appsettings.json`
  - Verify read-only commands allowed (GET_PART_DETAILS, LIST_INVENTORY)
  - Verify write commands blocked (UPDATE_INVENTORY, DELETE_PART)
  - Test citation requirement enforcement
  - Test citation format validation (regex)
  - **File**: `tests/integration/VisualApiWhitelistTests.cs`
  - **Source**: `quickstart.md` Scenario 7
  - **Acceptance**: Whitelist enforced, write commands blocked, citations validated

---

## Phase 6: Performance Validation & Polish

**Goal**: Verify performance targets and code quality

- [ ] **T029** [P] Performance test: Configuration lookup (<10ms)
  - Add tests to existing `tests/integration/PerformanceTests.cs`
  - Benchmark `GetValue` with 1000 iterations (tests in-memory cache only, not database retrieval)
  - Calculate average time, assert < 10ms
  - Test thread safety (concurrent reads)
  - **File**: `tests/integration/PerformanceTests.cs` (EXISTING FILE - add new test method)
  - **Source**: `quickstart.md` Performance Validation section
  - **Acceptance**: Average lookup time < 10ms for cached values, no race conditions
  - **Note**: File already has comprehensive performance tests for boot sequence; add configuration-specific tests

- [ ] **T030** [P] Performance test: Credential retrieval (<100ms) and feature flag evaluation (<5ms)
  - Add methods to existing `tests/integration/PerformanceTests.cs`
  - Benchmark `RetrieveSecretAsync` (single call, assert < 100ms)
  - Benchmark `IsEnabledAsync` with 1000 iterations, assert average < 5ms
  - **File**: `tests/integration/PerformanceTests.cs` (EXISTING FILE - add new test methods)
  - **Source**: `quickstart.md` Performance Validation section
  - **Acceptance**: Credential retrieval < 100ms, flag evaluation < 5ms
  - **Note**: File already has performance test infrastructure; add secrets and feature flag benchmarks

---

## Task Dependencies

### Critical Path

```
T001 (config files) → T002 (SQL scripts) → T004 (run migration)
T001-T004 → T005-T009 (contract tests) → T010-T017 (implementation)
T012 (ViewModel) → T018-T019 (View)
T015 (ErrorService) → T020-T021 (UI integration)
T013-T021 → T022-T030 (integration tests)
```

### Parallel Execution Groups
- **Setup**: T002, T003 (can run in parallel after T001)
- **Contract Tests**: T005, T006, T007, T008, T009 (all parallel, no dependencies)
- **Models**: T010, T011, T012 (all parallel, different files)
- **Service Enhancements**: T016, T017 (parallel, different platforms)
- **Integration Tests**: T022-T028 (all parallel, independent scenarios)
- **Performance Tests**: T029, T030 (parallel)

### Blocking Dependencies
- T004 blocks all database-related tests (T006, T009, T023)
- T005-T009 must FAIL before starting T010-T017 (TDD gate)
- T012 blocks T018 (ViewModel before View)
- T013-T017 must complete before T022-T030 (implementation before integration tests)

---

## Parallel Execution Examples

### Launch Contract Tests Together (Phase 2)

```bash
# All independent, can run in parallel
dotnet test --filter "FullyQualifiedName~ConfigurationServiceContractTests" &
dotnet test --filter "FullyQualifiedName~FeatureFlagEvaluatorContractTests" &
dotnet test --filter "FullyQualifiedName~SecretsServiceContractTests" &
dotnet test --filter "FullyQualifiedName~DatabaseSchemaContractTests" &
wait
```

### Launch Integration Tests Together (Phase 5)

```bash
# All scenarios are independent
dotnet test --filter "FullyQualifiedName~ConfigurationPrecedenceTests" &
dotnet test --filter "FullyQualifiedName~UserPreferencesPersistenceTests" &
dotnet test --filter "FullyQualifiedName~CredentialRecoveryTests" &
dotnet test --filter "FullyQualifiedName~FeatureFlagDeterministicTests" &
dotnet test --filter "FullyQualifiedName~ConfigurationErrorNotificationTests" &
dotnet test --filter "FullyQualifiedName~FeatureFlagEnvironmentTests" &
dotnet test --filter "FullyQualifiedName~VisualApiWhitelistTests" &
wait
```

---

## Validation Checklist

*GATE: Review before marking feature complete*

- [ ] All contract tests (T005-T009) PASS
- [ ] All integration tests (T022-T028) PASS
- [ ] All performance tests (T029-T030) meet targets
- [ ] Configuration precedence enforced (env > user > default)
- [ ] User preferences persist across restarts
- [ ] Credential recovery dialog works on both platforms
- [ ] Feature flags are deterministic for same user
- [ ] Error notifications route by severity
- [ ] Environment filtering respects current environment
- [ ] Visual API whitelist blocks write commands
- [ ] All XAML uses `x:DataType` and `CompiledBinding`
- [ ] Nullable reference types used throughout
- [ ] Async methods have `CancellationToken` parameters
- [ ] Zero build warnings
- [ ] `update-agent-context.ps1` executed to update copilot-instructions.md

---

## Notes

- **TDD Enforcement**: Phase 2 tests MUST fail before Phase 3 implementation starts
- **[P] Tasks**: Different files, truly independent, can be parallelized
- **Sequential Tasks**: Same file modifications (T013, T014, T018-T019) must be sequential
- **Commit Strategy**: Commit after each task completion (30 commits total)
- **Database Setup**: T004 prerequisite for any database-dependent tests
- **Constitutional Compliance**: All tasks follow MVVM Community Toolkit, CompiledBinding, null safety, async patterns

---

**Last Updated**: 2025-10-05
**Total Tasks**: 35 (updated after /fix: +T031 log redaction validation)
**Estimated Completion**: 2-3 weeks (with parallel execution)
**Critical Path Duration**: ~10 days (sequential dependencies)
