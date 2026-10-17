# Employee Identifier Fields Update

## Problem
The frontend was sending identifier data (idNumber, idExpiryDate, iqamaNumber, iqamaExpiryDate, passportNumber, passportExpiryDate) but the `/api/hr/employees` PUT endpoint was not saving them. The response showed all these fields as null.

## Solution
Updated the employee update flow to properly handle three types of identifiers:

### 1. National ID (identifier_type_dict_code = 1)
- Fields: `idNumber`, `idExpiryDate`
- Primary identifier
- Stored in `hr_person_identifier` table

### 2. Iqama (identifier_type_dict_code = 3)
- Fields: `iqamaNumber`, `iqamaExpiryDate`
- Saudi residence permit
- Stored in `hr_person_identifier` table

### 3. Passport (identifier_type_dict_code = 2)
- Fields: `passportNumber`, `passportExpiryDate`
- International travel document
- Stored in `hr_person_identifier` table

## Files Modified

### 1. UpdateEmployeeRequest DTO
**File**: `ZR.Model/HR/Dto/EmployeeCore/EmployeeApiDto.cs`

Added fields:
```csharp
public string IdNumber { get; set; }
public DateTime? IdExpiryDate { get; set; }          // NEW
public string IqamaNumber { get; set; }               // NEW
public DateTime? IqamaExpiryDate { get; set; }        // NEW
public string PassportNumber { get; set; }
public DateTime? PassportExpiryDate { get; set; }
```

### 2. EmployeesController Mapping Logic
**File**: `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs`

Updated `MapToUpdateDtoAsync` method to handle all three identifier types:
- Uses `GetIdentifierByTypeAsync(personId, typeCode)` for specific lookups
- Properly sets expiry dates for each identifier type
- Creates new records if identifier doesn't exist (PersonIdentifierId = 0)
- Updates existing records if found

### 3. EmployeeApiMappingService
**File**: `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`

Added new helper method:
```csharp
public async Task<HrPersonIdentifier> GetIdentifierByTypeAsync(long personId, long identifierTypeCode)
{
    return await Context.Queryable<HrPersonIdentifier>()
        .Where(i => i.PersonId == personId && i.IdentifierTypeDictCode == identifierTypeCode)
        .FirstAsync();
}
```

Also added to `IEmployeeApiMappingService` interface.

### 4. Database Seed Script
**File**: `sql/seed_identifier_types.sql`

Created dictionary seed data for identifier types:
- Type 1: National ID
- Type 2: Passport
- Type 3: Iqama
- Type 4: Driver License
- Type 5: Social Security

## Testing

### Test Payload (before fix - fields were null in response):
```json
{
  "employeeId": "18",
  "idNumber": "13241",
  "idExpiryDate": "2026-01-05",
  "iqamaNumber": "423512",
  "iqamaExpiryDate": "2026-01-12",
  "passportNumber": "541223",
  "passportExpiryDate": "2026-01-19"
}
```

### Expected Result (after fix):
All identifier data should be saved to `hr_person_identifier` table:
- 3 records created/updated for person_id=21
- Each with correct identifier_type_dict_code (1, 2, 3)
- Each with correct expiry_date values

## Database Schema Reference

```sql
hr_person_identifier
├── person_identifier_id (PK)
├── person_id (FK)
├── identifier_type_dict_code (1=National ID, 2=Passport, 3=Iqama)
├── identifier_number
├── issue_date
├── expiry_date
├── is_primary
└── is_verified
```

## API Endpoint
- **PUT** `/api/hr/employees`
- Accepts `UpdateEmployeeRequest` body
- Now properly saves all identifier fields with expiry dates

## Notes
- The orchestration service handles the actual database insert/update
- Identifiers are versioned and can have effective date ranges
- Expiry dates are now properly stored and can trigger alerts for expiring documents
