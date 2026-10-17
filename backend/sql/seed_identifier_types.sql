-- =============================================
-- Seed Identifier Types Dictionary
-- Date: 2026-01-01
-- Purpose: Define identifier types for hr_person_identifier table
-- =============================================

-- Insert identifier types into sys_dict_type
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, create_by, create_time, remark)
VALUES
  (1007, 'Identifier Types', 'identifier_type', '0', 'admin', CURRENT_TIMESTAMP, 'Types of personal identifiers (National ID, Passport, Iqama, etc.)')
ON CONFLICT (dict_type) DO NOTHING;

-- Insert identifier type values into sys_dict_data
INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, create_by, create_time, remark)
VALUES
  (1, 1, 'National ID', '1', 'identifier_type', '', 'primary', 'Y', '0', 'admin', CURRENT_TIMESTAMP, 'National ID or Resident ID'),
  (2, 2, 'Passport', '2', 'identifier_type', '', 'success', 'N', '0', 'admin', CURRENT_TIMESTAMP, 'Passport number'),
  (3, 3, 'Iqama', '3', 'identifier_type', '', 'info', 'N', '0', 'admin', CURRENT_TIMESTAMP, 'Saudi Iqama (Residence permit)'),
  (4, 4, 'Driver License', '4', 'identifier_type', '', 'warning', 'N', '0', 'admin', CURRENT_TIMESTAMP, 'Driver license number'),
  (5, 5, 'Social Security', '5', 'identifier_type', '', 'default', 'N', '0', 'admin', CURRENT_TIMESTAMP, 'Social security number')
ON CONFLICT (dict_code, dict_type) DO UPDATE SET
  dict_label = EXCLUDED.dict_label,
  dict_value = EXCLUDED.dict_value,
  remark = EXCLUDED.remark,
  update_time = CURRENT_TIMESTAMP;

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'Identifier types dictionary seeded successfully';
END $$;
