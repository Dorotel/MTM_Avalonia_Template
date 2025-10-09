# Reference: Constitutional Requirements

**Date**: October 8, 2025

**Purpose**: Checklist of Constitution principles to validate against during Feature 005 implementation

**Constitution Version**: 1.1.0

---

## Principle I: Spec-Driven Development

**Rule**: All features must follow the Spec-Kit workflow (SPEC → PLAN → TASKS → Implementation).

### Feature 005 Compliance Checklist

- [ ] `SPEC_005.md` created with functional requirements
- [ ] `PLAN_005.md` created with technical architecture
- [ ] `TASKS_005.md` created with granular task breakdown
- [ ] Each task has acceptance criteria
- [ ] Implementation follows task order
- [ ] Tasks marked completed as work progresses
- [ ] Validation script passes before PR

**Validation**: Run `.specify/scripts/powershell/validate-implementation.ps1`

---

## Principle II: Nullable Reference Types Everywhere

**Rule**: All projects must have `<Nullable>enable</Nullable>` in `.csproj` files.

### Feature 005 Compliance Checklist

- [ ] All new C# files explicitly annotate nullable types with `?`
- [ ] No use of `!` null-forgiving operator without clear justification
- [ ] Constructor parameters validated with `ArgumentNullException.ThrowIfNull()`
- [ ] All async methods include `CancellationToken` parameter
- [ ] Use null-conditional operators (`?.`, `??`, `??=`) instead of explicit null checks

**Example**:

```csharp
public async Task<User?> GetUserAsync(string? userId, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(userId);
    return await _repository.FindAsync(userId, cancellationToken);
}
```

---

## Principle III: CompiledBinding Everywhere in AXAML

**Rule**: All XAML files must use `x:DataType` and `{CompiledBinding}` syntax.

### Feature 005 Compliance Checklist

- [ ] All new `.axaml` files have `x:DataType` attribute on root element
- [ ] All bindings use `{CompiledBinding}` syntax (never `{Binding}`)
- [ ] `x:CompileBindings="True"` set on all root elements
- [ ] `Design.DataContext` included for previewer support
- [ ] No use of `{ReflectionBinding}` (unless absolutely necessary with justification)

**Example**:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Template_Application.ViewModels"
        x:Class="MTM_Template_Application.Views.SettingsWindow"
        x:DataType="vm:SettingsViewModel"
        x:CompileBindings="True">
    <Design.DataContext>
        <vm:SettingsViewModel />
    </Design.DataContext>
    <!-- Content -->
</Window>
```

---

## Principle IV: Test-Driven Development (TDD)

**Rule**: Write tests BEFORE implementation. Minimum 80% code coverage for critical paths.

### Feature 005 Compliance Checklist

- [ ] Unit tests written for all ViewModels
- [ ] Unit tests written for all custom controls
- [ ] Integration tests written for configuration persistence
- [ ] Integration tests written for Visual API integration
- [ ] Contract tests written for Visual API endpoints
- [ ] Tests use xUnit + FluentAssertions + NSubstitute
- [ ] Tests follow AAA pattern (Arrange, Act, Assert)
- [ ] All tests passing before PR
- [ ] Code coverage >80% for critical paths

**Test Structure**:

```
tests/
├── unit/
│   ├── ViewModels/SettingsViewModelTests.cs
│   └── Controls/StatusCardTests.cs
├── integration/
│   ├── ConfigurationPersistenceTests.cs
│   └── VisualApiIntegrationTests.cs
└── contract/
    └── VisualApiContractTests.cs
```

---

## Principle V: Performance Budgets Are Non-Negotiable

**Rule**: Performance budgets must be met for all features.

### Feature 005 Performance Budgets

| Component                    | Budget     | Measurement                       |
| ---------------------------- | ---------- | --------------------------------- |
| Settings screen load         | <500ms     | Time from open to render complete |
| Settings save                | <1s        | Time from save click to persisted |
| Visual API call              | <2s        | Single API request round-trip     |
| Offline mode activation      | <100ms     | Switch to offline mode            |
| Custom control render        | <16ms      | Single frame render (60 FPS)      |
| Configuration retrieval      | <100ms     | `GetValue<T>()` call              |
| Database query               | <500ms     | User preferences query            |

### Performance Testing

- [ ] Performance tests written for all budgets
- [ ] Tests located in `tests/integration/PerformanceTests.cs`
- [ ] Tests use `System.Diagnostics.Stopwatch` for measurement
- [ ] Tests fail if budget exceeded

**Example**:

```csharp
[Fact]
public async Task SettingsLoad_ShouldComplete_WithinBudget()
{
    var stopwatch = Stopwatch.StartNew();
    await _viewModel.LoadSettingsAsync(CancellationToken.None);
    stopwatch.Stop();

    stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
}
```

---

## Principle VI: Error Categorization and Recovery

**Rule**: All errors must be categorized and handled with appropriate recovery strategies.

### Feature 005 Compliance Checklist

- [ ] All exceptions caught and logged with Serilog
- [ ] Error severity assigned (Info, Warning, Critical)
- [ ] User-friendly error messages (no stack traces in UI)
- [ ] Recovery options presented for critical errors
- [ ] Graceful degradation when dependencies unavailable

**Error Severity**:

- **Info**: Status bar notification only
- **Warning**: Status bar with click-for-details
- **Critical**: Modal dialog blocking interaction

**Example**:

```csharp
try
{
    await _configurationService.SaveUserPreferenceAsync(key, value, cancellationToken);
}
catch (MySqlException ex)
{
    _logger.LogWarning(ex, "Database unavailable - caching preference locally");
    // Gracefully degrade to local cache
    CachePreferenceLocally(key, value);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to save preference {Key}", key);
    ShowErrorDialog("Unable to save setting. Please try again.");
}
```

---

## Principle VII: Secrets Never Touch Code or Config Files

**Rule**: All credentials stored in OS-native secure storage.

### Feature 005 Compliance Checklist

- [ ] Visual API credentials stored via `ISecretsService`
- [ ] Database credentials stored via `ISecretsService`
- [ ] No credentials in `appsettings.json` or environment variables
- [ ] No credentials logged (use `***FILTERED***` in logs)
- [ ] Credential recovery flow implemented for storage failures

**Example**:

```csharp
// Store credential
await _secretsService.StoreSecretAsync("Visual.ApiKey", apiKey, cancellationToken);

// Retrieve credential
var apiKey = await _secretsService.RetrieveSecretAsync("Visual.ApiKey", cancellationToken);
```

---

## Principle VIII: Graceful Degradation Always

**Rule**: Application continues operating when dependencies are unavailable.

### Feature 005 Compliance Checklist

- [ ] Settings screen loads even if database unavailable
- [ ] Visual integration continues with cached data if API unavailable
- [ ] Offline mode activates automatically when network unavailable
- [ ] User preferences persist locally if database unavailable
- [ ] Feature flags use cached values if database unavailable

**Offline Capabilities**:

- Configuration: Environment variables → User config (cached) → Defaults
- User Preferences: Last-known cached values (local JSON)
- Feature Flags: Last-known cached values (local JSON)
- Visual Data: Cached API responses (LZ4 compressed)

---

## Principle IX: Logging Is Structured and Contextual

**Rule**: Use structured logging with Serilog. No `Console.WriteLine()`.

### Feature 005 Compliance Checklist

- [ ] All logging uses `ILogger<T>` interface
- [ ] Log messages use structured format: `_logger.LogInformation("Message with {Parameter}", value)`
- [ ] No string interpolation in log messages
- [ ] No `Console.WriteLine()` calls
- [ ] Correlation IDs included for distributed operations
- [ ] Sensitive data filtered from logs

**Example**:

```csharp
_logger.LogInformation(
    "User {UserId} updated setting {Key} from {OldValue} to {NewValue}",
    userId, key, oldValue, newValue
);
```

---

## Principle X: Dependency Injection (DI) Everywhere

**Rule**: All dependencies injected via constructor. No `new` operator for services.

### Feature 005 Compliance Checklist

- [ ] All ViewModels registered in `Program.cs` DI container
- [ ] All services registered in `Program.cs` DI container
- [ ] Constructor injection used exclusively
- [ ] Interfaces used for testability
- [ ] No service locator pattern

**Example**:

```csharp
// Program.cs
services.AddTransient<SettingsViewModel>();
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<IVisualApiClient, VisualApiClient>();

// SettingsViewModel.cs
public SettingsViewModel(
    ILogger<SettingsViewModel> logger,
    IConfigurationService configurationService,
    IVisualApiClient visualApiClient)
{
    ArgumentNullException.ThrowIfNull(logger);
    ArgumentNullException.ThrowIfNull(configurationService);
    ArgumentNullException.ThrowIfNull(visualApiClient);

    _logger = logger;
    _configurationService = configurationService;
    _visualApiClient = visualApiClient;
}
```

---

## Principle XI: Reusable Custom Controls (3+ Rule)

**Rule**: Extract to custom control when pattern occurs 3+ times across codebase.

### Feature 005 Compliance Checklist

- [ ] All repeated XAML patterns (3+ occurrences) extracted to custom controls
- [ ] Custom controls use `PurposeTypeControl` naming convention
- [ ] Custom controls documented in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
- [ ] Custom controls have unit tests (property validation)
- [ ] Custom controls have visual tests (previewer)

**Custom Controls to Extract**:

1. **StatusCard** (15+ occurrences in DebugTerminalWindow)
2. **MetricDisplay** (20+ occurrences in DebugTerminalWindow)
3. **ErrorListPanel** (5+ occurrences in DebugTerminalWindow)
4. **ConnectionHealthBadge** (8+ occurrences in DebugTerminalWindow)
5. **BootTimelineChart** (3+ occurrences in DebugTerminalWindow)

**New Controls to Create**:

1. **SettingsCategory** (Settings screen)
2. **SettingRow** (Settings screen)
3. **NavigationMenuItem** (Debug Terminal rewrite)
4. **ActionButtonGroup** (Debug Terminal rewrite)
5. **ConfigurationErrorDialog** (MainWindow TODO)

---

## Constitution TODOs (Feature 005 Responsibilities)

### Constitution.md Line 34-37 TODOs

- [ ] Create `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with complete catalog
- [ ] Document manufacturing field controls pattern
- [ ] Add custom control examples to quickstart guide
- [ ] Update Constitution with custom control catalog reference

---

## Feature 001 TODOs (Boot Sequence)

**None identified** - Feature 001 complete.

---

## Feature 002 TODOs (Configuration & Secrets)

**None identified** - Feature 002 complete.

---

## Feature 003 TODOs (Debug Terminal)

### DebugTerminalViewModel.cs TODOs

- [ ] Implement `CopyToClipboardCommand` with actual clipboard integration
- [ ] Implement `IsMonitoring` toggle with performance snapshot collection
- [ ] Implement environment variables display with filtering

### MainWindow.axaml TODO

- [ ] Implement `ConfigurationErrorDialog` modal for configuration errors

---

## Radio Silence Mode Compliance

**User selected**: Radio Silence Mode for Feature 005 implementation

### Radio Silence Requirements

- [ ] Pre-implementation plan posted with timebox estimate
- [ ] User approval obtained before entering silence
- [ ] No commentary during implementation (deliverables only)
- [ ] Output format: PATCH/NEW FILE/DELETE FILE/TEST/COMMIT
- [ ] Repository standards enforced at all times
- [ ] Exit protocol: SUMMARY, CHANGES, TESTS, NEXT

---

## Validation Checklist Summary

Before PR submission:

1. [ ] Run `dotnet build MTM_Template_Application.sln` (zero errors/warnings)
2. [ ] Run `dotnet test MTM_Template_Application.sln` (all passing)
3. [ ] Run `.specify/scripts/powershell/validate-implementation.ps1` (all tasks completed)
4. [ ] Run `.specify/scripts/powershell/constitutional-audit.ps1` (all principles satisfied)
5. [ ] Code coverage >80% for critical paths
6. [ ] Performance budgets met (run performance tests)
7. [ ] All custom controls documented in catalog
8. [ ] All Constitution TODOs completed
9. [ ] All Feature 003 TODOs completed
10. [ ] User acceptance testing completed

---

## Constitutional Violations to Avoid

| Violation                                    | Impact      | Prevention                                    |
| -------------------------------------------- | ----------- | --------------------------------------------- |
| Missing `x:DataType` in XAML                 | Build error | Always use CompiledBinding pattern            |
| Missing `?` for nullable reference types     | Warning     | Enable nullable reference types everywhere    |
| Using `!` null-forgiving operator            | Runtime NRE | Use null checks or `?.` operator              |
| Missing `CancellationToken` in async methods | Poor UX     | Always include cancellation support           |
| Hardcoded credentials in config              | Security    | Use ISecretsService for all credentials       |
| <80% test coverage on critical paths         | Fragility   | Write tests before implementation (TDD)       |
| Performance budget exceeded                  | User impact | Run performance tests regularly               |
| Custom control not documented                | Maintenance | Update catalog immediately after creation     |
| Repeated XAML pattern (3+ occurrences)       | Duplication | Extract to custom control immediately         |

---

## Post-Implementation Constitutional Audit

After Feature 005 completion, run:

```powershell
.\.specify\scripts\powershell\constitutional-audit.ps1 -FeatureId "005-migrate-infor-visual"
```

**Expected Output**:

```
✓ Principle I: Spec-Driven Development - PASS
✓ Principle II: Nullable Reference Types - PASS
✓ Principle III: CompiledBinding Everywhere - PASS
✓ Principle IV: Test-Driven Development - PASS (Coverage: 85%)
✓ Principle V: Performance Budgets - PASS
✓ Principle VI: Error Categorization - PASS
✓ Principle VII: Secrets Never Touch Code - PASS
✓ Principle VIII: Graceful Degradation - PASS
✓ Principle IX: Structured Logging - PASS
✓ Principle X: Dependency Injection - PASS
✓ Principle XI: Reusable Custom Controls - PASS (10 controls created)

Constitutional Compliance: 100%
```
