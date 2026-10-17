#!/bin/bash

OUTPUT="sql_queries/all_dict_sql_queries.sql"
mkdir -p sql_queries

echo "-- ========================================================" > "$OUTPUT"
echo "-- Auto-Generated Dictionary SQL Queries for All Entities" >> "$OUTPUT"
echo "-- Generated: $(date '+%Y-%m-%d %H:%M:%S')" >> "$OUTPUT"
echo "-- ========================================================" >> "$OUTPUT"
echo "" >> "$OUTPUT"

# Read from our existing entity data
tail -n +3 /tmp/all_entities.txt | while IFS='|' read -r table_name class_name namespace file_path; do
    if [ ! -z "$table_name" ]; then
        class_name=$(echo "$class_name" | cut -d':' -f1)
        
        # Determine SQL pattern based on table name
        case "$table_name" in
            event_types|event_zone_types|event_gate_types|event_visitor_types|event_ticket_types|event_permit_types|event_booth_types|event_incident_types|event_status)
                # Standard type pattern: code - name - namear
                cat >> "$OUTPUT" << SQL
-- Table: $table_name ($class_name)
SELECT
    typecode AS dictValue,
    CONCAT(typecode, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
ORDER BY typecode;

SQL
                ;;
            
            event_payment_methods)
                cat >> "$OUTPUT" << SQL
-- Table: $table_name ($class_name)
SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
ORDER BY methodcode;

SQL
                ;;
            
            event_locations)
                cat >> "$OUTPUT" << SQL
-- Table: $table_name ($class_name)
SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    address AS remark,
    'sql_$table_name' AS dictType
FROM $table_name
ORDER BY locationid;

SQL
                ;;
            
            hr_employee)
                cat >> "$OUTPUT" << SQL
-- Table: $table_name ($class_name)
SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
WHERE status = 'active'
ORDER BY employeeid;

SQL
                ;;
            
            *)
                # Generic pattern: id and first string field
                cat >> "$OUTPUT" << SQL
-- Table: $table_name ($class_name)
SELECT
    id AS dictValue,
    name AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
ORDER BY id;

SQL
                ;;
        esac
    fi
done

echo "SQL queries generated in: $OUTPUT"
wc -l "$OUTPUT"
