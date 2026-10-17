-- Check if employee exists in new tables
SELECT 'hr_employee_core' as table_name, COUNT(*) as count 
FROM hr_employee_core 
WHERE employee_id = 1976266761632354304;

SELECT 'hr_person' as table_name, COUNT(*) as count 
FROM hr_person 
WHERE person_id = 1976266761632354304;

-- Show the actual employee data
SELECT employee_id, employee_code, hire_date, is_active 
FROM hr_employee_core 
WHERE employee_id = 1976266761632354304;

-- Check employee assignment
SELECT employee_id, department_id, position_id 
FROM hr_employee_assignment 
WHERE employee_id = 1976266761632354304;
