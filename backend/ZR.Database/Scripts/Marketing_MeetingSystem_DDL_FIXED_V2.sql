-- =====================================================
-- Meeting Management System - Database Schema (FIXED V2)
-- =====================================================
-- Created: 2025-12-27
-- Updated: 2025-12-27 - Fixed ALL column names to match PostgreSQL lowercase
-- Purpose: Generic meeting system for Leads & Customers with conflict management
-- =====================================================

-- Drop existing tables if they exist (for clean reinstall)
DROP TABLE IF EXISTS bp_meeting_outcome CASCADE;
DROP TABLE IF EXISTS bp_meeting_conflict_lock CASCADE;
DROP TABLE IF EXISTS bp_meeting_participant CASCADE;
DROP TABLE IF EXISTS bp_meeting CASCADE;

-- =====================================================
-- 1. Main Meeting Table (Generic for Lead/Customer)
-- =====================================================

CREATE TABLE bp_meeting (
    meetingid BIGINT PRIMARY KEY,

    -- Generic entity reference (Lead or Customer)
    entitytype VARCHAR(20) NOT NULL,
    entityid BIGINT NOT NULL,

    -- Meeting details
    title VARCHAR(200) NOT NULL,
    description TEXT,

    -- Timing
    startdatetime TIMESTAMP NOT NULL,
    enddatetime TIMESTAMP NOT NULL,
    timezone VARCHAR(50) DEFAULT 'Asia/Riyadh',

    -- Meeting format
    meetingtype VARCHAR(20) NOT NULL,
    meetingchannel VARCHAR(30),

    -- Location/Link based on type
    location VARCHAR(500),
    meetingurl VARCHAR(500),

    -- Status tracking
    status VARCHAR(20) NOT NULL DEFAULT 'SCHEDULED',
    cancelreason VARCHAR(500),

    -- Audit fields
    createdby BIGINT NOT NULL,
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedby BIGINT,
    updatedat TIMESTAMP,
    companyid BIGINT DEFAULT 0,
    branchid BIGINT DEFAULT 0,

    -- Constraints
    CONSTRAINT chk_bp_meeting_entity_type CHECK (entitytype IN ('LEAD', 'CUSTOMER')),
    CONSTRAINT chk_bp_meeting_type CHECK (meetingtype IN ('ONSITE', 'ONLINE', 'CALL')),
    CONSTRAINT chk_bp_meeting_channel CHECK (meetingchannel IN ('ZOOM', 'GOOGLE_MEET', 'TEAMS', 'PHONE', 'OFFICE', 'OTHER')),
    CONSTRAINT chk_bp_meeting_status CHECK (status IN ('SCHEDULED', 'COMPLETED', 'CANCELLED', 'NO_SHOW')),
    CONSTRAINT chk_bp_meeting_dates CHECK (enddatetime > startdatetime)
);

-- Indexes for performance
CREATE INDEX idx_bp_meeting_entity ON bp_meeting(entitytype, entityid);
CREATE INDEX idx_bp_meeting_dates ON bp_meeting(startdatetime, enddatetime);
CREATE INDEX idx_bp_meeting_status ON bp_meeting(status);
CREATE INDEX idx_bp_meeting_created_by ON bp_meeting(createdby);
CREATE INDEX idx_bp_meeting_timeline ON bp_meeting(startdatetime DESC) WHERE status = 'SCHEDULED';

-- Comments
COMMENT ON TABLE bp_meeting IS 'Generic meeting table for Leads and Customers';
COMMENT ON COLUMN bp_meeting.entitytype IS 'ENUM: LEAD, CUSTOMER';
COMMENT ON COLUMN bp_meeting.entityid IS 'ID of Lead (bp_lead) or Customer (bp_businesspartner)';
COMMENT ON COLUMN bp_meeting.meetingtype IS 'ENUM: ONSITE, ONLINE, CALL';
COMMENT ON COLUMN bp_meeting.meetingchannel IS 'ENUM: ZOOM, GOOGLE_MEET, TEAMS, PHONE, OFFICE, OTHER';
COMMENT ON COLUMN bp_meeting.status IS 'ENUM: SCHEDULED, COMPLETED, CANCELLED, NO_SHOW';
COMMENT ON COLUMN bp_meeting.timezone IS 'Timezone for proper datetime handling';

-- =====================================================
-- 2. Meeting Participants (Many-to-Many)
-- =====================================================

CREATE TABLE bp_meeting_participant (
    participantid BIGINT PRIMARY KEY,
    meetingid BIGINT NOT NULL,
    employeeid BIGINT NOT NULL,

    -- Role and attendance
    role VARCHAR(20) NOT NULL DEFAULT 'ATTENDEE',
    attendancestatus VARCHAR(20) NOT NULL DEFAULT 'PENDING',

    -- Response tracking
    responsedatetime TIMESTAMP,
    notes TEXT,

    -- Audit fields
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_participant_role CHECK (role IN ('ORGANIZER', 'ATTENDEE', 'OBSERVER')),
    CONSTRAINT chk_bp_meeting_participant_status CHECK (attendancestatus IN ('PENDING', 'ACCEPTED', 'DECLINED', 'ATTENDED', 'ABSENT')),

    -- Unique constraint: One employee cannot be added twice to same meeting
    CONSTRAINT uk_bp_meeting_participant UNIQUE (meetingid, employeeid)
);

-- Indexes
CREATE INDEX idx_bp_meeting_participant_meeting ON bp_meeting_participant(meetingid);
CREATE INDEX idx_bp_meeting_participant_employee ON bp_meeting_participant(employeeid);
CREATE INDEX idx_bp_meeting_participant_organizer ON bp_meeting_participant(meetingid, role) WHERE role = 'ORGANIZER';

-- Comments
COMMENT ON TABLE bp_meeting_participant IS 'Meeting participants with roles and attendance tracking';
COMMENT ON COLUMN bp_meeting_participant.role IS 'ENUM: ORGANIZER, ATTENDEE, OBSERVER';
COMMENT ON COLUMN bp_meeting_participant.attendancestatus IS 'ENUM: PENDING, ACCEPTED, DECLINED, ATTENDED, ABSENT';

-- =====================================================
-- 3. Meeting Conflict Lock (Explicit Conflict Prevention)
-- =====================================================

CREATE TABLE bp_meeting_conflict_lock (
    lockid BIGINT PRIMARY KEY,
    meetingid BIGINT NOT NULL,

    -- Time range being locked
    startdatetime TIMESTAMP NOT NULL,
    enddatetime TIMESTAMP NOT NULL,

    -- Scope of lock
    scope VARCHAR(20) NOT NULL,
    scopeid BIGINT,

    -- Audit
    createdat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_conflict_lock_scope CHECK (scope IN ('COMPANY', 'EMPLOYEE', 'ENTITY')),
    CONSTRAINT chk_bp_meeting_conflict_lock_dates CHECK (enddatetime > startdatetime)
);

-- Indexes for fast conflict detection
CREATE INDEX idx_bp_meeting_conflict_lock_scope ON bp_meeting_conflict_lock(scope, scopeid);
CREATE INDEX idx_bp_meeting_conflict_lock_dates ON bp_meeting_conflict_lock(startdatetime, enddatetime);
CREATE INDEX idx_bp_meeting_conflict_lock_meeting ON bp_meeting_conflict_lock(meetingid);

-- Composite index for overlap detection
CREATE INDEX idx_bp_meeting_conflict_overlap ON bp_meeting_conflict_lock(scope, scopeid, startdatetime, enddatetime);

-- Comments
COMMENT ON TABLE bp_meeting_conflict_lock IS 'Explicit conflict locks to prevent double booking';
COMMENT ON COLUMN bp_meeting_conflict_lock.scope IS 'ENUM: COMPANY (all employees busy), EMPLOYEE (specific employee), ENTITY (specific lead/customer)';
COMMENT ON COLUMN bp_meeting_conflict_lock.scopeid IS 'NULL for COMPANY scope, employeeid for EMPLOYEE, entityid for ENTITY';

-- =====================================================
-- 4. Meeting Outcome (Post-Meeting Results)
-- =====================================================

CREATE TABLE bp_meeting_outcome (
    outcomeid BIGINT PRIMARY KEY,
    meetingid BIGINT NOT NULL,

    -- Outcome classification
    outcometype VARCHAR(20) NOT NULL,
    notes TEXT,

    -- Next actions
    nextaction VARCHAR(30),
    nextactiondate TIMESTAMP,
    nextactionassignee BIGINT,

    -- Audit
    recordedby BIGINT,
    recordedat TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedat TIMESTAMP,

    -- Constraints
    CONSTRAINT chk_bp_meeting_outcome_type CHECK (outcometype IN ('POSITIVE', 'NEUTRAL', 'NEGATIVE')),
    CONSTRAINT chk_bp_meeting_outcome_action CHECK (nextaction IN ('FOLLOW_UP', 'PROPOSAL', 'CLOSE', 'SCHEDULE_MEETING', 'SEND_INFO', 'NONE'))
);

-- Indexes
CREATE INDEX idx_bp_meeting_outcome_meeting ON bp_meeting_outcome(meetingid);
CREATE INDEX idx_bp_meeting_outcome_type ON bp_meeting_outcome(outcometype);
CREATE INDEX idx_bp_meeting_outcome_next_action ON bp_meeting_outcome(nextactiondate) WHERE nextaction IS NOT NULL;

-- Comments
COMMENT ON TABLE bp_meeting_outcome IS 'Post-meeting outcomes and follow-up actions';
COMMENT ON COLUMN bp_meeting_outcome.outcometype IS 'ENUM: POSITIVE, NEUTRAL, NEGATIVE';
COMMENT ON COLUMN bp_meeting_outcome.nextaction IS 'ENUM: FOLLOW_UP, PROPOSAL, CLOSE, SCHEDULE_MEETING, SEND_INFO, NONE';

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
      AND (p_scope_id IS NULL OR scopeid = p_scope_id OR scopeid IS NULL)
      AND startdatetime < p_end_datetime
      AND enddatetime > p_start_datetime
      AND (p_exclude_meeting_id IS NULL OR meetingid != p_exclude_meeting_id);

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
            DELETE FROM bp_meeting_conflict_lock WHERE meetingid = NEW.meetingid;
        END IF;

        -- Create entity lock (prevent lead/customer from having overlapping meetings)
        INSERT INTO bp_meeting_conflict_lock (
            lockid,
            meetingid,
            startdatetime,
            enddatetime,
            scope,
            scopeid
        ) VALUES (
            (SELECT COALESCE(MAX(lockid), 0) + 1 FROM bp_meeting_conflict_lock),
            NEW.meetingid,
            NEW.startdatetime,
            NEW.enddatetime,
            'ENTITY',
            NEW.entityid
        );
    END IF;

    -- Remove locks if meeting is cancelled/completed
    IF NEW.status IN ('CANCELLED', 'COMPLETED') THEN
        DELETE FROM bp_meeting_conflict_lock WHERE meetingid = NEW.meetingid;
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
-- 7. Views for Reporting
-- =====================================================

-- View: Upcoming Meetings with Participants
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'bp_lead') THEN
        EXECUTE '
        CREATE OR REPLACE VIEW v_upcoming_meetings AS
        SELECT
            m.meetingid,
            m.entitytype,
            m.entityid,
            CASE
                WHEN m.entitytype = ''LEAD'' THEN l.brandname
                WHEN m.entitytype = ''CUSTOMER'' THEN bp.commercialname
            END AS entity_name,
            m.title,
            m.startdatetime,
            m.enddatetime,
            m.meetingtype,
            m.meetingchannel,
            m.status,
            m.location,
            m.meetingurl,

            -- Organizer info
            COALESCE(org_pn.given_name || '' '' || org_pn.family_name, ''Unknown'') AS organizer_name,

            -- Participant count
            COUNT(DISTINCT p.employeeid) AS participant_count,

            -- Time until meeting
            EXTRACT(EPOCH FROM (m.startdatetime - CURRENT_TIMESTAMP))/3600 AS hours_until_meeting

        FROM bp_meeting m
        LEFT JOIN bp_lead l ON m.entitytype = ''LEAD'' AND m.entityid = l.leadid
        LEFT JOIN bp_businesspartner bp ON m.entitytype = ''CUSTOMER'' AND m.entityid = bp.partnerid
        LEFT JOIN bp_meeting_participant p ON m.meetingid = p.meetingid
        LEFT JOIN bp_meeting_participant org ON m.meetingid = org.meetingid AND org.role = ''ORGANIZER''
        LEFT JOIN hr_employee_core org_emp ON org.employeeid = org_emp.employee_id
        LEFT JOIN hr_person_name org_pn ON org_emp.person_id = org_pn.person_id AND org_pn.is_primary = TRUE

        WHERE m.status = ''SCHEDULED''
          AND m.startdatetime > CURRENT_TIMESTAMP

        GROUP BY
            m.meetingid, m.entitytype, m.entityid, l.brandname, bp.commercialname,
            m.title, m.startdatetime, m.enddatetime, m.meetingtype, m.meetingchannel,
            m.status, m.location, m.meetingurl, org_pn.given_name, org_pn.family_name

        ORDER BY m.startdatetime ASC
        ';
    END IF;
END $$;

-- =====================================================
-- End of Migration Script
-- =====================================================
