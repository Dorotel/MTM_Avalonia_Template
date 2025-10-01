# Offline and Sync Policy
Defines how the app behaves when the network or servers are unavailable.

For humans
- Purpose: Keep work moving during outages while protecting data integrity.
- When used: Any operation that needs server contact; always on mobile; desktop as needed.
- Dependencies: Connectivity service, local queue/storage, API client, clock.
- What depends on it: Android especially; desktop for intermittent connectivity.
- Priority: High (mobile), Medium (desktop).

For AI agents
- Intent: Provide a durable outbound queue for app-owned transactions; Visual data remains inbound-only (read caches via API Toolkit projections). Reconcile on reconnect with idempotency keys and conflict policy.
- Dependencies: Session state, validation, resilience policies, serialization.
- Consumers: Receiving (app DB writes), moves/issues/adjustments.
- Non-functionals: Power-aware, crash-safe, bounded memory/disk, telemetry on retries.
- Priority: High.

Modes
- Online: Read Visual via server projections (Toolkit-backed); write app DB via API; confirm immediately.
- Degraded: Visual offline → use cached master; warn user; continue app writes queued.
- Offline: Queue app writes; restrict features that cannot validate safely; present clear banners.

Conflict resolution
- Server authoritative; client retries on 409 with user guidance.
- Idempotency with client-generated keys to avoid duplicates.

Clarification questions
- Q: Which operations are allowed fully offline?
  - Why: Safety vs productivity.
  - Suggested: Moves/issues with local validation; receiving limited if master cache is fresh.
  - Reason: Minimize risk while enabling work.
  - Options: [A] Moves only [B] Moves+Issues [C] Add Receiving [D] Other: ______