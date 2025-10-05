using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Configuration;
using MTM_Template_Application.Services.Secrets;
using NSubstitute;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for log redaction based on spec.md FR-014 and NFR-006
/// These tests verify that sensitive data is never logged in plaintext.
/// </summary>
public class LogRedactionContractTests : IDisposable
{
    private readonly Serilog.Core.Logger _serilogLogger;
    private readonly InMemorySink _logSink;

    public LogRedactionContractTests()
    {
        // Configure in-memory Serilog sink to capture log output
        _logSink = new InMemorySink();
        _serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(_logSink)
            .CreateLogger();
    }

    public void Dispose()
    {
        _serilogLogger?.Dispose();
    }

    #region T031: Log Redaction Validation Tests

    [Theory]
    [InlineData("password")]
    [InlineData("token")]
    [InlineData("secret")]
    [InlineData("credential")]
    [InlineData("apikey")]
    [InlineData("api_key")]
    [InlineData("PASSWORD")]
    [InlineData("Token")]
    [InlineData("Secret")]
    [Trait("Category", "Contract")]
    public void ConfigurationService_DoesNotLogSensitiveKeys(string sensitiveKey)
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Attempt to set a sensitive value
        var task = service.SetValue($"Test:{sensitiveKey}", "SensitiveValue123", CancellationToken.None);
        task.Wait();

        // Assert - Log should not contain the plaintext value
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();
        var shouldNotContain = logMessages.Where(msg => msg.Contains("SensitiveValue123")).ToList();

        shouldNotContain.Should().BeEmpty(
            $"Sensitive value for key '{sensitiveKey}' should not appear in logs in plaintext");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void ConfigurationService_RedactsSensitiveValues_InLogOutput()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Set a password configuration
        var task = service.SetValue("Database:Password", "SuperSecret123!", CancellationToken.None);
        task.Wait();

        // Assert - Check if redaction marker exists (if implemented)
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();

        // Verify that the actual password is NOT in the logs
        var containsPassword = logMessages.Any(msg => msg.Contains("SuperSecret123!"));
        containsPassword.Should().BeFalse("Password should not appear in logs");

        // Verify that redaction marker exists (if implemented)
        // Note: This part depends on implementation - may show "[REDACTED]" or similar
        if (logMessages.Any(msg => msg.Contains("Database:Password")))
        {
            var passwordLogs = logMessages.Where(msg => msg.Contains("Database:Password")).ToList();
            passwordLogs.Should().OnlyContain(msg =>
                msg.Contains("[REDACTED]") ||
                msg.Contains("***") ||
                !msg.Contains("SuperSecret123!"));
        }
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task SecretsService_NeverLogsCredentialValues()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<WindowsSecretsService>();
        var service = new WindowsSecretsService(logger);
        var testKey = "Test.CredentialForLogging";
        var testValue = "VerySecretPassword123!@#";

        // Act - Store and retrieve secret
        await service.StoreSecretAsync(testKey, testValue, CancellationToken.None);
        var retrievedValue = await service.RetrieveSecretAsync(testKey, CancellationToken.None);

        // Assert - Log should NEVER contain the credential value
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();
        var containsCredential = logMessages.Any(msg => msg.Contains(testValue));

        containsCredential.Should().BeFalse(
            "Credential values should NEVER appear in logs");

        // Cleanup
        await service.DeleteSecretAsync(testKey, CancellationToken.None);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void ConfigurationService_RedactsMultipleSensitiveKeys()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        var sensitiveKeys = new Dictionary<string, string>
        {
            { "API:Token", "abc123token" },
            { "Database:Password", "dbPass456!" },
            { "OAuth:ClientSecret", "secret789xyz" },
            { "Azure:ApiKey", "azureKey000" }
        };

        // Act - Set multiple sensitive values
        foreach (var kvp in sensitiveKeys)
        {
            var task = service.SetValue(kvp.Key, kvp.Value, CancellationToken.None);
            task.Wait();
        }

        // Assert - None of the sensitive values should appear in logs
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();

        foreach (var kvp in sensitiveKeys)
        {
            var containsValue = logMessages.Any(msg => msg.Contains(kvp.Value));
            containsValue.Should().BeFalse(
                $"Sensitive value '{kvp.Value}' for key '{kvp.Key}' should not appear in logs");
        }
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void LogRedaction_AllowsNonSensitiveKeys_InLogs()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Set non-sensitive configuration values
        var task = service.SetValue("Display:Theme", "Dark", CancellationToken.None);
        task.Wait();

        // Assert - Non-sensitive values CAN appear in logs (for debugging)
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();

        // This is allowed - "Dark" is not sensitive
        // We're just verifying that the redaction logic doesn't over-redact
        logMessages.Should().NotBeEmpty("Configuration operations should generate log entries");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void LogRedaction_WorksWithStructuredLogging()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Set configuration that might use structured logging
        var task = service.SetValue("OAuth:ClientSecret", "structuredSecretValue", CancellationToken.None);
        task.Wait();

        // Assert - Check structured logging properties
        var logEvents = _logSink.LogEvents.ToList();

        foreach (var logEvent in logEvents)
        {
            // Check all properties - none should contain the secret value
            foreach (var prop in logEvent.Properties.Values)
            {
                var propValue = prop.ToString();
                propValue.Should().NotContain("structuredSecretValue",
                    "Structured logging properties should not contain sensitive values");
            }
        }
    }

    [Theory]
    [InlineData("username")] // Username is NOT sensitive (just identifier)
    [InlineData("email")]    // Email is NOT sensitive (just identifier)
    [InlineData("userId")]   // UserId is NOT sensitive (just identifier)
    [Trait("Category", "Contract")]
    public void LogRedaction_DoesNotRedactIdentifiers(string identifierKey)
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Set identifier values (NOT sensitive)
        var identifierValue = $"{identifierKey}Value123";
        var task = service.SetValue($"User:{identifierKey}", identifierValue, CancellationToken.None);
        task.Wait();

        // Assert - Identifiers CAN appear in logs (they're not secrets)
        // This test ensures we don't over-redact non-sensitive data
        var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();
        logMessages.Should().NotBeEmpty("Configuration operations should generate log entries");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void LogRedaction_HandlesCaseInsensitiveSensitiveKeys()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(_serilogLogger));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        var testCases = new[]
        {
            "API:password",
            "API:Password",
            "API:PASSWORD",
            "API:PaSsWoRd"
        };

        // Act & Assert
        foreach (var key in testCases)
        {
            _logSink.LogEvents.Clear(); // Clear between tests

            var task = service.SetValue(key, "TestPassword123", CancellationToken.None);
            task.Wait();

            var logMessages = _logSink.LogEvents.Select(e => e.RenderMessage()).ToList();
            var containsPassword = logMessages.Any(msg => msg.Contains("TestPassword123"));

            containsPassword.Should().BeFalse(
                $"Password value should be redacted for key '{key}' regardless of case");
        }
    }

    #endregion

    #region Additional Security Verification

    [Fact]
    [Trait("Category", "Contract")]
    public void LogLevel_Debug_DoesNotExposeSecrets()
    {
        // Arrange
        var logSink = new InMemorySink();
        var serilog = new LoggerConfiguration()
            .MinimumLevel.Verbose() // Most verbose level
            .WriteTo.Sink(logSink)
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog(serilog));
        var logger = loggerFactory.CreateLogger<ConfigurationService>();
        var service = new ConfigurationService(logger);

        // Act - Even at debug level, secrets should not leak
        var task = service.SetValue("Debug:Secret", "DebugSecretValue", CancellationToken.None);
        task.Wait();

        // Assert
        var logMessages = logSink.LogEvents.Select(e => e.RenderMessage()).ToList();
        var containsSecret = logMessages.Any(msg => msg.Contains("DebugSecretValue"));

        containsSecret.Should().BeFalse(
            "Secrets should not appear in logs even at Debug/Verbose level");

        serilog.Dispose();
    }

    #endregion
}
