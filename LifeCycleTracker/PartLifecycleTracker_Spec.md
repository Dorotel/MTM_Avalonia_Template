# Part Lifecycle Transaction Tracker (HTA Macro) – Requirements & Technical Specification

## 1. Overview
Starting over (clean slate) design. A Windows HTA (HTML Application) + single supporting VBScript file `PartLifecycleTracker.vbs` (code-behind) to inspect, analyze, and print the lifecycle of part inventory transactions (receipts, transfers, shipments, adjustments) from a Visual/VMFG (SQL Server) database. It provides a modernized UI (Windows 11 inspired) or an optional dark legacy theme and replaces CSV export with in-browser print formatting.

## 0. File Structure (Clean Rebuild Target)
| File | Purpose |
|------|---------|
| `PartLifecycleTracker.hta` | UI container (HTML/CSS) + minimal VBScript glue (only window events & element wiring). |
| `PartLifecycleTracker.vbs` | All business logic: data loading, lifecycle reconstruction, sorting, printing helpers, constants, sentinel. |
| `Database.config` (optional) | Plaintext or simple key=value lines for default server / catalog (optional; falls back to prompts). |
| `README.md` (generated later) | Quick start, troubleshooting, version history. |

Important: The legacy names (`PartLifecycleCore.vbs`, `PartLifecycleCoreClean.vbs`) are retired to avoid confusion and caching issues under mshta.

## 2. Primary Objectives
- Rapidly look up a Part ID and view its recent transactions.
- Reconstruct a chronological lifecycle (movement history and status) from raw transaction records.
- Detect and visually mark transfer pairings and potential split transfers.
- Provide a “Today’s Transactions” mode with day navigation.
- Allow sorting, row selection, lifecycle drill-down, and printing of the currently displayed dataset.
- Offer minimal friction authentication (SQL username/password) at launch.
- Keep implementation self‑contained: `PartLifecycleTracker.hta` + `PartLifecycleTracker.vbs` + optional `Database.config`.

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
- In `PartLifecycleTracker.vbs` always `Dim CORE_LOADED` near the top; set `CORE_LOADED = True` as the last executable line (parse success sentinel).
- HTA `Window_OnLoad` checks `IsEmpty(CORE_LOADED)`; if empty, show a fatal load dialog and abort further initialization.

### 5.13 Error Handling
- Wrap ADO open/execute in `On Error Resume Next` + explicit checks.
- Surface connection / SQL errors in UI instead of silent failure.

### 5.14 Theming / UI Modes
- Modern light (Win11 style) default.
- Optional dark legacy theme retained (maybe separate HTA or a toggle stylesheet injection).

### 5.15 Export (Deprecated)
- Prior CSV export replaced by Print; future: optional CSV/clipboard re-add with explicit user request.
- Initial rebuild will implement only Print; CSV left for backlog.

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

## 7. Tracker Script Function Inventory (Target Set in `PartLifecycleTracker.vbs`)
- `Core_Window_OnLoad` (invoked from HTA `Window_OnLoad`)
- `Core_LoadData`
- `Core_LoadTodaysTransactions`
- `Core_DayPrevious` / `Core_DayForward` (day navigation)
- `Core_SortTable`
- `Core_SelectRow`
- Lifecycle builders: `BuildLifecycleFromDB` (SQL CTE approach) and/or `BuildLifecycleInMemory`
- `Core_ExportData` (Print wrapper)
- Panel toggles: `Core_ToggleLifecycle`, `Core_ToggleResults`
- Helper / utility: `SqlSafe`, `ToSqlDate`, `EnsureCapacity`, comparators, selection re-map.

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
- Files: `PartLifecycleTracker.hta`, `PartLifecycleTracker.vbs`, optional `Database.config`.
- Launch via double-click or a launcher script (`LaunchPartLifecycleTracker.vbs`) which only resolves absolute path & executes `mshta`.
- Ensure `PartLifecycleTracker.vbs` saved ANSI or UTF-8 (no BOM) to avoid legacy parser issues.
- During development set `SINGLEINSTANCE="no"` in the HTA to prevent stale caching; switch to `yes` for production if desired.

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

## 16. Compatibility & Pitfalls to Avoid (Clean Rebuild Focus)
| Pitfall | Explanation | Mitigation |
|---------|-------------|------------|
| Legacy corrupted core remnants | Old `PartLifecycleCore.vbs` fragments may linger and get accidentally included. | Remove / archive legacy files outside working folder before starting. |
| Duplicate `Option Explicit` blocks | Repeated headers in merged files quietly break execution. | Keep a single definitive script (`PartLifecycleTracker.vbs`). |
| Non-VBScript `//` comments | Not valid in VBScript; can stop parsing. | Use `'` or `Rem` only. |
| Sentinel undeclared | `CORE_LOADED` assignment fails under `Option Explicit`. | `Dim CORE_LOADED` near top; set it last. |
| Overuse of silent `On Error Resume Next` | Masks root cause; UI appears frozen. | Limit scope; immediately check `Err` and clear. |
| Fallback stub subs | Hide true missing-function errors. | Let missing subs fail loudly during development. |
| HTA instance caching | Edits ignored when `SINGLEINSTANCE="yes"`. | Use `SINGLEINSTANCE="no"` until stable. |
| Mismatched include filename | HTA still points to old filename, new script never runs. | Standardize on `PartLifecycleTracker.vbs`. |
| `VBScript:` prefix variability | Some HTA builds handle plain names more consistently. | Prefer `onclick="Core_LoadData"` form. |
| UTF-8 BOM issues | BOM can confuse legacy engine sporadically. | Save without BOM. |
| Zone security block | Downloaded files flagged, blocking execution. | Unblock via file Properties dialog. |
| Selection lost after sort | Index-based highlight invalid. | Track by transaction ID and re-find after sort. |
| Credential typos unhandled | Single failed attempt yields misleading state. | Provide retry loop or clear status & reprompt. |
| Large unbounded queries | Performance drag & UI freeze. | Cap row count (e.g., 500/1000) + show count. |
| Inefficient DOM rebuilding | Excess per-row writes slower in IE engine. | Build string buffer then single `innerHTML` update. |
| Mixed date filter semantics | Off-by-one day errors with BETWEEN vs `< nextDay`. | Standardize on `>= from` and `< to + 1 day`. |
| Lifecycle SQL drift | DB schema changes break CTE silently. | Wrap each union part with test query during dev; version comments. |
| Print formatting bleed | Dark theme colors unreadable on paper. | Apply print media override with high-contrast neutrals. |
| Accidental password logging | Debug prints may expose credentials. | Never concatenate password into status/log strings. |
| Time zone ambiguity | Server vs client date mismatch in "today" mode. | Base "today" on server date if exposed (future enhancement). |

### 16.1 Additional Cross-Version Considerations
- Use a single version constant updated per release to aid user support.
- Keep a CHANGELOG section in `README.md` (not in script) to avoid re-parsing cost.
- Consider a minimal self-test routine (optional hidden button) verifying DB connectivity + sentinel.

## 17. Acceptance Criteria (Representative – Clean Rebuild)
- Launch always produces credential prompts (unless user cancels deliberately).
- Loading valid part returns ≥1 row when DB has data; status reflects count.
- Sorting toggles direction and updates arrow or status hint (optional).
- Selecting a row opens lifecycle panel (placeholder acceptable initial, fully reconstructed in final release).
- Print hides controls and produces legible transactional output.
- Transfer split scenarios visibly marked where applicable.
- No undefined function errors (no silent fallbacks); sentinel load check passes; user sees credential prompts on every fresh launch (unless cancelled).
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
