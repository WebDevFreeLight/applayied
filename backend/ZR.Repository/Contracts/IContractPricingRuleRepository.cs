using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Pricing Rule Repository Interface - واجهة مستودع قواعد تسعير العقد
    /// </summary>
    public interface IContractPricingRuleRepository : IBaseRepository<ErpContractPricingRule>
    {
        /// <summary>
        /// Get pricing rules by contract ID
        /// </summary>
        List<ErpContractPricingRuleListDto> GetByContractId(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get pricing rules by contract ID async
        /// </summary>
        Task<List<ErpContractPricingRuleListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null);

        /// <summary>
        /// Get pricing rule by ID
        /// </summary>
        ErpContractPricingRule GetById(Guid ruleId);

        /// <summary>
        /// Get pricing rule by ID async
        /// </summary>
        Task<ErpContractPricingRule> GetByIdAsync(Guid ruleId);

        /// <summary>
        /// Create pricing rule
        /// </summary>
        Guid Create(ErpContractPricingRule rule);

        /// <summary>
        /// Create pricing rule async
        /// </summary>
        Task<Guid> CreateAsync(ErpContractPricingRule rule);

        /// <summary>
        /// Update pricing rule
        /// </summary>
        int UpdateRule(ErpContractPricingRule rule);

        /// <summary>
        /// Update pricing rule async
        /// </summary>
        Task<int> UpdateRuleAsync(ErpContractPricingRule rule);

        /// <summary>
        /// Delete pricing rule
        /// </summary>
        int Delete(Guid ruleId);

        /// <summary>
        /// Delete pricing rule async
        /// </summary>
        Task<int> DeleteAsync(Guid ruleId);

        /// <summary>
        /// Delete rules by contract ID
        /// </summary>
        int DeleteByContractId(Guid contractId);

        /// <summary>
        /// Delete rules by contract ID async
        /// </summary>
        Task<int> DeleteByContractIdAsync(Guid contractId);

        /// <summary>
        /// Toggle active status
        /// </summary>
        int ToggleActive(Guid ruleId);

        /// <summary>
        /// Toggle active status async
        /// </summary>
        Task<int> ToggleActiveAsync(Guid ruleId);

        /// <summary>
        /// Get active rules for date
        /// </summary>
        List<ErpContractPricingRule> GetActiveRulesForDate(Guid contractId, DateTime date);

        /// <summary>
        /// Get active rules for date async
        /// </summary>
        Task<List<ErpContractPricingRule>> GetActiveRulesForDateAsync(Guid contractId, DateTime date);

        /// <summary>
        /// Calculate amount with pricing rules
        /// </summary>
        decimal CalculateAmount(Guid contractId, decimal baseAmount, DateTime billingDate);

        /// <summary>
        /// Calculate amount with pricing rules async
        /// </summary>
        Task<decimal> CalculateAmountAsync(Guid contractId, decimal baseAmount, DateTime billingDate);
    }
}
