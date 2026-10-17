using Microsoft.AspNetCore.Mvc;
using System;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.Marketing.Dto;
using ZR.Service.Marketing.IMarketingService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Advertising Controller - إدارة الحملات الإعلانية
    /// Multi-platform paid advertising management with AI optimization
    /// </summary>
    [Verify]
    [Route("api/marketing/ads")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class AdvertisingController : BaseController
    {
        private readonly IAdvertisingService _adService;

        public AdvertisingController(IAdvertisingService adService)
        {
            _adService = adService;
        }

        #region Ad Campaigns

        [HttpGet("campaigns")]
        [ActionPermissionFilter("marketing:ads:campaigns")]
        public IActionResult GetAdCampaigns([FromQuery] AdCampaignQueryDto dto)
        {
            var result = _adService.GetAdCampaigns(dto);
            return SUCCESS(result);
        }

        [HttpGet("campaigns/{adCampaignId}")]
        [ActionPermissionFilter("marketing:ads:campaigns")]
        public IActionResult GetAdCampaignDetail(long adCampaignId)
        {
            var result = _adService.GetAdCampaignDetail(adCampaignId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Ad campaign not found");
            return SUCCESS(result);
        }

        [HttpPost("campaigns")]
        [ActionPermissionFilter("marketing:ads:campaigns:add")]
        [Log(Title = "Ad Campaign", BusinessType = BusinessType.INSERT)]
        public IActionResult AddAdCampaign([FromBody] AdCampaignDto dto)
        {
            var result = _adService.AddAdCampaign(dto);
            return SUCCESS(result);
        }

        [HttpPut("campaigns")]
        [ActionPermissionFilter("marketing:ads:campaigns:edit")]
        [Log(Title = "Ad Campaign", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAdCampaign([FromBody] AdCampaignDto dto)
        {
            var result = _adService.UpdateAdCampaign(dto);
            return ToResponse(result);
        }

        [HttpDelete("campaigns/{ids}")]
        [ActionPermissionFilter("marketing:ads:campaigns:delete")]
        [Log(Title = "Ad Campaign", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteAdCampaigns(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _adService.DeleteAdCampaigns(idArray);
            return ToResponse(result);
        }

        [HttpPut("campaigns/{adCampaignId}/status")]
        [ActionPermissionFilter("marketing:ads:campaigns:edit")]
        [Log(Title = "Ad Campaign Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateAdCampaignStatus(long adCampaignId, [FromBody] string status)
        {
            var result = _adService.UpdateAdCampaignStatus(adCampaignId, status);
            return ToResponse(result);
        }

        #endregion

        #region Ad Creatives

        [HttpGet("creatives")]
        [ActionPermissionFilter("marketing:ads:creatives")]
        public IActionResult GetCreatives([FromQuery] AdCreativeQueryDto dto)
        {
            var result = _adService.GetCreatives(dto);
            return SUCCESS(result);
        }

        [HttpGet("creatives/{creativeId}")]
        [ActionPermissionFilter("marketing:ads:creatives")]
        public IActionResult GetCreativeDetail(long creativeId)
        {
            var result = _adService.GetCreativeDetail(creativeId);
            return SUCCESS(result);
        }

        [HttpPost("creatives")]
        [ActionPermissionFilter("marketing:ads:creatives:add")]
        [Log(Title = "Ad Creative", BusinessType = BusinessType.INSERT)]
        public IActionResult AddCreative([FromBody] AdCreativeDto dto)
        {
            var result = _adService.AddCreative(dto);
            return SUCCESS(result);
        }

        [HttpPut("creatives")]
        [ActionPermissionFilter("marketing:ads:creatives:edit")]
        [Log(Title = "Ad Creative", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCreative([FromBody] AdCreativeDto dto)
        {
            var result = _adService.UpdateCreative(dto);
            return ToResponse(result);
        }

        [HttpDelete("creatives/{ids}")]
        [ActionPermissionFilter("marketing:ads:creatives:delete")]
        [Log(Title = "Ad Creative", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCreatives(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _adService.DeleteCreatives(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Performance & Analytics

        [HttpGet("performance")]
        [ActionPermissionFilter("marketing:ads:performance")]
        public IActionResult GetPerformance([FromQuery] AdPerformanceQueryDto dto)
        {
            var result = _adService.GetPerformance(dto);
            return SUCCESS(result);
        }

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:ads:analytics")]
        public IActionResult GetAdAnalytics([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var result = _adService.GetAdAnalytics(dateFrom, dateTo);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get platform-level spending summary
        /// </summary>
        [HttpGet("platform-spending")]
        [ActionPermissionFilter("marketing:ads:analytics")]
        public IActionResult GetPlatformSpending([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var result = _adService.GetPlatformSpending(dateFrom, dateTo);
            return SUCCESS(result);
        }

        [HttpGet("export")]
        [ActionPermissionFilter("marketing:ads:export")]
        [Log(Title = "Ad Export", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportAdCampaigns([FromQuery] AdCampaignQueryDto dto)
        {
            var list = _adService.GetAdCampaigns(dto);
            var result = ExportExcelMini(list.Result, "AdCampaigns", "AdCampaigns");
            return ExportExcel(result.Item2, result.Item1);
        }

        #endregion
    }
}
