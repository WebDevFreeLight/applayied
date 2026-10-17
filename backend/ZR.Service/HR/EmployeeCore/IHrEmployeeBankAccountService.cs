using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeBankAccountService
    {
        Task<HrEmployeeBankAccount> GetByIdAsync(long employeeBankAccountId);
        Task<List<HrEmployeeBankAccount>> GetListAsync(Expression<Func<HrEmployeeBankAccount, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeBankAccount, bool>> predicate);

        Task<int> AddAsync(HrEmployeeBankAccount entity);
        Task<int> UpdateAsync(HrEmployeeBankAccount entity);
        Task<int> DeleteAsync(long[] employeeBankAccountIds);
    }
}














