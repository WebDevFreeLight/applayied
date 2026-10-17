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
    [AppService(ServiceType = typeof(IHrPersonNameService), ServiceLifetime = LifeTime.Transient)]
    public partial class HrPersonNameService : BaseService<HrPersonName>, IHrPersonNameService
    {
        public async Task<HrPersonName> GetByIdAsync(long personNameId)
        {
            return await Context.Queryable<HrPersonName>().InSingleAsync(personNameId);
        }

        public async Task<List<HrPersonName>> GetListAsync(Expression<Func<HrPersonName, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPersonName, bool>> predicate)
        {
            return await Context.Queryable<HrPersonName>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPersonName entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPersonName entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personNameIds)
        {
            return await Context.Deleteable<HrPersonName>().In(personNameIds).ExecuteCommandAsync();
        }
    }
}














