-- =====================================================
-- Event Management Dictionary Type SQL Queries
-- =====================================================
-- These queries demonstrate how to generate sys_dict_data
-- entries from Event Management and HR tables
-- =====================================================

-- 1. Event Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'event_type' AS dictType
FROM event_types
ORDER BY typecode;

-- 2. Zone Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'zone_type' AS dictType
FROM event_zone_types
ORDER BY typecode;

-- 3. Gate Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'gate_type' AS dictType
FROM event_gate_types
ORDER BY typecode;

-- 4. Visitor Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'visitor_type' AS dictType
FROM visitor_types
ORDER BY typecode;

-- 5. Ticket Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'ticket_type' AS dictType
FROM ticket_types
ORDER BY typecode;

-- 6. Permit Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'permit_type' AS dictType
FROM permit_types
ORDER BY typecode;

-- 7. Booth Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'booth_type' AS dictType
FROM booth_types
ORDER BY typecode;

-- 8. Incident Types
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'incident_type' AS dictType
FROM incident_types
ORDER BY typecode;

-- 9. Payment Methods
SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'payment_method' AS dictType
FROM payment_methods
ORDER BY methodcode;

-- 10. Event Status
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'event_status' AS dictType
FROM event_status
ORDER BY typecode;

-- 11. Event Locations
SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark,
    'event_location' AS dictType
FROM event_locations
ORDER BY locationid;

-- 12. HR Employees (Original Pattern)
SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'hr_employee' AS dictType
FROM hr_employee
WHERE status = 'active'
ORDER BY employeeid;

-- =====================================================
-- Combined Query - Union All
-- =====================================================
-- This query combines all dictionary sources into one result set
-- Useful for bulk data generation or reporting

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'event_type' AS dictType,
    1 AS sortOrder
FROM event_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'zone_type' AS dictType,
    2 AS sortOrder
FROM event_zone_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'gate_type' AS dictType,
    3 AS sortOrder
FROM event_gate_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'visitor_type' AS dictType,
    4 AS sortOrder
FROM visitor_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'ticket_type' AS dictType,
    5 AS sortOrder
FROM ticket_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'permit_type' AS dictType,
    6 AS sortOrder
FROM permit_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'booth_type' AS dictType,
    7 AS sortOrder
FROM booth_types

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'incident_type' AS dictType,
    8 AS sortOrder
FROM incident_types

UNION ALL

SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'payment_method' AS dictType,
    9 AS sortOrder
FROM payment_methods

UNION ALL

SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'event_status' AS dictType,
    10 AS sortOrder
FROM event_status

UNION ALL

SELECT
    locationid::text AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark,
    'event_location' AS dictType,
    11 AS sortOrder
FROM event_locations

UNION ALL

SELECT
    employeeid::text AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'hr_employee' AS dictType,
    12 AS sortOrder
FROM hr_employee
WHERE status = 'active'

ORDER BY sortOrder, dictValue;

-- =====================================================
-- Insert into sys_dict_data (Example)
-- =====================================================
-- Example of how to insert generated data into sys_dict_data table
-- Note: This is a template - adjust column names as needed

/*
INSERT INTO sys_dict_data (
    dict_type,
    dict_value,
    dict_label,
    dict_sort,
    remark,
    is_default,
    status,
    create_by,
    create_time
)
SELECT
    'event_type' AS dict_type,
    typecode AS dict_value,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dict_label,
    ROW_NUMBER() OVER (ORDER BY typecode) AS dict_sort,
    remark,
    'N' AS is_default,
    '0' AS status,
    'system' AS create_by,
    NOW() AS create_time
FROM event_types
ON CONFLICT (dict_type, dict_value)
DO UPDATE SET
    dict_label = EXCLUDED.dict_label,
    remark = EXCLUDED.remark,
    update_time = NOW();
*/

-- =====================================================
-- Verification Queries
-- =====================================================

-- Count records by dictionary type
SELECT
    dict_type,
    COUNT(*) as record_count
FROM sys_dict_data
WHERE dict_type IN (
    'event_type', 'zone_type', 'gate_type', 'visitor_type',
    'ticket_type', 'permit_type', 'booth_type', 'incident_type',
    'payment_method', 'event_status', 'event_location', 'hr_employee'
)
GROUP BY dict_type
ORDER BY dict_type;

-- View all Event Management dictionaries
SELECT
    dict_type,
    dict_value,
    dict_label,
    dict_sort,
    status
FROM sys_dict_data
WHERE dict_type LIKE '%event%' OR dict_type LIKE '%zone%' OR dict_type LIKE '%gate%'
ORDER BY dict_type, dict_sort;
