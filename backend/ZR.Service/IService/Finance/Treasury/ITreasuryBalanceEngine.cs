using System;
using System.Collections.Generic;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using static ZR.Model.Finance.Treasury.BalanceLedger;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Treasury Balance Engine Interface - واجهة محرك رصيد الخزينة
    /// Centralized balance calculations, projections, and reconciliations (Phase 3.1)
    /// </summary>
    public interface ITreasuryBalanceEngine
    {
        #region Real-Time Balance Queries

        /// <summary>
        /// Get current balance for bank account - الحصول على الرصيد الحالي للحساب البنكي
        /// </summary>
        BalanceInfo GetBankAccountBalance(Guid bankAccountId);

        /// <summary>
        /// Get current balance for cash account - الحصول على الرصيد الحالي لحساب النقد
        /// </summary>
        BalanceInfo GetCashAccountBalance(Guid cashAccountId);

        /// <summary>
        /// Get available balance (accounting for pending transactions) - الحصول على الرصيد المتاح
        /// </summary>
        decimal GetAvailableBalance(Guid accountId, bool isBankAccount);

        /// <summary>
        /// Get total treasury balance across all accounts - إجمالي رصيد الخزينة
        /// </summary>
        TreasuryBalanceSummary GetTotalTreasuryBalance(long? companyId = null);

        #endregion

        #region Balance Projections

        /// <summary>
        /// Project cash flow for a date range - توقع التدفق النقدي لفترة زمنية
        /// </summary>
        CashFlowProjection ProjectCashFlow(Guid accountId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get projected balance at a future date - الحصول على الرصيد المتوقع في تاريخ مستقبلي
        /// </summary>
        decimal GetProjectedBalance(Guid accountId, DateTime targetDate);

        /// <summary>
        /// Get consolidated cash flow projection for all accounts - توقع التدفق النقدي الموحد
        /// </summary>
        ConsolidatedCashFlowProjection GetConsolidatedCashFlowProjection(
            long? companyId, DateTime fromDate, DateTime toDate);

        #endregion

        #region Balance Reconciliation

        /// <summary>
        /// Calculate book balance vs statement balance difference - حساب الفرق بين الرصيد الدفتري ورصيد الكشف
        /// </summary>
        ReconciliationDifference CalculateReconciliationDifference(
            Guid bankAccountId, DateTime reconciliationDate);

        /// <summary>
        /// Get unreconciled transactions - الحصول على المعاملات غير المسواة
        /// </summary>
        List<UnreconciledTransaction> GetUnreconciledTransactions(Guid bankAccountId, DateTime? asOfDate = null);

        /// <summary>
        /// Validate reconciliation completeness - التحقق من اكتمال التسوية
        /// </summary>
        ReconciliationValidationResult ValidateReconciliation(Guid reconciliationId);

        #endregion

        #region Balance Adjustments

        /// <summary>
        /// Apply balance adjustment with audit trail - تطبيق تعديل الرصيد مع سجل التدقيق
        /// </summary>
        BalanceAdjustmentResult AdjustBalance(
            Guid accountId, bool isBankAccount, decimal amount,
            string reason, string userId, Guid? glJournalId = null);

        /// <summary>
        /// Recalculate balance from transaction history - إعادة حساب الرصيد من تاريخ المعاملات
        /// </summary>
        BalanceRecalculationResult RecalculateBalance(Guid accountId, bool isBankAccount);

        #endregion

        #region Centralized Balance Operations (Ledger-Based)

        /// <summary>
        /// Record a balance movement in the ledger and update account balance atomically
        /// This is the ONLY way to change account balances - ensures audit trail
        /// </summary>
        BalanceOperationResult RecordBalanceMovement(BalanceMovementRequest request);

        /// <summary>
        /// Reverse a previous balance movement
        /// Creates a reversal entry and updates balance atomically
        /// </summary>
        BalanceOperationResult ReverseBalanceMovement(Guid originalLedgerEntryId, string reason, string userId);

        /// <summary>
        /// Reserve available balance (for pending transactions like cheques)
        /// </summary>
        BalanceOperationResult ReserveAvailableBalance(Guid accountId, bool isBankAccount, decimal amount,
            Guid referenceEntityId, TreasuryEntityType referenceEntityType, string referenceNumber, string userId);

        /// <summary>
        /// Release previously reserved available balance
        /// </summary>
        BalanceOperationResult ReleaseAvailableBalance(Guid accountId, bool isBankAccount, decimal amount,
            Guid referenceEntityId, string reason, string userId);

        /// <summary>
        /// Validate sufficient balance before operation
        /// </summary>
        BalanceValidationResult ValidateSufficientBalance(Guid accountId, bool isBankAccount, decimal amount,
            bool checkAvailableBalance = true, bool allowOverdraft = false);

        /// <summary>
        /// Get ledger entries for an account
        /// </summary>
        List<BalanceLedger> GetLedgerEntries(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Verify balance integrity between ledger and stored balance
        /// </summary>
        BalanceIntegrityResult VerifyBalanceIntegrity(Guid accountId, bool isBankAccount);

        #endregion

        #region Cheque Impact

        /// <summary>
        /// Calculate impact of pending cheques on available balance - حساب تأثير الشيكات المعلقة
        /// </summary>
        ChequeImpact CalculatePendingChequeImpact(Guid bankAccountId);

        /// <summary>
        /// Get expected clearing schedule - جدول التصفية المتوقع
        /// </summary>
        List<ClearingScheduleItem> GetClearingSchedule(Guid bankAccountId, int daysAhead = 30);

        #endregion

        #region Multi-Currency

        /// <summary>
        /// Get balance in reporting currency - الحصول على الرصيد بعملة التقارير
        /// </summary>
        decimal GetBalanceInReportingCurrency(Guid accountId, bool isBankAccount, string reportingCurrency);

        /// <summary>
        /// Calculate FX gain/loss on balance - حساب ربح/خسارة الصرف على الرصيد
        /// </summary>
        FxGainLossResult CalculateFxGainLoss(Guid accountId, bool isBankAccount, decimal currentRate);

        #endregion
    }

    #region DTOs and Result Classes

    /// <summary>
    /// Balance information - معلومات الرصيد
    /// </summary>
    public class BalanceInfo
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingDebits { get; set; }
        public decimal PendingCredits { get; set; }
        public string Currency { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Treasury balance summary - ملخص رصيد الخزينة
    /// </summary>
    public class TreasuryBalanceSummary
    {
        public decimal TotalBankBalance { get; set; }
        public decimal TotalCashBalance { get; set; }
        public decimal TotalAvailableBalance { get; set; }
        public decimal TotalPendingCheques { get; set; }
        public string BaseCurrency { get; set; }
        public List<CurrencyBalance> BalancesByCurrency { get; set; } = new();
        public List<AccountBalance> BankAccounts { get; set; } = new();
        public List<AccountBalance> CashAccounts { get; set; } = new();
        public DateTime AsOfDate { get; set; }
    }

    public class CurrencyBalance
    {
        public string Currency { get; set; }
        public decimal BankBalance { get; set; }
        public decimal CashBalance { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal EquivalentInBaseCurrency { get; set; }
    }

    public class AccountBalance
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    /// <summary>
    /// Cash flow projection - توقع التدفق النقدي
    /// </summary>
    public class CashFlowProjection
    {
        public Guid AccountId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ProjectedInflows { get; set; }
        public decimal ProjectedOutflows { get; set; }
        public decimal ProjectedClosingBalance { get; set; }
        public List<DailyCashFlow> DailyProjections { get; set; } = new();
    }

    public class DailyCashFlow
    {
        public DateTime Date { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal Inflows { get; set; }
        public decimal Outflows { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<ProjectedTransaction> Transactions { get; set; } = new();
    }

    public class ProjectedTransaction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsInflow { get; set; }
        public string TransactionType { get; set; }
        public Guid? ReferenceId { get; set; }
    }

    /// <summary>
    /// Consolidated cash flow projection - توقع التدفق النقدي الموحد
    /// </summary>
    public class ConsolidatedCashFlowProjection
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalOpeningBalance { get; set; }
        public decimal TotalProjectedInflows { get; set; }
        public decimal TotalProjectedOutflows { get; set; }
        public decimal TotalProjectedClosingBalance { get; set; }
        public List<CashFlowProjection> AccountProjections { get; set; } = new();
    }

    /// <summary>
    /// Reconciliation difference - فرق التسوية
    /// </summary>
    public class ReconciliationDifference
    {
        public Guid BankAccountId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal BookBalance { get; set; }
        public decimal StatementBalance { get; set; }
        public decimal Difference { get; set; }
        public decimal OutstandingDeposits { get; set; }
        public decimal OutstandingCheques { get; set; }
        public decimal OtherAdjustments { get; set; }
        public decimal AdjustedBookBalance { get; set; }
        public bool IsReconciled => Math.Abs(AdjustedBookBalance - StatementBalance) < 0.01m;
    }

    /// <summary>
    /// Unreconciled transaction - معاملة غير مسواة
    /// </summary>
    public class UnreconciledTransaction
    {
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public bool IsDebit { get; set; }
        public int DaysOutstanding { get; set; }
    }

    /// <summary>
    /// Reconciliation validation result - نتيجة التحقق من التسوية
    /// </summary>
    public class ReconciliationValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int UnmatchedStatementLines { get; set; }
        public int UnmatchedBookTransactions { get; set; }
        public decimal TotalDifference { get; set; }
    }

    /// <summary>
    /// Balance adjustment result - نتيجة تعديل الرصيد
    /// </summary>
    public class BalanceAdjustmentResult
    {
        public bool Success { get; set; }
        public Guid? AdjustmentId { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Balance recalculation result - نتيجة إعادة حساب الرصيد
    /// </summary>
    public class BalanceRecalculationResult
    {
        public bool Success { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CalculatedBalance { get; set; }
        public decimal Discrepancy { get; set; }
        public bool BalanceWasAdjusted { get; set; }
        public int TransactionsProcessed { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Cheque impact on balance - تأثير الشيك على الرصيد
    /// </summary>
    public class ChequeImpact
    {
        public Guid BankAccountId { get; set; }
        public decimal TotalPendingOutgoing { get; set; }
        public decimal TotalPendingIncoming { get; set; }
        public decimal NetPendingImpact { get; set; }
        public int PendingOutgoingCount { get; set; }
        public int PendingIncomingCount { get; set; }
        public decimal OldestPendingAmount { get; set; }
        public DateTime? OldestPendingDate { get; set; }
    }

    /// <summary>
    /// Clearing schedule item - عنصر جدول التصفية
    /// </summary>
    public class ClearingScheduleItem
    {
        public DateTime ExpectedClearingDate { get; set; }
        public Guid ChequeId { get; set; }
        public string ChequeNumber { get; set; }
        public decimal Amount { get; set; }
        public bool IsOutgoing { get; set; }
        public decimal RunningBalance { get; set; }
    }

    /// <summary>
    /// FX gain/loss result - نتيجة ربح/خسارة الصرف
    /// </summary>
    public class FxGainLossResult
    {
        public Guid AccountId { get; set; }
        public string AccountCurrency { get; set; }
        public decimal BalanceInAccountCurrency { get; set; }
        public decimal PreviousRate { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal PreviousValueInReportingCurrency { get; set; }
        public decimal CurrentValueInReportingCurrency { get; set; }
        public decimal UnrealizedGainLoss { get; set; }
        public bool IsGain => UnrealizedGainLoss > 0;
    }

    /// <summary>
    /// Balance movement request - طلب حركة رصيد
    /// </summary>
    public class BalanceMovementRequest
    {
        public Guid AccountId { get; set; }
        public TreasuryAccountType AccountType { get; set; }
        /// <summary>
        /// Convenience property - sets AccountType to BankAccount if true, CashAccount if false
        /// </summary>
        public bool IsBankAccount
        {
            get => AccountType == TreasuryAccountType.BankAccount;
            set => AccountType = value ? TreasuryAccountType.BankAccount : TreasuryAccountType.CashAccount;
        }
        public BalanceLedgerTransactionType TransactionType { get; set; }
        public TreasuryEntityType ReferenceEntityType { get; set; }
        public Guid ReferenceEntityId { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ValueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsDebit { get; set; } // true = increase balance, false = decrease balance
        /// <summary>
        /// Debit amount (increases balance for assets) - if set, overrides Amount with IsDebit=true
        /// </summary>
        public decimal DebitAmount
        {
            get => IsDebit ? Amount : 0;
            set { Amount = value; IsDebit = true; }
        }
        /// <summary>
        /// Credit amount (decreases balance for assets) - if set, overrides Amount with IsDebit=false
        /// </summary>
        public decimal CreditAmount
        {
            get => !IsDebit ? Amount : 0;
            set { Amount = value; IsDebit = false; }
        }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; } = 1;
        public Guid? GLJournalId { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Arabic description - الوصف بالعربية
        /// </summary>
        public string DescriptionAr { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string TraceId { get; set; }
        public string RequestId { get; set; }
        public string Notes { get; set; }
        public bool UpdateAvailableBalance { get; set; } = true;
    }

    /// <summary>
    /// Balance operation result - نتيجة عملية الرصيد
    /// </summary>
    public class BalanceOperationResult
    {
        public bool Success { get; set; }
        public Guid? LedgerEntryId { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal NewBalance { get; set; }
        public decimal? PreviousAvailableBalance { get; set; }
        public decimal? NewAvailableBalance { get; set; }
        public decimal MovementAmount { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageAr { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }

    /// <summary>
    /// Balance validation result - نتيجة التحقق من الرصيد
    /// </summary>
    public class BalanceValidationResult
    {
        public bool IsValid { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal OverdraftLimit { get; set; }
        public bool IsOverdraftAllowed { get; set; }
        public decimal ShortfallAmount { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorMessageAr { get; set; }
    }

    /// <summary>
    /// Balance integrity result - نتيجة سلامة الرصيد
    /// </summary>
    public class BalanceIntegrityResult
    {
        public bool IsValid { get; set; }
        public Guid AccountId { get; set; }
        public decimal StoredBalance { get; set; }
        public decimal CalculatedBalance { get; set; }
        public decimal Discrepancy { get; set; }
        public decimal? StoredAvailableBalance { get; set; }
        public decimal? CalculatedAvailableBalance { get; set; }
        public decimal? AvailableBalanceDiscrepancy { get; set; }
        public int LedgerEntryCount { get; set; }
        public DateTime? LastLedgerEntryDate { get; set; }
        public List<string> Issues { get; set; } = new();
        public bool RequiresCorrection { get; set; }
    }

    #endregion
}
