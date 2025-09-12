# Purchasing Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for procurement analysts & buyers; predictive risk, supplier performance, material assurance. No writes.

## Strategic Goals
- Reduce material-driven production disruptions.
- Improve supplier performance transparency.
- Anticipate cost & lead time volatility.

## Feature List
1. Supplier Lead Time Drift Monitor
   - Detects statistically significant elongation vs rolling baseline; Visual shows static current lead time only.
2. PO On-Time Probability Scoring
   - Predictive score for each open PO line using supplier trend, part volatility, and historical expedite frequency.
3. Supply Chain Disruption Radar
   - Aggregated risk heatmap (high impact parts with single-source + rising lead time variance).
4. Multi-Sourcing Opportunity Finder
   - Identifies single-source critical parts with at least 2 analogous alternates in item master (read-only advisory).
5. Expedite Impact Simulator (Read-only)
   - Shows projected schedule recovery if a candidate PO is expedited X days (no action taken, just modeling).
6. Cost Delta Early Warning
   - Flags inbound POs where pending receipt cost deviates > threshold from trailing 90-day average (before variance posts).
7. Supplier Quality Drift Index
   - Rolling defects per million (DPM) trend vs baseline with change-point detection; Visual may show discrete NCR counts only.
8. Inbound Shortage Bridge Panel
   - Lists shortages solvable by already placed inbound POs (partial allocations) vs requiring new sourcing.
9. Risk-Weighted Open PO Dashboard
   - Prioritized list: (Line Value * Late Probability * Criticality Score).
10. Payment Term Advantage Analyzer
    - Highlights suppliers where DPO compression risk is rising vs agreed terms (cash flow early warning).
11. Currency Exposure Snapshot (If Multi-Currency Enabled)
    - Aggregated foreign currency PO value vs hedge coverage (read-only view).
12. Economic Order Interval Deviation
    - Finds items with irregular reorder spacing causing administrative overhead (variance vs ideal EOQ timing).
13. Supplier Consolidation Candidate Matrix
    - Overlapping low-spend, similar commodity suppliers = consolidation potential.
14. MOQ Utilization Efficiency
    - Shows percent of MOQ wasted (ordered - actual short-term need) to target reorder alignment improvements.
15. Pending Approval Aging Tracker
    - Aging distribution for POs stuck in approval to unblock earlier (Visual may show list but not distribution analytics).
16. Inbound Allocation Conflict Detector
    - Warns when multiple shortages are contending for same inbound quantity and projects allocation fairness outcome.
17. Lead Time Forecast Confidence Score
    - Confidence band width metric for each part's predicted lead time (narrower = stable supply).
18. Supplier Tier Migration Signals
    - Indicates when a Tier 2 supplier is approaching performance of Tier 1 (candidate for strategic uplift) or vice versa.

## Predictive / Scoring Notes
- Late Probability Model Features: historical on-time %, recent lead time drift, part criticality, expedite history ratio, supplier quality index.
- Criticality Score = (Demand At Risk Hours / Safety Stock Hours Cover) capped at factor 3.

## UI Layout (Avalonia)
- Tabs: Risk | Performance | Cost | Allocation | Modeling
- Drill Drawer: Supplier or Part context – trend charts, rationale factors list.

## Data Inputs
- Purchase orders & line schedules
- Supplier master, terms, quality incidents
- Item master criticality flags / safety stock
- Shortage lists & production demand
- Historical receipt dates vs PO need/confirm dates

## Macro Launch Parameters
- May include FocusSupplier or FocusPart for filtered startup.

## Acceptance Highlights
- No PO modification or expedite triggers executed.
- Each predictive score includes factor contribution breakdown (>=3 factors).
- Risk dashboard initial load < 6s cold; incremental refresh < 1.5s.

---
Draft – Subject to stakeholder review.