# Settings Management Contract Tests

**Feature**: 005 - Manufacturing Application Modernization (Phase 2: Settings Management)
**Date**: 2025-10-09
**Purpose**: Define contract tests for Settings UI interactions with IConfigurationService

---

## Overview

These contract tests validate the Settings Management UI's interactions with the underlying configuration system. They ensure that:
- Settings can be retrieved by key with proper type safety
- Setting validation works correctly before persistence
- Settings export/import preserves data integrity while filtering sensitive values

**Related Files**:
- Implementation: `MTM_Template_Application/Services/Configuration/IConfigurationService.cs`
- ViewModels: `MTM_Template_Application/ViewModels/Settings/SettingsViewModel.cs`
- Views: `MTM_Template_Application/Views/Settings/SettingsWindow.axaml`
- Test Implementation: `tests/contract/SettingsContractTests.cs`

---

## CT-SETTINGS-001: Get Setting By Key

**Purpose**: Verify that settings can be retrieved by key with proper type conversion and default value handling.

**Test Scenario**: Get Database Connection String Setting

**Request**:
```csharp
// Request parameters
string settingKey = "Database:ConnectionString";
string defaultValue = "Server=localhost;Port=3306;Database=mtm_template_dev;User=root";
```

**Expected Response**:
```csharp
// Response structure
SettingDefinition {
    SettingKey = "Database:ConnectionString",
    SettingValue = "Server=localhost;Port=3306;Database=mtm_template_dev;User=root;Password=root",
    SettingType = SettingValueType.ConnectionString,
    Category = SettingCategory.Database,
    DisplayName = "MySQL Connection String",
    Description = "Connection string for MAMP MySQL database",
    ValidationRules = ["NotEmpty", "ValidConnectionStringFormat"],
    DefaultValue = "Server=localhost;Port=3306;Database=mtm_template_dev;User=root",
    IsReadOnly = false,
    IsSensitive = true,
    LastModified = "2025-10-09T10:30:00Z",
    ModifiedBy = "admin"
}
```

**Validation Rules**:
- `SettingKey` MUST match requested key exactly (case-sensitive)
- `SettingValue` MUST be returned as string (even for numeric types)
- `SettingType` MUST be valid enum value (String, Int, Decimal, Boolean, Enum, FilePath, ConnectionString, URL)
- `Category` MUST be valid SettingCategory enum
- `IsSensitive` MUST be true for credentials/passwords
- `LastModified` MUST be valid ISO 8601 datetime
- If setting not found, return default value with `IsReadOnly = false`

**Contract Test Implementation**:
```csharp
[Fact]
public async Task GetSettingByKey_WithConnectionString_ReturnsValidSettingDefinition()
{
    // Arrange
    var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
    var settingKey = "Database:ConnectionString";

    // Act
    var setting = await configService.GetSettingAsync(settingKey, CancellationToken.None);

    // Assert
    setting.Should().NotBeNull();
    setting.SettingKey.Should().Be(settingKey);
    setting.SettingType.Should().Be(SettingValueType.ConnectionString);
    setting.Category.Should().Be(SettingCategory.Database);
    setting.IsSensitive.Should().BeTrue();
    setting.SettingValue.Should().Contain("Server=").And.Contain("Database=");
}
```

---

## CT-SETTINGS-002: Save Setting With Validation

**Purpose**: Verify that settings are validated before persistence and validation errors are returned with actionable messages.

**Test Scenario**: Save Invalid Database Connection String

**Request**:
```csharp
// Request parameters
SettingDefinition settingToSave = new()
{
    SettingKey = "Database:ConnectionString",
    SettingValue = "InvalidConnectionString", // Missing Server= and Database=
    SettingType = SettingValueType.ConnectionString,
    Category = SettingCategory.Database
};
```

**Expected Response (Validation Failure)**:
```csharp
// Response structure
SaveSettingResult {
    Success = false,
    ValidationErrors = [
        {
            PropertyName = "SettingValue",
            ErrorMessage = "Connection string must contain 'Server=' parameter",
            Severity = ValidationSeverity.Error
        },
        {
            PropertyName = "SettingValue",
            ErrorMessage = "Connection string must contain 'Database=' parameter",
            Severity = ValidationSeverity.Error
        }
    ],
    SettingKey = "Database:ConnectionString",
    Timestamp = "2025-10-09T10:31:00Z"
}
```

**Validation Rules**:
- `Success` MUST be false if any validation errors exist
- `ValidationErrors` array MUST contain all validation failures
- Each error MUST have `PropertyName`, `ErrorMessage`, and `Severity`
- Connection strings MUST contain "Server=" and "Database=" parameters
- URLs MUST be valid URI format
- Numeric types MUST parse correctly to target type
- File paths MUST be valid absolute or relative paths

**Contract Test Implementation**:
```csharp
[Fact]
public async Task SaveSetting_WithInvalidConnectionString_ReturnsValidationErrors()
{
    // Arrange
    var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
    var invalidSetting = new SettingDefinition
    {
        SettingKey = "Database:ConnectionString",
        SettingValue = "InvalidConnectionString",
        SettingType = SettingValueType.ConnectionString,
        Category = SettingCategory.Database
    };

    // Act
    var result = await configService.SaveSettingAsync(invalidSetting, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse();
    result.ValidationErrors.Should().NotBeEmpty();
    result.ValidationErrors.Should().Contain(e =>
        e.PropertyName == "SettingValue" &&
        e.ErrorMessage.Contains("Server="));
    result.ValidationErrors.Should().Contain(e =>
        e.PropertyName == "SettingValue" &&
        e.ErrorMessage.Contains("Database="));
}
```

**Test Scenario**: Save Valid Setting

**Request**:
```csharp
// Request parameters
SettingDefinition settingToSave = new()
{
    SettingKey = "UI:Theme",
    SettingValue = "Dark",
    SettingType = SettingValueType.Enum,
    Category = SettingCategory.UI
};
```

**Expected Response (Success)**:
```csharp
// Response structure
SaveSettingResult {
    Success = true,
    ValidationErrors = [], // Empty array
    SettingKey = "UI:Theme",
    Timestamp = "2025-10-09T10:32:00Z",
    PreviousValue = "Light",
    NewValue = "Dark"
}
```

**Contract Test Implementation**:
```csharp
[Fact]
public async Task SaveSetting_WithValidTheme_SuccessfullyPersists()
{
    // Arrange
    var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
    var validSetting = new SettingDefinition
    {
        SettingKey = "UI:Theme",
        SettingValue = "Dark",
        SettingType = SettingValueType.Enum,
        Category = SettingCategory.UI
    };

    // Act
    var result = await configService.SaveSettingAsync(validSetting, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue();
    result.ValidationErrors.Should().BeEmpty();
    result.SettingKey.Should().Be("UI:Theme");
    result.NewValue.Should().Be("Dark");
}
```

---

## CT-SETTINGS-003: Export/Import Settings JSON

**Purpose**: Verify that settings can be exported to JSON format with sensitive values filtered, and imported with proper validation and conflict resolution.

**Test Scenario**: Export Settings to JSON

**Request**:
```csharp
// Request parameters
ExportSettingsRequest {
    IncludeCategories = [SettingCategory.General, SettingCategory.Database, SettingCategory.UI],
    ExcludeSensitive = true, // Filter passwords, tokens, keys
    ExportFormat = ExportFormat.JSON
}
```

**Expected Response**:
```json
{
  "exportMetadata": {
    "timestamp": "2025-10-09T10:33:00Z",
    "appVersion": "1.0.0",
    "exportedBy": "admin",
    "categories": ["General", "Database", "UI"],
    "settingCount": 15
  },
  "settings": [
    {
      "settingKey": "UI:Theme",
      "settingValue": "Dark",
      "settingType": "Enum",
      "category": "UI"
    },
    {
      "settingKey": "Database:ConnectionString",
      "settingValue": "***FILTERED***",
      "settingType": "ConnectionString",
      "category": "Database",
      "note": "Sensitive value excluded from export"
    },
    {
      "settingKey": "General:AppLanguage",
      "settingValue": "en-US",
      "settingType": "String",
      "category": "General"
    }
  ]
}
```

**Validation Rules**:
- `exportMetadata.timestamp` MUST be valid ISO 8601 datetime
- `exportMetadata.settingCount` MUST match length of settings array
- Settings with `IsSensitive = true` MUST show "***FILTERED***" when `ExcludeSensitive = true`
- JSON MUST be valid and parseable
- Export MUST only include requested categories

**Contract Test Implementation**:
```csharp
[Fact]
public async Task ExportSettings_WithSensitiveFiltering_FiltersPasswords()
{
    // Arrange
    var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
    var exportRequest = new ExportSettingsRequest
    {
        IncludeCategories = [SettingCategory.Database],
        ExcludeSensitive = true,
        ExportFormat = ExportFormat.JSON
    };

    // Act
    var json = await configService.ExportSettingsAsync(exportRequest, CancellationToken.None);
    var export = JsonSerializer.Deserialize<SettingsExport>(json);

    // Assert
    export.Should().NotBeNull();
    export.ExportMetadata.SettingCount.Should().Be(export.Settings.Count);

    var connectionStringSetting = export.Settings.FirstOrDefault(s =>
        s.SettingKey == "Database:ConnectionString");
    connectionStringSetting.Should().NotBeNull();
    connectionStringSetting.SettingValue.Should().Be("***FILTERED***");
}
```

**Test Scenario**: Import Settings from JSON

**Request**:
```csharp
// Request parameters
ImportSettingsRequest {
    JsonContent = "{ /* exported JSON from above */ }",
    ConflictResolution = ConflictResolution.Overwrite, // or Merge, Skip
    ValidateBeforeImport = true
}
```

**Expected Response**:
```csharp
// Response structure
ImportSettingsResult {
    Success = true,
    ImportedCount = 12,
    SkippedCount = 3, // Sensitive settings with "***FILTERED***"
    ValidationErrors = [],
    ConflictResolutions = [
        {
            SettingKey = "UI:Theme",
            ExistingValue = "Light",
            ImportedValue = "Dark",
            Resolution = "Overwritten"
        }
    ],
    Timestamp = "2025-10-09T10:34:00Z"
}
```

**Validation Rules**:
- `Success` MUST be false if any critical validation errors occur
- `ImportedCount + SkippedCount` MUST equal total settings in JSON
- Settings with "***FILTERED***" values MUST be skipped (not imported)
- Conflict resolution MUST follow specified strategy (Overwrite, Merge, Skip)
- All imported settings MUST pass validation before persistence

**Contract Test Implementation**:
```csharp
[Fact]
public async Task ImportSettings_WithFilteredValues_SkipsSensitiveSettings()
{
    // Arrange
    var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
    var json = @"{
        ""exportMetadata"": { ""timestamp"": ""2025-10-09T10:33:00Z"", ""settingCount"": 2 },
        ""settings"": [
            { ""settingKey"": ""UI:Theme"", ""settingValue"": ""Dark"", ""settingType"": ""Enum"" },
            { ""settingKey"": ""Database:ConnectionString"", ""settingValue"": ""***FILTERED***"", ""settingType"": ""ConnectionString"" }
        ]
    }";
    var importRequest = new ImportSettingsRequest
    {
        JsonContent = json,
        ConflictResolution = ConflictResolution.Overwrite,
        ValidateBeforeImport = true
    };

    // Act
    var result = await configService.ImportSettingsAsync(importRequest, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue();
    result.ImportedCount.Should().Be(1); // Only UI:Theme
    result.SkippedCount.Should().Be(1); // ConnectionString with ***FILTERED***
    result.ValidationErrors.Should().BeEmpty();
}
```

---

## Summary

**Total Contract Tests**: 3
- CT-SETTINGS-001: Get setting by key (1 test scenario)
- CT-SETTINGS-002: Save setting with validation (2 test scenarios: invalid + valid)
- CT-SETTINGS-003: Export/import settings (2 test scenarios: export + import)

**Coverage**:
- ✅ Setting retrieval with type safety
- ✅ Setting validation before persistence
- ✅ Sensitive value filtering in exports
- ✅ Import conflict resolution
- ✅ Error handling and validation messages

**Implementation Location**: `tests/contract/SettingsContractTests.cs`

**Related Functional Requirements**:
- FR-036: Settings window with tabbed navigation
- FR-037: Display all 60+ settings
- FR-038: Validate setting values before save
- FR-039: Persist changes to UserPreferences table
- FR-041: Export settings to JSON with filtered sensitive values
- FR-042: Import settings from JSON with validation
