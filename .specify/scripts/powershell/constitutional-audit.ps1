#!/usr/bin/env pwsh
# Constitutional Compliance Audit Script
# Performs automated constitutional compliance checks on a feature

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$FeatureId,

    [switch]$Json,

    [switch]$Commit,

    [switch]$Help
)

$ErrorActionPreference = 'Stop'

# Show help
if ($Help) {
    Write-Host @"
Constitutional Audit Script

Usage:
  ./constitutional-audit.ps1 [OPTIONS] [FEATURE_ID]

Arguments:
  FEATURE_ID    Feature directory name (e.g., 001-boot-sequence-splash)
                If omitted, uses current git branch or latest feature

Options:
  -Json         Output results in JSON format
  -Commit       Auto-commit audit results to git
  -Help         Show this help message

Examples:
  ./constitutional-audit.ps1
  ./constitutional-audit.ps1 001-boot-sequence-splash
  ./constitutional-audit.ps1 -Json 001-boot-sequence-splash
  ./constitutional-audit.ps1 -Commit 001-boot-sequence-splash
"@
    exit 0
}

# Source common functions
$commonScript = Join-Path $PSScriptRoot "common.ps1"
if (Test-Path $commonScript) {
    . $commonScript
}
else {
    Write-Error "Error: common.ps1 not found at $commonScript"
    exit 1
}

# Get repository root
$repoRoot = Get-RepoRoot
Set-Location $repoRoot

$specsDir = Join-Path $repoRoot "specs"
$templatesDir = Join-Path $repoRoot ".specify" "templates"
$templateFile = Join-Path $templatesDir "constitutional-audit.md"

# Verify template exists
if (-not (Test-Path $templateFile)) {
    Write-Error "Error: Constitutional audit template not found at $templateFile"
    exit 1
}

# Determine feature to audit
if ($FeatureId) {
    $featureName = $FeatureId
}
else {
    # Try to get from current branch
    $currentBranch = Get-CurrentBranch
    if ($currentBranch -match '(\d{3}-[\w-]+)') {
        $featureName = $matches[1]
    }
    else {
        # Fall back to latest feature
        $latestFeature = Get-ChildItem -Path $specsDir -Directory |
        Where-Object { $_.Name -match '^\d{3}-' } |
        Sort-Object Name -Descending |
        Select-Object -First 1

        if ($latestFeature) {
            $featureName = $latestFeature.Name
        }
        else {
            Write-Error "Error: No feature specified and cannot determine from branch or specs directory"
            exit 1
        }
    }
}

$featureDir = Join-Path $specsDir $featureName

# Verify feature directory exists
if (-not (Test-Path $featureDir)) {
    Write-Error "Error: Feature directory not found: $featureDir"
    exit 1
}

# Verify required files exist
$specFile = Join-Path $featureDir "spec.md"
$planFile = Join-Path $featureDir "plan.md"

if (-not (Test-Path $specFile)) {
    Write-Error "Error: spec.md not found in $featureDir"
    exit 1
}

if (-not (Test-Path $planFile)) {
    Write-Error "Error: plan.md not found in $featureDir"
    exit 1
}

# Generate audit filename with date
$auditDate = Get-Date -Format "yyyy-MM-dd"
$auditFileName = "AUDIT_${featureName}_${auditDate}.md"
$auditFile = Join-Path $featureDir $auditFileName

# Copy template to audit file
Copy-Item -Path $templateFile -Destination $auditFile -Force

# Read spec to get feature name
$specContent = Get-Content $specFile -Raw
$featureTitle = "Unknown Feature"
if ($specContent -match '(?m)^#\s+(?:Feature Specification:\s+)?(.+)$') {
    $featureTitle = $matches[1].Trim()
}

# Update audit file with feature metadata
$auditContent = Get-Content $auditFile -Raw
$auditContent = $auditContent -replace '\[FEATURE NAME\]', $featureTitle
$auditContent = $auditContent -replace '\[DATE\]', $auditDate
Set-Content -Path $auditFile -Value $auditContent -NoNewline

if (-not $Json) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  CONSTITUTIONAL COMPLIANCE AUDIT v1.1.0" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Feature:      " -NoNewline -ForegroundColor Yellow
    Write-Host $featureTitle
    Write-Host "Feature ID:   " -NoNewline -ForegroundColor Yellow
    Write-Host $featureName
    Write-Host "Audit Date:   " -NoNewline -ForegroundColor Yellow
    Write-Host $auditDate
    Write-Host "Audit File:   " -NoNewline -ForegroundColor Yellow
    Write-Host $auditFile
    Write-Host ""
    Write-Host "───────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "✅ Audit template created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Review the audit checklist in $auditFileName"
    Write-Host "  2. Use AI agent to analyze code against constitutional principles"
    Write-Host "  3. Document violations with specific file:line references"
    Write-Host "  4. Create remediation plan for any violations found"
    Write-Host "  5. Update summary section with totals and next steps"
    Write-Host ""
    Write-Host "AI Agent Guidance:" -ForegroundColor Cyan
    Write-Host "  • Read: .specify/templates/constitutional-audit.prompt"
    Write-Host "  • Analyze: All files mentioned in $featureName/plan.md"
    Write-Host "  • Focus: Critical violations (Principle VI - CompiledBinding)"
    Write-Host "  • Document: Specific file paths and line numbers"
    Write-Host ""
}

# Commit if requested
if ($Commit) {
    try {
        git add $auditFile
        git commit -m "constitutional-audit: Add compliance audit for $featureName"

        if (-not $Json) {
            Write-Host "✅ Audit file committed to git" -ForegroundColor Green
            Write-Host ""
        }
    }
    catch {
        if (-not $Json) {
            Write-Warning "Warning: Failed to commit audit file: $_"
        }
    }
}

# Output JSON if requested
if ($Json) {
    $result = @{
        "AUDIT_FILE"       = $auditFile.Replace($repoRoot, "").TrimStart('\', '/')
        "FEATURE_ID"       = $featureName
        "FEATURE_NAME"     = $featureTitle
        "AUDIT_DATE"       = $auditDate
        "TEMPLATE_CREATED" = $true
    }

    Write-Output ($result | ConvertTo-Json -Compress)
}
else {
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

exit 0
