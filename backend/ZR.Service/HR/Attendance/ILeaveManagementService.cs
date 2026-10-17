using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Leave Management Service
    /// Processes leave requests and calculates leave effects on attendance
    /// </summary>
    public interface ILeaveManagementService
    {
        /// <summary>
        /// Submit a leave request
        /// </summary>
        Task<HrLeaveRequestDto> SubmitLeaveRequest(LeaveRequestCreateDto dto);

        /// <summary>
        /// Approve a leave request and generate leave effects
        /// </summary>
        Task ApproveLeaveRequest(LeaveApprovalDto dto);

        /// <summary>
        /// Reject a leave request
        /// </summary>
        Task<bool> RejectLeaveRequest(RejectLeaveRequestDto dto);

        /// <summary>
        /// Get leave balance for an employee
        /// </summary>
        Task<List<LeaveBalanceDto>> GetLeaveBalances(long employeeId);

        /// <summary>
        /// Get leave history for an employee
        /// </summary>
        Task<List<HrLeaveRequestDto>> GetLeaveHistory(long employeeId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Calculate leave effects for a leave request (internal - no return needed)
        /// Generates HrLeaveAttendanceEffect records for each affected date
        /// </summary>
        Task CalculateLeaveEffects(long requestId);

        /// <summary>
        /// Update leave balance after leave approval
        /// </summary>
        Task<bool> UpdateLeaveBalance(long employeeId, long leaveTypeId, decimal deductMinutes);
    }
}
