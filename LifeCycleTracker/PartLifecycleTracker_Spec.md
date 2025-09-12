# Part Lifecycle Transaction Tracker (HTA Macro) – Requirements & Technical Specification

## 1. Overview
A Windows HTA (HTML Application) + VBScript tool to inspect, analyze, and print the lifecycle of part inventory transactions (receipts, transfers, shipments, adjustments) from a Visual/VMFG (SQL Server) database. It provides a modernized UI (Windows 11 inspired) or an optional dark legacy theme and replaces CSV export with in-browser print formatting.

## 2. Primary Objectives
- Rapidly look up a Part ID and view its recent transactions.
- Reconstruct a chronological lifecycle (movement history and status) from raw transaction records.
- Detect and visually mark transfer pairings and potential split transfers.
- Provide a “Today’s Transactions” mode with day navigation.
- Allow sorting, row selection, lifecycle drill-down, and printing of the currently displayed dataset.
- Offer minimal friction authentication (SQL username/password) at launch.
- Keep implementation self‑contained: `*.hta` + `*.vbs` + optional `Database.config`.

## 3. Scope (Inclusions)
- Authentication prompts (username & password) via InputBox.
- ADO / SQLOLEDB connection to SQL Server.
- Configurable date range filtering.
- UI sections: Controls, Results (transactions), Lifecycle panel.
- Sorting by clicking table headers (stable, toggled ascending/descending).
- Row highlighting & selection state retention across sorts.
- Lifecycle reconstruction from transactions (including multi-step events via SQL CTE or in-memory logic).
- Detection of transfer “splits” (multiple transfers within same minute / grouping key).
- Role classification: receipt, transfer-in, transfer-out, shipment, adjust in/out (if applicable).
- Print-friendly stylesheet (replacing old “export to CSV”).
- Status bar messaging (progress, counts, errors, version info).
- Version tagging & optional sentinel variable indicating successful core load.
- Performance-conscious array-based storage (avoid per-row DOM builds during intermediate operations).

## 4. Out of Scope (Initial Phase)
- Persistent credentials storage (no secure vault integration).
- Editing or writing back to the database.
- Full audit logging or telemetry beyond basic console/status feedback.
- Rich filtering (warehouse, user, quantity ranges) beyond Part ID + date span.
- Multi-part bulk comparison or batch exports.
- Cross-database federation.
- Modern browser-only rewrite (the Chrome/Edge experiment was intentionally abandoned for now).

## 5. Functional Requirements
### 5.1 Authentication
- Prompt for SQL username & password on load (blocking flow until provided or cancelled).
- Cancel gracefully aborts initialization with clear status.

### 5.2 Data Loading (By Part)
- Inputs: Part ID (required), From Date (optional), To Date (optional).
- Retrieve up to N recent transactions (configurable cap, e.g., 500 or 1000).
- Date filtering inclusive logic: `>= fromDate` and `< toDate + 1 day` (or BETWEEN with time normalized) to ensure full-day coverage.
- Populate internal `tableData` array with normalized columns.

### 5.3 Today Mode
- Load all transactions for “today” by default (target date = today + offset).
- Provide navigation (Previous / Next Day) adjusting an internal `todayOffset`.
- Distinguish visually from part-specific mode (status prefix or panel title change).

### 5.4 Data Model (tableData Baseline)
Suggested columns (customizable):
0 TransactionID
1 Date (display)
2 Time (display)
3 Type (raw TYPE code or semantic type)
4 Quantity
5 Warehouse
6 Location / Flow
7 Reference (WO:/CO:/PO: or composite reference)
8 User / Operator
9 Related / Context ID (e.g., Work Order Base ID)
10 RawDate (Date object for sorting)
11 Role classification (receipt / transfer-in / transfer-out / shipment / adjust / etc.)
12 PartID (optional if needed for multi-part contexts)

### 5.5 Lifecycle Reconstruction
- For a selected transaction (or full part context), compute ordered lifecycle events.
- Use either:
  1. SQL CTE building a union of candidate events (receipts, transfers, shipments, adjustments) with sequence ordering, OR
  2. In-memory pass combining filtered transactions, grouped by time slices / reference attributes.
- Mark current selected transaction within lifecycle view.
- Add classification row classes (e.g., `lifecycle-current`, `lifecycle-transfer`, `lifecycle-ship`).

### 5.6 Transfer Pairing & Split Detection
- Pair inbound/outbound transfers via shared transfer group identifiers or time + quantity heuristics.
- Identify multiple transfers in same minute window for the same part/location group and tag as potential “split” events (e.g., label `TRANSFER (Split)`).

### 5.7 Sorting
- Click column headers to sort; clicking same header toggles direction.
- Numeric vs textual vs date-aware comparisons.
- Preserve selected row (re-map by unique TransactionID if possible rather than index).

### 5.8 Row Selection & Lifecycle Panel
- Single-click row selects it, populates lifecycle panel.
- Second click on same row deselects and hides lifecycle panel.
- In today mode (if defined as read-only snapshot), optional: disable lifecycle if part context insufficient.

### 5.9 Printing
- Print only current table and (optionally) associated lifecycle panel.
- Hide controls, buttons, status bar via `@media print` rules.
- Abort printing with user message if no transactions loaded.

### 5.10 Status & Messaging
- Show: Idle, Loading…, Row counts, Errors, Version string.
- Show explicit message when core fails to load (sentinel check) or DB connection fails.

### 5.11 Configuration
- Optional `Database.config` for server/catalog defaults (plaintext acceptable initially; improvement path to encryption noted).
- Possibly support override via environment variable / InputBox fallback.

### 5.12 Versioning & Sentinel
- Maintain `CORE_VERSION` constant and append to status line.
- Optionally define `Dim CORE_LOADED : CORE_LOADED = True` at end of successful parse.

### 5.13 Error Handling
- Wrap ADO open/execute in `On Error Resume Next` + explicit checks.
- Surface connection / SQL errors in UI instead of silent failure.

### 5.14 Theming / UI Modes
- Modern light (Win11 style) default.
- Optional dark legacy theme retained (maybe separate HTA or a toggle stylesheet injection).

### 5.15 Export (Deprecated)
- Prior CSV export replaced by Print; future: optional CSV/clipboard re-add with explicit user request.

## 6. Non-Functional Requirements
| Category | Requirement |
|----------|-------------|
| Performance | Load & render 500 rows in < 2 seconds on typical workstation. |
| Memory | In-memory arrays only; avoid per-row COM object retention. |
| Reliability | Robust against malformed or missing date inputs; fails gracefully if DB offline. |
| Maintainability | Single authoritative core file; avoid duplicate function definitions. |
| Usability | Minimal clicks from launch to data (< 3 actions). Intuitive column labels. |
| Security | Credentials transient (held only in memory). No logging of password. |
| Portability | Runs on Windows 10/11 with built-in mshta (IE11 engine). |
| Observability | Clear status messages; optional temporary debug MsgBoxes gated by a debug flag. |

## 7. Core Function Inventory (Target Set)
- `Core_Window_OnLoad`
- `Core_LoadData`
- `Core_LoadTodaysTransactions`
- `Core_DayPrevious` / `Core_DayForward` (day navigation)
- `Core_SortTable`
- `Core_SelectRow`
- Lifecycle builders: `BuildLifecycleFromDB` (SQL CTE approach) and/or `BuildLifecycleInMemory`
- `Core_ExportData` (Print wrapper)
- Panel toggles: `Core_ToggleLifecycle`, `Core_ToggleResults`
- Utility: `SqlSafe`, `ToSqlDate`, `EnsureCapacity`, comparators

## 8. User Flows
### 8.1 Standard Lookup
Open HTA → Prompt credentials → Enter Part ID → (Optional date range) → Load Transactions → Select row → View lifecycle → Print if needed.

### 8.2 Today Mode Investigation
Open HTA → Credentials → Click Today’s Transactions → (Optional Prev/Next) → Sort by time → (Optional) select a transaction → Print.

### 8.3 Lifecycle Drilldown
Load part → Sort / filter visually → Click transaction of interest → Lifecycle panel shows sequence → Identify shipment or outstanding status.

## 9. Data & Classification Logic Summary
- Receipt: Type I with WORKORDER or PO context.
- Transfer-In / Transfer-Out: Type I / O without customer order but with location changes.
- Shipment: Type O with customer order.
- Adjustment: Type O (or I) without WO/CO/PO and possibly flagged differently by reference logic.
- Split Transfer Marker: >1 transfer events in defined grouping window (same minute + same reference or quantity pattern).

## 10. Lifecycle Reconstruction (SQL CTE Concept)
Union sets:
1. Receipts (WO/PO)
2. Transfers (paired or single)
3. Shipments (customer order)
4. Adjustments (in/out)
Apply ordering: event_date, sequence bias, transaction id aggregation.
Group duplicates within same minute for split detection; annotate current transaction.

## 11. Printing Requirements
- Single page table if possible (allow natural pagination).
- Hide controls/status & navigation buttons.
- Maintain row classification background colors (muted but visible in monochrome printers).

## 12. Error Messaging Examples
| Scenario | Message |
|----------|---------|
| Missing part | "Enter a Part ID first." |
| DB connection fail | "Database connection failed: <description>" |
| SQL error | "SQL error: <description>" |
| No data to print | "No data to print." |
| Core not loaded | "Core script failed to load / parse." |

## 13. Deployment & Launch
- Files: `PartLifecycleTracker.hta`, `PartLifecycleCore.vbs`, optional `Database.config`.
- Launch via double-click or a launcher script (`LaunchPartLifecycleTracker.vbs`).
- Ensure file encoding ANSI or UTF-8 without BOM for VBScript includes.

## 14. Logging & Diagnostics (Optional Enhancements)
- Debug flag enabling MsgBox checkpoints or `document.title` tag updates.
- Lightweight in-memory log ring buffer shown via hidden keystroke (e.g., Ctrl+L).

## 15. Future Enhancements (Backlog)
- Advanced multi-part compare tab.
- Column filter row (warehouse / user / type quick filters).
- CSV / clipboard export return.
- Credential caching (encrypted DPAPI) opt-in.
- Progress bar for large fetches.
- Lifecycle graph visualization (simple ASCII Gantt or HTML timeline).
- Configurable color themes at runtime.

## 16. Compatibility & Pitfalls to Avoid
| Pitfall | Explanation | Mitigation |
|---------|-------------|------------|
| Duplicate `Option Explicit` & mixed fragments | Corrupted file with repeated blocks can cause silent parse aborts. | Maintain single authoritative core file; code reviews before replacing. |
| Non-VBScript comments (`//`) | Not valid; halts or partially aborts parsing. | Use only `'` or `Rem` comments. |
| Undeclared sentinel under `Option Explicit` | Assigning undeclared `CORE_LOADED` raises error. | Always `Dim CORE_LOADED` first. |
| Overly defensive fallback stubs | Fake subs hide real “undefined” problems; UI appears inert. | Fail fast—surface errors instead of stubbing. |
| HTA caching with `SINGLEINSTANCE="yes"` | Old script stays in memory; modifications ignored. | Use `SINGLEINSTANCE="no"` during development; restart between builds. |
| Filename mismatch (`PartLifecycleCoreClean.vbs` vs expected) | HTA keeps referencing old name; new logic never runs. | Keep consistent include name or update HTA reliably. |
| `VBScript:` prefix inconsistencies in `onclick` | In some HTA contexts plain `onclick="SubName"` is more reliable. | Use plain sub name for simplicity. |
| Silent ADO errors | Without checking `Err`, UI stalls. | Wrap open/execute and surface `Err.Description`. |
| Mixed encodings / BOM | UTF-8 BOM can confuse legacy parser. | Save without BOM or ANSI. |
| Zone-blocked files (downloaded) | Windows may mark file as blocked, script not executed properly. | Right-click → Properties → Unblock. |
| Large monolithic patches causing corruption | Manual merges introduce duplicate subs. | Use smaller, verified diffs; keep backups (`*_backup.vbs`). |
| Sorting losing selection | Index-based selection invalid after reorder. | Re-map by unique TransactionID if preserving selection is required. |
| Over-reliance on InputBox for creds | User mistypes with no validation. | (Future) add masked credential modal or retry loop. |

## 17. Acceptance Criteria (Representative)
- Launch always produces credential prompts (unless user cancels deliberately).
- Loading valid part returns ≥1 row when DB has data; status reflects count.
- Sorting toggles direction and updates arrow or status hint (optional).
- Selecting a row opens lifecycle panel (placeholder acceptable initial, fully reconstructed in final release).
- Print hides controls and produces legible transactional output.
- Transfer split scenarios visibly marked where applicable.
- No undefined function errors in console / no silent failure states.
- Core load failure yields explicit user-facing message.

## 18. Glossary
| Term | Meaning |
|------|---------|
| Lifecycle | Ordered sequence of significant inventory events for a part or transaction context. |
| Split Transfer | Multiple transfers representing a logical single movement executed as discrete records. |
| Role | Semantic classification of a transaction (receipt, transfer-in, transfer-out, shipment, adjust). |
| Sentinel | A variable set at end-of-file used to confirm successful parse & load of the core script. |
| Today Mode | A filtered view showing only transactions for a single calendar day (navigable). |

## 19. Summary
This specification consolidates functional, structural, and resilience requirements for a maintainable HTA/VBScript Part Lifecycle Tracker. Adhering to the compatibility guidance and pitfalls list will prevent recurrence of prior corruption and loading issues while enabling incremental enhancement (modern UI, lifecycle intelligence, and print fidelity) without destabilizing core reliability.

---
*End of Specification*
