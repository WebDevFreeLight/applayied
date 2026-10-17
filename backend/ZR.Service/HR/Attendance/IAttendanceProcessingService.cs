using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Attendance Processing Service
    /// Core service for processing attendance logs into daily attendance records
    /// </summary>
    public interface IAttendanceProcessingService
    {
        /// <summary>
        /// Process attendance logs for a specific employee and date
        /// Generates HrAttendanceDay and HrAttendanceSegmentResult records
        /// </summary>
        Task<HrAttendanceDayDto> ProcessAttendanceForDate(long employeeId, DateTime date);

        /// <summary>
        /// Process attendance for an employee for a date range
        /// </summary>
        Task<List<HrAttendanceDayDto>> ProcessAttendanceForDateRange(long employeeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Process attendance for all employees for a specific date
        /// </summary>
        Task<int> ProcessAttendanceForAllEmployees(DateTime date);

        /// <summary>
        /// Recalculate attendance for a specific employee and date
        /// Useful after adjustments or corrections
        /// </summary>
        Task<HrAttendanceDayDto> RecalculateAttendance(long employeeId, DateTime date);

        /// <summary>
        /// Get processed attendance summary for an employee
        /// </summary>
        Task<List<HrAttendanceDayDto>> GetAttendanceSummary(long employeeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get employee attendance for a date range (alias for GetAttendanceSummary)
        /// </summary>
        Task<List<HrAttendanceDayDto>> GetEmployeeAttendanceAsync(long employeeId, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Check in employee attendance
        /// </summary>
        Task<int> CheckInAsync(CreateHrAttendanceDayDto dto);

        /// <summary>
        /// Check out employee attendance
        /// </summary>
        Task<int> CheckOutAsync(UpdateHrAttendanceDayDto dto);

        // Additional methods for controller support
        Task<List<HrAttendanceDayDto>> GetDailyAttendanceAsync(DateTime date);
        Task<PagedInfo<HrAttendanceDayDto>> GetPagedAttendanceAsync(HrAttendanceDayQueryDto dto);
        Task<object> GetDailySummaryAsync(DateTime date);
        Task<object> GetMonthlyReportAsync(long employeeId, int year, int month);
        Task<int> ProcessAttendanceForDateAsync(DateTime date);
        Task<object> GetAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<object> GetDepartmentSummaryAsync(long departmentId, DateTime date);
        Task<object> CreateAdjustmentAsync(object dto);
        Task<List<object>> GetPendingAdjustmentsAsync();
        Task<bool> ApproveAdjustmentAsync(long adjustmentId, object dto);
        Task<bool> RejectAdjustmentAsync(long adjustmentId, object dto);
    }
}
