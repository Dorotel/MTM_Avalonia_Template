# Spec-Kit Workflow: Feature 005 Implementation Guide

**Date**: October 8, 2025

**Feature**: 005-migrate-infor-visual (All-in-one mega-feature)

**Purpose**: Step-by-step guide for using `.specify` commands to successfully implement Feature 005

---

## Overview: The Spec-Kit Workflow

Spec-Kit follows a **7-step workflow** from specification to implementation:

```
1. /constitution  → Set project principles
2. /specify       → Write feature specification
3. /clarify       → Answer ambiguous questions
4. /plan          → Create technical plan
5. /tasks         → Generate task breakdown
6. /analyze       → Cross-check consistency
7. /implement     → Execute implementation
```

**For Feature 005**: You've already completed preparatory work (clarifications, reference files). Now we'll use the Spec-Kit commands to generate formal specification documents.

---

## Pre-Implementation Checklist

Before starting, ensure:

- [x] Clarification questions answered (21 questions in REFERENCE-CLARIFICATIONS.md)
- [x] Reference files created (6 files in `reference/` directory)
- [x] Prompt file ready (RESTART-PROMPT.md)
- [x] Branch created: `004-infor-visual-api` ✅ (you're already on it)
- [x] Constitution exists (`.specify/memory/constitution.md` v1.1.0)
- [ ] You've reviewed RESTART-GUIDE.md
- [ ] You understand the high-risk items
- [ ] You're ready to commit to Radio Silence Mode implementation

---

## Step 1: Review Constitution (Already Done)

**Command**: `/constitution`

**Purpose**: Review project principles that govern all feature development

**Status**: ✅ **Already completed** - Constitution v1.1.0 exists at `.specify/memory/constitution.md`

**Key Principles for Feature 005**:
- Principle I: Spec-Driven Development (what we're doing now)
- Principle IV: Test-Driven Development (80%+ coverage)
- Principle XI: Reusable Custom Controls (3+ rule)

**Action**: No action needed - constitution already established

---

## Step 2: Generate Feature Specification

**Command**: `/specify`

**Purpose**: Generate `SPEC_005.md` with functional requirements, user stories, and acceptance criteria

### How to Execute

There are **two options** for generating the spec:

#### Option A: Using Your Prepared Prompt File (Recommended)

```
/specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md
```

**This will**:
- Use RESTART-PROMPT.md as the feature description
- Reference all 6 categorized reference files for context
- Generate comprehensive SPEC_005.md based on your clarifications

**Advantages**:
- ✅ All context already prepared (~2500 lines of references)
- ✅ Your 21 Q&A answers incorporated
- ✅ Custom controls catalog included
- ✅ Settings inventory included
- ✅ Constitutional requirements included

#### Option B: Using PowerShell Script Directly

```powershell
# Navigate to .specify/scripts/powershell/
cd .specify/scripts/powershell/

# Run create-new-feature script
./create-new-feature.ps1 "Settings UI with custom controls, Debug Terminal modernization, and Visual ERP integration"
```

**Note**: This creates a NEW feature. Since you want to RESTART Feature 005 with new scope, **Option A is strongly recommended**.

### What Gets Generated

**File**: `specs/005-migrate-infor-visual/SPEC_005.md`

**Contents**:
1. **User Scenarios & Testing** (prioritized user journeys P1-P5)
2. **Functional Requirements** (FR-001 through FR-100+)
3. **Key Entities** (data models)
4. **Performance Requirements** (budgets from REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md)
5. **Security Requirements** (secrets, GDPR compliance)
6. **Edge Cases** (error scenarios, offline mode)

### Post-Generation Actions

1. **Review SPEC_005.md thoroughly** (~30 minutes)
2. **Validate all user stories are independently testable**
3. **Confirm functional requirements match your intent**
4. **Check for any `[NEEDS CLARIFICATION]` markers**
5. **Add/modify requirements as needed**

**Expected Time**: 1-2 hours for generation + review

---

## Step 3: Clarify Ambiguities

**Command**: `/clarify`

**Purpose**: Identify and resolve any ambiguities or missing details in the spec

### How to Execute

After reviewing SPEC_005.md:

```
/clarify
```

**This will**:
- Scan SPEC_005.md for `[NEEDS CLARIFICATION]` markers
- Identify undefined terms or vague requirements
- Generate clarification questions
- Wait for your answers

### What to Clarify

Based on your reference files, likely clarifications:

1. **Visual API Endpoint**: Actual URL or start with mock service for Phase 5?
2. **Database Credentials**: Should Settings UI allow editing, or read-only display?
3. **Feature Flags**: Toggleable in Settings UI, or read-only?
4. **Debug Terminal Access**: Main menu item, or hidden/developer-only?
5. **Settings Export**: Include credentials (filtered), or exclude entirely?
6. **Custom Control Extraction Order**: Extract all 5 at once, or one-by-one?
7. **Testing Strategy**: Write tests before extraction, or after each control?

### Post-Clarification Actions

1. **Answer all clarification questions**
2. **Update SPEC_005.md with clarified details**
3. **Remove all `[NEEDS CLARIFICATION]` markers**
4. **Commit updated spec**: `git commit -am "chore: clarify Feature 005 requirements"`

**Expected Time**: 1-2 hours for clarification + updates

---

## Step 4: Generate Technical Plan

**Command**: `/plan`

**Purpose**: Generate `PLAN_005.md` with technical architecture, implementation approach, and risk mitigation

### How to Execute

```
/plan for feature: 005-migrate-infor-visual
```

**This will**:
- Read SPEC_005.md requirements
- Reference REFERENCE-EXISTING-PATTERNS.md for architecture patterns
- Reference REFERENCE-CUSTOM-CONTROLS.md for control design
- Reference REFERENCE-VISUAL-API-SCOPE.md for API architecture
- Generate comprehensive technical plan

### What Gets Generated

**File**: `specs/005-migrate-infor-visual/PLAN_005.md`

**Contents**:
1. **Architecture Overview** (5-phase approach)
2. **Technology Stack** (Avalonia 11.3.6, MVVM Toolkit 8.4.0, MySQL 5.7)
3. **Implementation Phases**:
   - Phase 1: Custom Controls Extraction
   - Phase 2: Settings Screen UI
   - Phase 3: Debug Terminal Rewrite
   - Phase 4: Configuration Error Dialog
   - Phase 5: Visual ERP Integration
4. **File Structure** (new directories, files to create/modify)
5. **Service Layer Design** (IVisualApiClient, SettingsViewModel)
6. **Data Flow** (configuration persistence, offline sync)
7. **Testing Strategy** (unit, integration, contract tests)
8. **Risk Mitigation** (HIGH RISK items from REFERENCE-CLARIFICATIONS.md)
9. **Performance Considerations** (budgets, optimization strategies)
10. **Security Considerations** (secrets, credentials, GDPR)

### Post-Generation Actions

1. **Review PLAN_005.md thoroughly** (~45 minutes)
2. **Validate architecture aligns with existing patterns**
3. **Confirm all constitutional principles addressed**
4. **Check implementation order (custom controls first!)**
5. **Verify performance budgets are realistic**
6. **Add/modify architecture decisions as needed**

**Expected Time**: 2-3 hours for generation + review

---

## Step 5: Generate Task Breakdown

**Command**: `/tasks`

**Purpose**: Generate `TASKS_005.md` with granular, actionable tasks

### How to Execute

```
/tasks for feature: 005-migrate-infor-visual
```

**This will**:
- Read SPEC_005.md and PLAN_005.md
- Break down each phase into granular tasks
- Assign task IDs, dependencies, and acceptance criteria
- Generate task progress tracking

### What Gets Generated

**File**: `specs/005-migrate-infor-visual/TASKS_005.md`

**Contents**:
1. **Task Overview** (total count, estimated hours)
2. **Phase 1 Tasks** (Custom Controls - 15-20 tasks)
   - Task 001: Extract StatusCard control
   - Task 002: Create StatusCard tests
   - Task 003: Update DebugTerminalWindow to use StatusCard
   - [etc.]
3. **Phase 2 Tasks** (Settings UI - 25-30 tasks)
   - Task 020: Create SettingsViewModel
   - Task 021: Create SettingsWindow XAML
   - Task 022: Implement Visual settings category
   - [etc.]
4. **Phase 3 Tasks** (Debug Terminal - 15-20 tasks)
   - Task 050: Refactor DebugTerminalViewModel
   - Task 051: Create SplitView navigation
   - [etc.]
5. **Phase 4 Tasks** (Error Dialog - 5-10 tasks)
6. **Phase 5 Tasks** (Visual API - 30-40 tasks)
7. **Documentation Tasks** (10-15 tasks)
8. **Testing Tasks** (20-30 tasks)

**Total Expected Tasks**: 120-150 tasks

### Task Format

Each task includes:
- **Task ID**: Unique identifier (TASK-001)
- **Title**: Brief description
- **Description**: Detailed requirements
- **Acceptance Criteria**: Specific conditions for completion
- **Dependencies**: Other tasks that must complete first
- **Estimated Time**: Hours (1h, 2h, 4h, 8h)
- **Status**: Not Started | In Progress | Blocked | Completed
- **Assignee**: (empty for now)

### Post-Generation Actions

1. **Review TASKS_005.md thoroughly** (~1 hour)
2. **Validate task order follows dependencies**
3. **Confirm acceptance criteria are testable**
4. **Check estimated hours are realistic**
5. **Add/split/merge tasks as needed**
6. **Mark any tasks that can be parallelized**

**Expected Time**: 3-4 hours for generation + review + adjustments

---

## Step 6: Analyze Consistency

**Command**: `/analyze`

**Purpose**: Cross-check SPEC, PLAN, and TASKS for consistency and completeness

### How to Execute

```
/analyze feature: 005-migrate-infor-visual
```

**This will**:
- Compare SPEC_005.md requirements to PLAN_005.md implementation
- Verify all functional requirements have corresponding tasks
- Check all user stories have acceptance tests
- Identify gaps, contradictions, or missing coverage
- Generate analysis report

### What Gets Generated

**File**: `specs/005-migrate-infor-visual/ANALYSIS_005.md`

**Contents**:
1. **Completeness Check**
   - All FRs mapped to tasks? ✅/❌
   - All user stories testable? ✅/❌
   - All custom controls documented? ✅/❌
   - All settings covered? ✅/❌
2. **Consistency Check**
   - SPEC vs PLAN alignment
   - PLAN vs TASKS alignment
   - Performance budgets realistic?
3. **Coverage Gaps**
   - Missing test coverage areas
   - Unaddressed edge cases
   - Missing documentation
4. **Risk Assessment**
   - HIGH RISK items mitigation
   - Dependencies on external systems
   - Performance bottlenecks
5. **Recommendations**
   - Split tasks for parallelization
   - Add integration tests
   - Consider feature flags

### Post-Analysis Actions

1. **Review ANALYSIS_005.md** (~30 minutes)
2. **Address all identified gaps**
3. **Update SPEC/PLAN/TASKS as needed**
4. **Re-run `/analyze` to verify fixes**
5. **Commit all updates**: `git commit -am "feat: complete Feature 005 specification"`

**Expected Time**: 1-2 hours for analysis + fixes

---

## Step 7: Implement (Radio Silence Mode)

**Command**: `/implement`

**Purpose**: Execute implementation with Radio Silence Mode protocol

### Prerequisites Before Implementation

Ensure **ALL** of these are true:

- [ ] SPEC_005.md complete with no `[NEEDS CLARIFICATION]`
- [ ] PLAN_005.md reviewed and approved
- [ ] TASKS_005.md granular and actionable
- [ ] ANALYSIS_005.md shows 100% coverage
- [ ] All clarification questions answered
- [ ] Constitutional audit passes
- [ ] Build completes: `dotnet build MTM_Template_Application.sln`
- [ ] Tests pass: `dotnet test MTM_Template_Application.sln`
- [ ] You understand Radio Silence Mode protocol
- [ ] You're ready to commit to 3-5 weeks of implementation

### How to Execute

```
/implement feature: 005-migrate-infor-visual --mode=radio-silence
```

**This will**:
- Enter Radio Silence Mode (autonomous implementation)
- Work through tasks in TASKS_005.md order
- Generate PATCH files for each change
- Run tests after each task
- Update TASKS_005.md progress
- Exit with SUMMARY when complete

### Radio Silence Mode Protocol

**During Silence** (agent behavior):
- ❌ No commentary or status updates
- ❌ No thoughts or reasoning explanations
- ❌ No questions (except critical blockers)
- ✅ PATCH output only (unified diff format)
- ✅ TEST results only (pass/fail + minimal summary)
- ✅ COMMIT messages only (conventional format)

**Agent Output Format**:

```markdown
## PATCH: path/to/file.cs

```diff
--- a/path/to/file.cs
+++ b/path/to/file.cs
@@ -10,7 +10,7 @@
 public class ExampleViewModel : ObservableObject
 {
-    private readonly ILogger _logger;
+    private readonly ILogger<ExampleViewModel> _logger;
 }
```

## TEST: TASK-001
- Unit tests: 15/15 passed ✅
- Integration tests: 3/3 passed ✅
- Coverage: 87% ✅

## COMMIT
feat(controls): extract StatusCard control from DebugTerminalWindow

- Extract repeated Border/StackPanel pattern to StatusCard control
- Add unit tests for StatusCard properties
- Update DebugTerminalWindow to use StatusCard
- Fixes: #005-TASK-001
```

**Allowed Interrupts** (agent may break silence for):
1. **Critical spec ambiguity** (blocks progress)
2. **Missing secrets/config** (cannot proceed)
3. **Version/API conflicts** (breaking change)
4. **Cross-platform constraints** (unsupported API)

### Exit Protocol

When implementation complete (or timebox reached), agent posts:

```markdown
## SUMMARY
Completed Phase 1 (Custom Controls Extraction) and Phase 2 (Settings UI).
Total: 45 tasks completed, 8 hours elapsed.

## CHANGES
- Created 10 custom controls in MTM_Template_Application/Controls/
- Created SettingsWindow and 8 category ViewModels
- Updated DebugTerminalWindow to use new controls
- Added 87 unit tests, 23 integration tests
- Updated documentation: docs/UI-CUSTOM-CONTROLS-CATALOG.md

Files changed: 127 files
Lines added: 8,243
Lines removed: 3,156

## TESTS
- Unit tests: 287/287 passed ✅
- Integration tests: 45/45 passed ✅
- Contract tests: 12/12 passed ✅
- Performance tests: 8/8 passed ✅
- Coverage: 86% (target: 80%) ✅

## NEXT
- Phase 3: Debug Terminal Rewrite (15 tasks remaining)
- Phase 4: Configuration Error Dialog (8 tasks remaining)
- Phase 5: Visual ERP Integration (35 tasks remaining)

Awaiting user review and approval to continue.
```

### Post-Implementation Actions

1. **Review all PATCH files** (ensure quality)
2. **Run validation scripts**:
   ```powershell
   dotnet build MTM_Template_Application.sln
   dotnet test MTM_Template_Application.sln
   .\.specify\scripts\powershell\validate-implementation.ps1
   .\.specify\scripts\powershell\constitutional-audit.ps1
   ```
3. **Test manually** (smoke test key workflows)
4. **Review TASKS_005.md progress** (all completed tasks marked)
5. **Approve continuation** or request changes

**Expected Time**: 3-5 weeks for full implementation (all 5 phases)

---

## Validation & Sign-Off

### Final Validation Checklist

Before merging Feature 005 to main:

- [ ] All tasks in TASKS_005.md marked completed
- [ ] Build succeeds with zero errors/warnings
- [ ] All tests passing (unit, integration, contract, performance)
- [ ] Code coverage ≥80% on critical paths
- [ ] Performance budgets met (all components)
- [ ] Constitutional audit passes (100%)
- [ ] All custom controls documented in catalog
- [ ] All Constitution TODOs completed
- [ ] All Feature 003 TODOs completed
- [ ] User acceptance testing completed
- [ ] Documentation updated (README, user guides)
- [ ] PR reviewed and approved

### Validation Commands

```powershell
# Build
dotnet clean MTM_Template_Application.sln
dotnet build MTM_Template_Application.sln

# Test
dotnet test MTM_Template_Application.sln --logger "console;verbosity=detailed"

# Coverage
dotnet test /p:CollectCoverage=true /p:CoverageOutputFormat=cobertura

# Validate implementation
.\.specify\scripts\powershell\validate-implementation.ps1 -FeatureId "005-migrate-infor-visual" -Strict

# Constitutional audit
.\.specify\scripts\powershell\constitutional-audit.ps1 -FeatureId "005-migrate-infor-visual"

# Update agent context
.\.specify\scripts\powershell\update-agent-context.ps1
```

**Expected Output**:
```
✓ All tasks completed (150/150)
✓ Build succeeded (0 errors, 0 warnings)
✓ Tests passed (367/367)
✓ Coverage: 86% (target: 80%)
✓ Performance budgets met
✓ Constitutional compliance: 100%
✓ Documentation complete
```

---

## Timeline Estimates

### Optimistic (Best Case)

- **Step 2 (Specify)**: 1 hour
- **Step 3 (Clarify)**: 1 hour
- **Step 4 (Plan)**: 2 hours
- **Step 5 (Tasks)**: 3 hours
- **Step 6 (Analyze)**: 1 hour
- **Step 7 (Implement)**: 3 weeks
- **Validation**: 1 day
- **Total**: ~3.5 weeks

### Realistic (Expected)

- **Step 2 (Specify)**: 2 hours
- **Step 3 (Clarify)**: 2 hours
- **Step 4 (Plan)**: 3 hours
- **Step 5 (Tasks)**: 4 hours
- **Step 6 (Analyze)**: 2 hours
- **Step 7 (Implement)**: 4 weeks
- **Validation**: 2 days
- **Total**: ~5 weeks

### Pessimistic (Worst Case)

- **Step 2 (Specify)**: 3 hours
- **Step 3 (Clarify)**: 3 hours
- **Step 4 (Plan)**: 4 hours
- **Step 5 (Tasks)**: 5 hours
- **Step 6 (Analyze)**: 3 hours
- **Step 7 (Implement)**: 5 weeks
- **Validation**: 3 days
- **Total**: ~6 weeks

**Note**: All-in-one approach is 3-5x larger than split approach, hence the extended timeline.

---

## Common Pitfalls to Avoid

### During Specification (Steps 2-3)

- ❌ **Don't skip clarifications** - resolve ALL ambiguities before planning
- ❌ **Don't write vague requirements** - be specific and testable
- ❌ **Don't forget edge cases** - error scenarios, offline mode, limits
- ❌ **Don't ignore constitutional principles** - reference REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md

### During Planning (Step 4)

- ❌ **Don't deviate from existing patterns** - reference REFERENCE-EXISTING-PATTERNS.md
- ❌ **Don't skip risk mitigation** - address HIGH RISK items from clarifications
- ❌ **Don't forget performance budgets** - validate they're achievable
- ❌ **Don't plan extraction after new code** - ALWAYS extract custom controls FIRST

### During Task Breakdown (Step 5)

- ❌ **Don't create mega-tasks** - break down to 1-4 hour chunks
- ❌ **Don't forget dependencies** - mark prerequisites explicitly
- ❌ **Don't skip test tasks** - include test creation in task list
- ❌ **Don't forget documentation tasks** - catalog, guides, examples

### During Analysis (Step 6)

- ❌ **Don't skip gaps** - address ALL identified issues
- ❌ **Don't ignore coverage warnings** - ensure 100% requirement mapping
- ❌ **Don't rush this step** - thorough analysis saves implementation time

### During Implementation (Step 7)

- ❌ **Don't break Radio Silence** (agent) - output PATCH/TEST/COMMIT only
- ❌ **Don't skip tests** - write tests BEFORE implementation (TDD)
- ❌ **Don't deviate from plan** - follow TASKS_005.md order
- ❌ **Don't commit untested code** - all tests must pass before commit
- ❌ **Don't skip constitutional validation** - run audit after each phase

---

## Emergency Stop Procedures

### If Implementation Goes Off Track

1. **Agent breaks silence** with critical blocker
2. **User reviews** PATCH files and TASKS progress
3. **Identify root cause** (spec ambiguity, missing clarification, wrong assumption)
4. **Update SPEC/PLAN/TASKS** as needed
5. **Rollback problematic changes**
6. **Re-run `/analyze`** to validate fixes
7. **Resume implementation** from last good state

### If Timeline Exceeds Estimates

1. **Review TASKS_005.md progress** (which phase taking longest?)
2. **Identify bottlenecks** (complex controls, API issues, test failures)
3. **Consider phase split** (complete Phase 1-2, defer Phase 3-5)
4. **Update estimates** and communicate
5. **Adjust scope** if needed (move features to Phase 6)

### If Tests Fail Repeatedly

1. **Stop implementation**
2. **Review failing tests** (unit vs integration vs contract)
3. **Check for environment issues** (MAMP running, secrets configured)
4. **Review test setup** (mocks, fixtures, test data)
5. **Fix root cause** before continuing
6. **Update PLAN_005.md** if architecture needs adjustment

---

## Success Criteria

Feature 005 is successfully implemented when:

- ✅ All 150+ tasks completed
- ✅ All user stories testable and passing
- ✅ All functional requirements satisfied
- ✅ All acceptance criteria met
- ✅ All performance budgets met
- ✅ All constitutional principles satisfied
- ✅ All custom controls documented
- ✅ All TODOs completed (Constitution, Feature 003)
- ✅ Code coverage ≥80%
- ✅ Validation scripts pass
- ✅ Constitutional audit passes (100%)
- ✅ User acceptance testing passes

---

## Next Steps (Immediate Actions)

### Today (Step 2-3: Specify & Clarify)

1. **Read this guide thoroughly** (~20 minutes)
2. **Run `/specify` command** using RESTART-PROMPT.md
3. **Review generated SPEC_005.md** (~1 hour)
4. **Run `/clarify` command** to identify ambiguities
5. **Answer clarification questions** (~1 hour)
6. **Update SPEC_005.md** with clarifications
7. **Commit**: `git commit -am "feat: complete Feature 005 specification"`

### Tomorrow (Step 4-5: Plan & Tasks)

1. **Run `/plan` command** for Feature 005
2. **Review generated PLAN_005.md** (~1 hour)
3. **Validate architecture** against existing patterns
4. **Run `/tasks` command** for Feature 005
5. **Review generated TASKS_005.md** (~2 hours)
6. **Adjust task breakdown** as needed
7. **Commit**: `git commit -am "feat: complete Feature 005 technical plan"`

### Day After (Step 6: Analyze)

1. **Run `/analyze` command** for Feature 005
2. **Review ANALYSIS_005.md** (~30 minutes)
3. **Address all gaps** and inconsistencies
4. **Re-run `/analyze`** to verify
5. **Final review** of SPEC/PLAN/TASKS
6. **Commit**: `git commit -am "feat: validate Feature 005 specification"`

### Following Week (Step 7: Implement)

1. **Final pre-implementation checklist**
2. **Approve Radio Silence Mode**
3. **Run `/implement` command**
4. **Monitor progress** (daily PATCH review)
5. **Approve phase completions**
6. **Run validation after each phase**

---

## Support Resources

- **This Guide**: `specs/005-migrate-infor-visual/SPECKIT-WORKFLOW-STEPS.md`
- **Restart Guide**: `specs/005-migrate-infor-visual/RESTART-GUIDE.md`
- **Reference Files**: `specs/005-migrate-infor-visual/reference/`
- **Spec-Kit Guides**: `docs/Specify Guides/`
- **Constitution**: `.specify/memory/constitution.md`
- **AGENTS.md**: Root-level agent guide
- **Copilot Instructions**: `.github/copilot-instructions.md`

---

## Questions?

If you're unsure about:
- **Spec-Kit commands**: See `docs/Specify Guides/SpecKit-Feature-Flow-Guide.md`
- **Feature scope**: See `RESTART-GUIDE.md` or `REFERENCE-CLARIFICATIONS.md`
- **Technical patterns**: See `reference/REFERENCE-EXISTING-PATTERNS.md`
- **Constitutional compliance**: See `reference/REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md`

**Ready to start?** Run `/specify using prompt file: specs/005-migrate-infor-visual/RESTART-PROMPT.md`

---

**Created**: October 8, 2025

**Status**: Ready for Step 2 (Specify)

**Estimated Completion**: 5-6 weeks from specification start
