# AGENTS.md

**Optimized for: Claude Sonnet 4.5**

> This file provides AI coding agents with the context and instructions needed to work effectively on the MTM Avalonia Template project. For human-readable documentation, see [README.md](README.md).

## 🎯 Project Overview

**MTM Avalonia Template** is a cross-platform manufacturing data management application built with:
- **Framework**: C# .NET 9.0 with Avalonia UI 11.3+
- **Architecture**: MVVM with CommunityToolkit.Mvvm 8.3+
- **Platforms**: Windows Desktop, Android (Linux/macOS planned)
- **Data Sources**: MySQL 5.7 (MAMP), Visual ERP API (read-only), LZ4-compressed local cache
- **Key Features**: 3-stage boot sequence, offline-first operation, OS-native credential storage

### Critical Architecture Principles

1. **Spec-Driven Development**: All features follow `.specify/` workflow (**Performance Tests**: `tests/integration/PerformanceTests.cs`

## 🔍 Debug Terminal Diagnostic Patterns (Feature 003)

### Overview

The Debug Terminal provides real-time diagnostics for application performance, boot metrics, errors, and system state. AI agents can use Debug Terminal data to:
- Identify performance bottlenecks (memory leaks, slow boot stages)
- Diagnose errors and their patterns
- Validate performance budgets
- Troubleshoot configuration issues

### Interpreting Performance Snapshots

**Snapshot Structure** (`DiagnosticSnapshot`):
```csharp
public record DiagnosticSnapshot(
    DateTime Timestamp,
    double TotalMemoryMB,      // Total memory (private + shared)
    double PrivateMemoryMB,    // Application-exclusive memory
    long Stage0DurationMs,     // Splash screen stage
    long Stage1DurationMs,     // Core services stage
    long Stage2DurationMs,     // Application ready stage
    int ErrorCount,            // Cumulative errors
    string ErrorSummary,       // Recent error summary
    int ConnectionPoolActive,  // Visual API active connections
    int ConnectionPoolIdle,    // Visual API idle connections
    Dictionary<string, string> EnvironmentVariables
);
```

**Health Indicators**:

| Metric                      | Healthy Range | Warning     | Critical | Action                                               |
| --------------------------- | ------------- | ----------- | -------- | ---------------------------------------------------- |
| Private Memory              | <70MB         | 70-90MB     | >90MB    | Check for memory leaks, review object retention      |
| Total Memory                | <100MB        | 100-120MB   | >120MB   | Exceeded budget, optimize or increase                |
| Stage 0 Duration            | <1000ms       | 1000-1500ms | >1500ms  | Investigate splash screen initialization             |
| Stage 1 Duration            | <3000ms       | 3000-4000ms | >4000ms  | Profile service initialization (Config, Secrets, DB) |
| Stage 2 Duration            | <1000ms       | 1000-1500ms | >1500ms  | Check UI initialization, ViewModels                  |
| Error Count Trend           | 0             | 1-5         | >5       | Review error history, identify error sources         |
| Connection Pool Utilization | 20-80%        | 80-95%      | 95-100%  | Increase pool size or reduce concurrent calls        |

### Reading Boot Timeline

**Boot Timeline Entry** (`BootTimelineEntry`):
```csharp
public record BootTimelineEntry(
    int StageNumber,        // 0, 1, or 2
    string StageName,       // "Splash", "Core Services", "Application Ready"
    long DurationMs,        // Actual duration
    long TargetMs,          // Spec target
    bool MeetsTarget        // true if duration <= target
);
```

**Diagnostic Patterns**:

1. **Slow Boot (Total >10s)**:
   - Identify stage with highest duration
   - Stage 0 >1000ms: Check splash screen initialization, logging setup
   - Stage 1 >3000ms: Check service initialization (Config, Secrets, DB migrations)
   - Stage 2 >1000ms: Check UI initialization, ViewModel construction

2. **Boot Performance Regression**:
   - Compare current timeline with baseline (exported diagnostics)
   - Look for stage duration increases >20%
   - Common causes: New service dependencies, database migrations, network calls

3. **Inconsistent Boot Times**:
   - If boot time varies significantly between runs:
     - Check for network-dependent initialization (should be async + timeout)
     - Review database migrations (should skip if already applied)
     - Check for non-deterministic initialization order

### Analyzing Error History

**Error Log Entry** (`ErrorLogEntry`):
```csharp
public record ErrorLogEntry(
    DateTime Timestamp,
    ErrorSeverity Severity,  // Critical, Error, Warning, Info
    string Message,
    string? StackTrace
);
```

**Error Pattern Recognition**:

1. **Error Frequency Analysis**:
   - Single error: Likely one-time failure (network timeout, user input)
   - Repeating errors (same message): Systemic issue (configuration error, missing dependency)
   - Error bursts: Cascading failure (service down, connection pool exhausted)

2. **Severity Interpretation**:
   - **Critical**: Application unstable, data loss risk → Immediate attention
   - **Error**: Operation failed, user impact → Investigation required
   - **Warning**: Potential issue, operation succeeded → Monitor for patterns
   - **Info**: Informational only → Review for context

3. **Common Error Patterns**:

| Error Message Pattern               | Likely Cause                  | Diagnostic Steps                                   |
| ----------------------------------- | ----------------------------- | -------------------------------------------------- |
| `TimeoutException`                  | Network call exceeded timeout | Check connection pool, API endpoint availability   |
| `MySqlException: Unable to connect` | Database unavailable          | Verify MAMP running, check connection string       |
| `NullReferenceException`            | Missing null check            | Review stack trace, add null validation            |
| `UnauthorizedException`             | Invalid credentials           | Check secrets service, verify credential retrieval |
| `OutOfMemoryException`              | Memory budget exceeded        | Review snapshot history, check for leaks           |

### Connection Pool Diagnostics

**Connection Pool Stats** (`ConnectionPoolStats`):
```csharp
public record ConnectionPoolStats(
    int ActiveConnections,      // Currently in-use
    int IdleConnections,        // Available in pool
    int TotalConnections,       // Active + Idle
    double UtilizationPercentage // Active / Total * 100
);
```

**Healthy Pool Indicators**:
- **Idle > 0**: Always have connections available (prevents queuing)
- **Utilization 20-80%**: Efficient sizing, room for spikes
- **No stuck connections**: Active count decreases after operations complete

**Connection Pool Issues**:

1. **Utilization 100% + API Timeouts**:
   - **Cause**: Pool exhausted, requests queuing
   - **Fix**: Increase pool size in configuration (`MaxPoolSize` setting)
   - **Alternative**: Reduce concurrent API calls, add request throttling

2. **Utilization <10% Consistently**:
   - **Cause**: Over-provisioned pool
   - **Fix**: Reduce pool size to free resources
   - **Note**: Minimal impact, but reduces memory footprint

3. **Active Connections Not Returning to Idle**:
   - **Cause**: Connection leak (not disposed properly)
   - **Fix**: Review API client code for missing `using` statements or `Dispose()` calls
   - **Test**: Perform API operation, wait 30s, check if connections return to idle

### Diagnostic Export Analysis

**Export JSON Structure**:
```json
{
  "exportMetadata": {
    "timestamp": "2025-10-08T14:30:00Z",
    "appVersion": "1.0.0",
    "platform": "Windows 11"
  },
  "performanceSnapshots": [ /* Array of DiagnosticSnapshot */ ],
  "errorHistory": [ /* Array of ErrorLogEntry */ ],
  "bootMetrics": {
    "stage0DurationMs": 850,
    "stage1DurationMs": 2400,
    "stage2DurationMs": 750,
    "totalBootTimeMs": 4000
  },
  "connectionPool": { /* ConnectionPoolStats */ },
  "environmentVariables": [ /* Filtered list */ ]
}
```

**Analysis Workflows**:

1. **Performance Comparison** (Before/After Changes):
   ```powershell
   # Export baseline before changes
   $baseline = Get-Content diagnostics-before.json | ConvertFrom-Json

   # Export after changes
   $current = Get-Content diagnostics-after.json | ConvertFrom-Json

   # Compare boot times
   $bootDelta = $current.bootMetrics.totalBootTimeMs - $baseline.bootMetrics.totalBootTimeMs
   Write-Host "Boot time change: $bootDelta ms"

   # Compare memory usage
   $memoryBaseline = ($baseline.performanceSnapshots | Measure-Object -Property PrivateMemoryMB -Average).Average
   $memoryCurrent = ($current.performanceSnapshots | Measure-Object -Property PrivateMemoryMB -Average).Average
   $memoryDelta = $memoryCurrent - $memoryBaseline
   Write-Host "Memory change: $memoryDelta MB"
   ```

2. **Memory Leak Detection** (Trending Analysis):
   ```powershell
   $export = Get-Content diagnostics.json | ConvertFrom-Json

   # Calculate memory trend (should be flat for no leak)
   $snapshots = $export.performanceSnapshots | Sort-Object Timestamp
   $firstMemory = $snapshots[0].PrivateMemoryMB
   $lastMemory = $snapshots[-1].PrivateMemoryMB
   $memoryGrowth = $lastMemory - $firstMemory

   if ($memoryGrowth -gt 10) {
       Write-Host "⚠️ Potential memory leak: $memoryGrowth MB growth over $($snapshots.Count) snapshots"
   }
   ```

3. **Error Frequency Analysis**:
   ```powershell
   $export = Get-Content diagnostics.json | ConvertFrom-Json

   # Group errors by message
   $errorGroups = $export.errorHistory | Group-Object -Property Message

   # Find most frequent errors
   $errorGroups | Sort-Object Count -Descending | Select-Object -First 5 | ForEach-Object {
       Write-Host "Error: $($_.Name) - Count: $($_.Count)"
   }
   ```

### Environment Variable Diagnostics

**Security-Filtered Variables**:
- Variables containing `PASSWORD`, `TOKEN`, `SECRET`, `KEY`, `CONNECTION_STRING` show as `***FILTERED***`
- If filtered variable is needed for diagnosis, request access from user (never log actual values)

**Common Configuration Issues**:

| Missing Variable          | Impact                       | Solution                                      |
| ------------------------- | ---------------------------- | --------------------------------------------- |
| `VISUAL_API_ENDPOINT`     | Visual ERP integration fails | Set in app config or environment              |
| `MYSQL_CONNECTION_STRING` | Database unavailable         | Verify MAMP running, check connection details |
| `ASPNETCORE_ENVIRONMENT`  | Wrong configuration loaded   | Set to Development/Staging/Production         |
| `SERILOG_MINIMUM_LEVEL`   | Log verbosity incorrect      | Set to Debug/Information/Warning/Error        |

### Quick Actions for AI Agents

When analyzing Debug Terminal data, AI agents can suggest these Quick Actions to users:

1. **Clear Errors**: Before capturing clean diagnostic export
2. **Force GC**: Establish memory baseline before/after tests
3. **Reset Timeline**: Clear test data before production boot measurements
4. **Export Diagnostics**: Capture state for offline analysis or sharing with engineering

### Performance Optimization Checklist

When Debug Terminal shows performance issues:

- [ ] **Memory >90MB**: Review object retention, check for circular references, verify Dispose() calls
- [ ] **Boot >10s**: Profile service initialization, check for synchronous I/O, review database migrations
- [ ] **Connection Pool 100%**: Increase pool size or reduce concurrent API calls
- [ ] **Error Count >5**: Review error history, identify root cause, fix systemic issues
- [ ] **Stage Duration >Target**: Profile specific stage, identify slow operations, optimize or parallelize

### Related Documentation

- **User Guide**: `docs/USER-GUIDE-DEBUG-TERMINAL.md`
- **Implementation Details**: `docs/DEBUG-TERMINAL-IMPLEMENTATION.md`
- **Performance Analysis**: `specs/003-debug-terminal-modernization/PERFORMANCE-OPTIMIZATION.md`
- **Constitutional Compliance**: `specs/003-debug-terminal-modernization/CONSTITUTIONAL-AUDIT.md`

## 📚 Additional ResourcesC → PLAN → TASKS → Implementation)
2. **Nullable Reference Types**: Enabled project-wide - use `?` explicitly, avoid `!` operator
3. **CompiledBinding Required**: Always use `x:DataType` and `{CompiledBinding}` in AXAML files
4. **Performance Budgets**: <10s boot time, <100MB memory, <3s service initialization
5. **Error Categorization**: All exceptions use `ErrorCategorizer` for structured recovery

## 🤖 AI Agent Command Execution Pattern

**CRITICAL FOR AI AGENTS**: When you need to run console commands, **ALWAYS create executable scripts** in `.copilot-scripts/` directory instead of requesting user approval for each command. After creating the script run it.

### Script Creation Rules:

1. **Create `.ps1` files** in `.copilot-scripts/` directory
2. **Use descriptive names**: `build-*.ps1`, `test-*.ps1`, `check-*.ps1`, `fix-*.ps1`
3. **Run immediately** using: `pwsh -ExecutionPolicy Bypass -File ".copilot-scripts/script-name.ps1"`
4. **No approval needed** - Scripts execute automatically via Code Runner
5. **Scripts are git-ignored** - Won't clutter commits

### Example Pattern:

```powershell
# Instead of requesting approval for multiple commands:
# ❌ DON'T: Run individual commands requiring approval
dotnet build
dotnet test
git status

# ✅ DO: Create and run a script
# File: .copilot-scripts/build-and-test.ps1
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build MTM_Template_Application.sln
if ($LASTEXITCODE -eq 0) {
    Write-Host "Running tests..." -ForegroundColor Cyan
    dotnet test
}
git status

# Execute script (no approval needed)
pwsh -ExecutionPolicy Bypass -File ".copilot-scripts/build-and-test.ps1"
```

## 🚀 Quick Start Commands

### Essential Setup

```powershell
# Clone and restore
git clone https://github.com/Dorotel/MTM_Avalonia_Template.git
cd MTM_Avalonia_Template
dotnet restore MTM_Template_Application.sln

# Build entire solution
dotnet build MTM_Template_Application.sln

# Run desktop application
dotnet run --project MTM_Template_Application.Desktop/MTM_Template_Application.Desktop.csproj
```

### Development Workflow

```powershell
# Clean build
dotnet clean MTM_Template_Application.sln
dotnet build MTM_Template_Application.sln

# Watch mode (auto-rebuild)
dotnet watch --project MTM_Template_Application.Desktop/MTM_Template_Application.Desktop.csproj

# Build specific project
dotnet build MTM_Template_Application/MTM_Template_Application.csproj
```

### Android Development

```powershell
# Set environment variables (required for Android builds)
$env:ANDROID_SDK_ROOT = "$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
$env:JAVA_HOME = "C:\Program Files\Android\Android Studio\jbr"

# Build Android APK
dotnet build MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android

# Deploy to connected device
dotnet build MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android -t:Install

# Check connected devices
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" devices

# Monitor Android logs
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" logcat -s "MTM:*" "mono:*" "Avalonia:*"
```

## 🧪 Testing Instructions

### Running Tests

```powershell
# All tests (unit + integration + contract)
dotnet test MTM_Template_Application.sln

# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# Contract tests only
dotnet test --filter "Category=Contract"

# Performance tests
dotnet test --filter "Category=Performance"

# Specific test class
dotnet test --filter "FullyQualifiedName~BootOrchestratorTests"

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# With coverage (requires coverlet.collector)
dotnet test /p:CollectCoverage=true /p:CoverageOutputFormat=cobertura
```

### Test Structure

```
tests/
├── unit/                  # ViewModels, services, business logic
├── integration/           # Database, API, file system
├── contract/              # Visual ERP API contract validation
└── TestHelpers/           # Mocks, builders, fixtures
```

**Testing Patterns:**
- Use **xUnit** for test framework
- Use **FluentAssertions** for readable assertions: `result.Should().BeOfType<Success>()`
- Use **NSubstitute** for mocking: `Substitute.For<IService>()`
- Follow **AAA pattern**: Arrange, Act, Assert
- Test ViewModels without UI dependencies (pure MVVM testing)

### Validation Scripts

```powershell
# Validate current feature implementation (auto-detects from branch name)
.\.specify\scripts\powershell\validate-implementation.ps1

# Validate specific feature
.\.specify\scripts\powershell\validate-implementation.ps1 -FeatureId "001-boot-sequence-splash"

# Strict mode (fail on warnings)
.\.specify\scripts\powershell\validate-implementation.ps1 -Strict

# JSON output (for CI/CD integration)
.\.specify\scripts\powershell\validate-implementation.ps1 -Json
```

**Validation Criteria:**
- ✅ 100% task completion in TASKS_*.md
- ✅ All acceptance criteria met
- ✅ Clean build with zero errors
- ✅ All tests passing
- ✅ Constitutional compliance
- ✅ Documentation complete

## 📐 Code Style & Standards

### C# Conventions

#### Nullable Reference Types (CRITICAL)

```csharp
// ✅ CORRECT - Explicit nullability
public async Task<User?> GetUserAsync(string? userId, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(userId))
        return null;

    var user = await _repository.FindAsync(userId, cancellationToken);
    _logger?.LogInformation("Retrieved user {UserId}", userId);
    return user;
}

// ❌ INCORRECT - Missing nullability annotations
public async Task<User> GetUserAsync(string userId)
{
    return await _repository.FindAsync(userId)!; // Avoid ! operator
}
```

**Rules:**
- Use `?` for all nullable reference types
- Avoid `!` null-forgiving operator (only use when absolutely certain)
- Use null-conditional operators: `?.`, `??`, `??=`
- All async methods MUST have `CancellationToken` parameter

#### MVVM with CommunityToolkit.Mvvm

```csharp
// ✅ CORRECT - Modern source generator pattern
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _userName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
    private bool _isLoading;

    [RelayCommand(CanExecute = nameof(CanLoadData))]
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            // Load data with cancellation support
            var data = await _service.GetDataAsync(cancellationToken);
            // Process data
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLoadData() => !IsLoading;
}
```

**Rules:**
- Use `[ObservableProperty]` instead of manual property implementation
- Use `[RelayCommand]` for all commands (auto-generates `ICommand` properties)
- ViewModels inherit from `ObservableObject` or `ObservableRecipient`
- Use `partial` class modifier for source generators
- Never use ReactiveUI patterns (no `ReactiveObject`, `ReactiveCommand`, etc.)

### Avalonia XAML Conventions (CRITICAL)

```xml
<!-- ✅ CORRECT - CompiledBinding with x:DataType -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Template_Application.ViewModels"
        x:Class="MTM_Template_Application.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        x:CompileBindings="True"
        Design.Width="800" Design.Height="450">
    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <StackPanel>
        <TextBlock Text="{CompiledBinding UserName}" />
        <Button Content="Load" Command="{CompiledBinding LoadDataCommand}" />
        <ProgressBar IsVisible="{CompiledBinding IsLoading}" />
    </StackPanel>
</Window>

<!-- ❌ INCORRECT - Missing x:DataType or using Binding -->
<TextBlock Text="{Binding UserName}" /> <!-- Don't use {Binding} -->
<TextBlock Text="{ReflectionBinding UserName}" /> <!-- Don't use ReflectionBinding -->
```

**MANDATORY XAML Rules:**
1. **Always** set `x:DataType="vm:YourViewModel"` on root element (Window/UserControl)
2. **Always** set `x:CompileBindings="True"` on root element
3. **Always** use `{CompiledBinding PropertyName}` syntax (never `{Binding}`)
4. **Always** include `Design.DataContext` for previewer support
5. Use `Design.Width` and `Design.Height` for design-time layout

### Dependency Injection Pattern

```csharp
// Program.cs - Service registration
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace()
        .ConfigureServices(services =>
        {
            // ViewModels (transient - new instance each time)
            services.AddTransient<MainViewModel>();
            services.AddTransient<SplashViewModel>();

            // Services (singleton - shared instance)
            services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ISecretsService>(sp =>
                SecretsServiceFactory.Create(sp.GetRequiredService<ILoggerFactory>()));

            // Logging
            services.AddLogging(builder => builder.AddSerilog());
        });
}
```

### Database Patterns (MySQL)

```csharp
// ✅ CORRECT - Parameterized queries with async/await
public async Task<List<Order>> GetOrdersAsync(string customerId, CancellationToken cancellationToken)
{
    await using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);

    await using var command = new MySqlCommand(
        "SELECT * FROM orders WHERE customer_id = @customerId",
        connection
    );
    command.Parameters.AddWithValue("@customerId", customerId);

    var orders = new List<Order>();
    await using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
        orders.Add(MapOrderFromReader(reader));
    }
    return orders;
}

// ❌ INCORRECT - String concatenation (SQL injection risk)
var command = new MySqlCommand($"SELECT * FROM orders WHERE customer_id = '{customerId}'", connection);
```

### Error Handling with Polly

```csharp
// Retry policy with exponential backoff
private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
    Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}ms due to {Reason}",
                    retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
            });

// Usage
var response = await _retryPolicy.ExecuteAsync(
    ct => _httpClient.GetAsync("/api/endpoint", ct),
    cancellationToken
);
```

## 🏗️ Project Structure

```
MTM_Avalonia_Template/
├── MTM_Template_Application/          # Shared Avalonia UI library
│   ├── ViewModels/                    # MVVM ViewModels (ObservableObject)
│   ├── Views/                         # Avalonia XAML views (.axaml)
│   ├── Models/                        # Domain models & DTOs
│   │   ├── Boot/                      # Boot-related models
│   │   ├── Cache/                     # Cache entry models
│   │   ├── Configuration/             # Config models
│   │   └── Core/                      # Shared core models
│   ├── Services/                      # Business logic & infrastructure
│   │   ├── Boot/                      # BootOrchestrator, stages
│   │   ├── Configuration/             # ConfigurationService
│   │   ├── Secrets/                   # Platform-specific secrets
│   │   ├── Logging/                   # Serilog configuration
│   │   ├── Diagnostics/               # Health checks
│   │   ├── DataLayer/                 # MySQL, Visual API, HttpClient
│   │   ├── Cache/                     # LZ4 cache service
│   │   ├── Core/                      # Message bus, validation
│   │   ├── Localization/              # i18n resources
│   │   ├── Theme/                     # Theme management
│   │   └── Navigation/                # Navigation service
│   ├── Behaviors/                     # Avalonia behaviors
│   ├── Extensions/                    # DI extension methods
│   ├── Assets/                        # Icons, images
│   └── App.axaml                      # Application entry point
├── MTM_Template_Application.Desktop/  # Desktop-specific launcher
│   ├── Program.cs                     # Desktop entry point
│   └── Services/                      # Desktop-specific services
├── MTM_Template_Application.Android/  # Android-specific launcher
│   ├── MainActivity.cs                # Android entry point
│   └── Services/                      # Android-specific services
├── tests/                             # Test project
│   ├── unit/                          # Unit tests
│   ├── integration/                   # Integration tests
│   ├── contract/                      # Contract tests
│   └── TestHelpers/                   # Test utilities
├── specs/                             # Feature specifications
│   ├── 001-boot-sequence-splash/      # Feature 001 specs
│   └── 002-environment-and-configuration/ # Feature 002 specs
├── docs/                              # Documentation
└── .specify/                          # Spec-Kit workflow files
    ├── features/                      # SPEC/PLAN/TASKS files
    ├── scripts/powershell/            # Validation scripts
    ├── templates/                     # Command templates
    └── memory/constitution.md         # Project principles
```

## 🔧 Spec-Driven Development Workflow

This project uses **GitHub Spec Kit** for structured feature development.

### Feature Development Commands

```powershell
# Create new feature specification
.\.specify\scripts\powershell\create-new-feature.ps1 -FeatureId "003-my-feature" -FeatureName "My Feature Name"

# Validate implementation readiness
.\.specify\scripts\powershell\validate-implementation.ps1

# Constitutional audit (check alignment with project principles)
.\.specify\scripts\powershell\constitutional-audit.ps1

# Update Copilot context from feature plans
.\.specify\scripts\powershell\update-agent-context.ps1
```

### Feature Workflow

1. **Specify** (`SPEC_*.md`): Functional requirements, acceptance criteria, success metrics
2. **Plan** (`PLAN_*.md`): Technical architecture, implementation approach, risks
3. **Tasks** (`TASKS_*.md`): Granular task breakdown with completion tracking
4. **Implement**: Follow tasks, update progress, run tests
5. **Validate**: Run validation script, ensure 100% completion
6. **Review**: Constitutional audit, code review, merge

### Key Spec-Kit Prompts

Reference these prompts when working with specifications:
- `/constitution` - View project governing principles
- `/specify` - Create new feature specification
- `/clarify` - De-risk ambiguous requirements
- `/plan` - Generate technical implementation plan
- `/tasks` - Break down into actionable tasks
- `/analyze` - Check cross-artifact consistency
- `/implement` - Execute implementation

## 🔒 Security & Secrets Management

### Credential Storage

```csharp
// Platform-specific credential storage (never hardcode secrets)
var secretsService = SecretsServiceFactory.Create(loggerFactory);

// Store credential
await secretsService.StoreSecretAsync("VisualERP.ApiKey", "your-api-key", cancellationToken);

// Retrieve credential
var apiKey = await secretsService.RetrieveSecretAsync("VisualERP.ApiKey", cancellationToken);

// Delete credential
await secretsService.DeleteSecretAsync("VisualERP.ApiKey", cancellationToken);
```

**Platform Implementations:**
- **Windows**: DPAPI via `CredentialManager`
- **Android**: `KeyStore` API
- **macOS/iOS**: Keychain Services (planned)
- **Linux**: Secret Service API (planned)

### Security Rules

- ❌ Never log secrets, API keys, passwords, or PII
- ❌ Never commit secrets to version control
- ✅ Always use OS-native credential storage
- ✅ Validate all user inputs with FluentValidation
- ✅ Use HTTPS for all external API calls
- ✅ Parameterize all SQL queries (prevent injection)

## 📊 Performance Guidelines

### Boot Performance Targets

| Stage     | Target   | Description                          |
| --------- | -------- | ------------------------------------ |
| Stage 0   | <1s      | Splash screen display                |
| Stage 1   | <3s      | Service initialization (10 services) |
| Stage 2   | <1s      | Application shell construction       |
| **Total** | **<10s** | **End-to-end boot time**             |

### Memory Budget

- **Cache**: ~40MB (LZ4 compressed, ~3:1 ratio)
- **Services**: ~30MB (DI container, connection pools)
- **Framework**: ~30MB (Avalonia UI, .NET runtime)
- **Total**: <100MB during startup

### Performance Best Practices

- Use `VirtualizingStackPanel` for lists >100 items
- Use `ItemsRepeater` for custom virtualization
- Avoid binding to complex properties in loops
- Use `x:CompileBindings` (faster than ReflectionBinding)
- Profile with dotMemory/dotTrace before optimizing

## 🐛 Debugging & Troubleshooting

### Common Issues

**Issue: AVLN2000 binding errors in XAML**

```
Solution: Ensure x:DataType is set and property exists on ViewModel
- Check x:CompileBindings="True" is present
- Verify property is public and uses [ObservableProperty]
- Ensure namespace is imported: xmlns:vm="using:YourNamespace.ViewModels"
```

**Issue: "Cannot resolve symbol" in AXAML**

```
Solution: Rebuild project and restart Avalonia XAML previewer
- Run: dotnet build
- Close and reopen XAML file
- Check that ViewModel class is public
```

**Issue: NullReferenceException in ViewModel**

```
Solution: Check DI registration and nullable annotations
- Verify service is registered in Program.cs
- Ensure constructor parameters match registered services
- Add null checks for optional dependencies
```

**Issue: Android APK won't install**

```
Solution: Check Android SDK paths and device connection
- Verify ANDROID_SDK_ROOT environment variable
- Run: adb devices (should show device)
- Uninstall previous version first
- Check device has USB debugging enabled
```

### Logging & Diagnostics

```powershell
# View application logs
Get-Content logs/app-$(Get-Date -Format yyyyMMdd).txt -Tail 50 -Wait

# Filter for errors only
Select-String -Path "logs/*.txt" -Pattern "ERROR|FATAL"

# Check boot sequence timing
Select-String -Path "logs/*.txt" -Pattern "Stage.*completed in"
```

### Avalonia DevTools

Press **F12** while application is running (Debug mode only) to open DevTools:
- **Visual Tree**: Inspect control hierarchy
- **Property Inspector**: View computed styles and properties
- **Events**: Monitor routed events
- **Resources**: Inspect resource dictionaries

## 📝 Pull Request Guidelines

### PR Title Format

```
[feature-id] Brief description

Examples:
[001] Fix splash screen timeout handling
[002] Add configuration precedence tests
[shared] Update MVVM toolkit to 8.4.0
```

### Pre-Commit Checklist

```powershell
# 1. Build solution (zero warnings)
dotnet build MTM_Template_Application.sln

# 2. Run all tests (all passing)
dotnet test

# 3. Validate implementation (if feature complete)
.\.specify\scripts\powershell\validate-implementation.ps1

# 4. Check for constitutional compliance
.\.specify\scripts\powershell\constitutional-audit.ps1

# 5. Update documentation if needed
# - Update .github/copilot-instructions.md if adding new patterns
# - Update AGENTS.md if changing workflow
# - Update feature TASKS_*.md with completion status
```

### Code Review Focus Areas

1. **Nullable safety**: Proper `?` annotations, no unnecessary `!` operators
2. **XAML bindings**: CompiledBinding with x:DataType everywhere
3. **Async patterns**: CancellationToken support, no blocking calls
4. **Error handling**: Comprehensive try-catch with ErrorCategorizer
5. **Testing**: Unit tests for ViewModels, integration tests for services
6. **Performance**: Stays within budgets (<10s boot, <100MB memory)

## 🎨 Theme & Styling

### Theme V2 Semantic Tokens

```xml
<!-- Use semantic tokens from Theme V2 (see .github/instructions/Themes.instructions.md) -->
<Border Background="{DynamicResource ThemeV2.Input.Background}"
        BorderBrush="{DynamicResource ThemeV2.Input.Border}"
        CornerRadius="{DynamicResource ThemeV2.CornerRadius.Medium}">
    <TextBox Classes="ManufacturingInput" />
</Border>
```

### Available Themes

- **Light**: Default light mode
- **Dark**: Default dark mode
- **Auto**: Follow OS preference
- **HighContrast**: Accessibility mode

Theme selection persists in user configuration.

## � Configuration & Secrets Management Patterns (Feature 002)

### Configuration Service Pattern

The application uses **layered configuration precedence** for flexible deployment:

```csharp
// Configuration precedence (highest to lowest):
// 1. Environment Variables (MTM_*, ASPNETCORE_*, DOTNET_*)
// 2. User Configuration (runtime SetValue, persisted to MySQL)
// 3. Application Defaults (hard-coded fallbacks)

// Get configuration value with type safety
var timeout = _configurationService.GetValue<int>("API:Timeout", defaultValue: 30);

// Set user preference (triggers OnConfigurationChanged event)
_configurationService.SetValue("UI:Theme", "Dark");

// Listen for configuration changes
_configurationService.OnConfigurationChanged += (sender, args) =>
{
    _logger.LogInformation("Config changed: {Key} = {NewValue}", args.Key, args.NewValue);
};
```

**Key Principles**:
- Use `IConfigurationService` interface (DI-friendly)
- Type-safe access via `GetValue<T>()`
- Change detection prevents unnecessary UI re-renders
- Database persistence for user preferences
- Environment variables read once at startup (not runtime polling)

### Secrets Management Pattern

OS-native credential storage with platform-specific implementations:

```csharp
// Factory pattern for platform detection
var secretsService = SecretsServiceFactory.Create(loggerFactory);

// Store credential (encrypted in OS-native storage)
await secretsService.StoreSecretAsync("Visual.Username", username, cancellationToken);
await secretsService.StoreSecretAsync("Visual.Password", password, cancellationToken);

// Retrieve credential (decrypted from OS storage)
var username = await secretsService.RetrieveSecretAsync("Visual.Username", cancellationToken);

// Delete credential
await secretsService.DeleteSecretAsync("Visual.Username", cancellationToken);
```

**Platform Implementations**:
- **Windows**: `WindowsSecretsService` (DPAPI via Credential Manager)
- **Android**: `AndroidSecretsService` (KeyStore with hardware-backed encryption)
- **Unsupported platforms**: Throws `PlatformNotSupportedException`

**Error Handling**:
- Storage unavailable → User-friendly dialog prompting credential re-entry
- Dialog cancellation → Application closes with clear warning (FR-013)
- Corrupted credentials → Automatic recovery flow (see `docs/CREDENTIAL-RECOVERY-FLOW.md`)

### Feature Flag Evaluation Pattern

Deterministic feature flag evaluation with environment-specific configurations:

```csharp
// Register feature flag with environment and rollout percentage
evaluator.RegisterFlag(new FeatureFlag
{
    FlagName = "Visual.UseForItems",
    IsEnabled = true,
    Environment = "Production",
    RolloutPercentage = 50 // 50% of users see this feature
});

// Check if enabled (deterministic per userId)
bool enabled = await evaluator.IsEnabledAsync("Visual.UseForItems", userId, cancellationToken);

// Update flag state
await evaluator.SetEnabledAsync("Visual.UseForItems", true, cancellationToken);

// Refresh flags from database
await evaluator.RefreshFlagsAsync(cancellationToken);
```

**Key Behaviors**:
- **Unregistered flags return false** (disabled) + log warning
- **Rollout percentage uses hash-based determinism** (same userId always sees same result)
- **Environment filtering**: Only flags for current environment are active
- **Launch-time sync**: Flags loaded from MySQL at startup, no hot-reload
- **Validation**: Flag names must match `^[a-zA-Z0-9._-]+$` regex

### Database Schema Management

Always reference `.github/mamp-database/schema-tables.json` before database operations:

```csharp
// ✅ CORRECT - Parameterized queries with exact table/column names
await using var command = new MySqlCommand(
    "SELECT PreferenceKey, PreferenceValue FROM UserPreferences WHERE UserId = @userId",
    connection
);
command.Parameters.AddWithValue("@userId", userId);

// ❌ INCORRECT - String concatenation (SQL injection risk)
var command = new MySqlCommand($"SELECT * FROM UserPreferences WHERE UserId = {userId}", connection);
```

**Tables from Feature 002**:
- `Users` (UserId PK, Username, DisplayName, IsActive, CreatedAt, LastLoginAt)
- `UserPreferences` (PreferenceId PK, UserId FK, PreferenceKey, PreferenceValue, LastUpdated)
- `FeatureFlags` (FlagId PK, FlagName UNIQUE, IsEnabled, Environment, RolloutPercentage, UpdatedAt)

**Critical Rules**:
- Table names are case-sensitive: `Users`, `UserPreferences`, `FeatureFlags`
- Column names are case-sensitive: `UserId`, `PreferenceKey`, `FlagName`
- Always use parameterized queries (never string concatenation)
- Update `schema-tables.json` immediately after schema changes
- Track migrations in `migrations-history.json`

### Environment Variable Handling

Environment variables follow strict precedence and naming conventions:

```csharp
// Precedence order (startup-only detection):
// 1. MTM_ENVIRONMENT (app-specific)
// 2. ASPNETCORE_ENVIRONMENT (ASP.NET default)
// 3. DOTNET_ENVIRONMENT (generic .NET)
// 4. Build configuration (DEBUG → Development, RELEASE → Production)
// 5. Defaults (Development)

// Naming convention: MTM_ prefix for app-specific variables
Environment.GetEnvironmentVariable("MTM_DATABASE_SERVER");
Environment.GetEnvironmentVariable("MTM_VISUAL_API_BASE_URL");
Environment.GetEnvironmentVariable("MTM_FEATURE_FLAG_OFFLINE_MODE");
```

**Key Principles**:
- Read **once at startup** (not runtime polling)
- App-specific variables (`MTM_*`) override framework defaults
- Underscore format (not colon): `MTM_DATABASE_SERVER` (not `MTM:Database:Server`)

### Error Categorization Pattern

Severity-based error handling with user-friendly recovery:

```csharp
public enum ErrorSeverity
{
    Info,       // Status bar notification
    Warning,    // Status bar with click-for-details
    Critical    // Modal dialog blocking interaction
}

// Usage
try
{
    await _secretsService.RetrieveSecretAsync(key, cancellationToken);
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Storage unavailable"))
{
    // Critical error - show modal recovery dialog
    _logger.LogError(ex, "OS-native credential storage unavailable");
    await ShowCredentialRecoveryDialogAsync();
}
catch (Exception ex)
{
    // Warning - show status bar notification
    _logger.LogWarning(ex, "Non-critical error retrieving credential");
    ShowStatusBarWarning("Credential retrieval failed. Using cached data.");
}
```

**Recovery Strategies**:
- **Configuration errors**: Fall back to defaults + log warning
- **Credential errors**: Prompt user for re-entry (see `docs/CREDENTIAL-RECOVERY-FLOW.md`)
- **Database unavailable**: Use cached values + graceful degradation
- **Feature flag errors**: Treat as disabled + log warning

### Graceful Degradation Pattern

Application continues operating when dependencies are unavailable:

```csharp
// Database unavailable
try
{
    await _configurationService.LoadUserPreferencesAsync(userId, cancellationToken);
}
catch (MySqlException ex)
{
    _logger.LogWarning(ex, "Database unavailable - using cached preferences");
    // Continue with in-memory defaults
}

// Feature flags unavailable
try
{
    await _featureFlagEvaluator.RefreshFlagsAsync(cancellationToken);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Feature flag sync failed - using cached flags");
    // Continue with last-known flag state from local JSON
}
```

**Offline Capabilities**:
- Configuration: Environment variables → User config (cached) → Defaults
- User Preferences: Last-known cached values (persisted to local JSON)
- Feature Flags: Last-known cached values (persisted to local JSON)
- Credentials: Must be available (cannot proceed without)

### Performance Targets (Feature 002)

Configuration and secrets operations have strict performance budgets:

| Operation                   | Target | Measured In                          |
| --------------------------- | ------ | ------------------------------------ |
| Configuration retrieval     | <100ms | `GetValue<T>()` with 50+ keys        |
| Credential retrieval        | <200ms | `RetrieveSecretAsync()` (OS storage) |
| Feature flag evaluation     | <5ms   | `IsEnabledAsync()` (in-memory cache) |
| Configuration change events | <50ms  | Event dispatch to subscribers        |
| User preference persistence | <500ms | MySQL insert/update query            |

**Performance Tests**: `tests/integration/PerformanceTests.cs`

## �📚 Additional Resources

### Key Documentation Files

- [Copilot Instructions](.github/copilot-instructions.md) - Development patterns & standards
- [Boot Sequence Guide](<docs/BOOT-SEQUENCE (Complete!).md>) - Detailed startup architecture
- [Troubleshooting Catalog](docs/TROUBLESHOOTING-CATALOG.md) - Common issues & solutions
- [Themes Guide](.github/instructions/Themes.instructions.md) - Theme V2 usage
- [Spec-Kit Guides](docs/Specify%20Guides/) - Complete spec workflow documentation

### Package Versions (Central Management)

All package versions are managed in `Directory.Packages.props`:
- Avalonia: 11.3.6
- CommunityToolkit.Mvvm: 8.4.0
- .NET: 9.0
- Serilog: 8.0.0
- Polly: 8.4.2
- xUnit: 2.9.2

When adding new packages, always add version to `Directory.Packages.props` first.

## ⚠️ Common Pitfalls to Avoid

| ❌ Don't                                     | ✅ Do                                         |
| ------------------------------------------- | -------------------------------------------- |
| Use `{Binding}` in XAML                     | Use `{CompiledBinding}` with `x:DataType`    |
| Omit `?` for nullable types                 | Explicitly mark all nullable reference types |
| Use `!` null-forgiving operator             | Use null checks or `?.` operator             |
| Forget `CancellationToken` in async methods | Always include cancellation support          |
| Block async with `.Result` or `.Wait()`     | Use `await` everywhere                       |
| Concatenate SQL strings                     | Use parameterized queries                    |
| Log sensitive data                          | Use structured logging without secrets       |
| Use ReactiveUI patterns                     | Use CommunityToolkit.Mvvm patterns           |
| Skip validation script                      | Run validation before PR                     |

---

**Last Updated**: October 5, 2025
**Optimized For**: Claude Sonnet 4.5
**Project Version**: .NET 9.0 / Avalonia 11.3.6
**Maintainer**: John Koll ([@Dorotel](https://github.com/Dorotel))
