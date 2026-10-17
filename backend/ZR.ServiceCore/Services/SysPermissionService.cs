using Infrastructure;
using Infrastructure.Attribute;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ????
    /// </summary>
    [AppService(ServiceType = typeof(ISysPermissionService), ServiceLifetime = LifeTime.Transient)]
    public class SysPermissionService : ISysPermissionService
    {
        private readonly ISysRoleService SysRoleService;
        private readonly ISysMenuService SysMenuService;

        public SysPermissionService(
            ISysRoleService sysRoleService,
            ISysMenuService sysMenuService)
        {
            SysRoleService = sysRoleService;
            SysMenuService = sysMenuService;
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="user">????</param>
        /// <returns>??????</returns>
        public List<string> GetRolePermission(SysUserDto user)
        {
            List<string> roles = new();
            // ?????????
            if (user.IsAdmin)
            {
                roles.Add("admin");
            }
            else
            {
                roles.AddRange(SysRoleService.SelectUserRoleKeys(user.UserId));
            }
            return roles;
        }

        /// <summary>
        /// Get menu permissions for user
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Permission list</returns>
        public List<string> GetMenuPermission(SysUserDto user)
        {
            List<string> perms = new();

            // ✅ ADMIN FIX: Admin users get the super permission that bypasses all checks
            // This matches the behavior of the old Vue frontend (ZR.Vue/src/directive/permission/hasPermi.js)
            // When admin has "*:*:*", frontend will show all menu items without filtering
            if (user.IsAdmin)
            {
                perms.Add(GlobalConstant.AdminPerm);  // "*:*:*"
            }

            // ✅ FIX: Use SelectMenuPermsByUserId which returns ALL permissions
            // including button permissions (type F) - not just M,C,L menu types
            // This is the correct method for getting user permissions
            perms.AddRange(SysMenuService.SelectMenuPermsByUserId(user.UserId));

            // Remove nulls, empty strings, and duplicates
            perms = perms.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();

            return perms;
        }

        /// <summary>
        /// Recursively extract permissions from menu tree
        /// </summary>
        /// <param name="menus">Menu tree</param>
        /// <returns>Flat list of permission strings</returns>
        private List<string> ExtractPermissionsFromMenus(List<ZR.Model.System.SysMenu> menus)
        {
            List<string> permissions = new();

            if (menus == null || menus.Count == 0)
            {
                return permissions;
            }

            foreach (var menu in menus)
            {
                // Add this menu's permission (if it has one)
                if (!string.IsNullOrWhiteSpace(menu.Perms))
                {
                    permissions.Add(menu.Perms);
                }

                // Recursively extract from children
                if (menu.Children != null && menu.Children.Count > 0)
                {
                    permissions.AddRange(ExtractPermissionsFromMenus(menu.Children));
                }
            }

            return permissions;
        }
    }
}

