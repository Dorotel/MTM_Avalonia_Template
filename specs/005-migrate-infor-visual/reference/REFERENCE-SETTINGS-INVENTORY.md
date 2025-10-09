# Reference: Settings Inventory

**Date**: October 8, 2025

**Purpose**: Complete catalog of all application settings requiring UI management

---

## Configuration Architecture

**Service**: `IConfigurationService`

**Precedence** (highest to lowest):

1. Environment Variables (`MTM_*`, `ASPNETCORE_*`, `DOTNET_*`)
2. User Configuration (runtime `SetValue`, persisted to MySQL `UserPreferences` table)
3. Application Defaults (hard-coded fallbacks)

**Persistence**:

- User preferences → MySQL `UserPreferences` table
- Offline cache → Local JSON file
- Environment variables → Read once at startup

---

## Settings Categories

### 1. Visual ERP Integration

| Setting Key                | Type    | Default               | Description                         | Validation                       |
| -------------------------- | ------- | --------------------- | ----------------------------------- | -------------------------------- |
| `Visual:BaseUrl`           | string  | (empty)               | Base URL for Visual ERP API         | Must be valid HTTPS URL          |
| `Visual:Timeout`           | int     | 30                    | API request timeout (seconds)       | Range: 5-300                     |
| `Visual:RetryCount`        | int     | 3                     | Number of retry attempts            | Range: 0-10                      |
| `Visual:RetryDelay`        | int     | 1000                  | Delay between retries (ms)          | Range: 100-10000                 |
| `Visual:UseMockData`       | bool    | false                 | Use mock data instead of API        | -                                |
| `Visual:EnableCaching`     | bool    | true                  | Enable response caching             | -                                |
| `Visual:CacheExpiryHours`  | int     | 24                    | Cache expiry duration (hours)       | Range: 1-168 (7 days)            |
| `Visual:MaxConcurrent`     | int     | 5                     | Max concurrent API requests         | Range: 1-20                      |
| `Visual:UseForItems`       | bool    | true                  | Use Visual for item data            | Feature flag sync                |
| `Visual:UseForWorkOrders`  | bool    | true                  | Use Visual for work order data      | Feature flag sync                |
| `Visual:UseForInventory`   | bool    | true                  | Use Visual for inventory data       | Feature flag sync                |
| `Visual:EnableBarcode`     | bool    | true                  | Enable barcode scanning             | -                                |
| `Visual:BarcodeTimeout`    | int     | 10                    | Barcode scan timeout (seconds)      | Range: 1-60                      |
| `Visual:OfflineMode`       | bool    | false                 | Force offline mode                  | -                                |
| `Visual:SyncInterval`      | int     | 300                   | Data sync interval (seconds)        | Range: 60-3600                   |

---

### 2. Database Configuration

| Setting Key              | Type   | Default               | Description                      | Validation                |
| ------------------------ | ------ | --------------------- | -------------------------------- | ------------------------- |
| `Database:Server`        | string | `127.0.0.1`           | MySQL server hostname            | Valid hostname/IP         |
| `Database:Port`          | int    | 3306                  | MySQL server port                | Range: 1-65535            |
| `Database:Name`          | string | `mtm_template_dev`    | Database name                    | Non-empty                 |
| `Database:Timeout`       | int    | 30                    | Connection timeout (seconds)     | Range: 5-300              |
| `Database:PoolSize`      | int    | 10                    | Connection pool size             | Range: 5-100              |
| `Database:EnableLogging` | bool   | false                 | Log SQL queries                  | -                         |
| `Database:Schema`        | string | `mtm_template_dev`    | Default schema name              | Non-empty                 |

**Note**: Database credentials stored in OS-native secrets service, not configuration.

---

### 3. Logging Configuration

| Setting Key                  | Type   | Default           | Description                    | Validation                                       |
| ---------------------------- | ------ | ----------------- | ------------------------------ | ------------------------------------------------ |
| `Logging:MinimumLevel`       | string | `Information`     | Minimum log level              | Debug, Information, Warning, Error, Critical     |
| `Logging:EnableFileLogging`  | bool   | true              | Write logs to file             | -                                                |
| `Logging:FilePath`           | string | `logs/app-.txt`   | Log file path template         | Valid file path                                  |
| `Logging:RollingInterval`    | string | `Day`             | Log file rolling interval      | Minute, Hour, Day, Month, Year                   |
| `Logging:MaxFileSize`        | int    | 100               | Max log file size (MB)         | Range: 10-1000                                   |
| `Logging:RetentionDays`      | int    | 30                | Log retention period (days)    | Range: 1-365                                     |
| `Logging:EnableConsole`      | bool   | true              | Write logs to console          | -                                                |
| `Logging:EnableDiagnostics`  | bool   | true              | Enable diagnostics logging     | -                                                |

---

### 4. UI Configuration

| Setting Key                | Type   | Default     | Description                     | Validation                          |
| -------------------------- | ------ | ----------- | ------------------------------- | ----------------------------------- |
| `UI:Theme`                 | string | `Auto`      | Application theme               | Light, Dark, Auto, HighContrast     |
| `UI:Language`              | string | `en-US`     | UI language/locale              | Valid culture code                  |
| `UI:FontSize`              | int    | 14          | Base font size (px)             | Range: 10-24                        |
| `UI:ShowSplash`            | bool   | true        | Show splash screen on boot      | -                                   |
| `UI:SplashTimeout`         | int    | 10          | Splash screen timeout (seconds) | Range: 5-30                         |
| `UI:EnableAnimations`      | bool   | true        | Enable UI animations            | -                                   |
| `UI:DebugTerminalAutoOpen` | bool   | false       | Auto-open debug terminal        | -                                   |
| `UI:WindowWidth`           | int    | 1200        | Default window width (px)       | Range: 800-4096                     |
| `UI:WindowHeight`          | int    | 800         | Default window height (px)      | Range: 600-2160                     |
| `UI:RememberWindowSize`    | bool   | true        | Remember window size/position   | -                                   |

---

### 5. Cache Configuration

| Setting Key              | Type   | Default           | Description                    | Validation          |
| ------------------------ | ------ | ----------------- | ------------------------------ | ------------------- |
| `Cache:Enabled`          | bool   | true              | Enable local caching           | -                   |
| `Cache:MaxSizeMB`        | int    | 40                | Maximum cache size (MB)        | Range: 10-500       |
| `Cache:Compression`      | string | `LZ4`             | Compression algorithm          | LZ4, None           |
| `Cache:CompressionLevel` | int    | 1                 | Compression level              | Range: 1-9          |
| `Cache:ExpiryHours`      | int    | 24                | Default cache expiry (hours)   | Range: 1-168        |
| `Cache:Path`             | string | `cache/`          | Cache directory path           | Valid directory     |
| `Cache:CleanupInterval`  | int    | 3600              | Cleanup interval (seconds)     | Range: 300-86400    |

---

### 6. Performance Configuration

| Setting Key                      | Type | Default | Description                       | Validation       |
| -------------------------------- | ---- | ------- | --------------------------------- | ---------------- |
| `Performance:EnableProfiling`    | bool | false   | Enable performance profiling      | -                |
| `Performance:BootTargetMs`       | int  | 10000   | Boot time target (ms)             | Range: 1000-30000|
| `Performance:Stage1TargetMs`     | int  | 3000    | Stage 1 target (ms)               | Range: 1000-10000|
| `Performance:MemoryBudgetMB`     | int  | 100     | Memory usage budget (MB)          | Range: 50-500    |
| `Performance:EnableTelemetry`    | bool | false   | Enable telemetry (OpenTelemetry)  | -                |
| `Performance:TelemetryEndpoint`  | string | (empty) | Telemetry endpoint URL          | Valid URL        |

---

### 7. Feature Flags

**Note**: Feature flags stored in MySQL `FeatureFlags` table and synchronized at boot.

| Flag Name                | Type | Default | Description                        |
| ------------------------ | ---- | ------- | ---------------------------------- |
| `Visual.UseForItems`     | bool | true    | Use Visual ERP for item data       |
| `Visual.UseForWorkOrders`| bool | true    | Use Visual ERP for work orders     |
| `Visual.UseForInventory` | bool | true    | Use Visual ERP for inventory       |
| `Visual.OfflineMode`     | bool | false   | Force offline mode                 |
| `UI.NewSettingsScreen`   | bool | true    | Enable new settings UI             |
| `Debug.VerboseLogging`   | bool | false   | Enable verbose debug logging       |

---

### 8. User Folder Paths

**Note**: User-specific folder paths for data storage.

| Setting Key          | Type   | Default                          | Description                  | Validation      |
| -------------------- | ------ | -------------------------------- | ---------------------------- | --------------- |
| `Folders:Data`       | string | `%APPDATA%/MTM/data`             | User data directory          | Valid directory |
| `Folders:Cache`      | string | `%APPDATA%/MTM/cache`            | Cache directory              | Valid directory |
| `Folders:Logs`       | string | `%APPDATA%/MTM/logs`             | Log file directory           | Valid directory |
| `Folders:Exports`    | string | `%USERPROFILE%/Documents/MTM`    | Export file directory        | Valid directory |
| `Folders:Temp`       | string | `%TEMP%/MTM`                     | Temporary file directory     | Valid directory |

---

## Settings Screen UI Design

### Layout Structure

```
Settings Window
├── Side Panel Navigation
│   ├── Visual ERP Integration
│   ├── Database
│   ├── Logging
│   ├── UI Preferences
│   ├── Cache
│   ├── Performance
│   ├── Feature Flags
│   └── User Folders
└── Content Area
    ├── Category Title
    ├── Settings Rows (grouped by subcategory)
    └── Action Buttons (Save, Reset, Cancel)
```

### Settings Row Types

1. **Text Input**: `Visual:BaseUrl`, `Database:Server`, `Folders:Data`
2. **Number Input**: `Visual:Timeout`, `Database:Port`, `Cache:MaxSizeMB`
3. **Dropdown**: `Logging:MinimumLevel`, `UI:Theme`, `Cache:Compression`
4. **Toggle**: `Visual:EnableCaching`, `Database:EnableLogging`, `UI:ShowSplash`
5. **Slider**: `UI:FontSize`, `Cache:CompressionLevel`

### Validation Feedback

- **Real-time validation**: Show errors inline as user types
- **Save button state**: Disabled until all validations pass
- **Error summary**: List all validation errors at top of form

---

## Configuration Change Handling

### Change Events

```csharp
_configurationService.OnConfigurationChanged += (sender, args) =>
{
    // args.Key: Changed configuration key
    // args.OldValue: Previous value
    // args.NewValue: New value

    // Trigger appropriate response
};
```

### Settings Requiring Restart

| Setting                     | Requires Restart | Reason                          |
| --------------------------- | ---------------- | ------------------------------- |
| `Visual:BaseUrl`            | Yes              | API client initialization       |
| `Database:Server`           | Yes              | Connection pool recreation      |
| `Logging:MinimumLevel`      | No               | Runtime reconfiguration         |
| `UI:Theme`                  | No               | Runtime theme switching         |
| `Cache:MaxSizeMB`           | No               | Runtime cache resize            |
| `Performance:BootTargetMs`  | Yes              | Boot sequence configuration     |

---

## Settings Export/Import

### Export Format (JSON)

```json
{
  "exportMetadata": {
    "timestamp": "2025-10-08T14:30:00Z",
    "appVersion": "1.0.0",
    "platform": "Windows 11"
  },
  "settings": {
    "Visual:BaseUrl": "https://api.visual.example.com",
    "Visual:Timeout": 30,
    "Database:Server": "127.0.0.1",
    "UI:Theme": "Dark"
  }
}
```

### Import Validation

- Verify JSON schema
- Validate all setting values
- Warn about settings requiring restart
- Option to skip invalid settings

---

## User Preference Persistence

### Stored in MySQL UserPreferences Table

```sql
CREATE TABLE UserPreferences (
    PreferenceId INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(100) NOT NULL,
    PreferenceKey VARCHAR(255) NOT NULL,
    PreferenceValue TEXT NOT NULL,
    LastUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY UK_UserPreference (UserId, PreferenceKey)
);
```

### Offline Cache (Local JSON)

When database unavailable, persist to `cache/user-preferences.json`:

```json
{
  "userId": "user@example.com",
  "preferences": {
    "UI:Theme": "Dark",
    "UI:Language": "en-US"
  },
  "lastSync": "2025-10-08T14:30:00Z"
}
```

---

## Testing Requirements

1. **Validation tests**: Each setting constraint validated
2. **Persistence tests**: User preferences save/load correctly
3. **Precedence tests**: Environment variables override user config
4. **Change event tests**: Configuration changes trigger events
5. **Offline tests**: Graceful degradation when database unavailable
6. **UI tests**: Settings screen correctly displays/modifies values

**Coverage Target**: 80%+ per Constitution Principle IV

---

## Implementation Notes

- Use `SettingsCategory` control for category grouping
- Use `SettingRow` control for individual settings
- All settings must have validation before persistence
- Show restart prompt for settings requiring restart
- Export/import feature for backup/sharing configurations
