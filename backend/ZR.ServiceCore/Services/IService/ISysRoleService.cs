using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    public interface ISysRoleService : IBaseService<SysRole>
    {
        /// <summary>
        /// Query role data by conditions with pagination
        /// </summary>
        /// <param name="role">Role information</param>
        /// <param name="pager"></param>
        /// <returns>Role data collection information</returns>
        public PagedInfo<SysRole> SelectRoleList(SysRole role, PagerInfo pager);

        /// <summary>
        /// Query all roles
        /// </summary>
        /// <returns></returns>
        public List<SysRole> SelectRoleAll();

        /// <summary>
        /// Query by user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysRole> SelectRolePermissionByUserId(long userId);

        /// <summary>
        /// Query role by role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role object information</returns>
        public SysRole SelectRoleById(long roleId);

        /// <summary>
        /// Query role by role key
        /// </summary>
        /// <param name="roleKey">Role key</param>
        /// <returns>Role object information</returns>
        public SysRole SelectRoleByKey(string roleKey);

        /// <summary>
        /// Query role by role key, bypassing data permission filters.
        /// Used by background/system services (e.g. notification dispatcher).
        /// </summary>
        public SysRole SelectRoleByKeyUnfiltered(string roleKey);

        /// <summary>
        /// Batch delete role information
        /// </summary>
        /// <param name="roleIds">Role IDs to be deleted</param>
        /// <returns></returns>
        public int DeleteRoleByRoleId(long[] roleIds);

        /// <summary>
        /// Change role permission status
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        public int UpdateRoleStatus(SysRole roleDto);

        /// <summary>
        /// Check if role permission is unique
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public string CheckRoleKeyUnique(SysRole sysRole);

        /// <summary>
        /// Check if role is allowed for operation
        /// </summary>
        /// <param name="role"></param>
        public void CheckRoleAllowed(SysRole role);

        /// <summary>
        /// Add and save role information
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public long InsertRole(SysRole sysRole);

        /// <summary>
        /// Delete role and menu associations by role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns></returns>
        public int DeleteRoleMenuByRoleId(long roleId);

        /// <summary>
        /// Authorize data scope
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        bool AuthDataScope(SysRoleDto role);
        #region Service


        /// <summary>
        /// Add role menu information
        /// </summary>
        /// <param name="sysRoleDto"></param>
        /// <returns></returns>
        public int InsertRoleMenu(SysRoleDto sysRoleDto);

        /// <summary>
        /// Check if user is administrator
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsAdmin(long userid);

        /// <summary>
        /// Get role menu ID collection
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<string> SelectUserRoleMenus(long roleId);
        List<long> SelectRoleMenuByRoleIds(long[] roleIds);
        /// <summary>
        /// Get user role list
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysRole> SelectUserRoleListByUserId(long userId);

        /// <summary>
        /// Get user permission collection
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<long> SelectUserRoles(long userId);

        /// <summary>
        /// Get user permission string collection
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> SelectUserRoleKeys(long userId);

        public List<string> SelectUserRoleNames(long userId);

        #endregion

        /// <summary>
        /// Update and save role information
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public int UpdateRole(SysRole sysRole);

        int UpdateSysRole(SysRole sysRole);
    }
}

