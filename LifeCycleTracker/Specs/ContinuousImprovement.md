# Continuous Improvement (Lean / OpEx) Read‑Only Feature Spec

Company Context: High-mix precision stamping & fabrication seeking throughput, quality, and cost optimization.
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for CI leaders & lean facilitators. No KPI edits or event creation.

## Strategic Goals
- Surface waste and variability patterns automatically.
- Prioritize Kaizen opportunities by quantified impact.
- Track sustainability of implemented improvements.

## Feature List
1. Waste Signature Detector
   - Aggregates motion, wait, transport, overproduction proxies from operational timestamps (automated Muda scoring).
2. Value Stream Flow Imbalance Index
   - Identifies process steps with disproportionate cycle time vs takt causing starvation or blocking.
3. Kaizen Opportunity Ranker
   - Ranks candidate improvement areas blending impact potential (cost/time saved) vs complexity (historical similar improvements).
4. Post-Kaizen Benefit Sustenance Monitor
   - Tracks regression of KPIs post-implementation; flags benefit erosion.
5. SMED Changeover Reduction Candidates
   - Detects operations with high internal vs external setup composition potential (needs externalization advisory).
6. OEE Component Variability Analyzer
   - Variance decomposition (Availability vs Performance vs Quality) to focus improvement energy.
7. Hidden Factory Estimator
   - Estimates unrealized capacity from micro-stoppages & minor speed losses.
8. Lead Time Compression Simulator (Read-only)
   - Models effect on end-to-end lead time if top N bottleneck variances reduced.
9. Standard Work Adherence Drift
   - Detects elongation in repeatable task sequences vs documented standard.
10. 80/20 Improvement Leverage Matrix
    - Maps processes accounting for majority of cumulative delay vs complexity to implement changes.
11. Rework Loop Recurrence Tracker
    - Identifies circular rework patterns indicating unclear defect definitions.
12. First Pass Yield Improvement Attribution
    - Attributes FPY gains to contributing actions (tool change, parameter adjustment) using change-point correlation.
13. Continuous Flow Interruption Log (Aggregated)
    - Consolidates interruptions (shortages, quality holds, maintenance stops) with frequency ranking.
14. Energy per Good Unit Benchmark (If Data)
    - Tracks energy intensity improvements post initiatives.
15. Kanban Signal Volatility Analyzer
    - Measures irregularity in pull signal cadence suggesting sizing misalignment.
16. Operator Multi-Skilling Coverage Score
    - Quantifies cross-training depth vs target matrix for resilience.
17. Improvement Portfolio Balance View
    - Distribution of active initiatives across themes (quality, flow, cost) ensuring balanced pipeline.
18. ROI Realization Confidence Score
    - Assesses probability projected savings will be realized based on similar past initiative variance.

## Predictive / Scoring Notes
- ROI Confidence features: InitiativeType, HistoricalVariance, SponsorTrackRecord, ComplexityScore, Early KPI Trajectory.
- Waste Signature Score uses weighted composite of queue wait ratio, changeover percent, scrap %, rework hours ratio.

## UI Layout (Avalonia)
- Tabs: Opportunities | Waste | Sustainability | Simulation | Portfolio
- Drill Drawer: Initiative or Process -> baseline vs current charts, factor contributions.

## Data Inputs
- Operation timestamps (start/finish, setup, run)
- Scrap/rework logs
- Changeover records
- Improvement initiative registry (read-only reference)
- Labor cross-training matrix
- Production output & uptime logs

## Macro Launch Parameters
- Optional FocusProcess or FocusInitiative.

## Acceptance Highlights
- No updates to initiative registry or standard work docs.
- All simulated benefits clearly marked hypothetical.
- Opportunity ranking deterministic per snapshot.

---
Draft – Subject to stakeholder review.