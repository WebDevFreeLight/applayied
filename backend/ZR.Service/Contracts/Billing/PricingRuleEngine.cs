using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using SqlSugar.IOC;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;
using ZR.Service.IService.Contracts.Billing;

namespace ZR.Service.Contracts.Billing
{
    /// <summary>
    /// Pricing Rule Engine - محرك قواعد التسعير
    /// Applies pricing rules, escalations, and seasonal adjustments
    /// Works with ErpContractPricingRule and its extension tables
    /// Updated to use [AppService] for DI registration
    /// </summary>
    [AppService(ServiceType = typeof(IPricingRuleEngine), ServiceLifetime = LifeTime.Transient)]
    public class PricingRuleEngine : IPricingRuleEngine
    {
        /// <summary>
        /// Database context using SqlSugar IOC pattern
        /// </summary>
        private ISqlSugarClient Db => DbScoped.SugarScope.GetConnectionScope(0);

        public PricingRuleEngine()
        {
        }

        #region Main Calculation Methods

        /// <summary>
        /// Apply all applicable pricing rules to a base amount
        /// Order: 1. Fixed Rules → 2. Escalations → 3. Seasonal → 4. Discounts → 5. Tiered
        /// </summary>
        public async Task<PricingCalculationResult> CalculatePriceAsync(PricingCalculationRequest request)
        {
            var result = new PricingCalculationResult
            {
                BaseAmount = request.BaseAmount,
                Currency = request.Currency,
                AdjustmentDetails = new List<PricingAdjustmentDetail>()
            };

            decimal currentAmount = request.BaseAmount;
            int orderCounter = 1;

            // Get all active pricing rules for this contract
            var rules = await GetActivePricingRulesAsync(request.ContractId, request.UnitId, request.BillingDate);

            // Step 1: Apply Escalation Rules
            if (request.ApplyEscalations)
            {
                var escalationRules = rules
                    .Where(r => r.RuleType == PricingRuleType.Escalation)
                    .OrderBy(r => r.Priority)
                    .ToList();

                foreach (var rule in escalationRules)
                {
                    var escalationAmount = CalculatePricingRuleAmount(currentAmount, rule);
                    result.EscalationAmount += escalationAmount;

                    result.AdjustmentDetails.Add(new PricingAdjustmentDetail
                    {
                        AdjustmentType = "Escalation",
                        RuleName = rule.RuleName ?? "Escalation",
                        Percentage = rule.ValueType == "Percentage" ? rule.Value : 0,
                        Amount = escalationAmount,
                        EffectiveDate = rule.StartDate ?? DateTime.MinValue,
                        Description = rule.Description ?? GetRuleDescription(rule),
                        ApplicationOrder = orderCounter++
                    });

                    if (rule.IsCompounding)
                    {
                        currentAmount += escalationAmount;
                    }
                }
            }

            // Step 2: Apply Seasonal Rules
            if (request.ApplySeasonalRules)
            {
                var seasonalRules = rules
                    .Where(r => r.RuleType == PricingRuleType.Seasonal)
                    .OrderBy(r => r.Priority)
                    .ToList();

                foreach (var rule in seasonalRules)
                {
                    // Load seasonal extension for period calculation
                    var seasonalExt = await Db.Queryable<ErpContractSeasonalRule>()
                        .FirstAsync(s => s.PricingRuleId == rule.PricingRuleId);

                    var seasonalAdjustment = CalculateSeasonalAdjustment(
                        currentAmount + result.EscalationAmount,
                        rule,
                        seasonalExt,
                        request.PeriodStart,
                        request.PeriodEnd);

                    result.SeasonalAdjustment += seasonalAdjustment;

                    result.AdjustmentDetails.Add(new PricingAdjustmentDetail
                    {
                        AdjustmentType = "Seasonal",
                        RuleName = seasonalExt?.SeasonName ?? rule.RuleName ?? "Seasonal",
                        Percentage = rule.ValueType == "Percentage" ? rule.Value : 0,
                        Amount = seasonalAdjustment,
                        EffectiveDate = rule.StartDate ?? DateTime.MinValue,
                        Description = rule.Description ?? GetRuleDescription(rule),
                        ApplicationOrder = orderCounter++
                    });

                    if (rule.IsCompounding)
                    {
                        currentAmount += seasonalAdjustment;
                    }
                }
            }

            // Step 3: Apply Discount Rules
            if (request.ApplyDiscounts)
            {
                var discountRules = rules
                    .Where(r => r.RuleType == PricingRuleType.PercentageDiscount ||
                               r.RuleType == PricingRuleType.FixedDiscount ||
                               r.RuleType == PricingRuleType.AdvancePaymentDiscount)
                    .OrderBy(r => r.Priority)
                    .ToList();

                foreach (var rule in discountRules)
                {
                    var amountBeforeDiscount = currentAmount + result.EscalationAmount + result.SeasonalAdjustment;
                    var discountAmount = Math.Abs(CalculatePricingRuleAmount(amountBeforeDiscount, rule));
                    result.DiscountAmount += discountAmount;

                    result.AdjustmentDetails.Add(new PricingAdjustmentDetail
                    {
                        AdjustmentType = "Discount",
                        RuleName = rule.RuleName ?? "Discount",
                        Percentage = Math.Abs(rule.Value),
                        Amount = -discountAmount,
                        EffectiveDate = rule.StartDate ?? DateTime.MinValue,
                        Description = rule.Description ?? GetRuleDescription(rule),
                        ApplicationOrder = orderCounter++
                    });
                }
            }

            // Calculate final amount
            result.FinalAmount = result.BaseAmount + result.EscalationAmount + result.SeasonalAdjustment - result.DiscountAmount;
            result.FinalAmount = Math.Max(0, Math.Round(result.FinalAmount, 4));

            // Calculate total adjustment percentage
            if (result.BaseAmount > 0)
            {
                result.TotalAdjustmentPercentage = Math.Round(((result.FinalAmount - result.BaseAmount) / result.BaseAmount) * 100, 4);
            }

            return result;
        }

        /// <summary>
        /// Apply escalation rules for a specific date
        /// </summary>
        public async Task<decimal> ApplyEscalationAsync(Guid contractId, decimal baseAmount, DateTime effectiveDate)
        {
            var rules = await GetActivePricingRulesAsync(contractId, null, effectiveDate);
            var escalationRules = rules
                .Where(r => r.RuleType == PricingRuleType.Escalation)
                .OrderBy(r => r.Priority)
                .ToList();

            decimal currentAmount = baseAmount;
            foreach (var rule in escalationRules)
            {
                var escalationAmount = CalculatePricingRuleAmount(currentAmount, rule);
                if (rule.IsCompounding)
                {
                    currentAmount += escalationAmount;
                }
                else
                {
                    currentAmount = baseAmount + escalationAmount;
                }
            }

            return Math.Round(currentAmount, 4);
        }

        /// <summary>
        /// Apply seasonal rules for a specific period
        /// </summary>
        public async Task<decimal> ApplySeasonalAdjustmentAsync(Guid contractId, decimal amount, DateTime periodStart, DateTime periodEnd)
        {
            var rules = await GetActivePricingRulesAsync(contractId, null, periodStart);
            var seasonalRules = rules
                .Where(r => r.RuleType == PricingRuleType.Seasonal)
                .OrderBy(r => r.Priority)
                .ToList();

            decimal adjustedAmount = amount;
            foreach (var rule in seasonalRules)
            {
                var seasonalExt = await Db.Queryable<ErpContractSeasonalRule>()
                    .FirstAsync(s => s.PricingRuleId == rule.PricingRuleId);

                var adjustment = CalculateSeasonalAdjustment(adjustedAmount, rule, seasonalExt, periodStart, periodEnd);
                if (rule.IsCompounding)
                {
                    adjustedAmount += adjustment;
                }
                else
                {
                    adjustedAmount = amount + adjustment;
                }
            }

            return Math.Round(adjustedAmount, 4);
        }

        #endregion

        #region Data Access Methods

        /// <summary>
        /// Get active pricing rules for a contract
        /// </summary>
        private async Task<List<ErpContractPricingRule>> GetActivePricingRulesAsync(
            Guid contractId, Guid? unitId, DateTime asOfDate)
        {
            var query = Db.Queryable<ErpContractPricingRule>()
                .Where(r => r.ContractId == contractId)
                .Where(r => r.IsActive)
                .Where(r => !r.StartDate.HasValue || r.StartDate <= asOfDate)
                .Where(r => !r.EndDate.HasValue || r.EndDate >= asOfDate);

            // If unitId specified, include both unit-specific and contract-wide rules
            if (unitId.HasValue)
            {
                query = query.Where(r => !r.UnitId.HasValue || r.UnitId == unitId);
            }
            else
            {
                query = query.Where(r => !r.UnitId.HasValue); // Only contract-wide rules
            }

            return await query.OrderBy(r => r.Priority).ToListAsync();
        }

        /// <summary>
        /// Get active escalation rules with extension data
        /// </summary>
        public async Task<List<ErpContractEscalationRule>> GetActiveEscalationsAsync(Guid contractId, DateTime asOfDate)
        {
            var escalationRules = await Db.Queryable<ErpContractPricingRule>()
                .Includes(r => r.EscalationRule)
                .Where(r => r.ContractId == contractId)
                .Where(r => r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Escalation)
                .Where(r => !r.StartDate.HasValue || r.StartDate <= asOfDate)
                .Where(r => !r.EndDate.HasValue || r.EndDate >= asOfDate)
                .ToListAsync();

            return escalationRules
                .Where(r => r.EscalationRule != null)
                .Select(r => r.EscalationRule)
                .ToList();
        }

        /// <summary>
        /// Get active seasonal rules with extension data
        /// </summary>
        public async Task<List<ErpContractSeasonalRule>> GetActiveSeasonalRulesAsync(
            Guid contractId, DateTime periodStart, DateTime periodEnd)
        {
            var seasonalRules = await Db.Queryable<ErpContractPricingRule>()
                .Includes(r => r.SeasonalRule)
                .Where(r => r.ContractId == contractId)
                .Where(r => r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Seasonal)
                .ToListAsync();

            // Filter by period overlap
            var result = new List<ErpContractSeasonalRule>();
            foreach (var rule in seasonalRules.Where(r => r.SeasonalRule != null))
            {
                var seasonal = rule.SeasonalRule;
                if (IsSeasonOverlapping(seasonal, periodStart, periodEnd))
                {
                    result.Add(seasonal);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate cumulative escalation percentage
        /// </summary>
        public async Task<decimal> GetCumulativeEscalationPercentageAsync(Guid contractId, DateTime fromDate, DateTime toDate)
        {
            var rules = await Db.Queryable<ErpContractPricingRule>()
                .Includes(r => r.EscalationRule)
                .Where(r => r.ContractId == contractId)
                .Where(r => r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Escalation)
                .Where(r => r.StartDate >= fromDate && r.StartDate <= toDate)
                .OrderBy(r => r.StartDate)
                .ToListAsync();

            decimal cumulativePercentage = 0;
            decimal compoundMultiplier = 1m;

            foreach (var rule in rules)
            {
                if (rule.ValueType == "Percentage")
                {
                    if (rule.IsCompounding)
                    {
                        compoundMultiplier *= (1 + rule.Value / 100);
                    }
                    else
                    {
                        cumulativePercentage += rule.Value;
                    }
                }
            }

            return Math.Round((compoundMultiplier - 1) * 100 + cumulativePercentage, 4);
        }

        /// <summary>
        /// Preview price after all adjustments
        /// </summary>
        public async Task<PricingPreviewResult> PreviewPriceAsync(PricingCalculationRequest request)
        {
            var baseResult = await CalculatePriceAsync(request);

            var preview = new PricingPreviewResult
            {
                BaseAmount = baseResult.BaseAmount,
                EscalationAmount = baseResult.EscalationAmount,
                SeasonalAdjustment = baseResult.SeasonalAdjustment,
                DiscountAmount = baseResult.DiscountAmount,
                FinalAmount = baseResult.FinalAmount,
                TotalAdjustmentPercentage = baseResult.TotalAdjustmentPercentage,
                Currency = baseResult.Currency,
                AdjustmentDetails = baseResult.AdjustmentDetails
            };

            // Add future escalations preview
            var futureRules = await Db.Queryable<ErpContractPricingRule>()
                .Includes(r => r.EscalationRule)
                .Where(r => r.ContractId == request.ContractId)
                .Where(r => r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Escalation)
                .Where(r => r.StartDate > request.BillingDate)
                .OrderBy(r => r.StartDate)
                .Take(5)
                .ToListAsync();

            decimal projectedAmount = preview.FinalAmount;
            foreach (var rule in futureRules)
            {
                var escalation = rule.ValueType == "Percentage"
                    ? projectedAmount * rule.Value / 100
                    : rule.Value;
                projectedAmount += escalation;

                preview.FutureEscalations.Add(new EscalationPreview
                {
                    EffectiveDate = rule.StartDate ?? DateTime.MinValue,
                    Percentage = rule.ValueType == "Percentage" ? rule.Value : 0,
                    ProjectedAmount = Math.Round(projectedAmount, 4),
                    Description = rule.Description ?? rule.RuleName ?? "Future Escalation"
                });
            }

            // Add seasonal rules preview
            var seasonalRules = await Db.Queryable<ErpContractPricingRule>()
                .Includes(r => r.SeasonalRule)
                .Where(r => r.ContractId == request.ContractId)
                .Where(r => r.IsActive)
                .Where(r => r.RuleType == PricingRuleType.Seasonal)
                .ToListAsync();

            foreach (var rule in seasonalRules.Where(r => r.SeasonalRule != null))
            {
                var seasonal = rule.SeasonalRule;
                preview.ApplicableSeasons.Add(new SeasonalPreview
                {
                    SeasonName = seasonal.SeasonName ?? "Season",
                    StartDate = BuildSeasonDate(seasonal.MonthStart, seasonal.DayStart, request.PeriodStart.Year),
                    EndDate = BuildSeasonDate(seasonal.MonthEnd, seasonal.DayEnd, request.PeriodStart.Year),
                    AdjustmentPercentage = rule.ValueType == "Percentage" ? rule.Value : 0,
                    Description = rule.Description ?? seasonal.SeasonName ?? "Seasonal Adjustment"
                });
            }

            return preview;
        }

        #endregion

        #region Calculation Helpers

        /// <summary>
        /// Calculate amount based on pricing rule
        /// </summary>
        private decimal CalculatePricingRuleAmount(decimal baseAmount, ErpContractPricingRule rule)
        {
            decimal amount;

            switch (rule.ValueType)
            {
                case "Percentage":
                    amount = baseAmount * rule.Value / 100;
                    break;
                case "PerUnit":
                    // Would need quantity context - use as fixed for now
                    amount = rule.Value;
                    break;
                case "Amount":
                default:
                    amount = rule.Value;
                    break;
            }

            // Apply min/max constraints
            if (rule.MinValue.HasValue && amount < rule.MinValue.Value)
            {
                amount = rule.MinValue.Value;
            }
            if (rule.MaxValue.HasValue && amount > rule.MaxValue.Value)
            {
                amount = rule.MaxValue.Value;
            }

            return Math.Round(amount, 4);
        }

        /// <summary>
        /// Calculate seasonal adjustment with period overlap
        /// </summary>
        private decimal CalculateSeasonalAdjustment(
            decimal currentAmount,
            ErpContractPricingRule rule,
            ErpContractSeasonalRule seasonal,
            DateTime periodStart,
            DateTime periodEnd)
        {
            if (seasonal == null) return 0;

            // Build effective season dates for the billing period year
            var seasonStart = BuildSeasonDate(seasonal.MonthStart, seasonal.DayStart, periodStart.Year);
            var seasonEnd = BuildSeasonDate(seasonal.MonthEnd, seasonal.DayEnd, periodStart.Year);

            // Handle year-spanning seasons
            if (seasonEnd < seasonStart)
            {
                if (periodStart.Month >= (seasonal.MonthStart ?? 1))
                {
                    seasonEnd = seasonEnd.AddYears(1);
                }
                else
                {
                    seasonStart = seasonStart.AddYears(-1);
                }
            }

            // Calculate overlap
            var overlapStart = periodStart > seasonStart ? periodStart : seasonStart;
            var overlapEnd = periodEnd < seasonEnd ? periodEnd : seasonEnd;

            if (overlapStart > overlapEnd)
            {
                return 0; // No overlap
            }

            var periodDays = (periodEnd - periodStart).Days + 1;
            var overlapDays = (overlapEnd - overlapStart).Days + 1;
            var overlapRatio = (decimal)overlapDays / periodDays;

            var adjustment = CalculatePricingRuleAmount(currentAmount, rule) * overlapRatio;
            return Math.Round(adjustment, 4);
        }

        /// <summary>
        /// Check if a seasonal rule overlaps with the given period
        /// </summary>
        private bool IsSeasonOverlapping(ErpContractSeasonalRule seasonal, DateTime periodStart, DateTime periodEnd)
        {
            if (seasonal.MonthStart == null || seasonal.MonthEnd == null)
                return false;

            var seasonStart = BuildSeasonDate(seasonal.MonthStart, seasonal.DayStart, periodStart.Year);
            var seasonEnd = BuildSeasonDate(seasonal.MonthEnd, seasonal.DayEnd, periodStart.Year);

            // Handle year-spanning seasons
            if (seasonEnd < seasonStart)
            {
                seasonEnd = seasonEnd.AddYears(1);
            }

            return seasonStart <= periodEnd && seasonEnd >= periodStart;
        }

        /// <summary>
        /// Build a date from month/day/year
        /// </summary>
        private DateTime BuildSeasonDate(int? month, int? day, int year)
        {
            var m = month ?? 1;
            var d = Math.Min(day ?? 1, DateTime.DaysInMonth(year, m));
            return new DateTime(year, m, d);
        }

        /// <summary>
        /// Generate description for a pricing rule
        /// </summary>
        private string GetRuleDescription(ErpContractPricingRule rule)
        {
            var typeDesc = rule.RuleType switch
            {
                PricingRuleType.Fixed => "Fixed",
                PricingRuleType.Variable => "Variable",
                PricingRuleType.Seasonal => "Seasonal",
                PricingRuleType.AreaBased => "Area Based",
                PricingRuleType.PercentageDiscount => "Percentage Discount",
                PricingRuleType.FixedDiscount => "Fixed Discount",
                PricingRuleType.AdvancePaymentDiscount => "Advance Payment Discount",
                PricingRuleType.Tiered => "Tiered",
                PricingRuleType.Escalation => "Escalation",
                _ => "Adjustment"
            };

            var valueDesc = rule.ValueType == "Percentage"
                ? $"{rule.Value:N2}%"
                : $"{rule.Value:N4} {rule.Currency ?? "SAR"}";

            return $"{typeDesc}: {valueDesc}";
        }

        #endregion
    }
}
