# Quickstart: Environment and Configuration Management

**Feature**: 002-environment-and-configuration
**Purpose**: Integration test scenarios to validate end-to-end configuration functionality
**Date**: 2025-10-05

---

## Overview

This quickstart provides executable test scenarios that validate the complete Environment and Configuration Management system. Each scenario maps to user stories from the feature specification and can be used as acceptance tests.

---

## Prerequisites

### Environment Setup
1. **MAMP MySQL 5.7** running on `localhost:3306`
2. **Database created**: `mtm_template_dev`
3. **Tables created**: Run SQL from `database-schema-contract.json`
4. **Configuration files**:
   - `config/user-folders.json` exists with placeholder replaced
   - `appsettings.json` has Visual API whitelist

### Test Data

```sql
-- Insert test user
INSERT INTO Users (UserId, Username, DisplayName, IsActive)
VALUES (99, 'testuser', 'Test User', TRUE);

-- Insert test preferences
INSERT INTO UserPreferences (UserId, PreferenceKey, PreferenceValue, Category)
VALUES
  (99, 'Display.Theme', 'Dark', 'Display'),
  (99, 'Filter.DefaultLocation', 'WH-01', 'Filters');

-- Insert test feature flags
INSERT INTO FeatureFlags (FlagName, IsEnabled, Environment, RolloutPercentage, AppVersion)
VALUES
  ('Visual.UseForItems', TRUE, 'Development', 100, '1.0.0'),
  ('OfflineModeAllowed', TRUE, NULL, 50, '1.0.0');
```

---

## Scenario 1: Configuration Precedence Validation

**User Story**: As a developer, I need to verify that environment variables override user settings, which override application defaults.

### Test Steps

1. **Set environment variable**:

   ```powershell
   $env:MTM_API_TIMEOUT = "120"
   ```

2. **Run application and check config service**:

   ```csharp
   var configService = serviceProvider.GetRequiredService<IConfigurationService>();

   // Test environment variable precedence (highest)
   var timeout = configService.GetValue<int>("API:TimeoutSeconds", 30);
   Assert.Equal(120, timeout); // Env var wins

   // Test user configuration (medium)
   await configService.SetValue("API:TimeoutSeconds", 90, CancellationToken.None);
   timeout = configService.GetValue<int>("API:TimeoutSeconds", 30);
   Assert.Equal(120, timeout); // Env var still wins

   // Remove env var
   Environment.SetEnvironmentVariable("MTM_API_TIMEOUT", null);
   await configService.ReloadAsync(CancellationToken.None);

   timeout = configService.GetValue<int>("API:TimeoutSeconds", 30);
   Assert.Equal(90, timeout); // User config now wins

   // Test default fallback (lowest)
   var nonExistent = configService.GetValue<string>("NonExistent:Key", "DefaultValue");
   Assert.Equal("DefaultValue", nonExistent);
   ```

3. **Expected Result**:
   - Environment variable > User config > Default
   - No exceptions thrown
   - All assertions pass

---

## Scenario 2: User Preferences Persistence

**User Story**: As an end user, I want my display preferences saved so they persist across application restarts.

### Test Steps

1. **Load user preferences**:

   ```csharp
   var configService = serviceProvider.GetRequiredService<IConfigurationService>();
   await configService.LoadUserPreferencesAsync(userId: 99, CancellationToken.None);

   // Verify loaded preferences
   var theme = configService.GetValue<string>("Display.Theme", "Light");
   Assert.Equal("Dark", theme);

   var location = configService.GetValue<string>("Filter.DefaultLocation", "");
   Assert.Equal("WH-01", location);
   ```

2. **Update preference at runtime**:

   ```csharp
   await configService.SetValue("Display.Theme", "Light", CancellationToken.None);
   ```

3. **Verify database persistence**:

   ```sql
   SELECT PreferenceValue FROM UserPreferences
   WHERE UserId = 99 AND PreferenceKey = 'Display.Theme';
   -- Expected: "Light"
   ```

4. **Restart application and reload**:

   ```csharp
   // Simulate restart (clear in-memory cache)
   await configService.ReloadAsync(CancellationToken.None);
   await configService.LoadUserPreferencesAsync(userId: 99, CancellationToken.None);

   var theme = configService.GetValue<string>("Display.Theme", "Dark");
   Assert.Equal("Light", theme); // Persisted value loaded
   ```

5. **Expected Result**:
   - Preference saved to database
   - Value persists across restart
   - OnConfigurationChanged event fired

---

## Scenario 3: Credential Recovery Flow

**User Story**: As an end user, when my saved credentials can't be retrieved, I need a way to re-enter them without application failure.

### Test Steps

1. **Simulate credential storage corruption**:

   ```csharp
   var secretsService = serviceProvider.GetRequiredService<ISecretsService>();

   // Manually corrupt credential (platform-specific - may need to delete from Credential Manager)
   // OR mock the service to throw CryptographicException
   ```

2. **Attempt credential retrieval**:

   ```csharp
   string? username = null;
   bool dialogShown = false;

   try
   {
       username = await secretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
   }
   catch (CryptographicException ex)
   {
       // Trigger CredentialDialogView
       var dialogViewModel = new CredentialDialogViewModel(secretsService, logger);
       dialogViewModel.DialogTitle = "Credentials Required";
       dialogViewModel.DialogMessage = "Your saved credentials could not be retrieved. Please enter them again.";

       var dialog = new CredentialDialogView { DataContext = dialogViewModel };
       var result = await dialog.ShowDialog<bool>(mainWindow);

       dialogShown = true;

       if (result)
       {
           // Credentials re-stored successfully
           username = await secretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
       }
   }

   Assert.True(dialogShown);
   Assert.NotNull(username);
   ```

3. **Expected Result**:
   - CryptographicException caught gracefully
   - CredentialDialogView shown with clear message
   - User can re-enter credentials
   - Credentials successfully re-stored
   - Application continues normally

---

## Scenario 4: Feature Flag Deterministic Rollout

**User Story**: As a product manager, I need feature flags to show consistently for the same user (not randomly changing).

### Test Steps

1. **Register feature flag with 50% rollout**:

   ```csharp
   var flagEvaluator = serviceProvider.GetRequiredService<FeatureFlagEvaluator>();

   var flag = new FeatureFlag
   {
       Name = "TestFeature.Rollout50",
       IsEnabled = true,
       Environment = "",
       RolloutPercentage = 50
   };
   flagEvaluator.RegisterFlag(flag);
   ```

2. **Evaluate for same user multiple times**:

   ```csharp
   int userId = 42;

   var result1 = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", userId);
   var result2 = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", userId);
   var result3 = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", userId);

   Assert.Equal(result1, result2);
   Assert.Equal(result2, result3);
   // Same user always gets same result (deterministic)
   ```

3. **Evaluate for different users**:

   ```csharp
   var results = new Dictionary<int, bool>();

   for (int userId = 1; userId <= 100; userId++)
   {
       var enabled = await flagEvaluator.IsEnabledAsync("TestFeature.Rollout50", userId);
       results[userId] = enabled;
   }

   int enabledCount = results.Values.Count(e => e);

   // Should be approximately 50% (allow ±10% variance for small sample)
   Assert.InRange(enabledCount, 40, 60);
   ```

4. **Expected Result**:
   - Same user always gets same result (deterministic)
   - Distribution approximates rollout percentage
   - No random fluctuations

---

## Scenario 5: Configuration Error Notification

**User Story**: As an end user, I need clear, non-technical error messages when configuration issues occur.

### Test Steps

1. **Trigger non-critical error (invalid type)**:

   ```csharp
   // Set environment variable to invalid integer
   Environment.SetEnvironmentVariable("MTM_API_TIMEOUT", "NotANumber");

   var configService = serviceProvider.GetRequiredService<IConfigurationService>();
   var errorService = serviceProvider.GetRequiredService<ErrorNotificationService>();

   bool errorOccurred = false;
   errorService.OnErrorOccurred += (sender, error) =>
   {
       errorOccurred = true;
       Assert.Equal(ErrorSeverity.Warning, error.Severity);
       Assert.Contains("timeout", error.Message.ToLower());
   };

   // Should use default value and log warning
   var timeout = configService.GetValue<int>("API:TimeoutSeconds", 30);
   Assert.Equal(30, timeout); // Default used
   Assert.True(errorOccurred);
   ```

2. **Trigger critical error (database connection)**:

   ```csharp
   // Simulate database unavailable (stop MAMP MySQL)

   var configService = serviceProvider.GetRequiredService<IConfigurationService>();

   try
   {
       await configService.LoadUserPreferencesAsync(userId: 99, CancellationToken.None);
   }
   catch (DbException ex)
   {
       var error = new ConfigurationError
       {
           Key = "Database:Connection",
           Message = "Could not connect to the database. Please check that MySQL is running.",
           Severity = ErrorSeverity.Critical,
           Timestamp = DateTimeOffset.UtcNow,
           UserAction = "Start MAMP MySQL and try again."
       };

       var dialogShown = await errorService.ShowModalDialogAsync(error, CancellationToken.None);
       Assert.True(dialogShown);
   }
   ```

3. **Expected Result**:
   - Non-critical errors → status bar warning (non-blocking)
   - Critical errors → modal dialog (blocking)
   - User-friendly language (no technical jargon)
   - Clear user action guidance

---

## Scenario 6: Feature Flag Environment Filtering

**User Story**: As a system administrator, I want to enable features only in specific environments (e.g., Development).

### Test Steps

1. **Set environment to Development**:

   ```powershell
   $env:MTM_ENVIRONMENT = "Development"
   ```

2. **Register environment-specific flag**:

   ```csharp
   var flagEvaluator = serviceProvider.GetRequiredService<FeatureFlagEvaluator>();

   var devFlag = new FeatureFlag
   {
       Name = "Debug.ShowSqlQueries",
       IsEnabled = true,
       Environment = "Development",
       RolloutPercentage = 100
   };
   flagEvaluator.RegisterFlag(devFlag);

   var prodFlag = new FeatureFlag
   {
       Name = "Production.AdvancedFeature",
       IsEnabled = true,
       Environment = "Production",
       RolloutPercentage = 100
   };
   flagEvaluator.RegisterFlag(prodFlag);
   ```

3. **Evaluate flags**:

   ```csharp
   var devEnabled = await flagEvaluator.IsEnabledAsync("Debug.ShowSqlQueries");
   var prodEnabled = await flagEvaluator.IsEnabledAsync("Production.AdvancedFeature");

   Assert.True(devEnabled);   // Development flag enabled in Development env
   Assert.False(prodEnabled);  // Production flag disabled in Development env
   ```

4. **Change environment**:

   ```powershell
   $env:MTM_ENVIRONMENT = "Production"
   ```

5. **Re-evaluate flags**:

   ```csharp
   await configService.ReloadAsync(CancellationToken.None);

   var devEnabled = await flagEvaluator.IsEnabledAsync("Debug.ShowSqlQueries");
   var prodEnabled = await flagEvaluator.IsEnabledAsync("Production.AdvancedFeature");

   Assert.False(devEnabled);  // Development flag disabled in Production env
   Assert.True(prodEnabled);   // Production flag enabled in Production env
   ```

6. **Expected Result**:
   - Flags respect environment constraints
   - Environment detection follows precedence (MTM_ENVIRONMENT → ASPNETCORE_ENVIRONMENT → build config)
   - Mismatched environment returns false

---

## Scenario 7: Visual API Command Whitelist Enforcement

**User Story**: As a security administrator, I need to ensure only read-only Visual API commands are allowed.

### Test Steps

1. **Load whitelist from appsettings.json**:

   ```csharp
   var configuration = serviceProvider.GetRequiredService<IConfiguration>();
   var allowedCommands = configuration.GetSection("Visual:AllowedCommands").Get<string[]>();

   Assert.NotNull(allowedCommands);
   Assert.Contains("GET_PART_DETAILS", allowedCommands);
   Assert.DoesNotContain("UPDATE_INVENTORY", allowedCommands); // Write command blocked
   ```

2. **Test command validation**:

   ```csharp
   bool IsCommandAllowed(string command)
   {
       return allowedCommands.Contains(command, StringComparer.OrdinalIgnoreCase);
   }

   Assert.True(IsCommandAllowed("GET_PART_DETAILS"));
   Assert.True(IsCommandAllowed("LIST_INVENTORY"));
   Assert.False(IsCommandAllowed("UPDATE_INVENTORY"));
   Assert.False(IsCommandAllowed("DELETE_PART"));
   ```

3. **Test citation requirement**:

   ```csharp
   var requireCitation = configuration.GetValue<bool>("Visual:RequireCitation");
   Assert.True(requireCitation);

   // Citation format: "Reference-{File Name} - {Chapter/Section/Page}"
   var validCitation = "Reference-Visual_API_Toolkit_Manual - Chapter 3, Page 42";
   var citationPattern = @"^Reference-[\w\s]+ - [\w\s/,]+$";

   Assert.Matches(citationPattern, validCitation);
   ```

4. **Expected Result**:
   - Only whitelisted commands allowed
   - Write commands explicitly blocked
   - Citation requirement enforced

---

## Performance Validation

### Configuration Lookup (<10ms)

```csharp
var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < 1000; i++)
{
    var value = configService.GetValue<int>("API:TimeoutSeconds", 30);
}
stopwatch.Stop();

var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;
Assert.True(avgTime < 10, $"Average lookup time {avgTime}ms exceeds 10ms target");
```

### Credential Retrieval (<100ms)

```csharp
var stopwatch = Stopwatch.StartNew();
var username = await secretsService.RetrieveSecretAsync("Visual.Username", CancellationToken.None);
stopwatch.Stop();

Assert.True(stopwatch.ElapsedMilliseconds < 100, $"Credential retrieval took {stopwatch.ElapsedMilliseconds}ms (target <100ms)");
```

### Feature Flag Evaluation (<5ms)

```csharp
var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < 1000; i++)
{
    var enabled = await flagEvaluator.IsEnabledAsync("Visual.UseForItems", userId: 42);
}
stopwatch.Stop();

var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;
Assert.True(avgTime < 5, $"Average flag evaluation {avgTime}ms exceeds 5ms target");
```

---

## Cleanup

After running tests, clean up test data:

```sql
DELETE FROM UserPreferences WHERE UserId = 99;
DELETE FROM FeatureFlags WHERE FlagName LIKE 'TestFeature%';
DELETE FROM Users WHERE UserId = 99;
```

---

## Success Criteria

- ✅ All 7 scenarios pass without errors
- ✅ Performance targets met (config <10ms, secrets <100ms, flags <5ms)
- ✅ No exceptions thrown during normal operation
- ✅ Configuration precedence respected
- ✅ Credential recovery flow works correctly
- ✅ Feature flags are deterministic for same user
- ✅ Error notifications use appropriate severity
- ✅ Environment filtering works as expected
- ✅ Visual API whitelist enforced

---

**Last Updated**: 2025-10-05
