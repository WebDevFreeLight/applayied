# Permission Setup Guide - Step by Step

**Purpose:** Connect users to roles and assign menus/permissions to roles

---

## 🎯 How Permission System Works

```
User → Role → Menus → Permissions
```

**Example:**
- User: "admin" (user_id=1)
- Has Role: "Admin" (role_id=1)
- Role has Menus: Dashboard, HR Dashboard, Users, etc.
- Each Menu has Permission: "hr:dashboard:view", "system:user:list"
- Backend returns flat array: ["hr:dashboard:view", "system:user:list"]
- Frontend filters sidebar by these permissions

---

## 📊 Database Tables

1. **`sys_user`** - Users (admin, hr_user, etc.)
2. **`sys_role`** - Roles (Admin, HR Manager, etc.)
3. **`sys_user_role`** - Connects users to roles
4. **`sys_role_menu`** - Connects roles to menus (THIS IS KEY!)
5. **`sys_menu`** - Menus with their permissions

---

## 🚀 Quick Setup for Admin User

### Step 1: Connect Admin User to Admin Role

```sql
-- Make sure admin user (user_id=1) has admin role (role_id=1)
DELETE FROM sys_user_role WHERE user_id = 1;

INSERT INTO sys_user_role (user_id, role_id, create_by, create_time)
VALUES (1, 1, 'admin', NOW());
```

### Step 2: Give Admin Role ALL Menus

```sql
-- Remove old assignments
DELETE FROM sys_role_menu WHERE role_id = 1;

-- Assign ALL menus to admin role
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    1 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE status = '0';  -- Only active menus
```

### Step 3: Verify It Works

```sql
-- See what permissions admin will get
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1
  AND m.perms IS NOT NULL
  AND m.status = '0';
```

**Expected Output:**
```
system:user:list
hr:dashboard:view
executive:overview:view
EventManagement:eventtypes:list
... (all your permissions)
```

### Step 4: Login and Test

1. Login with admin user
2. Backend should return all these permissions
3. Frontend sidebar shows all menu items

---

## 👥 Create Limited Role (Example: HR Manager)

### Step 1: Create Role

```sql
-- Create HR Manager role
INSERT INTO sys_role (role_id, role_name, role_key, role_sort, status, data_scope, create_by, create_time, remark)
VALUES (10, 'HR Manager', 'hr_manager', 10, 0, 2, 'admin', NOW(), 'HR Manager with HR access only')
ON CONFLICT (role_id) DO NOTHING;
```

### Step 2: Assign Specific Menus to HR Manager Role

**Option A: By Menu Name Key**
```sql
-- Give HR Manager only HR-related menus
DELETE FROM sys_role_menu WHERE role_id = 10;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    10 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE (
    menuname_key = 'sidebar-menu-file-manager'  -- Dashboard
    OR menuname_key LIKE '%hr%'  -- All HR menus
)
AND status = '0';
```

**Option B: By Permission String**
```sql
-- Give HR Manager specific permissions
DELETE FROM sys_role_menu WHERE role_id = 10;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    10 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE perms IN (
    'hr:dashboard:view',
    'hr:employee:list',
    'hr:attendance:view'
)
AND status = '0';
```

### Step 3: Assign User to HR Manager Role

```sql
-- Assign user_id=2 to HR Manager role
INSERT INTO sys_user_role (user_id, role_id, create_by, create_time)
VALUES (2, 10, 'admin', NOW());
```

### Step 4: Verify

```sql
-- See what this user will see
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 2
  AND m.perms IS NOT NULL
  AND m.status = '0';
```

---

## 🔧 Common Tasks

### Task 1: Give Role Access to Specific Page

**Example:** Give role_id=10 access to "Users" page

```sql
-- Find the menu_id for Users page
SELECT menu_id, menu_name, perms
FROM sys_menu
WHERE perms = 'system:user:list';

-- Assign it to role
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
VALUES (10, <menu_id_from_above>, 'admin', NOW());
```

### Task 2: Remove Access to Specific Page

**Example:** Remove role_id=10 access to "Users" page

```sql
-- Find the menu_id
SELECT menu_id FROM sys_menu WHERE perms = 'system:user:list';

-- Remove assignment
DELETE FROM sys_role_menu
WHERE role_id = 10
  AND menu_id = <menu_id_from_above>;
```

### Task 3: See What a User Can Access

```sql
-- Replace user_id=1 with target user
SELECT
    u.user_name,
    r.role_name,
    m.menu_name,
    m.perms
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role r ON ur.role_id = r.role_id
JOIN sys_role_menu rm ON r.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE u.user_id = 1
  AND m.status = '0'
ORDER BY m.ordernum;
```

### Task 4: Copy Permissions from One Role to Another

**Example:** Copy admin role permissions to new role

```sql
-- Copy role_id=1 (admin) permissions to role_id=15 (new role)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    15 as role_id,  -- Target role
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_role_menu
WHERE role_id = 1;  -- Source role
```

---

## 📋 Permission Patterns

### Pattern 1: Full Admin Access
```sql
-- Admin sees everything
DELETE FROM sys_role_menu WHERE role_id = 1;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW()
FROM sys_menu
WHERE status = '0';
```

### Pattern 2: Module-Based Access
```sql
-- HR Manager sees only HR module
DELETE FROM sys_role_menu WHERE role_id = 10;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 10, menu_id, 'admin', NOW()
FROM sys_menu
WHERE (
    menuname_key LIKE '%hr%'
    OR perms LIKE 'hr:%'
)
AND status = '0';
```

### Pattern 3: Read-Only Access
```sql
-- User can LIST but not ADD/EDIT/DELETE
DELETE FROM sys_role_menu WHERE role_id = 20;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 20, menu_id, 'admin', NOW()
FROM sys_menu
WHERE perms LIKE '%:list'  -- Only list/view permissions
   OR perms LIKE '%:view'
AND status = '0';
```

### Pattern 4: Specific Pages Only
```sql
-- Give access to Dashboard + Users + HR Dashboard only
DELETE FROM sys_role_menu WHERE role_id = 30;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 30, menu_id, 'admin', NOW()
FROM sys_menu
WHERE perms IN (
    'system:user:list',
    'hr:dashboard:view'
)
AND status = '0';
```

---

## 🐛 Troubleshooting

### Issue: User sees no menus after login

**Check:**
```sql
-- 1. Does user have a role?
SELECT * FROM sys_user_role WHERE user_id = 1;

-- 2. Does role have menus?
SELECT * FROM sys_role_menu WHERE role_id = 1;

-- 3. What permissions will backend return?
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1
  AND m.perms IS NOT NULL;
```

### Issue: User sees all menus (should see limited)

**Check:**
```sql
-- Count menus assigned to user's role
SELECT
    r.role_name,
    COUNT(rm.menu_id) as menu_count
FROM sys_user_role ur
JOIN sys_role r ON ur.role_id = r.role_id
LEFT JOIN sys_role_menu rm ON r.role_id = rm.role_id
WHERE ur.user_id = 2
GROUP BY r.role_name;

-- If count is too high, role has too many menus assigned
-- Remove unwanted assignments:
DELETE FROM sys_role_menu
WHERE role_id = <role_id>
  AND menu_id IN (
    SELECT menu_id FROM sys_menu
    WHERE menuname_key NOT LIKE '%hr%'  -- Remove non-HR menus
  );
```

### Issue: Permission exists but menu not showing

**Check frontend static menu:**
1. Open `static-menu-structure.tsx`
2. Find the menu item
3. Verify `permission` field matches database `perms` exactly
4. Case-sensitive! Must match exactly

**Example:**
```typescript
// Frontend
{
  name: 'Users',
  permission: 'system:user:list'  // Must match database exactly
}

// Database
perms = 'system:user:list'  // Must match exactly (case-sensitive!)
```

---

## ✅ Verification Checklist

After setup, verify:

- [ ] User has role assigned in `sys_user_role`
- [ ] Role has menus assigned in `sys_role_menu`
- [ ] Menus have `perms` field populated in `sys_menu`
- [ ] Backend returns correct permissions array
- [ ] Frontend sidebar shows correct items
- [ ] Login → Sidebar works
- [ ] Page refresh → Sidebar still works

---

## 🎯 Quick Commands

### Give Admin Full Access
```sql
DELETE FROM sys_role_menu WHERE role_id = 1;
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW() FROM sys_menu WHERE status = '0';
```

### Create Limited User
```sql
-- 1. Create user (if needed)
-- INSERT INTO sys_user (user_id, user_name, nick_name, password, status, dept_id, create_by, create_time)
-- VALUES (2, 'hr_user', 'HR User', '21232f297a57a5a743894a0e4a801fc3', 0, 0, 'admin', NOW());

-- 2. Assign to limited role
INSERT INTO sys_user_role (user_id, role_id) VALUES (2, 10);
```

### View User Permissions
```sql
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1 AND m.perms IS NOT NULL;
```

---

## 📚 Summary

### Tables and Their Purpose:

1. **`sys_user_role`** - Who has what role?
2. **`sys_role_menu`** - What menus does each role have?
3. **`sys_menu.perms`** - What permission does each menu require?

### Permission Flow:

```
User Login
    ↓
Backend queries: user → roles → menus → permissions
    ↓
Returns flat array: ["system:user:list", "hr:dashboard:view"]
    ↓
Frontend filters static menu by these permissions
    ↓
Sidebar shows only matching items
```

### Key SQL:

```sql
-- Connect user to role
INSERT INTO sys_user_role (user_id, role_id) VALUES (1, 1);

-- Give role access to menus
INSERT INTO sys_role_menu (role_id, menu_id)
SELECT 1, menu_id FROM sys_menu WHERE status = '0';

-- Check what user will see
SELECT m.perms FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1;
```

---

**SQL file location:** `Database/SQL_PERMISSION_SETUP.sql`

**Run these scripts in your PostgreSQL database to set up permissions!**
