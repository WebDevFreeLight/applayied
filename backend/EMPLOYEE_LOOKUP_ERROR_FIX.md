# Employee Lookup Error Fix - "object does not contain a definition for 'EmployeeId'"

**Date:** 2026-01-06
**Status:** ✅ **FIXED - RESTART REQUIRED**
**Backend PID:** 27964 (must be stopped and restarted)

---

## 🐛 **Problem**

When opening the marketing team page in the frontend, the following error occurred:

```
Error retrieving employee lookup: 'object' does not contain a definition for 'EmployeeId'
```

This error prevented the employee dropdown from loading on the marketing team page.

---

## 🔍 **Root Cause**

The `GetLookupList` method in `HrEmployeeCoreService` was returning `List<dynamic>`, which caused property resolution issues at runtime. When the controller tried to access properties like `e.EmployeeId`, `e.EmployeeCode`, etc., the dynamic type system couldn't resolve them.

**Problem Code:**
```csharp
// Service returned List<dynamic>
public async Task<List<dynamic>> GetLookupList(string search = null)
{
    // ...
    var result = await query.Select((emp, name) => new { ... }).ToListAsync();
    return result.Cast<dynamic>().ToList();  // ❌ Dynamic casting loses type information
}

// Controller tried to access properties on dynamic objects
var result = employees.Select(e => new
{
    value = e.EmployeeId,           // ❌ Property not found!
    label = $"{e.EmployeeCode} - {e.FirstName} {e.LastName}",
    // ...
});
```

---

## ✅ **Solution**

Created a proper DTO class (`EmployeeLookupDto`) and updated the service and interface to use strongly-typed returns instead of `dynamic`.

### **Files Changed:**

#### **1. New File: `ZR.Model/HR/Dto/EmployeeCore/EmployeeLookupDto.cs`**

Created a new DTO for employee lookup responses:

```csharp
namespace ZR.Model.HR.Dto.EmployeeCore
{
    /// <summary>
    /// Employee lookup DTO for dropdowns
    /// Minimal data for performance
    /// </summary>
    public class EmployeeLookupDto
    {
        /// <summary>
        /// Employee ID (Snowflake ID)
        /// </summary>
        public long EmployeeId { get; set; }

        /// <summary>
        /// Employee code (e.g., "EMP001")
        /// </summary>
        public string EmployeeCode { get; set; }

        /// <summary>
        /// Given/First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Family/Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Full Arabic name
        /// </summary>
        public string FullNameAr { get; set; }
    }
}
```

#### **2. Updated: `ZR.Service/HR/EmployeeCore/HrEmployeeCoreService.cs`**

**Changes:**
- Added using statement: `using ZR.Model.HR.Dto.EmployeeCore;`
- Changed return type from `List<dynamic>` to `List<EmployeeLookupDto>`
- Updated Select to create `EmployeeLookupDto` objects instead of anonymous types
- Removed dynamic cast

**Before:**
```csharp
public async Task<List<dynamic>> GetLookupList(string search = null)
{
    var result = await query
        .Select((emp, name) => new
        {
            EmployeeId = emp.EmployeeId,
            EmployeeCode = emp.EmployeeCode,
            FirstName = name.GivenName,
            LastName = name.FamilyName,
            FullNameAr = name.FullNameAr
        })
        .Take(100)
        .ToListAsync();

    return result.Cast<dynamic>().ToList();  // ❌ Problem!
}
```

**After:**
```csharp
public async Task<List<EmployeeLookupDto>> GetLookupList(string search = null)
{
    var result = await query
        .Select((emp, name) => new EmployeeLookupDto  // ✅ Strongly typed!
        {
            EmployeeId = emp.EmployeeId,
            EmployeeCode = emp.EmployeeCode,
            FirstName = name.GivenName,
            LastName = name.FamilyName,
            FullNameAr = name.FullNameAr
        })
        .Take(100)
        .ToListAsync();

    return result;  // ✅ No cast needed!
}
```

#### **3. Updated: `ZR.Service/HR/IService/IHrEmployeeCoreService.cs`**

**Changes:**
- Added using statement: `using ZR.Model.HR.Dto.EmployeeCore;`
- Changed interface signature from `Task<List<dynamic>>` to `Task<List<EmployeeLookupDto>>`

**Before:**
```csharp
Task<List<dynamic>> GetLookupList(string search = null);
```

**After:**
```csharp
Task<List<EmployeeLookupDto>> GetLookupList(string search = null);
```

#### **4. Controller: `ZR.Admin.WebApi/Controllers/HR/EmployeeCore/HrEmployeeCoreController.cs`**

**No changes needed!** The controller code already works correctly with the strongly-typed DTO:

```csharp
[HttpGet("lookup")]
[LookupPermission("hr:employeecore:employee:lookup", isPublic: true)]
public async Task<IActionResult> GetLookup([FromQuery] string search = null)
{
    try
    {
        var employees = await _employeeCoreService.GetLookupList(search);  // ✅ Now returns List<EmployeeLookupDto>

        var result = employees.Select(e => new
        {
            value = e.EmployeeId,  // ✅ Property accessible!
            label = $"{e.EmployeeCode} - {e.FirstName} {e.LastName}",
            employeeCode = e.EmployeeCode,
            fullName = $"{e.FirstName} {e.LastName}"
        }).ToList();

        return SUCCESS(result);
    }
    catch (Exception ex)
    {
        return ToResponse(ResultCode.FAIL, $"Error retrieving employee lookup: {ex.Message}");
    }
}
```

---

## 📊 **Build Results**

✅ **ZR.Model** - Build succeeded
✅ **ZR.Service** - Build succeeded
❌ **ZR.Admin.WebApi** - Build failed due to file locks (backend running on PID 27964)

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

## 🧪 **Testing After Restart**

### **Test 1: Marketing Team Page**

1. Open the marketing team page in the frontend
2. The page should load without errors
3. The employee dropdown should work correctly

**Expected:** No error messages, employee list loads successfully

### **Test 2: Employee Lookup API**

```http
GET https://localhost:8885/api/hr/employeecore/employee/lookup
```

**Expected Response:**
```json
{
  "code": 200,
  "data": [
    {
      "value": 1976234156241326000,
      "label": "EMP001 - John Doe",
      "employeeCode": "EMP001",
      "fullName": "John Doe"
    },
    ...
  ]
}
```

### **Test 3: Employee Lookup with Search**

```http
GET https://localhost:8885/api/hr/employeecore/employee/lookup?search=EMP001
```

**Expected:** Filtered results matching the search query

---

## 🎯 **Why This Fix Works**

### **Before (Dynamic):**
- ❌ Runtime type resolution
- ❌ No IntelliSense support
- ❌ Properties could fail at runtime
- ❌ Harder to debug
- ❌ Less performant

### **After (Strongly-Typed DTO):**
- ✅ Compile-time type checking
- ✅ Full IntelliSense support
- ✅ Properties guaranteed to exist
- ✅ Easy to debug
- ✅ Better performance

---

## 📝 **Summary**

**Problem:** Employee lookup endpoint was failing with "object does not contain a definition for 'EmployeeId'" error.

**Root Cause:** Service was returning `List<dynamic>` which caused runtime property resolution issues.

**Solution:**
1. Created `EmployeeLookupDto` class with proper properties
2. Updated service to return `List<EmployeeLookupDto>` instead of `List<dynamic>`
3. Updated interface to match new signature
4. Controller code works without changes

**Result:** Strongly-typed, reliable employee lookup that compiles correctly and provides IntelliSense support.

---

## ✅ **Verification Checklist**

After restarting backend:

- [ ] Backend starts successfully without errors
- [ ] Marketing team page loads without errors
- [ ] Employee dropdown shows employees correctly
- [ ] GET `/api/hr/employeecore/employee/lookup` returns employee list
- [ ] Search parameter filters employees correctly
- [ ] No errors in browser console
- [ ] No errors in backend logs

---

**Status:** ✅ **READY FOR TESTING**
**Next Action:** Stop backend (PID 27964), restart, and test marketing team page

**All changes compiled successfully in Model and Service projects - restart backend to apply!** 🚀
