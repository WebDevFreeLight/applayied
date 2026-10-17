using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonContactService
    {
        Task<HrPersonContact> GetByIdAsync(long personContactId);
        Task<List<HrPersonContact>> GetListAsync(Expression<Func<HrPersonContact, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPersonContact, bool>> predicate);

        Task<int> AddAsync(HrPersonContact entity);
        Task<int> UpdateAsync(HrPersonContact entity);
        Task<int> DeleteAsync(long[] personContactIds);
    }
}














