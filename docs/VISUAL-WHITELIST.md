# Infor Visual Read-Only Allowlist (API Toolkit)

> **Purpose:** Prevent accidental reads from sensitive or irrelevant sources; standardize master data access from Visual.
> **Priority:** Critical
> **Dependencies:** CSV dictionary files in `Visual Files/Database Files/`, Reference guides (Toolkit)
> **Consumers:** Visual repositories, API projections, validation

---

## AI Agent Intent

- **Maintain**: Allowlist of API Toolkit commands and dataset projections
- **Citations**: CSV source lines and Toolkit pages required
- **Direct SQL**: **Forbidden**
- **Non-functionals**: Easy schema updates, auditable

---

## Allowlist Structure

### Items/Parts

- **Toolkit Command**: `<Reference-Inventory - {Chapter/Section/Page}>`
- **Fields**:
  - `ID nvarchar(30)`
  - `DESCRIPTION nvarchar(255)`
  - `STOCK_UM nvarchar(15)`
  - `ACTIVE_FLAG nchar(1)`
- **CSV Sources**:
  - `MTMFG Tables.csv` (Lines: 1657-1773, 117 PART fields)
  - `Visual Data Table.csv` (Lines: 5779-5888, PART table definition)
- **Primary Key**: `PART.ID` (Line: 5780)

### Locations/Racks

- **Toolkit Command**: `<Reference-Inventory - {Chapter/Section/Page}>`
- **Fields**:
  - `ID nvarchar(15)`
  - `WAREHOUSE_ID nvarchar(15)`
  - `DESCRIPTION nvarchar(80)`
- **CSV Sources**:
  - `MTMFG Tables.csv` (Lines: 1513-1526, 14 LOCATION fields)
  - `Visual Data Table.csv` (Lines: 5246-5263, LOCATION table definition)
- **Primary Key**: `LOCATION.ID` (Line: 5247)
- **Foreign Key**: `LOCATION.WAREHOUSE_ID → WAREHOUSE.ID` (`MTMFG Relationships.csv` Line: 427)

### Warehouses

- **Toolkit Command**: `<Reference-Inventory - {Chapter/Section/Page}>`
- **Fields**:
  - `ID nvarchar(15)`
  - `DESCRIPTION nvarchar(50)`
  - `SITE_ID nvarchar(15)`
- **CSV Sources**:
  - `MTMFG Tables.csv` (Lines: 4229-4244, 16 WAREHOUSE fields)
  - `Visual Data Table.csv` (Lines: 14262-14288, WAREHOUSE table definition)
- **Primary Key**: `WAREHOUSE.ID` (Line: 14263)

### WorkCenters/Resources

- **Toolkit Command**: `<Reference-Shop Floor - {Chapter/Section/Page}>`
- **Fields**:
  - `ID nvarchar(15)`
  - `DESCRIPTION nvarchar(50)`
  - `ACTIVE_FLAG nchar(1)`
- **CSV Sources**:
  - `MTMFG Tables.csv` (Lines: 3452-3493, 42 SHOP_RESOURCE fields)
  - `Visual Data Table.csv` (Lines: 9299-9343, SHOP_RESOURCE table definition)
- **Primary Key**: `SHOP_RESOURCE.ID` (Line: 9299)

### Sites

- **Toolkit Command**: `<Reference-Core - {Chapter/Section/Page}>`
- **Fields**:
  - `ID nvarchar(15)`
  - `DESCRIPTION nvarchar(50)`
  - `ENTITY_ID nvarchar(15)`
- **CSV Sources**:
  - `MTMFG Tables.csv` (Lines: 3382-3425, 44 SITE fields)
  - `Visual Data Table.csv` (Lines: 9398-9443, SITE table definition)
- **Primary Key**: `SITE.ID` (Line: 9399)

---

## Approved Read-Only Operations (Toolkit Only)

- `GetItemByID` — Reference-Inventory - {Chapter/Section/Page}
- `GetLocationsByWarehouse` — Reference-Inventory - {Chapter/Section/Page}
- `GetWorkCenterByID` — Reference-Shop Floor - {Chapter/Section/Page}

---

## Relationships (from MTMFG Relationships.csv)

- `LOCATION.WAREHOUSE_ID → WAREHOUSE.ID` (Line: 427: FKEY0117,LOCATION,WAREHOUSE_ID,WAREHOUSE,ID)
- `PART_LOCATION`
  - Links:
    - `PART_ID → PART.ID` (Line: 459)
    - `WAREHOUSE_ID → LOCATION.WAREHOUSE_ID` (Line: 460)
    - `LOCATION_ID → LOCATION.ID` (Line: 460)
- `INVENTORY_TRANS.SITE_ID → SITE.ID` (Line: 412: FK_INVTRANS_TO_SITE,INVENTORY_TRANS,SITE_ID,SITE,ID)

---

## Change Control

- **Additions**: Must cite CSV source + line, confirm read-only, and provide valid Toolkit citation
- **Change Log**: Date, author, reason required

---

## Clarification

- **Second Reviewer Required for Allowlist Changes?**
  - **Why**: Prevent risky additions
  - **Suggested**: Yes
  - **Reason**: Defense-in-depth
  - **Options**: [A] Yes [B] No

---

> _Formatting and structure comply with [MTM Avalonia Template Constitution](../.specify/memory/constitution.md) and extension best practices._
