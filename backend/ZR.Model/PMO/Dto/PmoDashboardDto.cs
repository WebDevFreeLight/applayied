using System;
using System.Collections.Generic;
using Infrastructure.Converter;
using Newtonsoft.Json;

namespace ZR.Model.PMO.Dto
{
    /// <summary>Coordinated dashboard filter context (scope-aware). Applied to every KPI/visual/drill-down.</summary>
    public class PmoDashboardFilter
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long? DeptId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? ProjectId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? ManagerId { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))] public long? CategoryId { get; set; }
        /// <summary>Reporting cutoff for spend/overdue (baseline demo uses 2026-09-30; default = today).</summary>
        public DateTime? Cutoff { get; set; }
    }

    public class PmoDashboardKpis
    {
        public string Currency { get; set; } = "AED";
        public DateTime Cutoff { get; set; }
        // Delivery
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public decimal PortfolioProgress { get; set; }
        public int OverdueTasks { get; set; }
        public int MilestonesDue14 { get; set; }
        // Risk & governance
        public int OpenHighExposureRisks { get; set; }
        public int BlockedTasks { get; set; }
        public int PendingChangeRequests { get; set; }
        // Financial
        public decimal ApprovedBudget { get; set; }
        public decimal ActualSpend { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal? UtilizationPct { get; set; }
        public decimal Eac { get; set; }
        public int ProjectedOverrunCount { get; set; }
        public decimal ProjectedOverrunAmount { get; set; }
    }

    public class PmoProjectHealthRow
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public decimal Progress { get; set; }
        public decimal? UtilizationPct { get; set; }
        public bool IsProjectedOverrun { get; set; }
    }

    public class PmoBudgetVsActualRow
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public decimal ApprovedBudget { get; set; }
        public decimal ActualSpend { get; set; }
        public decimal Eac { get; set; }
    }

    public class PmoRiskMatrixCell
    {
        public int Probability { get; set; }
        public int Impact { get; set; }
        public int Count { get; set; }
    }

    public class PmoOwnerOverdueRow
    {
        public string OwnerName { get; set; }
        public int OverdueCount { get; set; }
    }

    public class PmoMilestoneRoadmapRow
    {
        [JsonConverter(typeof(ValueToStringConverter))] public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
    }

    public class PmoDashboardAnalytics
    {
        public List<PmoProjectHealthRow> ProjectHealth { get; set; } = new();
        public List<PmoBudgetVsActualRow> BudgetVsActual { get; set; } = new();
        public List<PmoMonthlySpendDto> MonthlyTrend { get; set; } = new();
        public List<PmoCategorySpendDto> SpendByCategory { get; set; } = new();
        public List<PmoRiskMatrixCell> RiskMatrix { get; set; } = new();
        public List<PmoOwnerOverdueRow> OverdueByOwner { get; set; } = new();
        public List<PmoMilestoneRoadmapRow> MilestoneRoadmap { get; set; } = new();
    }
}
