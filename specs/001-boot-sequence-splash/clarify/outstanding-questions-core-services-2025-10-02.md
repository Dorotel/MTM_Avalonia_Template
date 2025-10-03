# Outstanding Clarification Questions: Core Application Services - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Core Application Services** feature.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (7 total)

### Q1: [Integration] - Message bus delivery guarantees

**Impact**: High
**Category**: Integration & External Dependencies
**Feature Context**: FR-066 message bus/event aggregator, FR-072 priority messages and dead letter queue

What delivery guarantees should the message bus provide?

| Option | Description |
|--------|-------------|
| A | At-most-once (fire and forget, no retries) |
| B | At-least-once (retry until delivered, possible duplicates) |
| C | Exactly-once (guaranteed single delivery) |
| D | Configurable per message type |

**Answer: D — Configurable per message type**

Reasoning: Different message types have different requirements. UI notifications: at-most-once (fire and forget, no harm if missed). Data sync events: at-least-once (subscribers must be idempotent). Critical business events: exactly-once (use transactional outbox pattern). Default to at-least-once for safety, but allow message-level configuration. Subscribers implement idempotency keys to handle duplicate deliveries safely.

---

### Q2: [Non-Functional] - Dead letter queue retry policy

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-072 dead letter queue

When should messages be moved to dead letter queue?

| Option | Description |
|--------|-------------|
| A | After 3 failed delivery attempts |
| B | After 5 failed delivery attempts |
| C | After 10 failed delivery attempts |
| D | Configurable per message type |
| E | Never (retry indefinitely) |

**Answer: B — After 5 failed delivery attempts**

Reasoning: 5 retries balances reliability and resource consumption. With exponential backoff (1s, 2s, 4s, 8s, 16s), total retry time is ~30 seconds — enough to recover from transient subscriber failures. Beyond 5 attempts indicates persistent problem requiring manual intervention. Log dead-lettered messages with full context for troubleshooting. Provide admin UI to inspect and replay dead-lettered messages after issue resolution.

---

### Q3: [Functional Scope] - Validation rule discovery mechanism

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-073 auto-discover validation rules

How should validation rules be discovered?

| Option | Description |
|--------|-------------|
| A | Attribute-based (e.g., [Required], [Range(1,100)]) |
| B | Convention-based (e.g., ValidateX methods) |
| C | Configuration files (JSON/XML rule definitions) |
| D | All of the above (hybrid approach) |

**Answer: D — All of the above (hybrid approach)**

Reasoning: Each approach has distinct advantages. Attributes handle simple constraints (Required, Range, RegEx) declaratively. Convention methods enable complex business logic (cross-field validation, async database checks). Configuration files allow runtime customization without recompilation. Use FluentValidation for composition. Discovery order: Attributes first (performance), then conventions (logic), then configuration overrides (customization). Cache discovered validators per type for performance.

---

### Q4: [Data Model] - Mapping profile scope

**Impact**: Medium
**Category**: Domain & Data Model
**Feature Context**: FR-074 multiple mapping profiles

What are the different mapping profile contexts?

| Option | Description |
|--------|-------------|
| A | API (external) vs. Internal (domain models) |
| B | Create vs. Update vs. Display contexts |
| C | Per-feature profiles (Inventory, Manufacturing, etc.) |
| D | All of the above |

**Answer: D — All of the above**

Reasoning: Multiple profile dimensions address different concerns. API vs Internal (boundary translation). Create vs Update (field inclusion rules: CreateDate excluded from updates). Display vs Edit (computed fields, security filtering). Feature-specific (Inventory needs different mappings than Manufacturing). Use AutoMapper with named profiles. Profile naming convention: `{Feature}.{Direction}.{Context}` (e.g., `Inventory.Api.Display`, `Manufacturing.Internal.Update`). Register profiles via assembly scanning at startup.

---

### Q5: [Data Model] - Schema versioning backward compatibility depth

**Impact**: High
**Category**: Domain & Data Model
**Feature Context**: FR-075 schema versioning for backward compatibility

How many versions back should be supported?

| Option | Description |
|--------|-------------|
| A | 1 version (N-1) |
| B | 2 versions (N-2) |
| C | 3 versions (N-3) |
| D | All versions (indefinite support) |
| E | Configurable per deployment |

**Answer: B — 2 versions (N-2)**

Reasoning: N-2 support covers realistic deployment scenarios. Current version (N) on all servers, previous version (N-1) during staged rollouts, emergency rollback version (N-2). Three-version window allows 2 release cycles before breaking changes. Indefinite support (option D) creates unbounded technical debt. Single version (N-1) is too fragile for distributed deployments. Version negotiation: client sends supported versions, server responds with highest compatible. Deprecate schema versions with 2-release notice period.

---

### Q6: [Non-Functional] - Retry exponential backoff parameters

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-076 smart retry with exponential backoff and jitter

What backoff strategy should be used?

| Option | Description |
|--------|-------------|
| A | Base: 1s, Multiplier: 2x, Jitter: ±25%, Max: 30s |
| B | Base: 2s, Multiplier: 2x, Jitter: ±50%, Max: 60s |
| C | Base: 500ms, Multiplier: 1.5x, Jitter: ±10%, Max: 15s |
| D | Configurable per service |

**Answer: A — Base: 1s, Multiplier: 2x, Jitter: ±25%, Max: 30s**

Reasoning: 1s base provides quick recovery from transient failures without overwhelming retry frequency. 2x multiplier (1s, 2s, 4s, 8s, 16s, 30s cap) scales appropriately. 25% jitter prevents thundering herd while maintaining predictable timing. 30s max keeps retry window reasonable for user experience. Formula: `delay = min(base * (multiplier ^ attempt) * random(0.75, 1.25), max)`. Override per-operation via Polly policies for special cases (critical operations: lower base, background jobs: higher max).

---

### Q7: [Non-Functional] - Service health check interval

**Impact**: Low
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-077 expose health checks for all services

How frequently should service health be checked?

| Option | Description |
|--------|-------------|
| A | Every 10 seconds |
| B | Every 30 seconds |
| C | Every 60 seconds |
| D | On-demand only (no background checks) |
| E | Adaptive based on recent health status |

**Answer: B — Every 30 seconds**

Reasoning: 30-second interval balances responsiveness and resource usage. Frequent enough to detect service degradation quickly (within 1 minute). Infrequent enough to avoid performance impact on services. Health check endpoints should be lightweight (cache status for 5 seconds, return immediately). On-demand checks (option D) miss background degradation. Adaptive checks (option E) add complexity with minimal benefit. Expose health via `/health` endpoint returning: Healthy, Degraded, Unhealthy with subsystem details.

---
