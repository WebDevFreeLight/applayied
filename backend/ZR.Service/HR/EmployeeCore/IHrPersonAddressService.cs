using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonAddressService
    {
        Task<HrPersonAddress> GetByIdAsync(long personAddressId);
        Task<List<HrPersonAddress>> GetListAsync(Expression<Func<HrPersonAddress, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPersonAddress, bool>> predicate);

        Task<int> AddAsync(HrPersonAddress entity);
        Task<int> UpdateAsync(HrPersonAddress entity);
        Task<int> DeleteAsync(long[] personAddressIds);
    }
}














