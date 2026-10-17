using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Campaigns
{
    /// <summary>
    /// Marketing Campaign Entity - الحملات التسويقية
    /// Core campaign management with multi-channel support and AI optimization
    /// </summary>
    [SugarTable("mkt_campaign")]
    public class MktCampaign : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CampaignId { get; set; }

        [SugarColumn(Length = 100)]
        public string CampaignCode { get; set; }

        [SugarColumn(Length = 300)]
        public string CampaignName { get; set; }

        [SugarColumn(Length = 300)]
        public string CampaignNameAr { get; set; }

        /// <summary>
        /// Type: awareness, lead_generation, conversion, retention, event_promotion, sponsor_attraction, crowd_engagement
        /// </summary>
        [SugarColumn(Length = 50)]
        public string CampaignType { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DescriptionAr { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        /// <summary>
        /// Status: draft, scheduled, active, paused, completed, cancelled
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Status { get; set; } = "draft";

        [SugarColumn(DecimalDigits = 2)]
        public decimal TotalBudget { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal SpentAmount { get; set; } = 0;

        [SugarColumn(Length = 10)]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Target Audience: all, vip, new_attendees, returning, investors, merchants, exhibitors
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TargetAudience { get; set; } = "all";

        /// <summary>
        /// JSON: audience segment IDs for AI targeting
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TargetSegmentsJson { get; set; }

        /// <summary>
        /// JSON: goal metrics (impressions, clicks, conversions, revenue targets)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string GoalsJson { get; set; }

        /// <summary>
        /// Priority: low, medium, high, critical
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// JSON: tags for categorization
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TagsJson { get; set; }

        /// <summary>
        /// AI optimization enabled
        /// </summary>
        public bool AiOptimizationEnabled { get; set; } = false;

        /// <summary>
        /// AI confidence score (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiConfidenceScore { get; set; }

        /// <summary>
        /// AI recommended actions JSON
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiRecommendationsJson { get; set; }

        /// <summary>
        /// Related Event ID (for event-specific campaigns)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Assigned Manager Employee ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ManagerId { get; set; }

        // CompanyId and BranchId are inherited from SysBase

        // Navigation
        [SugarColumn(IsIgnore = true)]
        public List<MktCampaignChannel> Channels { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<MktCampaignPerformance> PerformanceRecords { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ManagerName { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public decimal BudgetUtilization => TotalBudget > 0 ? (SpentAmount / TotalBudget) * 100 : 0;

        [SugarColumn(IsIgnore = true)]
        public bool IsActive => Status == "active";

        [SugarColumn(IsIgnore = true)]
        public int RemainingDays => EndDate > DateTime.Now ? (int)(EndDate - DateTime.Now).TotalDays : 0;
    }

    /// <summary>
    /// Campaign Channel - قنوات الحملة
    /// Links campaigns to specific marketing channels with individual budgets
    /// </summary>
    [SugarTable("mkt_campaign_channel")]
    public class MktCampaignChannel : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ChannelId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CampaignId { get; set; }

        /// <summary>
        /// Channel: google_ads, facebook, instagram, twitter, linkedin, snapchat, tiktok, email, sms, whatsapp, push_notification, billboard, print, radio, tv
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ChannelType { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal ChannelBudget { get; set; }

        [SugarColumn(DecimalDigits = 2)]
        public decimal SpentAmount { get; set; } = 0;

        /// <summary>
        /// Status: active, paused, completed
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "active";

        /// <summary>
        /// External platform campaign ID
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ExternalCampaignId { get; set; }

        /// <summary>
        /// JSON: channel-specific configuration (ad group IDs, targeting settings)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ConfigJson { get; set; }

        [SugarColumn(IsNullable = true)]
        public long Impressions { get; set; } = 0;

        [SugarColumn(IsNullable = true)]
        public long Clicks { get; set; } = 0;

        [SugarColumn(IsNullable = true)]
        public long Conversions { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? CostPerClick { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? CostPerConversion { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ReturnOnAdSpend { get; set; }
    }

    /// <summary>
    /// Campaign Performance Tracking - تتبع أداء الحملة
    /// Daily performance snapshots for campaigns
    /// </summary>
    [SugarTable("mkt_campaign_performance")]
    public class MktCampaignPerformance
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PerformanceId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CampaignId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ChannelId { get; set; }

        public DateTime RecordDate { get; set; }

        public long Impressions { get; set; } = 0;
        public long Reach { get; set; } = 0;
        public long Clicks { get; set; } = 0;
        public long Conversions { get; set; } = 0;
        public long Engagements { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2)]
        public decimal Revenue { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2)]
        public decimal Spend { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ClickThroughRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ConversionRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? CostPerClick { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? ReturnOnInvestment { get; set; }

        public long NewSubscribers { get; set; } = 0;
        public long Unsubscribes { get; set; } = 0;
        public long BounceCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// A/B Testing - اختبار A/B
    /// </summary>
    [SugarTable("mkt_ab_test")]
    public class MktAbTest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TestId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CampaignId { get; set; }

        [SugarColumn(Length = 200)]
        public string TestName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string TestNameAr { get; set; }

        /// <summary>
        /// Test element: subject_line, content, cta, image, audience, send_time, landing_page
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TestElement { get; set; }

        /// <summary>
        /// JSON: variant A configuration
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string VariantAJson { get; set; }

        /// <summary>
        /// JSON: variant B configuration
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string VariantBJson { get; set; }

        /// <summary>
        /// Traffic split percentage for variant A (0-100)
        /// </summary>
        public int SplitPercentage { get; set; } = 50;

        /// <summary>
        /// Winner metric: click_rate, conversion_rate, revenue, engagement
        /// </summary>
        [SugarColumn(Length = 50)]
        public string WinnerMetric { get; set; }

        /// <summary>
        /// Status: draft, running, completed, cancelled
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        /// <summary>
        /// Winner: variant_a, variant_b, inconclusive
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string Winner { get; set; }

        /// <summary>
        /// Statistical confidence (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? Confidence { get; set; }

        /// <summary>
        /// JSON: results data
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ResultsJson { get; set; }

        public DateTime StartDate { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? EndDate { get; set; }

        public int SampleSizeA { get; set; } = 0;
        public int SampleSizeB { get; set; } = 0;
    }

    /// <summary>
    /// Campaign Type Constants
    /// </summary>
    public static class CampaignTypes
    {
        public const string AWARENESS = "awareness";
        public const string LEAD_GENERATION = "lead_generation";
        public const string CONVERSION = "conversion";
        public const string RETENTION = "retention";
        public const string EVENT_PROMOTION = "event_promotion";
        public const string SPONSOR_ATTRACTION = "sponsor_attraction";
        public const string CROWD_ENGAGEMENT = "crowd_engagement";
    }

    public static class CampaignStatus
    {
        public const string DRAFT = "draft";
        public const string SCHEDULED = "scheduled";
        public const string ACTIVE = "active";
        public const string PAUSED = "paused";
        public const string COMPLETED = "completed";
        public const string CANCELLED = "cancelled";
    }

    public static class ChannelTypes
    {
        public const string GOOGLE_ADS = "google_ads";
        public const string FACEBOOK = "facebook";
        public const string INSTAGRAM = "instagram";
        public const string TWITTER = "twitter";
        public const string LINKEDIN = "linkedin";
        public const string SNAPCHAT = "snapchat";
        public const string TIKTOK = "tiktok";
        public const string EMAIL = "email";
        public const string SMS = "sms";
        public const string WHATSAPP = "whatsapp";
        public const string PUSH_NOTIFICATION = "push_notification";
    }
}
