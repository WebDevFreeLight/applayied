using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Leave Management Service Interface - Enterprise & Minute-Based
    /// </summary>
    public interface ILeaveService
    {
        // =========================
        // Query
        // =========================

        Task<PagedInfo<HrLeave>> GetPageListAsync(
            Expression<Func<HrLeave, bool>> predicate,
            PagerInfo pager);

        Task<HrLeave> GetByIdAsync(long leaveId);

        Task<List<HrLeave>> GetEmployeeLeavesAsync(
            long employeeId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // =========================
        // Command
        // =========================

        /// <summary>
        /// Submit new leave request (single entry point)
        /// </summary>
        Task<long> SubmitLeaveRequestAsync(
            HrLeave leave,
            long requestedById);

        Task<bool> ApproveLeaveAsync(
            long leaveId,
            long approverId,
            string comment = null);

        Task<bool> RejectLeaveAsync(
            long leaveId,
            long rejectedById,
            string reason);

        Task<bool> CancelLeaveAsync(
            long leaveId,
            long cancelledById,
            string reason = null);

        /// <summary>
        /// Forward leave to next approver (temporary – workflow engine will replace this)
        /// </summary>
        Task<bool> ForwardToNextApproverAsync(
            long leaveId,
            long currentApproverId,
            string comment = null);

        // =========================
        // Balance
        // =========================

        Task<HrLeaveBalance> GetCurrentBalanceAsync(
            long employeeId,
            long leaveTypeId);

        Task<bool> HasSufficientBalanceAsync(
            long employeeId,
            long leaveTypeId,
            decimal requestedMinutes);

        // =========================
        // Analytics
        // =========================

        Task<LeaveStatisticsDto> GetLeaveStatisticsAsync(
            long? employeeId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // =========================
        // Legacy
        // =========================

        [Obsolete("Use SubmitLeaveRequestAsync instead. This method is kept for backward compatibility only.")]
        Task<int> ApplyLeaveAsync(HrLeave leave);
    }
}
