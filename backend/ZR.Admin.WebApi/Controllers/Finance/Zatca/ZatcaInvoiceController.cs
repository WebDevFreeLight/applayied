using Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Admin.WebApi.Controllers.Finance.Zatca
{
    /// <summary>
    /// ZATCA Invoice Controller
    /// تحكم فواتير زاتكا
    /// </summary>
    [Verify]
    [Route("api/erp/finance/zatca/invoices")]
    [ApiController]
    [Authorize]
    public class ZatcaInvoiceController : BaseController
    {
        private readonly IZatcaInvoiceService _invoiceService;

        public ZatcaInvoiceController(IZatcaInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        #region Submission Endpoints

        /// <summary>
        /// Submit invoice to ZATCA
        /// إرسال فاتورة لزاتكا
        /// </summary>
        [HttpPost("submit")]
        [Log(Title = "ZATCA Submit Invoice", BusinessType = BusinessType.INSERT)]
        public IActionResult SubmitInvoice([FromBody] ZatcaSubmitInvoiceRequestDto request)
        {
            var result = _invoiceService.SubmitInvoice(request, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Submit multiple invoices to ZATCA
        /// إرسال مجموعة فواتير لزاتكا
        /// </summary>
        [HttpPost("submit-bulk")]
        [Log(Title = "ZATCA Bulk Submit", BusinessType = BusinessType.INSERT)]
        public IActionResult SubmitBulkInvoices([FromBody] ZatcaBulkSubmitRequestDto request)
        {
            var result = _invoiceService.SubmitBulkInvoices(request, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Retry failed invoice submission
        /// إعادة محاولة إرسال فاتورة فاشلة
        /// </summary>
        [HttpPost("{invoiceId}/retry")]
        [Log(Title = "ZATCA Retry Submit", BusinessType = BusinessType.UPDATE)]
        public IActionResult RetrySubmission(Guid invoiceId)
        {
            var result = _invoiceService.RetrySubmission(invoiceId, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        #endregion

        #region Status Endpoints

        /// <summary>
        /// Get ZATCA status for invoice
        /// الحصول على حالة زاتكا للفاتورة
        /// </summary>
        [HttpGet("{invoiceId}/status")]
        public IActionResult GetInvoiceStatus(Guid invoiceId)
        {
            var result = _invoiceService.GetInvoiceStatus(invoiceId);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Invoice not found - الفاتورة غير موجودة");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get pending invoices for ZATCA submission
        /// الحصول على الفواتير المعلقة
        /// </summary>
        [HttpGet("pending")]
        public IActionResult GetPendingInvoices([FromQuery] PendingZatcaInvoicesQueryDto query)
        {
            var result = _invoiceService.GetPendingInvoices(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get invoices needing retry
        /// الحصول على الفواتير التي تحتاج إعادة محاولة
        /// </summary>
        [HttpGet("needing-retry")]
        public IActionResult GetInvoicesNeedingRetry([FromQuery] int maxRetries = 3)
        {
            var result = _invoiceService.GetInvoicesNeedingRetry(maxRetries);
            return SUCCESS(result);
        }

        #endregion

        #region QR Code Endpoints

        /// <summary>
        /// Get QR code for invoice
        /// الحصول على رمز QR للفاتورة
        /// </summary>
        [HttpGet("{invoiceId}/qr-code")]
        public IActionResult GetQrCode(Guid invoiceId)
        {
            var result = _invoiceService.GetQrCode(invoiceId);
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate QR code for invoice (without submission)
        /// إنشاء رمز QR للفاتورة
        /// </summary>
        [HttpPost("{invoiceId}/generate-qr")]
        [Log(Title = "ZATCA Generate QR", BusinessType = BusinessType.UPDATE)]
        public IActionResult GenerateQrCode(Guid invoiceId)
        {
            var result = _invoiceService.GenerateQrCode(invoiceId, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        #endregion

        #region XML Endpoints

        /// <summary>
        /// Generate invoice XML (without signing)
        /// إنشاء XML للفاتورة
        /// </summary>
        [HttpGet("{invoiceId}/xml")]
        public IActionResult GenerateInvoiceXml(Guid invoiceId)
        {
            var result = _invoiceService.GenerateInvoiceXml(invoiceId);
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate signed invoice XML
        /// إنشاء XML موقع للفاتورة
        /// </summary>
        [HttpGet("{invoiceId}/signed-xml")]
        public IActionResult GenerateSignedInvoiceXml(Guid invoiceId)
        {
            var result = _invoiceService.GenerateSignedInvoiceXml(invoiceId, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get signed XML for submitted invoice
        /// الحصول على XML الموقع للفاتورة المرسلة
        /// </summary>
        [HttpGet("{invoiceId}/submitted-xml")]
        public IActionResult GetSignedXml(Guid invoiceId)
        {
            var result = _invoiceService.GetSignedXml(invoiceId);
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        #endregion

        #region Hash Endpoints

        /// <summary>
        /// Get invoice hash list
        /// الحصول على قائمة هاش الفواتير
        /// </summary>
        [HttpGet("hashes")]
        public IActionResult GetInvoiceHashList([FromQuery] ZatcaInvoiceHashQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _invoiceService.GetInvoiceHashList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get submission logs
        /// الحصول على سجلات الإرسال
        /// </summary>
        [HttpGet("logs")]
        public IActionResult GetSubmissionLogs([FromQuery] ZatcaSubmissionLogQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _invoiceService.GetSubmissionLogs(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get submission log detail
        /// الحصول على تفاصيل سجل الإرسال
        /// </summary>
        [HttpGet("logs/{logId}")]
        public IActionResult GetSubmissionLogDetail(Guid logId)
        {
            var result = _invoiceService.GetSubmissionLogDetail(logId);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Log not found - السجل غير موجود");
            return SUCCESS(result);
        }

        #endregion

        #region Dashboard Endpoints

        /// <summary>
        /// Get dashboard statistics
        /// الحصول على إحصائيات لوحة التحكم
        /// </summary>
        [HttpGet("dashboard")]
        public IActionResult GetDashboardStats()
        {
            var result = _invoiceService.GetDashboardStats();
            return SUCCESS(result);
        }

        #endregion

        #region Validation Endpoints

        /// <summary>
        /// Validate invoice for ZATCA submission
        /// التحقق من صلاحية الفاتورة لإرسال زاتكا
        /// </summary>
        [HttpGet("{invoiceId}/validate")]
        public IActionResult ValidateInvoice(Guid invoiceId, [FromQuery] string invoiceSourceType = "AR")
        {
            var (isValid, errors, warnings) = _invoiceService.ValidateInvoiceForZatca(invoiceId, invoiceSourceType);
            return SUCCESS(new { IsValid = isValid, Errors = errors, Warnings = warnings });
        }

        /// <summary>
        /// Determine invoice type (B2B/B2C)
        /// تحديد نوع الفاتورة
        /// </summary>
        [HttpGet("{invoiceId}/determine-type")]
        public IActionResult DetermineInvoiceType(Guid invoiceId, [FromQuery] string invoiceSourceType = "AR")
        {
            var (type, subType) = _invoiceService.DetermineInvoiceType(invoiceId, invoiceSourceType);
            return SUCCESS(new { Type = type, SubType = subType });
        }

        #endregion
    }
}
