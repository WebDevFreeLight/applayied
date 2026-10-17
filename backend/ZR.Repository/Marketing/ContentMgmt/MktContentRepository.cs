using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.ContentMgmt;
using ZR.Repository;

namespace ZR.Repository.Marketing.ContentMgmt
{
    /// <summary>
    /// Marketing Content Repository - مستودع المحتوى التسويقي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktContentRepository : BaseRepository<MktContent>
    {
        #region Content Queries

        /// <summary>
        /// Get published content - الحصول على المحتوى المنشور
        /// </summary>
        public List<MktContent> GetPublishedContent()
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.Status == "published")
                .OrderByDescending(c => c.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get content by type - الحصول على المحتوى حسب النوع
        /// </summary>
        public List<MktContent> GetContentByType(string type)
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.ContentType == type)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get content pending approvals - الحصول على المحتوى بانتظار الموافقة
        /// </summary>
        public List<MktContent> GetPendingApprovals()
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.Status == "review")
                .OrderBy(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get content by category - الحصول على المحتوى حسب الفئة
        /// </summary>
        public List<MktContent> GetByCategory(string category)
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.Category == category)
                .Where(c => c.Status == "published")
                .OrderByDescending(c => c.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get content by campaign - الحصول على المحتوى حسب الحملة
        /// </summary>
        public List<MktContent> GetByCampaignId(long campaignId)
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.CampaignId == campaignId)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get content by event - الحصول على المحتوى حسب الحدث
        /// </summary>
        public List<MktContent> GetByEventId(long eventId)
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.EventId == eventId)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get top performing content - الحصول على المحتوى الأفضل أداء
        /// </summary>
        public List<MktContent> GetTopPerformingContent(int count = 10)
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.Status == "published")
                .OrderByDescending(c => c.EngagementRate)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Get AI-generated content - الحصول على المحتوى المولد بالذكاء الاصطناعي
        /// </summary>
        public List<MktContent> GetAiGeneratedContent()
        {
            return Context.Queryable<MktContent>()
                .Where(c => c.IsAiGenerated)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// AI Content Generation Repository - مستودع إنشاء المحتوى بالذكاء الاصطناعي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAiContentGenerationRepository : BaseRepository<MktAiContentGeneration>
    {
        /// <summary>
        /// Get generations by type - الحصول على المحتوى المولد حسب النوع
        /// </summary>
        public List<MktAiContentGeneration> GetByGenerationType(string generationType)
        {
            return Context.Queryable<MktAiContentGeneration>()
                .Where(g => g.GenerationType == generationType)
                .OrderByDescending(g => g.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Get pending approval generations - الحصول على المحتوى المولد بانتظار الموافقة
        /// </summary>
        public List<MktAiContentGeneration> GetPendingApproval()
        {
            return Context.Queryable<MktAiContentGeneration>()
                .Where(g => g.Status == "completed" && !g.IsApproved)
                .OrderByDescending(g => g.QualityScore)
                .ToList();
        }

        /// <summary>
        /// Get generations by provider - الحصول على المحتوى المولد حسب المزود
        /// </summary>
        public List<MktAiContentGeneration> GetByProvider(string provider)
        {
            return Context.Queryable<MktAiContentGeneration>()
                .Where(g => g.AiProvider == provider)
                .OrderByDescending(g => g.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Get generation by linked content ID - الحصول على المحتوى المولد حسب المحتوى المرتبط
        /// </summary>
        public MktAiContentGeneration GetByLinkedContentId(long contentId)
        {
            return Context.Queryable<MktAiContentGeneration>()
                .Where(g => g.LinkedContentId == contentId)
                .OrderByDescending(g => g.CreatedAt)
                .First();
        }

        /// <summary>
        /// Get recent generations by user - الحصول على المحتوى المولد مؤخرا حسب المستخدم
        /// </summary>
        public List<MktAiContentGeneration> GetRecentByUser(long userId, int count = 20)
        {
            return Context.Queryable<MktAiContentGeneration>()
                .Where(g => g.RequestedBy == userId)
                .OrderByDescending(g => g.CreatedAt)
                .Take(count)
                .ToList();
        }
    }
}
