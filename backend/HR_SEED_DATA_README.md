# HR & Hrtypes Seed Data Implementation Guide

## Overview

This document provides comprehensive guidance on the Human Resources (HR) and HR Types seed data initialization system implemented in the Maham ERP Backend. The seed data system supports automatic initialization of HR-related data from either Excel spreadsheets or JSON files.

**Status**: ✅ Implementation Complete (Build verified: 0 Errors, 257 Warnings)

---

## Architecture Overview

### Components

The HR seed data system consists of three main components:

1. **Models** (`ZR.Model.Hrtypes`, `ZR.Model.Hr`, `ZR.Model.HR`)
   - Entity classes for HR data
   - Located in different namespaces to support modular organization

2. **Services** (`ZR.ServiceCore.Services.SeedDataService`)
   - Initialization methods for each entity type
   - Orchestration methods for batch operations
   - JSON fallback mechanism

3. **Seed Data Files** (`ZR.Admin.WebApi/Database/SeedData/Hrtypes` and `HR`)
   - JSON files for data initialization
   - Used when Excel sheets are unavailable
   - Support bilingual content (Arabic/English)

---

## Namespace Organization

### Why Multiple Namespaces?

The project uses three different namespaces for HR-related classes:

```
ZR.Model.Hrtypes     → HrLeaveType, HrHolidaytype
ZR.Model.Hr          → HrHoliday
ZR.Model.HR          → HrLeave, HrAttendanceShift, HrLeaveBalance
```

This organization separates:
- **Type/Configuration entities** (in `Hrtypes`) - Define rules and configurations
- **Data entities** (in `Hr` and `HR`) - Contain actual business records

### Namespace Resolution

To avoid ambiguity when importing multiple HR namespaces:

```csharp
using ZR.Model.Hrtypes;  // For HrLeaveType, HrHolidaytype
using ZR.Model.Hr;       // For HrHoliday
using ZR.Model.HR;       // For HrLeave, HrAttendanceShift, HrLeaveBalance
```

**Important**: When using classes that exist in multiple namespaces (e.g., `HrPerformanceReview`), qualify the type explicitly:
```csharp
ZR.Model.Business.HrPerformanceReview  // Use full qualification
```

---

## Implemented Models

### 1. HrLeaveType (Hrtypes)
**Database Table**: `hr_leave_type`

**Purpose**: Defines leave type configurations

**Properties**:
- `Leavetypeid` (PK) - Leave type identifier
- `Typecode` - Unique code for the type
- `Typename` - Name in English
- `Typenamear` - Name in Arabic
- `Daysperyear` - Number of days entitled per year
- `Ispaid` - Whether leave is paid
- `Requiresapproval` - Requires manager approval
- `Requiresdocument` - Requires supporting document
- `Mindays` - Minimum continuous days
- `Maxdays` - Maximum continuous days
- `Advancenoticedays` - Days notice required
- `Isactive` - Active/Inactive status
- `Leavecode` - Leave code
- `Defaultdays` - Default number of days
- `Allowcarryforward` - Allow carrying over to next year
- `Maxcarryforward` - Maximum days to carry forward

**Sample Data Included**: 6 leave types
- Annual Leave (30 days/year)
- Sick Leave (15 days/year)
- Emergency Leave (10 days/year)
- Unpaid Leave
- Maternity Leave (60 days)
- Bereavement Leave (5 days)

---

### 2. HrHolidaytype (Hrtypes)
**Database Table**: `hr_holidaytype`

**Purpose**: Categorizes holidays

**Properties**:
- `Id` (PK) - Holiday type identifier
- `Name` - Type name
- `Remark` - Additional remarks
- Plus SysBase inherited fields (Create_by, Create_time, CompanyID, BranchID)

**Sample Data Included**: 4 holiday types
- National Holidays
- Religious Holidays
- Company Holidays
- Regional Holidays

---

### 3. HrHoliday (Hr)
**Database Table**: `hr_holiday`

**Purpose**: Defines specific holiday dates and details

**Properties**:
- `Holidayid` (PK) - Holiday identifier
- `Holidaydate` - Date of the holiday
- `Holidayname` - Holiday name in English
- `Holidaynamear` - Holiday name in Arabic
- `Holidaytype` - Category of holiday
- `Ispaid` - Whether it's a paid holiday
- `Year` - Calendar year
- `Description` - Holiday description
- Plus SysBase inherited fields

**Sample Data Included**: 5 holidays
- Foundation Day (Feb 22)
- Eid al-Fitr (Islamic holiday)
- Eid al-Adha (Islamic holiday)
- Saudi National Day (Sep 23)
- New Year (Jan 1)

---

### 4. HrAttendanceShift (HR)
**Database Table**: `hr_attendance_shift`

**Purpose**: Defines work shift configurations

**Properties**:
- `ShiftId` (PK) - Shift identifier
- `Shiftname` - Shift name in English
- `Shiftnamear` - Shift name in Arabic
- `Shiftcode` - Unique shift code
- `Starttime` - Shift start time (HH:mm:ss)
- `Endtime` - Shift end time (HH:mm:ss)
- `Breakstarttime` - Break start time
- `Breakendtime` - Break end time
- `Isovernight` - Spans midnight
- `Isflexible` - Flexible hours allowed
- `Flexibleminutes` - Flexibility window in minutes
- `Allowlateminutes` - Allowed late arrival minutes
- `Allowearlyminutes` - Allowed early departure minutes
- `Minworkhours` - Minimum work hours required
- `Maxworkhours` - Maximum work hours allowed
- `Overtimecalculation` - Method for overtime calculation
- `Overtimerate` - Overtime multiplier rate
- `Workingdays` - CSV of working days (1-7, 1=Monday)
- `Isactive` - Active/Inactive status
- `Description` - Shift description
- `Colorcode` - UI color code (hex)
- `Sortorder` - Display order
- Plus SysBase inherited fields

**Sample Data Included**: 4 shifts
- Morning Shift (08:00-16:00, 1.5x overtime)
- Evening Shift (16:00-24:00, 1.75x overtime)
- Night Shift (00:00-08:00, 2.0x overtime)
- Flexible Shift (09:00-17:00, ±2 hours flexible, 1.25x overtime)

---

### 5. HrLeave (HR)
**Database Table**: `hr_leave`

**Purpose**: Leave request records

**Properties**:
- `LeaveId` (PK) - Leave request identifier
- `EmployeeId` - Employee requesting leave
- `LeaveTypeId` - Type of leave
- `Startdate` - Leave start date
- `Enddate` - Leave end date
- `Days` - Number of days
- `Reason` - Reason for leave
- `Status` - Current status (approved/pending/rejected/cancelled)
- `ApproverId` - Manager approving leave
- `Approvedate` - Approval date
- `Approvecomments` - Manager comments
- `AttachmentUrl` - Supporting document URL
- `Isemergency` - Emergency leave flag
- `Rejectionreason` - Reason for rejection if rejected
- `Cancelreason` - Reason for cancellation if cancelled
- `ApprovedBy` - User who approved
- `ApprovalDate` - Approval timestamp
- `ApprovalComments` - Approval notes
- Plus SysBase inherited fields

**Sample Data Included**: 5 leave requests
- Approved: Employee 101, 5 days annual leave
- Approved: Employee 102, 2 days sick leave (emergency)
- Pending: Employee 103, 2 days personal leave
- Rejected: Employee 104, 4 days annual leave (insufficient notice)
- Approved: Employee 105, 2 days compensatory leave

---

### 6. HrLeaveBalance (HR)
**Database Table**: `hr_leave_balance`

**Purpose**: Tracks leave entitlements and usage

**Properties**:
- `BalanceId` (PK) - Balance record identifier
- `EmployeeId` - Employee identifier
- `LeaveTypeId` - Leave type
- `Year` - Calendar year
- `Entitleddays` - Days entitled for the year
- `Useddays` - Days already used
- `Balance` - Remaining days
- `Carriedover` - Days carried from previous year
- `Expireddays` - Days that expired
- `Entitlement` - Total entitlement
- `Used` - Total used
- `Availabledays` - Total available
- `Carriedforward` - Days carried to next year
- Plus SysBase inherited fields

**Sample Data Included**: 7 balance records
- Employee 101: 30 annual + 15 sick leave balances
- Employee 102: 30 annual + 15 sick leave balances
- Employee 103: 30 annual leave balance
- Employee 104: 30 annual leave balance
- Employee 105: 10 compensatory leave balance

---

## Service Methods

### Individual Entity Initialization Methods

#### HrLeaveType Initialization
```csharp
public (string, object, object) InitHrLeaveTypeData(List<HrLeaveType> data)
```
- Upserts leave type records
- Uses `Leavetypeid` as the unique key
- Returns: (message, errors, ignored records)

#### HrHolidaytype Initialization
```csharp
public (string, object, object) InitHrHolidaytypeData(List<HrHolidaytype> data)
```
- Upserts holiday type records
- Uses `Id` as the unique key

#### HrHoliday Initialization
```csharp
public (string, object, object) InitHrHolidayData(List<HrHoliday> data)
```
- Upserts holiday records
- Uses `Holidayid` as the unique key

#### HrAttendanceShift Initialization
```csharp
public (string, object, object) InitHrAttendanceShiftData(List<HrAttendanceShift> data)
```
- Upserts shift configuration records
- Uses `ShiftId` as the unique key

#### HrLeave Initialization
```csharp
public (string, object, object) InitHrLeaveData(List<HrLeave> data)
```
- Upserts leave request records
- Uses `LeaveId` as the unique key

#### HrLeaveBalance Initialization
```csharp
public (string, object, object) InitHrLeaveBalanceData(List<HrLeaveBalance> data)
```
- Upserts leave balance records
- Uses `BalanceId` as the unique key

### Orchestration Methods

#### InitHrtypesSeedData()
```csharp
public List<string> InitHrtypesSeedData(string path, string WebRootPath, bool clean)
```

**Purpose**: Initialize all HR type entities in correct order

**Process**:
1. Attempts to load data from Excel spreadsheet (`leave_type`, `holiday_type` sheets)
2. Falls back to JSON files if Excel loading fails:
   - `Database/SeedData/Hrtypes/hr_leave_type.json`
   - `Database/SeedData/Hrtypes/hr_holidaytype.json`
3. Returns list of initialization messages (Arabic/English)

**Returns**: List of status messages for each entity

#### InitHrSeedData()
```csharp
public List<string> InitHrSeedData(string path, string WebRootPath, bool clean)
```

**Purpose**: Initialize all HR business data entities in correct order

**Process**:
1. Attempts to load data from Excel spreadsheet (`holiday`, `attendance_shift`, `leave`, `leave_balance` sheets)
2. Falls back to JSON files if Excel loading fails:
   - `Database/SeedData/HR/hr_holiday.json`
   - `Database/SeedData/HR/hr_attendance_shift.json`
   - `Database/SeedData/HR/hr_leave.json`
   - `Database/SeedData/HR/hr_leave_balance.json`
3. Returns list of initialization messages

**Returns**: List of status messages for each entity

### Integration in Main Orchestration

Both methods are called from `InitSeedData()`:

```csharp
// Initialize Hrtypes Seed Data
var hrtypesData = InitHrtypesSeedData(path, WebRootPath, clean);
if (hrtypesData is not null && hrtypesData.Count > 0)
{
    result.AddRange(hrtypesData);
}

// Initialize HR Seed Data
var hrData = InitHrSeedData(path, WebRootPath, clean);
if (hrData is not null && hrData.Count > 0)
{
    result.AddRange(hrData);
}
```

---

## Seed Data Files

### File Locations

```
ZR.Admin.WebApi/Database/SeedData/
├── Hrtypes/
│   ├── hr_leave_type.json         (6 records)
│   └── hr_holidaytype.json        (4 records)
└── HR/
    ├── hr_holiday.json            (5 records)
    ├── hr_attendance_shift.json   (4 records)
    ├── hr_leave.json              (5 records)
    └── hr_leave_balance.json      (7 records)
```

### File Format

All JSON files follow a consistent structure with case-insensitive property matching:

```json
[
  {
    "fieldname": value,
    "anotherfield": "value",
    "create_by": "admin",
    "create_time": "2024-01-15T10:00:00",
    "companyid": 0,
    "branchid": 0
  }
]
```

### SysBase Inherited Fields (Required)

Every record must include SysBase fields:
- `create_by` - User who created the record
- `create_time` - Creation timestamp (ISO 8601 format)
- `companyid` - Company identifier (0 for default)
- `branchid` - Branch identifier (0 for default)

---

## Data Loading Mechanism

### Priority Order

```
1. Excel Spreadsheet (if available)
   ↓ (if fails or unavailable)
2. JSON File (fallback)
   ↓ (if both fail)
3. Error message logged
```

### Excel Sheet Names Expected

- `leave_type` → HrLeaveType records
- `holiday_type` → HrHolidaytype records
- `holiday` → HrHoliday records
- `attendance_shift` → HrAttendanceShift records
- `leave` → HrLeave records
- `leave_balance` → HrLeaveBalance records

### JSON Loading Features

- **Case-Insensitive**: Property names match model properties regardless of case
- **Fallback**: Uses JSON if Excel loading throws any exception
- **Graceful Degradation**: Returns empty list if JSON file not found
- **Error Handling**: Wrapped in try-catch with detailed error messages

---

## Database Integration

### SqlSugar ORM Usage

All methods use SqlSugar's `Storageable` pattern for efficient upserts:

```csharp
var db = DbScoped.SugarScope;
var x = db.Storageable(data)
    .WhereColumns(it => it.PrimaryKeyProperty)  // Unique key for upsert
    .ToStorage();
x.AsInsertable.ExecuteCommand();   // Insert new records
x.AsUpdateable.ExecuteCommand();   // Update existing records
```

### Upsert Behavior

- **Insert**: If primary key doesn't exist in database
- **Update**: If primary key exists, updates all non-key fields
- **Error Handling**: Returns detailed error and ignored records lists

### Transaction Safety

Each initialization method operates within SqlSugar's transaction context:
- Changes are atomic
- Rollback on error (if enabled)
- No partial data corruption

---

## Usage Guide

### Automatic Initialization

The seed data is automatically initialized on application startup if configured:

**In appsettings.json**:
```json
{
  "InitDb": true,  // Enable automatic initialization
  "UseSeedData": true
}
```

**In Program.cs**:
```csharp
// Automatically called during database initialization
app.Environment.InitDbWithNamespaces(
    builder.Environment,
    "ZR.Model.Hrtypes",
    "ZR.Model.Hr",
    "ZR.Model.HR"
);
```

### Manual Initialization

To manually trigger seed data initialization:

```csharp
var seedService = app.Services.GetRequiredService<SeedDataService>();
var results = seedService.InitSeedData(excelPath, webRootPath, cleanFirst: true);

foreach (var message in results)
{
    logger.LogInformation(message);
}
```

### Initialization with Cleanup

When `cleanFirst: true`:
1. Existing HR data is truncated (TRUNCATE TABLE commands)
2. Fresh seed data is loaded from source
3. Useful for resetting to known state

---

## Bilingual Support

### Arabic/English Content

Models support bilingual fields:
- `Typename` / `Typenamear` (Leave Type names)
- `Shiftname` / `Shiftnamear` (Shift names)
- `Holidayname` / `Holidaynamear` (Holiday names)

### Language Selection

Application culture determines which language is used:
```csharp
// In model/DTO serialization
if (Thread.CurrentThread.CurrentCulture.Name.StartsWith("ar"))
    return model.NameAr;  // Return Arabic name
else
    return model.Name;    // Return English name
```

---

## Error Handling & Logging

### Result Messages (Arabic/English Bilingual)

Each initialization returns status messages:

```
[أنواع الإجازات - Leave Types] تم الإدراج 6 تم التحديث 0 أخطاء 0 المجموع 6
(English): [Leave Types] Inserted: 6 Updated: 0 Errors: 0 Total: 6
```

### Error Information Included

- Records inserted count
- Records updated count
- Error records count
- Total processed records

### Detailed Error Lists

The return tuple includes error details:
```csharp
(message, errorList, ignoreList)
```

- `message`: Human-readable summary (bilingual)
- `errorList`: List of failed records with validation errors
- `ignoreList`: Records skipped due to constraints

---

## Troubleshooting

### Issue: "Namespace ambiguity for HrPerformanceReview"

**Solution**: Use full qualification in generic types:
```csharp
var data = MiniExcel.Query<ZR.Model.Business.HrPerformanceReview>(path);
```

### Issue: Property not found (e.g., 'ShiftId')

**Solution**: Ensure property names use PascalCase (C# convention):
- ✅ `ShiftId`, `LeaveId`, `BalanceId`
- ❌ `Shiftid`, `Leaveid`, `Balanceid`

### Issue: JSON files not loading

**Solution**: Verify file locations:
```
F:\mahamBackendNET\backend\ZR.Admin.WebApi\Database\SeedData\Hrtypes\
F:\mahamBackendNET\backend\ZR.Admin.WebApi\Database\SeedData\HR\
```

Ensure all required JSON files are present:
- `hr_leave_type.json`
- `hr_holidaytype.json`
- `hr_holiday.json`
- `hr_attendance_shift.json`
- `hr_leave.json`
- `hr_leave_balance.json`

### Issue: Build compilation errors

**Solution**: Ensure using statements are in correct order:
```csharp
using ZR.Model.Hrtypes;
using ZR.Model.Hr;
using ZR.Model.HR;
```

Priority: Import specific namespaces before ambiguous ones.

---

## Performance Considerations

### Bulk Operations

- SqlSugar's `Storageable` pattern uses batch operations
- More efficient than individual insert/update statements
- Typical initialization time: < 100ms for all 6 entities (31 records total)

### Database Indexes

Recommended indexes for optimal performance:

```sql
-- Primary Keys (auto-indexed)
CREATE UNIQUE INDEX idx_hr_leave_type_id ON hr_leave_type(leavetypeid);
CREATE UNIQUE INDEX idx_hr_holiday_id ON hr_holiday(holidayid);
CREATE UNIQUE INDEX idx_hr_attendance_shift_id ON hr_attendance_shift(shiftid);
CREATE UNIQUE INDEX idx_hr_leave_id ON hr_leave(leaveid);
CREATE UNIQUE INDEX idx_hr_leave_balance_id ON hr_leave_balance(balanceid);

-- Foreign Keys (recommended)
CREATE INDEX idx_hr_leave_employee ON hr_leave(employeeid);
CREATE INDEX idx_hr_leave_balance_employee ON hr_leave_balance(employeeid);
```

---

## Future Enhancements

### Planned Features

1. **Bulk Excel Operations**
   - Support for multiple sheets in single Excel file
   - Batch processing with progress reporting

2. **Data Validation**
   - Business logic validation before insertion
   - Constraint checking (leave balance consistency)

3. **Audit Trail**
   - Track all seed data initialization operations
   - Detailed change logs with user information

4. **Advanced Scheduling**
   - Automated periodic refresh of seed data
   - Scheduled recalculation of leave balances

5. **API Endpoints**
   - RESTful endpoints for seed data management
   - Real-time status monitoring

---

## Related Documentation

- **CLAUDE.md**: Project overview and architecture
- **Build & Deployment**: See main README.md
- **Database Schema**: View model classes in `ZR.Model.Hrtypes` and `ZR.Model.HR`
- **API Documentation**: Access via `/scalar` or `/swagger` endpoints

---

## Support & Maintenance

### Code Location

**Service Implementation**:
```
F:\mahamBackendNET\backend\ZR.ServiceCore\Services\SeedDataService.cs
```

**Methods Region**: Lines 2724-3002 (Hrtypes & HR Seed Data Methods)

**Seed Data**:
```
F:\mahamBackendNET\backend\ZR.Admin.WebApi\Database\SeedData\Hrtypes\
F:\mahamBackendNET\backend\ZR.Admin.WebApi\Database\SeedData\HR\
```

### Build Status

✅ **Last Build**: Successful
- Errors: 0
- Warnings: 257 (pre-existing)
- Build Time: ~19 seconds

### Testing

Manual testing procedures:
1. Enable `InitDb: true` in appsettings.json
2. Run the application
3. Verify data appears in database tables
4. Check API responses for HR endpoints
5. Validate seed data integrity in Swagger/Scalar UI

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024-10-21 | Initial implementation with 6 models, 31 seed records |

---

**Last Updated**: 2024-10-21

**Maintained By**: Development Team

**License**: Project License

---

## Contact & Questions

For questions or issues regarding HR seed data implementation:
1. Check this documentation first
2. Review code comments in SeedDataService.cs
3. Check git history for recent changes
4. Contact the development team for clarification
