using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Line - بند تسوية بنكية
    /// Enhanced with manual review, attachments, and status tracking
    /// </summary>
    [SugarTable("bank_reconciliation_line")]
    public class BankReconciliationLine : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Line ID - معرف البند
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Reconciliation ID - معرف التسوية
        /// </summary>
        [SugarColumn(ColumnName = "reconciliation_id")]
        public Guid ReconciliationId { get; set; }

        /// <summary>
        /// Bank Statement Line ID - معرف بند كشف الحساب
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bank_statement_line_id")]
        public Guid? BankStatementLineId { get; set; }

        /// <summary>
        /// Matched Payment ID - معرف الدفعة المطابقة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "matched_payment_id")]
        public Guid? MatchedPaymentId { get; set; }

        /// <summary>
        /// Matched Cheque ID - معرف الشيك المطابق
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "matched_cheque_id")]
        public Guid? MatchedChequeId { get; set; }

        /// <summary>
        /// Line Status - حالة البند
        /// </summary>
        [SugarColumn(ColumnName = "line_status")]
        public ReconciliationLineStatus LineStatus { get; set; } = ReconciliationLineStatus.Unmatched;

        /// <summary>
        /// Match Type - نوع المطابقة (Auto/Manual)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "match_type")]
        public string MatchType { get; set; }

        /// <summary>
        /// Match Confidence - درجة الثقة في المطابقة (0-100)
        /// </summary>
        [SugarColumn(ColumnName = "match_confidence")]
        public int MatchConfidence { get; set; }

        /// <summary>
        /// Statement Amount - مبلغ الكشف
        /// </summary>
        [SugarColumn(ColumnName = "statement_amount", ColumnDataType = "decimal(18,4)")]
        public decimal StatementAmount { get; set; }

        /// <summary>
        /// Ledger Amount - مبلغ السجل
        /// </summary>
        [SugarColumn(ColumnName = "ledger_amount", ColumnDataType = "decimal(18,4)")]
        public decimal LedgerAmount { get; set; }

        /// <summary>
        /// Difference Amount - مبلغ الفرق
        /// </summary>
        [SugarColumn(ColumnName = "difference_amount", ColumnDataType = "decimal(18,4)")]
        public decimal DifferenceAmount { get; set; }

        /// <summary>
        /// Statement Date - تاريخ الكشف
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "statement_date")]
        public DateTime? StatementDate { get; set; }

        /// <summary>
        /// Statement Description - وصف الكشف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "statement_description")]
        public string StatementDescription { get; set; }

        /// <summary>
        /// Statement Reference - مرجع الكشف
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "statement_reference")]
        public string StatementReference { get; set; }

        /// <summary>
        /// Ledger Description - وصف السجل
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "ledger_description")]
        public string LedgerDescription { get; set; }

        /// <summary>
        /// Is Manual Review Required - هل تتطلب مراجعة يدوية
        /// </summary>
        [SugarColumn(ColumnName = "requires_manual_review")]
        public bool RequiresManualReview { get; set; }

        /// <summary>
        /// Review Reason - سبب المراجعة
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "review_reason")]
        public string ReviewReason { get; set; }

        /// <summary>
        /// Reviewed By - مراجع بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reviewed_by")]
        public string ReviewedBy { get; set; }

        /// <summary>
        /// Reviewed At - تاريخ المراجعة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Exclusion Reason - سبب الاستبعاد
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "exclusion_reason")]
        public string ExclusionReason { get; set; }

        /// <summary>
        /// Created Journal ID - معرف القيد المنشأ (for missing transactions)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "created_journal_id")]
        public Guid? CreatedJournalId { get; set; }

        /// <summary>
        /// Attachment Path - مسار المرفق
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "attachment_path")]
        public string AttachmentPath { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Navigation - Reconciliation
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BankReconciliation Reconciliation { get; set; }

        /// <summary>
        /// Navigation - Bank Statement Line
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BankStatementLine BankStatementLine { get; set; }

        /// <summary>
        /// Navigation - Matched Payment
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Payment MatchedPayment { get; set; }
    }
}






























