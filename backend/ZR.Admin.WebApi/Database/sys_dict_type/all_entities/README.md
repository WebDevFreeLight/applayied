# Complete System Dictionary Types - All Entities

## 📊 Overview

This directory contains **sys_dict_type** JSON definitions for **ALL 124 entities** in the ZR.Admin system with `[SugarTable]` attributes.

### Statistics

| Metric | Count |
|--------|-------|
| **Total Entities** | 124 |
| **Individual JSON Files** | 122 |
| **Combined JSON File** | 1 |
| **Modules Covered** | 12+ |

## 📁 Files

- **all_system_dict_types.json** - Combined file with all 124 dict types
- **[table_name].json** - Individual files (122 files)
- **README.md** - This documentation
- **CATEGORIES.md** - Entities organized by module
- **generate_combined.sh** - Script to regenerate combined file

## 🗂️ Entities by Module

### 1. AI Module (4 entities)
- `ai_agent_mode` - AI Agent Mode
- `ai_modelprovider` - AI Model Provider
- `ai_team_mode` - AI Team Mode
- `ai_tooltype` - AI Tool Type

### 2. Boulevard Module (8 entities)
- `set_admin_settings` - Admin Settings
- `blv_boulevard_seasons` - Boulevard Seasons
- `blv_boulevard_seasons_shops` - Boulevard Season Shops
- `blv_boulevard_season_zones` - Boulevard Season Zones
- `blv_boulevard_shop_category` - Shop Category
- `blv_boulevard_shops` - Boulevard Shops
- `blv_boulevard_shop_status` - Shop Status
- `blv_customerregistrationform` - Customer Registration Form

### 3. Business/HR Management (6 entities)
- `hr_documenttype` - Document Type
- `hr_insurance_plan` - Insurance Plan
- `hr_performance_review` - Performance Review
- `hr_reviewtype` - Review Type
- `organizations` - Organizations
- `organizationtype` - Organization Type

### 4. Business Partner (1 entity)
- `bp_businesspartner` - Business Partner

### 5. Content Management (6 entities)
- `article` - Article
- `article_browsing_log` - Browsing Log
- `articleCategory` - Article Category
- `article_comment` - Article Comment
- `article_praise` - Article Praise
- `article_topic` - Article Topic
- `article_userCircles` - User Circles

### 6. Event Management (20 entities)
- `event_booth_types` - Booth Types
- `event_events` - Events
- `event_booths` - Booths
- `event_contracts` - Contracts
- `event_gates` - Gates
- `event_gate_types` - Gate Types
- `event_incidents` - Incidents
- `event_invoices` - Invoices
- `event_locations` - Locations
- `event_payments` - Payments
- `event_permits` - Permits
- `event_status` - Status
- `event_tenants` - Tenants
- `event_tickets` - Tickets
- `event_types` - Event Types
- `event_visitors` - Visitors
- `event_zones` - Zones
- `event_zone_types` - Zone Types
- `event_incident_types` - Incident Types
- `event_payment_methods` - Payment Methods
- `event_permit_types` - Permit Types
- `event_ticket_types` - Ticket Types
- `event_visitor_types` - Visitor Types

### 7. HR Module (35+ entities)
- `hr_analytics_report` - Analytics Report
- `hr_attendance` - Attendance
- `hr_attendance_device` - Attendance Device
- `hr_attendance_log` - Attendance Log
- `hr_attendance_shift` - Attendance Shift
- `hr_benefit_plan` - Benefit Plan
- `hr_manager_report` - Manager Report
- `hr_document` - Document
- `hr_employee` - Employee
- `hr_employee_gosi` - Employee GOSI
- `hr_employee_insurance` - Employee Insurance
- `hr_insurance_employee` - Insurance Employee
- `hr_insurance_type` - Insurance Type
- `hr_leave_balance` - Leave Balance
- `hr_leave_request` - Leave Request
- `hr_leave_type` - Leave Type
- `hr_overtime_request` - Overtime Request
- `hr_payroll` - Payroll
- `hr_payslip` - Payslip
- `hr_position` - Position
- `hr_salary_component` - Salary Component
- `hr_time_off_request` - Time Off Request
- And more...

### 8. Localization (1 entity)
- `sys_localization_language` - Localization Language

### 9. System Module (20+ entities)
- `sys_config` - System Config
- `sys_dept` - Department
- `sys_dict_data` - Dictionary Data
- `sys_dict_type` - Dictionary Type
- `sys_login_log` - Login Log
- `sys_menu` - Menu
- `sys_notice` - Notice
- `sys_oper_log` - Operation Log
- `sys_post` - Post
- `sys_role` - Role
- `sys_role_dept` - Role Department
- `sys_role_menu` - Role Menu
- `sys_tasks` - Tasks
- `sys_tasks_log` - Tasks Log
- `sys_user` - User
- `sys_user_msg` - User Message
- `sys_user_post` - User Post
- `sys_user_role` - User Role
- `userOnlineLog` - User Online Log

## 📝 JSON Format

Each JSON file follows this structure:

```json
{
  "dictType": "table_name",
  "dictName": "Class Name - table_name",
  "status": "0",
  "remark": "Generated from ClassName table (Namespace)",
  "createBy": "system"
}
```

### Example: HR Employee

```json
{
  "dictType": "hr_employee",
  "dictName": "Hr Employee - hr_employee",
  "status": "0",
  "remark": "Generated from HrEmployee table (ZR.Model.HR)",
  "createBy": "system"
}
```

## 🚀 Usage

### Import All Dictionary Types

**Option 1: Use Combined File**
```bash
# Via API
curl -X POST http://localhost:8887/api/system/dictType/bulk-import \
  -H "Content-Type: application/json" \
  -d @all_system_dict_types.json
```

**Option 2: Individual Import**
```bash
# Import specific module
for file in event_*.json; do
    curl -X POST http://localhost:8887/api/system/dictType/import \
      -H "Content-Type: application/json" \
      -d @$file
done
```

### Query Dictionary Types

```sql
-- Get all dictionary types
SELECT dict_type, dict_name, remark
FROM sys_dict_type
ORDER BY dict_type;

-- Get by module
SELECT dict_type, dict_name
FROM sys_dict_type
WHERE dict_type LIKE 'event_%'
ORDER BY dict_type;

-- Count by prefix
SELECT
    SUBSTRING(dict_type, 1, POSITION('_' IN dict_type)) as module,
    COUNT(*) as count
FROM sys_dict_type
GROUP BY SUBSTRING(dict_type, 1, POSITION('_' IN dict_type))
ORDER BY count DESC;
```

### Use in Code

```csharp
// Get all system dictionary types
var dictTypes = await _db.Queryable<SysDictType>()
    .OrderBy(d => d.DictType)
    .ToListAsync();

// Get HR module types
var hrTypes = await _db.Queryable<SysDictType>()
    .Where(d => d.DictType.StartsWith("hr_"))
    .ToListAsync();

// Get Event Management types
var eventTypes = await _db.Queryable<SysDictType>()
    .Where(d => d.DictType.StartsWith("event_"))
    .ToListAsync();
```

## 🔄 Regenerate Files

If you add new entities or modify existing ones:

```bash
# 1. Extract entities
bash /tmp/extract_entities.sh > /tmp/all_entities.txt

# 2. Regenerate JSON files
cd ZR.Admin.WebApi/Database/sys_dict_type/all_entities
bash generate_combined.sh
```

## 📊 Entity Distribution

```
AI Module:              4 entities (3%)
Boulevard:              8 entities (6%)
Business:               6 entities (5%)
Content:                6 entities (5%)
Event Management:      20 entities (16%)
HR Module:             35 entities (28%)
System Module:         20 entities (16%)
Others:                25 entities (20%)
```

## 🎯 Key Features

✅ **Complete Coverage** - All 124 SugarTable entities included
✅ **Module Organization** - Grouped by business domain
✅ **Individual Files** - Easy to manage and version control
✅ **Combined File** - Bulk operations support
✅ **Auto-generated** - Consistent naming and structure
✅ **Namespace Tracking** - Know which module owns each entity

## 📚 Related Files

- **../event_management/** - Event Management specific types
- **../sys_dict_type/** - System dictionary types
- **../SeedData/** - Seed data JSON files
- **CATEGORIES.md** - Detailed categorization

## 🔍 Finding Entities

### By Module
```bash
# List all HR entities
ls -1 hr_*.json

# List all Event entities
ls -1 event_*.json

# List all System entities
ls -1 sys_*.json
```

### By Name
```bash
# Find employee-related
grep -l "employee" *.json

# Find type definitions
grep -l "type" *.json
```

## 💡 Tips

1. **Use Combined File** for initial bulk import
2. **Use Individual Files** for incremental updates
3. **Check Namespace** to understand module ownership
4. **Regenerate Regularly** when adding new entities
5. **Version Control** both individual and combined files

## 🛠️ Maintenance

### Adding New Entity

1. Create the entity class with `[SugarTable]` attribute
2. Run extraction script
3. Regenerate JSON files
4. Review and commit changes

### Updating Existing Entity

1. Modify entity class
2. Update corresponding JSON file
3. Regenerate combined file if needed
4. Update documentation

## 📞 Support

For questions or issues:
1. Check entity namespace in remark field
2. Review module documentation
3. Verify SugarTable attribute in source code
4. Check combined file for duplicates

---

**Generated**: October 20, 2025
**Total Entities**: 124
**Format Version**: 1.0
