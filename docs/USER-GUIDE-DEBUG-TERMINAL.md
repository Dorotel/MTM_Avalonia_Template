# Debug Terminal User Guide

**Application**: MTM Avalonia Template
**Version**: 1.0.0
**Last Updated**: October 8, 2025
**Audience**: Developers, QA Engineers, Support Teams

---

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Feature Guide](#feature-guide)
   - [Performance Monitoring](#performance-monitoring)
   - [Boot Timeline](#boot-timeline)
   - [Error History](#error-history)
   - [Connection Pool Diagnostics](#connection-pool-diagnostics)
   - [Environment Variables](#environment-variables)
   - [Quick Actions](#quick-actions)
   - [Auto-Refresh](#auto-refresh)
   - [Diagnostic Export](#diagnostic-export)
4. [Understanding the Display](#understanding-the-display)
5. [Common Workflows](#common-workflows)
6. [Troubleshooting](#troubleshooting)
7. [FAQ](#faq)
8. [Technical Reference](#technical-reference)

---

## Overview

The Debug Terminal is a comprehensive diagnostic tool built into the MTM Avalonia Template application. It provides real-time visibility into application performance, boot metrics, errors, and system state.

### What Can You Do With It?

- **Monitor Performance**: Track memory usage, CPU metrics, and system health in real-time
- **Diagnose Boot Issues**: Visualize boot sequence timeline and identify slow stages
- **Review Errors**: Access persistent error history with severity levels and timestamps
- **Check Connectivity**: Monitor Visual ERP API connection pool utilization
- **Inspect Configuration**: View environment variables (sensitive data filtered)
- **Run Quick Diagnostics**: Clear errors, force garbage collection, reset metrics
- **Export Reports**: Save diagnostic data to JSON for offline analysis
- **Auto-Refresh**: Continuously update metrics at configurable intervals (1-10 seconds)

### When Should You Use It?

- **Development**: Validate performance budgets, monitor memory leaks, debug boot issues
- **QA/Testing**: Verify application meets performance requirements, capture diagnostic snapshots
- **Production Support**: Troubleshoot customer issues, export diagnostics for engineering review
- **Performance Profiling**: Establish baselines, identify bottlenecks, compare before/after changes

---

## Getting Started

### Opening the Debug Terminal

**Method 1: From Main Window**
1. Launch the MTM Avalonia Template application
2. Look for the **🔧 Debug Terminal** button in the title bar (center position)
3. Click the button to open the Debug Terminal window

**Method 2: Keyboard Shortcut** *(if configured)*
- Press **Ctrl+Shift+D** (Windows/Linux) or **Cmd+Shift+D** (macOS)

### Initial View

When the Debug Terminal opens, you'll see:
- **Header**: Application name, version, platform information
- **Boot Sequence Section**: Stage timings and service initialization metrics
- **Performance Monitoring**: Real-time memory and CPU snapshots
- **Configuration Section**: Environment type, configuration settings, feature flags
- **Error History**: Recent errors with severity indicators
- **Quick Actions**: Buttons for common diagnostic operations
- **Auto-Refresh Controls**: Toggle and interval slider

### First-Time Setup

**Recommended Settings**:
1. **Enable Auto-Refresh**: Toggle the "Auto-Refresh" switch to ON
2. **Set Interval**: Use the slider to set refresh interval (recommended: 2 seconds)
3. **Pin Window**: Keep the Debug Terminal open on a secondary monitor during development

---

## Feature Guide

### Performance Monitoring

**Purpose**: Track real-time system performance metrics and historical trends.

#### What You'll See

- **Current Snapshot**:
  - **Total Memory**: Application's total memory usage (private + shared) in MB
  - **Private Memory**: Memory exclusively used by this process
  - **Snapshot Count**: Number of snapshots captured (max 100, circular buffer)
  - **Last Updated**: Timestamp of most recent snapshot

- **Snapshot History**: DataGrid showing last 100 performance snapshots
  - Timestamp (date and time)
  - Total Memory (MB) - color-coded: 🟢 <70MB, 🟡 70-90MB, 🔴 >90MB
  - Private Memory (MB)
  - Error Count (number of errors at that snapshot)

#### How to Use It

**Real-Time Monitoring**:
1. Enable auto-refresh (2-second interval)
2. Perform operations in the application
3. Watch memory usage column for spikes or trends
4. Color indicators show if memory is within acceptable range

**Memory Leak Detection**:
1. Force garbage collection (Quick Actions → Force GC)
2. Note baseline memory usage
3. Perform test scenario
4. Force GC again
5. Compare memory usage - significant increase suggests leak

**Performance Baselining**:
1. Start application with Debug Terminal open
2. Wait for boot to complete
3. Note memory usage after Stage 2 (baseline)
4. Compare future runs against this baseline

#### Color Indicators

| Color    | Memory Range | Meaning                           |
| -------- | ------------ | --------------------------------- |
| 🟢 Green  | <70MB        | Excellent - well below target     |
| 🟡 Yellow | 70-90MB      | Good - approaching target         |
| 🔴 Red    | >90MB        | Warning - exceeds target (<100MB) |

---

### Boot Timeline

**Purpose**: Visualize boot sequence performance and identify slow stages.

#### What You'll See

- **Stage Breakdown**:
  - **Stage 0**: Splash screen and initial setup (Target: <1 second)
  - **Stage 1**: Core service initialization (Target: <3 seconds)
  - **Stage 2**: Application ready (Target: <1 second)
  - **Total Boot Time**: Sum of all stages (Target: <10 seconds)

- **For Each Stage**:
  - Stage name and number
  - Duration in milliseconds
  - Target duration (spec requirement)
  - Visual indicator: 🟢 Meets target, 🔴 Exceeds target
  - Percentage of total boot time

#### How to Use It

**Identifying Slow Stages**:
1. Look for red indicators (exceeds target)
2. Note the duration and percentage
3. Review service metrics for that stage (in Boot Sequence section)
4. Investigate slow services

**Comparing Boot Performance**:
1. Export diagnostics to JSON (capture baseline)
2. Make code changes
3. Restart application
4. Export diagnostics again
5. Compare boot timelines between exports

**Resetting Timeline** *(if implemented)*:
- Click "Reset Timeline" in Quick Actions
- Useful for clearing test data before production measurements

#### Performance Targets

| Stage     | Target       | Description                               |
| --------- | ------------ | ----------------------------------------- |
| Stage 0   | <1000ms      | Splash screen, logging setup              |
| Stage 1   | <3000ms      | Core services (Config, Secrets, Database) |
| Stage 2   | <1000ms      | UI initialization, main window            |
| **Total** | **<10000ms** | **Complete boot sequence**                |

---

### Error History

**Purpose**: Access persistent log of application errors with severity levels and timestamps.

#### What You'll See

- **Error List**: Last 50 errors (most recent first)
  - **Timestamp**: When error occurred
  - **Severity Icon**: 🔴 Critical/Error, 🟡 Warning, ℹ️ Info
  - **Message**: Error description
  - **Stack Trace** *(if available)*: Expandable detailed error information

- **Error Count**: Total errors captured since last clear
- **Latest Error Indicator**: Most recent error highlighted

#### How to Use It

**Debugging Production Issues**:
1. User reports an issue
2. Open Debug Terminal → Error History
3. Look for errors near the reported time
4. Expand stack trace for detailed information
5. Export diagnostics to share with engineering

**Monitoring Error Frequency**:
1. Enable auto-refresh
2. Perform test scenario
3. Watch error count in performance snapshots
4. Identify scenarios that trigger errors

**Clearing Error History**:
- Click "Clear Errors" in Quick Actions
- Useful before capturing clean diagnostic exports

#### Severity Levels

| Icon | Severity | Meaning                              | Action                 |
| ---- | -------- | ------------------------------------ | ---------------------- |
| 🔴    | Critical | Application unstable, data loss risk | Immediate attention    |
| 🔴    | Error    | Operation failed, user impact        | Investigation required |
| 🟡    | Warning  | Potential issue, operation succeeded | Monitor for patterns   |
| ℹ️    | Info     | Informational, no action needed      | Reference only         |

---

### Connection Pool Diagnostics

**Purpose**: Monitor Visual ERP API connection pool health and utilization.

#### What You'll See

- **Active Connections**: Currently in-use connections to Visual ERP API
- **Idle Connections**: Available connections in pool (waiting for requests)
- **Total Connections**: Active + Idle (pool size)
- **Utilization Percentage**: Active / Total × 100%

#### How to Use It

**Identifying Connection Leaks**:
1. Check idle connections count
2. Perform API operations
3. Verify connections return to idle state
4. If idle count doesn't increase, investigate connection disposal

**Optimizing Pool Size**:
1. Monitor utilization during peak usage
2. If utilization frequently >80%, increase pool size
3. If utilization always <20%, decrease pool size

**Troubleshooting API Timeout Issues**:
1. Check active connections during timeouts
2. If active = total (100% utilization), pool is exhausted
3. Increase pool size or reduce concurrent API calls

#### Healthy Connection Pool

- **Idle > 0**: Always have available connections
- **Utilization 20-80%**: Efficient pool sizing
- **No stuck connections**: Active count decreases after operations complete

---

### Environment Variables

**Purpose**: Inspect application configuration from environment variables.

#### What You'll See

- **Filtered Variable List**: Key-value pairs from environment
  - Variable name (key)
  - Variable value
  - Security indicator (if sensitive data filtered)

- **Security Note**: Sensitive variables (PASSWORD, TOKEN, SECRET, KEY) are automatically filtered for security

#### How to Use It

**Verifying Configuration**:
1. Check for expected environment variables
2. Verify values match deployment configuration
3. Confirm sensitive data is redacted (shows "***FILTERED***")

**Troubleshooting Configuration Issues**:
1. User reports feature not working
2. Check environment variables for feature flags or API endpoints
3. Verify values are set correctly
4. Export diagnostics to share configuration (sensitive data excluded)

#### Filtered Variables

These patterns are automatically filtered:
- `*PASSWORD*`
- `*TOKEN*`
- `*SECRET*`
- `*KEY*`
- `*CONNECTION_STRING*`
- `*API_KEY*`

Filtered values show as: `***FILTERED FOR SECURITY***`

---

### Quick Actions

**Purpose**: Run common diagnostic operations with a single click.

#### Available Actions

**1. Clear Errors**
- **What It Does**: Resets error history (clears all 50 errors)
- **When to Use**: Before capturing clean diagnostic export, after fixing known issues
- **Result**: Error History panel shows "No errors logged"

**2. Force Garbage Collection**
- **What It Does**: Forces .NET garbage collector to run and captures snapshot
- **When to Use**: Establishing memory baseline, before/after memory leak tests
- **Result**: Memory usage may decrease, new snapshot added to history

**3. Reset Timeline** *(if implemented)*
- **What It Does**: Clears boot timeline data
- **When to Use**: Before production boot measurements, after test runs
- **Result**: Boot Timeline panel resets

**4. Export Diagnostics** *(see [Diagnostic Export](#diagnostic-export))*
- **What It Does**: Saves full diagnostic report to JSON file
- **When to Use**: Sharing diagnostics with engineering, archiving performance data
- **Result**: JSON file created in user-selected location

---

### Auto-Refresh

**Purpose**: Continuously update diagnostic data at configurable intervals.

#### How to Use It

**Enabling Auto-Refresh**:
1. Toggle the "Auto-Refresh" switch to **ON** (blue)
2. Diagnostic data refreshes automatically at set interval
3. Status indicator shows "Active"

**Setting Refresh Interval**:
1. Use the slider to adjust interval (1-10 seconds)
2. **1 second**: Real-time monitoring (higher CPU usage)
3. **2-3 seconds**: Recommended for general use (balanced)
4. **5-10 seconds**: Low overhead, periodic checks

**Disabling Auto-Refresh**:
1. Toggle the switch to **OFF** (gray)
2. Manual refresh only (click Refresh button)
3. Useful for reducing system load or capturing static snapshots

#### Best Practices

- **Development**: 2-second interval for active coding
- **Testing**: 1-second interval for performance validation
- **Production Support**: 5-second interval (minimize overhead)
- **Exporting Diagnostics**: Disable auto-refresh before export (stable snapshot)

---

### Diagnostic Export

**Purpose**: Save complete diagnostic data to JSON file for offline analysis, sharing, or archiving.

#### What's Included in Export

**Metadata**:
- Export timestamp
- Application version
- Platform information (OS, runtime version)
- Export reason (optional user note)

**Performance Data**:
- All snapshots in circular buffer (up to 100)
- Memory usage history
- Error counts per snapshot

**Error History**:
- Last 50 errors with timestamps, severity, messages, stack traces

**Boot Metrics**:
- Stage durations (Stage 0, 1, 2)
- Total boot time
- Service initialization metrics

**Connection Pool**:
- Active/idle connection counts
- Utilization percentage

**Environment Variables**:
- Filtered variable list (sensitive data excluded)

**Configuration**:
- Environment type (Development, Staging, Production)
- Feature flag states
- Secrets service status

#### How to Export

**Method 1: Quick Actions Button**:
1. Click "Export Diagnostics" in Quick Actions panel
2. Choose save location and filename in file dialog
3. Wait for export completion notification
4. JSON file saved to selected location

**Method 2: Keyboard Shortcut** *(if configured)*:
- Press **Ctrl+Shift+E** (Export)

#### Using Exported Data

**Offline Analysis**:
- Open JSON file in text editor or JSON viewer
- Search for specific errors, timestamps, or metrics
- Compare exports from different runs or versions

**Sharing with Engineering**:
- Attach JSON to support ticket or GitHub issue
- Email to engineering team (secure - sensitive data filtered)
- Include reproduction steps and expected vs. actual behavior

**Automated Processing**:
- Parse JSON in scripts for automated testing
- Generate reports from exported data
- Track performance trends across versions

#### JSON Structure

```json
{
  "exportMetadata": {
    "timestamp": "2025-10-08T14:30:00Z",
    "appVersion": "1.0.0",
    "platform": "Windows 11",
    "reason": "Performance investigation"
  },
  "performanceSnapshots": [ ... ],
  "errorHistory": [ ... ],
  "bootMetrics": { ... },
  "connectionPool": { ... },
  "environmentVariables": [ ... ],
  "configuration": { ... }
}
```

---

## Understanding the Display

### Color Coding System

The Debug Terminal uses consistent color coding across all panels:

| Color        | Meaning                | Examples                                   |
| ------------ | ---------------------- | ------------------------------------------ |
| 🟢 **Green**  | Meets/beats target     | Memory <70MB, boot stage <target           |
| 🟡 **Yellow** | Approaching limit      | Memory 70-90MB, warnings                   |
| 🔴 **Red**    | Exceeds limit or error | Memory >90MB, boot stage >target, errors   |
| ⚪ **Gray**   | Neutral/disabled       | Disabled feature flags, filtered variables |

### Reading Metrics

**Duration Metrics** (milliseconds):
- Shown as: `1234ms` or `1.234s`
- Target shown inline: `(Target: <3000ms)`
- Compare actual vs. target to assess performance

**Memory Metrics** (megabytes):
- Shown as: `45.67 MB`
- Private memory = application-exclusive memory
- Total memory = private + shared libraries

**Percentages**:
- Shown as: `45%` or `45.6%`
- Connection pool utilization: 0% = all idle, 100% = all active
- Boot stage percentage: % of total boot time

**Timestamps**:
- Format: `2025-10-08 14:30:45` (ISO 8601 local time)
- All timestamps use local time zone for readability

---

## Common Workflows

### Workflow 1: Investigating Slow Boot

**Scenario**: Application takes >10 seconds to start.

**Steps**:
1. Open Debug Terminal immediately after boot
2. Navigate to **Boot Timeline** section
3. Identify stages with 🔴 red indicators (exceeds target)
4. Check **Total Boot Time** - if >10000ms, exceeds spec
5. Review **Service Metrics** in Boot Sequence section
6. Find slowest service in the problem stage
7. Export diagnostics for offline analysis
8. Share JSON with engineering team

**Expected Outcome**:
- Identify specific slow service (e.g., "DatabaseMigrationService: 4500ms")
- Root cause: Database migration running on every boot (should be one-time)
- Fix: Add migration version check

---

### Workflow 2: Detecting Memory Leaks

**Scenario**: Application memory grows over time during normal use.

**Steps**:
1. Open Debug Terminal
2. Enable auto-refresh (2-second interval)
3. Click **Force GC** in Quick Actions (establish baseline)
4. Note **Private Memory** value (e.g., 45MB)
5. Perform test scenario (e.g., open/close windows 10 times)
6. Click **Force GC** again
7. Compare **Private Memory** - significant increase suggests leak
8. Review **Snapshot History** for growth trend
9. Export diagnostics with note: "Memory leak test - 10 iterations"

**Expected Outcome**:
- Memory returns to baseline after GC = No leak
- Memory remains elevated (e.g., +20MB) = Potential leak
- Export shows clear "before vs. after" data for engineering

---

### Workflow 3: Troubleshooting API Connectivity

**Scenario**: Visual ERP API calls timing out or failing.

**Steps**:
1. Open Debug Terminal
2. Navigate to **Connection Pool Diagnostics**
3. Check **Utilization Percentage**
4. If 100% utilization:
   - Pool exhausted (all connections active)
   - Increase pool size in configuration
5. If <20% utilization:
   - Connections available but still failing
   - Check network connectivity or API endpoint
6. Review **Error History** for API-related errors
7. Export diagnostics for support ticket

**Expected Outcome**:
- High utilization → Increase pool size
- Low utilization + errors → Network or API issue
- Detailed error messages provide clues for troubleshooting

---

### Workflow 4: Validating Performance After Changes

**Scenario**: Code changes might impact performance - need validation.

**Steps**:
1. Before making changes:
   - Open Debug Terminal
   - Export diagnostics → Save as `baseline.json`
2. Make code changes and rebuild
3. Restart application
4. Open Debug Terminal
5. Compare current metrics to baseline:
   - Boot time (should be similar or better)
   - Memory usage (should not increase significantly)
   - Error count (should be zero or known errors)
6. Export diagnostics → Save as `after-changes.json`
7. Compare JSON files (automated diff or manual review)

**Expected Outcome**:
- Boot time unchanged or improved ✅
- Memory usage unchanged or reduced ✅
- No new errors ✅
- Changes validated, safe to merge

---

## Troubleshooting

### Debug Terminal Won't Open

**Symptom**: Clicking Debug Terminal button does nothing.

**Possible Causes**:
1. Exception during ViewModel construction
2. Missing service dependency (DI registration issue)
3. UI thread blocked

**Solutions**:
1. Check application logs for exceptions
2. Verify `DebugTerminalViewModel` is registered in `ServiceCollectionExtensions.cs`
3. Restart application
4. If persistent, check `MainViewModel.OpenDebugTerminalCommand` for errors

---

### Auto-Refresh Stops Working

**Symptom**: Auto-refresh enabled but metrics don't update.

**Possible Causes**:
1. Timer disposed unexpectedly
2. Exception during refresh operation
3. UI thread blocked

**Solutions**:
1. Toggle auto-refresh OFF then ON
2. Close and reopen Debug Terminal window
3. Check error history for refresh-related errors
4. Restart application if issue persists

---

### Export Fails or Produces Empty File

**Symptom**: Export diagnostics creates empty or invalid JSON file.

**Possible Causes**:
1. Insufficient disk space
2. File path contains invalid characters
3. Insufficient permissions
4. Exception during serialization

**Solutions**:
1. Verify disk space available
2. Choose simple filename (no special characters)
3. Select location with write permissions (e.g., Documents folder)
4. Check error history for serialization errors
5. Try exporting to different location

---

### Memory Metrics Show N/A or Zero

**Symptom**: Memory usage shows "N/A" or "0 MB".

**Possible Causes**:
1. Performance counter not available on this platform
2. Exception during metrics capture
3. Insufficient permissions

**Solutions**:
1. Run application with administrator privileges (Windows)
2. Check platform support (some metrics unavailable on Android)
3. Review error history for metrics-related errors
4. Verify `DiagnosticService` is initialized correctly

---

### Connection Pool Shows All Zeros

**Symptom**: Active, idle, and total connections all show 0.

**Possible Causes**:
1. Visual API client not initialized yet
2. Desktop-only feature accessed on Android
3. API client unavailable (configuration error)

**Solutions**:
1. Wait for boot to complete (Stage 2)
2. Verify running on Desktop (not Android)
3. Check configuration for Visual API endpoint
4. Review boot metrics for API client initialization errors

---

## FAQ

### General Questions

**Q: Does the Debug Terminal affect application performance?**
A: Minimal impact (<0.01% CPU). Performance monitoring is lightweight by design. Auto-refresh at 2-second interval adds negligible overhead. For production use, disable auto-refresh to further reduce impact.

**Q: Can I use the Debug Terminal in production builds?**
A: Yes, but with caution. Sensitive data is filtered from exports, but the terminal reveals internal metrics. Recommended: Include in Production but hide button (accessible via keyboard shortcut for support staff).

**Q: How much memory does the circular buffer use?**
A: Approximately 65.8KB for 100 snapshots. Well under the 100KB budget. See `PERFORMANCE-OPTIMIZATION.md` for detailed analysis.

**Q: What happens when the circular buffer fills up?**
A: Oldest snapshots are automatically overwritten (circular/ring buffer). You always have the most recent 100 snapshots.

---

### Feature-Specific Questions

**Q: Why are some environment variables showing "***FILTERED***"?**
A: Security measure. Variables containing PASSWORD, TOKEN, SECRET, KEY, or CONNECTION_STRING are automatically redacted to prevent accidental exposure of sensitive data.

**Q: Can I increase the snapshot buffer size beyond 100?**
A: Not via UI. Developers can modify `CircularBufferDiagnosticSnapshot` capacity (constructor parameter). Recommended: Keep at 100 to stay within performance budget.

**Q: Why doesn't the boot timeline show historical data?**
A: Boot timeline reflects the current session only. For historical tracking, export diagnostics after each boot and compare JSON files.

**Q: What's the difference between Total Memory and Private Memory?**
A: **Private Memory** = memory exclusively used by this process. **Total Memory** = private + shared memory (libraries, system). Private memory is better for leak detection.

---

### Troubleshooting Questions

**Q: Why do some snapshots show higher error counts than others?**
A: Error count is cumulative since last "Clear Errors". If errors spike at a specific timestamp, investigate what operation occurred at that time.

**Q: The Debug Terminal shows different metrics than Task Manager. Why?**
A: Different measurement methods. Debug Terminal uses .NET `GC.GetTotalMemory()` (managed memory) while Task Manager shows process working set (includes unmanaged memory). Both are valid metrics for different purposes.

**Q: Can I schedule automatic diagnostic exports?**
A: Not currently supported via UI. Developers can implement scheduled export using `DiagnosticService.ExportDiagnostics()` in background service.

**Q: Why doesn't Force GC reduce memory significantly?**
A: Normal behavior. GC only reclaims memory from unreferenced objects. If memory is in active use (caches, collections, state), GC won't free it. Persistent high memory after GC may indicate necessary working set, not a leak.

---

## Technical Reference

### System Requirements

- **Platform**: Windows 10+, Linux (Ubuntu 20.04+), macOS 11+
- **Runtime**: .NET 9.0 or later
- **Memory**: Minimum 4GB RAM (application + Debug Terminal)
- **Disk Space**: 10MB for diagnostic exports (recommended)

### Performance Budgets

| Metric            | Budget      | Debug Terminal Overhead          |
| ----------------- | ----------- | -------------------------------- |
| Memory            | <100MB      | +65.8KB (0.065% of budget)       |
| CPU               | <2%         | <0.01% avg                       |
| Disk I/O          | Minimal     | Only during export (~500KB JSON) |
| UI Responsiveness | No blocking | Fully async, zero blocking       |

### Keyboard Shortcuts

| Shortcut     | Action               | Availability     |
| ------------ | -------------------- | ---------------- |
| Ctrl+Shift+D | Open Debug Terminal  | (if configured)  |
| Ctrl+Shift+E | Export Diagnostics   | (if configured)  |
| Ctrl+R       | Manual Refresh       | (if configured)  |
| Esc          | Close Debug Terminal | Always available |

### Related Documentation

- **Implementation Details**: `docs/DEBUG-TERMINAL-IMPLEMENTATION.md`
- **Performance Analysis**: `specs/003-debug-terminal-modernization/PERFORMANCE-OPTIMIZATION.md`
- **Constitutional Compliance**: `specs/003-debug-terminal-modernization/CONSTITUTIONAL-AUDIT.md`
- **AI Agent Patterns**: `AGENTS.md` (Section: Debug Terminal Diagnostics)
- **Technical Spec**: `specs/003-debug-terminal-modernization/SPEC_003-debug-terminal-modernization.md`
- **Implementation Plan**: `specs/003-debug-terminal-modernization/PLAN_003-debug-terminal-modernization.md`

### Support

**For Developers**:
- GitHub Issues: https://github.com/Dorotel/MTM_Avalonia_Template/issues
- Pull Requests: Feature 003 Debug Terminal branch
- Technical Questions: Review implementation docs first

**For Users**:
- Export diagnostics when reporting issues
- Include reproduction steps in support requests
- Check FAQ and Troubleshooting sections before contacting support

---

**Document Version**: 1.0.0
**Last Reviewed**: October 8, 2025
**Next Review**: January 2026 (or after major feature updates)
