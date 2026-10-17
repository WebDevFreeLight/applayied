using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Pdf.Core;
using ZR.Pdf.Mvc;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;

namespace ZR.Admin.WebApi.Controllers.Finance.ARAP
{
    /// <summary>
    /// ARAP PDF Export Controller - تصدير PDF للفواتير
    /// Exports AR/AP Invoices to PDF format (e-Fatoora / ZATCA compliant)
    /// </summary>
    [Route("erp/finance/arap/pdf")]
    public class ARAPPdfExportController : BaseController
    {
        private readonly IPdfRenderer _pdfRenderer;
        private readonly IARInvoiceService _arInvoiceService;
        private readonly IAPInvoiceService _apInvoiceService;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IRazorViewRender _razorViewRender;

        public ARAPPdfExportController(
            IPdfRenderer pdfRenderer,
            IARInvoiceService arInvoiceService,
            IAPInvoiceService apInvoiceService,
            ICustomerService customerService,
            IVendorService vendorService,
            IRazorViewRender razorViewRender)
        {
            _pdfRenderer = pdfRenderer;
            _arInvoiceService = arInvoiceService;
            _apInvoiceService = apInvoiceService;
            _customerService = customerService;
            _vendorService = vendorService;
            _razorViewRender = razorViewRender;
        }

        /// <summary>
        /// Export AR Invoice to PDF - تصدير فاتورة العميل إلى PDF
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="lang">Language: "en" for English, "ar" for Arabic (default: en)</param>
        [HttpGet("ar-invoice/{invoiceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportARInvoicePdf(Guid invoiceId, [FromQuery] string lang = "en")
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportARInvoicePdf - Starting export for invoiceId: {invoiceId}");

                var invoice = _arInvoiceService.GetById(invoiceId);
                if (invoice == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportARInvoicePdf - Invoice not found for invoiceId: {invoiceId}");
                    return ToResponse(ResultCode.FAIL, "Invoice not found - الفاتورة غير موجودة");
                }

                // Get customer details (GetById returns CustomerDetailDto)
                var customer = _customerService.GetById(invoice.CustomerId);

                // Create view model
                var viewModel = new ARInvoiceExportViewModel
                {
                    // Invoice Details
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,

                    // Amounts
                    Subtotal = invoice.Subtotal,
                    TaxAmount = invoice.TaxAmount,
                    TotalAmount = invoice.TotalAmount,
                    PaidAmount = invoice.PaidAmount,
                    RemainingAmount = invoice.RemainingAmount,

                    // Status
                    Status = invoice.Status,
                    StatusName = GetInvoiceStatusName(invoice.Status),
                    StatusNameAr = GetInvoiceStatusNameAr(invoice.Status),

                    // Reference
                    Description = invoice.Description,
                    ReferenceNumber = invoice.ReferenceNumber,
                    Notes = invoice.Notes,

                    // Customer Details (from invoice or customer DTO)
                    CustomerCode = invoice.CustomerCode ?? customer?.CustomerCode,
                    CustomerName = invoice.CustomerName ?? customer?.NameEn,
                    CustomerNameAr = customer?.NameAr,
                    CustomerAddress = customer?.AddressEn,
                    CustomerAddressAr = customer?.AddressAr,
                    CustomerTaxNumber = customer?.TaxNumber,
                    CustomerEmail = customer?.Email,
                    CustomerPhone = customer?.Phone,

                    // Invoice Lines
                    Lines = invoice.Lines?.Select(l => new InvoiceLineExportViewModel
                    {
                        LineNumber = l.LineNumber,
                        Description = l.Description,
                        AccountCode = l.AccountCode,
                        AccountName = l.AccountName,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        Amount = l.Amount,
                        TaxRate = l.TaxRate,
                        TaxAmount = l.TaxAmount,
                        TotalAmount = l.TotalAmount
                    }).ToList() ?? new List<InvoiceLineExportViewModel>(),

                    // ZATCA Compliance
                    InvoiceUUID = invoice.Id.ToString().ToUpper(),
                    InvoiceHash = GenerateInvoiceHash(invoice.InvoiceNumber, invoice.TotalAmount),
                    QRCodeData = GenerateQRCodeData(invoice.InvoiceNumber, invoice.TotalAmount, invoice.TaxAmount, invoice.InvoiceDate)
                };

                // Select template based on language
                var isArabic = lang?.ToLower() == "ar";
                var templateName = isArabic ? "ARInvoice_AR" : "ARInvoice_EN";
                var templatePath = isArabic ? "/Views/ARAPPdfExport/ARInvoice_AR.cshtml" : "/Views/ARAPPdfExport/ARInvoice_EN.cshtml";

                var html = await _razorViewRender.RenderViewToStringAsync(templateName, viewModel, new()
                {
                    templatePath
                });

                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "15px", Bottom = "15px", Left = "15px", Right = "15px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportARInvoicePdf - Successfully generated PDF for invoiceId: {invoiceId}");

                return File(pdfData, "application/pdf", $"ARInvoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportARInvoicePdf - invoiceId: {invoiceId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate AR invoice PDF", message = ex.Message });
            }
        }

        /// <summary>
        /// Export AP Invoice to PDF - تصدير فاتورة المورد إلى PDF
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="lang">Language: "en" for English, "ar" for Arabic (default: en)</param>
        [HttpGet("ap-invoice/{invoiceId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportAPInvoicePdf(Guid invoiceId, [FromQuery] string lang = "en")
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportAPInvoicePdf - Starting export for invoiceId: {invoiceId}");

                var invoice = _apInvoiceService.GetById(invoiceId);
                if (invoice == null)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportAPInvoicePdf - Invoice not found for invoiceId: {invoiceId}");
                    return ToResponse(ResultCode.FAIL, "Invoice not found - الفاتورة غير موجودة");
                }

                // Get vendor details (GetById returns VendorDetailDto)
                var vendor = _vendorService.GetById(invoice.VendorId);

                // Create view model
                var viewModel = new APInvoiceExportViewModel
                {
                    // Invoice Details
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    Currency = invoice.Currency ?? "SAR",
                    ExchangeRate = invoice.ExchangeRate,

                    // Amounts
                    Subtotal = invoice.Subtotal,
                    TaxAmount = invoice.TaxAmount,
                    TotalAmount = invoice.TotalAmount,
                    PaidAmount = invoice.PaidAmount,
                    RemainingAmount = invoice.RemainingAmount,

                    // Status
                    Status = invoice.Status,
                    StatusName = GetInvoiceStatusName(invoice.Status),
                    StatusNameAr = GetInvoiceStatusNameAr(invoice.Status),

                    // Reference
                    Description = invoice.Description,
                    ReferenceNumber = invoice.ReferenceNumber,
                    Notes = invoice.Notes,

                    // Vendor Details (from invoice or vendor DTO)
                    VendorCode = invoice.VendorCode ?? vendor?.VendorCode,
                    VendorName = invoice.VendorName ?? vendor?.NameEn,
                    VendorNameAr = vendor?.NameAr,
                    VendorAddress = vendor?.AddressEn,
                    VendorAddressAr = vendor?.AddressAr,
                    VendorTaxNumber = vendor?.TaxNumber,
                    VendorEmail = vendor?.Email,
                    VendorPhone = vendor?.Phone,

                    // Invoice Lines
                    Lines = invoice.Lines?.Select(l => new InvoiceLineExportViewModel
                    {
                        LineNumber = l.LineNumber,
                        Description = l.Description,
                        AccountCode = l.AccountCode,
                        AccountName = l.AccountName,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        Amount = l.Amount,
                        TaxRate = l.TaxRate,
                        TaxAmount = l.TaxAmount,
                        TotalAmount = l.TotalAmount
                    }).ToList() ?? new List<InvoiceLineExportViewModel>()
                };

                // Select template based on language
                var isArabic = lang?.ToLower() == "ar";
                var templateName = isArabic ? "APInvoice_AR" : "APInvoice_EN";
                var templatePath = isArabic ? "/Views/ARAPPdfExport/APInvoice_AR.cshtml" : "/Views/ARAPPdfExport/APInvoice_EN.cshtml";

                var html = await _razorViewRender.RenderViewToStringAsync(templateName, viewModel, new()
                {
                    templatePath
                });

                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions { Top = "15px", Bottom = "15px", Left = "15px", Right = "15px" }
                };

                var pdfData = await _pdfRenderer.GetPdfByHtmlAsync(html, pdfOptions);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ExportAPInvoicePdf - Successfully generated PDF for invoiceId: {invoiceId}");

                return File(pdfData, "application/pdf", $"APInvoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in ExportAPInvoicePdf - invoiceId: {invoiceId}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = "Failed to generate AP invoice PDF", message = ex.Message });
            }
        }

        #region Helper Methods

        private string GetInvoiceStatusName(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "Draft",
                InvoiceStatus.Posted => "Posted",
                InvoiceStatus.PartiallyPaid => "Partially Paid",
                InvoiceStatus.Paid => "Paid",
                InvoiceStatus.Overdue => "Overdue",
                InvoiceStatus.Cancelled => "Cancelled",
                _ => status.ToString()
            };
        }

        private string GetInvoiceStatusNameAr(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "مسودة",
                InvoiceStatus.Posted => "مرحّل",
                InvoiceStatus.PartiallyPaid => "مدفوع جزئياً",
                InvoiceStatus.Paid => "مدفوع",
                InvoiceStatus.Overdue => "متأخر",
                InvoiceStatus.Cancelled => "ملغي",
                _ => status.ToString()
            };
        }

        private string GenerateInvoiceHash(string invoiceNumber, decimal totalAmount)
        {
            var data = $"{invoiceNumber}|{totalAmount}|{DateTime.Now:yyyyMMdd}";
            using var sha256 = global::System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(global::System.Text.Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(bytes).Substring(0, 32);
        }

        private string GenerateQRCodeData(string invoiceNumber, decimal totalAmount, decimal taxAmount, DateTime invoiceDate)
        {
            // Simplified QR code data - in production, use proper ZATCA TLV encoding
            return $"INV:{invoiceNumber}|DATE:{invoiceDate:yyyy-MM-dd}|TOTAL:{totalAmount:F2}|TAX:{taxAmount:F2}";
        }

        #endregion
    }

    #region View Models

    /// <summary>
    /// View Model for AR Invoice PDF Export
    /// </summary>
    public class ARInvoiceExportViewModel
    {
        // Invoice Details
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }

        // Amounts
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        // Status
        public InvoiceStatus Status { get; set; }
        public string StatusName { get; set; }
        public string StatusNameAr { get; set; }

        // Reference
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }

        // Customer Details
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameAr { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerAddressAr { get; set; }
        public string CustomerTaxNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        // Invoice Lines
        public List<InvoiceLineExportViewModel> Lines { get; set; }

        // ZATCA Compliance
        public string InvoiceUUID { get; set; }
        public string InvoiceHash { get; set; }
        public string QRCodeData { get; set; }
    }

    /// <summary>
    /// View Model for AP Invoice PDF Export
    /// </summary>
    public class APInvoiceExportViewModel
    {
        // Invoice Details
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }

        // Amounts
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        // Status
        public InvoiceStatus Status { get; set; }
        public string StatusName { get; set; }
        public string StatusNameAr { get; set; }

        // Reference
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }

        // Vendor Details
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string VendorNameAr { get; set; }
        public string VendorAddress { get; set; }
        public string VendorAddressAr { get; set; }
        public string VendorTaxNumber { get; set; }
        public string VendorEmail { get; set; }
        public string VendorPhone { get; set; }

        // Invoice Lines
        public List<InvoiceLineExportViewModel> Lines { get; set; }
    }

    /// <summary>
    /// View Model for Invoice Line in PDF Export
    /// </summary>
    public class InvoiceLineExportViewModel
    {
        public int LineNumber { get; set; }
        public string Description { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    #endregion
}
