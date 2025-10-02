# Implementation Order (TOC) for Your Current Files# Implementation Order (TOC) for Your Current Files



Audience: practical, step‑by‑step order so you always build foundations first. Each step says why it's next, what it depends on, and what "done" looks like.Audience: practical, step‑by‑step order so you always build foundations first. Each step says why it’s next, what it depends on, and what “done” looks like.



Major rules to keep in mindMajor rules to keep in mind

- Security-first: Understand Visual read-only constraints before building anything to prevent rework.- Splash-first: Start with a plain loading screen that initializes services; no themes/services used by the splash itself.

- Infor Visual ERP: Read‑only via API Toolkit. Never write to Visual. Login uses the current user's Visual username/password (validated locally in MAMP).- Infor Visual ERP: Read‑only via API Toolkit. Never write to Visual. Login uses the current user’s Visual username/password (validated locally in MAMP).

  - Visual Schema: 14,796 fields across tables (MTMFG Tables.csv), 14,776 field definitions (Visual Data Table.csv), 1,266 FK relationships (MTMFG Relationships.csv)- Android: Never connects to Visual directly (use your API projections). Desktop may consume Visual read-only data via Toolkit if IT permits.

  - Key Tables: PART (30-char ID), LOCATION (15-char ID), WAREHOUSE (15-char ID), SHOP_RESOURCE (15-char ID), SITE (15-char ID)- Spec-driven: Every feature later references acceptance criteria (AC IDs) and traceability.

- Android: Never connects to Visual directly (use your API projections). Desktop may consume Visual read-only data via Toolkit if IT permits.

- Spec-driven: Every feature later references acceptance criteria (AC IDs) and traceability.Phase 1 — Absolute Foundations

1) docs/BOOT-SEQUENCE.md

Phase 1 — Security and Foundations (Build Constraints First)2) docs/ENVIRONMENTS-AND-CONFIG.md

1) docs/SECURITY-COMPLIANCE-POLICY.md3) docs/SECURITY-COMPLIANCE-POLICY.md

   - WHY: Establish read-only Visual boundaries and security policies before any code

   - DEPENDENCIES: NonePhase 2 — Data and Visual Boundaries

   - DONE WHEN: Team understands no Visual writes, credential handling, audit requirements4) docs/DATA-CONTRACTS.md

   - VISUAL SCHEMA: References APPLICATION_USER table (Lines: 564-592), Visual read-only enforcement rules5) docs/VISUAL-WHITELIST.md

6) docs/VISUAL-CREDENTIALS-FLOW.md

2) docs/ENVIRONMENTS-AND-CONFIG.md

   - WHY: Configuration management for Dev/Staging/Prod environmentsPhase 3 — Platform Access and Reliability

   - DEPENDENCIES: Security policies7) docs/API-SPECIFICATION.md

   - DONE WHEN: Environment switching works; feature flags defined; Toolkit endpoints configured8) docs/OFFLINE-SYNC-POLICY.md

   - VISUAL SCHEMA: Cache size planning (PART: 117 fields, LOCATION: 14 fields, WAREHOUSE: 16 fields, SHOP_RESOURCE: 42 fields)

Phase 4 — UX, Scanning, Roles

3) docs/BOOT-SEQUENCE.md9) docs/UI-UX-GUIDELINES.md

   - WHY: Predictable startup with service initialization order10) docs/BARCODE-AND-LABELING-STANDARDS.md

   - DEPENDENCIES: Security policies, environment config11) docs/ROLES-AND-PERMISSIONS.md

   - DONE WHEN: Splash screen initializes all services; caches warmed; diagnostics pass

   - VISUAL SCHEMA: Prefetch specifications for PART (Lines: 5779-5888), LOCATION (Lines: 5246-5263), WAREHOUSE (Lines: 14262-14288), SHOP_RESOURCE (Lines: 9299-9343)Phase 5 — Quality, Traceability, Support

12) docs/TEST-STRATEGY-AND-TRACEABILITY.md

Phase 2 — Data and Visual Boundaries13) docs/TROUBLESHOOTING-CATALOG.md

4) docs/DATA-CONTRACTS.md14) docs/COPILOT-ASSETS-CHECKLIST.md

   - WHY: Stable entity shapes for API, storage, and UI bindings15) docs/SPECIFY-CHECKLIST.md

   - DEPENDENCIES: Security policy, Visual schema knowledge

   - DONE WHEN: DTOs defined with exact field lengths from Visual; validation rules documentedPhase 6 — Feature Docs (run /specify)

   - VISUAL SCHEMA: PART.ID nvarchar(30) Line: 5780, LOCATION.ID nvarchar(15) Line: 5247, exact field types and lengths16) Generate feature specs under docs/specs/features/* using the prompt library.

5) docs/VISUAL-CREDENTIALS-FLOW.md
   - WHY: Secure credential handling before accessing Visual
   - DEPENDENCIES: Security policy, data contracts
   - DONE WHEN: Local validation implemented; Toolkit session established; no plaintext storage
   - VISUAL SCHEMA: APPLICATION_USER.NAME nvarchar(20) Line: 565, USER_PWD nvarchar(90) Line: 576

6) docs/VISUAL-WHITELIST.md
   - WHY: Explicit allowlist of what can be read from Visual
   - DEPENDENCIES: Data contracts, credentials flow
   - DONE WHEN: Allowlist documented with CSV line references; Toolkit commands cited; FK relationships mapped
   - VISUAL SCHEMA: All key tables with CSV line numbers, FK relationships from MTMFG Relationships.csv (1,266 relationships)

Phase 3 — Platform Access and Reliability
7) docs/API-SPECIFICATION.md
   - WHY: Android-server communication contract
   - DEPENDENCIES: Data contracts, Visual whitelist
   - DONE WHEN: Endpoints defined with Visual field constraints; projections documented; error shapes standardized
   - VISUAL SCHEMA: Visual projection endpoints with exact field lengths and sources

8) docs/OFFLINE-SYNC-POLICY.md
   - WHY: Handle connectivity issues gracefully
   - DEPENDENCIES: API specification, Visual whitelist
   - DONE WHEN: Online/Degraded/Offline modes defined; queue implemented; conflict resolution rules clear
   - VISUAL SCHEMA: Cache validation against Visual tables, FK integrity checks during offline mode

Phase 4 — UX, Scanning, Roles
9) docs/ROLES-AND-PERMISSIONS.md
   - WHY: Authorization model before building features
   - DEPENDENCIES: Security policy, API specification
   - DONE WHEN: Role-to-action mappings clear; approval workflows defined; Visual read-only enforced at all levels
   - VISUAL SCHEMA: APPLICATION_USER.IS_ADMIN Line: 574, role enforcement with Visual read-only constraint

10) docs/UI-UX-GUIDELINES.md
   - WHY: Consistent operator experience
   - DEPENDENCIES: Roles and permissions
   - DONE WHEN: Touch targets, scan-first patterns, state banners designed; accessibility rules defined
   - VISUAL SCHEMA: Field length constraints for inputs (PART.ID 30-char, LOCATION.ID 15-char, TRACE.ID 30-char)

11) docs/BARCODE-AND-LABELING-STANDARDS.md
   - WHY: Reliable scanning and printing
   - DEPENDENCIES: Data contracts, UI/UX guidelines
   - DONE WHEN: GS1 parsing rules defined; label templates created; validation against Visual cache specified
   - VISUAL SCHEMA: PART.ID (30-char) Line: 5780, PART.STOCK_UM (15-char) Line: 5791, lot validation

Phase 5 — Quality, Traceability, Support
12) docs/TEST-STRATEGY-AND-TRACEABILITY.md
   - WHY: Quality gates and AC tracking
   - DEPENDENCIES: All above (tests validate everything)
   - DONE WHEN: Test layers defined; AC-to-test mapping automated; Visual citation validation in CI
   - VISUAL SCHEMA: Field validation rules, FK relationship tests, read-only enforcement tests

13) docs/TROUBLESHOOTING-CATALOG.md
   - WHY: Support efficiency
   - DEPENDENCIES: All operational documents
   - DONE WHEN: Common issues documented; resolution steps clear; Visual-specific errors covered
   - VISUAL SCHEMA: Visual table/field references for error diagnosis, FK relationship troubleshooting

14) docs/COPILOT-ASSETS-CHECKLIST.md
   - WHY: AI assistance consistency
   - DEPENDENCIES: All documentation complete
   - DONE WHEN: copilot-instructions.md reflects Visual constraints; templates reference specs

15) docs/SPECIFY-CHECKLIST.md
   - WHY: Documentation generation consistency
   - DEPENDENCIES: All core documents
   - DONE WHEN: /specify prompts validate Visual constraints; CSV citations automated

Phase 6 — Feature Docs (run /specify)
16) Generate feature specs under docs/specs/features/* using the prompt library.
    - Each feature spec must reference Visual schema with CSV line numbers
    - All Visual data access must cite Toolkit commands and pages
