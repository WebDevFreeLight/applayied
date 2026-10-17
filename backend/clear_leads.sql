-- Clear all test leads from the database
DELETE FROM bp_lead;

-- Verify the table is empty
SELECT COUNT(*) as total_leads FROM bp_lead;
