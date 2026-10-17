using System;
using SqlSugar;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation - تسوية بنكية
    /// Enhanced with tolerance check, outstanding cheques, auto-journal creation
    /// </summary>
    [SugarTable("bank_reconciliation")]
    public class BankReconciliation : SysBase
    {
        /// <summary>
        /// Override BranchID from SysBase - not used in Treasury tables
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public new long? BranchID { get; set; }

        /// <summary>
        /// Reconciliation ID - معرف التسوية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Reconciliation Number - رقم التسوية
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "reconciliation_number")]
        public string ReconciliationNumber { get; set; }

        /// <summary>
        /// Bank Account ID - معرف الحساب البنكي
        /// </summary>
        [SugarColumn(ColumnName = "bank_account_id")]
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Bank Statement ID - معرف كشف الحساب (if linked)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bank_statement_id")]
        public Guid? BankStatementId { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Reconciliation Date - تاريخ التسوية
        /// </summary>
        [SugarColumn(ColumnName = "reconciliation_date")]
        public DateTime ReconciliationDate { get; set; }

        /// <summary>
        /// Period Start - بداية الفترة
        /// </summary>
        [SugarColumn(ColumnName = "period_start")]
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Period End - نهاية الفترة
        /// </summary>
        [SugarColumn(ColumnName = "period_end")]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Statement Opening Balance - الرصيد الافتتاحي للكشف
        /// </summary>
        [SugarColumn(ColumnName = "statement_opening_balance", ColumnDataType = "decimal(18,4)")]
        public decimal StatementOpeningBalance { get; set; }

        /// <summary>
        /// Statement Closing Balance - الرصيد الختامي للكشف
        /// </summary>
        [SugarColumn(ColumnName = "statement_closing_balance", ColumnDataType = "decimal(18,4)")]
        public decimal StatementClosingBalance { get; set; }

        /// <summary>
        /// Ledger Opening Balance - الرصيد الافتتاحي للسجل
        /// </summary>
        [SugarColumn(ColumnName = "ledger_opening_balance", ColumnDataType = "decimal(18,4)")]
        public decimal LedgerOpeningBalance { get; set; }

        /// <summary>
        /// Ledger Closing Balance - الرصيد الختامي للسجل
        /// </summary>
        [SugarColumn(ColumnName = "ledger_closing_balance", ColumnDataType = "decimal(18,4)")]
        public decimal LedgerClosingBalance { get; set; }

        /// <summary>
        /// Adjusted Book Balance - الرصيد الدفتري المعدل
        /// </summary>
        [SugarColumn(ColumnName = "adjusted_book_balance", ColumnDataType = "decimal(18,4)")]
        public decimal AdjustedBookBalance { get; set; }

        /// <summary>
        /// Outstanding Cheques - الشيكات المعلقة
        /// </summary>
        [SugarColumn(ColumnName = "outstanding_cheques", ColumnDataType = "decimal(18,4)")]
        public decimal OutstandingCheques { get; set; }

        /// <summary>
        /// Outstanding Cheques Count - عدد الشيكات المعلقة
        /// </summary>
        [SugarColumn(ColumnName = "outstanding_cheques_count")]
        public int OutstandingChequesCount { get; set; }

        /// <summary>
        /// Deposits In Transit - الإيداعات في الطريق
        /// </summary>
        [SugarColumn(ColumnName = "deposits_in_transit", ColumnDataType = "decimal(18,4)")]
        public decimal DepositsInTransit { get; set; }

        /// <summary>
        /// Deposits In Transit Count - عدد الإيداعات في الطريق
        /// </summary>
        [SugarColumn(ColumnName = "deposits_in_transit_count")]
        public int DepositsInTransitCount { get; set; }

        /// <summary>
        /// Bank Charges - رسوم البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_charges", ColumnDataType = "decimal(18,4)")]
        public decimal BankCharges { get; set; }

        /// <summary>
        /// Bank Interest - فوائد البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_interest", ColumnDataType = "decimal(18,4)")]
        public decimal BankInterest { get; set; }

        /// <summary>
        /// NSF Cheques - شيكات بدون رصيد
        /// </summary>
        [SugarColumn(ColumnName = "nsf_cheques", ColumnDataType = "decimal(18,4)")]
        public decimal NsfCheques { get; set; }

        /// <summary>
        /// Other Adjustments - تعديلات أخرى
        /// </summary>
        [SugarColumn(ColumnName = "other_adjustments", ColumnDataType = "decimal(18,4)")]
        public decimal OtherAdjustments { get; set; }

        /// <summary>
        /// Final Difference - الفرق النهائي
        /// </summary>
        [SugarColumn(ColumnName = "final_difference", ColumnDataType = "decimal(18,4)")]
        public decimal FinalDifference { get; set; }

        /// <summary>
        /// Tolerance Amount - مبلغ التسامح
        /// </summary>
        [SugarColumn(ColumnName = "tolerance_amount", ColumnDataType = "decimal(18,4)")]
        public decimal ToleranceAmount { get; set; }

        /// <summary>
        /// Is Within Tolerance - هل ضمن التسامح
        /// </summary>
        [SugarColumn(ColumnName = "is_within_tolerance")]
        public bool IsWithinTolerance { get; set; }

        /// <summary>
        /// Matched Items Count - عدد البنود المتطابقة
        /// </summary>
        [SugarColumn(ColumnName = "matched_items_count")]
        public int MatchedItemsCount { get; set; }

        /// <summary>
        /// Unmatched Statement Items - عدد بنود الكشف غير المتطابقة
        /// </summary>
        [SugarColumn(ColumnName = "unmatched_statement_items")]
        public int UnmatchedStatementItems { get; set; }

        /// <summary>
        /// Unmatched Ledger Items - عدد بنود السجل غير المتطابقة
        /// </summary>
        [SugarColumn(ColumnName = "unmatched_ledger_items")]
        public int UnmatchedLedgerItems { get; set; }

        /// <summary>
        /// Discrepancies Summary - ملخص الفروقات (JSON)
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true, ColumnName = "discrepancies_summary")]
        public string DiscrepanciesSummary { get; set; }

        /// <summary>
        /// Bank Charges Journal ID - معرف قيد رسوم البنك
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bank_charges_journal_id")]
        public Guid? BankChargesJournalId { get; set; }

        /// <summary>
        /// Bank Interest Journal ID - معرف قيد فوائد البنك
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "bank_interest_journal_id")]
        public Guid? BankInterestJournalId { get; set; }

        /// <summary>
        /// Other Adjustments Journal ID - معرف قيد التعديلات الأخرى
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "other_adjustments_journal_id")]
        public Guid? OtherAdjustmentsJournalId { get; set; }

        /// <summary>
        /// Adjustment Journal ID - معرف قيد التعديلات (legacy)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "adjustment_journal_id")]
        public Guid? AdjustmentJournalId { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية (requirement #35)
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية (requirement #35)
        /// </summary>
        [SugarColumn(ColumnName = "fiscal_period")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public ReconciliationStatus Status { get; set; } = ReconciliationStatus.InProgress;

        /// <summary>
        /// Reconciled At - تاريخ إتمام التسوية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reconciled_at")]
        public DateTime? ReconciledAt { get; set; }

        /// <summary>
        /// Reconciled By - مسوى بواسطة
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reconciled_by")]
        public string ReconciledBy { get; set; }

        /// <summary>
        /// Attachment Path - مسار المرفق
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "attachment_path")]
        public string AttachmentPath { get; set; }

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
    }
}






























