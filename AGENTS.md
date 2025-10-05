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

1. **Spec-Driven Development**: All features follow `.specify/` workflow (SPEC → PLAN → TASKS → Implementation)
2. **Nullable Reference Types**: Enabled project-wide - use `?` explicitly, avoid `!` operator
3. **CompiledBinding Required**: Always use `x:DataType` and `{CompiledBinding}` in AXAML files
4. **Performance Budgets**: <10s boot time, <100MB memory, <3s service initialization
5. **Error Categorization**: All exceptions use `ErrorCategorizer` for structured recovery

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

## 📚 Additional Resources

### Key Documentation Files

- [Copilot Instructions](.github/copilot-instructions.md) - Development patterns & standards
- [Boot Sequence Guide](docs/BOOT-SEQUENCE.md) - Detailed startup architecture
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
