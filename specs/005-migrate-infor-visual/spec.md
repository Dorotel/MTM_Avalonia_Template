# Feature Specification: Manufacturing Application Modernization - All-in-One Mega-Feature

**Feature Branch**: `005-migrate-infor-visual`
**Created**: 2025-10-08
**Status**: Draft
**Input**: Create comprehensive manufacturing application modernization with 5 phases: (1) Custom Controls Library extraction from existing UIs, (2) Settings Management UI with 60+ configurable settings, (3) Debug Terminal complete rewrite with feature-based navigation, (4) Configuration Error Dialog for graceful error handling, and (5) Visual ERP Integration for manufacturing data synchronization. Must follow Constitution Principle XI (reusable custom controls), achieve 80%+ test coverage, and complete outstanding TODOs from Features 002 and 003.

---

## 📑 Table of Contents

### Core Specification
- [Clarifications](#clarifications) - Q&A sessions resolving functional requirements
- [User Scenarios & Testing](#user-scenarios--testing-mandatory) - User stories with acceptance criteria
  - [US1: Custom Controls Library (P1)](#user-story-1---custom-controls-library-creation-priority-p1--foundation)
  - [US2: Settings Management UI (P1)](#user-story-2---settings-management-ui-priority-p1)
  - [US3: Debug Terminal Modernization (P2)](#user-story-3---debug-terminal-modernization-priority-p2)
  - [US4: Configuration Error Handling (P2)](#user-story-4---configuration-error-handling-priority-p2)
  - [US5: Part Information Lookup (P1)](#user-story-5---part-information-lookup-for-manufacturing-priority-p1)
  - [US6: Work Order Operations (P1)](#user-story-6---work-order-operations-for-production-management-priority-p1)
  - [US7: Inventory Transaction Recording (P2)](#user-story-7---inventory-transaction-recording-for-warehouse-operations-priority-p2)
  - [US8: Shipment Confirmation (P2)](#user-story-8---shipment-confirmation-for-fulfillment-priority-p2)
  - [US9: Offline Operation & Performance Monitoring (P3)](#user-story-9---offline-operation--performance-monitoring-priority-p3)

### Requirements
- [Functional Requirements (FR)](#functional-requirements) - Feature capabilities and behaviors
- [Non-Functional Requirements (NFR)](#non-functional-requirements) - Performance, security, reliability
- [Edge Cases & Error Handling](#edge-cases--error-handling) - Failure scenarios and recovery
- [Success Criteria (SC)](#success-criteria) - Definition of done

### Technical Details
- [Key Entities](#key-entities) - Data models and domain objects
- [API Contracts](#api-contracts) - VISUAL API Toolkit integration
- [Database Schema](#database-schema) - Local transaction storage
- [Testing Strategy](#testing-strategy) - Test categories and coverage targets

### Reference
- [Related Files](#related-files) - Links to plan.md, tasks.md, data-model.md
- [Constitution Compliance](#constitution-compliance) - Principle validation checklist

---

## Clarifications

### Session 2025-10-08 (Functional Requirements)

- Q: Which authentication mechanism should the system use with Infor VISUAL API Toolkit? → A: Username/Password - users enter VISUAL credentials at application startup, stored in Windows Credential Manager (already implemented via WindowsSecretsService using DPAPI)
- Q: How long should the system maintain cached VISUAL data before automatic cleanup? → A: Already implemented - Parts: 24 hours, Other entities (work orders, inventory, shipments): 7 days (defined in CacheStalenessDetector)
- Q: Does the Infor VISUAL API Toolkit integration support WRITE operations, or is it read-only access? → A: Read-only ONLY - VISUAL API Toolkit used exclusively for reading data (parts, work orders, inventory levels). All write operations (work order updates, inventory transactions, shipment confirmations) are recorded ONLY to local MAMP MySQL database. No syncing to VISUAL server - MAMP MySQL is the system of record for user-entered transactions.
- Q: Should VISUAL API performance be monitored silently (logs only), with user-facing indicators, or hybrid approach? → A: Hybrid approach - Silent structured logging of all VISUAL API calls (response times, error rates) PLUS user-facing performance indicators integrated into DebugTerminalWindow.axaml. Automatic degradation mode if 5 consecutive requests exceed performance thresholds. UI shows optional "View Performance" button with detailed metrics panel in DebugTerminalWindow.

### Session 2025-10-08 (API Toolkit Availability Discovery)

- Q: Are Infor VISUAL API Toolkit assemblies and documentation accessible for development? → A: ✅ YES - **MAJOR DISCOVERY**: All required files available locally in repository at `docs\Visual Files\`:
  - **API Assemblies** (9 DLLs at `ReferenceFiles\`): LsaCore, LsaShared, VmfgFinancials, VmfgInventory, VmfgPurchasing, VmfgSales, VmfgShared, VmfgShopFloor, VmfgTrace
  - **Reference Documentation** (7 TXT files at `Guides\`): Development Guide, Core, Inventory, Shared Library, Shop Floor, VMFG Shared Library, User Manual
  - **Database Schema** (4 CSV files at `Database Files\`): MTMFG Tables, Procedures, Relationships, Visual Data Table
  - **Connection Configuration**: Database.config with VISUAL instance details (MTMFGPLAY on VISUAL\PLAY server)
  - **Impact**: Original HIGH-RISK assumption about documentation accessibility is RESOLVED. Research phase timeline reduced from 4-8 hours to 1-2 hours (reference guides eliminate API discovery guesswork).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Custom Controls Library Creation (Priority: P1) 🎯 FOUNDATION

As a developer, I need a comprehensive library of reusable custom Avalonia controls extracted from existing UIs so that I can build consistent, maintainable user interfaces across the application without duplicating XAML patterns.

**Why this priority**: Foundation for all subsequent phases. Settings UI, Debug Terminal, and VISUAL integration all depend on these controls. Addresses Constitution Principle XI and TODO from constitution.md (lines 34-37). Reduces XAML duplication by 60%+.

**Independent Test**: Can be fully tested by extracting one control (e.g., StatusCard) → documenting in catalog → using in test view → verifying properties work. Success = Control renders correctly, properties bind, documentation complete.

**Acceptance Scenarios**:

1. **Given** DebugTerminalWindow.axaml contains repeated StatusCard pattern (15+ occurrences), **When** I extract StatusCard custom control, **Then** control renders identically to original pattern with bindable Title, Status, and IconSource properties
2. **Given** StatusCard control is extracted, **When** I document it in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`, **Then** catalog includes control name, purpose, properties table, usage example, and screenshot
3. **Given** MetricDisplay pattern appears 20+ times across views, **When** I extract MetricDisplay control, **Then** control supports Value, Label, Format, and Trend properties with proper StyledProperty declarations
4. **Given** all 10 controls are extracted, **When** I run tests, **Then** each control has 80%+ test coverage for property changes, validation, and rendering

---

### User Story 2 - Settings Management UI (Priority: P1)

As a system administrator, I need a comprehensive settings management UI so that I can configure all application settings (API endpoints, database connections, logging levels, cache policies, feature flags) without editing configuration files directly.

**Why this priority**: Enables non-technical users to configure application. Reduces support burden. Addresses 60+ settings currently requiring manual file edits. Uses custom controls from US1.

**Independent Test**: Can be fully tested by opening Settings window → changing a setting (e.g., theme) → clicking Save → restarting application → verifying change persisted. Success = Settings UI functional with validation and persistence.

**Acceptance Scenarios**:

1. **Given** I open Settings window, **When** window loads, **Then** I see tabbed navigation with 8 categories: General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer
2. **Given** I'm in General Settings tab, **When** I change Theme setting from "Light" to "Dark", **Then** change is staged (not applied) and Save button becomes enabled
3. **Given** I've changed Database Connection string to invalid value, **When** I click Save, **Then** system validates and shows error "Invalid connection string format" with inline feedback
4. **Given** I've made multiple setting changes, **When** I click Cancel, **Then** all changes are discarded and settings revert to previous values
5. **Given** I've saved setting changes, **When** I restart application, **Then** new settings are loaded from UserPreferences table and applied

---

### User Story 3 - Debug Terminal Modernization (Priority: P2)

As a developer, I need a modernized Debug Terminal with feature-based navigation so that I can quickly access diagnostics for specific features (Boot, Config, VISUAL) without scrolling through a single monolithic view.

**Why this priority**: Current Debug Terminal is difficult to navigate. Complete rewrite uses custom controls from US1, addresses Feature 003 TODOs (Copy to Clipboard, IsMonitoring toggle, Environment Variables), improves developer experience.

**Independent Test**: Can be fully tested by opening Debug Terminal → clicking "Feature 001: Boot" menu → viewing Boot Timeline → copying data to clipboard. Success = Navigation works, all sections functional, pending bindings complete.

**Acceptance Scenarios**:

1. **Given** I open Debug Terminal, **When** window loads, **Then** I see SplitView with collapsible side panel containing feature-based menu items (Boot, Config, Diagnostics, VISUAL)
2. **Given** I click "Feature 001: Boot" menu item, **When** content loads, **Then** I see Boot Timeline chart using BootTimelineChart custom control from Phase 1
3. **Given** I'm viewing Performance snapshots, **When** I click "Copy to Clipboard" button, **Then** current snapshot data is copied to system clipboard in JSON format (addresses Feature 003 TODO)
4. **Given** I toggle "IsMonitoring" switch, **When** monitoring is enabled, **Then** system begins collecting performance snapshots every 5 seconds and updates display in real-time (addresses Feature 003 TODO)
5. **Given** I'm viewing Environment Variables section, **When** display loads, **Then** I see filtered list with sensitive variables (PASSWORD, TOKEN, SECRET, KEY) showing as "***FILTERED***" (addresses Feature 003 TODO)

---

### User Story 4 - Configuration Error Handling (Priority: P2)

As a user, I need clear, actionable error messages when configuration issues occur so that I can resolve problems quickly without contacting support.

**Why this priority**: Addresses Feature 002 TODO (ConfigurationErrorDialog), improves user experience, reduces support burden. Small focused feature that integrates with MainWindow error paths.

**Independent Test**: Can be fully tested by causing configuration error (e.g., invalid database connection) → verifying error dialog appears with recovery suggestions. Success = Dialog shows, recovery options work.

**Acceptance Scenarios**:

1. **Given** application detects invalid database configuration during startup, **When** error occurs, **Then** system displays ConfigurationErrorDialog modal with error details and recovery suggestions
2. **Given** ConfigurationErrorDialog is displayed, **When** I click "Edit Settings" button, **Then** Settings window opens to Database tab with invalid setting highlighted
3. **Given** ConfigurationErrorDialog shows "Database unreachable" error, **When** I click "Retry Connection" button, **Then** system attempts to reconnect and closes dialog if successful
4. **Given** critical configuration error prevents startup, **When** dialog displays, **Then** "Exit Application" button is available as last resort

---

### User Story 5 - Part Information Lookup for Manufacturing (Priority: P1)

A manufacturing operator needs to look up part details (description, current inventory level, specifications) while standing at a workstation to verify they're using the correct component for assembly. They scan a barcode or enter a part number, and the system retrieves details from Infor VISUAL ERP within 3 seconds.

**Why this priority**: This is the most frequent user operation (100+ times per shift). Manufacturing stops without reliable part lookups. Directly impacts production throughput and quality.

**Independent Test**: Can be fully tested by performing barcode scan → part lookup → display details flow. Delivers immediate value by enabling operators to verify parts without accessing VISUAL directly. Success = Part details displayed within 3 seconds with 99.9% reliability.

**Acceptance Scenarios**:

1. **Given** operator is on manufacturing floor with network connection, **When** they scan part barcode "12345-ABC", **Then** system displays part description, on-hand quantity, location, and specifications within 3 seconds
2. **Given** operator enters partial part number "12345", **When** they trigger search, **Then** system displays list of matching parts ordered by relevance within 3 seconds
3. **Given** network is unavailable, **When** operator scans previously-viewed part "12345-ABC", **Then** system displays cached part details with "Last updated [timestamp]" indicator within 1 second
4. **Given** part "12345-ABC" does not exist in VISUAL, **When** operator scans barcode, **Then** system displays "Part not found" with option to retry or search similar parts

---

### User Story 6 - Work Order Operations for Production Management (Priority: P1)

A production supervisor needs to view open work orders, check material requirements, update work order status, and record labor hours. They need real-time visibility into what's scheduled, what materials are allocated, and the ability to close work orders when jobs complete.

**Why this priority**: Production scheduling depends on accurate work order data. Delays in status updates cascade to downstream operations. Essential for meeting customer delivery commitments.

**Independent Test**: Can be fully tested by opening work order list → selecting work order → viewing details → updating status flow. Delivers value by centralizing work order management without VISUAL client. Success = Work order operations complete within 2 seconds with real-time synchronization.

**Acceptance Scenarios**:

1. **Given** supervisor views work order dashboard, **When** they filter by "Open" status, **Then** system displays all open work orders with part numbers, quantities, due dates, and current status within 2 seconds
2. **Given** supervisor selects work order "WO-2024-001", **When** details load, **Then** system displays required materials, allocated quantities, labor hours, and operation steps with real-time data from VISUAL
3. **Given** work order "WO-2024-001" is complete, **When** supervisor changes status to "Closed" and saves, **Then** system persists status change to local MAMP MySQL within 2 seconds and confirms successful save with timestamp
4. **Given** network connection to VISUAL is unavailable, **When** supervisor attempts to update work order status, **Then** system still saves change to local MAMP MySQL (VISUAL connectivity not required for write operations - only MAMP MySQL required)

---

### User Story 7 - Inventory Transaction Recording for Warehouse Operations (Priority: P2)

An inventory manager needs to record inventory transactions (receipts, issues, transfers) and have those transactions immediately reflected in VISUAL's inventory balances. They perform these operations multiple times per hour and need confidence that inventory counts are accurate.

**Why this priority**: Inventory accuracy directly impacts production planning and financial reporting. Delayed or lost transactions cause stock discrepancies that require manual reconciliation. High-frequency operation (50+ times per day).

**Independent Test**: Can be fully tested by creating inventory transaction → submitting to VISUAL → verifying inventory balance update flow. Delivers value by enabling warehouse operations without direct VISUAL access. Success = Transaction recorded within 5 seconds with inventory balance updated in VISUAL.

**Acceptance Scenarios**:

1. **Given** inventory manager receives shipment of part "12345-ABC" (qty: 100), **When** they record receipt transaction with reference PO-2024-050, **Then** system persists transaction to local MAMP MySQL within 5 seconds with confirmation and transaction ID
2. **Given** part "12345-ABC" is issued to work order "WO-2024-001" (qty: 10), **When** inventory manager records issue transaction, **Then** system persists transaction to local MAMP MySQL within 5 seconds (local inventory balances updated, VISUAL balances remain unchanged)
3. **Given** MAMP MySQL connection is unavailable, **When** inventory manager attempts to record transaction, **Then** system displays error "Cannot save transaction - local database unavailable" with option to retry (VISUAL connectivity not relevant for writes)
4. **Given** transaction violates cached VISUAL data (e.g., issuing more than cached on-hand quantity), **When** system validates transaction, **Then** system displays warning "This exceeds cached VISUAL inventory. Transaction will be saved to local database. Verify accuracy before proceeding." with option to adjust or confirm

---

### User Story 8 - Shipment Confirmation for Order Fulfillment (Priority: P2)

A warehouse operator needs to confirm shipments by recording tracking numbers, quantities shipped, and shipment dates. These confirmations update customer order status in VISUAL and trigger invoicing processes.

**Why this priority**: Customer satisfaction depends on accurate shipment tracking. Late or missing confirmations delay invoicing and impact cash flow. Directly tied to revenue recognition.

**Independent Test**: Can be fully tested by selecting orders ready to ship → entering shipment details → confirming shipment flow. Delivers value by streamlining shipment workflow without VISUAL client access. Success = Shipment confirmed in VISUAL within 3 seconds with customer order status updated.

**Acceptance Scenarios**:

1. **Given** warehouse operator views orders ready to ship, **When** they filter by ship date (today), **Then** system displays all orders scheduled for shipment with customer names, order numbers, and quantities within 2 seconds
2. **Given** operator selects order "SO-2024-100" for shipment, **When** they enter tracking number "1Z999AA10123456784", shipment date (today), and quantities, **Then** system persists shipment record to local MAMP MySQL within 3 seconds with confirmation and shipment ID
3. **Given** partial shipment of order "SO-2024-100" (50 of 100 units), **When** operator records partial shipment, **Then** system persists partial shipment to local MAMP MySQL with remaining quantity calculated and displayed
4. **Given** MAMP MySQL connection is unavailable, **When** operator attempts to confirm shipment, **Then** system displays error "Cannot save shipment - local database unavailable" with option to retry (VISUAL connectivity not relevant for writes)

---

### User Story 9 - Offline Operation with Cached Data (Priority: P3)

When network connectivity to VISUAL is lost, users need to continue viewing previously-accessed data (parts, work orders, inventory levels) with clear indicators showing data age. When connectivity restores, the system automatically synchronizes queued transactions.

**Why this priority**: Manufacturing environments have intermittent connectivity. Users need to continue operations during outages without manual intervention. Reduces downtime impact.

**Independent Test**: Can be fully tested by simulating network outage → accessing cached data → restoring network → verifying auto-sync flow. Delivers value by ensuring continuity during connectivity issues. Success = Cached data accessible within 1 second, auto-sync completes within 30 seconds of reconnection.

**Acceptance Scenarios**:

1. **Given** network connection to VISUAL is lost, **When** user accesses previously-viewed part "12345-ABC", **Then** system displays cached part details with banner "Offline Mode - Data as of [timestamp]" within 1 second
2. **Given** user is in offline mode (VISUAL unavailable), **When** they attempt to update work order status, **Then** system saves change to local MAMP MySQL successfully (write operations independent of VISUAL connectivity)
3. **Given** network connectivity to VISUAL is restored after 10 minutes, **When** system detects connection, **Then** system refreshes cached VISUAL data (parts, work orders, inventory levels) for read operations only - no transaction syncing occurs
4. **Given** user views previously-entered local transactions while VISUAL is offline, **When** they access transaction history, **Then** system displays all locally-recorded transactions with "Saved to local database [timestamp]" indicator and option to export for external processing

---

### Edge Cases

- **What happens when VISUAL API Toolkit returns timeout (>30s)?** System should cancel request, log timeout event, and display user-friendly error: "VISUAL ERP is taking longer than expected. Would you like to retry or work offline?" with "Retry" and "Go Offline" buttons.

- **How does system handle concurrent updates to the same work order by multiple users?** System should detect conflicts during save, fetch latest VISUAL state, and present merge dialog: "Work order WO-2024-001 was updated by [User] at [Time]. Review changes before saving." with side-by-side comparison.

- **What happens when user's VISUAL credentials expire mid-session?** System should detect 401 Unauthorized response, prompt for credential re-entry with "Your VISUAL session has expired. Please re-authenticate to continue." without losing current work context.

- **How does system handle large result sets (>10,000 records)?** System should implement server-side pagination with VISUAL API, display first 100 results with "Showing 100 of 10,234 results" indicator, and offer "Load More" button rather than client-side crash.

- **What happens when offline queue exceeds storage limits (>1GB cached data)?** System should alert user: "Offline storage is 95% full (950MB of 1GB). Please sync pending transactions to free space." and prevent new offline operations until storage is below threshold.

- **How does system handle VISUAL API Toolkit version mismatches?** System should detect toolkit version on startup, compare against supported versions list, and block operations with clear message: "Infor VISUAL API Toolkit version X.Y detected. This application requires version A.B or later. Please contact IT." if incompatible.

- **How does system handle partial load failures where some VISUAL queries succeed but others timeout?** System should display partial data with clear indicators of what loaded successfully and what failed. Example: If part lookup succeeds (<3s) but work order query times out (>5s), display part details with banner message "Part data loaded successfully. Work orders unavailable (timeout). Using cached work orders from {CacheTimestamp}." with "Retry Work Orders" button. System MUST NOT treat partial failures as complete failures - show what data IS available, clearly indicate what failed, offer targeted retry for failed operations only (not full page refresh). This prevents user frustration when 90% of data loads fine but 10% times out. Related to FR-010 (error handling) and FR-020 (degradation mode) - partial failures should NOT trigger full degradation mode unless 5 CONSECUTIVE requests exceed thresholds.

## Requirements *(mandatory)*

### Functional Requirements

#### Custom Controls (Phase 1)

- **FR-025**: System MUST provide StatusCard custom control with bindable Title, Status, IconSource, and Background properties using StyledProperty pattern
- **FR-026**: System MUST provide MetricDisplay custom control with Value, Label, Format, Trend, and Color properties
- **FR-027**: System MUST provide ErrorListPanel custom control displaying error collections with severity icons and timestamps
- **FR-028**: System MUST provide ConnectionHealthBadge custom control with Status (Healthy/Degraded/Offline) and LastChecked properties
- **FR-029**: System MUST provide BootTimelineChart custom control visualizing boot stage durations with target comparison
- **FR-030**: System MUST provide SettingsCategory custom control with CategoryName, Icon, and Children collection
- **FR-031**: System MUST provide SettingRow custom control with SettingKey, SettingValue, SettingType, and Validation properties
- **FR-032**: System MUST provide NavigationMenuItem custom control with MenuText, Icon, IsSelected, and Command properties
- **FR-033**: System MUST provide ConfigurationErrorDialog custom control with ErrorMessage, RecoveryOptions, and result handling
- **FR-034**: System MUST provide ActionButtonGroup custom control with Buttons collection and consistent spacing
- **FR-035**: System MUST document all custom controls in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with properties, usage examples, and screenshots

#### Settings Management (Phase 2)

- **FR-036**: System MUST provide Settings window with tabbed navigation for 8 categories: General, Database, VISUAL ERP, Logging, UI, Cache, Performance, Developer
- **FR-037**: System MUST display all 60+ IConfigurationService settings in Settings UI with proper grouping by category
- **FR-038**: System MUST validate setting values before save using FluentValidation patterns (database connection strings, URLs, numeric ranges, file paths)
- **FR-039**: System MUST persist setting changes to UserPreferences table per user with timestamp and audit trail
- **FR-040**: System MUST stage setting changes until "Save" is clicked with "Cancel" button to discard
- **FR-041**: System MUST export settings to JSON format with filtered sensitive values (passwords, tokens, keys)
- **FR-042**: System MUST import settings from JSON format with validation and conflict resolution
- **FR-043**: System MUST provide real-time validation feedback with inline error messages for invalid setting values
- **FR-044**: System MUST support setting value types: String, Int, Decimal, Boolean, Enum, FilePath, ConnectionString, URL

#### Debug Terminal (Phase 3)

- **FR-045**: System MUST provide Debug Terminal with SplitView navigation containing collapsible side panel with hamburger menu
- **FR-046**: System MUST organize Debug Terminal content by feature: "Feature 001: Boot", "Feature 002: Config", "Feature 003: Diagnostics", "Feature 005: VISUAL"
- **FR-047**: System MUST implement CopyToClipboardCommand with actual clipboard integration for all Debug Terminal sections (addresses Feature 003 TODO)
- **FR-048**: System MUST implement IsMonitoring toggle with performance snapshot collection every 5 seconds when enabled (addresses Feature 003 TODO)
- **FR-049**: System MUST implement Environment Variables display with filtering for sensitive variables showing as "***FILTERED***" (addresses Feature 003 TODO)
- **FR-050**: System MUST use custom controls from Phase 1 (StatusCard, MetricDisplay, ErrorListPanel, BootTimelineChart) in Debug Terminal content sections
- **FR-051**: Debug Terminal MUST load within 500ms (performance budget)

#### Configuration Error Dialog (Phase 4)

- **FR-052**: System MUST display ConfigurationErrorDialog modal when critical configuration errors occur during startup or runtime
- **FR-053**: Dialog MUST show error details including error message, error category, affected setting key, and timestamp
- **FR-054**: Dialog MUST provide recovery options: "Edit Settings" (opens Settings window to relevant tab), "Retry Operation", "Exit Application"
- **FR-055**: Dialog MUST integrate with MainWindow.axaml error paths (addresses Feature 002 TODO from line 1234)

#### VISUAL ERP Integration (Phase 5)

- **FR-001**: System MUST provide part lookup by barcode scan or manual entry returning part description, on-hand quantity, location, unit of measure, and specifications within 3 seconds (99.9% of requests)

- **FR-002**: System MUST display work order list with filters (status, due date, part number) returning up to 500 work orders with details (part number, quantity, due date, current status, allocated materials) within 2 seconds

- **FR-003**: System MUST allow authorized users to update work order status (Open → In Progress → Closed) with changes persisted to local MAMP MySQL within 2 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record for user-entered work order status changes)

- **FR-004**: System MUST record inventory transactions (receipts, issues, adjustments, transfers) with transaction type, part number, quantity, unit of measure, reference document (PO/WO), and transaction date persisted to local MAMP MySQL within 5 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record for user-entered inventory transactions)

- **FR-005**: System MUST confirm shipments with tracking number, carrier, shipment date, and quantities shipped persisted to local MAMP MySQL within 3 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record for user-entered shipment confirmations)

- **FR-006**: System MUST cache previously-accessed data (parts, work orders, inventory balances) locally with last-updated timestamps enabling read operations within 1 second when VISUAL connection is unavailable

- **FR-006A**: System MUST display cached data age using consistent timestamp format across all UI views: (1) Inline timestamps in data grids use "HH:mm:ss" format (e.g., "14:35:22"), (2) "Last updated" indicators for cached VISUAL data use "yyyy-MM-dd HH:mm:ss" format (e.g., "2025-10-08 14:35:22"), (3) Offline mode banner displays "Offline Mode - Data as of yyyy-MM-dd HH:mm:ss", (4) File exports use "yyyyMMdd-HHmmss" format for filenames (e.g., "diagnostics-export-20251008-143522.json")

- **FR-007**: System MUST persist all write operations (work order updates, inventory transactions, shipment confirmations) to local MAMP MySQL regardless of VISUAL connectivity status (write operations are independent of VISUAL - only MAMP MySQL connection required)

- **FR-008**: System MUST automatically detect VISUAL connectivity restoration and refresh cached read-only data (parts, work orders, inventory levels) within 30 seconds displaying "VISUAL data refreshed" notification (NOTE: No transaction syncing occurs - VISUAL integration is read-only)

- **FR-009**: System MUST authenticate with Infor VISUAL API Toolkit using username/password credentials entered at application startup and securely store credentials using Windows DPAPI (Data Protection API) via WindowsSecretsService (keys: "Visual.Username", "Visual.Password") with automatic credential recovery dialog if credentials become corrupted or unavailable

- **FR-010**: System MUST handle VISUAL API read errors gracefully displaying user-friendly error messages (not raw API exceptions) with actionable recovery options (Retry, Use Cached Data, Contact Support) and persist write operations to local MAMP MySQL regardless of VISUAL connectivity

- **FR-011**: System MUST log all VISUAL API calls (request timestamps, operation types, response times, error codes) to structured log files for troubleshooting without logging sensitive data (credentials, PII)

- **FR-012**: System MUST provide transaction export functionality allowing users to export locally-recorded transactions (work orders, inventory, shipments) to CSV/JSON format for external processing or reporting

- **FR-013**: System MUST validate transactions against cached VISUAL data before recording to local MAMP MySQL, displaying warnings (not blocking) when transactions exceed cached values (e.g., "Issuing 100 units but cached VISUAL inventory shows 50 units on-hand. Verify accuracy.") while still allowing save to MAMP MySQL

- **FR-014**: System MUST support Windows desktop platform only (x86 architecture) with clear messaging displayed when launched on unsupported platforms (Android, iOS, macOS): "VISUAL integration requires Windows. You are running in view-only mode with cached data."

- **FR-015**: System MUST maintain cached data with automatic expiration using TTL (Time-To-Live) policies: Parts cached for 24 hours, all other entities (work orders, inventory transactions, shipments) cached for 7 days, with automatic cleanup removing expired entries during refresh cycles (implemented via CacheStalenessDetector and CacheService)

- **FR-016**: System MUST enforce read-only access to Infor VISUAL ERP via API Toolkit with NO direct write operations to VISUAL server per management security policy - all VISUAL API Toolkit calls limited to read operations (queries, data retrieval) only

- **FR-017**: System MUST maintain MAMP MySQL as system of record for all user-entered transactions (work order updates, inventory transactions, shipment confirmations) with complete audit trail (user, timestamp, original values, new values) - NO synchronization to VISUAL server performed by this application

- **FR-019**: System MUST monitor VISUAL API Toolkit performance using hybrid approach: (1) Silent structured logging of ALL VISUAL API calls (request timestamp, operation type, response time in milliseconds, HTTP status code, error details if failed, cached vs live data indicator) to Serilog logs, AND (2) User-facing performance indicators integrated into DebugTerminalWindow.axaml showing real-time metrics (average response time, success rate, cache hit rate, connection status) with optional "View VISUAL Performance" button for detailed breakdown

- **FR-020**: System MUST automatically trigger degradation mode (stop live VISUAL API calls, serve all requests from cache) if 5 consecutive VISUAL API requests exceed performance thresholds (>5 seconds response time OR HTTP 500+ errors OR timeout exceptions), displaying prominent "VISUAL API unavailable - using cached data" notification in UI with manual "Retry Connection" button and automatic retry every 5 minutes

- **FR-021**: DebugTerminalWindow.axaml MUST include dedicated "VISUAL API Performance" panel showing: (1) Last 10 API call details (timestamp, operation, duration, status), (2) Performance trend chart (response times over last hour), (3) Error history with categorization (timeouts, 500 errors, authentication failures, network errors), (4) Connection pool statistics (active connections, idle connections, total requests, waiting requests, average wait time) with alert displayed when average wait time exceeds 1 second indicating connection pool exhaustion, (5) Cache effectiveness metrics (cache hits vs misses, cached data age), (6) Manual actions with defined outcomes: **Force Refresh** (refresh all cached VISUAL data immediately, display "VISUAL data refreshed" notification), **Clear Cache** (remove all cached entries, refresh cache statistics display), **Test Connection** (validate VISUAL API connectivity, display "Connection Test: SUCCESS/FAILED" with status indicator color), **Export Diagnostics** (export performance metrics and error history to timestamped JSON file in application directory, display export path in log)

- **FR-022**: System MUST display user-friendly empty state UI patterns when VISUAL queries return zero results with actionable guidance for each scenario: (1) **Zero Work Orders** - Display message "No work orders found matching filters. Try adjusting date range or status filters." with "Clear Filters" button, (2) **Zero Inventory Transactions** - Display message "No inventory transactions recorded for this part. Use 'Record Transaction' to add receipts, issues, or adjustments." with "Record Transaction" button navigating to transaction entry form, (3) **Zero Shipments** - Display message "No shipments found for this date range. Expand date range or check shipment confirmation records." with date range picker, (4) **Part Not Found** - Display message "Part '{PartNumber}' not found in VISUAL ERP. Verify part number or search by description." with "Search by Description" button, (5) **Generic Empty Result Set** - For all other queries returning zero results, display message "No data found. {ContextualGuidance}" where ContextualGuidance provides query-specific next steps (e.g., "Try broadening search criteria", "Check filter settings", "Verify date range"). All empty state messages MUST include visual icon (empty box/folder), friendly tone, and at least one actionable button or link to resolve the empty state.

### Key Entities

- **Part**: Represents a manufactured or purchased component in VISUAL ERP. Key attributes include part number (unique identifier), description, unit of measure, on-hand quantity, allocated quantity, location, product class, and specifications (dimensions, weight, material). Relationships: Part → Work Order (many-to-many via material requirements), Part → Inventory Transaction (one-to-many).

- **Work Order**: Represents a manufacturing job scheduled for production. Key attributes include work order number (unique identifier), part number (output), quantity to produce, due date, current status (Open, In Progress, Closed), required materials with allocated quantities, operation steps, and labor hours. Relationships: Work Order → Part (many-to-many for materials), Work Order → Inventory Transaction (one-to-many for material issues).

- **Inventory Transaction**: Represents a change to inventory levels. Key attributes include transaction ID (unique identifier), transaction type (Receipt, Issue, Adjustment, Transfer), part number, quantity, unit of measure, reference document (PO number, WO number), transaction date/time, and user who recorded transaction. Relationships: Inventory Transaction → Part (many-to-one), Inventory Transaction → Work Order (many-to-one, optional).

- **Customer Order / Shipment**: Represents a customer's order and its fulfillment. Key attributes include order number (unique identifier), customer name, order date, ship date, tracking number, carrier, order lines (part number, quantity ordered, quantity shipped), and shipment status (Open, Partially Shipped, Shipped, Invoiced). Relationships: Customer Order → Part (many-to-many via order lines).

- **Local Transaction Record**: Represents a user-entered transaction stored permanently in MAMP MySQL (system of record). Key attributes include transaction ID (unique identifier), transaction type (Work Order Status Update, Inventory Receipt, Inventory Issue, Inventory Adjustment, Inventory Transfer, Shipment Confirmation), entity data (JSON payload with all transaction details), created timestamp, created by user, VISUAL reference data (part number, work order number, order number from cached VISUAL data at time of transaction), and audit metadata. Relationships: Transaction → User (many-to-one). NOTE: This application does NOT sync transactions to VISUAL - MAMP MySQL is permanent storage.

## Success Criteria *(mandatory)*

### Measurable Outcomes

#### Phases 1-4 (Custom Controls, Settings, Debug Terminal, Configuration Error Dialog)

- **SC-007**: Custom control catalog documentation complete with all 10 controls documented (property tables, usage examples, screenshots) within 24 hours of implementation
- **SC-008**: XAML duplication reduced by 60%+ measured by comparing file sizes before/after custom control extraction
- **SC-009**: Settings UI loads within 500ms for 99% of launches measured via performance telemetry
- **SC-010**: 90% of system administrators successfully configure application settings without documentation consultation (measured via user study)
- **SC-011**: Debug Terminal navigation improves discoverability by 50% measured by time-to-find specific diagnostic (before: 30s average, after: 15s average)
- **SC-012**: Configuration error dialog reduces support tickets related to startup issues by 70% compared to baseline (previous 6 months)

#### Phase 5 (VISUAL ERP Integration)

- **SC-001**: Part lookup operations complete in under 3 seconds for 99.9% of requests under normal network conditions (measured via performance telemetry over 30-day period post-deployment)

- **SC-002**: Work order query operations complete in under 2 seconds for result sets up to 500 records for 99.9% of requests (measured via performance telemetry over 30-day period post-deployment)

- **SC-003**: Inventory transaction recording operations complete in under 5 seconds with VISUAL confirmation for 99.5% of requests (measured via performance telemetry over 30-day period post-deployment)

- **SC-004**: System uptime for VISUAL integration reaches 99.9% (measured as successful API calls / total API calls, excluding scheduled VISUAL maintenance windows, over 30-day rolling window)

- **SC-005**: User-reported errors related to VISUAL connectivity or data synchronization decrease by 80% compared to baseline (previous 6 months with HTTP endpoint integration) within 60 days post-deployment

- **SC-006**: Zero user retraining required - existing users complete all primary workflows (part lookup, work order update, inventory transaction) without training sessions or documentation consultation (measured via user observation study with 10 representative users)
