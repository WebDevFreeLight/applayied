# Marketing Module - Implementation Report

**Date:** 2026-01-07
**Version:** 1.0
**Status:** Complete

---

## Executive Summary

The Marketing module has been fully refactored to implement HR-driven IAM architecture with role-based data filtering, automatic employee assignment, and proper permission controls.

---

## Changes Implemented

### Backend Changes

#### 1. Role-Based Data Filtering (BpLeadService.cs)
- **Admin users** now see ALL leads in the system
- **Non-admin users** see ONLY leads assigned to them
- User-to-employee linking via email/phone matching
- Empty result returned for users without employee profile

#### 2. Auto-Assignment for Non-Admin Users (BpLeadService.cs)
- When a non-admin user creates a lead without specifying AssignedTo
- The system automatically finds their employee ID via email/phone
- Lead is assigned to that employee

#### 3. Updated Service Interface (IBpLeadService.cs)
```csharp
Task<BpLead> AddLeadAsync(BpLead lead, long? currentUserId = null, bool isAdmin = true);
Task<long?> GetEmployeeIdByUserIdAsync(long userId);
```

#### 4. Updated Controller (BpLeadController.cs)
- Added `GetCurrentUser()` helper method
- Passes user context to service for filtering
- Handles auto-assignment logic

#### 5. Updated DTO (LeadDto.cs)
```csharp
// New properties for role-based filtering
public long? CurrentUserId { get; set; }
public long? CurrentUserEmployeeId { get; set; }
public bool IsCurrentUserAdmin { get; set; }
```

### Frontend Changes

#### 1. Leads Page (page.tsx)
- Added **pagination** with configurable page sizes (10, 20, 50, 100)
- Added **permission checks** for all actions
- Shows "No Permission" message if user lacks `marketing:leads:list`
- Buttons hidden based on permissions:
  - Add button requires `marketing:leads:add`
  - Edit button requires `marketing:leads:edit`
  - Export button requires `marketing:leads:export`
  - Import button requires `marketing:leads:import`

#### 2. Modal Protection
- Modals only render if user has required permission
- Permission errors show toast message instead of redirect
- All modal actions checked before opening

---

## Permissions Matrix

| Permission | Marketing Staff | Marketing Manager | Admin |
|------------|----------------|-------------------|-------|
| marketing:leads:list | ✅ (filtered) | ✅ (all) | ✅ (all) |
| marketing:leads:view | ✅ | ✅ | ✅ |
| marketing:leads:add | ✅ (auto-assign) | ✅ | ✅ |
| marketing:leads:edit | ✅ (own leads) | ✅ | ✅ |
| marketing:leads:delete | ❌ | ✅ | ✅ |
| marketing:leads:export | ❌ | ✅ | ✅ |
| marketing:leads:import | ❌ | ✅ | ✅ |
| marketing:activities:list | ✅ | ✅ | ✅ |
| marketing:activities:add | ✅ | ✅ | ✅ |
| marketing:activities:delete | ❌ | ✅ | ✅ |
| marketing:assignments:assign | ❌ | ✅ | ✅ |
| marketing:employees:list | ✅ | ✅ | ✅ |

---

## Data Filtering Rules

### Admin Users
- **Identification:** `RoleKeys.Contains("admin")` OR `UserId == 1`
- **Data Access:** All leads in the system
- **Can Assign:** Yes, to any employee

### Non-Admin Users
- **Identification:** Does not meet admin criteria
- **Data Access:** Only leads where `AssignedTo == CurrentUserEmployeeId`
- **Employee Linking:** Via `email` or `phone` match between `sys_user` and `hr_employee_core`
- **Auto-Assignment:** Leads created by non-admin are auto-assigned to their employee ID

---

## Files Modified

### Backend
| File | Changes |
|------|---------|
| `ZR.Model/Marketing/Dto/LeadDto.cs` | Added role-based filtering properties |
| `ZR.Service/Marketing/IMarketingService/IBpLeadService.cs` | Updated interface with new parameters |
| `ZR.Service/Marketing/Leads/BpLeadService.cs` | Implemented role-based filtering and auto-assignment |
| `ZR.Admin.WebApi/Controllers/Marketing/BpLeadController.cs` | Added user context passing |

### Frontend
| File | Changes |
|------|---------|
| `src/app/[locale]/(hydrogen)/marketing/leads/page.tsx` | Added pagination and permission checks |

### New Files
| File | Description |
|------|-------------|
| `Scripts/Marketing_Permissions_Setup.sql` | SQL script for permissions setup |
| `Scripts/Marketing_Module_Export.json` | JSON export of permissions and roles |
| `Scripts/MARKETING_MODULE_REPORT.md` | This report |

---

## Verification Steps

### 1. Check Permissions in Database
```sql
SELECT m.menuId, m.menuName, m.perms, r.roleName
FROM sys_menu m
LEFT JOIN sys_role_menu rm ON m.menuId = rm.menuId
LEFT JOIN sys_role r ON rm.roleId = r.roleId
WHERE m.perms LIKE 'marketing:%'
ORDER BY m.perms;
```

### 2. Check User Permissions
```sql
SELECT u.userName, m.perms
FROM sys_user u
JOIN sys_user_role ur ON u.userId = ur.userId
JOIN sys_role_menu rm ON ur.roleId = rm.roleId
JOIN sys_menu m ON rm.menuId = m.menuId
WHERE u.userName = 'YOUR_USERNAME'
AND m.perms LIKE 'marketing:%';
```

### 3. Check User-Employee Linking
```sql
SELECT u.userId, u.userName, u.email, u.phonenumber,
       e.employeeId, c.email as empEmail, c.mobilePhone
FROM sys_user u
LEFT JOIN hr_employee_core e ON
    (u.email = (SELECT c.email FROM hr_person_contact c WHERE c.personId = e.personId))
    OR (u.phonenumber = (SELECT c.mobilePhone FROM hr_person_contact c WHERE c.personId = e.personId))
WHERE u.userName = 'YOUR_USERNAME';
```

---

## Troubleshooting

### Issue: 403 Forbidden Error
**Cause:** User doesn't have required permission
**Solution:**
1. Run the SQL script `Marketing_Permissions_Setup.sql`
2. Assign user to appropriate role (marketing_staff or marketing_manager)
3. Clear backend permission cache

### Issue: Empty Leads List (Non-Admin)
**Cause:** User not linked to employee
**Solution:**
1. Ensure user's email or phone matches employee record
2. Or create employee record with matching email/phone

### Issue: Modals Show Permission Error
**Cause:** Missing specific action permission
**Solution:**
1. Add required permission to user's role
2. Check browser console for which permission is needed

---

## Testing Checklist

- [ ] Admin user can see all leads
- [ ] Non-admin user sees only assigned leads
- [ ] Add Lead auto-assigns for non-admin
- [ ] Pagination works correctly
- [ ] Export/Import buttons hidden for non-managers
- [ ] Status change modal checks permission
- [ ] Activity/Meeting modals work correctly
- [ ] Empty state shows when no permissions

---

## Deployment Notes

1. **Run SQL Script:** Execute `Marketing_Permissions_Setup.sql` on the database
2. **Clear Cache:** Clear the permission cache after script execution
3. **Assign Roles:** Assign users to appropriate marketing roles
4. **Test:** Verify with both admin and non-admin accounts

---

## Contact

For issues or questions, refer to the JSON export file `Marketing_Module_Export.json` for detailed permission mappings and API endpoint documentation.
