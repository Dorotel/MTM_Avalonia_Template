---
description: Comprehensive MAMP MySQL database audit - identifies missing data, inaccuracies, and obsolete entries in .github/mamp-database JSON files
---

# MAMP Database Synchronization Audit

This prompt performs a comprehensive audit of the MAMP MySQL 5.7 database to ensure `.github/mamp-database/` JSON files remain accurate and complete.

## Audit Scope

This audit checks for:

1. **Missing Data**: Database objects that exist but are not documented in JSON files
2. **Inaccuracies**: Mismatches between actual database structure and JSON documentation
3. **Obsolete Data**: JSON entries for database objects that no longer exist

## Execution Flow

### Phase 1: Codebase Analysis

**Objective**: Find all database interactions in the codebase

1. **SQL Query Analysis**
   - Search for SQL queries in all `.cs` files using `grep_search`
   - Pattern: `SELECT|INSERT|UPDATE|DELETE|CREATE TABLE|ALTER TABLE|DROP TABLE`
   - Extract table names, column names, and query types
   - Document findings in temporary analysis structure

2. **MySqlCommand Usage**
   - Search for `MySqlCommand` instantiation patterns
   - Pattern: `new MySqlCommand|MySqlCommand\(`
   - Extract command text and parameters
   - Identify stored procedure calls (`CALL procedure_name`)

3. **Connection String Analysis**
   - Locate all connection string builders and configurations
   - Verify connection parameters match `connection-info.json`
   - Check for hardcoded credentials (security issue)

4. **Entity/Model Analysis**
   - Search for classes that map to database tables
   - Pattern: class names matching table names in `schema-tables.json`
   - Verify property names match column names
   - Check data type compatibility

### Phase 2: MAMP Server Analysis

**Objective**: Query actual MAMP MySQL 5.7 server for ground truth

1. **Table Structure Query**
   ```sql
   SELECT
       TABLE_NAME,
       ENGINE,
       TABLE_COLLATION,
       TABLE_COMMENT,
       CREATE_TIME,
       UPDATE_TIME
   FROM information_schema.TABLES
   WHERE TABLE_SCHEMA = 'mtm_template_dev';
   ```

2. **Column Details Query**
   ```sql
   SELECT
       TABLE_NAME,
       COLUMN_NAME,
       COLUMN_TYPE,
       IS_NULLABLE,
       COLUMN_DEFAULT,
       EXTRA,
       COLUMN_COMMENT
   FROM information_schema.COLUMNS
   WHERE TABLE_SCHEMA = 'mtm_template_dev'
   ORDER BY TABLE_NAME, ORDINAL_POSITION;
   ```

3. **Index Information Query**
   ```sql
   SELECT
       TABLE_NAME,
       INDEX_NAME,
       COLUMN_NAME,
       NON_UNIQUE,
       SEQ_IN_INDEX
   FROM information_schema.STATISTICS
   WHERE TABLE_SCHEMA = 'mtm_template_dev'
   ORDER BY TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX;
   ```

4. **Foreign Key Constraints Query**
   ```sql
   SELECT
       CONSTRAINT_NAME,
       TABLE_NAME,
       COLUMN_NAME,
       REFERENCED_TABLE_NAME,
       REFERENCED_COLUMN_NAME,
       UPDATE_RULE,
       DELETE_RULE
   FROM information_schema.KEY_COLUMN_USAGE
   WHERE TABLE_SCHEMA = 'mtm_template_dev'
       AND REFERENCED_TABLE_NAME IS NOT NULL;
   ```

5. **Stored Procedures Query**
   ```sql
   SELECT
       ROUTINE_NAME,
       ROUTINE_TYPE,
       DTD_IDENTIFIER AS RETURN_TYPE,
       ROUTINE_DEFINITION,
       CREATED,
       LAST_ALTERED
   FROM information_schema.ROUTINES
   WHERE ROUTINE_SCHEMA = 'mtm_template_dev';
   ```

6. **Views Query**
   ```sql
   SELECT
       TABLE_NAME AS VIEW_NAME,
       VIEW_DEFINITION,
       CHECK_OPTION,
       IS_UPDATABLE
   FROM information_schema.VIEWS
   WHERE TABLE_SCHEMA = 'mtm_template_dev';
   ```

### Phase 3: JSON File Validation

**Objective**: Load and validate all JSON files

1. **Load JSON Files**
   - Read `.github/mamp-database/connection-info.json`
   - Read `.github/mamp-database/schema-tables.json`
   - Read `.github/mamp-database/stored-procedures.json`
   - Read `.github/mamp-database/functions.json`
   - Read `.github/mamp-database/views.json`
   - Read `.github/mamp-database/indexes.json`
   - Read `.github/mamp-database/sample-data.json`
   - Read `.github/mamp-database/migrations-history.json`

2. **Validate JSON Structure**
   - Verify JSON syntax is valid
   - Check for required fields ($schema, version, lastUpdated)
   - Validate data types match schema expectations
   - Check for duplicate entries

3. **Cross-Reference Validation**
   - Verify foreign keys reference existing tables/columns
   - Validate index columns exist in referenced tables
   - Check view dependencies on tables/columns
   - Verify sample data respects constraints

### Phase 4: Discrepancy Analysis

**Objective**: Identify specific mismatches

1. **Missing Tables** (Critical)
   - Tables in MAMP server not in `schema-tables.json`
   - Impact: HIGH - Undocumented database objects
   - Action: Add table definition with full column structure

2. **Missing Columns** (High)
   - Columns in MAMP tables not in `schema-tables.json`
   - Impact: HIGH - Incomplete table documentation
   - Action: Add column definition with type and constraints

3. **Type Mismatches** (High)
   - Column types differ between MAMP and JSON
   - Impact: HIGH - Runtime errors possible
   - Action: Update JSON to match actual type or fix database

4. **Missing Indexes** (Medium)
   - Indexes in MAMP not in `indexes.json`
   - Impact: MEDIUM - Performance optimizations not documented
   - Action: Add index definition with purpose

5. **Missing Foreign Keys** (Medium)
   - Foreign keys in MAMP not in `schema-tables.json`
   - Impact: MEDIUM - Referential integrity not documented
   - Action: Add foreign key with cascade rules

6. **Obsolete Tables** (Medium)
   - Tables in `schema-tables.json` not in MAMP
   - Impact: MEDIUM - Documentation bloat
   - Action: Remove from JSON or document as planned

7. **Obsolete Columns** (Medium)
   - Columns in JSON not in actual tables
   - Impact: MEDIUM - Confusion for developers
   - Action: Remove from JSON or add to database

8. **Missing Stored Procedures** (Low)
   - Procedures in MAMP not in `stored-procedures.json`
   - Impact: LOW - If no procedures exist
   - Action: Document procedure signature and logic

9. **Missing Views** (Low)
   - Views in MAMP not in `views.json`
   - Impact: LOW - If no views exist
   - Action: Document view definition and dependencies

10. **Codebase References to Undocumented Objects** (Critical)
    - Code references tables/columns not in JSON
    - Impact: CRITICAL - Maintenance risk
    - Action: Add to JSON immediately

11. **JSON Objects Never Used in Code** (Low)
    - JSON entries with no code references
    - Impact: LOW - May be planned future features
    - Action: Verify with stakeholder, mark as planned or remove

### Phase 5: Report Generation

**Objective**: Produce actionable audit report

Generate report with following sections:

#### 5.1 Executive Summary
```
- Total tables audited: X
- Total columns audited: Y
- Critical issues: N
- High priority issues: M
- Medium priority issues: P
- Low priority issues: Q
- Files requiring updates: K
```

#### 5.2 Critical Issues (Block Development)
List all critical mismatches that could cause runtime errors:
- Missing tables referenced in code
- Type mismatches that could cause data loss
- Foreign key violations in sample data

#### 5.3 High Priority Issues (Update Soon)
List high-priority mismatches:
- Undocumented columns
- Missing constraints
- Index mismatches affecting performance

#### 5.4 Medium Priority Issues (Update Next Sprint)
List medium-priority issues:
- Documentation completeness
- Obsolete entries
- Sample data updates needed

#### 5.5 Low Priority Issues (Nice to Have)
List low-priority issues:
- Missing procedure documentation
- View definitions
- Future planned objects

#### 5.6 Recommended Actions
For each issue, provide:
- Specific file(s) to update
- Exact JSON structure to add/modify
- SQL verification query (if applicable)
- Estimated effort (minutes)

#### 5.7 Compliance Check
Verify constitutional requirement:
- ✅ All database objects documented in JSON
- ✅ JSON files are up-to-date (lastUpdated within 30 days)
- ✅ Migration history complete
- ✅ Sample data matches current schema

### Phase 6: Auto-Fix Suggestions

**Objective**: Provide copy-paste fixes when possible

For each discrepancy, generate:

1. **JSON Update Template**
   ```json
   // Add to schema-tables.json under "tables" → "TableName" → "columns"
   {
     "name": "NewColumn",
     "type": "VARCHAR(100)",
     "nullable": true,
     "default": null,
     "description": "TODO: Add description"
   }
   ```

2. **SQL Verification Query**
   ```sql
   -- Verify column exists in MAMP
   SHOW COLUMNS FROM TableName LIKE 'NewColumn';
   ```

3. **Code Reference Finder**
   ```
   # Find all code references to this object
   grep -r "NewColumn" MTM_Template_Application/
   ```

### Phase 7: Update Workflow

After generating report, guide user through updates:

1. **Backup Current JSON Files**
   ```powershell
   Copy-Item .github/mamp-database/*.json .github/mamp-database/backup/
   ```

2. **Apply Critical Fixes First**
   - Update JSON files for critical issues
   - Verify with SQL queries
   - Rerun contract tests

3. **Apply High Priority Fixes**
   - Update JSON files for high-priority issues
   - Update lastUpdated timestamp
   - Increment patch version

4. **Commit Changes**
   ```powershell
   git add .github/mamp-database/
   git commit -m "db: sync MAMP database documentation (audit 2025-10-05)"
   ```

5. **Update Constitution** (if new principle needed)
   - Document any new database governance rules
   - Update `.specify/memory/constitution.md`

## Usage Instructions

### For GitHub Copilot

To run this audit:

1. **Trigger**: User says "run MAMP database audit" or "sync database documentation"
2. **Prerequisites**: Ensure MAMP MySQL 5.7 is running and accessible
3. **Execution**: Follow Phase 1-7 systematically
4. **Output**: Generate comprehensive report as Markdown
5. **Follow-up**: Offer to apply auto-fix suggestions

### Example Invocation

```
@workspace Run the MAMP database synchronization audit to check for discrepancies
between .github/mamp-database JSON files and actual database structure
```

### Manual Verification Queries

Developers can run these manually to verify accuracy:

```sql
-- List all tables
SHOW TABLES FROM mtm_template_dev;

-- Get table structure
DESCRIBE mtm_template_dev.Users;
DESCRIBE mtm_template_dev.UserPreferences;
DESCRIBE mtm_template_dev.FeatureFlags;

-- List all indexes
SHOW INDEX FROM mtm_template_dev.Users;
SHOW INDEX FROM mtm_template_dev.UserPreferences;
SHOW INDEX FROM mtm_template_dev.FeatureFlags;

-- List foreign keys
SELECT * FROM information_schema.KEY_COLUMN_USAGE
WHERE TABLE_SCHEMA = 'mtm_template_dev'
  AND REFERENCED_TABLE_NAME IS NOT NULL;

-- List stored procedures and functions
SHOW PROCEDURE STATUS WHERE Db = 'mtm_template_dev';
SHOW FUNCTION STATUS WHERE Db = 'mtm_template_dev';

-- List views
SHOW FULL TABLES IN mtm_template_dev WHERE TABLE_TYPE = 'VIEW';
```

## Constitutional Integration

This audit fulfills the constitutional requirement:

> **PRINCIPLE: Database Schema Accuracy**
> GitHub Copilot MUST maintain accurate documentation of all MAMP MySQL database
> objects in `.github/mamp-database/` JSON files. Any database creation, modification,
> or deletion MUST immediately update corresponding JSON files. Regular audits MUST
> verify accuracy and identify discrepancies.

## Success Criteria

Audit is successful when:
- ✅ All database objects documented in JSON files
- ✅ All JSON entries verified against actual database
- ✅ Zero critical discrepancies
- ✅ High/medium issues documented with action plan
- ✅ JSON files updated with lastUpdated timestamp
- ✅ Migration history reflects current schema version
- ✅ Sample data matches current schema constraints

## Notes

- **Run this audit**: After any manual database changes, after merging database-related PRs, before major releases
- **Frequency**: Weekly during active development, monthly during maintenance
- **Automation**: Consider CI/CD integration for automatic audits
- **Security**: Never log or expose database credentials in audit reports
