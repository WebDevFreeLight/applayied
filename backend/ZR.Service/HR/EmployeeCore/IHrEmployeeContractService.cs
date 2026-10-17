using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeContractService
    {
        Task<HrEmployeeContract> GetByIdAsync(long employeeContractId);
        Task<List<HrEmployeeContract>> GetListAsync(Expression<Func<HrEmployeeContract, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeContract, bool>> predicate);

        Task<int> AddAsync(HrEmployeeContract entity);
        Task<int> UpdateAsync(HrEmployeeContract entity);
        Task<int> DeleteAsync(long[] employeeContractIds);
    }
}














