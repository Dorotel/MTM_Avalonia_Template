# Boot Sequence — Splash-First, Services Initialization Order

This document defines the complete startup sequence and service initialization architecture for the application.

## Features/Implementations Covered by This Document

> **Spec-Kit Integration:**
> When using `/specify` with this file, generate implementation specifications for **all** features below.
> **Reference:** [constitution.md v1.1.0](../.specify/memory/constitution.md)

---

### 1. Splash Screen (Theme-less Loading UI)

- Pre-initialization display with progress states
- Exception handling UI
- User-cancellable operations (Retry/Exit)
- Progress percentage (0–100%) and time estimates
- Animated branding (`\Resources\Images\MTM_Logo_web.png`)
- Stage visualization (Stage 0 → Stage 1 → Stage 2)
- Accessibility: Screen reader announces progress
- Dark mode support (system preference)

### 2. Configuration Service

- Environment detection (Dev/Staging/Prod)
- Feature flags loading/management
- Layered config with override precedence
- Hot reload (feature flag changes)
- Validation on load (clear errors)
- Multiple profiles (e.g., "Development-FastCache")
- Auto-detect environment (hostname/IP/path)
- Config audit log (timestamp/source)
- Encrypted config sections
- Remote config support

### 3. Secrets Management Service

- Secure username/password loading (never logged/committed)
- Credential encryption at rest
- Secrets façade for app-wide access

### 4. Logging and Telemetry Service

- Console/file sinks initialization
- Command-line log level filtering:
  - `--ErrorLogOnly`, `--WarningAndAbove`, `--InfoAndAbove`, `--DebugAll`
- Category/namespace filtering:
  - `--LogCategory=ReceivingView,InventoryService`
  - `--LogNamespace=MTM_Template_Application.Views.Receiving`
  - `--ExcludeCategory=BootSequence,Cache`
- Remote telemetry endpoints (optional)
- Boot metrics persistence
- Structured logging (JSON)
- Correlation IDs, performance metrics
- Log buffering, rolling file policy
- Remote sink fallback, sensitive data redaction
- Sampling, context enrichment

### 5. Diagnostics Service

- Permissions verification
- Path/storage checks
- Camera detection (Android)
- Network reachability
- Benchmark tests (startup performance)
- Hardware detection (CPU/RAM/screen)
- Compatibility checks (.NET/runtime/deps)
- Previous crash detection
- Self-healing for common issues

### 6. Data Layer Initialization

- MySQL client setup (desktop)
- Visual API Toolkit client (read-only)
- HTTP API client (Android)
- Connection pooling/retry policies
- Health monitoring, circuit breaker
- Bulk operations, query caching
- Lazy initialization, multi-database support
- Read-only enforcement

### 7. Core Application Services

- Config snapshot service
- Message bus/event aggregator
- Clock/scheduler, validation, mapping, serialization
- Connectivity monitoring
- Priority messages, dead letter queue, replay
- Validation rule discovery
- Multiple mapping profiles
- Serialization versioning
- Smart retry, service health checks
- Graceful degradation

### 8. Visual Master Data Cache Service

- Prefetch: Items/Parts, Locations, Warehouses, Work Centers
- Cache refresh/maintenance policies
- Age tracking, staleness indicators
- Incremental updates (delta sync)
- Compression (LZ4)
- Cache warming priorities
- Background refresh, versioning
- Smart expiry, preemptive refresh
- Cache statistics

### 9. Localization Service

- Culture setup, format providers
- Resource dictionary loading
- User preference storage
- Runtime language switch
- Missing translation fallback/logging
- Pluralization, date/time/number formatting

### 10. Navigation Service

- Route registration, shell construction
- Home navigation trigger
- Navigation history, deep linking
- Navigation guards, breadcrumb trail
- Tab/window management
- Navigation analytics, gesture navigation

### 11. Theme Service

- Theme resource loading (post-splash)
- Light/Dark/Auto mode selection
- High contrast mode, theme persistence
- Theme scheduling, preview, per-view theming

### 12. Boot Orchestrator

- Stage management (Stage 0 → Stage 1 → Stage 2)
- Strict initialization order, timeout enforcement
- Cancellation support, progress reporting
- Parallel/conditional steps, watchdog timer
- Boot analytics, DI for all services
- Unit/integration tests for boot sequence
- Performance/memory budgets
- Monitoring dashboard, feature toggles
- Service documentation, metrics export

### 13. Failure Handling Service

- Credential validation failures
- Endpoint unreachability
- User-friendly error messages
- Retry/Skip/Exit logic
- Diagnostic bundle creation
- Error categorization/recovery
- User guidance, offline documentation
- Error reporting, graceful degradation map
- Fallback modes, historical error tracking

---

## Startup Stages

| Stage       | Description                                                                                                                                                                                                                                     |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Stage 0** | Pre-init: Theme-less splash, basic exception trap                                                                                                                                                                                               |
| **Stage 1** | Initialization (strict order):<br>1. Config/env<br>2. Secrets<br>3. Logging/telemetry<br>4. Diagnostics<br>5. Reachability<br>6. Data layers<br>7. Core services<br>8. Caching/prefetch<br>9. Localization<br>10. Navigation/theming (deferred) |
| **Stage 2** | Transition: Hide splash, load theme/resources, construct shell, navigate home, persist boot metrics                                                                                                                                             |

---

## Visual Server Constraints

- **Read-only access only**; no DML
- Use current user’s Visual credentials (validated locally)
- Access Visual via API Toolkit commands only
- Reference Visual schema via CSV dictionary/toolkit docs
- Android: API projections only (never direct DB)

---

## Failure Behavior

- Fail fast on invalid credentials/unreachable endpoints (Retry/Exit)
- Simple messages for operators; detailed logs for diagnostics
- Manual “Skip prefetch” only when safe
- Block until caches/minimum references ready

---

## Clarification

> **Q:** Block launch until Visual is reachable, or allow cached-only mode?
> **A:** **Allow cached-only mode** > **Reason:** Maintains productivity; display banner when operating in cached-only mode.

---

## Compliance Notes

- All formatting, headings, and lists follow [constitution.md](../.specify/memory/constitution.md) and VS Code extension standards.
- Use semantic headings, bullet lists, and code blocks as shown.
- All features must be referenced in `/specify`, `/plan`, `/tasks` workflows.
- All services must be testable, DI-compliant, and support cancellation tokens where applicable.
