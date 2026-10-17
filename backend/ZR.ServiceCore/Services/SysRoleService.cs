using Infrastructure;
using Infrastructure.Attribute;
using System.Collections;
using ZR.Model;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Repository;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// Role
    /// </summary>
    [AppService(ServiceType = typeof(ISysRoleService), ServiceLifetime = LifeTime.Transient)]
    public class SysRoleService : BaseService<SysRole>, ISysRoleService
    {
        private ISysUserRoleService SysUserRoleService;
        private ISysDeptService DeptService;
        private ISysRoleMenuService RoleMenuService;
        public SysRoleService(
            ISysUserRoleService sysUserRoleService,
            ISysDeptService deptService,
            ISysRoleMenuService roleMenuService)
        {
            SysUserRoleService = sysUserRoleService;
            DeptService = deptService;
            RoleMenuService = roleMenuService;
        }

        /// <summary>
        /// Query role data by conditions with pagination
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <param name="pager">Pagination information</param>
        /// <returns>Role data collection information</returns>
        public PagedInfo<SysRole> SelectRoleList(SysRole sysRole, PagerInfo pager)
        {
            var exp = Expressionable.Create<SysRole>();
            exp.And(role => role.DelFlag == 0);
            exp.AndIF(!string.IsNullOrEmpty(sysRole.RoleName), role => role.RoleName.Contains(sysRole.RoleName));
            exp.AndIF(sysRole.Status != -1, role => role.Status == sysRole.Status);
            exp.AndIF(!string.IsNullOrEmpty(sysRole.RoleKey), role => role.RoleKey == sysRole.RoleKey);

            var query = Queryable()
                .Where(exp.ToExpression())
                .OrderBy(x => x.RoleSort)
                .Select((role) => new SysRole
                {
                    UserNum = SqlFunc.Subqueryable<SysUserRole>().Where(f => f.RoleId == role.RoleId).Count()
                }, true);

            return query.ToPage(pager);
        }

        /// <summary>
        /// Query all roles
        /// </summary>
        /// <returns></returns>
        public List<SysRole> SelectRoleAll()
        {
            return Queryable()
                .Where(role => role.DelFlag == 0)
                .OrderBy(role => role.RoleSort)
                .ToList();
        }

        /// <summary>
        /// Query by user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysRole> SelectRolePermissionByUserId(long userId)
        {
            return Queryable()
                .Where(role => role.DelFlag == 0)
                .Where(it => SqlFunc.Subqueryable<SysUserRole>().Where(s => s.UserId == userId).Any())
                .OrderBy(role => role.RoleSort)
                .ToList();
        }

        /// <summary>
        /// Query role by role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role object information</returns>
        public SysRole SelectRoleById(long roleId)
        {
            return GetId(roleId);
        }

        /// <summary>
        /// Query role by role key
        /// </summary>
        /// <param name="roleKey">Role key</param>
        /// <returns>Role object information</returns>
        public SysRole SelectRoleByKey(string roleKey)
        {
            return GetFirst(r => r.RoleKey == roleKey && r.DelFlag == 0);
        }

        /// <summary>
        /// Query role by key, bypassing global data permission filters.
        /// </summary>
        public SysRole SelectRoleByKeyUnfiltered(string roleKey)
        {
            return Context.Queryable<SysRole>().ClearFilter()
                .First(r => r.RoleKey == roleKey && r.DelFlag == 0);
        }

        /// <summary>
        /// Batch delete role information
        /// </summary>
        /// <param name="roleIds">Role IDs to be deleted</param>
        /// <returns></returns>
        public int DeleteRoleByRoleId(long[] roleIds)
        {
            foreach (var item in roleIds)
            {
                CheckRoleAllowed(new SysRole(item));
                SysRole role = SelectRoleById(item);
                if (SysUserRoleService.CountUserRoleByRoleId(item) > 0)
                {
                    throw new CustomException($"{role.RoleName}已分配,不能删除");
                }
            }
            return Delete(roleIds);
        }

        /// <summary>
        /// Change role permission status
        /// </summary>
        /// <param name="roleDto"></param>
        /// <returns></returns>
        public int UpdateRoleStatus(SysRole roleDto)
        {
            return Update(roleDto, it => new { it.Status }, f => f.RoleId == roleDto.RoleId);
        }

        /// <summary>
        /// Check if role permission is unique
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public string CheckRoleKeyUnique(SysRole sysRole)
        {
            long roleId = 0 == sysRole.RoleId ? -1L : sysRole.RoleId;
            SysRole info = GetFirst(it => it.RoleKey == sysRole.RoleKey);
            if (info != null && info.RoleId != roleId)
            {
                return UserConstants.NOT_UNIQUE;
            }
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// Check if role is allowed for operation
        /// </summary>
        /// <param name="role"></param>
        public void CheckRoleAllowed(SysRole role)
        {
            if (IsRoleAdmin(role.RoleId))
            {
                throw new CustomException("不允许操作超级管理员角色");
            }
        }

        /// <summary>
        /// Add and save role information
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public long InsertRole(SysRole sysRole)
        {
            sysRole.Create_time = DateTime.Now;


            sysRole.RoleId = InsertReturnBigIdentity(sysRole);
            //插入角色部门数据
            DeptService.InsertRoleDepts(sysRole);

            return sysRole.RoleId;
        }

        /// <summary>
        /// Delete role and menu associations by role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns></returns>
        public int DeleteRoleMenuByRoleId(long roleId)
        {
            return RoleMenuService.DeleteRoleMenuByRoleId(roleId);
        }



        public bool AuthDataScope22222222222(SysRoleDto sysRoleDto)
        {
            var result = UseTran(() =>
            {
                var oldMenus = SelectUserRoleMenus(sysRoleDto.RoleId).Select(t => t.ParseToLong());
                var newMenus = sysRoleDto.MenuIds;

                // 获取需要删除的菜单（在旧菜单中但不在新菜单中）
                var delMenuIds = oldMenus.Except(newMenus).ToList();

                // 获取需要新增的菜单（在新菜单中但不在旧菜单中）
                var addMenuIds = newMenus.Except(oldMenus).ToList();

                // 删除不再需要的菜单权限
                if (delMenuIds.Count > 0)
                {
                    RoleMenuService.DeleteRoleMenuByRoleIdMenuIds(sysRoleDto.RoleId, delMenuIds.ToArray());
                }

                // 只添加新增的菜单权限
                if (addMenuIds.Count > 0)
                {
                    // 创建一个新的DTO用于添加菜单
                    var addMenuDto = new SysRoleDto
                    {
                        RoleId = sysRoleDto.RoleId,
                        MenuIds = addMenuIds
                    };
                    InsertRoleMenu(addMenuDto);
                }

                // 如果需要，可以设置输出参数
                sysRoleDto.DelMenuIds = delMenuIds;

                Console.WriteLine($"删除了{delMenuIds.Count}个菜单，新增了{addMenuIds.Count}个菜单");
            });

            return result.IsSuccess;
        }



        /// <summary>
        /// Update data permission information
        /// </summary>
        /// <param name="sysRoleDto"></param>
        /// <returns></returns>
        public bool AuthDataScope(SysRoleDto sysRoleDto)
        {
            var result = UseTran(() =>
            {
                //删除角色菜单
                //DeleteRoleMenuByRoleId(sysRoleDto.RoleId);
                //InsertRoleMenu(sysRoleDto);
                var oldMenus = SelectUserRoleMenus(sysRoleDto.RoleId).Select(t=>t.ParseToLong());
                var newMenus = sysRoleDto.MenuIds;

                //并集菜单
                var arr_c = oldMenus.Intersect(newMenus).ToArray();
                //获取减量菜单
                var delMenuIds = oldMenus.Where(c => !arr_c.Contains(c)).ToArray();
                //获取增量
                var addMenuIds = newMenus.Where(c => !arr_c.Contains(c)).ToArray();

                RoleMenuService.DeleteRoleMenuByRoleIdMenuIds(sysRoleDto.RoleId, delMenuIds);
                sysRoleDto.MenuIds = [.. addMenuIds ];
                sysRoleDto.DelMenuIds = [.. delMenuIds];
                InsertRoleMenu(sysRoleDto);
                Console.WriteLine($"减少了{delMenuIds.Length},增加了{addMenuIds.Length}菜单");
            });
            return result.IsSuccess;
        }

        #region Service

        /// <summary>
        /// Batch add role menu information
        /// </summary>
        /// <param name="sysRoleDto"></param>
        /// <returns></returns>
        public int InsertRoleMenu(SysRoleDto sysRoleDto)
        {
            int rows = 1;
            // 新增用户与角色管理
            List<SysRoleMenu> sysRoleMenus = new();
            foreach (var item in sysRoleDto.MenuIds)
            {
                SysRoleMenu rm = new()
                {
                    Menu_id = item,
                    Role_id = sysRoleDto.RoleId,
                    Create_by = sysRoleDto.Create_by,
                    Create_time = DateTime.Now
                };
                sysRoleMenus.Add(rm);
            }
            //添加角色菜单
            if (sysRoleMenus.Count > 0)
            {
                rows = RoleMenuService.AddRoleMenu(sysRoleMenus);
            }

            return rows;
        }

        /// <summary>
        /// Check if user is administrator
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public bool IsAdmin(long userid)
        {
            List<string> roles = SelectUserRoleKeys(userid);

            return ((IList)roles).Contains(GlobalConstant.AdminRole);
        }

        /// <summary>
        /// Check if user is administrator
        /// </summary>
        /// <param name="roleid"></param>
        /// <returns></returns>
        public bool IsRoleAdmin(long roleid)
        {
            var roleInfo = GetFirst(x => x.RoleId == roleid);

            return roleInfo.RoleKey == GlobalConstant.AdminRole;
        }

        /// <summary>
        /// Get role menu ID collection
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<string> SelectUserRoleMenus(long roleId)
        {
            var list = RoleMenuService.SelectRoleMenuByRoleId(roleId);

            return list.Select(x => x.Menu_id.ToString()).Distinct().ToList();
        }

        /// <summary>
        /// Get menus by all user roles
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        public List<long> SelectRoleMenuByRoleIds(long[] roleIds)
        {
            return RoleMenuService.SelectRoleMenuByRoleIds(roleIds)
                .Select(x => x.Menu_id)
                .Distinct().ToList();
        }

        /// <summary>
        /// Get user role list
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<SysRole> SelectUserRoleListByUserId(long userId)
        {
            return Context.Queryable<SysUserRole>()
                .LeftJoin<SysRole>((ur, r) => ur.RoleId == r.RoleId)
                .Where((ur, r) => ur.UserId == userId && r.RoleId > 0)
                .Select((ur, r) => r)
                .ToList();
        }

        /// <summary>
        /// Get user permission collection
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<long> SelectUserRoles(long userId)
        {
            var list = SelectUserRoleListByUserId(userId).Where(f => f.Status == 0);

            return list.Select(x => x.RoleId).ToList();
        }

        /// <summary>
        /// Get user permission string collection
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> SelectUserRoleKeys(long userId)
        {
            var list = SelectUserRoleListByUserId(userId);
            return list.Select(x => x.RoleKey).ToList();
        }

        /// <summary>
        /// Get all user role names
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<string> SelectUserRoleNames(long userId)
        {
            var list = SelectUserRoleListByUserId(userId);
            return list.Select(x => x.RoleName).ToList();
        }
        #endregion

        /// <summary>
        /// Update and save role information
        /// </summary>
        /// <param name="sysRole">Role information</param>
        /// <returns></returns>
        public int UpdateRole(SysRole sysRole)
        {
            var result = UseTran(() =>
            {
                //修改角色信息
                UpdateSysRole(sysRole);
                //删除角色与部门管理
                DeptService.DeleteRoleDeptByRoleId(sysRole.RoleId);
                //插入角色部门数据
                DeptService.InsertRoleDepts(sysRole);
            });

            return result.IsSuccess ? 1 : 0;
        }

        /// <summary>
        /// Update user role
        /// </summary>
        /// <param name="sysRole"></param>
        /// <returns></returns>
        public int UpdateSysRole(SysRole sysRole)
        {
            var db = Context;
            sysRole.Update_time = db.GetDate();

            return db.Updateable<SysRole>()
            .SetColumns(it => it.Update_time == sysRole.Update_time)
            .SetColumns(it => it.DataScope == sysRole.DataScope)
            .SetColumns(it => it.Remark == sysRole.Remark)
            .SetColumns(it => it.Update_by == sysRole.Update_by)
            //.SetColumns(it => it.MenuCheckStrictly == sysRole.MenuCheckStrictly)
            .SetColumns(it => it.DeptCheckStrictly == sysRole.DeptCheckStrictly)
            .SetColumnsIF(!string.IsNullOrEmpty(sysRole.RoleName), it => it.RoleName == sysRole.RoleName)
            .SetColumnsIF(!string.IsNullOrEmpty(sysRole.RoleKey), it => it.RoleKey == sysRole.RoleKey)
            .SetColumnsIF(sysRole.RoleSort >= 0, it => it.RoleSort == sysRole.RoleSort)
            .Where(it => it.RoleId == sysRole.RoleId)
            .ExecuteCommand();
        }
    }
}

