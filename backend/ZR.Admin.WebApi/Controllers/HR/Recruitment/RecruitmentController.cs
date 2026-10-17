using System.Text.Json.Serialization;
using Infrastructure.Converter;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Http;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using SqlSugar;

namespace ZR.Admin.WebApi.Controllers.HR.Recruitment
{
    /// <summary>
    /// Recruitment Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/recruitment")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class RecruitmentController : BaseController
    {
        private readonly IRecruitmentService _recruitmentService;

        public RecruitmentController(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
        }

        #region Job Requisitions

        /// <summary>
        /// Get job requisitions
        /// </summary>
        [HttpGet("requisitions")]
        [ActionPermissionFilter("hr:recruitment:list")]
        public async Task<IActionResult> GetRequisitions([FromQuery] RequisitionQueryDto query)
        {
            var result = await _recruitmentService.GetRequisitionsAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get requisition by ID
        /// </summary>
        [HttpGet("requisitions/{id}")]
        [ActionPermissionFilter("hr:recruitment:query")]
        public async Task<IActionResult> GetRequisition(long id)
        {
            var result = await _recruitmentService.GetRequisitionByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Requisition not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create job requisition
        /// </summary>
        [HttpPost("requisitions")]
        [ActionPermissionFilter("hr:recruitment:add")]
        [Log(Title = "Job Requisition", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateRequisition([FromBody] JobRequisitionDto dto)
        {
            dto.Create_by = HttpContext.GetName();
            var result = await _recruitmentService.CreateRequisitionAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update job requisition
        /// </summary>
        [HttpPut("requisitions")]
        [ActionPermissionFilter("hr:recruitment:edit")]
        [Log(Title = "Job Requisition", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateRequisition([FromBody] JobRequisitionDto dto)
        {
            var result = await _recruitmentService.UpdateRequisitionAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve requisition
        /// </summary>
        [HttpPost("requisitions/{requisitionId}/approve")]
        [ActionPermissionFilter("hr:recruitment:approve")]
        [Log(Title = "Approve Requisition", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveRequisition(long requisitionId)
        {
            var approvedBy = HttpContext.GetName();
            var result = await _recruitmentService.ApproveRequisitionAsync(requisitionId, approvedBy);
            return SUCCESS(result);
        }

        /// <summary>
        /// Publish job
        /// </summary>
        [HttpPost("requisitions/{requisitionId}/publish")]
        [ActionPermissionFilter("hr:recruitment:publish")]
        [Log(Title = "Publish Job", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> PublishJob(long requisitionId, [FromBody] PublishJobDto dto)
        {
            var result = await _recruitmentService.PublishJobAsync(requisitionId, dto.JobBoards);
            return SUCCESS(result);
        }

        /// <summary>
        /// Close requisition
        /// </summary>
        [HttpPost("requisitions/{requisitionId}/close")]
        [ActionPermissionFilter("hr:recruitment:close")]
        [Log(Title = "Close Requisition", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CloseRequisition(long requisitionId, [FromBody] CloseRequisitionDto dto)
        {
            var result = await _recruitmentService.CloseRequisitionAsync(requisitionId, dto.Reason);
            return SUCCESS(result);
        }

        #endregion

        #region Candidate Management

        /// <summary>
        /// Get candidates
        /// </summary>
        [HttpGet("candidates")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        public async Task<IActionResult> GetCandidates([FromQuery] CandidateQueryDto query)
        {
            var result = await _recruitmentService.GetCandidatesAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get candidate by ID
        /// </summary>
        [HttpGet("candidates/{id}")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        public async Task<IActionResult> GetCandidate(long id)
        {
            var result = await _recruitmentService.GetCandidateByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Candidate not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create candidate
        /// </summary>
        [HttpPost("candidates")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        [Log(Title = "Add Candidate", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateCandidate([FromBody] CandidateDto dto)
        {
            var result = await _recruitmentService.CreateCandidateAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update candidate
        /// </summary>
        [HttpPut("candidates")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        [Log(Title = "Update Candidate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateCandidate([FromBody] CandidateDto dto)
        {
            var result = await _recruitmentService.UpdateCandidateAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Upload resume
        /// </summary>
        [HttpPost("candidates/{candidateId}/resume")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        [Log(Title = "Upload Resume", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UploadResume(long candidateId, IFormFile file)
        {
            var result = await _recruitmentService.UploadResumeAsync(candidateId, file);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update candidate status
        /// </summary>
        [HttpPut("candidates/{candidateId}/status")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        [Log(Title = "Update Candidate Status", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateCandidateStatus(long candidateId, [FromBody] UpdateStatusDto dto)
        {
            var result = await _recruitmentService.UpdateCandidateStatusAsync(candidateId, dto.Status, dto.Notes);
            return SUCCESS(result);
        }

        /// <summary>
        /// Search candidates
        /// </summary>
        [HttpGet("candidates/search")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        public async Task<IActionResult> SearchCandidates([FromQuery] string skills, [FromQuery] string experience)
        {
            var result = await _recruitmentService.SearchCandidatesAsync(skills, experience);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get candidate history
        /// </summary>
        [HttpGet("candidates/{candidateId}/history")]
        [ActionPermissionFilter("hr:recruitment:candidates")]
        public async Task<IActionResult> GetCandidateHistory(long candidateId)
        {
            var result = await _recruitmentService.GetCandidateHistoryAsync(candidateId);
            return SUCCESS(result);
        }

        #endregion

        #region Interview Management

        /// <summary>
        /// Get interviews
        /// </summary>
        [HttpGet("interviews")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        public async Task<IActionResult> GetInterviews([FromQuery] InterviewQueryDto query)
        {
            var result = await _recruitmentService.GetInterviewsAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Schedule interview
        /// </summary>
        [HttpPost("interviews")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        [Log(Title = "Schedule Interview", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ScheduleInterview([FromBody] InterviewDto dto)
        {
            var result = await _recruitmentService.ScheduleInterviewAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update interview
        /// </summary>
        [HttpPut("interviews")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        [Log(Title = "Update Interview", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateInterview([FromBody] InterviewDto dto)
        {
            var result = await _recruitmentService.UpdateInterviewAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel interview
        /// </summary>
        [HttpPost("interviews/{interviewId}/cancel")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        [Log(Title = "Cancel Interview", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelInterview(long interviewId, [FromBody] CancelDto dto)
        {
            var result = await _recruitmentService.CancelInterviewAsync(interviewId, dto.Reason);
            return SUCCESS(result);
        }

        /// <summary>
        /// Submit interview feedback
        /// </summary>
        [HttpPost("interviews/{interviewId}/feedback")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        [Log(Title = "Interview Feedback", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SubmitInterviewFeedback(long interviewId, [FromBody] InterviewFeedbackDto feedback)
        {
            var result = await _recruitmentService.SubmitInterviewFeedbackAsync(interviewId, feedback);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get upcoming interviews
        /// </summary>
        [HttpGet("interviews/upcoming")]
        [ActionPermissionFilter("hr:recruitment:interviews")]
        public async Task<IActionResult> GetUpcomingInterviews([FromQuery] long? interviewerId, [FromQuery] int days = 7)
        {
            var result = await _recruitmentService.GetUpcomingInterviewsAsync(interviewerId, days);
            return SUCCESS(result);
        }

        #endregion

        #region Offer Management

        /// <summary>
        /// Get job offers
        /// </summary>
        [HttpGet("offers")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        public async Task<IActionResult> GetOffers([FromQuery] long? candidateId, [FromQuery] string status)
        {
            var result = await _recruitmentService.GetOffersAsync(candidateId, status);
            return SUCCESS(result);
        }

        /// <summary>
        /// Create job offer
        /// </summary>
        [HttpPost("offers")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        [Log(Title = "Create Offer", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateOffer([FromBody] JobOfferDto dto)
        {
            var result = await _recruitmentService.CreateOfferAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update job offer
        /// </summary>
        [HttpPut("offers")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        [Log(Title = "Update Offer", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateOffer([FromBody] JobOfferDto dto)
        {
            var result = await _recruitmentService.UpdateOfferAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Send offer
        /// </summary>
        [HttpPost("offers/{offerId}/send")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        [Log(Title = "Send Offer", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SendOffer(long offerId)
        {
            var result = await _recruitmentService.SendOfferAsync(offerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Accept offer
        /// </summary>
        [HttpPost("offers/{offerId}/accept")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        [Log(Title = "Accept Offer", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> AcceptOffer(long offerId, [FromBody] AcceptOfferDto dto)
        {
            var result = await _recruitmentService.AcceptOfferAsync(offerId, dto.JoiningDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Reject offer
        /// </summary>
        [HttpPost("offers/{offerId}/reject")]
        [ActionPermissionFilter("hr:recruitment:offers")]
        [Log(Title = "Reject Offer", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectOffer(long offerId, [FromBody] RejectOfferDto dto)
        {
            var result = await _recruitmentService.RejectOfferAsync(offerId, dto.Reason);
            return SUCCESS(result);
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get recruitment metrics
        /// </summary>
        [HttpGet("metrics")]
        [ActionPermissionFilter("hr:recruitment:analytics")]
        public async Task<IActionResult> GetRecruitmentMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _recruitmentService.GetRecruitmentMetricsAsync(startDate, endDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get source analytics
        /// </summary>
        [HttpGet("analytics/source")]
        [ActionPermissionFilter("hr:recruitment:analytics")]
        public async Task<IActionResult> GetSourceAnalytics([FromQuery] int year, [FromQuery] int? quarter)
        {
            var result = await _recruitmentService.GetSourceAnalyticsAsync(year, quarter);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get pipeline status
        /// </summary>
        [HttpGet("pipeline/{requisitionId}")]
        [ActionPermissionFilter("hr:recruitment:view")]
        public async Task<IActionResult> GetPipelineStatus(long requisitionId)
        {
            var result = await _recruitmentService.GetPipelineStatusAsync(requisitionId);
            return SUCCESS(result);
        }

        #endregion

        #region Onboarding

        /// <summary>
        /// Get onboarding checklist
        /// </summary>
        [HttpGet("onboarding/{candidateId}/checklist")]
        [ActionPermissionFilter("hr:recruitment:onboarding")]
        public async Task<IActionResult> GetOnboardingChecklist(long candidateId)
        {
            var result = await _recruitmentService.GetOnboardingChecklistAsync(candidateId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update onboarding task
        /// </summary>
        [HttpPut("onboarding/tasks/{taskId}")]
        [ActionPermissionFilter("hr:recruitment:onboarding")]
        [Log(Title = "Update Onboarding Task", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateOnboardingTask(long taskId, [FromBody] OnboardingTaskUpdateDto dto)
        {
            var result = await _recruitmentService.UpdateOnboardingTaskAsync(taskId, dto.Completed, dto.Notes);
            return SUCCESS(result);
        }

        /// <summary>
        /// Complete onboarding
        /// </summary>
        [HttpPost("onboarding/{candidateId}/complete")]
        [ActionPermissionFilter("hr:recruitment:onboarding")]
        [Log(Title = "Complete Onboarding", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CompleteOnboarding(long candidateId, [FromBody] CompleteOnboardingDto dto)
        {
            var result = await _recruitmentService.CompleteOnboardingAsync(candidateId, dto.EmployeeId);
            return SUCCESS(result);
        }

        #endregion
    }

    // DTOs for request bodies
    public class PublishJobDto
    {
        public List<string> JobBoards { get; set; }
    }

    public class CloseRequisitionDto
    {
        public string Reason { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class CancelDto
    {
        public string Reason { get; set; }
    }

    public class AcceptOfferDto
    {
        public DateTime JoiningDate { get; set; }
    }

    public class RejectOfferDto
    {
        public string Reason { get; set; }
    }

    public class OnboardingTaskUpdateDto
    {
        public bool Completed { get; set; }
        public string Notes { get; set; }
    }

    public class CompleteOnboardingDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
    }
}

