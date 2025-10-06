# Feature Specification: Environment and Configuration Management System

**Feature Branch**: `002-environment-and-configuration`
**Created**: 2025-10-05
**Status**: Draft - Partially Implemented (Boot Feature 001)
**Input**: User description: "Environment and Configuration Management System"

## Execution Flow (main)

```
1. Parse user description from Input
   → Feature: Environment and Configuration Management System
2. Extract key concepts from description
   → Actors: Developers, System Administrators, End Users, AI Agents
   → Actions: Configure, Store, Retrieve, Evaluate, Detect, Validate, Notify
   → Data: Configuration values, Credentials, Feature flags, Environment variables
   → Constraints: Platform-specific (Windows Desktop + Android only), OS-native security
3. Unclear aspects marked with [NEEDS CLARIFICATION]
4. User Scenarios & Testing section completed
5. Functional Requirements generated (testable)
6. Key Entities identified (ConfigurationService, SecretsService, FeatureFlagEvaluator)
7. Review Checklist passed with clarification markers
8. Return: SUCCESS (spec ready for planning with documented ambiguities)
```

---

## Clarifications

*Use this section whenever any [NEEDS CLARIFICATION: …] markers exist in the spec. Keep each question atomic and traceable.*

### Index
**Resolved (Session 2025-10-05)**: CL-001 (Credential Recovery), CL-002 (Unconfigured Flags), CL-003 (Invalid Env Vars), CL-004 (Storage Unavailable), CL-005 (User Preference Location), CL-006 (DB Schema Config), CL-007 (Android Connectivity), CL-008 (Visual Whitelist), CL-009 (Error Notifications), CL-010 (Flag Sync), CL-011 (Folder Path Config), CL-012 (Schema Location), CL-013 (Accessibility), CL-014 (Data Scale), CL-015 (Compliance), CL-016 (MySQL Compression), CL-017 (Env Var Timing), CL-018 (Env Var Precedence), CL-019 (Flag Naming), CL-020 (Invalid Flag Names), CL-021 (Change Notifications), CL-022 (Notification Efficiency), CL-023 (Visual Connectivity Validation), CL-024 (Schema Migration Failures), CL-025 (Credential Dialog Cancellation), CL-026 (Network Partition Handling), CL-027 (Config File Corruption), CL-028 (User Config Architecture)

---

### CL-001: Credential Storage Recovery
- **Status**: Answered (2025-10-05)
- **Question**: When credentials cannot be retrieved from OS-native storage (corrupted keychain, revoked permissions), what should the system do?
- **Answer/Decision**: Prompt user for re-entry - Show credential dialog, attempt to re-save to OS storage
- **Spec Changes**: Updated FR-013 to include credential recovery dialog requirement

### CL-002: Unconfigured Feature Flags
- **Status**: Answered (2025-10-05)
- **Question**: When checking if a feature is enabled but the feature's settings haven't been configured yet, what should happen?
- **Answer/Decision**: Keep feature turned off - Treat unconfigured features as disabled until enabled, record note in logs
- **Spec Changes**: Updated FR-021 to specify default disabled behavior for unregistered flags

### CL-003: Invalid Environment Variable Values
- **Status**: Answered (2025-10-05)
- **Question**: When reading a configuration setting from an environment variable but the value doesn't match what's expected (e.g., "ABC" instead of a number), what should happen?
- **Answer/Decision**: Combination of use default value with log warning AND show dialog to user for correction - Application continues with safe defaults while alerting user to the configuration problem
- **Spec Changes**: Updated FR-037 and edge cases to include severity-based notification approach

### CL-004: Complete Storage System Failure
- **Status**: Answered (2025-10-05)
- **Question**: When OS-native secure storage is completely unavailable (not just specific credentials, but entire storage system), what should the system do?
- **Answer/Decision**: Prompt user for required credentials with clear explanation - Show user-friendly dialog explaining that secure storage is unavailable and request re-entry of needed credentials (e.g., "We couldn't access your saved login information. Please enter your Visual ERP username and password to continue.")
- **Spec Changes**: Updated NFR-010 to specify user-friendly error dialog requirement, updated edge cases

### CL-005: User Preference Storage Location
- **Status**: Answered (2025-10-05)
- **Question**: When a user changes a setting at runtime (display preferences, default filters, UI customization), where should those personal settings be saved?
- **Answer/Decision**: Combination of admin-configured central server location AND database storage - User folder locations stored in admin-configured central server location, user preferences stored in MAMP MySQL 5.7 database
- **Spec Changes**: Updated FR-032 to specify dual-storage approach for user configuration

### CL-006: Database Schema Configuration
- **Status**: Answered (2025-10-05)
- **Question**: Where should user preferences (like display settings, sort orders, default filters) be saved in the database?
- **Answer/Decision**: JSON configuration file with placeholders - Create a JSON configuration file defining database schema (table structures, column types, constraints, relationships) with placeholder values that developer will replace later before MySQL setup
- **Spec Changes**: Updated FR-032 and Key Entities to include DatabaseSchemaConfiguration entity

### CL-007: Android Database Connectivity
- **Status**: Answered (2025-10-05)
- **Question**: Should the Android version connect directly to the MySQL database or use an intermediary server?
- **Answer/Decision**: MTM Server API - Android app connects to MTM Server API via HTTPS (same architecture as Feature 001). Server handles MySQL and Visual API Toolkit connections server-side. This provides security boundary, connection pooling, cached Visual data projections, and device certificate validation
- **Spec Changes**: Updated FR-025 to specify Android MTM Server API connectivity

### CL-008: Visual API Toolkit Command Whitelist
- **Status**: Answered (2025-10-05)
- **Question**: Where should the list of allowed Visual API Toolkit commands be stored, and what commands should be included?
- **Answer/Decision**: Configuration file with read-only commands - Store whitelist in `docs/VISUAL-WHITELIST.md` and validate in application code. Include any Visual API Toolkit commands that are NOT write commands (read-only operations only). All commands MUST use citation format: `"Reference-{FileName} - {Chapter/Section/Page}"`
- **Spec Changes**: Updated FR-030 and FR-031 to specify dual-storage whitelist approach

### CL-009: Configuration Error Notifications
- **Status**: Answered (2025-10-05)
- **Question**: When showing the user a configuration error notification, what should the message look like and how should it appear?
- **Answer/Decision**: Severity-dependent approach - For non-critical settings (display preferences, timeouts, filters): Show status bar indicator with warning icon that displays details on click (non-intrusive). For critical settings (user credentials, database connection, required configuration): Show modal dialog blocking user interaction until setting is corrected (forces resolution before continuing)
- **Spec Changes**: Updated FR-037 to specify severity-based notification strategy

### CL-010: Feature Flag Server Synchronization
- **Status**: Answered (2025-10-05)
- **Question**: When feature flags are changed on the server, how should running user applications respond?
- **Answer/Decision**: Next launch only - Feature flag changes on server are applied only when user's MTM_Application_Launcher detects version mismatch and updates their local copy. This ties feature flags to version deployments for predictable, stable experience and leverages existing launcher infrastructure
- **Spec Changes**: Added FR-022a to specify launch-time-only flag synchronization

### CL-011: User Folder Path Configuration
- **Status**: Answered (2025-10-05)
- **Question**: Where should admin-configured central server path for user folders be specified?
- **Answer/Decision**: Use JSON config file with placeholder; default to MyDocuments for now
- **Spec Changes**: Updated FR-032 to specify config/user-folders.json location

### CL-012: Database Schema File Location
- **Status**: Answered (2025-10-05)
- **Question**: Where should the database schema configuration file be located?
- **Answer/Decision**: root/config folder
- **Spec Changes**: Updated FR-032 and DatabaseSchemaConfiguration entity to specify config/ location

### CL-013: Accessibility and Localization
- **Status**: Answered (2025-10-05)
- **Question**: What accessibility and localization support is required?
- **Answer/Decision**: Both accessibility and localization; localization service already implemented
- **Spec Changes**: Added Non-Functional Requirements section for Accessibility & Localization

### CL-014: Data Volume and Scale
- **Status**: Answered (2025-10-05)
- **Question**: What are the data volume/scale assumptions?
- **Answer/Decision**: Up to 1,000 users, moderate data volume; allow admin to remove inactive users and their data
- **Spec Changes**: Added Non-Functional Requirements section for Data Volume & Scale

### CL-015: Compliance Requirements
- **Status**: Answered (2025-10-05)
- **Question**: Are there compliance or regulatory constraints?
- **Answer/Decision**: No specific compliance required; not health-related, based in USA
- **Spec Changes**: Added Non-Functional Requirements section for Compliance

### CL-016: MySQL Compression Protocol
- **Status**: Answered (2025-10-05)
- **Question**: When connecting to MAMP MySQL database, should the connection use compressed or uncompressed protocol?
- **Answer/Decision**: Standard uncompressed protocol (UseCompression=false) - This provides universal compatibility with MAMP MySQL 5.7 and avoids EndOfStreamException errors during connection. Compression is unnecessary for local development database connections where network latency is negligible
- **Spec Changes**: No direct spec change (implementation detail), documented in clarifications for developer reference

### CL-017: Environment Variable Reading Timing
- **Status**: Answered (2025-10-05)
- **Question**: When should the configuration system check environment variables (MTM_ENVIRONMENT, ASPNETCORE_ENVIRONMENT, DOTNET_ENVIRONMENT)?
- **Answer/Decision**: Read all environment variables once at application startup only. This provides optimal performance for manufacturing environments where configuration doesn't change during runtime. Environment variable changes require application restart, which is standard practice in production deployments
- **Spec Changes**: Updated FR-001 and edge cases to clarify startup-only environment detection

### CL-018: Environment Variable Precedence
- **Status**: Answered (2025-10-05)
- **Question**: When multiple environment variables are set, which one takes precedence?
- **Answer/Decision**: MTM_ENVIRONMENT (highest priority) → ASPNETCORE_ENVIRONMENT → DOTNET_ENVIRONMENT → build configuration (Debug/Release) → application defaults (lowest priority). Application-specific variables override framework defaults to match user expectations
- **Spec Changes**: Updated FR-001 to specify exact precedence order

### CL-019: Feature Flag Naming Rules
- **Status**: Answered (2025-10-05)
- **Question**: What characters should be allowed in feature flag names?
- **Answer/Decision**: Allow letters, numbers, dots, underscores, AND hyphens only (regex: `^[a-zA-Z0-9._-]+$`). This supports hierarchical organization (Visual.UseForItems) and common naming conventions (kebab-case, snake_case, dot.notation) without causing technical problems in URLs, file paths, or configuration files
- **Spec Changes**: No direct FR addition (implementation detail), documented in clarifications for developer reference

### CL-020: Invalid Feature Flag Name Handling
- **Status**: Answered (2025-10-05)
- **Question**: When someone tries to register a feature flag with an invalid name, what should happen?
- **Answer/Decision**: Fail immediately with ArgumentException containing clear error message explaining the naming rules. Failing fast prevents downstream problems and encourages developers to fix issues immediately rather than discovering them later during deployment
- **Spec Changes**: No direct FR addition (implementation detail), documented in clarifications for developer reference

### CL-021: Configuration Change Notifications
- **Status**: Answered (2025-10-05)
- **Question**: When a configuration value changes, should other parts of the application be notified automatically?
- **Answer/Decision**: Yes - automatically notify all interested parts of the application immediately when settings change (OnConfigurationChanged event). This provides optimal user experience in manufacturing environments where users expect immediate visual feedback when changing settings like theme, language, or default filters
- **Spec Changes**: Updated FR-007 and NFR-004 to specify automatic change notification requirement

### CL-022: Notification Efficiency
- **Status**: Answered (2025-10-05)
- **Question**: Should change notifications happen for every save operation, or only when the value actually changes?
- **Answer/Decision**: Only send notifications when the new value differs from the old value. This combines best user experience (immediate feedback when needed) with optimal performance (no wasted processing for unchanged values). This is the standard pattern in modern UI frameworks and prevents unnecessary UI re-renders
- **Spec Changes**: Updated FR-007 to clarify change notifications fire only when value differs

### CL-024: Database Schema Migration Failures
- **Status**: Answered (2025-10-05)
- **Question**: When database schema migration fails (corrupted schema file, incompatible MySQL version, schema conflicts), what should happen?
- **Answer/Decision**: Use in-memory fallback for non-vital data; block app startup for vital data. Application continues running with user preferences stored in memory only (not persisted across sessions) when schema migration fails for non-critical data. For vital configuration data required for application operation, block startup with error dialog requiring admin intervention. This provides balance between operational continuity for non-critical failures and safety for critical failures
- **Spec Changes**: Added FR-038 for schema migration failure handling with severity-based approach

### CL-025: Credential Dialog Cancellation
- **Status**: Answered (2025-10-05)
- **Question**: When the credential recovery dialog is shown and the user clicks "Cancel" instead of entering credentials, what should happen?
- **Answer/Decision**: Block app entirely - Close application immediately when user cancels credential dialog. Dialog MUST clearly explain "If you cancel, the application will close" before user makes decision. This enforces security by ensuring credentials are required while giving users informed choice about closing
- **Spec Changes**: Updated FR-013 to specify app closure on credential dialog cancellation with clear warning text

### CL-026: Network Partition Handling
- **Status**: Answered (2025-10-05)
- **Question**: When Android device loses network connectivity during operation (user in warehouse moves to area without WiFi), what should the configuration system do?
- **Answer/Decision**: Block operations requiring server - Allow viewing cached data and continue local operations, but disable any operations requiring server connectivity (config updates, credential refresh, Visual ERP sync). Show "Offline Mode" indicator and reconnect button in status bar. This maintains productivity while preventing dangerous operations during network issues
- **Spec Changes**: Added FR-039 for Android offline mode behavior and edge case documentation

### CL-027: Config File Corruption Recovery
- **Status**: Answered (2025-10-05)
- **Question**: When the user configuration file is corrupted or cannot be read (file system error, power loss during write, invalid JSON), what should happen?
- **Answer/Decision**: Prompt user for recovery action - Show dialog with options: "Restore defaults", "Try to repair", "Contact support". If user attempts repair and it fails, disable the "Repair" button and show explanation of failure, leaving only "Restore defaults" and "Contact support" options available. This respects user agency while preventing infinite retry loops
- **Spec Changes**: Added FR-040 for config file corruption recovery with interactive user dialog

### CL-028: User Configuration Architecture
- **Status**: Answered (2025-10-05)
- **Question**: When two users modify the same global configuration setting simultaneously, how should conflicts be resolved?
- **Answer/Decision**: User-based filter architecture prevents this conflict - Each user has their own configuration settings (filters, preferences, UI customization). There are no shared global configuration settings that multiple users can modify simultaneously. This architectural decision eliminates concurrent update conflicts for user preferences
- **Spec Changes**: Updated FR-032 and Key Entities to clarify user-scoped configuration architecture

### CL-023: Visual ERP Connectivity Validation Scope
- **Status**: Answered (2025-10-05)
- **Question**: What happens when Visual ERP credentials are valid but the API endpoint is unreachable? Is this a configuration concern or a service resilience concern? Should config system validate connectivity?
- **Answer/Decision**: Service layer handles connectivity with retry logic (configuration just stores endpoint) - Configuration system is responsible only for storing and retrieving endpoint URLs and credentials. Service layer is responsible for connectivity validation, retry logic, circuit breakers, and resilience patterns (using Polly). This maintains clean separation of concerns: configuration manages "what to connect to", services manage "how to connect and handle failures"
- **Spec Changes**: Edge case already updated to clarify service layer responsibility for connectivity validation

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers
- 📅 Keep it concise and focused on key points
- 🔄 Include examples and use cases to illustrate concept
- ✍️ Use clear and simple language, avoiding jargon, but make it easy for a copilot agent to understand

---

## User Scenarios & Testing

### Primary User Story

**Developer Scenario**: A developer working on the MTM Avalonia Template application needs to configure the system for different environments (Development, Staging, Production). They need to securely store Visual ERP credentials, toggle feature flags to enable/disable functionality, and ensure the application behaves correctly in each environment without accidentally leaking production credentials or enabling unsafe development features in production.

**System Administrator Scenario**: A system administrator deploying the application to production needs to set environment-specific variables (database connection strings, API endpoints) without modifying code or configuration files, ensuring credentials are stored securely using OS-native mechanisms (Windows DPAPI or Android KeyStore).

**End User Scenario**: An end user launches the application, and it automatically detects the current environment, loads appropriate configuration values with proper precedence (environment variables override user settings override defaults), and enables/disables features based on rollout percentages and environment flags.

### Acceptance Scenarios

1. **Given** a developer has set MTM_ENVIRONMENT="Development", **When** the application starts, **Then** it loads development-specific settings and enables all development feature flags.

2. **Given** a system administrator stores Visual ERP credentials using OS-native secrets storage, **When** the application needs to authenticate, **Then** it retrieves credentials securely without exposing them in logs or configuration files.

3. **Given** a feature flag "Visual.UseForItems" has a 50% rollout percentage in Staging environment, **When** the application evaluates this flag, **Then** approximately 50% of users see the feature enabled (deterministic per user).

4. **Given** an environment variable "MTM_API_TIMEOUT=60" is set, **When** the application reads "API:TimeoutSeconds" configuration, **Then** it returns 60 (environment variable precedence over defaults).

5. **Given** a user has changed a configuration value at runtime via ConfigurationService.SetValue(), **When** the configuration changes, **Then** all subscribers receive a notification event with old and new values.

6. **Given** the application is running on Android, **When** it needs to store credentials, **Then** it uses Android KeyStore (not Windows DPAPI).

7. **Given** the application is running on an unsupported OS (macOS, Linux), **When** it attempts to initialize SecretsService, **Then** it throws PlatformNotSupportedException.

### Edge Cases

- **What happens when environment variable MTM_ENVIRONMENT is not set?**
  → System defaults to "Development" (DEBUG builds) or "Production" (RELEASE builds) based on build configuration.

- **What happens when a configuration key doesn't exist?**
  → System returns the provided default value without throwing an exception.

- **What happens when credentials cannot be retrieved from OS-native storage?**
  → System prompts user with credential dialog for re-entry and attempts to re-save to OS-native storage. This provides a user-recoverable path when storage is corrupted or permissions are revoked.

- **What happens when a feature flag doesn't exist in the registry?**
  → System treats the feature as disabled by default and logs a warning. This prevents unexpected behavior from unconfigured features while allowing the application to run normally.

- **How does system handle concurrent configuration updates from multiple threads?**
  → System uses thread-safe locking to prevent race conditions (already implemented).

- **What happens when environment variables contain invalid data types (e.g., "ABC" for an integer setting)?**
  → System uses severity-based approach: (1) Non-critical settings - Uses default value, logs warning, shows status bar warning icon with details available on click; (2) Critical settings (credentials, database connection) - Shows modal dialog requiring user to correct setting before continuing. This ensures application stability while appropriately alerting users based on error impact.

- **What happens when Visual ERP credentials are valid but the API endpoint is unreachable?**
  → Service layer responsibility (not configuration system). Configuration system stores endpoint URLs and credentials; service layer handles connectivity validation, retry logic, and resilience patterns. This maintains clean separation of concerns.

- **What happens when OS-native secure storage is completely unavailable?**
  → System displays user-friendly dialog explaining the storage issue in plain language (e.g., "We couldn't access your saved login information") and prompts user to enter required credentials. Application continues with user-provided credentials for the session. System attempts to re-save credentials when storage becomes available.

- **What happens when database schema migration fails during startup?**
  → System evaluates severity: (1) Non-vital data (user preferences, UI settings) - Continue with in-memory storage, log warning, show status indicator that preferences won't persist; (2) Vital data (required configuration) - Block app startup, show error dialog with admin instructions, require manual intervention.

- **What happens when user cancels the credential recovery dialog?**
  → Application closes immediately. Dialog displays clear warning: "If you cancel, the application will close" before user makes decision. This enforces security requirement that credentials are mandatory for application operation.

- **What happens when Android device loses network connectivity during operation?**
  → System enters "Offline Mode": Allow viewing cached data and local operations, disable operations requiring server (config updates, credential refresh, Visual ERP sync), show "Offline Mode" indicator in status bar with reconnect button. Automatically reconnect when network becomes available.

- **What happens when user configuration file is corrupted?**
  → Show recovery dialog with options: "Restore defaults", "Try to repair", "Contact support". If repair is attempted and fails, disable "Repair" button, explain failure, leave "Restore defaults" and "Contact support" options. Log corruption details for support diagnostics.

---

## Requirements

### Functional Requirements

#### Environment Detection
- **FR-001**: System MUST automatically detect the current environment using the following precedence: MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → DOTNET_ENVIRONMENT → Build configuration default (Development for DEBUG, Production for RELEASE).
- **FR-002**: System MUST support three distinct environments: Development, Staging, and Production.
- **FR-003**: System MUST allow developers to override environment detection by setting the MTM_ENVIRONMENT environment variable.

#### Configuration Management
- **FR-004**: System MUST implement layered configuration precedence: Environment Variables (highest) → User Configuration (runtime-set) → Application Defaults (fallback).
- **FR-005**: System MUST provide ConfigurationService.GetValue<T>(key, defaultValue) for reading configuration values with type safety and default fallback.
- **FR-006**: System MUST provide ConfigurationService.SetValue(key, value) for runtime configuration updates.
- **FR-007**: System MUST emit configuration change events when values are updated, including key, old value, and new value.
- **FR-008**: System MUST ensure thread-safe access to configuration values across multiple threads.
- **FR-009**: Environment variable keys MUST use underscore format (e.g., MTM_ENVIRONMENT, MTM_API_TIMEOUT) for OS compatibility.
- **FR-010**: System MUST NOT throw exceptions when reading non-existent configuration keys; it MUST return the provided default value.

#### Secure Credentials Storage
- **FR-011**: System MUST store credentials using OS-native secure storage mechanisms (WindowsSecretsService using Windows DPAPI for Windows Desktop, AndroidSecretsService using Android KeyStore for Android).
- **FR-012**: System MUST provide SecretsService.SetSecretAsync(key, value) for storing credentials securely.
- **FR-013**: System MUST provide SecretsService.GetSecretAsync(key) for retrieving stored credentials. When credential retrieval fails (corrupted storage, revoked permissions), system MUST display credential entry dialog to allow user re-entry and attempt to re-save to OS-native storage. Dialog MUST include clear warning: "If you cancel, the application will close". If user cancels dialog, application MUST close immediately.
- **FR-014**: System MUST NEVER log credentials, passwords, tokens, or other sensitive data in plaintext.
- **FR-015**: System MUST throw PlatformNotSupportedException when attempting to use SecretsService on unsupported platforms (macOS, Linux, iOS).
- **FR-016**: System MUST support storing Visual ERP username and password for authentication.

#### Feature Flag Management
- **FR-017**: System MUST provide FeatureFlagEvaluator for registering and evaluating feature flags.
- **FR-018**: System MUST support the following core feature flags: Visual.UseForItems, Visual.UseForLocations, Visual.UseForWorkCenters, OfflineModeAllowed, Printing.Enabled.
- **FR-019**: System MUST support environment-specific feature flag configuration (flags can be enabled in Development but disabled in Production).
- **FR-020**: System MUST support rollout percentage (0-100%) for gradual feature rollouts.
- **FR-021**: System MUST provide FeatureFlagEvaluator.IsEnabledAsync(flagName) for checking if a feature is enabled. When a feature flag is not registered, system MUST treat it as disabled and log a warning message.
- **FR-022**: System MUST provide FeatureFlagEvaluator.SetEnabledAsync(flagName, enabled) for runtime flag updates.
- **FR-022a**: Feature flag changes made on server MUST only take effect in user applications after MTM_Application_Launcher detects version mismatch and updates local copy. Running applications MUST NOT synchronize feature flags in real-time from server.
- **FR-023**: Feature flag evaluation MUST be deterministic per user (same user always sees same flag state for a given rollout percentage).

#### Platform Support
- **FR-024**: System MUST support Windows Desktop platform with direct MySQL connection and direct Visual API Toolkit client.
- **FR-025**: System MUST support Android platform via MTM Server API (HTTPS connection). Android devices MUST NOT connect directly to MySQL or Visual ERP. Server-side handles MySQL access, Visual API Toolkit commands, connection pooling, and cached Visual data projections.
- **FR-025a**: Android authentication MUST use two-factor auth (user credentials + device certificate stored in Android KeyStore).
- **FR-026**: System MUST explicitly reject unsupported platforms (macOS, Linux, iOS) with clear error messages.
- **FR-027**: System MUST use platform-specific services via factory pattern (SecretsServiceFactory creates platform-appropriate implementations).

#### Visual ERP Integration Configuration
- **FR-028**: System MUST enforce read-only access to Visual ERP data (architectural principle, not runtime flag).
- **FR-029**: System MUST store Visual ERP credentials using SecretsService (not plaintext configuration):
  - Windows Desktop: `WindowsSecretsService` (Windows DPAPI)
  - Android: `AndroidSecretsService` (Android KeyStore)
- **FR-030**: System MUST enforce whitelist for Visual API Toolkit commands using dual-storage approach: (1) Human-readable documentation in `docs/VISUAL-WHITELIST.md` with citations and rationale (source of truth), (2) Runtime validation whitelist in `appsettings.json` under `Visual:AllowedCommands` array (maintained in sync with markdown). Whitelist MUST include only read-only commands (any commands that are NOT write operations). All Visual API calls MUST validate against runtime whitelist before execution. Write commands MUST be explicitly blocked.
- **FR-031**: System MUST require citation format for all toolkit commands: "Reference-{FileName} - {Chapter/Section/Page}".
- **FR-031a**: Windows Desktop MUST use direct Visual API Toolkit client connection with whitelist validation.
- **FR-031b**: Android MUST access Visual data via MTM Server API projections only. Server executes Visual API Toolkit commands server-side.

#### Configuration Persistence
- **FR-032**: System MUST persist user-scoped configuration values (each user has independent settings) across application restarts using dual-storage approach:
  - (1) Admin-configured central server path for user folders MUST be specified in `config/user-folders.json` configuration file with the following dynamic location detection logic:
    - **Home Development Detection**: Check public ISP IP address (via external service). If IP matches `HomeDevelopmentIPAddress` setting (default: `73.94.78.172`), use local path only.
    - **Network Drive Availability**: Attempt to access `NetworkDrivePath` with timeout (`NetworkAccessTimeoutSeconds`, default: 2s).
    - **Fallback Strategy**: If network drive unavailable, use `LocalFallbackPath` (default: `{MyDocuments}\\MTM_Apps\\users`).
    - **Dual Write Mode**: If `EnableDualWrite` is true, write to both network and local paths.
    - **Cache Duration**: Cache location decision for `LocationCacheDurationMinutes` (default: 5 minutes).
  - (2) Database schema configuration file for user preferences MUST be located in `config/database-schema.json` and define table structures, column types, constraints, and relationships for user preferences storage.
- **FR-033**: System MUST NOT persist environment variable overrides (they remain environment-specific).
- **FR-034**: System MUST persist credentials in OS-native secure storage (not configuration files).

#### Error Handling and Validation
- **FR-035**: System MUST provide safe fallback to default values when configuration keys are missing or invalid.
- **FR-036**: System MUST log configuration errors with structured logging (Serilog) including key, attempted value, and error reason.
- **FR-037**: System MUST validate configuration types at runtime. When type validation fails, system MUST use severity-based notification: (1) Non-critical settings (display preferences, timeouts, filters) - Use default value, log warning, show status bar indicator with details on click; (2) Critical settings (credentials, database connection, required config) - Show modal dialog blocking user interaction until setting is corrected.
- **FR-038**: System MUST handle database schema migration failures with severity-based approach: (1) Non-vital data (user preferences, UI settings) - Continue with in-memory storage only, log warning, show status indicator that preferences won't persist until schema fixed; (2) Vital configuration data - Block app startup, show error dialog with admin instructions for manual schema correction.
- **FR-039**: Android platform MUST support offline mode when network connectivity is lost: (1) Allow viewing cached data and local operations, (2) Disable operations requiring server (config updates, credential refresh, Visual ERP sync), (3) Show "Offline Mode" indicator in status bar with reconnect button, (4) Automatically reconnect and sync when network becomes available.
- **FR-040**: System MUST provide interactive recovery for corrupted configuration files: Show dialog with options "Restore defaults", "Try to repair", "Contact support". If repair attempted and fails, disable "Repair" button, show failure explanation, keep "Restore defaults" and "Contact support" options available. Log corruption details for diagnostics.

### Non-Functional Requirements

#### Compliance
- No specific compliance or regulatory constraints required. Application is not health-related and is based in the USA.

#### Data Volume & Scale
- System is designed for up to 1,000 users and moderate data volume. Administrators can review and remove inactive users and their associated data to manage scale.

#### Accessibility & Localization
- Application MUST support both accessibility (screen readers, keyboard navigation) and localization (translations for all user-facing text). Localization service is already implemented.

#### Performance
- **NFR-001**: Configuration value retrieval MUST complete in <10ms for cached values (cached = in-memory Dictionary<string, object?> lookups).
- **NFR-002**: Credential retrieval from OS-native storage MUST complete in <100ms.
- **NFR-003**: Feature flag evaluation MUST complete in <5ms.
- **NFR-004**: Configuration change events MUST be dispatched to subscribers within 50ms of value update.

#### Security
- **NFR-005**: Credentials MUST be stored using hardware-backed encryption when available (Windows TPM, Android hardware-backed KeyStore).
- **NFR-006**: Configuration logging MUST redact sensitive keys (password, token, secret, credential).
- **NFR-007**: OS-native credential storage MUST be isolated per user account (Windows) or per application (Android).

#### Reliability
- **NFR-008**: Configuration system MUST be thread-safe for concurrent reads and writes.
- **NFR-009**: Configuration system MUST not crash application if individual configuration values are corrupt or missing.
- **NFR-010**: When OS-native secure storage is completely unavailable, system MUST display user-friendly error dialog (avoiding technical jargon) and prompt user to enter required credentials. System MUST continue operation with user-provided credentials for current session and attempt to re-save when storage becomes available.

#### Maintainability
- **NFR-011**: All configuration services MUST be registered via dependency injection (ServiceCollectionExtensions).
- **NFR-012**: Configuration keys MUST follow hierarchical namespace convention (e.g., API:BaseUrl, Visual:Username).
- **NFR-013**: Feature flags MUST be centrally registered and documented in code.

#### Testing
- **NFR-014**: Configuration logic MUST be covered by unit tests (>80% coverage on ConfigurationService).
- **NFR-015**: Platform-specific secrets implementations MUST be covered by integration tests (Windows DPAPI and Android KeyStore).
- **NFR-016**: Feature flag evaluation logic MUST be covered by unit tests including rollout percentage scenarios.
- **NFR-017**: Credential storage retry policy MUST use exponential backoff (5s, 15s, 30s) with maximum 3 attempts when OS-native storage becomes temporarily unavailable. After 3 failed attempts, user must manually trigger retry via credential dialog.

### Key Entities

- **ConfigurationService**: Manages layered configuration with event-driven change notifications. Provides GetValue<T> and SetValue methods for reading and writing configuration values. Implements thread-safe access with lock statements.

- **SecretsService (Interface)**: Defines contract for secure credential storage with SetSecretAsync and GetSecretAsync methods. Platform-agnostic interface implemented by platform-specific services.

- **WindowsSecretsService**: Windows Desktop implementation using DPAPI (Data Protection API) for secure credential storage in Windows Credential Manager.

- **AndroidSecretsService**: Android implementation using Android KeyStore for secure credential storage (hardware-backed when available).

- **SecretsServiceFactory**: Factory class that creates appropriate SecretsService implementation based on current platform (Windows or Android). Throws PlatformNotSupportedException for unsupported platforms.

- **FeatureFlagEvaluator**: Manages feature flag registration, evaluation, and runtime updates. Supports environment-specific flags and rollout percentages. Provides IsEnabledAsync and SetEnabledAsync methods.

- **FeatureFlag**: Data structure representing a feature flag with properties: Name (string), IsEnabled (bool), Environment (string), RolloutPercentage (int 0-100).

- **ConfigurationChangedEventArgs**: Event data for configuration change notifications containing Key (string), OldValue (object?), and NewValue (object?).

- **Environment Variables**: OS-level configuration values with highest precedence. Keys use underscore format (MTM_*, DOTNET_*, ASPNETCORE_*).

- **User Configuration**: User-scoped configuration values (each user has independent settings) persisted across sessions using dual-storage: (1) User folder locations in admin-configured central server location specified in `config/user-folders.json` (defaults to MyDocuments), (2) User preference values in MAMP MySQL 5.7 database with schema loaded from JSON configuration file. User-scoped architecture eliminates concurrent update conflicts.

- **DatabaseSchemaConfiguration**: JSON configuration file defining database schema for user preferences storage. Contains table definitions, column types, constraints, and relationships. Editable by developer before MySQL server setup. File location: `config/database-schema.json`.

- **Application Defaults**: Hard-coded fallback values in code when no environment variable or user configuration exists.

---

### Database Schema Changes

**IMPORTANT**: This feature modifies the MAMP MySQL 5.7 database. All changes MUST be documented in `.github/mamp-database/` JSON files per Constitution v1.3.0 Principle VIII.

**Tables Modified/Created**:

1. **Users** (if not exists):
   - Purpose: Store user account information for application access
   - Columns:
     - `UserId` (INT, PRIMARY KEY, AUTO_INCREMENT): Unique user identifier
     - `Username` (VARCHAR(100), NOT NULL, UNIQUE): User login name
     - `Email` (VARCHAR(255), NULLABLE): User email for notifications
     - `IsActive` (BOOLEAN, NOT NULL, DEFAULT TRUE): User account status
     - `CreatedDate` (DATETIME, NOT NULL): Account creation timestamp
     - `LastLoginDate` (DATETIME, NULLABLE): Last successful login
   - Indexes: INDEX on `Username`, INDEX on `Email`, INDEX on `IsActive`
   - Reason: FR-032 requires user-scoped configuration architecture

2. **UserPreferences**:
   - Purpose: Store user-specific application settings (display preferences, filters, UI customization)
   - Columns:
     - `PreferenceId` (INT, PRIMARY KEY, AUTO_INCREMENT): Unique preference identifier
     - `UserId` (INT, NOT NULL, FOREIGN KEY): Reference to Users.UserId
     - `PreferenceKey` (VARCHAR(100), NOT NULL): Configuration key (e.g., "Theme", "DefaultView")
     - `PreferenceValue` (TEXT, NULLABLE): Configuration value (JSON or string)
     - `LastUpdated` (DATETIME, NOT NULL): Last modification timestamp
   - Indexes: INDEX on `UserId`, INDEX on `PreferenceKey`, UNIQUE INDEX on (`UserId`, `PreferenceKey`)
   - Foreign Keys: `UserId` → `Users.UserId` (ON DELETE CASCADE)
   - Reason: FR-032 requires persistent user preferences in MAMP MySQL database

3. **FeatureFlags**:
   - Purpose: Store feature flag configurations with environment-specific settings
   - Columns:
     - `FlagId` (INT, PRIMARY KEY, AUTO_INCREMENT): Unique flag identifier
     - `FlagName` (VARCHAR(100), NOT NULL, UNIQUE): Feature flag name (e.g., "Visual.UseForItems")
     - `IsEnabled` (BOOLEAN, NOT NULL, DEFAULT FALSE): Global enable/disable state
     - `Environment` (VARCHAR(50), NOT NULL): Target environment (Development, Staging, Production)
     - `RolloutPercentage` (INT, NOT NULL, DEFAULT 0): Percentage rollout (0-100)
     - `Description` (TEXT, NULLABLE): Human-readable flag purpose
     - `CreatedDate` (DATETIME, NOT NULL): Flag creation timestamp
     - `LastModified` (DATETIME, NOT NULL): Last modification timestamp
   - Indexes: INDEX on `FlagName`, INDEX on `Environment`, INDEX on `IsEnabled`
   - Reason: FR-017, FR-018, FR-019 require persistent feature flag storage

**Schema Documentation Requirements**:
- [x] Read `.github/mamp-database/schema-tables.json` before implementation
- [x] Exact table names documented (case-sensitive: `Users`, `UserPreferences`, `FeatureFlags`)
- [x] Exact column names documented (case-sensitive: `UserId`, `PreferenceKey`, `FlagName`)
- [x] Data types specified (VARCHAR lengths, INT, DATETIME, BOOLEAN, TEXT)
- [x] Nullable vs NOT NULL constraints documented
- [x] Default values documented where applicable
- [x] Indexes documented for query performance
- [x] Foreign key relationships documented
- [x] Must update `.github/mamp-database/schema-tables.json` during implementation
- [x] Must increment version in `.github/mamp-database/migrations-history.json`

**Migration Notes**:
- Tables created if not exist (safe for incremental deployments)
- Foreign key constraints enforce referential integrity
- CASCADE delete on UserPreferences when user deleted
- Indexes optimize query performance for configuration lookups

---

## Review & Acceptance Checklist

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified
- [x] Database schema changes documented in detail
- [x] JSON files in `.github/mamp-database/` will be updated during implementation

---

## Execution Status

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities resolved (all 28 clarifications complete)
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [x] Review checklist passed
