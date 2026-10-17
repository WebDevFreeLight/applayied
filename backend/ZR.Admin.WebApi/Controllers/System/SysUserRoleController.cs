using Microsoft.AspNetCore.Mvc;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// User Role Management
    /// </summary>
    [Route("system/userRole")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysUserRoleController : BaseController
    {
        private readonly ISysUserRoleService SysUserRoleService;
        private readonly ISysUserService UserService;

        public SysUserRoleController(
            ISysUserRoleService sysUserRoleService,
            ISysUserService userService)
        {
            SysUserRoleService = sysUserRoleService;
            UserService = userService;
        }

        /// <summary>
        /// Get assigned users by role number
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:roleusers:list")]
        public IActionResult GetList([FromQuery] RoleUserQueryDto roleUserQueryDto)
        {
            var list = SysUserRoleService.GetSysUsersByRoleId(roleUserQueryDto);

            return SUCCESS(list, TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Add role user
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ActionPermissionFilter(Permission = "system:roleusers:add")]
        [Log(Title = "Add Role User", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] RoleUsersCreateDto roleUsersCreateDto)
        {
            var response = SysUserRoleService.InsertRoleUser(roleUsersCreateDto);

            return SUCCESS(response);
        }

        /// <summary>
        /// Delete role user
        /// </summary>
        /// <param name="roleUsersCreateDto"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        [ActionPermissionFilter(Permission = "system:roleusers:remove")]
        [Log(Title = "Delete Role User", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromBody] RoleUsersCreateDto roleUsersCreateDto)
        {
            return SUCCESS(SysUserRoleService.DeleteRoleUserByUserIds(roleUsersCreateDto.RoleId, roleUsersCreateDto.UserIds));
        }

        /// <summary>
        /// Get unassigned user roles
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        [HttpGet("GetExcludeUsers")]
        public IActionResult GetExcludeUsers([FromQuery] RoleUserQueryDto roleUserQueryDto)
        {
            if (roleUserQueryDto.RoleId <= 0)
            {
                throw new CustomException(ResultCode.PARAM_ERROR, "roleId cannot be empty");
            }

            // Get unadded users
            var list = SysUserRoleService.GetExcludedSysUsersByRoleId(roleUserQueryDto);

            return SUCCESS(list, TIME_FORMAT_FULL);
        }
    }
}

