using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.PaymentHub.Dto;
using ZR.Model.Finance.PaymentHub.Enums;

namespace ZR.Service.IService.Finance.PaymentHub
{
    /// <summary>
    /// Payment Hub Service Interface - واجهة خدمة مركز الدفعات
    /// THE SINGLE canonical service for ALL payment operations in the system.
    /// All ARAP, Treasury, and other modules must use this interface.
    /// </summary>
    public interface IPaymentHubService
    {
        #region Payment CRUD

        /// <summary>
        /// Create a new payment - إنشاء دفعة جديدة
        /// Idempotent: If IdempotencyKey already exists, returns existing payment
        /// </summary>
        PaymentOperationResult CreatePayment(CreatePaymentDto dto, string userId);

        /// <summary>
        /// Get payment by ID - الحصول على دفعة بالمعرف
        /// </summary>
        PaymentDetailDto GetPayment(Guid paymentId);

        /// <summary>
        /// Get payment by payment number - الحصول على دفعة برقم الدفعة
        /// </summary>
        PaymentDetailDto GetPaymentByNumber(string paymentNumber);

        /// <summary>
        /// Get payments list with paging - الحصول على قائمة الدفعات
        /// </summary>
        PagedInfo<PaymentListDto> GetPaymentList(PaymentQueryDto query);

        /// <summary>
        /// Delete payment (Draft only) - حذف دفعة (مسودة فقط)
        /// </summary>
        PaymentOperationResult DeletePayment(Guid paymentId, string userId);

        #endregion

        #region Payment Lifecycle (State Machine)

        /// <summary>
        /// Approve payment (Draft → Approved) - اعتماد دفعة
        /// </summary>
        PaymentOperationResult ApprovePayment(ApprovePaymentDto dto, string userId);

        /// <summary>
        /// Post payment (Approved → Posted) - ترحيل دفعة
        /// ATOMIC: Single PostgreSQL transaction for:
        /// 1. GL Journal creation & posting
        /// 2. Balance Ledger update
        /// 3. Account balance update
        /// 4. Payment status update
        /// 5. Audit event creation
        /// Idempotent: Uses IdempotencyKey to prevent duplicate posts
        /// </summary>
        PostPaymentResult PostPayment(PostPaymentDto dto, string userId);

        /// <summary>
        /// Reverse payment (Posted → Reversed) - عكس دفعة
        /// ATOMIC: Reverses all effects including allocations, GL, balances
        /// </summary>
        PaymentOperationResult ReversePayment(ReversePaymentDto dto, string userId);

        /// <summary>
        /// Cancel payment (Draft/Approved → Cancelled) - إلغاء دفعة
        /// </summary>
        PaymentOperationResult CancelPayment(Guid paymentId, string reason, string userId);

        #endregion

        #region Allocation Operations

        /// <summary>
        /// Allocate payment to documents - تخصيص دفعة للمستندات
        /// ATOMIC: Updates both payment and document balances
        /// FX-Aware: Handles multi-currency with exchange rates
        /// </summary>
        List<AllocationOperationResult> AllocatePayment(AllocatePaymentDto dto, string userId);

        /// <summary>
        /// Reverse allocation - عكس تخصيص
        /// </summary>
        AllocationOperationResult ReverseAllocation(ReverseAllocationDto dto, string userId);

        /// <summary>
        /// Get allocations for payment - الحصول على تخصيصات الدفعة
        /// </summary>
        List<AllocationListDto> GetPaymentAllocations(Guid paymentId);

        /// <summary>
        /// Get allocations for document - الحصول على تخصيصات المستند
        /// </summary>
        List<AllocationListDto> GetDocumentAllocations(Guid documentId, AllocatableDocumentType documentType);

        /// <summary>
        /// Auto-allocate payment to oldest invoices (FIFO) - تخصيص تلقائي
        /// </summary>
        List<AllocationOperationResult> AutoAllocate(Guid paymentId, string userId);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get payments with unallocated balance for counterparty - الدفعات غير المخصصة
        /// </summary>
        List<PaymentListDto> GetUnallocatedPayments(Guid? counterpartyId = null, PaymentType? paymentType = null);

        /// <summary>
        /// Get payment events (audit trail) - الحصول على أحداث الدفعة
        /// </summary>
        List<PaymentEventDto> GetPaymentEvents(Guid paymentId);

        /// <summary>
        /// Get payments by account - الحصول على دفعات الحساب
        /// </summary>
        List<PaymentListDto> GetPaymentsByAccount(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get payments in fiscal period - الحصول على دفعات الفترة المالية
        /// </summary>
        List<PaymentListDto> GetPaymentsInFiscalPeriod(int fiscalYear, int fiscalPeriod);

        #endregion

        #region Validation

        /// <summary>
        /// Validate payment can be posted - التحقق من إمكانية الترحيل
        /// </summary>
        PaymentOperationResult ValidateForPosting(Guid paymentId);

        /// <summary>
        /// Validate allocation - التحقق من التخصيص
        /// </summary>
        AllocationOperationResult ValidateAllocation(Guid paymentId, CreateAllocationDto allocation);

        #endregion

        #region Reporting

        /// <summary>
        /// Get payment summary by counterparty - ملخص الدفعات للطرف المقابل
        /// </summary>
        CounterpartyPaymentSummary GetCounterpartyPaymentSummary(Guid counterpartyId);

        /// <summary>
        /// Get payment aging report - تقرير أعمار الدفعات
        /// </summary>
        List<PaymentAgingItem> GetPaymentAgingReport(DateTime asOfDate, CounterpartyType? counterpartyType = null);

        #endregion

        #region ARAP Compatibility

        /// <summary>
        /// Get next payment number - الحصول على رقم الدفعة التالي
        /// For backward compatibility with ARAP UI
        /// </summary>
        string GetNextPaymentNumber(PaymentType paymentType);

        /// <summary>
        /// Get GL journal preview for payment - معاينة القيد المحاسبي للدفعة
        /// Shows what GL entries will be created when posting
        /// </summary>
        GLJournalPreviewDto GetPaymentGLPreview(Guid paymentId);

        #endregion

        #region Atomic Operations (ERP Best Practice)

        /// <summary>
        /// Generate GL journal preview from payment data WITHOUT creating a payment - معاينة قيد الدفعة بدون إنشاء
        /// This is the correct flow: preview first, then create-and-post atomically
        /// </summary>
        GLJournalPreviewDto GenerateJournalPreview(PreviewPaymentJournalDto dto);

        /// <summary>
        /// Create and post payment in a single atomic transaction - إنشاء وترحيل الدفعة في معاملة واحدة
        /// Payment number is ONLY generated when the transaction succeeds.
        /// If any step fails, the entire operation is rolled back and no number is burned.
        /// This follows SAP/Oracle/Dynamics best practices.
        /// </summary>
        PostPaymentResult CreateAndPostPayment(CreatePaymentDto dto, string userId);

        /// <summary>
        /// Get all active treasury accounts (bank + cash) for payment dropdown - الحصول على حسابات الخزينة النشطة
        /// </summary>
        List<TreasuryAccountDto> GetActiveTreasuryAccounts(string currency = null);

        #endregion
    }

    /// <summary>
    /// Treasury Account DTO for payment dropdown - حساب الخزينة للقائمة المنسدلة
    /// </summary>
    public class TreasuryAccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; } // "bank" or "cash"
        public string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string GLAccountCode { get; set; }
        public string GLAccountName { get; set; }
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// GL Journal Preview DTO - معاينة القيد المحاسبي
    /// </summary>
    public class GLJournalPreviewDto
    {
        public string JournalDate { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string ReferenceNumber { get; set; }
        public string ReferenceType { get; set; }
        public string ReferenceId { get; set; }
        public List<GLJournalEntryPreviewDto> Entries { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced { get; set; }
    }

    /// <summary>
    /// GL Journal Entry Preview DTO - معاينة بند القيد المحاسبي
    /// </summary>
    public class GLJournalEntryPreviewDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal BaseDebit { get; set; }
        public decimal BaseCredit { get; set; }
        public string Description { get; set; }
        public string CostCenterId { get; set; }
        public string CostCenterName { get; set; }
    }

    #region Additional DTOs for Service

    /// <summary>
    /// Counterparty Payment Summary - ملخص دفعات الطرف المقابل
    /// </summary>
    public class CounterpartyPaymentSummary
    {
        public Guid CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal TotalUnallocated { get; set; }
        public int UnallocatedCount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }

    /// <summary>
    /// Payment Aging Item - عنصر تقرير أعمار الدفعات
    /// </summary>
    public class PaymentAgingItem
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid? CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public decimal Amount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public string Currency { get; set; }
        public int DaysOld { get; set; }
        public string AgingBucket { get; set; }
    }

    #endregion
}
