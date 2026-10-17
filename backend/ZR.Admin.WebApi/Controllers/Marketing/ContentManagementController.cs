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
    /// Content Management Controller - إدارة المحتوى التسويقي
    /// Content library, AI content generation, and performance analytics
    /// </summary>
    [Verify]
    [Route("api/marketing/content")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class ContentManagementController : BaseController
    {
        private readonly IContentManagementService _contentService;

        public ContentManagementController(IContentManagementService contentService)
        {
            _contentService = contentService;
        }

        #region Content Library

        [HttpGet("list")]
        [ActionPermissionFilter("marketing:content:list")]
        public IActionResult GetContent([FromQuery] ContentQueryDto dto)
        {
            var result = _contentService.GetContent(dto);
            return SUCCESS(result);
        }

        [HttpGet("{contentId}")]
        [ActionPermissionFilter("marketing:content:query")]
        public IActionResult GetContentDetail(long contentId)
        {
            var result = _contentService.GetContentDetail(contentId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Content not found");
            return SUCCESS(result);
        }

        [HttpPost]
        [ActionPermissionFilter("marketing:content:add")]
        [Log(Title = "Marketing Content", BusinessType = BusinessType.INSERT)]
        public IActionResult AddContent([FromBody] ContentDto dto)
        {
            var result = _contentService.AddContent(dto);
            return SUCCESS(result);
        }

        [HttpPut]
        [ActionPermissionFilter("marketing:content:edit")]
        [Log(Title = "Marketing Content", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateContent([FromBody] ContentDto dto)
        {
            var result = _contentService.UpdateContent(dto);
            return ToResponse(result);
        }

        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:content:delete")]
        [Log(Title = "Marketing Content", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteContent(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _contentService.DeleteContent(idArray);
            return ToResponse(result);
        }

        [HttpPut("{contentId}/status")]
        [ActionPermissionFilter("marketing:content:edit")]
        [Log(Title = "Content Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateContentStatus(long contentId, [FromBody] string status)
        {
            var result = _contentService.UpdateContentStatus(contentId, status);
            return ToResponse(result);
        }

        [HttpPut("{contentId}/publish")]
        [ActionPermissionFilter("marketing:content:publish")]
        [Log(Title = "Content Publish", BusinessType = BusinessType.UPDATE)]
        public IActionResult PublishContent(long contentId)
        {
            var result = _contentService.PublishContent(contentId);
            return ToResponse(result);
        }

        #endregion

        #region AI Content Generation

        [HttpGet("ai-generations")]
        [ActionPermissionFilter("marketing:content:ai")]
        public IActionResult GetAiGenerations([FromQuery] AiContentGenerationQueryDto dto)
        {
            var result = _contentService.GetAiGenerations(dto);
            return SUCCESS(result);
        }

        [HttpPost("ai-generate")]
        [ActionPermissionFilter("marketing:content:ai:generate")]
        [Log(Title = "AI Content Generation", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateAiContent([FromBody] GenerateContentRequest dto)
        {
            var result = _contentService.GenerateAiContent(dto);
            return SUCCESS(result);
        }

        [HttpPost("ai-generations/{generationId}/approve")]
        [ActionPermissionFilter("marketing:content:ai:approve")]
        [Log(Title = "AI Content Approval", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApproveAiContent(long generationId, [FromBody] ApproveContentRequest dto)
        {
            dto.GenerationId = generationId;
            var result = _contentService.ApproveAiContent(dto);
            return ToResponse(result);
        }

        #endregion

        #region Analytics

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:content:analytics")]
        public IActionResult GetContentAnalytics([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
        {
            var result = _contentService.GetContentAnalytics(dateFrom, dateTo);
            return SUCCESS(result);
        }

        #endregion
    }
}
