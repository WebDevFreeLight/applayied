using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Shifts_Scheduling;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Employee Shift Assignments
    /// Manages employee to shift assignments
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeShiftAssignmentRepository : BaseRepository<HrEmployeeShiftAssignment>
    {
        /// <summary>
        /// Get current active shift assignment for an employee
        /// </summary>
        public async Task<HrEmployeeShiftAssignment> GetCurrentAssignment(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrEmployeeShiftAssignment>()
                .Where(asg => asg.EmployeeId == employeeId
                    && asg.EffectiveFrom <= date
                    && (asg.EffectiveTo == null || asg.EffectiveTo >= date))
                .OrderByDescending(asg => asg.IsPrimary)
                .FirstAsync();
        }

        /// <summary>
        /// Get all shift assignments for an employee
        /// </summary>
        public async Task<List<HrEmployeeShiftAssignment>> GetAssignmentsByEmployee(long employeeId)
        {
            return await Context.Queryable<HrEmployeeShiftAssignment>()
                .Where(asg => asg.EmployeeId == employeeId)
                .OrderByDescending(asg => asg.EffectiveFrom)
                .ToListAsync();
        }
    }
}
