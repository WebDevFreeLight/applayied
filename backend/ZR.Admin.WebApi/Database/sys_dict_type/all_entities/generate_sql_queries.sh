#!/bin/bash

# Generate SQL queries for all SugarTable entities
cd /f/mahamBackendNET/backend

OUTPUT_DIR="ZR.Admin.WebApi/Database/sys_dict_type/all_entities/sql_queries"
mkdir -p "$OUTPUT_DIR"

echo "-- ======================================================="  > "$OUTPUT_DIR/all_dict_sql_queries.sql"
echo "-- Auto-Generated Dictionary SQL Queries for All Entities" >> "$OUTPUT_DIR/all_dict_sql_queries.sql"
echo "-- Generated: $(date)" >> "$OUTPUT_DIR/all_dict_sql_queries.sql"
echo "-- Total Entities: $(find ZR.Model -name '*.cs' -exec grep -l '\[SugarTable' {} \; | wc -l)" >> "$OUTPUT_DIR/all_dict_sql_queries.sql"
echo "-- =======================================================" >> "$OUTPUT_DIR/all_dict_sql_queries.sql"
echo "" >> "$OUTPUT_DIR/all_dict_sql_queries.sql"

count=0

# Process each entity
find ZR.Model -name "*.cs" -type f | while read file; do
    if grep -q "\[SugarTable" "$file" 2>/dev/null; then
        table_name=$(grep "\[SugarTable" "$file" | head -1 | sed 's/.*SugarTable("\([^"]*\)".*/\1/')
        class_name=$(grep "public class" "$file" | head -1 | awk '{print $3}' | cut -d':' -f1)

        # Skip if table name is empty
        if [ -z "$table_name" ]; then
            continue
        fi

        # Find primary key
        primary_key=$(grep -B 5 "IsPrimaryKey.*true" "$file" | grep "public" | tail -1 | awk '{print $NF}' | sed 's/[{};]//g' | tr '[:upper:]' '[:lower:]')

        # Default primary key if not found
        if [ -z "$primary_key" ]; then
            primary_key="id"
        fi

        # Determine label fields based on common patterns
        dict_value="$primary_key"

        # Check for common code/name patterns
        has_code=$(grep -i "public string.*code" "$file" | head -1)
        has_name=$(grep -i "public string.*name[^a]" "$file" | head -1)
        has_namear=$(grep -i "public string.*namear" "$file" | head -1)
        has_typename=$(grep -i "public string.*typename" "$file" | head -1)
        has_typenamear=$(grep -i "public string.*typenamear" "$file" | head -1)

        # Build label based on available fields
        if [ ! -z "$has_typename" ] && [ ! -z "$has_typenamear" ]; then
            # Pattern: code - typename - typenamear
            code_field=$(echo "$has_code" | awk '{print $NF}' | sed 's/[{};]//g' | tr '[:upper:]' '[:lower:]')
            if [ -z "$code_field" ]; then
                code_field=$(echo "$has_typename" | awk '{print $NF}' | sed 's/[{};]//g' | sed 's/name$/code/i' | tr '[:upper:]' '[:lower:]')
            fi
            dict_label="CONCAT($code_field, ' - ', typename, ' - ', typenamear)"
        elif [ ! -z "$has_name" ] && [ ! -z "$has_namear" ]; then
            # Pattern: name - namear
            name_field=$(echo "$has_name" | awk '{print $NF}' | sed 's/[{};]//g' | tr '[:upper:]' '[:lower:]')
            dict_label="CONCAT($name_field, ' - ', namear)"
        elif [ ! -z "$has_name" ]; then
            # Pattern: just name
            name_field=$(echo "$has_name" | awk '{print $NF}' | sed 's/[{};]//g' | tr '[:upper:]' '[:lower:]')
            dict_label="$name_field"
        else
            # Fallback: use primary key
            dict_label="$primary_key::text"
        fi

        # Special handling for specific tables
        case "$table_name" in
            "event_payment_methods")
                dict_value="methodcode"
                dict_label="CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear)"
                ;;
            "event_locations")
                dict_value="locationid"
                dict_label="CONCAT(locationid, ' - ', locationname, ' - ', city)"
                ;;
            "hr_employee")
                dict_value="employeeid"
                dict_label="CONCAT(firstname, ' ', lastname, ' ', namear)"
                ;;
        esac

        # Generate SQL query
        cat >> "$OUTPUT_DIR/all_dict_sql_queries.sql" << SQL
-- ========================================
-- Table: $table_name ($class_name)
-- ========================================
SELECT
    $dict_value AS dictValue,
    $dict_label AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
WHERE isactive = '1' OR status = '0'  -- Active records only
ORDER BY $dict_value;

SQL

        # Create individual SQL file
        cat > "$OUTPUT_DIR/sql_${table_name}.sql" << SQL
-- Dictionary Query for $table_name
-- Class: $class_name
-- DictType: sql_$table_name

SELECT
    $dict_value AS dictValue,
    $dict_label AS dictLabel,
    remark,
    'sql_$table_name' AS dictType
FROM $table_name
WHERE isactive = '1' OR status = '0'
ORDER BY $dict_value;
SQL

        count=$((count + 1))
    fi
done

echo "Generated SQL queries for $count entities"
echo "Output: $OUTPUT_DIR/all_dict_sql_queries.sql"
