using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeStatutoryService
    {
        Task<HrEmployeeStatutory> GetByIdAsync(long employeeStatutoryId);
        Task<List<HrEmployeeStatutory>> GetListAsync(Expression<Func<HrEmployeeStatutory, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeStatutory, bool>> predicate);

        Task<int> AddAsync(HrEmployeeStatutory entity);
        Task<int> UpdateAsync(HrEmployeeStatutory entity);
        Task<int> DeleteAsync(long[] employeeStatutoryIds);
    }
}














