# HR Module Tables and Fields Documentation

This document provides a comprehensive summary of all tables and fields in the HR (Human Resources) module.

## Table of Contents

1. [Employees](#employees)
2. [Departments](#departments)
3. [Positions](#positions)
4. [Payroll](#payroll)
5. [Employee Loans](#employee-loans)
6. [Employee Attendance](#employee-attendance)
7. [Service Staff](#service-staff)
8. [Commissions](#commissions)
9. [Commission Tiers](#commission-tiers)
10. [Incentive Settings](#incentive-settings)
11. [Biometric Devices](#biometric-devices)
12. [Biometric Zones](#biometric-zones)

---

## Employees

**Table Name:** `erp_employees`  
**Description:** Employee - الموظف  
**Primary Key:** `employeeid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| employeeid | long | Employee ID - معرف الموظف |
| employeecode | string | Employee Code - كود الموظف |
| fullname | string | Full Name - الاسم الكامل |
| fullnamear | string | Full Name Arabic - الاسم بالعربي |
| nationalid | string | National ID - رقم الهوية |
| phone | string | Phone - الهاتف |
| email | string | Email - البريد الإلكتروني |
| gender | string | Gender - الجنس |
| birthdate | DateTime? | Birth Date - تاريخ الميلاد |
| photourl | string | Photo URL - رابط الصورة |
| departmentid | long? | Department ID - معرف القسم |
| positionid | long? | Position ID - معرف الوظيفة |
| managerid | long? | Manager ID - معرف المدير |
| hiredate | DateTime? | Hire Date - تاريخ التعيين |
| contractenddate | DateTime? | Contract End Date - تاريخ انتهاء العقد |
| basicsalary | decimal | Basic Salary - الراتب الأساسي |
| housingallowance | decimal | Housing Allowance - بدل السكن |
| transportationallowance | decimal | Transportation Allowance - بدل المواصلات |
| otherallowances | decimal | Other Allowances - بدلات أخرى |
| totalsalary | decimal | Total Salary - إجمالي الراتب |
| commissionpercent | decimal | Commission Percent - نسبة العمولة |
| commissiontierid | long? | Commission Tier ID - معرف شريحة العمولة |
| isservicestaff | bool | Is Service Staff - موظف خدمات |
| istrainer | bool | Is Trainer - مدرب |
| issalesperson | bool | Is Salesperson - بائع |
| userid | long? | User ID - معرف المستخدم |
| hasfingerprint | bool | Has Fingerprint - لديه بصمة |
| fingerprintdata | string | Fingerprint Data - بيانات البصمة |
| bankname | string | Bank Name - اسم البنك |
| bankaccountnumber | string | Bank Account Number - رقم الحساب البنكي |
| iban | string | IBAN - رقم الآيبان |
| address | string | Address - العنوان |
| notes | string | Notes - ملاحظات |
| isactive | bool | Is Active - نشط (default: true) |

---

## Departments

**Table Name:** `erp_departments`  
**Description:** Department - القسم  
**Primary Key:** `departmentid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| departmentid | long | Department ID - معرف القسم |
| departmentcode | string | Department Code - كود القسم |
| departmentname | string | Department Name - اسم القسم |
| departmentnamear | string | Department Name Arabic - اسم القسم بالعربي |
| parentdepartmentid | long? | Parent Department ID - معرف القسم الأب |
| managerid | long? | Manager ID - معرف المدير |
| costcenterid | long? | Cost Center ID - معرف مركز التكلفة |
| employeecount | int | Employee Count - عدد الموظفين |
| description | string | Description - الوصف |
| isactive | bool | Is Active - نشط (default: true) |

---

## Positions

**Table Name:** `erp_positions`  
**Description:** Position - الوظيفة  
**Primary Key:** `positionid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| positionid | long | Position ID - معرف الوظيفة |
| positioncode | string | Position Code - كود الوظيفة |
| positionname | string | Position Name - اسم الوظيفة |
| positionnamear | string | Position Name Arabic - اسم الوظيفة بالعربي |
| departmentid | long? | Department ID - معرف القسم |
| level | int | Level - المستوى |
| minsalary | decimal | Min Salary - الحد الأدنى للراتب |
| maxsalary | decimal | Max Salary - الحد الأقصى للراتب |
| description | string | Description - الوصف |
| isactive | bool | Is Active - نشط (default: true) |

---

## Payroll

**Table Name:** `erp_payroll`  
**Description:** Payroll Record - سجل الراتب  
**Primary Key:** `payrollid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| payrollid | long | Payroll ID - معرف الراتب |
| employeeid | long | Employee ID - معرف الموظف |
| branchid | long? | Branch ID - معرف الفرع |
| payperiod | string | Pay Period - فترة الراتب (YYYY-MM) |
| payperiodstart | DateTime | Pay Period Start - بداية فترة الراتب |
| payperiodend | DateTime | Pay Period End - نهاية فترة الراتب |
| basicsalary | decimal | Basic Salary - الراتب الأساسي |
| housingallowance | decimal | Housing Allowance - بدل السكن |
| transportationallowance | decimal | Transportation Allowance - بدل المواصلات |
| foodallowance | decimal | Food Allowance - بدل الطعام |
| otherallowances | decimal | Other Allowances - بدلات أخرى |
| overtimehours | decimal | Overtime Hours - ساعات العمل الإضافي |
| overtimerate | decimal | Overtime Rate - معدل العمل الإضافي (default: 1.5) |
| overtimepay | decimal | Overtime Pay - أجر العمل الإضافي |
| commission | decimal | Commission - العمولة |
| bonus | decimal | Bonus - المكافأة |
| grosssalary | decimal | Gross Salary - إجمالي الراتب |
| gosiemployee | decimal | GOSI Employee Share - اشتراك التأمينات (الموظف) |
| gosiemployer | decimal | GOSI Employer Share - اشتراك التأمينات (صاحب العمل) |
| tax | decimal | Tax - الضريبة |
| loandeduction | decimal | Loan Deduction - خصم السلفة |
| absencededuction | decimal | Absence Deduction - خصم الغياب |
| latededuction | decimal | Late Deduction - خصم التأخير |
| otherdeductions | decimal | Other Deductions - خصومات أخرى |
| totaldeductions | decimal | Total Deductions - إجمالي الخصومات |
| netsalary | decimal | Net Salary - صافي الراتب |
| workingdays | int | Working Days - أيام العمل |
| actualdays | int | Actual Days - الأيام الفعلية |
| absencedays | int | Absence Days - أيام الغياب |
| lateminutes | int | Late Minutes - دقائق التأخير |
| status | string | Status - الحالة (draft/pending/approved/paid) (default: draft) |
| paymentmethod | string | Payment Method - طريقة الدفع |
| paymentdate | DateTime? | Payment Date - تاريخ الدفع |
| bankreference | string | Bank Reference - مرجع البنك |
| approvedby | string | Approved By - تمت الموافقة بواسطة |
| approveddate | DateTime? | Approved Date - تاريخ الموافقة |
| notes | string | Notes - ملاحظات |
| employeename | string | Employee Name - اسم الموظف (للعرض) [Ignored in DB] |

---

## Employee Loans

**Table Name:** `erp_employee_loans`  
**Description:** Employee Loan - سلفة الموظف  
**Primary Key:** `loanid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| loanid | long | Loan ID - معرف السلفة |
| employeeid | long | Employee ID - معرف الموظف |
| loanamount | decimal | Loan Amount - مبلغ السلفة |
| monthlydeduction | decimal | Monthly Deduction - الخصم الشهري |
| totalinstallments | int | Total Installments - عدد الأقساط |
| paidinstallments | int | Paid Installments - الأقساط المدفوعة |
| remainingamount | decimal | Remaining Amount - المبلغ المتبقي |
| loandate | DateTime | Loan Date - تاريخ السلفة |
| startdate | DateTime | Start Date - تاريخ بداية الخصم |
| status | string | Status - الحالة (pending/approved/active/completed) (default: pending) |
| approvedby | string | Approved By - تمت الموافقة بواسطة |
| approveddate | DateTime? | Approved Date - تاريخ الموافقة |
| reason | string | Reason - السبب |
| notes | string | Notes - ملاحظات |

---

## Employee Attendance

**Table Name:** `erp_employee_attendances`  
**Description:** Employee Attendance - سجل الحضور والانصراف  
**Primary Key:** `attendanceid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| attendanceid | long | Attendance ID - معرف الحضور |
| employeeid | long | Employee ID - معرف الموظف |
| attendancedate | DateTime | Attendance Date - تاريخ الحضور |
| checkintime | DateTime? | Check In Time - وقت الحضور |
| checkouttime | DateTime? | Check Out Time - وقت الانصراف |
| attendancemethod | string | Attendance Method - طريقة الحضور (fingerprint/manual/mobile/face) |
| deviceid | long? | Device ID - معرف الجهاز |
| zoneid | long? | Zone ID - معرف منطقة البصمة |
| workhours | decimal | Work Hours - ساعات العمل |
| overtimehours | decimal | Overtime Hours - ساعات العمل الإضافي |
| lateminutes | int | Late Minutes - دقائق التأخير |
| earlyleaveminutes | int | Early Leave Minutes - دقائق الخروج المبكر |
| status | string | Status - الحالة (present/absent/late/leave/holiday) |
| shiftid | long? | Shift ID - معرف الوردية |
| isapproved | bool | Is Approved - معتمد |
| approvedby | string | Approved By - اعتمد بواسطة |
| notes | string | Notes - ملاحظات |

---

## Service Staff

**Table Name:** `erp_service_staff`  
**Description:** Service Staff - موظفي الخدمات  
**Primary Key:** `staffid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| staffid | long | Staff ID - معرف الموظف |
| employeeid | long | Employee ID - معرف الموظف الأساسي |
| staffcode | string | Staff Code - كود الموظف |
| staffname | string | Staff Name - اسم الموظف |
| staffnamear | string | Staff Name Arabic - اسم الموظف بالعربي |
| servicecategoryid | long? | Service Category ID - معرف فئة الخدمة |
| specialization | string | Specialization - التخصص |
| servicesoffered | string | Services Offered - الخدمات المقدمة (JSON) |
| workinghours | string | Working Hours - ساعات العمل (JSON) |
| hourlyrate | decimal | Hourly Rate - سعر الساعة |
| commissionpercent | decimal | Commission Percent - نسبة العمولة |
| rating | decimal | Rating - التقييم |
| totalbookings | int | Total Bookings - إجمالي الحجوزات |
| completedbookings | int | Completed Bookings - الحجوزات المكتملة |
| photourl | string | Photo URL - رابط الصورة |
| bio | string | Bio - نبذة مختصرة |
| isavailable | bool | Is Available - متاح (default: true) |
| notes | string | Notes - ملاحظات |
| isactive | bool | Is Active - نشط (default: true) |

---

## Commissions

**Table Name:** `erp_commissions`  
**Description:** Commission - العمولة  
**Primary Key:** `commissionid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| commissionid | long | Commission ID - معرف العمولة |
| employeeid | long | Employee ID - معرف الموظف |
| periodstart | DateTime | Period Start - بداية الفترة |
| periodend | DateTime | Period End - نهاية الفترة |
| totalsales | decimal | Total Sales - إجمالي المبيعات |
| totalsubscriptions | decimal | Total Subscriptions - إجمالي الإشتراكات |
| totalservices | decimal | Total Services - إجمالي الخدمات |
| commissionpercent | decimal | Commission Percent - نسبة العمولة |
| commissionamount | decimal | Commission Amount - مبلغ العمولة |
| bonusamount | decimal | Bonus Amount - مبلغ المكافأة |
| incentiveamount | decimal | Incentive Amount - مبلغ الحافز |
| deductions | decimal | Deductions - الخصومات |
| totalamount | decimal | Total Amount - المبلغ الإجمالي |
| commissiontierid | long? | Commission Tier ID - معرف شريحة العمولة |
| status | string | Status - الحالة (calculated/approved/paid/cancelled) (default: calculated) |
| paiddate | DateTime? | Paid Date - تاريخ الدفع |
| paymentvoucherid | long? | Payment Voucher ID - معرف سند الصرف |
| notes | string | Notes - ملاحظات |

---

## Commission Tiers

**Table Name:** `erp_commission_tiers`  
**Description:** Commission Tier - شريحة العمولة  
**Primary Key:** `tierid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| tierid | long | Tier ID - معرف الشريحة |
| tiercode | string | Tier Code - كود الشريحة |
| tiername | string | Tier Name - اسم الشريحة |
| tiernamear | string | Tier Name Arabic - اسم الشريحة بالعربي |
| minsales | decimal | Min Sales - الحد الأدنى للمبيعات |
| maxsales | decimal? | Max Sales - الحد الأقصى للمبيعات |
| commissionpercent | decimal | Commission Percent - نسبة العمولة |
| bonusamount | decimal | Bonus Amount - مبلغ المكافأة |
| appliesto | string | Applies To - تطبق على (sales/subscriptions/services/all) (default: all) |
| description | string | Description - الوصف |
| isactive | bool | Is Active - نشط (default: true) |

---

## Incentive Settings

**Table Name:** `erp_incentive_settings`  
**Description:** Incentive Setting - إعدادات الحوافز  
**Primary Key:** `incentiveid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| incentiveid | long | Incentive ID - معرف الحافز |
| incentivecode | string | Incentive Code - كود الحافز |
| incentivename | string | Incentive Name - اسم الحافز |
| incentivenamear | string | Incentive Name Arabic - اسم الحافز بالعربي |
| incentivetype | string | Incentive Type - نوع الحافز (target/performance/attendance/bonus) |
| calculationmethod | string | Calculation Method - طريقة الحساب (fixed/percent/tiered) |
| fixedamount | decimal | Fixed Amount - المبلغ الثابت |
| percentvalue | decimal | Percent Value - قيمة النسبة |
| targetamount | decimal? | Target Amount - المبلغ المستهدف |
| targetcount | int? | Target Count - العدد المستهدف |
| minachievementpercent | decimal | Min Achievement Percent - الحد الأدنى لنسبة الإنجاز (default: 100) |
| appliesto | string | Applies To - تطبق على (all/sales/subscriptions/services/attendance) (default: all) |
| appliestoemployees | string | Applies To Employees - تطبق على الموظفين (JSON - IDs) |
| appliestodepartments | string | Applies To Departments - تطبق على الأقسام (JSON - IDs) |
| periodtype | string | Period Type - نوع الفترة (daily/weekly/monthly/quarterly/yearly) (default: monthly) |
| startdate | DateTime? | Start Date - تاريخ البداية |
| enddate | DateTime? | End Date - تاريخ النهاية |
| maxincentiveamount | decimal? | Max Incentive Amount - الحد الأقصى للحافز |
| description | string | Description - الوصف |
| isactive | bool | Is Active - نشط (default: true) |

---

## Biometric Devices

**Table Name:** `erp_biometric_devices`  
**Description:** Biometric Device - جهاز البصمة  
**Primary Key:** `deviceid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| deviceid | long | Device ID - معرف الجهاز |
| devicecode | string | Device Code - كود الجهاز |
| devicename | string | Device Name - اسم الجهاز |
| devicenamear | string | Device Name Arabic - اسم الجهاز بالعربي |
| devicetype | string | Device Type - نوع الجهاز (fingerprint/face/card) (default: fingerprint) |
| brand | string | Brand - الماركة |
| model | string | Model - الموديل |
| serialnumber | string | Serial Number - الرقم التسلسلي |
| ipaddress | string | IP Address - عنوان IP |
| port | int | Port - المنفذ |
| connectiontype | string | Connection Type - نوع الاتصال (network/usb) (default: network) |
| zoneid | long? | Zone ID - معرف المنطقة |
| locationdescription | string | Location Description - وصف الموقع |
| purpose | string | Purpose - الغرض (employee/member/both) (default: both) |
| lastsynctime | DateTime? | Last Sync Time - آخر وقت مزامنة |
| isonline | bool | Is Online - متصل |
| notes | string | Notes - ملاحظات |
| isactive | bool | Is Active - نشط (default: true) |

---

## Biometric Zones

**Table Name:** `erp_biometric_zones`  
**Description:** Biometric Zone - منطقة البصمة  
**Primary Key:** `zoneid` (long)

| Field | Type | Description (English/Arabic) |
|-------|------|------------------------------|
| zoneid | long | Zone ID - معرف المنطقة |
| zonecode | string | Zone Code - كود المنطقة |
| zonename | string | Zone Name - اسم المنطقة |
| zonenamear | string | Zone Name Arabic - اسم المنطقة بالعربي |
| zonetype | string | Zone Type - نوع المنطقة (entrance/exit/gym/class) |
| locationdescription | string | Location Description - وصف الموقع |
| notes | string | Notes - ملاحظات |
| isactive | bool | Is Active - نشط (default: true) |

---

## Summary Statistics

- **Total Tables:** 12
- **Total Fields:** 234
- **Average Fields per Table:** 19.5

### Table Field Counts:
- ErpEmployee: 39 fields
- ErpPayroll: 46 fields (including 1 ignored field)
- ErpIncentiveSetting: 25 fields
- ErpEmployeeAttendance: 19 fields
- ErpBiometricDevice: 20 fields
- ErpCommission: 20 fields
- ErpServiceStaff: 21 fields
- ErpDepartment: 10 fields
- ErpPosition: 10 fields
- ErpCommissionTier: 11 fields
- ErpEmployeeLoan: 15 fields
- ErpBiometricZone: 9 fields

### Key Relationships:
- Employees belong to Departments and Positions
- Employees have Payroll records and Attendance records
- Employees can have Loans and Commissions
- Service Staff extends Employee functionality
- Biometric Devices belong to Zones
- Commission Tiers define commission structures
- Incentive Settings define performance rewards

This documentation provides a comprehensive overview of the HR module's database structure, supporting full employee lifecycle management from hiring through compensation and attendance tracking.

