# Engineering (Process / Product / Tooling) Read‑Only Feature Spec

Company Context: Precision metal stamping, fabrication, assemblies
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for manufacturing engineering, process improvement, and tooling design. No BOM/routing edits.

## Strategic Goals
- Shorten engineering change cycle while improving impact clarity.
- Highlight design/process drift causing downstream cost or quality issues.
- Provide earlier visibility of manufacturability risks on new or revised parts.

## Feature List
1. ECO Impact Projection Engine
   - Forecasts WIP disruption, inventory obsolescence, and labor delta of pending Engineering Change Orders.
2. Manufacturability Risk Score
   - Predicts likelihood a new/revised part will exceed standard hours (features: geometry complexity proxy, tolerance tightness, historical analog variance).
3. Revision Adoption Velocity Tracker
   - Measures how quickly production shifts to latest revision across open WOs and backlog.
4. Tool Design Change Propagation Map
   - Visual graph linking tool design changes to affected parts, operations, routings.
5. Cost Driver Attribution Analyzer
   - Breaks variance between quoted vs current actual cost into labor learning, scrap, tooling change, material yield influences.
6. Simulation vs Actual Conformance Monitor (If Simulation Data)
   - Compares forming/press simulation predicted force vs actual measured press tonnage deviations.
7. High Scrap Geometry Feature Correlator
   - Associates recurring scrap with specific geometric features (holes near edge, tight radii) derived from CAD attribute metadata.
8. Alternate Material Feasibility Advisory
   - Identifies parts possibly transitioning to lower-cost or more available material grades (based on mechanical property mapping) read-only.
9. Tolerance Tightness Optimization Candidates
   - Flags tolerances historically achieved with large process capability margin (over-engineered tolerances cost opportunity).
10. Rework Root Complexity Scorer
    - Scores rework events by complexity of underlying design features to prioritize simplification efforts.
11. Make vs Buy Sensitivity View
    - Highlights parts near threshold where external sourcing cost ratios improved recently (read-only advisory).
12. BOM Volatility Heatmap
    - Parts with frequent ECO revisions vs stable baseline (process stabilization target list).
13. Setup Parameter Drift Insight (If Param Logs)
    - Detects systematic deviation in setup parameters (press speed, lubrication rate) vs standard sheets.
14. Tool Change Learning Curve Tracker
    - Measures improvement in time to implement tool design changes over successive ECOs.
15. Digital Thread Gap Detector
    - Identifies parts lacking linked digital assets (3D model, process sheet, inspection plan revision alignment).
16. Rapid Prototype Cycle Time Analyzer
    - Shows elapsed time from ECO submission to first-off approval for prototype-intense part families.
17. Engineering Capacity Forecast
    - Predict projected ECO workload vs available engineering labor hours (bottleneck alert).
18. Obsolescence Exposure Snapshot
    - Quantifies on-hand and WIP quantity of soon-to-be-replaced revisions to plan run-out.

## Predictive / Scoring Notes
- Manufacturability Risk logistic features: FeatureCountNormalized, AverageToleranceRatio, HistoricalAnalogVariance, NewToolRequiredFlag.
- ECO Impact cost driver weighting: WIP Rework Cost + Inventory Scrap Risk + Labor Delta (modeled) + Tool Modification Effort Index.

## UI Layout (Avalonia)
- Tabs: ECO | Risk | Cost | Tooling | Optimization
- Drill Drawer: Part/Revision -> risk factors, analog job comparisons, capability surplus/deficit.

## Data Inputs
- ECO records (pending, approved, implementation dates)
- BOM & routing revisions
- Labor standards vs actual history
- Scrap & rework by part revision
- Tool design change logs
- Press run parameter logs (if captured)
- CAD/feature metadata (if accessible read-only reference table)

## Macro Launch Parameters
- Optional FocusPartRevision or FocusECO.

## Acceptance Highlights
- No BOM, routing, or ECO status modifications.
- All predictive scores show factor contribution list.
- Impact projection stable for fixed snapshot (no non-deterministic variation).

---
Draft – Subject to stakeholder review.