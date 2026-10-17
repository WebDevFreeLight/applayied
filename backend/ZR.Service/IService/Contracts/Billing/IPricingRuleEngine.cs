using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.Contracts;
using ZR.Model.Contracts.Enums;

namespace ZR.Service.IService.Contracts.Billing
{
    /// <summary>
    /// IPricingRuleEngine - Pricing Rule Engine Interface
    /// محرك قواعد التسعير
    /// Applies pricing rules, escalations, and seasonal adjustments to billing amounts
    /// </summary>
    public interface IPricingRuleEngine
    {
        /// <summary>
        /// Apply all applicable pricing rules to a base amount
        /// Includes escalations and seasonal adjustments
        /// </summary>
        Task<PricingCalculationResult> CalculatePriceAsync(PricingCalculationRequest request);

        /// <summary>
        /// Apply escalation rules for a specific date
        /// </summary>
        Task<decimal> ApplyEscalationAsync(Guid contractId, decimal baseAmount, DateTime effectiveDate);

        /// <summary>
        /// Apply seasonal rules for a specific period
        /// </summary>
        Task<decimal> ApplySeasonalAdjustmentAsync(Guid contractId, decimal amount, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Get active escalation rules for a contract
        /// </summary>
        Task<List<ErpContractEscalationRule>> GetActiveEscalationsAsync(Guid contractId, DateTime asOfDate);

        /// <summary>
        /// Get active seasonal rules for a contract
        /// </summary>
        Task<List<ErpContractSeasonalRule>> GetActiveSeasonalRulesAsync(Guid contractId, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Calculate cumulative escalation percentage from contract start to date
        /// </summary>
        Task<decimal> GetCumulativeEscalationPercentageAsync(Guid contractId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Preview price after all adjustments without applying
        /// </summary>
        Task<PricingPreviewResult> PreviewPriceAsync(PricingCalculationRequest request);
    }

    /// <summary>
    /// Request for pricing calculation
    /// </summary>
    public class PricingCalculationRequest
    {
        public Guid ContractId { get; set; }
        public Guid? UnitId { get; set; }
        public decimal BaseAmount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime BillingDate { get; set; }
        public bool ApplyEscalations { get; set; } = true;
        public bool ApplySeasonalRules { get; set; } = true;
        public bool ApplyDiscounts { get; set; } = true;
        public string Currency { get; set; } = "SAR";
    }

    /// <summary>
    /// Result of pricing calculation
    /// </summary>
    public class PricingCalculationResult
    {
        public decimal BaseAmount { get; set; }
        public decimal EscalationAmount { get; set; }
        public decimal SeasonalAdjustment { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal TotalAdjustmentPercentage { get; set; }
        public string Currency { get; set; }
        public List<PricingAdjustmentDetail> AdjustmentDetails { get; set; } = new();
        public bool HasWarnings { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Detail of a single pricing adjustment
    /// </summary>
    public class PricingAdjustmentDetail
    {
        public string AdjustmentType { get; set; } // Escalation, Seasonal, Discount
        public string RuleName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; }
        public int ApplicationOrder { get; set; }
    }

    /// <summary>
    /// Result of pricing preview
    /// </summary>
    public class PricingPreviewResult : PricingCalculationResult
    {
        public List<EscalationPreview> FutureEscalations { get; set; } = new();
        public List<SeasonalPreview> ApplicableSeasons { get; set; } = new();
    }

    /// <summary>
    /// Preview of future escalation
    /// </summary>
    public class EscalationPreview
    {
        public DateTime EffectiveDate { get; set; }
        public decimal Percentage { get; set; }
        public decimal ProjectedAmount { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Preview of seasonal adjustment
    /// </summary>
    public class SeasonalPreview
    {
        public string SeasonName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AdjustmentPercentage { get; set; }
        public string Description { get; set; }
    }
}
