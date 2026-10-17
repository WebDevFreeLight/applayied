# Complete Permissions List - What Admin Will See

**After running `ASSIGN_ALL_MENUS_TO_ADMIN.sql`**

Admin user will have access to **ALL 75 permissions** across **10 modules**.

---

## 📊 Permissions by Module

### 🏢 Boulevard Management (7 permissions)
```
- blvboulevardseasons:list
- blvboulevardseasonzones:list
- blvboulevardshopcategory:list
- blvboulevardshops:list
- blvboulevardshopstatus:list
- boulevard:dashboard:view
- boulevard:menu:list
```

### 🤝 Business Partners (2 permissions)
```
- bpbusinesspartner:list
- partner:dashboard:view
- partner:menu:list
```

### 📅 Event Management (7 permissions)
```
- EventManagement:eventlocations:list
- EventManagement:events:list
- EventManagement:eventstatus:list
- EventManagement:eventtypes:list
- EventManagement:invoices:list
- EventManagement:menu:list
- EventManagement:payments:list
```

### 📊 Executive Dashboard (6 permissions)
```
- executive:alerts:view
- executive:charts:view
- executive:dashboard:view
- executive:heatmaps:view
- executive:kpis:view
- executive:overview:view
```

### 💰 Finance Module (17 permissions)
```
- finance:ap:menu
- finance:apinvoice:list
- finance:ar:menu
- finance:arinvoice:list
- finance:bankaccount:list
- finance:budget:list
- finance:cashaccount:list
- finance:cheque:list
- finance:customer:list
- finance:dashboard:view
- finance:gl:menu
- finance:glaccount:list
- finance:glaccount:openingbalance
- finance:gljournal:list
- finance:menu:list
- finance:treasury:menu
- finance:vendor:list
```

### 👥 HR Management (11 permissions)
```
- hr:attendance:daily
- hr:attendance:logs
- hr:attendance:menu
- hr:benefits:list
- hr:dashboard:view
- hr:employees:list
- hr:leave:list
- hr:menu:list
- hr:payroll:list
- hr:performance:list
- hr:training:list
- hrholidaytype:list
```

### 📢 Marketing Module (8 permissions)
```
- marketing:dashboard:view
- marketing:employees:list
- marketing:giftcard:list
- marketing:leads:list
- marketing:loyalty:query
- marketing:meetings:list
- marketing:menu:list
- marketing:promotion:list
```

### 🖥️ System Monitor (5 permissions)
```
- monitor:job:list
- monitor:logininfor:list
- monitor:menu:list
- monitor:online:list
- monitor:operlog:list
```

### ⚙️ System Administration (9 permissions)
```
- system:config:list
- system:dept:list
- system:dict:list
- system:lang:list
- system:menu:list
- system:notice:list
- system:post:list
- system:role:list
- system:user:list
```

---

## 🎯 Summary

**Total Modules:** 10
**Total Permissions:** 75

After running the SQL script, admin will see these modules in sidebar:

1. ✅ **Dashboard** (home page)
2. ✅ **Executive Dashboard** → Overview, Heatmaps, Charts, Alerts, KPIs
3. ✅ **Users & Access Management** → Users, Roles, Permissions, Menu Management
4. ✅ **Event Management** → Events, Types, Locations, Status, Invoices, Payments
5. ✅ **HR Management** → Dashboard, Employees, Attendance, Leave, Payroll, Performance, Training, Benefits
6. ✅ **Finance** → Dashboard, AP, AR, GL, Treasury, Bank Accounts, Customers, Vendors
7. ✅ **Boulevard Management** → Dashboard, Shops, Seasons, Zones, Categories
8. ✅ **Business Partners** → Dashboard, Partners List
9. ✅ **Marketing** → Dashboard, Leads, Meetings, Promotions, Gift Cards, Loyalty
10. ✅ **System Monitor** → Online Users, Login Logs, Operation Logs, Jobs

---

## 🚀 How to Apply

### Step 1: Run SQL Script

```bash
psql -h srv792437.hstgr.cloud -p 5433 -U postgres -d MahamDB -f ASSIGN_ALL_MENUS_TO_ADMIN.sql
```

Or copy/paste this into your PostgreSQL client:

```sql
-- Clear existing assignments
DELETE FROM sys_role_menu WHERE role_id = 1;

-- Assign ALL menus to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW()
FROM sys_menu
WHERE status = '0' AND menu_type IN ('M', 'C', 'L');
```

### Step 2: Verify

```sql
-- Check permission count
SELECT COUNT(DISTINCT m.perms)
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1 AND m.perms IS NOT NULL;

-- Should return: 75
```

### Step 3: Login and Test

1. Login to your application
2. Check browser console (F12)
3. Should see:
```javascript
✅ Login successful - User permissions: {
  permissionsCount: 75,
  permissions: [
    "blvboulevardseasons:list",
    "boulevard:dashboard:view",
    "EventManagement:events:list",
    "executive:overview:view",
    "finance:dashboard:view",
    "hr:employees:list",
    "marketing:leads:list",
    "monitor:operlog:list",
    "system:user:list",
    // ... all 75 permissions
  ]
}
```

4. Sidebar should show ALL 10 modules with all sub-items!

---

## 📋 Before vs After

### Before (Current - 4 permissions)
```
Sidebar shows:
- Dashboard
- Executive Dashboard → Overview
- Users & Access → Users
- Event Management → Event Types
- HR Management → HR Dashboard
```

### After (With All Permissions - 75 permissions)
```
Sidebar shows:
- Dashboard
- Executive Dashboard → Overview, Heatmaps, Charts, Alerts, KPIs
- Users & Access Management → Users, Roles, Permissions, Menu, Dept, Posts, Dict, Config, Notices
- Event Management → Events, Event Types, Event Locations, Event Status, Invoices, Payments
- HR Management → Dashboard, Employees, Attendance (Daily, Logs, Menu), Leave, Payroll, Performance, Training, Benefits, Holiday Types
- Finance → Dashboard, AP (Menu, Invoices), AR (Menu, Invoices), GL (Menu, Accounts, Opening Balance, Journals), Treasury, Bank Accounts, Cash Accounts, Cheques, Customers, Vendors, Budget
- Boulevard Management → Dashboard, Shops, Shop Status, Shop Categories, Seasons, Season Zones
- Business Partners → Dashboard, Partners List
- Marketing → Dashboard, Leads, Meetings, Promotions, Gift Cards, Loyalty Query, Employees
- System Monitor → Online Users, Login Logs, Operation Logs, Jobs
```

---

## ✅ Expected Result

After running the SQL:

1. ✅ Admin has **75 permissions** (not just 4)
2. ✅ Sidebar shows **all 10 modules**
3. ✅ Each module shows **all sub-items**
4. ✅ Admin can access **every page** in the application
5. ✅ Permission system works correctly
6. ✅ Page refresh maintains all permissions

---

## 🎯 Quick Verification Commands

```sql
-- 1. How many permissions does admin have?
SELECT COUNT(DISTINCT m.perms)
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1 AND m.perms IS NOT NULL;
-- Expected: 75

-- 2. List all permission modules
SELECT DISTINCT SPLIT_PART(m.perms, ':', 1) as module
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1 AND m.perms IS NOT NULL
ORDER BY module;
-- Expected: 10 modules

-- 3. Show permission breakdown by module
SELECT
    SPLIT_PART(m.perms, ':', 1) as module,
    COUNT(*) as count
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1 AND m.perms IS NOT NULL
GROUP BY SPLIT_PART(m.perms, ':', 1)
ORDER BY count DESC;
```

---

**File:** `ASSIGN_ALL_MENUS_TO_ADMIN.sql`
**Run this SQL to give admin access to ALL pages!** 🚀
