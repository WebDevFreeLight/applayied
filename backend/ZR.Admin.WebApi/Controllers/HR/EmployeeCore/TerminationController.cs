using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Termination / Exit Management
    /// </summary>
    [Verify]
    [Route("api/hr/termination")]
    [ApiController]
    public class TerminationController : BaseController
    {
        private readonly ITerminationService _terminationService;

        public TerminationController(ITerminationService terminationService)
        {
            _terminationService = terminationService;
        }

        /// <summary>
        /// Get termination records (paginated)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:termination:list")]
        public async Task<IActionResult> GetList([FromQuery] TerminationQueryDto query)
        {
            var result = await _terminationService.GetListAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get termination by ID
        /// </summary>
        [HttpGet("{terminationId}")]
        [ActionPermissionFilter("hr:termination:query")]
        public async Task<IActionResult> GetById(long terminationId)
        {
            var result = await _terminationService.GetByIdAsync(terminationId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Initiate termination / resignation
        /// </summary>
        [HttpPost("initiate")]
        [ActionPermissionFilter("hr:termination:add")]
        [Log(Title = "Initiate Termination", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Initiate([FromBody] TerminationRequestDto request)
        {
            var user = JwtUtil.GetLoginUser(HttpContext);
            var result = await _terminationService.InitiateTerminationAsync(request, user.UserName);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve or reject termination
        /// </summary>
        [HttpPost("approve")]
        [ActionPermissionFilter("hr:termination:approve")]
        [Log(Title = "Approve Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Approve([FromBody] TerminationApprovalDto approval)
        {
            var user = JwtUtil.GetLoginUser(HttpContext);
            var result = await _terminationService.ApproveTerminationAsync(approval, user.UserId);
            return result ? SUCCESS("Processed successfully") : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Calculate final settlement (ESB + Leave Encashment - Loans)
        /// </summary>
        [HttpGet("{terminationId}/settlement")]
        [ActionPermissionFilter("hr:termination:settlement")]
        public async Task<IActionResult> CalculateSettlement(long terminationId)
        {
            var result = await _terminationService.CalculateSettlementAsync(terminationId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update exit checklist
        /// </summary>
        [HttpPut("checklist")]
        [ActionPermissionFilter("hr:termination:edit")]
        [Log(Title = "Update Exit Checklist", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateChecklist([FromBody] TerminationChecklistDto checklist)
        {
            var result = await _terminationService.UpdateChecklistAsync(checklist);
            return result ? SUCCESS("Checklist updated") : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Complete termination (deactivates employee)
        /// </summary>
        [HttpPost("{terminationId}/complete")]
        [ActionPermissionFilter("hr:termination:complete")]
        [Log(Title = "Complete Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Complete(long terminationId)
        {
            var result = await _terminationService.CompleteTerminationAsync(terminationId);
            return result ? SUCCESS("Termination completed") : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Cancel termination
        /// </summary>
        [HttpPost("{terminationId}/cancel")]
        [ActionPermissionFilter("hr:termination:cancel")]
        [Log(Title = "Cancel Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Cancel(long terminationId, [FromQuery] string reason)
        {
            var result = await _terminationService.CancelTerminationAsync(terminationId, reason);
            return result ? SUCCESS("Termination cancelled") : ToResponse(ResultCode.FAIL);
        }
    }
}
