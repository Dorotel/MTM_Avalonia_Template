# Feature 005 Update Summary

**Date**: October 9, 2025
**Purpose**: Document the transformation from VISUAL-only to all-in-one mega-feature

---

## Overview

Transforming Feature 005 from single-focus (VISUAL ERP integration) to comprehensive all-in-one mega-feature covering:

1. **Phase 1**: Custom Controls Extraction (Foundation)
2. **Phase 2**: Settings Screen UI
3. **Phase 3**: Debug Terminal Modernization
4. **Phase 4**: Configuration Error Dialog
5. **Phase 5**: Visual ERP Integration (existing content)

---

## File-by-File Changes

### 1. spec.md

**Current State**: 5 user stories all focused on VISUAL ERP operations (Part Lookup, Work Orders, Inventory, Shipments, Offline Mode)

**Required Changes**:
- **Header**: Update title to "Manufacturing Application Modernization - All-in-One Mega-Feature"
- **Input**: Expand to describe all 5 phases
- **User Stories**: ADD 4 new user stories BEFORE existing VISUAL stories:
  - **US1 (P1)**: Custom Controls Library Creation
  - **US2 (P1)**: Settings Management UI
  - **US3 (P2)**: Debug Terminal Modernization
  - **US4 (P2)**: Configuration Error Handling
  - Existing VISUAL stories become US5-US9
- **Requirements**: ADD FR-### for Custom Controls, Settings, Debug Terminal
- **Success Criteria**: ADD SC-### for all phases

**Template Compliance**: Maintains spec-template.md structure with prioritized user stories

---

### 2. plan.md

**Current State**: Technical context only covers VISUAL integration (API Toolkit, MySQL writes, offline cache)

**Required Changes**:
- **Summary**: Expand to describe all 5 phases
- **Technical Context**: ADD dependencies:
  - Avalonia custom control patterns (StyledProperty)
  - IConfigurationService integration (Feature 002)
  - DebugTerminalWindow refactor patterns
- **Constitution Check**: ADD checks for:
  - Principle XI (Reusable Custom Controls) - CRITICAL
  - Custom control catalog requirement
  - Settings persistence patterns
- **Project Structure**: ADD directories:
  - `MTM_Template_Application/Controls/` (Cards, Metrics, Settings, Navigation, Dialogs, Lists, Buttons)
  - `MTM_Template_Application/ViewModels/Settings/`
  - `MTM_Template_Application/Views/Settings/`
  - `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
  - `tests/unit/Controls/`

**Template Compliance**: Maintains plan-template.md structure with expanded scope

---

### 3. tasks.md

**Current State**: Tasks only for VISUAL integration (Setup, Foundation, US1-US5 for VISUAL operations)

**Required Changes**:
- **Phase 1: Setup**: ADD tasks for custom control project structure
- **Phase 2: Foundation**: ADD tasks for control base classes
- **ADD Phase 3: Custom Controls Extraction** (10 controls):
  - T### Extract StatusCard from DebugTerminalWindow
  - T### Extract MetricDisplay from DebugTerminalWindow
  - T### Create SettingsCategory control
  - T### Create SettingRow control
  - ... (10 controls total per REFERENCE-CUSTOM-CONTROLS.md)
- **ADD Phase 4: Settings UI** (8 categories):
  - T### Create SettingsWindow with TabControl
  - T### Implement General Settings tab
  - T### Implement Database Settings tab
  - ... (8 tabs per REFERENCE-SETTINGS-INVENTORY.md)
- **ADD Phase 5: Debug Terminal Rewrite**:
  - T### Implement SplitView navigation
  - T### Create feature-based menu items
  - T### Wire up ViewModel bindings (Copy, IsMonitoring, Environment Variables)
- **ADD Phase 6: Configuration Error Dialog**:
  - T### Create ConfigurationErrorDialog.axaml
  - T### Integrate with MainWindow error paths
- **Existing VISUAL tasks**: Renumber as Phase 7-11

**Template Compliance**: Maintains tasks-template.md structure with phase-based organization

---

### 4. data-model.md

**Current State**: Entities only for VISUAL (Part, WorkOrder, InventoryTransaction, CustomerOrder, LocalTransactionRecord)

**Required Changes**:
- **ADD Section 1: Custom Control Metadata**:
  - `ControlDefinition` entity (ControlName, Category, Properties, Events, Usage Examples)
- **ADD Section 2: Settings Management**:
  - `SettingDefinition` entity (SettingKey, SettingValue, SettingType, Category, Validation, DefaultValue)
  - `SettingCategory` entity (CategoryName, DisplayName, Icon, SortOrder, Settings collection)
- **ADD Section 3: Debug Terminal Sections**:
  - `DiagnosticSection` entity (SectionName, DataSource, RefreshInterval, DisplayFormat)
- **Existing VISUAL entities**: Keep as Section 4-8

**Data Model ERD**: Add relationships between Settings → UserPreferences, Controls → Documentation

---

### 5. quickstart.md

**Current State**: Setup instructions only for VISUAL API Toolkit and MySQL

**Required Changes**:
- **ADD Step 2**: Custom Control Development Setup
  - Avalonia control development tools
  - XAML previewer configuration
  - StyledProperty pattern examples
- **ADD Step 3**: Settings UI Development
  - IConfigurationService API reference
  - UserPreferences table verification
  - Settings validation patterns
- **ADD Step 4**: Debug Terminal Navigation
  - SplitView pattern examples
  - Feature-based routing setup
- **Existing VISUAL setup**: Becomes Step 5+

**Quickstart Validation**: Update final validation to test all 5 phases

---

### 6. contracts/ (Directory)

**Current State**: CONTRACT-TESTS.md covers VISUAL API Toolkit contracts only

**Required Changes**:
- **ADD Settings Contracts**:
  - CT-SETTINGS-001: Get setting by key
  - CT-SETTINGS-002: Save setting with validation
  - CT-SETTINGS-003: Export/import settings JSON
- **ADD Debug Terminal Contracts**:
  - CT-DEBUG-001: Get performance snapshots
  - CT-DEBUG-002: Get boot timeline
  - CT-DEBUG-003: Get environment variables (filtered)
- **Existing VISUAL contracts**: Keep as-is

---

## Constitutional Compliance

### TODOs Completed by This Feature

From `.specify/memory/constitution.md`:
- ✅ Create `docs/UI-CUSTOM-CONTROLS-CATALOG.md` (Phase 1 deliverable)
- ✅ Document manufacturing field controls pattern (Phase 1 deliverable)
- ✅ Add custom control examples to quickstart guide (Phase 1 deliverable)

From Feature 003 (Debug Terminal):
- ✅ Implement `CopyToClipboardCommand` with actual clipboard integration (Phase 5)
- ✅ Implement `IsMonitoring` toggle with performance snapshot collection (Phase 5)
- ✅ Implement environment variables display with filtering (Phase 5)

From Feature 002 (Configuration):
- ✅ Implement `ConfigurationErrorDialog` modal in MainWindow.axaml (Phase 6)

### Principle Validation

- **Principle I (Spec-Driven)**: ✅ Using spec-kit workflow
- **Principle II (Nullable Types)**: ✅ All C# code
- **Principle III (CompiledBinding)**: ✅ All XAML files
- **Principle IV (TDD)**: ✅ 80%+ coverage target
- **Principle V (Performance Budgets)**: ✅ Defined per phase
- **Principle XI (Reusable Custom Controls)**: ✅ **PRIMARY FOCUS OF FEATURE**

---

## Risk Assessment

### HIGH RISK Items from RESTART-GUIDE.md

1. **All-in-one approach**: 3-5x larger than split approach
   - **Scope**: 10 controls + 60 settings + Debug Terminal rewrite + Config dialog + VISUAL integration
   - **Estimated Tasks**: 150-200 tasks (vs. 60 current)
   - **Mitigation**: Radio Silence Mode with PATCH output, frequent commits

2. **Debug Terminal complete rewrite**: Not refactor
   - **Risk**: Breaking existing functionality
   - **Mitigation**: Feature flag for new UI, comprehensive tests before/after

3. **Custom controls extraction**: 10 controls at once
   - **Risk**: Breaking existing UIs that use patterns
   - **Mitigation**: Extract one at a time, test after each extraction

4. **Settings UI**: 60+ settings with validation
   - **Risk**: Complex state management, validation logic
   - **Mitigation**: Use existing IConfigurationService patterns from Feature 002

---

## Implementation Order

Per RESTART-GUIDE.md clarifications (Q14):

1. **Phase 1: Custom Controls** - Build reusable components first
2. **Phase 2: Settings UI** - Use components from Phase 1
3. **Phase 3: Debug Terminal** - Refactor with components from Phase 1
4. **Phase 4: Config Error Dialog** - Small focused feature
5. **Phase 5: VISUAL Integration** - Largest feature, depends on Settings for config

---

## Success Indicators

From RESTART-GUIDE.md:

- [ ] Build completes with zero errors/warnings
- [ ] All tests passing (80%+ coverage)
- [ ] Performance budgets met (Settings <500ms, VISUAL API <2s, offline <100ms)
- [ ] Constitutional audit passes (100%)
- [ ] Validation script passes (100% task completion)
- [ ] Custom control catalog complete (10 controls documented)
- [ ] Settings UI functional (60+ settings accessible)
- [ ] Debug Terminal rewrite complete (SplitView navigation)
- [ ] Configuration error dialog integrated
- [ ] VISUAL integration working (offline-first)

---

## Next Steps

1. ✅ Review this summary (COMPLETE)
2. ⏳ Update spec.md with all 5 phases (IN PROGRESS)
3. ⏳ Update plan.md with expanded technical context
4. ⏳ Update tasks.md with complete task breakdown
5. ⏳ Update data-model.md with Settings/Controls entities
6. ⏳ Update quickstart.md with all setup instructions
7. ⏳ Update contracts/CONTRACT-TESTS.md with all contracts
8. ⏳ Create docs/UI-CUSTOM-CONTROLS-CATALOG.md template
9. ⏳ Run validation script
10. ⏳ Constitutional audit

---

## References

- **RESTART-GUIDE.md**: Feature 005 restart instructions
- **RESTART-PROMPT.md**: `/speckit.specify` input
- **reference/REFERENCE-CLARIFICATIONS.md**: 21 Q&A answers
- **reference/REFERENCE-CUSTOM-CONTROLS.md**: 10 controls to create
- **reference/REFERENCE-SETTINGS-INVENTORY.md**: 60+ settings catalog
- **reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md**: Compliance checklist
- **reference/REFERENCE-EXISTING-PATTERNS.md**: Codebase patterns
- **reference/REFERENCE-VISUAL-API-SCOPE.md**: VISUAL integration scope

---

**Update Date**: October 9, 2025
**Status**: Planning Complete, Updates In Progress
**Estimated Completion**: After all file updates applied
