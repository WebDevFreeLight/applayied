using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeEmergencyContactService
    {
        Task<PagedInfo<HrEmployeeEmergencyContact>> GetPageListAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate, PagerInfo pager);
        Task<HrEmployeeEmergencyContact> GetByIdAsync(long emergencyContactId);
        Task<List<HrEmployeeEmergencyContact>> GetListAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate);

        Task<int> AddAsync(HrEmployeeEmergencyContact entity);
        Task<int> UpdateAsync(HrEmployeeEmergencyContact entity);
        Task<int> DeleteAsync(long[] emergencyContactIds);
    }
}


