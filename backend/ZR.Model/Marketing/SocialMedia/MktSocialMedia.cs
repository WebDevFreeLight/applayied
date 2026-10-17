using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.SocialMedia
{
    /// <summary>
    /// Social Media Account - حسابات التواصل الاجتماعي
    /// Connected social media accounts for centralized management
    /// </summary>
    [SugarTable("mkt_social_account")]
    public class MktSocialAccount : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AccountId { get; set; }

        /// <summary>
        /// Platform: facebook, instagram, twitter, linkedin, snapchat, tiktok, youtube, threads
        /// </summary>
        [SugarColumn(Length = 30)]
        public string Platform { get; set; }

        [SugarColumn(Length = 200)]
        public string AccountName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AccountHandle { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string ProfileUrl { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Encrypted access token for API integration
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AccessTokenEncrypted { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? TokenExpiresAt { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string RefreshTokenEncrypted { get; set; }

        public long FollowersCount { get; set; } = 0;
        public long FollowingCount { get; set; } = 0;
        public long PostsCount { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? GrowthRate { get; set; }

        /// <summary>
        /// Connection status: connected, disconnected, expired, error
        /// </summary>
        [SugarColumn(Length = 20)]
        public string ConnectionStatus { get; set; } = "disconnected";

        [SugarColumn(IsNullable = true)]
        public DateTime? LastSyncAt { get; set; }

        /// <summary>
        /// JSON: platform-specific settings and configurations
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string PlatformConfigJson { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Social Media Post - المنشورات
    /// Scheduled and published social media posts
    /// </summary>
    [SugarTable("mkt_social_post")]
    public class MktSocialPost : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PostId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AccountId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CampaignId { get; set; }

        [SugarColumn(ColumnDataType = "TEXT")]
        public string Content { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ContentAr { get; set; }

        /// <summary>
        /// Type: text, image, video, carousel, story, reel, live, poll
        /// </summary>
        [SugarColumn(Length = 20)]
        public string PostType { get; set; } = "text";

        /// <summary>
        /// JSON: media URLs (images, videos)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MediaJson { get; set; }

        /// <summary>
        /// JSON: hashtags
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string HashtagsJson { get; set; }

        /// <summary>
        /// JSON: mentioned accounts
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string MentionsJson { get; set; }

        /// <summary>
        /// Link included in post
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string LinkUrl { get; set; }

        /// <summary>
        /// Status: draft, scheduled, publishing, published, failed, deleted
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        [SugarColumn(IsNullable = true)]
        public DateTime? ScheduledAt { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// External post ID from the platform
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ExternalPostId { get; set; }

        /// <summary>
        /// External post URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string ExternalPostUrl { get; set; }

        // Engagement metrics
        public long Likes { get; set; } = 0;
        public long Comments { get; set; } = 0;
        public long Shares { get; set; } = 0;
        public long Views { get; set; } = 0;
        public long Reach { get; set; } = 0;
        public long Impressions { get; set; } = 0;
        public long Saves { get; set; } = 0;
        public long Clicks { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        /// <summary>
        /// AI-generated content flag
        /// </summary>
        public bool IsAiGenerated { get; set; } = false;

        /// <summary>
        /// AI content quality score
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiQualityScore { get; set; }

        /// <summary>
        /// JSON: AI suggested optimal posting time
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiSuggestionsJson { get; set; }

        /// <summary>
        /// Sentiment of comments: positive, neutral, negative, mixed
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string CommentSentiment { get; set; }

        /// <summary>
        /// Saudi cultural compliance checked
        /// </summary>
        public bool CulturalComplianceChecked { get; set; } = false;

        /// <summary>
        /// Approved by for compliance
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ApprovedBy { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Social Media Analytics - تحليلات التواصل الاجتماعي
    /// Aggregated analytics per account per period
    /// </summary>
    [SugarTable("mkt_social_analytics")]
    public class MktSocialAnalytics
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AnalyticsId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long AccountId { get; set; }

        /// <summary>
        /// Period: daily, weekly, monthly
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Period { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        // Follower metrics
        public long FollowersStart { get; set; } = 0;
        public long FollowersEnd { get; set; } = 0;
        public long FollowersGained { get; set; } = 0;
        public long FollowersLost { get; set; } = 0;

        // Content metrics
        public int PostsPublished { get; set; } = 0;
        public long TotalImpressions { get; set; } = 0;
        public long TotalReach { get; set; } = 0;
        public long TotalEngagements { get; set; } = 0;
        public long TotalClicks { get; set; } = 0;
        public long TotalViews { get; set; } = 0;

        // Calculated rates
        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? AvgEngagementRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? AvgReachRate { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? GrowthRate { get; set; }

        /// <summary>
        /// JSON: top performing posts
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TopPostsJson { get; set; }

        /// <summary>
        /// JSON: audience demographics from platform
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AudienceDemographicsJson { get; set; }

        /// <summary>
        /// JSON: best posting times based on engagement
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BestTimesJson { get; set; }

        /// <summary>
        /// JSON: content type performance breakdown
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ContentTypePerformanceJson { get; set; }

        /// <summary>
        /// Sentiment overview: positive_pct, neutral_pct, negative_pct
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string SentimentJson { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }

        // Computed
        [SugarColumn(IsIgnore = true)]
        public long NetFollowerGrowth => FollowersGained - FollowersLost;
    }
}
