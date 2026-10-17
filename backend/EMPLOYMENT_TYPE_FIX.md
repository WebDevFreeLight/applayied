# Employment Type Display Fix - "رجل" Issue

**Date:** 2026-01-06
**Status:** ✅ **FIXED - RESTART REQUIRED**

---

## 🐛 **Problem**

Employee 19 (خالد) was showing:
```json
{
  "employeeId": "19",
  "employmentType": "رجل"  // ❌ This is "man" (gender), not an employment type!
}
```

Instead of the expected employment type like "full_time", "part_time", etc.

---

## 🔍 **Root Cause**

The frontend sends the **dict_value** (e.g., "1" for part-time), but the backend was storing it directly as the **dict_code**.

**Dictionary Structure:**
```json
{
  "dictCode": 77,           // Unique ID in sys_dict_data table
  "dictValue": "1",         // The value sent by frontend
  "dictType": "sys_employment_type",
  "dictLabel": "part_time"
}
```

**What Was Happening:**
1. Frontend sends: `employmentType: "1"` (dictValue)
2. Backend calls: `ParseDictCode("1")` → returns number `1`
3. Backend stores: `employment_type_dict_code = 1` ❌
4. Problem: `dict_code = 1` is probably a **gender** dictionary entry, not employment type!
5. When displaying: Looks up dict_code=1 → finds "رجل" (man)

**What Should Happen:**
1. Frontend sends: `employmentType: "1"` (dictValue)
2. Backend looks up: `sys_employment_type` where `dictValue = "1"` → finds dict_code=77
3. Backend stores: `employment_type_dict_code = 77` ✅
4. When displaying: Looks up dict_code=77 → finds "part_time"

---

## ✅ **Solution**

Added a new helper method to properly convert `dictValue` → `dictCode` for a specific `dictType`:

**File:** `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs`

### **New Helper Method (Line 941-956):**
```csharp
/// <summary>
/// Get dict_code by looking up dict_value in a specific dict_type
/// Used for employment_type, gender, etc. where frontend sends dictValue
/// </summary>
private long? GetDictCodeByValue(string dictType, string dictValue)
{
    if (string.IsNullOrEmpty(dictValue)) return null;

    try
    {
        var dictDataService = App.GetService<ISysDictDataService>();
        var dictList = dictDataService.SelectDictDataByType(dictType);
        var dictEntry = dictList?.FirstOrDefault(d => d.DictValue == dictValue);
        return dictEntry?.DictCode;
    }
    catch
    {
        return null;
    }
}
```

### **Updated Create Method (Line 354):**
```csharp
// OLD:
EmploymentTypeDictCode = ParseDictCode(request.EmploymentType),  // ❌ Stores "1" as dict_code

// NEW:
EmploymentTypeDictCode = GetDictCodeByValue("sys_employment_type", request.EmploymentType),  // ✅ Looks up dict_code=77
```

### **Updated Update Method (Line 539):**
```csharp
// OLD:
EmploymentTypeDictCode = ParseDictCode(request.EmploymentType),  // ❌ Stores "1" as dict_code

// NEW:
EmploymentTypeDictCode = GetDictCodeByValue("sys_employment_type", request.EmploymentType),  // ✅ Looks up dict_code=77
```

### **Added Using Statements:**
```csharp
using ZR.ServiceCore.Services;
using Infrastructure;
```

---

## 🔄 **Additional Fix: Return DictValue Instead of DictLabel**

**File:** `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`

The API was returning the wrong format for frontend. Now it returns **dictValue** for compatibility.

### **New Helper Method (Line 448-460):**
```csharp
/// <summary>
/// Get DictValue (not DictLabel) for frontend compatibility
/// Frontend uses DictValue for translations and select matching
/// </summary>
private string GetDictValue(long? dictCode)
{
    if (!dictCode.HasValue) return null;
    try
    {
        var data = _dictDataService?.SelectDictDataById(dictCode.Value);
        return data?.DictValue ?? dictCode.Value.ToString();
    }
    catch
    {
        return dictCode.Value.ToString();
    }
}
```

### **Updated ToApiDtoAsync (Line 192):**
```csharp
// OLD:
dto.EmploymentType = employee.EmploymentTypeDictCode?.ToString();  // ❌ Returns "76", "77", etc.

// NEW:
dto.EmploymentType = GetDictValue(employee.EmploymentTypeDictCode);  // ✅ Returns "0", "1", "2", "3"
```

### **Updated ToListDtoAsync (Line 268):**
```csharp
// OLD:
EmploymentType = GetDictLabel("employment_type", employee.EmploymentTypeDictCode)  // ❌ Returns "full_time", "part_time"

// NEW:
EmploymentType = GetDictValue(employee.EmploymentTypeDictCode)  // ✅ Returns "0", "1", "2", "3"
```

---

## 📊 **Employment Type Dictionary**

| dictCode | dictValue | dictLabel | Translation |
|----------|-----------|-----------|-------------|
| 76 | "0" | "full_time" | دوام كامل |
| 77 | "1" | "part_time" | دوام جزئي |
| 78 | "2" | "contract" | عقد |
| 79 | "3" | "intern" | متدرب |

---

## 🧪 **Expected Results After Restart**

### **Before Fix:**
```json
{
  "employeeId": "19",
  "employmentType": "رجل"  // ❌ Wrong! (gender label because wrong dict_code stored)
}
```

### **After Fix:**
```json
{
  "employeeId": "19",
  "employmentType": "0",  // ✅ Correct! (dictValue for full-time)
  "employmentTypeLabel": "full_time"  // ✅ Label for display
}
```

**Note:** The API now returns **dictValue** (not dictLabel) for `employmentType` because:
- Frontend uses dictValue for translations
- Frontend needs dictValue to match select dropdowns
- Frontend sends dictValue, so it should receive dictValue back

---

## 🚀 **How to Apply the Fix**

The backend is currently running. Restart it to apply the changes:

### **Step 1: Stop the Backend**
```bash
# Find the process
tasklist | findstr ZR.Admin.WebApi

# Kill it (replace with actual PID)
taskkill /F /PID <PID>
```

### **Step 2: Restart the Backend**
```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Wait for:** `Now listening on: https://localhost:8885`

---

## 🧪 **Testing After Restart**

### **Test 1: Create New Employee**
```http
POST https://localhost:8885/api/hr/employees
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "employmentType": "1",  // part-time (dictValue)
  ...
}
```

**Expected:**
- Database stores: `employment_type_dict_code = 77`
- Response shows: `"employmentType": "1"` (dictValue for part-time)

### **Test 2: Update Existing Employee**
```http
PUT https://localhost:8885/api/hr/employees
{
  "employeeId": "19",
  "employmentType": "0",  // full-time (dictValue)
  ...
}
```

**Expected:**
- Database stores: `employment_type_dict_code = 76`
- Response shows: `"employmentType": "0"` (dictValue for full-time)
- NOT: `"employmentType": "رجل"` ❌

### **Test 3: List Employees**
```http
GET https://localhost:8885/api/hr/employees
```

**Expected:** All employees show correct employment types as **dictValues**:
```json
{
  "result": [
    {
      "employeeId": "18",
      "employmentType": "0",  // ✅ dictValue (full-time) or null if not set
      "employmentTypeLabel": "full_time"  // ✅ Label for display
    },
    {
      "employeeId": "17",
      "employmentType": "0",  // ✅ dictValue
      "employmentTypeLabel": "full_time"
    },
    {
      "employeeId": "19",
      "employmentType": "0",  // ✅ dictValue NOT "رجل"
      "employmentTypeLabel": "full_time"
    }
  ]
}
```

---

## 🔧 **For Existing Employees with Wrong Data**

Employee 19 currently has the wrong dict_code stored in the database. To fix it:

**Option 1: Update via API**
```http
PUT https://localhost:8885/api/hr/employees
{
  "employeeId": "19",
  "employmentType": "0",  // Set to full-time
  ...all other fields...
}
```

**Option 2: Direct Database Update**
```sql
-- Find the correct dict_code for full-time
SELECT dict_code FROM sys_dict_data
WHERE dict_type = 'sys_employment_type' AND dict_value = '0';
-- Returns: 76

-- Update employee
UPDATE hr_employee_core
SET employment_type_dict_code = 76
WHERE employee_id = 19;
```

---

## 📝 **Files Modified**

| File | Change | Lines |
|------|--------|-------|
| `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` | Added `GetDictCodeByValue` helper | 941-956 |
| `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` | Updated create method | 354 |
| `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` | Updated update method | 539 |
| `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` | Added using statements | 8-9 |
| `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs` | Added `GetDictValue` helper | 448-460 |
| `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs` | Updated ToApiDtoAsync | 192 |
| `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs` | Updated ToListDtoAsync | 268 |

---

## ✅ **Verification Checklist**

After restarting backend:

- [ ] Backend starts successfully without errors
- [ ] Create new employee with `employmentType: "1"` shows `"employmentType": "1"` in response (dictValue)
- [ ] Update employee 19 with `employmentType: "0"` shows `"employmentType": "0"` in response (dictValue)
- [ ] List employees - employee 19 no longer shows "رجل"
- [ ] All employees show correct employment type **dictValues** ("0", "1", "2", "3")
- [ ] Frontend dropdowns can match the dictValue correctly
- [ ] Frontend translator converts dictValue to localized labels

---

## 🎯 **Summary**

**Problem:** Frontend sends dictValue, backend stored it as dict_code, causing wrong labels to appear.

**Solution (2 parts):**

**Part 1 - Store Correct dict_code:**
1. Created `GetDictCodeByValue(dictType, dictValue)` helper method
2. Look up the correct dict_code from sys_dict_data table
3. Store the actual dict_code (76, 77, 78, 79), not the dictValue

**Part 2 - Return Correct dictValue:**
1. Created `GetDictValue(dict_code)` helper method
2. Look up the dictValue from the stored dict_code
3. Return dictValue ("0", "1", "2", "3") to frontend, not dictLabel

**Result:**
- Backend stores the correct dict_code ✅
- Frontend receives the dictValue it expects ✅
- Frontend can match dropdowns and use translations ✅

---

**Status:** ✅ **READY FOR TESTING**
**Next Action:** Restart backend and test employment type display

**All changes compiled successfully - restart backend to apply!** 🚀
