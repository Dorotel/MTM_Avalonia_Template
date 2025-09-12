# Cross-File Consistency Review

Date: 2025-09-12
Scope: Ensure departmental specs align on terminology, scoring patterns, macro integration, and constraints.

## Common Constraints
- Read-only across all modules (no DB writes, no procedure invoking state changes).
- Explainable predictive outputs (>=3 contributing factors listed where applicable).
- Macro launch parameters optional and prefixed with Focus* (FocusPart, FocusWorkOrder, etc.).
- Performance targets: initial cold hydration under 6–8s; incremental refresh < 2s typical.

## Shared Predictive Themes
- Probability / Risk Scores: logistic or ensemble with clear factor listing.
- Forecast Windows: Common 7/14/30 day where temporal forecasting appears (standardization aids caching layer design).
- Severity / Criticality Metrics: Demand At Risk Hours, Shortage Severity, Bottleneck Probability reused with consistent naming.

## Terminology Alignment
| Concept | Term Used | Notes |
|---------|-----------|-------|
| On-Time In-Full | OTIF | Shipping spec defines; reused explicitly in Management risk context. |
| Late Risk Probability | Late Risk Probability | Sales, Management, Shipping share identical label. |
| Shortage Severity | Shortage Severity | Defined in Material Handling; referenced in Production & Management. |
| Bottleneck Probability | Bottleneck Probability | Defined in Production; reused in Management composite indices. |
| Readiness Score | Readiness Score | Setup spec primary; derivative references avoid redefining formula per context. |
| Claim Propensity | Claim Propensity | Shipping only; not cross-used to avoid domain overload. |

## Data Source Overlaps
- Shortage Lists: Material Handling, Production, Receiving, Management draw from same cached shortage view.
- Supplier Performance: Purchasing core; Shipping (carrier) & Receiving (ASN timeliness) separate domains to avoid conflation.
- Work Orders / Routing: Setup & Production share base schema; Setup adds tooling metadata layering.

## Caching / Hydration Strategy (Preliminary)
- Core Cache Layer: Parts, Suppliers, Customers, WorkCenters, Open WOs, Open POs, Backlog, Shortages.
- Domain-Specific Augmentation: Predictive results stored in memory with timestamp & factor array to avoid recompute inside refresh threshold (e.g., 60s sliding window).

## Factor Attribution Standard
Structure: For each scored entity store array of (FactorName, ContributionValue, Direction, NormalizedWeight).

## Security / Redaction
- Management & Sales: Margin / cost fields redacted for non-privileged roles (show index). Applied uniformly.
- Customer PII masking pattern reused from Material Handling spec.

## Open Harmonization Items
1. Define unified Demand At Risk Hours calculation function signature.
2. Consolidate learning curve & setup variance models to share historical normalization code.
3. Determine universal threshold palette (Green < 0.3, Amber 0.3–0.6, Red > 0.6 risk probability) for UI consistency.
4. Decide snapshot vs streaming update cadence for Bottleneck & Staging forecasts.

## Next Steps
- Approve shared naming & thresholds.
- Draft data model / DTO definitions for predictive outputs.
- Map Visual API Toolkit endpoints to each data input list in specs.

---
Review complete; awaiting stakeholder sign-off.