using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.PaymentHub;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Treasury;
using static ZR.Model.Finance.Treasury.BalanceLedger;
using PaymentStatus = ZR.Model.Finance.PaymentHub.Enums.PaymentStatus;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury Balance Engine - محرك رصيد الخزينة
    /// Centralized balance calculations, projections, and reconciliations (Phase 3.1)
    ///
    /// ⚠️ REFACTORING REQUIRED ⚠️
    /// This service currently reads/writes CurrentBalance, AvailableBalance, and OpeningBalance directly
    /// from BankAccount and CashAccount models. These fields have been removed as part of making
    /// GLAccountOpeningBalance the single source of truth for all opening balances.
    ///
    /// REFACTORING PLAN:
    /// 1. Balance retrieval should query GLAccountOpeningBalance + sum of journal entries
    /// 2. Balance updates should create GL journal entries instead of direct updates
    /// 3. All balance calculations should use GL-based computation
    /// 4. AvailableBalance logic should be reimplemented using pending transaction queries
    ///
    /// Until refactored, this service will NOT function correctly with the new GL-based architecture.
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryBalanceEngine), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryBalanceEngine : ITreasuryBalanceEngine
    {
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly CashAccountRepository _cashAccountRepository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly ChequeRepository _chequeRepository;
        private readonly BankReconciliationRepository _reconciliationRepository;
        private readonly BankReconciliationLineRepository _reconciliationLineRepository;
        private readonly BalanceLedgerRepository _balanceLedgerRepository;
        private readonly ITreasuryAuditService _auditService;

        // Lock object for thread-safe balance operations
        private static readonly object _balanceLock = new object();

        public TreasuryBalanceEngine(
            BankAccountRepository bankAccountRepository,
            CashAccountRepository cashAccountRepository,
            PaymentHubRepository paymentRepository,
            ChequeRepository chequeRepository,
            BankReconciliationRepository reconciliationRepository,
            BankReconciliationLineRepository reconciliationLineRepository,
            BalanceLedgerRepository balanceLedgerRepository,
            ITreasuryAuditService auditService)
        {
            _bankAccountRepository = bankAccountRepository;
            _cashAccountRepository = cashAccountRepository;
            _paymentRepository = paymentRepository;
            _chequeRepository = chequeRepository;
            _reconciliationRepository = reconciliationRepository;
            _reconciliationLineRepository = reconciliationLineRepository;
            _balanceLedgerRepository = balanceLedgerRepository;
            _auditService = auditService;
        }

        #region Real-Time Balance Queries

        /// <summary>
        /// REQUIRES REFACTORING: Should query GLAccountOpeningBalance + sum of journal entries
        /// </summary>
        public BalanceInfo GetBankAccountBalance(Guid bankAccountId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should query GLAccountOpeningBalance + sum of journal entries
        /// </summary>
        public BalanceInfo GetCashAccountBalance(Guid cashAccountId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should query GLAccountOpeningBalance + sum of journal entries
        /// </summary>
        public decimal GetAvailableBalance(Guid accountId, bool isBankAccount)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should query GLAccountOpeningBalance + sum of journal entries for all accounts
        /// </summary>
        public TreasuryBalanceSummary GetTotalTreasuryBalance(long? companyId = null)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries for all treasury accounts. See class documentation for details.");
        }

        #endregion

        #region Balance Projections

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for projections
        /// </summary>
        public CashFlowProjection ProjectCashFlow(Guid accountId, DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for projections
        /// </summary>
        public decimal GetProjectedBalance(Guid accountId, DateTime targetDate)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for consolidated projections
        /// </summary>
        public ConsolidatedCashFlowProjection GetConsolidatedCashFlowProjection(
            long? companyId, DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        #endregion

        #region Balance Reconciliation

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for book balance
        /// </summary>
        public ReconciliationDifference CalculateReconciliationDifference(
            Guid bankAccountId, DateTime reconciliationDate)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        public List<UnreconciledTransaction> GetUnreconciledTransactions(Guid bankAccountId, DateTime? asOfDate = null)
        {
            var cutoffDate = asOfDate ?? DateTime.UtcNow;
            var result = new List<UnreconciledTransaction>();

            // Get uncleared cheques
            var chequesPredicate = Expressionable.Create<Cheque>();
            chequesPredicate = chequesPredicate.And(x => x.BankAccountId == bankAccountId);
            chequesPredicate = chequesPredicate.And(x => x.Status == ChequeStatus.Issued || x.Status == ChequeStatus.Presented);
            chequesPredicate = chequesPredicate.And(x => x.IssueDate <= cutoffDate);

            var cheques = _chequeRepository.GetList(chequesPredicate.ToExpression());
            foreach (var cheque in cheques)
            {
                result.Add(new UnreconciledTransaction
                {
                    TransactionId = cheque.Id,
                    TransactionType = "Cheque",
                    TransactionDate = cheque.IssueDate,
                    Reference = cheque.ChequeNumber,
                    Amount = cheque.Amount,
                    IsDebit = cheque.ChequeType == ChequeType.Outgoing,
                    DaysOutstanding = (cutoffDate - cheque.IssueDate).Days
                });
            }

            // Get unreconciled payments
            var paymentPredicate = Expressionable.Create<Payment>();
            paymentPredicate = paymentPredicate.And(x => x.SourceAccountId == bankAccountId || x.DestinationAccountId == bankAccountId);
            paymentPredicate = paymentPredicate.And(x => x.Status == PaymentStatus.Posted);
            paymentPredicate = paymentPredicate.And(x => x.PaymentDate <= cutoffDate);

            var payments = _paymentRepository.GetList(paymentPredicate.ToExpression());
            foreach (var payment in payments)
            {
                result.Add(new UnreconciledTransaction
                {
                    TransactionId = payment.Id,
                    TransactionType = "Payment",
                    TransactionDate = payment.PaymentDate,
                    Reference = payment.PaymentNumber,
                    Amount = payment.Amount,
                    IsDebit = payment.SourceAccountId == bankAccountId,
                    DaysOutstanding = (cutoffDate - payment.PaymentDate).Days
                });
            }

            return result.OrderByDescending(t => t.DaysOutstanding).ToList();
        }

        /// <summary>
        /// REQUIRES REFACTORING: Depends on CalculateReconciliationDifference which uses CurrentBalance
        /// </summary>
        public ReconciliationValidationResult ValidateReconciliation(Guid reconciliationId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method depends on reconciliation methods that use CurrentBalance. Must be reimplemented with GLAccountOpeningBalance. See class documentation for details.");
        }

        #endregion

        #region Balance Adjustments

        /// <summary>
        /// REQUIRES REFACTORING: Should create GL Journal Entries instead of direct balance updates
        /// </summary>
        public BalanceAdjustmentResult AdjustBalance(
            Guid accountId, bool isBankAccount, decimal amount,
            string reason, string userId, Guid? glJournalId = null)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Balance adjustments should be made through GL Journal Entries, not direct updates. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + sum of journal entries
        /// </summary>
        public BalanceRecalculationResult RecalculateBalance(Guid accountId, bool isBankAccount)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Balance recalculation should use GLAccountOpeningBalance + sum of journal entries. See class documentation for details.");
        }

        #endregion

        #region Cheque Impact

        public ChequeImpact CalculatePendingChequeImpact(Guid bankAccountId)
        {
            var result = new ChequeImpact { BankAccountId = bankAccountId };

            // Get pending outgoing cheques
            var outgoingPredicate = Expressionable.Create<Cheque>();
            outgoingPredicate = outgoingPredicate.And(x => x.BankAccountId == bankAccountId);
            outgoingPredicate = outgoingPredicate.And(x => x.ChequeType == ChequeType.Outgoing);
            outgoingPredicate = outgoingPredicate.And(x => x.Status == ChequeStatus.Issued);

            var outgoingCheques = _chequeRepository.GetList(outgoingPredicate.ToExpression());
            result.TotalPendingOutgoing = outgoingCheques.Sum(c => c.Amount);
            result.PendingOutgoingCount = outgoingCheques.Count;

            // Get pending incoming cheques
            var incomingPredicate = Expressionable.Create<Cheque>();
            incomingPredicate = incomingPredicate.And(x => x.BankAccountId == bankAccountId);
            incomingPredicate = incomingPredicate.And(x => x.ChequeType == ChequeType.Incoming);
            incomingPredicate = incomingPredicate.And(x => x.Status == ChequeStatus.Presented);

            var incomingCheques = _chequeRepository.GetList(incomingPredicate.ToExpression());
            result.TotalPendingIncoming = incomingCheques.Sum(c => c.Amount);
            result.PendingIncomingCount = incomingCheques.Count;

            result.NetPendingImpact = result.TotalPendingIncoming - result.TotalPendingOutgoing;

            // Find oldest pending
            var allPending = outgoingCheques.Concat(incomingCheques).ToList();
            if (allPending.Any())
            {
                var oldest = allPending.OrderBy(c => c.IssueDate).First();
                result.OldestPendingAmount = oldest.Amount;
                result.OldestPendingDate = oldest.IssueDate;
            }

            return result;
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for running balance calculations
        /// </summary>
        public List<ClearingScheduleItem> GetClearingSchedule(Guid bankAccountId, int daysAhead = 30)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        #endregion

        #region Multi-Currency

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for balance calculations
        /// </summary>
        public decimal GetBalanceInReportingCurrency(Guid accountId, bool isBankAccount, string reportingCurrency)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GLAccountOpeningBalance + journal entries for FX calculations
        /// </summary>
        public FxGainLossResult CalculateFxGainLoss(Guid accountId, bool isBankAccount, decimal currentRate)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: This method must be reimplemented to use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        #endregion

        #region Centralized Balance Operations (Ledger-Based)

        /// <summary>
        /// REQUIRES REFACTORING: Should create GL Journal Entries instead of direct balance updates
        /// </summary>
        public BalanceOperationResult RecordBalanceMovement(BalanceMovementRequest request)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Balance movements should be recorded through GL Journal Entries. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Helper method - will be replaced with GL-based ledger creation
        /// </summary>
        private BalanceLedger CreateLedgerEntry(BalanceMovementRequest request, decimal balanceBefore,
            decimal balanceAfter, decimal availableBalanceBefore, decimal availableBalanceAfter, string currency)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should reverse GL Journal Entries
        /// </summary>
        public BalanceOperationResult ReverseBalanceMovement(Guid originalLedgerEntryId, string reason, string userId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Reversals should be done through GL Journal Entry reversals. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GL-based available balance calculations
        /// </summary>
        public BalanceOperationResult ReserveAvailableBalance(Guid accountId, bool isBankAccount, decimal amount,
            Guid referenceEntityId, TreasuryEntityType referenceEntityType, string referenceNumber, string userId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Available balance reservations should use GL-based calculations. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should use GL-based available balance calculations
        /// </summary>
        public BalanceOperationResult ReleaseAvailableBalance(Guid accountId, bool isBankAccount, decimal amount,
            Guid referenceEntityId, string reason, string userId)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Available balance releases should use GL-based calculations. See class documentation for details.");
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should validate using GLAccountOpeningBalance + journal entries
        /// </summary>
        public BalanceValidationResult ValidateSufficientBalance(Guid accountId, bool isBankAccount, decimal amount,
            bool checkAvailableBalance = true, bool allowOverdraft = false)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Balance validation should use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        /// <summary>
        /// Get ledger entries for an account (This method can remain as-is if ledger table is independent)
        /// </summary>
        public List<BalanceLedger> GetLedgerEntries(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _balanceLedgerRepository.GetAccountLedger(accountId, fromDate, toDate);
        }

        /// <summary>
        /// REQUIRES REFACTORING: Should verify using GLAccountOpeningBalance + journal entries
        /// </summary>
        public BalanceIntegrityResult VerifyBalanceIntegrity(Guid accountId, bool isBankAccount)
        {
            throw new NotImplementedException("REFACTORING REQUIRED: Balance integrity verification should use GLAccountOpeningBalance + journal entries. See class documentation for details.");
        }

        #endregion
    }
}
