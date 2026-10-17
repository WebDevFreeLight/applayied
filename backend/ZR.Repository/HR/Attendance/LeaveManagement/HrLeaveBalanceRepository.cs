using Infrastructure.Attribute;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Leave Balances
    /// Manages employee leave balances
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveBalanceRepository : BaseRepository<HrLeaveBalance>
    {
        /// <summary>
        /// Get leave balances for an employee
        /// </summary>
        public async Task<List<HrLeaveBalance>> GetBalancesByEmployee(long employeeId)
        {
            return await Context.Queryable<HrLeaveBalance>()
                .Where(bal => bal.EmployeeId == employeeId)
                .ToListAsync();
        }

        /// <summary>
        /// Get specific leave balance for employee and leave type
        /// </summary>
        public async Task<HrLeaveBalance> GetBalanceByEmployeeAndType(long employeeId, long leaveTypeId)
        {
            return await Context.Queryable<HrLeaveBalance>()
                .Where(bal => bal.EmployeeId == employeeId && bal.LeaveTypeId == leaveTypeId)
                .FirstAsync();
        }
    }
}
