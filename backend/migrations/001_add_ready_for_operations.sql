-- Migration: Add ready_for_operations column to bp_lead
-- Date: 2026-03-03
-- Description: Adds a boolean flag so Operations can see leads before they are fully "Confirmed"

ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS ready_for_operations BOOLEAN DEFAULT false;

-- Set existing Confirmed leads as ready for operations
UPDATE bp_lead SET ready_for_operations = true WHERE leadstatus = 'Confirmed';
