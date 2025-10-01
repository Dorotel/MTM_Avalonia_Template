# Infor Visual Read-Only Allowlist (API Toolkit)
Defines approved sources for read-only Visual data, backed by CSV dictionary references and API Toolkit citations.

For humans
- Purpose: Prevent accidental reads from sensitive or irrelevant sources; standardize where the app pulls master data from Visual.
- When used: Designing queries, repositories, or API projections for Visual.
- Dependencies: CSV dictionary files in Visual Files/Database Files/, Reference-{File Name} guides (Toolkit).
- What depends on it: Visual repositories, API projections, validation.
- Priority: Critical.

For AI agents
- Intent: Maintain an allow-list of API Toolkit commands and dataset projections with required fields and relationship notes, each citing CSV source lines and Toolkit pages. Direct SQL is forbidden.
- Dependencies: Configuration, mapping, validation.
- Consumers: Visual read-only repositories and API backend.
- Non-functionals: Easy to update on schema changes; auditable.
- Priority: Critical.

Allowlist structure (examples with placeholders)
- Items/Parts:
  - Toolkit Command = <from Reference-{File} - {Chapter/Section/Page}>
  - Dataset Fields = PartNumber, Description, UoM, IsActive (minimal set)
  - CSV: MTMFG Tables.csv (Line: <to be filled>), Visual Data Table.csv (Line: <to be filled>)
- Locations/Racks:
  - Toolkit Command = <Reference-{File} - {Chapter/Section/Page}>
  - Fields = LocationCode, Zone, IsActive
  - CSV: MTMFG Tables.csv (Line: <to be filled>)
- WorkCenters:
  - Toolkit Command = <Reference-{File} - {Chapter/Section/Page}>
  - Fields = Code, Name, IsActive
  - CSV: Visual Data Table.csv (Line: <to be filled>)
- Approved Read-Only Operations (Toolkit only):
  - <OperationName> — Reference-{File} - {Chapter/Section/Page}
- Relationships:
  - Item↔Location relationship path — MTMFG Relationships.csv (Line: <to be filled>); Toolkit cross-reference: Reference-{File} - {Chapter/Section/Page}

Change control
- Any additions require citing CSV source+line and confirming read-only nature plus a valid Toolkit citation.
- Keep a change log with date, author, reason.

Clarification questions
- Q: Require a second reviewer for allowlist changes?
  - Why: Prevent risky additions.
  - Suggested: Yes.
  - Reason: Defense-in-depth.
  - Options: [A] Yes [B] No