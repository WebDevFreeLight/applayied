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
    [AppService(ServiceType = typeof(IHrEmployeeContractService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeContractService : BaseService<HrEmployeeContract>, IHrEmployeeContractService
    {
        public async Task<HrEmployeeContract> GetByIdAsync(long employeeContractId)
        {
            return await Context.Queryable<HrEmployeeContract>().InSingleAsync(employeeContractId);
        }

        public async Task<List<HrEmployeeContract>> GetListAsync(Expression<Func<HrEmployeeContract, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeContract, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeContract>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeContract entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeContract entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeContractIds)
        {
            return await Context.Deleteable<HrEmployeeContract>().In(employeeContractIds).ExecuteCommandAsync();
        }
    }
}














