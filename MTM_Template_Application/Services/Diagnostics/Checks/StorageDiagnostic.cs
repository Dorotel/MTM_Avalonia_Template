using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics.Checks;

/// <summary>
/// Verify storage availability and free space
/// </summary>
public class StorageDiagnostic : IDiagnosticCheck
{
    private const long MinimumFreeSpaceBytes = 100 * 1024 * 1024; // 100MB minimum
    private const long RecommendedFreeSpaceBytes = 1024 * 1024 * 1024; // 1GB recommended

    public async Task<DiagnosticResult> RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var details = new Dictionary<string, object>();

        try
        {
            // Get the application directory
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var driveInfo = new DriveInfo(Path.GetPathRoot(appDirectory) ?? "C:\\");

            var availableSpace = driveInfo.AvailableFreeSpace;
            var totalSpace = driveInfo.TotalSize;
            var usedSpace = totalSpace - availableSpace;
            var usedPercentage = (double)usedSpace / totalSpace * 100;

            details["DriveName"] = driveInfo.Name;
            details["DriveFormat"] = driveInfo.DriveFormat;
            details["TotalSpaceGB"] = Math.Round(totalSpace / (1024.0 * 1024.0 * 1024.0), 2);
            details["AvailableSpaceGB"] = Math.Round(availableSpace / (1024.0 * 1024.0 * 1024.0), 2);
            details["UsedSpaceGB"] = Math.Round(usedSpace / (1024.0 * 1024.0 * 1024.0), 2);
            details["UsedPercentage"] = Math.Round(usedPercentage, 2);

            stopwatch.Stop();

            // Determine status
            if (availableSpace < MinimumFreeSpaceBytes)
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(StorageDiagnostic),
                    Status = DiagnosticStatus.Failed,
                    Message = $"Insufficient storage space: {Math.Round(availableSpace / (1024.0 * 1024.0), 2)} MB available (minimum 100 MB required)",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
            else if (availableSpace < RecommendedFreeSpaceBytes)
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(StorageDiagnostic),
                    Status = DiagnosticStatus.Warning,
                    Message = $"Low storage space: {Math.Round(availableSpace / (1024.0 * 1024.0 * 1024.0), 2)} GB available (1 GB recommended)",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(StorageDiagnostic),
                    Status = DiagnosticStatus.Passed,
                    Message = $"Storage check passed: {Math.Round(availableSpace / (1024.0 * 1024.0 * 1024.0), 2)} GB available",
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
                CheckName = nameof(StorageDiagnostic),
                Status = DiagnosticStatus.Failed,
                Message = $"Storage check failed: {ex.Message}",
                Details = details,
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
