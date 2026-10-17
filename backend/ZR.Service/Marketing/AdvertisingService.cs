using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.Advertising;
using ZR.Repository;
using ZR.Repository.Marketing.Advertising;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Service.Marketing
{
    [AppService(ServiceType = typeof(IAdvertisingService), ServiceLifetime = LifeTime.Transient)]
    public class AdvertisingService : BaseService<MktAdCampaign>, IAdvertisingService
    {
        private readonly MktAdCreativeRepository _creativeRepo;
        private readonly MktAdPerformanceRepository _perfRepo;

        public AdvertisingService(
            MktAdCreativeRepository creativeRepo,
            MktAdPerformanceRepository perfRepo)
        {
            _creativeRepo = creativeRepo;
            _perfRepo = perfRepo;
        }

        #region Ad Campaigns

        public PagedInfo<AdCampaignOutputDto> GetAdCampaigns(AdCampaignQueryDto query)
        {
            var predicate = Expressionable.Create<MktAdCampaign>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CampaignName),
                it => it.CampaignName.Contains(query.CampaignName) || it.CampaignNameAr.Contains(query.CampaignName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Platform), it => it.Platform == query.Platform);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Objective), it => it.Objective == query.Objective);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.ParentCampaignId.HasValue, it => it.ParentCampaignId == query.ParentCampaignId);
            predicate = predicate.AndIF(query.AudienceSegmentId.HasValue, it => it.AudienceSegmentId == query.AudienceSegmentId);
            predicate = predicate.AndIF(query.AiOptimizationEnabled.HasValue, it => it.AiOptimizationEnabled == query.AiOptimizationEnabled);
            predicate = predicate.AndIF(query.DateFrom.HasValue, it => it.StartDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo.HasValue, it => it.EndDate <= query.DateTo);

            return Context.Queryable<MktAdCampaign>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktAdCampaign, AdCampaignOutputDto>(query);
        }

        public AdCampaignOutputDto GetAdCampaignDetail(long adCampaignId)
        {
            var entity = GetById(adCampaignId);
            if (entity == null) return null;

            var dto = entity.Adapt<AdCampaignOutputDto>();
            dto.CreativeCount = _creativeRepo.Context.Queryable<MktAdCreative>()
                .Where(x => x.AdCampaignId == adCampaignId)
                .Count();
            return dto;
        }

        public long AddAdCampaign(AdCampaignDto dto)
        {
            var entity = dto.Adapt<MktAdCampaign>();
            entity.Status = "draft";
            return Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateAdCampaign(AdCampaignDto dto)
        {
            var entity = dto.Adapt<MktAdCampaign>();
            return Update(entity);
        }

        public int DeleteAdCampaigns(long[] ids)
        {
            return Delete(ids);
        }

        public int UpdateAdCampaignStatus(long adCampaignId, string status)
        {
            return Context.Updateable<MktAdCampaign>()
                .SetColumns(x => x.Status == status)
                .Where(x => x.AdCampaignId == adCampaignId)
                .ExecuteCommand();
        }

        #endregion

        #region Ad Creatives

        public PagedInfo<AdCreativeOutputDto> GetCreatives(AdCreativeQueryDto query)
        {
            var predicate = Expressionable.Create<MktAdCreative>();
            predicate = predicate.AndIF(query.AdCampaignId.HasValue, it => it.AdCampaignId == query.AdCampaignId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.CreativeType), it => it.CreativeType == query.CreativeType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Status), it => it.Status == query.Status);
            predicate = predicate.AndIF(query.IsAiGenerated.HasValue, it => it.IsAiGenerated == query.IsAiGenerated);

            return _creativeRepo.Context.Queryable<MktAdCreative>()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage<MktAdCreative, AdCreativeOutputDto>(query);
        }

        public AdCreativeOutputDto GetCreativeDetail(long creativeId)
        {
            var entity = _creativeRepo.GetById(creativeId);
            return entity?.Adapt<AdCreativeOutputDto>();
        }

        public long AddCreative(AdCreativeDto dto)
        {
            var entity = dto.Adapt<MktAdCreative>();
            entity.Status = "draft";
            return _creativeRepo.Insertable(entity).ExecuteReturnSnowflakeId();
        }

        public int UpdateCreative(AdCreativeDto dto)
        {
            var entity = dto.Adapt<MktAdCreative>();
            return _creativeRepo.Update(entity);
        }

        public int DeleteCreatives(long[] ids)
        {
            return _creativeRepo.Delete(ids);
        }

        #endregion

        #region Performance & Analytics

        public List<AdPerformanceDto> GetPerformance(AdPerformanceQueryDto query)
        {
            var predicate = Expressionable.Create<MktAdPerformance>();
            predicate = predicate.And(it => it.AdCampaignId == query.AdCampaignId);
            predicate = predicate.AndIF(query.CreativeId.HasValue, it => it.CreativeId == query.CreativeId);
            predicate = predicate.AndIF(query.DateFrom.HasValue, it => it.RecordDate >= query.DateFrom);
            predicate = predicate.AndIF(query.DateTo.HasValue, it => it.RecordDate <= query.DateTo);

            return _perfRepo.Context.Queryable<MktAdPerformance>()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.RecordDate)
                .ToList()
                .Select(x => x.Adapt<AdPerformanceDto>())
                .ToList();
        }

        public AdAnalyticsDto GetAdAnalytics(DateTime? dateFrom, DateTime? dateTo)
        {
            var from = dateFrom ?? DateTime.Now.AddMonths(-3);
            var to = dateTo ?? DateTime.Now;

            var allCampaigns = Context.Queryable<MktAdCampaign>().ToList();
            var activeCampaigns = allCampaigns.Where(x => x.Status == "active").ToList();

            var perfData = _perfRepo.Context.Queryable<MktAdPerformance>()
                .Where(x => x.RecordDate >= from && x.RecordDate <= to)
                .ToList();

            var totalImpressions = perfData.Sum(x => x.Impressions);
            var totalClicks = perfData.Sum(x => x.Clicks);
            var totalConversions = perfData.Sum(x => x.Conversions);
            var totalSpent = perfData.Sum(x => x.Spend);
            var totalRevenue = perfData.Sum(x => x.Revenue ?? 0);

            return new AdAnalyticsDto
            {
                TotalAdCampaigns = allCampaigns.Count,
                ActiveAdCampaigns = activeCampaigns.Count,
                TotalBudget = allCampaigns.Sum(x => x.TotalBudget),
                TotalSpent = totalSpent,
                TotalImpressions = totalImpressions,
                TotalClicks = totalClicks,
                TotalConversions = totalConversions,
                TotalRevenue = totalRevenue,
                OverallCtr = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions * 100 : 0,
                OverallCpc = totalClicks > 0 ? totalSpent / totalClicks : 0,
                OverallCpa = totalConversions > 0 ? totalSpent / totalConversions : 0,
                OverallRoas = totalSpent > 0 ? totalRevenue / totalSpent : 0,
                PlatformBreakdown = allCampaigns
                    .GroupBy(x => x.Platform)
                    .Select(g => new PlatformAdSummaryDto
                    {
                        Platform = g.Key,
                        Budget = g.Sum(x => x.TotalBudget),
                        Spent = g.Sum(x => x.SpentAmount),
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        Conversions = g.Sum(x => x.Conversions),
                        Ctr = g.Sum(x => x.Impressions) > 0
                            ? (decimal)g.Sum(x => x.Clicks) / g.Sum(x => x.Impressions) * 100 : 0,
                        Roas = g.Sum(x => x.SpentAmount) > 0
                            ? g.Sum(x => x.Revenue ?? 0) / g.Sum(x => x.SpentAmount) : 0
                    })
                    .OrderByDescending(x => x.Spent)
                    .ToList(),
                DailyTrend = perfData
                    .GroupBy(x => x.RecordDate.Date)
                    .Select(g => new AdTrendPointDto
                    {
                        Date = g.Key,
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        Conversions = g.Sum(x => x.Conversions),
                        Spend = g.Sum(x => x.Spend),
                        Revenue = g.Sum(x => x.Revenue ?? 0)
                    })
                    .OrderBy(x => x.Date)
                    .ToList(),
                TopCampaigns = allCampaigns
                    .Where(x => x.Revenue.HasValue && x.Revenue > 0)
                    .OrderByDescending(x => x.Roas)
                    .Take(10)
                    .Select(x => new TopAdCampaignDto
                    {
                        AdCampaignId = x.AdCampaignId,
                        CampaignName = x.CampaignName,
                        Platform = x.Platform,
                        Roas = x.Roas,
                        Revenue = x.Revenue ?? 0,
                        Conversions = x.Conversions
                    })
                    .ToList()
            };
        }

        public List<PlatformAdSummaryDto> GetPlatformSpending(DateTime? dateFrom, DateTime? dateTo)
        {
            var from = dateFrom ?? DateTime.Now.AddMonths(-3);
            var to = dateTo ?? DateTime.Now;

            var campaigns = Context.Queryable<MktAdCampaign>()
                .Where(x => x.StartDate <= to)
                .Where(x => x.EndDate == null || x.EndDate >= from)
                .ToList();

            return campaigns
                .GroupBy(x => x.Platform)
                .Select(g => new PlatformAdSummaryDto
                {
                    Platform = g.Key,
                    Budget = g.Sum(x => x.TotalBudget),
                    Spent = g.Sum(x => x.SpentAmount),
                    Impressions = g.Sum(x => x.Impressions),
                    Clicks = g.Sum(x => x.Clicks),
                    Conversions = g.Sum(x => x.Conversions),
                    Ctr = g.Sum(x => x.Impressions) > 0
                        ? (decimal)g.Sum(x => x.Clicks) / g.Sum(x => x.Impressions) * 100 : 0,
                    Roas = g.Sum(x => x.SpentAmount) > 0
                        ? g.Sum(x => x.Revenue ?? 0) / g.Sum(x => x.SpentAmount) : 0
                })
                .OrderByDescending(x => x.Spent)
                .ToList();
        }

        #endregion
    }
}
