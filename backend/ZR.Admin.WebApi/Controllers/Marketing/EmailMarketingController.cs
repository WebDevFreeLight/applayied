using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
    /// Email Marketing Controller - التسويق عبر البريد الإلكتروني
    /// Templates, campaigns, subscribers, lists, and analytics with SDAIA compliance
    /// </summary>
    [Verify]
    [Route("api/marketing/email")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class EmailMarketingController : BaseController
    {
        private readonly IEmailMarketingService _emailService;

        public EmailMarketingController(IEmailMarketingService emailService)
        {
            _emailService = emailService;
        }

        #region Templates

        [HttpGet("templates")]
        [ActionPermissionFilter("marketing:email:templates")]
        public IActionResult GetTemplates([FromQuery] EmailTemplateQueryDto dto)
        {
            var result = _emailService.GetTemplates(dto);
            return SUCCESS(result);
        }

        [HttpGet("templates/{templateId}")]
        [ActionPermissionFilter("marketing:email:templates")]
        public IActionResult GetTemplateDetail(long templateId)
        {
            var result = _emailService.GetTemplateDetail(templateId);
            return SUCCESS(result);
        }

        [HttpPost("templates")]
        [ActionPermissionFilter("marketing:email:templates:add")]
        [Log(Title = "Email Template", BusinessType = BusinessType.INSERT)]
        public IActionResult AddTemplate([FromBody] EmailTemplateDto dto)
        {
            var result = _emailService.AddTemplate(dto);
            return SUCCESS(result);
        }

        [HttpPut("templates")]
        [ActionPermissionFilter("marketing:email:templates:edit")]
        [Log(Title = "Email Template", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateTemplate([FromBody] EmailTemplateDto dto)
        {
            var result = _emailService.UpdateTemplate(dto);
            return ToResponse(result);
        }

        [HttpDelete("templates/{ids}")]
        [ActionPermissionFilter("marketing:email:templates:delete")]
        [Log(Title = "Email Template", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteTemplates(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _emailService.DeleteTemplates(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Campaigns

        [HttpGet("campaigns")]
        [ActionPermissionFilter("marketing:email:campaigns")]
        public IActionResult GetCampaigns([FromQuery] EmailCampaignQueryDto dto)
        {
            var result = _emailService.GetCampaigns(dto);
            return SUCCESS(result);
        }

        [HttpGet("campaigns/{campaignId}")]
        [ActionPermissionFilter("marketing:email:campaigns")]
        public IActionResult GetCampaignDetail(long campaignId)
        {
            var result = _emailService.GetCampaignDetail(campaignId);
            return SUCCESS(result);
        }

        [HttpPost("campaigns")]
        [ActionPermissionFilter("marketing:email:campaigns:add")]
        [Log(Title = "Email Campaign", BusinessType = BusinessType.INSERT)]
        public IActionResult CreateCampaign([FromBody] EmailCampaignDto dto)
        {
            var result = _emailService.CreateCampaign(dto);
            return SUCCESS(result);
        }

        [HttpPut("campaigns")]
        [ActionPermissionFilter("marketing:email:campaigns:edit")]
        [Log(Title = "Email Campaign", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCampaign([FromBody] EmailCampaignDto dto)
        {
            var result = _emailService.UpdateCampaign(dto);
            return ToResponse(result);
        }

        [HttpPost("campaigns/{campaignId}/send")]
        [ActionPermissionFilter("marketing:email:campaigns:send")]
        [Log(Title = "Email Campaign Send", BusinessType = BusinessType.OTHER)]
        public IActionResult SendCampaign(long campaignId)
        {
            var result = _emailService.SendCampaign(campaignId);
            return ToResponse(result);
        }

        [HttpPost("campaigns/{campaignId}/schedule")]
        [ActionPermissionFilter("marketing:email:campaigns:send")]
        [Log(Title = "Email Campaign Schedule", BusinessType = BusinessType.UPDATE)]
        public IActionResult ScheduleCampaign(long campaignId, [FromBody] DateTime scheduledAt)
        {
            var result = _emailService.ScheduleCampaign(campaignId, scheduledAt);
            return ToResponse(result);
        }

        [HttpPost("campaigns/{campaignId}/pause")]
        [ActionPermissionFilter("marketing:email:campaigns:edit")]
        public IActionResult PauseCampaign(long campaignId)
        {
            var result = _emailService.PauseCampaign(campaignId);
            return ToResponse(result);
        }

        [HttpDelete("campaigns/{ids}")]
        [ActionPermissionFilter("marketing:email:campaigns:delete")]
        [Log(Title = "Email Campaign", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCampaigns(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _emailService.DeleteCampaigns(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Subscribers

        [HttpGet("subscribers")]
        [ActionPermissionFilter("marketing:email:subscribers")]
        public IActionResult GetSubscribers([FromQuery] SubscriberQueryDto dto)
        {
            var result = _emailService.GetSubscribers(dto);
            return SUCCESS(result);
        }

        [HttpGet("subscribers/{subscriberId}")]
        [ActionPermissionFilter("marketing:email:subscribers")]
        public IActionResult GetSubscriberDetail(long subscriberId)
        {
            var result = _emailService.GetSubscriberDetail(subscriberId);
            return SUCCESS(result);
        }

        [HttpPost("subscribers")]
        [ActionPermissionFilter("marketing:email:subscribers:add")]
        [Log(Title = "Email Subscriber", BusinessType = BusinessType.INSERT)]
        public IActionResult AddSubscriber([FromBody] SubscriberDto dto)
        {
            var result = _emailService.AddSubscriber(dto);
            return SUCCESS(result);
        }

        [HttpPut("subscribers")]
        [ActionPermissionFilter("marketing:email:subscribers:edit")]
        [Log(Title = "Email Subscriber", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateSubscriber([FromBody] SubscriberDto dto)
        {
            var result = _emailService.UpdateSubscriber(dto);
            return ToResponse(result);
        }

        [HttpPost("subscribers/import")]
        [ActionPermissionFilter("marketing:email:subscribers:import")]
        [Log(Title = "Subscriber Import", BusinessType = BusinessType.IMPORT)]
        public IActionResult ImportSubscribers([FromBody] ImportSubscribersDto dto)
        {
            var result = _emailService.ImportSubscribers(dto);
            return SUCCESS(result);
        }

        [HttpPost("subscribers/{subscriberId}/unsubscribe")]
        [ActionPermissionFilter("marketing:email:subscribers:edit")]
        public IActionResult UnsubscribeSubscriber(long subscriberId, [FromBody] string reason)
        {
            var result = _emailService.Unsubscribe(subscriberId, reason);
            return ToResponse(result);
        }

        [HttpGet("subscribers/export")]
        [ActionPermissionFilter("marketing:email:subscribers:export")]
        [Log(Title = "Subscriber Export", BusinessType = BusinessType.EXPORT)]
        public IActionResult ExportSubscribers([FromQuery] SubscriberQueryDto dto)
        {
            var list = _emailService.GetSubscribers(dto);
            var result = ExportExcelMini(list.Result, "Subscribers", "Subscribers");
            return ExportExcel(result.Item2, result.Item1);
        }

        #endregion

        #region Subscriber Lists

        [HttpGet("lists")]
        [ActionPermissionFilter("marketing:email:lists")]
        public IActionResult GetLists([FromQuery] SubscriberListQueryDto dto)
        {
            var result = _emailService.GetLists(dto);
            return SUCCESS(result);
        }

        [HttpPost("lists")]
        [ActionPermissionFilter("marketing:email:lists:add")]
        [Log(Title = "Subscriber List", BusinessType = BusinessType.INSERT)]
        public IActionResult AddList([FromBody] SubscriberListDto dto)
        {
            var result = _emailService.AddList(dto);
            return SUCCESS(result);
        }

        [HttpPut("lists")]
        [ActionPermissionFilter("marketing:email:lists:edit")]
        [Log(Title = "Subscriber List", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateList([FromBody] SubscriberListDto dto)
        {
            var result = _emailService.UpdateList(dto);
            return ToResponse(result);
        }

        [HttpDelete("lists/{ids}")]
        [ActionPermissionFilter("marketing:email:lists:delete")]
        [Log(Title = "Subscriber List", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteLists(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _emailService.DeleteLists(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Analytics

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:email:analytics")]
        public IActionResult GetEmailAnalytics([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var result = _emailService.GetEmailAnalytics(dateFrom, dateTo);
            return SUCCESS(result);
        }

        #endregion
    }
}
