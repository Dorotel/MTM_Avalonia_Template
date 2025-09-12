# Sales Read‑Only Feature Spec

Version: Draft 1
Infor Visual: 9.0.8 SP6
Scope: Read-only augmentation for sales reps, account managers, and order management. Focus on backlog health, quote performance, retention risk, and promise reliability. No writes.

## Strategic Goals
- Improve quote win rate & cycle speed.
- Increase promise date credibility.
- Detect churn and margin risk early.

## Feature List
1. Backlog Health Gradient
   - Assigns each open order line a composite health color (late risk, margin risk, allocation readiness). Visual provides separate views.
2. Quote Win Probability Scoring
   - Predictive score per active quote using factors: historical customer acceptance latency, discount depth, similar item prior outcomes.
3. At-Risk Customer Retention Panel
   - Flags accounts with declining order cadence or shrinking average order value vs baseline (churn precursors).
4. Promise Reliability Forecast
   - Projects reliability % for current open backlog window (next 30 days) using production load + shortages + historical adherence.
5. Margin Guard Advisory
   - Highlights quotes where proposed selling price yields margin below peer deals for similar configuration.
6. Upsell / Cross-Sell Opportunity Lens
   - Suggests complementary items frequently ordered together that are absent for the account's current pattern (association rules). Read-only hints.
7. Response Time Variance Tracker
   - Measures actual quote response cycle vs target SLA by sales territory.
8. Expedite Request Impact Estimator (Read-only)
   - Simulates schedule ripple if a selected order is expedited X days (no changes applied).
9. Pricing Consistency Heatmap
   - Visualizes discount variance across similar items & regions to surface outliers.
10. Lost Quote Reason Cluster Analyzer
    - Clusters lost quotes by reason text (NLP keyword grouping) beyond static reason codes.
11. Backlog Concentration Risk Gauge
    - Identifies when top N customers exceed threshold % of total backlog exposure.
12. Promotional Effectiveness Snapshot
    - For flagged promo periods, compares lift vs baseline for targeted vs non-targeted accounts.
13. Real-Time Allocation Readiness Board
    - Shows unallocated but pick-ready order lines (material fully available) enabling proactive scheduling calls.
14. Split Shipment Probability Indicator
    - Predicts likelihood an order will require multiple shipments based on line readiness divergence.
15. Gross Margin Volatility Tracker
    - Rolling standard deviation of margin % across last 50 closed orders by product family to spotlight unstable pricing.
16. SLA Breach Early Warning
    - Alerts when unacknowledged quotes approach SLA limit (e.g., 75% elapsed with no draft sent).
17. Customer Engagement Recency Meter
    - Time since last meaningful interaction (quote or order) vs expected cadence for similar revenue tier.
18. Backlog Aging Waterfall
    - Visual decomposition of backlog value by aging buckets with delta vs prior week.

## Predictive / Scoring Notes
- Win Probability: logistic style blending discount ratio, historical latency, similar item acceptance, seasonality factor.
- Retention Risk = (Cadence Deviation Z + AOV Decline % + Engagement Gap Index)/3 normalized.
- Promise Reliability = 1 - weighted late risk across lines (weights by revenue contribution).

## UI Layout (Avalonia)
- Tabs: Backlog | Quotes | Accounts | Pricing | Predictive
- Drill Drawer: Customer or Quote -> historical patterns, risk factors, similar wins.

## Data Inputs
- Quotes (header, line, status, dates, price)
- Sales orders (promise, required, actual ship, margin)
- Customer master, historical order cadence
- Inventory allocation & shortage info
- Production schedule snapshots

## Macro Launch Parameters
- Optional FocusCustomer or FocusQuoteId

## Acceptance Highlights
- No repricing or date change operations triggered.
- Every predictive panel surfaces top 3 feature contributions.
- Backlog health computation deterministic within same dataset snapshot.

---
Draft – Subject to stakeholder review.