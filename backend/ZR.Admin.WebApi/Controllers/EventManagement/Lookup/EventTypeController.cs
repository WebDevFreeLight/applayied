using Microsoft.AspNetCore.Mvc;
using ZR.Model.EventManagement.Dto;
using ZR.Model.EventManagement;
using ZR.Service.EventManagement.IEventManagementService;
using MiniExcelLibs;

//创建时间：2025-10-15
namespace ZR.Admin.WebApi.Controllers.EventManagement.Lookup
{
    /// <summary>
    /// Event Type - أنواع الفعاليات
    /// </summary>
    [Route("EventManagement/EventTypes")]
    public class EventTypeController : BaseController
    {
        /// <summary>
        /// Event Type - أنواع الفعاليات接口
        /// </summary>
        private readonly IEventTypeService _EventTypeService;

        public EventTypeController(IEventTypeService EventTypeService)
        {
            _EventTypeService = EventTypeService;
        }

        /// <summary>
        /// 查询Event Type - أنواع الفعاليات列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:list")]
        public IActionResult QueryEventType([FromQuery] EventTypeQueryDto parm)
        {
            var response = _EventTypeService.GetList(parm);
            return SUCCESS(response);
        }


        /// <summary>
        /// 查询Event Type - أنواع الفعاليات详情
        /// </summary>
        /// <param name="Typeid"></param>
        /// <returns></returns>
        [HttpGet("{Typeid}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:query")]
        public IActionResult GetEventType(long Typeid)
        {
            var response = _EventTypeService.GetInfo(Typeid);
            
            var info = response.Adapt<EventTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// 添加Event Type - أنواع الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:add")]
        [Log(Title = "Event Type - أنواع الفعاليات", BusinessType = BusinessType.INSERT)]
        public IActionResult AddEventType([FromBody] EventTypeDto parm)
        {
            var modal = parm.Adapt<EventType>().ToCreate(HttpContext);

            var response = _EventTypeService.AddEventType(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 更新Event Type - أنواع الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:edit")]
        [Log(Title = "Event Type - أنواع الفعاليات", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateEventType([FromBody] EventTypeDto parm)
        {
            var modal = parm.Adapt<EventType>().ToUpdate(HttpContext);
            var response = _EventTypeService.UpdateEventType(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// 删除Event Type - أنواع الفعاليات
        /// </summary>
        /// <returns></returns>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:delete")]
        [Log(Title = "Event Type - أنواع الفعاليات", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteEventType([FromRoute]string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);

            return ToResponse(_EventTypeService.Delete(idArr, "删除Event Type - أنواع الفعاليات"));
        }

        /// <summary>
        /// 导出Event Type - أنواع الفعاليات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Type - أنواع الفعاليات", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:export")]
        public IActionResult Export([FromQuery] EventTypeQueryDto parm)
        {
            var list = _EventTypeService.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "没有要导出的数据");
            }
            var result = ExportExcelMini(list, "Event Type - أنواع الفعاليات", "Event Type - أنواع الفعاليات");
            return ExportExcel(result.Item2, result.Item1);
        }

        /// <summary>
        /// 清空Event Type - أنواع الفعاليات
        /// </summary>
        /// <returns></returns>
        [Log(Title = "Event Type - أنواع الفعاليات", BusinessType = BusinessType.CLEAN)]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:delete")]
        [HttpPost("clean")]
        public IActionResult Clear()
        {
            if (!HttpContext.IsAdmin())
            {
                return ToResponse(ResultCode.FAIL, "操作失败");
            }
            return SUCCESS(_EventTypeService.TruncateEventType());
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("importData")]
        [Log(Title = "Event Type - أنواع الفعاليات导入", BusinessType = BusinessType.IMPORT, IsSaveRequestData = false)]
        [ActionPermissionFilter(Permission = "EventManagement:eventtypes:import")]
        public IActionResult ImportData([FromForm(Name = "file")] IFormFile formFile)
        {
            List<EventTypeDto> list = new();
            using (var stream = formFile.OpenReadStream())
            {
                list = stream.Query<EventTypeDto>(startCell: "A1").ToList();
            }

            return SUCCESS(_EventTypeService.ImportEventType(list.Adapt<List<EventType>>()));
        }

        /// <summary>
        /// Event Type - أنواع الفعاليات导入模板下载
        /// </summary>
        /// <returns></returns>
        [HttpGet("importTemplate")]
        [Log(Title = "Event Type - أنواع الفعاليات模板", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [AllowAnonymous]
        public IActionResult ImportTemplateExcel()
        {
            var result = DownloadImportTemplate(new List<EventTypeDto>() { }, "EventType");
            return ExportExcel(result.Item2, result.Item1);
        }

    }
}
