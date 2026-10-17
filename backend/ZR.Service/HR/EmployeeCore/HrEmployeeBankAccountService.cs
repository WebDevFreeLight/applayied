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
    [AppService(ServiceType = typeof(IHrEmployeeBankAccountService), ServiceLifetime = LifeTime.Transient)]
    public partial class HrEmployeeBankAccountService : BaseService<HrEmployeeBankAccount>, IHrEmployeeBankAccountService
    {
        public async Task<HrEmployeeBankAccount> GetByIdAsync(long employeeBankAccountId)
        {
            return await Context.Queryable<HrEmployeeBankAccount>().InSingleAsync(employeeBankAccountId);
        }

        public async Task<List<HrEmployeeBankAccount>> GetListAsync(Expression<Func<HrEmployeeBankAccount, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeBankAccount, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeBankAccount>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeBankAccount entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeBankAccount entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeBankAccountIds)
        {
            return await Context.Deleteable<HrEmployeeBankAccount>().In(employeeBankAccountIds).ExecuteCommandAsync();
        }
    }
}














