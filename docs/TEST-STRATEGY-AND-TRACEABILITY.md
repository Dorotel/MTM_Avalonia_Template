# Test Strategy and Traceability Plan
Ensures acceptance criteria map to tests and code, enabling audits and confidence.

For humans
- Purpose: Know what’s covered and what’s missing; speed up reviews.
- When used: Feature planning, PR reviews, releases.
- Dependencies: Specs with AC IDs, test cases, traceability tool/process.
- What depends on it: CI pipeline, QA, audits.
- Priority: High.

For AI agents
- Intent: Define unit/integration/UI test layers, AC-to-test mapping, and a traceability matrix linking AC IDs to test IDs and PRs. Include checks that Visual Toolkit citations and CSV line references exist where required.
- Dependencies: Acceptance criteria documents, test frameworks, trace builder.
- Consumers: CI, dashboards, compliance reports.
- Non-functionals: Low maintenance cost; auto-generation where possible.
- Priority: High.

Layers
- Unit: Domain rules, validators, mappers.
  - Field length validation: PART.ID (30), LOCATION.ID (15), WAREHOUSE.ID (15), SHOP_RESOURCE.ID (15)
  - Data type validation: nvarchar, nchar, decimal precision from Visual Data Table.csv
  - Relationship integrity: FK validation against MTMFG Relationships.csv (1266 relationships)
- Integration: Repositories (app DB), API endpoints, Visual projection fallbacks (mocked/staged).
  - Mock Visual responses with correct field lengths and types from Visual Data Table.csv
  - Test FK constraint handling: LOCATION→WAREHOUSE (FK Line: 427), PART_LOCATION links (FK Lines: 459-460)
  - Validate read-only enforcement: no Visual write operations in any code path
- UI/Flow: Scan/submit flows with fake services.
  - Test barcode parsing with 30-char PART.ID limits
  - Validate location input with 15-char LOCATION.ID limits
  - Simulate cache aging and offline mode transitions

Traceability
- AC ID format per feature (e.g., AC-RECV-###).
- Each PR lists implemented AC IDs; CI ensures tests exist or are planned with tickets.
- CI gate: fail or warn when Visual references lack Toolkit citations or CSV line placeholders.
  - Required citations format: "Reference-{File Name} - {Chapter/Section/Page}"
  - Required CSV references format: "Visual Data Table.csv Line: {number}" or "MTMFG Tables.csv Line: {number}" or "MTMFG Relationships.csv Line: {number}"
  - Validation rules:
    - All PART references must cite Visual Data Table.csv Lines: 5779-5888
    - All LOCATION references must cite Visual Data Table.csv Lines: 5246-5263
    - All FK relationship code must cite MTMFG Relationships.csv with specific line numbers
    - All Toolkit API calls must include Reference-{File} citation

Clarification questions
- Q: Should we fail PR checks if AC IDs are missing?
  - Why: Enforces discipline.
  - Suggested: Yes.
  - Reason: Keeps spec and code aligned.
  - Options: [A] Warn [B] Fail [C] Block only on release branches