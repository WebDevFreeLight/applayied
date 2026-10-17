using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Leave Requests
    /// Manages employee leave requests and approvals
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveRequestRepository : BaseRepository<HrLeaveRequest>
    {
        /// <summary>
        /// Get approved leaves for an employee within a date range
        /// </summary>
        public async Task<List<HrLeaveRequest>> GetApprovedLeavesByDateRange(long employeeId, DateTime fromDate, DateTime toDate)
        {
            return await Context.Queryable<HrLeaveRequest>()
                .Where(req => req.EmployeeId == employeeId
                    && req.FromDate <= toDate
                    && req.ToDate >= fromDate
                    && req.StatusDictCode == 2) // Assuming 2 = Approved status
                .OrderBy(req => req.FromDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get leave requests by employee
        /// </summary>
        public async Task<List<HrLeaveRequest>> GetLeaveRequestsByEmployee(long employeeId)
        {
            return await Context.Queryable<HrLeaveRequest>()
                .Where(req => req.EmployeeId == employeeId)
                .OrderByDescending(req => req.FromDate)
                .ToListAsync();
        }
    }
}
