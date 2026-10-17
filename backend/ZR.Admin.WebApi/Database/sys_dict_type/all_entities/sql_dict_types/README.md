# SQL Dictionary Types - Complete System

## 🎯 Overview

This directory contains **SQL-based dictionary type definitions** for all 121 SugarTable entities with `sql_` prefix in dictType.

### Key Features

✅ **SQL-First Approach** - DictType starts with `sql_` to indicate SQL-generated data
✅ **Complete Coverage** - All 121 entities from the system
✅ **Ready-to-Use Queries** - Pre-generated SQL for each table
✅ **Flexible Patterns** - Adapts to different table structures
✅ **Bilingual Support** - Handles English and Arabic fields

## 📊 Statistics

| Metric | Value |
|--------|-------|
| **Total Entities** | 121 |
| **SQL Query Files** | 1 combined file (1122 lines) |
| **JSON Dict Types** | 121 individual + 1 combined |
| **Total Size** | ~60 KB |

## 📁 Files

- **all_sql_dict_types.json** (30 KB) - Combined JSON with all sql_ dict types
- **all_dict_sql_queries.sql** (1122 lines) - All SQL queries in one file
- **sql_[table_name].json** - Individual dict_type files (121 files)
- **README.md** - This documentation
- **combine.sh** - Script to regenerate combined JSON

## 🔍 DictType Naming Convention

All dictTypes use `sql_` prefix:

```
sql_event_types
sql_event_locations
sql_hr_employee
sql_sys_user
sql_ai_agent_mode
... etc
```

## 📝 SQL Query Patterns

### Pattern 1: Standard Type Tables (Code - Name - NameAr)
**Used for**: event_types, zone_types, gate_types, etc.

```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_types' AS dictType
FROM event_types
ORDER BY typecode;
```

**Entities using this pattern:**
- event_types
- event_zone_types
- event_gate_types
- event_visitor_types
- event_ticket_types
- event_permit_types
- event_booth_types
- event_incident_types
- event_status

### Pattern 2: Payment Methods (MethodCode - MethodName - MethodNameAr)

```sql
SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'sql_event_payment_methods' AS dictType
FROM event_payment_methods
ORDER BY methodcode;
```

### Pattern 3: Event Locations (LocationID - LocationName - City)

```sql
SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark,
    'sql_event_locations' AS dictType
FROM event_locations
ORDER BY locationid;
```

### Pattern 4: HR Employees (EmployeeID - FirstName LastName NameAr)

```sql
SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'sql_hr_employee' AS dictType
FROM hr_employee
WHERE status = 'active'
ORDER BY employeeid;
```

### Pattern 5: Generic (ID - Name)
**Used for**: Most other tables

```sql
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_[table_name]' AS dictType
FROM [table_name]
ORDER BY id;
```

## 🚀 Usage

### 1. View All SQL Queries
```bash
cat ../sql_queries/all_dict_sql_queries.sql
```

### 2. Execute Specific Query
```sql
-- Get event types
\i sql_queries/all_dict_sql_queries.sql

-- Or execute directly
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_types' AS dictType
FROM event_types
ORDER BY typecode;
```

### 3. Import Dict Types
```bash
# Import all SQL dict types
curl -X POST http://localhost:8887/api/system/dictType/bulk-import \
  -H "Content-Type: application/json" \
  -d @all_sql_dict_types.json
```

### 4. Generate Dict Data
```sql
-- Insert dictionary data from SQL query
INSERT INTO sys_dict_data (dict_type, dict_value, dict_label, dict_sort, remark, is_default, status, create_by)
SELECT
    'sql_event_types' as dict_type,
    typecode as dict_value,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) as dict_label,
    ROW_NUMBER() OVER (ORDER BY typecode) as dict_sort,
    remark,
    'N' as is_default,
    '0' as status,
    'system' as create_by
FROM event_types;
```

## 📊 Entity Categories with SQL Patterns

### Event Management (23 entities)

#### Type Tables (9)
```
sql_event_types              - Pattern 1
sql_event_zone_types         - Pattern 1
sql_event_gate_types         - Pattern 1
sql_event_visitor_types      - Pattern 1
sql_event_ticket_types       - Pattern 1
sql_event_permit_types       - Pattern 1
sql_event_booth_types        - Pattern 1
sql_event_incident_types     - Pattern 1
sql_event_status             - Pattern 1
```

#### Special Tables (2)
```
sql_event_payment_methods    - Pattern 2 (methodcode)
sql_event_locations          - Pattern 3 (locationid + city)
```

#### Main Tables (12)
```
sql_event_events
sql_event_zones
sql_event_gates
sql_event_visitors
sql_event_tickets
sql_event_permits
sql_event_booths
sql_event_tenants
sql_event_contracts
sql_event_incidents
sql_event_payments
sql_event_invoices
```

### HR Module (38 entities)

```
sql_hr_employee              - Pattern 4 (special)
sql_hr_attendance
sql_hr_payroll
sql_hr_leave_request
sql_hr_overtime_request
sql_hr_benefits
sql_hr_insurance
... and more
```

### System Module (22 entities)

```
sql_sys_user
sql_sys_role
sql_sys_menu
sql_sys_dept
sql_sys_config
sql_sys_dict_type
sql_sys_dict_data
... and more
```

## 💡 Customization

### Modify SQL Pattern

1. Edit the SQL query in `all_dict_sql_queries.sql`
2. Test the query in your database
3. Update the corresponding JSON file if needed

### Add Custom Field to Label

```sql
-- Original
CONCAT(typecode, ' - ', typename)

-- Add description
CONCAT(typecode, ' - ', typename, ' (', description, ')')
```

### Filter Active Records

```sql
-- Add WHERE clause
WHERE isactive = '1' OR status = '0'

-- Or for deleted records
WHERE deleted_at IS NULL
```

## 🔄 Regenerate Files

```bash
# Regenerate SQL queries
bash ../generate_sql.sh

# Regenerate combined JSON
bash combine.sh
```

## 📚 Examples by Use Case

### Use Case 1: Populate Dropdown
```csharp
// Get event types for dropdown
var query = @"
    SELECT
        typecode AS value,
        CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS label
    FROM event_types
    ORDER BY typecode";

var eventTypes = await _db.Ado.SqlQueryAsync<dynamic>(query);
```

### Use Case 2: Sync to sys_dict_data
```sql
-- Clear existing
DELETE FROM sys_dict_data WHERE dict_type = 'sql_event_types';

-- Insert fresh data
INSERT INTO sys_dict_data (dict_type, dict_value, dict_label, dict_sort, status, create_by)
SELECT
    'sql_event_types',
    typecode,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear),
    ROW_NUMBER() OVER (ORDER BY typecode),
    '0',
    'system'
FROM event_types;
```

### Use Case 3: API Endpoint
```csharp
[HttpGet("dict/{tableName}")]
public async Task<IActionResult> GetDictData(string tableName)
{
    var query = $@"
        SELECT
            typecode AS dictValue,
            CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
            'sql_{tableName}' AS dictType
        FROM {tableName}
        ORDER BY typecode";

    var data = await _db.Ado.SqlQueryAsync<SysDictData>(query);
    return Ok(data);
}
```

## 🎯 Pattern Selection Guide

| Table Has | Use Pattern | Example |
|-----------|-------------|---------|
| typecode + typename + typenamear | Pattern 1 | event_types |
| methodcode + methodname | Pattern 2 | payment_methods |
| locationid + locationname + city | Pattern 3 | event_locations |
| employeeid + firstname + lastname | Pattern 4 | hr_employee |
| Just id + name | Pattern 5 | Generic tables |

## 🛠️ Maintenance

### Add New Entity

1. Create entity with `[SugarTable]` attribute
2. Run entity discovery:
   ```bash
   bash /tmp/extract_entities.sh > /tmp/all_entities.txt
   ```
3. Regenerate SQL queries:
   ```bash
   bash ../generate_sql.sh
   ```
4. Create JSON dict_type file
5. Regenerate combined files

### Update Existing Query

1. Edit `all_dict_sql_queries.sql`
2. Test query in database
3. Update documentation if pattern changes

## 📞 Support

### Common Questions

**Q: Why sql_ prefix?**
A: To distinguish SQL-generated dictionaries from static ones and indicate the data source.

**Q: Can I customize the SQL queries?**
A: Yes! Edit the queries in `all_dict_sql_queries.sql` to match your needs.

**Q: How do I handle new columns?**
A: Add them to the CONCAT in dictLabel or create a custom pattern.

**Q: What about performance?**
A: Add indexes on the columns used in ORDER BY and WHERE clauses.

### Troubleshooting

**Issue: Query returns NULL labels**
- Check if column names match case-sensitive
- Verify columns exist in table
- Check for NULL values in source data

**Issue: Duplicate dict_values**
- Ensure primary key or unique field is used as dictValue
- Add DISTINCT if needed

## 🔗 Related Files

- **../all_system_dict_types.json** - Non-SQL dict types
- **../sql_queries/** - Individual SQL query files
- **../../event_management/** - Original Event Management types

---

**Generated**: October 20, 2025
**Total Queries**: 121
**Pattern Types**: 5
**Format Version**: 1.0
