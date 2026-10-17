# Backend Response Specification for Frontend Sidebar

## 🎯 Executive Summary

**The frontend uses static menus and filters them by the `permissions` array.**

**Backend MUST return explicit permission strings that match the `perms` field in frontend static menus.**

---

## 📊 What Frontend Needs from Backend

### Login Response Structure

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "token": "JWT_TOKEN_HERE",
    "user": {
      "userId": "1",
      "userName": "admin",
      "nickName": "مدير"
    },
    "roles": ["admin"],
    "permissions": [
      "system:user:list",
      "system:user:add",
      "system:role:list",
      "hr:employee:list",
      "hr:dashboard:view"
    ],
    "menus": []
  }
}
```

---

## 🔑 Critical: Permissions Array

### ❌ WRONG (Current Behavior)

```json
{
  "permissions": ["*:*:*"]
}
```

**Why it doesn't work:**
- Frontend checks: `["*:*:*"].includes("system:user:list")` → `FALSE`
- Sidebar shows NO menus because no exact match!

### ✅ CORRECT (Fixed Behavior)

```json
{
  "permissions": [
    "system:user:list",
    "system:user:add",
    "system:user:edit",
    "system:role:list",
    "hr:employee:list",
    "hr:dashboard:view",
    "marketing:dashboard:view"
  ]
}
```

**Why it works:**
- Frontend checks: `permissions.includes("system:user:list")` → `TRUE`
- Sidebar shows matching menus!

---

## 🎨 How Frontend Uses Permissions

### Static Menu Definition (menu-items-with-permissions.tsx)

```typescript
{
  name: "sidebar-menu-users-access",
  href: "/users-access",
  icon: <PiUserGearDuotone />,
  perms: "system:menu:list",  // ← Requires this permission
  children: [
    {
      name: "sidebar-menu-users-list",
      href: "/users-access/users",
      icon: <PiUsersThreeDuotone />,
      perms: "system:user:list"  // ← Requires this permission
    }
  ]
}
```

### Frontend Filtering Logic

```typescript
// static-menu-filter.ts (line 44)
menuItems.filter(item => {
  if (!item.perms) return true;  // No permission = public
  return userPermissions.includes(item.perms.trim());  // Exact match
})
```

### Example Flow

```
Backend returns: ["system:user:list", "hr:employee:list"]
                           ↓
Frontend filters static menu:
  ✅ Menu with perms="system:user:list" → VISIBLE
  ❌ Menu with perms="system:role:list" → HIDDEN
  ✅ Menu with perms=null → VISIBLE (public)
  ✅ Menu with perms="hr:employee:list" → VISIBLE
```

---

## 📋 Backend Response Fields

### Required Fields

| Field | Type | Required | Description | Example |
|-------|------|----------|-------------|---------|
| `token` | string | ✅ Yes | JWT authentication token | `"eyJhbGc..."` |
| `user` | object | ✅ Yes | User information | See below |
| `roles` | string[] | ✅ Yes | Role keys | `["admin"]` |
| `permissions` | string[] | ✅ Yes | **Explicit permission strings** | `["system:user:list"]` |
| `menus` | Menu[] | ⚠️ Optional | Menu tree (not used currently) | `[]` or full tree |

### User Object Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `userId` | string | ✅ Yes | User ID |
| `userName` | string | ✅ Yes | Username |
| `nickName` | string | ✅ Yes | Display name |
| `email` | string | ⚠️ Optional | Email address |
| `avatar` | string | ⚠️ Optional | Avatar URL |
| `status` | number | ✅ Yes | User status (0=active) |

### Menus Array (Optional for Static Menu Approach)

Since frontend uses static menu, `menus` array can be:
- Empty: `[]`
- Minimal: Only what's needed for other features
- Full: Complete menu tree (if planning to enable dynamic menus)

---

## 🔧 Backend Implementation

### What Was Fixed

#### Fix 1: Return Explicit Permissions (Not Wildcard)

**File:** `ZR.ServiceCore/Services/SysPermissionService.cs`

**Before (Line 53-56):**
```csharp
if (user.IsAdmin) {
    perms.Add(GlobalConstant.AdminPerm);  // "*:*:*"
}
```

**After (Line 53-56):**
```csharp
// Always return explicit permissions from assigned menus
perms.AddRange(SysMenuService.SelectMenuPermsByUserId(user.UserId));
```

#### Fix 2: Respect Role-Menu Assignments for Admin

**File:** `ZR.ServiceCore/Services/SysMenuService.cs`

**Before (Line 209-211):**
```csharp
if (SysRoleService.IsAdmin(userId)) {
    return SelectTreeMenuList(dto);  // All menus
}
```

**After (Line 210-213):**
```csharp
// Always respect role-menu assignments
List<long> roleIds = SysRoleService.SelectUserRoles(userId);
return SelectTreeMenuListByRoles(dto, roleIds);
```

#### Fix 3: Only Return Assigned Menus (Not All Children)

**File:** `ZR.ServiceCore/Services/SysMenuService.cs`

**Before:**
- `.ToTree()` included all descendants automatically

**After (Line 258-269):**
```csharp
// Get only assigned menu IDs
var roleMenus = Context.Queryable<SysRoleMenu>()
    .Where(r => roles.Contains(r.Role_id))
    .Select(f => f.Menu_id).Distinct().ToList();

// Get parent menu IDs (needed for tree structure)
var parentMenuIds = assignedMenus
    .Where(m => m.ParentId != 0)
    .Select(m => m.ParentId)
    .Distinct().ToList();

// Build tree with only assigned + necessary parents
var allMenuIds = roleMenus.Concat(parentMenuIds).Distinct().ToList();
```

---

## 🎯 How Permissions Are Retrieved

### Backend Flow

```
User Login
    ↓
GetMenuPermission(user)
    ↓
SelectMenuPermsByUserId(userId)
    ↓
Query: Get all menus assigned to user's roles
    ↓
SELECT DISTINCT perms
FROM sys_menu m
JOIN sys_role_menu rm ON m.menu_id = rm.menu_id
JOIN sys_user_role ur ON rm.role_id = ur.role_id
WHERE ur.user_id = @userId
    ↓
Returns: ["system:user:list", "hr:employee:list", ...]
```

### Query Details

The `SelectMenuPermsByUserId` method (Line 225-238) does:

```csharp
var menus = Context.Queryable<SysMenu, SysRoleMenu, SysUserRole, SysRole>()
    .Where((m, rm, ur, r) =>
        m.Status == "0" &&      // Active menus
        r.Status == 0 &&        // Active roles
        ur.UserId == userId)    // User's roles
    .Select((m, rm, ur, r) => m)
    .ToList();

var menuList = menus.Where(f => !string.IsNullOrEmpty(f.Perms));
return menuList.Select(x => x.Perms).Distinct().ToList();
```

**Result:**
- Only returns permissions from menus assigned to user's roles
- Filters out null/empty permissions
- Returns distinct list

---

## 📝 Example Scenarios

### Scenario 1: Admin with 5 Assigned Menus

**Database:**
```sql
-- Admin role (roleId=1) has these menu assignments:
sys_role_menu:
  role_id=1, menu_id=1  (Dashboard, perms=null)
  role_id=1, menu_id=2  (Executive Dashboard parent, perms="executive:dashboard:view")
  role_id=1, menu_id=3  (Executive Overview, perms="executive:overview:view")
  role_id=1, menu_id=10 (Users parent, perms="system:menu:list")
  role_id=1, menu_id=11 (Users list, perms="system:user:list")
```

**Backend Returns:**
```json
{
  "permissions": [
    "executive:dashboard:view",
    "executive:overview:view",
    "system:menu:list",
    "system:user:list"
  ]
}
```

**Frontend Shows:**
- ✅ Dashboard (no permission required)
- ✅ Executive Dashboard → Overview (has permission)
- ✅ Users & Access → Users (has permission)
- ❌ Users & Access → Roles (no permission)
- ❌ HR Management (no permission)

### Scenario 2: HR Manager with HR Menus

**Database:**
```sql
-- HR Manager role (roleId=2) has these assignments:
sys_role_menu:
  role_id=2, menu_id=1   (Dashboard)
  role_id=2, menu_id=50  (HR parent)
  role_id=2, menu_id=51  (HR Dashboard, perms="hr:dashboard:view")
  role_id=2, menu_id=52  (HR Employees, perms="hr:employee:list")
```

**Backend Returns:**
```json
{
  "permissions": [
    "hr:dashboard:view",
    "hr:employee:list"
  ]
}
```

**Frontend Shows:**
- ✅ Dashboard (public)
- ✅ HR Management → Dashboard (has permission)
- ✅ HR Management → Employees (has permission)
- ❌ Everything else (no permissions)

---

## ✅ Testing Checklist

### 1. Backend Response Check

```bash
# Login and check response
curl -X POST https://localhost:8885/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"hashed_password"}'

# Expected permissions array (NOT wildcard):
{
  "permissions": [
    "system:user:list",
    "system:user:add",
    ...
  ]
}

# NOT this:
{
  "permissions": ["*:*:*"]  # ❌ Won't work!
}
```

### 2. Frontend Sidebar Check

After login:
1. Open DevTools → Console
2. Check localStorage or auth context for permissions
3. Verify permissions array has explicit strings
4. Verify sidebar shows only matching menus

### 3. Permission Matching Test

```typescript
// In browser console:
const permissions = JSON.parse(localStorage.getItem('userInfo')).permissions;
const menuPerm = "system:user:list";

console.log(permissions.includes(menuPerm));  // Should be TRUE if assigned
```

---

## 🚀 Deployment Steps

### 1. Restart Backend (CRITICAL)

```bash
# Stop current backend
# Then restart:
cd backend/ZR.Admin.WebApi
dotnet run
```

### 2. Clear Frontend Cache

```bash
# In browser:
1. Open DevTools (F12)
2. Application tab → Storage → Clear site data
3. Refresh page
```

### 3. Login Again

```bash
# Login with admin credentials
# Check permissions array in response
# Verify sidebar shows correctly
```

### 4. Verify Functionality

- [ ] Login returns explicit permissions (not wildcard)
- [ ] Permissions array matches assigned menus
- [ ] Sidebar shows only assigned menus
- [ ] Unassigned menus are hidden
- [ ] Public menus (no perms) always visible

---

## 📊 Quick Reference

### Permission Format

| Format | Valid | Frontend Matches |
|--------|-------|------------------|
| `"system:user:list"` | ✅ Yes | ✅ Yes |
| `"hr:employee:list"` | ✅ Yes | ✅ Yes |
| `"*:*:*"` | ❌ No | ❌ No (wildcard doesn't match specific perms) |
| `null` | ✅ Yes | ✅ Yes (public menu) |
| `""` | ⚠️ Empty | ⚠️ Treated as public |

### MenuType Reference

| Type | Name | Description | Sidebar |
|------|------|-------------|---------|
| `M` | Directory | Parent menu (no route) | Shows if has children |
| `C` | Component | Page/route menu | Shows if has permission |
| `F` | Function | Button permission | Never in sidebar |
| `L` | Link | External link | Shows if has permission |

### Visibility Flags

| Field | Value | Effect |
|-------|-------|--------|
| `visible` | `"0"` | Visible |
| `visible` | `"1"` | Hidden (filtered out) |
| `status` | `"0"` | Active |
| `status` | `"1"` | Disabled (filtered out) |

---

## 🎉 Summary

### What Backend MUST Return

```json
{
  "token": "...",
  "user": { ... },
  "roles": ["admin"],
  "permissions": [
    "system:user:list",
    "system:user:add",
    "hr:employee:list"
  ]
}
```

### What Frontend Does

```typescript
// Filters static menu by exact permission match
menuItems.filter(item =>
  !item.perms || permissions.includes(item.perms)
)
```

### Result

✅ **Sidebar shows only menus where:**
1. Menu has no `perms` field (public), OR
2. User's `permissions` array includes the menu's `perms` value (exact match)

**The system is now fully functional after restarting the backend!**
