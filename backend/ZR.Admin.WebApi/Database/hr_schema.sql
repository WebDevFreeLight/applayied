-- HR Management System Database Schema
-- إدارة الموارد البشرية

-- ====================================
-- 1. Employee Table (الموظفون)
-- ====================================
CREATE TABLE hr_employee (
    employee_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_code NVARCHAR(50) NOT NULL UNIQUE,
    name NVARCHAR(100) NOT NULL,
    name_ar NVARCHAR(100),
    email NVARCHAR(100),
    phone NVARCHAR(20),
    department_id BIGINT,
    position_id BIGINT,
    manager_id BIGINT,
    join_date DATETIME,
    status NVARCHAR(20) DEFAULT 'active', -- active, inactive, terminated
    employment_type NVARCHAR(20), -- full-time, part-time, contract
    nationality NVARCHAR(50),
    id_number NVARCHAR(50), -- ID/Iqama Number
    birth_date DATETIME,
    gender NVARCHAR(10),
    marital_status NVARCHAR(20),
    address NVARCHAR(500),
    emergency_contact NVARCHAR(100),
    emergency_phone NVARCHAR(20),
    bank_account NVARCHAR(50),
    iban NVARCHAR(50),
    basic_salary DECIMAL(10,2),
    allowances DECIMAL(10,2),
    biometric_id NVARCHAR(50),
    work_location NVARCHAR(100),
    contract_start_date DATETIME,
    contract_end_date DATETIME,
    gosi_number NVARCHAR(50),
    medical_insurance_number NVARCHAR(50),
    remark NVARCHAR(1000),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (department_id) REFERENCES sys_dept(dept_id),
    FOREIGN KEY (manager_id) REFERENCES hr_employee(employee_id)
);

-- ====================================
-- 2. Attendance Tables (الحضور والانصراف)
-- ====================================

-- Attendance Shifts (المناوبات)
CREATE TABLE hr_attendance_shift (
    shift_id BIGINT PRIMARY KEY IDENTITY(1,1),
    shift_code NVARCHAR(50) NOT NULL UNIQUE,
    shift_name NVARCHAR(100) NOT NULL,
    shift_name_ar NVARCHAR(100),
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    break_start_time TIME,
    break_end_time TIME,
    allow_late_minutes INT DEFAULT 0,
    allow_early_minutes INT DEFAULT 0,
    is_overnight BIT DEFAULT 0,
    is_active BIT DEFAULT 1,
    remark NVARCHAR(500),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME
);

-- Attendance Records (سجلات الحضور)
CREATE TABLE hr_attendance (
    attendance_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    attendance_date DATE NOT NULL,
    shift_id BIGINT,
    check_in_time DATETIME,
    check_out_time DATETIME,
    work_hours DECIMAL(5,2),
    overtime_hours DECIMAL(5,2),
    late_minutes INT,
    early_leave_minutes INT,
    status NVARCHAR(20), -- present, absent, late, early, holiday, leave
    is_abnormal BIT DEFAULT 0,
    abnormal_reason NVARCHAR(500),
    check_in_device_id BIGINT,
    check_out_device_id BIGINT,
    check_in_location NVARCHAR(200),
    check_out_location NVARCHAR(200),
    remark NVARCHAR(500),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (shift_id) REFERENCES hr_attendance_shift(shift_id),
    UNIQUE (employee_id, attendance_date)
);

-- Attendance Logs (سجلات البصمة)
CREATE TABLE hr_attendance_log (
    log_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    biometric_id NVARCHAR(50),
    device_id BIGINT,
    log_time DATETIME NOT NULL,
    log_type NVARCHAR(20), -- check_in, check_out, break_start, break_end
    verification_method NVARCHAR(20), -- fingerprint, face, card, password
    device_name NVARCHAR(100),
    location NVARCHAR(200),
    temperature DECIMAL(3,1),
    is_processed BIT DEFAULT 0,
    processed_time DATETIME,
    raw_data NVARCHAR(1000),
    create_time DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id)
);

-- Attendance Devices (أجهزة البصمة)
CREATE TABLE hr_attendance_device (
    device_id BIGINT PRIMARY KEY IDENTITY(1,1),
    device_code NVARCHAR(50) NOT NULL UNIQUE,
    device_name NVARCHAR(100) NOT NULL,
    device_type NVARCHAR(20), -- fingerprint, face, card
    brand NVARCHAR(50),
    model NVARCHAR(50),
    ip_address NVARCHAR(50),
    port INT,
    location NVARCHAR(200),
    last_sync_time DATETIME,
    is_online BIT DEFAULT 0,
    is_active BIT DEFAULT 1,
    remark NVARCHAR(500),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME
);

-- ====================================
-- 3. Leave Management (الإجازات)
-- ====================================

-- Leave Types (أنواع الإجازات)
CREATE TABLE hr_leave_type (
    leave_type_id BIGINT PRIMARY KEY IDENTITY(1,1),
    type_code NVARCHAR(50) NOT NULL UNIQUE,
    type_name NVARCHAR(100) NOT NULL,
    type_name_ar NVARCHAR(100),
    days_per_year DECIMAL(5,2),
    is_paid BIT DEFAULT 1,
    requires_approval BIT DEFAULT 1,
    requires_document BIT DEFAULT 0,
    min_days INT DEFAULT 1,
    max_days INT,
    advance_notice_days INT DEFAULT 0,
    is_active BIT DEFAULT 1,
    remark NVARCHAR(500),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME
);

-- Leave Requests (طلبات الإجازة)
CREATE TABLE hr_leave_request (
    request_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    leave_type_id BIGINT NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    days DECIMAL(5,2) NOT NULL,
    reason NVARCHAR(500),
    status NVARCHAR(20) DEFAULT 'pending', -- pending, approved, rejected, cancelled
    approver_id BIGINT,
    approve_date DATETIME,
    approve_comments NVARCHAR(500),
    attachment_url NVARCHAR(500),
    is_emergency BIT DEFAULT 0,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (leave_type_id) REFERENCES hr_leave_type(leave_type_id),
    FOREIGN KEY (approver_id) REFERENCES hr_employee(employee_id)
);

-- Leave Balance (رصيد الإجازات)
CREATE TABLE hr_leave_balance (
    balance_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    leave_type_id BIGINT NOT NULL,
    year INT NOT NULL,
    entitled_days DECIMAL(5,2),
    used_days DECIMAL(5,2) DEFAULT 0,
    balance DECIMAL(5,2),
    carried_over DECIMAL(5,2) DEFAULT 0,
    expired_days DECIMAL(5,2) DEFAULT 0,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (leave_type_id) REFERENCES hr_leave_type(leave_type_id),
    UNIQUE (employee_id, leave_type_id, year)
);

-- Holidays (العطلات الرسمية)
CREATE TABLE hr_holiday (
    holiday_id BIGINT PRIMARY KEY IDENTITY(1,1),
    holiday_date DATE NOT NULL,
    holiday_name NVARCHAR(100) NOT NULL,
    holiday_name_ar NVARCHAR(100),
    holiday_type NVARCHAR(20), -- national, religious, company
    is_paid BIT DEFAULT 1,
    year INT NOT NULL,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    UNIQUE (holiday_date)
);

-- ====================================
-- 4. Performance Management (الأداء)
-- ====================================

-- Performance Reviews (تقييمات الأداء)
CREATE TABLE hr_performance_review (
    review_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    reviewer_id BIGINT NOT NULL,
    review_period_start DATE NOT NULL,
    review_period_end DATE NOT NULL,
    overall_rating DECIMAL(3,2), -- 1.00 to 5.00
    productivity_rating DECIMAL(3,2),
    quality_rating DECIMAL(3,2),
    attendance_rating DECIMAL(3,2),
    teamwork_rating DECIMAL(3,2),
    leadership_rating DECIMAL(3,2),
    strengths NVARCHAR(1000),
    improvements NVARCHAR(1000),
    goals NVARCHAR(1000),
    comments NVARCHAR(1000),
    status NVARCHAR(20) DEFAULT 'draft', -- draft, submitted, approved
    review_date DATETIME,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (reviewer_id) REFERENCES hr_employee(employee_id)
);

-- Performance Goals (أهداف الأداء)
CREATE TABLE hr_performance_goal (
    goal_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    goal_title NVARCHAR(200) NOT NULL,
    goal_description NVARCHAR(1000),
    target_date DATE,
    completion_percentage INT DEFAULT 0,
    status NVARCHAR(20) DEFAULT 'active', -- active, completed, cancelled
    priority NVARCHAR(20), -- high, medium, low
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id)
);

-- ====================================
-- 5. Payroll & Compliance (الرواتب والامتثال)
-- ====================================

-- Payroll Records (سجلات الرواتب)
CREATE TABLE hr_payroll (
    payroll_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    pay_period NVARCHAR(20) NOT NULL, -- YYYY-MM
    basic_salary DECIMAL(10,2),
    housing_allowance DECIMAL(10,2),
    transport_allowance DECIMAL(10,2),
    other_allowances DECIMAL(10,2),
    overtime_pay DECIMAL(10,2),
    deductions DECIMAL(10,2),
    gosi_employee DECIMAL(10,2),
    gosi_employer DECIMAL(10,2),
    tax DECIMAL(10,2),
    net_salary DECIMAL(10,2),
    payment_date DATE,
    payment_method NVARCHAR(20), -- bank_transfer, cash, check
    bank_reference NVARCHAR(100),
    status NVARCHAR(20) DEFAULT 'pending', -- pending, processed, paid
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    UNIQUE (employee_id, pay_period)
);

-- ====================================
-- 6. Employee Documents (وثائق الموظفين)
-- ====================================

-- Document Types (أنواع المستندات)
CREATE TABLE hr_document_type (
    doc_type_id BIGINT PRIMARY KEY IDENTITY(1,1),
    type_code NVARCHAR(50) NOT NULL UNIQUE,
    type_name NVARCHAR(100) NOT NULL,
    type_name_ar NVARCHAR(100),
    is_required BIT DEFAULT 0,
    has_expiry BIT DEFAULT 0,
    reminder_days INT DEFAULT 30,
    is_active BIT DEFAULT 1,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME
);

-- Employee Documents (مستندات الموظفين)
CREATE TABLE hr_employee_document (
    document_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    doc_type_id BIGINT NOT NULL,
    document_name NVARCHAR(200) NOT NULL,
    document_number NVARCHAR(100),
    issue_date DATE,
    expiry_date DATE,
    issuing_authority NVARCHAR(200),
    file_path NVARCHAR(500),
    file_size BIGINT,
    status NVARCHAR(20) DEFAULT 'valid', -- valid, expired, pending
    is_verified BIT DEFAULT 0,
    verified_by NVARCHAR(50),
    verified_date DATETIME,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (doc_type_id) REFERENCES hr_document_type(doc_type_id)
);

-- ====================================
-- 7. Recruitment (التوظيف)
-- ====================================

-- Job Requisitions (طلبات التوظيف)
CREATE TABLE hr_job_requisition (
    requisition_id BIGINT PRIMARY KEY IDENTITY(1,1),
    job_title NVARCHAR(200) NOT NULL,
    job_title_ar NVARCHAR(200),
    department_id BIGINT,
    positions_required INT DEFAULT 1,
    job_description NVARCHAR(MAX),
    requirements NVARCHAR(MAX),
    salary_range_min DECIMAL(10,2),
    salary_range_max DECIMAL(10,2),
    employment_type NVARCHAR(20),
    status NVARCHAR(20) DEFAULT 'open', -- open, closed, on_hold
    posting_date DATE,
    closing_date DATE,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (department_id) REFERENCES sys_dept(dept_id)
);

-- Candidates (المرشحون)
CREATE TABLE hr_candidate (
    candidate_id BIGINT PRIMARY KEY IDENTITY(1,1),
    requisition_id BIGINT,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100),
    email NVARCHAR(100) NOT NULL,
    phone NVARCHAR(20),
    nationality NVARCHAR(50),
    current_location NVARCHAR(200),
    years_experience INT,
    current_salary DECIMAL(10,2),
    expected_salary DECIMAL(10,2),
    notice_period_days INT,
    cv_file_path NVARCHAR(500),
    cover_letter NVARCHAR(MAX),
    status NVARCHAR(20) DEFAULT 'new', -- new, screening, interview, offer, hired, rejected
    rating INT,
    notes NVARCHAR(MAX),
    source NVARCHAR(50), -- website, referral, agency, linkedin
    referral_employee_id BIGINT,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (requisition_id) REFERENCES hr_job_requisition(requisition_id),
    FOREIGN KEY (referral_employee_id) REFERENCES hr_employee(employee_id)
);

-- ====================================
-- 8. Hybrid Work (العمل الهجين)
-- ====================================

-- Hybrid Work Schedule (جدول العمل الهجين)
CREATE TABLE hr_hybrid_schedule (
    schedule_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    week_start_date DATE NOT NULL,
    monday_location NVARCHAR(20), -- office, home, leave
    tuesday_location NVARCHAR(20),
    wednesday_location NVARCHAR(20),
    thursday_location NVARCHAR(20),
    friday_location NVARCHAR(20),
    saturday_location NVARCHAR(20),
    sunday_location NVARCHAR(20),
    status NVARCHAR(20) DEFAULT 'pending', -- pending, approved, rejected
    approver_id BIGINT,
    approve_date DATETIME,
    comments NVARCHAR(500),
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (approver_id) REFERENCES hr_employee(employee_id),
    UNIQUE (employee_id, week_start_date)
);

-- ====================================
-- 9. Benefits (المزايا)
-- ====================================

-- Insurance Plans (خطط التأمين)
CREATE TABLE hr_insurance_plan (
    plan_id BIGINT PRIMARY KEY IDENTITY(1,1),
    plan_code NVARCHAR(50) NOT NULL UNIQUE,
    plan_name NVARCHAR(200) NOT NULL,
    plan_name_ar NVARCHAR(200),
    provider NVARCHAR(200),
    plan_type NVARCHAR(50), -- medical, dental, life
    coverage_details NVARCHAR(MAX),
    monthly_premium DECIMAL(10,2),
    employer_contribution DECIMAL(5,2), -- percentage
    employee_contribution DECIMAL(5,2), -- percentage
    is_active BIT DEFAULT 1,
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME
);

-- Employee Insurance Enrollments (تسجيلات التأمين)
CREATE TABLE hr_insurance_enrollment (
    enrollment_id BIGINT PRIMARY KEY IDENTITY(1,1),
    employee_id BIGINT NOT NULL,
    plan_id BIGINT NOT NULL,
    enrollment_date DATE NOT NULL,
    termination_date DATE,
    coverage_amount DECIMAL(10,2),
    monthly_premium DECIMAL(10,2),
    status NVARCHAR(20) DEFAULT 'active', -- active, terminated, pending
    create_by NVARCHAR(50),
    create_time DATETIME DEFAULT GETDATE(),
    update_by NVARCHAR(50),
    update_time DATETIME,
    FOREIGN KEY (employee_id) REFERENCES hr_employee(employee_id),
    FOREIGN KEY (plan_id) REFERENCES hr_insurance_plan(plan_id)
);

-- ====================================
-- Indexes for Performance
-- ====================================
CREATE INDEX idx_employee_dept ON hr_employee(department_id);
CREATE INDEX idx_employee_status ON hr_employee(status);
CREATE INDEX idx_attendance_date ON hr_attendance(attendance_date);
CREATE INDEX idx_attendance_employee ON hr_attendance(employee_id);
CREATE INDEX idx_leave_request_employee ON hr_leave_request(employee_id);
CREATE INDEX idx_leave_request_status ON hr_leave_request(status);
CREATE INDEX idx_payroll_period ON hr_payroll(pay_period);
CREATE INDEX idx_document_expiry ON hr_employee_document(expiry_date);
CREATE INDEX idx_candidate_status ON hr_candidate(status);