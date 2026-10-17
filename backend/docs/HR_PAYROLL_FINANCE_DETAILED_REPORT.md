# HR Payroll & Loans — Detailed Front-end/Back-end Analysis + Finance Integration

**Repository-based report (no external assumptions).**  
This report is derived strictly from:
- Front-end pages/services under `Internal_Event_Management_System/apps/isomorphic-intl/`
- Back-end controllers/services/models under `mahamBackendNET/backend/`
- Updated table docs:
  - `ZR.Model/HR/docs/PAYROLL_MODULE_TABLES.md`
  - `ZR.Model/ERP/Finance/FINANCE_TABLES_DOCUMENTATION.md`
  - `ZR.Model/ERP/HR/HR_TABLES_DOCUMENTATION.md` (legacy/parallel ERP HR schema)

> **Important note about schemas**: the running HR API implementation in this repo uses `hr_*` tables (`hr_payroll`, `hr_loan`, `hr_loan_repayment`, ...). The repo also contains an ERP HR documentation file describing `erp_*` tables (e.g., `erp_payroll`, `erp_employee_loans`). Treat `erp_*` as legacy/parallel unless your deployment is actually wired to it.

---

## Front-end Analysis (HR Payroll, Salary Data, Loans)

### Payroll UI/UX flows (implemented)

**Primary routes**
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/payroll/page.tsx`
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/payroll/process/page.tsx`
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/payroll/[periodId]/page.tsx`
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/payroll/employee/[employeeId]/page.tsx`

**Service layer**
- `apps/isomorphic-intl/src/services/payrollService.ts` (BASE: `/api/hr/payroll`)
- Hooks: `apps/isomorphic-intl/src/lib/hooks/use-hr.ts`

#### 1) Payroll list (`/hr/payroll`)
- **Filters**
  - PayPeriod (free text)
  - Status (All/Draft/Approved/Paid)
  - Pagination (PageNum/PageSize)
- **Actions**
  - **Process Payroll** → navigates to `/hr/payroll/process`
  - **Export** → calls `payrollService.exportPayroll(...)` (download)
- **Displayed fields**
  - PayrollId, EmployeeId, PayPeriod, NetSalary, APInvoiceId (truncated), Status

#### 2) Process payroll (`/hr/payroll/process`)
- Builds `payPeriod` as `YYYY-MM` from year+month selectors.
- Loads employees using `useEmployees({ Status: 'active', PageSize: 500 })`.
- Multi-select employees and submit:
  - Calls `useProcessPayroll()` → `POST /api/hr/payroll/bulk-process` with payload:
    - `payPeriod`
    - `employeeIds`
    - `autoApprove` (currently set `false` in UI)

#### 3) Period view (`/hr/payroll/[periodId]`)
- Fetches summary via `GET /api/hr/payroll/summary?payPeriod=...`
- Fetches payroll records via `GET /api/hr/payroll/list?PayPeriod=...`
- Per record: **Payslip download** via `GET /api/hr/payroll/{payrollId}/payslip`

#### 4) Employee payroll history (`/hr/payroll/employee/[employeeId]`)
- Uses payroll list API with `employeeId` filter + payment date range.
- Provides payslip download per row.

---

### Loans UI/UX flow (implemented)

**Route**
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/loans/page.tsx`

**API wrapper**
- `apps/isomorphic-intl/src/app/shared/hr/api.ts` → `hrLoanApi`

**User flow**
- List loans with filters:
  - employeeId, loanType, status
- Create (Apply) loan:
  - Modal form fields:
    - employeeId, loanType, amount, interestRate, repaymentMonths, documents
  - Submits to `hrLoanApi.apply` (POST)
- Edit:
  - Fetches `hrLoanApi.get(loanId)` then `hrLoanApi.update(loanId, payload)`
- Approve/Reject:
  - Buttons shown when UI sees status `"Pending"` (capitalized)
  - Calls:
    - `POST /api/hr/loans/{loanId}/approve`
    - `POST /api/hr/loans/{loanId}/reject`
  - **These do not match the back-end routes (see Consistency section).**

**Documents**
- Front-end stores document path(s) under a `documents` string and sends it to the API.
- Back-end model uses `HrLoan.AttachmentUrl` (string, semicolon-delimited list in service logic).

---

### Employee financial data UI (bank + payroll GL account)

**Primary employee UI**
- `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/hr/employees/page.tsx`
- Modal: `apps/isomorphic-intl/src/app/shared/employees/employee-modal.tsx`

**Financial data handled in UI**
- Bank data:
  - BankName, BankAccount, IBAN
- Finance mapping:
  - `glAccountId` (Payroll GL Account) stored as GUID string

**Notably missing from UI**
- No employee salary assignment fields (BasicSalary/Housing/Transport/Other) are exposed in the live employee modal flow.

**Mock-only employee pages (not wired to back-end)**
- `apps/isomorphic-intl/src/app/shared/hr/employees/create.tsx`
- `apps/isomorphic-intl/src/app/shared/hr/employees/edit.tsx`
- These contain placeholder forms and do not submit to the real employee APIs.

---

## Back-end Analysis (Payroll, Salary Assignment, Loans, Computations)

### HR Payroll database tables (source: `ZR.Model/HR/docs/PAYROLL_MODULE_TABLES.md`)

**Payroll**
- `hr_payroll`: stores monthly payroll outcomes per employee per `PayPeriod`
- `hr_payroll_detail`: stores component-level details (earnings/deductions) and totals

**Loans**
- `hr_loan`: loan master
- `hr_loan_repayment`: repayment transactions per pay period (or manual payments)

**Compliance**
- `hr_salary_compliance`: holds country rules like `Minimumwage`, `Overtimeratemultiplier`, `Taxbrackets`, etc.

---

### Payroll API endpoints (controller)
Controller: `ZR.Admin.WebApi/Controllers/HR/PayrollController.cs`  
Base route: `api/hr/payroll`

Endpoints include:
- `GET  list` → `GetPayrollList(PayrollQueryDto)`
- `POST calculate` → `CalculatePayroll(PayrollCalculationDto)`
- `POST bulk-process` → `ProcessBulkPayroll(BulkPayrollDto)`
- `POST {payrollId}/approve` → `ApprovePayroll`
- `POST {payrollId}/payment` → `ProcessPayment(PaymentDto)`
- `GET  {payrollId}/payslip` → `GeneratePayslip` (file response)
- `GET  {payrollId}/payslip-details` → `GetPayslipDetails`
- `GET  gosi-report` → `GetGOSIReport(payPeriod)`
- `POST bank-transfer` → `GenerateBankTransfer(BankTransferRequestDto)`
- `GET  summary` → `GetPayrollSummary(payPeriod, departmentId?)`
- `GET  statistics` → `GetPayrollStatistics(payPeriod)`
- `GET  export` → `ExportPayroll(PayrollQueryDto)`

---

### Payroll service logic (actual computation path used by API)
Service: `ZR.Service/HR/Payroll/PayrollService.cs` (implements `IPayrollService`)

#### 1) Payroll calculation inputs used
- Employee record: `HrEmployee`
- Attendance records: `HrAttendance` (overtime, late minutes, presence)
- Leave records: `HrLeaveRequest` + `HrLeaveType` (for leave deduction)
- **Salary basis source**:
  - Current implementation uses `GetLatestPayrollSnapshotAsync(employeeId)` from `hr_payroll` to derive:
    - `BasicSalary`, `HousingAllowance`, `TransportAllowance`, `OtherAllowances`
  - This means first payroll with no historical snapshot will default salary components to 0.

#### 2) Computation model (as implemented)
- **Period boundaries**: derived from `payPeriod` (YYYY-MM) → start=1st, end=last day
- **Daily rate**: `basicSalary / totalDaysInMonth` (calendar days)
- **Late deduction**: `(totalLateMinutes/60) * (dailyRate/8)`
- **Overtime pay**: `overtimeHours * (dailyRate/8) * 1.5`
- **Leave deduction**: `unpaidLeaveDays * dailyRate`  
  > There is a likely logic error in unpaid leave detection (see Issues).
- **Gross**:
  - `basic + housing + transport + other + overtime + bonus + commission`
  - Bonus/commission are currently placeholder functions returning 0.
- **GOSI**
  - Computed on `(basic + housing)`
  - Saudi: employee 10%, employer 12
  - Non-Saudi: employee 2%, employer 2
- **Tax**
  - Placeholder implementation returns 0 in this service.
- **Net**
  - `net = gross - (deductions(leave+late) + gosiEmployee + tax)`

#### 3) Persistence
- Writes to `hr_payroll` (insert or update)
- Does **not** write `hr_payroll_detail` rows (components), despite table existing.

---

### Loan API endpoints (controller)
Controller: `ZR.Admin.WebApi/Controllers/HR/LoanController.cs`  
Base route: `api/hr/loans`

Key endpoints include:
- `GET list`
- `GET {loanId}`
- `POST apply`
- `PUT {loanId}`
- `POST approve` (**body-based**) — not `/{loanId}/approve`
- `POST reject` (**body-based**) — not `/{loanId}/reject`
- `POST repayments` (manual repayment)
- `POST repayments/process-monthly` (auto salary deduction processing by pay period)
- `GET {loanId}/outstanding-balance`, `GET {loanId}/monthly-repayment`
- `POST {loanId}/recalculate-schedule`
- `GET/POST early settlement` endpoints
- `POST export` (body-based export)

---

### Loan service logic
Service: `ZR.Service/HR/Loans/LoanService.cs`

Key behaviors:
- **Apply**
  - Validates employee and eligibility rules
  - Calculates `MonthlyDeduction` using amortization formula if interest > 0
  - Sets:
    - `Status = "pending"`
    - `OutstandingAmount = LoanAmount`
    - `IsActive = true`
- **Approve**
  - Allows only pending loans
  - Sets:
    - `Status = "active"`
    - `StartDeductionDate` to first day of next month
    - `EndDeductionDate` based on repayment months
- **Monthly processing**
  - For each active loan, creates a `hr_loan_repayment` row with `PaymentMethod = "salary_deduction"`
  - Updates loan balances and marks loan completed when repaid.

---

## Consistency and Matching Between Front-end and Back-end

### A) Endpoint mismatches (high impact)

#### Loan approve/reject mismatch
- Front-end calls:
  - `POST /api/hr/loans/{loanId}/approve`
  - `POST /api/hr/loans/{loanId}/reject`
- Back-end exposes:
  - `POST /api/hr/loans/approve` (expects body DTO containing LoanId)
  - `POST /api/hr/loans/reject` (expects body DTO containing LoanId)

**Impact**
- Loans UI approval/rejection will fail (404) or never reach the controller action.

---

### B) Status casing / value inconsistencies

#### Loan status casing
- Loans UI uses capitalized labels: `Pending`, `Approved`, `Active`, `Completed`, `Rejected`
- Loan service uses lowercase: `pending`, `active`, `completed`, `rejected`
- Loan controller currently sets `loan.Status = "PENDING"` on apply.

**Impact**
- Approval rules (`status == "pending"`) can fail.
- UI conditional rendering (buttons) can fail or misbehave.
- Filters can mismatch (UI sends `Pending`, DB stores `pending` or `PENDING`).

---

### C) Download/export method mismatches

#### Loans export
- Back-end: `POST /api/hr/loans/export` (body DTO)
- Front-end: `download(...)` defaults to GET in `hrLoanApi.export`

#### Payroll bank transfer
- Back-end: `POST /api/hr/payroll/bank-transfer` (body)
- Front-end: `payrollService.exportBankTransfer` uses `download(...)` as GET query

#### Payroll GOSI report
- Back-end: `GET /api/hr/payroll/gosi-report` returns JSON DTO (not file)
- Front-end: attempts to download `.xlsx`

---

### D) Data/algorithm mismatches (financial correctness)

#### Loan deductions are not part of payroll net
- HR DB tables include `hr_payroll_detail.LoanDeduction` and ERP HR docs include `erp_payroll.loandeduction`
- But the payroll service used by the API does not subtract loan deductions from net salary.

**Impact**
- Net salary displayed and used for transfers/AP invoices can be overstated.
- Repayments may exist in `hr_loan_repayment` but payroll ignores them, creating reconciliation issues.

---

## Integration with Finance Module (grounded in `FINANCE_TABLES_DOCUMENTATION.md`)

### A) Available finance constructs to integrate with

From `ZR.Model/ERP/Finance/FINANCE_TABLES_DOCUMENTATION.md`:
- **AP invoices**
  - `arap_ap_invoice` (APInvoice) includes:
    - `Id`, `InvoiceNumber`, `VendorId`, `TotalAmount`, `Status`, `GLJournalId`, `FiscalYearId`, `FiscalPeriodId`
  - `arap_ap_invoice_line` includes:
    - `InvoiceId`, `AccountId (GLAccount FK)`, `TaxCodeId`, `CostCenterId`, `DepartmentId`, ...
- **GL**
  - `gl_journal` (GLJournal) + `gl_journal_entry` (GLJournalEntry)
- **Tax**
  - `tax_code`, `tax_group`, etc.
- **Treasury**
  - `bank_account` includes `IBAN`, `GLAccountId`, currency fields
  - `treasury_balance_ledger` provides immutable balance audit
- **Payment Hub**
  - `hub_payment` and allocations (unified payment processing)
- **ZATCA**
  - `zatca_invoice_hash` (primarily used for AR/AP invoicing chains)

---

### B) What your HR code already implements (partial)

Your HR payroll model includes:
- `HrPayroll.APInvoiceId` (intended link to `arap_ap_invoice.Id`)
- `HrPayroll.CurrencyId` (to `currencies.Id`) and `ExchangeRate`

Your payroll service already contains logic to:
- Ensure a vendor `PAYROLL_CLEARING` exists
- Create an `APInvoice` draft and link it to payroll
- Create a `HrFinanceReconciliation` record (HR-side)

---

### C) Recommended HR→Finance posting design (safe, auditable, and aligned with your tables)

#### Option 1 (recommended with your existing fields): AP-led payroll clearing
1. **Approve payroll in HR** (`hr_payroll.Status = approved`)
2. Create a single **APInvoice** per pay period (or per company/branch/department), not one per employee:
   - `arap_ap_invoice.TotalAmount = sum(NetSalary)`
   - `arap_ap_invoice.ReferenceNumber = "HR:PAYROLL:{payPeriod}"`
3. Create `arap_ap_invoice_line` lines:
   - Salary expense lines:
     - `AccountId` = employee or department payroll expense account (`HrEmployee.GLAccountId`)
     - `DepartmentId` / `CostCenterId` (if you maintain these mappings)
   - Liability line(s) can be handled through posting rules or as separate invoice lines depending on Finance design.
4. Post invoice to GL:
   - Finance sets `arap_ap_invoice.GLJournalId`
   - GL contains balanced `gl_journal_entry` rows
5. Pay AP invoice:
   - Use `hub_payment` (Payment Hub) to create payment and allocations to APInvoice
   - Treasury integration uses `bank_account` and writes `treasury_balance_ledger`
6. Link HR records:
   - Store APInvoiceId on payroll header, and optionally store payment ids/treasury transaction ids for traceability.

#### Option 2: GL-led direct payroll posting
1. On payroll approval, create one `gl_journal` + multiple `gl_journal_entry` lines:
   - Debits: salary expense, employer contributions
   - Credits: payroll payable, GOSI payable, etc.
2. Use Treasury/Payment Hub to settle payroll payable.

**Why Option 1 fits this repo**: the payroll model already carries `APInvoiceId`, and Finance tables already support `APInvoice.GLJournalId` and Payment allocation.

---

### D) Loans integration with Finance

Use `hr_loan_repayment` as source of repayment events and post:
- Principal portion → reduces loan receivable (asset account)
- Interest portion → interest income/expense (depends on policy; typically income if company is lender)
- Counter account:
  - If repayment is salary deduction, it should net-off against payroll payable in the same pay period
  - If repayment is bank/cash, use Treasury/Payment Hub accounts

Your current service creates only a `gl_journal` header for repayment and does not create `gl_journal_entry` lines — this must be completed to be financially valid.

---

## Identification of Incorrect Points (Bugs / Inaccuracies / Gaps)

### High severity (financial correctness / system breaks)

- **Loans approve/reject endpoints mismatch**
  - FE expects `POST /api/hr/loans/{loanId}/approve|reject`
  - BE exposes `POST /api/hr/loans/approve|reject` (body DTO)
  - **Effect**: approvals/rejections broken from UI

- **Loan status casing mismatch**
  - Controller sets `"PENDING"`, service expects `"pending"`, UI shows `"Pending"`
  - **Effect**: approval rules and UI conditions break; filters unreliable

- **Loan deductions not included in payroll net**
  - Payroll tables include `LoanDeduction` field (`hr_payroll_detail.LoanDeduction`)
  - Payroll compute path does not subtract it
  - **Effect**: net salary overstated; bank transfers and AP invoices wrong

- **Salary source-of-truth is unstable**
  - Payroll compute pulls salary components from *previous* `hr_payroll` snapshot, not from employee salary fields
  - **Effect**: new employee payroll can compute as 0; salary assignment process unclear

- **Potential leave deduction logic error**
  - Leave paid/unpaid check appears inverted in service logic (requires verification against `hr_leave_type.Ispaid`)
  - **Effect**: over/under-deduction

- **Loan repayment GL posting incomplete**
  - Service creates `gl_journal` but no `gl_journal_entry` rows
  - **Effect**: journal is not balanced; no real accounting impact

### Medium severity (broken exports, misleading outputs)
- Payroll bank-transfer export mismatch (FE uses GET download; BE is POST returning DTO)
- Payroll GOSI report mismatch (FE expects file; BE returns DTO)
- Payslip endpoint returns placeholder bytes while declaring `application/pdf`

### Security/compliance concerns
- Employee bank details (IBAN/account) stored and transmitted as plain strings:
  - Risk of exposure via logs, exports, unauthorized access
  - Recommend: field-level encryption/masking + strict permissions + audit trails

---

## Recommended Fix Plan (implementation-oriented)

### 1) Normalize loan workflow (fast)
- Make routes consistent:
  - Either update FE to call `POST /api/hr/loans/approve` and `POST /api/hr/loans/reject` with `{ loanId, ... }`
  - Or add new BE endpoints `POST /api/hr/loans/{loanId}/approve|reject` that adapt to the existing service
- Standardize status values (single canonical set, e.g., lower-case):
  - pending/active/completed/rejected/cancelled
  - Enforce via constants or enum

### 2) Make payroll net salary financially correct
- Incorporate loan deduction:
  - Aggregate active loans’ monthly deductions or repayments per `payPeriod`
  - Persist `LoanDeduction` and subtract from NetSalary
- Populate `hr_payroll_detail`:
  - Create rows for each component (basic, housing, overtime, leave deduction, GOSI, loan deduction, etc.)

### 3) Establish salary source-of-truth
Choose one:
- **Employee salary fields** (`HrEmployee.BasicSalary`, allowances) + salary history table
- Or a dedicated compensation table (preferred for audit)
Then modify payroll computation to use that source, not previous payroll snapshot.

### 4) Complete HR→Finance integration
Using Finance tables:
- Create APInvoice (`arap_ap_invoice`) at payroll approval (preferably per period, not per employee)
- Create APInvoiceLine (`arap_ap_invoice_line`) mapped to GL accounts
- Post to GL: `gl_journal` + `gl_journal_entry`
- Pay via Payment Hub: `hub_payment` + `hub_payment_allocation`
- Treasury settlement uses `bank_account` and writes `treasury_balance_ledger`

---

## Appendix — Table/Field Cross-Reference (HR ↔ Finance)

### HR tables (Payroll module docs)
- `hr_payroll`: `NetSalary`, `PaymentDate`, `PaymentMethod`, `BankReference`, `Status`
- `hr_payroll_detail`: `LoanDeduction`, `TaxCodeId`, `ZatcaHashId`
- `hr_loan`: `MonthlyDeduction`, `OutstandingAmount`, `Status`, `GLJournalId`, `PaymentHubTransactionId`
- `hr_loan_repayment`: `PayPeriod`, `PrincipalAmount`, `InterestAmount`, `GLJournalId`, `PaymentHubTransactionId`

### Finance tables (Finance tables doc)
- `arap_ap_invoice`: `TotalAmount`, `Status`, `GLJournalId`, `FiscalYearId`, `FiscalPeriodId`
- `arap_ap_invoice_line`: `AccountId` (GLAccount FK), `TaxCodeId`, `CostCenterId`, `DepartmentId`
- `gl_journal`, `gl_journal_entry`
- `hub_payment`, `hub_payment_allocation`
- `bank_account`, `treasury_balance_ledger`














