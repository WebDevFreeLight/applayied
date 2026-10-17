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
using ZR.Repository.Finance.Configuration;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Tax Rule Service - خدمة قواعد الضريبة
    /// </summary>
    [AppService(ServiceType = typeof(ITaxRuleService), ServiceLifetime = LifeTime.Transient)]
    public class TaxRuleService : BaseService<TaxRule>, ITaxRuleService
    {
        private readonly ITaxRuleRepository _taxRuleRepository;
        private readonly IGLAccountRepository _accountRepository;

        public TaxRuleService(
            ITaxRuleRepository taxRuleRepository,
            IGLAccountRepository accountRepository)
        {
            _taxRuleRepository = taxRuleRepository;
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Get paginated list of tax rules
        /// </summary>
        public PagedInfo<TaxRuleListDto> GetList(TaxRuleQueryDto query, PagerInfo pager)
        {
            var searchModel = new TaxRule
            {
                TaxName = query.TaxName,
                CompanyID = query.CompanyId
            };

            var result = _taxRuleRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<TaxRuleListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get tax rule by ID
        /// </summary>
        public TaxRuleDetailDto GetById(Guid id)
        {
            var taxRule = _taxRuleRepository.GetById(id);
            if (taxRule == null) return null;

            return MapToDetailDto(taxRule);
        }

        /// <summary>
        /// Create tax rule
        /// </summary>
        public TaxRule Create(CreateTaxRuleDto dto, string userName)
        {
            // Validate GL account if provided
            if (dto.ApplicableAccountId.HasValue)
            {
                var account = _accountRepository.GetById(dto.ApplicableAccountId.Value);
                if (account == null || !account.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_GL_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.ApplicableAccountId.Value);
                }
            }

            // Validate rate
            if (dto.Rate < 0 || dto.Rate > 100)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_RATE_OUT_OF_RANGE)
                    .WithParameter("Rate", dto.Rate);
            }

            var model = dto.Adapt<TaxRule>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _taxRuleRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update tax rule
        /// </summary>
        public int Update(UpdateTaxRuleDto dto, string userName)
        {
            // Validate GL account if provided
            if (dto.ApplicableAccountId.HasValue)
            {
                var account = _accountRepository.GetById(dto.ApplicableAccountId.Value);
                if (account == null || !account.IsActive)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_GL_ACCOUNT_INVALID)
                        .WithParameter("AccountId", dto.ApplicableAccountId.Value);
                }
            }

            // Validate rate
            if (dto.Rate < 0 || dto.Rate > 100)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_RATE_OUT_OF_RANGE)
                    .WithParameter("Rate", dto.Rate);
            }

            var model = dto.Adapt<TaxRule>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return Update(model, it => new
            {
                it.TaxName,
                it.TaxType,
                it.Rate,
                it.ApplicableAccountId,
                it.Status,
                it.EffectiveFrom,
                it.EffectiveTo,
                it.DescriptionAr,
                it.DescriptionEn,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete tax rules
        /// </summary>
        public int Delete(Guid[] ids)
        {
            return Context.Deleteable<TaxRule>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Calculate tax
        /// </summary>
        public TaxCalculationResultDto CalculateTax(CalculateTaxDto dto)
        {
            var taxRule = _taxRuleRepository.GetById(dto.TaxRuleId);
            if (taxRule == null)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_NOT_FOUND)
                    .WithParameter("TaxRuleId", dto.TaxRuleId);

            if (!taxRule.Status)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_INACTIVE)
                    .WithParameter("TaxRuleId", dto.TaxRuleId);

            // Check effective dates
            var now = DateTime.Now;
            if (taxRule.EffectiveFrom.HasValue && taxRule.EffectiveFrom > now)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_NOT_EFFECTIVE_YET)
                    .WithParameter("EffectiveFrom", taxRule.EffectiveFrom.Value.ToString("yyyy-MM-dd"));
            
            if (taxRule.EffectiveTo.HasValue && taxRule.EffectiveTo < now)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_TAXRULE_EXPIRED)
                    .WithParameter("EffectiveTo", taxRule.EffectiveTo.Value.ToString("yyyy-MM-dd"));

            var taxAmount = dto.BaseAmount * (taxRule.Rate / 100);
            var totalAmount = dto.BaseAmount + taxAmount;

            return new TaxCalculationResultDto
            {
                TaxRuleId = taxRule.Id,
                TaxName = taxRule.TaxName,
                BaseAmount = dto.BaseAmount,
                TaxRate = taxRule.Rate,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount
            };
        }

        /// <summary>
        /// Get active tax rules
        /// </summary>
        public List<TaxRuleListDto> GetActiveTaxRules()
        {
            var taxRules = _taxRuleRepository.GetActiveTaxRules();
            return taxRules.Select(MapToDto).ToList();
        }

        private TaxRuleListDto MapToDto(TaxRule taxRule)
        {
            return taxRule.Adapt<TaxRuleListDto>();
        }

        private TaxRuleDetailDto MapToDetailDto(TaxRule taxRule)
        {
            return taxRule.Adapt<TaxRuleDetailDto>();
        }

    }
}


