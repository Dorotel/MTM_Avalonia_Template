<#
.SYNOPSIS
    Automates Copilot Chat interactions during #file:.specify implement phase.

.DESCRIPTION
    This script uses UI automation to:
    1. Monitor Copilot Chat window for file edit suggestions
    2. Auto-accept all proposed changes (clicks "Apply in Editor" buttons)
    3. Detect when Copilot shows feedback icons (thumbs up/down)
    4. Automatically type "/implement" and send to continue the workflow
    5. Runs for a maximum of 10 cycles to prevent infinite loops

.PARAMETER MaxCycles
    Maximum number of implement cycles to run (default: 10)

.PARAMETER CheckInterval
    Seconds between UI checks (default: 2)

.PARAMETER FeedbackIconTimeout
    Seconds to wait for feedback icons after accepting changes (default: 10)

.EXAMPLE
    .\auto-implement-copilot.ps1
    Runs with default settings (10 cycles, 2 second checks)

.EXAMPLE
    .\auto-implement-copilot.ps1 -MaxCycles 5 -CheckInterval 3
    Runs 5 cycles with 3 second check intervals

.NOTES
    Requires: Windows PowerShell 5.1+ or PowerShell 7+
    Requires: VS Code to be the active window
    WARNING: This script simulates keyboard/mouse input - do not interact with your computer while running!
#>

[CmdletBinding()]
param(
    [Parameter()]
    [int]$MaxCycles = 10,

    [Parameter()]
    [int]$CheckInterval = 2,

    [Parameter()]
    [int]$FeedbackIconTimeout = 10
)

# Load required assemblies for UI automation
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# Script state
$script:CycleCount = 0
$script:Running = $true
$script:LogPath = Join-Path $PSScriptRoot "auto-implement-copilot.log"

#region Helper Functions

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('INFO', 'SUCCESS', 'WARNING', 'ERROR')]
        [string]$Level = 'INFO'
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"

    # Console output with colors
    switch ($Level) {
        'SUCCESS' { Write-Host $logMessage -ForegroundColor Green }
        'WARNING' { Write-Host $logMessage -ForegroundColor Yellow }
        'ERROR' { Write-Host $logMessage -ForegroundColor Red }
        default { Write-Host $logMessage -ForegroundColor Cyan }
    }

    # File logging
    Add-Content -Path $script:LogPath -Value $logMessage
}

function Test-VSCodeActive {
    <#
    .SYNOPSIS
        Checks if VS Code is the active window
    #>
    Add-Type @"
        using System;
        using System.Runtime.InteropServices;
        using System.Text;
        public class WindowHelper {
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            public static string GetActiveWindowTitle() {
                const int nChars = 256;
                StringBuilder buff = new StringBuilder(nChars);
                IntPtr handle = GetForegroundWindow();
                if (GetWindowText(handle, buff, nChars) > 0) {
                    return buff.ToString();
                }
                return null;
            }
        }
"@ -ErrorAction SilentlyContinue

    $activeTitle = [WindowHelper]::GetActiveWindowTitle()
    return $activeTitle -like "*Visual Studio Code*"
}

function Send-Keys {
    param([string]$Keys)

    Start-Sleep -Milliseconds 100
    [System.Windows.Forms.SendKeys]::SendWait($Keys)
    Start-Sleep -Milliseconds 100
}

function Find-CopilotChatPane {
    <#
    .SYNOPSIS
        Attempts to locate Copilot Chat pane using UI Automation
    #>
    try {
        $automation = [System.Windows.Automation.AutomationElement]::RootElement

        # Find VS Code window
        $condition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty,
            "*Visual Studio Code*"
        )

        $vscodeWindow = $automation.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            $condition
        )

        if ($vscodeWindow) {
            Write-Log "Found VS Code window: $($vscodeWindow.Current.Name)" -Level INFO
            return $vscodeWindow
        }

        return $null
    }
    catch {
        Write-Log "Error finding Copilot Chat pane: $($_.Exception.Message)" -Level ERROR
        return $null
    }
}

function Test-FeedbackIconsPresent {
    <#
    .SYNOPSIS
        Checks if thumbs up/down feedback icons are visible (indicates Copilot is done)
    #>
    try {
        $vscodeWindow = Find-CopilotChatPane
        if (-not $vscodeWindow) {
            return $false
        }

        # Look for buttons with accessibility names containing "thumb" or "feedback"
        $buttonCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Button
        )

        $buttons = $vscodeWindow.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            $buttonCondition
        )

        foreach ($button in $buttons) {
            $name = $button.Current.Name.ToLower()
            if ($name -match "thumb|feedback|helpful|not helpful") {
                Write-Log "Detected feedback icon: $($button.Current.Name)" -Level SUCCESS
                return $true
            }
        }

        return $false
    }
    catch {
        Write-Log "Error checking for feedback icons: $($_.Exception.Message)" -Level ERROR
        return $false
    }
}

function Invoke-AutoAcceptChanges {
    <#
    .SYNOPSIS
        Clicks "Apply in Editor" or similar buttons in Copilot Chat
    #>
    try {
        $vscodeWindow = Find-CopilotChatPane
        if (-not $vscodeWindow) {
            Write-Log "VS Code window not found - cannot auto-accept" -Level WARNING
            return $false
        }

        # Look for buttons with text like "Apply", "Insert", "Accept"
        $buttonCondition = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Button
        )

        $buttons = $vscodeWindow.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            $buttonCondition
        )

        $acceptedCount = 0
        foreach ($button in $buttons) {
            $name = $button.Current.Name.ToLower()
            if ($name -match "apply|insert|accept|use") {
                Write-Log "Clicking button: $($button.Current.Name)" -Level SUCCESS

                # Invoke the button
                $invokePattern = $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                if ($invokePattern) {
                    $invokePattern.Invoke()
                    $acceptedCount++
                    Start-Sleep -Milliseconds 500
                }
            }
        }

        if ($acceptedCount -gt 0) {
            Write-Log "Auto-accepted $acceptedCount change(s)" -Level SUCCESS
            return $true
        }

        return $false
    }
    catch {
        Write-Log "Error auto-accepting changes: $($_.Exception.Message)" -Level ERROR
        return $false
    }
}

function Send-ImplementCommand {
    <#
    .SYNOPSIS
        Types "/implement" in Copilot Chat and sends it
    #>
    try {
        Write-Log "Sending /implement command..." -Level INFO

        # Type /implement directly (assumes chat input is already focused)
        Send-Keys "/implement"
        Start-Sleep -Milliseconds 300

        # Press Enter twice to send (first for autocomplete, second to submit)
        Send-Keys "{ENTER}"
        Start-Sleep -Milliseconds 200
        Send-Keys "{ENTER}"
        Start-Sleep -Milliseconds 500

        Write-Log "/implement command sent successfully" -Level SUCCESS
        return $true
    }
    catch {
        Write-Log "Error sending /implement command: $($_.Exception.Message)" -Level ERROR
        return $false
    }
}

#endregion

#region Main Automation Loop

function Start-AutoImplementCycle {
    Write-Log "=== Starting Auto-Implement Copilot Automation ===" -Level INFO
    Write-Log "Max Cycles: $MaxCycles" -Level INFO
    Write-Log "Check Interval: $CheckInterval seconds" -Level INFO
    Write-Log "Feedback Icon Timeout: $FeedbackIconTimeout seconds" -Level INFO
    Write-Log "Press Ctrl+C to stop early" -Level WARNING
    Write-Log "" -Level INFO

    Start-Sleep -Seconds 3  # Give user time to switch to VS Code

    while ($script:Running -and $script:CycleCount -lt $MaxCycles) {
        $script:CycleCount++
        Write-Log "=== Cycle $script:CycleCount of $MaxCycles ===" -Level INFO

        # Verify VS Code is active
        if (-not (Test-VSCodeActive)) {
            Write-Log "VS Code is not the active window - please switch to VS Code" -Level WARNING
            Start-Sleep -Seconds 5
            continue
        }

        # Step 1: Continuously auto-accept changes and wait for feedback icons
        Write-Log "Step 1: Monitoring for Accept buttons and feedback icons..." -Level INFO
        $iconWaitStart = Get-Date
        $iconsFound = $false
        $totalAccepted = 0

        while (((Get-Date) - $iconWaitStart).TotalSeconds -lt $FeedbackIconTimeout) {
            # Check for Accept buttons continuously
            $changesAccepted = Invoke-AutoAcceptChanges
            if ($changesAccepted) {
                $totalAccepted++
                Write-Log "Auto-accepted changes (batch $totalAccepted)" -Level SUCCESS
                Start-Sleep -Milliseconds 500
            }

            # Check if feedback icons appeared (Copilot is done)
            if (Test-FeedbackIconsPresent) {
                $iconsFound = $true
                Write-Log "Feedback icons detected - Copilot is ready" -Level SUCCESS
                break
            }

            # Wait before next check
            Start-Sleep -Seconds $CheckInterval
        }

        if (-not $iconsFound) {
            Write-Log "Timeout waiting for feedback icons - continuing anyway" -Level WARNING
        }

        if ($totalAccepted -gt 0) {
            Write-Log "Total changes auto-accepted this cycle: $totalAccepted" -Level SUCCESS
        }

        # Step 2: Send /implement command
        Write-Log "Step 2: Sending /implement command..." -Level INFO
        $implementSent = Send-ImplementCommand

        if (-not $implementSent) {
            Write-Log "Failed to send /implement command - retrying in next cycle" -Level ERROR
        }

        # Wait before next cycle
        Write-Log "Cycle $script:CycleCount complete. Waiting $CheckInterval seconds before next cycle..." -Level SUCCESS
        Write-Log "" -Level INFO
        Start-Sleep -Seconds $CheckInterval
    }

    Write-Log "=== Auto-Implement Automation Complete ===" -Level SUCCESS
    Write-Log "Total cycles completed: $script:CycleCount" -Level INFO
}

#endregion

#region Script Entry Point

# Handle Ctrl+C gracefully
[Console]::TreatControlCAsInput = $false
$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    Write-Log "Script interrupted by user" -Level WARNING
    $script:Running = $false
}

# Verify prerequisites
if (-not (Test-VSCodeActive)) {
    Write-Log "WARNING: VS Code is not the active window!" -Level WARNING
    Write-Log "Please switch to VS Code within 5 seconds..." -Level WARNING
    Start-Sleep -Seconds 5
}

# Start automation
try {
    Start-AutoImplementCycle
}
catch {
    Write-Log "Fatal error: $($_.Exception.Message)" -Level ERROR
    Write-Log $_.ScriptStackTrace -Level ERROR
}
finally {
    Write-Log "Script finished. Log saved to: $script:LogPath" -Level INFO
}

#endregion
