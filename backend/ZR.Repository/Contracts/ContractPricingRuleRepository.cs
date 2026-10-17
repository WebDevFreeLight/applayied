using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Dto;
using ZR.Model.Contracts.Enums;
using ZR.Repository;

namespace ZR.Repository.Contracts
{
    /// <summary>
    /// Contract Pricing Rule Repository - مستودع قواعد تسعير العقد
    /// Updated to follow Finance module pattern with DI
    /// </summary>
    [AppService(ServiceType = typeof(IContractPricingRuleRepository), ServiceLifetime = LifeTime.Transient)]
    public class ContractPricingRuleRepository : BaseRepository<ErpContractPricingRule>, IContractPricingRuleRepository
    {
        /// <summary>
        /// Get pricing rules by contract ID
        /// </summary>
        public List<ErpContractPricingRuleListDto> GetByContractId(Guid contractId, bool? isActive = null)
        {
            var query = Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId);

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            return query
                .OrderBy(r => r.Priority)
                .Select(r => new ErpContractPricingRuleListDto
                {
                    PricingRuleId = r.PricingRuleId,
                    ContractId = r.ContractId,
                    RuleType = r.RuleType,
                    RuleName = r.RuleName,
                    RuleNameAr = r.RuleNameAr,
                    Value = r.Value,
                    ValueType = r.ValueType,
                    Priority = r.Priority,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    IsActive = r.IsActive,
                    Description = r.Description
                })
                .ToList();
        }

        /// <summary>
        /// Get pricing rule by ID
        /// </summary>
        public ErpContractPricingRule GetById(Guid ruleId)
        {
            return Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.PricingRuleId == ruleId)
                .First();
        }

        /// <summary>
        /// Create pricing rule
        /// </summary>
        public Guid Create(ErpContractPricingRule rule)
        {
            rule.PricingRuleId = Guid.NewGuid();
            rule.Create_time = DateTime.Now;

            Context.Insertable(rule).ExecuteCommand();
            return rule.PricingRuleId;
        }

        /// <summary>
        /// Update pricing rule
        /// </summary>
        public int UpdateRule(ErpContractPricingRule rule)
        {
            rule.Update_time = DateTime.Now;
            return Context.Updateable(rule)
                .IgnoreColumns(r => new { r.PricingRuleId, r.ContractId, r.Create_by, r.Create_time })
                .Where(r => r.PricingRuleId == rule.PricingRuleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete pricing rule
        /// </summary>
        public int Delete(Guid ruleId)
        {
            return Context.Deleteable<ErpContractPricingRule>()
                .Where(r => r.PricingRuleId == ruleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete rules by contract ID
        /// </summary>
        public int DeleteByContractId(Guid contractId)
        {
            return Context.Deleteable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Toggle active status
        /// </summary>
        public int ToggleActive(Guid ruleId)
        {
            var rule = GetById(ruleId);
            if (rule == null) return 0;

            return Context.Updateable<ErpContractPricingRule>()
                .SetColumns(r => r.IsActive == !rule.IsActive)
                .SetColumns(r => r.Update_time == DateTime.Now)
                .Where(r => r.PricingRuleId == ruleId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get active rules for date
        /// </summary>
        public List<ErpContractPricingRule> GetActiveRulesForDate(Guid contractId, DateTime date)
        {
            return Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId && r.IsActive)
                .Where(r => !r.StartDate.HasValue || r.StartDate.Value <= date)
                .Where(r => !r.EndDate.HasValue || r.EndDate.Value >= date)
                .OrderBy(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// Calculate amount with pricing rules
        /// </summary>
        public decimal CalculateAmount(Guid contractId, decimal baseAmount, DateTime billingDate)
        {
            var rules = GetActiveRulesForDate(contractId, billingDate);
            return ApplyPricingRules(rules, baseAmount);
        }

        #region Async Methods

        /// <summary>
        /// Get pricing rules by contract ID async
        /// </summary>
        public async Task<List<ErpContractPricingRuleListDto>> GetByContractIdAsync(Guid contractId, bool? isActive = null)
        {
            var query = Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId);

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(r => r.Priority)
                .Select(r => new ErpContractPricingRuleListDto
                {
                    PricingRuleId = r.PricingRuleId,
                    ContractId = r.ContractId,
                    RuleType = r.RuleType,
                    RuleName = r.RuleName,
                    RuleNameAr = r.RuleNameAr,
                    Value = r.Value,
                    ValueType = r.ValueType,
                    Priority = r.Priority,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    IsActive = r.IsActive,
                    Description = r.Description
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get pricing rule by ID async
        /// </summary>
        public async Task<ErpContractPricingRule> GetByIdAsync(Guid ruleId)
        {
            return await Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.PricingRuleId == ruleId)
                .FirstAsync();
        }

        /// <summary>
        /// Create pricing rule async
        /// </summary>
        public async Task<Guid> CreateAsync(ErpContractPricingRule rule)
        {
            rule.PricingRuleId = Guid.NewGuid();
            rule.Create_time = DateTime.Now;

            await Context.Insertable(rule).ExecuteCommandAsync();
            return rule.PricingRuleId;
        }

        /// <summary>
        /// Update pricing rule async
        /// </summary>
        public async Task<int> UpdateRuleAsync(ErpContractPricingRule rule)
        {
            rule.Update_time = DateTime.Now;
            return await Context.Updateable(rule)
                .IgnoreColumns(r => new { r.PricingRuleId, r.ContractId, r.Create_by, r.Create_time })
                .Where(r => r.PricingRuleId == rule.PricingRuleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete pricing rule async
        /// </summary>
        public async Task<int> DeleteAsync(Guid ruleId)
        {
            return await Context.Deleteable<ErpContractPricingRule>()
                .Where(r => r.PricingRuleId == ruleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete rules by contract ID async
        /// </summary>
        public async Task<int> DeleteByContractIdAsync(Guid contractId)
        {
            return await Context.Deleteable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Toggle active status async
        /// </summary>
        public async Task<int> ToggleActiveAsync(Guid ruleId)
        {
            var rule = await GetByIdAsync(ruleId);
            if (rule == null) return 0;

            return await Context.Updateable<ErpContractPricingRule>()
                .SetColumns(r => r.IsActive == !rule.IsActive)
                .SetColumns(r => r.Update_time == DateTime.Now)
                .Where(r => r.PricingRuleId == ruleId)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Get active rules for date async
        /// </summary>
        public async Task<List<ErpContractPricingRule>> GetActiveRulesForDateAsync(Guid contractId, DateTime date)
        {
            return await Context.Queryable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId && r.IsActive)
                .Where(r => !r.StartDate.HasValue || r.StartDate.Value <= date)
                .Where(r => !r.EndDate.HasValue || r.EndDate.Value >= date)
                .OrderBy(r => r.Priority)
                .ToListAsync();
        }

        /// <summary>
        /// Calculate amount with pricing rules async
        /// </summary>
        public async Task<decimal> CalculateAmountAsync(Guid contractId, decimal baseAmount, DateTime billingDate)
        {
            var rules = await GetActiveRulesForDateAsync(contractId, billingDate);
            return ApplyPricingRules(rules, baseAmount);
        }

        #endregion

        #region Private Helper Methods

        private decimal ApplyPricingRules(List<ErpContractPricingRule> rules, decimal baseAmount)
        {
            var totalAmount = baseAmount;

            foreach (var rule in rules)
            {
                decimal adjustment = 0;
                switch (rule.ValueType?.ToLower())
                {
                    case "percentage":
                        adjustment = totalAmount * (rule.Value / 100);
                        break;
                    case "perunit":
                    case "amount":
                    default:
                        adjustment = rule.Value;
                        break;
                }

                // Apply adjustment based on rule type
                switch (rule.RuleType)
                {
                    case PricingRuleType.PercentageDiscount:
                    case PricingRuleType.FixedDiscount:
                    case PricingRuleType.AdvancePaymentDiscount:
                        totalAmount -= adjustment;
                        break;
                    case PricingRuleType.Fixed:
                    case PricingRuleType.Variable:
                    case PricingRuleType.Seasonal:
                    case PricingRuleType.AreaBased:
                    case PricingRuleType.Tiered:
                    case PricingRuleType.Escalation:
                    default:
                        totalAmount += adjustment;
                        break;
                }
            }

            return Math.Max(0, totalAmount);
        }

        #endregion
    }
}
