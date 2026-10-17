using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Billing Engine Controller - محرك الفوترة
    /// Handles invoice generation, pricing calculations, and billing operations
    /// </summary>
    [Authorize]
    [Route("erp/contracts/billing")]
    [ApiController]
    public class ErpContractBillingEngineController : BaseController
    {
        private readonly IBillingEngine _billingEngine;
        private readonly IPricingRuleEngine _pricingEngine;

        public ErpContractBillingEngineController(
            IBillingEngine billingEngine,
            IPricingRuleEngine pricingEngine)
        {
            _billingEngine = billingEngine;
            _pricingEngine = pricingEngine;
        }

        #region Invoice Generation Endpoints

        /// <summary>
        /// Generate invoices for due billing schedules
        /// إنشاء فواتير للجداول المستحقة
        /// </summary>
        [HttpPost("generate-invoices")]
        [Log(Title = "Contract Billing", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateInvoices([FromBody] InvoiceGenerationRequest request)
        {
            try
            {
                request.ProcessedBy = HttpContext.GetName();
                var result = await _billingEngine.GenerateInvoicesAsync(request);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Generate invoice for a specific billing schedule
        /// إنشاء فاتورة لجدول فوترة محدد
        /// </summary>
        [HttpPost("generate-invoice/{scheduleId}")]
        [Log(Title = "Contract Billing", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateInvoiceForSchedule(Guid scheduleId)
        {
            try
            {
                var result = await _billingEngine.GenerateInvoiceForScheduleAsync(scheduleId, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Preview invoice before generation
        /// معاينة الفاتورة قبل الإنشاء
        /// </summary>
        [HttpGet("preview-invoice/{scheduleId}")]
        public async Task<IActionResult> PreviewInvoice(Guid scheduleId)
        {
            try
            {
                var result = await _billingEngine.PreviewInvoiceAsync(scheduleId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get pending invoices for generation
        /// الحصول على الفواتير المعلقة للإنشاء
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingInvoices([FromQuery] DateTime? asOfDate = null, [FromQuery] Guid? contractId = null)
        {
            try
            {
                var result = await _billingEngine.GetPendingInvoicesAsync(asOfDate, contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Batch generate invoices for all due schedules
        /// إنشاء فواتير دفعة واحدة لكل الجداول المستحقة
        /// </summary>
        [HttpPost("batch-generate")]
        [Log(Title = "Contract Billing", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> BatchGenerateInvoices([FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var result = await _billingEngine.BatchGenerateInvoicesAsync(asOfDate ?? DateTime.Today, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Pricing Calculation Endpoints

        /// <summary>
        /// Calculate price with all adjustments
        /// حساب السعر مع جميع التعديلات
        /// </summary>
        [HttpPost("calculate-price")]
        public async Task<IActionResult> CalculatePrice([FromBody] PricingCalculationRequest request)
        {
            try
            {
                var result = await _pricingEngine.CalculatePriceAsync(request);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Preview price with all adjustments
        /// معاينة السعر مع جميع التعديلات
        /// </summary>
        [HttpPost("preview-price")]
        public async Task<IActionResult> PreviewPrice([FromBody] PricingCalculationRequest request)
        {
            try
            {
                var result = await _pricingEngine.PreviewPriceAsync(request);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply escalation to amount
        /// تطبيق التصعيد على المبلغ
        /// </summary>
        [HttpGet("escalation/{contractId}")]
        public async Task<IActionResult> ApplyEscalation(Guid contractId, [FromQuery] decimal baseAmount, [FromQuery] DateTime effectiveDate)
        {
            try
            {
                var result = await _pricingEngine.ApplyEscalationAsync(contractId, baseAmount, effectiveDate);
                return SUCCESS(new { BaseAmount = baseAmount, AdjustedAmount = result, EffectiveDate = effectiveDate });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply seasonal adjustment to amount
        /// تطبيق التعديل الموسمي على المبلغ
        /// </summary>
        [HttpGet("seasonal/{contractId}")]
        public async Task<IActionResult> ApplySeasonalAdjustment(Guid contractId, [FromQuery] decimal amount, [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            try
            {
                var result = await _pricingEngine.ApplySeasonalAdjustmentAsync(contractId, amount, periodStart, periodEnd);
                return SUCCESS(new { OriginalAmount = amount, AdjustedAmount = result, PeriodStart = periodStart, PeriodEnd = periodEnd });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get active escalation rules for contract
        /// الحصول على قواعد التصعيد النشطة للعقد
        /// </summary>
        [HttpGet("escalations/{contractId}")]
        public async Task<IActionResult> GetActiveEscalations(Guid contractId, [FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                var result = await _pricingEngine.GetActiveEscalationsAsync(contractId, asOfDate ?? DateTime.Today);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get active seasonal rules for contract
        /// الحصول على القواعد الموسمية النشطة للعقد
        /// </summary>
        [HttpGet("seasonal-rules/{contractId}")]
        public async Task<IActionResult> GetActiveSeasonalRules(Guid contractId, [FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            try
            {
                var result = await _pricingEngine.GetActiveSeasonalRulesAsync(contractId, periodStart, periodEnd);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get cumulative escalation percentage
        /// الحصول على نسبة التصعيد التراكمية
        /// </summary>
        [HttpGet("cumulative-escalation/{contractId}")]
        public async Task<IActionResult> GetCumulativeEscalation(Guid contractId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var result = await _pricingEngine.GetCumulativeEscalationPercentageAsync(contractId, fromDate, toDate);
                return SUCCESS(new { ContractId = contractId, FromDate = fromDate, ToDate = toDate, CumulativePercentage = result });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Billing Analytics Endpoints

        /// <summary>
        /// Get billing summary for a contract
        /// الحصول على ملخص الفوترة للعقد
        /// </summary>
        [HttpGet("summary/{contractId}")]
        public async Task<IActionResult> GetBillingSummary(Guid contractId)
        {
            try
            {
                var result = await _billingEngine.GetBillingSummaryAsync(contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get billing timeline for a contract
        /// الحصول على الجدول الزمني للفوترة
        /// </summary>
        [HttpGet("timeline/{contractId}")]
        public async Task<IActionResult> GetBillingTimeline(Guid contractId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _billingEngine.GetBillingTimelineAsync(contractId, fromDate, toDate);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get overdue invoices
        /// الحصول على الفواتير المتأخرة
        /// </summary>
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueInvoices([FromQuery] int minDaysOverdue = 1, [FromQuery] Guid? customerId = null)
        {
            try
            {
                var result = await _billingEngine.GetOverdueInvoicesAsync(minDaysOverdue, customerId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get revenue forecast
        /// الحصول على توقعات الإيرادات
        /// </summary>
        [HttpGet("forecast")]
        public async Task<IActionResult> GetRevenueForecast([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] Guid? contractId = null)
        {
            try
            {
                var result = await _billingEngine.GetRevenueForecastAsync(fromDate, toDate, contractId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion
    }
}
