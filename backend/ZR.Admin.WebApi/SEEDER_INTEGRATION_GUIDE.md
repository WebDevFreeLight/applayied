# Seeder Integration Guide

## 🚀 Quick Start - Add to Program.cs

Copy this code to `Program.cs` to enable automatic seeding on startup:

### Option 1: Auto-run in Development (Recommended)

Add **AFTER** `var app = builder.Build();` line:

```csharp
// ============================================================
// AUTO-RUN SEEDERS IN DEVELOPMENT
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("🌱 Development mode: Running database seeders...");

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeederRunner>>();
            var seederRunner = new SeederRunner(db, logger);

            // Check if menus already exist
            var existingMenuCount = await db.Queryable<ZR.Model.System.SysMenu>().CountAsync();

            if (existingMenuCount == 0)
            {
                app.Logger.LogInformation("📋 No menus found in database. Running seeder...");
                await seederRunner.RunMenuSeederAsync(truncateFirst: false);
            }
            else
            {
                app.Logger.LogInformation("✅ Database already has {MenuCount} menus. Skipping seeder.", existingMenuCount);
                app.Logger.LogInformation("💡 To re-seed, call API: POST /api/system/seeder/menu?truncateFirst=true");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "❌ Failed to run seeders");
        }
    }
}
```

### Option 2: Command Line Argument

Add **BEFORE** `app.Run();` line:

```csharp
// ============================================================
// COMMAND LINE SEEDER EXECUTION
// ============================================================
var args = Environment.GetCommandLineArgs();
if (args.Contains("--seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(db, logger);

        bool truncate = args.Contains("--truncate");
        bool showPermissions = args.Contains("--show-permissions");
        bool showMapping = args.Contains("--show-mapping");

        if (showPermissions)
        {
            await seederRunner.DisplayPermissionsSummaryAsync();
        }
        else if (showMapping)
        {
            await seederRunner.DisplayMappingReviewAsync();
        }
        else
        {
            await seederRunner.RunMenuSeederAsync(truncateFirst: truncate);
        }
    }

    return; // Exit after seeding
}
```

**Usage:**
```bash
# Seed menus (keep existing)
dotnet run --seed

# Seed menus (delete existing first)
dotnet run --seed --truncate

# Show permissions summary
dotnet run --seed --show-permissions

# Show mapping review
dotnet run --seed --show-mapping
```

### Option 3: Manual API Call

No code changes needed. Just call the API endpoints:

```bash
# Using curl
curl -X POST "http://localhost:8887/api/system/seeder/menu?truncateFirst=false" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Using Swagger/Scalar
# Navigate to http://localhost:8887/scalar
# Find POST /api/system/seeder/menu
# Execute with truncateFirst parameter
```

## 📝 Complete Program.cs Example

Here's where to insert the seeder code in Program.cs:

```csharp
// ... existing code ...

var app = builder.Build();

// ============================================================
// 🌱 ADD SEEDER CODE HERE (Option 1 - Auto-run Development)
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("🌱 Development mode: Running database seeders...");

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
                app.Logger.LogInformation("✅ Database has {MenuCount} menus. Skipping.", existingMenuCount);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "❌ Failed to run seeders");
        }
    }
}

// ... existing middleware configuration ...

app.UseRouting();
app.UseCors("Policy1");
app.UseAuthentication();
// ... etc ...

// ============================================================
// 🌱 ADD SEEDER CODE HERE (Option 2 - Command Line)
// ============================================================
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

## 🎯 Execution Flow

### Flow 1: Auto-run on Startup (Development)

```
1. Application starts in Development mode
2. Check if sys_menu table is empty
3. If empty → Run seeder → Insert 87 menus
4. If not empty → Skip seeding
5. Continue application startup
```

### Flow 2: Command Line Execution

```
1. Run: dotnet run --seed
2. Application starts
3. Execute seeder before app.Run()
4. Exit application
5. Database now has seeded data
```

### Flow 3: API Endpoint

```
1. Application running
2. POST /api/system/seeder/menu
3. Seeder executes (requires permission)
4. Returns JSON result
5. Application continues running
```

## 🔧 Customization Options

### Conditional Seeding

```csharp
// Only seed if specific condition
var shouldSeed = Configuration.GetValue<bool>("EnableAutoSeeding", false);
if (shouldSeed && app.Environment.IsDevelopment())
{
    // Run seeder
}
```

### Seeding Multiple Tables

```csharp
// Extend SeederRunner with more methods
await seederRunner.RunMenuSeederAsync();
await seederRunner.RunRoleSeederAsync();  // Add this method
await seederRunner.RunUserSeederAsync();   // Add this method
```

### Environment-Specific Seeding

```csharp
if (app.Environment.IsStaging())
{
    // Seed demo data for staging
    await seederRunner.RunDemoDataSeederAsync();
}
else if (app.Environment.IsProduction())
{
    // Only seed critical reference data
    await seederRunner.RunReferenceDataSeederAsync();
}
```

## ⚙️ Configuration (appsettings.json)

Add seeder configuration:

```json
{
  "Seeder": {
    "EnableAutoSeeding": true,
    "TruncateFirst": false,
    "SeederPath": "Database/Seeders",
    "RunOnStartup": true,
    "SkipIfDataExists": true
  }
}
```

Then use in Program.cs:

```csharp
var seederConfig = builder.Configuration.GetSection("Seeder");
var enableAutoSeeding = seederConfig.GetValue<bool>("EnableAutoSeeding", false);
var skipIfDataExists = seederConfig.GetValue<bool>("SkipIfDataExists", true);

if (enableAutoSeeding && app.Environment.IsDevelopment())
{
    // ... seeder code with skipIfDataExists check
}
```

## 📊 Expected Output

When seeder runs successfully:

```
info: ZR.Admin.WebApi.SeederRunner[0]
      🌱 Starting Menu Seeder...
info: ZR.Admin.WebApi.SeederRunner[0]
      Truncate first: False
info: ZR.Admin.WebApi.SeederRunner[0]
      ✅ Successfully seeded 87 menu items
info: ZR.Admin.WebApi.SeederRunner[0]
         📊 Inserted: 87
```

## 🐛 Troubleshooting

### Issue: "Seeder files not found"

**Solution:**
1. Ensure JSON files are in `Database/Seeders/` directory
2. Add to `.csproj`:
```xml
<ItemGroup>
  <None Update="Database\Seeders\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```
3. Rebuild project

### Issue: "Database connection failed"

**Solution:**
- Check `appsettings.json` → `dbConfigs` → connection strings
- Ensure PostgreSQL is running
- Verify credentials

### Issue: "Permission denied on API call"

**Solution:**
- Login as admin user first
- Add `system:seeder:menu` permission to your role
- Or use auto-run startup method instead

## ✅ Verification Steps

After running seeder:

1. **Check database**:
   ```sql
   SELECT COUNT(*) FROM sys_menu;
   -- Should return: 87
   ```

2. **Check hierarchy**:
   ```sql
   SELECT menu_name, parent_id, order_num
   FROM sys_menu
   ORDER BY order_num;
   ```

3. **Test API**:
   ```bash
   GET /api/system/seeder/status
   ```

4. **Check logs** for successful insertion messages

---

**Status**: ✅ Ready to integrate
**Estimated Time**: 5 minutes
**Risk**: Low (read-only on first run)
