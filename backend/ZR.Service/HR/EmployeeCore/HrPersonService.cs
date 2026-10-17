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
    [AppService(ServiceType = typeof(IHrPersonService), ServiceLifetime = LifeTime.Transient)]
    public class HrPersonService : BaseService<HrPerson>, IHrPersonService
    {
        public async Task<PagedInfo<HrPerson>> GetPageListAsync(Expression<Func<HrPerson, bool>> predicate, PagerInfo pager)
        {
            RefAsync<int> totalCount = 0;
            var list = await Queryable()
                .Where(predicate)
                .ToPageListAsync(pager.PageNum, pager.PageSize, totalCount);

            return new PagedInfo<HrPerson>
            {
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                TotalNum = totalCount,
                Result = list
            };
        }

        public async Task<HrPerson> GetByIdAsync(long personId)
        {
            return await Context.Queryable<HrPerson>().InSingleAsync(personId);
        }

        public async Task<List<HrPerson>> GetListAsync(Expression<Func<HrPerson, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrPerson, bool>> predicate)
        {
            return await Context.Queryable<HrPerson>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrPerson entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrPerson entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] personIds)
        {
            return await Context.Deleteable<HrPerson>().In(personIds).ExecuteCommandAsync();
        }
    }
}


