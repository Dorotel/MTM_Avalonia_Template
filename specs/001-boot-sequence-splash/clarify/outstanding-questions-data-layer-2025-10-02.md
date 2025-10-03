# Outstanding Clarification Questions: Data Layer & Connectivity - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Data Layer & Connectivity** feature.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (10 total)

### Q1: [Non-Functional] - Connection pool size

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-056 connection pooling with retry policies

What should be the connection pool configuration?

| Option | Description |
|--------|-------------|
| A | Min: 1, Max: 5 (conservative) |
| B | Min: 2, Max: 10 (balanced) |
| C | Min: 5, Max: 20 (high throughput) |
| D | Dynamic: scale based on load |
| E | Configurable per environment |

**Answer: E — Configurable per environment**

Reasoning: Different deployment scenarios have vastly different needs. Desktop development (Min: 1, Max: 3), production desktop (Min: 2, Max: 10), Android warehouse tablets (Min: 1, Max: 5 due to resource constraints). Provide sensible defaults per platform but allow override via configuration. Monitor pool utilization metrics to tune values post-deployment.

---

### Q2: [Non-Functional] - Connection retry policy

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-056 retry policies

What retry strategy should be used for failed connections?

| Option | Description |
|--------|-------------|
| A | Exponential backoff: 1s, 2s, 4s, 8s, 16s, max 5 retries |
| B | Fixed interval: 5 seconds, max 3 retries |
| C | Immediate retry 3 times, then exponential |
| D | Circuit breaker pattern: fail fast after threshold |

**Answer: A — Exponential backoff: 1s, 2s, 4s, 8s, 16s, max 5 retries**

Reasoning: Exponential backoff prevents overwhelming struggling servers while providing rapid recovery from transient failures. First retry at 1s catches momentary glitches. 5 retries over ~30 seconds balances user patience with network recovery time. Add jitter (±25%) to prevent thundering herd when multiple clients reconnect simultaneously. Combine with circuit breaker for persistent failures.

---

### Q3: [Non-Functional] - Connection health check interval

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-057 monitor connection health with automatic reconnection

How frequently should connection health be checked?

| Option | Description |
|--------|-------------|
| A | Every 10 seconds (aggressive) |
| B | Every 30 seconds (balanced) |
| C | Every 60 seconds (conservative) |
| D | Before each operation (no background checks) |
| E | Adaptive: more frequent during issues, less during stability |

**Answer: E — Adaptive: more frequent during issues, less during stability**

Reasoning: Adaptive strategy balances responsiveness and resource usage. Stable connections: check every 60 seconds. After failed operation: increase to every 10 seconds for 5 minutes, then scale back. After successful reconnection: gradually return to normal interval. Reduces battery drain on mobile devices while providing quick recovery from network issues.

---

### Q4: [Non-Functional] - Circuit breaker thresholds

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-058 circuit breaker pattern

What should trigger the circuit breaker to open?

| Option | Description |
|--------|-------------|
| A | 3 consecutive failures |
| B | 5 failures within 1 minute |
| C | 50% failure rate over 10 requests |
| D | Configurable per service/endpoint |

**Answer: D — Configurable per service/endpoint**

Reasoning: Different endpoints have different criticality and failure characteristics. Critical operations (authentication): 3 consecutive failures (fast fail). Data queries: 50% failure rate over 20 requests (tolerate intermittent issues). Batch operations: 5 failures within 2 minutes. Provide defaults: 5 consecutive failures for general case, but allow tuning based on telemetry and operational experience.

---

### Q5: [Non-Functional] - Circuit breaker recovery timing

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-058 circuit breaker pattern

How long should circuit breaker stay open before attempting recovery?

| Option | Description |
|--------|-------------|
| A | 30 seconds |
| B | 1 minute |
| C | 5 minutes |
| D | Exponential: 30s, 1m, 2m, 5m, 10m |

**Answer: D — Exponential: 30s, 1m, 2m, 5m, 10m**

Reasoning: Exponential backoff prevents repeated attempts to failing services while allowing quick recovery from transient issues. First attempt at 30s catches brief outages. Subsequent backoff gives struggling services time to recover without constant probe traffic. Cap at 10 minutes to ensure eventual reconnection attempts. Reset backoff timer on successful recovery. Allow manual retry button for user-initiated reconnection.

---

### Q6: [Non-Functional] - Connection pool metrics tracking

**Impact**: Low
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-059 track connection pool metrics

What connection pool metrics should be tracked?

| Option | Description |
|--------|-------------|
| A | Active connections, idle connections only |
| B | Option A + average wait time, peak usage |
| C | Option B + connection errors, timeouts, lifespan distribution |
| D | Comprehensive: all metrics with percentile distributions |

**Answer: C — Basic + errors, timeouts, lifespan distribution**

Reasoning: Option C provides actionable diagnostics without excessive overhead. Active/idle counts show pool utilization. Wait times reveal contention. Errors and timeouts indicate infrastructure problems. Connection lifespan helps tune pool settings. Skip percentile distributions (option D overhead) unless performance issues require deep analysis. Emit metrics to telemetry system for dashboarding and alerting.

---

### Q7: [Non-Functional] - Bulk operation batch size

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-060 optimize for bulk/batch operations

What should be the default batch size for bulk operations?

| Option | Description |
|--------|-------------|
| A | 50 records per batch |
| B | 100 records per batch |
| C | 500 records per batch |
| D | 1000 records per batch |
| E | Dynamic based on record size and network speed |

**Answer: E — Dynamic based on record size and network speed**

Reasoning: Optimal batch size varies significantly by context. Small records (Part lookups ~200 bytes): 500 records/batch. Large records (Transaction history ~2KB): 100 records/batch. Target: 50-200KB per batch for reliable network transmission. Measure network speed at startup and adjust batch sizes dynamically. Provide configuration override for specific operations. Start with conservative default (100 records) until dynamic calculation completes.

---

### Q8: [Non-Functional] - Query cache TTL

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-061 cache frequently-accessed queries

How long should query results be cached?

| Option | Description |
|--------|-------------|
| A | 1 minute (fresh data, high query volume) |
| B | 5 minutes (balanced) |
| C | 15 minutes (reduce load) |
| D | 1 hour (mostly static data) |
| E | Different TTL per query type |

**Answer: E — Different TTL per query type**

Reasoning: Data volatility varies dramatically. User lookup/permissions: 15 minutes (rarely changes). Active work orders: 1 minute (frequently updated). Part master data: 1 hour (stable). Configuration tables: 24 hours (nearly static). Provide metadata attribute to specify TTL per query. Default: 5 minutes for unspecified queries. Support cache invalidation hints from Visual integration layer when data changes are detected.

---

### Q9: [Functional Scope] - Lazy initialization trigger

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-062 lazy initialization to defer data layer init

When should lazy-initialized data layer be triggered?

| Option | Description |
|--------|-------------|
| A | On first database query |
| B | On first screen that needs data |
| C | After user authenticates |
| D | User preference: option to pre-initialize or lazy-load |

**Answer: C — After user authenticates**

Reasoning: Authentication provides necessary credentials for database connections. Triggering before auth would require fallback/retry logic. Triggering on first query causes unpredictable latency spikes during user workflows. Post-authentication initialization allows connection pooling and cache preloading in background while user navigates to first screen. Show subtle loading indicator during initialization. Allow power users to disable auto-initialization if they work offline frequently.

---

### Q10: [Data Model] - Read-only connection enforcement

**Impact**: High
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-064 enforce read-only connections to Visual

How should read-only enforcement be implemented?

| Option | Description |
|--------|-------------|
| A | Database user with SELECT-only permissions |
| B | Connection string with ReadOnly=true |
| C | Application-level validation (reject INSERT/UPDATE/DELETE) |
| D | All of the above (defense in depth) |

**Answer: D — All of the above (defense in depth)**

Reasoning: Multiple layers prevent accidental or malicious writes. Database permissions (Layer 1) is authoritative but requires DBA coordination. ReadOnly connection string (Layer 2) fails fast at driver level. Application validation (Layer 3) catches attempts before network calls. Audit logs track any rejected write attempts for security monitoring. Exception: API Toolkit commands may perform writes through controlled pathways; these bypass app-level validation but still respect database permissions.

NOTE: THIS ONLY APPLIES TO THE INFOR VISUAL DATABASE / SERVER NOT THE MAMP MYSQL 5.7 SERVER! THE MAMP MYSQL SERVER WILL BE BASED ON THE USER'S ACCESS LEVEL.  ADMIN USERS WILL HAVE FULL ACCESS, WHILE STANDARD USERS WILL HAVE RESTRICTED ACCESS AND READ ONLY USERS WILL HAVE READ ONLY ACCESS.

---
