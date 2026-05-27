# HART Database Scripts

## 📁 Folder Structure

```
database/
├── Tables/
│   └── CreateJobProfileTables.sql  ✅ Job Profile tables schema
├── StoredProcedures/
│   ├── usp_LookupData.sql          ✅ Lookup data stored procedures (CW/FTE)
│   ├── usp_GetJobProfiles.sql      ✅ Job Profile stored procedure
│   └── usp_GetEmployeeHierarchy.sql ✅ Hierarchy stored procedure (NEW)
├── SeedData/
│   └── SeedLookupData.sql          ✅ Sample/test data
└── README.md                        ✅ This file
```

---

## 🚀 Execution Order

### 1. Schema (Run Once)
The full schema is provided in the prompt/documentation. Create all tables in this order:
- **Tier 1**: Lookup tables (no dependencies)
  - Run: `Tables/CreateJobProfileTables.sql` (creates JobFamily and JobProfile tables)
  - Create: Employee table (for Hierarchy API - see docs/HIERARCHY_API.md)
- **Tier 2**: Second-level tables (FK to Tier 1)
- **Tier 3**: Core tables (FK to Tier 1 & 2)
- **Tier 4**: Supporting tables

### 2. Stored Procedures (Run Once, Update as Needed)
```sql
-- Execute: StoredProcedures/usp_LookupData.sql
-- Creates:
--   - usp_GetFTELookupData (8 result sets)
--   - usp_GetCWLookupData (6 result sets)

-- Execute: StoredProcedures/usp_GetJobProfiles.sql
-- Creates:
--   - usp_GetJobProfiles (with optional job level filter)

-- Execute: StoredProcedures/usp_GetEmployeeHierarchy.sql (NEW)
-- Creates:
--   - usp_GetEmployeeHierarchy (recursive CTE for org hierarchy)
```

### 3. Seed Data (Run Once or After Schema Reset)
```sql
-- Execute: SeedData/SeedLookupData.sql
-- Populates all 10 lookup tables with sample data
```

---

## 📄 File Descriptions
### `StoredProcedures/usp_LookupData.sql`
**Purpose**: Fetch lookup data for FTE and CW request types

**Contains**:
- `usp_GetFTELookupData` - Returns 8 result sets
- `usp_GetCWLookupData` - Returns 6 result sets

**Features**:
- Returns multiple result sets in single call
- Filters only active records (`IsActive = 1`)
- Orders results alphabetically by name
- Returns GUID as Value, Name as Label

**Usage**:
```sql
-- Test FTE
EXEC dbo.usp_GetFTELookupData;

-- Test CW
EXEC dbo.usp_GetCWLookupData;
```

---

### `SeedData/SeedLookupData.sql`
**Purpose**: Insert sample/test data into lookup tables

**Contains**:
- MERGE statements for 10 lookup tables
- Idempotent (safe to run multiple times)
- Sample business data

**Features**:
- Uses MERGE (updates if exists, inserts if not)
- Sets IsActive = 1 for all records
- Adds audit fields (CreatedBy, CreatedOn)
- Includes verification query at end

**Tables Populated**:
1. JobLevel (~8 records)
2. SWPRole (~11 records)
3. PositionType (~4 records)
4. FundingType (~4 records)
5. RoleType (~5 records)
6. CountryList (~8 records)
7. PositionStatus (~7 records)
8. VacancyType (~4 records)
9. LineOfBusiness (~7 records)
10. ProjectTheme (~5 records)

---

## 🔐 Permissions

### Minimum Required
```sql
-- Create application user
CREATE USER HARTAppUser WITH PASSWORD = 'YourStrongPassword!';

-- Grant execute on stored procedures
GRANT EXECUTE ON dbo.usp_GetFTELookupData TO HARTAppUser;
GRANT EXECUTE ON dbo.usp_GetCWLookupData TO HARTAppUser;

-- Optional: Grant SELECT on lookup tables (if direct access needed)
GRANT SELECT ON dbo.JobLevel TO HARTAppUser;
GRANT SELECT ON dbo.SWPRole TO HARTAppUser;
-- ... etc for all 10 tables
```

---

## 🧪 Testing

### Test Stored Procedures
```sql
-- Test FTE SP
EXEC dbo.usp_GetFTELookupData;
-- Should return 8 result sets

-- Test CW SP
EXEC dbo.usp_GetCWLookupData;
-- Should return 6 result sets
```

### Verify Data
```sql
-- Check row counts
SELECT 'JobLevel' AS [Table], COUNT(*) AS [Count] 
FROM dbo.JobLevel WHERE IsActive = 1
UNION ALL
SELECT 'SWPRole', COUNT(*) FROM dbo.SWPRole WHERE IsActive = 1
UNION ALL
SELECT 'PositionType', COUNT(*) FROM dbo.PositionType WHERE IsActive = 1
UNION ALL
SELECT 'FundingType', COUNT(*) FROM dbo.FundingType WHERE IsActive = 1
UNION ALL
SELECT 'RoleType', COUNT(*) FROM dbo.RoleType WHERE IsActive = 1
UNION ALL
SELECT 'CountryList', COUNT(*) FROM dbo.CountryList WHERE IsActive = 1
UNION ALL
SELECT 'PositionStatus', COUNT(*) FROM dbo.PositionStatus WHERE IsActive = 1
UNION ALL
SELECT 'VacancyType', COUNT(*) FROM dbo.VacancyType WHERE IsActive = 1
UNION ALL
SELECT 'LineOfBusiness', COUNT(*) FROM dbo.LineOfBusiness WHERE IsActive = 1
UNION ALL
SELECT 'ProjectTheme', COUNT(*) FROM dbo.ProjectTheme WHERE IsActive = 1;
```

### Verify Indexes
```sql
-- Check indexes exist
SELECT 
	OBJECT_NAME(object_id) AS TableName,
	name AS IndexName,
	type_desc AS IndexType
FROM sys.indexes
WHERE OBJECT_NAME(object_id) IN (
	'JobLevel', 'SWPRole', 'PositionType', 'FundingType', 
	'RoleType', 'CountryList', 'PositionStatus', 'VacancyType',
	'LineOfBusiness', 'ProjectTheme'
)
ORDER BY TableName, name;
```

---

## 📊 Performance Monitoring

### Check SP Execution Stats
```sql
SELECT 
	OBJECT_NAME(object_id) AS ProcedureName,
	execution_count AS Executions,
	total_elapsed_time / 1000000.0 AS TotalTimeSec,
	total_elapsed_time / execution_count / 1000.0 AS AvgTimeMs,
	last_execution_time AS LastRun
FROM sys.dm_exec_procedure_stats
WHERE OBJECT_NAME(object_id) IN ('usp_GetFTELookupData', 'usp_GetCWLookupData')
ORDER BY total_elapsed_time DESC;
```

### Check Table Sizes
```sql
SELECT 
	t.name AS TableName,
	p.rows AS RowCount,
	CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name IN (
	'JobLevel', 'SWPRole', 'PositionType', 'FundingType', 
	'RoleType', 'CountryList', 'PositionStatus', 'VacancyType',
	'LineOfBusiness', 'ProjectTheme'
)
GROUP BY t.name, p.rows
ORDER BY t.name;
```

---

## 🔄 Maintenance

### Add New Lookup Item
```sql
-- Example: Add new SWP Role
INSERT INTO dbo.SWPRole (SWPRoleCode, SWPRoleName, IsActive, CreatedBy, CreatedOn)
VALUES ('SWP012', 'Security Engineer', 1, 'Admin', GETUTCDATE());
```

### Deactivate Lookup Item
```sql
-- Example: Deactivate a role (don't delete - referential integrity)
UPDATE dbo.SWPRole
SET IsActive = 0, 
	ModifiedBy = 'Admin',
	ModifiedOn = GETUTCDATE()
WHERE SWPRoleCode = 'SWP012';
```

### Update Stored Procedure
```sql
-- Drop and recreate
DROP PROCEDURE IF EXISTS dbo.usp_GetFTELookupData;
GO

-- Then re-run the CREATE PROCEDURE script
```

---

## 🚨 Troubleshooting

### Issue: Stored procedure not found
**Solution**: Run `StoredProcedures/usp_LookupData.sql`

### Issue: No data returned
**Solution**: Run `SeedData/SeedLookupData.sql`

### Issue: Permission denied
**Solution**: Grant EXECUTE permission to application user

### Issue: Duplicate key error
**Solution**: Check unique indexes on business key columns

### Issue: Foreign key violation
**Solution**: Ensure parent records exist before inserting child records

---

## 📝 Notes

### Business Keys
Each lookup table has a business key (e.g., `JobLevelCode`, `SWPRoleCode`) that:
- Must be unique
- Is indexed
- Should be used for external system integration
- Is separate from the GUID primary key

### Audit Fields
All tables include:
- `IsActive` - Soft delete flag
- `CreatedBy` - Who created the record
- `CreatedOn` - When created
- `ModifiedBy` - Who last modified
- `ModifiedOn` - When last modified

### Indexes
- **Primary Key**: Clustered on GUID ID column
- **Unique Index**: On business key (Code) column
- **Filtered Index**: Excludes NULL values for performance

---

## 📚 Related Documentation

- `docs/ADO_NET_MIGRATION.md` - Complete technical guide
- `docs/DEPLOYMENT_CHECKLIST.md` - Deployment steps
- `docs/QUICK_START.md` - Local development setup
- `docs/MIGRATION_SUMMARY.md` - Migration summary

---

**For questions or issues, refer to the main documentation.**
