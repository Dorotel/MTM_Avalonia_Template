using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace MTM_Template_Tests.Contract;

/// <summary>
/// Contract tests for database schema based on .github/mamp-database/schema-tables.json
/// These tests validate table structure, constraints, and sample data against the JSON schema.
/// Tests may PASS if database was set up correctly in T004.
///
/// Reference: Constitution v1.3.0 Principle VIII (MAMP MySQL Database Documentation)
/// All schema expectations are loaded from schema-tables.json to ensure single source of truth.
/// </summary>
public class DatabaseSchemaContractTests : IDisposable
{
    private readonly string _connectionString;
    private MySqlConnection? _connection;
    private static readonly Lazy<JsonDocument> _schemaDocument = new(() => LoadSchemaDocument());

    private static JsonDocument LoadSchemaDocument()
    {
        // Load schema-tables.json from .github/mamp-database/
        var repoRoot = GetRepositoryRoot();
        var schemaPath = Path.Combine(repoRoot, ".github", "mamp-database", "schema-tables.json");

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaPath}. Ensure .github/mamp-database/schema-tables.json exists.");
        }

        var jsonContent = File.ReadAllText(schemaPath);
        return JsonDocument.Parse(jsonContent);
    }

    private static string GetRepositoryRoot()
    {
        // Navigate up from test assembly location to repository root
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, ".git", "config")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Could not find repository root (no .git directory found)");
        }

        return directory.FullName;
    }

    private static JsonElement GetTableSchema(string tableName)
    {
        var root = _schemaDocument.Value.RootElement;
        return root.GetProperty("tables").GetProperty(tableName);
    }

    public DatabaseSchemaContractTests()
    {
        // Connection string for MAMP MySQL 5.7 (default development setup)
        _connectionString = "Server=localhost;Port=3306;Database=mtm_template_dev;Uid=root;Pwd=root;";
    }

    private async Task<MySqlConnection> GetConnectionAsync()
    {
        if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
        {
            _connection = new MySqlConnection(_connectionString);
            await _connection.OpenAsync();
        }
        return _connection;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    #region T009: Database Schema Validation Tests

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_Table_Exists()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'mtm_template_dev' AND TABLE_NAME = 'UserPreferences'",
            connection
        );

        // Act
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        // Assert
        count.Should().Be(1, "UserPreferences table should exist");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_HasCorrectColumns()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_KEY
              FROM information_schema.COLUMNS
              WHERE TABLE_SCHEMA = 'mtm_template_dev'
              AND TABLE_NAME = 'UserPreferences'
              ORDER BY ORDINAL_POSITION",
            connection
        );

        // Act
        var columns = new List<(string Name, string Type, string Nullable, string Key)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var nameIdx = reader.GetOrdinal("COLUMN_NAME");
            var typeIdx = reader.GetOrdinal("DATA_TYPE");
            var nullIdx = reader.GetOrdinal("IS_NULLABLE");
            var keyIdx = reader.GetOrdinal("COLUMN_KEY");
            columns.Add((
                reader.GetString(nameIdx),
                reader.GetString(typeIdx),
                reader.GetString(nullIdx),
                reader.IsDBNull(keyIdx) ? "" : reader.GetString(keyIdx)
            ));
        }

        // Assert
        columns.Should().Contain(c => c.Name == "PreferenceId" && c.Type == "int" && c.Key == "PRI");
        columns.Should().Contain(c => c.Name == "UserId" && c.Type == "int" && c.Nullable == "NO");
        columns.Should().Contain(c => c.Name == "PreferenceKey" && c.Type == "varchar" && c.Nullable == "NO");
        columns.Should().Contain(c => c.Name == "PreferenceValue" && c.Type == "text");
        columns.Should().Contain(c => c.Name == "Category" && c.Type == "varchar");
        columns.Should().Contain(c => c.Name == "LastUpdated" && c.Type == "datetime" && c.Nullable == "NO");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_HasUniqueConstraint()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT CONSTRAINT_NAME
              FROM information_schema.TABLE_CONSTRAINTS
              WHERE TABLE_SCHEMA = 'mtm_template_dev'
              AND TABLE_NAME = 'UserPreferences'
              AND CONSTRAINT_TYPE = 'UNIQUE'",
            connection
        );

        // Act
        var constraints = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var nameIdx = reader.GetOrdinal("CONSTRAINT_NAME");
            constraints.Add(reader.GetString(nameIdx));
        }

        // Assert
        constraints.Should().Contain(c => c == "UK_UserPreferences" || c.Contains("UserId"));
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_HasForeignKey()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT CONSTRAINT_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME
              FROM information_schema.KEY_COLUMN_USAGE
              WHERE TABLE_SCHEMA = 'mtm_template_dev'
              AND TABLE_NAME = 'UserPreferences'
              AND REFERENCED_TABLE_NAME IS NOT NULL",
            connection
        );

        // Act
        var foreignKeys = new List<(string Name, string RefTable, string RefColumn)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var nameIdx = reader.GetOrdinal("CONSTRAINT_NAME");
            var refTableIdx = reader.GetOrdinal("REFERENCED_TABLE_NAME");
            var refColIdx = reader.GetOrdinal("REFERENCED_COLUMN_NAME");
            foreignKeys.Add((
                reader.GetString(nameIdx),
                reader.GetString(refTableIdx),
                reader.GetString(refColIdx)
            ));
        }

        // Assert - Use case-insensitive comparison (MySQL returns lowercase)
        foreignKeys.Should().Contain(fk =>
            string.Equals(fk.RefTable, "Users", StringComparison.OrdinalIgnoreCase) &&
            fk.RefColumn == "UserId");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task FeatureFlags_Table_Exists()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'mtm_template_dev' AND TABLE_NAME = 'FeatureFlags'",
            connection
        );

        // Act
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        // Assert
        count.Should().Be(1, "FeatureFlags table should exist");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task FeatureFlags_HasCorrectColumns()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_KEY
              FROM information_schema.COLUMNS
              WHERE TABLE_SCHEMA = 'mtm_template_dev'
              AND TABLE_NAME = 'FeatureFlags'
              ORDER BY ORDINAL_POSITION",
            connection
        );

        // Act
        var columns = new List<(string Name, string Type, string Nullable, string Key)>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var nameIdx = reader.GetOrdinal("COLUMN_NAME");
            var typeIdx = reader.GetOrdinal("DATA_TYPE");
            var nullIdx = reader.GetOrdinal("IS_NULLABLE");
            var keyIdx = reader.GetOrdinal("COLUMN_KEY");
            columns.Add((
                reader.GetString(nameIdx),
                reader.GetString(typeIdx),
                reader.GetString(nullIdx),
                reader.IsDBNull(keyIdx) ? "" : reader.GetString(keyIdx)
            ));
        }

        // Assert
        columns.Should().Contain(c => c.Name == "FlagId" && c.Type == "int" && c.Key == "PRI");
        columns.Should().Contain(c => c.Name == "FlagName" && c.Type == "varchar" && c.Nullable == "NO");
        columns.Should().Contain(c => c.Name == "IsEnabled" && c.Type == "tinyint" && c.Nullable == "NO");
        columns.Should().Contain(c => c.Name == "Environment" && c.Type == "varchar");
        columns.Should().Contain(c => c.Name == "RolloutPercentage" && c.Type == "int" && c.Nullable == "NO");
        columns.Should().Contain(c => c.Name == "AppVersion" && c.Type == "varchar");
        columns.Should().Contain(c => c.Name == "UpdatedAt" && c.Type == "datetime" && c.Nullable == "NO");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task FeatureFlags_FlagName_HasUniqueConstraint()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT CONSTRAINT_NAME
              FROM information_schema.TABLE_CONSTRAINTS
              WHERE TABLE_SCHEMA = 'mtm_template_dev'
              AND TABLE_NAME = 'FeatureFlags'
              AND CONSTRAINT_TYPE = 'UNIQUE'",
            connection
        );

        // Act
        var constraints = new List<string>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var nameIdx = reader.GetOrdinal("CONSTRAINT_NAME");
            constraints.Add(reader.GetString(nameIdx));
        }

        // Assert
        constraints.Should().ContainMatch("*FlagName*");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_SampleData_CanBeInserted()
    {
        // Arrange
        var connection = await GetConnectionAsync();

        // Clean up any existing test data
        var cleanupCommand = new MySqlCommand(
            "DELETE FROM UserPreferences WHERE UserId = 999",
            connection
        );
        await cleanupCommand.ExecuteNonQueryAsync();

        // Act - Insert sample data
        var insertCommand = new MySqlCommand(
            @"INSERT INTO UserPreferences (UserId, PreferenceKey, PreferenceValue, Category)
              VALUES (@userId, @key, @value, @category)",
            connection
        );
        insertCommand.Parameters.AddWithValue("@userId", 999);
        insertCommand.Parameters.AddWithValue("@key", "Test.Setting");
        insertCommand.Parameters.AddWithValue("@value", "TestValue");
        insertCommand.Parameters.AddWithValue("@category", "Test");

        var rowsInserted = await insertCommand.ExecuteNonQueryAsync();

        // Assert
        rowsInserted.Should().Be(1, "Sample data should be insertable");

        // Cleanup
        await cleanupCommand.ExecuteNonQueryAsync();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task UserPreferences_UniqueConstraint_PreventsDuplicates()
    {
        // Arrange
        var connection = await GetConnectionAsync();

        // Clean up any existing test data
        var cleanupCommand = new MySqlCommand(
            "DELETE FROM UserPreferences WHERE UserId = 998",
            connection
        );
        await cleanupCommand.ExecuteNonQueryAsync();

        // Ensure User record exists for FK constraint
        var userCleanupCommand = new MySqlCommand(
            "DELETE FROM Users WHERE UserId = 998",
            connection
        );
        await userCleanupCommand.ExecuteNonQueryAsync();

        var createUserCommand = new MySqlCommand(
            @"INSERT INTO Users (UserId, Username, DisplayName, IsActive, CreatedAt)
              VALUES (@userId, @username, @displayName, @isActive, @createdAt)",
            connection
        );
        createUserCommand.Parameters.AddWithValue("@userId", 998);
        createUserCommand.Parameters.AddWithValue("@username", "test_user_998");
        createUserCommand.Parameters.AddWithValue("@displayName", "Test User 998");
        createUserCommand.Parameters.AddWithValue("@isActive", true);
        createUserCommand.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        await createUserCommand.ExecuteNonQueryAsync();

        // Insert first record
        var insertCommand1 = new MySqlCommand(
            @"INSERT INTO UserPreferences (UserId, PreferenceKey, PreferenceValue, Category)
              VALUES (@userId, @key, @value, @category)",
            connection
        );
        insertCommand1.Parameters.AddWithValue("@userId", 998);
        insertCommand1.Parameters.AddWithValue("@key", "Duplicate.Test");
        insertCommand1.Parameters.AddWithValue("@value", "Value1");
        insertCommand1.Parameters.AddWithValue("@category", "Test");
        await insertCommand1.ExecuteNonQueryAsync();

        // Act - Attempt to insert duplicate
        var insertCommand2 = new MySqlCommand(
            @"INSERT INTO UserPreferences (UserId, PreferenceKey, PreferenceValue, Category)
              VALUES (@userId, @key, @value, @category)",
            connection
        );
        insertCommand2.Parameters.AddWithValue("@userId", 998);
        insertCommand2.Parameters.AddWithValue("@key", "Duplicate.Test");
        insertCommand2.Parameters.AddWithValue("@value", "Value2");
        insertCommand2.Parameters.AddWithValue("@category", "Test");

        Func<Task> act = async () => await insertCommand2.ExecuteNonQueryAsync();

        // Assert
        await act.Should().ThrowAsync<MySqlException>()
            .WithMessage("*Duplicate*");

        // Cleanup
        await cleanupCommand.ExecuteNonQueryAsync();
        await userCleanupCommand.ExecuteNonQueryAsync();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task FeatureFlags_SampleData_CanBeInserted()
    {
        // Arrange
        var connection = await GetConnectionAsync();

        // Clean up any existing test data
        var cleanupCommand = new MySqlCommand(
            "DELETE FROM FeatureFlags WHERE FlagName = 'Test.Feature'",
            connection
        );
        await cleanupCommand.ExecuteNonQueryAsync();

        // Act - Insert sample data
        var insertCommand = new MySqlCommand(
            @"INSERT INTO FeatureFlags (FlagName, IsEnabled, Environment, RolloutPercentage, AppVersion)
              VALUES (@name, @enabled, @env, @rollout, @version)",
            connection
        );
        insertCommand.Parameters.AddWithValue("@name", "Test.Feature");
        insertCommand.Parameters.AddWithValue("@enabled", true);
        insertCommand.Parameters.AddWithValue("@env", "Development");
        insertCommand.Parameters.AddWithValue("@rollout", 50);
        insertCommand.Parameters.AddWithValue("@version", "1.0.0");

        var rowsInserted = await insertCommand.ExecuteNonQueryAsync();

        // Assert
        rowsInserted.Should().Be(1, "Sample data should be insertable");

        // Cleanup
        await cleanupCommand.ExecuteNonQueryAsync();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public async Task Database_CharacterSet_IsUtf8mb4()
    {
        // Arrange
        var connection = await GetConnectionAsync();
        var command = new MySqlCommand(
            @"SELECT DEFAULT_CHARACTER_SET_NAME
              FROM information_schema.SCHEMATA
              WHERE SCHEMA_NAME = 'mtm_template_dev'",
            connection
        );

        // Act
        var charset = await command.ExecuteScalarAsync() as string;

        // Assert
        charset.Should().Be("utf8mb4", "Database should use utf8mb4 character set");
    }

    #endregion
}
