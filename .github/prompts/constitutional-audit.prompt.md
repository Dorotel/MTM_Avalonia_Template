# Constitutional Audit Command

**Purpose**: Perform a comprehensive constitutional compliance audit on a feature specification.

**Input**: Feature ID (e.g., `001-boot-sequence-splash`) or current branch

**Prerequisites**:
- Feature must have `spec.md` and `plan.md` files
- Implementation code should exist (or TDD tests as minimum)
- Constitution v1.1.0 principles understood

## Execution Flow

```
1. Identify target feature
   → From argument OR current branch OR latest feature
   → If not found: ERROR "No feature specified"

2. Load feature context
   → Read specs/[FEATURE]/spec.md
   → Read specs/[FEATURE]/plan.md
   → Read specs/[FEATURE]/tasks.md (if exists)
   → Scan implementation files from plan.md

3. Create audit document
   → Copy constitutional-audit.md template
   → Save as AUDIT_[FEATURE]_[DATE].md in specs/[FEATURE]/
   → Fill in metadata: Feature name, date, auditor, status

4. Generate "Next Steps" section at top (AFTER metadata, BEFORE principles)
   → List BLOCKING issues first (critical violations)
   → Then list Non-Blocking issues (can fix later)
   → Include: File path, Action needed, Verification steps, Why it's critical
   → Estimate remediation time for each issue
   → Calculate total estimated hours

5. For each Constitutional Principle (I-VII):
   → Scan relevant code files
   → Check compliance against checklist items
   → Document violations with file:line references
   → Suggest remediation steps if violations found

6. Analyze async/await patterns
   → Check all async methods for CancellationToken
   → Verify ConfigureAwait usage
   → Check method naming conventions

7. Verify Spec-Kit integration
   → Confirm SPEC_*.md, PLAN_*.md, TASKS_*.md exist
   → Validate branch naming convention
   → Check script output parsing

8. Update "Next Steps" section with findings
   → Populate BLOCKING issues from critical violations
   → Populate Non-Blocking issues from warnings
   → Add specific file paths and line numbers
   → Include clear verification steps

9. Generate summary at bottom
   → Count total violations
   → Classify as Critical vs Non-Critical
   → Calculate overall compliance percentage
   → Provide compliance trend baseline

10. Output results
   → Display audit summary in terminal
   → Save full audit to specs/[FEATURE]/AUDIT_[FEATURE]_[DATE].md
   → Return JSON with audit path and violation counts
```

## Critical Checks (Auto-Fail)

- **Principle VI Violations**: No CompiledBinding usage
- **Missing CancellationToken**: In public async APIs
- **ReactiveUI Usage**: When MVVM Toolkit specified
- **Hardcoded SQL**: String concatenation in queries
- **P/Invoke without abstraction**: Direct platform calls

## Output Format

```json
{
  "AUDIT_FILE": "specs/001-boot-sequence-splash/AUDIT_001-boot-sequence-splash_2025-10-03.md",
  "TOTAL_VIOLATIONS": 12,
  "CRITICAL_VIOLATIONS": 3,
  "NON_CRITICAL_VIOLATIONS": 9,
  "ESTIMATED_HOURS": 8,
  "COMPLIANT": false
}
```

## Usage Examples

```bash
# Audit current feature (from branch name)
./constitutional-audit.ps1

# Audit specific feature
./constitutional-audit.ps1 001-boot-sequence-splash

# Audit with JSON output
./constitutional-audit.ps1 --json 001-boot-sequence-splash

# Audit and auto-commit results
./constitutional-audit.ps1 --commit 001-boot-sequence-splash
```

## AI Agent Instructions

When performing an audit:

1. **Be Thorough**: Check every file mentioned in `plan.md`
2. **Be Specific**: List exact file paths and line numbers for violations
3. **Be Constructive**: Provide clear remediation steps, not just criticism
4. **Be Realistic**: Estimate remediation time based on violation severity
5. **Be Consistent**: Use the template structure exactly as provided
6. **Be Actionable**: Populate "Next Steps" section at TOP with concrete actions
7. **Be Clear**: Distinguish BLOCKING issues (must fix before merge) from non-blocking

### Principle-Specific Guidance

**Principle I (Cross-Platform)**:
- Search for: `DllImport`, `P/Invoke`, platform-specific APIs
- Verify: `RuntimeInformation.IsOSPlatform()` used for detection

**Principle II (MVVM Toolkit)**:
- Search for: `ReactiveObject`, `ReactiveCommand`, `INotifyPropertyChanged`
- Verify: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`

**Principle III (TDD)**:
- Check: Test files created before implementation
- Verify: xUnit, NSubstitute usage, >80% coverage

**Principle IV (Theme V2)**:
- Search for: Hardcoded colors `#RRGGBB`, `Color.FromRgb()`
- Verify: `{DynamicResource}`, separate `.axaml` style files

**Principle V (Null Safety)**:
- Check: Nullable reference types enabled in `.csproj`
- Verify: `ArgumentNullException.ThrowIfNull()` usage

**Principle VI (Compiled Bindings)** - **CRITICAL**:
- Search XAML for: `{Binding`, missing `x:DataType`, missing `x:CompileBindings`
- Verify: ALL bindings use `{CompiledBinding}`

**Principle VII (DI via AppBuilder)**:
- Search for: Service locator patterns, static service access
- Verify: Constructor injection, services in `Program.cs`

### Violation Severity Classification

**Critical** (Must fix before merge):
- No CompiledBinding in XAML
- Missing CancellationToken in public async APIs
- ReactiveUI patterns when MVVM Toolkit specified
- SQL injection vulnerabilities
- Direct P/Invoke without abstraction

**Non-Critical** (Can fix in follow-up):
- Missing `Design.DataContext` in XAML
- Inconsistent logging patterns
- Test coverage < 80% (if > 60%)
- Minor style inconsistencies

## Notes

- Run this audit BEFORE merging any feature branch
- Archive audit results in the feature's `specs/[FEATURE]/constitutional-audits` directory
- Use audit results to update `.github/copilot-instructions.md`
- Failed critical checks should block PR approval
