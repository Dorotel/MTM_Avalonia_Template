# Constitutional Remediation System - Quick Reference

## Purpose

The Constitutional Remediation system extracts violations from constitutional audits and generates actionable, trackable fix checklists. It bridges the gap between "what's wrong" (audit) and "how to fix it" (remediation).

## Files Created

### 1. `.github/prompts/constitutional-remediation.prompt.md`
AI agent instructions for generating and implementing remediation checklists.

### 2. `.specify/templates/constitutional-remediation.md`
Template for remediation checklists with Before/After code patterns, verification steps, and task synchronization.

### 3. `.specify/scripts/powershell/constitutional-remediation.ps1`
PowerShell automation script (to be created).

### 4. This README
Usage documentation and workflow guidance.

---

## Workflow Integration

```mermaid
graph TD
    A[/specify - Create Spec] --> B[/plan - Technical Plan]
    B --> C[/tasks - Generate Tasks]
    C --> D[/implement - Build Feature]
    D --> E[/constitutional-audit - Check Compliance]
    E --> F{Violations Found?}
    F -->|Yes| G[/constitutional-remediation - Generate Checklist]
    G --> H[Agent Implements Fixes]
    H --> I[Mark REM items + Task items]
    I --> J{All Fixed?}
    J -->|No| H
    J -->|Yes| K[Re-run /constitutional-audit]
    K --> L{Now Compliant?}
    L -->|Yes| M[Continue /implement]
    L -->|No| G
    F -->|No| M
```

---

## Usage

### Basic Usage

```powershell
# Generate remediation checklist from latest audit
.\.specify\scripts\powershell\constitutional-remediation.ps1 001-boot-sequence-splash
```

**Output**:
```
specs/001-boot-sequence-splash/
├── AUDIT_001-boot-sequence-splash_2025-10-03.md        ← Original audit
└── REMEDIATION_001-boot-sequence-splash_2025-10-03.md  ← New remediation checklist
```

### Advanced Usage

```powershell
# JSON output for scripting
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Json 001-boot-sequence-splash

# Check remediation progress
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Status 001-boot-sequence-splash

# Mark specific item complete (if needed)
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Complete REM001 001-boot-sequence-splash
```

---

## Remediation Document Structure

```markdown
# Constitutional Remediation Checklist
**Feature**: 001-boot-sequence-splash
**Status**: 🔴 NOT STARTED → 🟡 IN PROGRESS → ✅ COMPLETE

## 🔴 BLOCKING Remediations

- [ ] REM001: Fix XAML Binding in MainView.axaml
  - **Related Task**: T125 (SplashViewModel)
  - **Files**: `MTM_Template_Application/Views/MainView.axaml:15`
  - **Before**: `{Binding Greeting}`
  - **After**: `{CompiledBinding Greeting}`
  - **Verification**: `dotnet build` (no warnings)
  - **Commit**: `fix(xaml): Use CompiledBinding per Principle VI`
  - **Time**: 15 minutes

- [ ] REM002: Add CancellationToken to All Async Methods
  - **Related Task**: T039-T054 (Service interfaces)
  - **Files**: 40+ service files
  - **Pattern**: Add `CancellationToken cancellationToken = default`
  - **Verification**: `dotnet build && dotnet test`
  - **Time**: 8-12 hours
  - **Dependencies**: Complete interfaces before implementations
```

---

## Key Features

### ✅ Task Synchronization
- Each `REM###` item links to `T###` task in `tasks.md`
- When agent marks `[x] REM001`, it also marks `[x] T125`
- Two-way traceability: Remediation ↔ Tasks

### ✅ Code Patterns
- Exact "Before" and "After" code snippets
- Not just descriptions - actual copy-paste code
- Language-specific syntax highlighting

### ✅ Verification Steps
- Specific commands to verify each fix
- Expected outcomes clearly stated
- Prevents "fixed but broken" scenarios

### ✅ Git Integration
- Suggested commit messages follow conventional commits
- Each REM item gets atomic commit
- Easy to track remediation in git history

### ✅ Dependency Tracking
- Some fixes must happen before others
- Dependencies explicitly stated in each REM item
- Prevents implementation in wrong order

---

## Agent Implementation Workflow

When an AI agent receives the remediation checklist:

```
1. Parse REMEDIATION_*.md file
2. Identify BLOCKING items
3. For each REM item (in order):
   a. Open file(s) at specified line numbers
   b. Locate "Before" code pattern
   c. Replace with "After" code pattern
   d. Run verification command
   e. If pass:
      - Mark [x] REM### in remediation file
      - Mark [x] T### in tasks.md file
      - Git commit with suggested message
   f. If fail:
      - Report error with diagnostic info
      - Pause for human review
4. After all BLOCKING items:
   - Run full test suite
   - Re-run constitutional audit
5. If audit passes:
   - Mark remediation status: ✅ COMPLETE
   - Update audit status: ✅ COMPLIANT
6. Proceed with remaining implementation
```

---

## Example: REM001 Fix XAML Binding

### Remediation Item
```markdown
- [ ] REM001: Fix XAML Binding in MainView.axaml
  - **Related Task**: T125 (SplashViewModel)
  - **Principle Violated**: Principle VI (Compiled Bindings Only)
  - **Severity**: BLOCKING
  - **Estimated Time**: 15 minutes

#### Files to Modify
- `MTM_Template_Application/Views/MainView.axaml:15`

#### Current Code (Before)
```xml
<TextBlock Text="{Binding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
```

#### Required Code (After)
```xml
<TextBlock Text="{CompiledBinding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
```

#### Verification Steps
1. Run: `dotnet build`
2. Expected: Build succeeds with 0 warnings
3. Test: Open Avalonia previewer, verify binding works

#### Git Commit
```bash
git add MTM_Template_Application/Views/MainView.axaml
git commit -m "fix(xaml): Use CompiledBinding in MainView per Principle VI"
```
```

### Agent Implementation
```
Agent: Opening MTM_Template_Application/Views/MainView.axaml
Agent: Located line 15: <TextBlock Text="{Binding Greeting}"...
Agent: Replacing {Binding Greeting} with {CompiledBinding Greeting}
Agent: Running verification: dotnet build
Agent: ✅ Build succeeded, 0 warnings
Agent: Marking [x] REM001 in REMEDIATION_001-boot-sequence-splash_2025-10-03.md
Agent: Marking [x] T125 in tasks.md (line 215)
Agent: Committing: fix(xaml): Use CompiledBinding in MainView per Principle VI
Agent: ✅ REM001 complete, moving to REM002
```

---

## Progress Tracking

### Remediation Status Values
- 🔴 **NOT STARTED** - 0% complete
- 🟡 **IN PROGRESS** - 1-99% complete
- ✅ **COMPLETE** - 100% complete
- ⏭️ **SKIPPED** - No violations found in audit

### Checking Progress
```powershell
# Get current progress
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Status 001-boot-sequence-splash
```

**Output**:
```json
{
  "REMEDIATION_FILE": "specs/001-boot-sequence-splash/REMEDIATION_001-boot-sequence-splash_2025-10-03.md",
  "TOTAL_ITEMS": 48,
  "COMPLETED_ITEMS": 12,
  "COMPLETION_PERCENTAGE": 25,
  "STATUS": "IN_PROGRESS",
  "BLOCKING_REMAINING": 31,
  "NON_BLOCKING_REMAINING": 5
}
```

---

## Integration with Other Commands

### Before Remediation
1. `/specify` - Create feature spec
2. `/clarify` - Resolve ambiguities
3. `/plan` - Technical design
4. `/tasks` - Generate task list
5. `/implement` - Build feature (Phases 3.1-3.4 in this case)
6. `/constitutional-audit` - Check compliance ❌ Violations found

### During Remediation
7. **`/constitutional-remediation`** - Generate fix checklist
8. Agent implements fixes using checklist
9. Agent marks items complete in both remediation + tasks.md

### After Remediation
10. `/constitutional-audit` - Re-check compliance ✅ Should pass now
11. `/implement` - Continue with remaining phases (3.5+)
12. `/analyze` - Final consistency check before merge

---

## File Naming Convention

- **Audit**: `AUDIT_[FEATURE]_[DATE].md`
- **Remediation**: `REMEDIATION_[FEATURE]_[DATE].md`
- **Pattern**: Same feature ID and date for traceability

**Example**:
```
specs/001-boot-sequence-splash/
├── AUDIT_001-boot-sequence-splash_2025-10-03.md
└── REMEDIATION_001-boot-sequence-splash_2025-10-03.md
```

Multiple remediations possible if audit fails multiple times:
```
specs/001-boot-sequence-splash/
├── AUDIT_001-boot-sequence-splash_2025-10-03.md
├── REMEDIATION_001-boot-sequence-splash_2025-10-03.md  ← First attempt
├── AUDIT_001-boot-sequence-splash_2025-10-04.md        ← Re-audit
└── REMEDIATION_001-boot-sequence-splash_2025-10-04.md  ← Second attempt (if needed)
```

---

## Best Practices

### For AI Agents

1. **Read the entire REM item first** before making changes
2. **Verify file:line still matches** (code may have changed)
3. **Use exact Before/After patterns** (don't improvise)
4. **Run verification after each fix** (don't batch)
5. **Update both remediation + tasks.md** (two-way sync)
6. **Commit atomically** (one REM = one commit)
7. **Handle dependencies** (complete prerequisites first)

### For Humans

1. **Review the remediation checklist** before agent starts
2. **Validate agent's fixes** periodically (don't trust blindly)
3. **Monitor verification outputs** for unexpected failures
4. **Use git history** to track remediation progress
5. **Re-run audit after completion** to confirm compliance

---

## Troubleshooting

### "No audit found" Error
**Cause**: `/constitutional-remediation` requires existing audit
**Fix**: Run `/constitutional-audit [FEATURE]` first

### "Verification failed" Error
**Cause**: Fix didn't resolve the issue
**Fix**: Review "Before" vs "After" code, check for typos

### "Can't find task T###" Error
**Cause**: Task ID doesn't exist in tasks.md
**Fix**: Update remediation to use correct task ID, or add task to tasks.md

### "Multiple audits found" Warning
**Cause**: Multiple `AUDIT_*.md` files exist
**Fix**: Script uses most recent (by date in filename)

---

## Next Steps

1. ✅ Prompt created (`.github/prompts/constitutional-remediation.prompt.md`)
2. ✅ Template created (`.specify/templates/constitutional-remediation.md`)
3. ✅ README created (this file)
4. 🔜 PowerShell script (`.specify/scripts/powershell/constitutional-remediation.ps1`)
5. 🔜 Test on `001-boot-sequence-splash` feature
6. 🔜 Document in main README.md

---

**Version**: 1.0 | **Created**: 2025-10-03 | **Status**: Ready for Implementation
