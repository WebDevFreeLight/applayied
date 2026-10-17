-- =============================================
-- Create HR Employee Document Table
-- Date: 2026-01-01
-- DB: PostgreSQL
-- Purpose: Store employee documents with metadata
-- =============================================

-- Create hr_employee_document table
CREATE TABLE IF NOT EXISTS hr_employee_document (
    document_id              BIGSERIAL PRIMARY KEY,
    employee_id              BIGINT NOT NULL,
    document_type_dict_code  BIGINT NULL,
    document_name            VARCHAR(250) NOT NULL,
    document_number          VARCHAR(100) NULL,
    file_name                VARCHAR(250) NOT NULL,
    file_path                VARCHAR(500) NOT NULL,
    file_size                BIGINT NOT NULL,
    mime_type                VARCHAR(100) NULL,
    upload_date              TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    issue_date               DATE NULL,
    expiry_date              DATE NULL,
    issuing_authority        VARCHAR(200) NULL,
    is_verified              BOOLEAN NOT NULL DEFAULT FALSE,
    verified_by              VARCHAR(100) NULL,
    verified_date            TIMESTAMPTZ NULL,
    notes                    TEXT NULL,
    status                   VARCHAR(50) NULL DEFAULT 'Active',

    -- Audit columns (from SysBase)
    companyid                BIGINT NULL,
    branchid                 BIGINT NULL,
    create_by                VARCHAR(64) NULL,
    create_time              TIMESTAMPTZ NULL,
    update_by                VARCHAR(64) NULL,
    update_time              TIMESTAMPTZ NULL,
    remark                   VARCHAR(500) NULL
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS ix_hr_employee_document_employee
    ON hr_employee_document(employee_id);

CREATE INDEX IF NOT EXISTS ix_hr_employee_document_type
    ON hr_employee_document(document_type_dict_code);

CREATE INDEX IF NOT EXISTS ix_hr_employee_document_expiry
    ON hr_employee_document(expiry_date)
    WHERE expiry_date IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_hr_employee_document_status
    ON hr_employee_document(status);

CREATE INDEX IF NOT EXISTS ix_hr_employee_document_upload_date
    ON hr_employee_document(upload_date DESC);

-- Add comments for documentation
COMMENT ON TABLE hr_employee_document IS 'Employee document storage with metadata tracking';
COMMENT ON COLUMN hr_employee_document.document_id IS 'Primary key - auto-increment document ID';
COMMENT ON COLUMN hr_employee_document.employee_id IS 'Foreign key to hr_employee_core or hr_employee';
COMMENT ON COLUMN hr_employee_document.document_type_dict_code IS 'Reference to sys_dict_data for document types';
COMMENT ON COLUMN hr_employee_document.document_name IS 'User-friendly document name';
COMMENT ON COLUMN hr_employee_document.document_number IS 'Official document number (e.g., passport number, ID number)';
COMMENT ON COLUMN hr_employee_document.file_name IS 'Original uploaded file name';
COMMENT ON COLUMN hr_employee_document.file_path IS 'Server file system path to stored document';
COMMENT ON COLUMN hr_employee_document.file_size IS 'File size in bytes';
COMMENT ON COLUMN hr_employee_document.mime_type IS 'MIME type of the file (e.g., application/pdf, image/jpeg)';
COMMENT ON COLUMN hr_employee_document.upload_date IS 'Date and time when document was uploaded';
COMMENT ON COLUMN hr_employee_document.issue_date IS 'Official issue date of the document';
COMMENT ON COLUMN hr_employee_document.expiry_date IS 'Expiration date of the document (for tracking expiring documents)';
COMMENT ON COLUMN hr_employee_document.issuing_authority IS 'Authority that issued the document';
COMMENT ON COLUMN hr_employee_document.is_verified IS 'Whether document has been verified by HR';
COMMENT ON COLUMN hr_employee_document.verified_by IS 'Username of person who verified the document';
COMMENT ON COLUMN hr_employee_document.verified_date IS 'Date and time when document was verified';
COMMENT ON COLUMN hr_employee_document.notes IS 'Additional notes or comments about the document';
COMMENT ON COLUMN hr_employee_document.status IS 'Document status (Active, Archived, Expired, etc.)';

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'hr_employee_document table created successfully';
END $$;
