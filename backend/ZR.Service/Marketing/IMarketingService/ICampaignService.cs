using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Marketing.Campaigns;
using ZR.Model.Marketing.Dto;

namespace ZR.Service.Marketing.IMarketingService
{
    /// <summary>
    /// Campaign Service Interface - خدمة الحملات التسويقية
    /// </summary>
    public interface ICampaignService : IBaseService<MktCampaign>
    {
        /// <summary>
        /// Get paginated campaign list with filtering
        /// </summary>
        PagedInfo<CampaignOutputDto> GetCampaignList(CampaignQueryDto query);

        /// <summary>
        /// Get campaign detail by ID with channels and performance
        /// </summary>
        CampaignOutputDto GetCampaignDetail(long campaignId);

        /// <summary>
        /// Add a new campaign
        /// </summary>
        MktCampaign AddCampaign(CampaignDto dto);

        /// <summary>
        /// Update an existing campaign
        /// </summary>
        int UpdateCampaign(CampaignDto dto);

        /// <summary>
        /// Delete campaigns by IDs
        /// </summary>
        int DeleteCampaign(long[] campaignIds);

        /// <summary>
        /// Update campaign status
        /// </summary>
        int UpdateStatus(long campaignId, string status);

        /// <summary>
        /// Get campaign analytics summary
        /// </summary>
        CampaignAnalyticsDto GetCampaignAnalytics(long? eventId = null);

        /// <summary>
        /// Get campaign performance data
        /// </summary>
        List<CampaignPerformanceDto> GetCampaignPerformance(CampaignPerformanceQueryDto query);

        /// <summary>
        /// Add a channel to a campaign
        /// </summary>
        MktCampaignChannel AddChannel(CampaignChannelDto dto);

        /// <summary>
        /// Update a campaign channel
        /// </summary>
        int UpdateChannel(CampaignChannelDto dto);

        /// <summary>
        /// Create an A/B test for a campaign
        /// </summary>
        MktAbTest CreateAbTest(AbTestDto dto);

        /// <summary>
        /// Get A/B tests for a campaign
        /// </summary>
        List<AbTestDto> GetAbTests(long campaignId);
    }
}
