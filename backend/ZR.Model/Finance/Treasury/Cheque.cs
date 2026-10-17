using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Cheque - شيك
    /// Enhanced with Full Lifecycle, Available Balance Impact, Bounce Handling, and Attachments
    /// </summary>
    [SugarTable("cheque")]
    public class Cheque : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Cheque ID - معرف الشيك
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Cheque Number - رقم الشيك
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "cheque_number")]
        public string ChequeNumber { get; set; }

        /// <summary>
        /// Cheque Type - نوع الشيك (صادر/وارد)
        /// </summary>
        [SugarColumn(ColumnName = "cheque_type")]
        public ChequeType ChequeType { get; set; }

        /// <summary>
        /// Idempotency Key - مفتاح التكرار
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "idempotency_key")]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// Bank Account ID - معرف الحساب البنكي المصدر
        /// </summary>
        [SugarColumn(ColumnName = "bank_account_id")]
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Drawer Name - اسم الساحب (for incoming cheques)
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "drawer_name")]
        public string DrawerName { get; set; }

        /// <summary>
        /// Drawer Bank Name - اسم بنك الساحب
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "drawer_bank_name")]
        public string DrawerBankName { get; set; }

        /// <summary>
        /// Payee Name - اسم المستفيد
        /// </summary>
        [SugarColumn(Length = 200, ColumnName = "payee_name")]
        public string PayeeName { get; set; }

        /// <summary>
        /// Issue Date - تاريخ الإصدار
        /// </summary>
        [SugarColumn(ColumnName = "issue_date")]
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        [SugarColumn(ColumnName = "due_date")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Presented Date - تاريخ التقديم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "presented_date")]
        public DateTime? PresentedDate { get; set; }

        /// <summary>
        /// Cleared Date - تاريخ التصفية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cleared_date")]
        public DateTime? ClearedDate { get; set; }

        /// <summary>
        /// Bounced Date - تاريخ الارتجاع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bounced_date")]
        public DateTime? BouncedDate { get; set; }

        /// <summary>
        /// Cancelled Date - تاريخ الإلغاء
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [SugarColumn(ColumnName = "amount", ColumnDataType = "decimal(18,4)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency Code - رمز العملة
        /// </summary>
        [SugarColumn(Length = 3, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Currency ID - معرف العملة (FK)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "currency_id")]
        public Guid? CurrencyId { get; set; }

        /// <summary>
        /// Bank Fees - رسوم البنك (for bounced cheques)
        /// </summary>
        [SugarColumn(ColumnName = "bank_fees", ColumnDataType = "decimal(18,4)")]
        public decimal BankFees { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public ChequeStatus Status { get; set; } = ChequeStatus.Issued;

        /// <summary>
        /// Bounce Reason - سبب الارتجاع
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "bounce_reason")]
        public string BounceReason { get; set; }

        /// <summary>
        /// Bounce Count - عدد مرات الارتجاع
        /// </summary>
        [SugarColumn(ColumnName = "bounce_count")]
        public int BounceCount { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "cancellation_reason")]
        public string CancellationReason { get; set; }

        /// <summary>
        /// Related Payment ID - معرف الدفعة المرتبطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "related_payment_id")]
        public Guid? RelatedPaymentId { get; set; }

        /// <summary>
        /// Reversal Payment ID - معرف دفعة العكس (for bounced cheques)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversal_payment_id")]
        public Guid? ReversalPaymentId { get; set; }

        /// <summary>
        /// GL Journal ID (Issuance) - معرف قيد الإصدار
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "issuance_journal_id")]
        public Guid? IssuanceJournalId { get; set; }

        /// <summary>
        /// GL Journal ID (Clearing) - معرف قيد التصفية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "clearing_journal_id")]
        public Guid? ClearingJournalId { get; set; }

        /// <summary>
        /// GL Journal ID (Bounce) - معرف قيد الارتجاع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bounce_journal_id")]
        public Guid? BounceJournalId { get; set; }

        /// <summary>
        /// Bank Fees Journal ID - معرف قيد رسوم البنك
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bank_fees_journal_id")]
        public Guid? BankFeesJournalId { get; set; }

        /// <summary>
        /// Clearing Ledger Entry ID - معرف قيد سجل التصفية
        /// Links to the BalanceLedger entry for audit trail
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "clearing_ledger_entry_id")]
        public Guid? ClearingLedgerEntryId { get; set; }

        /// <summary>
        /// Bounce Ledger Entry ID - معرف قيد سجل الارتجاع
        /// Links to the BalanceLedger entry for bounce audit trail
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bounce_ledger_entry_id")]
        public Guid? BounceLedgerEntryId { get; set; }

        /// <summary>
        /// Impacts Available Balance - هل يؤثر على الرصيد المتاح
        /// </summary>
        [SugarColumn(ColumnName = "impacts_available_balance")]
        public bool ImpactsAvailableBalance { get; set; } = true;

        /// <summary>
        /// Available Balance Adjusted - هل تم تعديل الرصيد المتاح
        /// </summary>
        [SugarColumn(ColumnName = "available_balance_adjusted")]
        public bool AvailableBalanceAdjusted { get; set; }

        /// <summary>
        /// Attachment Path - مسار المرفق (صورة الشيك)
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "attachment_path")]
        public string AttachmentPath { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_no")]
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Memo - الملاحظة
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "memo")]
        public string Memo { get; set; }

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

        /// <summary>
        /// Row Version - نسخة الصف (للتزامن المتفائل)
        /// </summary>
        [SugarColumn(ColumnName = "row_version", IsEnableUpdateVersionValidation = true)]
        public int RowVersion { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Navigation - Bank Account
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BankAccount BankAccount { get; set; }

        /// <summary>
        /// Navigation - Related Payment
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Payment RelatedPayment { get; set; }
    }
}






























