# Barcode and Labeling Standards

> **Purpose:** Ensure reliable barcode scanning and consistent label generation across all warehouse operations.
> **Scope:** Applies to receiving, put-away, moves, kitting, cycle counts, and traceability reports.

---

## Human-Facing Guidelines

- **Goal:** Minimize manual entry and errors; maximize scan reliability.
- **Usage:** Any UI screen involving barcode scanning or label printing.
- **Dependencies:**
  - Barcode service
  - Printing service
  - Visual master data cache (for part descriptions/UoM)
- **Downstream Consumers:**
  - Receiving
  - Location management
  - Moves
  - Kitting
  - Cycle counts
  - Traceability reports
- **Priority:** High

---

## AI Agent Guidelines

- **Intent:**
  - Normalize supported symbologies
  - Parse GS1 Application Identifiers (AIs)
  - Validate content (part, lot, qty, date)
  - Render templates (ZPL/PDF) with stable tokens
- **Dependencies:**
  - Localization/culture for date/decimal formats
  - Mapping to domain primitives
  - Printer discovery/configuration
- **Consumers:**
  - ViewModels
  - Print queue
  - Background services
- **Non-Functionals:**
  - Low-latency decoding
  - Offline-friendly template resolution
  - Predictable label sizing
- **Priority:** High

---

## Scanning Rules

- **Supported Symbologies:**
  - GS1-128
  - Code128
  - QR (expandable)
- **Content Parsing:**
  - GS1 AIs:
    - Lot (`10`)
    - Quantity (`30`)
    - Expiry (`17`)
    - Serial (where applicable)
  - **Part Number Mapping:**
    - `PART.ID` (`nvarchar(30)`)
    - Source: Visual Data Table.csv (Line 5780), Reference-Inventory
  - **Location Code Mapping:**
    - `LOCATION.ID` (`nvarchar(15)`)
    - Source: Visual Data Table.csv (Line 5247), Reference-Inventory
- **Validation:**
  - Part numbers (max 30 chars) and lots (max 30 chars from `TRACE.ID`)
    - Validate against Visual cache (offline) or projection (online)
  - Quantities:
    - Respect UoM (`PART.STOCK_UM`, `nvarchar(15)`, Line 5791)
    - Decimal precision: `decimal(18,5)`

---

## Labeling Rules

- **Printer Language:**
  - ZPL (primary)
  - PDF (fallback for non-Zebra devices)
- **Templates:**
  - Central library, versioned
  - Tokens: `{PartNumber}`, `{PartDescription}`, `{Lot}`, `{Qty}`, `{UoM}`, `{Date}`, `{WorkCenter}`, `{LocationCode}`
  - Visual-derived content sourced from cache to avoid print delays

---

## Clarification Questions

- **Mandatory GS1 fields for receiving labels?**
  - *Why:* Drives template and validation
  - *Suggested:* Part, Lot, Qty, Expiry (if perishable)
  - *Reason:* Core traceability
  - *Options:*
    - [A] Part + Lot
    - [B] Part + Lot + Qty
    - [C] Part + Lot + Qty + Expiry

- **Default label size?**
  - *Why:* Impacts printers and template design
  - *Suggested:* 4x6 inches (pallets), 2x1 inches (bins)
  - *Reason:* Common industrial sizes
  - *Options:*
    - [A] 4x6
    - [B] 2x1
    - [C] Both
    - [D] Other: ______

---

> **Formatting Compliance:**
> - Use semantic headings and bullet lists
> - Group related rules under clear sections
> - Use code formatting for field names and tokens
> - Avoid dense paragraphs; prefer concise, scan-friendly layout
> - Aligns with [MTM Avalonia Template Constitution v1.1.0](../.specify/memory/constitution.md)
