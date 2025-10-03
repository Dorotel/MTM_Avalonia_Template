# Boot Sequence — Splash-First, Services Initialization Order

This document defines the complete startup sequence and service initialization architecture for the application.

## Features/Implementations Covered by This Document

When using `/specify` with this file, generate implementation specifications for ALL of the following:

1. **Splash Screen (Theme-less Loading UI)**
   - Pre-initialization display with progress states
   - Basic exception handling UI
   - User-cancellable operations with Retry/Exit options
   - Add progress percentage: Show actual percentage (0-100%) alongside progress states for better user feedback
   - Time estimates: Display estimated time remaining for long operations (e.g., "Caching Visual data... ~30s remaining")
   - Animated branding: Consider subtle logo animation or pulse effect to show app is alive (keep minimal for performance) use \Resources\Images\MTM_Logo_web.png
   - Stage visualization: Show visual indicator of current stage (Stage 0 → Stage 1 → Stage 2) with checkmarks
   - Accessibility: Ensure screen reader announces progress updates for visually impaired users
   - Dark mode support: Even though theme-less, respect system dark mode preference for splash background

2. **Configuration Service**
   - Environment detection (Development/Staging/Production)
   - Feature flags loading and management
   - Configuration layering and override precedence
   - Hot reload: Support configuration reload without restart (useful for feature flag changes in production)
   - Configuration validation: Validate all config on load with clear error messages for malformed values
   - Configuration profiles: Support multiple profiles (e.g., "Development-FastCache", "Development-SlowNetwork")
   - Environment auto-detection: Detect environment from hostname, IP range, or deployment path as fallback
   - Config audit log: Track all config changes with timestamp and source (file/environment/override)
   - Encrypted sections: Allow encryption of sensitive config sections (not just secrets)
   - Remote configuration: Option to pull config from central server for easier multi-site management

3. **Secrets Management Service**
   - Visual username/password secure loading (never logged, never committed)
   - Credential encryption at rest
   - Secrets façade for application-wide access

4. **Logging and Telemetry Service**
   - Console/file sinks initialization
   - Command-line argument-based log level filtering:
     - `--ErrorLogOnly`: Show only Error level logs
     - `--WarningAndAbove`: Show Warning, Error, and Critical logs
     - `--InfoAndAbove`: Show Info, Warning, Error, and Critical logs
     - `--DebugAll`: Show all logs including Debug and Trace
     - No argument (default): Show all logs (Debug, Info, Warning, Error, Critical)
   - Category-based filtering for feature-specific development:
     - `--LogCategory=ReceivingView`: Only show logs from ReceivingView and related classes
     - `--LogCategory=ReceivingView,InventoryService`: Multiple categories (comma-separated)
     - `--LogNamespace=MTM_Template_Application.Views.Receiving`: Filter by namespace
     - Can combine with log level: `--InfoAndAbove --LogCategory=ReceivingView`
   - Category exclusion support:
     - `--ExcludeCategory=BootSequence,Cache`: Hide logs from already-implemented features
   - Optional remote telemetry endpoints
   - Boot metrics persistence (success/failure, durations)
   - Log level and categories can be changed via arguments without recompilation
   - Examples:
     - Development of new view: `--DebugAll --LogCategory=NewFeatureView`
     - Focus on one area: `--LogNamespace=MTM_Template_Application.ViewModels.Inventory`
     - Exclude noise: `--ExcludeCategory=Navigation,Theme,Cache`
   - Structured logging: Support JSON structured logs for better log aggregation (Seq, ELK, Splunk)
   - Correlation IDs: Automatic correlation ID generation for tracking requests across services
   - Performance metrics: Built-in performance counters (method execution time, memory usage)
   - Log buffering: Buffer logs during high-volume operations to prevent I/O bottleneck
   - Rolling file policy: Auto-rotate log files by size/date with configurable retention
   - Remote sink fallback: If remote telemetry fails, queue locally and retry
   - Sensitive data redaction: Auto-redact passwords, tokens, PII from logs
   - Sampling: Support log sampling (log every Nth occurrence) for high-frequency events
   - Context enrichment: Automatically add user, session, version to all logs

5. **Diagnostics Service**
   - Permissions verification
   - Path and storage availability checks
   - Camera availability detection (Android)
   - Network reachability checks
   - Benchmark tests: Run quick performance benchmarks on startup to detect slow hardware
   - Network quality check: Measure latency/bandwidth to Visual/API endpoints
   - Hardware detection: Detect CPU cores, RAM, screen resolution for optimization hints
   - Compatibility checks: Verify .NET runtime version, required dependencies present - Application will be built as self-contained
   - Previous crash detection: Detect if app crashed last time and offer to skip problematic features
   - Self-healing: Attempt automatic fixes for common issues (recreate missing folders, reset corrupt cache)

6. **Data Layer Initialization**
   - MySQL client setup (desktop app DB)
   - Visual API Toolkit client (read-only, desktop)
   - HTTP API client (Android for app data and Visual projections)
   - Connection pooling and retry policies
   - Connection health monitoring: Continuous health checks with automatic reconnection
   - Circuit breaker pattern: Prevent cascading failures when Visual/MySQL is down
   - Connection pooling metrics: Track pool utilization, connection lifespan
   - Bulk operations support: Optimize for batch Visual API calls (fetch multiple items at once)
   - Query caching: Cache frequently-accessed Visual queries at data layer level
   - Lazy initialization: Option to defer data layer init until first use (faster startup)
   - Multi-database support: Handle multiple MySQL databases (app DB, archive DB)
   - Read-only connection enforcement: Prevent accidental writes by using read-only connection strings

7. **Core Application Services**
   - Config façade snapshot service
   - Message bus / event aggregator
   - Clock/scheduler service
   - Validation service
   - Mapping service (DTOs to domain entities)
   - Serialization service (JSON/XML)
   - Connectivity monitoring service
   - Message bus features: Add support for priority messages, dead letter queue, message replay
   - Validation rule discovery: Auto-discover validation rules via attributes or conventions
   - Mapping profiles: Support multiple mapping profiles for different contexts (API vs UI)
   - Serialization versioning: Handle schema versioning for backward compatibility
   - Connectivity smart retry: Exponential backoff with jitter for retries
   - Service health checks: Each service exposes health endpoint for monitoring
   - Graceful degradation: Services continue functioning at reduced capacity when dependencies fail

8. **Visual Master Data Cache Service**
   - Items/Parts prefetch (PART table: 30-char ID, 255-char description)
   - Locations prefetch (LOCATION table: 15-char ID, warehouse FK)
   - Warehouses prefetch (WAREHOUSE table: 15-char ID, 50-char description)
   - Work Centers prefetch (SHOP_RESOURCE table: 15-char ID, 50-char description)
   - Cache refresh policies and maintenance windows
   - Cache age tracking and staleness indicators
   - Incremental updates: Only fetch changed records since last sync (delta sync)
   - Compression: Compress cached data to reduce memory footprint
   - Cache warming priorities: Load critical data first (Parts before Warehouses)
   - Background refresh: Refresh cache in background without blocking UI
   - Cache versioning: Track cache schema version for migration support
   - Smart expiry: Different TTL for different data types (Parts: 1 hour, Locations: 24 hours)
   - Preemptive refresh: Refresh cache before expiry based on usage patterns
   - Cache statistics: Track hit/miss rates, staleness, refresh durations

9. **Localization Service**
   - Culture setup
   - Format providers (date, number, currency)
   - Resource dictionary loading
   - User preference storage: Remember user's language/format choice
   - Runtime language switch: Allow changing language without restart
   - Missing translation handling: Fall back to English, log missing translations
   - Pluralization support: Handle pluralization rules properly (1 item vs 2 items)
   - Date/time formatting: Support relative times ("2 hours ago") and local time zones
   - Number formatting: Handle different decimal separators, thousand separators

10. **Navigation Service**
    - Route registration
    - Shell construction
    - Home navigation trigger
    - Navigation history: Track navigation history for back/forward navigation
    - Deep linking: Support app:// URLs to navigate directly to specific screens
    - Navigation guards: Validate navigation (prevent leaving unsaved changes)
    - Breadcrumb trail: Show current location in app hierarchy
    - Tab/window management: Support multiple tabs or windows (desktop)
    - Navigation analytics: Track most-used screens and navigation paths
    - Gesture navigation: Support swipe-back gesture on Android

11. **Theme Service**
    - Theme resource loading (deferred until after splash)
    - Light/Dark/Auto mode selection
    - High contrast mode: Support Windows high contrast mode for accessibility
    - Theme persistence: Remember user's theme choice across sessions
    - Theme scheduling: Auto-switch to dark mode at night - Toggle based on system settings
    - Theme preview: Show theme preview before applying
    - Per-view theming: Allow certain views to override theme (e.g., always dark for media)

12. **Boot Orchestrator**
    - Stage management (Stage 0 → Stage 1 → Stage 2)
    - Strict initialization ordering
    - Timeout enforcement
    - Cancellation support
    - Progress reporting to splash screen
    - Parallel stage execution: Execute independent steps in parallel for faster boot
    - Conditional steps: Skip optional steps based on feature flags or configuration
    - Watchdog timer: Detect hung boot steps and force timeout
    - Boot analytics: Track which steps take longest, identify bottlenecks
    - Dependency injection: Use proper DI container for all services (Microsoft.Extensions.DependencyInjection)
    - Unit testing: Ensure all services are unit-testable with mock dependencies
    - Integration tests: Automated tests for boot sequence with various failure scenarios
    - Performance budget: Set max boot time targets (e.g., <3s for stage 1)
    - Memory budget: Set max memory usage targets for startup
    - Monitoring dashboard: Admin panel showing status of all services
    - Feature toggles: Runtime enable/disable of services for troubleshooting
    - Service documentation: Generate API documentation from service interfaces
    - Metrics export: Export boot metrics to Prometheus/Grafana for monitoring

13. **Failure Handling Service**
    - Credential validation failures
    - Endpoint unreachability handling
    - User-friendly error message translation
    - Retry/Skip/Exit decision logic
    - Diagnostic bundle creation
    - Error categorization: Classify errors (transient, permanent, configuration, network)
    - Automatic recovery: Attempt automatic recovery for transient errors
    - User guidance: Provide step-by-step recovery instructions for each error type
    - Offline documentation: Include offline help for common errors (no network needed)
    - Error reporting: One-click error report to support with diagnostic bundle
    - Graceful degradation map: Document what features work in each failure scenario
    - Fallback modes: "Safe Mode" that disables advanced features to allow basic operation
    - Historical error tracking: Track error frequency to identify chronic issues

For humans

- Purpose: Ensure the app always starts cleanly and predictably. If anything fails, the splash clearly communicates the problem and offers retry or exit.
- When used: Every app launch, before the main UI appears.
- Dependencies: None for the splash itself; it initializes all dependencies listed below.
- What depends on it: All features (receiving, moves, counts, etc.) rely on initialized services and warmed caches.
- Priority: Critical. Never bypass this sequence.

For AI agents

- Intent: Orchestrate a deterministic, cancellable boot process, with strict ordering and telemetry. No UI themes or DI access on the splash until Stage 1 completes.
- Dependencies: OS platform primitives only in Stage 0; configuration/secrets/logging/etc. begin in Stage 1.
- Consumers: All core services, data layers, and feature modules.
- Non-functionals: Idempotent, timeout-aware, user-cancellable, low-IO contention, minimal memory overhead.
- Priority: Critical. Must run on every process start.

Startup stages

- Stage 0 (Pre-init, no services):
  - Show theme-less splash with progress states (not bound to services).
  - Wire basic exception trap to show a simple error if Stage 1 fails catastrophically.
- Stage 1 (Initialization, strict order):
  1) Load configuration and environment (Development/Staging/Production, feature flags).
  2) Load secrets (Visual username/password for current user in dev; never log; never commit).
  3) Initialize logging/telemetry sinks (console/file; optional remote).
     - Parse command-line arguments for log level filtering:
       - `--ErrorLogOnly`: Error level only
       - `--WarningAndAbove`: Warning+ levels
       - `--InfoAndAbove`: Info+ levels (production default)
       - `--DebugAll`: All levels including Debug/Trace
       - No argument: All levels (development default)
     - Parse category/namespace filtering arguments:
       - `--LogCategory=CategoryName`: Include only specified categories
       - `--LogNamespace=Full.Namespace.Path`: Include only specified namespaces
       - `--ExcludeCategory=CategoryName`: Exclude specified categories
       - Multiple values supported (comma-separated)
     - Configure sinks with specified filter level and category filters
     - Support combining level and category filters for precise control
     - Examples:
       - Working on ReceivingView: `--DebugAll --LogCategory=ReceivingView`
       - Exclude boot noise: `--ExcludeCategory=BootSequence,Cache,Navigation`
       - Focus on ViewModels: `--LogNamespace=MTM_Template_Application.ViewModels`
  4) Run diagnostics (permissions, paths, available storage, camera availability on Android).
  5) Reachability checks:
     - Network available (if needed).
     - Visual API Toolkit endpoint reachable with read-only context; do not write. Cite toolkit references when used: Reference-{File Name} - {Chapter/Section/Page}.
     - MySQL 5.7 reachable (desktop) or API host reachable (Android).
  6) Initialize data layers:
     - Desktop: MySQL client (app DB); Visual API Toolkit client (read-only).
     - Android: API client for app data; API projections for Visual data (devices NEVER connect to Visual DB directly).
  7) Initialize core services (config façade snapshot, secrets façade, message bus, clock/scheduler, validation, mapping, serialization, connectivity).
  8) Caching/prefetch (Visual master data where flagged; respect time limits and maintenance windows; read-only via API Toolkit).
     - Items: PART table (30-char ID, 255-char description) - Visual Data Table.csv Lines: 5779-5888
     - Locations: LOCATION table (15-char ID, 15-char warehouse FK) - Visual Data Table.csv Lines: 5246-5263, Relationships FK Line: 427
     - Warehouses: WAREHOUSE table (15-char ID, 50-char description) - Visual Data Table.csv Lines: 14262-14288
     - WorkCenters: SHOP_RESOURCE table (15-char ID, 50-char description) - Visual Data Table.csv Lines: 9299-9343
  9) Localization/culture setup; format providers.
  10) Register navigation, theming resources (deferred until after splash).
- Stage 2 (Transition):
  - Hide splash, load theme/resources, construct root shell, navigate to home.
  - Persist boot metrics (success/failure, durations) for diagnostics.

Visual server constraints (re-affirmed)

- Read-only access only; no DML of any kind.
- Use current user’s Visual credentials for login (validated locally against MAMP MySQL before any attempt).
- Access Visual via API Toolkit commands only; never direct SQL.
- Reference Visual schema via CSV dictionary files (MTMFG Tables/Relationships/Procedures, Visual Data Table) and toolkit documentation citations.
- Android never connects directly to Visual DB; always via API projections.

Failure behavior

- Fail fast on invalid credentials, unreachable endpoints (with Retry/Exit).
- Provide simple, non-technical messages for operators; detailed telemetry in logs.
- Allow manual “Skip prefetch” only when safe; otherwise block until caches or minimum viable references are ready.

Clarification questions

- Q: Should the app block launch until Visual is reachable, or allow cached-only mode?
  - Why: Decides whether operators can continue when ERP is offline.
  - **Answer: [B] Allow cached-only mode**
  - Reason: Maintains productivity while signaling risk. Display a clear banner when operating in cached-only mode to inform users that Visual data may be stale.
