using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>Reference-data lookups for pickers and dashboard filters.</summary>
    [AppService(ServiceType = typeof(IPmoLookupService), ServiceLifetime = LifeTime.Transient)]
    public class PmoLookupService : BaseService<PmoProject>, IPmoLookupService
    {
        public List<PmoLookupItem> Departments() =>
            Context.Queryable<SysDept>().Where(d => d.DelFlag == 0)
                .OrderBy(d => d.DeptName)
                .Select(d => new PmoLookupItem { Id = d.DeptId.ToString(), Name = d.DeptName }).ToList();

        public List<PmoLookupItem> Users() =>
            Context.Queryable<SysUser>().Where(u => u.DelFlag == 0 && u.Status == 0)
                .OrderBy(u => u.NickName)
                .ToList()
                .Select(u => new PmoLookupItem
                {
                    Id = u.UserId.ToString(),
                    Name = string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName
                }).ToList();

        public List<PmoLookupItem> Managers()
        {
            // Managers surfaced to the dashboard = distinct current project owners.
            var ownerIds = Context.Queryable<PmoProject>().Select(p => p.OwnerUserId).Distinct().ToList();
            return Context.Queryable<SysUser>().Where(u => ownerIds.Contains(u.UserId)).ToList()
                .Select(u => new PmoLookupItem
                {
                    Id = u.UserId.ToString(),
                    Name = string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName
                }).OrderBy(x => x.Name).ToList();
        }

        public List<PmoLookupItem> Projects() =>
            Context.Queryable<PmoProject>().OrderBy(p => p.Name)
                .Select(p => new PmoLookupItem { Id = p.ProjectId.ToString(), Name = p.Name }).ToList();
    }
}
