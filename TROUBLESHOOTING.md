# Troubleshooting Guide: Boot Sequence Issues

**Last Updated**: 2025-10-04
**Feature**: 001-boot-sequence-splash

This guide helps diagnose and resolve common boot sequence issues.

## Table of Contents

1. [Boot Failures](#boot-failures)
2. [Performance Issues](#performance-issues)
3. [Configuration Problems](#configuration-problems)
4. [Credential Issues](#credential-issues)
5. [Network and Connectivity](#network-and-connectivity)
6. [Cache Problems](#cache-problems)
7. [Platform-Specific Issues](#platform-specific-issues)
8. [Diagnostic Tools](#diagnostic-tools)

---

## Boot Failures

### Issue: Splash screen doesn't appear

**Symptoms**:
- Application starts but no window is visible
- Process runs in background but no UI

**Possible Causes**:
1. Graphics driver incompatibility
2. Display configuration issues
3. Avalonia rendering failure

**Solutions**:

```powershell
# Enable console logging to diagnose
.\MTM_Template_Application.exe --LogLevel Debug --console

# Check graphics capabilities
.\MTM_Template_Application.exe --diagnostics

# Try software rendering fallback
$env:AVALONIA_RENDERER = "software"
.\MTM_Template_Application.exe
```

**Prevention**:
- Keep graphics drivers updated
- Test on target hardware during development

---

### Issue: Stage 1 timeout

**Symptoms**:
- Boot hangs during "Services initialization"
- Error: "Stage 1 exceeded 60s timeout"
- Application exits with timeout error

**Possible Causes**:
1. Network connectivity issues (trying to reach Visual server)
2. MySQL server not responding
3. Service deadlock
4. Insufficient system resources

**Solutions**:

```powershell
# Check network connectivity
Test-NetConnection visual-server.company.com -Port 80

# Verify MySQL is running
Get-Service MySQL* | Where-Object {$_.Status -eq "Running"}

# Check system resources
Get-Counter '\Memory\Available MBytes'
Get-Counter '\Processor(_Total)\% Processor Time'

# Increase timeout for slow networks
$env:MTM_BOOT_TIMEOUT_STAGE1 = "120"
.\MTM_Template_Application.exe
```

**Prevention**:
- Ensure Visual server is reachable before starting
- Start MySQL service before application
- Close resource-intensive applications

---

### Issue: "Previous session ended unexpectedly"

**Symptoms**:
- Warning message on startup
- Offer to run in Safe Mode
- Lock file present from previous run

**Possible Causes**:
1. Application crashed during previous session
2. Forced termination (Task Manager)
3. System shutdown during runtime

**Solutions**:

```powershell
# Remove lock file manually
Remove-Item -Path ".\app.lock" -Force

# Check crash logs
Get-Content ".\logs\application-*.log" | Select-String -Pattern "ERROR|CRITICAL"

# Start in Safe Mode
.\MTM_Template_Application.exe --safe-mode
```

**Prevention**:
- Use proper shutdown procedures (File > Exit)
- Don't force-kill the application
- Report crashes with diagnostic bundles

---

## Performance Issues

### Issue: Boot time exceeds 10 seconds

**Symptoms**:
- Slow startup
- Stage 1 takes >3 seconds
- Users complain about wait times

**Diagnostics**:

```powershell
# Enable boot metrics
$env:MTM_BOOT_METRICS_ENABLED = "true"
.\MTM_Template_Application.exe

# View boot metrics log
Get-Content ".\logs\boot-metrics.log"
```

**Common Causes**:

| Stage | Bottleneck | Solution |
|-------|------------|----------|
| Stage 1 | Cache population | Reduce cache size or enable lazy loading |
| Stage 1 | Network latency | Use cached-only mode or optimize queries |
| Stage 1 | Diagnostics | Disable non-critical health checks |
| Stage 2 | Theme loading | Pre-compile resources |

**Solutions**:

```powershell
# Skip cache pre-population for faster startup
$env:MTM_CACHE_LAZY_LOAD = "true"

# Disable slow diagnostics
$env:MTM_DIAGNOSTICS_SKIP = "Network,Hardware"

# Reduce log verbosity
.\MTM_Template_Application.exe --LogLevel Warning
```

**Prevention**:
- Profile boot sequence regularly
- Optimize service initialization order
- Cache resources for faster subsequent startups

---

### Issue: High memory usage during boot

**Symptoms**:
- Memory usage >100MB during startup
- System becomes sluggish
- Out of memory errors on low-RAM devices

**Diagnostics**:

```powershell
# Run performance tests with memory profiling
dotnet test --filter "Performance_MemoryUsageShouldBeLessThan100MB"

# Monitor memory in real-time
while ($true) {
    Get-Process MTM_Template_Application | Select-Object WorkingSet64
    Start-Sleep -Seconds 1
}
```

**Solutions**:

```powershell
# Disable cache compression (uses more CPU, less RAM)
$env:MTM_CACHE_COMPRESSION = "false"

# Reduce cache size
$env:MTM_CACHE_MAX_ENTRIES = "1000"

# Enable aggressive garbage collection
$env:DOTNET_gcServer = "false"
```

**Expected Memory Breakdown**:
- Cache: ~40MB (compressed with LZ4)
- Core Services: ~30MB (DI container, logging, pools)
- Framework: ~30MB (Avalonia UI, .NET runtime)
- **Total: <100MB**

---

## Configuration Problems

### Issue: "Configuration file is invalid"

**Symptoms**:
- Boot fails during Stage 1
- Error: "Invalid JSON in config.base.json"
- Application offers to reset to defaults

**Solutions**:

```powershell
# Validate JSON syntax
Get-Content ".\config.base.json" | ConvertFrom-Json

# Check for common JSON errors
# - Missing commas
# - Trailing commas
# - Unescaped quotes
# - Incorrect braces

# Reset to defaults
.\MTM_Template_Application.exe --reset-config

# Use validated sample config
Copy-Item ".\config.base.sample.json" -Destination ".\config.base.json"
```

---

### Issue: Environment variable overrides not working

**Symptoms**:
- Set `$env:MTM_LOG_LEVEL = "Debug"` but application uses default
- Configuration precedence seems incorrect

**Diagnostics**:

```powershell
# Verify environment variable is set
Get-ChildItem Env:MTM_*

# Check configuration audit log
Get-Content ".\logs\config-audit.log" | Select-String -Pattern "Environment"
```

**Solution**:

Ensure environment variable naming follows convention:
- `MTM_` prefix
- Section separator: `__` (double underscore)
- Example: `MTM_Database__Timeout`

```powershell
# ❌ WRONG
$env:MTM_DATABASE_TIMEOUT = "60"

# ✅ CORRECT
$env:MTM_Database__Timeout = "60"
```

---

## Credential Issues

### Issue: "Authentication failed - please verify your credentials"

**Symptoms**:
- Can't connect to Visual server
- Stage 1 fails during data layer initialization
- Retry/Exit dialog appears

**Solutions**:

```powershell
# Re-enter credentials
.\MTM_Template_Application.exe --setup-credentials

# Verify credentials are stored
cmdkey /list | Select-String -Pattern "MTM.Template"

# Test connection manually
Test-NetConnection visual-server.company.com -Port 80

# Check credential expiration
# (Visual credentials may expire after 90 days)
```

**Prevention**:
- Use service accounts with no expiration
- Enable credential rotation notifications
- Test credentials before each boot

---

### Issue: Credentials missing after Windows update

**Symptoms**:
- Previously working credentials no longer found
- Windows Credential Manager shows empty entries

**Cause**:
Windows updates can sometimes clear or corrupt DPAPI-encrypted credentials.

**Solutions**:

```powershell
# Re-enter credentials
.\MTM_Template_Application.exe --setup-credentials

# Backup credentials (encrypted)
Export-Credential -Path ".\credentials.backup"

# Restore from backup
Import-Credential -Path ".\credentials.backup"
```

**Prevention**:
- Export credentials before major Windows updates
- Use enterprise credential management if available

---

## Network and Connectivity

### Issue: "Visual server unavailable - using cached data"

**Symptoms**:
- Warning banner at top of screen
- Cache age indicator shows stale data
- Write operations are blocked

**Diagnostics**:

```powershell
# Test Visual server connectivity
Test-NetConnection visual-server.company.com -Port 80

# Check DNS resolution
Resolve-DnsName visual-server.company.com

# Verify firewall rules
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*Visual*"}

# Test with curl
curl -v http://visual-server.company.com/api/health
```

**Solutions**:

1. **If Visual server is down**: Work in cached-only mode
2. **If network is down**: Check physical connections, WiFi
3. **If firewall blocking**: Add exception for Visual server
4. **If VPN required**: Connect to VPN before starting app

**Manual Reconnection**:

```powershell
# Retry connection manually
# Click "Retry Now" button in warning banner

# Or restart application
Stop-Process -Name MTM_Template_Application
Start-Process .\MTM_Template_Application.exe
```

---

### Issue: Circuit breaker opened

**Symptoms**:
- Error: "Circuit breaker opened - Visual service unavailable"
- Retry countdown visible: "Retrying in 30 seconds..."
- Multiple consecutive failures

**Cause**:
Circuit breaker opens after 5 consecutive failures to protect against cascading failures.

**Exponential Backoff Schedule**:
1. 30 seconds
2. 1 minute
3. 2 minutes
4. 5 minutes
5. 10 minutes (maximum)

**Solutions**:

```powershell
# Wait for automatic retry
# Circuit breaker will attempt recovery

# Manual retry (resets circuit breaker)
# Click "Retry Now" button

# Check Visual server status
# Contact server administrator if circuit breaker keeps opening
```

---

## Cache Problems

### Issue: Cache corruption

**Symptoms**:
- Error: "Failed to decompress cache entry"
- Application crashes when loading cached data
- Cache statistics show invalid data

**Solutions**:

```powershell
# Clear cache
Remove-Item -Path ".\cache\*" -Recurse -Force

# Rebuild cache on next startup
$env:MTM_CACHE_REBUILD = "true"
.\MTM_Template_Application.exe

# Verify cache integrity
.\MTM_Template_Application.exe --verify-cache
```

**Prevention**:
- Don't manually edit cache files
- Allow graceful shutdown (cache flush)
- Use UPS to prevent power interruptions

---

### Issue: Cache not updating

**Symptoms**:
- Data is stale but Visual server is reachable
- Cache age indicator shows old timestamp
- Manual refresh doesn't work

**Diagnostics**:

```powershell
# Check cache staleness settings
Get-Content ".\config.base.json" | ConvertFrom-Json | Select-Object -ExpandProperty Cache

# View cache refresh log
Get-Content ".\logs\cache-refresh.log" | Select-String -Pattern "Parts|Locations|Warehouses"

# Check TTL configuration
# Parts: 24 hours (default)
# Others: 7 days (default)
```

**Solutions**:

```powershell
# Force full cache refresh
$env:MTM_CACHE_FORCE_REFRESH = "true"
.\MTM_Template_Application.exe

# Reduce TTL for more frequent updates
$env:MTM_Cache__PartsTTL = "12h"
$env:MTM_Cache__LocationsTTL = "3d"
```

---

## Platform-Specific Issues

### Windows

**Issue**: "DPAPI encryption failed"

**Solution**:
```powershell
# Run as administrator
Start-Process powershell -Verb RunAs

# Repair user profile
sfc /scannow

# Check DPAPI master key
certutil -store my
```

---

### macOS

**Issue**: "Keychain access denied"

**Solution**:
```bash
# Grant keychain access
security unlock-keychain ~/Library/Keychains/login.keychain

# Reset keychain if corrupted
security delete-keychain ~/Library/Keychains/login.keychain
security create-keychain -p password ~/Library/Keychains/login.keychain
```

---

### Android

**Issue**: "KeyStore not initialized"

**Solution**:
```bash
# Verify KeyStore
adb shell ls /data/system/users/0/keystore*

# Clear KeyStore (will require re-entering credentials)
adb shell pm clear com.android.keychain

# Reinstall application
adb install -r MTM_Template_Application.apk
```

---

## Diagnostic Tools

### Generate Diagnostic Bundle

When reporting issues, always include a diagnostic bundle:

```powershell
# Method 1: Through UI
# 1. When error occurs, click "Generate Diagnostic Bundle"
# 2. Bundle saved to .\diagnostics\bundle-{timestamp}.zip

# Method 2: Command line
.\MTM_Template_Application.exe --generate-diagnostic-bundle
```

**Bundle Contents**:
- Recent logs (last 7 days, redacted)
- Boot metrics
- Configuration (secrets redacted)
- System information
- Error details

### Enable Verbose Logging

```powershell
# Debug-level logging to file
.\MTM_Template_Application.exe --LogLevel Debug

# Trace-level logging to console
.\MTM_Template_Application.exe --LogLevel Trace --console

# Log specific categories
.\MTM_Template_Application.exe --LogCategory Boot,Cache,DataLayer
```

### Performance Profiling

```powershell
# Run performance tests
dotnet test --filter "Category=Performance"

# Profile memory usage
dotnet run --project .\src\MTM_Template_Application.Desktop --memory-profile

# Profile boot sequence
dotnet run --project .\src\MTM_Template_Application.Desktop --boot-profile
```

### Health Check

```powershell
# Run all diagnostic checks
.\MTM_Template_Application.exe --diagnostics

# Run specific check
.\MTM_Template_Application.exe --diagnostics --check Storage
.\MTM_Template_Application.exe --diagnostics --check Network
.\MTM_Template_Application.exe --diagnostics --check Permissions
```

---

## Getting Help

### Before Reporting Issues

1. ✅ Check this troubleshooting guide
2. ✅ Review logs in `.\logs\` directory
3. ✅ Generate diagnostic bundle
4. ✅ Note exact error message and steps to reproduce

### Reporting Issues

Include the following information:

- **Environment**: Windows/macOS/Linux/Android version
- **Application Version**: Check Help > About
- **.NET Runtime**: Run `dotnet --info`
- **Diagnostic Bundle**: Attach bundle ZIP
- **Steps to Reproduce**: Detailed steps
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Screenshots**: If relevant

### Contact

- **GitHub Issues**: [MTM_Avalonia_Template/issues](https://github.com/Dorotel/MTM_Avalonia_Template/issues)
- **Author**: John Koll (@Dorotel)

---

**Note**: This guide covers Feature 001 (Boot Sequence) issues. For other features, see feature-specific troubleshooting guides.
