-- Cost Centers Table Creation Script
-- مراكز التكلفة - جدول الإنشاء
-- Date: 2024-12-06

-- Create the cost centers table if not exists
CREATE TABLE IF NOT EXISTS erp_cost_centers (
    costcenterid BIGINT PRIMARY KEY,
    parentid BIGINT NULL,
    costcentercode VARCHAR(50) NOT NULL,
    costcentername VARCHAR(200) NOT NULL,
    costcenternamear VARCHAR(200) NULL,
    costcentertype VARCHAR(50) NOT NULL DEFAULT 'department',
    level INT NOT NULL DEFAULT 1,
    isparent BOOLEAN NOT NULL DEFAULT FALSE,
    budgetamount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    actualamount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    variance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    description VARCHAR(500) NULL,
    isactive BOOLEAN NOT NULL DEFAULT TRUE,
    create_by VARCHAR(64) NULL,
    create_time TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    update_by VARCHAR(64) NULL,
    update_time TIMESTAMP NULL,
    remark VARCHAR(500) NULL,
    branchid BIGINT NULL,
    CONSTRAINT fk_costcenter_parent FOREIGN KEY (parentid)
        REFERENCES erp_cost_centers(costcenterid) ON DELETE SET NULL
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_costcenter_code ON erp_cost_centers(costcentercode);
CREATE INDEX IF NOT EXISTS idx_costcenter_parent ON erp_cost_centers(parentid);
CREATE INDEX IF NOT EXISTS idx_costcenter_type ON erp_cost_centers(costcentertype);
CREATE INDEX IF NOT EXISTS idx_costcenter_active ON erp_cost_centers(isactive);

-- Add unique constraint on code
ALTER TABLE erp_cost_centers ADD CONSTRAINT uq_costcenter_code UNIQUE (costcentercode);

-- Sample data for testing
-- INSERT INTO erp_cost_centers (costcenterid, costcentercode, costcentername, costcenternamear, costcentertype, level, isparent, budgetamount, isactive)
-- VALUES
--     (1, 'CC001', 'Head Office', 'المكتب الرئيسي', 'branch', 1, TRUE, 1000000, TRUE),
--     (2, 'CC001-01', 'IT Department', 'قسم تقنية المعلومات', 'department', 2, FALSE, 200000, TRUE),
--     (3, 'CC001-02', 'Finance Department', 'القسم المالي', 'department', 2, FALSE, 150000, TRUE),
--     (4, 'CC001-03', 'HR Department', 'قسم الموارد البشرية', 'department', 2, FALSE, 100000, TRUE);
-- UPDATE erp_cost_centers SET parentid = 1 WHERE costcenterid IN (2, 3, 4);

COMMENT ON TABLE erp_cost_centers IS 'Cost Centers - مراكز التكلفة';
COMMENT ON COLUMN erp_cost_centers.costcenterid IS 'Cost Center ID - معرف مركز التكلفة';
COMMENT ON COLUMN erp_cost_centers.parentid IS 'Parent ID - معرف الأب';
COMMENT ON COLUMN erp_cost_centers.costcentercode IS 'Cost Center Code - كود مركز التكلفة';
COMMENT ON COLUMN erp_cost_centers.costcentername IS 'Cost Center Name - اسم مركز التكلفة';
COMMENT ON COLUMN erp_cost_centers.costcenternamear IS 'Cost Center Name (Arabic) - اسم مركز التكلفة بالعربي';
COMMENT ON COLUMN erp_cost_centers.costcentertype IS 'Cost Center Type (branch/department/project) - نوع مركز التكلفة';
COMMENT ON COLUMN erp_cost_centers.level IS 'Hierarchy Level - المستوى';
COMMENT ON COLUMN erp_cost_centers.isparent IS 'Is Parent Node - رئيسي';
COMMENT ON COLUMN erp_cost_centers.budgetamount IS 'Budget Amount - الميزانية';
COMMENT ON COLUMN erp_cost_centers.actualamount IS 'Actual Amount - المبلغ الفعلي';
COMMENT ON COLUMN erp_cost_centers.variance IS 'Variance (Budget - Actual) - الفرق';
COMMENT ON COLUMN erp_cost_centers.isactive IS 'Is Active - نشط';
