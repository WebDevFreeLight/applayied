using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Overtime Management Controller - Overtime calculations, requests, approvals
    /// </summary>
    [Verify]
    [Route("api/hr/attendance/overtime")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class OvertimeController : BaseController
    {
        private readonly IOvertimeCalculationService _overtimeService;

        public OvertimeController(IOvertimeCalculationService overtimeService)
        {
            _overtimeService = overtimeService;
        }

        /// <summary>
        /// Calculate overtime for employee and date range
        /// </summary>
        [HttpPost("calculate")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Calculate Overtime", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CalculateOvertime([FromBody] OvertimeCalculationRequestDto dto)
        {
            var result = await _overtimeService.CalculateOvertimeAsync(dto.EmployeeId, dto.StartDate, dto.EndDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get employee overtime summary
        /// </summary>
        [HttpGet("employee/{employeeId}/summary")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetEmployeeOvertimeSummary(long employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var summary = await _overtimeService.GetEmployeeOvertimeSummaryAsync(employeeId, startDate, endDate);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get overtime requests
        /// </summary>
        [HttpGet("requests")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetOvertimeRequests([FromQuery] HrOvertimeRequestQueryDto dto)
        {
            var requests = await _overtimeService.GetOvertimeRequestsAsync(dto);
            return SUCCESS(requests);
        }

        /// <summary>
        /// Create overtime request
        /// </summary>
        [HttpPost("requests")]
        [ActionPermissionFilter("hr:attendance:add")]
        [Log(Title = "Overtime Request", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateOvertimeRequest([FromBody] CreateHrOvertimeRequestDto dto)
        {
            var result = await _overtimeService.CreateOvertimeRequestAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve overtime request
        /// </summary>
        [HttpPost("requests/{requestId}/approve")]
        [ActionPermissionFilter("hr:attendance:approve")]
        [Log(Title = "Approve Overtime", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveOvertimeRequest(long requestId, [FromBody] ApproveOvertimeDto dto)
        {
            var result = await _overtimeService.ApproveOvertimeRequestAsync(requestId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Reject overtime request
        /// </summary>
        [HttpPost("requests/{requestId}/reject")]
        [ActionPermissionFilter("hr:attendance:approve")]
        [Log(Title = "Reject Overtime", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectOvertimeRequest(long requestId, [FromBody] RejectOvertimeDto dto)
        {
            var result = await _overtimeService.RejectOvertimeRequestAsync(requestId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get overtime report
        /// </summary>
        [HttpGet("reports")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetOvertimeReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] long? departmentId)
        {
            var report = await _overtimeService.GetOvertimeReportAsync(startDate, endDate, departmentId);
            return SUCCESS(report);
        }

        /// <summary>
        /// Get overtime policies
        /// </summary>
        [HttpGet("policies")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetOvertimePolicies()
        {
            var policies = await _overtimeService.GetOvertimePoliciesAsync();
            return SUCCESS(policies);
        }

        /// <summary>
        /// Create overtime policy
        /// </summary>
        [HttpPost("policies")]
        [ActionPermissionFilter("hr:attendance:add")]
        [Log(Title = "Overtime Policy", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateOvertimePolicy([FromBody] CreateHrOvertimePolicyDto dto)
        {
            var result = await _overtimeService.CreateOvertimePolicyAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update overtime policy
        /// </summary>
        [HttpPut("policies/{policyId}")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Overtime Policy", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateOvertimePolicy(long policyId, [FromBody] UpdateHrOvertimePolicyDto dto)
        {
            var result = await _overtimeService.UpdateOvertimePolicyAsync(policyId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Verify overtime segments
        /// </summary>
        [HttpPost("verify")]
        [ActionPermissionFilter("hr:attendance:approve")]
        [Log(Title = "Verify Overtime", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> VerifyOvertimeSegments([FromBody] VerifyOvertimeDto dto)
        {
            var result = await _overtimeService.VerifyOvertimeSegmentsAsync(dto);
            return SUCCESS(new { VerifiedCount = result });
        }

        /// <summary>
        /// Get pending verifications
        /// </summary>
        [HttpGet("verifications/pending")]
        [ActionPermissionFilter("hr:attendance:list")]
        public async Task<IActionResult> GetPendingVerifications()
        {
            var verifications = await _overtimeService.GetPendingVerificationsAsync();
            return SUCCESS(verifications);
        }
    }

    public class OvertimeCalculationRequestDto
    {
        public long EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ApproveOvertimeDto
    {
        public decimal? ApprovedMinutes { get; set; }
        public string ApprovalNotes { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovalDate { get; set; }
    }

    public class RejectOvertimeDto
    {
        public string RejectionReason { get; set; }
        public string RejectedBy { get; set; }
        public DateTime RejectionDate { get; set; }
    }

    public class VerifyOvertimeDto
    {
        public long[] SegmentIds { get; set; }
        public string VerificationNotes { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime VerificationDate { get; set; }
    }
}
