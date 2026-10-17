# ATTENDANCE MODULE - ENTERPRISE GAP ANALYSIS & REFACTORING PLAN

**Date**: 2025-12-22
**Reviewer**: Senior ERP Architect
**Scope**: HR Attendance Module - Enterprise Compliance & Audit Readiness
**Severity Scale**: 🔴 CRITICAL | 🟠 HIGH | 🟡 MEDIUM | 🟢 LOW

---

## EXECUTIVE SUMMARY

**Current State**: The attendance module demonstrates **good architectural patterns** but has **critical enterprise-grade deficiencies** that would **fail financial audit** and **cannot support multi-country operations** safely.

**Risk Assessment**:
- 🔴 **PAYROLL CORRUPTION RISK**: Medium-High
- 🔴 **AUDIT FAILURE RISK**: High
- 🔴 **LEGAL COMPLIANCE RISK**: Medium
- 🟠 **SCALABILITY RISK**: Medium

**Recommendation**: **MANDATORY REFACTORING** before production use in regulated environments.

---

## STEP 1 - ARCHITECTURAL GAP ANALYSIS

### 🔴 CRITICAL GAP #1: NO TRUE SNAPSHOT ARCHITECTURE

**Current State**:
- `HrPayrollAttendanceSummary.IsFinalized = true` acts as "pseudo-snapshot"
- **BUT**: Can be modified after finalization (no enforcement)
- **BUT**: No historical versions (delete + regenerate loses history)
- **BUT**: No snapshot metadata (policy versions, calculation engine version)

**What Goes Wrong**:
```
SCENARIO: February 2025 Payroll
1. Payroll processes summary (IsFinalized = true)
2. Payroll calculates salary based on summary
3. Employee disputes attendance
4. Manager calls RecalculateSummary()
5. ❌ SUMMARY CHANGES UNDERNEATH ALREADY-PAID PAYROLL
6. ❌ Audit cannot reconstruct "what did payroll see?"
7. ❌ Cannot explain salary calculation discrepancy
```

**Audit Failure**:
- ❌ Cannot reproduce historical calculations
- ❌ Cannot prove what data payroll used
- ❌ Cannot survive "show me Feb 2024 attendance as it appeared in March 2024"

**Financial Impact**:
- Salary overpayments/underpayments cannot be proven
- Legal disputes unwinnable
- Regulatory fines likely

**MANDATORY FIX**:
```sql
-- NEW TABLE: Attendance Day Snapshot (immutable)
CREATE TABLE hr_attendance_day_snapshot (
    snapshot_id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT NOT NULL,
    attendance_date DATE NOT NULL,
    snapshot_version INT NOT NULL, -- 1, 2, 3...
    created_at TIMESTAMP NOT NULL,
    created_by VARCHAR(100),
    -- Snapshot metadata
    policy_id BIGINT,
    policy_snapshot_json JSONB, -- Full policy at time of calculation
    shift_id BIGINT,
    shift_snapshot_json JSONB,
    calculation_engine_version VARCHAR(20), -- "v2.1.0"
    -- Calculated values (immutable)
    expected_minutes INT,
    worked_minutes INT,
    late_minutes INT,
    early_leave_minutes INT,
    overtime_minutes INT,
    is_absent BOOLEAN,
    -- Segment proofs
    segments_json JSONB, -- [{segment, in, out, worked}]
    -- Finalization
    is_finalized BOOLEAN DEFAULT false,
    finalized_at TIMESTAMP,
    finalized_by VARCHAR(100),
    finalized_for_payroll_period VARCHAR(7), -- "2025-02"
    -- Audit
    UNIQUE(employee_id, attendance_date, snapshot_version)
);

-- Payroll summary MUST reference snapshot version
ALTER TABLE hr_payroll_attendance_summary
ADD COLUMN snapshot_version_start INT,
ADD COLUMN snapshot_version_end INT;
```

**Business Rule**:
- ✅ Finalized snapshots are **IMMUTABLE FOREVER**
- ✅ Recalculations create **NEW snapshot version**
- ✅ Payroll references **specific snapshot version range**
- ✅ Audit can reconstruct **exact state at any point in time**

---

### 🔴 CRITICAL GAP #2: NO POLICY VERSIONING

**Current State**:
- `HrAttendancePolicy` table has NO effective dating
- `HrOvertimePolicy` table has NO effective dating
- Policy changes apply to **ALL future calculations**
- **CANNOT** reconstruct "what was the late policy on Jan 15, 2024?"

**What Goes Wrong**:
```
SCENARIO: Policy Change Mid-Month
1. Jan 1-15: Late policy = "30 min grace period"
2. Jan 16: Policy updated = "15 min grace period"
3. Feb 1: Recalculate Jan attendance (forgot to process)
4. ❌ Jan 1-15 now use NEW policy (15 min)
5. ❌ Historical calculations CHANGED RETROACTIVELY
6. ❌ Audit failure: "Why did Jan 5 calculation change?"
```

**Audit Failure**:
- ❌ Cannot prove which policy was active on a given date
- ❌ Cannot reproduce historical calculations
- ❌ Legal disputes: "Prove you applied the policy that existed at that time"

**MANDATORY FIX**:
```sql
-- Add temporal versioning to ALL policy tables
ALTER TABLE hr_attendance_policy
ADD COLUMN effective_from DATE NOT NULL DEFAULT '2000-01-01',
ADD COLUMN effective_to DATE DEFAULT '9999-12-31',
ADD COLUMN policy_version INT NOT NULL DEFAULT 1,
ADD COLUMN superseded_by_policy_id BIGINT REFERENCES hr_attendance_policy(policy_id),
DROP CONSTRAINT IF EXISTS hr_attendance_policy_pkey,
ADD PRIMARY KEY (policy_id, policy_version);

ALTER TABLE hr_overtime_policy
ADD COLUMN effective_from DATE NOT NULL DEFAULT '2000-01-01',
ADD COLUMN effective_to DATE DEFAULT '9999-12-31',
ADD COLUMN policy_version INT NOT NULL DEFAULT 1;

-- Attendance snapshot MUST capture policy snapshot
-- (Already in Gap #1 fix via policy_snapshot_json)
```

**Business Rule**:
- ✅ Policy changes create **NEW version**
- ✅ Old versions remain active for their effective period
- ✅ Calculations **ALWAYS** record which policy version used
- ✅ Audit can prove "Policy X was active on Date Y"

---

### 🔴 CRITICAL GAP #3: NO EVENT-DRIVEN ARCHITECTURE

**Current State**:
- Services call each other directly (synchronous)
- No event log
- No audit trail of "what triggered this recalculation?"
- Cannot replay events

**What Goes Wrong**:
```
SCENARIO: Attendance Recalculation
1. Manager approves attendance adjustment
2. Service calls RecalculateAttendance()
3. Attendance changes
4. ❌ NO RECORD OF WHY IT CHANGED
5. Audit: "Why did attendance change on Mar 15?"
6. Answer: "Uh... someone clicked a button?"
7. ❌ AUDIT FAILURE
```

**Compliance Failure**:
- ❌ Cannot prove WHY data changed
- ❌ Cannot trace who triggered change (service account ≠ real user)
- ❌ Cannot replay events to debug
- ❌ Cannot prevent duplicate processing

**MANDATORY FIX**:
```sql
-- NEW TABLE: Attendance Event Log
CREATE TABLE hr_attendance_event (
    event_id BIGSERIAL PRIMARY KEY,
    event_type VARCHAR(50) NOT NULL, -- LeaveApproved, AdjustmentApplied, PolicyChanged, ShiftSwapped
    event_source VARCHAR(100) NOT NULL, -- API, ScheduledJob, ManualRecalculation
    event_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    triggered_by VARCHAR(100) NOT NULL, -- Real user, not service account
    -- Event payload
    employee_id BIGINT,
    affected_date_from DATE,
    affected_date_to DATE,
    source_table VARCHAR(100), -- hr_leave_request, hr_attendance_adjustment
    source_record_id BIGINT,
    event_payload_json JSONB, -- Full event details
    -- Processing
    processing_status VARCHAR(20), -- Pending, Processed, Failed
    processed_at TIMESTAMP,
    processing_error TEXT,
    recalculation_triggered BOOLEAN DEFAULT false,
    affected_snapshot_ids BIGINT[], -- Snapshots created by this event
    -- Priority
    event_priority INT DEFAULT 5, -- 1=urgent, 5=normal, 10=low
    -- Idempotency
    idempotency_key VARCHAR(255) UNIQUE,
    -- Audit
    CONSTRAINT chk_event_type CHECK (event_type IN (
        'LeaveApproved', 'LeaveCancelled',
        'AttendanceAdjustmentApploved', 'AttendanceAdjustmentRejected',
        'PolicyChanged', 'ShiftReassigned', 'HolidayAdded',
        'ManualRecalculation', 'ScheduledProcessing'
    ))
);

CREATE INDEX idx_attendance_event_unprocessed
ON hr_attendance_event(processing_status, event_priority, event_timestamp)
WHERE processing_status = 'Pending';
```

**Business Rule**:
- ✅ **ALL** attendance changes emit events
- ✅ Events are **source of truth** for "why"
- ✅ Processing is **idempotent** (replay-safe)
- ✅ Audit can trace **complete causality chain**

---

### 🔴 CRITICAL GAP #4: FINALIZATION NOT ENFORCED

**Current State**:
```csharp
// FROM: PayrollAttendanceSummaryService.RecalculateSummary()
public async Task<HrPayrollAttendanceSummary> RecalculateSummary(
    long employeeId, string payrollPeriod)
{
    var existing = await GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);
    if (existing != null)
    {
        await _summaryRepo.DeleteAsync(existing); // ❌ NO FINALIZATION CHECK
    }
    return await GenerateSummaryForEmployee(employeeId, payrollPeriod);
}
```

**What Goes Wrong**:
```
SCENARIO: Accidental Payroll Corruption
1. Payroll finalizes Feb summary (IsFinalized = true)
2. Payroll processes salaries, pays employees
3. Manager forgets summary was finalized
4. Manager calls RecalculateSummary()
5. ❌ FINALIZED SUMMARY DELETED
6. ❌ NEW SUMMARY GENERATED (with different values)
7. ❌ Payroll and attendance now INCONSISTENT
8. ❌ Cannot reconcile payments
```

**Financial Impact**:
- Salary overpayments cannot be detected
- Audit trail broken
- Regulatory compliance failure

**MANDATORY FIX**:
```csharp
// ENFORCED FINALIZATION CHECK
public async Task<HrPayrollAttendanceSummary> RecalculateSummary(
    long employeeId, string payrollPeriod, string requestedBy, string reason)
{
    var existing = await GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);

    if (existing != null && existing.IsFinalized)
    {
        // 🔴 CRITICAL: Cannot modify finalized summary
        throw new InvalidOperationException(
            $"Cannot recalculate finalized summary. " +
            $"Summary for {employeeId} period {payrollPeriod} was finalized on " +
            $"{existing.FinalizedDate} by {existing.FinalizedBy}. " +
            $"To recalculate, you must UNFINALIZE first via approved workflow."
        );
    }

    // Log the recalculation request
    await _auditService.LogRecalculation(employeeId, payrollPeriod, requestedBy, reason);

    // Archive old summary before deletion (for audit)
    if (existing != null)
    {
        await _snapshotService.ArchiveSummary(existing, requestedBy, reason);
        await _summaryRepo.DeleteAsync(existing);
    }

    return await GenerateSummaryForEmployee(employeeId, payrollPeriod);
}

// NEW: Unfinalize workflow (requires approval)
public async Task<bool> UnfinalizeSummary(
    long employeeId, string payrollPeriod,
    string requestedBy, string approvedBy, string reason)
{
    // Requires dual authorization (requestor + approver)
    if (requestedBy == approvedBy)
    {
        throw new InvalidOperationException("Unfinalizing requires approval from different user");
    }

    var summary = await GetSummaryByEmployeeAndPeriod(employeeId, payrollPeriod);
    if (summary == null || !summary.IsFinalized)
    {
        return false;
    }

    // Log unfinalization (critical audit event)
    await _auditService.LogUnfinalization(summary, requestedBy, approvedBy, reason);

    // Create snapshot before unfinalizing
    await _snapshotService.CreateUnfinalizationSnapshot(summary, approvedBy, reason);

    summary.IsFinalized = false;
    summary.FinalizedDate = null;
    summary.FinalizedBy = null;
    summary.Update_by = approvedBy;
    summary.Update_time = DateTime.UtcNow;
    summary.Remark = $"Unfinalized by {approvedBy} on {DateTime.UtcNow}: {reason}";

    await _summaryRepo.UpdateAsync(summary);

    return true;
}
```

**Business Rule**:
- ✅ Finalized summaries are **PROTECTED**
- ✅ Unfinalizing requires **DUAL AUTHORIZATION**
- ✅ All unfinalization events are **AUDITED**
- ✅ Snapshot created **BEFORE** unfinalizing

---

### 🟠 HIGH GAP #5: RAW LOGS NOT TRULY IMMUTABLE

**Current State**:
```csharp
// HrAttendanceLog fields
public bool IsProcessed { get; set; } // ❌ Can be changed
public DateTime? ProcessedTime { get; set; } // ❌ Can be updated
```

**Missing Critical Fields**:
- ❌ No device raw payload (JSON from biometric device)
- ❌ No timezone information (log captured in which TZ?)
- ❌ No duplicate detection hash
- ❌ No source confidence score
- ❌ No segment linkage (which shift segment does this belong to?)

**What Goes Wrong**:
```
SCENARIO: Disputed Punch Time
Employee: "I punched in at 8:00 AM"
Manager: "System shows 8:15 AM"
Employee: "Someone changed my punch time"
Audit: "Prove the original punch time was 8:00 AM"
System: ❌ "We only have current value, no original device payload"
```

**MANDATORY FIX**:
```sql
-- Enhance HrAttendanceLog (ADD columns, don't modify existing)
ALTER TABLE hr_attendance_log
-- Device payload (raw data from device)
ADD COLUMN device_payload_json JSONB, -- Store COMPLETE device response
ADD COLUMN device_payload_hash VARCHAR(64), -- SHA256 for integrity

-- Timezone handling
ADD COLUMN captured_at_utc TIMESTAMP, -- When device captured (UTC)
ADD COLUMN captured_timezone VARCHAR(50), -- "Asia/Riyadh", "America/New_York"
ADD COLUMN log_time_utc TIMESTAMP, -- LogTime converted to UTC
ADD COLUMN timezone_offset_minutes INT, -- +180 for Asia/Riyadh

-- Source confidence
ADD COLUMN source_confidence VARCHAR(20), -- High (biometric), Medium (card), Low (manual)
ADD COLUMN biometric_score INT, -- 0-100, device confidence in biometric match
ADD COLUMN duplicate_of_log_id BIGINT REFERENCES hr_attendance_log(log_id),
ADD COLUMN is_duplicate BOOLEAN DEFAULT false,

-- Segment linkage
ADD COLUMN linked_shift_segment_id BIGINT REFERENCES hr_shift_segment(segment_id),
ADD COLUMN punch_direction VARCHAR(10), -- IN, OUT, BREAK_START, BREAK_END

-- Immutability enforcement
ADD COLUMN is_locked BOOLEAN DEFAULT false, -- Once locked, cannot be modified
ADD COLUMN locked_at TIMESTAMP,
ADD COLUMN locked_by VARCHAR(100);

-- Create trigger to prevent modification of locked logs
CREATE OR REPLACE FUNCTION prevent_locked_log_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.is_locked = true THEN
        RAISE EXCEPTION 'Cannot modify locked attendance log (log_id=%, locked_at=%, locked_by=%)',
            OLD.log_id, OLD.locked_at, OLD.locked_by;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_locked_log_modification
BEFORE UPDATE OR DELETE ON hr_attendance_log
FOR EACH ROW
EXECUTE FUNCTION prevent_locked_log_modification();
```

**Business Rule**:
- ✅ Logs are **LOCKED** after processing
- ✅ Device payload **PRESERVED FOREVER**
- ✅ Timezone **ALWAYS RECORDED**
- ✅ Duplicates **DETECTED AND FLAGGED**
- ✅ Audit can **PROVE ORIGINAL DATA**

---

### 🟠 HIGH GAP #6: LEAVE/ATTENDANCE COUPLING

**Current State**:
- `HrLeaveAttendanceEffect` exists (good!)
- **BUT**: `AttendanceProcessingService` doesn't consume it
- **BUT**: Leave approval creates effects, but attendance doesn't read them
- **BUT**: Payroll summary aggregates effects, but attendance day doesn't

**Architecture Flaw**:
```
Leave System ──(generates)──> LeaveAttendanceEffect
                                        ↓
                                (NOT consumed by)
                                        ↓
                              AttendanceProcessing
```

**What Goes Wrong**:
```
SCENARIO: Leave During Work Day
1. Employee takes half-day leave (approved)
2. LeaveAttendanceEffect created (DeductedMinutes = 240)
3. Employee also punches in/out (worked 4 hours)
4. AttendanceProcessing runs
5. ❌ Marks day as "worked 4 hours" (ignores leave effect)
6. ❌ PayrollSummary sees BOTH worked + leave time
7. ❌ Employee gets paid for 8 hours (4 worked + 4 leave)
8. ❌ Should only be paid for 4 hours total
```

**MANDATORY FIX**:
```csharp
// REFACTORED: AttendanceProcessingService.ProcessAttendanceForDate()
private async Task<HrAttendanceDay> ProcessEmployeeAttendanceForDate(
    long employeeId, DateTime date)
{
    // 1. Get raw logs
    var logs = await GetLogsForEmployeeAndDate(employeeId, date);

    // 2. Get shift and policy
    var shift = await GetEmployeeShift(employeeId, date);
    var policy = await GetAttendancePolicy(shift?.PolicyId);

    // 3. Calculate worked time from logs
    var workedMinutes = CalculateWorkedMinutesFromLogs(logs, shift);
    var expectedMinutes = CalculateExpectedMinutes(date, shift);

    // 4. 🔴 NEW: Check leave effects
    var leaveEffects = await _leaveEffectRepo.GetEffectsByEmployeeAndDate(employeeId, date);
    var totalLeaveMinutes = leaveEffects.Sum(e => e.DeductedMinutes);
    var isOnLeave = leaveEffects.Any();

    // 5. Adjust expected minutes based on leave
    if (isOnLeave)
    {
        expectedMinutes -= totalLeaveMinutes;

        // If full-day leave, mark as absent but with leave reason
        if (totalLeaveMinutes >= expectedMinutes)
        {
            return new HrAttendanceDay
            {
                EmployeeId = employeeId,
                AttendanceDate = date,
                IsAbsent = true,
                AbsentReason = "Full Day Leave",
                LeaveMinutes = totalLeaveMinutes,
                ExpectedMinutes = 0, // No work expected
                WorkedMinutes = 0,
                // ... other fields
            };
        }
    }

    // 6. Calculate late/early leave based on ADJUSTED expected time
    var lateMinutes = CalculateLateMinutes(logs, shift, policy);
    var earlyLeaveMinutes = CalculateEarlyLeaveMinutes(logs, shift, policy);

    // 7. Overtime only if worked > expected (excluding leave time)
    var overtimeMinutes = Math.Max(0, workedMinutes - expectedMinutes);

    return new HrAttendanceDay
    {
        EmployeeId = employeeId,
        AttendanceDate = date,
        ExpectedMinutes = expectedMinutes, // Adjusted for leave
        WorkedMinutes = workedMinutes,
        LeaveMinutes = totalLeaveMinutes, // NEW FIELD
        LateMinutes = lateMinutes,
        EarlyLeaveMinutes = earlyLeaveMinutes,
        OvertimeMinutes = overtimeMinutes,
        IsAbsent = workedMinutes == 0 && expectedMinutes > 0,
        // ...
    };
}
```

**Database Enhancement**:
```sql
-- Add leave tracking to attendance day
ALTER TABLE hr_attendance_day
ADD COLUMN leave_minutes INT DEFAULT 0,
ADD COLUMN is_partial_day_leave BOOLEAN DEFAULT false,
ADD COLUMN leave_effect_ids BIGINT[]; -- Array of effect IDs
```

**Business Rule**:
- ✅ Attendance **ALWAYS** checks leave effects
- ✅ Expected time is **ADJUSTED** for leave
- ✅ Overtime calculated on **NET expected time**
- ✅ Payroll sees **SINGLE SOURCE OF TRUTH**

---

### 🟠 HIGH GAP #7: NO OVERTIME VERIFICATION MODEL

**Current State**:
- Overtime calculated at day level
- **NO** linkage to specific time segments
- **NO** proof of which hours were overtime
- **CANNOT** answer: "Show me exactly when the employee worked overtime"

**What Goes Wrong**:
```
SCENARIO: Overtime Audit
Auditor: "Employee claimed 2 hours overtime on Mar 15. Prove it."
System: "OvertimeMinutes = 120"
Auditor: "Show me the punch times"
System: "CheckIn = 08:00, CheckOut = 18:00"
Auditor: "How do you know the 2 hours were from 16:00-18:00 and not a data entry error?"
System: ❌ "We don't track segment-level overtime"
Auditor: ❌ "AUDIT FAILURE"
```

**MANDATORY FIX**:
```sql
-- NEW TABLE: Overtime Segment Verification
CREATE TABLE hr_overtime_segment_verification (
    verification_id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT NOT NULL,
    work_date DATE NOT NULL,
    overtime_result_id BIGINT REFERENCES hr_overtime_result(overtime_result_id),
    -- Segment proof
    shift_segment_id BIGINT REFERENCES hr_shift_segment(segment_id),
    segment_expected_end_time TIME,
    actual_work_start_time TIMESTAMP, -- Exact punch in
    actual_work_end_time TIMESTAMP, -- Exact punch out
    overtime_start_time TIMESTAMP, -- When overtime started
    overtime_end_time TIMESTAMP, -- When overtime ended
    overtime_minutes INT,
    -- Proof of work (link to raw logs)
    overtime_start_log_id BIGINT REFERENCES hr_attendance_log(log_id),
    overtime_end_log_id BIGINT REFERENCES hr_attendance_log(log_id),
    -- Verification
    is_verified BOOLEAN DEFAULT false,
    verified_by VARCHAR(100),
    verified_at TIMESTAMP,
    verification_method VARCHAR(50), -- Auto, ManagerApproved, Disputed
    -- Approval linkage
    overtime_request_id BIGINT REFERENCES hr_overtime_request(overtime_request_id),
    is_pre_approved BOOLEAN DEFAULT false,
    -- Audit
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (employee_id, work_date)
        REFERENCES hr_attendance_day(employee_id, attendance_date)
);
```

**Business Rule**:
- ✅ Overtime **MUST** be linked to specific punch times
- ✅ Overtime **MUST** reference original attendance logs
- ✅ Overtime **MUST** be verifiable by manager
- ✅ Audit can **PROVE** exact overtime period

---

### 🟡 MEDIUM GAP #8: NO MULTI-TIMEZONE SUPPORT

**Current State**:
- All timestamps stored in server timezone (assume UTC)
- **NO** employee timezone
- **NO** shift timezone
- **NO** device timezone
- **WILL FAIL** for global operations

**What Goes Wrong**:
```
SCENARIO: Multi-Country Operations
Employee in Dubai (UTC+4) punches in at 08:00 local time
Device sends: 2025-03-15 08:00:00
Server (in UTC) stores: 2025-03-15 08:00:00 UTC ❌ WRONG
Correct storage: 2025-03-15 04:00:00 UTC (08:00 Dubai - 4 hours)

Later: Generate attendance report for "March 14"
Query: attendance_date = '2025-03-14'
Result: ❌ MISSING punch (because stored as 08:00 UTC = March 15 Dubai)
```

**MANDATORY FIX**:
```sql
-- Add timezone tracking to ALL time-sensitive tables
ALTER TABLE hr_employee_core
ADD COLUMN work_timezone VARCHAR(50) DEFAULT 'UTC'; -- "Asia/Riyadh", "America/New_York"

ALTER TABLE hr_shift
ADD COLUMN shift_timezone VARCHAR(50) DEFAULT 'UTC';

ALTER TABLE hr_attendance_device
ADD COLUMN device_timezone VARCHAR(50) DEFAULT 'UTC';

-- Attendance log already enhanced with timezone in Gap #5

-- Add timezone conversion functions
CREATE OR REPLACE FUNCTION convert_to_employee_timezone(
    utc_time TIMESTAMP,
    employee_id BIGINT
) RETURNS TIMESTAMP AS $$
DECLARE
    emp_timezone VARCHAR(50);
BEGIN
    SELECT work_timezone INTO emp_timezone
    FROM hr_employee_core
    WHERE employee_id = $2;

    RETURN utc_time AT TIME ZONE 'UTC' AT TIME ZONE emp_timezone;
END;
$$ LANGUAGE plpgsql;
```

**Business Rule**:
- ✅ **ALL** timestamps stored in UTC
- ✅ **ALL** entities have timezone field
- ✅ **ALL** queries convert to local timezone
- ✅ **ALL** reports show local time

---

### 🟡 MEDIUM GAP #9: HARDCODED BUSINESS RULES

**Current State**:
```csharp
// FROM: OvertimeCalculationService.DetermineOvertimeType()
private string DetermineOvertimeType(DateTime date, bool isHoliday)
{
    if (isHoliday)
        return "Holiday";

    // ❌ HARDCODED: Weekend = Friday/Saturday
    if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
        return "Weekend";

    return "Daily";
}
```

**What Goes Wrong**:
- Cannot support different weekend configurations (Saturday/Sunday, Friday only, etc.)
- Cannot support 6-day work weeks
- Cannot support custom weekend calendars per region

**MANDATORY FIX**:
```sql
-- Add weekend configuration to work calendar
ALTER TABLE hr_work_calendar
ADD COLUMN weekend_days VARCHAR(50) DEFAULT 'Friday,Saturday'; -- Comma-separated: "Friday,Saturday" or "Saturday,Sunday"

-- Or more structured:
CREATE TABLE hr_weekend_configuration (
    config_id BIGSERIAL PRIMARY KEY,
    calendar_id BIGINT REFERENCES hr_work_calendar(calendar_id),
    day_of_week INT, -- 0=Sunday, 1=Monday, ..., 6=Saturday
    is_weekend BOOLEAN DEFAULT true,
    UNIQUE(calendar_id, day_of_week)
);
```

```csharp
// REFACTORED: Use calendar configuration
private async Task<string> DetermineOvertimeType(DateTime date, long employeeId)
{
    // Check holiday first
    var isHoliday = await _holidayRepo.IsHoliday(date);
    if (isHoliday)
        return "Holiday";

    // Check weekend via calendar (no hardcoding)
    var calendar = await _calendarService.GetEmployeeCalendar(employeeId, date);
    var isWeekend = await _calendarService.IsWeekendDay(calendar.CalendarId, date.DayOfWeek);

    if (isWeekend)
        return "Weekend";

    return "Daily";
}
```

---

### 🟡 MEDIUM GAP #10: NO RECALCULATION VERSIONING

**Current State**:
- Delete + regenerate pattern
- **NO** history of "what changed?"
- **NO** diff between v1 and v2
- **NO** rollback capability

**MANDATORY FIX**: Addressed by Gap #1 (Snapshot Versioning)

---

## STEP 2 - SNAPSHOT ARCHITECTURE DESIGN

See **CRITICAL GAP #1** above for complete design.

**Key Tables**:
1. `hr_attendance_day_snapshot` - Daily attendance snapshots (versioned, immutable)
2. `hr_payroll_attendance_summary_snapshot` - Payroll summary snapshots (versioned)
3. `hr_overtime_segment_verification` - Overtime proof snapshots

**Business Rules**:
- Snapshots are **IMMUTABLE** after finalization
- Snapshots are **VERSIONED** (v1, v2, v3...)
- Snapshots **CAPTURE POLICY STATE** (policy_snapshot_json)
- Snapshots **REFERENCE CALCULATION ENGINE VERSION**
- Payroll **REFERENCES SPECIFIC SNAPSHOT VERSIONS**

---

## STEP 3 - EVENT-DRIVEN ARCHITECTURE DESIGN

See **CRITICAL GAP #3** above for complete design.

**Key Table**: `hr_attendance_event`

**Event Types**:
1. **LeaveApproved** - Leave request approved, triggers attendance recalculation
2. **LeaveCancelled** - Leave cancelled, triggers attendance recalculation
3. **AttendanceAdjustmentApproved** - Manual adjustment approved
4. **PolicyChanged** - Policy updated, triggers affected period recalculation
5. **ShiftReassigned** - Shift changed, triggers affected period recalculation
6. **HolidayAdded** - Holiday added, triggers affected day recalculation
7. **ManualRecalculation** - User-triggered recalculation
8. **ScheduledProcessing** - Daily batch job

**Processing Flow**:
```
1. Event Emitter → Insert event into hr_attendance_event
2. Event Processor (background job) → Poll pending events
3. Event Handler → Process event (generate snapshot)
4. Mark event as processed → Update processing_status
5. Audit → Link snapshot_id to event_id
```

---

## STEP 4 - POLICY VERSIONING DESIGN

See **CRITICAL GAP #2** above for complete design.

**Enhanced Tables**:
- `hr_attendance_policy` - Add effective_from, effective_to, policy_version
- `hr_overtime_policy` - Add effective_from, effective_to, policy_version
- `hr_leave_policy` - Add effective_from, effective_to, policy_version

**Policy Retrieval**:
```csharp
// Get policy active on specific date
public async Task<HrAttendancePolicy> GetPolicyForDate(long policyId, DateTime date)
{
    return await Context.Queryable<HrAttendancePolicy>()
        .Where(p => p.PolicyId == policyId)
        .Where(p => p.EffectiveFrom <= date && p.EffectiveTo >= date)
        .OrderByDescending(p => p.PolicyVersion)
        .FirstAsync();
}
```

---

## STEP 5 - ATTENDANCE LOG HARDENING

See **HIGH GAP #5** above for complete design.

**Enhanced Fields**:
- `device_payload_json` - Complete device response
- `device_payload_hash` - SHA256 integrity check
- `captured_timezone` - Device timezone
- `source_confidence` - High/Medium/Low
- `is_locked` - Immutability flag
- `duplicate_of_log_id` - Duplicate detection

**Immutability Trigger**: `prevent_locked_log_modification()`

---

## STEP 6 - LEAVE/ATTENDANCE DECOUPLING

See **HIGH GAP #6** above for complete design.

**Key Change**: `AttendanceProcessingService` **MUST** consume `HrLeaveAttendanceEffect`

**Data Flow**:
```
LeaveManagementService.ApproveLeave()
    ↓ (generates)
HrLeaveAttendanceEffect
    ↓ (consumed by)
AttendanceProcessingService.ProcessAttendance()
    ↓ (adjusts)
HrAttendanceDay.ExpectedMinutes -= LeaveMinutes
    ↓ (aggregates)
HrPayrollAttendanceSummary (single source of truth)
```

---

## STEP 7 - OVERTIME VERIFICATION MODEL

See **HIGH GAP #7** above for complete design.

**Key Table**: `hr_overtime_segment_verification`

**Proof Chain**:
```
AttendanceLog (punch times)
    ↓ (links to)
OvertimeSegmentVerification (proves overtime period)
    ↓ (links to)
OvertimeResult (payroll calculation)
    ↓ (consumed by)
PayrollAttendanceSummary (salary calculation)
```

---

## SEVERITY SUMMARY

| Gap # | Gap Description | Severity | Audit Risk | Payroll Risk | Compliance Risk |
|-------|----------------|----------|------------|--------------|-----------------|
| 1 | No True Snapshot Architecture | 🔴 CRITICAL | HIGH | HIGH | HIGH |
| 2 | No Policy Versioning | 🔴 CRITICAL | HIGH | MEDIUM | HIGH |
| 3 | No Event-Driven Architecture | 🔴 CRITICAL | HIGH | LOW | MEDIUM |
| 4 | Finalization Not Enforced | 🔴 CRITICAL | HIGH | HIGH | HIGH |
| 5 | Raw Logs Not Immutable | 🟠 HIGH | MEDIUM | MEDIUM | MEDIUM |
| 6 | Leave/Attendance Coupling | 🟠 HIGH | LOW | HIGH | LOW |
| 7 | No Overtime Verification | 🟠 HIGH | HIGH | MEDIUM | MEDIUM |
| 8 | No Multi-Timezone Support | 🟡 MEDIUM | LOW | LOW | MEDIUM |
| 9 | Hardcoded Business Rules | 🟡 MEDIUM | LOW | LOW | MEDIUM |
| 10 | No Recalculation Versioning | 🟡 MEDIUM | MEDIUM | MEDIUM | LOW |

---

## FINAL RECOMMENDATION

**Status**: **MANDATORY REFACTORING REQUIRED**

**Critical Path** (Must Fix Before Production):
1. ✅ Implement Snapshot Architecture (#1)
2. ✅ Implement Policy Versioning (#2)
3. ✅ Implement Event-Driven Architecture (#3)
4. ✅ Enforce Finalization Protection (#4)

**High Priority** (Required for Audit Compliance):
5. ✅ Harden Attendance Logs (#5)
6. ✅ Decouple Leave/Attendance (#6)
7. ✅ Implement Overtime Verification (#7)

**Medium Priority** (Required for Global Operations):
8. ✅ Add Multi-Timezone Support (#8)
9. ✅ Remove Hardcoded Business Rules (#9)

---

**Next Steps**: Proceed to **STEP 8 - DATABASE CHANGES** (migration scripts)

---

**Document Version**: 1.0
**Classification**: INTERNAL - ARCHITECTURE REVIEW
**Review Date**: 2025-12-22
