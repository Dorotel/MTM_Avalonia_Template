# Master Table of Contents (TOC) — Template Avalonia Application

> **Purpose:**
> Single index for implementing the application foundation and features in dependency order.

## Legend

- **[Core]** — Required foundational document
- **[Feature]** — Per-feature specification (optional, enable via checklist)
- **[Prompt]** — Prompt templates for `/specify` workflow

---

## Phase 1 — Foundations

- [Core] `docs/BOOT-SEQUENCE.md`
- [Core] `docs/ENVIRONMENTS-AND-CONFIG.md`
- [Core] `docs/SECURITY-COMPLIANCE-POLICY.md`
- [Core] `docs/VISUAL-CREDENTIALS-FLOW.md`
- [Core] `docs/DATA-CONTRACTS.md`
- [Core] `docs/VISUAL-WHITELIST.md`

## Phase 2 — Platform Interfaces

- [Core] `docs/API-SPECIFICATION.md`
- [Core] `docs/OFFLINE-SYNC-POLICY.md`

## Phase 3 — UX, Scanning, Roles

- [Core] `docs/UI-UX-GUIDELINES.md`
- [Core] `docs/BARCODE-AND-LABELING-STANDARDS.md`
- [Core] `docs/ROLES-AND-PERMISSIONS.md`

## Phase 4 — Quality and Operations

- [Core] `docs/TEST-STRATEGY-AND-TRACEABILITY.md`
- [Core] `docs/TROUBLESHOOTING-CATALOG.md`
- [Core] `docs/COPILOT-ASSETS-CHECKLIST.md`
- [Core] `docs/SPECIFY-CHECKLIST.md`

---

## Feature Specifications

_(Enable via `docs/FEATURES-CHECKLIST.md`)_

- [Feature] `docs/specs/features/receiving.md`
- [Feature] `docs/specs/features/put-away.md`
- [Feature] `docs/specs/features/location-management.md`
- [Feature] `docs/specs/features/kitting.md`
- [Feature] `docs/specs/features/cycle-counts.md`
- [Feature] `docs/specs/features/traceability.md`
- [Feature] `docs/specs/features/reporting.md`

---

## Prompt Library (for `/specify`)

- [Prompt] `specify files/MTM-TEMPLATE-CONSOLIDATION.master-prompt.md`
- [Prompt] `specify files/specify.master-prompt.md`
- [Prompt] `specify files/prompts/feature-receiving.prompt.md`
- [Prompt] `specify files/prompts/feature-put-away.prompt.md`
- [Prompt] `specify files/prompts/feature-location-management.prompt.md`
- [Prompt] `specify files/prompts/feature-kitting.prompt.md`
- [Prompt] `specify files/prompts/feature-cycle-counts.prompt.md`
- [Prompt] `specify files/prompts/feature-traceability.prompt.md`
- [Prompt] `specify files/prompts/feature-reporting.prompt.md`

---

## Implementation Order Helper

See [`docs/TOC-IMPLEMENTATION-ORDER.md`](docs/TOC-IMPLEMENTATION-ORDER.md) for step-by-step execution order, rationale, and completion criteria.

---

## Review Required (Pre-Release)

- Replace all `Line: <to be filled>` for CSV references in visual dictionary files.
- Insert precise Infor Visual API Toolkit citations: `{File Name} - {Chapter/Section/Page}` in VISUAL-related docs.

---

> **Formatting Standards:**
>
> - Use semantic section headings (`##`)
> - Use code formatting for file paths
> - List items with consistent indentation
> - Align with [constitution.md](../.specify/memory/constitution.md) principles: clarity, dependency order, and review requirements.
