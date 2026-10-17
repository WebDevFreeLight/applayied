# Database Table Initialization - Usage Guide

## Overview
The `InitTable.cs` has been updated to check if tables exist before creating them, preventing errors when tables already exist.

## Key Features

### 1. Automatic Table Discovery
- Automatically discovers all entities with `[SugarTable]` attribute
- Filters out system assemblies for better performance
- Supports batch initialization

### 2. Table Existence Check
- **Always checks if a table exists before attempting to create it**
- Prevents SQL errors when tables already exist
- Provides clear console output showing which tables were created vs already existed

### 3. New Helper Methods

#### `InitAllTables()`
Initializes all discovered tables with existence check:
```csharp
ZR.ServiceCore.SqlSugar.InitTable.InitAllTables();
```

#### `InitTablesFromNamespaces(params string[] namespaces)`
Initialize tables from specific namespaces:
```csharp
ZR.ServiceCore.SqlSugar.InitTable.InitTablesFromNamespaces("ZR.Model.HR", "ZR.Mall.Model");
```

#### `InitMissingTables(params string[] namespaces)`
Only creates tables that don't exist, with detailed reporting:
```csharp
ZR.ServiceCore.SqlSugar.InitTable.InitMissingTables("ZR.Model.HR");
```

#### `GetMissingTables(params string[] namespaces)`
Returns a list of table names that don't exist in the database:
```csharp
var missingTables = ZR.ServiceCore.SqlSugar.InitTable.GetMissingTables("ZR.Model.HR");
foreach(var table in missingTables)
{
    Console.WriteLine($"Missing table: {table}");
}
```

#### `IsTableExists(Type entityType)`
Check if a specific table exists:
```csharp
bool exists = ZR.ServiceCore.SqlSugar.InitTable.IsTableExists(typeof(HrEmployee));
```

## Usage in Program.cs

### Option 1: Initialize all tables
```csharp
var initDb = builder.Configuration["InitDb"].ParseToBool();
if (initDb)
{
    Console.WriteLine("Initializing database tables...");
    ZR.ServiceCore.SqlSugar.InitTable.InitAllTables();
    Console.WriteLine("Database initialization complete.");
}
```

### Option 2: Initialize specific namespaces
```csharp
var initDb = builder.Configuration["InitDb"].ParseToBool();
if (initDb)
{
    Console.WriteLine("Initializing HR and Mall tables...");
    ZR.ServiceCore.SqlSugar.InitTable.InitTablesFromNamespaces("ZR.Model.HR", "ZR.Mall.Model");
    Console.WriteLine("Database initialization complete.");
}
```

### Option 3: Initialize only missing tables with detailed reporting
```csharp
var initDb = builder.Configuration["InitDb"].ParseToBool();
if (initDb)
{
    Console.WriteLine("Checking for missing tables...");
    ZR.ServiceCore.SqlSugar.InitTable.InitMissingTables("ZR.Model.HR", "ZR.Mall.Model");
}
```

## Console Output Examples

When tables are created:
```
✓ Created new table: hr_employee
✓ Created new table: hr_attendance
✓ Created new table: hr_leave_request
```

When tables already exist:
```
Table already exists: hr_employee
Table already exists: hr_attendance
Table already exists: hr_leave_request
```

Summary output from `InitMissingTables`:
```
Table initialization complete:
  - Created: 5 tables
  - Existing: 20 tables
  - Total: 25 tables
```

## Benefits

1. **No More Errors**: Tables are only created if they don't exist
2. **Clear Feedback**: Console output shows exactly what happened
3. **Flexible**: Can initialize all tables or specific namespaces
4. **Performance**: Batch processing for better performance
5. **Diagnostic Tools**: Helper methods to check table existence

## Error Handling

- If a batch fails, the system automatically falls back to individual table creation
- Each table creation is wrapped in try-catch to prevent one failure from stopping others
- Detailed error messages are logged to console

## Configuration

Set `InitDb` to `true` in `appsettings.json` to enable automatic table initialization:
```json
{
  "InitDb": true,
  // ... other settings
}
```