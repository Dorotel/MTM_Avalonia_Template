# Inventory API Specification — Android Access and Server Projections
This defines the server API Android uses for all app data and for read-only projections of Visual master data. It complements DATABASE-MYSQL-57-MAMP-SETUP.md and VISUAL-SERVER-INTEGRATION.md.

For humans
- Purpose: Provide a secure, stable contract for mobile clients; keep Visual credentials off devices; centralize validation and policy.
- When used: Android always; desktop optionally uses API for shared logic or remote operations.
- Dependencies: Server connectivity; app authentication; Visual read-only connectivity on the server (via API Toolkit).
- What depends on it: Android features (receiving, moves, counts, kitting), reporting dashboards.
- Priority: Critical for Android viability.

For AI agents
- Intent: Define versioned REST endpoints for: items, locations, work centers, transactions, sequences, and Visual projections. Enforce read-only for Visual (Toolkit-only), and strict validation for app writes.
- Dependencies: Serialization/contract service, mapping, validation, authentication, rate limiting.
- Consumers: Android client, integration tests, admin tools.
- Non-functionals: Idempotent GETs, safe retry semantics, pagination, consistent error shapes, telemetry correlation IDs.
- Priority: High.

Endpoint categories
- Auth
  - Login: issue session token; server maps user to Visual read-only context (no device owns DB creds).
  - Refresh: rotate tokens; revoke on logout.
- Visual projections (read-only; Toolkit-backed only)
  - GET /api/visual/items?query=...&page=...&size=...
    - Returns: id (max 30 chars), description (max 255 chars), stockUm (max 15 chars), activeFlag (1 char)
    - Source: PART table (Visual Data Table.csv Lines: 5779-5888)
  - GET /api/visual/locations?query=...&page=...&size=...
    - Returns: id (max 15 chars), warehouseId (max 15 chars), description (max 80 chars)
    - Source: LOCATION table (Visual Data Table.csv Lines: 5246-5263)
  - GET /api/visual/warehouses?warehouseId=...
    - Returns: id (max 15 chars), description (max 50 chars), siteId (max 15 chars)
    - Source: WAREHOUSE table (Visual Data Table.csv Lines: 14262-14288)
  - GET /api/visual/workcenters?query=...&page=...&size=...
    - Returns: id (max 15 chars), description (max 50 chars), activeFlag (1 char)
    - Source: SHOP_RESOURCE table (Visual Data Table.csv Lines: 9299-9343)
  - Constraints: Sources defined by Visual CSV dictionaries (MTMFG Tables.csv: 14796 fields, Visual Data Table.csv: 14776 field definitions, MTMFG Relationships.csv: 1266 FK relationships); citations required: Reference-{File Name} - {Chapter/Section/Page}. Never expose raw tables directly; never execute direct SQL.
- Operational data (app-owned, MySQL)
  - GET/POST transactions (receive/move/issue/adjust) with server-side validation against Visual projections.
  - GET sequences (next IDs), audit logs, user settings.
- Health and meta
  - GET /api/health (VisualOnline, MySqlOnline, CacheFreshness).
  - GET /api/version.

Contracts and behaviors
- JSON camelCase; explicit DTO schemas; timestamps in UTC.
- Standard error envelope (code, message, traceId).
- Pagination with opaque cursors or page/size; default limits to protect server.
- Strict input validation (quantities, codes, lot formats); descriptive error messages.
- Compliance: No Visual writes; audit all transaction writes (app DB) with user and reason codes; Toolkit-only for Visual reads.

Clarification questions
- Q: Preferred pagination style?
  - Why: Impacts client implementation and server performance.
  - Suggested: Page/size initially; migrate to cursor when needed.
  - Reason: Simpler for v1; scalable later.
  - Options: [A] Page/size [B] Cursor [C] Both
- Q: Do we require TLS even on LAN in production?
  - Why: Protects credentials and data.
  - Suggested: Yes, HTTPS everywhere.
  - Reason: Security baseline.
  - Options: [A] HTTP dev only [B] HTTPS dev+prod [C] HTTPS prod only