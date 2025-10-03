# Test Strategy & Traceability Plan

Ensures acceptance criteria (AC) map to tests and code for auditability and confidence.

## Purpose & Usage

- **Purpose:** Identify coverage gaps, accelerate reviews, and support compliance.
- **When Used:** Feature planning, PR reviews, releases.
- **Dependencies:** Feature specs with AC IDs, test cases, traceability tooling.
- **Downstream:** CI pipeline, QA, audits.
- **Priority:** **High**

## AI Agent Guidance

- **Intent:** Define test layers (unit, integration, UI), map AC IDs to test IDs and PRs, and maintain a traceability matrix.
- **Checks:** Ensure Visual Toolkit citations and CSV line references are present where required.
- **Dependencies:** Acceptance criteria docs, test frameworks, trace builder.
- **Consumers:** CI, dashboards, compliance reports.
- **Non-Functionals:** Low maintenance cost; auto-generation preferred.
- **Priority:** **High**

## Test Layers

### Unit Tests

- **Domain Rules:** Validators, mappers.
  - Field length validation:
    - `PART.ID` (30)
    - `LOCATION.ID` (15)
    - `WAREHOUSE.ID` (15)
    - `SHOP_RESOURCE.ID` (15)
  - Data type validation: `nvarchar`, `nchar`, decimal precision (see `Visual Data Table.csv`)
  - Relationship integrity: FK validation (`MTMFG Relationships.csv`, 1266 relationships)

### Integration Tests

- **Repositories:** App DB, API endpoints, Visual projection fallbacks (mocked/staged).
  - Mock Visual responses using correct field lengths/types from `Visual Data Table.csv`
  - Test FK constraint handling:
    - `LOCATION→WAREHOUSE` (FK Line: 427)
    - `PART_LOCATION` links (FK Lines: 459–460)
  - Validate read-only enforcement: No Visual write operations in any code path

### UI/Flow Tests

- **Scan/Submit Flows:** Use fake services.
  - Barcode parsing: Enforce 30-char `PART.ID` limits
  - Location input: Enforce 15-char `LOCATION.ID` limits
  - Simulate cache aging and offline mode transitions

## Traceability Matrix

- **AC ID Format:** Per feature (e.g., `AC-RECV-###`)
- **PRs:** Must list implemented AC IDs; CI ensures tests exist or are planned (with tickets)
- **CI Gate:** Fail or warn if Visual references lack Toolkit citations or CSV line placeholders

### Citation & Reference Rules

- **Required Citation Format:**
  - `"Reference-{File Name} - {Chapter/Section/Page}"`
- **Required CSV Reference Format:**
  - `"Visual Data Table.csv Line: {number}"`
  - `"MTMFG Tables.csv Line: {number}"`
  - `"MTMFG Relationships.csv Line: {number}"`
- **Validation Rules:**
  - All `PART` references must cite `Visual Data Table.csv` Lines: 5779–5888
  - All `LOCATION` references must cite `Visual Data Table.csv` Lines: 5246–5263
  - All FK relationship code must cite `MTMFG Relationships.csv` with specific line numbers
  - All Toolkit API calls must include `Reference-{File}` citation

## PR/CI Clarification

- **Q:** Should PR checks fail if AC IDs are missing?
  - **A:** Yes. Enforces discipline and keeps spec/code aligned.
  - **Options:** [A] Warn [B] Fail [C] Block only on release branches

---

> **Formatting Compliance:**
>
> - Use semantic headings and bullet lists for clarity
> - Reference all external files and lines explicitly
> - Align test layer structure with constitution’s TDD and traceability mandates
> - Ensure all validation and citation rules are actionable by CI
> - Maintain markdown readability for both humans and AI agents
