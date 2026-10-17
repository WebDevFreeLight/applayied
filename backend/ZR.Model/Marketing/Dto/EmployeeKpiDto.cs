using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Employee KPI (Key Performance Indicators) DTO
    /// Contains comprehensive performance metrics for marketing team members
    /// </summary>
    public class EmployeeKpiDto
    {
        /// <summary>
        /// Employee ID
        /// </summary>
        public long EmployeeId { get; set; }

        /// <summary>
        /// Employee Name
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// Period for which KPIs are calculated (e.g., "Last 30 Days", "This Month")
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Lead Generation Metrics
        /// </summary>
        public LeadMetrics LeadMetrics { get; set; }

        /// <summary>
        /// Conversion Metrics
        /// </summary>
        public ConversionMetrics ConversionMetrics { get; set; }

        /// <summary>
        /// Activity Metrics
        /// </summary>
        public ActivityMetrics ActivityMetrics { get; set; }

        /// <summary>
        /// Performance Trends
        /// </summary>
        public PerformanceTrends Trends { get; set; }

        /// <summary>
        /// Quality Metrics
        /// </summary>
        public QualityMetrics QualityMetrics { get; set; }
    }

    /// <summary>
    /// Lead Generation Metrics
    /// </summary>
    public class LeadMetrics
    {
        /// <summary>
        /// Total number of leads assigned to employee
        /// </summary>
        public int TotalLeads { get; set; }

        /// <summary>
        /// New leads in current period
        /// </summary>
        public int NewLeads { get; set; }

        /// <summary>
        /// Leads breakdown by status
        /// </summary>
        public Dictionary<string, int> LeadsByStatus { get; set; }

        /// <summary>
        /// Leads breakdown by source
        /// </summary>
        public Dictionary<string, int> LeadsBySource { get; set; }

        /// <summary>
        /// Leads breakdown by priority
        /// </summary>
        public Dictionary<string, int> LeadsByPriority { get; set; }
    }

    /// <summary>
    /// Conversion Metrics
    /// </summary>
    public class ConversionMetrics
    {
        /// <summary>
        /// Number of converted leads
        /// </summary>
        public int ConvertedLeads { get; set; }

        /// <summary>
        /// Number of lost leads
        /// </summary>
        public int LostLeads { get; set; }

        /// <summary>
        /// Conversion rate (percentage)
        /// </summary>
        public decimal ConversionRate { get; set; }

        /// <summary>
        /// Lost leads rate (percentage)
        /// </summary>
        public decimal LostRate { get; set; }

        /// <summary>
        /// Total expected revenue from converted leads
        /// </summary>
        public decimal TotalExpectedRevenue { get; set; }

        /// <summary>
        /// Average deal size
        /// </summary>
        public decimal AverageDealSize { get; set; }

        /// <summary>
        /// Average time to convert (in days)
        /// </summary>
        public decimal AverageTimeToConvert { get; set; }
    }

    /// <summary>
    /// Activity Metrics
    /// </summary>
    public class ActivityMetrics
    {
        /// <summary>
        /// Total number of activities logged
        /// </summary>
        public int TotalActivities { get; set; }

        /// <summary>
        /// Activities in current period
        /// </summary>
        public int RecentActivities { get; set; }

        /// <summary>
        /// Activities breakdown by type
        /// </summary>
        public Dictionary<string, int> ActivitiesByType { get; set; }

        /// <summary>
        /// Activities breakdown by outcome
        /// </summary>
        public Dictionary<string, int> ActivitiesByOutcome { get; set; }

        /// <summary>
        /// Average activities per lead
        /// </summary>
        public decimal ActivitiesPerLead { get; set; }

        /// <summary>
        /// Follow-up compliance rate (percentage)
        /// </summary>
        public decimal FollowUpComplianceRate { get; set; }
    }

    /// <summary>
    /// Performance Trends (time-series data)
    /// </summary>
    public class PerformanceTrends
    {
        /// <summary>
        /// Weekly lead generation trend (last 12 weeks)
        /// </summary>
        public List<TrendDataPoint> WeeklyLeadsTrend { get; set; }

        /// <summary>
        /// Weekly conversion trend
        /// </summary>
        public List<TrendDataPoint> WeeklyConversionTrend { get; set; }

        /// <summary>
        /// Weekly activity trend
        /// </summary>
        public List<TrendDataPoint> WeeklyActivityTrend { get; set; }

        /// <summary>
        /// Monthly revenue trend (last 6 months)
        /// </summary>
        public List<TrendDataPoint> MonthlyRevenueTrend { get; set; }
    }

    /// <summary>
    /// Quality Metrics
    /// </summary>
    public class QualityMetrics
    {
        /// <summary>
        /// Qualified leads rate (percentage)
        /// </summary>
        public decimal QualificationRate { get; set; }

        /// <summary>
        /// Average response time (in hours)
        /// </summary>
        public decimal AverageResponseTime { get; set; }

        /// <summary>
        /// Average days since last contact for active leads
        /// </summary>
        public decimal AverageDaysSinceLastContact { get; set; }

        /// <summary>
        /// Number of stale leads (no contact in 30+ days)
        /// </summary>
        public int StaleLeads { get; set; }
    }

    /// <summary>
    /// Trend Data Point for time-series charts
    /// </summary>
    public class TrendDataPoint
    {
        /// <summary>
        /// Period label (e.g., "Week 1", "Jan 2024")
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Date for the period
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Value for the metric
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Additional value (for dual-axis charts)
        /// </summary>
        public decimal? Value2 { get; set; }
    }
}
