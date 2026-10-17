using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Mapster;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Repository.Contracts;
using ZR.Service.Contracts.IService;
using ZR.ServiceCore;

namespace ZR.Service.Contracts
{
    /// <summary>
    /// Contract Pricing Rule Service - خدمة قواعد تسعير العقد
    /// Business logic layer for contract pricing rule operations
    /// Updated to follow Finance module pattern with BaseService and DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractPricingRuleService), ServiceLifetime = LifeTime.Transient)]
    public class ContractPricingRuleService : BaseService<ErpContractPricingRule>, IContractPricingRuleService
    {
        private readonly IContractPricingRuleRepository _pricingRuleRepository;

        public ContractPricingRuleService(IContractPricingRuleRepository pricingRuleRepository)
        {
            _pricingRuleRepository = pricingRuleRepository;
        }

        /// <summary>
        /// Get pricing rules by contract ID
        /// </summary>
        public List<ErpContractPricingRuleListDto> GetByContractId(Guid contractId, bool? isActive = null)
        {
            return _pricingRuleRepository.GetByContractId(contractId, isActive);
        }

        /// <summary>
        /// Get pricing rules by contract ID async
        /// </summary>
        public async Task<List<ErpContractPricingRuleListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null)
        {
            return await _pricingRuleRepository.GetByContractIdAsync(contractId, isActive);
        }

        /// <summary>
        /// Get pricing rule by ID
        /// </summary>
        public ErpContractPricingRule GetById(Guid ruleId)
        {
            return _pricingRuleRepository.GetById(ruleId);
        }

        /// <summary>
        /// Get pricing rule by ID async
        /// </summary>
        public async Task<ErpContractPricingRule> GetByIdAsync(Guid ruleId)
        {
            return await _pricingRuleRepository.GetByIdAsync(ruleId);
        }

        /// <summary>
        /// Create pricing rule
        /// </summary>
        public Guid Create(CreateErpContractPricingRuleDto dto, string createdBy)
        {
            var rule = dto.Adapt<ErpContractPricingRule>();
            rule.Create_by = createdBy;

            return _pricingRuleRepository.Create(rule);
        }

        /// <summary>
        /// Create pricing rule async
        /// </summary>
        public async Task<Guid> CreateAsync(CreateErpContractPricingRuleDto dto, string createdBy)
        {
            var rule = dto.Adapt<ErpContractPricingRule>();
            rule.Create_by = createdBy;

            return await _pricingRuleRepository.CreateAsync(rule);
        }

        /// <summary>
        /// Update pricing rule
        /// </summary>
        public int Update(UpdateErpContractPricingRuleDto dto, string updatedBy)
        {
            var existing = _pricingRuleRepository.GetById(dto.PricingRuleId);
            if (existing == null) return 0;

            var rule = dto.Adapt<ErpContractPricingRule>();
            rule.ContractId = existing.ContractId;
            rule.Update_by = updatedBy;

            return _pricingRuleRepository.UpdateRule(rule);
        }

        /// <summary>
        /// Update pricing rule async
        /// </summary>
        public async Task<int> UpdateAsync(UpdateErpContractPricingRuleDto dto, string updatedBy)
        {
            var existing = await _pricingRuleRepository.GetByIdAsync(dto.PricingRuleId);
            if (existing == null) return 0;

            var rule = dto.Adapt<ErpContractPricingRule>();
            rule.ContractId = existing.ContractId;
            rule.Update_by = updatedBy;

            return await _pricingRuleRepository.UpdateRuleAsync(rule);
        }

        /// <summary>
        /// Delete pricing rule
        /// </summary>
        public int Delete(Guid ruleId)
        {
            return _pricingRuleRepository.Delete(ruleId);
        }

        /// <summary>
        /// Delete pricing rule async
        /// </summary>
        public async Task<int> DeleteAsync(Guid ruleId)
        {
            return await _pricingRuleRepository.DeleteAsync(ruleId);
        }

        /// <summary>
        /// Toggle rule active status
        /// </summary>
        public int ToggleActive(Guid ruleId)
        {
            return _pricingRuleRepository.ToggleActive(ruleId);
        }

        /// <summary>
        /// Toggle rule active status async
        /// </summary>
        public async Task<int> ToggleActiveAsync(Guid ruleId)
        {
            return await _pricingRuleRepository.ToggleActiveAsync(ruleId);
        }

        /// <summary>
        /// Calculate amount with pricing rules
        /// </summary>
        public decimal CalculateAmount(Guid contractId, decimal baseAmount, DateTime billingDate)
        {
            return _pricingRuleRepository.CalculateAmount(contractId, baseAmount, billingDate);
        }

        /// <summary>
        /// Calculate amount with pricing rules async
        /// </summary>
        public async Task<decimal> CalculateAmountAsync(Guid contractId, decimal baseAmount, DateTime billingDate)
        {
            return await _pricingRuleRepository.CalculateAmountAsync(contractId, baseAmount, billingDate);
        }

        /// <summary>
        /// Get active rules for date
        /// </summary>
        public List<ErpContractPricingRule> GetActiveRulesForDate(Guid contractId, DateTime date)
        {
            return _pricingRuleRepository.GetActiveRulesForDate(contractId, date);
        }

        /// <summary>
        /// Get active rules for date async
        /// </summary>
        public async Task<List<ErpContractPricingRule>> GetActiveRulesForDateAsync(Guid contractId, DateTime date)
        {
            return await _pricingRuleRepository.GetActiveRulesForDateAsync(contractId, date);
        }
    }
}
