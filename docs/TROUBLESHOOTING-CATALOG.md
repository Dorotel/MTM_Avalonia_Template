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
  - Check credentials (validated locally first); confirm Toolkit endpoint reachability; review allowlist; use cached master; notify IT.
- MySQL unavailable:
  - Confirm MAMP running and port; switch to API for tests; review firewall.
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