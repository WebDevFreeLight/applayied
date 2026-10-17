using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Role Information
    /// </summary>
    [Route("system/role")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysRoleController : BaseController
    {
        private readonly ISysRoleService sysRoleService;
        private readonly ISysMenuService sysMenuService;

        public SysRoleController(
            ISysRoleService sysRoleService,
            ISysMenuService sysMenuService)
        {
            this.sysRoleService = sysRoleService;
            this.sysMenuService = sysMenuService;
        }

        /// <summary>
        /// Get System Role Management
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:role:list")]
        [HttpGet("list")]
        public IActionResult List([FromQuery] SysRole role, [FromQuery] PagerInfo pager)
        {
            var list = sysRoleService.SelectRoleList(role, pager);

            return SUCCESS(list, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Get detailed information by role ID
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}")]
        public IActionResult GetInfo(long roleId = 0)
        {
            var info = sysRoleService.SelectRoleById(roleId);

            return SUCCESS(info, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Add role
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "system:role:add")]
        [Log(Title = "Role Management", BusinessType = BusinessType.INSERT)]
        [Route("edit")]
        public IActionResult RoleAdd([FromBody] SysRoleDto dto)
        {
            if (dto == null) return ToResponse(ApiResult.Error(101, "Request parameters are invalid"));
            SysRole sysRoleDto = dto.Adapt<SysRole>();
            if (UserConstants.NOT_UNIQUE.Equals(sysRoleService.CheckRoleKeyUnique(sysRoleDto)))
            {
                return ToResponse(ApiResult.Error((int)ResultCode.CUSTOM_ERROR, $"Add role '{sysRoleDto.RoleName}' failed, role permission already exists"));
            }

            sysRoleDto.Create_by = HttpContext.GetName();
            long roleId = sysRoleService.InsertRole(sysRoleDto);

            return ToResponse(roleId);
        }

        /// <summary>
        /// Modify role
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:role:edit")]
        [Log(Title = "Role Management", BusinessType = BusinessType.UPDATE)]
        [Route("edit")]
        public IActionResult RoleEdit([FromBody] SysRoleDto dto)
        {
            if (dto == null || dto.RoleId <= 0 || string.IsNullOrEmpty(dto.RoleKey))
            {
                return ToResponse(ApiResult.Error(101, "Request parameters are invalid"));
            }
            SysRole sysRoleDto = dto.Adapt<SysRole>();
            sysRoleService.CheckRoleAllowed(sysRoleDto);
            var info = sysRoleService.SelectRoleById(sysRoleDto.RoleId);
            if (info != null && info.RoleKey != sysRoleDto.RoleKey)
            {
                if (UserConstants.NOT_UNIQUE.Equals(sysRoleService.CheckRoleKeyUnique(sysRoleDto)))
                {
                    return ToResponse(ApiResult.Error($"Edit role '{sysRoleDto.RoleName}' failed, role permission already exists"));
                }
            }
            sysRoleDto.Update_by = HttpContext.GetName();
            int upResult = sysRoleService.UpdateRole(sysRoleDto);
            if (upResult > 0)
            {
                return SUCCESS(upResult);
            }
            return ToResponse(ApiResult.Error($"Modify role '{sysRoleDto.RoleName}' failed, please contact administrator"));
        }

        /// <summary>
        /// Assign menus based on role
        /// </summary>
        /// <param name="sysRoleDto"></param>
        /// <returns></returns>
        [HttpPut("dataScope")]
        [ActionPermissionFilter(Permission = "system:role:authorize")]
        [Log(Title = "Role Management", BusinessType = BusinessType.UPDATE)]
        public IActionResult DataScope([FromBody] SysRoleDto sysRoleDto)
        {
            if (sysRoleDto == null || sysRoleDto.RoleId <= 0) return ToResponse(ApiResult.Error(101, "Request parameters are invalid"));
            SysRole sysRole = sysRoleDto.Adapt<SysRole>();
            sysRoleDto.Create_by = HttpContext.GetName();
            sysRoleService.CheckRoleAllowed(sysRole);

            bool result = sysRoleService.AuthDataScope(sysRoleDto);

            return SUCCESS(result);
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpDelete("{roleId}")]
        [Log(Title = "Role Management", BusinessType = BusinessType.DELETE)]
        [ActionPermissionFilter(Permission = "system:role:remove")]
        public IActionResult Remove(string roleId)
        {
            long[] roleIds = Tools.SpitLongArrary(roleId);
            int result = sysRoleService.DeleteRoleByRoleId(roleIds);

            return ToResponse(result);
        }

        /// <summary>
        /// Modify role status
        /// </summary>
        /// <param name="roleDto">Role object</param>
        /// <returns></returns>
        [HttpPut("changeStatus")]
        [Log(Title = "Modify Role Status", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:role:edit")]
        public IActionResult ChangeStatus([FromBody] SysRole roleDto)
        {
            sysRoleService.CheckRoleAllowed(roleDto);
            int result = sysRoleService.UpdateRoleStatus(roleDto);

            return ToResponse(result);
        }

        /// <summary>
        /// Export roles
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Export Roles")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:role:export")]
        public IActionResult Export()
        {
            var list = sysRoleService.SelectRoleAll();

            string sFileName = ExportExcel(list, "sysrole", "Roles");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        /// <summary>
        /// Export role menus
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "Export Role Menus")]
        [HttpGet("exportRoleMenu")]
        [ActionPermissionFilter(Permission = "system:role:export")]
        public IActionResult ExportRoleMenu(int roleId)
        {
            MenuQueryDto dto = new() { Status = "0", MenuTypeIds = "M,C,F" };

            var list = sysMenuService.SelectRoleMenuListByRole(dto, roleId);

            var result = ExportExcelMini(list, roleId.ToString(), "Role Menus");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

