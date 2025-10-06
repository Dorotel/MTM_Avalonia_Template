
# Implementation Plan: Environment and Configuration Management System

**Branch**: `002-environment-and-configuration` | **Date**: 2025-10-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-environment-and-configuration/spec.md`

## Execution Flow (/plan command scope)

```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary

Implement a robust environment and configuration management system that provides dual-storage architecture for application settings (appsettings.json with environment variable overrides), platform-agnostic credential management using OS-native secure storage (Windows DPAPI, Android KeyStore), and launch-time-only feature flag synchronization from MySQL database. The system ensures type-safe configuration access, graceful degradation when dependencies are unavailable, severity-based error handling with structured logging, and constitutional compliance for nullable safety, MVVM patterns, and test-first development. Implementation follows Feature 002 specification with 28 clarifications resolving credential recovery, flag defaults, invalid values, database connectivity, and platform-specific credential storage patterns.

## Technical Context
**Language/Version**: C# .NET 9.0 with nullable reference types enabled (`<LangVersion>latest</LangVersion>`)
**Primary Dependencies**: Avalonia 11.3.6, CommunityToolkit.Mvvm 8.4.0, MySql.Data 9.0.0, Serilog 8.0.0, Microsoft.Extensions.Configuration 9.0.0
**Storage**: MAMP MySQL 5.7.24 (127.0.0.1:3306, database: mtm_template_dev), OS-native credential storage (DPAPI/KeyStore), local appsettings.json
**Testing**: xUnit 2.9.2, NSubstitute 5.1.0, FluentAssertions 6.12.1
**Target Platform**: Windows Desktop (Phase 1), Android (Phase 1), iOS/Linux (deferred to Phase 2+)
**Project Type**: Mobile + Desktop (MTM_Template_Application shared library, platform-specific launchers)
**Performance Goals**: <3s service initialization, <100ms configuration retrieval, <200ms credential retrieval
**Constraints**: Offline-first (graceful degradation), platform-agnostic interfaces with platform-specific implementations, launch-time-only feature flag sync (no runtime hot-reload)
**Scale/Scope**: ~50 configuration keys, ~10 credentials, ~20 feature flags, 3 database tables (Users, UserPreferences, FeatureFlags)

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*
*Based on Constitution v1.3.0*

### I. Cross-Platform First
- [x] Phase 1 platforms supported (Windows Desktop + Android)
- [x] Deferred platforms documented (iOS/Linux to Phase 2+)
- [x] Platform-specific code abstracted through interfaces (ISecretsService)
- [x] Platform differences handled via dependency injection (SecretsServiceFactory)
- [x] Unsupported platforms throw `PlatformNotSupportedException`

### II. MVVM Community Toolkit Standard
- [x] ViewModels use `[ObservableObject]` + `[ObservableProperty]`
- [x] Commands use `[RelayCommand]` with async support
- [x] NO ReactiveUI patterns
- [x] Constructor dependency injection
- [x] `partial` classes for source generation

### III. Test-First Development
- [x] Tests written before implementation (TDD)
- [x] xUnit as standard testing framework
- [x] Contract tests for APIs (IConfigurationService, ISecretsService, IFeatureFlagEvaluator)
- [x] Integration tests for workflows (credential storage/retrieval, database sync)
- [x] Unit tests for business logic (configuration precedence, flag evaluation)
- [x] NSubstitute for mocking
- [x] Target >80% code coverage on critical paths

### IV. Theme V2 Semantic Tokens
- [x] DynamicResource usage (no hardcoded colors)
- [x] FluentTheme or Material.Avalonia base
- [x] Manufacturing Design System integration
- [x] Light/dark theme switching support

### V. Null Safety and Error Resilience
- [x] Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- [x] `ArgumentNullException.ThrowIfNull()` for parameters
- [x] Error boundaries in ViewModels
- [x] Graceful degradation for offline scenarios (cached flags, default values)
- [x] Serilog structured logging (no sensitive data - credentials excluded)

### VI. Compiled Bindings Only
- [x] `x:DataType` on Window/UserControl root
- [x] `x:CompileBindings="True"` for validation
- [x] `{CompiledBinding}` syntax (NO legacy `{Binding}`)
- [x] `Design.DataContext` for previewer

### VII. Dependency Injection via AppBuilder
- [x] Services registered in `Program.cs` via `AppBuilder.ConfigureServices()`
- [x] ViewModels as `Transient`
- [x] Services as `Singleton` (ConfigurationService, FeatureFlagEvaluator) or platform-specific factory
- [x] NO service locator pattern

### VIII. MAMP MySQL Database Documentation
- [x] Reference JSON files in `.github/mamp-database/` before code generation
- [x] Update JSON files immediately after database changes (Users, UserPreferences, FeatureFlags tables)
- [x] Complete metadata (tables, columns, indexes, foreign keys, procedures, functions, views)
- [x] Schema accuracy maintained (single source of truth in schema-tables.json)
- [x] Version tracking in `migrations-history.json`

### IX. Visual ERP Integration Standards (if applicable)
- [x] Read-only access only (Feature 002 does NOT interact with Visual ERP - N/A)
- [x] Feature focuses on configuration/credentials - Visual integration deferred to future features

### Async/Await Patterns
- [x] `CancellationToken` parameter on all async methods
- [x] Methods suffixed with `Async`
- [x] `ConfigureAwait(false)` in library/service code (NOT UI)
- [x] `OperationCanceledException` handled gracefully

**Gate Status**: ✅ PASS

All constitutional principles satisfied. No complexity deviations to document.

## Project Structure

### Documentation (this feature)

```
specs/002-environment-and-configuration/
├── spec.md              # Feature specification with 28 clarifications
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command) ✅
├── data-model.md        # Phase 1 output (/plan command) ✅
├── contracts/           # Phase 1 output (/plan command) ✅
│   ├── IConfigurationService.cs
│   ├── ISecretsService.cs
│   └── IFeatureFlagEvaluator.cs
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)

```
MTM_Template_Application/               # Shared Avalonia UI library
├── Models/
│   ├── Configuration/
│   │   ├── AppConfiguration.cs        # appsettings.json model
│   │   ├── UserPreference.cs          # User-specific settings
│   │   └── FeatureFlag.cs             # Feature flag entity
│   ├── Core/
│   │   ├── Result.cs                  # Result<T> pattern
│   │   └── ErrorCategory.cs           # Error categorization
│   └── Enums/
│       ├── ErrorSeverity.cs           # Critical/Warning/Info
│       └── ConfigurationSource.cs     # File/EnvVar/Database
├── Services/
│   ├── Configuration/
│   │   ├── ConfigurationService.cs    # Implements IConfigurationService
│   │   ├── IConfigurationService.cs   # Core configuration contract
│   │   └── ConfigurationExtensions.cs # DI registration
│   ├── Secrets/
│   │   ├── ISecretsService.cs         # Platform-agnostic contract
│   │   ├── SecretsServiceFactory.cs   # Platform detection factory
│   │   ├── WindowsSecretsService.cs   # DPAPI implementation
│   │   └── AndroidSecretsService.cs   # KeyStore implementation
│   ├── FeatureFlags/
│   │   ├── FeatureFlagEvaluator.cs    # Implements IFeatureFlagEvaluator
│   │   ├── IFeatureFlagEvaluator.cs   # Flag evaluation contract
│   │   └── FeatureFlagExtensions.cs   # DI registration
│   ├── DataLayer/
│   │   ├── MySqlConnectionFactory.cs  # Connection string management
│   │   └── UserPreferencesRepository.cs # Database access
│   └── Logging/
│       └── SerilogConfiguration.cs    # Structured logging setup
├── appsettings.json                   # Default configuration
└── App.axaml.cs                       # Service registration

MTM_Template_Application.Desktop/       # Windows Desktop launcher
├── Program.cs                          # Entry point + DI setup
└── Services/
    └── DesktopSecretsService.cs       # Desktop-specific implementations

MTM_Template_Application.Android/       # Android launcher
├── MainActivity.cs                     # Android entry point
└── Services/
    └── AndroidSecretsService.cs       # Android-specific implementations

tests/
├── unit/
│   ├── Configuration/
│   │   ├── ConfigurationServiceTests.cs
│   │   └── ConfigurationPrecedenceTests.cs
│   ├── Secrets/
│   │   ├── WindowsSecretsServiceTests.cs
│   │   └── AndroidSecretsServiceTests.cs
│   └── FeatureFlags/
│       ├── FeatureFlagEvaluatorTests.cs
│       └── FlagDefaultBehaviorTests.cs
├── integration/
│   ├── Configuration/
│   │   └── EnvironmentVariableOverrideTests.cs
│   ├── Database/
│   │   ├── UserPreferencesRepositoryTests.cs
│   │   └── FeatureFlagSyncTests.cs
│   └── Secrets/
│       ├── CredentialRecoveryFlowTests.cs
│       └── PlatformStorageTests.cs
└── contract/
    ├── IConfigurationServiceContractTests.cs
    ├── ISecretsServiceContractTests.cs
    └── IFeatureFlagEvaluatorContractTests.cs
```

**Structure Decision**: Mobile + Desktop architecture using shared MTM_Template_Application library with platform-specific launchers (Desktop/Android). Platform-specific implementations use factory pattern with runtime detection. Services registered in Program.cs via AppBuilder.ConfigureServices() following constitutional DI principle.

## Phase 0: Outline & Research

**Status**: ✅ Complete - research.md generated

### Research Areas Completed

1. **Configuration Storage Architecture**: Dual-storage pattern with appsettings.json + environment variable overrides
   - Decision: Microsoft.Extensions.Configuration with layered providers
   - Rationale: Industry standard, type-safe, testable
   - Alternatives: Custom JSON parser (rejected - reinventing wheel), Registry (Windows-only)

2. **Credential Management**: Platform-specific OS-native secure storage
   - Decision: DPAPI (Windows), KeyStore (Android) via factory pattern
   - Rationale: OS-native security, no external dependencies, constitutional compliance
   - Alternatives: Encrypted files (less secure), cloud storage (requires connectivity)

3. **Feature Flag Architecture**: Launch-time-only sync from MySQL database
   - Decision: In-memory cache populated at startup, no hot-reload
   - Rationale: Simplicity, predictable behavior, performance
   - Alternatives: Real-time sync (complexity), file-based (no shared state)

4. **Error Categorization**: Severity-based handling (Critical/Warning/Info)
   - Decision: ErrorCategory enum with Result<T> pattern
   - Rationale: Type-safe error handling, functional approach
   - Alternatives: Exception-only (lost context), HTTP status codes (web-specific)

5. **Database Schema**: Three tables (Users, UserPreferences, FeatureFlags)
   - Decision: Normalized schema with foreign keys, indexed lookups
   - Rationale: Data integrity, query performance, scalability
   - Alternatives: NoSQL (overkill), single config table (poor separation)

6. **Platform Detection**: RuntimeInformation.IsOSPlatform()
   - Decision: .NET built-in platform detection with factory pattern
   - Rationale: Standard library, no external dependencies
   - Alternatives: Conditional compilation (less flexible), #if directives (harder to test)

7. **Configuration Precedence**: Environment variables override appsettings.json
   - Decision: Layered providers with explicit precedence order
   - Rationale: Deployment flexibility, 12-factor app principles
   - Alternatives: Single source (inflexible), merge logic (complex)

8. **Credential Recovery**: User prompt on retrieval failure
   - Decision: Modal dialog with retry logic, re-save to OS storage
   - Rationale: User control, self-healing, constitutional error resilience
   - Alternatives: Silent failure (poor UX), crash (breaks graceful degradation)

9. **Testing Strategy**: Contract → Integration → Unit tests
   - Decision: TDD with contract tests first (interface verification)
   - Rationale: Constitutional principle III, design-by-contract
   - Alternatives: Implementation-first (violates TDD), manual testing (unreliable)

10. **Logging Approach**: Serilog structured logging with severity levels
    - Decision: Structured logs to file and console, exclude credentials
    - Rationale: Debuggability, constitutional logging requirement
    - Alternatives: Console.WriteLine (unstructured), no logging (unmaintainable)

**Output**: [research.md](./research.md) with all 10 areas documented

## Phase 1: Design & Contracts

**Status**: ✅ Complete - data-model.md and contracts/ generated

### Entities Defined (data-model.md)

1. **ConfigurationService**
   - Type: Service implementation
   - Responsibilities: Dual-storage management, precedence resolution, type-safe access
   - Key methods: `GetValue<T>()`, `GetSection()`, `Reload()`
   - Dependencies: IConfiguration, ILogger<ConfigurationService>

2. **ISecretsService**
   - Type: Platform-agnostic contract
   - Implementations: WindowsSecretsService (DPAPI), AndroidSecretsService (KeyStore)
   - Key methods: `StoreSecretAsync()`, `RetrieveSecretAsync()`, `DeleteSecretAsync()`
   - Platform factory: SecretsServiceFactory with RuntimeInformation detection

3. **FeatureFlagEvaluator**
   - Type: Service implementation
   - Responsibilities: Launch-time flag sync, in-memory cache, evaluation logic
   - Key methods: `IsEnabledAsync()`, `RefreshFlagsAsync()`, `GetAllFlags()`
   - Dependencies: MySqlConnectionFactory, ILogger<FeatureFlagEvaluator>

4. **AppConfiguration**
   - Type: Domain model
   - Properties: ConnectionStrings, LogLevel, FeatureFlagDefaults, FolderPaths
   - Validation: FluentValidation rules for required fields

5. **UserPreference**
   - Type: Domain model (database entity)
   - Properties: UserId, PreferenceKey, PreferenceValue, LastUpdated
   - Database: UserPreferences table with composite key (UserId, PreferenceKey)

6. **FeatureFlag**
   - Type: Domain model (database entity)
   - Properties: FlagName, IsEnabled, LastUpdated
   - Database: FeatureFlags table with primary key (FlagName)

### Database Schema (MySQL 5.7)

**Tables**:
- Users (UserId PK, Username, DisplayName, IsActive, CreatedAt, LastLoginAt)
- UserPreferences (UserId FK, PreferenceKey, PreferenceValue, LastUpdated) - Composite PK (UserId, PreferenceKey)
- FeatureFlags (FlagName PK, IsEnabled, LastUpdated)

**Indexes**:
- UserPreferences: IX_UserPreferences_UserId (performance)
- FeatureFlags: IX_FeatureFlags_IsEnabled (query optimization)

### API Contracts Generated

1. **[IConfigurationService.cs](./contracts/IConfigurationService.cs)**
   - Contract tests: GetValue returns typed values, GetSection returns subsection, precedence enforced
   - Methods: `T GetValue<T>(string key, T defaultValue)`, `IConfigurationSection GetSection(string key)`

2. **[ISecretsService.cs](./contracts/ISecretsService.cs)**
   - Contract tests: Store succeeds, Retrieve returns stored value, Delete removes credential
   - Methods: `Task<Result> StoreSecretAsync(string key, string value, CancellationToken)`, `Task<Result<string>> RetrieveSecretAsync(string key, CancellationToken)`, `Task<Result> DeleteSecretAsync(string key, CancellationToken)`

3. **[IFeatureFlagEvaluator.cs](./contracts/IFeatureFlagEvaluator.cs)**
   - Contract tests: IsEnabled returns flag state, unconfigured flags default to false, refresh updates cache
   - Methods: `Task<bool> IsEnabledAsync(string flagName, CancellationToken)`, `Task<Result> RefreshFlagsAsync(CancellationToken)`, `IReadOnlyDictionary<string, bool> GetAllFlags()`

### Test Scenarios (from spec.md user stories)

1. Developer overrides database connection via environment variable → Integration test
2. User's credential is corrupted in Windows Credential Manager → Integration test (recovery dialog)
3. Feature flag sync fails due to network outage → Integration test (graceful degradation, cached flags)
4. Administrator disables feature flag in database → Contract test (database state → evaluator state)

**Output**: [data-model.md](./data-model.md), [contracts/](./contracts/), failing contract tests

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

### Task Generation Strategy

The /tasks command will generate tasks.md by:

1. **Loading template**: `.specify/templates/tasks-template.md` as structural base
2. **Extracting from Phase 1 artifacts**:
   - Each contract (IConfigurationService, ISecretsService, IFeatureFlagEvaluator) → contract test task [P]
   - Each entity (AppConfiguration, UserPreference, FeatureFlag) → model creation task [P]
   - Each database table (Users, UserPreferences, FeatureFlags) → schema migration task
   - Each user story from spec.md → integration test task
   - Each service implementation → implementation task to make tests pass

3. **Constitutional ordering**:
   - **TDD order**: Contract tests → Integration tests → Models → Service implementations → ViewModels
   - **Dependency order**: Database schema → Models → Repository → Services → DI registration → Tests
   - **Parallel marking**: Independent tasks marked [P] (e.g., model creation, contract tests for different services)

### Task Breakdown Estimation

**Database layer** (5 tasks):
- Create migration for Users, UserPreferences, FeatureFlags tables
- Create MySqlConnectionFactory
- Create UserPreferencesRepository
- Update .github/mamp-database/schema-tables.json
- Write database integration tests [P]

**Configuration layer** (6 tasks):
- Write IConfigurationService contract tests [P]
- Create AppConfiguration model
- Implement ConfigurationService
- Create ConfigurationExtensions (DI registration)
- Write configuration precedence integration tests
- Write environment variable override tests [P]

**Secrets layer** (7 tasks):
- Write ISecretsService contract tests [P]
- Implement WindowsSecretsService (DPAPI)
- Implement AndroidSecretsService (KeyStore)
- Create SecretsServiceFactory
- Write credential recovery integration tests
- Write platform storage integration tests [P]
- Update DI registration in Program.cs

**Feature Flags layer** (6 tasks):
- Write IFeatureFlagEvaluator contract tests [P]
- Create FeatureFlag model
- Implement FeatureFlagEvaluator
- Create FeatureFlagExtensions (DI registration)
- Write flag sync integration tests
- Write flag default behavior unit tests [P]

**Cross-cutting** (4 tasks):
- Create ErrorCategory and Result<T> models [P]
- Configure Serilog with structured logging
- Write error categorization unit tests [P]
- Document credential recovery UI flow

**Total estimated**: 28 tasks (10 marked [P] for parallel execution)

### Dependency Management

**Critical path**:
1. Database schema → Models → Repository
2. Contract interfaces → Service implementations
3. Factory pattern → Platform-specific implementations
4. DI registration → Integration tests

**Parallel paths**:
- Configuration, Secrets, and FeatureFlags layers can be implemented in parallel after database schema
- Contract tests for different services are independent [P]
- Unit tests for models are independent [P]
- Platform-specific implementations (Windows/Android) are independent [P]

### Test Coverage Strategy

**Contract tests** (interface verification):
- Verify method signatures match contracts
- Test return types and error conditions
- No implementation dependencies

**Integration tests** (end-to-end workflows):
- Configuration precedence (env var overrides appsettings.json)
- Credential recovery flow (corrupted storage → user prompt → re-save)
- Feature flag sync (database → in-memory cache → evaluation)
- Offline scenarios (graceful degradation with cached flags)

**Unit tests** (business logic):
- Configuration parsing and type conversion
- Error categorization and severity mapping
- Flag evaluation with default behaviors
- Platform detection logic

**Performance validation**:
- <3s service initialization
- <100ms configuration retrieval
- <200ms credential retrieval
- All measured in integration tests

**IMPORTANT**: The /tasks command will execute this plan to create tasks.md - the /plan command STOPS HERE.

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)
**Phase 4**: Implementation (execute tasks.md following constitutional principles)
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

**No violations to document** - Constitution Check: ✅ PASS

## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command) - research.md generated with 10 research areas
- [x] Phase 1: Design complete (/plan command) - data-model.md + 3 contracts + database schema
- [x] Phase 2: Task planning complete (/plan command) - 28-task breakdown documented above
- [ ] Phase 3: Tasks generated (/tasks command) - awaiting /tasks execution
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: ✅ PASS (all 9 principles satisfied)
- [x] Post-Design Constitution Check: ✅ PASS (no violations introduced)
- [x] All NEEDS CLARIFICATION resolved (28 clarifications in spec.md Session 2025-10-05)
- [x] Complexity deviations documented: None (no violations)

**Artifact Inventory**:
- [x] spec.md (Feature specification with 28 clarifications)
- [x] plan.md (This file - complete)
- [x] research.md (10 research areas documented)
- [x] data-model.md (6 entities + database schema)
- [x] contracts/IConfigurationService.cs
- [x] contracts/ISecretsService.cs
- [x] contracts/IFeatureFlagEvaluator.cs
- [ ] tasks.md (awaiting /tasks command)

**Metrics**:
- Research areas: 10/10 complete
- Entities defined: 6 (ConfigurationService, ISecretsService, FeatureFlagEvaluator, AppConfiguration, UserPreference, FeatureFlag)
- Contracts created: 3 (IConfigurationService, ISecretsService, IFeatureFlagEvaluator)
- Database tables: 3 (Users, UserPreferences, FeatureFlags)
- Constitutional principles verified: 9/9
- Estimated tasks: 28 (10 parallelizable)
- Target test coverage: >80% on critical paths

---
*Based on Constitution v1.3.0 - See `.specify/memory/constitution.md`*
*Ready for /tasks command execution*
