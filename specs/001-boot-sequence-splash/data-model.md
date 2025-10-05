# Data Model: Boot Sequence Entities

**Feature**: Boot Sequence — Splash-First, Services Initialization Order
**Date**: 2025-10-02
**Status**: Design Complete

## Entity Overview

This document defines the core entities and their relationships for the boot sequence system. Entities are organized by subsystem.

---

## 1. Boot Orchestration Entities

### BootMetrics
Captures telemetry data during startup for monitoring and performance optimization.

**Properties**:
- `SessionId` (Guid): Unique identifier for boot session
- `StartTimestamp` (DateTimeOffset): When boot sequence began
- `EndTimestamp` (DateTimeOffset?): When boot sequence completed (null if in progress/failed)
- `TotalDurationMs` (long?): Total boot time in milliseconds
- `Stage0DurationMs` (long): Splash screen initialization duration
- `Stage1DurationMs` (long?): Services initialization duration
- `Stage2DurationMs` (long?): Application initialization duration
- `SuccessStatus` (BootStatus): Success, Failed, Cancelled, Timeout
- `ErrorCategory` (ErrorCategory?): Transient, Configuration, Network, Permission, Permanent
- `ErrorMessage` (string?): User-friendly error description
- `ErrorDetails` (string?): Technical error details
- `MemoryUsageMB` (int): Peak memory usage during boot
- `PlatformInfo` (string): Windows, Android (Note: macOS/Linux support deferred)
- `AppVersion` (string): Application version

**Relationships**:
- Has many `StageMetrics` (one per stage)
- Has many `ServiceMetrics` (one per initialized service)

**Validation Rules**:
- `SessionId` required, must be unique
- `StartTimestamp` required
- `EndTimestamp` must be after `StartTimestamp` when set
- `TotalDurationMs` must be positive when set
- `MemoryUsageMB` must be positive

---

### StageMetrics
Detailed timing for each boot stage.

**Properties**:
- `SessionId` (Guid): Foreign key to BootMetrics
- `StageNumber` (int): 0, 1, or 2
- `StageName` (string): "Splash", "Services", "Application"
- `StartTimestamp` (DateTimeOffset): Stage start time
- `EndTimestamp` (DateTimeOffset?): Stage end time
- `DurationMs` (long?): Stage duration
- `ProgressPercentage` (int): 0-100 progress through stage
- `StatusMessage` (string): Current status message shown to user
- `OperationsCompleted` (int): Number of operations finished
- `OperationsTotal` (int): Total operations in stage

**Validation Rules**:
- `StageNumber` must be 0, 1, or 2
- `ProgressPercentage` must be 0-100
- `OperationsCompleted` <= `OperationsTotal`

---

### ServiceMetrics
Individual service initialization timing.

**Properties**:
- `SessionId` (Guid): Foreign key to BootMetrics
- `ServiceName` (string): Name of service (e.g., "ConfigurationService", "LoggingService")
- `StartTimestamp` (DateTimeOffset): When service init began
- `EndTimestamp` (DateTimeOffset?): When service init completed
- `DurationMs` (long?): Initialization duration
- `Success` (bool): Whether initialization succeeded
- `ErrorMessage` (string?): Error if failed
- `DependenciesWaitMs` (long): Time spent waiting for dependencies

**Validation Rules**:
- `ServiceName` required
- `DurationMs` must be positive when set
- `DependenciesWaitMs` must be non-negative

---

## 2. Configuration Entities

### ConfigurationProfile
Represents a complete configuration set for an environment.

**Properties**:
- `ProfileName` (string): Development, Staging, Production, Custom
- `Environment` (string): Auto-detected or explicitly set environment
- `BaseConfigPath` (string): Path to base configuration file
- `OverlayConfigPaths` (List<string>): Ordered list of overlay files
- `LastLoadedTimestamp` (DateTimeOffset): When config was last loaded
- `ConfigurationHash` (string): SHA256 hash for change detection
- `IsValid` (bool): Result of validation
- `ValidationErrors` (List<string>): Validation error messages

**Relationships**:
- Has many `ConfigurationSetting`
- Has many `FeatureFlag`

**Validation Rules**:
- `ProfileName` required, max 50 characters
- `BaseConfigPath` must exist
- `OverlayConfigPaths` must all exist

---

### ConfigurationSetting
Individual configuration key-value pair.

**Properties**:
- `Key` (string): Setting key (e.g., "Database:ConnectionString")
- `Value` (string): Setting value
- `ValueType` (ConfigValueType): String, Int, Bool, Json, Encrypted
- `Source` (ConfigSource): CommandLine, EnvironmentVariable, UserConfig, AppConfig, Default
- `IsHotReloadable` (bool): Can be changed without restart
- `IsSensitive` (bool): Should be redacted in logs
- `LastModifiedTimestamp` (DateTimeOffset): When last changed
- `LastModifiedBy` (string): User or system that changed it

**Validation Rules**:
- `Key` required, max 200 characters
- `Value` max 10,000 characters
- `Source` follows override precedence

---

### FeatureFlag
Boolean or enum flag controlling application behavior.

**Properties**:
- `FlagName` (string): Unique flag identifier
- `FlagType` (FlagType): Boolean, Enum, Percentage
- `Value` (string): Flag value (bool, enum name, or percentage 0-100)
- `IsEnabled` (bool): Computed boolean for Boolean type
- `EvaluationStrategy` (FlagEvaluation): Startup, OnDemand, Continuous
- `LastEvaluatedTimestamp` (DateTimeOffset): When last evaluated
- `Description` (string): Purpose of flag

**Validation Rules**:
- `FlagName` required, max 100 characters
- `Value` must match `FlagType` (parseable bool, valid enum, or 0-100 int)

---

## 3. Secrets Entities

### SecretEntry
Encrypted credential stored in OS-native secure storage.

**Properties**:
- `SecretKey` (string): Unique identifier (e.g., "Visual.Username", "Visual.Password")
- `SecretType` (SecretType): Password, ApiKey, ConnectionString, Certificate
- `EncryptedValue` (byte[]): Encrypted secret value
- `CreatedTimestamp` (DateTimeOffset): When secret was first stored
- `LastAccessedTimestamp` (DateTimeOffset): When last retrieved
- `LastRotatedTimestamp` (DateTimeOffset?): When last changed
- `ExpirationTimestamp` (DateTimeOffset?): When secret expires (if applicable)
- `UserId` (string): OS user account owning this secret
- `MachineId` (string): Machine identifier for binding

**Validation Rules**:
- `SecretKey` required, max 200 characters
- `EncryptedValue` required
- `UserId` and `MachineId` required for audit trail
- Never log or serialize `EncryptedValue`

---

## 4. Logging Entities

### LogEntry
Structured log entry following OpenTelemetry conventions.

**Properties**:
- `Timestamp` (DateTimeOffset): UTC timestamp (ISO 8601)
- `SeverityText` (string): Trace, Debug, Information, Warning, Error, Critical
- `SeverityNumber` (int): 1-24 (OTel spec)
- `Body` (string): Log message text
- `LoggerName` (string): Logger/category name
- `TraceId` (string): Distributed trace identifier
- `SpanId` (string): Current span identifier
- `CorrelationId` (Guid): Request correlation ID
- `ExceptionType` (string?): Exception type name if error
- `ExceptionMessage` (string?): Exception message
- `ExceptionStackTrace` (string?): Stack trace
- `ServiceName` (string): "MTM.Template.Application"
- `ServiceVersion` (string): Application version
- `DeploymentEnvironment` (string): Development, Staging, Production
- `HostName` (string): Machine hostname
- `ProcessId` (int): Process ID
- `ThreadId` (int): Thread ID
- `UserId` (string?): Current user identifier
- `SessionId` (Guid?): User session identifier
- `Attributes` (Dictionary<string, object>): Custom structured fields

**Validation Rules**:
- `Timestamp` required
- `SeverityText` and `SeverityNumber` must align
- `Body` required
- All PII in `Body` and `Attributes` must be redacted

---

### TelemetryBatch
Queued telemetry for remote transmission.

**Properties**:
- `BatchId` (Guid): Unique batch identifier
- `LogEntries` (List<LogEntry>): Logs in this batch
- `CreatedTimestamp` (DateTimeOffset): When batch was created
- `RetryCount` (int): Number of send attempts
- `LastRetryTimestamp` (DateTimeOffset?): When last retry occurred
- `Status` (BatchStatus): Pending, Sent, Failed

**Validation Rules**:
- Max 1000 log entries per batch
- `RetryCount` max 5 before dead letter

---

## 5. Diagnostics Entities

### DiagnosticResult
Comprehensive system health check result.

**Properties**:
- `DiagnosticId` (Guid): Unique diagnostic run identifier
- `Timestamp` (DateTimeOffset): When diagnostics ran
- `OverallStatus` (DiagnosticStatus): Healthy, Degraded, Unhealthy
- `PermissionsStatus` (HealthStatus): Healthy, Warning, Error
- `PermissionsDetails` (string): Specific permissions checked
- `StorageStatus` (HealthStatus): Healthy, Warning, Error
- `StorageAvailableMB` (long): Available storage in MB
- `StoragePercentUsed` (int): Storage utilization 0-100
- `NetworkStatus` (HealthStatus): Healthy, Warning, Error
- `NetworkLatencyMs` (int?): Average network latency
- `NetworkBandwidthKBps` (int?): Download bandwidth in KB/s
- `HardwareInfo` (HardwareCapabilities): CPU, RAM, GPU, etc.
- `PerformanceScore` (int): Composite performance score 0-100
- `DetectedIssues` (List<DiagnosticIssue>): Specific problems found
- `AutoFixesApplied` (List<string>): Automatic fixes applied

**Relationships**:
- Has many `DiagnosticIssue`
- Has one `HardwareCapabilities`

**Validation Rules**:
- `StoragePercentUsed` must be 0-100
- `PerformanceScore` must be 0-100

---

### HardwareCapabilities
Detected hardware specifications.

**Properties**:
- `CpuCores` (int): Number of CPU cores
- `TotalMemoryMB` (long): Total RAM in MB
- `ScreenResolution` (string): Width x Height (e.g., "1920x1080")
- `GpuAvailable` (bool): Whether GPU detected
- `GpuName` (string?): GPU model name
- `StorageType` (StorageType): SSD, HDD, Unknown
- `NetworkAdapters` (List<string>): Network adapter names
- `CameraAvailable` (bool): Camera detected (Android)
- `OsVersion` (string): Operating system version

**Validation Rules**:
- `CpuCores` must be positive
- `TotalMemoryMB` must be positive

---

### DiagnosticIssue
Specific problem detected during diagnostics.

**Properties**:
- `IssueId` (Guid): Unique issue identifier
- `IssueName` (string): Short name (e.g., "LowStorage", "MissingPermission")
- `Severity` (IssueSeverity): Info, Warning, Error, Critical
- `Description` (string): User-friendly description
- `TechnicalDetails` (string): Technical information for support
- `ResolutionSteps` (List<string>): Step-by-step fix instructions
- `CanAutoFix` (bool): Whether automatic fix available
- `AutoFixApplied` (bool): Whether auto-fix was attempted
- `AutoFixSucceeded` (bool?): Result of auto-fix attempt

**Validation Rules**:
- `IssueName` required, max 50 characters
- `Description` required, max 500 characters
- If `AutoFixApplied` true, `AutoFixSucceeded` must be set

---

## 6. Data Layer Entities

### ConnectionPoolMetrics
Tracks database/API connection pool health.

**Properties**:
- `PoolName` (string): MySQL, VisualApiToolkit, HttpApi
- `MinConnections` (int): Minimum pool size
- `MaxConnections` (int): Maximum pool size
- `ActiveConnections` (int): Currently in use
- `IdleConnections` (int): Available for use
- `AverageWaitTimeMs` (int): Average wait for connection
- `PeakConnections` (int): Highest usage observed
- `TotalConnectionsCreated` (long): Lifetime connections created
- `TotalConnectionErrors` (long): Failed connection attempts
- `TotalTimeouts` (long): Connection timeouts
- `AverageConnectionLifespanSec` (int): Average connection lifespan

**Validation Rules**:
- `ActiveConnections` + `IdleConnections` <= `MaxConnections`
- All timing values must be non-negative

---

### CircuitBreakerState
Tracks circuit breaker status for service endpoints.

**Properties**:
- `ServiceName` (string): Service/endpoint name
- `State` (CircuitState): Closed, Open, HalfOpen
- `FailureCount` (int): Consecutive failures in current window
- `FailureThreshold` (int): Failures before opening
- `LastFailureTimestamp` (DateTimeOffset?): When last failure occurred
- `NextRetryTimestamp` (DateTimeOffset?): When to attempt recovery
- `SuccessCount` (int): Successes since last open
- `CurrentBackoffSec` (int): Current backoff duration

**Validation Rules**:
- `State` changes must follow valid transitions (Closed→Open→HalfOpen→Closed)
- `FailureCount` resets to 0 on successful recovery

---

## 7. Cache Entities

### CacheEntry
Represents cached Visual master data.

**Properties**:
- `CacheKey` (string): Unique identifier (e.g., "Part:12345", "Location:A-01")
- `EntityType` (CachedEntityType): Part, Location, Warehouse, WorkCenter
- `EntityId` (string): Visual entity primary key
- `Data` (byte[]): Compressed (LZ4) JSON data
- `SchemaVersion` (int): Cache schema version
- `CreatedTimestamp` (DateTimeOffset): When first cached
- `LastRefreshedTimestamp` (DateTimeOffset): When last synced from Visual
- `ExpiresTimestamp` (DateTimeOffset): TTL expiration
- `SourceVersion` (long): Visual entity version for delta sync
- `SourceTimestamp` (DateTimeOffset): Visual entity LastModified
- `IsStale` (bool): Computed: past TTL or delta detected
- `HitCount` (long): Number of cache reads
- `LastAccessedTimestamp` (DateTimeOffset): Last read time

**Validation Rules**:
- `CacheKey` required, unique
- `Data` must be valid LZ4 compressed JSON
- `ExpiresTimestamp` > `LastRefreshedTimestamp`

---

### CacheStatistics
Aggregated cache performance metrics.

**Properties**:
- `EntityType` (CachedEntityType): Part, Location, Warehouse, WorkCenter
- `TotalEntries` (long): Number of cached items
- `CompressedSizeMB` (int): Storage used
- `UncompressedSizeMB` (int): Expanded size
- `CompressionRatio` (decimal): Compressed / Uncompressed
- `HitCount` (long): Successful cache reads
- `MissCount` (long): Cache misses (fetched from source)
- `HitRate` (decimal): HitCount / (HitCount + MissCount)
- `AverageAgeDays` (int): Average cache entry age
- `StaleEntries` (long): Entries past TTL
- `LastRefreshTimestamp` (DateTimeOffset): Last cache refresh
- `LastRefreshDurationMs` (long): Duration of last refresh
- `TotalRefreshErrors` (long): Refresh failures

**Validation Rules**:
- All counts must be non-negative
- `HitRate` must be 0.0-1.0

---

## 8. Core Services Entities

### MessageEnvelope
Message bus message wrapper.

**Properties**:
- `MessageId` (Guid): Unique message identifier
- `MessageType` (string): Fully qualified type name
- `Payload` (object): Message data
- `Priority` (MessagePriority): Low, Normal, High, Critical
- `DeliveryGuarantee` (DeliveryType): AtMostOnce, AtLeastOnce, ExactlyOnce
- `CorrelationId` (Guid): For request/response correlation
- `PublishedTimestamp` (DateTimeOffset): When message was published
- `DeliveryAttempts` (int): Number of delivery tries
- `LastDeliveryAttemptTimestamp` (DateTimeOffset?): Last attempt time
- `Status` (MessageStatus): Pending, Delivered, DeadLettered

**Validation Rules**:
- `MessageType` required
- `Payload` required
- `DeliveryAttempts` max 5 before dead letter

---

### ValidationRuleMetadata
Discovered validation rule information.

**Properties**:
- `RuleName` (string): Rule identifier
- `TargetType` (string): Type being validated
- `RuleSource` (ValidationSource): Attribute, Convention, Configuration
- `Priority` (int): Execution order
- `IsAsync` (bool): Whether rule is async
- `CacheDurationSec` (int?): Cache validation results

**Validation Rules**:
- `RuleName` required, unique per `TargetType`
- `Priority` lower values execute first

---

## 9. Localization & Theme Entities

### LocalizationSetting
User's language and format preferences.

**Properties**:
- `UserId` (string): User identifier
- `PreferredLanguage` (string): ISO 639-1 code (en, es, fr, etc.)
- `PreferredRegion` (string): ISO 3166-1 code (US, MX, FR, etc.)
- `DateFormat` (string): Date format pattern
- `NumberFormat` (string): Number format pattern
- `CurrencyFormat` (string): Currency format pattern
- `LastChangedTimestamp` (DateTimeOffset): When last modified

**Validation Rules**:
- `PreferredLanguage` must be supported ISO code
- `PreferredRegion` must be valid ISO code

---

### MissingTranslation
Tracked missing translation for localization team.

**Properties**:
- `TranslationKey` (string): Key requested
- `RequestedLanguage` (string): Language code
- `FallbackLanguage` (string): Language used (typically "en")
- `FirstRequestedTimestamp` (DateTimeOffset): When first encountered
- `RequestCount` (long): Number of times requested
- `LastRequestedTimestamp` (DateTimeOffset): Most recent request

**Validation Rules**:
- `TranslationKey` + `RequestedLanguage` unique
- `RequestCount` must be positive

---

### ThemeConfiguration
User's visual theme preferences.

**Properties**:
- `UserId` (string): User identifier
- `ThemeMode` (ThemeMode): Light, Dark, Auto
- `AccentColor` (string?): Optional custom accent color
- `HighContrastEnabled` (bool): Accessibility mode
- `LastChangedTimestamp` (DateTimeOffset): When last modified

**Validation Rules**:
- `ThemeMode` required
- `AccentColor` must be valid hex color if set

---

## 10. Navigation Entities

### NavigationHistoryEntry
Single navigation history entry.

**Properties**:
- `EntryId` (Guid): Unique entry identifier
- `SessionId` (Guid): User session
- `ScreenName` (string): Target screen/view name
- `Parameters` (Dictionary<string, string>): Navigation parameters
- `Timestamp` (DateTimeOffset): When navigated
- `DurationSeconds` (int?): Time spent on screen (null if current)

**Relationships**:
- Belongs to circular buffer (max 25 entries)

**Validation Rules**:
- `ScreenName` required, max 100 characters
- `DurationSeconds` must be non-negative when set

---

## 11. Error Handling Entities

### ErrorReport
Captured failure information with diagnostic context.

**Properties**:
- `ErrorId` (Guid): Unique error identifier
- `ErrorCategory` (ErrorCategory): Transient, Configuration, Network, Permission, Permanent
- `UserFriendlyMessage` (string): Non-technical error description
- `TechnicalDetails` (string): Stack trace and technical info
- `DiagnosticBundlePath` (string?): Path to diagnostic bundle file
- `RecoveryInstructions` (List<string>): Step-by-step guidance
- `OccurredTimestamp` (DateTimeOffset): When error happened
- `ReportedToRemote` (bool): Whether sent to telemetry
- `ReportedTimestamp` (DateTimeOffset?): When sent
- `FrequencyCount` (int): How many times this error occurred
- `IsChronicIssue` (bool): Exceeds chronic threshold

**Validation Rules**:
- `UserFriendlyMessage` required, max 500 characters
- `TechnicalDetails` max 50,000 characters
- `FrequencyCount` must be positive

---

## Entity Relationships Summary

```
BootMetrics 1 ──── * StageMetrics
BootMetrics 1 ──── * ServiceMetrics

ConfigurationProfile 1 ──── * ConfigurationSetting
ConfigurationProfile 1 ──── * FeatureFlag

DiagnosticResult 1 ──── * DiagnosticIssue
DiagnosticResult 1 ──── 1 HardwareCapabilities

LogEntry * ──── 1 TelemetryBatch

Session 1 ──── * NavigationHistoryEntry (circular buffer, max 25)

CacheStatistics (aggregates) * CacheEntry
```

## Enumeration Types

- **BootStatus**: Success, Failed, Cancelled, Timeout
- **ErrorCategory**: Transient, Configuration, Network, Permission, Permanent
- **ConfigValueType**: String, Int, Bool, Json, Encrypted
- **ConfigSource**: CommandLine, EnvironmentVariable, UserConfig, AppConfig, Default
- **FlagType**: Boolean, Enum, Percentage
- **FlagEvaluation**: Startup, OnDemand, Continuous
- **SecretType**: Password, ApiKey, ConnectionString, Certificate
- **DiagnosticStatus**: Healthy, Degraded, Unhealthy
- **HealthStatus**: Healthy, Warning, Error
- **StorageType**: SSD, HDD, Unknown
- **IssueSeverity**: Info, Warning, Error, Critical
- **CircuitState**: Closed, Open, HalfOpen
- **CachedEntityType**: Part, Location, Warehouse, WorkCenter
- **MessagePriority**: Low, Normal, High, Critical
- **DeliveryType**: AtMostOnce, AtLeastOnce, ExactlyOnce
- **MessageStatus**: Pending, Delivered, DeadLettered
- **ValidationSource**: Attribute, Convention, Configuration
- **ThemeMode**: Light, Dark, Auto
- **BatchStatus**: Pending, Sent, Failed

---

*Data model complete. Ready for contract generation and implementation.*
