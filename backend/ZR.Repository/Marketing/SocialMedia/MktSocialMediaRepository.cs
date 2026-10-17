using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.SocialMedia;
using ZR.Repository;

namespace ZR.Repository.Marketing.SocialMedia
{
    /// <summary>
    /// Social Account Repository - مستودع حسابات التواصل الاجتماعي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSocialAccountRepository : BaseRepository<MktSocialAccount>
    {
        #region Account Queries

        /// <summary>
        /// Get connected accounts - الحصول على الحسابات المتصلة
        /// </summary>
        public List<MktSocialAccount> GetConnectedAccounts()
        {
            return Context.Queryable<MktSocialAccount>()
                .Where(a => a.ConnectionStatus == "connected")
                .Where(a => a.IsActive)
                .OrderBy(a => a.Platform)
                .ToList();
        }

        /// <summary>
        /// Get accounts by platform - الحصول على الحسابات حسب المنصة
        /// </summary>
        public List<MktSocialAccount> GetByPlatform(string platform)
        {
            return Context.Queryable<MktSocialAccount>()
                .Where(a => a.Platform == platform)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.FollowersCount)
                .ToList();
        }

        /// <summary>
        /// Get accounts with expired tokens - الحصول على الحسابات ذات الرموز المنتهية
        /// </summary>
        public List<MktSocialAccount> GetAccountsWithExpiredTokens()
        {
            return Context.Queryable<MktSocialAccount>()
                .Where(a => a.IsActive)
                .Where(a => a.TokenExpiresAt != null && a.TokenExpiresAt <= DateTime.Now)
                .ToList();
        }

        /// <summary>
        /// Get accounts needing sync - الحصول على الحسابات التي تحتاج مزامنة
        /// </summary>
        public List<MktSocialAccount> GetAccountsNeedingSync(int hoursThreshold = 6)
        {
            var threshold = DateTime.Now.AddHours(-hoursThreshold);
            return Context.Queryable<MktSocialAccount>()
                .Where(a => a.IsActive && a.ConnectionStatus == "connected")
                .Where(a => a.LastSyncAt == null || a.LastSyncAt < threshold)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Social Post Repository - مستودع المنشورات
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSocialPostRepository : BaseRepository<MktSocialPost>
    {
        /// <summary>
        /// Get scheduled posts - الحصول على المنشورات المجدولة
        /// </summary>
        public List<MktSocialPost> GetScheduledPosts()
        {
            return Context.Queryable<MktSocialPost>()
                .Where(p => p.Status == "scheduled")
                .Where(p => p.ScheduledAt != null)
                .OrderBy(p => p.ScheduledAt)
                .ToList();
        }

        /// <summary>
        /// Get posts by account - الحصول على منشورات الحساب
        /// </summary>
        public List<MktSocialPost> GetByAccountId(long accountId)
        {
            return Context.Queryable<MktSocialPost>()
                .Where(p => p.AccountId == accountId)
                .OrderByDescending(p => p.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get posts by campaign - الحصول على منشورات الحملة
        /// </summary>
        public List<MktSocialPost> GetByCampaignId(long campaignId)
        {
            return Context.Queryable<MktSocialPost>()
                .Where(p => p.CampaignId == campaignId)
                .OrderByDescending(p => p.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get posts due for publishing - الحصول على المنشورات المستحقة للنشر
        /// </summary>
        public List<MktSocialPost> GetPostsDueForPublishing()
        {
            return Context.Queryable<MktSocialPost>()
                .Where(p => p.Status == "scheduled")
                .Where(p => p.ScheduledAt != null && p.ScheduledAt <= DateTime.Now)
                .OrderBy(p => p.ScheduledAt)
                .ToList();
        }

        /// <summary>
        /// Get published posts with engagement data - الحصول على المنشورات المنشورة مع بيانات التفاعل
        /// </summary>
        public List<MktSocialPost> GetPublishedPostsWithEngagement(long accountId, DateTime? from, DateTime? to)
        {
            return Context.Queryable<MktSocialPost>()
                .Where(p => p.AccountId == accountId)
                .Where(p => p.Status == "published")
                .WhereIF(from.HasValue, p => p.PublishedAt >= from.Value)
                .WhereIF(to.HasValue, p => p.PublishedAt <= to.Value)
                .OrderByDescending(p => p.EngagementRate)
                .ToList();
        }
    }

    /// <summary>
    /// Social Analytics Repository - مستودع تحليلات التواصل الاجتماعي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSocialAnalyticsRepository : BaseRepository<MktSocialAnalytics>
    {
        /// <summary>
        /// Get analytics by account and date range - الحصول على التحليلات حسب الحساب والفترة
        /// </summary>
        public List<MktSocialAnalytics> GetPostAnalytics(long accountId, DateTime from, DateTime to)
        {
            return Context.Queryable<MktSocialAnalytics>()
                .Where(a => a.AccountId == accountId)
                .Where(a => a.PeriodStart >= from && a.PeriodEnd <= to)
                .OrderBy(a => a.PeriodStart)
                .ToList();
        }

        /// <summary>
        /// Get latest analytics for account - الحصول على أحدث التحليلات للحساب
        /// </summary>
        public MktSocialAnalytics GetLatestAnalytics(long accountId, string period = "daily")
        {
            return Context.Queryable<MktSocialAnalytics>()
                .Where(a => a.AccountId == accountId)
                .Where(a => a.Period == period)
                .OrderByDescending(a => a.PeriodEnd)
                .First();
        }

        /// <summary>
        /// Get analytics by period type - الحصول على التحليلات حسب نوع الفترة
        /// </summary>
        public List<MktSocialAnalytics> GetByPeriodType(long accountId, string period)
        {
            return Context.Queryable<MktSocialAnalytics>()
                .Where(a => a.AccountId == accountId)
                .Where(a => a.Period == period)
                .OrderByDescending(a => a.PeriodStart)
                .ToList();
        }
    }
}
