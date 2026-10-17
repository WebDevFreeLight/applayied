using System;
using System.Threading.Tasks;
using Infrastructure.Controllers;
using Infrastructure.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Finance.Tax.Dto;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Admin.WebApi.Controllers.Finance.Tax
{
    /// <summary>
    /// Tax Calculation API Controller
    /// THE MAIN TAX CALCULATION ENGINE ENDPOINT
    /// وحدة تحكم API حساب الضرائب - نقطة النهاية الرئيسية لمحرك حساب الضرائب
    /// </summary>
    [Verify]
    [Route("api/finance/tax/calculation")]
    [ApiController]
    [Authorize]
    public class TaxCalculationController : BaseController
    {
        private readonly ITaxCalculationService _taxCalculationService;
        private readonly ITaxPostingService _taxPostingService;

        public TaxCalculationController(
            ITaxCalculationService taxCalculationService,
            ITaxPostingService taxPostingService)
        {
            _taxCalculationService = taxCalculationService;
            _taxPostingService = taxPostingService;
        }

        #region Calculation Operations

        /// <summary>
        /// Calculate tax for a request (preview)
        /// This is the MAIN calculation endpoint
        /// حساب الضريبة للطلب (معاينة) - نقطة النهاية الرئيسية للحساب
        /// </summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] TaxCalculationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var result = await _taxCalculationService.CalculateAsync(request);

            if (!result.Success)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, string.Join("; ", result.Errors));
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Validate calculation request
        /// التحقق من صحة طلب الحساب
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] TaxCalculationRequestDto request)
        {
            var result = await _taxCalculationService.ValidateRequestAsync(request);
            return SUCCESS(result);
        }

        #endregion

        #region Simple Calculation Helpers

        /// <summary>
        /// Calculate tax on exclusive amount (simple)
        /// حساب الضريبة على مبلغ غير شامل الضريبة (بسيط)
        /// </summary>
        [HttpGet("tax-on-exclusive")]
        public IActionResult CalculateTaxOnExclusive(
            [FromQuery] decimal amount,
            [FromQuery] decimal taxRate,
            [FromQuery] int decimalPlaces = 2)
        {
            var taxAmount = _taxCalculationService.CalculateTaxOnExclusive(amount, taxRate, decimalPlaces);
            var total = _taxCalculationService.GetTotalFromExclusive(amount, taxRate, decimalPlaces);

            return SUCCESS(new
            {
                OriginalAmount = amount,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = total,
                IsInclusive = false
            });
        }

        /// <summary>
        /// Extract tax from inclusive amount (simple)
        /// استخراج الضريبة من مبلغ شامل الضريبة (بسيط)
        /// </summary>
        [HttpGet("tax-from-inclusive")]
        public IActionResult ExtractTaxFromInclusive(
            [FromQuery] decimal amount,
            [FromQuery] decimal taxRate,
            [FromQuery] int decimalPlaces = 2)
        {
            var taxAmount = _taxCalculationService.ExtractTaxFromInclusive(amount, taxRate, decimalPlaces);
            var taxableAmount = _taxCalculationService.GetTaxableFromInclusive(amount, taxRate, decimalPlaces);

            return SUCCESS(new
            {
                OriginalAmount = amount,
                TaxRate = taxRate,
                TaxableAmount = taxableAmount,
                TaxAmount = taxAmount,
                TotalAmount = amount,
                IsInclusive = true
            });
        }

        /// <summary>
        /// Convert to base currency
        /// التحويل إلى العملة الأساسية
        /// </summary>
        [HttpGet("convert-to-base")]
        public IActionResult ConvertToBase(
            [FromQuery] decimal foreignAmount,
            [FromQuery] decimal exchangeRate,
            [FromQuery] int decimalPlaces = 2)
        {
            var baseAmount = _taxCalculationService.ConvertToBase(foreignAmount, exchangeRate, decimalPlaces);

            return SUCCESS(new
            {
                ForeignAmount = foreignAmount,
                ExchangeRate = exchangeRate,
                BaseAmount = baseAmount
            });
        }

        #endregion

        #region Hash Verification

        /// <summary>
        /// Verify calculation hash
        /// التحقق من صحة هاش الحساب
        /// </summary>
        [HttpPost("verify-hash")]
        public IActionResult VerifyHash([FromBody] TaxCalculationResultDto result, [FromQuery] string hash)
        {
            var isValid = _taxCalculationService.VerifyCalculationHash(result, hash);
            return SUCCESS(new { IsValid = isValid });
        }

        #endregion

        #region Reporting Operations

        /// <summary>
        /// Get tax summary by tax code for reporting
        /// الحصول على ملخص الضرائب حسب رمز الضريبة للتقارير
        /// </summary>
        [HttpGet("summary/by-code")]
        public async Task<IActionResult> GetTaxSummaryByCode(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? companyId = null)
        {
            var result = await _taxPostingService.GetTaxSummaryByCodeAsync(fromDate, toDate, companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get period totals for reporting
        /// الحصول على إجماليات الفترة للتقارير
        /// </summary>
        [HttpGet("summary/period-totals")]
        public async Task<IActionResult> GetPeriodTotals(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? companyId = null)
        {
            var result = await _taxPostingService.GetPeriodTotalsAsync(fromDate, toDate, companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get recoverable summary (input vs output VAT)
        /// الحصول على ملخص الضريبة المستردة (ضريبة المدخلات مقابل ضريبة المخرجات)
        /// </summary>
        [HttpGet("summary/recoverable")]
        public async Task<IActionResult> GetRecoverableSummary(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? companyId = null)
        {
            var result = await _taxPostingService.GetRecoverableSummaryAsync(fromDate, toDate, companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax journal lines by date range
        /// الحصول على سطور دفتر الضرائب حسب نطاق التاريخ
        /// </summary>
        [HttpGet("journal-lines")]
        public async Task<IActionResult> GetTaxJournalLines(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? companyId = null)
        {
            var result = await _taxPostingService.GetTaxJournalLinesByDateRangeAsync(fromDate, toDate, companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax journal lines by journal ID
        /// الحصول على سطور دفتر الضرائب بواسطة معرف القيد
        /// </summary>
        [HttpGet("journal-lines/by-journal/{journalId}")]
        public async Task<IActionResult> GetTaxJournalLinesByJournal(Guid journalId)
        {
            var result = await _taxPostingService.GetTaxJournalLinesByJournalAsync(journalId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax journal lines by invoice ID
        /// الحصول على سطور دفتر الضرائب بواسطة معرف الفاتورة
        /// </summary>
        [HttpGet("journal-lines/by-invoice/{invoiceId}")]
        public async Task<IActionResult> GetTaxJournalLinesByInvoice(Guid invoiceId)
        {
            var result = await _taxPostingService.GetTaxJournalLinesByInvoiceAsync(invoiceId);
            return SUCCESS(result);
        }

        #endregion

        #region Posting Operations

        /// <summary>
        /// Create and save tax journal lines from calculation
        /// إنشاء وحفظ سطور دفتر الضرائب من الحساب
        /// </summary>
        [HttpPost("create-journal-lines")]
        [Log(Title = "Tax Journal Lines", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateTaxJournalLines(
            [FromBody] TaxCalculationResultDto calculationResult,
            [FromQuery] long companyId,
            [FromQuery] Guid? journalId = null,
            [FromQuery] Guid? invoiceId = null,
            [FromQuery] string sourceModule = null,
            [FromQuery] string currency = null,
            [FromQuery] decimal exchangeRate = 1)
        {
            var userName = HttpContext.User.Identity?.Name ?? "System";

            var context = new TaxPostingContext
            {
                CompanyId = companyId,
                JournalId = journalId,
                InvoiceId = invoiceId,
                SourceModule = sourceModule,
                TaxDate = calculationResult.Input?.Date ?? DateTime.Today,
                Currency = currency ?? calculationResult.Input?.Currency,
                ExchangeRate = exchangeRate,
                CreatedBy = userName
            };

            var lines = await _taxPostingService.CreateTaxJournalLinesAsync(calculationResult, context);
            var count = await _taxPostingService.SaveTaxJournalLinesAsync(lines);

            return SUCCESS(new
            {
                LinesCreated = count,
                JournalId = journalId,
                InvoiceId = invoiceId
            });
        }

        /// <summary>
        /// Post tax journal lines (mark as immutable)
        /// ترحيل سطور دفتر الضرائب (تعليمها كغير قابلة للتعديل)
        /// </summary>
        [HttpPost("post")]
        [Log(Title = "Tax Journal Lines", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> PostTaxJournalLines([FromBody] Guid[] ids)
        {
            var userName = HttpContext.User.Identity?.Name ?? "System";
            var count = await _taxPostingService.PostTaxJournalLinesAsync(ids, userName);
            return ToResponse(count);
        }

        /// <summary>
        /// Post tax journal lines by journal ID
        /// ترحيل سطور دفتر الضرائب بواسطة معرف القيد
        /// </summary>
        [HttpPost("post/by-journal/{journalId}")]
        [Log(Title = "Tax Journal Lines", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> PostTaxJournalLinesByJournal(Guid journalId)
        {
            var userName = HttpContext.User.Identity?.Name ?? "System";
            var count = await _taxPostingService.PostTaxJournalLinesByJournalAsync(journalId, userName);
            return ToResponse(count);
        }

        #endregion
    }
}
