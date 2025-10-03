using System;
using System.Threading;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace MTM_Template_Application.Services.Logging;

/// <summary>
/// Log rotation policy: 10MB max file size, 7 days retention
/// </summary>
public class LogRotationPolicy
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private const int RetentionDays = 7;

    private readonly string _logDirectory;
    private readonly string _logFilePrefix;

    public LogRotationPolicy(string logDirectory, string logFilePrefix = "app")
    {
        ArgumentNullException.ThrowIfNull(logDirectory);
        ArgumentNullException.ThrowIfNull(logFilePrefix);

        _logDirectory = logDirectory;
        _logFilePrefix = logFilePrefix;
    }

    /// <summary>
    /// Get the current log file path
    /// </summary>
    public string GetCurrentLogPath()
    {
        EnsureLogDirectoryExists();
        return Path.Combine(_logDirectory, $"{_logFilePrefix}-.log");
    }

    /// <summary>
    /// Get the rolling log file path with date
    /// </summary>
    public string GetRollingLogPath()
    {
        EnsureLogDirectoryExists();
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        return Path.Combine(_logDirectory, $"{_logFilePrefix}-{date}-.log");
    }

    /// <summary>
    /// Check if the current log file should be rotated
    /// </summary>
    public bool ShouldRotate(string logFilePath)
    {
        if (!File.Exists(logFilePath))
        {
            return false;
        }

        var fileInfo = new FileInfo(logFilePath);
        return fileInfo.Length >= MaxFileSizeBytes;
    }

    /// <summary>
    /// Clean up old log files based on retention policy
    /// </summary>
    public void CleanupOldLogs()
    {
        EnsureLogDirectoryExists();

        var directory = new DirectoryInfo(_logDirectory);
        var cutoffDate = DateTime.UtcNow.AddDays(-RetentionDays);

        var oldLogFiles = directory.GetFiles($"{_logFilePrefix}-*.log")
            .Where(f => f.CreationTimeUtc < cutoffDate)
            .ToList();

        foreach (var file in oldLogFiles)
        {
            try
            {
                file.Delete();
            }
            catch (Exception)
            {
                // Silently ignore deletion failures (file might be in use)
            }
        }
    }

    /// <summary>
    /// Get the size of the current log file
    /// </summary>
    public long GetCurrentLogSize(string logFilePath)
    {
        if (!File.Exists(logFilePath))
        {
            return 0;
        }

        var fileInfo = new FileInfo(logFilePath);
        return fileInfo.Length;
    }

    /// <summary>
    /// Get all log files in the log directory
    /// </summary>
    public FileInfo[] GetAllLogFiles()
    {
        EnsureLogDirectoryExists();

        var directory = new DirectoryInfo(_logDirectory);
        return directory.GetFiles($"{_logFilePrefix}-*.log")
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToArray();
    }

    /// <summary>
    /// Get total size of all log files
    /// </summary>
    public long GetTotalLogSize()
    {
        var logFiles = GetAllLogFiles();
        return logFiles.Sum(f => f.Length);
    }

    /// <summary>
    /// Ensure the log directory exists
    /// </summary>
    private void EnsureLogDirectoryExists()
    {
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    /// <summary>
    /// Configuration for Serilog file sink with rotation
    /// </summary>
    public static class SerilogConfiguration
    {
        /// <summary>
        /// Get the rolling file path template for Serilog
        /// </summary>
        public static string GetRollingFileTemplate(string logDirectory, string logFilePrefix = "app")
        {
            return Path.Combine(logDirectory, $"{logFilePrefix}-.log");
        }

        /// <summary>
        /// Get the file size limit for Serilog (10MB)
        /// </summary>
        public static long GetFileSizeLimit()
        {
            return MaxFileSizeBytes;
        }

        /// <summary>
        /// Get the retained file count limit (7 days worth)
        /// </summary>
        public static int GetRetainedFileCountLimit()
        {
            // Assuming 1 file per day, keep 7 days worth
            return RetentionDays;
        }

        /// <summary>
        /// Get the rolling interval (daily)
        /// </summary>
        public static Serilog.RollingInterval GetRollingInterval()
        {
            return Serilog.RollingInterval.Day;
        }
    }
}
