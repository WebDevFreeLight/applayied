using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Site Visit Management - Operations module
    /// </summary>
    [Verify]
    [Route("api/operations/visits")]
    public class SiteVisitController : BaseController
    {
        private readonly ISiteVisitService _siteVisitService;

        public SiteVisitController(ISiteVisitService siteVisitService)
        {
            _siteVisitService = siteVisitService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get paginated visit list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("operations:visits:list,sales:visits:list")]
        public async Task<IActionResult> GetVisitsList([FromQuery] SiteVisitQueryDto query)
        {
            var currentUser = GetCurrentUser();
            query.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            query.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            var result = await _siteVisitService.GetVisitsListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get visit by ID
        /// </summary>
        [HttpGet("{visitId}")]
        [ActionPermissionFilter("operations:visits:view,sales:visits:list")]
        public async Task<IActionResult> GetVisit(long visitId)
        {
            var visit = await _siteVisitService.GetVisitByIdAsync(visitId);
            if (visit == null)
                return ToResponse(ResultCode.FAIL, "Visit not found");

            return SUCCESS(visit);
        }

        /// <summary>
        /// Get visits for a specific lead
        /// </summary>
        [HttpGet("lead/{leadId}")]
        [ActionPermissionFilter("operations:visits:list,sales:visits:list")]
        public async Task<IActionResult> GetVisitsByLead(long leadId)
        {
            var visits = await _siteVisitService.GetVisitsByLeadAsync(leadId);
            return SUCCESS(visits);
        }

        /// <summary>
        /// Get today's visits
        /// </summary>
        [HttpGet("today")]
        [ActionPermissionFilter("operations:visits:list,sales:visits:list")]
        public async Task<IActionResult> GetTodayVisits()
        {
            var currentUser = GetCurrentUser();
            long? staffId = null;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            if (!isAdmin && !hasAllDataScope && currentUser?.UserId != null)
            {
                staffId = await _siteVisitService.GetEmployeeIdByUserIdAsync(currentUser.UserId);
            }

            var visits = await _siteVisitService.GetTodayVisitsAsync(staffId);
            return SUCCESS(visits);
        }

        /// <summary>
        /// Get upcoming visits (next N days)
        /// </summary>
        [HttpGet("upcoming")]
        [ActionPermissionFilter("operations:visits:list,sales:visits:list")]
        public async Task<IActionResult> GetUpcomingVisits([FromQuery] int days = 7)
        {
            var currentUser = GetCurrentUser();
            long? staffId = null;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            if (!isAdmin && !hasAllDataScope && currentUser?.UserId != null)
            {
                staffId = await _siteVisitService.GetEmployeeIdByUserIdAsync(currentUser.UserId);
            }

            var visits = await _siteVisitService.GetUpcomingVisitsAsync(days, staffId);
            return SUCCESS(visits);
        }

        /// <summary>
        /// Schedule a new visit (only for Confirmed leads)
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter("sales:visits:add")]
        public async Task<IActionResult> CreateVisit([FromBody] CreateSiteVisitDto dto)
        {
            try
            {
                var currentUser = GetCurrentUser();
                var visit = new SiteVisit
                {
                    LeadId = dto.LeadId,
                    DealId = dto.DealId,
                    ScheduledDate = dto.ScheduledDate,
                    ScheduledTime = dto.ScheduledTime,
                    AssignedStaffId = dto.AssignedStaffId,
                    VisitNotes = dto.VisitNotes,
                    Create_by = currentUser?.UserName
                };

                var result = await _siteVisitService.CreateVisitAsync(visit);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update visit details
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("operations:visits:edit")]
        public async Task<IActionResult> UpdateVisit([FromBody] UpdateSiteVisitDto dto)
        {
            try
            {
                var existing = await _siteVisitService.GetVisitByIdAsync(dto.VisitId);
                if (existing == null)
                    return ToResponse(ResultCode.FAIL, "Visit not found");

                var currentUser = GetCurrentUser();
                var visit = new SiteVisit
                {
                    VisitId = dto.VisitId,
                    LeadId = dto.LeadId,
                    DealId = dto.DealId,
                    ScheduledDate = dto.ScheduledDate,
                    ScheduledTime = dto.ScheduledTime,
                    AssignedStaffId = dto.AssignedStaffId,
                    VisitNotes = dto.VisitNotes,
                    VisitStatus = existing.VisitStatus,
                    CompletionNotes = existing.CompletionNotes,
                    CompletedDate = existing.CompletedDate,
                    CancellationReason = existing.CancellationReason,
                    Create_by = existing.Create_by,
                    Create_time = existing.Create_time,
                    Update_by = currentUser?.UserName
                };

                var result = await _siteVisitService.UpdateVisitAsync(visit);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update visit status (Scheduled -> InProgress -> Completed / Cancelled)
        /// </summary>
        [HttpPost("update-status")]
        [ActionPermissionFilter("operations:visits:edit")]
        public async Task<IActionResult> UpdateVisitStatus([FromBody] UpdateVisitStatusDto dto)
        {
            try
            {
                var result = await _siteVisitService.UpdateVisitStatusAsync(dto.VisitId, dto.NewStatus, dto.Notes);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete visits
        /// </summary>
        [HttpPost("delete")]
        [ActionPermissionFilter("operations:visits:delete")]
        public async Task<IActionResult> DeleteVisits([FromBody] long[] visitIds)
        {
            var result = await _siteVisitService.DeleteVisitsAsync(visitIds);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get visit dashboard stats
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("operations:visits:list,sales:visits:list")]
        public async Task<IActionResult> GetDashboard()
        {
            var currentUser = GetCurrentUser();
            long? staffId = null;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            if (!isAdmin && !hasAllDataScope && currentUser?.UserId != null)
            {
                staffId = await _siteVisitService.GetEmployeeIdByUserIdAsync(currentUser.UserId);
            }

            var dashboard = await _siteVisitService.GetDashboardAsync(staffId);
            return SUCCESS(dashboard);
        }
    }
}
