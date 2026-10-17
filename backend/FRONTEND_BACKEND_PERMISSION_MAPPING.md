# Frontend-Backend Permission System Mapping

## 📊 Complete Data Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                         LOGIN REQUEST                                 │
│  POST /login                                                          │
│  { username, password }                                               │
└────────────────────────────┬──────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       BACKEND RETURNS                                 │
│  {                                                                    │
│    token: "jwt_token",                                                │
│    user: { userId, userName, nickName, ... },                        │
│    roles: ["admin"],                                                  │
│    permissions: ["*:*:*", "system:user:list", "hr:employee:list"],  │
│    menus: [Menu[]]  ← HIERARCHICAL TREE                             │
│  }                                                                    │
└────────────────────────────┬──────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   FRONTEND PROCESSING                                 │
│                                                                       │
│  1. Store: token, user, roles, permissions                           │
│  2. Filter menus by permissions                                      │
│  3. Transform menus to MenuItem structure                            │
│  4. Render sidebar                                                   │
└─────────────────────────────────────────────────────────────────────┘
```

## 🔑 Key Understanding

### TWO Sidebar Approaches in Frontend

#### Approach 1: **Dynamic Menu from Backend** (Currently Disabled)
- Source: `menus` array from login response
- Location: `auth-context.tsx:196-197`
- Status: ✅ Implemented but commented out due to translation issues

```typescript
// auth-context.tsx (line 196-197)
const filteredMenu = filterMenuByPermissions(menus, permissions || []);
const transformedMenu = transformBackendMenu(filteredMenu);
```

#### Approach 2: **Static Menu with Permission Filtering** (Currently Active)
- Source: `menu-items-with-permissions.tsx` (hardcoded)
- Location: `sidebar-menu.tsx:244-246`
- Status: ✅ Currently used

```typescript
// sidebar-menu.tsx (line 244-246)
if (isAuthenticated && permissions.length > 0) {
  const filteredStaticMenu = filterStaticMenuByPermissions(menuItems, permissions);
  return filterBlockedRoutes(filteredStaticMenu);
}
```

## 📋 What Backend MUST Return

### Login Response Structure

```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "userId": "1",
      "userName": "admin",
      "nickName": "مدير",
      "email": null,
      "phonenumber": null,
      "avatar": null,
      "status": 0
    },
    "roles": ["admin"],
    "permissions": [
      "system:user:list",
      "system:user:add",
      "hr:employee:list",
      "hr:dashboard:view"
    ],
    "menus": [
      {
        "menuId": "2007854563268370432",
        "menuName": "Dashboard",
        "menuNameKey": "sidebar-menu-file-manager",
        "parentId": "0",
        "orderNum": 1,
        "path": "/",
        "component": "Layout",
        "menuType": "C",
        "visible": "0",
        "status": "0",
        "perms": null,
        "icon": "FoldersDuotone",
        "children": []
      },
      {
        "menuId": "2007854563327090688",
        "menuName": "Users & Access",
        "menuNameKey": "sidebar-menu-users-access",
        "parentId": "0",
        "orderNum": 2,
        "path": "/users-access",
        "component": "Layout",
        "menuType": "M",
        "visible": "0",
        "status": "0",
        "perms": "system:menu:list",
        "icon": "UserGearDuotone",
        "children": [
          {
            "menuId": "2007854563599720448",
            "menuName": "Users",
            "menuNameKey": "sidebar-menu-users-list",
            "parentId": "2007854563327090688",
            "orderNum": 1,
            "path": "/users-access/users",
            "component": "system/users",
            "menuType": "C",
            "visible": "0",
            "status": "0",
            "perms": "system:user:list",
            "icon": "UsersThreeDuotone",
            "children": []
          }
        ]
      }
    ]
  }
}
```

## 📦 Required Menu Fields

### Essential Fields (MUST have)

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `menuId` | string | Unique menu identifier | `"2007854563268370432"` |
| `menuName` | string | Display name (fallback) | `"Dashboard"` |
| `menuNameKey` | string | **Translation key** (preferred) | `"sidebar-menu-file-manager"` |
| `menuType` | string | M=Directory, C=Menu, F=Button | `"M"` or `"C"` |
| `orderNum` | number | Sort order | `1, 2, 3...` |
| `parentId` | string | Parent menu ID (0=root) | `"0"` or parent ID |

### Permission Control Fields

| Field | Type | Description | Example | Effect |
|-------|------|-------------|---------|--------|
| `perms` | string? | Permission required | `"system:user:list"` | Frontend filters by this |
| `visible` | string | Visibility flag | `"0"` (visible) or `"1"` (hidden) | Hidden if `"1"` |
| `status` | string | Status flag | `"0"` (active) or `"1"` (disabled) | Disabled if `"1"` |

### Optional Fields (for dynamic menu)

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `path` | string | Route path | `"/users-access/users"` |
| `component` | string | Component path | `"system/users"` |
| `icon` | string | Icon name | `"UserGearDuotone"` |
| `children` | Menu[] | Child menus | `[...]` |

## 🎯 How Frontend Filters Menus

### Step 1: Permission Array Matching

```typescript
// menu-filter.ts (line 37-56)
.filter(item => {
  // Check visibility
  if (item.visible === '1') return false;  // Hidden

  // Check status
  if (item.status === '1') return false;   // Disabled

  // Permission check
  if (!item.perms) return true;            // Public (no permission needed)

  // Check if user has this permission
  return userPermissions.includes(item.perms.trim());
})
```

### Step 2: Recursive Children Filtering

```typescript
// menu-filter.ts (line 57-66)
.map(item => {
  if (item.children && item.children.length > 0) {
    const filteredChildren = filterMenuByPermissions(item.children, userPermissions);
    return { ...item, children: filteredChildren };
  }
  return item;
})
```

### Step 3: Remove Empty Parents

```typescript
// menu-filter.ts (line 68-76)
.filter(item => {
  // Remove parent directories (M) with no children
  if (item.menuType === 'M') {
    return item.children && item.children.length > 0;
  }
  return true;
})
```

## 🔐 Permission Matching Logic

### Simple Permission Check
```typescript
// User has this permission?
userPermissions.includes(menuItem.perms)

// Example:
userPermissions = ["system:user:list", "hr:employee:list"]
menuItem.perms = "system:user:list"
// ✅ Match! User can see this menu
```

### Wildcard Permission (Admin)
```typescript
// User permissions
["*:*:*"]

// Special case: Admin wildcard does NOT automatically pass
// Frontend still needs explicit permission match!
// Backend must return explicit permissions or all menu permissions
```

### No Permission = Public
```typescript
// Menu without perms field
menuItem.perms = null  // or undefined
// ✅ Always visible (public menu)
```

## 🎨 Static Menu Filtering (Current Approach)

### How Static Menu Works

```typescript
// menu-items-with-permissions.tsx
export const menuItems: MenuItem[] = [
  {
    name: "sidebar-menu-users-access",
    href: "/users-access",
    icon: <PiUserGearDuotone />,
    perms: "system:menu:list",  // ← Permission required
    children: [
      {
        name: "sidebar-menu-users-list",
        href: "/users-access/users",
        icon: <PiUsersThreeDuotone />,
        perms: "system:user:list"  // ← Permission required
      }
    ]
  }
];

// sidebar-menu.tsx filters this using permissions from backend
const filteredMenu = filterStaticMenuByPermissions(menuItems, permissions);
```

### Static vs Dynamic Menu

| Feature | Static Menu | Dynamic Menu |
|---------|-------------|--------------|
| Source | `menu-items-with-permissions.tsx` | `/login` response |
| Permissions | Hardcoded `perms` field | Backend `perms` field |
| Icons | Hardcoded React components | Icon name string |
| Translations | Hardcoded i18n keys | Backend `menuNameKey` |
| Flexibility | 🔴 Must edit code | 🟢 Database-driven |
| Current Status | ✅ Active | ⚠️ Disabled (translation issues) |

## ✅ What Backend Should Return

### Minimal Response (For Static Menu Approach)

Since frontend is using **static menu + permission filtering**, backend only needs:

```json
{
  "token": "...",
  "user": { ... },
  "roles": ["admin"],
  "permissions": [
    "system:user:list",
    "system:user:add",
    "system:role:list",
    "hr:employee:list",
    "hr:dashboard:view"
  ],
  "menus": []  // Can be empty! Not used currently
}
```

### Full Response (For Dynamic Menu Approach)

If enabling dynamic menus, backend must return:

```json
{
  "token": "...",
  "user": { ... },
  "roles": ["admin"],
  "permissions": [
    "system:user:list",
    ...
  ],
  "menus": [
    {
      "menuId": "...",
      "menuName": "Dashboard",
      "menuNameKey": "sidebar-menu-file-manager",  // ← For i18n
      "menuType": "C",
      "orderNum": 1,
      "parentId": "0",
      "path": "/",
      "icon": "FoldersDuotone",
      "perms": null,
      "visible": "0",
      "status": "0",
      "children": [...]
    }
  ]
}
```

## 🐛 Current Issues & Fixes

### Issue 1: Backend Returns ALL Children
**Problem:** `.ToTree()` includes all descendants, not just assigned menus

**Fix Applied:**
```csharp
// Only include explicitly assigned menus + necessary parents
var roleMenus = Context.Queryable<SysRoleMenu>()
    .Where(r => roles.Contains(r.Role_id))
    .Select(f => f.Menu_id).Distinct().ToList();

var parentMenuIds = assignedMenus
    .Where(m => m.ParentId != 0)
    .Select(m => m.ParentId)
    .Distinct().ToList();

var allMenuIds = roleMenus.Concat(parentMenuIds).Distinct().ToList();
```

### Issue 2: Admin Bypasses Role-Menu Assignments
**Problem:** Admin sees all menus regardless of assignments

**Fix Applied:**
```csharp
// BEFORE
if (SysRoleService.IsAdmin(userId)) {
    return SelectTreeMenuList(dto);  // ❌ All menus
}

// AFTER
List<long> roleIds = SysRoleService.SelectUserRoles(userId);
return SelectTreeMenuListByRoles(dto, roleIds);  // ✅ Only assigned
```

### Issue 3: Permission Wildcard Confusion
**Problem:** `"*:*:*"` permission doesn't automatically give access

**Solution:** Backend must return **explicit permissions** that match menu `perms` fields

Options:
1. Return actual permission list: `["system:user:list", "hr:employee:list", ...]`
2. Frontend checks for wildcard specially (not recommended)

## 📝 Recommendations

### For Current Setup (Static Menu)

1. ✅ **Backend should return only `permissions` array**
   - List all permissions user has
   - Frontend filters static menu by these permissions
   - Simple and works perfectly

2. ✅ **Menus array can be empty or minimal**
   - Not used for sidebar rendering
   - Only used if you switch to dynamic menu approach

### For Future (Dynamic Menu)

1. 🔄 **Fix translation system**
   - Ensure `menuNameKey` matches i18n keys
   - Add missing translations

2. 🔄 **Enable dynamic menu in sidebar-menu.tsx**
   ```typescript
   // Uncomment line 239-241
   if (isAuthenticated && dynamicMenu.length > 0) {
     return filterBlockedRoutes(dynamicMenu);
   }
   ```

3. 🔄 **Backend must return complete menu structure**
   - All required fields
   - Proper icon names
   - Valid translation keys

## 🎯 Summary: What Backend Returns vs What Frontend Uses

| Field | Backend Returns | Frontend Uses | For What |
|-------|-----------------|---------------|----------|
| `permissions` | ✅ `string[]` | ✅ Filters static menu | Permission matching |
| `menus` | ✅ `Menu[]` | ⚠️ Ignored (currently) | Would be for dynamic menu |
| `menus[].perms` | ✅ String | ✅ Matches with permissions | Filter each menu item |
| `menus[].visible` | ✅ "0"/"1" | ✅ Hides if "1" | Hide specific items |
| `menus[].status` | ✅ "0"/"1" | ✅ Disables if "1" | Disable specific items |
| `menus[].menuNameKey` | ✅ String | ⚠️ For i18n (when dynamic) | Translation key |
| `menus[].icon` | ✅ String | ⚠️ Converted to React component | Icon display |

---

## 🚀 Action Items

### For Admin to See Only Assigned Menus:

1. ✅ **Backend fixes applied** (remove admin bypass)
2. 🔄 **Restart backend** to apply fixes
3. 🔄 **Login again** to get fresh permissions
4. ✅ **Frontend already filters by permissions**

### Current Behavior After Fixes:

```
User logs in
  ↓
Backend returns permissions: ["system:user:list", "hr:dashboard:view"]
  ↓
Frontend filters static menu by these permissions
  ↓
Sidebar shows ONLY menus with matching perms
```

**Example:**
- ✅ Menu with `perms: "system:user:list"` → VISIBLE
- ❌ Menu with `perms: "system:role:list"` → HIDDEN
- ✅ Menu with `perms: null` → VISIBLE (public)
