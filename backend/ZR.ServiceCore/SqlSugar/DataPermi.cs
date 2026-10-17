using Infrastructure;
using SqlSugar.IOC;
using ZR.Model;
using ZR.Model.Models;
using ZR.Model.System;

namespace ZR.ServiceCore.SqlSugar
{
    public enum DataPermiEnum
    {
        None = 0,
        /// <summary>
        /// All data permissions
        /// </summary>
        All = 1,
        /// <summary>
        /// Personal data permissions only
        /// </summary>
        SELF = 5,
        /// <summary>
        /// Department data permissions
        /// </summary>
        DEPT = 3,
        /// <summary>
        /// Custom data permissions
        /// </summary>
        CUSTOM = 2,
        /// <summary>
        /// Department and sub-department data permissions
        /// </summary>
        DEPT_CHILD = 4
    }
    /// <summary>
    /// Data permissions
    /// </summary>
    public class DataPermi
    {
        /// <summary>
        /// Data filtering
        /// </summary>
        /// <param name="configId">Multi-database id</param>
        public static void FilterData(string configId)
        {
            //Get current user information
            var user = JwtUtil.GetLoginUser(App.HttpContext);
            if (user == null || user.RoleKeys == null) return;

            var db = DbScoped.SugarScope.GetConnectionScope(configId);
            var expUser = Expressionable.Create<SysUser>().And(it => it.DelFlag == 0);
            var expRole = Expressionable.Create<SysRole>();
            var expLoginlog = Expressionable.Create<SysLogininfor>();
            var expSysMsg = Expressionable.Create<SysUserMsg>().And(it => it.IsDelete == 0);
            var expDept = Expressionable.Create<SysDept>();
            
            db.QueryFilter.AddTableFilter(expSysMsg.ToExpression());
            //Administrator is not filtered
            if (user.RoleKeys.Any(f => f.Equals(GlobalConstant.AdminRole))) return;

            foreach (var role in user.Roles.OrderBy(f => f.DataScope))
            {
                var dataScope = (DataPermiEnum)role.DataScope;
                if (DataPermiEnum.All.Equals(dataScope))//All permissions
                {
                    break;
                }
                else if (DataPermiEnum.CUSTOM.Equals(dataScope))//Custom data permissions
                {
                    //" OR {}.dept_id IN ( SELECT dept_id FROM sys_role_dept WHERE role_id = {} ) ", deptAlias, role.getRoleId()));

                    expUser.Or(it => SqlFunc.Subqueryable<SysRoleDept>().Where(f => f.DeptId == it.DeptId && f.RoleId == role.RoleId).Any());
                }
                else if (DataPermiEnum.DEPT.Equals(dataScope))//Department data
                {
                    expUser.And(it => it.DeptId == user.DeptId);
                    expDept.And(it => it.DeptId == user.DeptId);
                }
                else if (DataPermiEnum.DEPT_CHILD.Equals(dataScope))//Department and sub-department data
                {
                    //SQl  OR {}.dept_id IN ( SELECT dept_id FROM sys_dept WHERE dept_id = {} or find_in_set( {} , ancestors ) )
                    var allChildDepts = db.Queryable<SysDept>().ToChildList(it => it.ParentId, user.DeptId);
                    var allDeptId = allChildDepts.Select(f => f.DeptId).ToList();

                    expUser.Or(it => allDeptId.Contains(it.DeptId));
                    expDept.And(it => allDeptId.Contains(it.DeptId));
                }
                else if (DataPermiEnum.SELF.Equals(dataScope))//Personal data only
                {
                    expUser.Or(it => it.UserId == user.UserId);
                    expRole.Or(it => user.RoleKeys.Contains(it.RoleKey));
                    expLoginlog.And(it => it.UserName == user.UserName);
                }
            }

            db.QueryFilter.AddTableFilter(expDept.ToExpression());
            db.QueryFilter.AddTableFilter(expUser.ToExpression());
            db.QueryFilter.AddTableFilter(expRole.ToExpression());
            db.QueryFilter.AddTableFilter(expLoginlog.ToExpression());
            db.QueryFilter.AddTableFilter<UserOnlineLog>(f => f.UserId == user.UserId, QueryFilterProvider.FilterJoinPosition.Where);
        }
    }
}

