# Outstanding Clarification Questions: Visual Server Integration & Cached-Only Mode - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Visual Server Integration Constraints** and **Cached-Only Mode Operation** features.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (6 total)

### Q1: [Integration] - Visual API Toolkit command validation

**Impact**: High
**Category**: Integration & External Dependencies
**Feature Context**: FR-151 access Visual via API Toolkit commands only

Should there be a whitelist of allowed API Toolkit commands?

| Option | Description |
|--------|-------------|
| A | Yes, explicit whitelist (security by default) |
| B | No, all read-only commands allowed |
| C | Whitelist during development, open in production |
| D | Configurable per deployment |

**Answer: A — Yes, explicit whitelist (security by default)**

Reasoning: Explicit whitelist provides defense in depth against malicious commands and accidental writes. Visual API Toolkit is powerful — unrestricted access risks data corruption. Whitelist maintenance burden is acceptable given security benefit. Define whitelist in configuration file (JSON/YAML) per deployment. Include: command name, allowed parameters, read-only flag. Reject unlisted commands with clear error. Log rejected attempts for security auditing. Review and expand whitelist during feature development; lock down in production.

---

### Q2: [Data Model] - Visual schema CSV dictionary update frequency

**Impact**: Medium
**Category**: Domain & Data Model
**Feature Context**: FR-152 reference Visual schema via CSV dictionary files

How often should Visual schema CSV files be updated?

| Option | Description |
|--------|-------------|
| A | Manually when Visual schema changes |
| B | Auto-fetch at startup (always current) |
| C | Auto-fetch weekly + manual trigger |
| D | Embedded in app (updated with app releases) |

**Answer: D — Embedded in app (updated with app releases)**

Reasoning: Schema dictionaries should be versioned with app code for consistency. Visual schema changes infrequently (quarterly at most). Embedding in app bundle ensures: (1) offline capability, (2) version compatibility (schema matches app expectations), (3) predictable behavior (no runtime failures from schema fetch issues). Update process: include schema CSV files in source control, regenerate from Visual database during release preparation, bundle in app assets. Provide admin diagnostic screen showing schema version and comparing to live Visual schema (detect drift).

---

### Q3: [Security] - Android API projection authentication

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-154 Android devices access Visual via API projections

How should Android devices authenticate to API projections?

| Option | Description |
|--------|-------------|
| A | User credentials (entered at login) |
| B | Device certificate |
| C | OAuth2/JWT tokens |
| D | API key per device |
| E | Combination: User credentials + device certificate |

**Answer: E — Combination: User credentials + device certificate**

Reasoning: Two-factor authentication provides robust security. User credentials (username/password) verify identity. Device certificate (stored in Android KeyStore) verifies device is authorized. Combined approach: user authenticates with credentials → receives short-lived JWT token → subsequent API calls use JWT + device certificate for mutual TLS. Device certificate provisioned during device enrollment (admin process). Allows per-device revocation without affecting user across all devices. Rotation: JWT expires after 8 hours, device certificate valid for 1 year.

---

### Q4: [Non-Functional] - Cached-only mode reconnection attempt interval

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-159 attempt reconnection at appropriate intervals

How frequently should reconnection be attempted?

| Option | Description |
|--------|-------------|
| A | Every 30 seconds (aggressive) |
| B | Every 2 minutes (balanced) |
| C | Every 5 minutes (conservative) |
| D | Exponential backoff: 30s → 1m → 2m → 5m → 10m |
| E | User-initiated only (manual retry) |

**Answer: D — Exponential backoff: 30s → 1m → 2m → 5m → 10m**

Reasoning: Exponential backoff balances recovery speed and resource usage. First retry at 30s catches brief network blips. Subsequent backoff reduces network traffic during prolonged outages. Cap at 10-minute interval prevents excessive wait. Reset backoff on successful reconnection or user navigation (indicates user is active). Show countdown timer in warning banner ("Retrying in 2:30..."). Provide manual "Retry Now" button for user control. Successful reconnection: show success toast, refresh stale data, resume normal operation.

---

### Q5: [User Experience] - Cached-only banner dismissal

**Impact**: Low
**Category**: Interaction & UX Flow
**Feature Context**: FR-156 display warning banner in cached mode

Can the cached-only warning banner be dismissed?

| Option | Description |
|--------|-------------|
| A | No, always visible in cached mode |
| B | Yes, dismissible but reappears on app restart |
| C | Yes, dismissible with "Don't show again" option |
| D | Dismissible, but reappears after configurable time |

**Answer: D — Dismissible, but reappears after configurable time**

Reasoning: Users need awareness but persistent banner causes banner blindness. Dismissible with 10-minute reappearance strikes balance. Users acknowledge limitation but aren't constantly annoyed. Banner reappears after 10 minutes or when user attempts write operation (reminder at point of impact). Reset dismissal timer on app restart (fresh reminder each session). Provide persistent visual indicator (small icon in status bar) even when banner dismissed. Clicking status icon re-expands banner with reconnection details.

---

### Q6: [Functional Scope] - Functionality limitations in cached-only mode

**Impact**: High
**Category**: Functional Scope & Behavior
**Feature Context**: FR-158 limit functionality when operating on cached data

What specific features should be disabled in cached-only mode?

| Option | Description |
|--------|-------------|
| A | All write operations (read-only mode) |
| B | All writes + real-time data queries |
| C | All writes + reports + data export |
| D | Minimal restrictions (warn but allow most operations) |
| E | Configurable per feature flag |

**Answer: B — All writes + real-time data queries**

Reasoning: Write operations are obviously blocked (no server to persist changes). Real-time queries also fail gracefully (e.g., "show current inventory levels" — cache is stale). Allow: browsing cached master data, viewing historical transactions (cached), local draft creation (queue for sync). Disable: transaction posting, master data edits, real-time lookups. Gray out disabled UI elements with "Requires connection" tooltip. Show queued drafts count in header ("3 drafts pending sync"). On reconnection, prompt user to review and sync queued items. Reports/exports work from cache (with staleness warning).

---
