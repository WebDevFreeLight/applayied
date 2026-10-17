using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Marketing.SocialMedia;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Social Media Service Interface - خدمة وسائل التواصل الاجتماعي
    /// </summary>
    public interface ISocialMediaService : IBaseService<MktSocialAccount>
    {
        /// <summary>
        /// Get social media accounts with filtering
        /// </summary>
        PagedInfo<SocialAccountDto> GetAccounts(SocialAccountQueryDto query);

        /// <summary>
        /// Connect a new social media account
        /// </summary>
        MktSocialAccount ConnectAccount(ConnectAccountDto dto);

        /// <summary>
        /// Get account detail by ID
        /// </summary>
        SocialAccountDto GetAccountDetail(long accountId);

        /// <summary>
        /// Disconnect a social media account
        /// </summary>
        int DisconnectAccount(long accountId);

        /// <summary>
        /// Sync a social media account (refresh data from platform)
        /// </summary>
        int SyncAccount(long accountId);

        /// <summary>
        /// Get social media posts with filtering
        /// </summary>
        PagedInfo<SocialPostOutputDto> GetPosts(SocialPostQueryDto query);

        /// <summary>
        /// Create a new social media post
        /// </summary>
        MktSocialPost CreatePost(SocialPostDto dto);

        /// <summary>
        /// Get post detail by ID
        /// </summary>
        SocialPostOutputDto GetPostDetail(long postId);

        /// <summary>
        /// Update an existing social media post
        /// </summary>
        int UpdatePost(SocialPostDto dto);

        /// <summary>
        /// Delete social media posts by IDs
        /// </summary>
        int DeletePosts(long[] postIds);

        /// <summary>
        /// Publish a social media post
        /// </summary>
        int PublishPost(long postId);

        /// <summary>
        /// Schedule a post for future publishing
        /// </summary>
        int SchedulePost(long postId, DateTime scheduledAt);

        /// <summary>
        /// Generate AI-powered social media post content
        /// </summary>
        MktSocialPost GenerateAiPost(GenerateAiPostRequest request);

        /// <summary>
        /// Get social media analytics
        /// </summary>
        List<SocialAnalyticsOutputDto> GetAnalytics(SocialAnalyticsQueryDto query);

        /// <summary>
        /// Get social media dashboard summary
        /// </summary>
        SocialDashboardDto GetDashboard();
    }
}
