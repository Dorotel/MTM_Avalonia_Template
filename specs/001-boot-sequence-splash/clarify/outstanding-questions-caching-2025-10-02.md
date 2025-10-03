# Outstanding Clarification Questions: Visual Master Data Caching - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the Visual Master Data Caching feature.

Note: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (11 total)

### Q1: [Non-Functional] - Cache refresh maintenance window

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-083 cache refresh policies with maintenance windows

When should cache refresh occur?

| Option | Description |
|--------|-------------|
| A | During app startup (every launch) |
| B | Nightly at configured time (e.g., 2 AM) |
| C | On-demand via user action only |
| D | Startup + nightly + on-demand |
| E | Background refresh at configurable intervals |

Answer: D — Startup + nightly + on-demand  
Reasoning:

- Startup guarantees basic freshness without blocking on large syncs.
- Nightly aligns work with maintenance windows and reduces daytime load.
- On-demand covers exceptional cases after known upstream changes.

---

### Q2: [Data Model] - Cache staleness threshold

Impact: High  
Category: Domain & Data Model  
Feature Context: FR-084 track cache age and staleness indicators

At what age should cache be considered "stale"?

| Option | Description |
|--------|-------------|
| A | 1 hour (very fresh) |
| B | 24 hours (daily refresh) |
| C | 7 days (weekly refresh) |
| D | 30 days (monthly refresh) |
| E | Different thresholds per data type |

Answer: E — Different thresholds per data type  
Reasoning:

- Data sensitivity varies; parts tend to change more frequently than locations/warehouses.
- Aligns with per-type TTL policy (see Q8) and avoids over-refreshing stable data.

---

### Q3: [Integration] - Delta sync detection method

Impact: High  
Category: Integration & External Dependencies  
Feature Context: FR-085 incremental/delta sync

How should changed records be detected for delta sync?

| Option | Description |
|--------|-------------|
| A | Timestamp column (LastModified) in Visual tables |
| B | Change tracking table in Visual |
| C | Full comparison: fetch all, compare with cache |
| D | Version numbers per record |
| E | Combination: timestamp + version for reliability |

Answer: E — Combination: timestamp + version  
Reasoning:

- Timestamps are simple but susceptible to clock skew and coarse precision.
- Version adds monotonic change tracking; together they reduce false negatives/positives.
- Falls back to full comparison only on detected inconsistencies.

---

### Q4: [Non-Functional] - Cache compression algorithm

Impact: Low  
Category: Non-Functional Quality Attributes  
Feature Context: FR-086 compress cached data

What compression should be used for cached data?

| Option | Description |
|--------|-------------|
| A | None (simplicity, faster access) |
| B | GZip (standard, moderate compression) |
| C | Brotli (better compression, slower) |
| D | LZ4 (fast, lower compression) |

Answer: D — LZ4  
Reasoning:

- Optimized for very fast decompression with acceptable ratios for structured data.
- Minimizes startup and background refresh latency relative to GZip/Brotli.

---

### Q5: [Functional Scope] - Critical data load priority order

Impact: Medium  
Category: Functional Scope & Behavior  
Feature Context: FR-087 prioritize critical data first

What is the exact priority order for loading cached data?

| Option | Description |
|--------|-------------|
| A | Parts → Locations → Warehouses → Work Centers |
| B | Warehouses → Locations → Parts → Work Centers |
| C | Parts → Warehouses → Locations → Work Centers |
| D | Configurable priority order |

Answer: C — Parts → Warehouses → Locations → Work Centers  
Reasoning:

- Parts unlocks most UX scenarios; warehouses contextualize inventory; locations refine placement.
- Work centers are less critical to initial navigation.

---

### Q6: [User Experience] - Background refresh notification

Impact: Medium  
Category: Interaction & UX Flow  
Feature Context: FR-088 refresh cache in background without blocking UI

Should users be notified during background cache refresh?

| Option | Description |
|--------|-------------|
| A | Yes, always show notification |
| B | Only if refresh takes >30 seconds |
| C | Only on errors or completion |
| D | Silent, only log to diagnostics |
| E | User preference setting |

Answer: E — User preference setting  
Reasoning:

- Defaults: notify on errors or completion (C); escalate to progress toast if >30s (B).
- Allows quiet environments to stay silent and power users to opt into progress.

---

### Q7: [Data Model] - Cache schema versioning strategy

Impact: High  
Category: Domain & Data Model  
Feature Context: FR-089 track cache schema version

How should cache schema changes be handled?

| Option | Description |
|--------|-------------|
| A | Auto-migrate: detect version, apply migrations |
| B | Full rebuild: invalidate cache on schema change |
| C | Backward compatible: older cache works with new app |
| D | Manual migration: admin reviews and approves |

Answer: A — Auto-migrate (with safe fallback)  
Reasoning:

- Automatic, idempotent migrations minimize user friction.
- If migration fails, fall back to full rebuild (B) to ensure operability.

---

### Q8: [Non-Functional] - TTL configuration by data type

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-090 different TTL for different data types

What TTL should each data type use?

| Option | Description |
|--------|-------------|
| A | Parts: 24h, Locations: 7d, Warehouses: 7d, Work Centers: 7d |
| B | Parts: 7d, Locations: 30d, Warehouses: 30d, Work Centers: 30d |
| C | All: 24h (uniform, simple) |
| D | All: 7d (uniform, balanced) |
| E | Configurable per data type |

Answer: E — Configurable per data type (defaults = A)  
Reasoning:

- Per-type configurability aligns with variability in change rates.
- Ship with sensible defaults (Parts 24h; others 7d) and allow ops override.

---

### Q9: [Non-Functional] - Preemptive refresh timing

Impact: Low  
Category: Non-Functional Quality Attributes  
Feature Context: FR-091 preemptively refresh cache before expiry

How early should cache be refreshed before expiry?

| Option | Description |
|--------|-------------|
| A | 10% of TTL before expiry |
| B | 25% of TTL before expiry |
| C | Fixed time: 1 hour before expiry |
| D | No preemptive refresh (refresh only on expiry) |

Answer: B — 25% of TTL before expiry  
Reasoning:

- Scales with TTL to smooth load and reduce synchronized expirations.
- Provides ample buffer for retries and slow networks.

---

### Q10: [Non-Functional] - Cache statistics tracking granularity

Impact: Low  
Category: Non-Functional Quality Attributes  
Feature Context: FR-092 track cache statistics

What cache statistics should be tracked?

| Option | Description |
|--------|-------------|
| A | Hit/miss rates only |
| B | Hit/miss rates + cache size + refresh durations |
| C | Option B + staleness per data type + error counts |
| D | Comprehensive: all metrics + per-query breakdown |

Answer: C — B plus staleness and error counts  
Reasoning:

- Captures effectiveness (hit/miss), cost (size/durations), and risk (staleness/errors).
- Avoids high-cardinality per-query metrics overhead.

---

### Q11: [Edge Cases] - Cache initialization failure handling

Impact: High  
Category: Edge Cases & Failure Handling  
Feature Context: Overall caching system

What should happen if initial cache load fails?

| Option | Description |
|--------|-------------|
| A | Block app startup (cannot proceed without cache) |
| B | Continue with empty cache, retry in background |
| C | Use last known good cache (if available) |
| D | Option C, else option B |

Answer: D — Use last known good; else continue empty and retry  
Reasoning:

- Prioritizes availability with known-good data when possible.
- Maintains usability even when cache is empty, while background recovery proceeds.

---
