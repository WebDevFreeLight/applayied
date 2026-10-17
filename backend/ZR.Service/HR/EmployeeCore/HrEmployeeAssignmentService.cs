using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.EmployeeCore
{
    [AppService(ServiceType = typeof(IHrEmployeeAssignmentService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeAssignmentService : BaseService<HrEmployeeAssignment>, IHrEmployeeAssignmentService
    {
        public async Task<HrEmployeeAssignment> GetByIdAsync(long employeeAssignmentId)
        {
            return await Context.Queryable<HrEmployeeAssignment>().InSingleAsync(employeeAssignmentId);
        }

        public async Task<List<HrEmployeeAssignment>> GetListAsync(Expression<Func<HrEmployeeAssignment, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeAssignment, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeAssignment>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeAssignment entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeAssignment entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeAssignmentIds)
        {
            return await Context.Deleteable<HrEmployeeAssignment>().In(employeeAssignmentIds).ExecuteCommandAsync();
        }
    }
}














