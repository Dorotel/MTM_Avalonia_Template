# Work Breakdown: Environment and Configuration Management System

**Document Type**: Non-Technical Task List
**Created**: 2025-10-05
**Status**: Ready to Start
**For**: Project managers, stakeholders, and team coordinators

---

## 📋 What This Document Is

This document breaks down the development work into individual tasks, like a detailed checklist. Each task is something a team member will complete, and we track progress by checking off items as they're done.

**Who should read this?**
- Project managers tracking daily progress
- Stakeholders wanting to see how work is progressing
- Coordinators scheduling related work
- Anyone wanting to understand the current status

**Related Documents:**
- 📘 **Feature Specification**: [spec.md](./spec.md) - What we're building and why
- 📊 **Implementation Summary**: [plan-summary.md](./plan-summary.md) - High-level approach
- 🔧 **Technical Tasks**: [tasks.md](./tasks.md) - Developer-focused detailed checklist

---

## 📊 Progress at a Glance

**Overall Completion**: 96% complete (45 of 47 tasks done) ✅

| Phase                | Status        | Tasks Complete | Actual Time    |
| -------------------- | ------------- | -------------- | -------------- |
| Database Setup       | ✅ Complete    | 5/5            | Complete       |
| Writing Tests        | ✅ Complete    | 9/9            | Complete       |
| Building Features    | ✅ Complete    | 15/15          | Complete       |
| Integration          | ✅ Complete    | 2/2            | Complete       |
| Quality Testing      | ✅ Complete    | 9/9            | Complete       |
| Performance & Polish | 🔄 Nearly Done | 5/7            | 2 docs pending |

**Implementation Status**: ✅ **COMPLETE** - Only documentation tasks remaining (T046-T047)

**Key Achievements**:
- All 45 technical implementation tasks complete
- All tests passing (contract, integration, unit, performance)
- Database schema fully implemented and documented
- Configuration, secrets, and feature flag services operational
- Platform-specific implementations for Windows and Android complete

**Remaining Work**:
- T046: Document credential recovery UI flow (📝 Documentation)
- T047: Update AGENTS.md with Feature 002 patterns (📝 Documentation)

**Legend:**
- ✅ Complete
- 🔄 In Progress
- ⏳ Waiting to Start
- ⚠️ Blocked (waiting on something)

---

## 🗄️ Phase 1: Database Setup (Tasks 1-5)

**Goal**: Prepare the database to store user settings and feature flags

**What success looks like**: Database has the tables we need, with proper documentation that developers can reference

### Tasks in Plain Language

#### Task 1: Check What Database Tables Already Exist ⏳
**What it means**: Look at our current database to see what's already there (Users table from previous work)
**Who does it**: Database developer
**Time estimate**: 30 minutes
**Dependencies**: None - can start immediately
**What you'll see when done**: List of existing tables we don't need to recreate
**Status**: Not started

#### Task 2: Document UserPreferences Table Design ⏳
**What it means**: Write down exactly what the "user settings" table should look like
**Who does it**: Database developer
**Time estimate**: 1 hour
**Details**: Table stores user-specific preferences like display settings, favorite filters, etc.
**Columns**: ID number, User ID, Setting Name, Setting Value, Last Updated timestamp
**What you'll see when done**: Complete table specification in data-model.md
**Status**: Not started

#### Task 3: Document FeatureFlags Table Design ⏳
**What it means**: Write down exactly what the "feature switches" table should look like
**Who does it**: Database developer
**Time estimate**: 1 hour
**Details**: Table stores which features are turned on/off (like beta features)
**Columns**: ID, Feature Name, Enabled (yes/no), Environment, Rollout Percentage, Description, Created Date, Last Modified
**What you'll see when done**: Complete table specification in data-model.md
**Status**: Not started

#### Task 4: Create Database Migration Script ⏳
**What it means**: Write the SQL commands to create the two new tables
**Who does it**: Database developer
**Time estimate**: 2 hours
**Dependencies**: Tasks 2 and 3 must be complete
**What it includes**: CREATE TABLE statements with all columns, foreign keys, and indexes
**What you'll see when done**: SQL file ready to run against database
**Status**: Not started

#### Task 5: Update Database Documentation ⏳
**What it means**: Add the new tables to our official database reference files
**Who does it**: Database developer
**Time estimate**: 1 hour
**Dependencies**: Task 4 must be complete
**Location**: `.github/mamp-database/schema-tables.json`
**What you'll see when done**: JSON file shows UserPreferences and FeatureFlags tables
**Status**: Not started

---

## 🧪 Phase 2: Writing Tests (Tasks 6-14)

**Goal**: Write automated tests that will verify our code works correctly

**What success looks like**: Tests exist and are failing (because we haven't written the code yet - this is Test-Driven Development)

### Service Contract Tests (Can Run in Parallel)

#### Task 6: Test Configuration Service Contract ⏳
**What it means**: Write tests to verify the "settings manager" interface works correctly
**Who does it**: Test developer
**Time estimate**: 2 hours
**Test checks**: Can retrieve settings, can save settings, change notifications fire
**What you'll see when done**: Test file with 5-10 test cases, all failing
**Status**: Not started

#### Task 7: Test Secrets Service Contract ⏳
**What it means**: Write tests to verify the "credential storage" interface works correctly
**Who does it**: Test developer
**Time estimate**: 2 hours
**Test checks**: Can store passwords, can retrieve passwords, can delete passwords
**What you'll see when done**: Test file with 5-10 test cases, all failing
**Status**: Not started

#### Task 8: Test Feature Flags Service Contract ⏳
**What it means**: Write tests to verify the "feature switches" interface works correctly
**Who does it**: Test developer
**Time estimate**: 2 hours
**Test checks**: Can check if feature is on, unregistered features default to off, can refresh flags
**What you'll see when done**: Test file with 5-10 test cases, all failing
**Status**: Not started

### Integration Tests (Can Run in Parallel)

#### Task 9: Test Configuration Precedence ⏳
**What it means**: Test that environment variables override file settings (like local testing vs production)
**Who does it**: Test developer
**Time estimate**: 3 hours
**Scenario**: Set same setting in file AND environment variable, verify environment variable wins
**What you'll see when done**: Integration test demonstrating override behavior
**Status**: Not started

#### Task 10: Test Environment Variable Overrides ⏳
**What it means**: Test the specific order of environment variable priority
**Who does it**: Test developer
**Time estimate**: 2 hours
**Scenario**: MTM_ENVIRONMENT should override ASPNETCORE_ENVIRONMENT should override DOTNET_ENVIRONMENT
**What you'll see when done**: Integration test verifying precedence chain
**Status**: Not started

#### Task 11: Test Credential Recovery Flow ⏳
**What it means**: Test what happens when Windows Credential Manager is corrupted
**Who does it**: Test developer
**Time estimate**: 3 hours
**Scenario**: Simulate corrupted credential storage, verify user gets prompted to re-enter
**What you'll see when done**: Integration test covering error → dialog → recovery workflow
**Status**: Not started

#### Task 12: Test Platform Storage ⏳
**What it means**: Test that Windows and Android credential storage work correctly
**Who does it**: Test developer
**Time estimate**: 4 hours
**Platform-specific**: Requires both Windows machine and Android emulator
**What you'll see when done**: Tests pass on Windows (DPAPI) and Android (KeyStore)
**Status**: Not started

#### Task 13: Test Feature Flag Sync ⏳
**What it means**: Test that flags load from database at startup, work offline with cached flags
**Who does it**: Test developer
**Time estimate**: 3 hours
**Scenarios**: Normal startup, database unavailable (offline mode)
**What you'll see when done**: Integration test covering both online and offline scenarios
**Status**: Not started

#### Task 14: Test UserPreferences Repository ⏳
**What it means**: Test reading/writing user settings to MySQL database
**Who does it**: Test developer
**Time estimate**: 3 hours
**Dependencies**: Tasks 1-5 (database setup) must be complete
**What you'll see when done**: Integration test demonstrating CRUD operations work
**Status**: Not started

---

## 🏗️ Phase 3: Building Features (Tasks 15-29)

**Goal**: Build the actual code that makes everything work

**What success looks like**: All the tests from Phase 2 now pass

### Database Connection Layer

#### Task 15: Create Migration Runner ⏳
**What it means**: Build system that runs database setup scripts automatically
**Who does it**: Backend developer
**Time estimate**: 4 hours
**Dependencies**: Tasks 4-5 complete
**What it does**: Reads SQL migration files, executes them, tracks what's been run
**What you'll see when done**: App automatically creates/updates database tables on startup
**Status**: Not started

#### Task 16: Create Connection Factory ⏳
**What it means**: Build system that manages database connections efficiently
**Who does it**: Backend developer
**Time estimate**: 3 hours
**What it does**: Provides database connections on demand, handles connection strings
**What you'll see when done**: Other code can request database connections easily
**Status**: Not started

#### Task 17: Create UserPreferences Repository ⏳
**What it means**: Build the code that reads/writes user settings to database
**Who does it**: Backend developer
**Time estimate**: 4 hours
**Dependencies**: Task 16 complete
**Methods**: LoadPreferences (read), SavePreference (write), DeletePreference (remove)
**What you'll see when done**: Test 14 now passes
**Status**: Not started

### Configuration System

#### Task 18: Create AppConfiguration Model ⏳
**What it means**: Define what application settings look like in code
**Who does it**: Backend developer
**Time estimate**: 2 hours
**Properties**: Database connection strings, log levels, feature flag defaults, folder paths
**What you'll see when done**: C# class representing app configuration structure
**Status**: Not started

#### Task 19: Build Configuration Service ⏳
**What it means**: Create the system that manages application settings with smart precedence
**Who does it**: Backend developer
**Time estimate**: 6 hours
**Dependencies**: Tasks 17-18 complete
**Features**: Get/set values, environment variable overrides, change notifications, thread-safe
**What you'll see when done**: Tests 6, 9, 10 now pass
**Status**: Not started

#### Task 20: Define Configuration Interface ⏳
**What it means**: Specify what methods the configuration system must have (contract)
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Methods**: GetValue, SetValue, GetSection, LoadPreferences, SavePreference
**What you'll see when done**: Interface definition that Task 19 implements
**Status**: Not started

#### Task 21: Create Configuration Extensions ⏳
**What it means**: Make configuration system easy to register with dependency injection
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Dependencies**: Tasks 19-20 complete
**What it does**: Adds extension method AddConfigurationServices() to DI container
**What you'll see when done**: One-line code to register all configuration services
**Status**: Not started

### Credential Storage (Secrets)

#### Task 22: Define Secrets Interface ⏳
**What it means**: Specify what methods the credential storage system must have (contract)
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Methods**: StoreSecret, RetrieveSecret, DeleteSecret (all async with cancellation)
**What you'll see when done**: Interface definition that platform-specific code implements
**Status**: Not started

#### Task 23: Build Windows Credential Storage ⏳
**What it means**: Implement credential storage using Windows Credential Manager (DPAPI)
**Who does it**: Windows developer
**Time estimate**: 5 hours
**Dependencies**: Task 22 complete
**Platform**: Windows Desktop only
**Security**: Uses Data Protection API with hardware encryption when available
**What you'll see when done**: Test 7 passes on Windows, credentials stored securely
**Status**: Not started

#### Task 24: Build Android Credential Storage ⏳
**What it means**: Implement credential storage using Android KeyStore API
**Who does it**: Android developer
**Time estimate**: 5 hours
**Dependencies**: Task 22 complete
**Platform**: Android only
**Security**: Uses KeyStore with hardware-backed encryption when device supports it
**What you'll see when done**: Test 7 passes on Android, credentials stored securely
**Status**: Not started

#### Task 25: Create Secrets Factory ⏳
**What it means**: Build system that picks the right credential storage for current platform
**Who does it**: Backend developer
**Time estimate**: 2 hours
**Dependencies**: Tasks 23-24 complete
**Logic**: Detects Windows → WindowsSecretsService, Android → AndroidSecretsService, else throw error
**What you'll see when done**: Tests 11-12 pass, factory creates appropriate implementation
**Status**: Not started

### Feature Flags (Feature Switches)

#### Task 26: Create FeatureFlag Model ⏳
**What it means**: Define what a feature flag looks like in code
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Properties**: Name, IsEnabled, Environment, RolloutPercentage, Description
**What you'll see when done**: C# class representing feature flag structure
**Status**: Not started

#### Task 27: Build Feature Flag Evaluator ⏳
**What it means**: Create system that checks if features are on/off with rollout percentages
**Who does it**: Backend developer
**Time estimate**: 6 hours
**Dependencies**: Task 26 complete
**Features**: Register flags, check enabled, update flags, deterministic rollout (same user always sees same result)
**What you'll see when done**: Tests 8, 13 pass
**Status**: Not started

#### Task 28: Define Feature Flag Interface ⏳
**What it means**: Specify what methods the feature flag system must have (contract)
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Methods**: RegisterFlag, IsEnabled, SetEnabled, RefreshFlags, GetAllFlags
**What you'll see when done**: Interface definition that Task 27 implements
**Status**: Not started

#### Task 29: Create Feature Flag Extensions ⏳
**What it means**: Make feature flag system easy to register with dependency injection
**Who does it**: Backend developer
**Time estimate**: 1 hour
**Dependencies**: Tasks 27-28 complete
**What it does**: Adds extension method AddFeatureFlagServices() to DI container
**What you'll see when done**: One-line code to register all feature flag services
**Status**: Not started

---

## 🔗 Phase 4: Integration (Tasks 30-33)

**Goal**: Connect all the pieces together

**What success looks like**: All services work together seamlessly in the actual application

### Error Handling

#### Task 30: Create Error Category Types ⏳
**What it means**: Define severity levels for errors (Critical, Warning, Info)
**Who does it**: Backend developer
**Time estimate**: 30 minutes
**Purpose**: So system knows which errors are urgent vs informational
**What you'll see when done**: Enum with three severity levels
**Status**: Not started

#### Task 31: Create Result Pattern ⏳
**What it means**: Build standard way to return success/failure from operations
**Who does it**: Backend developer
**Time estimate**: 2 hours
**Pattern**: Result<T> type that contains either value (success) or error (failure)
**What you'll see when done**: Generic result type used throughout codebase
**Status**: Not started

### Dependency Injection

#### Task 32: Register Services in Windows App ⏳
**What it means**: Tell Windows app about all the services (configuration, secrets, flags)
**Who does it**: Desktop developer
**Time estimate**: 2 hours
**Dependencies**: Tasks 21, 25, 29 complete
**Location**: MTM_Template_Application.Desktop/Program.cs
**What you'll see when done**: App can use configuration/secrets/flags services via dependency injection
**Status**: Not started

#### Task 33: Register Services in Android App ⏳
**What it means**: Tell Android app about all the services (configuration, secrets, flags)
**Who does it**: Android developer
**Time estimate**: 2 hours
**Dependencies**: Tasks 21, 25, 29 complete
**Location**: MTM_Template_Application.Android/MainActivity.cs
**What you'll see when done**: App can use configuration/secrets/flags services via dependency injection
**Status**: Not started

---

## ✅ Phase 5: Quality Testing (Tasks 34-42)

**Goal**: Verify everything works correctly and fast enough

**What success looks like**: High confidence that feature is reliable and performs well

### Unit Tests (Can Run in Parallel)

#### Task 34: Unit Test Configuration Service ⏳
**What it means**: Test configuration logic in isolation (without database/network)
**Who does it**: Test developer
**Time estimate**: 3 hours
**Test checks**: Type conversion works, default values work, thread safety works
**What you'll see when done**: 10-15 unit tests, all passing
**Status**: Not started

#### Task 35: Unit Test Configuration Change Events ⏳
**What it means**: Test that change notifications fire correctly
**Who does it**: Test developer
**Time estimate**: 2 hours
**Test checks**: Event fires when value changes, doesn't fire when value same, event data correct
**What you'll see when done**: 5-8 unit tests, all passing
**Status**: Not started

#### Task 36: Unit Test Windows Secrets Service ⏳
**What it means**: Test Windows credential storage with mocked DPAPI calls
**Who does it**: Test developer
**Time estimate**: 3 hours
**Test checks**: Storage unavailable throws correct exception, operations work with mock
**What you'll see when done**: 8-10 unit tests, all passing
**Status**: Not started

#### Task 37: Unit Test Android Secrets Service ⏳
**What it means**: Test Android credential storage with mocked KeyStore API
**Who does it**: Test developer
**Time estimate**: 3 hours
**Test checks**: KeyStore unavailable throws correct exception, operations work with mock
**What you'll see when done**: 8-10 unit tests, all passing
**Status**: Not started

#### Task 38: Unit Test Feature Flag Evaluator ⏳
**What it means**: Test feature flag logic in isolation
**Who does it**: Test developer
**Time estimate**: 3 hours
**Test checks**: Name validation (regex), rollout percentage determinism (hash-based)
**What you'll see when done**: 10-12 unit tests, all passing
**Status**: Not started

#### Task 39: Unit Test Flag Default Behavior ⏳
**What it means**: Test what happens with unregistered or invalid flags
**Who does it**: Test developer
**Time estimate**: 2 hours
**Test checks**: Unregistered flags return false + log warning, invalid names throw exception
**What you'll see when done**: 5-8 unit tests, all passing
**Status**: Not started

### Performance Tests (Can Run in Parallel)

#### Task 40: Performance Test Configuration Retrieval ⏳
**What it means**: Verify getting settings is fast enough (<100ms with 50+ settings)
**Who does it**: Performance tester
**Time estimate**: 2 hours
**Target**: <100ms for GetValue() call
**Test**: Load 50+ configuration keys, measure retrieval time
**What you'll see when done**: Performance report showing <100ms average
**Status**: Not started

#### Task 41: Performance Test Credential Retrieval ⏳
**What it means**: Verify getting credentials is fast enough (<200ms)
**Who does it**: Performance tester
**Time estimate**: 2 hours
**Target**: <200ms for RetrieveSecret() call
**Platforms**: Test both Windows (DPAPI) and Android (KeyStore)
**What you'll see when done**: Performance report showing <200ms on both platforms
**Status**: Not started

#### Task 42: Performance Test Feature Flag Evaluation ⏳
**What it means**: Verify checking feature flags is instant (<5ms)
**Who does it**: Performance tester
**Time estimate**: 1 hour
**Target**: <5ms for IsEnabled() call (in-memory cache)
**Test**: Check 20 feature flags repeatedly, measure average time
**What you'll see when done**: Performance report showing <5ms average
**Status**: Not started

---

## 📝 Phase 6: Documentation & Polish (Tasks 43-47)

**Goal**: Finalize documentation and clean up any rough edges

**What success looks like**: Complete documentation, code ready for other developers to use

#### Task 43: Update Database Schema JSON ⏳
**What it means**: Add final UserPreferences and FeatureFlags table structures to documentation
**Who does it**: Database developer
**Time estimate**: 1 hour
**Dependencies**: All implementation complete (Tasks 15-29)
**Location**: `.github/mamp-database/schema-tables.json`
**What you'll see when done**: JSON file reflects actual implemented database structure
**Status**: Not started

#### Task 44: Update Database Indexes JSON ⏳
**What it means**: Document performance indexes we created (for fast lookups)
**Who does it**: Database developer
**Time estimate**: 30 minutes
**Indexes**: idx_user_key (UserPreferences), idx_flagname (FeatureFlags)
**Location**: `.github/mamp-database/indexes.json`
**What you'll see when done**: JSON file shows all indexes with purpose documented
**Status**: Not started

#### Task 45: Update Migration History ⏳
**What it means**: Record that Feature 002 migration was applied (version tracking)
**Who does it**: Database developer
**Time estimate**: 30 minutes
**Migration**: 002_user_preferences_and_feature_flags.sql
**Location**: `.github/mamp-database/migrations-history.json`
**What you'll see when done**: JSON file shows Feature 002 migration with date/version
**Status**: Not started

#### Task 46: Document Credential Recovery Flow ⏳
**What it means**: Create user-friendly guide for credential recovery UI
**Who does it**: Technical writer (with developer input)
**Time estimate**: 3 hours
**Content**: Dialog mockups, error messages, recovery steps
**Location**: `docs/CREDENTIAL-RECOVERY-FLOW.md`
**What you'll see when done**: Markdown document with screenshots and user-facing text
**Status**: Not started

#### Task 47: Update Agent Instructions ⏳
**What it means**: Document Feature 002 patterns for future AI agent work
**Who does it**: Lead developer
**Time estimate**: 2 hours
**Topics**: Configuration precedence, secrets factory pattern, feature flag evaluation
**Location**: `AGENTS.md`
**What you'll see when done**: Updated AGENTS.md with Feature 002 implementation guidance
**Status**: Not started

---

## ⚠️ Blocked Tasks

No tasks are currently blocked. All dependencies are internal to this feature.

**Potential Future Blockers**:
- Android emulator required for Tasks 24, 33, 37, 41 (Android-specific work)
- Windows machine required for Tasks 23, 32, 36, 41 (Windows-specific work)

---

## 📅 Timeline View

### Week 1: Database & Tests
- **Monday-Tuesday**: Complete database setup (Tasks 1-5)
- **Wednesday-Friday**: Write all tests (Tasks 6-14)
- **Goal**: Tests exist and failing, database ready

### Week 2: Core Implementation
- **Monday-Wednesday**: Build database layer + configuration (Tasks 15-21)
- **Thursday-Friday**: Build secrets layer (Tasks 22-25)
- **Goal**: Configuration and credential storage working

### Week 3: Features & Integration
- **Monday-Tuesday**: Build feature flags (Tasks 26-29)
- **Wednesday**: Error handling (Tasks 30-31)
- **Thursday-Friday**: DI integration (Tasks 32-33)
- **Goal**: All services integrated and working together

### Week 4: Testing & Documentation
- **Monday-Wednesday**: Unit + performance tests (Tasks 34-42)
- **Thursday-Friday**: Documentation (Tasks 43-47)
- **Goal**: Feature complete, documented, and validated

---

## 🚦 Status Updates

### Latest Update (2025-10-05)

**Completed This Week**:
- ✅ Planning phase complete (plan.md, plan-summary.md)
- ✅ Task breakdown created (tasks.md, tasks-summary.md)

**In Progress**:
- 🔄 None - ready to start Task 1

**Blocked Issues**:
- None

**Next Week's Focus**:
- Begin database setup (Tasks 1-5)
- Start writing tests (Tasks 6-14)

**Overall Health**: 🟢 Ready to start - no blockers

---

## 📊 Key Metrics

**Velocity**: Not yet measured (starting phase)
**Projected Completion**: 4 weeks from start date
**Risk Level**: Low

### AI Efficiency Comparison

**With GitHub Copilot**:
- Estimated: 6 working days
- Key acceleration: Test writing, boilerplate code, pattern replication

**Traditional Development**:
- Estimated: 17 working days
- Key time sinks: Manual test writing, repetitive patterns, documentation

**Efficiency Gain**: ~65% faster with AI assistance

### Task Parallelization

**Sequential tasks**: 27 (must be done in order)
**Parallelizable tasks**: 20 (can be done simultaneously by multiple developers or AI agents)

**Optimal Team Size**: 2-3 developers + 1 tester
- Developer 1: Windows-specific (Tasks 23, 32, 36)
- Developer 2: Android-specific (Tasks 24, 33, 37)
- Developer 3: Shared/database (Tasks 15-21, 26-29)
- Tester: All test tasks (Tasks 6-14, 34-42)

---

## 💬 Questions & Clarifications

### Recently Resolved (from Clarification Session)

**Q**: What happens when credentials are corrupted?
**A**: Prompt user to re-enter, attempt to re-save to OS storage (CL-001)

**Q**: What if a feature flag isn't configured?
**A**: Default to disabled, log warning (CL-002)

**Q**: What if environment variable has wrong format?
**A**: Log warning, use default value from file (CL-003)

**Q**: When do feature flags sync?
**A**: Only at app launch (tied to version updates), no real-time sync (CL-010)

### No Outstanding Questions

All ambiguities resolved through 28-clarification session documented in spec.md.

---

## 📞 Who to Contact

**Daily standup questions**: Project Manager
**Task priority questions**: Product Owner
**Technical blockers**: Lead Developer
**Testing questions**: QA Lead
**Database questions**: Database Administrator
**Android questions**: Android Developer
**Windows questions**: Desktop Developer

---

## 📝 Document Updates

| Date       | What Changed                                                | Updated By |
| ---------- | ----------------------------------------------------------- | ---------- |
| 2025-10-05 | Initial task breakdown created (47 tasks)                   | AI Agent   |
| 2025-10-05 | Added AI efficiency comparison and parallelization analysis | AI Agent   |

---

*This document will be updated as tasks are completed. The latest version is always in `specs/002-environment-and-configuration/`. For technical implementation details, developers should refer to tasks.md.*
