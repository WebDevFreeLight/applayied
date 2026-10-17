# HR Management System Integration Guide
# دليل تكامل نظام إدارة الموارد البشرية

## Overview / نظرة عامة
This document provides a comprehensive guide for integrating the HR Management System between the .NET backend and Vue.js frontend.

## System Architecture / هيكل النظام

### Backend (.NET Core)
- **Location**: `D:/maham/backend/`
- **API Base URL**: `http://localhost:5000/api/hr/`
- **Database**: SQL Server with Arabic support

### Frontend (Vue.js)
- **Location**: `F:/San@/maham-frontend/`
- **Dev Server**: `http://localhost:8080`
- **Language Support**: Arabic (ar) and English (en)

## HR Modules / وحدات الموارد البشرية

### 1. Employees Management / إدارة الموظفين
- Employee CRUD operations
- Profile management
- Department assignments
- Biometric ID mapping

### 2. Attendance & Time Tracking / الحضور والانصراف
- Biometric device integration
- Shift management
- Daily attendance logs
- Monthly reports

### 3. Leave Management / إدارة الإجازات
- Leave requests and approvals
- Balance tracking
- Holiday calendar
- Leave types configuration

### 4. Performance Management / إدارة الأداء
- Performance reviews
- Goal setting
- KPI tracking
- AI-powered insights

### 5. Payroll & Compliance / الرواتب والامتثال
- Salary calculation
- GOSI compliance
- Tax management
- Payment processing

### 6. Employee Documents / وثائق الموظفين
- Document upload/download
- Expiry tracking
- Verification workflow
- Document types management

### 7. Recruitment / التوظيف
- Job postings
- Candidate tracking
- Interview scheduling
- Assessment management

### 8. Hybrid Work / العمل الهجين
- Work schedule management
- Office/Home tracking
- Carbon footprint calculation
- Workspace allocation

### 9. Benefits / المزايا
- Insurance plans
- Enrollment management
- Dependent tracking
- Contribution calculations

## Database Setup / إعداد قاعدة البيانات

### 1. Run the HR schema migration:
```sql
-- Execute the script in:
D:/maham/backend/Database/hr_schema.sql
```

### 2. Verify tables created:
- hr_employee
- hr_attendance
- hr_attendance_log
- hr_attendance_shift
- hr_attendance_device
- hr_leave_request
- hr_leave_type
- hr_leave_balance
- hr_holiday
- hr_performance_review
- hr_performance_goal
- hr_payroll
- hr_employee_document
- hr_document_type
- hr_job_requisition
- hr_candidate
- hr_hybrid_schedule
- hr_insurance_plan
- hr_insurance_enrollment

## Backend Configuration / تكوين الخادم

### 1. Update appsettings.json:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=HRManagement;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "HR": {
    "BiometricDevices": {
      "SyncInterval": 300,
      "DefaultPort": 4370
    },
    "Leave": {
      "YearlyEntitlement": 21,
      "CarryOverDays": 5
    },
    "Payroll": {
      "GOSIEmployeeRate": 0.10,
      "GOSIEmployerRate": 0.12
    }
  }
}
```

### 2. Start the backend server:
```bash
cd D:/maham/backend
dotnet restore
dotnet build
dotnet run --project ZR.Admin.WebApi
```

## Frontend Configuration / تكوين الواجهة الأمامية

### 1. Install dependencies:
```bash
cd F:/San@/maham-frontend
npm install
```

### 2. Update environment variables (.env):
```env
VUE_APP_API_URL=http://localhost:5000
VUE_APP_DEFAULT_LANG=ar
VUE_APP_HR_MODULE=true
```

### 3. Start the development server:
```bash
npm run serve
```

## API Endpoints / نقاط نهاية API

### Employee Endpoints
- `GET /api/hr/employees/list` - Get employee list
- `GET /api/hr/employees/{id}` - Get employee details
- `POST /api/hr/employees` - Create employee
- `PUT /api/hr/employees` - Update employee
- `DELETE /api/hr/employees/{ids}` - Delete employees
- `GET /api/hr/employees/statistics` - Get statistics

### Attendance Endpoints
- `GET /api/hr/attendance/list` - Get attendance records
- `GET /api/hr/attendance/logs` - Get attendance logs
- `POST /api/hr/attendance` - Add attendance record
- `POST /api/hr/attendance/import/biometric` - Import biometric data
- `GET /api/hr/attendance/shifts` - Get shifts
- `GET /api/hr/attendance/daily-summary` - Get daily summary

### Leave Endpoints
- `GET /api/hr/leave/requests` - Get leave requests
- `POST /api/hr/leave/requests` - Submit leave request
- `POST /api/hr/leave/requests/{id}/approve` - Approve/reject leave
- `GET /api/hr/leave/balance/{employeeId}` - Get leave balance
- `GET /api/hr/leave/holidays` - Get holidays

## Testing Integration / اختبار التكامل

### 1. Test Employee Creation:
```javascript
// Frontend test
import { addEmployee } from '@/api/hr/employee'

const testEmployee = {
  employeeCode: 'EMP001',
  name: 'John Doe',
  nameAr: 'جون دو',
  email: 'john@example.com',
  phone: '+966501234567',
  departmentId: 1,
  status: 'active'
}

addEmployee(testEmployee).then(response => {
  console.log('Employee created:', response)
})
```

### 2. Test Attendance Import:
```javascript
// Test biometric data import
const formData = new FormData()
formData.append('file', biometricFile)
formData.append('deviceId', 1)

importBiometric(formData).then(response => {
  console.log('Biometric data imported:', response)
})
```

### 3. Test Leave Request:
```javascript
// Submit leave request
const leaveRequest = {
  employeeId: 1,
  leaveTypeId: 1,
  startDate: '2024-01-15',
  endDate: '2024-01-20',
  days: 5,
  reason: 'Annual vacation'
}

submitLeaveRequest(leaveRequest).then(response => {
  console.log('Leave request submitted:', response)
})
```

## Localization / التعريب

### Arabic translations are available in:
- Frontend: `F:/San@/maham-frontend/src/i18n/lang/ar.json`
- Backend: Uses resource files for Arabic support

### To switch language:
```javascript
// In Vue component
this.$i18n.locale = 'ar' // Switch to Arabic
this.$i18n.locale = 'en' // Switch to English
```

## Security Considerations / اعتبارات الأمن

1. **Authentication**: JWT token-based authentication
2. **Authorization**: Role-based access control (RBAC)
3. **Data Encryption**: Sensitive data encrypted at rest
4. **Audit Trail**: All HR operations are logged
5. **GDPR Compliance**: Personal data protection measures

## Troubleshooting / استكشاف الأخطاء

### Common Issues:

1. **CORS Error**:
   - Ensure CORS is configured in Program.cs
   - Check allowed origins match frontend URL

2. **Database Connection**:
   - Verify SQL Server is running
   - Check connection string in appsettings.json

3. **API Not Responding**:
   - Check backend is running on port 5000
   - Verify firewall settings

4. **Arabic Text Display**:
   - Ensure database collation supports Arabic
   - Set proper charset in frontend

## Performance Optimization / تحسين الأداء

1. **Caching**: Implement Redis for frequently accessed data
2. **Pagination**: Use server-side pagination for large datasets
3. **Lazy Loading**: Load HR modules on demand
4. **Image Optimization**: Compress employee photos
5. **Database Indexing**: Add indexes on frequently queried columns

## Deployment / النشر

### Backend Deployment:
```bash
dotnet publish -c Release -o ./publish
# Deploy to IIS or Azure App Service
```

### Frontend Deployment:
```bash
npm run build
# Deploy dist folder to web server
```

## Support / الدعم

For technical support or questions:
- Email: hr-support@maham.com
- Documentation: `/docs/hr-api`
- Logs: Check `D:/maham/backend/logs/`

## Version History / سجل الإصدارات

- **v1.0.0** (2024-01-10): Initial HR system integration
  - Employee management
  - Attendance tracking
  - Leave management
  - Basic payroll
  - Arabic localization

## License / الترخيص

This HR Management System is proprietary software.
© 2024 Maham. All rights reserved.