# Implementation Plan: Manufacturing Application Modernization - All-in-One Mega-Feature

**Branch**: `005-migrate-infor-visual` | **Date**: 2025-10-08 (Updated: 2025-10-09) | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-migrate-infor-visual/spec.md`

**Note**: This plan covers 5 integrated phases as defined in spec.md and RESTART-GUIDE.md.

---

## 📑 Table of Contents

### Planning Overview
- [Summary](#summary) - High-level feature overview and phase descriptions
- [Technical Context](#technical-context) - Language, dependencies, storage, testing

### Constitutional Validation
- [Constitution Check](#constitution-check) - 11 principle validation with evidence
  - [I: Spec-Driven Development](#principle-i-spec-driven-development--pass)
  - [II: Nullable Reference Types](#principle-ii-nullable-reference-types--pass)
  - [III: Avalonia CompiledBinding](#principle-iii-avalonia-compiledbinding--pass)
  - [IV: Test-First Development](#principle-iv-test-first-development--pass)
  - [V: Performance Budgets](#principle-v-performance-budgets--pass)
  - [VI: MySQL Schema Documentation](#principle-vi-mamp-mysql-database-documentation--pass)
  - [VII: MVVM Source Generators](#principle-vii-communitytoolkitmvvm-source-generators--pass)
  - [VIII: Async with Cancellation](#principle-viii-asynchronous-programming-with-cancellation--pass)
  - [IX: OS-Native Credentials](#principle-ix-os-native-credential-storage--pass)
  - [X: Dependency Injection](#principle-x-dependency-injection--pass)
  - [XI: Reusable Controls](#principle-xi-reusable-custom-controls--pass)

### Architecture & Design
- [High-Level Architecture](#high-level-architecture) - System layers and component interaction
- [Phase-Specific Architecture](#phase-specific-architecture) - Detailed designs per phase
  - [Phase 1: Custom Controls](#phase-1-custom-controls-library-foundation)
  - [Phase 2: Settings UI](#phase-2-settings-management-ui)
  - [Phase 3: Debug Terminal](#phase-3-debug-terminal-modernization)
  - [Phase 4: Config Dialog](#phase-4-configuration-error-dialog)
  - [Phase 5: VISUAL Integration](#phase-5-visual-erp-integration)

### Implementation Details
- [Service Layer Design](#service-layer-design) - Service responsibilities and interactions
- [Data Flow Patterns](#data-flow-patterns) - Request/response sequences
- [Error Handling Strategy](#error-handling-strategy) - Recovery patterns and fallback
- [Testing Approach](#testing-approach) - Test categories, mocking, coverage

### Risk & Validation
- [Risk Assessment](#risk-assessment) - Identified risks and mitigation strategies
- [Performance Validation](#performance-validation) - Benchmarking approach
- [Open Questions](#open-questions) - Items requiring clarification

### Reference
- [Related Files](#related-files) - Links to spec.md, tasks.md, data-model.md
- [Dependencies Matrix](#dependencies-matrix) - Phase dependencies and critical path

---

## Summary

Comprehensive manufacturing application modernization with 5 tightly-integrated phases:

**Phase 1 - Custom Controls Library** (Foundation): Extract 10 reusable Avalonia custom controls from existing UIs (DebugTerminalWindow, MainWindow) to eliminate XAML duplication (60%+ reduction). Controls include StatusCard, MetricDisplay, ErrorListPanel, ConnectionHealthBadge, BootTimelineChart, SettingsCategory, SettingRow, NavigationMenuItem, ConfigurationErrorDialog, and ActionButtonGroup. Each control uses StyledProperty pattern for bindable properties and is documented in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with usage examples and screenshots. Foundation for all subsequent phases. Addresses Constitution Principle XI (reusable custom controls) and TODO from constitution.md lines 34-37.

**Phase 2 - Settings Management UI**: Build comprehensive settings window with tabbed navigation for 8 categories (General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer) exposing all 60+ IConfigurationService settings. Uses custom controls from Phase 1 (SettingsCategory, SettingRow). Supports validation (FluentValidation patterns for connection strings, URLs, file paths), staging (changes not applied until Save), import/export (JSON with filtered sensitive values), and persistence (UserPreferences table per user). Enables non-technical users to configure application without editing files, reducing support burden.

**Phase 3 - Debug Terminal Modernization**: Complete rewrite of DebugTerminalWindow with SplitView navigation, collapsible side panel with hamburger menu, and feature-based organization ("Feature 001: Boot", "Feature 002: Config", "Feature 003: Diagnostics", "Feature 005: VISUAL"). Uses custom controls from Phase 1 (StatusCard, MetricDisplay, ErrorListPanel, BootTimelineChart). Addresses Feature 003 TODOs: CopyToClipboardCommand (actual clipboard integration), IsMonitoring toggle (5-second snapshot collection), Environment Variables display (filtered sensitive vars showing "***FILTERED***"). Performance budget: <500ms load time.

**Phase 4 - Configuration Error Dialog**: Small focused feature creating ConfigurationErrorDialog modal for graceful configuration error handling during startup/runtime. Shows error details (message, category, affected setting key, timestamp) with recovery options: "Edit Settings" (opens Settings window to relevant tab), "Retry Operation", "Exit Application". Integrates with MainWindow.axaml error paths (addresses Feature 002 TODO from line 1234). Uses custom controls from Phase 1.

**Phase 5 - Visual ERP Integration**: Migrate Infor VISUAL ERP integration from custom HTTP endpoints to official Infor VISUAL API Toolkit for reliable access to part information, work orders, inventory transactions, and shipment data. The integration follows a **READ-ONLY architecture** (management security policy) where VISUAL API Toolkit provides data queries only, and all write operations (work order updates, inventory transactions, shipment confirmations) persist exclusively to local MAMP MySQL database. Must maintain current performance (<3s queries), support offline operation with LZ4-compressed cache, and provide hybrid performance monitoring integrated into DebugTerminalWindow.axaml. Windows desktop only (Toolkit x86 .NET 4.x limitation). Uses custom controls from Phase 1 for VISUAL performance panel.

## Technical Context

**Language/Version**: C# (LangVersion: latest) on .NET 9.0 with nullable reference types enabled

**Primary Dependencies**:

### Phase 1 - Custom Controls Library
- **Avalonia UI 11.3.6** - Custom control base classes, StyledProperty pattern
- **Avalonia.Markup.Xaml** - XAML compilation and resources
- **Key Patterns**: `TemplatedControl` base class, `StyledProperty<T>` for bindable properties, `OnPropertyChanged` callbacks, `ControlTheme` definitions
- **Documentation Target**: `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with properties tables, usage examples, screenshots

### Phase 2 - Settings Management UI
- **IConfigurationService** (existing, Feature 002) - 60+ settings access and persistence
- **FluentValidation 11.10.0** - Validation for connection strings, URLs, file paths, numeric ranges
- **UserPreferences table** (existing, Feature 002) - Per-user settings persistence in MAMP MySQL
- **System.Text.Json** - Settings import/export to JSON format
- **Avalonia TabControl** - Category navigation
- **Custom Controls from Phase 1**: SettingsCategory, SettingRow, ActionButtonGroup

### Phase 3 - Debug Terminal Modernization
- **Avalonia SplitView** - Collapsible side panel navigation
- **DiagnosticSnapshot, BootTimelineEntry, ErrorLogEntry** (existing, Feature 003) - Data models for diagnostics
- **System.Windows.Clipboard** (or Avalonia.Input.Platform) - Clipboard integration for CopyToClipboardCommand
- **Custom Controls from Phase 1**: StatusCard, MetricDisplay, ErrorListPanel, BootTimelineChart, NavigationMenuItem
- **Performance Budget**: <500ms window load time

### Phase 4 - Configuration Error Dialog
- **Avalonia Window** - Modal dialog base
- **IConfigurationService** (existing) - Error context (affected setting key)
- **MainWindow error paths** (existing, Feature 002) - Integration points for showing dialog
- **Custom Controls from Phase 1**: ConfigurationErrorDialog (the control itself), ActionButtonGroup

### Phase 5 - Visual ERP Integration
- **Infor VISUAL API Toolkit** (Windows x86 .NET 4.x) - READ-ONLY data access
  - ✅ **AVAILABLE LOCALLY**: 9 DLLs at `docs\Visual Files\ReferenceFiles\` (LsaCore, LsaShared, VmfgFinancials, VmfgInventory, VmfgPurchasing, VmfgSales, VmfgShared, VmfgShopFloor, VmfgTrace)
  - ✅ **DOCUMENTATION AVAILABLE**: 7 reference guides at `docs\Visual Files\Guides\` (Development Guide, Core, Inventory, Shared Library, Shop Floor, VMFG Shared Library, User Manual)
  - ✅ **DATABASE SCHEMA**: 4 CSV files at `docs\Visual Files\Database Files\` (Tables, Procedures, Relationships, Visual Data Table)
  - **MAJOR RISK ELIMINATED**: API Toolkit documentation accessibility changed from UNKNOWN/HIGH to RESOLVED (discovery date: 2025-10-08)
- **CommunityToolkit.Mvvm 8.4.0** - Source generators for ViewModels
- **MySql.Data 9.0.0** - MAMP MySQL 5.7 client (system of record for writes)
- **K4os.Compression.LZ4 1.3.8** - Cache compression (~3:1 ratio)
- **WindowsSecretsService** (existing) - DPAPI credential storage
- **CacheService/CacheStalenessDetector** (existing) - TTL management (Parts: 24h, Others: 7d)
- **Polly 8.4.2** - Retry policies and circuit breakers
- **Custom Controls from Phase 1**: StatusCard, MetricDisplay, ErrorListPanel, ConnectionHealthBadge for VISUAL performance panel

**Storage**:
- **MAMP MySQL 5.7** (local database) - System of record for user-entered transactions (work orders, inventory, shipments)
- **LZ4 Cache** (~40MB compressed) - VISUAL data cache for offline operation
- **Windows Credential Manager** (DPAPI) - Secure credential storage

**Testing**:
- **xUnit 2.9.2** - Test framework
- **NSubstitute 5.1.0** - Mocking
- **FluentAssertions 6.12.1** - Assertions
- **Test Categories**: Unit (ViewModels, services), Integration (database, API), Contract (VISUAL API compatibility), Performance (response times, cache hit rates)

**Target Platform**: Windows Desktop only (x86 architecture) - Infor VISUAL API Toolkit limitation
**Project Type**: Desktop application (Avalonia UI) with MVVM architecture

**Performance Goals**:
- Part lookup: <3s (99.9% of requests)
- Work order queries: <2s (up to 500 records)
- Work order status updates: <2s (local MySQL persist)
- Inventory transaction recording: <5s (local MySQL persist)
- Shipment confirmation: <3s (local MySQL persist)
- Cached data access (offline): <1s
- Auto-refresh after reconnection: <30s
- Boot time: <10s total (existing budget)

**Constraints**:
- **Read-only VISUAL access** (management security policy - no direct writes to VISUAL server)
- **MAMP MySQL as system of record** for all write operations from this application
- **Windows-only deployment** (Toolkit x86 .NET 4.x constraint)
- **Offline-first operation** (manufacturing floor has unreliable network)
- **Memory budget**: Existing <100MB startup budget (cache ~40MB of budget)
- **Cache TTL**: Parts 24h, other entities 7d (already implemented via CacheStalenessDetector)
- **No ReactiveUI** (CommunityToolkit.Mvvm only per constitution)
- **CompiledBinding only** (no Binding or ReflectionBinding)
- **Nullable reference types** (explicit ? annotations required)

**Scale/Scope**:
- **Users**: 50-100 concurrent manufacturing floor users
- **Data Volume**: NEEDS CLARIFICATION (How many parts, work orders, inventory transactions in production VISUAL database?)
- **API Call Frequency**: ~200-300 VISUAL API calls per user per shift (estimate)
- **Cache Size**: ~40MB compressed (~120MB uncompressed)
- **Local Transaction Volume**: NEEDS CLARIFICATION (Expected daily transaction count to MAMP MySQL?)
- **UI Screens**: 5-7 new/modified views (Part Lookup, Work Order List/Detail, Inventory Transaction, Shipment Confirmation, VISUAL Performance panel in DebugTerminalWindow)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Spec-Driven Development ✅ PASS
- **Status**: Followed complete workflow (Specification → Clarification → Planning)
- **Evidence**:
  - `spec.md` complete with 5 prioritized user stories (P1, P2, P3)
  - All 4 clarification questions resolved (Session 2025-10-08)
  - 18 functional requirements with acceptance criteria
  - Planning readiness checklist: 60/60 checks passed (100%)
- **Next**: Tasks will be organized by user story priority for incremental MVP delivery

### Principle II: Nullable Reference Types ✅ PASS
- **Status**: All C# code will use nullable reference types (`<Nullable>enable</Nullable>`)
- **Evidence**: Project-wide nullable enabled, ? annotations required for all reference types
- **Implementation**: ViewModels, services, API wrappers will use explicit nullability
- **Risk**: None - established pattern in existing codebase

### Principle III: Avalonia CompiledBinding ✅ PASS
- **Status**: All XAML will use `x:DataType` with `{CompiledBinding}` syntax
- **Evidence**: Project default `<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>`
- **Implementation**:
  - All new Views (PartLookupView, WorkOrderListView, etc.) will set `x:DataType="vm:ViewModelName"`
  - DebugTerminalWindow.axaml VISUAL Performance panel will use CompiledBinding
- **Risk**: None - established pattern in existing codebase

### Principle IV: Test-First Development ✅ PASS
- **Status**: TDD workflow will be followed for all new features
- **Testing Strategy**:
  - **Unit Tests**: ViewModels (PartLookupViewModel, WorkOrderListViewModel, InventoryTransactionViewModel)
  - **Integration Tests**: VISUAL API Toolkit wrappers, MAMP MySQL persistence, cache operations
  - **Contract Tests**: VISUAL API Toolkit endpoint compatibility (validate read-only operations)
  - **Performance Tests**: Response time validation (<3s part lookup, <2s work orders, <5s inventory, <3s shipments)
- **Coverage Target**: >80% for critical paths (VISUAL API wrappers, cache fallback, local persistence)
- **Risk**: Low - xUnit/FluentAssertions/NSubstitute already established

### Principle V: Performance Budgets ✅ PASS
- **Status**: Feature stays within existing performance budgets
- **Budget Allocation**:
  - **Boot Time**: No impact (services initialized in existing Stage 1)
  - **Memory**: Cache ~40MB (within existing <100MB budget)
  - **Operations**: All operations <5s (part lookup <3s, work orders <2s, inventory <5s, shipments <3s)
- **Monitoring**: Hybrid approach (structured logs + DebugTerminalWindow.axaml VISUAL Performance panel)
- **Risk**: Low - performance targets specified in FR-001 through FR-005, will be validated via performance tests

### Principle VI: MAMP MySQL Database Documentation ✅ PASS
- **Status**: Will create/update `.github/mamp-database/schema-tables.json` for Local Transaction Records table
- **New Schema Objects**:
  - **Table**: `LocalTransactionRecords` (transaction_id, transaction_type, entity_data JSON, created_at, created_by, visual_reference_data, audit_metadata)
  - **Indexes**: ON transaction_type, created_at, created_by for query performance
- **Implementation**: MUST reference `schema-tables.json` before writing any queries, use parameterized queries ONLY
- **Documentation Update**: Will update `schema-tables.json` immediately after schema creation, increment version
- **Risk**: Low - established pattern, code reviews enforce schema-tables.json consultation

### Principle VII: CommunityToolkit.Mvvm Source Generators ✅ PASS
- **Status**: All ViewModels will use `[ObservableProperty]` and `[RelayCommand]` source generators
- **Evidence**: No ReactiveUI patterns, all ViewModels inherit from `ObservableObject`
- **Implementation**:
  - PartLookupViewModel: `[ObservableProperty]` for PartNumber, SearchResults; `[RelayCommand]` for SearchPartCommand
  - WorkOrderListViewModel: `[ObservableProperty]` for WorkOrders, FilterStatus; `[RelayCommand]` for LoadWorkOrdersCommand
  - InventoryTransactionViewModel: `[ObservableProperty]` for Transaction, PartInfo; `[RelayCommand]` for SaveTransactionCommand
  - ShipmentConfirmationViewModel: `[ObservableProperty]` for Shipment, OrderInfo; `[RelayCommand]` for ConfirmShipmentCommand
- **Risk**: None - established pattern in existing codebase

### Principle VIII: Asynchronous Programming with Cancellation ✅ PASS
- **Status**: All async methods will have `CancellationToken cancellationToken = default` parameter
- **Evidence**: All VISUAL API calls, database operations, cache operations will support cancellation
- **Error Handling**: Polly retry policies for transient failures (exponential backoff: 1s, 2s, 4s), circuit breaker for cascading failures
- **Implementation**:
  - VISUAL API wrappers: `GetPartAsync(string partNumber, CancellationToken cancellationToken)`
  - Database operations: `SaveTransactionAsync(LocalTransactionRecord transaction, CancellationToken cancellationToken)`
  - Cache operations: `GetCachedPartAsync(string partNumber, CancellationToken cancellationToken)`
- **Risk**: Low - Polly 8.4.2 already used in existing services

### Principle IX: OS-Native Credential Storage ✅ PASS
- **Status**: Will reuse existing `WindowsSecretsService` (DPAPI) for VISUAL credentials
- **Evidence**: FR-009 specifies Username/Password stored in Windows Credential Manager (keys: "Visual.Username", "Visual.Password")
- **Implementation**: Already implemented - no new credential storage code required
- **Error Handling**: Automatic credential recovery dialog on corruption/unavailability (existing pattern)
- **Risk**: None - already implemented and tested (WindowsSecretsServiceTests.cs)

### Principle X: Graceful Degradation and Offline-First ✅ PASS
- **Status**: Feature designed for offline-first operation with LZ4-compressed cache
- **Offline Capabilities**:
  - **Read Operations**: Serve from cache when VISUAL unavailable (FR-006: <1s cached data access)
  - **Write Operations**: Always persist to MAMP MySQL regardless of VISUAL connectivity (FR-007: write operations independent of VISUAL)
  - **Reconnection**: Auto-refresh cached VISUAL data within 30s (FR-008)
  - **Degradation**: Automatic degradation mode after 5 consecutive failures (FR-020)
- **User Experience**: Warning banner "Working offline - data may be stale", cache age indicator "Last updated: [timestamp]"
- **Risk**: Low - cache patterns already established (CacheService, CacheStalenessDetector)

### Principle XI: Reusable Custom Controls ✅ PASS
- **Status**: Phase 1 extracts 10 reusable Avalonia custom controls from existing UIs (addresses constitution.md TODO lines 34-37)
- **Evidence**: Custom Controls Library phase extracts StatusCard, MetricDisplay, ErrorListPanel, ConnectionHealthBadge, BootTimelineChart, SettingsCategory, SettingRow, NavigationMenuItem, ConfigurationErrorDialog, ActionButtonGroup
- **Implementation**:
  - All controls use StyledProperty pattern for bindable properties
  - Each control has `ControlTheme` definition for styling
  - Complete documentation in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with properties, usage examples, screenshots
  - 80%+ test coverage for property changes, validation, rendering
- **Impact**: Reduces XAML duplication by 60%+, enables consistent UI patterns across Settings, Debug Terminal, and VISUAL integration
- **Risk**: Low - StyledProperty pattern already used in Avalonia framework, well-documented

---

### Overall Constitutional Compliance: ✅ PASS (11/11 principles)

**Gates**: All constitutional principles satisfied. No violations requiring justification.

**Proceed to Phase 0**: Research unknowns (data volume, API endpoint discovery, connection pooling)

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
MTM_Template_Application/                      # Shared Avalonia UI library
├── Controls/                                  # NEW: Custom Controls Library (Phase 1)
│   ├── StatusCard.axaml / .cs                 # Status display with icon/title/status
│   ├── MetricDisplay.axaml / .cs              # Metric display with value/label/trend
│   ├── ErrorListPanel.axaml / .cs             # Error list with severity icons
│   ├── ConnectionHealthBadge.axaml / .cs      # Connection health indicator
│   ├── BootTimelineChart.axaml / .cs          # Boot timeline visualization
│   ├── SettingsCategory.axaml / .cs           # Settings category container
│   ├── SettingRow.axaml / .cs                 # Individual setting row
│   ├── NavigationMenuItem.axaml / .cs         # Navigation menu item
│   ├── ConfigurationErrorDialog.axaml / .cs   # Configuration error dialog
│   └── ActionButtonGroup.axaml / .cs          # Action button group
├── ViewModels/
│   ├── Settings/                              # NEW: Settings Management (Phase 2)
│   │   ├── SettingsViewModel.cs               # Main settings window ViewModel
│   │   ├── GeneralSettingsViewModel.cs        # General category (theme, language, startup)
│   │   ├── DatabaseSettingsViewModel.cs       # Database category (connection strings)
│   │   ├── VisualSettingsViewModel.cs         # VISUAL ERP category (API endpoint, credentials)
│   │   ├── LoggingSettingsViewModel.cs        # Logging category (levels, targets)
│   │   ├── UiSettingsViewModel.cs             # UI category (theme, fonts, layout)
│   │   ├── CacheSettingsViewModel.cs          # Cache category (TTL, size, policies)
│   │   ├── PerformanceSettingsViewModel.cs    # Performance category (budgets, thresholds)
│   │   └── DeveloperSettingsViewModel.cs      # Developer category (debug, tracing)
│   ├── PartLookupViewModel.cs                 # NEW: Part search/display (FR-001) - Phase 5
│   ├── WorkOrderListViewModel.cs              # NEW: Work order filtering (FR-002) - Phase 5
│   ├── WorkOrderDetailViewModel.cs            # NEW: Work order details/status update (FR-003) - Phase 5
│   ├── InventoryTransactionViewModel.cs       # NEW: Inventory transaction recording (FR-004) - Phase 5
│   ├── ShipmentConfirmationViewModel.cs       # NEW: Shipment confirmation (FR-005) - Phase 5
│   └── DebugTerminalViewModel.cs              # MODIFIED: Add VISUAL Performance panel (FR-021) - Phase 3 rewrite + Phase 5 VISUAL panel
├── Views/
│   ├── Settings/                              # NEW: Settings Management UI (Phase 2)
│   │   ├── SettingsWindow.axaml               # Main settings window with tab navigation
│   │   ├── GeneralSettingsView.axaml          # General category UI
│   │   ├── DatabaseSettingsView.axaml         # Database category UI
│   │   ├── VisualSettingsView.axaml           # VISUAL ERP category UI
│   │   ├── LoggingSettingsView.axaml          # Logging category UI
│   │   ├── UiSettingsView.axaml               # UI category UI
│   │   ├── CacheSettingsView.axaml            # Cache category UI
│   │   ├── PerformanceSettingsView.axaml      # Performance category UI
│   │   └── DeveloperSettingsView.axaml        # Developer category UI
│   ├── PartLookupView.axaml                   # NEW: Part search UI - Phase 5
│   ├── WorkOrderListView.axaml                # NEW: Work order list/filter UI - Phase 5
│   ├── WorkOrderDetailView.axaml              # NEW: Work order detail/status UI - Phase 5
│   ├── InventoryTransactionView.axaml         # NEW: Inventory transaction UI - Phase 5
│   ├── ShipmentConfirmationView.axaml         # NEW: Shipment confirmation UI - Phase 5
│   └── DebugTerminalWindow.axaml              # MODIFIED: Complete rewrite with SplitView navigation (Phase 3) + VISUAL API Performance panel (Phase 5)
├── Models/
│   ├── Visual/                                # NEW: VISUAL ERP domain models - Phase 5
│   │   ├── Part.cs                            # Part entity (spec §Key Entities)
│   │   ├── WorkOrder.cs                       # Work Order entity
│   │   ├── InventoryTransaction.cs            # Inventory Transaction entity
│   │   ├── CustomerOrder.cs                   # Customer Order entity
│   │   └── Shipment.cs                        # Shipment entity
│   ├── LocalTransactionRecord.cs              # NEW: Local MySQL transaction record - Phase 5
│   └── VisualPerformanceMetrics.cs            # NEW: Performance monitoring data - Phase 5
├── Services/
│   ├── Visual/                                # NEW: VISUAL API Toolkit integration
│   │   ├── IVisualApiService.cs               # Interface for DI/testing
│   │   ├── VisualApiService.cs                # Wrapper for VISUAL API Toolkit (read-only)
│   │   ├── VisualPartsRepository.cs           # Part queries (FR-001)
│   │   ├── VisualWorkOrderRepository.cs       # Work order queries (FR-002)
│   │   ├── VisualInventoryRepository.cs       # Inventory balance queries
│   │   └── VisualShipmentRepository.cs        # Shipment/order queries
│   ├── DataLayer/
│   │   └── LocalTransactionRepository.cs      # NEW: MAMP MySQL CRUD for local transactions (FR-003, FR-004, FR-005, FR-017)
│   ├── Cache/
│   │   └── VisualCacheService.cs              # NEW: VISUAL-specific cache operations (reuses existing CacheService)
│   ├── Diagnostics/
│   │   ├── VisualPerformanceMonitor.cs        # NEW: Performance monitoring (FR-019)
│   │   └── VisualDegradationDetector.cs       # NEW: Auto-degradation logic (FR-020)
│   └── Validation/
│       └── TransactionValidator.cs            # NEW: Validate transactions against cached VISUAL data (FR-013)
├── Extensions/
│   └── VisualServiceExtensions.cs             # NEW: DI registration for VISUAL services

tests/
├── unit/
│   ├── Controls/                              # NEW: Custom control tests (Phase 1)
│   │   ├── StatusCardTests.cs                 # StatusCard property/rendering tests
│   │   ├── MetricDisplayTests.cs              # MetricDisplay property/rendering tests
│   │   ├── ErrorListPanelTests.cs             # ErrorListPanel tests
│   │   ├── ConnectionHealthBadgeTests.cs      # ConnectionHealthBadge tests
│   │   ├── BootTimelineChartTests.cs          # BootTimelineChart tests
│   │   ├── SettingsCategoryTests.cs           # SettingsCategory tests
│   │   ├── SettingRowTests.cs                 # SettingRow tests
│   │   ├── NavigationMenuItemTests.cs         # NavigationMenuItem tests
│   │   ├── ConfigurationErrorDialogTests.cs   # ConfigurationErrorDialog tests
│   │   └── ActionButtonGroupTests.cs          # ActionButtonGroup tests
│   ├── ViewModels/
│   │   ├── Settings/                          # NEW: Settings ViewModel tests (Phase 2)
│   │   │   ├── SettingsViewModelTests.cs      # Main settings ViewModel tests
│   │   │   ├── GeneralSettingsViewModelTests.cs
│   │   │   ├── DatabaseSettingsViewModelTests.cs
│   │   │   └── (other 6 category ViewModel tests)
│   │   ├── PartLookupViewModelTests.cs        # NEW: Part lookup ViewModel tests - Phase 5
│   │   ├── WorkOrderListViewModelTests.cs     # NEW: Work order list ViewModel tests - Phase 5
│   │   ├── WorkOrderDetailViewModelTests.cs   # NEW: Work order detail ViewModel tests - Phase 5
│   │   ├── InventoryTransactionViewModelTests.cs  # NEW: Inventory ViewModel tests - Phase 5
│   │   └── ShipmentConfirmationViewModelTests.cs  # NEW: Shipment ViewModel tests - Phase 5
│   └── Services/
│       ├── VisualApiServiceTests.cs           # NEW: VISUAL API wrapper tests - Phase 5
│       ├── LocalTransactionRepositoryTests.cs # NEW: Local MySQL persistence tests - Phase 5
│       └── VisualPerformanceMonitorTests.cs   # NEW: Performance monitoring tests - Phase 5
├── integration/
│   ├── SettingsPersistenceTests.cs            # NEW: Settings persistence to UserPreferences (Phase 2)
│   ├── DebugTerminalNavigationTests.cs        # NEW: Debug Terminal navigation tests (Phase 3)
│   ├── VisualApiIntegrationTests.cs           # NEW: End-to-end VISUAL API calls (read-only validation) - Phase 5
│   ├── LocalTransactionPersistenceTests.cs    # NEW: MAMP MySQL transaction CRUD tests - Phase 5
│   └── CacheIntegrationTests.cs               # NEW: VISUAL cache operations tests - Phase 5
├── contract/
│   └── VisualApiContractTests.cs              # NEW: VISUAL API Toolkit endpoint contract validation - Phase 5
└── performance/
    ├── CustomControlPerformanceTests.cs       # NEW: Control rendering performance (Phase 1)
    ├── SettingsLoadPerformanceTests.cs        # NEW: Settings window <500ms load (Phase 2)
    ├── DebugTerminalLoadPerformanceTests.cs   # NEW: Debug Terminal <500ms load (Phase 3)
    ├── VisualApiPerformanceTests.cs           # NEW: Validate response time budgets (FR-001 through FR-005) - Phase 5
    └── CachePerformanceTests.cs               # NEW: Validate cache hit rate, cached access <1s (FR-006) - Phase 5

docs/
└── UI-CUSTOM-CONTROLS-CATALOG.md              # NEW: Custom controls documentation (Phase 1)

.github/
└── mamp-database/
    ├── schema-tables.json                     # MODIFIED: Add LocalTransactionRecords table
    └── migrations-history.json                # MODIFIED: Add migration entry for Local Transaction Records
```

**Structure Decision**: Avalonia desktop application with MVVM architecture (single project structure). Feature organized in 5 phases with clear dependencies: Phase 1 (Custom Controls Library) is foundation for Phases 2-5, Phase 2 (Settings) and Phase 3 (Debug Terminal) are independent of each other but both use Phase 1 controls, Phase 4 (Configuration Error Dialog) integrates with Phase 2 (Settings), Phase 5 (VISUAL Integration) uses controls from Phase 1 for performance panel. All new components added to existing `MTM_Template_Application/` library following established patterns (Services, ViewModels, Views, Controls). Tests organized by type (unit, integration, contract, performance) to support TDD workflow and CI/CD validation.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

**No violations** - All constitutional principles satisfied. Feature integrates cleanly with existing architecture and established patterns.

---

## Phase 1: Design (COMPLETE ✅)

**Goal**: Define data model, developer setup guide, and API contract tests to establish implementation foundation.

**Date Completed**: 2025-10-08

**Deliverables**:

### 1. data-model.md ✅
- **Location**: `specs/005-migrate-infor-visual/data-model.md`
- **Content**:
  - 6 entity definitions: Part, WorkOrder, InventoryTransaction, CustomerOrder/Shipment, LocalTransactionRecord, VisualPerformanceMetrics
  - Complete attribute tables with types, constraints, descriptions
  - Validation rules for each entity
  - Cache behavior (TTL: Parts 24h, Others 7d, LZ4 compression 3:1 target)
  - ERD showing read-only VISUAL entities vs. local MAMP MySQL writes
  - LocalTransactionRecord MySQL schema (AUTO_INCREMENT PK, JSON entity_data column)
- **Lines**: 538 lines total
- **Key Decisions**:
  - **READ-ONLY VISUAL Architecture**: All VISUAL entities (Part, WorkOrder, InventoryTransaction, CustomerOrder) fetched via API Toolkit but never written back
  - **LocalTransactionRecord as System of Record**: All user-entered transactions (work order status updates, inventory receipts/issues, shipment confirmations) persist exclusively to MAMP MySQL with no VISUAL sync
  - **VisualReferenceData JSON field**: Stores cached VISUAL state at transaction time for validation warnings (FR-013)
  - **EntityData JSON structure varies by TransactionType**: WorkOrderStatusUpdate, InventoryReceipt, InventoryIssue, ShipmentConfirmation have distinct schemas

### 2. quickstart.md ✅
- **Location**: `specs/005-migrate-infor-visual/quickstart.md`
- **Content**:
  - 9 setup steps: Prerequisites, VISUAL API Toolkit installation, MAMP MySQL configuration, credential setup, clone/build, run application, run tests, development workflow, troubleshooting
  - Prerequisite table (Visual Studio 2022, .NET 9.0 SDK, MAMP, Git, VISUAL API Toolkit)
  - System requirements (Windows 10/11 x64, 16GB RAM, x86 architecture for Toolkit)
  - MAMP MySQL database creation with migration script execution
  - VISUAL credentials storage via WindowsSecretsService pattern
  - Build/run commands (dotnet restore, dotnet build, dotnet run)
  - VISUAL API Toolkit reference setup (manual .csproj edit vs. Visual Studio)
  - Test execution commands (contract tests, performance tests, unit tests)
  - Troubleshooting section (common errors with solutions)
  - Quick reference commands and key file locations
- **Lines**: 538 lines total
- **Key Sections**:
  - **Step 1: VISUAL API Toolkit Installation** - Critical Windows-only dependency (x86 .NET 4.x), requires admin rights, COM registration
  - **Step 3: Configure Credentials** - WindowsSecretsService usage pattern (already implemented in feature 002), credential verification commands
  - **Step 5: Run Application** - Expected boot sequence (Stage 0 → Stage 1 → Stage 2), Debug Terminal VISUAL Performance tab verification
  - **Step 8: Troubleshooting** - 4 common issues with symptoms and solutions (Toolkit not found, MySQL connection failed, credentials not found, performance test timeouts)

### 3. contracts/CONTRACT-TESTS.md ✅
- **Location**: `specs/005-migrate-infor-visual/contracts/CONTRACT-TESTS.md`
- **Content**:
  - 7 contract test categories: Authentication, Part Lookup, Work Order Query, Inventory Transaction, Customer Order/Shipment, Error Handling, Performance
  - 16 detailed contract tests (CT-AUTH-001 through CT-PERF-003)
  - Request/response examples for each VISUAL API endpoint
  - Performance targets validated: <3s parts, <2s work orders, <5s inventory
  - Circuit breaker behavior: 5 consecutive failures → 30s degradation mode
  - Test implementation plan: Phase 1 (7 core tests), Phase 2 (6 advanced), Phase 3 (3 performance)
- **Lines**: 621 lines total
- **Key Contract Tests**:
  - **CT-AUTH-001/002**: Windows Integrated vs. Username/Password authentication patterns
  - **CT-PART-001**: Get Part by ID with full response structure including specifications dictionary
  - **CT-WO-001**: Get Work Order by ID with nested MaterialRequirement and OperationStep structures
  - **CT-ERROR-003**: Circuit breaker opens after 5 consecutive failures, stays open 30s, automatically retries (FR-020 compliance)
  - **CT-PERF-001/002/003**: P50/P95/P99 response time validation under load

**Phase 1 Gate Check**: ✅ PASS
- ✅ Data model defines all entities from spec.md (5 VISUAL entities + LocalTransactionRecord + VisualPerformanceMetrics)
- ✅ ERD shows read-only architecture (VISUAL → Cache → Display; User Input → MAMP MySQL only)
- ✅ LocalTransactionRecord MySQL schema documented with AUTO_INCREMENT PK, JSON entity_data, indexes
- ✅ Quickstart guide enables new developer setup (estimated <2 hours from zero to running application)
- ✅ Contract tests validate VISUAL API Toolkit behavior matches functional requirements (16 tests covering authentication, queries, error handling, performance)
- ✅ Cache strategy documented (Parts 24h TTL, Others 7d TTL, LZ4 3:1 compression, ~40MB budget)
- ✅ Performance targets specified (<3s parts, <2s work orders, <5s inventory, <1s cached)
- ✅ Circuit breaker behavior specified (5 failures → 30s timeout → auto-retry)

**Ready for Phase 2 (Task Breakdown)**: ✅ YES

**Next Step**: Execute `/speckit.tasks` command to generate tasks.md from spec.md user stories and plan.md Phase 1 deliverables
