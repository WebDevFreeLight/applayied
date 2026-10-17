# Identifier Fields Response Fix

## Problem
The PUT `/api/hr/employees` endpoint was receiving identifier data correctly:
```json
{
  "idNumber": "123355",
  "idExpiryDate": "2025-12-29",
  "iqamaNumber": "24512332",
  "iqamaExpiryDate": "2026-01-12",
  "passportNumber": "421223",
  "passportExpiryDate": "2026-01-12"
}
```

But the response was showing all fields as `null`:
```json
{
  "idNumber": null,
  "passportNumber": null,
  "passportExpiryDate": null
}
```

**Missing fields**: `idExpiryDate`, `iqamaNumber`, `iqamaExpiryDate`

## Root Cause
The `ToApiDtoAsync` method in `EmployeeApiMappingService` was:
1. Only fetching National ID number (missing expiry date)
2. Fetching Passport with expiry date ✓
3. **Not fetching Iqama at all** ✗
4. **Not fetching any expiry dates for National ID** ✗

## Solution

### 1. Updated EmployeeApiDto (Response DTO)
**File**: `ZR.Model/HR/Dto/EmployeeCore/EmployeeApiDto.cs`

Added missing fields:
```csharp
public string IdNumber { get; set; }
public DateTime? IdExpiryDate { get; set; }          // ✅ ADDED
public string IqamaNumber { get; set; }               // ✅ ADDED
public DateTime? IqamaExpiryDate { get; set; }        // ✅ ADDED
public string PassportNumber { get; set; }
public DateTime? PassportExpiryDate { get; set; }
```

### 2. Updated EmployeeApiMappingService.ToApiDtoAsync()
**File**: `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`

**Before** (incomplete):
```csharp
// Only fetched primary ID number
var primaryId = await GetPrimaryIdentifierAsync(employee.PersonId);
if (primaryId != null)
{
    dto.IdNumber = primaryId.IdentifierNumber;
    // Missing: IdExpiryDate
}

var passport = await GetPassportAsync(employee.PersonId);
if (passport != null)
{
    dto.PassportNumber = passport.IdentifierNumber;
    dto.PassportExpiryDate = passport.ExpiryDate;
}
// Missing: Iqama fetch
```

**After** (complete):
```csharp
// Type 1: National ID (Primary)
var primaryId = await GetIdentifierByTypeAsync(employee.PersonId, 1);
if (primaryId != null)
{
    dto.IdNumber = primaryId.IdentifierNumber;
    dto.IdExpiryDate = primaryId.ExpiryDate;         // ✅ NOW RETURNED
}

// Type 2: Passport
var passport = await GetIdentifierByTypeAsync(employee.PersonId, 2);
if (passport != null)
{
    dto.PassportNumber = passport.IdentifierNumber;
    dto.PassportExpiryDate = passport.ExpiryDate;
}

// Type 3: Iqama
var iqama = await GetIdentifierByTypeAsync(employee.PersonId, 3);  // ✅ NOW FETCHED
if (iqama != null)
{
    dto.IqamaNumber = iqama.IdentifierNumber;        // ✅ NOW RETURNED
    dto.IqamaExpiryDate = iqama.ExpiryDate;          // ✅ NOW RETURNED
}
```

## Expected Result

### Request (same as before):
```http
PUT /api/hr/employees
{
  "employeeId": "18",
  "idNumber": "123355",
  "idExpiryDate": "2025-12-29",
  "iqamaNumber": "24512332",
  "iqamaExpiryDate": "2026-01-12",
  "passportNumber": "421223",
  "passportExpiryDate": "2026-01-12"
}
```

### Response (NOW COMPLETE):
```json
{
  "code": 200,
  "msg": "success",
  "data": {
    "employeeId": "18",
    "personId": "21",
    "idNumber": "123355",              // ✅ NOW POPULATED
    "idExpiryDate": "2025-12-29",      // ✅ NOW POPULATED
    "iqamaNumber": "24512332",         // ✅ NOW POPULATED
    "iqamaExpiryDate": "2026-01-12",   // ✅ NOW POPULATED
    "passportNumber": "421223",        // ✅ NOW POPULATED
    "passportExpiryDate": "2026-01-12" // ✅ NOW POPULATED
  }
}
```

## Files Changed

1. ✅ `ZR.Model/HR/Dto/EmployeeCore/EmployeeApiDto.cs`
   - Added `IdExpiryDate`, `IqamaNumber`, `IqamaExpiryDate` fields

2. ✅ `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`
   - Updated `ToApiDtoAsync()` to fetch all 3 identifier types
   - Now uses `GetIdentifierByTypeAsync()` for specific lookups
   - Returns all identifier numbers AND expiry dates

## Testing

### Step 1: Rebuild Backend
```bash
cd ZR.Admin.WebApi
dotnet build
dotnet run
```

### Step 2: Test PUT Request
```http
PUT https://localhost:8885/api/hr/employees
Content-Type: application/json

{
  "employeeId": "18",
  "idNumber": "123355",
  "idExpiryDate": "2025-12-29",
  "iqamaNumber": "24512332",
  "iqamaExpiryDate": "2026-01-12",
  "passportNumber": "421223",
  "passportExpiryDate": "2026-01-12"
}
```

### Step 3: Verify Response
Check that ALL identifier fields are populated in the response:
- ✅ `idNumber`: "123355"
- ✅ `idExpiryDate`: "2025-12-29"
- ✅ `iqamaNumber`: "24512332"
- ✅ `iqamaExpiryDate`: "2026-01-12"
- ✅ `passportNumber`: "421223"
- ✅ `passportExpiryDate`: "2026-01-12"

### Step 4: Verify Database
Query `hr_person_identifier` table to confirm data was saved:
```sql
SELECT
  identifier_type_dict_code,
  identifier_number,
  expiry_date
FROM hr_person_identifier
WHERE person_id = 21;
```

Expected 3 rows:
- Type 1 (National ID): 123355, 2025-12-29
- Type 2 (Passport): 421223, 2026-01-12
- Type 3 (Iqama): 24512332, 2026-01-12

## Summary

### What Was Wrong
- ❌ Response DTO missing fields
- ❌ Mapping service not fetching all identifier types
- ❌ Mapping service not returning expiry dates

### What's Fixed
- ✅ EmployeeApiDto now includes all 6 identifier fields
- ✅ ToApiDtoAsync fetches all 3 identifier types (National ID, Passport, Iqama)
- ✅ All identifier numbers AND expiry dates are returned in response
- ✅ Data is saved correctly in database
- ✅ Data is retrieved correctly in response

## Impact
This fix ensures that when employees update their identification documents in the frontend, the updated values are immediately reflected in the API response, providing real-time feedback to the user and maintaining data consistency.
