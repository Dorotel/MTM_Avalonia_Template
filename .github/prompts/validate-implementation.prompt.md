---
description: Deep validation of completed feature implementation against specification and constitution
---

# Validate Implementation Command

**Purpose**: Perform comprehensive validation when a feature reaches 100% implementation (all tasks in tasks.md marked complete). Ensures implementation matches specification and maintains constitutional compliance.

**Input**: Feature ID (e.g., `001-boot-sequence-splash`) or current branch

**Prerequisites**:
- Feature has `spec.md`, `plan.md`, `tasks.md` files
- All tasks in `tasks.md` marked as `[x]` complete
- Implementation code exists
- Constitution v1.1.0 available

## Execution Flow

```
1. Identify and validate target feature
   → From argument OR current branch OR latest feature
   → Verify tasks.md exists
   → Count tasks: total vs completed
   → If < 100% complete: WARN but continue
   → If not found: ERROR "No feature specified"

2. Load complete feature context
   → Read specs/[FEATURE]/spec.md (functional requirements)
   → Read specs/[FEATURE]/plan.md (technical architecture)
   → Read specs/[FEATURE]/tasks.md (implementation checklist)
   → Read specs/[FEATURE]/data-model.md (if exists)
   → Read specs/[FEATURE]/quickstart.md (validation scenarios)
   → Read .specify/memory/constitution.md (compliance rules)
   → Scan clarify/ folder for decision context

3. Create validation report structure
   → Copy validate-implementation.md template
   → Save as VALIDATION_[FEATURE]_[DATE].md in specs/[FEATURE]/
   → Fill metadata: Feature name, date, validator, completion %

4. Phase 1: Task Completion Verification (CRITICAL)
   → Parse tasks.md for all task IDs and descriptions
   → For each task:
     a. Extract file paths from task description
     b. Verify files exist at specified paths
     c. Check file content matches task intent
     d. Verify no placeholder/TODO comments remain
     e. Check tests exist for implementation tasks
   → Document missing implementations
   → Document incomplete implementations (TODOs, NotImplementedException)

5. Phase 2: Functional Requirements Mapping
   → Parse spec.md for all FR-XXX requirements
   → For each requirement:
     a. Find corresponding tasks in tasks.md
     b. Verify tasks completed
     c. Locate implementation in codebase
     d. Check implementation logic matches requirement
     e. Verify test coverage exists
   → Document unmapped requirements (no implementation)
   → Document untested requirements

6. Phase 3: Constitutional Compliance Audit
   → Run all constitutional checks from constitutional-audit.prompt.md:

   **I. Cross-Platform First** ✓
   - No direct P/Invoke without abstraction
   - Platform detection using RuntimeInformation
   - Platform-specific code properly isolated

   **II. MVVM Community Toolkit** ✓
   - No ReactiveUI patterns (ReactiveObject, ReactiveCommand)
   - All ViewModels use [ObservableObject]
   - Properties use [ObservableProperty]
   - Commands use [RelayCommand]

   **III. Test-First Development** ✓
   - xUnit + FluentAssertions + NSubstitute
   - Tests written before implementation (git history check)
   - >80% code coverage (if measurable)
   - Contract tests for external APIs
   - Integration tests for boot scenarios

   **IV. Theme V2 Semantic Tokens** ✓
   - No hardcoded colors (#RRGGBB) in XAML (except splash screen)
   - All bindings use {DynamicResource ThemeV2.*}
   - Separate style files (.axaml)

   **V. Null Safety** ✓
   - <Nullable>enable</Nullable> in all .csproj files
   - ArgumentNullException.ThrowIfNull() for public APIs
   - Proper nullable annotations (?, !)

   **VI. Compiled Bindings (CRITICAL)** ✓✓✓
   - All .axaml files have x:CompileBindings="True"
   - All bindings use {CompiledBinding ...} syntax
   - All root elements have x:DataType="..."
   - NO {Binding} without x:CompileBindings (except ItemTemplate DataTemplate contexts)

   **VII. Dependency Injection** ✓
   - Services registered in Program.cs via AppBuilder
   - Constructor injection throughout
   - No service locator pattern
   - No static service access

7. Phase 4: Architecture Consistency Check
   → Verify plan.md architecture implemented:
     a. All documented services exist
     b. All documented models exist
     c. Folder structure matches plan
     d. Dependencies match package list
     e. Entry points configured correctly
   → Check for architectural drift (unplanned additions)
   → Verify separation of concerns (Services/, Models/, ViewModels/, Views/)

8. Phase 5: Data Model Validation
   → If data-model.md exists:
     a. Verify all entities implemented
     b. Check all properties present
     c. Verify relationships implemented
     d. Check validation rules applied
     e. Confirm state transitions handled
   → Document missing entities/properties
   → Verify entity usage in services

9. Phase 6: Test Execution and Coverage
   → Run all tests: dotnet test
   → Verify test results:
     a. All tests pass (0 failures)
     b. No skipped tests (unless documented)
     c. Performance tests meet targets
     d. Integration tests validate scenarios
   → Check test coverage (if tool available):
     a. Overall coverage >80%
     b. Critical paths >90%
     c. Service logic >85%
   → Document test gaps

10. Phase 7: Build and Runtime Validation
    → Build solution: dotnet build
    → Verify clean build (0 errors, 0 warnings)
    → Check for suppressed warnings
    → Run desktop application (smoke test)
    → Run Android application if applicable (smoke test)
    → Verify no startup crashes
    → Verify boot sequence completes successfully

11. Phase 8: Code Quality Assessment
    → Check for code smells:
      - God classes (>500 lines)
      - Long methods (>100 lines)
      - Duplicate code
      - Unused imports
      - Dead code
      - Magic numbers/strings
    → Verify logging patterns consistent
    → Check error handling comprehensive
    → Verify async/await patterns correct:
      - All async methods have CancellationToken parameter
      - Method names end with Async
      - ConfigureAwait(false) in library code only
      - No .Result or .Wait() blocking calls

12. Phase 9: Documentation Validation
    → Verify documentation completeness:
      a. spec.md exists and complete
      b. plan.md exists and matches implementation
      c. tasks.md exists with all tasks checked
      d. quickstart.md exists with validation scenarios
      e. how-to-use.md exists (if applicable)
      f. README.md updated with feature
      g. TROUBLESHOOTING.md updated (if issues documented)
    → Check inline code documentation:
      - Public APIs have XML comments
      - Complex logic has explanatory comments
      - No misleading comments

13. Phase 10: Security and Performance Review
    → Security checks:
      - No hardcoded credentials
      - No sensitive data in logs
      - Proper input validation
      - SQL injection prevention (parameterized queries)
      - XSS prevention (if applicable)
    → Performance checks:
      - No N+1 query patterns
      - Proper connection pooling
      - Async operations don't block threads
      - Memory leaks checked (if tools available)
      - Performance targets met (from spec.md)

14. Generate Validation Summary
    → Calculate completion metrics:
      - Tasks completed: X / Y (Z%)
      - Requirements implemented: X / Y (Z%)
      - Tests passing: X / Y (Z%)
      - Constitutional violations: X (Critical: Y, Non-Critical: Z)
      - Code coverage: X% (if available)
    → Overall status: PASS / CONDITIONAL PASS / FAIL

    **PASS**: All checks green, ready for merge
    **CONDITIONAL PASS**: Minor non-blocking issues, can merge with follow-up tasks
    **FAIL**: Critical violations exist, cannot merge

15. Generate Remediation Plan (if issues found)
    → List all blocking issues with:
      - Issue ID
      - Severity (Critical/High/Medium/Low)
      - Description
      - File path and line number
      - Expected vs Actual
      - Remediation steps
      - Estimated time to fix
    → Total estimated remediation hours
    → Recommended next steps

16. Output validation report
    → Display summary in terminal
    → Save full report to specs/[FEATURE]/VALIDATION_[FEATURE]_[DATE].md
    → Return JSON with validation results
```

## Output Format

```json
{
  "VALIDATION_FILE": "specs/001-boot-sequence-splash/VALIDATION_001-boot-sequence-splash_2025-10-05.md",
  "FEATURE_ID": "001-boot-sequence-splash",
  "COMPLETION_PERCENTAGE": 100,
  "TASKS_COMPLETED": 175,
  "TASKS_TOTAL": 175,
  "REQUIREMENTS_IMPLEMENTED": 160,
  "REQUIREMENTS_TOTAL": 160,
  "TESTS_PASSED": 311,
  "TESTS_TOTAL": 311,
  "CONSTITUTIONAL_VIOLATIONS": {
    "CRITICAL": 0,
    "NON_CRITICAL": 3
  },
  "CODE_COVERAGE_PERCENT": 87.5,
  "BUILD_STATUS": "SUCCESS",
  "OVERALL_STATUS": "PASS",
  "BLOCKING_ISSUES": 0,
  "NON_BLOCKING_ISSUES": 3,
  "ESTIMATED_REMEDIATION_HOURS": 2,
  "READY_FOR_MERGE": true
}
```

## Validation Thresholds

### PASS Criteria (Ready for Merge)
- ✅ 100% of tasks completed
- ✅ 100% of critical requirements implemented
- ✅ 100% of tests passing
- ✅ 0 critical constitutional violations
- ✅ 0 blocking issues
- ✅ Clean build (0 errors)
- ✅ Smoke tests pass
- ✅ Code coverage >80% (or >60% with justification)

### CONDITIONAL PASS Criteria (Merge with Follow-up)
- ✅ 95-99% of tasks completed (minor polish remaining)
- ✅ 100% of critical requirements implemented
- ✅ >95% of tests passing
- ✅ 0 critical constitutional violations
- ✅ <5 non-critical violations
- ✅ Clean build
- ⚠️ Minor documentation gaps
- ⚠️ Code coverage 60-80%

### FAIL Criteria (Cannot Merge)
- ❌ <95% of tasks completed
- ❌ Critical requirements missing
- ❌ Tests failing
- ❌ Any critical constitutional violations
- ❌ Build errors
- ❌ Runtime crashes
- ❌ Blocking security issues
- ❌ Code coverage <60%

## Usage Examples

```powershell
# Validate current branch (auto-detect feature)
./validate-implementation.ps1

# Validate specific feature
./validate-implementation.ps1 001-boot-sequence-splash

# Validate with JSON output
./validate-implementation.ps1 --json 001-boot-sequence-splash

# Validate and create GitHub PR comment
./validate-implementation.ps1 --pr-comment 001-boot-sequence-splash

# Strict mode (fail on any warnings)
./validate-implementation.ps1 --strict 001-boot-sequence-splash

# Generate detailed HTML report
./validate-implementation.ps1 --html 001-boot-sequence-splash
```

## AI Agent Instructions

When performing validation:

### 1. Be Methodical
- Follow the 16-phase process in order
- Don't skip phases even if previous ones pass
- Document everything, even small issues

### 2. Be Thorough
- Check EVERY file mentioned in tasks.md
- Verify EVERY requirement in spec.md
- Test EVERY scenario in quickstart.md
- Audit EVERY constitutional principle

### 3. Be Specific
- Provide exact file paths for all issues
- Include line numbers for code violations
- Quote relevant spec text when flagging gaps
- Show expected vs actual for discrepancies

### 4. Be Fair
- Distinguish critical vs non-critical issues
- Consider context (test fixtures may use hardcoded values)
- Allow documented deviations (with justification)
- Recognize patterns vs isolated issues

### 5. Be Actionable
- Every issue must have remediation steps
- Estimate time realistically
- Prioritize blocking issues first
- Provide code examples for fixes when helpful

### 6. Be Constructive
- Highlight what's done well
- Explain WHY violations matter
- Suggest improvements, not just criticisms
- Acknowledge complexity

## Critical Validation Patterns

### Task-to-Code Mapping

```
Task: "T080 ConfigurationService implementation in MTM_Template_Application/Services/Configuration/ConfigurationService.cs"

Verify:
1. File exists at exact path
2. Class named ConfigurationService
3. Implements IConfigurationService interface
4. Has required methods from spec.md
5. No TODO or NotImplementedException
6. Has corresponding tests
7. Registered in DI container
```

### Requirement-to-Implementation Mapping

```
Requirement: "FR-001: Application SHALL display splash screen immediately on launch"

Verify:
1. Find tasks implementing FR-001 in tasks.md
2. Locate SplashWindow.axaml and SplashViewModel
3. Check App.axaml.cs or Program.cs shows splash first
4. Verify no blocking operations before splash
5. Find test validating immediate splash display
6. Check quickstart.md scenario covers this
```

### XAML Binding Validation (CRITICAL)

```
For EVERY .axaml file:
1. Has x:CompileBindings="True" at root
2. Has x:DataType="vm:SomeViewModel" at root
3. Every {Binding ...} is {CompiledBinding ...}
   EXCEPTION: ItemTemplate DataTemplate children (context switches to item)
4. No ReflectionBinding syntax
5. Design.DataContext set for previewer
```

### Async Pattern Validation

```
For EVERY public async method:
1. Name ends with Async
2. Has CancellationToken parameter (or documented why not)
3. Passes cancellation token to nested async calls
4. No .Result or .Wait() calls
5. Handles OperationCanceledException appropriately
6. No async void (except event handlers)
```

### Test Coverage Validation

```
For EVERY service class:
1. Has corresponding *Tests.cs file
2. Tests cover public API surface
3. Tests cover error paths
4. Tests cover boundary conditions
5. Integration tests exist for external dependencies
6. Performance tests exist if performance-critical
```

## Common Pitfalls to Check

### False Positives
- ❌ Flagging {Binding} in ItemTemplate children (valid context switch)
- ❌ Flagging hardcoded colors in test fixtures
- ❌ Flagging TODO in comments explaining future enhancements (not blockers)
- ❌ Flagging P/Invoke in platform-specific abstraction layer (correct pattern)

### False Negatives
- ⚠️ Missing async methods that should exist per spec
- ⚠️ Incomplete implementations (methods that throw NotImplementedException)
- ⚠️ Tests that pass but don't actually test the requirement
- ⚠️ Requirements "implemented" but not connected to UI/workflow

## Validation Report Template Structure

```markdown
# Implementation Validation Report

**Feature**: [Feature Name]
**Feature ID**: [ID]
**Branch**: [Branch Name]
**Date**: [ISO Date]
**Validator**: GitHub Copilot / [Human Name]

---

## Executive Summary

- **Overall Status**: [PASS / CONDITIONAL PASS / FAIL]
- **Completion**: [X]% ([Y]/[Z] tasks)
- **Tests**: [PASSING / FAILING] ([X]/[Y] tests)
- **Build**: [SUCCESS / FAILED]
- **Constitutional Compliance**: [X] violations ([Y] critical, [Z] non-critical)
- **Ready for Merge**: [YES / NO / WITH CONDITIONS]

### Quick Stats
| Metric | Status | Details |
|--------|--------|---------|
| Tasks Completed | [X]/[Y] | [Z]% complete |
| Requirements Implemented | [X]/[Y] | [Z]% coverage |
| Tests Passing | [X]/[Y] | [Z]% pass rate |
| Build Status | [SUCCESS/FAIL] | [X] errors, [Y] warnings |
| Code Coverage | [X]% | Target: >80% |
| Constitutional Violations | [X] | [Y] critical |

---

## Phase 1: Task Completion Verification

### Completed Tasks ([X]/[Y])
[List of all completed tasks with ✅]

### Incomplete Tasks ([X])
[List of incomplete tasks with ❌, reasons, and file paths]

### Tasks with Issues ([X])
[List of tasks marked complete but with problems]

---

## Phase 2: Functional Requirements Mapping

### Implemented Requirements ([X]/[Y])
[Table mapping FR-XXX to tasks to code files]

### Missing Requirements ([X])
[Requirements from spec.md not implemented]

### Partially Implemented Requirements ([X])
[Requirements with incomplete implementations]

---

## Phase 3: Constitutional Compliance Audit

[Full constitutional audit following constitutional-audit.prompt.md format]

---

## Phase 4-10: [Detailed Results for Each Phase]

---

## Blocking Issues ([X])

[List of critical issues preventing merge]

---

## Non-Blocking Issues ([X])

[List of minor issues that can be addressed in follow-up]

---

## Recommendations

1. [Prioritized list of actions]

---

## Appendices

### Appendix A: File Inventory
[Complete list of files created/modified]

### Appendix B: Test Results
[Detailed test output]

### Appendix C: Performance Metrics
[Boot time, memory usage, etc.]
```

## Notes

- Run this validation when tasks.md shows 100% completion
- Treat this as a "pre-merge gate" quality check
- Archive validation reports in specs/[FEATURE]/validations/
- Use validation results to update team's coding standards
- Failed validations should trigger remediation before re-validation
- Successful validations can be referenced in PR descriptions

---

*This validation ensures feature quality and constitutional compliance before merge.*
