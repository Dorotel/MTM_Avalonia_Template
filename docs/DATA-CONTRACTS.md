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

Entities (exact types/lengths from Visual CSV and MySQL schema; include Toolkit citations)
- Item: 
  - PartNumber: nvarchar(30) (PART.ID, Visual Data Table.csv Line: 5780; Reference-Inventory - {Chapter/Section/Page})
  - Description: nvarchar(255) (PART.DESCRIPTION, Visual Data Table.csv Line: 5781)
  - UoM: nvarchar(15) (PART.STOCK_UM, Visual Data Table.csv Line: 5791)
  - IsActive: nchar(1) (PART.ACTIVE_FLAG, Visual Data Table.csv Line: 5805)
- Location: 
  - Code: nvarchar(15) (LOCATION.ID, Visual Data Table.csv Line: 5247; Reference-Inventory - {Chapter/Section/Page})
  - WarehouseId: nvarchar(15) (LOCATION.WAREHOUSE_ID, Visual Data Table.csv Line: 5248)
  - Description: nvarchar(80) (LOCATION.DESCRIPTION, Visual Data Table.csv Line: 5249)
  - IsActive: nchar(1) (derived from status)
- WorkCenter: 
  - Code: nvarchar(15) (SHOP_RESOURCE.ID, Visual Data Table.csv Line: 9299; Reference-Shop Floor - {Chapter/Section/Page})
  - Name: nvarchar(50) (SHOP_RESOURCE.DESCRIPTION, Visual Data Table.csv Line: 9300)
  - IsActive: nchar(1) (SHOP_RESOURCE.ACTIVE_FLAG)
- Sequence: Name nvarchar(50), CurrentValue bigint (app DB; server-generated).
- User: Username nvarchar(20) (APPLICATION_USER.NAME, Visual Data Table.csv Line: 565), DisplayName nvarchar(50), Roles varchar(255) (app-level), IsActive bit.
- InventoryRow: ItemId nvarchar(30), LocationCode nvarchar(15), Lot nvarchar(30) (opt), Quantity decimal(18,5), ExpiryDate datetime (opt).
- InventoryTransaction: Item nvarchar(30), FromLocation nvarchar(15), ToLocation nvarchar(15), Lot nvarchar(30), Qty decimal(18,5), Reason nvarchar(15), Reference nvarchar(50), PerformedBy nvarchar(20), PerformedAt datetime.

Clarification questions
- Q: Max length for PartNumber and LocationCode?
  - Why: Prevent truncation and DB errors.
  - Suggested: Use Visual dictionary values as hard limits.
  - Reason: Aligns with source of truth.
  - Options: [A] 32 [B] 64 [C] From CSV: ______