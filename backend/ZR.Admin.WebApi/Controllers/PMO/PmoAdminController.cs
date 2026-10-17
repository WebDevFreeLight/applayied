using System.IO;
using System.Linq;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Service.PMO;
using ZR.ServiceCore.Services;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>Admin-only PMO operations (data seeding / setup). Guarded server-side by role.</summary>
    [Route("pmo/admin")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoAdminController : PmoBaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public PmoAdminController(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        /// <summary>
        /// Import the reproducible synthetic data package into the database (idempotent, transactional).
        /// Administrator only — a Contributor/PM calling this directly is refused by the role filter.
        /// </summary>
        [HttpPost("seed")]
        [PmoAuthorize(PmoRoleKeys.Admin)]
        public IActionResult Seed()
        {
            var csvDir = _config["PmoSeedPath"];
            if (string.IsNullOrWhiteSpace(csvDir))
                csvDir = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "..", "data", "csv"));

            var log = new PmoDataSeeder().Seed(csvDir);
            return SUCCESS(new { csvDir, log });
        }

        /// <summary>
        /// One-click RBAC bootstrap (PMO admin only):
        ///   1) scans every [ActionPermissionFilter] and saves any missing permission as a sys_menu row
        ///      (so a newly added route's permission is persisted to the database), then
        ///   2) grants every menu/permission to the pmo_admin role so the admin console is usable.
        /// Re-login as pmo.admin afterwards to refresh the cached permission set.
        /// </summary>
        [HttpPost("setup-rbac")]
        [PmoAuthorize(PmoRoleKeys.Admin)]
        public IActionResult SetupRbac(
            [FromServices] ISysMenuService menuService,
            [FromServices] ISysRoleService roleService)
        {
            // 1) persist permissions declared on controllers that aren't in the menu table yet
            int permissionsAdded = menuService.SyncMissingPermissionsToMenus("pmo-bootstrap");

            // 2) grant EVERY menu/permission to the pmo_admin role (resolve id by key — RoleId is identity)
            var role = roleService.SelectRoleByKey(PmoRoleKeys.Admin)
                ?? throw new CustomException(ResultCode.FAIL, "pmo_admin role not found — run /pmo/admin/seed first");
            var db = SqlSugar.IOC.DbScoped.SugarScope.GetConnectionScope(0);
            var allMenuIds = db.Queryable<SysMenu>().Select(m => m.MenuId).ToList();
            roleService.DeleteRoleMenuByRoleId(role.RoleId);
            roleService.InsertRoleMenu(new SysRoleDto { RoleId = role.RoleId, MenuIds = allMenuIds });

            // Invalidate the cached permission set for every user holding the pmo_admin role so the
            // new grants take effect immediately (no restart / re-login needed).
            var affectedUserIds = db.Queryable<SysUserRole>()
                .Where(x => x.RoleId == role.RoleId).Select(x => x.UserId).ToList();
            foreach (var uid in affectedUserIds)
                CacheService.RemoveUserPerms(GlobalConstant.UserPermKEY + uid);

            return SUCCESS(new
            {
                permissionsAdded,
                menusGrantedToPmoAdmin = allMenuIds.Count,
                roleId = role.RoleId,
                affectedUsers = affectedUserIds.Count,
                note = "Permissions refreshed; pmo_admin members now have full access."
            });
        }
    }
}
