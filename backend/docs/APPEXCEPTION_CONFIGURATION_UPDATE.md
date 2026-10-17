# AppException and Predefined Messages Implementation

## Overview

Updated `TaxConfigurationService` and `TreasuryConfigService` to use **CustomException** with **predefined bilingual messages** and **structured logging** instead of Console.WriteLine statements.

## Changes Summary

### 1. Created ConfigurationMessages.cs

**File**: `ZR.Service\ERP\Finance\Configuration\ConfigurationMessages.cs`

A new static class containing all predefined bilingual messages for configuration services:

```csharp
public static class ConfigurationMessages
{
    // Tax Configuration Messages
    public const string TAX_CONFIG_NOT_FOUND =
        "إعدادات الضريبة غير موجودة - Tax configuration not found";

    public const string TAX_CONFIG_ALREADY_EXISTS =
        "إعدادات الضريبة موجودة بالفعل لهذه الشركة - Tax configuration already exists for this company";

    public const string TAX_CONFIG_SAVED_SUCCESSFULLY =
        "تم حفظ إعدادات الضريبة بنجاح - Tax configuration saved successfully";

    // Treasury Configuration Messages
    public const string TREASURY_CONFIG_NOT_FOUND =
        "إعدادات الخزينة غير موجودة - Treasury configuration not found";

    public const string TREASURY_CONFIG_ALREADY_EXISTS =
        "إعدادات الخزينة موجودة بالفعل لهذه الشركة - Treasury configuration already exists for this company";

    // General Configuration Messages
    public const string CONFIGURATION_SAVE_FAILED =
        "فشل في حفظ الإعدادات - Failed to save configuration";

    public const string CONFIGURATION_UPDATE_FAILED =
        "فشل في تحديث الإعدادات - Failed to update configuration";
}
```

**Benefits**:
- ✅ Centralized message management
- ✅ Consistent bilingual format: "Arabic - English"
- ✅ Easy to maintain and update
- ✅ Type-safe constants (no typos)
- ✅ Supports BilingualMessageHelper extraction

### 2. Updated TaxConfigurationService

**File**: `ZR.Service\ERP\Finance\Configuration\TaxConfigurationService.cs`

#### Changes:

**a) Added Dependencies**:
```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<TaxConfigurationService> _logger;

public TaxConfigurationService(
    TaxConfigurationRepository repository,
    IGLAccountRepository glAccountRepository,
    ILogger<TaxConfigurationService> logger)
{
    _repository = repository;
    _glAccountRepository = glAccountRepository;
    _logger = logger;
}
```

**b) Replaced Console.WriteLine with Structured Logging**:

**OLD CODE**:
```csharp
Console.WriteLine($"[TaxConfig] SaveConfiguration called - CompanyId: {dto.CompanyId}");
Console.WriteLine($"[TaxConfig] Update result: {result} rows affected");
```

**NEW CODE**:
```csharp
_logger.LogInformation($"[TaxConfig] SaveConfiguration called - CompanyId: {dto.CompanyId}");
_logger.LogDebug($"[TaxConfig] Update result: {result} rows affected");
_logger.LogWarning("[TaxConfig] No tax configuration found");
_logger.LogError(ex, "[TaxConfig] Failed to update tax configuration");
```

**c) Added CustomException with Predefined Messages**:

**OLD CODE**:
```csharp
// No exception thrown, just returns null or creates duplicate
if (existing != null)
{
    // Update
}
else
{
    // Create new - ALWAYS executed due to NULL comparison bug
}
```

**NEW CODE**:
```csharp
try
{
    // Update logic
    var result = _repository.Context.Updateable(existing)
        .UpdateColumns(x => new { ... })
        .ExecuteCommand();

    if (verified == null)
    {
        throw new CustomException(ConfigurationMessages.CONFIGURATION_UPDATE_FAILED);
    }
}
catch (Exception ex) when (!(ex is CustomException))
{
    _logger.LogError(ex, "[TaxConfig] Failed to update tax configuration");
    throw new CustomException(ConfigurationMessages.CONFIGURATION_UPDATE_FAILED);
}
```

**d) Updated Status Messages**:

**OLD CODE**:
```csharp
status.Message = "Tax configuration is complete.";
status.MessageAr = "إعدادات الضريبة مكتملة.";
```

**NEW CODE**:
```csharp
status.Message = ConfigurationMessages.TAX_CONFIG_SAVED_SUCCESSFULLY;
status.MessageAr = ConfigurationMessages.TAX_CONFIG_SAVED_SUCCESSFULLY;
```

### 3. Updated TreasuryConfigService

**File**: `ZR.Service\ERP\Finance\Treasury\TreasuryConfigService.cs`

#### Changes:

**a) Added Dependencies**:
```csharp
using Microsoft.Extensions.Logging;
using ZR.Service.ERP.Finance.Configuration;

private readonly ILogger<TreasuryConfigService> _logger;

public TreasuryConfigService(
    TreasuryConfigRepository repository,
    ILogger<TreasuryConfigService> logger)
{
    _repository = repository;
    _logger = logger;
}
```

**b) Replaced Console.WriteLine with Structured Logging**:

**OLD CODE**:
```csharp
Console.WriteLine($"[TreasuryConfig] GetByTenant called - CompanyId: {companyId}");
```

**NEW CODE**:
```csharp
_logger.LogDebug($"[TreasuryConfig] GetByTenant called - CompanyId: {companyId}");
```

**c) Updated Exception Messages**:

**OLD CODE**:
```csharp
throw new CustomException("Configuration not found - الإعداد غير موجود");
throw new CustomException("Configuration already exists for this tenant - الإعداد موجود بالفعل لهذا المستأجر");
```

**NEW CODE**:
```csharp
throw new CustomException(ConfigurationMessages.TREASURY_CONFIG_NOT_FOUND);
throw new CustomException(ConfigurationMessages.TREASURY_CONFIG_ALREADY_EXISTS);
```

**d) Added Try-Catch with Proper Error Handling**:

```csharp
try
{
    var entity = dto.Adapt<TreasuryConfig>();
    entity.Id = Guid.NewGuid();
    _repository.Insert(entity);
    _logger.LogInformation($"[TreasuryConfig] Created treasury configuration with ID: {entity.Id}");
    return entity;
}
catch (Exception ex) when (!(ex is CustomException))
{
    _logger.LogError(ex, "[TreasuryConfig] Failed to create treasury configuration");
    throw new CustomException(ConfigurationMessages.CONFIGURATION_SAVE_FAILED);
}
```

## Benefits of These Changes

### 1. Consistent Error Messaging
- ✅ All error messages follow "Arabic - English" format
- ✅ BilingualMessageHelper can extract language-specific message
- ✅ Frontend displays only the user's language

### 2. Structured Logging
- ✅ LogDebug: Detailed debugging information
- ✅ LogInformation: General information and success messages
- ✅ LogWarning: Non-critical issues (e.g., record not found)
- ✅ LogError: Exceptions and failures

### 3. Better Error Handling
- ✅ Exceptions are caught and re-thrown with meaningful messages
- ✅ Original exception is logged with stack trace
- ✅ CustomException preserves the message for frontend
- ✅ Prevents duplicate records due to silent failures

### 4. Centralized Message Management
- ✅ Easy to update messages in one place
- ✅ Consistent across all services
- ✅ Support for future localization improvements
- ✅ No hardcoded strings in service logic

### 5. Production-Ready Logging
- ✅ Console.WriteLine removed (not production-appropriate)
- ✅ ILogger uses configured logging providers (NLog, file, console, etc.)
- ✅ Log levels can be configured per environment
- ✅ Structured logging supports log aggregation tools

## Log Levels Guide

### When to Use Each Level:

**LogDebug**: Detailed diagnostic information
```csharp
_logger.LogDebug($"[TaxConfig] Searching for record with CompanyID = {companyId}");
_logger.LogDebug($"[TaxConfig] Total records in table: {allRecords.Count}");
```

**LogInformation**: General flow information
```csharp
_logger.LogInformation($"[TaxConfig] SaveConfiguration called - CompanyId: {dto.CompanyId}");
_logger.LogInformation($"[TaxConfig] Update successful - {result} rows affected");
```

**LogWarning**: Unexpected but non-critical situations
```csharp
_logger.LogWarning("[TaxConfig] No tax configuration found");
_logger.LogWarning($"[TreasuryConfig] Configuration already exists for TenantId: {dto.TenantId}");
```

**LogError**: Exceptions and critical failures
```csharp
_logger.LogError(ex, "[TaxConfig] Failed to update tax configuration");
_logger.LogError(ex, "[TreasuryConfig] Failed to create treasury configuration");
```

## Frontend Integration

The BilingualMessageHelper (created earlier) will extract the correct language from these messages:

```csharp
// Backend throws:
throw new CustomException("إعدادات الضريبة غير موجودة - Tax configuration not found");

// Frontend receives based on Accept-Language header:
// ar: "إعدادات الضريبة غير موجودة"
// en: "Tax configuration not found"
```

## Configuration

### appsettings.json - Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "ZR.Service.ERP.Finance.Configuration": "Debug",
      "ZR.Service.ERP.Finance.Treasury": "Debug"
    }
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "ZR.Service.ERP.Finance": "Debug"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "ZR.Service.ERP.Finance": "Information"
    }
  }
}
```

## Testing

### 1. Start Backend and Check Logs

After restarting the backend, you'll see structured logs:

```
info: ZR.Service.ERP.Finance.Configuration.TaxConfigurationService[0]
      [TaxConfig] SaveConfiguration called - CompanyId: null, AR: 13333333-..., AP: 23333333-...
dbug: ZR.Service.ERP.Finance.Configuration.TaxConfigurationService[0]
      [TaxConfig] Searching for record with CompanyID IS NULL
dbug: ZR.Service.ERP.Finance.Configuration.TaxConfigurationService[0]
      [TaxConfig] Total records in table: 1
info: ZR.Service.ERP.Finance.Configuration.TaxConfigurationService[0]
      [TaxConfig] Updating existing record: 9b2b185a-ed9f-410a-8c6e-9ad04732b9dc
info: ZR.Service.ERP.Finance.Configuration.TaxConfigurationService[0]
      [TaxConfig] Update successful - 1 rows affected
```

### 2. Test Error Messages

**Tax Configuration Not Found**:
```bash
GET /erp/finance/configuration/taxconfiguration/status

Response:
{
  "code": 200,
  "message": "إعدادات الضريبة غير موجودة - Tax configuration not found"
}
```

**Treasury Configuration Already Exists**:
```bash
POST /erp/finance/treasury/treasuryconfig

Response:
{
  "code": 500,
  "message": "إعدادات الخزينة موجودة بالفعل لهذه الشركة - Treasury configuration already exists for this company"
}
```

### 3. Verify Bilingual Message Extraction

The frontend should display only one language based on Accept-Language header.

## Files Modified

1. ✅ `ZR.Service\ERP\Finance\Configuration\ConfigurationMessages.cs` (NEW)
   - Predefined bilingual messages for all configuration services

2. ✅ `ZR.Service\ERP\Finance\Configuration\TaxConfigurationService.cs`
   - Added ILogger dependency
   - Replaced Console.WriteLine with structured logging
   - Added CustomException with predefined messages
   - Added try-catch error handling

3. ✅ `ZR.Service\ERP\Finance\Treasury\TreasuryConfigService.cs`
   - Added ILogger dependency
   - Replaced Console.WriteLine with structured logging
   - Updated CustomException to use predefined messages
   - Added try-catch error handling

4. ✅ `docs\APPEXCEPTION_CONFIGURATION_UPDATE.md` (NEW)
   - This documentation file

## Next Steps

1. ✅ **Build succeeded** - Code compiles without errors
2. ⏳ **Restart backend** - Load the updated services
3. ⏳ **Clean up database** - Run `sql\cleanup_tax_config_duplicates.sql`
4. ⏳ **Test tax configuration** - Save AR/AP accounts and verify dropdown
5. ⏳ **Test treasury configuration** - Update settings and verify save
6. ⏳ **Verify logs** - Check that structured logging is working
7. ⏳ **Test frontend** - Verify bilingual messages display correctly

## Related Documentation

- `docs\NULL_COMPARISON_FIX.md` - SqlSugar NULL comparison fix
- `ZR.Service\ERP\Finance\Validation\BilingualMessageHelper.cs` - Message extraction
- `Infrastructure\Exceptions\AppException.cs` - Exception base class
- `Infrastructure\CustomException\CustomException.cs` - Legacy exception class

## Migration Path

To migrate other services to this pattern:

1. Add `ILogger<YourService>` dependency
2. Create message constants in `ConfigurationMessages.cs` or new file
3. Replace Console.WriteLine with `_logger.LogXXX()`
4. Replace hardcoded exception messages with constants
5. Add try-catch blocks with proper error handling
6. Use `when (!(ex is CustomException))` to avoid catching CustomException

## Example for New Services

```csharp
public class YourConfigService
{
    private readonly ILogger<YourConfigService> _logger;

    public YourConfigService(ILogger<YourConfigService> logger)
    {
        _logger = logger;
    }

    public void SaveConfig(SaveConfigDto dto)
    {
        _logger.LogInformation($"[YourConfig] Saving configuration for: {dto.Name}");

        try
        {
            // Your logic here
            _logger.LogInformation("[YourConfig] Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[YourConfig] Failed to save configuration");
            throw new CustomException(ConfigurationMessages.CONFIGURATION_SAVE_FAILED);
        }
    }
}
```
