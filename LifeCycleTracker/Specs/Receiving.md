# Receiving Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for inbound logistics & receiving; visibility into schedule adherence, ASN deviations, inspection bottlenecks, and put-away readiness. No writes.

## Strategic Goals
- Reduce inbound dock congestion.
- Accelerate availability of critical materials.
- Detect supplier ASN / packaging issues early.

## Feature List
1. Inbound Arrival Forecast Board
   - Predicts ETA windows for due today/tomorrow POs using historical supplier transit variance vs static due date.
2. ASN Deviation Detector
   - Compares ASN declared quantities / lot attributes vs historical accuracy to flag likely mismatches before arrival.
3. Dock Utilization & Queue Forecast
   - Projects dock door occupancy by hour from scheduled and predicted arrivals.
4. Critical Material Fast-Track Candidates
   - Highlights inbound lines resolving active production shortages; shows recommended priority lane assignment.
5. Quarantine Aging Heatmap
   - Visualizes age distribution of items in QC hold vs target release SLA.
6. Inspection Bottleneck Analyzer
   - Identifies part families causing disproportionate inspection queue delay.
7. Packaging Damage Risk Predictor
   - Predictive score using supplier historical damage incident rate + packaging type + weather exposure (if data).
8. Partial Receipt Trend Tracker
   - Monitors increasing partial receipts frequency per supplier (efficiency & planning signal).
9. Put-Away Ready vs Space Constraint Panel
   - Items cleared but awaiting bin assignment vs available bin capacity (space utilization tension insight).
10. Overdue Receipt Alert Feed
    - Ranked list of POs past expected arrival filtered by criticality & shortage impact.
11. Lot Trace Pre-Validation
    - Pre-check of incoming lots against restricted or superseded lot lists (compliance risk) without receiving.
12. First Pass Yield (FPY) Insight
    - Rolling FPY for inspected items; downward trends signal process or supplier quality shifts earlier.
13. Advance Prioritization Simulator (Read-only)
    - Simulate impact on shortage resolution timeline if a selected inbound is expedited through inspection.
14. Supplier ASN Timeliness Score
    - Measures average lead time between ASN submission and actual arrival; penalizes just-in-time ASNs.
15. Receiving Cycle Time Breakdown
    - Decomposition of total cycle (arrival → put-away) into wait, inspection, staging, system update (read-only analytic).
16. Temperature Sensitive Material Arrival Watch (If Data)
    - List of expected refrigerated items with dwell risk predictions.
17. Reusable Container Turn Tracker
    - Tracks return cycle duration for supplier returnable containers; flags slow turn suppliers.
18. Label Readability Exception Trend
    - Frequency of manual relabel events per supplier (process improvement signal).

## Predictive / Scoring Notes
- ETA Forecast uses supplier-specific variance model + day-of-week correction.
- Packaging Damage Probability logistic on incident density, packaging type code, weather severity index.

## UI Layout (Avalonia)
- Tabs: Schedule | Risk | Quality | Space | Simulation
- Timeline lane for dock/queue; drill drawer for PO line context (ASN, shortages, inspection status).

## Data Inputs
- Purchase orders & expected dates
- ASN records (if integrated)
- Receiving transactions / timestamps
- QC inspection dispositions
- Inventory on-hand & bin capacity
- Supplier incident / damage logs
- Shortage list

## Macro Launch Parameters
- Optional FocusPO or FocusSupplier.

## Acceptance Highlights
- No receipt, inspection, or put-away actions triggered.
- Every predictive panel lists contributing factors.
- Dock forecast refresh incremental < 2s.

---
Draft – Subject to stakeholder review.