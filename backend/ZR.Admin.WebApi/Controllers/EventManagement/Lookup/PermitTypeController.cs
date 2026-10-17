using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-16
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Permit Type - أنواع التصاريح
    /// </summary>
    [Route("EventManagement/EventPermitTypes")]
    public class PermitTypeController : BaseController
    {
        /// <summary>
        /// Permit Type - أنواع التصاريح接口
        /// </summary>
        private readonly IPermitTypeService _PermitTypeService;

        public PermitTypeController(IPermitTypeService PermitTypeService)
        {
            _PermitTypeService = PermitTypeService;
        }

        /// <summary>
        /// 查询Permit Type - أنواع التصاريح列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:list")]
        public IActionResult QueryPermitType([FromQuery] PermitTypeQueryDto parm)
        {
            var response = _PermitTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Permit Type - أنواع التصاريح详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:query")]
        public IActionResult GetPermitType(long Typeid)
        {
            var response = _PermitTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<PermitTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Permit Type - أنواع التصاريح
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:add")]
        [Log(Title = "Permit Type - أنواع التصاريح", BusinessType = BusinessType.INSERT)]
        public IActionResult AddPermitType([FromBody] PermitTypeDto parm)
        {
            var modal = parm.Adapt<PermitType>().ToCreate(HttpContext);

            var response = _PermitTypeService.AddPermitType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Permit Type - أنواع التصاريح
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:edit")]
        [Log(Title = "Permit Type - أنواع التصاريح", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdatePermitType([FromBody] PermitTypeDto parm)
        {
            var modal = parm.Adapt<PermitType>().ToUpdate(HttpContext);
            var response = _PermitTypeService.UpdatePermitType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Permit Type - أنواع التصاريح
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:delete")]
        [Log(Title = "Permit Type - أنواع التصاريح", BusinessType = BusinessType.DELETE)]
        public IActionResult DeletePermitType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_PermitTypeService.Delete(idArr, "删除Permit Type - أنواع التصاريح"));
        }

        /// <summary>
        /// 导出Permit Type - أنواع التصاريح
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Permit Type - أنواع التصاريح", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:export")]
        public IActionResult Export([FromQuery] PermitTypeQueryDto parm)
        {
            var list = _PermitTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Permit Type - أنواع التصاريح", "Permit Type - أنواع التصاريح");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Permit Type - أنواع التصاريح
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Permit Type - أنواع التصاريح", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_PermitTypeService.TruncatePermitType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Permit Type - أنواع التصاريح导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventpermittypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<PermitTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<PermitTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_PermitTypeService.ImportPermitType(list.Adapt<List<PermitType>>()));
        }

        /// <summary>
        /// Permit Type - أنواع التصاريح导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Permit Type - أنواع التصاريح模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<PermitTypeDto>() { }, "PermitType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
