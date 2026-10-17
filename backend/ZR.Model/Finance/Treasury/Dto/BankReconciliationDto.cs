using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model;
using ZR.Model.Finance.Treasury.Enums;

namespace ZR.Model.Finance.Treasury.Dto
{
    /// <summary>
    /// Bank Reconciliation Query DTO - استعلام تسوية بنكية
    /// </summary>
    public class BankReconciliationQueryDto : PagerInfo
    {
        public Guid? Id { get; set; }
        public Guid? BankAccountId { get; set; }
        public ReconciliationStatus? Status { get; set; }
        public DateTime? PeriodStartFrom { get; set; }
        public DateTime? PeriodStartTo { get; set; }
        public long? TenantId { get; set; }
        public int? FiscalYear { get; set; }
        public int? FiscalPeriod { get; set; }
    }

    /// <summary>
    /// Create Reconciliation DTO - إنشاء تسوية
    /// </summary>
    public class CreateReconciliationDto
    {
        [Required(ErrorMessage = "معرف الحساب البنكي مطلوب")]
        public Guid BankAccountId { get; set; }

        [Required(ErrorMessage = "بداية الفترة مطلوبة")]
        public DateTime PeriodStart { get; set; }

        [Required(ErrorMessage = "نهاية الفترة مطلوبة")]
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Statement opening balance - رصيد افتتاحي للكشف
        /// </summary>
        public decimal? StatementOpeningBalance { get; set; }

        /// <summary>
        /// Statement closing balance - رصيد ختامي للكشف
        /// </summary>
        public decimal? StatementClosingBalance { get; set; }

        /// <summary>
        /// Tolerance amount for reconciliation difference
        /// </summary>
        public decimal? ToleranceAmount { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Outstanding Cheque DTO - شيك معلق
    /// </summary>
    public class OutstandingChequeDto
    {
        public Guid ChequeId { get; set; }
        public string ChequeNumber { get; set; }
        public string PayeeName { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public int DaysOutstanding { get; set; }
    }

    /// <summary>
    /// Deposit in Transit DTO - إيداع في الطريق
    /// </summary>
    public class DepositInTransitDto
    {
        public Guid PaymentId { get; set; }
        public string PaymentNumber { get; set; }
        public string CounterpartyName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Method { get; set; }
        public int DaysInTransit { get; set; }
    }

    /// <summary>
    /// Reconciliation Detail DTO - تفاصيل التسوية
    /// Enhanced with all reconciliation fields for requirements #31-35
    /// </summary>
    public class ReconciliationDetailDto
    {
        public Guid Id { get; set; }
        public string ReconciliationNumber { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string Currency { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime? ReconciliationDate { get; set; }

        // Balance fields
        public decimal StatementOpeningBalance { get; set; }
        public decimal StatementClosingBalance { get; set; }
        public decimal LedgerOpeningBalance { get; set; }
        public decimal LedgerClosingBalance { get; set; }
        public decimal AdjustedBookBalance { get; set; }

        // Outstanding items
        public decimal OutstandingCheques { get; set; }
        public int OutstandingChequesCount { get; set; }
        public decimal DepositsInTransit { get; set; }
        public int DepositsInTransitCount { get; set; }

        // Adjustments
        public decimal BankCharges { get; set; }
        public decimal BankInterest { get; set; }
        public decimal NsfCheques { get; set; }
        public decimal OtherAdjustments { get; set; }

        // Final calculations
        public decimal FinalDifference { get; set; }
        public decimal ToleranceAmount { get; set; }
        public bool IsWithinTolerance { get; set; }

        public decimal Difference => StatementClosingBalance - LedgerClosingBalance;
        public string DiscrepanciesSummary { get; set; }
        public ReconciliationStatus Status { get; set; }
        public string StatusName => Status switch
        {
            ReconciliationStatus.InProgress => "In Progress - قيد التنفيذ",
            ReconciliationStatus.Reconciled => "Reconciled - مسوى",
            ReconciliationStatus.PendingInvestigation => "Pending Investigation - قيد التحقيق",
            _ => Status.ToString()
        };

        // Matching statistics
        public int MatchedItemsCount { get; set; }
        public int UnmatchedStatementItems { get; set; }
        public int UnmatchedLedgerItems { get; set; }

        // Fiscal period (requirement #35)
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }

        // Journal references
        public Guid? BankChargesJournalId { get; set; }
        public Guid? BankInterestJournalId { get; set; }
        public Guid? OtherAdjustmentsJournalId { get; set; }

        public DateTime? ReconciledAt { get; set; }
        public string ReconciledBy { get; set; }
        public string Notes { get; set; }
        public string AttachmentPath { get; set; }
        public long? TenantId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        // Outstanding cheques list
        public List<OutstandingChequeDto> OutstandingChequesList { get; set; }
        public List<DepositInTransitDto> DepositsInTransitList { get; set; }
    }

    /// <summary>
    /// Reconciliation Result DTO - نتيجة التسوية
    /// </summary>
    public class ReconciliationResultDto
    {
        public Guid ReconciliationId { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal LedgerBalance { get; set; }
        public decimal Difference { get; set; }
        public int MatchedItems { get; set; }
        public int UnmatchedStatementItems { get; set; }
        public int UnmatchedLedgerItems { get; set; }
        public bool IsBalanced => Difference == 0;
    }

    /// <summary>
    /// Match Reconciliation Line DTO - مطابقة بند تسوية
    /// </summary>
    public class MatchReconciliationLineDto
    {
        [Required(ErrorMessage = "معرف التسوية مطلوب")]
        public Guid ReconciliationId { get; set; }

        [Required(ErrorMessage = "معرف بند كشف الحساب مطلوب")]
        public Guid BankStatementLineId { get; set; }

        [Required(ErrorMessage = "معرف الدفعة المطابقة مطلوب")]
        public Guid MatchedPaymentId { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Complete Reconciliation DTO - إكمال التسوية
    /// Enhanced with bank charges/interest for requirement #32
    /// </summary>
    public class CompleteReconciliationDto
    {
        [Required(ErrorMessage = "معرف التسوية مطلوب")]
        public Guid ReconciliationId { get; set; }

        /// <summary>
        /// Bank charges to be recorded - رسوم البنك
        /// </summary>
        public decimal? BankCharges { get; set; }

        /// <summary>
        /// Bank interest income to be recorded - فائدة البنك
        /// </summary>
        public decimal? BankInterest { get; set; }

        /// <summary>
        /// NSF (bounced) cheques from bank - شيكات مرتجعة
        /// </summary>
        public decimal? NsfCheques { get; set; }

        /// <summary>
        /// Other adjustments (positive or negative) - تعديلات أخرى
        /// </summary>
        public decimal? OtherAdjustments { get; set; }

        /// <summary>
        /// Override tolerance for completion - تجاوز حد التسامح
        /// </summary>
        public bool ForceComplete { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Update Reconciliation Adjustments DTO - تحديث تعديلات التسوية
    /// </summary>
    public class UpdateReconciliationAdjustmentsDto
    {
        [Required(ErrorMessage = "معرف التسوية مطلوب")]
        public Guid ReconciliationId { get; set; }

        public decimal? BankCharges { get; set; }
        public decimal? BankInterest { get; set; }
        public decimal? NsfCheques { get; set; }
        public decimal? OtherAdjustments { get; set; }
        public string AdjustmentDescription { get; set; }
    }
}






























