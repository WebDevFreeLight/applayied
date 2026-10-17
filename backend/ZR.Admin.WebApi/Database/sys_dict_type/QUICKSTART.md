# Quick Start Guide - Event Management Dictionary System

## 🚀 Quick Overview

This system automatically generates dictionary data (for dropdowns, select lists, etc.) from Event Management and HR tables, following the pattern:

```sql
SELECT employeeid AS dictValue, 
       CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel, 
       remark 
FROM hr_employee
```

## 📁 What's Included

- **12 Dictionary Types** (event_type, zone_type, gate_type, etc.)
- **Individual JSON files** for each type
- **Combined JSON file** with all types
- **SQL queries** for direct database operations
- **Complete documentation**

## ⚡ Quick Start (3 Steps)

### Step 1: Review Dictionary Types
```bash
# View all dictionary type definitions
cat all_event_management_dict_types.json
```

### Step 2: Run Seed Initialization
```bash
# Option A: Via API
curl -X POST http://localhost:8887/api/system/seed/initialize

# Option B: Via C# code
var results = seedDataService.InitSeedData(excelPath, webRootPath, clean: false);
```

### Step 3: Verify Results
```sql
-- Check dictionary types
SELECT dict_type, dict_name FROM sys_dict_type 
WHERE dict_type LIKE '%event%' OR dict_type = 'hr_employee';

-- Check dictionary data
SELECT dict_type, COUNT(*) as count 
FROM sys_dict_data 
WHERE dict_type IN ('event_type', 'zone_type', 'hr_employee')
GROUP BY dict_type;
```

## 📊 Dictionary Types Available

| Type | Source Table | Example Value |
|------|--------------|---------------|
| `event_type` | event_types | "CONF - Conference - مؤتمر" |
| `zone_type` | event_zone_types | "VIP - VIP Area - منطقة كبار الشخصيات" |
| `gate_type` | event_gate_types | "MAIN - Main Gate - البوابة الرئيسية" |
| `visitor_type` | visitor_types | "GUEST - Guest - ضيف" |
| `ticket_type` | ticket_types | "GEN - General - عام" |
| `permit_type` | permit_types | "STAFF - Staff Permit - تصريح موظف" |
| `booth_type` | booth_types | "FOOD - Food Booth - كشك طعام" |
| `incident_type` | incident_types | "SEC - Security - أمني" |
| `payment_method` | payment_methods | "CASH - Cash - نقدي" |
| `event_status` | event_status | "ACTIVE - Active - نشط" |
| `event_location` | event_locations | "1 - Riyadh Center - مركز الرياض" |
| `hr_employee` | hr_employee | "Ahmed Ali أحمد علي" |

## 💻 Usage in Code

### Get Dictionary Data (C#)
```csharp
// Get all event types
var eventTypes = await _dictDataService.GetDictDataByType("event_type");

// Use in dropdown
<select asp-items="@(new SelectList(eventTypes, "DictValue", "DictLabel"))">
```

### Get Dictionary Data (SQL)
```sql
-- Get all active event types
SELECT dict_value, dict_label
FROM sys_dict_data
WHERE dict_type = 'event_type' AND status = '0'
ORDER BY dict_sort;
```

### Get Dictionary Data (JavaScript/Frontend)
```javascript
// Fetch event types
const response = await fetch('/api/system/dictData/type/event_type');
const eventTypes = await response.json();

// Populate dropdown
const select = document.getElementById('eventTypeSelect');
eventTypes.forEach(item => {
    const option = document.createElement('option');
    option.value = item.dictValue;
    option.text = item.dictLabel;
    select.appendChild(option);
});
```

## 🔄 How It Works

```
Source Tables                Dictionary Generation            Output
--------------              ---------------------            --------
event_types         →                              →      sys_dict_type
zone_types          →   SeedDataService           →      sys_dict_data
gate_types          →   .GenerateAndSave          →      JSON files
visitor_types       →   AllDictionaries()         →      
...                 →                              →      
hr_employee         →                              →      
```

## 📝 Add New Source Data

### Method 1: Insert to Database
```sql
-- Add new event type
INSERT INTO event_types (typecode, typename, typenamear, remark)
VALUES ('EXPO', 'Exhibition', 'معرض', 'Exhibition event type');

-- Regenerate dictionaries
-- (Call seed initialization or GenerateAndSaveAllDictionaries())
```

### Method 2: Update JSON and Import
1. Edit `../SeedData/EventManagement/event_types.json`
2. Add new entry
3. Run seed initialization

## 🛠️ Customization

### Add New Dictionary Type

1. **Create JSON definition** (`my_new_type.json`):
```json
{
  "dictType": "my_new_type",
  "dictName": "My New Type",
  "status": "0",
  "remark": "SELECT id AS dictValue, name AS dictLabel FROM my_table",
  "createBy": "system"
}
```

2. **Add to combined file** (`all_event_management_dict_types.json`)

3. **Implement C# method** in `SeedDataService.cs`:
```csharp
public List<SysDictData> GenerateDictDataFromMyNewType()
{
    var db = DbScoped.SugarScope;
    var data = db.Queryable<MyNewType>().ToList();
    
    return data.Select((item, index) => new SysDictData
    {
        DictType = "my_new_type",
        DictLabel = $"{item.Code} - {item.Name}",
        DictValue = item.Code,
        DictSort = index + 1,
        Remark = item.Remark,
        IsDefault = "N",
        Status = "0",
        Create_by = "system"
    }).ToList();
}
```

4. **Add to master method** (`GenerateAndSaveAllDictionaries()`):
```csharp
var myNewTypeData = GenerateDictDataFromMyNewType();
allDictData.AddRange(myNewTypeData);
SaveToJson(myNewTypeData, Path.Combine(jsonBasePath, "dict_data_my_new_type.json"));
```

## 📚 Documentation Files

- **INDEX.md** - Complete reference guide
- **README.md** - SQL query patterns
- **STRUCTURE.md** - Directory structure and data flow
- **QUICKSTART.md** - This file (quick start)
- **dictionary_queries.sql** - All SQL queries

## 🔍 Troubleshooting

### No data generated?
```bash
# Check source tables have data
psql -d MahamDB -c "SELECT COUNT(*) FROM event_types;"

# Check logs
grep "Dictionary Generation" /path/to/logs/application.log
```

### Duplicate entries?
```sql
-- Remove duplicates
DELETE FROM sys_dict_data a
USING sys_dict_data b
WHERE a.dict_code < b.dict_code
  AND a.dict_type = b.dict_type
  AND a.dict_value = b.dict_value;
```

### Wrong data format?
1. Check column names in source table
2. Verify SQL pattern in JSON remark field
3. Review C# generation method

## 🎯 Key Features

✅ Bilingual support (English + Arabic)
✅ Automatic updates from source tables
✅ JSON export for backup/import
✅ Follows SQL query pattern consistently
✅ Integrates with seed data system
✅ Transaction-safe (insert/update)

## 📞 Need Help?

1. Check [INDEX.md](INDEX.md) for complete reference
2. Review [README.md](README.md) for SQL patterns
3. See [STRUCTURE.md](STRUCTURE.md) for architecture
4. Run verification queries in [dictionary_queries.sql](dictionary_queries.sql)

---

**Quick Links**:
- Source Code: `ZR.ServiceCore/Services/SeedDataService.cs` (Lines 1340-1847)
- Models: `ZR.Model/System/Model/SysDictType.cs` and `SysDictData.cs`
- Generated Data: `../SeedData/sys_dict_type/EventManagement/*.json`

Happy coding! 🚀
