using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.PaymentHub;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Repository.Finance.PaymentHub;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Treasury;
using PaymentMethod = ZR.Model.Finance.PaymentHub.Enums.PaymentMethod;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Cheque Service - خدمة الشيك
    /// Enhanced with TransactionScope, Validation, GL Integration, Balance Engine, and Status History
    /// Full lifecycle: Issue → Present → Clear/Bounce → Cancel
    /// </summary>
    [AppService(ServiceType = typeof(IChequeService), ServiceLifetime = LifeTime.Transient)]
    public class ChequeService : BaseService<Cheque>, IChequeService
    {
        private readonly ChequeRepository _repository;
        private readonly ChequeStatusHistoryRepository _historyRepository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly ITreasuryValidationService _validationService;
        private readonly ITreasuryGLIntegrationService _glIntegrationService;
        private readonly ITreasuryBalanceEngine _balanceEngine;
        private readonly ITreasuryAuditService _auditService;

        public ChequeService(
            ChequeRepository repository,
            ChequeStatusHistoryRepository historyRepository,
            PaymentHubRepository paymentRepository,
            BankAccountRepository bankAccountRepository,
            ITreasuryValidationService validationService,
            ITreasuryGLIntegrationService glIntegrationService,
            ITreasuryBalanceEngine balanceEngine,
            ITreasuryAuditService auditService)
        {
            _repository = repository;
            _historyRepository = historyRepository;
            _paymentRepository = paymentRepository;
            _bankAccountRepository = bankAccountRepository;
            _validationService = validationService;
            _glIntegrationService = glIntegrationService;
            _balanceEngine = balanceEngine;
            _auditService = auditService;
        }

        public PagedInfo<ChequeListDto> GetList(ChequeQueryDto parm)
        {
            var predicate = Expressionable.Create<Cheque>();

            if (parm.CompanyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == parm.CompanyId.Value);
            }

            if (!string.IsNullOrEmpty(parm.ChequeNumber))
            {
                predicate = predicate.And(x => x.ChequeNumber.Contains(parm.ChequeNumber));
            }

            if (parm.BankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == parm.BankAccountId.Value);
            }

            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            if (parm.IssueDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.IssueDate >= parm.IssueDateFrom.Value);
            }

            if (parm.IssueDateTo.HasValue)
            {
                predicate = predicate.And(x => x.IssueDate <= parm.IssueDateTo.Value);
            }

            if (parm.DueDateFrom.HasValue)
            {
                predicate = predicate.And(x => x.DueDate >= parm.DueDateFrom.Value);
            }

            if (parm.DueDateTo.HasValue)
            {
                predicate = predicate.And(x => x.DueDate <= parm.DueDateTo.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            var result = response.Adapt<PagedInfo<ChequeListDto>>();

            // Populate BankAccountName for each cheque
            if (result.Result != null && result.Result.Count > 0)
            {
                var bankAccountIds = result.Result.Select(c => c.BankAccountId).Distinct().ToList();
                var bankAccounts = _bankAccountRepository.GetList(x => bankAccountIds.Contains(x.Id));
                var bankAccountDict = bankAccounts.ToDictionary(b => b.Id, b => b.BankNameEn ?? b.BankNameAr ?? b.AccountNumber);

                foreach (var dto in result.Result)
                {
                    if (bankAccountDict.TryGetValue(dto.BankAccountId, out var bankName))
                    {
                        dto.BankAccountName = bankName;
                    }
                }
            }

            return result;
        }

        public ChequeDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }

            var dto = entity.Adapt<ChequeDetailDto>();

            // Get bank account name
            var bankAccount = _bankAccountRepository.GetById(entity.BankAccountId);
            if (bankAccount != null)
            {
                dto.BankAccountName = bankAccount.BankNameEn ?? bankAccount.BankNameAr;
            }

            // Get related payment number
            if (entity.RelatedPaymentId.HasValue)
            {
                var payment = _paymentRepository.GetById(entity.RelatedPaymentId.Value);
                if (payment != null)
                {
                    dto.RelatedPaymentNumber = payment.PaymentNumber;
                }
            }

            return dto;
        }

        /// <summary>
        /// Create cheque - إنشاء شيك
        /// GL Journal IS created here for proper accounting (Issue stage)
        /// </summary>
        public Cheque Create(CreateChequeDto dto, string userId)
        {
            // Validate input
            _validationService.ValidateCreateCheque(dto);

            Cheque entity = null;

            // Use TransactionScope for atomic operation
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    entity = dto.Adapt<Cheque>();
                    entity.Id = Guid.NewGuid();
                    entity.Status = ChequeStatus.Issued;
                    entity.Create_time = DateTime.Now;
                    entity.Update_time = DateTime.Now;

                    if (Guid.TryParse(userId, out Guid userGuid))
                    {
                        entity.Create_by = userGuid.ToString();
                        entity.Update_by = userGuid.ToString();
                    }

                    _repository.Insert(entity);

                    // For outgoing cheques, track pending amount from issuance
                    if (entity.ChequeType == ChequeType.Outgoing)
                    {
                        var bankAccount = _bankAccountRepository.GetById(entity.BankAccountId);
                        if (bankAccount != null)
                        {
                            bankAccount.PendingChequesAmount += entity.Amount;
                            bankAccount.Update_time = DateTime.Now;
                            bankAccount.Update_by = userId;
                            _bankAccountRepository.Update(bankAccount);
                            entity.AvailableBalanceAdjusted = true;
                        }
                    }

                    // Create GL Journal for issued cheque (both outgoing and incoming)
                    var journal = _glIntegrationService.CreateChequeIssuedJournal(entity, userId);
                    if (journal != null)
                    {
                        entity.IssuanceJournalId = journal.Id;
                        _repository.Update(entity);
                    }

                    // Record status history for new cheque (no previous status)
                    RecordStatusChange(
                        entity,
                        ChequeStatus.Issued, // Using Issued as "from" for new cheques
                        ChequeStatus.Issued,
                        userId,
                        notes: "Cheque created / تم إنشاء الشيك",
                        glJournalId: journal?.Id);

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_CREATE_FAILED)
                        .WithParameter("Error", ex.Message ?? string.Empty);
                }
            }

            return entity;
        }

        public int Update(UpdateChequeDto dto, string userId)
        {
            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", dto.Id);
            }

            // Validate update
            _validationService.ValidateUpdateCheque(dto, entity);

            entity.ChequeNumber = dto.ChequeNumber;
            entity.BankAccountId = dto.BankAccountId;
            entity.PayeeName = dto.PayeeName;
            entity.IssueDate = dto.IssueDate;
            entity.DueDate = dto.DueDate;
            entity.Amount = dto.Amount;
            entity.RelatedPaymentId = dto.RelatedPaymentId;
            entity.Notes = dto.Notes;
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Update_by = userGuid.ToString();
            }

            return _repository.Update(entity);
        }

        public int Delete(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var cheque = _repository.GetById(id);
                if (cheque != null)
                {
                    _validationService.ValidateDeleteCheque(cheque);
                }
            }

            return _repository.Delete(ids);
        }

        /// <summary>
        /// Issue cheque from payment - إصدار شيك من دفعة
        /// GL Journal IS created here for proper accounting (Issue stage)
        /// </summary>
        public Cheque IssueCheque(Guid paymentId, CreateChequeDto dto, string userId)
        {
            var payment = _paymentRepository.GetById(paymentId);
            if (payment == null)
            {
                throw new AppException(ExceptionCode.FINANCE_PAYMENT_NOT_FOUND)
                    .WithParameter("PaymentId", paymentId);
            }

            Cheque cheque = null;

            // Use TransactionScope for atomic operation
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    // Validate
                    _validationService.ValidateCreateCheque(dto);

                    // Create cheque
                    cheque = dto.Adapt<Cheque>();
                    cheque.Id = Guid.NewGuid();
                    cheque.Status = ChequeStatus.Issued;
                    cheque.RelatedPaymentId = paymentId;
                    cheque.Create_time = DateTime.Now;
                    cheque.Update_time = DateTime.Now;

                    if (Guid.TryParse(userId, out Guid userGuid))
                    {
                        cheque.Create_by = userGuid.ToString();
                        cheque.Update_by = userGuid.ToString();
                    }

                    _repository.Insert(cheque);

                    // For outgoing cheques, track pending amount from issuance
                    if (cheque.ChequeType == ChequeType.Outgoing)
                    {
                        var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);
                        if (bankAccount != null)
                        {
                            bankAccount.PendingChequesAmount += cheque.Amount;
                            bankAccount.Update_time = DateTime.Now;
                            bankAccount.Update_by = userId;
                            _bankAccountRepository.Update(bankAccount);
                            cheque.AvailableBalanceAdjusted = true;
                        }
                    }

                    // Create GL Journal for issued cheque (both outgoing and incoming)
                    var journal = _glIntegrationService.CreateChequeIssuedJournal(cheque, userId);
                    if (journal != null)
                    {
                        cheque.IssuanceJournalId = journal.Id;
                        _repository.Update(cheque);
                    }

                    // Update payment method
                    payment.PaymentMethod = PaymentMethod.Cheque;
                    payment.ReferenceNumber = cheque.ChequeNumber;
                    _paymentRepository.Update(payment);

                    // Record status history for issued cheque from payment
                    RecordStatusChange(
                        cheque,
                        ChequeStatus.Issued, // Using Issued as "from" for new cheques
                        ChequeStatus.Issued,
                        userId,
                        notes: $"Cheque issued from payment {payment.PaymentNumber} / تم إصدار الشيك من الدفعة",
                        glJournalId: journal?.Id);

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_ISSUE_FAILED)
                        .WithParameter("Error", ex.Message ?? string.Empty);
                }
            }

            return cheque;
        }

        /// <summary>
        /// Present cheque for clearing - تقديم الشيك للتصفية
        /// This changes status to Presented (cheque submitted to bank)
        /// NO GL Journal created here - GL is created when cheque is cleared
        /// </summary>
        public int Present(Guid chequeId, string userId)
        {
            var cheque = _repository.GetById(chequeId);
            if (cheque == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", chequeId);
            }

            // Validate status transition - only Issued cheques can be presented
            _validationService.ValidateChequeStatusChange(cheque, ChequeStatus.Presented);

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var previousStatus = cheque.Status;

                // Update cheque status to Presented
                cheque.Status = ChequeStatus.Presented;
                cheque.PresentedDate = DateTime.UtcNow;
                cheque.Update_time = DateTime.Now;
                cheque.Update_by = userId;

                // NO pending amount update here - already tracked when cheque was issued

                _repository.Update(cheque);

                // Log status change
                _auditService.LogStatusChange(
                    chequeId,
                    TreasuryEntityType.Cheque,
                    cheque.ChequeNumber,
                    previousStatus.ToString(),
                    ChequeStatus.Presented.ToString(),
                    userId);

                // Record status history
                RecordStatusChange(
                    cheque,
                    previousStatus,
                    ChequeStatus.Presented,
                    userId,
                    notes: "Cheque presented to bank for clearing / تم تقديم الشيك للبنك للتصفية");

                scope.Complete();
            }

            return 1;
        }

        /// <summary>
        /// Mark cheque as presented to bank (OBSOLETE - merged into Present() method)
        /// Kept for backward compatibility - redirects to Present()
        /// </summary>
        [Obsolete("Use Present() method instead. This method is kept for backward compatibility only.")]
        public int MarkPresented(Guid chequeId, string userId)
        {
            // Redirect to Present() method which now handles the Presented status
            return Present(chequeId, userId);
        }

        /// <summary>
        /// Clear cheque - تصفية الشيك
        /// For outgoing cheques: reduces CurrentBalance, releases pending amount
        /// For incoming cheques: increases CurrentBalance
        /// </summary>
        public int Clear(Guid chequeId, string userId)
        {
            var cheque = _repository.GetById(chequeId);
            if (cheque == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", chequeId);
            }

            // Validate status transition - only Presented cheques can be cleared
            _validationService.ValidateChequeStatusChange(cheque, ChequeStatus.Cleared);

            // Use TransactionScope for atomic operation
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var previousStatus = cheque.Status;
                var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);

                if (bankAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                        .WithParameter("BankAccountId", cheque.BankAccountId);
                }

                // Update cheque status
                cheque.Status = ChequeStatus.Cleared;
                cheque.ClearedDate = DateTime.UtcNow;
                cheque.Update_time = DateTime.Now;
                cheque.Update_by = userId;

                // Release pending cheques amount for outgoing cheques
                // Note: Balance Engine is not yet implemented, so we update bank account directly
                // The GL Journal (created below) handles the actual accounting entries
                if (cheque.ChequeType == ChequeType.Outgoing && cheque.AvailableBalanceAdjusted)
                {
                    // Release the pending amount that was reserved when cheque was issued
                    bankAccount.PendingChequesAmount = Math.Max(0, bankAccount.PendingChequesAmount - cheque.Amount);
                    bankAccount.Update_time = DateTime.Now;
                    bankAccount.Update_by = userId;
                    _bankAccountRepository.Update(bankAccount);
                }

                _repository.Update(cheque);

                // Create GL Journal for cheque clearing
                var journal = _glIntegrationService.CreateChequeClearedJournal(cheque, userId);
                if (journal != null)
                {
                    cheque.ClearingJournalId = journal.Id;
                    _repository.Update(cheque);
                }

                // Log status change
                _auditService.LogStatusChange(
                    chequeId,
                    TreasuryEntityType.Cheque,
                    cheque.ChequeNumber,
                    previousStatus.ToString(),
                    ChequeStatus.Cleared.ToString(),
                    userId);

                // Record status history
                RecordStatusChange(
                    cheque,
                    previousStatus,
                    ChequeStatus.Cleared,
                    userId,
                    notes: "Cheque cleared successfully / تمت تصفية الشيك بنجاح",
                    glJournalId: journal?.Id);

                scope.Complete();
            }

            return 1;
        }

        public int Bounce(Guid chequeId, string userId, string reason)
        {
            return BounceWithFees(chequeId, userId, reason, 0);
        }

        /// <summary>
        /// Bounce cheque with bank fees - إرجاع الشيك مع رسوم البنك
        /// Creates GL Journal for bounced cheque reversal
        /// </summary>
        public int BounceWithFees(Guid chequeId, string userId, string reason, decimal bankFees)
        {
            var cheque = _repository.GetById(chequeId);
            if (cheque == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", chequeId);
            }

            // Validate status transition
            _validationService.ValidateChequeStatusChange(cheque, ChequeStatus.Bounced);

            // Use TransactionScope for atomic operation
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var previousStatus = cheque.Status;
                var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);

                if (bankAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                        .WithParameter("BankAccountId", cheque.BankAccountId);
                }

                // Update cheque status and bounce details
                cheque.Status = ChequeStatus.Bounced;
                cheque.BouncedDate = DateTime.UtcNow;
                cheque.BounceReason = reason;
                cheque.BounceCount += 1;
                cheque.BankFees = bankFees;
                cheque.Update_time = DateTime.Now;
                cheque.Update_by = userId;

                // If cheque was presented, release pending amount
                if (previousStatus == ChequeStatus.Presented)
                {
                    bankAccount.PendingChequesAmount = Math.Max(0, bankAccount.PendingChequesAmount - cheque.Amount);
                    bankAccount.Update_time = DateTime.Now;
                    bankAccount.Update_by = userId;
                    _bankAccountRepository.Update(bankAccount);
                }

                _repository.Update(cheque);

                // Create GL Journal for bounced cheque (if GL accounts are configured)
                var bounceJournalId = _glIntegrationService.CreateChequeBouncedJournal(cheque, userId);
                if (bounceJournalId != Guid.Empty)
                {
                    cheque.BounceJournalId = bounceJournalId;
                }

                // Handle bank fees - create GL Journal for fees (if configured)
                if (bankFees > 0)
                {
                    var feesJournalId = _glIntegrationService.CreateBankFeesJournal(cheque, bankFees, userId);
                    if (feesJournalId != Guid.Empty)
                    {
                        cheque.BankFeesJournalId = feesJournalId;
                    }
                }

                // Update cheque with journal IDs
                _repository.Update(cheque);

                // Log status change
                _auditService.LogStatusChange(
                    chequeId,
                    TreasuryEntityType.Cheque,
                    cheque.ChequeNumber,
                    previousStatus.ToString(),
                    ChequeStatus.Bounced.ToString(),
                    userId);

                // Record status history
                RecordStatusChange(
                    cheque,
                    previousStatus,
                    ChequeStatus.Bounced,
                    userId,
                    reason: reason,
                    notes: "Cheque bounced by bank / تم ارتجاع الشيك من البنك",
                    bankFees: bankFees,
                    glJournalId: bounceJournalId != Guid.Empty ? bounceJournalId : null);

                scope.Complete();
            }

            return 1;
        }

        /// <summary>
        /// Cancel cheque - إلغاء الشيك
        /// NO GL impact since GL is not created until clearing
        /// </summary>
        public int Cancel(Guid chequeId, string userId, string reason)
        {
            var cheque = _repository.GetById(chequeId);
            if (cheque == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", chequeId);
            }

            // Validate status transition - Issued, Presented, or Bounced cheques can be cancelled
            _validationService.ValidateChequeStatusChange(cheque, ChequeStatus.Cancelled);

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var previousStatus = cheque.Status;

                // If cheque was presented or bounced (for outgoing cheques), release pending amount on bank account
                if (previousStatus == ChequeStatus.Presented ||
                    (previousStatus == ChequeStatus.Bounced && cheque.ChequeType == ChequeType.Outgoing && cheque.AvailableBalanceAdjusted))
                {
                    var bankAccount = _bankAccountRepository.GetById(cheque.BankAccountId);
                    if (bankAccount != null)
                    {
                        bankAccount.PendingChequesAmount = Math.Max(0, bankAccount.PendingChequesAmount - cheque.Amount);
                        bankAccount.Update_time = DateTime.Now;
                        bankAccount.Update_by = userId;
                        _bankAccountRepository.Update(bankAccount);
                    }
                }

                cheque.Status = ChequeStatus.Cancelled;
                cheque.CancelledDate = DateTime.UtcNow;
                cheque.CancellationReason = reason;
                cheque.Notes = string.IsNullOrEmpty(cheque.Notes)
                    ? $"Cancelled: {reason}"
                    : $"{cheque.Notes}\nCancelled: {reason}";
                cheque.Update_time = DateTime.Now;
                cheque.Update_by = userId;

                _repository.Update(cheque);

                // NO GL reversal needed - GL is not created until cheque is cleared
                // Cancellation only happens before clearing, so no GL impact

                // Log status change
                _auditService.LogStatusChange(
                    chequeId,
                    TreasuryEntityType.Cheque,
                    cheque.ChequeNumber,
                    previousStatus.ToString(),
                    ChequeStatus.Cancelled.ToString(),
                    userId);

                // Record status history
                RecordStatusChange(
                    cheque,
                    previousStatus,
                    ChequeStatus.Cancelled,
                    userId,
                    reason: reason,
                    notes: "Cheque cancelled / تم إلغاء الشيك");

                scope.Complete();
            }

            return 1;
        }

        public List<ChequeListDto> GetPendingCheques(Guid? bankAccountId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<Cheque>();
            predicate = predicate.And(x =>
                x.Status == ChequeStatus.Issued ||
                x.Status == ChequeStatus.Presented);

            if (bankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == bankAccountId.Value);
            }

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var cheques = _repository.GetList(predicate.ToExpression());
            return cheques.Adapt<List<ChequeListDto>>();
        }

        /// <summary>
        /// Get cheques due within a date range - الحصول على الشيكات المستحقة خلال نطاق تاريخ
        /// </summary>
        public List<ChequeListDto> GetChequesDueInPeriod(DateTime startDate, DateTime endDate, Guid? bankAccountId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<Cheque>();
            predicate = predicate.And(x => x.DueDate >= startDate && x.DueDate <= endDate);
            predicate = predicate.And(x =>
                x.Status == ChequeStatus.Issued ||
                x.Status == ChequeStatus.Presented);

            if (bankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == bankAccountId.Value);
            }

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var cheques = _repository.GetList(predicate.ToExpression());
            return cheques.Adapt<List<ChequeListDto>>();
        }

        /// <summary>
        /// Preview GL Journal for cheque operation - معاينة القيد المحاسبي لعملية الشيك
        /// </summary>
        public CreateJournalDto PreviewGLJournal(Guid chequeId, ChequeStatus targetStatus)
        {
            var cheque = _repository.GetById(chequeId);
            if (cheque == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CHEQUE_NOT_FOUND)
                    .WithParameter("ChequeId", chequeId);
            }

            return _glIntegrationService.PreviewChequeJournal(cheque, targetStatus);
        }

        #region Status History Methods

        /// <summary>
        /// Record status change to history table - تسجيل تغيير الحالة في جدول السجل
        /// </summary>
        private void RecordStatusChange(
            Cheque cheque,
            ChequeStatus previousStatus,
            ChequeStatus newStatus,
            string userId,
            string reason = null,
            string notes = null,
            decimal? bankFees = null,
            string bankReference = null,
            Guid? glJournalId = null)
        {
            var history = new ChequeStatusHistory
            {
                Id = Guid.NewGuid(),
                ChequeId = cheque.Id,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId,
                Reason = reason,
                Notes = notes,
                BankFees = bankFees,
                BankReference = bankReference,
                GLJournalId = glJournalId,
                IPAddress = null, // Could be passed from controller if needed
                CreatedAt = DateTime.UtcNow
            };

            _historyRepository.Insert(history);
        }

        /// <summary>
        /// Get cheque status history - الحصول على سجل تغييرات الحالة
        /// </summary>
        public List<ChequeStatusHistoryDto> GetChequeHistory(Guid chequeId)
        {
            var history = _historyRepository.GetByChequeId(chequeId);
            return history.Select(h => new ChequeStatusHistoryDto
            {
                Id = h.Id,
                ChequeId = h.ChequeId,
                PreviousStatus = (int)h.PreviousStatus,
                PreviousStatusName = GetStatusName(h.PreviousStatus, false),
                PreviousStatusNameAr = GetStatusName(h.PreviousStatus, true),
                NewStatus = (int)h.NewStatus,
                NewStatusName = GetStatusName(h.NewStatus, false),
                NewStatusNameAr = GetStatusName(h.NewStatus, true),
                ChangedAt = h.ChangedAt,
                ChangedBy = h.ChangedBy,
                Reason = h.Reason,
                Notes = h.Notes,
                BankFees = h.BankFees,
                BankReference = h.BankReference,
                GLJournalId = h.GLJournalId
            }).ToList();
        }

        /// <summary>
        /// Get cheque timeline for UI display - الحصول على الجدول الزمني للعرض
        /// </summary>
        public List<ChequeTimelineItemDto> GetChequeTimeline(Guid chequeId)
        {
            var history = _historyRepository.GetByChequeId(chequeId);
            return history.Select(h => new ChequeTimelineItemDto
            {
                Id = h.Id,
                Status = (int)h.NewStatus,
                StatusLabel = GetStatusName(h.NewStatus, false),
                StatusLabelAr = GetStatusName(h.NewStatus, true),
                StatusColor = GetStatusColor(h.NewStatus),
                StatusIcon = GetStatusIcon(h.NewStatus),
                Description = GetStatusDescription(h, false),
                DescriptionAr = GetStatusDescription(h, true),
                Timestamp = h.ChangedAt,
                UserName = h.ChangedBy,
                Reason = h.Reason,
                BankFees = h.BankFees,
                BankReference = h.BankReference,
                GLJournalId = h.GLJournalId
            }).ToList();
        }

        /// <summary>
        /// Get cheque statistics - الحصول على إحصائيات الشيكات
        /// </summary>
        public ChequeStatisticsDto GetStatistics(Guid? bankAccountId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<Cheque>();

            if (bankAccountId.HasValue)
            {
                predicate = predicate.And(x => x.BankAccountId == bankAccountId.Value);
            }

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var cheques = _repository.GetList(predicate.ToExpression());

            return new ChequeStatisticsDto
            {
                Total = new ChequeStatItem
                {
                    Count = cheques.Count,
                    Amount = cheques.Sum(c => c.Amount)
                },
                Issued = new ChequeStatItem
                {
                    Count = cheques.Count(c => c.Status == ChequeStatus.Issued),
                    Amount = cheques.Where(c => c.Status == ChequeStatus.Issued).Sum(c => c.Amount)
                },
                Presented = new ChequeStatItem
                {
                    Count = cheques.Count(c => c.Status == ChequeStatus.Presented),
                    Amount = cheques.Where(c => c.Status == ChequeStatus.Presented).Sum(c => c.Amount)
                },
                Cleared = new ChequeStatItem
                {
                    Count = cheques.Count(c => c.Status == ChequeStatus.Cleared),
                    Amount = cheques.Where(c => c.Status == ChequeStatus.Cleared).Sum(c => c.Amount)
                },
                Bounced = new ChequeStatItem
                {
                    Count = cheques.Count(c => c.Status == ChequeStatus.Bounced),
                    Amount = cheques.Where(c => c.Status == ChequeStatus.Bounced).Sum(c => c.Amount)
                },
                Cancelled = new ChequeStatItem
                {
                    Count = cheques.Count(c => c.Status == ChequeStatus.Cancelled),
                    Amount = cheques.Where(c => c.Status == ChequeStatus.Cancelled).Sum(c => c.Amount)
                }
            };
        }

        #endregion

        #region Helper Methods

        private string GetStatusName(ChequeStatus status, bool arabic)
        {
            return status switch
            {
                ChequeStatus.Issued => arabic ? "صادر" : "Issued",
                ChequeStatus.Presented => arabic ? "مقدم" : "Presented",
                ChequeStatus.Cleared => arabic ? "مقاص" : "Cleared",
                ChequeStatus.Bounced => arabic ? "مرتجع" : "Bounced",
                ChequeStatus.Cancelled => arabic ? "ملغي" : "Cancelled",
                _ => arabic ? "غير معروف" : "Unknown"
            };
        }

        private string GetStatusColor(ChequeStatus status)
        {
            return status switch
            {
                ChequeStatus.Issued => "blue",
                ChequeStatus.Presented => "orange",
                ChequeStatus.Cleared => "green",
                ChequeStatus.Bounced => "red",
                ChequeStatus.Cancelled => "gray",
                _ => "gray"
            };
        }

        private string GetStatusIcon(ChequeStatus status)
        {
            return status switch
            {
                ChequeStatus.Issued => "DocumentTextIcon",
                ChequeStatus.Presented => "BuildingLibraryIcon",
                ChequeStatus.Cleared => "CheckCircleIcon",
                ChequeStatus.Bounced => "ExclamationTriangleIcon",
                ChequeStatus.Cancelled => "XCircleIcon",
                _ => "QuestionMarkCircleIcon"
            };
        }

        private string GetStatusDescription(ChequeStatusHistory h, bool arabic)
        {
            return h.NewStatus switch
            {
                ChequeStatus.Issued => arabic
                    ? "تم إصدار الشيك"
                    : "Cheque issued",
                ChequeStatus.Presented => arabic
                    ? "تم تقديم الشيك للبنك"
                    : "Cheque presented to bank",
                ChequeStatus.Cleared => arabic
                    ? "تمت تصفية الشيك بنجاح"
                    : "Cheque cleared successfully",
                ChequeStatus.Bounced => arabic
                    ? $"تم ارتجاع الشيك: {h.Reason}"
                    : $"Cheque bounced: {h.Reason}",
                ChequeStatus.Cancelled => arabic
                    ? $"تم إلغاء الشيك: {h.Reason}"
                    : $"Cheque cancelled: {h.Reason}",
                _ => arabic ? "تغيير الحالة" : "Status changed"
            };
        }

        #endregion

        #region Cheque Number Generation

        /// <summary>
        /// Generate next cheque number - توليد رقم الشيك التالي
        /// Format: CHQ-{Type}-{BankCode}-{YYYYMMDD}-{Sequence}
        /// Example: CHQ-OUT-RAJHI-20251206-0001
        /// </summary>
        public string GenerateNextChequeNumber(Guid bankAccountId, ChequeType chequeType)
        {
            // Get bank account for bank code
            var bankAccount = _bankAccountRepository.GetById(bankAccountId);
            var bankCode = bankAccount?.BankCode?.ToUpper() ?? "BANK";

            // Limit bank code to 6 characters
            if (bankCode.Length > 6)
            {
                bankCode = bankCode.Substring(0, 6);
            }

            // Get type prefix
            var typePrefix = chequeType == ChequeType.Outgoing ? "OUT" : "IN";

            // Get today's date
            var today = DateTime.Now;
            var datePrefix = today.ToString("yyyyMMdd");

            // Build prefix for searching existing cheques
            var prefix = $"CHQ-{typePrefix}-{bankCode}-{datePrefix}-";

            // Get all cheques for today with this prefix
            var todayCheques = _repository.GetList(c =>
                c.ChequeNumber.StartsWith(prefix) &&
                c.BankAccountId == bankAccountId &&
                c.ChequeType == chequeType);

            // Find the next sequence number
            int nextSequence = 1;
            if (todayCheques.Any())
            {
                var maxSequence = todayCheques
                    .Select(c => {
                        var parts = c.ChequeNumber.Split('-');
                        if (parts.Length >= 5 && int.TryParse(parts[4], out int seq))
                        {
                            return seq;
                        }
                        return 0;
                    })
                    .Max();
                nextSequence = maxSequence + 1;
            }

            // Generate the cheque number
            return $"{prefix}{nextSequence:D4}";
        }

        #endregion
    }
}











