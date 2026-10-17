using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.PaymentHub
{
    /// <summary>
    /// Payment Allocation - تخصيص الدفعة
    /// Links payments to invoices/documents with FX support.
    /// Immutable once created - reversals create new entries.
    /// </summary>
    [SugarTable("hub_payment_allocation", "Payment Allocation - تخصيص الدفعات")]
    public class PaymentAllocation : SysBase
    {
        /// <summary>
        /// Ignore BranchID from SysBase
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        #region Primary Key

        /// <summary>
        /// Allocation ID - معرف التخصيص
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        #endregion

        #region Payment Reference

        /// <summary>
        /// Payment ID - معرف الدفعة (FK to hub_payment)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_id")]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Payment Number (denormalized for reporting) - رقم الدفعة
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "payment_number")]
        public string PaymentNumber { get; set; }

        #endregion

        #region Document Reference

        /// <summary>
        /// Document Type - نوع المستند
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "document_type")]
        public AllocatableDocumentType DocumentType { get; set; }

        /// <summary>
        /// Document ID - معرف المستند (AR/AP Invoice ID)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "document_id")]
        public Guid DocumentId { get; set; }

        /// <summary>
        /// Document Number (denormalized) - رقم المستند
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "document_number")]
        public string DocumentNumber { get; set; }

        #endregion

        #region Amounts in Payment Currency

        /// <summary>
        /// Allocated Amount (in payment currency) - المبلغ المخصص بعملة الدفعة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allocated_amount", DecimalDigits = 4)]
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Payment Currency - عملة الدفعة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = false, ColumnName = "payment_currency")]
        public string PaymentCurrency { get; set; }

        #endregion

        #region Amounts in Document Currency

        /// <summary>
        /// Allocated Amount in Document Currency - المبلغ المخصص بعملة المستند
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allocated_amount_doc_currency", DecimalDigits = 4)]
        public decimal AllocatedAmountDocCurrency { get; set; }

        /// <summary>
        /// Document Currency - عملة المستند
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = false, ColumnName = "document_currency")]
        public string DocumentCurrency { get; set; }

        /// <summary>
        /// Exchange Rate (Payment to Document) - سعر الصرف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "exchange_rate", DecimalDigits = 6)]
        public decimal ExchangeRate { get; set; } = 1m;

        #endregion

        #region FX Gain/Loss

        /// <summary>
        /// FX Gain/Loss (realized at allocation) - ربح/خسارة الصرف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fx_gain_loss", DecimalDigits = 4)]
        public decimal FxGainLoss { get; set; } = 0m;

        /// <summary>
        /// FX Gain/Loss GL Journal ID - معرف قيد ربح/خسارة الصرف
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fx_gl_journal_id")]
        public Guid? FxGLJournalId { get; set; }

        #endregion

        #region Document State Snapshot

        /// <summary>
        /// Document Amount Before Allocation - مبلغ المستند قبل التخصيص
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "document_balance_before", DecimalDigits = 4)]
        public decimal DocumentBalanceBefore { get; set; }

        /// <summary>
        /// Document Amount After Allocation - مبلغ المستند بعد التخصيص
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "document_balance_after", DecimalDigits = 4)]
        public decimal DocumentBalanceAfter { get; set; }

        #endregion

        #region Status & Reversal

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "status")]
        public AllocationStatus Status { get; set; } = AllocationStatus.Active;

        /// <summary>
        /// Is Reversed - هل معكوس
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_reversed")]
        public bool IsReversed { get; set; } = false;

        /// <summary>
        /// Reversal Date - تاريخ العكس
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversal_date")]
        public DateTime? ReversalDate { get; set; }

        /// <summary>
        /// Reversal Reason - سبب العكس
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "reversal_reason")]
        public string ReversalReason { get; set; }

        /// <summary>
        /// Reversed By User - معكوس بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reversed_by")]
        public string ReversedBy { get; set; }

        #endregion

        #region Dates

        /// <summary>
        /// Allocation Date - تاريخ التخصيص
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allocation_date")]
        public DateTime AllocationDate { get; set; }

        #endregion

        #region Notes

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Concurrency

        /// <summary>
        /// Row Version - إصدار الصف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "row_version", IsEnableUpdateVersionValidation = true)]
        public int RowVersion { get; set; } = 1;

        #endregion

        #region Navigation Properties (Not Persisted)

        /// <summary>
        /// Payment - الدفعة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Payment Payment { get; set; }

        #endregion
    }
}
