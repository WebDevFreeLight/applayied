using Microsoft.AspNetCore.Mvc;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Position-Role Management
    /// Manages the mapping between positions and their default roles
    /// </summary>
    [Route("system/postRole")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysPostRoleController : BaseController
    {
        private readonly ISysPostRoleService _postRoleService;
        private readonly ISysPostService _postService;
        private readonly ISysRoleService _roleService;

        public SysPostRoleController(
            ISysPostRoleService postRoleService,
            ISysPostService postService,
            ISysRoleService roleService)
        {
            _postRoleService = postRoleService;
            _postService = postService;
            _roleService = roleService;
        }

        /// <summary>
        /// Get all positions with their assigned roles
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:postRole:list")]
        public IActionResult GetList()
        {
            var list = _postRoleService.GetAllPostsWithRoles();
            return SUCCESS(list);
        }

        /// <summary>
        /// Get a specific position with its assigned roles
        /// </summary>
        /// <param name="postId">Position ID</param>
        [HttpGet("{postId}")]
        [ActionPermissionFilter(Permission = "system:postRole:query")]
        public IActionResult GetByPostId(long postId)
        {
            var result = _postRoleService.GetPostWithRoles(postId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Position not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get roles for a specific position
        /// </summary>
        /// <param name="postId">Position ID</param>
        [HttpGet("{postId}/roles")]
        [ActionPermissionFilter(Permission = "system:postRole:query")]
        public IActionResult GetRolesByPostId(long postId)
        {
            var roles = _postRoleService.GetRolesByPostId(postId);
            return SUCCESS(roles);
        }

        /// <summary>
        /// Get role IDs for a specific position
        /// </summary>
        /// <param name="postId">Position ID</param>
        [HttpGet("{postId}/roleIds")]
        [ActionPermissionFilter(Permission = "system:postRole:query")]
        public IActionResult GetRoleIdsByPostId(long postId)
        {
            var roleIds = _postRoleService.GetRoleIdsByPostId(postId);
            return SUCCESS(roleIds);
        }

        /// <summary>
        /// Update roles for a position (replace all) and sync to users
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <param name="dto">DTO containing role IDs</param>
        [HttpPut("{postId}/roles")]
        [ActionPermissionFilter(Permission = "system:postRole:edit")]
        [Log(Title = "Position-Role Update", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePostRoles(long postId, [FromBody] SysPostRoleDto dto)
        {
            if (dto.RoleIds == null)
            {
                dto.RoleIds = Array.Empty<long>();
            }

            var result = _postRoleService.UpdatePostRoles(postId, dto.RoleIds);

            if (result > 0)
            {
                // Auto-sync: update roles for all users who have this position
                var syncedCount = _postRoleService.SyncRolesToUsersForPost(postId);
                return SUCCESS(new { updated = result, usersSynced = syncedCount },
                    $"Position roles updated. {syncedCount} user(s) synced.");
            }

            return ToResponse(result);
        }

        /// <summary>
        /// Add a role to a position
        /// </summary>
        [HttpPost("add")]
        [ActionPermissionFilter(Permission = "system:postRole:add")]
        [Log(Title = "Position-Role Add", BusinessType = BusinessType.INSERT)]
        public IActionResult AddPostRole([FromBody] SysPostRoleDto dto)
        {
            if (dto.PostId <= 0 || dto.RoleId <= 0)
            {
                return ToResponse(ResultCode.FAIL, "Position ID and Role ID are required");
            }

            var result = _postRoleService.AddPostRole(dto.PostId, dto.RoleId, dto.IsPrimary);
            return ToResponse(result);
        }

        /// <summary>
        /// Remove a role from a position
        /// </summary>
        [HttpDelete("{postId}/roles/{roleId}")]
        [ActionPermissionFilter(Permission = "system:postRole:remove")]
        [Log(Title = "Position-Role Remove", BusinessType = BusinessType.DELETE)]
        public IActionResult RemovePostRole(long postId, long roleId)
        {
            var result = _postRoleService.RemovePostRole(postId, roleId);
            return ToResponse(result);
        }

        /// <summary>
        /// Remove all roles from a position
        /// </summary>
        [HttpDelete("{postId}/roles")]
        [ActionPermissionFilter(Permission = "system:postRole:remove")]
        [Log(Title = "Position-Role Remove All", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePostRoles(long postId)
        {
            var result = _postRoleService.DeletePostRoles(postId);
            return ToResponse(result);
        }

        /// <summary>
        /// Get available positions (for dropdown)
        /// </summary>
        [HttpGet("positions")]
        public IActionResult GetPositions()
        {
            var positions = _postService.GetAll();
            return SUCCESS(positions);
        }

        /// <summary>
        /// Get available roles (for dropdown)
        /// </summary>
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleService.GetAll();
            return SUCCESS(roles);
        }

        /// <summary>
        /// Get positions that have a specific role assigned
        /// </summary>
        /// <param name="roleId">Role ID</param>
        [HttpGet("byRole/{roleId}")]
        [ActionPermissionFilter(Permission = "system:postRole:query")]
        public IActionResult GetPostsByRoleId(long roleId)
        {
            var positions = _postRoleService.GetPostsByRoleId(roleId);
            return SUCCESS(positions);
        }
    }
}
