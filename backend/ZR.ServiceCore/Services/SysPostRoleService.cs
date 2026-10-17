using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// Position-Role mapping service
    /// Manages the relationship between positions and their default roles
    /// </summary>
    [AppService(ServiceType = typeof(ISysPostRoleService), ServiceLifetime = LifeTime.Transient)]
    public class SysPostRoleService : BaseService<SysPostRole>, ISysPostRoleService
    {
        private readonly ISysRoleService _roleService;

        public SysPostRoleService(ISysRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Get all roles assigned to a position
        /// </summary>
        public List<SysRole> GetRolesByPostId(long postId)
        {
            return Context.Queryable<SysPostRole, SysRole>((pr, r) => new JoinQueryInfos(
                    JoinType.Inner, pr.RoleId == r.RoleId))
                .Where((pr, r) => pr.PostId == postId && r.DelFlag == 0 && r.Status == 0)
                .Select((pr, r) => r)
                .ToList();
        }

        /// <summary>
        /// Get role IDs assigned to a position
        /// </summary>
        public long[] GetRoleIdsByPostId(long postId)
        {
            return Queryable()
                .Where(it => it.PostId == postId)
                .Select(it => it.RoleId)
                .ToArray();
        }

        /// <summary>
        /// Get all positions with their assigned roles
        /// </summary>
        public List<PostWithRolesDto> GetAllPostsWithRoles()
        {
            // Get all active positions
            var positions = Context.Queryable<SysPost>()
                .Where(p => p.Status == "0")
                .OrderBy(p => p.PostSort)
                .ToList();

            var result = new List<PostWithRolesDto>();

            foreach (var pos in positions)
            {
                var roles = GetRolesByPostId(pos.PostId);
                result.Add(new PostWithRolesDto
                {
                    PostId = pos.PostId,
                    PostCode = pos.PostCode,
                    PostName = pos.PostName,
                    Status = pos.Status,
                    RoleIds = roles.Select(r => r.RoleId).ToArray(),
                    Roles = roles
                });
            }

            return result;
        }

        /// <summary>
        /// Get position with its assigned roles by position ID
        /// </summary>
        public PostWithRolesDto GetPostWithRoles(long postId)
        {
            var position = Context.Queryable<SysPost>()
                .Where(p => p.PostId == postId)
                .First();

            if (position == null)
            {
                return null;
            }

            var roles = GetRolesByPostId(postId);

            return new PostWithRolesDto
            {
                PostId = position.PostId,
                PostCode = position.PostCode,
                PostName = position.PostName,
                Status = position.Status,
                RoleIds = roles.Select(r => r.RoleId).ToArray(),
                Roles = roles
            };
        }

        /// <summary>
        /// Assign roles to a position (replaces existing)
        /// </summary>
        public int UpdatePostRoles(long postId, long[] roleIds)
        {
            var result = UseTran(() =>
            {
                // Delete existing roles for this position
                Delete(it => it.PostId == postId);

                // Insert new roles
                if (roleIds != null && roleIds.Length > 0)
                {
                    var postRoles = roleIds.Select((roleId, index) => new SysPostRole
                    {
                        PostId = postId,
                        RoleId = roleId,
                        IsPrimary = index == 0 ? 1 : 0 // First role is primary
                    }).ToList();

                    InsertRange(postRoles);
                }
            });

            return result.IsSuccess ? 1 : 0;
        }

        /// <summary>
        /// Add a single role to a position
        /// </summary>
        public int AddPostRole(long postId, long roleId, int isPrimary = 0)
        {
            // Check if already exists
            var exists = GetFirst(it => it.PostId == postId && it.RoleId == roleId);
            if (exists != null)
            {
                return 0; // Already exists
            }

            var postRole = new SysPostRole
            {
                PostId = postId,
                RoleId = roleId,
                IsPrimary = isPrimary
            };

            return Insert(postRole);
        }

        /// <summary>
        /// Remove a role from a position
        /// </summary>
        public int RemovePostRole(long postId, long roleId)
        {
            return Delete(it => it.PostId == postId && it.RoleId == roleId) ? 1 : 0;
        }

        /// <summary>
        /// Remove all roles from a position
        /// </summary>
        public int DeletePostRoles(long postId)
        {
            return Delete(it => it.PostId == postId) ? 1 : 0;
        }

        /// <summary>
        /// Check if a position has any roles assigned
        /// </summary>
        public bool HasRoles(long postId)
        {
            return Count(it => it.PostId == postId) > 0;
        }

        /// <summary>
        /// Get positions that have a specific role assigned
        /// </summary>
        public List<SysPost> GetPostsByRoleId(long roleId)
        {
            return Context.Queryable<SysPostRole, SysPost>((pr, p) => new JoinQueryInfos(
                    JoinType.Inner, pr.PostId == p.PostId))
                .Where((pr, p) => pr.RoleId == roleId && p.Status == "0")
                .Select((pr, p) => p)
                .ToList();
        }

        /// <summary>
        /// Sync roles to all users who have the given position.
        /// Each user's roles become the union of roles from all their positions.
        /// </summary>
        public int SyncRolesToUsersForPost(long postId)
        {
            // Find all users who have this position
            var userIds = Context.Queryable<SysUserPost>()
                .Where(up => up.PostId == postId)
                .Select(up => up.UserId)
                .ToList();

            if (userIds == null || userIds.Count == 0)
                return 0;

            int syncedCount = 0;

            foreach (var userId in userIds)
            {
                // Get all positions for this user
                var userPostIds = Context.Queryable<SysUserPost>()
                    .Where(up => up.UserId == userId)
                    .Select(up => up.PostId)
                    .ToList();

                // Get the union of all roles from all user positions
                var positionRoleIds = Context.Queryable<SysPostRole>()
                    .Where(pr => userPostIds.Contains(pr.PostId))
                    .Select(pr => pr.RoleId)
                    .Distinct()
                    .ToList();

                if (positionRoleIds.Count == 0)
                    continue;

                var result = UseTran(() =>
                {
                    // Delete existing user roles
                    Context.Deleteable<SysUserRole>()
                        .Where(ur => ur.UserId == userId)
                        .ExecuteCommand();

                    // Insert new roles from positions
                    var newUserRoles = positionRoleIds.Select(roleId => new SysUserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    }).ToList();

                    Context.Insertable(newUserRoles).ExecuteCommand();
                });

                if (result.IsSuccess)
                    syncedCount++;
            }

            return syncedCount;
        }
    }
}
