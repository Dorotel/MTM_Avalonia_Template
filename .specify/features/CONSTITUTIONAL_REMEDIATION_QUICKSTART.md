# Constitutional Remediation Quick Start

## What Is This?

The Constitutional Remediation system converts audit violations into actionable fix checklists that AI agents can execute. It's the bridge between "/constitutional-audit" (what's wrong) and "/implement" (fixing it).

---

## Quick Start (30 seconds)

```powershell
# 1. Generate remediation checklist
.\.specify\scripts\powershell\constitutional-remediation.ps1 001-boot-sequence-splash

# 2. Agent implements fixes (you or GitHub Copilot coding agent)
# Agent reads REMEDIATION_*.md and executes each REM item

# 3. Check progress
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Status 001-boot-sequence-splash

# 4. Re-audit when complete
.\.specify\scripts\powershell\constitutional-audit.ps1 001-boot-sequence-splash
```

---

## For AI Agents

You've been given a `REMEDIATION_*.md` file. Here's what to do:

### Step 1: Read the File
```markdown
## 🔴 BLOCKING Remediations

- [ ] REM001: Fix XAML Binding in MainView.axaml
  - **Related Task**: T125
  - **Files**: MTM_Template_Application/Views/MainView.axaml:15
  - **Before**: {Binding Greeting}
  - **After**: {CompiledBinding Greeting}
  - **Test**: dotnet build
```

### Step 2: Implement Each REM
1. Open the file at the specified line
2. Find the "Before" code
3. Replace with "After" code
4. Run the verification command
5. If it passes:
   - Mark `[x] REM001` in REMEDIATION file
   - Mark `[x] T125` in tasks.md file
   - Git commit with suggested message

### Step 3: Move to Next REM
Repeat until all BLOCKING items are complete.

### Step 4: Verify
After all fixes:
- Run `dotnet build && dotnet test`
- Re-run constitutional audit
- Should show ✅ COMPLIANT

---

## Key Rules for Agents

1. **Follow order** - Complete BLOCKING before Non-Blocking
2. **One at a time** - Don't batch unrelated fixes
3. **Mark both files** - Update REMEDIATION and tasks.md
4. **Verify each** - Run test command before moving on
5. **Atomic commits** - One REM = one commit

---

## Example Agent Workflow

```
Agent: Reading REMEDIATION_001-boot-sequence-splash_2025-10-03.md
Agent: Found 43 BLOCKING items, 5 Non-Blocking items
Agent: Starting with REM001...

Agent: Opening MTM_Template_Application/Views/MainView.axaml:15
Agent: Found: <TextBlock Text="{Binding Greeting}" />
Agent: Replacing with: <TextBlock Text="{CompiledBinding Greeting}" />
Agent: Running: dotnet build
Agent: ✅ Build succeeded, 0 warnings

Agent: Marking [x] REM001 in REMEDIATION_001-boot-sequence-splash_2025-10-03.md
Agent: Marking [x] T125 in tasks.md (line 215)
Agent: Committing: fix(xaml): Use CompiledBinding per Principle VI

Agent: ✅ REM001 complete (1/43)
Agent: Moving to REM002...
```

---

## Progress Tracking

Check status anytime:
```powershell
.\.specify\scripts\powershell\constitutional-remediation.ps1 -Status 001-boot-sequence-splash
```

Output:
```
📊 Remediation Progress for 001-boot-sequence-splash
Status: IN_PROGRESS (12 / 43 items, 28%)
File: specs/001-boot-sequence-splash/REMEDIATION_001-boot-sequence-splash_2025-10-03.md
```

---

## Troubleshooting

### "Can't find Before code"
The code may have changed. Check the file manually and update the REM item.

### "Verification failed"
The fix didn't work. Review the "After" code and try again.

### "Multiple remediation files"
Script uses most recent. Delete old ones if not needed.

---

## Integration with Spec-Kit Commands

```
/specify          → Create spec
/clarify          → Resolve ambiguities
/plan             → Technical design
/tasks            → Generate task list
/implement        → Build feature (Phases 3.1-3.4)
/constitutional-audit → Check compliance ❌ Violations found
                    ↓
/constitutional-remediation → Generate fix checklist ← YOU ARE HERE
                    ↓
Agent implements fixes + marks tasks.md
                    ↓
/constitutional-audit → Re-check ✅ Should pass now
                    ↓
/implement        → Continue (Phases 3.5+)
```

---

## Files You'll See

```
specs/001-boot-sequence-splash/
├── AUDIT_001-boot-sequence-splash_2025-10-03.md       ← What's wrong (analysis)
├── REMEDIATION_001-boot-sequence-splash_2025-10-03.md ← How to fix (actions)
└── tasks.md                                            ← Task list (syncs with REM)
```

---

## That's It!

The system is designed to be simple:
1. Audit finds violations
2. Remediation creates actionable fix list
3. Agent (or you) implements fixes
4. Tasks.md stays in sync
5. Re-audit confirms compliance

**Questions?** See `.specify/features/CONSTITUTIONAL_REMEDIATION_README.md` for full documentation.
