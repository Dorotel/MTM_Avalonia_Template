# How-To-Use Guide: Boot Sequence for GitHub Copilot

**Feature**: Boot Sequence — Splash-First, Services Initialization Order
**Status**: ✅ **COMPLETE - PRODUCTION READY**
**Target**: GitHub Copilot and AI-assisted development
**Date**: 2025-10-02 | **Updated**: 2025-10-05
**Implementation**: 100% (175/175 tasks) | **Pull Request**: #8

## Purpose

This guide helps GitHub Copilot understand how to work with the Boot Sequence feature effectively. It provides context, patterns, and examples for generating code that adheres to the project's architecture and constitutional principles.

---

## Quick Reference for Copilot

### Key Technologies Stack

```csharp
// C# .NET 9.0 with nullable reference types
#nullable enable

// MVVM Community Toolkit 8.3+ (REQUIRED - NO ReactiveUI)
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

// Avalonia 11.3+ for cross-platform UI
using Avalonia;
using Avalonia.Controls;

// OpenTelemetry for structured logging
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;

// Polly for retry/circuit breakers
using Polly;
using Polly.CircuitBreaker;
```

### Constitutional Patterns (NON-NEGOTIABLE)

1. **Always use MVVM Community Toolkit attributes**:

```csharp
[ObservableObject]
public partial class SplashViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    [ObservableProperty]
    private int _progressPercentage;

    [RelayCommand]
    private async Task RetryAsync()
    {
        // Command implementation
    }
}
```

2. **Always use ArgumentNullException.ThrowIfNull for parameters**:

```csharp
public ConfigurationService(ILogger<ConfigurationService> logger)
{
    ArgumentNullException.ThrowIfNull(logger);
    _logger = logger;
}
```

3. **Always enable nullable reference types**:

```csharp
public string? ErrorMessage { get; set; }  // Nullable
public string ConfigPath { get; set; } = string.Empty;  // Non-nullable
```

---

## Architecture Patterns

### 1. Service Registration Pattern

```csharp
// In Program.cs or Startup.cs
public static void ConfigureServices(IServiceCollection services)
{
    // Singleton for boot orchestration
    services.AddSingleton<IBootOrchestrator, BootOrchestrator>();

    // Scoped for per-session services
    services.AddScoped<IConfigurationService, ConfigurationService>();

    // Transient for stateless services
    services.AddTransient<IValidationService, ValidationService>();
}
```

### 2. Boot Stage Pattern

```csharp
public async Task<BootResult> ExecuteStage1Async(CancellationToken cancellationToken)
{
    var stage = new StageMetrics
    {
        StageNumber = 1,
        StageName = "Services",
        StartTimestamp = DateTimeOffset.UtcNow
    };

    try
    {
        // Stage 1: Initialize core services with dependency awareness
        await InitializeLoggingAsync(cancellationToken);
        await InitializeConfigurationAsync(cancellationToken);

        // Parallel initialization for independent services
        await Task.WhenAll(
            InitializeDiagnosticsAsync(cancellationToken),
            InitializeSecretsAsync(cancellationToken),
            InitializeLocalizationAsync(cancellationToken)
        );

        // Data layer depends on secrets and config
        await InitializeDataLayerAsync(cancellationToken);

        stage.EndTimestamp = DateTimeOffset.UtcNow;
        stage.ProgressPercentage = 100;

        return BootResult.Success(stage);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Stage 1 failed");
        return BootResult.Failure(stage, ErrorCategory.Configuration, ex.Message);
    }
}
```

### 3. Exponential Backoff Retry Pattern

```csharp
// Using Polly for retry with exponential backoff (1s, 2s, 4s, 8s, 16s)
private readonly IAsyncPolicy _retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            logger.LogWarning(
                "Retry {RetryCount} after {Delay}s due to {Exception}",
                retryCount, timeSpan.TotalSeconds, exception.Message);
        });

public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
{
    return await _retryPolicy.ExecuteAsync(operation);
}
```

### 4. Circuit Breaker Pattern

```csharp
// Circuit breaker: 5 consecutive failures, 30s->10m exponential backoff
private readonly IAsyncPolicy _circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (exception, duration) =>
        {
            logger.LogError("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
        },
        onReset: () =>
        {
            logger.LogInformation("Circuit breaker reset");
        });
```

### 5. Structured Logging Pattern (OpenTelemetry)

```csharp
// Use structured logging with semantic fields
_logger.LogInformation(
    "Boot stage {StageNumber} completed in {Duration}ms with {Status}",
    stageNumber,
    durationMs,
    status);

// For errors, include exception and context
_logger.LogError(
    exception,
    "Failed to initialize {ServiceName} after {Attempts} attempts",
    serviceName,
    attemptCount);

// Never log sensitive data - it will be auto-redacted
// ❌ WRONG: _logger.LogDebug("Password: {Password}", password);
// ✅ RIGHT: _logger.LogDebug("Credential validation attempted");
```

### 6. Configuration Management Pattern

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        // Override precedence: CLI > Env > User > App > Default
        return _configuration.GetValue<T>(key, defaultValue);
    }

    public async Task<bool> HotReloadAsync(string settingKey, object newValue)
    {
        // Only hot-reload if setting is marked as hot-reloadable in schema
        var metadata = _configSchema.GetMetadata(settingKey);
        if (!metadata.IsHotReloadable)
        {
            _logger.LogWarning("Setting {Key} requires restart", settingKey);
            return false;
        }

        // Validate before applying
        var validationResult = await ValidateSettingAsync(settingKey, newValue);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Invalid value for {Key}: {Errors}",
                settingKey, string.Join(", ", validationResult.Errors));
            return false;
        }

        // Apply and audit
        _configuration[settingKey] = newValue?.ToString();
        await AuditConfigChangeAsync(settingKey, newValue);
        return true;
    }
}
```

### 7. OS-Native Secrets Storage Pattern

```csharp
// Windows: Credential Manager
public class WindowsSecretsService : ISecretsService
{
    public async Task<string?> GetSecretAsync(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        using var cred = new Credential();
        cred.Target = $"MTM.Template.{key}";

        if (!cred.Load())
        {
            _logger.LogWarning("Secret {Key} not found", key);
            return null;
        }

        return cred.Password; // Auto-decrypted by DPAPI
    }

    public async Task SetSecretAsync(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        using var cred = new Credential
        {
            Target = $"MTM.Template.{key}",
            Password = value,
            Type = CredentialType.Generic,
            PersistenceType = PersistenceType.LocalMachine
        };

        cred.Save(); // Auto-encrypted by DPAPI
        _logger.LogInformation("Secret {Key} stored securely", key);
    }
}
```

### 8. Visual Master Data Caching Pattern

```csharp
public class CacheService : ICacheService
{
    public async Task<T?> GetOrFetchAsync<T>(
        string key,
        CachedEntityType entityType,
        Func<Task<T>> fetchFunc,
        CancellationToken cancellationToken = default)
    {
        // Check cache first
        var cached = await _cacheStore.GetAsync<T>(key, cancellationToken);
        if (cached != null && !IsStale(cached, entityType))
        {
            await UpdateHitCountAsync(key);
            return cached.Data;
        }

        // Cache miss or stale - fetch from Visual
        _logger.LogInformation("Cache miss for {Key}, fetching from source", key);
        var data = await fetchFunc();

        // Store with TTL based on entity type
        var ttl = GetTTLForEntityType(entityType);
        await _cacheStore.SetAsync(key, data, ttl, cancellationToken);

        return data;
    }

    private TimeSpan GetTTLForEntityType(CachedEntityType entityType)
    {
        // Per clarifications: Parts 24h, Others 7d
        return entityType switch
        {
            CachedEntityType.Part => TimeSpan.FromHours(24),
            _ => TimeSpan.FromDays(7)
        };
    }
}
```

### 9. Splash Screen ViewModel Pattern

```csharp
[ObservableObject]
public partial class SplashViewModel : ViewModelBase
{
    private readonly IBootOrchestrator _bootOrchestrator;

    [ObservableProperty]
    private string _statusMessage = "Starting application...";

    [ObservableProperty]
    private int _progressPercentage;

    [ObservableProperty]
    private int _currentStage;

    [ObservableProperty]
    private bool _showError;

    [ObservableProperty]
    private string? _errorMessage;

    public SplashViewModel(IBootOrchestrator bootOrchestrator)
    {
        ArgumentNullException.ThrowIfNull(bootOrchestrator);
        _bootOrchestrator = bootOrchestrator;

        // Subscribe to boot progress events
        _bootOrchestrator.ProgressChanged += OnBootProgressChanged;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            var result = await _bootOrchestrator.ExecuteBootSequenceAsync(CancellationToken.None);

            if (!result.Success)
            {
                ShowError = true;
                ErrorMessage = result.UserFriendlyMessage;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Boot sequence failed unexpectedly");
            ShowError = true;
            ErrorMessage = "An unexpected error occurred during startup.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private async Task RetryAsync()
    {
        ShowError = false;
        ErrorMessage = null;
        await InitializeAsync();
    }

    private bool CanRetry() => ShowError;

    private void OnBootProgressChanged(object? sender, BootProgressEventArgs e)
    {
        CurrentStage = e.StageNumber;
        StatusMessage = e.StatusMessage;
        ProgressPercentage = e.ProgressPercentage;
    }
}
```

---

## Testing Patterns (TDD Required)

### 1. Service Unit Test Pattern

```csharp
public class ConfigurationServiceTests
{
    [Fact]
    public async Task GetValue_WithValidKey_ReturnsValue()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ConfigurationService>>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Database:Timeout"] = "30"
            })
            .Build();
        var service = new ConfigurationService(logger, config);

        // Act
        var result = service.GetValue<int>("Database:Timeout");

        // Assert
        result.Should().Be(30);
    }

    [Fact]
    public async Task HotReload_ForNonReloadableSetting_ReturnsFalse()
    {
        // Arrange - setup test
        // Act - call method
        // Assert - verify expectations
    }
}
```

### 2. Integration Test Pattern

```csharp
public class BootSequenceIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _services;

    [Fact]
    public async Task ExecuteBootSequence_WithAllServicesAvailable_CompletesSuccessfully()
    {
        // Arrange
        var orchestrator = _services.GetRequiredService<IBootOrchestrator>();

        // Act
        var result = await orchestrator.ExecuteBootSequenceAsync(CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalDurationMs.Should().BeLessThan(10000); // <10s target
        result.Stage1DurationMs.Should().BeLessThan(3000); // <3s target
    }
}
```

---

## Common Prompts for Copilot

### Generate a new service

```
Create a service implementing IThemeService with:
- MVVM Community Toolkit patterns (ObservableObject, ObservableProperty)
- Constructor dependency injection with ArgumentNullException.ThrowIfNull
- Methods to switch theme (Light/Dark/Auto) with 200ms animation
- Real-time system theme monitoring
- Structured logging with OpenTelemetry
- Nullable reference types enabled
```

### Generate retry logic

```
Add exponential backoff retry to this method using Polly:
- 5 retries: 1s, 2s, 4s, 8s, 16s
- Add ±25% jitter
- Log each retry attempt with structured logging
- Handle HttpRequestException and TimeoutException
```

### Generate a ViewModel

```
Create SomeViewModel with:
- Inherit from ViewModelBase
- Use [ObservableObject] attribute
- Properties with [ObservableProperty]
- Commands with [RelayCommand]
- Constructor with ILogger and IService dependencies
- ArgumentNullException.ThrowIfNull validation
```

---

## Gotchas and Anti-Patterns to Avoid

### ❌ DON'T USE ReactiveUI

```csharp
// WRONG - ReactiveUI patterns
public class MyViewModel : ReactiveObject
{
    private string _value;
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}
```

### ✅ DO USE MVVM Community Toolkit

```csharp
// CORRECT - MVVM Community Toolkit
[ObservableObject]
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _value = string.Empty;
}
```

### ❌ DON'T Forget Null Checks

```csharp
// WRONG - no null check
public MyService(ILogger logger)
{
    _logger = logger;
}
```

### ✅ DO Validate Parameters

```csharp
// CORRECT - ArgumentNullException.ThrowIfNull
public MyService(ILogger<MyService> logger)
{
    ArgumentNullException.ThrowIfNull(logger);
    _logger = logger;
}
```

### ❌ DON'T Log Sensitive Data

```csharp
// WRONG - logs password
_logger.LogDebug("Login with user {User} password {Password}", user, password);
```

### ✅ DO Redact Automatically

```csharp
// CORRECT - no sensitive data
_logger.LogDebug("Login attempted for user {User}", user);
// Password is automatically redacted if accidentally logged
```

---

## File Organization Reference

When creating new files, follow this structure:

```
Services/
├── Boot/
│   ├── IBootOrchestrator.cs (interface)
│   ├── BootOrchestrator.cs (implementation)
│   ├── BootStage.cs (enum/model)
│   └── BootMetrics.cs (model)
├── Configuration/
│   ├── IConfigurationService.cs
│   ├── ConfigurationService.cs
│   └── Models/ (if complex models needed)
└── [ServiceCategory]/
    ├── I[ServiceName].cs
    ├── [ServiceName].cs
    └── Models/

tests/
├── unit/
│   └── [ServiceName]Tests.cs
├── integration/
│   └── [Feature]IntegrationTests.cs
└── contract/
    └── [Api]ContractTests.cs
```

---

## Quick Decision Tree

**When to use what:**

- **Singleton**: Boot orchestrator, logging, configuration (one instance app-wide)
- **Scoped**: Per-session services (user session, navigation state)
- **Transient**: Stateless services (validation, mapping, formatters)

**Retry vs Circuit Breaker:**

- **Retry**: Transient failures (network blips, timeout)
- **Circuit Breaker**: Persistent failures (service down, prevent cascading)
- **Both**: Use Polly's PolicyWrap to combine

**Cache TTL:**

- **Parts**: 24 hours (changes frequently)
- **Locations/Warehouses/Work Centers**: 7 days (mostly stable)
- **Configuration**: Per-setting basis
- **Query results**: 1-60 minutes based on volatility

---

## Summary Checklist for Copilot

Before generating code, verify:

- [ ] MVVM Community Toolkit attributes used (NO ReactiveUI)
- [ ] ArgumentNullException.ThrowIfNull for all injected dependencies
- [ ] Nullable reference types enabled (`#nullable enable`)
- [ ] Structured logging with semantic fields
- [ ] No sensitive data in logs
- [ ] Retry logic for transient failures
- [ ] Circuit breaker for service calls
- [ ] Unit tests following TDD (red-green-refactor)
- [ ] XML documentation comments on public APIs
- [ ] Async methods end with `Async` suffix

---

*This guide ensures GitHub Copilot generates code that adheres to constitutional principles and architectural patterns.*
