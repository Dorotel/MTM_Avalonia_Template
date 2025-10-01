# Data Contracts — Domain Entities and Field Expectations
Defines stable shapes for key domain entities and how they map to Visual master data and app DB.

For humans
- Purpose: Align expectations across UI, API, and storage; avoid mismatch and rework.
- When used: Designing endpoints, repositories, and UI bindings.
- Dependencies: Visual dictionary CSVs, Reference-{File Name} guides (Toolkit), MySQL schema.
- What depends on it: API, repositories, validation, mapping.
- Priority: High.

For AI agents
- Intent: Specify canonical fields, types, lengths, and validation rules for Items, Locations, WorkCenters, Users, Inventory, Transactions, and Sequences.
- Dependencies: Serialization/contract service, mapping, validation, Visual CSV references and Toolkit citations.
- Consumers: API controllers, repositories, DTOs.
- Non-functionals: Backward compatibility, versioning strategy.
- Priority: High.

Entities (high-level fields; exact types/lengths documented per Visual CSV and MySQL schema; include Toolkit citations)
- Item: PartNumber (authoritative from Visual; Visual Data Table.csv Line: <to be filled>; Reference-{File Name} - {Chapter/Section/Page}), Description, UoM, IsActive.
- Location: Code (Visual source; CSV Line: <to be filled>; Reference-{File Name} - {Chapter/Section/Page}), Zone, IsActive.
- WorkCenter: Code, Name, IsActive (Visual source; Reference-{File Name} - {Chapter/Section/Page}).
- Sequence: Name, CurrentValue (app DB; server-generated).
- User: Username, DisplayName, Roles (app-level), IsActive.
- InventoryRow: ItemId/PartNumber, LocationCode, Lot (opt), Quantity, ExpiryDate (opt).
- InventoryTransaction: Item, From/To Location, Lot, Qty, Reason, Reference, PerformedBy, PerformedAt.

Clarification questions
- Q: Max length for PartNumber and LocationCode?
  - Why: Prevent truncation and DB errors.
  - Suggested: Use Visual dictionary values as hard limits.
  - Reason: Aligns with source of truth.
  - Options: [A] 32 [B] 64 [C] From CSV: ______