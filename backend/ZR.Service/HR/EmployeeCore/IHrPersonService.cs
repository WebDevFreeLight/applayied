using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonService
    {
        Task<PagedInfo<HrPerson>> GetPageListAsync(Expression<Func<HrPerson, bool>> predicate, PagerInfo pager);
        Task<HrPerson> GetByIdAsync(long personId);
        Task<List<HrPerson>> GetListAsync(Expression<Func<HrPerson, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPerson, bool>> predicate);

        Task<int> AddAsync(HrPerson entity);
        Task<int> UpdateAsync(HrPerson entity);
        Task<int> DeleteAsync(long[] personIds);
    }
}


