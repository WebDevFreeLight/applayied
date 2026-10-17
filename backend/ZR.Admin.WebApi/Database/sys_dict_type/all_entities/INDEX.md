# Complete System Dictionary Types - Master Index

## 🎯 Quick Stats

| Metric | Value |
|--------|-------|
| **Total Entities** | 121 |
| **Modules** | 8+ |
| **Individual JSON Files** | 121 |
| **Combined JSON File** | 1 (25 KB) |
| **Total Directory Size** | 214 KB |

## 📁 Files in This Directory

| File | Size | Description |
|------|------|-------------|
| `all_system_dict_types.json` | 25 KB | All 121 dict types combined |
| `README.md` | - | Complete documentation |
| `INDEX.md` | - | This file |
| `SUMMARY.txt` | - | Module statistics |
| `generate_combined.sh` | - | Regeneration script |
| `[table_name].json` | ~200B ea. | Individual dict type files (121 files) |

## 📊 Entities by Module

### AI Module (4 entities)
```
ai_agent_mode.json
ai_modelprovider.json
ai_team_mode.json
ai_tooltype.json
```

### Boulevard Module (6 entities)
```
blv_boulevard_season_zones.json
blv_boulevard_seasons_Shops.json
blv_boulevard_shop_category.json
blv_boulevard_shop_status.json
blv_boulevard_shops.json
blv_customerregistrationform.json
```

### Business Partner (8 entities)
```
bp_businesspartner.json
hr_documenttype.json
hr_insurance_plan.json
hr_performance_review.json
hr_reviewtype.json
organizations.json
organizationtype.json
set_admin_settings.json
```

### Content/Article (7 entities)
```
article.json
article_browsing_log.json
article_comment.json
article_praise.json
article_topic.json
article_userCircles.json
articleCategory.json
```

### Event Management (23 entities)
```
event_booths.json
event_booth_types.json
event_contracts.json
event_events.json
event_gates.json
event_gate_types.json
event_incidents.json
event_incident_types.json
event_invoices.json
event_locations.json
event_payments.json
event_payment_methods.json
event_permits.json
event_permit_types.json
event_status.json
event_tenants.json
event_tickets.json
event_ticket_types.json
event_types.json
event_visitors.json
event_visitor_types.json
event_zones.json
event_zone_types.json
```

### HR Module (38 entities)
```
hr_analytics_report.json
hr_attendance.json
hr_attendance_device.json
hr_attendance_log.json
hr_attendance_shift.json
hr_benefit_plan.json
hr_benefits_assignment.json
hr_department.json
hr_document.json
hr_documenttype.json
hr_employee.json
hr_employee_benefits.json
hr_employee_gosi.json
hr_employee_insurance.json
hr_insurance_employee.json
hr_insurance_plan.json
hr_insurance_type.json
hr_leave_balance.json
hr_leave_request.json
hr_leave_type.json
hr_manager_report.json
hr_overtime_request.json
hr_payroll.json
hr_payslip.json
hr_performance_review.json
hr_position.json
hr_reviewtype.json
hr_salary_component.json
hr_time_off_request.json
... and more
```

### System Module (22 entities)
```
sys_config.json
sys_dept.json
sys_dict_data.json
sys_dict_type.json
sys_localization_language.json
sys_login_log.json
sys_menu.json
sys_notice.json
sys_oper_log.json
sys_post.json
sys_role.json
sys_role_dept.json
sys_role_menu.json
sys_tasks.json
sys_tasks_log.json
sys_user.json
sys_user_msg.json
sys_user_post.json
sys_user_role.json
userOnlineLog.json
... and more
```

### Other Modules (13 entities)
```
banner_config.json
common_city.json
common_district.json
gen_table.json
gen_table_column.json
... and more
```

## 🚀 Quick Start

### 1. View Combined File
```bash
cat all_system_dict_types.json | jq '.' | less
```

### 2. Import All Types
```bash
curl -X POST http://localhost:8887/api/system/dictType/bulk-import \
  -H "Content-Type: application/json" \
  -d @all_system_dict_types.json
```

### 3. Import Specific Module
```bash
# Event Management only
cat all_system_dict_types.json | jq '[.[] | select(.dictType | startswith("event_"))]' > event_types.json

curl -X POST http://localhost:8887/api/system/dictType/bulk-import \
  -H "Content-Type: application/json" \
  -d @event_types.json
```

## 📝 JSON Format

Each file contains:

```json
{
  "dictType": "table_name",          // Database table name
  "dictName": "Display Name",        // Human-readable name
  "status": "0",                     // Active status
  "remark": "Generated from...",     // Source information
  "createBy": "system"               // Creator
}
```

## 🔍 Finding Files

### By Pattern
```bash
# All HR entities
ls hr_*.json

# All Event entities
ls event_*.json

# All System entities
ls sys_*.json

# All type definitions
ls *_type*.json
```

### By Content
```bash
# Find by namespace
grep "ZR.Model.HR" *.json

# Find by class name
grep "Employee" *.json

# Count by module
for prefix in hr_ event_ sys_ ai_ blv_ bp_; do
    echo "$prefix: $(ls ${prefix}*.json 2>/dev/null | wc -l)"
done
```

## 📊 Module Distribution

```
HR Module:              38 entities (31%)
Event Management:       23 entities (19%)
System Module:          22 entities (18%)
Business/BP:             8 entities (7%)
Content:                 7 entities (6%)
Boulevard:               6 entities (5%)
AI:                      4 entities (3%)
Other:                  13 entities (11%)
```

## 🛠️ Maintenance

### Regenerate Combined File
```bash
bash generate_combined.sh
```

### Add New Entity
1. Create entity with `[SugarTable]` attribute
2. Run extraction:
   ```bash
   cd /f/mahamBackendNET/backend
   bash /tmp/extract_entities.sh > /tmp/all_entities.txt
   ```
3. Generate new JSON files
4. Regenerate combined file

### Update Existing Entity
1. Modify the individual JSON file
2. Regenerate combined file:
   ```bash
   bash generate_combined.sh
   ```

## 💡 Use Cases

### 1. Dropdown Population
Get all entities of a specific type for dropdown lists:
```csharp
var hrTypes = Directory.GetFiles(
    "Database/sys_dict_type/all_entities",
    "hr_*.json"
).Select(f => JsonSerializer.Deserialize<SysDictType>(File.ReadAllText(f)));
```

### 2. Module Discovery
Find all entities in a module:
```sql
SELECT dict_type, dict_name
FROM sys_dict_type
WHERE remark LIKE '%ZR.Model.HR%'
ORDER BY dict_type;
```

### 3. Bulk Operations
Import all types at once:
```csharp
var allTypes = JsonSerializer.Deserialize<List<SysDictType>>(
    File.ReadAllText("all_system_dict_types.json")
);

await _db.Storageable(allTypes)
    .WhereColumns(it => it.DictType)
    .ToStorage()
    .AsInsertable.ExecuteCommandAsync();
```

## 📚 Related Documentation

- **README.md** - Complete guide with examples
- **SUMMARY.txt** - Module statistics
- **../event_management/** - Event-specific documentation
- **../sys_dict_type/** - System dictionary documentation

## 🎯 Key Features

✅ **Complete Coverage** - All 121 SugarTable entities
✅ **Module Organization** - Easy to find by business domain
✅ **Dual Format** - Individual and combined files
✅ **Auto-generated** - Consistent structure
✅ **Namespace Tracking** - Know module ownership
✅ **Easy Maintenance** - Simple regeneration script

## 📞 Support

### Common Issues

**Q: Why are there 121 files but 124 entities?**
A: Some entities share the same table name or have duplicates.

**Q: How do I import just one module?**
A: Use jq to filter: `cat all_system_dict_types.json | jq '[.[] | select(.dictType | startswith("hr_"))]'`

**Q: Can I modify the generated files?**
A: Yes, but changes will be overwritten if you regenerate. Modify the source entity instead.

### Getting Help

1. Check individual JSON file for entity details
2. Review README.md for usage examples
3. Check namespace in remark field
4. Verify SugarTable attribute in source code

## 🔗 Quick Links

| Link | Description |
|------|-------------|
| [all_system_dict_types.json](all_system_dict_types.json) | Combined file |
| [README.md](README.md) | Full documentation |
| [SUMMARY.txt](SUMMARY.txt) | Module statistics |
| [generate_combined.sh](generate_combined.sh) | Regeneration script |

---

**Last Updated**: October 20, 2025
**Total Entities**: 121
**Format Version**: 1.0
**Generator**: Automated extraction from SugarTable attributes
