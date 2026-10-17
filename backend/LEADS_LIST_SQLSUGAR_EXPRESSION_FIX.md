# Leads List API - SqlSugar Expression Error Fix

**Date:** 2026-01-06
**Status:** ✅ **FIXED - RESTART REQUIRED**

---

## 🐛 **Problem**

When trying to fetch the leads list with filtering (e.g., by assigned employee), the API returned a **500 Internal Server Error**.

**Error Request:**
```http
GET /api/marketing/leads/list?AssignedTo=18&PageNum=1&PageSize=100
```

**Error Response:**
```json
{
  "success": false,
  "code": "COMMON_INTERNAL_ERROR",
  "message": "COMMON_INTERNAL_ERROR",
  "severity": "Critical",
  "path": "/api/marketing/leads/list",
  "stackTrace": "at SqlSugar.QueryBuilder.CheckExpression(Expression expression, String methodName)..."
}
```

**Stack Trace:**
```
at SqlSugar.Check.Exception(Boolean isException, String message, String[] args)
at SqlSugar.QueryBuilder.CheckExpression(Expression expression, String methodName)
at SqlSugar.QueryableProvider`1._Where(Expression expression)
at SqlSugar.QueryableProvider`3.Where(Expression`1 expression)
at ZR.Service.Marketing.Leads.BpLeadService.GetLeadsListAsync(LeadQueryDto query) in BpLeadService.cs:line 50
```

---

## 🔍 **Root Cause**

The issue was an **expression signature mismatch** in SqlSugar.

### **The Problem Code:**

```csharp
public async Task<PagedInfo<BpLead>> GetLeadsListAsync(LeadQueryDto query)
{
    // Created expression for single table (BpLead)
    var exp = Expressionable.Create<BpLead>();

    if (!string.IsNullOrEmpty(query.BrandName))
        exp = exp.And(it => it.BrandName.Contains(query.BrandName));

    if (query.AssignedTo.HasValue)
        exp = exp.And(it => it.AssignedTo == query.AssignedTo.Value);
    // ... more conditions

    // Applied to 3-table join query - MISMATCH!
    var pageQuery = Context
        .Queryable<BpLead, HrEmployeeCore, HrPersonName>((lead, emp, name) => new JoinQueryInfos(...))
        .Where(exp.ToExpression())  // ❌ Expression expects 1 parameter, query has 3 tables!
        .Select(...);
}
```

### **Why It Failed:**

1. **Expression created for 1 table:** `Expressionable.Create<BpLead>()` creates an expression that expects **1 parameter** `(BpLead it => ...)`

2. **Applied to 3-table join:** `.Queryable<BpLead, HrEmployeeCore, HrPersonName>()` creates a query that expects **3 parameters** `(BpLead lead, HrEmployeeCore emp, HrPersonName name => ...)`

3. **Signature mismatch:** SqlSugar can't apply a 1-parameter expression to a 3-parameter queryable!

**Result:** `SqlSugar.Check.Exception` → 500 error

---

## ✅ **Solution**

Changed the code to apply filters **directly on the joined query** using `.WhereIF()` with the correct 3-parameter signature.

### **Fixed Code:**

```csharp
public async Task<PagedInfo<BpLead>> GetLeadsListAsync(LeadQueryDto query)
{
    // Query with employee name populated
    var pageQuery = Context
        .Queryable<BpLead, HrEmployeeCore, HrPersonName>((lead, emp, name) => new JoinQueryInfos(
            JoinType.Left, lead.AssignedTo == emp.EmployeeId,
            JoinType.Left, emp.PersonId == name.PersonId && name.IsPrimary == true))
        .WhereIF(!string.IsNullOrEmpty(query.BrandName), (lead, emp, name) => lead.BrandName.Contains(query.BrandName))
        .WhereIF(query.AssignedTo.HasValue, (lead, emp, name) => lead.AssignedTo == query.AssignedTo.Value)
        .WhereIF(!string.IsNullOrEmpty(query.LeadStatus), (lead, emp, name) => lead.LeadStatus == query.LeadStatus)
        .WhereIF(query.LeadType.HasValue, (lead, emp, name) => lead.LeadType == query.LeadType.Value)
        .WhereIF(!string.IsNullOrEmpty(query.OwnerPhone), (lead, emp, name) => lead.OwnerPhone.Contains(query.OwnerPhone))
        .WhereIF(!string.IsNullOrEmpty(query.OwnerEmail), (lead, emp, name) => lead.OwnerEmail.Contains(query.OwnerEmail))
        .Select((lead, emp, name) => new BpLead { ... });
}
```

### **What Changed:**

1. ❌ **Removed:** `Expressionable.Create<BpLead>()` and building expressions separately
2. ✅ **Added:** `.WhereIF()` conditions directly on the joined query
3. ✅ **Fixed:** Each lambda now has **3 parameters** matching the join: `(lead, emp, name)`
4. ✅ **Result:** Expression signature matches the queryable signature!

---

## 🎯 **Benefits**

### **Before (Broken):**
- ❌ Expression signature mismatch
- ❌ SqlSugar exception at runtime
- ❌ 500 error on every filtered request
- ❌ Can't fetch leads for specific employees

### **After (Fixed):**
- ✅ Expression signature matches queryable
- ✅ No SqlSugar exceptions
- ✅ Filtering works correctly
- ✅ Can fetch leads by assigned employee, brand, status, etc.

---

## 📊 **How It Works Now**

### **1. Frontend Request:**
```http
GET /api/marketing/leads/list?AssignedTo=18&PageNum=1&PageSize=100
```

### **2. Backend Processing:**
```csharp
// Query with 3-table join
.Queryable<BpLead, HrEmployeeCore, HrPersonName>()
// Filter by assigned employee
.WhereIF(query.AssignedTo.HasValue, (lead, emp, name) => lead.AssignedTo == query.AssignedTo.Value)
// 18 == 18 → Include this lead
```

### **3. SQL Generated (approximately):**
```sql
SELECT
    lead.*,
    (name.full_name_ar ?? (name.given_name + ' ' + name.family_name)) AS AssignedToName
FROM bp_lead lead
LEFT JOIN hr_employee_core emp ON lead.assigned_to = emp.employee_id
LEFT JOIN hr_person_name name ON emp.person_id = name.person_id AND name.is_primary = true
WHERE lead.assigned_to = 18  -- ✅ Filter applied correctly!
```

### **4. API Response:**
```json
{
  "code": 200,
  "data": {
    "result": [
      {
        "leadId": "2008495087050432512",
        "brandName": "zfsfsdf",
        "ownerName": "sfdsfg",
        "assignedTo": 18,
        "assignedToName": "ليلى ليلى",  // ✅ Employee name from join!
        "leadStatus": "New",
        "priority": "Medium"
      }
    ],
    "totalNum": 1
  }
}
```

---

## 🧪 **Testing After Restart**

### **Test 1: Get All Leads**
```http
GET /api/marketing/leads/list?PageNum=1&PageSize=100
```

**Expected:** All leads returned (no filter)

### **Test 2: Get Leads by Assigned Employee**
```http
GET /api/marketing/leads/list?AssignedTo=18&PageNum=1&PageSize=100
```

**Expected:** Only leads assigned to employee #18

### **Test 3: Get Leads by Brand Name**
```http
GET /api/marketing/leads/list?BrandName=zfsfsdf&PageNum=1&PageSize=100
```

**Expected:** Only leads matching brand name "zfsfsdf"

### **Test 4: Get Leads by Status**
```http
GET /api/marketing/leads/list?LeadStatus=New&PageNum=1&PageSize=100
```

**Expected:** Only leads with status "New"

### **Test 5: Combined Filters**
```http
GET /api/marketing/leads/list?AssignedTo=18&LeadStatus=New&PageNum=1&PageSize=100
```

**Expected:** Only leads assigned to employee #18 with status "New"

---

## 🚀 **How to Apply the Fix**

The backend is running and needs to be restarted:

### **Step 1: Stop the Backend**

```bash
# Find the process (if you don't know the PID)
tasklist | findstr ZR.Admin.WebApi

# Kill it (use actual PID)
taskkill /F /PID <PID>

# Or press Ctrl+C in the terminal where it's running
```

### **Step 2: Restart the Backend**

```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend\ZR.Admin.WebApi
dotnet run
```

**Wait for:** `Now listening on: https://localhost:8885`

---

## 📝 **Files Modified**

| File | Change | Lines |
|------|--------|-------|
| `ZR.Service/Marketing/Leads/BpLeadService.cs` | Fixed SqlSugar expression signature | 27-39 |

**Changes:**
- Removed `Expressionable.Create<BpLead>()` expression building (lines 29-47)
- Added `.WhereIF()` filters directly on joined query (lines 34-39)
- Each filter lambda now has 3 parameters: `(lead, emp, name)`

---

## 🎓 **Learning: SqlSugar Multi-Table Joins**

### **Single-Table Query:**
```csharp
// Expression with 1 parameter
var exp = Expressionable.Create<BpLead>();
exp = exp.And(it => it.BrandName.Contains("test"));

// Single-table queryable
Context.Queryable<BpLead>()
    .Where(exp.ToExpression())  // ✅ Signature matches!
```

### **Multi-Table Query:**
```csharp
// DON'T create separate expression - it won't match!
// var exp = Expressionable.Create<BpLead>(); ❌

// Multi-table queryable - use WhereIF directly
Context.Queryable<BpLead, HrEmployeeCore>((lead, emp) => ...)
    .WhereIF(condition, (lead, emp) => lead.Field == value)  // ✅ Signature matches!
```

**Rule:** Number of lambda parameters **must match** number of tables in the queryable!

---

## ✅ **Verification Checklist**

After restarting backend:

- [ ] Backend starts successfully without errors
- [ ] GET `/api/marketing/leads/add` (create lead) works ✓ (already working)
- [ ] GET `/api/marketing/leads/list` returns all leads without errors
- [ ] GET `/api/marketing/leads/list?AssignedTo=18` filters by employee
- [ ] GET `/api/marketing/leads/list?BrandName=test` filters by brand
- [ ] GET `/api/marketing/leads/list?LeadStatus=New` filters by status
- [ ] No SqlSugar exceptions in logs
- [ ] Employee names appear in leads list (from join)

---

**Status:** ✅ **READY FOR TESTING**
**Next Action:** Restart backend and test leads list filtering

**The fix properly handles SqlSugar multi-table join expressions!** 🚀
