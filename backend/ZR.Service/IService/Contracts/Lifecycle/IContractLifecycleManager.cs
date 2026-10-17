using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.IService.Contracts.Lifecycle
{
    /// <summary>
    /// IContractLifecycleManager - Contract Lifecycle Management Interface
    /// مدير دورة حياة العقد
    /// Handles contract lifecycle events: renewal, termination, amendment, suspension
    /// </summary>
    public interface IContractLifecycleManager
    {
        #region Contract Renewal

        /// <summary>
        /// Initiate contract renewal process
        /// بدء عملية تجديد العقد
        /// </summary>
        Task<RenewalResult> InitiateRenewalAsync(RenewalRequest request);

        /// <summary>
        /// Process auto-renewal for contracts with auto-renew enabled
        /// معالجة التجديد التلقائي للعقود
        /// </summary>
        Task<AutoRenewalBatchResult> ProcessAutoRenewalsAsync(DateTime asOfDate);

        /// <summary>
        /// Get contracts eligible for renewal
        /// الحصول على العقود المؤهلة للتجديد
        /// </summary>
        Task<List<RenewalEligibility>> GetRenewalEligibleContractsAsync(int daysBeforeExpiry = 90);

        /// <summary>
        /// Calculate renewal terms based on contract history and rules
        /// حساب شروط التجديد بناءً على تاريخ العقد والقواعد
        /// </summary>
        Task<RenewalTerms> CalculateRenewalTermsAsync(Guid contractId, RenewalOptions options);

        /// <summary>
        /// Apply renewal to contract
        /// تطبيق التجديد على العقد
        /// </summary>
        Task<RenewalResult> ApplyRenewalAsync(Guid contractId, RenewalTerms terms, string userName);

        #endregion

        #region Contract Termination

        /// <summary>
        /// Initiate contract termination
        /// بدء إنهاء العقد
        /// </summary>
        Task<TerminationResult> InitiateTerminationAsync(TerminationRequest request);

        /// <summary>
        /// Calculate early termination penalties and refunds
        /// حساب غرامات الإنهاء المبكر والمبالغ المستردة
        /// </summary>
        Task<TerminationFinancials> CalculateTerminationFinancialsAsync(Guid contractId, DateTime terminationDate, TerminationType terminationType);

        /// <summary>
        /// Process termination - finalize and close contract
        /// معالجة الإنهاء - إتمام وإغلاق العقد
        /// </summary>
        Task<TerminationResult> ProcessTerminationAsync(Guid contractId, string userName);

        /// <summary>
        /// Calculate deposit settlement for termination
        /// حساب تسوية الضمان عند الإنهاء
        /// </summary>
        Task<DepositSettlement> CalculateDepositSettlementAsync(Guid contractId, DateTime terminationDate);

        /// <summary>
        /// Get termination checklist for a contract
        /// الحصول على قائمة فحص الإنهاء للعقد
        /// </summary>
        Task<TerminationChecklist> GetTerminationChecklistAsync(Guid contractId);

        #endregion

        #region Contract Amendment

        /// <summary>
        /// Create contract amendment
        /// إنشاء تعديل على العقد
        /// </summary>
        Task<AmendmentResult> CreateAmendmentAsync(AmendmentRequest request);

        /// <summary>
        /// Apply approved amendment to contract
        /// تطبيق التعديل المعتمد على العقد
        /// </summary>
        Task<AmendmentResult> ApplyAmendmentAsync(Guid amendmentId, string userName);

        /// <summary>
        /// Reverse applied amendment (if allowed)
        /// عكس التعديل المطبق (إذا كان مسموحاً)
        /// </summary>
        Task<AmendmentResult> ReverseAmendmentAsync(Guid amendmentId, string userName, string reason);

        /// <summary>
        /// Calculate amendment impact on billing
        /// حساب تأثير التعديل على الفوترة
        /// </summary>
        Task<AmendmentImpact> CalculateAmendmentImpactAsync(Guid contractId, AmendmentRequest request);

        /// <summary>
        /// Get amendment history for contract
        /// الحصول على تاريخ التعديلات للعقد
        /// </summary>
        Task<List<AmendmentHistory>> GetAmendmentHistoryAsync(Guid contractId);

        #endregion

        #region Contract Suspension

        /// <summary>
        /// Suspend contract
        /// تعليق العقد
        /// </summary>
        Task<SuspensionResult> SuspendContractAsync(SuspensionRequest request);

        /// <summary>
        /// Resume suspended contract
        /// استئناف العقد المعلق
        /// </summary>
        Task<SuspensionResult> ResumeContractAsync(Guid contractId, string userName, DateTime? resumeDate = null);

        /// <summary>
        /// Calculate billing impact of suspension
        /// حساب تأثير التعليق على الفوترة
        /// </summary>
        Task<SuspensionImpact> CalculateSuspensionImpactAsync(Guid contractId, DateTime suspensionStart, DateTime? suspensionEnd);

        #endregion

        #region Security Deposit Management

        /// <summary>
        /// Record security deposit
        /// تسجيل الضمان
        /// </summary>
        Task<DepositResult> RecordDepositAsync(DepositRequest request);

        /// <summary>
        /// Process deposit refund
        /// معالجة استرداد الضمان
        /// </summary>
        Task<DepositRefundResult> ProcessDepositRefundAsync(Guid contractId, DepositRefundRequest request);

        /// <summary>
        /// Apply deposit deduction
        /// تطبيق خصم من الضمان
        /// </summary>
        Task<DepositDeductionResult> ApplyDepositDeductionAsync(Guid contractId, DepositDeductionRequest request);

        /// <summary>
        /// Get deposit balance for contract
        /// الحصول على رصيد الضمان للعقد
        /// </summary>
        Task<DepositBalance> GetDepositBalanceAsync(Guid contractId);

        #endregion
    }

    #region Request/Response Classes

    /// <summary>
    /// Renewal request
    /// </summary>
    public class RenewalRequest
    {
        public Guid ContractId { get; set; }
        public DateTime NewEndDate { get; set; }
        public decimal? NewRentAmount { get; set; }
        public bool ApplyEscalation { get; set; } = true;
        public decimal? EscalationPercentage { get; set; }
        public bool GenerateNewBillingSchedules { get; set; } = true;
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Renewal result
    /// </summary>
    public class RenewalResult
    {
        public bool Success { get; set; }
        public Guid? NewContractId { get; set; }
        public string RenewalNumber { get; set; }
        public DateTime? NewStartDate { get; set; }
        public DateTime? NewEndDate { get; set; }
        public decimal? NewRentAmount { get; set; }
        public int BillingSchedulesGenerated { get; set; }
        public string Message { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Auto-renewal batch result
    /// </summary>
    public class AutoRenewalBatchResult
    {
        public int TotalProcessed { get; set; }
        public int SuccessfulRenewals { get; set; }
        public int FailedRenewals { get; set; }
        public List<RenewalResult> Results { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Renewal eligibility information
    /// </summary>
    public class RenewalEligibility
    {
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysToExpiry { get; set; }
        public bool AutoRenewEnabled { get; set; }
        public decimal CurrentRentAmount { get; set; }
        public decimal SuggestedRenewalAmount { get; set; }
        public decimal EscalationPercentage { get; set; }
        public bool HasUnpaidInvoices { get; set; }
        public decimal UnpaidAmount { get; set; }
        public string EligibilityStatus { get; set; } // Eligible, Pending, Blocked
        public List<string> BlockingReasons { get; set; } = new();
    }

    /// <summary>
    /// Renewal options
    /// </summary>
    public class RenewalOptions
    {
        public int RenewalDurationMonths { get; set; } = 12;
        public bool ApplyEscalation { get; set; } = true;
        public bool CopyExistingRules { get; set; } = true;
        public bool ResetBillingSchedule { get; set; } = true;
    }

    /// <summary>
    /// Calculated renewal terms
    /// </summary>
    public class RenewalTerms
    {
        public DateTime NewStartDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public int DurationMonths { get; set; }
        public decimal CurrentRentAmount { get; set; }
        public decimal NewRentAmount { get; set; }
        public decimal EscalationAmount { get; set; }
        public decimal EscalationPercentage { get; set; }
        public string Currency { get; set; }
        public bool CopyPricingRules { get; set; }
        public bool CopyEscalationRules { get; set; }
        public int EstimatedBillingPeriods { get; set; }
        public decimal EstimatedTotalValue { get; set; }
    }

    /// <summary>
    /// Termination request
    /// </summary>
    public class TerminationRequest
    {
        public Guid ContractId { get; set; }
        public DateTime TerminationDate { get; set; }
        public TerminationType TerminationType { get; set; }
        public string TerminationReason { get; set; }
        public bool WaivePenalty { get; set; }
        public string WaiverReason { get; set; }
        public bool ProcessDepositRefund { get; set; } = true;
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Termination result
    /// </summary>
    public class TerminationResult
    {
        public bool Success { get; set; }
        public ContractStatus NewStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public decimal TotalPenalty { get; set; }
        public decimal TotalRefund { get; set; }
        public decimal DepositRefund { get; set; }
        public int CancelledSchedules { get; set; }
        public string Message { get; set; }
        public TerminationFinancials Financials { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Termination financial calculations
    /// </summary>
    public class TerminationFinancials
    {
        public Guid ContractId { get; set; }
        public DateTime TerminationDate { get; set; }
        public DateTime OriginalEndDate { get; set; }
        public int RemainingDays { get; set; }
        public int RemainingMonths { get; set; }

        // Outstanding amounts
        public decimal OutstandingRent { get; set; }
        public decimal OutstandingServiceCharges { get; set; }
        public decimal OutstandingUtilities { get; set; }
        public decimal TotalOutstanding { get; set; }

        // Penalties
        public decimal EarlyTerminationPenalty { get; set; }
        public decimal PenaltyMonths { get; set; }
        public decimal PenaltyRate { get; set; }
        public bool PenaltyWaived { get; set; }
        public string PenaltyWaiverReason { get; set; }

        // Refunds
        public decimal PrepaidRentRefund { get; set; }
        public decimal ProRatedRefund { get; set; }
        public decimal SecurityDepositBalance { get; set; }
        public decimal DepositDeductions { get; set; }
        public decimal DepositRefundAmount { get; set; }

        // Net settlement
        public decimal NetAmountDue { get; set; } // Positive = customer owes, Negative = refund
        public string Currency { get; set; }

        // Breakdown
        public List<FinancialLineItem> LineItems { get; set; } = new();
    }

    /// <summary>
    /// Financial line item for settlement
    /// </summary>
    public class FinancialLineItem
    {
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Category { get; set; } // Outstanding, Penalty, Refund, Deposit
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; } // True = customer credit, False = customer owes
        public string Reference { get; set; }
    }

    /// <summary>
    /// Deposit settlement calculation
    /// </summary>
    public class DepositSettlement
    {
        public Guid ContractId { get; set; }
        public decimal OriginalDeposit { get; set; }
        public decimal AdditionalDeposits { get; set; }
        public decimal TotalDeposit { get; set; }
        public List<DepositDeduction> Deductions { get; set; } = new();
        public decimal TotalDeductions { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; }
    }

    /// <summary>
    /// Deposit deduction detail
    /// </summary>
    public class DepositDeduction
    {
        public string Description { get; set; }
        public string Category { get; set; } // Damage, Outstanding, Cleaning, Other
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public DateTime DeductionDate { get; set; }
    }

    /// <summary>
    /// Termination checklist
    /// </summary>
    public class TerminationChecklist
    {
        public Guid ContractId { get; set; }
        public List<ChecklistItem> Items { get; set; } = new();
        public bool AllItemsComplete { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Checklist item
    /// </summary>
    public class ChecklistItem
    {
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public bool IsRequired { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletedBy { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Amendment request
    /// </summary>
    public class AmendmentRequest
    {
        public Guid ContractId { get; set; }
        public AmendmentType AmendmentType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Changes { get; set; } = new();
        public bool RequiresApproval { get; set; } = true;
        public string Notes { get; set; }
        public string UserName { get; set; }

        // Common amendment fields
        public decimal? NewRentAmount { get; set; }
        public DateTime? NewEndDate { get; set; }
        public Guid? AddUnitId { get; set; }
        public Guid? RemoveUnitId { get; set; }
    }

    /// <summary>
    /// Amendment result
    /// </summary>
    public class AmendmentResult
    {
        public bool Success { get; set; }
        public Guid? AmendmentId { get; set; }
        public string AmendmentNumber { get; set; }
        public AmendmentStatus Status { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string Message { get; set; }
        public AmendmentImpact Impact { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Amendment impact calculation
    /// </summary>
    public class AmendmentImpact
    {
        public Guid ContractId { get; set; }
        public AmendmentType AmendmentType { get; set; }

        // Value changes
        public decimal CurrentValue { get; set; }
        public decimal NewValue { get; set; }
        public decimal ValueChange { get; set; }
        public decimal ValueChangePercentage { get; set; }

        // Billing impact
        public int AffectedBillingPeriods { get; set; }
        public decimal BillingAdjustmentAmount { get; set; }
        public bool RequiresBillingRegeneration { get; set; }
        public int NewBillingPeriodsToGenerate { get; set; }

        // Schedule changes
        public int SchedulesToCancel { get; set; }
        public int SchedulesToAdjust { get; set; }

        // Financial summary
        public decimal ImpactOnContractValue { get; set; }
        public string Currency { get; set; }
    }

    /// <summary>
    /// Amendment history entry
    /// </summary>
    public class AmendmentHistory
    {
        public Guid AmendmentId { get; set; }
        public string AmendmentNumber { get; set; }
        public AmendmentType AmendmentType { get; set; }
        public AmendmentStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string Description { get; set; }
        public decimal ValueBefore { get; set; }
        public decimal ValueAfter { get; set; }
        public string ChangeSummary { get; set; }
    }

    /// <summary>
    /// Suspension request
    /// </summary>
    public class SuspensionRequest
    {
        public Guid ContractId { get; set; }
        public DateTime SuspensionDate { get; set; }
        public DateTime? ExpectedResumeDate { get; set; }
        public SuspensionReason Reason { get; set; }
        public string ReasonDetails { get; set; }
        public bool ExtendContractEndDate { get; set; } = true;
        public bool SuspendBilling { get; set; } = true;
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Suspension reason
    /// </summary>
    public enum SuspensionReason
    {
        NonPayment = 1,
        ContractViolation = 2,
        MutualAgreement = 3,
        LegalDispute = 4,
        ForceMajeure = 5,
        Renovation = 6,
        CustomerRequest = 7,
        Other = 99
    }

    /// <summary>
    /// Suspension result
    /// </summary>
    public class SuspensionResult
    {
        public bool Success { get; set; }
        public ContractStatus NewStatus { get; set; }
        public DateTime? SuspensionStart { get; set; }
        public DateTime? SuspensionEnd { get; set; }
        public int SuspensionDays { get; set; }
        public DateTime? NewContractEndDate { get; set; }
        public int AffectedBillingSchedules { get; set; }
        public string Message { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Suspension billing impact
    /// </summary>
    public class SuspensionImpact
    {
        public Guid ContractId { get; set; }
        public DateTime SuspensionStart { get; set; }
        public DateTime? SuspensionEnd { get; set; }
        public int SuspensionDays { get; set; }

        // Billing impact
        public int AffectedSchedules { get; set; }
        public decimal SuspendedBillingAmount { get; set; }
        public int SchedulesToDefer { get; set; }
        public int SchedulesToCancel { get; set; }

        // Contract impact
        public DateTime CurrentEndDate { get; set; }
        public DateTime NewEndDate { get; set; }
        public int ExtendedDays { get; set; }
    }

    /// <summary>
    /// Deposit request
    /// </summary>
    public class DepositRequest
    {
        public Guid ContractId { get; set; }
        public decimal Amount { get; set; }
        public string DepositType { get; set; } // Security, Utility, Other
        public DateTime ReceivedDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Deposit result
    /// </summary>
    public class DepositResult
    {
        public bool Success { get; set; }
        public Guid? DepositId { get; set; }
        public decimal TotalDepositBalance { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Deposit refund request
    /// </summary>
    public class DepositRefundRequest
    {
        public decimal RefundAmount { get; set; }
        public DateTime RefundDate { get; set; }
        public string RefundMethod { get; set; }
        public string RefundReference { get; set; }
        public string BankAccount { get; set; }
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Deposit refund result
    /// </summary>
    public class DepositRefundResult
    {
        public bool Success { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string RefundReference { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Deposit deduction request
    /// </summary>
    public class DepositDeductionRequest
    {
        public decimal DeductionAmount { get; set; }
        public string DeductionReason { get; set; }
        public string Category { get; set; } // Damage, Outstanding, Cleaning, Penalty
        public DateTime DeductionDate { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// Deposit deduction result
    /// </summary>
    public class DepositDeductionResult
    {
        public bool Success { get; set; }
        public decimal DeductedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Deposit balance
    /// </summary>
    public class DepositBalance
    {
        public Guid ContractId { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; }
        public List<DepositTransaction> Transactions { get; set; } = new();
    }

    /// <summary>
    /// Deposit transaction
    /// </summary>
    public class DepositTransaction
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } // Deposit, Deduction, Refund
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string ProcessedBy { get; set; }
    }

    #endregion
}
