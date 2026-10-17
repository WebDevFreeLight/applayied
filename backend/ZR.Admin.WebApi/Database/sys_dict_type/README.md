# Event Management Dictionary Types - SQL Query Reference

This directory contains JSON definitions for `sys_dict_type` entries used in the Event Management system and HR module.

## Purpose

These dictionary type definitions map database tables to the `sys_dict_data` table, allowing dynamic dropdown lists and reference data throughout the application.

## SQL Query Patterns

Each dictionary type follows a consistent pattern similar to the HR employee example:

### 1. Event Types (event_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM event_types
```

### 2. Zone Types (zone_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM event_zone_types
```

### 3. Gate Types (gate_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM event_gate_types
```

### 4. Visitor Types (visitor_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM visitor_types
```

### 5. Ticket Types (ticket_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM ticket_types
```

### 6. Permit Types (permit_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM permit_types
```

### 7. Booth Types (booth_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM booth_types
```

### 8. Incident Types (incident_type)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM incident_types
```

### 9. Payment Methods (payment_method)
```sql
SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark
FROM payment_methods
```

### 10. Event Status (event_status)
```sql
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark
FROM event_status
```

### 11. Event Locations (event_location)
```sql
SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark
FROM event_locations
```

### 12. HR Employees (hr_employee)
```sql
SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark
FROM hr_employee
```

## Usage

### Individual JSON Files
Each dictionary type has its own JSON file for easy import/reference:
- `event_type.json`
- `zone_type.json`
- `gate_type.json`
- `visitor_type.json`
- `ticket_type.json`
- `permit_type.json`
- `booth_type.json`
- `incident_type.json`
- `payment_method.json`
- `event_status.json`
- `event_location.json`
- `hr_employee.json`

### Combined File
`all_event_management_dict_types.json` - Contains all dictionary types in a single array for bulk import.

## Implementation

The C# implementation in `SeedDataService.cs` includes:

1. **GenerateEventManagementDictTypes()** - Creates all dict type definitions
2. **GenerateDictDataFrom[Type]()** - Individual methods that query each table and generate dict_data entries
3. **GenerateAndSaveAllDictionaries()** - Master method that:
   - Generates all dictionary types and data
   - Saves to JSON files
   - Inserts/updates database records
   - Returns detailed results

## Automatic Generation

When seed data initialization runs, the system automatically:
1. Queries each source table
2. Generates `sys_dict_data` entries using the SQL pattern above
3. Saves results to JSON files in `wwwroot/Database/SeedData/sys_dict_type/EventManagement/`
4. Inserts/updates the `sys_dict_type` and `sys_dict_data` tables

## File Locations

- **Type Definitions** (this directory): `ZR.Admin.WebApi/Database/sys_dict_type/`
- **Generated Data**: `ZR.Admin.WebApi/wwwroot/Database/SeedData/sys_dict_type/EventManagement/`
- **Implementation**: `ZR.ServiceCore/Services/SeedDataService.cs` (Lines 1340-1847)

## Notes

- All dictionary types use bilingual labels (English and Arabic)
- The `dictValue` is always the primary code/ID from the source table
- The `dictLabel` combines multiple fields for better context
- Status "0" means active/enabled
- CreateBy "system" indicates auto-generated entries
