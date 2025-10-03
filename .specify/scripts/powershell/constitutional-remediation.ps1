<#
.SYNOPSIS
    Constitutional Remediation - Generate actionable fix checklist from audit violations

.DESCRIPTION
    Reads constitutional audit results and generates a remediation checklist with:
    - REM### items for each violation
    - Links to tasks.md
    - Before/After code patterns
    - Verification commands
    - Git commit suggestions

.PARAMETER FeatureId
    Feature ID (e.g., "001-boot-sequence-splash")

.PARAMETER Json
    Output results as JSON for scripting

.PARAMETER Status
    Check remediation progress without generating new file

.PARAMETER Complete
    Mark specific REM item as complete

.EXAMPLE
    .\constitutional-remediation.ps1 001-boot-sequence-splash
    Generate remediation checklist

.EXAMPLE
    .\constitutional-remediation.ps1 -Json 001-boot-sequence-splash
    Generate with JSON output

.EXAMPLE
    .\constitutional-remediation.ps1 -Status 001-boot-sequence-splash
    Check progress

.EXAMPLE
    .\constitutional-remediation.ps1 -Complete REM001 001-boot-sequence-splash
    Mark REM001 as complete
#>

param(
    [Parameter(Position = 0)]
    [string]$FeatureId,

    [switch]$Json,
    [switch]$Status,
    [string]$Complete,
    [switch]$Help
)

# Show help if requested
if ($Help) {
    Get-Help $PSCommandPath -Detailed
    exit 0
}

# Repository root
$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$specsDir = Join-Path $repoRoot "specs"

# Detect feature ID from branch if not provided
if (-not $FeatureId) {
    try {
        $branch = git rev-parse --abbrev-ref HEAD 2>$null
        if ($branch -match '(\d{3}-[\w-]+)') {
            $FeatureId = $matches[1]
            Write-Host "🔍 Detected feature from branch: $FeatureId" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "❌ ERROR: No feature specified and couldn't detect from branch" -ForegroundColor Red
        Write-Host "Usage: .\constitutional-remediation.ps1 <feature-id>" -ForegroundColor Yellow
        exit 1
    }
}

$featureDir = Join-Path $specsDir $FeatureId

# Validate feature exists
if (-not (Test-Path $featureDir)) {
    Write-Host "❌ ERROR: Feature directory not found: $featureDir" -ForegroundColor Red
    exit 1
}

# Find latest audit file
$auditFiles = Get-ChildItem -Path $featureDir -Filter "AUDIT_*.md" | Sort-Object Name -Descending
if ($auditFiles.Count -eq 0) {
    Write-Host "❌ ERROR: No audit found for feature $FeatureId" -ForegroundColor Red
    Write-Host "Run constitutional-audit.ps1 first to generate audit" -ForegroundColor Yellow
    exit 1
}

$latestAudit = $auditFiles[0]
$auditPath = $latestAudit.FullName

# Check if status request
if ($Status) {
    $remediationFiles = Get-ChildItem -Path $featureDir -Filter "REMEDIATION_*.md" | Sort-Object Name -Descending
    if ($remediationFiles.Count -eq 0) {
        Write-Host "⚠️  No remediation file found" -ForegroundColor Yellow
        exit 0
    }

    $remediationPath = $remediationFiles[0].FullName
    $content = Get-Content $remediationPath -Raw

    # Count items
    $totalItems = ([regex]::Matches($content, '- \[ \] REM\d+')).Count + ([regex]::Matches($content, '- \[x\] REM\d+')).Count
    $completedItems = ([regex]::Matches($content, '- \[x\] REM\d+')).Count
    $percentage = if ($totalItems -gt 0) { [math]::Round(($completedItems / $totalItems) * 100) } else { 0 }

    $statusText = if ($percentage -eq 0) { "NOT_STARTED" }
    elseif ($percentage -eq 100) { "COMPLETE" }
    else { "IN_PROGRESS" }

    if ($Json) {
        @{
            REMEDIATION_FILE      = $remediationPath
            AUDIT_FILE            = $auditPath
            TOTAL_ITEMS           = $totalItems
            COMPLETED_ITEMS       = $completedItems
            COMPLETION_PERCENTAGE = $percentage
            STATUS                = $statusText
        } | ConvertTo-Json
    }
    else {
        Write-Host "📊 Remediation Progress for $FeatureId" -ForegroundColor Cyan
        Write-Host "Status: $statusText ($completedItems / $totalItems items, $percentage%)" -ForegroundColor $(if ($percentage -eq 100) { 'Green' } else { 'Yellow' })
        Write-Host "File: $remediationPath" -ForegroundColor Gray
    }
    exit 0
}

# Check if complete request
if ($Complete) {
    $remediationFiles = Get-ChildItem -Path $featureDir -Filter "REMEDIATION_*.md" | Sort-Object Name -Descending
    if ($remediationFiles.Count -eq 0) {
        Write-Host "❌ ERROR: No remediation file found" -ForegroundColor Red
        exit 1
    }

    $remediationPath = $remediationFiles[0].FullName
    $content = Get-Content $remediationPath -Raw

    # Mark item complete
    $updated = $content -replace "- \[ \] $Complete", "- [x] $Complete"
    if ($updated -eq $content) {
        Write-Host "⚠️  Item $Complete not found or already complete" -ForegroundColor Yellow
    }
    else {
        Set-Content -Path $remediationPath -Value $updated -NoNewline
        Write-Host "✅ Marked $Complete as complete" -ForegroundColor Green
    }
    exit 0
}

# Read audit file
$auditContent = Get-Content $auditPath -Raw

# Check if audit is already compliant
if ($auditContent -match 'Status.*✅.*COMPLIANT' -and $auditContent -notmatch 'NON-COMPLIANT') {
    Write-Host "⏭️  Feature $FeatureId is already COMPLIANT - no remediation needed" -ForegroundColor Green
    if ($Json) {
        @{
            REMEDIATION_FILE   = $null
            AUDIT_FILE         = $auditPath
            TOTAL_ITEMS        = 0
            BLOCKING_ITEMS     = 0
            NON_BLOCKING_ITEMS = 0
            STATUS             = "SKIPPED"
            MESSAGE            = "Audit shows no violations"
        } | ConvertTo-Json
    }
    exit 0
}

# Extract feature name
$featureName = if ($auditContent -match '\*\*Feature\*\*:\s*(.+)') { $matches[1].Trim() } else { $FeatureId }

# Extract violations from "Next Steps" section
$nextStepsMatch = [regex]::Match($auditContent, '(?s)## 🎯 Next Steps.*?(?=^## )', [System.Text.RegularExpressions.RegexOptions]::Multiline)
if (-not $nextStepsMatch.Success) {
    Write-Host "⚠️  No 'Next Steps' section found in audit" -ForegroundColor Yellow
    Write-Host "Audit may be in old format. Please re-run constitutional-audit." -ForegroundColor Yellow
    exit 1
}

$nextStepsText = $nextStepsMatch.Value

# Count violations
$blockingCount = ([regex]::Matches($nextStepsText, '(?m)^###+ \d+\. \*\*\[CRITICAL')).Count
$nonBlockingCount = ([regex]::Matches($nextStepsText, '(?m)^###+ \d+\. \*\*\[')).Count - $blockingCount
$totalCount = $blockingCount + $nonBlockingCount

# Extract time estimates
$timeEstimate = if ($auditContent -match 'Total.*?(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)\s*hours') {
    $matches[2]
}
elseif ($auditContent -match 'Total.*?(\d+(?:\.\d+)?)\s*hours') {
    $matches[1]
}
else {
    "Unknown"
}

# Generate remediation filename
$date = Get-Date -Format "yyyy-MM-dd"
$remediationFilename = "REMEDIATION_${FeatureId}_${date}.md"
$remediationPath = Join-Path $featureDir $remediationFilename

# Check if file already exists today
if (Test-Path $remediationPath) {
    Write-Host "⚠️  Remediation file already exists: $remediationFilename" -ForegroundColor Yellow
    $response = Read-Host "Overwrite? (y/N)"
    if ($response -ne 'y') {
        Write-Host "Cancelled" -ForegroundColor Gray
        exit 0
    }
}

# Load template
$templatePath = Join-Path $repoRoot ".specify" "templates" "constitutional-remediation.md"
if (-not (Test-Path $templatePath)) {
    Write-Host "❌ ERROR: Template not found: $templatePath" -ForegroundColor Red
    exit 1
}

$template = Get-Content $templatePath -Raw

# Fill in template metadata
$remediation = $template `
    -replace '\[FEATURE NAME\]', $featureName `
    -replace '\[DATE\]', $date `
    -replace '\[AUDIT_FILE_NAME\]', $latestAudit.Name `
    -replace '\[TOTAL\]', $totalCount `
    -replace '\[BLOCKING_COUNT\]', $blockingCount `
    -replace '\[NON_BLOCKING_COUNT\]', $nonBlockingCount

# TODO: Parse violations and generate detailed REM items
# For now, include a placeholder that directs to manual population
$placeholder = @"

---

**⚠️ MANUAL POPULATION REQUIRED**

This remediation checklist template has been generated. The AI agent should now:

1. Read the audit file: $($latestAudit.Name)
2. Extract each violation from the "Next Steps" section
3. For each violation, create a REM### item with:
   - File path and line number
   - Before/After code snippets (from audit details)
   - Verification command
   - Link to related task in tasks.md
   - Git commit message suggestion

4. Replace the sections below with actual REM items.

**Audit Summary**:
- BLOCKING violations: $blockingCount
- Non-Blocking violations: $nonBlockingCount
- Total: $totalCount
- Estimated time: $timeEstimate hours

**Next**: AI agent should populate this file with specific REM items from the audit.

---
"@

# Insert placeholder after metadata
$remediation = $remediation -replace '(?s)(---\s*## 📋 Quick Reference)', "$placeholder`$1"

# Save remediation file
Set-Content -Path $remediationPath -Value $remediation -NoNewline

# Output results
if ($Json) {
    @{
        REMEDIATION_FILE      = $remediationPath
        AUDIT_FILE            = $auditPath
        TOTAL_ITEMS           = $totalCount
        BLOCKING_ITEMS        = $blockingCount
        NON_BLOCKING_ITEMS    = $nonBlockingCount
        COMPLETED_ITEMS       = 0
        COMPLETION_PERCENTAGE = 0
        STATUS                = "NOT_STARTED"
        ESTIMATED_HOURS       = $timeEstimate
    } | ConvertTo-Json
}
else {
    Write-Host ""
    Write-Host "✅ Remediation checklist created" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 File: $remediationPath" -ForegroundColor Cyan
    Write-Host "📊 Items: $totalCount ($blockingCount BLOCKING, $nonBlockingCount non-blocking)" -ForegroundColor Cyan
    Write-Host "⏱️  Estimated time: $timeEstimate hours" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🤖 Next: AI agent should populate REM items from audit violations" -ForegroundColor Yellow
    Write-Host ""
}

exit 0
