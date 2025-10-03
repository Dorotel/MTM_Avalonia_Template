# Constitutional Compliance Audit - v1.1.0
**Feature**: [FEATURE NAME]
**Audit Date**: [DATE]
**Auditor**: @Dorotel
**Status**: [✅ COMPLIANT / ⚠️ PARTIAL COMPLIANCE / ❌ NON-COMPLIANT]

---

## 🎯 Next Steps (Action Required)

### BLOCKING Issues (Must Complete Before Merge):

#### 1. **[PRIORITY]** [Issue Title]
- **File**: [File path]
- **Action**: [What needs to be done]
- **Verification**: [How to verify the fix]
- **Why Critical**: [Which principle/requirement violated]

### Non-Blocking Issues (Can Address in Follow-Up):

#### 2. **[TIME ESTIMATE]** [Issue Title]
- **File**: [File path]
- **Action**: [What needs to be done]
- **Verification**: [How to verify the fix]

### Estimated Total Remediation Time
- **Critical Fixes**: [X] hours
- **Non-Critical Fixes**: [Y] hours
- **Total**: [X+Y] hours

---

## Principle I: Cross-Platform First
- [ ] All code uses platform abstractions (no direct P/Invoke)
- [ ] Platform-specific code behind interfaces
- [ ] Shared logic in `.Core` project
- [ ] Uses `RuntimeInformation.IsOSPlatform()` for detection

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle II: MVVM Community Toolkit Standard
- [ ] All ViewModels inherit from `ObservableObject`
- [ ] Properties use `[ObservableProperty]`
- [ ] Commands use `[RelayCommand]`
- [ ] NO ReactiveUI patterns (ReactiveObject, ReactiveCommand, etc.)
- [ ] Uses `[NotifyCanExecuteChangedFor]` for dependent commands

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle III: Test-First Development (TDD)
- [ ] Tests written and approved BEFORE implementation
- [ ] All tests use **xUnit** framework
- [ ] Integration tests for workflows exist
- [ ] Unit tests for ViewModels exist
- [ ] Mocks use **NSubstitute**
- [ ] >80% code coverage on critical paths

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle IV: Theme V2 Semantic Tokens
- [ ] All styles use `{DynamicResource}` tokens
- [ ] No hardcoded colors/values
- [ ] Styles in separate `.axaml` files
- [ ] Base theme is FluentTheme or Material.Avalonia

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle V: Null Safety and Error Resilience
- [ ] Nullable reference types enabled
- [ ] `ArgumentNullException.ThrowIfNull()` used
- [ ] Error boundaries in ViewModels (try-catch with logging)
- [ ] Serilog structured logging used
- [ ] Graceful offline degradation

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle VI: Compiled Bindings Only (CRITICAL)
- [ ] All XAML files have `x:DataType` attribute
- [ ] All XAML files have `x:CompileBindings="True"`
- [ ] All bindings use `{CompiledBinding}` syntax
- [ ] NO legacy `{Binding}` syntax without `x:CompileBindings`
- [ ] `Design.DataContext` set for previewer

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Principle VII: Dependency Injection via AppBuilder
- [ ] Services registered in `Program.cs`
- [ ] ViewModels use constructor injection
- [ ] NO service locator pattern
- [ ] NO static service access

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Async/Await Patterns
- [ ] All async methods have `CancellationToken` parameter
- [ ] Methods suffixed with `Async`
- [ ] `ConfigureAwait(false)` in library code (NOT UI code)
- [ ] Cancellation propagated to downstream calls

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Spec-Kit Integration
- [ ] Feature has SPEC_*.md file
- [ ] Feature has PLAN_*.md file
- [ ] Feature has TASKS_*.md file
- [ ] Scripts output parsed correctly
- [ ] Git branch follows naming convention

**Violations Found**: [NONE / LIST SPECIFIC FILES AND LINES]
**Remediation Plan**: [IF VIOLATIONS FOUND, DESCRIBE REMEDIATION STEPS]

---

## Summary
**Total Violations**: [NUMBER]
**Critical Violations** (must fix before proceeding): [NUMBER]
**Non-Critical Violations** (can fix later): [NUMBER]

**Next Steps**:
1. [ACTION ITEM 1]
2. [ACTION ITEM 2]
3. [ACTION ITEM 3]

**Estimated Remediation Time**: [NUMBER] hours
