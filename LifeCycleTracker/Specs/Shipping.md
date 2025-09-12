# Shipping Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for shipping coordinators; focus on on-time performance, dock efficiency, carrier quality, staging optimization. No writes.

## Strategic Goals
- Improve OTIF (On-Time In-Full) performance.
- Reduce dock dwell and staging congestion.
- Enhance carrier selection insight.

## Feature List
1. OTIF Predictive Dashboard
   - Forecast next 7/14 day OTIF % using backlog readiness + historical miss causes beyond static historical KPI.
2. Staging Lane Congestion Forecast
   - Predict occupancy by hour combining scheduled picks & planned ship confirmations.
3. Carrier Performance Deviation Monitor
   - Detects rising transit delay variance vs SLA for each carrier lane.
4. Consolidation Opportunity Finder
   - Suggests candidate partial shipments that could be consolidated before cutoff time (advisory only).
5. Pick-to-Ship Cycle Analyzer
   - Measures elapsed time from pick complete to ship confirm vs target; clusters chronic delays.
6. Late Risk Shipment Heat List
   - Ranks outbound orders by probability of missing ship date (shortage residual, documentation readiness, capacity backlog).
7. Packing Efficiency Indicators
   - Read-only derived metric: average cube utilization vs standard per product family.
8. Label/Reprint Exception Aggregator
   - Collects orders with frequent label reprint events (indicative of process confusion or data mismatch).
9. Carrier Claim Propensity Index
   - Predictive model of likelihood of damage/shortage claim based on lane, item fragility class, historical incident rate.
10. Dock Door Turn Rate Tracker
    - Rolling average door usage vs theoretical capacity; flags underutilized or bottleneck doors.
11. Hazard / Special Handling Visibility Panel
    - Consolidated view of shipments requiring hazmat, temperature control, or documentation exceptions.
12. Partial vs Full Shipment Ratio Trend
    - Trend visualization to detect increasing partial shipment frequency (margin & cost impact signal).
13. Shipment Prioritization Advisory
    - Suggests picking/packing sequence to maximize OTIF improvements (read-only list).
14. ASN Readiness Checker
    - Flags orders lacking required EDI/ASN data elements early.
15. Load Sequencing Static Optimizer
    - Proposed sequence for multi-stop loads based on distance & due time (advisory only—no TMS write).
16. Freight Cost Deviation Insight
    - Highlights shipments where actual or projected cost per weight diverges > threshold vs lane average.
17. Temperature Excursion Risk Alert (If Data Available)
    - Predicts risk of excursion given forecast weather + packaging dwell time.
18. Documentation Completeness Meter
    - Percent of required docs (BOL, certs, MSDS) prepared per outbound order.

## Predictive / Scoring Notes
- Late Ship Probability uses: allocation completion %, staging time remaining, carrier booking cut-off proximity, pick queue aging.
- Claim Propensity: logistic based on lane incident density + item fragility + carrier historical claim ratio.

## UI Layout (Avalonia)
- Tabs: Performance | Risk | Capacity | Carrier | Advisory
- Timeline panel for staging/dock occupancy.
- Drill Drawer: Shipment -> readiness checklist, risk factors, consolidation suggestions.

## Data Inputs
- Sales orders / shipment schedules
- Pick confirmations & staging timestamps
- Carrier master & historical transit times
- Freight cost records (if accessible read-only)
- Label print event logs (if available)
- Item handling attributes (hazmat, fragility)

## Macro Launch Parameters
- Optional FocusOrder or FocusCarrier.

## Acceptance Highlights
- No carrier booking or shipment confirmation actions triggered.
- Every predictive score delivers factor breakdown list.
- Staging forecast updates incrementally < 2s.

---
Draft – Subject to stakeholder review.