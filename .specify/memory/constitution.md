<!--
SYNC IMPACT REPORT - Constitution Update
Version Change: 1.1.0 → 1.2.0 (Platform Scope Clarification + Visual ERP Integration Standards)
Date: 2025-10-05
Status: ✅ COMPLETE

Modified Principles:
  ✅ Enhanced: I. Cross-Platform First → Clarified Phase 1 scope (Windows + Android only)
  ✅ Enhanced: Manufacturing Domain Requirements → Expanded Visual ERP integration details
  ✅ Added: VIII. Visual ERP Integration Standards (NEW - elevated from feature requirements)

Added Sections:
  ✅ Technology Stack (C# .NET 9.0, Avalonia 11.3+, MySQL 5.7, Visual ERP)
  ✅ Platform Architecture (Desktop vs Android data access patterns)
  ✅ Visual ERP Command Whitelist requirements

Templates Requiring Updates:
  ✅ plan-template.md: Constitution Check gates updated for Principle VIII (includes all 8 principles + Async patterns)
  ✅ spec-template.md: Added Visual ERP integration to AI guidance (feature-specific sections added per feature)
  ✅ Feature 002 spec.md: Android architecture aligned with Feature 001 (MTM Server API)
  ✅ Feature 002 plan.md: Android data access pattern updated

Implementation Updates Completed:
  ✅ Feature 002 spec.md: Updated FR-025, FR-025a, FR-028-031b (Android MTM Server API)
  ✅ Feature 002 spec.md: Updated clarification Q&A (Android architecture, Visual whitelist location)
  ✅ Feature 002 plan.md: Updated Constraints section (Android MTM Server API)
  ✅ Feature 002 plan.md: Updated Constitution Check (Principle I v1.2.0, added Principle VIII)
  ✅ Feature 002 plan.md: Updated Key Decisions and Clarification Resolution Summary

Follow-up TODOs (Implementation Phase):
  🔲 Verify Visual command whitelist exists at docs/VISUAL-WHITELIST.md (create if missing)
  🔲 Implement whitelist validation in Visual API client wrapper
  🔲 Ensure Android two-factor auth implementation (credentials + device certificate)
  🔲 Update Feature 001 documentation to reference Constitution v1.2.0 Principle VIII
  🔲 Create Visual API client service with whitelist enforcement
  🔲 Add placeholder for PLACEHOLDER_ANDROID_MIN_SDK in Technology Stack

Constitution Changes Summary:
  - Version: 1.1.0 → 1.2.0 (MINOR - new principle added)
  - New Principle: VIII. Visual ERP Integration Standards (NON-NEGOTIABLE)
  - Clarified: Phase 1 platform scope (Windows Desktop + Android only)
  - Added: Technology Stack section with complete framework/dependency listing
  - Added: Platform Architecture section with data access patterns
  - Enhanced: Manufacturing Domain Requirements with Visual/MySQL specifics
  - Updated: Version History table
  - Updated: Last Review date and Next Review Due date

Suggested Commit Message:
docs: amend constitution to v1.2.0 - platform scope + Visual ERP integration standards
-->

# MTM Avalonia Template Constitution

**Project Type**: Manufacturing/Warehouse Desktop + Android Application
**Framework**: Avalonia UI 11.3+ with .NET 9.0
**Methodology**: Spec-Driven Development (GitHub Spec Kit)
**Version**: 1.2.0 | **Ratified**: 2025-10-02 | **Last Amended**: 2025-10-05

---

## Preamble

This constitution establishes the foundational principles for the MTM Avalonia Template project. It ensures consistency, maintainability, and reliability for manufacturing-critical applications built on Avalonia UI. All development decisions must align with these principles. Deviations require explicit justification and approval.

---

## Core Principles

### I. Cross-Platform First (NON-NEGOTIABLE)

**Phase 1: All features MUST work on Windows Desktop and Android platforms.** Future phases may expand to Linux, macOS, iOS, and browser targets.

**Current Platform Support** (Phase 1):
- ✅ **Windows Desktop**: Primary development and operator platform
- ✅ **Android**: Warehouse tablet platform for mobile operations
- ⏸️ **Linux Desktop**: Deferred to Phase 2+
- ⏸️ **macOS Desktop**: Deferred to Phase 2+
- ⏸️ **iOS**: Deferred to Phase 2+
- ⏸️ **Browser/WASM**: Deferred to Phase 3+

**Requirements**:
- Platform-specific code MUST be abstracted through interfaces
- Shared business logic lives in common libraries (`.Core` project)
- Platform differences handled via dependency injection
- Use `RuntimeInformation.IsOSPlatform()` for platform detection
- NO direct P/Invoke calls; use Avalonia platform abstractions
- Unsupported platforms MUST throw `PlatformNotSupportedException` with clear messaging

**Rationale**: Avalonia's strength is true cross-platform capability. Phase 1 focuses on Windows (operator workstations) and Android (warehouse tablets) to meet immediate manufacturing requirements. Architecture maintains abstraction to support future platform expansion without code rewrites.

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

### VIII. Visual ERP Integration Standards (NON-NEGOTIABLE) 🆕

**All Visual ERP access MUST be read-only via Infor Visual API Toolkit commands with whitelist enforcement.**

**Requirements**:
- **Read-Only Access**: NO write operations to Visual ERP database (architectural mandate)
- **API Toolkit Only**: Use Infor Visual API Toolkit commands exclusively (NEVER direct SQL)
- **Command Whitelist**: Maintain explicit whitelist of approved read-only commands in `docs/VISUAL-WHITELIST.md`
- **Whitelist Validation**: All Visual API calls MUST validate against whitelist before execution
- **Citation Format**: All toolkit commands MUST use format: `"Reference-{FileName} - {Chapter/Section/Page}"`
- **Credential Storage**: Visual username/password stored using OS-native secrets:
  - **Windows Desktop**: Windows DPAPI via `WindowsSecretsService`
  - **Android**: Android KeyStore via `AndroidSecretsService`
- **Android Authentication**: Two-factor auth (user credentials + device certificate in Android KeyStore)
- **Schema Access**: Use provided CSV dictionary files for Visual schema reference (no direct schema queries)

**Platform-Specific Data Access** (Phase 1):
- **Windows Desktop**: Direct Visual API Toolkit client connection
- **Android**: MTM Server API → Visual API Toolkit (server-side integration)
  - Android devices MUST NEVER connect directly to Visual database
  - Android accesses Visual data via API projections only
  - Server handles Visual API Toolkit command execution and caching

**Rationale**: Visual ERP is company's production system of record. Read-only access via controlled API prevents accidental data corruption, maintains Visual data integrity, and ensures audit compliance. Whitelist enforcement prevents unauthorized or write operations.

**Example**:

```csharp
// ✅ Good - Whitelist-validated Visual API call
public async Task<List<Part>> GetPartsAsync(CancellationToken cancellationToken)
{
    const string command = "PART_GET_ALL"; // Reference-VisualAPIGuide - Chapter3/Section2.1

    // Validate command is in whitelist
    if (!_visualWhitelist.IsCommandAllowed(command))
    {
        _logger.LogError("Visual command {Command} not in whitelist", command);
        throw new UnauthorizedAccessException($"Command {command} not approved");
    }

    // Retrieve credentials from OS-native storage
    var username = await _secretsService.RetrieveSecretAsync("Visual.Username", cancellationToken);
    var password = await _secretsService.RetrieveSecretAsync("Visual.Password", cancellationToken);

    // Execute via API Toolkit (read-only)
    return await _visualClient.ExecuteCommandAsync<List<Part>>(
        command,
        username,
        password,
        cancellationToken
    );
}

// ❌ BAD - Direct SQL access
var parts = await _sqlConnection.QueryAsync("SELECT * FROM PART");

// ❌ BAD - Write operation
await _visualClient.ExecuteCommandAsync("PART_UPDATE", ...);

// ❌ BAD - No whitelist validation
await _visualClient.ExecuteCommandAsync(userProvidedCommand, ...);
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

## Technology Stack (Phase 1)

**Core Framework**:
- **Language**: C# .NET 9.0 with nullable reference types enabled
- **UI Framework**: Avalonia 11.3+ (cross-platform XAML)
- **MVVM**: CommunityToolkit.Mvvm 8.3+ (source generators)

**Data & Storage**:
- **Application Database**: MAMP MySQL 5.7 (local development), MySQL 5.7+ (production)
  - User preferences, feature flags, application configuration
  - Transactional data not suitable for Visual ERP
- **ERP Integration**: Infor Visual ERP (read-only via API Toolkit)
  - Master data: Parts, Locations, Warehouses, Work Centers
  - Production data caching with LZ4 compression
- **Local Cache**: LZ4-compressed JSON for offline operation
- **Secrets Storage**:
  - Windows: DPAPI via `WindowsSecretsService`
  - Android: KeyStore via `AndroidSecretsService`

**Testing & Quality**:
- **Test Framework**: xUnit (unit + integration tests)
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Coverage Target**: >80% on critical paths

**Observability**:
- **Logging**: Serilog with structured logging
- **Telemetry**: OpenTelemetry (optional remote endpoints)
- **Metrics**: Boot metrics, service health checks, cache statistics

**Resilience**:
- **Retry Logic**: Polly (exponential backoff with jitter)
- **Circuit Breaker**: Polly (5 failures → open, exponential recovery 30s-10m)
- **Mapping**: AutoMapper (DTO transformations)
- **Validation**: FluentValidation (data integrity)

**Platform**:
- **Windows Desktop**: .NET 9.0 Desktop Runtime
- **Android**: .NET 9.0 Android (API Level [PLACEHOLDER_ANDROID_MIN_SDK])

---

## Platform Architecture (Phase 1)

### Windows Desktop

**Data Access Pattern**:

```
Windows Desktop App
    ↓ (Direct Connection)
    ├→ MAMP MySQL 5.7 (Application DB)
    └→ Infor Visual API Toolkit → Visual ERP (Read-Only)
```

**Services**:
- Configuration: `ConfigurationService` (MySQL persistence)
- Secrets: `WindowsSecretsService` (DPAPI)
- Visual Access: Direct Visual API Toolkit client

### Android

**Data Access Pattern**:

```
Android Tablet App
    ↓ (HTTPS)
    MTM Server API
    ↓
    ├→ MySQL 5.7 (Application DB)
    └→ Infor Visual API Toolkit → Visual ERP (Read-Only)
```

**Services**:
- Configuration: `ConfigurationService` (via MTM Server API)
- Secrets: `AndroidSecretsService` (KeyStore)
- Visual Access: MTM Server API projections (server-side Visual integration)
- Authentication: Two-factor (credentials + device certificate)

**Rationale**: Android devices operate in warehouse environments with intermittent connectivity. MTM Server API provides:
- Centralized Visual access with connection pooling
- Cached Visual data projections for offline operation
- Security boundary (Android never directly accesses Visual/MySQL)
- Device certificate validation for enhanced security

---

## Manufacturing Domain Requirements

**All template features MUST align with manufacturing/warehouse operations.**

**Requirements**:
- Support offline-first scenarios with sync capabilities
- Implement barcode scanning and labeling standards (Code128, QR, DataMatrix)
- Role-based access control for manufacturing environments (Operator, Supervisor, Admin)
- Data contracts designed for industrial reliability (versioning, backward compatibility)
- Audit logging for all critical operations
- **Visual ERP Integration**: Read-only via Infor Visual API Toolkit (see Principle VIII)
- **Application Database**: MAMP MySQL 5.7 for user preferences, feature flags, and app-specific data

**Manufacturing-Specific Constraints**:
- No operations that require constant internet connectivity
- All Visual master data must be cacheable locally (LZ4 compressed)
- UI must be operable with touchscreen + keyboard
- Support for industrial barcode scanners and label printers
- Visual credentials stored in OS-native secure storage (DPAPI/KeyStore)
- Android devices authenticate with two-factor (credentials + device certificate)

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

| Version | Date       | Changes                                                                                                                                                                              |
| ------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1.0.0   | 2025-10-02 | Initial ratification - Manufacturing template setup                                                                                                                                  |
| 1.1.0   | 2025-10-03 | Added Avalonia best practices, CompiledBinding, DI patterns, Spec-Kit integration                                                                                                    |
| 1.2.0   | 2025-10-05 | Clarified Phase 1 platform scope (Windows + Android), Added Principle VIII (Visual ERP Integration), Technology Stack section, Platform Architecture, Android MTM Server API pattern |

---

**Maintained by**: John Koll (@Dorotel)
**Last Review**: 2025-10-05
**Next Review Due**: 2025-11-05 (Monthly review cycle)
