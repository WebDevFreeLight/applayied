using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-15
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Zone Type - أنواع المناطق
    /// </summary>
    [Route("EventManagement/EventZoneTypes")]
    public class EventZoneTypeController : BaseController
    {
        /// <summary>
        /// Zone Type - أنواع المناطق接口
        /// </summary>
        private readonly IEventZoneTypeService _EventZoneTypeService;

        public EventZoneTypeController(IEventZoneTypeService EventZoneTypeService)
        {
            _EventZoneTypeService = EventZoneTypeService;
        }

        /// <summary>
        /// 查询Zone Type - أنواع المناطق列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:list")]
        public IActionResult QueryEventZoneType([FromQuery] EventZoneTypeQueryDto parm)
        {
            var response = _EventZoneTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Zone Type - أنواع المناطق详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:query")]
        public IActionResult GetEventZoneType(long Typeid)
        {
            var response = _EventZoneTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<EventZoneTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:add")]
        [Log(Title = "Zone Type - أنواع المناطق", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventZoneType([FromBody] EventZoneTypeDto parm)
        {
            var modal = parm.Adapt<EventZoneType>().ToCreate(HttpContext);

            var response = _EventZoneTypeService.AddEventZoneType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:edit")]
        [Log(Title = "Zone Type - أنواع المناطق", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventZoneType([FromBody] EventZoneTypeDto parm)
        {
            var modal = parm.Adapt<EventZoneType>().ToUpdate(HttpContext);
            var response = _EventZoneTypeService.UpdateEventZoneType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:delete")]
        [Log(Title = "Zone Type - أنواع المناطق", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventZoneType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventZoneTypeService.Delete(idArr, "删除Zone Type - أنواع المناطق"));
        }

        /// <summary>
        /// 导出Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Zone Type - أنواع المناطق", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:export")]
        public IActionResult Export([FromQuery] EventZoneTypeQueryDto parm)
        {
            var list = _EventZoneTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Zone Type - أنواع المناطق", "Zone Type - أنواع المناطق");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Zone Type - أنواع المناطق
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Zone Type - أنواع المناطق", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventZoneTypeService.TruncateEventZoneType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Zone Type - أنواع المناطق导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventzonetypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventZoneTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventZoneTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventZoneTypeService.ImportEventZoneType(list.Adapt<List<EventZoneType>>()));
        }

        /// <summary>
        /// Zone Type - أنواع المناطق导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Zone Type - أنواع المناطق模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventZoneTypeDto>() { }, "EventZoneType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
