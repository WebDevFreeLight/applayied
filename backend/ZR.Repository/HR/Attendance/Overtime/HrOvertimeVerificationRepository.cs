using Infrastructure.Attribute;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Overtime;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Overtime Verifications
    /// Manages overtime verification and approval records
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrOvertimeVerificationRepository : BaseRepository<HrOvertimeVerification>
    {
        /// <summary>
        /// Get verification record by segment ID
        /// </summary>
        public async Task<HrOvertimeVerification> GetBySegmentIdAsync(long segmentId)
        {
            return await Context.Queryable<HrOvertimeVerification>()
                .Where(v => v.SegmentId == segmentId)
                .FirstAsync();
        }

        /// <summary>
        /// Get all unverified overtime records
        /// </summary>
        public async Task<List<HrOvertimeVerification>> GetUnverifiedAsync()
        {
            return await Context.Queryable<HrOvertimeVerification>()
                .Where(v => v.IsVerified == false)
                .ToListAsync();
        }

        /// <summary>
        /// Get verifications by employee (through segments)
        /// </summary>
        public async Task<List<HrOvertimeVerification>> GetByEmployeeAsync(long employeeId)
        {
            return await Context.Queryable<HrOvertimeVerification>()
                .InnerJoin<HrOvertimeSegment>((v, s) => v.SegmentId == s.SegmentId)
                .Where((v, s) => s.EmployeeId == employeeId)
                .Select((v, s) => v)
                .ToListAsync();
        }
    }
}
