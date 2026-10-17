using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.PaymentHub.Enums;
using ZR.Repository.Finance.Treasury;
using ZR.Repository.Finance.PaymentHub;
using ZR.Service.IService.Finance.Treasury;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Service - خدمة التسوية البنكية
    /// Enhanced with tolerance validation, auto-journal creation, and outstanding cheques calculation
    /// Implements requirements #31-35
    /// </summary>
    [AppService(ServiceType = typeof(IBankReconciliationService), ServiceLifetime = LifeTime.Transient)]
    public class BankReconciliationService : BaseService<BankReconciliation>, IBankReconciliationService
    {
        private readonly BankReconciliationRepository _repository;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly BankStatementLineRepository _statementLineRepository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly ChequeRepository _chequeRepository;
        private readonly BankReconciliationLineRepository _reconciliationLineRepository;
        private readonly ITreasuryValidationService _validationService;
        private readonly ITreasuryGLIntegrationService _glIntegrationService;
        private readonly ITreasuryNumberSequenceService _numberSequenceService;
        private readonly ITreasuryAuditService _auditService;
        private readonly IGLAccountService _glAccountService;

        // Default tolerance for reconciliation difference (configurable per tenant)
        private const decimal DEFAULT_TOLERANCE = 0.01m;

        public BankReconciliationService(
            BankReconciliationRepository repository,
            BankAccountRepository bankAccountRepository,
            BankStatementLineRepository statementLineRepository,
            PaymentHubRepository paymentRepository,
            ChequeRepository chequeRepository,
            BankReconciliationLineRepository reconciliationLineRepository,
            ITreasuryValidationService validationService,
            ITreasuryGLIntegrationService glIntegrationService,
            ITreasuryNumberSequenceService numberSequenceService,
            ITreasuryAuditService auditService,
            IGLAccountService glAccountService)
        {
            _repository = repository;
            _bankAccountRepository = bankAccountRepository;
            _statementLineRepository = statementLineRepository;
            _paymentRepository = paymentRepository;
            _chequeRepository = chequeRepository;
            _reconciliationLineRepository = reconciliationLineRepository;
            _validationService = validationService;
            _glIntegrationService = glIntegrationService;
            _numberSequenceService = numberSequenceService;
            _auditService = auditService;
            _glAccountService = glAccountService;
        }

        public PagedInfo<ReconciliationDetailDto> GetList(BankReconciliationQueryDto parm)
        {
            var predicate = Expressionable.Create<BankReconciliation>();

            if (parm.TenantId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == (long)parm.TenantId.Value.GetHashCode());
            }

            if (parm.BankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == parm.BankAccountId.Value);
            }

            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            if (parm.PeriodStartFrom.HasValue)
            {
                predicate = predicate.And(x => x.PeriodStart >= parm.PeriodStartFrom.Value);
            }

            if (parm.PeriodStartTo.HasValue)
            {
                predicate = predicate.And(x => x.PeriodStart <= parm.PeriodStartTo.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            return response.Adapt<PagedInfo<ReconciliationDetailDto>>();
        }

        public ReconciliationDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<ReconciliationDetailDto>();
        }

        public BankReconciliation StartReconciliation(CreateReconciliationDto dto, string userId)
        {
            var bankAccount = _bankAccountRepository.GetById(dto.BankAccountId);
            if (bankAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                    .WithParameter("BankAccountId", dto.BankAccountId);
            }

            // Validate reconciliation period
            _validationService.ValidateCreateReconciliation(dto);

            // Get fiscal period for the reconciliation end date
            var (fiscalYear, fiscalPeriod) = _validationService.GetFiscalPeriodForDate(dto.PeriodEnd, dto.TenantId);

            // Calculate outstanding cheques
            var outstandingCheques = CalculateOutstandingCheques(dto.BankAccountId, dto.PeriodEnd);

            // Get GL-based balance for the bank account
            decimal ledgerBalance = 0;
            decimal ledgerOpeningBalance = 0;
            if (bankAccount.GLAccountId.HasValue)
            {
                var glBalance = _glAccountService.GetAccountBalance(bankAccount.GLAccountId.Value, fiscalYear, dto.PeriodEnd);
                ledgerBalance = glBalance?.CurrentBalance ?? 0;
                
                // Get opening balance at period start
                var glOpeningBalance = _glAccountService.GetAccountBalance(bankAccount.GLAccountId.Value, fiscalYear, dto.PeriodStart.AddDays(-1));
                ledgerOpeningBalance = glOpeningBalance?.CurrentBalance ?? 0;
            }

            // Generate reconciliation number
            var reconciliationNumber = _numberSequenceService.GetNextReconciliationNumber(
                dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null);

            var reconciliation = new BankReconciliation
            {
                Id = Guid.NewGuid(),
                ReconciliationNumber = reconciliationNumber,
                BankAccountId = dto.BankAccountId,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                ReconciliationDate = DateTime.Now,
                Status = ReconciliationStatus.InProgress,
                LedgerOpeningBalance = ledgerOpeningBalance,
                LedgerClosingBalance = ledgerBalance,
                AdjustedBookBalance = ledgerBalance - outstandingCheques.TotalAmount,
                StatementOpeningBalance = dto.StatementOpeningBalance ?? 0,
                StatementClosingBalance = dto.StatementClosingBalance ?? 0,
                OutstandingCheques = outstandingCheques.TotalAmount,
                OutstandingChequesCount = outstandingCheques.Count,
                DepositsInTransit = 0, // Will be calculated during reconciliation
                DepositsInTransitCount = 0,
                BankCharges = 0,
                BankInterest = 0,
                NsfCheques = 0,
                OtherAdjustments = 0,
                FinalDifference = 0, // Will be calculated
                ToleranceAmount = dto.ToleranceAmount ?? DEFAULT_TOLERANCE,
                IsWithinTolerance = true,
                Currency = bankAccount.Currency,
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Notes = dto.Notes,
                Create_time = DateTime.Now
            };

            if (!string.IsNullOrEmpty(userId))
            {
                reconciliation.Create_by = userId;
            }

            _repository.Insert(reconciliation);

            // Audit log
            _auditService.LogCreate(reconciliation, TreasuryEntityType.BankReconciliation,
                reconciliationNumber, userId, null, null);

            return reconciliation;
        }

        /// <summary>
        /// Calculate outstanding cheques for a bank account up to a specific date
        /// Requirement #33: Outstanding cheques calculation
        /// </summary>
        public (decimal TotalAmount, int Count, List<OutstandingChequeDto> Cheques) CalculateOutstandingCheques(Guid bankAccountId, DateTime asOfDate)
        {
            // Get cheques that are issued but not yet cleared as of the date
            var predicate = Expressionable.Create<Cheque>();
            predicate = predicate.And(x => x.BankAccountId == bankAccountId);
            predicate = predicate.And(x => x.IssueDate <= asOfDate);
            predicate = predicate.And(x => x.Status == ChequeStatus.Issued ||
                                          x.Status == ChequeStatus.Presented);
            predicate = predicate.And(x => x.ChequeType == ChequeType.Outgoing);

            var outstandingCheques = _chequeRepository.GetList(predicate.ToExpression());

            var chequeList = outstandingCheques.Select(c => new OutstandingChequeDto
            {
                ChequeId = c.Id,
                ChequeNumber = c.ChequeNumber,
                PayeeName = c.PayeeName,
                Amount = c.Amount,
                IssueDate = c.IssueDate,
                DueDate = c.DueDate,
                Status = c.Status.ToString(),
                DaysOutstanding = (int)(asOfDate - c.IssueDate).TotalDays
            }).ToList();

            return (chequeList.Sum(c => c.Amount), chequeList.Count, chequeList);
        }

        /// <summary>
        /// Calculate deposits in transit for a bank account up to a specific date
        /// </summary>
        public (decimal TotalAmount, int Count) CalculateDepositsInTransit(Guid bankAccountId, DateTime asOfDate)
        {
            // Get receipts that are posted but not yet reflected in bank statement
            var predicate = Expressionable.Create<Payment>();
            predicate = predicate.And(x => x.DestinationAccountId == bankAccountId);
            predicate = predicate.And(x => x.PaymentType == Model.Finance.PaymentHub.Enums.PaymentType.CustomerReceipt);
            predicate = predicate.And(x => x.Status == Model.Finance.PaymentHub.Enums.PaymentStatus.Posted);
            predicate = predicate.And(x => x.PaymentDate <= asOfDate);
            // TODO: Add logic to exclude payments already matched in reconciliation

            var deposits = _paymentRepository.GetList(predicate.ToExpression());
            return (deposits.Sum(d => d.Amount), deposits.Count);
        }

        public int AutoMatch(Guid reconciliationId)
        {
            // TODO: Implement auto-matching logic
            return 0;
        }

        public int MatchLine(MatchReconciliationLineDto dto, string userId)
        {
            // TODO: Implement line matching
            return 0;
        }

        public ReconciliationResultDto Complete(CompleteReconciliationDto dto, string userId)
        {
            var reconciliation = _repository.GetById(dto.ReconciliationId);
            if (reconciliation == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_NOT_FOUND)
                    .WithParameter("ReconciliationId", dto.ReconciliationId);
            }

            // Validate reconciliation status
            _validationService.ValidateCompleteReconciliation(reconciliation);

            // Calculate final difference
            CalculateFinalDifference(reconciliation);

            // Requirement #31: Prevent closing if difference > tolerance
            _validationService.ValidateReconciliationTolerance(reconciliation, reconciliation.ToleranceAmount);

            // Update bank charges/interest from DTO if provided
            if (dto.BankCharges.HasValue && dto.BankCharges.Value != reconciliation.BankCharges)
            {
                reconciliation.BankCharges = dto.BankCharges.Value;
            }
            if (dto.BankInterest.HasValue && dto.BankInterest.Value != reconciliation.BankInterest)
            {
                reconciliation.BankInterest = dto.BankInterest.Value;
            }
            if (dto.NsfCheques.HasValue && dto.NsfCheques.Value != reconciliation.NsfCheques)
            {
                reconciliation.NsfCheques = dto.NsfCheques.Value;
            }
            if (dto.OtherAdjustments.HasValue)
            {
                reconciliation.OtherAdjustments = dto.OtherAdjustments.Value;
            }

            // Requirement #32: Auto-create journal for bank charges/interest
            CreateReconciliationJournals(reconciliation, userId);

            // Recalculate after adjustments
            CalculateFinalDifference(reconciliation);

            // Final tolerance check after adjustments
            reconciliation.IsWithinTolerance = Math.Abs(reconciliation.FinalDifference) <= reconciliation.ToleranceAmount;

            reconciliation.Status = ReconciliationStatus.Reconciled;
            reconciliation.ReconciledAt = DateTime.Now;
            reconciliation.ReconciledBy = userId;
            reconciliation.Notes = dto.Notes;

            _repository.Update(reconciliation);

            // Update bank account reconciliation date
            var bankAccount = _bankAccountRepository.GetById(reconciliation.BankAccountId);
            if (bankAccount != null)
            {
                bankAccount.ReconciliationDate = reconciliation.ReconciledAt;
                // Note: Balance updates are handled through GL journal entries created by CreateReconciliationJournals
                // No need to update CurrentBalance directly as it's calculated from GLAccountOpeningBalance + journal entries
                _bankAccountRepository.Update(bankAccount);
            }

            // Audit log
            _auditService.LogStatusChange(reconciliation.Id, TreasuryEntityType.BankReconciliation,
                reconciliation.ReconciliationNumber, ReconciliationStatus.InProgress.ToString(),
                ReconciliationStatus.Reconciled.ToString(), userId);

            return GetReport(dto.ReconciliationId);
        }

        /// <summary>
        /// Calculate final difference between adjusted book balance and statement balance
        /// </summary>
        private void CalculateFinalDifference(BankReconciliation reconciliation)
        {
            // Adjusted Book Balance = Ledger Balance - Outstanding Cheques + Deposits in Transit
            reconciliation.AdjustedBookBalance = reconciliation.LedgerClosingBalance
                - reconciliation.OutstandingCheques
                + reconciliation.DepositsInTransit;

            // Adjusted Statement Balance = Statement Balance - Bank Charges + Bank Interest - NSF Cheques + Other Adjustments
            var adjustedStatementBalance = reconciliation.StatementClosingBalance
                - reconciliation.BankCharges
                + reconciliation.BankInterest
                - reconciliation.NsfCheques
                + reconciliation.OtherAdjustments;

            // Final Difference
            reconciliation.FinalDifference = adjustedStatementBalance - reconciliation.AdjustedBookBalance;
        }

        /// <summary>
        /// Requirement #32: Auto-create journals for bank charges/interest
        /// </summary>
        private void CreateReconciliationJournals(BankReconciliation reconciliation, string userId)
        {
            var config = _glIntegrationService.GetGLAccountConfig(null);
            var bankAccount = _bankAccountRepository.GetById(reconciliation.BankAccountId);

            // Create journal for bank charges if > 0
            if (reconciliation.BankCharges > 0)
            {
                var chargesJournal = _glIntegrationService.CreateReconciliationAdjustmentJournal(
                    reconciliation,
                    -reconciliation.BankCharges, // Negative because it reduces bank balance
                    $"Bank charges for period {reconciliation.PeriodStart:yyyy-MM-dd} to {reconciliation.PeriodEnd:yyyy-MM-dd}",
                    userId);
                reconciliation.BankChargesJournalId = chargesJournal.Id;
            }

            // Create journal for bank interest if > 0
            if (reconciliation.BankInterest > 0)
            {
                var interestJournal = _glIntegrationService.CreateReconciliationAdjustmentJournal(
                    reconciliation,
                    reconciliation.BankInterest, // Positive because it increases bank balance
                    $"Bank interest income for period {reconciliation.PeriodStart:yyyy-MM-dd} to {reconciliation.PeriodEnd:yyyy-MM-dd}",
                    userId);
                reconciliation.BankInterestJournalId = interestJournal.Id;
            }

            // Create journal for NSF cheques if > 0
            if (reconciliation.NsfCheques > 0)
            {
                var nsfJournal = _glIntegrationService.CreateReconciliationAdjustmentJournal(
                    reconciliation,
                    -reconciliation.NsfCheques, // Negative because it reduces bank balance
                    $"NSF cheques returned for period {reconciliation.PeriodStart:yyyy-MM-dd} to {reconciliation.PeriodEnd:yyyy-MM-dd}",
                    userId);
                // Store in OtherAdjustmentsJournalId or add a separate NsfChequesJournalId
            }

            // Create journal for other adjustments if != 0
            if (reconciliation.OtherAdjustments != 0)
            {
                var otherJournal = _glIntegrationService.CreateReconciliationAdjustmentJournal(
                    reconciliation,
                    reconciliation.OtherAdjustments,
                    $"Other adjustments for period {reconciliation.PeriodStart:yyyy-MM-dd} to {reconciliation.PeriodEnd:yyyy-MM-dd}",
                    userId);
                reconciliation.OtherAdjustmentsJournalId = otherJournal.Id;
            }
        }

        /// <summary>
        /// Update reconciliation with bank charges (requirement #32)
        /// </summary>
        public int UpdateBankCharges(Guid reconciliationId, decimal bankCharges, string userId)
        {
            var reconciliation = _repository.GetById(reconciliationId);
            if (reconciliation == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_NOT_FOUND)
                    .WithParameter("ReconciliationId", reconciliationId);
            }

            if (reconciliation.Status != ReconciliationStatus.InProgress)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_STATUS_INVALID)
                    .WithParameter("Status", reconciliation.Status.ToString());
            }

            var oldCharges = reconciliation.BankCharges;
            reconciliation.BankCharges = bankCharges;
            CalculateFinalDifference(reconciliation);
            reconciliation.Update_time = DateTime.Now;

            _auditService.LogBalanceAdjustment(reconciliationId, TreasuryEntityType.BankReconciliation,
                reconciliation.ReconciliationNumber, oldCharges, bankCharges,
                reconciliation.Currency, userId, null, null, "Bank charges updated");

            return _repository.Update(reconciliation);
        }

        /// <summary>
        /// Update reconciliation with bank interest (requirement #32)
        /// </summary>
        public int UpdateBankInterest(Guid reconciliationId, decimal bankInterest, string userId)
        {
            var reconciliation = _repository.GetById(reconciliationId);
            if (reconciliation == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_NOT_FOUND)
                    .WithParameter("ReconciliationId", reconciliationId);
            }

            if (reconciliation.Status != ReconciliationStatus.InProgress)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_RECONCILIATION_STATUS_INVALID)
                    .WithParameter("Status", reconciliation.Status.ToString());
            }

            var oldInterest = reconciliation.BankInterest;
            reconciliation.BankInterest = bankInterest;
            CalculateFinalDifference(reconciliation);
            reconciliation.Update_time = DateTime.Now;

            _auditService.LogBalanceAdjustment(reconciliationId, TreasuryEntityType.BankReconciliation,
                reconciliation.ReconciliationNumber, oldInterest, bankInterest,
                reconciliation.Currency, userId, null, null, "Bank interest updated");

            return _repository.Update(reconciliation);
        }

        public ReconciliationResultDto GetReport(Guid reconciliationId)
        {
            var reconciliation = _repository.GetById(reconciliationId);
            if (reconciliation == null)
            {
                return null;
            }

            return new ReconciliationResultDto
            {
                ReconciliationId = reconciliation.Id,
                StatementBalance = reconciliation.StatementClosingBalance,
                LedgerBalance = reconciliation.LedgerClosingBalance,
                Difference = reconciliation.StatementClosingBalance - reconciliation.LedgerClosingBalance,
                MatchedItems = 0, // TODO: Calculate from reconciliation lines
                UnmatchedStatementItems = 0,
                UnmatchedLedgerItems = 0
            };
        }
    }
}















