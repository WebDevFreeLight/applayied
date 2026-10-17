using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Dto;

namespace ZR.Service.HR.IService
{
     
 

    public interface ILeaveBalanceService  
    {
        Task<HrLeaveBalance> GetBalanceAsync(long employeeId, long leaveTypeId);
        Task<List<HrLeaveBalance>> GetEmployeeBalancesAsync(long employeeId);
        Task<int> SetBalanceAsync(HrLeaveBalance balance);
        Task<bool> DeductBalanceAsync(long employeeId, long leaveTypeId, decimal days);
        Task<bool> AddBalanceAsync(long employeeId, long leaveTypeId, decimal days);
        Task<int> InitializeEmployeeBalancesAsync(long employeeId);
        Task<int> ProcessYearlyEntitlementsAsync(int year);
        Task<int> ProcessCarryOverAsync(int fromYear, int toYear);
        Task<object> GetBalanceSummaryAsync(long employeeId);
        Task<List<object>> GetDepartmentBalancesAsync(long departmentId);
        Task<int> AdjustBalanceAsync(long employeeId, long leaveTypeId, decimal adjustment, string reason);
    }
}
