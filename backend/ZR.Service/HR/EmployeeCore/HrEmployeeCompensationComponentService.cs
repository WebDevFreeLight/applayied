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
    [AppService(ServiceType = typeof(IHrEmployeeCompensationComponentService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeCompensationComponentService : BaseService<HrEmployeeCompensationComponent>, IHrEmployeeCompensationComponentService
    {
        public async Task<HrEmployeeCompensationComponent> GetByIdAsync(long compComponentId)
        {
            return await Context.Queryable<HrEmployeeCompensationComponent>().InSingleAsync(compComponentId);
        }

        public async Task<List<HrEmployeeCompensationComponent>> GetListAsync(Expression<Func<HrEmployeeCompensationComponent, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeCompensationComponent, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeCompensationComponent>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeCompensationComponent entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeCompensationComponent entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] compComponentIds)
        {
            return await Context.Deleteable<HrEmployeeCompensationComponent>().In(compComponentIds).ExecuteCommandAsync();
        }
    }
}














