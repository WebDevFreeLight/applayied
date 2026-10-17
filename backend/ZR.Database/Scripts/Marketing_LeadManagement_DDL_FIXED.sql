-- =====================================================
-- Marketing Lead Management - Database Schema (FIXED)
-- =====================================================
-- Created: 2025-12-27
-- Updated: 2025-12-27 - Fixed all errors
-- Purpose: Lead activity tracking and assignment history
-- =====================================================

-- NOTE: bp_lead table already exists with all required columns
-- NOTE: bp_lead_meeting is DEPRECATED - use bp_meeting instead

-- =====================================================
-- 1. Lead Activity Tracking Table
-- =====================================================
DROP  TABLE IF  EXISTS bp_lead_activity ;
DROP TABLE IF  EXISTS bp_lead_assignment_log;

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
    branchid BIGINT DEFAULT 0
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
-- 2. Lead Assignment History Table
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
    branchid BIGINT DEFAULT 0
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
-- 3. Additional Indexes for bp_lead table (if not exists)
-- =====================================================

CREATE INDEX IF NOT EXISTS idx_bp_lead_assigned_to ON bp_lead(assignedto);
CREATE INDEX IF NOT EXISTS idx_bp_lead_status ON bp_lead(leadstatus);
CREATE INDEX IF NOT EXISTS idx_bp_lead_source ON bp_lead(leadsource);
CREATE INDEX IF NOT EXISTS idx_bp_lead_conversion_date ON bp_lead(conversiondate);
CREATE INDEX IF NOT EXISTS idx_bp_lead_create_time ON bp_lead(create_time DESC);
CREATE INDEX IF NOT EXISTS idx_bp_lead_emp_status ON bp_lead(assignedto, leadstatus);

-- =====================================================
-- 4. Views for Reporting
-- =====================================================

-- View: Employee Lead Performance Summary
CREATE OR REPLACE VIEW v_marketing_employee_performance AS
SELECT
    e.employee_id,
    e.employee_code,
    COALESCE(pn.given_name, '') || ' ' || COALESCE(pn.family_name, '') AS employee_name,

    -- Lead counts by status
    COUNT(DISTINCT l.leadid) AS total_leads,
    COUNT(DISTINCT CASE WHEN l.leadstatus = 'New' THEN l.leadid END) AS new_leads,
    COUNT(DISTINCT CASE WHEN l.leadstatus = 'Contacted' THEN l.leadid END) AS contacted_leads,
    COUNT(DISTINCT CASE WHEN l.leadstatus = 'Qualified' THEN l.leadid END) AS qualified_leads,
    COUNT(DISTINCT CASE WHEN l.leadstatus = 'Converted' THEN l.leadid END) AS converted_leads,
    COUNT(DISTINCT CASE WHEN l.leadstatus = 'Lost' THEN l.leadid END) AS lost_leads,

    -- Conversion metrics
    CASE
        WHEN COUNT(DISTINCT l.leadid) > 0 THEN
            ROUND((COUNT(DISTINCT CASE WHEN l.leadstatus = 'Converted' THEN l.leadid END)::DECIMAL / COUNT(DISTINCT l.leadid) * 100), 2)
        ELSE 0
    END AS conversion_rate,

    -- Activity metrics
    COUNT(DISTINCT a.activity_id) AS total_activities,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Call' THEN a.activity_id END) AS total_calls,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Meeting' THEN a.activity_id END) AS total_meetings,
    COUNT(DISTINCT CASE WHEN a.activity_type = 'Email' THEN a.activity_id END) AS total_emails,

    -- Revenue metrics
    SUM(CASE WHEN l.leadstatus = 'Converted' THEN COALESCE(l.expectedrevenue, 0) ELSE 0 END) AS total_expected_revenue

FROM hr_employee_core e
LEFT JOIN hr_person_name pn ON e.person_id = pn.person_id AND pn.is_primary = TRUE
LEFT JOIN bp_lead l ON e.employee_id = l.assignedto
LEFT JOIN bp_lead_activity a ON l.leadid = a.lead_id AND a.employee_id = e.employee_id

WHERE e.is_active = TRUE

GROUP BY e.employee_id, e.employee_code, pn.given_name, pn.family_name;

-- Add comment
COMMENT ON VIEW v_marketing_employee_performance IS 'Marketing employee performance metrics including leads, activities, and conversion rates';

-- =====================================================
-- End of Migration Script
-- =====================================================
