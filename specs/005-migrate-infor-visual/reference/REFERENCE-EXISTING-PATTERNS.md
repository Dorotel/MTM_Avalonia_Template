# Reference: Existing Codebase Patterns
**Date**: October 8, 2025
**Purpose**: Document established patterns to follow during Feature 005 implementation

---

## MVVM Patterns (CommunityToolkit.Mvvm 8.4.0)

### ViewModel Structure
```csharp
public partial class ExampleViewModel : ObservableObject
{
    private readonly ILogger<ExampleViewModel> _logger;
    private readonly IExampleService _service;

    public ExampleViewModel(
        ILogger<ExampleViewModel> logger,
        IExampleService service)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(service);

        _logger = logger;
        _service = service;
    }

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
            // Implementation
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanLoadData() => !IsLoading;
}
```

**Key Points:**
- Use `[ObservableProperty]` for all bindable properties
- Use `[RelayCommand]` for all commands
- Include `CancellationToken` in all async methods
- Use `partial class` modifier
- Inherit from `ObservableObject`
- Constructor injection with null checks

---

## Avalonia XAML Patterns

### Window/View Structure
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MTM_Template_Application.ViewModels"
        x:Class="MTM_Template_Application.Views.ExampleWindow"
        x:DataType="vm:ExampleViewModel"
        x:CompileBindings="True"
        Title="{CompiledBinding Title}"
        Width="1200" Height="800"
        MinWidth="900" MinHeight="600">

    <Design.DataContext>
        <vm:ExampleViewModel />
    </Design.DataContext>

    <DockPanel>
        <!-- Content here -->
    </DockPanel>
</Window>
```

**Key Points:**
- ALWAYS use `x:DataType` and `x:CompileBindings="True"`
- ALWAYS use `{CompiledBinding}` syntax
- Include `Design.DataContext` for previewer
- Set `MinWidth`/`MinHeight` for usability

### Current Repeated XAML Patterns (Extract to Controls)

#### Status Card Pattern
```xml
<Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
    BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
    BorderThickness="1"
    CornerRadius="8"
    Padding="20">
    <StackPanel Spacing="16">
        <TextBlock Text="[Title]"
            FontSize="18"
            FontWeight="SemiBold"
            Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
        <!-- Content -->
    </StackPanel>
</Border>
```
**Occurrences**: 15+ in DebugTerminalWindow.axaml
**Extract to**: `StatusCard` control

#### Metric Display Pattern
```xml
<Grid ColumnDefinitions="200,*,150">
    <TextBlock Grid.Column="0" Text="[Label]"
        FontWeight="SemiBold"
        Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />
    <TextBlock Grid.Column="1"
        Text="{CompiledBinding Value}"
        Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />
    <TextBlock Grid.Column="2"
        Text="[Status]"
        Foreground="{DynamicResource SystemAccentColor}" />
</Grid>
```
**Occurrences**: 20+ in DebugTerminalWindow.axaml
**Extract to**: `MetricDisplay` control

---

## Configuration Service Patterns (Feature 002)

### Reading Configuration
```csharp
var value = _configurationService.GetValue<T>("Key:Path", defaultValue);
```

### Setting Configuration
```csharp
await _configurationService.SetValue("Key:Path", value, cancellationToken);
```

### Loading User Preferences
```csharp
await _configurationService.LoadUserPreferencesAsync(userId, cancellationToken);
```

### Saving User Preferences
```csharp
await _configurationService.SaveUserPreferenceAsync(userId, key, value, cancellationToken);
```

### Configuration Change Events
```csharp
_configurationService.OnConfigurationChanged += (sender, args) =>
{
    _logger.LogInformation("Config changed: {Key} = {NewValue}", args.Key, args.NewValue);
};
```

---

## Database Patterns (MySQL)

### Always Use Parameterized Queries
```csharp
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync(cancellationToken);

var query = "SELECT * FROM Users WHERE UserId = @userId";
await using var command = new MySqlCommand(query, connection);
command.Parameters.AddWithValue("@userId", userId);

await using var reader = await command.ExecuteReaderAsync(cancellationToken);
while (await reader.ReadAsync(cancellationToken))
{
    // Process results
}
```

**Critical:**
- Table names: `Users`, `UserPreferences`, `FeatureFlags`
- Column names: `UserId`, `PreferenceKey`, `FlagName`
- Case-sensitive in production (Linux)
- Reference `.github/mamp-database/schema-tables.json`

---

## Service Registration (DI)

### Singleton Services
```csharp
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<IBootOrchestrator, BootOrchestrator>();
```

### Transient ViewModels
```csharp
services.AddTransient<MainViewModel>();
services.AddTransient<SettingsViewModel>();
```

### Platform-Specific Services
```csharp
services.AddSingleton<ISecretsService>(sp =>
    SecretsServiceFactory.Create(sp.GetRequiredService<ILoggerFactory>()));
```

---

## Error Handling Patterns

### Service Layer
```csharp
try
{
    await operation(cancellationToken);
}
catch (OperationCanceledException)
{
    _logger.LogWarning("Operation cancelled");
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}
```

### ViewModel Layer
```csharp
[RelayCommand]
private async Task ExecuteAsync(CancellationToken cancellationToken)
{
    try
    {
        IsLoading = true;
        await _service.OperationAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Command failed");
        // Show user-friendly message
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## Testing Patterns

### ViewModel Unit Tests
```csharp
[Fact]
public async Task Command_Should_UpdateProperty()
{
    // Arrange
    var mockService = Substitute.For<IService>();
    var viewModel = new ExampleViewModel(mockService, logger);

    // Act
    await viewModel.CommandAsync(CancellationToken.None);

    // Assert
    viewModel.PropertyName.Should().Be(expectedValue);
    await mockService.Received(1).MethodAsync(Arg.Any<CancellationToken>());
}
```

### Service Integration Tests
```csharp
[Fact]
public async Task Service_Should_PersistData()
{
    // Arrange
    var service = new ConfigurationService(logger);
    const string key = "TestKey";
    const string value = "TestValue";

    // Act
    await service.SetValue(key, value, CancellationToken.None);
    var result = service.GetValue<string>(key);

    // Assert
    result.Should().Be(value);
}
```

---

## Theme V2 Patterns

### Semantic Tokens
```xml
Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}"
BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
```

### Accent Colors
```xml
Foreground="{DynamicResource SystemAccentColor}"
Background="{DynamicResource SystemAccentColorLight1}"
```

---

## File Organization

### Current Structure
```
MTM_Template_Application/
├── ViewModels/           # All ViewModels
├── Views/                # All XAML views
├── Models/               # Domain models
├── Services/             # Business logic
│   ├── Configuration/
│   ├── Boot/
│   └── Diagnostics/
└── Extensions/           # DI extensions
```

### Proposed Structure (Add for Feature 005)
```
MTM_Template_Application/
├── Controls/             # NEW: Custom controls
│   ├── Cards/
│   ├── Metrics/
│   └── Common/
├── ViewModels/
│   └── Settings/         # NEW: Settings ViewModels
└── Views/
    └── Settings/         # NEW: Settings views
```

---

## Null Safety Patterns

### Required Parameters
```csharp
public Service(ILogger logger)
{
    ArgumentNullException.ThrowIfNull(logger);
    _logger = logger;
}
```

### Nullable Reference Types
```csharp
public string? GetValue(string key)  // Can return null
{
    return _settings.TryGetValue(key, out var value) ? value : null;
}
```

### Null Conditional Operators
```csharp
var result = _service?.GetData()?.Property ?? defaultValue;
```

---

## Performance Budgets (Feature 005 Targets)

- Settings screen load: <500ms
- VISUAL API calls: <2s
- Offline mode activation: <100ms
- Custom control render: <16ms (60 FPS)
- Database queries: <500ms
- Configuration retrieval: <100ms

---

## Existing ViewModels to Reference

1. **MainViewModel** - Command patterns, test integration
2. **SplashViewModel** - Boot sequence integration
3. **DebugTerminalViewModel** - Diagnostic patterns
4. **CredentialDialogViewModel** - Dialog patterns

---

## Existing Services to Reference

1. **ConfigurationService** - Settings persistence
2. **BootOrchestrator** - Initialization patterns
3. **DiagnosticsService** - Performance monitoring
4. **SecretsService** - Platform-specific implementations
