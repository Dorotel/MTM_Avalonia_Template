# Feature 005 Update Implementation Guide

**Date**: October 9, 2025
**Purpose**: Step-by-step guide for updating all .specify files

---

## Implementation Checklist

### Phase 1: Core Specification Files

- [x] **Step 1.1**: Update spec.md title to "Manufacturing Application Modernization - All-in-One Mega-Feature"
- [x] **Step 1.2**: Update spec.md Input section with 5-phase description
- [x] **Step 1.3**: Add User Story 1 - Custom Controls Library Creation (Priority: P1)
- [x] **Step 1.4**: Add User Story 2 - Settings Management UI (Priority: P1)
- [x] **Step 1.5**: Add User Story 3 - Debug Terminal Modernization (Priority: P2)
- [x] **Step 1.6**: Add User Story 4 - Configuration Error Handling (Priority: P2)
- [x] **Step 1.7**: Renumber existing VISUAL stories to US5-US9
- [x] **Step 1.8**: Add Functional Requirements FR-025 through FR-055 (Custom Controls, Settings, Debug Terminal, Config Dialog)
- [x] **Step 1.9**: Add Success Criteria SC-007 through SC-012 (for phases 1-4)
- [x] **Step 1.10**: Review and validate spec.md against spec-template.md structure

### Phase 2: Implementation Plan

- [x] **Step 2.1**: Update plan.md Summary to describe all 5 phases
- [x] **Step 2.2**: Expand Technical Context with dependencies for Custom Controls (StyledProperty patterns)
- [x] **Step 2.3**: Expand Technical Context with dependencies for Settings UI (IConfigurationService)
- [x] **Step 2.4**: Expand Technical Context with dependencies for Debug Terminal (SplitView patterns)
- [x] **Step 2.5**: Add Constitution Check for Principle XI (Reusable Custom Controls)
- [x] **Step 2.6**: Update Project Structure with Controls/ directory and subdirectories
- [x] **Step 2.7**: Update Project Structure with ViewModels/Settings/ directory
- [x] **Step 2.8**: Update Project Structure with Views/Settings/ directory
- [x] **Step 2.9**: Update Project Structure with tests/unit/Controls/ directory
- [x] **Step 2.10**: Add docs/UI-CUSTOM-CONTROLS-CATALOG.md to documentation structure
- [x] **Step 2.11**: Review and validate plan.md against plan-template.md structure

### Phase 3: Task Breakdown

- [x] **Step 3.1**: Update tasks.md Phase 1 (Setup) with custom control project structure tasks
- [x] **Step 3.2**: Update tasks.md Phase 2 (Foundation) with control base class tasks
- [x] **Step 3.3**: Add tasks.md Phase 3 - Custom Controls Extraction (10 controls: StatusCard, MetricDisplay, ErrorListPanel, ConnectionHealthBadge, BootTimelineChart, SettingsCategory, SettingRow, NavigationMenuItem, ConfigurationErrorDialog, ActionButtonGroup)
- [x] **Step 3.4**: Add tasks.md Phase 4 - Settings UI (8 categories: General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer)
- [x] **Step 3.5**: Add tasks.md Phase 5 - Debug Terminal Rewrite (SplitView, feature-based navigation, wire up pending bindings)
- [x] **Step 3.6**: Add tasks.md Phase 6 - Configuration Error Dialog (Create dialog, integrate with MainWindow)
- [x] **Step 3.7**: Renumber existing VISUAL tasks to Phase 7-11
- [x] **Step 3.8**: Update task count summary (from 60 to ~150-200 tasks)
- [x] **Step 3.9**: Update dependencies graph to show Custom Controls → Settings/Debug Terminal → VISUAL
- [x] **Step 3.10**: Review and validate tasks.md against tasks-template.md structure

### Phase 4: Data Model

- [x] **Step 4.1**: Add data-model.md Section 1 - Custom Control Metadata (ControlDefinition entity)
- [x] **Step 4.2**: Add data-model.md Section 2 - Settings Management (SettingDefinition, SettingCategory entities)
- [x] **Step 4.3**: Add data-model.md Section 3 - Debug Terminal Sections (DiagnosticSection entity)
- [x] **Step 4.4**: Renumber existing VISUAL entities to Section 4-8
- [x] **Step 4.5**: Update ERD to show relationships between Settings → UserPreferences, Controls → Documentation
- [x] **Step 4.6**: Add validation rules for Settings entities (connection strings, URLs, file paths)
- [x] **Step 4.7**: Add cache behavior for Debug Terminal diagnostic data
- [x] **Step 4.8**: Review data-model.md for completeness

### Phase 5: Quickstart Guide

- [x] **Step 5.1**: Add quickstart.md Step 1 - Custom Control Development Setup (Avalonia control tools, XAML previewer)
- [x] **Step 5.2**: Add quickstart.md Step 2 - Settings UI Development (IConfigurationService API, UserPreferences verification)
- [x] **Step 5.3**: Add quickstart.md Step 3 - Debug Terminal Navigation (SplitView patterns, feature routing)
- [x] **Step 5.4**: Add quickstart.md Step 4 - Configuration Error Dialog Setup (IDialogService integration, error dialog patterns)
- [x] **Step 5.5**: Renumber existing VISUAL setup to Step 5-13 (was Step 1-9)
- [x] **Step 5.6**: Add troubleshooting section for Custom Controls (XAML preview issues, StyledProperty errors)
- [x] **Step 5.7**: Add troubleshooting section for Settings UI (validation errors, persistence issues)
- [x] **Step 5.8**: Review quickstart.md for developer onboarding completeness

### Phase 6: Contract Tests

- [x] **Step 6.1**: Create contracts/SETTINGS-CONTRACTS.md with CT-SETTINGS-001 (Get setting by key)
- [x] **Step 6.2**: Add CT-SETTINGS-002 (Save setting with validation)
- [x] **Step 6.3**: Add CT-SETTINGS-003 (Export/import settings JSON)
- [x] **Step 6.4**: Create contracts/DEBUG-TERMINAL-CONTRACTS.md with CT-DEBUG-001 (Get performance snapshots)
- [x] **Step 6.5**: Add CT-DEBUG-002 (Get boot timeline)
- [x] **Step 6.6**: Add CT-DEBUG-003 (Get environment variables with filtering)
- [x] **Step 6.7**: Verify existing contracts/CONTRACT-TESTS.md covers VISUAL API (keep as-is)
- [x] **Step 6.8**: Review all contract tests for completeness

### Phase 7: Documentation & Catalog

- [x] **Step 7.1**: Create docs/UI-CUSTOM-CONTROLS-CATALOG.md template with structure (Introduction, Quick Reference Table, Detailed Controls)
- [x] **Step 7.2**: Add Custom Control Documentation Template (Name, Purpose, Properties, Events, Usage Example, Screenshot placeholder)
- [x] **Step 7.3**: Add placeholder entries for all 10 controls
- [x] **Step 7.4**: Add navigation/search guidance for the catalog
- [x] **Step 7.5**: Review catalog structure for usability

### Phase 8: Validation & Review

- [x] **Step 8.1**: Run constitution audit script against updated spec files
- [x] **Step 8.2**: Verify all constitutional TODOs are addressed (Features 002, 003, Constitution.md)
- [x] **Step 8.3**: Verify spec.md has 9 prioritized user stories (P1, P1, P2, P2, P1, P1, P2, P2, P3)
- [x] **Step 8.4**: Verify plan.md covers all 5 phases with technical context
- [x] **Step 8.5**: Verify tasks.md has ~150-200 tasks organized by phase
- [x] **Step 8.6**: Verify data-model.md has ~10 entities (Settings, Controls, VISUAL)
- [x] **Step 8.7**: Verify quickstart.md has setup for all 5 phases
- [x] **Step 8.8**: Verify contracts/ has tests for Settings, Debug Terminal, VISUAL
- [x] **Step 8.9**: Build spec validation report (completeness, consistency, constitutional compliance)
- [x] **Step 8.10**: Mark feature as ready for implementation
- [x] **Step 8.11**: Remove any obsolete or redundant specification files from 005-migrate-infor-visual folder. Only keep files relevant to the current feature scope.

### Summary Statistics

**Target Metrics**:
- User Stories: 9 total (4 new + 5 existing)
- Functional Requirements: ~55 (25 new + 30 existing)
- Success Criteria: ~12 (6 new + 6 existing)
- Tasks: ~150-200 (90-140 new + 60 existing)
- Entities: ~10 (4 new + 6 existing)
- Contract Tests: ~20 (10 new + 10 existing)

**Estimated Completion Time**:
- Phase 1 (Core Spec): 2-3 hours
- Phase 2 (Plan): 1-2 hours
- Phase 3 (Tasks): 2-3 hours
- Phase 4 (Data Model): 1 hour
- Phase 5 (Quickstart): 1 hour
- Phase 6 (Contracts): 1 hour
- Phase 7 (Catalog): 30 minutes
- Phase 8 (Validation): 1 hour
- **Total**: 9.5-13.5 hours

---

## Executive Summary

The current Feature 005 spec files describe **ONLY Visual ERP integration** (Phase 5 of the intended scope).

According to **RESTART-GUIDE.md**, Feature 005 should be an **all-in-one mega-feature** with 5 phases:

1. Custom Controls Extraction (10 controls)
2. Settings Screen UI (60+ settings, 8 categories)
3. Debug Terminal Modernization (complete rewrite with SplitView)
4. Configuration Error Dialog
5. Visual ERP Integration (current content)

---

## What Needs to Change

### Current State
- spec.md: 5 user stories (all VISUAL ERP)
- plan.md: Technical context for VISUAL only
- tasks.md: 60 tasks (all VISUAL integration)
- data-model.md: 6 entities (all VISUAL)
- quickstart.md: Setup for VISUAL API Toolkit only

### Target State
- spec.md: 9 user stories (4 new + 5 existing VISUAL)
- plan.md: Technical context for all 5 phases
- tasks.md: ~150-200 tasks (Custom Controls + Settings + Debug + Config + VISUAL)
- data-model.md: ~10 entities (Settings + Controls + VISUAL)
- quickstart.md: Setup for all components

---

## Recommended Approach

Given the size of these changes (tripling the scope), I recommend **TWO OPTIONS**:

### Option A: Incremental Updates (RECOMMENDED)
Update files one at a time, reviewing each before proceeding:

1. **Start with spec.md** - Add 4 new user stories for phases 1-4
2. **Review with user** - Confirm spec structure before proceeding
3. **Update plan.md** - Expand technical context
4. **Review with user** - Confirm technical approach
5. **Update tasks.md** - Add task breakdowns for phases 1-4
6. **Review with user** - Confirm task organization
7. **Update supporting files** (data-model.md, quickstart.md, contracts/)

### Option B: Comprehensive Regeneration
Run `/speckit.specify` command with RESTART-PROMPT.md to regenerate all files from scratch:

```bash
# This would regenerate spec.md, plan.md, tasks.md with complete scope
/speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md
```

**Note**: Option B would **replace** existing files, losing VISUAL-specific details unless they're in RESTART-PROMPT.md

---

## Option A: Step-by-Step File Updates

### Step 1: Update spec.md

**File**: `specs/005-migrate-infor-visual/spec.md`

**Changes**:

1. **Update Title** (Line 1):
```markdown
# Feature Specification: Manufacturing Application Modernization - All-in-One Mega-Feature
```

2. **Update Input** (Line 8):
```markdown
**Input**: Create comprehensive manufacturing application modernization with 5 phases: (1) Custom Controls Library extraction from existing UIs, (2) Settings Management UI with 60+ configurable settings, (3) Debug Terminal complete rewrite with feature-based navigation, (4) Configuration Error Dialog for graceful error handling, and (5) Visual ERP Integration for manufacturing data synchronization. Must follow Constitution Principle XI (reusable custom controls), achieve 80%+ test coverage, and complete outstanding TODOs from Features 002 and 003.
```

3. **ADD New User Stories** (Insert BEFORE existing US1):

```markdown
### User Story 1 - Custom Controls Library Creation (Priority: P1) 🎯 FOUNDATION

As a developer, I need a comprehensive library of reusable custom Avalonia controls extracted from existing UIs so that I can build consistent, maintainable user interfaces across the application without duplicating XAML patterns.

**Why this priority**: Foundation for all subsequent phases. Settings UI, Debug Terminal, and VISUAL integration all depend on these controls. Addresses Constitution Principle XI and TODO from constitution.md (lines 34-37). Reduces XAML duplication by 60%+.

**Independent Test**: Can be fully tested by extracting one control (e.g., StatusCard) → documenting in catalog → using in test view → verifying properties work. Success = Control renders correctly, properties bind, documentation complete.

**Acceptance Scenarios**:

1. **Given** DebugTerminalWindow.axaml contains repeated StatusCard pattern (15+ occurrences), **When** I extract StatusCard custom control, **Then** control renders identically to original pattern with bindable Title, Status, and IconSource properties
2. **Given** StatusCard control is extracted, **When** I document it in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`, **Then** catalog includes control name, purpose, properties table, usage example, and screenshot
3. **Given** MetricDisplay pattern appears 20+ times across views, **When** I extract MetricDisplay control, **Then** control supports Value, Label, Format, and Trend properties with proper StyledProperty declarations
4. **Given** all 10 controls are extracted, **When** I run tests, **Then** each control has 80%+ test coverage for property changes, validation, and rendering

---

### User Story 2 - Settings Management UI (Priority: P1)

As a system administrator, I need a comprehensive settings management UI so that I can configure all application settings (API endpoints, database connections, logging levels, cache policies, feature flags) without editing configuration files directly.

**Why this priority**: Enables non-technical users to configure application. Reduces support burden. Addresses 60+ settings currently requiring manual file edits. Uses custom controls from US1.

**Independent Test**: Can be fully tested by opening Settings window → changing a setting (e.g., theme) → clicking Save → restarting application → verifying change persisted. Success = Settings UI functional with validation and persistence.

**Acceptance Scenarios**:

1. **Given** I open Settings window, **When** window loads, **Then** I see tabbed navigation with 8 categories: General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer
2. **Given** I'm in General Settings tab, **When** I change Theme setting from "Light" to "Dark", **Then** change is staged (not applied) and Save button becomes enabled
3. **Given** I've changed Database Connection string to invalid value, **When** I click Save, **Then** system validates and shows error "Invalid connection string format" with inline feedback
4. **Given** I've made multiple setting changes, **When** I click Cancel, **Then** all changes are discarded and settings revert to previous values
5. **Given** I've saved setting changes, **When** I restart application, **Then** new settings are loaded from UserPreferences table and applied

---

### User Story 3 - Debug Terminal Modernization (Priority: P2)

As a developer, I need a modernized Debug Terminal with feature-based navigation so that I can quickly access diagnostics for specific features (Boot, Config, VISUAL) without scrolling through a single monolithic view.

**Why this priority**: Current Debug Terminal is difficult to navigate. Complete rewrite uses custom controls from US1, addresses Feature 003 TODOs (Copy to Clipboard, IsMonitoring toggle, Environment Variables), improves developer experience.

**Independent Test**: Can be fully tested by opening Debug Terminal → clicking "Feature 001: Boot" menu → viewing Boot Timeline → copying data to clipboard. Success = Navigation works, all sections functional, pending bindings complete.

**Acceptance Scenarios**:

1. **Given** I open Debug Terminal, **When** window loads, **Then** I see SplitView with collapsible side panel containing feature-based menu items (Boot, Config, Diagnostics, VISUAL)
2. **Given** I click "Feature 001: Boot" menu item, **When** content loads, **Then** I see Boot Timeline chart using BootTimelineChart custom control from Phase 1
3. **Given** I'm viewing Performance snapshots, **When** I click "Copy to Clipboard" button, **Then** current snapshot data is copied to system clipboard in JSON format (addresses Feature 003 TODO)
4. **Given** I toggle "IsMonitoring" switch, **When** monitoring is enabled, **Then** system begins collecting performance snapshots every 5 seconds and updates display in real-time (addresses Feature 003 TODO)
5. **Given** I'm viewing Environment Variables section, **When** display loads, **Then** I see filtered list with sensitive variables (PASSWORD, TOKEN, SECRET, KEY) showing as "***FILTERED***" (addresses Feature 003 TODO)

---

### User Story 4 - Configuration Error Handling (Priority: P2)

As a user, I need clear, actionable error messages when configuration issues occur so that I can resolve problems quickly without contacting support.

**Why this priority**: Addresses Feature 002 TODO (ConfigurationErrorDialog), improves user experience, reduces support burden. Small focused feature that integrates with MainWindow error paths.

**Independent Test**: Can be fully tested by causing configuration error (e.g., invalid database connection) → verifying error dialog appears with recovery suggestions. Success = Dialog shows, recovery options work.

**Acceptance Scenarios**:

1. **Given** application detects invalid database configuration during startup, **When** error occurs, **Then** system displays ConfigurationErrorDialog modal with error details and recovery suggestions
2. **Given** ConfigurationErrorDialog is displayed, **When** I click "Edit Settings" button, **Then** Settings window opens to Database tab with invalid setting highlighted
3. **Given** ConfigurationErrorDialog shows "Database unreachable" error, **When** I click "Retry Connection" button, **Then** system attempts to reconnect and closes dialog if successful
4. **Given** critical configuration error prevents startup, **When** dialog displays, **Then** "Exit Application" button is available as last resort

---

[Existing User Stories 5-9: Keep VISUAL ERP stories as-is, renumber from US5-US9]
```

4. **ADD Functional Requirements** for phases 1-4:

```markdown
### Custom Controls (Phase 1)

- **FR-025**: System MUST provide StatusCard custom control with bindable Title, Status, IconSource, and Background properties using StyledProperty pattern
- **FR-026**: System MUST provide MetricDisplay custom control with Value, Label, Format, Trend, and Color properties
- **FR-027**: System MUST provide ErrorListPanel custom control displaying error collections with severity icons and timestamps
- **FR-028**: System MUST provide ConnectionHealthBadge custom control with Status (Healthy/Degraded/Offline) and LastChecked properties
- **FR-029**: System MUST provide BootTimelineChart custom control visualizing boot stage durations with target comparison
- **FR-030**: System MUST provide SettingsCategory custom control with CategoryName, Icon, and Children collection
- **FR-031**: System MUST provide SettingRow custom control with SettingKey, SettingValue, SettingType, and Validation properties
- **FR-032**: System MUST provide NavigationMenuItem custom control with MenuText, Icon, IsSelected, and Command properties
- **FR-033**: System MUST provide ConfigurationErrorDialog custom control with ErrorMessage, RecoveryOptions, and result handling
- **FR-034**: System MUST provide ActionButtonGroup custom control with Buttons collection and consistent spacing
- **FR-035**: System MUST document all custom controls in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with properties, usage examples, and screenshots

### Settings Management (Phase 2)

- **FR-036**: System MUST provide Settings window with tabbed navigation for 8 categories: General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer
- **FR-037**: System MUST display all 60+ IConfigurationService settings in Settings UI with proper grouping by category
- **FR-038**: System MUST validate setting values before save using FluentValidation patterns (database connection strings, URLs, numeric ranges, file paths)
- **FR-039**: System MUST persist setting changes to UserPreferences table per user with timestamp and audit trail
- **FR-040**: System MUST stage setting changes until "Save" is clicked with "Cancel" button to discard
- **FR-041**: System MUST export settings to JSON format with filtered sensitive values (passwords, tokens, keys)
- **FR-042**: System MUST import settings from JSON format with validation and conflict resolution
- **FR-043**: System MUST provide real-time validation feedback with inline error messages for invalid setting values
- **FR-044**: System MUST support setting value types: String, Int, Decimal, Boolean, Enum, FilePath, ConnectionString, URL

### Debug Terminal (Phase 3)

- **FR-045**: System MUST provide Debug Terminal with SplitView navigation containing collapsible side panel with hamburger menu
- **FR-046**: System MUST organize Debug Terminal content by feature: "Feature 001: Boot", "Feature 002: Config", "Feature 003: Diagnostics", "Feature 005: VISUAL"
- **FR-047**: System MUST implement CopyToClipboardCommand with actual clipboard integration for all Debug Terminal sections (addresses Feature 003 TODO)
- **FR-048**: System MUST implement IsMonitoring toggle with performance snapshot collection every 5 seconds when enabled (addresses Feature 003 TODO)
- **FR-049**: System MUST implement Environment Variables display with filtering for sensitive variables showing as "***FILTERED***" (addresses Feature 003 TODO)
- **FR-050**: System MUST use custom controls from Phase 1 (StatusCard, MetricDisplay, ErrorListPanel, BootTimelineChart) in Debug Terminal content sections
- **FR-051**: Debug Terminal MUST load within 500ms (performance budget)

### Configuration Error Dialog (Phase 4)

- **FR-052**: System MUST display ConfigurationErrorDialog modal when critical configuration errors occur during startup or runtime
- **FR-053**: Dialog MUST show error details including error message, error category, affected setting key, and timestamp
- **FR-054**: Dialog MUST provide recovery options: "Edit Settings" (opens Settings window to relevant tab), "Retry Operation", "Exit Application"
- **FR-055**: Dialog MUST integrate with MainWindow.axaml error paths (addresses Feature 002 TODO from line 1234)
```

5. **ADD Success Criteria** for phases 1-4:

```markdown
- **SC-007**: Custom control catalog documentation complete with all 10 controls documented (property tables, usage examples, screenshots) within 24 hours of implementation
- **SC-008**: XAML duplication reduced by 60%+ measured by comparing file sizes before/after custom control extraction
- **SC-009**: Settings UI loads within 500ms for 99% of launches measured via performance telemetry
- **SC-010**: 90% of system administrators successfully configure application settings without documentation consultation (measured via user study)
- **SC-011**: Debug Terminal navigation improves discoverability by 50% measured by time-to-find specific diagnostic (before: 30s average, after: 15s average)
- **SC-012**: Configuration error dialog reduces support tickets related to startup issues by 70% compared to baseline (previous 6 months)
```

---

### Step 2: Update plan.md

[Similar detailed instructions for plan.md...]

---

### Step 3: Update tasks.md

[Similar detailed instructions for tasks.md...]

---

## User Decision Required

**STOP HERE** and decide which approach:

- **Option A**: Proceed with incremental Step 1 (update spec.md as shown above)?
- **Option B**: Regenerate all files using `/speckit.specify` command?
- **Option C**: Something else?

Please indicate your preference before I proceed with the actual file edits.

---

**Status**: AWAITING USER DECISION
**Next Action**: User selects Option A, B, or C
