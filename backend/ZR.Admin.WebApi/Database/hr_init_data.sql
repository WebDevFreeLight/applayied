-- HR System Initialization Script with Test Data
-- نص تهيئة نظام الموارد البشرية مع بيانات الاختبار

-- ====================================
-- 1. Initialize Leave Types
-- ====================================
INSERT INTO hr_leave_type (type_code, type_name, type_name_ar, days_per_year, is_paid, requires_approval, requires_document, min_days, max_days, advance_notice_days, is_active, create_by, create_time)
VALUES 
("ANNUAL", "Annual Leave", "إجازة سنوية", 21, 1, 1, 0, 1, 21, 7, 1, "System", GETDATE()),
("SICK", "Sick Leave", "إجازة مرضية", 30, 1, 1, 1, 1, 30, 0, 1, "System", GETDATE()),
("MATERNITY", "Maternity Leave", "إجازة أمومة", 70, 1, 1, 1, 70, 70, 30, 1, "System", GETDATE()),
("PATERNITY", "Paternity Leave", "إجازة أبوة", 3, 1, 1, 0, 3, 3, 7, 1, "System", GETDATE()),
("HAJJ", "Hajj Leave", "إجازة حج", 10, 1, 1, 1, 10, 10, 30, 1, "System", GETDATE()),
("UNPAID", "Unpaid Leave", "إجازة بدون راتب", 365, 0, 1, 0, 1, 30, 14, 1, "System", GETDATE()),
("EMERGENCY", "Emergency Leave", "إجازة طارئة", 5, 1, 0, 0, 1, 3, 0, 1, "System", GETDATE()),
("STUDY", "Study Leave", "إجازة دراسية", 10, 1, 1, 1, 1, 10, 14, 1, "System", GETDATE()),
("MARRIAGE", "Marriage Leave", "إجازة زواج", 5, 1, 1, 1, 5, 5, 30, 1, "System", GETDATE()),
("BEREAVEMENT", "Bereavement Leave", "إجازة وفاة", 3, 1, 0, 1, 3, 3, 0, 1, "System", GETDATE());

-- ====================================
-- 2. Initialize Document Types
-- ====================================
INSERT INTO hr_document_type (type_code, type_name, type_name_ar, is_required, has_expiry, reminder_days, is_active, create_by, create_time)
VALUES 
("ID", "National ID", "بطاقة الهوية الوطنية", 1, 1, 90, 1, "System", GETDATE()),
("PASSPORT", "Passport", "جواز السفر", 1, 1, 180, 1, "System", GETDATE()),
("IQAMA", "Iqama", "الإقامة", 1, 1, 90, 1, "System", GETDATE()),
("DL", "Driving License", "رخصة القيادة", 0, 1, 60, 1, "System", GETDATE()),
("DEGREE", "Educational Certificate", "الشهادة العلمية", 1, 0, 0, 1, "System", GETDATE()),
("CONTRACT", "Employment Contract", "عقد العمل", 1, 1, 90, 1, "System", GETDATE()),
("MEDICAL", "Medical Certificate", "الشهادة الطبية", 1, 1, 30, 1, "System", GETDATE()),
("INSURANCE", "Insurance Card", "بطاقة التأمين", 1, 1, 60, 1, "System", GETDATE()),
("BANK", "Bank Account Details", "تفاصيل الحساب البنكي", 1, 0, 0, 1, "System", GETDATE()),
("CV", "Resume/CV", "السيرة الذاتية", 1, 0, 0, 1, "System", GETDATE());

-- ====================================
-- 3. Initialize Attendance Shifts
-- ====================================
INSERT INTO hr_attendance_shift (shift_code, shift_name, shift_name_ar, start_time, end_time, break_start_time, break_end_time, allow_late_minutes, allow_early_minutes, is_overnight, is_active, create_by, create_time)
VALUES 
("MORNING", "Morning Shift", "الوردية الصباحية", "08:00:00", "17:00:00", "12:00:00", "13:00:00", 15, 0, 0, 1, "System", GETDATE()),
("EVENING", "Evening Shift", "الوردية المسائية", "14:00:00", "23:00:00", "18:00:00", "19:00:00", 15, 0, 0, 1, "System", GETDATE()),
("NIGHT", "Night Shift", "الوردية الليلية", "22:00:00", "07:00:00", "02:00:00", "03:00:00", 15, 0, 1, 1, "System", GETDATE()),
("FLEXIBLE", "Flexible Hours", "دوام مرن", "07:00:00", "19:00:00", "12:00:00", "13:00:00", 60, 60, 0, 1, "System", GETDATE()),
("SPLIT", "Split Shift", "الوردية المقسمة", "06:00:00", "14:00:00", NULL, NULL, 10, 0, 0, 1, "System", GETDATE()),
("WEEKEND", "Weekend Shift", "وردية نهاية الأسبوع", "10:00:00", "18:00:00", "14:00:00", "14:30:00", 30, 30, 0, 1, "System", GETDATE());

-- ====================================
-- 4. Initialize Attendance Devices
-- ====================================
INSERT INTO hr_attendance_device (device_code, device_name, device_type, brand, model, ip_address, port, location, is_online, is_active, create_by, create_time)
VALUES 
("DEV001", "Main Entrance", "fingerprint", "ZKTeco", "K40", "192.168.1.100", 4370, "Building A - Main Entrance", 1, 1, "System", GETDATE()),
("DEV002", "Back Entrance", "fingerprint", "ZKTeco", "K40", "192.168.1.101", 4370, "Building A - Back Entrance", 1, 1, "System", GETDATE()),
("DEV003", "Floor 2 Entry", "face", "Hikvision", "DS-K1T671", "192.168.1.102", 8000, "Building A - Floor 2", 1, 1, "System", GETDATE()),
("DEV004", "Parking Entry", "card", "HID", "ProxPoint Plus", "192.168.1.103", 9000, "Parking Area", 1, 1, "System", GETDATE()),
("DEV005", "Building B Main", "fingerprint", "Suprema", "BioStation 2", "192.168.1.104", 4370, "Building B - Main Entrance", 1, 1, "System", GETDATE());

-- ====================================
-- 5. Initialize Test Employees
-- ====================================
DECLARE @DeptId INT = 1; -- Assuming department exists

INSERT INTO hr_employee (employee_code, name, name_ar, email, phone, department_id, position_id, manager_id, join_date, status, employment_type, nationality, id_number, birth_date, gender, marital_status, address, emergency_contact, emergency_phone, bank_account, iban, basic_salary, allowances, biometric_id, work_location, contract_start_date, contract_end_date, gosi_number, medical_insurance_number, create_by, create_time)
VALUES 
("EMP001", "Ahmed Al-Rashid", "أحمد الراشد", "ahmed.rashid@company.sa", "+966501234567", @DeptId, 1, NULL, "2020-01-15", "active", "full-time", "Saudi", "1234567890", "1985-03-20", "Male", "Married", "Riyadh, Al-Olaya District", "Mohammed Al-Rashid", "+966501234568", "1234567890123456", "SA0380000000608010167519", 15000, 3000, "BIO001", "Riyadh Office", "2020-01-15", "2025-01-14", "GOSI123456", "MED123456", "System", GETDATE()),

("EMP002", "Fatima Al-Zahrani", "فاطمة الزهراني", "fatima.zahrani@company.sa", "+966502345678", @DeptId, 2, 1, "2020-03-01", "active", "full-time", "Saudi", "2345678901", "1990-07-15", "Female", "Single", "Riyadh, Al-Malaz District", "Sara Al-Zahrani", "+966502345679", "2345678901234567", "SA0380000000608010167520", 12000, 2500, "BIO002", "Riyadh Office", "2020-03-01", "2025-02-28", "GOSI234567", "MED234567", "System", GETDATE()),

("EMP003", "John Smith", "جون سميث", "john.smith@company.sa", "+966503456789", @DeptId, 3, 1, "2021-06-15", "active", "full-time", "American", "3456789012", "1988-11-30", "Male", "Married", "Riyadh, Diplomatic Quarter", "Jane Smith", "+966503456790", "3456789012345678", "SA0380000000608010167521", 18000, 4000, "BIO003", "Riyadh Office", "2021-06-15", "2024-06-14", "GOSI345678", "MED345678", "System", GETDATE()),

("EMP004", "Maria Garcia", "ماريا غارسيا", "maria.garcia@company.sa", "+966504567890", @DeptId, 4, 2, "2021-09-01", "active", "full-time", "Spanish", "4567890123", "1992-04-25", "Female", "Single", "Riyadh, Al-Sahafa District", "Carlos Garcia", "+966504567891", "4567890123456789", "SA0380000000608010167522", 10000, 2000, "BIO004", "Riyadh Office", "2021-09-01", "2024-08-31", "GOSI456789", "MED456789", "System", GETDATE()),

("EMP005", "Ali Hassan", "علي حسن", "ali.hassan@company.sa", "+966505678901", @DeptId, 5, 2, "2022-01-10", "active", "full-time", "Egyptian", "5678901234", "1987-09-10", "Male", "Married", "Riyadh, Al-Rawdah District", "Hassan Ali", "+966505678902", "5678901234567890", "SA0380000000608010167523", 8000, 1500, "BIO005", "Riyadh Office", "2022-01-10", "2025-01-09", "GOSI567890", "MED567890", "System", GETDATE()),

("EMP006", "Sarah Johnson", "سارة جونسون", "sarah.johnson@company.sa", "+966506789012", @DeptId, 6, 3, "2022-04-15", "active", "part-time", "British", "6789012345", "1995-12-05", "Female", "Single", "Riyadh, Al-Nakheel District", "Tom Johnson", "+966506789013", "6789012345678901", "SA0380000000608010167524", 6000, 1000, "BIO006", "Riyadh Office", "2022-04-15", "2024-04-14", "GOSI678901", "MED678901", "System", GETDATE()),

("EMP007", "Khalid Al-Otaibi", "خالد العتيبي", "khalid.otaibi@company.sa", "+966507890123", @DeptId, 7, 3, "2022-07-01", "active", "full-time", "Saudi", "7890123456", "1983-06-18", "Male", "Married", "Riyadh, Al-Yasmin District", "Abdullah Al-Otaibi", "+966507890124", "7890123456789012", "SA0380000000608010167525", 20000, 5000, "BIO007", "Riyadh Office", "2022-07-01", "2025-06-30", "GOSI789012", "MED789012", "System", GETDATE()),

("EMP008", "Priya Sharma", "بريا شارما", "priya.sharma@company.sa", "+966508901234", @DeptId, 8, 4, "2022-10-15", "active", "full-time", "Indian", "8901234567", "1991-02-28", "Female", "Married", "Riyadh, Al-Aqiq District", "Raj Sharma", "+966508901235", "8901234567890123", "SA0380000000608010167526", 9000, 1800, "BIO008", "Riyadh Office", "2022-10-15", "2024-10-14", "GOSI890123", "MED890123", "System", GETDATE()),

("EMP009", "Omar Khalil", "عمر خليل", "omar.khalil@company.sa", "+966509012345", @DeptId, 9, 4, "2023-01-05", "active", "full-time", "Jordanian", "9012345678", "1989-08-12", "Male", "Single", "Riyadh, Al-Woroud District", "Khalil Omar", "+966509012346", "9012345678901234", "SA0380000000608010167527", 11000, 2200, "BIO009", "Riyadh Office", "2023-01-05", "2026-01-04", "GOSI901234", "MED901234", "System", GETDATE()),

("EMP010", "Noura Al-Qahtani", "نورة القحطاني", "noura.qahtani@company.sa", "+966510123456", @DeptId, 10, 5, "2023-04-01", "active", "full-time", "Saudi", "0123456789", "1994-05-22", "Female", "Single", "Riyadh, Al-Rabwa District", "Saleh Al-Qahtani", "+966510123457", "0123456789012345", "SA0380000000608010167528", 13000, 2600, "BIO010", "Riyadh Office", "2023-04-01", "2026-03-31", "GOSI012345", "MED012345", "System", GETDATE());

-- ====================================
-- 6. Initialize Employee Shift Assignments
-- ====================================
INSERT INTO hr_employee_shift (employee_id, shift_id, start_date, end_date, create_by, create_time)
SELECT 
    employee_id,
    1, -- Morning shift for all
    join_date,
    NULL,
    "System",
    GETDATE()
FROM hr_employee;

-- ====================================
-- 7. Initialize Leave Balances for Current Year
-- ====================================
DECLARE @CurrentYear INT = YEAR(GETDATE());

INSERT INTO hr_leave_balance (employee_id, leave_type_id, year, entitled_days, used_days, balance, carried_over, expired_days, create_by, create_time)
SELECT 
    e.employee_id,
    lt.leave_type_id,
    @CurrentYear,
    lt.days_per_year,
    0,
    lt.days_per_year,
    0,
    0,
    "System",
    GETDATE()
FROM hr_employee e
CROSS JOIN hr_leave_type lt
WHERE e.status = "active" AND lt.is_active = 1;

-- ====================================
-- 8. Initialize Sample Attendance Records (Last 30 Days)
-- ====================================
DECLARE @StartDate DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @EndDate DATE = GETDATE();
DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- Skip weekends (Friday and Saturday for Saudi Arabia)
    IF DATEPART(WEEKDAY, @CurrentDate) NOT IN (6, 7)
    BEGIN
        INSERT INTO hr_attendance (employee_id, attendance_date, shift_id, check_in_time, check_out_time, work_hours, overtime_hours, late_minutes, early_leave_minutes, status, is_abnormal, check_in_device_id, check_out_device_id, create_by, create_time)
        SELECT 
            employee_id,
            @CurrentDate,
            1, -- Morning shift
            DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 30 - 10, DATEADD(HOUR, 8, CAST(@CurrentDate AS DATETIME))), -- Random check-in between 7:50 and 8:20
            DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60 - 30, DATEADD(HOUR, 17, CAST(@CurrentDate AS DATETIME))), -- Random check-out between 16:30 and 17:30
            8 + (ABS(CHECKSUM(NEWID())) % 3), -- 8-10 hours
            CASE WHEN ABS(CHECKSUM(NEWID())) % 10 = 0 THEN ABS(CHECKSUM(NEWID())) % 3 ELSE 0 END, -- 10% chance of overtime
            CASE WHEN ABS(CHECKSUM(NEWID())) % 10 < 2 THEN ABS(CHECKSUM(NEWID())) % 30 ELSE 0 END, -- 20% chance of being late
            0,
            "present",
            0,
            1, -- Main entrance device
            1, -- Main entrance device
            "System",
            GETDATE()
        FROM hr_employee
        WHERE status = "active";
    END
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END;

-- ====================================
-- 9. Initialize Sample Leave Requests
-- ====================================
INSERT INTO hr_leave_request (employee_id, leave_type_id, start_date, end_date, days, reason, status, approver_id, approve_date, approve_comments, is_emergency, create_by, create_time)
VALUES 
(2, 1, DATEADD(DAY, 10, GETDATE()), DATEADD(DAY, 14, GETDATE()), 5, "Family vacation", "approved", 1, GETDATE(), "Approved", 0, "System", GETDATE()),
(3, 2, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -3, GETDATE()), 3, "Medical appointment", "approved", 1, DATEADD(DAY, -6, GETDATE()), "Get well soon", 0, "System", DATEADD(DAY, -7, GETDATE())),
(4, 1, DATEADD(DAY, 20, GETDATE()), DATEADD(DAY, 27, GETDATE()), 6, "Annual leave", "pending", NULL, NULL, NULL, 0, "System", GETDATE()),
(5, 7, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -2, GETDATE()), 1, "Family emergency", "approved", 2, DATEADD(DAY, -2, GETDATE()), "Emergency approved", 1, "System", DATEADD(DAY, -2, GETDATE()));

-- ====================================
-- 10. Initialize 2024 Holidays
-- ====================================
INSERT INTO hr_holiday (holiday_date, holiday_name, holiday_name_ar, holiday_type, is_paid, year, create_by, create_time)
VALUES 
("2024-02-22", "Founding Day", "يوم التأسيس", "national", 1, 2024, "System", GETDATE()),
("2024-03-11", "Start of Ramadan", "بداية شهر رمضان", "religious", 1, 2024, "System", GETDATE()),
("2024-04-10", "Eid al-Fitr", "عيد الفطر", "religious", 1, 2024, "System", GETDATE()),
("2024-04-11", "Eid al-Fitr Holiday", "إجازة عيد الفطر", "religious", 1, 2024, "System", GETDATE()),
("2024-04-12", "Eid al-Fitr Holiday", "إجازة عيد الفطر", "religious", 1, 2024, "System", GETDATE()),
("2024-06-16", "Eid al-Adha", "عيد الأضحى", "religious", 1, 2024, "System", GETDATE()),
("2024-06-17", "Eid al-Adha Holiday", "إجازة عيد الأضحى", "religious", 1, 2024, "System", GETDATE()),
("2024-06-18", "Eid al-Adha Holiday", "إجازة عيد الأضحى", "religious", 1, 2024, "System", GETDATE()),
("2024-09-23", "National Day", "اليوم الوطني", "national", 1, 2024, "System", GETDATE());

-- ====================================
-- 11. Initialize Sample Payroll Records
-- ====================================
DECLARE @PayPeriod VARCHAR(20) = FORMAT(DATEADD(MONTH, -1, GETDATE()), "yyyy-MM");

INSERT INTO hr_payroll (employee_id, pay_period, basic_salary, housing_allowance, transport_allowance, other_allowances, overtime_pay, deductions, gosi_employee, gosi_employer, tax, net_salary, payment_date, payment_method, bank_reference, status, working_days, actual_days, leave_deduction, late_deduction, create_by, create_time)
SELECT 
    employee_id,
    @PayPeriod,
    basic_salary,
    basic_salary * 0.25, -- Housing allowance 25%
    500, -- Fixed transport allowance
    allowances - (basic_salary * 0.25) - 500, -- Other allowances
    CASE WHEN ABS(CHECKSUM(NEWID())) % 5 = 0 THEN basic_salary * 0.1 ELSE 0 END, -- Random overtime
    0, -- No deductions
    basic_salary * 0.10, -- GOSI employee 10%
    basic_salary * 0.12, -- GOSI employer 12%
    0, -- No tax for most
    basic_salary + (basic_salary * 0.25) + 500 + (allowances - (basic_salary * 0.25) - 500) - (basic_salary * 0.10), -- Net calculation
    DATEADD(DAY, -5, GETDATE()), -- Payment date
    "bank_transfer",
    "REF" + CAST(ABS(CHECKSUM(NEWID())) AS VARCHAR(10)),
    "paid",
    22, -- Working days
    22, -- Actual days
    0, -- No leave deduction
    0, -- No late deduction
    "System",
    GETDATE()
FROM hr_employee
WHERE status = "active";

-- ====================================
-- 12. Initialize Performance Reviews
-- ====================================
INSERT INTO hr_performance_review (employee_id, reviewer_id, review_period_start, review_period_end, overall_rating, productivity_rating, quality_rating, attendance_rating, teamwork_rating, leadership_rating, strengths, improvements, goals, comments, status, review_date, create_by, create_time)
VALUES 
(2, 1, "2023-01-01", "2023-12-31", 4.2, 4.0, 4.5, 4.0, 4.3, 4.0, "Strong technical skills, excellent problem solver", "Could improve communication with other departments", "Lead a major project in 2024", "Overall excellent performance", "approved", "2024-01-15", "System", GETDATE()),
(3, 1, "2023-01-01", "2023-12-31", 4.5, 4.5, 4.8, 4.2, 4.5, 4.3, "Outstanding client relationships, innovative thinking", "Time management could be better", "Obtain professional certification", "Exceeds expectations in most areas", "approved", "2024-01-15", "System", GETDATE()),
(4, 2, "2023-01-01", "2023-12-31", 3.8, 3.5, 4.0, 4.0, 4.0, 3.5, "Good team player, reliable", "Needs to take more initiative", "Improve technical skills", "Meets expectations", "approved", "2024-01-16", "System", GETDATE());

-- ====================================
-- 13. Initialize Insurance Plans
-- ====================================
INSERT INTO hr_insurance_plan (plan_code, plan_name, plan_name_ar, provider, plan_type, coverage_details, monthly_premium, employer_contribution, employee_contribution, is_active, create_by, create_time)
VALUES 
("MED_BASIC", "Basic Medical Plan", "الخطة الطبية الأساسية", "Bupa Arabia", "medical", "Basic inpatient and outpatient coverage", 500, 70, 30, 1, "System", GETDATE()),
("MED_PREMIUM", "Premium Medical Plan", "الخطة الطبية المميزة", "Tawuniya", "medical", "Comprehensive medical coverage including dental and vision", 1000, 60, 40, 1, "System", GETDATE()),
("LIFE_BASIC", "Basic Life Insurance", "التأمين على الحياة الأساسي", "Allianz", "life", "2x annual salary coverage", 100, 100, 0, 1, "System", GETDATE()),
("DENTAL", "Dental Insurance", "تأمين الأسنان", "Medgulf", "dental", "Full dental coverage", 150, 50, 50, 1, "System", GETDATE());

-- ====================================
-- 14. Initialize Sample Job Requisitions
-- ====================================
INSERT INTO hr_job_requisition (job_title, job_title_ar, department_id, positions_required, job_description, requirements, salary_range_min, salary_range_max, employment_type, status, posting_date, closing_date, location, experience_level, benefits, create_by, create_time)
VALUES 
("Senior Software Developer", "مطور برمجيات أول", @DeptId, 2, "We are looking for an experienced software developer to join our team", "5+ years experience in .NET, SQL Server, Angular", 15000, 25000, "full-time", "open", GETDATE(), DATEADD(DAY, 30, GETDATE()), "Riyadh", "senior", "Medical insurance, annual leave, performance bonus", "System", GETDATE()),
("HR Specialist", "أخصائي موارد بشرية", @DeptId, 1, "HR Specialist to manage employee relations and recruitment", "3+ years HR experience, SHRM certification preferred", 8000, 12000, "full-time", "open", GETDATE(), DATEADD(DAY, 45, GETDATE()), "Riyadh", "mid", "Medical insurance, annual leave", "System", GETDATE()),
("Marketing Manager", "مدير تسويق", @DeptId, 1, "Lead our marketing initiatives and brand development", "7+ years marketing experience, MBA preferred", 18000, 30000, "full-time", "open", GETDATE(), DATEADD(DAY, 60, GETDATE()), "Riyadh", "senior", "Medical insurance, annual leave, car allowance", "System", GETDATE());

-- ====================================
-- 15. Initialize Sample Candidates
-- ====================================
INSERT INTO hr_candidate (requisition_id, first_name, last_name, email, phone, nationality, current_location, years_experience, current_salary, expected_salary, notice_period_days, status, rating, source, skills, education, create_by, create_time)
VALUES 
(1, "Mohammed", "Ali", "mohammed.ali@email.com", "+966551234567", "Saudi", "Riyadh", 6, 18000, 22000, 30, "interview", 4, "linkedin", ".NET, C#, SQL Server, Angular, Azure", "BSc Computer Science", "System", GETDATE()),
(1, "Sara", "Ahmed", "sara.ahmed@email.com", "+966552345678", "Saudi", "Jeddah", 5, 16000, 20000, 60, "screening", 3, "website", ".NET Core, React, MongoDB", "BSc Software Engineering", "System", GETDATE()),
(2, "Layla", "Hassan", "layla.hassan@email.com", "+966553456789", "Egyptian", "Riyadh", 4, 9000, 11000, 30, "offer", 5, "referral", "HR Management, Recruitment, Employee Relations", "BA Human Resources, CIPD", "System", GETDATE()),
(3, "James", "Wilson", "james.wilson@email.com", "+966554567890", "British", "Dubai", 10, 25000, 28000, 90, "new", NULL, "agency", "Digital Marketing, Brand Management, Strategy", "MBA Marketing", "System", GETDATE());

-- ====================================
-- Display Summary
-- ====================================
PRINT "HR System Initialization Complete!";
PRINT "================================";
SELECT "Employees" as [Table], COUNT(*) as [Records] FROM hr_employee
UNION ALL
SELECT "Leave Types", COUNT(*) FROM hr_leave_type
UNION ALL
SELECT "Document Types", COUNT(*) FROM hr_document_type
UNION ALL
SELECT "Attendance Shifts", COUNT(*) FROM hr_attendance_shift
UNION ALL
SELECT "Attendance Records", COUNT(*) FROM hr_attendance
UNION ALL
SELECT "Leave Requests", COUNT(*) FROM hr_leave_request
UNION ALL
SELECT "Leave Balances", COUNT(*) FROM hr_leave_balance
UNION ALL
SELECT "Holidays", COUNT(*) FROM hr_holiday
UNION ALL
SELECT "Payroll Records", COUNT(*) FROM hr_payroll
UNION ALL
SELECT "Performance Reviews", COUNT(*) FROM hr_performance_review
UNION ALL
SELECT "Insurance Plans", COUNT(*) FROM hr_insurance_plan
UNION ALL
SELECT "Job Requisitions", COUNT(*) FROM hr_job_requisition
UNION ALL
SELECT "Candidates", COUNT(*) FROM hr_candidate;