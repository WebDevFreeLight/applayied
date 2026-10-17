using Microsoft.AspNetCore.Mvc;
using ZR.Model.Finance.ZATCA;
using ZR.Service.IService.Finance.ZATCA;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Finance.ZATCA
{
    /// <summary>
    /// ZATCA Controller - متحكم هيئة الزكاة والضريبة والجمارك
    /// </summary>
    [Verify]
    [Route("api/erp/zatca")]
    [ApiController]
    public class ErpZatcaController : BaseController
    {
        private readonly IErpZatcaInvoiceService _zatcaInvoiceService;
        private readonly IErpZatcaSettingsService _zatcaSettingsService;

        public ErpZatcaController(IErpZatcaInvoiceService zatcaInvoiceService, IErpZatcaSettingsService zatcaSettingsService)
        {
            _zatcaInvoiceService = zatcaInvoiceService;
            _zatcaSettingsService = zatcaSettingsService;
        }

        #region Settings - الإعدادات

        [HttpGet("settings")]
        [ActionPermissionFilter(Permission = "erp:zatca:settings")]
        public IActionResult GetSettings([FromQuery] long? BranchID)
        {
            var response = _zatcaSettingsService.GetSettings(BranchID);
            return SUCCESS(response);
        }

        [HttpPost("settings")]
        [ActionPermissionFilter(Permission = "erp:zatca:settings")]
        [Log(Title = "ZATCA Settings", BusinessType = BusinessType.UPDATE)]
        public IActionResult SaveSettings([FromBody] ErpZatcaSettings settings)
        {
            var result = _zatcaSettingsService.SaveSettings(settings);
            return ToResponse(result);
        }

        [HttpPost("settings/test-connection")]
        [ActionPermissionFilter(Permission = "erp:zatca:settings")]
        public async Task<IActionResult> TestConnection()
        {
            var result = await _zatcaSettingsService.TestConnectionAsync();
            return SUCCESS(new { Success = result });
        }

        [HttpPost("settings/validate-certificate")]
        [ActionPermissionFilter(Permission = "erp:zatca:certificate")]
        public async Task<IActionResult> ValidateCertificate([FromBody] CertificatePathRequest request)
        {
            var result = await _zatcaSettingsService.ValidateCertificateAsync(request.CertificatePath);
            return SUCCESS(new { IsValid = result });
        }

        #endregion

        #region Invoices - الفواتير

        [HttpGet("invoices")]
        [ActionPermissionFilter(Permission = "erp:zatca:list")]
        public IActionResult GetZatcaInvoices([FromQuery] ErpZatcaInvoice parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _zatcaInvoiceService.GetList(parm, pager);
            return SUCCESS(response);
        }

        [HttpGet("invoices/{id}")]
        [ActionPermissionFilter(Permission = "erp:zatca:query")]
        public IActionResult GetZatcaInvoice(long id)
        {
            var response = _zatcaInvoiceService.GetInfo(id);
            if (response == null) return ToResponse(ResultCode.FAIL, "ZATCA invoice not found - فاتورة الزكاة غير موجودة");
            return SUCCESS(response);
        }

        [HttpPost("invoices/{invoiceId}/create")]
        [ActionPermissionFilter(Permission = "erp:zatca:create")]
        [Log(Title = "Create ZATCA Invoice", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateZatcaInvoice(long invoiceId)
        {
            var result = _zatcaInvoiceService.CreateZatcaInvoice(invoiceId);
            return SUCCESS(result);
        }

        [HttpPost("invoices/{zatcaInvoiceId}/submit")]
        [ActionPermissionFilter(Permission = "erp:zatca:submit")]
        [Log(Title = "Submit to ZATCA", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> SubmitInvoice(long zatcaInvoiceId)
        {
            var result = await _zatcaInvoiceService.SubmitToZatcaAsync(zatcaInvoiceId);
            return SUCCESS(new { Success = result });
        }

        [HttpPost("invoices/{zatcaInvoiceId}/check-status")]
        [ActionPermissionFilter(Permission = "erp:zatca:query")]
        public async Task<IActionResult> CheckStatus(long zatcaInvoiceId)
        {
            var result = await _zatcaInvoiceService.CheckStatusAsync(zatcaInvoiceId);
            return SUCCESS(new { Success = result });
        }

        [HttpGet("invoices/{invoiceId}/xml")]
        [ActionPermissionFilter(Permission = "erp:zatca:query")]
        public IActionResult GetInvoiceXml(long invoiceId)
        {
            var xml = _zatcaInvoiceService.GenerateInvoiceXml(invoiceId);
            return SUCCESS(new { Xml = xml });
        }

        [HttpPost("invoices/sign")]
        [ActionPermissionFilter(Permission = "erp:zatca:sign")]
        public IActionResult SignInvoice([FromBody] SignInvoiceRequest request)
        {
            var signedXml = _zatcaInvoiceService.SignInvoice(request.InvoiceXml);
            return SUCCESS(new { SignedXml = signedXml });
        }

        #endregion

        #region Reports - التقارير

        [HttpGet("reports/pending")]
        [ActionPermissionFilter(Permission = "erp:zatca:report")]
        public IActionResult GetPendingInvoices()
        {
            var result = _zatcaInvoiceService.GetPendingInvoices();
            return SUCCESS(result);
        }

        [HttpGet("reports/failed")]
        [ActionPermissionFilter(Permission = "erp:zatca:report")]
        public IActionResult GetFailedInvoices()
        {
            var result = _zatcaInvoiceService.GetFailedInvoices();
            return SUCCESS(result);
        }

        [HttpPost("invoices/retry-failed")]
        [ActionPermissionFilter(Permission = "erp:zatca:submit")]
        [Log(Title = "Retry Failed ZATCA Submissions", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> RetryFailedSubmissions()
        {
            var failed = _zatcaInvoiceService.GetFailedInvoices();
            int successCount = 0;
            foreach (var invoice in failed)
            {
                var result = await _zatcaInvoiceService.SubmitToZatcaAsync(invoice.ZatcaInvoiceId);
                if (result) successCount++;
            }
            return SUCCESS(new { TotalFailed = failed.Count, RetrySuccessCount = successCount });
        }

        #endregion

        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:zatca:export")]
        [Log(Title = "ZATCA Invoice", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpZatcaInvoice parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _zatcaInvoiceService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "ZATCAInvoices", "ZATCA Invoices - فواتير هيئة الزكاة");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class CertificatePathRequest { public string CertificatePath { get; set; } }
    public class SignInvoiceRequest { public string InvoiceXml { get; set; } }
}

