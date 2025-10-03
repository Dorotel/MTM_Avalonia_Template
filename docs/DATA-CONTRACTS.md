# Data Contracts — Domain Entities and Field Expectations

> **Purpose:** Align expectations across UI, API, and storage; avoid mismatch and rework.
> **When Used:** Designing endpoints, repositories, and UI bindings.
> **Dependencies:** Visual dictionary CSVs, Reference guides (Toolkit), MySQL schema.
> **Consumers:** API, repositories, validation, mapping.
> **Priority:** High.

---

## AI Agent Guidance

- **Intent:** Specify canonical fields, types, lengths, and validation rules for Items, Locations, WorkCenters, Users, Inventory, Transactions, and Sequences.
- **Dependencies:** Serialization/contract service, mapping, validation, Visual CSV references and Toolkit citations.
- **Consumers:** API controllers, repositories, DTOs.
- **Non-functionals:** Backward compatibility, versioning strategy.

---

## Entity Contracts

> **All types/lengths sourced from Visual CSV and MySQL schema. Toolkit citations included.
> All field names use PascalCase.
> All string fields use explicit max length.
> All boolean/flag fields use `bit` or `nchar(1)` as per source.
> All contracts must be backward compatible.
> All changes require versioning.**

### Item

| Field       | Type          | Source/Citation                                          |
| ----------- | ------------- | -------------------------------------------------------- |
| PartNumber  | nvarchar(30)  | PART.ID, Visual Data Table.csv Line: 5780; Ref-Inventory |
| Description | nvarchar(255) | PART.DESCRIPTION, Visual Data Table.csv Line: 5781       |
| UoM         | nvarchar(15)  | PART.STOCK_UM, Visual Data Table.csv Line: 5791          |
| IsActive    | nchar(1)      | PART.ACTIVE_FLAG, Visual Data Table.csv Line: 5805       |

### Location

| Field       | Type         | Source/Citation                                              |
| ----------- | ------------ | ------------------------------------------------------------ |
| Code        | nvarchar(15) | LOCATION.ID, Visual Data Table.csv Line: 5247; Ref-Inventory |
| WarehouseId | nvarchar(15) | LOCATION.WAREHOUSE_ID, Visual Data Table.csv Line: 5248      |
| Description | nvarchar(80) | LOCATION.DESCRIPTION, Visual Data Table.csv Line: 5249       |
| IsActive    | nchar(1)     | Derived from status                                          |

### WorkCenter

| Field    | Type         | Source/Citation                                                    |
| -------- | ------------ | ------------------------------------------------------------------ |
| Code     | nvarchar(15) | SHOP_RESOURCE.ID, Visual Data Table.csv Line: 9299; Ref-Shop Floor |
| Name     | nvarchar(50) | SHOP_RESOURCE.DESCRIPTION, Visual Data Table.csv Line: 9300        |
| IsActive | nchar(1)     | SHOP_RESOURCE.ACTIVE_FLAG                                          |

### Sequence

| Field        | Type         | Source/Citation          |
| ------------ | ------------ | ------------------------ |
| Name         | nvarchar(50) | app DB; server-generated |
| CurrentValue | bigint       | app DB; server-generated |

### User

| Field       | Type         | Source/Citation                                        |
| ----------- | ------------ | ------------------------------------------------------ |
| Username    | nvarchar(20) | APPLICATION_USER.NAME, Visual Data Table.csv Line: 565 |
| DisplayName | nvarchar(50) | app-level                                              |
| Roles       | varchar(255) | app-level                                              |
| IsActive    | bit          | app-level                                              |

### InventoryRow

| Field        | Type          | Source/Citation |
| ------------ | ------------- | --------------- |
| ItemId       | nvarchar(30)  | app DB          |
| LocationCode | nvarchar(15)  | app DB          |
| Lot          | nvarchar(30)  | Optional        |
| Quantity     | decimal(18,5) | app DB          |
| ExpiryDate   | datetime      | Optional        |

### InventoryTransaction

| Field        | Type          | Source/Citation |
| ------------ | ------------- | --------------- |
| Item         | nvarchar(30)  | app DB          |
| FromLocation | nvarchar(15)  | app DB          |
| ToLocation   | nvarchar(15)  | app DB          |
| Lot          | nvarchar(30)  | app DB          |
| Qty          | decimal(18,5) | app DB          |
| Reason       | nvarchar(15)  | app DB          |
| Reference    | nvarchar(50)  | app DB          |
| PerformedBy  | nvarchar(20)  | app DB          |
| PerformedAt  | datetime      | app DB          |

---

## Clarification Questions

- **Q:** Max length for PartNumber and LocationCode?
  **Why:** Prevent truncation and DB errors.
  **Suggested:** Use Visual dictionary values as hard limits.
  **Reason:** Aligns with source of truth.
  **Options:**
  - [A] 32
  - [B] 64
  - [C] From CSV: **\_\_**

---

> **Formatting Compliance:**
>
> - All tables use Markdown pipe syntax for clarity and VS Code preview compatibility.
> - All field names and types match Visual CSV and MySQL schema.
> - All sections use clear headings and separation.
> - All content is structured for cross-platform, test-first, and spec-driven development per [constitution.md](../.specify/memory/constitution.md).
