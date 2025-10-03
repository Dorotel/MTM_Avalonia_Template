# Constitutional Remediation Checklist
**Feature**: 001-boot-sequence-splash (Boot Sequence — Splash-First, Services Initialization Order)
**Remediation Date**: 2025-10-03
**Audit Reference**: [AUDIT_001-boot-sequence-splash_2025-10-03.md](./AUDIT_001-boot-sequence-splash_2025-10-03.md)
**Status**: 🔴 NOT STARTED

---

## Progress Summary

| Category | Total | Complete | Remaining | % Complete |
|----------|-------|----------|-----------|------------|
| **BLOCKING Issues** | 43 | 0 | 43 | 0% |
| **Non-Blocking Issues** | 5 | 0 | 5 | 0% |
| **TOTAL** | 48 | 0 | 48 | 0% |

**Estimated Total Time**: 11.75 - 15.75 hours
- Critical Fixes: 8.25 - 12.25 hours
- Non-Critical Fixes: 3.5 hours

---

## 🚨 BLOCKING Issues (Must Complete Before Merge)

### Group 1: XAML Binding Violations (Principle VI)

#### REM001: Fix Legacy Binding Syntax in MainView.axaml

- [ ] **REM001** Fix legacy `{Binding}` syntax in MainView.axaml
  - **Related Task**: T125 (XAML view implementation)
  - **Principle Violated**: Principle VI - Compiled Bindings Only
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Views/MainView.axaml:15`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Views\MainView.axaml#15)
  - **Before**:
    ```xml
    <TextBlock Text="{Binding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    ```
  - **After**:
    ```xml
    <TextBlock Text="{CompiledBinding Greeting}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: No binding warnings
    - Check: Avalonia previewer works correctly
  - **Commit Message**: `fix(xaml): use CompiledBinding in MainView instead of legacy Binding syntax`
  - **Estimated Time**: 5 minutes
  - **Dependencies**: None

#### REM002: Add x:DataType to MainWindow.axaml

- [ ] **REM002** Add missing `x:DataType` attribute to MainWindow.axaml root element
  - **Related Task**: T124 (MainWindow XAML)
  - **Principle Violated**: Principle VI - Compiled Bindings Only
  - **Severity**: ⚠️ WARNING (Best Practice)
  - **Files**:
    - [`MTM_Template_Application/Views/MainWindow.axaml:8`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Views\MainWindow.axaml#8)
  - **Before**:
    ```xml
    <Window xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:vm="using:MTM_Template_Application.ViewModels"
            xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
            xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
            xmlns:views="clr-namespace:MTM_Template_Application.Views"
            mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
            x:Class="MTM_Template_Application.Views.MainWindow"
            Icon="/Assets/avalonia-logo.ico"
            Title="MTM_Template_Application">
    ```
  - **After**:
    ```xml
    <Window xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:vm="using:MTM_Template_Application.ViewModels"
            xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
            xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
            xmlns:views="clr-namespace:MTM_Template_Application.Views"
            mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
            x:Class="MTM_Template_Application.Views.MainWindow"
            x:DataType="vm:MainViewModel"
            x:CompileBindings="True"
            Icon="/Assets/avalonia-logo.ico"
            Title="MTM_Template_Application">
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: No warnings
  - **Commit Message**: `fix(xaml): add x:DataType and x:CompileBindings to MainWindow`
  - **Estimated Time**: 5 minutes
  - **Dependencies**: None

---

### Group 2: Missing CancellationToken Parameters (Async Patterns)

> **⚠️ IMPORTANT**: Follow this order to minimize breaking changes:
> 1. Update all interface definitions first (REM003-REM008)
> 2. Update all implementations (REM009-REM042)
> 3. Update all tests (REM043)

#### REM003: Add CancellationToken to IBootOrchestrator Interface

- [ ] **REM003** Add `CancellationToken` parameters to all async methods in IBootOrchestrator
  - **Related Task**: T039
  - **Principle Violated**: Async/Await Patterns (Constitutional Requirement)
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Boot/IBootOrchestrator.cs:15,20,25`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Boot\IBootOrchestrator.cs)
  - **Before**:
    ```csharp
    public interface IBootOrchestrator
    {
        Task ExecuteStage0Async();
        Task ExecuteStage1Async();
        Task ExecuteStage2Async();
        BootMetrics GetBootMetrics();
        event EventHandler<BootProgressEventArgs>? OnProgressChanged;
    }
    ```
  - **After**:
    ```csharp
    public interface IBootOrchestrator
    {
        Task ExecuteStage0Async(CancellationToken cancellationToken = default);
        Task ExecuteStage1Async(CancellationToken cancellationToken = default);
        Task ExecuteStage2Async(CancellationToken cancellationToken = default);
        BootMetrics GetBootMetrics();
        event EventHandler<BootProgressEventArgs>? OnProgressChanged;
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in implementation classes (to be fixed in REM009)
  - **Commit Message**: `fix(interfaces): add CancellationToken to IBootOrchestrator async methods`
  - **Estimated Time**: 5 minutes
  - **Dependencies**: None

#### REM004: Add CancellationToken to ISecretsService Interface

- [ ] **REM004** Add `CancellationToken` parameters to all async methods in ISecretsService
  - **Related Task**: T042
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Secrets/ISecretsService.cs:13,18,23,28`](../../MTM_Template_Application/Services/Secrets/ISecretsService.cs)
  - **Before**:
    ```csharp
    public interface ISecretsService
    {
        Task StoreSecretAsync(string key, string value);
        Task<string?> RetrieveSecretAsync(string key);
        Task DeleteSecretAsync(string key);
        Task RotateSecretAsync(string key, string newValue);
    }
    ```
  - **After**:
    ```csharp
    public interface ISecretsService
    {
        Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default);
        Task<string?> RetrieveSecretAsync(string key, CancellationToken cancellationToken = default);
        Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default);
        Task RotateSecretAsync(string key, string newValue, CancellationToken cancellationToken = default);
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in WindowsSecretsService, MacOSSecretsService, AndroidSecretsService (to be fixed in REM010-REM012)
  - **Commit Message**: `fix(interfaces): add CancellationToken to ISecretsService async methods`
  - **Estimated Time**: 5 minutes
  - **Dependencies**: None

#### REM005: Add CancellationToken to IConfigurationService Interface

- [ ] **REM005** Add `CancellationToken` parameters to all async methods in IConfigurationService
  - **Related Task**: T041
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Configuration/IConfigurationService.cs:19,24`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Configuration\IConfigurationService.cs)
  - **Before**:
    ```csharp
    public interface IConfigurationService
    {
        T? GetValue<T>(string key, T? defaultValue = default);
        Task SetValue(string key, object value);
        Task ReloadAsync();
        event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;
    }
    ```
  - **After**:
    ```csharp
    public interface IConfigurationService
    {
        T? GetValue<T>(string key, T? defaultValue = default);
        Task SetValue(string key, object value, CancellationToken cancellationToken = default);
        Task ReloadAsync(CancellationToken cancellationToken = default);
        event EventHandler<ConfigurationChangedEventArgs>? OnConfigurationChanged;
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in ConfigurationService (to be fixed in REM013)
  - **Commit Message**: `fix(interfaces): add CancellationToken to IConfigurationService async methods`
  - **Estimated Time**: 3 minutes
  - **Dependencies**: None

#### REM006: Add CancellationToken to ILoggingService Interface

- [ ] **REM006** Add `CancellationToken` parameter to FlushAsync in ILoggingService
  - **Related Task**: T043
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Logging/ILoggingService.cs:33`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Logging\ILoggingService.cs)
  - **Before**:
    ```csharp
    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, Exception? exception = null, params object[] args);
        void SetContext(string key, object value);
        Task FlushAsync();
    }
    ```
  - **After**:
    ```csharp
    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, Exception? exception = null, params object[] args);
        void SetContext(string key, object value);
        Task FlushAsync(CancellationToken cancellationToken = default);
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in LoggingService (to be fixed in REM014)
  - **Commit Message**: `fix(interfaces): add CancellationToken to ILoggingService.FlushAsync`
  - **Estimated Time**: 3 minutes
  - **Dependencies**: None

#### REM007: Add CancellationToken to IDiagnosticsService Interface

- [ ] **REM007** Add `CancellationToken` parameters to all async methods in IDiagnosticsService
  - **Related Task**: T044
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Diagnostics/IDiagnosticsService.cs:15,20`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Diagnostics\IDiagnosticsService.cs)
  - **Before**:
    ```csharp
    public interface IDiagnosticsService
    {
        Task<List<DiagnosticResult>> RunAllChecksAsync();
        Task<DiagnosticResult> RunCheckAsync(string checkName);
        HardwareCapabilities GetHardwareCapabilities();
    }
    ```
  - **After**:
    ```csharp
    public interface IDiagnosticsService
    {
        Task<List<DiagnosticResult>> RunAllChecksAsync(CancellationToken cancellationToken = default);
        Task<DiagnosticResult> RunCheckAsync(string checkName, CancellationToken cancellationToken = default);
        HardwareCapabilities GetHardwareCapabilities();
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in DiagnosticsService (to be fixed in REM015)
  - **Commit Message**: `fix(interfaces): add CancellationToken to IDiagnosticsService async methods`
  - **Estimated Time**: 3 minutes
  - **Dependencies**: None

#### REM008: Add CancellationToken to INavigationService Interface

- [ ] **REM008** Add `CancellationToken` parameters to all async methods in INavigationService
  - **Related Task**: T054
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Navigation/INavigationService.cs:15,20,25`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Navigation\INavigationService.cs)
  - **Before**:
    ```csharp
    public interface INavigationService
    {
        Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null);
        Task<bool> GoBackAsync();
        Task<bool> GoForwardAsync();
        List<NavigationHistoryEntry> GetHistory();
    }
    ```
  - **After**:
    ```csharp
    public interface INavigationService
    {
        Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
        Task<bool> GoBackAsync(CancellationToken cancellationToken = default);
        Task<bool> GoForwardAsync(CancellationToken cancellationToken = default);
        List<NavigationHistoryEntry> GetHistory();
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: Compilation errors in NavigationService (to be fixed in REM016)
  - **Commit Message**: `fix(interfaces): add CancellationToken to INavigationService async methods`
  - **Estimated Time**: 3 minutes
  - **Dependencies**: None

---

#### Implementation Updates (Complete after all interface updates REM003-REM008)

#### REM009: Update BootOrchestrator Implementation

- [ ] **REM009** Add `CancellationToken` parameters and propagate cancellation in BootOrchestrator
  - **Related Task**: T079
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - `MTM_Template_Application/Services/Boot/BootOrchestrator.cs` (multiple methods)
  - **Pattern to Apply**:
    ```csharp
    // Before
    public async Task ExecuteStage0Async()
    {
        await someService.DoWorkAsync();
    }

    // After
    public async Task ExecuteStage0Async(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await someService.DoWorkAsync(cancellationToken);
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: No compilation errors
    - Run: Integration tests `dotnet test --filter "BootSequenceTests"`
  - **Commit Message**: `fix(boot): add CancellationToken support to BootOrchestrator`
  - **Estimated Time**: 30 minutes
  - **Dependencies**: REM003 (interface update)

#### REM010: Update WindowsSecretsService Implementation

- [ ] **REM010** Add `CancellationToken` parameters to WindowsSecretsService
  - **Related Task**: T082
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Secrets/WindowsSecretsService.cs:26,69,104,121`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Secrets\WindowsSecretsService.cs)
  - **Pattern to Apply**:
    ```csharp
    // Before
    public async Task StoreSecretAsync(string key, string value)
    {
        // ... implementation
    }

    // After
    public async Task StoreSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // ... implementation
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Contract tests `dotnet test --filter "SecretsTests"`
  - **Commit Message**: `fix(secrets): add CancellationToken support to WindowsSecretsService`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: REM004 (interface update)

#### REM011: Update MacOSSecretsService Implementation

- [ ] **REM011** Add `CancellationToken` parameters to MacOSSecretsService
  - **Related Task**: T083
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Secrets/MacOSSecretsService.cs:25,64,96,105`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Secrets\MacOSSecretsService.cs)
  - **Pattern to Apply**: Same as REM010
  - **Verification**:
    - Run: `dotnet build`
    - Run: Contract tests `dotnet test --filter "SecretsTests"`
  - **Commit Message**: `fix(secrets): add CancellationToken support to MacOSSecretsService`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: REM004 (interface update)

#### REM012: Update AndroidSecretsService Implementation

- [ ] **REM012** Add `CancellationToken` parameters to AndroidSecretsService
  - **Related Task**: T084
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Secrets/AndroidSecretsService.cs:25,64,96,105`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Secrets\AndroidSecretsService.cs)
  - **Pattern to Apply**: Same as REM010
  - **Verification**:
    - Run: `dotnet build`
    - Run: Contract tests `dotnet test --filter "SecretsTests"`
  - **Commit Message**: `fix(secrets): add CancellationToken support to AndroidSecretsService`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: REM004 (interface update)

#### REM013: Update ConfigurationService Implementation

- [ ] **REM013** Add `CancellationToken` parameters to ConfigurationService
  - **Related Task**: T079
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Configuration/ConfigurationService.cs:60,97`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Configuration\ConfigurationService.cs)
  - **Before**:
    ```csharp
    public Task SetValue(string key, object value)
    {
        ArgumentNullException.ThrowIfNull(key);
        lock (_lock)
        {
            _settings[key] = new ConfigurationSetting
            {
                Key = key,
                Value = value,
                Source = "Runtime",
                Precedence = 100
            };
        }
        OnConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(key, value));
        return Task.CompletedTask;
    }

    public Task ReloadAsync()
    {
        lock (_lock)
        {
            // Reload logic
        }
        return Task.CompletedTask;
    }
    ```
  - **After**:
    ```csharp
    public Task SetValue(string key, object value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(key);
        lock (_lock)
        {
            _settings[key] = new ConfigurationSetting
            {
                Key = key,
                Value = value,
                Source = "Runtime",
                Precedence = 100
            };
        }
        OnConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(key, value));
        return Task.CompletedTask;
    }

    public Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            // Reload logic
        }
        return Task.CompletedTask;
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Integration tests `dotnet test --filter "ConfigurationTests"`
  - **Commit Message**: `fix(config): add CancellationToken support to ConfigurationService`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: REM005 (interface update)

#### REM014: Update LoggingService Implementation

- [ ] **REM014** Add `CancellationToken` parameter to LoggingService.FlushAsync
  - **Related Task**: T086
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Logging/LoggingService.cs:102`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Logging\LoggingService.cs)
  - **Before**:
    ```csharp
    public async Task FlushAsync()
    {
        await Log.CloseAndFlushAsync();
    }
    ```
  - **After**:
    ```csharp
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5s timeout for flush

        await Log.CloseAndFlushAsync();
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Integration tests `dotnet test --filter "LoggingTests"`
  - **Commit Message**: `fix(logging): add CancellationToken support to LoggingService.FlushAsync`
  - **Estimated Time**: 10 minutes
  - **Dependencies**: REM006 (interface update)

#### REM015: Update DiagnosticsService Implementation

- [ ] **REM015** Add `CancellationToken` parameters to DiagnosticsService
  - **Related Task**: T089
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Diagnostics/DiagnosticsService.cs:32,69`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Diagnostics\DiagnosticsService.cs)
  - **Before**:
    ```csharp
    public async Task<List<DiagnosticResult>> RunAllChecksAsync()
    {
        var results = new List<DiagnosticResult>();
        foreach (var check in _checks)
        {
            results.Add(await check.RunAsync());
        }
        return results;
    }

    public async Task<DiagnosticResult> RunCheckAsync(string checkName)
    {
        var check = _checks.FirstOrDefault(c => c.Name == checkName);
        if (check == null)
        {
            throw new ArgumentException($"Check '{checkName}' not found");
        }
        return await check.RunAsync();
    }
    ```
  - **After**:
    ```csharp
    public async Task<List<DiagnosticResult>> RunAllChecksAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<DiagnosticResult>();
        foreach (var check in _checks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await check.RunAsync(cancellationToken));
        }
        return results;
    }

    public async Task<DiagnosticResult> RunCheckAsync(string checkName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var check = _checks.FirstOrDefault(c => c.Name == checkName);
        if (check == null)
        {
            throw new ArgumentException($"Check '{checkName}' not found");
        }
        return await check.RunAsync(cancellationToken);
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Integration tests `dotnet test --filter "DiagnosticsTests"`
  - **Commit Message**: `fix(diagnostics): add CancellationToken support to DiagnosticsService`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: REM007 (interface update)

#### REM016: Update NavigationService Implementation

- [ ] **REM016** Add `CancellationToken` parameters to NavigationService
  - **Related Task**: T110
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Navigation/NavigationService.cs:28,61,89`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Navigation\NavigationService.cs)
  - **Pattern to Apply**:
    ```csharp
    // Before
    public async Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null)
    {
        // ... implementation
    }

    // After
    public async Task NavigateToAsync(string viewName, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // ... implementation
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Integration tests for navigation
  - **Commit Message**: `fix(navigation): add CancellationToken support to NavigationService`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: REM008 (interface update)

#### REM017: Update TelemetryBatchProcessor Implementation

- [ ] **REM017** Add `CancellationToken` parameters to TelemetryBatchProcessor
  - **Related Task**: T089
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Logging/TelemetryBatchProcessor.cs:52,81`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Logging\TelemetryBatchProcessor.cs)
  - **Pattern to Apply**: Add cancellation token to AddAsync and FlushAsync
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(telemetry): add CancellationToken support to TelemetryBatchProcessor`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

#### REM018: Update StorageDiagnostic Check

- [ ] **REM018** Add `CancellationToken` parameter to StorageDiagnostic.RunAsync
  - **Related Task**: T090
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Diagnostics/Checks/StorageDiagnostic.cs:18`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Diagnostics\Checks\StorageDiagnostic.cs)
  - **Pattern to Apply**: Add cancellation token parameter
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(diagnostics): add CancellationToken to StorageDiagnostic`
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

#### REM019: Update PermissionsDiagnostic Check

- [ ] **REM019** Add `CancellationToken` parameters to PermissionsDiagnostic
  - **Related Task**: T091
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Diagnostics/Checks/PermissionsDiagnostic.cs:17,78`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Diagnostics\Checks\PermissionsDiagnostic.cs)
  - **Pattern to Apply**: Add cancellation token parameters
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(diagnostics): add CancellationToken to PermissionsDiagnostic`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

#### REM020: Update NetworkDiagnostic Check

- [ ] **REM020** Add `CancellationToken` parameters to NetworkDiagnostic
  - **Related Task**: T092
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Diagnostics/Checks/NetworkDiagnostic.cs:27,134`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Diagnostics\Checks\NetworkDiagnostic.cs)
  - **Pattern to Apply**: Add cancellation token parameters, use for HTTP request timeout
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(diagnostics): add CancellationToken to NetworkDiagnostic`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

#### REM021: Update MySqlClient Implementation

- [ ] **REM021** Add `CancellationToken` parameters to MySqlClient
  - **Related Task**: T094
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/DataLayer/MySqlClient.cs:66,128`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\DataLayer\MySqlClient.cs)
  - **Pattern to Apply**:
    ```csharp
    // Pass cancellation token to MySqlCommand operations
    await command.ExecuteReaderAsync(cancellationToken);
    await connection.OpenAsync(cancellationToken);
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Run: Contract tests `dotnet test --filter "MySqlContractTests"`
  - **Commit Message**: `fix(data): add CancellationToken support to MySqlClient`
  - **Estimated Time**: 30 minutes
  - **Dependencies**: None

#### REM022: Update HttpApiClient Implementation

- [ ] **REM022** Add `CancellationToken` parameters to HttpApiClient
  - **Related Task**: T096
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/DataLayer/HttpApiClient.cs:115`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\DataLayer\HttpApiClient.cs)
  - **Pattern to Apply**: Pass cancellation token to HttpClient methods
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(data): add CancellationToken support to HttpApiClient`
  - **Estimated Time**: 20 minutes
  - **Dependencies**: None

#### REM023: Update VisualApiClient Implementation

- [ ] **REM023** Add `CancellationToken` parameters to VisualApiClient
  - **Related Task**: T095
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/DataLayer/VisualApiClient.cs:40,81,136`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\DataLayer\VisualApiClient.cs)
  - **Pattern to Apply**: Pass cancellation token through to HttpApiClient
  - **Verification**:
    - Run: `dotnet build`
    - Run: Contract tests `dotnet test --filter "VisualApiContractTests"`
  - **Commit Message**: `fix(data): add CancellationToken support to VisualApiClient`
  - **Estimated Time**: 25 minutes
  - **Dependencies**: REM022 (HttpApiClient update)

#### REM024: Update HealthCheckService Implementation

- [ ] **REM024** Add `CancellationToken` parameters to HealthCheckService
  - **Related Task**: T100
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Core/HealthCheckService.cs:24,63`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Core\HealthCheckService.cs)
  - **Pattern to Apply**: Add cancellation token parameters
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(health): add CancellationToken support to HealthCheckService`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

#### REM025: Update FeatureFlagEvaluator Implementation

- [ ] **REM025** Add `CancellationToken` parameters to FeatureFlagEvaluator
  - **Related Task**: T081
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - [`MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs:41,86`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application\Services\Configuration\FeatureFlagEvaluator.cs)
  - **Pattern to Apply**: Add cancellation token parameters
  - **Verification**:
    - Run: `dotnet build`
  - **Commit Message**: `fix(config): add CancellationToken support to FeatureFlagEvaluator`
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

#### REM026-042: Update Remaining Service Implementations

> **Note**: The following REM items follow the same pattern as above. Each requires:
> 1. Adding `CancellationToken cancellationToken = default` parameter
> 2. Propagating token to downstream async calls
> 3. Adding `cancellationToken.ThrowIfCancellationRequested()` at method start for long-running operations
> 4. Updating XML documentation to mention cancellation support

- [ ] **REM026** Update ICacheService interface and CacheService implementation
  - **Related Task**: T048, T101
  - **Estimated Time**: 30 minutes
  - **Dependencies**: None

- [ ] **REM027** Update IMessageBus interface and MessageBus implementation
  - **Related Task**: T049, T106
  - **Estimated Time**: 25 minutes
  - **Dependencies**: None

- [ ] **REM028** Update IValidationService interface and ValidationService implementation
  - **Related Task**: T050, T107
  - **Estimated Time**: 20 minutes
  - **Dependencies**: None

- [ ] **REM029** Update IMappingService interface (if has async methods)
  - **Related Task**: T051
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

- [ ] **REM030** Update ILocalizationService interface (if has async methods)
  - **Related Task**: T052
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

- [ ] **REM031** Update IThemeService interface (if has async methods)
  - **Related Task**: T053
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

- [ ] **REM032** Update ConfigurationPersistence implementation
  - **Related Task**: T080
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

- [ ] **REM033** Update PiiRedactionMiddleware implementation
  - **Related Task**: T087
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

- [ ] **REM034** Update LogRotationPolicy implementation
  - **Related Task**: T088
  - **Estimated Time**: 10 minutes
  - **Dependencies**: None

- [ ] **REM035** Update HardwareDetection implementation
  - **Related Task**: T093
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

- [ ] **REM036** Update ConnectionPoolMonitor implementation
  - **Related Task**: T099
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

- [ ] **REM037** Update LZ4CompressionHandler implementation
  - **Related Task**: T102
  - **Estimated Time**: 15 minutes
  - **Dependencies**: None

- [ ] **REM038** Update VisualMasterDataSync implementation
  - **Related Task**: T103
  - **Estimated Time**: 25 minutes
  - **Dependencies**: None

- [ ] **REM039** Update CacheStalenessDetector implementation
  - **Related Task**: T104
  - **Estimated Time**: 20 minutes
  - **Dependencies**: None

- [ ] **REM040** Update CachedOnlyModeManager implementation
  - **Related Task**: T105
  - **Estimated Time**: 20 minutes
  - **Dependencies**: None

- [ ] **REM041** Update all remaining service implementations with async methods
  - **Estimated Time**: 60 minutes
  - **Dependencies**: None

- [ ] **REM042** Update all helper/utility classes with async methods
  - **Estimated Time**: 30 minutes
  - **Dependencies**: None

#### REM043: Update All Tests

- [ ] **REM043** Update all integration and contract tests to pass CancellationToken
  - **Related Tasks**: T055-T078 (all test tasks)
  - **Principle Violated**: Async/Await Patterns
  - **Severity**: 🔴 BLOCKING (CRITICAL)
  - **Files**:
    - `tests/integration/BootSequenceTests.cs`
    - `tests/integration/ConfigurationTests.cs`
    - `tests/integration/SecretsTests.cs`
    - `tests/integration/DiagnosticsTests.cs`
    - `tests/integration/LoggingTests.cs`
    - `tests/contract/*.cs` (all contract tests)
  - **Pattern to Apply**:
    ```csharp
    // Before
    var result = await service.DoWorkAsync();

    // After - Option 1: Pass CancellationToken.None
    var result = await service.DoWorkAsync(CancellationToken.None);

    // After - Option 2: Test cancellation
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var result = await service.DoWorkAsync(cts.Token);
    ```
  - **Verification**:
    - Run: `dotnet test`
    - Expected: All tests pass
  - **Commit Message**: `test: update all tests to support CancellationToken parameters`
  - **Estimated Time**: 2-3 hours
  - **Dependencies**: REM003-REM042 (all interface and implementation updates)

---

## ⚠️ Non-Blocking Issues (Can Address in Follow-Up)

### Group 3: Dependency Injection Configuration (Principle VII)

#### REM044: Configure DI Container in Program.cs

- [ ] **REM044** Add service registrations to Program.cs AppBuilder
  - **Related Task**: T079-T110 (service implementations)
  - **Principle Violated**: Principle VII - Dependency Injection via AppBuilder
  - **Severity**: ⚠️ WARNING (Non-Critical)
  - **Files**:
    - [`MTM_Template_Application.Desktop/Program.cs:16`](c:\Users\jkoll\source\repos\MTM_Avalonia_Template\MTM_Template_Application.Desktop\Program.cs)
  - **Before**:
    ```csharp
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    ```
  - **After**:
    ```csharp
    using Microsoft.Extensions.DependencyInjection;

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .ConfigureServices(services =>
            {
                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<SplashViewModel>();

                // Boot Services
                services.AddSingleton<IBootOrchestrator, BootOrchestrator>();

                // Configuration
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ISecretsService>(sp => SecretsServiceFactory.Create());

                // Logging
                services.AddSingleton<ILoggingService, LoggingService>();
                services.AddSingleton<TelemetryBatchProcessor>();

                // Diagnostics
                services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
                services.AddTransient<StorageDiagnostic>();
                services.AddTransient<PermissionsDiagnostic>();
                services.AddTransient<NetworkDiagnostic>();
                services.AddTransient<HardwareDetection>();

                // Data Layer
                services.AddSingleton<IMySqlClient, MySqlClient>();
                services.AddSingleton<IVisualApiClient, VisualApiClient>();
                services.AddSingleton<IHttpApiClient, HttpApiClient>();

                // Cache
                services.AddSingleton<ICacheService, CacheService>();
                services.AddSingleton<LZ4CompressionHandler>();
                services.AddSingleton<VisualMasterDataSync>();
                services.AddSingleton<CacheStalenessDetector>();
                services.AddSingleton<CachedOnlyModeManager>();

                // Core Services
                services.AddSingleton<IMessageBus, MessageBus>();
                services.AddSingleton<IValidationService, ValidationService>();
                services.AddSingleton<IMappingService, MappingService>();
                services.AddSingleton<IHealthCheckService, HealthCheckService>();

                // Localization
                services.AddSingleton<ILocalizationService, LocalizationService>();

                // Theme
                services.AddSingleton<IThemeService, ThemeService>();

                // Navigation
                services.AddSingleton<INavigationService, NavigationService>();
            });
    }
    ```
  - **Verification**:
    - Run: `dotnet build`
    - Expected: No compilation errors
    - Add: Integration test to verify services can be resolved
  - **Commit Message**: `feat(di): configure dependency injection in Program.cs AppBuilder`
  - **Estimated Time**: 1.5 hours
  - **Dependencies**: None (can be done in parallel with BLOCKING fixes)

#### REM045: Update App.axaml.cs to Use DI Container

- [ ] **REM045** Resolve ViewModels from DI container in App.axaml.cs
  - **Related Task**: T122 (App initialization)
  - **Principle Violated**: Principle VII - Dependency Injection
  - **Severity**: ⚠️ WARNING (Non-Critical)
  - **Files**:
    - `MTM_Template_Application/App.axaml.cs`
  - **Pattern to Apply**:
    ```csharp
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Resolve from DI container instead of new()
            var services = AvaloniaLocator.Current.GetService<IServiceProvider>();
            var mainViewModel = services?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    ```
  - **Verification**:
    - Run: `dotnet run --project MTM_Template_Application.Desktop`
    - Expected: App starts with DI-injected ViewModel
  - **Commit Message**: `feat(di): resolve ViewModels from DI container in App.axaml.cs`
  - **Estimated Time**: 30 minutes
  - **Dependencies**: REM044 (DI configuration)

---

### Group 4: Unit Tests for ViewModels (Principle III - TDD)

#### REM046: Create MainViewModel Unit Tests

- [ ] **REM046** Create unit tests for MainViewModel
  - **Related Task**: T125 (MainViewModel)
  - **Principle Violated**: Principle III - Test-First Development
  - **Severity**: ⚠️ WARNING (Non-Critical)
  - **Files**:
    - Create: `tests/unit/ViewModels/MainViewModelTests.cs`
  - **Implementation**:
    ```csharp
    using Xunit;
    using FluentAssertions;
    using MTM_Template_Application.ViewModels;

    namespace MTM_Template_Tests.Unit.ViewModels;

    public class MainViewModelTests
    {
        [Fact]
        public void Greeting_ShouldBeSetInitially()
        {
            // Arrange & Act
            var viewModel = new MainViewModel();

            // Assert
            viewModel.Greeting.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Greeting_PropertyChanged_ShouldBeRaised()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.Greeting))
                    propertyChangedRaised = true;
            };

            // Act
            viewModel.Greeting = "New Greeting";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }
    }
    ```
  - **Verification**:
    - Run: `dotnet test --filter "MainViewModelTests"`
    - Expected: All tests pass
  - **Commit Message**: `test: add unit tests for MainViewModel`
  - **Estimated Time**: 30 minutes
  - **Dependencies**: None

#### REM047: Create SplashViewModel Unit Tests (Future)

- [ ] **REM047** Create unit tests for SplashViewModel when implemented
  - **Related Task**: T126 (SplashViewModel - future)
  - **Principle Violated**: Principle III - Test-First Development
  - **Severity**: ⚠️ WARNING (Non-Critical)
  - **Files**:
    - Create: `tests/unit/ViewModels/SplashViewModelTests.cs`
  - **Note**: This is for future implementation when SplashViewModel is created
  - **Verification**:
    - Run: `dotnet test --filter "SplashViewModelTests"`
  - **Commit Message**: `test: add unit tests for SplashViewModel`
  - **Estimated Time**: 30 minutes
  - **Dependencies**: SplashViewModel implementation

---

### Group 5: Code Coverage Configuration (Principle III - TDD)

#### REM048: Configure Code Coverage Tooling

- [ ] **REM048** Add code coverage measurement to test project
  - **Related Task**: T012 (test project setup)
  - **Principle Violated**: Principle III - Test-First Development (>80% coverage target)
  - **Severity**: ⚠️ WARNING (Non-Critical)
  - **Files**:
    - `tests/MTM_Template_Tests.csproj`
    - Create: `.github/workflows/coverage.yml` (if using GitHub Actions)
  - **Implementation**:
    ```xml
    <!-- Add to MTM_Template_Tests.csproj -->
    <ItemGroup>
      <PackageReference Include="coverlet.collector" Version="6.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
      <PackageReference Include="coverlet.msbuild" Version="6.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
      </PackageReference>
    </ItemGroup>
    ```
  - **Verification**:
    - Run: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`
    - Expected: Coverage report generated
    - Run: `dotnet tool install -g dotnet-reportgenerator-globaltool`
    - Run: `reportgenerator -reports:coverage.opencover.xml -targetdir:coverage`
    - Check: coverage/index.html shows coverage percentages
  - **Commit Message**: `test: add code coverage measurement with Coverlet`
  - **Estimated Time**: 1 hour
  - **Dependencies**: None

---

## Verification Checklist

After completing all remediation items, verify the following:

- [ ] All XAML files use `CompiledBinding` syntax (no legacy `{Binding}`)
- [ ] All XAML files have `x:DataType` attribute
- [ ] All async methods have `CancellationToken cancellationToken = default` parameter
- [ ] All service interfaces updated before implementations
- [ ] All implementations propagate cancellation tokens to downstream calls
- [ ] All tests updated to pass cancellation tokens
- [ ] Full test suite passes: `dotnet test`
- [ ] No build warnings: `dotnet build`
- [ ] DI container configured in Program.cs
- [ ] ViewModels resolved from DI container
- [ ] Unit tests for ViewModels exist
- [ ] Code coverage measured and visible

---

## Post-Remediation Actions

1. **Run Constitutional Audit Again**:
   ```powershell
   # Run audit to verify 100% compliance
   ./constitutional-audit.ps1 001-boot-sequence-splash
   ```

2. **Update Audit Status**:
   - Mark `AUDIT_001-boot-sequence-splash_2025-10-03.md` status as `✅ COMPLIANT`

3. **Update Feature Status**:
   - Mark feature as "Constitution Compliant" in project tracking

4. **Commit Remediation Checklist**:
   ```bash
   git add specs/001-boot-sequence-splash/REMEDIATION_001-boot-sequence-splash_2025-10-03.md
   git commit -m "docs: add constitutional remediation checklist for 001-boot-sequence-splash"
   ```

5. **Create Pull Request**:
   - Title: "Constitutional Remediation: 001-boot-sequence-splash"
   - Link to audit and remediation documents
   - Include compliance summary in PR description

---

## Notes

- **Atomic Commits**: Each REM item should be committed individually for traceability
- **Test First**: Run tests after each implementation fix to catch regressions early
- **Dependencies**: Some REM items depend on others (marked in Dependencies field)
- **Parallel Work**: REM items marked with `[P]` in original tasks can be done in parallel
- **Time Estimates**: Based on complexity; actual time may vary
- **Constitution Version**: This remediation is based on Constitution v1.1.0

---

**Generated**: 2025-10-03 | **Audit Reference**: AUDIT_001-boot-sequence-splash_2025-10-03.md | **Status**: 🔴 NOT STARTED
