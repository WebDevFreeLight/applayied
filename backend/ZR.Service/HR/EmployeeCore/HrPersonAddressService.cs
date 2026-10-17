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
    [AppService(ServiceType = typeof(IHrPersonAddressService), ServiceLifetime = LifeTime.Transient)]
    public class HrPersonAddressService : BaseService<HrPersonAddress>, IHrPersonAddressService
    {
        public async Task<HrPersonAddress> GetByIdAsync(long personAddressId)
        {
            return await Context.Queryable<HrPersonAddress>().InSingleAsync(personAddressId);
        }

        public async Task<List<HrPersonAddress>> GetListAsync(Expression<Func<HrPersonAddress, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPersonAddress, bool>> predicate)
        {
            return await Context.Queryable<HrPersonAddress>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPersonAddress entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPersonAddress entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personAddressIds)
        {
            return await Context.Deleteable<HrPersonAddress>().In(personAddressIds).ExecuteCommandAsync();
        }
    }
}














