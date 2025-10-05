# MTM_Avalonia_Template Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-03

## Spec-Driven Development Context

This project uses **GitHub Spec Kit** for Spec-Driven Development (SDD).

### Spec-Kit Workflow Commands
When working with specifications, always reference:
- `/constitution` - Project principles and guidelines
- `/specify` - Feature specification creation
- `/clarify` - De-risk ambiguous areas before planning
- `/plan` - Technical implementation plans
- `/tasks` - Generate actionable task lists
- `/analyze` - Cross-artifact consistency analysis
- `/implement` - Execute implementation

### Spec-Kit Directory Structure
- `.specify/features/` - All feature specifications (SPEC_*.md, PLAN_*.md, TASKS_*.md)
- `.specify/templates/` - Command templates
- `memory/constitution.md` - Project governing principles
- `scripts/bash/` - Shell scripts for spec operations
- `scripts/powershell/` - PowerShell scripts for spec operations

### Script Execution Rules
- Scripts **always** run from repository root
- Scripts output **JSON** with file paths and branch names
- **Always parse JSON output** before using paths or proceeding with implementation
- Example: `create-new-feature.sh --json "{ARGS}"` returns `{"BRANCH_NAME": "...", "SPEC_FILE": "..."}`

## Active Technologies
- [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION] + [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION] (002-environment-and-configuration)
- [if applicable, e.g., PostgreSQL, CoreData, files or N/A] (002-environment-and-configuration)

### Core Stack (001-boot-sequence-splash)
- **Framework**: C# .NET 9.0 with nullable reference types enabled
- **UI Framework**: Avalonia 11.3+
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.3+
- **Database**: MySQL.Data (MAMP MySQL 5.7)
- **HTTP Client**: HttpClient (Visual API Toolkit integration)
- **Observability**: Serilog + OpenTelemetry
- **Resilience**: Polly
- **Mapping**: AutoMapper
- **Validation**: FluentValidation
- **Compression**: LZ4 for local cache
- **Security**: OS-native credential storage

### Data Sources
- MySQL 5.7 (MAMP) - Local database
- Visual ERP - Read-only via API Toolkit
- Local cache with LZ4 compression

## Project Structure

```
MTM_Avalonia_Template/
├── .specify/
│   ├── features/           # Feature specs, plans, tasks
│   ├── templates/          # Command templates
│   └── memory/
│       └── constitution.md # Project principles
├── src/
│   ├── MTM_Avalonia_Template/          # Main Avalonia app
│   │   ├── ViewModels/                 # MVVM ViewModels
│   │   ├── Views/                      # Avalonia XAML views
│   │   ├── Models/                     # Domain models
│   │   ├── Services/                   # Business logic & API clients
│   │   ├── Infrastructure/             # Database, caching, logging
│   │   ├── App.axaml                   # Application entry
│   │   └── Program.cs                  # Entry point
│   └── MTM_Avalonia_Template.Core/     # Shared/core logic (if needed)
├── tests/
│   ├── MTM_Avalonia_Template.Tests/    # Unit tests
│   └── MTM_Avalonia_Template.IntegrationTests/
├── scripts/
│   ├── bash/               # Linux/macOS scripts
│   └── powershell/         # Windows scripts
└── .github/
    └── copilot-instructions.md
```

## Code Style & Standards

### C# .NET 9.0 Conventions

#### Nullable Reference Types
- **Required**: All projects have nullable reference types enabled
- Always use `?` for nullable reference types
- Use `!` null-forgiving operator only when you're certain (avoid when possible)
- Prefer null-conditional operators: `?.`, `??`, `??=`

```csharp
// ✅ Good
public class UserService
{
    private readonly ILogger<UserService>? _logger;

    public async Task<User?> GetUserAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _repository.FindAsync(userId);
        _logger?.LogInformation("Retrieved user {UserId}", userId);
        return user;
    }
}

// ❌ Bad
public async Task<User> GetUserAsync(string userId)
{
    return await _repository.FindAsync(userId)!; // Avoid !
}
```

#### MVVM with CommunityToolkit.Mvvm
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands (supports async with `[RelayCommand(CanExecute = nameof(MethodName))]`)
- ViewModels inherit from `ObservableObject` or `ObservableRecipient`
- Use `partial` classes for source generators
- Commands automatically support `CanExecute` when suffixed with `CanExecuteMethodName`

```csharp
// ✅ Good - Modern CommunityToolkit.Mvvm pattern
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _userName;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadDataCommand))]
    private bool _canLoad = true;

    [RelayCommand(CanExecute = nameof(CanLoadData))]
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        CanLoad = false;
        try
        {
            // Load data with cancellation support
            await Task.Delay(1000, cancellationToken);
        }
        finally
        {
            IsLoading = false;
            CanLoad = true;
        }
    }

    private bool CanLoadData() => CanLoad && !IsLoading;
}
```

#### Avalonia XAML Conventions (CRITICAL)

**Always use CompiledBinding** - This is the recommended and performant approach:

```xml
<!-- ✅ CORRECT - Use x:DataType and CompiledBinding -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Avalonia_Template.ViewModels"
        x:Class="MTM_Avalonia_Template.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        x:CompileBindings="True"
        Design.Width="800" Design.Height="450">

    <StackPanel>
        <TextBlock Text="{CompiledBinding UserName}" />
        <Button Content="Load" Command="{CompiledBinding LoadDataCommand}" />
        <ProgressBar IsVisible="{CompiledBinding IsLoading}" />
    </StackPanel>
</Window>

<!-- ❌ WRONG - Don't use Binding without x:CompileBindings -->
<TextBlock Text="{Binding UserName}" />

<!-- ⚠️ Only use ReflectionBinding if absolutely necessary (slower, runtime errors) -->
<TextBlock Text="{ReflectionBinding UserName}" />
```

**Key Avalonia XAML Rules:**
- **Always** set `x:DataType` on Window/UserControl root
- **Always** set `x:CompileBindings="True"` for compile-time binding validation
- Use `CompiledBinding` syntax (Avalonia 11.0+)
- Use `Design.DataContext` for design-time preview
- Use `Design.Width` and `Design.Height` for previewer

```xml
<!-- ✅ Complete example with design-time support -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:MTM_Avalonia_Template.ViewModels"
             mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
             x:Class="MTM_Avalonia_Template.Views.MainView"
             x:DataType="vm:MainViewModel"
             x:CompileBindings="True">
    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <Grid RowDefinitions="Auto,*">
        <TextBlock Grid.Row="0" Text="{CompiledBinding Title}" />
        <ListBox Grid.Row="1" ItemsSource="{CompiledBinding Items}" />
    </Grid>
</UserControl>
```

#### Avalonia Styles & Resources
- Define styles in separate `.axaml` files
- Use `StyleInclude` to import styles
- Follow Avalonia's resource dictionary conventions

```xml
<!-- App.axaml -->
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MTM_Avalonia_Template.App">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="/Styles/CustomStyles.axaml" />
    </Application.Styles>
</Application>
```

#### Async/Await Patterns
- Always use `async`/`await` for I/O operations
- Use `ConfigureAwait(false)` in library code (not UI code)
- Suffix async methods with `Async`
- Use `ValueTask<T>` for hot paths when appropriate
- **Always** provide `CancellationToken` parameters for async operations

```csharp
// ✅ Good - Proper async pattern with cancellation
public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromSeconds(30));

    return await _httpClient
        .GetFromJsonAsync<List<Order>>("/api/orders", cts.Token)
        .ConfigureAwait(false);
}

// ❌ Bad - No cancellation support
public async Task<List<Order>> GetOrdersAsync()
{
    return await _httpClient.GetFromJsonAsync<List<Order>>("/api/orders");
}
```

#### Dependency Injection (Avalonia-specific)
- Register services in `Program.cs` using the `AppBuilder`
- Use constructor injection
- Use interfaces for testability

```csharp
// ✅ Good - Program.cs with DI setup
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .WithInterFont()
        .ConfigureServices(services =>
        {
            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register Services
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<ILogger<OrderService>>(
                LoggerFactory.Create(b => b.AddSerilog()).CreateLogger<OrderService>()
            );
        });
}
```

#### Error Handling with Polly
- Use retry policies for transient failures
- Use circuit breakers for cascading failures
- Log all retry attempts with Serilog

```csharp
// ✅ Good
private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
    Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
            });
```

#### Logging with Serilog (Avalonia Context)
- Use structured logging
- Log to file and console for desktop apps
- Include correlation IDs for distributed tracing

```csharp
// ✅ Good - Serilog configuration for Avalonia
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MTM_Avalonia_Template")
    .CreateLogger();

_logger.LogInformation(
    "Processing order {OrderId} for customer {CustomerId}",
    orderId,
    customerId
);
```

#### Validation with FluentValidation
- Create validator classes for DTOs/models
- Use async validation when needed
- Chain validation rules

```csharp
// ✅ Good
public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be positive");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress()
            .WithMessage("Valid email required");
    }
}
```

### Database (MySQL)
- Use parameterized queries **always** (prevent SQL injection)
- Use transactions for multi-step operations
- Close connections properly (use `using` statements)
- Connection strings stored in OS-native credential storage
- Use async methods: `OpenAsync()`, `ExecuteReaderAsync()`, etc.

```csharp
// ✅ Good
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync(cancellationToken);

await using var command = new MySqlCommand(
    "SELECT * FROM orders WHERE customer_id = @customerId",
    connection
);
command.Parameters.AddWithValue("@customerId", customerId);

await using var reader = await command.ExecuteReaderAsync(cancellationToken);
```

### Testing Standards
- Unit tests for business logic and ViewModels
- Integration tests for API/database operations
- Use **xUnit** (recommended for .NET)
- Mock external dependencies with **NSubstitute** or Moq
- Aim for >80% code coverage on critical paths
- Test ViewModels without UI dependencies

```csharp
// ✅ Good - ViewModel unit test
public class MainViewModelTests
{
    [Fact]
    public async Task LoadDataCommand_ShouldSetIsLoading()
    {
        // Arrange
        var mockService = Substitute.For<IDataService>();
        var viewModel = new MainViewModel(mockService);

        // Act
        await viewModel.LoadDataCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.IsLoading); // Should be false after completion
    }
}
```

## Avalonia-Specific Best Practices

### Performance Optimization
- Use `VirtualizingStackPanel` for large lists
- Use `ItemsRepeater` for custom virtualization
- Avoid binding to complex properties in tight loops
- Use `x:CompileBindings` for compile-time validation and performance

### Cross-Platform Considerations
- Test on Windows, Linux, and macOS
- Use `RuntimeInformation.IsOSPlatform()` for platform-specific code
- Use Avalonia's platform abstractions (don't P/Invoke directly)

```csharp
// ✅ Good - Platform detection
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows-specific code
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    // Linux-specific code
}
```

### Avalonia Community Packages
Consider using these vetted packages:
- `Avalonia.Controls.DataGrid` - Official DataGrid control
- `Avalonia.Controls.TreeDataGrid` - TreeView + DataGrid hybrid
- `AvaloniaEdit` - Code/text editor control
- `Avalonia.Xaml.Behaviors` - Behaviors library (like WPF)
- `Material.Avalonia` or `Citrus.Avalonia` - Material/Fluent themes

## Recent Changes
- 002-environment-and-configuration: Added [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION] + [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]
- **2025-10-03**: Added Avalonia-specific best practices from AvaloniaUI organization
- **2025-10-03**: Corrected XAML binding syntax to use `CompiledBinding` with `x:DataType`

## Performance Guidelines
- Use LZ4 compression for cached data
- Implement connection pooling for MySQL
- Use `IMemoryCache` for frequently accessed data
- Profile with dotMemory/dotTrace before optimizing
- Use CompiledBinding (not ReflectionBinding) for best XAML performance

## Security Guidelines
- Store credentials in OS-native storage (Windows Credential Manager, macOS Keychain, Linux Secret Service)
- Never log sensitive data (passwords, tokens, PII)
- Validate all user inputs with FluentValidation
- Use HTTPS for all external API calls
- Implement rate limiting for API endpoints

## Boot Sequence Patterns (Feature 001)

### Three-Stage Boot Architecture
The application uses a strict three-stage boot sequence managed by `BootOrchestrator`:

```csharp
// Stage 0: Splash Screen (10s timeout)
await orchestrator.ExecuteStage0Async();

// Stage 1: Core Services (60s timeout, <3s target)
await orchestrator.ExecuteStage1Async();

// Stage 2: Application Ready (15s timeout)
await orchestrator.ExecuteStage2Async();
```

### Service Registration Pattern
All services use extension methods for clean DI registration:

```csharp
public static IServiceCollection AddBootServices(this IServiceCollection services)
{
    services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
    services.AddTransient<IBootStage, Stage0Bootstrap>();
    return services;
}
```

### Platform-Specific Services
Use factory pattern for platform-dependent services:

```csharp
public static ISecretsService Create(ILoggerFactory loggerFactory)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return new WindowsSecretsService(loggerFactory.CreateLogger<WindowsSecretsService>());
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        return new MacOSSecretsService(loggerFactory.CreateLogger<MacOSSecretsService>());
    return new AndroidSecretsService(loggerFactory.CreateLogger<AndroidSecretsService>());
}
```

### Performance Budgets
- Total boot time: <10 seconds
- Stage 1 (services): <3 seconds
- Memory usage: <100MB (40MB cache + 30MB services + 30MB framework)

### Error Handling Pattern
Always use comprehensive error categorization:

```csharp
try
{
    await service.InitializeAsync(cancellationToken);
}
catch (OperationCanceledException)
{
    _logger.LogWarning("Operation cancelled");
    // Clean shutdown
}
catch (Exception ex)
{
    var category = _errorCategorizer.Categorize(ex);
    var recovery = _recoveryStrategy.DetermineAction(category);
    // Show user-friendly error with recovery options
}
```

## When Creating New Features
1. Check `.specify/features/` for existing specs
2. Reference `memory/constitution.md` for project principles
3. Run appropriate spec-kit command (`/specify`, `/plan`, etc.)
4. Parse JSON output from scripts before using file paths
5. Follow MVVM pattern: Model → ViewModel → View
6. **Always use `x:DataType` and `CompiledBinding` in XAML**
7. Write tests before implementation (TDD)
8. Update this file if new technologies are introduced

## Common Pitfalls to Avoid
- ❌ Don't use `!` null-forgiving operator without clear justification
- ❌ Don't mix UI logic in ViewModels (keep ViewModels testable)
- ❌ Don't use string concatenation for SQL (use parameterized queries)
- ❌ Don't ignore cancellation tokens in async methods
- ❌ Don't catch generic `Exception` without rethrowing or logging
- ❌ Don't block async code with `.Result` or `.Wait()`
- ❌ **Don't use `{Binding}` without `x:CompileBindings`** - always use `{CompiledBinding}`
- ❌ Don't forget `x:DataType` on root XAML elements
- ❌ Don't use `ConfigureAwait(false)` in UI code (only in libraries)

<!-- MANUAL ADDITIONS START -->
## User: John Koll (GitHub Username: Dorotel)
## Current Date: 2025-10-03

## Additional Project Notes
- MAMP MySQL runs on default port 3306
- Visual ERP API Toolkit requires read-only access token
- LZ4 compression ratio target: ~3:1 for cached JSON payloads
- OpenTelemetry exports to local Jaeger instance (optional)
- Avalonia version: 11.3+ (ensure latest stable)
- Use FluentTheme or Material.Avalonia for theming

<!-- MANUAL ADDITIONS END -->
