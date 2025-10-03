# Constitutional Compliance Audit - v1.1.0

**Feature**: 001-boot-sequence-splash (Boot Sequence — Splash-First, Services Initialization Order)
**Audit Date**: 2025-10-03
**Auditor**: GitHub Copilot (Constitutional Audit Agent)
**Status**: ⚠️ NON-COMPLIANT (Critical violations found)

---

## 🎯 Next Steps (Action Required)

### BLOCKING Issues (Must Complete Before Merge)

#### 1. **[CRITICAL - 15 min]** Fix XAML Binding Violations

- **File**: `MTM_Template_Application/Views/MainView.axaml`
- **Action**: Change line 15 from `{Binding Greeting}` to `{CompiledBinding Greeting}`
- **Verification**: `dotnet build` produces no warnings
- **Why Critical**: Violates Principle VI (Compiled Bindings Only)

#### 2. **[CRITICAL - 8-12 hours]** Add CancellationToken to All Async Methods

- **Files**: 40+ service interfaces and implementations
- **Action**: Add `CancellationToken cancellationToken = default` parameter to all async methods
- **Action**: Propagate cancellation to downstream calls
- **Verification**: Update all tests, run full test suite
- **Why Critical**: Violates Constitutional Async/Await Patterns requirement

### Non-Blocking Issues (Can Address in Follow-Up)

#### 3. **[2 hours]** Configure Dependency Injection in Program.cs

- **File**: `MTM_Template_Application.Desktop/Program.cs`
- **Action**: Add `.ConfigureServices()` with service registrations
- **Verification**: Integration test for DI container

#### 4. **[1 hour]** Create ViewModel Unit Tests

- **File**: Create `tests/unit/ViewModels/MainViewModelTests.cs`
- **Action**: Test property changes, command execution
- **Verification**: Run tests with `dotnet test`

#### 5. **[30 min]** Configure Code Coverage

- **Action**: Add Coverlet or dotnet-coverage to test project
- **Action**: Set up coverage reporting in CI/CD
- **Verification**: Generate coverage report

### Estimated Total Remediation Time

- **Critical Fixes**: 8.25 - 12.25 hours
- **Non-Critical Fixes**: 3.5 hours
- **Total**: 11.75 - 15.75 hours

---

## Executive Summary

This audit evaluates the `001-boot-sequence-splash` feature against the MTM Avalonia Template Constitution v1.1.0. The feature implements a three-stage boot sequence with splash screen, services initialization, and comprehensive error handling.

**Audit Scope**:

- 23 Entity Models (all created)
- 12+ Service Interfaces and Implementations
- 3 XAML Views
- 16+ Test Files (Contract, Integration, Unit)
- Project Configuration Files

**Overall Compliance**: **67% (4/7 principles fully compliant + async violations)**

**Critical Findings**:

- ❌ **CRITICAL**: Principle VI violated - Legacy `{Binding}` syntax found without `CompiledBinding`
- ❌ **CRITICAL**: Missing `CancellationToken` parameters in 40+ public async methods
- ⚠️ **WARNING**: No services registered in `Program.cs` (Principle VII)
- ⚠️ **WARNING**: Missing unit tests for ViewModels

---

## Principle I: Cross-Platform First ✅ COMPLIANT

- [x] All code uses platform abstractions (no direct P/Invoke)
- [x] Platform-specific code behind interfaces
- [x] Shared logic in `.Core` project (planned architecture)
- [x] Uses `RuntimeInformation.IsOSPlatform()` for detection

**Violations Found**: NONE

**Evidence**:

- ✅ `SecretsServiceFactory.cs` properly abstracts platform-specific credential storage
- ✅ `WindowsSecretsService.cs`, `MacOSSecretsService.cs`, `AndroidSecretsService.cs` implement `ISecretsService`
- ✅ `OSDarkModeMonitor.cs` uses platform detection for theme monitoring

**Compliance Score**: 100% (4/4 checks passed)

---

## Principle II: MVVM Community Toolkit Standard ✅ COMPLIANT

- [x] All ViewModels inherit from `ObservableObject`
- [x] Properties use `[ObservableProperty]`
- [x] Commands use `[RelayCommand]`
- [x] NO ReactiveUI patterns (ReactiveObject, ReactiveCommand, etc.)
- [x] Uses `[NotifyCanExecuteChangedFor]` for dependent commands (N/A - no commands yet)

**Violations Found**: NONE

**Evidence**:

- ✅ `ViewModelBase.cs` (line 5): `public abstract class ViewModelBase : ObservableObject`
- ✅ `MainViewModel.cs` (line 5): `public partial class MainViewModel : ViewModelBase`
- ✅ `MainViewModel.cs` (line 7): `[ObservableProperty] private string _greeting`
- ✅ No ReactiveUI imports or usage detected in codebase

**Compliance Score**: 100% (5/5 checks passed)

---

## Principle III: Test-First Development (TDD) ⚠️ PARTIAL COMPLIANCE

- [x] Tests written and approved BEFORE implementation (per plan.md Phase 3.1-3.2)
- [x] All tests use **xUnit** framework
- [x] Integration tests for workflows exist
- [ ] Unit tests for ViewModels exist **[VIOLATION]**
- [x] Mocks use **NSubstitute**
- [ ] > 80% code coverage on critical paths **[UNKNOWN - not measured]**

**Violations Found**:

- ⚠️ **Non-Critical**: No unit tests found for `MainViewModel` or future `SplashViewModel`
- ⚠️ **Non-Critical**: Code coverage not measured (tooling not configured)

**Evidence**:

- ✅ `tests/integration/BootSequenceTests.cs` - xUnit integration tests
- ✅ `tests/contract/MySqlContractTests.cs` - Contract tests for MySQL
- ✅ `tests/contract/VisualApiContractTests.cs` - Contract tests for Visual ERP API
- ✅ NSubstitute usage confirmed in `BootSequenceTests.cs` (line 19)
- ❌ No `tests/unit/ViewModels/` directory exists

**Remediation Plan**:

1. Create `tests/unit/ViewModels/MainViewModelTests.cs` with basic property change tests
2. Add unit tests for boot-related ViewModels (SplashViewModel when created)
3. Configure code coverage tool (Coverlet or dotnet-coverage)

**Compliance Score**: 67% (4/6 checks passed)

---

## Principle IV: Theme V2 Semantic Tokens ✅ COMPLIANT

- [x] All styles use `{DynamicResource}` tokens (N/A - no custom styles yet)
- [x] No hardcoded colors/values
- [x] Styles in separate `.axaml` files (N/A - using FluentTheme base)
- [x] Base theme is FluentTheme or Material.Avalonia

**Violations Found**: NONE

**Evidence**:

- ✅ `App.axaml` (line 13): `<FluentTheme />` - Base theme configured
- ✅ No hardcoded colors found in XAML files
- ✅ `ThemeService.cs` implements dynamic theme switching

**Note**: Feature currently uses default FluentTheme. Custom styles will be added in future phases per spec.

**Compliance Score**: 100% (4/4 checks passed)

---

## Principle V: Null Safety and Error Resilience ✅ COMPLIANT

- [x] Nullable reference types enabled
- [x] `ArgumentNullException.ThrowIfNull()` used
- [x] Error boundaries in ViewModels (try-catch with logging) - Planned in services
- [x] Serilog structured logging used
- [x] Graceful offline degradation

**Violations Found**: NONE

**Evidence**:

- ✅ `MTM_Template_Application.csproj` (line 4): `<Nullable>enable</Nullable>`
- ✅ `ConfigurationService.cs` (line 23): `ArgumentNullException.ThrowIfNull(logger);`
- ✅ `ConfigurationService.cs` (line 32): `ArgumentNullException.ThrowIfNull(key);`
- ✅ Package reference: `Serilog.Sinks.Console`, `Serilog.Sinks.File`, `Serilog.Sinks.OpenTelemetry`
- ✅ `ErrorReport.cs` entity model supports diagnostic error reporting

**Compliance Score**: 100% (5/5 checks passed)

---

## Principle VI: Compiled Bindings Only (CRITICAL) ❌ **CRITICAL VIOLATION**

- [x] All XAML files have `x:DataType` attribute (1/2 files)
- [ ] All XAML files have `x:CompileBindings="True"` **[VIOLATION]**
- [ ] All bindings use `{CompiledBinding}` syntax **[CRITICAL VIOLATION]**
- [ ] NO legacy `{Binding}` syntax without `x:CompileBindings` **[CRITICAL VIOLATION]**
- [x] `Design.DataContext` set for previewer

**Violations Found**:

- ❌ **CRITICAL**: `MainView.axaml` (line 15): Uses legacy `{Binding Greeting}` syntax
- ❌ **CRITICAL**: `MainWindow.axaml`: Missing `x:DataType` attribute
- ⚠️ **WARNING**: `MTM_Template_Application.csproj` has `<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>` but XAML files don't comply

**Evidence**:

```xml
<!-- ❌ VIOLATION: MainView.axaml line 15 -->
<TextBlock Text="{Binding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>

<!-- Should be: -->
<TextBlock Text="{CompiledBinding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
```

**Remediation Plan** (MUST FIX BEFORE MERGE):

1. **File**: `MTM_Template_Application/Views/MainView.axaml`

   - Line 15: Change `{Binding Greeting}` to `{CompiledBinding Greeting}`
   - Verify `x:DataType="vm:MainViewModel"` is present (already exists on line 8)

2. **File**: `MTM_Template_Application/Views/MainWindow.axaml`

   - Add `x:DataType="vm:MainViewModel"` to root `<Window>` element (even if no direct bindings)
   - Or remove DataContext assignment if not needed

3. **Verification**:
   - Run `dotnet build` - should produce no binding warnings
   - Check Avalonia previewer works correctly
   - Add binding unit tests to verify compiled binding performance

**Estimated Fix Time**: 15 minutes

**Compliance Score**: 40% (2/5 checks passed) - **BLOCKING ISSUE**

---

## Principle VII: Dependency Injection via AppBuilder ⚠️ PARTIAL COMPLIANCE

- [ ] Services registered in `Program.cs` **[VIOLATION]**
- [x] ViewModels use constructor injection (N/A - no services injected yet)
- [x] NO service locator pattern
- [x] NO static service access

**Violations Found**:

- ⚠️ **Non-Critical**: `Program.cs` contains no service registrations
- ⚠️ **Non-Critical**: AppBuilder not configured with DI container

**Evidence**:

```csharp
// ❌ VIOLATION: Program.cs - No DI configuration
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
```

**Expected Pattern** (from constitution):

```csharp
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .ConfigureServices(services =>
        {
            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<SplashViewModel>();

            // Register Services
            services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ISecretsService, SecretsServiceFactory.Create>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
            // ... more services
        });
}
```

**Remediation Plan**:

1. Add `Microsoft.Extensions.DependencyInjection` package reference
2. Update `Program.cs` with `.ConfigureServices()` call
3. Register all service interfaces from `specs/001-boot-sequence-splash/plan.md`
4. Update `App.axaml.cs` to resolve ViewModels from DI container
5. Add integration test to verify DI configuration

**Estimated Fix Time**: 2 hours

**Compliance Score**: 75% (3/4 checks passed)

---

## Async/Await Patterns ❌ **CRITICAL VIOLATION**

- [ ] All async methods have `CancellationToken` parameter **[CRITICAL VIOLATION]**
- [x] Methods suffixed with `Async`
- [x] `ConfigureAwait(false)` in library code (NOT UI code) - Correctly avoided in UI code
- [ ] Cancellation propagated to downstream calls **[UNKNOWN - can't verify without CancellationToken]**

**Violations Found**:

- ❌ **CRITICAL**: 40+ async methods missing `CancellationToken` parameter

**Evidence of Violations**:

### Interface Violations (Must fix - breaking changes)

1. `IBootOrchestrator.cs`:

   - Line 15: `Task ExecuteStage0Async();` → Add `CancellationToken cancellationToken = default`
   - Line 20: `Task ExecuteStage1Async();` → Add `CancellationToken cancellationToken = default`
   - Line 25: `Task ExecuteStage2Async();` → Add `CancellationToken cancellationToken = default`

2. `ISecretsService.cs`:

   - Line 13: `Task StoreSecretAsync(string key, string value);`
   - Line 18: `Task<string?> RetrieveSecretAsync(string key);`
   - Line 23: `Task DeleteSecretAsync(string key);`
   - Line 28: `Task RotateSecretAsync(string key, string newValue);`

3. `IConfigurationService.cs`:

   - Line 19: `Task SetValue(string key, object value);`
   - Line 24: `Task ReloadAsync();`

4. `ILoggingService.cs`:

   - Line 33: `Task FlushAsync();`

5. `IDiagnosticsService.cs`:

   - Line 15: `Task<List<DiagnosticResult>> RunAllChecksAsync();`
   - Line 20: `Task<DiagnosticResult> RunCheckAsync(string checkName);`

6. `INavigationService.cs`:
   - Line 15: `Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null);`
   - Line 20: `Task<bool> GoBackAsync();`
   - Line 25: `Task<bool> GoForwardAsync();`

### Implementation Violations (Examples)

7. `ConfigurationService.cs`:

   - Line 60: `public Task SetValue(string key, object value)`
   - Line 97: `public Task ReloadAsync()`

8. `DiagnosticsService.cs`:

   - Line 32: `public async Task<List<DiagnosticResult>> RunAllChecksAsync()`
   - Line 69: `public async Task<DiagnosticResult> RunCheckAsync(string checkName)`

9. `NavigationService.cs`:

   - Line 28: `public async Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null)`
   - Line 61: `public async Task<bool> GoBackAsync()`
   - Line 89: `public async Task<bool> GoForwardAsync()`

10. `LoggingService.cs`:

    - Line 102: `public async Task FlushAsync()`

11. `TelemetryBatchProcessor.cs`:

    - Line 52: `public async Task AddAsync(LogEntry entry)`
    - Line 81: `public async Task FlushAsync()`

12. All platform secrets services:

    - `WindowsSecretsService.cs` (lines 26, 69, 104, 121)
    - `MacOSSecretsService.cs` (lines 25, 64, 96, 105)
    - `AndroidSecretsService.cs` (lines 25, 64, 96, 105)

13. Diagnostics checks:

    - `StorageDiagnostic.cs` (line 18)
    - `PermissionsDiagnostic.cs` (lines 17, 78)
    - `NetworkDiagnostic.cs` (lines 27, 134)

14. Data layer clients:

    - `MySqlClient.cs` (lines 66, 128)
    - `HttpApiClient.cs` (line 115)
    - `VisualApiClient.cs` (lines 40, 81, 136)

15. Core services:
    - `HealthCheckService.cs` (lines 24, 63)
    - `FeatureFlagEvaluator.cs` (lines 41, 86)

**Remediation Plan** (MUST FIX BEFORE MERGE):

### Phase 1: Update All Interfaces (Breaking Changes)

1. Add `CancellationToken cancellationToken = default` to all async interface methods
2. Update XML documentation to mention cancellation support
3. Run `dotnet build` to identify all implementations that need updates

### Phase 2: Update All Implementations

1. Add `CancellationToken cancellationToken = default` parameter to all async methods
2. Propagate `cancellationToken` to downstream async calls:
   - `await someService.DoWorkAsync(cancellationToken);`
   - `await Task.Delay(1000, cancellationToken);`
   - Pass to `HttpClient` requests
   - Pass to database operations
3. Wrap long-running operations with cancellation checks:

   ```csharp
   cancellationToken.ThrowIfCancellationRequested();
   ```

### Phase 3: Update Tests

1. Update integration tests to pass `CancellationToken.None` or test cancellation
2. Add cancellation tests for long-running operations

### Example Fix Pattern

```csharp
// ❌ BEFORE (IBootOrchestrator.cs)
Task ExecuteStage0Async();

// ✅ AFTER
Task ExecuteStage0Async(CancellationToken cancellationToken = default);

// ❌ BEFORE (ConfigurationService.cs)
public Task ReloadAsync()
{
    lock (_lock)
    {
        // ... reload logic
    }
    return Task.CompletedTask;
}

// ✅ AFTER
public Task ReloadAsync(CancellationToken cancellationToken = default)
{
    cancellationToken.ThrowIfCancellationRequested();

    lock (_lock)
    {
        // ... reload logic
    }
    return Task.CompletedTask;
}
```

**Estimated Fix Time**: 8-12 hours (40+ files to update, tests to verify)

**Compliance Score**: 50% (2/4 checks passed) - **BLOCKING ISSUE**

---

## Spec-Kit Integration ✅ COMPLIANT

- [x] Feature has SPEC\_\*.md file (spec.md)
- [x] Feature has PLAN\_\*.md file (plan.md)
- [x] Feature has TASKS\_\*.md file (tasks.md)
- [x] Scripts output parsed correctly (N/A - no scripts run yet)
- [x] Git branch follows naming convention

**Violations Found**: NONE

**Evidence**:

- ✅ `specs/001-boot-sequence-splash/spec.md` - Complete functional specification
- ✅ `specs/001-boot-sequence-splash/plan.md` - Detailed implementation plan
- ✅ `specs/001-boot-sequence-splash/tasks.md` - 171 tasks generated
- ✅ Git branch: `copilot/fix-5c069ed5-b943-4e34-a7c0-3e9a60d34428` (follows convention)
- ✅ 11 clarification documents in `clarify/` subdirectory

**Compliance Score**: 100% (5/5 checks passed)

---

## Summary

**Total Violations**: 48 (2 Critical Principles + 40+ Async Methods)
**Critical Violations** (must fix before merge): 43

- 1 XAML binding violation (Principle VI)
- 2 XAML missing attributes (Principle VI)
- 40+ Missing CancellationToken parameters (Async Patterns)

**Non-Critical Violations** (can fix in follow-up): 5

- Missing ViewModel unit tests (Principle III)
- Code coverage not measured (Principle III)
- No DI service registration (Principle VII)
- No DI configuration in Program.cs (Principle VII)
- AppBuilder not using .ConfigureServices (Principle VII)

**Compliant Principles**: 4/7

- ✅ Principle I: Cross-Platform First (100%)
- ✅ Principle II: MVVM Community Toolkit (100%)
- ✅ Principle IV: Theme V2 Semantic Tokens (100%)
- ✅ Principle V: Null Safety (100%)

**Partially Compliant**: 1/7

- ⚠️ Principle III: TDD (67% - missing ViewModel tests)

**Non-Compliant**: 2/7

- ❌ Principle VI: Compiled Bindings (40% - CRITICAL)
- ❌ Principle VII: DI via AppBuilder (75% - non-critical)
- ❌ Async/Await Patterns (50% - CRITICAL)

---

## Next Steps

### BLOCKING Issues (Must Complete Before Merge)

1. **[CRITICAL - 15 min]** Fix XAML Binding Violations

   - File: `MTM_Template_Application/Views/MainView.axaml`
   - Action: Change line 15 from `{Binding Greeting}` to `{CompiledBinding Greeting}`
   - Verification: `dotnet build` produces no warnings

2. **[CRITICAL - 8-12 hours]** Add CancellationToken to All Async Methods
   - Files: 40+ service interfaces and implementations
   - Action: Add `CancellationToken cancellationToken = default` parameter
   - Action: Propagate cancellation to downstream calls
   - Verification: Update all tests, run full test suite

### Non-Blocking Issues (Can Address in Follow-Up)

3. **[2 hours]** Configure Dependency Injection in Program.cs

   - File: `MTM_Template_Application.Desktop/Program.cs`
   - Action: Add `.ConfigureServices()` with service registrations
   - Verification: Integration test for DI container

4. **[1 hour]** Create ViewModel Unit Tests

   - File: Create `tests/unit/ViewModels/MainViewModelTests.cs`
   - Action: Test property changes, command execution
   - Verification: Run tests with `dotnet test`

5. **[30 min]** Configure Code Coverage
   - Action: Add Coverlet or dotnet-coverage to test project
   - Action: Set up coverage reporting in CI/CD
   - Verification: Generate coverage report

---

## Estimated Total Remediation Time

- **Critical Fixes**: 8.25 - 12.25 hours
- **Non-Critical Fixes**: 3.5 hours
- **Total**: 11.75 - 15.75 hours

---

## Compliance Trend

This is the first audit for the `001-boot-sequence-splash` feature. Baseline metrics:

- **Overall Compliance**: 67% (4/7 principles fully compliant)
- **Critical Issues**: 43
- **Non-Critical Issues**: 5

**Target for Next Audit**: 100% compliance (0 critical issues)

---

## Auditor Notes

**Positive Findings**:

- ✅ Excellent adherence to MVVM Community Toolkit patterns
- ✅ Strong null safety implementation with nullable types enabled
- ✅ Good platform abstraction with `ISecretsService` implementations
- ✅ Comprehensive test coverage plan with Contract, Integration, and Unit test structure
- ✅ Well-documented spec-kit integration with clarifications

**Areas of Concern**:

- ⚠️ The project has `<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>` but legacy `{Binding}` syntax still used
- ⚠️ Missing CancellationToken is a systemic issue affecting 40+ methods - suggests pattern was not followed during initial implementation
- ⚠️ DI configuration missing despite comprehensive service architecture - may indicate incomplete setup phase

**Recommendations**:

1. Create a pre-commit hook to detect `{Binding` syntax in XAML files
2. Add a code analyzer rule to enforce CancellationToken in async methods
3. Update GitHub Copilot instructions to always include CancellationToken in async method signatures
4. Consider creating a "Service Registration Checklist" for new services

---

**Audit Complete** | **Status**: ⚠️ BLOCKING ISSUES FOUND | **Next Audit**: After Critical Fixes Implemented

---

## Appendix A: File-by-File Compliance Matrix

| File Path                  | Principle I | Principle II | Principle III | Principle IV | Principle V | Principle VI       | Principle VII | Async/Await |
| -------------------------- | ----------- | ------------ | ------------- | ------------ | ----------- | ------------------ | ------------- | ----------- |
| `MainView.axaml`           | N/A         | N/A          | N/A           | ✅           | N/A         | ❌ (Binding)       | N/A           | N/A         |
| `MainWindow.axaml`         | N/A         | N/A          | N/A           | ✅           | N/A         | ⚠️ (No x:DataType) | N/A           | N/A         |
| `MainViewModel.cs`         | ✅          | ✅           | ⚠️ (No tests) | N/A          | ✅          | N/A                | ✅            | N/A         |
| `ViewModelBase.cs`         | ✅          | ✅           | N/A           | N/A          | ✅          | N/A                | ✅            | N/A         |
| `IBootOrchestrator.cs`     | ✅          | N/A          | N/A           | N/A          | ✅          | N/A                | N/A           | ❌ (No CT)  |
| `IConfigurationService.cs` | ✅          | N/A          | N/A           | N/A          | ✅          | N/A                | N/A           | ❌ (No CT)  |
| `ConfigurationService.cs`  | ✅          | N/A          | ⚠️            | N/A          | ✅          | N/A                | ✅            | ❌ (No CT)  |
| `ISecretsService.cs`       | ✅          | N/A          | N/A           | N/A          | ✅          | N/A                | N/A           | ❌ (No CT)  |
| `WindowsSecretsService.cs` | ✅          | N/A          | ⚠️            | N/A          | ✅          | N/A                | ✅            | ❌ (No CT)  |
| `Program.cs`               | ✅          | N/A          | N/A           | N/A          | ✅          | N/A                | ❌ (No DI)    | N/A         |
| `BootSequenceTests.cs`     | ✅          | N/A          | ✅ (xUnit)    | N/A          | ✅          | N/A                | N/A           | N/A         |

**Legend**:

- ✅ Compliant
- ⚠️ Partial Compliance / Warning
- ❌ Non-Compliant
- N/A Not Applicable
- CT = CancellationToken

---

**Generated**: 2025-10-03 | **Constitution Version**: 1.1.0 | **Audit Format Version**: 1.0
