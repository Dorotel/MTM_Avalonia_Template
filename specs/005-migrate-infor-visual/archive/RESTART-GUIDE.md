# Feature 005 Restart Guide

**Date**: October 8, 2025

**Purpose**: Instructions for restarting Feature 005 specification development

---

## Quick Start

### Step 1: Review Reference Files

Read the reference files in this order:

1. **`reference/README.md`** - Index and usage guide (5 minutes)
2. **`reference/REFERENCE-CLARIFICATIONS.md`** - User's answers to 21 questions (5 minutes)
3. Skim remaining reference files based on need (see reference/README.md for guidance)

### Step 2: Run Spec-Kit Command

Use the restart prompt with the `/speckit.specify` command:

```
/speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md
```

### Step 3: Generate Specification

The spec-kit tool will:

- Read `RESTART-PROMPT.md` for feature description
- Consult all 6 reference files for context
- Generate `SPEC_005.md` with complete functional requirements
- Generate `PLAN_005.md` with technical architecture
- Generate `TASKS_005.md` with task breakdown

---

## What's Been Prepared

### Prompt File: RESTART-PROMPT.md

**Purpose**: Feature description formatted for `/speckit.specify` command

**Contents**:

- Feature overview (Settings UI + Custom Controls + Debug Terminal + VISUAL)
- 5-phase implementation strategy
- Success criteria and deliverables
- References to 6 categorized reference files

**Format**: Markdown document structured for spec-kit consumption

### Reference Files (6 files in `reference/` directory)

| File                                      | Purpose                                  | Lines | Read Time |
| ----------------------------------------- | ---------------------------------------- | ----- | --------- |
| README.md                                 | Index and usage guide                    | ~300  | 5 min     |
| REFERENCE-CLARIFICATIONS.md               | User's Q&A (21 questions)                | ~200  | 5 min     |
| REFERENCE-EXISTING-PATTERNS.md            | Codebase patterns to follow              | ~400  | 10 min    |
| REFERENCE-CUSTOM-CONTROLS.md              | Controls to extract/create (10)          | ~450  | 12 min    |
| REFERENCE-SETTINGS-INVENTORY.md           | All settings requiring UI (60+)          | ~400  | 15 min    |
| REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md  | Constitution compliance checklist        | ~550  | 20 min    |
| REFERENCE-VISUAL-API-SCOPE.md             | Visual ERP integration scope             | ~500  | 18 min    |

**Total Context**: ~2500 lines, ~80 minutes total read time

---

## Scope Summary

### What's Included (All-in-One Mega-Feature)

#### Phase 1: Custom Controls Extraction (Foundation)

- Extract 5 controls from DebugTerminalWindow.axaml
- Create 5 new controls for Settings/Debug Terminal
- Document all 10 controls in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
- 80%+ test coverage for all controls

#### Phase 2: Settings Screen UI

- Side panel navigation with 8 categories
- 60+ settings with real-time validation
- Export/import settings (JSON format)
- Integration with IConfigurationService (Feature 002)
- User preference persistence (MySQL + offline cache)

#### Phase 3: Debug Terminal Modernization

- Complete rewrite (not refactor)
- SplitView navigation (left panel menu, right panel content)
- 5 content sections (Performance, Boot Timeline, Errors, Configuration, Environment)
- Use custom controls from Phase 1
- Performance budget: <500ms load

#### Phase 4: Configuration Error Dialog

- Modal dialog for critical configuration errors
- Recovery options presentation
- Integration with MainWindow.axaml (Feature 002 TODO)

#### Phase 5: Visual ERP Integration

- Read-only API client for items, work orders, inventory
- Barcode scanning integration
- Offline-first operation with LZ4 cache
- Sync queue for offline transactions
- Mock service for testing

### What's NOT Included (Future Features)

- Real-time sync with WebSockets
- Push notifications
- Advanced barcode scanning (batch scan)
- Inventory cycle counting workflows

---

## Risk Assessment

### HIGH RISK Items

1. **All-in-one approach**: 3-5x larger than split approach
   - **Risk**: Longer development time, harder to review
   - **Mitigation**: Radio Silence Mode with PATCH output, frequent commits

2. **Debug Terminal complete rewrite**: Not refactor
   - **Risk**: Breaking existing functionality
   - **Mitigation**: Comprehensive tests before/after, feature flag for new UI

3. **Visual ERP integration**: External dependency
   - **Risk**: API availability, authentication issues
   - **Mitigation**: Mock service for testing, offline mode by default

4. **Custom controls extraction**: 10 controls at once
   - **Risk**: Breaking existing UIs that use patterns
   - **Mitigation**: Extract one at a time, test after each extraction

### Success Indicators

- Build completes with zero errors/warnings
- All tests passing (80%+ coverage)
- Performance budgets met
- Constitutional audit passes (100%)
- Validation script passes (100% task completion)

---

## Constitutional Compliance

### TODOs to Complete

**From Constitution.md (lines 34-37)**:

- [ ] Create `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
- [ ] Document manufacturing field controls pattern
- [ ] Add custom control examples to quickstart guide

**From Feature 003 (Debug Terminal)**:

- [ ] Implement `CopyToClipboardCommand` with actual clipboard integration
- [ ] Implement `IsMonitoring` toggle with performance snapshot collection
- [ ] Implement environment variables display with filtering

**From Feature 002 (Configuration)**:

- [ ] Implement `ConfigurationErrorDialog` modal in MainWindow.axaml

### Principles to Validate

- Principle I: Spec-Driven Development ✅ (using spec-kit)
- Principle II: Nullable Reference Types ✅
- Principle III: CompiledBinding Everywhere ✅
- Principle IV: Test-Driven Development ✅ (80%+ coverage)
- Principle V: Performance Budgets ✅ (defined in references)
- Principle VI: Error Categorization ✅
- Principle VII: Secrets Never Touch Code ✅
- Principle VIII: Graceful Degradation ✅ (offline mode)
- Principle IX: Structured Logging ✅
- Principle X: Dependency Injection ✅
- Principle XI: Reusable Custom Controls ✅ (10 controls)

---

## Development Workflow

### 1. Specification Phase (Current)

```bash
# Generate specification
/speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md

# Review generated spec
code specs/005-migrate-infor-visual/SPEC_005.md

# Generate plan
/speckit.plan for feature: 005-migrate-infor-visual

# Review generated plan
code specs/005-migrate-infor-visual/PLAN_005.md

# Generate tasks
/speckit.tasks for feature: 005-migrate-infor-visual

# Review generated tasks
code specs/005-migrate-infor-visual/TASKS_005.md
```

### 2. Implementation Phase (Radio Silence Mode)

**Agent Workflow**:

1. Clarify unknowns (single concise question set)
2. Post short plan and expected outputs (with timebox)
3. Await user approval to proceed
4. **ENTER RADIO SILENCE**
5. Output only deliverables (PATCH/NEW FILE/DELETE FILE/TEST/COMMIT)
6. Enforce repository standards at all times
7. **EXIT RADIO SILENCE**: Post SUMMARY, CHANGES, TESTS, NEXT

**No Commentary During Silence**:

- ❌ Status updates
- ❌ Thoughts or reasoning
- ❌ Questions (except critical ambiguities)
- ✅ PATCH files only
- ✅ TEST results only
- ✅ COMMIT messages only

### 3. Validation Phase

```powershell
# Build solution (zero errors/warnings)
dotnet build MTM_Template_Application.sln

# Run all tests (all passing)
dotnet test MTM_Template_Application.sln

# Validate implementation (100% task completion)
.\.specify\scripts\powershell\validate-implementation.ps1

# Constitutional audit (100% compliance)
.\.specify\scripts\powershell\constitutional-audit.ps1
```

---

## Performance Budgets

| Component                    | Budget     | Validation Method                    |
| ---------------------------- | ---------- | ------------------------------------ |
| Settings screen load         | <500ms     | Performance test                     |
| Settings save                | <1s        | Performance test                     |
| Visual API call              | <2s        | Integration test                     |
| Offline mode activation      | <100ms     | Performance test                     |
| Custom control render        | <16ms      | Visual profiler                      |
| Configuration retrieval      | <100ms     | Unit test                            |
| Database query               | <500ms     | Integration test                     |

---

## File Structure After Implementation

```
MTM_Template_Application/
├── Controls/                          # NEW: Custom controls
│   ├── Cards/
│   │   └── StatusCard.axaml
│   ├── Metrics/
│   │   ├── MetricDisplay.axaml
│   │   ├── BootTimelineChart.axaml
│   │   └── ConnectionHealthBadge.axaml
│   ├── Settings/
│   │   ├── SettingsCategory.axaml
│   │   └── SettingRow.axaml
│   ├── Navigation/
│   │   └── NavigationMenuItem.axaml
│   ├── Dialogs/
│   │   └── ConfigurationErrorDialog.axaml
│   ├── Lists/
│   │   └── ErrorListPanel.axaml
│   └── Buttons/
│       ├── ActionButton.axaml
│       └── ActionButtonGroup.axaml
├── ViewModels/
│   ├── Settings/                      # NEW: Settings ViewModels
│   │   ├── SettingsViewModel.cs
│   │   ├── VisualSettingsViewModel.cs
│   │   └── ...
│   └── DebugTerminalViewModel.cs      # REFACTORED
├── Views/
│   ├── Settings/                      # NEW: Settings views
│   │   └── SettingsWindow.axaml
│   ├── DebugTerminalWindow.axaml      # REFACTORED
│   └── MainWindow.axaml               # UPDATED (ConfigurationErrorDialog)
└── Services/
    └── DataLayer/
        └── Visual/                    # NEW: Visual ERP integration
            ├── IVisualApiClient.cs
            ├── VisualApiClient.cs
            ├── Mock/
            │   └── MockVisualApiClient.cs
            └── Models/
                ├── VisualItem.cs
                ├── VisualWorkOrder.cs
                └── VisualInventoryTransaction.cs

docs/
└── UI-CUSTOM-CONTROLS-CATALOG.md      # NEW: Control documentation

tests/
├── unit/
│   ├── Controls/                      # NEW: Control tests
│   ├── ViewModels/Settings/           # NEW: Settings VM tests
│   └── Services/Visual/               # NEW: Visual service tests
├── integration/
│   ├── VisualApiIntegrationTests.cs   # NEW
│   └── PerformanceTests.cs            # UPDATED
└── contract/
    └── VisualApiContractTests.cs      # NEW
```

---

## Next Steps

1. **Review this guide** (~5 minutes)
2. **Read reference files** (~80 minutes total, or skim as needed)
3. **Run spec-kit command** to generate specification
4. **Review generated spec, plan, and tasks**
5. **Approve for implementation** (enter Radio Silence Mode)

---

## Questions to Resolve Before Implementation

### Before entering Radio Silence Mode, clarify:

1. **Visual API Endpoint**: Do you have actual Visual ERP instance URL, or start with mock service?
2. **Database Credentials**: Should Settings UI allow editing database credentials, or read-only?
3. **Feature Flag Strategy**: Should Settings UI allow toggling feature flags, or read-only?
4. **Debug Terminal Access**: Should Debug Terminal be accessible from main menu, or hidden by default?
5. **Settings Import/Export**: Should export include credentials (filtered), or exclude entirely?

---

## Support Resources

- **Constitution**: `.specify/memory/constitution.md` (v1.1.0)
- **Spec-Kit Guides**: `docs/Specify Guides/`
- **Existing Specs**: `specs/001-boot-sequence-splash/`, `specs/002-environment-and-configuration/`, `specs/003-debug-terminal-modernization/`
- **AGENTS.md**: Root-level agent guide
- **Copilot Instructions**: `.github/copilot-instructions.md`

---

## Contact

**User**: John Koll (GitHub: @Dorotel)

**Date Created**: October 8, 2025

**Feature Version**: 005 (Restart)
