using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Employee Document Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IHrEmployeeDocumentService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeDocumentService : BaseService<HrEmployeeDocument>, IHrEmployeeDocumentService
    {
        public async Task<HrEmployeeDocument> GetByIdAsync(long documentId)
        {
            return await Context.Queryable<HrEmployeeDocument>().InSingleAsync(documentId);
        }

        public async Task<List<HrEmployeeDocument>> GetListAsync(Expression<Func<HrEmployeeDocument, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeDocument, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeDocument>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeDocument entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeDocument entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] documentIds)
        {
            return await Context.Deleteable<HrEmployeeDocument>().In(documentIds).ExecuteCommandAsync();
        }
    }
}
