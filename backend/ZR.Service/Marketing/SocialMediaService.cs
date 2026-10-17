using Infrastructure.Attribute;
using Infrastructure.Extensions;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Model;
using ZR.Model.Marketing.SocialMedia;
using ZR.Model.Marketing.Dto;
using ZR.Repository;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    /// <summary>
    /// Social Media Service Implementation - خدمة وسائل التواصل الاجتماعي
    /// Account management, post scheduling, AI content generation, analytics aggregation
    /// </summary>
    [AppService(ServiceType = typeof(ISocialMediaService), ServiceLifetime = LifeTime.Transient)]
    public class SocialMediaService : BaseService<MktSocialAccount>, ISocialMediaService
    {
        #region Accounts

        /// <summary>
        /// Get social media accounts with filtering
        /// </summary>
        public PagedInfo<SocialAccountDto> GetAccounts(SocialAccountQueryDto query)
        {
            var predicate = Expressionable.Create<MktSocialAccount>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Platform), it => it.Platform == query.Platform);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.ConnectionStatus), it => it.ConnectionStatus == query.ConnectionStatus);
            predicate = predicate.AndIF(query.IsActive != null, it => it.IsActive == query.IsActive);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(a => a.Create_time)
                .Select(a => new SocialAccountDto
                {
                    AccountId = a.AccountId,
                    Platform = a.Platform,
                    AccountName = a.AccountName,
                    AccountHandle = a.AccountHandle,
                    ProfileUrl = a.ProfileUrl,
                    AvatarUrl = a.AvatarUrl,
                    FollowersCount = a.FollowersCount,
                    FollowingCount = a.FollowingCount,
                    PostsCount = a.PostsCount,
                    EngagementRate = a.EngagementRate,
                    GrowthRate = a.GrowthRate,
                    ConnectionStatus = a.ConnectionStatus,
                    LastSyncAt = a.LastSyncAt,
                    PlatformConfigJson = a.PlatformConfigJson,
                    IsActive = a.IsActive
                })
                .ToPage(query);

            return response;
        }

        /// <summary>
        /// Connect a new social media account
        /// </summary>
        public MktSocialAccount ConnectAccount(ConnectAccountDto dto)
        {
            var account = new MktSocialAccount
            {
                Platform = dto.Platform,
                AccountName = dto.AccountName,
                AccountHandle = dto.AccountHandle,
                AccessTokenEncrypted = dto.AccessToken, // In production, encrypt before storing
                RefreshTokenEncrypted = dto.RefreshToken,
                TokenExpiresAt = dto.TokenExpiresAt,
                ConnectionStatus = "connected",
                IsActive = true,
                LastSyncAt = DateTime.Now
            };

            Insertable(account).ExecuteReturnSnowflakeId();
            return account;
        }

        /// <summary>
        /// Get account detail by ID
        /// </summary>
        public SocialAccountDto GetAccountDetail(long accountId)
        {
            var account = Queryable().Where(a => a.AccountId == accountId).First();
            if (account == null) return null;

            return new SocialAccountDto
            {
                AccountId = account.AccountId,
                Platform = account.Platform,
                AccountName = account.AccountName,
                AccountHandle = account.AccountHandle,
                ProfileUrl = account.ProfileUrl,
                AvatarUrl = account.AvatarUrl,
                FollowersCount = account.FollowersCount,
                FollowingCount = account.FollowingCount,
                PostsCount = account.PostsCount,
                EngagementRate = account.EngagementRate,
                GrowthRate = account.GrowthRate,
                ConnectionStatus = account.ConnectionStatus,
                LastSyncAt = account.LastSyncAt,
                PlatformConfigJson = account.PlatformConfigJson,
                IsActive = account.IsActive
            };
        }

        /// <summary>
        /// Disconnect a social media account
        /// </summary>
        public int DisconnectAccount(long accountId)
        {
            return Context.Updateable<MktSocialAccount>()
                .SetColumns(a => a.ConnectionStatus == "disconnected")
                .SetColumns(a => a.IsActive == false)
                .Where(a => a.AccountId == accountId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Sync a social media account (refresh data from platform)
        /// </summary>
        public int SyncAccount(long accountId)
        {
            return Context.Updateable<MktSocialAccount>()
                .SetColumns(a => a.LastSyncAt == DateTime.Now)
                .Where(a => a.AccountId == accountId)
                .ExecuteCommand();
        }

        #endregion

        #region Posts

        /// <summary>
        /// Get social media posts with filtering
        /// </summary>
        public PagedInfo<SocialPostOutputDto> GetPosts(SocialPostQueryDto query)
        {
            var predicate = Expressionable.Create<MktSocialPost>();

            predicate = predicate.AndIF(query.AccountId != null, it => it.AccountId == query.AccountId);
            predicate = predicate.AndIF(query.CampaignId != null, it => it.CampaignId == query.CampaignId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.PostType), it => it.PostType == query.PostType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.IsAiGenerated != null, it => it.IsAiGenerated == query.IsAiGenerated);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.Create_time >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.Create_time <= query.DateTo);

            var response = Context.Queryable<MktSocialPost>()
                .Where(predicate.ToExpression())
                .OrderByDescending(p => p.Create_time)
                .Select(p => new SocialPostOutputDto
                {
                    PostId = p.PostId,
                    AccountId = p.AccountId,
                    CampaignId = p.CampaignId,
                    Content = p.Content,
                    ContentAr = p.ContentAr,
                    PostType = p.PostType,
                    MediaJson = p.MediaJson,
                    HashtagsJson = p.HashtagsJson,
                    MentionsJson = p.MentionsJson,
                    LinkUrl = p.LinkUrl,
                    Status = p.Status,
                    ScheduledAt = p.ScheduledAt,
                    PublishedAt = p.PublishedAt,
                    ExternalPostId = p.ExternalPostId,
                    ExternalPostUrl = p.ExternalPostUrl,
                    Likes = p.Likes,
                    Comments = p.Comments,
                    Shares = p.Shares,
                    Views = p.Views,
                    Reach = p.Reach,
                    Impressions = p.Impressions,
                    Saves = p.Saves,
                    Clicks = p.Clicks,
                    EngagementRate = p.EngagementRate,
                    IsAiGenerated = p.IsAiGenerated,
                    AiQualityScore = p.AiQualityScore,
                    CommentSentiment = p.CommentSentiment,
                    CulturalComplianceChecked = p.CulturalComplianceChecked
                })
                .ToPage(query);

            // Load account info for each post
            foreach (var item in response.Result)
            {
                var account = Queryable().Where(a => a.AccountId == item.AccountId).First();
                if (account != null)
                {
                    item.AccountName = account.AccountName;
                    item.Platform = account.Platform;
                }
            }

            return response;
        }

        /// <summary>
        /// Create a new social media post
        /// </summary>
        public MktSocialPost CreatePost(SocialPostDto dto)
        {
            var post = new MktSocialPost
            {
                AccountId = dto.AccountId,
                CampaignId = dto.CampaignId,
                Content = dto.Content,
                ContentAr = dto.ContentAr,
                PostType = dto.PostType ?? "text",
                MediaJson = dto.MediaJson,
                HashtagsJson = dto.HashtagsJson,
                MentionsJson = dto.MentionsJson,
                LinkUrl = dto.LinkUrl,
                Status = dto.Status ?? "draft",
                ScheduledAt = dto.ScheduledAt
            };

            Context.Insertable(post).ExecuteReturnSnowflakeId();
            return post;
        }

        /// <summary>
        /// Schedule a post for future publishing
        /// </summary>
        public int SchedulePost(long postId, DateTime scheduledAt)
        {
            return Context.Updateable<MktSocialPost>()
                .SetColumns(p => p.ScheduledAt == scheduledAt)
                .SetColumns(p => p.Status == "scheduled")
                .Where(p => p.PostId == postId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get post detail by ID
        /// </summary>
        public SocialPostOutputDto GetPostDetail(long postId)
        {
            var post = Context.Queryable<MktSocialPost>().Where(p => p.PostId == postId).First();
            if (post == null) return null;

            var dto = new SocialPostOutputDto
            {
                PostId = post.PostId,
                AccountId = post.AccountId,
                CampaignId = post.CampaignId,
                Content = post.Content,
                ContentAr = post.ContentAr,
                PostType = post.PostType,
                MediaJson = post.MediaJson,
                HashtagsJson = post.HashtagsJson,
                MentionsJson = post.MentionsJson,
                LinkUrl = post.LinkUrl,
                Status = post.Status,
                ScheduledAt = post.ScheduledAt,
                PublishedAt = post.PublishedAt,
                ExternalPostId = post.ExternalPostId,
                ExternalPostUrl = post.ExternalPostUrl,
                Likes = post.Likes,
                Comments = post.Comments,
                Shares = post.Shares,
                Views = post.Views,
                Reach = post.Reach,
                Impressions = post.Impressions,
                Saves = post.Saves,
                Clicks = post.Clicks,
                EngagementRate = post.EngagementRate,
                IsAiGenerated = post.IsAiGenerated,
                AiQualityScore = post.AiQualityScore,
                CommentSentiment = post.CommentSentiment,
                CulturalComplianceChecked = post.CulturalComplianceChecked
            };

            var account = Queryable().Where(a => a.AccountId == post.AccountId).First();
            if (account != null)
            {
                dto.AccountName = account.AccountName;
                dto.Platform = account.Platform;
            }

            return dto;
        }

        /// <summary>
        /// Update an existing social media post
        /// </summary>
        public int UpdatePost(SocialPostDto dto)
        {
            return Context.Updateable<MktSocialPost>()
                .SetColumns(p => p.Content == dto.Content)
                .SetColumns(p => p.ContentAr == dto.ContentAr)
                .SetColumns(p => p.PostType == dto.PostType)
                .SetColumns(p => p.MediaJson == dto.MediaJson)
                .SetColumns(p => p.HashtagsJson == dto.HashtagsJson)
                .SetColumns(p => p.MentionsJson == dto.MentionsJson)
                .SetColumns(p => p.LinkUrl == dto.LinkUrl)
                .SetColumns(p => p.Status == dto.Status)
                .SetColumns(p => p.ScheduledAt == dto.ScheduledAt)
                .Where(p => p.PostId == dto.PostId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete social media posts by IDs
        /// </summary>
        public int DeletePosts(long[] postIds)
        {
            return Context.Deleteable<MktSocialPost>()
                .Where(p => postIds.Contains(p.PostId))
                .ExecuteCommand();
        }

        /// <summary>
        /// Publish a social media post
        /// </summary>
        public int PublishPost(long postId)
        {
            return Context.Updateable<MktSocialPost>()
                .SetColumns(p => p.Status == "published")
                .SetColumns(p => p.PublishedAt == DateTime.Now)
                .Where(p => p.PostId == postId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Generate AI-powered social media post content
        /// </summary>
        public MktSocialPost GenerateAiPost(GenerateAiPostRequest request)
        {
            var post = new MktSocialPost
            {
                AccountId = request.AccountId,
                CampaignId = request.CampaignId,
                Content = $"[AI Generated] Topic: {request.Topic}, Tone: {request.Tone}",
                PostType = request.PostType ?? "text",
                Status = request.AutoSchedule ? "scheduled" : "draft",
                IsAiGenerated = true,
                AiSuggestionsJson = $"{{\"topic\":\"{request.Topic}\",\"tone\":\"{request.Tone}\",\"language\":\"{request.Language}\"}}"
            };

            Context.Insertable(post).ExecuteReturnSnowflakeId();
            return post;
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get social media analytics
        /// </summary>
        public List<SocialAnalyticsOutputDto> GetAnalytics(SocialAnalyticsQueryDto query)
        {
            var predicate = Expressionable.Create<MktSocialAnalytics>();

            predicate = predicate.AndIF(query.AccountId != null, it => it.AccountId == query.AccountId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Period), it => it.Period == query.Period);
            predicate = predicate.AndIF(query.DateFrom != null, it => it.PeriodStart >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo != null, it => it.PeriodEnd <= query.DateTo);

            var result = Context.Queryable<MktSocialAnalytics>()
                .Where(predicate.ToExpression())
                .OrderByDescending(a => a.PeriodStart)
                .Select(a => new SocialAnalyticsOutputDto
                {
                    AnalyticsId = a.AnalyticsId,
                    AccountId = a.AccountId,
                    Period = a.Period,
                    PeriodStart = a.PeriodStart,
                    PeriodEnd = a.PeriodEnd,
                    FollowersStart = a.FollowersStart,
                    FollowersEnd = a.FollowersEnd,
                    FollowersGained = a.FollowersGained,
                    FollowersLost = a.FollowersLost,
                    NetFollowerGrowth = a.NetFollowerGrowth,
                    PostsPublished = a.PostsPublished,
                    TotalImpressions = a.TotalImpressions,
                    TotalReach = a.TotalReach,
                    TotalEngagements = a.TotalEngagements,
                    TotalClicks = a.TotalClicks,
                    TotalViews = a.TotalViews,
                    AvgEngagementRate = a.AvgEngagementRate,
                    AvgReachRate = a.AvgReachRate,
                    GrowthRate = a.GrowthRate,
                    TopPostsJson = a.TopPostsJson,
                    AudienceDemographicsJson = a.AudienceDemographicsJson,
                    BestTimesJson = a.BestTimesJson,
                    ContentTypePerformanceJson = a.ContentTypePerformanceJson,
                    SentimentJson = a.SentimentJson
                })
                .ToList();

            // Enrich with account info
            foreach (var item in result)
            {
                var account = Queryable().Where(a => a.AccountId == item.AccountId).First();
                if (account != null)
                {
                    item.AccountName = account.AccountName;
                    item.Platform = account.Platform;
                }
            }

            return result;
        }

        /// <summary>
        /// Get social media dashboard summary
        /// </summary>
        public SocialDashboardDto GetDashboard()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && a.ConnectionStatus == "connected")
                .ToList();

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var postsThisMonth = Context.Queryable<MktSocialPost>()
                .Where(p => p.PublishedAt != null && p.PublishedAt >= monthStart)
                .Count();

            var dashboard = new SocialDashboardDto
            {
                TotalFollowers = accounts.Sum(a => a.FollowersCount),
                OverallEngagementRate = accounts.Count > 0
                    ? accounts.Where(a => a.EngagementRate.HasValue).Select(a => a.EngagementRate.Value).DefaultIfEmpty(0).Average()
                    : 0,
                TotalReach = 0,
                OverallGrowthRate = accounts.Count > 0
                    ? accounts.Where(a => a.GrowthRate.HasValue).Select(a => a.GrowthRate.Value).DefaultIfEmpty(0).Average()
                    : 0,
                TotalAccountsConnected = accounts.Count,
                TotalPostsThisMonth = postsThisMonth,

                PlatformSummaries = accounts.Select(a => new PlatformSummaryDto
                {
                    Platform = a.Platform,
                    AccountName = a.AccountName,
                    Followers = a.FollowersCount,
                    EngagementRate = a.EngagementRate ?? 0,
                    PostCount = (int)a.PostsCount,
                    GrowthRate = a.GrowthRate ?? 0
                }).ToList(),

                WeeklyTrend = new List<SocialTrendPointDto>(),

                TopPosts = Context.Queryable<MktSocialPost>()
                    .Where(p => p.Status == "published")
                    .OrderByDescending(p => p.Likes + p.Comments + p.Shares)
                    .Take(5)
                    .Select(p => new TopPostDto
                    {
                        PostId = p.PostId,
                        Content = p.Content,
                        TotalEngagements = p.Likes + p.Comments + p.Shares,
                        Reach = p.Reach,
                        PublishedAt = p.PublishedAt ?? DateTime.MinValue
                    })
                    .ToList(),

                UpcomingPosts = Context.Queryable<MktSocialPost>()
                    .Where(p => p.Status == "scheduled" && p.ScheduledAt != null && p.ScheduledAt > now)
                    .OrderBy(p => p.ScheduledAt)
                    .Take(5)
                    .Select(p => new ScheduledPostDto
                    {
                        PostId = p.PostId,
                        Content = p.Content,
                        ScheduledAt = p.ScheduledAt ?? DateTime.MinValue
                    })
                    .ToList()
            };

            return dashboard;
        }

        #endregion
    }
}
