# Inventory API Specification — Android Access & Server Projections

> **Purpose:** Secure, stable contract for mobile clients; keeps Visual credentials off devices; centralizes validation and policy.
> **Usage:** Android always; desktop optionally for shared logic/remote ops.
> **Dependencies:** Server connectivity, app authentication, Visual read-only connectivity (API Toolkit).
> **Consumers:** Android features (receiving, moves, counts, kitting), reporting dashboards.
> **Priority:** **Critical** for Android viability.

---

## API Principles

- **Cross-Platform First:** All endpoints must work for Android and desktop clients.
- **MVVM DTO Contracts:** All DTOs follow explicit schemas, camelCase, UTC timestamps.
- **Null Safety:** All responses and errors must be resilient to nulls.
- **Compiled Contracts:** DTOs versioned; never expose raw tables or direct SQL.
- **Test-First:** All endpoints covered by xUnit integration tests.
- **Security:** No Visual writes; all transaction writes audited; HTTPS required in production.
- **Error Envelope:** Standard shape `{ code, message, traceId }`.

---

## Endpoint Categories

### Auth

- `POST /api/auth/login`
  Issues session token; server maps user to Visual read-only context (no device DB creds).
- `POST /api/auth/refresh`
  Rotates tokens; revokes on logout.

### Visual Projections (Read-Only, Toolkit-Backed)

- `GET /api/visual/items?query=...&page=...&size=...`
  Returns:
  - `id` (max 30 chars)
  - `description` (max 255 chars)
  - `stockUm` (max 15 chars)
  - `activeFlag` (1 char)
  Source: PART table (Visual Data Table.csv, Lines 5779-5888)

- `GET /api/visual/locations?query=...&page=...&size=...`
  Returns:
  - `id` (max 15 chars)
  - `warehouseId` (max 15 chars)
  - `description` (max 80 chars)
  Source: LOCATION table (Visual Data Table.csv, Lines 5246-5263)

- `GET /api/visual/warehouses?warehouseId=...`
  Returns:
  - `id` (max 15 chars)
  - `description` (max 50 chars)
  - `siteId` (max 15 chars)
  Source: WAREHOUSE table (Visual Data Table.csv, Lines 14262-14288)

- `GET /api/visual/workcenters?query=...&page=...&size=...`
  Returns:
  - `id` (max 15 chars)
  - `description` (max 50 chars)
  - `activeFlag` (1 char)
  Source: SHOP_RESOURCE table (Visual Data Table.csv, Lines 9299-9343)

> **Constraints:**
> - Sources defined by Visual CSV dictionaries
> - Citations required: `Reference-{File Name} - {Chapter/Section/Page}`
> - Never expose raw tables or direct SQL

### Operational Data (App-Owned, MySQL)

- `GET/POST /api/transactions`
  Receive/move/issue/adjust with server-side validation against Visual projections.
- `GET /api/sequences`
  Next IDs.
- `GET /api/audit-logs`
  Audit logs.
- `GET /api/user-settings`
  User settings.

### Health & Meta

- `GET /api/health`
  Returns: `VisualOnline`, `MySqlOnline`, `CacheFreshness`
- `GET /api/version`

---

## Contracts & Behaviors

- **JSON:** camelCase, explicit DTO schemas, UTC timestamps.
- **Errors:** Standard envelope `{ code, message, traceId }`.
- **Pagination:** Page/size (default), opaque cursors for future scalability.
- **Validation:** Strict input validation (quantities, codes, lot formats); descriptive error messages.
- **Compliance:**
  - No Visual writes
  - Audit all transaction writes (app DB) with user/reason codes
  - Toolkit-only for Visual reads

---

## Clarification Questions

- **Preferred Pagination Style?**
  - **Suggested:** Page/size initially; migrate to cursor when needed.
  - **Reason:** Simpler for v1; scalable later.
  - **Options:** [A] Page/size [B] Cursor [C] Both

- **TLS Requirement (Production)?**
  - **Suggested:** Yes, HTTPS everywhere.
  - **Reason:** Security baseline.
  - **Options:** [A] HTTP dev only [B] HTTPS dev+prod [C] HTTPS prod only

---

> **Formatting & Compliance:**
> - All DTOs and endpoints must be documented with explicit field constraints and source citations.
> - All code and contracts must follow the [MTM Avalonia Template Constitution](../.specify/memory/constitution.md) v1.1.0.
> - All changes require test coverage and constitutional review.
