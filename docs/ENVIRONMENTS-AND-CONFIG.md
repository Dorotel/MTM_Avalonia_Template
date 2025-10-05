# Environments and Configuration

> **Purpose**: Clean separation of Dev, Staging, and Prod environments to prevent configuration drift and surprise behavior.
> **Platform Support**: Windows Desktop + Android (macOS/Linux desktop not supported)

## Overview

This document defines environment-specific settings, configuration overlay precedence, and compliance with MTM Avalonia Template constitutional standards. Configuration services are already implemented with full layered precedence and secure credential storage.

---

## For Humans

- **When Used**: Setup, deployment, troubleshooting.
- **Dependencies**: ✅ ConfigurationService, ✅ SecretsService (Windows DPAPI / Android KeyStore), ✅ FeatureFlagEvaluator.
- **Consumers**: All components reading endpoints or toggling features.
- **Priority**: **High**
- **Status**: **Implemented** (Boot Feature 001)

---

## For AI Agents

- **Intent**: Layered configuration (env vars > user config > app defaults) with deterministic resolution and safe fallbacks.
- **Implementation Status**: ✅ ConfigurationService with event-driven hot reload, ✅ OS-native secrets (Windows/Android only), ✅ Feature flag evaluation with rollout percentages.
- **Dependencies**: ✅ ConfigurationService, ✅ SecretsServiceFactory (Windows/Android), ✅ FeatureFlagEvaluator.
- **Consumers**: All services needing endpoints/flags.
- **Non-Functionals**: Deterministic resolution; minimal runtime mutation; thread-safe.
- **Priority**: **High**

---

## Configuration Overlay Precedence (✅ Implemented)

1. **Environment Variables** (highest precedence) - Keys use underscore format (e.g., `MTM_ENVIRONMENT`)
2. **User Configuration** (runtime-set values via `ConfigurationService.SetValue()`)
3. **Application Defaults** (fallback values in code)

**Note**: File-based configuration (`appsettings.{Environment}.json`) is not currently implemented. Configuration is managed in-memory with environment variable overrides and user-set values persisted through ConfigurationService.

---

## Key Settings

### Visual (Read-Only) - ⚠️ Partially Implemented

- **Toolkit Integration**: ✅ VisualApiClient with whitelist enforcement, authentication provider
- **Read-Only Enforce Flag**: `Visual.ReadOnly=true` (**architectural principle** - enforced by design, not runtime flag)
- **Poll Cadence**: ⏳ Not yet implemented (master cache polling planned for future)
- **Table Size Awareness** (for reference):
  - `PART` table: ~117 fields (`MTMFG Tables.csv` lines 1657–1773)
  - `LOCATION` table: ~14 fields (lines 1513–1526)
  - `WAREHOUSE` table: ~16 fields (lines 4229–4244)
  - `SHOP_RESOURCE` table: ~42 fields (lines 3452–3493)
- **Citations**: All toolkit commands require citation format: `Reference-{File Name} - {Chapter/Section/Page}`
- **Status**: VisualApiClient implemented with whitelist enforcement; polling and cache refresh not yet implemented

### Credentials Flow - ✅ Implemented

- **Validation**: Visual username/password stored in OS-native secure storage (Windows DPAPI / Android KeyStore)
- **Security**: ✅ No plaintext secrets in logs or files; ✅ WindowsSecretsService and AndroidSecretsService implemented
- **Platform Support**: Windows Desktop (DPAPI) and Android (KeyStore) only - **macOS not supported**
- **Field Reference**: Visual `APPLICATION_USER.USER_PWD` nvarchar(90) for password hash validation
- **Status**: SecretsServiceFactory with platform-specific implementations operational

### API (App Server) - ⏳ Not Implemented

- **Base URL**: Per environment (not yet configured)
- **TLS**: Required (planned)
- **Timeouts/Retry Policy**: Configurable (planned with Polly)
- **Rate Limits**: Enforced (planned)
- **Status**: API server not yet built; desktop uses direct MySQL, Android will use API

### MySQL (MAMP, Desktop-Only Direct) - ✅ Implemented

- **Connection**: ✅ MySqlClient with connection pooling (Desktop: 2-10, Android: 1-5 connections)
- **Platform Support**: Desktop uses direct MySQL connection; Android uses API (when API server is implemented)
- **Pooling**: ✅ Configurable pool sizes per platform
- **Status**: MySqlClient fully operational for desktop; Android will transition to API-only access

### Feature Flags - ✅ Implemented

Core feature flag system operational via FeatureFlagEvaluator:

- ✅ `Visual.UseForItems` - Use Visual ERP for item/part data
- ✅ `Visual.UseForLocations` - Use Visual ERP for location data
- ✅ `Visual.UseForWorkCenters` - Use Visual ERP for work center data
- ✅ `OfflineModeAllowed` - Allow offline operation with cached data
- ✅ `Printing.Enabled` - Enable printing functionality

**Implementation Details**:
- Environment-based flag evaluation
- Rollout percentage support (0-100%)
- Runtime flag updates with `SetEnabledAsync()`
- Automatic environment detection (MTM_ENVIRONMENT / DOTNET_ENVIRONMENT / DEBUG build default)

---

## Environment Detection (✅ Implemented)

The application automatically detects the current environment using the following priority:

1. `MTM_ENVIRONMENT` environment variable
2. `ASPNETCORE_ENVIRONMENT` environment variable
3. `DOTNET_ENVIRONMENT` environment variable
4. Default: `Development` (DEBUG builds) / `Production` (RELEASE builds)

**Supported Environments**: `Development`, `Staging`, `Production`

### Setting Environment (Windows Desktop)

```powershell
# Set environment for current session
$env:MTM_ENVIRONMENT = "Development"

# Set permanently (user-level)
[Environment]::SetEnvironmentVariable("MTM_ENVIRONMENT", "Development", "User")

# Set permanently (system-level, requires admin)
[Environment]::SetEnvironmentVariable("MTM_ENVIRONMENT", "Production", "Machine")
```

### Setting Environment (Android)

Environment is set via build configuration in `MTM_Template_Application.Android.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
    <DefineConstants>DEBUG;ANDROID</DefineConstants>
</PropertyGroup>
```

---

## Configuration Usage Examples

### Reading Configuration Values

```csharp
public class MyService
{
    private readonly IConfigurationService _config;

    public MyService(IConfigurationService config)
    {
        _config = config;
    }

    public async Task DoSomethingAsync()
    {
        // Get configuration value with default fallback
        var apiUrl = _config.GetValue<string>("API:BaseUrl", "https://localhost:5001");
        var timeout = _config.GetValue<int>("API:TimeoutSeconds", 30);
        var isFeatureEnabled = _config.GetValue<bool>("Features:NewFeature", false);
    }
}
```

### Setting Configuration Values at Runtime

```csharp
// Update configuration value
await _config.SetValue("API:BaseUrl", "https://api.production.com");

// Subscribe to configuration changes
_config.OnConfigurationChanged += (sender, args) =>
{
    Console.WriteLine($"Config {args.Key} changed from {args.OldValue} to {args.NewValue}");
};
```

### Using Feature Flags

```csharp
public class MyViewModel
{
    private readonly FeatureFlagEvaluator _flags;

    public MyViewModel(FeatureFlagEvaluator flags)
    {
        _flags = flags;

        // Register flags
        _flags.RegisterFlag(new FeatureFlag
        {
            Name = "Visual.UseForItems",
            IsEnabled = true,
            Environment = "Production",
            RolloutPercentage = 100
        });
    }

    public async Task<bool> ShouldUseVisualForItemsAsync()
    {
        return await _flags.IsEnabledAsync("Visual.UseForItems");
    }
}
```

### Using Secure Credentials

```csharp
public class AuthService
{
    private readonly ISecretsService _secrets;

    public AuthService(ISecretsService secrets)
    {
        _secrets = secrets;
    }

    public async Task StoreCredentialsAsync(string username, string password)
    {
        // Store securely in OS-native storage (DPAPI on Windows, KeyStore on Android)
        await _secrets.SetSecretAsync("Visual.Username", username);
        await _secrets.SetSecretAsync("Visual.Password", password);
    }

    public async Task<(string username, string password)?> GetCredentialsAsync()
    {
        var username = await _secrets.GetSecretAsync("Visual.Username");
        var password = await _secrets.GetSecretAsync("Visual.Password");

        if (username == null || password == null)
            return null;

        return (username, password);
    }
}
```

---

## Platform-Specific Considerations

### Windows Desktop

- ✅ Direct MySQL connection via MySqlClient (pooled, 2-10 connections)
- ✅ Credentials stored in Windows DPAPI (Data Protection API)
- ✅ Full offline mode support with cached data
- ✅ Configuration persisted in user profile

### Android

- ✅ Credentials stored in Android KeyStore (hardware-backed when available)
- ⚠️ MySQL connection pool limited to 1-5 connections (resource-constrained)
- ⏳ Will use API server when implemented (no direct MySQL access)
- ⏳ Offline sync queue for write operations

### Not Supported

- ❌ macOS desktop (SecretsServiceFactory throws PlatformNotSupportedException)
- ❌ Linux desktop (not tested; may work with Android KeyStore but unsupported)
- ❌ iOS (not implemented)

---

## Implementation Checklist

### ✅ Completed (Boot Feature 001)

- [x] ConfigurationService with layered precedence (env vars > user config > defaults)
- [x] Thread-safe configuration access and updates
- [x] Event-driven configuration change notifications
- [x] SecretsServiceFactory with Windows DPAPI support
- [x] SecretsServiceFactory with Android KeyStore support
- [x] FeatureFlagEvaluator with environment-based evaluation
- [x] Feature flag rollout percentage support
- [x] MySqlClient with connection pooling (platform-specific pool sizes)
- [x] VisualApiClient with command whitelist enforcement
- [x] Environment auto-detection (MTM_ENVIRONMENT/DOTNET_ENVIRONMENT/DEBUG)

### ⏳ Pending (Future Features)

- [ ] File-based configuration (`appsettings.{Environment}.json` loading)
- [ ] Remote configuration server support
- [ ] Configuration schema validation
- [ ] Visual ERP master data polling and cache refresh
- [ ] API server for Android client access
- [ ] Configuration profiles (multi-plant deployments)
- [ ] Configuration audit logging with timestamps
- [ ] Encrypted configuration sections for sensitive data
- [ ] Hot-reload for feature flags from remote config

---

## Constitutional Alignment

All implemented configuration services comply with MTM Avalonia Template standards:

- ✅ **CompiledBinding**: All configuration UI uses `x:DataType` and `{CompiledBinding}` in Avalonia XAML
- ✅ **DI**: Configuration services registered via `ServiceCollectionExtensions` in `Program.cs`
- ✅ **Null Safety**: All config accessors use nullable reference types (`T?`, `string?`)
- ✅ **Security**: Credentials stored in OS-native storage (Windows DPAPI / Android KeyStore), never logged
- ✅ **Error Resilience**: Safe fallback to defaults when keys not found; no exceptions on missing config
- ✅ **Testing**: Configuration logic covered by xUnit integration tests (ConfigurationTests.cs)
- ✅ **Thread Safety**: Configuration access protected with lock statements
- ✅ **Logging**: Structured logging with Serilog for all configuration operations

### Design Patterns Used

- **Factory Pattern**: SecretsServiceFactory for platform-specific credential storage
- **Strategy Pattern**: Different secrets implementations for Windows vs Android
- **Observer Pattern**: Event-driven configuration change notifications
- **Singleton Pattern**: ConfigurationService and FeatureFlagEvaluator registered as singletons
- **Repository Pattern**: MySqlClient abstracts database access with connection pooling

---

_Last updated: 2025-10-05 | Platform Support: Windows + Android only | See [constitution.md](../.specify/memory/constitution.md) for project principles._
