using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.ContentMgmt
{
    /// <summary>
    /// Marketing Content - المحتوى التسويقي
    /// Centralized content library for all marketing materials
    /// </summary>
    [SugarTable("mkt_content")]
    public class MktContent : SysBase
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContentId { get; set; }

        [SugarColumn(Length = 300)]
        public string Title { get; set; }

        [SugarColumn(Length = 300, IsNullable = true)]
        public string TitleAr { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Type: blog_post, article, video, image, infographic, document, presentation,
        ///       case_study, whitepaper, social_graphic, banner, flyer, brochure
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ContentType { get; set; }

        /// <summary>
        /// Full content body (for text-based content)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string Body { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string BodyAr { get; set; }

        /// <summary>
        /// File URL for media content
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string FileUrl { get; set; }

        /// <summary>
        /// Thumbnail URL
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long? FileSize { get; set; }

        /// <summary>
        /// MIME type
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string MimeType { get; set; }

        /// <summary>
        /// Duration in seconds (for video/audio)
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// JSON: SEO metadata (meta title, description, keywords)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string SeoMetadataJson { get; set; }

        /// <summary>
        /// JSON: tags for categorization
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string TagsJson { get; set; }

        /// <summary>
        /// Category: event, product, service, brand, educational, testimonial
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string Category { get; set; }

        /// <summary>
        /// Status: draft, review, approved, published, archived
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";

        [SugarColumn(IsNullable = true)]
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Author/Creator
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? AuthorId { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string AuthorName { get; set; }

        /// <summary>
        /// Related campaign
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CampaignId { get; set; }

        /// <summary>
        /// Related event
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? EventId { get; set; }

        // Performance metrics
        public long Views { get; set; } = 0;
        public long UniqueViews { get; set; } = 0;
        public long Downloads { get; set; } = 0;
        public long Shares { get; set; } = 0;
        public long Likes { get; set; } = 0;
        public long Comments { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? EngagementRate { get; set; }

        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AvgTimeOnPageSeconds { get; set; }

        /// <summary>
        /// AI-generated content
        /// </summary>
        public bool IsAiGenerated { get; set; } = false;

        /// <summary>
        /// AI quality score (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? AiQualityScore { get; set; }

        /// <summary>
        /// Saudi cultural compliance verified
        /// </summary>
        public bool CulturalComplianceVerified { get; set; } = false;

        /// <summary>
        /// JSON: version history
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string VersionHistoryJson { get; set; }

        public int VersionNumber { get; set; } = 1;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }

    /// <summary>
    /// AI Content Generation Record - سجل إنشاء المحتوى بالذكاء الاصطناعي
    /// Tracks all AI-generated content with prompts and outputs
    /// </summary>
    [SugarTable("mkt_ai_content_generation")]
    public class MktAiContentGeneration
    {
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GenerationId { get; set; }

        /// <summary>
        /// Type: text, image, video_script, social_post, email_copy, ad_copy,
        ///       press_release, event_description, seo_content, translation
        /// </summary>
        [SugarColumn(Length = 50)]
        public string GenerationType { get; set; }

        /// <summary>
        /// The prompt/instruction given to AI
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Prompt { get; set; }

        /// <summary>
        /// JSON: additional context provided (brand guidelines, tone, audience)
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string ContextJson { get; set; }

        /// <summary>
        /// Generated output
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT")]
        public string Output { get; set; }

        /// <summary>
        /// Arabic version of output
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string OutputAr { get; set; }

        /// <summary>
        /// AI model used
        /// </summary>
        [SugarColumn(Length = 100)]
        public string AiModel { get; set; }

        /// <summary>
        /// AI provider: openai, anthropic, google, stability
        /// </summary>
        [SugarColumn(Length = 30)]
        public string AiProvider { get; set; }

        /// <summary>
        /// Quality score (0-100) - AI self-assessment or human rating
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? QualityScore { get; set; }

        /// <summary>
        /// Cultural compliance score for Saudi context (0-100)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? CulturalScore { get; set; }

        /// <summary>
        /// Status: generating, completed, approved, rejected, published
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "generating";

        /// <summary>
        /// Was the output approved for use?
        /// </summary>
        public bool IsApproved { get; set; } = false;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? ApprovedBy { get; set; }

        /// <summary>
        /// Linked content item (if output was saved to content library)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? LinkedContentId { get; set; }

        public int TokensUsed { get; set; } = 0;

        [SugarColumn(DecimalDigits = 4, IsNullable = true)]
        public decimal? ProcessingCost { get; set; }

        public long ProcessingTimeMs { get; set; } = 0;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? RequestedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CompanyId { get; set; }
    }
}
