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

Allowlist structure (with specific Visual schema references)
- Items/Parts:
  - Toolkit Command = <from Reference-Inventory - {Chapter/Section/Page}>
  - Dataset Fields = ID nvarchar(30), DESCRIPTION nvarchar(255), STOCK_UM nvarchar(15), ACTIVE_FLAG nchar(1) (minimal set)
  - CSV: MTMFG Tables.csv (Lines: 1657-1773 - 117 PART fields), Visual Data Table.csv (Lines: 5779-5888 - PART table definition)
  - Primary Key: PART.ID (Line: 5780)
- Locations/Racks:
  - Toolkit Command = <Reference-Inventory - {Chapter/Section/Page}>
  - Dataset Fields = ID nvarchar(15), WAREHOUSE_ID nvarchar(15), DESCRIPTION nvarchar(80)
  - CSV: MTMFG Tables.csv (Lines: 1513-1526 - 14 LOCATION fields), Visual Data Table.csv (Lines: 5246-5263 - LOCATION table definition)
  - Primary Key: LOCATION.ID (Line: 5247)
  - Foreign Key: LOCATION.WAREHOUSE_ID → WAREHOUSE.ID (MTMFG Relationships.csv Line: 427)
- Warehouses:
  - Toolkit Command = <Reference-Inventory - {Chapter/Section/Page}>
  - Dataset Fields = ID nvarchar(15), DESCRIPTION nvarchar(50), SITE_ID nvarchar(15)
  - CSV: MTMFG Tables.csv (Lines: 4229-4244 - 16 WAREHOUSE fields), Visual Data Table.csv (Lines: 14262-14288 - WAREHOUSE table definition)
  - Primary Key: WAREHOUSE.ID (Line: 14263)
- WorkCenters/Resources:
  - Toolkit Command = <Reference-Shop Floor - {Chapter/Section/Page}>
  - Dataset Fields = ID nvarchar(15), DESCRIPTION nvarchar(50), ACTIVE_FLAG nchar(1)
  - CSV: MTMFG Tables.csv (Lines: 3452-3493 - 42 SHOP_RESOURCE fields), Visual Data Table.csv (Lines: 9299-9343 - SHOP_RESOURCE table definition)
  - Primary Key: SHOP_RESOURCE.ID (Line: 9299)
- Sites:
  - Toolkit Command = <Reference-Core - {Chapter/Section/Page}>
  - Dataset Fields = ID nvarchar(15), DESCRIPTION nvarchar(50), ENTITY_ID nvarchar(15)
  - CSV: MTMFG Tables.csv (Lines: 3382-3425 - 44 SITE fields), Visual Data Table.csv (Lines: 9398-9443 - SITE table definition)
  - Primary Key: SITE.ID (Line: 9399)
- Approved Read-Only Operations (Toolkit only):
  - GetItemByID — Reference-Inventory - {Chapter/Section/Page}
  - GetLocationsByWarehouse — Reference-Inventory - {Chapter/Section/Page}
  - GetWorkCenterByID — Reference-Shop Floor - {Chapter/Section/Page}
- Relationships (from MTMFG Relationships.csv - 1266 total FK relationships):
  - LOCATION.WAREHOUSE_ID → WAREHOUSE.ID (Line: 427: FKEY0117,LOCATION,WAREHOUSE_ID,WAREHOUSE,ID)
  - PART_LOCATION (Links: PART_ID→PART.ID Line: 459, WAREHOUSE_ID→LOCATION.WAREHOUSE_ID Line: 460, LOCATION_ID→LOCATION.ID Line: 460)
  - INVENTORY_TRANS.SITE_ID → SITE.ID (Line: 412: FK_INVTRANS_TO_SITE,INVENTORY_TRANS,SITE_ID,SITE,ID)

Change control
- Any additions require citing CSV source+line and confirming read-only nature plus a valid Toolkit citation.
- Keep a change log with date, author, reason.

Clarification questions
- Q: Require a second reviewer for allowlist changes?
  - Why: Prevent risky additions.
  - Suggested: Yes.
  - Reason: Defense-in-depth.
  - Options: [A] Yes [B] No