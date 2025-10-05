# Implementation Validation System

## Overview

This validation system ensures that completed features meet specification requirements and maintain constitutional compliance before merging to the main branch.

## Files Created

### 1. Prompt File
**Location**: `.github/prompts/validate-implementation.prompt.md`

Comprehensive instructions for AI agents and human reviewers performing deep validation of completed features. Includes:
- 16-phase validation process
- Constitutional compliance checks
- Task-to-code mapping procedures
- Requirement-to-implementation verification
- Code quality assessment guidelines
- Security and performance review criteria

### 2. PowerShell Script
**Location**: `.specify/scripts/powershell/validate-implementation.ps1`

Automated validation script that performs:
- Task completion verification (from tasks.md)
- Constitutional compliance audit
- Build validation (clean build check)
- Test execution and results parsing
- Report generation (markdown format)
- JSON output for CI/CD integration

### 3. Report Template
**Location**: `.specify/templates/validation-report-template.md`

Standardized template for validation reports including:
- Executive summary with quick stats
- Phase-by-phase validation results
- Constitutional compliance audit section
- Blocking and non-blocking issues
- Remediation recommendations
- Appendices for detailed data

## Usage

### Manual Validation (Human or AI Agent)

Follow the comprehensive guide in `.github/prompts/validate-implementation.prompt.md`:

1. Open the prompt file in your IDE
2. Follow each of the 16 phases systematically
3. Document findings in a new validation report
4. Use the template from `.specify/templates/validation-report-template.md`

### Automated Validation (PowerShell Script)

```powershell
# Basic usage (auto-detects feature from branch)
.\.specify\scripts\powershell\validate-implementation.ps1

# Validate specific feature
.\.specify\scripts\powershell\validate-implementation.ps1 001-boot-sequence-splash

# JSON output for automation/CI
.\.specify\scripts\powershell\validate-implementation.ps1 -Json

# Strict mode (fail on any warnings)
.\.specify\scripts\powershell\validate-implementation.ps1 -Strict

# Generate HTML report (future enhancement)
.\.specify\scripts\powershell\validate-implementation.ps1 -Html

# Generate PR comment format
.\.specify\scripts\powershell\validate-implementation.ps1 -PrComment
```

### When to Run Validation

**Trigger**: When `tasks.md` shows 100% completion (all tasks marked `[x]`)

**Workflow**:
1. Developer completes all tasks in feature branch
2. Developer commits final changes
3. Developer runs validation script
4. Script generates validation report
5. If PASS: Create pull request with report attached
6. If CONDITIONAL PASS: Create PR, document follow-up tasks
7. If FAIL: Fix blocking issues, re-run validation

## Validation Phases

### Phase 1: Task Completion Verification ✅ CRITICAL
- Parse all task IDs from tasks.md
- Verify each file path exists
- Check for TODO/NotImplementedException
- Verify tests exist for implementation tasks

### Phase 2: Functional Requirements Mapping ✅
- Extract all FR-XXX requirements from spec.md
- Map requirements to implementing tasks
- Locate implementation in codebase
- Verify test coverage

### Phase 3: Constitutional Compliance Audit ✅ CRITICAL
Checks all seven constitutional principles:
- **I. Cross-Platform First**: No direct P/Invoke
- **II. MVVM Toolkit**: No ReactiveUI patterns
- **III. TDD**: Tests written first, >80% coverage
- **IV. Theme V2**: No hardcoded colors (except splash)
- **V. Null Safety**: Nullable enabled, proper annotations
- **VI. Compiled Bindings**: ❗ CRITICAL - All XAML uses CompiledBinding
- **VII. DI via AppBuilder**: Constructor injection, no service locator

### Phase 4: Architecture Consistency ✅
- Verify all documented services exist
- Check all documented models exist
- Validate folder structure matches plan.md
- Confirm dependency list matches

### Phase 5: Data Model Validation ✅
- Verify all entities from data-model.md implemented
- Check all properties present
- Verify relationships implemented
- Confirm validation rules applied

### Phase 6: Test Execution and Coverage ✅
- Run `dotnet test`
- Parse results: total, passed, failed, skipped
- Calculate pass rate (must be 100%)
- Check coverage if tool available (target >80%)

### Phase 7: Build and Runtime Validation ✅
- Run `dotnet build`
- Verify 0 errors (required)
- Check warnings (0 preferred)
- Run smoke tests (app starts without crash)

### Phase 8: Code Quality Assessment
- Check for god classes (>500 lines)
- Check for long methods (>100 lines)
- Verify async/await patterns
- Check CancellationToken usage
- Assess error handling coverage

### Phase 9: Documentation Validation
- Verify spec.md complete
- Confirm plan.md matches implementation
- Check tasks.md all checked
- Validate quickstart.md scenarios
- Check inline code documentation

### Phase 10: Security and Performance
- No hardcoded credentials
- Proper input validation
- SQL injection prevention
- Performance targets met
- No memory leaks

## Validation Results

### Status Definitions

**PASS** ✅
- 100% tasks completed
- 100% tests passing
- Clean build (0 errors)
- 0 critical constitutional violations
- 0 blocking issues
- **Action**: Ready for merge immediately

**CONDITIONAL PASS** ⚠️
- 95-99% tasks completed
- >95% tests passing
- Clean build
- 0 critical violations
- <5 non-critical issues
- **Action**: Can merge with documented follow-up tasks

**FAIL** ❌
- <95% tasks completed
- Tests failing
- Build errors
- Critical constitutional violations
- Blocking issues exist
- **Action**: Fix issues and re-validate before merge

### Report Outputs

**Markdown Report**: `specs/[FEATURE]/VALIDATION_[FEATURE]_[DATE].md`
- Human-readable comprehensive report
- Saved in feature directory
- Includes all findings and recommendations

**JSON Output** (with `-Json` flag):
```json
{
  "VALIDATION_FILE": "...",
  "FEATURE_ID": "001-boot-sequence-splash",
  "COMPLETION_PERCENTAGE": 100,
  "TASKS_COMPLETED": 171,
  "TASKS_TOTAL": 171,
  "TESTS_PASSED": 311,
  "TESTS_TOTAL": 311,
  "CONSTITUTIONAL_VIOLATIONS": {
    "CRITICAL": 0,
    "NON_CRITICAL": 3
  },
  "BUILD_STATUS": "SUCCESS",
  "OVERALL_STATUS": "PASS",
  "READY_FOR_MERGE": true
}
```

**Exit Codes**:
- `0` - Validation passed, ready for merge
- `1` - Validation failed, blocking issues exist
- `2` - Script error (validation could not complete)

## Critical Checks

These checks **AUTO-FAIL** validation if violated:

1. **Principle VI Violations**: Missing CompiledBinding in XAML
2. **Missing CancellationToken**: In public async APIs
3. **ReactiveUI Usage**: When MVVM Toolkit specified
4. **SQL Injection**: String concatenation in queries
5. **Build Errors**: Any compilation errors
6. **Test Failures**: Any failing tests
7. **Incomplete Tasks**: <95% completion rate

## CI/CD Integration

Add to your GitHub Actions workflow:

```yaml
- name: Validate Implementation
  run: |
    $result = .\.specify\scripts\powershell\validate-implementation.ps1 -Json | ConvertFrom-Json
    if (-not $result.READY_FOR_MERGE) {
      Write-Error "Validation failed: $($result.OVERALL_STATUS)"
      exit 1
    }
  shell: pwsh
```

## Best Practices

### For Developers

1. **Run Early**: Validate at 90% completion to catch issues early
2. **Fix Immediately**: Address violations as you discover them
3. **Document Deviations**: If constitutional rules don't apply, document why
4. **Test First**: Write tests before marking tasks complete
5. **Clean Build**: Resolve all warnings, not just errors

### For Reviewers

1. **Check Report First**: Review validation report before code review
2. **Verify Critical Fixes**: Manually check critical violations were fixed
3. **Question Overrides**: Challenge any constitutional compliance overrides
4. **Test Validation**: Run validation script yourself if results seem suspect
5. **Archive Reports**: Save validation reports in feature directory

### For AI Agents

1. **Be Thorough**: Check every file, every task, every requirement
2. **Be Specific**: Provide exact file paths and line numbers
3. **Be Fair**: Consider context (test fixtures may use hardcoded values)
4. **Be Actionable**: Include remediation steps for every issue
5. **Be Consistent**: Use report template structure exactly

## Common Issues

### False Positives

- ❌ {Binding} in ItemTemplate DataContext switches (VALID)
- ❌ Hardcoded colors in test fixtures (ACCEPTABLE)
- ❌ TODO comments explaining future enhancements (NON-BLOCKING)
- ❌ P/Invoke in platform-specific abstraction layers (CORRECT PATTERN)

### False Negatives

- ⚠️ Methods that throw NotImplementedException
- ⚠️ Tests that pass but don't actually test requirements
- ⚠️ Requirements "implemented" but not connected to UI
- ⚠️ Incomplete async patterns (missing CancellationToken)

## Future Enhancements

- [ ] HTML report generation
- [ ] Code coverage integration (dotnet-coverage)
- [ ] Performance profiling automation
- [ ] GitHub PR comment generation
- [ ] Slack/Teams notification integration
- [ ] Historical trend analysis
- [ ] Automated remediation suggestions
- [ ] Visual diff of architecture vs implementation

## Support

For issues or questions about validation:
1. Check `.github/prompts/validate-implementation.prompt.md` for detailed guidance
2. Review validation reports from previous features
3. Consult `.specify/memory/constitution.md` for compliance rules
4. Contact project maintainers

## Summary

The validation system provides:
- ✅ Automated quality gates before merge
- ✅ Constitutional compliance enforcement
- ✅ Comprehensive specification verification
- ✅ Actionable remediation guidance
- ✅ CI/CD integration support
- ✅ Consistent quality standards

**Result**: Higher quality, fewer regressions, faster reviews, confident merges.
