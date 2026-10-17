using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.PaymentHub
{
    /// <summary>
    /// Payment - الدفعة
    /// THE canonical single source of truth for all payments in the system.
    /// All payment operations (AR, AP, Treasury) flow through this entity.
    /// </summary>
    [SugarTable("hub_payment", "Payment Hub - الدفعات الموحدة")]
    public class Payment : SysBase
    {
        /// <summary>
        /// Ignore BranchID from SysBase - using CompanyID instead
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        #region Primary Key & Identification

        /// <summary>
        /// Primary Key - المعرف الأساسي (UUID v7 for time-ordered IDs)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Payment Number - رقم الدفعة (unique, human-readable)
        /// Format: PMT-YYYYMMDD-XXXXX
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "payment_number")]
        public string PaymentNumber { get; set; }

        /// <summary>
        /// Idempotency Key - مفتاح منع التكرار
        /// UNIQUE constraint at DB level prevents duplicate submissions
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "idempotency_key")]
        public string IdempotencyKey { get; set; }

        #endregion

        #region Payment Classification

        /// <summary>
        /// Payment Type - نوع الدفعة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_type")]
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Payment Method - طريقة الدفع
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_method")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Draft;

        #endregion

        #region Dates

        /// <summary>
        /// Payment Date - تاريخ الدفعة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_date")]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Value Date - تاريخ القيمة (when funds actually move)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "value_date")]
        public DateTime? ValueDate { get; set; }

        /// <summary>
        /// Posted At - تاريخ الترحيل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "posted_at")]
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Approved At - تاريخ الاعتماد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_at")]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Reversed At - تاريخ العكس
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversed_at")]
        public DateTime? ReversedAt { get; set; }

        #endregion

        #region Amounts & Currency

        /// <summary>
        /// Amount in Transaction Currency - المبلغ بعملة المعاملة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "amount", DecimalDigits = 4)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency Code (ISO 4217) - رمز العملة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = false, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate to Base Currency - سعر الصرف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "exchange_rate", DecimalDigits = 6)]
        public decimal ExchangeRate { get; set; } = 1m;

        /// <summary>
        /// Amount in Base Currency - المبلغ بالعملة الأساسية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "base_currency_amount", DecimalDigits = 4)]
        public decimal BaseCurrencyAmount { get; set; }

        /// <summary>
        /// Allocated Amount - المبلغ المخصص
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allocated_amount", DecimalDigits = 4)]
        public decimal AllocatedAmount { get; set; } = 0m;

        /// <summary>
        /// Unallocated Amount - المبلغ غير المخصص
        /// Computed: Amount - AllocatedAmount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "unallocated_amount", DecimalDigits = 4)]
        public decimal UnallocatedAmount { get; set; }

        /// <summary>
        /// FX Gain/Loss (realized on allocation) - ربح/خسارة الصرف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fx_gain_loss", DecimalDigits = 4)]
        public decimal FxGainLoss { get; set; } = 0m;

        #endregion

        #region Account References

        /// <summary>
        /// Source Account ID - معرف الحساب المصدر
        /// For outgoing payments (VendorPayment, Transfer)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "source_account_id")]
        public Guid? SourceAccountId { get; set; }

        /// <summary>
        /// Source Account Type - نوع الحساب المصدر
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "source_account_type")]
        public PaymentAccountType? SourceAccountType { get; set; }

        /// <summary>
        /// Destination Account ID - معرف الحساب الوجهة
        /// For incoming payments (CustomerReceipt, Transfer)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "destination_account_id")]
        public Guid? DestinationAccountId { get; set; }

        /// <summary>
        /// Destination Account Type - نوع الحساب الوجهة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "destination_account_type")]
        public PaymentAccountType? DestinationAccountType { get; set; }

        #endregion

        #region Counterparty

        /// <summary>
        /// Counterparty Type - نوع الطرف المقابل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "counterparty_type")]
        public CounterpartyType? CounterpartyType { get; set; }

        /// <summary>
        /// Counterparty ID - معرف الطرف المقابل (Customer/Vendor/Employee ID)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "counterparty_id")]
        public Guid? CounterpartyId { get; set; }

        /// <summary>
        /// Counterparty Name (denormalized for reporting) - اسم الطرف المقابل
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "counterparty_name")]
        public string CounterpartyName { get; set; }

        #endregion

        #region GL Integration

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Reversal GL Journal ID - معرف قيد العكس
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversal_gl_journal_id")]
        public Guid? ReversalGLJournalId { get; set; }

        /// <summary>
        /// Balance Ledger Entry ID - معرف قيد سجل الرصيد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "balance_ledger_entry_id")]
        public Guid? BalanceLedgerEntryId { get; set; }

        #endregion

        #region Fiscal Period

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_period")]
        public int FiscalPeriod { get; set; }

        #endregion

        #region Segmentation (Cost Accounting)

        /// <summary>
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "department_id")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "project_id")]
        public Guid? ProjectId { get; set; }

        #endregion

        #region Reversal Tracking

        /// <summary>
        /// Is Reversed - هل معكوسة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_reversed")]
        public bool IsReversed { get; set; } = false;

        /// <summary>
        /// Reversal Of ID - معرف الدفعة الأصلية (for reversal payments)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversal_of_id")]
        public Guid? ReversalOfId { get; set; }

        /// <summary>
        /// Reversed By Payment ID - معرف دفعة العكس
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversed_by_id")]
        public Guid? ReversedById { get; set; }

        /// <summary>
        /// Reversal Reason - سبب العكس
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "reversal_reason")]
        public string ReversalReason { get; set; }

        #endregion

        #region Approval

        /// <summary>
        /// Approved By - معتمد بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "approved_by")]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// Posted By - مرحّل بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "posted_by")]
        public string PostedBy { get; set; }

        /// <summary>
        /// Approval Comments - تعليقات الاعتماد
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "approval_comments")]
        public string ApprovalComments { get; set; }

        #endregion

        #region Saudi Voucher (سند قبض / سند صرف)

        /// <summary>
        /// Voucher Type - نوع السند
        /// "Receipt" = سند قبض, "Disbursement" = سند صرف
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "voucher_type")]
        public string VoucherType { get; set; }

        /// <summary>
        /// Voucher Number - رقم السند
        /// Format: RQ-YYYYMM-NNNNN (Receipt) or DS-YYYYMM-NNNNN (Disbursement)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "voucher_number")]
        public string VoucherNumber { get; set; }

        #endregion

        #region References & Notes

        /// <summary>
        /// External Reference Number - رقم المرجع الخارجي
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Bank Reference (for bank transfers) - مرجع البنك
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "bank_reference")]
        public string BankReference { get; set; }

        /// <summary>
        /// Cheque Number (if payment method is cheque) - رقم الشيك
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "cheque_number")]
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque ID (FK to cheque table) - معرف الشيك
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cheque_id")]
        public Guid? ChequeId { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Concurrency & Audit

        /// <summary>
        /// Row Version for optimistic concurrency - إصدار الصف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "row_version", IsEnableUpdateVersionValidation = true)]
        public int RowVersion { get; set; } = 1;

        #endregion

        #region Computed Properties (Not Persisted)

        /// <summary>
        /// Is Fully Allocated - مخصصة بالكامل
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsFullyAllocated => UnallocatedAmount <= 0.0001m;

        /// <summary>
        /// Is Partially Allocated - مخصصة جزئياً
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsPartiallyAllocated => AllocatedAmount > 0 && !IsFullyAllocated;

        /// <summary>
        /// Can Be Posted - يمكن ترحيلها
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool CanBePosted => Status == PaymentStatus.Approved && !IsReversed;

        /// <summary>
        /// Can Be Reversed - يمكن عكسها
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool CanBeReversed => Status == PaymentStatus.Posted && !IsReversed;

        #endregion
    }
}
