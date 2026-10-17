-- Migration: Add ChequeStatusHistory Table
-- Date: 2025-12-06
-- Module: Treasury - Cheque Management
-- Description: Creates table to track all cheque status changes for audit and timeline display

-- Create the cheque_status_history table
CREATE TABLE IF NOT EXISTS cheque_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cheque_id UUID NOT NULL,
    previous_status INT NOT NULL,
    new_status INT NOT NULL,
    changed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    changed_by VARCHAR(100) NOT NULL,
    reason VARCHAR(500),
    notes VARCHAR(1000),
    bank_fees DECIMAL(18,2),
    bank_reference VARCHAR(100),
    gl_journal_id UUID,
    ip_address VARCHAR(50),

    -- Audit fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    -- Foreign key
    CONSTRAINT fk_cheque_status_history_cheque
        FOREIGN KEY (cheque_id) REFERENCES cheque(id) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_cheque_status_history_cheque_id
    ON cheque_status_history(cheque_id);

CREATE INDEX IF NOT EXISTS idx_cheque_status_history_changed_at
    ON cheque_status_history(changed_at DESC);

CREATE INDEX IF NOT EXISTS idx_cheque_status_history_new_status
    ON cheque_status_history(new_status);

-- Comment
COMMENT ON TABLE cheque_status_history IS 'Tracks all status changes for cheques - سجل تغييرات حالة الشيكات';
COMMENT ON COLUMN cheque_status_history.cheque_id IS 'Reference to cheque - مرجع الشيك';
COMMENT ON COLUMN cheque_status_history.previous_status IS 'Previous status before change - الحالة السابقة';
COMMENT ON COLUMN cheque_status_history.new_status IS 'New status after change - الحالة الجديدة';
COMMENT ON COLUMN cheque_status_history.changed_at IS 'When the status change occurred - وقت التغيير';
COMMENT ON COLUMN cheque_status_history.changed_by IS 'User who made the change - المستخدم';
COMMENT ON COLUMN cheque_status_history.reason IS 'Reason for status change (bounce/cancel) - سبب التغيير';
COMMENT ON COLUMN cheque_status_history.notes IS 'Additional notes - ملاحظات';
COMMENT ON COLUMN cheque_status_history.bank_fees IS 'Bank fees if applicable - رسوم البنك';
COMMENT ON COLUMN cheque_status_history.bank_reference IS 'Bank reference number - مرجع البنك';
COMMENT ON COLUMN cheque_status_history.gl_journal_id IS 'GL Journal ID if created - معرف القيد المحاسبي';
