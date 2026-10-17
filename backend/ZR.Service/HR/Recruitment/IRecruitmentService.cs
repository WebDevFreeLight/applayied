using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.System;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Recruitment Service Interface
    /// </summary>
    public interface IRecruitmentService
    {
        // Job Requisitions
        Task<PagedInfo<JobRequisitionDto>> GetRequisitionsAsync(RequisitionQueryDto query);
        Task<JobRequisitionDto> GetRequisitionByIdAsync(long requisitionId);
        Task<JobRequisitionDto> CreateRequisitionAsync(JobRequisitionDto requisition);
        Task<bool> UpdateRequisitionAsync(JobRequisitionDto requisition);
        Task<bool> ApproveRequisitionAsync(long requisitionId, string approvedBy);
        Task<bool> PublishJobAsync(long requisitionId, List<string> jobBoards);
        Task<bool> CloseRequisitionAsync(long requisitionId, string reason);
        
        // Candidate Management
        Task<PagedInfo<CandidateDto>> GetCandidatesAsync(CandidateQueryDto query);
        Task<CandidateDto> GetCandidateByIdAsync(long candidateId);
        Task<CandidateDto> CreateCandidateAsync(CandidateDto candidate);
        Task<bool> UpdateCandidateAsync(CandidateDto candidate);
        Task<bool> UploadResumeAsync(long candidateId, IFormFile file);
        Task<bool> UpdateCandidateStatusAsync(long candidateId, string status, string notes);
        Task<List<CandidateDto>> SearchCandidatesAsync(string skills, string experience);
        
        // Interview Management
        Task<List<InterviewDto>> GetInterviewsAsync(InterviewQueryDto query);
        Task<InterviewDto> ScheduleInterviewAsync(InterviewDto interview);
        Task<bool> UpdateInterviewAsync(InterviewDto interview);
        Task<bool> CancelInterviewAsync(long interviewId, string reason);
        Task<bool> SubmitInterviewFeedbackAsync(long interviewId, InterviewFeedbackDto feedback);
        Task<List<InterviewDto>> GetUpcomingInterviewsAsync(long? interviewerId = null, int days = 7);
        
        // Offer Management
        Task<List<JobOfferDto>> GetOffersAsync(long? candidateId = null, string status = null);
        Task<JobOfferDto> CreateOfferAsync(JobOfferDto offer);
        Task<bool> UpdateOfferAsync(JobOfferDto offer);
        Task<bool> SendOfferAsync(long offerId);
        Task<bool> AcceptOfferAsync(long offerId, DateTime joiningDate);
        Task<bool> RejectOfferAsync(long offerId, string reason);
        Task<bool> NegotiateOfferAsync(long offerId, decimal proposedSalary, string comments);
        
        // Pipeline Management
        Task<bool> MoveCandidateToStageAsync(long candidateId, long requisitionId, string stage);
        Task<RecruitmentPipelineDto> GetPipelineStatusAsync(long requisitionId);
        Task<List<CandidateStageHistoryDto>> GetCandidateHistoryAsync(long candidateId);
        
        // Analytics and Reporting
        Task<RecruitmentMetricsDto> GetRecruitmentMetricsAsync(DateTime startDate, DateTime endDate);
        Task<SourceAnalyticsDto> GetSourceAnalyticsAsync(int year, int? quarter = null);
        Task<TimeToHireDto> GetTimeToHireReportAsync(long? departmentId = null, int months = 6);
        Task<InterviewerPerformanceDto> GetInterviewerPerformanceAsync(long interviewerId, int months = 12);
        
        // Onboarding
        Task<OnboardingChecklistDto> GetOnboardingChecklistAsync(long candidateId);
        Task<bool> UpdateOnboardingTaskAsync(long taskId, bool completed, string notes);
        Task<bool> CompleteOnboardingAsync(long candidateId, long employeeId);
        
        // Talent Pool
        Task<bool> AddToTalentPoolAsync(long candidateId, string category, string notes);
        Task<List<CandidateDto>> GetTalentPoolAsync(string category = null, string skills = null);
        Task<bool> ReachOutToCandidateAsync(long candidateId, string subject, string message);
    }
}
