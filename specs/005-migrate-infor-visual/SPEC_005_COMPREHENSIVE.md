# Feature Specification: Manufacturing Application Modernization (Mega-Feature)

**Feature Branch**: `005-migrate-infor-visual`  
**Created**: 2025-10-08  
**Status**: Draft (Comprehensive Restart)  
**Scope**: All-in-one mega-feature combining Custom Controls, Settings UI, Debug Terminal Rewrite, Error Handling, and VISUAL ERP Integration

---

## Executive Summary

This comprehensive feature delivers a complete manufacturing application modernization combining five integrated workstreams:

1. **Custom Controls Library**: Extract 10+ reusable UI components from existing views, reducing XAML duplication by 60%+
2. **Settings Management System**: Build comprehensive settings UI with 60+ configuration options organized in tabbed categories
3. **Debug Terminal Modernization**: Complete rewrite with feature-based side panel navigation and custom controls
4. **Configuration Error Handling**: Implement graceful error recovery dialogs for configuration issues
5. **Infor VISUAL ERP Integration**: Full integration with barcode scanning, offline operation, and manufacturing floor workflows

**Target Users**: Manufacturing floor workers, production managers, inventory clerks, system administrators

**Key Value**: Users gain complete configuration control through intuitive UI, manufacturing workflows operate seamlessly with offline support, and application maintains consistency through reusable components following Constitution Principle XI.

---

## Clarifications

### Session 2025-10-08 (All-In-One Scope Confirmation)

**Q**: Should Feature 005 be split into multiple smaller features (Settings UI, Custom Controls, Debug Terminal, VISUAL Integration) or implemented as single comprehensive feature?

**A**: **All-in-one mega-feature** - Implement everything together in single feature branch. HIGH RISK but delivers complete manufacturing solution. Estimated 3-5x larger than split approach. Proceed with comprehensive specification.

### Session 2025-10-08 (Custom Controls Strategy)

**Q**: How aggressive should custom control extraction be? What occurrence threshold?

**A**: **Aggressive extraction with 3+ occurrence threshold** - Extract any XAML pattern appearing 3 or more times across codebase. Matches Constitution Principle XI default. DebugTerminalWindow has 50+ repeated patterns qualifying for extraction.

**Q**: What naming convention for custom controls?

**A**: **PurposeType format** - Examples: `StatusCard`, `MetricDisplay`, `ErrorListPanel`, `ConnectionHealthBadge`. No unnecessary prefixes. Matches Constitution examples.

**Q**: Should custom controls catalog be created as deliverable?

**A**: **Yes, comprehensive catalog** - Create `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with examples, properties, usage patterns. Constitution TODO requirement. Document as controls are created, don't defer.

### Session 2025-10-08 (Settings UI Design)

**Q**: What settings should be included in Settings UI?

**A**: **All configurable settings** - Include every setting from `IConfigurationService`: API endpoints, timeouts, database connections, UI themes, logging levels, cache settings, folder paths, feature flags. Complete configuration access without file editing.

**Q**: How should settings be organized in UI?

**A**: **Tabbed categories** - Multiple tabs for logical grouping:
- **General**: UI preferences, language, window size
- **Database**: MySQL connection, pool size, timeouts
- **VISUAL ERP**: API endpoints, credentials, sync settings  
- **Advanced**: Cache, performance, telemetry
- **Developer**: Logging levels, debug options, feature flags

Familiar pattern reduces cognitive load.

**Q**: Should settings changes take effect immediately or require Save button?

**A**: **Save button required** - Changes staged until "Save" clicked. "Cancel" button discards changes. Prevents accidental configuration changes. Clear transaction boundaries.

**Q**: Where should user settings persist?

**A**: **Database per-user** - Store in `UserPreferences` table (existing pattern from Feature 002). Supports multi-user scenarios. Falls back to local JSON if database unavailable.

### Session 2025-10-08 (Debug Terminal Refactoring)

**Q**: How should Debug Terminal side panel navigation be organized?

**A**: **Feature-based navigation** - Menu items organized by feature number:
- Feature 001: Boot Sequence
- Feature 002: Configuration  
- Feature 003: Diagnostics
- Feature 005: VISUAL Integration

Clear structure aligned with project organization.

**Q**: What UI framework for side panel?

**A**: **Avalonia SplitView** - Collapsible side panel with hamburger menu (mobile app pattern). Modern, familiar UX. Built-in resize handle.

**Q**: Should Debug Terminal content be preserved or completely rewritten?

**A**: **Complete rewrite from scratch** - Use as opportunity to redesign cleanly with custom controls. Extract repeated patterns during rewrite. **HIGH RISK but cleaner result**. Maintain functional equivalence.

### Session 2025-10-08 (Pending TODOs)

**Q**: Should Feature 003 incomplete ViewModel bindings be completed in Feature 005?

**A**: **Yes, complete all pending bindings** - Implement:
- Copy to Clipboard functionality  
- IsMonitoring toggle (start/stop performance snapshot collection)
- Environment variables display with sensitive data filtering

Wire up all incomplete features from Feature 003.

**Q**: Should Constitution TODO for custom control catalog be addressed?

**A**: **Part of Feature 005 deliverables** - Create catalog as controls are developed. Don't defer to future features.

**Q**: Should MainWindow.axaml ConfigurationErrorDialog TODO be implemented?

**A**: **Yes, implement now** - Complete as part of Feature 005. Show error details with recovery suggestions. Integrate with `IErrorNotificationService`.

### Session 2025-10-08 (Testing Approach)

**Q**: What testing approach for Feature 005?

**A**: **Test-first (TDD)** - Constitution Principle IV requirement. Write tests BEFORE implementation. Prevents regressions during large refactoring.

**Q**: Should custom controls have dedicated tests?

**A**: **Yes, test properties and behavior** - Test `StyledProperty` defaults, validation, change notifications. Visual tests via Avalonia previewer.

**Q**: What test coverage target?

**A**: **Good coverage (80%+)** - Constitution standard. Cover critical paths: ViewModels, service methods, custom control properties, configuration persistence.

### Session 2025-10-08 (Implementation Mode)

**Q**: Should Radio Silence Mode be used for Feature 005 implementation?

**A**: **Yes, Radio Silence** - AI works through entire feature with minimal interaction. Produces concrete deliverables (patches, tests, documentation).

**Q**: What deliverable format?

**A**: **Patch files (unified diffs)** - Radio Silence standard format. Easy to apply with git. Include test results and coverage reports.

### Session 2025-10-08 (VISUAL ERP Integration Scope)

**Q**: Which VISUAL ERP features should be included in Feature 005?

**A**: **Complete VISUAL integration** - Parts lookup, work orders, inventory management, barcode scanning, offline operation with sync queue, connection health monitoring, production metrics dashboard.

**Q**: Should VISUAL integration write operations be supported?

**A**: **Read-only ONLY** - VISUAL API Toolkit used exclusively for reading data. All write operations (work order updates, inventory transactions) recorded ONLY to local MAMP MySQL. No syncing to VISUAL server. MAMP MySQL is system of record.

**Q**: What offline capabilities required?

**A**: **Offline-first operation** - Parts, work orders, inventory data cached locally with LZ4 compression. Offline mode activates within 100ms of connection loss. Graceful degradation when VISUAL unavailable.

---

## User Scenarios & Testing

### User Story 1: Manufacturing Floor Worker - Part Lookup with Barcode Scanning (Priority: P1)

**As a** manufacturing floor worker  
**I want to** scan a part barcode and instantly see inventory levels, location, and specifications  
**So that** I can verify I'm using the correct component without leaving the workstation

**Why P1**: Most frequent operation (100+ times per shift). Manufacturing stops without reliable part lookups. Directly impacts production throughput and quality.

**Independent Value**: Delivers immediate value by enabling operators to verify parts without accessing VISUAL directly. Works offline with cached data.

**Acceptance Scenarios**:

1. **Given** operator is on manufacturing floor with network connection, **When** they scan part barcode "12345-ABC", **Then** system displays part description, on-hand quantity, location, and specifications within 2 seconds
2. **Given** operator enters partial part number "12345" in search box, **When** they trigger search, **Then** system displays list of matching parts ordered by relevance within 2 seconds
3. **Given** network is unavailable (offline mode), **When** operator scans previously-viewed part "12345-ABC", **Then** system displays cached part details with "Last updated [timestamp]" indicator within 1 second
4. **Given** part "12345-ABC" does not exist in VISUAL, **When** operator scans barcode, **Then** system displays "Part not found" with option to retry or search similar parts
5. **Given** barcode scanner is properly configured, **When** operator scans barcode in under 10 seconds, **Then** system processes scan and displays results without manual input required

**Testing**: Verify barcode scan → API lookup → cache write → UI display flow. Measure response times under load (100+ scans per hour).

---

### User Story 2: Production Manager - Work Order Operations (Priority: P1)

**As a** production supervisor  
**I want to** view open work orders, check material requirements, and update job status  
**So that** I can manage production schedule and meet customer delivery commitments

**Why P1**: Production scheduling depends on accurate work order data. Delays in status updates cascade to downstream operations.

**Independent Value**: Centralizes work order management without VISUAL client. Enables mobile access from factory floor.

**Acceptance Scenarios**:

1. **Given** supervisor views work order dashboard, **When** they filter by "Open" status, **Then** system displays all open work orders with part numbers, quantities, due dates, current status within 2 seconds
2. **Given** supervisor selects work order "WO-2024-001", **When** details load, **Then** system displays required materials, allocated quantities, labor hours, operation steps with real-time VISUAL data
3. **Given** work order "WO-2024-001" is complete, **When** supervisor changes status to "Closed" and saves, **Then** system persists status change to local MAMP MySQL within 2 seconds with confirmation
4. **Given** network connection to VISUAL unavailable (offline mode), **When** supervisor updates work order status, **Then** system still saves change to local MAMP MySQL (VISUAL connectivity not required for writes)

**Testing**: Verify work order list → selection → details → status update → persistence flow. Test offline mode with database writes.

---

### User Story 3: System Administrator - Complete Application Configuration (Priority: P1)

**As a** system administrator  
**I want to** configure all application settings through intuitive UI  
**So that** I can manage deployments without editing configuration files or database records

**Why P1**: Configuration errors are leading cause of support tickets. File editing requires technical expertise. Settings UI democratizes configuration management.

**Independent Value**: Immediate value by eliminating manual configuration file editing. Reduces deployment time from hours to minutes.

**Acceptance Scenarios**:

1. **Given** administrator opens Settings window, **When** they navigate to "VISUAL ERP" tab, **Then** system displays all VISUAL-related settings (base URL, timeout, retry count, credentials) within 500ms
2. **Given** administrator changes "Visual:Timeout" from 30 to 60 seconds, **When** they click Save, **Then** system persists change to `UserPreferences` table and displays "Settings saved successfully" confirmation within 1 second
3. **Given** administrator modifies database connection settings requiring restart, **When** they click Save, **Then** system displays warning "These changes require application restart. Restart now?" with Yes/No buttons
4. **Given** settings validation fails (invalid URL format), **When** administrator attempts to save, **Then** system displays inline error "Invalid URL format. Must start with https://" and disables Save button until corrected
5. **Given** database connection unavailable, **When** administrator saves settings, **Then** system persists to local JSON fallback and displays "Saved locally (database unavailable). Changes will sync when connection restored."

**Testing**: Verify settings load → modify → validate → save → persist flow. Test validation rules for all 60+ settings. Confirm restart prompts for applicable settings.

---

### User Story 4: Developer - Debug Application Performance Issues (Priority: P2)

**As a** developer troubleshooting performance issues  
**I want to** view real-time diagnostics with feature-organized navigation  
**So that** I can quickly identify bottlenecks without searching through scattered logs

**Why P2**: Debug Terminal currently difficult to navigate (80+ scattered metrics). Feature-based organization improves troubleshooting efficiency. Custom controls reduce maintenance burden.

**Independent Value**: Developers gain intuitive diagnostic tool. Feature-based navigation maps directly to codebase structure.

**Acceptance Scenarios**:

1. **Given** developer opens Debug Terminal, **When** window loads, **Then** system displays side panel with feature-based navigation (Boot, Config, Diagnostics, VISUAL) and performance snapshot within 500ms
2. **Given** developer selects "Feature 005: VISUAL" from side panel, **When** view loads, **Then** system displays VISUAL-specific metrics (API call history, response times, cache hit rate, connection pool stats) using custom controls
3. **Given** developer clicks "Export Diagnostics" button, **When** export completes, **Then** system saves timestamped JSON file to `diagnostics-export-{timestamp}.json` and displays file path in notification
4. **Given** developer toggles "IsMonitoring" switch ON, **When** monitoring activates, **Then** system begins collecting performance snapshots every 30 seconds and updates UI in real-time
5. **Given** developer clicks "Copy to Clipboard" on error log entry, **When** action completes, **Then** system copies error details (timestamp, severity, message, stack trace) to clipboard and shows "Copied" confirmation

**Testing**: Verify side panel navigation → feature views → custom control rendering → export/copy actions. Measure UI responsiveness with 100+ error log entries.

---

### User Story 5: Inventory Manager - Record Transactions with Offline Support (Priority: P2)

**As an** inventory manager  
**I want to** record inventory transactions (receipts, issues, transfers) with offline capability  
**So that** I can continue warehouse operations during network outages without data loss

**Why P2**: Inventory accuracy directly impacts production planning. Delayed transactions cause stock discrepancies requiring manual reconciliation. High-frequency operation (50+ per day).

**Independent Value**: Enables warehouse operations without VISUAL access. Offline queue prevents data loss during outages.

**Acceptance Scenarios**:

1. **Given** inventory manager receives shipment of part "12345-ABC" (qty: 100), **When** they record receipt transaction with reference PO-2024-050, **Then** system persists transaction to local MAMP MySQL within 5 seconds with confirmation
2. **Given** part "12345-ABC" issued to work order "WO-2024-001" (qty: 10), **When** inventory manager records issue transaction, **Then** system persists to MAMP MySQL within 5 seconds (local inventory balances updated)
3. **Given** MAMP MySQL connection unavailable, **When** inventory manager attempts to record transaction, **Then** system displays error "Cannot save transaction - local database unavailable" with Retry option
4. **Given** transaction violates cached VISUAL data (issuing more than cached on-hand quantity), **When** system validates, **Then** displays warning "This exceeds cached VISUAL inventory. Transaction will be saved to local database. Verify accuracy before proceeding." with Continue/Cancel options

**Testing**: Verify transaction entry → validation → persistence flow. Test offline queue with network failures. Measure transaction throughput (100+ per hour).

---

### User Story 6: End User - Visual Consistency Through Reusable Controls (Priority: P3)

**As a** end user navigating multiple application screens  
**I want** consistent visual design patterns (cards, metrics, buttons, indicators)  
**So that** I can quickly recognize UI elements and complete tasks efficiently without learning new patterns

**Why P3**: Constitution Principle XI mandates reusable controls. Current codebase has 50+ repeated XAML patterns. Consistency improves usability and reduces maintenance.

**Independent Value**: Immediate value through reduced development time for new features. Consistency reduces user training time.

**Acceptance Scenarios**:

1. **Given** user views Debug Terminal performance metrics, **When** screen loads, **Then** all metric displays use consistent `MetricDisplay` control with uniform label/value/status layout
2. **Given** user navigates between Settings and Debug Terminal, **When** viewing status indicators, **Then** all status badges use consistent `ConnectionHealthBadge` control with uniform colors (green=connected, red=disconnected, yellow=connecting)
3. **Given** user views error lists across different screens, **When** comparing layouts, **Then** all error displays use consistent `ErrorListPanel` control with uniform severity icons, timestamps, messages
4. **Given** developer adds new diagnostic feature, **When** implementing UI, **Then** developer references `docs/UI-CUSTOM-CONTROLS-CATALOG.md` and reuses existing controls without creating duplicates

**Testing**: Visual regression tests comparing control usage across all screens. Verify custom controls catalog documentation complete with examples.

---

### Edge Cases

#### Configuration Settings

- **What happens when user modifies setting that requires application restart?** System displays modal dialog: "Changes to database connection require application restart. Restart now?" with "Restart Now" and "Restart Later" buttons. If "Restart Later", show persistent notification banner until restart.

- **How does system handle invalid setting values during save?** System validates all settings before save. Display inline errors next to invalid fields (red border + error message). Disable Save button until all validations pass. Show error summary at top: "3 validation errors must be corrected before saving."

- **What happens when database unavailable during settings save?** System persists settings to local JSON fallback (`cache/user-preferences.json`). Display notification: "Settings saved locally (database unavailable). Changes will sync when connection restored." Auto-sync when database connection restored.

- **How does system handle concurrent settings modifications by multiple users?** Settings are per-user in `UserPreferences` table. No conflicts possible. Each user has independent configuration.

#### Custom Controls

- **What happens when custom control property validation fails?** Control displays validation error via `Validation.Errors` attached property. Parent form disables action buttons until validation passes. Example: `MetricDisplay` with invalid numeric value shows red border + tooltip error.

- **How do custom controls handle design-time vs runtime mode?** Controls check `Design.IsDesignMode` property. In design mode, display sample data for preview. In runtime, bind to actual ViewModel properties.

#### Debug Terminal

- **What happens when Debug Terminal displays 1000+ error log entries?** System implements virtualization using `VirtualizingStackPanel`. Only visible entries rendered. Performance remains <16ms per frame (60 FPS). Scrolling smooth regardless of entry count.

- **How does Debug Terminal handle real-time updates while user scrolling?** System pauses real-time updates when user scrolls away from bottom. Display "Paused (scrolled up)" indicator. Resume updates when user scrolls to bottom or clicks "Jump to Latest" button.

- **What happens when user exports diagnostics with 100MB+ of data?** System streams export to file without loading entire dataset into memory. Display progress bar during export. Cancel button available. Estimated time shown based on export rate.

#### VISUAL Integration

- **What happens when VISUAL API returns timeout (>30s)?** System cancels request, logs timeout event, displays user-friendly error: "VISUAL ERP is taking longer than expected. Would you like to retry or work offline?" with "Retry" and "Go Offline" buttons.

- **How does system handle concurrent updates to same work order by multiple users?** System detects conflicts during save, fetches latest VISUAL state, presents merge dialog: "Work order WO-2024-001 was updated by [User] at [Time]. Review changes before saving." with side-by-side comparison.

- **What happens when user's VISUAL credentials expire mid-session?** System detects 401 Unauthorized response, prompts for credential re-entry: "Your VISUAL session has expired. Please re-authenticate to continue." without losing current work context.

- **How does system handle large result sets (>10,000 records)?** System implements server-side pagination with VISUAL API. Display first 100 results with "Showing 100 of 10,234 results" indicator. Offer "Load More" button rather than client-side crash.

- **What happens when offline queue exceeds storage limits (>1GB cached data)?** System alerts user: "Offline storage is 95% full (950MB of 1GB). Please sync pending transactions to free space." Prevent new offline operations until storage below threshold.

- **How does system handle VISUAL API Toolkit version mismatches?** System detects toolkit version on startup, compares against supported versions list. Block operations with clear message: "Infor VISUAL API Toolkit version X.Y detected. This application requires version A.B or later. Please contact IT." if incompatible.

- **How does system handle partial load failures?** System displays partial data with clear indicators. Example: If part lookup succeeds (<3s) but work order query times out (>5s), display part details with banner: "Part data loaded successfully. Work orders unavailable (timeout). Using cached work orders from {CacheTimestamp}." with "Retry Work Orders" button. Partial failures do NOT trigger full degradation mode unless 5 consecutive requests exceed thresholds.

---

## Requirements

### Functional Requirements

#### Custom Controls Library (FR-100 Series)

- **FR-101**: System MUST provide `StatusCard` control with properties: `Title` (string), `Icon` (PathGeometry?), `Content` (object), `ContentTemplate` (DataTemplate?) usable across all views requiring card-based layout

- **FR-102**: System MUST provide `MetricDisplay` control with properties: `Label` (string), `Value` (object), `Unit` (string?), `Status` (string?), `StatusColor` (Brush?) displaying labeled metrics with optional status indicators

- **FR-103**: System MUST provide `ErrorListPanel` control with properties: `Errors` (IEnumerable<ErrorLogEntry>), `MaxHeight` (double), `ShowTimestamps` (bool), `ShowSeverityIcons` (bool) displaying error lists with severity indicators and virtualization for 1000+ entries

- **FR-104**: System MUST provide `ConnectionHealthBadge` control with properties: `Status` (ConnectionStatus enum: Connected, Disconnected, Connecting, Error), `Label` (string) displaying connection status with color-coded indicators (green=connected, red=disconnected, yellow=connecting, red=error)

- **FR-105**: System MUST provide `BootTimelineChart` control with properties: `Timeline` (IEnumerable<BootTimelineEntry>), `MaxDuration` (long), `ShowTargets` (bool) displaying boot stage timeline with duration bars and target indicators

- **FR-106**: System MUST provide `SettingsCategory` control with properties: `Title` (string), `Icon` (PathGeometry?), `IsExpanded` (bool), `Content` (object) grouping settings with collapsible header

- **FR-107**: System MUST provide `SettingRow` control with properties: `Label` (string), `Description` (string?), `InputControl` (Control) displaying individual setting with label, optional description, and input control

- **FR-108**: System MUST provide `NavigationMenuItem` control with properties: `Label` (string), `Icon` (PathGeometry), `IsSelected` (bool), `Command` (ICommand) for side panel navigation menus

- **FR-109**: System MUST provide `ActionButtonGroup` control with properties: `Orientation` (Orientation), `Spacing` (double) grouping action buttons with consistent styling

- **FR-110**: System MUST provide `ConfigurationErrorDialog` control with properties: `ErrorMessage` (string), `RecoveryOptions` (IEnumerable<string>), `OnRecoverySelected` (ICommand) displaying configuration errors with recovery suggestions

- **FR-111**: System MUST document all custom controls in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with: control name, purpose, properties, events, usage examples, visual preview, best practices, known limitations

- **FR-112**: System MUST implement all custom controls using `StyledProperty` pattern for bindable properties with proper default values, validation, change notifications

- **FR-113**: System MUST reduce XAML duplication by 60%+ across codebase by replacing repeated patterns with custom control references (measured by lines of duplicate XAML before vs after)

#### Settings Management System (FR-200 Series)

- **FR-201**: System MUST provide Settings window with TabControl containing 5 categories: General (UI preferences), Database (MySQL connection), VISUAL ERP (API integration), Advanced (cache, performance), Developer (logging, feature flags)

- **FR-202**: System MUST display all `IConfigurationService` settings (60+ configuration options) organized by category with appropriate input controls (TextBox, ComboBox, CheckBox, Slider, NumericUpDown)

- **FR-203**: System MUST validate all settings before persistence with inline error messages displayed next to invalid fields (red border + error tooltip) and Save button disabled until all validations pass

- **FR-204**: System MUST persist user settings to `UserPreferences` table (per-user) using existing Feature 002 `ConfigurationService` pattern with fallback to local JSON (`cache/user-preferences.json`) if database unavailable

- **FR-205**: System MUST stage settings changes until Save button clicked with Cancel button discarding all pending changes and Reset button reverting to default values per setting

- **FR-206**: System MUST display restart prompt for settings requiring application restart (database connection, API base URL, boot configuration) with "Restart Now" and "Restart Later" options

- **FR-207**: System MUST complete settings screen load (render all tabs with values populated) within 500ms from window open

- **FR-208**: System MUST complete settings save operation (validate + persist to database + update in-memory configuration) within 1 second from Save button click

- **FR-209**: System MUST provide export/import functionality for settings backup with JSON format including metadata (timestamp, app version, platform) and validation on import

- **FR-210**: Settings window MUST support keyboard navigation (Tab, Arrow keys, Enter to save, Escape to cancel) with focus indicators and accessibility ARIA labels

#### Debug Terminal Modernization (FR-300 Series)

- **FR-301**: System MUST rewrite Debug Terminal using `SplitView` control with collapsible side panel (250px default width) containing feature-based navigation menu with hamburger toggle button

- **FR-302**: Debug Terminal side panel MUST display navigation menu items organized by features: "Feature 001: Boot", "Feature 002: Configuration", "Feature 003: Diagnostics", "Feature 005: VISUAL Integration" using `NavigationMenuItem` custom control

- **FR-303**: System MUST replace all repeated XAML patterns in Debug Terminal with custom controls: `StatusCard` for performance snapshots, `MetricDisplay` for individual metrics, `ErrorListPanel` for error history, `ConnectionHealthBadge` for connection status, `BootTimelineChart` for boot timeline

- **FR-304**: System MUST implement "Copy to Clipboard" functionality for error log entries (pending Feature 003 TODO) copying timestamp, severity, message, stack trace in structured text format with "Copied" confirmation notification

- **FR-305**: System MUST implement "IsMonitoring" toggle switch (pending Feature 003 TODO) starting/stopping performance snapshot collection every 30 seconds with real-time UI updates

- **FR-306**: System MUST implement environment variables display (pending Feature 003 TODO) with sensitive data filtering (variables containing PASSWORD, TOKEN, SECRET, KEY shown as `***FILTERED***`) and copy-to-clipboard per variable

- **FR-307**: Debug Terminal "VISUAL API Performance" panel MUST display: Last 10 API calls (timestamp, operation, duration, status), performance trend chart (response times over last hour), error history with categorization (timeouts, 500 errors, auth failures, network errors), connection pool statistics (active, idle, total requests, waiting requests, average wait time with alert when >1 second)

- **FR-308**: Debug Terminal MUST provide manual action buttons with defined outcomes: "Force Refresh" (refresh all cached VISUAL data immediately, display "VISUAL data refreshed" notification), "Clear Cache" (remove all cached entries, refresh cache statistics), "Test Connection" (validate VISUAL API connectivity, display "Connection Test: SUCCESS/FAILED" with status color), "Export Diagnostics" (export to timestamped JSON, display file path)

- **FR-309**: Debug Terminal MUST maintain functional equivalence with current implementation (all existing metrics, error logging, boot timeline, performance snapshots) while improving organization and usability

- **FR-310**: Debug Terminal MUST implement virtualization for error lists (1000+ entries) using `VirtualizingStackPanel` maintaining <16ms render time per frame (60 FPS)

#### Configuration Error Handling (FR-400 Series)

- **FR-401**: System MUST implement `ConfigurationErrorDialog` as modal dialog displayed when critical configuration errors occur (database connection failure, invalid settings, missing credentials)

- **FR-402**: `ConfigurationErrorDialog` MUST display error message with user-friendly description (no raw exceptions), error category (Database, Credentials, Settings, Network), and timestamp

- **FR-403**: `ConfigurationErrorDialog` MUST provide recovery options appropriate to error type: Database errors → "Retry Connection", "Check Settings", "Use Offline Mode"; Credential errors → "Re-enter Credentials", "Reset Credentials"; Settings errors → "Open Settings", "Reset to Defaults"

- **FR-404**: System MUST integrate `ConfigurationErrorDialog` with `IErrorNotificationService` for centralized error handling with automatic categorization using existing `ErrorCategorizer`

- **FR-405**: System MUST complete MainWindow.axaml TODO (Feature 002) integrating `ConfigurationErrorDialog` into error handling paths for configuration-related failures

#### Infor VISUAL ERP Integration (FR-500 Series)

##### Core Integration

- **FR-501**: System MUST provide part lookup by barcode scan or manual entry returning part description, on-hand quantity, location, unit of measure, specifications within 2 seconds (99.9% of requests)

- **FR-502**: System MUST display work order list with filters (status, due date, part number) returning up to 500 work orders with details (part number, quantity, due date, current status, allocated materials) within 2 seconds

- **FR-503**: System MUST allow authorized users to update work order status (Open → In Progress → Closed) with changes persisted to local MAMP MySQL within 2 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record for user-entered work order status changes)

- **FR-504**: System MUST record inventory transactions (receipts, issues, adjustments, transfers) with transaction type, part number, quantity, unit of measure, reference document (PO/WO), transaction date persisted to local MAMP MySQL within 5 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record)

- **FR-505**: System MUST confirm shipments with tracking number, carrier, shipment date, quantities shipped persisted to local MAMP MySQL within 3 seconds (NOTE: VISUAL API Toolkit is read-only - MAMP MySQL is system of record)

##### Caching & Offline Operation

- **FR-506**: System MUST cache previously-accessed data (parts, work orders, inventory balances) locally with last-updated timestamps using LZ4 compression enabling read operations within 1 second when VISUAL connection unavailable

- **FR-507**: System MUST display cached data age using consistent timestamp format: Inline timestamps use "HH:mm:ss" format, "Last updated" indicators use "yyyy-MM-dd HH:mm:ss" format, offline mode banner displays "Offline Mode - Data as of yyyy-MM-dd HH:mm:ss", file exports use "yyyyMMdd-HHmmss" format for filenames

- **FR-508**: System MUST persist all write operations (work order updates, inventory transactions, shipment confirmations) to local MAMP MySQL regardless of VISUAL connectivity status (write operations independent of VISUAL - only MAMP MySQL connection required)

- **FR-509**: System MUST automatically detect VISUAL connectivity restoration and refresh cached read-only data (parts, work orders, inventory levels) within 30 seconds displaying "VISUAL data refreshed" notification (NOTE: No transaction syncing - VISUAL integration is read-only)

- **FR-510**: System MUST activate offline mode within 100ms of connection loss detection with UI banner: "Offline Mode - Using cached data as of [timestamp]"

##### Authentication & Security

- **FR-511**: System MUST authenticate with Infor VISUAL API Toolkit using username/password credentials entered at application startup and securely store using Windows DPAPI via `WindowsSecretsService` (keys: "Visual.Username", "Visual.Password") with automatic credential recovery dialog if credentials corrupted or unavailable

- **FR-512**: System MUST handle VISUAL API read errors gracefully displaying user-friendly error messages (not raw API exceptions) with actionable recovery options (Retry, Use Cached Data, Contact Support) and persist write operations to local MAMP MySQL regardless of VISUAL connectivity

##### Monitoring & Logging

- **FR-513**: System MUST log all VISUAL API calls (request timestamps, operation types, response times, error codes) to structured log files for troubleshooting without logging sensitive data (credentials, PII)

- **FR-514**: System MUST monitor VISUAL API Toolkit performance using hybrid approach: (1) Silent structured logging of ALL VISUAL API calls (request timestamp, operation type, response time in ms, HTTP status code, error details, cached vs live indicator), AND (2) User-facing performance indicators in Debug Terminal showing real-time metrics (average response time, success rate, cache hit rate, connection status)

- **FR-515**: System MUST automatically trigger degradation mode (stop live VISUAL API calls, serve all requests from cache) if 5 consecutive VISUAL API requests exceed performance thresholds (>5 seconds response time OR HTTP 500+ errors OR timeout exceptions), displaying prominent "VISUAL API unavailable - using cached data" notification with manual "Retry Connection" button and automatic retry every 5 minutes

##### Data Management

- **FR-516**: System MUST provide transaction export functionality allowing users to export locally-recorded transactions (work orders, inventory, shipments) to CSV/JSON format for external processing or reporting

- **FR-517**: System MUST validate transactions against cached VISUAL data before recording to local MAMP MySQL, displaying warnings (not blocking) when transactions exceed cached values (e.g., "Issuing 100 units but cached VISUAL inventory shows 50 units on-hand. Verify accuracy.") while still allowing save to MAMP MySQL

- **FR-518**: System MUST maintain cached data with automatic expiration using TTL policies: Parts cached for 24 hours, all other entities (work orders, inventory transactions, shipments) cached for 7 days, with automatic cleanup removing expired entries during refresh cycles

- **FR-519**: System MUST enforce read-only access to Infor VISUAL ERP via API Toolkit with NO direct write operations to VISUAL server per management security policy

- **FR-520**: System MUST maintain MAMP MySQL as system of record for all user-entered transactions (work order updates, inventory transactions, shipment confirmations) with complete audit trail (user, timestamp, original values, new values) - NO synchronization to VISUAL server performed

##### Barcode Scanning

- **FR-521**: System MUST support barcode scanning with configurable timeout (`Visual:BarcodeTimeout` setting, default 10 seconds) for formats: Code 39, Code 128, UPC-A, UPC-E, EAN-13, EAN-8, QR Code with audio feedback on successful scan

- **FR-522**: System MUST process barcode scan → part lookup → display flow within 2 seconds total (scan recognition + API call + UI render)

##### User Experience

- **FR-523**: System MUST display user-friendly empty state UI patterns when VISUAL queries return zero results with actionable guidance: "No work orders found matching filters" with "Clear Filters" button, "No inventory transactions recorded for this part" with "Record Transaction" button, "No shipments found for this date range" with date range picker, "Part not found" with "Search by Description" button

- **FR-524**: System MUST support Windows desktop platform only (x86 architecture) with clear messaging when launched on unsupported platforms: "VISUAL integration requires Windows. You are running in view-only mode with cached data."

### Non-Functional Requirements

#### Performance (NFR-100 Series)

- **NFR-101**: Settings screen MUST load within 500ms from window open to render complete (all tabs populated)

- **NFR-102**: Settings save MUST complete within 1 second from Save button click to persistence confirmation

- **NFR-103**: VISUAL API part lookup MUST complete within 2 seconds for 99.9% of requests

- **NFR-104**: VISUAL API work order queries MUST complete within 2 seconds for result sets up to 500 records (99.9% of requests)

- **NFR-105**: Inventory transaction recording MUST complete within 5 seconds (99.5% of requests)

- **NFR-106**: Offline mode activation MUST complete within 100ms of connection loss detection

- **NFR-107**: Custom control render MUST complete within 16ms per frame (60 FPS) for smooth UI animations

- **NFR-108**: Debug Terminal MUST maintain <16ms render time with 1000+ error log entries using virtualization

- **NFR-109**: Configuration retrieval via `IConfigurationService.GetValue<T>()` MUST complete within 100ms

- **NFR-110**: Database queries for user preferences MUST complete within 500ms

#### Scalability (NFR-200 Series)

- **NFR-201**: System MUST support VISUAL result sets up to 10,000 records with server-side pagination (100 records per page)

- **NFR-202**: System MUST support offline cache up to 1GB with automatic cleanup when 95% full

- **NFR-203**: Debug Terminal MUST virtualize error lists supporting 10,000+ entries without performance degradation

- **NFR-204**: Connection pool MUST support 5 concurrent VISUAL API requests with queuing for additional requests

#### Reliability (NFR-300 Series)

- **NFR-301**: VISUAL integration MUST achieve 99.9% uptime (measured as successful API calls / total API calls, excluding VISUAL maintenance windows, over 30-day rolling window)

- **NFR-302**: Settings persistence MUST achieve 100% success rate with fallback to local JSON if database unavailable

- **NFR-303**: Custom controls MUST maintain backward compatibility with existing ViewModel bindings during refactoring

#### Usability (NFR-400 Series)

- **NFR-401**: Zero user retraining required - existing users MUST complete all primary workflows (part lookup, work order update, inventory transaction) without training sessions or documentation

- **NFR-402**: Settings window MUST support full keyboard navigation (Tab, Arrow keys, Enter to save, Escape to cancel)

- **NFR-403**: All UI text MUST use consistent terminology (no synonyms for same concept: "Work Order" not "Job", "Part" not "Item")

- **NFR-404**: Error messages MUST be user-friendly (no technical jargon, raw exceptions, or stack traces)

#### Maintainability (NFR-500 Series)

- **NFR-501**: Custom controls MUST reduce XAML duplication by 60%+ (measured by lines of duplicate XAML before vs after)

- **NFR-502**: All custom controls MUST have comprehensive documentation in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`

- **NFR-503**: All code MUST follow Constitution Principle II (nullable reference types with explicit `?` annotations)

- **NFR-504**: All XAML MUST follow Constitution Principle III (`x:DataType` and `{CompiledBinding}` everywhere)

- **NFR-505**: Test coverage MUST achieve 80%+ for critical paths (ViewModels, services, custom control properties)

#### Security (NFR-600 Series)

- **NFR-601**: VISUAL credentials MUST be stored using Windows DPAPI via `WindowsSecretsService` (never in config files)

- **NFR-602**: Sensitive data (passwords, API keys, tokens) MUST be filtered from logs (displayed as `***FILTERED***`)

- **NFR-603**: All VISUAL API calls MUST use HTTPS only (no HTTP fallback)

- **NFR-604**: Audit trail MUST record all user-entered transactions (who, when, what changed) in MAMP MySQL

### Key Entities

#### Custom Control Models

- **CustomControl**: Base class for all custom controls. Key attributes: `Name` (string), `Properties` (List<StyledProperty>), `Events` (List<RoutedEvent>), `Usage` (string documentation), `Examples` (List<string> code samples). Relationships: CustomControl → Application Views (many-to-many).

#### Settings Models

- **SettingDefinition**: Represents a configurable setting. Key attributes: `Key` (string, unique identifier like "Visual:BaseUrl"), `DisplayName` (string), `Description` (string), `DefaultValue` (object), `CurrentValue` (object), `InputType` (enum: TextBox, ComboBox, CheckBox, Slider, NumericUpDown), `Category` (enum: General, Database, VisualERP, Advanced, Developer), `ValidationRules` (List<ValidationRule>), `RequiresRestart` (bool). Relationships: SettingDefinition → SettingCategory (many-to-one).

- **SettingCategory**: Logical grouping of settings. Key attributes: `CategoryName` (string), `DisplayName` (string), `Icon` (PathGeometry), `Order` (int), `Settings` (List<SettingDefinition>). Relationships: SettingCategory → SettingDefinition (one-to-many).

#### VISUAL Integration Models

- **VisualPart**: Represents a part/component from VISUAL ERP. Key attributes: `PartNumber` (string, primary key), `Description` (string), `LongDescription` (string?), `Location` (string?), `UnitOfMeasure` (string), `UnitCost` (decimal), `QuantityOnHand` (decimal), `QuantityAvailable` (decimal), `QuantityAllocated` (decimal), `ReorderPoint` (decimal), `ReorderQuantity` (decimal), `Barcode` (string?), `LastReceived` (DateTime?), `LastIssued` (DateTime?), `IsActive` (bool), `LastUpdated` (DateTime), `CachedAt` (DateTime). Relationships: VisualPart → VisualWorkOrder (many-to-many via materials), VisualPart → InventoryTransaction (one-to-many).

- **VisualWorkOrder**: Represents a manufacturing work order. Key attributes: `WorkOrderNumber` (string, primary key), `PartNumber` (string), `PartDescription` (string?), `QuantityOrdered` (decimal), `QuantityCompleted` (decimal), `QuantityRemaining` (decimal), `Status` (enum: Open, Released, Complete, Closed), `StartDate` (DateTime?), `DueDate` (DateTime?), `CompletionDate` (DateTime?), `Location` (string?), `Notes` (string?), `Operations` (List<WorkOrderOperation>), `LastUpdated` (DateTime), `CachedAt` (DateTime). Relationships: VisualWorkOrder → VisualPart (many-to-many for materials), VisualWorkOrder → WorkOrderOperation (one-to-many).

- **WorkOrderOperation**: Represents an operation step in work order. Key attributes: `SequenceNumber` (int), `OperationCode` (string), `Description` (string), `Status` (enum: Pending, InProgress, Complete), `StandardHours` (decimal), `ActualHours` (decimal), `StartedAt` (DateTime?), `CompletedAt` (DateTime?). Relationships: WorkOrderOperation → VisualWorkOrder (many-to-one).

- **InventoryTransaction**: Represents inventory movement. Key attributes: `TransactionId` (int, primary key), `PartNumber` (string), `TransactionType` (enum: Receipt, Issue, Adjustment, Transfer), `Quantity` (decimal), `FromLocation` (string?), `ToLocation` (string?), `ReferenceNumber` (string?), `Notes` (string?), `UserId` (string), `TransactionDate` (DateTime), `UnitCost` (decimal), `ExtendedCost` (decimal), `SavedToMAMP` (bool), `SyncedToVisual` (bool, always false for read-only integration). Relationships: InventoryTransaction → VisualPart (many-to-one).

- **LocalTransactionRecord**: Represents user-entered transaction stored in MAMP MySQL (system of record). Key attributes: `TransactionId` (Guid, primary key), `TransactionType` (enum: WorkOrderStatusUpdate, InventoryReceipt, InventoryIssue, InventoryAdjustment, InventoryTransfer, ShipmentConfirmation), `EntityData` (string, JSON payload), `CreatedAt` (DateTime), `CreatedBy` (string, UserId), `VisualReferenceData` (string, JSON with part numbers, work order numbers from cached VISUAL data), `AuditMetadata` (string, JSON with before/after values). Relationships: LocalTransactionRecord → User (many-to-one). NOTE: No sync to VISUAL - MAMP MySQL is permanent storage.

#### Diagnostic Models

- **PerformanceSnapshot**: Diagnostic snapshot for Debug Terminal. Key attributes: `Timestamp` (DateTime), `TotalMemoryMB` (double), `PrivateMemoryMB` (double), `Stage0DurationMs` (long), `Stage1DurationMs` (long), `Stage2DurationMs` (long), `ErrorCount` (int), `ErrorSummary` (string), `ConnectionPoolActive` (int), `ConnectionPoolIdle` (int), `VisualApiAvgResponseMs` (double?), `VisualCacheHitRate` (double?). Relationships: None (point-in-time snapshot).

- **ErrorLogEntry**: Error log entry for Debug Terminal. Key attributes: `Timestamp` (DateTime), `Severity` (enum: Critical, Error, Warning, Info), `Message` (string), `StackTrace` (string?), `Category` (enum: Database, Configuration, VisualAPI, Network, General), `IsResolved` (bool). Relationships: None (historical log).

---

## Success Criteria

### Measurable Outcomes

#### Custom Controls

- **SC-101**: Custom controls catalog (`docs/UI-CUSTOM-CONTROLS-CATALOG.md`) contains documentation for 10+ controls with examples, properties, usage patterns, visual previews

- **SC-102**: XAML duplication reduced by 60%+ across codebase (measured by lines of duplicate XAML before vs after extraction)

- **SC-103**: All custom controls pass unit tests for property validation, change notifications, default values (80%+ code coverage)

- **SC-104**: Custom controls render within <16ms per frame (60 FPS) measured via Avalonia profiler

#### Settings Management

- **SC-201**: Settings screen displays all 60+ configuration options organized in 5 tabbed categories loading within 500ms

- **SC-202**: Settings save operations complete within 1 second from Save button click to database persistence confirmation

- **SC-203**: All settings validation rules implemented with inline error display and Save button disabled when validation fails

- **SC-204**: Settings persist to `UserPreferences` table per-user with fallback to local JSON when database unavailable

- **SC-205**: Zero configuration-related support tickets after deployment (compared to baseline 10+ per month with file editing)

#### Debug Terminal

- **SC-301**: Debug Terminal rewritten with feature-based side panel navigation and all repeated XAML replaced with custom controls

- **SC-302**: All pending Feature 003 ViewModel bindings completed: Copy to Clipboard, IsMonitoring toggle, environment variables display

- **SC-303**: Debug Terminal maintains functional equivalence with current implementation (all metrics, error logging, boot timeline preserved)

- **SC-304**: Debug Terminal renders 1000+ error log entries with virtualization maintaining <16ms per frame (60 FPS)

- **SC-305**: Diagnostic export functionality completes in <5 seconds for 100MB+ data with progress indicator and cancel button

#### Configuration Error Handling

- **SC-401**: `ConfigurationErrorDialog` implemented and integrated into MainWindow.axaml error handling paths (Feature 002 TODO complete)

- **SC-402**: All configuration errors display user-friendly messages with recovery options (no raw exceptions in UI)

- **SC-403**: Error recovery flows tested with 95%+ success rate (users successfully recover from configuration errors without IT support)

#### VISUAL Integration

- **SC-501**: Part lookup operations complete within 2 seconds for 99.9% of requests under normal network conditions (measured via performance telemetry over 30-day period)

- **SC-502**: Work order query operations complete within 2 seconds for result sets up to 500 records (99.9% of requests)

- **SC-503**: Inventory transaction recording completes within 5 seconds (99.5% of requests)

- **SC-504**: VISUAL integration achieves 99.9% uptime (successful API calls / total API calls, excluding maintenance, over 30-day rolling window)

- **SC-505**: Offline mode activates within 100ms of connection loss with cached data accessible within 1 second

- **SC-506**: User-reported errors related to VISUAL connectivity or data synchronization decrease by 80% compared to baseline (previous 6 months)

- **SC-507**: Zero user retraining required - existing users complete all workflows without training sessions (validated via user observation study with 10 representative users)

- **SC-508**: Barcode scan → part lookup → display completes within 2 seconds total for 99% of scans

#### Overall Feature

- **SC-601**: All Constitution principles validated: Spec-driven (SPEC/PLAN/TASKS), nullable reference types (`?` everywhere), CompiledBinding (`x:DataType` everywhere), TDD (80%+ coverage), performance budgets met, error categorization complete, secrets in OS storage, graceful degradation, structured logging, DI everywhere, reusable controls documented

- **SC-602**: Build completes with zero errors and zero warnings (`dotnet build MTM_Template_Application.sln`)

- **SC-603**: All tests pass (unit + integration + contract + performance) with 80%+ code coverage on critical paths

- **SC-604**: Validation script passes: `.specify/scripts/powershell/validate-implementation.ps1 -FeatureId "005-migrate-infor-visual"`

- **SC-605**: Constitutional audit passes: `.specify/scripts/powershell/constitutional-audit.ps1 -FeatureId "005-migrate-infor-visual"`

---

## Open Questions

1. **Custom Controls Styling**: Should custom controls support theme overrides or enforce consistent styling? Decision needed before implementation.

2. **Settings Export Format**: Should settings export include sensitive data (database passwords, API keys) encrypted or excluded entirely? Security review required.

3. **Debug Terminal Real-time Updates**: Should Debug Terminal auto-scroll to latest entries or respect user scroll position? UX testing needed.

4. **VISUAL API Connection Pool**: Should connection pool size be configurable or fixed at 5 concurrent connections? Performance testing required.

5. **Offline Queue Priority**: Should offline sync queue prioritize certain transaction types (work orders before inventory) or process FIFO? Business rule clarification needed.

6. **Barcode Scanner Hardware**: Which barcode scanner models should be officially supported? Hardware compatibility testing required.

7. **Cache Compression Level**: Should LZ4 compression use level 1 (fast) or level 9 (maximum compression)? Storage vs performance trade-off analysis needed.

---

## Implementation Phases

### Phase 1: Custom Controls Foundation (Priority 1)

**Duration**: 5-7 days  
**Risk**: Medium (refactoring existing UI)

**Deliverables**:
- Extract 10 custom controls from existing views
- Implement `StyledProperty` pattern for all bindable properties
- Create unit tests for property behavior
- Document in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
- Visual tests in Avalonia previewer

**Dependencies**: None (foundation for all other phases)

### Phase 2: Settings UI (Priority 2)

**Duration**: 4-6 days  
**Risk**: Low (new UI, no refactoring)

**Deliverables**:
- Build `SettingsWindow.axaml` with TabControl and 5 categories
- Create `SettingsViewModel` with `[ObservableProperty]` for 60+ settings
- Implement Save/Cancel transaction pattern
- Persist to `UserPreferences` table
- Validate all inputs with inline error display
- Unit tests for validation rules and persistence

**Dependencies**: Phase 1 (requires `SettingsCategory` and `SettingRow` controls)

### Phase 3: Debug Terminal Rewrite (Priority 3)

**Duration**: 6-8 days  
**Risk**: High (complete rewrite of working code)

**Deliverables**:
- Complete rewrite using SplitView with feature-based navigation
- Replace repeated XAML with custom controls from Phase 1
- Wire up pending Feature 003 ViewModel bindings (Copy to Clipboard, IsMonitoring, environment variables)
- Implement virtualization for error lists (1000+ entries)
- Integration tests for all diagnostic features
- Functional equivalence validation

**Dependencies**: Phase 1 (requires custom controls)

### Phase 4: Configuration Error Dialog (Priority 4)

**Duration**: 2-3 days  
**Risk**: Low (isolated feature)

**Deliverables**:
- Implement `ConfigurationErrorDialog.axaml` with recovery options
- Integrate with `IErrorNotificationService`
- Complete MainWindow.axaml TODO (Feature 002)
- Unit tests for error categorization and recovery flows

**Dependencies**: Phase 1 (requires `ConfigurationErrorDialog` control)

### Phase 5: VISUAL ERP Integration (Priority 5)

**Duration**: 10-14 days  
**Risk**: High (complex API integration, offline support)

**Deliverables**:
- Implement `IVisualApiClient` service with Infor VISUAL API Toolkit
- Create VISUAL UI views (Parts, Work Orders, Inventory) using custom controls
- Implement barcode scanning support
- Build offline sync queue with LZ4 cache compression
- Add connection health monitoring to Debug Terminal
- Implement production metrics dashboard
- Contract tests for VISUAL API compatibility
- Integration tests for offline queue and sync
- Performance tests validating 2-second response time budget

**Dependencies**: Phase 1 (requires custom controls), Phase 3 (requires Debug Terminal for monitoring)

### Phase 6: Integration & Testing (Priority 6)

**Duration**: 3-5 days  
**Risk**: Medium (integration issues possible)

**Deliverables**:
- End-to-end integration testing (all features working together)
- Performance validation (all budgets met)
- User acceptance testing with 10 representative users
- Documentation updates (README, USER-GUIDE, TROUBLESHOOTING)
- Constitutional audit passing all principles
- Validation script passing all checks

**Dependencies**: Phases 1-5 (all implementation complete)

---

## Risk Assessment

### High Risk Items

1. **Debug Terminal Complete Rewrite** - Rewriting working code risks introducing regressions. Mitigation: Comprehensive test suite before refactoring, maintain functional equivalence checklist, parallel implementation with feature flag.

2. **All-In-One Mega-Feature Scope** - 3-5x larger than recommended split approach increases delivery risk. Mitigation: Phased implementation with integration points, extensive testing after each phase, Radio Silence Mode for focused execution.

3. **VISUAL API Integration Complexity** - Offline support, sync queue, connection health monitoring adds significant complexity. Mitigation: Mock service for testing, contract tests for API compatibility, incremental delivery (read-only first, then advanced features).

### Medium Risk Items

4. **Custom Controls Backward Compatibility** - Extracting controls risks breaking existing ViewModel bindings. Mitigation: Unit tests for all bindings, integration tests for views using controls, backward compatibility validation.

5. **Settings Persistence Edge Cases** - Database unavailable, concurrent modifications, validation failures. Mitigation: Fallback to local JSON, per-user settings eliminate conflicts, comprehensive validation test suite.

### Low Risk Items

6. **Configuration Error Dialog** - Isolated feature with clear requirements. Mitigation: Standard patterns already established in Feature 002.

7. **Barcode Scanning** - Well-defined integration pattern. Mitigation: Hardware compatibility testing, timeout handling, audio feedback.

---

## Dependencies

### External Dependencies

- **Infor VISUAL API Toolkit**: Version compatibility must be verified. Assemblies located at `docs\Visual Files\ReferenceFiles\`. Documentation at `docs\Visual Files\Guides\`.

- **MySQL Database**: MAMP MySQL 5.7 on localhost:3306. Schema documented at `.github/mamp-database/schema-tables.json`.

- **Barcode Scanner Hardware**: Compatible models TBD. Timeout configurable via `Visual:BarcodeTimeout` setting.

### Internal Dependencies

- **Feature 002 (Configuration Service)**: `IConfigurationService` interface, `UserPreferences` table schema, credential storage via `ISecretsService`.

- **Feature 003 (Debug Terminal)**: Existing `DebugTerminalViewModel`, `DiagnosticSnapshot`, `ErrorLogEntry` models. Pending TODOs to complete.

- **Constitution Principle XI**: Custom control documentation standards, 3+ occurrence extraction threshold.

### Package Dependencies

- **Avalonia 11.3.6**: SplitView control, CompiledBinding, x:DataType support
- **CommunityToolkit.Mvvm 8.4.0**: `[ObservableProperty]`, `[RelayCommand]` source generators
- **MySql.Data 9.0.0**: Database connectivity for `UserPreferences` and `LocalTransactionRecord` persistence
- **K4os.Compression.LZ4 1.3.8**: Offline cache compression
- **Polly 8.4.2**: VISUAL API retry policies and circuit breakers
- **Serilog 8.0.0**: Structured logging for VISUAL API calls
- **xUnit 2.9.2**: Unit testing framework
- **NSubstitute 5.1.0**: Mocking for services
- **FluentAssertions 6.12.1**: Readable test assertions

---

## Notes

This is an **all-in-one mega-feature** combining five major workstreams. Estimated to be **3-5x larger** than split approach but delivers complete manufacturing solution in single feature branch.

**High Risk, High Reward**: Success requires disciplined execution following Constitution principles, comprehensive testing (80%+ coverage), and Radio Silence Mode for focused implementation.

**Constitution Compliance**: This feature directly addresses multiple Constitution TODOs:
- Custom control catalog (Principle XI)
- Debug Terminal pending bindings (Feature 003)
- MainWindow ConfigurationErrorDialog (Feature 002)

**Validation**: Run `.specify/scripts/powershell/validate-implementation.ps1` and `.specify/scripts/powershell/constitutional-audit.ps1` before PR submission.

---

**Specification Complete** - Ready for `/plan` phase to generate technical implementation plan.
