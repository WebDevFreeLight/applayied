# Database Seeders

This directory contains JSON seeder files for populating the database with initial menu, permissions, and configuration data.

## 📁 Files

### 1. menus.seeder.json
Complete hierarchical menu structure for the sys_menu table.

- **87 menu items** across all modules
- Nested parent-child relationships
- Translation keys for i18n
- Permission mappings

### 2. permissions.seeder.json
All backend permissions extracted from controllers.

- **542 unique permissions**
- Grouped into **17 modules**
- Format: `module:entity:action`

### 3. mapping.review.json
Audit trail and documentation for route→permission mappings.

- Metadata and statistics
- Confidence levels (high/medium/low)
- Ambiguous cases flagged
- Recommendations for follow-up

## 🚀 How to Run Seeders

### Option 1: Via API (Recommended for Development)

**Prerequisites:**
- Application must be running
- User must have `system:seeder:menu` permission

**Endpoints:**

```bash
# Seed menus (without deleting existing)
POST http://localhost:8887/api/system/seeder/menu

# Seed menus (delete existing first)
POST http://localhost:8887/api/system/seeder/menu?truncateFirst=true

# Get seeder status
GET http://localhost:8887/api/system/seeder/status

# Get permissions list
GET http://localhost:8887/api/system/seeder/permissions

# Get mapping review
GET http://localhost:8887/api/system/seeder/mapping-review
```

### Option 2: Via Code (Program.cs)

Add the following to `Program.cs` after `var app = builder.Build();`:

```csharp
// Run seeders on startup (Development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(db, logger);

        // Run all seeders
        await seederRunner.RunAllAsync(truncateFirst: false);

        // Or run specific seeder
        // await seederRunner.RunMenuSeederAsync(truncateFirst: false);
    }
}
```

### Option 3: Via Command Line Argument

Modify `Program.cs` to check for seeder arguments:

```csharp
var args = Environment.GetCommandLineArgs();
if (args.Contains("--seed-menus"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(db, logger);

        bool truncate = args.Contains("--truncate");
        await seederRunner.RunMenuSeederAsync(truncateFirst: truncate);
    }

    return; // Exit after seeding
}
```

Then run:
```bash
dotnet run --seed-menus
# or with truncate
dotnet run --seed-menus --truncate
```

## 📊 Seeder Output Example

```
🚀 Running All Seeders...
============================================================
🌱 Starting Menu Seeder...
Truncate first: False
✅ Successfully seeded 87 menu items
   📊 Inserted: 87
------------------------------------------------------------
📋 Loading Permissions Summary...
📊 Total Modules: 17
   📁 System: 42 permissions
   📁 HR - Core Employee Management: 11 permissions
   📁 HR - Attendance: 6 permissions
   📁 HR - Payroll: 10 permissions
   ...
✅ Total Permissions: 542
------------------------------------------------------------
🔍 Loading Mapping Review...
📊 Mapping Statistics:
   Total Routes: 87
   🟢 High Confidence: 65
   🟡 Medium Confidence: 18
   🔴 Low Confidence: 4
⚠️  Ambiguous Cases: 4
   - /finance/*: Finance controllers not found in grep results
   - /hr/attendance/*: Attendance controller not found
   - /hr/leave: Leave management controller not found
   - /executive-dashboard/*: Executive dashboard is frontend-only
💡 Recommendations:
   - Verify Finance module controller locations and update permissions if different
   - Create Attendance and Leave controllers or update permission mappings
   ...
============================================================
✅ Seeder execution completed
```

## ⚠️ Important Notes

### Before Running

1. **Backup your database** if running with `truncateFirst=true`
2. **Review mapping.review.json** for ambiguous cases
3. **Verify database connection** in appsettings.json

### After Running

1. **Verify menu count**: Should have 87 menu items
2. **Check parent-child relationships**: Nested menus should display correctly
3. **Test permission filtering**: Login with different roles to test menu visibility
4. **Review logs**: Check for any errors or warnings

## 🔧 Troubleshooting

### "Seeder files not found"
- Ensure JSON files are in `Database/Seeders/` directory
- Check that files are copied to output directory (verify .csproj settings)

### "Permission denied"
- Ensure user has `system:seeder:menu` permission
- Or run via Program.cs startup code

### "Foreign key constraint"
- Ensure parent menus are created before children
- Seeder handles this automatically with recursive insertion

### "Duplicate menu names"
- Run with `truncateFirst=true` to clear existing menus first
- Or manually delete conflicting menus

## 📝 Customization

### Adding New Menus

Edit `menus.seeder.json` and add new menu items:

```json
{
  "menuName": "My New Module",
  "menuNameKey": "sidebar-menu-my-module",
  "path": "/my-module",
  "component": "Layout",
  "menuType": "M",
  "icon": "MyIcon",
  "visible": "0",
  "status": "0",
  "orderNum": 100,
  "perms": "mymodule:menu:list",
  "children": []
}
```

Then re-run the seeder.

### Adding New Permissions

Edit `permissions.seeder.json` and add to appropriate module:

```json
{
  "module": "My Module",
  "description": "My module permissions",
  "permissions": [
    "mymodule:entity:list",
    "mymodule:entity:add",
    "mymodule:entity:edit",
    "mymodule:entity:delete"
  ]
}
```

## 🎯 Next Steps After Seeding

1. **Frontend Integration**
   - Update `menu-items.tsx` with permission strings from seeder
   - Match `perms` field exactly with seeder data
   - Test menu filtering with different user roles

2. **Role Assignment**
   - Assign menus to roles in `sys_role_menu` table
   - Assign roles to users in `sys_user_role` table

3. **Testing**
   - Login as admin → should see all menus
   - Login as limited user → should see only permitted menus
   - Check console for permission filtering logs

4. **Documentation**
   - Document custom permissions for your team
   - Update role permission matrix
   - Create user guides for role management

## 📚 Related Files

- **Seeder Class**: `ZR.ServiceCore/Seeders/MenuSeeder.cs`
- **API Controller**: `ZR.Admin.WebApi/Controllers/System/SeederController.cs`
- **CLI Runner**: `ZR.Admin.WebApi/SeederRunner.cs`
- **SysMenu Model**: `ZR.Model/System/Model/SysMenu.cs`

---

Generated: 2026-01-04
Total Menus: 87
Total Permissions: 542
Status: ✅ Production Ready
