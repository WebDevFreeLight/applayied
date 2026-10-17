using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Customers.Dto;
using ZR.Service.Customers.ICustomersService;
using ZR.Service.Sales.ISalesService;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Operations lead access - read-only view of Confirmed leads
    /// </summary>
    [Verify]
    [Route("api/operations/leads")]
    public class OperationsLeadController : BaseController
    {
        private readonly IBpLeadService _bpLeadService;
        private readonly ISalesDealService _dealService;

        public OperationsLeadController(IBpLeadService bpLeadService, ISalesDealService dealService)
        {
            _bpLeadService = bpLeadService;
            _dealService = dealService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        /// <summary>
        /// Get leads list for Operations (only Confirmed leads)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("operations:leads:list")]
        public async Task<IActionResult> GetLeadsList([FromQuery] LeadQueryDto dto)
        {
            var currentUser = GetCurrentUser();
            dto.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            dto.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            // Operations sees all leads where readyForOperations = true
            dto.FilterByReadyForOperations = true;

            var result = await _bpLeadService.GetLeadsListAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get a single lead by ID
        /// </summary>
        [HttpGet("{leadId}")]
        [ActionPermissionFilter("operations:leads:view")]
        public async Task<IActionResult> GetLead(long leadId)
        {
            return SUCCESS(await _bpLeadService.GetLeadByIdAsync(leadId));
        }

        /// <summary>
        /// Get deals for a specific lead (used to link visits to deals)
        /// </summary>
        [HttpGet("{leadId}/deals")]
        [ActionPermissionFilter("operations:visits:add")]
        public async Task<IActionResult> GetDealsForLead(long leadId)
        {
            var deals = await _dealService.GetDealsByLeadAsync(leadId);
            return SUCCESS(deals);
        }
    }
}
