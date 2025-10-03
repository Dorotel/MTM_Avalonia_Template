# Outstanding Clarification Questions: Logging, Telemetry & Monitoring - 2025-10-02

Generated from multi-feature specification analysis. This file contains clarification questions specific to the Logging, Telemetry & Monitoring feature.

Note: This is part of a multi-feature specification. See other clarification files in this directory for questions about other features.

## Questions (12 total)

### Q1: [Data Model] - Default log level

Impact: Medium  
Category: Functional Scope & Behavior  
Feature Context: FR-027 command-line log level filtering

What is the default log level when no filter is specified?

| Option | Description |
|--------|-------------|
| D | Debug (all logs) |

Answer: D — Debug (all logs).

---

### Q2: [Non-Functional] - Log file size limit

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-038 auto-rotate log files by size/date

What is the maximum size for a single log file before rotation?

| Option | Description |
|--------|-------------|
| B | 10 MB (balanced) |

Answer: B — 10 MB (balanced).

---

### Q3: [Non-Functional] - Log file retention policy

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-038 configurable retention policy

How long should log files be retained?

| Option | Description |
|--------|-------------|
| A | 7 days |

Answer: A — 7 days.

---

### Q4: [Integration] - Remote telemetry protocol

Impact: High  
Category: Integration & External Dependencies  
Feature Context: FR-032 optional remote telemetry endpoints

What protocol should be used for remote telemetry?

| Option | Description |
|--------|-------------|
| D | Standard telemetry protocol (OpenTelemetry, Application Insights) |

Answer: D — Standard telemetry protocol (OpenTelemetry, Application Insights).

---

### Q5: [Non-Functional] - Correlation ID scope

Impact: Medium  
Category: Domain & Data Model  
Feature Context: FR-035 correlation IDs for tracking operations

What is the scope/lifetime of a correlation ID?

| Option | Description |
|--------|-------------|
| C | Per request (including all downstream operations) |

Answer: C — Per request (including all downstream operations).

---

### Q6: [Non-Functional] - Performance metrics granularity

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-036 method execution time and memory usage

What is the threshold for capturing performance metrics?

| Option | Description |
|--------|-------------|
| B | Only methods taking >100ms |
| D | Only explicitly marked "monitored" methods |
| E | Adaptive: sample percentage based on system load |

Answer: Start with D (tag critical paths) + B (catch unexpected slow calls). Move to E if broader coverage is needed without heavy overhead.

---

### Q7: [Non-Functional] - Log buffer size

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-037 buffer logs during high-volume operations

How many log entries should be buffered before flushing to disk?

| Option | Description |
|--------|-------------|
| B | 100 entries (balanced) |

Answer: B — 100 entries (balanced).

---

### Q8: [Integration] - Remote telemetry retry policy

Impact: High  
Category: Integration & External Dependencies  
Feature Context: FR-039 queue locally and retry when endpoints fail

What retry policy should be used for failed telemetry sends?

| Option | Description |
|--------|-------------|
| D | Queue only: store locally, send on next successful connection |

Answer: D — Queue locally and resend on next successful connection.

---

### Q9: [Security] - PII redaction patterns

Impact: High  
Category: Non-Functional Quality Attributes  
Feature Context: FR-040 automatically redact PII from logs

What patterns should be considered PII and redacted?

| Option | Description |
|--------|-------------|
| A | Email addresses, phone numbers, SSN patterns |
| B | Option A + IP addresses, MAC addresses |
| C | Option B + names matching person entity patterns |
| D | Option C + addresses, dates of birth |
| E | Configurable regex patterns + default sensitive fields |

Answer: Prefer E: ship strong defaults (at least A + B, and usually addresses and DOB from D), then allow project-specific additions via config.  
Use structured logging so you can redact by field name, not just regex.  
Favor allowlisting safe fields to log, instead of trying to blocklist everything risky.  
Where values are needed, consider hashing/truncation or tokenization.  
Watch for false positives (names/addresses), international formats, and performance of regex.  
Add unit tests for redaction and verify redaction happens before logs leave the process.

---

### Q10: [Non-Functional] - Log sampling rate for high-frequency events

Impact: Medium  
Category: Non-Functional Quality Attributes  
Feature Context: FR-041 log sampling for high-frequency events

What is the sampling strategy for high-frequency events?

| Option | Description |
|--------|-------------|
| A | Log every 100th occurrence |

Answer: A — Log every 100th occurrence.

---

### Q11: [Data Model] - Log context enrichment fields

Impact: Medium  
Category: Domain & Data Model  
Feature Context: FR-042 enrich logs with user, session, and version context

What contextual information should be added to every log entry?

| Option | Description |
|--------|-------------|
| E | All available context (comprehensive, larger logs) |

Answer: E — All available context (comprehensive).

---

### Q12: [Non-Functional] - Structured logging format

Impact: Medium  
Category: Domain & Data Model  
Feature Context: FR-034 structured JSON logging

What JSON schema should structured logs follow?

| Option | Description |
|--------|-------------|
| A | Custom flat schema: { timestamp, level, message, context } |
| B | Industry standard (e.g., ECS - Elastic Common Schema) |
| C | Cloud provider format (Application Insights, CloudWatch) |
| D | OpenTelemetry semantic conventions |

Answer: Prefer D (OpenTelemetry semantic conventions) for field names and semantics. Emit one JSON object per line, OTel-aligned and easily mappable to ECS/AI when needed.

Minimal OTel-aligned fields:

- timestamp (ISO 8601 UTC)
- severity_text, severity_number
- body (message text)
- logger.name
- exception.type, exception.message, exception.stacktrace (when present)
- trace_id, span_id, correlation_id
- service.name, service.version
- deployment.environment
- host.name
- process.pid
- thread.id
- attributes { ...custom structured fields... }

Notes:

- Redact PII before serialization (see Q9).
- Keep keys stable and use OTel names; provide adapters to map to ECS (@timestamp, log.level, message, trace.id, span.id, service.*) or cloud providers when exporting.
- One event per line; UTF-8; no trailing commas.
- Enable correlation (Q5) by always setting trace_id/span_id/correlation_id.

---
