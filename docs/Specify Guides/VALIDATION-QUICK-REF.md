# Validation Quick Reference

## When to Validate

✅ **Run validation when `tasks.md` shows 100% completion**

```powershell
# Quick validation
.\.specify\scripts\powershell\validate-implementation.ps1
```

## What It Checks

| Phase | What | Pass Criteria |
|-------|------|---------------|
| 1 | Task Completion | 100% tasks marked `[x]` |
| 2 | Requirements | All FR-XXX implemented |
| 3 | Constitution | 0 critical violations |
| 4 | Architecture | Matches plan.md |
| 5 | Data Model | All entities present |
| 6 | Tests | 100% passing |
| 7 | Build | Clean (0 errors) |
| 8 | Code Quality | No god classes, proper async |
| 9 | Documentation | Complete and accurate |
| 10 | Security | No vulnerabilities |

## Status Meanings

### ✅ PASS
- Ready for merge immediately
- All checks green
- 0 blocking issues

### ⚠️ CONDITIONAL PASS  
- Can merge with follow-up PR
- Minor issues documented
- <5 non-critical violations

### ❌ FAIL
- Cannot merge
- Blocking issues exist
- Fix and re-validate required

## Critical Violations (Auto-Fail)

1. ❌ Missing `{CompiledBinding}` in XAML
2. ❌ Missing `CancellationToken` in async methods
3. ❌ Using ReactiveUI (should use MVVM Toolkit)
4. ❌ SQL string concatenation
5. ❌ Build errors
6. ❌ Test failures

## Quick Commands

```powershell
# Validate current branch
.\.specify\scripts\powershell\validate-implementation.ps1

# Validate specific feature
.\.specify\scripts\powershell\validate-implementation.ps1 001-boot-sequence-splash

# JSON output for CI/CD
.\.specify\scripts\powershell\validate-implementation.ps1 -Json

# Strict mode (fail on warnings)
.\.specify\scripts\powershell\validate-implementation.ps1 -Strict
```

## Report Location

**Saved to**: `specs/[FEATURE]/VALIDATION_[FEATURE]_[DATE].md`

## Exit Codes

- `0` = Ready for merge ✅
- `1` = Validation failed ❌
- `2` = Script error ⚠️

## Common Fixes

### Missing CompiledBinding
```xml
<!-- ❌ Wrong -->
<TextBlock Text="{Binding UserName}" />

<!-- ✅ Correct -->
<TextBlock Text="{CompiledBinding UserName}" />
```

### Missing CancellationToken
```csharp
// ❌ Wrong
public async Task LoadDataAsync()

// ✅ Correct
public async Task LoadDataAsync(CancellationToken cancellationToken = default)
```

### Hardcoded Colors
```xml
<!-- ❌ Wrong -->
<Border Background="#FFFFFF" />

<!-- ✅ Correct -->
<Border Background="{DynamicResource ThemeV2.Surface.Background}" />
```

## More Info

See `docs/VALIDATION-SYSTEM.md` for comprehensive documentation.
