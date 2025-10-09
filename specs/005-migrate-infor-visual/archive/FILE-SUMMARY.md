# Feature 005 Restart - File Summary

**Date**: October 8, 2025

**Status**: Ready for specification generation

---

## Files Created

### Main Prompt File

**`RESTART-PROMPT.md`** (890 lines)

- Feature description for `/speckit.specify` command
- 5-phase implementation strategy
- Success criteria and deliverables
- References to 6 categorized reference files

**Usage**: Run `/speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md`

---

### Reference Files (6 files in `reference/` directory)

#### 1. `reference/README.md` (~300 lines)

**Purpose**: Index and usage guide for all reference files

**Contents**:

- Overview of all 6 reference files
- When to use each file (phase-specific guidance)
- Quick reference matrix
- File sizes and complexity
- Success criteria

#### 2. `reference/REFERENCE-CLARIFICATIONS.md` (~200 lines)

**Purpose**: Complete record of 21 clarification questions and user's answers

**Contents**:

- Feature scope (all-in-one mega-feature confirmed)
- Debug Terminal approach (complete rewrite)
- Implementation strategy (5 phases)
- Timeline & risk assessment (HIGH RISK items documented)
- Success indicators

#### 3. `reference/REFERENCE-EXISTING-PATTERNS.md` (~400 lines)

**Purpose**: Document established codebase patterns to follow

**Contents**:

- MVVM patterns (CommunityToolkit.Mvvm 8.4.0)
- Avalonia XAML patterns (CompiledBinding examples)
- Current repeated XAML patterns (controls to extract)
- Configuration service patterns (Feature 002)
- Database patterns (MySQL parameterized queries)
- Service registration (DI)
- Error handling patterns
- Testing patterns

#### 4. `reference/REFERENCE-CUSTOM-CONTROLS.md` (~450 lines)

**Purpose**: Catalog of custom controls to extract and create

**Contents**:

- Extraction threshold rule (3+ occurrences)
- 10 custom controls documented:
  - 5 to extract from DebugTerminalWindow
  - 5 new controls to create
- Proposed APIs for each control
- Control library structure
- Testing strategy
- Implementation order

#### 5. `reference/REFERENCE-SETTINGS-INVENTORY.md` (~400 lines)

**Purpose**: Complete catalog of all application settings requiring UI

**Contents**:

- Configuration architecture (3-tier precedence)
- 8 settings categories with 60+ total settings
- Settings screen UI design
- Validation feedback
- Configuration change handling
- Settings export/import (JSON format)
- User preference persistence (MySQL + offline cache)

#### 6. `reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md` (~550 lines)

**Purpose**: Checklist of Constitution principles to validate against

**Contents**:

- All 11 Constitution principles with compliance checklists
- Constitution TODOs (Feature 005 responsibilities)
- Feature 003 TODOs (DebugTerminalViewModel)
- Radio Silence Mode compliance
- Validation checklist summary
- Constitutional violations to avoid
- Post-implementation audit process

#### 7. `reference/REFERENCE-VISUAL-API-SCOPE.md` (~500 lines)

**Purpose**: Define Visual ERP integration requirements and API scope

**Contents**:

- Integration overview (read-only, mobile-first)
- API architecture (base config, client structure)
- 3 data entities with complete contracts:
  - Items (VisualItem)
  - Work Orders (VisualWorkOrder)
  - Inventory Transactions (VisualInventoryTransaction)
- Barcode scanning integration
- Offline mode operation
- Error handling (retry policies)
- Visual API mock service
- Performance requirements
- Testing requirements
- Security & compliance
- Visual integration roadmap (4 phases)

---

### Guide Files

#### `RESTART-GUIDE.md` (~400 lines)

**Purpose**: Instructions for restarting Feature 005 specification development

**Contents**:

- Quick start (3 steps)
- What's been prepared (prompt + reference files)
- Scope summary (what's included/excluded)
- Risk assessment (HIGH RISK items)
- Constitutional compliance (TODOs to complete)
- Development workflow (spec → plan → tasks → implement → validate)
- Performance budgets
- File structure after implementation
- Next steps
- Questions to resolve before implementation

#### `CLARIFICATION-QUESTIONS.html` (~600 lines)

**Purpose**: Interactive HTML form with 21 clarification questions

**Contents**:

- 8 question sections
- Multiple choice with recommendations
- Progress bar
- Copy-to-clipboard functionality
- Styling with professional appearance

**Note**: Already completed by user - answers recorded in REFERENCE-CLARIFICATIONS.md

---

## Total Context Provided

| Category           | Files | Total Lines | Read Time  |
| ------------------ | ----- | ----------- | ---------- |
| Main Prompt        | 1     | ~900        | 10 min     |
| Reference Files    | 6     | ~2500       | 80 min     |
| Guide Files        | 2     | ~1000       | 20 min     |
| **Total**          | **9** | **~4400**   | **110 min**|

---

## Directory Structure

```
specs/005-migrate-infor-visual/
├── RESTART-PROMPT.md              # Main prompt file for /speckit.specify
├── RESTART-GUIDE.md               # Usage instructions
├── CLARIFICATION-QUESTIONS.html   # Interactive Q&A form (completed)
├── FILE-SUMMARY.md                # This file
└── reference/                     # Reference files directory
    ├── README.md                  # Index and usage guide
    ├── REFERENCE-CLARIFICATIONS.md
    ├── REFERENCE-EXISTING-PATTERNS.md
    ├── REFERENCE-CUSTOM-CONTROLS.md
    ├── REFERENCE-SETTINGS-INVENTORY.md
    ├── REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md
    └── REFERENCE-VISUAL-API-SCOPE.md
```

---

## Feature Scope Summary

### All-in-One Mega-Feature (3-5x larger than split approach)

**Phase 1: Custom Controls Extraction (Foundation)**

- Extract 5 controls from DebugTerminalWindow.axaml
- Create 5 new controls for Settings/Debug Terminal
- Document all 10 controls in catalog
- 80%+ test coverage

**Phase 2: Settings Screen UI**

- Side panel navigation (8 categories)
- 60+ settings with validation
- Export/import (JSON)
- Integration with IConfigurationService

**Phase 3: Debug Terminal Modernization**

- Complete rewrite with SplitView navigation
- 5 content sections
- Use custom controls from Phase 1
- <500ms load performance

**Phase 4: Configuration Error Dialog**

- Modal for critical errors
- Recovery options
- Integration with MainWindow

**Phase 5: Visual ERP Integration**

- Read-only API client
- Barcode scanning
- Offline-first with LZ4 cache
- Sync queue for offline transactions

---

## Next Steps for User

### Immediate Actions

1. **Review RESTART-GUIDE.md** (~5 minutes)
   - Understand workflow and risks
   - Review performance budgets
   - Note questions to resolve

2. **Skim reference files** (~20-80 minutes depending on depth)
   - Start with `reference/README.md` for overview
   - Focus on areas relevant to your concerns
   - Use reference matrix to prioritize

3. **Run spec-kit command**:

   ```
   /speckit.specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md
   ```

4. **Review generated outputs**:
   - `SPEC_005.md` - Functional requirements
   - `PLAN_005.md` - Technical architecture
   - `TASKS_005.md` - Task breakdown

5. **Approve for implementation** (enter Radio Silence Mode)

### Questions to Answer Before Implementation

From `RESTART-GUIDE.md`:

1. Visual API Endpoint: Actual URL or mock service?
2. Database Credentials: Editable in Settings UI or read-only?
3. Feature Flag Strategy: Toggleable in Settings UI or read-only?
4. Debug Terminal Access: Main menu or hidden by default?
5. Settings Import/Export: Include credentials (filtered) or exclude?

---

## Success Criteria

Feature 005 restart is successful when:

- [ ] All reference files created and organized
- [ ] Main prompt file formatted correctly
- [ ] Restart guide provides clear instructions
- [ ] User can run `/speckit.specify` command without issues
- [ ] Generated spec, plan, and tasks are comprehensive
- [ ] All constitutional requirements addressed
- [ ] All 10 custom controls documented
- [ ] All 60+ settings documented
- [ ] All 3 Visual API contracts documented

---

## Risk Mitigation

**HIGH RISK Items**:

1. **All-in-one approach** (3-5x larger)
   - Mitigation: Radio Silence Mode with PATCH output

2. **Debug Terminal rewrite** (not refactor)
   - Mitigation: Comprehensive tests, feature flag

3. **Visual ERP integration** (external dependency)
   - Mitigation: Mock service, offline mode

4. **Custom controls extraction** (10 at once)
   - Mitigation: Extract one at a time, test after each

---

## Contact Information

**User**: John Koll (GitHub: @Dorotel)

**Date**: October 8, 2025

**Feature**: 005-migrate-infor-visual (Restart)

**Status**: Ready for specification generation

---

## Additional Resources

- **Constitution**: `.specify/memory/constitution.md` (v1.1.0)
- **Spec-Kit Guides**: `docs/Specify Guides/`
- **Existing Specs**: `specs/001-boot-sequence-splash/`, etc.
- **AGENTS.md**: Root-level agent guide
- **Copilot Instructions**: `.github/copilot-instructions.md`
