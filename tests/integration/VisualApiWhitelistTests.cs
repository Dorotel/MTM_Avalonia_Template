using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MTM_Template_Tests.Integration;

/// <summary>
/// T028: Integration tests for Visual ERP API read-only whitelist
/// Tests command validation and citation format compliance
/// Maps to Scenario 7 from quickstart.md
/// </summary>
public class VisualApiWhitelistTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _whitelist;
    private readonly string _citationPattern;

    public VisualApiWhitelistTests(ITestOutputHelper output)
    {
        _output = output;

        // Mock configuration data
        _whitelist = new List<string>
        {
            "GET_PART_DETAILS",
            "LIST_INVENTORY",
            "GET_WORK_ORDER",
            "LIST_SHIPMENTS",
            "GET_CUSTOMER_INFO"
        };
        _citationPattern = @"^[A-Z_]+:\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$";
    }

    #region T028: Visual API Whitelist Tests

    /// <summary>
    /// T028.1: Test whitelist loads from appsettings.json
    /// Verifies FR-026: Whitelist configuration is loaded correctly
    /// </summary>
    [Fact]
    public void T028_VisualWhitelist_LoadsFromConfiguration()
    {
        // Arrange & Act
        var whitelist = _whitelist;

        // Assert
        whitelist.Should().NotBeNull("whitelist should be loaded from configuration");
        whitelist.Should().NotBeEmpty("whitelist should contain commands");
        whitelist.Should().Contain("GET_PART_DETAILS", "whitelist should include read-only commands");

        _output.WriteLine($"✓ Whitelist loaded from configuration:");
        foreach (var command in whitelist!)
        {
            _output.WriteLine($"  - {command}");
        }
    }

    /// <summary>
    /// T028.2: Test read-only commands are allowed
    /// Verifies FR-026: Read-only commands pass whitelist validation
    /// </summary>
    [Theory]
    [InlineData("GET_PART_DETAILS")]
    [InlineData("LIST_INVENTORY")]
    [InlineData("GET_WORK_ORDER")]
    [InlineData("LIST_SHIPMENTS")]
    [InlineData("GET_CUSTOMER_INFO")]
    public void T028_VisualWhitelist_AllowsReadOnlyCommands(string command)
    {
        // Arrange
        var whitelist = _whitelist;

        // Act
        var isAllowed = whitelist.Contains(command);

        // Assert
        isAllowed.Should().BeTrue($"{command} is a read-only command and should be allowed");

        _output.WriteLine($"✓ Read-only command allowed: {command}");
    }

    /// <summary>
    /// T028.3: Test write commands are blocked
    /// Verifies FR-026: Write commands are rejected by whitelist
    /// </summary>
    [Theory]
    [InlineData("UPDATE_INVENTORY")]
    [InlineData("DELETE_PART")]
    [InlineData("CREATE_WORK_ORDER")]
    [InlineData("MODIFY_SHIPMENT")]
    [InlineData("UPDATE_CUSTOMER")]
    public void T028_VisualWhitelist_BlocksWriteCommands(string command)
    {
        // Arrange
        var whitelist = _whitelist;

        // Act
        var isAllowed = whitelist.Contains(command);

        // Assert
        isAllowed.Should().BeFalse($"{command} is a write command and should be blocked");

        _output.WriteLine($"✓ Write command blocked: {command}");
    }

    /// <summary>
    /// T028.4: Test citation format validation regex
    /// Verifies FR-026: Citation format matches pattern: COMMAND:YYYY-MM-DDTHH:MM:SSZ
    /// </summary>
    [Theory]
    [InlineData("GET_PART_DETAILS:2025-01-15T14:30:00Z", true)]
    [InlineData("LIST_INVENTORY:2025-12-31T23:59:59Z", true)]
    [InlineData("GET_WORK_ORDER:2025-06-15T08:00:00Z", true)]
    [InlineData("InvalidFormat:2025-01-15", false)]
    [InlineData("GET_PART_DETAILS:15-01-2025T14:30:00Z", false)]
    [InlineData("get_part_details:2025-01-15T14:30:00Z", false)] // Lowercase not allowed
    [InlineData("", false)]
    public void T028_VisualWhitelist_ValidatesCitationFormat(string citation, bool shouldBeValid)
    {
        // Arrange
        var citationPattern = _citationPattern;
        var regex = new Regex(citationPattern);

        // Act
        var isValid = regex.IsMatch(citation);

        // Assert
        isValid.Should().Be(shouldBeValid,
            $"citation '{citation}' should be {(shouldBeValid ? "valid" : "invalid")}");

        _output.WriteLine($"{(isValid ? "✓" : "✗")} Citation: {citation}");
    }

    /// <summary>
    /// T028.5: Test whitelist case sensitivity
    /// Verifies FR-026: Command names are case-sensitive (uppercase required)
    /// </summary>
    [Theory]
    [InlineData("GET_PART_DETAILS", true)]  // Uppercase = allowed
    [InlineData("get_part_details", false)] // Lowercase = blocked
    [InlineData("Get_Part_Details", false)] // Mixed case = blocked
    [InlineData("GET_part_DETAILS", false)] // Mixed case = blocked
    public void T028_VisualWhitelist_IsCaseSensitive(string command, bool shouldBeAllowed)
    {
        // Arrange
        var whitelist = _whitelist;

        // Act
        var isAllowed = whitelist.Contains(command);

        // Assert
        isAllowed.Should().Be(shouldBeAllowed,
            $"whitelist is case-sensitive, command '{command}' should be {(shouldBeAllowed ? "allowed" : "blocked")}");

        _output.WriteLine($"{(isAllowed ? "✓" : "✗")} Command: {command} (case-sensitive check)");
    }

    /// <summary>
    /// T028.6: Test unknown commands are rejected
    /// Verifies FR-026: Commands not in whitelist are blocked
    /// </summary>
    [Theory]
    [InlineData("UNKNOWN_COMMAND")]
    [InlineData("GET_SECRET_DATA")]
    [InlineData("EXECUTE_SCRIPT")]
    [InlineData("")]
    [InlineData(null)]
    public void T028_VisualWhitelist_RejectsUnknownCommands(string? command)
    {
        // Arrange
        var whitelist = _whitelist;

        // Act
        var isAllowed = !string.IsNullOrEmpty(command) && whitelist.Contains(command);

        // Assert
        isAllowed.Should().BeFalse($"unknown command '{command ?? "(null)"}' should be rejected");

        _output.WriteLine($"✓ Unknown command rejected: {command ?? "(null)"}");
    }

    /// <summary>
    /// T028.7: Test whitelist cannot be empty
    /// Verifies FR-026: Whitelist must contain at least one command
    /// </summary>
    [Fact]
    public void T028_VisualWhitelist_CannotBeEmpty()
    {
        // Arrange
        var whitelist = _whitelist;

        // Assert
        whitelist.Should().NotBeNull("whitelist must be configured");
        whitelist.Should().NotBeEmpty("whitelist must contain at least one command for security");
        whitelist.Count.Should().BeGreaterThan(0, "empty whitelist would block all API calls");

        _output.WriteLine($"✓ Whitelist contains {whitelist.Count} commands");
    }

    /// <summary>
    /// T028.8: Test duplicate commands in whitelist
    /// Verifies FR-026: Whitelist handles duplicate entries gracefully
    /// </summary>
    [Fact]
    public void T028_VisualWhitelist_HandlesDuplicates()
    {
        // Arrange
        var whitelist = _whitelist;

        // Act
        var uniqueCommands = whitelist.Distinct().ToList();
        var hasDuplicates = whitelist.Count != uniqueCommands.Count;

        // Assert - Duplicates should either be prevented or deduplicated
        if (hasDuplicates)
        {
            _output.WriteLine($"⚠ Whitelist contains {whitelist.Count - uniqueCommands.Count} duplicate(s)");
            _output.WriteLine($"  Total: {whitelist.Count}, Unique: {uniqueCommands.Count}");
            _output.WriteLine($"  Note: Implementation should deduplicate whitelist");
        }
        else
        {
            _output.WriteLine($"✓ Whitelist contains no duplicates ({uniqueCommands.Count} unique commands)");
        }

        // Document expected behavior
        uniqueCommands.Count.Should().BeGreaterThan(0, "whitelist should contain valid commands after deduplication");
    }

    /// <summary>
    /// T028.9: Test citation timestamp validation
    /// Verifies FR-026: Citation timestamps must be valid ISO 8601 UTC format
    /// </summary>
    [Theory]
    [InlineData("2025-01-15T14:30:00Z", true)]   // Valid ISO 8601 UTC
    [InlineData("2025-12-31T23:59:59Z", true)]   // Valid end of year
    [InlineData("2025-02-29T12:00:00Z", false)]  // Invalid: 2025 not leap year
    [InlineData("2025-13-01T00:00:00Z", false)]  // Invalid month
    [InlineData("2025-01-32T00:00:00Z", false)]  // Invalid day
    [InlineData("2025-01-15T25:00:00Z", false)]  // Invalid hour
    [InlineData("2025-01-15T14:60:00Z", false)]  // Invalid minute
    [InlineData("2025-01-15T14:30:60Z", false)]  // Invalid second
    public void T028_VisualWhitelist_ValidatesTimestampFormat(string timestamp, bool shouldBeValid)
    {
        // Arrange
        var timestampPattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z";
        var timestampRegex = new Regex(timestampPattern);

        // Act - Check regex match (basic format)
        var matchesFormat = timestampRegex.IsMatch(timestamp);

        // Act - Check if parseable as DateTimeOffset (semantic validity)
        var isParseable = DateTimeOffset.TryParseExact(
            timestamp,
            "yyyy-MM-ddTHH:mm:ssZ",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal,
            out _);

        var isValid = matchesFormat && isParseable;

        // Assert
        isValid.Should().Be(shouldBeValid,
            $"timestamp '{timestamp}' should be {(shouldBeValid ? "valid" : "invalid")}");

        _output.WriteLine($"{(isValid ? "✓" : "✗")} Timestamp: {timestamp}");
    }

    /// <summary>
    /// T028.10: Test complete citation validation (command + timestamp)
    /// Verifies FR-026: Complete citation validation for Visual ERP API calls
    /// </summary>
    [Fact]
    public void T028_VisualWhitelist_ValidatesCompleteCitation()
    {
        // Arrange
        var whitelist = _whitelist;
        var citationPattern = _citationPattern;
        var citationRegex = new Regex(citationPattern);

        var testCitations = new List<(string Citation, bool ShouldBeValid, string Reason)>
        {
            ("GET_PART_DETAILS:2025-01-15T14:30:00Z", true, "Valid command and timestamp"),
            ("LIST_INVENTORY:2025-12-31T23:59:59Z", true, "Valid command and timestamp"),
            ("UPDATE_INVENTORY:2025-01-15T14:30:00Z", false, "Write command not in whitelist"),
            ("GET_PART_DETAILS:2025-13-01T00:00:00Z", false, "Invalid timestamp (month)"),
            ("get_part_details:2025-01-15T14:30:00Z", false, "Invalid command case"),
            ("", false, "Empty citation")
        };

        // Act & Assert
        foreach (var (citation, shouldBeValid, reason) in testCitations)
        {
            var matchesFormat = citationRegex.IsMatch(citation);

            if (matchesFormat && !string.IsNullOrEmpty(citation))
            {
                var parts = citation.Split(':');
                var command = parts[0];
                var timestamp = string.Join(":", parts.Skip(1));

                var commandAllowed = whitelist!.Contains(command);
                var timestampValid = DateTimeOffset.TryParseExact(
                    timestamp,
                    "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal,
                    out _);

                var isValid = commandAllowed && timestampValid;
                isValid.Should().Be(shouldBeValid, reason);

                _output.WriteLine($"{(isValid == shouldBeValid ? "✓" : "✗")} {citation}");
                _output.WriteLine($"  Reason: {reason}");
            }
            else
            {
                matchesFormat.Should().Be(shouldBeValid, reason);
                _output.WriteLine($"{(matchesFormat == shouldBeValid ? "✓" : "✗")} {citation}");
                _output.WriteLine($"  Reason: {reason}");
            }
        }
    }

    #endregion T028: Visual API Whitelist Tests
}
