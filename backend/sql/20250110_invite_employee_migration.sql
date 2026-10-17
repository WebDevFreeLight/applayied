-- Migration: Add invitation and password reset fields to sys_user
-- Date: 2025-01-10
-- Description: Support for employee invitation and forgot password workflows

-- Add password reset fields
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS password_reset_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS password_reset_expiry TIMESTAMP;

-- Add invitation fields
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS invitation_token VARCHAR(255);
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS invitation_expiry TIMESTAMP;

-- Add activation status (0=pending invitation, 1=activated)
ALTER TABLE sys_user ADD COLUMN IF NOT EXISTS is_activated INT DEFAULT 1;

-- Create index for faster token lookups
CREATE INDEX IF NOT EXISTS idx_sys_user_invitation_token ON sys_user(invitation_token);
CREATE INDEX IF NOT EXISTS idx_sys_user_password_reset_token ON sys_user(password_reset_token);

-- Existing users should have is_activated = 1 (already activated)
UPDATE sys_user SET is_activated = 1 WHERE is_activated IS NULL;
