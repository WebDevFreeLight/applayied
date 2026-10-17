using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.EmployeeCore
{
    [AppService(ServiceType = typeof(IHrEmployeeCompensationPackageService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeCompensationPackageService : BaseService<HrEmployeeCompensationPackage>, IHrEmployeeCompensationPackageService
    {
        public async Task<PagedInfo<HrEmployeeCompensationPackage>> GetPageListAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate, PagerInfo pager)
        {
            RefAsync<int> totalCount = 0;
            var list = await Queryable()
                .Where(predicate)
                .ToPageListAsync(pager.PageNum, pager.PageSize, totalCount);

            return new PagedInfo<HrEmployeeCompensationPackage>
            {
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                TotalNum = totalCount,
                Result = list
            };
        }

        public async Task<HrEmployeeCompensationPackage> GetByIdAsync(long compPackageId)
        {
            return await Context.Queryable<HrEmployeeCompensationPackage>().InSingleAsync(compPackageId);
        }

        public async Task<List<HrEmployeeCompensationPackage>> GetListAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeCompensationPackage, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeCompensationPackage>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeCompensationPackage entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeCompensationPackage entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] compPackageIds)
        {
            return await Context.Deleteable<HrEmployeeCompensationPackage>().In(compPackageIds).ExecuteCommandAsync();
        }
    }
}


