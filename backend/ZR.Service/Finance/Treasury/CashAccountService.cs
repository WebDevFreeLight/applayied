using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
using ZR.Model.Finance.GL;
using ZR.Repository.Finance.Treasury;
using ZR.Repository.Finance.PaymentHub;
using ZR.Service.IService.Finance.Treasury;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Cash Account Service - خدمة الحساب النقدي
    /// </summary>
    [AppService(ServiceType = typeof(ICashAccountService), ServiceLifetime = LifeTime.Transient)]
    public class CashAccountService : BaseService<CashAccount>, ICashAccountService
    {
        private readonly CashAccountRepository _repository;
        private readonly PaymentHubRepository _paymentRepository;
        private readonly ITreasuryAuditService _auditService;
        private readonly IGLAccountAutoCreator _glAccountAutoCreator;

        public CashAccountService(
            CashAccountRepository repository,
            PaymentHubRepository paymentRepository,
            ITreasuryAuditService auditService,
            IGLAccountAutoCreator glAccountAutoCreator)
        {
            _repository = repository;
            _paymentRepository = paymentRepository;
            _auditService = auditService;
            _glAccountAutoCreator = glAccountAutoCreator;
        }

        public PagedInfo<CashAccountListDto> GetList(CashAccountQueryDto parm)
        {
            var predicate = Expressionable.Create<CashAccount>();

            // Exclude soft-deleted accounts
            predicate = predicate.And(x => !x.IsDeleted);

            if (parm.TenantId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == (long)parm.TenantId.Value.GetHashCode());
            }

            if (!string.IsNullOrEmpty(parm.Code))
            {
                predicate = predicate.And(x => x.Code.Contains(parm.Code));
            }

            if (!string.IsNullOrEmpty(parm.Name))
            {
                predicate = predicate.And(x => x.NameEn.Contains(parm.Name) || 
                    (x.NameAr != null && x.NameAr.Contains(parm.Name)));
            }

            if (parm.Type.HasValue)
            {
                predicate = predicate.And(x => x.Type == parm.Type.Value);
            }

            if (parm.Status.HasValue)
            {
                predicate = predicate.And(x => x.Status == parm.Status.Value);
            }

            if (!string.IsNullOrEmpty(parm.Currency))
            {
                predicate = predicate.And(x => x.Currency == parm.Currency);
            }

            if (parm.BranchId.HasValue)
            {
                predicate = predicate.And(x => x.BranchId == parm.BranchId.Value);
            }

            var response = _repository.GetPages(predicate.ToExpression(), parm);
            return response.Adapt<PagedInfo<CashAccountListDto>>();
        }

        public CashAccountDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                return null;
            }
            return entity.Adapt<CashAccountDetailDto>();
        }

        public CashAccount Create(CreateCashAccountDto dto, string userId)
        {
            return CreateAsync(dto, userId).GetAwaiter().GetResult();
        }

        public async Task<CashAccount> CreateAsync(CreateCashAccountDto dto, string userId)
        {
            var companyId = dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null;

            // Auto-generate cash account code if not provided
            string cashCode = dto.Code;
            if (string.IsNullOrWhiteSpace(cashCode))
            {
                cashCode = GenerateCashCode(dto.NameEn);
                Console.WriteLine($"[CashAccountService] Auto-generated cash code: {cashCode}");
            }

            if (CodeExists(cashCode, null, companyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_CODE_DUPLICATE)
                    .WithParameter("Code", cashCode);
            }

            Guid glAccountId;

            // Determine if we should auto-create GL account
            bool shouldAutoCreate = dto.AutoCreateGLAccount &&
                                   (!dto.GLAccountId.HasValue || dto.GLAccountId.Value == Guid.Empty);

            if (shouldAutoCreate)
            {
                // Auto-create GL account
                Console.WriteLine($"[CashAccountService] Auto-creating GL account for cash: {dto.NameEn}");

                var glAccount = await _glAccountAutoCreator.CreateCashGLAccountAsync(
                    dto.NameAr,
                    dto.NameEn,
                    dto.Currency,
                    companyId,
                    userId,
                    dto.ParentGLAccountId,
                    dto.CustomGLAccountCode);

                glAccountId = glAccount.AccountId;

                Console.WriteLine($"[CashAccountService] Created GL account: {glAccount.AccountNumber} (ID: {glAccountId})");
            }
            else if (dto.GLAccountId.HasValue && dto.GLAccountId.Value != Guid.Empty)
            {
                // Use provided GL account
                glAccountId = dto.GLAccountId.Value;
                Console.WriteLine($"[CashAccountService] Using existing GL account: {glAccountId}");
            }
            else
            {
                // Neither auto-create nor existing GL account provided
                throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED);
            }

            var entity = dto.Adapt<CashAccount>();
            entity.Id = Guid.NewGuid();
            entity.Code = cashCode; // Use auto-generated or provided code
            entity.GLAccountId = glAccountId;
            entity.Status = dto.Status;
            // NOTE: Opening balance should be managed through GLAccountOpeningBalance table
            entity.Create_time = DateTime.Now;
            entity.Update_time = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
            {
                entity.Create_by = userId;
                entity.Update_by = userId;
            }

            _repository.Insert(entity);

            Console.WriteLine($"[CashAccountService] Created cash account: {entity.Code} (ID: {entity.Id}) linked to GL: {glAccountId}");

            return entity;
        }

        /// <summary>
        /// Generate unique cash account code - توليد رمز حساب نقدي فريد
        /// </summary>
        private string GenerateCashCode(string cashName)
        {
            // Generate code: CASH-XXX where XXX is sequential
            var prefix = "CASH";
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            return $"{prefix}-{timestamp}";
        }

        /// <summary>
        /// Gets parent GL accounts suitable for cash account placement
        /// </summary>
        public async Task<List<GLAccount>> GetCashParentAccountOptionsAsync(long? companyId)
        {
            return await _glAccountAutoCreator.GetCashParentAccountOptionsAsync(companyId);
        }

        public int Update(UpdateCashAccountDto dto, string userId)
        {
            // Validate GLAccountId is provided (mandatory field)
            if (dto.GLAccountId == Guid.Empty)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_GL_ACCOUNT_REQUIRED);
            }

            var companyId = dto.TenantId.HasValue ? (long?)dto.TenantId.Value.GetHashCode() : null;
            if (CodeExists(dto.Code, dto.Id, companyId))
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_CODE_DUPLICATE)
                    .WithParameter("Code", dto.Code);
            }

            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_TREASURY_CASH_ACCOUNT_NOT_FOUND)
                    .WithParameter("CashAccountId", dto.Id);
            }

            entity.Code = dto.Code;
            entity.NameAr = dto.NameAr;
            entity.NameEn = dto.NameEn;
            entity.Currency = dto.Currency;
            entity.BranchId = dto.BranchId;
            entity.Type = dto.Type;
            entity.Status = dto.Status;
            entity.Notes = dto.Notes;
            entity.GLAccountId = dto.GLAccountId; // Update GL Account ID
            entity.Update_time = DateTime.Now;

            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.Update_by = userGuid.ToString();
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
                        var accountCode = account?.Code ?? id.ToString();
                        throw new AppException(ExceptionCode.FINANCE_TREASURY_HAS_RELATED_TRANSACTIONS)
                            .WithParameter("Identifier", accountCode)
                            .WithParameter("EntityType", "CashAccount");
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
                    _auditService.LogDelete(entity, TreasuryEntityType.CashAccount, entity.Code, userId);

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
            // Check for payments (from or to this cash account)
            var paymentPredicate = Expressionable.Create<Payment>();
            paymentPredicate = paymentPredicate.Or(x => x.SourceAccountId == accountId);
            paymentPredicate = paymentPredicate.Or(x => x.DestinationAccountId == accountId);

            return _paymentRepository.Count(paymentPredicate.ToExpression()) > 0;
        }

        public bool CodeExists(string code, Guid? excludeId = null, long? companyId = null)
        {
            var predicate = Expressionable.Create<CashAccount>();
            predicate = predicate.And(x => x.Code == code);

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

        /// <summary>
        /// OBSOLETE: Balance should be retrieved from GLAccountOpeningBalance and computed from journal entries
        /// </summary>
        [Obsolete("Balance should be retrieved from GLAccountOpeningBalance and computed from journal entries")]
        public decimal GetBalance(Guid accountId)
        {
            throw new AppException(ExceptionCode.FINANCE_TREASURY_OBSOLETE_OPERATION)
                .WithParameter("Hint", "Use GLAccountOpeningBalance to retrieve account balance.");
        }

        /// <summary>
        /// OBSOLETE: Balance adjustments should be made through GL Journal Entries
        /// </summary>
        [Obsolete("Balance adjustments should be made through GL Journal Entries")]
        public int AdjustBalance(Guid accountId, decimal adjustmentAmount, string reason, string userId)
        {
            throw new AppException(ExceptionCode.FINANCE_TREASURY_OBSOLETE_OPERATION)
                .WithParameter("Hint", "Balance adjustments should be made through GL Journal Entries.");
        }
    }
}















