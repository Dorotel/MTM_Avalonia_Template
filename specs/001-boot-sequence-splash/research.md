# Technology Research: Boot Sequence Implementation

**Feature**: Boot Sequence — Splash-First, Services Initialization Order
**Date**: 2025-10-02
**Status**: Complete - All clarifications resolved

## Executive Summary

All technical unknowns resolved through comprehensive clarification sessions across 11 categories. Key decisions favor:
- OS-native secure storage for credentials
- OpenTelemetry for structured logging
- Exponential backoff for retries (1s, 2s, 4s, 8s, 16s)
- Configurable timeouts and thresholds per environment
- Defense-in-depth security approach
- Graceful degradation for offline scenarios

## Research Areas

### 1. Boot Orchestration & Stage Management

**Decision**: Three-stage sequential with dependency-aware parallelization within stages

**Key Parameters** (from Clarifications):
- Stage timeouts: Configurable per environment (default: Stage 0: 10s, Stage 1: 60s, Stage 2: 15s) [Q1]
- Parallelization: All services except those with explicit dependencies [Q2]
- Watchdog timer: Per-step configurable thresholds (default: max(2x expected, 30s) with progress beacons) [Q3]
- Total boot target: <10 seconds (balanced) [Q4]
- Memory budget: 100MB during startup [Q5]
- Admin dashboard access: Role-based access control (RBAC) [Q6]

**Rationale**: Balanced performance with reliability. Configurable parameters allow environment-specific tuning based on telemetry.

**Alternatives Considered**:
- Fixed timeouts: Rejected - environments vary too much
- Full parallelization: Rejected - dependency violations cause race conditions
- No watchdog: Rejected - hung processes need detection

---

### 2. Configuration & Environment Management

**Decision**: Layered configuration with explicit precedence and hot-reload capability

**Override Precedence** [Q1]: Command-line args > Environment vars > User config > App config > Defaults

**Configuration Patterns**:
- Feature flag evaluation: At startup + on-demand when accessed [Q2]
- Hot-reload scope: Only settings explicitly marked "hot-reloadable" in schema [Q3]
- Validation timing: At load + before every hot reload [Q4]
- Profile structure: Base file + overlay files (config.base.json + config.dev.json) [Q5]
- Auto-detection order: Environment variable → Hostname → IP range → Config file → Development default [Q6]

**Audit & Security**:
- Audit log format: Timestamp + Setting + Old value + New value + User + Source [Q7]
- Encrypted sections: Any section marked "sensitive" in schema + all credentials [Q8]
- Remote updates: Pull-based polling (startup + configurable intervals) [Q9]
- Conflict resolution: Configurable per-setting (centrally-managed vs user-configurable) [Q10]

**Rationale**: Schema-driven approach provides flexibility while maintaining security and auditability.

---

### 3. Secrets & Credential Management

**Decision**: OS-native secure storage with machine binding

**Storage Mechanism** [Q1]: Windows Credential Manager / Android KeyStore (macOS Keychain deferred)
**Encryption** [Q2]: Machine-specific keys (DPAPI on Windows, KeyStore on Android)

**Security Measures**:
- Redaction scope [Q3]: Schema-marked sensitive fields + comprehensive pattern detection
- Patterns: Passwords, tokens, API keys, connection strings, base64 tokens
- Never log: Automatic redaction in logging pipeline before serialization
- Rotation support [Q4]: Reactive - prompted on first auth failure with guided resolution
- Multi-user handling [Q5]: Each OS user has separate encrypted store (natural isolation)
- Validation frequency [Q6]: At startup + after any authentication failure

**Rationale**: Leverages OS security infrastructure, avoids custom crypto implementation risks, provides hardware-backed encryption where available (TPM/Secure Enclave).

---

### 4. Logging, Telemetry & Monitoring

**Decision**: Structured JSON logging with OpenTelemetry semantic conventions

**Log Configuration**:
- Default level [Q1]: Debug (all logs) - can be filtered via CLI args
- File rotation [Q2]: 10MB per file (balanced)
- Retention policy [Q3]: 7 days
- Remote protocol [Q4]: OpenTelemetry / Application Insights (industry standard)
- Correlation ID scope [Q5]: Per request including all downstream operations

**Performance Metrics** [Q6]:
- Start with: Explicitly marked "monitored" methods (tag critical paths)
- Add: Methods taking >100ms (catch unexpected slow calls)
- Evolve to: Adaptive sampling if broader coverage needed

**Buffering & Retry**:
- Buffer size [Q7]: 100 entries before flush
- Remote retry [Q8]: Queue locally, resend on next successful connection (no real-time retry)
- Sampling [Q10]: Every 100th occurrence for high-frequency events

**Data Collection**:
- PII redaction [Q9]: Schema-driven allowlisting + comprehensive regex patterns (emails, phones, SSN, IPs, MACs, addresses, DOB)
  - Structured logging enables field-name redaction
  - Hash/truncate where values needed
  - Unit tests verify redaction before logs leave process
- Context enrichment [Q11]: All available (user, session, version, correlation ID, trace ID, span ID)
- Structured format [Q12]: OpenTelemetry semantic conventions, one JSON object per line

**Minimal OTel Fields**:
```
timestamp, severity_text, severity_number, body, logger.name,
exception.*, trace_id, span_id, correlation_id,
service.name, service.version, deployment.environment,
host.name, process.pid, thread.id, attributes{...}
```

**Rationale**: Industry standard enables integration with any observability platform. Structured format supports powerful querying and aggregation.

---

### 5. Diagnostics & System Health

**Decision**: Comprehensive startup diagnostics with graceful degradation

**Thresholds & Checks**:
- Storage warning [Q1]: <5% of total capacity OR 1GB, whichever is less
- Permission failures [Q2]: Continue with reduced functionality (disable dependent features)
- Network timeout [Q3]: 5 seconds for reachability checks
- Performance benchmark [Q4]: Composite score (CPU + disk I/O + memory), percentile-based thresholds from telemetry
- Network quality [Q5]: Multiple pings (average of 5) + download speed test (~100KB file)
- Hardware detection [Q6]: CPU cores, RAM, screen resolution, GPU, storage type (SSD/HDD), network adapters

**Failure Handling**:
- Crash detection [Q7]: Multi-source (lock file + log analysis + OS crash reports)
- Automatic fixes [Q8]: Safe operations + cache rebuild (missing folders, temp cleanup, cache corruption recovery)
  - Configuration reset requires user confirmation
  - All fixes logged for audit trail

**Rationale**: Comprehensive diagnostics enable proactive issue detection. Graceful degradation maintains productivity even when some subsystems fail.

---

### 6. Data Layer & Connectivity

**Decision**: Multi-client architecture with adaptive retry and circuit breakers

**Connection Management**:
- Pool size [Q1]: Configurable per environment
  - Desktop dev: Min 1, Max 3
  - Desktop prod: Min 2, Max 10
  - Android: Min 1, Max 5 (resource constraints)
- Retry policy [Q2]: Exponential backoff: 1s, 2s, 4s, 8s, 16s, max 5 retries (±25% jitter to prevent thundering herd)
- Health checks [Q3]: Adaptive interval
  - Stable: every 60 seconds
  - After failure: every 10 seconds for 5 minutes, then scale back
  - After reconnection: gradually return to normal
- Circuit breaker threshold [Q4]: Configurable per service/endpoint
  - Critical operations (auth): 3 consecutive failures
  - Data queries: 50% failure rate over 20 requests
  - Batch operations: 5 failures within 2 minutes
  - Default: 5 consecutive failures

**Recovery & Performance**:
- Circuit breaker recovery [Q5]: Exponential backoff: 30s, 1m, 2m, 5m, 10m (cap)
  - Manual retry button for user-initiated reconnection
- Metrics tracking [Q6]: Active/idle connections + average wait time + errors + timeouts + lifespan distribution
- Bulk operations [Q7]: Dynamic batch size based on record size and network speed
  - Target: 50-200KB per batch
  - Small records (~200 bytes): 500/batch
  - Large records (~2KB): 100/batch
  - Conservative default: 100 records until dynamic calculation completes
- Query cache TTL [Q8]: Different per query type
  - User lookup/permissions: 15 minutes
  - Active work orders: 1 minute
  - Part master data: 1 hour
  - Configuration tables: 24 hours
  - Default: 5 minutes for unspecified queries
  - Cache invalidation hints from Visual integration layer

**Lazy Initialization** [Q9]: Trigger after user authenticates (provides necessary credentials for connections)

**Read-Only Enforcement (Visual only)** [Q10]: Defense in depth
1. Database user with SELECT-only permissions
2. Connection string with ReadOnly=true
3. Application-level validation (reject INSERT/UPDATE/DELETE)
4. Audit logs track rejected write attempts

**Note**: MySQL access based on user role (Admin: full, Standard: restricted, ReadOnly: read-only)

**Rationale**: Adaptive strategies balance quick recovery with resource conservation. Defense-in-depth prevents accidental writes to Visual.

---

### 7. Core Application Services

**Decision**: Event-driven message bus with validation/mapping/serialization services

**Message Bus**:
- Delivery guarantees [Q1]: Configurable per message type
  - UI notifications: at-most-once (fire and forget)
  - Data sync events: at-least-once (subscribers idempotent)
  - Critical business events: exactly-once (transactional outbox pattern)
  - Default: at-least-once for safety
- Dead letter queue [Q2]: After 5 failed delivery attempts (~30 seconds with exponential backoff)
  - Admin UI to inspect and replay dead-lettered messages

**Service Discovery**:
- Validation rules [Q3]: Hybrid discovery (all approaches)
  - Attributes: Simple constraints (Required, Range, RegEx)
  - Conventions: Complex logic (cross-field validation, async DB checks)
  - Configuration: Runtime customization without recompilation
  - Discovery order: Attributes → Conventions → Configuration overrides
  - Use FluentValidation for composition
- Mapping profiles [Q4]: Multiple profile dimensions
  - API vs Internal (boundary translation)
  - Create vs Update vs Display (field inclusion rules)
  - Feature-specific (Inventory, Manufacturing, etc.)
  - Naming: `{Feature}.{Direction}.{Context}` (e.g., `Inventory.Api.Display`)
  - AutoMapper with assembly scanning

**Versioning & Resilience**:
- Schema versioning [Q5]: N-2 backward compatibility (current + 2 previous versions)
  - Version negotiation: client sends supported versions, server responds with highest compatible
  - Deprecate with 2-release notice period
- Retry strategy [Q6]: Base 1s, Multiplier 2x, Jitter ±25%, Max 30s
  - Formula: `delay = min(base * (multiplier ^ attempt) * random(0.75, 1.25), max)`
  - Override per-operation via Polly policies
- Health checks [Q7]: Every 30 seconds
  - Lightweight (cache status for 5 seconds, return immediately)
  - Endpoint: `/health` returning Healthy/Degraded/Unhealthy with subsystem details

**Rationale**: Flexible service architecture supports evolving requirements. Message-level configuration prevents over-engineering while maintaining reliability.

---

### 8. Visual Master Data Caching

**Decision**: Multi-tiered refresh strategy with delta sync and priority loading

**Refresh Strategy**:
- Refresh windows [Q1]: Startup + nightly + on-demand
  - Startup: Basic freshness without blocking on large syncs
  - Nightly: Aligns with maintenance windows, reduces daytime load
  - On-demand: Exceptional cases after known upstream changes
- Staleness threshold [Q2]: Different per data type
  - Parts: More volatile, shorter threshold
  - Locations/Warehouses: More stable, longer threshold
  - Aligns with per-type TTL policy
- Delta sync detection [Q3]: Timestamp + version combination
  - Timestamps: Simple but susceptible to clock skew
  - Versions: Monotonic change tracking
  - Together: Reduce false negatives/positives
  - Fallback to full comparison on detected inconsistencies

**Storage & Loading**:
- Compression [Q4]: LZ4 (fast decompression, acceptable ratios)
  - Minimizes startup and background refresh latency
- Priority order [Q5]: Parts → Warehouses → Locations → Work Centers
  - Parts unlock most UX scenarios
  - Warehouses contextualize inventory
  - Locations refine placement
  - Work centers less critical to initial navigation
- Background refresh notification [Q6]: User preference
  - Default: Notify on errors or completion (C)
  - Escalate to progress toast if >30 seconds (B)
  - Power users opt into progress, quiet environments stay silent

**Schema & TTL Management**:
- Schema versioning [Q7]: Auto-migrate with fallback
  - Detect version, apply idempotent migrations
  - If migration fails, fall back to full rebuild
- TTL configuration [Q8]: Configurable per data type (defaults)
  - Parts: 24 hours
  - Locations/Warehouses/Work Centers: 7 days
  - Allow operational override
- Preemptive refresh [Q9]: 25% of TTL before expiry
  - Scales with TTL to smooth load
  - Provides buffer for retries and slow networks

**Monitoring & Failure Handling**:
- Statistics tracking [Q10]: Hit/miss rates + cache size + refresh durations + staleness per type + error counts
  - Avoids high-cardinality per-query metrics
- Cache init failure [Q11]: Use last known good cache if available, else continue with empty cache and retry in background
  - Prioritizes availability

**Rationale**: Sophisticated caching strategy balances freshness with performance. Delta sync and priority loading minimize startup time.

---

### 9. Localization, Theme & Navigation

**Localization**:
- Initial language [Q1]: Priority cascade (User preference → System OS → Organization default → English)
- Missing translations [Q2]: Log once per key as Warning
  - Include: translation key, requested language, fallback used
  - Aggregate for translation team prioritization
  - DEBUG builds: show [EN] prefix on fallback text
- Runtime switching [Q3]: Update all screens in-place via reactive property binding
  - Immediate feedback without losing context
  - ViewModels re-query translations
  - AXAML resources: Replace ResourceDictionary to trigger visual tree refresh

**Theme**:
- Transition duration [Q4]: 200ms with ease-in-out
  - Matches platform standards (iOS/Android material design)
  - Respect prefers-reduced-motion (make instant)
- Auto-switching [Q5]: Continuous real-time monitoring (event-driven)
  - Windows: SystemEvents.UserPreferenceChanged
  - Android: onConfigurationChanged
  - Avalonia: ActualThemeVariantChanged
  - Only when mode is "Auto" (manual override disables)

**Navigation**:
- History depth [Q6]: 25 entries in circular buffer
  - Covers typical workflows without excessive memory
  - Serialize on shutdown for session restoration
- Unsaved changes [Q7]: Always block with Save/Discard/Cancel dialog
  - Three-button: "Save and Continue" (primary), "Discard Changes" (danger), "Cancel" (secondary)
  - Exception: breadcrumb up can auto-save if validation passes
  - "Don't ask again this session" checkbox for power users (resets on restart)
- Deep link failure [Q8]: Navigate to home with error notification
  - "Could not open [Screen Name]: [Reason]. The link may be outdated."
  - Include "Try Again" or "Go to [Screen]" buttons
- Analytics [Q9]: Screen views + time spent + navigation paths
  - Anonymize user IDs, no PII tracking
  - Local caching with batch upload
  - Allow opt-out in settings (default enabled)
- Multi-window [Q10]: Not supported (single window only)
  - Reduces complexity (shared state, synchronization, resource contention)
  - Single-instance enforcement: focus existing window on second launch

**Rationale**: Comprehensive localization and theme support enhance accessibility. Simple navigation model reduces complexity while meeting requirements.

---

### 10. Visual Server Integration & Cached-Only Mode

**Visual Integration**:
- API command whitelist [Q1]: Explicit whitelist (security by default)
  - Define in configuration: command name, allowed parameters, read-only flag
  - Reject unlisted commands with clear error
  - Log rejected attempts for security auditing
- Schema dictionary [Q2]: Embedded in app bundle (updated with releases)
  - Version compatibility (schema matches app expectations)
  - Offline capability
  - Include CSV files in source control, bundle in assets
  - Admin diagnostic screen shows schema version and compares to live Visual
- Android authentication [Q3]: User credentials + device certificate (two-factor)
  - User auth → short-lived JWT token (8 hour expiry)
  - Device certificate in Android KeyStore (1 year validity)
  - Subsequent API calls: JWT + device certificate for mutual TLS
  - Per-device revocation capability

**Cached-Only Mode**:
- Reconnection interval [Q4]: Exponential backoff: 30s, 1m, 2m, 5m, 10m (cap)
  - Reset on successful reconnection or user navigation
  - Show countdown in warning banner ("Retrying in 2:30...")
  - Manual "Retry Now" button
- Banner dismissal [Q5]: Dismissible, reappears after 10 minutes or on write attempt
  - Persistent status icon when dismissed
  - Clicking icon re-expands banner
- Feature limitations [Q6]: Block writes + real-time queries
  - Allow: Browsing cached data, viewing historical transactions, local draft creation (queued for sync)
  - Disable: Transaction posting, master data edits, real-time lookups
  - Gray out disabled UI with "Requires connection" tooltip
  - Show queued drafts count ("3 drafts pending sync")
  - On reconnection: prompt to review and sync
  - Reports/exports work from cache (with staleness warning)

**Rationale**: Security-first approach with whitelist. Cached-only mode maintains productivity during outages while preventing data loss.

---

### 11. Splash Screen & User Communication

**Progress & Animation**:
- Progress calculation [Q1]: Weight by measured duration from telemetry
  - Start with equal weights until telemetry accumulates
  - Adjust dynamically based on real-world timing
- Time estimates [Q2]: Show for operations ≥5 seconds
  - Meaningful context without UI noise
- Logo animation [Q3]: Continuous pulse (no fixed cycle)
  - Indicates active processing
  - Adapts to variable boot times
- Screen reader announcements [Q4]: Stage transitions + major milestones
  - Balance accessibility with avoiding overload
  - Structural checkpoints (Stage 0 → 1 → 2)
  - Major milestones (Configuration loaded, Services initialized)

**Error Handling**:
- Retry behavior [Q5]: Configurable based on error type
  - Transient errors (network): Retry operation only
  - Structural errors (config): Full restart from Stage 0
- Cancellation [Q6]: Always show confirmation dialog
  - Prevents accidental exits during startup
  - Brief confirmation prevents frustration from restart requirement
- Banner placement [Q7]: Top of window (above all content)
  - High visibility for critical operational mode indicator
  - Conventional placement for system warnings
- State persistence [Q8]: Always restart from Stage 0
  - Clean boot guarantees consistent state after crashes
  - Boot fast enough (<10s) that full restart acceptable

**Rationale**: User-centric design provides clear feedback without overwhelming. Accessibility built-in from start.

---

### 12. Error Categorization & Recovery

**Error Categories** [Q10]: Transient, Configuration, Network, Permission, Permanent
- Transient: Retry automatically (network glitches, temporary server issues)
- Configuration: Full restart required (invalid settings, missing files)
- Network: Retry with backoff, offer cached-only mode
- Permission: Show specific resolution guidance (missing storage access, camera permissions)
- Permanent: Cannot recover, show diagnostic bundle and support contact

**Failure Actions**:
- Credential validation failure [Q7]: Retry/Exit on first failure, escalate to offline mode after multiple failures
  - Avoids lockout UX while providing continuity
- Endpoint unreachability [Q8]: Retry / Continue (cached-only) / Safe Mode / Exit
  - Full set of safe continuations
- Diagnostic bundles [Q9]: Logs + error details + system info + configuration (redacted)
  - Sufficient for triage, minimal size/sensitivity
  - Opt-in extensions when requested
- Safe Mode [Q11]: Configurable feature set per deployment
  - Minimal baseline: View cached data only (no writes, no sync)
  - Enable additional capabilities by policy: manual data entry, settings/configuration
- Chronic issues [Q12]: Configurable threshold per error type
  - Defaults: Transient (3+ in 24h), Persistent (5+ in 7 days)
  - Environment-specific overrides

**Rationale**: Comprehensive error handling prevents dead-ends. Users always have actionable next steps.

---

## Implementation Priorities

Based on research, implementation should proceed in this order:

1. **Foundation** (Stage 0): Splash screen, logging infrastructure, configuration loading
2. **Core Services** (Stage 1): Diagnostics, secrets, data layer, message bus, validation
3. **Domain Services** (Stage 1): Cache management, localization, theme
4. **Application** (Stage 2): Navigation, UI shell, error handling
5. **Polish**: Telemetry integration, admin dashboard, chronic issue detection

## Open Questions

None - all clarifications resolved through systematic clarification process across 11 categories.

## References

- Constitution v1.0.0
- Feature Specification: specs/001-boot-sequence-splash/spec.md
- Clarification Files: specs/001-boot-sequence-splash/clarify/*.md (11 files)
- Avalonia 11.3 Documentation
- OpenTelemetry Semantic Conventions
- MVVM Community Toolkit 8.3 Patterns

---

*Research complete. Ready for Phase 1: Design & Contracts.*
