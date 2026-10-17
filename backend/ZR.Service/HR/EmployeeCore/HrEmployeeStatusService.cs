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
    [AppService(ServiceType = typeof(IHrEmployeeStatusService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeStatusService : BaseService<HrEmployeeStatus>, IHrEmployeeStatusService
    {
        public async Task<HrEmployeeStatus> GetByIdAsync(long employeeStatusId)
        {
            return await Context.Queryable<HrEmployeeStatus>().InSingleAsync(employeeStatusId);
        }

        public async Task<List<HrEmployeeStatus>> GetListAsync(Expression<Func<HrEmployeeStatus, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeStatus, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeStatus>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeStatus entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeStatus entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeStatusIds)
        {
            return await Context.Deleteable<HrEmployeeStatus>().In(employeeStatusIds).ExecuteCommandAsync();
        }
    }
}














