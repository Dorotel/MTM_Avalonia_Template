# Constitutional Remediation Command

**Purpose**: Generate an actionable remediation checklist from constitutional audit violations.

**Input**: Feature ID (e.g., `001-boot-sequence-splash`) or current branch

**Prerequisites**:
- Constitutional audit MUST exist (`AUDIT_[FEATURE]_[DATE].md`)
- Audit must have identified violations in "Next Steps" section
- Feature must have `tasks.md` file for task tracking

## Execution Flow

```
1. Identify target feature
   → From argument OR current branch OR latest feature
   → If not found: ERROR "No feature specified"

2. Locate existing audit file
   → Search specs/[FEATURE]/AUDIT_*.md (use most recent if multiple)
   → If not found: ERROR "No audit found. Run /constitutional-audit first"
   → If audit status is COMPLIANT: INFO "No violations found, nothing to remediate"

3. Extract violations from audit
   → Parse "Next Steps" section
   → Extract BLOCKING issues (critical violations)
   → Extract Non-Blocking issues (warnings)
   → Capture: File paths, Line numbers, Actions, Verification steps, Time estimates

4. Map violations to tasks.md
   → Load specs/[FEATURE]/tasks.md
   → Match violation file paths to task IDs (e.g., MainView.axaml → T125)
   → Create bidirectional links: REM items ↔ Task IDs

5. Generate remediation checklist
   → Use constitutional-remediation.md template
   → Save as REMEDIATION_[FEATURE]_[DATE].md in specs/[FEATURE]/
   → Fill metadata: Feature, Date, Audit reference, Status
   → Create REM001, REM002... items with full context

6. Include code patterns
   → For each violation, include Before/After code snippets
   → Provide exact changes needed (not just descriptions)
   → Include file:line references for direct navigation

7. Add verification commands
   → Each REM item has specific test command
   → Include git commit message suggestions
   → Provide acceptance criteria

8. Output results
   → Display remediation summary in terminal
   → Save full checklist to specs/[FEATURE]/REMEDIATION_[FEATURE]_[DATE].md
   → Return JSON with file path and item counts
```

## Remediation Item Structure

Each REM item must include:

```markdown
- [ ] REM001 [Brief Description]
  - **Related Task**: T### (from tasks.md)
  - **Principle Violated**: [Which constitutional principle]
  - **Severity**: [BLOCKING / WARNING]
  - **Files**:
    - `path/to/file.cs:line` (click to open)
  - **Before**:
    ```csharp
    // Current code (exact)
    ```
  - **After**:
    ```csharp
    // Required code (exact)
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: No binding warnings
  - **Commit Message**: `fix(scope): brief description`
  - **Estimated Time**: X minutes
  - **Dependencies**: [REM### that must be completed first, if any]
```

## Critical Rules for AI Agents

When implementing remediations:

1. **Follow Order**: Complete BLOCKING items before Non-Blocking
2. **One at a Time**: Don't batch unrelated fixes (atomic commits)
3. **Mark Progress**: Update `[x]` in remediation checklist as you complete items
4. **Update tasks.md**: When marking REM item complete, also mark related task in tasks.md
5. **Verify Each**: Run verification command after each fix before moving to next
6. **Commit Individually**: Use suggested commit message for traceability
7. **Handle Dependencies**: Complete dependency REMs before dependent REMs

## Special Patterns

### Pattern 1: Missing CancellationToken (Async Methods)

When REM item is about adding CancellationToken:
- **Start with interfaces** (breaking changes)
- **Then update implementations** (propagate token)
- **Then update call sites** (pass token through)
- **Finally update tests** (verify cancellation support)

### Pattern 2: XAML Binding Changes

When REM item is about CompiledBinding:
- **Verify x:DataType exists** on root element
- **Change {Binding} to {CompiledBinding}** syntax
- **Add x:CompileBindings="True"** if missing
- **Test in designer** before committing

### Pattern 3: Dependency Injection Setup

When REM item is about DI registration:
- **Register in Program.cs** (Desktop) or MainActivity.cs (Android)
- **Use .ConfigureServices()** extension method
- **Register interfaces before implementations**
- **Test with integration test** that resolves service

## Progress Tracking

### Remediation Status Values
- `🔴 NOT STARTED` - No REM items completed
- `🟡 IN PROGRESS` - Some REM items completed (1-99%)
- `✅ COMPLETE` - All REM items completed (100%)
- `⏭️ SKIPPED` - Remediation not needed (audit was compliant)

### Completion Detection
- Script counts `[x]` vs `[ ]` items in remediation file
- Calculates completion percentage
- Updates remediation status automatically
- Can update audit status when 100% complete

## Output Format

```json
{
  "REMEDIATION_FILE": "specs/001-boot-sequence-splash/REMEDIATION_001-boot-sequence-splash_2025-10-03.md",
  "AUDIT_FILE": "specs/001-boot-sequence-splash/AUDIT_001-boot-sequence-splash_2025-10-03.md",
  "TOTAL_ITEMS": 48,
  "BLOCKING_ITEMS": 43,
  "NON_BLOCKING_ITEMS": 5,
  "COMPLETED_ITEMS": 0,
  "COMPLETION_PERCENTAGE": 0,
  "STATUS": "NOT_STARTED",
  "ESTIMATED_HOURS": 12
}
```

## Usage Examples

```bash
# Generate remediation checklist from latest audit
./constitutional-remediation.ps1 001-boot-sequence-splash

# Generate with JSON output
./constitutional-remediation.ps1 --json 001-boot-sequence-splash

# Check remediation progress
./constitutional-remediation.ps1 --status 001-boot-sequence-splash

# Mark specific item complete (manual override)
./constitutional-remediation.ps1 --complete REM001 001-boot-sequence-splash
```

## Integration with /implement

When AI agent is implementing remediations:

```
Agent: "I'm implementing REM001: Fix XAML binding in MainView.axaml"
  → Opens file at specified line
  → Makes exact change (Before → After)
  → Runs verification command
  → If pass: Marks [x] REM001 + [x] T125
  → Commits with suggested message
  → Moves to next REM item
```

## Validation Rules

Before completing a REM item, verify:
- [ ] Exact change matches "After" code snippet
- [ ] Verification command passes
- [ ] No new errors introduced (run full test suite)
- [ ] Related task.md item also marked complete
- [ ] Git commit uses suggested message format

## Notes

- Remediation checklist is **action-oriented** (focus on HOW to fix)
- Audit document is **analysis-oriented** (focus on WHAT is wrong)
- Both documents reference each other for traceability
- Remediation progress can be tracked separately from overall feature progress
- Agent should update both remediation checklist AND tasks.md in sync
