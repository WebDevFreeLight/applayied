$headers = @{
    "Authorization" = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkJlYXJlciJ9.eyJwcmltYXJ5c2lkIjoiMSIsIm5hbWVpZCI6IjEiLCJ1bmlxdWVfbmFtZSI6ImFkbWluIiwiZ3JvdXBzaWQiOiIwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6IntcIlVzZXJJZFwiOjEsXCJEZXB0SWRcIjowLFwiVXNlck5hbWVcIjpcImFkbWluXCIsXCJOaWNrTmFtZVwiOlwi2YXYr9mK2LFcIixcIlJvbGVLZXlzXCI6W1wiYWRtaW5cIl0sXCJSb2xlc1wiOlt7XCJSb2xlSWRcIjoxLFwiUm9sZUtleVwiOlwiYWRtaW5cIixcIkRhdGFTY29wZVwiOjF9XSxcIkV4cGlyZVRpbWVcIjpcIjAwMDEtMDEtMDFUMDA6MDA6MDBcIixcIlRlbmFudElkXCI6bnVsbCxcIlBlcm1pc3Npb25zXCI6W1wiKjoqOipcIl0sXCJDb21wYW55SURcIjowLFwiQnJhbmNoSURcIjowfSIsIkF1ZGllbmNlIjoibWFoYW0tYWkuTkVUIiwiSXNzdWVyIjoibWFoYW0tYWkuTkVUIiwibmJmIjoxNzY0ODc1MTc5LCJleHAiOjE3NjQ5NjE1NzksImlhdCI6MTc2NDg3NTE3OSwiaXNzIjoibWFoYW0tYWkuTkVUIiwiYXVkIjoibWFoYW0tYWkuTkVUIn0.qn4A3xrvBv1XyuLcLMHSUXmA_wQNCU-vfUig4eo8Q3k"
    "Accept" = "application/json"
}

Write-Host "Testing API connection..." -ForegroundColor Yellow
try {
    $test = Invoke-RestMethod -Uri "http://localhost:8885/system/dict/data/type/sys_common_status" -Method GET -Headers $headers
    Write-Host "API is responding!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now running seeder..." -ForegroundColor Yellow
    Write-Host ""
    
    $response = Invoke-RestMethod -Uri "http://localhost:8885/finance/accounts/seed/local/all" -Method POST -Headers $headers
    
    Write-Host "SUCCESS - Seeding Completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Results:" -ForegroundColor Cyan
    Write-Host "  Total Records: $($response.data.totalRecordsCreated)" -ForegroundColor White
    Write-Host "  Currencies: $($response.data.currenciesCreated)" -ForegroundColor White
    Write-Host "  Asset Categories: $($response.data.assetCategoriesCreated)" -ForegroundColor White
    Write-Host "  Customers: $($response.data.customersCreated)" -ForegroundColor White
    Write-Host "  Vendors: $($response.data.vendorsCreated)" -ForegroundColor White
    Write-Host "  Bank Accounts: $($response.data.bankAccountsCreated)" -ForegroundColor White
    Write-Host "  Cash Accounts: $($response.data.cashAccountsCreated)" -ForegroundColor White
    Write-Host "  Treasury Mappings: $($response.data.treasuryMappingsCreated)" -ForegroundColor White
    Write-Host "  Treasury Config: $($response.data.treasuryConfigCreated)" -ForegroundColor White
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host $_.ErrorDetails.Message -ForegroundColor Yellow
    }
}





