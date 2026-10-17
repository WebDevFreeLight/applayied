using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    public interface ISysUserRoleService : IBaseService<SysUserRole>
    {
        public int CountUserRoleByRoleId(long roleId);

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int DeleteUserRoleByUserId(long userId);

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int DeleteRoleUserByUserIds(long roleId, List<long> userIds);

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="sysUsers"></param>
        /// <returns></returns>
        public int AddUserRole(List<SysUserRole> sysUsers);

        /// <summary>
        /// ??????????id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<SysUser> GetSysUsersByRoleId(long roleId);

        /// <summary>
        /// ??????????id
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        public PagedInfo<SysUser> GetSysUsersByRoleId(RoleUserQueryDto roleUserQueryDto);

        /// <summary>
        /// ???????????????id
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        public PagedInfo<SysUser> GetExcludedSysUsersByRoleId(RoleUserQueryDto roleUserQueryDto);

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="roleIds"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int InsertUserRole(long[] roleIds, long userId);

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="roleUsersCreateDto"></param>
        /// <returns></returns>
        public int InsertRoleUser(RoleUsersCreateDto roleUsersCreateDto);
    }
}

