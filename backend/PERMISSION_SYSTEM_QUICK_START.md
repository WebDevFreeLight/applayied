# Permission System - Quick Start Guide

**Last Updated:** 2026-01-05
**Status:** ✅ Phase 1 Complete | 🚧 Phase 2 In Progress

---

## 🎯 **What's Been Done**

### ✅ **Phase 1: Admin Wildcard Permission** - COMPLETE

**Problem:** Admin users couldn't see all sidebar items in the new React frontend.

**Solution:**
1. ✅ Backend now returns `*:*:*` for admin users
2. ✅ Frontend recognizes `*:*:*` as super permission
3. ✅ Admin users see ALL menu items without filtering

**Files Modified:**
- `ZR.ServiceCore/Services/SysPermissionService.cs`
- `permission-filter.ts` (frontend)

**Documentation:** `PERMISSION_SYSTEM_FIX_SUMMARY.md`

---

## 🚧 **Phase 2: Lookup Permission System** - IN PROGRESS

### **The Problem**

Users have permission to edit leads (`marketing:leads:edit`) but can't see employee dropdown because it requires `hr:employee:list`. Giving them `hr:employee:list` exposes the HR Employees management page (unwanted).

### **The Solution**

Implement **separate lookup permissions**:

```
hr:employee:list   → Full page access (management)
hr:employee:lookup → Dropdown access only (read-only reference data)
```

---

## 📋 **Implementation Checklist**

### **Step 1: Run Discovery Scripts** ⏳

#### **Frontend Discovery:**
```bash
cd Internal_Event_Management_System/apps/isomorphic-intl
npx ts-node src/scripts/discover-permissions.ts
```

**Output:** `discovery-output/permission-discovery.json`

**This will:**
- Extract all sidebar menu items
- Map pages to API dependencies
- Identify lookup requirements
- Find missing permissions

#### **Backend Discovery:**
```bash
cd mahamBackendNET/backend/ZR.Admin.WebApi
dotnet run
```

Then add endpoint in `Program.cs`:
```csharp
// In Program.cs (Development only)
using ZR.Admin.WebApi.Discovery;

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/dev/discover-permissions", () =>
    {
        var output = PermissionDiscoveryTool.DiscoverPermissions();
        PermissionDiscoveryTool.SaveToFile(output, "backend-permission-discovery.json");
        return Results.Ok(output);
    });
}
```

**Output:** `backend-permission-discovery.json`

**This will:**
- Scan all controllers
- Extract API endpoints and permissions
- Suggest lookup endpoints
- Identify common lookups

---

### **Step 2: Implement Lookup Permission Attribute** ⏳

Create: `Infrastructure/Attribute/LookupPermissionAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class LookupPermissionAttribute : Attribute
{
    public string LookupPermission { get; set; }
    public bool IsPublicLookup { get; set; }

    public LookupPermissionAttribute(string lookupPermission, bool isPublic = false)
    {
        LookupPermission = lookupPermission;
        IsPublicLookup = isPublic;
    }
}
```

---

### **Step 3: Update Action Permission Filter** ⏳

Modify: `Infrastructure/Filters/ActionPermissionFilter.cs`

Add lookup permission check BEFORE regular permission check:

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    // CHECK 1: Lookup permission (new)
    var lookupAttr = context.ActionDescriptor.EndpointMetadata
        .OfType<LookupPermissionAttribute>()
        .FirstOrDefault();

    if (lookupAttr != null)
    {
        if (lookupAttr.IsPublicLookup)
        {
            // Any authenticated user
            await next();
            return;
        }

        var userPermissions = GetUserPermissions(context.HttpContext);
        if (userPermissions.Contains("*:*:*") ||
            userPermissions.Contains(lookupAttr.LookupPermission))
        {
            await next();
            return;
        }

        context.Result = new JsonResult(new { code = 403, msg = "No lookup permission" });
        return;
    }

    // CHECK 2: Regular permission (existing logic)
    // ... existing code ...
}
```

---

### **Step 4: Add Lookup Endpoints to Controllers** ⏳

For each entity that needs dropdown support, add a `/lookup` endpoint:

#### **Example: HR Employees**

```csharp
// File: ZR.Admin.WebApi/Controllers/HR/HrEmployeeCoreController.cs

[HttpGet]
[ActionPermissionFilter(Permission = "hr:employee:list")]
public async Task<IActionResult> GetList([FromQuery] EmployeeQueryDto query)
{
    // Full list with pagination - requires full permission
    var result = await _employeeService.GetPagedList(query);
    return SUCCESS(result);
}

// ✅ NEW: Lookup endpoint
[HttpGet("lookup")]
[LookupPermission("hr:employee:lookup", isPublic: true)]  // Public for all authenticated users
public async Task<IActionResult> GetLookup([FromQuery] string search = null)
{
    // Returns minimal data for dropdowns (id, name only)
    var employees = await _employeeService.GetLookupList(search);

    var result = employees.Select(e => new
    {
        value = e.EmployeeId,
        label = $"{e.EmployeeCode} - {e.FullName}"
    });

    return SUCCESS(result);
}
```

#### **Add Service Method:**

```csharp
// File: ZR.Service/HR/EmployeeCore/HrEmployeeCoreService.cs

public async Task<List<HrEmployeeCore>> GetLookupList(string search = null)
{
    var query = Context.Queryable<HrEmployeeCore>()
        .Where(e => e.Status == "1"); // Active only

    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(e =>
            e.EmployeeCode.Contains(search) ||
            e.FullName.Contains(search));
    }

    return await query
        .Select(e => new HrEmployeeCore
        {
            EmployeeId = e.EmployeeId,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName
        })
        .Take(50) // Limit results
        .ToListAsync();
}
```

---

### **Step 5: Create Common Lookup Endpoints** ⏳

Create lookup endpoints for these common entities:

#### **HR Module:**
- [x] `GET /api/hr/employees/lookup` - Employee dropdown
- [ ] `GET /api/hr/departments/lookup` - Department dropdown
- [ ] `GET /api/hr/positions/lookup` - Position dropdown

#### **System Module:**
- [ ] `GET /api/system/users/lookup` - User dropdown
- [ ] `GET /api/system/roles/lookup` - Role dropdown
- [ ] `GET /api/system/depts/lookup` - Department dropdown

#### **Finance Module:**
- [ ] `GET /api/finance/customers/lookup` - Customer dropdown
- [ ] `GET /api/finance/vendors/lookup` - Vendor dropdown
- [ ] `GET /api/finance/accounts/lookup` - GL Account dropdown
- [ ] `GET /api/finance/currencies/lookup` - Currency dropdown
- [ ] `GET /api/finance/taxcodes/lookup` - Tax Code dropdown
- [ ] `GET /api/finance/costcenters/lookup` - Cost Center dropdown

#### **Event Module:**
- [ ] `GET /api/event/types/lookup` - Event Type dropdown
- [ ] `GET /api/event/statuses/lookup` - Event Status dropdown
- [ ] `GET /api/event/locations/lookup` - Location dropdown

---

### **Step 6: Frontend Lookup Service** ⏳

Create: `src/app/shared/services/lookup.service.ts`

```typescript
class LookupService {
  private cache = new Map<string, { data: any[]; timestamp: number }>();
  private cacheExpiry = 5 * 60 * 1000; // 5 minutes

  async getEmployees(search?: string): Promise<LookupOption[]> {
    return this.fetchLookup('/api/hr/employees/lookup', { search });
  }

  async getDepartments(search?: string): Promise<LookupOption[]> {
    return this.fetchLookup('/api/hr/departments/lookup', { search });
  }

  async getCustomers(search?: string): Promise<LookupOption[]> {
    return this.fetchLookup('/api/finance/customers/lookup', { search });
  }

  private async fetchLookup(endpoint: string, params?: any): Promise<LookupOption[]> {
    const cacheKey = `${endpoint}:${JSON.stringify(params)}`;
    const cached = this.cache.get(cacheKey);

    if (cached && Date.now() - cached.timestamp < this.cacheExpiry) {
      return cached.data;
    }

    const response = await apiClient.get(endpoint, { params });
    const data = response.data.data;

    this.cache.set(cacheKey, { data, timestamp: Date.now() });
    return data;
  }
}

export const lookupService = new LookupService();
```

---

### **Step 7: Update Frontend Pages** ⏳

Update pages to use lookup endpoints instead of full list endpoints:

#### **Before (❌ Broken):**
```typescript
// Requires hr:employee:list permission
const employees = await employeeService.getList();
```

#### **After (✅ Works):**
```typescript
// Only requires hr:employee:lookup (public)
const employees = await lookupService.getEmployees();
```

---

### **Step 8: Update Permission Seeders** ⏳

Add lookup permissions to `Database/Seeders/permissions.seeder.json`:

```json
{
  "permissions": [
    // Existing permissions
    {
      "permissionId": 1001,
      "permissionName": "hr:employee:list",
      "permissionType": "page"
    },
    {
      "permissionId": 1002,
      "permissionName": "hr:employee:add",
      "permissionType": "button"
    },

    // ✅ NEW: Lookup permissions
    {
      "permissionId": 1100,
      "permissionName": "hr:employee:lookup",
      "permissionType": "lookup",
      "isPublic": true
    },
    {
      "permissionId": 1101,
      "permissionName": "hr:department:lookup",
      "permissionType": "lookup",
      "isPublic": true
    }
  ]
}
```

---

### **Step 9: Update Complete Menu Seeder** ⏳

Run discovery script to generate complete menu list, then update `Database/Seeders/menus.seeder.json` with all 200+ menu items.

---

### **Step 10: Testing** ⏳

#### **Test 1: Admin User**
```bash
# Login as admin
# Expected: See all 200+ menu items
# Expected: All dropdowns work
```

#### **Test 2: Regular User with Page Permission**
```bash
# Login as regular user
# Give permission: marketing:leads:list
# Expected: See Leads page
# Expected: Employee dropdown works (uses hr:employee:lookup)
# Expected: Cannot access HR Employees page
```

#### **Test 3: User Without Lookup Permission**
```bash
# Create custom lookup (not public)
# Login as user without that lookup permission
# Expected: Dropdown is empty or shows error
```

---

## 📁 **Key Files & Locations**

### **Documentation:**
- `COMPREHENSIVE_PERMISSION_ARCHITECTURE.md` - Full system design
- `PERMISSION_SYSTEM_FIX_SUMMARY.md` - Admin wildcard fix
- `PERMISSION_SYSTEM_QUICK_START.md` - This file

### **Discovery Tools:**
- Frontend: `src/scripts/discover-permissions.ts`
- Backend: `Tools/PermissionDiscoveryTool.cs`

### **Seeders:**
- `Database/Seeders/menus.seeder.json`
- `Database/Seeders/permissions.seeder.json`

### **Backend Core:**
- `Infrastructure/Attribute/LookupPermissionAttribute.cs` (to be created)
- `Infrastructure/Filters/ActionPermissionFilter.cs` (to be updated)
- `ZR.ServiceCore/Services/SysPermissionService.cs` (✅ updated)

### **Frontend Core:**
- `src/app/shared/utils/permission-filter.ts` (✅ updated)
- `src/app/shared/services/lookup.service.ts` (to be created)

---

## 🚀 **Quick Commands**

### **Run Frontend Discovery:**
```bash
cd Internal_Event_Management_System/apps/isomorphic-intl
npx ts-node src/scripts/discover-permissions.ts
```

### **Run Backend Discovery:**
```bash
cd mahamBackendNET/backend/ZR.Admin.WebApi
# Add endpoint and hit: GET /api/dev/discover-permissions
```

### **Build & Test:**
```bash
# Backend
cd mahamBackendNET/backend
dotnet build
cd ZR.Admin.WebApi
dotnet run

# Frontend
cd Internal_Event_Management_System/apps/isomorphic-intl
npm run dev
```

---

## 🎯 **Success Criteria**

- [x] Admin users see all menu items (✅ Complete)
- [x] Admin wildcard `*:*:*` works (✅ Complete)
- [ ] Lookup permissions implemented
- [ ] All common lookups available
- [ ] Frontend uses lookup endpoints
- [ ] Seeders updated with complete data
- [ ] Users can access pages and use dropdowns
- [ ] Page permissions separate from lookup permissions

---

## 💬 **Need Help?**

Refer to:
1. `COMPREHENSIVE_PERMISSION_ARCHITECTURE.md` - Detailed architecture
2. `PERMISSION_SYSTEM_FIX_SUMMARY.md` - Admin wildcard implementation
3. Discovery output JSONs - Generated data

---

**Next Step:** Run the discovery scripts to see what's currently in your system!

