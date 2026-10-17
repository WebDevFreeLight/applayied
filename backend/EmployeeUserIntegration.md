# Employee-User Integration Implementation

This document describes the implementation of automatic user account creation when adding employees to the HR system.

## Overview

When a new employee is added to the system, a corresponding user account is automatically created to enable system login and access.

## Changes Made

### 1. HrEmployee Model Enhancement

**File:** `ZR.Model/HR/HrEmployee.cs`

- Added `UserId` property to establish relationship between employee and user records
- This nullable field stores the ID of the associated user account

```csharp
/// <summary>
/// Associated User ID for system login
/// </summary>
[JsonConverter(typeof(ValueToStringConverter))]
public long? UserId { get; set; }
```

### 2. EmployeeService Integration

**File:** `ZR.Service/HR/EmployeeService.cs`

#### Dependencies Added:
- `using ZR.Model.System.Dto;`
- `using ZR.ServiceCore.Services;`
- Injected `ISysUserService` into constructor

#### User Creation Logic:
When adding an employee, the service now:

1. **Creates SysUserDto** with employee data:
   - Username: Employee Code
   - Nickname: Employee Name
   - Email: Employee Email
   - Phone: Employee Phone
   - Password: Default "123456"
   - Status: Active (0)
   - Department: Employee's Department
   - Gender: Default Male (0)
   - Remark: "Auto-created from employee"

2. **Calls UserService.AddUser()** to create the user account

3. **Links Records** by storing the created user's ID in `employee.UserId`

4. **Error Handling** ensures employee creation continues even if user creation fails

## Benefits

1. **Seamless Integration**: Employees automatically get system access
2. **Data Consistency**: Maintains relationship between HR and user data
3. **Error Resilience**: Employee creation doesn't fail if user creation encounters issues
4. **Audit Trail**: Clear linkage between employee and user records

## Usage Example

When adding an employee through the API:

```json
{
  "employeeCode": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "phone": "+1234567890",
  "deptId": 1,
  "status": 0
}
```

The system will:
1. Create the employee record
2. Automatically create a user account with username "EMP001"
3. Link them via the `UserId` field
4. The employee can now login with username "EMP001" and password "123456"

## Security Considerations

- Default password "123456" should be changed on first login
- Consider implementing password policy enforcement
- User accounts inherit department-based permissions

## Future Enhancements

- Add option to disable automatic user creation
- Implement custom password generation
- Add role assignment based on employee position
- Sync employee updates with user account data