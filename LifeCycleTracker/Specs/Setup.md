# Setup (Job / Die Preparation) Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Augmented visibility & predictive readiness for setup technicians; no writes.

## Context
Setup personnel need forward-looking insight into tooling, material readiness, discrepancies, and changeover efficiency beyond standard dispatch & traveler reports.

## Feature List
1. Upcoming Changeover Timeline
   - Chronological board of next 24–72h WOs requiring setup with readiness status badges (tooling/material/doc completeness). Visual lacks consolidated readiness scoring.
2. Die Utilization Heat Grid
   - Calendar visualization of die assignments vs capacity windows highlighting idle gaps & over-booking.
3. Tooling Readiness Scorecard
   - Composite readiness score (drawings current? last maintenance within interval? all inserts located?) with drill factors.
4. Pre-Setup Exception Aggregator
   - Unified list of blockers (missing material, QC hold, prior job overrun risk) fed by predictive overrun model.
5. Setup Duration Variance Analyzer
   - Compares actual historical setup times vs standard for same part/die family; flags chronic variance clusters.
6. Predictive Overrun Risk Model
   - Probability of current running job exceeding planned end time -> impacts start times of upcoming setups.
7. Tool Life Remaining Projection
   - Forecast cycles remaining before tool pull based on wear rate trend instead of raw last-maint metrics.
8. Missing Document Detector
   - Identifies WOs lacking latest revision of setup sheet, safety checklist, or inspection plan.
9. Alternate Tool Suggestion (Advisory)
   - Shows compatible approved tool alternatives currently free to mitigate a scheduling conflict (visibility only).
10. Staging Completeness Panel
    - Percentage of required components staged at point-of-use (derived from pick confirmations & location on-hand). Visual requires manual cross-check.
11. Die Change Impact Tree
    - Graph of downstream WOs affected if a die maintenance pull is advanced or delayed.
12. Setup Sequence Optimizer (Static)
    - Suggests reordering upcoming setups to minimize cumulative changeover time (based on similarity matrix). Advisory only.
13. Skill Match Indicator
    - Highlights setups where assigned technician rarely performs this family (training / risk flag).
14. Setup Scrap Early Warning
    - Pattern detection: higher-than-normal early scrap trend for first N pieces on similar recent jobs -> pre-emptive inspection reminder.
15. Compressed Prep Window Alerts
    - Flags when prep window (prior job actual completion → next job planned start) is below historical average open time.
16. Multi-Die Conflict Radar
    - Visual map when multiple queued jobs require same die family cluster; shows best resolution order.
17. Tool Maintenance Drift Tracker
    - Rolling graph of actual maintenance interval deviation vs planned for critical tools.

## Scoring / Models
- Readiness Score = (MaterialReady *0.35)+(ToolLocated*0.15)+(DocsCurrent*0.2)+(NoBlockers*0.2)+(SkillCoverage*0.1)
- Overrun Probability: Logistic regression style using features: last 3 run overruns %, current run scrap rate early phase, operator experience tier, machine utilization.

## UI Structure
- Left Nav: Timeline | Readiness | Predictive | Variance | Advisory
- Central Gantt-esque timeline with color-coded status bars.
- Drill Drawer: Selected WO -> tooling list, docs status, predicted duration, risk flags.

## Data Inputs
- Work orders (routing, standards, scheduled start/end)
- Tooling master & maintenance history
- Labor history (setup actuals, technicians)
- Inventory allocations / staging confirmations
- Document control metadata (revision, effective dates)
- Scrap transaction early-phase markers

## Macro Launch Parameters
- Optionally seeded with FocusWorkOrder to center timeline.

## Acceptance Excerpts
- Every predictive output exposes underlying factors.
- No modification of tooling maintenance counters.
- Timeline refresh incremental (delta hydration) under 2s.

---
Draft – Subject to stakeholder review.