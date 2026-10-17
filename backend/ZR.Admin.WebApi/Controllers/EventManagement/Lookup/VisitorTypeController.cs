using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-16
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Visitor Type - أنواع الزوار
    /// </summary>
    [Route("EventManagement/EventVisitorTypes")]
    public class VisitorTypeController : BaseController
    {
        /// <summary>
        /// Visitor Type - أنواع الزوار接口
        /// </summary>
        private readonly IVisitorTypeService _VisitorTypeService;

        public VisitorTypeController(IVisitorTypeService VisitorTypeService)
        {
            _VisitorTypeService = VisitorTypeService;
        }

        /// <summary>
        /// 查询Visitor Type - أنواع الزوار列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:list")]
        public IActionResult QueryVisitorType([FromQuery] VisitorTypeQueryDto parm)
        {
            var response = _VisitorTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Visitor Type - أنواع الزوار详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:query")]
        public IActionResult GetVisitorType(long Typeid)
        {
            var response = _VisitorTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<VisitorTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Visitor Type - أنواع الزوار
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:add")]
        [Log(Title = "Visitor Type - أنواع الزوار", BusinessType = BusinessType.INSERT)]
        public IActionResult AddVisitorType([FromBody] VisitorTypeDto parm)
        {
            var modal = parm.Adapt<VisitorType>().ToCreate(HttpContext);

            var response = _VisitorTypeService.AddVisitorType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Visitor Type - أنواع الزوار
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:edit")]
        [Log(Title = "Visitor Type - أنواع الزوار", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateVisitorType([FromBody] VisitorTypeDto parm)
        {
            var modal = parm.Adapt<VisitorType>().ToUpdate(HttpContext);
            var response = _VisitorTypeService.UpdateVisitorType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Visitor Type - أنواع الزوار
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:delete")]
        [Log(Title = "Visitor Type - أنواع الزوار", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteVisitorType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_VisitorTypeService.Delete(idArr, "删除Visitor Type - أنواع الزوار"));
        }

        /// <summary>
        /// 导出Visitor Type - أنواع الزوار
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Visitor Type - أنواع الزوار", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:export")]
        public IActionResult Export([FromQuery] VisitorTypeQueryDto parm)
        {
            var list = _VisitorTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Visitor Type - أنواع الزوار", "Visitor Type - أنواع الزوار");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Visitor Type - أنواع الزوار
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Visitor Type - أنواع الزوار", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_VisitorTypeService.TruncateVisitorType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Visitor Type - أنواع الزوار导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventvisitortypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<VisitorTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<VisitorTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_VisitorTypeService.ImportVisitorType(list.Adapt<List<VisitorType>>()));
        }

        /// <summary>
        /// Visitor Type - أنواع الزوار导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Visitor Type - أنواع الزوار模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<VisitorTypeDto>() { }, "VisitorType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
