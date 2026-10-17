# Finance Data Seeder - Troubleshooting Guide

## Current Status

The seeder scripts are ready but the API connection is failing. Here's what to check:

## Issue: API Connection Refused

**Symptoms:**
- Script shows "Connection refused" or "تم إغلاق الاتصال"
- Port 8885 is listening but not accepting connections

**Solutions:**

### 1. Check API is Running
```powershell
netstat -ano | findstr :8885
```
Should show LISTENING status.

### 2. Try Different URL Formats
The API might be bound to a specific interface. Try:
- `http://localhost:8885`
- `https://localhost:8885`
- `http://127.0.0.1:8885`
- `https://127.0.0.1:8885`

### 3. Check API Logs
Look at the application console output for errors when the seeder endpoint is called.

### 4. Verify Endpoint Exists
Test the endpoint directly:
```powershell
$h = @{'Authorization'='Bearer YOUR_TOKEN'}
Invoke-RestMethod -Uri 'http://localhost:8885/finance/accounts/seed/local/all' -Method POST -Headers $h
```

## Why Data Might Not Be Inserted

The seeders have existence checks that prevent duplicate insertion:

### Bank/Cash Accounts
- Checks: `GetList(x => !x.IsDeleted)`
- If ANY bank/cash account exists (even deleted), returns 0
- **Fix**: Delete existing accounts or modify the check

### Asset Categories
- Checks: `GetAll()` - if any category exists, returns 0
- **Fix**: Delete existing categories or modify the check

### Treasury Mappings
- Checks: `GetFirst(x => x.MappingType == ChequesIssued)`
- If ChequesIssued mapping exists, returns 0
- **Fix**: Delete existing mappings or modify the check

## Force Re-seeding

To force re-seeding even if data exists, you can:

1. **Temporarily modify the seeder** to skip existence checks
2. **Delete existing data** from database
3. **Use the forceRecreate option** (for GL accounts only)

## Manual Database Check

Check if data was actually inserted:
```sql
-- Check GL Accounts
SELECT COUNT(*) FROM gl_account WHERE is_system_account = true;

-- Check Bank Accounts
SELECT COUNT(*) FROM bank_account WHERE is_deleted = false;

-- Check Cash Accounts  
SELECT COUNT(*) FROM cash_account WHERE is_deleted = false;

-- Check Treasury Mappings
SELECT COUNT(*) FROM treasury_gl_account_mapping;

-- Check Asset Categories
SELECT COUNT(*) FROM asset_category;
```

## Next Steps

1. **Ensure API is running and accessible**
2. **Check API logs for errors**
3. **Verify authentication token is valid**
4. **Check if data already exists** (seeders skip if data exists)
5. **Review seeder existence checks** if you want to force re-seeding





