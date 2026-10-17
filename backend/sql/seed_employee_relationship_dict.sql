-- =============================================
-- Dependent Relationship Dictionary Data
-- Creates dict type and dict data for dependent relationships
-- (Spouse, Child, Parent, Sibling, Other)
-- =============================================

-- Insert dict type (if not exists)
INSERT INTO sys_dict_type (dict_type, dict_name, status, remark, create_by, create_time)
SELECT
    'sys_dependent_relationship',
    'علاقة المعال - Dependent Relationship',
    '0',
    'Employee dependent relationship types',
    'system',
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM sys_dict_type WHERE dict_type = 'sys_dependent_relationship'
);

-- Insert dict data values
INSERT INTO sys_dict_data (dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_by, create_time)
SELECT 1, 'زوج/زوجة - Spouse', '0', 'sys_dependent_relationship', '', 'default', 'N', '0', 'Husband or Wife', 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM sys_dict_data WHERE dict_type = 'sys_dependent_relationship' AND dict_value = '0')
UNION ALL
SELECT 2, 'ابن/ابنة - Child', '1', 'sys_dependent_relationship', '', 'default', 'N', '0', 'Son or Daughter', 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM sys_dict_data WHERE dict_type = 'sys_dependent_relationship' AND dict_value = '1')
UNION ALL
SELECT 3, 'أب/أم - Parent', '2', 'sys_dependent_relationship', '', 'default', 'N', '0', 'Father or Mother', 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM sys_dict_data WHERE dict_type = 'sys_dependent_relationship' AND dict_value = '2')
UNION ALL
SELECT 4, 'أخ/أخت - Sibling', '3', 'sys_dependent_relationship', '', 'default', 'N', '0', 'Brother or Sister', 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM sys_dict_data WHERE dict_type = 'sys_dependent_relationship' AND dict_value = '3')
UNION ALL
SELECT 5, 'أخرى - Other', '4', 'sys_dependent_relationship', '', 'default', 'N', '0', 'Other family relationship', 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM sys_dict_data WHERE dict_type = 'sys_dependent_relationship' AND dict_value = '4');
