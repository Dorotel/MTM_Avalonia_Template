# Reference: Clarification Answers
**Date**: October 8, 2025
**Purpose**: Record all 21 clarification answers for Feature 005 restart

---

## Section 1: Settings Screen Scope

### Q1: What settings should be included?
**Answer**: All configurable settings
- Include every setting from IConfigurationService
- API endpoints, timeouts, database connections
- UI themes, logging levels, cache settings
- Folder paths, feature flags

### Q2: How should settings be organized?
**Answer**: Tabbed categories
- Multiple tabs: "General", "Database", "VISUAL ERP", "Advanced", "Developer"
- Familiar pattern for users
- Reduces cognitive load

### Q3: Settings changes effect?
**Answer**: Save button required
- Changes staged until "Save" is clicked
- "Cancel" button to discard changes
- Prevents accidental changes
- Clear transaction boundaries

---

## Section 2: Custom Controls Strategy

### Q4: Extraction aggressiveness?
**Answer**: Aggressive (3+ occurrences)
- Extract any pattern appearing 3 or more times
- Matches Constitution Principle XI default
- DebugTerminalWindow has many repeated patterns

### Q5: Naming convention?
**Answer**: PurposeType format
- Examples: StatusCard, MetricDisplay, ErrorListPanel
- No unnecessary prefixes
- Matches Constitution examples

### Q6: Custom controls catalog?
**Answer**: Yes, comprehensive catalog
- Create docs/UI-CUSTOM-CONTROLS-CATALOG.md
- Include examples, properties, usage
- Constitution TODO requirement

---

## Section 3: Debug Terminal Refactoring

### Q7: Side panel navigation organization?
**Answer**: Feature-based navigation
- "Feature 001: Boot"
- "Feature 002: Config"
- "Feature 003: Diagnostics"
- "Feature 005: VISUAL"

### Q8: UI framework for side panel?
**Answer**: Avalonia SplitView
- Collapsible side panel
- Hamburger menu (like mobile apps)
- Modern, familiar UX

### Q9: Keep or rewrite content?
**Answer**: Rewrite from scratch
- Use as opportunity to redesign cleanly
- Extract custom controls during rewrite
- **HIGH RISK but cleaner result**

---

## Section 4: TODOs from Previous Features

### Q10: Feature 003 ViewModel bindings?
**Answer**: Yes, complete all
- Copy to Clipboard functionality
- IsMonitoring toggle
- Environment variables display
- Wire up all pending bindings

### Q11: Constitution TODO - custom control catalog?
**Answer**: Part of Feature 005
- Create catalog as deliverable
- Document as controls are created
- Don't defer

### Q12: ConfigurationErrorDialog TODO?
**Answer**: Yes, implement now
- Complete as part of Feature 005
- Show error details with recovery suggestions
- Integrate with IErrorNotificationService

---

## Section 5: Feature 005 Scope Definition

### Q13: Split features or all-in-one?
**Answer**: All-in-one mega-feature
- Settings UI + Custom Controls + Debug Terminal + VISUAL Integration
- **VERY AMBITIOUS - largest possible scope**
- High risk, high reward approach

### Q14: Task priority order?
**Answer**: Custom Controls → Settings UI → Debug Terminal
- Build reusable components first
- Use components in Settings UI
- Refactor Debug Terminal with components
- VISUAL integration last

### Q15: Settings persistence?
**Answer**: Database (per-user)
- Store in UserPreferences table
- Existing pattern from Feature 002
- Supports multi-user scenarios

---

## Section 6: Testing & Validation

### Q16: Testing approach?
**Answer**: Test-first (TDD)
- Constitution Principle IV requirement
- Write tests before implementation
- Prevents regressions

### Q17: Custom control tests?
**Answer**: Yes, test properties
- Test StyledProperty defaults
- Test validation
- Test change notifications

### Q18: Test coverage target?
**Answer**: Good (80%+)
- Constitution target
- Cover critical paths
- ViewModels and service methods

---

## Section 7: Implementation Approach

### Q19: Use Radio Silence Mode?
**Answer**: Yes, use Radio Silence
- AI works through entire feature
- Minimal interaction
- Produces concrete deliverables

### Q20: Deliverable format?
**Answer**: Patch files (diffs)
- Unified diffs for each file change
- Radio Silence standard format
- Easy to apply with git

---

## Section 8: Final Confirmation

### Q21: Agree with scope?
**Answer**: Need modifications / ALL IN 1 SUPER SPEC
- Proceeding with largest possible scope
- All features in one mega-feature
- Confirmed ready to proceed

---

## Risk Assessment

### HIGH RISK Items:
1. **Debug Terminal rewrite** - Complete recreation of working code
2. **All-in-one scope** - 3-5x larger than recommended split approach
3. **VISUAL integration** - Complex API integration with offline support

### Mitigation:
- Test-first development reduces regression risk
- Custom controls provide consistency
- Radio Silence Mode ensures focused execution
- Comprehensive testing (80%+ coverage)

---

## Success Indicators:
- All custom controls documented and reusable
- Settings UI provides complete configuration access
- Debug Terminal has intuitive feature-based navigation
- VISUAL integration works offline with sync
- 80%+ test coverage achieved
- Constitutional compliance verified
