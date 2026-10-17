using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonDemographicService
    {
        Task<HrPersonDemographic> GetByIdAsync(long personId);
        Task<List<HrPersonDemographic>> GetListAsync(Expression<Func<HrPersonDemographic, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPersonDemographic, bool>> predicate);

        Task<int> AddAsync(HrPersonDemographic entity);
        Task<int> UpdateAsync(HrPersonDemographic entity);
        Task<int> DeleteAsync(long[] personIds);
    }
}














