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
    [AppService(ServiceType = typeof(IHrEmployeeEmergencyContactService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeEmergencyContactService : BaseService<HrEmployeeEmergencyContact>, IHrEmployeeEmergencyContactService
    {
        public async Task<PagedInfo<HrEmployeeEmergencyContact>> GetPageListAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate, PagerInfo pager)
        {
            RefAsync<int> totalCount = 0;
            var list = await Queryable()
                .Where(predicate)
                .ToPageListAsync(pager.PageNum, pager.PageSize, totalCount);

            return new PagedInfo<HrEmployeeEmergencyContact>
            {
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                TotalNum = totalCount,
                Result = list
            };
        }

        public async Task<HrEmployeeEmergencyContact> GetByIdAsync(long emergencyContactId)
        {
            return await Context.Queryable<HrEmployeeEmergencyContact>().InSingleAsync(emergencyContactId);
        }

        public async Task<List<HrEmployeeEmergencyContact>> GetListAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeEmergencyContact, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeEmergencyContact>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeEmergencyContact entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeEmergencyContact entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] emergencyContactIds)
        {
            return await Context.Deleteable<HrEmployeeEmergencyContact>().In(emergencyContactIds).ExecuteCommandAsync();
        }
    }
}


