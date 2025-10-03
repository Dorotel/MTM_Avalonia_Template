# Offline and Sync Policy

> **Purpose:** Ensure uninterrupted workflow during network/server outages while maintaining data integrity.

---

## Human Context

- **When Used:**

  - Any operation requiring server contact
  - Always active on mobile
  - Desktop: as needed for intermittent connectivity

- **Dependencies:**

  - Connectivity service
  - Local queue/storage
  - API client
  - Clock

- **Consumers:**

  - Android (primary)
  - Desktop (secondary)

- **Priority:**
  - High (mobile)
  - Medium (desktop)

---

## AI Agent Context

- **Intent:**

  - Durable outbound queue for app-owned transactions
  - Visual data: inbound-only (read caches via API Toolkit projections)
  - Reconcile on reconnect using idempotency keys and conflict policy

- **Dependencies:**

  - Session state
  - Validation
  - Resilience policies
  - Serialization

- **Consumers:**

  - Receiving (app DB writes)
  - Moves/issues/adjustments

- **Non-Functionals:**

  - Power-aware
  - Crash-safe
  - Bounded memory/disk
  - Telemetry on retries

- **Priority:**
  - High

---

## Modes

### Online

- Read Visual via server projections (Toolkit-backed)
- Write app DB via API; confirm immediately
- **Cached Master Data:**
  - PART (30-char IDs)
  - LOCATION (15-char IDs)
  - WAREHOUSE (15-char IDs)
  - SHOP_RESOURCE (15-char IDs)
- **Relationships Validated:**
  - LOCATION→WAREHOUSE (FK Line: 427)
  - PART_LOCATION links (FK Lines: 459-460)

### Degraded

- Visual offline → use cached master
- Warn user; continue app writes queued
- Cache validation against last sync timestamp
- Aged data flagged with warnings

### Offline

- Queue app writes
- Restrict features that cannot validate safely
- Present clear banners
- Part/Location validation uses last cached snapshot from Visual

---

## Conflict Resolution

- **Server Authoritative:**
  - Client retries on 409 with user guidance
- **Idempotency:**
  - Client-generated keys to avoid duplicates

---

## Clarification Questions

> **Q:** Which operations are allowed fully offline?
> **Why:** Safety vs productivity
> **Suggested:**
>
> - Moves/issues with local validation
> - Receiving limited if master cache is fresh
>   **Reason:** Minimize risk while enabling work
>   **Options:**
> - [A] Moves only
> - [B] Moves + Issues
> - [C] Add Receiving
> - [D] Other: **\_\_**

---

<!--
Formatting aligns with project constitution:
- Section headers use ATX style (#, ##, ###)
- Lists use consistent indentation and spacing
- Semantic grouping for clarity
- No hardcoded values; all master data references use domain terms
- No legacy markdown syntax
-->
