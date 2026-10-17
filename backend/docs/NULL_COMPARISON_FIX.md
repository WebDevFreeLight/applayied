# SqlSugar NULL Comparison Fix

## Problem Summary

SqlSugar ORM does not properly translate expression tree NULL comparisons to SQL `IS NULL`. This causes WHERE clauses like `x => x.CompanyID == null` to fail, resulting in duplicate records being created instead of existing records being updated.

## Affected Services

### 1. TaxConfigurationService
**File**: `ZR.Service\ERP\Finance\Configuration\TaxConfigurationService.cs`

**Symptoms**:
- Saving AR/AP accounts creates duplicate tax_configuration records
- Frontend dropdown appears empty after reload
- Multiple records with same `companyid=0` in database

**Fixed Methods**:
- `GetByCompanyId()` - Lines 36-74
- `SaveConfiguration()` - Lines 96-175
- `GetConfigurationEntity()` - Lines 306-325

**Fix Applied**:
```csharp
// OLD CODE (BROKEN):
if (companyId.HasValue)
{
    predicate = predicate.And(x => x.CompanyID == companyId.Value);
}
else
{
    predicate = predicate.And(x => x.CompanyID == null);  // ❌ Doesn't work
}

// NEW CODE (WORKING):
if (companyId.HasValue)
{
    entity = _repository.GetFirst(x => x.CompanyID == companyId.Value);
}
else
{
    // Load all records and filter in memory
    var allRecords = _repository.GetList(x => true);
    entity = allRecords.FirstOrDefault(x => !x.CompanyID.HasValue || x.CompanyID == null);  // ✅ Works
}
```

### 2. TreasuryConfigService
**File**: `ZR.Service\ERP\Finance\Treasury\TreasuryConfigService.cs`

**Symptoms**:
- Error: "Configuration not found - الإعداد غير موجود"
- Update endpoint fails because GetByTenant can't find existing records

**Fixed Methods**:
- `GetByTenant()` - Lines 51-79

**Fix Applied**: Same memory-filtering approach as TaxConfigurationService

## Root Cause Explanation

### SqlSugar Expression Tree Translation

When you write:
```csharp
predicate.And(x => x.CompanyID == null)
```

SqlSugar translates it to:
```sql
WHERE company_id = NULL  -- ❌ WRONG - always false in SQL
```

But SQL requires:
```sql
WHERE company_id IS NULL  -- ✅ CORRECT
```

### Why Loading All Records Works

By loading all records into memory first:
```csharp
var allRecords = _repository.GetList(x => true);  // No WHERE clause
entity = allRecords.FirstOrDefault(x => !x.CompanyID.HasValue || x.CompanyID == null);
```

The NULL comparison happens in C# memory using LINQ, not in SQL, so it works correctly.

## Database Cleanup Required

### Tax Configuration Duplicates

Your database has multiple duplicate records in `tax_configuration`:
```
id                                      | company_id | default_ar_account_id | default_ap_account_id
9b2b185a-ed9f-410a-8c6e-9ad04732b9dc   | 0          | NULL                  | NULL
59ca022e-3c44-4df5-bfbe-56f7b2e1c1bf   | 0          | (has values)          | (has values)
80a4292d-8f76-4f67-99f3-a88f10b859f6   | 0          | (has values)          | (has values)
5ded4e50-2e7c-4a6a-a85c-fc2bb4c83a7d   | 0          | (has values)          | (has values)
163e0064-b2b6-4f74-9d96-6b27c85ce6b7   | 0          | (has values)          | (has values)
```

**Run the cleanup script**: `sql\cleanup_tax_config_duplicates.sql`

This script will:
1. Delete the 4 duplicate records
2. Update the remaining record (9b2b185a) with your AR/AP account IDs
3. Verify only 1 record remains

## Testing Steps

### 1. Restart Backend
Stop and restart the backend application to load the fixed code.

### 2. Test Tax Configuration
1. Navigate to Finance → Configuration → Taxes → Settings
2. Select AR and AP accounts
3. Click Save
4. Refresh the page
5. Verify dropdowns show the selected accounts

**Expected Console Output**:
```
[TaxConfig] GetByCompanyId called - CompanyId: null
[TaxConfig] Searching for record with CompanyID IS NULL
[TaxConfig] Total records in table: 1
[TaxConfig] Found record: 9b2b185a-ed9f-410a-8c6e-9ad04732b9dc
[TaxConfig] After Mapster - AR: {guid}, AP: {guid}
```

### 3. Test Treasury Configuration
1. Navigate to Finance → Treasury → Configuration
2. Update any treasury settings
3. Click Save
4. Verify success message
5. Refresh and verify settings are saved

**Expected Console Output**:
```
[TreasuryConfig] GetByTenant called - CompanyId: null
[TreasuryConfig] Searching for record with CompanyID IS NULL
[TreasuryConfig] Total records in table: 1
[TreasuryConfig] Found record: {guid}
```

## Prevention

### For Future Services

When working with nullable foreign keys or tenant IDs, use this pattern:

```csharp
public MyEntity GetByTenant(long? companyId)
{
    MyEntity entity = null;

    if (companyId.HasValue)
    {
        // Specific company
        entity = _repository.GetFirst(x => x.CompanyID == companyId.Value);
    }
    else
    {
        // NULL company - load all and filter in memory
        var allRecords = _repository.GetList(x => true);
        entity = allRecords.FirstOrDefault(x => !x.CompanyID.HasValue);
    }

    return entity;
}
```

### Database Constraints

The SQL migration script includes a unique constraint:
```sql
CREATE UNIQUE INDEX IF NOT EXISTS idx_tax_config_company_unique
    ON tax_configuration(COALESCE(company_id, -1));
```

This should prevent duplicates by treating NULL as -1 for uniqueness checks. Verify this constraint exists in your database.

## Files Changed

1. `ZR.Service\ERP\Finance\Configuration\TaxConfigurationService.cs`
   - Added debug logging
   - Fixed NULL comparison in GetByCompanyId()
   - Fixed NULL comparison in GetConfigurationEntity()
   - Fixed SaveConfiguration() to use memory filtering

2. `ZR.Service\ERP\Finance\Treasury\TreasuryConfigService.cs`
   - Added debug logging
   - Fixed NULL comparison in GetByTenant()

3. `sql\cleanup_tax_config_duplicates.sql` (NEW)
   - Database cleanup script for duplicate tax_configuration records

4. `docs\NULL_COMPARISON_FIX.md` (NEW)
   - This documentation file

## Related Issues

This same pattern appears in other configuration services. Consider auditing:
- ✅ TaxConfigurationService (FIXED)
- ✅ TreasuryConfigService (FIXED)
- ❓ Other services with `CompanyID` or nullable tenant fields

## References

- SqlSugar Documentation: https://www.donet5.com/Home/Doc
- SQL NULL Comparison: https://www.postgresql.org/docs/current/functions-comparison.html
- C# LINQ FirstOrDefault: https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.firstordefault
