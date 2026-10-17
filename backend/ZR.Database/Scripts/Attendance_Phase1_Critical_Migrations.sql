-- ===========================================================================
-- ATTENDANCE MODULE - PHASE 1 CRITICAL MIGRATIONS
-- ===========================================================================
-- Purpose: Enterprise-grade snapshot, event-driven, and policy versioning
-- Date: 2025-12-22
-- Classification: CRITICAL - Required for Audit Compliance
-- Execution Order: Phase 1 (before Phase 2 and Phase 3)
-- Rollback: See rollback script at end
-- ===========================================================================

-- ===========================================================================
-- MIGRATION #1: ATTENDANCE DAY SNAPSHOT ARCHITECTURE
-- ===========================================================================
-- Purpose: Immutable, versioned snapshots of daily attendance
-- Audit Requirement: Must be able to reproduce calculations as they appeared at any point in time
-- ===========================================================================

CREATE TABLE IF NOT EXISTS hr_attendance_day_snapshot (
    -- Primary Key
    snapshot_id BIGSERIAL PRIMARY KEY,

    -- Composite Natural Key
    employee_id BIGINT NOT NULL,
    attendance_date DATE NOT NULL,
    snapshot_version INT NOT NULL DEFAULT 1,

    -- Snapshot Metadata
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    snapshot_reason VARCHAR(200), -- "InitialProcessing", "AdjustmentApplied", "LeaveApproved", "ManualRecalculation"
    superseded_by_snapshot_id BIGINT REFERENCES hr_attendance_day_snapshot(snapshot_id),

    -- Policy Snapshot (capture policy state at calculation time)
    policy_id BIGINT,
    policy_snapshot_json JSONB, -- Complete policy configuration as JSON
    policy_version INT,

    -- Shift Snapshot
    shift_id BIGINT,
    shift_snapshot_json JSONB, -- Complete shift configuration as JSON

    -- Calculation Engine Version (for algorithm tracking)
    calculation_engine_version VARCHAR(20) NOT NULL DEFAULT 'v1.0.0',
    calculation_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),

    -- Calculated Values (IMMUTABLE after finalization)
    expected_minutes INT NOT NULL DEFAULT 0,
    worked_minutes INT NOT NULL DEFAULT 0,
    late_minutes INT NOT NULL DEFAULT 0,
    early_leave_minutes INT NOT NULL DEFAULT 0,
    overtime_minutes INT NOT NULL DEFAULT 0,
    leave_minutes INT NOT NULL DEFAULT 0,
    break_minutes INT NOT NULL DEFAULT 0,

    -- Status Flags
    is_absent BOOLEAN NOT NULL DEFAULT false,
    is_weekend BOOLEAN NOT NULL DEFAULT false,
    is_holiday BOOLEAN NOT NULL DEFAULT false,
    is_partial_day_leave BOOLEAN NOT NULL DEFAULT false,

    -- Punch Times
    check_in_time TIMESTAMP,
    check_out_time TIMESTAMP,
    check_in_device_id BIGINT,
    check_out_device_id BIGINT,

    -- Segment Proofs (JSON array of segments)
    segments_json JSONB, -- [{"segment_id": 1, "in": "08:00", "out": "12:00", "worked": 240}]

    -- Leave Effect Linkage
    leave_effect_ids BIGINT[], -- Array of hr_leave_attendance_effect IDs

    -- Attendance Log Linkage (proof of source data)
    source_log_ids BIGINT[], -- Array of hr_attendance_log IDs used

    -- Finalization (CRITICAL: Makes snapshot immutable)
    is_finalized BOOLEAN NOT NULL DEFAULT false,
    finalized_at TIMESTAMP,
    finalized_by VARCHAR(100),
    finalized_for_payroll_period VARCHAR(7), -- "2025-02" (YYYY-MM)

    -- Event Linkage (what triggered this snapshot)
    triggered_by_event_id BIGINT, -- References hr_attendance_event (created later)

    -- Audit Fields
    remark TEXT,

    -- Constraints
    CONSTRAINT chk_snapshot_version_positive CHECK (snapshot_version > 0),
    CONSTRAINT chk_expected_minutes_non_negative CHECK (expected_minutes >= 0),
    CONSTRAINT chk_worked_minutes_non_negative CHECK (worked_minutes >= 0),
    CONSTRAINT uq_attendance_snapshot_version UNIQUE(employee_id, attendance_date, snapshot_version)
);

-- Indexes for Performance
CREATE INDEX idx_attendance_snapshot_employee_date
ON hr_attendance_day_snapshot(employee_id, attendance_date);

CREATE INDEX idx_attendance_snapshot_finalized_period
ON hr_attendance_day_snapshot(finalized_for_payroll_period, employee_id)
WHERE is_finalized = true;

CREATE INDEX idx_attendance_snapshot_latest_version
ON hr_attendance_day_snapshot(employee_id, attendance_date, snapshot_version DESC);

-- Immutability Trigger (CRITICAL: Prevent modification of finalized snapshots)
CREATE OR REPLACE FUNCTION prevent_finalized_snapshot_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.is_finalized = true THEN
        RAISE EXCEPTION 'Cannot modify finalized attendance snapshot (snapshot_id=%, finalized_at=%, finalized_by=%)',
            OLD.snapshot_id, OLD.finalized_at, OLD.finalized_by;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_finalized_snapshot_modification
BEFORE UPDATE OR DELETE ON hr_attendance_day_snapshot
FOR EACH ROW
EXECUTE FUNCTION prevent_finalized_snapshot_modification();

-- Comments
COMMENT ON TABLE hr_attendance_day_snapshot IS 'Immutable, versioned snapshots of daily attendance for audit compliance and payroll integrity';
COMMENT ON COLUMN hr_attendance_day_snapshot.snapshot_version IS 'Version number (1, 2, 3...). Increments on recalculation.';
COMMENT ON COLUMN hr_attendance_day_snapshot.policy_snapshot_json IS 'Complete policy configuration at calculation time for reproducibility';
COMMENT ON COLUMN hr_attendance_day_snapshot.calculation_engine_version IS 'Software version used for calculation (e.g., v2.1.0)';
COMMENT ON COLUMN hr_attendance_day_snapshot.is_finalized IS 'CRITICAL: When true, snapshot is immutable and locked for payroll';
COMMENT ON COLUMN hr_attendance_day_snapshot.segments_json IS 'Proof of work segments: [{"segment_id": 1, "in": "08:00", "out": "12:00", "worked": 240}]';

-- ===========================================================================
-- MIGRATION #2: PAYROLL SUMMARY SNAPSHOT ENHANCEMENT
-- ===========================================================================
-- Purpose: Link payroll summaries to specific snapshot versions
-- Audit Requirement: Payroll must reference exact snapshot version used for salary calculation
-- ===========================================================================

ALTER TABLE hr_payroll_attendance_summary
-- Snapshot Version Linkage (CRITICAL: Links to specific snapshots used)
ADD COLUMN IF NOT EXISTS snapshot_version_start INT,
ADD COLUMN IF NOT EXISTS snapshot_version_end INT,
ADD COLUMN IF NOT EXISTS snapshot_version_used VARCHAR(50), -- "v1" or "v1-v3" for range

-- Calculation Metadata
ADD COLUMN IF NOT EXISTS calculation_engine_version VARCHAR(20),
ADD COLUMN IF NOT EXISTS generated_at TIMESTAMP DEFAULT NOW(),

-- Finalization Enhancement (stronger enforcement)
ADD COLUMN IF NOT EXISTS unfinalized_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS unfinalized_by VARCHAR(100),
ADD COLUMN IF NOT EXISTS unfinalization_reason TEXT,
ADD COLUMN IF NOT EXISTS finalization_count INT DEFAULT 0; -- Track how many times finalized/unfinalized

-- Add check constraint
ALTER TABLE hr_payroll_attendance_summary
ADD CONSTRAINT chk_finalization_count_non_negative
CHECK (finalization_count >= 0);

-- Index for finalized summaries
CREATE INDEX IF NOT EXISTS idx_payroll_summary_finalized
ON hr_payroll_attendance_summary(payroll_period, is_finalized)
WHERE is_finalized = true;

-- Comments
COMMENT ON COLUMN hr_payroll_attendance_summary.snapshot_version_used IS 'Snapshot versions used for this summary (e.g., "v1" or "v1-v3" if recalculated)';
COMMENT ON COLUMN hr_payroll_attendance_summary.finalization_count IS 'Number of times summary was finalized (for audit trail)';
COMMENT ON COLUMN hr_payroll_attendance_summary.unfinalization_reason IS 'Required reason when summary is unfinalized (dual authorization)';

-- ===========================================================================
-- MIGRATION #3: EVENT-DRIVEN ARCHITECTURE
-- ===========================================================================
-- Purpose: Event log for all attendance changes (audit trail + event sourcing)
-- Audit Requirement: Must be able to answer "Why did this change?" and "Who triggered it?"
-- ===========================================================================

CREATE TABLE IF NOT EXISTS hr_attendance_event (
    -- Primary Key
    event_id BIGSERIAL PRIMARY KEY,

    -- Event Metadata
    event_type VARCHAR(50) NOT NULL,
    event_source VARCHAR(100) NOT NULL, -- "API", "ScheduledJob", "ManualRecalculation", "BackgroundWorker"
    event_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    triggered_by VARCHAR(100) NOT NULL, -- Real user (not service account)

    -- Event Scope (which data affected)
    employee_id BIGINT,
    affected_date_from DATE,
    affected_date_to DATE,
    department_id BIGINT, -- For bulk operations

    -- Event Source (what caused this event)
    source_table VARCHAR(100), -- "hr_leave_request", "hr_attendance_adjustment"
    source_record_id BIGINT,
    source_record_snapshot JSONB, -- Snapshot of source record for audit

    -- Event Payload (complete event details)
    event_payload_json JSONB NOT NULL, -- Full event data
    event_payload_hash VARCHAR(64), -- SHA256 for integrity verification

    -- Processing Status
    processing_status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    processed_at TIMESTAMP,
    processing_started_at TIMESTAMP,
    processing_error TEXT,
    processing_retry_count INT DEFAULT 0,

    -- Processing Results
    recalculation_triggered BOOLEAN DEFAULT false,
    affected_snapshot_ids BIGINT[], -- Snapshots created by this event
    affected_employee_ids BIGINT[], -- For bulk operations

    -- Priority & Scheduling
    event_priority INT DEFAULT 5, -- 1=urgent, 5=normal, 10=low
    scheduled_for TIMESTAMP, -- For delayed processing

    -- Idempotency (prevent duplicate processing)
    idempotency_key VARCHAR(255) UNIQUE,
    duplicate_of_event_id BIGINT REFERENCES hr_attendance_event(event_id),

    -- Audit
    created_at TIMESTAMP DEFAULT NOW(),

    -- Constraints
    CONSTRAINT chk_event_type CHECK (event_type IN (
        'LeaveApproved', 'LeaveCancelled', 'LeaveModified',
        'AttendanceAdjustmentApproved', 'AttendanceAdjustmentRejected',
        'PolicyChanged', 'PolicyActivated', 'PolicyDeactivated',
        'ShiftReassigned', 'ShiftModified',
        'HolidayAdded', 'HolidayRemoved',
        'ManualRecalculation', 'BulkRecalculation',
        'ScheduledProcessing', 'PeriodFinalized', 'PeriodUnfinalized',
        'OvertimeApproved', 'OvertimeRejected'
    )),
    CONSTRAINT chk_processing_status CHECK (processing_status IN (
        'Pending', 'Processing', 'Processed', 'Failed', 'Cancelled', 'Duplicate'
    )),
    CONSTRAINT chk_event_priority CHECK (event_priority BETWEEN 1 AND 10),
    CONSTRAINT chk_processing_retry_count CHECK (processing_retry_count >= 0)
);

-- Indexes for Event Processing
CREATE INDEX idx_attendance_event_unprocessed
ON hr_attendance_event(processing_status, event_priority, event_timestamp)
WHERE processing_status = 'Pending';

CREATE INDEX idx_attendance_event_employee_date
ON hr_attendance_event(employee_id, affected_date_from, affected_date_to);

CREATE INDEX idx_attendance_event_source
ON hr_attendance_event(source_table, source_record_id);

CREATE INDEX idx_attendance_event_idempotency
ON hr_attendance_event(idempotency_key)
WHERE idempotency_key IS NOT NULL;

-- Add event linkage to snapshot table
ALTER TABLE hr_attendance_day_snapshot
DROP CONSTRAINT IF EXISTS fk_snapshot_event,
ADD CONSTRAINT fk_snapshot_event
FOREIGN KEY (triggered_by_event_id)
REFERENCES hr_attendance_event(event_id);

-- Comments
COMMENT ON TABLE hr_attendance_event IS 'Event log for all attendance changes - enables event sourcing and complete audit trail';
COMMENT ON COLUMN hr_attendance_event.event_type IS 'Type of event that occurred (LeaveApproved, PolicyChanged, etc.)';
COMMENT ON COLUMN hr_attendance_event.triggered_by IS 'REAL USER who triggered event (not service account) for accountability';
COMMENT ON COLUMN hr_attendance_event.idempotency_key IS 'Unique key to prevent duplicate event processing (e.g., "leave_approved_12345")';
COMMENT ON COLUMN hr_attendance_event.event_payload_json IS 'Complete event data for replay and debugging';
COMMENT ON COLUMN hr_attendance_event.affected_snapshot_ids IS 'Array of snapshot IDs created as result of this event';

-- ===========================================================================
-- MIGRATION #4: POLICY VERSIONING
-- ===========================================================================
-- Purpose: Temporal versioning of policies for reproducibility
-- Audit Requirement: Must be able to prove which policy was active on any given date
-- ===========================================================================

-- Attendance Policy Versioning
ALTER TABLE hr_attendance_policy
ADD COLUMN IF NOT EXISTS effective_from DATE NOT NULL DEFAULT '2000-01-01',
ADD COLUMN IF NOT EXISTS effective_to DATE DEFAULT '9999-12-31',
ADD COLUMN IF NOT EXISTS policy_version INT NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS superseded_by_policy_id BIGINT REFERENCES hr_attendance_policy(policy_id),
ADD COLUMN IF NOT EXISTS version_notes TEXT,
ADD COLUMN IF NOT EXISTS is_active_version BOOLEAN DEFAULT true;

-- Create unique constraint on policy versioning
CREATE UNIQUE INDEX IF NOT EXISTS uq_attendance_policy_version
ON hr_attendance_policy(policy_id, policy_version);

-- Index for temporal queries
CREATE INDEX IF NOT EXISTS idx_attendance_policy_temporal
ON hr_attendance_policy(policy_id, effective_from, effective_to)
WHERE is_active_version = true;

-- Overtime Policy Versioning
ALTER TABLE hr_overtime_policy
ADD COLUMN IF NOT EXISTS effective_from DATE NOT NULL DEFAULT '2000-01-01',
ADD COLUMN IF NOT EXISTS effective_to DATE DEFAULT '9999-12-31',
ADD COLUMN IF NOT EXISTS policy_version INT NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS superseded_by_policy_id BIGINT REFERENCES hr_overtime_policy(overtime_policy_id),
ADD COLUMN IF NOT EXISTS version_notes TEXT,
ADD COLUMN IF NOT EXISTS is_active_version BOOLEAN DEFAULT true;

CREATE UNIQUE INDEX IF NOT EXISTS uq_overtime_policy_version
ON hr_overtime_policy(overtime_policy_id, policy_version);

CREATE INDEX IF NOT EXISTS idx_overtime_policy_temporal
ON hr_overtime_policy(overtime_policy_id, effective_from, effective_to)
WHERE is_active_version = true;

-- Leave Policy Versioning
ALTER TABLE hr_leave_policy
ADD COLUMN IF NOT EXISTS effective_from DATE NOT NULL DEFAULT '2000-01-01',
ADD COLUMN IF NOT EXISTS effective_to DATE DEFAULT '9999-12-31',
ADD COLUMN IF NOT EXISTS policy_version INT NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS superseded_by_policy_id BIGINT REFERENCES hr_leave_policy(leave_policy_id),
ADD COLUMN IF NOT EXISTS version_notes TEXT,
ADD COLUMN IF NOT EXISTS is_active_version BOOLEAN DEFAULT true;

CREATE UNIQUE INDEX IF NOT EXISTS uq_leave_policy_version
ON hr_leave_policy(leave_policy_id, policy_version);

CREATE INDEX IF NOT EXISTS idx_leave_policy_temporal
ON hr_leave_policy(leave_policy_id, effective_from, effective_to)
WHERE is_active_version = true;

-- Comments
COMMENT ON COLUMN hr_attendance_policy.effective_from IS 'Date when this policy version becomes active';
COMMENT ON COLUMN hr_attendance_policy.effective_to IS 'Date when this policy version expires';
COMMENT ON COLUMN hr_attendance_policy.policy_version IS 'Version number (1, 2, 3...). Increments when policy changes.';
COMMENT ON COLUMN hr_attendance_policy.superseded_by_policy_id IS 'Points to newer version of this policy';
COMMENT ON COLUMN hr_attendance_policy.is_active_version IS 'TRUE if this version is currently active for its effective period';

-- ===========================================================================
-- MIGRATION #5: FINALIZATION ENFORCEMENT
-- ===========================================================================
-- Purpose: Prevent accidental modification of finalized payroll summaries
-- Audit Requirement: Finalized summaries must be protected from modification
-- ===========================================================================

-- Trigger to prevent modification of finalized summaries
CREATE OR REPLACE FUNCTION prevent_finalized_summary_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.is_finalized = true AND (
        NEW.total_worked_minutes != OLD.total_worked_minutes OR
        NEW.total_expected_minutes != OLD.total_expected_minutes OR
        NEW.total_late_minutes != OLD.total_late_minutes OR
        NEW.total_early_leave_minutes != OLD.total_early_leave_minutes OR
        NEW.total_absent_minutes != OLD.total_absent_minutes OR
        NEW.total_leave_minutes != OLD.total_leave_minutes OR
        NEW.total_overtime_minutes != OLD.total_overtime_minutes OR
        NEW.calculated_salary_factor != OLD.calculated_salary_factor
    ) THEN
        RAISE EXCEPTION 'Cannot modify finalized payroll summary (summary_id=%, period=%, finalized_at=%, finalized_by=%). Use UnfinalizeSummary workflow first.',
            OLD.summary_id, OLD.payroll_period, OLD.finalized_date, OLD.finalized_by;
    END IF;

    -- Allow unfinalizing (is_finalized = false) but track it
    IF OLD.is_finalized = true AND NEW.is_finalized = false THEN
        NEW.unfinalized_at := NOW();
        NEW.unfinalized_by := COALESCE(NEW.update_by, 'SYSTEM');
        -- Increment finalization count (tracks finalize/unfinalize cycles)
        NEW.finalization_count := OLD.finalization_count;
    END IF;

    -- Track when finalizing
    IF OLD.is_finalized = false AND NEW.is_finalized = true THEN
        NEW.finalization_count := OLD.finalization_count + 1;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_finalized_summary_modification
BEFORE UPDATE ON hr_payroll_attendance_summary
FOR EACH ROW
EXECUTE FUNCTION prevent_finalized_summary_modification();

-- ===========================================================================
-- MIGRATION #6: AUDIT LOG ENHANCEMENT
-- ===========================================================================
-- Purpose: Enhanced audit tracking with event linkage
-- ===========================================================================

ALTER TABLE hr_attendance_audit_log
ADD COLUMN IF NOT EXISTS triggered_by_event_id BIGINT REFERENCES hr_attendance_event(event_id),
ADD COLUMN IF NOT EXISTS snapshot_id_before BIGINT,
ADD COLUMN IF NOT EXISTS snapshot_id_after BIGINT,
ADD COLUMN IF NOT EXISTS change_summary_json JSONB, -- Structured diff
ADD COLUMN IF NOT EXISTS ip_address VARCHAR(50),
ADD COLUMN IF NOT EXISTS user_agent TEXT;

CREATE INDEX IF NOT EXISTS idx_audit_log_event
ON hr_attendance_audit_log(triggered_by_event_id);

-- ===========================================================================
-- DATA MIGRATION: Initialize Existing Records
-- ===========================================================================
-- Purpose: Set default values for existing records to maintain compatibility
-- ===========================================================================

-- Initialize existing attendance policies with version 1
UPDATE hr_attendance_policy
SET policy_version = 1,
    effective_from = '2000-01-01',
    effective_to = '9999-12-31',
    is_active_version = true
WHERE policy_version IS NULL;

-- Initialize existing overtime policies with version 1
UPDATE hr_overtime_policy
SET policy_version = 1,
    effective_from = '2000-01-01',
    effective_to = '9999-12-31',
    is_active_version = true
WHERE policy_version IS NULL;

-- Initialize existing leave policies with version 1
UPDATE hr_leave_policy
SET policy_version = 1,
    effective_from = '2000-01-01',
    effective_to = '9999-12-31',
    is_active_version = true
WHERE policy_version IS NULL;

-- Initialize finalization count for existing summaries
UPDATE hr_payroll_attendance_summary
SET finalization_count = CASE WHEN is_finalized THEN 1 ELSE 0 END
WHERE finalization_count IS NULL;

-- ===========================================================================
-- VERIFICATION QUERIES
-- ===========================================================================
-- Purpose: Verify migrations completed successfully
-- ===========================================================================

-- Verify snapshot table created
SELECT COUNT(*) as snapshot_table_exists
FROM information_schema.tables
WHERE table_name = 'hr_attendance_day_snapshot';
-- Expected: 1

-- Verify event table created
SELECT COUNT(*) as event_table_exists
FROM information_schema.tables
WHERE table_name = 'hr_attendance_event';
-- Expected: 1

-- Verify policy versioning columns added
SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_attendance_policy'
AND column_name IN ('effective_from', 'effective_to', 'policy_version');
-- Expected: 3 rows

-- Verify triggers created
SELECT trigger_name
FROM information_schema.triggers
WHERE trigger_name IN (
    'trg_prevent_finalized_snapshot_modification',
    'trg_prevent_finalized_summary_modification'
);
-- Expected: 2 rows

-- ===========================================================================
-- ROLLBACK SCRIPT (USE WITH CAUTION)
-- ===========================================================================
-- WARNING: This will drop all new tables and columns. Use only in development.
-- ===========================================================================

/*
-- Drop triggers
DROP TRIGGER IF EXISTS trg_prevent_finalized_snapshot_modification ON hr_attendance_day_snapshot;
DROP TRIGGER IF EXISTS trg_prevent_finalized_summary_modification ON hr_payroll_attendance_summary;

-- Drop functions
DROP FUNCTION IF EXISTS prevent_finalized_snapshot_modification();
DROP FUNCTION IF EXISTS prevent_finalized_summary_modification();

-- Drop new tables
DROP TABLE IF EXISTS hr_attendance_day_snapshot CASCADE;
DROP TABLE IF EXISTS hr_attendance_event CASCADE;

-- Remove added columns from payroll summary
ALTER TABLE hr_payroll_attendance_summary
DROP COLUMN IF EXISTS snapshot_version_start,
DROP COLUMN IF EXISTS snapshot_version_end,
DROP COLUMN IF EXISTS snapshot_version_used,
DROP COLUMN IF EXISTS calculation_engine_version,
DROP COLUMN IF EXISTS generated_at,
DROP COLUMN IF EXISTS unfinalized_at,
DROP COLUMN IF EXISTS unfinalized_by,
DROP COLUMN IF EXISTS unfinalization_reason,
DROP COLUMN IF EXISTS finalization_count;

-- Remove versioning from policies
ALTER TABLE hr_attendance_policy
DROP COLUMN IF EXISTS effective_from,
DROP COLUMN IF EXISTS effective_to,
DROP COLUMN IF EXISTS policy_version,
DROP COLUMN IF EXISTS superseded_by_policy_id,
DROP COLUMN IF EXISTS version_notes,
DROP COLUMN IF EXISTS is_active_version;

ALTER TABLE hr_overtime_policy
DROP COLUMN IF EXISTS effective_from,
DROP COLUMN IF EXISTS effective_to,
DROP COLUMN IF EXISTS policy_version,
DROP COLUMN IF EXISTS superseded_by_policy_id,
DROP COLUMN IF EXISTS version_notes,
DROP COLUMN IF EXISTS is_active_version;

ALTER TABLE hr_leave_policy
DROP COLUMN IF EXISTS effective_from,
DROP COLUMN IF EXISTS effective_to,
DROP COLUMN IF EXISTS policy_version,
DROP COLUMN IF EXISTS superseded_by_policy_id,
DROP COLUMN IF EXISTS version_notes,
DROP COLUMN IF EXISTS is_active_version;

-- Remove audit log enhancements
ALTER TABLE hr_attendance_audit_log
DROP COLUMN IF EXISTS triggered_by_event_id,
DROP COLUMN IF EXISTS snapshot_id_before,
DROP COLUMN IF EXISTS snapshot_id_after,
DROP COLUMN IF EXISTS change_summary_json,
DROP COLUMN IF EXISTS ip_address,
DROP COLUMN IF EXISTS user_agent;
*/

-- ===========================================================================
-- END OF PHASE 1 MIGRATIONS
-- ===========================================================================
-- Next: Run Phase 2 migrations (Attendance Log Hardening, Overtime Verification)
-- ===========================================================================
