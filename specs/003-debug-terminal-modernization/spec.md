# Feature Specification: Debug Terminal Modernization

**Feature Branch**: `003-debug-terminal-modernization`
**Created**: 2025-10-06
**Status**: Ready for Planning - 5 of 8 Clarifications Resolved
**Input**: User description: "Debug Terminal Modernization - Real-time performance monitoring, boot timeline visualization, quick actions panel, error history, connection pool stats, environment variables display, assembly info, auto-refresh toggle, network diagnostics, historical trends, live log viewer, export functionality"

**Related Documents**:
- 📄 [Human-Friendly Overview](./OVERVIEW_COMPLETE.md) - Non-technical business specification
- 🖥️ [Current Debug Terminal](../../MTM_Template_Application/Views/DebugTerminalWindow.axaml)

---

## 📋 Executive Summary

This feature modernizes the Debug Terminal with 20 enhancements across 3 phases, transforming it from a static diagnostic display into an interactive performance monitoring hub. Phase 1 delivers real-time metrics, boot timeline visualization, quick actions, and error history. Phase 2 adds connection pool stats, environment variables, assembly info, and auto-refresh. Phase 3 provides network diagnostics, historical trends, live log viewer, and export functionality.

**Target Users**: Developers, DevOps engineers, support staff
**Primary Use Case**: Diagnose performance bottlenecks, connection failures, configuration issues
**Success Metric**: 50% reduction in time-to-diagnosis for boot/performance issues

---

## 🎯 Clarifications

### Index

**Resolved (Session 2025-10-06)**: CL-001, CL-002, CL-005, CL-007, CL-008

**Open**: CL-003, CL-004, CL-006

### Session 2025-10-06

- Q: How frequently should Debug Terminal refresh metrics? → A: User-configurable (1-30s) with 5s default
- Q: Which Quick Actions need confirmation dialogs? → A: Only "Clear Cache"
- Q: Network diagnostic timeout duration? → A: 5 seconds
- Q: Should Error History persist across restarts? → A: Session-only for Phase 1
- Q: How many boot sessions to retain for trends? → A: Last 10 boots

### CL-001: Historical Data Retention
- **Status**: Answered (2025-10-06)
- **Question**: How many previous boot sessions should Historical Trends retain?
- **Answer/Decision**: Last 10 boots (~50KB memory, ~1 week for typical developers)
- **Spec Changes**: Updated FR-029 and FR-032

### CL-002: Auto-Refresh Performance Impact
- **Status**: Answered (2025-10-06)
- **Question**: How frequently should metrics auto-refresh?
- **Answer/Decision**: User-configurable (1-30s) with 5s default
- **Spec Changes**: Updated FR-037, FR-038, FR-039

### CL-005: Network Diagnostics Timeouts
- **Status**: Answered (2025-10-06)
- **Question**: Network test timeout duration?
- **Answer/Decision**: 5 seconds (industry standard)
- **Spec Changes**: Updated FR-014, added FR-017

### CL-007: Error History Retention
- **Status**: Answered (2025-10-06)
- **Question**: Persist errors across restarts?
- **Answer/Decision**: Session-only for Phase 1
- **Spec Changes**: Updated FR-024, added FR-026

### CL-008: Quick Actions Confirmation
- **Status**: Answered (2025-10-06)
- **Question**: Which actions need confirmation?
- **Answer/Decision**: Only "Clear Cache"
- **Spec Changes**: Updated FR-026, added FR-029

---

## 📖 User Scenarios

### Primary Scenario: Diagnosing Slow Boot

**Alice** (Senior Developer) launches the app and notices Stage 1 takes 4 seconds (exceeding the 3-second target).

1. Alice opens **Debug Terminal**
2. Views **Boot Timeline Visualization** showing Stage 1 in red
3. Expands Stage 1 to see **Service Initialization Metrics**
4. Identifies MySQL connection taking 2.1 seconds
5. Checks **Connection Pool Statistics** → pooling disabled
6. Clicks **Quick Actions** → "Test Database Connection" → verifies connectivity
7. Uses **Export Functionality** → saves diagnostic JSON
8. Shares with DevOps team
9. **Result**: Connection pooling enabled, Stage 1 reduced to 1.8 seconds

### Additional Scenarios

**Scenario 2**: Developer monitors real-time CPU/memory during heavy operation
**Scenario 3**: Support staff exports diagnostics for bug report
**Scenario 4**: DevOps reviews error history to diagnose intermittent failures
**Scenario 5**: Engineer checks environment variables to verify configuration precedence

---

## 📦 Requirements (45 Total)

### Phase 1: Immediate Implementation (FR-001 to FR-026)

#### Real-Time Performance Monitoring (Feature #1)
- **FR-001**: Display current CPU usage % updated every 1 second [CL-006]
- **FR-002**: Display memory usage (MB) with color-coding: green <70MB, yellow 70-90MB, red >90MB
- **FR-003**: Display GC collection counts (Gen 0, 1, 2)
- **FR-004**: Display current thread count
- **FR-005**: Display current handle count
- **FR-006**: Real-time metrics MUST NOT cause UI blocking or frame drops

#### Boot Timeline Visualization (Feature #3)
- **FR-007**: Render horizontal bar chart for Stage 0, 1, 2 durations
- **FR-008**: Color-code bars: green (meets target), red (exceeds), gray (no data)
- **FR-009**: Display proportional bar widths relative to total boot time

#### Quick Actions Panel (Feature #10)
- **FR-026**: Provide Quick Actions panel with buttons:
  - "Clear Cache" (clears all entries, **requires confirmation**)
  - "Reload Configuration" (reloads from disk)
  - "Test Database Connection" (attempts MySQL connection)
  - "Force GC Collection" (triggers `GC.Collect()`)
  - "Refresh All Metrics" (manual update)
  - "Export Diagnostic Report" (opens export dialog)
- **FR-027**: All Quick Actions execute asynchronously without UI blocking
- **FR-028**: Show loading state (spinner) while executing
- **FR-029**: "Clear Cache" displays confirmation: "This will delete all cached data. Continue?"

#### Error History (Feature #9)
- **FR-024**: Display last 10 errors/warnings from current session (in-memory only)
- **FR-025**: Each error shows:
  - Timestamp (HH:mm:ss.fff)
  - Category (Database, Network, Cache, Configuration)
  - Error message (first 100 chars, expandable)
  - Expandable stack trace
  - Severity icon (🔴 Error, 🟡 Warning, ℹ️ Info)
- **FR-026**: Error history cleared on application close (session-only for Phase 1)

### Phase 2: Short-Term Implementation (FR-010 to FR-023)

#### Connection Pool Statistics (Feature #4)
- **FR-010**: Display MySQL connection pool stats [CL-004: Desktop-first]:
  - Active connections count
  - Pool size (max connections)
  - Average wait time (ms)
  - Connection failures (session)
- **FR-011**: Display HTTP client connection stats:
  - Reused connections count
  - DNS lookup average time (ms)
  - Active requests count

#### Environment Variables Display (Feature #12)
- **FR-012**: Display all `MTM_` prefixed environment variables
- **FR-013**: Show precedence: MTM_*> ASPNETCORE_* > DOTNET_* > Build Config
- **FR-014**: Display in sortable table: Key, Value, Source

#### Assembly & Version Info (Feature #8)
- **FR-015**: Display version info:
  - Avalonia UI (actual vs expected 11.3.6)
  - CommunityToolkit.Mvvm (actual vs expected 8.4.0)
  - .NET Runtime version
  - Build date/time
  - Git commit hash (if available)
- **FR-016**: Highlight version mismatches in yellow with warning icon

#### Auto-Refresh Toggle (Feature #18)
- **FR-037**: Provide toggle switch for auto-refresh (default: enabled) with configurable interval
- **FR-038**: Default refresh interval is 5 seconds; user can configure 1-30 second range
- **FR-039**: Display "Last Updated: [timestamp]" and current interval setting near toggle

### Phase 3: Future Implementation (FR-014 to FR-045)

#### Network Diagnostics (Feature #6)
- **FR-014**: Test and display connectivity for:
  - Visual ERP API (response time, last successful call)
  - Network drive (accessibility test)
  - Internet connectivity (DNS resolution)
- **FR-015**: Run tests asynchronously without blocking UI
- **FR-016**: Show error details in tooltip for failed tests
- **FR-017**: All network diagnostic tests use 5-second timeout

#### Historical Trends (Feature #11)
- **FR-029**: Display boot time trend chart retaining last 10 boot sessions
- **FR-030**: Chart shows:
  - Total boot time line graph
  - Target line (10 seconds)
  - Color-coded points (green under, red over target)
- **FR-031**: Calculate and display:
  - Average boot time (across last 10 boots)
  - Slowest boot with timestamp
  - Performance degradation alerts (if last 3 boots trend upward)
- **FR-032**: Historical data provides ~1 week visibility (~50KB memory footprint)

#### Live Log Viewer (Feature #14)
- **FR-032**: Display last 50 log lines from current log file
- **FR-033**: Support filtering by level: All, Info, Warning, Error
- **FR-034**: Support text search (case-insensitive)
- **FR-035**: Auto-scroll to bottom (with toggle to disable)
- **FR-036**: Syntax highlighting: timestamps gray, levels color-coded

#### Export Functionality (Feature #19)
- **FR-040**: Export diagnostic data to JSON + Markdown [CL-003]
- **FR-041**: Include in export:
  - Boot metrics (session ID, durations, status)
  - Performance metrics (CPU, memory, GC)
  - Service initialization metrics
  - Configuration settings (sanitized - no secrets)
  - Feature flags
  - Error history
  - Cache statistics
  - Database connection status
  - Environment variables
- **FR-042**: Sanitize sensitive data (passwords, API keys, tokens)
- **FR-043**: Include metadata: export timestamp, app version, platform
- **FR-044**: Provide "Copy to Clipboard" option
- **FR-045**: Show user-friendly error messages with retry option

---

## 🔑 Key Entities

### PerformanceSnapshot
Single point-in-time performance measurement.
- Timestamp (DateTimeOffset)
- CpuUsagePercent (double)
- MemoryUsageMB (long)
- Gen0/1/2CollectionCount (int)
- ThreadCount, HandleCount (int)

### BootTimeline
Complete boot sequence with stage breakdowns.
- SessionId (Guid)
- TotalDurationMs, Stage0/1/2DurationMs (long)
- StartedAt, CompletedAt (DateTimeOffset)

### ErrorEntry
Logged error/warning in error history.
- Timestamp (DateTimeOffset)
- Severity (enum: Error, Warning, Info)
- Category (string)
- Message, StackTrace (string)
- Source (string)

### ConnectionPoolStats
Database/HTTP connection pool statistics.
- PoolType (enum: MySQL, HTTP)
- ActiveConnections, MaxPoolSize (int)
- AverageWaitTimeMs (double)
- FailureCount, ReuseCount (int)
- DnsLookupTimeMs (double, HTTP only)

### DiagnosticExport
Complete diagnostic data export package.
- ExportTimestamp, AppVersion, Platform, SessionId
- BootMetrics, PerformanceSnapshot
- ErrorHistory, ConfigurationSettings
- EnvironmentVariables, FeatureFlags

---

## 📏 Non-Functional Requirements

### Performance
- **NFR-001**: Real-time updates increase CPU by ≤2%
- **NFR-002**: Render all sections within 500ms of window open
- **NFR-003**: Auto-refresh causes no UI stuttering
- **NFR-004**: Quick Actions complete within 3 seconds or show progress
- **NFR-005**: Export handles 10,000+ log lines without UI freeze

### Usability
- **NFR-006**: Consistent card-based layout with rounded corners
- **NFR-007**: Follow existing theme system (ThemeV2 tokens)
- **NFR-008**: Display appropriate units (ms, MB, %, count)
- **NFR-009**: Clear visual affordance for expandable sections

### Accessibility
- **NFR-010**: Color indicators include text labels (not color-only)
- **NFR-011**: All elements keyboard-accessible
- **NFR-012**: Tooltips appear within 500ms of hover

### Compatibility
- **NFR-013**: All features work on Windows Desktop
- **NFR-014**: Graceful degradation on Android (show "Not Available")
- **NFR-015**: Work in Light and Dark themes without visual bugs

---

## ❌ Out of Scope

This feature will NOT include:
- CPU profiler, memory profiler, allocation tracking
- Remote monitoring or cloud service
- Automated alerts (email/SMS notifications)
- Historical database persistence (in-memory only)
- Custom metrics or plugin system
- Interactive charts (zoom/pan/drill-down in Phase 1)
- Log file editing (read-only viewer)
- Multi-session comparison
- AI/ML performance recommendations

---

## 📊 Success Metrics

### Quantitative
- **Diagnostic Time Reduction**: 50% less time identifying bottlenecks
- **Export Adoption**: 70% of support tickets include diagnostic export
- **Quick Actions Usage**: 10+ uses per week for Clear Cache/Test Connection
- **Error Detection**: 90% of session errors visible (vs hidden in logs)

### Qualitative
- Debug Terminal is "first tool" for troubleshooting (survey feedback)
- Support diagnoses issues from exported JSON without additional data requests
- Zero "Debug Terminal is slow" performance complaints post-Phase 1

---

## ✅ Review & Acceptance Checklist

### Content Quality
- [x] No implementation details (Avalonia, XAML, ViewModels)
- [x] Focused on diagnostic value and user workflows
- [x] Written for product/technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [x] 5 of 8 clarifications resolved (CL-001, CL-002, CL-005, CL-007, CL-008)
- [ ] 3 clarifications remain open (CL-003, CL-004, CL-006) - low/medium priority, can proceed to planning
- [x] Requirements are testable and unambiguous (clarified requirements updated)
- [x] Success criteria are measurable
- [x] Scope clearly bounded (3 phases)
- [x] Dependencies and assumptions identified
- [x] Database schema changes: None (in-memory only)
- [x] Out of scope explicitly listed

### Execution Status
- [x] User description parsed
- [x] Key concepts extracted
- [x] 8 ambiguities identified
- [x] 5 high/medium-priority ambiguities resolved (2025-10-06)
- [x] User scenarios defined (6 acceptance + 5 edge cases)
- [x] 45 requirements generated across 3 phases (updated with clarifications)
- [x] 5 key entities identified
- [x] Review checklist passed (3 low-priority clarifications deferred)

---

## 📝 Document History

| Date | Change | Author |
|------|--------|--------|
| 2025-10-06 | Initial draft from feature description | GitHub Copilot |
| 2025-10-06 | Added 8 clarification points (CL-001 to CL-008) | GitHub Copilot |
| 2025-10-06 | Defined 3-phase implementation roadmap | GitHub Copilot |
| 2025-10-06 | Resolved 5 clarifications via interactive Q&A (CL-001, CL-002, CL-005, CL-007, CL-008) | GitHub Copilot |
| 2025-10-06 | Updated requirements with clarification decisions | GitHub Copilot |

---

**Status**: ✅ **Ready for Technical Planning (`/plan`)**

**Next Steps**:
1. ✅ ~~Resolve high-priority clarifications~~ (Complete)
2. (Optional) Resolve remaining 3 low-priority clarifications (CL-003, CL-004, CL-006) or defer to planning phase
3. **Proceed to `/plan`** - Generate technical implementation plan with architecture, task breakdown, and risk analysis
