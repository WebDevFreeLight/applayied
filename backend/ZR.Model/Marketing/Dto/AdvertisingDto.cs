using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Ad Campaign DTOs

    public class AdCampaignQueryDto : PagerInfo
    {
        public string CampaignName { get; set; }
        public string Platform { get; set; }
        public string Objective { get; set; }
        public string Status { get; set; }
        public long? ParentCampaignId { get; set; }
        public long? AudienceSegmentId { get; set; }
        public bool? AiOptimizationEnabled { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class AdCampaignDto
    {
        public long AdCampaignId { get; set; }
        public string CampaignCode { get; set; }
        public string CampaignName { get; set; }
        public string CampaignNameAr { get; set; }
        public long? ParentCampaignId { get; set; }
        public string Platform { get; set; }
        public string Objective { get; set; }
        public decimal DailyBudget { get; set; }
        public decimal TotalBudget { get; set; }
        public string Currency { get; set; }
        public string BiddingStrategy { get; set; }
        public decimal? BidAmount { get; set; }
        public string TargetingJson { get; set; }
        public string PlacementsJson { get; set; }
        public string ScheduleJson { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool AiOptimizationEnabled { get; set; }
        public long? AudienceSegmentId { get; set; }
    }

    public class AdCampaignOutputDto : AdCampaignDto
    {
        public decimal SpentAmount { get; set; }
        public decimal BudgetUtilization { get; set; }
        public string ExternalId { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public long Reach { get; set; }
        public long VideoViews { get; set; }
        public decimal? Ctr { get; set; }
        public decimal? Cpc { get; set; }
        public decimal? Cpm { get; set; }
        public decimal? Cpa { get; set; }
        public decimal? Roas { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? QualityScore { get; set; }
        public string AiOptimizationJson { get; set; }
        public string ParentCampaignName { get; set; }
        public string SegmentName { get; set; }
        public int CreativeCount { get; set; }
    }

    #endregion

    #region Ad Creative DTOs

    public class AdCreativeQueryDto : PagerInfo
    {
        public long? AdCampaignId { get; set; }
        public string CreativeType { get; set; }
        public string Status { get; set; }
        public bool? IsAiGenerated { get; set; }
    }

    public class AdCreativeDto
    {
        public long CreativeId { get; set; }
        public long AdCampaignId { get; set; }
        public string CreativeName { get; set; }
        public string CreativeType { get; set; }
        public string Headline { get; set; }
        public string HeadlineAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string CallToAction { get; set; }
        public string DestinationUrl { get; set; }
        public string MediaJson { get; set; }
        public string PrimaryImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public string Status { get; set; }
    }

    public class AdCreativeOutputDto : AdCreativeDto
    {
        public string ExternalCreativeId { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal? Ctr { get; set; }
        public decimal? EngagementRate { get; set; }
        public bool IsAiGenerated { get; set; }
        public decimal? AiQualityScore { get; set; }
        public bool CulturalComplianceChecked { get; set; }
        public string CampaignName { get; set; }
    }

    #endregion

    #region Ad Performance DTOs

    public class AdPerformanceQueryDto
    {
        public long AdCampaignId { get; set; }
        public long? CreativeId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class AdPerformanceDto
    {
        public long PerformanceId { get; set; }
        public long AdCampaignId { get; set; }
        public long? CreativeId { get; set; }
        public DateTime RecordDate { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public long Reach { get; set; }
        public long VideoViews { get; set; }
        public decimal Spend { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Ctr { get; set; }
        public decimal? Cpc { get; set; }
        public decimal? Cpm { get; set; }
        public decimal? Cpa { get; set; }
        public decimal? Roas { get; set; }
        public decimal? QualityScore { get; set; }
        public string BreakdownJson { get; set; }
    }

    #endregion

    #region Ad Analytics DTOs

    public class AdAnalyticsDto
    {
        public int TotalAdCampaigns { get; set; }
        public int ActiveAdCampaigns { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public long TotalImpressions { get; set; }
        public long TotalClicks { get; set; }
        public long TotalConversions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal OverallCtr { get; set; }
        public decimal OverallCpc { get; set; }
        public decimal OverallCpa { get; set; }
        public decimal OverallRoas { get; set; }
        public List<PlatformAdSummaryDto> PlatformBreakdown { get; set; }
        public List<AdTrendPointDto> DailyTrend { get; set; }
        public List<TopAdCampaignDto> TopCampaigns { get; set; }
    }

    public class PlatformAdSummaryDto
    {
        public string Platform { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal? Ctr { get; set; }
        public decimal? Roas { get; set; }
    }

    public class AdTrendPointDto
    {
        public DateTime Date { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public long Conversions { get; set; }
        public decimal Spend { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopAdCampaignDto
    {
        public long AdCampaignId { get; set; }
        public string CampaignName { get; set; }
        public string Platform { get; set; }
        public decimal? Roas { get; set; }
        public decimal Revenue { get; set; }
        public long Conversions { get; set; }
    }

    #endregion
}
