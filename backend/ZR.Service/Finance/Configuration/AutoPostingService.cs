using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Repository.Finance.Configuration;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Service - خدمة الترحيل التلقائي
    /// </summary>
    [AppService(ServiceType = typeof(IAutoPostingService), ServiceLifetime = LifeTime.Transient)]
    public class AutoPostingService : BaseService<AutoPostingRule>, IAutoPostingService
    {
        private readonly IAutoPostingRuleRepository _ruleRepository;
        private readonly IGLAccountRepository _accountRepository;

        public AutoPostingService(
            IAutoPostingRuleRepository ruleRepository,
            IGLAccountRepository accountRepository)
        {
            _ruleRepository = ruleRepository;
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Get paginated list of rules
        /// </summary>
        public PagedInfo<AutoPostingRuleListDto> GetList(AutoPostingRuleQueryDto query, PagerInfo pager)
        {
            var searchModel = new AutoPostingRule
            {
                CompanyID = query.CompanyId
            };

            var result = _ruleRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(rule => MapToDto(rule)).ToList();

            return new PagedInfo<AutoPostingRuleListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        public AutoPostingRuleDetailDto GetById(Guid id)
        {
            var rule = _ruleRepository.GetById(id);
            if (rule == null) return null;

            return MapToDetailDto(rule);
        }

        /// <summary>
        /// Create rule
        /// </summary>
        public AutoPostingRule Create(CreateAutoPostingRuleDto dto, string userName)
        {
            // Validate GL accounts
            if (dto.DebitAccountId.HasValue)
            {
                var debitAccount = _accountRepository.GetById(dto.DebitAccountId.Value);
                if (debitAccount == null || !debitAccount.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_DEBIT_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.DebitAccountId.Value);
                }
            }

            if (dto.CreditAccountId.HasValue)
            {
                var creditAccount = _accountRepository.GetById(dto.CreditAccountId.Value);
                if (creditAccount == null || !creditAccount.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_CREDIT_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.CreditAccountId.Value);
                }
            }

            // At least one account must be specified
            if (!dto.DebitAccountId.HasValue && !dto.CreditAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_ACCOUNT_REQUIRED);
            }

            var model = dto.Adapt<AutoPostingRule>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _ruleRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update rule
        /// </summary>
        public int Update(UpdateAutoPostingRuleDto dto, string userName)
        {
            // Validate GL accounts
            if (dto.DebitAccountId.HasValue)
            {
                var debitAccount = _accountRepository.GetById(dto.DebitAccountId.Value);
                if (debitAccount == null || !debitAccount.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_DEBIT_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.DebitAccountId.Value);
                }
            }

            if (dto.CreditAccountId.HasValue)
            {
                var creditAccount = _accountRepository.GetById(dto.CreditAccountId.Value);
                if (creditAccount == null || !creditAccount.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_CREDIT_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.CreditAccountId.Value);
                }
            }

            // At least one account must be specified
            if (!dto.DebitAccountId.HasValue && !dto.CreditAccountId.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_AUTOPOSTING_ACCOUNT_REQUIRED);
            }

            var model = dto.Adapt<AutoPostingRule>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return Update(model, it => new
            {
                it.TransactionType,
                it.DebitAccountId,
                it.CreditAccountId,
                it.DescriptionTemplateAr,
                it.DescriptionTemplateEn,
                it.Active,
                it.Priority,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete rules
        /// </summary>
        public int Delete(Guid[] ids)
        {
            return Context.Deleteable<AutoPostingRule>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Get rules for transaction type
        /// </summary>
        public List<AutoPostingRuleListDto> GetRulesForTransactionType(TransactionType transactionType)
        {
            var rules = _ruleRepository.GetActiveRulesByTransactionType(transactionType);
            return rules.Select(rule => MapToDto(rule)).ToList();
        }

        /// <summary>
        /// Apply auto posting rule
        /// </summary>
        public object ApplyAutoPostingRule(TransactionType transactionType, decimal amount, string description = null)
        {
            var rules = _ruleRepository.GetActiveRulesByTransactionType(transactionType);
            if (!rules.Any())
            {
                return null; // No rules found
            }

            // Get the highest priority rule (lowest priority number)
            var rule = rules.OrderBy(r => r.Priority).First();

            var descriptionAr = rule.DescriptionTemplateAr ?? description ?? $"ترحيل تلقائي - {transactionType}";
            var descriptionEn = rule.DescriptionTemplateEn ?? description ?? $"Auto Posting - {transactionType}";

            return new
            {
                DebitAccountId = rule.DebitAccountId,
                CreditAccountId = rule.CreditAccountId,
                DebitAmount = rule.DebitAccountId.HasValue ? amount : 0,
                CreditAmount = rule.CreditAccountId.HasValue ? amount : 0,
                DescriptionAr = descriptionAr,
                DescriptionEn = descriptionEn
            };
        }

        private AutoPostingRuleListDto MapToDto(AutoPostingRule rule)
        {
            var dto = rule.Adapt<AutoPostingRuleListDto>();
            
            if (rule.DebitAccountId.HasValue)
            {
                var account = _accountRepository.GetById(rule.DebitAccountId.Value);
                dto.DebitAccountName = account != null ? account.AccountNameEn : string.Empty;
            }

            if (rule.CreditAccountId.HasValue)
            {
                var account = _accountRepository.GetById(rule.CreditAccountId.Value);
                dto.CreditAccountName = account != null ? account.AccountNameEn : string.Empty;
            }

            return dto;
        }

        private AutoPostingRuleDetailDto MapToDetailDto(AutoPostingRule rule)
        {
            var dto = rule.Adapt<AutoPostingRuleDetailDto>();
            
            if (rule.DebitAccountId.HasValue)
            {
                var account = _accountRepository.GetById(rule.DebitAccountId.Value);
                dto.DebitAccountName = account != null ? account.AccountNameEn : string.Empty;
            }

            if (rule.CreditAccountId.HasValue)
            {
                var account = _accountRepository.GetById(rule.CreditAccountId.Value);
                dto.CreditAccountName = account != null ? account.AccountNameEn : string.Empty;
            }

            return dto;
        }

    }
}


