<!--
SYNC IMPACT REPORT - Constitution Update
Version Change: 1.0.0 → 1.1.0 (Avalonia Best Practices Integration)
Modified Principles:
  - Enhanced: I. Cross-Platform First (Added platform detection guidance)
  - Enhanced: II. MVVM Community Toolkit Standard (Added CancellationToken requirements)
  - Enhanced: III. Test-First Development (Added xUnit as standard)
  - Enhanced: IV. Theme V2 Semantic Tokens (Added Avalonia theming specifics)
  - Enhanced: V. Null Safety and Error Resilience (Added async patterns)
  - Added: VI. Compiled Bindings Only (Avalonia 11.3+ performance standard)
  - Added: VII. Dependency Injection via AppBuilder (Avalonia DI patterns)
Added Sections:
  - Avalonia XAML Standards (x:DataType, x:CompileBindings requirements)
  - Async/Await Patterns (CancellationToken mandatory)
  - Spec-Kit Integration (Workflow commands and script execution)
Templates Requiring Updates:
  ✅ plan-template.md: Constitution version reference updated (v1.0.0 → v1.1.0)
  ✅ spec-template.md: Aligned with enhanced XAML principles
  ✅ tasks-template.md: xUnit testing standards integrated
Follow-up TODOs:
  - Review existing XAML files for CompiledBinding compliance
  - Audit async methods for CancellationToken parameters
  - Update legacy Binding syntax to CompiledBinding
-->

# MTM Avalonia Template Constitution

**Project Type**: Manufacturing/Warehouse Desktop Application
**Framework**: Avalonia UI 11.3+ with .NET 9.0
**Methodology**: Spec-Driven Development (GitHub Spec Kit)
**Version**: 1.1.0 | **Ratified**: 2025-10-02 | **Last Amended**: 2025-10-03

---

## Preamble

This constitution establishes the foundational principles for the MTM Avalonia Template project. It ensures consistency, maintainability, and reliability for manufacturing-critical applications built on Avalonia UI. All development decisions must align with these principles. Deviations require explicit justification and approval.

---

## Core Principles

### I. Cross-Platform First (NON-NEGOTIABLE)

**All features MUST work across all supported platforms** (Windows, Linux, macOS, and future mobile/browser targets).

**Requirements**:
- Platform-specific code MUST be abstracted through interfaces
- Shared business logic lives in common libraries (`.Core` project)
- Platform differences handled via dependency injection
- Use `RuntimeInformation.IsOSPlatform()` for platform detection
- NO direct P/Invoke calls; use Avalonia platform abstractions

**Rationale**: Avalonia's strength is true cross-platform capability. Breaking this principle fragments the user experience and increases maintenance overhead.

**Example**:
```csharp
// ✅ Good - Platform abstraction
public interface IPlatformService
{
    string GetConfigPath();
}

// Platform-specific implementations registered via DI
public class WindowsPlatformService : IPlatformService
{
    public string GetConfigPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MTM");
}
```

---

### II. MVVM Community Toolkit Standard (NON-NEGOTIABLE)

**Use MVVM Community Toolkit 8.3+ patterns exclusively.**

**Requirements**:
- `[ObservableObject]` as base class with `[ObservableProperty]` for properties
- `[RelayCommand]` for all commands (supports async and CanExecute)
- **NO ReactiveUI patterns** (ReactiveObject, ReactiveCommand, RaiseAndSetIfChanged)
- Constructor dependency injection for services
- `partial` classes for source generation
- Use `[NotifyCanExecuteChangedFor]` for dependent command state

**Rationale**: Provides compile-time source generation, reduces boilerplate, ensures consistency. ReactiveUI patterns conflict and create maintenance confusion.

**Example**:
```csharp
// ✅ Good - CommunityToolkit.Mvvm pattern
public partial class OrderViewModel : ObservableObject
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isDirty;

    [ObservableProperty]
    private Order? _currentOrder;

    public OrderViewModel(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (CurrentOrder is null) return;
        await _orderService.SaveOrderAsync(CurrentOrder, cancellationToken);
        IsDirty = false;
    }

    private bool CanSave() => IsDirty && CurrentOrder is not null;
}
```

---

### III. Test-First Development (NON-NEGOTIABLE)

**TDD is mandatory for all features.**

**Requirements**:
- Tests written → User approved → Tests fail → **Then** implement
- Red-Green-Refactor cycle strictly enforced
- Use **xUnit** as the standard testing framework
- Contract tests for all API endpoints
- Integration tests for user workflows
- Unit tests for business logic (ViewModels, Services)
- Mock external dependencies with **NSubstitute**
- Aim for >80% code coverage on critical paths

**Rationale**: Avalonia's complexity and manufacturing domain criticality require reliable testing. Template users need confidence in foundation stability.

**Example**:
```csharp
// ✅ Good - xUnit test with NSubstitute
public class OrderViewModelTests
{
    [Fact]
    public async Task SaveCommand_WithValidOrder_ShouldCallService()
    {
        // Arrange
        var mockService = Substitute.For<IOrderService>();
        var order = new Order { Id = 1, Total = 100 };
        var vm = new OrderViewModel(mockService)
        {
            CurrentOrder = order,
            IsDirty = true
        };

        // Act
        await vm.SaveCommand.ExecuteAsync(CancellationToken.None);

        // Assert
        await mockService.Received(1).SaveOrderAsync(order, Arg.Any<CancellationToken>());
        Assert.False(vm.IsDirty);
    }
}
```

---

### IV. Theme V2 Semantic Tokens (REQUIRED)

**All styling MUST use Theme V2 dynamic resources for adaptive theming.**

**Requirements**:
- Use FluentTheme or Material.Avalonia as base theme
- Consistent Manufacturing Design System integration
- Semantic token usage over hardcoded values (`{DynamicResource TextColor}` not `#000000`)
- Base styles with variant overrides for customization
- Support light/dark theme switching
- Define custom themes in separate `.axaml` resource dictionaries

**Rationale**: Ensures consistent theming across the application and enables easy theme switching and customization.

**Example**:
```xml
<!-- ✅ Good - Semantic tokens -->
<Style Selector="Button.PrimaryAction">
    <Setter Property="Background" Value="{DynamicResource SystemAccentColor}" />
    <Setter Property="Foreground" Value="{DynamicResource SystemAccentForegroundColor}" />
    <Setter Property="Padding" Value="{DynamicResource ButtonPadding}" />
</Style>

<!-- ❌ Bad - Hardcoded values -->
<Style Selector="Button.PrimaryAction">
    <Setter Property="Background" Value="#0078D4" />
    <Setter Property="Foreground" Value="White" />
</Style>
```

---

### V. Null Safety and Error Resilience (REQUIRED)

**All code must be resilient to null values and errors.**

**Requirements**:
- Enable nullable reference types (`<Nullable>enable</Nullable>`)
- Use `ArgumentNullException.ThrowIfNull()` for method parameters
- Proper error boundaries in ViewModels (try-catch with logging)
- Graceful degradation for offline scenarios
- Comprehensive logging with **Serilog** and structured data
- Use `Result<T>` or `Option<T>` patterns for failure-prone operations

**Rationale**: Manufacturing environments are mission-critical. Application crashes are unacceptable and can halt operations.

**Example**:
```csharp
// ✅ Good - Null safety and error handling
public async Task<Result<Order>> GetOrderAsync(int orderId, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(orderId);

    try
    {
        var order = await _repository.GetByIdAsync(orderId, cancellationToken);
        return order is not null
            ? Result<Order>.Success(order)
            : Result<Order>.Failure("Order not found");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve order {OrderId}", orderId);
        return Result<Order>.Failure($"Database error: {ex.Message}");
    }
}
```

---

### VI. Compiled Bindings Only (NON-NEGOTIABLE) 🆕

**All XAML bindings MUST use CompiledBinding with x:DataType.**

**Requirements**:
- **Always** set `x:DataType` on Window/UserControl root
- **Always** set `x:CompileBindings="True"` for compile-time validation
- Use `{CompiledBinding PropertyName}` syntax (Avalonia 11.0+)
- **NO** legacy `{Binding}` syntax without `x:CompileBindings`
- Use `{ReflectionBinding}` only for dynamic scenarios with explicit justification
- Use `Design.DataContext` for XAML previewer support

**Rationale**: CompiledBinding provides compile-time type safety, eliminates runtime binding errors, and significantly improves performance over reflection-based bindings.

**Example**:
```xml
<!-- ✅ CORRECT - CompiledBinding with x:DataType -->
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Avalonia_Template.ViewModels"
        x:Class="MTM_Avalonia_Template.Views.MainWindow"
        x:DataType="vm:MainViewModel"
        x:CompileBindings="True">
    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <StackPanel>
        <TextBlock Text="{CompiledBinding Title}" />
        <Button Content="Save" Command="{CompiledBinding SaveCommand}" />
    </StackPanel>
</Window>

<!-- ❌ WRONG - Legacy Binding without x:CompileBindings -->
<TextBlock Text="{Binding Title}" />
```

---

### VII. Dependency Injection via AppBuilder (REQUIRED) 🆕

**All service registration MUST occur in Program.cs using AppBuilder.**

**Requirements**:
- Register services in `Program.cs` via `AppBuilder.ConfigureServices()`
- Use constructor injection for all dependencies
- Register ViewModels as `Transient` (one per view)
- Register Services as `Singleton` or `Scoped` based on lifecycle
- Use interfaces for all service contracts
- NO service locator pattern or static service access

**Rationale**: Avalonia's DI integration ensures proper lifecycle management and testability.

**Example**:
```csharp
// ✅ Good - Program.cs DI setup
public static AppBuilder BuildAvaloniaApp()
{
    return AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .WithInterFont()
        .ConfigureServices(services =>
        {
            // Register ViewModels (Transient - new instance per view)
            services.AddTransient<MainViewModel>();
            services.AddTransient<OrderViewModel>();

            // Register Services (Singleton - shared instance)
            services.AddSingleton<IOrderService, OrderService>();
            services.AddSingleton<IAuthService, AuthService>();

            // Register Infrastructure
            services.AddSingleton<ILogger>(
                new LoggerConfiguration()
                    .WriteTo.File("logs/app.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger()
            );
        });
}
```

---

## Async/Await Patterns (REQUIRED) 🆕

**All async operations MUST support cancellation.**

**Requirements**:
- **Always** provide `CancellationToken` parameter (default value allowed)
- Suffix async methods with `Async`
- Use `ConfigureAwait(false)` in library/service code (NOT in UI code)
- Use `ValueTask<T>` for hot paths when appropriate
- Propagate cancellation to downstream async calls
- Handle `OperationCanceledException` gracefully

**Rationale**: Manufacturing operations may be long-running; users need ability to cancel without blocking UI.

**Example**:
```csharp
// ✅ Good - Proper async pattern
public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromSeconds(30)); // Timeout fallback

    return await _httpClient
        .GetFromJsonAsync<List<Order>>("/api/orders", cts.Token)
        .ConfigureAwait(false); // Only in library code, NOT UI
}
```

---

## Manufacturing Domain Requirements

**All template features MUST align with manufacturing/warehouse operations.**

**Requirements**:
- Support offline-first scenarios with sync capabilities
- Implement barcode scanning and labeling standards (Code128, QR, DataMatrix)
- Role-based access control for manufacturing environments (Operator, Supervisor, Admin)
- Data contracts designed for industrial reliability (versioning, backward compatibility)
- Audit logging for all critical operations
- Integration with Visual ERP via read-only API Toolkit

**Manufacturing-Specific Constraints**:
- No operations that require constant internet connectivity
- All data must be cacheable locally (LZ4 compressed)
- UI must be operable with touchscreen + keyboard
- Support for industrial barcode scanners and label printers

---

## Spec-Driven Development Workflow (REQUIRED) 🆕

**All features MUST follow the Spec-Kit methodology.**

**Workflow Commands** (executed in order):
1. `/constitution` - Establish/update project principles
2. `/specify` - Create feature specification
3. `/clarify` - De-risk ambiguous areas (before planning)
4. `/plan` - Create technical implementation plan
5. `/tasks` - Generate actionable task breakdown
6. `/analyze` - Cross-artifact consistency check
7. `/implement` - Execute implementation

**Script Execution Rules**:
- Scripts **always** run from repository root
- Scripts output **JSON** with file paths and branch names
- **Always parse JSON output** before using paths
- Example: `create-new-feature.sh --json "{ARGS}"` returns `{"BRANCH_NAME": "...", "SPEC_FILE": "..."}`

**Required Artifacts**:
- `.specify/features/SPEC_*.md` - Feature specifications
- `.specify/features/PLAN_*.md` - Implementation plans
- `.specify/features/TASKS_*.md` - Task breakdowns
- `memory/constitution.md` - This file (project principles)

---

## Development Workflow

**Code Review Requirements**:
- All PRs MUST verify constitutional compliance
- All template components must be production-ready
- Tests must pass before merge (Red-Green-Refactor validated)
- CompiledBinding usage verified in XAML files
- CancellationToken parameters present in async methods

**Documentation Standards**:
- Include quickstart guides for new features
- Troubleshooting sections for common issues
- Agent-specific files (`.github/copilot-instructions.md`, `CLAUDE.md`) kept under 150 lines
- Update `docs/TOC-IMPLEMENTATION-ORDER.md` for structured guidance

**Complexity Management**:
- Complexity deviations require explicit justification in plan
- Use `docs/TOC-IMPLEMENTATION-ORDER.md` for dependency ordering
- Break large features into incremental deliverables

---

## Governance

**Constitutional Authority**:
- This constitution supersedes all other development practices
- Amendments require documentation, approval, and template migration plan
- All PRs/reviews must verify constitutional compliance before merge

**Amendment Process**:
1. Propose change with justification (issue or PR)
2. Review impact on existing features and templates
3. Document migration path for affected code
4. Update constitution version and SYNC IMPACT REPORT
5. Gain approval from project maintainers
6. Update all affected templates and documentation

**Enforcement**:
- CI/CD pipelines enforce TDD and test coverage requirements
- Code reviews verify CompiledBinding and async patterns
- Automated linters check for ReactiveUI usage (fails build)

---

## Version History

| Version | Date       | Changes                                              |
|---------|------------|------------------------------------------------------|
| 1.0.0   | 2025-10-02 | Initial ratification - Manufacturing template setup  |
| 1.1.0   | 2025-10-03 | Added Avalonia best practices, CompiledBinding, DI patterns, Spec-Kit integration |

---

**Maintained by**: John Koll (@Dorotel)
**Last Review**: 2025-10-03
**Next Review Due**: 2025-11-03 (Monthly review cycle)
