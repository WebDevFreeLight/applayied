using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrEmployeeCompensationPackageService
    {
        Task<PagedInfo<HrEmployeeCompensationPackage>> GetPageListAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate, PagerInfo pager);
        Task<HrEmployeeCompensationPackage> GetByIdAsync(long compPackageId);
        Task<List<HrEmployeeCompensationPackage>> GetListAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate);

        Task<int> AddAsync(HrEmployeeCompensationPackage entity);
        Task<int> UpdateAsync(HrEmployeeCompensationPackage entity);
        Task<int> DeleteAsync(long[] compPackageIds);
    }
}


