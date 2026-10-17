using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.IService.Finance.GL;
using ZR.Service.IService.Finance;
using ZR.Service.Finance;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Account Controller - متحكم الحسابات المالية
    /// Comprehensive chart of accounts management with IFRS compliance
    /// </summary>
    [Verify]
    [Route("finance/accounts")]
    [ApiController]
    public class GLAccountController : BaseController
    {
        private readonly IGLAccountService _accountService;
        //private readonly IGLAccountSeeder _accountSeeder;
        //private readonly IFinanceDataSeeder _financeDataSeeder;
        //private readonly ILocalFinanceDataSeeder _localFinanceDataSeeder;

        public GLAccountController(IGLAccountService accountService) //GLAccountSeeder accountSeeder, IFinanceDataSeeder financeDataSeeder, ILocalFinanceDataSeeder localFinanceDataSeeder
        {
            _accountService = accountService;
            //_accountSeeder = accountSeeder;
            //_financeDataSeeder = financeDataSeeder;
            //_localFinanceDataSeeder = localFinanceDataSeeder;
        }

        #region Account CRUD Operations

        /// <summary>
        /// Get list of accounts - الحصول على قائمة الحسابات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetList([FromQuery] GLAccountQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_accountService.GetList(query, pager));
        }

        /// <summary>
        /// Get account by ID - الحصول على الحساب بالمعرف
        /// </summary>
        [HttpGet("{id:guid}")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _accountService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Account not found - الحساب غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get account by code - الحصول على الحساب برقم الحساب
        /// </summary>
        [HttpGet("code/{accountNumber}")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetByAccountNumber(string accountNumber)
        {
            var response = _accountService.GetByAccountNumber(accountNumber);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Account not found - الحساب غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create account - إنشاء حساب
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:account:add")]
        [Log(Title = "GL Account", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                // Note: FullAccountNumber uniqueness is validated in the service layer
                // AccountNumber can be repeated across different parents (e.g., 1000.1100 and 2000.1100)
                var result = _accountService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Create account async - إنشاء حساب (غير متزامن)
        /// </summary>
        [HttpPost("async")]
        [ActionPermissionFilter(Permission = "finance:account:add")]
        [Log(Title = "GL Account", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                // Note: FullAccountNumber uniqueness is validated in the service layer
                // AccountNumber can be repeated across different parents (e.g., 1000.1100 and 2000.1100)
                var result = await _accountService.CreateAsync(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update account - تحديث حساب
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:account:edit")]
        [Log(Title = "GL Account", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                // Note: FullAccountNumber uniqueness is validated in the service layer
                // AccountNumber can be repeated across different parents
                return ToResponse(_accountService.Update(dto, HttpContext.GetName()));
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete accounts - حذف حسابات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:account:delete")]
        [Log(Title = "GL Account", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            // Validate each account can be deleted
            foreach (var id in idsArr)
            {
                var validation = _accountService.ValidateForDeletion(id);
                if (!validation.IsValid)
                    return ToResponse(ResultCode.FAIL, string.Join("; ", validation.Errors));
            }

            var result = _accountService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete accounts - لا يمكن حذف الحسابات");

            return ToResponse(result);
        }

        #endregion

        #region Account Query Operations

        /// <summary>
        /// Get account tree - الحصول على شجرة الحسابات
        /// </summary>
        [HttpGet("tree")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetAccountTree()
        {
            return SUCCESS(_accountService.GetAccountTree());
        }

        /// <summary>
        /// Get accounts by type - الحصول على الحسابات بالنوع
        /// </summary>
        [HttpGet("type/{accountType}")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetByType(AccountType accountType)
        {
            return SUCCESS(_accountService.GetByType(accountType));
        }

        /// <summary>
        /// Get leaf accounts (postable accounts) - الحصول على الحسابات القابلة للترحيل
        /// </summary>
        [HttpGet("leaf")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetLeafAccounts()
        {
            return SUCCESS(_accountService.GetLeafAccounts());
        }

        /// <summary>
        /// Get child accounts - الحصول على الحسابات الفرعية
        /// </summary>
        [HttpGet("{parentId:guid}/children")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetChildAccounts(Guid parentId)
        {
            return SUCCESS(_accountService.GetChildAccounts(parentId));
        }

        /// <summary>
        /// Get cash accounts - الحصول على حسابات الصندوق
        /// </summary>
        [HttpGet("cash")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetCashAccounts()
        {
            return SUCCESS(_accountService.GetCashAccounts());
        }

        /// <summary>
        /// Get bank accounts - الحصول على الحسابات البنكية
        /// </summary>
        [HttpGet("bank")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult GetBankAccounts()
        {
            return SUCCESS(_accountService.GetBankAccounts());
        }

        /// <summary>
        /// Search accounts - البحث عن الحسابات
        /// </summary>
        [HttpGet("search")]
        [ActionPermissionFilter(Permission = "finance:account:list")]
        public IActionResult Search([FromQuery] string searchTerm, [FromQuery] int maxResults = 20)
        {
            return SUCCESS(_accountService.Search(searchTerm, maxResults));
        }

        /// <summary>
        /// Get account hierarchy path - الحصول على مسار التسلسل الهرمي
        /// </summary>
        [HttpGet("{id:guid}/hierarchy")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetHierarchyPath(Guid id)
        {
            return SUCCESS(_accountService.GetHierarchyPath(id));
        }

        #endregion

        #region Account Balance Operations

        /// <summary>
        /// Get account balance for fiscal year - الحصول على رصيد الحساب للسنة المالية
        /// </summary>
        [HttpGet("{id:guid}/balance")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetBalance(Guid id, [FromQuery] int fiscalYear, [FromQuery] DateTime? asOfDate = null)
        {
            var balance = _accountService.GetAccountBalance(id, fiscalYear, asOfDate);
            return SUCCESS(balance);
        }

        /// <summary>
        /// Get aggregated balance including sub-accounts - الحصول على الرصيد المجمع شاملاً الحسابات الفرعية
        /// </summary>
        [HttpGet("{id:guid}/balance/aggregated")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetAggregatedBalance(Guid id, [FromQuery] int fiscalYear, [FromQuery] DateTime? asOfDate = null)
        {
            var balance = _accountService.GetAggregatedBalance(id, fiscalYear, asOfDate);
            return SUCCESS(balance);
        }

        /// <summary>
        /// Get balance summary for an account - الحصول على ملخص الرصيد
        /// </summary>
        [HttpPost("balance-summary")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetBalanceSummary([FromBody] BalanceSummaryQueryDto query)
        {
            return SUCCESS(_accountService.GetBalanceSummary(query));
        }

        /// <summary>
        /// Recalculate balance for account and period - إعادة حساب الرصيد
        /// </summary>
        [HttpPost("{id:guid}/balance/recalculate")]
        [ActionPermissionFilter(Permission = "finance:account:edit")]
        [Log(Title = "GL Account Balance", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RecalculateBalance(Guid id, [FromQuery] int fiscalYear, [FromQuery] int fiscalPeriod)
        {
            await _accountService.RecalculateBalanceAsync(id, fiscalYear, fiscalPeriod);
            return SUCCESS("Balance recalculated successfully - تم إعادة حساب الرصيد بنجاح");
        }

        #endregion

        #region Opening Balance Operations

        /// <summary>
        /// Get opening balances for fiscal year - الحصول على الأرصدة الافتتاحية للسنة المالية
        /// </summary>
        [HttpGet("opening-balances")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:list")]
        public IActionResult GetOpeningBalances([FromQuery] OpeningBalanceQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_accountService.GetOpeningBalances(query, pager));
        }

        /// <summary>
        /// Get opening balance by ID - الحصول على الرصيد الافتتاحي بالمعرف
        /// </summary>
        [HttpGet("opening-balances/{id:guid}")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:query")]
        public IActionResult GetOpeningBalanceById(Guid id)
        {
            var response = _accountService.GetOpeningBalanceById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Opening balance not found - الرصيد الافتتاحي غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create opening balance - إنشاء رصيد افتتاحي
        /// </summary>
        [HttpPost("opening-balances")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:add")]
        [Log(Title = "GL Opening Balance", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateOpeningBalance([FromBody] CreateOpeningBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _accountService.CreateOpeningBalance(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update opening balance - تحديث الرصيد الافتتاحي
        /// </summary>
        [HttpPut("opening-balances")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:edit")]
        [Log(Title = "GL Opening Balance", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateOpeningBalance([FromBody] UpdateOpeningBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            return ToResponse(_accountService.UpdateOpeningBalance(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete opening balance - حذف الرصيد الافتتاحي
        /// </summary>
        [HttpDelete("opening-balances/{id:guid}")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:delete")]
        [Log(Title = "GL Opening Balance", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteOpeningBalance(Guid id)
        {
            var result = _accountService.DeleteOpeningBalance(id);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete locked opening balance - لا يمكن حذف رصيد افتتاحي مقفل");
            return ToResponse(result);
        }

        /// <summary>
        /// Import bulk opening balances - استيراد الأرصدة الافتتاحية بالجملة
        /// </summary>
        [HttpPost("opening-balances/import")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:add")]
        [Log(Title = "GL Opening Balance Import", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> ImportOpeningBalances([FromBody] BulkOpeningBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = await _accountService.ImportOpeningBalancesAsync(dto, HttpContext.GetName());
            return SUCCESS(new { ImportedCount = result });
        }

        /// <summary>
        /// Lock opening balances for fiscal year - قفل الأرصدة الافتتاحية للسنة المالية
        /// </summary>
        [HttpPost("opening-balances/lock/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:lock")]
        [Log(Title = "GL Opening Balance Lock", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> LockOpeningBalances(int fiscalYear)
        {
            // Validate opening balance totals before locking
            var isBalanced = await _accountService.ValidateOpeningBalanceTotalsAsync(fiscalYear);
            if (!isBalanced)
                return ToResponse(ResultCode.FAIL, "Opening balances are not balanced (debits != credits) - الأرصدة الافتتاحية غير متوازنة");

            var userId = HttpContext.GetUId();
            // Create a deterministic GUID from the user ID
            var lockedByGuid = new Guid(userId.ToString().PadLeft(32, '0'));
            var result = await _accountService.LockOpeningBalancesAsync(fiscalYear, lockedByGuid);
            return SUCCESS(new { LockedCount = result });
        }

        /// <summary>
        /// Carryforward opening balances to new fiscal year - ترحيل الأرصدة الافتتاحية للسنة المالية الجديدة
        /// </summary>
        [HttpPost("opening-balances/carryforward")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:add")]
        [Log(Title = "GL Opening Balance Carryforward", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CarryforwardOpeningBalances([FromQuery] int newFiscalYear, [FromQuery] int previousFiscalYear)
        {
            var result = await _accountService.CarryforwardOpeningBalancesAsync(newFiscalYear, previousFiscalYear, HttpContext.GetName());
            return SUCCESS(new { CreatedCount = result });
        }

        /// <summary>
        /// Validate opening balance totals - التحقق من توازن الأرصدة الافتتاحية
        /// </summary>
        [HttpGet("opening-balances/validate/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:query")]
        public async Task<IActionResult> ValidateOpeningBalanceTotals(int fiscalYear)
        {
            var isValid = await _accountService.ValidateOpeningBalanceTotalsAsync(fiscalYear);
            return SUCCESS(new { FiscalYear = fiscalYear, IsBalanced = isValid });
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate account for posting - التحقق من صلاحية الحساب للترحيل
        /// </summary>
        [HttpGet("{id:guid}/validate/posting")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult ValidateForPosting(Guid id, [FromQuery] bool isManualEntry = true)
        {
            var result = _accountService.ValidateForPosting(id, isManualEntry);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate account for deletion - التحقق من إمكانية حذف الحساب
        /// </summary>
        [HttpGet("{id:guid}/validate/deletion")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult ValidateForDeletion(Guid id)
        {
            var result = _accountService.ValidateForDeletion(id);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check if account number is unique - التحقق من تفرد رقم الحساب
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult CheckCodeExists([FromQuery] string accountNumber, [FromQuery] Guid? excludeAccountId)
        {
            var exists = _accountService.CodeExists(accountNumber, excludeAccountId);
            return SUCCESS(new { Exists = exists });
        }

        /// <summary>
        /// Validate account hierarchy - التحقق من صحة التسلسل الهرمي
        /// </summary>
        [HttpGet("{id:guid}/validate/hierarchy")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult ValidateHierarchy(Guid id, [FromQuery] Guid? newParentId)
        {
            var isValid = _accountService.ValidateHierarchy(id, newParentId);
            return SUCCESS(new { IsValid = isValid });
        }

        #endregion

        #region Maintenance Operations

        /// <summary>
        /// Recalculate all account paths - إعادة حساب مسارات جميع الحسابات
        /// </summary>
        [HttpPost("recalculate-paths")]
        [ActionPermissionFilter(Permission = "finance:account:admin")]
        [Log(Title = "GL Account Path Recalculation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RecalculateAllPaths()
        {
            await _accountService.RecalculateAllPathsAsync();
            return SUCCESS("All account paths recalculated successfully - تم إعادة حساب جميع مسارات الحسابات بنجاح");
        }

        /// <summary>
        /// Get default normal balance for account type - الحصول على الرصيد الطبيعي الافتراضي لنوع الحساب
        /// </summary>
        [HttpGet("normal-balance/{accountType}")]
        [ActionPermissionFilter(Permission = "finance:account:query")]
        public IActionResult GetDefaultNormalBalance(AccountType accountType)
        {
            var normalBalance = _accountService.GetDefaultNormalBalance(accountType);
            return SUCCESS(new { AccountType = accountType, NormalBalance = normalBalance });
        }

        ///// <summary>
        ///// Seed default IFRS chart of accounts - زراعة دليل الحسابات الافتراضي
        ///// Creates the default IFRS compliant chart of accounts structure
        ///// </summary>
        //[HttpPost("seed")]
        //[ActionPermissionFilter(Permission = "finance:account:admin")]
        //[Log(Title = "GL Account Seeding", BusinessType = BusinessType.INSERT)]
        //public async Task<IActionResult> SeedDefaultAccounts([FromQuery] long? companyId = null)
        //{
        //    try
        //    {
        //        // Check if accounts already exist
        //        var exists = await _accountSeeder.DefaultAccountsExistAsync(companyId);
        //        if (exists)
        //        {
        //            return ToResponse(ResultCode.FAIL, "Default accounts already exist for this company - الحسابات الافتراضية موجودة مسبقاً لهذه الشركة");
        //        }

        //        var count = await _accountSeeder.SeedDefaultAccountsAsync(companyId, HttpContext.GetName());
        //        return SUCCESS(new
        //        {
        //            Message = $"Successfully created {count} default accounts - تم إنشاء {count} حساب افتراضي بنجاح",
        //            AccountsCreated = count
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return ToResponse(ResultCode.FAIL, ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Check if default accounts exist - التحقق من وجود الحسابات الافتراضية
        ///// </summary>
        //[HttpGet("seed/check")]
        //[ActionPermissionFilter(Permission = "finance:account:query")]
        //public async Task<IActionResult> CheckDefaultAccountsExist([FromQuery] long? companyId = null)
        //{
        //    var exists = await _accountSeeder.DefaultAccountsExistAsync(companyId);
        //    return SUCCESS(new { CompanyId = companyId, DefaultAccountsExist = exists });
        //}

        ///// <summary>
        ///// Recalculate all hierarchy counts - إعادة حساب جميع عدادات التسلسل الهرمي
        ///// </summary>
        //[HttpPost("recalculate-counts")]
        //[ActionPermissionFilter(Permission = "finance:account:admin")]
        //[Log(Title = "GL Account Count Recalculation", BusinessType = BusinessType.UPDATE)]
        //public async Task<IActionResult> RecalculateHierarchyCounts()
        //{
        //    await _accountSeeder.RecalculateHierarchyAsync();
        //    return SUCCESS("All hierarchy counts recalculated successfully - تم إعادة حساب جميع عدادات التسلسل الهرمي بنجاح");
        //}

        ///// <summary>
        ///// Seed accounts from external API (onemay.sa system) - زراعة الحسابات من النظام الخارجي
        ///// Fetches accounts from the external system and imports them into the local database
        ///// </summary>
        //[HttpPost("seed/external")]
        //[ActionPermissionFilter(Permission = "finance:account:admin")]
        //[Log(Title = "GL Account External Seeding", BusinessType = BusinessType.INSERT)]
        //public async Task<IActionResult> SeedAccountsFromExternalApi([FromBody] SeedExternalAccountsRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(request.ApiBaseUrl))
        //        {
        //            return ToResponse(ResultCode.PARAM_ERROR, "API Base URL is required - عنوان URL الأساسي مطلوب");
        //        }

        //        if (string.IsNullOrEmpty(request.BearerToken))
        //        {
        //            return ToResponse(ResultCode.PARAM_ERROR, "Bearer token is required - رمز Bearer مطلوب");
        //        }

        //        var count = await _accountSeeder.SeedAccountsFromExternalApiAsync(
        //            apiBaseUrl: request.ApiBaseUrl,
        //            bearerToken: request.BearerToken,
        //            companyId: request.CompanyId,
        //            createdBy: HttpContext.GetName()
        //        );

        //        return SUCCESS(new
        //        {
        //            Message = $"Successfully imported {count} accounts from external API - تم استيراد {count} حساب من النظام الخارجي بنجاح",
        //            AccountsImported = count,
        //            ApiBaseUrl = request.ApiBaseUrl
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return ToResponse(ResultCode.FAIL, $"Failed to import accounts from external API: {ex.Message} - فشل استيراد الحسابات من النظام الخارجي: {ex.Message}");
        //    }
        //}

        ///// <summary>
        ///// Seed ALL finance data locally - زراعة جميع البيانات المالية محلياً
        ///// Creates default data for: Currencies, Asset Categories, Customers, Vendors
        ///// </summary>
        //[HttpPost("seed/local/all")]
        //[ActionPermissionFilter(Permission = "finance:account:admin")]
        //[Log(Title = "All Finance Data Local Seeding", BusinessType = BusinessType.INSERT)]
        //public async Task<IActionResult> SeedAllLocalFinanceData([FromQuery] long? companyId = null)
        //{
        //    try
        //    {
        //        var result = await _localFinanceDataSeeder.SeedAllLocalDataAsync(
        //            companyId: companyId,
        //            createdBy: HttpContext.GetName()
        //        );

        //        if (result.Success)
        //        {
        //            return SUCCESS(new
        //            {
        //                Message = result.Message,
        //                TotalRecordsCreated = result.TotalRecordsCreated,
        //                CurrenciesCreated = result.CurrenciesCreated,
        //                AssetCategoriesCreated = result.AssetCategoriesCreated,
        //                CustomersCreated = result.CustomersCreated,
        //                VendorsCreated = result.VendorsCreated,
        //                BankAccountsCreated = result.BankAccountsCreated,
        //                CashAccountsCreated = result.CashAccountsCreated,
        //                TreasuryMappingsCreated = result.TreasuryMappingsCreated,
        //                TreasuryConfigCreated = result.TreasuryConfigCreated,
        //                Errors = result.Errors
        //            });
        //        }
        //        else
        //        {
        //            return ToResponse(ResultCode.FAIL, result.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ToResponse(ResultCode.FAIL, $"Failed to seed local finance data: {ex.Message} - فشل زراعة البيانات المالية المحلية: {ex.Message}");
        //    }
        //}

        ///// <summary>
        ///// Seed ALL finance data from external API - زراعة جميع البيانات المالية من النظام الخارجي
        ///// Imports GL Accounts, ARAP, Assets, Configuration, and Treasury data in one operation
        ///// </summary>
        //[HttpPost("seed/external/all")]
        //[ActionPermissionFilter(Permission = "finance:account:admin")]
        //[Log(Title = "All Finance Data External Seeding", BusinessType = BusinessType.INSERT)]
        //public async Task<IActionResult> SeedAllFinanceDataFromExternalApi([FromBody] SeedExternalAccountsRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(request.ApiBaseUrl))
        //        {
        //            return ToResponse(ResultCode.PARAM_ERROR, "API Base URL is required - عنوان URL الأساسي مطلوب");
        //        }

        //        if (string.IsNullOrEmpty(request.BearerToken))
        //        {
        //            return ToResponse(ResultCode.PARAM_ERROR, "Bearer token is required - رمز Bearer مطلوب");
        //        }

        //        var result = await _financeDataSeeder.SeedAllFinanceDataAsync(
        //            apiBaseUrl: request.ApiBaseUrl,
        //            bearerToken: request.BearerToken,
        //            companyId: request.CompanyId,
        //            createdBy: HttpContext.GetName()
        //        );

        //        if (result.Success)
        //        {
        //            return SUCCESS(new
        //            {
        //                Message = result.Message,
        //                TotalRecordsImported = result.TotalRecordsImported,
        //                GLAccountsImported = result.GLAccountsImported,
        //                CurrenciesImported = result.CurrenciesImported,
        //                FiscalYearsImported = result.FiscalYearsImported,
        //                TaxRulesImported = result.TaxRulesImported,
        //                CustomersImported = result.CustomersImported,
        //                VendorsImported = result.VendorsImported,
        //                AssetCategoriesImported = result.AssetCategoriesImported,
        //                BankAccountsImported = result.BankAccountsImported,
        //                CashAccountsImported = result.CashAccountsImported,
        //                ApiBaseUrl = request.ApiBaseUrl,
        //                Errors = result.Errors
        //            });
        //        }
        //        else
        //        {
        //            return ToResponse(ResultCode.FAIL, result.Message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ToResponse(ResultCode.FAIL, $"Failed to import finance data from external API: {ex.Message} - فشل استيراد البيانات المالية من النظام الخارجي: {ex.Message}");
        //    }
        //}

        #endregion

        #region Import Operations

        /// <summary>
        /// Import accounts from Excel file - استيراد الحسابات من ملف Excel
        /// </summary>
        [HttpPost("import/excel")]
        [ActionPermissionFilter(Permission = "finance:account:add")]
        [Log(Title = "GL Account Import", BusinessType = BusinessType.IMPORT)]
        public async Task<IActionResult> ImportFromExcel(IFormFile file, [FromQuery] bool updateExisting = false)
        {
            if (file == null || file.Length == 0)
                return ToResponse(ResultCode.PARAM_ERROR, "Please select a file to upload - يرجى اختيار ملف للتحميل");

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
                return ToResponse(ResultCode.PARAM_ERROR, "Only Excel files (.xlsx, .xls) are allowed - يُسمح بملفات Excel فقط");

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _accountService.ImportFromExcelAsync(stream, updateExisting, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Import failed: {ex.Message} - فشل الاستيراد: {ex.Message}");
            }
        }

        /// <summary>
        /// Rebuild account hierarchy - إعادة بناء التسلسل الهرمي للحسابات
        /// </summary>
        [HttpPost("rebuild-hierarchy")]
        [ActionPermissionFilter(Permission = "finance:account:admin")]
        [Log(Title = "GL Account Hierarchy Rebuild", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RebuildHierarchy()
        {
            try
            {
                await _accountService.RebuildAccountHierarchyAsync();
                return SUCCESS("Account hierarchy rebuilt successfully - تم إعادة بناء التسلسل الهرمي بنجاح");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to rebuild hierarchy: {ex.Message}");
            }
        }

        /// <summary>
        /// Fix NormalBalance for all accounts based on their AccountType
        /// This corrects accounts that were imported with incorrect NormalBalance values
        /// إصلاح الرصيد الطبيعي لجميع الحسابات بناءً على نوع الحساب
        /// </summary>
        [HttpPost("fix-normal-balances")]
        [ActionPermissionFilter(Permission = "finance:account:admin")]
        [Log(Title = "GL Account NormalBalance Fix", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> FixNormalBalances()
        {
            try
            {
                var fixedCount = await _accountService.FixAccountNormalBalancesAsync();
                return SUCCESS($"Fixed {fixedCount} accounts with incorrect NormalBalance - تم إصلاح {fixedCount} حساب");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to fix NormalBalance: {ex.Message}");
            }
        }

        #endregion

        #region Export Operations

        /// <summary>
        /// Export accounts - تصدير الحسابات
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "finance:account:export")]
        [Log(Title = "GL Account", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] GLAccountQueryDto query, [FromQuery] PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _accountService.GetList(query, pager).Result;
            var result = ExportExcelMini(list, "GL_Accounts", "Chart of Accounts - دليل الحسابات");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// Export opening balances - تصدير الأرصدة الافتتاحية
        /// </summary>
        [HttpGet("opening-balances/export")]
        [ActionPermissionFilter(Permission = "finance:opening-balance:export")]
        [Log(Title = "GL Opening Balance", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportOpeningBalances([FromQuery] OpeningBalanceQueryDto query, [FromQuery] PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _accountService.GetOpeningBalances(query, pager).Result;
            var result = ExportExcelMini(list, "GL_Opening_Balances", "Opening Balances - الأرصدة الافتتاحية");
            return ExportExcel(result.Item2, result.Item1);
        }

        #endregion
    }

    /// <summary>
    /// Request DTO for seeding accounts from external API
    /// </summary>
    //public class SeedExternalAccountsRequest
    //{
    //    /// <summary>
    //    /// Base URL of the external API (e.g., https://onemay.sa:8887)
    //    /// </summary>
    //    public string ApiBaseUrl { get; set; }

    //    /// <summary>
    //    /// Bearer token for authentication
    //    /// </summary>
    //    public string BearerToken { get; set; }

    //    /// <summary>
    //    /// Company ID to seed accounts for (optional)
    //    /// </summary>
    //    public long? CompanyId { get; set; }
    //}
}

