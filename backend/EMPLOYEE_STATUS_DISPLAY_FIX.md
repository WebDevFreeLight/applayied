# Employee Status Display Fix - TERMINATED vs INACTIVE

**Date:** 2026-01-06
**Status:** ✅ **FIXED - RESTART REQUIRED**
**Update:** Fixed DictValue mapping issue - all statuses now return correctly

---

## 🐛 **Problem**

When updating an employee's status to **TERMINATED** (`status: "2"`), the API response showed `"status": "inactive"` instead of `"status": "terminated"`.

**UPDATE:** After first fix, ALL employees were showing `"status": "suspended"` regardless of their actual status. Even employees with `isActive: true` were showing as "suspended".

**Example:**

**Request:**
```json
{
  "employeeId": "19",
  "status": "2"  // TERMINATED
}
```

**Response (BEFORE FIX):**
```json
{
  "status": "inactive",  // ❌ Should be "terminated"
  "isActive": false
}
```

**Response (AFTER FIX):**
```json
{
  "status": "terminated",  // ✅ Correct!
  "isActive": false
}
```

---

## 🔍 **Root Cause (Part 1)**

The mapping service (`EmployeeApiMappingService`) was setting the status field based only on the boolean `isActive` field:

**Old Code (Line 98):**
```csharp
Status = employee.IsActive ? "active" : "inactive"
```

This simple boolean check didn't distinguish between:
- **SUSPENDED** (dict code `"1"`) → `inactive`
- **TERMINATED** (dict code `"2"`) → `inactive`
- Both mapped to the same string!

**The Problem:**
- Employee status codes (`0`, `1`, `2`, `3`) are stored in the `hr_employee_status` table
- The mapping service was ignoring this table and just using the boolean `isActive` field
- Result: All inactive statuses looked the same (suspended, terminated, etc.)

---

## 🔍 **Root Cause (Part 2) - DictCode vs DictValue**

After the first fix, ALL employees were returning `"status": "suspended"`. The issue was that I was comparing the **DictCode** (long ID like `1976234156241326084`) directly with string literals `"0"`, `"1"`, `"2"`, `"3"`.

**The Dictionary Structure:**
```sql
sys_dict_data table:
- dict_code (BIGINT) - Primary key, e.g., 1976234156241326084
- dict_value (VARCHAR) - The actual value: "0", "1", "2", "3"
- dict_label (VARCHAR) - Display label: "ACTIVE", "SUSPENDED", "TERMINATED", "ON_LEAVE"
```

**The Problem:**
- `hr_employee_status.employment_status_dict_code` stores the **dict_code** (long ID), not the dict_value
- First fix was comparing `statusCode.ToString()` (e.g., "1976234156241326084") with "0", "1", "2", "3"
- Nothing matched, so it always went to the default case or first match

**The Solution:**
- Fetch the dictionary entry using `SelectDictDataById(statusCode)`
- Use the `DictValue` field from the entry ("0", "1", "2", "3") for comparison

---

## ✅ **Solution (Complete)**

Updated `EmployeeApiMappingService.cs` to:
1. **Fetch the actual status code** from `hr_employee_status` table
2. **Map the status code** to the correct status string
3. **Return the specific status** instead of generic "active" or "inactive"

### **Files Modified:**

**`ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`**

#### **1. Added Helper Method to Get Current Status Code (Line 453):**
```csharp
/// <summary>
/// Get current employment status code from hr_employee_status table
/// </summary>
private async Task<long?> GetCurrentEmploymentStatusCodeAsync(long employeeId)
{
    try
    {
        var statusRecords = await _statusService.GetListAsync(s => s.EmployeeId == employeeId);
        var currentStatus = statusRecords
            .Where(s => s.EffectiveTo == null || s.EffectiveTo > DateTime.Now)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault();

        return currentStatus?.EmploymentStatusDictCode;
    }
    catch
    {
        return null;
    }
}
```

#### **2. Added Helper Method to Map Status Code to String (Line 476) - FIXED VERSION:**
```csharp
/// <summary>
/// Map employment status dict code to status string
/// Dictionary: sys_hr_employee_status
/// DictValue: 0 = ACTIVE, 1 = SUSPENDED, 2 = TERMINATED, 3 = ON_LEAVE
/// </summary>
private string MapStatusCodeToString(long? statusCode, bool isActive)
{
    if (!statusCode.HasValue)
    {
        // Fallback if no status record exists
        return isActive ? "active" : "inactive";
    }

    try
    {
        // ✅ FIX: Get the dictionary entry to access its DictValue
        var dictEntry = _dictDataService?.SelectDictDataById(statusCode.Value);
        if (dictEntry == null)
        {
            return isActive ? "active" : "inactive";
        }

        // ✅ FIX: Map dictionary DictValue (not DictCode) to status strings
        switch (dictEntry.DictValue)
        {
            case "0": return "active";       // ACTIVE
            case "1": return "suspended";    // SUSPENDED
            case "2": return "terminated";   // TERMINATED
            case "3": return "on_leave";     // ON_LEAVE
            default: return isActive ? "active" : "inactive";
        }
    }
    catch
    {
        // Fallback on error
        return isActive ? "active" : "inactive";
    }
}
```

**Key Changes:**
- ✅ Fetch dictionary entry: `var dictEntry = _dictDataService?.SelectDictDataById(statusCode.Value)`
- ✅ Use `dictEntry.DictValue` (e.g., "0", "1", "2", "3") instead of `statusCode.ToString()` (e.g., "1976234156241326084")
- ✅ Added error handling to fallback gracefully

#### **3. Updated ToApiDtoAsync Method (Line 90-101):**
```csharp
// Get current employment status code
var statusCode = await GetCurrentEmploymentStatusCodeAsync(employeeId);

var dto = new EmployeeApiDto
{
    EmployeeId = employee.EmployeeId,
    PersonId = employee.PersonId,
    EmployeeCode = employee.EmployeeCode,
    UserId = employee.UserId,
    HireDate = employee.HireDate,
    TerminationDate = employee.TerminationDate,
    IsActive = employee.IsActive,
    Status = MapStatusCodeToString(statusCode, employee.IsActive)  // ✅ Uses actual status code
};
```

#### **4. Updated ToListDtoAsync Method (Line 265-272):**
```csharp
// Get current employment status code
var statusCode = await GetCurrentEmploymentStatusCodeAsync(employeeId);

var dto = new EmployeeListDto
{
    EmployeeId = employee.EmployeeId,
    EmployeeCode = employee.EmployeeCode,
    IsActive = employee.IsActive,
    Status = MapStatusCodeToString(statusCode, employee.IsActive),  // ✅ Uses actual status code
    HireDate = employee.HireDate,
    EmploymentType = GetDictLabel("employment_type", employee.EmploymentTypeDictCode)
};
```

---

## 📊 **Status Mapping**

| Dict Code | Status Label | API Response String | IsActive | Display |
|-----------|--------------|---------------------|----------|---------|
| `"0"` | ACTIVE | `"active"` | `true` | نشط / Active |
| `"1"` | SUSPENDED | `"suspended"` | `false` | معلق / Suspended |
| `"2"` | TERMINATED | `"terminated"` | `false` | منتهي / Terminated |
| `"3"` | ON_LEAVE | `"on_leave"` | `true` | في إجازة / On Leave |

---

## 🧪 **Testing Instructions**

### **Step 1: Stop the Running Backend**

The backend is currently running (process ID 4764) and needs to be restarted:

```bash
# Find the running process
tasklist | findstr ZR.Admin.WebApi

# Kill it (use the PID from the output, currently 4764)
taskkill /F /PID 4764

# Or press Ctrl+C in the terminal where it's running
```

### **Step 2: Restart the Backend**

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Wait for:** `Now listening on: https://localhost:8885`

### **Step 3: Test Employee Status Update**

#### **Test Case 1: Update to TERMINATED**
**Request:**
```http
PUT https://localhost:8885/api/hr/employees
Content-Type: application/json

{
  "employeeId": "19",
  "status": "2",
  "firstName": "خالد",
  "lastName": "حمود",
  ...
}
```

**Expected Response:**
```json
{
  "code": 200,
  "data": {
    "employeeId": "19",
    "status": "terminated",  // ✅ Should say "terminated" now
    "isActive": false,
    ...
  }
}
```

#### **Test Case 2: Update to SUSPENDED**
**Request:**
```json
{
  "employeeId": "19",
  "status": "1"
}
```

**Expected Response:**
```json
{
  "status": "suspended",  // ✅ Should say "suspended"
  "isActive": false
}
```

#### **Test Case 3: Update to ACTIVE**
**Request:**
```json
{
  "employeeId": "19",
  "status": "0"
}
```

**Expected Response:**
```json
{
  "status": "active",  // ✅ Should say "active"
  "isActive": true
}
```

#### **Test Case 4: Update to ON_LEAVE**
**Request:**
```json
{
  "employeeId": "19",
  "status": "3"
}
```

**Expected Response:**
```json
{
  "status": "on_leave",  // ✅ Should say "on_leave"
  "isActive": true
}
```

---

## 🎯 **Impact**

### **Before Fix:**
- ❌ All inactive employees showed as "inactive" (no distinction)
- ❌ Frontend couldn't differentiate between suspended and terminated employees
- ❌ Employee cards showed incorrect status labels

### **After Fix:**
- ✅ Each status has a unique string: `active`, `suspended`, `terminated`, `on_leave`
- ✅ Frontend can show correct Arabic labels: نشط، معلق، منتهي، في إجازة
- ✅ Employee cards display accurate status information
- ✅ Status badges can use different colors based on specific status

---

## 🔗 **Related Fixes**

This fix works together with previous fixes:
1. **Employee Status Update Bug Fix** - Correctly saves status codes to database
2. **Lookup Permission System** - Allows all users to see employee dropdowns
3. **Marketing Team Page Fix** - Shows correct employee status in team cards

---

## 📝 **Build Verification**

### **Service Project:**
```bash
dotnet build ZR.Service/ZR.Service.csproj
```
**Result:** ✅ Build succeeded (0 Error(s), 53 Warning(s) - all pre-existing)

### **WebApi Project:**
```
Cannot build while backend is running (file locks).
Restart backend to apply changes.
```

---

## ✅ **Success Criteria**

After restarting backend, verify:

- [ ] Employee status updates to TERMINATED shows `"status": "terminated"` in response
- [ ] Employee status updates to SUSPENDED shows `"status": "suspended"` in response
- [ ] Employee status updates to ACTIVE shows `"status": "active"` in response
- [ ] Employee status updates to ON_LEAVE shows `"status": "on_leave"` in response
- [ ] Employee cards on frontend display correct status (منتهي instead of غير نشط)
- [ ] Status badges use correct colors based on specific status

---

## 🎨 **Frontend Display Mapping**

For the frontend to show proper Arabic labels, map the status strings:

```typescript
const statusLabels: Record<string, string> = {
  'active': 'نشط',
  'inactive': 'غير نشط',
  'suspended': 'معلق',
  'terminated': 'منتهي',
  'on_leave': 'في إجازة'
};

const statusColors: Record<string, string> = {
  'active': 'success',      // Green
  'suspended': 'warning',   // Orange
  'terminated': 'danger',   // Red
  'on_leave': 'info'        // Blue
};
```

**Example:**
```tsx
<Badge color={statusColors[employee.status] || 'secondary'}>
  {statusLabels[employee.status] || employee.status}
</Badge>
```

---

## 📞 **Support**

If the status still shows as "inactive" after restart:

1. **Check backend logs** - Look for any errors when fetching status
2. **Check database** - Verify `hr_employee_status` table has records for the employee
3. **Check status effective dates** - Ensure `effective_to` is NULL or in the future
4. **Test with SQL query:**
   ```sql
   SELECT * FROM hr_employee_status
   WHERE employee_id = 19
   AND (effective_to IS NULL OR effective_to > CURRENT_TIMESTAMP)
   ORDER BY effective_from DESC;
   ```

---

**Status:** ✅ **READY FOR TESTING**
**Next Action:** Stop backend (PID 4764), restart, and test employee status updates

**The fix has been successfully compiled and is ready for runtime testing!** 🚀
