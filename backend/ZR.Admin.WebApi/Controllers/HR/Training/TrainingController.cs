using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using ZR.Model.HR.Training;

namespace ZR.Admin.WebApi.Controllers.HR.Training
{
    /// <summary>
    /// Training Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/training")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class TrainingController : BaseController
    {
        private readonly ITrainingService _trainingService;

        public TrainingController(ITrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        #region Training Course Management

        /// <summary>
        /// Get training courses
        /// </summary>
        [HttpGet("courses")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetCourses([FromQuery] TrainingCourseQueryDto dto)
        {
            var predicate = Expressionable.Create<HrTrainingCourse>();
            
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.CourseName), 
                it => it.CourseName.Contains(dto.CourseName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Category), 
                it => it.Category == dto.Category);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Level), 
                it => it.Level == dto.Level);
            predicate = predicate.AndIF(dto.IsActive.HasValue, 
                it => it.IsActive == dto.IsActive);
            predicate = predicate.AndIF(dto.IsMandatory.HasValue, 
                it => it.IsMandatory == dto.IsMandatory);
            
            var response = await _trainingService.GetCoursesAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active courses
        /// </summary>
        [HttpGet("courses/active")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetActiveCourses()
        {
            var courses = await _trainingService.GetActiveCoursesAsync();
            return SUCCESS(courses);
        }

        /// <summary>
        /// Get course by ID
        /// </summary>
        [HttpGet("courses/{courseId}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetCourseById(long courseId)
        {
            var course = await _trainingService.GetCourseByIdAsync(courseId);
            return SUCCESS(course);
        }

        /// <summary>
        /// Create training course
        /// </summary>
        [HttpPost("courses")]
        [ActionPermissionFilter("hr:training:add")]
        [Log(Title = "Training Course Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateCourse([FromBody] TrainingDto dto)
        {
            var course = dto.Adapt<HrTrainingCourse>();
            course.Create_by = HttpContext.GetName();
            course.Create_time = DateTime.Now;
            
            var result = await _trainingService.CreateCourseAsync(course);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update training course
        /// </summary>
        [HttpPut("courses/{courseId}")]
        [ActionPermissionFilter("hr:training:edit")]
        [Log(Title = "Training Course Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateCourse(long courseId, [FromBody] TrainingDto dto)
        {
            var course = dto.Adapt<HrTrainingCourse>();
            course.CourseId = courseId;
            course.Update_by = HttpContext.GetName();
            course.Update_time = DateTime.Now;
            
            var result = await _trainingService.UpdateCourseAsync(course);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete training course
        /// </summary>
        [HttpDelete("courses/{courseId}")]
        [ActionPermissionFilter("hr:training:delete")]
        [Log(Title = "Training Course Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteCourse(long courseId)
        {
            var result = await _trainingService.DeleteCourseAsync(courseId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get courses by category
        /// </summary>
        [HttpGet("courses/category/{category}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetCoursesByCategory(string category)
        {
            var courses = await _trainingService.GetCoursesByCategoryAsync(category);
            return SUCCESS(courses);
        }

        /// <summary>
        /// Get mandatory courses
        /// </summary>
        [HttpGet("courses/mandatory")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetMandatoryCourses()
        {
            var courses = await _trainingService.GetMandatoryCoursesAsync();
            return SUCCESS(courses);
        }

        #endregion

        #region Training Session Management

        /// <summary>
        /// Get training sessions
        /// </summary>
        [HttpGet("sessions")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetSessions([FromQuery] TrainingSessionQueryDto dto)
        {
            var predicate = Expressionable.Create<HrTrainingSession>();
            
            predicate = predicate.AndIF(dto.CourseId > 0, 
                it => it.CourseId == dto.CourseId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status), 
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.StartDate != null, 
                it => it.StartDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null, 
                it => it.StartDate <= dto.EndDate);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Instructor), 
                it => it.Instructor.Contains(dto.Instructor));
            
            var response = await _trainingService.GetSessionsAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get upcoming sessions
        /// </summary>
        [HttpGet("sessions/upcoming")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetUpcomingSessions()
        {
            var sessions = await _trainingService.GetUpcomingSessionsAsync();
            return SUCCESS(sessions);
        }

        /// <summary>
        /// Get session by ID
        /// </summary>
        [HttpGet("sessions/{sessionId}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetSessionById(long sessionId)
        {
            var session = await _trainingService.GetSessionByIdAsync(sessionId);
            return SUCCESS(session);
        }



        /// <summary>
        /// Schedule training session
        /// </summary>
        [HttpPost("sessions")]
        [ActionPermissionFilter("hr:training:schedule")]
        [Log(Title = "Training Session Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ScheduleSession([FromBody] TrainingSessionDto dto)
        {
            var session = dto.Adapt<HrTrainingSession>();
            session.Create_by = HttpContext.GetName();
            session.Create_time = DateTime.Now;
            session.Status = "SCHEDULED";
            
            var result = await _trainingService.ScheduleSessionAsync(session);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update training session
        /// </summary>
        [HttpPut("sessions/{sessionId}")]
        [ActionPermissionFilter("hr:training:edit")]
        [Log(Title = "Training Session Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateSession(long sessionId, [FromBody] TrainingSessionDto dto)
        {
            var session = dto.Adapt<HrTrainingSession>();
            session.SessionId = sessionId;
            session.Update_by = HttpContext.GetName();
            session.Update_time = DateTime.Now;
            
            var result = await _trainingService.UpdateSessionAsync(session);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel training session
        /// </summary>
        [HttpPost("sessions/{sessionId}/cancel")]
        [ActionPermissionFilter("hr:training:cancel")]
        [Log(Title = "Training Session Cancellation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelSession(long sessionId, [FromBody] CancelSessionDto dto)
        {
            var result = await _trainingService.CancelSessionAsync(sessionId, dto.Reason);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get sessions by course
        /// </summary>
        [HttpGet("sessions/course/{courseId}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetSessionsByCourse(long courseId)
        {
            var sessions = await _trainingService.GetSessionsByCourseAsync(courseId);
            return SUCCESS(sessions);
        }

        #endregion

        #region Training Enrollment

        /// <summary>
        /// Get session enrollments
        /// </summary>
        [HttpGet("sessions/{sessionId}/enrollments")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetEnrollments(long sessionId)
        {
            var enrollments = await _trainingService.GetEnrollmentsAsync(sessionId);
            return SUCCESS(enrollments);
        }

        /// <summary>
        /// Get employee enrollments
        /// </summary>
        [HttpGet("enrollments/employee/{employeeId}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetEmployeeEnrollments(long employeeId)
        {
            var enrollments = await _trainingService.GetEmployeeEnrollmentsAsync(employeeId);
            return SUCCESS(enrollments);
        }

        /// <summary>
        /// Enroll employee in training
        /// </summary>
        [HttpPost("enrollments")]
        [ActionPermissionFilter("hr:training:enroll")]
        [Log(Title = "Training Enrollment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> EnrollEmployee([FromBody] TrainingEnrollmentDto dto)
        {
            var enrollment = dto.Adapt<HrTrainingEnrollment>();
            enrollment.Create_by = HttpContext.GetName();
            enrollment.Create_time = DateTime.Now;
            enrollment.EnrollmentDate = DateTime.Now;
            enrollment.Status = "ENROLLED";
            
            var result = await _trainingService.EnrollEmployeeAsync(enrollment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update enrollment
        /// </summary>
        [HttpPut("enrollments/{enrollmentId}")]
        [ActionPermissionFilter("hr:training:edit")]
        [Log(Title = "Training Enrollment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateEnrollment(long enrollmentId, [FromBody] TrainingEnrollmentDto dto)
        {
            var enrollment = dto.Adapt<HrTrainingEnrollment>();
            enrollment.EnrollmentId = enrollmentId;
            enrollment.Update_by = HttpContext.GetName();
            enrollment.Update_time = DateTime.Now;
            
            var result = await _trainingService.UpdateEnrollmentAsync(enrollment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel enrollment
        /// </summary>
        [HttpPost("enrollments/{enrollmentId}/cancel")]
        [ActionPermissionFilter("hr:training:cancel")]
        [Log(Title = "Training Enrollment Cancellation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelEnrollment(long enrollmentId, [FromBody] CancelEnrollmentDto dto)
        {
            var result = await _trainingService.CancelEnrollmentAsync(enrollmentId, dto.Reason);
            return SUCCESS(result);
        }

        /// <summary>
        /// Complete enrollment
        /// </summary>
        [HttpPost("enrollments/{enrollmentId}/complete")]
        [ActionPermissionFilter("hr:training:complete")]
        [Log(Title = "Training Enrollment Completion", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CompleteEnrollment(long enrollmentId, [FromBody] CompleteEnrollmentDto dto)
        {
            var result = await _trainingService.CompleteEnrollmentAsync(enrollmentId, dto.Score);
            return SUCCESS(result);
        }

        /// <summary>
        /// Bulk enroll employees
        /// </summary>
        [HttpPost("enrollments/bulk")]
        [ActionPermissionFilter("hr:training:enroll")]
        [Log(Title = "Bulk Training Enrollment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> BulkEnroll([FromBody] BulkEnrollmentDto dto)
        {
            var result = await _trainingService.BulkEnrollAsync(dto.SessionId, dto.EmployeeIds);
            return SUCCESS(new { EnrolledCount = result });
        }

        /// <summary>
        /// Check enrollment eligibility
        /// </summary>
        [HttpGet("enrollments/eligibility")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> CheckEnrollmentEligibility([FromQuery] long employeeId, [FromQuery] long courseId)
        {
            var eligible = await _trainingService.CheckEnrollmentEligibilityAsync(employeeId, courseId);
            return SUCCESS(new { IsEligible = eligible });
        }

        #endregion

        #region Training Progress

        /// <summary>
        /// Get training progress
        /// </summary>
        [HttpGet("progress/{enrollmentId}")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetProgress(long enrollmentId)
        {
            var progress = await _trainingService.GetProgressAsync(enrollmentId);
            return SUCCESS(progress);
        }

        /// <summary>
        /// Update training progress
        /// </summary>
        [HttpPut("progress/{progressId}")]
        [ActionPermissionFilter("hr:training:progress")]
        [Log(Title = "Training Progress Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateProgress(long progressId, [FromBody] TrainingProgressDto dto)
        {
            var progress = dto.Adapt<HrTrainingProgress>();
            progress.ProgressId = progressId;
            progress.Update_by = HttpContext.GetName();
            progress.Update_time = DateTime.Now;
            
            var result = await _trainingService.UpdateProgressAsync(progress);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get overall progress
        /// </summary>
        [HttpGet("progress/{enrollmentId}/overall")]
        [ActionPermissionFilter("hr:training:list")]
        public async Task<IActionResult> GetOverallProgress(long enrollmentId)
        {
            var progress = await _trainingService.GetOverallProgressAsync(enrollmentId);
            return SUCCESS(new { OverallProgress = progress });
        }

        /// <summary>
        /// Record module completion
        /// </summary>
        [HttpPost("progress/{enrollmentId}/module")]
        [ActionPermissionFilter("hr:training:progress")]
        [Log(Title = "Training Module Completion", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RecordModuleCompletion(long enrollmentId, [FromBody] ModuleCompletionDto dto)
        {
            var result = await _trainingService.RecordModuleCompletionAsync(enrollmentId, dto.ModuleName, (decimal)dto.Score);
            return SUCCESS(result);
        }

        #endregion

        #region Training Budget

        /// <summary>
        /// Get training budget
        /// </summary>
        [HttpGet("budget")]
        [ActionPermissionFilter("hr:training:budget")]
        public async Task<IActionResult> GetBudget([FromQuery] long? departmentId, [FromQuery] int year)
        {
            var budget = await _trainingService.GetBudgetAsync(departmentId, year);
            return SUCCESS(budget);
        }

        /// <summary>
        /// Allocate training budget
        /// </summary>
        [HttpPost("budget")]
        [ActionPermissionFilter("hr:training:budget:allocate")]
        [Log(Title = "Training Budget Allocation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AllocateBudget([FromBody] TrainingBudgetDto dto)
        {
            var budget = dto.Adapt<HrTrainingBudget>();
            budget.Create_by = HttpContext.GetName();
            budget.Create_time = DateTime.Now;
            
            var result = await _trainingService.AllocateBudgetAsync(budget);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update budget usage
        /// </summary>
        [HttpPost("budget/{budgetId}/usage")]
        [ActionPermissionFilter("hr:training:budget:update")]
        [Log(Title = "Training Budget Usage Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateBudgetUsage(long budgetId, [FromBody] UpdateBudgetUsageDto dto)
        {
            var result = await _trainingService.UpdateBudgetUsageAsync(budgetId, dto.Amount);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get remaining budget
        /// </summary>
        [HttpGet("budget/remaining")]
        [ActionPermissionFilter("hr:training:budget")]
        public async Task<IActionResult> GetRemainingBudget([FromQuery] long? departmentId, [FromQuery] int year)
        {
            var remaining = await _trainingService.GetRemainingBudgetAsync(departmentId, year);
            return SUCCESS(new { RemainingBudget = remaining });
        }

        /// <summary>
        /// Get budget utilization
        /// </summary>
        [HttpGet("budget/utilization")]
        [ActionPermissionFilter("hr:training:budget")]
        public async Task<IActionResult> GetBudgetUtilization([FromQuery] int year)
        {
            var utilization = await _trainingService.GetBudgetUtilizationAsync(year);
            return SUCCESS(utilization);
        }

        #endregion

        #region Development Plans

        /// <summary>
        /// Get development plans
        /// </summary>
        [HttpGet("development-plans/employee/{employeeId}")]
        [ActionPermissionFilter("hr:training:development")]
        public async Task<IActionResult> GetDevelopmentPlans(long employeeId)
        {
            var plans = await _trainingService.GetDevelopmentPlansAsync(employeeId);
            return SUCCESS(plans);
        }

        /// <summary>
        /// Get development plan by ID
        /// </summary>
        [HttpGet("development-plans/{planId}")]
        [ActionPermissionFilter("hr:training:development")]
        public async Task<IActionResult> GetDevelopmentPlanById(long planId)
        {
            var plan = await _trainingService.GetDevelopmentPlanByIdAsync(planId);
            return SUCCESS(plan);
        }

        /// <summary>
        /// Create development plan
        /// </summary>
        [HttpPost("development-plans")]
        [ActionPermissionFilter("hr:training:development:create")]
        [Log(Title = "Development Plan Creation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateDevelopmentPlan([FromBody] DevelopmentPlanDto dto)
        {
            var plan = dto.Adapt<HrDevelopmentPlan>();
            plan.Create_by = HttpContext.GetName();
            plan.Create_time = DateTime.Now;
            
            var result = await _trainingService.CreateDevelopmentPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update development plan
        /// </summary>
        [HttpPut("development-plans/{planId}")]
        [ActionPermissionFilter("hr:training:development:edit")]
        [Log(Title = "Development Plan Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDevelopmentPlan(long planId, [FromBody] DevelopmentPlanDto dto)
        {
            var plan = dto.Adapt<HrDevelopmentPlan>();
            plan.PlanId = planId;
            plan.Update_by = HttpContext.GetName();
            plan.Update_time = DateTime.Now;
            
            var result = await _trainingService.UpdateDevelopmentPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update plan progress
        /// </summary>
        [HttpPut("development-plans/{planId}/progress")]
        [ActionPermissionFilter("hr:training:development:progress")]
        [Log(Title = "Development Plan Progress", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdatePlanProgress(long planId, [FromBody] UpdatePlanProgressDto dto)
        {
            var result = await _trainingService.UpdatePlanProgressAsync(planId, dto.Progress);
            return SUCCESS(result);
        }

        /// <summary>
        /// Provide mentor feedback
        /// </summary>
        [HttpPost("development-plans/{planId}/feedback")]
        [ActionPermissionFilter("hr:training:development:mentor")]
        [Log(Title = "Mentor Feedback", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProvideMentorFeedback(long planId, [FromBody] MentorFeedbackDto dto)
        {
            var result = await _trainingService.ProvideMentorFeedbackAsync(planId, dto.Feedback);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get plans for mentor
        /// </summary>
        [HttpGet("development-plans/mentor/{mentorId}")]
        [ActionPermissionFilter("hr:training:development:mentor")]
        public async Task<IActionResult> GetPlansForMentor(string mentorId)
        {
            var plans = await _trainingService.GetPlansForMentorAsync(mentorId);
            return SUCCESS(plans);
        }

        #endregion

        #region Training Analytics

        /// <summary>
        /// Get training metrics
        /// </summary>
        [HttpGet("analytics/metrics")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetTrainingMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var metrics = await _trainingService.GetTrainingMetricsAsync(startDate, endDate);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get employee training history
        /// </summary>
        [HttpGet("analytics/employee/{employeeId}/history")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetEmployeeTrainingHistory(long employeeId)
        {
            var history = await _trainingService.GetEmployeeTrainingHistoryAsync(employeeId);
            return SUCCESS(history);
        }

        /// <summary>
        /// Get course effectiveness
        /// </summary>
        [HttpGet("analytics/course/{courseId}/effectiveness")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetCourseEffectiveness(long courseId)
        {
            var effectiveness = await _trainingService.GetCourseEffectivenessAsync(courseId);
            return SUCCESS(effectiveness);
        }

        /// <summary>
        /// Get training completion rates
        /// </summary>
        [HttpGet("analytics/completion-rates")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetTrainingCompletionRates([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var rates = await _trainingService.GetTrainingCompletionRatesAsync(startDate, endDate);
            return SUCCESS(rates);
        }

        /// <summary>
        /// Get skill gap analysis
        /// </summary>
        [HttpGet("analytics/skill-gap")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetSkillGapAnalysis([FromQuery] long? departmentId)
        {
            var analysis = await _trainingService.GetSkillGapAnalysisAsync(departmentId);
            return SUCCESS(analysis);
        }

        /// <summary>
        /// Get training ROI
        /// </summary>
        [HttpGet("analytics/roi/{courseId}")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetTrainingROI(long courseId)
        {
            var roi = await _trainingService.GetTrainingROIAsync(courseId);
            return SUCCESS(roi);
        }

        /// <summary>
        /// Get top performing trainees
        /// </summary>
        [HttpGet("analytics/top-performers/{sessionId}")]
        [ActionPermissionFilter("hr:training:analytics")]
        public async Task<IActionResult> GetTopPerformingTrainees(long sessionId)
        {
            var performers = await _trainingService.GetTopPerformingTraineesAsync(sessionId);
            return SUCCESS(performers);
        }

        #endregion

        #region Certification Management

        /// <summary>
        /// Issue certificate
        /// </summary>
        [HttpPost("certificates/issue/{enrollmentId}")]
        [ActionPermissionFilter("hr:training:certificates")]
        [Log(Title = "Issue Training Certificate", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> IssueCertificate(long enrollmentId)
        {
            var result = await _trainingService.IssueCertificateAsync(enrollmentId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get certificate details
        /// </summary>
        [HttpGet("certificates/{certificateNumber}")]
        [ActionPermissionFilter("hr:training:certificates")]
        public async Task<IActionResult> GetCertificateDetails(string certificateNumber)
        {
            var certificate = await _trainingService.GetCertificateDetailsAsync(certificateNumber);
            return SUCCESS(certificate);
        }

        /// <summary>
        /// Get expiring certificates
        /// </summary>
        [HttpGet("certificates/expiring")]
        [ActionPermissionFilter("hr:training:certificates")]
        public async Task<IActionResult> GetExpiringCertificates([FromQuery] int daysAhead = 30)
        {
            var certificates = await _trainingService.GetExpiringCertificatesAsync(daysAhead);
            return SUCCESS(certificates);
        }

        /// <summary>
        /// Renew certification
        /// </summary>
        [HttpPost("certificates/renew/{enrollmentId}")]
        [ActionPermissionFilter("hr:training:certificates")]
        [Log(Title = "Renew Training Certificate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RenewCertification(long enrollmentId)
        {
            var result = await _trainingService.RenewCertificationAsync(enrollmentId);
            return SUCCESS(result);
        }

        #endregion

        #region Training Feedback

        /// <summary>
        /// Submit training feedback
        /// </summary>
        [HttpPost("feedback")]
        [ActionPermissionFilter("hr:training:feedback")]
        [Log(Title = "Training Feedback", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SubmitTrainingFeedback([FromBody] TrainingFeedbackDto dto)
        {
            var result = await _trainingService.SubmitTrainingFeedbackAsync(dto.EnrollmentId, dto.Feedback, dto.Rating);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get course feedback
        /// </summary>
        [HttpGet("feedback/course/{courseId}")]
        [ActionPermissionFilter("hr:training:feedback")]
        public async Task<IActionResult> GetCourseFeedback(long courseId)
        {
            var feedback = await _trainingService.GetCourseFeedbackAsync(courseId);
            return SUCCESS(feedback);
        }

        /// <summary>
        /// Get instructor ratings
        /// </summary>
        [HttpGet("feedback/instructor/{instructor}")]
        [ActionPermissionFilter("hr:training:feedback")]
        public async Task<IActionResult> GetInstructorRatings(string instructor)
        {
            var ratings = await _trainingService.GetInstructorRatingsAsync(instructor);
            return SUCCESS(ratings);
        }

        #endregion

        #region Training Reports

        /// <summary>
        /// Generate training report
        /// </summary>
        [HttpPost("reports/individual")]
        [ActionPermissionFilter("hr:training:reports")]
        [Log(Title = "Generate Training Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateTrainingReport([FromBody] TrainingReportDto dto)
        {
            var reportData = await _trainingService.GenerateTrainingReportAsync(dto.EmployeeId, dto.StartDate, dto.EndDate);
            var fileName = $"Training_Report_{dto.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(reportData, "application/pdf", fileName);
        }

        /// <summary>
        /// Generate session report
        /// </summary>
        [HttpGet("reports/session/{sessionId}")]
        [ActionPermissionFilter("hr:training:reports")]
        [Log(Title = "Generate Session Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateSessionReport(long sessionId)
        {
            var reportData = await _trainingService.GenerateSessionReportAsync(sessionId);
            var fileName = $"Session_Report_{sessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(reportData, "application/pdf", fileName);
        }

        /// <summary>
        /// Generate certificate
        /// </summary>
        [HttpGet("certificates/generate/{enrollmentId}")]
        [ActionPermissionFilter("hr:training:certificates")]
        [Log(Title = "Generate Certificate", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateCertificate(long enrollmentId)
        {
            var certificateData = await _trainingService.GenerateCertificateAsync(enrollmentId);
            var fileName = $"Certificate_{enrollmentId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(certificateData, "application/pdf", fileName);
        }

        /// <summary>
        /// Export training data
        /// </summary>
        [HttpPost("export")]
        [ActionPermissionFilter("hr:training:export")]
        [Log(Title = "Export Training Data", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportTrainingData([FromBody] ExportTrainingDto dto)
        {
            var predicate = Expressionable.Create<HrTrainingCourse>();
            
            if (!string.IsNullOrEmpty(dto.Category))
                predicate = predicate.And(it => it.Category == dto.Category);
            if (dto.IsActive.HasValue)
                predicate = predicate.And(it => it.IsActive == dto.IsActive);
            if (dto.IsMandatory.HasValue)
                predicate = predicate.And(it => it.IsMandatory == dto.IsMandatory);
            
            var exportData = await _trainingService.ExportTrainingDataAsync(predicate.ToExpression(), dto.Format);
            var fileName = $"Training_Data_{DateTime.Now:yyyyMMdd_HHmmss}.{dto.Format.ToLower()}";
            var contentType = dto.Format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(exportData, contentType, fileName);
        }

        #endregion

        #region Training Recommendations

        /// <summary>
        /// Get recommended courses
        /// </summary>
        [HttpGet("recommendations/{employeeId}")]
        [ActionPermissionFilter("hr:training:recommendations")]
        public async Task<IActionResult> GetRecommendedCourses(long employeeId)
        {
            var courses = await _trainingService.GetRecommendedCoursesAsync(employeeId);
            return SUCCESS(courses);
        }

        /// <summary>
        /// Get career path courses
        /// </summary>
        [HttpGet("recommendations/{employeeId}/career-path")]
        [ActionPermissionFilter("hr:training:recommendations")]
        public async Task<IActionResult> GetCareerPathCourses(long employeeId)
        {
            var courses = await _trainingService.GetCareerPathCoursesAsync(employeeId);
            return SUCCESS(courses);
        }

        /// <summary>
        /// Get skill-based recommendations
        /// </summary>
        [HttpGet("recommendations/{employeeId}/skill-based")]
        [ActionPermissionFilter("hr:training:recommendations")]
        public async Task<IActionResult> GetSkillBasedRecommendations(long employeeId)
        {
            var courses = await _trainingService.GetSkillBasedRecommendationsAsync(employeeId);
            return SUCCESS(courses);
        }

        #endregion
    }
}
