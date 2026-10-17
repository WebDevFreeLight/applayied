using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Attendance Adjustment Service
    /// Handles manual adjustments to attendance records
    /// </summary>
    public interface IAttendanceAdjustmentService
    {
        /// <summary>
        /// Create an attendance adjustment
        /// </summary>
        Task<HrAttendanceAdjustmentDto> CreateAdjustment(CreateHrAttendanceAdjustmentDto dto);

        /// <summary>
        /// Approve an attendance adjustment
        /// </summary>
        Task<bool> ApproveAdjustment(ApproveAdjustmentDto dto);

        /// <summary>
        /// Reject an attendance adjustment
        /// </summary>
        Task<bool> RejectAdjustment(long adjustmentId, long rejectedBy, string reason);

        /// <summary>
        /// Apply adjustment to attendance day
        /// Recalculates attendance after adjustment
        /// </summary>
        Task<HrAttendanceDayDto> ApplyAdjustment(long adjustmentId);

        /// <summary>
        /// Get adjustments for an employee
        /// </summary>
        Task<List<HrAttendanceAdjustmentDto>> GetAdjustments(long employeeId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
