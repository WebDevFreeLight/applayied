#!/bin/bash

cd "$(dirname "$0")"

echo "[" > all_sql_dict_types.json

first=true
for file in sql_*.json; do
    if [ "$file" != "all_sql_dict_types.json" ]; then
        if [ "$first" = false ]; then
            printf "," >> all_sql_dict_types.json
        fi
        first=false

        printf "\n  " >> all_sql_dict_types.json
        cat "$file" | tr '\n' ' ' | sed 's/  */ /g' >> all_sql_dict_types.json
    fi
done

printf "\n]\n" >> all_sql_dict_types.json

echo "Created: all_sql_dict_types.json"
ls -lh all_sql_dict_types.json
