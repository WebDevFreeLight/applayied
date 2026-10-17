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
    [AppService(ServiceType = typeof(IHrPersonIdentifierService), ServiceLifetime = LifeTime.Transient)]
    public class HrPersonIdentifierService : BaseService<HrPersonIdentifier>, IHrPersonIdentifierService
    {
        public async Task<HrPersonIdentifier> GetByIdAsync(long personIdentifierId)
        {
            return await Context.Queryable<HrPersonIdentifier>().InSingleAsync(personIdentifierId);
        }

        public async Task<List<HrPersonIdentifier>> GetListAsync(Expression<Func<HrPersonIdentifier, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPersonIdentifier, bool>> predicate)
        {
            return await Context.Queryable<HrPersonIdentifier>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPersonIdentifier entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPersonIdentifier entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personIdentifierIds)
        {
            return await Context.Deleteable<HrPersonIdentifier>().In(personIdentifierIds).ExecuteCommandAsync();
        }
    }
}














