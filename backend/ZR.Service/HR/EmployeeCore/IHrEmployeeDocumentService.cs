using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Employee Document Service Interface
    /// </summary>
    public interface IHrEmployeeDocumentService : IBaseService<HrEmployeeDocument>
    {
        Task<HrEmployeeDocument> GetByIdAsync(long documentId);
        Task<List<HrEmployeeDocument>> GetListAsync(Expression<Func<HrEmployeeDocument, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<HrEmployeeDocument, bool>> predicate);
        Task<int> AddAsync(HrEmployeeDocument entity);
        Task<int> UpdateAsync(HrEmployeeDocument entity);
        Task<int> DeleteAsync(long[] documentIds);
    }
}
