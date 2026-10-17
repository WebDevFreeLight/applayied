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
    /// Customer Journey Controller - رحلة العميل التسويقية
    /// Hyper-personalized automated marketing journeys with dynamic content
    /// </summary>
    [Verify]
    [Route("api/marketing/journeys")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class JourneyController : BaseController
    {
        private readonly IJourneyService _journeyService;

        public JourneyController(IJourneyService journeyService)
        {
            _journeyService = journeyService;
        }

        #region Journey Management

        [HttpGet("list")]
        [ActionPermissionFilter("marketing:journey:list")]
        public IActionResult GetJourneys([FromQuery] JourneyQueryDto dto)
        {
            var result = _journeyService.GetJourneys(dto);
            return SUCCESS(result);
        }

        [HttpGet("{journeyId}")]
        [ActionPermissionFilter("marketing:journey:query")]
        public IActionResult GetJourneyDetail(long journeyId)
        {
            var result = _journeyService.GetJourneyDetail(journeyId);
            if (result == null) return ToResponse(ResultCode.FAIL, "Journey not found");
            return SUCCESS(result);
        }

        [HttpPost]
        [ActionPermissionFilter("marketing:journey:add")]
        [Log(Title = "Customer Journey", BusinessType = BusinessType.INSERT)]
        public IActionResult AddJourney([FromBody] JourneyDto dto)
        {
            var result = _journeyService.AddJourney(dto);
            return SUCCESS(result);
        }

        [HttpPut]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Customer Journey", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateJourney([FromBody] JourneyDto dto)
        {
            var result = _journeyService.UpdateJourney(dto);
            return ToResponse(result);
        }

        [HttpDelete("{ids}")]
        [ActionPermissionFilter("marketing:journey:delete")]
        [Log(Title = "Customer Journey", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteJourneys(string ids)
        {
            var idArray = ids.Split(',').Select(long.Parse).ToArray();
            var result = _journeyService.DeleteJourneys(idArray);
            return ToResponse(result);
        }

        [HttpPut("{journeyId}/status")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateJourneyStatus(long journeyId, [FromBody] string status)
        {
            var result = _journeyService.UpdateJourneyStatus(journeyId, status);
            return ToResponse(result);
        }

        #endregion

        #region Stages

        [HttpGet("{journeyId}/stages")]
        [ActionPermissionFilter("marketing:journey:query")]
        public IActionResult GetStages(long journeyId)
        {
            var result = _journeyService.GetStages(journeyId);
            return SUCCESS(result);
        }

        [HttpPost("{journeyId}/stages")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Stage", BusinessType = BusinessType.INSERT)]
        public IActionResult AddStage(long journeyId, [FromBody] JourneyStageDto dto)
        {
            dto.JourneyId = journeyId;
            var result = _journeyService.AddStage(dto);
            return SUCCESS(result);
        }

        [HttpPut("stages")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Stage", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateStage([FromBody] JourneyStageDto dto)
        {
            var result = _journeyService.UpdateStage(dto);
            return ToResponse(result);
        }

        [HttpDelete("stages/{stageId}")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Stage", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteStage(long stageId)
        {
            var result = _journeyService.DeleteStage(stageId);
            return ToResponse(result);
        }

        #endregion

        #region Triggers

        [HttpGet("{journeyId}/triggers")]
        [ActionPermissionFilter("marketing:journey:query")]
        public IActionResult GetTriggers(long journeyId)
        {
            var result = _journeyService.GetTriggers(journeyId);
            return SUCCESS(result);
        }

        [HttpPost("{journeyId}/triggers")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Trigger", BusinessType = BusinessType.INSERT)]
        public IActionResult AddTrigger(long journeyId, [FromBody] JourneyTriggerDto dto)
        {
            dto.JourneyId = journeyId;
            var result = _journeyService.AddTrigger(dto);
            return SUCCESS(result);
        }

        [HttpPut("triggers")]
        [ActionPermissionFilter("marketing:journey:edit")]
        [Log(Title = "Journey Trigger", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateTrigger([FromBody] JourneyTriggerDto dto)
        {
            var result = _journeyService.UpdateTrigger(dto);
            return ToResponse(result);
        }

        #endregion

        #region Executions

        [HttpGet("executions")]
        [ActionPermissionFilter("marketing:journey:executions")]
        public IActionResult GetExecutions([FromQuery] JourneyExecutionQueryDto dto)
        {
            var result = _journeyService.GetExecutions(dto);
            return SUCCESS(result);
        }

        [HttpPost("enroll")]
        [ActionPermissionFilter("marketing:journey:enroll")]
        [Log(Title = "Journey Enrollment", BusinessType = BusinessType.INSERT)]
        public IActionResult EnrollMember([FromBody] EnrollJourneyRequest dto)
        {
            var result = _journeyService.EnrollMember(dto);
            return SUCCESS(result);
        }

        [HttpPost("executions/{executionId}/advance")]
        [ActionPermissionFilter("marketing:journey:executions")]
        public IActionResult AdvanceExecution(long executionId)
        {
            var result = _journeyService.AdvanceExecution(executionId);
            return ToResponse(result);
        }

        [HttpPost("executions/{executionId}/pause")]
        [ActionPermissionFilter("marketing:journey:executions")]
        public IActionResult PauseExecution(long executionId)
        {
            var result = _journeyService.PauseExecution(executionId);
            return ToResponse(result);
        }

        #endregion

        #region Dynamic Content

        [HttpGet("dynamic-content")]
        [ActionPermissionFilter("marketing:journey:content")]
        public IActionResult GetDynamicContent([FromQuery] DynamicContentQueryDto dto)
        {
            var result = _journeyService.GetDynamicContent(dto);
            return SUCCESS(result);
        }

        [HttpPost("dynamic-content")]
        [ActionPermissionFilter("marketing:journey:content:add")]
        [Log(Title = "Dynamic Content", BusinessType = BusinessType.INSERT)]
        public IActionResult AddDynamicContent([FromBody] DynamicContentDto dto)
        {
            var result = _journeyService.ManageDynamicContent(dto);
            return SUCCESS(result);
        }

        [HttpPut("dynamic-content")]
        [ActionPermissionFilter("marketing:journey:content:edit")]
        [Log(Title = "Dynamic Content", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateDynamicContent([FromBody] DynamicContentDto dto)
        {
            var result = _journeyService.ManageDynamicContent(dto);
            return SUCCESS(result);
        }

        #endregion

        #region Analytics

        [HttpGet("analytics")]
        [ActionPermissionFilter("marketing:journey:analytics")]
        public IActionResult GetJourneyAnalytics()
        {
            var result = _journeyService.GetJourneyAnalytics();
            return SUCCESS(result);
        }

        #endregion
    }
}
