# Outstanding Clarification Questions: Splash Screen & User Communication - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Splash Screen & User Communication** feature.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (8 total)

### Q1: [User Experience] - Progress percentage calculation method

**Impact**: High
**Category**: Functional Scope & Behavior
**Feature Context**: FR-002 progress indicators

When showing progress percentage (0-100%), how should it be calculated?

| Option | Description |
|--------|-------------|
| A | Equal weight per stage (Stage 0: 0-33%, Stage 1: 34-66%, Stage 2: 67-100%) |
| B | Weight by number of operations per stage (more operations = longer percentage range) |
| C | Weight by measured average duration of each stage from telemetry |
| D | Hybrid: weight major stages, equal weight for operations within each stage |

**Answer: C — Weight by measured average duration from telemetry**

Reasoning: Most accurate representation of actual progress. Telemetry-driven weighting reflects real-world timing variations. Start with equal weights (A) until telemetry data accumulates, then adjust dynamically.

---

### Q2: [Non-Functional] - Time estimate display threshold

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-002 time estimates for long operations

What is the minimum operation duration before showing time estimates?

Format: Short answer (<=5 words)

**Answer: 5 seconds or longer**

Reasoning: Estimates for shorter operations introduce UI noise and are often inaccurate. 5 seconds gives users meaningful context without over-communicating.

---

### Q3: [User Experience] - Logo animation duration

**Impact**: Low
**Category**: Interaction & UX Flow
**Feature Context**: FR-003 branding logo with subtle animation

How long should the logo animation cycle last?

| Option | Description |
|--------|-------------|
| A | 1 second (fast, energetic) |
| B | 2 seconds (balanced) |
| C | 3 seconds (slow, calm) |
| D | Continuous pulse (no cycle) |
| E | Animation matches longest stage duration |

**Answer: D — Continuous pulse (no cycle)**

Reasoning: Continuous subtle animation indicates active processing without creating repetitive visual fatigue. Pulsing conveys "working" better than fixed cycles and adapts naturally to variable boot times.

---

### Q4: [Accessibility] - Screen reader announcement frequency

**Impact**: High
**Category**: Interaction & UX Flow
**Feature Context**: FR-006 screen reader progress updates

How frequently should progress updates be announced to screen readers?

| Option | Description |
|--------|-------------|
| A | Every status message change |
| B | Every 10% progress increment |
| C | Every 25% progress increment |
| D | Only at stage transitions (0 → 1 → 2) |
| E | Stage transitions + major milestones (e.g., "Configuration loaded", "Services initialized") |

**Answer: E — Stage transitions + major milestones**

Reasoning: Balances accessibility with avoiding announcement overload. Stage transitions provide structural checkpoints; major milestones give meaningful progress updates without excessive verbosity.

---

### Q5: [Error Handling] - Retry behavior specifics

**Impact**: High
**Category**: Edge Cases & Failure Handling
**Feature Context**: FR-008 Retry/Exit options

When user clicks Retry after an error, what exactly gets retried?

| Option | Description |
|--------|-------------|
| A | Only the failed operation |
| B | The entire current stage from beginning |
| C | Full restart from Stage 0 |
| D | Configurable based on error type (transient = operation only, config = full restart) |

**Answer: D — Configurable based on error type**

Reasoning: Error-type-aware retry prevents unnecessary work while ensuring clean state for configuration/dependency issues. Transient errors (network) retry operation only; structural errors (config) require full restart.

---

### Q6: [User Experience] - Cancellation confirmation

**Impact**: Medium
**Category**: Interaction & UX Flow
**Feature Context**: FR-009 cancel startup operations

Does cancelling startup require user confirmation?

| Option | Description |
|--------|-------------|
| A | Yes, always show confirmation dialog |
| B | No, immediate cancellation |
| C | Only if >50% progress completed |
| D | Only if critical data operations in progress |

**Answer: A — Yes, always show confirmation dialog**

Reasoning: Prevents accidental exits during startup, which frustrates users who must restart. Brief confirmation is low-cost insurance against misclicks, especially important during long initialization.

---

### Q7: [User Experience] - Cached-only banner placement

**Impact**: Medium
**Category**: Interaction & UX Flow
**Feature Context**: FR-010 persistent warning banner

Where should the cached-only mode warning banner appear?

| Option | Description |
|--------|-------------|
| A | Top of window (above all content) |
| B | Bottom of window (above status bar) |
| C | Floating overlay (can be dismissed temporarily) |
| D | Integrated into navigation bar |

**Answer: A — Top of window (above all content)**

Reasoning: High visibility for critical operational mode indicator. Top placement is conventional for system-level warnings. Always visible position ensures users know they're working with cached data.

---

### Q8: [Data Model] - Splash screen state persistence

**Impact**: Low
**Category**: Domain & Data Model
**Feature Context**: Overall splash screen behavior

Should splash screen remember its last state if the app crashes during boot?

| Option | Description |
|--------|-------------|
| A | Yes, show resume option on next launch |
| B | No, always restart from Stage 0 |
| C | Only for Stage 2 crashes (earlier stages restart fully) |

**Answer: B — No, always restart from Stage 0**

Reasoning: Clean boot guarantees consistent state after crashes. Resume adds complexity and may perpetuate issues. Boot sequence should be fast enough (<10s) that full restart is acceptable.

---
