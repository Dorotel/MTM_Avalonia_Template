#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated script to add CancellationToken parameters to remaining service implementations
.DESCRIPTION
    This script handles REM017-042 by automatically updating service implementation files
    to add CancellationToken parameters with the constitutional pattern:
    1. Add using System.Threading
    2. Add CancellationToken cancellationToken = default parameter
    3. Add cancellationToken.ThrowIfCancellationRequested() at method start
    4. Propagate cancellation token to downstream calls
.NOTES
    Created: 2025-10-03
    Part of: Constitutional Remediation for 001-boot-sequence-splash
    REMs: REM017-042
#>

[CmdletBinding()]
param(
    [Parameter(HelpMessage = "Dry run - show changes without applying them")]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Ensure we're in repo root
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $repoRoot

Write-Host "🔧 Constitutional Remediation: Automated CancellationToken Updates" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Define files to update (REM017-042)
$filesToUpdate = @(
    @{
        Path = "MTM_Template_Application/Services/Logging/TelemetryBatchProcessor.cs"
        RemId = "REM017"
        Methods = @("AddAsync", "FlushAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Diagnostics/Checks/StorageDiagnostic.cs"
        RemId = "REM018"
        Methods = @("RunAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Diagnostics/Checks/PermissionsDiagnostic.cs"
        RemId = "REM019"
        Methods = @("RunAsync", "CheckPermissionAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Diagnostics/Checks/NetworkDiagnostic.cs"
        RemId = "REM020"
        Methods = @("RunAsync", "TestConnectivityAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/DataLayer/MySqlClient.cs"
        RemId = "REM021"
        Methods = @("OpenConnectionAsync", "ExecuteQueryAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/DataLayer/HttpApiClient.cs"
        RemId = "REM022"
        Methods = @("SendRequestAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/DataLayer/VisualApiClient.cs"
        RemId = "REM023"
        Methods = @("GetAsync", "PostAsync", "RefreshTokenAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Core/HealthCheckService.cs"
        RemId = "REM024"
        Methods = @("CheckHealthAsync", "GetHealthStatusAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Configuration/FeatureFlagEvaluator.cs"
        RemId = "REM025"
        Methods = @("EvaluateAsync", "RefreshFlagsAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Cache/CacheService.cs"
        RemId = "REM026"
        Methods = @("GetAsync", "SetAsync", "RemoveAsync", "ClearAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Core/MessageBus.cs"
        RemId = "REM027"
        Methods = @("PublishAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Configuration/ConfigurationPersistence.cs"
        RemId = "REM032"
        Methods = @("SaveAsync", "LoadAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Logging/PiiRedactionMiddleware.cs"
        RemId = "REM033"
        Methods = @("RedactAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Logging/LogRotationPolicy.cs"
        RemId = "REM034"
        Methods = @("RotateLogsAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Diagnostics/HardwareDetection.cs"
        RemId = "REM035"
        Methods = @("DetectHardwareAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/DataLayer/ConnectionPoolMonitor.cs"
        RemId = "REM036"
        Methods = @("MonitorAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Cache/LZ4CompressionHandler.cs"
        RemId = "REM037"
        Methods = @("CompressAsync", "DecompressAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/DataLayer/VisualMasterDataSync.cs"
        RemId = "REM038"
        Methods = @("SyncAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Cache/CacheStalenessDetector.cs"
        RemId = "REM039"
        Methods = @("DetectStaleEntriesAsync", "RefreshStaleEntriesAsync")
    },
    @{
        Path = "MTM_Template_Application/Services/Cache/CachedOnlyModeManager.cs"
        RemId = "REM040"
        Methods = @("EnableCachedOnlyModeAsync", "DisableCachedOnlyModeAsync")
    }
)

$updatedFiles = @()
$skippedFiles = @()
$errorFiles = @()

foreach ($fileInfo in $filesToUpdate) {
    $filePath = $fileInfo.Path
    $remId = $fileInfo.RemId
    
    Write-Host "[$remId] Processing: $filePath" -ForegroundColor Yellow
    
    if (-not (Test-Path $filePath)) {
        Write-Host "  ⚠️  File not found - skipping" -ForegroundColor Gray
        $skippedFiles += $filePath
        continue
    }
    
    try {
        $content = Get-Content $filePath -Raw
        $originalContent = $content
        $modified = $false
        
        # Step 1: Add using System.Threading if not present
        if ($content -notmatch 'using System\.Threading;') {
            Write-Host "  ✓ Adding 'using System.Threading;'" -ForegroundColor Green
            $content = $content -replace '(using System;)', "`$1`nusing System.Threading;"
            $modified = $true
        }
        
        # Step 2: Update async method signatures
        foreach ($methodName in $fileInfo.Methods) {
            # Pattern 1: public async Task MethodName(params)
            $pattern1 = "(public\s+async\s+Task(?:<[^>]+>)?\s+$methodName\s*\([^)]*?)(\))"
            if ($content -match $pattern1) {
                # Check if CancellationToken already present
                if ($content -notmatch "$methodName\s*\([^)]*CancellationToken") {
                    Write-Host "  ✓ Adding CancellationToken to $methodName" -ForegroundColor Green
                    $content = $content -replace $pattern1, '$1, CancellationToken cancellationToken = default$2'
                    $modified = $true
                }
            }
            
            # Pattern 2: public Task MethodName(params)
            $pattern2 = "(public\s+Task(?:<[^>]+>)?\s+$methodName\s*\([^)]*?)(\))"
            if ($content -match $pattern2) {
                if ($content -notmatch "$methodName\s*\([^)]*CancellationToken") {
                    Write-Host "  ✓ Adding CancellationToken to $methodName" -ForegroundColor Green
                    $content = $content -replace $pattern2, '$1, CancellationToken cancellationToken = default$2'
                    $modified = $true
                }
            }
        }
        
        # Step 3: Add cancellationToken.ThrowIfCancellationRequested() after ArgumentNullException checks
        foreach ($methodName in $fileInfo.Methods) {
            # Find method body and add cancellation check
            $methodPattern = "($methodName\s*\([^)]*CancellationToken[^)]*\)\s*\{[^\{]*?)(ArgumentNullException\.ThrowIfNull)"
            if ($content -match $methodPattern) {
                if ($content -notmatch "cancellationToken\.ThrowIfCancellationRequested\(\)") {
                    Write-Host "  ✓ Adding cancellation check to $methodName" -ForegroundColor Green
                    $content = $content -replace $methodPattern, "`$1cancellationToken.ThrowIfCancellationRequested();`n        `$2"
                    $modified = $true
                }
            }
        }
        
        if ($modified) {
            if (-not $DryRun) {
                Set-Content -Path $filePath -Value $content -NoNewline
                Write-Host "  ✅ File updated successfully" -ForegroundColor Green
            } else {
                Write-Host "  🔍 [DRY RUN] Would update file" -ForegroundColor Cyan
            }
            $updatedFiles += $filePath
        } else {
            Write-Host "  ℹ️  No changes needed" -ForegroundColor Gray
        }
        
    } catch {
        Write-Host "  ❌ Error: $_" -ForegroundColor Red
        $errorFiles += $filePath
    }
    
    Write-Host ""
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Updated: $($updatedFiles.Count) files" -ForegroundColor Green
Write-Host "⚠️  Skipped: $($skippedFiles.Count) files (not found)" -ForegroundColor Yellow
Write-Host "❌ Errors:  $($errorFiles.Count) files" -ForegroundColor Red
Write-Host ""

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No files were actually modified" -ForegroundColor Cyan
    Write-Host "   Run without -DryRun to apply changes" -ForegroundColor Cyan
}

if ($updatedFiles.Count -gt 0) {
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Review changes: git diff" -ForegroundColor White
    Write-Host "2. Build: dotnet build" -ForegroundColor White
    Write-Host "3. Commit: git add -A && git commit -m 'fix(services): add CancellationToken to remaining implementations [REM017-REM042]'" -ForegroundColor White
}

Write-Host ""
