# 🚀 Quick Start: Run the Menu Seeder

## ✅ Files Are Ready

The seeder files are now in place:
- ✅ `ZR.Admin.WebApi/Database/Seeders/menus.seeder.json`
- ✅ `ZR.Admin.WebApi/Database/Seeders/permissions.seeder.json`
- ✅ `ZR.Admin.WebApi/Database/Seeders/mapping.review.json`

---

## 🎯 Option 1: Run via API (EASIEST - Recommended)

### Step 1: Start your application
```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

### Step 2: Wait for app to start
Look for: `Now listening on: http://localhost:8887`

### Step 3: Call the seeder API

**Using Scalar (http://localhost:8887/scalar):**
1. Open http://localhost:8887/scalar in your browser
2. Find `POST /api/system/seeder/menu`
3. Click "Try it out"
4. Set `truncateFirst` to `false`
5. Click "Execute"

**Or using curl:**
```bash
curl -X POST "http://localhost:8887/api/system/seeder/menu?truncateFirst=false"
```

**Or using PowerShell:**
```powershell
Invoke-WebRequest -Uri "http://localhost:8887/api/system/seeder/menu?truncateFirst=false" -Method POST
```

### Step 4: Verify
```bash
# Check seeder status
curl http://localhost:8887/api/system/seeder/status
```

---

## 🎯 Option 2: Auto-Run on Startup

### Step 1: Open Program.cs
```
C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi\Program.cs
```

### Step 2: Find this line (around line 150):
```csharp
var app = builder.Build();
```

### Step 3: Add this code RIGHT AFTER it:
```csharp
var app = builder.Build();

// 🌱 AUTO-SEED DATABASE (Development only)
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("🌱 Checking database seeders...");

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeederRunner>>();
            var seederRunner = new SeederRunner(db, logger);

            var existingMenuCount = await db.Queryable<ZR.Model.System.SysMenu>().CountAsync();

            if (existingMenuCount == 0)
            {
                app.Logger.LogInformation("📋 No menus found. Running seeder...");
                await seederRunner.RunMenuSeederAsync(truncateFirst: false);
            }
            else
            {
                app.Logger.LogInformation("✅ Database has {MenuCount} menus. Skipping seeder.", existingMenuCount);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "❌ Failed to run seeders");
        }
    }
}

// Continue with rest of Program.cs...
```

### Step 4: Restart your application
The seeder will run automatically on startup if the database is empty.

---

## 🎯 Option 3: Command Line

### Step 1: Open Program.cs

### Step 2: Find this line (near the END, before `app.Run();`):
```csharp
app.Run();
```

### Step 3: Add this code BEFORE it:
```csharp
// Command line seeder
var args = Environment.GetCommandLineArgs();
if (args.Contains("--seed"))
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

app.Run();
```

### Step 4: Run with --seed flag
```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run --seed
```

---

## ✅ Expected Result

After running the seeder, you should see:

```
🌱 Starting Menu Seeder...
Truncate first: False
✅ Successfully seeded 87 menu items
   📊 Inserted: 87
```

### Verify in Database:
```sql
SELECT COUNT(*) FROM sys_menu;
-- Should return: 87

SELECT menu_name, path, perms FROM sys_menu WHERE parent_id = 0;
-- Should show all top-level menus
```

---

## ⚠️ Troubleshooting

### Issue: "Seeder files not found"
**Solution:** Files are in the wrong location.

Check:
```bash
dir C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi\Database\Seeders
```

Should show:
- menus.seeder.json
- permissions.seeder.json
- mapping.review.json

### Issue: "Permission denied" (API call)
**Solution:** Temporarily disable permission check.

Edit `ZR.Admin.WebApi/Controllers/System/SeederController.cs`:
```csharp
[HttpPost("menu")]
// [ActionPermissionFilter(Permission = "system:seeder:menu")]  // ✅ Comment this out
public async Task<IActionResult> SeedMenus([FromQuery] bool truncateFirst = false)
```

### Issue: "Database connection failed"
**Solution:** Check appsettings.json

Verify PostgreSQL connection string:
```json
"dbConfigs": [
  {
    "ConfigId": "0",
    "DbType": 4,
    "ConnectionString": "Host=srv792437.hstgr.cloud;Port=5433;Database=MahamDB;Username=postgres;Password=..."
  }
]
```

---

## 🎯 Recommended Approach

**I recommend Option 1 (API call)** because:
- ✅ No code changes needed
- ✅ Can re-run easily
- ✅ Can check status before and after
- ✅ Works immediately

**Steps:**
1. Start app: `dotnet run`
2. Open Scalar: http://localhost:8887/scalar
3. Execute: `POST /api/system/seeder/menu`
4. Verify: Check console logs

---

**Status:** ✅ Files ready, choose your method above!
