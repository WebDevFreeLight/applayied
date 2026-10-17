# Permission System Application Guide

## Overview
This guide shows you how to apply the seeded menu permissions to the sidebar in your frontend.

## System Architecture

### Backend
- **Menu Seeder**: `Database/Seeders/menus.seeder.json` contains all menus with `perms` field
- **Role-Menu Table**: `SysRoleMenu` links roles to menus
- **Permissions**: Each menu has a `perms` field (e.g., `"system:user:list"`)

### Frontend
- **Static Menus**: `src/layouts/hydrogen/menu-items-with-permissions.tsx` has hardcoded menus with `perms`
- **Permission Filtering**: `sidebar-menu.tsx` filters menus based on user permissions (line 244-246)
- **Auth Context**: Provides user permissions from backend

## Step-by-Step Application Process

### Step 1: Stop and Restart Backend
```bash
# Stop the current backend process
# Then restart it to load the new SeederController endpoints
cd backend/ZR.Admin.WebApi
dotnet run
```

### Step 2: Run the Menu Seeder
This will insert all menus from the JSON file into the database.

**API Request:**
```http
POST https://localhost:8885/api/system/seeder/menu?truncateFirst=true
Authorization: Bearer YOUR_JWT_TOKEN
```

**Using curl:**
```bash
curl -X POST "https://localhost:8885/api/system/seeder/menu?truncateFirst=true" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Menu seeding completed successfully!",
  "insertedCount": 150,
  "deletedCount": 80
}
```

### Step 3: Get All Menus to Find Menu IDs
Get all menu IDs to see what was seeded:

**API Request:**
```http
GET https://localhost:8885/api/system/seeder/menus/all
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:**
```json
{
  "success": true,
  "data": [
    {
      "menuId": 1,
      "menuName": "Dashboard",
      "parentId": 0,
      "perms": null,
      "menuType": "C",
      "visible": "0",
      "status": "0"
    },
    {
      "menuId": 2,
      "menuName": "Executive Dashboard",
      "parentId": 0,
      "perms": "executive:dashboard:view",
      "menuType": "M",
      "visible": "0",
      "status": "0"
    },
    ...
  ]
}
```

### Step 4: Clear Old Role-Menu Assignments for Admin Role
Admin role typically has roleId = 1. Clear all its current menu assignments:

**API Request:**
```http
DELETE https://localhost:8885/api/system/seeder/role/1/menus
Authorization: Bearer YOUR_JWT_TOKEN
```

**Expected Response:**
```json
{
  "success": true,
  "deletedCount": 85,
  "message": "Cleared 85 menu assignment(s) from role 1"
}
```

### Step 5: Assign Selected Menus to Admin Role
Choose which menus the admin should see. Start with a small test set:

**Test Menus to Assign (example menu IDs):**
- Dashboard (menuId: 1)
- Executive Dashboard parent (menuId: 2)
- Executive Dashboard > Overview (menuId: 3)
- Users & Access Management parent (menuId: 10)
- Users & Access > Users List (menuId: 11)
- Users & Access > Roles (menuId: 12)
- HR Management parent (menuId: 50)
- HR > Employees (menuId: 51)

**API Request:**
```http
PUT https://localhost:8885/api/system/seeder/role/1/menus
Content-Type: application/json
Authorization: Bearer YOUR_JWT_TOKEN

[1, 2, 3, 10, 11, 12, 50, 51]
```

**Expected Response:**
```json
{
  "success": true,
  "deletedCount": 0,
  "insertedCount": 8,
  "message": "Replaced role menus: removed 0, added 8"
}
```

### Step 6: Test Frontend Sidebar
1. **Logout** from the frontend
2. **Login** again as admin user
3. The sidebar should now show **ONLY** the menus you assigned in Step 5

### Step 7: Verify Permission Filtering Works
The frontend filters menus based on the `perms` field. Check that:

1. Open DevTools Console
2. Look for the permissions array in the auth context
3. Verify that only menus matching user permissions are visible

**Permission Matching Logic:**
- Menu with `perms: "system:user:list"` requires user to have `"system:user:list"` permission
- Menu with no `perms` field is visible to everyone
- Parent menus are shown if at least one child is visible

## Understanding the System

### How Permissions Flow

```
┌──────────────┐
│   Backend    │
│   Login API  │
└──────┬───────┘
       │
       │ Returns JWT + permissions array
       │ e.g., ["system:user:list", "hr:employee:list"]
       ↓
┌──────────────┐
│   Frontend   │
│  Auth Context│
└──────┬───────┘
       │
       │ Stores permissions in context
       ↓
┌──────────────┐
│  Sidebar     │
│  Menu Filter │
└──────┬───────┘
       │
       │ filterStaticMenuByPermissions(menuItems, permissions)
       ↓
┌──────────────┐
│   Rendered   │
│   Sidebar    │
└──────────────┘
```

### Menu Types in Database
- **M** = Module/Parent menu (has children)
- **C** = Component/Page menu (actual page)
- **F** = Function/Button (button-level permission)

### Permission Naming Convention
Format: `module:feature:action`

Examples:
- `system:user:list` = View user list
- `system:user:add` = Add new user
- `hr:employee:edit` = Edit employee
- `executive:dashboard:view` = View executive dashboard

## Complete API Reference

### 1. Seed Menus
```http
POST /api/system/seeder/menu?truncateFirst={true|false}
```

### 2. Get All Menus
```http
GET /api/system/seeder/menus/all
```

### 3. Clear Role Menus
```http
DELETE /api/system/seeder/role/{roleId}/menus
```

### 4. Assign Menus to Role
```http
POST /api/system/seeder/role/{roleId}/menus
Body: [menuId1, menuId2, ...]
```

### 5. Replace Role Menus (Clear + Assign)
```http
PUT /api/system/seeder/role/{roleId}/menus
Body: [menuId1, menuId2, ...]
```

### 6. Get Seeder Status
```http
GET /api/system/seeder/status
```

## Troubleshooting

### Issue: Sidebar shows all menus
**Solution:** Make sure you're logged in and the auth context has permissions

### Issue: Sidebar shows no menus
**Solution:** Check that the admin role has menu assignments in the database

### Issue: Permission filtering not working
**Solution:**
1. Check browser DevTools > Application > Local Storage for JWT token
2. Decode the JWT to verify permissions are included
3. Check the Auth Context in React DevTools

### Issue: Backend 500 error when seeding
**Solution:** Ensure database connection is working and tables exist

## Next Steps

### Recommended Menu Assignment for Admin
For a full admin experience, assign these menu groups:

**Core System:**
- Dashboard (1)
- Executive Dashboard (2-7)
- Users & Access Management (10-20)

**Business Modules:**
- Event Management (30-60)
- HR Management (90-110)
- Financial System (120-180)

**Settings:**
- System Settings (250+)

### For Non-Admin Roles
Create different role menu assignments:

**Example: HR Manager Role**
```json
[1, 90, 91, 92, 93, 94, 95]
```
- Dashboard
- HR Management
- HR > Dashboard
- HR > Employees
- HR > Attendance
- HR > Payroll

**Example: Finance Manager Role**
```json
[1, 120, 121, 122, 123, 124]
```
- Dashboard
- Financial System
- Financial > Dashboard
- Financial > General Ledger
- Financial > Accounts Receivable
- Financial > Accounts Payable

## Database Queries for Manual Testing

### Check Current Role-Menu Assignments
```sql
SELECT rm.role_id, rm.menu_id, m.menu_name, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1
ORDER BY m.order_num;
```

### Check User Roles
```sql
SELECT u.user_id, u.user_name, r.role_id, r.role_name
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role r ON ur.role_id = r.role_id
WHERE u.user_name = 'admin';
```

### View All Seeded Menus
```sql
SELECT menu_id, menu_name, parent_id, perms, menu_type, visible, status
FROM sys_menu
ORDER BY order_num;
```

## Summary

✅ **What We Did:**
1. Fixed ISqlSugarClient dependency injection issue
2. Created MenuSeeder to load menus from JSON
3. Added role-menu management endpoints
4. Frontend already has permission filtering implemented

✅ **What You Need to Do:**
1. Restart backend to load new endpoints
2. Run menu seeder API endpoint
3. Clear old role-menu assignments
4. Assign selected menus to admin role
5. Test in frontend sidebar

✅ **Result:**
- Sidebar will show only menus assigned to user's role
- Permission system fully functional
- Can easily manage which users see which menus
