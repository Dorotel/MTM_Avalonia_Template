# Implementation Order (TOC) for Your Current Files

Audience: practical, step‑by‑step order so you always build foundations first. Each step says why it’s next, what it depends on, and what “done” looks like.

Major rules to keep in mind
- Splash-first: Start with a plain loading screen that initializes services; no themes/services used by the splash itself.
- Infor Visual ERP: Read‑only via API Toolkit. Never write to Visual. Login uses the current user’s Visual username/password (validated locally in MAMP).
- Android: Never connects to Visual directly (use your API projections). Desktop may consume Visual read-only data via Toolkit if IT permits.
- Spec-driven: Every feature later references acceptance criteria (AC IDs) and traceability.

Phase 1 — Absolute Foundations
1) docs/BOOT-SEQUENCE.md
2) docs/ENVIRONMENTS-AND-CONFIG.md
3) docs/SECURITY-COMPLIANCE-POLICY.md

Phase 2 — Data and Visual Boundaries
4) docs/DATA-CONTRACTS.md
5) docs/VISUAL-WHITELIST.md
6) docs/VISUAL-CREDENTIALS-FLOW.md

Phase 3 — Platform Access and Reliability
7) docs/API-SPECIFICATION.md
8) docs/OFFLINE-SYNC-POLICY.md

Phase 4 — UX, Scanning, Roles
9) docs/UI-UX-GUIDELINES.md
10) docs/BARCODE-AND-LABELING-STANDARDS.md
11) docs/ROLES-AND-PERMISSIONS.md

Phase 5 — Quality, Traceability, Support
12) docs/TEST-STRATEGY-AND-TRACEABILITY.md
13) docs/TROUBLESHOOTING-CATALOG.md
14) docs/COPILOT-ASSETS-CHECKLIST.md
15) docs/SPECIFY-CHECKLIST.md

Phase 6 — Feature Docs (run /specify)
16) Generate feature specs under docs/specs/features/* using the prompt library.