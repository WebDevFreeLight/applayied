using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeAssignmentService
    {
        Task<HrEmployeeAssignment> GetByIdAsync(long employeeAssignmentId);
        Task<List<HrEmployeeAssignment>> GetListAsync(Expression<Func<HrEmployeeAssignment, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeAssignment, bool>> predicate);

        Task<int> AddAsync(HrEmployeeAssignment entity);
        Task<int> UpdateAsync(HrEmployeeAssignment entity);
        Task<int> DeleteAsync(long[] employeeAssignmentIds);
    }
}














