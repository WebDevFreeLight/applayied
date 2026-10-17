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
    /// Public Relations Controller - إدارة العلاقات العامة
    /// Press releases, media contacts, mentions, sentiment analysis
    /// </summary>
    [Verify]
    [Route("api/marketing/pr")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class PublicRelationsController : BaseController
    {
        private readonly IPublicRelationsService _prService;

        public PublicRelationsController(IPublicRelationsService prService)
        {
            _prService = prService;
        }

        #region Press Releases

        [HttpGet("releases")]
        [ActionPermissionFilter("marketing:pr:releases")]
        public IActionResult GetPressReleases([FromQuery] PressReleaseQueryDto dto)
        {
            var result = _prService.GetPressReleases(dto);
            return SUCCESS(result);
        }

        [HttpGet("releases/{releaseId}")]
        [ActionPermissionFilter("marketing:pr:releases")]
        public IActionResult GetPressReleaseDetail(long releaseId)
        {
            var result = _prService.GetPressReleaseDetail(releaseId);
            return SUCCESS(result);
        }

        [HttpPost("releases")]
        [ActionPermissionFilter("marketing:pr:releases:add")]
        [Log(Title = "Press Release", BusinessType = BusinessType.INSERT)]
        public IActionResult AddPressRelease([FromBody] PressReleaseDto dto)
        {
            var result = _prService.AddPressRelease(dto);
            return SUCCESS(result);
        }

        [HttpPut("releases")]
        [ActionPermissionFilter("marketing:pr:releases:edit")]
        [Log(Title = "Press Release", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePressRelease([FromBody] PressReleaseDto dto)
        {
            var result = _prService.UpdatePressRelease(dto);
            return ToResponse(result);
        }

        [HttpDelete("releases/{ids}")]
        [ActionPermissionFilter("marketing:pr:releases:delete")]
        [Log(Title = "Press Release", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePressReleases(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _prService.DeletePressReleases(idArray);
            return ToResponse(result);
        }

        [HttpPost("releases/{releaseId}/distribute")]
        [ActionPermissionFilter("marketing:pr:releases:distribute")]
        [Log(Title = "Press Release Distribution", BusinessType = BusinessType.OTHER)]
        public IActionResult DistributePressRelease(long releaseId)
        {
            var result = _prService.DistributePressRelease(releaseId);
            return ToResponse(result);
        }

        [HttpPost("releases/{releaseId}/approve")]
        [ActionPermissionFilter("marketing:pr:releases:approve")]
        [Log(Title = "Press Release Approval", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApprovePressRelease(long releaseId)
        {
            var result = _prService.ApprovePressRelease(releaseId);
            return ToResponse(result);
        }

        #endregion

        #region Media Contacts

        [HttpGet("contacts")]
        [ActionPermissionFilter("marketing:pr:contacts")]
        public IActionResult GetMediaContacts([FromQuery] MediaContactQueryDto dto)
        {
            var result = _prService.GetMediaContacts(dto);
            return SUCCESS(result);
        }

        [HttpGet("contacts/{contactId}")]
        [ActionPermissionFilter("marketing:pr:contacts")]
        public IActionResult GetMediaContactDetail(long contactId)
        {
            var result = _prService.GetMediaContactDetail(contactId);
            return SUCCESS(result);
        }

        [HttpPost("contacts")]
        [ActionPermissionFilter("marketing:pr:contacts:add")]
        [Log(Title = "Media Contact", BusinessType = BusinessType.INSERT)]
        public IActionResult AddMediaContact([FromBody] MediaContactDto dto)
        {
            var result = _prService.AddMediaContact(dto);
            return SUCCESS(result);
        }

        [HttpPut("contacts")]
        [ActionPermissionFilter("marketing:pr:contacts:edit")]
        [Log(Title = "Media Contact", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateMediaContact([FromBody] MediaContactDto dto)
        {
            var result = _prService.UpdateMediaContact(dto);
            return ToResponse(result);
        }

        [HttpDelete("contacts/{ids}")]
        [ActionPermissionFilter("marketing:pr:contacts:delete")]
        [Log(Title = "Media Contact", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteMediaContacts(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _prService.DeleteMediaContacts(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Media Mentions

        [HttpGet("mentions")]
        [ActionPermissionFilter("marketing:pr:mentions")]
        public IActionResult GetMentions([FromQuery] MediaMentionQueryDto dto)
        {
            var result = _prService.GetMentions(dto);
            return SUCCESS(result);
        }

        [HttpGet("mentions/{mentionId}")]
        [ActionPermissionFilter("marketing:pr:mentions")]
        public IActionResult GetMentionDetail(long mentionId)
        {
            var result = _prService.GetMentionDetail(mentionId);
            return SUCCESS(result);
        }

        [HttpPost("mentions")]
        [ActionPermissionFilter("marketing:pr:mentions:add")]
        [Log(Title = "Media Mention", BusinessType = BusinessType.INSERT)]
        public IActionResult AddMention([FromBody] MediaMentionDto dto)
        {
            var result = _prService.AddMention(dto);
            return SUCCESS(result);
        }

        [HttpPut("mentions")]
        [ActionPermissionFilter("marketing:pr:mentions:edit")]
        [Log(Title = "Media Mention", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateMention([FromBody] MediaMentionDto dto)
        {
            var result = _prService.UpdateMention(dto);
            return ToResponse(result);
        }

        [HttpDelete("mentions/{ids}")]
        [ActionPermissionFilter("marketing:pr:mentions:delete")]
        [Log(Title = "Media Mention", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteMentions(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _prService.DeleteMentions(idArray);
            return ToResponse(result);
        }

        #endregion

        #region Sentiment & Analytics

        [HttpGet("sentiment")]
        [ActionPermissionFilter("marketing:pr:sentiment")]
        public IActionResult GetSentiment([FromQuery] PrSentimentQueryDto dto)
        {
            var result = _prService.GetSentiment(dto);
            return SUCCESS(result);
        }

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:pr:analytics")]
        public IActionResult GetPrAnalytics([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var result = _prService.GetPrAnalytics(dateFrom, dateTo);
            return SUCCESS(result);
        }

        #endregion
    }
}
