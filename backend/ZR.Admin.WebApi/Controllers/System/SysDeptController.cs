using Microsoft.AspNetCore.Mvc;
using System.Collections;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Department
    /// </summary>
    [Route("system/dept")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysDeptController : BaseController
    {
        public ISysDeptService DeptService;
        public ISysUserService UserService;
        public SysDeptController(ISysDeptService deptService
            , ISysUserService userService)
        {
            DeptService = deptService;
            UserService = userService;
        }

        /// <summary>
        /// Get department list
        /// </summary>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "system:dept:list")]
        [LookupPermission("system:dept:lookup", isPublic: true)]
        [HttpGet("list")]
        public IActionResult List([FromQuery] SysDeptQueryDto dept)
        {
            return SUCCESS(DeptService.GetList(dept));
        }

        /// <summary>
        /// Query department list (exclude nodes)
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        [HttpGet("list/exclude/{deptId}")]
        public IActionResult ExcludeChild(long deptId)
        {
            var depts = DeptService.GetList(new SysDeptQueryDto());

            for (int i = 0; i < depts.Count; i++)
            {
                SysDeptDto d = depts[i];
                long[] deptIds = Tools.SpitLongArrary(d.Ancestors);
                if (d.DeptId == deptId || ((IList)deptIds).Contains(deptId))
                {
                    depts.Remove(d);
                }
            }
            return SUCCESS(depts);
        }

        /// <summary>
        /// Get department dropdown tree list
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        [HttpGet("treeselect")]
        public IActionResult TreeSelect(SysDeptQueryDto dept)
        {
            var depts = DeptService.GetSysDepts(dept);

            return SUCCESS(DeptService.BuildDeptTreeSelect(depts), TIME_FORMAT_FULL);
        }

        /// <summary>
        /// Get role department information
        /// Load corresponding role department list tree
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>        
        [HttpGet("roleDeptTreeselect/{roleId}")]
        public IActionResult RoleMenuTreeselect(long roleId)
        {
            var depts = DeptService.GetSysDepts(new SysDeptQueryDto());
            var checkedKeys = DeptService.SelectRoleDepts(roleId);
            return SUCCESS(new
            {
                checkedKeys,
                depts = DeptService.BuildDeptTreeSelect(depts),
            });
        }

        /// <summary>
        /// Get detailed information by department number
        /// </summary>
        /// <returns></returns>
        [HttpGet("{deptId}")]
        [ActionPermissionFilter(Permission = "system:dept:query")]
        public IActionResult GetInfo(long deptId)
        {
            var info = DeptService.GetFirst(f => f.DeptId == deptId);
            return SUCCESS(info);
        }

        /// <summary>
        /// Add new department
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        [HttpPost]
        [Log(Title = "Department Management", BusinessType = BusinessType.INSERT)]
        [ActionPermissionFilter(Permission = "system:dept:add")]
        public IActionResult Add([FromBody] SysDept dept)
        {
            if (UserConstants.NOT_UNIQUE.Equals(DeptService.CheckDeptNameUnique(dept)))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Add department {dept.DeptName} failed, department name already exists");
            }
            dept.Create_by = HttpContext.GetName();
            return ToResponse(DeptService.InsertDept(dept));
        }

        /// <summary>
        /// Modify department
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        [HttpPut]
        [Log(Title = "Department Management", BusinessType = BusinessType.UPDATE)]
        [ActionPermissionFilter(Permission = "system:dept:update")]
        public IActionResult Update([FromBody] SysDept dept)
        {
            if (UserConstants.NOT_UNIQUE.Equals(DeptService.CheckDeptNameUnique(dept)))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Modify department {dept.DeptName} failed, department name already exists");
            }
            else if (dept.ParentId.Equals(dept.DeptId))
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Modify department {dept.DeptName} failed, parent department cannot be itself");
            }
            dept.Update_by = HttpContext.GetName();
            return ToResponse(DeptService.UpdateDept(dept));
        }

        /// <summary>
        /// Delete department
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{deptId}")]
        [ActionPermissionFilter(Permission = "system:dept:remove")]
        [Log(Title = "Department Management", BusinessType = BusinessType.DELETE)]
        public IActionResult Remove(long deptId)
        {
            if (DeptService.Queryable().Count(it => it.ParentId == deptId && it.DelFlag == 0) > 0)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Subdepartments exist, deletion not allowed");
            }
            if (UserService.Queryable().Count(it => it.DeptId == deptId && it.DelFlag == 0) > 0)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Department has users, deletion not allowed");
            }

            return SUCCESS(DeptService.Delete(deptId));
        }
    }
}

