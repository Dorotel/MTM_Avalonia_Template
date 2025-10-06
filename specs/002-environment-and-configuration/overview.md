# Feature Overview: Environment and Configuration Management System

**Document Type**: Non-Technical Business Specification
**Created**: 2025-10-05
**Status**: Draft - Partially Implemented
**For**: Business stakeholders, project managers, and non-technical team members
**Related Document**: [Technical Specification](spec.md)

---

## 📋 What This Document Is

This document describes **what** the Environment and Configuration Management System does and **why** it's valuable - written in plain language without technical jargon. Think of it as the "instruction manual" that explains how the application adapts to different situations and keeps sensitive information secure.

**Who should read this?**
- Business owners who need to understand how the system behaves in different environments
- Managers who need to explain deployment and security practices
- Anyone who needs to verify the system handles configuration correctly

**Who should NOT use this as their primary reference?**
- Developers (they have a separate technical document: spec.md)
- Testers (they have detailed test specifications)

---

## 🎯 The Big Picture

### What Problem Does This Solve?

Right now, applications need to behave differently depending on where they're running. A developer testing new features needs different settings than what's used when real customers use the application in production. Additionally, sensitive information like passwords must be stored securely, not written in plain text where anyone could find them. This system automatically detects where the application is running, applies the right settings, and stores secrets safely using your computer's or phone's built-in security features.

### Who Benefits?

**Developers**: Can work on new features without worrying about accidentally breaking production systems. They can toggle features on and off easily to test different scenarios.

**System Administrators**: Can deploy the application to production by simply setting environment variables, without touching code or configuration files. Security credentials are stored safely using the operating system's own protection.

**End Users**: Experience a seamless application that "just works" because it automatically uses the right settings for their environment.

**IT Security Teams**: Can verify that passwords and credentials are never stored in plain text - they're protected by Windows Data Protection (on desktops) or Android KeyStore (on phones).

---

## 📖 What Users Will Experience

### The Main User Journey

#### For Developers

**Story**: Maria is a developer working on a new feature for the MTM application. She opens the project on her development laptop.

1. **Automatic Environment Detection**: The application automatically detects it's running in "Development" mode (because Maria has DEBUG mode turned on in her development tools)
2. **Load Development Settings**: All the development-friendly settings load automatically - extra logging, test database connections, and experimental features enabled
3. **Feature Flag Toggle**: Maria wants to test a new feature that's not ready for customers yet. She can turn this feature on or off with a simple flag, without changing any code
4. **Secure Credentials**: When Maria needs to connect to the Visual ERP test system, she enters her username and password once. The application stores them securely using Windows Data Protection, so she doesn't have to type them again every time

**Result**: Maria can work productively without accidentally affecting production users or having to remember complex configuration settings.

#### For System Administrators

**Story**: James is deploying the latest version of the application to the production server.

1. **Set Environment Variables**: James sets a simple environment variable (MTM_ENVIRONMENT=Production) on the production server
2. **No Code Changes**: He doesn't need to edit any files or modify the application code
3. **Automatic Configuration**: When the application starts, it automatically detects "Production" environment and applies production-appropriate settings
4. **Secure Credential Storage**: Production database passwords are stored using Windows Credential Manager (not in text files where they could be exposed)

**Result**: James can deploy confidently, knowing the right settings will apply automatically and sensitive information is protected.

#### For End Users

**Story**: Sarah launches the MTM application on her work computer.

1. **Seamless Start**: The application starts normally - Sarah doesn't see any configuration screens or technical questions
2. **Right Features Enabled**: The application automatically has the right features turned on for her organization
3. **Secure Login**: When Sarah enters her Visual ERP credentials, they're stored securely so she doesn't have to type them repeatedly

**Result**: Sarah experiences a professional application that doesn't bother her with technical details.

---

## ✅ What This Feature Must Do

Below is a list of specific things the feature **must** be able to do. Each item is numbered so we can refer to it easily in discussions.

### Core Capabilities

#### Environment Detection (Requirements 1-3)

**Requirement 1**: The system must automatically figure out if it's running in Development, Staging, or Production mode
- *In plain English*: The application is smart enough to know "where it lives" - is this a developer's laptop, a testing server, or the real production server?
- *Example scenario*: Maria opens the app on her laptop → System sees DEBUG mode is on → Automatically uses Development settings. James deploys to production server → System sees MTM_ENVIRONMENT=Production → Automatically uses Production settings.

**Requirement 2**: The system must support three distinct worlds: Development, Staging, and Production
- *In plain English*: Like having three separate playgrounds - one for building and testing (Development), one for final testing (Staging), and one for real customers (Production)
- *Example scenario*: A new feature can be enabled in Development for testing, partially rolled out in Staging to a few test users, and fully released in Production when ready.

**Requirement 3**: Administrators must be able to override the automatic detection
- *In plain English*: Even if the system guesses wrong about where it's running, an administrator can tell it explicitly
- *Example scenario*: James wants to test production settings on a staging server → He sets MTM_ENVIRONMENT=Production → System behaves like production

#### Configuration Management (Requirements 4-10)

**Requirement 4**: The system must use a clear priority order for settings: Environment Variables beat User Settings beat Default Values
- *In plain English*: Like a cascade of rules - the most important rules (environment variables) always win over less important ones (defaults in the code)
- *Example scenario*: The default timeout is 30 seconds. Maria sets it to 60 seconds in her user settings. James sets MTM_API_TIMEOUT=90 as an environment variable on the server. Result: The application uses 90 seconds (environment variable wins).

**Requirement 5**: The system must provide a safe way to read settings with fallback values
- *In plain English*: When the application asks for a setting, it provides a sensible default in case that setting doesn't exist
- *Example scenario*: App asks for "PrinterName" setting. If it doesn't exist, use "Default Printer" as the fallback. The application never crashes because a setting is missing.

**Requirement 6**: Users must be able to change settings while the application is running
- *In plain English*: You can adjust settings without closing and reopening the application
- *Example scenario*: Maria wants to change the API timeout from 30 to 60 seconds → She changes it in settings → The application immediately starts using 60 seconds for the next API call

**Requirement 7**: When settings change, the system must notify all parts of the application that care about that change
- *In plain English*: Like a loudspeaker announcement - when a setting changes, everyone who needs to know hears about it immediately
- *Example scenario*: Maria changes the "Theme" setting from Light to Dark → The configuration system broadcasts this change → All UI components receive the notification and update their appearance

**Requirement 8**: The system must handle multiple people or processes changing settings at the same time without conflicts
- *In plain English*: Even if two things try to change settings at exactly the same moment, the system doesn't get confused or crash
- *Example scenario*: Background process tries to save a log level setting while user is changing a timeout setting → Both changes complete successfully without interfering

**Requirement 9**: Environment variable names must use underscores (like MTM_ENVIRONMENT, not MTM.ENVIRONMENT)
- *In plain English*: A formatting rule that makes environment variables work correctly on all operating systems
- *Example scenario*: James sets MTM_API_TIMEOUT=60 (with underscore) → Works correctly on Windows and Android

**Requirement 10**: Reading a non-existent setting must never crash the application
- *In plain English*: If you ask for a setting that doesn't exist, the system just returns the default value you provided - it doesn't throw an error
- *Example scenario*: App asks for "NewFeatureTimeout" setting that doesn't exist yet → System returns the default value of 30 seconds → Application continues working normally

#### Secure Credentials Storage (Requirements 11-16)

**Requirement 11**: Passwords and credentials must be stored using the operating system's built-in secure storage
- *In plain English*: Like putting your valuables in a bank vault instead of under your mattress - the OS has professional-grade security for passwords
- *Example scenario*: On Windows, passwords stored in Windows Credential Manager (protected by Data Protection API). On Android, stored in Android KeyStore (hardware-protected when available).

**Requirement 12**: The system must provide a secure way to save credentials
- *In plain English*: There's a special "safe deposit box" function for storing sensitive information
- *Example scenario*: Maria calls SecretsService.SetSecretAsync("Visual.Password", "MyP@ssw0rd") → Password is encrypted and stored in Windows Credential Manager

**Requirement 13**: The system must provide a secure way to retrieve credentials
- *In plain English*: There's a special "retrieve from safe deposit box" function for getting sensitive information back out
- *Example scenario*: Application needs Visual ERP password → Calls SecretsService.GetSecretAsync("Visual.Password") → Gets decrypted password from Windows Credential Manager

**Requirement 14**: The system must NEVER write passwords, tokens, or secrets to log files in plain text
- *In plain English*: Like a doctor-patient confidentiality rule - sensitive information never appears in logs where someone could accidentally see it
- *Example scenario*: Application logs "Attempting to connect to Visual ERP as user 'maria'" (OK) but NEVER logs "Attempting to connect with password 'MyP@ssw0rd'" (NOT OK)

**Requirement 15**: The system must refuse to run on unsupported operating systems with a clear error message
- *In plain English*: This system only works on Windows and Android. If you try to run it on macOS or Linux, it tells you clearly why it won't work
- *Example scenario*: Developer tries to run on macOS → Gets error: "PlatformNotSupportedException: Only Windows and Android are supported"

**Requirement 16**: The system must be able to store Visual ERP username and password
- *In plain English*: Specifically designed to securely store the credentials needed to connect to the Visual ERP system
- *Example scenario*: First-time setup → User enters Visual username and password → Stored in OS-native secure storage → Never asked again

#### Feature Flag Management (Requirements 17-23)

**Requirement 17**: The system must provide a way to turn features on and off with flags
- *In plain English*: Like light switches for features - you can turn them on or off without changing code
- *Example scenario*: New printing feature exists in code but is turned "off" by default → Administrator flips the "Printing.Enabled" flag to "on" → Feature becomes available to users

**Requirement 18**: The system must support specific core feature flags
- *In plain English*: There are five main feature switches built into the system: Visual data sources (Items, Locations, WorkCenters), Offline Mode, and Printing
- *Example scenario*: Visual.UseForItems flag controls whether item/part data comes from Visual ERP or local database

**Requirement 19**: Feature flags must be able to have different values in different environments
- *In plain English*: A feature can be "on" in Development for testing, but "off" in Production until it's ready for customers
- *Example scenario*: New barcode feature → Enabled in Development (for Maria to test) → Disabled in Production (customers don't see it yet)

**Requirement 20**: Feature flags must support gradual rollouts with percentage control
- *In plain English*: You can turn a feature "on" for only 25% of users, then 50%, then 100% as you gain confidence
- *Example scenario*: New reporting feature → Start at 10% rollout (only 1 in 10 users see it) → Monitor for problems → Increase to 100% rollout when stable

**Requirement 21**: The system must provide a way to check if a feature is enabled
- *In plain English*: Before showing a feature to a user, the code can ask "Is this feature turned on?"
- *Example scenario*: Before showing Print button → Code calls IsEnabledAsync("Printing.Enabled") → Returns true → Show button. Returns false → Hide button.

**Requirement 22**: Administrators must be able to turn flags on/off while the application is running
- *In plain English*: No need to restart the application to enable or disable features
- *Example scenario*: Production issue detected with new feature → Administrator calls SetEnabledAsync("NewFeature", false) → Feature immediately disabled for all users → Issue contained

**Requirement 23**: Each user must consistently see the same flag state for gradual rollouts
- *In plain English*: If a feature is enabled for you at 50% rollout, it stays enabled for you - it doesn't randomly turn on and off each time you use the app
- *Example scenario*: Rollout at 50% → Sarah gets the feature (she's in the "on" group) → Every time Sarah uses the app, she sees the feature → Bob doesn't get it (he's in the "off" group) → Bob never sees it randomly appear

#### Platform Support (Requirements 24-27)

**Requirement 24**: The system must work on Windows Desktop with efficient database connections
- *In plain English*: When running on a Windows PC, the system can directly connect to the MySQL database with 2-10 simultaneous connections
- *Example scenario*: Desktop application running on office PC → Uses direct MySQL connection with pool of 5 connections → Fast database access

**Requirement 25**: The system must work on Android phones with resource-aware database connections
- *In plain English*: Android phones have less memory and power, so the system limits database connections to 1-5 to avoid draining the battery
- *Example scenario*: App running on Android phone → Uses limited connection pool of 2 connections → Conserves battery and memory
- *Note*: [CLARIFICATION NEEDED - HUMAN EXPLANATION] Will Android eventually use an API server instead of direct database connections? This could affect how we explain performance expectations.

**Requirement 26**: The system must clearly refuse to run on unsupported platforms
- *In plain English*: If someone tries to run this on a Mac or Linux computer, they get a helpful error message explaining it only works on Windows and Android
- *Example scenario*: Developer tries to run on macOS → Clear error: "This application only runs on Windows Desktop and Android"

**Requirement 27**: The system must automatically use the right security approach for each platform
- *In plain English*: The system is smart enough to use Windows security features on Windows, and Android security features on Android
- *Example scenario*: Same code running on Windows → Uses Windows Credential Manager. Same code on Android → Uses Android KeyStore. User doesn't see any difference.

#### Visual ERP Integration Configuration (Requirements 28-31)

**Requirement 28**: The system must enforce that Visual ERP data can only be read, never written
- *In plain English*: This is a safety rule - the application can look at Visual ERP data but can never change it
- *Example scenario*: Application reads part numbers from Visual ERP → Works fine. Application tries to update a part number in Visual ERP → System blocks the attempt (architectural principle)

**Requirement 29**: Visual ERP credentials must be stored in the secure secrets system
- *In plain English*: Visual ERP username and password are too important to store in regular configuration - they go in the secure vault
- *Example scenario*: Visual username stored via SecretsService → Protected by Windows DPAPI → Never visible in configuration files or logs

**Requirement 30**: The system must enforce a whitelist of allowed Visual API commands
- *In plain English*: Only pre-approved commands can be sent to Visual ERP - like having a list of allowed questions you can ask
- *Example scenario*: Application tries to run "GetPartInfo" command → Check whitelist → Allowed → Execute. Tries to run "DeletePart" → Not on whitelist → Blocked.
- *Note*: [CLARIFICATION NEEDED - HUMAN EXPLANATION] What specific commands are on the approved list? How does an administrator modify this list?

**Requirement 31**: All Visual API commands must include citation references
- *In plain English*: Every command sent to Visual must document where it came from, like citing sources in a research paper
- *Example scenario*: GetPartInfo command must include citation: "Reference-Visual API Manual - Chapter 3/Page 45"

#### Configuration Persistence (Requirements 32-34)

**Requirement 32**: User-chosen settings must survive application restarts
- *In plain English*: When you change a setting and close the application, your change is remembered when you open it again
- *Example scenario*: Maria sets PrintTimeout=90 seconds → Closes application → Opens it tomorrow → PrintTimeout still 90 seconds
- *Note*: [CLARIFICATION NEEDED - HUMAN EXPLANATION] Where exactly are these settings saved? User's documents folder? Application data folder? This affects backup and profile roaming.

**Requirement 33**: Environment variable settings must NOT be saved by the application
- *In plain English*: Settings from environment variables are controlled by the system administrator, not saved by the app
- *Example scenario*: MTM_ENVIRONMENT=Production is set by James on the server → Application reads it but doesn't try to save or change it

**Requirement 34**: Credentials must be stored in the OS security vault, never in configuration files
- *In plain English*: Passwords go in the operating system's secure storage, not in text files where they could be accidentally copied or backed up
- *Example scenario*: Password stored in Windows Credential Manager (encrypted by OS) → NOT stored in config.json or settings.xml

#### Error Handling and Validation (Requirements 35-37)

**Requirement 35**: When a setting is missing or invalid, the system must use a sensible default
- *In plain English*: If a setting doesn't exist or is corrupted, the system doesn't crash - it just uses a reasonable fallback value
- *Example scenario*: PrinterTimeout setting is corrupted → System logs a warning → Uses default value of 30 seconds → Application continues working

**Requirement 36**: Configuration errors must be logged with full details for troubleshooting
- *In plain English*: When something goes wrong with configuration, the system writes a detailed log entry to help figure out what happened
- *Example scenario*: Attempt to set timeout to "ABC" (not a number) → Log: "Configuration error: Key=PrintTimeout, AttemptedValue=ABC, Error=Invalid number format"

**Requirement 37**: The system must validate setting types and log warnings for mismatches
- *In plain English*: If a setting should be a number but someone provides text, the system catches this and logs a warning
- *Example scenario*: MTM_API_TIMEOUT="sixty" (text instead of number) → System logs warning → Uses default value of 30
- *Note*: [CLARIFICATION NEEDED - HUMAN EXPLANATION] Should the system try to be smart and convert "60" (text) to 60 (number)? Or should it strictly require the right type?

---

## 🚫 What This Feature Will NOT Do

It's important to be clear about what's **not** included, so expectations are set correctly:

- ❌ **File-based configuration** (appsettings.json files): The system doesn't currently load settings from JSON files - it uses environment variables, user settings, and code defaults
- ❌ **Remote configuration server**: Settings are local to each machine/device - there's no central server to push configuration updates
- ❌ **Configuration schema validation**: The system doesn't have a formal schema that defines all possible settings and validates them
- ❌ **Visual ERP master data polling**: The system doesn't automatically check Visual ERP for data updates on a schedule
- ❌ **API server for Android**: Android currently connects directly to MySQL - there's no intermediate API server (planned for future)
- ❌ **Multi-plant configuration profiles**: The system doesn't have built-in support for managing settings for multiple factory locations
- ❌ **Configuration audit logging**: Changes to settings aren't tracked in a separate audit log with timestamps and user attribution
- ❌ **Encrypted configuration sections**: Only credentials are encrypted (via OS security) - regular settings are stored in plaintext
- ❌ **Hot-reload from remote source**: Feature flags can be changed at runtime, but not pulled from a remote configuration server

**Why these are excluded**: These are planned enhancements for future versions. The current implementation focuses on the core capabilities needed for secure, environment-aware configuration management.

---

## ❓ Questions That Need Answers

[If there are aspects that aren't fully decided yet, list them here with the questions that need answering]

**Question 1**: What should happen when credentials can't be retrieved from OS-native storage?
- *Why this matters*: If Windows Credential Manager or Android KeyStore fails, the application needs to handle this gracefully
- *Options*:
  - A) Prompt user to re-enter credentials immediately
  - B) Use a cached copy (less secure but more convenient)
  - C) Fail with error and require manual intervention
- **Status**: NEEDS RESOLUTION

**Question 2**: What should happen when code checks a feature flag that doesn't exist?
- *Why this matters*: Affects debugging and error handling - should missing flags be treated as "off" by default or as a programming error?
- *Options*:
  - A) Return false (feature disabled) - more forgiving
  - B) Throw exception (programming error) - catches mistakes earlier
  - C) Require explicit registration (middle ground)
- **Status**: NEEDS RESOLUTION

**Question 3**: Should the system try to convert text to numbers when reading settings?
- *Why this matters*: Environment variables are always text - should "123" (text) automatically become 123 (number)?
- *Options*:
  - A) Yes, try intelligent conversion (user-friendly)
  - B) No, strictly require correct types (safer)
  - C) Convert only for environment variables, strict for code (hybrid)
- **Status**: NEEDS RESOLUTION

**Question 4**: Is validating API connectivity part of configuration's responsibility?
- *Why this matters*: Should ConfigurationService check that the API endpoint is reachable, or is that a separate concern?
- *Options*:
  - A) Configuration only stores settings - connectivity is a separate service concern
  - B) Configuration validates connectivity and marks endpoints as "healthy" or "unreachable"
- **Status**: NEEDS RESOLUTION

**Question 5**: Will Android use direct MySQL or API server access?
- *Why this matters*: Affects architecture decisions, performance expectations, and security model
- *Options*:
  - A) Keep direct MySQL (current implementation)
  - B) Build API server for Android to use (planned but not implemented)
- **Status**: NEEDS RESOLUTION - Plan shows API server is intended

**Question 6**: What Visual API commands are whitelisted and how is the whitelist managed?
- *Why this matters*: Security - need to know exactly what operations are allowed
- *Missing information*:
  - List of allowed commands
  - Where whitelist is stored (database? config file? hardcoded?)
  - Who can modify the whitelist
- **Status**: NEEDS RESOLUTION

**Question 7**: Where are user settings files stored?
- *Why this matters*: Affects backup, profile roaming, and troubleshooting
- *Options*:
  - A) User profile directory (C:\Users\{username}\AppData\Roaming\MTM)
  - B) Application data folder (shared location)
  - C) Documents folder (user-visible)
- **Status**: NEEDS RESOLUTION

**Question 8**: How should the system degrade gracefully when OS-native credential storage fails?
- *Why this matters*: Windows Credential Manager or Android KeyStore could be unavailable or corrupted
- *Options*:
  - A) In-memory only (secure but lost on restart)
  - B) Prompt user immediately to re-enter
  - C) Fail completely and require manual resolution
- **Status**: NEEDS RESOLUTION

**Question 9**: What's the default behavior for unregistered feature flags?
- *Why this matters*: Consistency - should all flags require explicit registration?
- *Options*:
  - A) Unregistered flags return false (feature disabled)
  - B) Unregistered flags throw exception (require registration)
- **Status**: NEEDS RESOLUTION

**Question 10**: Type coercion strategy for configuration values - permissive or strict?
- *Why this matters*: User experience vs error detection - forgiving systems are easier to use but may hide problems
- *Options*:
  - A) Permissive: Try to convert "123" to 123, "true" to true, etc.
  - B) Strict: Require exact type match or use default
  - C) Hybrid: Permissive for environment variables, strict for code
- **Status**: NEEDS RESOLUTION

---

## 📊 How We'll Know It's Working

This section describes what "success" looks like for this feature.

### Measurable Goals

**Goal 1**: Configuration retrieval is fast
- *How to measure*: Time how long it takes to read a configuration value
- *Success criteria*: Less than 10 milliseconds (0.01 seconds) for values already in memory
- *Why this matters*: Configuration is used frequently - it must be instant

**Goal 2**: Credential retrieval is reasonably fast
- *How to measure*: Time how long it takes to get a password from Windows Credential Manager or Android KeyStore
- *Success criteria*: Less than 100 milliseconds (0.1 seconds)
- *Why this matters*: Users shouldn't notice delays when credentials are retrieved

**Goal 3**: Feature flag checks are instant
- *How to measure*: Time how long it takes to check if a feature is enabled
- *Success criteria*: Less than 5 milliseconds (0.005 seconds)
- *Why this matters*: Feature flags are checked frequently during normal operation

**Goal 4**: High test coverage for reliability
- *How to measure*: Code coverage percentage from automated tests
- *Success criteria*: Greater than 80% coverage for ConfigurationService
- *Why this matters*: Ensures the system is thoroughly tested and reliable

**Goal 5**: Zero plaintext credentials
- *How to measure*: Search all log files and configuration files for password patterns
- *Success criteria*: No passwords, tokens, or secrets found in plaintext
- *Why this matters*: Security - sensitive information must never be exposed

**Goal 6**: Explicit platform support
- *How to measure*: Attempt to run on Windows, Android, macOS, and Linux
- *Success criteria*: Works on Windows and Android, clear error message on macOS/Linux
- *Why this matters*: Users on unsupported platforms should get helpful guidance, not cryptic crashes

### User Satisfaction

- Developers can switch between Development, Staging, and Production environments without editing code
- System administrators can deploy to production by only setting environment variables
- Credentials are stored securely and retrieved transparently (users don't notice the security happening)
- Feature flags can be toggled without restarting the application
- Configuration changes are immediately reflected in the UI (via event notifications)

---

## 🗓️ Timeline and Dependencies

### What Needs to Happen First?

**Boot Feature 001 (Already Completed)**: The core configuration infrastructure is already built and working:
- ConfigurationService with layered precedence
- Thread-safe configuration access
- Event-driven change notifications
- SecretsService for Windows and Android
- FeatureFlagEvaluator with environment support
- MySQL connection pooling

**This means**: The foundation is solid - we're documenting what exists and identifying areas that need clarification or enhancement.

### Assumptions We're Making

- **MAMP MySQL**: Developers have MAMP MySQL installed and running on port 3306 for local development
- **Visual ERP Access**: Visual ERP API Toolkit credentials are provided by system administrator
- **OS Security Available**: Windows DPAPI and Android KeyStore are functional on target devices
- **Infrequent Changes**: Configuration doesn't change frequently (not a high-speed streaming scenario)
- **Admin-Controlled Flags**: Feature flag rollout percentages are set by administrators, not end users
- **Network Connectivity**: Android users have network access (when API server is implemented)

---

## 📞 Who to Contact

**Questions about business requirements**: Project Manager
**Questions about user experience**: UX Lead
**Questions about security and compliance**: IT Security Team
**Questions about technical implementation**: Lead Developer (see spec.md for technical details)
**Questions about deployment**: DevOps/System Administrator

---

## 📝 Document History

| Date | Change | Who |
|------|--------|-----|
| 2025-10-05 | Initial draft created from ENVIRONMENTS-AND-CONFIG.md | AI Assistant |
| | | |

---

## 🔗 Related Documents

- **Technical Specification** (for developers): [spec.md](spec.md) in this same folder
- **Original Requirements Document**: `docs/ENVIRONMENTS-AND-CONFIG.md` in repository root
- **Project Constitution**: `constitution.md` in repository root
- **Boot Sequence Documentation**: `docs/BOOT-SEQUENCE.md`

---

*This document is meant to be read by people without technical backgrounds. If you find any section confusing or full of jargon, please let us know so we can make it clearer!*
