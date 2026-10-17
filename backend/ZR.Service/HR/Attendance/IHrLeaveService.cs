using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Configuration;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// HR Leave Service Interface
    /// </summary>
    public interface IHrLeaveService
    {
        // Leave Request Management





        Task<PagedInfo<HrLeaveRequest>> GetPageListAsync(Expression<Func<HrLeaveRequest, bool>> predicate, LeaveQueryDto pager);
        Task<List<HrLeaveRequest>> GetListAsync(Expression<Func<HrLeaveRequest, bool>> predicate);
        Task<HrLeaveRequest> GetByIdAsync(long requestId);
        Task<long> AddAsync(HrLeaveRequest request);
        new Task<int> UpdateAsync(HrLeaveRequest request);
        Task<int> DeleteAsync(long[] requestIds);
        Task<bool> CancelRequestAsync(long requestId, string cancelledBy);
        Task<bool> ApproveRequestAsync(long requestId, bool approved, string comments, string approvedBy);
        Task<List<HrLeaveRequest>> GetPendingApprovalsAsync(long managerId);
        Task<List<HrLeaveRequest>> GetEmployeeRequestsAsync(long employeeId, int year);
        Task<object> GetStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<List<HrHoliday>> GetHolidaysAsync(int year);
         




        Task<bool> IsHolidayAsync(      DateTime date);

         Task<int> CalculateLeaveDaysWithOptionsAsync(DateTime startDate, DateTime endDate, LeaveCalculationOptions options = null);


        Task<int> CalculateLeaveDaysAsync(DateTime startDate, DateTime endDate, bool excludeHolidays = true);
        Task<bool> CheckLeaveConflictAsync(long employeeId, DateTime startDate, DateTime endDate, long? excludeRequestId = null);
        Task<object> GetLeaveCalendarAsync(long? departmentId, int year, int month);
        Task<int> ProcessLeaveExpirationsAsync();
        Task<bool> UpdateRequestStatus(long requestId, string status, string comments);
        Task<bool> ApproveRequest(long requestId, bool approved, string comments, string approvedBy);












        Task<List<HrLeaveRequest>> GetEmployeeLeaveRequestsAsync(long employeeId, int year);
        Task<HrLeaveRequest> GetLeaveRequestByIdAsync(long leaveId);
        Task<int> SubmitLeaveRequestAsync(HrLeaveRequest request);
        Task<int> UpdateLeaveRequestAsync(HrLeaveRequest request);
        Task<bool> DeleteLeaveRequestAsync(long leaveId);
        Task<bool> ApproveLeaveRequestAsync(long leaveId, long approvedBy, string comments);
        Task<bool> RejectLeaveRequestAsync(long leaveId, long rejectedBy, string reason);
        Task<bool> CancelLeaveRequestAsync(long leaveId, string reason);
        Task<List<HrLeaveRequest>> GetPendingLeaveRequestsAsync(long managerId);
        Task<List<HrLeaveRequest>> GetLeaveRequestsByStatusAsync(string status);
        
        // Leave Balance Management
        Task<List<HrLeaveBalance>> GetEmployeeLeaveBalanceAsync(long? employeeId, int year);
        Task<bool> UpdateLeaveBalanceAsync(HrLeaveBalance balance);
        Task<bool> DeductLeaveBalanceAsync(long employeeId, long leaveTypeId, decimal days);
        Task<bool> AddLeaveBalanceAsync(long employeeId, long leaveTypeId, decimal days);
        Task<bool> ProcessYearEndBalancesAsync(int year);
        Task<bool> CarryForwardLeaveAsync(long employeeId, int fromYear, int toYear);
        
        // Leave Type Management
        Task<List<HrLeaveType>> GetLeaveTypesAsync();
        Task<HrLeaveType> GetLeaveTypeByIdAsync(long typeId);
        Task<int> AddLeaveTypeAsync(HrLeaveType leaveType);
        Task<bool> UpdateLeaveTypeAsync(HrLeaveType leaveType);
        Task<bool> DeleteLeaveTypeAsync(long typeId);
        
        // Holidays Management
        Task<bool> AddHolidayAsync(HrHoliday holiday);
        Task<bool> UpdateHolidayAsync(HrHoliday holiday);
        Task<bool> DeleteHolidayAsync(long holidayId);
        
        
        // Reports
        Task<byte[]> GenerateLeaveReportAsync(long employeeId, int year);
        Task<byte[]> GenerateDepartmentLeaveReportAsync(long departmentId, int year);
        
        // Additional Methods
        Task<object> GetEmployeeLeaveHistoryAsync(long employeeId, int year);
    }
}
