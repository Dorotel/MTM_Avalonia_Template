# Scheduling (Finite Capacity & Sequencing) Read‑Only Feature Spec

Company Context: Multi-press metal stamping & fabrication with tooling changeovers
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for master scheduler & dispatch coordinators. No rescheduling writes.

## Strategic Goals
- Improve constraint awareness and proactive sequence adjustments (advisory only).
- Increase schedule adherence by forecasting slippage sources early.
- Provide what-if visualizations without committing changes.

## Feature List
1. Constraint Emergence Forecast
   - Predicts which workcenter will become bottleneck over next 8/16/24 hours using WIP arrival & capacity derate trends.
2. Sequence Adherence Deviation Monitor
   - Tracks actual vs planned execution order, highlighting divergence impact risk.
3. Setup Family Changeover Optimization Advisory
   - Suggests alternative sequence with reduced cumulative changeover (read-only simulation output).
4. Cross-Resource Load Imbalance Heatmap
   - Highlights underloaded vs overloaded parallel resources enabling manual balancing.
5. Promised Date Risk Horizon
   - Timeline showing projected risk bands for late completions grouped by customer priority.
6. Overtime Avoidance Simulator (Read-only)
   - Models overtime hours avoided if specific advisory sequence applied.
7. Shortage Impact Propagation Timeline
   - Visual timeline of downstream ops blocked if top shortages persist X hours.
8. Frozen Zone Stability Score
   - Measures frequency of changes inside agreed scheduling freeze window (process discipline indicator).
9. Priority Inversion Detector
   - Identifies lower-priority jobs executing ahead of higher criticality work with projected impact.
10. Slack Consumption Tracker
    - Monitors buffer/slack erosion for critical path jobs.
11. Earliest Feasible Completion Estimator (What-if)
    - Calculates potential earliest completion for selected job if expedited (without altering system schedule).
12. Capacity Saturation Projection
    - Future load vs effective capacity view factoring historical derate and planned maintenance.
13. Sequencing Sensitivity Analyzer
    - Shows effect size of swapping adjacent jobs on makespan & late risk.
14. Changeover Volatility Index
    - Measures variance in daily cumulative changeover time vs prior rolling average.
15. Schedule Quality Score
    - Composite metric (Adherence, Priority Alignment, Changeover Efficiency, Constraint Stability) with factor breakdown.
16. Cross-Department Conflict Radar
    - Flags schedule segments where tooling readiness or maintenance windows create latent conflict.
17. WIP Aging Surge Alert
    - Detects surge in aging WIP at pre-bottleneck queues predicting downstream delays.
18. Dispatch List Freshness Gauge
    - Measures how long operators are running tasks beyond their latest recommended dispatch update timestamp.

## Predictive / Scoring Notes
- Constraint Probability features: QueueLengthZ, ArrivalRateSlope, EffectiveCapacity, SetupOverrunRisk.
- Schedule Quality Score weighting example: Adherence 0.3, Priority Alignment 0.25, Changeover Efficiency 0.25, Constraint Stability 0.2.

## UI Layout (Avalonia)
- Tabs: Horizon | Risk | Optimization | Capacity | Metrics
- Gantt-style overlay with advisory layer toggles (no writes).

## Data Inputs
- Work orders & routing schedule (planned vs actual start/end)
- Workcenter capacities & calendars
- Setup/changeover standards
- Shortages & tool readiness signals
- Maintenance window schedule
- WIP queue snapshots

## Macro Launch Parameters
- Optional FocusWorkcenterGroup or FocusJob.

## Acceptance Highlights
- No schedule commits or dispatch alterations.
- Advisory sequences clearly flagged “Simulation”.
- Forecast refresh incremental < 90s; risk timeline render < 2s.

---
Draft – Subject to stakeholder review.