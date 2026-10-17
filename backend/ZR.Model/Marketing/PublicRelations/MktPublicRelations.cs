using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.PublicRelations
{
    /// <summary>
    /// Press Release - البيانات الصحفية
    /// </summary>
    [SugarTable("mkt_press_release")]
    public class MktPressRelease : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReleaseId { get; set; }

        [SugarColumn(Length = 300)]
        public string Title { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string TitleAr { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string Subtitle { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SubtitleAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT")]
        public string Body { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BodyAr { get; set; }

        /// <summary>
        /// Summary/Abstract
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Summary { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string SummaryAr { get; set; }

        /// <summary>
        /// Category: event_announcement, partnership, achievement, product_launch,
        ///           financial, community, sustainability, executive, crisis
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        /// <summary>
        /// Status: draft, review, approved, distributed, published, archived
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        [SugarColumn(IsNullable = true)]
        public DateTime? PublishDate { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? DistributedAt { get; set; }

        /// <summary>
        /// JSON: distribution channels (wire services, direct, social)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DistributionChannelsJson { get; set; }

        /// <summary>
        /// JSON: target media outlets
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TargetMediaJson { get; set; }

        /// <summary>
        /// JSON: attached images/documents
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AttachmentsJson { get; set; }

        /// <summary>
        /// JSON: quotes included in release
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string QuotesJson { get; set; }

        /// <summary>
        /// JSON: boilerplate/about section
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BoilerplateJson { get; set; }

        /// <summary>
        /// Contact person name
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContactPerson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string ContactEmail { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string ContactPhone { get; set; }

        // Performance
        public long MediaReach { get; set; } = 0;
        public int MediaMentions { get; set; } = 0;
        public int MediaPickups { get; set; } = 0;

        /// <summary>
        /// Sentiment: positive, neutral, negative, mixed
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string OverallSentiment { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EstimatedPrValue { get; set; }

        public long SocialShares { get; set; } = 0;
        public long WebsiteTraffic { get; set; } = 0;

        /// <summary>
        /// AI-generated
        /// </summary>
        public bool IsAiGenerated { get; set; } = false;

        /// <summary>
        /// Related event
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ApprovedBy { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? ApprovedAt { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Media Contact - جهات الاتصال الإعلامية
    /// </summary>
    [SugarTable("mkt_media_contact")]
    public class MktMediaContact : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContactId { get; set; }

        [SugarColumn(Length = 200)]
        public string FullName { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string FullNameAr { get; set; }

        [SugarColumn(Length = 200)]
        public string MediaOutlet { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string MediaOutletAr { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string JobTitle { get; set; }

        [SugarColumn(Length = 200)]
        public string Email { get; set; }

        [SugarColumn(Length = 50, IsNullable = true)]
        public string Phone { get; set; }

        [SugarColumn(Length = 500, IsNullable = true)]
        public string SocialMediaHandles { get; set; }

        /// <summary>
        /// Type: journalist, editor, blogger, influencer, analyst, producer
        /// </summary>
        [SugarColumn(Length = 30)]
        public string ContactType { get; set; }

        /// <summary>
        /// Beat/specialty: technology, business, entertainment, events, lifestyle, finance
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Specialty { get; set; }

        /// <summary>
        /// Reach: local, national, international
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Reach { get; set; } = "local";

        /// <summary>
        /// Country focus
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Country { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string City { get; set; }

        /// <summary>
        /// Preferred language: ar, en, both
        /// </summary>
        [SugarColumn(Length = 10)]
        public string PreferredLanguage { get; set; } = "ar";

        /// <summary>
        /// Relationship strength: cold, warm, strong, advocate
        /// </summary>
        [SugarColumn(Length = 20)]
        public string RelationshipLevel { get; set; } = "cold";

        [SugarColumn(IsNullable = true)]
        public DateTime? LastContactedAt { get; set; }

        public int TotalInteractions { get; set; } = 0;
        public int TotalCoverages { get; set; } = 0;

        /// <summary>
        /// Estimated audience reach
        /// </summary>
        public long EstimatedReach { get; set; } = 0;

        /// <summary>
        /// JSON: interaction history notes
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string InteractionHistoryJson { get; set; }

        /// <summary>
        /// JSON: preferred topics/interests
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string InterestsJson { get; set; }

        /// <summary>
        /// Influence score (0-100) calculated from reach and engagement
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? InfluenceScore { get; set; }

        public bool IsActive { get; set; } = true;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string PhotoUrl { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// Media Mention - التغطيات الإعلامية
    /// Tracks media coverage and mentions
    /// </summary>
    [SugarTable("mkt_media_mention")]
    public class MktMediaMention : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MentionId { get; set; }

        [SugarColumn(Length = 300)]
        public string Title { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string TitleAr { get; set; }

        [SugarColumn(Length = 500)]
        public string SourceUrl { get; set; }

        [SugarColumn(Length = 200)]
        public string MediaOutlet { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AuthorName { get; set; }

        /// <summary>
        /// Type: article, tv_segment, radio_mention, social_post, blog_post, podcast, video
        /// </summary>
        [SugarColumn(Length = 30)]
        public string MentionType { get; set; }

        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Sentiment: positive, neutral, negative
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Sentiment { get; set; } = "neutral";

        /// <summary>
        /// AI sentiment confidence (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? SentimentConfidence { get; set; }

        public long EstimatedReach { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? EstimatedPrValue { get; set; }

        /// <summary>
        /// JSON: key messages mentioned
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string KeyMessagesJson { get; set; }

        /// <summary>
        /// JSON: AI-generated summary
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiSummaryJson { get; set; }

        /// <summary>
        /// Source: manual, media_monitoring, ai_discovery, api_webhook
        /// </summary>
        [SugarColumn(Length = 30)]
        public string DiscoverySource { get; set; } = "manual";

        /// <summary>
        /// Related press release ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? PressReleaseId { get; set; }

        /// <summary>
        /// Related media contact ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? MediaContactId { get; set; }

        /// <summary>
        /// Related event ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Is a featured/prominent mention
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Clipping image URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string ClippingImageUrl { get; set; }

        /// <summary>
        /// Full article text (if available)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string FullText { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// PR Sentiment Analysis - تحليل المشاعر العامة
    /// Aggregated sentiment tracking over time
    /// </summary>
    [SugarTable("mkt_pr_sentiment")]
    public class MktPrSentiment
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SentimentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        /// <summary>
        /// Period: daily, weekly, monthly
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Period { get; set; }

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalMentions { get; set; } = 0;
        public int PositiveMentions { get; set; } = 0;
        public int NeutralMentions { get; set; } = 0;
        public int NegativeMentions { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? PositivePercentage { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? NeutralPercentage { get; set; }

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? NegativePercentage { get; set; }

        /// <summary>
        /// Net sentiment score (-100 to +100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? NetSentimentScore { get; set; }

        public long TotalReach { get; set; } = 0;

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? TotalPrValue { get; set; }

        /// <summary>
        /// JSON: top topics/keywords
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TopTopicsJson { get; set; }

        /// <summary>
        /// JSON: sentiment trend data points
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TrendDataJson { get; set; }

        /// <summary>
        /// JSON: AI-generated insights
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string AiInsightsJson { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }
}
