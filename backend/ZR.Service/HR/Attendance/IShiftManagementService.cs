using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Shift Management Service
    /// Manages shifts and shift assignments
    /// </summary>
    public interface IShiftManagementService
    {
        /// <summary>
        /// Create a new shift with segments
        /// </summary>
        Task<ShiftDto> CreateShift(CreateShiftDto dto);

        /// <summary>
        /// Assign an employee to a shift
        /// </summary>
        Task<ShiftAssignmentDto> AssignEmployeeToShift(ShiftAssignmentRequestDto dto);

        /// <summary>
        /// Get employee's active shift for a specific date
        /// </summary>
        Task<ShiftDto> GetEmployeeShift(long employeeId, DateTime date);

        /// <summary>
        /// Get shift segments for a shift
        /// </summary>
        Task<List<ShiftSegmentDto>> GetShiftSegments(long shiftId);

        /// <summary>
        /// Get all active shifts
        /// </summary>
        Task<List<ShiftDto>> GetActiveShifts();

        /// <summary>
        /// Create shift calendar override for an employee
        /// </summary>
        Task<long> CreateShiftOverride(long employeeId, DateTime overrideDate, long? overrideShiftId, string reason);

        // Additional methods for controller support
        Task<List<object>> GetAllShiftsAsync();
        Task<object> GetShiftByIdAsync(long shiftId);
        Task<object> CreateShiftAsync(object dto);
        Task<bool> UpdateShiftAsync(long shiftId, object dto);
        Task<bool> DeleteShiftAsync(long shiftId);
        Task<object> GetEmployeeCurrentShiftAsync(long employeeId, DateTime date);
        Task<List<object>> GetEmployeeShiftAssignmentsAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<object> AssignShiftToEmployeeAsync(object dto);
        Task<int> BulkAssignShiftsAsync(object dto);
        Task<List<object>> GetShiftSchedulesAsync(DateTime startDate, DateTime endDate);
        Task<object> CreateShiftOverrideAsync(object dto);
        Task<List<object>> GetShiftOverridesAsync(DateTime startDate, DateTime endDate);
    }
}
