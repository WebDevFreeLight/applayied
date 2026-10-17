using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Sales;
using ZR.Model.Sales.Dto;
using ZR.Service.Sales.ISalesService;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;
using Infrastructure;
using Infrastructure.Model;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    [Verify]
    [Route("api/sales/deals")]
    public class SalesDealController : BaseController
    {
        private readonly ISalesDealService _dealService;
        private readonly ICustomerService _customerService;

        public SalesDealController(ISalesDealService dealService, ICustomerService customerService)
        {
            _dealService = dealService;
            _customerService = customerService;
        }

        private TokenModel GetCurrentUser()
        {
            return JwtUtil.GetLoginUser(HttpContext);
        }

        private bool IsCurrentUserAdmin()
        {
            var user = GetCurrentUser();
            return user?.IsAdmin() ?? false;
        }

        /// <summary>
        /// Get deals list with pagination and filtering
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> GetDealsList([FromQuery] DealQueryDto dto)
        {
            var currentUser = GetCurrentUser();
            dto.CurrentUserId = currentUser?.UserId;

            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
            dto.IsCurrentUserAdmin = isAdmin || hasAllDataScope;

            var result = await _dealService.GetDealsListAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get deal by ID
        /// </summary>
        [HttpGet("{dealId}")]
        [ActionPermissionFilter("sales:deals:view")]
        public async Task<IActionResult> GetDeal(long dealId)
        {
            return SUCCESS(await _dealService.GetDealByIdAsync(dealId));
        }

        /// <summary>
        /// Get deals for a specific lead
        /// </summary>
        [HttpGet("lead/{leadId}")]
        [ActionPermissionFilter("sales:deals:list")]
        public async Task<IActionResult> GetDealsByLead(long leadId)
        {
            return SUCCESS(await _dealService.GetDealsByLeadAsync(leadId));
        }

        /// <summary>
        /// Create a new deal
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter("sales:deals:add")]
        public async Task<IActionResult> AddDeal([FromBody] SalesDeal deal)
        {
            try
            {
                var currentUser = GetCurrentUser();
                bool isAdmin = currentUser?.IsAdmin() ?? false;
                bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;
                bool isManager = isAdmin || hasAllDataScope;

                deal.Create_by = currentUser?.UserName;

                return SUCCESS(await _dealService.CreateDealAsync(deal, currentUser?.UserId, isManager));
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update an existing deal
        /// </summary>
        [HttpPost("update")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> UpdateDeal([FromBody] SalesDeal deal)
        {
            return SUCCESS(await _dealService.UpdateDealAsync(deal));
        }

        /// <summary>
        /// Update deal status (handles Won -> Converted logic)
        /// </summary>
        [HttpPost("update-status")]
        [ActionPermissionFilter("sales:deals:edit")]
        public async Task<IActionResult> UpdateDealStatus([FromBody] UpdateDealStatusDto dto)
        {
            try
            {
                var result = await _dealService.UpdateDealStatusAsync(dto.DealId, dto.NewStatus, dto.Notes);
                return SUCCESS(result, "Deal status updated successfully");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update deal status: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete deals
        /// </summary>
        [HttpPost("delete")]
        [ActionPermissionFilter("sales:deals:delete")]
        public async Task<IActionResult> Delete([FromBody] long[] dealIds)
        {
            return SUCCESS(await _dealService.DeleteDealsAsync(dealIds));
        }

        /// <summary>
        /// Get customer profile completion percentage by lead ID
        /// </summary>
        [HttpGet("profile-completion/{leadId}")]
        [ActionPermissionFilter("sales:deals:list")]
        public IActionResult GetProfileCompletion(long leadId)
        {
            var customer = _customerService.GetByLeadId(leadId);
            if (customer == null)
            {
                return SUCCESS(new { completion = 0, customerExists = false });
            }

            var completion = _customerService.GetProfileCompletionPercentage(customer.Id);
            return SUCCESS(new { completion, customerExists = true });
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("sales:dashboard:view")]
        public async Task<IActionResult> GetDashboard()
        {
            var currentUser = GetCurrentUser();
            bool isAdmin = currentUser?.IsAdmin() ?? false;
            bool hasAllDataScope = currentUser?.Roles?.Any(r => r.DataScope == 1) == true;

            long? assignedTo = null;
            if (!isAdmin && !hasAllDataScope && currentUser?.UserId > 0)
            {
                assignedTo = await _dealService.GetEmployeeIdByUserIdAsync(currentUser.UserId);
            }

            return SUCCESS(await _dealService.GetDashboardStatsAsync(assignedTo));
        }
    }
}
