# Constitutional Remediation Checklist - v1.1.0
**Feature**: [FEATURE NAME]
**Remediation Date**: [DATE]
**Audit Reference**: [AUDIT_FILE_NAME]
**Status**: 🔴 NOT STARTED

**Progress**: 0 / [TOTAL] items complete (0%)
- BLOCKING: 0 / [BLOCKING_COUNT] complete
- Non-Blocking: 0 / [NON_BLOCKING_COUNT] complete

---

## 📋 Quick Reference

| REM ID | Description | Task ID | Time | Status |
|--------|-------------|---------|------|--------|
| REM001 | [Brief desc] | T### | 15m | ⬜ |
| REM002 | [Brief desc] | T### | 2h | ⬜ |

---

## 🔴 BLOCKING Remediations (Must Complete Before Merge)

### REM001: [Violation Title]
- **Related Task**: T### ([Task description from tasks.md])
- **Principle Violated**: [Principle I-VII or Async/Await Pattern]
- **Severity**: BLOCKING
- **Estimated Time**: [X] minutes/hours

#### Files to Modify
- `path/to/file.ext:line`

#### Current Code (Before)
```language
// Exact current code that violates the principle
```

#### Required Code (After)
```language
// Exact code that fixes the violation
```

#### Verification Steps
1. Run: `command to verify fix`
2. Expected: [What should happen]
3. Test: `dotnet test --filter "TestName"`

#### Git Commit
```bash
git add path/to/file.ext
git commit -m "fix(scope): brief description per Principle X"
```

#### Dependencies
- [ ] None (can start immediately)
- [ ] Requires: REM### to be completed first

#### Checklist
- [ ] Code changed to match "After" pattern
- [ ] Verification command passes
- [ ] No new errors introduced
- [ ] Related task T### marked complete in tasks.md
- [ ] Git commit completed

---

## ⚠️ Non-Blocking Remediations (Can Fix in Follow-Up)

### REM999: [Violation Title]
- **Related Task**: T### ([Task description from tasks.md])
- **Principle Violated**: [Principle I-VII]
- **Severity**: WARNING
- **Estimated Time**: [X] minutes/hours

[Same structure as BLOCKING items]

---

## 📊 Progress Tracking

### Completion Status by Category

**BLOCKING Items**:
- [ ] REM001 - [Brief description]
- [ ] REM002 - [Brief description]

**Non-Blocking Items**:
- [ ] REM999 - [Brief description]

### Time Tracking
- **Estimated Total Time**: [X] hours
  - BLOCKING: [Y] hours
  - Non-Blocking: [Z] hours
- **Actual Time Spent**: [To be tracked during implementation]
- **Time Remaining**: [Estimated - Actual]

---

## 🔄 Task Synchronization

When marking REM items complete, also update tasks.md:

| REM ID | Related Task | tasks.md Line | Action |
|--------|--------------|---------------|--------|
| REM001 | T125 | Line 215 | Change `[ ]` to `[x]` |
| REM002 | T039 | Line 89 | Change `[ ]` to `[x]` |

---

## 📝 Implementation Notes

### Common Patterns Used

#### Pattern: Adding CancellationToken to Async Methods
```csharp
// Before:
Task<Result> DoWorkAsync();

// After:
Task<Result> DoWorkAsync(CancellationToken cancellationToken = default);

// Don't forget to:
// 1. Update interface first
// 2. Update all implementations
// 3. Propagate token to downstream calls
// 4. Update tests to pass CancellationToken.None
```

#### Pattern: Changing XAML Bindings to CompiledBinding
```xml
<!-- Before: -->
<TextBlock Text="{Binding PropertyName}" />

<!-- After: -->
<!-- 1. Ensure x:DataType exists on root element -->
<!-- 2. Change Binding to CompiledBinding -->
<TextBlock Text="{CompiledBinding PropertyName}" />
```

#### Pattern: Registering Services in DI Container
```csharp
// In Program.cs or MainActivity.cs:
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .ConfigureServices(services =>
        {
            // Register in order: Dependencies first, dependents second
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
        });
}
```

---

## ✅ Completion Criteria

This remediation is complete when:
- [ ] All BLOCKING items marked `[x]`
- [ ] All BLOCKING verification commands pass
- [ ] `dotnet build` produces no warnings
- [ ] `dotnet test` passes all tests
- [ ] Corresponding tasks.md items marked `[x]`
- [ ] Re-run constitutional audit shows COMPLIANT status

---

## 🔗 Related Documents

- **Audit**: [AUDIT_FILE_NAME] - Original violation analysis
- **Tasks**: specs/[FEATURE]/tasks.md - Feature task list
- **Spec**: specs/[FEATURE]/spec.md - Feature specification
- **Plan**: specs/[FEATURE]/plan.md - Implementation plan
- **Constitution**: .specify/memory/constitution.md - Project principles

---

## 📞 Getting Help

If you encounter issues during remediation:

1. **Check the constitution**: `.specify/memory/constitution.md` for principle details
2. **Review the audit**: Original audit has detailed analysis and context
3. **Search for patterns**: `.github/copilot-instructions.md` has code examples
4. **Ask for clarification**: Use `/clarify` command if requirements unclear

---

**Generated**: [DATE] | **Last Updated**: [DATE] | **Version**: 1.0
