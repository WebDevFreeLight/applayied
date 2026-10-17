using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Content DTOs

    public class ContentQueryDto : PagerInfo
    {
        public string Title { get; set; }
        public string ContentType { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public long? CampaignId { get; set; }
        public long? EventId { get; set; }
        public long? AuthorId { get; set; }
        public bool? IsAiGenerated { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class ContentDto
    {
        public long ContentId { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string ContentType { get; set; }
        public string Body { get; set; }
        public string BodyAr { get; set; }
        public string FileUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? FileSize { get; set; }
        public string MimeType { get; set; }
        public int? DurationSeconds { get; set; }
        public string SeoMetadataJson { get; set; }
        public string TagsJson { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public long? AuthorId { get; set; }
        public string AuthorName { get; set; }
        public long? CampaignId { get; set; }
        public long? EventId { get; set; }
    }

    public class ContentOutputDto : ContentDto
    {
        public DateTime? PublishedAt { get; set; }
        public long Views { get; set; }
        public long UniqueViews { get; set; }
        public long Downloads { get; set; }
        public long Shares { get; set; }
        public long Likes { get; set; }
        public long Comments { get; set; }
        public decimal? EngagementRate { get; set; }
        public decimal? AvgTimeOnPageSeconds { get; set; }
        public bool IsAiGenerated { get; set; }
        public decimal? AiQualityScore { get; set; }
        public bool CulturalComplianceVerified { get; set; }
        public int VersionNumber { get; set; }
        public string CampaignName { get; set; }
        public string EventName { get; set; }
    }

    public class ContentAnalyticsDto
    {
        public int TotalContent { get; set; }
        public int PublishedContent { get; set; }
        public long TotalViews { get; set; }
        public long TotalShares { get; set; }
        public decimal AvgEngagementRate { get; set; }
        public List<ContentTypeBreakdownDto> TypeBreakdown { get; set; }
        public List<ContentPerformanceDto> TopContent { get; set; }
        public List<ContentTrendDto> MonthlyTrend { get; set; }
    }

    public class ContentTypeBreakdownDto
    {
        public string ContentType { get; set; }
        public int Count { get; set; }
        public long TotalViews { get; set; }
        public decimal AvgEngagement { get; set; }
    }

    public class ContentPerformanceDto
    {
        public long ContentId { get; set; }
        public string Title { get; set; }
        public string ContentType { get; set; }
        public long Views { get; set; }
        public long Shares { get; set; }
        public decimal? EngagementRate { get; set; }
    }

    public class ContentTrendDto
    {
        public DateTime Date { get; set; }
        public int Published { get; set; }
        public long Views { get; set; }
        public long Engagements { get; set; }
    }

    #endregion

    #region AI Content Generation DTOs

    public class AiContentGenerationQueryDto : PagerInfo
    {
        public string GenerationType { get; set; }
        public string Status { get; set; }
        public string AiProvider { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class AiContentGenerationDto
    {
        public long GenerationId { get; set; }
        public string GenerationType { get; set; }
        public string Prompt { get; set; }
        public string ContextJson { get; set; }
        public string Output { get; set; }
        public string OutputAr { get; set; }
        public string AiModel { get; set; }
        public string AiProvider { get; set; }
        public decimal? QualityScore { get; set; }
        public decimal? CulturalScore { get; set; }
        public string Status { get; set; }
        public bool IsApproved { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public long? LinkedContentId { get; set; }
        public int TokensUsed { get; set; }
        public decimal? ProcessingCost { get; set; }
        public long ProcessingTimeMs { get; set; }
        public long? RequestedBy { get; set; }
        public string RequestedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GenerateContentRequest
    {
        public string GenerationType { get; set; }
        public string Prompt { get; set; }
        public string Tone { get; set; }
        public string Language { get; set; } = "bilingual";
        public string TargetAudience { get; set; }
        public string ContextJson { get; set; }
        public bool AutoSaveToLibrary { get; set; } = false;
    }

    public class ApproveContentRequest
    {
        public long GenerationId { get; set; }
        public bool Approve { get; set; }
        public bool SaveToLibrary { get; set; } = true;
    }

    #endregion
}
