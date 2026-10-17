using Microsoft.AspNetCore.Mvc;
using ZR.Model.System.Model;
using ZR.Model.System.Model.Dto;

//Created: 2025-06-08
namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// File Group
    /// </summary>
    [Route("tool/FileGroup")]
    public class SysFileGroupController : BaseController
    {
        /// <summary>
        /// File group interface
        /// </summary>
        private readonly ISysFileGroupService _SysFileGroupService;

        public SysFileGroupController(ISysFileGroupService SysFileGroupService)
        {
            _SysFileGroupService = SysFileGroupService;
        }

        /// <summary>
        /// Query file group list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "filegroup:list")]
        public IActionResult QuerySysFileGroup([FromQuery] SysFileGroupQueryDto parm)
        {
            var response = _SysFileGroupService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query file group tree list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("treeList")]
        //[ActionPermissionFilter(Permission = "filegroup:list")]
        public IActionResult QueryTreeSysFileGroup([FromQuery] SysFileGroupQueryDto parm)
        {
            var response = _SysFileGroupService.GetTreeList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Add file group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[ActionPermissionFilter(Permission = "filegroup:add")]
        [Log(Title = "File Group", BusinessType = BusinessType.INSERT)]
        public IActionResult AddSysFileGroup([FromBody] SysFileGroupDto parm)
        {
            var modal = parm.Adapt<SysFileGroup>().ToCreate(HttpContext);

            var response = _SysFileGroupService.AddSysFileGroup(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update file group
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "filegroup:edit")]
        [Log(Title = "File Group", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateSysFileGroup([FromBody] SysFileGroupDto parm)
        {
            var modal = parm.Adapt<SysFileGroup>().ToUpdate(HttpContext);
            var response = _SysFileGroupService.UpdateSysFileGroup(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete file group
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "filegroup:delete")]
        [Log(Title = "File Group", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSysFileGroup([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<int>(ids);

            return ToResponse(_SysFileGroupService.Delete(idArr));
        }

        /// <summary>
        /// Save sort order
        /// </summary>
        /// <param name="id">Primary key</param>
        /// <param name="value">Sort value</param>
        /// <returns></returns>
        [ActionPermissionFilter(Permission = "filegroup:edit")]
        [HttpGet("ChangeSort")]
        [Log(Title = "Save Sort Order", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeSort(int id = 0, int value = 0)
        {
            if (id <= 0) { return ToResponse(ApiResult.Error(101, "Request parameters are invalid")); }
            var response = _SysFileGroupService.Update(w => w.GroupId == id, it => new SysFileGroup()
            {
                Sort = value,
            });

            return ToResponse(response);
        }
    }
}
