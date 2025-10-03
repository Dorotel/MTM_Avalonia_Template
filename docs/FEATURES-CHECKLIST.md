# Features Checklist — MTM Avalonia Template

> **How to use:**
>
> - Check off what you plan to include now.
> - Every enabled item must follow our doc format:
>   - “For humans” and “For AI agents”
>   - Acceptance criteria (AC IDs)
>   - Edge cases
>   - Data needs (Visual via API Toolkit, with citations)
>   - Platform notes
>   - Traceability
>   - Quick clarification questions

---

## Mandatory Features (`MANDATORY`)

| Feature                           | Status | Doc                                                                                      | Summary                                                             |
| --------------------------------- | ------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Splash‑first Boot Sequence        | ✅     | [BOOT-SEQUENCE.md](/docs/BOOT-SEQUENCE.md)                                               | Plain loading screen starts first, wakes up all app parts in order. |
| Environments & Configuration      | ✅     | [ENVIRONMENTS-AND-CONFIG.md](/docs/ENVIRONMENTS-AND-CONFIG.md)                           | One place to set “Dev, Test, Prod” and server addresses.            |
| Data Contracts                    | ✅     | [DATA-CONTRACTS.md](/docs/DATA-CONTRACTS.md)                                             | Lists of fields (e.g., Part Number, Location Code) and sizes.       |
| Test Strategy & Traceability      | ✅     | [TEST-STRATEGY-AND-TRACEABILITY.md](/docs/TEST-STRATEGY-AND-TRACEABILITY.md)             | Every rule has a test, tracked to PRs.                              |
| UI & UX Guidelines                | ⬜     | [UI-UX-GUIDELINES.md](/docs/UI-UX-GUIDELINES.md)                                         | Designs for shop floor: easy tap/read, banners for online/offline.  |
| Roles & Permissions               | ⬜     | [ROLES-AND-PERMISSIONS.md](/docs/ROLES-AND-PERMISSIONS.md)                               | Defines who can do what (e.g., Supervisor approvals).               |
| Troubleshooting Catalog           | ⬜     | [TROUBLESHOOTING-CATALOG.md](/docs/TROUBLESHOOTING-CATALOG.md)                           | Quick fixes for common problems (Visual, scanner, printer).         |
| Copilot Assets Checklist          | ⬜     | [COPILOT-ASSETS-CHECKLIST.md](/docs/COPILOT-ASSETS-CHECKLIST.md)                         | AI helpers get correct rules/templates.                             |
| Specify Command Checklist         | ⬜     | [SPECIFY-CHECKLIST.md](/docs/SPECIFY-CHECKLIST.md)                                       | How to run `/specify` prompts for consistent docs.                  |
| Master TOC & Implementation Order | ⬜     | [TOC.md](/docs/TOC.md), [TOC-IMPLEMENTATION-ORDER.md](/docs/TOC-IMPLEMENTATION-ORDER.md) | Master index and build order.                                       |
| Update & Version Check            | ⬜     | _(notifications/policies)_                                                               | Notifies users of new versions and changes.                         |
| Modular Extensions Framework      | ⬜     | _(future plug-ins)_                                                                      | Add new app parts later without breaking core.                      |

> **Note:**
> Data Contracts are `MANDATORY` when using `/specify` for specs. Otherwise, strongly recommended.

---

## Optional Features

| Feature                      | Status | Doc                                                                          | Summary                                                              |
| ---------------------------- | ------ | ---------------------------------------------------------------------------- | -------------------------------------------------------------------- |
| Security & Compliance        | ⬜     | [SECURITY-COMPLIANCE-POLICY.md](/docs/SECURITY-COMPLIANCE-POLICY.md)         | Data safety rules (HTTPS, no secrets in files, Visual is read-only). |
| Visual Credentials Flow      | ⬜     | [VISUAL-CREDENTIALS-FLOW.md](/docs/VISUAL-CREDENTIALS-FLOW.md)               | Local Visual login, then connect in read-only mode.                  |
| Visual Read‑Only Allowlist   | ⬜     | [VISUAL-WHITELIST.md](/docs/VISUAL-WHITELIST.md)                             | Approved read-only Visual data list. No writes.                      |
| Inventory API Specification  | ⬜     | [API-SPECIFICATION.md](/docs/API-SPECIFICATION.md)                           | Phone app talks to server, not direct to Visual/MySQL.               |
| Offline & Sync Policy        | ⬜     | [OFFLINE-SYNC-POLICY.md](/docs/OFFLINE-SYNC-POLICY.md)                       | Handles internet drops—save now, send later, safe retry.             |
| Barcode & Labeling Standards | ⬜     | [BARCODE-AND-LABELING-STANDARDS.md](/docs/BARCODE-AND-LABELING-STANDARDS.md) | Which barcodes to scan, label requirements.                          |
| Receiving Purchase Orders    | ⬜     | [receiving.md](/docs/specs/features/receiving.md)                            | Goods arrival, PO matching, lots, labels.                            |
| Put‑Away                     | ⬜     | [put-away.md](/docs/specs/features/put-away.md)                              | Move items to bins/racks, QC/allergen rules.                         |
| Location Management          | ⬜     | [location-management.md](/docs/specs/features/location-management.md)        | Bin/rack accuracy, Visual reference sync.                            |
| Kitting to Work Orders       | ⬜     | [kitting.md](/docs/specs/features/kitting.md)                                | Picking parts, substitutions, kit labels.                            |
| Cycle Counts                 | ⬜     | [cycle-counts.md](/docs/specs/features/cycle-counts.md)                      | Frequent counts, recounts, approvals.                                |
| Traceability                 | ⬜     | [traceability.md](/docs/specs/features/traceability.md)                      | List raw lots for finished goods, audit support.                     |
| Reporting & KPIs             | ⬜     | [reporting.md](/docs/specs/features/reporting.md)                            | Key metrics, calculation explanations.                               |
| Printing Pipeline Extensions | ⬜     | _(advanced templates/printer overrides)_                                     | Extra printing tools/templates for special cases.                    |

---

## Validation Reminders

- For any enabled document:
  - Include acceptance criteria (with IDs), edge cases, data needs (Visual API citations), platform notes, scanning/printing, traceability hooks, and “why it matters.”
  - Always follow Visual rules: **READ-ONLY**, use API Toolkit only, check Visual login locally before session.

---

## Tips

- Change an item from optional to mandatory by adding `— MANDATORY` at the end of the line.
- All docs must align with [project constitution](../.specify/memory/constitution.md):
  - Use CommunityToolkit.Mvvm patterns
  - CompiledBinding in XAML (`x:DataType`, `x:CompileBindings="True"`)
  - Async methods with `CancellationToken`
  - Test-first (xUnit, >80% coverage)
  - DI via AppBuilder
  - Theming via semantic tokens
