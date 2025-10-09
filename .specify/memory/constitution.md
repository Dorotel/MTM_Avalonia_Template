# MTM Avalonia Template Constitution

<!--
=============================================================================
SYNC IMPACT REPORT - VERSION 1.1.0

Version Change: 1.0.0 → 1.1.0 (Minor Amendment)
Change Type: MINOR (New principle added - Reusable Custom Controls)

Ratification Date: 2025-10-08
Last Amended: 2025-10-08
Rationale: Added Principle XI (Reusable Custom Controls) to reduce UI code
           duplication and enforce consistency across manufacturing views.
           Prevents constant recoding when adding new VISUAL ERP UI elements.

Modified Principles:
- None (existing principles unchanged)

Added Sections:
- Principle XI: Reusable Custom Controls for Manufacturing UI

Removed Sections:
- None

Template Alignment Status:
✅ plan-template.md - Constitution Check section now includes Principle XI
✅ spec-template.md - Requirements align with custom control reuse
✅ tasks-template.md - Task organization supports custom control development
✅ README.md - Boot sequence and architecture documentation aligned
✅ AGENTS.md - AI agent instructions consistent with all 11 principles
✅ .github/copilot-instructions.md - Development guidelines fully aligned
✅ UI-UX-GUIDELINES.md - Custom control catalog completed (Feature 005, Phase 7)

✅ Follow-up TODOs COMPLETED (2025-10-09):
- ✅ Created custom control catalog in docs/UI-CUSTOM-CONTROLS-CATALOG.md (10 controls documented)
- ✅ Documented manufacturing field controls pattern (StatusCard, MetricDisplay, SettingRow, etc.)
- ✅ Added custom control examples to quickstart guide (Feature 005)

=============================================================================
-->

## Core Principles

### I. Spec-Driven Development (NON-NEGOTIABLE)

Every feature MUST follow the complete Spec-Kit workflow before implementation:

1. **Specification** (`SPEC_*.md`): Functional requirements, user stories with priorities (P1, P2, P3), acceptance criteria, and independently testable scenarios
2. **Planning** (`PLAN_*.md`): Technical architecture, implementation approach, technology constraints, performance budgets
3. **Tasks** (`TASKS_*.md`): Granular task breakdown organized by user story priority, enabling incremental delivery
4. **Validation**: 100% task completion, all acceptance criteria met, constitutional compliance verified

**Rationale**: Prevents scope creep, ensures architectural alignment, enables parallel development by multiple agents/developers, and provides audit trail for decisions.

**No feature may bypass this workflow**. All user stories MUST be prioritized and independently testable to enable MVP-first delivery.

### II. Nullable Reference Types (NON-NEGOTIABLE)

ALL C# code MUST have nullable reference types enabled (`<Nullable>enable</Nullable>`):

- Use `?` for all nullable reference types
- Avoid `!` null-forgiving operator except when provably safe (e.g., after explicit null check)
- Use null-conditional operators (`?.`, `??`, `??=`) instead of null checks where appropriate
- All async methods MUST include `CancellationToken` parameter (default to `CancellationToken.None`)

**Rationale**: Prevents `NullReferenceException` at compile-time, reduces runtime errors by 90%+ (industry data), enforces contract clarity between components.

### III. Avalonia CompiledBinding (NON-NEGOTIABLE)

ALL Avalonia XAML files MUST use CompiledBinding with `x:DataType`:

- **ALWAYS** set `x:DataType="vm:ViewModelName"` on Window/UserControl root elements
- **ALWAYS** set `x:CompileBindings="True"` on root elements (project default, but be explicit)
- **ALWAYS** use `{CompiledBinding PropertyName}` syntax (NEVER `{Binding}` or `{ReflectionBinding}`)
- **ALWAYS** include `Design.DataContext` for previewer support

**Rationale**: Compile-time binding validation eliminates 100% of runtime binding errors, provides 30-50% performance improvement over reflection binding (Avalonia benchmarks), enables refactoring with compiler errors.

**Violations WILL break the build** - this is by design to prevent silent runtime failures.

### IV. Test-First Development (REQUIRED)

All new features MUST follow Test-Driven Development (TDD) workflow:

1. **Write tests FIRST** based on acceptance criteria from spec
2. **Verify tests FAIL** (red) before implementation
3. **Implement minimum code** to make tests pass (green)
4. **Refactor** while keeping tests green
5. **Commit** only when tests pass

**Test Organization**:
- **Unit tests**: ViewModels, services, business logic (fast, isolated, >80% coverage target)
- **Integration tests**: Database, API, file system operations (slower, requires infrastructure)
- **Contract tests**: Visual ERP API contract validation (ensures API compatibility)
- **Performance tests**: Boot time (<10s), memory (<100MB), service initialization (<3s)

**Framework**: xUnit + FluentAssertions + NSubstitute (consistent across all tests)

**Rationale**: TDD prevents regressions, enables refactoring confidence, documents behavior, catches edge cases early. Projects with TDD have 40-80% fewer production defects (IBM, Microsoft research).

### V. Performance Budgets (NON-NEGOTIABLE)

All features MUST stay within established performance budgets:

**Boot Sequence**:
- Stage 0 (Splash): <1000ms
- Stage 1 (Services): <3000ms (10 services initialized)
- Stage 2 (Application Ready): <1000ms
- **Total Boot Time: <10 seconds**

**Memory**:
- Cache (LZ4 compressed): ~40MB (3:1 compression ratio)
- Services (DI, connection pools): ~30MB
- Framework (Avalonia + .NET): ~30MB
- **Total: <100MB during startup**

**Operations**:
- Configuration retrieval: <100ms (`GetValue<T>()` with 50+ keys)
- Credential retrieval: <200ms (OS-native storage)
- Feature flag evaluation: <5ms (in-memory cache)
- Database queries: <500ms (user preference persistence)

**Rationale**: Manufacturing environment requires fast, predictable startup. Performance budgets prevent gradual degradation and ensure consistent user experience across deployment sites.

**Measurement**: Performance tests in `tests/integration/PerformanceTests.cs` MUST validate budgets before merge. Debug Terminal provides real-time monitoring.

### VI. MAMP MySQL Database Documentation (NON-NEGOTIABLE 🔴)

ALL database objects (tables, columns, stored procedures, functions, views, indexes) MUST be documented in `.github/mamp-database/` JSON files as the **single source of truth**:

**Schema Documentation Files**:
- `schema-tables.json`: Complete table structures (columns, types, constraints, foreign keys, indexes)
- `stored-procedures.json`: Procedure signatures, parameters, logic
- `functions.json`: User-defined function definitions and return types
- `views.json`: Database view SQL definitions
- `indexes.json`: Index documentation with performance notes
- `sample-data.json`: Test data for development and testing
- `connection-info.json`: Connection settings and environment configs
- `migrations-history.json`: Version history with semantic versioning

**Mandatory Workflow**:
1. **Before writing code**: ALWAYS read `schema-tables.json` to verify table/column names (case-sensitive: `Users`, `UserId`, `PreferenceKey`)
2. **During development**: Reference exact schema from JSON files (no guessing)
3. **After database changes**: IMMEDIATELY update corresponding JSON file(s) and increment version
4. **Before PR**: Ensure `lastUpdated` timestamp is current and version incremented
5. **After merge**: Run database audit to verify JSON accuracy matches actual schema

**Rationale**: MySQL table/column names are case-sensitive in production (Linux) but not in development (Windows). JSON documentation prevents runtime errors from schema mismatches, enables schema evolution tracking, supports automated validation, and provides single source of truth for multiple consumers (C# code, API toolkit, documentation, migration scripts).

**Enforcement**: GitHub Actions workflow validates JSON structure and freshness (<30 days). Code reviews MUST verify schema-tables.json was consulted before database code.

### VII. CommunityToolkit.Mvvm Source Generators (NON-NEGOTIABLE)

ALL ViewModels MUST use CommunityToolkit.Mvvm 8.4.0 source generators (NEVER ReactiveUI):

**Mandatory Patterns**:
- `[ObservableProperty]` for all bindable properties (generates `INotifyPropertyChanged` boilerplate)
- `[RelayCommand]` for all commands (generates `ICommand` properties with async support)
- `partial class` modifier REQUIRED for source generators
- Inherit from `ObservableObject` or `ObservableRecipient`
- Use `[NotifyCanExecuteChangedFor(nameof(CommandName))]` for command enablement

**Rationale**: Source generators eliminate 90% of MVVM boilerplate, enforce consistent patterns, provide compile-time validation, reduce human error. ReactiveUI patterns are explicitly forbidden to maintain codebase consistency.

**No manual INotifyPropertyChanged implementation** - let source generators handle it.

### VIII. Asynchronous Programming with Cancellation (REQUIRED)

ALL async operations MUST support cancellation:

- Every async method MUST have `CancellationToken cancellationToken = default` parameter
- Use `ConfigureAwait(false)` in library code (NOT in UI code - deadlock risk)
- Suffix async methods with `Async`
- Use `ValueTask<T>` for hot paths when appropriate (e.g., cache hits)
- Link cancellation tokens when creating nested operations: `CancellationTokenSource.CreateLinkedTokenSource()`

**Error Handling**:
- Use Polly for retry policies (exponential backoff: 1s, 2s, 4s)
- Use circuit breakers for cascading failures (threshold: 5 failures in 10s → open for 30s)
- Log all retry attempts with structured logging (Serilog)
- Categorize errors with `ErrorCategorizer` for user-friendly recovery

**Rationale**: Manufacturing environment requires responsive UI during long-running operations. Cancellation prevents resource leaks, enables graceful shutdown, and improves user experience. Polly resilience prevents cascading failures from unreliable network/API calls.

### IX. OS-Native Credential Storage (NON-NEGOTIABLE)

ALL credentials (passwords, API keys, tokens) MUST use OS-native secure storage (NEVER hardcoded, NEVER in config files):

**Platform Implementations**:
- **Windows**: `WindowsSecretsService` (DPAPI via Credential Manager)
- **Android**: `AndroidSecretsService` (KeyStore with hardware-backed encryption)
- **macOS/iOS**: Keychain Services (planned)
- **Linux**: Secret Service API (planned)

**Error Handling**:
- Storage unavailable → Show modal dialog prompting credential re-entry
- Dialog cancellation → Application closes with clear warning (FR-013)
- Corrupted credentials → Automatic recovery flow (see `docs/CREDENTIAL-RECOVERY-FLOW.md`)

**Rationale**: Security compliance requirement for manufacturing environment. OS-native storage provides encryption at rest, hardware-backed security (Android KeyStore), audit trails, and separation from application code/config.

**Enforcement**: Code reviews MUST reject hardcoded secrets or credentials in configuration files. Use `ISecretsService` interface for testability.

### X. Graceful Degradation and Offline-First (REQUIRED)

Application MUST continue operating when dependencies are unavailable:

**Offline Capabilities**:
- **Configuration**: Environment variables → User config (cached) → Application defaults
- **User Preferences**: Last-known cached values (persisted to local JSON)
- **Feature Flags**: Last-known cached values (persisted to local JSON)
- **Visual ERP Data**: LZ4-compressed cache (~40MB, 3:1 ratio, stale data acceptable with warning)
- **Credentials**: MUST be available (cannot proceed without - security requirement)

**User Experience**:
- Show warning banner: "Working offline - data may be stale"
- Display cache age indicator: "Last updated: 2 hours ago"
- Queue write operations for sync when reconnected
- Automatic reconnection attempts with exponential backoff (1s, 2s, 4s, 8s, max 60s)

**Rationale**: Manufacturing floor has unreliable network connectivity. Offline-first design ensures productivity continues during network outages, reduces server dependency, and improves perceived performance.

### XI. Reusable Custom Controls for Manufacturing UI (REQUIRED)

ALL frequently-used UI patterns MUST be implemented as reusable Avalonia custom controls (NEVER copy-paste XAML):

**Custom Control Requirements**:
- **Encapsulation**: Complete UI logic in single `.axaml` + `.axaml.cs` file pair
- **Styling Variants**: Support multiple visual variants via style classes (e.g., `ManufacturingField.Notes` for expandable fields)
- **Theme Integration**: Use Theme V2 semantic tokens exclusively (no hardcoded colors/sizes)
- **Bindable Properties**: Expose `AvaloniaProperty` for all data-driven attributes with `x:DataType` support
- **Dependency Properties**: Use `StyledProperty<T>` pattern with proper default values and validation
- **Documentation**: XML comments on all public properties/methods with usage examples

**When to Create Custom Control** (3+ usage threshold):
- Pattern appears in **3 or more views** → Extract to custom control
- Complex layout requiring **10+ lines of XAML** → Candidate for encapsulation
- Behavior requires **code-behind logic** → Custom control with attached properties
- Styling variants needed across **multiple contexts** → Style-class-based control

**Control Location & Naming**:
- Path: `MTM_Template_Application/Controls/{Domain}/{ControlName}.axaml`
- Namespace: `MTM_Template_Application.Controls.{Domain}`
- Naming: `{Purpose}{Type}` (e.g., `ManufacturingField`, `BarcodeInput`, `StatusBadge`, `ConnectionHealthIndicator`)
- Base classes: Inherit from `UserControl`, `ContentControl`, or `TemplatedControl` based on complexity

**Styling Architecture**:
- **Base styles**: Define common properties in `ControlName` selector
- **Variant styles**: Use compound selectors (`ControlName.VariantName`) for specific overrides
- **State styles**: Use pseudo-classes (`:pointerover`, `:focus-within`, `:disabled`) for interaction states
- **Container constraints**: Use `ClipToBounds="True"` and `Margin="0"` for proper boundary containment

**Manufacturing-Specific Controls** (established patterns):
- `ManufacturingField`: Form field with label, input, validation, offline indicator (base + Notes/Barcode/Numeric variants)
- `BarcodeInput`: Text input optimized for scanner integration with validation and focus management
- `StatusBadge`: Color-coded status indicator with icon + text (Open/InProgress/Urgent/Closed variants)
- `ConnectionHealthIndicator`: Persistent VISUAL ERP connection status (Online/Degraded/Offline with tooltip)
- `CachedDataBanner`: Warning banner showing cache age and last sync timestamp
- `QuickFilterChip`: Pill-shaped toggle button for common filter scenarios (reusable across grids)
- `TransactionConfirmationToast`: Non-blocking notification with auto-dismiss and action buttons

**Rationale**: Manufacturing UI has repetitive patterns (part lookups, work order lists, inventory forms). Custom controls prevent copy-paste errors, ensure consistent behavior, enable centralized bug fixes, reduce XAML from 500+ lines to 50-100 lines per view, and accelerate feature development by 40-60% (measured in Features 001-003). Copy-pasting XAML creates maintenance debt where bugs must be fixed in 5+ locations.

**Enforcement**: Code reviews MUST reject duplicate XAML patterns. When reviewers identify 2+ similar XAML blocks, request custom control extraction before merge. Existing controls documented in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` (catalog created during Feature 005 UI enhancement work).

## Technology Stack Requirements

### Language and Framework (Fixed Versions)
- **Language**: C# with `<LangVersion>latest</LangVersion>` targeting .NET 9.0
- **Nullable Reference Types**: ENABLED (`<Nullable>enable</Nullable>`) - project-wide, no exceptions
- **UI Framework**: Avalonia UI 11.3.6 (cross-platform XAML)
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.4.0 (source generators)
- **Compiled Bindings**: DEFAULT (`<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>`)

### Infrastructure Dependencies
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection 9.0.0
- **Logging**: Serilog.Extensions.Logging 8.0.0 (structured logging)
- **Observability**: OpenTelemetry (optional Jaeger export)
- **Resilience**: Polly 8.4.2 (retry policies, circuit breakers)
- **Mapping**: AutoMapper 13.0.1 (DTO/model mapping)
- **Validation**: FluentValidation 11.10.0 (input validation)

### Data Storage
- **Database**: MySql.Data 9.0.0 against MAMP MySQL 5.7 (parameterized queries ONLY)
- **Cache**: K4os.Compression.LZ4 1.3.8 (local cache, ~3:1 compression ratio)
- **Secrets**: OS-native secure storage (DPAPI/KeyStore/Keychain)

### Testing
- **Test Framework**: xUnit 2.9.2
- **Mocking**: NSubstitute 5.1.0
- **Assertions**: FluentAssertions 6.12.1

**Rationale**: Fixed versions prevent compatibility issues, ensure reproducible builds, and enable automated dependency updates with confidence.

**Version Changes**: ALL package version changes MUST be documented in `Directory.Packages.props` with rationale in PR description. Breaking changes require constitutional review.

## Development Workflow

### Feature Development (Spec-Kit Workflow)
1. **Create specification**: Use `/specify` prompt to generate `SPEC_*.md` with prioritized user stories
2. **Clarify requirements**: Use `/clarify` prompt to resolve ambiguities (prevents rework)
3. **Generate plan**: Use `/plan` prompt to create `PLAN_*.md` with technical architecture
4. **Break down tasks**: Use `/tasks` prompt to generate `TASKS_*.md` organized by user story priority
5. **Implement incrementally**: Complete Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3+ (User Stories by priority)
6. **Validate continuously**: Run tests after each task, validate user stories independently
7. **Complete validation**: Use `validate-implementation.ps1` script to verify 100% task completion and constitutional compliance

### Code Review Requirements
All pull requests MUST verify:
1. **Nullable safety**: Proper `?` annotations, no unnecessary `!` operators
2. **XAML bindings**: CompiledBinding with `x:DataType` everywhere
3. **Async patterns**: `CancellationToken` support, no blocking calls (`.Result`, `.Wait()`)
4. **Error handling**: Comprehensive try-catch with `ErrorCategorizer`
5. **Testing**: Unit tests for ViewModels, integration tests for services
6. **Performance**: Stays within budgets (<10s boot, <100MB memory, <3s service initialization)
7. **Database**: References `schema-tables.json` before writing queries, uses parameterized queries
8. **Constitutional compliance**: Validation script passes with zero blocking issues

### Commit Conventions
- Use conventional commit format: `type(scope): description`
- Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `style`
- Reference feature ID: `feat(001): add splash screen timeout handler`
- Include task ID: `feat(001): implement Stage1 services (T012, T013, T014)`

### Branch Naming
- Feature branches: `###-feature-name` (e.g., `001-boot-sequence-splash`)
- Hotfix branches: `hotfix/brief-description`
- Experiment branches: `experiment/brief-description` (not merged to main)

## Quality Standards

### Code Quality
- **Coverage Target**: >80% for critical paths (boot sequence, configuration, secrets)
- **Cyclomatic Complexity**: <10 per method (enforce with analyzer)
- **File Length**: <500 lines (split into multiple files if exceeded)
- **Method Length**: <50 lines (extract helper methods)
- **Class Coupling**: <10 dependencies (use interfaces for testability)

### Performance Monitoring
- **Boot Metrics**: Tracked via Debug Terminal (real-time performance snapshots)
- **Memory Profiling**: Use dotMemory for leak detection before major releases
- **Build Time**: <60 seconds for full solution rebuild (warm)
- **Test Execution**: <30 seconds for all unit tests, <5 minutes for integration tests

### Documentation Requirements
- **Code Comments**: XML documentation for all public APIs
- **Architecture Decisions**: Record in `docs/` directory with rationale
- **User Guides**: Markdown files in `docs/` for features with user-facing changes
- **Database Changes**: Update `migrations-history.json` immediately after schema changes

### Accessibility
- **Keyboard Navigation**: All UI elements accessible via keyboard
- **Screen Reader Support**: ARIA labels on all interactive elements
- **High Contrast Mode**: Theme V2 semantic tokens ensure visibility
- **Font Scaling**: UI layout adapts to system font size settings (up to 200%)

## Governance

### Constitution Authority
This constitution supersedes all other development practices, style guides, and team preferences. When conflicts arise, **constitution wins**.

### Amendment Process
1. **Proposal**: Document proposed change with rationale and impact analysis
2. **Discussion**: Review with team and stakeholders (minimum 3 business days)
3. **Approval**: Requires consensus (blocking concerns must be addressed)
4. **Migration Plan**: Document how existing code will be updated (if applicable)
5. **Version Bump**: Increment version according to semantic versioning:
   - **MAJOR**: Backward incompatible governance/principle removals or redefinitions
   - **MINOR**: New principle/section added or materially expanded guidance
   - **PATCH**: Clarifications, wording, typo fixes, non-semantic refinements
6. **Template Sync**: Update all affected templates in `.specify/templates/`
7. **Communication**: Announce change to all contributors with effective date

### Compliance Review
- **Pre-merge**: Validation script checks constitutional compliance (automated)
- **Weekly**: Team reviews recent PRs for principle adherence (spot check)
- **Quarterly**: Constitutional audit to identify drift and technical debt
- **Annual**: Full constitutional review to assess relevance and effectiveness

### Complexity Justification
When a feature requires violating a principle:
1. Document the violation in `PLAN_*.md` Complexity Tracking section
2. Explain why the principle doesn't apply (specific context)
3. Propose simpler alternative and explain why it was rejected
4. Get explicit approval in PR review (requires maintainer sign-off)
5. Add technical debt item to revisit in future refactoring

### Runtime Development Guidance
For day-to-day development patterns and AI agent instructions:
- **AI Agents**: See `AGENTS.md` for comprehensive agent context
- **Developers**: See `.github/copilot-instructions.md` for coding standards
- **Domain Patterns**: See `.github/instructions/*.instructions.md` for specific patterns (Avalonia UI, database integration, debugging workflows)

**Constitution vs. Guidance**: Constitution defines **WHAT** must be done (principles, requirements). Guidance documents define **HOW** to do it (patterns, examples, troubleshooting).

---

**Version**: 1.1.0 | **Ratified**: 2025-10-08 | **Last Amended**: 2025-10-08
