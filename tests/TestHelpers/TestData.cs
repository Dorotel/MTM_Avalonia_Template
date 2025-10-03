using System.Collections.Generic;

namespace MTM_Template_Tests.TestHelpers;

/// <summary>
/// Provides shared test data fixtures for unit and integration tests
/// </summary>
public static class TestData
{
    /// <summary>
    /// Sample configuration settings for testing
    /// </summary>
    public static class Configuration
    {
        public const string DefaultEnvironment = "Test";
        public const string DefaultProfile = "TestProfile";
        public static readonly Dictionary<string, string> DefaultSettings = new()
        {
            { "Setting1", "Value1" },
            { "Setting2", "Value2" }
        };
    }

    /// <summary>
    /// Sample boot metrics for testing
    /// </summary>
    public static class Boot
    {
        public const int ExpectedStageCount = 3;
        public const int MaxBootTimeMs = 10000;
        public const int Stage1MaxTimeMs = 3000;
    }

    /// <summary>
    /// Sample cache settings for testing
    /// </summary>
    public static class Cache
    {
        public const int PartsTimeToLiveHours = 24;
        public const int OthersTimeToLiveDays = 7;
        public const int MaxCacheSizeMB = 100;
    }

    /// <summary>
    /// Sample diagnostic thresholds for testing
    /// </summary>
    public static class Diagnostics
    {
        public const int NetworkTimeoutMs = 5000;
        public const long MinimumFreeSpaceBytes = 100 * 1024 * 1024; // 100MB
    }

    /// <summary>
    /// Sample error messages for testing
    /// </summary>
    public static class ErrorMessages
    {
        public const string ConfigurationLoadFailed = "Failed to load configuration";
        public const string NetworkTimeout = "Network operation timed out";
        public const string DatabaseConnectionFailed = "Failed to connect to database";
    }
}
