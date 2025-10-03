---
description: Execute constitutional remediation by processing and completing all REM items from the remediation checklist
---

The user input can be provided directly by the agent or as a command argument - you **MUST** consider it before proceeding with the prompt (if not empty).

User input:

$ARGUMENTS

## Purpose

This command executes constitutional remediation fixes for a feature based on violations identified in a constitutional audit. It processes the remediation checklist (REMEDIATION_*.md) systematically, applying fixes in dependency order while maintaining atomic commits and proper verification.

## Prerequisites

- Constitutional audit must exist (AUDIT_[FEATURE]_[DATE].md)
- Remediation checklist must exist (REMEDIATION_[FEATURE]_[DATE].md)
- Audit status must show violations (not COMPLIANT)
- Feature must have tasks.md for cross-reference

## Execution Flow

### 1. Identify and Validate Feature

```powershell
# Run from repo root
.\.specify\scripts\powershell\check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
```

Parse output to get:
- `FEATURE_DIR`: Absolute path to feature directory
- `FEATURE_ID`: Feature identifier (e.g., 001-boot-sequence-splash)
- `AVAILABLE_DOCS`: List of available documentation files

If no feature specified in $ARGUMENTS:
- Use current branch name to detect feature
- Or use most recent feature with remediation file
- Or ERROR: "No feature specified and cannot auto-detect"

### 2. Load Remediation Context

**REQUIRED FILES** (must exist or ERROR):
- `{FEATURE_DIR}/REMEDIATION_{FEATURE}_{DATE}.md` - Main remediation checklist
- `{FEATURE_DIR}/AUDIT_{FEATURE}_{DATE}.md` - Original audit findings
- `{FEATURE_DIR}/tasks.md` - Task cross-references

**OPTIONAL FILES** (load if exists):
- `{FEATURE_DIR}/plan.md` - Technical architecture context
- `{FEATURE_DIR}/spec.md` - Feature specification
- `memory/constitution.md` - Constitutional principles reference

### 3. Parse Remediation Checklist Structure

Extract from REMEDIATION_*.md:

#### A. Progress Summary
```markdown
| Category | Total | Complete | Remaining | % Complete |
|----------|-------|----------|-----------|------------|
| **BLOCKING Issues** | 43 | 0 | 43 | 0% |
```

Parse:
- `TOTAL_ITEMS`: Total REM items
- `BLOCKING_ITEMS`: Critical issues (must fix before merge)
- `NON_BLOCKING_ITEMS`: Warnings (can fix later)
- `COMPLETED_ITEMS`: Items already marked [x]
- `COMPLETION_PERCENTAGE`: Progress percentage

#### B. REM Item Details

For each REM item, extract:
- **Checkbox State**: `[ ]` (pending) or `[x]` (complete)
- **REM ID**: REM001, REM002, etc.
- **Title**: Brief description
- **Related Task**: T### from tasks.md
- **Principle Violated**: Which constitutional rule
- **Severity**: 🔴 BLOCKING or ⚠️ WARNING
- **Files**: Array of file paths with line numbers
- **Before Code**: Current code (exact string)
- **After Code**: Required code (exact string)
- **Verification Steps**: Commands to run
- **Commit Message**: Suggested git commit message
- **Estimated Time**: Time estimate in minutes/hours
- **Dependencies**: Array of prerequisite REM IDs

#### C. Group Structure

Parse REM items into execution groups:
- **Group 1**: XAML Binding Violations (REM001-002)
- **Group 2**: Missing CancellationToken (REM003-043)
  - **Phase 1**: Interface Updates (REM003-008)
  - **Phase 2**: Implementation Updates (REM009-042)
  - **Phase 3**: Test Updates (REM043)
- **Group 3**: Dependency Injection (REM044-045)
- **Group 4**: ViewModel Unit Tests (REM046-047)
- **Group 5**: Code Coverage (REM048)

### 4. Execution Strategy

#### Priority Order

1. **BLOCKING Issues First**: Complete all 🔴 BLOCKING before ⚠️ WARNING
2. **Dependency Order**: Complete dependency REMs before dependent REMs
3. **Group Order**: Follow group sequence (Group 1 → Group 2 → Group 3...)
4. **Atomic Changes**: One REM item = one commit (for traceability)

#### Execution Rules

- **Skip Completed**: If `[x]` already checked, skip to next
- **Check Dependencies**: Before starting REM###, verify all dependency REMs are `[x]`
- **Read Current File State**: Always read file before editing (may have manual changes)
- **Apply Exact Changes**: Use Before/After code snippets exactly as specified
- **Run Verification**: Execute verification commands after each fix
- **Commit Individually**: Use suggested commit message format
- **Update Checklist**: Mark `[x]` after successful completion
- **Update Progress**: Recalculate completion percentage

#### Parallel vs Sequential

- **Sequential**: Same file edits must be sequential
- **Sequential**: Interface changes must complete before implementation changes
- **Parallel Possible**: Different files with no dependencies can be done in parallel
- **Parallel Possible**: Test creation can happen in parallel with non-blocking fixes

### 5. Implementation Execution

For each REM item (in dependency order):

#### Step 1: Pre-Execution Checks

```
1. Check if REM item already marked [x] → Skip if complete
2. Check dependencies: All dependency REMs must be [x]
3. Read current file state (may have manual edits since checklist creation)
4. Verify file paths exist (create directories if needed)
```

#### Step 2: Apply Fix

**For Code Changes** (REM with Before/After):
```
1. Use multi_replace_string_in_file if multiple files affected
2. Match "Before" code exactly (including whitespace)
3. Replace with "After" code exactly
4. Handle edge cases:
   - File may have changed since audit
   - Line numbers may have shifted
   - Use surrounding context to find correct location
```

**For File Creation** (REM creating new files):
```
1. Create parent directories if needed
2. Use exact code from "After" section
3. Follow project conventions (namespace, using statements, etc.)
```

**For Configuration Changes** (REM editing config files):
```
1. Parse existing configuration
2. Add/modify settings as specified
3. Preserve existing settings not mentioned
```

#### Step 3: Verification

```
1. Run verification command(s) from REM item:
   - dotnet build
   - dotnet test --filter "TestName"
   - Custom validation commands
2. Check for expected output (no errors, tests pass, etc.)
3. If verification fails:
   - Report error with full output
   - Do NOT mark [x]
   - Do NOT commit
   - HALT execution (unless non-critical)
```

#### Step 4: Commit

```
1. Stage changed files only (not entire workspace)
2. Use suggested commit message from REM item
3. Commit with REM ID in message for traceability
4. Example: "fix(xaml): use CompiledBinding in MainView [REM001]"
```

#### Step 5: Update Checklist

```
1. Mark REM item as [x] in REMEDIATION_*.md
2. Recalculate completion percentage
3. Update progress summary table
4. Commit checklist update separately
5. Example: "docs: mark REM001 complete in remediation checklist"
```

### 6. Special Remediation Patterns

#### Pattern A: Interface + Implementation Updates

When REM items update interfaces and implementations:

```
1. Complete ALL interface REMs first (REM003-008)
2. Verify: dotnet build (expect compilation errors in implementations)
3. Then complete implementation REMs (REM009-042)
4. Verify: dotnet build (expect no errors)
5. Finally complete test REMs (REM043)
6. Verify: dotnet test (expect all pass)
```

**Why**: Interface changes are breaking. Implementations must update together.

#### Pattern B: XAML Binding Changes

When REM items fix XAML bindings:

```
1. Verify x:DataType exists on root element
2. Change {Binding} to {CompiledBinding}
3. Add x:CompileBindings="True" if missing
4. Verify: dotnet build (no binding warnings)
5. Verify: Avalonia previewer opens without errors
6. Test: Run app and verify UI renders correctly
```

**Why**: CompiledBinding requires x:DataType for compile-time validation.

#### Pattern C: CancellationToken Propagation

When REM items add CancellationToken parameters:

```
1. Add parameter: CancellationToken cancellationToken = default
2. Add cancellation check: cancellationToken.ThrowIfCancellationRequested();
3. Propagate to downstream calls: await service.DoWorkAsync(cancellationToken);
4. Update HttpClient calls: await httpClient.GetAsync(url, cancellationToken);
5. Update database calls: await command.ExecuteReaderAsync(cancellationToken);
6. Handle timeout scenarios: CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
```

**Why**: Cancellation must propagate through entire call chain.

#### Pattern D: Dependency Injection Registration

When REM items configure DI:

```
1. Add Microsoft.Extensions.DependencyInjection reference (if needed)
2. Update Program.cs with .ConfigureServices()
3. Register services: Singleton vs Transient vs Scoped
4. Register interfaces before implementations
5. Use factory pattern for platform-specific services
6. Verify: Create integration test that resolves services
```

**Why**: Service lifetime and registration order matter.

### 7. Progress Tracking and Reporting

#### During Execution

After each REM item completion:
```
✅ REM001 Complete: Fix legacy Binding syntax in MainView.axaml
   - Files: MTM_Template_Application/Views/MainView.axaml
   - Verification: dotnet build ✓
   - Commit: fix(xaml): use CompiledBinding in MainView [REM001]
   - Progress: 1/48 (2%)
```

After each failed REM item:
```
❌ REM009 Failed: Update BootOrchestrator Implementation
   - Error: Compilation error in BootOrchestrator.cs:45
   - Output: [error details]
   - Action: Fix compilation error and retry
   - Progress: Halted at 8/48 (17%)
```

#### After Each Group

```
🎯 Group 1 Complete: XAML Binding Violations
   - Items: REM001-002 (2 items)
   - Time: 15 minutes
   - Status: ✅ All verified
   - Progress: 2/48 (4%)
   - Next: Group 2 - Missing CancellationToken
```

### 8. Error Handling

#### Compilation Errors

```
IF: dotnet build fails after REM item
THEN:
  1. Report full build output
  2. Do NOT mark [x]
  3. Do NOT commit
  4. If BLOCKING: HALT execution
  5. If NON-BLOCKING: Skip and continue
  6. Suggest: Review "After" code for syntax errors
```

#### Test Failures

```
IF: dotnet test fails after REM item
THEN:
  1. Report failing test names and output
  2. Do NOT mark [x]
  3. Do NOT commit
  4. If integration test: Check service dependencies
  5. If unit test: Review implementation logic
  6. Suggest: Run test in isolation to debug
```

#### File Not Found

```
IF: File path in REM item doesn't exist
THEN:
  1. Check if file was moved/renamed
  2. Search for similar file names
  3. Check git history for deletions
  4. If found: Update REM with correct path
  5. If not found: Skip with warning
  6. Report: "File not found, may need manual review"
```

#### Merge Conflicts

```
IF: File has manual edits conflicting with REM fix
THEN:
  1. Read current file state
  2. Check if fix already applied manually
  3. If already fixed: Mark [x] and skip
  4. If conflict: Report difference
  5. Suggest: Manual review needed
  6. Do NOT overwrite manual changes
```

### 9. Completion Validation

After all REM items processed:

#### A. Verify Completion Status

```
1. Check all BLOCKING items marked [x]
2. Calculate final completion percentage
3. Update remediation status:
   - 🔴 NOT STARTED → 🟡 IN PROGRESS → ✅ COMPLETE
4. Update progress summary table
5. Commit final checklist state
```

#### B. Run Full Validation

```
1. dotnet clean
2. dotnet restore
3. dotnet build
4. dotnet test
5. Check for warnings
6. Verify no regressions
```

#### C. Cross-Check with Audit

```
1. Re-read AUDIT_*.md violations
2. Verify each violation has corresponding completed REM item
3. Check if any violations missed
4. Report any gaps
```

#### D. Suggest Re-Audit

```
IF: All BLOCKING items complete (100%)
THEN:
  Suggest: "Run /constitutional-audit to verify 100% compliance"

IF: Only non-blocking items remain
THEN:
  Suggest: "BLOCKING fixes complete. Merge-ready. Address non-blocking in follow-up."
```

### 10. Final Report

Generate completion summary:

```markdown
## Constitutional Remediation Complete

**Feature**: 001-boot-sequence-splash
**Remediation File**: REMEDIATION_001-boot-sequence-splash_2025-10-03.md
**Audit File**: AUDIT_001-boot-sequence-splash_2025-10-03.md

### Summary
- **Total Items**: 48
- **Completed**: 43 (89%)
- **Remaining**: 5 (11%)
- **BLOCKING Complete**: 43/43 ✅
- **Non-Blocking Complete**: 0/5 ⚠️

### Completed Groups
- ✅ Group 1: XAML Binding Violations (2 items)
- ✅ Group 2: Missing CancellationToken (41 items)
  - ✅ Phase 1: Interface Updates (6 items)
  - ✅ Phase 2: Implementation Updates (34 items)
  - ✅ Phase 3: Test Updates (1 item)
- ⏭️ Group 3: Dependency Injection (0/2 items) - Non-blocking
- ⏭️ Group 4: ViewModel Unit Tests (0/2 items) - Non-blocking
- ⏭️ Group 5: Code Coverage (0/1 items) - Non-blocking

### Verification Results
- ✅ dotnet build: No errors, 0 warnings
- ✅ dotnet test: 171 tests passed
- ✅ No regressions detected
- ✅ All BLOCKING violations resolved

### Next Steps
1. ✅ Commit remediation checklist updates
2. ✅ Push branch to remote
3. 🔄 Run `/constitutional-audit` to verify compliance
4. 📝 Update PR with remediation summary
5. ⏭️ Address non-blocking items in follow-up (optional)

### Commits
- fix(xaml): use CompiledBinding in MainView [REM001]
- fix(xaml): add x:DataType and x:CompileBindings to MainWindow [REM002]
- fix(interfaces): add CancellationToken to IBootOrchestrator async methods [REM003]
- [... 40 more commits ...]
- test: update all tests to support CancellationToken parameters [REM043]
- docs: update remediation progress to 89% complete

**Status**: 🟢 MERGE-READY (All blocking issues resolved)
```

## Usage Examples

```bash
# Execute remediation for feature 001-boot-sequence-splash
/implement-remediation 001-boot-sequence-splash

# Execute remediation for feature detected from current branch
/implement-remediation

# Execute only BLOCKING items (skip non-blocking)
/implement-remediation --blocking-only 001-boot-sequence-splash

# Resume from specific REM item (skip completed)
/implement-remediation --start-from REM009 001-boot-sequence-splash

# Dry run (show what would be done, don't execute)
/implement-remediation --dry-run 001-boot-sequence-splash
```

## Integration with Spec-Kit Workflow

### Before Implementation

```
1. /specify → Create feature spec
2. /plan → Create implementation plan
3. /tasks → Generate task list
4. /implement → Execute implementation
5. /constitutional-audit → Audit compliance
6. /constitutional-remediation → Generate REM checklist
```

### During Remediation

```
7. /implement-remediation → Execute fixes (THIS COMMAND)
```

### After Remediation

```
8. /constitutional-audit → Verify 100% compliance
9. Merge PR if compliant
```

## Notes for AI Agents

- **Always read current file state** before editing (manual changes may exist)
- **Use exact Before/After code** from REM items (don't paraphrase)
- **Atomic commits** - one REM = one commit (traceability)
- **Verify before committing** - all verification steps must pass
- **Update checklist progress** - mark [x] and update percentages
- **Respect dependencies** - don't start dependent REM until dependency complete
- **Halt on critical failures** - BLOCKING items must succeed
- **Report clearly** - user should understand progress and blockers
- **Cross-reference tasks.md** - use Task IDs for additional context
- **Follow constitutional patterns** - refer to constitution.md when in doubt

## Success Criteria

✅ All BLOCKING REM items marked [x]
✅ All verification commands pass
✅ dotnet build succeeds with no warnings
✅ dotnet test succeeds (all tests pass)
✅ Each REM committed individually with suggested message
✅ Remediation checklist progress updated to ≥89%
✅ Ready for re-audit

---

**Prompt Version**: 1.0.0
**Compatible With**: Constitutional Audit v1.0, Remediation Checklist v1.0
**Last Updated**: 2025-10-03
