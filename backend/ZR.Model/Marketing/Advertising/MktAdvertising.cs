using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Advertising
{
    /// <summary>
    /// Ad Campaign - الحملات الإعلانية
    /// Paid advertising campaigns across multiple platforms
    /// </summary>
    [SugarTable("mkt_ad_campaign")]
    public class MktAdCampaign : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdCampaignId { get; set; }

        [SugarColumn(Length = 100)]
        public string CampaignCode { get; set; }

        [SugarColumn(Length = 300)]
        public string CampaignName { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string CampaignNameAr { get; set; }

        /// <summary>
        /// Parent marketing campaign
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ParentCampaignId { get; set; }

        /// <summary>
        /// Platform: google_ads, facebook_ads, instagram_ads, linkedin_ads,
        ///           snapchat_ads, tiktok_ads, twitter_ads, youtube_ads, programmatic
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Platform { get; set; }

        /// <summary>
        /// Objective: awareness, traffic, engagement, leads, conversions, app_installs, video_views
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Objective { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal DailyBudget { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal TotalBudget { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal SpentAmount { get; set; } = 0;

        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Bidding strategy: manual_cpc, auto_cpc, target_cpa, maximize_conversions, target_roas
        /// </summary>
        [SugarColumn(Length = 30)]
        public string BiddingStrategy { get; set; } = "auto_cpc";

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? BidAmount { get; set; }

        /// <summary>
        /// JSON: targeting configuration (demographics, interests, behaviors, locations)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string TargetingJson { get; set; }

        /// <summary>
        /// JSON: placement configuration (feed, stories, search, display)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PlacementsJson { get; set; }

        /// <summary>
        /// JSON: schedule (days, hours for ad delivery)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ScheduleJson { get; set; }

        /// <summary>
        /// Status: draft, pending_review, active, paused, completed, rejected
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        public DateTime StartDate { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// External platform campaign ID
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ExternalId { get; set; }

        // Performance metrics
        public long Impressions { get; set; } = 0;
        public long Clicks { get; set; } = 0;
        public long Conversions { get; set; } = 0;
        public long Reach { get; set; } = 0;
        public long VideoViews { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Ctr { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpc { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpm { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpa { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Roas { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Revenue { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? QualityScore { get; set; }

        /// <summary>
        /// AI optimization enabled for this campaign
        /// </summary>
        public bool AiOptimizationEnabled { get; set; } = false;

        /// <summary>
        /// JSON: AI optimization history and recommendations
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiOptimizationJson { get; set; }

        /// <summary>
        /// Audience segment ID for AI targeting
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? AudienceSegmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public decimal BudgetUtilization => TotalBudget > 0 ? (SpentAmount / TotalBudget) * 100 : 0;
    }

    /// <summary>
    /// Ad Creative - التصاميم الإعلانية
    /// Individual ad creatives within ad campaigns
    /// </summary>
    [SugarTable("mkt_ad_creative")]
    public class MktAdCreative : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CreativeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdCampaignId { get; set; }

        [SugarColumn(Length = 200)]
        public string CreativeName { get; set; }

        /// <summary>
        /// Type: image, video, carousel, text, responsive, dynamic
        /// </summary>
        [SugarColumn(Length = 30)]
        public string CreativeType { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string Headline { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string HeadlineAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Call to action: learn_more, shop_now, sign_up, book_now, get_tickets, contact_us
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true)]
        public string CallToAction { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string DestinationUrl { get; set; }

        /// <summary>
        /// JSON: media assets (images, videos, carousel items)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MediaJson { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string PrimaryImageUrl { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string VideoUrl { get; set; }

        /// <summary>
        /// Status: draft, review, approved, active, paused, rejected
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        /// <summary>
        /// External platform creative ID
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ExternalCreativeId { get; set; }

        // Performance
        public long Impressions { get; set; } = 0;
        public long Clicks { get; set; } = 0;
        public long Conversions { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Ctr { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        /// <summary>
        /// AI-generated creative
        /// </summary>
        public bool IsAiGenerated { get; set; } = false;

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiQualityScore { get; set; }

        /// <summary>
        /// Saudi cultural compliance checked
        /// </summary>
        public bool CulturalComplianceChecked { get; set; } = false;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Ad Performance Snapshot - لقطات أداء الإعلانات
    /// Periodic performance snapshots for ad campaigns
    /// </summary>
    [SugarTable("mkt_ad_performance")]
    public class MktAdPerformance
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PerformanceId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdCampaignId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CreativeId { get; set; }

        public DateTime RecordDate { get; set; }

        public long Impressions { get; set; } = 0;
        public long Clicks { get; set; } = 0;
        public long Conversions { get; set; } = 0;
        public long Reach { get; set; } = 0;
        public long VideoViews { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2)]
        public decimal Spend { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Revenue { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Ctr { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpc { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpm { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? Cpa { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Roas { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? QualityScore { get; set; }

        /// <summary>
        /// JSON: breakdown by demographic, device, placement
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BreakdownJson { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }
}
