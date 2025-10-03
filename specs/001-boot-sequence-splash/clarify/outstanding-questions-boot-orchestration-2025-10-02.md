# Outstanding Clarification Questions: Boot Orchestration & Failure Handling - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the Boot Orchestration & Failure Handling feature.

Note: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (12 total)

### Q1: [Non-Functional] - Stage timeout values

Impact: High  
Category: Non-Functional Quality Attributes  
Feature Context: FR-121 enforce timeout limits on all operations

What timeout should be enforced for each boot stage?

| Option | Description |
|--------|-------------|
| A | Stage 0: 5s, Stage 1: 30s, Stage 2: 10s |
| B | Stage 0: 10s, Stage 1: 60s, Stage 2: 15s |
| C | Stage 0: 15s, Stage 1: 90s, Stage 2: 20s |
| D | Configurable per environment |
| E | No fixed timeout (wait indefinitely) |

Answer: D  
Reasoning: Environments differ; set per-environment timeouts with sane defaults (e.g., B) and telemetry-driven tuning.

---

### Q2: [Non-Functional] - Parallel initialization strategy

Impact: High  
Category: Non-Functional Quality Attributes  
Feature Context: FR-124 execute independent steps in parallel

Which Stage 1 operations can be parallelized?

| Option | Description |
|--------|-------------|
| A | Config + Logging (independent, rest sequential) |
| B | Config + Logging + Diagnostics (all independent) |
| C | All services except those with explicit dependencies |
| D | No parallelization (strict sequential for simplicity) |

Answer: C  
Reasoning: Maximize concurrency via dependency graph; parallelize everything without explicit dependencies.

---

### Q3: [Non-Functional] - Watchdog timer threshold

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-126 detect hung initialization steps with watchdog

What delay indicates a "hung" initialization step?

| Option | Description |
|--------|-------------|
| A | 2x expected duration for that step |
| B | 30 seconds with no progress update |
| C | 60 seconds with no progress update |
| D | Per-step configurable thresholds |

Answer: D  
Reasoning: Step durations vary; use per-step thresholds with defaults like max(2x expected, 30s) and progress beacons.

---

### Q4: [Non-Functional] - Boot time performance budget

Impact: High  
Category: Non-Functional Quality Attributes  
Feature Context: FR-130 performance budget for boot time (<3s for Stage 1)

What is the TOTAL boot time target (all stages)?

| Option | Description |
|--------|-------------|
| A | <5 seconds (aggressive) |
| B | <10 seconds (balanced) |
| C | <15 seconds (conservative) |
| D | No total limit (only Stage 1 has <3s requirement) |

Answer: B  
Reasoning: Balanced target aligns with user expectations and allows Stage 1 <3s while keeping total under 10s.

---

### Q5: [Non-Functional] - Memory budget for startup

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-131 memory budget for startup operations

What is the maximum memory footprint during startup?

| Option | Description |
|--------|-------------|
| A | 50 MB (lightweight) |
| B | 100 MB (balanced) |
| C | 200 MB (feature-rich) |
| D | 500 MB (no constraints) |
| E | Device-dependent (percentage of available RAM) |

Answer: B  
Reasoning: 100 MB is a practical ceiling for desktop-class apps, preventing pressure on lower-end systems.

---

### Q6: [Functional Scope] - Service monitoring dashboard access

Impact: Low  
Category: Functional Scope & Behavior  
Feature Context: FR-132 admin monitoring dashboard for service status

Who should have access to the admin monitoring dashboard?

| Option | Description |
|--------|-------------|
| A | System administrators only |
| B | Administrators + power users |
| C | All users (transparency) |
| D | Configurable role-based access |

Answer: D  
Reasoning: RBAC supports least privilege and deployment-specific roles without hardcoding audiences.

---

### Q7: [Edge Cases] - Credential validation failure action

Impact: High  
Category: Edge Cases & Failure Handling  
Feature Context: FR-136 validate credentials before remote connections

What should happen if credential validation fails?

| Option | Description |
|--------|-------------|
| A | Block startup, force re-entry |
| B | Block startup, offer Retry/Exit options |
| C | Continue in offline mode (if possible) |
| D | Option B for first failure, option C after multiple failures |

Answer: D  
Reasoning: Avoid lockout UX but provide continuity; escalate to offline mode after repeated failures.

---

### Q8: [Edge Cases] - Endpoint unreachability decision logic

Impact: High  
Category: Edge Cases & Failure Handling  
Feature Context: FR-137 handle endpoint unreachability

What options should be presented when Visual server is unreachable?

| Option | Description |
|--------|-------------|
| A | Retry / Exit only |
| B | Retry / Continue (cached-only) / Exit |
| C | Retry / Skip (reduced features) / Exit |
| D | Retry / Continue / Safe Mode / Exit |

Answer: D  
Reasoning: Presents full set of safe continuations, including Safe Mode, matching resilience goals.

---

### Q9: [Data Model] - Diagnostic bundle contents

Impact: Medium  
Category: Domain & Data Model  
Feature Context: FR-140 create diagnostic bundles for support

What should be included in diagnostic bundles?

| Option | Description |
|--------|-------------|
| A | Logs + error details only |
| B | Logs + error details + system info |
| C | Logs + error details + system info + configuration (redacted) |
| D | Option C + screenshots + network traces |
| E | Option D + full memory dump (comprehensive) |

Answer: C  
Reasoning: Sufficient for triage while minimizing size/sensitivity; allow opt-in extensions when requested.

---

### Q10: [Edge Cases] - Error categorization strategy

Impact: Medium  
Category: Edge Cases & Failure Handling  
Feature Context: FR-141 categorize errors

What error categories should be defined?

| Option | Description |
|--------|-------------|
| A | Transient / Permanent only (simple) |
| B | Transient / Configuration / Network / Permanent |
| C | Transient / Configuration / Network / Permission / Permanent |
| D | Comprehensive taxonomy with subcategories |

Answer: C  
Reasoning: Adds Permission as a common cause while keeping taxonomy actionable and not over-complex.

---

### Q11: [User Experience] - Safe Mode feature scope

Impact: High  
Category: Functional Scope & Behavior  
Feature Context: FR-147 Safe Mode for basic operation

What features should be available in Safe Mode?

| Option | Description |
|--------|-------------|
| A | View cached data only (no writes, no sync) |
| B | Option A + manual data entry (offline mode) |
| C | Option B + settings/configuration |
| D | Configurable feature set per deployment |

Answer: D  
Reasoning: Deployments vary; define a minimal baseline (A) and enable additional capabilities by policy.

---

### Q12: [Non-Functional] - Error frequency threshold for chronic issue detection

Impact: Low  
Category: Non-Functional Quality Attributes  
Feature Context: FR-148 track error frequency to identify chronic issues

What threshold identifies a "chronic issue"?

| Option | Description |
|--------|-------------|
| A | Same error 3+ times in 24 hours |
| B | Same error 5+ times in 7 days |
| C | Same error 10+ times in 30 days |
| D | Configurable threshold per error type |

Answer: D  
Reasoning: Error rates differ by type and environment; supply defaults (e.g., A for transient, B for persistent) but allow overrides.
