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
    [AppService(ServiceType = typeof(IHrEmployeeStatutoryService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeStatutoryService : BaseService<HrEmployeeStatutory>, IHrEmployeeStatutoryService
    {
        public async Task<HrEmployeeStatutory> GetByIdAsync(long employeeStatutoryId)
        {
            return await Context.Queryable<HrEmployeeStatutory>().InSingleAsync(employeeStatutoryId);
        }

        public async Task<List<HrEmployeeStatutory>> GetListAsync(Expression<Func<HrEmployeeStatutory, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeStatutory, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeStatutory>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeStatutory entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeStatutory entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeStatutoryIds)
        {
            return await Context.Deleteable<HrEmployeeStatutory>().In(employeeStatutoryIds).ExecuteCommandAsync();
        }
    }
}














