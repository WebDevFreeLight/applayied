using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.EmployeeCore
{
    [AppService(ServiceType = typeof(IHrEmployeeCoreService), ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeCoreService : BaseService<HrEmployeeCore>, IHrEmployeeCoreService
    {
        public async Task<PagedInfo<HrEmployeeCore>> GetPageListAsync(Expression<Func<HrEmployeeCore, bool>> predicate, PagerInfo pager)
        {
            RefAsync<int> totalCount = 0;
            var list = await Queryable()
                .Where(predicate)
                .ToPageListAsync(pager.PageNum, pager.PageSize, totalCount);

            return new PagedInfo<HrEmployeeCore>
            {
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                TotalNum = totalCount,
                Result = list
            };
        }

        public async Task<HrEmployeeCore> GetByIdAsync(long employeeId)
        {
            return await Context.Queryable<HrEmployeeCore>().InSingleAsync(employeeId);
        }

        public async Task<List<HrEmployeeCore>> GetListAsync(Expression<Func<HrEmployeeCore, bool>> predicate)
        {
            return await Queryable().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<HrEmployeeCore, bool>> predicate)
        {
            return await Context.Queryable<HrEmployeeCore>().Where(predicate).AnyAsync();
        }

        public async Task<int> AddAsync(HrEmployeeCore entity)
        {
            return await Context.Insertable(entity).ExecuteCommandAsync();
        }

        public async Task<int> UpdateAsync(HrEmployeeCore entity)
        {
            return await Context.Updateable(entity).ExecuteCommandAsync();
        }

        public async Task<int> DeleteAsync(long[] employeeIds)
        {
            return await Context.Deleteable<HrEmployeeCore>().In(employeeIds).ExecuteCommandAsync();
        }

        /// <summary>
        /// Get lookup list for dropdowns (minimal data)
        /// Returns: EmployeeId, EmployeeCode, FirstName, LastName, FullName
        /// Only returns active employees with primary names
        /// </summary>
        public async Task<List<EmployeeLookupDto>> GetLookupList(string search = null)
        {
            var query = Context.Queryable<HrEmployeeCore, HrPersonName>((emp, name) => new JoinQueryInfos(
                JoinType.Inner, emp.PersonId == name.PersonId
            ))
            .Where((emp, name) => emp.IsActive == true && name.IsPrimary == true);

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where((emp, name) =>
                    emp.EmployeeCode.Contains(search) ||
                    name.GivenName.Contains(search) ||
                    name.FamilyName.Contains(search) ||
                    name.FullNameAr.Contains(search)
                );
            }

            // Select minimal fields and limit results
            var result = await query
                .Select((emp, name) => new EmployeeLookupDto
                {
                    EmployeeId = emp.EmployeeId,
                    EmployeeCode = emp.EmployeeCode,
                    FirstName = name.GivenName,
                    LastName = name.FamilyName,
                    FullNameAr = name.FullNameAr
                })
                .Take(100) // Limit to 100 results for performance
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Get employee ID by user ID (hr_employee_core.user_id → sys_user.user_id)
        /// الحصول على معرف الموظف من معرف المستخدم
        /// </summary>
        public async Task<long?> GetEmployeeIdByUserIdAsync(long userId)
        {
            var employee = await Context.Queryable<HrEmployeeCore>()
                .Where(e => e.UserId.HasValue && e.UserId.Value == userId)
                .FirstAsync();

            return employee?.EmployeeId;
        }
    }
}


