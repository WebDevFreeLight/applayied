using Infrastructure.Attribute;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Repository;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ????
    /// </summary>
    [AppService(ServiceType = typeof(ISysUserRoleService), ServiceLifetime = LifeTime.Transient)]
    public class SysUserRoleService : BaseService<SysUserRole>, ISysUserRoleService
    {
        /// <summary>
        /// ????ID????????
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public int CountUserRoleByRoleId(long roleId)
        {
            return Count(it => it.RoleId == roleId);
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int DeleteUserRoleByUserId(long userId)
        {
            return Delete(it => it.UserId == userId) ? 1 : 0;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public int DeleteRoleUserByUserIds(long roleId, List<long> userIds)
        {
            return Delete(it => it.RoleId == roleId && userIds.Contains(it.UserId)) ? 1 : 0;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="sysUserRoles"></param>
        /// <returns></returns>
        public int AddUserRole(List<SysUserRole> sysUserRoles)
        {
            SysUserRole[] insertObjs = sysUserRoles.ToArray();
            int v = 0;
            
          var insrted=  InsertRange(sysUserRoles);
            if (insrted)
            {
                v=1;
                
            }


            return v;
        }

        /// <summary>
        /// ??????????id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<SysUser> GetSysUsersByRoleId(long roleId)
        {
            return Context.Queryable<SysUserRole, SysUser>((t1, u) => new JoinQueryInfos(
                   JoinType.Left, t1.UserId == u.UserId))
                .WithCache(60 * 10)
                .Where((t1, u) => t1.RoleId == roleId && u.DelFlag == 0)
                .Select((t1, u) => u)
                .ToList();
        }

        /// <summary>
        /// ??????????id
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        public PagedInfo<SysUser> GetSysUsersByRoleId(RoleUserQueryDto roleUserQueryDto)
        {
            var query = Context.Queryable<SysUserRole, SysUser>((t1, u) => new JoinQueryInfos(
                JoinType.Left, t1.UserId == u.UserId))
                .Where((t1, u) => t1.RoleId == roleUserQueryDto.RoleId && u.DelFlag == 0);
            if (!string.IsNullOrEmpty(roleUserQueryDto.UserName))
            {
                query = query.Where((t1, u) => u.UserName.Contains(roleUserQueryDto.UserName));
            }
            return query.Select((t1, u) => u).ToPage(roleUserQueryDto);
        }

        /// <summary>
        /// ???????????????id
        /// </summary>
        /// <param name="roleUserQueryDto"></param>
        /// <returns></returns>
        public PagedInfo<SysUser> GetExcludedSysUsersByRoleId(RoleUserQueryDto roleUserQueryDto)
        {
            var query = Context.Queryable<SysUser>()
                .Where(it => it.DelFlag == 0)
                .Where(it => SqlFunc.Subqueryable<SysUserRole>().Where(s => s.UserId == it.UserId && s.RoleId == roleUserQueryDto.RoleId).NotAny())
                .WhereIF(roleUserQueryDto.UserName.IsNotEmpty(), it => it.UserName.Contains(roleUserQueryDto.UserName));

            return query.ToPage(roleUserQueryDto);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="roleIds"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int InsertUserRole(long[] roleIds, long userId)
        {


            

            List<SysUserRole> userRoles = [];
            foreach (var item in roleIds)
            {
                userRoles.Add(new SysUserRole() { RoleId = item, UserId = userId });
            }

            return userRoles.Count > 0 ? AddUserRole(userRoles) : 0;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="roleUsersCreateDto"></param>
        /// <returns></returns>
        public int InsertRoleUser(RoleUsersCreateDto roleUsersCreateDto)
        {
            List<SysUserRole> userRoles = new();
            foreach (var item in roleUsersCreateDto.UserIds)
            {
                userRoles.Add(new SysUserRole() { RoleId = roleUsersCreateDto.RoleId, UserId = item });
            }

            return userRoles.Count > 0 ? AddUserRole(userRoles) : 0;
        }
    }
}

