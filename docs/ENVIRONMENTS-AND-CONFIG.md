# How this document works with /specify (Spec-Kit)

As a spec author, I want one source of truth for environments, settings, platforms, secrets, and feature switches, so that every /specify plan is consistent and compliant.

## Access and Audience

- As an engineer using /specify, I want to know where the prompt lives, so that I run the correct template: .github/prompts/specify.prompt.md.
- As anyone creating a new feature plan, I want to know who this section is for, so that I use it during planning: all authors running /specify.

## Technical plan (spec.md)

- Platform scope
  - As a spec author, I document “Windows Desktop + Android only,” so that unsupported OS are excluded (see “Repository Cleanup Directive”).
- Setting override order
  - As a developer, I describe precedence “Environment Variables > User Configuration > App Defaults,” so that resolution is deterministic (see “Configuration Overlay Precedence”).
- Picking the environment
  - As a developer, I specify MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → DOTNET_ENVIRONMENT, so that environment detection is predictable (see “Environment Detection”).
- Secrets
  - As a security-conscious engineer, I require ISecretsService (Windows DPAPI / Android KeyStore) and forbid plaintext, so that credentials remain protected (see “Credentials Flow”).
- Feature flags
  - As a planner, I list flags, target environments, and rollout percentages, so that controlled rollouts are explicit (see “Feature Flags”).
- Data access
  - As a platform owner, I state “Desktop uses MySQL directly; Android uses API when available,” so that data paths are correct (see “MySQL” and “API (App Server)”).
- Quality rules
  - As a reviewer, I reference clear overrides, safe fallbacks, resiliency on setting changes, and no unsupported OS mentions, so that the spec meets standards (see “For AI Agents” and “Constitutional Alignment”).

## Friendly overview (overview.md)

- As a reader, I want a plain-language summary of supported platforms (Windows + Android), environment selection, override rules, secure password storage, and gradual feature rollouts, so that I can understand behavior quickly.
- As a writer, I include concrete examples from “Setting Environment (Windows Desktop)” and “Using Secure Credentials,” so that readers can copy/paste working steps.

## Acceptance criteria

- As a tester, I can change MTM_ENVIRONMENT and see the environment update without restart, or the doc clearly explains the hot-reload behavior.
- As a tester, I see an environment variable override beat the same key set via ConfigurationService.SetValue(); defaults apply only when nothing else is set.
- As a security reviewer, I can read secrets via ISecretsService and confirm no plaintext secrets appear in logs.
- As a feature owner, I can toggle flags at runtime and observe rollout percentage behavior.
- As a release engineer, I can connect Desktop builds to MySQL with pooling; Android builds use the API once available.
- As a doc auditor, I find no remaining references to macOS, Linux, or iOS for this feature in code or docs.

## When to add [NEEDS CLARIFICATION] in spec.md

- As a spec author, I mark [NEEDS CLARIFICATION] if any of the following are unknown, so that owners can provide inputs:
  - API base URL per environment, HTTPS/TLS policy, and Polly retry/timeout defaults.
  - Visual ERP polling cadence and cache refresh timing.
  - Android offline sync/write-back behavior.
  - API rate limits and back-off policy.
  - Any per-environment keys or credentials not yet defined.

## Tips when using /specify

- As a spec author, I link to this file under “Non-Functional Requirements” and “Security,” so that reviewers can trace standards.
- As a spec author, I reuse exact section titles (“Configuration Overlay Precedence,” “Environment Detection,” “Credentials Flow,” “Feature Flags,” “Repository Cleanup Directive”), so that cross-references stay accurate.
- As a spec author, I keep numbering aligned between spec.md (FR-xxx) and overview.md (Requirement x), so that acceptance maps cleanly to features.

## Dictonary (Technica terms)

- /specify — Spec-Kit command that generates a feature spec from a prompt template.
- Spec-Kit — Spec-driven development toolkit and workflow used in this project.
- spec.md — Canonical feature specification file produced by /specify.
- overview.md — Plain-language companion overview to spec.md.
- .github/prompts/specify.prompt.md — Prompt template file consumed by /specify.
- Repository Cleanup Directive — Policy to remove unsupported platforms; only Windows + Android remain.
- Environment Variables — OS-level key/value pairs that have highest config precedence.
- User Configuration — Runtime-set values persisted by the ConfigurationService for the user.
- App Defaults — Fallback values defined in code when no overrides exist.
- Configuration Overlay Precedence — Deterministic order: Env Vars > User Config > App Defaults.
- MTM_ENVIRONMENT — Primary variable controlling selected environment.
- ASPNETCORE_ENVIRONMENT — Secondary .NET environment variable signal.
- DOTNET_ENVIRONMENT — Tertiary .NET environment variable signal.
- Environment Detection — Logic resolving the active environment from the variables above.
- ISecretsService — Abstraction for OS-native secure secret storage and retrieval.
- Windows DPAPI — Windows Data Protection API used to encrypt/decrypt secrets.
- Android KeyStore — Android secure keystore (hardware/software-backed) for secrets.
- Credentials Flow — Rules and steps for storing and retrieving credentials securely.
- Feature flags — Runtime switches controlling availability of features.
- Rollout percentage — Gradual enablement of a flag to a percentage of users/sessions.
- MySQL — Relational database used directly by the desktop app.
- API (App Server) — Planned HTTP service used by Android and future clients.
- ConfigurationService.SetValue() — API to set user configuration values at runtime.
- Hot-reload — Applying configuration changes without restarting the application.
- Connection pooling — Reusing database connections to reduce open/close overhead.
- [NEEDS CLARIFICATION] — Marker for unresolved decisions requiring inputs.
- HTTPS/TLS — Encrypted transport protocols for secure API communication.
- Polly — .NET resilience library for retries, timeouts, and circuit breakers.
- Visual ERP — External ERP system accessed read-only via API Toolkit.
- Polling cadence — Frequency for fetching updates from a data source.
- Cache refresh — Updating local cache from the source of truth.
- Offline sync — Reconciling queued changes once connectivity is restored.
- Write-back — Sending locally staged changes to the server.
- Rate limits — Server-imposed request quotas/time windows.
- Back-off policy — Strategy to delay/retry after failures or throttling.
- Non-Functional Requirements — Security, performance, reliability, and similar qualities.
- Cross-references — Pointers to related sections/files for context.
- FR-xxx — Functional requirement identifier pattern used in specs.
- Platform scope — Declared list of supported platforms for the feature.
- Data access — Defined technologies/paths used to read/write data.
- Quality rules — Review checklist ensuring consistency, safety, and compliance.

# Environments and Configuration

> Purpose: Clean separation of Dev, Staging, and Prod environments to prevent configuration drift and surprise behavior.
> Platform Support: Windows Desktop + Android only

## Overview

This document defines environment-specific settings, configuration overlay precedence, and compliance with MTM Avalonia Template constitutional standards. Configuration services are already implemented with full layered precedence and secure credential storage.

---

## For Humans

- When Used: Setup, deployment, troubleshooting.
- Dependencies: ✅ ConfigurationService, ✅ SecretsService (Windows DPAPI / Android KeyStore), ✅ FeatureFlagEvaluator.
- Consumers: All components reading endpoints or toggling features.
- Priority: High
- Status: Implemented (Boot Feature 001)

---

## For AI Agents

- Intent: Layered configuration (env vars > user config > app defaults) with deterministic resolution and safe fallbacks.
- Implementation Status: ✅ ConfigurationService with event-driven hot reload, ✅ OS-native secrets (Windows/Android only), ✅ Feature flag evaluation with rollout percentages.
- Dependencies: ✅ ConfigurationService, ✅ SecretsServiceFactory (Windows/Android), ✅ FeatureFlagEvaluator.
- Consumers: All services needing endpoints/flags.
- Non-Functionals: Deterministic resolution; minimal runtime mutation; thread-safe.
- Priority: High

---

## Repository Cleanup Directive: Unsupported OS Removal (Windows + Android only)

Effective immediately, remove all references and code paths for unsupported platforms. Only Windows Desktop and Android are supported.

Action items:
- Code
  - Remove types/files specific to other platforms (e.g., any MacOSSecretsService, LinuxSecretsService, iOS projects).
  - Simplify platform factories to only Windows and Android.

    ```csharp
    public static ISecretsService Create(ILoggerFactory loggerFactory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsSecretsService(loggerFactory.CreateLogger<WindowsSecretsService>());
        if (OperatingSystem.IsAndroid())
            return new AndroidSecretsService(loggerFactory.CreateLogger<AndroidSecretsService>());
        throw new PlatformNotSupportedException("Only Windows and Android are supported.");
    }
    ```

  - Verify .csproj files: keep only supported targets/RIDs; remove macOS/Linux/iOS targets and runtime identifiers.
  - CI: Remove non-Windows/non-Android build agents from GitHub Actions matrices.
- Docs
  - Remove mentions, sections, or caveats about unsupported platforms.
  - State platform scope as “Windows Desktop + Android only.”
- Search commands (examples)
  - PowerShell (repo root):

    ```powershell
    Get-ChildItem -Recurse -File | Select-String -Pattern 'macOS|OSX|OS X|Linux|iOS|Darwin|MacOSSecretsService|LinuxSecretsService' -SimpleMatch -CaseSensitive:$false
    ```

  - ripgrep:

    ```bash
    rg -n -i "(macos|osx|linux|ios|darwin|MacOSSecretsService|LinuxSecretsService)"
    ```

- Validation
  - Build on Windows; run Android build.
  - Ensure no remaining references to unsupported OS in code or docs.

---

## Configuration Overlay Precedence (Implemented)

1. Environment Variables (highest precedence) - Keys use underscore format (e.g., MTM_ENVIRONMENT)
2. User Configuration (runtime-set values via ConfigurationService.SetValue())
3. Application Defaults (fallback values in code)

Note: File-based configuration (appsettings.{Environment}.json) is not currently implemented. Configuration is managed in-memory with environment variable overrides and user-set values persisted through ConfigurationService.

---

## Key Settings

### Visual (Read-Only) - Partially Implemented

- Toolkit Integration: ✅ VisualApiClient with whitelist enforcement, authentication provider
- Read-Only Enforce Flag: Visual.ReadOnly=true (architectural principle - enforced by design, not runtime flag)
- Poll Cadence: ⏳ Not yet implemented (master cache polling planned for future)
- Table Size Awareness (for reference):
  - PART table: ~117 fields (MTMFG Tables.csv lines 1657–1773)
  - LOCATION table: ~14 fields (lines 1513–1526)
  - WAREHOUSE table: ~16 fields (lines 4229–4244)
  - SHOP_RESOURCE table: ~42 fields (lines 3452–3493)
- Citations: All toolkit commands require citation format: Reference-{File Name} - {Chapter/Section/Page}
- Status: VisualApiClient implemented with whitelist enforcement; polling and cache refresh not yet implemented

### Credentials Flow - Implemented

- Validation: Visual username/password stored in OS-native secure storage (Windows DPAPI / Android KeyStore)
- Security: ✅ No plaintext secrets in logs or files; ✅ WindowsSecretsService and AndroidSecretsService implemented
- Platform Support: Windows Desktop and Android only
- Field Reference: Visual APPLICATION_USER.USER_PWD nvarchar(90) for password hash validation
- Status: SecretsServiceFactory with platform-specific implementations operational

### API (App Server) - ⏳ Not Implemented

- Base URL: Per environment (not yet configured)
- TLS: Required (planned)
- Timeouts/Retry Policy: Configurable (planned with Polly)
- Rate Limits: Enforced (planned)
- Status: API server not yet built; desktop uses direct MySQL, Android will use API

### MySQL (MAMP, Desktop-Only Direct) - Implemented

- Connection: ✅ MySqlClient with connection pooling (Desktop: 2-10, Android: 1-5 connections)
- Platform Support: Desktop uses direct MySQL connection; Android uses API (when API server is implemented)
- Pooling: ✅ Configurable pool sizes per platform
- Status: MySqlClient fully operational for desktop; Android will transition to API-only access

### Feature Flags - Implemented

Core feature flag system operational via FeatureFlagEvaluator:

- ✅ Visual.UseForItems - Use Visual ERP for item/part data
- ✅ Visual.UseForLocations - Use Visual ERP for location data
- ✅ Visual.UseForWorkCenters - Use Visual ERP for work center data
- ✅ OfflineModeAllowed - Allow offline operation with cached data
- ✅ Printing.Enabled - Enable printing functionality

Implementation Details:
- Environment-based flag evaluation
- Rollout percentage support (0-100%)
- Runtime flag updates with SetEnabledAsync()
- Automatic environment detection (MTM_ENVIRONMENT / DOTNET_ENVIRONMENT / DEBUG build default)

---

## Environment Detection (Implemented)

The application automatically detects the current environment using the following priority:

1. MTM_ENVIRONMENT environment variable
2. ASPNETCORE_ENVIRONMENT environment variable
3. DOTNET_ENVIRONMENT environment variable
4. Default: Development (DEBUG builds) / Production (RELEASE builds)

Supported Environments: Development, Staging, Production

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

Environment is set via build configuration in MTM_Template_Application.Android.csproj:

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

---

## Implementation Checklist

### Completed (Boot Feature 001)

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

### Pending (Future Features)

- [ ] File-based configuration (appsettings.{Environment}.json loading)
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

- ✅ CompiledBinding: All configuration UI uses x:DataType and {CompiledBinding} in Avalonia XAML
- ✅ DI: Configuration services registered via ServiceCollectionExtensions in Program.cs
- ✅ Null Safety: All config accessors use nullable reference types (T?, string?)
- ✅ Security: Credentials stored in OS-native storage (Windows DPAPI / Android KeyStore), never logged
- ✅ Error Resilience: Safe fallback to defaults when keys not found; no exceptions on missing config
- ✅ Testing: Configuration logic covered by xUnit integration tests (ConfigurationTests.cs)
- ✅ Thread Safety: Configuration access protected with lock statements
- ✅ Logging: Structured logging with Serilog for all configuration operations

### Design Patterns Used

- Factory Pattern: SecretsServiceFactory for platform-specific credential storage
- Strategy Pattern: Different secrets implementations for Windows vs Android
- Observer Pattern: Event-driven configuration change notifications
- Singleton Pattern: ConfigurationService and FeatureFlagEvaluator registered as singletons
- Repository Pattern: MySqlClient abstracts database access with connection pooling

---

Last updated: 2025-10-05 | Platform Support: Windows + Android only | See constitution.md for project principles.
