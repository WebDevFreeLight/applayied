using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeCoreService
    {
        Task<PagedInfo<HrEmployeeCore>> GetPageListAsync(Expression<Func<HrEmployeeCore, bool>> predicate, PagerInfo pager);
        Task<HrEmployeeCore> GetByIdAsync(long employeeId);
        Task<List<HrEmployeeCore>> GetListAsync(Expression<Func<HrEmployeeCore, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeCore, bool>> predicate);

        Task<int> AddAsync(HrEmployeeCore entity);
        Task<int> UpdateAsync(HrEmployeeCore entity);
        Task<int> DeleteAsync(long[] employeeIds);

        /// <summary>
        /// Get lookup list for dropdowns (minimal data: EmployeeId, EmployeeCode, Names)
        /// </summary>
        Task<List<EmployeeLookupDto>> GetLookupList(string search = null);

        /// <summary>
        /// Get employee ID by user ID (hr_employee_core.user_id → sys_user.user_id)
        /// الحصول على معرف الموظف من معرف المستخدم
        /// </summary>
        Task<long?> GetEmployeeIdByUserIdAsync(long userId);
    }
}


