# Configuration Files Documentation

This directory contains configuration files for the MTM Avalonia Template application.

## File Overview

### user-folders.json
Configures dynamic user folder location detection with dual-storage support.

### database-schema.json
Defines database connection settings and schema structure for MySQL.

---

## Dynamic User Folder Location Logic

The application automatically detects whether you're developing at home or on-premises:

### Home Development Mode
- **Trigger**: Your public IP matches `HomeDevelopmentIPAddress` (currently: `73.94.78.172`)
- **Behavior**: Uses local path only (`{MyDocuments}\MTM_Apps\users`)
- **Benefit**: No network dependency, faster access

### On-Premises Mode
- **Trigger**: Your public IP differs from `HomeDevelopmentIPAddress`
- **Behavior**:
  1. Attempts to access network drive (`NetworkDrivePath`)
  2. Falls back to local path if network unavailable (2s timeout)
  3. If `EnableDualWrite` is true and both accessible, writes to both locations
- **Caching**: Location decision cached for 5 minutes to minimize network checks
- **Runtime Folder Creation**: User folders created automatically on first access
  - Local path: `{MyDocuments}\MTM_Apps\users\{userId}\` created if missing
  - Network path: `{NetworkDrivePath}\{userId}\` created only if network accessible
  - Graceful fallback: If creation fails, uses available alternative location
  - Never throws exceptions - silently falls back to working location

### Configuration Settings (`user-folders.json`)

| Setting                        | Description                            | Example Value                                                 | Required for Production      |
| ------------------------------ | -------------------------------------- | ------------------------------------------------------------- | ---------------------------- |
| `HomeDevelopmentIPAddress`     | Developer's home ISP IP                | `"73.94.78.172"`                                              | ✅ Yes                        |
| `NetworkDrivePath`             | Production network drive path          | `"\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users"` | ✅ Yes                        |
| `LocalFallbackPath`            | Local backup path                      | `"{MyDocuments}\\MTM_Apps\\users"`                            | No (defaults to MyDocuments) |
| `NetworkAccessTimeoutSeconds`  | Network drive test timeout             | `2`                                                           | No (default: 2)              |
| `LocationCacheDurationMinutes` | Location cache duration                | `5`                                                           | No (default: 5)              |
| `EnableDualWrite`              | Write to both locations when available | `true`                                                        | No (default: true)           |

**Development Defaults**: When production placeholders are not configured, the system uses the MyDocuments folder for local storage (`Environment.SpecialFolder.MyDocuments`).

**Special Token**: `{MyDocuments}` is automatically resolved to the user's Documents folder at runtime.

---

## Dynamic Database Connection Logic

The application automatically selects the correct database based on your location:

### Home Development Mode
- **Trigger**: Your public IP matches `HomeDevelopmentIPAddress`
- **Connection**: Uses `HomeDatabase` settings (localhost MAMP MySQL)
- **Benefit**: Develop against local database without network dependency

### On-Premises Mode
- **Trigger**: Your public IP differs from `HomeDevelopmentIPAddress`
- **Connection**: Uses `ProductionDatabase` settings (production MySQL server)
- **Benefit**: Automatically connects to production database when at work

### Configuration Settings (`database-schema.json`)

#### Connection Settings

| Setting                       | Description                | Home Development     | Production                | Required for Prod  |
| ----------------------------- | -------------------------- | -------------------- | ------------------------- | ------------------ |
| `HomeDevelopmentIPAddress`    | IP detection trigger       | `"73.94.78.172"`     | Same as user-folders.json | ✅ Yes              |
| `HomeDatabase.Host`           | Local MAMP host            | `"localhost"`        | N/A                       | No                 |
| `HomeDatabase.Port`           | Local MAMP port            | `3306`               | N/A                       | No                 |
| `HomeDatabase.Database`       | Dev database name          | `"mtm_template_dev"` | N/A                       | No                 |
| `ProductionDatabase.Host`     | Production server hostname | N/A                  | `"mtmanu-sql01"`          | ✅ Yes              |
| `ProductionDatabase.Port`     | Production MySQL port      | N/A                  | `3306`                    | No (default)       |
| `ProductionDatabase.Database` | Production database name   | N/A                  | `"mtm_template_prod"`     | ✅ Yes              |
| `ConnectionTimeoutSeconds`    | Connection timeout         | `5`                  | `5`                       | No (default: 5)    |
| `EnableConnectionPooling`     | Use connection pooling     | `true`               | `true`                    | No (default: true) |
| `MaxPoolSize`                 | Max connections in pool    | `100`                | `100`                     | No (default: 100)  |
| `MinPoolSize`                 | Min connections in pool    | `5`                  | `5`                       | No (default: 5)    |

**Development Defaults**: When production placeholders are not configured, the system uses `localhost:3306` with MAMP MySQL defaults.

#### Schema Definition

The `Schema.Tables` section contains complete table definitions:
- **UserPreferences**: Stores user-specific configuration overrides
- **FeatureFlags**: Stores feature flag states with environment filtering

Each table definition includes:
- Column names, types, nullable constraints, primary keys, defaults
- Indexes for performance optimization

---

## Configuration Precedence

The application uses a layered configuration system:

1. **Environment Variables** (highest priority)
   - Format: `MTM_*`, `DOTNET_*`, `ASPNETCORE_*`
   - Example: `MTM_API_TIMEOUT=30000`
   - Not persisted across sessions

2. **User Preferences** (medium priority)
   - Stored in MySQL `UserPreferences` table
   - Per-user configuration overrides
   - Persists across sessions

3. **Default Values** (lowest priority)
   - Hardcoded application defaults
   - Used when no override exists

---

## Production Deployment Checklist

⚠️ **CRITICAL**: Before deploying to production, administrators MUST:

1. ✅ **Update `user-folders.json`**:
   - Set `HomeDevelopmentIPAddress` to each developer's home IP
   - Set `NetworkDrivePath` to actual production network drive path
   - Verify network drive accessibility from production environment

2. ✅ **Update `database-schema.json`**:
   - Set `ProductionDatabase.Host` to production MySQL server hostname
   - Set `ProductionDatabase.Database` to production database name
   - Verify connection pooling settings are appropriate for production load

3. ✅ **Review Security**:
   - Database credentials stored in OS-native credential storage (not in config files)
   - Network drive permissions configured for application service account
   - Visual API command whitelist reviewed (see `docs/VISUAL-WHITELIST.md`)

4. ✅ **Test Dynamic Detection**:
   - Test application behavior from different IPs (home vs on-premises)
   - Verify network drive fallback logic works correctly
   - Verify database connection switches correctly based on IP

---

## Example: Development vs Production

### Development (Home IP: 73.94.78.172)

```json
{
  "HomeDevelopmentIPAddress": "73.94.78.172",
  "LocalFallbackPath": "{MyDocuments}\\MTM_Apps\\users"
}
```

**Behavior**:
- Uses local MyDocuments path: `C:\Users\johnk\OneDrive\Documents\MTM_Apps\users`
- Connects to localhost MAMP MySQL: `localhost:3306/mtm_template_dev`

### Production (On-Premises IP: 10.0.1.50)

```json
{
  "HomeDevelopmentIPAddress": "73.94.78.172",
  "NetworkDrivePath": "\\\\mtmanu-fs01\\Expo Drive\\MH_RESOURCE\\MTM_Apps\\Users",
  "LocalFallbackPath": "{MyDocuments}\\MTM_Apps\\users"
}
```

**Behavior**:
- Attempts network drive: `\\mtmanu-fs01\Expo Drive\MH_RESOURCE\MTM_Apps\Users`
- Falls back to local if network unavailable
- Connects to production MySQL: `mtmanu-sql01:3306/mtm_template_prod`

---

## Troubleshooting

### Issue: Application uses wrong database
**Solution**: Verify your public IP matches/differs from `HomeDevelopmentIPAddress` setting

### Issue: Network drive not accessible
**Solution**:
1. Check `NetworkDrivePath` is correct
2. Verify network connectivity
3. Check `NetworkAccessTimeoutSeconds` (increase if slow network)
4. Application will automatically fall back to local path

### Issue: Location cache not updating
**Solution**:
1. Restart application to clear cache
2. Reduce `LocationCacheDurationMinutes` for more frequent checks

---

## Migration Scripts

SQL migration scripts are located in `config/migrations/`:

- `001_initial_schema.sql`: Creates UserPreferences and FeatureFlags tables

To run migrations:
```bash
# Execute against MAMP MySQL (development)
mysql -u root -p -h localhost mtm_template_dev < config/migrations/001_initial_schema.sql

# Execute against production MySQL
mysql -u app_user -p -h mtmanu-sql01 mtm_template_prod < config/migrations/001_initial_schema.sql
```

---

## Related Documentation

- Feature Specification: `specs/002-environment-and-configuration/spec.md`
- Technical Plan: `specs/002-environment-and-configuration/plan.md`
- Implementation Tasks: `specs/002-environment-and-configuration/tasks.md`
- Visual API Whitelist: `docs/VISUAL-WHITELIST.md`

---

**Last Updated**: October 5, 2025
**Feature**: 002-environment-and-configuration
**Configuration Version**: 1.0
