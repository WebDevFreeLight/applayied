using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    public interface IHrPersonIdentifierService
    {
        Task<HrPersonIdentifier> GetByIdAsync(long personIdentifierId);
        Task<List<HrPersonIdentifier>> GetListAsync(Expression<Func<HrPersonIdentifier, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrPersonIdentifier, bool>> predicate);

        Task<int> AddAsync(HrPersonIdentifier entity);
        Task<int> UpdateAsync(HrPersonIdentifier entity);
        Task<int> DeleteAsync(long[] personIdentifierIds);
    }
}














