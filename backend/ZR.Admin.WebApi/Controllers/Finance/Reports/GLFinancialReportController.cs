using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.IService.Finance.GL;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Financial Report Controller - التقارير المالية
    /// </summary>
    [Verify]
    [Route("api/erp/finance/reports")]
    [ApiController]
    public class GLFinancialReportController : BaseController
    {
        private readonly IGLFinancialReportService _reportService;

        public GLFinancialReportController(IGLFinancialReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Get paginated list of reports - الحصول على قائمة التقارير
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:finance:report:list")]
        public IActionResult GetList([FromQuery] GLFinancialReportQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _reportService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get report by ID - الحصول على التقرير بالمعرف
        /// </summary>
        [HttpGet("get/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetById(Guid id)
        {
            var result = _reportService.GetById(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Report not found - التقرير غير موجود");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get report by ID async - الحصول على التقرير بالمعرف (غير متزامن)
        /// </summary>
        [HttpGet("get-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _reportService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Report not found - التقرير غير موجود");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get reports by type - الحصول على التقارير بالنوع
        /// </summary>
        [HttpGet("by-type/{reportType}")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetByType(int reportType, [FromQuery] long? companyId = null)
        {
            var type = (ReportType)reportType;
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetByType(type, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get reports by period - الحصول على التقارير بالفترة
        /// </summary>
        [HttpGet("by-period")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetByPeriod([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetByPeriod(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate Trial Balance - توليد ميزان المراجعة
        /// </summary>
        [HttpPost("trial-balance")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Trial Balance", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateTrialBalance([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _reportService.GenerateTrialBalance(dto, userName);
            return SUCCESS(result, "Trial Balance generated successfully - تم توليد ميزان المراجعة بنجاح");
        }

        /// <summary>
        /// Generate Trial Balance async - توليد ميزان المراجعة (غير متزامن)
        /// </summary>
        [HttpPost("trial-balance-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Trial Balance", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateTrialBalanceAsync([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _reportService.GenerateTrialBalanceAsync(dto, userName);
            return SUCCESS(result, "Trial Balance generated successfully - تم توليد ميزان المراجعة بنجاح");
        }

        /// <summary>
        /// Generate Balance Sheet - توليد الميزانية العمومية
        /// </summary>
        [HttpPost("balance-sheet")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Balance Sheet", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateBalanceSheet([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _reportService.GenerateBalanceSheet(dto, userName);
            return SUCCESS(result, "Balance Sheet generated successfully - تم توليد الميزانية العمومية بنجاح");
        }

        /// <summary>
        /// Generate Balance Sheet async - توليد الميزانية العمومية (غير متزامن)
        /// </summary>
        [HttpPost("balance-sheet-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Balance Sheet", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateBalanceSheetAsync([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _reportService.GenerateBalanceSheetAsync(dto, userName);
            return SUCCESS(result, "Balance Sheet generated successfully - تم توليد الميزانية العمومية بنجاح");
        }

        /// <summary>
        /// Generate Income Statement - توليد قائمة الدخل
        /// </summary>
        [HttpPost("income-statement")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Income Statement", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateIncomeStatement([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _reportService.GenerateIncomeStatement(dto, userName);
            return SUCCESS(result, "Income Statement generated successfully - تم توليد قائمة الدخل بنجاح");
        }

        /// <summary>
        /// Generate Income Statement async - توليد قائمة الدخل (غير متزامن)
        /// </summary>
        [HttpPost("income-statement-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Income Statement", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateIncomeStatementAsync([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _reportService.GenerateIncomeStatementAsync(dto, userName);
            return SUCCESS(result, "Income Statement generated successfully - تم توليد قائمة الدخل بنجاح");
        }

        /// <summary>
        /// Generate Cash Flow - توليد قائمة التدفقات النقدية
        /// </summary>
        [HttpPost("cash-flow")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Cash Flow", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateCashFlow([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _reportService.GenerateCashFlow(dto, userName);
            return SUCCESS(result, "Cash Flow generated successfully - تم توليد قائمة التدفقات النقدية بنجاح");
        }

        /// <summary>
        /// Generate Cash Flow async - توليد قائمة التدفقات النقدية (غير متزامن)
        /// </summary>
        [HttpPost("cash-flow-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - Cash Flow", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateCashFlowAsync([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _reportService.GenerateCashFlowAsync(dto, userName);
            return SUCCESS(result, "Cash Flow generated successfully - تم توليد قائمة التدفقات النقدية بنجاح");
        }

        #region IAS 7 Cash Flow Statement (Indirect Method) - قائمة التدفقات النقدية وفق معيار IAS 7

        /// <summary>
        /// Generate IAS 7 Cash Flow Statement (Indirect Method)
        /// توليد قائمة التدفقات النقدية وفق معيار IAS 7 (الطريقة غير المباشرة)
        /// </summary>
        /// <remarks>
        /// IAS 7 compliant Cash Flow Statement using the Indirect Method:
        /// - Operating Activities: Start with Net Income, adjust for non-cash items and working capital changes
        /// - Investing Activities: Cash flows from acquisition/disposal of long-term assets
        /// - Financing Activities: Cash flows from changes in equity and borrowings
        /// </remarks>
        [HttpPost("cash-flow-ias7")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - IAS 7 Cash Flow", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateCashFlowIAS7([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            dto.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = _reportService.GenerateCashFlowIAS7(dto, userName);
            return SUCCESS(result, "IAS 7 Cash Flow Statement generated successfully - تم توليد قائمة التدفقات النقدية وفق IAS 7 بنجاح");
        }

        /// <summary>
        /// Generate IAS 7 Cash Flow Statement async (Indirect Method)
        /// توليد قائمة التدفقات النقدية وفق معيار IAS 7 (غير متزامن)
        /// </summary>
        [HttpPost("cash-flow-ias7-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:generate")]
        [Log(Title = "GL Financial Report - IAS 7 Cash Flow", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateCashFlowIAS7Async([FromBody] GenerateReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            dto.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = await _reportService.GenerateCashFlowIAS7Async(dto, userName);
            return SUCCESS(result, "IAS 7 Cash Flow Statement generated successfully - تم توليد قائمة التدفقات النقدية وفق IAS 7 بنجاح");
        }

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data (without saving to database)
        /// الحصول على بيانات قائمة التدفقات النقدية وفق IAS 7 (بدون الحفظ في قاعدة البيانات)
        /// </summary>
        /// <remarks>
        /// Returns the detailed IAS 7 Cash Flow Statement structure:
        /// - Opening Cash Balance
        /// - Operating Activities (Indirect Method): Net Income, Non-Cash Adjustments, Working Capital Changes
        /// - Investing Activities: PPE, Intangibles, Investments
        /// - Financing Activities: Equity, Borrowings, Dividends
        /// - Net Change in Cash
        /// - Closing Cash Balance
        /// - Reconciliation with actual cash accounts
        /// </remarks>
        [HttpGet("cash-flow-ias7-data")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetCashFlowDataIAS7([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetCashFlowDataIAS7(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get IAS 7 Cash Flow Statement Data async
        /// الحصول على بيانات قائمة التدفقات النقدية وفق IAS 7 (غير متزامن)
        /// </summary>
        [HttpGet("cash-flow-ias7-data-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetCashFlowDataIAS7Async([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetCashFlowDataIAS7Async(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        #endregion

        /// <summary>
        /// Get Trial Balance data - الحصول على بيانات ميزان المراجعة
        /// </summary>
        [HttpGet("trial-balance-data")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetTrialBalanceData([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetTrialBalanceData(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Trial Balance data async - الحصول على بيانات ميزان المراجعة (غير متزامن)
        /// </summary>
        [HttpGet("trial-balance-data-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetTrialBalanceDataAsync([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetTrialBalanceDataAsync(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        #region Enhanced Trial Balance (IFRS Compliant) - ميزان المراجعة المحسن

        /// <summary>
        /// Get Enhanced Trial Balance with IFRS-compliant structure
        /// الحصول على ميزان المراجعة المحسن مع هيكل متوافق مع IFRS
        /// </summary>
        /// <remarks>
        /// Returns enhanced Trial Balance with:
        /// - Hierarchical account structure (Parent → Child) with expand/collapse support
        /// - Opening Balance (Debit/Credit) - calculated from transactions before period start
        /// - Period Debits/Credits - transactions within the period
        /// - Closing Balance (Debit/Credit) - Opening + Period movements
        /// - Full Account Number, Account Type, Level, Currency
        /// - Roll-up totals for parent accounts
        ///
        /// Filters:
        /// - Date Range (required)
        /// - Company ID, Account Type, Level
        /// - Show Zero Balances, Show Hierarchy, Include Inactive
        /// </remarks>
        [HttpGet("trial-balance/enhanced")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetEnhancedTrialBalance([FromQuery] TrialBalanceQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = _reportService.GetEnhancedTrialBalance(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Enhanced Trial Balance async
        /// الحصول على ميزان المراجعة المحسن (غير متزامن)
        /// </summary>
        [HttpGet("trial-balance/enhanced-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetEnhancedTrialBalanceAsync([FromQuery] TrialBalanceQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetEnhancedTrialBalanceAsync(query);
            return SUCCESS(result);
        }

        #endregion

        /// <summary>
        /// Get Balance Sheet data (without saving) - الحصول على بيانات الميزانية العمومية
        /// </summary>
        /// <remarks>
        /// Returns the Balance Sheet structure:
        /// - Assets: All asset accounts with balances
        /// - Liabilities: All liability accounts with balances
        /// - Equity: All equity accounts with balances
        /// - Totals for each section
        /// </remarks>
        [HttpGet("balance-sheet-data")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetBalanceSheetData([FromQuery] DateTime asOfDate, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetBalanceSheetData(asOfDate, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Balance Sheet data async - الحصول على بيانات الميزانية العمومية (غير متزامن)
        /// </summary>
        [HttpGet("balance-sheet-data-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetBalanceSheetDataAsync([FromQuery] DateTime asOfDate, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetBalanceSheetDataAsync(asOfDate, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get IFRS-compliant Balance Sheet data
        /// الحصول على بيانات الميزانية العمومية وفق المعايير الدولية (IAS 1)
        /// </summary>
        /// <remarks>
        /// Returns IFRS-compliant Balance Sheet with:
        /// - Current Assets: Cash, receivables, inventory, prepaid expenses
        /// - Non-Current Assets: PPE, intangibles, investments
        /// - Current Liabilities: Payables, accrued expenses, short-term loans
        /// - Non-Current Liabilities: Long-term loans, deferred tax
        /// - Equity: Share capital, retained earnings, reserves
        /// - Balance validation: Assets = Liabilities + Equity
        ///
        /// View modes:
        /// - "hierarchical": Tree view with roll-up totals (default)
        /// - "flat": Simple list without hierarchy
        /// </remarks>
        /// <param name="asOfDate">Report date - تاريخ التقرير</param>
        /// <param name="viewMode">View mode: "hierarchical" or "flat" - وضع العرض</param>
        /// <param name="companyId">Company ID (optional) - معرف الشركة</param>
        /// <param name="includePreviousPeriod">Include comparative period - تضمين الفترة المقارنة</param>
        /// <returns>IFRS-compliant balance sheet report</returns>
        [HttpGet("balance-sheet/ifrs")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetIFRSBalanceSheet(
            [FromQuery] DateTime asOfDate,
            [FromQuery] string viewMode = "hierarchical",
            [FromQuery] long? companyId = null,
            [FromQuery] bool includePreviousPeriod = false)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var query = new BalanceSheetQueryDto
            {
                AsOfDate = asOfDate,
                CompanyId = actualCompanyId,
                ViewMode = viewMode,
                IncludePreviousPeriod = includePreviousPeriod
            };
            var result = await _reportService.GetIFRSBalanceSheetAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Income Statement data (without saving) - الحصول على بيانات قائمة الدخل
        /// </summary>
        /// <remarks>
        /// Returns the Income Statement structure:
        /// - Revenue: All revenue accounts with balances
        /// - Expenses: All expense accounts with balances
        /// - Total Revenue, Total Expenses, Net Income
        /// </remarks>
        [HttpGet("income-statement-data")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetIncomeStatementData([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetIncomeStatementData(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Income Statement data async - الحصول على بيانات قائمة الدخل (غير متزامن)
        /// </summary>
        [HttpGet("income-statement-data-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetIncomeStatementDataAsync([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetIncomeStatementDataAsync(periodStart, periodEnd, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get IFRS-compliant Income Statement data
        /// الحصول على بيانات قائمة الدخل وفق المعايير الدولية (IAS 1)
        /// </summary>
        /// <remarks>
        /// Returns comprehensive Income Statement with:
        /// - Revenue: Operating revenue accounts
        /// - Cost of Goods Sold (COGS): Direct costs
        /// - Gross Profit: Revenue - COGS
        /// - Operating Expenses: SG&A and other operating costs
        /// - Operating Profit: Gross Profit - Operating Expenses
        /// - Other Income/Expenses: Non-operating items
        /// - Net Profit Before Tax / Net Profit
        ///
        /// Supports:
        /// - Hierarchical view (tree structure with roll-up totals)
        /// - Flat view (simple list)
        /// - Previous period comparison
        /// </remarks>
        /// <param name="periodStart">Period start date - تاريخ بداية الفترة</param>
        /// <param name="periodEnd">Period end date - تاريخ نهاية الفترة</param>
        /// <param name="viewMode">View mode: "hierarchical" or "flat" (default: hierarchical)</param>
        /// <param name="companyId">Optional company ID filter</param>
        /// <param name="includePreviousPeriod">Include previous period comparison (default: false)</param>
        [HttpGet("income-statement/ifrs")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetIFRSIncomeStatement(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] string viewMode = "hierarchical",
            [FromQuery] long? companyId = null,
            [FromQuery] bool includePreviousPeriod = false)
        {
            var query = new IncomeStatementQueryDto
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                CompanyId = companyId,
                ViewMode = viewMode,
                IncludePreviousPeriod = includePreviousPeriod
            };
            var result = await _reportService.GetIFRSIncomeStatementAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete report - حذف التقرير
        /// </summary>
        [HttpDelete("delete/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:report:delete")]
        [Log(Title = "GL Financial Report", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _reportService.Delete(id);
            if (result > 0)
            {
                return SUCCESS("Report deleted successfully - تم حذف التقرير بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to delete report - فشل حذف التقرير");
        }

        /// <summary>
        /// Delete report async - حذف التقرير (غير متزامن)
        /// </summary>
        [HttpDelete("delete-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:report:delete")]
        [Log(Title = "GL Financial Report", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _reportService.DeleteAsync(id);
            if (result > 0)
            {
                return SUCCESS("Report deleted successfully - تم حذف التقرير بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to delete report - فشل حذف التقرير");
        }

        #region Cost Center & Project P&L Reports - تقارير الربح والخسارة حسب مركز التكلفة والمشروع

        /// <summary>
        /// Get Cost Center P&L Report - تقرير الربح والخسارة حسب مركز التكلفة
        /// </summary>
        /// <remarks>
        /// Returns P&L statement grouped by cost center, showing:
        /// - Revenue accounts and amounts per cost center
        /// - Expense accounts and amounts per cost center
        /// - Net profit/loss per cost center
        /// - Overall totals
        /// </remarks>
        [HttpGet("cost-center-pl")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetCostCenterPLReport([FromQuery] CostCenterPLQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = _reportService.GetCostCenterPLReport(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Cost Center P&L Report async - تقرير الربح والخسارة حسب مركز التكلفة (غير متزامن)
        /// </summary>
        [HttpGet("cost-center-pl-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetCostCenterPLReportAsync([FromQuery] CostCenterPLQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetCostCenterPLReportAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Project P&L Report - تقرير الربح والخسارة حسب المشروع
        /// </summary>
        /// <remarks>
        /// Returns P&L statement grouped by project, showing:
        /// - Revenue accounts and amounts per project
        /// - Expense accounts and amounts per project
        /// - Net profit/loss per project
        /// - Project status and profitability metrics
        /// - Overall totals
        /// </remarks>
        [HttpGet("project-pl")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetProjectPLReport([FromQuery] ProjectPLQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = _reportService.GetProjectPLReport(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Project P&L Report async - تقرير الربح والخسارة حسب المشروع (غير متزامن)
        /// </summary>
        [HttpGet("project-pl-async")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public async Task<IActionResult> GetProjectPLReportAsync([FromQuery] ProjectPLQueryDto query)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            query.CompanyId ??= (long)HttpContext.GetCompanyId();
            var result = await _reportService.GetProjectPLReportAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Cost Center P&L Summary - ملخص الربح والخسارة حسب مراكز التكلفة
        /// For dashboard widgets showing top performing/underperforming cost centers
        /// </summary>
        [HttpGet("cost-center-pl-summary")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetCostCenterPLSummary(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] long? companyId = null,
            [FromQuery] int topN = 10)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetCostCenterPLSummary(periodStart, periodEnd, actualCompanyId, topN);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get Project P&L Summary - ملخص الربح والخسارة حسب المشاريع
        /// For dashboard widgets showing top performing/underperforming projects
        /// </summary>
        [HttpGet("project-pl-summary")]
        [ActionPermissionFilter(Permission = "erp:finance:report:query")]
        public IActionResult GetProjectPLSummary(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] long? companyId = null,
            [FromQuery] int topN = 10)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _reportService.GetProjectPLSummary(periodStart, periodEnd, actualCompanyId, topN);
            return SUCCESS(result);
        }

        #endregion
    }
}

