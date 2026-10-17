using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Calendar Assignment Service
    /// Manages employee to work calendar assignments
    /// </summary>
    public interface ICalendarAssignmentService
    {
        /// <summary>
        /// Assign an employee to a work calendar
        /// </summary>
        Task<CalendarAssignmentDto> AssignCalendar(CalendarAssignmentRequestDto dto);

        /// <summary>
        /// Get current active calendar for an employee
        /// </summary>
        Task<WorkCalendarDto> GetEmployeeCalendar(long employeeId, DateTime date);

        /// <summary>
        /// Get all calendar assignments for an employee
        /// </summary>
        Task<List<CalendarAssignmentDto>> GetEmployeeCalendarAssignments(long employeeId);

        /// <summary>
        /// Check if a date is a working day for an employee
        /// </summary>
        Task<bool> IsWorkingDay(long employeeId, DateTime date);

        /// <summary>
        /// Get expected working hours for an employee on a specific date
        /// </summary>
        Task<int> GetExpectedMinutes(long employeeId, DateTime date);

        // Additional methods for controller support
        Task<bool> AssignCalendarToEmployeeAsync(long employeeId, long calendarId, DateTime effectiveDate);
        Task<object> GetEmployeeCalendarAsync(long employeeId);
    }
}
