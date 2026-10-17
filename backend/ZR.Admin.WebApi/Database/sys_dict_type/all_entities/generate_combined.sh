#!/bin/bash

cd "/f/mahamBackendNET/backend/ZR.Admin.WebApi/Database/sys_dict_type/all_entities"

echo "[" > all_system_dict_types.json

first=true
tail -n +3 /tmp/all_entities.txt | while IFS='|' read -r table_name class_name namespace file_path; do
    if [ ! -z "$table_name" ]; then
        class_name=$(echo "$class_name" | cut -d':' -f1)

        if [ "$first" != true ]; then
            printf "," >> all_system_dict_types.json
        fi
        first=false

        # Read the individual JSON file
        if [ -f "${table_name}.json" ]; then
            printf "\n  " >> all_system_dict_types.json
            cat "${table_name}.json" | tr '\n' ' ' | sed 's/{  /{ /g' | sed 's/  }/ }/g' >> all_system_dict_types.json
        fi
    fi
done

printf "\n]\n" >> all_system_dict_types.json

echo "Done! Created all_system_dict_types.json"
wc -l all_system_dict_types.json
