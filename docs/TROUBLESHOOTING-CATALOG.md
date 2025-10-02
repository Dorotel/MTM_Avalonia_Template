# Troubleshooting Catalog
Common problems, symptoms, and fixes.

For humans
- Purpose: Speed up resolution on the floor and for IT.
- When used: When something breaks or behaves strangely.
- Dependencies: Logging, health checks, diagnostics.
- What depends on it: Support efficiency.
- Priority: Medium-High.

For AI agents
- Intent: Provide a symptom→cause→action table for Visual Toolkit connectivity, MySQL/API issues, scanning, printing, and caches.
- Dependencies: Telemetry, health endpoints.
- Consumers: Support teams, operators.
- Non-functionals: Clear language, short steps, link to detailed logs.
- Priority: Medium-High.

Sections
- Visual offline or Toolkit error:
  - Check credentials (APPLICATION_USER.NAME nvarchar(20), Visual Data Table.csv Line: 565) validated locally first
  - Confirm Toolkit endpoint reachability; review allowlist (PART: 30-char max, LOCATION: 15-char max)
  - Use cached master data (last sync timestamp shown in banner)
  - Verify read-only access: no writes to Visual tables (PART, LOCATION, WAREHOUSE, SHOP_RESOURCE, SITE)
  - Check foreign key relationships cache: LOCATION→WAREHOUSE (FK Line: 427), PART_LOCATION (FK Lines: 459-460)
  - Notify IT with specific table/field reference if projection fails
- MySQL unavailable:
  - Confirm MAMP running and port (default 3306); check MySQL service status
  - Switch to API for tests; review firewall rules
  - Verify local app database (not Visual) is the target
- Android can’t reach API:
  - Use 10.0.2.2 for emulator; check TLS certificates; firewall rules.
- Scanning fails:
  - Confirm symbology; camera permission; ambient lighting; hardware status.
- Printing fails:
  - Printer online; correct language (ZPL); driver settings; test-print template.

Clarification questions
- Q: Allow operators to send diagnostic bundles to IT?
  - Why: Faster support.
  - Suggested: Yes, with consent and redaction.
  - Reason: Balance privacy and speed.
  - Options: [A] Yes [B] No