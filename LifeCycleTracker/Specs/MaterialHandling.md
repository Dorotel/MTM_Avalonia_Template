# Material Handling Read‑Only Feature Spec (Avalonia App)

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation via Visual API Toolkit (no writes). Focus on actionable visibility and predictive assistance not natively available or materially enhancing existing baseline screens.

## Design Principles
- Zero database writes; leverage API Toolkit queries, system views, and cached snapshots.
- Latency-aware: sub-second for common lookups (< 5k rows), progressive load for heavy analytics.
- Non-invasive: launched via Visual Macro (Macro Name TBD) passing user/session context.
- Explainable: each predictive score accompanied by rationale factors.
- Drill-friendly: high-level tiles → tabular detail → record context pivot.

## Data Sources (Read Only)
- Inventory master (item, description, UOM, cost method)
- Location / bin definitions
- On-hand by location & lot/serial
- Open work orders (released vs firm vs planned)
- Open purchase orders & expected receipts
- Sales order allocations & shortages
- Transfer histories / inventory movement transactions
- Cycle count history & variances

## Feature List
1. Dynamic Shortage Heatmap
   - Grid of items with color-coded shortage severity (SO/WO demand vs supply). Improves baseline shortage lists with multi-source aggregation & severity scoring.
2. Cross-Order Material Impact Explorer
   - Select an at-risk part to see cascading jobs & customer orders impacted (graph visualization). Visual only exposes this piecemeal.
3. Real-Time Replenishment Opportunity Panel
   - Flags parts whose kanban/min-max thresholds will breach within next N hours based on consumption velocity (predictive, not native pre-breach view).
4. Pick Path Optimization (Static Advisory)
   - Suggests fastest pick sequence for a selected wave using historical travel times (advisory overlay, not execution). Visual lacks travel-time optimization.
5. Multi-Location Consolidation Candidates
   - Identifies SKUs scattered across many small bins suggesting consolidation to reclaim space (space efficiency score).
6. Dead Stock Aging Radar
   - Radar chart & table highlighting items with no movements in N days adjusted for seasonality (improved over basic last-activity date lists).
7. Anomalous Movement Detector
   - Statistical z-score or IQR outlier detection on issue/receipt quantities vs historical norms (alerts for investigation).
8. Lot/Serial Trace Snapshot
   - Instant drill listing all downstream WOs/SOs from a selected lot without launching multiple native trace windows.
9. Imminent Expiry Watchlist
   - Aggregates lots expiring within rolling windows (7/14/30 days) with weighted criticality (allocated demand vs free). Enhances standard expiry lists by priority scoring.
10. Substitute Material Awareness
    - For a shortage item, show approved substitutes with on-hand & projected availability; Visual requires manual cross-reference.
11. Cycle Count Risk Prioritizer
    - Ranks items for next count based on value * volatility * historical variance, vs basic ABC or calendar schedule.
12. Shrinkage Trend Analyzer
    - Rolling variance ratio charts (book vs actual) by item class and location; identifies creeping loss patterns earlier.
13. Put-Away Congestion Monitor
    - Detects inbound receipts lacking assigned final bins plus current bin saturation percent (prevents dock buildup).
14. Frozen vs Movable Inventory Map
    - Visual segregation of inventory under QC hold, quarantine, or inspection vs free available (overlay vs manual filter).
15. Transfer Lead Time Deviation Board
    - Tracks actual vs expected inter-location transfer durations; flags systemic delays impacting promise dates.
16. High Velocity Restock Preview
    - Predict next restock time for top 50 velocity items given trailing usage + inbound ETAs.
17. Safety Stock Stress Test Sandbox (Read-only Modeling)
    - Simulates what-if reduction or increase in safety stock and displays projected shortage events (no parameter writes).
18. Staging Lane Capacity Forecaster
    - Estimates staging lane occupancy next 8 hours combining scheduled picks + inbound receipts.

## Predictive / Scoring Method Notes
- Shortage Severity Score = (Uncovered Demand Qty * Customer Priority Weight) / Time-To-Cover Hours.
- Velocity Forecast: Exponential smoothing (alpha 0.35 default) over last 30 days filtered for abnormal spikes (IQR filter).
- Anomaly Threshold: |z| > 2.5 or outside 1.5 * IQR band after demand smoothing.

## UI Structure (Avalonia)
- Left Nav: Tiles (Shortages, Movements, Optimization, Risk, Aging, Simulation)
- Main Panels: Adaptive layout (2 columns >1400px, single below)
- Detail Drawer: Slide-in right panel for selected item / lot context (activity timeline, allocations, substitutes)
- Export: Print to PDF / CSV of current filtered view (client-side only)

## Macro Launch Integration
- Visual Macro passes: UserId, Company/Entity, OrgUnit, Optional FocusPart.
- App boots with Splash → Context validation → Loads baseline caches (on-hand summary, open demand) asynchronously.

## Security & Compliance
- Enforce read-only by using dedicated low-privilege API user or enforced Toolkit role excluding UPDATE/INSERT procs.
- PII avoidance: Mask customer names beyond first 8 chars for non-Management roles.

## Performance Targets
- Initial cache hydrate (< 8k items, < 25k movement rows) under 6 seconds cold; warm navigation instantaneous (<200ms view switch).

## Open Questions
- Are containerization / bin cluster definitions available for path optimization dataset?
- Access to historical travel time or need synthetic weighting approximations?

## Acceptance Criteria (Excerpt)
- All queries execute with explicit NOLOCK hints where safe (or snapshot isolation) to minimize blocking (documented rationale).
- No operation attempts to invoke stored procedures performing writes.
- Each predictive feature exposes method returning rationale factors list.

---
Draft – Subject to stakeholder review.