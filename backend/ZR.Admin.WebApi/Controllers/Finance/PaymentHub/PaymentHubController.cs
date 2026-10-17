using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Controllers;
using ZR.Model;
using ZR.Model.Finance.PaymentHub.Dto;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Service.IService.Finance.PaymentHub;

namespace ZR.Admin.WebApi.Controllers.Finance.PaymentHub
{
    /// <summary>
    /// Payment Hub API - واجهة برمجة تطبيقات مركز الدفعات
    /// THE CANONICAL single source of truth for ALL payment operations.
    /// All ARAP, Treasury, and other modules must use these endpoints.
    /// </summary>
    [Route("finance/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentHubController : BaseController
    {
        private readonly IPaymentHubService _paymentHubService;

        public PaymentHubController(IPaymentHubService paymentHubService)
        {
            _paymentHubService = paymentHubService;
        }

        #region Payment CRUD

        /// <summary>
        /// Create a new payment - إنشاء دفعة جديدة
        /// IDEMPOTENT: If IdempotencyKey already exists, returns existing payment
        /// </summary>
        /// <param name="dto">Payment creation request</param>
        /// <returns>Operation result with payment ID</returns>
        [HttpPost]
        public IActionResult CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.CreatePayment(dto, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(result.ErrorCode == "VALIDATION_FAILED" ? 400 : 500, result.ErrorMessage);
        }

        /// <summary>
        /// Get payment by ID - الحصول على دفعة بالمعرف
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Payment details</returns>
        [HttpGet("{id}")]
        public IActionResult GetPayment(Guid id)
        {
            var payment = _paymentHubService.GetPayment(id);
            if (payment == null)
            {
                return ToResponse(404, "Payment not found - الدفعة غير موجودة");
            }

            return SUCCESS(payment);
        }

        /// <summary>
        /// Get payment by payment number - الحصول على دفعة برقم الدفعة
        /// </summary>
        /// <param name="paymentNumber">Payment number</param>
        /// <returns>Payment details</returns>
        [HttpGet("by-number/{paymentNumber}")]
        public IActionResult GetPaymentByNumber(string paymentNumber)
        {
            var payment = _paymentHubService.GetPaymentByNumber(paymentNumber);
            if (payment == null)
            {
                return ToResponse(404, "Payment not found - الدفعة غير موجودة");
            }

            return SUCCESS(payment);
        }

        /// <summary>
        /// Get payments list with paging - الحصول على قائمة الدفعات مع ترقيم الصفحات
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <returns>Paged list of payments</returns>
        [HttpGet]
        public IActionResult GetPaymentList([FromQuery] PaymentQueryDto query)
        {
            var result = _paymentHubService.GetPaymentList(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete payment (Draft only) - حذف دفعة (مسودة فقط)
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Operation result</returns>
        [HttpDelete("{id}")]
        public IActionResult DeletePayment(Guid id)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.DeletePayment(id, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(result.ErrorCode == "NOT_FOUND" ? 404 : 400, result.ErrorMessage);
        }

        #endregion

        #region Payment Lifecycle

        /// <summary>
        /// Approve payment (Draft → Approved) - اعتماد دفعة
        /// </summary>
        /// <param name="dto">Approval request</param>
        /// <returns>Operation result</returns>
        [HttpPost("approve")]
        public IActionResult ApprovePayment([FromBody] ApprovePaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.ApprovePayment(dto, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        /// <summary>
        /// Post payment (Approved → Posted) - ترحيل دفعة
        /// ATOMIC: Single transaction for GL journal, balance ledger, account update
        /// IDEMPOTENT: Safe to retry - duplicate posts return success
        /// </summary>
        /// <param name="dto">Post request</param>
        /// <returns>Post result with balance details</returns>
        [HttpPost("post")]
        public IActionResult PostPayment([FromBody] PostPaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.PostPayment(dto, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        /// <summary>
        /// Reverse payment (Posted → Reversed) - عكس دفعة
        /// ATOMIC: Reverses all effects including allocations, GL, balances
        /// </summary>
        /// <param name="dto">Reversal request</param>
        /// <returns>Operation result</returns>
        [HttpPost("reverse")]
        public IActionResult ReversePayment([FromBody] ReversePaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.ReversePayment(dto, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        /// <summary>
        /// Cancel payment (Draft/Approved → Cancelled) - إلغاء دفعة
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="reason">Cancellation reason</param>
        /// <returns>Operation result</returns>
        [HttpPost("{id}/cancel")]
        public IActionResult CancelPayment(Guid id, [FromBody] CancelPaymentRequest request)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.CancelPayment(id, request.Reason, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        /// <summary>
        /// Validate payment can be posted - التحقق من إمكانية الترحيل
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Validation result</returns>
        [HttpGet("{id}/validate-post")]
        public IActionResult ValidateForPosting(Guid id)
        {
            var result = _paymentHubService.ValidateForPosting(id);
            return SUCCESS(result);
        }

        #endregion

        #region Allocation Operations

        /// <summary>
        /// Allocate payment to documents - تخصيص دفعة للمستندات
        /// ATOMIC: Single transaction for all allocations
        /// FX-Aware: Handles multi-currency with exchange rates
        /// </summary>
        /// <param name="dto">Allocation request</param>
        /// <returns>List of allocation results</returns>
        [HttpPost("allocate")]
        public IActionResult AllocatePayment([FromBody] AllocatePaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var results = _paymentHubService.AllocatePayment(dto, userId);
            return SUCCESS(results);
        }

        /// <summary>
        /// Reverse allocation - عكس تخصيص
        /// </summary>
        /// <param name="dto">Reversal request</param>
        /// <returns>Operation result</returns>
        [HttpPost("allocation/reverse")]
        public IActionResult ReverseAllocation([FromBody] ReverseAllocationDto dto)
        {
            var userId = HttpContext.GetUId().ToString();
            var result = _paymentHubService.ReverseAllocation(dto, userId);

            if (result.Success)
            {
                return SUCCESS(result);
            }

            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        /// <summary>
        /// Get allocations for payment - الحصول على تخصيصات الدفعة
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>List of allocations</returns>
        [HttpGet("{id}/allocations")]
        public IActionResult GetPaymentAllocations(Guid id)
        {
            var allocations = _paymentHubService.GetPaymentAllocations(id);
            return SUCCESS(allocations);
        }

        /// <summary>
        /// Get allocations for document - الحصول على تخصيصات المستند
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="documentType">Document type (ARInvoice, APInvoice, etc.)</param>
        /// <returns>List of allocations</returns>
        [HttpGet("document/{documentId}/allocations")]
        public IActionResult GetDocumentAllocations(Guid documentId, [FromQuery] AllocatableDocumentType documentType)
        {
            var allocations = _paymentHubService.GetDocumentAllocations(documentId, documentType);
            return SUCCESS(allocations);
        }

        /// <summary>
        /// Validate allocation - التحقق من التخصيص
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <param name="dto">Allocation details to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("{paymentId}/validate-allocation")]
        public IActionResult ValidateAllocation(Guid paymentId, [FromBody] CreateAllocationDto dto)
        {
            var result = _paymentHubService.ValidateAllocation(paymentId, dto);
            return SUCCESS(result);
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get payments with unallocated balance - الدفعات غير المخصصة
        /// </summary>
        /// <param name="counterpartyId">Optional counterparty filter</param>
        /// <param name="paymentType">Optional payment type filter</param>
        /// <returns>List of unallocated payments</returns>
        [HttpGet("unallocated")]
        public IActionResult GetUnallocatedPayments([FromQuery] Guid? counterpartyId, [FromQuery] PaymentType? paymentType)
        {
            var payments = _paymentHubService.GetUnallocatedPayments(counterpartyId, paymentType);
            return SUCCESS(payments);
        }

        /// <summary>
        /// Get payment audit trail (events) - الحصول على سجل أحداث الدفعة
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>List of events</returns>
        [HttpGet("{id}/events")]
        public IActionResult GetPaymentEvents(Guid id)
        {
            var events = _paymentHubService.GetPaymentEvents(id);
            return SUCCESS(events);
        }

        /// <summary>
        /// Get payments by account - الحصول على دفعات الحساب
        /// </summary>
        /// <param name="accountId">Account ID (bank or cash)</param>
        /// <param name="fromDate">Start date filter</param>
        /// <param name="toDate">End date filter</param>
        /// <returns>List of payments</returns>
        [HttpGet("by-account/{accountId}")]
        public IActionResult GetPaymentsByAccount(Guid accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var payments = _paymentHubService.GetPaymentsByAccount(accountId, fromDate, toDate);
            return SUCCESS(payments);
        }

        /// <summary>
        /// Get payments in fiscal period - الحصول على دفعات الفترة المالية
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="fiscalPeriod">Fiscal period (month)</param>
        /// <returns>List of payments</returns>
        [HttpGet("fiscal/{fiscalYear}/{fiscalPeriod}")]
        public IActionResult GetPaymentsInFiscalPeriod(int fiscalYear, int fiscalPeriod)
        {
            var payments = _paymentHubService.GetPaymentsInFiscalPeriod(fiscalYear, fiscalPeriod);
            return SUCCESS(payments);
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Get counterparty payment summary - ملخص دفعات الطرف المقابل
        /// </summary>
        /// <param name="counterpartyId">Counterparty ID</param>
        /// <returns>Payment summary</returns>
        [HttpGet("summary/counterparty/{counterpartyId}")]
        public IActionResult GetCounterpartyPaymentSummary(Guid counterpartyId)
        {
            var summary = _paymentHubService.GetCounterpartyPaymentSummary(counterpartyId);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get payment aging report - تقرير أعمار الدفعات
        /// </summary>
        /// <param name="asOfDate">As of date</param>
        /// <param name="counterpartyType">Optional counterparty type filter</param>
        /// <returns>Aging report</returns>
        [HttpGet("aging")]
        public IActionResult GetPaymentAgingReport([FromQuery] DateTime? asOfDate, [FromQuery] CounterpartyType? counterpartyType)
        {
            var date = asOfDate ?? DateTime.Today;
            var report = _paymentHubService.GetPaymentAgingReport(date, counterpartyType);
            return SUCCESS(report);
        }

        #endregion

        #region ARAP Compatibility Endpoints

        /// <summary>
        /// Get next payment number - الحصول على رقم الدفعة التالي
        /// For backward compatibility with ARAP UI
        /// </summary>
        /// <param name="paymentType">Payment type (0=CustomerReceipt, 1=VendorPayment)</param>
        /// <returns>Next payment number</returns>
        [HttpGet("next-number")]
        public IActionResult GetNextPaymentNumber([FromQuery] int paymentType)
        {
            var number = _paymentHubService.GetNextPaymentNumber((PaymentType)paymentType);
            return SUCCESS(number);
        }

        /// <summary>
        /// Get GL journal preview for payment - معاينة القيد المحاسبي للدفعة
        /// Shows what GL entries will be created when posting
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>GL journal preview</returns>
        [HttpGet("{id}/gl-preview")]
        public IActionResult GetPaymentGLPreview(Guid id)
        {
            var preview = _paymentHubService.GetPaymentGLPreview(id);
            if (preview == null)
            {
                return ToResponse(404, "Payment not found - الدفعة غير موجودة");
            }
            return SUCCESS(preview);
        }

        /// <summary>
        /// Approve and Post payment in one step - اعتماد وترحيل الدفعة في خطوة واحدة
        /// For backward compatibility with ARAP UI that doesn't have separate approve step
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Post result</returns>
        [HttpPost("{id}/approve-and-post")]
        public IActionResult ApproveAndPostPayment(Guid id)
        {
            var userId = HttpContext.GetUId().ToString();

            // First approve
            var approveResult = _paymentHubService.ApprovePayment(new ApprovePaymentDto
            {
                PaymentId = id,
                Comments = "Auto-approved for posting"
            }, userId);

            if (!approveResult.Success)
            {
                // If already approved, continue to post
                if (approveResult.ErrorCode != "INVALID_STATUS")
                {
                    return ToResponse(GetStatusCode(approveResult.ErrorCode), approveResult.ErrorMessage);
                }
            }

            // Then post
            var postResult = _paymentHubService.PostPayment(new PostPaymentDto
            {
                PaymentId = id,
                IdempotencyKey = $"auto-post-{id}-{DateTime.Now.Ticks}"
            }, userId);

            if (postResult.Success)
            {
                return SUCCESS(postResult);
            }

            return ToResponse(GetStatusCode(postResult.ErrorCode), postResult.ErrorMessage);
        }

        #endregion

        #region Atomic Operations (ERP Best Practice)

        /// <summary>
        /// Get all active treasury accounts (bank + cash) for payment dropdown - الحصول على حسابات الخزينة النشطة
        /// </summary>
        /// <param name="currency">Optional currency filter</param>
        /// <returns>Combined list of bank and cash accounts</returns>
        [HttpGet("treasury-accounts")]
        public IActionResult GetActiveTreasuryAccounts([FromQuery] string currency = null)
        {
            var accounts = _paymentHubService.GetActiveTreasuryAccounts(currency);
            return SUCCESS(accounts);
        }

        /// <summary>
        /// Preview GL journal for payment data WITHOUT creating the payment - معاينة قيد الدفعة بدون إنشاء
        /// This is the correct flow: user previews first, then creates-and-posts atomically
        /// </summary>
        /// <param name="dto">Preview request with payment details</param>
        /// <returns>GL journal preview with guaranteed balanced entries</returns>
        [HttpPost("preview-journal")]
        public IActionResult PreviewPaymentJournal([FromBody] PreviewPaymentJournalDto dto)
        {
            var preview = _paymentHubService.GenerateJournalPreview(dto);
            return SUCCESS(preview);
        }

        /// <summary>
        /// Create and post payment in single atomic transaction - إنشاء وترحيل الدفعة في معاملة واحدة
        /// Payment number is ONLY generated when posting succeeds.
        /// This prevents burned payment numbers on Cancel.
        /// </summary>
        /// <param name="dto">Payment creation request with treasury account</param>
        /// <returns>Post result with payment ID and GL journal ID</returns>
        [HttpPost("create-and-post")]
        public IActionResult CreateAndPostPayment([FromBody] CreatePaymentDto dto)
        {
            var userId = HttpContext.GetUId().ToString();

            // Debug logging
            Console.WriteLine($"[CreateAndPost] Starting for userId: {userId}");
            Console.WriteLine($"[CreateAndPost] PaymentType: {dto.PaymentType}, Amount: {dto.Amount}, Currency: {dto.Currency}");
            Console.WriteLine($"[CreateAndPost] DestinationAccountId: {dto.DestinationAccountId}, SourceAccountId: {dto.SourceAccountId}");

            // Log allocations received from frontend
            if (dto.Allocations != null && dto.Allocations.Count > 0)
            {
                Console.WriteLine($"[CreateAndPost] Allocations received: {dto.Allocations.Count}");
                foreach (var alloc in dto.Allocations)
                {
                    Console.WriteLine($"[CreateAndPost] -> Allocation: DocumentId={alloc.DocumentId}, Amount={alloc.AllocatedAmount}");
                }
            }
            else
            {
                Console.WriteLine("[CreateAndPost] No allocations received");
            }

            var result = _paymentHubService.CreateAndPostPayment(dto, userId);

            Console.WriteLine($"[CreateAndPost] Result.Success: {result?.Success}, PaymentId: {result?.PaymentId}, PaymentNumber: {result?.PaymentNumber}");
            Console.WriteLine($"[CreateAndPost] ErrorCode: {result?.ErrorCode}, ErrorMessage: {result?.ErrorMessage}");

            if (result == null)
            {
                Console.WriteLine("[CreateAndPost] ERROR: result is NULL!");
                return ToResponse(500, "Service returned null result");
            }

            if (result.Success)
            {
                Console.WriteLine("[CreateAndPost] SUCCESS - returning result");
                return SUCCESS(result);
            }

            Console.WriteLine($"[CreateAndPost] FAILED - ErrorCode: {result.ErrorCode}");
            return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
        }

        #endregion

        #region Helpers

        private int GetStatusCode(string errorCode)
        {
            return errorCode switch
            {
                "NOT_FOUND" => 404,
                "VALIDATION_FAILED" => 400,
                "INVALID_STATUS" => 400,
                "ALREADY_REVERSED" => 400,
                "INSUFFICIENT_BALANCE" => 400,
                "GL_JOURNAL_FAILED" => 400,
                "CREATE_POST_FAILED" => 500,
                _ => 500
            };
        }

        /// <summary>
        /// Return error response with HTTP status code and message
        /// </summary>
        private IActionResult ToResponse(int statusCode, string message)
        {
            // Use anonymous object to match API response format
            var response = new { code = statusCode, msg = message };
            return Content(
                Newtonsoft.Json.JsonConvert.SerializeObject(response, new Newtonsoft.Json.JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                }),
                "application/json"
            );
        }

        #endregion
    }

    /// <summary>
    /// Cancel payment request body
    /// </summary>
    public class CancelPaymentRequest
    {
        /// <summary>
        /// Cancellation reason - سبب الإلغاء
        /// </summary>
        public string Reason { get; set; }
    }
}
