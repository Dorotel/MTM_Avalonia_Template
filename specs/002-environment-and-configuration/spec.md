# Feature Specification: Environment and Configuration Management System

**Feature Branch**: `002-environment-and-configuration`
**Created**: 2025-10-05
**Status**: Draft - Partially Implemented (Boot Feature 001)
**Input**: User description: "Environment and Configuration Management System"
**Related Document**: [Non-Technical Overview](overview.md)

## Execution Flow (main)

```
1. Parse user description from Input
   → Feature: Environment and Configuration Management System
2. Extract key concepts from description
   → Actors: Developers, System Administrators, End Users, AI Agents
   → Actions: Configure environments, manage secrets, toggle features, read/write config
   → Data: Environment variables, user settings, application defaults, credentials
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

### Session 2025-10-05

- Q: When credentials cannot be retrieved from OS-native storage (corrupted keychain, revoked permissions), what should the system do? → A: Prompt user for re-entry - Show credential dialog, attempt to re-save to OS storage
- Q: When checking if a feature is enabled but the feature's settings haven't been configured yet, what should happen? → A: Keep feature turned off - Treat unconfigured features as disabled until enabled, record note in logs
- Q: When reading a configuration setting from an environment variable but the value doesn't match what's expected (e.g., "ABC" instead of a number), what should happen? → A: Combination of use default value with log warning AND show dialog to user for correction - Application continues with safe defaults while alerting user to the configuration problem
- Q: When OS-native secure storage is completely unavailable (not just specific credentials, but entire storage system), what should the system do? → A: Prompt user for required credentials with clear explanation - Show user-friendly dialog explaining that secure storage is unavailable and request re-entry of needed credentials (e.g., "We couldn't access your saved login information. Please enter your Visual ERP username and password to continue.")
- Q: When a user changes a setting at runtime (display preferences, default filters, UI customization), where should those personal settings be saved? → A: Combination of admin-configured central server location AND database storage - User folder locations stored in admin-configured central server location [PLACEHOLDER: Path to be determined], user preferences stored in MAMP MySQL 5.7 database [PLACEHOLDER: Schema/table structure to be determined]
- Q: Where should user preferences (like display settings, sort orders, default filters) be saved in the database? → A: JSON configuration file with placeholders - Create a JSON configuration file defining database schema (table structures, column types, constraints, relationships) with placeholder values that developer will replace later before MySQL setup.
- Q: Should the Android version connect directly to the MySQL database or use an intermediary server? → A: Direct MySQL connection - Android app connects directly to MAMP MySQL database (same as Windows Desktop). Application will not be public-facing, so no need for connection pool limits or intermediary API server.
- Q: Where should the list of allowed Visual API Toolkit commands be stored, and what commands should be included? → A: Configuration file with read-only commands - Store whitelist in application configuration file (appsettings.json or similar). Include any Visual API Toolkit commands that are NOT write commands (read-only operations only).
- Q: When showing the user a configuration error notification, what should the message look like and how should it appear? → A: Severity-dependent approach - For non-critical settings (display preferences, timeouts, filters): Show status bar indicator with warning icon that displays details on click (non-intrusive). For critical settings (user credentials, database connection, required configuration): Show modal dialog blocking user interaction until setting is corrected (forces resolution before continuing).
- Q: When feature flags are changed on the server, how should running user applications respond? → A: Next launch only - Feature flag changes on server are applied only when user's MTM_Application_Launcher detects version mismatch and updates their local copy. This ties feature flags to version deployments for predictable, stable experience and leverages existing launcher infrastructure.

### Session 2025-10-05 (Additional Clarifications)
- Q: Where should admin-configured central server path for user folders be specified? → A: Use JSON config file with placeholder; default to MyDocuments for now
- Q: Where should the database schema configuration file be located? → A: root/config folder
- Q: What accessibility and localization support is required? → A: Both accessibility and localization; localization service already implemented
- Q: What are the data volume/scale assumptions? → A: Up to 1,000 users, moderate data volume; allow admin to remove inactive users and their data
- Q: Are there compliance or regulatory constraints? → A: No specific compliance required; not health-related, based in USA

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers

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
  → [NEEDS CLARIFICATION: Is this a configuration concern or a service resilience concern? Should config system validate connectivity?]

- **What happens when OS-native secure storage is completely unavailable?**
  → System displays user-friendly dialog explaining the storage issue in plain language (e.g., "We couldn't access your saved login information") and prompts user to enter required credentials. Application continues with user-provided credentials for the session. System attempts to re-save credentials when storage becomes available.

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
- **FR-011**: System MUST store credentials using OS-native secure storage mechanisms (Windows DPAPI for Windows Desktop, Android KeyStore for Android).
- **FR-012**: System MUST provide SecretsService.SetSecretAsync(key, value) for storing credentials securely.
- **FR-013**: System MUST provide SecretsService.GetSecretAsync(key) for retrieving stored credentials.
- **FR-013a**: When credential retrieval fails (corrupted storage, revoked permissions), system MUST display credential entry dialog to allow user re-entry and attempt to re-save to OS-native storage.
- **FR-014**: System MUST NEVER log credentials, passwords, tokens, or other sensitive data in plaintext.
- **FR-015**: System MUST throw PlatformNotSupportedException when attempting to use SecretsService on unsupported platforms (macOS, Linux, iOS).
- **FR-016**: System MUST support storing Visual ERP username and password for authentication.

#### Feature Flag Management
- **FR-017**: System MUST provide FeatureFlagEvaluator for registering and evaluating feature flags.
- **FR-018**: System MUST support the following core feature flags: Visual.UseForItems, Visual.UseForLocations, Visual.UseForWorkCenters, OfflineModeAllowed, Printing.Enabled.
- **FR-019**: System MUST support environment-specific feature flag configuration (flags can be enabled in Development but disabled in Production).
- **FR-020**: System MUST support rollout percentage (0-100%) for gradual feature rollouts.
- **FR-021**: System MUST provide FeatureFlagEvaluator.IsEnabledAsync(flagName) for checking if a feature is enabled.
- **FR-021a**: When a feature flag is not registered, system MUST treat it as disabled and log a warning message. This safe-default behavior prevents unexpected feature behavior while allowing normal application operation.
- **FR-022**: System MUST provide FeatureFlagEvaluator.SetEnabledAsync(flagName, enabled) for runtime flag updates.
- **FR-022a**: Feature flag changes made on server MUST only take effect in user applications after MTM_Application_Launcher detects version mismatch and updates local copy. Running applications MUST NOT synchronize feature flags in real-time from server.
- **FR-023**: Feature flag evaluation MUST be deterministic per user (same user always sees same flag state for a given rollout percentage).

#### Platform Support
- **FR-024**: System MUST support Windows Desktop platform with direct MySQL connection.
- **FR-025**: System MUST support Android platform with direct MySQL connection (same architecture as Windows Desktop, no connection pool limits required for non-public application).
- **FR-026**: System MUST explicitly reject unsupported platforms (macOS, Linux, iOS) with clear error messages.
- **FR-027**: System MUST use platform-specific services via factory pattern (SecretsServiceFactory creates platform-appropriate implementations).

#### Visual ERP Integration Configuration
- **FR-028**: System MUST enforce read-only access to Visual ERP data (architectural principle, not runtime flag).
- **FR-029**: System MUST store Visual ERP credentials using SecretsService (not plaintext configuration).
- **FR-030**: System MUST enforce whitelist for Visual API Toolkit commands stored in application configuration file. Whitelist MUST include only read-only commands (any commands that are NOT write operations). Write commands MUST be explicitly blocked to maintain read-only access principle.
- **FR-031**: System MUST require citation format for all toolkit commands: "Reference-{File Name} - {Chapter/Section/Page}".

#### Configuration Persistence
- **FR-032**: System MUST persist user-set configuration values across application restarts using dual-storage approach: (1) Admin-configured central server path for user folders MUST be specified in a JSON configuration file (with placeholder value); default location is user's MyDocuments folder until finalized. (2) Database schema configuration file for user preferences MUST be located in the root/config folder and define table structures, column types, constraints, and relationships for user preferences storage.
- **FR-033**: System MUST NOT persist environment variable overrides (they remain environment-specific).
- **FR-034**: System MUST persist credentials in OS-native secure storage (not configuration files).

#### Error Handling and Validation
- **FR-035**: System MUST provide safe fallback to default values when configuration keys are missing or invalid.
- **FR-036**: System MUST log configuration errors with structured logging (Serilog) including key, attempted value, and error reason.
- **FR-037**: System MUST validate configuration types at runtime. When type validation fails, system MUST use severity-based notification: (1) Non-critical settings (display preferences, timeouts, filters) - Use default value, log warning, show status bar indicator with details on click; (2) Critical settings (credentials, database connection, required config) - Show modal dialog blocking user interaction until setting is corrected.

### Non-Functional Requirements
#### Compliance
No specific compliance or regulatory constraints required. Application is not health-related and is based in the USA.
#### Data Volume & Scale
System is designed for up to 1,000 users and moderate data volume. Administrators can review and remove inactive users and their associated data to manage scale.
#### Accessibility & Localization
Application MUST support both accessibility (screen readers, keyboard navigation) and localization (translations for all user-facing text). Localization service is already implemented.

#### Performance
- **NFR-001**: Configuration value retrieval MUST complete in <10ms for cached values.
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

- **User Configuration**: Runtime-set configuration values persisted across sessions using dual-storage: (1) User folder locations in admin-configured central server location [PLACEHOLDER: Path specification], (2) User preference values in MAMP MySQL 5.7 database with schema loaded from JSON configuration file.

- **DatabaseSchemaConfiguration**: JSON configuration file defining database schema for user preferences storage. Contains table definitions, column types, constraints, and relationships. Editable by developer before MySQL server setup. File location: [PLACEHOLDER: To be determined during planning].

- **Application Defaults**: Hard-coded fallback values in code when no environment variable or user configuration exists.

---

## Review & Acceptance Checklist

### Content Quality
- [x] No implementation details (languages, frameworks, APIs) - Implementation status noted as context, not requirements
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders (with technical context where necessary)
- [x] All mandatory sections completed

### Requirement Completeness
- [x] No [NEEDS CLARIFICATION] markers remain - **9 of 10 clarification items resolved, 1 deferred**:
  1. ✅ Behavior when credentials cannot be retrieved from OS-native storage → RESOLVED
  2. ✅ Behavior when feature flag doesn't exist in registry → RESOLVED
  3. ✅ Type coercion strategy for invalid environment variable data types → RESOLVED
  4. ✅ Graceful degradation strategy when OS-native storage unavailable → RESOLVED
  5. ✅ User settings persistence location (user profile vs app data folder) → RESOLVED
  6. ⏸️ Configuration system responsibility for API connectivity validation → DEFERRED (plan phase)
  7. ✅ Android platform: Direct MySQL vs API server access strategy → RESOLVED (direct MySQL, no connection pool limits)
  8. ✅ Visual API Toolkit command whitelist storage and contents → RESOLVED (config file, read-only commands only)
  9. ✅ Feature flag default behavior when not registered → RESOLVED (duplicate of #2)
  10. ✅ Database schema/table structure for user preferences → RESOLVED (JSON-based schema configuration)
  11. ✅ Feature flag runtime synchronization with launcher deployment → RESOLVED (next launch only, tied to version updates)
  12. [PLACEHOLDER] Admin-configured central server path for user folders → TO BE DETERMINED
- [x] Requirements are testable and unambiguous (5 high-impact clarifications resolved)
- [x] Success criteria are measurable (NFR performance targets defined)
- [x] Scope is clearly bounded (Windows Desktop + Android only)
- [x] Dependencies and assumptions identified (Boot Feature 001 partial implementation noted)

---

## Execution Status

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked (10 clarification items)
- [x] User scenarios defined
- [x] Requirements generated (37 functional, 16 non-functional)
- [x] Entities identified (8 key entities)
- [x] Review checklist passed (with documented clarifications)

---

## Dependencies and Assumptions

### Dependencies
- **Boot Feature 001**: Core configuration infrastructure already implemented (ConfigurationService, SecretsService, FeatureFlagEvaluator)
- **MTM_Application_Launcher**: WinForms launcher application (MTM_Application_Launcher_Winforms repository) that checks version mismatches between server and local copies, prompts users to update before launching
- **MAMP MySQL**: Local MySQL 5.7 instance for Windows Desktop development
- **Visual ERP API Toolkit**: External API for read-only access to ERP data (whitelist enforcement implemented)
- **OS-Native Security APIs**: Windows DPAPI and Android KeyStore availability

### Assumptions
- Windows Desktop users have MAMP MySQL installed and running on default port 3306
- Android users will have network connectivity for API access (when API server implemented)
- Visual ERP API Toolkit credentials are provided by system administrator
- OS-native credential storage (DPAPI/KeyStore) is available and functional on target platforms
- Configuration changes at runtime are infrequent (not high-throughput scenario)
- Feature flag rollout percentages are set by developers/administrators, not end users

### Out of Scope (Future Enhancements)
- File-based configuration (appsettings.{Environment}.json loading) - Not implemented
- Remote configuration server support - Not implemented
- Configuration schema validation - Not implemented
- Visual ERP master data polling and cache refresh - Not implemented
- Configuration profiles for multi-plant deployments - Not implemented
- Configuration audit logging with timestamps - Not implemented
- Encrypted configuration sections - Not implemented (only credentials use OS encryption)
- Hot-reload for feature flags from remote config - Not implemented

---

## Success Criteria

### Measurable Outcomes
1. **Configuration retrieval performance**: <10ms for cached values (NFR-001)
2. **Credential retrieval performance**: <100ms from OS-native storage (NFR-002)
3. **Feature flag evaluation performance**: <5ms (NFR-003)
4. **Test coverage**: >80% for ConfigurationService (NFR-014)
5. **Zero plaintext credentials**: No passwords/tokens in logs or config files (FR-014, NFR-006)
6. **Platform support**: Windows Desktop and Android only, explicit rejection of unsupported platforms (FR-026)

### Qualitative Outcomes
1. Developers can configure environment-specific settings without code changes
2. System administrators can deploy to production with environment variables only (no code/config file modifications)
3. Credentials are stored securely using OS-native mechanisms (DPAPI/KeyStore)
4. Feature flags can be toggled at runtime without application restart
5. Configuration changes are observable via event notifications for reactive UI updates

---

**Last Updated**: 2025-10-05
**Platform Support**: Windows Desktop + Android only
**Implementation Status**: Partially Implemented (Boot Feature 001)
**Next Phase**: Planning (address 10 clarification items)
