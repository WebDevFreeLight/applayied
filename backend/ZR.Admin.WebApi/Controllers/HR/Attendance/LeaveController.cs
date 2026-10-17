using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR;
using ZR.Model.HR.Attendance.Configuration;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Leave Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/leave")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class LeaveController : BaseController
    {
        private readonly IHrLeaveService _leaveService;
        private readonly IHrLeaveTypeService _leaveTypeService;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly INotificationService _notificationService;
        public LeaveController(
            IHrLeaveService leaveService,
            IHrLeaveTypeService leaveTypeService,
            ILeaveBalanceService leaveBalanceService,
            INotificationService notificationService
            )
        {
            _notificationService= notificationService;
            _leaveService = leaveService;
            _leaveTypeService = leaveTypeService;
            _leaveBalanceService = leaveBalanceService;
        }


        /// <summary>
        /// Cancel leave request
        /// </summary>
        [HttpPost("requests/{id}/cancel")]
        [ActionPermissionFilter("hr:leave:edit")]
        [Log(Title = "Leave Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelLeaveRequest(long id)
        {
            var result = await _leaveService.CancelRequestAsync(id, HttpContext.GetName());
            return SUCCESS(result);
        } 

        /// <summary>
        /// Get leave types
        /// </summary>
        [HttpGet("types")]
        [ActionPermissionFilter("hr:leave:list")]
        public async Task<IActionResult> GetLeaveTypes()
        {
            var types = await _leaveTypeService.GetActiveTypesAsync();
            return SUCCESS(types);
        }

        /// <summary>
        /// Get all leave types (including inactive)
        /// </summary>
        [HttpGet("types/all")]
        [ActionPermissionFilter("hr:leave:list")]
        public IActionResult GetAllLeaveTypes([FromQuery] HrLeaveTypeQueryDto dto)
        {
            var types = _leaveTypeService.GetList(dto);
            return SUCCESS(types);
        }

        /// <summary>
        /// Get holidays
        /// </summary>
        [HttpGet("holidays")]
        [ActionPermissionFilter("hr:leave:list")]
        public async Task<IActionResult> GetHolidays([FromQuery] int year)
        {
            var holidays = await _leaveService.GetHolidaysAsync(year);
            return SUCCESS(holidays);
        }

        /// <summary>
        /// Add holiday
        /// </summary>
        [HttpPost("holidays")]
        [ActionPermissionFilter("hr:leave:add")]
        [Log(Title = "Holiday Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddHoliday([FromBody] HolidayDto dto)
        {
            var holiday = dto.Adapt<HrHoliday>();
            holiday.Create_by = HttpContext.GetName();
            holiday.Create_time = DateTime.Now;
            
            var result = await _leaveService.AddHolidayAsync(holiday);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get leave statistics
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter ("hr:leave:list")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var stats = await _leaveService.GetStatisticsAsync(startDate, endDate);
            return SUCCESS(stats);
        }

        /// <summary>
        /// Calculate leave days between two dates
        /// </summary>
        [HttpPost("calculate-days")]
        [ActionPermissionFilter("hr:leave:query")]

        public async Task<IActionResult> CalculateLeaveDays([FromBody] CalculateDaysDto dto)
        {
            var days = await _leaveService.CalculateLeaveDaysAsync(dto.StartDate, dto.EndDate, dto.ExcludeHolidays);
            return SUCCESS(days);
        }

        /// <summary>
        /// Approve/Reject leave request
        /// </summary>
        [HttpPost("requests/{id}/approve")]
        [ActionPermissionFilter("hr:leave:approve")]
        [Log(Title = "Leave Approval", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveLeaveRequest(long id, [FromBody] ApprovalDto dto)
        {
            var result = await _leaveService.ApproveRequestAsync(id, dto.Approved, dto.Comments, HttpContext.GetUId().ToString());

            if (result && dto.Approved)
            {
                // Update leave balance
                var request = await _leaveService.GetByIdAsync(id);
                await _leaveBalanceService.DeductBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.Days ?? 0);
            }

            return SUCCESS(result);
        }



        /// <summary>
        /// Approve leave request
        /// </summary>
        [HttpPost("approve/{id}")]
        [ActionPermissionFilter("hr:leave:approve")]
        public async Task<IActionResult> ApproveLeave(long id, [FromBody] ApprovalRequest request)
        {
            var userId = HttpContext.GetUId();
            var result = await _leaveService.ApproveLeaveRequestAsync(id, userId, request.Comments);

            if (result)
            {
                var leave = await _leaveService.GetLeaveRequestByIdAsync(id);
                await _notificationService.SendLeaveApprovalNotificationAsync(leave.EmployeeId, id, "approved");
            }

            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Reject leave request
        /// </summary>
        [HttpPost("reject/{id}")]
        [ActionPermissionFilter("hr:leave:reject")]
        public async Task<IActionResult> RejectLeave(long id, [FromBody] RejectionRequest request)
        {
            var userId = HttpContext.GetUId();
            var result = await _leaveService.RejectLeaveRequestAsync(id, userId, request.Reason);

            if (result)
            {
                var leave = await _leaveService.GetLeaveRequestByIdAsync(id);
                await _notificationService.SendLeaveApprovalNotificationAsync(leave.EmployeeId, id, "rejected");
            }

            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get leave balance
        /// </summary>
        [HttpGet("balance")]
        [ActionPermissionFilter("hr:leave:balance")]
        public async Task<IActionResult> GetLeaveBalance([FromQuery] long? employeeId = null)
        {
            var year = DateTime.Now.Year;
            var balance = await _leaveService.GetEmployeeLeaveBalanceAsync(employeeId, year);
            return SUCCESS(balance);
        }

        /// <summary>
        /// Cancel leave request
        /// </summary>
        [HttpPost("cancel/{id}")]
        [ActionPermissionFilter("hr:leave:cancel")]
        public async Task<IActionResult> CancelLeave(long id, [FromBody] CancellationRequest request)
        {
            var result = await _leaveService.CancelLeaveRequestAsync(id, request.Reason);

            if (result)
            {
                var leave = await _leaveService.GetLeaveRequestByIdAsync(id);
                await _notificationService.SendNotificationAsync(new HrNotification
                {
                    RecipientId = leave.EmployeeId,
                    NotificationType = "in_app",
                    Subject = "Leave Request Cancelled",
                    Body = $"Your leave request from {leave.StartDate:yyyy-MM-dd} to {leave.EndDate:yyyy-MM-dd} has been cancelled.",
                    Category = "leave",
                    Priority = "low",
                    Create_by = "System",
                    Create_time = DateTime.Now
                });
            }

            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get leave history
        /// </summary>
        [HttpGet("history/{employeeId}")]
        [ActionPermissionFilter("hr:leave:view")]
        public async Task<IActionResult> GetLeaveHistory(long employeeId, [FromQuery] int year)
        {
            var history = await _leaveService.GetEmployeeLeaveHistoryAsync(employeeId, year);
            return SUCCESS(history);
        }


        /// <summary>
        /// Get leave requests
        /// </summary>
        [HttpGet("requestsv1")]
        [ActionPermissionFilter("hr:leave:list")]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] string status = null)
        {
            var requests = status != null
                ? await _leaveService.GetLeaveRequestsByStatusAsync(status)
                : await _leaveService.GetLeaveRequestsByStatusAsync("all");
            return SUCCESS(requests);
        }


        /// <summary>
        /// Get leave request list
        /// </summary>
        [HttpGet("requests")]
        [ActionPermissionFilter("hr:leave:list")]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] LeaveQueryDto dto)
        {
            var predicate = Expressionable.Create<HrLeaveRequest>();

            predicate = predicate.AndIF(dto.EmployeeId > 0,
                it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(dto.LeaveTypeId > 0,
                it => it.LeaveTypeId == dto.LeaveTypeId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status),
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.StartDate != null,
                it => it.StartDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null,
                it => it.EndDate <= dto.EndDate);

            var response = await _leaveService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }


        /// <summary>
        /// Get leave request details
        /// </summary>
        [HttpGet("requests/{id}")]
        [ActionPermissionFilter("hr:leave:query")]
        public async Task<IActionResult> GetLeaveRequest(long id)
        {
            var request = await _leaveService.GetByIdAsync(id);
            if (request == null)
            {
                return ToResponse(ResultCode.FAIL, "Leave request not found");
            }
            return SUCCESS(request);
        }


        /// <summary>
        /// Submit leave request
        /// </summary>
        [HttpPost("requests")]
        [ActionPermissionFilter("hr:leave:add")]
        public async Task<IActionResult> SubmitLeaveRequest([FromBody] LeaveRequestDto dto)
        {
            var request = dto.Adapt<HrLeaveRequest>();
            request.Create_by = HttpContext.GetName();
            request.Create_time = DateTime.Now;

            // Service handles all validation and throws AppException on failure
            var result = await _leaveService.SubmitLeaveRequestAsync(request);

            // Send notification
            await _notificationService.SendNotificationAsync(new HrNotification
            {
                RecipientId = request.EmployeeId,
                NotificationType = "in_app",
                Subject = "Leave Request Submitted",
                Body = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been submitted.",
                Category = "leave",
                Priority = "medium",
                Create_by = "System",
                Create_time = DateTime.Now
            });

            return SUCCESS(result);
        }

        /// <summary>
        /// Update leave request
        /// </summary>
        [HttpPut("requests")]
        [ActionPermissionFilter("hr:leave:edit")]
        [Log(Title = "Leave Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateLeaveRequest([FromBody] LeaveRequestDto dto)
        {
            var request = dto.Adapt<HrLeaveRequest>();
            request.Update_by = HttpContext.GetName();
            request.Update_time = DateTime.Now;

            var result = await _leaveService.UpdateAsync(request);
            return SUCCESS(result);
        }

  

        /// <summary>
        /// Add leave type
        /// </summary>
        [HttpPost("types")]
        [ActionPermissionFilter("hr:leave:add")]
        [Log(Title = "Leave Type", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddLeaveType([FromBody] LeaveTypeDto dto)
        {
            var type = dto.Adapt<HrLeaveType>();
            type.Create_by = HttpContext.GetName();
            type.Create_time = DateTime.Now;

            var result =   _leaveTypeService.Add (type);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update leave type
        /// </summary>
        [HttpPut("types")]
        [ActionPermissionFilter("hr:leave:edit")]
        [Log(Title = "Leave Type", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateLeaveType([FromBody] UpdateHrLeaveTypeDto dto)
        {
            var result = _leaveTypeService.UpdateHrLeaveType(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete leave type
        /// </summary>
        [HttpDelete("types/{id}")]
        [ActionPermissionFilter("hr:leave:delete")]
        [Log(Title = "Leave Type", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteLeaveType(long id)
        {
            var result = await _leaveTypeService.DeleteAsync(id);
            return SUCCESS(result);
        }



        /// <summary>
        /// Get employee leave balance
        /// </summary>
        [HttpGet("employeebalance/{employeeId}")]
        [ActionPermissionFilter("hr:leave:query")]
        public async Task<IActionResult> GetEmployeeBalancesAsync(long employeeId)
        {
            var balances = await _leaveBalanceService.GetEmployeeBalancesAsync(employeeId);
            return SUCCESS(balances);
        }




 

        /// <summary>
        /// Set employee leave balance
        /// </summary>
        [HttpPost("balance")]
        [ActionPermissionFilter("hr:leave:edit")]
        [Log(Title = "Leave Balance", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SetLeaveBalance([FromBody] ZR.Model.HR.Dto.LeaveBalanceDto dto)
        {
            var balance = dto.Adapt<HrLeaveBalance>();
            balance.Update_by = HttpContext.GetName();
            balance.Update_time = DateTime.Now;

            var result = await _leaveBalanceService.SetBalanceAsync(balance);
            return SUCCESS(result);
        }
 
    }



    public class ApprovalRequest
    {
        public string? Comments { get; set; }
    }

    public class RejectionRequest
    {
        public string? Reason { get; set; }
    }

    public class CancellationRequest
    {
        public string? Reason { get; set; }
    }
}
