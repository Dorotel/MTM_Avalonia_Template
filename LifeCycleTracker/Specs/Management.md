# Management (Executive / Plant Leadership) Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Strategic visibility & forward risk signals beyond standard Visual dashboards; strictly read-only.

## Value Objectives
- Anticipate service, margin, and capacity risks earlier.
- Provide explainable predictive KPIs bridging departmental silos.
- Compress decision latency with consolidated anomaly surfacing.

## Feature List
1. Unified Risk Command Center
   - Single panel of ranked cross-domain risks (late shipment risk, margin erosion, supply disruption). Visual separates domain metrics.
2. Predictive On-Time Shipment Index
   - 7/14/30 day forecast of on-time performance using WIP velocity + shortage probability + capacity saturation.
3. Margin Erosion Watchlist
   - Detects open orders where projected material+labor variance may compress gross margin below threshold (based on recent similar runs).
4. Capacity Saturation Horizon
   - Predicts when any critical workcenter group exceeds 85% effective capacity in next 21 days factoring planned maintenance & historical derating.
5. Strategic Inventory Imbalance Radar
   - Highlights item classes with surplus vs shortage tension (overstock ties up capital while related substitutes are constrained).
6. Customer Service Risk Matrix
   - 2D matrix (Revenue Contribution vs Late Risk Probability) to triage mitigation focus.
7. Cash Conversion Cycle Decomposer
   - Breaks CCC variance vs target into DIO, DSO, DPO deltas with causal factor hints (inventory spikes, slow AR segments).
8. Quote-to-Order Velocity Analyzer
   - Trend of cycle time distribution; anomaly spikes traced to product families or sales territories.
9. Forecast Realization Gauge
   - Compares actual order intake vs forecast with attribution (top gainers/laggards).
10. Supplier Disruption Early Signal
    - Flags suppliers with elongating actual receipt lead time trend + rising shortage dependencies.
11. Labor Productivity Pulse
    - Rolling composite metric using earned vs actual hours, rework ratio, and learning curve drift.
12. Engineering Change Impact Glass
    - Shows cumulative open ECO-driven rework exposure (WOs touched & value at risk).
13. Working Capital Stress Simulator (Read-only What-if)
    - Simulate inventory reduction targets or payment term shifts and project CCC impact.
14. Sustainability KPI Overlay (If Data Available)
    - Energy usage per standard hour vs target trend (if metering data accessible read-only).
15. Deferred Maintenance Risk Index
    - Aggregates overdue PM tasks weighted by machine criticality & production dependency web.
16. Late Root Cause Attribution Engine
    - Clusters late orders by primary cause dimension (shortage, capacity delay, QC hold, external supplier) beyond static late list.
17. Demand Volatility Shock Detector
    - Alerts when incoming order pattern deviates beyond volatility band triggering re-plan attention.
18. Executive Daily Digest Export
    - One-click PDF snapshot of top 10 risk shifts vs prior day with delta explanations.

## Predictive / Scoring Concepts
- Late Risk Probability: Ensemble (logistic + gradient trend weighting) on shortage depth, WIP queue aging, routing complexity.
- Margin Risk: (Projected Actual Cost - Quoted Cost) / Quoted Revenue using analog job cluster averages.
- Capacity Effective Utilization: Scheduled Load * Derate Factor (historical average runtime / standard).

## UI Structure
- Dashboard Tabs: Risk | Financial | Capacity | Demand | Suppliers | Productivity
- Alert Badges with color intensity scaled by risk probability * business impact weight.
- Drill Drawer reveals rationale factors & historical trace.

## Data Inputs
- Sales orders, promise dates, quoted costs
- WIP operations progress & labor postings
- Inventory & shortages
- Purchase orders & receipt history
- Workcenter calendars & maintenance backlog
- ECO / Engineering change references
- Supplier lead time performance history

## Governance & Security
- Role-based redaction: Hide unit cost details for non-finance managers (show index only).
- Every predictive metric lists top 3 contributing factors.

## Performance Targets
- Dashboard initial assembly under 7s (cold); risk tile refresh incremental < 1.5s every auto-cycle (configurable).

## Acceptance Snippets
- No direct drill triggers write forms in Visual.
- Export excludes suppressed redacted fields.
- Risk rankings stable (rerun within same data snapshot yields identical order).

---
Draft – Subject to stakeholder review.