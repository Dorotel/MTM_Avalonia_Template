# Research Findings: Environment and Configuration Management

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-05
**Status**: Complete (28 clarifications resolved)

## Overview

All research completed through clarification session documented in `spec.md`. This document consolidates the key decisions, rationale, and alternatives considered.

## Key Research Areas

### 1. Configuration Storage Architecture (CL-005, CL-006, CL-011, CL-012, CL-028)

**Decision**: Dual-storage approach with user-scoped configuration
- **User folder locations**: `config/user-folders.json` with dynamic detection (network/local fallback)
- **User preference values**: MAMP MySQL 5.7 database (`UserPreferences` table)
- **Database schema**: `config/database-schema.json` with placeholder values

**Rationale**:
- Separates infrastructure (folder paths) from data (preferences)
- User-scoped eliminates concurrent update conflicts
- Network fallback ensures reliability in various network conditions
- Database provides query performance and transactional integrity

**Alternatives Considered**:
- ❌ Single database storage: Network dependency for all operations
- ❌ Single file storage: Poor concurrency, no ACID guarantees
- ✅ Hybrid approach: Optimal balance of performance, reliability, and flexibility

### 2. Error Handling Strategy (CL-001, CL-003, CL-004, CL-009, CL-024, CL-025, CL-027)

**Decision**: Severity-based notification and recovery
- **Non-critical errors**: Default value + status bar warning (click for details)
- **Critical errors**: Modal dialog blocking interaction until resolved
- **Recovery options**: "Restore defaults", "Try to repair", "Contact support"

**Rationale**:
- Manufacturing environments prioritize uptime
- Critical issues demand immediate attention
- User agency preserved through clear recovery options
- Prevents infinite retry loops

**Alternatives Considered**:
- ❌ Always throw exceptions: Too disruptive
- ❌ Silent failure: Users miss critical problems
- ✅ Severity-based: Balances usability with safety

### 3. Feature Flag Synchronization (CL-010)

**Decision**: Launch-time-only updates (tied to version deployments)
- Changes applied only when MTM_Application_Launcher detects version mismatch
- Running applications do NOT sync flags in real-time
- Leverages existing launcher infrastructure

**Rationale**:
- Predictable, stable experience
- Prevents mid-session behavior changes
- Aligns with version control best practices
- No complex real-time sync infrastructure needed

**Alternatives Considered**:
- ❌ Real-time server sync: Unpredictable mid-session changes
- ❌ Manual restart prompts: User friction
- ✅ Launcher-driven: Predictable + existing infrastructure

### 4. Android Architecture (CL-007, CL-023, CL-026)

**Decision**: MTM Server API for all server-side access
- Android → MTM Server API → MySQL/Visual ERP
- Server handles connection pooling, caching, Visual API Toolkit integration
- Two-factor auth: Credentials + device certificate
- Offline mode: Cached data viewing + local operations only

**Rationale**:
- Security boundary (Android never directly accesses Visual/MySQL)
- Centralized Visual API access with caching
- Offline capability for warehouse environments
- Device certificate validation

**Alternatives Considered**:
- ❌ Direct MySQL from Android: Security risk, connectivity issues
- ❌ Direct Visual API from Android: Credential exposure
- ✅ Server API proxy: Security + reliability + offline support

### 5. Environment Variable Handling (CL-017, CL-018)

**Decision**: Startup-only detection with explicit precedence
- **Timing**: Read once at startup (not runtime polling)
- **Precedence**: MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → DOTNET_ENVIRONMENT → Build config → Defaults
- **Format**: Underscore format (MTM_*, DOTNET_*, ASPNETCORE_*)

**Rationale**:
- Manufacturing environments have stable configuration
- Startup detection provides optimal performance
- App-specific variables override framework defaults (user expectations)
- No mid-session configuration changes

**Alternatives Considered**:
- ❌ Runtime polling: Performance overhead, unexpected changes
- ❌ Framework defaults first: Conflicts with app-specific expectations
- ✅ Startup-only with app priority: Performance + predictability

### 6. Credential Storage (CL-004, CL-015, CL-025)

**Decision**: OS-native storage with clear platform boundaries
- **Windows**: DPAPI via `WindowsSecretsService`
- **Android**: KeyStore via `AndroidSecretsService`
- **Unsupported platforms**: Throw `PlatformNotSupportedException`
- **Storage unavailable**: User-friendly dialog prompting re-entry
- **Dialog cancellation**: App closes immediately (with clear warning)

**Rationale**:
- Phase 1 focus: Windows + Android only
- OS-native provides hardware-backed encryption when available
- Clear platform boundaries prevent misleading errors
- User-friendly error messages (no technical jargon)

**Alternatives Considered**:
- ❌ Cross-platform library: Less secure, adds complexity
- ❌ Plaintext fallback: Unacceptable security risk
- ✅ OS-native per platform: Maximum security

### 7. Visual API Whitelist (CL-008)

**Decision**: Dual-storage whitelist
- **Documentation**: `docs/VISUAL-WHITELIST.md` (source of truth)
- **Runtime validation**: `appsettings.json` under `Visual:AllowedCommands`
- **Citation format**: "Reference-{FileName} - {Chapter/Section/Page}"
- **Enforcement**: All Visual API calls validate before execution

**Rationale**:
- Human-readable documentation for maintainability
- Runtime array for performance (no file I/O on every call)
- Dual storage ensures auditability + performance
- Citations enable compliance verification

**Alternatives Considered**:
- ❌ Code-only whitelist: Poor maintainability
- ❌ Database-only: Requires DB for validation
- ✅ Documentation + runtime: Best of both worlds

### 8. Database Schema Management (CL-006, CL-012, CL-024)

**Decision**: JSON schema definition with placeholders
- **Location**: `config/database-schema.json`
- **Purpose**: Define tables, columns, constraints, relationships
- **Content**: Placeholder values developer replaces before setup
- **Migration failures**: In-memory fallback (non-vital) or block startup (vital)

**Rationale**:
- Single source of truth for schema
- Enables code generation and validation
- Placeholders allow developer customization
- Severity-based failure handling maintains uptime

**Alternatives Considered**:
- ❌ Code-first migrations: Requires EF Core, adds complexity
- ❌ SQL scripts only: No programmatic schema access
- ✅ JSON schema: Flexibility + automation

### 9. Configuration Change Notifications (CL-021, CL-022)

**Decision**: Automatic event-driven with change detection
- **Behavior**: Notify all subscribers immediately on change
- **Optimization**: Only fire events when value differs from previous
- **Mechanism**: `OnConfigurationChanged` event with Key, OldValue, NewValue

**Rationale**:
- Manufacturing users expect immediate visual feedback
- Change detection prevents wasted UI re-renders
- Event-driven pattern standard in modern UI frameworks
- No developer burden to remember manual notifications

**Alternatives Considered**:
- ❌ Polling-based: Performance overhead, delayed feedback
- ❌ Manual triggers: Developer burden, easy to forget
- ✅ Automatic event-driven: Best UX + performance

### 10. Scale and Data Volume (CL-014)

**Decision**: 1,000 users with admin-managed cleanup
- **Capacity**: Up to 1,000 concurrent users
- **Management**: Admin can remove inactive users and data
- **Optimization**: Indexes on UserId, PreferenceKey, FlagName

**Rationale**:
- Moderate scale appropriate for manufacturing
- Admin-managed cleanup provides operational flexibility
- Indexed queries ensure performance at scale
- No complex auto-archival logic needed

**Alternatives Considered**:
- ❌ Unlimited scale: Over-engineering for requirements
- ❌ Auto-archival: Complex logic, potential data loss risks
- ✅ Admin-managed: Simple, flexible, appropriate

## Technology Stack Decisions

### Primary Technologies
- **Language**: C# .NET 9.0 (nullable reference types enabled)
- **UI**: Avalonia 11.3.6 (cross-platform XAML)
- **MVVM**: CommunityToolkit.Mvvm 8.4.0 (source generators)
- **Database**: MySql.Data 9.0.0 (MAMP MySQL 5.7)
- **Testing**: xUnit 2.9.2, NSubstitute 5.1.0, FluentAssertions 6.12.1
- **Logging**: Serilog 8.0.0 (structured logging)
- **Validation**: FluentValidation 11.10.0

### Rationale
- .NET 9.0: Latest LTS, best Avalonia support, modern C# features
- Avalonia 11.3.6: Proven cross-platform UI, active community
- CommunityToolkit.Mvvm: Source generators reduce boilerplate, standard MVVM pattern
- MySQL: Existing MAMP infrastructure, relational integrity for preferences
- xUnit: Standard .NET testing framework, excellent async support
- Serilog: Best-in-class structured logging, flexible sinks

## Performance Targets

Based on non-functional requirements from spec:
- Configuration retrieval: <10ms (in-memory cache)
- Credential retrieval: <100ms (OS-native storage)
- Feature flag evaluation: <5ms
- Configuration change events: <50ms dispatch

**Validation Strategy**: Performance tests in Phase 5 validation

## Security Decisions

- **Credential Storage**: OS-native only (DPAPI/KeyStore), never plaintext
- **Logging**: Sensitive data redaction (password, token, secret, credential keywords)
- **Android Auth**: Two-factor (user credentials + device certificate)
- **Visual Access**: Read-only via whitelist-validated API Toolkit commands
- **Network**: HTTPS only for Android→MTM Server API communication

## Outstanding Items

None - all clarifications resolved through CL-001 through CL-028.

## References

- Feature Specification: `spec.md`
- Constitution: `.specify/memory/constitution.md` v1.3.0
- Clarifications: Session 2025-10-05 in `spec.md` (28 questions resolved)
