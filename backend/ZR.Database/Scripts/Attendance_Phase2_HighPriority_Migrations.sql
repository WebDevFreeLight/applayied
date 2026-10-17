-- ===========================================================================
-- ATTENDANCE MODULE - PHASE 2 HIGH PRIORITY MIGRATIONS
-- ===========================================================================
-- Purpose: Attendance log hardening, overtime verification, leave/attendance decoupling
-- Date: 2025-12-22
-- Classification: HIGH PRIORITY - Required for Audit Compliance
-- Execution Order: Phase 2 (after Phase 1)
-- Dependencies: Phase 1 must be completed first
-- ===========================================================================

-- ===========================================================================
-- MIGRATION #7: ATTENDANCE LOG HARDENING (Immutability + Device Payload)
-- ===========================================================================
-- Purpose: Make raw attendance logs truly immutable with complete device data
-- Audit Requirement: Must preserve original device payload forever
-- ===========================================================================

ALTER TABLE hr_attendance_log
-- Device Payload Preservation (CRITICAL: Store complete device response)
ADD COLUMN IF NOT EXISTS device_payload_json JSONB,
ADD COLUMN IF NOT EXISTS device_payload_hash VARCHAR(64), -- SHA256 for integrity verification

-- Timezone Handling (CRITICAL: Multi-country support)
ADD COLUMN IF NOT EXISTS captured_at_utc TIMESTAMP,
ADD COLUMN IF NOT EXISTS captured_timezone VARCHAR(50), -- IANA timezone: "Asia/Riyadh", "America/New_York"
ADD COLUMN IF NOT EXISTS log_time_utc TIMESTAMP, -- LogTime converted to UTC
ADD COLUMN IF NOT EXISTS timezone_offset_minutes INT, -- +180 for Asia/Riyadh

-- Source Confidence (Data Quality)
ADD COLUMN IF NOT EXISTS source_confidence VARCHAR(20), -- "High", "Medium", "Low"
ADD COLUMN IF NOT EXISTS biometric_score INT, -- 0-100, device confidence in biometric match
ADD COLUMN IF NOT EXISTS biometric_type VARCHAR(50), -- "Fingerprint", "Face", "Iris", "Card", "PIN"

-- Duplicate Detection
ADD COLUMN IF NOT EXISTS duplicate_of_log_id BIGINT REFERENCES hr_attendance_log(log_id),
ADD COLUMN IF NOT EXISTS is_duplicate BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS duplicate_detection_hash VARCHAR(64), -- Hash for detecting duplicates

-- Segment Linkage
ADD COLUMN IF NOT EXISTS linked_shift_segment_id BIGINT REFERENCES hr_shift_segment(segment_id),
ADD COLUMN IF NOT EXISTS punch_direction VARCHAR(20), -- "IN", "OUT", "BREAK_START", "BREAK_END"

-- Immutability Enforcement (CRITICAL: Lock logs after processing)
ADD COLUMN IF NOT EXISTS is_locked BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS locked_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS locked_by VARCHAR(100),
ADD COLUMN IF NOT EXISTS lock_reason VARCHAR(200), -- "ProcessedToAttendance", "SnapshotFinalized"

-- Geo-location (Optional: GPS from mobile app)
ADD COLUMN IF NOT EXISTS latitude DECIMAL(10, 8),
ADD COLUMN IF NOT EXISTS longitude DECIMAL(11, 8),
ADD COLUMN IF NOT EXISTS location_accuracy_meters DECIMAL(10, 2),
ADD COLUMN IF NOT EXISTS geo_fence_name VARCHAR(100); -- "HeadOffice", "Branch_Dubai"

-- Add constraints
ALTER TABLE hr_attendance_log
ADD CONSTRAINT chk_source_confidence CHECK (source_confidence IN ('High', 'Medium', 'Low') OR source_confidence IS NULL),
ADD CONSTRAINT chk_punch_direction CHECK (punch_direction IN ('IN', 'OUT', 'BREAK_START', 'BREAK_END') OR punch_direction IS NULL),
ADD CONSTRAINT chk_biometric_score CHECK (biometric_score BETWEEN 0 AND 100 OR biometric_score IS NULL);

-- Indexes for Performance
CREATE INDEX IF NOT EXISTS idx_attendance_log_locked
ON hr_attendance_log(is_locked, employee_id, log_time);

CREATE INDEX IF NOT EXISTS idx_attendance_log_duplicate
ON hr_attendance_log(duplicate_detection_hash)
WHERE is_duplicate = false;

CREATE INDEX IF NOT EXISTS idx_attendance_log_utc
ON hr_attendance_log(employee_id, log_time_utc);

-- Immutability Trigger (CRITICAL: Prevent modification of locked logs)
CREATE OR REPLACE FUNCTION prevent_locked_log_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.is_locked = true THEN
        RAISE EXCEPTION 'Cannot modify locked attendance log (log_id=%, locked_at=%, locked_by=%, reason=%)',
            OLD.log_id, OLD.locked_at, OLD.locked_by, OLD.lock_reason;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_locked_log_modification
BEFORE UPDATE OR DELETE ON hr_attendance_log
FOR EACH ROW
EXECUTE FUNCTION prevent_locked_log_modification();

-- Function to calculate duplicate detection hash
CREATE OR REPLACE FUNCTION calculate_log_duplicate_hash(
    p_employee_id BIGINT,
    p_log_time TIMESTAMP,
    p_device_id BIGINT
) RETURNS VARCHAR(64) AS $$
BEGIN
    -- Hash: employee + time rounded to minute + device
    RETURN encode(
        digest(
            p_employee_id::text ||
            date_trunc('minute', p_log_time)::text ||
            COALESCE(p_device_id::text, 'UNKNOWN'),
            'sha256'
        ),
        'hex'
    );
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Trigger to auto-calculate duplicate hash on insert
CREATE OR REPLACE FUNCTION set_duplicate_detection_hash()
RETURNS TRIGGER AS $$
BEGIN
    NEW.duplicate_detection_hash := calculate_log_duplicate_hash(
        NEW.employee_id,
        NEW.log_time,
        NEW.device_id
    );

    -- Check for duplicates (within 1 minute of same employee on same device)
    IF EXISTS (
        SELECT 1 FROM hr_attendance_log
        WHERE log_id != COALESCE(NEW.log_id, 0)
        AND duplicate_detection_hash = NEW.duplicate_detection_hash
        AND is_duplicate = false
        LIMIT 1
    ) THEN
        -- Mark as duplicate and link to original
        NEW.is_duplicate := true;
        SELECT log_id INTO NEW.duplicate_of_log_id
        FROM hr_attendance_log
        WHERE duplicate_detection_hash = NEW.duplicate_detection_hash
        AND is_duplicate = false
        ORDER BY log_time
        LIMIT 1;
    END IF;

    -- Auto-calculate UTC time if timezone provided
    IF NEW.captured_timezone IS NOT NULL AND NEW.log_time IS NOT NULL THEN
        BEGIN
            NEW.log_time_utc := NEW.log_time AT TIME ZONE NEW.captured_timezone AT TIME ZONE 'UTC';
        EXCEPTION WHEN OTHERS THEN
            -- Fallback: treat as UTC if timezone conversion fails
            NEW.log_time_utc := NEW.log_time;
        END;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_set_duplicate_detection_hash
BEFORE INSERT OR UPDATE ON hr_attendance_log
FOR EACH ROW
EXECUTE FUNCTION set_duplicate_detection_hash();

-- Comments
COMMENT ON COLUMN hr_attendance_log.device_payload_json IS 'Complete raw payload from biometric device (IMMUTABLE after lock)';
COMMENT ON COLUMN hr_attendance_log.device_payload_hash IS 'SHA256 hash of device payload for integrity verification';
COMMENT ON COLUMN hr_attendance_log.captured_timezone IS 'IANA timezone where log was captured (e.g., "Asia/Riyadh")';
COMMENT ON COLUMN hr_attendance_log.source_confidence IS 'High (biometric), Medium (card/PIN), Low (manual entry)';
COMMENT ON COLUMN hr_attendance_log.is_locked IS 'CRITICAL: When true, log is immutable and cannot be modified';
COMMENT ON COLUMN hr_attendance_log.duplicate_detection_hash IS 'Hash for detecting duplicate punches within 1 minute';
COMMENT ON COLUMN hr_attendance_log.punch_direction IS 'IN, OUT, BREAK_START, BREAK_END (for segment processing)';

-- ===========================================================================
-- MIGRATION #8: OVERTIME VERIFICATION MODEL
-- ===========================================================================
-- Purpose: Prove which hours were overtime with segment-level tracking
-- Audit Requirement: Must be able to show exact overtime period and link to raw logs
-- ===========================================================================

CREATE TABLE IF NOT EXISTS hr_overtime_segment_verification (
    -- Primary Key
    verification_id BIGSERIAL PRIMARY KEY,

    -- Employee & Date
    employee_id BIGINT NOT NULL,
    work_date DATE NOT NULL,

    -- Link to Overtime Result
    overtime_result_id BIGINT REFERENCES hr_overtime_result(overtime_result_id),

    -- Shift Segment Context
    shift_id BIGINT REFERENCES hr_shift(shift_id),
    shift_segment_id BIGINT REFERENCES hr_shift_segment(segment_id),
    segment_expected_start_time TIME,
    segment_expected_end_time TIME,

    -- Actual Work Times (PROOF)
    actual_work_start_time TIMESTAMP NOT NULL,
    actual_work_end_time TIMESTAMP NOT NULL,
    actual_worked_minutes INT NOT NULL,

    -- Overtime Period (CRITICAL: Exact overtime hours)
    overtime_start_time TIMESTAMP,
    overtime_end_time TIMESTAMP,
    overtime_minutes INT NOT NULL DEFAULT 0,
    overtime_type VARCHAR(20), -- "Daily", "Weekend", "Holiday"

    -- Proof of Work (Link to raw attendance logs)
    work_start_log_id BIGINT REFERENCES hr_attendance_log(log_id),
    work_end_log_id BIGINT REFERENCES hr_attendance_log(log_id),
    overtime_start_log_id BIGINT REFERENCES hr_attendance_log(log_id), -- When OT started
    overtime_end_log_id BIGINT REFERENCES hr_attendance_log(log_id), -- When OT ended

    -- Policy Application
    overtime_policy_id BIGINT REFERENCES hr_overtime_policy(overtime_policy_id),
    overtime_policy_version INT,
    overtime_policy_snapshot_json JSONB, -- Policy state at calculation time
    overtime_rate DECIMAL(5, 2), -- 1.5x, 2.0x, etc.

    -- Verification Status
    is_verified BOOLEAN DEFAULT false,
    verified_by VARCHAR(100),
    verified_at TIMESTAMP,
    verification_method VARCHAR(50), -- "Auto", "ManagerApproved", "Disputed", "RequiresReview"
    verification_notes TEXT,

    -- Pre-Approval Linkage
    overtime_request_id BIGINT REFERENCES hr_overtime_request(overtime_request_id),
    is_pre_approved BOOLEAN DEFAULT false,
    pre_approval_variance_minutes INT, -- Difference between requested and actual

    -- Calculation Metadata
    calculated_by VARCHAR(100),
    calculated_at TIMESTAMP DEFAULT NOW(),
    calculation_engine_version VARCHAR(20),

    -- Snapshot Linkage
    attendance_snapshot_id BIGINT REFERENCES hr_attendance_day_snapshot(snapshot_id),

    -- Audit
    created_at TIMESTAMP DEFAULT NOW(),
    created_by VARCHAR(100),

    -- Constraints
    CONSTRAINT chk_overtime_minutes_non_negative CHECK (overtime_minutes >= 0),
    CONSTRAINT chk_overtime_type CHECK (overtime_type IN ('Daily', 'Weekend', 'Holiday')),
    CONSTRAINT chk_verification_method CHECK (verification_method IN (
        'Auto', 'ManagerApproved', 'HRApproved', 'Disputed', 'RequiresReview'
    )),
    FOREIGN KEY (employee_id, work_date)
        REFERENCES hr_attendance_day(employee_id, attendance_date)
);

-- Indexes
CREATE INDEX idx_overtime_verification_employee_date
ON hr_overtime_segment_verification(employee_id, work_date);

CREATE INDEX idx_overtime_verification_result
ON hr_overtime_segment_verification(overtime_result_id);

CREATE INDEX idx_overtime_verification_unverified
ON hr_overtime_segment_verification(is_verified, verification_method)
WHERE is_verified = false;

CREATE INDEX idx_overtime_verification_snapshot
ON hr_overtime_segment_verification(attendance_snapshot_id);

-- Comments
COMMENT ON TABLE hr_overtime_segment_verification IS 'Segment-level overtime verification with proof chain to raw attendance logs';
COMMENT ON COLUMN hr_overtime_segment_verification.overtime_start_time IS 'EXACT timestamp when overtime started (proves OT period)';
COMMENT ON COLUMN hr_overtime_segment_verification.work_start_log_id IS 'Link to raw attendance log (punch in) - PROOF of work start';
COMMENT ON COLUMN hr_overtime_segment_verification.overtime_start_log_id IS 'Link to log where OT began (e.g., stayed after shift end)';
COMMENT ON COLUMN hr_overtime_segment_verification.pre_approval_variance_minutes IS 'Difference between requested OT and actual OT (for audit)';
COMMENT ON COLUMN hr_overtime_segment_verification.verification_method IS 'How OT was verified: Auto (within policy), ManagerApproved, Disputed';

-- ===========================================================================
-- MIGRATION #9: LEAVE/ATTENDANCE DECOUPLING ENHANCEMENT
-- ===========================================================================
-- Purpose: Enhance leave effect tracking for proper attendance integration
-- Audit Requirement: Leave must generate events that attendance consumes
-- ===========================================================================

ALTER TABLE hr_leave_attendance_effect
-- Enhanced Effect Tracking
ADD COLUMN IF NOT EXISTS effect_type VARCHAR(30), -- "FullDayAbsence", "PartialDayReduction", "NoEffect"
ADD COLUMN IF NOT EXISTS expected_minutes_before_leave INT,
ADD COLUMN IF NOT EXISTS expected_minutes_after_leave INT,
ADD COLUMN IF NOT EXISTS leave_start_time TIME, -- For partial day leave
ADD COLUMN IF NOT EXISTS leave_end_time TIME,

-- Event Linkage
ADD COLUMN IF NOT EXISTS generated_by_event_id BIGINT REFERENCES hr_attendance_event(event_id),

-- Processing Status
ADD COLUMN IF NOT EXISTS is_applied_to_attendance BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS applied_to_attendance_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS applied_to_snapshot_id BIGINT REFERENCES hr_attendance_day_snapshot(snapshot_id),

-- Reversal Support (when leave is cancelled)
ADD COLUMN IF NOT EXISTS is_reversed BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS reversed_at TIMESTAMP,
ADD COLUMN IF NOT EXISTS reversed_by VARCHAR(100),
ADD COLUMN IF NOT EXISTS reversal_event_id BIGINT REFERENCES hr_attendance_event(event_id);

-- Add constraints
ALTER TABLE hr_leave_attendance_effect
ADD CONSTRAINT chk_effect_type CHECK (effect_type IN ('FullDayAbsence', 'PartialDayReduction', 'NoEffect') OR effect_type IS NULL);

-- Index for processing
CREATE INDEX IF NOT EXISTS idx_leave_effect_unapplied
ON hr_leave_attendance_effect(is_applied_to_attendance, affected_date)
WHERE is_applied_to_attendance = false AND is_reversed = false;

-- Comments
COMMENT ON COLUMN hr_leave_attendance_effect.effect_type IS 'FullDayAbsence (entire day off), PartialDayReduction (half day), NoEffect (holiday leave)';
COMMENT ON COLUMN hr_leave_attendance_effect.is_applied_to_attendance IS 'TRUE when effect has been consumed by attendance processing';
COMMENT ON COLUMN hr_leave_attendance_effect.is_reversed IS 'TRUE when leave was cancelled and effect reversed';

-- ===========================================================================
-- MIGRATION #10: ATTENDANCE DAY ENHANCEMENT (Leave Integration)
-- ===========================================================================
-- Purpose: Track leave minutes in attendance day for proper calculation
-- ===========================================================================

ALTER TABLE hr_attendance_day
-- Leave Tracking
ADD COLUMN IF NOT EXISTS leave_minutes INT DEFAULT 0,
ADD COLUMN IF NOT EXISTS is_partial_day_leave BOOLEAN DEFAULT false,
ADD COLUMN IF NOT EXISTS leave_effect_ids BIGINT[], -- Array of effect IDs that affected this day

-- Adjusted Expected Minutes (expected - leave)
ADD COLUMN IF NOT EXISTS expected_minutes_original INT, -- Before leave adjustment
ADD COLUMN IF NOT EXISTS expected_minutes_adjusted INT; -- After leave adjustment

-- Add constraint
ALTER TABLE hr_attendance_day
ADD CONSTRAINT chk_leave_minutes_non_negative CHECK (leave_minutes >= 0);

-- Comments
COMMENT ON COLUMN hr_attendance_day.leave_minutes IS 'Total leave minutes for this day (from leave effects)';
COMMENT ON COLUMN hr_attendance_day.expected_minutes_original IS 'Expected work minutes before leave adjustment';
COMMENT ON COLUMN hr_attendance_day.expected_minutes_adjusted IS 'Expected work minutes after deducting leave (used for OT calculation)';
COMMENT ON COLUMN hr_attendance_day.leave_effect_ids IS 'Array of hr_leave_attendance_effect IDs that affected this day';

-- ===========================================================================
-- MIGRATION #11: MULTI-TIMEZONE SUPPORT FOUNDATION
-- ===========================================================================
-- Purpose: Add timezone tracking to all time-sensitive entities
-- ===========================================================================

-- Employee Timezone (already exists in hr_employee_core, verify and add if missing)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'hr_employee_core' AND column_name = 'work_timezone'
    ) THEN
        ALTER TABLE hr_employee_core
        ADD COLUMN work_timezone VARCHAR(50) DEFAULT 'UTC';
    END IF;
END $$;

-- Shift Timezone
ALTER TABLE hr_shift
ADD COLUMN IF NOT EXISTS shift_timezone VARCHAR(50) DEFAULT 'UTC';

-- Device Timezone (already added in Migration #7)
-- Attendance Log Timezone (already added in Migration #7)

-- Add timezone conversion helper function
CREATE OR REPLACE FUNCTION convert_to_employee_local_time(
    p_utc_time TIMESTAMP,
    p_employee_id BIGINT
) RETURNS TIMESTAMP AS $$
DECLARE
    v_timezone VARCHAR(50);
BEGIN
    -- Get employee's work timezone
    SELECT COALESCE(work_timezone, 'UTC') INTO v_timezone
    FROM hr_employee_core
    WHERE employee_id = p_employee_id;

    -- Convert UTC to local timezone
    BEGIN
        RETURN p_utc_time AT TIME ZONE 'UTC' AT TIME ZONE v_timezone;
    EXCEPTION WHEN OTHERS THEN
        -- Fallback: return UTC if conversion fails
        RETURN p_utc_time;
    END;
END;
$$ LANGUAGE plpgsql STABLE;

CREATE OR REPLACE FUNCTION convert_to_utc(
    p_local_time TIMESTAMP,
    p_timezone VARCHAR(50)
) RETURNS TIMESTAMP AS $$
BEGIN
    BEGIN
        RETURN p_local_time AT TIME ZONE p_timezone AT TIME ZONE 'UTC';
    EXCEPTION WHEN OTHERS THEN
        -- Fallback: treat as UTC if conversion fails
        RETURN p_local_time;
    END;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Comments
COMMENT ON FUNCTION convert_to_employee_local_time IS 'Convert UTC timestamp to employee local timezone';
COMMENT ON FUNCTION convert_to_utc IS 'Convert local timestamp to UTC using specified timezone';

-- ===========================================================================
-- MIGRATION #12: WEEKEND CONFIGURATION (Remove Hardcoded Rules)
-- ===========================================================================
-- Purpose: Make weekend days configurable per work calendar
-- ===========================================================================

CREATE TABLE IF NOT EXISTS hr_weekend_configuration (
    config_id BIGSERIAL PRIMARY KEY,
    calendar_id BIGINT NOT NULL REFERENCES hr_work_calendar(calendar_id),
    day_of_week INT NOT NULL, -- 0=Sunday, 1=Monday, ..., 6=Saturday
    is_weekend BOOLEAN DEFAULT true,
    is_half_day BOOLEAN DEFAULT false,
    half_day_expected_minutes INT,
    effective_from DATE,
    effective_to DATE,

    -- Audit
    created_at TIMESTAMP DEFAULT NOW(),
    created_by VARCHAR(100),

    -- Constraints
    CONSTRAINT chk_day_of_week CHECK (day_of_week BETWEEN 0 AND 6),
    UNIQUE(calendar_id, day_of_week, effective_from)
);

-- Index
CREATE INDEX idx_weekend_config_calendar
ON hr_weekend_configuration(calendar_id, effective_from, effective_to);

-- Default weekend configuration for existing calendars
INSERT INTO hr_weekend_configuration (calendar_id, day_of_week, is_weekend, created_by)
SELECT calendar_id, 5, true, 'SYSTEM' -- Friday
FROM hr_work_calendar
WHERE NOT EXISTS (
    SELECT 1 FROM hr_weekend_configuration w
    WHERE w.calendar_id = hr_work_calendar.calendar_id AND w.day_of_week = 5
)
ON CONFLICT DO NOTHING;

INSERT INTO hr_weekend_configuration (calendar_id, day_of_week, is_weekend, created_by)
SELECT calendar_id, 6, true, 'SYSTEM' -- Saturday
FROM hr_work_calendar
WHERE NOT EXISTS (
    SELECT 1 FROM hr_weekend_configuration w
    WHERE w.calendar_id = hr_work_calendar.calendar_id AND w.day_of_week = 6
)
ON CONFLICT DO NOTHING;

-- Comments
COMMENT ON TABLE hr_weekend_configuration IS 'Configurable weekend days per work calendar (replaces hardcoded Friday/Saturday)';
COMMENT ON COLUMN hr_weekend_configuration.day_of_week IS '0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday';
COMMENT ON COLUMN hr_weekend_configuration.is_half_day IS 'TRUE for half-day weekends (e.g., Friday half day in some countries)';

-- ===========================================================================
-- DATA MIGRATION: Initialize Existing Records
-- ===========================================================================

-- Set default timezones for existing records
UPDATE hr_shift
SET shift_timezone = 'UTC'
WHERE shift_timezone IS NULL;

-- Initialize leave effect types for existing records
UPDATE hr_leave_attendance_effect
SET effect_type = 'FullDayAbsence',
    is_applied_to_attendance = true
WHERE effect_type IS NULL AND deducted_minutes >= 420; -- Assuming 7+ hours = full day

UPDATE hr_leave_attendance_effect
SET effect_type = 'PartialDayReduction',
    is_applied_to_attendance = true
WHERE effect_type IS NULL AND deducted_minutes < 420;

-- Initialize leave minutes in attendance day (if data exists)
-- This is a manual reconciliation - adjust based on your data
UPDATE hr_attendance_day ad
SET leave_minutes = COALESCE((
    SELECT SUM(e.deducted_minutes)
    FROM hr_leave_attendance_effect e
    WHERE e.employee_id = ad.employee_id
    AND e.affected_date = ad.attendance_date
    AND e.is_reversed = false
), 0)
WHERE leave_minutes IS NULL;

-- ===========================================================================
-- VERIFICATION QUERIES
-- ===========================================================================

-- Verify overtime verification table created
SELECT COUNT(*) as overtime_verification_table_exists
FROM information_schema.tables
WHERE table_name = 'hr_overtime_segment_verification';
-- Expected: 1

-- Verify weekend configuration table created
SELECT COUNT(*) as weekend_config_table_exists
FROM information_schema.tables
WHERE table_name = 'hr_weekend_configuration';
-- Expected: 1

-- Verify attendance log enhancements
SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_attendance_log'
AND column_name IN ('device_payload_json', 'captured_timezone', 'is_locked', 'punch_direction');
-- Expected: 4 rows

-- Verify triggers
SELECT trigger_name
FROM information_schema.triggers
WHERE trigger_name IN (
    'trg_prevent_locked_log_modification',
    'trg_set_duplicate_detection_hash'
);
-- Expected: 2 rows

-- Verify functions
SELECT routine_name
FROM information_schema.routines
WHERE routine_name IN (
    'prevent_locked_log_modification',
    'set_duplicate_detection_hash',
    'calculate_log_duplicate_hash',
    'convert_to_employee_local_time',
    'convert_to_utc'
);
-- Expected: 5 rows

-- ===========================================================================
-- ROLLBACK SCRIPT (USE WITH CAUTION)
-- ===========================================================================

/*
-- Drop triggers
DROP TRIGGER IF EXISTS trg_prevent_locked_log_modification ON hr_attendance_log;
DROP TRIGGER IF EXISTS trg_set_duplicate_detection_hash ON hr_attendance_log;

-- Drop functions
DROP FUNCTION IF EXISTS prevent_locked_log_modification();
DROP FUNCTION IF EXISTS set_duplicate_detection_hash();
DROP FUNCTION IF EXISTS calculate_log_duplicate_hash(BIGINT, TIMESTAMP, BIGINT);
DROP FUNCTION IF EXISTS convert_to_employee_local_time(TIMESTAMP, BIGINT);
DROP FUNCTION IF EXISTS convert_to_utc(TIMESTAMP, VARCHAR);

-- Drop tables
DROP TABLE IF EXISTS hr_overtime_segment_verification CASCADE;
DROP TABLE IF EXISTS hr_weekend_configuration CASCADE;

-- Remove columns from hr_attendance_log
ALTER TABLE hr_attendance_log
DROP COLUMN IF EXISTS device_payload_json,
DROP COLUMN IF EXISTS device_payload_hash,
DROP COLUMN IF EXISTS captured_at_utc,
DROP COLUMN IF EXISTS captured_timezone,
DROP COLUMN IF EXISTS log_time_utc,
DROP COLUMN IF EXISTS timezone_offset_minutes,
DROP COLUMN IF EXISTS source_confidence,
DROP COLUMN IF EXISTS biometric_score,
DROP COLUMN IF EXISTS biometric_type,
DROP COLUMN IF EXISTS duplicate_of_log_id,
DROP COLUMN IF EXISTS is_duplicate,
DROP COLUMN IF EXISTS duplicate_detection_hash,
DROP COLUMN IF EXISTS linked_shift_segment_id,
DROP COLUMN IF EXISTS punch_direction,
DROP COLUMN IF EXISTS is_locked,
DROP COLUMN IF EXISTS locked_at,
DROP COLUMN IF EXISTS locked_by,
DROP COLUMN IF EXISTS lock_reason,
DROP COLUMN IF EXISTS latitude,
DROP COLUMN IF EXISTS longitude,
DROP COLUMN IF EXISTS location_accuracy_meters,
DROP COLUMN IF EXISTS geo_fence_name;

-- Remove columns from hr_leave_attendance_effect
ALTER TABLE hr_leave_attendance_effect
DROP COLUMN IF EXISTS effect_type,
DROP COLUMN IF EXISTS expected_minutes_before_leave,
DROP COLUMN IF EXISTS expected_minutes_after_leave,
DROP COLUMN IF EXISTS leave_start_time,
DROP COLUMN IF EXISTS leave_end_time,
DROP COLUMN IF EXISTS generated_by_event_id,
DROP COLUMN IF EXISTS is_applied_to_attendance,
DROP COLUMN IF EXISTS applied_to_attendance_at,
DROP COLUMN IF EXISTS applied_to_snapshot_id,
DROP COLUMN IF EXISTS is_reversed,
DROP COLUMN IF EXISTS reversed_at,
DROP COLUMN IF EXISTS reversed_by,
DROP COLUMN IF EXISTS reversal_event_id;

-- Remove columns from hr_attendance_day
ALTER TABLE hr_attendance_day
DROP COLUMN IF EXISTS leave_minutes,
DROP COLUMN IF EXISTS is_partial_day_leave,
DROP COLUMN IF EXISTS leave_effect_ids,
DROP COLUMN IF EXISTS expected_minutes_original,
DROP COLUMN IF EXISTS expected_minutes_adjusted;

-- Remove timezone columns
ALTER TABLE hr_shift DROP COLUMN IF EXISTS shift_timezone;
*/

-- ===========================================================================
-- END OF PHASE 2 MIGRATIONS
-- ===========================================================================
-- Next: Create enhanced domain models and services
-- ===========================================================================
