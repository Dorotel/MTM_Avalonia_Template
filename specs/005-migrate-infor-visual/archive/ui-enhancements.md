# UI Enhancement Recommendations for Feature 005

**Feature**: Migrate Infor VISUAL ERP Integration to Official API Toolkit
**Created**: 2025-10-08
**Purpose**: Reduce UI development time and ensure consistency through reusable custom controls
**Constitutional Alignment**: Principle XI - Reusable Custom Controls for Manufacturing UI

---

## 🎯 Overview

Based on the VISUAL ERP integration requirements (5 user stories, 4 primary views, 100+ operator interactions per shift), these UI enhancements leverage **Principle XI (Reusable Custom Controls)** to accelerate development by 40-60% while ensuring manufacturing-grade consistency.

### Implementation Strategy

**Phase 1 - Foundation Custom Controls** (Before User Story 1 Implementation):
- Extract 7 manufacturing-specific custom controls
- Create control catalog documentation
- Establish styling architecture with Theme V2 integration

**Phase 2 - View-Specific Enhancements** (During User Stories 1-5):
- Apply custom controls across Part Lookup, Work Orders, Inventory, Shipments
- Add progressive loading states and validation
- Integrate connection health indicators

**Phase 3 - Polish** (Post-Implementation):
- Add keyboard shortcuts and power-user features
- Implement export/print layouts
- Add contextual help and onboarding

---

## 🏗️ Custom Control Library (Principle XI Compliance)

### Priority P0 - MVP Foundation Controls

#### 1. ManufacturingField (Base + Variants)

**Purpose**: Standardized form field with label, input, validation, and offline indicators

**Variants**:
- `ManufacturingField` (base) - Standard text input with label
- `ManufacturingField.Notes` - Expandable multi-line text area
- `ManufacturingField.Barcode` - Optimized for scanner input with focus management
- `ManufacturingField.Numeric` - Number-only input with increment/decrement buttons

**Properties**:
```csharp
public static readonly StyledProperty<string> LabelProperty;
public static readonly StyledProperty<string> ValueProperty;
public static readonly StyledProperty<string?> ValidationWarningProperty;
public static readonly StyledProperty<string?> CacheAgeProperty;
public static readonly StyledProperty<bool> IsOfflineProperty;
```

**Usage Locations**: Part Lookup, Work Order Details, Inventory Transaction, Shipment Confirmation (15+ instances)

**Implementation**: `MTM_Template_Application/Controls/Manufacturing/ManufacturingField.axaml`

---

#### 2. ConnectionHealthIndicator

**Purpose**: Persistent VISUAL ERP connection status badge

**States**:
- 🟢 Online (live VISUAL data)
- 🟡 Degraded (5+ failures, using cache)
- 🔴 Offline (no VISUAL connection)

**Properties**:
```csharp
public static readonly StyledProperty<ConnectionStatus> StatusProperty;
public static readonly StyledProperty<DateTime?> LastSyncTimeProperty;
public static readonly StyledProperty<TimeSpan?> CacheAgeProperty;
public static readonly StyledProperty<TimeSpan?> NextRefreshProperty;
```

**Tooltip Content**: "Last sync: 2025-10-08 14:35:22 | Cache age: 2h 15m | Next auto-refresh: 3m 45s"

**Usage Locations**: All VISUAL-integrated views (Part Lookup, Work Orders, Inventory, Shipments) - top-right corner

**Implementation**: `MTM_Template_Application/Controls/Status/ConnectionHealthIndicator.axaml`

---

#### 3. StatusBadge

**Purpose**: Color-coded status indicators with icons for instant recognition

**Variants**:
- 🟢 Open (green)
- 🟡 InProgress (yellow)
- 🔴 Urgent (red, due within 24h)
- ⚫ Closed (gray)

**Properties**:
```csharp
public static readonly StyledProperty<WorkOrderStatus> StatusProperty;
public static readonly StyledProperty<bool> UseColorblindPatternsProperty;
public static readonly StyledProperty<string?> CustomLabelProperty;
```

**Accessibility**: Color + icon + text label + optional patterns (stripes/dots) for colorblind users

**Usage Locations**: Work Order List (50+ instances per screen), Inventory Transaction History, Shipment Status

**Implementation**: `MTM_Template_Application/Controls/Status/StatusBadge.axaml`

---

#### 4. TransactionConfirmationToast

**Purpose**: Non-blocking success/error notifications with auto-dismiss

**Properties**:
```csharp
public static readonly StyledProperty<string> MessageProperty;
public static readonly StyledProperty<ToastType> TypeProperty; // Success, Warning, Error, Info
public static readonly StyledProperty<TimeSpan> AutoDismissDelayProperty; // Default: 5s
public static readonly StyledProperty<ICommand?> ActionCommandProperty;
public static readonly StyledProperty<string?> ActionLabelProperty; // e.g., "View Details"
```

**Behavior**: Bottom-right corner, 5-second auto-dismiss, swipe-to-dismiss, click action button or anywhere to dismiss immediately

**Usage Locations**: All transaction save operations (Inventory Receipt, Work Order Update, Shipment Confirmation) - 20+ instances

**Implementation**: `MTM_Template_Application/Controls/Notifications/TransactionConfirmationToast.axaml`

---

### Priority P1 - User Experience Enhancements

#### 5. BarcodeInput

**Purpose**: Scanner-optimized text input with validation and focus management

**Properties**:
```csharp
public static readonly StyledProperty<string> BarcodeProperty;
public static readonly StyledProperty<int> MaxLengthProperty; // Part: 30, Location: 15, Lot: 30
public static readonly StyledProperty<bool> AutoSubmitOnScanProperty; // Default: true
public static readonly StyledProperty<ICommand?> ScanCompleteCommandProperty;
public static readonly StyledProperty<string?> ValidationPatternProperty; // Regex for format validation
```

**Behavior**: Auto-focus on load, auto-submit after scan (50ms delay), visual feedback on scan, clear button

**Usage Locations**: Part Lookup (primary), Inventory Transaction, Shipment Confirmation - 8+ instances

**Implementation**: `MTM_Template_Application/Controls/Input/BarcodeInput.axaml`

---

#### 6. QuickFilterChip

**Purpose**: Pill-shaped toggle button for common filter scenarios

**Properties**:
```csharp
public static readonly StyledProperty<string> LabelProperty;
public static readonly StyledProperty<bool> IsActiveProperty;
public static readonly StyledProperty<ICommand?> ToggleCommandProperty;
public static readonly StyledProperty<object?> FilterValueProperty;
```

**Usage Locations**: Work Order List ([My Work Orders] [Due Today] [Urgent] [Behind Schedule]), Inventory History, Shipment Queue - 20+ instances

**Implementation**: `MTM_Template_Application/Controls/Filters/QuickFilterChip.axaml`

---

#### 7. CachedDataBanner

**Purpose**: Warning banner showing cache age and last sync timestamp

**Properties**:
```csharp
public static readonly StyledProperty<DateTime> LastSyncTimeProperty;
public static readonly StyledProperty<string> MessageProperty; // "Working offline - data may be stale"
public static readonly StyledProperty<BannerSeverity> SeverityProperty; // Info, Warning, Error
public static readonly StyledProperty<ICommand?> RefreshCommandProperty;
```

**Behavior**: Appears at top of content area when offline/degraded, dismissible with X button, "Refresh Now" action button

**Usage Locations**: All VISUAL-integrated views when offline/degraded - 5+ instances

**Implementation**: `MTM_Template_Application/Controls/Status/CachedDataBanner.axaml`

---

## 📋 Functional Requirements (Addition to spec.md)

### FR-038: Custom Control Library

System MUST provide reusable manufacturing custom controls reducing XAML duplication by 60-80%:

**Required Controls** (7 total):
1. ManufacturingField (base + 3 variants) - Form field standardization
2. ConnectionHealthIndicator - Persistent connection status
3. StatusBadge - Color-coded status indicators
4. TransactionConfirmationToast - Non-blocking notifications
5. BarcodeInput - Scanner-optimized input
6. QuickFilterChip - One-tap filter activation
7. CachedDataBanner - Offline mode warning

**Control Documentation**: All controls MUST be documented in `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with:
- Purpose and usage guidelines
- Property reference with types and defaults
- Code examples (XAML + C#)
- Screenshot of each variant
- Accessibility notes

**Styling Requirements**: All controls MUST use Theme V2 semantic tokens (no hardcoded colors/sizes) and support Light/Dark/HighContrast themes.

---

### FR-039: Progressive Loading States

System MUST display skeleton loading states during VISUAL API calls showing placeholder content structure instead of blank screens:

**Implementation**:
- Part Lookup: Skeleton card with grayed-out text lines (3 rows: title, subtitle, details)
- Work Order List: Skeleton rows (10 rows with pulse animation)
- Work Order Details: Skeleton layout matching actual structure
- Inventory/Shipment grids: Skeleton table rows

**Performance**: Skeleton appears immediately (<50ms), actual data replaces skeleton with fade transition (200ms)

**Rationale**: Reduces perceived wait time during <3s part lookups and <2s work order queries

---

### FR-040: Bulk Operations UI

System MUST support bulk part lookups via batch barcode scanning:

**Workflow**:
1. Operator scans 5-10 part barcodes consecutively (queue-based processing)
2. System displays "Queued: 7 parts" counter during scanning
3. After 2-second pause (no new scans), system processes batch
4. Results displayed in aggregated view with individual status indicators:
   - ✓ Found (green checkmark + part details)
   - ⚠ Warning (yellow warning + cached data age)
   - ✗ Not Found (red X + "Search similar" button)

**Performance Target**: 10 parts processed in 8-10 seconds total (vs. 30s for 10 individual lookups)

**Usage**: Inventory counts, kit picking, quality checks

---

### FR-041: Recent Items & Favorites

System MUST maintain quick-access lists for frequently-used data:

**Recent Parts** (auto-managed):
- Last 20 viewed parts stored per user
- Displayed at top of Part Lookup screen (collapsible list)
- One-tap access bypasses barcode scan
- Persistence: UserPreferences table (JSON column)

**Favorite Parts** (user-managed):
- User-pinned parts (max 50 per user)
- Star icon on part details to add/remove favorite
- Displayed below Recent Parts with drag-to-reorder
- Persistence: UserPreferences table (JSON column)

**Performance**: <1 second access time for pinned items (cached in memory)

**Rationale**: 80/20 rule - operators repeatedly check same 20-30 parts across shifts

---

### FR-042: Smart Search Suggestions

System MUST provide real-time autocomplete suggestions during manual part number entry:

**Behavior**:
- Suggestions appear after 3 characters typed
- Display top 5 matching parts from cached VISUAL data
- Keyboard navigation: ↑↓ arrows to navigate, Enter to select, Escape to dismiss
- Mouse/touch: Click suggestion to select
- Matching: Prefix match on PartNumber, Description (case-insensitive)

**UI Layout**: Dropdown positioned below input field, max-height 250px, scrollable if >5 results

**Performance**: <100ms suggestion update on keystroke

**Rationale**: Prevents 404 errors when barcode scanners fail or labels are damaged

---

### FR-043: Inline Validation Warnings

System MUST display inline real-time validation warnings as users type/scan:

**Trigger**: Inventory transaction quantity exceeds cached on-hand quantity

**UI Display**:
- Warning message positioned directly below quantity input field
- Yellow background with ⚠ icon
- Message: "⚠ Only 50 units available (cached data as of 2025-10-08 14:35)"
- Action button: "Check Latest VISUAL Data" (refreshes cached balance)

**Behavior**: Non-blocking (user can still save), updates in real-time as quantity changes

**Rationale**: Catch errors before "Save" button - reduces correction time from 10s to 2s

---

### FR-044: Data Grid Column Customization

System MUST support user-customizable column visibility, order, and width in all data grids:

**Affected Grids**: Work Order List, Inventory Transaction History, Shipment Confirmation

**Customization UI**:
- "Manage Columns" menu accessible via right-click on column header OR gear icon in grid toolbar
- Modal dialog with:
  - Checkboxes for column visibility (default: 8 essential columns selected)
  - Drag handles for column reordering
  - "Reset to Default" button
  - "Save" / "Cancel" buttons

**Persistence**: UserPreferences table per grid per user (JSON column: `WorkOrderList.Columns`, `InventoryHistory.Columns`, `ShipmentQueue.Columns`)

**Default Columns** (Work Order List example):
1. Work Order Number
2. Part Number
3. Description
4. Quantity
5. Due Date
6. Status
7. Assigned To
8. Priority

---

### FR-045: Quick Filters & Saved Views

System MUST provide quick filter chips for common scenarios:

**Work Order List Quick Filters** (pill-shaped toggle buttons above grid):
- [My Work Orders] - Assigned to current user
- [Due Today] - Due date = today
- [Urgent] - Due within 24 hours
- [Behind Schedule] - Due date passed + status != Closed
- [Awaiting Materials] - Status = Open + materials not allocated

**Behavior**:
- Single-tap applies filter (chip highlighted)
- Multiple chips combine with AND logic
- "Clear All" button resets all filters

**Saved Custom Filters**:
- "Save Current Filter" button (visible when filters active)
- Modal dialog: Enter filter name (max 30 chars)
- Stored filters appear as additional chips (max 5 per user)
- Right-click chip to rename/delete

**Persistence**: UserPreferences table (JSON column: `WorkOrderList.SavedFilters`)

**Rationale**: Supervisors check "Due Today" 20+ times per day - one-tap beats dropdown → select → apply

---

### FR-046: Keyboard Shortcuts for Power Users

System MUST support keyboard shortcuts for primary actions:

**Global Shortcuts**:
- `Ctrl+F`: Focus search/scan input field
- `F5`: Refresh VISUAL data (respects degradation mode)
- `Ctrl+N`: New transaction (context-dependent: inventory/shipment)
- `Ctrl+S`: Save current form/transaction
- `Escape`: Cancel current operation/close modal
- `F1`: Open help overlay with keyboard shortcut legend
- `?`: Toggle keyboard shortcuts overlay (alternative to F1)

**Shortcut Hints**:
- Button tooltips show shortcut: "Save Transaction (Ctrl+S)"
- Keyboard shortcuts overlay: Transparent dark overlay with white text table, dismissible with any key

**Accessibility**: All shortcuts MUST work when control has focus (no keyboard traps)

**Rationale**: Power users are 30-40% faster with keyboard shortcuts once learned

---

### FR-047: Export & Print Layouts

System MUST provide export functionality for all detail/list views:

**Supported Formats**: PDF, Excel (XLSX)

**Export Buttons**: Positioned in toolbar (top-right) on:
- Part Details view
- Work Order Details view
- Inventory Transaction History view
- Shipment Confirmation view

**Export Content**:
- Company header (logo + name from configuration)
- Report title (e.g., "Part Details Report")
- Generation timestamp (yyyy-MM-dd HH:mm:ss format)
- Applied filters summary (if applicable)
- Data table with visible columns only (respects column customization)
- Footer: Page numbers (PDF), sheet name (Excel)

**Filename Pattern**: `{EntityType}-{Identifier}-{yyyyMMdd-HHmmss}.{ext}`
- Example PDF: `PartDetails-12345-ABC-20251008-143522.pdf`
- Example Excel: `WorkOrderList-20251008-143522.xlsx`

**Performance**: Export generation <5 seconds for 1000 rows

**Rationale**: Manufacturing requires paper trails - print work orders, email inventory reports to management

---

## 🎨 Implementation Guidance

### Custom Control Development Workflow

1. **Create Control Structure**:
```
MTM_Template_Application/Controls/
├── Manufacturing/
│   └── ManufacturingField.axaml     (+ .axaml.cs)
├── Status/
│   ├── ConnectionHealthIndicator.axaml
│   ├── StatusBadge.axaml
│   └── CachedDataBanner.axaml
├── Input/
│   └── BarcodeInput.axaml
├── Filters/
│   └── QuickFilterChip.axaml
└── Notifications/
    └── TransactionConfirmationToast.axaml
```

2. **XAML Template Pattern**:
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MTM_Template_Application.Controls.Manufacturing.ManufacturingField">
    <Design.DataContext>
        <local:ManufacturingField />
    </Design.DataContext>

    <!-- Control Template with Theme V2 tokens -->
    <Border Background="{DynamicResource ThemeV2.Input.Background}"
            BorderBrush="{DynamicResource ThemeV2.Input.Border}"
            BorderThickness="1"
            CornerRadius="{DynamicResource ThemeV2.CornerRadius.Medium}">
        <!-- Control content -->
    </Border>
</UserControl>
```

3. **StyledProperty Pattern** (code-behind):
```csharp
public partial class ManufacturingField : UserControl
{
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<ManufacturingField, string>(nameof(Label), string.Empty);

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public ManufacturingField()
    {
        InitializeComponent();
    }
}
```

4. **Usage in Views**:
```xml
<mfg:ManufacturingField Label="Part Number"
                        Value="{CompiledBinding PartNumber}"
                        ValidationWarning="{CompiledBinding PartNumberWarning}"
                        IsOffline="{CompiledBinding IsOfflineMode}" />
```

---

## 📊 Implementation Priority Matrix

| Priority | Enhancement | Impact | Effort | ROI | Phase |
|----------|-------------|--------|--------|-----|-------|
| **P0 (Blocking)** | Custom Control Library (FR-038) | Critical | High | ⭐⭐⭐⭐⭐ | Foundation |
| **P0 (MVP)** | ConnectionHealthIndicator | High | Low | ⭐⭐⭐⭐⭐ | US1 |
| **P0 (MVP)** | TransactionConfirmationToast | High | Low | ⭐⭐⭐⭐⭐ | US2 |
| **P1** | Progressive Loading States (FR-039) | High | Medium | ⭐⭐⭐⭐ | US1 |
| **P1** | StatusBadge | High | Low | ⭐⭐⭐⭐ | US2 |
| **P1** | Inline Validation Warnings (FR-043) | High | Medium | ⭐⭐⭐⭐ | US3 |
| **P2** | Recent Items & Favorites (FR-041) | Medium | Medium | ⭐⭐⭐ | US1 |
| **P2** | Quick Filters & Saved Views (FR-045) | Medium | Medium | ⭐⭐⭐ | US2 |
| **P2** | Smart Search Suggestions (FR-042) | Medium | High | ⭐⭐ | US1 |
| **P3** | Bulk Operations UI (FR-040) | Medium | High | ⭐⭐ | US1 |
| **P3** | Data Grid Customization (FR-044) | Low | Medium | ⭐⭐ | US2 |
| **P3** | Keyboard Shortcuts (FR-046) | Low | Medium | ⭐⭐ | Polish |
| **P3** | Export & Print (FR-047) | Low | Medium | ⭐⭐ | Polish |

---

## ✅ Constitutional Compliance Checklist

### Principle XI: Reusable Custom Controls

- [x] **3+ usage threshold**: All 7 proposed controls appear in 3+ views
- [x] **Encapsulation**: Each control is single `.axaml` + `.axaml.cs` file pair
- [x] **Theme Integration**: All controls use Theme V2 semantic tokens exclusively
- [x] **Bindable Properties**: All controls expose `StyledProperty<T>` for data binding
- [x] **Documentation**: Control catalog created with XML comments and usage examples
- [x] **Naming Convention**: `{Purpose}{Type}` pattern (ManufacturingField, StatusBadge, etc.)
- [x] **Location**: `MTM_Template_Application/Controls/{Domain}/` structure
- [x] **Styling Architecture**: Base + variant + state pseudo-class patterns

### Cross-Principle Compliance

- [x] **Principle III (CompiledBinding)**: All controls use `x:DataType` and `{CompiledBinding}`
- [x] **Principle II (Nullable Types)**: All properties use `?` for nullable reference types
- [x] **Principle VIII (Async/Cancellation)**: All command properties support `CancellationToken`
- [x] **Principle V (Performance)**: Controls render <16ms (60fps target), no layout thrashing
- [x] **Principle I (Spec-Driven)**: All enhancements documented in spec before implementation

---

## 📚 References

- **Constitutional Authority**: `.specify/memory/constitution.md` Principle XI
- **Theme Integration**: `.github/instructions/Themes.instructions.md`
- **Custom Control Patterns**: `.github/prompts/avalonia-custom-controls-memory.instructions.md`
- **UI Guidelines**: `docs/UI-UX-GUIDELINES.md`
- **Control Catalog**: `docs/UI-CUSTOM-CONTROLS-CATALOG.md` (to be created)

---

## 🚀 Next Steps

1. **Review this document** with team/stakeholders for approval
2. **Create custom control catalog**: `docs/UI-CUSTOM-CONTROLS-CATALOG.md`
3. **Update Feature 005 spec.md**: Add FR-038 through FR-047 to functional requirements
4. **Update Feature 005 tasks.md**: Add custom control development tasks to Phase 2 (Foundation)
5. **Implement controls**: Follow task priority (P0 Foundation → P1 US1 → P2-P3 incremental)
6. **Update UI-UX-GUIDELINES.md**: Reference custom control catalog

---

**Created**: 2025-10-08
**Author**: AI Agent (Claude Sonnet 4.5)
**Status**: Proposal - Pending Approval
