using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.Marketing.Campaigns;
using ZR.Repository;

namespace ZR.Repository.Marketing.Campaigns
{
    /// <summary>
    /// Marketing Campaign Repository - مستودع الحملات التسويقية
    /// Core campaign data access with multi-channel and analytics support
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCampaignRepository : BaseRepository<MktCampaign>
    {
        #region Campaign Queries

        /// <summary>
        /// Get campaigns with their associated channels - الحصول على الحملات مع القنوات
        /// </summary>
        public List<MktCampaign> GetCampaignsWithChannels()
        {
            var campaigns = Context.Queryable<MktCampaign>()
                .OrderByDescending(c => c.Create_time)
                .ToList();

            if (campaigns.Any())
            {
                var campaignIds = campaigns.Select(c => c.CampaignId).ToList();
                var channels = Context.Queryable<MktCampaignChannel>()
                    .Where(ch => campaignIds.Contains(ch.CampaignId))
                    .ToList();

                foreach (var campaign in campaigns)
                {
                    campaign.Channels = channels.Where(ch => ch.CampaignId == campaign.CampaignId).ToList();
                }
            }

            return campaigns;
        }

        /// <summary>
        /// Get all active campaigns - الحصول على الحملات النشطة
        /// </summary>
        public List<MktCampaign> GetActiveCampaigns()
        {
            return Context.Queryable<MktCampaign>()
                .Where(c => c.Status == CampaignStatus.ACTIVE)
                .Where(c => c.StartDate <= DateTime.Now && c.EndDate >= DateTime.Now)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get campaigns by event ID - الحصول على حملات الحدث
        /// </summary>
        public List<MktCampaign> GetCampaignsByEvent(long eventId)
        {
            return Context.Queryable<MktCampaign>()
                .Where(c => c.EventId == eventId)
                .OrderByDescending(c => c.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get campaign analytics with performance data - تحليلات الحملات مع بيانات الأداء
        /// </summary>
        public List<MktCampaign> GetCampaignAnalytics(DateTime? from, DateTime? to)
        {
            var query = Context.Queryable<MktCampaign>()
                .WhereIF(from.HasValue, c => c.StartDate >= from.Value)
                .WhereIF(to.HasValue, c => c.EndDate <= to.Value)
                .OrderByDescending(c => c.Create_time);

            var campaigns = query.ToList();

            if (campaigns.Any())
            {
                var campaignIds = campaigns.Select(c => c.CampaignId).ToList();

                var channels = Context.Queryable<MktCampaignChannel>()
                    .Where(ch => campaignIds.Contains(ch.CampaignId))
                    .ToList();

                var performances = Context.Queryable<MktCampaignPerformance>()
                    .Where(p => campaignIds.Contains(p.CampaignId))
                    .WhereIF(from.HasValue, p => p.RecordDate >= from.Value)
                    .WhereIF(to.HasValue, p => p.RecordDate <= to.Value)
                    .ToList();

                foreach (var campaign in campaigns)
                {
                    campaign.Channels = channels.Where(ch => ch.CampaignId == campaign.CampaignId).ToList();
                    campaign.PerformanceRecords = performances.Where(p => p.CampaignId == campaign.CampaignId).ToList();
                }
            }

            return campaigns;
        }

        #endregion
    }

    /// <summary>
    /// Campaign Channel Repository - مستودع قنوات الحملة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCampaignChannelRepository : BaseRepository<MktCampaignChannel>
    {
        /// <summary>
        /// Get channels by campaign ID - الحصول على قنوات الحملة
        /// </summary>
        public List<MktCampaignChannel> GetByCampaignId(long campaignId)
        {
            return Context.Queryable<MktCampaignChannel>()
                .Where(ch => ch.CampaignId == campaignId)
                .OrderBy(ch => ch.ChannelType)
                .ToList();
        }

        /// <summary>
        /// Get active channels - الحصول على القنوات النشطة
        /// </summary>
        public List<MktCampaignChannel> GetActiveChannels(long campaignId)
        {
            return Context.Queryable<MktCampaignChannel>()
                .Where(ch => ch.CampaignId == campaignId)
                .Where(ch => ch.Status == "active")
                .ToList();
        }
    }

    /// <summary>
    /// Campaign Performance Repository - مستودع أداء الحملات
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktCampaignPerformanceRepository : BaseRepository<MktCampaignPerformance>
    {
        /// <summary>
        /// Get performance records by campaign and date range - سجلات الأداء حسب الحملة والفترة
        /// </summary>
        public List<MktCampaignPerformance> GetByCampaignAndDateRange(long campaignId, DateTime? from, DateTime? to)
        {
            return Context.Queryable<MktCampaignPerformance>()
                .Where(p => p.CampaignId == campaignId)
                .WhereIF(from.HasValue, p => p.RecordDate >= from.Value)
                .WhereIF(to.HasValue, p => p.RecordDate <= to.Value)
                .OrderBy(p => p.RecordDate)
                .ToList();
        }

        /// <summary>
        /// Get latest performance snapshot - أحدث لقطة أداء
        /// </summary>
        public MktCampaignPerformance GetLatestPerformance(long campaignId)
        {
            return Context.Queryable<MktCampaignPerformance>()
                .Where(p => p.CampaignId == campaignId)
                .OrderByDescending(p => p.RecordDate)
                .First();
        }
    }

    /// <summary>
    /// A/B Test Repository - مستودع اختبار A/B
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class MktAbTestRepository : BaseRepository<MktAbTest>
    {
        /// <summary>
        /// Get A/B tests by campaign - الحصول على اختبارات A/B حسب الحملة
        /// </summary>
        public List<MktAbTest> GetByCampaignId(long campaignId)
        {
            return Context.Queryable<MktAbTest>()
                .Where(t => t.CampaignId == campaignId)
                .OrderByDescending(t => t.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get running tests - الحصول على الاختبارات الجارية
        /// </summary>
        public List<MktAbTest> GetRunningTests()
        {
            return Context.Queryable<MktAbTest>()
                .Where(t => t.Status == "running")
                .OrderBy(t => t.StartDate)
                .ToList();
        }
    }
}
