# Quality (QA / Compliance) Read‑Only Feature Spec

Company Context: Precision metal stamping, fabrication, assemblies (Manitowoc Tool & Manufacturing)
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation (no NCR creation, no disposition edits). Emphasis on dimensional stability, process capability, supplier quality, corrective action effectiveness.

## Strategic Goals
- Detect quality drift before nonconformance thresholds breach.
- Increase visibility of systemic variation sources across dies, presses, cells.
- Accelerate root cause isolation for recurring defects.

## Feature List
1. Dimensional Drift Early Warning
   - EWMA / CUSUM monitoring of key dimensions trending toward spec edge; baseline Visual relies on static inspection result review.
2. Cp/Cpk Capability Heatmap
   - Aggregated capability indices by part family / die / press cell; color-coded improvement priority.
3. First Article Approval Risk Predictor
   - Score indicating probability a pending first article will fail based on similarity to historical problem launches.
4. Defect Pareto Auto-Clustering
   - NLP + code grouping to cluster text-based defect descriptions into actionable buckets beyond raw code counts.
5. Tool Wear Impact Correlator
   - Correlates dimensional variation upticks with tool wear usage curves (integration with Die Shop life metrics).
6. Multi-Operation Defect Lineage Trace
   - Trace recurring defect appearance across sequential operations to isolate originating step.
7. Supplier Quality DPM Trend with Change-Point Detection
   - Identifies statistically significant shift in supplier DPM earlier than manual trending.
8. Rework Recurrence Index
   - Measures rework event clustering on same part revision; flags insufficient corrective action effectiveness.
9. Containment Effectiveness Score
   - Evaluates if implemented containment actually reduced defect escape rate (pre vs post window comparison).
10. Inspection Load Forecast
    - Predicts next 7-day inspection labor hours required based on incoming WOs, first article queue, supplier receipt mix.
11. Gage Calibration Risk Heat List
    - Ranks gages nearing calibration date weighted by usage criticality and outstanding open inspections.
12. SPC Exception Consolidator
    - Single panel showing all active control chart violations (Western rules) across monitored processes.
13. Scrap Cost Attribution Analyzer
    - Allocates scrap cost by cause dimension (tooling, material lot, operator learning, setup) with confidence factors.
14. Escape Risk Probability for Open NCRs
    - Probability unresolved NCR will cause external customer issue given current WIP exposure.
15. Measurement System Variation Watch (If Data)
    - Flags when measurement repeatability variance increases (potential gage deterioration).
16. Corrective Action Aging & SLA Tracker
    - Aging buckets of open CARs vs target closure with stagnation flags.
17. Process Capability Improvement Simulator (Read-only)
    - Models potential Cpk shift if specific variance contributors reduced (no parameter write).
18. Multi-Cell Defect Synchronization Detector
    - Identifies same defect type emerging simultaneously across multiple presses (systemic cause suspicion).

## Predictive / Scoring Notes
- Drift Detection: CUSUM with threshold k=0.5σ, h tuned for false alarm rate.
- First Article Failure Probability features: part complexity index, historical similar revision failure rate, new tool indicator, operator experience tier.
- Containment Effectiveness = (Pre Escape Rate - Post Escape Rate) / Pre Rate (with minimum sample size guardrail).

## UI Layout (Avalonia)
- Tabs: Capability | Defects | Predictive | Supplier | Actions
- Drill Drawer: Part/Tool/Supplier context with dimension trend sparkline & factor contributions.

## Data Inputs
- Inspection results (dimensions, sample timestamps, spec limits)
- NCR / CAR records (status, cause, text description)
- Rework transactions
- Scrap logs (cost, cause, operation)
- Supplier receipt inspection outcomes
- Tool usage / wear metrics (from Die Shop spec)
- Calibration schedule & gage master

## Macro Launch Parameters
- Optional FocusPart, FocusSupplier, or FocusNCR.

## Acceptance Highlights
- No NCR/CAR status modifications.
- Each predictive panel provides top 3 contributing factors with numeric influence sign.
- Capability heatmap loads under 5s for <= 2k feature dimensions.

---
Draft – Subject to stakeholder review.