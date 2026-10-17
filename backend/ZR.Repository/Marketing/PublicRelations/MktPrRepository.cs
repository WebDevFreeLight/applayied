using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.PublicRelations;
using ZR.Repository;

namespace ZR.Repository.Marketing.PublicRelations
{
    /// <summary>
    /// Press Release Repository - مستودع البيانات الصحفية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktPressReleaseRepository : BaseRepository<MktPressRelease>
    {
        #region Press Release Queries

        /// <summary>
        /// Get published releases - الحصول على البيانات الصحفية المنشورة
        /// </summary>
        public List<MktPressRelease> GetPublishedReleases()
        {
            return Context.Queryable<MktPressRelease>()
                .Where(r => r.Status == "published" || r.Status == "distributed")
                .OrderByDescending(r => r.PublishDate)
                .ToList();
        }

        /// <summary>
        /// Get releases by category - الحصول على البيانات حسب الفئة
        /// </summary>
        public List<MktPressRelease> GetByCategory(string category)
        {
            return Context.Queryable<MktPressRelease>()
                .Where(r => r.Category == category)
                .OrderByDescending(r => r.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get releases pending approval - الحصول على البيانات بانتظار الموافقة
        /// </summary>
        public List<MktPressRelease> GetPendingApproval()
        {
            return Context.Queryable<MktPressRelease>()
                .Where(r => r.Status == "review")
                .OrderBy(r => r.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get releases by event - الحصول على البيانات حسب الحدث
        /// </summary>
        public List<MktPressRelease> GetByEventId(long eventId)
        {
            return Context.Queryable<MktPressRelease>()
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.PublishDate)
                .ToList();
        }

        /// <summary>
        /// Get releases ready for distribution - الحصول على البيانات الجاهزة للتوزيع
        /// </summary>
        public List<MktPressRelease> GetReadyForDistribution()
        {
            return Context.Queryable<MktPressRelease>()
                .Where(r => r.Status == "approved")
                .OrderBy(r => r.Create_time)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Media Contact Repository - مستودع جهات الاتصال الإعلامية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktMediaContactRepository : BaseRepository<MktMediaContact>
    {
        /// <summary>
        /// Get active contacts ordered by reach - الحصول على جهات الاتصال النشطة حسب الوصول
        /// </summary>
        public List<MktMediaContact> GetMediaContactsByReach()
        {
            return Context.Queryable<MktMediaContact>()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.EstimatedReach)
                .ToList();
        }

        /// <summary>
        /// Get contacts by type - الحصول على جهات الاتصال حسب النوع
        /// </summary>
        public List<MktMediaContact> GetByContactType(string contactType)
        {
            return Context.Queryable<MktMediaContact>()
                .Where(c => c.ContactType == contactType)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.InfluenceScore)
                .ToList();
        }

        /// <summary>
        /// Get contacts by specialty - الحصول على جهات الاتصال حسب التخصص
        /// </summary>
        public List<MktMediaContact> GetBySpecialty(string specialty)
        {
            return Context.Queryable<MktMediaContact>()
                .Where(c => c.Specialty == specialty)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.InfluenceScore)
                .ToList();
        }

        /// <summary>
        /// Get contacts by relationship level - الحصول على جهات الاتصال حسب مستوى العلاقة
        /// </summary>
        public List<MktMediaContact> GetByRelationshipLevel(string level)
        {
            return Context.Queryable<MktMediaContact>()
                .Where(c => c.RelationshipLevel == level)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.TotalCoverages)
                .ToList();
        }

        /// <summary>
        /// Get contacts by reach level - الحصول على جهات الاتصال حسب مستوى الوصول
        /// </summary>
        public List<MktMediaContact> GetByReachLevel(string reach)
        {
            return Context.Queryable<MktMediaContact>()
                .Where(c => c.Reach == reach)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.InfluenceScore)
                .ToList();
        }
    }

    /// <summary>
    /// Media Mention Repository - مستودع التغطيات الإعلامية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktMediaMentionRepository : BaseRepository<MktMediaMention>
    {
        /// <summary>
        /// Get mentions by sentiment - الحصول على التغطيات حسب المشاعر
        /// </summary>
        public List<MktMediaMention> GetMentionsBySentiment(string sentiment)
        {
            return Context.Queryable<MktMediaMention>()
                .Where(m => m.Sentiment == sentiment)
                .OrderByDescending(m => m.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get mentions by press release - الحصول على التغطيات حسب البيان الصحفي
        /// </summary>
        public List<MktMediaMention> GetByPressReleaseId(long pressReleaseId)
        {
            return Context.Queryable<MktMediaMention>()
                .Where(m => m.PressReleaseId == pressReleaseId)
                .OrderByDescending(m => m.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get mentions by event - الحصول على التغطيات حسب الحدث
        /// </summary>
        public List<MktMediaMention> GetByEventId(long eventId)
        {
            return Context.Queryable<MktMediaMention>()
                .Where(m => m.EventId == eventId)
                .OrderByDescending(m => m.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// Get featured mentions - الحصول على التغطيات المميزة
        /// </summary>
        public List<MktMediaMention> GetFeaturedMentions()
        {
            return Context.Queryable<MktMediaMention>()
                .Where(m => m.IsFeatured)
                .OrderByDescending(m => m.EstimatedReach)
                .ToList();
        }

        /// <summary>
        /// Get mentions by date range - الحصول على التغطيات حسب الفترة
        /// </summary>
        public List<MktMediaMention> GetByDateRange(DateTime from, DateTime to)
        {
            return Context.Queryable<MktMediaMention>()
                .Where(m => m.PublishedAt >= from && m.PublishedAt <= to)
                .OrderByDescending(m => m.PublishedAt)
                .ToList();
        }
    }

    /// <summary>
    /// PR Sentiment Repository - مستودع تحليل المشاعر العامة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktPrSentimentRepository : BaseRepository<MktPrSentiment>
    {
        /// <summary>
        /// Get latest sentiment data - الحصول على أحدث بيانات المشاعر
        /// </summary>
        public MktPrSentiment GetLatestSentiment(string period = "daily")
        {
            return Context.Queryable<MktPrSentiment>()
                .Where(s => s.Period == period)
                .OrderByDescending(s => s.PeriodEnd)
                .First();
        }

        /// <summary>
        /// Get sentiment by date range - الحصول على المشاعر حسب الفترة
        /// </summary>
        public List<MktPrSentiment> GetByDateRange(DateTime from, DateTime to, string period = null)
        {
            var query = Context.Queryable<MktPrSentiment>()
                .Where(s => s.PeriodStart >= from && s.PeriodEnd <= to);

            if (!string.IsNullOrEmpty(period))
            {
                query = query.Where(s => s.Period == period);
            }

            return query.OrderBy(s => s.PeriodStart).ToList();
        }

        /// <summary>
        /// Get sentiment by event - الحصول على المشاعر حسب الحدث
        /// </summary>
        public List<MktPrSentiment> GetByEventId(long eventId, string period = null)
        {
            var query = Context.Queryable<MktPrSentiment>()
                .Where(s => s.EventId == eventId);

            if (!string.IsNullOrEmpty(period))
            {
                query = query.Where(s => s.Period == period);
            }

            return query.OrderBy(s => s.PeriodStart).ToList();
        }
    }
}
