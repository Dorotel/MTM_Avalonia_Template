
# Implementation Plan: Boot Sequence - Splash-First Services Initialization

**Branch**: `001-boot-sequence-splash` | **Date**: 2025-10-02 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-boot-sequence-splash/spec.md`

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

The boot sequence feature implements a splash-first initialization pattern for a cross-platform manufacturing application (Avalonia Desktop + Android). The system initializes services in strict sequential order while displaying a theme-less splash screen with real-time progress indicators. Key technical approach includes cascading data source fallback (Visual API Toolkit → MAMP MySQL 5.7 → Local Cache), fail-fast validation with operator-controlled retry mechanisms, and comprehensive boot metrics logging. The design prioritizes offline resilience, single-instance enforcement, and graceful degradation for manufacturing environments where system availability is critical.

## Technical Context

**Language/Version**: C# .NET 9.0 with nullable reference types enabled  
**Primary Dependencies**: Avalonia 11.3+, CommunityToolkit.Mvvm 8.3+, MySQL.Data (for MAMP), HttpClient (for Visual API Toolkit)  
**Storage**: Local file system (JSON/CSV for boot metrics and cache manifest), SQLite or LiteDB for local cache, MySQL 5.7 (MAMP backup server)  
**Testing**: xUnit, FluentAssertions for unit/integration tests, contract tests for API endpoints  
**Target Platform**: Windows 10+ Desktop, Android 8.0+ (API 26+)
**Project Type**: Mobile + Desktop (cross-platform Avalonia with shared core + platform-specific projects)  
**Performance Goals**: Splash visible <500ms, normal boot <10s, reachability checks 10s timeout each, cache prefetch 30s default (10-120s configurable)  
**Constraints**: <200MB memory during init (mobile constraint), offline-capable with cascading fallback, single-instance enforcement, fail-fast on invalid credentials  
**Scale/Scope**: Single-user client application, 10 initialization stages, 3 data source tiers (Visual/MAMP/Cache), 100-boot metrics retention with FIFO rotation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Cross-Platform First**: Boot sequence works on both Desktop and Android with platform-specific abstractions (MAMP access via client interfaces)
- [x] **MVVM Community Toolkit**: ViewModels use [ObservableObject], [ObservableProperty], [RelayCommand] patterns exclusively
- [x] **Test-First Development**: TDD workflow enforced - contract tests, integration tests, and unit tests written before implementation
- [x] **Theme V2 Semantic Tokens**: Splash screen is theme-less by design (FR-001); main UI transitions to Theme V2 styling post-boot
- [x] **Null Safety**: Nullable reference types enabled, ArgumentNullException.ThrowIfNull() for DI parameters, graceful error handling
- [x] **Manufacturing Domain**: Offline-first with cascading fallback, operator retry controls, persistent status banners for data source awareness
- [x] **Production-Ready**: Comprehensive logging, boot metrics persistence, single-instance enforcement, deterministic retry behavior

**Initial Assessment**: PASS - No constitutional violations detected

## Project Structure

### Documentation (this feature)
```
specs/001-boot-sequence-splash/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
MTM_Template_Application/              # Shared core library (cross-platform)
├── Models/
│   ├── Boot/
│   │   ├── BootStage.cs              # Boot stage entity
│   │   ├── BootMetrics.cs            # Metrics entity
│   │   ├── ReachabilityStatus.cs     # Network status entity
│   │   └── CacheManifest.cs          # Cache inventory entity
│   ├── Configuration/
│   │   └── ServiceConfiguration.cs   # Environment settings
│   └── DataSources/
│       └── DataSourceType.cs         # Visual/MAMP/Cache enum
├── Services/
│   ├── Boot/
│   │   ├── IBootSequenceService.cs   # Boot orchestration interface
│   │   ├── BootSequenceService.cs    # Boot orchestration implementation
│   │   └── BootMetricsService.cs     # Metrics persistence
│   ├── Configuration/
│   │   └── ConfigurationService.cs   # Environment/feature flags
│   ├── Reachability/
│   │   ├── IReachabilityService.cs
│   │   └── ReachabilityService.cs    # Endpoint checks
│   ├── DataLayer/
│   │   ├── IVisualApiService.cs      # Visual API Toolkit client
│   │   ├── IMampDataService.cs       # MAMP MySQL client (Desktop)
│   │   ├── IMampApiProxyService.cs   # MAMP proxy (Android)
│   │   └── ICacheService.cs          # Local cache service
│   └── Logging/
│       └── LoggingService.cs         # Structured logging
├── ViewModels/
│   ├── SplashViewModel.cs            # Splash screen VM
│   └── MainViewModel.cs              # Main app VM (updated)
└── Views/
    └── SplashView.axaml/.cs          # Splash screen view

MTM_Template_Application.Desktop/     # Windows platform
├── Services/
│   └── MampDataService.cs            # MySQL direct connection
└── Program.cs                         # Desktop entry point (updated)

MTM_Template_Application.Android/     # Android platform
├── Services/
│   └── MampApiProxyService.cs        # MAMP via API proxy
└── MainActivity.cs                    # Android entry point (updated)

tests/                                 # Test projects (new)
├── MTM_Template_Application.Tests.Unit/
│   ├── Boot/
│   │   ├── BootSequenceServiceTests.cs
│   │   └── BootMetricsServiceTests.cs
│   ├── Configuration/
│   │   └── ConfigurationServiceTests.cs
│   └── Reachability/
│       └── ReachabilityServiceTests.cs
├── MTM_Template_Application.Tests.Integration/
│   ├── BootSequenceIntegrationTests.cs
│   └── DataSourceFallbackTests.cs
└── MTM_Template_Application.Tests.Contract/
    ├── VisualApiContractTests.cs
    └── MampApiContractTests.cs
```

**Structure Decision**: This is a cross-platform Avalonia application (Desktop + Android) with a shared core library (MTM_Template_Application) containing all business logic, ViewModels, and Views. Platform-specific projects (Desktop/Android) provide platform-dependent implementations via dependency injection. The boot sequence feature adds boot orchestration services, splash screen UI, and cascading data source implementations. Test projects follow constitutional TDD requirements with unit, integration, and contract test separation.

```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->
```
# [REMOVE IF UNUSED] Option 1: Single project (DEFAULT)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [REMOVE IF UNUSED] Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Phase 0: Outline & Research

1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:

   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts

*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach

*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:

1. Load `.specify/templates/tasks-template.md` as base template
2. Generate contract test tasks from `/contracts/` directory:
   - `visual-api-contract.md` → VisualApiContractTests.cs with 6 endpoint test methods
   - `mamp-api-contract.md` → MampApiContractTests.cs with Desktop MySQL + Android proxy test methods
3. Generate model creation tasks from `data-model.md`:
   - 5 core entities (BootStage, ServiceConfiguration, ReachabilityStatus, BootMetrics, CacheManifest)
   - Each entity as separate task for parallel execution
4. Generate service implementation tasks:
   - IBootSequenceService (orchestration)
   - IReachabilityService (health checks)
   - IVisualApiService, IMampDataService, ICacheService (data sources)
   - BootMetricsService (metrics persistence)
5. Generate UI tasks:
   - SplashView.axaml + SplashViewModel (splash screen)
   - MainViewModel updates (status banner, data source indicator)
   - Theme-less splash styling
6. Generate integration test tasks from `quickstart.md`:
   - 12 acceptance scenarios → 12 integration test methods
   - Full boot sequence validation
   - Data source fallback cascade testing
7. Generate platform-specific tasks:
   - Desktop: MampDataService (MySQL.Data client)
   - Android: MampApiProxyService (HttpClient proxy)
   - Single-instance enforcement (Mutex vs ActivityFlags)

**Ordering Strategy**:

- **Phase A: Foundation (Tests First - TDD)** [Tasks 1-10]
  - Contract tests (failing) - All [P] parallel
  - Model creation - All [P] parallel
  - Service interfaces - All [P] parallel

- **Phase B: Services Implementation** [Tasks 11-20]
  - Reachability service (depends on models)
  - Data source services (depends on reachability)
  - Boot orchestration (depends on all services)
  - Metrics persistence (depends on models)

- **Phase C: UI Layer** [Tasks 21-25]
  - SplashView + ViewModel (depends on boot service)
  - MainViewModel updates (depends on data models)
  - Status banner component (depends on data source enums)

- **Phase D: Integration Tests** [Tasks 26-35]
  - Happy path test
  - Fallback cascade tests (3 tests)
  - Error handling tests (5 tests)
  - Performance validation tests

- **Phase E: Platform-Specific** [Tasks 36-40]
  - Desktop MAMP service
  - Android MAMP proxy
  - Single-instance enforcement
  - Cross-platform DI configuration

**Parallelization Markers**:
- [P] = Independent task, can run in parallel
- Sequential tasks have explicit dependencies listed

**Estimated Output**: 40-45 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation

*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

## Progress Tracking

*This checklist is updated during execution flow*

**Phase Status**:

- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:

- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented (none - no violations)

**Artifacts Generated**:

- [x] `research.md` - 13 technical decisions documented with alternatives
- [x] `data-model.md` - 5 core entities with validation rules and state transitions
- [x] `contracts/visual-api-contract.md` - 6 Visual API Toolkit endpoints
- [x] `contracts/mamp-api-contract.md` - Desktop MySQL + Android proxy patterns
- [x] `quickstart.md` - 12 manual test scenarios from acceptance criteria
- [x] `.github/copilot-instructions.md` - Updated with boot sequence context

**Ready for /tasks command**: ✅ All Phase 0-2 gates passed, design artifacts complete

---
*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
