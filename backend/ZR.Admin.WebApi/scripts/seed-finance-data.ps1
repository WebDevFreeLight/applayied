# Finance Data Seeder Script
# This script seeds finance data using both Local and External seeders
# Script Location: scripts/seed-finance-data.ps1

param(
    [string]$ApiBaseUrl = "https://localhost:8885",
    [string]$BearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkJlYXJlciJ9.eyJwcmltYXJ5c2lkIjoiMSIsIm5hbWVpZCI6IjEiLCJ1bmlxdWVfbmFtZSI6ImFkbWluIiwiZ3JvdXBzaWQiOiIwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6IntcIlVzZXJJZFwiOjEsXCJEZXB0SWRcIjowLFwiVXNlck5hbWVcIjpcImFkbWluXCIsXCJOaWNrTmFtZVwiOlwi2YXYr9mK2LFcIixcIlJvbGVLZXlzXCI6W1wiYWRtaW5cIl0sXCJSb2xlc1wiOlt7XCJSb2xlSWRcIjoxLFwiUm9sZUtleVwiOlwiYWRtaW5cIixcIkRhdGFTY29wZVwiOjF9XSxcIkV4cGlyZVRpbWVcIjpcIjAwMDEtMDEtMDFUMDA6MDA6MDBcIixcIlRlbmFudElkXCI6bnVsbCxcIlBlcm1pc3Npb25zXCI6W1wiKjoqOipcIl0sXCJDb21wYW55SURcIjowLFwiQnJhbmNoSURcIjowfSIsIkF1ZGllbmNlIjoibWFoYW0tYWkuTkVUIiwiSXNzdWVyIjoibWFoYW0tYWkuTkVUIiwibmJmIjoxNzY0ODc1MTc5LCJleHAiOjE3NjQ5NjE1NzksImlhdCI6MTc2NDg3NTE3OSwiaXNzIjoibWFoYW0tYWkuTkVUIiwiYXVkIjoibWFoYW0tYWkuTkVUIn0.qn4A3xrvBv1XyuLcLMHSUXmA_wQNCU-vfUig4eo8Q3k",
    [long]$CompanyId = 0,
    [switch]$LocalOnly = $false,
    [switch]$ExternalOnly = $false,
    [switch]$SkipCertificateValidation = $true
)

# Ignore SSL certificate errors for localhost development
if ($SkipCertificateValidation) {
    if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
        $certCallback = @"
            using System;
            using System.Net;
            using System.Net.Security;
            using System.Security.Cryptography.X509Certificates;
            public class ServerCertificateValidationCallback
            {
                public static void Ignore()
                {
                    ServicePointManager.ServerCertificateValidationCallback += 
                        delegate
                        (
                            Object obj, 
                            X509Certificate certificate, 
                            X509Chain chain, 
                            SslPolicyErrors errors
                        )
                        {
                            return true;
                        };
                }
            }
"@
        Add-Type $certCallback
    }
    [ServerCertificateValidationCallback]::Ignore()
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Finance Data Seeder Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = $ApiBaseUrl.TrimEnd('/')
$headers = @{
    "Authorization" = "Bearer $BearerToken"
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

# Function to make API calls
function Invoke-SeederApi {
    param(
        [string]$Uri,
        [string]$Method = "POST",
        [object]$Body = $null
    )

        try {
        $params = @{
            Uri = $Uri
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
        }

        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10 -Compress
            $params.Body = $jsonBody
            $params.ContentType = "application/json"
        }

        Write-Host "Calling: $Uri" -ForegroundColor Yellow
        $response = Invoke-RestMethod @params -ErrorAction Stop
        
        # Handle response - it could be a PSCustomObject or Hashtable
        $responseObj = $response
        if ($responseObj -is [Hashtable] -or $responseObj -is [PSCustomObject]) {
            # Check if response has code/msg structure
            $code = if ($responseObj.code) { $responseObj.code } elseif ($responseObj.Code) { $responseObj.Code } else { 200 }
            $success = ($code -eq 200 -or $code -eq 0)
        }
        else {
            $success = $true
        }
        
        return @{
            Success = $success
            Data = $responseObj
            Error = $null
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($_.ErrorDetails) {
            try {
                $errorObj = $_.ErrorDetails.Message | ConvertFrom-Json
                if ($errorObj.msg) {
                    $errorMessage = $errorObj.msg
                }
                elseif ($errorObj.message) {
                    $errorMessage = $errorObj.message
                }
            }
            catch {
                # Keep original error message
            }
        }
        
        Write-Host "Error details: $($_.Exception)" -ForegroundColor Red
        
        return @{
            Success = $false
            Data = $null
            Error = $errorMessage
        }
    }
}

# 1. Seed Local Finance Data
if (-not $ExternalOnly) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Step 1: Seeding Local Finance Data" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""

    $localUri = "$baseUrl/finance/accounts/seed/local/all"
    if ($CompanyId -gt 0) {
        $localUri += "?companyId=$CompanyId"
    }

    $localResult = Invoke-SeederApi -Uri $localUri

    if ($localResult.Success) {
        Write-Host "✓ Local seeding completed successfully!" -ForegroundColor Green
        Write-Host ""
        
        # API response structure: { code: 200, msg: "...", data: {...} }
        $responseObj = $localResult.Data
        
        # Try to get data from response - handle both PSCustomObject and Hashtable
        $data = $null
        if ($responseObj.data) {
            $data = $responseObj.data
        }
        elseif ($responseObj.Data) {
            $data = $responseObj.Data
        }
        elseif ($responseObj -is [PSCustomObject]) {
            # Check if properties exist
            $props = $responseObj | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name
            if ($props -contains "data") {
                $data = $responseObj.data
            }
        }
        
        if ($data) {
            Write-Host "Results:" -ForegroundColor Cyan
            $totalRecords = if ($data.totalRecordsCreated) { $data.totalRecordsCreated } elseif ($data.TotalRecordsCreated) { $data.TotalRecordsCreated } else { 0 }
            Write-Host "  Total Records Created: $totalRecords" -ForegroundColor White
            
            $currencies = if ($data.currenciesCreated) { $data.currenciesCreated } elseif ($data.CurrenciesCreated) { $data.CurrenciesCreated } else { 0 }
            Write-Host "  Currencies Created: $currencies" -ForegroundColor White
            
            $assetCategories = if ($data.assetCategoriesCreated) { $data.assetCategoriesCreated } elseif ($data.AssetCategoriesCreated) { $data.AssetCategoriesCreated } else { 0 }
            Write-Host "  Asset Categories Created: $assetCategories" -ForegroundColor White
            
            $customers = if ($data.customersCreated) { $data.customersCreated } elseif ($data.CustomersCreated) { $data.CustomersCreated } else { 0 }
            Write-Host "  Customers Created: $customers" -ForegroundColor White
            
            $vendors = if ($data.vendorsCreated) { $data.vendorsCreated } elseif ($data.VendorsCreated) { $data.VendorsCreated } else { 0 }
            Write-Host "  Vendors Created: $vendors" -ForegroundColor White
            
            $bankAccounts = if ($data.bankAccountsCreated) { $data.bankAccountsCreated } elseif ($data.BankAccountsCreated) { $data.BankAccountsCreated } else { 0 }
            Write-Host "  Bank Accounts Created: $bankAccounts" -ForegroundColor White
            
            $cashAccounts = if ($data.cashAccountsCreated) { $data.cashAccountsCreated } elseif ($data.CashAccountsCreated) { $data.CashAccountsCreated } else { 0 }
            Write-Host "  Cash Accounts Created: $cashAccounts" -ForegroundColor White
            
            $treasuryMappings = if ($data.treasuryMappingsCreated) { $data.treasuryMappingsCreated } elseif ($data.TreasuryMappingsCreated) { $data.TreasuryMappingsCreated } else { 0 }
            Write-Host "  Treasury Mappings Created: $treasuryMappings" -ForegroundColor White
            
            $treasuryConfig = if ($data.treasuryConfigCreated) { $data.treasuryConfigCreated } elseif ($data.TreasuryConfigCreated) { $data.TreasuryConfigCreated } else { 0 }
            Write-Host "  Treasury Config Created: $treasuryConfig" -ForegroundColor White
            
            $errors = if ($data.errors) { $data.errors } elseif ($data.Errors) { $data.Errors } else { $null }
            if ($errors -and ($errors.Count -gt 0 -or ($errors -is [string] -and $errors.Length -gt 0))) {
                Write-Host ""
                Write-Host "Warnings/Errors:" -ForegroundColor Yellow
                if ($errors -is [array] -or $errors -is [System.Collections.ICollection]) {
                    foreach ($error in $errors) {
                        Write-Host "  - $error" -ForegroundColor Yellow
                    }
                }
                else {
                    Write-Host "  - $errors" -ForegroundColor Yellow
                }
            }
        }
        else {
            $msg = if ($responseObj.msg) { $responseObj.msg } elseif ($responseObj.Msg) { $responseObj.Msg } else { "Seeding completed" }
            Write-Host "Message: $msg" -ForegroundColor White
            Write-Host "Full Response:" -ForegroundColor Gray
            $responseObj | ConvertTo-Json -Depth 3 | Write-Host
        }
    }
    else {
        Write-Host "✗ Local seeding failed: $($localResult.Error)" -ForegroundColor Red
    }

    Write-Host ""
}

# 2. Seed External Finance Data
if (-not $LocalOnly) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Step 2: Seeding External Finance Data" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""

    $externalUri = "$baseUrl/finance/accounts/seed/external/all"
    
    $requestBody = @{
        ApiBaseUrl = $baseUrl
        BearerToken = $BearerToken
    }
    
    if ($CompanyId -gt 0) {
        $requestBody.CompanyId = $CompanyId
    }

    $externalResult = Invoke-SeederApi -Uri $externalUri -Body $requestBody

    if ($externalResult.Success) {
        Write-Host "✓ External seeding completed successfully!" -ForegroundColor Green
        Write-Host ""
        
        # API response structure: { code: 200, msg: "...", data: {...} }
        $responseObj = $externalResult.Data
        
        # Try to get data from response - handle both PSCustomObject and Hashtable
        $data = $null
        if ($responseObj.data) {
            $data = $responseObj.data
        }
        elseif ($responseObj.Data) {
            $data = $responseObj.Data
        }
        elseif ($responseObj -is [PSCustomObject]) {
            $props = $responseObj | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name
            if ($props -contains "data") {
                $data = $responseObj.data
            }
        }
        
        if ($data) {
            Write-Host "Results:" -ForegroundColor Cyan
            $totalRecords = if ($data.totalRecordsImported) { $data.totalRecordsImported } elseif ($data.TotalRecordsImported) { $data.TotalRecordsImported } else { 0 }
            Write-Host "  Total Records Imported: $totalRecords" -ForegroundColor White
            
            $glAccounts = if ($data.glAccountsImported) { $data.glAccountsImported } elseif ($data.GLAccountsImported) { $data.GLAccountsImported } else { 0 }
            Write-Host "  GL Accounts Imported: $glAccounts" -ForegroundColor White
            
            $currencies = if ($data.currenciesImported) { $data.currenciesImported } elseif ($data.CurrenciesImported) { $data.CurrenciesImported } else { 0 }
            Write-Host "  Currencies Imported: $currencies" -ForegroundColor White
            
            $fiscalYears = if ($data.fiscalYearsImported) { $data.fiscalYearsImported } elseif ($data.FiscalYearsImported) { $data.FiscalYearsImported } else { 0 }
            Write-Host "  Fiscal Years Imported: $fiscalYears" -ForegroundColor White
            
            $taxRules = if ($data.taxRulesImported) { $data.taxRulesImported } elseif ($data.TaxRulesImported) { $data.TaxRulesImported } else { 0 }
            Write-Host "  Tax Rules Imported: $taxRules" -ForegroundColor White
            
            $customers = if ($data.customersImported) { $data.customersImported } elseif ($data.CustomersImported) { $data.CustomersImported } else { 0 }
            Write-Host "  Customers Imported: $customers" -ForegroundColor White
            
            $vendors = if ($data.vendorsImported) { $data.vendorsImported } elseif ($data.VendorsImported) { $data.VendorsImported } else { 0 }
            Write-Host "  Vendors Imported: $vendors" -ForegroundColor White
            
            $assetCategories = if ($data.assetCategoriesImported) { $data.assetCategoriesImported } elseif ($data.AssetCategoriesImported) { $data.AssetCategoriesImported } else { 0 }
            Write-Host "  Asset Categories Imported: $assetCategories" -ForegroundColor White
            
            $bankAccounts = if ($data.bankAccountsImported) { $data.bankAccountsImported } elseif ($data.BankAccountsImported) { $data.BankAccountsImported } else { 0 }
            Write-Host "  Bank Accounts Imported: $bankAccounts" -ForegroundColor White
            
            $cashAccounts = if ($data.cashAccountsImported) { $data.cashAccountsImported } elseif ($data.CashAccountsImported) { $data.CashAccountsImported } else { 0 }
            Write-Host "  Cash Accounts Imported: $cashAccounts" -ForegroundColor White
            
            $errors = if ($data.errors) { $data.errors } elseif ($data.Errors) { $data.Errors } else { $null }
            if ($errors -and ($errors.Count -gt 0 -or ($errors -is [string] -and $errors.Length -gt 0))) {
                Write-Host ""
                Write-Host "Warnings/Errors:" -ForegroundColor Yellow
                if ($errors -is [array] -or $errors -is [System.Collections.ICollection]) {
                    foreach ($error in $errors) {
                        Write-Host "  - $error" -ForegroundColor Yellow
                    }
                }
                else {
                    Write-Host "  - $errors" -ForegroundColor Yellow
                }
            }
        }
        else {
            $msg = if ($responseObj.msg) { $responseObj.msg } elseif ($responseObj.Msg) { $responseObj.Msg } else { "Seeding completed" }
            Write-Host "Message: $msg" -ForegroundColor White
            Write-Host "Full Response:" -ForegroundColor Gray
            $responseObj | ConvertTo-Json -Depth 3 | Write-Host
        }
    }
    else {
        Write-Host "✗ External seeding failed: $($externalResult.Error)" -ForegroundColor Red
    }

    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Seeding Process Completed" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

