# Barcode and Labeling Standards
Defines how we read barcodes and produce labels across receiving, put-away, moves, and kitting.

For humans
- Purpose: Make scanning reliable and labels consistent; reduce manual typing and errors.
- When used: Any screen involving scanning or printing.
- Dependencies: Barcode service, printing service, Visual master data cache (for part descriptions/UoM).
- What depends on it: Receiving, location management, moves, kitting, cycle counts, traceability reports.
- Priority: High.

For AI agents
- Intent: Normalize supported symbologies, GS1 AI parsing, validation of content (part/lot/qty/date), and rendering templates (ZPL/PDF) with stable tokens.
- Dependencies: Localization/culture for date and decimal format; mapping to domain primitives; printer discovery/config.
- Consumers: ViewModels, print queue, background services.
- Non-functionals: Low latency decoding, offline-friendly template resolution, predictable label sizing.
- Priority: High.

Scanning rules
- Supported symbologies: GS1-128, Code128, QR (expandable).
- Content parsing:
  - GS1 AIs for lot (e.g., 10), quantity (e.g., 30), expiry (e.g., 17), serial where applicable.
  - Part number mapping: PART.ID nvarchar(30) is authoritative field (Visual Data Table.csv Line: 5780) and toolkit guide (Reference-Inventory - {Chapter/Section/Page}).
  - Location code mapping: LOCATION.ID nvarchar(15) (Visual Data Table.csv Line: 5247; Reference-Inventory - {Chapter/Section/Page}).
- Validation:
  - Part numbers (max 30 chars) and lots (max 30 chars from TRACE.ID) validated against Visual cache (offline) or via projection (online).
  - Quantities must respect UoM field (PART.STOCK_UM nvarchar(15), Visual Data Table.csv Line: 5791) and decimal(18,5) precision policy.

Labeling rules
- Printer language: ZPL (primary), PDF fallback on non-Zebra devices.
- Templates:
  - Central library; versioned; tokens include {PartNumber}, {PartDescription}, {Lot}, {Qty}, {UoM}, {Date}, {WorkCenter}, {LocationCode}.
  - Visual-derived content must be sourced from cache to avoid print delays.

Clarification questions
- Q: Mandatory GS1 fields for receiving labels?
  - Why: Drives template and validation.
  - Suggested: Part, Lot, Qty, Expiry (if perishable).
  - Reason: Core traceability.
  - Options: [A] Part+Lot [B] Part+Lot+Qty [C] Part+Lot+Qty+Expiry
- Q: Default label size?
  - Why: Impacts printers and template design.
  - Suggested: 4x6 inches for pallets; 2x1 for bins.
  - Reason: Common industrial sizes.
  - Options: [A] 4x6 [B] 2x1 [C] Both [D] Other: ______