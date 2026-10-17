using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Service.IService.Finance.GL;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// Year-End Closing Controller - متحكم إغلاق نهاية السنة
    /// Handles fiscal year closing with P&amp;L transfer to Retained Earnings
    /// يعالج إغلاق السنة المالية مع تحويل الأرباح والخسائر إلى الأرباح المحتجزة
    /// </summary>
    [Verify]
    [Route("erp/finance/year-end")]
    [ApiController]
    public class YearEndClosingController : BaseController
    {
        private readonly IYearEndClosingService _yearEndClosingService;

        public YearEndClosingController(IYearEndClosingService yearEndClosingService)
        {
            _yearEndClosingService = yearEndClosingService;
        }

        /// <summary>
        /// Preview year-end closing (dry run)
        /// معاينة إغلاق نهاية السنة (تشغيل تجريبي)
        /// Returns detailed preview of what will happen on closing without making any changes
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to preview</param>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("preview/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public async Task<IActionResult> PreviewYearEndClosing(int fiscalYear, [FromQuery] long? companyId = null)
        {
            try
            {
                var result = await _yearEndClosingService.PreviewYearEndClosingAsync(fiscalYear, companyId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Validate fiscal year for closing
        /// التحقق من صلاحية إغلاق السنة المالية
        /// Returns detailed validation result with all checks
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to validate</param>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("validate/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public async Task<IActionResult> ValidateForClosing(int fiscalYear, [FromQuery] long? companyId = null)
        {
            try
            {
                var result = await _yearEndClosingService.ValidateForClosingAsync(fiscalYear, companyId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Check if fiscal year is already closed
        /// التحقق إذا كانت السنة المالية مغلقة مسبقاً
        /// Returns: { "code": 200, "data": { "isClosed": true|false, "fiscalYear": 2025 }, "msg": null }
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to check</param>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("check-closed/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public IActionResult CheckFiscalYearClosed(int fiscalYear, [FromQuery] long? companyId = null)
        {
            var isClosed = _yearEndClosingService.IsFiscalYearClosed(fiscalYear, companyId);
            return SUCCESS(new { IsClosed = isClosed, FiscalYear = fiscalYear });
        }

        /// <summary>
        /// Get net income for fiscal year
        /// الحصول على صافي الدخل للسنة المالية
        /// Returns revenue, expenses, and net income calculation
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("net-income/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public IActionResult GetNetIncome(int fiscalYear, [FromQuery] long? companyId = null)
        {
            var (totalRevenue, totalExpenses, netIncome) = _yearEndClosingService.GetNetIncome(fiscalYear, companyId);
            return SUCCESS(new
            {
                FiscalYear = fiscalYear,
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetIncome = netIncome,
                IsProfit = netIncome > 0,
                IsLoss = netIncome < 0
            });
        }

        /// <summary>
        /// Get retained earnings account
        /// الحصول على حساب الأرباح المحتجزة
        /// Returns the account ID of the retained earnings account for the company
        /// </summary>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("retained-earnings-account")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public IActionResult GetRetainedEarningsAccount([FromQuery] long? companyId = null)
        {
            var accountId = _yearEndClosingService.GetRetainedEarningsAccountId(companyId);
            if (accountId == null)
            {
                return SUCCESS(new
                {
                    Found = false,
                    AccountId = (Guid?)null,
                    Message = "No retained earnings account configured - لم يتم تكوين حساب الأرباح المحتجزة"
                });
            }
            return SUCCESS(new
            {
                Found = true,
                AccountId = accountId,
                Message = "Retained earnings account found - تم العثور على حساب الأرباح المحتجزة"
            });
        }

        /// <summary>
        /// Close fiscal year and transfer P&amp;L to Retained Earnings
        /// إغلاق السنة المالية وتحويل الربح/الخسارة إلى الأرباح المحتجزة
        /// This is an IFRS/GAAP compliant year-end closing operation
        /// </summary>
        /// <param name="dto">Closing parameters including fiscal year and optional retained earnings account</param>
        [HttpPost("close")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:close")]
        [Log(Title = "Year-End Closing", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CloseFiscalYear([FromBody] CloseFiscalYearDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            try
            {
                var userName = HttpContext.GetName();
                var result = await _yearEndClosingService.CloseFiscalYearAndTransferProfitLossAsync(dto, userName);

                if (!result.Success)
                {
                    // Return full result with errors so frontend can display them
                    return SUCCESS(result, result.Message);
                }

                return SUCCESS(result, result.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Preview closing entries (detailed journal entries that will be created)
        /// معاينة قيود الإغلاق (القيود المحاسبية التفصيلية التي سيتم إنشاؤها)
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to preview</param>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("preview-entries/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:year-end:query")]
        public async Task<IActionResult> PreviewClosingEntries(int fiscalYear, [FromQuery] long? companyId = null)
        {
            try
            {
                var preview = await _yearEndClosingService.PreviewYearEndClosingAsync(fiscalYear, companyId);

                if (!preview.CanClose)
                {
                    return SUCCESS(new
                    {
                        CanClose = false,
                        BlockingIssues = preview.BlockingIssues,
                        Warnings = preview.Warnings,
                        Entries = new object[] { }
                    });
                }

                // Build preview of journal entries
                var entries = new List<object>();

                // Revenue closing entries
                foreach (var revenue in preview.RevenueAccounts)
                {
                    entries.Add(new
                    {
                        Type = "Close Revenue",
                        AccountId = revenue.AccountId,
                        AccountCode = revenue.AccountCode,
                        AccountNameAr = revenue.AccountNameAr,
                        AccountNameEn = revenue.AccountNameEn,
                        Debit = revenue.ClosingDebit,
                        Credit = revenue.ClosingCredit,
                        DescriptionAr = $"إغلاق حساب الإيرادات - السنة المالية {fiscalYear}",
                        DescriptionEn = $"Close revenue account - FY {fiscalYear}"
                    });
                }

                // Expense closing entries
                foreach (var expense in preview.ExpenseAccounts)
                {
                    entries.Add(new
                    {
                        Type = "Close Expense",
                        AccountId = expense.AccountId,
                        AccountCode = expense.AccountCode,
                        AccountNameAr = expense.AccountNameAr,
                        AccountNameEn = expense.AccountNameEn,
                        Debit = expense.ClosingDebit,
                        Credit = expense.ClosingCredit,
                        DescriptionAr = $"إغلاق حساب المصروفات - السنة المالية {fiscalYear}",
                        DescriptionEn = $"Close expense account - FY {fiscalYear}"
                    });
                }

                // Retained earnings entry
                entries.Add(new
                {
                    Type = preview.IsProfit ? "Transfer Profit" : "Transfer Loss",
                    AccountId = preview.RetainedEarningsAccountId,
                    AccountCode = preview.RetainedEarningsAccountCode,
                    AccountNameAr = "الأرباح المحتجزة",
                    AccountNameEn = preview.RetainedEarningsAccountName,
                    Debit = preview.IsLoss ? Math.Abs(preview.NetIncome) : 0m,
                    Credit = preview.IsProfit ? preview.NetIncome : 0m,
                    DescriptionAr = $"تحويل صافي الدخل إلى الأرباح المحتجزة - السنة المالية {fiscalYear}",
                    DescriptionEn = $"Transfer net income to retained earnings - FY {fiscalYear}"
                });

                return SUCCESS(new
                {
                    CanClose = true,
                    FiscalYear = fiscalYear,
                    NetIncome = preview.NetIncome,
                    TotalRevenue = preview.TotalRevenue,
                    TotalExpenses = preview.TotalExpenses,
                    IsProfit = preview.IsProfit,
                    IsLoss = preview.IsLoss,
                    RetainedEarningsAccount = preview.RetainedEarningsAccountCode,
                    Warnings = preview.Warnings,
                    Entries = entries
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }
}
