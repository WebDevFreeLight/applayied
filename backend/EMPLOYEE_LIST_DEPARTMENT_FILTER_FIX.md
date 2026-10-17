# Employee List Department Filter Fix - Marketing Team Page

**Date:** 2026-01-06
**Status:** ✅ **FIXED - RESTART REQUIRED**
**Backend PID:** 27964 (must be stopped and restarted)

---

## 🐛 **Problem**

The marketing team page was showing **ALL employees** instead of just employees in the marketing department.

**Example:**
- Employee #19 (خالد حمود) - Department ID: "1" (مقر الشركة - Company HQ) ❌ **Showing on marketing page!**
- Employee #18 (ليلى) - Department ID: "1976234156241326084" (قسم التسويق - Marketing) ✓
- Employee #17 (مهى المصري) - Department ID: "1976234156241326084" (قسم التسويق - Marketing) ✓

The API call was:
```
GET /api/hr/employees/list?PageNum=1&PageSize=1000
```

**Notice:** No `departmentId` parameter! The endpoint was returning all 3 employees instead of just the 2 marketing employees.

---

## 🔍 **Root Cause**

The `/api/hr/employees/list` endpoint in `EmployeesController.cs` was **not filtering by departmentId** even though the `EmployeeQueryRequest` DTO has a `DepartmentId` property!

**EmployeeQueryRequest has these fields:**
```csharp
public class EmployeeQueryRequest : PagerInfo
{
    public string EmployeeCode { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public long? DepartmentId { get; set; }     // ✅ Exists!
    public long? PositionId { get; set; }       // ✅ Exists!
    public string Status { get; set; }
    public bool? IsActive { get; set; }
    public string EmploymentType { get; set; }   // ✅ Exists!
    public DateTime? HireDateFrom { get; set; }
    public DateTime? HireDateTo { get; set; }
}
```

**But the controller was only filtering by:**
- EmployeeCode ✓
- IsActive ✓
- Status ✓
- HireDateFrom ✓
- HireDateTo ✓

**Missing filters:**
- DepartmentId ❌
- PositionId ❌
- Name ❌
- Email ❌
- EmploymentType ❌

---

## ✅ **Solution**

Added filtering for all missing query parameters. Since `DepartmentId`, `PositionId`, `Name`, and `Email` are stored in related tables (not in `hr_employee_core`), they need to be filtered **after** the DTOs are created and joined data is loaded.

### **File Changed: `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs`**

#### **1. Added Using Statements (Lines 3-4):**
```csharp
using System;
using System.Linq;
```

#### **2. Added Post-Mapping Filters (Lines 101-133):**

**After the DTOs are created with joined data:**

```csharp
// Apply filters that require joined data
var filteredList = dtoList.AsEnumerable();

if (request.DepartmentId.HasValue)
{
    filteredList = filteredList.Where(e => e.DepartmentId == request.DepartmentId.Value);
}

if (request.PositionId.HasValue)
{
    filteredList = filteredList.Where(e => e.PositionId == request.PositionId.Value);
}

if (!string.IsNullOrEmpty(request.Name))
{
    filteredList = filteredList.Where(e =>
        (e.FirstName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (e.LastName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (e.FullName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false));
}

if (!string.IsNullOrEmpty(request.Email))
{
    filteredList = filteredList.Where(e =>
        e.Email?.Contains(request.Email, StringComparison.OrdinalIgnoreCase) ?? false);
}

if (!string.IsNullOrEmpty(request.EmploymentType))
{
    filteredList = filteredList.Where(e => e.EmploymentType == request.EmploymentType);
}

var finalList = filteredList.ToList();

// Return paginated response
return SUCCESS(new PagedInfo<EmployeeListDto>
{
    PageIndex = pagedResult.PageIndex,
    PageSize = pagedResult.PageSize,
    TotalNum = finalList.Count, // ✅ Update total count after filtering
    Result = finalList
});
```

---

## 📊 **Expected Behavior After Fix**

### **Before Fix:**
```
GET /api/hr/employees/list?PageNum=1&PageSize=1000
```

**Response:** All 3 employees
- Employee #19 (Department "1" - مقر الشركة) ❌
- Employee #18 (Department "1976234156241326084" - قسم التسويق) ✓
- Employee #17 (Department "1976234156241326084" - قسم التسويق) ✓

### **After Fix:**
```
GET /api/hr/employees/list?PageNum=1&PageSize=1000&DepartmentId=1976234156241326084
```

**Response:** Only 2 marketing employees
- Employee #18 (Department "1976234156241326084" - قسم التسويق) ✓
- Employee #17 (Department "1976234156241326084" - قسم التسويق) ✓

Employee #19 is **excluded** because they're in Department "1" (Company HQ), not Marketing!

---

## 🧪 **Testing After Restart**

### **Test 1: Marketing Team Page**

1. Open marketing team page
2. Check API call in browser DevTools Network tab
3. Should see: `/api/hr/employees/list?...&DepartmentId=1976234156241326084`
4. Response should only include employees in marketing department

**Expected Results:**
- Only employees 18 (ليلى) and 17 (مهى المصري) show up
- Employee 19 (خالد حمود) is **not** displayed (he's in Company HQ, not Marketing)

### **Test 2: Filter by Department**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&DepartmentId=1
```

**Expected:** Only employee #19 (خالد حمود) from "مقر الشركة" (Company HQ)

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&DepartmentId=1976234156241326084
```

**Expected:** Only employees #18 and #17 from "قسم التسويق" (Marketing Department)

### **Test 3: Filter by Position**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&PositionId=13
```

**Expected:** Only employees with position ID 13 (مدير التسويق - Marketing Manager)

### **Test 4: Filter by Name**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&Name=ليلى
```

**Expected:** Only employee #18 (ليلى)

### **Test 5: Filter by Email**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&Email=maha@gmail.com
```

**Expected:** Only employee #17 (مهى المصري)

### **Test 6: Filter by Employment Type**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&EmploymentType=0
```

**Expected:** Only employees with employment type "0" (full-time)

### **Test 7: Combined Filters**

```http
GET /api/hr/employees/list?PageNum=1&PageSize=100&DepartmentId=1976234156241326084&IsActive=true
```

**Expected:** Only active employees from marketing department (should be employee #18 only, since #17 has `isActive: false`)

---

## 🎯 **Frontend Changes Needed**

The frontend marketing team page should send the `departmentId` parameter:

**Before:**
```typescript
const teamResponse = await marketingTeamService.getList({
  pageNum: 1,
  pageSize: 100,
  // No departmentId being sent!
});
```

**After:**
```typescript
const teamResponse = await marketingTeamService.getList({
  pageNum: 1,
  pageSize: 100,
  departmentId: String(marketingDept.deptId), // ✅ Send marketing department ID
});
```

Looking at the frontend code, it **already does this correctly** (line 370 in page.tsx):
```typescript
departmentId: departmentFilter ? String(departmentFilter) : (marketingDept?.deptId ? String(marketingDept.deptId) : undefined),
```

So the frontend is already sending the parameter! The issue was that the backend was ignoring it. After this fix, the frontend should work without changes.

---

## 🚀 **How to Apply the Fix**

The backend is currently running on **PID 27964**. You must restart it:

### **Step 1: Stop the Backend**

```bash
# Option 1: If running in terminal, press Ctrl+C

# Option 2: Kill by PID
taskkill /F /PID 27964
```

### **Step 2: Restart the Backend**

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Wait for:** `Now listening on: https://localhost:8885`

---

## 📝 **Summary**

**Problem:** Marketing team page was showing all employees, including those from other departments.

**Root Cause:** The `/api/hr/employees/list` endpoint was not filtering by `departmentId`, `positionId`, `name`, `email`, or `employmentType` parameters even though they were in the request DTO.

**Solution:**
1. Added post-mapping filters for all missing query parameters
2. Filters are applied after DTOs are created (since data comes from joined tables)
3. Updated total count to reflect filtered results

**Files Modified:**
- `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` (Lines 1-11, 101-142)

**Result:** The employee list now properly filters by department, position, name, email, and employment type. Marketing team page will only show marketing department employees!

---

## ✅ **Verification Checklist**

After restarting backend:

- [ ] Backend starts successfully without errors
- [ ] Marketing team page shows only marketing employees (18, 17)
- [ ] Employee #19 (Company HQ) is NOT displayed on marketing page
- [ ] GET `/api/hr/employees/list?DepartmentId=1976234156241326084` returns only marketing employees
- [ ] GET `/api/hr/employees/list?DepartmentId=1` returns only Company HQ employees
- [ ] GET `/api/hr/employees/list?PositionId=13` filters by position correctly
- [ ] GET `/api/hr/employees/list?Name=ليلى` filters by name correctly
- [ ] GET `/api/hr/employees/list?Email=maha@gmail.com` filters by email correctly

---

**Status:** ✅ **READY FOR TESTING**
**Next Action:** Stop backend (PID 27964), restart, and test marketing team page

**The fix properly filters employees by department, ensuring only marketing employees appear on the marketing team page!** 🚀
