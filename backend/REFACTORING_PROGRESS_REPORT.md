# Employee System Refactoring - Progress Report

## Objective
Clean up old employee model and modernize the employee management system with:
- Delete old flat `HrEmployee` model
- Use normalized `EmployeeCore` structure
- Clean API with proper DTOs
- Modern error handling
- Simplified architecture

## Progress So Far

### ✅ COMPLETED

#### 1. Modern API DTOs Created
**File**: `ZR.Model/HR/Dto/EmployeeCore/EmployeeApiDto.cs`

**What it contains:**
- `EmployeeApiDto` - Full employee details for frontend
- `EmployeeListDto` - Lightweight for list views
- `CreateEmployeeRequest` - Input for creating employees
- `UpdateEmployeeRequest` - Input for updating employees
- `EmployeeQueryRequest` - For filtering/searching
- `EmployeeStatisticsResponse` - Statistics summary

**Benefits:**
- Clean, flat structure for frontend
- Strongly typed with JSON converters
- Masked sensitive data (bank info)
- Dictionary labels (Male/Female instead of 0/1)

#### 2. Clean Mapping Service Created
**File**: `ZR.Service/HR/EmployeeCore/EmployeeApiMappingService.cs`

**What it does:**
- Converts normalized EmployeeCore → flat API DTOs
- Fetches related data (name, contact, address, assignment, etc.)
- Decrypts and masks sensitive bank data
- Resolves dictionary labels
- Gets department/position names

**Methods:**
- `ToApiDtoAsync(long employeeId)` - Full details
- `ToListDtoAsync(long employeeId)` - Lightweight
- `ToListDtoAsync(List<long> employeeIds)` - Batch conversion

#### 3. New Modern Controller Created
**File**: `ZR.Admin.WebApi/Controllers/HR/EmployeesController.cs` (NEW)

**Old controller backed up to**: `EmployeesController.cs.OLD`

**Endpoints:**
- `GET /api/hr/employees/list` - Paginated employee list
- `GET /api/hr/employees/{id}` - Employee details
- `POST /api/hr/employees` - Create employee
- `PUT /api/hr/employees` - Update employee
- `DELETE /api/hr/employees/{ids}` - Delete employee(s)
- `GET /api/hr/employees/statistics` - Statistics
- `GET /api/hr/employees/export` - Export to Excel

**Features:**
- Uses `EmployeeApiMappingService` for clean DTOs
- Uses `EmployeeCoreOrchestrationService` for operations
- Proper error handling with try-catch
- Audit fields (created_by, updated_by)
- Soft delete (mark inactive instead of deleting)

## ⚠️ INCOMPLETE / IN PROGRESS

### 4. Controller Mapping Methods
**Status**: Partially implemented

The `MapToCreateDto` and `MapToUpdateDtoAsync` methods need to be completed to handle:
- Contacts (email, phone, personal email, mobile)
- Identifiers (ID number, passport)
- Addresses
- Assignments (department, position, manager)
- Contracts
- Statutory info (GOSI, medical insurance)
- Bank accounts (with encryption)
- Emergency contacts

**Current issue**: Methods are skeletal - need full mapping logic

## 📋 NEXT STEPS

### Step 1: Complete Controller Mapping (CURRENT)
Need to implement full mapping in:
- `MapToCreateDto(CreateEmployeeRequest request)`
- `MapToUpdateDtoAsync(UpdateEmployeeRequest request, long personId)`

### Step 2: Delete Old Employee Model
Files to delete:
- `ZR.Model/HR/Employees/HrEmployee.cs`
- `ZR.Model/HR/Employees/HrEmployeeDocument.cs`
- `ZR.Model/HR/Employees/HrDocument.cs`
- `ZR.Service/HR/Employees/EmployeeService.cs`
- `ZR.Repository/HR/EmployeeRepository.cs`
- `ZR.Service/HR/EmployeeCore/EmployeeCoreAdapterService.cs` (adapter)

### Step 3: Update Dependent Services
36 files reference the old `HrEmployee` model. Need to update:
- Payroll services
- Attendance services
- Leave services
- Performance services
- Analytics services
- etc.

Most can be updated to use `EmployeeId` lookups instead of full `HrEmployee` objects.

### Step 4: Update Frontend
**Files to update:**
- `employeeService.ts` - Update API calls to use new DTOs
- `view.tsx` - Already partially updated, needs testing
- `personal-info-edit-modal.tsx` - Update field mappings
- `financial-info-edit-modal.tsx` - Update field mappings

**Changes needed:**
- Use new DTO structure
- Handle dictionary codes properly (0/1 for gender)
- Test all CRUD operations

### Step 5: Test Everything
- Create employee
- Update employee (personal, financial, etc.)
- List employees with filters
- Delete employee
- Export employees
- Statistics

## 🎯 DECISION POINT

**OPTION A: Complete Current Approach (Recommended)**
- Finish the mapping methods
- Test the new controller
- Delete old files
- Update frontend
- **Time**: Medium effort
- **Risk**: Low (we have backups)

**OPTION B: Use Adapter Temporarily**
- Keep the old `EmployeeCoreAdapterService` for now
- Use it in the new controller
- Gradually migrate away from it
- **Time**: Low effort
- **Risk**: Medium (keeps old code)

**OPTION C: Radical Clean Up**
- Delete everything old immediately
- Fix compilation errors as they come
- Force migration
- **Time**: High effort
- **Risk**: High (might break things)

## 📊 ARCHITECTURE COMPARISON

### BEFORE (Old System)
```
Frontend → EmployeesController (uses old HrEmployee)
                ↓
        Old EmployeeService
                ↓
        Old EmployeeRepository
                ↓
        hr_employee table (1 flat table)
```

### TRANSITION (Current State)
```
Frontend → NEW EmployeesController
                ↓
        EmployeeApiMappingService
                ↓
        EmployeeCoreOrchestrationService
                ↓
        20+ Services (HrPersonService, HrEmployeeCoreService, etc.)
                ↓
        16 normalized tables
```

### AFTER (Target)
```
Frontend (Clean DTOs) → NEW EmployeesController
                            ↓
            EmployeeApiMappingService (EmployeeCore → DTOs)
                            ↓
            EmployeeCoreOrchestrationService (Business Logic)
                            ↓
            Individual Services (Person, Name, Contact, Assignment, etc.)
                            ↓
            16 Normalized Tables (Modern Database Design)
```

## 🚀 RECOMMENDATIONS

1. **Let's complete the mapping methods** - This is 80% done, just needs the detailed mapping

2. **Test the new controller** with Postman/Swagger before deleting old files

3. **Delete old files incrementally**:
   - First: Backup everything
   - Second: Delete old model files
   - Third: Fix compilation errors in dependent services
   - Fourth: Test thoroughly

4. **Update frontend last** - After backend is stable

5. **Keep adapter as fallback** - Don't delete `EmployeeCoreAdapterService.cs` until we're 100% sure

## ⏱️ ESTIMATED TIME

- Complete mapping methods: 1-2 hours
- Delete old files + fix errors: 2-3 hours
- Update dependent services: 3-4 hours
- Update frontend: 2-3 hours
- Testing: 2-3 hours

**Total**: 10-15 hours of focused work

## 🤔 YOUR INPUT NEEDED

**Which option do you prefer?**

1. **Option A**: Finish the current approach (complete mappings, test, then delete old)
2. **Option B**: Use adapter temporarily (quick win, migrate later)
3. **Option C**: Delete everything now (radical but risky)

**Also:**
- Do you want me to continue right now, or pause and let you review?
- Should I create a branch/backup before proceeding?
- Any specific concerns or requirements I should know about?

Let me know and I'll proceed accordingly!
