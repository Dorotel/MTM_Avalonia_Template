# Outstanding Clarification Questions: Diagnostics & System Health - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the **Diagnostics & System Health** feature.

**Note**: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (8 total)

### Q1: [Non-Functional] - Storage capacity warning threshold

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-044 check storage capacity

What storage capacity threshold should trigger a warning?

| Option | Description |
|--------|-------------|
| A | <100 MB free space |
| B | <500 MB free space |
| C | <1 GB free space |
| D | <10% of total capacity |
| E | <5% of total capacity or 1 GB, whichever is less |

**Answer: E — <5% of total capacity or 1 GB, whichever is less**

Reasoning: Hybrid approach works across device sizes. Small devices (32GB) benefit from percentage-based threshold (1.6GB at 5%), while large devices (512GB+) avoid excessive warnings (25GB at 5% is wasteful). The 1GB floor ensures minimum working space for cache operations, logs, and temporary files.

---

### Q2: [Edge Cases] - Permission failure handling

**Impact**: High
**Category**: Edge Cases & Failure Handling
**Feature Context**: FR-043 verify required permissions

What should happen if required permissions are missing?

| Option | Description |
|--------|-------------|
| A | Block startup completely with error message |
| B | Show warning but continue (may cause issues later) |
| C | Attempt to request permissions from user/system |
| D | Continue with reduced functionality (disable features needing permissions) |

**Answer: D — Continue with reduced functionality (disable features needing permissions)**

Reasoning: Graceful degradation provides better user experience than hard failure. Some permissions (camera, location) may be optional for core workflows. Android guidelines recommend runtime permission requests at feature usage time, not startup. Critical permissions (storage, network) should be requested at startup; if denied, disable dependent features and show persistent notification explaining limitations.

---

### Q3: [Non-Functional] - Network reachability timeout

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-046 verify network reachability

What timeout should be used for network reachability checks?

Format: Short answer (<=5 words)

**Answer: 5 seconds**

Reasoning: Balance between startup speed and reliable detection. Mobile networks can have 2-3 second latency spikes; 5 seconds accommodates this while preventing excessive boot delays. Use parallel checks if multiple endpoints are tested.

---

### Q4: [Non-Functional] - Performance benchmark criteria

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-047 performance benchmarks to detect slow hardware

What constitutes "slow hardware" that requires warning?

| Option | Description |
|--------|-------------|
| A | CPU benchmark <1000 ops/sec |
| B | Disk I/O <10 MB/sec |
| C | Memory allocation >100ms for 10MB |
| D | Composite score below threshold (CPU + disk + memory) |

**Answer: D — Composite score below threshold**

Reasoning: Holistic assessment better predicts user experience than single metrics. Weigh components by app impact (disk I/O matters most for boot/cache). Track percentiles from telemetry to set threshold.

---

### Q5: [Non-Functional] - Network quality measurement method

**Impact**: Medium
**Category**: Non-Functional Quality Attributes
**Feature Context**: FR-048 measure network quality (latency/bandwidth)

How should network quality be measured?

| Option | Description |
|--------|-------------|
| A | Single ping to Visual server |
| B | Multiple pings (average of 5) for latency + download speed test |
| C | Full bandwidth test (upload/download) |
| D | Latency only (bandwidth not measured) |

**Answer: B — Multiple pings (average of 5) for latency + download speed test**

Reasoning: Multiple pings provide statistically reliable latency measurement (outliers can be filtered). Download speed test (small file ~100KB) estimates data transfer performance without consuming excessive bandwidth. Skip upload test at boot to minimize startup time. Track metrics over time to detect degradation patterns.

---

### Q6: [Data Model] - Hardware capability detection scope

**Impact**: Low
**Category**: Domain & Data Model
**Feature Context**: FR-049 detect hardware capabilities

Which hardware capabilities should be detected?

| Option | Description |
|--------|-------------|
| A | CPU cores, total RAM only (minimal) |
| B | Option A + screen resolution, GPU availability |
| C | Option B + storage type (SSD/HDD), network adapters |
| D | Comprehensive: all of above + battery status, camera specs, sensors |

**Answer: C — CPU cores, RAM, screen resolution, GPU, storage type, network adapters**

Reasoning: Covers performance-relevant factors (CPU/RAM/storage type), UI adaptation needs (screen resolution), and connectivity capabilities (network adapters). Omit battery/camera/sensors unless features explicitly require them. GPU detection helps optimize rendering strategy (hardware acceleration vs. software). Storage type (SSD/HDD) informs cache preloading strategies.

---

### Q7: [Edge Cases] - Crash detection mechanism

**Impact**: High
**Category**: Edge Cases & Failure Handling
**Feature Context**: FR-051 detect if app crashed on previous launch

How should crash detection work?

| Option | Description |
|--------|-------------|
| A | Check for existence of lock file (created at startup, deleted at clean shutdown) |
| B | Check last log entry timestamp vs. expected shutdown time |
| C | OS crash reporting integration |
| D | Combination: lock file + log analysis + OS reports |

**Answer: D — Combination: lock file + log analysis + OS reports**

Reasoning: Defense in depth provides most reliable detection. Lock file catches abnormal terminations (force close, power loss). Log analysis detects crashes during shutdown sequence. OS crash reporting (Windows Event Log, Android logcat) captures system-level crashes. Cross-reference all three sources for high confidence. Store lock file in app data directory with timestamp for age-based cleanup.

---

### Q8: [Functional Scope] - Automatic fix scope

**Impact**: Medium
**Category**: Functional Scope & Behavior
**Feature Context**: FR-052 automatic fixes for common issues

Which issues should be automatically fixed vs. reported to user?

| Option | Description |
|--------|-------------|
| A | Only safe operations (create missing folders, clear temp files) |
| B | Option A + cache rebuild (after corruption detection) |
| C | Option B + configuration reset to defaults |
| D | Ask user permission before any automatic fix |

**Answer: B — Safe operations + cache rebuild after corruption detection**

Reasoning: Automatic fixes should be reversible and low-risk. Missing folders and temp file cleanup are idempotent. Cache rebuild is safe because cache is derivable from source data. Configuration reset requires user confirmation because it loses customizations. Log all automatic fixes for audit trail. Show post-boot summary of fixes applied.

---
