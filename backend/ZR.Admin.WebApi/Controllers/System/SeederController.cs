using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZR.Admin.WebApi.Filters;
using ZR.ServiceCore.Seeders;
using IOFile = System.IO.File;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Database Seeder Controller
    /// Executes database seeders for menu, permissions, and initial data
    /// </summary>
    [Route("api/system/seeder")]
    [ApiController]
    public class SeederController : BaseController
    {
        private readonly ISqlSugarClient _db;
        private readonly ISysRoleMenuService _roleMenuService;

        public SeederController(ISqlSugarClient db, ISysRoleMenuService roleMenuService)
        {
            _db = db;
            _roleMenuService = roleMenuService;
        }

        /// <summary>
        /// Seed menu data from JSON seeder files
        /// </summary>
        /// <param name="truncateFirst">Whether to delete existing menus before seeding</param>
        /// <returns></returns>
        [HttpPost("menu")]
        [ActionPermissionFilter(Permission = "system:seeder:menu")]
        public async Task<IActionResult> SeedMenus([FromQuery] bool truncateFirst = false)
        {
            try
            {
                var seeder = new MenuSeeder(_db);
                var result = await seeder.SeedAsync(truncateFirst);

                if (result.Success)
                {
                    return SUCCESS(result, result.Message);
                }
                else
                {
                    return ToResponse(ResultCode.FAIL, result.Message);
                }
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Seeding failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all permissions from seeder file
        /// </summary>
        /// <returns></returns>
        [HttpGet("permissions")]
        [ActionPermissionFilter(Permission = "system:seeder:view")]
        public async Task<IActionResult> GetPermissions()
        {
            try
            {
                var seeder = new MenuSeeder(_db);
                var permissions = await seeder.GetPermissionsAsync();
                return SUCCESS(permissions);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to load permissions: {ex.Message}");
            }
        }

        /// <summary>
        /// Get mapping review data
        /// </summary>
        /// <returns></returns>
        [HttpGet("mapping-review")]
        [ActionPermissionFilter(Permission = "system:seeder:view")]
        public async Task<IActionResult> GetMappingReview()
        {
            try
            {
                var seeder = new MenuSeeder(_db);
                var mapping = await seeder.GetMappingReviewAsync();
                return SUCCESS(mapping);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to load mapping review: {ex.Message}");
            }
        }

        /// <summary>
        /// Get seeder status and statistics
        /// </summary>
        /// <returns></returns>
        [HttpGet("status")]
        [ActionPermissionFilter(Permission = "system:seeder:view")]
        public async Task<IActionResult> GetSeederStatus()
        {
            try
            {
                var menuCount = await _db.Queryable<ZR.Model.System.SysMenu>().CountAsync();
                var seederPath = Path.Combine(AppContext.BaseDirectory, "Database", "Seeders");

                var menusFileExists = IOFile.Exists(Path.Combine(seederPath, "menus.seeder.json"));
                var permissionsFileExists = IOFile.Exists(Path.Combine(seederPath, "permissions.seeder.json"));
                var mappingFileExists = IOFile.Exists(Path.Combine(seederPath, "mapping.review.json"));

                return SUCCESS(new
                {
                    currentMenuCount = menuCount,
                    seederFilesExist = new
                    {
                        menus = menusFileExists,
                        permissions = permissionsFileExists,
                        mapping = mappingFileExists
                    },
                    seederPath = seederPath,
                    ready = menusFileExists && permissionsFileExists && mappingFileExists
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get status: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all menu assignments for a role
        /// </summary>
        /// <param name="roleId">Role ID to clear menus for</param>
        /// <returns></returns>
        [HttpDelete("role/{roleId}/menus")]
        [ActionPermissionFilter(Permission = "system:seeder:manage")]
        public IActionResult ClearRoleMenus(long roleId)
        {
            try
            {
                var deletedCount = _db.Deleteable<SysRoleMenu>()
                    .Where(rm => rm.Role_id == roleId)
                    .ExecuteCommand();

                return SUCCESS(new
                {
                    deletedCount,
                    message = $"Cleared {deletedCount} menu assignment(s) from role {roleId}"
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to clear role menus: {ex.Message}");
            }
        }

        /// <summary>
        /// Assign multiple menus to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="menuIds">Array of menu IDs to assign</param>
        /// <returns></returns>
        [HttpPost("role/{roleId}/menus")]
        [ActionPermissionFilter(Permission = "system:seeder:manage")]
        public IActionResult AssignMenusToRole(long roleId, [FromBody] long[] menuIds)
        {
            try
            {
                if (menuIds == null || menuIds.Length == 0)
                {
                    return ToResponse(ResultCode.FAIL, "No menu IDs provided");
                }

                // Create role-menu assignments
                var roleMenus = menuIds.Select(menuId => new SysRoleMenu
                {
                    Role_id = roleId,
                    Menu_id = menuId
                }).ToList();

                var insertedCount = _db.Insertable(roleMenus).ExecuteCommand();

                return SUCCESS(new
                {
                    insertedCount,
                    message = $"Assigned {insertedCount} menu(s) to role {roleId}"
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to assign menus to role: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear and reassign menus for a role (combines clear + assign in one operation)
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <param name="menuIds">Array of menu IDs to assign</param>
        /// <returns></returns>
        [HttpPut("role/{roleId}/menus")]
        [ActionPermissionFilter(Permission = "system:seeder:manage")]
        public IActionResult ReplaceRoleMenus(long roleId, [FromBody] long[] menuIds)
        {
            try
            {
                // Start transaction
                _db.Ado.BeginTran();

                // Clear existing assignments
                var deletedCount = _db.Deleteable<SysRoleMenu>()
                    .Where(rm => rm.Role_id == roleId)
                    .ExecuteCommand();

                int insertedCount = 0;
                if (menuIds != null && menuIds.Length > 0)
                {
                    // Create new assignments
                    var roleMenus = menuIds.Select(menuId => new SysRoleMenu
                    {
                        Role_id = roleId,
                        Menu_id = menuId
                    }).ToList();

                    insertedCount = _db.Insertable(roleMenus).ExecuteCommand();
                }

                _db.Ado.CommitTran();

                return SUCCESS(new
                {
                    deletedCount,
                    insertedCount,
                    message = $"Replaced role menus: removed {deletedCount}, added {insertedCount}"
                });
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                return ToResponse(ResultCode.FAIL, $"Failed to replace role menus: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all menus for testing/debugging
        /// </summary>
        /// <returns></returns>
        [HttpGet("menus/all")]
        [ActionPermissionFilter(Permission = "system:seeder:view")]
        public IActionResult GetAllMenus()
        {
            try
            {
                var menus = _db.Queryable<SysMenu>()
                    .OrderBy(m => m.OrderNum)
                    .Select(m => new
                    {
                        m.MenuId,
                        m.MenuName,
                        m.ParentId,
                        m.Perms,
                        m.MenuType,
                        m.Visible,
                        m.Status
                    })
                    .ToList();

                return SUCCESS(menus);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get menus: {ex.Message}");
            }
        }

        /// <summary>
        /// Assign ALL existing menus to admin role (role_id = 1)
        /// Use this to fix the case where menus exist but are not visible in the UI
        /// because sys_role_menu is empty
        /// </summary>
        /// <returns></returns>
        [HttpPost("assign-all-to-admin")]
        [ActionPermissionFilter(Permission = "system:seeder:manage")]
        public async Task<IActionResult> AssignAllMenusToAdminRole()
        {
            try
            {
                var seeder = new MenuSeeder(_db);
                var result = await seeder.AssignAllMenusToAdminRoleAsync();

                if (result.Success)
                {
                    return SUCCESS(result, result.Message);
                }
                else
                {
                    return ToResponse(ResultCode.FAIL, result.Message);
                }
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to assign menus to admin role: {ex.Message}");
            }
        }

        /// <summary>
        /// Get role-menu assignment statistics
        /// </summary>
        /// <returns></returns>
        [HttpGet("role-menu-stats")]
        [ActionPermissionFilter(Permission = "system:seeder:view")]
        public IActionResult GetRoleMenuStats()
        {
            try
            {
                var totalMenus = _db.Queryable<SysMenu>().Count();
                var totalRoleMenus = _db.Queryable<SysRoleMenu>().Count();

                var roleStats = _db.Queryable<SysRoleMenu>()
                    .GroupBy(rm => rm.Role_id)
                    .Select(rm => new { RoleId = rm.Role_id, MenuCount = SqlFunc.AggregateCount(rm.Menu_id) })
                    .ToList();

                return SUCCESS(new
                {
                    totalMenus,
                    totalRoleMenuAssignments = totalRoleMenus,
                    roleStats,
                    menusWithoutRoleAssignment = totalMenus - (roleStats.Sum(r => r.MenuCount) > 0 ? roleStats.Max(r => r.MenuCount) : 0)
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get role-menu stats: {ex.Message}");
            }
        }
    }
}
