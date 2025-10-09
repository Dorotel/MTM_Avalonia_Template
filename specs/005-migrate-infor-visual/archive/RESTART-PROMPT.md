# Feature 005 Restart Prompt

**Purpose**: Complete restart of Feature 005 with expanded scope based on clarification answers
**Date**: October 8, 2025
**Mode**: All-in-one mega-feature

---

## Feature Description for /speckit.specify

```
Create a comprehensive manufacturing application modernization feature that includes:

1. CUSTOM CONTROLS LIBRARY: Extract reusable UI components from existing views (3+ occurrence threshold). Components include StatusCard, MetricDisplay, ErrorListPanel, ConnectionHealthBadge, PerformanceChart, TimelineVisualization, QuickActionButton, SettingRow, and TabCategory. Create complete documentation catalog with usage examples and property definitions.

2. SETTINGS MANAGEMENT SYSTEM: Build comprehensive settings screen with tabbed categories (General, Database, VISUAL ERP, Advanced, Developer). Include all IConfigurationService settings: API endpoints, timeouts, database connections, UI themes, logging levels, cache settings, folder paths, and feature flags. Changes require Save/Cancel buttons. Persist to UserPreferences table per user.

3. DEBUG TERMINAL MODERNIZATION: Complete rewrite of Debug Terminal with SplitView side panel navigation organized by features (001: Boot, 002: Config, 003: Diagnostics, 005: VISUAL). Replace all repeated XAML with custom controls. Complete pending ViewModel bindings (Copy to Clipboard, IsMonitoring toggle, environment variables).

4. CONFIGURATION ERROR HANDLING: Implement ConfigurationErrorDialog for graceful error display with recovery suggestions when configuration issues occur.

5. INFOR VISUAL ERP INTEGRATION: Integrate with Infor VISUAL ERP system including:
   - Part lookup and inventory management
   - Work order tracking and status updates
   - Manufacturing floor data synchronization
   - Real-time connection health monitoring
   - Offline-first operation with sync queue
   - Barcode scanning integration for shop floor
   - Production metrics dashboard

Target users: Manufacturing floor workers, production managers, and system administrators. Must work offline with degraded functionality and sync when connection restored. Performance targets: Settings screen loads in <500ms, VISUAL API calls return in <2s, offline mode activates within 100ms of connection loss.

All components must follow Constitution Principle XI (reusable custom controls), use MVVM Toolkit patterns, CompiledBinding, nullable reference types, and achieve 80%+ test coverage following test-first development.
```

---

## Implementation Strategy

Based on clarification answers:

### Phase 1: Custom Controls Foundation (Priority 1)
- Extract 10-15 custom controls using 3+ occurrence threshold
- Create `MTM_Template_Application/Controls/` directory structure
- Implement StyledProperty pattern for all bindable properties
- Write unit tests for property behavior
- Document in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`

### Phase 2: Settings UI (Priority 2)
- Build SettingsWindow.axaml with TabControl
- Create SettingsViewModel with [ObservableProperty]
- Organize settings into 5 tabs using custom TabCategory control
- Implement Save/Cancel transaction pattern
- Persist to UserPreferences table (Feature 002 pattern)
- Validate all inputs before save

### Phase 3: Debug Terminal Rewrite (Priority 3)
- Complete rewrite using SplitView for navigation
- Feature-based menu structure
- Replace repeated XAML with custom controls from Phase 1
- Wire up all pending ViewModel bindings
- Implement collapsible side panel with hamburger menu

### Phase 4: Configuration Error Dialog (Priority 4)
- Implement ConfigurationErrorDialog.axaml
- Add recovery suggestions for common errors
- Integrate with IErrorNotificationService
- Show in MainWindow.axaml error paths

### Phase 5: VISUAL ERP Integration (Priority 5)
- Implement VISUAL API client service
- Create manufacturing UI views (Parts, Work Orders, Inventory)
- Add barcode scanning support
- Build offline sync queue
- Add connection health monitoring
- Implement production metrics dashboard

### Testing Strategy
- Test-first (TDD) for all ViewModels and Services
- Unit tests for custom control properties
- Integration tests for VISUAL API
- Contract tests for API compatibility
- Target 80%+ coverage on critical paths

### Radio Silence Execution
- Work through entire feature with minimal interaction
- Deliver patch files (unified diffs) for all changes
- Include test results and coverage reports
- Provide COMMIT messages for each logical change

---

## Reference Files

The following reference files have been created to support specification development:

1. **REFERENCE-CLARIFICATIONS.md** - All 21 clarification answers
2. **REFERENCE-EXISTING-PATTERNS.md** - Current codebase patterns to follow
3. **REFERENCE-CUSTOM-CONTROLS.md** - Identified controls to extract
4. **REFERENCE-SETTINGS-INVENTORY.md** - Complete settings catalog
5. **REFERENCE-CONSTITUTIONAL-REQUIREMENTS.md** - Constitution compliance checklist
6. **REFERENCE-VISUAL-API-SCOPE.md** - VISUAL ERP integration requirements

---

## Expected Deliverables

1. Complete Feature 005 specification (spec.md)
2. Technical implementation plan (plan.md)
3. Granular task breakdown (tasks.md)
4. Custom controls catalog (docs/UI-CUSTOM-CONTROLS-CATALOG.md)
5. All XAML files with custom controls
6. Settings UI with complete functionality
7. Rewritten Debug Terminal
8. ConfigurationErrorDialog implementation
9. VISUAL ERP integration with offline support
10. Comprehensive test suite (80%+ coverage)
11. Unified diff patches for all changes
12. Constitutional compliance audit

---

## Success Criteria

- Users can access all configuration settings through UI without editing files
- Settings changes persist per user and survive application restart
- Debug Terminal provides clear feature-based navigation
- Custom controls reduce XAML duplication by 60%+
- VISUAL ERP data displays within 2 seconds of request
- Application works offline with graceful degradation
- All tests pass with 80%+ code coverage
- Zero nullable reference warnings
- Zero CompiledBinding errors
- Constitutional audit passes all checks

---

## Notes

This is an all-in-one mega-feature combining:
- Settings UI (new)
- Custom Controls extraction (new)
- Debug Terminal rewrite (refactor)
- Error handling (enhancement)
- VISUAL integration (major new feature)

High risk, high reward. Estimated to be 3-5x larger than split approach but delivers complete manufacturing solution in single feature.
