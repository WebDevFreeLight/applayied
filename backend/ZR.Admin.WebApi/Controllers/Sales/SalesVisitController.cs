using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Sales Visit Management - Sales can create visits and view status (read-only)
    /// </summary>
    [Verify]
    [Route("api/sales/visits")]
    public class SalesVisitController : BaseController
    {
        private readonly ISiteVisitService _siteVisitService;

        public SalesVisitController(ISiteVisitService siteVisitService)
        {
            _siteVisitService = siteVisitService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get paginated list of visits - admins see all, sales users see only their own
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("sales:visits:list")]
        public async Task<IActionResult> GetVisitsList([FromQuery] SiteVisitQueryDto query)
        {
            var currentUser = GetCurrentUser();
            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            if (isAdmin || hasAllDataScope)
            {
                query.IsCurrentUserAdmin = true;
                var result = await _siteVisitService.GetVisitsListAsync(query);
                return SUCCESS(result);
            }
            else
            {
                var result = await _siteVisitService.GetVisitsByCreatorAsync(query, currentUser?.UserName);
                return SUCCESS(result);
            }
        }

        /// <summary>
        /// Get visits for a specific lead (read-only for Sales)
        /// </summary>
        [HttpGet("lead/{leadId}")]
        [ActionPermissionFilter("sales:visits:list")]
        public async Task<IActionResult> GetVisitsByLead(long leadId)
        {
            var query = new SiteVisitQueryDto
            {
                LeadId = leadId,
                IsCurrentUserAdmin = true, // Sales can see all visits for their leads
                PageSize = 100
            };
            var result = await _siteVisitService.GetVisitsListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get visits for a specific deal (read-only for Sales)
        /// </summary>
        [HttpGet("deal/{dealId}")]
        [ActionPermissionFilter("sales:visits:list")]
        public async Task<IActionResult> GetVisitsByDeal(long dealId)
        {
            var query = new SiteVisitQueryDto
            {
                DealId = dealId,
                IsCurrentUserAdmin = true,
                PageSize = 100
            };
            var result = await _siteVisitService.GetVisitsListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Schedule a new visit (Sales can create, only for Confirmed leads)
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
    }
}
