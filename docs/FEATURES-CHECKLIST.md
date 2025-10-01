# Features Checklist — Template Avalonia Application (Windows + Android)

Rule
- If it says “MANDATORY” next to an item, it is required.
- If it does NOT say “MANDATORY,” it is optional.

How to use this checklist
- Check off what you plan to include now. You can add optional items later.
- Every enabled item should follow our doc format with “For humans” and “For AI agents,” plus acceptance criteria (AC IDs) where it makes sense, edge cases, data needs (Visual via API Toolkit with citations), platform notes, traceability, and quick clarification questions.

--------------------------------------------------------------------------------

## Mandatory (tagged “MANDATORY”)

- [x] Splash‑first Boot Sequence (docs/BOOT-SEQUENCE.md) — MANDATORY
  - Simple explanation: A plain loading screen starts first and wakes up all the app’s parts in the right order. This keeps startup clean and safe.

- [x] Environments and Configuration (docs/ENVIRONMENTS-AND-CONFIG.md) — MANDATORY
  - Simple explanation: One place to set “Dev, Test, or Prod” and all server addresses so you can switch without changing code.

- [x] Data Contracts (docs/DATA-CONTRACTS.md) — MANDATORY when used by /specify
  - Simple explanation: Clear lists of fields (like Part Number, Location Code) and their sizes. Everyone uses the same shapes so things fit together.

- [x] Test Strategy and Traceability (docs/TEST-STRATEGY-AND-TRACEABILITY.md) — MANDATORY
  - Simple explanation: Every rule has a test, and we track rule → test → pull request so nothing slips through.

- [ ] UI and UX Guidelines (docs/UI-UX-GUIDELINES.md) — MANDATORY
  - Simple explanation: Designs that are easy to tap and read on the shop floor. Clear banners show when you’re online, limited, or offline.

- [ ] Roles and Permissions (docs/ROLES-AND-PERMISSIONS.md) — MANDATORY
  - Simple explanation: Who can do what. For example, only a Supervisor can approve an over‑receipt or a big change.

- [ ] Troubleshooting Catalog (docs/TROUBLESHOOTING-CATALOG.md) — MANDATORY
  - Simple explanation: Quick “fix it” steps for common problems like “can’t reach Visual,” “scanner not working,” or “printer won’t print.”

- [ ] Copilot Assets Checklist (docs/COPILOT-ASSETS-CHECKLIST.md) — MANDATORY
  - Simple explanation: Makes AI helpers smarter by giving them the right rules and templates for your project.

- [ ] Specify Command Checklist (docs/SPECIFY-CHECKLIST.md) — MANDATORY
  - Simple explanation: How to run the /specify prompts so your feature docs are created the same way every time.

- [ ] Master TOC (docs/TOC.md) and Implementation Order (docs/TOC-IMPLEMENTATION-ORDER.md) — MANDATORY
  - Simple explanation: The master index and the step‑by‑step build order so you always know what to do next.

- [ ] Update and Version Check (notifications and policies) — MANDATORY
  - Simple explanation: Tells users when a new app version is ready and what changed. Can require minimum versions if needed.

- [ ] Modular Extensions Framework (future plug‑ins) — MANDATORY
  - Simple explanation: Lets you add new parts to the app later (like advanced reports) without breaking the core.

Note about “MANDATORY when used by /specify”
- Data Contracts are mandatory when you are using /specify to generate or manage spec files, because /specify needs stable field shapes. If you are not using /specify yet, you may stage this, but it’s still strongly recommended.

--------------------------------------------------------------------------------

## Optional (no “MANDATORY” tag)

- [ ] Security and Compliance (docs/SECURITY-COMPLIANCE-POLICY.md)
  - Simple explanation: Rules that keep your data safe (HTTPS, no secret passwords in files, Visual is read‑only). Highly recommended.

- [ ] Visual Credentials Flow (docs/VISUAL-CREDENTIALS-FLOW.md)
  - Simple explanation: Users type their Visual username and password. We check it locally first, then connect to Visual in read‑only mode.

- [ ] Visual Read‑Only Allowlist (API Toolkit) (docs/VISUAL-WHITELIST.md)
  - Simple explanation: The approved “read‑only” list for Visual data. Says exactly what we can read and from where. No writes to Visual, ever.

- [ ] Inventory API Specification (docs/API-SPECIFICATION.md)
  - Simple explanation: The phone app talks to your server, not straight to Visual or MySQL. This file explains every endpoint and the data it returns.

- [ ] Offline and Sync Policy (docs/OFFLINE-SYNC-POLICY.md)
  - Simple explanation: What the app does when the internet drops—save work now, send later—with safe retry rules.

- [ ] Barcode and Labeling Standards (docs/BARCODE-AND-LABELING-STANDARDS.md)
  - Simple explanation: Which barcodes we scan (like GS1‑128) and what must be on labels (like Part, Lot, Qty). Keeps scanning and printing consistent.

Feature specs (add when in scope; one file per feature in docs/specs/features/)
- [ ] Receiving Purchase Orders (receiving.md)
  - Simple explanation: How goods arrive and get checked in. Covers matching to POs, lots, and labels.
- [ ] Put‑Away (put-away.md)
  - Simple explanation: Moving items from the dock to the right bin or rack. Handles rules like QC hold or allergen areas.
- [ ] Location Management (location-management.md)
  - Simple explanation: Keeping bins and racks accurate. Matches your app’s locations with Visual’s reference list.
- [ ] Kitting to Work Orders (kitting.md)
  - Simple explanation: Picking parts for a job. Handles allowed substitutions and prints kit labels.
- [ ] Cycle Counts (cycle-counts.md)
  - Simple explanation: Counting a small set of items often. Handles recounts and approvals for big differences.
- [ ] Traceability (traceability.md)
  - Simple explanation: Given a finished good, list which raw lots were used, who did it, and when. Great for audits.
- [ ] Reporting & KPIs (reporting.md)
  - Simple explanation: Key numbers like inventory accuracy and count time. Explains how each number is calculated.
- [ ] Printing Pipeline Extensions (advanced templates, per‑printer overrides)
  - Simple explanation: Extra printing tools and templates for special labels or special printers.

--------------------------------------------------------------------------------

Validation reminders
- For any enabled document:
  - Include acceptance criteria with IDs (if it makes sense), edge cases, data needs (Visual via API Toolkit with exact citations; CSV line placeholders if needed), platform notes (Windows uses MySQL directly; Android uses the API), scanning/printing when relevant, traceability hooks, and simple “why it matters” questions.
  - Always follow the Visual rules: READ‑ONLY; use the API Toolkit only (no direct SQL); check Visual login locally in MAMP before any Visual session starts.

Tip
- You can change an item from optional to mandatory by adding “— MANDATORY” at the end of the line.