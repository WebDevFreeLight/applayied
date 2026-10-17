using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonNameService
    {
        Task<HrPersonName> GetByIdAsync(long personNameId);
        Task<List<HrPersonName>> GetListAsync(Expression<Func<HrPersonName, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPersonName, bool>> predicate);

        Task<int> AddAsync(HrPersonName entity);
        Task<int> UpdateAsync(HrPersonName entity);
        Task<int> DeleteAsync(long[] personNameIds);
    }
}














