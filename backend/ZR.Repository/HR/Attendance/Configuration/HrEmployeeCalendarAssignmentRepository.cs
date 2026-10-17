using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Employee Calendar Assignments
    /// Manages employee to work calendar assignments
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeCalendarAssignmentRepository : BaseRepository<HrEmployeeCalendarAssignment>
    {
        /// <summary>
        /// Get current active calendar assignment for an employee
        /// </summary>
        public async Task<HrEmployeeCalendarAssignment> GetCurrentAssignment(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrEmployeeCalendarAssignment>()
                .Where(asg => asg.EmployeeId == employeeId
                    && asg.EffectiveFrom <= date
                    && (asg.EffectiveTo == null || asg.EffectiveTo >= date))
                .OrderByDescending(asg => asg.IsPrimary)
                .FirstAsync();
        }

        /// <summary>
        /// Get all calendar assignments for an employee
        /// </summary>
        public async Task<List<HrEmployeeCalendarAssignment>> GetAssignmentsByEmployee(long employeeId)
        {
            return await Context.Queryable<HrEmployeeCalendarAssignment>()
                .Where(asg => asg.EmployeeId == employeeId)
                .OrderByDescending(asg => asg.EffectiveFrom)
                .ToListAsync();
        }
    }
}
