# Maintenance (Asset Reliability) Read‑Only Feature Spec

Company Context: High-cycle presses, fabrication equipment, tooling support assets
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for maintenance planners & reliability engineers. No work order edits or schedule changes.

## Strategic Goals
- Predict and prevent unplanned downtime.
- Optimize preventive intervals based on actual stress factors.
- Expose systemic reliability degraders early.

## Feature List
1. Predictive Failure Probability Board
   - Next 7/14 day failure risk per critical asset using micro-stoppage trend + load factor + temperature/vibration proxies (if logged).
2. PM Interval Optimization Advisor
   - Flags PM tasks consistently early (unused life) or late (elevated condition indicators) for potential interval right-sizing.
3. Mean Time Between Failure (MTBF) Drift Monitor
   - Detects statistically significant downward trend per asset group earlier than raw cumulative metrics.
4. Condition-Based Trigger Alignment View
   - Compares actual sensor/operational triggers to PM schedule timing (missed opportunities).
5. Downtime Cause Concentration Heatmap
   - Aggregates downtime minutes by cause code vs asset to identify top systemic issues.
6. Spare Parts Stockout Risk Lens
   - Predicts potential spare shortfalls given failure likelihood + current on-hand + open PO ETA.
7. Maintenance Backlog Aging Waterfall
   - Visual aging distribution of open corrective vs preventive tasks with ratio shift tracking.
8. Asset Criticality vs Spend Bubble Chart
   - Plots maintenance spend vs criticality to surface over/under maintenance scenarios.
9. Early Degradation Pattern Detector
   - Identifies assets with increasing micro-stoppage frequency or longer restart times (precursor to failure).
10. Lubrication Compliance Tracker
    - Measures adherence to lubrication schedule vs actual logged events (if data available) to correlate with wear.
11. Repeated Repair Ineffectiveness Index
    - Flags assets where repeat same-cause repairs occur within too-short intervals (root cause unresolved).
12. PM Effectiveness Score
    - Reduction in failure probability post-PM vs baseline average (effectiveness regression analysis).
13. Energy Anomaly Insight (If Energy Data)
    - Detects increased kWh per productive hour suggesting mechanical drag or inefficiency.
14. Tooling Related Downtime Attribution
    - Separates die/tool related downtime minutes for cross-link with Die Shop improvement actions.
15. Deferred Maintenance Risk Stack
    - Aggregates risk-weighted impact of deferred tasks (Probability * Criticality * Production Exposure Hours).
16. Technician Skill Coverage Map
    - Highlights maintenance domains with limited skill redundancy (single point of failure risk).
17. Predictive vs Actual Failure Post-Mortem Panel
    - Compares recent actual failures vs prior predicted risk band for model calibration transparency.
18. Opportunity Window Advisor (Read-only)
    - Suggests ideal upcoming production lull windows to schedule high-impact PM without rescheduling jobs.

## Predictive / Scoring Notes
- Failure Probability logistic features: RecentMicroStoppageRate, MeanCycleLoad%, TimeSinceLastPM, TemperatureVariance, VibrationAmplitudeIndex.
- PM Effectiveness = (Pre-PM Failure Risk - Post-PM Failure Risk)/Pre Risk with confidence interval.

## UI Layout (Avalonia)
- Tabs: Risk | PM Optimization | Downtime | Spares | Analytics
- Drill Drawer: Asset -> risk factors, stoppage timeline, recommended interval adjustments.

## Data Inputs
- Maintenance work orders (PM & corrective)
- Downtime logs (cause codes, duration)
- Micro-stoppage event logs (if captured)
- Asset master (criticality, manufacturer, baseline intervals)
- Spare parts inventory & open POs
- Sensor or condition data (temperature, vibration, energy) if accessible

## Macro Launch Parameters
- Optional FocusAssetId.

## Acceptance Highlights
- No work order create/close/modify actions.
- Each predictive risk lists factor contributions & last model refresh timestamp.
- Risk board loads < 5s for <= 500 critical assets.

---
Draft – Subject to stakeholder review.