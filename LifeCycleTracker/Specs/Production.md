# Production (Shop Floor Operations) Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for supervisors & operators: WIP flow, bottleneck prediction, quality drift, labor effectiveness. No writes.

## Strategic Goals
- Increase schedule adherence & flow.
- Surface emerging bottlenecks & quality issues early.
- Provide explainable performance insights at operator & cell level.

## Feature List
1. WIP Flow Velocity Dashboard
   - Real-time takt vs actual flow deviation by routing family; Visual shows discrete queues without velocity narrative.
2. Bottleneck Emergence Predictor
   - Forecasts which workcenter group will become constraint in next 4/8 hours using queue aging + arrival rate.
3. Operation Start Delay Analyzer
   - Clusters reasons (material wait, prior op overrun, labor unassigned) for starts delayed > threshold.
4. Scrap Pattern Drift Detector
   - Identifies statistically significant increase in scrap rate adjusted for mix & learning curve.
5. Rework Gravity Map
   - Heatmap of operations generating highest rework hours relative to throughput.
6. Labor Effectiveness Pulse
   - Rolling earned vs actual hour ratio with stability band; out-of-band alerts.
7. Downtime Early Warning Panel
   - Predictive probability of unscheduled downtime next shift based on micro-stoppage frequency & maintenance drift.
8. Cross-Job Setup Overrun Radar
   - Highlights active setups trending beyond expected 75th percentile based on similar jobs.
9. WIP Aging Waterfall
   - Cascade view of WIP value by aging buckets (0-4h, 4-8h, 8-24h, 1-3d, 3d+) with movement deltas.
10. Shortage Propagation Impact Lens
    - Shows at-risk downstream operations if current shortage persists X hours (read-only scenario forecast).
11. Learning Curve Conformance Tracker
    - Compares actual hour per unit decline vs expected learning model for new/repeat builds.
12. Operator Skill Diversification View
    - Identifies over-concentration risk where few operators own majority of critical tasks.
13. On-Time Operation Completion Forecast
    - Probability each in-process WO operation completes by planned time; aggregated cell reliability metric.
14. OEE Input Advisor (Pseudo-OEE)
    - Read-only decomposition (Availability proxy, Performance vs standard, Quality yield) without writing OEE events.
15. Micro-Stoppage Pattern Analyzer
    - Clusters short interruptions to reveal systemic causes (tool adjustment, sensor fault) before major downtime.
16. Shift Change Readiness Snapshot
    - Summarizes open setups, critical shortages, pending QC releases buffering next shift performance.
17. Priority Discrepancy Detector
    - Flags WOs being processed out of recommended priority sequence impacting higher risk jobs.
18. Production Risk Composite Index
    - Weighted index (bottleneck probability + shortage severity + scrap drift + labor gap) trending line.

## Predictive / Scoring Notes
- Bottleneck Probability uses: Queue Length Z-score, Arrival Rate Trend, Effective Capacity Derate, Setup Overrun Probability.
- Scrap Drift: CUSUM or EWMA control method triggers when sustained shift beyond baseline.

## UI Layout (Avalonia)
- Tabs: Flow | Bottlenecks | Quality | Labor | Risk
- Real-time panels with color-coded thresholds; drill drawer for selected workcenter/operation.

## Data Inputs
- Work orders & routing operations (status, planned start/finish)
- Labor postings / time tickets
- Scrap & rework transactions
- Machine / workcenter definitions & capacities
- Shortage lists
- Maintenance work orders / micro-stoppage logs (if available read-only)

## Macro Launch Parameters
- Optional FocusWorkcenter or FocusWO.

## Acceptance Highlights
- No labor, operation status, or maintenance updates performed.
- Predictive outputs list top contributing factors.
- Bottleneck forecast refresh < 90s cycle; incremental updates < 1.5s.

---
Draft – Subject to stakeholder review.