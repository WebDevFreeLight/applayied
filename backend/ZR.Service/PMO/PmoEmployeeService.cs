using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.Repository;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// PMO employee directory. Thin management layer over the shared hr_employee_core table, scoped
    /// to what the PMO portal needs: a code, hire/termination dates, an active flag and an optional
    /// link to a portal user. The display name is resolved from sys_user via a single IN query.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoEmployeeService), ServiceLifetime = LifeTime.Transient)]
    public class PmoEmployeeService : BaseService<HrEmployeeCore>, IPmoEmployeeService
    {
        public PagedInfo<PmoEmployeeVo> GetList(PmoEmployeeQueryDto parm)
        {
            var exp = Expressionable.Create<HrEmployeeCore>();
            exp.AndIF(!string.IsNullOrWhiteSpace(parm.Keyword), e => e.EmployeeCode.Contains(parm.Keyword));
            exp.AndIF(parm.IsActive != null, e => e.IsActive == parm.IsActive);

            var page = Queryable().Where(exp.ToExpression())
                .OrderBy(e => e.EmployeeId, OrderByType.Desc).ToPage(parm);

            var names = ResolveNames(page.Result.Select(e => e.UserId));
            var vo = page.Result.Select(e => ToVo(e, names)).ToList();
            return new PagedInfo<PmoEmployeeVo> { Result = vo, TotalNum = page.TotalNum, PageIndex = page.PageIndex, PageSize = page.PageSize };
        }

        public PmoEmployeeVo GetInfo(long employeeId)
        {
            var e = Queryable().First(x => x.EmployeeId == employeeId);
            if (e == null) return null;
            return ToVo(e, ResolveNames(new List<long?> { e.UserId }));
        }

        public long AddEmployee(PmoEmployeeInputDto dto, string actor)
        {
            Validate(dto, isNew: true);

            var entity = new HrEmployeeCore
            {
                // hr_person is not used by the PMO portal; anchor person_id to the linked user (or a
                // generated id for an unlinked employee) so the NOT NULL column always has a stable value.
                PersonId = dto.UserId ?? SnowFlakeSingle.Instance.NextId(),
                UserId = dto.UserId,
                EmployeeCode = dto.EmployeeCode.Trim(),
                HireDate = dto.HireDate,
                TerminationDate = dto.TerminationDate,
                IsActive = dto.IsActive,
                Create_by = actor,
                Create_time = DateTime.Now
            };
            return Insertable(entity).ExecuteReturnBigIdentity();
        }

        public int UpdateEmployee(PmoEmployeeInputDto dto, string actor)
        {
            var existing = Queryable().First(x => x.EmployeeId == dto.EmployeeId)
                ?? throw new CustomException(ResultCode.FAIL, "Employee not found");
            Validate(dto, isNew: false);

            existing.UserId = dto.UserId;
            existing.EmployeeCode = dto.EmployeeCode.Trim();
            existing.HireDate = dto.HireDate;
            existing.TerminationDate = dto.TerminationDate;
            existing.IsActive = dto.IsActive;
            existing.Update_by = actor;
            existing.Update_time = DateTime.Now;
            return Update(existing, true, "Updated employee");
        }

        public int RemoveEmployee(long employeeId)
        {
            var existing = Queryable().First(x => x.EmployeeId == employeeId)
                ?? throw new CustomException(ResultCode.FAIL, "Employee not found");
            return Context.Deleteable<HrEmployeeCore>().In(existing.EmployeeId).ExecuteCommand();
        }

        public List<object> UnlinkedUsers(string search)
        {
            var linked = Queryable().Where(e => e.UserId != null).Select(e => e.UserId).ToList()
                .Where(id => id.HasValue).Select(id => id.Value).ToList();
            var q = Context.Queryable<SysUser>().Where(u => u.DelFlag == 0 && !linked.Contains(u.UserId));
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.UserName.Contains(search) || u.NickName.Contains(search));
            return q.Take(50).ToList()
                .Select(u => (object)new { id = u.UserId.ToString(), name = string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName })
                .ToList();
        }

        // ---- helpers ----

        private void Validate(PmoEmployeeInputDto dto, bool isNew)
        {
            if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
                throw new CustomException(ResultCode.FAIL, "Employee code is required");
            var code = dto.EmployeeCode.Trim();
            if (Queryable().Any(e => e.EmployeeCode == code && e.EmployeeId != dto.EmployeeId))
                throw new CustomException(ResultCode.FAIL, $"Employee code '{code}' is already in use");
            if (dto.UserId.HasValue && Queryable().Any(e => e.UserId == dto.UserId && e.EmployeeId != dto.EmployeeId))
                throw new CustomException(ResultCode.FAIL, "That user is already linked to another employee");
        }

        private Dictionary<long, string> ResolveNames(IEnumerable<long?> userIds)
        {
            var ids = userIds.Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<long, string>();
            return Context.Queryable<SysUser>().Where(u => ids.Contains(u.UserId))
                .Select(u => new { u.UserId, u.NickName, u.UserName }).ToList()
                .ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
        }

        private static PmoEmployeeVo ToVo(HrEmployeeCore e, Dictionary<long, string> names) => new()
        {
            EmployeeId = e.EmployeeId,
            UserId = e.UserId,
            UserName = e.UserId.HasValue && names.TryGetValue(e.UserId.Value, out var n) ? n : null,
            EmployeeCode = e.EmployeeCode,
            HireDate = e.HireDate,
            TerminationDate = e.TerminationDate,
            IsActive = e.IsActive,
        };
    }
}
