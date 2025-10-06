# MAMP MySQL Database Reference

This directory contains the canonical source of truth for all MAMP MySQL 5.7 database structures, stored procedures, and interactions used by the MTM Avalonia Template application.

## Purpose

GitHub Copilot **MUST** reference these files whenever:
- Creating new database objects (tables, procedures, functions, views)
- Modifying existing database structures
- Writing queries that interact with the database
- Documenting database-related code

## File Structure

```
mamp-database/
├── README.md                    # This file - overview and usage guidelines
├── connection-info.json         # Connection settings and database credentials
├── schema-tables.json           # All table structures with columns, types, constraints
├── stored-procedures.json       # All stored procedures with parameters and logic
├── functions.json               # All user-defined functions
├── views.json                   # All database views
├── indexes.json                 # All indexes and performance optimizations
├── sample-data.json             # Sample/test data used in development
└── migrations-history.json      # Migration history and versioning
```

## Constitutional Requirement

As per `.specify/memory/constitution.md`:

> **PRINCIPLE: Database Schema Accuracy**
>
> When GitHub Copilot performs ANY operation involving the MAMP MySQL 5.7 server, it MUST:
> 1. **Reference** the JSON files in `.github/mamp-database/` before generating code
> 2. **Validate** that any database objects referenced exist in these files
> 3. **Update** the appropriate JSON file(s) immediately after creating/modifying database objects
> 4. **Run** the `mamp-database-sync.prompt.md` audit after significant database changes

## Usage Guidelines

### For GitHub Copilot

**Before writing database code:**
1. Read `schema-tables.json` to verify table names, column names, and data types
2. Read `stored-procedures.json` to check if a procedure already exists
3. Read `connection-info.json` to understand environment-specific connection settings

**After creating database objects:**
1. Add the new object to the appropriate JSON file with complete metadata
2. Update `migrations-history.json` with the change and timestamp
3. Suggest running the audit prompt to verify accuracy

**When generating queries:**
1. Use exact table names from `schema-tables.json` (case-sensitive: `Users`, `UserPreferences`, `FeatureFlags`)
2. Use exact column names (case-sensitive: `UserId`, `PreferenceKey`, `LastUpdated`)
3. Respect nullable constraints and data types

### For Developers

**Audit database accuracy:**
```powershell
# Run the comprehensive audit
# This checks for mismatches between JSON files and actual MAMP server
# See .github/prompts/mamp-database-sync.prompt.md
```

**After manual database changes:**
1. Update the appropriate JSON file manually
2. Run the audit to verify consistency
3. Commit changes with message: `db: update [object-type] [object-name]`

## Versioning

Database schema version is tracked in `migrations-history.json` and follows semantic versioning:
- **MAJOR**: Breaking changes (column removals, table drops, type changes)
- **MINOR**: New tables, columns, procedures (backward compatible)
- **PATCH**: Index additions, constraint modifications, documentation

Current schema version: **1.0.0** (as of 2025-10-05)

## MAMP MySQL 5.7 Specifics

**Important limitations:**
- No `CHECK` constraints support (use application-level validation)
- Compression protocol disabled (`UseCompression=false`) - causes `EndOfStreamException`
- Case sensitivity: Table names are case-insensitive on Windows, case-sensitive on Linux
- Engine: InnoDB only (no MyISAM)
- Charset: utf8mb4 with utf8mb4_general_ci collation

## Related Files

- `config/database-schema.json` - Legacy configuration (being phased out, DO NOT USE)
- `config/migrations/001_initial_schema.sql` - Initial migration script
- `.github/prompts/mamp-database-sync.prompt.md` - Audit prompt for accuracy checks
- `MTM_Template_Application/Services/Configuration/ConfigurationService.cs` - Connection string builder
