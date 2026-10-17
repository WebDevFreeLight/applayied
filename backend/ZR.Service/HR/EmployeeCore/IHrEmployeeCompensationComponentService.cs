using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeCompensationComponentService
    {
        Task<HrEmployeeCompensationComponent> GetByIdAsync(long compComponentId);
        Task<List<HrEmployeeCompensationComponent>> GetListAsync(Expression<Func<HrEmployeeCompensationComponent, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeCompensationComponent, bool>> predicate);

        Task<int> AddAsync(HrEmployeeCompensationComponent entity);
        Task<int> UpdateAsync(HrEmployeeCompensationComponent entity);
        Task<int> DeleteAsync(long[] compComponentIds);
    }
}














