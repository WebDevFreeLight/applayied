using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;

using ZR.Model.Finance.GL;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Model.Finance.Treasury.Enums;
using ZR.Model.Finance.PaymentHub;

using ZR.Repository.Finance.Treasury;
using ZR.Repository.Finance.PaymentHub;
using ZR.Service.IService.Finance.Treasury;
using ZR.Service.IService.Finance.GL;
namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Bank Account Service - خدمة الحساب البنكي
    /// </summary>
    [AppService(ServiceType = typeof(IBankAccountService), ServiceLifetime = LifeTime.Transient)]
    public class BankAccountService : BaseService<BankAccount>, IBankAccountService
    {
        private readonly BankAccountRepository _repository;
        private readonly BankReconciliationRepository _reconciliationRepository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly ChequeRepository _chequeRepository;
        private readonly ITreasuryAuditService _auditService;
        private readonly IGLAccountService _glAccountService;
        private readonly IGLAccountAutoCreator _glAccountAutoCreator;

        public BankAccountService(
            BankAccountRepository repository,
            BankReconciliationRepository reconciliationRepository,
            PaymentHubRepository paymentRepository,
            ChequeRepository chequeRepository,
            ITreasuryAuditService auditService,
            IGLAccountService glAccountService,
            IGLAccountAutoCreator glAccountAutoCreator)
        {
            _repository = repository;
            _reconciliationRepository = reconciliationRepository;
            _paymentRepository = paymentRepository;
            _chequeRepository = chequeRepository;
            _auditService = auditService;
            _glAccountService = glAccountService;
            _glAccountAutoCreator = glAccountAutoCreator;
        }

        public PagedInfo<BankAccountListDto> GetList(BankAccountQueryDto parm)
        {
            var predicate = Expressionable.Create<BankAccount>();

            // Exclude soft-deleted accounts
            predicate = predicate.And(x => !x.IsDeleted);

            if (parm.TenantId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == (long)parm.TenantId.Value.GetHashCode());
            }

            if (!string.IsNullOrEmpty(parm.BankCode))
            {
                predicate = predicate.And(x => x.BankCode.Contains(parm.BankCode));
            }

            if (!string.IsNullOrEmpty(parm.BankName))
            {
                predicate = predicate.And(x => x.BankNameEn.Contains(parm.BankName) ||
                    (x.BankNameAr != null && x.BankNameAr.Contains(parm.BankName)));
            }

            if (!string.IsNullOrEmpty(parm.AccountNumber))
            {
                predicate = predicate.And(x => x.AccountNumber.Contains(parm.AccountNumber));
            }

            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            if (!string.IsNullOrEmpty(parm.Currency))
            {
                predicate = predicate.And(x => x.Currency == parm.Currency);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            var result = response.Adapt<PagedInfo<BankAccountListDto>>();

            // Populate balance for each account from GL system
            if (result?.Result != null)
            {
                foreach (var account in result.Result)
                {
                    PopulateAccountBalance(account);
                }
            }

            return result;
        }

        public BankAccountDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<BankAccountDetailDto>();
        }

        public BankAccount Create(CreateBankAccountDto dto, string userId)
        {
            // Validate GLAccountId is provided (mandatory field)
            if (dto.GLAccountId == Guid.Empty)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED);
            }

            var companyId = dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null;
            if (AccountNumberExists(dto.AccountNumber, null, companyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NUMBER_DUPLICATE)
                    .WithParameter("AccountNumber", dto.AccountNumber);
            }

            var entity = dto.Adapt<BankAccount>();
            entity.Id = Guid.NewGuid();
            entity.GLAccountId = dto.GLAccountId; // Map GL Account ID
            entity.Status = AccountStatus.Active;
            // NOTE: Opening balance should be managed through GLAccountOpeningBalance table
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
            {
                entity.Create_by = userId;
                entity.Update_by = userId;
            }

            _repository.Insert(entity);
            return entity;
        }

        public int Update(UpdateBankAccountDto dto, string userId)
        {
            // Validate GLAccountId is provided (mandatory field)
            if (dto.GLAccountId == Guid.Empty)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED);
            }

            var companyId = dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null;
            if (AccountNumberExists(dto.AccountNumber, dto.Id, companyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NUMBER_DUPLICATE)
                    .WithParameter("AccountNumber", dto.AccountNumber);
            }

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NOT_FOUND)
                    .WithParameter("BankAccountId", dto.Id);
            }

            entity.BankCode = dto.BankCode;
            entity.BankNameAr = dto.BankNameAr;
            entity.BankNameEn = dto.BankNameEn;
            entity.AccountNumber = dto.AccountNumber;
            entity.IBAN = dto.IBAN;
            entity.SwiftCode = dto.SwiftCode;
            entity.Currency = dto.Currency;
            entity.Branch = dto.Branch;
            entity.BankContact = dto.BankContact;
            // NOTE: Opening balance should be managed through GLAccountOpeningBalance table
            entity.Status = dto.Status;
            entity.Notes = dto.Notes;
            entity.GLAccountId = dto.GLAccountId; // Update GL Account ID
            entity.Update_time = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
            {
                entity.Update_by = userId;
            }

            return _repository.Update(entity);
        }

        public int Delete(Guid[] ids)
        {
            return _repository.Delete(ids);
        }

        /// <summary>
        /// Soft delete with transaction check - حذف ناعم مع التحقق من المعاملات
        /// </summary>
        public int SoftDelete(Guid id, string userId)
        {
            return SoftDelete(new[] { id }, userId);
        }

        /// <summary>
        /// Soft delete multiple accounts with transaction check
        /// </summary>
        public int SoftDelete(Guid[] ids, string userId)
        {
            if (ids == null || ids.Length == 0)
            {
                return 0;
            }

            var deletedCount = 0;

            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                foreach (var id in ids)
                {
                    // Check for related transactions
                    if (HasRelatedTransactions(id))
                    {
                        var account = _repository.GetById(id);
                        var accountNumber = account?.AccountNumber ?? id.ToString();
                        throw new AppException(ExceptionCode.FINANCE_TREASURY_HAS_RELATED_TRANSACTIONS)
                            .WithParameter("Identifier", accountNumber)
                            .WithParameter("EntityType", "BankAccount");
                    }

                    var entity = _repository.GetById(id);
                    if (entity == null || entity.IsDeleted)
                    {
                        continue;
                    }

                    // Perform soft delete
                    entity.IsDeleted = true;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedBy = userId;
                    entity.Update_time = DateTime.Now;
                    entity.Update_by = userId;

                    _repository.Update(entity);

                    // Audit log the deletion
                    _auditService.LogDelete(entity, TreasuryEntityType.BankAccount, entity.AccountNumber, userId);

                    deletedCount++;
                }

                scope.Complete();
            }

            return deletedCount;
        }

        /// <summary>
        /// Check if account has related transactions - التحقق من وجود معاملات مرتبطة
        /// </summary>
        public bool HasRelatedTransactions(Guid accountId)
        {
            // Check for payments (from or to this account)
            var paymentPredicate = Expressionable.Create<Payment>();
            paymentPredicate = paymentPredicate.Or(x => x.SourceAccountId == accountId);
            paymentPredicate = paymentPredicate.Or(x => x.DestinationAccountId == accountId);

            var hasPayments = _paymentRepository.Count(paymentPredicate.ToExpression()) > 0;
            if (hasPayments)
            {
                return true;
            }

            // Check for cheques linked to this account
            var chequePredicate = Expressionable.Create<Cheque>();
            chequePredicate = chequePredicate.And(x => x.BankAccountId == accountId);

            var hasCheques = _chequeRepository.Count(chequePredicate.ToExpression()) > 0;
            return hasCheques;
        }

        public bool AccountNumberExists(string accountNumber, Guid? excludeId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<BankAccount>();
            predicate = predicate.And(x => x.AccountNumber == accountNumber);

            if (excludeId.HasValue)
            {
                predicate = predicate.And(x => x.Id != excludeId.Value);
            }

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            return _repository.Count(predicate.ToExpression()) > 0;
        }

        public BankAccountBalanceDto GetBalance(Guid accountId)
        {
            var account = _repository.GetById(accountId);
            if (account == null)
            {
                return null;
            }

            var result = new BankAccountBalanceDto
            {
                BankAccountId = account.Id,
                BankName = account.BankNameEn,
                AccountNumber = account.AccountNumber,
                Currency = account.Currency,
                LastReconciliationDate = account.ReconciliationDate,
                GLAccountId = account.GLAccountId,
                AsOfDate = DateTime.Now,
                FiscalYear = GetCurrentFiscalYear()
            };

            // Get balance from GL Account if linked
            if (account.GLAccountId.HasValue)
            {
                try
                {
                    var glBalance = _glAccountService.GetAccountBalance(
                        account.GLAccountId.Value,
                        result.FiscalYear,
                        result.AsOfDate);

                    if (glBalance != null)
                    {
                        result.OpeningBalance = glBalance.OpeningBalance;
                        result.TotalDebit = glBalance.PeriodDebit;
                        result.TotalCredit = glBalance.PeriodCredit;
                        result.CurrentBalance = glBalance.CurrentBalance;

                        // Calculate available balance = current balance - pending cheques (calculated on-the-fly)
                        result.PendingChequesAmount = CalculatePendingChequesAmount(accountId);
                        result.AvailableBalance = result.CurrentBalance - result.PendingChequesAmount;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but return basic info
                    result.OpeningBalance = 0;
                    result.TotalDebit = 0;
                    result.TotalCredit = 0;
                    result.CurrentBalance = 0;
                    result.AvailableBalance = 0;
                    result.PendingChequesAmount = 0;
                }
            }

            return result;
        }

        public List<BankReconciliation> GetPendingReconciliations(Guid accountId)
        {
            var predicate = Expressionable.Create<BankReconciliation>();
            predicate = predicate.And(x => x.BankAccountId == accountId);
            predicate = predicate.And(x => x.Status == ReconciliationStatus.InProgress ||
                x.Status == ReconciliationStatus.PendingInvestigation);

            return _reconciliationRepository.GetList(predicate.ToExpression());
        }

        public List<BankAccountListDto> GetActiveAccounts(long? companyId = null)
        {
            var predicate = Expressionable.Create<BankAccount>();
            predicate = predicate.And(x => x.Status == AccountStatus.Active);
            predicate = predicate.And(x => !x.IsDeleted);

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var accounts = _repository.GetList(predicate.ToExpression());

            var result = accounts.Select(a => new BankAccountListDto
            {
                Id = a.Id,
                BankCode = a.BankCode,
                BankNameAr = a.BankNameAr,
                BankNameEn = a.BankNameEn,
                AccountNumber = a.AccountNumber,
                IBAN = a.IBAN,
                SwiftCode = a.SwiftCode,
                Currency = a.Currency,
                Status = a.Status,
                GLAccountId = a.GLAccountId
            }).ToList();

            // Populate balance for each account from GL system
            foreach (var account in result)
            {
                PopulateAccountBalance(account);
            }

            return result;
        }

        #region GL Account Auto-Creation

        /// <summary>
        /// Create bank account with automatic GL account creation - إنشاء حساب بنكي مع إنشاء تلقائي لحساب دفتر الأستاذ
        /// </summary>
        public async Task<BankAccount> CreateWithGLAccountAsync(CreateBankAccountDto dto, string userId)
        {
            var companyId = dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null;

            // Validate bank account number doesn't already exist
            if (AccountNumberExists(dto.AccountNumber, null, companyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_BANK_ACCOUNT_NUMBER_DUPLICATE)
                    .WithParameter("AccountNumber", dto.AccountNumber);
            }

            // Auto-generate bank code if not provided
            string bankCode = dto.BankCode;
            if (string.IsNullOrWhiteSpace(bankCode))
            {
                bankCode = GenerateBankCode(dto.BankNameEn, dto.AccountNumber);
                Console.WriteLine($"[BankAccountService] Auto-generated bank code: {bankCode}");
            }

            Guid glAccountId;

            // Determine if we should auto-create GL account
            bool shouldAutoCreate = dto.AutoCreateGLAccount &&
                                   (!dto.GLAccountId.HasValue || dto.GLAccountId.Value == Guid.Empty);

            if (shouldAutoCreate)
            {
                // Auto-create GL account
                Console.WriteLine($"[BankAccountService] Auto-creating GL account for bank: {dto.BankNameEn}");

                var glAccount = await _glAccountAutoCreator.CreateBankGLAccountAsync(
                    dto.BankNameAr,
                    dto.BankNameEn,
                    dto.Currency,
                    companyId,
                    userId,
                    dto.ParentGLAccountId,
                    dto.CustomGLAccountCode);

                glAccountId = glAccount.AccountId;

                Console.WriteLine($"[BankAccountService] Created GL account: {glAccount.AccountNumber} (ID: {glAccountId})");
            }
            else if (dto.GLAccountId.HasValue && dto.GLAccountId.Value != Guid.Empty)
            {
                // Use provided GL account
                glAccountId = dto.GLAccountId.Value;

                // Validate the GL account exists
                var existingGLAccount = _glAccountService.GetById(glAccountId);
                if (existingGLAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND,
                        new Dictionary<string, object> { { "AccountId", glAccountId } });
                }

                Console.WriteLine($"[BankAccountService] Using existing GL account: {existingGLAccount.AccountNumber}");
            }
            else
            {
                // Neither auto-create nor existing GL account provided
                throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED);
            }

            // Create the bank account
            var entity = dto.Adapt<BankAccount>();
            entity.Id = Guid.NewGuid();
            entity.BankCode = bankCode; // Use auto-generated or provided bank code
            entity.GLAccountId = glAccountId;
            entity.Status = dto.Status;
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
            {
                entity.Create_by = userId;
                entity.Update_by = userId;
            }

            _repository.Insert(entity);

            Console.WriteLine($"[BankAccountService] Created bank account: {entity.AccountNumber} (ID: {entity.Id}) linked to GL: {glAccountId}");

            return entity;
        }

        /// <summary>
        /// Generate unique bank code - توليد رمز بنك فريد
        /// Format: BNK-{BankInitials}-{AccountSuffix}-{Sequence}
        /// </summary>
        private string GenerateBankCode(string bankName, string accountNumber)
        {
            // Get bank initials (first 3 letters of bank name, uppercase)
            var bankInitials = string.IsNullOrWhiteSpace(bankName)
                ? "BNK"
                : new string(bankName.Where(char.IsLetter).Take(3).ToArray()).ToUpper();

            if (bankInitials.Length < 3)
            {
                bankInitials = bankInitials.PadRight(3, 'X');
            }

            // Get last 4 digits of account number
            var accountSuffix = string.IsNullOrWhiteSpace(accountNumber)
                ? "0000"
                : new string(accountNumber.Where(char.IsDigit).TakeLast(4).ToArray());

            if (accountSuffix.Length < 4)
            {
                accountSuffix = accountSuffix.PadLeft(4, '0');
            }

            // Generate base code
            var baseCode = $"BNK-{bankInitials}-{accountSuffix}";

            // Check if code exists and add sequence if needed
            var existingCodes = _repository.GetList()
                .Where(x => x.BankCode != null && x.BankCode.StartsWith(baseCode))
                .Select(x => x.BankCode)
                .ToList();

            if (!existingCodes.Any())
            {
                return baseCode;
            }

            // Find next available sequence
            var maxSequence = 0;
            foreach (var code in existingCodes)
            {
                var parts = code.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[^1], out var seq))
                {
                    if (seq > maxSequence) maxSequence = seq;
                }
            }

            return $"{baseCode}-{maxSequence + 1}";
        }

        /// <summary>
        /// Get available parent GL accounts for bank account creation - الحصول على حسابات الأصل المتاحة
        /// </summary>
        public async Task<List<GLAccount>> GetBankParentAccountOptionsAsync(long? companyId = null)
        {
            return await _glAccountAutoCreator.GetBankParentAccountOptionsAsync(companyId);
        }

        #endregion

        #region Balance Calculation Helpers

        /// <summary>
        /// Get current fiscal year - الحصول على السنة المالية الحالية
        /// </summary>
        private int GetCurrentFiscalYear()
        {
            // TODO: This should be retrieved from fiscal year configuration or company settings
            // For now, using calendar year
            return DateTime.Now.Year;
        }

        /// <summary>
        /// Calculate pending cheques amount from cheque table - حساب مبلغ الشيكات المعلقة من جدول الشيكات
        /// This calculates on-the-fly to ensure accuracy instead of using cached denormalized field.
        /// Pending cheques are outgoing cheques with status: Issued (0) or Presented (1)
        /// </summary>
        private decimal CalculatePendingChequesAmount(Guid bankAccountId)
        {
            try
            {
                // Pending cheques are outgoing cheques that are Issued or Presented (not yet cleared, bounced, or cancelled)
                var predicate = Expressionable.Create<Cheque>();
                predicate = predicate.And(c => c.BankAccountId == bankAccountId);
                predicate = predicate.And(c => c.ChequeType == ChequeType.Outgoing);
                predicate = predicate.And(c => c.Status == ChequeStatus.Issued || c.Status == ChequeStatus.Presented);

                var pendingCheques = _chequeRepository.GetList(predicate.ToExpression());
                var pendingAmount = pendingCheques.Sum(c => c.Amount);

                Console.WriteLine($"[BankAccountService] BankAccountId {bankAccountId}: Calculated {pendingCheques.Count} pending cheques totaling {pendingAmount}");

                return pendingAmount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BankAccountService] Error calculating pending cheques for {bankAccountId}: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Calculate balance for a bank account from GL system
        /// حساب الرصيد للحساب البنكي من نظام دفتر الأستاذ
        /// </summary>
        private void PopulateAccountBalance(BankAccountListDto dto)
        {
            if (!dto.GLAccountId.HasValue)
            {
                Console.WriteLine($"[BankAccountService] Bank {dto.BankCode}: No GLAccountId linked");
                dto.CurrentBalance = 0;
                dto.AvailableBalance = 0;
                return;
            }

            try
            {
                var fiscalYear = GetCurrentFiscalYear();
                Console.WriteLine($"[BankAccountService] Bank {dto.BankCode}: Fetching balance for GLAccountId={dto.GLAccountId.Value}, FiscalYear={fiscalYear}");

                var glBalance = _glAccountService.GetAccountBalance(dto.GLAccountId.Value, fiscalYear, DateTime.Now);

                if (glBalance != null)
                {
                    Console.WriteLine($"[BankAccountService] Bank {dto.BankCode}: OpeningBalance={glBalance.OpeningBalance}, PeriodDebit={glBalance.PeriodDebit}, PeriodCredit={glBalance.PeriodCredit}, CurrentBalance={glBalance.CurrentBalance}");

                    dto.CurrentBalance = glBalance.CurrentBalance;

                    // Calculate available balance = current balance - pending cheques (calculated on-the-fly)
                    var pendingChequesAmount = CalculatePendingChequesAmount(dto.Id);
                    dto.AvailableBalance = dto.CurrentBalance - pendingChequesAmount;
                }
                else
                {
                    Console.WriteLine($"[BankAccountService] Bank {dto.BankCode}: GL Balance returned null");
                    dto.CurrentBalance = 0;
                    dto.AvailableBalance = 0;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the entire request
                Console.WriteLine($"[BankAccountService] Bank {dto.BankCode}: Error fetching balance - {ex.Message}");
                dto.CurrentBalance = 0;
                dto.AvailableBalance = 0;
            }
        }

        #endregion
    }
}





