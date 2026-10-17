using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.Finance.PaymentHub.Enums;

namespace ZR.Model.Finance.PaymentHub.Dto
{
    #region Payment DTOs

    /// <summary>
    /// Create Payment Request - طلب إنشاء دفعة
    /// </summary>
    public class CreatePaymentDto
    {
        /// <summary>
        /// Idempotency Key (client-generated, prevents duplicates) - مفتاح منع التكرار
        /// </summary>
        [StringLength(100)]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// Payment Type - نوع الدفعة
        /// </summary>
        [Required]
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Payment Method - طريقة الدفع
        /// </summary>
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Payment Date - تاريخ الدفعة
        /// </summary>
        [Required]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Value Date (optional, defaults to PaymentDate) - تاريخ القيمة
        /// </summary>
        public DateTime? ValueDate { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency (ISO 4217) - العملة
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate to Base Currency - سعر الصرف
        /// </summary>
        [Range(0.000001, double.MaxValue)]
        public decimal ExchangeRate { get; set; } = 1m;

        /// <summary>
        /// Source Account ID (for outgoing payments) - معرف الحساب المصدر
        /// </summary>
        public Guid? SourceAccountId { get; set; }

        /// <summary>
        /// Source Account Type - نوع الحساب المصدر
        /// </summary>
        public PaymentAccountType? SourceAccountType { get; set; }

        /// <summary>
        /// Destination Account ID (for incoming payments) - معرف الحساب الوجهة
        /// </summary>
        public Guid? DestinationAccountId { get; set; }

        /// <summary>
        /// Destination Account Type - نوع الحساب الوجهة
        /// </summary>
        public PaymentAccountType? DestinationAccountType { get; set; }

        /// <summary>
        /// Counterparty Type - نوع الطرف المقابل
        /// </summary>
        public CounterpartyType? CounterpartyType { get; set; }

        /// <summary>
        /// Counterparty ID - معرف الطرف المقابل
        /// </summary>
        public Guid? CounterpartyId { get; set; }

        /// <summary>
        /// Counterparty Name - اسم الطرف المقابل
        /// </summary>
        [StringLength(200)]
        public string CounterpartyName { get; set; }

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// External Reference Number - رقم المرجع الخارجي
        /// </summary>
        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Bank Reference - مرجع البنك
        /// </summary>
        [StringLength(100)]
        public string BankReference { get; set; }

        /// <summary>
        /// Cheque Number (if method is cheque) - رقم الشيك
        /// </summary>
        [StringLength(50)]
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque ID (link to existing cheque) - معرف الشيك
        /// </summary>
        public Guid? ChequeId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }

        /// <summary>
        /// Initial Allocations (optional - can allocate during creation) - التخصيصات الأولية
        /// </summary>
        public List<CreateAllocationDto> Allocations { get; set; }

        /// <summary>
        /// Voucher Type - نوع السند
        /// "Receipt" = سند قبض (Receipt Voucher), "Disbursement" = سند صرف (Disbursement Voucher)
        /// When set, automatically sets PaymentType and generates VoucherNumber
        /// </summary>
        [StringLength(20)]
        public string VoucherType { get; set; }
    }

    /// <summary>
    /// Approve Payment Request - طلب اعتماد دفعة
    /// </summary>
    public class ApprovePaymentDto
    {
        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Approval Comments - تعليقات الاعتماد
        /// </summary>
        [StringLength(500)]
        public string Comments { get; set; }
    }

    /// <summary>
    /// Post Payment Request - طلب ترحيل دفعة
    /// </summary>
    public class PostPaymentDto
    {
        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Idempotency Key for Post operation - مفتاح منع التكرار للترحيل
        /// </summary>
        [StringLength(100)]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// Posting Notes - ملاحظات الترحيل
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Reverse Payment Request - طلب عكس دفعة
    /// </summary>
    public class ReversePaymentDto
    {
        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Reversal Reason - سبب العكس
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        /// <summary>
        /// Reversal Date (defaults to now) - تاريخ العكس
        /// </summary>
        public DateTime? ReversalDate { get; set; }
    }

    /// <summary>
    /// Payment List DTO - عرض قائمة الدفعات
    /// </summary>
    public class PaymentListDto
    {
        public Guid Id { get; set; }
        public string PaymentNumber { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodDisplay { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public string CounterpartyName { get; set; }
        public string ReferenceNumber { get; set; }
        public bool IsReversed { get; set; }
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Voucher Type - نوع السند ("Receipt" or "Disbursement")
        /// </summary>
        public string VoucherType { get; set; }

        /// <summary>
        /// Voucher Number - رقم السند
        /// </summary>
        public string VoucherNumber { get; set; }
    }

    /// <summary>
    /// Payment Detail DTO - تفاصيل الدفعة
    /// </summary>
    public class PaymentDetailDto
    {
        public Guid Id { get; set; }
        public string PaymentNumber { get; set; }
        public string IdempotencyKey { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodDisplay { get; set; }
        public PaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime? ValueDate { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReversedAt { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal BaseCurrencyAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public decimal FxGainLoss { get; set; }

        public Guid? SourceAccountId { get; set; }
        public string SourceAccountName { get; set; }
        public PaymentAccountType? SourceAccountType { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public string DestinationAccountName { get; set; }
        public PaymentAccountType? DestinationAccountType { get; set; }

        public CounterpartyType? CounterpartyType { get; set; }
        public Guid? CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }

        public Guid? GLJournalId { get; set; }
        public string GLJournalNumber { get; set; }
        public Guid? BalanceLedgerEntryId { get; set; }

        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }

        public Guid? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid? ProjectId { get; set; }
        public string ProjectName { get; set; }

        public bool IsReversed { get; set; }
        public Guid? ReversalOfId { get; set; }
        public Guid? ReversedById { get; set; }
        public string ReversalReason { get; set; }

        public string ApprovedBy { get; set; }
        public string PostedBy { get; set; }
        public string ApprovalComments { get; set; }

        public string ReferenceNumber { get; set; }
        public string BankReference { get; set; }
        public string ChequeNumber { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }

        public DateTime? CreateTime { get; set; }
        public string CreateBy { get; set; }

        /// <summary>
        /// Voucher Type - نوع السند ("Receipt" or "Disbursement")
        /// </summary>
        public string VoucherType { get; set; }

        /// <summary>
        /// Voucher Number - رقم السند
        /// </summary>
        public string VoucherNumber { get; set; }

        public List<AllocationListDto> Allocations { get; set; }
        public List<PaymentEventDto> Events { get; set; }
    }

    /// <summary>
    /// Payment Query DTO - استعلام الدفعات
    /// </summary>
    public class PaymentQueryDto : PagerInfo
    {
        public PaymentType? PaymentType { get; set; }
        public PaymentStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Currency { get; set; }
        public Guid? CounterpartyId { get; set; }
        public CounterpartyType? CounterpartyType { get; set; }
        public Guid? SourceAccountId { get; set; }
        public Guid? DestinationAccountId { get; set; }
        public string PaymentNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public bool? IsReversed { get; set; }
        public bool? HasUnallocatedAmount { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Filter by Voucher Type - تصفية حسب نوع السند
        /// </summary>
        public string VoucherType { get; set; }
    }

    #endregion

    #region Allocation DTOs

    /// <summary>
    /// Create Allocation Request - طلب إنشاء تخصيص
    /// </summary>
    public class CreateAllocationDto
    {
        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// Document Type - نوع المستند
        /// </summary>
        [Required]
        public AllocatableDocumentType DocumentType { get; set; }

        /// <summary>
        /// Document ID (Invoice ID) - معرف المستند
        /// </summary>
        [Required]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Allocated Amount in Payment Currency - المبلغ المخصص بعملة الدفعة
        /// </summary>
        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Exchange Rate (if document currency differs) - سعر الصرف
        /// </summary>
        [Range(0.000001, double.MaxValue)]
        public decimal ExchangeRate { get; set; } = 1m;

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Allocate Payment Request (batch allocation) - طلب تخصيص دفعة (دفعة واحدة)
    /// </summary>
    public class AllocatePaymentDto
    {
        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Allocations - التخصيصات
        /// </summary>
        [Required]
        public List<CreateAllocationDto> Allocations { get; set; }
    }

    /// <summary>
    /// Reverse Allocation Request - طلب عكس تخصيص
    /// </summary>
    public class ReverseAllocationDto
    {
        /// <summary>
        /// Allocation ID - معرف التخصيص
        /// </summary>
        [Required]
        public Guid AllocationId { get; set; }

        /// <summary>
        /// Reason - السبب
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
    }

    /// <summary>
    /// Allocation List DTO - عرض قائمة التخصيصات
    /// </summary>
    public class AllocationListDto
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; }
        public AllocatableDocumentType DocumentType { get; set; }
        public string DocumentTypeDisplay { get; set; }
        public Guid DocumentId { get; set; }
        public string DocumentNumber { get; set; }
        public decimal AllocatedAmount { get; set; }
        public string PaymentCurrency { get; set; }
        public decimal AllocatedAmountDocCurrency { get; set; }
        public string DocumentCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal FxGainLoss { get; set; }
        public AllocationStatus Status { get; set; }
        public bool IsReversed { get; set; }
        public DateTime AllocationDate { get; set; }
        public string Notes { get; set; }
    }

    #endregion

    #region Event DTOs

    /// <summary>
    /// Payment Event DTO - عرض حدث الدفعة
    /// </summary>
    public class PaymentEventDto
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public PaymentEventType EventType { get; set; }
        public string EventTypeDisplay { get; set; }
        public DateTime EventTimestamp { get; set; }
        public PaymentStatus? PreviousStatus { get; set; }
        public PaymentStatus? NewStatus { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    #endregion

    #region Preview DTOs

    /// <summary>
    /// Preview Payment Journal Request - طلب معاينة قيد الدفعة
    /// Used to generate GL journal preview WITHOUT creating a payment
    /// </summary>
    public class PreviewPaymentJournalDto
    {
        /// <summary>
        /// Payment Type - نوع الدفعة
        /// </summary>
        [Required]
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Payment Date - تاريخ الدفعة
        /// </summary>
        [Required]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency (ISO 4217) - العملة
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate to Base Currency - سعر الصرف
        /// </summary>
        [Range(0.000001, double.MaxValue)]
        public decimal ExchangeRate { get; set; } = 1m;

        /// <summary>
        /// Treasury Account ID (bank or cash) - معرف حساب الخزينة
        /// </summary>
        [Required]
        public Guid TreasuryAccountId { get; set; }

        /// <summary>
        /// Treasury Account Code - رمز حساب الخزينة
        /// </summary>
        public string TreasuryAccountCode { get; set; }

        /// <summary>
        /// Treasury Account Name - اسم حساب الخزينة
        /// </summary>
        public string TreasuryAccountName { get; set; }

        /// <summary>
        /// Treasury Account Type - نوع حساب الخزينة
        /// </summary>
        public PaymentAccountType TreasuryAccountType { get; set; }

        /// <summary>
        /// Counterparty ID - معرف الطرف المقابل
        /// </summary>
        public Guid? CounterpartyId { get; set; }

        /// <summary>
        /// Counterparty Name - اسم الطرف المقابل
        /// </summary>
        public string CounterpartyName { get; set; }

        /// <summary>
        /// Counterparty GL Account Code - رمز حساب الطرف المقابل
        /// </summary>
        public string CounterpartyGLAccountCode { get; set; }

        /// <summary>
        /// Counterparty GL Account Name - اسم حساب الطرف المقابل
        /// </summary>
        public string CounterpartyGLAccountName { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }

    #endregion

    #region Result DTOs

    /// <summary>
    /// Payment Operation Result - نتيجة عملية الدفعة
    /// </summary>
    public class PaymentOperationResult
    {
        public bool Success { get; set; }
        public Guid? PaymentId { get; set; }
        public string PaymentNumber { get; set; }
        public PaymentStatus? NewStatus { get; set; }
        public Guid? GLJournalId { get; set; }
        public Guid? BalanceLedgerEntryId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageAr { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Allocation Operation Result - نتيجة عملية التخصيص
    /// </summary>
    public class AllocationOperationResult
    {
        public bool Success { get; set; }
        public Guid? AllocationId { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public decimal? PaymentUnallocatedBalance { get; set; }
        public decimal? DocumentRemainingBalance { get; set; }
        public decimal? FxGainLoss { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageAr { get; set; }
    }

    /// <summary>
    /// Post Payment Result - نتيجة ترحيل الدفعة
    /// </summary>
    public class PostPaymentResult : PaymentOperationResult
    {
        public decimal? SourceAccountBalanceBefore { get; set; }
        public decimal? SourceAccountBalanceAfter { get; set; }
        public decimal? DestinationAccountBalanceBefore { get; set; }
        public decimal? DestinationAccountBalanceAfter { get; set; }
    }

    #endregion
}
