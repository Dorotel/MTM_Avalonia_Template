# Die Shop / Tool Crib Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for tooling engineers, die maintenance planners, and crib attendants. Focus on tool life, maintenance prediction, location accountability, and readiness. No writes.

## Strategic Goals
- Extend tooling life via early intervention signals.
- Reduce setup delays from missing or unready tooling.
- Increase accountability for tool location & condition.

## Feature List
1. Tool Life Consumption Radar
   - Visual ring or bar showing % life consumed vs expected curve (usage normalized by material hardness & stroke count). Baseline Visual may show simple last maintenance metrics only.
2. Predictive Regrind / Refurb Forecast
   - Forecast next required regrind date using wear rate trend (EWMA) rather than fixed cycle.
3. Die Location Accountability Map
   - Current custody/location trace with last scan/posting and anomaly flag if idle in staging > threshold.
4. Missing / Overdue Return Alert Board
   - Lists tools checked out to setup/production beyond allowed time window.
5. Tool Readiness Scorecard
   - Composite readiness (Cleaned, Inspected, Components Complete, Documentation Current) with factor rationale.
6. Component Depletion Watchlist
   - Tracks inserts/punches/spares approaching reorder point based on consumption velocity.
7. Multi-Die Conflict Predictor
   - Identifies future schedule windows where multiple jobs require same die family cluster simultaneously.
8. Dimensional Drift Analyzer
   - Monitors inspection measurement variance drift toward tolerance boundary (trend slope warning).
9. Corrective vs Preventive Work Ratio Trend
   - Shows shift toward corrective maintenance indicating under-planned PM.
10. Tool Utilization Balance Heatmap
    - Highlights under-used vs over-used dies (load leveling opportunity) vs simple usage counts.
11. Tooling Cost of Delay Estimator (Read-only)
    - Calculates potential downtime cost if a predicted failure is not serviced before window.
12. Setup Delay Attribution (Tooling)
    - Aggregates historical setup delays rooted in tooling issues (not material or labor) to prioritize improvements.
13. Rapid Turn Candidate List
    - Tools currently in maintenance that could be expedited (short remaining tasks + high upcoming job impact).
14. Spare Capacity Projection
    - Forecast availability of critical die set types vs scheduled demand horizon.
15. Tool Check-Out Pattern Anomaly Detection
    - Flags unusual after-hours or prolonged check-outs relative to use history.
16. Calibration Compliance Tracker (If Gauges Included)
    - Lists gauges/dies with upcoming calibration/expires window and risk score if overdue.
17. Environmental Exposure Risk (If Data)
    - Indicates tools stored outside controlled environment beyond safe dwell time (corrosion risk).
18. Tool Failure Root Cause Cluster
    - Clusters recent failures by cause (heat checking, chipping, galling) from inspection notes NLP extraction.

## Predictive / Scoring Notes
- Wear Rate = Smoothed(Usage Increment / Time) adjusted by material hardness factor.
- Regrind Forecast Date = Today + (RemainingLife% / AdjustedWearRate).
- Readiness Score weighting example: (InspectionClear 0.3 + ComponentsComplete 0.25 + Cleaned 0.2 + DocsCurrent 0.15 + LocationVerified 0.1).

## UI Layout (Avalonia)
- Tabs: Life | Readiness | Scheduling | Maintenance | Risk
- Drill Drawer: Selected Tool -> life curve, inspection timeline, upcoming schedule consumers, factor attribution.

## Data Inputs
- Tool/Dies master (family, life baseline, maintenance intervals)
- Usage postings (strokes, cycles, run hours)
- Maintenance work orders / history
- Inspection measurements & notes
- Tool checkout / return logs (crib transactions)
- Job schedule (upcoming WOs requiring tooling)
- Spare component inventory levels
- Calibration records (if separate table)

## Macro Launch Parameters
- Optional FocusToolId or FocusDieFamily.

## Acceptance Highlights
- No updates to maintenance counters or tool status.
- All predictive forecasts list data points used (last N usage samples, slope, variance).
- Performance: Tool list (<=5k tools) loads with life % calculations in < 5s cold.

## Open Questions
- Are stroke counts always posted per operation or aggregated daily?
- Is there structured cause coding for failures or only free-text notes (affects NLP reliability)?

---
Draft – Subject to stakeholder review.