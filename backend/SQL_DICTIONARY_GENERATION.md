# SQL Dictionary Generation System

## Overview

This system automatically discovers entities with `[SqlDict]` attribute and generates SQL-based dictionary types in the `sys_dict_type` table. It eliminates the need for manual SQL query creation for lookup data and provides a consistent pattern for managing reference data across the application.

## Features

- **Automatic Discovery**: Scans all assemblies for entities with `[SqlDict]` attribute
- **SQL Query Generation**: Automatically creates SELECT queries based on attribute configuration
- **Duplicate Prevention**: Checks if dictionary types exist before insertion
- **Update Support**: Updates existing dictionary types with new SQL queries
- **Testing Tools**: Validates and tests SQL queries before execution
- **Bilingual Support**: Supports Arabic and English labels with CONCAT expressions

## Architecture

### 1. SqlDictAttribute (ZR.Model)

Custom attribute that defines metadata for SQL dictionary generation.

**Location**: `F:\mahamBackendNET\backend\ZR.Model\SqlDictAttribute.cs`

**Key Properties**:
- `DictType`: Dictionary type code (auto-prefixed with "sql_")
- `DictName`: Display name (supports bilingual)
- `ValueColumn`: Column to use as DictValue
- `LabelColumns`: Column(s) for DictLabel (supports CONCAT)
- `RemarkColumn`: Optional remark/description column
- `OrderBy`: SQL ORDER BY clause
- `WhereClause`: Optional WHERE conditions
- `CustomSql`: Override auto-generation with custom SQL

**Example Usage**:
```csharp
[SugarTable("event_types")]
[SqlDict(
    dictType: "event_type",
    dictName: "أنواع الفعاليات - Event Types",
    valueColumn: "typecode",
    labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
    RemarkColumn = "remark",
    OrderBy = "typecode ASC",
    Description = "Generated from EventType table"
)]
public class EventType : SysBase
{
    // ... properties
}
```

### 2. SqlDictGeneratorService (ZR.ServiceCore)

Core service that handles entity discovery and SQL generation.

**Location**: `F:\mahamBackendNET\backend\ZR.ServiceCore\Services\SqlDictGeneratorService.cs`

**Key Methods**:

#### DiscoverSqlDictEntities()
Discovers all entities with SqlDictAttribute across all loaded assemblies.

```csharp
var entities = generator.DiscoverSqlDictEntities();
// Returns: List<(Type EntityType, SqlDictAttribute Attribute, string TableName)>
```

#### GenerateSqlQuery(string tableName, SqlDictAttribute attr)
Generates SELECT query based on attribute configuration.

**Generated SQL Format**:
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark AS remark,
    'sql_event_type' AS dictType
FROM event_types
WHERE isactive = '1'
ORDER BY typecode ASC
```

#### GenerateAndSaveAllSqlDictionaries()
Main method that discovers, generates, and saves all SQL dictionaries.

**Returns**: List of result messages (created, updated, errors)

#### ValidateSqlQuery(string sql)
Validates SQL syntax and checks for dangerous keywords.

#### TestSqlQuery(string sql, int sampleSize)
Executes query and returns sample data for testing.

### 3. SeedDataService Integration (ZR.ServiceCore)

Added methods to SeedDataService for easy integration.

**Location**: `F:\mahamBackendNET\backend\ZR.ServiceCore\Services\SeedDataService.cs`

**Methods**:
- `GenerateAndSaveSqlDictionaries()`: Generate all SQL dictionaries
- `GenerateSqlDictionariesForSpecificTypes(params Type[])`: Generate for specific types

### 4. API Endpoints (ZR.Admin.WebApi)

Three new endpoints added to `SysDictTypeController`.

**Location**: `F:\mahamBackendNET\backend\ZR.Admin.WebApi\Controllers\System\SysDictTypeController.cs`

#### POST /system/dict/type/generate-sql-dictionaries
Generates and saves all SQL dictionaries from entities with SqlDictAttribute.

**Response**:
```json
{
  "code": 200,
  "data": {
    "message": "SQL Dictionary generation completed",
    "results": [
      "=== SQL Dictionary Generation Started ===",
      "Found 4 entities with SqlDictAttribute",
      "✓ EventType: Created new dictionary type: sql_event_type",
      "✓ EventZoneType: Created new dictionary type: sql_zone_type",
      "=== Summary ===",
      "Created: 4",
      "Updated: 0",
      "Errors: 0"
    ]
  }
}
```

#### GET /system/dict/type/discover-sql-dict-entities
Discovers and returns all entities with SqlDictAttribute for preview.

**Response**:
```json
{
  "code": 200,
  "data": {
    "count": 4,
    "entities": [
      {
        "entityName": "EventType",
        "tableName": "event_types",
        "dictType": "sql_event_type",
        "dictName": "أنواع الفعاليات - Event Types",
        "valueColumn": "typecode",
        "labelColumns": "typecode, ' - ', typename, ' - ', typenamear",
        "orderBy": "typecode ASC",
        "description": "Generated from EventType table"
      }
    ]
  }
}
```

#### GET /system/dict/type/test-sql-query/{dictType}
Tests SQL query for a specific dictionary type.

**Response**:
```json
{
  "code": 200,
  "data": {
    "message": "Query executed successfully. Returned 5 rows.",
    "sql": "SELECT typecode AS dictValue...",
    "sampleData": [
      {
        "dictValue": "EV01",
        "dictLabel": "EV01 - Conference - مؤتمر",
        "remark": "Business conference",
        "dictType": "sql_event_type"
      }
    ]
  }
}
```

## Usage Guide

### Step 1: Add SqlDictAttribute to Entity

```csharp
using ZR.Model;

namespace ZR.Model.EventManagement
{
    [SugarTable("event_visitor_types")]
    [SqlDict(
        dictType: "visitor_type",
        dictName: "أنواع الزوار - Visitor Types",
        valueColumn: "typecode",
        labelColumns: "typecode, ' - ', typename, ' - ', typenamear",
        RemarkColumn = "remark",
        OrderBy = "displayorder ASC",
        WhereClause = "isactive = '1'",
        Description = "Generated from VisitorType table"
    )]
    public class VisitorType : SysBase
    {
        public long Typeid { get; set; }
        public string Typecode { get; set; }
        public string Typename { get; set; }
        public string Typenamear { get; set; }
        public string Isactive { get; set; }
        public int Displayorder { get; set; }
    }
}
```

### Step 2: Generate Dictionary Types

**Option A: Using API Endpoint (Recommended)**

```bash
POST http://localhost:8887/system/dict/type/generate-sql-dictionaries
Authorization: Bearer {your_jwt_token}
```

**Option B: Using Service Directly**

```csharp
var seedDataService = new SeedDataService();
var results = seedDataService.GenerateAndSaveSqlDictionaries();

foreach (var result in results)
{
    Console.WriteLine(result);
}
```

### Step 3: Use Generated Dictionaries

The generated dictionary types can now be used in the frontend with the dictionary data service:

```javascript
// Frontend (Vue.js)
getDictData('sql_event_type').then(data => {
  this.eventTypeOptions = data;
});
```

## Examples

### Example 1: Simple Dictionary with Single Label Column

```csharp
[SugarTable("payment_methods")]
[SqlDict(
    dictType: "payment_method",
    dictName: "Payment Methods - طرق الدفع",
    valueColumn: "code",
    labelColumns: "name",  // Single column
    OrderBy = "displayorder"
)]
public class PaymentMethod : SysBase
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int Displayorder { get; set; }
}
```

**Generated SQL**:
```sql
SELECT
    code AS dictValue,
    name AS dictLabel,
    'sql_payment_method' AS dictType
FROM payment_methods
ORDER BY displayorder
```

### Example 2: Bilingual Dictionary with CONCAT

```csharp
[SugarTable("event_gate_types")]
[SqlDict(
    dictType: "gate_type",
    dictName: "أنواع البوابات - Gate Types",
    valueColumn: "typecode",
    labelColumns: "typecode, ' - ', typename, ' - ', typenamear",  // CONCAT
    RemarkColumn = "description",
    OrderBy = "typecode ASC"
)]
public class EventGateType : SysBase
{
    public string Typecode { get; set; }
    public string Typename { get; set; }
    public string Typenamear { get; set; }
    public string Description { get; set; }
}
```

**Generated SQL**:
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    description AS remark,
    'sql_gate_type' AS dictType
FROM event_gate_types
ORDER BY typecode ASC
```

### Example 3: Filtered Dictionary with WHERE Clause

```csharp
[SugarTable("event_zone_types")]
[SqlDict(
    dictType: "active_zone_type",
    dictName: "Active Zone Types",
    valueColumn: "typecode",
    labelColumns: "typename",
    WhereClause = "isactive = '1' AND requiresspecialaccess = 'N'",
    OrderBy = "displayorder"
)]
public class EventZoneType : SysBase
{
    public string Typecode { get; set; }
    public string Typename { get; set; }
    public string Isactive { get; set; }
    public string Requiresspecialaccess { get; set; }
    public int Displayorder { get; set; }
}
```

**Generated SQL**:
```sql
SELECT
    typecode AS dictValue,
    typename AS dictLabel,
    'sql_active_zone_type' AS dictType
FROM event_zone_types
WHERE isactive = '1' AND requiresspecialaccess = 'N'
ORDER BY displayorder
```

### Example 4: Custom SQL Override

```csharp
[SugarTable("event_types")]
[SqlDict(
    dictType: "event_type_custom",
    dictName: "Event Types with Count",
    valueColumn: "typecode",
    labelColumns: "typename",
    CustomSql = @"
        SELECT
            et.typecode AS dictValue,
            CONCAT(et.typename, ' (', COUNT(e.eventid), ' events)') AS dictLabel,
            et.remark AS remark,
            'sql_event_type_custom' AS dictType
        FROM event_types et
        LEFT JOIN events e ON e.eventtypecode = et.typecode
        GROUP BY et.typecode, et.typename, et.remark
        ORDER BY et.typename"
)]
public class EventType : SysBase
{
    // ...
}
```

## Current Implementation Status

### Entities with SqlDictAttribute

Currently, 4 entities have been configured with SqlDictAttribute:

1. **EventType** (`event_types`)
   - Dict Type: `sql_event_type`
   - Label: Bilingual (Code - Name EN - Name AR)

2. **EventZoneType** (`event_zone_types`)
   - Dict Type: `sql_zone_type`
   - Label: Bilingual (Code - Name EN - Name AR)

3. **EventGateType** (`event_gate_types`)
   - Dict Type: `sql_gate_type`
   - Label: Bilingual (Code - Name EN - Name AR)

4. **VisitorType** (`event_visitor_types`)
   - Dict Type: `sql_visitor_type`
   - Label: Bilingual (Code - Name EN - Name AR)

### Available for Configuration

These entities can benefit from SqlDictAttribute:

- `TicketType` (`event_ticket_types`)
- `PermitType` (`event_permit_types`)
- `BoothType` (`event_booth_types`)
- `IncidentType` (`event_incident_types`)
- `PaymentMethod` (`event_payment_methods`)
- `EventStatus` (`event_statuses`)
- `HrInsuranceType` (HR module)
- `PartnerType` (Business Partners module)

## Database Schema

### sys_dict_type Table

The generated dictionary types are stored in the `sys_dict_type` table:

| Column | Type | Description |
|--------|------|-------------|
| DictId | bigint | Primary key (auto-increment) |
| DictName | varchar(100) | Display name |
| DictType | varchar(100) | Unique type code (e.g., "sql_event_type") |
| Status | varchar(1) | Active status ("0" = active) |
| Type | varchar(1) | Dictionary type ("Y" = SQL-based) |
| CustomSql | text | Generated SELECT query |
| Remark | text | Description |
| Create_by | varchar(64) | Creator (system) |
| Create_time | datetime | Creation timestamp |

### Dictionary Type Example

```sql
INSERT INTO sys_dict_type (
    DictName,
    DictType,
    Status,
    Type,
    CustomSql,
    Remark,
    Create_by
) VALUES (
    'أنواع الفعاليات - Event Types',
    'sql_event_type',
    '0',
    'Y',
    'SELECT typecode AS dictValue, CONCAT(typecode, '' - '', typename, '' - '', typenamear) AS dictLabel, remark, ''sql_event_type'' AS dictType FROM event_types ORDER BY typecode ASC',
    'Generated from EventType table',
    'system'
);
```

## Best Practices

### 1. Naming Conventions

- **DictType**: Use lowercase with underscores (e.g., `event_type`, `zone_type`)
- **Prefixing**: The system automatically adds `sql_` prefix to distinguish SQL-based dictionaries
- **ValueColumn**: Should be the code/ID column (e.g., `typecode`, `statuscode`)
- **LabelColumns**: Should provide meaningful display text

### 2. Label Design

**Good Practice**: Combine code with names
```csharp
labelColumns: "typecode, ' - ', typename, ' - ', typenamear"
// Result: "EV01 - Conference - مؤتمر"
```

**Alternative**: Name only (for simple dictionaries)
```csharp
labelColumns: "typename"
// Result: "Conference"
```

### 3. Ordering

Always specify `OrderBy` for consistent results:
```csharp
OrderBy = "displayorder ASC, typename ASC"  // Multi-column ordering
OrderBy = "typecode"  // Default ASC ordering
```

### 4. Filtering Active Records

Use `WhereClause` to filter active/valid records:
```csharp
WhereClause = "isactive = '1'"
WhereClause = "status = 'A' AND deleted_at IS NULL"
```

### 5. Performance Considerations

- Keep queries simple (avoid complex JOINs unless necessary)
- Index columns used in WHERE and ORDER BY clauses
- Use `LIMIT` in CustomSql for large result sets (if needed)

### 6. Testing

Always test generated queries before deploying:

1. Discover entities: `GET /system/dict/type/discover-sql-dict-entities`
2. Generate dictionaries: `POST /system/dict/type/generate-sql-dictionaries`
3. Test SQL: `GET /system/dict/type/test-sql-query/sql_event_type`

## Troubleshooting

### Issue: Entity Not Discovered

**Problem**: Entity with SqlDictAttribute not found by discovery service.

**Solutions**:
1. Verify attribute is correctly applied to the class
2. Check that the assembly is loaded (referenced in the project)
3. Ensure the entity has `[SugarTable]` attribute
4. Verify `IsValid()` method returns true (all required properties set)

### Issue: SQL Query Fails

**Problem**: Generated SQL query execution fails.

**Solutions**:
1. Test query using: `GET /system/dict/type/test-sql-query/{dictType}`
2. Check column names match database schema
3. Verify table name from `[SugarTable]` attribute
4. Review WHERE clause syntax
5. Check for SQL syntax errors in CONCAT expressions

### Issue: Duplicate Dictionary Type

**Problem**: Dictionary type already exists error.

**Behavior**: System automatically updates existing dictionary types with new SQL.

**To Verify**: Check `sys_dict_type` table for existing entry:
```sql
SELECT * FROM sys_dict_type WHERE DictType = 'sql_event_type';
```

### Issue: Empty Results

**Problem**: Query returns no data.

**Solutions**:
1. Check if source table has data
2. Verify WHERE clause conditions
3. Test query directly in database
4. Check for NULL values in key columns

## Security Considerations

### SQL Injection Protection

The `ValidateSqlQuery` method blocks dangerous keywords:
- DROP, DELETE, TRUNCATE
- INSERT, UPDATE, ALTER, CREATE
- EXEC, EXECUTE

**Note**: CustomSql should only be used by developers, not end-users.

### Permission Requirements

API endpoints require these permissions:
- `system:dict:add` - Generate dictionaries
- `system:dict:list` - Discover entities
- `system:dict:query` - Test queries

### Audit Trail

All generated dictionaries are tracked:
- `Create_by`: "system"
- `Create_time`: Timestamp of generation
- `Update_by`: "system" (on updates)
- `Update_time`: Last update timestamp

## Future Enhancements

### Planned Features

1. **Automatic Refresh**: Scheduled job to refresh SQL dictionaries
2. **Multi-Language Support**: Dynamic language column selection
3. **Caching Strategy**: Redis caching for dictionary results
4. **Versioning**: Track changes to generated SQL queries
5. **UI Tool**: Admin interface for managing SQL dictionaries
6. **Validation Rules**: Extended validation for SQL queries
7. **Performance Monitoring**: Track query execution times
8. **Bulk Operations**: Generate dictionaries by namespace or module

### Suggested Improvements

1. Add support for nested dictionaries (parent-child relationships)
2. Support for computed columns in labels
3. Integration with code generation for dropdown components
4. Export/import dictionary configurations
5. Automated testing of all dictionary queries

## Related Files

### Core Implementation
- `F:\mahamBackendNET\backend\ZR.Model\SqlDictAttribute.cs`
- `F:\mahamBackendNET\backend\ZR.ServiceCore\Services\SqlDictGeneratorService.cs`
- `F:\mahamBackendNET\backend\ZR.ServiceCore\Services\SeedDataService.cs` (lines 1859-1910)
- `F:\mahamBackendNET\backend\ZR.Admin.WebApi\Controllers\System\SysDictTypeController.cs` (lines 122-229)

### Example Entities
- `F:\mahamBackendNET\backend\ZR.Model\EventManagement\EventType.cs`
- `F:\mahamBackendNET\backend\ZR.Model\EventManagement\EventZoneType.cs`
- `F:\mahamBackendNET\backend\ZR.Model\EventManagement\EventGateType.cs`
- `F:\mahamBackendNET\backend\ZR.Model\EventManagement\VisitorType.cs`

### Database Models
- `F:\mahamBackendNET\backend\ZR.Model\System\Model\SysDictType.cs`
- `F:\mahamBackendNET\backend\ZR.Model\System\Model\SysDictData.cs`

## Support

For questions or issues:
1. Review this documentation
2. Check the Troubleshooting section
3. Test using the provided API endpoints
4. Review example implementations

## Changelog

### Version 1.0.0 (2025-10-20)
- Initial implementation
- Core SqlDictAttribute with metadata support
- SqlDictGeneratorService with discovery and generation
- API endpoints for generation, discovery, and testing
- Integration with SeedDataService
- Documentation and examples
- 4 entities configured with SqlDictAttribute
