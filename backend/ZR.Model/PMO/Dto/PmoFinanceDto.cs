using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    // ---------- Cost transactions ----------
    public class PmoCostTransactionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? CategoryId { get; set; }
        public string Status { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Keyword { get; set; }
    }

    public class PmoCostTransactionInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long TransactionId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long CategoryId { get; set; }

        [Range(0.01, 9999999999, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        public DateTime TxnDate { get; set; }
        public string Description { get; set; }
    }

    // ---------- Budget revisions ----------
    public class PmoBudgetRevisionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
        public string Status { get; set; }
    }

    public class PmoBudgetRevisionInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long RevisionId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "A rationale is required for a budget revision")]
        public string Reason { get; set; }
    }

    public class PmoBudgetRevisionDecisionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long RevisionId { get; set; }
        /// <summary>true = Approve, false = Reject.</summary>
        public bool Approve { get; set; }
        public string Reason { get; set; }
    }

    // ---------- Forecasts ----------
    public class PmoForecastInputDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ForecastId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public DateTime PeriodMonth { get; set; }
        public decimal ForecastAmount { get; set; }
        /// <summary>MonthlyEstimate | ForecastToComplete</summary>
        public string Kind { get; set; } = "ForecastToComplete";
    }

    // ---------- Derived summary result models (NOT persisted) ----------
    public class PmoCategorySpendDto
    {
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
    }

    public class PmoMonthlySpendDto
    {
        public string Period { get; set; }   // yyyy-MM
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Project financial summary — every value is DERIVED from SQL at the given reporting cutoff.
    /// </summary>
    public class PmoFinancialSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Currency { get; set; } = "AED";
        public DateTime Cutoff { get; set; }

        public decimal InitialApprovedBudget { get; set; }
        public decimal ApprovedRevisions { get; set; }
        public decimal CurrentApprovedBudget { get; set; }   // initial + approved revisions
        public decimal ActualSpend { get; set; }             // posted, txn_date <= cutoff
        public decimal RemainingBudget { get; set; }         // current - actual
        public decimal? UtilizationPct { get; set; }         // actual / current (null if budget = 0)
        public decimal ForecastToComplete { get; set; }
        public decimal Eac { get; set; }                     // actual + forecast-to-complete
        public decimal ProjectedVariance { get; set; }       // current - EAC (negative => overrun)
        public bool IsOverBudget { get; set; }               // actual > current
        public bool IsProjectedOverrun { get; set; }         // EAC > current
        public bool HasForecast { get; set; }

        public List<PmoCategorySpendDto> SpendByCategory { get; set; } = new();
        public List<PmoMonthlySpendDto> MonthlyTrend { get; set; } = new();
    }
}
