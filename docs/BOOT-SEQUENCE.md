# Boot Sequence — Splash-First, Services Initialization Order
This document summarizes the startup sequence your app must follow. It preserves the rule: the splash screen is theme-less and uses no app services; its only job is to initialize them.

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
  4) Run diagnostics (permissions, paths, available storage, camera availability on Android).
  5) Reachability checks:
     - Network available (if needed).
     - Visual API Toolkit endpoint reachable with read-only context; do not write. Cite toolkit references when used: Reference-{File Name} - {Chapter/Section/Page}.
     - MySQL 5.7 reachable (desktop) or API host reachable (Android).
  6) Initialize data layers:
     - Desktop: MySQL client (app DB); Visual API Toolkit client (read-only).
     - Android: API client for app data; API projections for Visual data (devices NEVER connect to Visual DB directly).
  7) Initialize core services (config façade snapshot, secrets façade, message bus, clock/scheduler, validation, mapping, serialization, connectivity).
  8) Caching/prefetch (Visual master data where flagged; Items, Locations, WorkCenters; respect time limits and maintenance windows; read-only via API Toolkit).
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
  - Suggested: Allow cached-only mode with a clear banner.
  - Reason: Maintains productivity while signaling risk.
  - Options: [A] Block until online [B] Allow cached-only mode [C] Always online-required