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
    [AppService(ServiceType = typeof(IHrPersonDemographicService), ServiceLifetime = LifeTime.Transient)]
    public class HrPersonDemographicService : BaseService<HrPersonDemographic>, IHrPersonDemographicService
    {
        public async Task<HrPersonDemographic> GetByIdAsync(long personId)
        {
            return await Context.Queryable<HrPersonDemographic>().InSingleAsync(personId);
        }

        public async Task<List<HrPersonDemographic>> GetListAsync(Expression<Func<HrPersonDemographic, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPersonDemographic, bool>> predicate)
        {
            return await Context.Queryable<HrPersonDemographic>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPersonDemographic entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPersonDemographic entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personIds)
        {
            return await Context.Deleteable<HrPersonDemographic>().In(personIds).ExecuteCommandAsync();
        }
    }
}














