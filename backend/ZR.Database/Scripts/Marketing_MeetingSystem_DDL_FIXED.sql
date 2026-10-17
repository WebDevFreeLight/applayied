-- =====================================================
-- Meeting Management System - Database Schema (FIXED)
-- =====================================================
-- Created: 2025-12-27
-- Updated: 2025-12-27 - Fixed all errors and removed FK dependencies
-- Purpose: Generic meeting system for Leads & Customers with conflict management
-- =====================================================

-- =====================================================
-- 1. Main Meeting Table (Generic for Lead/Customer)
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_meeting (
    meeting_id BIGINT PRIMARY KEY,

    -- Generic entity reference (Lead or Customer)
    entity_type VARCHAR(20) NOT NULL,
    entity_id BIGINT NOT NULL,

    -- Meeting details
    title VARCHAR(200) NOT NULL,
    description TEXT,

    -- Timing
    start_datetime TIMESTAMP NOT NULL,
    end_datetime TIMESTAMP NOT NULL,
    timezone VARCHAR(50) DEFAULT 'Asia/Riyadh',

    -- Meeting format
    meeting_type VARCHAR(20) NOT NULL,
    meeting_channel VARCHAR(30),

    -- Location/Link based on type
    location VARCHAR(500),
    meeting_url VARCHAR(500),

    -- Status tracking
    status VARCHAR(20) NOT NULL DEFAULT 'SCHEDULED',
    cancel_reason VARCHAR(500),

    -- Audit fields
    created_by BIGINT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by BIGINT,
    updated_at TIMESTAMP,
    companyid BIGINT DEFAULT 0,
    branchid BIGINT DEFAULT 0,

    -- Constraints
    CONSTRAINT chk_bp_meeting_entity_type CHECK (entity_type IN ('LEAD', 'CUSTOMER')),
    CONSTRAINT chk_bp_meeting_type CHECK (meeting_type IN ('ONSITE', 'ONLINE', 'CALL')),
    CONSTRAINT chk_bp_meeting_channel CHECK (meeting_channel IN ('ZOOM', 'GOOGLE_MEET', 'TEAMS', 'PHONE', 'OFFICE', 'OTHER')),
    CONSTRAINT chk_bp_meeting_status CHECK (status IN ('SCHEDULED', 'COMPLETED', 'CANCELLED', 'NO_SHOW')),
    CONSTRAINT chk_bp_meeting_dates CHECK (end_datetime > start_datetime)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_bp_meeting_entity ON bp_meeting(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_dates ON bp_meeting(start_datetime, end_datetime);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_status ON bp_meeting(status);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_created_by ON bp_meeting(created_by);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_timeline ON bp_meeting(start_datetime DESC) WHERE status = 'SCHEDULED';

-- Comments
COMMENT ON TABLE bp_meeting IS 'Generic meeting table for Leads and Customers';
COMMENT ON COLUMN bp_meeting.entity_type IS 'ENUM: LEAD, CUSTOMER';
COMMENT ON COLUMN bp_meeting.entity_id IS 'ID of Lead (bp_lead) or Customer (bp_businesspartner)';
COMMENT ON COLUMN bp_meeting.meeting_type IS 'ENUM: ONSITE, ONLINE, CALL';
COMMENT ON COLUMN bp_meeting.meeting_channel IS 'ENUM: ZOOM, GOOGLE_MEET, TEAMS, PHONE, OFFICE, OTHER';
COMMENT ON COLUMN bp_meeting.status IS 'ENUM: SCHEDULED, COMPLETED, CANCELLED, NO_SHOW';
COMMENT ON COLUMN bp_meeting.timezone IS 'Timezone for proper datetime handling';

-- =====================================================
-- 2. Meeting Participants (Many-to-Many)
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_meeting_participant (
    participant_id BIGINT PRIMARY KEY,
    meeting_id BIGINT NOT NULL,
    employee_id BIGINT NOT NULL,

    -- Role and attendance
    role VARCHAR(20) NOT NULL DEFAULT 'ATTENDEE',
    attendance_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',

    -- Response tracking
    response_datetime TIMESTAMP,
    notes TEXT,

    -- Audit fields
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_participant_role CHECK (role IN ('ORGANIZER', 'ATTENDEE', 'OBSERVER')),
    CONSTRAINT chk_bp_meeting_participant_status CHECK (attendance_status IN ('PENDING', 'ACCEPTED', 'DECLINED', 'ATTENDED', 'ABSENT')),

    -- Unique constraint: One employee cannot be added twice to same meeting
    CONSTRAINT uk_bp_meeting_participant UNIQUE (meeting_id, employee_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_bp_meeting_participant_meeting ON bp_meeting_participant(meeting_id);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_participant_employee ON bp_meeting_participant(employee_id);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_participant_organizer ON bp_meeting_participant(meeting_id, role) WHERE role = 'ORGANIZER';

-- Comments
COMMENT ON TABLE bp_meeting_participant IS 'Meeting participants with roles and attendance tracking';
COMMENT ON COLUMN bp_meeting_participant.role IS 'ENUM: ORGANIZER, ATTENDEE, OBSERVER';
COMMENT ON COLUMN bp_meeting_participant.attendance_status IS 'ENUM: PENDING, ACCEPTED, DECLINED, ATTENDED, ABSENT';

-- =====================================================
-- 3. Meeting Conflict Lock (Explicit Conflict Prevention)
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_meeting_conflict_lock (
    lock_id BIGINT PRIMARY KEY,
    meeting_id BIGINT NOT NULL,

    -- Time range being locked
    start_datetime TIMESTAMP NOT NULL,
    end_datetime TIMESTAMP NOT NULL,

    -- Scope of lock
    scope VARCHAR(20) NOT NULL,
    scope_id BIGINT,

    -- Audit
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_conflict_lock_scope CHECK (scope IN ('COMPANY', 'EMPLOYEE', 'ENTITY')),
    CONSTRAINT chk_bp_meeting_conflict_lock_dates CHECK (end_datetime > start_datetime)
);

-- Indexes for fast conflict detection
CREATE INDEX IF NOT EXISTS idx_bp_meeting_conflict_lock_scope ON bp_meeting_conflict_lock(scope, scope_id);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_conflict_lock_dates ON bp_meeting_conflict_lock(start_datetime, end_datetime);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_conflict_lock_meeting ON bp_meeting_conflict_lock(meeting_id);

-- Composite index for overlap detection
CREATE INDEX IF NOT EXISTS idx_bp_meeting_conflict_overlap ON bp_meeting_conflict_lock(scope, scope_id, start_datetime, end_datetime);

-- Comments
COMMENT ON TABLE bp_meeting_conflict_lock IS 'Explicit conflict locks to prevent double booking';
COMMENT ON COLUMN bp_meeting_conflict_lock.scope IS 'ENUM: COMPANY (all employees busy), EMPLOYEE (specific employee), ENTITY (specific lead/customer)';
COMMENT ON COLUMN bp_meeting_conflict_lock.scope_id IS 'NULL for COMPANY scope, employee_id for EMPLOYEE, entity_id for ENTITY';

-- =====================================================
-- 4. Meeting Outcome (Post-Meeting Results)
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_meeting_outcome (
    outcome_id BIGINT PRIMARY KEY,
    meeting_id BIGINT NOT NULL,

    -- Outcome classification
    outcome_type VARCHAR(20) NOT NULL,
    notes TEXT,

    -- Next actions
    next_action VARCHAR(30),
    next_action_date TIMESTAMP,
    next_action_assignee BIGINT,

    -- Audit
    recorded_by BIGINT,
    recorded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_outcome_type CHECK (outcome_type IN ('POSITIVE', 'NEUTRAL', 'NEGATIVE')),
    CONSTRAINT chk_bp_meeting_outcome_action CHECK (next_action IN ('FOLLOW_UP', 'PROPOSAL', 'CLOSE', 'SCHEDULE_MEETING', 'SEND_INFO', 'NONE'))
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_bp_meeting_outcome_meeting ON bp_meeting_outcome(meeting_id);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_outcome_type ON bp_meeting_outcome(outcome_type);
CREATE INDEX IF NOT EXISTS idx_bp_meeting_outcome_next_action ON bp_meeting_outcome(next_action_date) WHERE next_action IS NOT NULL;

-- Comments
COMMENT ON TABLE bp_meeting_outcome IS 'Post-meeting outcomes and follow-up actions';
COMMENT ON COLUMN bp_meeting_outcome.outcome_type IS 'ENUM: POSITIVE, NEUTRAL, NEGATIVE';
COMMENT ON COLUMN bp_meeting_outcome.next_action IS 'ENUM: FOLLOW_UP, PROPOSAL, CLOSE, SCHEDULE_MEETING, SEND_INFO, NONE';

-- =====================================================
-- 5. Helper Functions
-- =====================================================

-- Function: Check if time slot conflicts with existing locks
CREATE OR REPLACE FUNCTION check_meeting_conflict(
    p_scope VARCHAR,
    p_scope_id BIGINT,
    p_start_datetime TIMESTAMP,
    p_end_datetime TIMESTAMP,
    p_exclude_meeting_id BIGINT DEFAULT NULL
) RETURNS BOOLEAN AS $$
DECLARE
    conflict_count INTEGER;
BEGIN
    SELECT COUNT(*)
    INTO conflict_count
    FROM bp_meeting_conflict_lock
    WHERE scope = p_scope
      AND (p_scope_id IS NULL OR scope_id = p_scope_id OR scope_id IS NULL)
      AND start_datetime < p_end_datetime
      AND end_datetime > p_start_datetime
      AND (p_exclude_meeting_id IS NULL OR meeting_id != p_exclude_meeting_id);

    RETURN conflict_count > 0;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION check_meeting_conflict IS 'Returns TRUE if time slot conflicts with existing locks';

-- =====================================================
-- 6. Triggers for Auto-Lock Management
-- =====================================================

-- Trigger function: Auto-create conflict locks when meeting is created/updated
CREATE OR REPLACE FUNCTION trg_auto_create_meeting_locks()
RETURNS TRIGGER AS $$
BEGIN
    -- Only create locks for SCHEDULED meetings
    IF NEW.status = 'SCHEDULED' THEN
        -- Delete old locks if updating
        IF TG_OP = 'UPDATE' THEN
            DELETE FROM bp_meeting_conflict_lock WHERE meeting_id = NEW.meeting_id;
        END IF;

        -- Create entity lock (prevent lead/customer from having overlapping meetings)
        INSERT INTO bp_meeting_conflict_lock (
            lock_id,
            meeting_id,
            start_datetime,
            end_datetime,
            scope,
            scope_id
        ) VALUES (
            (SELECT COALESCE(MAX(lock_id), 0) + 1 FROM bp_meeting_conflict_lock),
            NEW.meeting_id,
            NEW.start_datetime,
            NEW.end_datetime,
            'ENTITY',
            NEW.entity_id
        );

        -- Create employee locks for all participants (inserted separately)
        -- This will be handled in application layer to avoid circular dependency
    END IF;

    -- Remove locks if meeting is cancelled/completed
    IF NEW.status IN ('CANCELLED', 'COMPLETED') THEN
        DELETE FROM bp_meeting_conflict_lock WHERE meeting_id = NEW.meeting_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Drop trigger if exists (to avoid errors)
DROP TRIGGER IF EXISTS trg_meeting_auto_lock ON bp_meeting;

-- Create trigger
CREATE TRIGGER trg_meeting_auto_lock
AFTER INSERT OR UPDATE ON bp_meeting
FOR EACH ROW
EXECUTE FUNCTION trg_auto_create_meeting_locks();

COMMENT ON TRIGGER trg_meeting_auto_lock ON bp_meeting IS 'Auto-creates/removes conflict locks based on meeting status';

-- =====================================================
-- 7. Views for Reporting (Optional - will work if tables exist)
-- =====================================================

-- View: Upcoming Meetings with Participants
-- NOTE: This view requires bp_lead and bp_businesspartner tables
-- If those tables don't exist yet, comment out this section

DO $$
BEGIN
    -- Check if bp_lead table exists
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'bp_lead') THEN
        EXECUTE '
        CREATE OR REPLACE VIEW v_upcoming_meetings AS
        SELECT
            m.meeting_id,
            m.entity_type,
            m.entity_id,
            CASE
                WHEN m.entity_type = ''LEAD'' THEN l.brand_name
                WHEN m.entity_type = ''CUSTOMER'' THEN bp.partner_name
            END AS entity_name,
            m.title,
            m.start_datetime,
            m.end_datetime,
            m.meeting_type,
            m.meeting_channel,
            m.status,
            m.location,
            m.meeting_url,

            -- Organizer info
            COALESCE(org_pn.given_name || '' '' || org_pn.family_name, ''Unknown'') AS organizer_name,

            -- Participant count
            COUNT(DISTINCT p.employee_id) AS participant_count,

            -- Time until meeting
            EXTRACT(EPOCH FROM (m.start_datetime - CURRENT_TIMESTAMP))/3600 AS hours_until_meeting

        FROM bp_meeting m
        LEFT JOIN bp_lead l ON m.entity_type = ''LEAD'' AND m.entity_id = l.lead_id
        LEFT JOIN bp_businesspartner bp ON m.entity_type = ''CUSTOMER'' AND m.entity_id = bp.partner_id
        LEFT JOIN bp_meeting_participant p ON m.meeting_id = p.meeting_id
        LEFT JOIN bp_meeting_participant org ON m.meeting_id = org.meeting_id AND org.role = ''ORGANIZER''
        LEFT JOIN hr_employee_core org_emp ON org.employee_id = org_emp.employee_id
        LEFT JOIN hr_person_name org_pn ON org_emp.person_id = org_pn.person_id AND org_pn.is_primary = TRUE

        WHERE m.status = ''SCHEDULED''
          AND m.start_datetime > CURRENT_TIMESTAMP

        GROUP BY
            m.meeting_id, m.entity_type, m.entity_id, l.brand_name, bp.partner_name,
            m.title, m.start_datetime, m.end_datetime, m.meeting_type, m.meeting_channel,
            m.status, m.location, m.meeting_url, org_pn.given_name, org_pn.family_name

        ORDER BY m.start_datetime ASC
        ';
    END IF;
END $$;

-- =====================================================
-- End of Migration Script
-- =====================================================
