# Finance Data Seeder Scripts

This directory contains scripts to seed finance data into the database.

## seed-finance-data.ps1

PowerShell script to seed finance data using both Local and External seeders.

### Prerequisites

- PowerShell 5.1 or later
- Application must be running on the specified API base URL
- Valid bearer token for authentication

### Usage

#### Basic Usage (All defaults)
```powershell
.\scripts\seed-finance-data.ps1
```

This will:
- Use default API URL: `https://localhost:8885`
- Use the pre-configured bearer token
- Run both local and external seeders

#### Custom API URL and Token
```powershell
.\scripts\seed-finance-data.ps1 -ApiBaseUrl "https://localhost:8885" -BearerToken "your-token-here"
```

#### With Company ID
```powershell
.\scripts\seed-finance-data.ps1 -CompanyId 1
```

#### Local Seeder Only
```powershell
.\scripts\seed-finance-data.ps1 -LocalOnly
```

#### External Seeder Only
```powershell
.\scripts\seed-finance-data.ps1 -ExternalOnly
```

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ApiBaseUrl` | string | `https://localhost:8885` | Base URL of the API |
| `BearerToken` | string | *(pre-configured)* | Bearer token for authentication |
| `CompanyId` | long | `0` | Company ID (0 = null/default) |
| `LocalOnly` | switch | `false` | Run only local seeder |
| `ExternalOnly` | switch | `false` | Run only external seeder |
| `SkipCertificateValidation` | switch | `true` | Skip SSL certificate validation (for localhost) |

### What Gets Seeded

#### Local Seeder (`LocalFinanceDataSeeder`)
- GL Accounts (Chart of Accounts)
- Currencies (SAR, USD, EUR, GBP)
- Asset Categories (Vehicles, Equipment, Buildings, Furniture, Computers)
- Customers (3 sample customers)
- Vendors (3 sample vendors)
- Bank Accounts (Al Rajhi Bank, NCB)
- Cash Accounts (Main Cash Box, Petty Cash)
- Treasury Configuration

#### External Seeder (`FinanceDataSeeder`)
- GL Accounts (from external API)
- Currencies (from external API)
- Fiscal Years (from external API)
- Tax Rules (from external API)
- Customers (from external API)
- Vendors (from external API)
- Asset Categories (from external API)
- Bank Accounts (from external API)
- Cash Accounts (from external API)

### API Endpoints Used

- **Local Seeder**: `POST /finance/accounts/seed/local/all`
- **External Seeder**: `POST /finance/accounts/seed/external/all`

### Examples

#### Example 1: Seed all data locally
```powershell
.\scripts\seed-finance-data.ps1 -LocalOnly
```

#### Example 2: Import from external API only
```powershell
.\scripts\seed-finance-data.ps1 -ExternalOnly -ApiBaseUrl "https://your-api.com" -BearerToken "your-token"
```

#### Example 3: Seed both for specific company
```powershell
.\scripts\seed-finance-data.ps1 -CompanyId 1
```

### Troubleshooting

#### SSL Certificate Errors
The script automatically skips SSL certificate validation by default for localhost development. If you need to disable this:
```powershell
.\scripts\seed-finance-data.ps1 -SkipCertificateValidation:$false
```

#### Authentication Errors
Make sure your bearer token is valid and not expired. You can get a new token by logging into the application.

#### Connection Errors
Ensure the API is running and accessible at the specified URL. Test with:
```powershell
Invoke-WebRequest -Uri "https://localhost:8885/system/dict/data/type/sys_common_status" -Headers @{"Authorization"="Bearer your-token"}
```

### Output

The script provides detailed output showing:
- Success/failure status for each seeder
- Number of records created/imported for each data type
- Any warnings or errors encountered

Example output:
```
========================================
Finance Data Seeder Script
========================================

========================================
Step 1: Seeding Local Finance Data
========================================

✓ Local seeding completed successfully!

Results:
  Total Records Created: 18
  Currencies Created: 4
  Asset Categories Created: 5
  Customers Created: 3
  Vendors Created: 3
  Bank Accounts Created: 2
  Cash Accounts Created: 2
  Treasury Config Created: 1
```





