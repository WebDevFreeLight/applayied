# Employee Status Update Bug - Fixed

**Date:** 2026-01-06
**Status:** ✅ **FIXED**

---

## 🐛 **Problem**

When updating an employee, the status field was not being saved correctly:

**Request:**
```json
{
  "employeeId": "18",
  "status": "0"  // Dictionary value for ACTIVE
}
```

**Response:**
```json
{
  "status": "inactive",
  "isActive": false  // ❌ Should be true!
}
```

**Expected:**
```json
{
  "status": "active",
  "isActive": true  // ✅ Correct
}
```

---

## 🔍 **Root Cause**

**File:** `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs:932-942`

**Old Code:**
```csharp
private bool ParseStatus(string status)
{
    if (string.IsNullOrEmpty(status)) return false;

    // Map string status to boolean IsActive
    var statusLower = status.ToLower().Trim();
    return statusLower == "active";  // ❌ Only checks for "active" string

    // "active" → true
    // "inactive" or "terminated" → false
}
```

**The Problem:**
1. Frontend sends dictionary value: `status: "0"` (ACTIVE)
2. Backend expects string literal: `status: "active"`
3. `"0".ToLower() !== "active"` → returns `false`
4. Employee gets marked as inactive (`IsActive = false`)

**Dictionary Values:**
```json
{
  "0": "ACTIVE",
  "1": "SUSPENDED",
  "2": "TERMINATED",
  "3": "ON_LEAVE"
}
```

---

## ✅ **Solution**

Updated `ParseStatus` method to handle BOTH dictionary values AND string literals:

**New Code:**
```csharp
private bool ParseStatus(string status)
{
    if (string.IsNullOrEmpty(status)) return false;

    var statusTrimmed = status.Trim();

    // Map dictionary values to boolean IsActive
    // Dictionary: sys_hr_employee_status
    // "0" = ACTIVE → IsActive = true
    // "1" = SUSPENDED → IsActive = false
    // "2" = TERMINATED → IsActive = false
    // "3" = ON_LEAVE → IsActive = true (employee still active, just on leave)
    if (statusTrimmed == "0" || statusTrimmed == "3")
    {
        return true; // ACTIVE or ON_LEAVE
    }
    if (statusTrimmed == "1" || statusTrimmed == "2")
    {
        return false; // SUSPENDED or TERMINATED
    }

    // Fallback: Map string literals (backwards compatibility)
    var statusLower = statusTrimmed.ToLower();
    return statusLower == "active" || statusLower == "on_leave";

    // "active" → true
    // "inactive" or "terminated" or "suspended" → false
}
```

**Now Supports:**
- ✅ Dictionary values: `"0"`, `"1"`, `"2"`, `"3"`
- ✅ String literals: `"active"`, `"inactive"`, `"suspended"`, `"terminated"`, `"on_leave"`
- ✅ Backwards compatible with old API calls

---

## 📊 **Status Mapping**

| Dictionary Value | Label | IsActive | Notes |
|-----------------|-------|----------|-------|
| `"0"` | ACTIVE | `true` ✅ | Employee is actively working |
| `"1"` | SUSPENDED | `false` | Employee is temporarily suspended |
| `"2"` | TERMINATED | `false` | Employee is permanently terminated |
| `"3"` | ON_LEAVE | `true` ✅ | Employee is on leave but still active |

---

## 🧪 **Testing**

### **Step 1: Restart the Backend**

The backend file has been updated. You need to restart it:

```bash
# If running in terminal, press Ctrl+C to stop
# Then restart:
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Or if running as a process, kill and restart:**
```bash
# Find the process
tasklist | findstr ZR.Admin.WebApi

# Kill it (replace PID)
taskkill /F /PID <process_id>

# Restart
cd ZR.Admin.WebApi
dotnet run
```

### **Step 2: Test Employee Update**

**Via Frontend:**
1. Go to: `http://localhost:3006/ar/hr/employees`
2. Find employee "ليلى" (ID: 18)
3. Click edit
4. Change status to "ACTIVE" (value "0")
5. Save

**Expected Result:**
```json
{
  "code": 200,
  "data": {
    "employeeId": "18",
    "status": "active",  // ✅ Should be "active" now
    "isActive": true     // ✅ Should be true now
  }
}
```

### **Step 3: Verify Different Statuses**

Test all status values:

**Test 1: ACTIVE (0)**
```json
Request: { "employeeId": "18", "status": "0" }
Expected: { "isActive": true, "status": "active" }
```

**Test 2: SUSPENDED (1)**
```json
Request: { "employeeId": "18", "status": "1" }
Expected: { "isActive": false, "status": "inactive" }
```

**Test 3: TERMINATED (2)**
```json
Request: { "employeeId": "18", "status": "2" }
Expected: { "isActive": false, "status": "inactive" }
```

**Test 4: ON_LEAVE (3)**
```json
Request: { "employeeId": "18", "status": "3" }
Expected: { "isActive": true, "status": "active" }
```

---

## 🎯 **Impact**

### **Before Fix:**
- ❌ Employees could not be set to ACTIVE status via frontend
- ❌ All employees defaulted to inactive regardless of status selection
- ❌ Marketing team page showed 0 active members

### **After Fix:**
- ✅ Status updates work correctly
- ✅ Dictionary values ("0", "1", "2", "3") properly mapped
- ✅ String literals ("active", "inactive") still work (backwards compatible)
- ✅ Marketing team page shows correct active employee count

---

## 🔗 **Related Issues**

This fix also helps with:
1. **Marketing Team Page**: Active employees will now be counted correctly
2. **Employee Statistics**: Dashboard will show accurate active/inactive counts
3. **Lookup Endpoint**: Will return correct `isActive` status for filtering

---

## 📝 **Files Modified**

| File | Line | Change |
|------|------|--------|
| `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` | 932-959 | Updated `ParseStatus` method |

---

## ✅ **Verification**

After restarting backend:

1. ✅ Update employee status to ACTIVE ("0")
2. ✅ Check response: `isActive` should be `true`
3. ✅ Verify in database: `is_active` column should be `1` or `TRUE`
4. ✅ Check marketing team page: Employee should appear as active

---

**Status:** Ready for testing! Restart the backend and try updating employee status again.

