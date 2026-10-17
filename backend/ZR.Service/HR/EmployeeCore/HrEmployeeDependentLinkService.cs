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
    [AppService(ServiceType = typeof(IHrEmployeeDependentLinkService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeDependentLinkService : BaseService<HrEmployeeDependentLink>, IHrEmployeeDependentLinkService
    {
        public async Task<HrEmployeeDependentLink> GetByIdAsync(long employeeDependentLinkId)
        {
            return await Context.Queryable<HrEmployeeDependentLink>().InSingleAsync(employeeDependentLinkId);
        }

        public async Task<List<HrEmployeeDependentLink>> GetListAsync(Expression<Func<HrEmployeeDependentLink, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeDependentLink, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeDependentLink>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeDependentLink entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeDependentLink entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeDependentLinkIds)
        {
            return await Context.Deleteable<HrEmployeeDependentLink>().In(employeeDependentLinkIds).ExecuteCommandAsync();
        }
    }
}














