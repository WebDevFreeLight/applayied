using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.EmailMarketing;
using ZR.Repository;

namespace ZR.Repository.Marketing.EmailMarketing
{
    /// <summary>
    /// Email Template Repository - مستودع قوالب البريد الإلكتروني
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktEmailTemplateRepository : BaseRepository<MktEmailTemplate>
    {
        #region Template Queries

        /// <summary>
        /// Get active templates - الحصول على القوالب النشطة
        /// </summary>
        public List<MktEmailTemplate> GetActiveTemplates()
        {
            return Context.Queryable<MktEmailTemplate>()
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.TimesUsed)
                .ToList();
        }

        /// <summary>
        /// Get templates by category - الحصول على القوالب حسب الفئة
        /// </summary>
        public List<MktEmailTemplate> GetByCategory(string category)
        {
            return Context.Queryable<MktEmailTemplate>()
                .Where(t => t.Category == category)
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.AvgOpenRate)
                .ToList();
        }

        /// <summary>
        /// Get template by code - الحصول على القالب بالرمز
        /// </summary>
        public MktEmailTemplate GetByTemplateCode(string templateCode)
        {
            return Context.Queryable<MktEmailTemplate>()
                .Where(t => t.TemplateCode == templateCode)
                .First();
        }

        /// <summary>
        /// Get top performing templates - الحصول على القوالب الأفضل أداء
        /// </summary>
        public List<MktEmailTemplate> GetTopPerformingTemplates(int count = 10)
        {
            return Context.Queryable<MktEmailTemplate>()
                .Where(t => t.IsActive)
                .Where(t => t.TimesUsed > 0)
                .OrderByDescending(t => t.AvgOpenRate)
                .Take(count)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Email Campaign Repository - مستودع حملات البريد الإلكتروني
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktEmailCampaignRepository : BaseRepository<MktEmailCampaign>
    {
        /// <summary>
        /// Get campaign with metrics - الحصول على الحملة مع المقاييس
        /// </summary>
        public MktEmailCampaign GetCampaignWithMetrics(long campaignId)
        {
            return Context.Queryable<MktEmailCampaign>()
                .Where(c => c.EmailCampaignId == campaignId)
                .First();
        }

        /// <summary>
        /// Get campaigns by status - الحصول على الحملات حسب الحالة
        /// </summary>
        public List<MktEmailCampaign> GetByStatus(string status)
        {
            return Context.Queryable<MktEmailCampaign>()
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get scheduled campaigns due for sending - الحصول على الحملات المجدولة المستحقة
        /// </summary>
        public List<MktEmailCampaign> GetScheduledCampaignsDue()
        {
            return Context.Queryable<MktEmailCampaign>()
                .Where(c => c.Status == "scheduled")
                .Where(c => c.ScheduledAt != null && c.ScheduledAt <= DateTime.Now)
                .OrderBy(c => c.ScheduledAt)
                .ToList();
        }

        /// <summary>
        /// Get campaigns by parent campaign - الحصول على حملات البريد حسب الحملة الأم
        /// </summary>
        public List<MktEmailCampaign> GetByParentCampaignId(long parentCampaignId)
        {
            return Context.Queryable<MktEmailCampaign>()
                .Where(c => c.ParentCampaignId == parentCampaignId)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }
    }

    /// <summary>
    /// Subscriber Repository - مستودع المشتركين
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSubscriberRepository : BaseRepository<MktSubscriber>
    {
        /// <summary>
        /// Get subscriber by email - الحصول على المشترك بالبريد الإلكتروني
        /// </summary>
        public MktSubscriber GetSubscriberByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return Context.Queryable<MktSubscriber>()
                .Where(s => s.Email == email)
                .First();
        }

        /// <summary>
        /// Get active subscribers - الحصول على المشتركين النشطين
        /// </summary>
        public List<MktSubscriber> GetActiveSubscribers()
        {
            return Context.Queryable<MktSubscriber>()
                .Where(s => s.Status == "active")
                .OrderByDescending(s => s.EngagementScore)
                .ToList();
        }

        /// <summary>
        /// Get subscribers by type - الحصول على المشتركين حسب النوع
        /// </summary>
        public List<MktSubscriber> GetBySubscriberType(string subscriberType)
        {
            return Context.Queryable<MktSubscriber>()
                .Where(s => s.SubscriberType == subscriberType)
                .Where(s => s.Status == "active")
                .OrderByDescending(s => s.EngagementScore)
                .ToList();
        }

        /// <summary>
        /// Get unengaged subscribers - الحصول على المشتركين غير المتفاعلين
        /// </summary>
        public List<MktSubscriber> GetUnengagedSubscribers(int daysSinceLastOpen = 90)
        {
            var threshold = DateTime.Now.AddDays(-daysSinceLastOpen);
            return Context.Queryable<MktSubscriber>()
                .Where(s => s.Status == "active")
                .Where(s => s.LastOpenedAt == null || s.LastOpenedAt < threshold)
                .OrderBy(s => s.LastOpenedAt)
                .ToList();
        }
    }

    /// <summary>
    /// Subscriber List Repository - مستودع قوائم المشتركين
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSubscriberListRepository : BaseRepository<MktSubscriberList>
    {
        /// <summary>
        /// Get active lists - الحصول على القوائم النشطة
        /// </summary>
        public List<MktSubscriberList> GetActiveLists()
        {
            return Context.Queryable<MktSubscriberList>()
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.MemberCount)
                .ToList();
        }

        /// <summary>
        /// Get dynamic lists needing refresh - الحصول على القوائم الديناميكية التي تحتاج تحديث
        /// </summary>
        public List<MktSubscriberList> GetDynamicListsNeedingRefresh(int hoursThreshold = 24)
        {
            var threshold = DateTime.Now.AddHours(-hoursThreshold);
            return Context.Queryable<MktSubscriberList>()
                .Where(l => l.IsActive && l.ListType == "dynamic")
                .Where(l => l.LastRefreshedAt == null || l.LastRefreshedAt < threshold)
                .ToList();
        }
    }

    /// <summary>
    /// Subscriber List Member Repository - مستودع أعضاء القائمة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktSubscriberListMemberRepository : BaseRepository<MktSubscriberListMember>
    {
        /// <summary>
        /// Get subscribers by list - الحصول على المشتركين حسب القائمة
        /// </summary>
        public List<MktSubscriberListMember> GetSubscribersByList(long listId)
        {
            return Context.Queryable<MktSubscriberListMember>()
                .Where(m => m.ListId == listId)
                .OrderByDescending(m => m.AddedAt)
                .ToList();
        }

        /// <summary>
        /// Get member lists - الحصول على قوائم العضو
        /// </summary>
        public List<MktSubscriberListMember> GetMemberLists(long subscriberId)
        {
            return Context.Queryable<MktSubscriberListMember>()
                .Where(m => m.SubscriberId == subscriberId)
                .ToList();
        }

        /// <summary>
        /// Check if subscriber is in list - التحقق من وجود المشترك في القائمة
        /// </summary>
        public bool IsSubscriberInList(long listId, long subscriberId)
        {
            return Context.Queryable<MktSubscriberListMember>()
                .Where(m => m.ListId == listId && m.SubscriberId == subscriberId)
                .Any();
        }
    }
}
