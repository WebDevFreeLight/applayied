using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeDependentLinkService
    {
        Task<HrEmployeeDependentLink> GetByIdAsync(long employeeDependentLinkId);
        Task<List<HrEmployeeDependentLink>> GetListAsync(Expression<Func<HrEmployeeDependentLink, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeDependentLink, bool>> predicate);

        Task<int> AddAsync(HrEmployeeDependentLink entity);
        Task<int> UpdateAsync(HrEmployeeDependentLink entity);
        Task<int> DeleteAsync(long[] employeeDependentLinkIds);
    }
}














