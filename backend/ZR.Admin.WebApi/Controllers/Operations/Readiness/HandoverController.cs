using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Controllers;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Operations.Readiness.Dto;
using ZR.Service.Operations.Readiness;

namespace ZR.Admin.WebApi.Controllers.Operations.Readiness
{
    /// <summary>
    /// Handover - تسليم المحل (Percentage-Based Handover Tracking)
    /// </summary>
    [Verify]
    [Route("api/operations/readiness")]
    [ApiController]
    public class HandoverController : BaseController
    {
        private readonly IOpsHandoverRequirementService _requirementService;
        private readonly IOpsStoreHandoverService _handoverService;

        public HandoverController(
            IOpsHandoverRequirementService requirementService,
            IOpsStoreHandoverService handoverService)
        {
            _requirementService = requirementService;
            _handoverService = handoverService;
        }

        // ═══════════════════════════════════════════════════
        // Requirements (config)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Get all active requirements (ordered by sort_order)
        /// </summary>
        [HttpGet("requirements")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> GetRequirements()
        {
            var result = await _requirementService.GetActiveRequirementsAsync();
            return SUCCESS(result);
        }

        /// <summary>
        /// Create a new requirement
        /// </summary>
        [HttpPost("requirements")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> CreateRequirement([FromBody] HandoverRequirementDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _requirementService.CreateRequirementAsync(dto, loginUser.UserName);
            // Sync new requirement to all active handovers
            await _handoverService.SyncRequirementsForActiveHandoversAsync(loginUser.UserName);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update a requirement
        /// </summary>
        [HttpPut("requirements/{id}")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> UpdateRequirement(long id, [FromBody] HandoverRequirementDto dto)
        {
            dto.RequirementId = id;
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _requirementService.UpdateRequirementAsync(dto, loginUser.UserName);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Requirement not found");
            // Sync updated requirements to all active handovers (handles reactivation/deactivation)
            await _handoverService.SyncRequirementsForActiveHandoversAsync(loginUser.UserName);
            return SUCCESS(result);
        }

        /// <summary>
        /// Soft-delete a requirement (set is_active = false)
        /// </summary>
        [HttpDelete("requirements/{id}")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> DeleteRequirement(long id)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _requirementService.SoftDeleteRequirementAsync(id, loginUser.UserName);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Requirement not found");
            // Sync: remove uncompleted items for deactivated requirement from active handovers
            await _handoverService.SyncRequirementsForActiveHandoversAsync(loginUser.UserName);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate that active requirement percentages sum to 100
        /// </summary>
        [HttpGet("requirements/validate")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> ValidateRequirements()
        {
            var result = await _requirementService.ValidatePercentagesAsync();
            return SUCCESS(result);
        }

        // ═══════════════════════════════════════════════════
        // Store Handover (tracking + workflow)
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Get paginated list of handovers (filter by status, shopId, seasonId)
        /// </summary>
        [HttpGet("handovers")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> GetHandovers([FromQuery] StoreHandoverQueryDto query)
        {
            var result = await _handoverService.GetHandoverListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get single handover with items
        /// </summary>
        [HttpGet("handovers/{id}")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> GetHandoverById(long id)
        {
            var result = await _handoverService.GetHandoverByIdAsync(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Handover not found");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get latest handover for a shop
        /// </summary>
        [HttpGet("handovers/by-shop/{shopId}")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> GetHandoverByShop(long shopId)
        {
            var result = await _handoverService.GetHandoverByShopAsync(shopId);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "No handover found for this store");
            return SUCCESS(result);
        }

        /// <summary>
        /// Initialize handover for a store (creates handover + items for each active requirement)
        /// </summary>
        [HttpPost("handovers/initialize")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> InitializeHandover([FromBody] InitializeHandoverDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.InitializeHandoverAsync(dto, loginUser.UserName);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Cannot initialize. Store may already have an active handover or no requirements are configured.");
            return SUCCESS(result);
        }

        /// <summary>
        /// Mark a handover item as complete (with optional photos)
        /// </summary>
        [HttpPost("handovers/items/{itemId}/complete")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> CompleteItem(long itemId, [FromBody] CompleteItemDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.CompleteItemAsync(itemId, dto, loginUser.UserName);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Item not found or photo required but not provided");
            return SUCCESS(result);
        }

        /// <summary>
        /// Revert a handover item to incomplete
        /// </summary>
        [HttpPost("handovers/items/{itemId}/uncomplete")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> UncompleteItem(long itemId)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.UncompleteItemAsync(itemId, loginUser.UserName);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Item not found");
            return SUCCESS(result);
        }

        /// <summary>
        /// Request handover approval (field team action)
        /// </summary>
        [HttpPost("handovers/{id}/request")]
        [ActionPermissionFilter(Permission = "operations:readiness:manage")]
        public async Task<IActionResult> RequestHandover(long id, [FromBody] RequestHandoverDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.RequestHandoverAsync(id, dto, loginUser.UserName);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Handover not found");
            if (result == -1)
                return ToResponse(ResultCode.FAIL, "Handover must be in ReadyForHandover status to request approval");
            if (result == -2)
                return ToResponse(ResultCode.FAIL, "Cannot request handover. Progress must be 100% with no blocking items incomplete.");
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve handover (manager action)
        /// </summary>
        [HttpPost("handovers/{id}/approve")]
        [ActionPermissionFilter(Permission = "operations:readiness:approve")]
        public async Task<IActionResult> ApproveHandover(long id, [FromBody] ApproveHandoverDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.ApproveHandoverAsync(id, dto, loginUser.UserName);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Handover not found");
            if (result == -1)
                return ToResponse(ResultCode.FAIL, "Handover must be in ReadyForHandover status to approve");
            return SUCCESS(result);
        }

        /// <summary>
        /// Reject handover (manager action)
        /// </summary>
        [HttpPost("handovers/{id}/reject")]
        [ActionPermissionFilter(Permission = "operations:readiness:approve")]
        public async Task<IActionResult> RejectHandover(long id, [FromBody] RejectHandoverDto dto)
        {
            var loginUser = JwtUtil.GetLoginUser(HttpContext);
            var result = await _handoverService.RejectHandoverAsync(id, dto, loginUser.UserName);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Handover not found");
            if (result == -1)
                return ToResponse(ResultCode.FAIL, "Handover must be in ReadyForHandover status to reject");
            return SUCCESS(result);
        }

        /// <summary>
        /// Get progress summary for a handover
        /// </summary>
        [HttpGet("handovers/{id}/progress")]
        [ActionPermissionFilter(Permission = "operations:readiness:query")]
        public async Task<IActionResult> GetProgress(long id)
        {
            var result = await _handoverService.GetProgressAsync(id);
            return SUCCESS(result);
        }
    }
}
