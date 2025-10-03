# Constitutional Audit System - Quick Reference

## Files Created

### 1. `.specify/templates/constitutional-audit.md`
Template file containing the full audit checklist based on Constitution v1.1.0 principles.

### 2. `.github/prompts/constitutional-audit.prompt`
AI agent guidance for performing constitutional compliance audits. Contains:
- Execution flow
- Critical auto-fail checks
- Principle-specific search patterns
- Violation severity classification
- Output format specifications

### 3. `.specify/scripts/powershell/constitutional-audit.ps1`
PowerShell automation script that:
- Creates audit files from template
- Auto-fills feature metadata
- Supports JSON output for scripting
- Optional git commit integration
- Works with current branch or explicit feature ID

## Usage

### Basic Usage (Interactive)
```powershell
# Audit current feature (from branch)
.\.specify\scripts\powershell\constitutional-audit.ps1

# Audit specific feature
.\.specify\scripts\powershell\constitutional-audit.ps1 001-boot-sequence-splash
```

### Advanced Usage
```powershell
# JSON output (for parsing/automation)
.\.specify\scripts\powershell\constitutional-audit.ps1 -Json 001-boot-sequence-splash

# Auto-commit audit to git
.\.specify\scripts\powershell\constitutional-audit.ps1 -Commit 001-boot-sequence-splash

# Show help
.\.specify\scripts\powershell\constitutional-audit.ps1 -Help
```

## Workflow

1. **Generate Audit File**
   ```powershell
   .\.specify\scripts\powershell\constitutional-audit.ps1 001-boot-sequence-splash
   ```
   Creates: `specs/001-boot-sequence-splash/constitutional-audits/AUDIT_001-boot-sequence-splash_YYYY-MM-DD.md`

2. **AI Agent Analysis**
   - Agent reads `.github/prompts/constitutional-audit.prompt`
   - Agent analyzes all files mentioned in feature's `plan.md`
   - Agent fills in audit checklist with findings

3. **Document Violations**
   - List specific file paths and line numbers
   - Mark checkboxes for compliant items
   - Add remediation plans for violations

4. **Generate Summary**
   - Count total violations
   - Classify as Critical vs Non-Critical
   - Estimate remediation time
   - Prioritize action items

5. **Take Action**
   - Fix critical violations immediately
   - Plan follow-up work for non-critical items
   - Update audit file with results
   - Commit audit results

## Audit Document Structure

Each audit file follows this structure:
1. **Metadata** - Feature name, date, auditor, status
2. **🎯 Next Steps** - Actionable remediation plan (BLOCKING issues first)
3. **Principle-by-Principle Analysis** - Detailed compliance checks
4. **Summary** - Violation counts, compliance percentage, trends

This puts the most important information (what to fix) at the top.

## Output Structure

```
specs/001-boot-sequence-splash/
├── spec.md
├── plan.md
├── tasks.md
├── AUDIT_001-boot-sequence-splash_2025-10-03.md  ← Generated audit
└── ...other files
```

## Critical Principles (Auto-Fail)

These violations **block PR approval**:
- ❌ No CompiledBinding in XAML (Principle VI)
- ❌ Missing CancellationToken in async APIs (Async/Await)
- ❌ ReactiveUI when MVVM Toolkit specified (Principle II)
- ❌ SQL injection vulnerabilities (Security)
- ❌ Direct P/Invoke without abstraction (Principle I)

## Integration with Spec-Kit

This audit system integrates with the existing spec-kit workflow:
1. `/specify` → Create feature spec
2. `/clarify` → Resolve ambiguities
3. `/plan` → Create technical plan
4. `/tasks` → Generate task list
5. `/implement` → Build the feature
6. **`/constitutional-audit`** ← New step: Verify compliance
7. Remediate violations
8. Merge to main

## Example Output

```json
{
  "AUDIT_FILE": "specs/001-boot-sequence-splash/AUDIT_001-boot-sequence-splash_2025-10-03.md",
  "FEATURE_ID": "001-boot-sequence-splash",
  "FEATURE_NAME": "Boot Sequence — Splash-First, Services Initialization Order",
  "AUDIT_DATE": "2025-10-03",
  "TEMPLATE_CREATED": true
}
```

## Next Steps

1. ✅ Template created and tested
2. ✅ PowerShell script implemented
3. ✅ AI guidance documented
4. 🔄 Perform actual audit on 001-boot-sequence-splash
5. 📝 Document findings and violations
6. 🔧 Create remediation plans
7. ✨ Fix critical violations
8. 📦 Commit results

## Notes

- Audit files are named with date: `AUDIT_[FEATURE]_[DATE].md`
- Multiple audits can exist for the same feature (track progress)
- AI agents should use the `.prompt` file for guidance
- Scripts work in both git and non-git environments
- JSON output enables automation and CI/CD integration
