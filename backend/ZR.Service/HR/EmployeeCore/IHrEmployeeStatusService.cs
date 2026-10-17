using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeStatusService
    {
        Task<HrEmployeeStatus> GetByIdAsync(long employeeStatusId);
        Task<List<HrEmployeeStatus>> GetListAsync(Expression<Func<HrEmployeeStatus, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeStatus, bool>> predicate);

        Task<int> AddAsync(HrEmployeeStatus entity);
        Task<int> UpdateAsync(HrEmployeeStatus entity);
        Task<int> DeleteAsync(long[] employeeStatusIds);
    }
}














