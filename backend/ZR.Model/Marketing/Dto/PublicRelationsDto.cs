using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Press Release DTOs

    public class PressReleaseQueryDto : PagerInfo
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public long? EventId { get; set; }
        public bool? IsAiGenerated { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PressReleaseDto
    {
        public long ReleaseId { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Subtitle { get; set; }
        public string SubtitleAr { get; set; }
        public string Body { get; set; }
        public string BodyAr { get; set; }
        public string Summary { get; set; }
        public string SummaryAr { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime? PublishDate { get; set; }
        public string DistributionChannelsJson { get; set; }
        public string TargetMediaJson { get; set; }
        public string AttachmentsJson { get; set; }
        public string QuotesJson { get; set; }
        public string BoilerplateJson { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public long? EventId { get; set; }
    }

    public class PressReleaseOutputDto : PressReleaseDto
    {
        public DateTime? DistributedAt { get; set; }
        public long MediaReach { get; set; }
        public int MediaMentions { get; set; }
        public int MediaPickups { get; set; }
        public string OverallSentiment { get; set; }
        public decimal? EstimatedPrValue { get; set; }
        public long SocialShares { get; set; }
        public long WebsiteTraffic { get; set; }
        public bool IsAiGenerated { get; set; }
        public long? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string EventName { get; set; }
    }

    #endregion

    #region Media Contact DTOs

    public class MediaContactQueryDto : PagerInfo
    {
        public string FullName { get; set; }
        public string MediaOutlet { get; set; }
        public string ContactType { get; set; }
        public string Specialty { get; set; }
        public string Reach { get; set; }
        public string RelationshipLevel { get; set; }
        public bool? IsActive { get; set; }
    }

    public class MediaContactDto
    {
        public long ContactId { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string MediaOutlet { get; set; }
        public string MediaOutletAr { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SocialMediaHandles { get; set; }
        public string ContactType { get; set; }
        public string Specialty { get; set; }
        public string Reach { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string PreferredLanguage { get; set; }
        public string RelationshipLevel { get; set; }
        public DateTime? LastContactedAt { get; set; }
        public int TotalInteractions { get; set; }
        public int TotalCoverages { get; set; }
        public long EstimatedReach { get; set; }
        public string InteractionHistoryJson { get; set; }
        public string InterestsJson { get; set; }
        public decimal? InfluenceScore { get; set; }
        public bool IsActive { get; set; }
        public string PhotoUrl { get; set; }
        public string Notes { get; set; }
    }

    #endregion

    #region Media Mention DTOs

    public class MediaMentionQueryDto : PagerInfo
    {
        public string Title { get; set; }
        public string MediaOutlet { get; set; }
        public string MentionType { get; set; }
        public string Sentiment { get; set; }
        public long? EventId { get; set; }
        public long? PressReleaseId { get; set; }
        public bool? IsFeatured { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class MediaMentionDto
    {
        public long MentionId { get; set; }
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string SourceUrl { get; set; }
        public string MediaOutlet { get; set; }
        public string AuthorName { get; set; }
        public string MentionType { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Sentiment { get; set; }
        public decimal? SentimentConfidence { get; set; }
        public long EstimatedReach { get; set; }
        public decimal? EstimatedPrValue { get; set; }
        public string KeyMessagesJson { get; set; }
        public string AiSummaryJson { get; set; }
        public string DiscoverySource { get; set; }
        public long? PressReleaseId { get; set; }
        public string PressReleaseTitle { get; set; }
        public long? MediaContactId { get; set; }
        public string MediaContactName { get; set; }
        public long? EventId { get; set; }
        public bool IsFeatured { get; set; }
        public string ClippingImageUrl { get; set; }
    }

    #endregion

    #region PR Analytics DTOs

    public class PrAnalyticsDto
    {
        public int TotalPressReleases { get; set; }
        public int TotalMediaMentions { get; set; }
        public long TotalMediaReach { get; set; }
        public decimal TotalPrValue { get; set; }
        public int TotalMediaContacts { get; set; }
        public decimal PositiveSentimentRate { get; set; }
        public decimal NeutralSentimentRate { get; set; }
        public decimal NegativeSentimentRate { get; set; }
        public decimal NetSentimentScore { get; set; }
        public List<PrTrendDto> MonthlyTrend { get; set; }
        public List<TopMediaOutletDto> TopMediaOutlets { get; set; }
        public List<SentimentDistributionDto> SentimentTrend { get; set; }
    }

    public class PrTrendDto
    {
        public DateTime Date { get; set; }
        public int Mentions { get; set; }
        public long Reach { get; set; }
        public decimal PrValue { get; set; }
    }

    public class TopMediaOutletDto
    {
        public string MediaOutlet { get; set; }
        public int Mentions { get; set; }
        public long TotalReach { get; set; }
        public string DominantSentiment { get; set; }
    }

    public class SentimentDistributionDto
    {
        public DateTime Date { get; set; }
        public int Positive { get; set; }
        public int Neutral { get; set; }
        public int Negative { get; set; }
        public decimal NetScore { get; set; }
    }

    #endregion

    #region PR Sentiment DTOs

    public class PrSentimentQueryDto
    {
        public long? EventId { get; set; }
        public string Period { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class PrSentimentDto
    {
        public long SentimentId { get; set; }
        public long? EventId { get; set; }
        public string Period { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalMentions { get; set; }
        public int PositiveMentions { get; set; }
        public int NeutralMentions { get; set; }
        public int NegativeMentions { get; set; }
        public decimal? PositivePercentage { get; set; }
        public decimal? NeutralPercentage { get; set; }
        public decimal? NegativePercentage { get; set; }
        public decimal? NetSentimentScore { get; set; }
        public long TotalReach { get; set; }
        public decimal? TotalPrValue { get; set; }
        public string TopTopicsJson { get; set; }
        public string TrendDataJson { get; set; }
        public string AiInsightsJson { get; set; }
    }

    #endregion
}
