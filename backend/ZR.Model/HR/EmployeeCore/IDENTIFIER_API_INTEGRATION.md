# Identifier Panel - API Integration

## Overview
Updated the Identifiers Panel component to use actual backend API instead of mock data. This enables full CRUD operations for employee/person identifiers (National ID, Passport, Iqama, etc.).

---

## Backend Changes

### 1. Created Person Identifier Controller
**File**: `ZR.Admin.WebApi/Controllers/HR/EmployeeCore/HrPersonIdentifierController.cs`

**Endpoints**:
- `GET /api/hr/employeecore/person/{personId}/identifier` - Get all identifiers
- `POST /api/hr/employeecore/person/{personId}/identifier` - Create identifier
- `PUT /api/hr/employeecore/person/{personId}/identifier/{identifierId}` - Update identifier
- `DELETE /api/hr/employeecore/person/{personId}/identifier/{identifierId}` - Delete identifier
- `POST /api/hr/employeecore/person/{personId}/identifier/{identifierId}/verify` - Verify identifier

**Features**:
- Full CRUD operations
- Audit trail (Create_by, Update_by timestamps)
- Permission checks
- Logging integration
- Validation checks

### 2. Updated Person Controller
**File**: `ZR.Admin.WebApi/Controllers/HR/EmployeeCore/HrPersonController.cs`

Added endpoint:
```csharp
GET /api/hr/employeecore/person/{id}/identifiers
```

Returns array of identifiers for a person.

### 3. Dictionary Seed Script
**File**: `sql/seed_identifier_types.sql`

Created dictionary entries for identifier types:
- Type 1: National ID
- Type 2: Passport
- Type 3: Iqama (Saudi Residence Permit)
- Type 4: Driver License
- Type 5: Social Security

---

## Frontend Changes

### 1. Created Identifier Service
**File**: `src/services/identifierService.ts`

TypeScript service for API communication:

```typescript
export interface Identifier {
  personIdentifierId: number;
  personId: number;
  identifierTypeDictCode?: number;
  identifierNumber: string;
  issuingCountryDictCode?: number;
  issueDate?: string;
  expiryDate?: string;
  isPrimary: boolean;
  isVerified: boolean;
  verifiedTime?: string;
}

export const identifierService = {
  getIdentifiers(personId)
  createIdentifier(personId, data)
  updateIdentifier(personId, identifierId, data)
  deleteIdentifier(personId, identifierId)
  verifyIdentifier(personId, identifierId)
}
```

### 2. Updated Identifiers Panel Component
**File**: `src/app/shared/employees/identifiers-panel.tsx`

**Changes**:
1. Imported `identifierService`
2. Changed dictionary type from `'sys_identifier_type'` to `'identifier_type'`
3. Replaced all mock API calls with real service calls

**Functions Updated**:

#### fetchIdentifiers()
```typescript
const response = await identifierService.getIdentifiers(personId);
if (response.code === 200) {
  setIdentifiers(response.data);
}
```

#### handleSave()
```typescript
// Create
if (isCreatingNew) {
  const response = await identifierService.createIdentifier(personId, saveData);
}

// Update
else if (editingIdentifier) {
  const response = await identifierService.updateIdentifier(
    personId,
    editingIdentifier.personIdentifierId,
    saveData
  );
}
```

#### handleDelete()
```typescript
const response = await identifierService.deleteIdentifier(
  personId,
  identifier.personIdentifierId
);
```

---

## Database Schema

### hr_person_identifier Table
```sql
person_identifier_id    BIGSERIAL PRIMARY KEY
person_id               BIGINT NOT NULL
identifier_type_dict_code  BIGINT (FK to sys_dict_data)
identifier_number       VARCHAR(100) NOT NULL
issuing_country_dict_code  BIGINT (FK to sys_dict_data)
issue_date              DATE
expiry_date             DATE
is_primary              BOOLEAN DEFAULT FALSE
is_verified             BOOLEAN DEFAULT FALSE
verified_time           TIMESTAMPTZ
```

**Indexes**:
- `person_id` (for fast person lookup)
- `identifier_type_dict_code` (for type filtering)
- `expiry_date` (for expiry alerts)

---

## Features

### Frontend Features
1. **View Identifiers**: Display all identifiers for a person
2. **Add Identifier**: Create new identifiers with full metadata
3. **Edit Identifier**: Update existing identifier details
4. **Delete Identifier**: Remove identifiers
5. **Expiry Tracking**: Visual alerts for expired/expiring identifiers
6. **Primary Identifier**: Mark one identifier as primary
7. **Verification Status**: Show verified status

### Backend Features
1. **CRUD Operations**: Full create, read, update, delete
2. **Audit Trail**: Track who created/updated records
3. **Permission Control**: Action-level permissions
4. **Validation**: Ensure data integrity
5. **Logging**: Business operation logging

---

## UI Features

### Visual Indicators
- **Primary Badge**: Star icon for primary identifier
- **Verified Badge**: Green checkmark for verified identifiers
- **Expiry Warnings**:
  - Red border + alert: Expired identifiers
  - Orange border + alert: Expiring within 60 days
  - Shows "Expires in X days"

### Form Fields
- Identifier Type (dropdown from dictionary)
- Identifier Number (required, monospace font)
- Issuing Country (dropdown from dictionary)
- Issue Date
- Expiry Date
- Primary checkbox

---

## Testing

### Prerequisites
1. Run the identifier types seed script:
```bash
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB \
  -f sql/seed_identifier_types.sql
```

2. Rebuild backend:
```bash
cd ZR.Admin.WebApi
dotnet build
dotnet run
```

### Test Scenarios

#### 1. View Identifiers
```
Navigate to employee details page
Click "Identifiers" tab
Should display existing identifiers or empty state
```

#### 2. Add Identifier
```
Click "Add Identifier" button
Fill in form:
  - Type: National ID
  - Number: 1234567890
  - Expiry: 2026-12-31
  - Check "Primary"
Click "Save"
Should show success toast and display new identifier
```

#### 3. Edit Identifier
```
Click edit icon on identifier card
Modify fields
Click "Save"
Should update and refresh list
```

#### 4. Delete Identifier
```
Click delete icon
Confirm deletion
Should remove identifier and refresh list
```

#### 5. Expiry Alerts
```
Create identifier with expiry date < today
Should show red border + "This identifier has expired!"

Create identifier expiring in 30 days
Should show orange border + "Expires in 30 days"
```

---

## API Request/Response Examples

### Get Identifiers
**Request**:
```http
GET /api/hr/employeecore/person/21/identifiers
```

**Response**:
```json
{
  "code": 200,
  "data": [
    {
      "personIdentifierId": 1,
      "personId": 21,
      "identifierTypeDictCode": 1,
      "identifierNumber": "1234567890",
      "issuingCountryDictCode": 682,
      "issueDate": "2020-01-01",
      "expiryDate": "2030-01-01",
      "isPrimary": true,
      "isVerified": true,
      "verifiedTime": "2025-12-15T10:30:00Z"
    }
  ]
}
```

### Create Identifier
**Request**:
```http
POST /api/hr/employeecore/person/21/identifier
Content-Type: application/json

{
  "identifierTypeDictCode": 3,
  "identifierNumber": "2234567890",
  "issuingCountryDictCode": 682,
  "issueDate": "2023-01-01",
  "expiryDate": "2028-01-01",
  "isPrimary": false
}
```

**Response**:
```json
{
  "code": 200,
  "msg": "success",
  "data": 1
}
```

---

## Error Handling

### Frontend
- Validates required fields before submission
- Shows user-friendly error toasts
- Handles network failures gracefully
- Displays loading states

### Backend
- Returns proper HTTP status codes
- Provides descriptive error messages
- Validates person ownership
- Prevents duplicate primary identifiers

---

## Security

### Authentication
- All endpoints require authentication via `[Verify]` attribute
- JWT token validation

### Authorization
- Action-level permissions: `hr:employeecore:person:query` and `hr:employeecore:person:edit`
- User can only access identifiers for persons they have permission to view

### Data Protection
- Audit trail tracks all changes
- Soft delete support (can be implemented)
- No sensitive data exposure in logs

---

## Future Enhancements

1. **Bulk Operations**: Add/update multiple identifiers at once
2. **Document Linking**: Link identifier to uploaded document scans
3. **Expiry Notifications**: Email alerts for expiring identifiers
4. **History Tracking**: View change history
5. **Verification Workflow**: Multi-step verification process
6. **OCR Integration**: Auto-extract data from ID scans
7. **Duplicate Detection**: Warn about duplicate identifier numbers

---

## Dependencies

### Backend
- SqlSugar ORM
- ASP.NET Core Web API
- PostgreSQL database

### Frontend
- Next.js 15
- TypeScript
- Rizzui components
- React Hot Toast
- Lucide React icons

---

## Notes

- The `personId` prop must be passed to the IdentifiersPanel component
- Identifiers are person-level data, not employee-specific
- Multiple persons can have the same identifier number (different types)
- Primary identifier should be unique per person
- Expiry tracking is automatic based on `expiryDate` field
