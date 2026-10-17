-- =====================================================
-- Marketing Lead Management - Database Schema
-- =====================================================
-- Created: 2025-12-27
-- Purpose: Lead tracking, activity logging, assignment history, and meeting scheduling
-- =====================================================

-- =====================================================
-- 1. Enhance Existing bp_lead Table
-- =====================================================

-- Add missing fields to bp_lead table
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS lead_source VARCHAR(100);
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS budget_range VARCHAR(50);
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS first_contact_date TIMESTAMP;
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS last_contact_date TIMESTAMP;
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS priority VARCHAR(20) DEFAULT 'Medium';
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS expected_revenue DECIMAL(18,2);
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS lost_reason VARCHAR(200);

-- Add comments
COMMENT ON COLUMN bp_lead.lead_source IS 'Lead source: Website, Referral, Social Media, Cold Call, Event, etc.';
COMMENT ON COLUMN bp_lead.budget_range IS 'Estimated budget range for qualification';
COMMENT ON COLUMN bp_lead.first_contact_date IS 'Date of first contact with lead';
COMMENT ON COLUMN bp_lead.last_contact_date IS 'Date of last interaction with lead';
COMMENT ON COLUMN bp_lead.priority IS 'Priority: Low, Medium, High, Urgent';
COMMENT ON COLUMN bp_lead.expected_revenue IS 'Expected revenue from conversion (non-binding estimate)';
COMMENT ON COLUMN bp_lead.lost_reason IS 'Reason for losing the lead (if status=Lost)';

-- =====================================================
-- 2. Lead Activity Tracking Table
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_lead_activity (
    activity_id BIGINT PRIMARY KEY,
    lead_id BIGINT NOT NULL,
    employee_id BIGINT NOT NULL,
    activity_type VARCHAR(50) NOT NULL,
    activity_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    duration_minutes INT,
    outcome VARCHAR(50),
    notes TEXT,
    next_follow_up_date TIMESTAMP,
    is_automated BOOLEAN DEFAULT FALSE,

    -- Audit fields
    create_by VARCHAR(64),
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_by VARCHAR(64),
    update_time TIMESTAMP,
    remark VARCHAR(500),
    companyid BIGINT DEFAULT 0,
    branchid BIGINT DEFAULT 0,

    -- Foreign keys
    CONSTRAINT fk_bp_lead_activity_lead FOREIGN KEY (lead_id) REFERENCES bp_lead(lead_id) ON DELETE CASCADE,
    CONSTRAINT fk_bp_lead_activity_employee FOREIGN KEY (employee_id) REFERENCES hr_employee_core(employee_id)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_bp_lead_activity_lead ON bp_lead_activity(lead_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_activity_employee ON bp_lead_activity(employee_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_activity_date ON bp_lead_activity(activity_date DESC);
CREATE INDEX IF NOT EXISTS idx_bp_lead_activity_type ON bp_lead_activity(activity_type);

-- Add comments
COMMENT ON TABLE bp_lead_activity IS 'Tracks all activities (calls, meetings, emails, follow-ups) between marketing employees and leads';
COMMENT ON COLUMN bp_lead_activity.activity_type IS 'Activity type: Call, Meeting, Email, SMS, WhatsApp, Follow-up, Note, etc.';
COMMENT ON COLUMN bp_lead_activity.outcome IS 'Outcome: Successful, No Answer, Voicemail, Scheduled Meeting, Not Interested, etc.';
COMMENT ON COLUMN bp_lead_activity.duration_minutes IS 'Duration of the activity in minutes';
COMMENT ON COLUMN bp_lead_activity.next_follow_up_date IS 'Scheduled date for next follow-up';
COMMENT ON COLUMN bp_lead_activity.is_automated IS 'True if activity was automated (e.g., automated email)';

-- =====================================================
-- 3. Lead Assignment History Table
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_lead_assignment_log (
    assignment_log_id BIGINT PRIMARY KEY,
    lead_id BIGINT NOT NULL,
    from_employee_id BIGINT,
    to_employee_id BIGINT NOT NULL,
    assignment_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reason VARCHAR(500),
    reassignment_type VARCHAR(50),

    -- Audit fields
    create_by VARCHAR(64),
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    remark VARCHAR(500),
    companyid BIGINT DEFAULT 0,
    branchid BIGINT DEFAULT 0,

    -- Foreign keys
    CONSTRAINT fk_bp_lead_assignment_log_lead FOREIGN KEY (lead_id) REFERENCES bp_lead(lead_id) ON DELETE CASCADE,
    CONSTRAINT fk_bp_lead_assignment_log_from_emp FOREIGN KEY (from_employee_id) REFERENCES hr_employee_core(employee_id),
    CONSTRAINT fk_bp_lead_assignment_log_to_emp FOREIGN KEY (to_employee_id) REFERENCES hr_employee_core(employee_id)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_bp_lead_assignment_log_lead ON bp_lead_assignment_log(lead_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_assignment_log_from_emp ON bp_lead_assignment_log(from_employee_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_assignment_log_to_emp ON bp_lead_assignment_log(to_employee_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_assignment_log_date ON bp_lead_assignment_log(assignment_date DESC);

-- Add comments
COMMENT ON TABLE bp_lead_assignment_log IS 'Tracks lead reassignments and workload distribution history';
COMMENT ON COLUMN bp_lead_assignment_log.from_employee_id IS 'Previous owner (NULL if initial assignment)';
COMMENT ON COLUMN bp_lead_assignment_log.to_employee_id IS 'New owner';
COMMENT ON COLUMN bp_lead_assignment_log.reassignment_type IS 'Type: Initial, Workload_Balance, Performance, Request, Auto, etc.';
COMMENT ON COLUMN bp_lead_assignment_log.reason IS 'Reason for reassignment';

-- =====================================================
-- 4. Lead Meeting / Appointment Scheduling Table
-- =====================================================

CREATE TABLE IF NOT EXISTS bp_lead_meeting (
    meeting_id BIGINT PRIMARY KEY,
    lead_id BIGINT NOT NULL,
    assigned_employee_id BIGINT NOT NULL,
    meeting_title VARCHAR(200) NOT NULL,
    meeting_type VARCHAR(50) NOT NULL,
    meeting_status VARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    start_datetime TIMESTAMP NOT NULL,
    end_datetime TIMESTAMP NOT NULL,
    location VARCHAR(500),
    meeting_link VARCHAR(500),
    attendees TEXT,
    agenda TEXT,
    meeting_notes TEXT,
    outcome VARCHAR(500),
    cancelled_reason VARCHAR(500),
    cancelled_by VARCHAR(64),
    cancelled_at TIMESTAMP,

    -- Audit fields
    create_by VARCHAR(64),
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_by VARCHAR(64),
    update_time TIMESTAMP,
    remark VARCHAR(500),
    companyid BIGINT DEFAULT 0,
    branchid BIGINT DEFAULT 0,

    -- Foreign keys
    CONSTRAINT fk_bp_lead_meeting_lead FOREIGN KEY (lead_id) REFERENCES bp_lead(lead_id) ON DELETE CASCADE,
    CONSTRAINT fk_bp_lead_meeting_employee FOREIGN KEY (assigned_employee_id) REFERENCES hr_employee_core(employee_id),

    -- Business rule constraints
    CONSTRAINT chk_bp_lead_meeting_dates CHECK (end_datetime > start_datetime),
    CONSTRAINT chk_bp_lead_meeting_status CHECK (meeting_status IN ('Scheduled', 'InProgress', 'Completed', 'Cancelled', 'NoShow'))
);

-- Create indexes for performance and conflict detection
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_lead ON bp_lead_meeting(lead_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_employee ON bp_lead_meeting(assigned_employee_id);
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_start ON bp_lead_meeting(start_datetime);
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_end ON bp_lead_meeting(end_datetime);
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_status ON bp_lead_meeting(meeting_status);
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_date_range ON bp_lead_meeting(start_datetime, end_datetime);

-- Create composite index for overlap detection
CREATE INDEX IF NOT EXISTS idx_bp_lead_meeting_overlap_check ON bp_lead_meeting(assigned_employee_id, start_datetime, end_datetime)
    WHERE meeting_status NOT IN ('Cancelled', 'Completed');

-- Add comments
COMMENT ON TABLE bp_lead_meeting IS 'Calendar-based meeting/appointment scheduling with conflict detection';
COMMENT ON COLUMN bp_lead_meeting.meeting_type IS 'Meeting type: Online, Onsite, Phone_Call, Video_Conference, etc.';
COMMENT ON COLUMN bp_lead_meeting.meeting_status IS 'Status: Scheduled, InProgress, Completed, Cancelled, NoShow';
COMMENT ON COLUMN bp_lead_meeting.start_datetime IS 'Meeting start date and time';
COMMENT ON COLUMN bp_lead_meeting.end_datetime IS 'Meeting end date and time';
COMMENT ON COLUMN bp_lead_meeting.location IS 'Physical location for onsite meetings';
COMMENT ON COLUMN bp_lead_meeting.meeting_link IS 'Online meeting link (Zoom, Teams, etc.)';
COMMENT ON COLUMN bp_lead_meeting.attendees IS 'JSON or comma-separated list of attendee IDs';
COMMENT ON COLUMN bp_lead_meeting.outcome IS 'Meeting outcome/result';

-- =====================================================
-- 5. Additional Indexes for Performance
-- =====================================================

-- Lead performance indexes
CREATE INDEX IF NOT EXISTS idx_bp_lead_assigned_to ON bp_lead(assigned_to);
CREATE INDEX IF NOT EXISTS idx_bp_lead_status ON bp_lead(lead_status);
CREATE INDEX IF NOT EXISTS idx_bp_lead_source ON bp_lead(lead_source);
CREATE INDEX IF NOT EXISTS idx_bp_lead_conversion_date ON bp_lead(conversion_date);
CREATE INDEX IF NOT EXISTS idx_bp_lead_create_time ON bp_lead(create_time DESC);

-- Composite index for employee performance queries
CREATE INDEX IF NOT EXISTS idx_bp_lead_emp_status ON bp_lead(assigned_to, lead_status);

-- =====================================================
-- 6. Views for Reporting
-- =====================================================

-- View: Employee Lead Performance Summary
CREATE OR REPLACE VIEW v_marketing_employee_performance AS
SELECT
    e.employee_id,
    e.employee_code,
    COALESCE(pn.given_name, '') || ' ' || COALESCE(pn.family_name, '') AS employee_name,

    -- Lead counts by status
    COUNT(DISTINCT l.lead_id) AS total_leads,
    COUNT(DISTINCT CASE WHEN l.lead_status = 'New' THEN l.lead_id END) AS new_leads,
    COUNT(DISTINCT CASE WHEN l.lead_status = 'Contacted' THEN l.lead_id END) AS contacted_leads,
    COUNT(DISTINCT CASE WHEN l.lead_status = 'Qualified' THEN l.lead_id END) AS qualified_leads,
    COUNT(DISTINCT CASE WHEN l.lead_status = 'Converted' THEN l.lead_id END) AS converted_leads,
    COUNT(DISTINCT CASE WHEN l.lead_status = 'Lost' THEN l.lead_id END) AS lost_leads,

    -- Conversion metrics
    CASE
        WHEN COUNT(DISTINCT l.lead_id) > 0 THEN
            ROUND((COUNT(DISTINCT CASE WHEN l.lead_status = 'Converted' THEN l.lead_id END)::DECIMAL / COUNT(DISTINCT l.lead_id) * 100), 2)
        ELSE 0
    END AS conversion_rate,

    -- Activity metrics
    COUNT(DISTINCT a.activity_id) AS total_activities,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Call' THEN a.activity_id END) AS total_calls,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Meeting' THEN a.activity_id END) AS total_meetings,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Email' THEN a.activity_id END) AS total_emails,

    -- Meeting metrics
    COUNT(DISTINCT m.meeting_id) AS total_scheduled_meetings,
    COUNT(DISTINCT CASE WHEN m.meeting_status = 'Completed' THEN m.meeting_id END) AS completed_meetings,

    -- Revenue metrics
    SUM(CASE WHEN l.lead_status = 'Converted' THEN COALESCE(l.expected_revenue, 0) ELSE 0 END) AS total_expected_revenue

FROM hr_employee_core e
LEFT JOIN hr_person_name pn ON e.person_id = pn.person_id AND pn.is_primary = TRUE
LEFT JOIN bp_lead l ON e.employee_id = l.assigned_to
LEFT JOIN bp_lead_activity a ON l.lead_id = a.lead_id AND a.employee_id = e.employee_id
LEFT JOIN bp_lead_meeting m ON l.lead_id = m.lead_id AND m.assigned_employee_id = e.employee_id

WHERE e.is_active = TRUE

GROUP BY e.employee_id, e.employee_code, pn.given_name, pn.family_name;

-- Add comment
COMMENT ON VIEW v_marketing_employee_performance IS 'Marketing employee performance metrics including leads, activities, and conversion rates';

-- =====================================================
-- 7. Sample Data / Default Values
-- =====================================================

-- Insert default lead sources if needed (optional)
-- This can be moved to a separate seed data script

-- =====================================================
-- End of Migration Script
-- =====================================================
