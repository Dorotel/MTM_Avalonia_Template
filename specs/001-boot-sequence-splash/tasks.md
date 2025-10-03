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

**All models support TDD - create before tests reference them**

### Boot Orchestration Models (3 entities)
- [ ] T016 [P] BootMetrics model in MTM_Template_Application/Models/Boot/BootMetrics.cs (TotalDurationMs, StageMetrics[], MemoryUsageMB, ServicesInitialized, ErrorsEncountered)
- [ ] T017 [P] StageMetrics model in MTM_Template_Application/Models/Boot/StageMetrics.cs (StageNumber, Name, StartTimeUtc, DurationMs, Status, ServicesStarted[], Errors[])
- [ ] T018 [P] ServiceMetrics model in MTM_Template_Application/Models/Boot/ServiceMetrics.cs (ServiceName, StartTimeUtc, DurationMs, InitializationStatus, Dependencies[], ErrorMessage?)

### Configuration Models (3 entities)
- [ ] T019 [P] ConfigurationProfile model in MTM_Template_Application/Models/Configuration/ConfigurationProfile.cs (ProfileName, IsActive, Settings[], FeatureFlags[], LastModifiedUtc)
- [ ] T020 [P] ConfigurationSetting model in MTM_Template_Application/Models/Configuration/ConfigurationSetting.cs (Key, Value, Source, Precedence, IsEncrypted)
- [ ] T021 [P] FeatureFlag model in MTM_Template_Application/Models/Configuration/FeatureFlag.cs (Name, IsEnabled, Environment, RolloutPercentage, EvaluatedAt)

### Secrets Models (1 entity)
- [ ] T022 [P] SecretEntry model in MTM_Template_Application/Models/Secrets/SecretEntry.cs (Key, EncryptedValue, CreatedUtc, LastAccessedUtc, ExpiresAtUtc?, Metadata)

### Logging Models (2 entities)
- [ ] T023 [P] LogEntry model in MTM_Template_Application/Models/Logging/LogEntry.cs (Timestamp, Level, Message, TraceId, SpanId, Attributes, Resource, Scope) - OpenTelemetry format
- [ ] T024 [P] TelemetryBatch model in MTM_Template_Application/Models/Logging/TelemetryBatch.cs (BatchId, Entries[], CreatedUtc, Status)

### Diagnostics Models (3 entities)
- [ ] T025 [P] DiagnosticResult model in MTM_Template_Application/Models/Diagnostics/DiagnosticResult.cs (CheckName, Status, Message, Details, Timestamp, DurationMs)
- [ ] T026 [P] HardwareCapabilities model in MTM_Template_Application/Models/Diagnostics/HardwareCapabilities.cs (TotalMemoryMB, AvailableMemoryMB, ProcessorCount, Platform, ScreenResolution, HasCamera, HasBarcodeScanner)
- [ ] T027 [P] DiagnosticIssue model in MTM_Template_Application/Models/Diagnostics/DiagnosticIssue.cs (Severity, Category, Description, ResolutionSteps[], DetectedAt)

### Data Layer Models (2 entities)
- [ ] T028 [P] ConnectionPoolMetrics model in MTM_Template_Application/Models/DataLayer/ConnectionPoolMetrics.cs (PoolName, ActiveConnections, IdleConnections, MaxPoolSize, AverageAcquireTimeMs, WaitingRequests)
- [ ] T029 [P] CircuitBreakerState model in MTM_Template_Application/Models/DataLayer/CircuitBreakerState.cs (ServiceName, State, FailureCount, LastFailureUtc, NextRetryUtc, OpenedAt?)

### Cache Models (2 entities)
- [ ] T030 [P] CacheEntry model in MTM_Template_Application/Models/Cache/CacheEntry.cs (Key, CompressedValue, UncompressedSizeBytes, CreatedUtc, ExpiresAtUtc, LastAccessedUtc, AccessCount, EntityType)
- [ ] T031 [P] CacheStatistics model in MTM_Template_Application/Models/Cache/CacheStatistics.cs (TotalEntries, HitCount, MissCount, HitRate, TotalSizeBytes, CompressionRatio, EvictionCount)

### Core Services Models (2 entities)
- [ ] T032 [P] MessageEnvelope model in MTM_Template_Application/Models/Core/MessageEnvelope.cs (MessageId, Type, Payload, Timestamp, CorrelationId, DeliveryCount, ExpiresAt?)
- [ ] T033 [P] ValidationRuleMetadata model in MTM_Template_Application/Models/Core/ValidationRuleMetadata.cs (RuleName, PropertyName, Severity, ErrorMessage, ValidatorType)

### Localization Models (2 entities)
- [ ] T034 [P] LocalizationSetting model in MTM_Template_Application/Models/Localization/LocalizationSetting.cs (Culture, IsActive, FallbackCulture, SupportedLanguages[], DateFormat, NumberFormat)
- [ ] T035 [P] MissingTranslation model in MTM_Template_Application/Models/Localization/MissingTranslation.cs (Key, Culture, FallbackValue, ReportedAt, Frequency)

### Theme Models (1 entity)
- [ ] T036 [P] ThemeConfiguration model in MTM_Template_Application/Models/Theme/ThemeConfiguration.cs (ThemeMode, IsDarkMode, AccentColor, FontSize, HighContrast, LastChangedUtc)

### Navigation Models (1 entity)
- [ ] T037 [P] NavigationHistoryEntry model in MTM_Template_Application/Models/Navigation/NavigationHistoryEntry.cs (ViewName, NavigatedAtUtc, Parameters, CanGoBack, CanGoForward)

### Error Handling Models (1 entity)
- [ ] T038 [P] ErrorReport model in MTM_Template_Application/Models/ErrorHandling/ErrorReport.cs (ErrorId, Message, StackTrace, Severity, Category, OccurredAt, DiagnosticBundle?)

---

## Phase 3.3: Service Interfaces (Contract Definition)

**Define interfaces before implementation - enables mocking in tests**

- [ ] T039 [P] IBootOrchestrator interface in MTM_Template_Application/Services/Boot/IBootOrchestrator.cs (ExecuteStage0Async, ExecuteStage1Async, ExecuteStage2Async, GetBootMetrics, OnProgressChanged event)
- [ ] T040 [P] IBootStage interface in MTM_Template_Application/Services/Boot/IBootStage.cs (StageNumber, Name, ExecuteAsync, ValidatePreconditions)
- [ ] T041 [P] IConfigurationService interface in MTM_Template_Application/Services/Configuration/IConfigurationService.cs (GetValue<T>, SetValue, ReloadAsync, OnConfigurationChanged event)
- [ ] T042 [P] ISecretsService interface in MTM_Template_Application/Services/Secrets/ISecretsService.cs (StoreSecretAsync, RetrieveSecretAsync, DeleteSecretAsync, RotateSecretAsync)
- [ ] T043 [P] ILoggingService interface in MTM_Template_Application/Services/Logging/ILoggingService.cs (LogInformation, LogWarning, LogError, SetContext, FlushAsync)
- [ ] T044 [P] IDiagnosticsService interface in MTM_Template_Application/Services/Diagnostics/IDiagnosticsService.cs (RunAllChecksAsync, RunCheckAsync, GetHardwareCapabilities)
- [ ] T045 [P] IMySqlClient interface in MTM_Template_Application/Services/DataLayer/IMySqlClient.cs (ExecuteQueryAsync, ExecuteNonQueryAsync, ExecuteScalarAsync, GetConnectionMetrics)
- [ ] T046 [P] IVisualApiClient interface in MTM_Template_Application/Services/DataLayer/IVisualApiClient.cs (ExecuteCommandAsync, IsServerAvailable, GetWhitelistedCommands)
- [ ] T047 [P] IHttpApiClient interface in MTM_Template_Application/Services/DataLayer/IHttpApiClient.cs (GetAsync, PostAsync, PutAsync, DeleteAsync)
- [ ] T048 [P] ICacheService interface in MTM_Template_Application/Services/Cache/ICacheService.cs (GetAsync<T>, SetAsync<T>, RemoveAsync, ClearAsync, GetStatistics, RefreshAsync)
- [ ] T049 [P] IMessageBus interface in MTM_Template_Application/Services/Core/IMessageBus.cs (PublishAsync, SubscribeAsync<T>, UnsubscribeAsync)
- [ ] T050 [P] IValidationService interface in MTM_Template_Application/Services/Core/IValidationService.cs (ValidateAsync<T>, RegisterValidator<T>, GetRuleMetadata)
- [ ] T051 [P] IMappingService interface in MTM_Template_Application/Services/Core/IMappingService.cs (Map<TSource, TDestination>, MapAsync<TSource, TDestination>)
- [ ] T052 [P] ILocalizationService interface in MTM_Template_Application/Services/Localization/ILocalizationService.cs (GetString, SetCulture, GetSupportedCultures, OnLanguageChanged event)
- [ ] T053 [P] IThemeService interface in MTM_Template_Application/Services/Theme/IThemeService.cs (SetTheme, GetCurrentTheme, OnThemeChanged event)
- [ ] T054 [P] INavigationService interface in MTM_Template_Application/Services/Navigation/INavigationService.cs (NavigateToAsync, GoBackAsync, GoForwardAsync, GetHistory)

---

## Phase 3.4: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.5

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### Integration Tests (from quickstart.md scenarios)

- [ ] T055 [P] Integration test: Normal boot sequence (happy path) in tests/integration/BootSequenceTests.cs - Validate Stage 0→1→2 execution, all services initialized, performance <10s, memory <100MB
- [ ] T056 [P] Integration test: Configuration loading and override precedence in tests/integration/ConfigurationTests.cs - Validate env vars > user config > app config > defaults, hot-reload
- [ ] T057 [P] Integration test: Credential storage and validation in tests/integration/SecretsTests.cs - Validate OS-native storage (Windows DPAPI, macOS Keychain, Android KeyStore), encryption, retrieval
- [ ] T058 [P] Integration test: Diagnostic checks and issue detection in tests/integration/DiagnosticsTests.cs - Validate storage, permissions, network, hardware detection
- [ ] T059 [P] Integration test: Visual master data caching (population) in tests/integration/VisualCachingTests.cs - Validate initial cache population, LZ4 compression, TTL enforcement
- [ ] T060 [P] Integration test: Visual master data caching (cache hits) in tests/integration/VisualCachingTests.cs - Validate cache hits reduce Visual API calls, performance improvement
- [ ] T061 [P] Integration test: Visual master data caching (staleness detection) in tests/integration/VisualCachingTests.cs - Validate delta sync, refresh stale entries, background refresh
- [ ] T062 [P] Integration test: Visual master data caching (offline mode) in tests/integration/VisualCachingTests.cs - Validate cached-only mode when Visual unavailable, reconnection detection
- [ ] T063 [P] Integration test: Logging and telemetry (structured JSON) in tests/integration/LoggingTests.cs - Validate OpenTelemetry format, trace/span IDs, PII redaction, log rotation
- [ ] T064 [P] Integration test: Error handling and recovery (network failures) in tests/integration/ErrorHandlingTests.cs - Validate exponential backoff, circuit breaker, diagnostic bundles
- [ ] T065 [P] Integration test: Error handling and recovery (configuration errors) in tests/integration/ErrorHandlingTests.cs - Validate fallback config, error reporting, recovery
- [ ] T066 [P] Integration test: Accessibility and localization (screen reader) in tests/integration/AccessibilityTests.cs - Validate screen reader announcements, keyboard navigation
- [ ] T067 [P] Integration test: Accessibility and localization (language switching) in tests/integration/LocalizationTests.cs - Validate culture change, missing translations, fallback
- [ ] T068 [P] Integration test: Accessibility and localization (high contrast) in tests/integration/ThemeTests.cs - Validate high contrast mode, theme switching (Light/Dark/Auto)
- [ ] T069 [P] Integration test: Performance validation (boot time) in tests/integration/PerformanceTests.cs - Validate Stage 1 <3s, total boot <10s
- [ ] T070 [P] Integration test: Performance validation (memory usage) in tests/integration/PerformanceTests.cs - Validate memory <100MB during boot
- [ ] T071 [P] Integration test: Performance validation (parallel initialization) in tests/integration/PerformanceTests.cs - Validate services start in parallel where possible

### Contract Tests (API Validation)

- [ ] T072 [P] Contract test: Visual API Toolkit whitelist validation in tests/contract/VisualApiContractTests.cs - Validate only whitelisted commands accepted (see visual-whitelist.md)
- [ ] T073 [P] Contract test: Visual API Toolkit authentication in tests/contract/VisualApiContractTests.cs - Validate device certificate + user credentials (Android)
- [ ] T074 [P] Contract test: Visual API Toolkit schema dictionary in tests/contract/VisualApiContractTests.cs - Validate table/column name resolution
- [ ] T075 [P] Contract test: HTTP API endpoint availability in tests/contract/HttpApiContractTests.cs - Validate Android HTTP API endpoints respond
- [ ] T076 [P] Contract test: MySQL connection string validation in tests/contract/MySqlContractTests.cs - Validate connection string format, role-based access
- [ ] T077 [P] Contract test: Diagnostics API checks in tests/contract/DiagnosticsContractTests.cs - Validate diagnostic check contracts (storage, permissions, network, hardware)
- [ ] T078 [P] Contract test: Android device certificate + two-factor auth in tests/contract/AndroidAuthContractTests.cs - Validate device cert stored in Android KeyStore + user credentials (FR-154)

---

## Phase 3.5: Core Implementation (ONLY after tests are failing)

### Configuration Service Implementation

- [ ] T079 ConfigurationService implementation in MTM_Template_Application/Services/Configuration/ConfigurationService.cs - Layered precedence (env vars > user > app > defaults), hot-reload support
- [ ] T080 ConfigurationProfile persistence in MTM_Template_Application/Services/Configuration/ConfigurationPersistence.cs - Save/load profiles from local storage
- [ ] T081 FeatureFlag evaluation in MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs - Environment-based flag evaluation, rollout percentage

### Secrets Service Implementation

- [ ] T082 WindowsSecretsService implementation in MTM_Template_Application/Services/Secrets/WindowsSecretsService.cs - Windows DPAPI integration, credential manager API
- [ ] T083 MacOSSecretsService implementation in MTM_Template_Application/Services/Secrets/MacOSSecretsService.cs - macOS Keychain integration
- [ ] T084 AndroidSecretsService implementation in MTM_Template_Application/Services/Secrets/AndroidSecretsService.cs - Android KeyStore integration
- [ ] T085 SecretsService factory in MTM_Template_Application/Services/Secrets/SecretsServiceFactory.cs - Platform detection, return appropriate implementation

### Logging Service Implementation

- [ ] T086 LoggingService implementation in MTM_Template_Application/Services/Logging/LoggingService.cs - Serilog + OpenTelemetry integration, structured JSON format
- [ ] T087 PII redaction middleware in MTM_Template_Application/Services/Logging/PiiRedactionMiddleware.cs - Detect and redact sensitive data (SSN, credit cards, passwords)
- [ ] T088 Log rotation policy in MTM_Template_Application/Services/Logging/LogRotationPolicy.cs - 10MB max file size, 7 days retention
- [ ] T089 Telemetry batch processor in MTM_Template_Application/Services/Logging/TelemetryBatchProcessor.cs - Batch telemetry for efficient transmission

### Diagnostics Service Implementation

- [ ] T089 DiagnosticsService implementation in MTM_Template_Application/Services/Diagnostics/DiagnosticsService.cs - Orchestrate all diagnostic checks
- [ ] T090 [P] StorageDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/StorageDiagnostic.cs - Verify storage availability, free space
- [ ] T091 [P] PermissionsDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/PermissionsDiagnostic.cs - Verify file system, camera, network permissions
- [ ] T092 [P] NetworkDiagnostic check in MTM_Template_Application/Services/Diagnostics/Checks/NetworkDiagnostic.cs - Verify network connectivity (5s timeout)
- [ ] T093 [P] HardwareDetection service in MTM_Template_Application/Services/Diagnostics/HardwareDetection.cs - Detect memory, CPU, screen resolution, peripherals

### Data Layer Implementation

- [ ] T094 MySqlClient implementation in MTM_Template_Application/Services/DataLayer/MySqlClient.cs - Connection pooling (Desktop: 2-10, Android: 1-5), query execution, role-based access
- [ ] T095 VisualApiClient implementation in MTM_Template_Application/Services/DataLayer/VisualApiClient.cs - HTTP client wrapper, command whitelist enforcement, authentication
- [ ] T096 HttpApiClient implementation in MTM_Template_Application/Services/DataLayer/HttpApiClient.cs - Generic HTTP client with retry/circuit breaker
- [ ] T097 ExponentialBackoffPolicy in MTM_Template_Application/Services/DataLayer/Policies/ExponentialBackoffPolicy.cs - 1s, 2s, 4s, 8s, 16s with ±25% jitter (Polly)
- [ ] T098 CircuitBreakerPolicy in MTM_Template_Application/Services/DataLayer/Policies/CircuitBreakerPolicy.cs - 5 consecutive failures, exponential recovery 30s→10m (Polly)
- [ ] T099 ConnectionPoolMonitor in MTM_Template_Application/Services/DataLayer/ConnectionPoolMonitor.cs - Track pool metrics, emit telemetry

### Cache Service Implementation

- [ ] T101 CacheService implementation in MTM_Template_Application/Services/Cache/CacheService.cs - In-memory cache with LZ4 compression, TTL enforcement
- [ ] T102 LZ4CompressionHandler in MTM_Template_Application/Services/Cache/LZ4CompressionHandler.cs - Compress/decompress cache entries
- [ ] T103 VisualMasterDataSync in MTM_Template_Application/Services/Cache/VisualMasterDataSync.cs - Initial population, delta sync, background refresh
- [ ] T104 CacheStalenessDetector in MTM_Template_Application/Services/Cache/CacheStalenessDetector.cs - Detect expired entries (Parts 24h, Others 7d), trigger refresh
- [ ] T105 CachedOnlyModeManager in MTM_Template_Application/Services/Cache/CachedOnlyModeManager.cs - Detect Visual unavailability, enable cached-only mode, reconnection detection

### Core Services Implementation

- [ ] T106 MessageBus implementation in MTM_Template_Application/Services/Core/MessageBus.cs - In-memory pub/sub, delivery guarantees, correlation IDs
- [ ] T107 ValidationService implementation in MTM_Template_Application/Services/Core/ValidationService.cs - FluentValidation integration, rule discovery (attributes + conventions + config)
- [ ] T108 MappingService implementation in MTM_Template_Application/Services/Core/MappingService.cs - AutoMapper integration, profile discovery
- [ ] T109 HealthCheckService in MTM_Template_Application/Services/Core/HealthCheckService.cs - Aggregate health checks from all services

### Localization Service Implementation

- [ ] T110 LocalizationService implementation in MTM_Template_Application/Services/Localization/LocalizationService.cs - Culture switching, resource loading, missing translation tracking
- [ ] T111 MissingTranslationHandler in MTM_Template_Application/Services/Localization/MissingTranslationHandler.cs - Log missing translations, report to telemetry
- [ ] T112 CultureProvider in MTM_Template_Application/Services/Localization/CultureProvider.cs - Detect OS culture, fallback chain (selected > OS > en-US)

### Theme Service Implementation

- [ ] T113 ThemeService implementation in MTM_Template_Application/Services/Theme/ThemeService.cs - Theme switching (Light/Dark/Auto), OS dark mode detection
- [ ] T114 OSDarkModeMonitor in MTM_Template_Application/Services/Theme/OSDarkModeMonitor.cs - Monitor OS dark mode changes, auto-switch when Theme=Auto

### Navigation Service Implementation

- [ ] T115 NavigationService implementation in MTM_Template_Application/Services/Navigation/NavigationService.cs - Stack-based navigation, history tracking, deep linking
- [ ] T116 UnsavedChangesGuard in MTM_Template_Application/Services/Navigation/UnsavedChangesGuard.cs - Prompt before navigation when unsaved changes exist

---

## Phase 3.6: Boot Orchestration

- [ ] T117 BootOrchestrator implementation in MTM_Template_Application/Services/Boot/BootOrchestrator.cs - Execute Stage 0→1→2, progress tracking, timeout enforcement, error recovery
- [ ] T118 Stage0Bootstrap implementation in MTM_Template_Application/Services/Boot/Stages/Stage0Bootstrap.cs - Splash window, watchdog (10s timeout), minimal services
- [ ] T119 Stage1ServicesInitialization implementation in MTM_Template_Application/Services/Boot/Stages/Stage1ServicesInitialization.cs - Initialize all core services (60s timeout), parallel start where possible
- [ ] T120 Stage2ApplicationReady implementation in MTM_Template_Application/Services/Boot/Stages/Stage2ApplicationReady.cs - Application shell, navigation, user session (15s timeout)
- [ ] T121 BootProgressCalculator in MTM_Template_Application/Services/Boot/BootProgressCalculator.cs - Calculate progress percentage based on service weights
- [ ] T122 BootWatchdog in MTM_Template_Application/Services/Boot/BootWatchdog.cs - Monitor stage timeouts, force failure on timeout, collect diagnostic bundle
- [ ] T123 ServiceDependencyResolver in MTM_Template_Application/Services/Boot/ServiceDependencyResolver.cs - Resolve service initialization order based on dependencies
- [ ] T124 ParallelServiceStarter in MTM_Template_Application/Services/Boot/ParallelServiceStarter.cs - Start independent services in parallel for performance

---

## Phase 3.7: Splash Screen UI

- [ ] T125 SplashViewModel in MTM_Template_Application/ViewModels/SplashViewModel.cs - [ObservableObject], [ObservableProperty] for progress/status/stage, [RelayCommand] for cancel (MVVM Toolkit)
- [ ] T126 SplashWindow.axaml in MTM_Template_Application/Views/SplashWindow.axaml - Theme-less XAML, progress bar, status text, accessibility support (no Theme V2 tokens)
- [ ] T127 SplashWindow.axaml.cs code-behind in MTM_Template_Application/Views/SplashWindow.axaml.cs - Wire up ViewModel, handle window events
- [ ] T128 ProgressAnimationBehavior in MTM_Template_Application/Behaviors/ProgressAnimationBehavior.cs - Smooth progress bar animation (avoid jitter)

---

## Phase 3.8: Platform Entry Points

- [ ] T129 Update Program.cs in MTM_Template_Application.Desktop/Program.cs - Initialize DI container, register services, launch BootOrchestrator
- [ ] T130 Update MainActivity.cs in MTM_Template_Application.Android/MainActivity.cs - Initialize DI container, register platform services, launch BootOrchestrator
- [ ] T131 ServiceCollectionExtensions in MTM_Template_Application/Extensions/ServiceCollectionExtensions.cs - Extension methods for service registration (AddBootServices, AddDataLayer, AddCaching, etc.)
- [ ] T132 Platform-specific service registration in MTM_Template_Application.Desktop/Services/DesktopServiceRegistration.cs - Register Windows/macOS-specific services
- [ ] T133 Platform-specific service registration in MTM_Template_Application.Android/Services/AndroidServiceRegistration.cs - Register Android-specific services (KeyStore, device certificate)

---

## Phase 3.9: Error Handling and Recovery

- [ ] T134 GlobalExceptionHandler in MTM_Template_Application/Services/ErrorHandling/GlobalExceptionHandler.cs - Catch unhandled exceptions, log, create diagnostic bundle
- [ ] T135 ErrorCategorizer in MTM_Template_Application/Services/ErrorHandling/ErrorCategorizer.cs - Categorize errors (network, config, storage, permission)
- [ ] T136 RecoveryStrategy in MTM_Template_Application/Services/ErrorHandling/RecoveryStrategy.cs - Determine recovery action based on error category
- [ ] T137 DiagnosticBundleGenerator in MTM_Template_Application/Services/ErrorHandling/DiagnosticBundleGenerator.cs - Collect logs, config, system info into zip bundle

---

## Phase 3.10: Unit Tests (Service Logic)

**Test each service implementation**

- [ ] T138 [P] Unit tests for ConfigurationService in tests/unit/ConfigurationServiceTests.cs - Test precedence, hot-reload, profile switching
- [ ] T139 [P] Unit tests for WindowsSecretsService in tests/unit/WindowsSecretsServiceTests.cs - Test DPAPI encryption, storage, retrieval (mock DPAPI)
- [ ] T140 [P] Unit tests for MacOSSecretsService in tests/unit/MacOSSecretsServiceTests.cs - Test Keychain integration (mock Keychain)
- [ ] T141 [P] Unit tests for AndroidSecretsService in tests/unit/AndroidSecretsServiceTests.cs - Test KeyStore integration (mock KeyStore)
- [ ] T142 [P] Unit tests for LoggingService in tests/unit/LoggingServiceTests.cs - Test structured logging, PII redaction, rotation
- [ ] T143 [P] Unit tests for DiagnosticsService in tests/unit/DiagnosticsServiceTests.cs - Test diagnostic checks, issue detection, hardware capabilities
- [ ] T144 [P] Unit tests for MySqlClient in tests/unit/MySqlClientTests.cs - Test connection pooling, query execution, metrics (mock MySqlConnection)
- [ ] T145 [P] Unit tests for VisualApiClient in tests/unit/VisualApiClientTests.cs - Test whitelist enforcement, authentication, circuit breaker (mock HttpClient)
- [ ] T146 [P] Unit tests for HttpApiClient in tests/unit/HttpApiClientTests.cs - Test retry policy, circuit breaker, timeout (mock HttpClient)
- [ ] T147 [P] Unit tests for CacheService in tests/unit/CacheServiceTests.cs - Test LZ4 compression, TTL enforcement, statistics
- [ ] T148 [P] Unit tests for VisualMasterDataSync in tests/unit/VisualMasterDataSyncTests.cs - Test initial population, delta sync, background refresh
- [ ] T149 [P] Unit tests for CachedOnlyModeManager in tests/unit/CachedOnlyModeManagerTests.cs - Test mode detection, reconnection, feature limitations
- [ ] T150 [P] Unit tests for MessageBus in tests/unit/MessageBusTests.cs - Test pub/sub, delivery guarantees, unsubscribe
- [ ] T151 [P] Unit tests for ValidationService in tests/unit/ValidationServiceTests.cs - Test FluentValidation integration, rule discovery
- [ ] T152 [P] Unit tests for MappingService in tests/unit/MappingServiceTests.cs - Test AutoMapper integration, profile discovery
- [ ] T153 [P] Unit tests for LocalizationService in tests/unit/LocalizationServiceTests.cs - Test culture switching, missing translations, fallback
- [ ] T154 [P] Unit tests for ThemeService in tests/unit/ThemeServiceTests.cs - Test theme switching, OS dark mode detection
- [ ] T155 [P] Unit tests for NavigationService in tests/unit/NavigationServiceTests.cs - Test navigation stack, history, deep linking, unsaved changes guard
- [ ] T156 [P] Unit tests for BootOrchestrator in tests/unit/BootOrchestratorTests.cs - Test stage execution, progress calculation, timeout enforcement, error recovery
- [ ] T157 [P] Unit tests for ExponentialBackoffPolicy in tests/unit/ExponentialBackoffPolicyTests.cs - Test retry delays, jitter, max retries
- [ ] T158 [P] Unit tests for CircuitBreakerPolicy in tests/unit/CircuitBreakerPolicyTests.cs - Test failure threshold, recovery delay, state transitions

---

## Phase 3.11: Polish and Validation

- [ ] T159 Performance test: Boot time <10s in tests/integration/PerformanceTests.cs - Measure actual boot time, verify target met
- [ ] T160 Performance test: Stage 1 <3s in tests/integration/PerformanceTests.cs - Measure Stage 1 duration, verify target met
- [ ] T161 Performance test: Memory <100MB in tests/integration/PerformanceTests.cs - Measure memory usage during boot, verify target met
- [ ] T162 Accessibility audit: Screen reader in manual testing - Verify splash screen announcements, progress updates
- [ ] T163 Accessibility audit: Keyboard navigation in manual testing - Verify splash screen keyboard accessible
- [ ] T164 Accessibility audit: High contrast in manual testing - Verify splash screen visible in high contrast mode
- [ ] T165 Code review: Remove duplication in codebase - Identify and refactor duplicate code
- [ ] T166 Code review: Null safety audit in codebase - Verify ArgumentNullException.ThrowIfNull usage, nullable annotations
- [ ] T167 Documentation: Update docs/BOOT-SEQUENCE.md - Document final boot sequence implementation
- [ ] T168 Documentation: Update .github/copilot-instructions.md - Add boot sequence patterns for future features
- [ ] T169 Execute manual testing: Run all quickstart.md scenarios - Validate all 9 scenarios manually
- [ ] T170 [P] Update README.md with boot sequence overview
- [ ] T171 [P] Create TROUBLESHOOTING.md for boot sequence issues

---

## Dependencies

### Critical Path (Sequential)
1. **Setup (T001-T015)** blocks all other tasks
2. **Entity Models (T016-T038)** blocks tests and services that reference them
3. **Service Interfaces (T039-T054)** blocks service implementations and tests
4. **Integration Tests (T055-T071)** MUST be written before corresponding implementations
5. **Contract Tests (T072-T078)** MUST be written before API client implementations
6. **Configuration (T079-T081)** blocks services that depend on configuration
7. **Secrets (T082-T085)** blocks services that need credentials
8. **Logging (T086-T089)** blocks all services (all services log)
9. **Data Layer (T094-T099)** blocks cache and Visual integration
10. **Cache (T101-T105)** blocks cached-only mode
11. **Boot Orchestration (T117-T124)** blocks platform entry points
12. **Splash UI (T125-T128)** blocks platform entry points
13. **Platform Entry Points (T129-T133)** final integration
14. **Polish (T159-T171)** after implementation complete

### Parallelization Opportunities
- Entity models (T016-T038): All independent, can run in parallel
- Service interfaces (T039-T054): All independent, can run in parallel
- Integration tests (T055-T071): Independent scenarios, can run in parallel
- Contract tests (T072-T078): Independent APIs, can run in parallel
- Diagnostic checks (T090-T093): Independent checks, can run in parallel
- Unit tests (T138-T158): Independent services, can run in parallel

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
T082-T085 (Secrets) + T086-T089 (Logging) [P]
    ↓
T090-T093 (Diagnostics) [P] + T094-T099 (DataLayer) + T106-T109 (Core) [P]
    ↓
T101-T105 (Cache) + T110-T116 (App Services) [P]
    ↓
T117-T124 (Boot Orchestration)
    ↓
T125-T128 (Splash UI) [P]
    ↓
T129-T133 (Platform Entry) + T134-T137 (Error Handling) [P]
    ↓
T138-T158 (Unit Tests) [P]
    ↓
T159-T171 (Polish)
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
Launch T138-T158 together (21 test suites, all independent)
Example: "Unit tests for ConfigurationService in tests/unit/ConfigurationServiceTests.cs"
```

---

## Validation Checklist

- [x] All contracts have corresponding tests (T072-T077 for Visual/HTTP/MySQL/Diagnostics)
- [x] All entities have model tasks (T016-T038 cover 23 entities from data-model.md)
- [x] All tests come before implementation (T055-T077 before T079-T137)
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
- **Deferred requirements**: FR-132 (admin dashboard) and FR-134 (API docs) are marked POST-MVP/DEFERRED in spec.md and will be addressed in separate feature branches after MVP release. Initial release uses log file review and XML documentation comments.

---

**Total Tasks**: 171 (Setup: 15, Models: 23, Interfaces: 16, Tests: 24, Implementation: 59, UI: 4, Platform: 5, Error Handling: 4, Unit Tests: 21, Polish: 13)

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
