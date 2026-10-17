using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    /// <summary>
    /// Campaign Query Parameters
    /// </summary>
    public class CampaignQueryDto : PagerInfo
    {
        public string CampaignCode { get; set; }
        public string CampaignName { get; set; }
        public string CampaignType { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string TargetAudience { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public long? EventId { get; set; }
        public long? ManagerId { get; set; }
        public bool? AiOptimizationEnabled { get; set; }
    }

    /// <summary>
    /// Campaign Create/Update DTO
    /// </summary>
    public class CampaignDto
    {
        public long CampaignId { get; set; }
        public string CampaignCode { get; set; }
        public string CampaignName { get; set; }
        public string CampaignNameAr { get; set; }
        public string CampaignType { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public decimal TotalBudget { get; set; }
        public string Currency { get; set; }
        public string TargetAudience { get; set; }
        public string TargetSegmentsJson { get; set; }
        public string GoalsJson { get; set; }
        public string Priority { get; set; }
        public string TagsJson { get; set; }
        public bool AiOptimizationEnabled { get; set; }
        public long? EventId { get; set; }
        public long? ManagerId { get; set; }
        public List<CampaignChannelDto> Channels { get; set; }
    }

    /// <summary>
    /// Campaign Response with computed fields
    /// </summary>
    public class CampaignOutputDto : CampaignDto
    {
        public decimal SpentAmount { get; set; }
        public decimal BudgetUtilization { get; set; }
        public int RemainingDays { get; set; }
        public decimal? AiConfidenceScore { get; set; }
        public string AiRecommendationsJson { get; set; }
        public string ManagerName { get; set; }
        public List<CampaignChannelDto> Channels { get; set; }

        // Aggregated performance
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public long TotalConversions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? OverallRoi { get; set; }
    }

    /// <summary>
    /// Campaign Channel DTO
    /// </summary>
    public class CampaignChannelDto
    {
        public long ChannelId { get; set; }
        public long CampaignId { get; set; }
        public string ChannelType { get; set; }
        public decimal ChannelBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public string Status { get; set; }
        public string ExternalCampaignId { get; set; }
        public string ConfigJson { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal? CostPerClick { get; set; }
        public decimal? CostPerConversion { get; set; }
        public decimal? ReturnOnAdSpend { get; set; }
    }

    /// <summary>
    /// Campaign Performance DTO
    /// </summary>
    public class CampaignPerformanceDto
    {
        public long PerformanceId { get; set; }
        public long CampaignId { get; set; }
        public long? ChannelId { get; set; }
        public DateTime RecordDate { get; set; }
        public long Impressions { get; set; }
        public long Reach { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public long Engagements { get; set; }
        public decimal Revenue { get; set; }
        public decimal Spend { get; set; }
        public decimal? ClickThroughRate { get; set; }
        public decimal? ConversionRate { get; set; }
        public decimal? CostPerClick { get; set; }
        public decimal? ReturnOnInvestment { get; set; }
    }

    /// <summary>
    /// Campaign Performance Query
    /// </summary>
    public class CampaignPerformanceQueryDto
    {
        public long CampaignId { get; set; }
        public long? ChannelId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Granularity { get; set; } = "daily";
    }

    /// <summary>
    /// A/B Test DTO
    /// </summary>
    public class AbTestDto
    {
        public long TestId { get; set; }
        public long CampaignId { get; set; }
        public string TestName { get; set; }
        public string TestNameAr { get; set; }
        public string TestElement { get; set; }
        public string VariantAJson { get; set; }
        public string VariantBJson { get; set; }
        public int SplitPercentage { get; set; }
        public string WinnerMetric { get; set; }
        public string Status { get; set; }
        public string Winner { get; set; }
        public decimal? Confidence { get; set; }
        public string ResultsJson { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SampleSizeA { get; set; }
        public int SampleSizeB { get; set; }
    }

    /// <summary>
    /// Campaign Analytics Summary
    /// </summary>
    public class CampaignAnalyticsDto
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal OverallRoi { get; set; }
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public long TotalConversions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgConversionRate { get; set; }
        public decimal AvgCostPerClick { get; set; }
        public decimal AvgCostPerConversion { get; set; }
        public List<ChannelPerformanceSummary> ChannelBreakdown { get; set; }
        public List<CampaignTrendPoint> DailyTrend { get; set; }
        public List<TopCampaignDto> TopCampaigns { get; set; }
    }

    public class ChannelPerformanceSummary
    {
        public string ChannelType { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal? Roi { get; set; }
    }

    public class CampaignTrendPoint
    {
        public DateTime Date { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopCampaignDto
    {
        public long CampaignId { get; set; }
        public string CampaignName { get; set; }
        public decimal Roi { get; set; }
        public decimal Revenue { get; set; }
        public long Conversions { get; set; }
    }
}
