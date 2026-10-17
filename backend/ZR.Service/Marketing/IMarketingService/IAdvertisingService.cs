using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Marketing.Advertising;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Advertising Service Interface - خدمة الإعلانات
    /// </summary>
    public interface IAdvertisingService : IBaseService<MktAdCampaign>
    {
        #region Ad Campaigns

        PagedInfo<AdCampaignOutputDto> GetAdCampaigns(AdCampaignQueryDto query);
        AdCampaignOutputDto GetAdCampaignDetail(long adCampaignId);
        long AddAdCampaign(AdCampaignDto dto);
        int UpdateAdCampaign(AdCampaignDto dto);
        int DeleteAdCampaigns(long[] ids);
        int UpdateAdCampaignStatus(long adCampaignId, string status);

        #endregion

        #region Ad Creatives

        PagedInfo<AdCreativeOutputDto> GetCreatives(AdCreativeQueryDto query);
        AdCreativeOutputDto GetCreativeDetail(long creativeId);
        long AddCreative(AdCreativeDto dto);
        int UpdateCreative(AdCreativeDto dto);
        int DeleteCreatives(long[] ids);

        #endregion

        #region Performance & Analytics

        List<AdPerformanceDto> GetPerformance(AdPerformanceQueryDto query);
        AdAnalyticsDto GetAdAnalytics(DateTime? dateFrom, DateTime? dateTo);
        List<PlatformAdSummaryDto> GetPlatformSpending(DateTime? dateFrom, DateTime? dateTo);

        #endregion
    }
}
