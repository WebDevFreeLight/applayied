using Infrastructure;
using Infrastructure.Attribute;
using ZR.Common;
using ZR.Model.System;
using ZR.Model.System.Dto;
using ZR.Model.System.Vo; 

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ????
    /// </summary>
    [AppService(ServiceType = typeof(ISysDeptService), ServiceLifetime = LifeTime.Transient)]
    public class SysDeptService : BaseService<SysDept>, ISysDeptService
    {
        public ISysRoleDeptService RoleDeptRepository;
        public SysDeptService(ISysRoleDeptService roleDeptRepository)
        {
            RoleDeptRepository = roleDeptRepository;
        }
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public List<SysDeptDto> GetList(SysDeptQueryDto dept)
        {
            var predicate = Expressionable.Create<SysDept>();
            predicate = predicate.And(it => it.DelFlag == 0);
            predicate = predicate.AndIF(dept.DeptName.IfNotEmpty(), it => it.DeptName.Contains(dept.DeptName));
            predicate = predicate.AndIF(dept.Status != null, it => it.Status == dept.Status);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .Select((it) => new SysDeptDto()
                {
                    UserNum = SqlFunc.Subqueryable<SysUser>().Where(f => f.DeptId == it.DeptId).Count()
                }, true)
                .ToList();

            return response;
        }
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public List<SysDept> GetSysDepts(SysDeptQueryDto dept)
        {
            var predicate = Expressionable.Create<SysDept>();
            predicate = predicate.And(it => it.DelFlag == 0);
            predicate = predicate.AndIF(dept.DeptName.IfNotEmpty(), it => it.DeptName.Contains(dept.DeptName));
            predicate = predicate.AndIF(dept.Status != null, it => it.Status == dept.Status);

            var response = Queryable().Where(predicate.ToExpression()).ToList();

            return response;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public string CheckDeptNameUnique(SysDept dept)
        {
            long deptId = dept.DeptId == 0 ? -1L : dept.DeptId;
            SysDept info = GetFirst(it => it.DeptName == dept.DeptName && it.ParentId == dept.ParentId);
            if (info != null && info.DeptId != deptId)
            {
                return UserConstants.NOT_UNIQUE;
            }
            return UserConstants.UNIQUE;
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public int InsertDept(SysDept dept)
        {
            SysDept info = GetFirst(it => it.DeptId == dept.ParentId);
            //???????????,?????????
            if (info != null && UserConstants.DEPT_NORMAL != info?.Status)
            {
                throw new CustomException("????,?????");
            }
            dept.Ancestors = "";
            if (info != null)
            {
                dept.Ancestors = info.Ancestors + "," + dept.ParentId;
            }


            Insertable(dept).ExecuteReturnSnowflakeId();


            return 1;


            return Add(dept);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        public int UpdateDept(SysDept dept)
        {
            SysDept newParentDept = GetFirst(it => it.DeptId == dept.ParentId);
            SysDept oldDept = GetFirst(m => m.DeptId == dept.DeptId);
            if (newParentDept != null && oldDept != null)
            {
                string newAncestors = newParentDept.Ancestors + "," + newParentDept.DeptId;
                string oldAncestors = oldDept.Ancestors;
                dept.Ancestors = newAncestors;
                UpdateDeptChildren(dept.DeptId, newAncestors, oldAncestors);
            }
            int result = Context.Updateable(dept).ExecuteCommand();
            if (UserConstants.DEPT_NORMAL.Equals(dept.Status) && dept.Ancestors.IfNotEmpty()
                && !"0".Equals(dept.Ancestors))
            {
                // ??????????,?????????????
                UpdateParentDeptStatusNormal(dept);
            }
            return result;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="dept">????</param>
        private void UpdateParentDeptStatusNormal(SysDept dept)
        {
            long[] depts = Tools.SpitLongArrary(dept.Ancestors);
            dept.Status = 0;
            dept.Update_time = DateTime.Now;

            Update(dept, it => new { it.Update_by, it.Update_time, it.Status }, f => depts.Contains(f.DeptId));
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="deptId">??????ID</param>
        /// <param name="newAncestors">???ID??</param>
        /// <param name="oldAncestors">???ID??</param>
        public void UpdateDeptChildren(long deptId, string newAncestors, string oldAncestors)
        {
            List<SysDept> children = GetChildrenDepts(GetSysDepts(new SysDeptQueryDto()), deptId);

            foreach (var child in children)
            {
                string ancestors = child.Ancestors.ReplaceFirst(oldAncestors, newAncestors);
                long[] ancestorsArr = Tools.SpitLongArrary(ancestors).Distinct().ToArray();
                child.Ancestors = string.Join(",", ancestorsArr);
            }
            if (children.Any())
            {
                Context.Updateable(children).WhereColumns(f => new { f.DeptId })
                .UpdateColumns(it => new { it.Ancestors }).ExecuteCommand();
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="deptId"></param>
        /// <returns></returns>
        public List<SysDept> GetChildrenDepts(List<SysDept> depts, long deptId)
        {
            return depts.FindAll(delegate (SysDept item)
            {
                long[] pid = Tools.SpitLongArrary(item.Ancestors);
                return pid.Contains(deptId);
            });
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="depts">????</param>
        /// <returns></returns>
        public List<SysDept> BuildDeptTree(List<SysDept> depts)
        {
            List<SysDept> returnList = new List<SysDept>();
            List<long> tempList = depts.Select(f => f.DeptId).ToList();
            foreach (var dept in depts)
            {
                // ???????, ????????????
                if (!tempList.Contains(dept.ParentId))
                {
                    RecursionFn(depts, dept);
                    returnList.Add(dept);
                }
            }

            if (!returnList.Any())
            {
                returnList = depts;
            }
            return returnList;
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="depts"></param>
        /// <returns></returns>
        public List<TreeSelectVo> BuildDeptTreeSelect(List<SysDept> depts)
        {
            List<SysDept> menuTrees = BuildDeptTree(depts);
            List<TreeSelectVo> treeMenuVos = new List<TreeSelectVo>();
            foreach (var item in menuTrees)
            {
                treeMenuVos.Add(new TreeSelectVo(item));
            }
            return treeMenuVos;
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="list"></param>
        /// <param name="t"></param>
        private void RecursionFn(List<SysDept> list, SysDept t)
        {
            //???????
            List<SysDept> childList = GetChildList(list, t);
            t.children = childList;
            foreach (var item in childList)
            {
                if (GetChildList(list, item).Count() > 0)
                {
                    RecursionFn(list, item);
                }
            }
        }
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="list">????</param>
        /// <param name="dept"></param>
        /// <returns></returns>
        private List<SysDept> GetChildList(List<SysDept> list, SysDept dept)
        {
            return list.Where(p => p.ParentId == dept.DeptId).ToList();
        }

        #region ????

        /// <summary>
        /// ????????id
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<SysRoleDept> SelectRoleDeptByRoleId(long roleId)
        {
            return RoleDeptRepository.SelectRoleDeptByRoleId(roleId);
        }

        /// <summary>
        /// ??????id??
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public List<long> SelectRoleDepts(long roleId)
        {
            var list = SelectRoleDeptByRoleId(roleId);

            return list.Select(x => x.DeptId).Distinct().ToList();
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public bool DeleteRoleDeptByRoleId(long roleId)
        {
            return RoleDeptRepository.Delete(f => f.RoleId == roleId);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public int InsertRoleDepts(SysRole role)
        {
            int rows = 1;
            List<SysRoleDept> list = new();
            if (role.DeptIds!=null && role.DeptIds.Count()>0)
            {
                foreach (var item in role.DeptIds)
                {
                    list.Add(new SysRoleDept() { DeptId = item, RoleId = role.RoleId });
                }
                if (list.Count > 0)
                {
                    rows = RoleDeptRepository.Insert(list);
                }
            }

          
            return rows;
        }
        #endregion
    }

    /// <summary>
    /// ????
    /// </summary>
    [AppService(ServiceType = typeof(ISysRoleDeptService), ServiceLifetime = LifeTime.Transient)]
    public class SysRoleDeptService : BaseService<SysRoleDept>, ISysRoleDeptService
    {
        public List<SysRoleDept> SelectRoleDeptByRoleId(long roleId)
        {
            return GetList(it => it.RoleId == roleId).ToList();
        }
    }
}

