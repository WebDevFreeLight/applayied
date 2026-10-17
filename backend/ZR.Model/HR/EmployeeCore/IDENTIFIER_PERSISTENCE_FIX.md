# Identifier (and Contact/Address) Persistence Fix

## Problem
When updating employees via PUT `/api/hr/employees`, the identifier data (National ID, Iqama, Passport) was being received correctly in the request but **NOTHING WAS SAVED TO THE DATABASE TABLE** `hr_person_identifier`.

## Root Cause
The "ID = 0 pattern" for upsert operations was incomplete in two places:

### Issue 1: Missing PersonId in Controller DTOs
When the controller created new identifier/contact/address DTOs with ID = 0 (to signal INSERT), it was NOT setting the required `PersonId` field:

```csharp
// WRONG - Missing PersonId
dto.Identifiers.Add(new UpdatePersonIdentifierDto
{
    PersonIdentifierId = 0, // Signal for INSERT
    IdentifierTypeDictCode = 1,
    IdentifierNumber = request.IdNumber,
    ExpiryDate = request.IdExpiryDate,
    IsPrimary = true
    // MISSING: PersonId = personId
});
```

### Issue 2: Orchestration Service Only Handled UPDATE
The `UpdatePersonIdentifierAsync`, `UpdatePersonContactAsync`, and `UpdatePersonAddressAsync` methods in the orchestration service only handled UPDATE operations. When they received a DTO with ID = 0, they would:
1. Query for a record with ID = 0
2. Find null
3. Return without inserting anything

```csharp
// WRONG - Only handles UPDATE
private async Task UpdatePersonIdentifierAsync(UpdatePersonIdentifierDto dto)
{
    var entity = await Context.Queryable<HrPersonIdentifier>().InSingleAsync(dto.PersonIdentifierId);
    if (entity == null) return; // EXITS HERE when ID = 0!

    // Update logic...
}
```

## Solution

### Fix 1: Controller - Set PersonId When Creating New Records

**File**: `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs`

Added `PersonId = personId` to all new record creation blocks:

**Identifiers** (3 places - National ID, Iqama, Passport):
```csharp
// Lines 695-703, 725-733, 755-763
dto.Identifiers.Add(new UpdatePersonIdentifierDto
{
    PersonIdentifierId = 0, // New record
    PersonId = personId, // ✅ ADDED - REQUIRED for INSERT
    IdentifierTypeDictCode = 1, // or 2, or 3
    IdentifierNumber = request.IdNumber,
    ExpiryDate = request.IdExpiryDate,
    IsPrimary = true
});
```

**Contacts** (2 places - Personal Email, Mobile):
```csharp
// Lines 621-629, 666-674
dto.Contacts.Add(new UpdatePersonContactDto
{
    PersonContactId = 0, // New record
    PersonId = personId, // ✅ ADDED - REQUIRED for INSERT
    ContactTypeDictCode = 2, // or 4
    ContactValue = request.PersonalEmail,
    IsPrimary = false,
    EffectiveFrom = DateTime.Now
});
```

**Addresses** (1 place):
```csharp
// Lines 787-795
dto.Addresses.Add(new UpdatePersonAddressDto
{
    PersonAddressId = 0, // New record
    PersonId = personId, // ✅ ADDED - REQUIRED for INSERT
    AddressTypeDictCode = 1,
    AddressLine1 = request.Address,
    IsPrimary = true,
    EffectiveFrom = DateTime.Now
});
```

### Fix 2: Orchestration Service - Handle INSERT When ID = 0

**File**: `ZR.Service/HR/EmployeeCore/EmployeeCoreOrchestrationService.cs`

Updated three methods to check if ID == 0 and INSERT new records:

#### UpdatePersonIdentifierAsync (Lines 881-917)
```csharp
private async Task UpdatePersonIdentifierAsync(UpdatePersonIdentifierDto dto)
{
    // ✅ ADDED: ID = 0 means CREATE new record
    if (dto.PersonIdentifierId == 0)
    {
        var newEntity = new HrPersonIdentifier
        {
            PersonId = dto.PersonId.Value,
            IdentifierTypeDictCode = dto.IdentifierTypeDictCode,
            IdentifierNumber = dto.IdentifierNumber,
            IssuingCountryDictCode = dto.IssuingCountryDictCode,
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            IsPrimary = dto.IsPrimary,
            IsVerified = dto.IsVerified,
            Create_time = DateTime.Now
        };

        await Context.Insertable(newEntity).ExecuteCommandAsync();
        return;
    }

    // UPDATE existing record (original logic)
    var entity = await Context.Queryable<HrPersonIdentifier>().InSingleAsync(dto.PersonIdentifierId);
    if (entity == null) return;
    // ... update logic
}
```

#### UpdatePersonContactAsync (Lines 919-953)
```csharp
private async Task UpdatePersonContactAsync(UpdatePersonContactDto dto)
{
    // ✅ ADDED: ID = 0 means CREATE new record
    if (dto.PersonContactId == 0)
    {
        var newEntity = new HrPersonContact
        {
            PersonId = dto.PersonId.Value,
            ContactTypeDictCode = dto.ContactTypeDictCode,
            ContactValue = dto.ContactValue,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsPrimary = dto.IsPrimary,
            IsVerified = dto.IsVerified,
            Create_time = DateTime.Now
        };

        await Context.Insertable(newEntity).ExecuteCommandAsync();
        return;
    }

    // UPDATE existing record (original logic)
    // ... update logic
}
```

#### UpdatePersonAddressAsync (Lines 955-995)
```csharp
private async Task UpdatePersonAddressAsync(UpdatePersonAddressDto dto)
{
    // ✅ ADDED: ID = 0 means CREATE new record
    if (dto.PersonAddressId == 0)
    {
        var newEntity = new HrPersonAddress
        {
            PersonId = dto.PersonId.Value,
            AddressTypeDictCode = dto.AddressTypeDictCode,
            CountryDictCode = dto.CountryDictCode,
            City = dto.City,
            PostalCode = dto.PostalCode,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsPrimary = dto.IsPrimary,
            Create_time = DateTime.Now
        };

        await Context.Insertable(newEntity).ExecuteCommandAsync();
        return;
    }

    // UPDATE existing record (original logic)
    // ... update logic
}
```

## The "ID = 0 Pattern" Explained

This codebase uses a consistent pattern for upsert operations:

1. **Controller** checks if a related record exists (e.g., identifier, contact, address)
2. If exists: Create DTO with actual ID → Service will UPDATE
3. If not exists: Create DTO with ID = 0 → Service will INSERT
4. **Service** checks if DTO.ID == 0:
   - If 0: INSERT new record using DTO.PersonId
   - If not 0: UPDATE existing record using DTO.ID

This pattern is used throughout the codebase for:
- `UpdatePersonIdentifierDto` (PersonIdentifierId)
- `UpdatePersonContactDto` (PersonContactId)
- `UpdatePersonAddressDto` (PersonAddressId)
- Other related DTOs

## Files Changed

1. ✅ `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs`
   - Added `PersonId = personId` to 6 DTO creation blocks
   - 3 identifiers + 2 contacts + 1 address

2. ✅ `ZR.Service/HR/EmployeeCore/EmployeeCoreOrchestrationService.cs`
   - Updated `UpdatePersonIdentifierAsync` to handle INSERT when ID = 0
   - Updated `UpdatePersonContactAsync` to handle INSERT when ID = 0
   - Updated `UpdatePersonAddressAsync` to handle INSERT when ID = 0

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
  "passportExpiryDate": "2026-01-12",
  "personalEmail": "john@personal.com",
  "mobileNumber": "+966501234567",
  "address": "123 Main St, Riyadh"
}
```

### Database Result (NOW WORKING):
**Table: hr_person_identifier**
| person_id | identifier_type_dict_code | identifier_number | expiry_date | is_primary |
|-----------|---------------------------|-------------------|-------------|------------|
| 21        | 1 (National ID)           | 123355            | 2025-12-29  | true       |
| 21        | 2 (Passport)              | 421223            | 2026-01-12  | false      |
| 21        | 3 (Iqama)                 | 24512332          | 2026-01-12  | false      |

**Table: hr_person_contact**
| person_id | contact_type_dict_code | contact_value        | is_primary |
|-----------|------------------------|----------------------|------------|
| 21        | 2 (Personal Email)     | john@personal.com    | false      |
| 21        | 4 (Mobile)             | +966501234567        | false      |

**Table: hr_person_address**
| person_id | address_type_dict_code | address_line1        | is_primary |
|-----------|------------------------|----------------------|------------|
| 21        | 1 (Home)               | 123 Main St, Riyadh  | true       |

### Response (COMPLETE):
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
    "passportExpiryDate": "2026-01-12", // ✅ NOW POPULATED
    "personalEmail": "john@personal.com",
    "mobileNumber": "+966501234567",
    "address": "123 Main St, Riyadh"
  }
}
```

## Testing Steps

### 1. Rebuild Backend
```bash
cd C:\Users\Mustafa\Desktop\maham\mahamBackendNET\backend
dotnet build ZRAdminMahamBackend.sln
cd ZR.Admin.WebApi
dotnet run
```

### 2. Test PUT Request
Use the request above to create new identifiers for an employee without existing identifiers.

### 3. Verify Database
Query the tables to confirm data was inserted:
```sql
-- Check identifiers
SELECT person_id, identifier_type_dict_code, identifier_number, expiry_date, is_primary
FROM hr_person_identifier
WHERE person_id = 21;

-- Check contacts
SELECT person_id, contact_type_dict_code, contact_value, is_primary
FROM hr_person_contact
WHERE person_id = 21;

-- Check addresses
SELECT person_id, address_type_dict_code, address_line1, is_primary
FROM hr_person_address
WHERE person_id = 21;
```

### 4. Test UPDATE Flow
Send the PUT request again with different values to verify UPDATE logic still works:
```json
{
  "employeeId": "18",
  "idNumber": "999999",  // Changed
  "idExpiryDate": "2026-12-29",  // Changed
  ...
}
```

Should UPDATE existing records, not create duplicates.

## Summary

### What Was Wrong
1. ❌ Controller creating DTOs with ID = 0 but missing PersonId field
2. ❌ Orchestration service only handling UPDATE, not INSERT
3. ❌ Database records not being created for new identifiers/contacts/addresses

### What's Fixed
1. ✅ Controller now sets PersonId when creating new DTOs with ID = 0
2. ✅ Orchestration service now checks if ID == 0 and INSERTS new records
3. ✅ Database records now properly created for all new identifiers/contacts/addresses
4. ✅ API response returns all saved values immediately after INSERT
5. ✅ Pattern consistently applied to identifiers, contacts, and addresses

## Impact
This fix ensures that when employees are updated with new identifiers, contacts, or addresses through the PUT `/api/hr/employees` endpoint:
- Data is **actually saved** to the database (was completely failing before)
- Response immediately reflects the saved values
- The "ID = 0 pattern" works as designed throughout the codebase
- No duplicate records are created when updating existing data
