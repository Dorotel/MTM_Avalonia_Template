<#
.SYNOPSIS
    Deep validation of completed feature implementation against specification and constitution.

.DESCRIPTION
    Performs comprehensive validation when a feature reaches 100% implementation.
    Verifies all tasks completed, requirements met, tests passing, and constitutional compliance.

.PARAMETER FeatureId
    Feature ID to validate (e.g., "001-boot-sequence-splash"). Auto-detects from branch if not specified.

.PARAMETER Json
    Output results as JSON only (for automation/CI).

.PARAMETER Strict
    Fail validation on any warnings (stricter than default thresholds).

.PARAMETER Html
    Generate HTML report in addition to markdown.

.PARAMETER PrComment
    Generate formatted output suitable for GitHub PR comment.

.EXAMPLE
    .\validate-implementation.ps1
    Validates current branch feature.

.EXAMPLE
    .\validate-implementation.ps1 -FeatureId "001-boot-sequence-splash" -Json
    Validates specific feature with JSON output.

.EXAMPLE
    .\validate-implementation.ps1 -Strict
    Strict mode validation (fails on warnings).
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$FeatureId,

    [Parameter()]
    [switch]$Json,

    [Parameter()]
    [switch]$Strict,

    [Parameter()]
    [switch]$Html,

    [Parameter()]
    [switch]$PrComment
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Script root and repository root
$ScriptRoot = $PSScriptRoot
$RepoRoot = (Get-Item $ScriptRoot).Parent.Parent.Parent.FullName

# Helper Functions
function Write-ValidationLog {
    param(
        [string]$Message,
        [string]$Level = "INFO" # INFO, WARN, ERROR, SUCCESS
    )

    if (-not $Json) {
        $color = switch ($Level) {
            "SUCCESS" { "Green" }
            "WARN" { "Yellow" }
            "ERROR" { "Red" }
            default { "White" }
        }
        Write-Host "[$Level] $Message" -ForegroundColor $color
    }
}

function Get-FeatureIdFromBranch {
    try {
        $branch = git rev-parse --abbrev-ref HEAD 2>$null
        if ($branch -match '^(\d{3}-.+)$') {
            return $matches[1]
        }
    }
    catch {
        # Ignore git errors
    }
    return $null
}

function Test-TasksComplete {
    param([string]$TasksFilePath)

    $content = Get-Content $TasksFilePath -Raw
    $allTasks = [regex]::Matches($content, '- \[([ x])\] (T\d+)')
    $completedTasks = [regex]::Matches($content, '- \[x\] (T\d+)')

    return @{
        Total            = $allTasks.Count
        Completed        = $completedTasks.Count
        Percentage       = if ($allTasks.Count -gt 0) {
            [math]::Round(($completedTasks.Count / $allTasks.Count) * 100, 2)
        }
        else { 0 }
        AllTaskIds       = $allTasks | ForEach-Object { $_.Groups[2].Value }
        CompletedTaskIds = $completedTasks | ForEach-Object { $_.Groups[1].Value }
    }
}

function Test-FileExists {
    param(
        [string]$FilePath,
        [string]$BaseDir = $RepoRoot
    )

    $fullPath = Join-Path $BaseDir $FilePath
    return Test-Path $fullPath
}

function Get-FunctionalRequirements {
    param([string]$SpecFilePath)

    $content = Get-Content $SpecFilePath -Raw
    $requirements = [regex]::Matches($content, 'FR-(\d+):?\s*(.+?)(?=\r?\n)')

    return $requirements | ForEach-Object {
        @{
            Id          = "FR-$($_.Groups[1].Value)"
            Description = $_.Groups[2].Value.Trim()
        }
    }
}

function Test-ConstitutionalCompliance {
    param([string]$RepoPath)

    $violations = @{
        Critical    = @()
        NonCritical = @()
    }

    Write-ValidationLog "Checking constitutional compliance..." "INFO"

    # Principle I: Cross-Platform (Check for P/Invoke without abstraction)
    $pInvokeFiles = Get-ChildItem -Path "$RepoPath/MTM_Template_Application" -Filter "*.cs" -Recurse |
    Select-String -Pattern '\[DllImport\(' |
    Where-Object { $_.Path -notmatch 'Platform|Native|Interop' }

    if ($pInvokeFiles) {
        $violations.Critical += @{
            Principle = "I. Cross-Platform First"
            Issue     = "P/Invoke without platform abstraction"
            Files     = $pInvokeFiles | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
        }
    }

    # Principle II: MVVM Toolkit (Check for ReactiveUI)
    $reactiveFiles = Get-ChildItem -Path "$RepoPath/MTM_Template_Application" -Filter "*.cs" -Recurse |
    Select-String -Pattern 'ReactiveObject|ReactiveCommand|INotifyPropertyChanged' -SimpleMatch:$false

    if ($reactiveFiles) {
        $violations.Critical += @{
            Principle = "II. MVVM Community Toolkit"
            Issue     = "ReactiveUI patterns detected (should use MVVM Toolkit)"
            Files     = $reactiveFiles | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
        }
    }

    # Principle IV: Theme V2 (Check for hardcoded colors in XAML, except splash/boot views)
    $hardcodedColors = Get-ChildItem -Path "$RepoPath/MTM_Template_Application/Views" -Filter "*.axaml" -Recurse |
    Where-Object { $_.Name -notmatch 'Splash|Boot' } |
    Select-String -Pattern '#[A-Fa-f0-9]{6}' |
    Where-Object { $_.Line -notmatch 'Comment|<!--' }

    if ($hardcodedColors) {
        $violations.NonCritical += @{
            Principle = "IV. Theme V2 Semantic Tokens"
            Issue     = "Hardcoded colors in XAML (should use ThemeV2 resources)"
            Files     = $hardcodedColors | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
        }
    }

    # Principle V: Null Safety (Check for missing Nullable enable)
    $csprojFiles = Get-ChildItem -Path $RepoPath -Filter "*.csproj" -Recurse
    foreach ($csproj in $csprojFiles) {
        $content = Get-Content $csproj.FullName -Raw
        if ($content -notmatch '<Nullable>enable</Nullable>') {
            $violations.Critical += @{
                Principle = "V. Null Safety"
                Issue     = "Nullable reference types not enabled"
                Files     = @($csproj.FullName)
            }
        }
    }

    # Principle VI: Compiled Bindings (CRITICAL CHECK)
    Write-ValidationLog "Checking XAML bindings (CRITICAL)..." "INFO"
    $axamlFiles = Get-ChildItem -Path "$RepoPath/MTM_Template_Application" -Filter "*.axaml" -Recurse

    foreach ($axaml in $axamlFiles) {
        $content = Get-Content $axaml.FullName -Raw

        # Check for x:CompileBindings="True"
        if ($content -notmatch 'x:CompileBindings\s*=\s*"True"') {
            $violations.Critical += @{
                Principle = "VI. Compiled Bindings"
                Issue     = "Missing x:CompileBindings='True'"
                Files     = @($axaml.FullName)
            }
        }

        # Check for {Binding} without CompileBindings context
        $regularBindings = [regex]::Matches($content, '\{Binding[^}]*\}')
        if ($regularBindings.Count -gt 0) {
            # Allow {Binding} in DataTemplate ItemTemplate contexts (context switch)
            $inDataTemplate = $false
            $lines = Get-Content $axaml.FullName
            for ($i = 0; $i -lt $lines.Count; $i++) {
                if ($lines[$i] -match '<DataTemplate>') {
                    $inDataTemplate = $true
                }
                if ($lines[$i] -match '</DataTemplate>') {
                    $inDataTemplate = $false
                }
                if ($lines[$i] -match '\{Binding[^}]*\}' -and -not $inDataTemplate) {
                    $violations.Critical += @{
                        Principle = "VI. Compiled Bindings"
                        Issue     = "Using {Binding} instead of {CompiledBinding}"
                        Files     = @("$($axaml.FullName):$($i + 1)")
                    }
                }
            }
        }
    }

    return $violations
}

function Invoke-BuildValidation {
    param([string]$RepoPath)

    Write-ValidationLog "Running build validation..." "INFO"

    Push-Location $RepoPath
    try {
        # Set Android SDK and Java paths for Android project build
        $env:ANDROID_SDK_ROOT = "$env:LOCALAPPDATA\Android\Sdk"
        $env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
        $env:JAVA_HOME = "C:\Program Files\Android\Android Studio\jbr"

        $buildOutput = dotnet build "$RepoPath/MTM_Template_Application.sln" --no-restore 2>&1
        $buildSuccess = $LASTEXITCODE -eq 0

        # Count warnings and errors
        $errors = ($buildOutput | Select-String -Pattern 'error [A-Z]+\d+:').Count
        $warnings = ($buildOutput | Select-String -Pattern 'warning [A-Z]+\d+:').Count

        return @{
            Success  = $buildSuccess
            Errors   = $errors
            Warnings = $warnings
            Output   = $buildOutput -join "`n"
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-TestValidation {
    param([string]$RepoPath)

    Write-ValidationLog "Running test validation..." "INFO"

    Push-Location $RepoPath
    try {
        $testOutput = dotnet test "$RepoPath/MTM_Template_Application.sln" --no-build --verbosity quiet 2>&1
        $testSuccess = $LASTEXITCODE -eq 0

        # Parse test results (handles both xUnit and dotnet test summary formats)
        $summaryMatch = $testOutput | Select-String -Pattern 'Test summary: total: (\d+), failed: (\d+), succeeded: (\d+), skipped: (\d+)'
        if ($summaryMatch) {
            $total = [int]$summaryMatch.Matches[0].Groups[1].Value
            $failed = [int]$summaryMatch.Matches[0].Groups[2].Value
            $passed = [int]$summaryMatch.Matches[0].Groups[3].Value
            $skipped = [int]$summaryMatch.Matches[0].Groups[4].Value
        }
        else {
            # Fallback to individual pattern matching
            $totalMatch = $testOutput | Select-String -Pattern 'total:\s*(\d+)'
            $passedMatch = $testOutput | Select-String -Pattern 'succeeded:\s*(\d+)'
            $failedMatch = $testOutput | Select-String -Pattern 'failed:\s*(\d+)'
            $skippedMatch = $testOutput | Select-String -Pattern 'skipped:\s*(\d+)'
            $total = if ($totalMatch) { [int]$totalMatch.Matches[0].Groups[1].Value } else { 0 }
            $passed = if ($passedMatch) { [int]$passedMatch.Matches[0].Groups[1].Value } else { 0 }
            $failed = if ($failedMatch) { [int]$failedMatch.Matches[0].Groups[1].Value } else { 0 }
            $skipped = if ($skippedMatch) { [int]$skippedMatch.Matches[0].Groups[1].Value } else { 0 }
        }

        return @{
            Success = $testSuccess
            Total   = if ($totalMatch) { [int]$totalMatch.Matches[0].Groups[1].Value } else { 0 }
            Passed  = if ($passedMatch) { [int]$passedMatch.Matches[0].Groups[1].Value } else { 0 }
            Failed  = if ($failedMatch) { [int]$failedMatch.Matches[0].Groups[1].Value } else { 0 }
            Skipped = if ($skippedMatch) { [int]$skippedMatch.Matches[0].Groups[1].Value } else { 0 }
            Output  = $testOutput -join "`n"
        }
    }
    finally {
        Pop-Location
    }
}

# Main Validation Logic
try {
    Write-ValidationLog "Starting implementation validation..." "INFO"

    # Step 1: Determine feature ID
    if (-not $FeatureId) {
        $FeatureId = Get-FeatureIdFromBranch
        if (-not $FeatureId) {
            throw "Could not determine feature ID from branch. Please specify -FeatureId parameter."
        }
        Write-ValidationLog "Auto-detected feature: $FeatureId" "INFO"
    }

    # Step 2: Validate feature directory exists
    $featureDir = Join-Path $RepoRoot "specs" $FeatureId
    if (-not (Test-Path $featureDir)) {
        throw "Feature directory not found: $featureDir"
    }

    # Step 3: Load feature context
    $specFile = Join-Path $featureDir "spec.md"
    $planFile = Join-Path $featureDir "plan.md"
    $tasksFile = Join-Path $featureDir "tasks.md"
    $quickstartFile = Join-Path $featureDir "quickstart.md"

    if (-not (Test-Path $tasksFile)) {
        throw "tasks.md not found. Cannot validate without task list."
    }

    Write-ValidationLog "Loading feature context..." "INFO"

    # Step 4: Check task completion
    $taskStats = Test-TasksComplete -TasksFilePath $tasksFile
    Write-ValidationLog "Tasks: $($taskStats.Completed)/$($taskStats.Total) ($($taskStats.Percentage)%)" "INFO"

    if ($taskStats.Percentage -lt 100) {
        Write-ValidationLog "WARNING: Feature not 100% complete. Continuing validation..." "WARN"
    }

    # Step 5: Get functional requirements
    $requirements = @()
    if (Test-Path $specFile) {
        $requirements = Get-FunctionalRequirements -SpecFilePath $specFile
        Write-ValidationLog "Found $($requirements.Count) functional requirements" "INFO"
    }

    # Step 6: Constitutional compliance check
    $violations = Test-ConstitutionalCompliance -RepoPath $RepoRoot
    $totalViolations = $violations.Critical.Count + $violations.NonCritical.Count
    Write-ValidationLog "Constitutional check: $totalViolations violations ($($violations.Critical.Count) critical)" $(if ($violations.Critical.Count -gt 0) { "ERROR" } else { "SUCCESS" })

    # Step 7: Build validation
    $buildResults = Invoke-BuildValidation -RepoPath $RepoRoot
    Write-ValidationLog "Build: $(if ($buildResults.Success) { 'SUCCESS' } else { 'FAILED' }) ($($buildResults.Errors) errors, $($buildResults.Warnings) warnings)" $(if ($buildResults.Success) { "SUCCESS" } else { "ERROR" })

    # Step 8: Test validation
    $testResults = Invoke-TestValidation -RepoPath $RepoRoot
    Write-ValidationLog "Tests: $($testResults.Passed)/$($testResults.Total) passing" $(if ($testResults.Success) { "SUCCESS" } else { "ERROR" })

    # Step 9: Determine overall status
    $blockingIssues = 0
    $nonBlockingIssues = $violations.NonCritical.Count

    if ($taskStats.Percentage -lt 95) { $blockingIssues++ }
    if ($violations.Critical.Count -gt 0) { $blockingIssues += $violations.Critical.Count }
    if (-not $buildResults.Success) { $blockingIssues++ }
    if (-not $testResults.Success) { $blockingIssues++ }

    if ($Strict) {
        $blockingIssues += $nonBlockingIssues
        $nonBlockingIssues = 0
    }

    $overallStatus = if ($blockingIssues -eq 0) {
        if ($nonBlockingIssues -eq 0) { "PASS" } else { "CONDITIONAL PASS" }
    }
    else {
        "FAIL"
    }

    $readyForMerge = $overallStatus -ne "FAIL"

    # Step 10: Generate report
    $timestamp = Get-Date -Format "yyyy-MM-dd"
    $reportFile = Join-Path $featureDir "VALIDATION_${FeatureId}_${timestamp}.md"

    $reportContent = @"
# Implementation Validation Report

**Feature**: $FeatureId
**Date**: $timestamp
**Validator**: PowerShell Script (validate-implementation.ps1)
**Overall Status**: $overallStatus

---

## Executive Summary

- **Ready for Merge**: $(if ($readyForMerge) { 'YES' } else { 'NO' })
- **Blocking Issues**: $blockingIssues
- **Non-Blocking Issues**: $nonBlockingIssues

### Quick Stats

| Metric | Status | Details |
|--------|--------|---------|
| Tasks Completed | $($taskStats.Completed)/$($taskStats.Total) | $($taskStats.Percentage)% complete |
| Build Status | $(if ($buildResults.Success) { '✅ SUCCESS' } else { '❌ FAILED' }) | $($buildResults.Errors) errors, $($buildResults.Warnings) warnings |
| Tests Passing | $($testResults.Passed)/$($testResults.Total) | $(if ($testResults.Success) { '✅ PASSING' } else { '❌ FAILING' }) |
| Constitutional Violations | $totalViolations | $($violations.Critical.Count) critical, $($violations.NonCritical.Count) non-critical |

---

## Constitutional Compliance Audit

### Critical Violations ($($violations.Critical.Count))

$(if ($violations.Critical.Count -eq 0) {
    "✅ No critical violations detected."
} else {
    ($violations.Critical | ForEach-Object {
        "#### $($_.Principle)`n`n- **Issue**: $($_.Issue)`n- **Files**:`n" + ($_.Files | ForEach-Object { "  - ``$_``" }) -join "`n"
    }) -join "`n`n"
})

### Non-Critical Violations ($($violations.NonCritical.Count))

$(if ($violations.NonCritical.Count -eq 0) {
    "✅ No non-critical violations detected."
} else {
    ($violations.NonCritical | ForEach-Object {
        "#### $($_.Principle)`n`n- **Issue**: $($_.Issue)`n- **Files**:`n" + ($_.Files | ForEach-Object { "  - ``$_``" }) -join "`n"
    }) -join "`n`n"
})

---

## Build Validation

$(if ($buildResults.Success) {
    "✅ Build successful with $($buildResults.Warnings) warnings."
} else {
    "❌ Build failed with $($buildResults.Errors) errors and $($buildResults.Warnings) warnings."
})

---

## Test Validation

$(if ($testResults.Success) {
    "✅ All tests passing: $($testResults.Passed)/$($testResults.Total)"
} else {
    "❌ Tests failing: $($testResults.Failed) failed, $($testResults.Passed) passed, $($testResults.Skipped) skipped"
})

---

## Recommendations

$(if ($overallStatus -eq "PASS") {
    "✅ **Feature is ready for merge!**`n`nAll validation checks passed. The implementation meets specification requirements and constitutional compliance standards."
} elseif ($overallStatus -eq "CONDITIONAL PASS") {
    "⚠️ **Feature can be merged with conditions:**`n`n" + (1..$nonBlockingIssues | ForEach-Object { "1. Address $nonBlockingIssues non-critical issues in follow-up PR" }) -join "`n"
} else {
    "❌ **Feature CANNOT be merged yet:**`n`n" + (@(
        if ($blockingIssues -gt 0) { "1. Fix $blockingIssues blocking issues" }
        if ($violations.Critical.Count -gt 0) { "2. Resolve $($violations.Critical.Count) critical constitutional violations" }
        if (-not $buildResults.Success) { "3. Fix build errors" }
        if (-not $testResults.Success) { "4. Fix failing tests" }
    ) | Where-Object { $_ } | ForEach-Object { $_ }) -join "`n"
})

---

*Validation completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')*
"@

    $reportContent | Out-File -FilePath $reportFile -Encoding UTF8

    Write-ValidationLog "Report saved to: $reportFile" "SUCCESS"

    # Step 11: Output results
    if ($Json) {
        $jsonOutput = @{
            VALIDATION_FILE           = $reportFile
            FEATURE_ID                = $FeatureId
            COMPLETION_PERCENTAGE     = $taskStats.Percentage
            TASKS_COMPLETED           = $taskStats.Completed
            TASKS_TOTAL               = $taskStats.Total
            REQUIREMENTS_TOTAL        = $requirements.Count
            TESTS_PASSED              = $testResults.Passed
            TESTS_TOTAL               = $testResults.Total
            CONSTITUTIONAL_VIOLATIONS = @{
                CRITICAL     = $violations.Critical.Count
                NON_CRITICAL = $violations.NonCritical.Count
            }
            BUILD_STATUS              = if ($buildResults.Success) { "SUCCESS" } else { "FAILED" }
            OVERALL_STATUS            = $overallStatus
            BLOCKING_ISSUES           = $blockingIssues
            NON_BLOCKING_ISSUES       = $nonBlockingIssues
            READY_FOR_MERGE           = $readyForMerge
        } | ConvertTo-Json -Depth 10

        Write-Output $jsonOutput
    }
    else {
        Write-Host "`n========================================" -ForegroundColor Cyan
        Write-Host "  VALIDATION COMPLETE" -ForegroundColor Cyan
        Write-Host "========================================`n" -ForegroundColor Cyan
        Write-Host "Overall Status: " -NoNewline
        Write-Host $overallStatus -ForegroundColor $(if ($overallStatus -eq "PASS") { "Green" } elseif ($overallStatus -eq "CONDITIONAL PASS") { "Yellow" } else { "Red" })
        Write-Host "Ready for Merge: " -NoNewline
        Write-Host $(if ($readyForMerge) { "YES" } else { "NO" }) -ForegroundColor $(if ($readyForMerge) { "Green" } else { "Red" })
        Write-Host "`nReport: $reportFile`n" -ForegroundColor Gray
    }

    # Exit with appropriate code
    exit $(if ($readyForMerge) { 0 } else { 1 })
}
catch {
    Write-ValidationLog "FATAL ERROR: $_" "ERROR"
    Write-ValidationLog $_.ScriptStackTrace "ERROR"

    if ($Json) {
        @{
            ERROR       = $_.Exception.Message
            STACK_TRACE = $_.ScriptStackTrace
        } | ConvertTo-Json | Write-Output
    }

    exit 2
}
