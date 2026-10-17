using Microsoft.AspNetCore.Mvc;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// System Menu
    /// </summary>
    [Route("/system/menu")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysMenuController : BaseController
    {
        private readonly ISysRoleService sysRoleService;
        private readonly ISysMenuService sysMenuService;
        private readonly ISysRoleMenuService sysRoleMenuService;

        public SysMenuController(
            ISysRoleService sysRoleService,
            ISysMenuService sysMenuService,
            ISysRoleMenuService sysRoleMenuService)
        {
            this.sysRoleService = sysRoleService;
            this.sysMenuService = sysMenuService;
            this.sysRoleMenuService = sysRoleMenuService;
        }

        /// <summary>
        /// Get menu tree list
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:menu:list")]
        [HttpGet("treelist")]
        public IActionResult TreeMenuList([FromQuery] MenuQueryDto menu)
        {
            long userId = HttpContext.GetUId();
            return SUCCESS(sysMenuService.SelectTreeMenuList(menu, userId), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Get menu list
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:menu:list")]
        [HttpGet("list")]
        public IActionResult MenuList([FromQuery] MenuQueryDto menu)
        {
            long userId = HttpContext.GetUId();
            return SUCCESS(sysMenuService.SelectMenuList(menu, userId), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Get detailed information by menu number
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpGet("{menuId}")]
        [ActionPermissionFilter(Permission = "system:menu:query")]
        public IActionResult GetMenuInfo(long menuId = 0)
        {
            return SUCCESS(sysMenuService.GetMenuByMenuId(menuId), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Get menu list by menu number, first entry to menu management
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpGet("list/{menuId}")]
        [ActionPermissionFilter(Permission = "system:menu:query")]
        public IActionResult GetMenuList(long menuId = 0)
        {
            long userId = HttpContext.GetUId();
            return SUCCESS(sysMenuService.GetMenusByMenuId(menuId, userId), "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Get role menu information
        /// Load corresponding role menu list tree
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>        
        [HttpGet("roleMenuTreeselect/{roleId}")]
        public IActionResult RoleMenuTreeselect(long roleId)
        {
            long userId = HttpContext.GetUId();
            var menus = sysMenuService.SelectMenuList(new MenuQueryDto(), userId);
            var checkedKeys = sysRoleService.SelectUserRoleMenus(roleId);
            return SUCCESS(new
            {
                checkedKeys,
                menus = sysMenuService.BuildMenuTreeSelect(menus),
            });
        }

        /// <summary>
        /// Modify menu
        /// </summary>
        /// <param name="menuDto"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        [Log(Title = "Menu Management", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:menu:edit")]
        public IActionResult MenuEdit([FromBody] MenuDto menuDto)
        {
            if (menuDto == null) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            //if (UserConstants.NOT_UNIQUE.Equals(sysMenuService.CheckMenuNameUnique(MenuDto)))
            //{
            //    return ToResponse(ApiResult.Error($"Modify menu '{MenuDto.menuName}' failed, menu name already exists"));
            //}
            var config = new TypeAdapterConfig();
            //Mapping rules
            config.ForType<SysMenu, MenuDto>()
                .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);//Ignore case sensitivity of field names;//Ignore all fields except the above configuration

            var modal = menuDto.Adapt<SysMenu>(config).ToUpdate(HttpContext);
            if (UserConstants.YES_FRAME.Equals(modal.IsFrame) && (!modal.Path.StartsWith("http") && !modal.Path.StartsWith("/link")))
            {
                return ToResponse(ApiResult.Error($"Modify menu '{modal.MenuName}' failed, address must start with http(s)://"));
            }
            if (modal.MenuId.Equals(modal.ParentId))
            {
                return ToResponse(ApiResult.Error($"Modify menu '{modal.MenuName}' failed, parent menu cannot select itself"));
            }
            modal.Update_by = HttpContext.GetName();
            long result = sysMenuService.EditMenu(modal);

            return ToResponse(result);
        }

        /// <summary>
        /// Add menu
        /// </summary>
        /// <param name="menuDto"></param>
        /// <returns></returns>
        [HttpPut("add")]
        [Log(Title = "Menu Management", BusinessType = BusinessType.INSERT)]
        [ActionPermissionFilter(Permission = "system:menu:add")]
        public IActionResult MenuAdd([FromBody] MenuDto menuDto)
        {
            try
            {
                var uid = HttpContext.GetUId();
                var createBy = HttpContext.GetName();

                long result = sysMenuService.MenuAddAction(menuDto, uid, createBy);

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error(ex.Message));
            }
        }

        /// <summary>
        /// Menu delete
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpDelete("{menuId}")]
        [Log(Title = "Menu Management", BusinessType = BusinessType.DELETE)]
        [ActionPermissionFilter(Permission = "system:menu:remove")]
        public IActionResult Remove(long menuId = 0)
        {
            if (sysMenuService.HasChildByMenuId(menuId))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Sub-menus exist, deletion not allowed");
            }
            if (sysRoleMenuService.CheckMenuExistRole(menuId))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Menu already assigned, deletion not allowed");
            }
            int result = sysMenuService.DeleteMenuById(menuId);

            return ToResponse(result);
        }
        /// <summary>
        /// Menu delete
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        [HttpDelete("deleteAll/{menuId}")]
        [Log(Title = "Menu Management", BusinessType = BusinessType.DELETE)]
        [ActionPermissionFilter(Permission = "system:menu:remove")]
        public IActionResult RemoveAll(long menuId = 0)
        {
            int result = sysMenuService.DeleteAllMenuById(menuId);

            return ToResponse(result);
        }

        /// <summary>
        /// Save sort order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:menu:update")]
        [HttpGet("ChangeSort")]
        [Log(Title = "Save Sort Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeSort(long id = 0, int value = 0)
        {
            MenuDto MenuDto = new()
            {
                MenuId = id,
                OrderNum = value
            };
            if (MenuDto == null) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }

            int result = sysMenuService.ChangeSortMenu(MenuDto);
            return ToResponse(result);
        }

        /// <summary>
        /// Scan all ActionPermissionFilter attributes and sync missing permissions to menu system
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:menu:sync")]
        [HttpPost("SyncPermissions")]
        [Log(Title = "Sync Permissions", BusinessType = BusinessType.INSERT)]
        public IActionResult SyncPermissions()
        {
            try
            {


                string createBy = HttpContext.GetName();
                int addedCount = sysMenuService.SyncMissingPermissionsToMenus(createBy);

                return SUCCESS(new
                {
                    addedCount = addedCount,
                    message = $"Successfully synced {addedCount} missing permission(s) to menu system"
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to sync permissions: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get all ActionPermissionFilter attributes with detailed information
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:menu:query")]
        [HttpGet("GetAllPermissions")]
        public IActionResult GetAllPermissions()
        {
            try
            {
                var filterInfos = ZR.Admin.WebApi.Extensions.ActionPermissionFilterInfo.GetAllActionPermissionFiltersWithInfo();

                var result = filterInfos.Select(f => new
                {
                    controllerName = f.ControllerName,
                    methodName = f.MethodName,
                    fullName = f.FullName,
                    permission = f.Filter.Permission,
                    rolePermi = f.Filter.RolePermi,
                    isClassLevel = f.IsClassLevel,
                    individualPermissions = f.IndividualPermissions
                }).ToList();

                return SUCCESS(new
                {
                    total = result.Count,
                    filters = result
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Failed to get permissions: {ex.Message}"));
            }
        }
    }
}

