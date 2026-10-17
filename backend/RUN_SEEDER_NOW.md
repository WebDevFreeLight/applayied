# 🚀 RUN THE BACKEND SEEDER - Quick Guide

**⏱️ Time Required:** 5 minutes
**Status:** Ready to execute

---

## 📋 What This Does

The seeder will populate your database with:
- **87 menu items** (hierarchical structure)
- Ready for the permission system to use
- Matches the frontend menu structure

---

## ✅ Option 1: Using Scalar (EASIEST) ⭐

### Step 1: Start the Backend

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

### Step 2: Wait for App to Start

Look for this message:
```
Now listening on: http://localhost:8887
```

### Step 3: Open Scalar

Open your browser and go to:
```
http://localhost:8887/scalar
```

### Step 4: Execute the Seeder

1. **Find the endpoint:** Scroll or search for `POST /api/system/seeder/menu`
2. **Click "Try it out"** (if available) or click the endpoint
3. **Set parameters:**
   - `truncateFirst`: Set to `false` (keep existing data)
4. **Click "Execute"** or "Send"

### Step 5: Verify Success

**Console Output:**
```
🌱 Starting Menu Seeder...
Truncate first: False
✅ Successfully seeded 87 menu items
   📊 Inserted: 87
```

**API Response:**
```json
{
  "code": 200,
  "msg": "✅ Successfully seeded 87 menu items",
  "data": {
    "success": true,
    "message": "✅ Successfully seeded 87 menu items",
    "insertedCount": 87,
    "updatedCount": 0,
    "totalCount": 87
  }
}
```

---

## ✅ Option 2: Using PowerShell

### Step 1: Start the Backend (Terminal 1)

```powershell
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

### Step 2: Run Seeder (Terminal 2)

Open a **NEW** PowerShell window and run:

```powershell
Invoke-WebRequest -Uri "http://localhost:8887/api/system/seeder/menu?truncateFirst=false" -Method POST
```

### Step 3: Check Response

You should see:
```
StatusCode        : 200
StatusDescription : OK
Content           : {"code":200,"msg":"✅ Successfully seeded 87 menu items",...}
```

---

## ✅ Option 3: Using Curl (Git Bash / WSL)

### Step 1: Start the Backend (Terminal 1)

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

### Step 2: Run Seeder (Terminal 2)

```bash
curl -X POST "http://localhost:8887/api/system/seeder/menu?truncateFirst=false"
```

---

## 🔍 Verify Database

After running the seeder, check your database:

### Option 1: Using pgAdmin (PostgreSQL)

1. Open pgAdmin
2. Connect to: `srv792437.hstgr.cloud:5433`
3. Database: `MahamDB`
4. Run query:

```sql
-- Check menu count
SELECT COUNT(*) FROM sys_menu;
-- Expected: 87

-- View top-level menus
SELECT menu_id, menu_name, path, perms, order_num
FROM sys_menu
WHERE parent_id = 0
ORDER BY order_num;

-- View all menus with hierarchy
SELECT
  CASE
    WHEN parent_id = 0 THEN menu_name
    ELSE '  → ' || menu_name
  END as menu_hierarchy,
  path,
  perms,
  visible,
  status
FROM sys_menu
ORDER BY
  CASE WHEN parent_id = 0 THEN menu_id ELSE parent_id END,
  parent_id,
  order_num;
```

### Option 2: Using dotnet ef (Code)

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run

# Then access the database through your code
```

---

## ⚠️ Troubleshooting

### Issue: "Connection refused" or "Cannot connect"

**Solution:** Backend is not running

```bash
# Start the backend first
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run

# Wait for: "Now listening on: http://localhost:8887"
```

---

### Issue: "Permission denied" (403)

**Solution:** API requires authentication

**Option A:** Login first and get token
1. Login via Scalar: `POST /api/login`
2. Copy the token
3. Add to request header: `Authorization: Bearer {token}`

**Option B:** Temporarily disable permission check

Edit: `ZR.Admin.WebApi/Controllers/System/SeederController.cs`

```csharp
[HttpPost("menu")]
// [ActionPermissionFilter(Permission = "system:seeder:menu")]  // ✅ Comment this
public async Task<IActionResult> SeedMenus([FromQuery] bool truncateFirst = false)
```

---

### Issue: "Seeder files not found"

**Solution:** Files exist, check path

```bash
# Verify files exist
dir C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi\Database\Seeders

# Should show:
# menus.seeder.json
# permissions.seeder.json
# mapping.review.json
```

Files are there ✅ (verified 2026-01-04)

---

### Issue: "Duplicate menu names"

**Solution:** Run with truncate flag

```bash
# This will delete existing menus first
curl -X POST "http://localhost:8887/api/system/seeder/menu?truncateFirst=true"
```

⚠️ **Warning:** This deletes ALL existing menus!

---

### Issue: Backend won't build

**Solution:** It builds fine ✅

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend
dotnet build ZRAdminMahamBackend.sln

# Should succeed with only warnings
```

---

## 📊 What You Should See

### Console Output (Backend)

```
🌱 Starting Menu Seeder...
============================================================
📋 Seeder Configuration:
   Truncate First: False
   Seeder Path: C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi\Database\Seeders
   Menus File: menus.seeder.json
------------------------------------------------------------
📁 Loading seeder file: menus.seeder.json
✅ Successfully loaded 87 menu items from JSON
------------------------------------------------------------
🔄 Inserting menus recursively...
   📝 Inserting top-level menus...
   📝 Inserting child menus...
   📝 Inserting nested menus...
✅ Successfully seeded 87 menu items
   📊 Inserted: 87
   📊 Updated: 0
   📊 Total: 87
============================================================
✅ Seeder execution completed successfully
```

### Database Result

```sql
SELECT COUNT(*) FROM sys_menu;
-- Returns: 87

SELECT menu_name FROM sys_menu WHERE parent_id = 0;
-- Returns: All top-level menu names
```

---

## ✅ After Seeding - Next Steps

1. **Restart Frontend** (if running)
   ```bash
   # Ctrl+C to stop
   npm run dev
   ```

2. **Login to Test**
   - Open: http://localhost:3000
   - Login as admin
   - Check sidebar menus

3. **Verify Permission Filtering**
   - Console should show: "Permission filtering active"
   - Admin should see all menus (except blocked routes)

4. **Test with Limited User**
   - Create user with limited permissions
   - Login
   - Verify only permitted menus appear

---

## 🎯 Quick Commands Reference

**Start Backend:**
```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Run Seeder (PowerShell):**
```powershell
Invoke-WebRequest -Uri "http://localhost:8887/api/system/seeder/menu?truncateFirst=false" -Method POST
```

**Run Seeder (Curl):**
```bash
curl -X POST "http://localhost:8887/api/system/seeder/menu?truncateFirst=false"
```

**Check Database:**
```sql
SELECT COUNT(*) FROM sys_menu; -- Should be 87
```

---

## 📞 Need Help?

See detailed documentation:
- `QUICK_START_SEEDER.md` - Full guide with all options
- `PERMISSION_SYSTEM_STATUS_AND_NEXT_STEPS.md` - Overview
- `FIX_VISUAL_STUDIO_UNLOADED_PROJECTS.md` - VS issues

---

**Ready? Let's run it!** 🚀

**Recommended:** Use Option 1 (Scalar) - It's the easiest and most visual.

1. `dotnet run`
2. Open http://localhost:8887/scalar
3. Execute `POST /api/system/seeder/menu`
4. Done! ✅

---

**Status:** ✅ Ready to Execute
**Time:** 5 minutes
**Files:** ✅ Verified
**Backend:** ✅ Builds Successfully
