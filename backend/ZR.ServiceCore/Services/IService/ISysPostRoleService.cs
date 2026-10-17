using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// Position-Role mapping service interface
    /// </summary>
    public interface ISysPostRoleService : IBaseService<SysPostRole>
    {
        /// <summary>
        /// Get all roles assigned to a position
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <returns>List of roles</returns>
        List<SysRole> GetRolesByPostId(long postId);

        /// <summary>
        /// Get role IDs assigned to a position
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <returns>Array of role IDs</returns>
        long[] GetRoleIdsByPostId(long postId);

        /// <summary>
        /// Get all positions with their assigned roles
        /// </summary>
        /// <returns>List of positions with roles</returns>
        List<PostWithRolesDto> GetAllPostsWithRoles();

        /// <summary>
        /// Get position with its assigned roles by position ID
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <returns>Position with roles</returns>
        PostWithRolesDto GetPostWithRoles(long postId);

        /// <summary>
        /// Assign roles to a position (replaces existing)
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <param name="roleIds">Array of role IDs to assign</param>
        /// <returns>Number of affected rows</returns>
        int UpdatePostRoles(long postId, long[] roleIds);

        /// <summary>
        /// Add a single role to a position
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="isPrimary">Is this the primary role</param>
        /// <returns>Number of affected rows</returns>
        int AddPostRole(long postId, long roleId, int isPrimary = 0);

        /// <summary>
        /// Remove a role from a position
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Number of affected rows</returns>
        int RemovePostRole(long postId, long roleId);

        /// <summary>
        /// Remove all roles from a position
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <returns>Number of affected rows</returns>
        int DeletePostRoles(long postId);

        /// <summary>
        /// Check if a position has any roles assigned
        /// </summary>
        /// <param name="postId">Position ID</param>
        /// <returns>True if position has roles</returns>
        bool HasRoles(long postId);

        /// <summary>
        /// Get positions that have a specific role assigned
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of positions</returns>
        List<SysPost> GetPostsByRoleId(long roleId);

        /// <summary>
        /// Sync roles to all users who have the given position.
        /// Each user's roles become the union of roles from all their positions.
        /// </summary>
        /// <param name="postId">Position ID whose users should be synced</param>
        /// <returns>Number of users synced</returns>
        int SyncRolesToUsersForPost(long postId);
    }
}
