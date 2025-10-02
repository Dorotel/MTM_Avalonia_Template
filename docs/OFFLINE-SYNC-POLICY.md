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
  - Visual master data cached: PART (30-char IDs), LOCATION (15-char IDs), WAREHOUSE (15-char IDs), SHOP_RESOURCE (15-char IDs)
  - Relationships validated: LOCATION→WAREHOUSE (FK Line: 427), PART_LOCATION links (FK Lines: 459-460)
- Degraded: Visual offline → use cached master; warn user; continue app writes queued.
  - Cache validation against last sync timestamp; aged data flagged with warnings
- Offline: Queue app writes; restrict features that cannot validate safely; present clear banners.
  - Part/Location validation uses last cached snapshot from Visual

Conflict resolution
- Server authoritative; client retries on 409 with user guidance.
- Idempotency with client-generated keys to avoid duplicates.

Clarification questions
- Q: Which operations are allowed fully offline?
  - Why: Safety vs productivity.
  - Suggested: Moves/issues with local validation; receiving limited if master cache is fresh.
  - Reason: Minimize risk while enabling work.
  - Options: [A] Moves only [B] Moves+Issues [C] Add Receiving [D] Other: ______