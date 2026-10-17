-- =============================================
-- HR Normalization - Phase 1 (Implementation Mode)
-- Domain: Employee Core
-- Date: 2025-12-20
-- DB: PostgreSQL-style idempotent DDL (CREATE TABLE IF NOT EXISTS)
--
-- Constraints honored:
-- - Create NEW tables only (no ALTER/DROP of existing tables)
-- - SRP: one responsibility per table
-- - No data migration
-- - No code/service/controller changes
--
-- Naming:
-- - snake_case
-- - singular table names
-- - boolean columns use is_/has_
--
-- Notes:
-- - Existing tables `hr_employee` and `hr_employee_dependent` already exist in current schema.
--   Therefore this domain uses:
--     - `hr_employee_core` (new) instead of `hr_employee`
--     - `hr_employee_dependent_link` (new) instead of `hr_employee_dependent`
-- - Foreign keys are NOT enforced in Phase 1 (columns + PK/unique/indexes only).
-- =============================================

-- ---------------------------------------------
-- Person (identity) tables
-- ---------------------------------------------

CREATE TABLE IF NOT EXISTS hr_person (
    person_id        BIGSERIAL PRIMARY KEY,
    is_active        BOOLEAN NOT NULL DEFAULT TRUE,
    -- Audit / multi-company (align conceptually with SysBase)
    companyid        BIGINT NULL,
    branchid         BIGINT NULL,
    create_by        VARCHAR(64) NULL,
    create_time      TIMESTAMPTZ NULL,
    update_by        VARCHAR(64) NULL,
    update_time      TIMESTAMPTZ NULL,
    remark           VARCHAR(500) NULL
);

-- Semantic clarification (Phase 1 safe):
-- - hr_person.is_active indicates PERSON entity existence/visibility (identity record active/inactive)
COMMENT ON COLUMN hr_person.is_active IS 'Person entity existence/visibility flag (identity-level). Not employment status.';

CREATE INDEX IF NOT EXISTS ix_hr_person_company ON hr_person(companyid);

CREATE TABLE IF NOT EXISTS hr_person_name (
    person_name_id       BIGSERIAL PRIMARY KEY,
    person_id            BIGINT NOT NULL,
    name_type_dict_code  BIGINT NULL,
    given_name           VARCHAR(100) NULL,
    middle_name          VARCHAR(100) NULL,
    family_name          VARCHAR(100) NULL,
    full_name_ar         VARCHAR(200) NULL,
    effective_from       DATE NOT NULL,
    effective_to         DATE NULL,
    is_primary           BOOLEAN NOT NULL DEFAULT FALSE,
    companyid            BIGINT NULL,
    branchid             BIGINT NULL,
    create_by            VARCHAR(64) NULL,
    create_time          TIMESTAMPTZ NULL,
    update_by            VARCHAR(64) NULL,
    update_time          TIMESTAMPTZ NULL,
    remark               VARCHAR(500) NULL,
    CONSTRAINT ck_hr_person_name_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_person_name_person ON hr_person_name(person_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_person_name_primary ON hr_person_name(person_id) WHERE is_primary = TRUE;

CREATE TABLE IF NOT EXISTS hr_person_demographic (
    person_id                BIGINT PRIMARY KEY,
    date_of_birth            DATE NULL,
    gender_dict_code         BIGINT NULL,
    marital_status_dict_code BIGINT NULL,
    nationality_dict_code    BIGINT NULL,
    companyid                BIGINT NULL,
    branchid                 BIGINT NULL,
    create_by                VARCHAR(64) NULL,
    create_time              TIMESTAMPTZ NULL,
    update_by                VARCHAR(64) NULL,
    update_time              TIMESTAMPTZ NULL,
    remark                   VARCHAR(500) NULL
);

CREATE TABLE IF NOT EXISTS hr_person_identifier (
    person_identifier_id     BIGSERIAL PRIMARY KEY,
    person_id                BIGINT NOT NULL,
    identifier_type_dict_code BIGINT NOT NULL,
    identifier_number        VARCHAR(100) NOT NULL,
    issuing_country_dict_code BIGINT NULL,
    issue_date               DATE NULL,
    expiry_date              DATE NULL,
    is_primary               BOOLEAN NOT NULL DEFAULT FALSE,
    is_verified              BOOLEAN NOT NULL DEFAULT FALSE,
    verified_time            TIMESTAMPTZ NULL,
    companyid                BIGINT NULL,
    branchid                 BIGINT NULL,
    create_by                VARCHAR(64) NULL,
    create_time              TIMESTAMPTZ NULL,
    update_by                VARCHAR(64) NULL,
    update_time              TIMESTAMPTZ NULL,
    remark                   VARCHAR(500) NULL,
    CONSTRAINT ck_hr_person_identifier_expiry
        CHECK (expiry_date IS NULL OR issue_date IS NULL OR expiry_date >= issue_date)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_hr_person_identifier_unique_per_type
    ON hr_person_identifier(person_id, identifier_type_dict_code, identifier_number);
CREATE INDEX IF NOT EXISTS ix_hr_person_identifier_person ON hr_person_identifier(person_id);

CREATE TABLE IF NOT EXISTS hr_person_contact (
    person_contact_id    BIGSERIAL PRIMARY KEY,
    person_id            BIGINT NOT NULL,
    contact_type_dict_code BIGINT NOT NULL,
    contact_value        VARCHAR(200) NOT NULL,
    effective_from       DATE NOT NULL,
    effective_to         DATE NULL,
    is_primary           BOOLEAN NOT NULL DEFAULT FALSE,
    is_verified          BOOLEAN NOT NULL DEFAULT FALSE,
    verified_time        TIMESTAMPTZ NULL,
    companyid            BIGINT NULL,
    branchid             BIGINT NULL,
    create_by            VARCHAR(64) NULL,
    create_time          TIMESTAMPTZ NULL,
    update_by            VARCHAR(64) NULL,
    update_time          TIMESTAMPTZ NULL,
    remark               VARCHAR(500) NULL,
    CONSTRAINT ck_hr_person_contact_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_person_contact_person ON hr_person_contact(person_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_person_contact_primary ON hr_person_contact(person_id) WHERE is_primary = TRUE;

CREATE TABLE IF NOT EXISTS hr_person_address (
    person_address_id     BIGSERIAL PRIMARY KEY,
    person_id             BIGINT NOT NULL,
    address_type_dict_code BIGINT NOT NULL,
    country_dict_code     BIGINT NULL,
    city                  VARCHAR(100) NULL,
    postal_code           VARCHAR(20) NULL,
    address_line_1        VARCHAR(250) NULL,
    address_line_2        VARCHAR(250) NULL,
    effective_from        DATE NOT NULL,
    effective_to          DATE NULL,
    is_primary            BOOLEAN NOT NULL DEFAULT FALSE,
    companyid             BIGINT NULL,
    branchid              BIGINT NULL,
    create_by             VARCHAR(64) NULL,
    create_time           TIMESTAMPTZ NULL,
    update_by             VARCHAR(64) NULL,
    update_time           TIMESTAMPTZ NULL,
    remark                VARCHAR(500) NULL,
    CONSTRAINT ck_hr_person_address_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_person_address_person ON hr_person_address(person_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_person_address_primary ON hr_person_address(person_id) WHERE is_primary = TRUE;

-- ---------------------------------------------
-- Employee (employment anchor) tables
-- ---------------------------------------------

CREATE TABLE IF NOT EXISTS hr_employee_core (
    employee_id               BIGSERIAL PRIMARY KEY,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    person_id                 BIGINT NOT NULL,
    user_id                   BIGINT NULL,
    employee_code             VARCHAR(50) NOT NULL,
    hire_date                 DATE NULL,
    termination_date          DATE NULL,
    employment_type_dict_code BIGINT NULL,
    is_active                 BOOLEAN NOT NULL DEFAULT TRUE,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_core_term_after_hire
        CHECK (termination_date IS NULL OR hire_date IS NULL OR termination_date >= hire_date)
);

-- Semantic clarification (Phase 1 safe):
-- - hr_employee_core.is_active indicates EMPLOYMENT relationship activity (assignment/employment active vs inactive)
COMMENT ON COLUMN hr_employee_core.is_active IS 'Employment relationship activity flag (employee-person engagement). Not person existence.';

CREATE UNIQUE INDEX IF NOT EXISTS ux_hr_employee_core_company_code
    ON hr_employee_core(companyid, employee_code);
CREATE INDEX IF NOT EXISTS ix_hr_employee_core_person ON hr_employee_core(person_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_core_user ON hr_employee_core(user_id);

CREATE TABLE IF NOT EXISTS hr_employee_assignment (
    employee_assignment_id  BIGSERIAL PRIMARY KEY,
    companyid               BIGINT NULL,
    branchid                BIGINT NULL,
    employee_id             BIGINT NOT NULL,
    department_id           BIGINT NULL,
    position_id             BIGINT NULL,
    manager_employee_id     BIGINT NULL,
    work_location_id        BIGINT NULL,
    effective_from          DATE NOT NULL,
    effective_to            DATE NULL,
    is_primary              BOOLEAN NOT NULL DEFAULT FALSE,
    create_by               VARCHAR(64) NULL,
    create_time             TIMESTAMPTZ NULL,
    update_by               VARCHAR(64) NULL,
    update_time             TIMESTAMPTZ NULL,
    remark                  VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_assignment_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from),
    CONSTRAINT ck_hr_employee_assignment_manager_not_self
        CHECK (manager_employee_id IS NULL OR manager_employee_id <> employee_id)
);

-- TODO (do not enforce yet - Phase 1 safe):
-- - Only one primary assignment should be active per employee at a time
--   (e.g., one row where is_primary = true AND effective range contains "today").

CREATE INDEX IF NOT EXISTS ix_hr_employee_assignment_employee ON hr_employee_assignment(employee_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_employee_assignment_primary ON hr_employee_assignment(employee_id) WHERE is_primary = TRUE;
CREATE INDEX IF NOT EXISTS ix_hr_employee_assignment_manager ON hr_employee_assignment(manager_employee_id);

CREATE TABLE IF NOT EXISTS hr_employee_status (
    employee_status_id          BIGSERIAL PRIMARY KEY,
    companyid                   BIGINT NULL,
    branchid                    BIGINT NULL,
    employee_id                 BIGINT NOT NULL,
    employment_status_dict_code BIGINT NOT NULL,
    status_reason_dict_code     BIGINT NULL,
    effective_from              DATE NOT NULL,
    effective_to                DATE NULL,
    create_by                   VARCHAR(64) NULL,
    create_time                 TIMESTAMPTZ NULL,
    update_by                   VARCHAR(64) NULL,
    update_time                 TIMESTAMPTZ NULL,
    remark                      VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_status_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_employee_status_employee ON hr_employee_status(employee_id, effective_from);

CREATE TABLE IF NOT EXISTS hr_employee_contract (
    employee_contract_id     BIGSERIAL PRIMARY KEY,
    companyid                BIGINT NULL,
    branchid                 BIGINT NULL,
    employee_id              BIGINT NOT NULL,
    contract_type_dict_code  BIGINT NULL,
    contract_start_date      DATE NULL,
    contract_end_date        DATE NULL,
    probation_end_date       DATE NULL,
    effective_from           DATE NOT NULL,
    effective_to             DATE NULL,
    is_current               BOOLEAN NOT NULL DEFAULT FALSE,
    create_by                VARCHAR(64) NULL,
    create_time              TIMESTAMPTZ NULL,
    update_by                VARCHAR(64) NULL,
    update_time              TIMESTAMPTZ NULL,
    remark                   VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_contract_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from),
    CONSTRAINT ck_hr_employee_contract_dates
        CHECK (contract_end_date IS NULL OR contract_start_date IS NULL OR contract_end_date >= contract_start_date)
);

-- TODO (do not enforce yet - Phase 1 safe):
-- - Only one current contract should be active per employee
--   (e.g., one row where is_current = true AND effective range contains "today").

CREATE INDEX IF NOT EXISTS ix_hr_employee_contract_employee ON hr_employee_contract(employee_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_employee_contract_current ON hr_employee_contract(employee_id) WHERE is_current = TRUE;

CREATE TABLE IF NOT EXISTS hr_employee_statutory (
    employee_statutory_id     BIGSERIAL PRIMARY KEY,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    employee_id               BIGINT NOT NULL,
    country_dict_code         BIGINT NULL,
    has_gosi                  BOOLEAN NOT NULL DEFAULT FALSE,
    gosi_number               VARCHAR(50) NULL,
    is_saudi                  BOOLEAN NOT NULL DEFAULT FALSE,
    medical_insurance_number  VARCHAR(50) NULL,
    effective_from            DATE NOT NULL,
    effective_to              DATE NULL,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_statutory_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_employee_statutory_employee ON hr_employee_statutory(employee_id, effective_from);

CREATE TABLE IF NOT EXISTS hr_employee_bank_account (
    employee_bank_account_id  BIGSERIAL PRIMARY KEY,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    employee_id               BIGINT NOT NULL,
    bank_name                 VARCHAR(100) NULL,
    iban_encrypted            TEXT NULL,
    account_number_encrypted  TEXT NULL,
    bank_account_encrypted    TEXT NULL,
    currency_id               UUID NULL,
    effective_from            DATE NOT NULL,
    effective_to              DATE NULL,
    is_primary                BOOLEAN NOT NULL DEFAULT FALSE,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_bank_account_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_employee_bank_account_employee ON hr_employee_bank_account(employee_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_employee_bank_account_primary ON hr_employee_bank_account(employee_id) WHERE is_primary = TRUE;

CREATE TABLE IF NOT EXISTS hr_employee_compensation_package (
    comp_package_id           BIGSERIAL PRIMARY KEY,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    employee_id               BIGINT NOT NULL,
    currency_id               UUID NULL,
    effective_from            DATE NOT NULL,
    effective_to              DATE NULL,
    comp_package_status_dict_code BIGINT NULL,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_comp_package_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

-- TODO (do not enforce yet - Phase 1 safe):
-- - Only one active compensation package per employee at a time
--   (e.g., one row whose effective range contains "today" and status indicates active).

CREATE INDEX IF NOT EXISTS ix_hr_employee_comp_package_employee ON hr_employee_compensation_package(employee_id, effective_from);

CREATE TABLE IF NOT EXISTS hr_employee_compensation_component (
    comp_component_id         BIGSERIAL PRIMARY KEY,
    comp_package_id           BIGINT NOT NULL,
    component_type_dict_code  BIGINT NOT NULL,
    component_code_dict_code  BIGINT NOT NULL,
    amount                    NUMERIC(18,4) NOT NULL DEFAULT 0,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_hr_employee_comp_component_unique
    ON hr_employee_compensation_component(comp_package_id, component_type_dict_code, component_code_dict_code);
CREATE INDEX IF NOT EXISTS ix_hr_employee_comp_component_package ON hr_employee_compensation_component(comp_package_id);

CREATE TABLE IF NOT EXISTS hr_employee_emergency_contact (
    emergency_contact_id      BIGSERIAL PRIMARY KEY,
    companyid                 BIGINT NULL,
    branchid                  BIGINT NULL,
    employee_id               BIGINT NOT NULL,
    contact_name              VARCHAR(200) NOT NULL,
    relationship_dict_code    BIGINT NULL,
    phone                     VARCHAR(30) NULL,
    alternate_phone           VARCHAR(30) NULL,
    email                     VARCHAR(200) NULL,
    address                   VARCHAR(500) NULL,
    is_primary                BOOLEAN NOT NULL DEFAULT FALSE,
    is_active                 BOOLEAN NOT NULL DEFAULT TRUE,
    create_by                 VARCHAR(64) NULL,
    create_time               TIMESTAMPTZ NULL,
    update_by                 VARCHAR(64) NULL,
    update_time               TIMESTAMPTZ NULL,
    remark                    VARCHAR(500) NULL
);

CREATE INDEX IF NOT EXISTS ix_hr_employee_emergency_contact_employee ON hr_employee_emergency_contact(employee_id);
CREATE INDEX IF NOT EXISTS ix_hr_employee_emergency_contact_primary ON hr_employee_emergency_contact(employee_id) WHERE is_primary = TRUE;

CREATE TABLE IF NOT EXISTS hr_employee_dependent_link (
    employee_dependent_link_id BIGSERIAL PRIMARY KEY,
    companyid                  BIGINT NULL,
    branchid                   BIGINT NULL,
    employee_id                BIGINT NOT NULL,
    dependent_person_id        BIGINT NOT NULL,
    relationship_dict_code     BIGINT NULL,
    effective_from             DATE NOT NULL,
    effective_to               DATE NULL,
    is_active                  BOOLEAN NOT NULL DEFAULT TRUE,
    create_by                  VARCHAR(64) NULL,
    create_time                TIMESTAMPTZ NULL,
    update_by                  VARCHAR(64) NULL,
    update_time                TIMESTAMPTZ NULL,
    remark                     VARCHAR(500) NULL,
    CONSTRAINT ck_hr_employee_dependent_link_effective_range
        CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE INDEX IF NOT EXISTS ix_hr_employee_dependent_link_employee ON hr_employee_dependent_link(employee_id, effective_from);
CREATE INDEX IF NOT EXISTS ix_hr_employee_dependent_link_dependent ON hr_employee_dependent_link(dependent_person_id);


