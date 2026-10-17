#!/bin/bash

# Finance Data Seeder Script (Bash Version)
# This script seeds finance data using both Local and External seeders

API_BASE_URL="${API_BASE_URL:-https://localhost:8885}"
BEARER_TOKEN="${BEARER_TOKEN:-eyJhbGciOiJIUzI1NiIsInR5cCI6IkJlYXJlciJ9.eyJwcmltYXJ5c2lkIjoiMSIsIm5hbWVpZCI6IjEiLCJ1bmlxdWVfbmFtZSI6ImFkbWluIiwiZ3JvdXBzaWQiOiIwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6IntcIlVzZXJJZFwiOjEsXCJEZXB0SWRcIjowLFwiVXNlck5hbWVcIjpcImFkbWluXCIsXCJOaWNrTmFtZVwiOlwi2YXYr9mK2LFcIixcIlJvbGVLZXlzXCI6W1wiYWRtaW5cIl0sXCJSb2xlc1wiOlt7XCJSb2xlSWRcIjoxLFwiUm9sZUtleVwiOlwiYWRtaW5cIixcIkRhdGFTY29wZVwiOjF9XSxcIkV4cGlyZVRpbWVcIjpcIjAwMDEtMDEtMDFUMDA6MDA6MDBcIixcIlRlbmFudElkXCI6bnVsbCxcIlBlcm1pc3Npb25zXCI6W1wiKjoqOipcIl0sXCJDb21wYW55SURcIjowLFwiQnJhbmNoSURcIjowfSIsIkF1ZGllbmNlIjoibWFoYW0tYWkuTkVUIiwiSXNzdWVyIjoibWFoYW0tYWkuTkVUIiwibmJmIjoxNzY0ODc1MTc5LCJleHAiOjE3NjQ5NjE1NzksImlhdCI6MTc2NDg3NTE3OSwiaXNzIjoibWFoYW0tYWkuTkVUIiwiYXVkIjoibWFoYW0tYWkuTkVUIn0.qn4A3xrvBv1XyuLcLMHSUXmA_wQNCU-vfUig4eo8Q3k}"
COMPANY_ID="${COMPANY_ID:-0}"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

echo "========================================"
echo -e "${CYAN}Finance Data Seeder Script${NC}"
echo "========================================"
echo ""

# Function to make API calls
invoke_seeder_api() {
    local uri=$1
    local method=${2:-POST}
    local body=$3
    
    echo -e "${YELLOW}Calling: $uri${NC}"
    
    if [ -n "$body" ]; then
        response=$(curl -s -k -X "$method" \
            -H "Authorization: Bearer $BEARER_TOKEN" \
            -H "Content-Type: application/json" \
            -H "Accept: application/json" \
            -d "$body" \
            "$uri" 2>&1)
    else
        response=$(curl -s -k -X "$method" \
            -H "Authorization: Bearer $BEARER_TOKEN" \
            -H "Accept: application/json" \
            "$uri" 2>&1)
    fi
    
    if [ $? -eq 0 ] && [ -n "$response" ]; then
        # Check if response contains error
        if echo "$response" | grep -q '"code"'; then
            code=$(echo "$response" | grep -o '"code"[[:space:]]*:[[:space:]]*[0-9]*' | grep -o '[0-9]*')
            if [ "$code" = "200" ] || [ "$code" = "0" ]; then
                echo "$response"
                return 0
            else
                echo -e "${RED}Error: API returned code $code${NC}" >&2
                echo "$response" >&2
                return 1
            fi
        else
            echo "$response"
            return 0
        fi
    else
        echo -e "${RED}Connection Error: $response${NC}" >&2
        return 1
    fi
}

# 1. Seed Local Finance Data
echo "========================================"
echo -e "${GREEN}Step 1: Seeding Local Finance Data${NC}"
echo "========================================"
echo ""

LOCAL_URI="$API_BASE_URL/finance/accounts/seed/local/all"
if [ "$COMPANY_ID" != "0" ] && [ -n "$COMPANY_ID" ]; then
    LOCAL_URI="$LOCAL_URI?companyId=$COMPANY_ID"
fi

LOCAL_RESPONSE=$(invoke_seeder_api "$LOCAL_URI" "POST")

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Local seeding completed successfully!${NC}"
    echo ""
    
    # Parse and display results
    if command -v jq &> /dev/null; then
        echo -e "${CYAN}Results:${NC}"
        echo -e "${WHITE}  Total Records Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.totalRecordsCreated // 0')${NC}"
        echo -e "${WHITE}  Currencies Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.currenciesCreated // 0')${NC}"
        echo -e "${WHITE}  Asset Categories Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.assetCategoriesCreated // 0')${NC}"
        echo -e "${WHITE}  Customers Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.customersCreated // 0')${NC}"
        echo -e "${WHITE}  Vendors Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.vendorsCreated // 0')${NC}"
        echo -e "${WHITE}  Bank Accounts Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.bankAccountsCreated // 0')${NC}"
        echo -e "${WHITE}  Cash Accounts Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.cashAccountsCreated // 0')${NC}"
        echo -e "${WHITE}  Treasury Mappings Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.treasuryMappingsCreated // 0')${NC}"
        echo -e "${WHITE}  Treasury Config Created: $(echo "$LOCAL_RESPONSE" | jq -r '.data.treasuryConfigCreated // 0')${NC}"
        
        # Check for errors
        ERROR_COUNT=$(echo "$LOCAL_RESPONSE" | jq -r '.data.errors | length // 0')
        if [ "$ERROR_COUNT" -gt 0 ]; then
            echo ""
            echo -e "${YELLOW}Warnings/Errors:${NC}"
            echo "$LOCAL_RESPONSE" | jq -r '.data.errors[]?' | while read -r error; do
                echo -e "${YELLOW}  - $error${NC}"
            done
        fi
    else
        echo "$LOCAL_RESPONSE" | head -20
    fi
else
    echo -e "${RED}✗ Local seeding failed${NC}"
fi

echo ""

# 2. Seed External Finance Data
echo "========================================"
echo -e "${GREEN}Step 2: Seeding External Finance Data${NC}"
echo "========================================"
echo ""

EXTERNAL_URI="$API_BASE_URL/finance/accounts/seed/external/all"

REQUEST_BODY=$(cat <<EOF
{
  "apiBaseUrl": "$API_BASE_URL",
  "bearerToken": "$BEARER_TOKEN",
  "companyId": $COMPANY_ID
}
EOF
)

EXTERNAL_RESPONSE=$(invoke_seeder_api "$EXTERNAL_URI" "POST" "$REQUEST_BODY")

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ External seeding completed successfully!${NC}"
    echo ""
    
    # Parse and display results
    if command -v jq &> /dev/null; then
        echo -e "${CYAN}Results:${NC}"
        echo -e "${WHITE}  Total Records Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.totalRecordsImported // 0')${NC}"
        echo -e "${WHITE}  GL Accounts Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.glAccountsImported // 0')${NC}"
        echo -e "${WHITE}  Currencies Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.currenciesImported // 0')${NC}"
        echo -e "${WHITE}  Fiscal Years Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.fiscalYearsImported // 0')${NC}"
        echo -e "${WHITE}  Tax Rules Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.taxRulesImported // 0')${NC}"
        echo -e "${WHITE}  Customers Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.customersImported // 0')${NC}"
        echo -e "${WHITE}  Vendors Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.vendorsImported // 0')${NC}"
        echo -e "${WHITE}  Asset Categories Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.assetCategoriesImported // 0')${NC}"
        echo -e "${WHITE}  Bank Accounts Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.bankAccountsImported // 0')${NC}"
        echo -e "${WHITE}  Cash Accounts Imported: $(echo "$EXTERNAL_RESPONSE" | jq -r '.data.cashAccountsImported // 0')${NC}"
        
        # Check for errors
        ERROR_COUNT=$(echo "$EXTERNAL_RESPONSE" | jq -r '.data.errors | length // 0')
        if [ "$ERROR_COUNT" -gt 0 ]; then
            echo ""
            echo -e "${YELLOW}Warnings/Errors:${NC}"
            echo "$EXTERNAL_RESPONSE" | jq -r '.data.errors[]?' | while read -r error; do
                echo -e "${YELLOW}  - $error${NC}"
            done
        fi
    else
        echo "$EXTERNAL_RESPONSE" | head -20
    fi
else
    echo -e "${RED}✗ External seeding failed${NC}"
fi

echo ""
echo "========================================"
echo -e "${CYAN}Seeding Process Completed${NC}"
echo "========================================"
echo ""





