# Comprehensive Permission Architecture - Auto-Discovery & Lookup System

**Date:** 2026-01-05
**Status:** 🚧 DESIGN PHASE
**Scope:** Complete permission system redesign with automated discovery

---

## 🎯 **Executive Summary**

This document outlines a complete permission architecture that:
1. ✅ **Maintains admin wildcard (`*:*:*`)** - Already implemented
2. 🔄 **Auto-discovers all sidebar items** from frontend
3. 🔄 **Maps pages to API dependencies** automatically
4. 🔄 **Extracts permissions from backend controllers**
5. 🔄 **Solves the lookup/dropdown permission problem**
6. 🔄 **Generates comprehensive permission-to-feature JSON**

---

## 📊 **Problem Analysis**

### **1. Missing Sidebar Items**

**Current State:**
- `menus.seeder.json` has **87 menu items**
- Frontend `menu-items-with-permissions.tsx` has **200+ menu items**
- **Gap:** ~113 missing menu items

**Root Cause:**
- Manual maintenance of seeder JSON
- Frontend has grown with new features
- No automated sync mechanism

### **2. Dropdown/Lookup Permission Issue** ⚠️ **CRITICAL**

**The Problem:**
```
User has permission: "marketing:leads:edit"
Page needs dropdown: Select Employee
Dropdown API: GET /api/hr/employees (requires "hr:employee:list")

❌ User can't see dropdown → Page is broken
❌ Giving "hr:employee:list" → User sees HR Employees page (unwanted)
```

**Real-World Impact:**
- Users can access pages but can't use them (missing dropdowns)
- Granting full permissions exposes unauthorized pages
- Manual permission management becomes impossible at scale

---

## 🏗️ **Proposed Architecture**

### **Tier 1: Permission Types**

```
📁 Page Permissions (Full Access)
   └─ Format: {module}:{entity}:list
   └─ Example: hr:employee:list
   └─ Grants: Full page access + all CRUD operations

📋 Lookup Permissions (Read-Only Reference Data)
   └─ Format: {module}:{entity}:lookup
   └─ Example: hr:employee:lookup
   └─ Grants: Dropdown/select access ONLY (minimal data)

🔧 Action Permissions (Specific Operations)
   └─ Format: {module}:{entity}:{action}
   └─ Example: hr:employee:add, hr:employee:edit
   └─ Grants: Specific button/action access
```

### **Tier 2: Permission Inheritance**

**Option A: Explicit Dependencies** (Recommended)
```json
{
  "permission": "marketing:leads:edit",
  "requires_lookups": [
    "hr:employee:lookup",
    "system:dept:lookup"
  ]
}
```

**Option B: Auto-Grant Common Lookups**
```csharp
// Backend automatically includes common lookups for authenticated users
public static readonly string[] CommonLookups = {
    "hr:employee:lookup",
    "hr:department:lookup",
    "system:user:lookup"
};
```

---

## 🔍 **Auto-Discovery Strategy**

### **Phase 1: Frontend Discovery**

#### **Step 1.1: Sidebar Discovery**
```typescript
// Automated script to extract all menu items
function discoverSidebarItems(menuStructure: MenuItem[]): MenuDiscovery[] {
  const discovered: MenuDiscovery[] = [];

  function traverse(items: MenuItem[], parentPath: string = '') {
    items.forEach(item => {
      discovered.push({
        name: item.name,
        href: item.href,
        permission: item.perms,
        level: parentPath ? parentPath.split('/').length : 0,
        parentPath: parentPath,
        hasChildren: !!item.children
      });

      if (item.children) {
        traverse(item.children, item.href || parentPath);
      }
    });
  }

  traverse(menuStructure);
  return discovered;
}
```

**Output:** `frontend-sidebar-discovery.json`
```json
[
  {
    "name": "sidebar-menu-marketing-leads",
    "href": "/marketing/leads",
    "permission": "marketing:leads:list",
    "level": 2,
    "parentPath": "/marketing",
    "hasChildren": false
  },
  ...
]
```

#### **Step 1.2: API Usage Discovery**
```typescript
// Scan page components for API calls
function discoverPageAPIs(pageFilePath: string): PageAPIMapping {
  const content = fs.readFileSync(pageFilePath, 'utf-8');

  // Pattern matching for API calls
  const patterns = [
    /\w+Service\.(\w+)\(/g,           // leadsService.getList()
    /fetch\(['"]([^'"]+)['"]\)/g,      // fetch('/api/...')
    /axios\.(get|post|put|delete)\(['"]([^'"]+)['"]\)/g
  ];

  const apis = new Set<string>();
  patterns.forEach(pattern => {
    const matches = content.matchAll(pattern);
    for (const match of matches) {
      apis.add(extractAPIEndpoint(match));
    }
  });

  return {
    pagePath: pageFilePath,
    apis: Array.from(apis),
    lookupAPIs: identifyLookupAPIs(Array.from(apis))
  };
}
```

**Output:** `frontend-page-api-mapping.json`
```json
{
  "/marketing/leads/page.tsx": {
    "mainAPIs": [
      "GET /api/marketing/leads",
      "POST /api/marketing/leads",
      "PUT /api/marketing/leads/{id}"
    ],
    "lookupAPIs": [
      "GET /api/hr/employees",  // ← Dropdown
      "GET /api/system/departments"  // ← Dropdown
    ],
    "permissions_required": [
      "marketing:leads:list",
      "hr:employee:lookup",  // ← Auto-detected lookup
      "system:dept:lookup"
    ]
  }
}
```

### **Phase 2: Backend Discovery**

#### **Step 2.1: Controller Permission Extraction**
```csharp
// Automated reflection-based discovery
public class PermissionDiscoveryService
{
    public List<ControllerPermission> DiscoverControllerPermissions()
    {
        var controllers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)));

        var permissions = new List<ControllerPermission>();

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods()
                .Where(m => m.IsPublic && m.GetCustomAttribute<HttpMethodAttribute>() != null);

            foreach (var method in methods)
            {
                var permissionAttr = method.GetCustomAttribute<ActionPermissionFilterAttribute>();
                var route = GetRouteFromMethod(method);

                permissions.Add(new ControllerPermission
                {
                    Controller = controller.Name,
                    Action = method.Name,
                    Route = route,
                    Permission = permissionAttr?.Permission,
                    HttpMethod = GetHttpMethod(method),
                    IsLookupEndpoint = IsLookupEndpoint(method, route)
                });
            }
        }

        return permissions;
    }

    private bool IsLookupEndpoint(MethodInfo method, string route)
    {
        // Heuristics to identify lookup endpoints
        return method.Name.Contains("GetList") &&
               method.GetParameters().Length == 0 ||
               route.Contains("/lookup") ||
               method.ReturnType == typeof(List<SelectListItem>);
    }
}
```

**Output:** `backend-permission-discovery.json`
```json
[
  {
    "controller": "HrEmployeesController",
    "action": "GetList",
    "route": "GET /api/hr/employees",
    "permission": "hr:employee:list",
    "httpMethod": "GET",
    "isLookupEndpoint": true,
    "suggestedLookupPermission": "hr:employee:lookup"
  },
  ...
]
```

---

## 🔧 **Lookup Permission Implementation**

### **Backend Changes**

#### **Change 1: New Lookup Attribute**
```csharp
// File: Infrastructure/Attribute/LookupPermissionAttribute.cs
[AttributeUsage(AttributeTargets.Method)]
public class LookupPermissionAttribute : Attribute
{
    public string LookupPermission { get; set; }
    public bool IsPublicLookup { get; set; } // All authenticated users can access

    public LookupPermissionAttribute(string lookupPermission, bool isPublic = false)
    {
        LookupPermission = lookupPermission;
        IsPublicLookup = isPublic;
    }
}
```

#### **Change 2: Enhanced Permission Filter**
```csharp
// File: Infrastructure/Filters/ActionPermissionFilter.cs
public class ActionPermissionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var lookupAttr = context.ActionDescriptor.EndpointMetadata
            .OfType<LookupPermissionAttribute>()
            .FirstOrDefault();

        if (lookupAttr != null)
        {
            // Check lookup permission (more lenient)
            if (lookupAttr.IsPublicLookup)
            {
                // Any authenticated user can access
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

        // Regular permission check (existing logic)
        var permissionAttr = context.ActionDescriptor.EndpointMetadata
            .OfType<ActionPermissionFilterAttribute>()
            .FirstOrDefault();

        // ... existing permission logic ...
    }
}
```

#### **Change 3: Controller Updates**
```csharp
// File: ZR.Admin.WebApi/Controllers/HR/HrEmployeesController.cs
[ApiController]
[Route("api/hr/employees")]
public class HrEmployeesController : BaseController
{
    // Full list - requires full permission
    [HttpGet]
    [ActionPermissionFilter(Permission = "hr:employee:list")]
    public async Task<IActionResult> GetList([FromQuery] EmployeeQueryDto query)
    {
        // Returns full employee data with pagination
        var result = await _employeeService.GetPagedList(query);
        return SUCCESS(result);
    }

    // ✅ NEW: Lookup endpoint - requires only lookup permission
    [HttpGet("lookup")]
    [LookupPermission("hr:employee:lookup", isPublic: true)]  // ← Public lookup
    public async Task<IActionResult> GetLookup([FromQuery] string search = null)
    {
        // Returns minimal data for dropdowns (id, name only)
        var employees = await _employeeService.GetLookupList(search);
        var result = employees.Select(e => new {
            value = e.EmployeeId,
            label = $"{e.EmployeeCode} - {e.FullName}"
        });
        return SUCCESS(result);
    }

    // CRUD operations - require specific permissions
    [HttpPost]
    [ActionPermissionFilter(Permission = "hr:employee:add")]
    public async Task<IActionResult> Add([FromBody] EmployeeDto dto) { ... }

    [HttpPut("{id}")]
    [ActionPermissionFilter(Permission = "hr:employee:edit")]
    public async Task<IActionResult> Update(long id, [FromBody] EmployeeDto dto) { ... }
}
```

### **Frontend Changes**

#### **Change 1: New Lookup Service**
```typescript
// File: src/app/shared/services/lookup.service.ts
class LookupService {
  private cache = new Map<string, any[]>();
  private cacheExpiry = 5 * 60 * 1000; // 5 minutes

  async getEmployees(search?: string): Promise<LookupOption[]> {
    return this.fetchLookup('/api/hr/employees/lookup', { search });
  }

  async getDepartments(search?: string): Promise<LookupOption[]> {
    return this.fetchLookup('/api/hr/departments/lookup', { search });
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
```

#### **Change 2: Update Pages to Use Lookup Endpoints**
```typescript
// File: src/app/[locale]/(hydrogen)/marketing/leads/page.tsx
// BEFORE:
const employees = await employeeService.getList(); // ❌ Requires hr:employee:list

// AFTER:
const employees = await lookupService.getEmployees(); // ✅ Requires hr:employee:lookup (public)
```

---

## 📁 **Consolidated Permission JSON Structure**

### **Output:** `complete-permission-architecture.json`

```json
{
  "version": "2.0",
  "generated": "2026-01-05T12:00:00Z",
  "admin_permission": "*:*:*",

  "modules": [
    {
      "module": "Marketing",
      "prefix": "marketing",
      "pages": [
        {
          "name": "Leads Management",
          "path": "/marketing/leads",
          "component": "marketing/leads/page.tsx",
          "sidebar_key": "sidebar-menu-marketing-leads",
          "permission": "marketing:leads:list",
          "actions": [
            {
              "action": "add",
              "permission": "marketing:leads:add",
              "button": "Add Lead"
            },
            {
              "action": "edit",
              "permission": "marketing:leads:edit",
              "button": "Edit Lead"
            },
            {
              "action": "delete",
              "permission": "marketing:leads:delete",
              "button": "Delete Lead"
            },
            {
              "action": "export",
              "permission": "marketing:leads:export",
              "button": "Export to Excel"
            }
          ],
          "apis": [
            {
              "endpoint": "GET /api/marketing/leads",
              "permission": "marketing:leads:list",
              "type": "main"
            },
            {
              "endpoint": "POST /api/marketing/leads",
              "permission": "marketing:leads:add",
              "type": "crud"
            }
          ],
          "lookups": [
            {
              "entity": "employees",
              "endpoint": "GET /api/hr/employees/lookup",
              "permission": "hr:employee:lookup",
              "isPublic": true,
              "usage": "Assign lead to employee dropdown"
            },
            {
              "entity": "departments",
              "endpoint": "GET /api/hr/departments/lookup",
              "permission": "hr:department:lookup",
              "isPublic": true,
              "usage": "Department filter dropdown"
            }
          ]
        }
      ]
    },
    {
      "module": "HR",
      "prefix": "hr",
      "pages": [...],
      "lookups": [
        {
          "entity": "employees",
          "endpoint": "GET /api/hr/employees/lookup",
          "permission": "hr:employee:lookup",
          "isPublic": true,
          "description": "Public lookup for all authenticated users"
        },
        {
          "entity": "departments",
          "endpoint": "GET /api/hr/departments/lookup",
          "permission": "hr:department:lookup",
          "isPublic": true,
          "description": "Public lookup for all authenticated users"
        }
      ]
    }
  ],

  "public_lookups": [
    "hr:employee:lookup",
    "hr:department:lookup",
    "system:user:lookup",
    "finance:currency:lookup",
    "finance:taxcode:lookup"
  ],

  "permission_dependencies": {
    "marketing:leads:edit": {
      "requires": ["marketing:leads:list"],
      "requires_lookups": ["hr:employee:lookup"]
    },
    "finance:arinvoice:add": {
      "requires": ["finance:arinvoice:list"],
      "requires_lookups": [
        "finance:customer:lookup",
        "finance:glaccount:lookup",
        "finance:currency:lookup",
        "finance:taxcode:lookup"
      ]
    }
  }
}
```

---

## 🚀 **Implementation Roadmap**

### **Phase 1: Discovery Scripts** (Week 1)
- [ ] Create TypeScript script to extract all sidebar items
- [ ] Create script to map pages to API calls
- [ ] Create C# reflection tool to extract controller permissions
- [ ] Generate consolidated JSON files

### **Phase 2: Backend Lookup System** (Week 2)
- [ ] Create `LookupPermissionAttribute`
- [ ] Enhance `ActionPermissionFilter` to handle lookups
- [ ] Create `/lookup` endpoints for common entities
  - Employees
  - Departments
  - Users
  - Currencies
  - Tax Codes
  - GL Accounts
  - Customers/Vendors
- [ ] Add lookup permissions to database

### **Phase 3: Frontend Lookup Integration** (Week 3)
- [ ] Create centralized `LookupService`
- [ ] Update all pages to use lookup endpoints
- [ ] Implement lookup caching
- [ ] Update permission checks for lookups

### **Phase 4: Database Seeder Updates** (Week 4)
- [ ] Generate complete `menus.seeder.json` from discovery
- [ ] Generate `permissions.seeder.json` including lookups
- [ ] Create `lookup-permissions.seeder.json`
- [ ] Run seeders to update database

### **Phase 5: Testing & Validation** (Week 5)
- [ ] Test admin users (should see everything)
- [ ] Test regular users (should see only permitted pages)
- [ ] Test lookup access (users should see dropdowns without full page access)
- [ ] Validate permission dependencies
- [ ] Performance testing with caching

---

## 📋 **Checklist: Common Lookup Entities**

### **Core System**
- [x] hr:employee:lookup
- [x] hr:department:lookup
- [x] system:user:lookup
- [ ] system:role:lookup
- [ ] system:post:lookup (positions)

### **Finance**
- [ ] finance:customer:lookup
- [ ] finance:vendor:lookup
- [ ] finance:glaccount:lookup
- [ ] finance:currency:lookup
- [ ] finance:taxcode:lookup
- [ ] finance:paymentterm:lookup
- [ ] finance:costcenter:lookup
- [ ] finance:fiscalperiod:lookup

### **Event Management**
- [ ] event:eventtype:lookup
- [ ] event:eventstatus:lookup
- [ ] event:location:lookup
- [ ] event:zonetype:lookup

### **Marketing**
- [ ] marketing:leadstatus:lookup
- [ ] marketing:leadsource:lookup

---

## 💡 **Best Practices**

### **Lookup Endpoint Design**
```csharp
// ✅ Good: Minimal data
public class EmployeeLookupDto
{
    public long EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
}

// ❌ Bad: Full entity
public class EmployeeDto  // Too much data for dropdown
{
    public long EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public decimal Salary { get; set; }  // ← Sensitive data!
    // ... 50 more fields
}
```

### **Permission Naming Conventions**
```
{module}:{entity}:{action}

Examples:
- hr:employee:list       → Full list page access
- hr:employee:lookup     → Dropdown access only
- hr:employee:add        → Create new employee
- hr:employee:edit       → Edit existing employee
- hr:employee:delete     → Delete employee
- hr:employee:export     → Export to Excel
```

### **Frontend Permission Checks**
```typescript
// Page access
if (!hasPermission('hr:employee:list', userPermissions)) {
  redirect('/403');
}

// Button visibility
{hasPermission('hr:employee:add', userPermissions) && (
  <Button onClick={handleAdd}>Add Employee</Button>
)}

// Dropdown access (usually no check needed if using lookup endpoints)
const employees = await lookupService.getEmployees(); // Public lookup
```

---

## 🎯 **Success Criteria**

✅ **Admin users see all 200+ menu items**
✅ **Regular users see only their permitted pages**
✅ **Dropdown fields work for all users with appropriate lookups**
✅ **No manual permission configuration needed for common lookups**
✅ **Automated discovery keeps frontend/backend in sync**
✅ **Clear separation between page access and lookup access**

---

## 📞 **Next Steps**

1. **Review and approve this architecture**
2. **Run discovery scripts to generate JSONs**
3. **Implement lookup permission system**
4. **Update all seeders**
5. **Test with real users**

---

**Document Status:** Draft for Review
**Estimated Implementation Time:** 5 weeks
**Priority:** HIGH - Blocking user workflows

