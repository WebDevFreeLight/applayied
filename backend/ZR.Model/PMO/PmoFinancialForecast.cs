using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Monthly forecast input. ForecastToComplete rows feed EAC = actual spend + Σ(forecast-to-complete).
    /// </summary>
    [SugarTable("pmo_financial_forecast", "PMO Financial Forecast")]
    [SugarIndex("uk_pmo_forecast", nameof(ProjectId), OrderByType.Asc, nameof(PeriodMonth), OrderByType.Asc, nameof(Kind), OrderByType.Asc, true)]
    public class PmoFinancialForecast : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "forecast_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ForecastId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        /// <summary>First day of the forecast month.</summary>
        [SugarColumn(ColumnName = "period_month")]
        public DateTime PeriodMonth { get; set; }

        [SugarColumn(ColumnName = "forecast_amount", Length = 18, DecimalDigits = 2)]
        public decimal ForecastAmount { get; set; }

        /// <summary>MonthlyEstimate | ForecastToComplete</summary>
        [SugarColumn(ColumnName = "kind", Length = 20)]
        public string Kind { get; set; } = "ForecastToComplete";

        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
    }
}
