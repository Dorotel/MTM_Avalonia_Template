# Outstanding Clarification Questions: Localization, Theme, & Navigation - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Localization & Culture**, **Theme & Visual Presentation**, and **Navigation & UI Shell** features.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (10 total)

### Q1: [Functional Scope] - Initial language selection

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-093 set up culture and locale

How should the initial language be determined?

| Option | Description |
|--------|-------------|
| A | System OS language |
| B | User's last saved preference |
| C | Organization default from configuration |
| D | Priority: User preference → System → Organization default |

**Answer: D — Priority: User preference → System → Organization default**

Reasoning: Cascading priority respects user choice while providing sensible defaults. User preference (stored per-user) takes precedence — explicit choice should override all. System OS language provides good initial guess for new users. Organization default (from config) acts as final fallback and allows enterprise customization. If selected language lacks translation, fallback to English (universal fallback). Store user preference in user settings (synced across devices).

---

### Q2: [Edge Cases] - Missing translation fallback strategy

**Impact**: Medium
**Category**: Edge Cases & Failure Handling
**Feature Context**: FR-098 fall back to English for missing translations

Should missing translations be logged?

| Option | Description |
|--------|-------------|
| A | Yes, always log as warning |
| B | Yes, log as info (low priority) |
| C | Yes, but only once per missing key |
| D | No, silent fallback only |

**Answer: C — Yes, but only once per missing key**

Reasoning: Logging missing translations helps identify localization gaps without spam. Once-per-key prevents log flooding (same missing key accessed hundreds of times per session). Log as Warning severity for visibility in production monitoring. Include: translation key, requested language, fallback language used. Aggregate missing keys and emit telemetry summary (count by language) for translation team prioritization. In DEBUG builds, show visual indicator (e.g., [EN] prefix) on fallback text.

---

### Q3: [Functional Scope] - Runtime language switching scope

**Impact**: High
**Category**: Functional Scope & Behavior
**Feature Context**: FR-097 runtime language switching

Does language switching reload all screens?

| Option | Description |
|--------|-------------|
| A | Yes, full application restart (clean slate) |
| B | Reload current screen only |
| C | Update all open screens in-place |
| D | Update lazily (as screens are navigated to) |

**Answer: C — Update all open screens in-place**

Reasoning: In-place updates provide best user experience — immediate visual feedback without losing navigation context or unsaved changes. Implement via reactive property binding: UI elements bind to translation service's current language. On language change, service emits change event, bound elements refresh. ViewModels re-query translations, trigger property change notifications. For static text (compiled AXAML resources), trigger visual tree refresh via `ResourceDictionary` replacement. Android: recreate activity gracefully preserving state. Desktop: update in-place.

---

### Q4: [Non-Functional] - Theme switching animation duration

**Impact**: Low
**Category**: Interaction & UX Flow
**Feature Context**: FR-113 Light/Dark/Auto theme modes

How long should theme transitions take?

| Option | Description |
|--------|-------------|
| A | Instant (no animation) |
| B | 200ms (quick) |
| C | 500ms (smooth) |
| D | 1000ms (slow, dramatic) |

**Answer: B — 200ms (quick)**

Reasoning: 200ms feels responsive while providing smooth visual feedback. Matches platform animation standards (iOS/Android material design transitions). Instant (option A) feels jarring. 500ms+ feels sluggish for user-initiated actions. Use easing function (ease-in-out) for natural acceleration/deceleration. Transition colors and backgrounds, but not layout (layout changes should be instant). Respect OS accessibility settings: honor prefers-reduced-motion by making transitions instant.

---

### Q5: [Functional Scope] - Auto theme switching timing

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-116 auto-switch to dark mode based on system

When should auto theme switching check system preferences?

| Option | Description |
|--------|-------------|
| A | Only at startup |
| B | At startup + when app gains focus |
| C | Continuous monitoring (real-time) |
| D | At startup + hourly checks |

**Answer: C — Continuous monitoring (real-time)**

Reasoning: Real-time monitoring provides seamless user experience when OS switches theme (common at sunrise/sunset with auto dark mode). Windows: subscribe to `SystemEvents.UserPreferenceChanged`. Android: override `onConfigurationChanged`. Avalonia: listen to `Application.Current.ActualThemeVariantChanged`. Minimal performance cost (event-driven, not polling). Only applies when theme mode is "Auto" (user manual override to Light/Dark disables auto-switching). Persist user's mode preference separately from actual theme.

---

### Q6: [Data Model] - Navigation history depth

**Impact**: Low
**Category**: Domain & Data Model
**Feature Context**: FR-105 track navigation history

How many navigation history entries should be kept?

| Option | Description |
|--------|-------------|
| A | 10 entries |
| B | 25 entries |
| C | 50 entries |
| D | Unlimited (entire session) |
| E | Configurable limit |

**Answer: B — 25 entries**

Reasoning: 25 entries cover typical user workflows without excessive memory usage. Enough for complex multi-screen tasks (lookup → select → edit → review → confirm navigation patterns). History stored per-session (cleared on app restart). Store lightweight metadata: screen name, parameters, timestamp (not full view state). Implement as circular buffer (oldest entries evicted first). Serialize on app shutdown for session restoration ("continue where you left off"). Allow "clear history" action in navigation menu.

---

### Q7: [Edge Cases] - Unsaved changes validation behavior

**Impact**: High
**Category**: Edge Cases & Failure Handling
**Feature Context**: FR-107 validate navigation to prevent data loss

How should unsaved changes be handled?

| Option | Description |
|--------|-------------|
| A | Always block navigation with Save/Discard/Cancel dialog |
| B | Auto-save and navigate |
| C | Show warning only for "major" changes (configurable threshold) |
| D | User preference: always prompt vs. always auto-save |

**Answer: A — Always block navigation with Save/Discard/Cancel dialog**

Reasoning: Explicit confirmation prevents accidental data loss (highest priority for business app). Three-button dialog: "Save and Continue" (primary), "Discard Changes" (danger), "Cancel" (secondary, returns to current screen). Track changes via ViewModel dirty flag. Exception: navigation to parent detail screen (breadcrumb up) can auto-save if validation passes. For power users, add "Don't ask again this session" checkbox. Store preference in session memory only (reset on app restart for safety).

---

### Q8: [Functional Scope] - Deep linking parameter validation

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-106 deep linking to specific screens

What happens when deep link parameters are invalid?

| Option | Description |
|--------|-------------|
| A | Reject and show error, stay on current screen |
| B | Navigate to screen but show error banner |
| C | Navigate to screen with default/empty state |
| D | Navigate to home screen with error notification |

**Answer: D — Navigate to home screen with error notification**

Reasoning: Deep links often come from external sources (emails, notifications, web). Invalid parameters indicate stale links or tampering. Staying on current screen (option A) is confusing ("why didn't link work?"). Navigating to broken screen (options B/C) creates poor UX. Home screen navigation with clear error notification explains failure and provides recovery path. Error message: "Could not open [Screen Name]: [Reason]. The link may be outdated." Include "Try Again" button if transient failure, "Go to [Screen]" for manual navigation.

---

### Q9: [Non-Functional] - Navigation analytics data collection

**Impact**: Low
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-110 track navigation analytics

What navigation analytics should be collected?

| Option | Description |
|--------|-------------|
| A | Screen views only (minimal) |
| B | Screen views + time spent per screen |
| C | Option B + navigation paths (flow analysis) |
| D | Comprehensive: all of above + user interactions |
| E | User opt-in required for analytics |

**Answer: C — Screen views + time spent + navigation paths**

Reasoning: Option C provides actionable insights without excessive data collection. Screen views identify popular features. Time spent reveals engagement and potential UI issues (excessive time = confusion). Navigation paths expose user workflows and drop-off points. Skip granular interactions (option D) — that's separate feature analytics. Respect privacy: anonymize user IDs, don't track PII. Aggregate data server-side for dashboards. Local caching with batch upload (conserve network). Allow opt-out in settings but default to enabled (standard telemetry).

---

### Q10: [Functional Scope] - Multi-window support scope (desktop only)

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-109 multiple tabs or windows on desktop

What multi-window scenarios should be supported?

| Option | Description |
|--------|-------------|
| A | Multiple independent windows (separate sessions) |
| B | Tabbed interface within single window |
| C | Both options available to user |
| D | No multi-window support (single window only) |

**Answer: D — No multi-window support (single window only)**

Reasoning: Multi-window support adds significant complexity (shared state management, synchronization, resource contention) with limited benefit for manufacturing workflows. Single window with robust navigation and history is sufficient. Users can achieve multi-context workflows via navigation history and quick switching. Future enhancement: consider tabbed interface (option B) after core features stabilize. Single-instance enforcement: second app launch focuses existing window rather than creating new instance. Allows deep link handling without multi-window complications.

---
