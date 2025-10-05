# Tasks: Boot Sequence — Splash-First, Services Initialization Order

**Input**: Design documents from `/specs/001-boot-sequence-splash/`
**Prerequisites**: plan.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅, how-to-use.md ✅

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions

- **Shared project**: `MTM_Template_Application/` (Avalonia cross-platform)
- **Desktop entry**: `MTM_Template_Application.Desktop/`
- **Android entry**: `MTM_Template_Application.Android/`
- **Tests**: `tests/` (contract/, integration/, unit/)

---

## Phase 3.1: Setup (Project Initialization)

- [x] T001 Create MTM_Template_Application/Services/ directory structure (Boot/, Configuration/, Secrets/, Logging/, Diagnostics/, DataLayer/, Cache/, Core/, Localization/, Theme/, Navigation/)
- [x] T002 Create MTM_Template_Application/Models/ directory structure (Boot/, Configuration/, Secrets/, Logging/, Diagnostics/, DataLayer/, Cache/, Core/, Localization/, Theme/, Navigation/, ErrorHandling/)
- [x] T003 Create tests/ directory structure (contract/, integration/, unit/)
- [x] T004 Add Avalonia 11.3+ package references to MTM_Template_Application.csproj
- [x] T005 Add CommunityToolkit.Mvvm 8.3+ package reference to MTM_Template_Application.csproj
- [x] T006 Add Serilog.Sinks.Console, Serilog.Sinks.File, Serilog.Sinks.OpenTelemetry package references to MTM_Template_Application.csproj
- [x] T007 Add Polly 8.0+ package reference to MTM_Template_Application.csproj
- [x] T008 Add AutoMapper 13.0+ package reference to MTM_Template_Application.csproj
- [x] T009 Add FluentValidation 11.0+ package reference to MTM_Template_Application.csproj
- [x] T010 Add MySql.Data 8.0+ package reference to MTM_Template_Application.csproj
- [x] T011 Add K4os.Compression.LZ4 package reference for cache compression to MTM_Template_Application.csproj
- [x] T012 Create test project with xUnit, FluentAssertions, NSubstitute references
- [x] T013 [P] Configure .editorconfig for C# formatting (nullable enabled, var usage, spacing)
- [x] T014 [P] Create tests/TestHelpers/MockFactory.cs for NSubstitute mock creation patterns
- [x] T015 [P] Create tests/TestHelpers/TestData.cs for shared test data fixtures

---

## Phase 3.2: Entity Models (Data Model from data-model.md) ⚠️ FOUNDATION

### All models support TDD - create before tests reference them

### Boot Orchestration Models (3 entities)

- [x] T016 [P] BootMetrics model in MTM_Template_Application/Models/Boot/BootMetrics.cs (TotalDurationMs, StageMetrics[], MemoryUsageMB, ServicesInitialized, ErrorsEncountered)
- [x] T017 [P] StageMetrics model in MTM_Template_Application/Models/Boot/StageMetrics.cs (StageNumber, Name, StartTimeUtc, DurationMs, Status, ServicesStarted[], Errors[])

- [x] T018 [P] ServiceMetrics model in MTM_Template_Application/Models/Boot/ServiceMetrics.cs (ServiceName, StartTimeUtc, DurationMs, InitializationStatus, Dependencies[], ErrorMessage?)

### Configuration Models (3 entities)

- [x] T019 [P] ConfigurationProfile model in MTM_Template_Application/Models/Configuration/ConfigurationProfile.cs (ProfileName, IsActive, Settings[], FeatureFlags[], LastModifiedUtc)

- [x] T020 [P] ConfigurationSetting model in MTM_Template_Application/Models/Configuration/ConfigurationSetting.cs (Key, Value, Source, Precedence, IsEncrypted)
- [x] T021 [P] FeatureFlag model in MTM_Template_Application/Models/Configuration/FeatureFlag.cs (Name, IsEnabled, Environment, RolloutPercentage, EvaluatedAt)

### Secrets Models (1 entity)

- [x] T022 [P] SecretEntry model in MTM_Template_Application/Models/Secrets/SecretEntry.cs (Key, EncryptedValue, CreatedUtc, LastAccessedUtc, ExpiresAtUtc?, Metadata)

### Logging Models (2 entities)

- [x] T023 [P] LogEntry model in MTM_Template_Application/Models/Logging/LogEntry.cs (Timestamp, Level, Message, TraceId, SpanId, Attributes, Resource, Scope) - OpenTelemetry format
- [x] T024 [P] TelemetryBatch model in MTM_Template_Application/Models/Logging/TelemetryBatch.cs (BatchId, Entries[], CreatedUtc, Status)

### Diagnostics Models (3 entities)

- [x] T025 [P] DiagnosticResult model in MTM_Template_Application/Models/Diagnostics/DiagnosticResult.cs (CheckName, Status, Message, Details, Timestamp, DurationMs)
- [x] T026 [P] HardwareCapabilities model in MTM_Template_Application/Models/Diagnostics/HardwareCapabilities.cs (TotalMemoryMB, AvailableMemoryMB, ProcessorCount, Platform, ScreenResolution, HasCamera, HasBarcodeScanner)

- [x] T027 [P] DiagnosticIssue model in MTM_Template_Application/Models/Diagnostics/DiagnosticIssue.cs (Severity, Category, Description, ResolutionSteps[], DetectedAt)

### Data Layer Models (2 entities)

- [x] T028 [P] ConnectionPoolMetrics model in MTM_Template_Application/Models/DataLayer/ConnectionPoolMetrics.cs (PoolName, ActiveConnections, IdleConnections, MaxPoolSize, AverageAcquireTimeMs, WaitingRequests)

- [x] T029 [P] CircuitBreakerState model in MTM_Template_Application/Models/DataLayer/CircuitBreakerState.cs (ServiceName, State, FailureCount, LastFailureUtc, NextRetryUtc, OpenedAt?)

### Cache Models (2 entities)

- [x] T030 [P] CacheEntry model in MTM_Template_Application/Models/Cache/CacheEntry.cs (Key, CompressedValue, UncompressedSizeBytes, CreatedUtc, ExpiresAtUtc, LastAccessedUtc, AccessCount, EntityType)
- [x] T031 [P] CacheStatistics model in MTM_Template_Application/Models/Cache/CacheStatistics.cs (TotalEntries, HitCount, MissCount, HitRate, TotalSizeBytes, CompressionRatio, EvictionCount)

### Core Services Models (2 entities)

- [x] T032 [P] MessageEnvelope model in MTM_Template_Application/Models/Core/MessageEnvelope.cs (MessageId, Type, Payload, Timestamp, CorrelationId, DeliveryCount, ExpiresAt?)

- [x] T033 [P] ValidationRuleMetadata model in MTM_Template_Application/Models/Core/ValidationRuleMetadata.cs (RuleName, PropertyName, Severity, ErrorMessage, ValidatorType)

### Localization Models (2 entities)

- [x] T034 [P] LocalizationSetting model in MTM_Template_Application/Models/Localization/LocalizationSetting.cs (Culture, IsActive, FallbackCulture, SupportedLanguages[], DateFormat, NumberFormat)
- [x] T035 [P] MissingTranslation model in MTM_Template_Application/Models/Localization/MissingTranslation.cs (Key, Culture, FallbackValue, ReportedAt, Frequency)

### Theme Models (1 entity)

- [x] T036 [P] ThemeConfiguration model in MTM_Template_Application/Models/Theme/ThemeConfiguration.cs (ThemeMode, IsDarkMode, AccentColor, FontSize, HighContrast, LastChangedUtc)

### Navigation Models (1 entity)

- [x] T037 [P] NavigationHistoryEntry model in MTM_Template_Application/Models/Navigation/NavigationHistoryEntry.cs (ViewName, NavigatedAtUtc, Parameters, CanGoBack, CanGoForward)

### Error Handling Models (1 entity)

- [x] T038 [P] ErrorReport model in MTM_Template_Application/Models/ErrorHandling/ErrorReport.cs (ErrorId, Message, StackTrace, Severity, Category, OccurredAt, DiagnosticBundle?)

---

## Phase 3.3: Service Interfaces (Contract Definition)

**Define interfaces before implementation - enables mocking in tests**

- [x] T039 [P] IBootOrchestrator interface in MTM_Template_Application/Services/Boot/IBootOrchestrator.cs (ExecuteStage0Async, ExecuteStage1Async, ExecuteStage2Async, GetBootMetrics, OnProgressChanged event)
- [x] T040 [P] IBootStage interface in MTM_Template_Application/Services/Boot/IBootStage.cs (StageNumber, Name, ExecuteAsync, ValidatePreconditions)
- [x] T041 [P] IConfigurationService interface in MTM_Template_Application/Services/Configuration/IConfigurationService.cs (GetValue<T>, SetValue, ReloadAsync, OnConfigurationChanged event)
- [x] T042 [P] ISecretsService interface in MTM_Template_Application/Services/Secrets/ISecretsService.cs (StoreSecretAsync, RetrieveSecretAsync, DeleteSecretAsync, RotateSecretAsync)
- [x] T043 [P] ILoggingService interface in MTM_Template_Application/Services/Logging/ILoggingService.cs (LogInformation, LogWarning, LogError, SetContext, FlushAsync)
- [x] T044 [P] IDiagnosticsService interface in MTM_Template_Application/Services/Diagnostics/IDiagnosticsService.cs (RunAllChecksAsync, RunCheckAsync, GetHardwareCapabilities)
- [x] T045 [P] IMySqlClient interface in MTM_Template_Application/Services/DataLayer/IMySqlClient.cs (ExecuteQueryAsync, ExecuteNonQueryAsync, ExecuteScalarAsync, GetConnectionMetrics)
- [x] T046 [P] IVisualApiClient interface in MTM_Template_Application/Services/DataLayer/IVisualApiClient.cs (ExecuteCommandAsync, IsServerAvailable, GetWhitelistedCommands)
- [x] T047 [P] IHttpApiClient interface in MTM_Template_Application/Services/DataLayer/IHttpApiClient.cs (GetAsync, PostAsync, PutAsync, DeleteAsync)
- [x] T048 [P] ICacheService interface in MTM_Template_Application/Services/Cache/ICacheService.cs (GetAsync<T>, SetAsync<T>, RemoveAsync, ClearAsync, GetStatistics, RefreshAsync)
- [x] T049 [P] IMessageBus interface in MTM_Template_Application/Services/Core/IMessageBus.cs (PublishAsync, SubscribeAsync<T>, UnsubscribeAsync)
- [x] T050 [P] IValidationService interface in MTM_Template_Application/Services/Core/IValidationService.cs (ValidateAsync<T>, RegisterValidator<T>, GetRuleMetadata)
- [x] T051 [P] IMappingService interface in MTM_Template_Application/Services/Core/IMappingService.cs (Map<TSource, TDestination>, MapAsync<TSource, TDestination>)
- [x] T052 [P] ILocalizationService interface in MTM_Template_Application/Services/Localization/ILocalizationService.cs (GetString, SetCulture, GetSupportedCultures, OnLanguageChanged event)
- [x] T053 [P] IThemeService interface in MTM_Template_Application/Services/Theme/IThemeService.cs (SetTheme, GetCurrentTheme, OnThemeChanged event)
- [x] T054 [P] INavigationService interface in MTM_Template_Application/Services/Navigation/INavigationService.cs (NavigateToAsync, GoBackAsync, GoForwardAsync, GetHistory)

---

## Phase 3.4: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.5

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### Integration Tests (from quickstart.md scenarios)

- [x] T055 [P] Integration test: Normal boot sequence (happy path) in tests/integration/BootSequenceTests.cs - Validate Stage 0→1→2 execution, all services initialized, performance <10s, memory <100MB
- [x] T056 [P] Integration test: Configuration loading and override precedence in tests/integration/ConfigurationTests.cs - Validate env vars > user config > app config > defaults, hot-reload
- [x] T057 [P] Integration test: Credential storage and validation in tests/integration/SecretsTests.cs - Validate OS-native storage (Windows DPAPI, macOS Keychain, Android KeyStore), encryption, retrieval
- [x] T058 [P] Integration test: Diagnostic checks and issue detection in tests/integration/DiagnosticsTests.cs - Validate storage, permissions, network, hardware detection
- [x] T059 [P] Integration test: Visual master data caching (population) in tests/integration/VisualCachingTests.cs - Validate initial cache population, LZ4 compression, TTL enforcement
- [x] T060 [P] Integration test: Visual master data caching (cache hits) in tests/integration/VisualCachingTests.cs - Validate cache hits reduce Visual API calls, performance improvement
- [x] T061 [P] Integration test: Visual master data caching (staleness detection) in tests/integration/VisualCachingTests.cs - Validate delta sync, refresh stale entries, background refresh
- [x] T062 [P] Integration test: Visual master data caching (offline mode) in tests/integration/VisualCachingTests.cs - Validate cached-only mode when Visual unavailable, reconnection detection
- [x] T063 [P] Integration test: Logging and telemetry (structured JSON) in tests/integration/LoggingTests.cs - Validate OpenTelemetry format, trace/span IDs, PII redaction, log rotation
- [x] T064 [P] Integration test: Error handling and recovery (network failures) in tests/integration/ErrorHandlingTests.cs - Validate exponential backoff, circuit breaker, diagnostic bundles
- [x] T065 [P] Integration test: Error handling and recovery (configuration errors) in tests/integration/ErrorHandlingTests.cs - Validate fallback config, error reporting, recovery
- [x] T066 [P] Integration test: Accessibility and localization (screen reader) in tests/integration/AccessibilityTests.cs - Validate screen reader announcements, keyboard navigation
- [x] T067 [P] Integration test: Accessibility and localization (language switching) in tests/integration/LocalizationTests.cs - Validate culture change, missing translations, fallback
- [x] T068 [P] Integration test: Accessibility and localization (high contrast) in tests/integration/ThemeTests.cs - Validate high contrast mode, theme switching (Light/Dark/Auto)
- [x] T069 [P] Integration test: Performance validation (boot time) in tests/integration/PerformanceTests.cs - Validate Stage 1 <3s, total boot <10s
- [x] T070 [P] Integration test: Performance validation (memory usage) in tests/integration/PerformanceTests.cs - Validate memory <100MB during boot
- [x] T071 [P] Integration test: Performance validation (parallel initialization) in tests/integration/PerformanceTests.cs - Validate services start in parallel where possible

### Contract Tests (API Validation)

- [x] T072 [P] Contract test: Visual API Toolkit whitelist validation in tests/contract/VisualApiContractTests.cs - Validate only whitelisted commands accepted (see visual-whitelist.md)
- [x] T073 [P] Integration test: Boot cancellation during Stage 1 in tests/integration/BootSequenceTests.cs - Validate cancellation support (FR-009, FR-122): initiate boot sequence, invoke cancel during Stage 1 service initialization, verify CancellationToken propagated to all async operations, confirm resources released (connections closed, temp files cleaned), validate clean exit without exceptions, verify boot metrics recorded cancellation event
- [x] T074 [P] Contract test: Visual API Toolkit authentication in tests/contract/VisualApiContractTests.cs - Validate device certificate + user credentials (Android)
- [x] T075 [P] Contract test: Visual API Toolkit schema dictionary in tests/contract/VisualApiContractTests.cs - Validate table/column name resolution
- [x] T076 [P] Contract test: HTTP API endpoint availability in tests/contract/HttpApiContractTests.cs - Validate Android HTTP API endpoints respond
- [x] T077 [P] Contract test: MySQL connection string validation in tests/contract/MySqlContractTests.cs - Validate connection string format, role-based access
- [x] T078 [P] Contract test: Diagnostics API checks in tests/contract/DiagnosticsContractTests.cs - Validate diagnostic check contracts (storage, permissions, network, hardware)
- [x] T079 [P] Contract test: Android device certificate + two-factor auth in tests/contract/AndroidAuthContractTests.cs - Validate device cert stored in Android KeyStore + user credentials (FR-154)

---

## Phase 3.5: Core Implementation (ONLY after tests are failing)

### Configuration Service Implementation

- [x] T080 ConfigurationService implementation in MTM_Template_Application/Services/Configuration/ConfigurationService.cs - Layered precedence (env vars > user > app > defaults), hot-reload support
- [x] T081 ConfigurationProfile persistence in MTM_Template_Application/Services/Configuration/ConfigurationPersistence.cs - Save/load profiles from local storage
- [x] T082 FeatureFlag evaluation in MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs - Environment-based flag evaluation, rollout percentage

### Secrets Service Implementation

- [x] T083 WindowsSecretsService implementation in MTM_Template_Application/Services/Secrets/WindowsSecretsService.cs - Windows DPAPI integration, credential manager API
- [x] T084 MacOSSecretsService implementation in MTM_Template_Application/Services/Secrets/MacOSSecretsService.cs - macOS Keychain integration
- [x] T085 AndroidSecretsService implementation in MTM_Template_Application/Services/Secrets/AndroidSecretsService.cs - Android KeyStore integration
- [x] T086 SecretsService factory in MTM_Template_Application/Services/Secrets/SecretsServiceFactory.cs - Platform detection, return appropriate implementation

### Logging Service Implementation

- [x] T087 LoggingService implementation in MTM_Template_Application/Services/Logging/LoggingService.cs - Serilog + OpenTelemetry integration, structured JSON format
- [x] T088 PII redaction middleware in MTM_Template_Application/Services/Logging/PiiRedactionMiddleware.cs - Detect and redact sensitive data (SSN, credit cards, passwords)
- [x] T089 Log rotation policy in MTM_Template_Application/Services/Logging/LogRotationPolicy.cs - 10MB max file size, 7 days retention
- [x] T090 Telemetry batch processor in MTM_Template_Application/Services/Logging/TelemetryBatchProcessor.cs - Batch telemetry for efficient transmission

### Diagnostics Service Implementation

- [x] T091 DiagnosticsService implementation in MTM_Template_Application/Services/Diagnostics/DiagnosticsService.cs - Orchestrate all diagnostic checks
- [x] T092 [P] StorageDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/StorageDiagnostic.cs - Verify storage availability, free space
- [x] T093 [P] PermissionsDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/PermissionsDiagnostic.cs - Verify file system, camera, network permissions
- [x] T094 [P] NetworkDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/NetworkDiagnostic.cs - Verify network connectivity (5s timeout)
- [x] T095 [P] HardwareDetection service in MTM_Template_Application/Services/Diagnostics/HardwareDetection.cs - Detect memory, CPU, screen resolution, peripherals

### Data Layer Implementation

- [x] T096 MySqlClient implementation in MTM_Template_Application/Services/DataLayer/MySqlClient.cs - Connection pooling (Desktop: 2-10, Android: 1-5), query execution, role-based access
- [x] T097 VisualApiClient implementation in MTM_Template_Application/Services/DataLayer/VisualApiClient.cs - HTTP client wrapper, command whitelist enforcement, authentication
- [x] T098 HttpApiClient implementation in MTM_Template_Application/Services/DataLayer/HttpApiClient.cs - Generic HTTP client with retry/circuit breaker
- [x] T099 ExponentialBackoffPolicy in MTM_Template_Application/Services/DataLayer/Policies/ExponentialBackoffPolicy.cs - 1s, 2s, 4s, 8s, 16s with ±25% jitter (Polly)
- [x] T100 CircuitBreakerPolicy in MTM_Template_Application/Services/DataLayer/Policies/CircuitBreakerPolicy.cs - 5 consecutive failures, exponential recovery 30s→10m (Polly)
- [x] T101 ConnectionPoolMonitor in MTM_Template_Application/Services/DataLayer/ConnectionPoolMonitor.cs - Track pool metrics, emit telemetry

### Cache Service Implementation

- [x] T102 CacheService implementation in MTM_Template_Application/Services/Cache/CacheService.cs - In-memory cache with LZ4 compression, TTL enforcement
- [x] T103 LZ4CompressionHandler in MTM_Template_Application/Services/Cache/LZ4CompressionHandler.cs - Compress/decompress cache entries
- [x] T104 VisualMasterDataSync in MTM_Template_Application/Services/Cache/VisualMasterDataSync.cs - Initial population, delta sync, background refresh
- [x] T105 CacheStalenessDetector in MTM_Template_Application/Services/Cache/CacheStalenessDetector.cs - Detect expired entries (Parts 24h, Others 7d), trigger refresh
- [x] T106 CachedOnlyModeManager in MTM_Template_Application/Services/Cache/CachedOnlyModeManager.cs - Detect Visual unavailability, enable cached-only mode, reconnection detection

### Core Services Implementation

- [x] T107 MessageBus implementation in MTM_Template_Application/Services/Core/MessageBus.cs - In-memory pub/sub, delivery guarantees, correlation IDs
- [x] T108 ValidationService implementation in MTM_Template_Application/Services/Core/ValidationService.cs - FluentValidation integration, rule discovery (attributes + conventions + config)
- [x] T109 MappingService implementation in MTM_Template_Application/Services/Core/MappingService.cs - AutoMapper integration, profile discovery
- [x] T110 HealthCheckService in MTM_Template_Application/Services/Core/HealthCheckService.cs - Aggregate health checks from all services

### Localization Service Implementation

- [x] T111 LocalizationService implementation in MTM_Template_Application/Services/Localization/LocalizationService.cs - Culture switching, resource loading, missing translation tracking
- [x] T112 MissingTranslationHandler in MTM_Template_Application/Services/Localization/MissingTranslationHandler.cs - Log missing translations, report to telemetry
- [x] T113 CultureProvider in MTM_Template_Application/Services/Localization/CultureProvider.cs - Detect OS culture, fallback chain (selected > OS > en-US)

### Theme Service Implementation

- [x] T114 ThemeService implementation in MTM_Template_Application/Services/Theme/ThemeService.cs - Theme switching (Light/Dark/Auto), OS dark mode detection
- [x] T115 OSDarkModeMonitor in MTM_Template_Application/Services/Theme/OSDarkModeMonitor.cs - Monitor OS dark mode changes, auto-switch when Theme=Auto

### Navigation Service Implementation

- [x] T116 NavigationService implementation in MTM_Template_Application/Services/Navigation/NavigationService.cs - Stack-based navigation, history tracking, deep linking
- [x] T117 UnsavedChangesGuard in MTM_Template_Application/Services/Navigation/UnsavedChangesGuard.cs - Prompt before navigation when unsaved changes exist

---

## Phase 3.6: Boot Orchestration

- [x] T118 BootOrchestrator implementation in MTM_Template_Application/Services/Boot/BootOrchestrator.cs - Execute Stage 0→1→2, progress tracking, timeout enforcement, error recovery
- [x] T119 Stage0Bootstrap implementation in MTM_Template_Application/Services/Boot/Stages/Stage0Bootstrap.cs - Splash window, watchdog (10s timeout), minimal services
- [x] T120 Stage1ServicesInitialization implementation in MTM_Template_Application/Services/Boot/Stages/Stage1ServicesInitialization.cs - Initialize all core services (60s timeout), parallel start where possible
- [x] T121 Stage2ApplicationReady implementation in MTM_Template_Application/Services/Boot/Stages/Stage2ApplicationReady.cs - Application shell, navigation, user session (15s timeout)
- [x] T122 BootProgressCalculator in MTM_Template_Application/Services/Boot/BootProgressCalculator.cs - Calculate progress percentage based on service weights
- [x] T123 BootWatchdog in MTM_Template_Application/Services/Boot/BootWatchdog.cs - Monitor stage timeouts, force failure on timeout, collect diagnostic bundle
- [x] T124 ServiceDependencyResolver in MTM_Template_Application/Services/Boot/ServiceDependencyResolver.cs - Resolve service initialization order based on dependencies
- [x] T125 ParallelServiceStarter in MTM_Template_Application/Services/Boot/ParallelServiceStarter.cs - Start independent services in parallel for performance

---

## Phase 3.7: Splash Screen UI

- [x] T126 SplashViewModel in MTM_Template_Application/ViewModels/SplashViewModel.cs - [ObservableObject], [ObservableProperty] for progress/status/stage, [RelayCommand] for cancel (MVVM Toolkit)
- [x] T127 SplashWindow.axaml in MTM_Template_Application/Views/SplashWindow.axaml - Theme-less XAML, progress bar, status text, accessibility support (no Theme V2 tokens)
- [x] T128 SplashWindow.axaml.cs code-behind in MTM_Template_Application/Views/SplashWindow.axaml.cs - Wire up ViewModel, handle window events
- [x] T129 ProgressAnimationBehavior in MTM_Template_Application/Behaviors/ProgressAnimationBehavior.cs - Smooth progress bar animation (avoid jitter)

---

## Phase 3.8: Platform Entry Points

- [x] T130 Update Program.cs in MTM_Template_Application.Desktop/Program.cs - Initialize DI container, register services, launch BootOrchestrator
- [x] T131 Update MainActivity.cs in MTM_Template_Application.Android/MainActivity.cs - Initialize DI container, register platform services, launch BootOrchestrator
- [x] T132 ServiceCollectionExtensions in MTM_Template_Application/Extensions/ServiceCollectionExtensions.cs - Extension methods for service registration (AddBootServices, AddDataLayer, AddCaching, etc.)
- [x] T133 Platform-specific service registration in MTM_Template_Application.Desktop/Services/DesktopServiceRegistration.cs - Register Windows/macOS-specific services
- [x] T134 Platform-specific service registration in MTM_Template_Application.Android/Services/AndroidServiceRegistration.cs - Register Android-specific services (KeyStore, device certificate)

---

## Phase 3.9: Error Handling and Recovery

- [x] T135 GlobalExceptionHandler in MTM_Template_Application/Services/ErrorHandling/GlobalExceptionHandler.cs - Catch unhandled exceptions, log, create diagnostic bundle
- [x] T136 ErrorCategorizer in MTM_Template_Application/Services/ErrorHandling/ErrorCategorizer.cs - Categorize errors (network, config, storage, permission)
- [x] T137 RecoveryStrategy in MTM_Template_Application/Services/ErrorHandling/RecoveryStrategy.cs - Determine recovery action based on error category
- [x] T138 DiagnosticBundleGenerator in MTM_Template_Application/Services/ErrorHandling/DiagnosticBundleGenerator.cs - Collect logs, config, system info into zip bundle

---

## Phase 3.10: Unit Tests (Service Logic)

**Test each service implementation**

- [x] T139 [P] Unit tests for ConfigurationService in tests/unit/ConfigurationServiceTests.cs - Test precedence, hot-reload, profile switching (7 tests)
- [x] T140 [P] Unit tests for WindowsSecretsService in tests/unit/WindowsSecretsServiceTests.cs - Test DPAPI encryption, storage, retrieval (mock DPAPI)
- [x] T141 [P] Unit tests for AndroidSecretsService in tests/unit/AndroidSecretsServiceTests.cs - Test KeyStore integration (mock KeyStore) (17 tests)
- [x] T142 [P] Unit tests for LoggingService in tests/unit/LoggingServiceTests.cs - Test structured logging, PII redaction, rotation
- [x] T143 [P] Unit tests for DiagnosticsService in tests/unit/DiagnosticsServiceTests.cs - Test diagnostic checks, issue detection, hardware capabilities
- [x] T144 [P] Unit tests for MySqlClient in tests/unit/MySqlClientTests.cs - Test connection pooling, query execution, metrics (mock MySqlConnection) (9 tests)
- [x] T145 [P] Unit tests for VisualApiClient in tests/unit/VisualApiClientTests.cs - Test whitelist enforcement, authentication, circuit breaker (mock HttpClient) (11 tests)
- [x] T146 [P] Unit tests for HttpApiClient in tests/unit/HttpApiClientTests.cs - Test retry policy, circuit breaker, timeout (mock HttpClient) (15 tests)
- [x] T147 [P] Unit tests for CacheService in tests/unit/CacheServiceTests.cs - Test LZ4 compression, TTL enforcement, statistics (9 tests)
- [x] T148 [P] Unit tests for VisualMasterDataSync in tests/unit/VisualMasterDataSyncTests.cs - Test initial population, delta sync, background refresh (8 tests)
- [x] T149 [P] Unit tests for CachedOnlyModeManager in tests/unit/CachedOnlyModeManagerTests.cs - Test mode detection, reconnection, feature limitations (17 tests)
- [x] T150 [P] Unit tests for MessageBus in tests/unit/MessageBusTests.cs - Test pub/sub, delivery guarantees, unsubscribe (14 tests)
- [x] T151 [P] Unit tests for ValidationService in tests/unit/ValidationServiceTests.cs - Test FluentValidation integration, rule discovery (12 tests)
- [x] T152 [P] Unit tests for MappingService in tests/unit/MappingServiceTests.cs - Test AutoMapper integration, profile discovery (11 tests)
- [x] T153 [P] Unit tests for LocalizationService in tests/unit/LocalizationServiceTests.cs - Test culture switching, missing translations, fallback (13 tests)
- [x] T154 [P] Unit tests for ThemeService in tests/unit/ThemeServiceTests.cs - Test theme switching, OS dark mode detection (15 tests)
- [x] T155 [P] Unit tests for GlobalExceptionHandler in tests/unit/GlobalExceptionHandlerTests.cs - Test exception handling, logging, diagnostic bundle generation
- [x] T156 [P] Unit tests for ErrorCategorizer in tests/unit/ErrorCategorizerTests.cs - Test error categorization, transient detection, criticality assessment
- [x] T157 [P] Unit tests for RecoveryStrategy in tests/unit/RecoveryStrategyTests.cs - Test recovery action determination and execution
- [x] T158 [P] Unit tests for DiagnosticBundleGenerator in tests/unit/DiagnosticBundleGeneratorTests.cs - Test bundle generation, compression, decompression

- [x] T159 [P] Unit tests for NavigationService in tests/unit/NavigationServiceTests.cs - Test navigation stack, history, deep linking, unsaved changes guard (16 tests)
- [x] T160 [P] Unit tests for BootOrchestrator in tests/unit/BootOrchestratorTests.cs - Test stage execution, progress calculation, timeout enforcement, error recovery (9 tests)
- [x] T161 [P] Unit tests for ExponentialBackoffPolicy in tests/unit/ExponentialBackoffPolicyTests.cs - Test retry delays, jitter, max retries (5 tests)
- [x] T162 [P] Unit tests for CircuitBreakerPolicy in tests/unit/CircuitBreakerPolicyTests.cs - Test failure threshold, recovery delay, state transitions (8 tests)

---

## Phase 3.11: Polish and Validation

- [x] T163 Performance test: Boot time <10s in tests/integration/PerformanceTests.cs - Measure actual boot time, verify target met
- [x] T164 Performance test: Stage 1 <3s in tests/integration/PerformanceTests.cs - Measure Stage 1 duration, verify target met
- [x] T165 Performance test: Memory <100MB in tests/integration/PerformanceTests.cs - Measure memory usage during boot, verify target met. Include memory profiling subtasks: (a) measure peak memory at end of each stage, (b) validate allocation breakdown (cache ~40MB compressed, core services ~30MB, framework overhead ~30MB), (c) identify top 10 memory consumers, (d) verify no memory leaks during boot sequence, (e) export memory profile for documentation
- [ ] T166 **[MANUAL VALIDATION]** Accessibility audit: Screen reader - Verify splash screen announcements and progress updates with Windows Narrator enabled. Requires human tester to validate audio announcements for stage transitions and milestones. See IMPLEMENTATION_STATUS.md for detailed test steps.
- [ ] T167 **[MANUAL VALIDATION]** Accessibility audit: Keyboard navigation - Verify splash screen keyboard accessible (Tab, Enter, Escape keys). Requires human tester to validate focus indicators and keyboard-only operation. See IMPLEMENTATION_STATUS.md for detailed test steps.

- [ ] T168 **[MANUAL VALIDATION]** Accessibility audit: High contrast - Verify splash screen visible in Windows High Contrast mode. Requires human tester to validate text readability and sufficient color contrast. See IMPLEMENTATION_STATUS.md for detailed test steps.

- [x] T169 Code review: Remove duplication in codebase - Identify and refactor duplicate code
- [x] T170 Code review: Null safety audit in codebase - Verify ArgumentNullException.ThrowIfNull usage, nullable annotations
- [x] T171 Documentation: Update docs/BOOT-SEQUENCE.md - Document final boot sequence implementation
- [x] T172 Documentation: Update .github/copilot-instructions.md - Add boot sequence patterns for future features
- [ ] T173 **[MANUAL VALIDATION]** Execute manual testing: Run all 9 quickstart.md scenarios - Validate all scenarios manually (requires Visual ERP test server access or mocks). Scenarios include: boot sequence, configuration precedence, credential storage, diagnostics, caching, logging, error handling, localization, and performance. See quickstart.md for detailed validation checklists.
- [x] T174 [P] Update README.md with boot sequence overview
- [x] T175 [P] Create TROUBLESHOOTING.md for boot sequence issues

---

## Dependencies

### Critical Path (Sequential)

1. **Setup (T001-T015)** blocks all other tasks

2. **Entity Models (T016-T038)** blocks tests and services that reference them
3. **Service Interfaces (T039-T054)** blocks service implementations and tests
4. **Integration Tests (T055-T071)** MUST be written before corresponding implementations
5. **Contract Tests (T072-T078)** MUST be written before API client implementations
6. **Configuration (T080-T082)** blocks services that depend on configuration
7. **Secrets (T083-T086)** blocks services that need credentials
8. **Logging (T087-T090)** blocks all services (all services log)
9. **Data Layer (T096-T101)** blocks cache and Visual integration

10. **Cache (T102-T106)** blocks cached-only mode
11. **Boot Orchestration (T118-T125)** blocks platform entry points
12. **Splash UI (T126-T129)** blocks platform entry points
13. **Platform Entry Points (T130-T134)** final integration
14. **Polish (T163-T175)** after implementation complete

### Parallelization Opportunities

- Entity models (T016-T038): All independent, can run in parallel
- Service interfaces (T039-T054): All independent, can run in parallel
- Integration tests (T055-T071): Independent scenarios, can run in parallel
- Contract tests (T072-T078): Independent APIs, can run in parallel
- Diagnostic checks (T090-T093): Independent checks, can run in parallel
- Unit tests (T139-T162): Independent services, can run in parallel

### Dependency Graph (Key Relationships)

```
T001-T015 (Setup)
    ↓
T016-T038 (Models) [P]
    ↓

T039-T054 (Interfaces) [P]
    ↓
T055-T078 (Tests) [P]
    ↓
T079-T081 (Configuration)
    ↓

T083-T086 (Secrets) + T087-T090 (Logging) [P]
    ↓
T091-T095 (Diagnostics) [P] + T096-T101 (DataLayer) + T107-T110 (Core) [P]
    ↓
T102-T106 (Cache) + T111-T117 (App Services) [P]


    ↓
T118-T125 (Boot Orchestration)
    ↓
T126-T129 (Splash UI) [P]
    ↓
T130-T134 (Platform Entry) + T135-T138 (Error Handling) [P]

    ↓
T139-T162 (Unit Tests) [P]
    ↓
T163-T175 (Polish)
```

---

## Parallel Execution Examples

### Phase 1: Entity Models (All Parallel)

```
Launch T016-T038 together (23 model files, all independent)
Example: "Create BootMetrics model in MTM_Template_Application/Models/Boot/BootMetrics.cs"
```

### Phase 2: Service Interfaces (All Parallel)

```
Launch T039-T054 together (16 interface files, all independent)
Example: "Create IBootOrchestrator interface in MTM_Template_Application/Services/Boot/IBootOrchestrator.cs"
```

### Phase 3: Integration Tests (All Parallel)

```
Launch T055-T071 together (17 test scenarios, all independent)
Example: "Integration test: Normal boot sequence in tests/integration/BootSequenceTests.cs"
```

### Phase 4: Unit Tests (All Parallel)

```
Launch T139-T162 together (24 test suites, all independent)
Example: "Unit tests for ConfigurationService in tests/unit/ConfigurationServiceTests.cs"
```

---

## Validation Checklist

- [x] All contracts have corresponding tests (T072-T079 for Visual/HTTP/MySQL/Diagnostics)
- [x] All entities have model tasks (T016-T038 cover 23 entities from data-model.md)
- [x] All tests come before implementation (T055-T079 before T080-T138)
- [x] Parallel tasks truly independent (all [P] tasks target different files)
- [x] Each task specifies exact file path (all tasks include full path)
- [x] No task modifies same file as another [P] task (verified - all [P] tasks are different files)
- [x] All quickstart scenarios have integration tests (T055-T071 cover all 9 scenarios)
- [x] TDD ordering enforced (Setup → Models → Interfaces → Tests → Implementation)
- [x] Constitutional compliance (MVVM Toolkit, null safety, TDD, Theme V2)

---

## Notes

- **[P] tasks** = different files, no dependencies, safe for parallel execution
- **Verify tests fail** before implementing (TDD red-green-refactor cycle)
- **Commit after each task** for atomic changes and easy rollback
- **Performance targets**: Stage 1 <3s, Total boot <10s, Memory <100MB
- **Offline capability**: Cached-only mode when Visual ERP unavailable
- **Platform abstraction**: Services have Desktop/Android variants (Secrets, Entry Points)
- **Constitutional patterns**: MVVM Toolkit (NO ReactiveUI), nullable types, ArgumentNullException.ThrowIfNull()
- **Error handling**: Circuit breakers (5 failures), exponential backoff (1s→16s), diagnostic bundles
- **Caching**: LZ4 compression, TTLs (Parts 24h, Others 7d), delta sync, staleness detection
- **Logging**: OpenTelemetry format, structured JSON, PII redaction, 10MB/7 days rotation
- **Deferred requirements**:
  - **FR-132 (Admin Monitoring Dashboard)**: DEFERRED to post-MVP feature branch `002-admin-dashboard`. Initial release uses log file review for service monitoring. Includes tasks for: service status UI, real-time metrics dashboard, health check aggregation panel, alerting configuration.
  - **FR-134 (API Documentation Generation)**: DEFERRED to polish phase (Phase 4+). Current approach: XML documentation comments in code + optional DocFX generation. Will be addressed when team size >5 OR external contributors join. Includes tasks for: DocFX configuration, CI/CD integration, GitHub Pages deployment.

---

**Total Tasks**: 175 (Setup: 15, Models: 23, Interfaces: 16, Tests: 25, Implementation: 59, UI: 4, Platform: 5, Error Handling: 4, Unit Tests: 24, Polish: 13)

**Estimated Duration**:

- Setup: ~2 days
- Models + Interfaces: ~3 days (parallel)
- Tests: ~4 days (parallel)
- Core Implementation: ~12 days (sequential with some parallelization)
- Boot Orchestration: ~3 days
- UI + Platform: ~2 days
- Polish: ~2 days
- **Total: ~28 days** (with parallelization opportunities reducing actual calendar time)

**Ready for execution**: Run tasks in order, respecting dependencies. Mark [P] tasks for parallel execution to optimize throughput.
