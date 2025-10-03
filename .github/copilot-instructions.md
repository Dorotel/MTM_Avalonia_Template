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

### Core Stack (001-boot-sequence-splash)
- **Framework**: C# .NET 9.0 with nullable reference types enabled
- **UI**: Avalonia 11.3+
- **MVVM**: CommunityToolkit.Mvvm 8.3+
- **Database**: MySQL.Data (MAMP MySQL 5.7)
- **HTTP**: HttpClient (Visual API Toolkit integration)
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
│   │   └── App.axaml                   # Application entry
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
- Use `[RelayCommand]` for commands
- ViewModels inherit from `ObservableObject`
- Use `partial` classes for source generators

```csharp
// ✅ Good
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _userName;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load data
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

#### Avalonia XAML Conventions
- Use `x:DataType` for compiled bindings
- Prefer `CompiledBinding` over `Binding`
- Use design-time data: `Design.DataContext`

```xml
<!-- ✅ Good -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Avalonia_Template.ViewModels"
        x:Class="MTM_Avalonia_Template.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        Design.Width="800" Design.Height="450">
    
    <TextBlock Text="{CompiledBinding UserName}" />
</Window>
```

#### Async/Await Patterns
- Always use `async`/`await` for I/O operations
- Use `ConfigureAwait(false)` in library code
- Suffix async methods with `Async`
- Use `ValueTask<T>` for hot paths when appropriate

```csharp
// ✅ Good
public async Task<List<Order>> GetOrdersAsync(CancellationToken ct = default)
{
    return await _httpClient
        .GetFromJsonAsync<List<Order>>("/api/orders", ct)
        .ConfigureAwait(false);
}
```

#### Dependency Injection
- Use constructor injection
- Register services in `Program.cs` or startup
- Use interfaces for testability

```csharp
// ✅ Good
public class OrderService : IOrderService
{
    private readonly IHttpClient _httpClient;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(IHttpClient httpClient, ILogger<OrderService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

#### Error Handling with Polly
- Use retry policies for transient failures
- Use circuit breakers for cascading failures
- Log all retry attempts

```csharp
// ✅ Good
private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy = 
    Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

#### Logging with Serilog
- Use structured logging
- Use log levels appropriately
- Include correlation IDs for distributed tracing

```csharp
// ✅ Good
_logger.LogInformation(
    "Processing order {OrderId} for customer {CustomerId}", 
    orderId, 
    customerId
);

// ❌ Bad
_logger.LogInformation($"Processing order {orderId}");
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
    }
}
```

### Database (MySQL)
- Use parameterized queries **always** (prevent SQL injection)
- Use transactions for multi-step operations
- Close connections properly (use `using` statements)
- Connection strings stored in OS-native credential storage

```csharp
// ✅ Good
using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();

using var command = new MySqlCommand(
    "SELECT * FROM orders WHERE customer_id = @customerId", 
    connection
);
command.Parameters.AddWithValue("@customerId", customerId);
```

### Testing Standards
- Unit tests for business logic
- Integration tests for API/database operations
- Use xUnit or NUnit
- Mock external dependencies with Moq or NSubstitute
- Aim for >80% code coverage on critical paths

## Recent Changes
- **2025-10-03**: Enhanced Copilot instructions with spec-kit integration
- **001-boot-sequence-splash**: Added full observability stack (Serilog, OpenTelemetry)
- **001-boot-sequence-splash**: Added resilience patterns (Polly)
- **001-boot-sequence-splash**: Added validation (FluentValidation) and mapping (AutoMapper)
- **001-boot-sequence-splash**: Initial Avalonia + MVVM + MySQL setup

## Performance Guidelines
- Use LZ4 compression for cached data
- Implement connection pooling for MySQL
- Use `IMemoryCache` for frequently accessed data
- Profile with dotMemory/dotTrace before optimizing

## Security Guidelines
- Store credentials in OS-native storage (Windows Credential Manager, macOS Keychain, Linux Secret Service)
- Never log sensitive data (passwords, tokens, PII)
- Validate all user inputs
- Use HTTPS for all external API calls
- Implement rate limiting for API endpoints

## When Creating New Features
1. Check `.specify/features/` for existing specs
2. Reference `memory/constitution.md` for project principles
3. Run appropriate spec-kit command (`/specify`, `/plan`, etc.)
4. Parse JSON output from scripts before using file paths
5. Follow MVVM pattern: Model → ViewModel → View
6. Write tests before implementation (TDD)
7. Update this file if new technologies are introduced

## Common Pitfalls to Avoid
- Don't use `!` null-forgiving operator without clear justification
- Don't mix UI logic in ViewModels (keep ViewModels testable)
- Don't use string concatenation for SQL (use parameterized queries)
- Don't ignore cancellation tokens in async methods
- Don't catch generic `Exception` without rethrowing or logging
- Don't block async code with `.Result` or `.Wait()`

<!-- MANUAL ADDITIONS START -->
## User: John Koll (GitHub UserName: Dorotel)
## Current Date: 2025-10-03

## Additional Project Notes
- MAMP MySQL runs on default port 3306
- Visual ERP API Toolkit requires read-only access token
- LZ4 compression ratio target: ~3:1 for cached JSON payloads
- OpenTelemetry exports to local Jaeger instance (optional)

<!-- MANUAL ADDITIONS END -->