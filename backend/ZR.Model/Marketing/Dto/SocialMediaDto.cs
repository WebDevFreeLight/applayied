using System;
using System.Collections.Generic;

namespace ZR.Model.Marketing.Dto
{
    #region Social Account DTOs

    public class SocialAccountQueryDto : PagerInfo
    {
        public string Platform { get; set; }
        public string ConnectionStatus { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SocialAccountDto
    {
        public long AccountId { get; set; }
        public string Platform { get; set; }
        public string AccountName { get; set; }
        public string AccountHandle { get; set; }
        public string ProfileUrl { get; set; }
        public string AvatarUrl { get; set; }
        public long FollowersCount { get; set; }
        public long FollowingCount { get; set; }
        public long PostsCount { get; set; }
        public decimal? EngagementRate { get; set; }
        public decimal? GrowthRate { get; set; }
        public string ConnectionStatus { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public string PlatformConfigJson { get; set; }
        public bool IsActive { get; set; }
    }

    public class ConnectAccountDto
    {
        public string Platform { get; set; }
        public string AccountName { get; set; }
        public string AccountHandle { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
    }

    #endregion

    #region Social Post DTOs

    public class SocialPostQueryDto : PagerInfo
    {
        public long? AccountId { get; set; }
        public long? CampaignId { get; set; }
        public string PostType { get; set; }
        public string Status { get; set; }
        public bool? IsAiGenerated { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SocialPostDto
    {
        public long PostId { get; set; }
        public long AccountId { get; set; }
        public long? CampaignId { get; set; }
        public string Content { get; set; }
        public string ContentAr { get; set; }
        public string PostType { get; set; }
        public string MediaJson { get; set; }
        public string HashtagsJson { get; set; }
        public string MentionsJson { get; set; }
        public string LinkUrl { get; set; }
        public string Status { get; set; }
        public DateTime? ScheduledAt { get; set; }
    }

    public class SocialPostOutputDto : SocialPostDto
    {
        public string AccountName { get; set; }
        public string Platform { get; set; }
        public string CampaignName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string ExternalPostId { get; set; }
        public string ExternalPostUrl { get; set; }
        public long Likes { get; set; }
        public long Comments { get; set; }
        public long Shares { get; set; }
        public long Views { get; set; }
        public long Reach { get; set; }
        public long Impressions { get; set; }
        public long Saves { get; set; }
        public long Clicks { get; set; }
        public decimal? EngagementRate { get; set; }
        public bool IsAiGenerated { get; set; }
        public decimal? AiQualityScore { get; set; }
        public string CommentSentiment { get; set; }
        public bool CulturalComplianceChecked { get; set; }
    }

    public class GenerateAiPostRequest
    {
        public long AccountId { get; set; }
        public string Topic { get; set; }
        public string Tone { get; set; }
        public string Language { get; set; } = "bilingual";
        public string PostType { get; set; } = "text";
        public long? CampaignId { get; set; }
        public bool AutoSchedule { get; set; } = false;
    }

    #endregion

    #region Social Analytics DTOs

    public class SocialAnalyticsQueryDto
    {
        public long? AccountId { get; set; }
        public string Period { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SocialAnalyticsOutputDto
    {
        public long AnalyticsId { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string Platform { get; set; }
        public string Period { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public long FollowersStart { get; set; }
        public long FollowersEnd { get; set; }
        public long FollowersGained { get; set; }
        public long FollowersLost { get; set; }
        public long NetFollowerGrowth { get; set; }
        public int PostsPublished { get; set; }
        public long TotalImpressions { get; set; }
        public long TotalReach { get; set; }
        public long TotalEngagements { get; set; }
        public long TotalClicks { get; set; }
        public long TotalViews { get; set; }
        public decimal? AvgEngagementRate { get; set; }
        public decimal? AvgReachRate { get; set; }
        public decimal? GrowthRate { get; set; }
        public string TopPostsJson { get; set; }
        public string AudienceDemographicsJson { get; set; }
        public string BestTimesJson { get; set; }
        public string ContentTypePerformanceJson { get; set; }
        public string SentimentJson { get; set; }
    }

    public class SocialDashboardDto
    {
        public long TotalFollowers { get; set; }
        public decimal OverallEngagementRate { get; set; }
        public long TotalReach { get; set; }
        public decimal OverallGrowthRate { get; set; }
        public int TotalAccountsConnected { get; set; }
        public int TotalPostsThisMonth { get; set; }
        public List<PlatformSummaryDto> PlatformSummaries { get; set; }
        public List<SocialTrendPointDto> WeeklyTrend { get; set; }
        public List<TopPostDto> TopPosts { get; set; }
        public List<ScheduledPostDto> UpcomingPosts { get; set; }
    }

    public class PlatformSummaryDto
    {
        public string Platform { get; set; }
        public string AccountName { get; set; }
        public long Followers { get; set; }
        public decimal EngagementRate { get; set; }
        public int PostCount { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class SocialTrendPointDto
    {
        public DateTime Date { get; set; }
        public long Followers { get; set; }
        public long Engagements { get; set; }
        public long Reach { get; set; }
    }

    public class TopPostDto
    {
        public long PostId { get; set; }
        public string Platform { get; set; }
        public string Content { get; set; }
        public long TotalEngagements { get; set; }
        public long Reach { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    public class ScheduledPostDto
    {
        public long PostId { get; set; }
        public string Platform { get; set; }
        public string Content { get; set; }
        public DateTime ScheduledAt { get; set; }
    }

    #endregion
}
