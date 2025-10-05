# Implementation Plan: Environment and Configuration Management System

**Branch**: `002-environment-and-configuration` | **Date**: 2025-10-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `specs/002-environment-and-configuration/spec.md`

## Execution Flow (/plan command scope)

✅ 1. Feature spec loaded from Input path
✅ 2. Technical Context filled (all NEEDS CLARIFICATION resolved via clarifications in spec)
✅ 3. Constitution Check evaluated (v1.1.0)
✅ 4. Phase 0 executed → research.md created
✅ 5. Phase 1 executed → data-model.md, contracts/, quickstart.md, agent file updated
✅ 6. Phase 2 planning approach described (tasks.md generation strategy)
✅ 7. Ready for /tasks command

---

## Summary

This feature extends the partially implemented Environment and Configuration Management System from Boot Feature 001. It adds enhanced capabilities for:

- **Configuration persistence** to MySQL database (user preferences)
- **Credential recovery UI** when OS-native storage fails
- **Severity-based error notifications** (status bar warnings vs modal dialogs for critical issues)
- **Deterministic feature flag rollout** (same user always sees same result)
- **Visual API command whitelist enforcement** (read-only operations only)
- **Feature flag synchronization** tied to launcher version updates

Core infrastructure (ConfigurationService, SecretsService, FeatureFlagEvaluator) already exists and will be enhanced with new functionality.

---

## Technical Context

**Language/Version**: C# .NET 9.0 with nullable reference types enabled
**Primary Dependencies**:

- Avalonia 11.3+ (UI framework)
- CommunityToolkit.Mvvm 8.3+ (MVVM patterns)
- MySQL.Data (MAMP MySQL 5.7 connectivity)
- Serilog (structured logging)
- xUnit + NSubstitute (testing)

**Storage**: MAMP MySQL 5.7 (UserPreferences, FeatureFlags tables) + OS-native secure storage (DPAPI/KeyStore for credentials)
**Testing**: xUnit for unit/integration tests, NSubstitute for mocking, contract tests for data validation
**Target Platform**: Windows Desktop + Android only (cross-platform scope explicitly limited)
**Project Type**: Single Avalonia solution with platform-specific implementations (`.Desktop` and `.Android` projects)
**Performance Goals**:

- Configuration lookup: <10ms (in-memory cache)
- Credential retrieval: <100ms (OS-native storage)
- Feature flag evaluation: <5ms (deterministic hash + dictionary lookup)
- Database writes: <100ms (async, non-blocking)

**Constraints**:

- Windows Desktop and Android only (macOS, Linux, iOS explicitly unsupported)
- Direct MySQL connection (no API intermediary needed for non-public app)
- OS-native credential storage mandatory (DPAPI for Windows, KeyStore for Android)
- Feature flags sync only on launcher updates (no real-time polling)

**Scale/Scope**:

- Up to 1,000 users
- Moderate data volume (preferences, feature flags)
- Admin can review and remove inactive users

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Cross-Platform First ✅ WITH JUSTIFICATION

**Status**: PARTIAL COMPLIANCE (Windows Desktop + Android only)
**Justification**: Feature spec explicitly scopes to Windows Desktop and Android platforms per business requirements. macOS, Linux, and iOS are explicitly unsupported with `PlatformNotSupportedException`. Platform-specific code (WindowsSecretsService, AndroidSecretsService) is abstracted through `ISecretsService` interface and registered via factory pattern for future expansion if needed.

### II. MVVM Community Toolkit Standard ✅ PASS

- ✅ ViewModels use `[ObservableObject]` + `[ObservableProperty]`
- ✅ Commands use `[RelayCommand]` with async support
- ✅ No ReactiveUI patterns (project-wide standard)
- ✅ Constructor dependency injection for services
- ✅ `partial` classes for source generation

### III. Test-First Development ✅ PASS

- ✅ Contract tests defined in `contracts/` directory (JSON schemas)
- ✅ Quickstart.md provides integration test scenarios
- ✅ Unit tests planned for ConfigurationService, FeatureFlagEvaluator, SecretsService
- ✅ xUnit as standard testing framework
- ✅ NSubstitute for mocking external dependencies
- ✅ Target: >80% code coverage on critical paths

### IV. Theme V2 Semantic Tokens ✅ PASS

- ✅ CredentialDialogView uses `{DynamicResource}` for all styling
- ✅ ErrorNotificationService integrates with existing status bar UI (Theme V2 compliant)
- ✅ Material Design patterns for dialog UI
- ✅ Consistent with existing Manufacturing Design System

### V. Null Safety and Error Resilience ✅ PASS

- ✅ Nullable reference types enabled project-wide
- ✅ `ArgumentNullException.ThrowIfNull()` for method parameters
- ✅ Comprehensive error handling with severity categorization
- ✅ Graceful degradation (defaults when config missing, dialog when credentials fail)
- ✅ Structured logging with Serilog (no sensitive data logged)

### VI. Compiled Bindings Only ✅ PASS

- ✅ CredentialDialogView uses `x:DataType` and `x:CompileBindings="True"`
- ✅ All bindings use `{CompiledBinding}` syntax
- ✅ No legacy `{Binding}` without compile-time validation

### VII. Dependency Injection via AppBuilder ✅ PASS

- ✅ Services registered in `Program.cs` via `AppBuilder.ConfigureServices()`
- ✅ ViewModels registered as `Transient`
- ✅ Services (ConfigurationService, SecretsService, FeatureFlagEvaluator) registered as `Singleton`
- ✅ No service locator pattern or static service access

### VIII. Async/Await Patterns ✅ PASS

- ✅ All async methods suffixed with `Async`
- ✅ `CancellationToken` parameters on all async methods
- ✅ `ConfigureAwait(false)` in service code (not UI code)
- ✅ Proper `OperationCanceledException` handling

**Gate Status**: ✅ PASS (with documented scope justification for cross-platform)

---

## Project Structure

### Documentation (this feature)

```
specs/002-environment-and-configuration/
├── plan.md                              # This file (/plan command output)
├── plan-summary.md                      # Human-friendly summary
├── research.md                          # Phase 0 output (/plan command) ✅
├── data-model.md                        # Phase 1 output (/plan command) ✅
├── quickstart.md                        # Phase 1 output (/plan command) ✅
├── contracts/                           # Phase 1 output (/plan command) ✅
│   ├── configuration-service-contract.json
│   ├── secrets-service-contract.json
│   ├── feature-flag-evaluator-contract.json
│   └── database-schema-contract.json
└── tasks.md                             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)

```
MTM_Template_Application/                # Main Avalonia app (shared code)
├── Models/
│   └── Configuration/
│       ├── FeatureFlag.cs               # EXISTS (enhance with TargetUserIdHash, AppVersion)
│       └── ConfigurationError.cs        # NEW (error model with severity)
├── Services/
│   └── Configuration/
│       ├── IConfigurationService.cs     # EXISTS (add persistence methods)
│       ├── ConfigurationService.cs      # EXISTS (enhance with MySQL persistence)
│       ├── FeatureFlagEvaluator.cs      # EXISTS (enhance with deterministic rollout)
│       └── ErrorNotificationService.cs  # NEW (severity-based error notifications)
│   └── Secrets/
│       ├── ISecretsService.cs           # EXISTS (no changes needed)
│       ├── SecretsServiceFactory.cs     # EXISTS (no changes needed)
│       ├── WindowsSecretsService.cs     # EXISTS (add recovery trigger on exceptions)
│       └── AndroidSecretsService.cs     # EXISTS (add recovery trigger on exceptions)
├── ViewModels/
│   └── Configuration/
│       └── CredentialDialogViewModel.cs # NEW (credential recovery dialog)
└── Views/
    └── Configuration/
        └── CredentialDialogView.axaml   # NEW (credential recovery UI)

config/                                   # NEW (configuration files)
├── user-folders.json                     # NEW (central server path config)
└── database-schema.json                  # NEW (MySQL schema definition)

tests/
├── unit/
│   ├── ConfigurationServiceTests.cs      # EXISTS (add persistence tests)
│   ├── FeatureFlagEvaluatorTests.cs      # NEW (deterministic rollout tests)
│   ├── ErrorNotificationServiceTests.cs  # NEW (severity routing tests)
│   └── CredentialDialogViewModelTests.cs # NEW (dialog logic tests)
├── integration/
│   ├── ConfigurationPersistenceTests.cs  # NEW (end-to-end MySQL tests)
│   └── CredentialRecoveryTests.cs        # NEW (full recovery flow tests)
└── contract/
    ├── ConfigurationServiceContractTests.cs      # NEW (schema validation)
    ├── SecretsServiceContractTests.cs            # NEW (encryption verification)
    └── FeatureFlagEvaluatorContractTests.cs      # NEW (deterministic behavior)
```

**Structure Decision**: Single Avalonia solution with platform-specific projects (`.Desktop`, `.Android`). Shared business logic in main `MTM_Template_Application` project. Platform-specific services (WindowsSecretsService, AndroidSecretsService) in platform projects, registered via factory pattern.

---

## Phase 0: Outline & Research ✅ COMPLETE

**Status**: ✅ All NEEDS CLARIFICATION items resolved via stakeholder clarification sessions

**Research Output**: [research.md](./research.md)

### Key Decisions Made

1. **Configuration Persistence**: Dual-storage approach (JSON config file + MySQL database)
2. **Credential Recovery**: Avalonia dialog with Material Design styling
3. **Error Notifications**: Severity-based routing (status bar vs modal dialog)
4. **Visual API Whitelist**: Stored in `appsettings.json`, read-only commands only
5. **Feature Flag Sync**: Tied to launcher version checks (next launch only)
6. **Android Database**: Direct MySQL connection (no API intermediary)

### Clarification Resolution Summary

All 10+ clarification questions from spec.md resolved:

- ✅ Credential storage failures → Prompt for re-entry
- ✅ Unconfigured feature flags → Disabled by default
- ✅ Invalid config types → Use defaults + log warnings (non-critical) or show dialog (critical)
- ✅ User preferences storage → Dual approach (JSON config + MySQL)
- ✅ Android connectivity → Direct MySQL (same as Windows)
- ✅ Visual API whitelist → Config file with read-only commands
- ✅ Feature flag sync → Launcher-driven (next launch)

---

## Phase 1: Design & Contracts ✅ COMPLETE

**Status**: ✅ All design artifacts generated

### Artifacts Generated

1. **Data Model**: [data-model.md](./data-model.md)
   - Enhanced entity definitions (ConfigurationService, FeatureFlag, ConfigurationError)
   - New entities (CredentialDialogViewModel, ErrorNotificationService)
   - Database schema (UserPreferences, FeatureFlags, Users tables)
   - Validation rules and state transitions
   - Relationships and performance constraints

2. **Contracts**: [contracts/](./contracts/)
   - `configuration-service-contract.json`: Method signatures, precedence rules, events
   - `secrets-service-contract.json`: OS-native storage patterns, error recovery flows
   - `feature-flag-evaluator-contract.json`: Deterministic rollout algorithm, environment filtering
   - `database-schema-contract.json`: MySQL table definitions, indexes, sample data

3. **Quickstart**: [quickstart.md](./quickstart.md)
   - 7 integration test scenarios mapping to user stories
   - Performance validation tests (<10ms config, <100ms secrets, <5ms flags)
   - Test data setup and cleanup scripts
   - Success criteria checklist

4. **Agent Context**: `.github/copilot-instructions.md` updated
   - Added feature 002 technical context
   - Preserved manual additions between markers
   - Updated recent changes section
   - Kept under 150 lines for token efficiency

### Constitution Re-Check Post-Design

✅ **PASS**: All constitutional requirements validated after design phase

- Cross-platform scope justified (Windows + Android only)
- MVVM patterns consistent throughout ViewModels
- Test-first approach with contract tests defined
- Theme V2 integration for new UI components
- Null safety and error resilience patterns applied
- Compiled bindings enforced in AXAML
- DI registration planned for all services
- Async/await with CancellationToken throughout

---

## Phase 2: Task Planning Approach

*This section describes what the /tasks command will do - DO NOT execute during /plan*

### Task Generation Strategy

**Load** `.specify/templates/tasks-template.md` as base structure

**Generate tasks from Phase 1 artifacts**:

1. **Contract Tests** (from `contracts/` directory):
   - Each JSON contract → xUnit test class
   - Validate request/response schemas
   - Test error conditions
   - Verify performance constraints
   - Tasks marked [P] for parallel execution (independent test files)

2. **Data Model Implementation** (from `data-model.md`):
   - Enhance existing models (FeatureFlag with TargetUserIdHash, AppVersion)
   - Create new models (ConfigurationError)
   - Create new ViewModels (CredentialDialogViewModel)
   - Create new Services (ErrorNotificationService)
   - Tasks marked [P] for independent model files

3. **Database Schema** (from `database-schema-contract.json`):
   - Generate SQL migration scripts
   - Create `config/database-schema.json`
   - Create setup documentation
   - Test schema on development database

4. **Service Enhancements** (from contracts):
   - ConfigurationService: Add MySQL persistence methods
   - FeatureFlagEvaluator: Implement deterministic rollout with hash-based evaluation
   - SecretsService implementations: Add credential recovery triggers
   - ErrorNotificationService: Implement severity-based routing

5. **UI Components** (from quickstart scenarios):
   - CredentialDialogView (AXAML with x:DataType, CompiledBinding)
   - Status bar error indicator integration
   - Modal dialog for critical errors

6. **Integration Tests** (from `quickstart.md`):
   - Each scenario → xUnit integration test
   - Configuration precedence validation
   - User preferences persistence round-trip
   - Credential recovery flow end-to-end
   - Feature flag deterministic evaluation
   - Error notification severity routing
   - Environment filtering
   - Visual API whitelist enforcement

7. **Configuration Files**:
   - Create `config/user-folders.json` with placeholders
   - Update `appsettings.json` with Visual API whitelist
   - Document placeholder replacement process

### Task Ordering Strategy

**TDD Order** (Tests before implementation):

1. Contract test tasks (define expected behavior)
2. Model creation tasks (simple data structures first)
3. Implementation tasks (make contract tests pass)
4. Integration test tasks (validate end-to-end flows)

**Dependency Order**:

1. Database schema setup (prerequisite for persistence tests)
2. Models (prerequisite for services)
3. Services (prerequisite for ViewModels)
4. ViewModels (prerequisite for Views)
5. Views (final integration point)

**Parallelization** ([P] marker):

- All contract test tasks are parallel (independent files)
- All model creation tasks are parallel (independent classes)
- Service implementations can be parallel if no interdependencies
- View/ViewModel pairs must be sequential (ViewModel → View)

### Estimated Task Count

- **Contract Tests**: 4 tasks (one per contract file) [P]
- **Models**: 3 tasks (ConfigurationError, enhance FeatureFlag, CredentialDialogViewModel) [P]
- **Services**: 4 tasks (ErrorNotificationService, ConfigurationService enhancements, FeatureFlagEvaluator enhancements, SecretsService recovery)
- **Database**: 3 tasks (schema JSON, SQL scripts, setup docs)
- **UI**: 2 tasks (CredentialDialogView, status bar integration)
- **Integration Tests**: 7 tasks (one per quickstart scenario)
- **Configuration Files**: 2 tasks (user-folders.json, appsettings.json updates)

**Total Estimated**: 25-30 numbered, ordered tasks

**IMPORTANT**: Phase 2 execution (tasks.md creation) is handled by the `/tasks` command, NOT by `/plan`

---

## Phase 3+: Future Implementation

*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution

- `/tasks` command creates tasks.md with numbered, ordered task list
- Each task references contracts, data model, or quickstart scenarios
- TDD order enforced (contract tests → models → implementation → integration tests)

**Phase 4**: Implementation

- Execute tasks.md following constitutional principles
- Use `[P]` markers to identify parallel execution opportunities
- Run tests continuously (Red-Green-Refactor cycle)
- Update Progress Tracking as tasks complete

**Phase 5**: Validation

- Run all contract tests (schema validation)
- Execute all integration tests (quickstart scenarios)
- Performance validation (config <10ms, secrets <100ms, flags <5ms)
- Verify constitutional compliance (compiled bindings, null safety, async patterns)

---

## Complexity Tracking

*No constitutional violations requiring justification*

| Violation | Why Needed                          | Simpler Alternative Rejected Because |
| --------- | ----------------------------------- | ------------------------------------ |
| N/A       | All constitutional requirements met | N/A                                  |

**Cross-Platform Scope Note**: Feature spec explicitly limits support to Windows Desktop + Android only. This is a business requirement, not a constitutional violation. Platform abstraction via `ISecretsService` interface allows future expansion if needed.

---

## Progress Tracking

*This checklist is updated during execution flow*

**Phase Status**:

- [x] Phase 0: Research complete (/plan command) ✅
- [x] Phase 1: Design complete (/plan command) ✅
- [x] Phase 2: Task planning approach described (/plan command - describe only) ✅
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:

- [x] Initial Constitution Check: PASS ✅
- [x] All NEEDS CLARIFICATION resolved ✅
- [x] Post-Design Constitution Check: PASS ✅
- [ ] Complexity deviations documented (N/A - no deviations)

**Artifact Status**:

- [x] research.md created ✅
- [x] data-model.md created ✅
- [x] contracts/ directory populated ✅
- [x] quickstart.md created ✅
- [x] Agent file updated ✅
- [x] plan-summary.md created ✅
- [ ] tasks.md created (awaiting /tasks command)

---

## Next Steps

1. ✅ Planning complete (/plan command finished)
2. → Run `/tasks` command to generate tasks.md
3. → Execute tasks in TDD order (contract tests → implementation)
4. → Run quickstart scenarios as integration tests
5. → Validate performance targets (<10ms, <100ms, <5ms)
6. → Verify constitutional compliance (compiled bindings, null safety, async)

---

*Based on Constitution v1.1.0 - See `.specify/memory/constitution.md`*
*Feature Spec: `specs/002-environment-and-configuration/spec.md`*
*Research: `specs/002-environment-and-configuration/research.md`*
*Data Model: `specs/002-environment-and-configuration/data-model.md`*
*Contracts: `specs/002-environment-and-configuration/contracts/`*
*Quickstart: `specs/002-environment-and-configuration/quickstart.md`*

**Last Updated**: 2025-10-05
