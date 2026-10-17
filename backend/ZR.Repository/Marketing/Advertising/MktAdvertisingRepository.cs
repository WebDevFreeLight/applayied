using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.Advertising;
using ZR.Repository;

namespace ZR.Repository.Marketing.Advertising
{
    /// <summary>
    /// Ad Campaign Repository - مستودع الحملات الإعلانية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAdCampaignRepository : BaseRepository<MktAdCampaign>
    {
        #region Ad Campaign Queries

        /// <summary>
        /// Get active campaigns by platform - الحصول على الحملات النشطة حسب المنصة
        /// </summary>
        public List<MktAdCampaign> GetActiveCampaignsByPlatform(string platform)
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.Platform == platform)
                .Where(c => c.Status == "active")
                .Where(c => c.StartDate <= DateTime.Now)
                .Where(c => c.EndDate == null || c.EndDate >= DateTime.Now)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get all active ad campaigns - الحصول على جميع الحملات الإعلانية النشطة
        /// </summary>
        public List<MktAdCampaign> GetAllActiveCampaigns()
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.Status == "active")
                .Where(c => c.StartDate <= DateTime.Now)
                .Where(c => c.EndDate == null || c.EndDate >= DateTime.Now)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get campaigns by parent campaign - الحصول على الحملات الإعلانية حسب الحملة الأم
        /// </summary>
        public List<MktAdCampaign> GetByParentCampaignId(long parentCampaignId)
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.ParentCampaignId == parentCampaignId)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get campaigns by objective - الحصول على الحملات حسب الهدف
        /// </summary>
        public List<MktAdCampaign> GetByObjective(string objective)
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.Objective == objective)
                .Where(c => c.Status == "active")
                .OrderByDescending(c => c.Roas)
                .ToList();
        }

        /// <summary>
        /// Get campaigns nearing budget limit - الحصول على الحملات القريبة من حد الميزانية
        /// </summary>
        public List<MktAdCampaign> GetCampaignsNearBudgetLimit(decimal thresholdPercent = 90m)
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.Status == "active")
                .Where(c => c.TotalBudget > 0)
                .Where(c => (c.SpentAmount / c.TotalBudget) * 100 >= thresholdPercent)
                .OrderByDescending(c => c.SpentAmount)
                .ToList();
        }

        /// <summary>
        /// Get campaigns with AI optimization enabled - الحصول على الحملات مع تحسين الذكاء الاصطناعي
        /// </summary>
        public List<MktAdCampaign> GetAiOptimizedCampaigns()
        {
            return Context.Queryable<MktAdCampaign>()
                .Where(c => c.AiOptimizationEnabled)
                .Where(c => c.Status == "active")
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        #endregion
    }

    /// <summary>
    /// Ad Creative Repository - مستودع التصاميم الإعلانية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAdCreativeRepository : BaseRepository<MktAdCreative>
    {
        /// <summary>
        /// Get creatives by campaign - الحصول على التصاميم حسب الحملة
        /// </summary>
        public List<MktAdCreative> GetByCampaignId(long adCampaignId)
        {
            return Context.Queryable<MktAdCreative>()
                .Where(c => c.AdCampaignId == adCampaignId)
                .OrderByDescending(c => c.Ctr)
                .ToList();
        }

        /// <summary>
        /// Get active creatives - الحصول على التصاميم النشطة
        /// </summary>
        public List<MktAdCreative> GetActiveCreatives(long adCampaignId)
        {
            return Context.Queryable<MktAdCreative>()
                .Where(c => c.AdCampaignId == adCampaignId)
                .Where(c => c.Status == "active")
                .OrderByDescending(c => c.Ctr)
                .ToList();
        }

        /// <summary>
        /// Get creatives by type - الحصول على التصاميم حسب النوع
        /// </summary>
        public List<MktAdCreative> GetByCreativeType(string creativeType)
        {
            return Context.Queryable<MktAdCreative>()
                .Where(c => c.CreativeType == creativeType)
                .Where(c => c.Status == "active")
                .OrderByDescending(c => c.EngagementRate)
                .ToList();
        }

        /// <summary>
        /// Get creatives pending review - الحصول على التصاميم بانتظار المراجعة
        /// </summary>
        public List<MktAdCreative> GetPendingReview()
        {
            return Context.Queryable<MktAdCreative>()
                .Where(c => c.Status == "review")
                .OrderBy(c => c.Create_time)
                .ToList();
        }
    }

    /// <summary>
    /// Ad Performance Repository - مستودع أداء الإعلانات
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAdPerformanceRepository : BaseRepository<MktAdPerformance>
    {
        /// <summary>
        /// Get campaign performance by date range - الحصول على أداء الحملة حسب الفترة
        /// </summary>
        public List<MktAdPerformance> GetCampaignPerformance(long campaignId, DateTime from, DateTime to)
        {
            return Context.Queryable<MktAdPerformance>()
                .Where(p => p.AdCampaignId == campaignId)
                .Where(p => p.RecordDate >= from && p.RecordDate <= to)
                .OrderBy(p => p.RecordDate)
                .ToList();
        }

        /// <summary>
        /// Get performance by creative - الحصول على الأداء حسب التصميم
        /// </summary>
        public List<MktAdPerformance> GetByCreativeId(long creativeId, DateTime? from, DateTime? to)
        {
            var query = Context.Queryable<MktAdPerformance>()
                .Where(p => p.CreativeId == creativeId);

            if (from.HasValue)
            {
                query = query.Where(p => p.RecordDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(p => p.RecordDate <= to.Value);
            }

            return query.OrderBy(p => p.RecordDate).ToList();
        }

        /// <summary>
        /// Get latest performance snapshot - الحصول على أحدث لقطة أداء
        /// </summary>
        public MktAdPerformance GetLatestPerformance(long campaignId)
        {
            return Context.Queryable<MktAdPerformance>()
                .Where(p => p.AdCampaignId == campaignId)
                .OrderByDescending(p => p.RecordDate)
                .First();
        }

        /// <summary>
        /// Get total spend for campaign in date range - إجمالي الإنفاق للحملة في فترة
        /// </summary>
        public decimal GetTotalSpend(long campaignId, DateTime from, DateTime to)
        {
            return Context.Queryable<MktAdPerformance>()
                .Where(p => p.AdCampaignId == campaignId)
                .Where(p => p.RecordDate >= from && p.RecordDate <= to)
                .Sum(p => p.Spend);
        }
    }
}
