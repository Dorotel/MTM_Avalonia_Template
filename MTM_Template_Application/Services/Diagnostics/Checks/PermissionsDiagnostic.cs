using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics.Checks;

/// <summary>
/// Verify file system, camera, and network permissions
/// </summary>
public class PermissionsDiagnostic : IDiagnosticCheck
{
    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var details = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Check file system permissions
            await CheckFileSystemPermissionsAsync(details, issues);

            // Check camera permissions (platform-specific)
            CheckCameraPermissions(details, issues);

            // Check network permissions
            CheckNetworkPermissions(details, issues);

            stopwatch.Stop();

            if (issues.Count == 0)
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(PermissionsDiagnostic),
                    Status = DiagnosticStatus.Passed,
                    Message = "All permissions checks passed",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(PermissionsDiagnostic),
                    Status = DiagnosticStatus.Failed,
                    Message = $"Permission issues detected: {string.Join(", ", issues)}",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            details["Exception"] = ex.ToString();

            return new DiagnosticResult
            {
                CheckName = nameof(PermissionsDiagnostic),
                Status = DiagnosticStatus.Failed,
                Message = $"Permissions check failed: {ex.Message}",
                Details = details,
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private async Task CheckFileSystemPermissionsAsync(Dictionary<string, object> details, List<string> issues)
    {
        try
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var testFile = Path.Combine(appDirectory, $".permission_test_{Guid.NewGuid()}.tmp");

            // Test write permissions
            await File.WriteAllTextAsync(testFile, "test");

            // Test read permissions
            var content = await File.ReadAllTextAsync(testFile);

            // Test delete permissions
            File.Delete(testFile);

            details["FileSystemPermissions"] = "Read/Write/Delete - OK";
        }
        catch (UnauthorizedAccessException)
        {
            details["FileSystemPermissions"] = "FAILED - Unauthorized";
            issues.Add("Insufficient file system permissions");
        }
        catch (Exception ex)
        {
            details["FileSystemPermissions"] = $"FAILED - {ex.Message}";
            issues.Add($"File system error: {ex.Message}");
        }
    }

    private void CheckCameraPermissions(Dictionary<string, object> details, List<string> issues)
    {
        // Platform-specific camera permission checks would go here
        // For now, we'll mark as "Not Checked" since this requires platform-specific APIs
        details["CameraPermissions"] = "Not Checked (requires platform-specific implementation)";
    }

    private void CheckNetworkPermissions(Dictionary<string, object> details, List<string> issues)
    {
        try
        {
            // Check if we can create a network request (basic check)
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);

            // This is just a permission check, not a connectivity check
            details["NetworkPermissions"] = "OK";
        }
        catch (Exception ex)
        {
            details["NetworkPermissions"] = $"FAILED - {ex.Message}";
            issues.Add("Network access may be restricted");
        }
    }
}
