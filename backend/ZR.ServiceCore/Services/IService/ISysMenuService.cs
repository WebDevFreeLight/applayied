using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Model.System.Generate;
using ZR.Model.System.Vo;

namespace ZR.ServiceCore.Services
{
    public interface ISysMenuService : IBaseService<SysMenu>
    {
        //List<SysMenu> SelectMenuList(long userId);

        List<SysMenu> SelectMenuList(MenuQueryDto menu, long userId);
        /// <summary>All menus (no user scoping) — for role-permission assignment trees.</summary>
        List<SysMenu> SelectMenuList(MenuQueryDto menu);
        List<SysMenu> SelectTreeMenuList(MenuQueryDto menu, long userId);
        /// <summary>Full menu tree (no user scoping) — for the menu-management admin page.</summary>
        List<SysMenu> SelectTreeMenuList(MenuQueryDto menu);

        SysMenu GetMenuByMenuId(long menuId);
        List<SysMenu> GetMenusByMenuId(long menuId, long userId);
        long AddMenu(SysMenu menu);

        long EditMenu(SysMenu menu);

        int DeleteMenuById(long menuId);
        int DeleteAllMenuById(long menuId);

        string CheckMenuNameUnique(SysMenu menu);

        int ChangeSortMenu(MenuDto menuDto);

        bool HasChildByMenuId(long menuId);

        List<SysMenu> SelectMenuTreeByUserId(long userId);

        //List<SysMenu> SelectMenuPermsListByUserId(long userId);

        List<string> SelectMenuPermsByUserId(long userId);

        //bool CheckMenuExistRole(long menuId);

        List<RouterVo> BuildMenus(List<SysMenu> menus);

        List<TreeSelectVo> BuildMenuTreeSelect(List<SysMenu> menus);

        void AddSysMenu(GenTable genTableInfo, string permPrefix, bool showEdit, bool showExport, bool showImport);
        List<SysMenu> SelectTreeMenuListByRoles(MenuQueryDto menu, List<long> roles);
        List<RoleMenuExportDto> SelectRoleMenuListByRole(MenuQueryDto menu, int roleId);
        List<RouterVo> GetAppMenus(List<string> perms, int v);

        /// <summary>
        /// Menu add action with validation and role assignment
        /// </summary>
        long MenuAddAction(MenuDto menuDto, long userId, string createBy);

        /// <summary>
        /// Sync missing permissions from ActionPermissionFilter to menu system
        /// </summary>
        int SyncMissingPermissionsToMenus(string createBy = "system");
    }

    /// <summary>
    /// Role Menu
    /// </summary>
    public interface ISysRoleMenuService : IBaseService<SysRoleMenu>
    {
        bool CheckMenuExistRole(long menuId);
        /// <summary>
        /// Get menu ID by role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        List<SysRoleMenu> SelectRoleMenuByRoleId(long roleId);

        /// <summary>
        /// Get menus by all user roles
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        List<SysRoleMenu> SelectRoleMenuByRoleIds(long[] roleIds);

        /// <summary>
        /// Batch insert user menus
        /// </summary>
        /// <param name="sysRoleMenus"></param>
        /// <returns></returns>
        int AddRoleMenu(List<SysRoleMenu> sysRoleMenus);

        /// <summary>
        /// Delete role and menu association
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        int DeleteRoleMenuByRoleId(long roleId);

        /// <summary>
        /// Delete specific menus of role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="menuIds"></param>
        /// <returns></returns>
        bool DeleteRoleMenuByRoleIdMenuIds(long roleId, long[] menuIds);
    }
}

