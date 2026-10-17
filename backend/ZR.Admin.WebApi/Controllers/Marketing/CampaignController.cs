using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Marketing.Dto;
using ZR.Model.Marketing.Campaigns;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Campaign Management Controller - إدارة الحملات التسويقية
    /// Comprehensive campaign lifecycle management with multi-channel and AI optimization
    /// </summary>
    [Verify]
    [Route("api/marketing/campaigns")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class CampaignController : BaseController
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        /// <summary>
        /// Get campaign list with filtering and pagination
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("marketing:campaign:list")]
        public IActionResult GetCampaignList([FromQuery] CampaignQueryDto dto)
        {
            var result = _campaignService.GetCampaignList(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get campaign detail by ID
        /// </summary>
        [HttpGet("{campaignId}")]
        [ActionPermissionFilter("marketing:campaign:query")]
        public IActionResult GetCampaignDetail(long campaignId)
        {
            var result = _campaignService.GetCampaignDetail(campaignId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Campaign not found");
            return SUCCESS(result);
        }

        /// <summary>
        /// Create new campaign
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("marketing:campaign:add")]
        [Log(Title = "Campaign", BusinessType = BusinessType.INSERT)]
        public IActionResult AddCampaign([FromBody] CampaignDto dto)
        {
            var result = _campaignService.AddCampaign(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update campaign
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("marketing:campaign:edit")]
        [Log(Title = "Campaign", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCampaign([FromBody] CampaignDto dto)
        {
            var result = _campaignService.UpdateCampaign(dto);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete campaigns
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:campaign:delete")]
        [Log(Title = "Campaign", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCampaigns(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _campaignService.DeleteCampaign(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Update campaign status
        /// </summary>
        [HttpPut("{campaignId}/status")]
        [ActionPermissionFilter("marketing:campaign:edit")]
        [Log(Title = "Campaign Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCampaignStatus(long campaignId, [FromBody] string status)
        {
            var result = _campaignService.UpdateStatus(campaignId, status);
            return ToResponse(result);
        }

        /// <summary>
        /// Get campaign analytics summary
        /// </summary>
        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:campaign:analytics")]
        public IActionResult GetCampaignAnalytics([FromQuery] long? eventId = null)
        {
            var result = _campaignService.GetCampaignAnalytics(eventId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get campaign performance data
        /// </summary>
        [HttpGet("{campaignId}/performance")]
        [ActionPermissionFilter("marketing:campaign:query")]
        public IActionResult GetCampaignPerformance(long campaignId, [FromQuery] CampaignPerformanceQueryDto query)
        {
            query.CampaignId = campaignId;
            var result = _campaignService.GetCampaignPerformance(query);
            return SUCCESS(result);
        }

        #region Channel Management

        /// <summary>
        /// Add channel to campaign
        /// </summary>
        [HttpPost("{campaignId}/channels")]
        [ActionPermissionFilter("marketing:campaign:edit")]
        [Log(Title = "Campaign Channel", BusinessType = BusinessType.INSERT)]
        public IActionResult AddChannel(long campaignId, [FromBody] CampaignChannelDto dto)
        {
            dto.CampaignId = campaignId;
            var result = _campaignService.AddChannel(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update campaign channel
        /// </summary>
        [HttpPut("channels")]
        [ActionPermissionFilter("marketing:campaign:edit")]
        [Log(Title = "Campaign Channel", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateChannel([FromBody] CampaignChannelDto dto)
        {
            var result = _campaignService.UpdateChannel(dto);
            return ToResponse(result);
        }

        /// <summary>
        /// Get campaign channels
        /// </summary>
        [HttpGet("{campaignId}/channels")]
        [ActionPermissionFilter("marketing:campaign:query")]
        public IActionResult GetCampaignChannels(long campaignId)
        {
            var detail = _campaignService.GetCampaignDetail(campaignId);
            return SUCCESS(detail?.Channels);
        }

        #endregion

        #region A/B Testing

        /// <summary>
        /// Create A/B test for campaign
        /// </summary>
        [HttpPost("{campaignId}/ab-tests")]
        [ActionPermissionFilter("marketing:campaign:edit")]
        [Log(Title = "A/B Test", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateAbTest(long campaignId, [FromBody] AbTestDto dto)
        {
            dto.CampaignId = campaignId;
            var result = _campaignService.CreateAbTest(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get A/B tests for campaign
        /// </summary>
        [HttpGet("{campaignId}/ab-tests")]
        [ActionPermissionFilter("marketing:campaign:query")]
        public IActionResult GetAbTests(long campaignId)
        {
            var result = _campaignService.GetAbTests(campaignId);
            return SUCCESS(result);
        }

        #endregion

        /// <summary>
        /// Export campaigns to Excel
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("marketing:campaign:export")]
        [Log(Title = "Campaign Export", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportCampaigns([FromQuery] CampaignQueryDto dto)
        {
            var list = _campaignService.GetCampaignList(dto);
            var result = ExportExcelMini(list.Result, "Campaigns", "Campaigns");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
