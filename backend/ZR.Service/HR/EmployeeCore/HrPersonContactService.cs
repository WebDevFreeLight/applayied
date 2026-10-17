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
    [AppService(ServiceType = typeof(IHrPersonContactService), ServiceLifetime = LifeTime.Transient)]
    public class HrPersonContactService : BaseService<HrPersonContact>, IHrPersonContactService
    {
        public async Task<HrPersonContact> GetByIdAsync(long personContactId)
        {
            return await Context.Queryable<HrPersonContact>().InSingleAsync(personContactId);
        }

        public async Task<List<HrPersonContact>> GetListAsync(Expression<Func<HrPersonContact, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPersonContact, bool>> predicate)
        {
            return await Context.Queryable<HrPersonContact>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPersonContact entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPersonContact entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personContactIds)
        {
            return await Context.Deleteable<HrPersonContact>().In(personContactIds).ExecuteCommandAsync();
        }
    }
}














