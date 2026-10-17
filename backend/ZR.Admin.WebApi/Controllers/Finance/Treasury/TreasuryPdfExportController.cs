using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ZR.Pdf.Core;
using ZR.Pdf.Mvc;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Treasury PDF Export Controller - تصدير PDF للخزينة
    /// </summary>
    [Route("erp/finance/treasury/pdf")]
    public class TreasuryPdfExportController : BaseController
    {
        private readonly IPdfRenderer _pdfRenderer;
        private readonly IChequeService _chequeService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IRazorViewRender _razorViewRender;

        public TreasuryPdfExportController(
            IPdfRenderer pdfRenderer,
            IChequeService chequeService,
            IBankAccountService bankAccountService,
            IRazorViewRender razorViewRender)
        {
            _pdfRenderer = pdfRenderer;
            _chequeService = chequeService;
            _bankAccountService = bankAccountService;
            _razorViewRender = razorViewRender;
        }

        /// <summary>
        /// Export Cheque to PDF - تصدير شيك إلى PDF
        /// </summary>
        [HttpGet("cheque/{chequeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportChequePdf(Guid chequeId)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportChequePdf - Starting export for chequeId: {chequeId}");

                var cheque = _chequeService.GetDetail(chequeId);
                if (cheque == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportChequePdf - Cheque not found for chequeId: {chequeId}");
                    return ToResponse(ResultCode.FAIL, "Cheque not found - الشيك غير موجود");
                }

                // Get bank account details
                string bankAccountName = "";
                try
                {
                    var bankAccount = _bankAccountService.GetDetail(cheque.BankAccountId);
                    bankAccountName = bankAccount?.BankNameEn ?? bankAccount?.BankNameAr ?? bankAccount?.AccountNumber ?? "";
                }
                catch
                {
                    bankAccountName = cheque.BankAccountId.ToString();
                }

                // Create view model
                var viewModel = new ChequeExportViewModel
                {
                    ChequeNumber = cheque.ChequeNumber,
                    ChequeType = cheque.ChequeType,
                    ChequeTypeName = cheque.ChequeType == ChequeType.Outgoing ? "Outgoing - صادر" : "Incoming - وارد",
                    BankAccountName = bankAccountName,
                    PayeeName = cheque.PayeeName,
                    DrawerName = cheque.DrawerName,
                    DrawerBankName = cheque.DrawerBankName,
                    Amount = cheque.Amount,
                    Currency = cheque.Currency ?? "SAR",
                    IssueDate = cheque.IssueDate,
                    DueDate = cheque.DueDate,
                    ClearedDate = cheque.ClearedDate,
                    Status = cheque.Status,
                    StatusName = GetStatusName(cheque.Status),
                    StatusNameAr = GetStatusNameAr(cheque.Status),
                    ReferenceNo = cheque.ReferenceNo,
                    Memo = cheque.Memo,
                    BounceReason = cheque.BounceReason,
                    CancellationReason = cheque.CancellationReason,
                    Notes = cheque.Notes
                };

                var html = await _razorViewRender.RenderViewToStringAsync("Cheque", viewModel, new()
                {
                    "/Views/TreasuryPdfExport/Cheque.cshtml"
                });

                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "20px", Bottom = "20px", Left = "20px", Right = "20px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportChequePdf - Successfully generated PDF for chequeId: {chequeId}");

                return File(pdfData, "application/pdf", $"Cheque_{cheque.ChequeNumber}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportChequePdf - chequeId: {chequeId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate cheque PDF", message = ex.Message });
            }
        }

        private string GetStatusName(ChequeStatus status)
        {
            return status switch
            {
                ChequeStatus.Issued => "Issued",
                ChequeStatus.Presented => "Presented",
                ChequeStatus.Cleared => "Cleared",
                ChequeStatus.Bounced => "Bounced",
                ChequeStatus.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }

        private string GetStatusNameAr(ChequeStatus status)
        {
            return status switch
            {
                ChequeStatus.Issued => "صادر",
                ChequeStatus.Presented => "مقدم",
                ChequeStatus.Cleared => "مقاص",
                ChequeStatus.Bounced => "مرتجع",
                ChequeStatus.Cancelled => "ملغي",
                _ => status.ToString()
            };
        }
    }

    /// <summary>
    /// View Model for Cheque PDF Export
    /// </summary>
    public class ChequeExportViewModel
    {
        public string ChequeNumber { get; set; }
        public ChequeType ChequeType { get; set; }
        public string ChequeTypeName { get; set; }
        public string BankAccountName { get; set; }
        public string PayeeName { get; set; }
        public string DrawerName { get; set; }
        public string DrawerBankName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public ChequeStatus Status { get; set; }
        public string StatusName { get; set; }
        public string StatusNameAr { get; set; }
        public string ReferenceNo { get; set; }
        public string Memo { get; set; }
        public string BounceReason { get; set; }
        public string CancellationReason { get; set; }
        public string Notes { get; set; }
    }
}
